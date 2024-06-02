using Lox.Runtime;

namespace Lox.Parser;

public class Scanner(string source, IReporter reporter)
{
    private static readonly Dictionary<string, TokenType> keywords = new()
    {
        { "and", TokenType.AND },
        { "class", TokenType.CLASS },
        { "else", TokenType.ELSE },
        { "false", TokenType.FALSE },
        { "for", TokenType.FOR },
        { "fun", TokenType.FUN },
        { "if", TokenType.IF },
        { "nil", TokenType.NIL },
        { "or", TokenType.OR },
        { "print", TokenType.PRINT },
        { "return", TokenType.RETURN },
        { "base", TokenType.SUPER },
        { "self", TokenType.THIS },
        { "true", TokenType.TRUE },
        { "let", TokenType.VAR },
        { "while", TokenType.WHILE },
        { "lambda", TokenType.LAMBDA },
        { "break", TokenType.BREAK },
        { "continue", TokenType.CONTINUE }
    };

    private readonly List<Token> tokens = [];

    private int start = 0;
    private int current = 0;
    private int line = 1;

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(TokenType.EOF, "", null, line));
        return tokens;
    }

    private bool IsAtEnd() => current >= source.Length;

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '[':
                AddToken(TokenType.LEFT_SQUARE); break;
            case ']':
                AddToken(TokenType.RIGHT_SQUARE); break;
            case '(':
                AddToken(TokenType.LEFT_PAREN); break;
            case ')':
                AddToken(TokenType.RIGHT_PAREN); break;
            case '{':
                AddToken(TokenType.LEFT_BRACE); break;
            case '}':
                AddToken(TokenType.RIGHT_BRACE); break;
            case ',':
                AddToken(TokenType.COMMA); break;
            case '.':
                AddToken(TokenType.DOT); break;
            case '^':
                AddToken(TokenType.HAT); break;
            case '%':
                AddToken(TokenType.PERCENT); break;
            case '-':
                AddToken(Match('-') ? TokenType.MINUS_MINUS : TokenType.MINUS); break;
            case '+':
                AddToken(Match('+') ? TokenType.PLUS_PLUS : TokenType.PLUS); break;
            case ';':
                AddToken(TokenType.SEMICOLON); break;
            case '*':
                AddToken(TokenType.STAR); break;
            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd())
                        Advance();
                }
                else
                {
                    AddToken(TokenType.SLASH);
                }

                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                line++;
                break;
            case '"':
                String();
                break;
            default:
                if (IsDigit(c))
                {
                    Number();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    reporter.Error(line, $"Unexpected character '{c}'");
                }
                break;
        }
    }

    private void Identifier()
    {
        while (IsAlphanumeric(Peek()))
            Advance();

        string text = source.Substring(start, current - start);

        TokenType type = keywords.GetValueOrDefault(text, TokenType.IDENTIFIER);

        AddToken(type);
    }

    private void Number()
    {
        while (IsDigit(Peek()))
            Advance();

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();

            while (IsDigit(Peek()))
                Advance();
        }

        AddToken(
            TokenType.NUMBER,
            new LoxObject(double.Parse(source.Substring(start, current - start))));
    }

    private bool IsAlphanumeric(char c) => IsAlpha(c) || IsDigit(c);

    private bool IsDigit(char c) => char.IsDigit(c);

    private bool IsAlpha(char c)
    {
        return c >= 'a' && c <= 'z'
            || c >= 'A' && c <= 'Z'
            || c == '_';
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
                line++;
            Advance();
        }

        if (IsAtEnd())
        {
            reporter.Error(line, "Unterminated string.");
            return;
        }

        Advance();

        string value = source.Substring(start + 1, current - 2 - start);
        AddToken(TokenType.STRING, new LoxObject(value));
    }

    private char PeekNext()
    {
        if (current + 1 > source.Length) return '\0';
        return source[current + 1];
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return source[current];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (source[current] != expected) return false;
        current++;
        return true;
    }

    private char Advance()
    {
        //if (IsAtEnd()) return '\0';
        return source[current++];
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, LoxObject? literal)
    {
        string text = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }
}