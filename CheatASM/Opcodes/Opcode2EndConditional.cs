/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class Opcode2EndConditional : CheatOpcode
    {
        public Opcode2EndConditional() { }

        public Opcode2EndConditional(uint[] blocks) { }

        public override string ToASM()
        {
            return "endcond";
        }

        public override string ToByteString()
        {
            return "20000000";
        }
    }
}
