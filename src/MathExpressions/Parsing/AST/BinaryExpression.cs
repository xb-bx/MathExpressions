using System;
using System.Linq.Expressions;
namespace MathEpxressions.Parsing.AST
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
        public override string ToString()
        {
            return $"({FirstExpression} {Operator} {SecondExpression})";
        }
    }
}
