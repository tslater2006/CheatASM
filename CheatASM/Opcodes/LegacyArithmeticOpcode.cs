using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class LegacyArithmeticOpcode : CheatOpcode
    {
        BitWidthType BitWidth;
        uint RegisterIndex;
        RegisterArithmeticType MathType;
        uint Value;

        public LegacyArithmeticOpcode(uint[] blocks) : base(blocks[0])
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
            sb.Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" R").Append(RegisterIndex.ToString("X"));
            sb.Append(", 0x").Append(Value.ToString("x"));

            return sb.ToString();
        }
    }
}
