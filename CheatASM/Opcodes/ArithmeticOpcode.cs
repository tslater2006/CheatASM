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
                    Value = ((UInt64)blocks[1] << 32) | blocks[2];
                }
                else
                {
                    Value = blocks[1];
                }
            }
        }

        public override string ToASM()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Enum.GetName(typeof(RegisterArithmeticType), MathType));
            sb.Append(".").Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" R").Append(RegisterDest.ToString("X"));
            sb.Append(", R").Append(RegisterLeft.ToString("X"));
            if (RightHandRegister)
            {
                sb.Append(", R").Append(RegisterRight.ToString("X"));
            } else
            {
                sb.Append(", 0x").Append(Value.ToString("X"));
            }

            return sb.ToString();
        }

        public override string ToByteString()
        {
            uint[] blocks = null;
            if (BitWidth == BitWidthType.q)
            {
                blocks = new uint[4];
            } else
            {
                blocks = new uint[3];
            }

            SetNibble(ref blocks[0], 1, 9);
            SetNibble(ref blocks[0], 2, ((uint)BitWidth & 0xF));
            SetNibble(ref blocks[0], 3, ((uint)MathType & 0xF));
            SetNibble(ref blocks[0], 4, ((uint)RegisterDest & 0xF));
            SetNibble(ref blocks[0], 5, ((uint)RegisterLeft & 0xF));
            if (RightHandRegister)
            {
                SetNibble(ref blocks[0], 6, 0);
                SetNibble(ref blocks[0], 7, ((uint)RegisterRight & 0xF));
            } else
            {
                SetNibble(ref blocks[0], 6, 1);
                SetNibble(ref blocks[0], 7, 0);
            }
            SetNibble(ref blocks[0], 8, 0);

            if (!RightHandRegister)
            {
                if (BitWidth == BitWidthType.q)
                {
                    blocks[1] = (uint)(Value >> 32);
                    blocks[2] = (uint)(Value & 0xFFFFFFFF);
                } else
                {
                    blocks[1] = (UInt32)Value;
                }
            }

            return GetBlocksAsString(blocks);
        }
    }
}
