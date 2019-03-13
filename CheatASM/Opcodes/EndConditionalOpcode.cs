/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class EndConditionalOpcode : CheatOpcode
    {
        public EndConditionalOpcode() { }

        public EndConditionalOpcode(uint[] blocks) { }

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
