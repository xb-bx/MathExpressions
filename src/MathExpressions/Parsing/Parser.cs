using MathExpressions.Lexing;
using MathExpressions.Parsing.AST;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathExpressions.Parsing
{
    public class Parser
    { 
        private List<Token> tokens;
        private CultureInfo cultureInfo;

        public Parser(CultureInfo cultureInfo = null)
        {
            this.cultureInfo = cultureInfo;
        }

        private IExpression AdditiveExpression(ref int currentPos)
        {
            var first = Multiplicative(ref currentPos);
            if (Match(TokenType.Plus, out _, ref currentPos))
            {
                var second = AdditiveExpression(ref currentPos);
                return new BinaryExpression(first, second, '+');
            }
            else if (Match(TokenType.Minus, out _, ref currentPos))
            {
                var second = AdditiveExpression(ref currentPos);
                return new BinaryExpression(first, second, '-');
            }
            return first;
        }
        private IExpression Multiplicative(ref int currentPos)
        {
            var first = Power(ref currentPos);
            if (Match(TokenType.Star, out _, ref currentPos))
            {
                var second = Multiplicative(ref currentPos);
                return new BinaryExpression(first, second, '*');
            }
            else if (Match(TokenType.Slash, out _, ref currentPos))
            {
                var second = Multiplicative(ref currentPos);
                return new BinaryExpression(first, second, '/');
            }
			else if (Match(TokenType.Mod, out _, ref currentPos))
            {
                var second = Multiplicative(ref currentPos);
                return new BinaryExpression(first, second, '%');
            }
            return first;
        }
        private IExpression Power(ref int currentPos)
        {
            var first = Unary(ref currentPos);
            if (Match(TokenType.Power, out _, ref currentPos))
            {
                var second = Power(ref currentPos);
                return new BinaryExpression(first, second, '^');
            }
            return first;
        }
        private IExpression Unary(ref int currentPos)
        {
            if (Match(TokenType.Plus, out _, ref currentPos))
            {
                var expr = Expression(ref currentPos);
                return new UnaryExpression(expr, '+');
            }
            else if (Match(TokenType.Minus, out _, ref currentPos))
            {
                var expr = Expression(ref currentPos);
                return new UnaryExpression(expr, '-');
            }
            return Expression(ref currentPos);
        }
        private List<IExpression> CommaSeperated(ref int currentPos)
        {
            var result = new List<IExpression>();
            
            while(currentPos < tokens.Count)
            {
                var expr = AdditiveExpression(ref currentPos);
                result.Add(expr);
                if(Match(TokenType.RParen, out _, ref currentPos))
                {
                    break;
                }
                currentPos++;
            }

            return result;

        }
        private IExpression Expression(ref int currentPos)
        {
            switch (tokens[currentPos++].Type)
            {
                case TokenType.Constant:
                    return new ConstantExpression(double.Parse(tokens[currentPos - 1].Value, cultureInfo?.NumberFormat));
                case TokenType.Id:
                    var name = tokens[currentPos-1].Value;
                    if (Match(TokenType.LParen, out _, ref currentPos)) 
                    { 
                        var seperated = CommaSeperated(ref currentPos);
                        return new FunctionExpression(name, seperated);
                    }
                    else
                    {
                        return new VariableExpression(tokens[currentPos - 1].Value);
                    }
                case TokenType.LParen:
                    var expr = AdditiveExpression(ref currentPos);
                    currentPos++;
                    return expr;
                default:
                    return null;
            }
        }

        public IExpression Parse(List<Token> tokens)
        {
            this.tokens = tokens;
            int pos = 0;
            return AdditiveExpression(ref pos);
        }
        private bool Match(TokenType type, out Token? t, ref int currentPos)
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
