namespace Lox;

public interface IExprVisitor<R>
{
    R VisitAssignment(AssignmentExpr assignment);
    R VisitBinary(BinaryExpr binary);
    R VisitLogical(LogicalExpr logical);
    R VisitGrouping(GroupingExpr grouping);
    R VisitLiteral(LiteralExpr literal);
    R VisitUnary(UnaryExpr unary);
    R VisitVariable(VariableExpr variable);
    R VisitCall(CallExpr call);
    R VisitLambda(LambdaExpression lambda);
    R VisitGet(GetExpr getExpr);
    R VisitSet(SetExpr setExpr);
    R VisitThis(ThisExpr thisExpr);
    R VisitSuper(SuperExpr superExpr);
    R VisitList(ListExpr listExpr);
}

public interface IStmtVisitor<R>
{
    R VisitReturn(ReturnStmt returnStatement);
    R VisitFunction(FunctionStmt function);
    R VisitIfStmt(IfStmt ifStatement);
    R VisitWhile(WhileStmt whileStatement);
    R VisitBlock(BlockStmt block);
    R VisitVar(VarStmt var);
    R VisitPrint(PrintStmt print);
    R VisitExpression(ExpressionStmt expression);
    R VisitClass(ClassStmt @class);
}