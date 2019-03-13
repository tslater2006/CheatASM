using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Text;
using static CheatASM.CheatASMParser;

namespace CheatASM
{

    public class Assembler : BaseErrorListener
    {
        string errorMsg;
        int errorPos;
        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            base.SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e);
            errorMsg = msg;
            errorPos = charPositionInLine;
        }
        public string AssembleLine(string line)
        {
            AntlrInputStream stream = new AntlrInputStream(line);
            CheatASMLexer lexer = new CheatASMLexer(stream);
            CheatASMParser parser = new CheatASMParser(new CommonTokenStream(lexer));
            parser.ErrorHandler = new DefaultErrorStrategy();
            parser.TrimParseTree = true;
            parser.BuildParseTree = true;
            parser.AddErrorListener(this);
            StatementContext stmt = null;
            errorMsg = null;
            errorPos = 0;
            stmt = parser.statement();
            
            if (errorMsg != null)
            {
                return "# Error: " + errorMsg  + Environment.NewLine + line;
            }
            if (stmt.opCode0() != null)
            {
                OpCode0Context op = stmt.opCode0();
                /* assemble opcode 0 */
            }
            else if (stmt.opCode1() != null)
            {

            }
            else if (stmt.opCode2() != null)
            {

            }
            else if (stmt.opCode3() != null)
            {

            }
            else if (stmt.opCode4() != null)
            {

            }
            else if (stmt.opCode5() != null)
            {

            }
            else if (stmt.opCode6() != null)
            {

            }
            else if (stmt.opCode7() != null)
            {

            }
            else if (stmt.opCode8() != null)
            {

            }
            else if (stmt.opCode9() != null)
            {

            }
            else if (stmt.opCodeA() != null)
            {

            }

            var i = 3;
            return "Valid but we haven't assembled yet";
        }
    }
}
