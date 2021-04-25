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
using System.Reflection;
namespace MathExpressions
{
    public delegate string FuncRenamer(string funcName);
    public class EvaluationEngine
    {
        private Lexer lexer = new();
        private Compiler compiler = new();
        private Parser parser;


        public EvaluationEngine(CultureInfo cultureInfo = null)
        {
            parser = new(cultureInfo);
        }
        public void AddDefaultConstants()
        {
            SetConst("E", Math.E);
            SetConst("PI", Math.PI);
        }
        public void AddDefaultFunctions()
        {
            this["sin"] = (Func<double, double>)Math.Sin;
            this["sinh"] = (Func<double, double>)Math.Sinh;
            this["asin"] = (Func<double, double>)Math.Asin;
            this["asinh"] = (Func<double, double>)Math.Asinh;
            this["cos"] = (Func<double, double>)Math.Cos;
            this["acos"] = (Func<double, double>)Math.Acos;
            this["acosh"] = (Func<double, double>)Math.Acosh;
            this["cosh"] = (Func<double, double>)Math.Cosh;
            this["tan"] = (Func<double, double>)Math.Tan;
            this["atan"] = (Func<double, double>)Math.Atan;
            this["atanh"] = (Func<double, double>)Math.Atanh;
            this["tanh"] = (Func<double, double>)Math.Tanh;
            this["ctg"] = (Func<double, double>)(x => 1.0 / Math.Tan(x));
            this["floor"] = (Func<double, double>)Math.Floor;
            this["ceiling"] = (Func<double, double>)Math.Ceiling;
            this["round"] = (Func<double, double>)Math.Round;
            this["min"] = (Func<double, double, double>)Math.Min;
            this["max"] = (Func<double, double, double>)Math.Max;
            this["clamp"] = (Func<double, double, double, double>)Math.Clamp;
            this["sqrt"] = (Func<double, double>)Math.Sqrt;
            this["cbrt"] = (Func<double, double>)Math.Cbrt;
            this["log"] = (Func<double, double>)Math.Log;
            this["abs"] = (Func<double, double>)Math.Abs;
            this["rad"] = (Func<double, double>)(x => x * Math.PI / 180);
            this["deg"] = (Func<double, double>)(x => x * 180 / Math.PI);
        }

        public void SetConst(string name, double val)
        {
            if (compiler.Constants.ContainsKey(name))
            {
                compiler.Constants[name] = val;
            }
            else
            {
                compiler.Constants.Add(name, val);
            }
        }


        public Delegate this[string name]
        {
            get => compiler[name];
            set => compiler[name] = value; 
        }


        private double EvaluateExpression(IExpression expression, Dictionary<string, double> variables)
        {
            switch (expression)
            {
                case ConstantExpression constant:
                    return constant.Constant;
                case VariableExpression variable:
                    if (variables?.ContainsKey(variable.VariableName) == true)
                    {
                        return variables[variable.VariableName];
                    }
                    else if(compiler.Constants.ContainsKey(variable.VariableName))
                    {
                        return compiler.Constants[variable.VariableName];
                    }
                    else
                    {
                        throw new VariableNotFoundException(variable.VariableName);
                    }
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
                        '%' =>
                            EvaluateExpression(binary.FirstExpression, variables)
                            % EvaluateExpression(binary.SecondExpression, variables),

                    };
                case FunctionExpression function:
                    if (!compiler.Functions.ContainsKey(function.Name))
                    {
                        throw new FunctionNotFoundException(function.Name);
                    }
                    var fn =compiler.Functions[function.Name];
                    var args = function.Args.Select(x => (EvaluateExpression(x, variables)) as object).ToArray();
                    return Convert.ToDouble(fn.DynamicInvoke(args));
                default:

                    break;
            }
            return 0;
        }
        public IExpression Parse(string expression, bool optimize = false)
        {
            var tokens = lexer.Tokenize(expression);
            var expr = parser.Parse(tokens);
            expr = optimize ? expr.Optimize() : expr;
            return expr;
        }

        public void Bind(Type type, FuncRenamer renamer = null)
        {
            System.Diagnostics.Debugger.Launch();
            var dT = typeof(double);
            var fns = type
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.ReturnType == dT)
                .Where(x => x.GetParameters().All(x => x.ParameterType == dT))
                .Select(x =>
                    (Delegate.CreateDelegate(System.Linq.Expressions.Expression.GetFuncType(
                            x.GetParameters()
                                .Select(t => t.ParameterType)
                                .Append(dT)
                                .ToArray()
                    ),
                x), renamer is null ? x.Name : renamer(x.Name)))
                ;
            foreach (var fn in fns)
            {
                this[fn.Item2] = fn.Item1;
            }
            var consts = type.GetMembers(BindingFlags.Static | BindingFlags.Public).Where(member => member.MemberType == MemberTypes.Field);
            foreach (var c in consts)
            {
                SetConst(renamer is null ? c.Name : renamer(c.Name), (double)((FieldInfo)c).GetValue(null));
            }
        }

        public double Evaluate(IExpression expression, Dictionary<string, double> variables)
        {
            return EvaluateExpression(expression, variables);
        }

        public double Evaluate(IExpression expression, object variables = null)
        {
            return EvaluateExpression(expression, variables is not null ? GetVariables(variables) : null);
        }
        public double Evaluate(string expression, Dictionary<string, double> variables, bool optimize = false)
        {
            var tokens = lexer.Tokenize(expression);
            var expr = parser.Parse(tokens);
            expr = optimize ? expr.Optimize() : expr;
            return EvaluateExpression(expr, variables);
        }
        private Dictionary<string, double> GetVariables(object variables)
        {
            return new Dictionary<string, double>(
                variables.GetType()
                .GetProperties()
                .Select(x => new KeyValuePair<string, double>(x.Name, Convert.ToDouble(x.GetValue(variables)))));
        }
        public double Evaluate(string expression, object variables = null)
        {
            var vars = variables is not null ? GetVariables(variables) : null;
            var tokens = lexer.Tokenize(expression);
            var expr = parser.Parse(tokens);
            return EvaluateExpression(expr, vars);
        }
        public async Task<double> EvaluateAsync(string expression, Dictionary<string, double> variables, bool optimize = false, CancellationToken token = default)
        {
            var tokens = await lexer.TokenizeAsync(expression, token);
            var expr = parser.Parse(tokens);
            return EvaluateExpression(expr, variables);
        }
        public async Task<double> EvaluateAsync(string expression, object variables = null, bool optimize = false, CancellationToken token = default)
        {
            var vars = variables is not null ? GetVariables(variables) : null;
            var tokens = await lexer.TokenizeAsync(expression, token);
            var expr = parser.Parse(tokens);
            return EvaluateExpression(expr, vars);
        }
        public T Compile<T>(string expression, bool optimize = false) where T : Delegate
        {
            var tokens = lexer.Tokenize(expression);
            var expr = parser.Parse(tokens);
            expr = optimize ? expr.Optimize() : expr;
            return compiler.CompileToLambda<T>(expr);
        }
        public Delegate Compile(string expression, bool optimize = false)
        {
            var tokens = lexer.Tokenize(expression);
            var expr = parser.Parse(tokens);
            expr = optimize ? expr.Optimize() : expr;
            return compiler.CompileToDelegate(expr);
        }
    }
}
