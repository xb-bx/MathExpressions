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
        private CultureInfo cultureInfo;
        private Lexer lexer = new();
        private Compiler compiler = new();
        private Parser parser;
        private Dictionary<string, Delegate> functions = new();
        public EvaluationEngine(CultureInfo cultureInfo = null)
        {
            this.cultureInfo = cultureInfo;
            parser = new(cultureInfo);
            
        }
        public void AddDefaultFunctions()
        {
            functions["sin"] = (Func<double, double>)(x => Math.Sin(x));
            functions["sinh"] = (Func<double, double>)(x => Math.Sinh(x));
            functions["asin"] = (Func<double, double>)(x => Math.Asin(x));
            functions["asinh"] = (Func<double, double>)(x => Math.Asinh(x));
            functions["cos"] = (Func<double, double>)(x => Math.Cos(x));
            functions["acos"] = (Func<double, double>)(x => Math.Acos(x));
            functions["acosh"] = (Func<double, double>)(x => Math.Acosh(x));
            functions["cosh"] = (Func<double, double>)(x => Math.Cosh(x));
            functions["tan"] = (Func<double, double>)(x => Math.Tan(x));
            functions["atan"] = (Func<double, double>)(x => Math.Atan(x));
            functions["atanh"] = (Func<double, double>)(x => Math.Atanh(x));
            functions["tanh"] = (Func<double, double>)(x => Math.Tanh(x));
            functions["ctg"] = (Func<double, double>)(x => 1.0 / Math.Tan(x));
            functions["floor"] = (Func<double, double>)(x => Math.Floor(x));
            functions["ceiling"] = (Func<double, double>)(x => Math.Ceiling(x));
            functions["round"] = (Func<double, double>)(x => Math.Round(x));
            functions["min"] = (Func<double, double, double>)((x, y) => Math.Min(x, y));
            functions["max"] = (Func<double, double, double>)((x, y) => Math.Max(x, y));
            functions["clamp"] = (Func<double, double, double, double>)((x, y, z) => Math.Clamp(x, y, z));
            functions["sqrt"] = (Func<double, double>)((x) => Math.Sqrt(x));
            functions["cbrt"] = (Func<double, double>)((x) => Math.Cbrt(x));
            functions["log"] = (Func<double, double>)((x) => Math.Log(x));
            functions["abs"] = (Func<double, double>)(x => Math.Abs(x));
            functions["rad"] = (Func<double, double>)(x => x * Math.PI / 180);
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
                compiler.AddFunction(name, value);
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
