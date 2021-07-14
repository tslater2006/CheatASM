using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheatASM
{
    class SaveRestoreRegisterOpcode : CheatOpcode
    {
        public SaveRestoreRegisterOpcode() { }
        public SaveRestoreRegisterOpcode(uint[] blocks)
        {
            DestinationIndex = GetNibble(blocks[0], 4);
            SourceIndex = GetNibble(blocks[0], 6);
            OperandType = GetNibble(blocks[0], 7);
        }

        public uint DestinationIndex;
        public uint SourceIndex;
        public uint OperandType;

        public override string ToASM()
        {
            switch(OperandType)
            {
                case 0:
                    return $"load.reg R{DestinationIndex.ToString("X")}, 0x{SourceIndex.ToString("X")}";
                case 1:
                    return $"save.reg 0x{DestinationIndex.ToString("X")}, R{SourceIndex.ToString("X")}";
                case 2:
                    return $"clear.saved 0x{DestinationIndex.ToString("X")}";
                case 3:
                    return $"clear.reg R{DestinationIndex.ToString("X")}";
                default:
                    return "Error printing SaveResotreRegisterOpcode";
            }
        }

        public override string ToByteString()
        {
            uint[] blocks = new uint[1];
            SetNibble(ref blocks[0], 1, 0xC);
            SetNibble(ref blocks[0], 2, 0x1);
            SetNibble(ref blocks[0], 3, 0x0);
            SetNibble(ref blocks[0], 4, (DestinationIndex & 0xF));
            SetNibble(ref blocks[0], 5, 0);
            SetNibble(ref blocks[0], 6, (SourceIndex & 0xF));
            SetNibble(ref blocks[0], 7, (OperandType & 0xF));
            SetNibble(ref blocks[0], 8, 0);

            return GetBlocksAsString(blocks);
        }
    }
}
