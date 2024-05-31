using Lox.Lang;

namespace Lox;

public class Environment
{
    private readonly Dictionary<string, LoxObject?> values = new();

    private readonly Environment? enclosing;

    public Environment() => enclosing = null;

    public Environment(Environment enclosing) => this.enclosing = enclosing;

    public Environment? Enclosing => this.enclosing;

    public void Reset()
    {
        this.values.Clear();
    }

    public void Define(string name, LoxObject? value) 
        => this.values[name] = value;

    public void AssignAt(int distance, Token name, LoxObject? value)
    {
        Environment ancestor = Ancestor(distance);
        ancestor.values[name.Lexeme] = value;
    }

    public void Assign(Token name, LoxObject? value)
    {
        if (this.values.ContainsKey(name.Lexeme))
        {
            this.values[name.Lexeme] = value;
            return;
        }

        if (this.enclosing != null)
        {
            this.enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public LoxObject? GetAt(int distance, string name)
    {
        var env = Ancestor(distance); 
        return env.values[name];
    }

    public Environment Ancestor(int distance)
    {
        Environment? environment = this;
        for (int i = 0; i < distance; i++) {
            environment = environment!.enclosing; 
        }

        return environment!;
    }

    public LoxObject? Get(Token name)
    {
        if (this.values.TryGetValue(name.Lexeme, out LoxObject? value))
            return value;

        if (this.enclosing != null)
            return this.enclosing.Get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }
}