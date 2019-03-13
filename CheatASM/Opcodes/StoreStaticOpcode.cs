using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class StoreStaticOpcode : CheatOpcode
    {
        BitWidthType BitWidth;
        MemoryAccessType MemType;
        uint OffsetRegister;
        long RelativeAddress;
        UInt64 Value;
        public StoreStaticOpcode(UInt32[] blocks) : base(blocks[0])
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            MemType = (MemoryAccessType)GetNibble(blocks[0], 3);
            OffsetRegister = GetNibble(blocks[0], 4);
            RelativeAddress = blocks[1];
            if (BitWidth == BitWidthType.q)
            {
                Value = ((UInt64)blocks[2] << 32) | blocks[3];
            }
            else
            {
                Value = blocks[2];
            }
        }

        public override string ToASM()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("mov");
            sb.Append(Enum.GetName(typeof(BitWidthType), BitWidth));

            sb.Append(" [");

            sb.Append(Enum.GetName(typeof(MemoryAccessType), MemType));

            sb.Append("+R").Append(OffsetRegister.ToString("X")).Append("+0x");
            sb.Append(RelativeAddress.ToString("x"));
            sb.Append("], 0x").Append(Value.ToString("x"));
            return sb.ToString();
        }
    }
}
