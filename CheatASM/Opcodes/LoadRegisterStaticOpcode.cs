using System;

/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class LoadRegisterStaticOpcode : CheatOpcode
    {
        uint RegisterIndex;
        UInt64 Value;

        public LoadRegisterStaticOpcode(uint[] blocks) : base(blocks[0])
        {
            RegisterIndex = GetNibble(blocks[0], 4);
            Value = ((UInt64)blocks[1] << 32) + blocks[2];
        }

        public override string ToASM()
        {
            return "movq R" + RegisterIndex.ToString("X") + ", 0x" + Value.ToString("x");
        }
    }
}
