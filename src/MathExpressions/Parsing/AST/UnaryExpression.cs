using System;
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
        
        
         
        public override string ToString()
        {
            return $"{Operator}{Expression}";
        }
    }
}
