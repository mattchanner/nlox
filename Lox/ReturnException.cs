namespace Lox;

public class ReturnException(LoxObject? value) : Exception
{
    public LoxObject? Value => value;
}