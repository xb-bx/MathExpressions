using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathExpressions.Parsing.AST
{
    public class FunctionExpression : IExpression
    {
        public FunctionExpression(string name, List<IExpression> args)
        {
            Name = name;
            Args = args;
        }

        public string Name { get; set; }
        public List<IExpression> Args { get; set; }

        public override bool Equals(object obj)
        {
            return obj is FunctionExpression expression &&
                   Name == expression.Name &&
                   Args.SequenceEqual(expression.Args);
                   //EqualityComparer<List<IExpression>>.Default.Equals(Args, expression.Args);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Args);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            sb.Append('(');
            for(int i = 0; i < Args.Count; i++)
            {
                sb.Append(Args[i].ToString());
                if(i<Args.Count - 1){
                    sb.Append(',');
                }
            }    
            sb.Append(')');
            return sb.ToString();
        }
    }
}
