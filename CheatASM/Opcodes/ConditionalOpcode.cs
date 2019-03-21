using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class ConditionalOpcode : CheatOpcode
    {
        public BitWidthType BitWidth;
        public MemoryAccessType MemType;
        public ConditionalComparisonType Condition;
        public UInt64 Immediate;
        public UInt64 Value;

        public ConditionalOpcode() { }

        public ConditionalOpcode(uint[] blocks)
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            MemType = (MemoryAccessType)GetNibble(blocks[0], 3);
            Condition = (ConditionalComparisonType)GetNibble(blocks[0], 4);
            Immediate = ((UInt64)(blocks[0] & 0xFF) << 32) + blocks[1];

            if (BitWidth == BitWidthType.q)
            {
                Value = ((UInt64)blocks[2] << 32) + blocks[3];
            }
            else
            {
                Value = blocks[2];
            }    
        }

        public override string ToASM()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Enum.GetName(typeof(ConditionalComparisonType), Condition));
            sb.Append(".").Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" [").Append(Enum.GetName(typeof(MemoryAccessType), MemType));
            sb.Append("+0x").Append(Immediate.ToString("X"));
            sb.Append("], 0x").Append(Value.ToString("X"));

            return sb.ToString();
        }

        public override string ToByteString()
        {
            /* 1TMC00AA AAAAAAAA VVVVVVVV (VVVVVVVV) */
            uint[] blocks = null;
            if (BitWidth == BitWidthType.q)
            {
                blocks = new uint[4];
            }
            else
            {
                blocks = new uint[3];
            }

            /* build first DWORD */
            SetNibble(ref blocks[0], 1, 1);
            SetNibble(ref blocks[0], 2, (uint)BitWidth);
            SetNibble(ref blocks[0], 3, (uint)MemType);
            SetNibble(ref blocks[0], 4, (uint)Condition);
            SetNibble(ref blocks[0], 5, 0);
            SetNibble(ref blocks[0], 6, 0);
            SetNibble(ref blocks[0], 7, (uint)((Immediate >> 36) & 0xF));
            SetNibble(ref blocks[0], 7, (uint)((Immediate >> 32) & 0xF));
            blocks[1] = (uint)(Immediate & 0xFFFFFFFF);
            if (BitWidth == BitWidthType.q)
            {
                blocks[2] = (uint)(Value >> 32);
                blocks[3] = (uint)(Value & 0xFFFFFFFF);
            }
            else
            {
                blocks[2] = (uint)(Value & 0xFFFFFFFF);
            }

            return GetBlocksAsString(blocks);

        }
    }
}
