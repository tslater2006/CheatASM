using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                opTyped.RegisterDest = uint.Parse(opCtx.dest.Text.Substring(1));
                opTyped.MathType = Enum.Parse<RegisterArithmeticType>(opCtx.func.Text, true);

                opTyped.RegisterLeft = uint.Parse(opCtx.leftReg.Text.Substring(1));

                if (opCtx.rightReg != null)
                {
                    opTyped.RightHandRegister = true;
                    opTyped.RegisterRight = uint.Parse(opCtx.rightReg.Text.Substring(1));
                }
                else
                {
                    opTyped.RightHandRegister = false;
                    opTyped.Value = opTyped.Value = Convert.ToUInt64(opCtx.value.Text);
                }

            }
            else if (stmt.opCodeA() != null)
            {
                OpCodeAContext opCtx = stmt.opCodeA();
                StoreRegisterToAddressOpcode opTyped = new StoreRegisterToAddressOpcode();
                op = opTyped;

                opTyped.BitWidth = Enum.Parse<BitWidthType>(opCtx.bitWidth.Text, true);
                opTyped.RegisterBase = uint.Parse(opCtx.baseReg.Text.Substring(1));
                opTyped.RegisterToWrite = uint.Parse(opCtx.sourceReg.Text.Substring(1));
                if (opCtx.increment != null)
                {
                    opTyped.IncrementFlag = true;
                }
                opTyped.OffsetType = 0;
                if (opCtx.regIndex !=null)
                {
                    opTyped.OffsetType = 1;
                    opTyped.RegIndex1 = uint.Parse(opCtx.regIndex.Text.Substring(1));
                }
                if (opCtx.value != null)
                {
                    opTyped.OffsetType = 2;

                    opTyped.Value = Convert.ToUInt64(opCtx.value.Text);

                }

            }

            return op.ToByteString();
        }
    }
}
