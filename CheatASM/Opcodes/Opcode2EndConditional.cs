/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class Opcode2EndConditional : CheatOpcode
    {
        public bool IsElse = false;
        public Opcode2EndConditional() { }

        public Opcode2EndConditional(uint[] blocks)
        {
            IsElse = ((blocks[0]>> 24) & 0xF) == 0x1;
        }

        public override string ToASM()
        {
            if (IsElse)
            {
                return "else";
            } else
            {
                return "endcond";
            }
            
        }

        public override string ToByteString()
        {
            if (IsElse)
            {
                return "21000000";
            }
            else
            {
                return "20000000";
            }
        }
    }
}
