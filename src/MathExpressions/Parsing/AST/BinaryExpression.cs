using System;
using System.Collections.Generic;
using System.Linq.Expressions;
namespace MathExpressions.Parsing.AST
{
    public class BinaryExpression : IExpression
    {
        public BinaryExpression(IExpression firstExpression, IExpression secondExpression, char @operator)
        {
            FirstExpression = firstExpression;
            SecondExpression = secondExpression;
            Operator = @operator;
        }

        public IExpression FirstExpression { get; private set; }
        public IExpression SecondExpression { get; private set; }
        public char Operator { get; set; }

        public override bool Equals(object obj)
        {
            return obj is BinaryExpression expression &&
                   EqualityComparer<IExpression>.Default.Equals(FirstExpression, expression.FirstExpression) &&
                   EqualityComparer<IExpression>.Default.Equals(SecondExpression, expression.SecondExpression) &&
                   Operator == expression.Operator;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstExpression, SecondExpression, Operator);
        }

        public override string ToString()
        {
            return $"({FirstExpression} {Operator} {SecondExpression})";
        }
    }
}
