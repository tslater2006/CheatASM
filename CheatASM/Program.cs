using Mono.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CheatASM
{
    class Program
    {
        static void RunREPL()
        {
            Console.Clear();
            bool run = true;
            Console.WriteLine("CheatASM REPL Started.");
            Console.WriteLine("Enter blank line to execute, enter quit() to stop REPL.");
            List<string> replLines = new List<string>();
           
            while(run) {
                Console.Write(">> ");
                var line = Console.ReadLine();
                if (line.Length == 0)
                {
                    Disassembler disasm = new Disassembler();
                    Assembler asm = new Assembler();
                    /* process replLines */
                    foreach (var l in replLines)
                    {
                        if (l.Length == 0)
                        {
                            continue;
                        }
                        if (Regex.IsMatch(l, "^([0-9a-fA-F]{8}\\s?){1,4}$"))
                        {
                            Console.WriteLine(disasm.DisassembleLine(l));
                        }
                        else
                        {
                            if (l.Trim().StartsWith("["))
                            {
                                /* Cheat header probably? */
                                Console.WriteLine(l);
                            }
                            else
                            {
                                Console.WriteLine(asm.AssembleString(l));
                            }
                        }
                    }
                    replLines.Clear();
                }
                if (line.ToLower().Contains("quit"))
                {
                    run = false;
                    continue;
                } else
                {
                    /* just add to list to process, we'll handle what to do later */
                    replLines.Add(line.Trim());
                }
            }
        }

        static void Main(string[] args)
        {
            bool help = false;
            string inputPath = "";
            string outputPath = "";
            bool disassemble = false;
            bool assemble = false;
            bool recursive = false;
            bool verbose = false;
            string text = "";
            bool repl = false;
            OptionSet optSet = new OptionSet();
            optSet.Add("?|help|h", "Prints out the options.", option => help = option != null);
            optSet.Add("d|disassemble", "Disassembler mode.", option => disassemble = option != null);
            optSet.Add("a|assemble", "Assembler mode.", option => assemble = option != null);
            optSet.Add("i=|in=", "Input File or Directory.", option => inputPath = option);
            optSet.Add("o=|out=", "Output File or Directory.", option => outputPath = option);
            optSet.Add("t=|text=", "String to dis/assemble.", option => text = option);
            optSet.Add("r|recursive", "Process directory recursively.", option => recursive = option != null);
            optSet.Add("v|verbose", "Verbose Logging.", option => verbose = option != null);
            optSet.Add("repl", "REPL mode", option => repl = option != null);
            optSet.Parse(args);

            Assembler asm = new Assembler();
            var x = asm.AssembleFile(@"C:\Users\tslat\source\repos\CheatASM\CheatASM\examples\variables.asm");

            if (repl)
            {
                RunREPL();
                return;
            }

            /* movd [MAIN+R1+0x6C7634], 0x98967F */

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
            bool textMode = false;
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
                    if (text == "")
                    {
                        /* input path isn't an existing file or directory */
                        Console.Error.WriteLine("Unable to find the input path specified.");
                        return;
                    } else
                    {
                        textMode = true;
                    }
                }
            }

            /* at this point we know the inputPath exists, and if its a folder or not */

            asm = new Assembler();
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
                        File.WriteAllText(outputPath + relativePath, asm.AssembleFile(file).ToString());
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
                        if (textMode)
                        {
                            File.WriteAllText(outputPath, asm.AssembleString(text).ToString());
                        }
                        else
                        {
                            File.WriteAllText(outputPath, asm.AssembleFile(inputPath).ToString());
                        }
                    }
                    else
                    {
                        if (textMode)
                        {
                            Console.Write(asm.AssembleString(text));
                        }
                        else
                        {
                            Console.Write(asm.AssembleFile(inputPath));
                        }
                    }
                }else
                {
                    if (outputPath != "")
                    {
                        if (textMode)
                        {
                            File.WriteAllText(outputPath, disasm.DisassembleLine(text));
                        }
                        else
                        {
                            File.WriteAllText(outputPath, disasm.DisassembleFile(inputPath));
                        }
                    }
                    else
                    {
                        if (textMode)
                        {
                            Console.Write(disasm.DisassembleLine(text));
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

}
