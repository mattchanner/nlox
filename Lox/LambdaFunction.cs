namespace Lox;

/// <summary>
/// Lambda function implementation
/// </summary>
/// <param name="expression">The lambda expression to execute</param>
/// <param name="environment">The parent environment</param>
internal class LambdaFunction(LambdaExpression expression, Environment environment) : ILoxCallable
{
    public int Arity => expression.Parameters.Count;

    public LoxObject? Call(Interpreter interpreter, List<LoxObject?> arguments)
    {
        Environment env = new(environment);

        for (int i = 0; i < expression.Parameters.Count; i++)
        {
            // Add the named parameters to the local environment in order for the block to execute
            env.Define(expression.Parameters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(expression.Body, env);
        }
        catch (ReturnException re)
        {
            return re.Value;
        }

        return null;
    }
}