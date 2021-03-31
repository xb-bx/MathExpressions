using System;
using System.Linq;
using System.Threading.Tasks;
namespace MathEpxressions.Lexing
{
    public struct Token
    {
        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public TokenType Type { get; private set; }
        public string Value { get; private set; }
        public override string ToString()
            => $"{Type}: {Value}";
    }
}
