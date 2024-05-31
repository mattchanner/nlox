using System.Linq.Expressions;
using Lox.Lang;

namespace Lox;

public class Parser(List<Token> tokens, IReporter reporter)
{
    private int current;

    public List<Stmt> Parse()
    {
        List<Stmt> statements = [];

        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.CLASS)) return ClassDeclaration();
            if (Match(TokenType.FUN)) return Function("function");
            if (Match(TokenType.VAR)) return VarDeclaration();

            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt ClassDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect class name.");

        VariableExpr? superclass = null;
        if (Match(TokenType.LESS))
        {
            Consume(TokenType.IDENTIFIER, "Expect superclass name.");
            superclass = new VariableExpr(Previous());
        }

        Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");
        List<FunctionStmt> methods = [];
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add(Function("method"));
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

        return new ClassStmt(name, superclass, methods);
    }

    private FunctionStmt Function(string kind)
    {
        Token? name = null;
        if (Check(TokenType.IDENTIFIER))
        {
            name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
        }

        Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");

        List<Token> parameters = [];
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    reporter.Error(Peek(), "Can't have more than 255 parameters");
                }

                parameters.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name."));

            } while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters");
        Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
        List<Stmt> body = Block();

        return new FunctionStmt(name, parameters, body);
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expr? initializer = null;
        if (Match(TokenType.EQUAL))
            initializer = Expression();

        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

        return new VarStmt(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.FOR)) return ForStatement();
        if (Match(TokenType.IF)) return IfStatement();
        if (Match(TokenType.WHILE)) return WhileStatement();
        if (Match(TokenType.PRINT)) return PrintStatement();
        if (Match(TokenType.RETURN)) return ReturnStatement();
        if (Match(TokenType.LEFT_BRACE)) return new BlockStmt(Block());

        return ExpressionStatement();
    }

    private Stmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;

        if (!Check(TokenType.SEMICOLON))
        {
            value = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after return value.");

        return new ReturnStmt(keyword, value);
    }

    /// <summary>
    /// Rewrites a for loop into a while loop.
    /// <example>
    /// <para>The following Lox code</para>
    /// <code>
    /// for (var x = 0; x &lt; 10; x = x + 1) {
    ///    print x;
    /// }
    /// </code>
    /// <para> is rewritten to a while loop</para>
    /// <code>
    /// var x = 0;
    /// while (x &lt; 10) {
    ///     print x;
    ///     x = x + 1;
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <returns>The statement list</returns>
    private Stmt ForStatement()
    {
        
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

        Stmt? initializer;
        if (Match(TokenType.SEMICOLON))
            initializer = null;
        else if (Match(TokenType.VAR))
            initializer = VarDeclaration();
        else
            initializer = ExpressionStatement();

        Expr? condition = null;
        if (!Check(TokenType.SEMICOLON))
            condition = Expression();

        Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(TokenType.RIGHT_PAREN))
            increment = Expression();

        Consume(TokenType.RIGHT_PAREN, "Exepcted ')' after clauses.");

        Stmt body = Statement();

        if (increment != null)
        {
            body = new BlockStmt([body, new ExpressionStmt(increment)]);
        }

        if (condition == null)
            condition = new LiteralExpr(LoxObject.True);

        body = new WhileStmt(condition, body);

        if (initializer != null)
            body = new BlockStmt([initializer, body]);

        return body;
    }

    private Stmt IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

        Stmt thenBranch = Statement();
        Stmt? elseBranch = null;
        if (Match(TokenType.ELSE))
        {
            elseBranch = Statement();
        }

        return new IfStmt(condition, thenBranch, elseBranch);
    }

    private Stmt WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        Expr condition = Expression();
        
        Consume(TokenType.RIGHT_PAREN, "Exepect ')' after condition");
        Stmt body = Statement();

        return new WhileStmt(condition, body);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = [];

        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Stmt PrintStatement()
    {
        Expr value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new PrintStmt(value);
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new ExpressionStmt(expr);
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        Expr expr = Or();
        
        if (Match(TokenType.EQUAL))
        {
            Token equals = Previous();
            Expr value = Assignment();

            if (expr is VariableExpr variable)
            {
                Token name = variable.Name;
                return new AssignmentExpr(name, value);
            }
            else if (expr is GetExpr getExpr)
            {
                return new SetExpr(getExpr.Object, getExpr.Name, value);
            }

            reporter.Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();

        while (Match(TokenType.OR))
        {
            Token op = Previous();
            Expr right = And();
            expr = new LogicalExpr(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        Expr expr = Equality();

        while (Match(TokenType.AND))
        {
            Token op = Previous();
            Expr right = Equality();
            expr = new LogicalExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        Expr expr = Comparison();
        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            Token op = Previous();
            Expr right = Comparison();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            Token op = Previous();
            Expr right = Term();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            Token op = Previous();
            Expr right = Factor();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Power();

        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            Token op = Previous();
            Expr right = Power();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Power()
    {
        Expr expr = Modulus();

        while (Match(TokenType.HAT))
        {
            Token op = Previous();
            Expr right = Modulus();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Modulus()
    {
        Expr expr = Unary();

        while (Match(TokenType.PERCENT))
        {
            Token op = Previous();
            Expr right = Unary();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS, TokenType.PLUS_PLUS, TokenType.MINUS_MINUS))
        {
            Token op = Previous();
            Expr right = Unary();
            return new UnaryExpr(op, right);
        }

        return Call();
    }

    private Expr Call()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.DOT))
            {
                Token name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                expr = new GetExpr(expr, name);
            }
            else
                break;
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = [];
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    reporter.Error(Peek(), "Can't have move than 255 arguments.");
                }

                arguments.Add(Expression());
            } while (Match(TokenType.COMMA));
        }

        Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
        return new CallExpr(callee, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(TokenType.FALSE)) return new LiteralExpr(LoxObject.False);
        if (Match(TokenType.TRUE)) return new LiteralExpr(LoxObject.True);
        if (Match(TokenType.NIL)) return new LiteralExpr(LoxObject.Nil);

        if (Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new LiteralExpr(Previous().Literal);
        }

        if (Match(TokenType.SUPER))
        {
            Token keyword = Previous();
            Consume(TokenType.DOT, "Expect '.' after 'super'.");
            Token method = Consume(TokenType.IDENTIFIER, "Expect superclass method name");
            return new SuperExpr(keyword, method);
        }

        if (Match(TokenType.THIS)) return new ThisExpr(Previous());

        if (Match(TokenType.IDENTIFIER))
        {
            Token identifier = Previous();
            if (Match(TokenType.PLUS_PLUS, TokenType.MINUS_MINUS))
            {
                Token op = Previous();
                return new UnaryExpr(op, new VariableExpr(identifier));
            }

            return new VariableExpr(identifier);
        }

        if (Match(TokenType.LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new GroupingExpr(expr);
        }

        if (Match(TokenType.LAMBDA))
            return Lambda();

        if (Match(TokenType.LEFT_SQUARE))
            return List();

        throw reporter.Error(Peek(), "Expected an expression.");
    }

    private Expr List()
    {
        Token opening = Previous();
        List<Expr> items = [];
        if (!Check(TokenType.RIGHT_SQUARE))
        {

            do
            {
                items.Add(Expression());
            } while (Match(TokenType.COMMA) && !IsAtEnd());
        }

        Consume(TokenType.RIGHT_SQUARE, "Expect ']' after list.");

        return new ListExpr(opening, items);
    }

    private Expr Lambda()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'lambda'.");
        List<Token> parameters = [];
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 10)
                {
                    reporter.Error(Peek(), "Can#t have more than 10 parameters.");
                }

                parameters.Add(Consume(TokenType.IDENTIFIER, "Expected a parameter name."));

            } while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameter list.");
        Consume(TokenType.LEFT_BRACE, "Expected '{' before lambda body.");

        List<Stmt> body = Block();

        return new LambdaExpression(parameters, body);
    }

    private void Synchronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.SEMICOLON) 
                return;

            switch (Peek().Type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }

            Advance();
        }
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw reporter.Error(Peek(), message);
    }

    private bool Match(params TokenType[] tokensToMatch)
    {
        if (tokensToMatch.Any(Check))
        {
            Advance();
            return true;
        }

        return false;
    }

    private bool Check(TokenType token)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == token;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
            current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
        return tokens[current];
    }

    private Token Previous()
    {
        return tokens[current - 1];
    }
}