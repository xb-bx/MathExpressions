using System;
using System.Linq.Expressions;
namespace MathExpressions.Parsing.AST
{
    public class ConstantExpression : IExpression
    {
        public double Constant { get; private set; }

        public ConstantExpression(double constant)
        {
            Constant = constant;
        } 
 
        public override string ToString()
        {
            return Constant.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is ConstantExpression expression &&
                   Constant == expression.Constant;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Constant);
        }
    }
}
