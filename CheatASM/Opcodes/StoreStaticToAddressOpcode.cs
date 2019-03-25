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

            sb.Append("mov.").Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" [R").Append(RegisterIndex.ToString("X"));
            if (OffsetEnableFlag)
            {
                sb.Append("+").Append("R" + OffsetRegister.ToString("X"));
            }
            sb.Append("], 0x").Append(Value.ToString("X"));
            if (IncrementFlag)
            {
                sb.Append(" inc");
            }

            return sb.ToString();
        }

        public override string ToByteString()
        {
            /* 6T0RIor0 VVVVVVVV VVVVVVVV */

            uint[] blocks = new uint[3];
            SetNibble(ref blocks[0], 1, 6);
            SetNibble(ref blocks[0], 2, (uint)BitWidth & 0xF);
            SetNibble(ref blocks[0], 3, 0);
            SetNibble(ref blocks[0], 4, (uint)RegisterIndex & 0xF);

            if (IncrementFlag)
            {
                SetNibble(ref blocks[0], 5, 1);
            } else
            {
                SetNibble(ref blocks[0], 5, 0);
            }

            if (OffsetEnableFlag)
            {
                SetNibble(ref blocks[0], 6, 1);
            }
            else
            {
                SetNibble(ref blocks[0], 6, 0);
            }

            SetNibble(ref blocks[0], 7, (uint)OffsetRegister & 0xF);
            SetNibble(ref blocks[0], 8, 0);

            blocks[1] = (uint)((Value >> 32) & 0xFFFFFFFF);
            blocks[2] = (uint)(Value & 0xFFFFFFFF);

            return GetBlocksAsString(blocks);
        }
    }
}
