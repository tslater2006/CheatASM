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
        private void WriteOpcode(CheatOpcode op, StringBuilder sb)
        {
            var indent = string.Concat(Enumerable.Repeat("  ", (int)conditionalIndent));
            sb.Append(indent);
            sb.Append(op.ToASM());
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
                        op = new StoreStaticOpcode(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case '1':
                        op = new ConditionalOpcode(blocks);
                        WriteOpcode(op, sb);
                        conditionalIndent++;
                        break;
                    case '2':
                        op = new EndConditionalOpcode(blocks);
                        conditionalIndent--;
                        WriteOpcode(op, sb);
                        break;
                    case '3':
                        op = new LoopOpcode(blocks);
                        if (((LoopOpcode)op).IsEnd)
                        {
                            conditionalIndent--;
                        }
                        WriteOpcode(op, sb);
                        if (((LoopOpcode)op).IsEnd == false)
                        {
                            conditionalIndent++;
                        }
                        break;
                    case '4':
                        op = new LoadRegisterStaticOpcode(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case '5':
                        /*mov(b/w/d/q) R0 [HEAP+IMM]*/
                        op = new LoadRegisterMemoryOpcode(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case '6':
                        op = new StoreStaticToAddressOpcode(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case '7':
                        op = new LegacyArithmeticOpcode(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case '8':
                        op = new KeypressConditionalOpcode(blocks);
                        WriteOpcode(op, sb);
                        conditionalIndent++;
                        break;
                    case '9':
                        op = new ArithmeticOpcode(blocks);
                        WriteOpcode(op, sb);
                        break;
                    case 'a':
                    case 'A':
                        op = new StoreRegisterToAddressOpcode(blocks);
                        WriteOpcode(op, sb);
                        break;
                    default:
                        Debugger.Break();
                        break;
                    case 'B':
                        /* reserved */
                        break;
                    case 'C':
                        /* Extended width set 1 */
                        switch(line[1])
                        {
                            case '0':
                                /* Compare register */
                                op = new RegisterConditionalOpcode(blocks);
                                WriteOpcode(op, sb);
                                conditionalIndent++;
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
