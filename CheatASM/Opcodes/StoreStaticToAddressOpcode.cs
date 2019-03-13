using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class StoreStaticToAddressOpcode : CheatOpcode
    {
        public BitWidthType BitWidth;
        public uint RegisterIndex;
        public bool IncrementFlag;
        public bool OffsetEnableFlag;
        public uint OffsetRegister;
        public UInt64 Value;

        public StoreStaticToAddressOpcode() { }

        public StoreStaticToAddressOpcode(UInt32[] blocks)
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            RegisterIndex = GetNibble(blocks[0], 4);
            IncrementFlag = GetNibble(blocks[0], 5) == 1;
            OffsetEnableFlag = GetNibble(blocks[0], 6) == 1;
            OffsetRegister = GetNibble(blocks[0], 7);
            Value = ((UInt64)blocks[1] << 32) + blocks[2];
        }

        public override string ToASM()
        {
            /* mov(b/w/d/q) [R0 (+R2)] Value (inc) */
            StringBuilder sb = new StringBuilder();

            sb.Append("mov").Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" [R").Append(RegisterIndex.ToString("X"));
            if (OffsetEnableFlag)
            {
                sb.Append("+").Append("R" + OffsetRegister.ToString("X"));
            }
            sb.Append("], 0x").Append(Value.ToString("x"));
            if (IncrementFlag)
            {
                sb.Append(" inc");
            }

            return sb.ToString();
        }

        public override string ToByteString()
        {
            throw new NotImplementedException();
        }
    }
}
