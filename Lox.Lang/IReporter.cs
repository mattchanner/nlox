namespace Lox.Lang;

public interface IReporter
{
    void Error(int line, string message);

    void Report(int line, string where, string message);
}