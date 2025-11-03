using System;
using System.Diagnostics;
using System.Numerics;

namespace LabCalculator
{
    class LabCalculatorVisitor : LabCalculatorBaseVisitor<BigInteger>
    {
        private readonly Func<string, BigInteger> resolve;

        public LabCalculatorVisitor(Func<string, BigInteger> variableResolver)
        {
            resolve = variableResolver;
        }

        public override BigInteger VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override BigInteger VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            return BigInteger.Parse(context.GetText());
        }

        public override BigInteger VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            return resolve(context.GetText());
        }

        public override BigInteger VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override BigInteger VisitUnaryExpr(LabCalculatorParser.UnaryExprContext context)
        {
            var value = Visit(context.expression());

            if (context.operatorToken.Type == LabCalculatorLexer.SUBTRACT)
                return -value;
            else
                return value;
        }

        public override BigInteger VisitExponentialExpr(LabCalculatorParser.ExponentialExprContext context)
        {
            BigInteger left = Visit(context.expression(0));
            BigInteger right = Visit(context.expression(1));
            return BigInteger.Pow(left, (int)right);
        }

        public override BigInteger VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            BigInteger left = Visit(context.expression(0));
            BigInteger right = Visit(context.expression(1));

            switch (context.operatorToken.Type)
            {
                case LabCalculatorLexer.MULTIPLY:
                    return left * right;
                case LabCalculatorLexer.DIVIDE:
                case LabCalculatorLexer.DIV:
                    return left / right;
                case LabCalculatorLexer.MOD:
                    return left % right;
            }

            return 0;
        }

        public override BigInteger VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            BigInteger left = Visit(context.expression(0));
            BigInteger right = Visit(context.expression(1));

            if (context.operatorToken.Type == LabCalculatorLexer.ADD)
                return left + right;
            else
                return left - right;
        }
    }
}