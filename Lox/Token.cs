namespace Lox.Lang;

public record Token(TokenType Type, string Lexeme, LoxObject? Literal, int Line)
{
}