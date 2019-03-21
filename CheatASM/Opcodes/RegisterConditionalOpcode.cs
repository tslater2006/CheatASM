using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class RegisterConditionalOpcode : CheatOpcode
    {
        public BitWidthType BitWidth;
        public ConditionalComparisonType Condition;
        public uint SourceRegister;
        public uint OperandType;
        public MemoryAccessType MemType;
        public ulong AddressRegister;
        public ulong RelativeAddress;
        public uint OffsetRegister;
        public uint OtherRegister;
        public ulong Value;

        public RegisterConditionalOpcode() { }

        public RegisterConditionalOpcode(uint[] blocks)
        {
            /* C0TcSX## */
            /* C0TcS0Ma aaaaaaaa */
            /* C0TcS1Mr */
            /* C0TcS2Ra aaaaaaaa */
            /* C0TcS3Rr */
            /* C0TcS400 VVVVVVVV (VVVVVVVV) */
            /* C0 = opcode 0xC0 */
            /* T = bit width */
            /* c = condition type. */
            /* S = source register. */
            /* X = value operand type, 0 = main/heap with relative offset, 1 = main/heap with offset register, */
            /*     2 = register with relative offset, 3 = register with offset register, 4 = static value. */
            /* M = memory type. */
            /* a = relative address. */
            /* r = offset register. */
            /* V = value */

            BitWidth = (BitWidthType)GetNibble(blocks[0], 3);
            Condition = (ConditionalComparisonType)GetNibble(blocks[0], 4);
            SourceRegister = GetNibble(blocks[0], 5);
            OperandType = GetNibble(blocks[0], 6);
            switch(OperandType)
            {
                case 0:
                    MemType = (MemoryAccessType)GetNibble(blocks[0], 7);
                    RelativeAddress = ((ulong)(blocks[0] & 0xF) << 32) | blocks[1];
                    break;
                case 1:
                    MemType = (MemoryAccessType)GetNibble(blocks[0], 7);
                    OffsetRegister = GetNibble(blocks[0], 8);
                    break;
                case 2:
                    AddressRegister = GetNibble(blocks[0], 7);
                    RelativeAddress = ((ulong)(blocks[0] & 0xF) << 32) | blocks[1];
                    break;
                case 3:
                    AddressRegister = GetNibble(blocks[0], 7);
                    OffsetRegister = GetNibble(blocks[0], 8);
                    break;
                case 4:
                    if (BitWidth == BitWidthType.q)
                    {
                        Value = (((ulong)blocks[1]) << 32) | blocks[2];
                    } else
                    {
                        Value = blocks[1];
                    }
                    break;
                case 5:
                    OtherRegister = GetNibble(blocks[0], 7);
                    break;
            }
        }

        public override string ToASM()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Enum.GetName(typeof(ConditionalComparisonType), Condition));
            sb.Append(".").Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" ");
            switch(OperandType)
            {
                case 0:
                    /* C0TcS0M: ltb R0, [MAIN + 0x1234] */
                    sb.Append("R").Append(SourceRegister.ToString("X")).Append(", [");
                    sb.Append(Enum.GetName(typeof(MemoryAccessType), MemType));
                    sb.Append("+0x").Append(RelativeAddress.ToString("X"));
                    sb.Append("]");
                    break;
                case 1:
                    /* C0TcS1Mr: ltb R0, [MAIN + R1]*/
                    sb.Append("R").Append(SourceRegister.ToString("X")).Append(", [");
                    sb.Append(Enum.GetName(typeof(MemoryAccessType), MemType));
                    sb.Append("+R").Append(OffsetRegister.ToString("X"));
                    sb.Append("]");
                    break;
                case 2:
                    /* C0TcS2R: ltb R0, [R1 + 0x1234] */
                    sb.Append("R").Append(SourceRegister.ToString("X")).Append(", [");
                    sb.Append("R").Append(AddressRegister.ToString("X"));
                    sb.Append("+0x").Append(RelativeAddress.ToString("X"));                    
                    sb.Append("]");
                    break;
                case 3:
                    /* C0TcS3Rr: ltb R0, [R1 + R2] */
                    sb.Append("R").Append(SourceRegister.ToString("X")).Append(", [");
                    sb.Append("R").Append(AddressRegister.ToString("X"));
                    sb.Append("+R").Append(OffsetRegister.ToString("X"));
                    sb.Append("]");
                    break;
                case 4:
                    /* C0TcS400: ltb R0, 0x1234 */
                    sb.Append("R").Append(SourceRegister.ToString("X")).Append(", 0x");
                    sb.Append(Value.ToString("X"));
                    break;
                case 5:
                    sb.Append("R").Append(SourceRegister.ToString("X")).Append(", R");
                    sb.Append(OtherRegister.ToString("X"));
                    break;
            }
            
            return sb.ToString();
        }

        public override string ToByteString()
        {
            /* C0TcSX## */
            /* C0TcS0Ma aaaaaaaa */
            /* C0TcS1Mr */
            /* C0TcS2Ra aaaaaaaa */
            /* C0TcS3Rr */
            /* C0TcS400 VVVVVVVV (VVVVVVVV) */
            /* C0 = opcode 0xC0 */
            /* T = bit width */
            /* c = condition type. */
            /* S = source register. */
            /* X = value operand type, 0 = main/heap with relative offset, 1 = main/heap with offset register, */
            /*     2 = register with relative offset, 3 = register with offset register, 4 = static value. */
            /* M = memory type. */
            /* a = relative address. */
            /* r = offset register. */
            /* V = value */

            uint[] blocks = null;
            if (OperandType == 0 || OperandType == 2)
            {
                blocks = new uint[2];
            } else if (OperandType == 4)
            {
                blocks = new uint[3];
            } else
            {
                blocks = new uint[1];
            }

            SetNibble(ref blocks[0], 1, 0xC);
            SetNibble(ref blocks[0], 2, 0x0);

            SetNibble(ref blocks[0], 3, (uint)BitWidth);
            SetNibble(ref blocks[0], 4, (uint)Condition);
            SetNibble(ref blocks[0], 5, (uint)SourceRegister);
            SetNibble(ref blocks[0], 6, (uint)OperandType);

            switch(OperandType)
            {
                case 0:
                    SetNibble(ref blocks[0], 7, (uint)MemType);
                    SetNibble(ref blocks[0], 8, (uint)(RelativeAddress >> 32) & 0xF);
                    blocks[1] = (uint)(RelativeAddress & 0xFFFFFFFF);
                    break;
                case 1:
                    SetNibble(ref blocks[0], 7, (uint)MemType);
                    SetNibble(ref blocks[0], 8, (uint)OffsetRegister);
                    break;
                case 2:
                    SetNibble(ref blocks[0], 7, (uint)OffsetRegister);
                    SetNibble(ref blocks[0], 8, (uint)(RelativeAddress >> 32) & 0xF);
                    blocks[1] = (uint)(RelativeAddress & 0xFFFFFFFF);
                    break;
                case 3:
                    SetNibble(ref blocks[0], 7, (uint)AddressRegister);
                    SetNibble(ref blocks[0], 8, (uint)OffsetRegister);
                    break;
                case 4:
                    SetNibble(ref blocks[0], 7, 0);
                    SetNibble(ref blocks[0], 8, 0);

                    if (BitWidth == BitWidthType.q)
                    {
                        blocks[1] = (uint)(Value >> 32);
                        blocks[2] = (uint)(Value & 0xFFFFFFFF);
                    }
                    else
                    {
                        blocks[1] = (uint)(Value & 0xFFFFFFFF);
                    }
                    break;
                case 5:
                    SetNibble(ref blocks[0], 7, (uint)OtherRegister);
                    SetNibble(ref blocks[0], 8, 0);
                    break;
            }


            return GetBlocksAsString(blocks);
        }
    }
}
