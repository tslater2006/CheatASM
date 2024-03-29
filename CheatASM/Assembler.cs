﻿using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static CheatASM.CheatASMParser;

namespace CheatASM
{

    public enum AnyRefType { NUMBER, REGISTER }

    public class VariableDeclaration
    {
        public string Name;
        public bool Const;
        public string Value;
        public string Type;
    }

    public class AssemblerException : Exception
    {
        public AssemblerException(string str) : base(str) { }
    }


    public class AssemblyResult
    {
        public string GameInfo;
        public string TitleID;
        public string BuildID;
        public List<Cheat> Cheats = new List<Cheat>();


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (GameInfo != null)
            {
                sb.AppendLine("[" + GameInfo + "]");
                
            } else
            {
                sb.AppendLine("[Assembled by CheatASM]");
            }
            /* print out master code first if present */
            var masterCode = Cheats.Where(c => c.IsMaster).FirstOrDefault();
            if (masterCode != null)
            {
                sb.Append("{").Append(masterCode.Name).Append("}\r\n");

                foreach (var opcode in masterCode.VarInitCodes)
                {
                    sb.AppendLine(opcode.ToByteString());
                }

                foreach (var opcode in masterCode.Opcodes)
                {
                    sb.AppendLine(opcode.ToByteString());
                }
                sb.AppendLine();
            }

