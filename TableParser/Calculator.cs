using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableParser;

namespace TableParser
{
    public static class Calculator
    {
        public static TableIndex TableIndex { get; }

        static Calculator()
        {
            TableIndex = new();
        }
        public static double Evaluate(string expression)
        {
            var lexer = new TableParserLexer(new AntlrInputStream(expression));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ThrowExceptionErrorListener());

            var tokens = new CommonTokenStream(lexer);
            var parser = new TableParserParser(tokens);

            var tree = parser.compileUnit();

            var visitor = new LabCalculatorVisitor();

            return visitor.Visit(tree);
        }
    }
}
