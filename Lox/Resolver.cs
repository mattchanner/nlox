﻿using Lox.Lang;

namespace Lox;

public class Resolver(Interpreter interpreter, IReporter reporter) : IExprVisitor<object?>, IStmtVisitor<object?>
{
    private readonly Stack<Dictionary<string, bool>> scopes = new();
    private FunctionType currentFunction = FunctionType.None;
    private ClassType currentClass = ClassType.None;

    public object? VisitAssignment(AssignmentExpr assignment)
    {
        Resolve(assignment.Value);
        ResolveLocal(assignment, assignment.Name);

        return null;
    }

    public object? VisitBinary(BinaryExpr binary)
    {
        Resolve(binary.Left);
        Resolve(binary.Right);
        return null;
    }

    public object? VisitLogical(LogicalExpr logical)
    {
        Resolve(logical.Left);
        Resolve(logical.Right);
        return null;
    }

    public object? VisitGrouping(GroupingExpr grouping)
    {
        Resolve(grouping.Expression);
        return null;
    }

    public object? VisitLiteral(LiteralExpr literal)
    {
        return null;
    }

    public object? VisitUnary(UnaryExpr unary)
    {
        Resolve(unary.Right);
        return null;
    }

    public object? VisitVariable(VariableExpr variable)
    {
        // Check to see if the variable is being accessed inside its own initializer
        if (this.scopes.Count > 0)
        {
            Dictionary<string, bool> peeked = this.scopes.Peek();
            if (peeked.TryGetValue(variable.Name.Lexeme, out bool isInitialized) && !isInitialized)
                reporter.Error(variable.Name.Line, "Can't read local variable in its own initializer.");
        }

        ResolveLocal(variable, variable.Name);

        return null;
    }

    public object? VisitCall(CallExpr call)
    {
        Resolve(call.Callee);

        foreach (Expr argument in call.Arguments)
            Resolve(argument);
        
        return null;
    }

    public object? VisitLambda(LambdaExpression lambda)
    {
        ResolveLambda(lambda);
        return null;
    }

    public object? VisitGet(GetExpr getExpr)
    {
        Resolve(getExpr.Object);
        return null;
    }

    public object? VisitSet(SetExpr setExpr)
    {
        Resolve(setExpr.Value);
        Resolve(setExpr.Object);

        return null;
    }

    public object? VisitThis(ThisExpr thisExpr)
    {
        if (this.currentClass != ClassType.Class)
            reporter.Error(thisExpr.Keyword, "Can't use 'this' outside of a class.");

        ResolveLocal(thisExpr, thisExpr.Keyword);

        return null;
    }

    public object? VisitSuper(SuperExpr superExpr)
    {
        if (this.currentClass == ClassType.None)
        {
            reporter.Error(superExpr.Keyword, "Can't use 'super' outside of a class.");
        } 
        else if (this.currentClass != ClassType.Subclass)
        {
            reporter.Error(superExpr.Keyword, "Can't use 'super' in a class with no subclass.");
        }

        ResolveLocal(superExpr, superExpr.Keyword);
        return null;
    }

    public object? VisitList(ListExpr listExpr)
    {
        foreach (Expr expr in listExpr.Items)
            Resolve(expr);

        return null;
    }

    public object? VisitReturn(ReturnStmt returnStatement)
    {
        if (this.currentFunction == FunctionType.None)
        {
            reporter.Error(returnStatement.Keyword.Line, "Can't return from top-level code.");
            return null;
        }

        if (returnStatement.Value != null)
        {
            if (this.currentFunction == FunctionType.Initializer)
            {
                reporter.Error(returnStatement.Keyword, "Can't return a value from an initializer");
            }

            Resolve(returnStatement.Value);
        }

        return null;
    }

    public object? VisitFunction(FunctionStmt function)
    {
        Declare(function.Name);
        Define(function.Name);

        ResolveFunction(function, FunctionType.Function);
        
        return null;
    }

    public object? VisitIfStmt(IfStmt ifStatement)
    {
        Resolve(ifStatement.Condition);
        Resolve(ifStatement.ThenBranch);
        
        if (ifStatement.ElseBranch != null)
            Resolve(ifStatement.ElseBranch);

        return null;
    }

    public object? VisitWhile(WhileStmt whileStatement)
    {
        Resolve(whileStatement.Condition);
        Resolve(whileStatement.Body);

        return null;
    }

    public object? VisitBlock(BlockStmt block)
    {
        BeginScope();
        Resolve(block.Statements);
        EndScope();

        return null;
    }

    public object? VisitVar(VarStmt var)
    {
        Declare(var.Name);

        if (var.Initializer != null)
        {
            Resolve(var.Initializer);
        }

        Define(var.Name);
        
        return null;
    }

    public object? VisitPrint(PrintStmt print)
    {
        Resolve(print.Expression);
        return null;
    }

    public object? VisitExpression(ExpressionStmt expression)
    {
        Resolve(expression.Expr);
        return null;
    }

    public object? VisitClass(ClassStmt @class)
    {
        ClassType enclosingClass = currentClass;
        currentClass = ClassType.Class;

        Declare(@class.Name);
        Define(@class.Name);

        if (@class.Superclass != null &&
            @class.Name.Lexeme == @class.Superclass.Name.Lexeme)
        {
            reporter.Error(@class.Superclass.Name, "A class can't inherit from itself.");
        }

        if (@class.Superclass != null)
        {
            this.currentClass = ClassType.Subclass;

            Resolve(@class.Superclass);

            BeginScope();
            this.scopes.Peek()["super"] = true;
        }

        BeginScope();

        this.scopes.Peek().Add("this", true);

        foreach (FunctionStmt func in @class.Methods)
        {
            FunctionType funcType = FunctionType.Method;
            
            if (func.Name.Lexeme == "init")
            {
                funcType = FunctionType.Initializer;
            }

            ResolveFunction(func, funcType);
        }

        EndScope();

        if (@class.Superclass != null)
            EndScope();

        this.currentClass = enclosingClass;
        return null;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (Stmt statement in statements)
            Resolve(statement);
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expr expr) {
        expr.Accept(this);
    }

    private void ResolveFunction(FunctionStmt function, FunctionType functionType)
    {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = functionType;

        BeginScope();

        foreach (Token param in function.Parameters)
        {
            Declare(param);
            Define(param);
        }

        Resolve(function.Body);

        EndScope();

        currentFunction = enclosingFunction;
    }

    private void ResolveLambda(LambdaExpression lambda)
    {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = FunctionType.Function;

        BeginScope();

        foreach (Token param in lambda.Parameters)
        {
            Declare(param);
            Define(param);
        }

        Resolve(lambda.Body);

        EndScope();

        this.currentFunction = enclosingFunction;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        int steps = 0;
        //for (int i = scopes.Count - 1; i >= 0; i--)
        for (int i = 0; i < scopes.Count; i++)
        {
            if (this.scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, i);
                return;
            }

            steps++;
        }
    }

    private void BeginScope()
    {
        scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        this.scopes.Pop();
    }

    private void Declare(Token name)
    {
        if (this.scopes.Count == 0) return;

        Dictionary<string, bool> scope = this.scopes.Peek();

        if (scope.ContainsKey(name.Lexeme))
        {
            reporter.Error(name.Line, "A variable with this name already exists in this scope.");
        }

        scope[name.Lexeme] = false;
    }

    private void Define(Token name)
    {
        if (this.scopes.Count == 0) return;

        Dictionary<string, bool> scope = this.scopes.Peek();
        scope[name.Lexeme] = true;
    }
}