using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class StoreRegisterToAddressOpcode : CheatOpcode
    {
        public BitWidthType BitWidth;
        public MemoryAccessType MemType;
        public uint SourceRegister;
        public uint AddressRegister;
        public bool IncrementFlag;
        public uint OffsetType;
        public uint OffsetRegister;
        public ulong RelativeAddress;

        public StoreRegisterToAddressOpcode() { }

        public StoreRegisterToAddressOpcode(uint[] blocks)
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            SourceRegister = GetNibble(blocks[0], 3);
            AddressRegister = GetNibble(blocks[0], 4);
            IncrementFlag = GetNibble(blocks[0], 5) == 1;
            OffsetType = GetNibble(blocks[0], 6);
            switch(OffsetType)
            {
                case 1:
                    OffsetRegister = GetNibble(blocks[0], 7);
                    break;
                case 2:
                    RelativeAddress = ((UInt64)(blocks[0] & 0xF) << 32) + blocks[1];
                    break;
                case 3:
                    MemType = (MemoryAccessType)GetNibble(blocks[0], 7);
                    break;
                case 4:
                    MemType = (MemoryAccessType)GetNibble(blocks[0], 7);
                    RelativeAddress = ((UInt64)(blocks[0] & 0xF) << 32) + blocks[1];
                    break;
                case 5:
                    MemType = (MemoryAccessType)GetNibble(blocks[0], 7);
                    RelativeAddress = ((UInt64)(blocks[0] & 0xF) << 32) + blocks[1];
                    break;
            }
            
        }

        public override string ToASM()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("mov.").Append(Enum.GetName(typeof(BitWidthType), BitWidth));

            switch (OffsetType)
            {
                case 0:
                    sb.Append(" [R").Append(AddressRegister.ToString("X"));
                    sb.Append("], ");
                    break;
                case 1:
                    sb.Append(" [R").Append(AddressRegister.ToString("X"));
                    sb.Append("+R").Append(OffsetRegister.ToString("X")).Append("], ");
                    break;
                case 2:
                    sb.Append(" [R").Append(AddressRegister.ToString("X"));
                    sb.Append("+0x").Append(RelativeAddress.ToString("X")).Append("], ");
                    break;
                case 3:
                    sb.Append(" [").Append(Enum.GetName(typeof(MemoryAccessType),MemType));
                    sb.Append("+R").Append(AddressRegister.ToString("X")).Append("], ");
                    break;
                case 4:
                    sb.Append(" [").Append(Enum.GetName(typeof(MemoryAccessType), MemType));
                    sb.Append("+0x").Append(RelativeAddress.ToString("X")).Append("], ");
                    break;
                case 5:
                    sb.Append(" [").Append(Enum.GetName(typeof(MemoryAccessType), MemType));
                    sb.Append("+R").Append(AddressRegister.ToString("X"));
                    sb.Append("+0x").Append(RelativeAddress.ToString("X")).Append("], "); ;
                    break;
            }

            sb.Append("R").Append(SourceRegister.ToString("X")).Append(" ");

            if (IncrementFlag)
            {
                sb.Append(" inc");
            }
            return sb.ToString();
        }

        public override string ToByteString()
        {
            uint[] blocks = null;

            switch (OffsetType)
            {
                case 0:
                    blocks = new uint[1];
                    break;
                case 1:
                    blocks = new uint[1];
                    break;
                case 2:
                    blocks = new uint[2];
                    break;
                case 3:
                    blocks = new uint[1];
                    break;
                case 4:
                    blocks = new uint[2];
                    break;
                case 5:
                    blocks = new uint[2];
                    break;
            }

            SetNibble(ref blocks[0], 1, 0xA);
            SetNibble(ref blocks[0], 2, (uint)BitWidth & 0xF);
            SetNibble(ref blocks[0], 3, (uint)SourceRegister & 0xF);
            SetNibble(ref blocks[0], 4, (uint)AddressRegister & 0xF);

            if (IncrementFlag)
            {
                SetNibble(ref blocks[0], 5, 1);
            } else
            {
                SetNibble(ref blocks[0], 5, 0);
            }

            SetNibble(ref blocks[0], 6, (uint)OffsetType&0xF);

            switch (OffsetType)
            {
                case 0:
                    SetNibble(ref blocks[0], 7, 0);
                    SetNibble(ref blocks[0], 8, 0);
                    break;
                case 1:
                    SetNibble(ref blocks[0], 7, (uint)OffsetRegister & 0xF);
                    SetNibble(ref blocks[0], 8, 0);
                    break;
                case 2:
                    SetNibble(ref blocks[0], 8, (uint)(RelativeAddress >> 32) & 0xF);
                    blocks[1] = (uint)(RelativeAddress & 0xFFFFFFFF);
                    break;
                case 3:
                    SetNibble(ref blocks[0], 7, (uint)MemType & 0xF);
                    SetNibble(ref blocks[0], 8, 0);
                    break;
                case 4:
                    SetNibble(ref blocks[0], 7, (uint)MemType & 0xF);
                    SetNibble(ref blocks[0], 8, (uint)(RelativeAddress >> 32) & 0xF);
                    blocks[1] = (uint)(RelativeAddress & 0xFFFFFFFF);
                    break;
                case 5:
                    SetNibble(ref blocks[0], 7, (uint)MemType & 0xF);
                    SetNibble(ref blocks[0], 8, (uint)(RelativeAddress >> 32) & 0xF);
                    blocks[1] = (uint)(RelativeAddress & 0xFFFFFFFF);
                    break;
            }
            return GetBlocksAsString(blocks);
        }
    }
}
