using System;
using System.Linq.Expressions;

namespace MathExpressions.Parsing.AST
{
    public class VariableExpression : IExpression
    {
        public string VariableName { get; private set; }

        public VariableExpression(string variableName)
        {
            VariableName = variableName;
        }
 
 
        public override string ToString()
        {
            return VariableName;
        }

        public override bool Equals(object obj)
        {
            return obj is VariableExpression expression &&
                   VariableName == expression.VariableName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VariableName);
        }
    }
}