            foreach (var cheat in Cheats.Where(c => c.IsMaster == false))
            {
                sb.Append("[").Append(cheat.Name).Append("]\r\n");
                foreach(var opcode in cheat.VarInitCodes)
                {
                    sb.AppendLine(opcode.ToByteString());
                }
                foreach (var opcode in cheat.Opcodes)
                {
                    sb.AppendLine(opcode.ToByteString());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }

    public class Cheat
    {
        public bool IsMaster;
        public List<CheatOpcode> Opcodes = new List<CheatOpcode>();
        public List<CheatOpcode> VarInitCodes = new List<CheatOpcode>();
        internal string Name;

        internal Dictionary<VariableDeclaration, string> VariableReg = new Dictionary<VariableDeclaration, string>();

    }

    public class Assembler : BaseErrorListener
    {
        static string[] RegisterList = { "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "RA", "RB", "RC", "RD", "RE", "RF" };
        public Dictionary<string,VariableDeclaration> Variables = new Dictionary<string,VariableDeclaration>();
        HashSet<String> SeenRegisters = new HashSet<string>();
        string parsingContent = null;
        string errorMsg;
        int errorPos;
        static Dictionary<string, ConditionalComparisonType> CompareOperatorMap = new Dictionary<string, ConditionalComparisonType>()
        {
            {"<",ConditionalComparisonType.lt },
            {"<=",ConditionalComparisonType.le },
            {">",ConditionalComparisonType.gt },
            {">=",ConditionalComparisonType.ge },
            {"==",ConditionalComparisonType.eq },
            {"!=",ConditionalComparisonType.ne },
        };

        static Dictionary<string, ConditionalComparisonType> InvertedCompareOperatorMap = new Dictionary<string, ConditionalComparisonType>()
        {
            {"<",ConditionalComparisonType.ge },
            {"<=",ConditionalComparisonType.gt },
            {">",ConditionalComparisonType.le },
            {">=",ConditionalComparisonType.lt },
            {"==",ConditionalComparisonType.ne },
            {"!=",ConditionalComparisonType.eq },
        };


        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            base.SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e);
            errorMsg = msg;
            errorPos = charPositionInLine;
        }


        public AssemblyResult AssembleFile(string filePath)
        {
            return AssembleString(File.ReadAllText(filePath));

        }

        private string PreProcessConstants(string contents)
        {

            AntlrInputStream stream = new AntlrInputStream(contents);
            CheatASMLexer lexer = new CheatASMLexer(stream);

            CheatASMParser parser = new CheatASMParser(new CommonTokenStream(lexer));
            parser.ErrorHandler = new DefaultErrorStrategy();
            parser.TrimParseTree = true;
            parser.BuildParseTree = true;
            parser.AddErrorListener(this);
            errorMsg = null;
            errorPos = 0;
            
            ProgramContext prog = parser.program();
            Dictionary<string, string> constantValues = new();
            int lastConstantStopIndex = 0;
            foreach (var decl in prog.variableDecl())
            { 
                if (decl.@const != null)
                {
                    /* its a constant! */
                    constantValues.Add(decl.name.Text, decl.val.start.Text);
                    lastConstantStopIndex = decl.Stop.StopIndex;
                }
            }
            /* parse all of the tokens again */

            stream = new AntlrInputStream(contents);
            lexer = new CheatASMLexer(stream);

            var tokenStream = new CommonTokenStream(lexer);
            tokenStream.Fill();

            var variableTokens = tokenStream.GetTokens().Where(t => t.Type == CheatASMLexer.VARIABLE_NAME && t.StartIndex > lastConstantStopIndex).OrderByDescending(t => t.StartIndex).ToList();
            StringBuilder sb = new StringBuilder(contents);
            foreach(var variableName in variableTokens)
            {
                if (constantValues.ContainsKey(variableName.Text))
                {
                    sb.Remove(variableName.StartIndex, variableName.Text.Length);
                    sb.Insert(variableName.StartIndex, constantValues[variableName.Text]);
                }
            }

            contents = sb.ToString();
            return contents;
        }

        public AssemblyResult AssembleString(string contents)
        {

            contents = PreProcessConstants(contents);
            parsingContent = contents;
            AntlrInputStream stream = new AntlrInputStream(contents);
            CheatASMLexer lexer = new CheatASMLexer(stream);

            CheatASMParser parser = new CheatASMParser(new CommonTokenStream(lexer));
            parser.ErrorHandler = new DefaultErrorStrategy();
            parser.TrimParseTree = true;
            parser.BuildParseTree = true;
            parser.AddErrorListener(this);
            errorMsg = null;
            errorPos = 0;
            ProgramContext prog = parser.program();

            AssemblyResult result = new AssemblyResult();

            if (prog.gameInfo() != null)
            {
                result.GameInfo = prog.gameInfo().info.Text.Substring(1, prog.gameInfo().info.Text.Length - 2);
            }

            if (prog.titleID() != null)
            {
                result.TitleID = prog.titleID().title.Text.Substring(1, prog.titleID().title.Text.Length - 2);
            }

            if (prog.buildID() != null)
            {
                result.BuildID = prog.buildID().build.Text.Substring(1, prog.buildID().build.Text.Length - 2);
            }

            if (prog.variableDecl() != null && prog.variableDecl().Length > 0)
            {
                /* register all variables */
                foreach (var varCtx in prog.variableDecl())
                {
                    RegisterVariable(varCtx);
                }
            }

            if (prog.cheatEntry() != null && prog.cheatEntry().Length > 0)
            {
                /* has cheat entry */
                foreach (var entry in prog.cheatEntry())
                {
                    var cheat = new Cheat();

                    cheat.Name = entry.cheatHeader().name.Text.Replace("\"", "");
                    if (entry.cheatHeader().master != null)
                    {
                        cheat.IsMaster = true;
                    }

                    var statements = entry.statementList().statement();
                    foreach (var stmt in statements)
                    {
                        AssembleStatement(stmt, cheat);
                    }
                    result.Cheats.Add(cheat);
                }

            }
            else if (prog.statementList().statement() != null && prog.statementList().statement().Length > 0)
            {
                var cheat = new Cheat();
                cheat.Name = "Untitled";
                foreach (var stmt in prog.statementList().statement())
                {
                    AssembleStatement(stmt, cheat);
                }
                result.Cheats.Add(cheat);
            }

            if (errorMsg != null)
            {
                throw new AssemblerException(errorMsg);
            }

            return result;
        }

        public string AssembleSingleInstruction(string instr)
        {
            var result = AssembleString(instr);
            return result.Cheats[0].Opcodes[0].ToByteString();
        }

        private string ParseAnyRef(AnyRefContext anyRef, AnyRefType targetType, Cheat cheat)
        {
            var regVal = anyRef.reg;
            var numVal = anyRef.num;
            var varVal = anyRef.var;
            
            switch (targetType)
            {
                case AnyRefType.NUMBER:
                    
                    return ParseNumRef(new NumRefContext(null, 0) { num = numVal, var = varVal });
                case AnyRefType.REGISTER:
                    return ParseRegRef(new RegRefContext(null, 0) { reg = regVal, var = varVal }, cheat);
            }

            throw new AssemblerException("Failed to parse Any Ref");
        }


        private string ParseRegRef(RegRefContext regRef, Cheat cheat)
        {
            if (regRef.reg != null)
            {
                if (cheat.VariableReg.ContainsValue(regRef.reg.Text))
                {
                    throw new AssemblerException("Explicit usage of register " + regRef.reg.Text + " while it is assigned to variable: " + cheat.VariableReg.Where(s => s.Value == regRef.reg.Text).First());
                }
                return regRef.reg.Text;
            }
            else
            {
                var variableName = regRef.var.Text;
                VariableDeclaration variable = null;
                if (Variables.ContainsKey(variableName))
                {
                    variable = Variables[variableName];
                }
                if (variable != null)
                {

                    /* found a variable */
                    if (cheat.VariableReg.ContainsKey(variable))
                    {
                        return cheat.VariableReg[variable];
                    }
                    else
                    {
                        /* variable isn't in the map for this cheat yet... we need to add it */

                        /* find a register that isn't in use yet... */
                        var availableReg = RegisterList.Where(r => cheat.VariableReg.ContainsValue(r) == false).LastOrDefault();
                        if (String.IsNullOrEmpty(availableReg))
                        {
                            throw new AssemblerException("Unable to find an unused variable for variable: " + variable.Name + ".");
                        }
                        else
                        {
                            /* add this variable to the map */
                            cheat.VariableReg.Add(variable, availableReg);

                            /* create the variable init opcode */
                            var loadReg = new Opcode4LoadRegWithStatic();
                            loadReg.RegisterIndex = Convert.ToUInt32(availableReg.Substring(1), 16);
                            loadReg.Value = Convert.ToUInt64(ConvertVariableValue(variable), 16);

                            cheat.VarInitCodes.Add(loadReg);
                            return availableReg;
                        }

                    }
                }
                else
                {
                    // TODO: Support line numbers for errors 
                    throw new AssemblerException("Variable: " + variableName + " not defined.");
                }
            }
        }
 
        private string ConvertVariableValue(VariableDeclaration variable)
        {
            bool isValueHex = variable.Value.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase);
            byte[] bytes = new byte[0];
            switch(variable.Type)
            {
                case ".u8":
                    var byteVal = Convert.ToByte(variable.Value, (isValueHex ? 16 : 10));
                    bytes = BitConverter.GetBytes(byteVal);
                    break;
                case ".s8":
                    var sbyteVal = Convert.ToSByte(variable.Value, (isValueHex ? 16 : 10));
                    bytes = BitConverter.GetBytes(sbyteVal);
                    break;
                case ".u16":
                    var shortVal = Convert.ToUInt16(variable.Value, (isValueHex ? 16 : 10));
                    bytes = BitConverter.GetBytes(shortVal);
                    break;
                case ".s16":
                    var sshortVal = Convert.ToInt16(variable.Value, (isValueHex ? 16 : 10));
                    bytes = BitConverter.GetBytes(sshortVal);
                    break;
                case ".u32":
                    var intVal = Convert.ToUInt32(variable.Value, (isValueHex ? 16 : 10));
                    bytes = BitConverter.GetBytes(intVal);
                    break;
                case ".s32":
                    var sintVal = Convert.ToInt32(variable.Value, (isValueHex ? 16 : 10));
                    bytes = BitConverter.GetBytes(sintVal);
                    break;
                case ".u64":
                    var longVal = Convert.ToUInt64(variable.Value, (isValueHex ? 16 : 10));
                    bytes = BitConverter.GetBytes(longVal);
                    break;
                case ".s64":
                    var slongVal = Convert.ToInt64(variable.Value, (isValueHex ? 16 : 10));
                    bytes = BitConverter.GetBytes(slongVal);
                    break;
                case ".f32":
                    float sVal;
                    if (isValueHex)
                    {
                        sVal = Convert.ToSingle(Convert.ToUInt32(variable.Value, 16));
                    }
                    else
                    {
                        sVal = Convert.ToSingle(variable.Value);
                    }
                    bytes = BitConverter.GetBytes(sVal);
                    break;
                case ".f64":
                    double doubleVal;
                    if (isValueHex)
                    {
                        doubleVal = Convert.ToDouble(Convert.ToUInt64(variable.Value, 16));
                    }
                    else
                    {
                        doubleVal = Convert.ToDouble(variable.Value);
                    }
                    bytes = BitConverter.GetBytes(doubleVal);
                    break;
            }
           
            bytes.Reverse();
            return "0x" + BitConverter.ToString(bytes.Reverse().ToArray()).Replace("-", "");
        }

