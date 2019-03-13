using System;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class LoopOpcode : CheatOpcode
    {
        public bool IsEnd;
        public uint RegisterIndex;
        public UInt32 Count;

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
                return "loop R" + RegisterIndex.ToString("X") + ", 0x" + Count.ToString("X");
            } else
            {
                return "endloop R" + RegisterIndex.ToString("X");
            }
        }

        public override string ToByteString()
        {
            uint[] blocks = null;
            if (IsEnd)
            {
                blocks = new uint[1];
            } else
            {
                blocks = new uint[2];
            }

            if (IsEnd)
            {
                SetNibble(ref blocks[0], 1, 3);
                SetNibble(ref blocks[0], 2, 1);
                SetNibble(ref blocks[0], 3, 0);
                SetNibble(ref blocks[0], 4, (uint)RegisterIndex & 0xF);
                SetNibble(ref blocks[0], 5, 0);
                SetNibble(ref blocks[0], 6, 0);
                SetNibble(ref blocks[0], 7, 0);
                SetNibble(ref blocks[0], 8, 0);

            } else
            {
                SetNibble(ref blocks[0], 1, 3);
                SetNibble(ref blocks[0], 2, 0);
                SetNibble(ref blocks[0], 3, 0);
                SetNibble(ref blocks[0], 4, (uint)RegisterIndex & 0xF);
                SetNibble(ref blocks[0], 5, 0);
                SetNibble(ref blocks[0], 6, 0);
                SetNibble(ref blocks[0], 7, 0);
                SetNibble(ref blocks[0], 8, 0);

                blocks[1] = Count;
            }

            return GetBlocksAsString(blocks);
        }
    }
}
