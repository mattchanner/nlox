namespace Lox.Lang;

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
        { "super", TokenType.SUPER },
        { "this", TokenType.THIS },
        { "true", TokenType.TRUE },
        { "var", TokenType.VAR },
        { "while", TokenType.WHILE },
        { "lambda", TokenType.LAMBDA }
    };

    private readonly List<Token> tokens = [];

    private int start = 0;
    private int current = 0;
    private int line = 1;

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            this.start = this.current;
            ScanToken();
        }

        this.tokens.Add(new Token(TokenType.EOF, "", null, line));
        return this.tokens;
    }

    private bool IsAtEnd() => this.current >= source.Length;

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
                this.line++;
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
                    reporter.Error(this.line, $"Unexpected character '{c}'");    
                }
                break;
        }
    }

    private void Identifier()
    {
        while (IsAlphanumeric(Peek()))
            Advance();

        string text = source.Substring(this.start, this.current - this.start);

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
            new LoxObject(double.Parse(source.Substring(this.start, this.current - this.start))));
    }

    private bool IsAlphanumeric(char c) => IsAlpha(c) || IsDigit(c);

    private bool IsDigit(char c) => char.IsDigit(c);

    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z')
            || (c >= 'A' && c <= 'Z')
            || c == '_';
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
                this.line++;
            Advance();
        }

        if (IsAtEnd())
        {
            reporter.Error(this.line, "Unterminated string.");
            return;
        }

        Advance();

        string value = source.Substring(this.start + 1, this.current - 2 - this.start);
        AddToken(TokenType.STRING, new LoxObject(value));
    }

    private char PeekNext()
    {
        if (this.current + 1 > source.Length) return '\0';
        return source[this.current + 1];
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return source[this.current];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (source[current] != expected) return false;
        this.current++;
        return true;
    }

    private char Advance()
    {
        //if (IsAtEnd()) return '\0';
        return source[this.current++];
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, LoxObject? literal)
    {
        string text = source.Substring(start, current - start);
        this.tokens.Add(new Token(type, text, literal, line));
    }
}