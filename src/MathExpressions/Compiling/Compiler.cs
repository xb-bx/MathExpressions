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
        private Dictionary<string, Delegate> functions = new();
        private Dictionary<Type, object> objects = new(); 
        public Compiler AddFunction(string name, Delegate mi)
        {
            functions.Add(name, mi);
            return this;
        }
        public Compiler AddManyFunctions(IEnumerable<(string name, Delegate mi)> functions)
        {
            foreach(var item in functions)
            {
                AddFunction(item.name, item.mi);
            }
            return this;
        }
        public LinqExpressions.Expression CompileToExpression(IExpression expression, Dictionary<string, LinqExpressions.ParameterExpression> parameters) 
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
                            '^' => LinqExpressions.ExpressionType.Power,
                            _ => LinqExpressions.ExpressionType.Add
                        };
                        var expr1 = CompileToExpression(be.FirstExpression,parameters);
                        var expr2 = CompileToExpression(be.SecondExpression,parameters);
                        return LinqExpressions.Expression.MakeBinary(type, expr1, expr2);
                    }
                case UnaryExpression ue:
                    {
                        switch(ue.Operator) 
                        {
                            case '-':return LinqExpressions.Expression.Negate(CompileToExpression(ue.Expression,parameters));
                            default:return CompileToExpression(ue.Expression, parameters);
                        }    
                    }
                case FunctionExpression fe:
                    {
                        var method = functions[fe.Name]; 
                        var a = fe.Args.Select(x => CompileToExpression(x, parameters));
                        return LinqExpressions.Expression.Invoke(LinqExpressions.Expression.Constant(method), a);
                    }
            }           
            return null;
        }
        public T CompileToLambda<T>(IExpression expression) where T : Delegate 
        {
			var parameters = new Dictionary<string, LinqExpressions.ParameterExpression>();
            return System.Linq.Expressions.Expression.Lambda<T>(CompileToExpression(expression, parameters), parameters.Values).Compile();
        }
        public Delegate CompileToDelegate(IExpression expression) 
        {
			var parameters = new Dictionary<string, LinqExpressions.ParameterExpression>();
            return System.Linq.Expressions.Expression.Lambda(CompileToExpression(expression, parameters), parameters.Values).Compile();
        }
        
    }
}