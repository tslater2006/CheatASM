using System;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class LoadRegisterStaticOpcode : CheatOpcode
    {
        public uint RegisterIndex;
        public UInt64 Value;

        public LoadRegisterStaticOpcode() { }

        public LoadRegisterStaticOpcode(uint[] blocks)
        {
            RegisterIndex = GetNibble(blocks[0], 4);
            Value = ((UInt64)blocks[1] << 32) + blocks[2];
        }

        public override string ToASM()
        {
            return "mov.q R" + RegisterIndex.ToString("X") + ", 0x" + Value.ToString("X");
        }

        public override string ToByteString()
        {
            uint[] blocks = new uint[3];
            SetNibble(ref blocks[0], 1, 4);
            SetNibble(ref blocks[0], 2, 0);
            SetNibble(ref blocks[0], 3, 0);
            SetNibble(ref blocks[0], 4, (uint)RegisterIndex & 0xF);
            SetNibble(ref blocks[0], 5, 0);
            SetNibble(ref blocks[0], 6, 0);
            SetNibble(ref blocks[0], 7, 0);
            SetNibble(ref blocks[0], 8, 0);
            blocks[1] = (uint)((Value >> 32) & 0xFFFFFFFF);
            blocks[2] = (uint)(Value & 0xFFFFFFFF);

            return GetBlocksAsString(blocks);
        }
    }
}
