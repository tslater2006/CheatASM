using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class LoadRegisterMemoryOpcode : CheatOpcode
    {
        BitWidthType BitWidth;
        MemoryAccessType MemType;
        uint RegisterIndex;
        bool UseReg;
        UInt64 Immediate;

        public LoadRegisterMemoryOpcode() { };

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
            sb.Append(Enum.GetName(typeof(BitWidthType), BitWidth));

            sb.Append(" R" + RegisterIndex.ToString("X")).Append(", [");
            if (UseReg)
            {
                sb.Append("R" + RegisterIndex.ToString("X"));
            }
            else
            {
                sb.Append(Enum.GetName(typeof(MemoryAccessType), MemType));
            }

            sb.Append("+0x").Append(Immediate.ToString("x")).Append("]");
            return sb.ToString();


        }
    }
}
