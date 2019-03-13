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
            var fileList = Directory.GetFiles(@"Cheats", "*.txt",new EnumerationOptions() { RecurseSubdirectories = true });
            Disassembler d = new Disassembler();
            Assembler a = new Assembler();
            var correct = 0;
            var wrong = 0;
            foreach (var file in fileList)
            {
                var lines = File.ReadAllLines(file);
                
                foreach (var line in lines)
                {
                    if (line.StartsWith("[") == false && line.StartsWith("{") == false && line.Trim().Length > 0)
                    {
                        /* assume opcode */
                        var disassembled = d.DisassembleLine(line);
                        if (disassembled.StartsWith("#"))
                        {
                            continue;
                        } else
                        {
                            var assembled = a.AssembleLine(disassembled);
                            if (assembled.Trim().Equals(line.ToUpper().Trim()) == false)
                            {
                                wrong++;
                            } else
                            {
                                correct++;
                            }
                        }
                    }
                }
            }
            Console.Write("Assembler test: " + correct + " out of " + (correct + wrong) + " correct (missed " + wrong + ")!");
            return;
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

            Console.ReadKey();
        }

    }

}
