using System;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class Opcode8KeypressConditional : CheatOpcode
    {
        public KeyMask Mask;
        public Opcode8KeypressConditional() { }
        public Opcode8KeypressConditional(UInt32[] blocks)
        {
            Mask = (KeyMask)(blocks[0] & (0xFFFFFFF));
        }

        public override string ToASM()
        {
            return "keycheck " + Enum.GetName(typeof(KeyMask),Mask);
        }

        public override string ToByteString()
        {
            uint[] blocks = new uint[1];
            SetNibble(ref blocks[0], 1, 8);

            var byteMask = ((uint)Mask & 0x0FFFFFFF);
            blocks[0] |= byteMask;

            return GetBlocksAsString(blocks);
        }
    }
}
