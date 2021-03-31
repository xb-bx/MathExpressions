using System;
using LinqExpressions = System.Linq.Expressions;
using MathEpxressions.Parsing.AST;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace MathEpxressions.Compiling
{
    public class Compiler
    {
        private Dictionary<string, LinqExpressions.ParameterExpression> parameters = new();
        private Dictionary<string, MethodInfo> functions = new();

        public Compiler WithDefaultFunctions()
        {
            var mathMethods = typeof(Math).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(x => x.ReturnParameter.ParameterType == typeof(double) && x.GetParameters().All(x =>x.ParameterType == typeof(double)));
            
            foreach(var item in mathMethods)
            {
                var name = item.Name.ToLower();
                if(functions.ContainsKey(name))
                {
                    functions[name] = item;
                }
                else
                {
                    functions.Add(name, item);
                }
            }
            return this;
        }
        public Compiler AddFunction(string name, MethodInfo mi)
        {
            functions.Add(name, mi);
            return this;
        }
        public Compiler AddManyFunctions(IEnumerable<(string name, MethodInfo mi)> functions)
        {
            foreach(var item in functions)
            {
                AddFunction(item.name, item.mi);
            }
            return this;
        }
        public LinqExpressions.Expression CompileToExpression(IExpression expression) 
        {
            switch(expression) 
            {
                case ConstantExpression ce: 
                    return LinqExpressions.Expression.Constant(ce.Constant);
                case VariableExpression ve:
                    if(parameters.ContainsKey(ve.VariableName))
                    {
                        return parameters[ve.VariableName];
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
                            _ => LinqExpressions.ExpressionType.Add
                        };
                        var expr1 = CompileToExpression(be.FirstExpression);
                        var expr2 = CompileToExpression(be.SecondExpression);
                        return LinqExpressions.Expression.MakeBinary(type, expr1, expr2);
                    }
                case UnaryExpression ue:
                    {
                        switch(ue.Operator) 
                        {
                            case '-':return LinqExpressions.Expression.Negate(CompileToExpression(ue.Expression));
                            default:return CompileToExpression(ue.Expression);
                        }    
                    }
                case FunctionExpression fe:
                    {
                        var method = functions[fe.Name];
                        return LinqExpressions.Expression.Call(null, method, fe.Args.Select(x => CompileToExpression(x)));
                    }
            }
            return null;
        }
        public T CompileToLambda<T>(IExpression expression) where T : Delegate 
        {
            return System.Linq.Expressions.Expression.Lambda<T>(CompileToExpression(expression), parameters.Values).Compile();
        }
        public Delegate CompileToDelegate(IExpression expression) 
        {
            return System.Linq.Expressions.Expression.Lambda(CompileToExpression(expression), parameters.Values).Compile();
        }
        
    }
}