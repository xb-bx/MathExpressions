using System;

namespace MathExpressions
{
    public class VariableNotFoundException : Exception
    {
        public string Name { get; private set; }
        public VariableNotFoundException(string name) : base($"Variable {name} not found")
        {
            Name = name;
        }
    }
}
