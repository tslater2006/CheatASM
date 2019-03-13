/* The types located in this file are based of the Atmosphère cheat implementation: 
   https://github.com/Atmosphere-NX/Atmosphere/blob/master/stratosphere/dmnt/source/dmnt_cheat_vm.hpp
*/

namespace CheatASM
{
    public class EndConditionalOpcode : CheatOpcode
    {
        public EndConditionalOpcode(uint[] blocks) : base(blocks[0]) { }

        public override string ToASM()
        {
            return "endcond";
        }
    }
}
