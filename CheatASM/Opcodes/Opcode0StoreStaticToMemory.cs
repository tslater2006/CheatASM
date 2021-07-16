using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class Opcode0StoreStaticToMemory : CheatOpcode
    {
        public BitWidthType BitWidth;
        public MemoryAccessType MemType;
        public uint OffsetRegister;
        public ulong RelativeOffset;
        public UInt64 Value;

        public Opcode0StoreStaticToMemory() { }

        public Opcode0StoreStaticToMemory(UInt32[] blocks)
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            MemType = (MemoryAccessType)GetNibble(blocks[0], 3);
            OffsetRegister = GetNibble(blocks[0], 4);
            RelativeOffset = blocks[1];
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

            sb.Append(" + R").Append(OffsetRegister.ToString("X"));
            if (RelativeOffset > 0) { sb.Append(" + 0x").Append(RelativeOffset.ToString("X")); }
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
            SetNibble(ref blocks[0], 7, (uint)((RelativeOffset >> 36) & 0xF));
            SetNibble(ref blocks[0], 7, (uint)((RelativeOffset >> 32) & 0xF));
            blocks[1] = (uint)(RelativeOffset & 0xFFFFFFFF);
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
