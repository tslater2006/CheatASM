using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CheatASM
{
    public class Disassembler
    {
        uint conditionalIndent = 0;

        public void ResetIndent()
        {
            conditionalIndent = 0;
        }

        private void WriteOpcode(CheatOpcode op, StringBuilder sb)
        {
            var indent = string.Concat(Enumerable.Repeat("  ", (int)conditionalIndent));
            sb.Append(indent);
            sb.Append(op.ToASM());
        }

        public string DisassembleString(string contents)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var line in contents.Split('\n',StringSplitOptions.RemoveEmptyEntries))
            {
                sb.AppendLine(DisassembleLine(line));
            }

            return sb.ToString();
        }
        public string DisassembleLine(string line)
        {
            /* trim the line... */
            line = line.Trim();
            StringBuilder sb = new StringBuilder();
            CheatOpcode op = null;
            /* its a code... lets parse it */
            try
            {
                uint[] blocks = line.Split(' ').Select(s => uint.Parse(s, NumberStyles.HexNumber)).ToArray();

                switch (line[0])
                {
                    case '0':
                        op = new Opcode0StoreStaticToMemory(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case '1':
                        op = new Opcode1Conditional(blocks);
                        WriteOpcode(op, sb);
                        conditionalIndent++;
                        break;
                    case '2':
                        op = new Opcode2EndConditional(blocks);
                        if (conditionalIndent > 0)
                        {
                            conditionalIndent--;
                        }
                        WriteOpcode(op, sb);
                        if (((Opcode2EndConditional)op).IsElse)
                        {
                            conditionalIndent++;
                        }
                        break;
                    case '3':
                        op = new Opcode3Loop(blocks);
                        if (((Opcode3Loop)op).IsEnd)
                        {
                            if (conditionalIndent > 0)
                            {
                                conditionalIndent--;
                            }
                        }
                        WriteOpcode(op, sb);
                        if (((Opcode3Loop)op).IsEnd == false)
                        {
                            conditionalIndent++;
                        }
                        break;
                    case '4':
                        op = new Opcode4LoadRegWithStatic(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case '5':
                        /*mov(b/w/d/q) R0 [HEAP+IMM]*/
                        op = new Opcode5LoadRegWithMem(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case '6':
                        op = new Opcode6StoreStaticToAddress(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case '7':
                        op = new Opcode7LegacyArithmetic(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case '8':
                        op = new Opcode8KeypressConditional(blocks);
                        WriteOpcode(op, sb);
                        conditionalIndent++;
                        break;
                    case '9':
                        op = new Opcode9Arithmetic(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case 'a':
                    case 'A':
                        op = new OpcodeAStoreRegToAddress(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case 'C':
                        /* Extended width set 1 */
                        switch(line[1])
                        {
                            case '0':
                                op = new OpcodeC0RegisterConditional(blocks);
                                WriteOpcode(op, sb);
                                conditionalIndent++;
                                break;
                            case '1':
                                op = new OpcodeC1SaveRestoreReg(blocks);
                                WriteOpcode(op, sb);
                                break;
                            case '2':
                                op = new OpcodeC2SaveRestoreRegMask(blocks);
                                WriteOpcode(op, sb);
                                break;
                            case '3':
                                op = new OpcodeC3ReadWriteStaticReg(blocks);
                                WriteOpcode(op, sb);
                                break;
                        }
                        break;
                    case 'F':
                        switch(line[1])
                        {
                            case 'F':
                                switch(line[2])
                                {
                                    case '0':
                                        op = new OpcodeFF0PauseProcess(blocks);
                                        WriteOpcode(op, sb);
                                        break;
                                    case '1':
                                        op = new OpcodeFF1ResumeProcess(blocks);
                                        WriteOpcode(op, sb);
                                        break;
                                    case 'F':
                                        op = new OpcodeFFFDebugLog(blocks);
                                        WriteOpcode(op, sb);
                                        break;
                                }
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                var indent = string.Concat(Enumerable.Repeat("  ", (int)conditionalIndent));
                sb.Append(indent);
                sb.Append("# Invalid Cheat Opcode: " + line.Trim());
            }
            return sb.ToString();
        }
        public string DisassembleFile(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            conditionalIndent = 0;

            foreach (string line in File.ReadLines(filePath))
            {
                if (line.StartsWith("{") || line.StartsWith("[") || line.Trim().Length == 0)
                {
                    sb.AppendLine(line);
                }
                else
                {
                    var lineOut = DisassembleLine(line);
                    sb.AppendLine(lineOut);

                }
            }
            return sb.ToString();
        }
    }


}
