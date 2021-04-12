using System.Linq.Expressions;
using System;
namespace MathExpressions.Parsing.AST
{
    public interface IExpression
    {
        IExpression Optimize();
    }
}
