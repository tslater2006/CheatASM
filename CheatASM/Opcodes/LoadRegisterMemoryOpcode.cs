using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class LoadRegisterMemoryOpcode : CheatOpcode
    {
        public BitWidthType BitWidth;
        public MemoryAccessType MemType;
        public uint RegisterIndex;
        public bool UseReg;
        public UInt64 Immediate;

        public LoadRegisterMemoryOpcode() { }

        public LoadRegisterMemoryOpcode(uint[] blocks)
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            MemType = (MemoryAccessType)GetNibble(blocks[0], 3);
            RegisterIndex = GetNibble(blocks[0], 4);
            UseReg = GetNibble(blocks[0], 5) == 1;
            if (UseReg)
            {
                Immediate = ((UInt64)(blocks[0] & 0xFF) << 32) + blocks[1];
            }
            else
            {
                Immediate = blocks[1];
            }
        }

        public override string ToASM()
        {
            /* mov(b/w/d/q) R0 [HEAP+IMM] */
            /* mov(b/w/d/q) R0 [R0+IMM] */
            StringBuilder sb = new StringBuilder();
            sb.Append("mov");
            sb.Append(".").Append(Enum.GetName(typeof(BitWidthType), BitWidth));

            sb.Append(" R" + RegisterIndex.ToString("X")).Append(", [");
            if (UseReg)
            {
                sb.Append("R" + RegisterIndex.ToString("X"));
            }
            else
            {
                sb.Append(Enum.GetName(typeof(MemoryAccessType), MemType));
            }

            sb.Append("+0x").Append(Immediate.ToString("X")).Append("]");
            return sb.ToString();


        }

        public override string ToByteString()
        {
            /* 5TMR00AA AAAAAAAA */
            uint[] blocks = new uint[2];
            SetNibble(ref blocks[0], 1, 5);
            SetNibble(ref blocks[0], 2, ((uint)BitWidth & 0xF));
            SetNibble(ref blocks[0], 3, ((uint)MemType & 0xF));
            SetNibble(ref blocks[0], 4, ((uint)RegisterIndex & 0xF));

            if (UseReg)
            {
                SetNibble(ref blocks[0], 5, 1);
            }
            else
            {
                SetNibble(ref blocks[0], 5, 0);
            }
            SetNibble(ref blocks[0], 6, 0);

            SetNibble(ref blocks[0], 7, (uint)((Immediate >> 36) & 0xF));
            SetNibble(ref blocks[0], 8, (uint)((Immediate >> 32) & 0xF));

            blocks[1] = (uint)(Immediate & 0xFFFFFFFF);

            return GetBlocksAsString(blocks);
        }
    }
}
