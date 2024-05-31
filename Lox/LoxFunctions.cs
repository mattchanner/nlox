using static System.DateTime;

namespace Lox;

internal class Clock : ILoxCallable
{
    public int Arity => 0;

    public LoxObject? Call(Interpreter interpreter, List<LoxObject?> arguments)
    {
        return new LoxObject((UtcNow - UnixEpoch).TotalSeconds);
    }
}