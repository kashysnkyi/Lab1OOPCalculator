using Antlr4.Runtime;
using System;
using System.Numerics;

namespace LabCalculator
{
    public static class Calculator
    {

        public static BigInteger Evaluate(string expression, Func<string, BigInteger> resolveVariable)
        {
            var lexer = new LabCalculatorLexer(new AntlrInputStream(expression));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ThrowExceptionErrorListener());

            var tokens = new CommonTokenStream(lexer);
            var parser = new LabCalculatorParser(tokens);

            var tree = parser.compileUnit();
            var visitor = new LabCalculatorVisitor(resolveVariable);

            return visitor.Visit(tree);
        }
    }
}