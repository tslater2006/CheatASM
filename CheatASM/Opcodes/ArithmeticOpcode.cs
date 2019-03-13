using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class ArithmeticOpcode : CheatOpcode
    {
        public BitWidthType BitWidth;
        public RegisterArithmeticType MathType;
        public uint RegisterDest;
        public uint RegisterLeft;
        public uint RegisterRight;
        public ulong Value;
        public bool RightHandRegister;
        public ArithmeticOpcode() { }

        public ArithmeticOpcode(uint[] blocks)
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            MathType = (RegisterArithmeticType)GetNibble(blocks[0], 3);
            RegisterDest = GetNibble(blocks[0], 4);
            RegisterLeft = GetNibble(blocks[0], 5);

            if (GetNibble(blocks[0], 6) == 0)
            {
                RightHandRegister = true;
                RegisterRight = GetNibble(blocks[0], 7);
            }
            else
            {
                RightHandRegister = false;
                if (BitWidth == BitWidthType.q)
                {
                    Value = ((UInt64)blocks[2] << 32) | blocks[3];
                }
                else
                {
                    Value = blocks[2];
                }
            }
        }

        public override string ToASM()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Enum.GetName(typeof(RegisterArithmeticType), MathType));
            sb.Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" R").Append(RegisterDest.ToString("X"));
            sb.Append(", R").Append(RegisterLeft.ToString("X"));
            if (RightHandRegister)
            {
                sb.Append(", R").Append(RegisterRight.ToString("X"));
            } else
            {
                sb.Append(", 0x").Append(Value.ToString("x"));
            }

            return sb.ToString();
        }

        public override string ToByteString()
        {
            throw new NotImplementedException();
        }
    }
}
