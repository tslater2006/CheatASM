using System;
using System.Text;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class StoreStaticToAddressOpcode : CheatOpcode
    {
        /* 6T0RIor0 VVVVVVVV VVVVVVVV

    T: width of memory write (1, 2, 4, or 8 bytes)
    R: Register used as base memory address.
    I: Increment register flag (0 = do not increment R, 1 = increment R by T).
    o: Offset register enable flag (0 = do not add r to address, 1 = add r to address).
    r: Register used as offset when o is 1.
    V: Value to write to memory.
 */
        BitWidthType BitWidth;
        uint RegisterIndex;
        bool IncrementFlag;
        bool OffsetEnableFlag;
        uint OffsetRegister;
        UInt64 Value;

        public StoreStaticToAddressOpcode(UInt32[] blocks) : base(blocks[0])
        {
            BitWidth = (BitWidthType)GetNibble(blocks[0], 2);
            RegisterIndex = GetNibble(blocks[0], 4);
            IncrementFlag = GetNibble(blocks[0], 5) == 1;
            OffsetEnableFlag = GetNibble(blocks[0], 6) == 1;
            OffsetRegister = GetNibble(blocks[0], 7);
            Value = (blocks[1] << 32) + blocks[2];
        }

        public override string ToASM()
        {
            /* mov(b/w/d/q) [R0 (+R2)] Value (inc) */
            StringBuilder sb = new StringBuilder();

            sb.Append("mov").Append(Enum.GetName(typeof(BitWidthType), BitWidth));
            sb.Append(" [R").Append(RegisterIndex.ToString("X"));
            if (OffsetEnableFlag)
            {
                sb.Append("+").Append("R" + OffsetRegister.ToString("X"));
            }
            sb.Append("], 0x").Append(Value.ToString("x"));
            if (IncrementFlag)
            {
                sb.Append(" inc");
            }

            return sb.ToString();
        }
    }
}
