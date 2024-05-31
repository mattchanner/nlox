namespace Lox.Lang;

public interface IReporter
{
    ParseError Error(Token token, string message);

    void Error(int line, string message);
    
    void RuntimeError(RuntimeError error);

    string Stringify(object? value);
}