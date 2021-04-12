using System;
using System.Collections.Generic;
using System.Linq.Expressions;
namespace MathExpressions.Parsing.AST
{
    public class UnaryExpression : IExpression
    {
        public UnaryExpression(IExpression expression, char @operator)
        {
            Expression = expression;
            Operator = @operator;
        }

        public IExpression Expression { get; private set; }
        public char Operator { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is UnaryExpression expression &&
                   EqualityComparer<IExpression>.Default.Equals(Expression, expression.Expression) &&
                   Operator == expression.Operator;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Expression, Operator);
        }

        public override string ToString()
        {
            return $"{Operator}{Expression}";
        }
    }
}
