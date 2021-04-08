using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MathExpressions.Lexing
{
    public class Lexer
    {  
        private static IReadOnlyDictionary<char, TokenType> operators = new Dictionary<char, TokenType>()
        {
            {'(', TokenType.LParen },
            {')', TokenType.RParen },
            {'+', TokenType.Plus },
            {'-', TokenType.Minus },
            {'*', TokenType.Star },
            {'/', TokenType.Slash },
            {',', TokenType.Comma },
            {'^', TokenType.Power },
        };
        public List<Token> Tokenize(ReadOnlySpan<char> code)
        {
            var res = new List<Token>();
            var sb = new StringBuilder();
            for (int currentPos = 0; currentPos < code.Length;)
            {
                switch (code[currentPos])
                {
                    case char c when operators.ContainsKey(c):
                        res.Add(new(operators[c], string.Empty));
                        currentPos++;
                        break;
                    case char c when char.IsDigit(c):
                        res.Add(TokenizeConstant(code, ref currentPos, sb));
                        break;
                    case char c when char.IsLetter(c):
                        res.Add(TokenizeId(code, ref currentPos,sb));
                        break;

                    default:
                        currentPos++;
                        break;
                }
            }

            return res;
        }
        public Task<List<Token>> TokenizeAsync(string code, CancellationToken token)
        {
            var res = new List<Token>();
            var sb = new StringBuilder();
            for (int currentPos = 0; currentPos < code.Length;)
            {
                switch (code[currentPos])
                {
                    case char c when operators.ContainsKey(c):
                        res.Add(new(operators[c], string.Empty));
                        currentPos++;
                        break;
                    case char c when char.IsDigit(c):
                        res.Add(TokenizeConstant(code, ref currentPos, sb));
                        break;
                    case char c when char.IsLetter(c):
                        res.Add(TokenizeId(code, ref currentPos, sb));
                        break;

                    default:
                        currentPos++;
                        break;
                }
                if (token.IsCancellationRequested)
                {
                    return Task.FromCanceled<List<Token>>(token);
                }
            }
            return Task.FromResult(res);
        }
        private Token TokenizeId(ReadOnlySpan<char> code, ref int currentPos, StringBuilder sb)
        {
            sb.Clear();
            while (currentPos < code.Length && char.IsLetter(code[currentPos]))
            {
                sb.Append(code[currentPos++]);
            }
            var res = sb.ToString();
            return new Token(TokenType.Id, res);
        }

        private Token TokenizeConstant(ReadOnlySpan<char> code, ref int currentPos, StringBuilder sb)
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
