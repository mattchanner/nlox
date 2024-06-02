using Lox.Parser;

namespace Lox.Runtime;

public class RuntimeError : Exception
{
    public RuntimeError(string message) : base(message)
    {
    }

    public RuntimeError(Token token, string message) : base(message)
    {
        Token = token;
    }

    public Token? Token { get; }
}
