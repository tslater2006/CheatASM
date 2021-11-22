using CheatASM;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheatASMTests
{
    class DisassemblerTests
    {
        Disassembler disasm;
        [SetUp]
        public void Setup()
        {
            disasm = new Disassembler();
        }
        [Test]
        public void TestOpcode0()
        {
            /* MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE) PLUS_SIGN (regOffset=regRef) (PLUS_SIGN (numOffset=numRef))? RSQUARE COMMA (value=numRef)*/
            /* b w d q */
            /* MAIN HEAP */
            Dictionary<string, string> instructions = new()
            {
                { "04020000 00000000 00000123", "mov.d [MAIN + R2], 0x123" },
                { "08020000 00000000 00000000 00000123", "mov.q [MAIN + R2], 0x123" },
                { "04080000 00000007 00000345", "mov.d [MAIN + R8 + 0x7], 0x345" },
                { "02170000 00000000 00000236", "mov.w [HEAP + R7], 0x236" },
                { "01170000 00000009 000000FE", "mov.b [HEAP + R7 + 0x9], 0xFE" },
            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcode1()
        {
            /* (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) LSQUARE(memType=MEM_TYPE)PLUS_SIGN(offset=numRef)RSQUARE COMMA (value=numRef); */
            /* gt ge lt le eq ne */
            /* b w d q */
            /* MAIN HEAP */
            Dictionary<string, string> instructions = new()
            {
                { "14010000 00000123 00000456", "gt.d [MAIN + 0x123], 0x456" },
                { "11120000 00000123 00000056", "ge.b [HEAP + 0x123], 0x56" },
                { "18030012 3456789A 00000045 64123478", "lt.q [MAIN + 0x123456789A], 0x4564123478" },
                { "14040000 00000123 00000456", "le.d [MAIN + 0x123], 0x456" },
                { "14050000 00000123 00000456", "eq.d [MAIN + 0x123], 0x456" },
                { "14060000 00000123 00000456", "ne.d [MAIN + 0x123], 0x456" },
            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                disasm.ResetIndent();
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcode2()
        {
            Assert.AreEqual(disasm.DisassembleLine("20000000"), "endcond");
            Assert.AreEqual(disasm.DisassembleLine("21000000"), "else");
        }

        [Test]
        public void TestOpcode3()
        {
            /* LOOP (register=regRef) COMMA(value=numRef) | (endloop=END_LOOP) (register=regRef);*/

            Dictionary<string, string> instructions = new()
            {
                { "30070000 00000006", "loop R7, 0x6" },
                { "31060000", "endloop R6" }
            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcode4()
        {
            /* MOVE (DOT BIT_WIDTH)? (register=regRef) COMMA (value=numRef) # opCode4 */

            Dictionary<string, string> instructions = new()
            {
                { "40000000 00000000 00000001", "mov.q R0, 0x1" },
                { "40020000 00000000 00000003", "mov.q R2, 0x3" },
                { "40070000 00000000 12345678", "mov.q R7, 0x12345678" },
                { "400A0000 11223344 55667788", "mov.q RA, 0x1122334455667788" }
            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcode5()
        {
            /* MOVE DOT (bitWidth=BIT_WIDTH) (register=regRef) COMMA LSQUARE (memType=MEM_TYPE) (PLUS_SIGN (numOffset=numRef))? RSQUARE
               MOVE DOT (bitWidth=BIT_WIDTH) (register=regRef) COMMA LSQUARE (baseRegister=regRef) (PLUS_SIGN (numOffset=numRef))? RSQUARE */
            Dictionary<string, string> instructions = new()
            {
                { "51000000 00000012", "mov.b R0, [MAIN + 0x12]" },
                { "54000000 00000000", "mov.d R0, [MAIN]" },
                { "52000000 00000000", "mov.w R0, [MAIN]" },
                { "58100011 22334455", "mov.q R0, [HEAP + 0x1122334455]" },
                { "51001000 00000012", "mov.b R0, [R0 + 0x12]" },
                { "54001000 00000000", "mov.d R0, [R0]" },
                { "52001000 00000000", "mov.w R0, [R0]" },

            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }

        }

        [Test]
        public void TestOpcode6()
        {
            /* MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (base=regRef) (PLUS_SIGN (regOffset=regRef))? RSQUARE COMMA (value=numRef) (increment=INCREMENT)? */
            Dictionary<string, string> instructions = new()
            {
                { "61070000 00000000 00000012", "mov.b [R7], 0x12" },
                { "62041000 00000000 00000012", "mov.w [R4], 0x12 inc" },
                { "64030120 00000000 00000012", "mov.d [R3 + R2], 0x12" },
                { "680B0120 11223344 55667788", "mov.q [RB + R2], 0x1122334455667788" },
            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcode7()
        {
            /* (func=LEGACY_ARITHMETIC) DOT (bitWidth=BIT_WIDTH) (register=regRef) COMMA (value=numRef); */
            /* add sub mul lsh rsh */
            Dictionary<string, string> instructions = new()
            {
                { "71070000 00000012", "add.b R7, 0x12" },
                { "72041000 00000012", "sub.w R4, 0x12" },
                { "74032000 00000012", "mul.d R3, 0x12" },
                { "780B3000 11223344", "lsh.q RB, 0x11223344" },
                { "74044000 55667788", "rsh.d R4, 0x55667788" },
            };


            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcode8()
        {
            foreach (var val in Enum.GetNames(typeof(KeyMask)))
            {
                var enumVal = (uint)(Enum.Parse(typeof(KeyMask), val));
                Assert.AreEqual(disasm.DisassembleLine($"8{enumVal.ToString("X7")}"), $"keycheck {val}");
                disasm.ResetIndent();
            }
        }

        [Test]
        public void TestOpcode9()
        {
            /* (func=ARITHMETIC) DOT (bitWidth=BIT_WIDTH) (dest=regRef) COMMA (leftReg=regRef) COMMA (right=anyRef); */

            Dictionary<string, string> instructions = new()
            {
                { "91073100 00000012", "add.b R7, R3, 0x12" },
                { "92141100 00000012", "sub.w R4, R1, 0x12" },
                { "94230100 00000012", "mul.d R3, R0, 0x12" },
                { "983BD100 00001122 33445566", "lsh.q RB, RD, 0x112233445566" },
                { "94447100 55667788", "rsh.d R4, R7, 0x55667788" },
                { "91573100 00000012", "and.b R7, R3, 0x12" },
                { "91673100 00000012", "or.b R7, R3, 0x12" },
                { "91873100 00000012", "xor.b R7, R3, 0x12" },
                { "91073020", "add.b R7, R3, R2" },
                { "92141020", "sub.w R4, R1, R2" },
                { "94230070", "mul.d R3, R0, R7" },
                { "983BD080", "lsh.q RB, RD, R8" },
                { "94447010", "rsh.d R4, R7, R1" },
                { "91573000", "and.b R7, R3, R0" },
                { "91673040", "or.b R7, R3, R4" },
                { "91873060", "xor.b R7, R3, R6" },
                { "91773000", "not.b R7, R3" },
                { "91973000", "copy.b R7, R3" },
            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcodeA()
        {
            /* 
               MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (base=regRef) (PLUS_SIGN (regOffset=regRef))? RSQUARE COMMA (regValue=regRef) (increment=INCREMENT)? #opCodeA
               MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (base=regRef) (PLUS_SIGN (numOffset=numRef))? RSQUARE COMMA (regValue=regRef) (increment=INCREMENT)? #opCodeA
               MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE) (PLUS_SIGN (regOffset=regRef))? (PLUS_SIGN (numOffset=numRef))? RSQUARE COMMA (regValue=regRef) #opCodeA;
            */
            Dictionary<string, string> instructions = new()
            {
                { "A4100000", "mov.d [R0], R1" },
                { "A4291000", "mov.d [R9], R2 inc" },
                { "A43A0110", "mov.d [RA + R1], R3" },
                { "A44B1110", "mov.d [RB + R1], R4 inc" },
                { "A45C0200 00000123", "mov.d [RC + 0x123], R5" },
                { "A46D1200 00000123", "mov.d [RD + 0x123], R6 inc" },
                { "A47E0300", "mov.d [MAIN + RE], R7" },
                { "A4700400 00000123", "mov.d [MAIN + 0x123], R7" },
                { "A48F0511 23456789", "mov.d [HEAP + RF + 0x123456789], R8" }
            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }

        }

        [Test]
        public void TestOpcodeC0()
        {
            /* 
               (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA LSQUARE (memType=MEM_TYPE) PLUS_SIGN (offset=anyRef) RSQUARE
		       (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA LSQUARE (addrReg=regRef) (PLUS_SIGN (offset=anyRef))? RSQUARE
		       (cond=CONDITIONAL) DOT (bitWidth=BIT_WIDTH) (source=regRef) COMMA (value=anyRef); 
            */

            Dictionary<string, string> instructions = new()
            {
                { "C01_0510", ".b R0, R1" },
                { "C02_0400 00000123", ".w R0, 0x123" },
                { "C04_0210 00000000", ".d R0, [R1]" },
                { "C08_0210 00000123", ".q R0, [R1 + 0x123]" },
                { "C01_0312", ".b R0, [R1 + R2]" },
                { "C02_0000 00000123", ".w R0, [MAIN + 0x123]" },
                { "C04_0010 00000123", ".d R0, [HEAP + 0x123]" },
                { "C08_0000 00000000", ".q R0, [MAIN]" },
                { "C01_0100", ".b R0, [MAIN + R0]" },

            };
            string[] comparisons = new string[] { "gt", "ge", "lt", "le", "eq", "ne" };
            foreach (var kvp in instructions)
            {
                foreach (var comp in comparisons)
                {
                    
                    var testAnswer = kvp.Key;
                    testAnswer = testAnswer.Replace('_', ((uint)(Enum.Parse(typeof(ConditionalComparisonType), comp))).ToString()[0]);

                    var testInstruction = kvp.Value;
                    testInstruction = $"{comp}{testInstruction}";
                    

                    var disassembledInstruction = disasm.DisassembleLine(testAnswer);
                    Assert.AreEqual(disassembledInstruction, testInstruction, $"Disassembly of '{testAnswer}' gave '{disassembledInstruction}' instead of '{testInstruction}'");
                    disasm.ResetIndent();
                }
            }
        }

        [Test]
        public void TestOpcodeC1()
        {
            /* 
             (func=SAVE) DOT (type=REG) (index=numRef) COMMA (reg=regRef)
        | (func=LOAD) DOT (type=REG) (reg=regRef) COMMA (index=numRef) 
              | (func=CLEAR) DOT (type=REG) (reg=regRef)
        | (func=CLEAR) DOT (type=SAVED) (index=numRef)
             */

            Dictionary<string, string> instructions = new()
            {
                { "C1010310", "save.reg 0x1, R3" },
                { "C1030100", "load.reg R3, 0x1" },
                { "C1030030", "clear.reg R3" },
                { "C1010020", "clear.saved 0x1" },
            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcodeC2()
        {
            /* 
             (func=SAVE) DOT (type=REG) (index=numRef) COMMA (reg=regRef)
        | (func=LOAD) DOT (type=REG) (reg=regRef) COMMA (index=numRef) 
              | (func=CLEAR) DOT (type=REG) (reg=regRef)
        | (func=CLEAR) DOT (type=SAVED) (index=numRef)
             */

            Dictionary<string, string> instructions = new()
            {
                { "C2100002", "save.regs R1" },
                { "C2100006", "save.regs R1, R2" },
                { "C210FFFF", "save.regs R0, R1, R2, R3, R4, R5, R6, R7, R8, R9, RA, RB, RC, RD, RE, RF" },

                { "C2000002", "load.regs R1" },
                { "C2000006", "load.regs R1, R2" },
                { "C200FFFF", "load.regs R0, R1, R2, R3, R4, R5, R6, R7, R8, R9, RA, RB, RC, RD, RE, RF" },

                { "C2300002", "clear.regs R1" },
                { "C2300006", "clear.regs R1, R2" },
                { "C230FFFF", "clear.regs R0, R1, R2, R3, R4, R5, R6, R7, R8, R9, RA, RB, RC, RD, RE, RF" },


                { "C220001E", "clear.saved 0x1, 0x2, 0x3, 0x4" },
            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcodeC3()
        {
            /* 
              (func=SAVE) DOT (type=STATIC) (sreg=SREGISTER) COMMA (reg=regRef)
              (func=LOAD) DOT (type=STATIC) (reg=regRef) COMMA (sreg=SREGISTER);
            */

            Dictionary<string, string> instructions = new()
            {
                { "C3000810", "save.static SR1, R0" },
                { "C3000070", "load.static R0, SR7" },
                { "C3000FF1", "save.static SR7F, R1" },
                { "C30007F6", "load.static R6, SR7F" },

            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcodeFF0()
        {
            /* opCodeFF0: (func=PAUSE); */
            Assert.AreEqual(disasm.DisassembleLine("FF000000"), "pause");
        }

        [Test]
        public void TestOpcodeFF1()
        {
            /* opCodeFF1: (func=RESUME); */
            Assert.AreEqual(disasm.DisassembleLine("FF100000"), "resume");
        }

        [Test]
        public void TestOpcodeFFF()
        {
            /* 
               opCodeFFF: (func=LOG) DOT (bitWidth=BIT_WIDTH) (id=HEX_NUMBER) COMMA LSQUARE (memType=MEM_TYPE) (PLUS_SIGN (offset=anyRef))? RSQUARE
               (func=LOG) DOT (bitWidth=BIT_WIDTH) (id=HEX_NUMBER) COMMA LSQUARE (addrReg=regRef) (PLUS_SIGN (offset=anyRef))? RSQUARE
               (func=LOG) DOT (bitWidth=BIT_WIDTH) (id=HEX_NUMBER) COMMA (value=regRef);
            */

            Dictionary<string, string> instructions = new()
            {
                { "FFF11000 00000000", "log.b 0x1, [MAIN]" },
                { "FFF21000 00000123", "log.w 0x1, [MAIN + 0x123]" },
                { "FFF41103", "log.d 0x1, [MAIN + R3]" },
                { "FFF81010 00000123", "log.q 0x1, [HEAP + 0x123]" },
                { "FFF11220 00000000", "log.b 0x1, [R2]" },
                { "FFF11323", "log.b 0x1, [R2 + R3]" },
                { "FFF21240 00000123", "log.w 0x1, [R4 + 0x123]" },
                { "FFF41470", "log.d 0x1, R7" },
            };

            foreach (var kvp in instructions)
            {
                var disassembledInstruction = disasm.DisassembleLine(kvp.Key);
                Assert.AreEqual(disassembledInstruction, kvp.Value, $"Disassembly of '{kvp.Key}' gave '{disassembledInstruction}' instead of '{kvp.Value}'");
            }
        }
    }
}
