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
        [SetUp]
        public void Setup()
        {
            asm = new Assembler();
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

            foreach(var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value,$"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
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
                { "gt.d [MAIN + 0x123], 0x456", "14010000 00000123 00000456" },
                { "ge.b [HEAP + 0x123], 0x56", "11120000 00000123 00000056" },
                { "lt.q [MAIN + 0x123456789A], 0x4564123478", "18030012 3456789A 00000045 64123478" },
                { "le.d [MAIN + 0x123], 0x456", "14040000 00000123 00000456" },
                { "eq.d [MAIN + 0x123], 0x456", "14050000 00000123 00000456" },
                { "ne.d [MAIN + 0x123], 0x456", "14060000 00000123 00000456" },
            };

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
    }
    
}