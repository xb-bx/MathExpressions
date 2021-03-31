using System;
using System.Linq.Expressions;
namespace MathEpxressions.Parsing.AST
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
    }
}
