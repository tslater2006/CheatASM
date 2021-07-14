using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheatASM
{
    class OpcodeFF0PauseProcess : CheatOpcode
    {
        public OpcodeFF0PauseProcess() { }
        public OpcodeFF0PauseProcess(uint[] blocks) { }
        public override string ToASM()
        {
            return "pause";
        }

        public override string ToByteString()
        {
            uint[] blocks = new uint[1];            
            SetNibble(ref blocks[0], 1, 0xF);
            SetNibble(ref blocks[0], 2, 0xF);
            SetNibble(ref blocks[0], 3, 0);
            SetNibble(ref blocks[0], 4, 0);
            SetNibble(ref blocks[0], 5, 0);
            SetNibble(ref blocks[0], 6, 0);
            SetNibble(ref blocks[0], 7, 0);
            SetNibble(ref blocks[0], 8, 0);
            return GetBlocksAsString(blocks);
        }
    }
}
