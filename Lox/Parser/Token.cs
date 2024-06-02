using Lox.Runtime;

namespace Lox.Parser;

public record Token(TokenType Type, string Lexeme, LoxObject? Literal, int Line);