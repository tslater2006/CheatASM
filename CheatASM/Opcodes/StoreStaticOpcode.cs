using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class StoreStaticOpcode : CheatOpcode
    {
        public BitWidthType BitWidth;
        public MemoryAccessType MemType;
        public uint OffsetRegister;
        public ulong RelativeAddress;
        public UInt64 Value;

        public StoreStaticOpcode() { }

        public StoreStaticOpcode(UInt32[] blocks)
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
            sb.Append(".").Append(Enum.GetName(typeof(BitWidthType), BitWidth));

            sb.Append(" [");

            sb.Append(Enum.GetName(typeof(MemoryAccessType), MemType));

            sb.Append("+R").Append(OffsetRegister.ToString("X")).Append("+0x");
            sb.Append(RelativeAddress.ToString("X"));
            sb.Append("], 0x").Append(Value.ToString("X"));
            return sb.ToString();
        }

        public override string ToByteString()
        {
            /* 0TMR00AA AAAAAAAA VVVVVVVV (VVVVVVVV) */
            uint[] blocks = null;
            if (BitWidth == BitWidthType.q)
            {
                blocks = new uint[4];
            } else
            {
                blocks = new uint[3];
            }

            /* build first DWORD */
            SetNibble(ref blocks[0], 1, 0);
            SetNibble(ref blocks[0], 2, (uint)BitWidth);
            SetNibble(ref blocks[0], 3, (uint)MemType);
            SetNibble(ref blocks[0], 4, (uint)OffsetRegister);
            SetNibble(ref blocks[0], 5, 0);
            SetNibble(ref blocks[0], 6, 0);
            SetNibble(ref blocks[0], 7, (uint)((RelativeAddress >> 36) & 0xF));
            SetNibble(ref blocks[0], 7, (uint)((RelativeAddress >> 32) & 0xF));
            blocks[1] = (uint)(RelativeAddress & 0xFFFFFFFF);
            if (BitWidth == BitWidthType.q)
            {
                blocks[2] = (uint)(Value >> 32);
                blocks[3] = (uint)(Value & 0xFFFFFFFF);
            }
            else
            {
                blocks[2] = (uint)(Value & 0xFFFFFFFF);
            }

            return GetBlocksAsString(blocks);
        }
    }
}
