using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using static CheatASM.CheatASMParser;

namespace CheatASM
{

    public class Assembler : BaseErrorListener
    {
        string errorMsg;
        int errorPos;
        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            base.SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e);
            errorMsg = msg;
            errorPos = charPositionInLine;
        }
        public string AssembleFile(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();

            foreach (string line in File.ReadLines(filePath))
            {
                if (line.StartsWith("{") || line.StartsWith("[") || line.Trim().Length == 0)
                {
                    sb.AppendLine(line);
                }
                else
                {
                    var lineOut = AssembleLine(line);
                    sb.AppendLine(lineOut);

                }
            }
            return sb.ToString();
        }
        public string AssembleLine(string line)
        {
            AntlrInputStream stream = new AntlrInputStream(line);
            CheatASMLexer lexer = new CheatASMLexer(stream);
            CheatASMParser parser = new CheatASMParser(new CommonTokenStream(lexer));
            parser.ErrorHandler = new DefaultErrorStrategy();
            parser.TrimParseTree = true;
            parser.BuildParseTree = true;
            parser.AddErrorListener(this);
            StatementContext stmt = null;
            errorMsg = null;
            errorPos = 0;
            stmt = parser.statement();

            if (errorMsg != null)
            {
                return "# Error: " + errorMsg + " -- " + line;
            }
            CheatOpcode op = null;
            if (stmt.opCode0() != null)
            {
                OpCode0Context opCtx = stmt.opCode0();
                StoreStaticOpcode opTyped = new StoreStaticOpcode();
                op = opTyped;

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);
                opTyped.OffsetRegister = Convert.ToUInt16(opCtx.register.Text.Substring(1),16);
                opTyped.RelativeAddress = Convert.ToUInt64(opCtx.offset.Text, 16);
                opTyped.Value = Convert.ToUInt64(opCtx.value.Text, 16);
                /* assemble opcode 0 */

            }
            else if (stmt.opCode1() != null)
            {
                OpCode1Context opCtx = stmt.opCode1();
                ConditionalOpcode opTyped = new ConditionalOpcode();
                op = opTyped;

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);

                opTyped.Condition = Enum.Parse<ConditionalComparisonType>(opCtx.cond.Text, true);
                opTyped.Immediate = Convert.ToUInt64(opCtx.offset.Text,16);
                opTyped.Value = Convert.ToUInt64(opCtx.value.Text,16);

            }
            else if (stmt.opCode2() != null)
            {
                op = new EndConditionalOpcode();
            }
            else if (stmt.opCode3() != null)
            {
                OpCode3Context opCtx = stmt.opCode3();
                LoopOpcode opTyped = new LoopOpcode();
                op = opTyped;

                if (opCtx.endloop != null)
                {
                    opTyped.IsEnd = true;
                    opTyped.RegisterIndex = Convert.ToUInt16(opCtx.register.Text.Substring(1),16);
                }
                else
                {
                    opTyped.IsEnd = false;
                    opTyped.RegisterIndex = Convert.ToUInt16(opCtx.register.Text.Substring(1),16);
                    opTyped.Count = Convert.ToUInt32(opCtx.value.Text,16);
                }

            }
            else if (stmt.opCode4() != null)
            {
                OpCode4Context opCtx = stmt.opCode4();
                LoadRegisterStaticOpcode opTyped = new LoadRegisterStaticOpcode();
                op = opTyped;


                opTyped.RegisterIndex = Convert.ToUInt16(opCtx.register.Text.Substring(1),16);
                opTyped.Value = Convert.ToUInt64(opCtx.value.Text,16);
            }
            else if (stmt.opCode5() != null)
            {
                OpCode5Context opCtx = stmt.opCode5();
                LoadRegisterMemoryOpcode opTyped = new LoadRegisterMemoryOpcode();
                op = opTyped;

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);

                opTyped.RegisterIndex = Convert.ToUInt16(opCtx.register.Text.Substring(1),16);
                opTyped.Immediate = Convert.ToUInt64(opCtx.offset.Text,16);

                if (opCtx.memType != null)
                {
                    opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);
                }
                else
                {
                    opTyped.UseReg = true;
                }
            }
            else if (stmt.opCode6() != null)
            {
                OpCode6Context opCtx = stmt.opCode6();
                StoreStaticToAddressOpcode opTyped = new StoreStaticToAddressOpcode();
                op = opTyped;

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                opTyped.RegisterIndex = Convert.ToUInt16(opCtx.register.Text.Substring(1),16);

                if (opCtx.increment != null)
                {
                    opTyped.IncrementFlag = true;
                }
                if (opCtx.offsetReg != null)
                {
                    opTyped.OffsetEnableFlag = true;
                    opTyped.OffsetRegister = Convert.ToUInt16(opCtx.offsetReg.Text.Substring(1),16);
                }

                opTyped.Value = Convert.ToUInt64(opCtx.value.Text,16);
            }
            else if (stmt.opCode7() != null)
            {
                OpCode7Context opCtx = stmt.opCode7();
                LegacyArithmeticOpcode opTyped = new LegacyArithmeticOpcode();
                op = opTyped;

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                opTyped.RegisterIndex = Convert.ToUInt16(opCtx.register.Text.Substring(1),16);
                opTyped.MathType = Enum.Parse<RegisterArithmeticType>(opCtx.func.Text, true);
                opTyped.Value = Convert.ToUInt32(opCtx.value.Text,16);
            }
            else if (stmt.opCode8() != null)
            {
                OpCode8Context opCtx = stmt.opCode8();
                KeypressConditionalOpcode opTyped = new KeypressConditionalOpcode();
                op = opTyped;

                opTyped.Mask = Enum.Parse<KeyMask>(opCtx.key.Text);

            }
            else if (stmt.opCode9() != null)
            {
                OpCode9Context opCtx = stmt.opCode9();
                ArithmeticOpcode opTyped = new ArithmeticOpcode();
                op = opTyped;

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                opTyped.RegisterDest = Convert.ToUInt16(opCtx.dest.Text.Substring(1), 16);
                opTyped.MathType = Enum.Parse<RegisterArithmeticType>(opCtx.func.Text, true);

                opTyped.RegisterLeft = Convert.ToUInt16(opCtx.leftReg.Text.Substring(1), 16);

                if (opCtx.rightReg != null)
                {
                    opTyped.RightHandRegister = true;
                    opTyped.RegisterRight = Convert.ToUInt16(opCtx.rightReg.Text.Substring(1), 16);
                }
                else
                {
                    opTyped.RightHandRegister = false;
                    opTyped.Value = opTyped.Value = Convert.ToUInt64(opCtx.value.Text,16);
                }

            }
            else if (stmt.opCodeA() != null)
            {
                OpCodeAContext opCtx = stmt.opCodeA();
                StoreRegisterToAddressOpcode opTyped = new StoreRegisterToAddressOpcode();
                op = opTyped;

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                if (opCtx.baseReg != null)
                {
                    opTyped.AddressRegister = uint.Parse(opCtx.baseReg.Text.Substring(1));
                }
                opTyped.SourceRegister = uint.Parse(opCtx.sourceReg.Text.Substring(1));
                if (opCtx.increment != null)
                {
                    opTyped.IncrementFlag = true;
                }
                opTyped.OffsetType = 0;

                if (opCtx.memType != null)
                {
                    opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text);
                    /* has to be OffsetType 3,4,5 */
                    bool hasReg = opCtx.baseReg != null;
                    bool hasVal = opCtx.value != null;

                    if (hasReg && hasVal)
                    {
                        /* type 5 */
                        opTyped.OffsetType = 5;
                        opTyped.OffsetRegister = Convert.ToUInt16(opCtx.baseReg.Text.Substring(1),16);
                        opTyped.RelativeAddress = Convert.ToUInt64(opCtx.value.Text,16);
                    } else if (hasReg)
                    {
                        /* type 3 */
                        opTyped.OffsetType = 3;
                        opTyped.OffsetRegister = Convert.ToUInt16(opCtx.baseReg.Text.Substring(1),16);
                    } else if (hasVal)
                    {
                        /* type 4 */
                        opTyped.OffsetType = 4;
                        opTyped.RelativeAddress = Convert.ToUInt64(opCtx.value.Text,16);
                    }
                }
                else
                {
                    /* has to be OffsetType 1,2 */
                    if (opCtx.regIndex != null)
                    {
                        opTyped.OffsetType = 1;
                        opTyped.OffsetRegister = Convert.ToUInt16(opCtx.regIndex.Text.Substring(1),16);
                    }
                    if (opCtx.value != null)
                    {
                        opTyped.OffsetType = 2;
                        opTyped.RelativeAddress = Convert.ToUInt64(opCtx.value.Text,16);
                    }
                }
            } else if (stmt.opCodeC0() != null)
            {
                OpCodeC0Context opCtx = stmt.opCodeC0();
                RegisterConditionalOpcode opTyped = new RegisterConditionalOpcode();
                op = opTyped;

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                opTyped.Condition = Enum.Parse<ConditionalComparisonType>(opCtx.cond.Text, true);
                opTyped.SourceRegister = Convert.ToUInt16(opCtx.source.Text.Substring(1), 16);
                if (opCtx.memType != null)
                {
                    opTyped.MemType = Enum.Parse<MemoryAccessType>(opCtx.memType.Text, true);
                    
                    /* operand type is either 0 or 1... */
                    if (opCtx.offset != null)
                    {
                        opTyped.RelativeAddress = Convert.ToUInt64(opCtx.offset.Text, 16);
                        opTyped.OperandType = 0;
                    } else if (opCtx.offsetReg != null)
                    {
                        opTyped.OffsetRegister = Convert.ToUInt16(opCtx.offsetReg.Text.Substring(1), 16);
                        opTyped.OperandType = 1;
                    }
                } else if (opCtx.addrReg != null)
                {
                    /* operand type is either 2 or 3 */
                    if (opCtx.offset != null)
                    {
                        opTyped.RelativeAddress = Convert.ToUInt64(opCtx.offset.Text, 16);
                        opTyped.OperandType = 2;
                    }
                    else if (opCtx.offsetReg != null)
                    {
                        opTyped.OffsetRegister = Convert.ToUInt16(opCtx.offsetReg.Text.Substring(1), 16);
                        opTyped.OperandType = 3;
                    }
                } else if (opCtx.value != null)
                {
                    opTyped.Value = Convert.ToUInt64(opCtx.value.Text, 16);
                    opTyped.OperandType = 4;
                } else if (opCtx.otherReg != null)
                {
                    opTyped.OtherRegister  = Convert.ToUInt16(opCtx.otherReg.Text.Substring(1), 16);
                    opTyped.OperandType = 5;
                } else
                {
                    throw new NotSupportedException();
                }
                
            }

            return op.ToByteString();
        }
    }
}
