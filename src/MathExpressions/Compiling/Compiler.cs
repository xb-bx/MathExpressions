using System;
using LinqExpressions = System.Linq.Expressions;
using MathExpressions.Parsing.AST;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
namespace MathExpressions.Compiling
{
    public class Compiler
    {
        public Dictionary<string, Delegate> Functions { get; private set; } = new();
        public Dictionary<string, double> Constants { get; private set; } = new();
        public Delegate this[string name]
        {
            get => Functions[name];
            set
            {
                if (Functions.ContainsKey(name))
                {
                    Functions[name] = value;
                }
                else
                {
                    Functions.Add(name, value);
                }
            }
        }
        public Compiler AddManyFunctions(IEnumerable<(string name, Delegate mi)> functions)
        {
            foreach (var item in functions)
            {
                this[item.name] = item.mi;
            }
            return this;
        }
        public LinqExpressions.Expression CompileToExpression(IExpression expression, Dictionary<string, LinqExpressions.ParameterExpression> parameters)
        {
            switch (expression)
            {
                case ConstantExpression ce:
                    return LinqExpressions.Expression.Constant(ce.Constant);
                case VariableExpression ve:
                    if (parameters.ContainsKey(ve.VariableName))
                    {
                        return parameters[ve.VariableName];
                    }
                    else if (Constants.ContainsKey(ve.VariableName))
                    {
                        return LinqExpressions.Expression.Constant(Constants[ve.VariableName]);
                    }
                    else
                    {
                        parameters.Add(ve.VariableName, LinqExpressions.Expression.Parameter(typeof(double), ve.VariableName));
                        return parameters[ve.VariableName];
                    }
                case BinaryExpression be:
                    {
                        var type = be.Operator switch
                        {
                            '-' => LinqExpressions.ExpressionType.Subtract,
                            '*' => LinqExpressions.ExpressionType.Multiply,
                            '/' => LinqExpressions.ExpressionType.Divide,
                            '^' => LinqExpressions.ExpressionType.Power,
                            '%' => LinqExpressions.ExpressionType.Modulo,
                            _ => LinqExpressions.ExpressionType.Add
                        };
                        var expr1 = CompileToExpression(be.FirstExpression, parameters);
                        var expr2 = CompileToExpression(be.SecondExpression, parameters);
                        return LinqExpressions.Expression.MakeBinary(type, expr1, expr2);
                    }
                case UnaryExpression ue:
                    {
                        switch (ue.Operator)
                        {
                            case '-': return LinqExpressions.Expression.Negate(CompileToExpression(ue.Expression, parameters));
                            default: return CompileToExpression(ue.Expression, parameters);
                        }
                    }
                case FunctionExpression fe:
                    {
                        var method = Functions[fe.Name];
                        var a = fe.Args.Select(x => CompileToExpression(x, parameters));
                        return LinqExpressions.Expression.Invoke(LinqExpressions.Expression.Constant(method), a);
                    }
            }
            return null;
        }
        public T CompileToLambda<T>(IExpression expression) where T : Delegate
        {
            var parameters = new Dictionary<string, LinqExpressions.ParameterExpression>();
            return LinqExpressions.Expression.Lambda<T>(CompileToExpression(expression, parameters), parameters.Values).Compile();
        }
        public Delegate CompileToDelegate(IExpression expression)
        {
            var parameters = new Dictionary<string, LinqExpressions.ParameterExpression>();
            return LinqExpressions.Expression.Lambda(CompileToExpression(expression, parameters), parameters.Values).Compile();
        }

    }
}