        private string ParseNumRef(NumRefContext numRef)
        {
            if (numRef.num != null)
            {
                return numRef.num.Text;
            }
            else
            {
                var variableName = numRef.var.Text;
                VariableDeclaration variable = null;
                if (Variables.ContainsKey(variableName))
                {
                    variable = Variables[variableName];
                }
                if (variable != null)
                {
                    /* found a variable */
                    if (variable.Const != true)
                    {
                        /* variable is a constant, but used as a register... bad */
                        throw new AssemblerException("Variable '" + variableName + "' is not declared constant, but used like a constant number.");
                    }
                    else
                    {
                        return ConvertVariableValue(variable);
                    }
                }
                else
                {
                    // TODO: Support line numbers for errors 
                    throw new AssemblerException("Variable '" + variableName + "' not defined before line XYZ");
                }
            }
        }

        public AnyRefType GetAnyRefType(AnyRefContext ctx)
        {
            if (ctx.num != null)
            {
                return AnyRefType.NUMBER;
            } else if (ctx.reg != null)
            {
                return AnyRefType.REGISTER;
            } else if (ctx.var != null)
            {
                /* we have a variable... */
                VariableDeclaration variable = null;
                if (Variables.ContainsKey(ctx.var.Text))
                {
                    variable = Variables[ctx.var.Text];
                }
                if (variable == null)
                {
                    throw new AssemblerException("Use of undefined variable: " + ctx.var.Text);
                }
                if (variable.Const)
                {
                    return AnyRefType.NUMBER;
                } else
                {
                    return AnyRefType.REGISTER;
                }
            } else
            {
                throw new AssemblerException("Unable to determine type of AnyRef: " + ctx.ToString());
            }
        }

        public void RegisterVariable(VariableDeclContext varCtx)
        {

                VariableDeclaration decl = new VariableDeclaration();

                decl.Name = varCtx.name.Text;
                decl.Type = varCtx.type.Text;
                decl.Const = (varCtx.@const != null);
                decl.Value = varCtx.val.GetText();

                if (Variables.ContainsKey(decl.Name))
                {
                    throw new AssemblerException("Variable '" + decl.Name + "' already defined.");
                }

                Variables.Add(decl.Name, decl);
        }

