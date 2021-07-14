using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheatASM
{
    class OpcodeFFFDebugLog : CheatOpcode
    {
        public BitWidthType BitWidth;
        public uint LogId;
        public uint OperandType;
        public MemoryAccessType MemType;
        public uint AddressRegister;
        public ulong RelativeAddress;
        public uint OffsetRegister;
        public uint ValueRegister;

        public OpcodeFFFDebugLog() { }
        public OpcodeFFFDebugLog(uint[] blocks)
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 4);
            LogId = GetNibble(blocks[0], 5);
            OperandType = GetNibble(blocks[0], 6);

            switch(OperandType)
            {
                case 0:
                    MemType = (MemoryAccessType)GetNibble(blocks[0], 7);
                    RelativeAddress = (((ulong)(blocks[0] & 0xF) << 32) + blocks[1]);
                    break;
                case 1:
                    MemType = (MemoryAccessType)GetNibble(blocks[0], 7);
                    OffsetRegister = GetNibble(blocks[0], 8);
                    break;
                case 2:
                    AddressRegister = GetNibble(blocks[0], 7);
                    RelativeAddress = (((ulong)(blocks[0] & 0xF) << 32) + blocks[1]);
                    break;
                case 3:
                    AddressRegister = GetNibble(blocks[0], 7);
                    OffsetRegister = GetNibble(blocks[0], 8);
                    break;
                case 4:
                    ValueRegister = GetNibble(blocks[0], 7);
                    break;
            }
        }
        public override string ToASM()
        {
            switch (OperandType)
            {
                case 0:
                    return $"log.{Enum.GetName(typeof(BitWidthType), BitWidth)} 0x{LogId:X}, [{Enum.GetName(typeof(MemoryAccessType), MemType)} + 0x{RelativeAddress:X}]";
                case 1:
                    return $"log.{Enum.GetName(typeof(BitWidthType), BitWidth)} 0x{LogId:X}, [{Enum.GetName(typeof(MemoryAccessType), MemType)} + R{OffsetRegister:X}]";
                case 2:
                    return $"log.{Enum.GetName(typeof(BitWidthType), BitWidth)} 0x{LogId:X}, [R{AddressRegister:X} + 0x{RelativeAddress:X}]";
                case 3:
                    return $"log.{Enum.GetName(typeof(BitWidthType), BitWidth)} 0x{LogId:X}, [R{AddressRegister:X} + R{OffsetRegister:X}]";
                case 4:
                    return $"log.{Enum.GetName(typeof(BitWidthType), BitWidth)} 0x{LogId:X}, R{ValueRegister:X}";
            }
            throw new NotImplementedException();
        }

        public override string ToByteString()
        {
            /*  BitWidth = (BitWidthType)GetNibble(blocks[0], 4);
            LogId = GetNibble(blocks[0], 5);
            OperandType = GetNibble(blocks[0], 6);

            switch(OperandType)
            {
                case 0:
                    MemType = (MemoryAccessType)GetNibble(blocks[0], 7);
                    RelativeAddress = (((blocks[0] & 0xF) << 32) + blocks[1]);
                    break;
                case 1:
                    MemType = (MemoryAccessType)GetNibble(blocks[0], 7);
                    OffsetRegister = GetNibble(blocks[0], 8);
                    break;
                case 2:
                    AddressRegister = GetNibble(blocks[0], 7);
                    RelativeAddress = (((blocks[0] & 0xF) << 32) + blocks[1]);
                    break;
                case 3:
                    AddressRegister = GetNibble(blocks[0], 7);
                    OffsetRegister = GetNibble(blocks[0], 8);
                    break;
                case 4:
                    ValueRegister = GetNibble(blocks[0], 7);
                    break;
            } */

            uint[] blocks = null;
            switch(OperandType)
            {
                case 0:
                case 2:
                    blocks = new uint[2];
                    break;
                default:
                    blocks = new uint[1];
                    break;
            }
            SetNibble(ref blocks[0], 1, 0xF);
            SetNibble(ref blocks[0], 2, 0xF);
            SetNibble(ref blocks[0], 3, 0xF);
            SetNibble(ref blocks[0], 4, (uint)BitWidth);
            SetNibble(ref blocks[0], 5, LogId);
            SetNibble(ref blocks[0], 6, OperandType);

            switch(OperandType)
            {
                case 0:
                    SetNibble(ref blocks[0], 7, (uint)MemType);
                    SetNibble(ref blocks[0], 8, (uint)((RelativeAddress >> 32) & 0xF));
                    blocks[1] = (uint)(RelativeAddress & 0xFFFFFFFF);
                    break;
                case 1:
                    SetNibble(ref blocks[0], 7, (uint)MemType);
                    SetNibble(ref blocks[0], 8, OffsetRegister);
                    break;
                case 2:
                    SetNibble(ref blocks[0], 7, AddressRegister);
                    SetNibble(ref blocks[0], 8, (uint)((RelativeAddress >> 32) & 0xF));
                    blocks[1] = (uint)(RelativeAddress & 0xFFFFFFFF);
                    break;
                case 3:
                    SetNibble(ref blocks[0], 7, AddressRegister);
                    SetNibble(ref blocks[0], 8, OffsetRegister);
                    break;
                case 4:
                    SetNibble(ref blocks[0], 7, ValueRegister);
                    SetNibble(ref blocks[0], 8, 0);
                    break;
            }
            return GetBlocksAsString(blocks);
        }
    }
}
