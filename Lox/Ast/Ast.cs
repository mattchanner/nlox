using Lox.Parser;
using Lox.Runtime;

namespace Lox.Ast;

public abstract class Stmt
{
    public abstract R Accept<R>(IStmtVisitor<R> visitor);
}

public class ExpressionStmt(Expr expr) : Stmt
{
    public Expr Expr { get; } = expr;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitExpression(this);
    }
}

public class FunctionStmt(Token name, List<Token> parameters, List<Stmt> body) : Stmt
{
    public Token Name { get; } = name;
    public List<Token> Parameters { get; } = parameters;
    public List<Stmt> Body { get; } = body;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitFunction(this);
    }
}

public class IfStmt(Expr condition, Stmt thenBranch, Stmt? elseBranch) : Stmt
{
    public Expr Condition { get; } = condition;
    public Stmt ThenBranch { get; } = thenBranch;
    public Stmt? ElseBranch { get; } = elseBranch;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitIfStmt(this);
    }
}

public class ForStatement(Token keyword, Stmt? initializer, Expr? condition, Stmt? increment, Stmt body) : Stmt
{
    public Token Keyword { get; } = keyword;
    public Stmt? Initializer { get; } = initializer;
    public Expr? Condition { get; } = condition;
    public Stmt? Increment { get; } = increment;
    public Stmt Body { get; } = body;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitFor(this);
    }
}

public class WhileStmt(Expr condition, Stmt body) : Stmt
{
    public Expr Condition { get; } = condition;
    public Stmt Body { get; } = body;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitWhile(this);
    }
}

public class LogicalExpr(Expr left, Token op, Expr right) : Expr
{
    public Expr Left { get; } = left;
    public Token Operator { get; } = op;
    public Expr Right { get; } = right;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitLogical(this);
    }
}

public class BlockStmt(List<Stmt> statements) : Stmt
{
    public List<Stmt> Statements { get; } = statements;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitBlock(this);
    }
}

public class ReturnStmt(Token keyword, Expr? value) : Stmt
{
    public Token Keyword { get; } = keyword;
    public Expr? Value { get; } = value;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitReturn(this);
    }
}

public class BreakStmt(Token keyword) : Stmt
{
    public Token Keyword { get; } = keyword;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitBreak(this);
    }
}

public class ContinueStmt(Token keyword) : Stmt
{
    public Token Keyword { get; } = keyword;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitContinue(this);
    }
}

public class PrintStmt(Expr expression) : Stmt
{
    public Expr Expression { get; } = expression;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitPrint(this);
    }
}

public class VarStmt(Token name, Expr? initializer) : Stmt
{
    public Token Name { get; } = name;
    public Expr? Initializer { get; } = initializer;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitVar(this);
    }
}

public abstract class Expr
{
    public abstract R Accept<R>(IExprVisitor<R> visitor);
}

public class CallExpr(Expr callee, Token paren, List<Expr> arguments) : Expr
{
    public Expr Callee { get; } = callee;
    public Token Paren { get; } = paren;
    public List<Expr> Arguments { get; } = arguments;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitCall(this);
    }
}

public class AssignmentExpr(Token name, Expr value) : Expr
{
    public Token Name { get; } = name;
    public Expr Value { get; } = value;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitAssignment(this);
    }
}

public class BinaryExpr(Expr left, Token op, Expr right) : Expr
{
    public Expr Left { get; } = left;
    public Token Operator { get; } = op;
    public Expr Right { get; } = right;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitBinary(this);
    }
}

public class GroupingExpr(Expr expression) : Expr
{
    public Expr Expression { get; } = expression;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitGrouping(this);
    }
}

public class LiteralExpr(LoxObject? value) : Expr
{
    public LoxObject? Value { get; } = value;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitLiteral(this);
    }
}

public class UnaryExpr(Token op, Expr right) : Expr
{
    public Token Operator { get; } = op;
    public Expr Right { get; } = right;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitUnary(this);
    }
}

public class VariableExpr(Token name) : Expr
{
    public Token Name { get; } = name;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitVariable(this);
    }
}

public class LambdaExpression(List<Token> parameters, List<Stmt> body) : Expr
{
    public List<Token> Parameters { get; } = parameters;
    public List<Stmt> Body { get; } = body;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitLambda(this);
    }
}

public class ClassStmt(Token name, VariableExpr? superclass, List<FunctionStmt> methods) : Stmt
{
    public Token Name { get; } = name;
    public VariableExpr? Superclass { get; } = superclass;
    public List<FunctionStmt> Methods { get; } = methods;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitClass(this);
    }
}

public class GetExpr(Expr obj, Token name) : Expr
{
    public Expr Object { get; } = obj;
    public Token Name { get; } = name;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitGet(this);
    }
}

public class SetExpr(Expr obj, Token name, Expr value) : Expr
{
    public Expr Object { get; } = obj;
    public Token Name { get; } = name;
    public Expr Value { get; } = value;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitSet(this);
    }
}

public class ThisExpr(Token keyword) : Expr
{
    public Token Keyword { get; } = keyword;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitThis(this);
    }
}

public class SuperExpr(Token keyword, Token method) : Expr
{
    public Token Keyword { get; } = keyword;
    public Token Method { get; } = method;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitSuper(this);
    }
}

public class ListExpr(Token token, List<Expr> items) : Expr
{
    public Token Token { get; } = token;
    public List<Expr> Items { get; } = items;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitList(this);
    }
}