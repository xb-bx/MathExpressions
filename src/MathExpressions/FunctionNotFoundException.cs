using System;

namespace MathExpressions
{
    public class FunctionNotFoundException : Exception
    {
        public string Name { get; private set; }
        public FunctionNotFoundException(string name) : base($"Function {name} not found")
        {
            Name = name;
        }
    }
}
