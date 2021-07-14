using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheatASM
{
    class OpcodeC3ReadWriteStaticReg : CheatOpcode
    {
        public uint RegIndex;
        public uint StaticRegIndex;
        public bool WriteMode = false;

        public OpcodeC3ReadWriteStaticReg() { }
        public OpcodeC3ReadWriteStaticReg(uint[] blocks)
        {
            RegIndex = blocks[0] & 0xF;
            StaticRegIndex = (blocks[0] >> 4) & 0x7F;
            WriteMode = (((blocks[0] >> 4) & 0x80) == 0x80);
        }
        public override string ToASM()
        {
            if (WriteMode)
            {
                return $"save.static SR{StaticRegIndex:X}, R{RegIndex:X}";
            } else
            {
                return $"load.static R{RegIndex:X}, SR{StaticRegIndex:X}";
            }
        }

        public override string ToByteString()
        {
            uint[] blocks = new uint[1];
            SetNibble(ref blocks[0], 1, 0xC);
            SetNibble(ref blocks[0], 2, 0x3);

            blocks[0] |= ((StaticRegIndex | (uint)(WriteMode ? 0x80 : 0x0)) << 4);
            blocks[0] |= RegIndex;

            return GetBlocksAsString(blocks);
        }
    }
}
