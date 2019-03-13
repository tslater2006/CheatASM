using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class StoreRegisterToAddressOpcode : CheatOpcode
    {
        BitWidthType BitWidth;
        uint RegisterToWrite;
        uint RegisterBase;
        bool IncrementFlag;
        uint OffsetType;
        uint RegIndex1;
        UInt64 Value;

        public StoreRegisterToAddressOpcode() { }

        public StoreRegisterToAddressOpcode(uint[] blocks)
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            RegisterToWrite = GetNibble(blocks[0], 3);
            RegisterBase = GetNibble(blocks[0], 4);
            IncrementFlag = GetNibble(blocks[0], 5) == 1;
            OffsetType = GetNibble(blocks[0], 6);
            if (OffsetType == 1)
            {
                RegIndex1 = GetNibble(blocks[0], 7);
            } else if (OffsetType == 2) {

                if (BitWidth == BitWidthType.b)
                {
                    Value = blocks[0] & 0xF;
                } else
                {
                    Value = ((UInt64)(blocks[0] & 0xF) << 32) + blocks[1];
                }
            }
        }

        public override string ToASM()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("mov").Append(Enum.GetName(typeof(BitWidthType), BitWidth));

            sb.Append(" [R").Append(RegisterBase.ToString("X"));
            switch (OffsetType)
            {
                case 0:
                    sb.Append("], ");
                    break;
                case 1:
                    sb.Append(" + R").Append(RegIndex1.ToString("X")).Append("], ");
                    break;
                case 2:
                    sb.Append("+0x").Append(Value.ToString("x")).Append("], ");
                    break;
            }

            sb.Append("R").Append(RegisterToWrite.ToString("X")).Append(" ");

            if (IncrementFlag)
            {
                sb.Append(" inc");
            }
            return sb.ToString();
        }
    }
}
