using Lox.Lang;

namespace Lox;

public class RuntimeError : Exception
{
    public RuntimeError(string message) : base(message)
    {
    }

    public RuntimeError(Token token, string message) : base(message)
    {
        this.Token = token;
    }

    public Token? Token { get; }
}
