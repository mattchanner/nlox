namespace Lox;

public class LoxClass(string name, LoxObject? superclass, Dictionary<string, LoxFunction> methods) : ILoxCallable
{
    public override string ToString()
    {
        return name;
    }

    public LoxFunction? FindMethod(string methodName)
    {
        LoxFunction? func = methods.GetValueOrDefault(methodName) ?? superclass?.GetClass().FindMethod(methodName);

        return func;
    }

    public string Name => name;

    public int Arity
    {
        get
        {
            LoxFunction? initializer = FindMethod("init");
            return initializer?.Arity ?? 0;
        }
    }

    public LoxObject? Call(Interpreter interpreter, List<LoxObject?> arguments)
    {
        LoxInstance instance = new(this);
        LoxFunction? initializer = FindMethod("init");
        
        if (initializer != null)
        {
            initializer.Bind(instance).Call(interpreter, arguments);
        }

        return new LoxObject(instance);
    }
}