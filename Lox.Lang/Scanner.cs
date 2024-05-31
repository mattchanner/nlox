namespace Lox.Lang;

public class Scanner(string source)
{
    private readonly List<Token> tokens = [];

    private int start = 0;
    private int current = 0;
    private int line = 1;

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            this.start = this.current;
            ScanTokens();
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
            case '-': 
                AddToken(TokenType.MINUS); break;
            case '+': 
                AddToken(TokenType.PLUS); break;
            case ';': 
                AddToken(TokenType.SEMICOLON); break;
            case '*': 
                AddToken(TokenType.STAR); break; 
        }
    }

    private char Advance()
    {
        return source[this.current++];
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null)
    }

    private void AddToken(TokenType type, object? literal)
    {

    }
}