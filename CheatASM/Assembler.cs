using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
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
        public List<VariableDeclaration> Variables = new List<VariableDeclaration>();
        HashSet<String> SeenRegisters = new HashSet<string>();

        string errorMsg;
        int errorPos;
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

        public AssemblyResult AssembleString(string contents)
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

                    var statements = entry.statement();
                    foreach (var stmt in statements)
                    {
                        AssembleStatement(stmt, cheat);
                    }
                    result.Cheats.Add(cheat);
                }

            }
            else if (prog.statement() != null && prog.statement().Length > 0)
            {
                var cheat = new Cheat();
                cheat.Name = "Untitled";
                foreach (var stmt in prog.statement())
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

        public AssemblyResult AssembleInstructions(string[] instructions)
        {
            return AssembleString(String.Join(Environment.NewLine, instructions));
        }

        public AssemblyResult AssembleInstruction(string instr)
        {
            return AssembleString(instr);
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
                var variable = Variables.Find(v => v.Name == variableName);
                if (variable != null)
                {
                    if (variable.Const == true)
                    {
                        /* variable is a constant, but used as a register... bad */
                        throw new AssemblerException("Variable: " + variableName + " is declared as a constant, but used like a register.");
                    }
                    else
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
                                var loadReg = new LoadRegisterStaticOpcode();
                                loadReg.RegisterIndex = Convert.ToUInt32(availableReg.Substring(1), 16);
                                loadReg.Value = Convert.ToUInt64(ConvertVariableValue(variable),16);

                                cheat.VarInitCodes.Add(loadReg);
                                return availableReg;
                            }

                        }

                    }
                }
                else
                {
                    // TODO: Support line numbers for errors 
                    throw new AssemblerException("Variable: " + variableName + " not defined before line XYZ");
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
                var variable = Variables.Find(v => v.Name == variableName);
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
                var variable = Variables.Find(v => v.Name == ctx.var.Text);
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

                if (Variables.Find(v => v.Name == decl.Name) != null)
                {
                    throw new AssemblerException("Variable '" + decl.Name + "' already defined.");
                }

                Variables.Add(decl);
        }


        private void AssembleMovInstr(MovInstrContext opCtx, Cheat cheat)
        {
            /* deterimine if this should be opcode 0... */

            /* mov.d [HEAP + reg + num], num */
            if (opCtx.memType != null && GetAnyRefType(opCtx.source) == AnyRefType.NUMBER)
            {
                /* should be an opcode 0... */
                StoreStaticOpcode opTyped = new StoreStaticOpcode();

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);
                opTyped.OffsetRegister = Convert.ToUInt16(ParseAnyRef(opCtx.offset, AnyRefType.REGISTER, cheat).Substring(1), 16);
                opTyped.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.immediate, AnyRefType.NUMBER, cheat), 16);
                opTyped.Value = Convert.ToUInt64(ParseAnyRef(opCtx.source, AnyRefType.NUMBER, cheat), 16);

                cheat.Opcodes.Add(opTyped);
                return;
            }

            /* mov.d [reg], number */
            /* mov.d [reg + reg], number */
            if (opCtx.@base != null && GetAnyRefType(opCtx.source) == AnyRefType.NUMBER)
            {
                /* should be an opcode 6... */
                StoreStaticToAddressOpcode opTyped = new StoreStaticToAddressOpcode();

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                opTyped.RegisterIndex = Convert.ToUInt16(ParseRegRef(opCtx.@base, cheat).Substring(1), 16);

                if (opCtx.increment != null)
                {
                    opTyped.IncrementFlag = true;
                }
                if (opCtx.offset != null)
                {
                    opTyped.OffsetEnableFlag = true;
                    opTyped.OffsetRegister = Convert.ToUInt16(ParseAnyRef(opCtx.offset, AnyRefType.REGISTER, cheat).Substring(1), 16);
                }

                opTyped.Value = Convert.ToUInt64(ParseAnyRef(opCtx.source, AnyRefType.NUMBER, cheat), 16);
                cheat.Opcodes.Add(opTyped);
                return;
            }

            /* else it should be an opcode A */
            /* 
             *  mov.d [reg], reg
                mov.d [reg + reg], reg
                mov.d [reg + num], reg
                mov.d [HEAP + reg], reg
                mov.d [HEAP + num], reg
                mov.d [HEAP + reg + num], reg*/
            if (GetAnyRefType(opCtx.source) == AnyRefType.REGISTER)
            {
                StoreRegisterToAddressOpcode opTyped = new StoreRegisterToAddressOpcode();

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                if (opCtx.@base != null)
                {
                    opTyped.AddressRegister = Convert.ToUInt32(ParseRegRef(opCtx.@base, cheat).Substring(1), 16);
                }
                opTyped.SourceRegister = Convert.ToUInt32(ParseAnyRef(opCtx.source, AnyRefType.REGISTER, cheat).Substring(1), 16);
                if (opCtx.increment != null)
                {
                    opTyped.IncrementFlag = true;
                }
                opTyped.OffsetType = 0;

                if (opCtx.memType != null)
                {
                    opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text);
                    /* has to be OffsetType 3,4,5 */
                    bool hasReg = opCtx.@base != null;
                    bool hasVal = opCtx.immediate != null;

                    if (hasReg && hasVal)
                    {
                        /* type 5 */
                        opTyped.OffsetType = 5;
                        opTyped.OffsetRegister = Convert.ToUInt16(ParseRegRef(opCtx.@base, cheat).Substring(1), 16);
                        opTyped.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.immediate, AnyRefType.NUMBER, cheat), 16);
                    }
                    else if (hasReg)
                    {
                        /* type 3 */
                        opTyped.OffsetType = 3;
                        opTyped.OffsetRegister = Convert.ToUInt16(ParseRegRef(opCtx.@base, cheat).Substring(1), 16);
                    }
                    else if (hasVal)
                    {
                        /* type 4 */
                        opTyped.OffsetType = 4;
                        opTyped.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.immediate, AnyRefType.NUMBER, cheat), 16);
                    }
                }
                else
                {
                    /* has to be OffsetType 1,2 */
                    if (opCtx.offset != null)
                    {
                        if (GetAnyRefType(opCtx.offset) == AnyRefType.NUMBER)
                        {
                            opTyped.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.offset, AnyRefType.NUMBER, cheat), 16);
                            opTyped.OffsetType = 2;
                        }
                        else if (GetAnyRefType(opCtx.offset) == AnyRefType.REGISTER)
                        {
                            opTyped.OffsetRegister = Convert.ToUInt16(ParseAnyRef(opCtx.offset, AnyRefType.REGISTER, cheat).Substring(1), 16);
                            opTyped.OffsetType = 1;
                        }
                    }
                }
                cheat.Opcodes.Add(opTyped);
                return;
            }
            throw new AssemblerException("Unable to assemble mov instruction: " + opCtx.ToStringTree());

        }

        private void AssembleOpCodeC1(OpCodeC1Context opCtx, Cheat cheat)
        {

        }

        private void AssembleOpCodeC2(OpCodeC2Context opCtx, Cheat cheat)
        {

        }

        private void AssembleOpCodeC0(OpCodeC0Context opCtx, Cheat cheat)
        {
            /*opCodeC0: (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA LSQUARE (memType=MEM_TYPE) PLUS_SIGN (offset=anyRef) RSQUARE
		            | (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA LSQUARE (addrReg=regRef) PLUS_SIGN (offset=anyRef) RSQUARE
            		| (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA (value=anyRef);*/
            RegisterConditionalOpcode opTyped = new RegisterConditionalOpcode();

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            opTyped.Condition = Enum.Parse<ConditionalComparisonType>(opCtx.cond.Text, true);
            opTyped.SourceRegister = Convert.ToUInt16(ParseRegRef(opCtx.source, cheat).Substring(1), 16);
            if (opCtx.memType != null)
            {
                opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);

                /* operand type is either 0 or 1... */
                if (GetAnyRefType(opCtx.offset) == AnyRefType.NUMBER)
                {
                    opTyped.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.offset,AnyRefType.NUMBER,cheat), 16);
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
                /* operand type is either 2 or 3 */
                if (GetAnyRefType(opCtx.offset) == AnyRefType.NUMBER)
                {
                    opTyped.RelativeAddress = Convert.ToUInt64(ParseAnyRef(opCtx.offset, AnyRefType.NUMBER, cheat), 16);
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
                    opTyped.OperandType = 4;
                }
                else if (GetAnyRefType(opCtx.offset) == AnyRefType.REGISTER)
                {
                    opTyped.OtherRegister = Convert.ToUInt16(ParseAnyRef(opCtx.value, AnyRefType.REGISTER, cheat).Substring(1), 16);
                    opTyped.OperandType = 5;
                }


                
            }
            else
            {
                throw new NotSupportedException();
            }
            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode9(OpCode9Context opCtx, Cheat cheat)
        {
            ArithmeticOpcode opTyped = new ArithmeticOpcode();
            

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
                }
                else if (GetAnyRefType(opCtx.right) == AnyRefType.REGISTER)
                {
                    opTyped.RightHandRegister = true;
                    opTyped.RegisterRight = Convert.ToUInt16(ParseAnyRef(opCtx.right, AnyRefType.REGISTER, cheat).Substring(1), 16);
                }
            }

            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode8(OpCode8Context opCtx, Cheat cheat)
        {
            KeypressConditionalOpcode opTyped = new KeypressConditionalOpcode();

            opTyped.Mask = Enum.Parse<KeyMask>(opCtx.key.Text);
            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode7(OpCode7Context opCtx, Cheat cheat)
        {
            LegacyArithmeticOpcode opTyped = new LegacyArithmeticOpcode();
            

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            opTyped.RegisterIndex = Convert.ToUInt16(ParseRegRef(opCtx.register, cheat).Substring(1), 16);
            opTyped.MathType = Enum.Parse<RegisterArithmeticType>(opCtx.func.Text, true);
            opTyped.Value = Convert.ToUInt32(ParseNumRef(opCtx.value), 16);
            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode5(OpCode5Context opCtx, Cheat cheat)
        {
            LoadRegisterMemoryOpcode opTyped = new LoadRegisterMemoryOpcode();

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);

            opTyped.RegisterIndex = Convert.ToUInt16(ParseRegRef(opCtx.register, cheat).Substring(1), 16);
            opTyped.Immediate = Convert.ToUInt64(ParseNumRef(opCtx.offset), 16);

            if (opCtx.memType != null)
            {
                opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);
            }
            else
            {
                opTyped.UseReg = true;
            }
            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode4(OpCode4Context opCtx, Cheat cheat)
        {
            LoadRegisterStaticOpcode opTyped = new LoadRegisterStaticOpcode();


            opTyped.RegisterIndex = Convert.ToUInt16(ParseRegRef(opCtx.register, cheat).Substring(1), 16);
            opTyped.Value = Convert.ToUInt64(ParseNumRef(opCtx.value), 16);
            cheat.Opcodes.Add(opTyped);
        }

        private void AssembleOpCode3(OpCode3Context opCtx, Cheat cheat)
        {
            LoopOpcode opTyped = new LoopOpcode();

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
            cheat.Opcodes.Add(new EndConditionalOpcode());
        }

        private void AssembleOpCode1(OpCode1Context opCtx, Cheat cheat)
        {
            ConditionalOpcode opTyped = new ConditionalOpcode();

            opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
            opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);

            opTyped.Condition = Enum.Parse<ConditionalComparisonType>(opCtx.cond.Text, true);
            opTyped.Immediate = Convert.ToUInt64(ParseNumRef(opCtx.offset), 16);
            opTyped.Value = Convert.ToUInt64(ParseNumRef(opCtx.value), 16);
            cheat.Opcodes.Add(opTyped);
        }

        public void AssembleStatement(StatementContext stmt,Cheat cheat)
        {
            switch(stmt.GetRuleContext<ParserRuleContext>(0))
            {
                case MovInstrContext op:
                    AssembleMovInstr(op, cheat);
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
                case OpCode7Context op:
                    AssembleOpCode7(op, cheat);
                    break;
                case OpCode8Context op:
                    AssembleOpCode8(op, cheat);
                    break;
                case OpCode9Context op:
                    AssembleOpCode9(op, cheat);
                    break;
                case OpCodeC0Context op:
                    AssembleOpCodeC0(op, cheat);
                    break;
                case OpCodeC1Context op:
                    AssembleOpCodeC1(op, cheat);
                    break;
                case OpCodeC2Context op:
                    AssembleOpCodeC2(op, cheat);
                    break;
            }
        }

        
    }
}
