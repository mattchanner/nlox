namespace Lox;

public class LoxFunction(FunctionStmt function, Environment closure, bool isInitializer) : ILoxCallable
{
    public int Arity => function.Parameters.Count;

    public LoxFunction Bind(LoxInstance instance)
    {
        Environment environment = new(closure);
        environment.Define("this", new LoxObject(instance));
        return new LoxFunction(function, environment, isInitializer);
    }

    public LoxObject? Call(Interpreter interpreter, List<LoxObject?> arguments)
    {
        Environment env = new Environment(closure);

        for (int i = 0; i < function.Parameters.Count; i++)
        {
            env.Define(function.Parameters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(function.Body, env);
        }
        catch (ReturnException re)
        {
            // If there is a return within the init method, make sure that "this" is returned
            // rather than nil
            if (isInitializer) 
                return closure.GetAt(0, "this");

            return re.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fn {function.Name.Lexeme}>";
    }
}