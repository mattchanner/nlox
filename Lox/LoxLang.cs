using Lox.Ast;
using Lox.Parser;
using Lox.Runtime;

namespace Lox;

public class LoxLang : IReporter
{
    private readonly Interpreter interpreter;
    private bool hasError = false;
    private bool hadRuntimeError = false;

    public LoxLang() => interpreter = new(this);

    public void Reset()
    {
        interpreter.Reset();
        hasError = false;
        hadRuntimeError = false;
    }

    public bool HasError => this.hasError;

    public bool HasRuntimeError => this.hadRuntimeError;

    public void Run(string source)
    {
        Reset();
        Scanner scanner = new(source, this);
        List<Token> tokens = scanner.ScanTokens();
        Parser.Parser parser = new(tokens, this);
        List<Stmt> statements = parser.Parse();

        if (hasError) return;

        try
        {
            Resolver resolver = new Resolver(interpreter, this);

            resolver.Resolve(statements);

            // Stop if there was a resolution error.
            if (hasError) return;

            interpreter.Interpret(statements);
        }
        catch (RuntimeError rt)
        {
            RuntimeError(rt);
        }
        catch (ReturnException re)
        {
        }
    }

    public ParseError Error(Token token, string message)
    {
        Error(token.Line, message);
        return new ParseError(token, message);  
    }

    public void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public void RuntimeError(RuntimeError error) {
        Console.Error.WriteLine(error.Message +
                                "\n[line " + error.Token.Line + "]");
        hadRuntimeError = true;
    }

    private void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        hasError = true;
    }

    public string Stringify(object? value) 
    {
        if (value == null) return "nil";

        if (value is double) 
        {
            string text = value.ToString()!;
            if (text.EndsWith(".0")) {
                text = text.Substring(0, text.Length - 2);
            }
            return text;
        }

        return value!.ToString()!;
    }
}