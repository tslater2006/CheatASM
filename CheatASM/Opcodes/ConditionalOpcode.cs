using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class ConditionalOpcode : CheatOpcode
    {
        BitWidthType BitWidth;
        MemoryAccessType MemType;
        ConditionalComparisonType Condition;
        UInt64 Immediate;
        UInt64 Value;

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
            sb.Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" [").Append(Enum.GetName(typeof(MemoryAccessType), MemType));
            sb.Append("+0x").Append(Immediate.ToString("x"));
            sb.Append("], 0x").Append(Value.ToString("x"));

            return sb.ToString();
        }
    }
}
