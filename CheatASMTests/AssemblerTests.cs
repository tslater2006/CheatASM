using Antlr4.Runtime;
using CheatASM;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Tests
{
    public class AssemblerTests
    {
        Assembler asm;
        Disassembler disasm;
        [SetUp]
        public void Setup()
        {
            asm = new Assembler();
            disasm = new Disassembler();
        }

        [Test]
        public void TestOpcode0()
        {
            /* MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE) PLUS_SIGN (regOffset=regRef) (PLUS_SIGN (numOffset=numRef))? RSQUARE COMMA (value=numRef)*/
            /* b w d q */
            /* MAIN HEAP */
            Dictionary<string,string> instructions = new()
            {
                { "mov.d [MAIN + R2], 0x123", "04020000 00000000 00000123" },
                { "mov.q [MAIN + R2], 0x123", "08020000 00000000 00000000 00000123" },
                { "mov.d [MAIN + R8 + 0x7], 0x345", "04080000 00000007 00000345" },
                { "mov.w [HEAP + R7], 0x236", "02170000 00000000 00000236" },
                { "mov.b [HEAP + R7 + 0x9], 0xFE", "01170000 00000009 000000FE" },
            };

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value,$"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
            }

            Assert.Throws<AssemblerException>(() =>
            {
                /* tests bit width constraint on literals */
                asm.AssembleSingleInstruction("mov.b [HEAP + R7 + 0x9], 0x1234");
            });

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
                { "gt.d [MAIN + 0x123], 0x456", "14010000 00000123 00000456" },
                { "ge.b [HEAP + 0x123], 0x56", "11120000 00000123 00000056" },
                { "lt.q [MAIN + 0x123456789A], 0x4564123478", "18030012 3456789A 00000045 64123478" },
                { "le.d [MAIN + 0x123], 0x456", "14040000 00000123 00000456" },
                { "eq.d [MAIN + 0x123], 0x456", "14050000 00000123 00000456" },
                { "ne.d [MAIN + 0x123], 0x456", "14060000 00000123 00000456" },
            };

            Assert.Throws<AssemblerException>(() =>
            {
                /* tests bit width constraint on literals */
                asm.AssembleSingleInstruction("gt.b[MAIN + 0x123], 0x456");
            });

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcode2()
        {
            Assert.AreEqual(asm.AssembleSingleInstruction("endcond"), "20000000");
        }

        [Test]
        public void TestOpcode3()
        {
            /* LOOP (register=regRef) COMMA(value=numRef) | (endloop=END_LOOP) (register=regRef);*/
            
            Dictionary<string, string> instructions = new()
            {
                { "loop R7, 0x6", "30070000 00000006" },
                { "endloop R6", "31060000" }
            };

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcode4()
        {
            /* MOVE (DOT BIT_WIDTH)? (register=regRef) COMMA (value=numRef) # opCode4 */

            Dictionary<string, string> instructions = new()
            {
                { "mov.b R0, 0x1", "40000000 00000000 00000001" },
                { "mov R2, 0x3", "40020000 00000000 00000003" },
                { "mov.d R7, 0x12345678", "40070000 00000000 12345678" },
                { "mov.q RA, 0x1122334455667788", "400A0000 11223344 55667788" }
            };

            Assert.Throws<AssemblerException>(() =>
            {
                /* tests bit width constraint on literals */
                asm.AssembleSingleInstruction("mov.b R0, 0x123");
            });

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcode5()
        {
            /* MOVE DOT (bitWidth=BIT_WIDTH) (register=regRef) COMMA LSQUARE (memType=MEM_TYPE) (PLUS_SIGN (numOffset=numRef))? RSQUARE
               MOVE DOT (bitWidth=BIT_WIDTH) (register=regRef) COMMA LSQUARE (baseRegister=regRef) (PLUS_SIGN (numOffset=numRef))? RSQUARE */
            Dictionary<string, string> instructions = new()
            {
                { "mov.b R0, [MAIN + 0x12]", "51000000 00000012" },
                { "mov.d R0, [MAIN + 0x0]", "54000000 00000000" },
                { "mov.w R0, [MAIN]", "52000000 00000000" },
                { "mov.q R0, [HEAP + 0x1122334455]", "58100011 22334455" },
                { "mov.b R0, [R0 + 0x12]", "51001000 00000012" },
                { "mov.d R0, [R0 + 0x0]", "54001000 00000000" },
                { "mov.w R0, [R0]", "52001000 00000000" },
                
            };


            Assert.Throws<AssemblerException>(() =>
            {
                asm.AssembleSingleInstruction("mov.b R0, [R7 + 0x12]");
            });

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
            }


        }

        [Test]
        public void TestOpcode6()
        {
            /* MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (base=regRef) (PLUS_SIGN (regOffset=regRef))? RSQUARE COMMA (value=numRef) (increment=INCREMENT)? */
            Dictionary<string, string> instructions = new()
            {
                { "mov.b [R7], 0x12", "61070000 00000000 00000012" },
                { "mov.w [R4], 0x12 inc", "62041000 00000000 00000012" },
                { "mov.d [R3 + R2], 0x12", "64030120 00000000 00000012" },
                { "mov.q [RB + R2], 0x1122334455667788", "680B0120 11223344 55667788" },
            };


            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcode7()
        {
            /* (func=LEGACY_ARITHMETIC) DOT (bitWidth=BIT_WIDTH) (register=regRef) COMMA (value=numRef); */
            /* add sub mul lsh rsh */
            Dictionary<string, string> instructions = new()
            {
                { "add.b R7, 0x12", "71070000 00000012" },
                { "sub.w R4, 0x12", "72041000 00000012" },
                { "mul.d R3,  0x12", "74032000 00000012" },
                { "lsh.q RB, 0x11223344", "780B3000 11223344" },
                { "rsh.d R4, 0x55667788", "74044000 55667788" },
            };


            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
            }


            Assert.Throws<AssemblerException>(() =>
            {
                asm.AssembleSingleInstruction("mul.b R3,  0x1234");
            });
        }
    }
    
}