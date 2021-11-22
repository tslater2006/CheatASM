using Antlr4.Runtime;
using CheatASM;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace CheatASMTests
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
            Assert.AreEqual(asm.AssembleSingleInstruction("else"), "21000000");
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

        [Test]
        public void TestOpcode8()
        {
            foreach(var val in Enum.GetNames(typeof(KeyMask)))
            {
                var enumVal = (uint)(Enum.Parse(typeof(KeyMask), val));
                Assert.AreEqual(asm.AssembleSingleInstruction($"keycheck {val})"), $"8{enumVal.ToString("X7")}");
            }

            Assert.Throws<AssemblerException>(() =>
            {
                asm.AssembleSingleInstruction("keycheck Q");
            });
        }


        [Test]
        public void TestOpcode9()
        {
            /* (func=ARITHMETIC) DOT (bitWidth=BIT_WIDTH) (dest=regRef) COMMA (leftReg=regRef) COMMA (right=anyRef); */

            Dictionary<string, string> instructions = new()
            {
                { "add.b R7, R3, 0x12", "91073100 00000012" },
                { "sub.w R4, R1, 0x12", "92141100 00000012" },
                { "mul.d R3, R0, 0x12", "94230100 00000012" },
                { "lsh.q RB, RD, 0x112233445566", "983BD100 00001122 33445566" },
                { "rsh.d R4, R7, 0x55667788", "94447100 55667788" },
                { "and.b R7, R3, 0x12", "91573100 00000012" },
                { "or.b R7, R3, 0x12", "91673100 00000012" },
                { "xor.b R7, R3, 0x12", "91873100 00000012" },
                { "add.b R7, R3, R2", "91073020" },
                { "sub.w R4, R1, R2", "92141020" },
                { "mul.d R3, R0, R7", "94230070" },
                { "lsh.q RB, RD, R8", "983BD080" },
                { "rsh.d R4, R7, R1", "94447010" },
                { "and.b R7, R3, R0", "91573000" },
                { "or.b R7, R3, R4", "91673040" },
                { "xor.b R7, R3, R6", "91873060" },
                { "not.b R7, R3", "91773000" },
                { "copy.b R7, R3", "91973000" },
            };

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
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
                { "mov.d [R0], R1", "A4100000" },
                { "mov.d [R9], R2 inc", "A4291000" },
                { "mov.d [RA+ R1], R3", "A43A0110" },
                { "mov.d [RB + R1], R4 inc", "A44B1110" },
                { "mov.d [RC + 0x123], R5", "A45C0200 00000123" },
                { "mov.d [RD + 0x123], R6 inc", "A46D1200 00000123" },
                { "mov.d [MAIN + RE], R7", "A47E0300" },
                { "mov.d [MAIN + 0x123], R7", "A4700400 00000123" },
                { "mov.d [HEAP + RF + 0x123456789], R8", "A48F0511 23456789" }
            };

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
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
                { ".b R0, R1", "C01_0510" },
                { ".w R0, 0x123", "C02_0400 00000123" },
                { ".d R0, [R1]", "C04_0210 00000000" },
                { ".q R0, [R1 + 0x123]", "C08_0210 00000123" },
                { ".b R0, [R1 + R2]", "C01_0312" },
                { ".w R0, [MAIN + 0x123]", "C02_0000 00000123" },
                { ".d R0, [HEAP + 0x123]", "C04_0010 00000123" },
                { ".q R0, [MAIN]", "C08_0000 00000000" },
                { ".b R0, [MAIN + R0]", "C01_0100" },

            };
            string[] comparisons = new string[] { "gt", "ge", "lt", "le", "eq", "ne" };
            foreach (var kvp in instructions)
            {
                foreach (var comp in comparisons)
                {
                    var testInstruction = kvp.Key;
                    var testAnswer = kvp.Value;
                    testInstruction = $"{comp}{testInstruction}";
                    testAnswer = testAnswer.Replace('_', ((uint)(Enum.Parse(typeof(ConditionalComparisonType), comp))).ToString()[0]);

                    var assembledInstruction = asm.AssembleSingleInstruction(testInstruction);
                    Assert.AreEqual(assembledInstruction, testAnswer, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
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
                { "save.reg 0x1, R3", "C1010310" },
                { "load.reg R3, 0x1", "C1030100" },
                { "clear.reg R3", "C1030030" },
                { "clear.saved 0x1", "C1010020" },
            };

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
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
                { "save.regs R1", "C2100002" },
                { "save.regs R1,R2", "C2100006" },
                { "save.regs R0, R1,R2,R3,R4,R5,R6,R7,R8,R9,RA,RB,RC,RD,RE,RF", "C210FFFF" },

                { "load.regs R1", "C2000002" },
                { "load.regs R1,R2", "C2000006" },
                { "load.regs R0,R1,R2,R3,R4,R5,R6,R7,R8,R9,RA,RB,RC,RD,RE,RF", "C200FFFF" },

                { "clear.regs R1", "C2300002" },
                { "clear.regs R1,R2", "C2300006" },
                { "clear.regs R0,R1,R2,R3,R4,R5,R6,R7,R8,R9,RA,RB,RC,RD,RE,RF", "C230FFFF" },

                
                { "clear.saved 0x1,0x2,0x3,0x4", "C220001E" },
            };

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
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
                { "save.static SR1, R0", "C3000810" },
                { "load.static R0, SR7", "C3000070" },
                { "save.static SR7F, R1", "C3000FF1" },
                { "load.static R6, SR7F", "C30007F6" },

            };

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
            }
        }

        [Test]
        public void TestOpcodeFF0()
        {
            /* opCodeFF0: (func=PAUSE); */
            Assert.AreEqual(asm.AssembleSingleInstruction("pause"), "FF000000");
        }

        [Test]
        public void TestOpcodeFF1()
        {
            /* opCodeFF1: (func=RESUME); */
            Assert.AreEqual(asm.AssembleSingleInstruction("resume"), "FF100000");
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
                { "log.b 0x1, [MAIN]", "FFF11000 00000000" },
                { "log.w 0x1, [MAIN + 0x123]", "FFF21000 00000123" },
                { "log.d 0x1, [MAIN + R3]", "FFF41103" },
                { "log.q 0x1, [HEAP + 0x123]", "FFF81010 00000123" },
                { "log.b 0x1, [R2]", "FFF11220 00000000" },
                { "log.b 0x1, [R2 + R3]", "FFF11323" },
                { "log.w 0x1, [R4 + 0x123]", "FFF21240 00000123" },
                { "log.d 0x1, R7", "FFF41470" },
            };

            foreach (var kvp in instructions)
            {
                var assembledInstruction = asm.AssembleSingleInstruction(kvp.Key);
                Assert.AreEqual(assembledInstruction, kvp.Value, $"Assembly of '{kvp.Key}' gave '{assembledInstruction}' instead of '{kvp.Value}'");
            }


        }

        [Test]
        public void TestVariables()
        {
            var programText = @"
                                floatTest: .f32 4.83
                                mainOffset: .u32 const 0x1234
                                coinOffset: .u32 const 0x12
                                ten: .u32 0xA

                                .cheat master ""Setup""
                                mov.d[R0 + 0x123], floatTest
                                mov.q R0, [MAIN + mainOffset]

                                .cheat ""Sample""
                                mov.d[R0 + mainOffset], R7
                                mov.q R0, [MAIN + mainOffset]";




            var assembledText = asm.AssembleString(programText).ToString();

        }

        [Test]
        public void TestIfStatement()
        {
            var programText = @".if.b [MAIN + 0x123] == 0x3
                                    mov r0, 0x3
                                .else
                                    mov r0, 0x1
                                .fi
                                .if.b R0 == 0x1
                                    mov r0, 0x3
                                .fi
                                .if.b R0 == R2
                                    mov r0, 0x3
                                .fi
                                .if.b R0 == [MAIN + 0x123]
                                    mov r0, 0x3
                                .fi
                                .if.b R0 == [MAIN + R2]
                                    mov r0, 0x3
                                .fi
                                .if.b R0 == [R2]
                                    mov r0, 0x3
                                .fi
                                .if.b R0 == [R2 + R3]
                                    mov r0, 0x3
                                .fi
                                .if.b R0 == [R2 + 0x123]
                                    mov r0, 0x3
                                .fi
                                
                                .if.b R0 == [MAIN]
                                    mov r0, 0x3
                                .fi";


            var assembledText = asm.AssembleString(programText).ToString();
        }
    }
    
}