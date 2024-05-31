using Lox.Lang;

namespace Lox;

public class Interpreter : IExprVisitor<LoxObject?>, IStmtVisitor<LoxObject?>
{
    private readonly IReporter reporter;
    internal readonly Environment globals = new();
    internal readonly Dictionary<Expr, int> locals = [];
    private Environment environment;

    public Interpreter(IReporter reporter)
    {
        this.reporter = reporter;
        this.environment = globals;
        this.globals.Define("clock", new LoxObject(new Clock()));
        this.globals.Define("Math", new LoxObject(new LoxNativeInstance(typeof(Math))));
    }

    public void Reset()
    {
        this.globals.Reset();
        this.locals.Clear();
        this.environment = this.globals;
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt stmt in statements)
            {
                Execute(stmt);
            }
        }
        catch (RuntimeError err)
        {
            reporter.RuntimeError(err);
        }
    }

    public void Execute(Stmt stmt) => stmt.Accept(this);

    public LoxObject? Evaluate(Expr expr) => expr.Accept(this);

    public LoxObject? VisitAssignment(AssignmentExpr assignment)
    {
        LoxObject? value = Evaluate(assignment.Value);

        if (this.locals.TryGetValue(assignment, out int distance))
        {
            this.environment.AssignAt(distance, assignment.Name, value);
        }
        else
        {
            this.globals.Assign(assignment.Name, value);
        }
        
        return value;
    }

    public LoxObject? VisitBinary(BinaryExpr binary)
    {
        LoxObject? left = Evaluate(binary.Left);
        LoxObject? right = Evaluate(binary.Right);

        switch (binary.Operator.Type)
        {
            case TokenType.MINUS:
                return left! - right!;

            case TokenType.SLASH:
                return left! / right!;
            
            case TokenType.STAR:
                return left! * right!;
            
            case TokenType.PLUS:
                return left! + right!;

            case TokenType.HAT:
                return left!.Power(right!);

            case TokenType.PERCENT:
                return left!.Modulus(right!);
            
            case TokenType.GREATER:
                return new LoxObject(left! > right!);
            
            case TokenType.GREATER_EQUAL:
                return new LoxObject(left! >= right!);
            
            case TokenType.LESS:
                return new LoxObject(left! < right!);
            
            case TokenType.LESS_EQUAL:
                return new LoxObject(left! <= right!);
            
            case TokenType.BANG_EQUAL:
                return new LoxObject(left! != right!);
            
            case TokenType.EQUAL_EQUAL:
                return new LoxObject(left! == right!);
            
            default:
                return null;
        }
    }

    public LoxObject? VisitGrouping(GroupingExpr grouping) 
        => Evaluate(grouping.Expression);

    public LoxObject? VisitLiteral(LiteralExpr literal) => literal.Value!;

    public LoxObject? VisitReturn(ReturnStmt returnStatement)
    {
        LoxObject? value = null;
        if (returnStatement.Value != null)
            value = Evaluate(returnStatement.Value);

        throw new ReturnException(value);
    }

    public LoxObject? VisitFunction(FunctionStmt function)
    {
        LoxFunction func = new(function, this.environment, false);
        this.environment.Define(function.Name.Lexeme, new LoxObject(func));
        return null;
    }

    public LoxObject? VisitIfStmt(IfStmt ifStatement)
    {
        LoxObject? condition = Evaluate(ifStatement.Condition);
        if (condition!.IsTruthy())
            Execute(ifStatement.ThenBranch);
        else if (ifStatement.ElseBranch != null)
            Execute(ifStatement.ElseBranch);

        return null;
    }

    public LoxObject? VisitWhile(WhileStmt whileStatement)
    {
        while (Evaluate(whileStatement.Condition)?.IsTruthy() ?? false)
        {
            Execute(whileStatement.Body);
        }

        return null;
    }

    public LoxObject? VisitLogical(LogicalExpr logical)
    {
        LoxObject? leftResult = Evaluate(logical.Left);
        if (logical.Operator.Type == TokenType.OR)
        {
            if (leftResult!.IsTruthy())
                return leftResult;
        }
        else 
        {
            if (!leftResult!.IsTruthy())
                return leftResult;
        }

        return Evaluate(logical.Right);
    }

    public LoxObject? VisitBlock(BlockStmt block)
    {
        ExecuteBlock(block.Statements, new Environment(environment));
        return null;
    }

    public LoxObject? VisitVar(VarStmt var)
    {
        LoxObject? value = null;
        if (var.Initializer != null)
        {
            value = Evaluate(var.Initializer);
        }

        environment.Define(var.Name.Lexeme, value);
        return null;
    }

    public LoxObject? VisitPrint(PrintStmt print)
    {
        object? result = Evaluate(print.Expression);
        Console.WriteLine(reporter.Stringify(result));
        return null;
    }

    public LoxObject? VisitExpression(ExpressionStmt expression)
    {
        return Evaluate(expression.Expr);
    }

    public LoxObject? VisitClass(ClassStmt @class)
    {
        LoxObject? superclass = null;
        if (@class.Superclass != null)
        {
            superclass = Evaluate(@class.Superclass);
            if (!(superclass?.IsClass ?? false))
            {
                throw new RuntimeError(@class.Superclass.Name, "Superclass must be a class.");
            }
        }

        this.environment.Define(@class.Name.Lexeme, null);

        if (@class.Superclass != null)
        {
            this.environment = new(environment);
            this.environment.Define("super", superclass);
        }

        Dictionary<string, LoxFunction> methods = new();
        foreach (FunctionStmt method in @class.Methods)
        {
            LoxFunction func = new(method, this.environment, method.Name.Lexeme == "init");
            methods.Add(method.Name.Lexeme, func);
        }

        LoxClass klass = new LoxClass(@class.Name.Lexeme, superclass, methods);

        if (superclass != null)
        {
            this.environment = this.environment.Enclosing!;
        }

        this.environment.Assign(@class.Name, new LoxObject(klass));

        return null;
    }

    public LoxObject? VisitUnary(UnaryExpr unary)
    {
        LoxObject? right = Evaluate(unary.Right);

        switch (unary.Operator.Type)
        {
            case TokenType.MINUS:
                return -right;
            case TokenType.MINUS_MINUS:
                return --right;
            case TokenType.PLUS_PLUS:
                return ++right;
            case TokenType.BANG:
                return !right!;
        }

        return null;
    }

    public LoxObject? VisitVariable(VariableExpr variable)
    {
        return LookUpVariable(variable.Name, variable);
    }

    public LoxObject? VisitCall(CallExpr expr)
    {
        LoxObject callee = Evaluate(expr.Callee)!;
        ILoxCallable? function = callee.AsCallable();

        if (function == null)
            throw new RuntimeError(expr.Paren, "Can only call functions and classes.");

        if (function.Arity != expr.Arguments.Count)
            throw new RuntimeError(expr.Paren, $"Expected {function.Arity} arguments but got {expr.Arguments.Count}.");

        List<LoxObject?> arguments = expr.Arguments.Select(Evaluate).ToList();
        return function.Call(this, arguments);

    }

    public LoxObject? VisitLambda(LambdaExpression lambda)
    {
        LambdaFunction func = new(lambda, this.environment);
        return new LoxObject(func);
    }

    public LoxObject? VisitGet(GetExpr getExpr)
    {
        LoxObject? obj = Evaluate(getExpr.Object);

        if (obj.IsInstance)
            return obj.GetLoxInstance().Get(getExpr.Name);
        
        if (obj.IsNativeInstance)
            return obj.GetNativeInstance().Get(getExpr.Name);
        if (obj.IsList)
            return obj.GetList().Get(getExpr.Name);

        throw new RuntimeError(getExpr.Name, "Only instances have properties");
    }

    public LoxObject? VisitSet(SetExpr setExpr)
    {
        LoxObject? instance = Evaluate(setExpr.Object);

        if (!(instance?.IsInstance ?? false))
            throw new RuntimeError(setExpr.Name, "Only instances have fields.");

        LoxObject? value = Evaluate(setExpr.Value);
        instance!.GetLoxInstance().Set(setExpr.Name, value);
        return null;
    }

    public LoxObject? VisitThis(ThisExpr thisExpr)
    {
        return LookUpVariable(thisExpr.Keyword, thisExpr);
    }

    public LoxObject? VisitSuper(SuperExpr superExpr)
    {
        int distance = this.locals[superExpr];
        LoxClass? superclass = this.environment.GetAt(distance, "super")?.AsClass();

        LoxInstance instance = this.environment.GetAt(distance - 1, "this")!.AsLoxInstance()!;
        LoxFunction? method = superclass!.FindMethod(superExpr.Method.Lexeme);

        if (method == null) {
            throw new RuntimeError(superExpr.Method,
                "Undefined property '" + superExpr.Method.Lexeme + "'.");
        }

        return new LoxObject(method!.Bind(instance!));
    }

    public LoxObject? VisitList(ListExpr listExpr)
    {
        List<LoxObject> items = [];

        foreach (Expr expr in listExpr.Items)
        {
            LoxObject? value = Evaluate(expr);
            items.Add(value ?? LoxObject.Nil);
        }

        return new LoxObject(items);
    }

    public void ExecuteBlock(List<Stmt> statements, Environment env)
    {
        Environment previous = this.environment;

        try
        {
            this.environment = env;
            foreach (Stmt stmt in statements)
            {
                Execute(stmt);
            }
        }
        finally
        {
            this.environment = previous;
        }
    }

    public void Resolve(Expr expr, int depth)
    {
        this.locals.Add(expr, depth);
    }

    private LoxObject? LookUpVariable(Token name, Expr expr) 
    {
        if (this.locals.TryGetValue(expr, out int depth))
        {
            return this.environment.GetAt(depth, name.Lexeme);
        }
    
        return this.globals.Get(name);
    }
}