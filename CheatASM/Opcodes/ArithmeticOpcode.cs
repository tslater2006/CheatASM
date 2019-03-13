using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class ArithmeticOpcode : CheatOpcode
    {
        BitWidthType BitWidth;
        RegisterArithmeticType MathType;
        uint RegisterDest;
        uint RegisterLeft;
        uint RegisterRight;

        public ArithmeticOpcode() { }

        public ArithmeticOpcode(uint[] blocks)
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            MathType = (RegisterArithmeticType)GetNibble(blocks[0], 3);
            RegisterDest = GetNibble(blocks[0], 4);
            RegisterLeft = GetNibble(blocks[0], 5);
            RegisterRight = GetNibble(blocks[0], 7);

        }

        public override string ToASM()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Enum.GetName(typeof(RegisterArithmeticType), MathType));
            sb.Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" R").Append(RegisterDest.ToString("X"));
            sb.Append(", R").Append(RegisterLeft.ToString("X"));
            sb.Append(", R").Append(RegisterRight.ToString("X"));

            return sb.ToString();
        }
    }
}
