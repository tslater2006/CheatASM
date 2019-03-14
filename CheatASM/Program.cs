using Mono.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CheatASM
{
    class Program
    {
        static void Main(string[] args)
        {
            bool help = false;
            string inputPath = "";
            string outputPath = "";
            bool disassemble = false;
            bool assemble = false;
            bool recursive = false;
            bool verbose = false;
            OptionSet optSet = new OptionSet();
            optSet.Add("?|help|h", "Prints out the options.", option => help = option != null);
            optSet.Add("d|disassemble", "Disassembler mode.", option => disassemble = option != null);
            optSet.Add("a|assemble", "Assembler mode.", option => assemble = option != null);
            optSet.Add("i=|in=", "Input File or Directory.", option => inputPath = option);
            optSet.Add("o=|out=", "Output File or Directory.", option => outputPath = option);
            optSet.Add("r|recursive", "Process directory recursively.", option => recursive = option != null);
            optSet.Add("v|verbose", "Verbose Logging.", option => verbose = option != null);

            optSet.Parse(args);

            if (assemble && disassemble)
            {
                Console.Error.WriteLine("You cannot specifiy both assembler and disassembler modes simultaneously.");
                return;
            }

            if (help)
            {
                Console.WriteLine("Usage: CheatASM -d/a -in FILE -out FILE");
                optSet.WriteOptionDescriptions(Console.Error);
            }

            /* ensure input exists and determine if it is a directory or not */
            bool isInputDir = false;
            if (Directory.Exists(inputPath))
            {
                isInputDir = true;
                /* if you specified an input directory you must specifiy an output */
                if (outputPath == "")
                {
                    Console.Error.WriteLine("When processing a directoy an output directory *must* be specified.");
                    return;
                }
            }
            else
            {
                if (File.Exists(inputPath))
                {
                    isInputDir = false;
                }
                else
                {
                    /* input path isn't an existing file or directory */
                    Console.Error.WriteLine("Unable to find the input path specified.");
                    return;
                }
            }

            /* at this point we know the inputPath exists, and if its a folder or not */

            Assembler asm = new Assembler();
            Disassembler disasm = new Disassembler();

            if (isInputDir)
            {
                string[] fileList = Directory.GetFiles(inputPath, "*.txt", new EnumerationOptions() { RecurseSubdirectories = recursive });

                foreach (var file in fileList)
                {
                    var relativePath = file.Replace(inputPath, "");

                    /* make sure folder exists */
                    var newFolderPath = outputPath + relativePath.Substring(0,relativePath.LastIndexOf(Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(newFolderPath);
                    if (verbose)
                    {
                        Console.WriteLine("Saving " + outputPath + relativePath + "...");
                    }
                    if (assemble)
                    {
                        File.WriteAllText(outputPath + relativePath, asm.AssembleFile(file));
                    } else
                    {
                        File.WriteAllText(outputPath + relativePath, disasm.DisassembleFile(file));
                    }
                }
                Console.WriteLine("Processed " + fileList.Length + " files.");
            }
            else
            {
                /* dealing with a single file */
                if (assemble)
                {
                    if (outputPath != "")
                    {
                        File.WriteAllText(outputPath, asm.AssembleFile(inputPath));
                    }
                    else
                    {
                        Console.Write(asm.AssembleFile(inputPath));
                    }
                }else
                {
                    if (outputPath != "")
                    {
                        File.WriteAllText(outputPath, asm.AssembleFile(inputPath));
                    }
                    else
                    {
                        Console.Write(disasm.DisassembleFile(inputPath));
                    }
                }


            }

        }

    }

}
