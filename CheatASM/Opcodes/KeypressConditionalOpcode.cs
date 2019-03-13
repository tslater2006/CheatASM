using System;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class KeypressConditionalOpcode : CheatOpcode
    {
        KeyMask Mask;
        public KeypressConditionalOpcode(UInt32[] blocks) : base(blocks[0])
        {
            Mask = (KeyMask)(blocks[0] & (0xFFFFFFF));
        }

        public override string ToASM()
        {
            return "keycheck " + Enum.GetName(typeof(KeyMask),Mask);
        }
    }
}
