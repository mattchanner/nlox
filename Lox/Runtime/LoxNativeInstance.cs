using System.Globalization;
using System.Reflection;
using Lox.Parser;

namespace Lox.Runtime;

public class LoxNativeInstance
{
    private Dictionary<string, LoxNativeFunction> functions = new();

    public LoxNativeInstance(Type libraryType)
    {
        MethodInfo[] staticMethods = libraryType.GetMethods(BindingFlags.Static | BindingFlags.Public);
        foreach (MethodInfo method in staticMethods)
        {
            string camelCase = method.Name.Substring(0, 1).ToLowerInvariant() + method.Name.Substring(1);
            functions[camelCase] = new LoxNativeFunction(method);
        }
    }

    public LoxObject? Get(Token name)
    {
        LoxNativeFunction? func = functions.GetValueOrDefault(name.Lexeme);
        return func == null ? LoxObject.Nil : new LoxObject(func!);
    }
}

public class LoxNativeFunction(MethodInfo method) : ILoxCallable
{
    public int Arity => method.GetParameters().Length;

    public LoxObject? Call(Interpreter interpreter, List<LoxObject?> arguments)
    {
        List<(LoxObject? First, ParameterInfo Second)> argsWithParam =
            arguments.Zip(method.GetParameters()).ToList();

        object?[] coerced = argsWithParam.Select(Coerce).ToArray();
        object? result = method.Invoke(
            null,
            BindingFlags.Static | BindingFlags.InvokeMethod,
            null,
            coerced,
            CultureInfo.CurrentCulture);

        if (result == null)
            return LoxObject.Nil;
        if (result is double d)
            return new LoxObject(d);
        if (result is int i)
            return new LoxObject(i);
        if (result is string s)
            return new LoxObject(s);
        if (result is bool b)
            return new LoxObject(b);

        throw new RuntimeError(
            $"Unable to convert function return type of {result.GetType().FullName} to a LoxObject.");
    }

    private object? Coerce((LoxObject First, ParameterInfo Second) arg)
    {
        if (arg.Second.ParameterType == typeof(double))
            return arg.First.GetNumber();
        if (arg.Second.ParameterType == typeof(int))
            return (int)arg.First.GetNumber();
        if (arg.Second.ParameterType == typeof(long))
            return (long)arg.First.GetNumber();
        if (arg.Second.ParameterType == typeof(string))
            return arg.First.GetString();
        if (arg.Second.ParameterType == typeof(bool))
            return arg.First.GetBool();
        throw new RuntimeError(
            $"Could not convert input parameter of type {arg.First.LoxType} to {arg.Second.ParameterType.FullName}");
    }
}