        private void AssembleOpCode6(OpCode6Context opCtx, Cheat cheat)
        {
            /* should be an opcode 6... */
            Opcode6StoreStaticToAddress opTyped = new Opcode6StoreStaticToAddress();

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            opTyped.RegisterIndex = Convert.ToUInt16(ParseRegRef(opCtx.@base, cheat).Substring(1), 16);

            if (opCtx.increment != null)
            {
                opTyped.IncrementFlag = true;
            }
            if (opCtx.regOffset != null)
            {
                opTyped.OffsetEnableFlag = true;
                opTyped.OffsetRegister = Convert.ToUInt16(ParseRegRef(opCtx.regOffset, cheat).Substring(1), 16);
            }

            opTyped.Value = Convert.ToUInt64(ParseNumRef(opCtx.value), 16);
            CheckValueFitsBitWidth(opTyped.BitWidth, opTyped.Value);
            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCodeA(OpCodeAContext opCtx, Cheat cheat)
        {
            OpcodeAStoreRegToAddress opTyped = new OpcodeAStoreRegToAddress();

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            if (opCtx.@base != null)
            {
                opTyped.AddressRegister = Convert.ToUInt32(ParseRegRef(opCtx.@base, cheat).Substring(1), 16);
            }
            opTyped.SourceRegister = Convert.ToUInt32(ParseRegRef(opCtx.regValue, cheat).Substring(1), 16);
            if (opCtx.increment != null)
            {
                opTyped.IncrementFlag = true;
            }
            opTyped.OffsetType = 0;

            if (opCtx.memType != null)
            {
                opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text);
                /* has to be OffsetType 3,4,5 */
                bool hasReg = opCtx.regOffset != null;
                bool hasVal = opCtx.numOffset != null;

                if (hasReg && hasVal)
                {
                    /* type 5 */
                    opTyped.OffsetType = 5;
                    opTyped.OffsetRegister = Convert.ToUInt16(ParseRegRef(opCtx.regOffset, cheat).Substring(1), 16);
                    opTyped.RelativeAddress = Convert.ToUInt64(ParseNumRef(opCtx.numOffset), 16);
                }
                else if (hasReg)
                {
                    /* type 3 */
                    opTyped.OffsetType = 3;
                    opTyped.OffsetRegister = Convert.ToUInt16(ParseRegRef(opCtx.regOffset, cheat).Substring(1), 16);
                }
                else if (hasVal)
                {
                    /* type 4 */
                    opTyped.OffsetType = 4;
                    opTyped.RelativeAddress = Convert.ToUInt64(ParseNumRef(opCtx.numOffset), 16);
                }
            }
            else
            {
                /* has to be OffsetType 1,2 */
                if (opCtx.regOffset != null)
                {
                    opTyped.OffsetType = 1;
                    opTyped.OffsetRegister = Convert.ToUInt16(ParseRegRef(opCtx.regOffset, cheat).Substring(1), 16);
                } else if (opCtx.numOffset != null)
                {
                    opTyped.OffsetType = 2;
                    opTyped.RelativeAddress = Convert.ToUInt64(ParseNumRef(opCtx.numOffset), 16);
                }
            }
            cheat.Opcodes.Add(opTyped);
        }


        private void AssembleOpCodeC1C2(OpCodeC1C2Context opCtx, Cheat cheat)
        {
            string func = opCtx.func.Text.ToLower();
            string type = opCtx.type.Text.ToLower();

            if (opCtx.index != null || opCtx.reg != null)
            {
                OpcodeC1SaveRestoreReg op = new OpcodeC1SaveRestoreReg();
                switch (func)
                {
                    case "load":
                        op.OperandType = 0;
                        break;
                    case "save":
                        op.OperandType = 1;
                        break;
                    case "clear":
                        if (type.Equals("saved"))
                        {
                            op.OperandType = 2;
                        }
                        else if (type.Equals("reg"))
                        {
                            op.OperandType = 3;
                        }
                        break;
                }
                /* Op Code C1 */
                switch (op.OperandType) {
                    case 0:
                        /* Restore */
                        op.SourceIndex = Convert.ToUInt32(ParseNumRef(opCtx.index), 16);
                        op.DestinationIndex = Convert.ToUInt32(ParseRegRef(opCtx.reg, cheat).Substring(1), 16);
                        break;
                    case 1:
                        /* Save */
                        op.DestinationIndex = Convert.ToUInt32(ParseNumRef(opCtx.index), 16);
                        op.SourceIndex = Convert.ToUInt32(ParseRegRef(opCtx.reg, cheat).Substring(1), 16);
                        break;
                    case 2:
                        /* Clear saved */
                        op.DestinationIndex = Convert.ToUInt32(ParseNumRef(opCtx.index), 16);
                        break;
                    case 3:
                        /* clear register */
                        op.DestinationIndex = Convert.ToUInt32(ParseRegRef(opCtx.reg, cheat).Substring(1), 16);
                        break;
                }
                cheat.Opcodes.Add(op);
            } else if (opCtx.regs != null || opCtx.indexes != null)
            {
                /* Op Code C2 */
                OpcodeC2SaveRestoreRegMask op = new();
                switch (func)
                {
                    case "load":
                        op.OperandType = 0;
                        break;
                    case "save":
                        op.OperandType = 1;
                        break;
                    case "clear":
                        if (type.Equals("saved"))
                        {
                            op.OperandType = 2;
                        }
                        else if (type.Equals("regs"))
                        {
                            op.OperandType = 3;
                        }
                        break;
                }

                bool[] maskBits = new bool[16];

                if (opCtx.indexes != null)
                {
                    foreach (var numRefCtx in opCtx.indexes.GetRuleContexts<NumRefContext>())
                    {
                        var indexNum = Convert.ToUInt32(ParseNumRef(numRefCtx), 16);
                        maskBits[indexNum] = true;
                    }
                } else if (opCtx.regs != null)
                {
                    foreach(var regRefCtx in opCtx.regs.GetRuleContexts<RegRefContext>())
                    {
                        var regNum = Convert.ToUInt32(ParseRegRef(regRefCtx, cheat).Substring(1), 16);
                        maskBits[regNum] = true;
                    }
                }

                Array.Copy(maskBits, op.RegMask, 16);
                cheat.Opcodes.Add(op);
            }
        }

