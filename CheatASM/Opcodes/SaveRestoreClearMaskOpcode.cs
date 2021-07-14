using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheatASM
{
    class SaveRestoreClearMaskOpcode : CheatOpcode
    {
        public SaveRestoreClearMaskOpcode() { }
        public SaveRestoreClearMaskOpcode(uint[] blocks)
        {
            OperandType = GetNibble(blocks[0], 3);

            var maskBits = (blocks[0] & 0xFFFF);

            for(var x = 0x0; x <= 0xF; x++)
            {
                RegMask[x] = (maskBits % 2 == 1);
                maskBits >>= 1;
            }
        }

        public bool[] RegMask = new bool[16];
        public uint OperandType;

        public override string ToASM()
        {

            StringBuilder paramList = new();

            for(var x =0; x <= 0xF; x++)
            {
                if (RegMask[x])
                {
                    if (OperandType == 2)
                    {
                        paramList.Append($"0x{x:X}, ");
                    }
                    else
                    {
                        paramList.Append($"R{x:X}, ");
                    }
                }
            }

            paramList.Length -= 2;

            switch (OperandType)
            {
                case 0:
                    return $"load.regs {paramList}";
                case 1:
                    return $"save.regs {paramList}";
                case 2:
                    return $"clear.saved {paramList}";
                case 3:
                    return $"clear.regs {paramList}";
                default:
                    return "Error printing SaveResotreRegisterOpcode";
            }
        }

        public override string ToByteString()
        {
            uint[] blocks = new uint[1];
            SetNibble(ref blocks[0], 1, 0xC);
            SetNibble(ref blocks[0], 2, 0x2);
            SetNibble(ref blocks[0], 3, OperandType);
            SetNibble(ref blocks[0], 4, 0);

            uint maskValue = 0;
            for(var x = 0xF; x >= 0x0; x--)
            {
                maskValue <<= 1;
                if (RegMask[x])
                {
                    maskValue++;
                }
            }
            blocks[0] |= maskValue;
            

            return GetBlocksAsString(blocks);
        }
    }
}
