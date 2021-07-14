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
        public MemoryAccessType MemoryType;
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
                    MemoryType = (MemoryAccessType)GetNibble(blocks[0], 7);
                    RelativeAddress = (((blocks[0] & 0xF) << 32) + blocks[1]);
                    break;
                case 1:
                    MemoryType = (MemoryAccessType)GetNibble(blocks[0], 7);
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
            }
        }
        public override string ToASM()
        {
            switch (OperandType)
            {
                case 0:
                    return $"log.{Enum.GetName(typeof(BitWidthType), BitWidth)} 0x{LogId:X}, [{Enum.GetName(typeof(MemoryAccessType), MemoryType)} + 0x{RelativeAddress:X}]";
                case 1:
                    return $"log.{Enum.GetName(typeof(BitWidthType), BitWidth)} 0x{LogId:X}, [{Enum.GetName(typeof(MemoryAccessType), MemoryType)} + R{OffsetRegister:X}]";
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
            throw new NotImplementedException();
        }
    }
}
