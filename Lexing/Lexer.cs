using System.Collections.Generic;
using System.Text;

namespace MathEpxressions.Lexing
{
    public class Lexer
    {
        private int currentPos = 0;
        private StringBuilder sb = new StringBuilder();
        private static IReadOnlyDictionary<char, TokenType> operators = new Dictionary<char, TokenType>()
        {
            {'(', TokenType.LParen },
            {')', TokenType.RParen },
            {'+', TokenType.Plus },
            {'-', TokenType.Minus },
            {'*', TokenType.Star },
            {'/', TokenType.Slash },
            {',', TokenType.Comma },
        };
        public List<Token> Tokenize(char[] code)
        {
            var res = new List<Token>();

            for (; currentPos < code.Length;)
            {
                switch (code[currentPos])
                {
                    case char c when operators.ContainsKey(c):
                        res.Add(new(operators[c], string.Empty));
                        currentPos++;
                        break;
                    case char c when char.IsDigit(c):
                        res.Add(TokenizeConstant(code));
                        break;
                    case char c when char.IsLetter(c):
                        res.Add(TokenizeId(code));
                        break;

                    default:
                        currentPos++;
                        break;
                }
            }

            return res;
        }

        private Token TokenizeId(char[] code)
        {
            sb.Clear();
            while (currentPos < code.Length && char.IsLetter(code[currentPos]))
            {
                sb.Append(code[currentPos++]);
            }
            var res = sb.ToString();
            return new Token(TokenType.Id, res);
        }

        private Token TokenizeConstant(char[] code)
        {
            sb.Clear();
            while (currentPos < code.Length && (code[currentPos] == '.' || char.IsDigit(code[currentPos])))
            {
                sb.Append(code[currentPos++]);
            }
            var res = sb.ToString();
            return new Token(TokenType.Constant, res);
        }
    }
}
