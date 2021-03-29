using System;
using System.Linq.Expressions;
namespace MathEpxressions.Parsing.AST
{
    public interface IExpression
    {
        double Evaluate();

        Expression Compile();
    }
}
