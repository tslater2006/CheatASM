using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class LegacyArithmeticOpcode : CheatOpcode
    {
        public BitWidthType BitWidth;
        public uint RegisterIndex;
        public RegisterArithmeticType MathType;
        public uint Value;

        public LegacyArithmeticOpcode() { }

        public LegacyArithmeticOpcode(uint[] blocks)
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            RegisterIndex = GetNibble(blocks[0], 4);
            MathType = (RegisterArithmeticType)GetNibble(blocks[0], 5);
            Value = blocks[1];
        }

        public override string ToASM()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Enum.GetName(typeof(RegisterArithmeticType), MathType));
            sb.Append(".").Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" R").Append(RegisterIndex.ToString("X"));
            sb.Append(", 0x").Append(Value.ToString("X"));

            return sb.ToString();
        }

        public override string ToByteString()
        {
            /* 7T0RC000 VVVVVVVV */
            uint[] blocks = new uint[2];
            SetNibble(ref blocks[0], 1, 7);
            SetNibble(ref blocks[0], 2, ((uint)BitWidth & 0xF));
            SetNibble(ref blocks[0], 3, 0);
            SetNibble(ref blocks[0], 4, ((uint)RegisterIndex & 0xF));
            SetNibble(ref blocks[0], 5, ((uint)MathType & 0xF));
            SetNibble(ref blocks[0], 6, 0);
            SetNibble(ref blocks[0], 7, 0);
            SetNibble(ref blocks[0], 8, 0);

            blocks[1] = (uint)Value;

            return GetBlocksAsString(blocks);
        }
    }
}
