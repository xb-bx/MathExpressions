using MathExpressions.Compiling;
using MathExpressions.Lexing;
using MathExpressions.Parsing;
using MathExpressions.Parsing.AST;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization; 
using System.Threading;
using System.Threading.Tasks;

namespace MathExpressions
{

    public class EvaluationEngine
    { 
        private Lexer lexer = new();
        private Compiler compiler = new();
        private Parser parser;
        private Dictionary<string, Delegate> functions = new();
        public EvaluationEngine(CultureInfo cultureInfo = null)
        { 
            parser = new(cultureInfo);
            
        }
        public void AddDefaultFunctions()
        {
            functions["sin"] = (Func<double, double>)Math.Sin;
            functions["sinh"] = (Func<double, double>)Math.Sinh;
            functions["asin"] = (Func<double, double>)Math.Asin;
            functions["asinh"] = (Func<double, double>)Math.Asinh;
            functions["cos"] = (Func<double, double>)Math.Cos;
            functions["acos"] = (Func<double, double>)Math.Acos;
            functions["acosh"] = (Func<double, double>) Math.Acosh;
            functions["cosh"] = (Func<double, double>) Math.Cosh;
            functions["tan"] = (Func<double, double>) Math.Tan;
            functions["atan"] = (Func<double, double>) Math.Atan;
            functions["atanh"] = (Func<double, double>) Math.Atanh;
            functions["tanh"] = (Func<double, double>) Math.Tanh;
            functions["ctg"] = (Func<double, double>)(x => 1.0 / Math.Tan(x));
            functions["floor"] = (Func<double, double>) Math.Floor;
            functions["ceiling"] = (Func<double, double>) Math.Ceiling;
            functions["round"] = (Func<double, double>) Math.Round;
            functions["min"] = (Func<double, double, double>)Math.Min;
            functions["max"] = (Func<double, double, double>)Math.Max;
            functions["clamp"] = (Func<double, double, double, double>)Math.Clamp;
            functions["sqrt"] = (Func<double, double>)Math.Sqrt;
            functions["cbrt"] = (Func<double, double>)Math.Cbrt;
            functions["log"] = (Func<double, double>)Math.Log;
            functions["abs"] = (Func<double, double>)Math.Abs;
            functions["rad"] = (Func<double, double>)(x=>x * Math.PI / 180);
            functions["deg"] = (Func<double, double>)(x => x * 180 / Math.PI);

            compiler.AddManyFunctions(functions.AsEnumerable().Select(x => (x.Key, x.Value.Method)));
        }
        public Delegate this[string name]
        {
            get => functions[name];
            set
            {
                if (functions.ContainsKey(name))
                {
                    functions[name] = value;
                }
                else
                {
                    functions.Add(name, value);
                }
                compiler.AddFunction(name, value.Method);
            }
        }
        private double EvaluateExpression(IExpression expression, Dictionary<string, double> variables)
        {
            switch (expression)
            {
                case ConstantExpression constant:
                    return constant.Constant;
                case VariableExpression variable:
                    if (!variables.ContainsKey(variable.VariableName))
                        throw new VariableNotFoundException(variable.VariableName);
                    return variables[variable.VariableName];
                case UnaryExpression unary:
                    return unary.Operator switch
                    {
                        '-' => -EvaluateExpression(unary.Expression, variables),
                        _ => EvaluateExpression(unary.Expression, variables)
                    };
                case BinaryExpression binary:
                    return binary.Operator switch
                    {
                        '-' =>
                            EvaluateExpression(binary.FirstExpression, variables)
                            - EvaluateExpression(binary.SecondExpression, variables),
                        '*' =>
                            EvaluateExpression(binary.FirstExpression, variables)
                            * EvaluateExpression(binary.SecondExpression, variables),
                        '/' =>
                            EvaluateExpression(binary.FirstExpression, variables)
                            / EvaluateExpression(binary.SecondExpression, variables),
                        '^' =>
                            Math.Pow(EvaluateExpression(binary.FirstExpression, variables),
                             EvaluateExpression(binary.SecondExpression, variables)),
                        '+' =>
                            EvaluateExpression(binary.FirstExpression, variables)
                            + EvaluateExpression(binary.SecondExpression, variables),
                    };
                case FunctionExpression function:
                    if (!functions.ContainsKey(function.Name))
                    {
                        throw new FunctionNotFoundException(function.Name);
                    }
                    var fn = functions[function.Name];
                    var args = function.Args.Select(x => EvaluateExpression(x, variables) as object).ToArray();
                    return (double)fn.DynamicInvoke(args);
                default:

                    break;
            }
            return 0;
        }
        public double Evaluate(string expression, Dictionary<string, double> variables)
        {
            var tokens = lexer.Tokenize(expression);
            var expr = parser.Parse(tokens);
            return EvaluateExpression(expr, variables);
        }
        private Dictionary<string, double> GetVariables(object variables)
        {
            return new Dictionary<string, double>(
                variables.GetType()
                .GetProperties()
                .Select(x => new KeyValuePair<string, double>(x.Name, Convert.ToDouble(x.GetValue(variables)))));
        }
        public double Evaluate(string expression, object variables)
        {
            var vars = GetVariables(variables);
            var tokens = lexer.Tokenize(expression);
            var expr = parser.Parse(tokens);
            return EvaluateExpression(expr, vars);
        }
        public async Task<double> EvaluateAsync(string expression, Dictionary<string, double> variables, CancellationToken token)
        {
            var tokens = await lexer.TokenizeAsync(expression, token);
            var expr = parser.Parse(tokens);
            return EvaluateExpression(expr, variables);
        }
        public async Task<double> EvaluateAsync(string expression, object variables, CancellationToken token)
        {
            var vars = GetVariables(variables);
            var tokens = await lexer.TokenizeAsync(expression, token);
            var expr = parser.Parse(tokens);
            return EvaluateExpression(expr, vars);
        }
        public T Compile<T>(string expression) where T : Delegate
        {
            var tokens = lexer.Tokenize(expression);
            var expr = parser.Parse(tokens);
            return compiler.CompileToLambda<T>(expr);
        }
        public Delegate Compile(string expression)
        {
            var tokens = lexer.Tokenize(expression);
            var expr = parser.Parse(tokens);
            return compiler.CompileToDelegate(expr);
        }
    }
}
