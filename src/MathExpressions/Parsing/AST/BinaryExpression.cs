using System;
using System.Collections.Generic;
using System.Linq.Expressions;
namespace MathExpressions.Parsing.AST
{
    public class BinaryExpression : IExpression
    {
        public BinaryExpression(IExpression firstExpression, IExpression secondExpression, char @operator)
        {
            FirstExpression = firstExpression;
            SecondExpression = secondExpression;
            Operator = @operator;
        }

        public IExpression FirstExpression { get; private set; }
        public IExpression SecondExpression { get; private set; }
        public char Operator { get; set; }

        public override bool Equals(object obj)
        {
            return obj is BinaryExpression expression &&
                   EqualityComparer<IExpression>.Default.Equals(FirstExpression, expression.FirstExpression) &&
                   EqualityComparer<IExpression>.Default.Equals(SecondExpression, expression.SecondExpression) &&
                   Operator == expression.Operator;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstExpression, SecondExpression, Operator);
        }

        public IExpression Optimize()
        {
            var fst = FirstExpression.Optimize();
            var snd = SecondExpression.Optimize();
            if (fst is ConstantExpression constant1 && snd is ConstantExpression constant2)
            {
                if (constant1.Equals(constant2))
                {
                    if (Operator == '/')
                        return new ConstantExpression(1);
                    else if (Operator == '-')
                        return new ConstantExpression(0);
                }
                else
                {
                    return Operator switch
                    {
                        '+' => new ConstantExpression(constant1.Constant + constant2.Constant),
                        '-' => new ConstantExpression(constant1.Constant - constant2.Constant),
                        '*' => new ConstantExpression(constant1.Constant * constant2.Constant),
                        '/' => new ConstantExpression(constant1.Constant / constant2.Constant),
                        '%' => new ConstantExpression(constant1.Constant % constant2.Constant),
                        '^' => new ConstantExpression(Math.Pow(constant1.Constant, constant2.Constant))
                    };
                }
            }
            else if (Operator == '/' && fst.Equals(snd))
                return new ConstantExpression(1);
            return new BinaryExpression(fst, snd, Operator);
        }

        public override string ToString()
        {
            return $"({FirstExpression} {Operator} {SecondExpression})";
        }
    }
}
