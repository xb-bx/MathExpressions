# MathExpressions
.netstandard2.1 library to parse, evaluate and compile math expressions  
[Download](https://www.nuget.org/packages/MathExpressions)
[Try it online](https://dotnet-and-happiness.github.io/MathExpressions)
## Usage
### Interpete mode
```csharp
var engine = new EvaluationEngine();
double result = engine.Evaluate("54 + 43 / 5 * 2");
// Also there is asynchronious version of method
double result = await engine.EvaluateAsync("123 - 5");
```
#### Evaluate with object of anonymous type
```csharp
var engine = new EvaluationEngine();
double result = engine.Evaluate("x + 5", new {x = 10}); // 15
```

### Compile
#### Compile to Delegate
```csharp
var engine = new EvaluationEngine();
Delegate myDelegate = engine.Compile("124 + 5");
myDelegate.DynamicInvoke(); // 129
// Or with arguments 
Delegate delegateWithArgs = engine.Compile("x + 2");
delegateWithArgs.DynamicInvoke(8.0); // 10
```
#### Compile to lambda
```csharp
var engine = new EvaluationEngine();
Func<double, double> square = engine.Compile<Func<double,double>>("x^2");
square(2); // 4
```
### Optimization
You can evaluate or compile expression with optimization
```csharp
var engine = new EvaluationEngine();
double result = engine.Evaluate("x*0 + 5 + 11", new {x = 10}, true); // 16
```
Firstly, expression 'x*0 + 5 + 11' will be optimized to '16' and then evaluated to 16

### Functions
#### Using default functions
```csharp
var engine = new EvaluationEngine();
engine.AddDefaultFunctions();
engine.Evaluate("sqrt(x)", new {x = 4}); // 2
```
[List of default functions](#Functions)
#### Using custom functions
``` csharp
var engine = new EvaluationEngine();
engine["myfunc"] = (Func<double,double>)(x => x + 5);
engine.Evaluate("myfunc(x)", new {x = 4}); // 9
```

### Constants
#### Set contant
```csharp
var engine = new EvaluationEngine();
engine.SetCont("TwoPI", 6.28);
engine.Evaluate("TwoPI"); // 6.28
```
#### Default contstants
```csharp
var engine = new EvaluationEngine();
engine.AddDefaultConstants();
engine.Evaluate("PI") // 3.14
```
Default constants:
- E
- PI

### Binding methods and constants
For example, there is some class:  
```csharp
public static class MyFuncs
{
  public static const double MyConst = 55;
  public static double Twice(double val) => val * 2;
}
```
It can be binded in the following way:
```csharp
var engine = new EvaluationEngine();
engine.Bind(typeof(MyFuncs));
engine.Evaluate("Twice(MyConst)"); // 110
```
Also you can specify naming function:
```csharp
var engine = new EvaluationEngine();
engine.Bind(typeof(MyFuncs), member => member.Name.ToLower());
engine.Evaluate("twice(myconst)"); // 110
```

### Functions
- sin
- sinh
- asin
- asinh
- cos
- cosh
- acos
- acosh
- tan
- tanh
- atan
- atanh
- ctg
- floor
- ceiling
- round
- min
- max
- clamp 
- sqrt
- cbrt
- log
- abs
- rad
- deg
## Install 
```
dotnet add package MathExpressions --version 1.1.2
```
