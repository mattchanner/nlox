using Lox.Parser;

namespace Lox.Runtime;

public class LoxInstance(LoxClass @class)
{
    private readonly Dictionary<string, LoxObject?> fields = new();

    public LoxObject? Get(Token name)
    {
        if (fields.TryGetValue(name.Lexeme, out var value))
            return value;

        LoxFunction? method = @class.FindMethod(name.Lexeme);
        if (method != null)
            return new LoxObject(method.Bind(this));

        throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
    }

    public void Set(Token name, LoxObject? value)
    {
        fields[name.Lexeme] = value;
    }

    public override string ToString() => $"{@class.Name} instance";
}