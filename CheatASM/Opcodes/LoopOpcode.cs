using System;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class LoopOpcode : CheatOpcode
    {
        public bool IsEnd;
        uint RegisterIndex;
        UInt32 Count;

        public LoopOpcode() { }

        public LoopOpcode(uint[] blocks)
        {
            IsEnd = GetNibble(blocks[0], 2) == 1;
            RegisterIndex = GetNibble(blocks[0], 4);

            if (!IsEnd)
            {
                Count = blocks[1];
            }
        }

        public override string ToASM()
        {
            if (!IsEnd)
            {
                return "loop R" + RegisterIndex.ToString("X") + ", 0x" + Count.ToString("x");
            } else
            {
                return "endloop R" + RegisterIndex.ToString("X");
            }
        }

    }
}
