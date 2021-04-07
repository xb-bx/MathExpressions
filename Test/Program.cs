using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathExpressions;

namespace Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var engine = new EvaluationEngine();
            engine["sin"] = (Func<double, double>)(x => Math.Sin(x));
            engine["rad"] = (Func<double, double>)(x => x * Math.PI / 180);
            Console.WriteLine(engine.Evaluate("sin(rad(x))", new { x = 60 }));
        }
    }
}
