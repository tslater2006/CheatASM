using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheatASM
{
    class OpcodeFF1ResumeProcess : CheatOpcode
    {
        public OpcodeFF1ResumeProcess() { }
        public OpcodeFF1ResumeProcess(uint[] blocks) { }
        public override string ToASM()
        {
            return "resume";
        }

        public override string ToByteString()
        {
            uint[] blocks = new uint[1];
            SetNibble(ref blocks[0], 1, 0xF);
            SetNibble(ref blocks[0], 2, 0xF);
            SetNibble(ref blocks[0], 3, 0x1);
            SetNibble(ref blocks[0], 4, 0);
            SetNibble(ref blocks[0], 5, 0);
            SetNibble(ref blocks[0], 6, 0);
            SetNibble(ref blocks[0], 7, 0);
            SetNibble(ref blocks[0], 8, 0);
            return GetBlocksAsString(blocks);
        }
    }
}
