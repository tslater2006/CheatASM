using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CheatASM
{
    class Program
    {
        static void Main(string[] args)
        {
            /* currently we only support 1 file on the args, this will be improved over time */
            if (args.Length < 1)
            {
                Console.WriteLine("Please specify a cheat file to disassemble.");
                return;
            }

            if (File.Exists(args[0]) == false)
            {
                Console.WriteLine("Please specify an existing cheat file to disassemble.");
                return;
            }

            /* currently only the disassembler is working, will become a flag in the future */
            bool disassemble = false;

            if (disassemble)
            {
                Disassembler d = new Disassembler();
                
                var text = d.DisassembleFile(args[0]);
                Console.Write(text);
            } else
            {
                Assembler a = new Assembler();
                a.AssembleLine("movq [HEAP+R0+0x29af1388] 0x303030303030303");
            }
            Console.ReadKey();
        }

    }

}
