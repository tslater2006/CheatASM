using CheatASM;
using NUnit.Framework;
using System.IO;

namespace Tests
{
    public class Tests
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
            /*opCode0: MOVE DOT (bitWidth=BIT_WIDTH) LSQUARE (memType=MEM_TYPE)PLUS_SIGN(register=regRef)PLUS_SIGN(offset=numRef) RSQUARE COMMA (value=numRef);*/
            var noReg = asm.AssembleInstruction("mov.d [MAIN+R7+0x3456], 0x789");

            Assert.AreEqual("04070000 00003456 00000789", noReg.Cheats[0].Opcodes[0].ToByteString());

            asm.Variables.Add(new VariableDeclaration() { Name = "regVar", Value = "0x10", Type = ".u32", Const = false });
            asm.Variables.Add(new VariableDeclaration() { Name = "constVar", Value = "0x10", Type = ".u32", Const = true });

            var regTest = asm.AssembleInstruction("mov.d [MAIN+regVar+constVar], 0x0");
            Assert.AreEqual("040F0000 00000010 00000000", regTest.Cheats[0].Opcodes[0].ToByteString());


            
            Assert.Throws<AssemblerException>(() => {
                var undefinedTest = asm.AssembleInstruction("mov.d [MAIN+noVar+0x123], 0x123");
            });

            Assert.Throws<AssemblerException>(() =>
            {
                var wrongVarTest = asm.AssembleInstruction("mov.d [MAIN+constVar+regVar], 0x0");
            });
        }


        [Test]
        public void TestASMSkeleton()
        {
            var asmText = @".title {1234}
                            .build {456}

                            .cheat master ""Setup""
                            mov.q R0, [MAIN + 0x1234]

                            .cheat ""Always 10 coins""
                            mov.d R1, 0xA
                            mov.d[R0 + 0x1234], R1";

            var result = asm.AssembleString(asmText);

            var assembled = result.ToString();
            Assert.AreEqual("[Assembled by CheatASM]\r\n{Setup}\r\n58000000 00001234\r\n\r\n[Always 10 coins]\r\n40010000 00000000 0000000A\r\nA4100200 00001234\r\n\r\n", assembled.ToString());

            Assert.AreEqual("1234", result.TitleID);
            Assert.AreEqual("456", result.BuildID);

        }

        [Test]
        public void TestGameInfo()
        {
            var asmText = @".gameinfo ""Mario Kart 8 (1.7.1)""
                            .cheat ""Always 10 coins""
                            mov.d R1, 0xA
                            mov.d[R0 + 0x1234], R1";

            Assert.IsTrue(asm.AssembleString(asmText).ToString().StartsWith("[Mario Kart 8 (1.7.1)]"));

            asmText = @".cheat ""Always 10 coins""
                            mov.d R1, 0xA
                            mov.d[R0 + 0x1234], R1";
            Assert.IsTrue(asm.AssembleString(asmText).ToString().StartsWith("[Assembled by CheatASM]"));
        }
        [Test]
        public void TestRegisterVar()
        {
            var asmText = @"regVar: .u32 0x0A
                            offsetConst: .u32 const 0x1234
                            .cheat ""Always 10 coins""
                            mov.d[R0 + offsetConst], regVar
                            mov.d regVar, offsetConst";

            var result = asm.AssembleString(asmText);
        }
            
        [Test]
        public void TestMovInstruction()
        {
            var asmText = File.ReadAllText(@"C:\Users\tslat\source\repos\CheatASM\CheatASM\examples\mov_instructions.asm");
            var result = asm.AssembleString(asmText);
        }

        [Test]
        public void TestSaveRestore()
        {
            var result = asm.AssembleString("savereg R0, 0x1");
            result = asm.AssembleString("savereg R1, 0x1");
            result = asm.AssembleString("save R0, R1");
            result = asm.AssembleString("saveall");

            result = asm.AssembleString("loadreg 0x1, R0");
            result = asm.AssembleString("loadreg 0x1, R1");
            result = asm.AssembleString("load R0, R1");
            result = asm.AssembleString("loadall");
        }
    }
}