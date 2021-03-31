using MathExpressions.Lexing;
using MathExpressions.Parsing.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathExpressions.Parsing
{
    public class Parser
    {
        private int currentPos;
        private List<Token> tokens;
        private IExpression AdditiveExpression()
        {
            var first = Multiplicative();
            if (Match(TokenType.Plus, out _))
            {
                var second = AdditiveExpression();
                return new BinaryExpression(first, second, '+');
            }
            else if (Match(TokenType.Minus, out _))
            {
                var second = AdditiveExpression();
                return new BinaryExpression(first, second, '-');
            }
            return first;
        }
        private IExpression Multiplicative()
        {
            var first = Unary();
            if (Match(TokenType.Star, out _))
            {
                var second = Multiplicative();
                return new BinaryExpression(first, second, '*');
            }
            else if (Match(TokenType.Slash, out _))
            {
                var second = Multiplicative();
                return new BinaryExpression(first, second, '/');
            }
            return first;
        }
        private IExpression Unary()
        {
            if (Match(TokenType.Plus, out _))
            {
                var expr = Expression();
                return new UnaryExpression(expr, '+');
            }
            else if (Match(TokenType.Minus, out _))
            {
                var expr = Expression();
                return new UnaryExpression(expr, '-');
            }
            return Expression();
        }
        private List<IExpression> CommaSeperated()
        {
            var result = new List<IExpression>();
            
            while(currentPos < tokens.Count)
            {
                var expr = AdditiveExpression();
                result.Add(expr);
                if(Match(TokenType.RParen, out _))
                {
                    break;
                }
                currentPos++;
            }

            return result;

        }
        private IExpression Expression()
        {
            switch (tokens[currentPos++].Type)
            {
                case TokenType.Constant:
                    return new ConstantExpression(double.Parse(tokens[currentPos - 1].Value));
                case TokenType.Id:
                    var name = tokens[currentPos-1].Value;
                    if (Match(TokenType.LParen, out _)) 
                    { 
                        var seperated = CommaSeperated();
                        return new FunctionExpression(name, seperated);
                    }
                    else
                    {
                        return new VariableExpression(tokens[currentPos - 1].Value);
                    }
                case TokenType.LParen:
                    var expr = AdditiveExpression();
                    currentPos++;
                    return expr;
                default:
                    return null;
            }
        }

        public IExpression Parse(List<Token> tokens)
        {
            this.tokens = tokens;

            return AdditiveExpression();
        }
        private bool Match(TokenType type, out Token? t)
        {
            if (currentPos < tokens.Count && tokens[currentPos].Type == type)
            {
                t = tokens[currentPos++];
                return true;
            }
            else
            {
                t = null;
                return false;
            }
        }
    }
}