        private void AssembleOpCodeC3(OpCodeC3Context opCtx, Cheat cheat)
        {
            OpcodeC3ReadWriteStaticReg op = new();
            op.WriteMode = opCtx.func.Text.ToLower().Equals("save");

            op.StaticRegIndex = Convert.ToUInt32(opCtx.sreg.Text.Substring(2), 16);
            op.RegIndex = Convert.ToUInt32(ParseRegRef(opCtx.reg, cheat).Substring(1));

            cheat.Opcodes.Add(op);
        }

        private void AssembleOpCodeFF0(OpCodeFF0Context opCtx, Cheat cheat)
        {
            cheat.Opcodes.Add(new OpcodeFF0PauseProcess());
        }

        private void AssembleOpCodeFF1(OpCodeFF1Context opCtx, Cheat cheat)
        {
            cheat.Opcodes.Add(new OpcodeFF1ResumeProcess());
        }

        private void AssembleOpCodeFFF(OpCodeFFFContext opCtx, Cheat cheat)
        {
            OpcodeFFFDebugLog op = new();
            op.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            op.LogId = Convert.ToUInt32(opCtx.id.Text, 16);

            if (opCtx.memType != null)
            {
                op.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text);
                if (opCtx.offset != null)
                {
                    var refType = GetAnyRefType(opCtx.offset);
                    switch(refType)
                    {
                        case AnyRefType.NUMBER:
                            op.OperandType = 0;
                            op.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.offset, AnyRefType.NUMBER, cheat), 16);
                            break;
                        case AnyRefType.REGISTER:
                            op.OperandType = 1;
                            op.OffsetRegister = Convert.ToUInt32(ParseAnyRef(opCtx.offset, AnyRefType.REGISTER, cheat).Substring(1), 16);
                            break;
                    }
                }
            } else if (opCtx.addrReg != null)
            {
                op.AddressRegister = Convert.ToUInt32(ParseRegRef(opCtx.addrReg, cheat).Substring(1), 16);
                if (opCtx.offset == null)
                {
                    op.OperandType = 2;
                    op.RelativeAddress = 0;
                }
                else
                {
                    var refType = GetAnyRefType(opCtx.offset);
                    switch (refType)
                    {
                        case AnyRefType.NUMBER:
                            op.OperandType = 2;
                            op.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.offset, AnyRefType.NUMBER, cheat), 16);
                            break;
                        case AnyRefType.REGISTER:
                            op.OperandType = 3;
                            op.OffsetRegister = Convert.ToUInt32(ParseAnyRef(opCtx.offset, AnyRefType.REGISTER, cheat).Substring(1), 16);
                            break;
                    }
                }
            } else
            {
                op.OperandType = 4;
                op.ValueRegister = Convert.ToUInt32(ParseRegRef(opCtx.value, cheat).Substring(1), 16);
            }
            cheat.Opcodes.Add(op);
        }

        private void AssembleOpCodeC0(OpCodeC0Context opCtx, Cheat cheat)
        {
            /*opCodeC0: (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA LSQUARE (memType=MEM_TYPE) PLUS_SIGN (offset=anyRef) RSQUARE
		            | (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA LSQUARE (addrReg=regRef) PLUS_SIGN (offset=anyRef) RSQUARE
            		| (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA (value=anyRef);*/
            OpcodeC0RegisterConditional opTyped = new OpcodeC0RegisterConditional();

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            opTyped.Condition = Enum.Parse<ConditionalComparisonType>(opCtx.cond.Text, true);
            opTyped.SourceRegister = Convert.ToUInt16(ParseRegRef(opCtx.source, cheat).Substring(1), 16);
            if (opCtx.memType != null)
            {
                opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);

                /* operand type is either 0 or 1... */
                if (opCtx.offset == null || GetAnyRefType(opCtx.offset) == AnyRefType.NUMBER)
                {
                    if (opCtx.offset != null)
                    {
                        opTyped.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.offset, AnyRefType.NUMBER, cheat), 16);
                    } else
                    {
                        opTyped.RelativeAddress = 0;
                    }
                    opTyped.OperandType = 0;
                }
                else if (GetAnyRefType(opCtx.offset) == AnyRefType.REGISTER)
                {
                    opTyped.OffsetRegister = Convert.ToUInt16(ParseAnyRef(opCtx.offset,AnyRefType.REGISTER, cheat).Substring(1), 16);
                    opTyped.OperandType = 1;
                }
            }
            else if (opCtx.addrReg != null)
            {
                opTyped.AddressRegister = Convert.ToUInt32(ParseRegRef(opCtx.addrReg, cheat).Substring(1),16);
                /* operand type is either 2 or 3 */
                if (opCtx.offset == null || GetAnyRefType(opCtx.offset) == AnyRefType.NUMBER)
                {
                    if (opCtx.offset != null)
                    {
                        opTyped.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.offset, AnyRefType.NUMBER, cheat), 16);
                    } else
                    {
                        opTyped.RelativeAddress = 0;
                    }
                    opTyped.OperandType = 2;
                }
                else if (GetAnyRefType(opCtx.offset) == AnyRefType.REGISTER)
                {
                    opTyped.OffsetRegister = Convert.ToUInt16(ParseAnyRef(opCtx.offset, AnyRefType.REGISTER, cheat).Substring(1), 16);
                    opTyped.OperandType = 3;
                }
            }
            else if (opCtx.value != null)
            {
                /* operand type is either 2 or 3 */
                if (GetAnyRefType(opCtx.value) == AnyRefType.NUMBER)
                {
                    opTyped.Value = Convert.ToUInt64(ParseAnyRef(opCtx.value, AnyRefType.NUMBER, cheat), 16);
                    CheckValueFitsBitWidth(opTyped.BitWidth, opTyped.Value);
                    opTyped.OperandType = 4;
                }
                else if (GetAnyRefType(opCtx.value) == AnyRefType.REGISTER)
                {
                    opTyped.OtherRegister = Convert.ToUInt16(ParseAnyRef(opCtx.value, AnyRefType.REGISTER, cheat).Substring(1), 16);
                    opTyped.OperandType = 5;
                }
            }
            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode9(OpCode9Context opCtx, Cheat cheat)
        {
            Opcode9Arithmetic opTyped = new Opcode9Arithmetic();
            

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            opTyped.RegisterDest = Convert.ToUInt16(ParseRegRef(opCtx.dest, cheat).Substring(1), 16);
            opTyped.MathType = Enum.Parse<RegisterArithmeticType>(opCtx.func.Text, true);

            opTyped.RegisterLeft = Convert.ToUInt16(ParseRegRef(opCtx.leftReg, cheat).Substring(1), 16);

            if (opCtx.right != null)
            {
                if (GetAnyRefType(opCtx.right) == AnyRefType.NUMBER)
                {
                    opTyped.RightHandRegister = false;
                    opTyped.Value = opTyped.Value = Convert.ToUInt64(ParseAnyRef(opCtx.right, AnyRefType.NUMBER, cheat), 16);
                    CheckValueFitsBitWidth(opTyped.BitWidth, opTyped.Value);
                }
                else if (GetAnyRefType(opCtx.right) == AnyRefType.REGISTER)
                {
                    opTyped.RightHandRegister = true;
                    opTyped.RegisterRight = Convert.ToUInt16(ParseAnyRef(opCtx.right, AnyRefType.REGISTER, cheat).Substring(1), 16);
                }
            } else
            {
                opTyped.NoRightHandOperand = true;
            }

            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode8(OpCode8Context opCtx, Cheat cheat)
        {
            Opcode8KeypressConditional opTyped = new Opcode8KeypressConditional();

            try
            {
                opTyped.Mask = Enum.Parse<KeyMask>(opCtx.key.Text);
            }
            catch (Exception ex)
            {
                throw new AssemblerException($"On line: {opCtx.Start.Line} instruction: \"{GetCurrentInstructionText(opCtx)}\", The key \"{opCtx.key.Text}\" is not valid.");
            }
            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode7(OpCode7Context opCtx, Cheat cheat)
        {
            Opcode7LegacyArithmetic opTyped = new Opcode7LegacyArithmetic();
            

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            opTyped.RegisterIndex = Convert.ToUInt16(ParseRegRef(opCtx.register, cheat).Substring(1), 16);
            opTyped.MathType = Enum.Parse<RegisterArithmeticType>(opCtx.func.Text, true);
            opTyped.Value = Convert.ToUInt32(ParseNumRef(opCtx.value), 16);
            CheckValueFitsBitWidth(opTyped.BitWidth, opTyped.Value);
            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode5(OpCode5Context opCtx, Cheat cheat)
        {
            Opcode5LoadRegWithMem opTyped = new Opcode5LoadRegWithMem();

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);

            opTyped.RegisterIndex = Convert.ToUInt16(ParseRegRef(opCtx.register, cheat).Substring(1), 16);
            if (opCtx.numOffset != null)
            {
                opTyped.Immediate = Convert.ToUInt64(ParseNumRef(opCtx.numOffset), 16);
            } else
            {
                opTyped.Immediate = 0;
            }

            if (opCtx.memType != null)
            {
                opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);
            }
            else
            {
                opTyped.UseReg = true;
                if (!ParseRegRef(opCtx.register,cheat).Equals(ParseRegRef(opCtx.baseRegister,cheat)))
                {
                    throw new AssemblerException($"On line: {opCtx.Start.Line} instruction: \"{GetCurrentInstructionText(opCtx)}\", Both the destination and source registers must be the same");
                }
            }
            cheat.Opcodes.Add(opTyped);
        }
        private string GetCurrentInstructionText(ParserRuleContext ctx)
        {
            return parsingContent.Substring(ctx.Start.StartIndex, ctx.stop.StopIndex - ctx.start.StartIndex + 1);
        }
        private void AssembleOpCode4(OpCode4Context opCtx, Cheat cheat)
        {
            Opcode4LoadRegWithStatic opTyped = new Opcode4LoadRegWithStatic();


            opTyped.RegisterIndex = Convert.ToUInt16(ParseRegRef(opCtx.register, cheat).Substring(1), 16);
            opTyped.Value = Convert.ToUInt64(ParseNumRef(opCtx.value), 16);

            /* not strictly needed by Atmosphere, as the bitwidth isn't encoded... but if they provided it we should check the literal fits */
            if (opCtx.bitWidth != null)
            {
                var bitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                CheckValueFitsBitWidth(bitWidth, opTyped.Value);
            }

            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode3(OpCode3Context opCtx, Cheat cheat)
        {
            Opcode3Loop opTyped = new Opcode3Loop();

            if (opCtx.endloop != null)
            {
                opTyped.IsEnd = true;
                opTyped.RegisterIndex = Convert.ToUInt16(ParseRegRef(opCtx.register, cheat).Substring(1), 16);
            }
            else
            {
                opTyped.IsEnd = false;
                opTyped.RegisterIndex = Convert.ToUInt16(ParseRegRef(opCtx.register, cheat).Substring(1), 16);
                opTyped.Count = Convert.ToUInt32(ParseNumRef(opCtx.value), 16);
            }
            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode2(OpCode2Context opCtx, Cheat cheat)
        {
            var endCond = new Opcode2EndConditional();
            endCond.IsElse = (opCtx.ELSE() != null);
            cheat.Opcodes.Add(endCond);
        }

        private void AssembleOpCode1(OpCode1Context opCtx, Cheat cheat)
        {
            Opcode1Conditional opTyped = new Opcode1Conditional();

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);

            opTyped.Condition = Enum.Parse<ConditionalComparisonType>(opCtx.cond.Text, true);
            opTyped.Immediate = Convert.ToUInt64(ParseNumRef(opCtx.offset), 16);
            opTyped.Value = Convert.ToUInt64(ParseNumRef(opCtx.value), 16);

            CheckValueFitsBitWidth(opTyped.BitWidth, opTyped.Value);

            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode0(OpCode0Context opCtx, Cheat cheat)
        {
            Opcode0StoreStaticToMemory opTyped = new();

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);

            opTyped.OffsetRegister = Convert.ToUInt16(ParseRegRef(opCtx.regOffset, cheat).Substring(1), 16);

            if (opCtx.numOffset != null)
            {
                opTyped.RelativeOffset = Convert.ToUInt64(ParseNumRef(opCtx.numOffset), 16);
            }

            opTyped.Value = Convert.ToUInt64(ParseNumRef(opCtx.value), 16);
            CheckValueFitsBitWidth(opTyped.BitWidth, opTyped.Value);
            cheat.Opcodes.Add(opTyped);
        }
        private static void CheckValueFitsBitWidth(BitWidthType width, ulong value)
        {
            bool valueFits = true;
            ulong valueMax = 0;
            switch (width)
            {
                case BitWidthType.b:
                    valueMax = byte.MaxValue;
                    break;
                case BitWidthType.w:
                    valueMax = ushort.MaxValue;
                    break;
                case BitWidthType.d:
                    valueMax = uint.MaxValue;
                    break;
                case BitWidthType.q:
                    valueMax = ulong.MaxValue;
                    break;
            }

            if (value > valueMax)
            {
                throw new AssemblerException($"Instruction has bit width: '{width}' but value 0x{value:X} exceeds the maximum of 0x{valueMax:X}");
            }
        }

        public void AssembleStatement(StatementContext stmt,Cheat cheat)
        {
            switch(stmt.GetRuleContext<ParserRuleContext>(0))
            {
                case OpCode0Context op:
                    AssembleOpCode0(op, cheat);
                    break;
                case OpCode1Context op:
                    AssembleOpCode1(op, cheat);
                    break;
                case OpCode2Context op:
                    AssembleOpCode2(op, cheat);
                    break;
                case OpCode3Context op:
                    AssembleOpCode3(op, cheat);
                    break;
                case OpCode4Context op:
                    AssembleOpCode4(op, cheat);
                    break;
                case OpCode5Context op:
                    AssembleOpCode5(op, cheat);
                    break;
                case OpCode6Context op:
                    AssembleOpCode6(op, cheat);
                    break;
                case OpCode7Context op:
                    AssembleOpCode7(op, cheat);
                    break;
                case OpCode8Context op:
                    AssembleOpCode8(op, cheat);
                    break;
                case OpCode9Context op:
                    AssembleOpCode9(op, cheat);
                    break;
                case OpCodeAContext op:
                    AssembleOpCodeA(op, cheat);
                    break;
                case OpCodeC0Context op:
                    AssembleOpCodeC0(op, cheat);
                    break;
                case OpCodeC1C2Context op:
                    AssembleOpCodeC1C2(op, cheat);
                    break;
                case OpCodeC3Context op:
                    AssembleOpCodeC3(op, cheat);
                    break;
                case OpCodeFF0Context op:
                    AssembleOpCodeFF0(op, cheat);
                    break;
                case OpCodeFF1Context op:
                    AssembleOpCodeFF1(op, cheat);
                    break;
                case OpCodeFFFContext op:
                    AssembleOpCodeFFF(op, cheat);
                    break;
                case IfStatementContext op:
                    AssembleIfStatement(op, cheat);
                    break;
            }
        }

        private void AssembleIfStatement(IfStatementContext opCtx, Cheat cheat)
        {
            var compareBitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            bool containsElse = opCtx.IF_ELSE() != null;

            IfStatementType ifType;

            if (opCtx.leftMemType != null)
            {
                ifType = IfStatementType.CODE1;
            } else
            {
                if (opCtx.memType != null)
                {
                    ifType = IfStatementType.CODEC0MEM;
                } else
                {
                    if (opCtx.addrReg != null)
                    {
                        ifType = IfStatementType.CODEC0REG;
                    } else
                    {
                        ifType = IfStatementType.CODEC0VAL;
                    }
                }
            }

            CheatOpcode conditionalOp = null;
            CheatOpcode elseCondition = new Opcode2EndConditional() { IsElse = true };
            if (ifType == IfStatementType.CODE1)
            {
                conditionalOp = new Opcode1Conditional();
                var typed = (Opcode1Conditional)conditionalOp;
                typed.BitWidth = compareBitWidth;
                typed.MemType = Enum.Parse<MemoryAccessType>(opCtx.leftMemType.Text);
                typed.Immediate = Convert.ToUInt64(ParseNumRef(opCtx.numOffset), 16);
                typed.Value = Convert.ToUInt64(ParseNumRef(opCtx.value), 16);
                typed.Condition = CompareOperatorMap[opCtx.CONDITIONAL_SYMBOL().ToString()];
            }
            else
            {
                /* we'll be using C0 for the conditional check */
                conditionalOp = new OpcodeC0RegisterConditional();
                var typed = (OpcodeC0RegisterConditional)conditionalOp;
                typed.BitWidth = compareBitWidth;
                typed.Condition = CompareOperatorMap[opCtx.CONDITIONAL_SYMBOL().ToString()];
                typed.SourceRegister = Convert.ToUInt16(ParseRegRef(opCtx.reg, cheat).Substring(1), 16);
                if (ifType == IfStatementType.CODEC0VAL)
                {
                    /* operand type is either 2 or 3 */
                    if (GetAnyRefType(opCtx.rightAny) == AnyRefType.NUMBER)
                    {
                        typed.Value = Convert.ToUInt64(ParseAnyRef(opCtx.rightAny, AnyRefType.NUMBER, cheat), 16);
                        CheckValueFitsBitWidth(typed.BitWidth, typed.Value);
                        typed.OperandType = 4;
                    }
                    else if (GetAnyRefType(opCtx.rightAny) == AnyRefType.REGISTER)
                    {
                        typed.OtherRegister = Convert.ToUInt16(ParseAnyRef(opCtx.rightAny, AnyRefType.REGISTER, cheat).Substring(1), 16);
                        typed.OperandType = 5;
                    }
                }
                else if (ifType == IfStatementType.CODEC0REG)
                {
                    typed.AddressRegister = Convert.ToUInt32(ParseRegRef(opCtx.addrReg, cheat).Substring(1), 16);
                    /* operand type is either 2 or 3 */
                    if (opCtx.anyOffset == null || GetAnyRefType(opCtx.anyOffset) == AnyRefType.NUMBER)
                    {
                        if (opCtx.anyOffset != null)
                        {
                            typed.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.anyOffset, AnyRefType.NUMBER, cheat), 16);
                        }
                        else
                        {
                            typed.RelativeAddress = 0;
                        }
                        typed.OperandType = 2;
                    }
                    else if (GetAnyRefType(opCtx.anyOffset) == AnyRefType.REGISTER)
                    {
                        typed.OffsetRegister = Convert.ToUInt16(ParseAnyRef(opCtx.anyOffset, AnyRefType.REGISTER, cheat).Substring(1), 16);
                        typed.OperandType = 3;
                    }
                } else if (ifType == IfStatementType.CODEC0MEM)
                {
                    typed.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);

                    /* operand type is either 0 or 1... */
                    if (opCtx.anyOffset == null || GetAnyRefType(opCtx.anyOffset) == AnyRefType.NUMBER)
                    {
                        if (opCtx.anyOffset != null)
                        {
                            typed.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.anyOffset, AnyRefType.NUMBER, cheat), 16);
                        }
                        else
                        {
                            typed.RelativeAddress = 0;
                        }
                        typed.OperandType = 0;
                    }
                    else if (GetAnyRefType(opCtx.anyOffset) == AnyRefType.REGISTER)
                    {
                        typed.OffsetRegister = Convert.ToUInt16(ParseAnyRef(opCtx.anyOffset, AnyRefType.REGISTER, cheat).Substring(1), 16);
                        typed.OperandType = 1;
                    }
                }
            }

            cheat.Opcodes.Add(conditionalOp);

            foreach(var stmt in opCtx.stmtList.statement())
            {
                AssembleStatement(stmt, cheat);
            }

            if (containsElse)
            {
                cheat.Opcodes.Add(elseCondition);
                foreach (var stmt in opCtx.elseStmtList.statement())
                {
                    AssembleStatement(stmt, cheat);
                }
            }

            cheat.Opcodes.Add(new Opcode2EndConditional());

        }
    }




    enum IfStatementType
    {
        CODE1, CODEC0MEM, CODEC0REG, CODEC0VAL
    }
}
