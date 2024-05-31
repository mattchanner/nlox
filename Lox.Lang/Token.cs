namespace Lox.Lang;

public record Token(TokenType Type, string Lexeme, object? Literal, int Line)
{
}