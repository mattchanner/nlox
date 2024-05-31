﻿namespace Lox;

public interface ILoxCallable
{
    int Arity { get; }

    LoxObject? Call(Interpreter interpreter, List<LoxObject?> arguments);
}