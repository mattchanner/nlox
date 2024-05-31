using Lox;
using Lox.Lang;

namespace LoxEditor
{
    internal class AstTreeWriter(TreeView treeView, IReporter reporter) : IExprVisitor<object?>, IStmtVisitor<object?>
    {
        private Stack<TreeNode> nodes = new();

        public TreeNode AddNode(string text)
        {
            TreeNode expressionNode = new(text);
            nodes.Peek().Nodes.Add(expressionNode);
            return expressionNode;
        }

        public void Display(List<Stmt> statements)
        {
            treeView.BeginUpdate();
            this.nodes.Clear();
            treeView.Nodes.Clear();
            TreeNode root = new TreeNode("PROGRAM");
            treeView.Nodes.Add(root);

            this.nodes.Push(root);

            foreach (Stmt? stmt in statements)
            {
                if (stmt != null)
                    stmt.Accept(this);
                else
                    AddNode("NULL!");
            }

            treeView.ExpandAll();
            treeView.EndUpdate();
        }

        public object? VisitAssignment(AssignmentExpr assignment)
        {
            this.nodes.Push(AddNode("ASSIGNMENT"));

            AddNode(assignment.Name.Lexeme);
            
            assignment.Value.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitBinary(BinaryExpr binary)
        {
            this.nodes.Push(AddNode("BINARY"));

            binary.Left.Accept(this);
            AddNode(binary.Operator.Lexeme);
            binary.Right.Accept(this);
            
            return this.nodes.Pop();
        }

        public object? VisitLogical(LogicalExpr logical)
        {
            this.nodes.Push(AddNode("LOGICAL"));
            
            logical.Left.Accept(this);
            AddNode(logical.Operator.Lexeme);
            logical.Right.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitGrouping(GroupingExpr grouping)
        {
            this.nodes.Push(AddNode("GROUPING"));
            
            grouping.Expression.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitLiteral(LiteralExpr literal)
        {
            this.nodes.Push(AddNode("LITERAL"));

            AddNode(reporter.Stringify(literal.Value));

            return this.nodes.Pop();
        }

        public object? VisitUnary(UnaryExpr unary)
        {
            this.nodes.Push(AddNode("UNARY"));

            AddNode(unary.Operator.Lexeme);
            unary.Right.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitVariable(VariableExpr variable)
        {
            this.nodes.Push(AddNode("VARIABLE"));

            AddNode(variable.Name.Lexeme);

            return this.nodes.Pop();
        }

        public object? VisitCall(CallExpr call)
        {
            this.nodes.Push(AddNode("CALL"));

            call.Callee.Accept(this);

            this.nodes.Push(AddNode("ARGUMENTS"));
            foreach (var arg in call.Arguments)
            {
                arg.Accept(this);
            }

            this.nodes.Pop();

            return this.nodes.Pop();
        }

        public object? VisitLambda(LambdaExpression lambda)
        {
            this.nodes.Push(AddNode("LAMBDA"));

            if (lambda.Parameters.Any())
            {
                this.nodes.Push(AddNode("PARAMETERS"));
                foreach (Token p in lambda.Parameters)
                {
                    AddNode(p.Lexeme);
                }

                this.nodes.Pop();
            }
            
            this.nodes.Push(AddNode("BODY"));
            foreach (var stmt in lambda.Body)
                stmt.Accept(this);

            this.nodes.Pop();

            return this.nodes.Pop();
        }

        public object? VisitGet(GetExpr getExpr)
        {
            this.nodes.Push(AddNode("GET"));

            AddNode(getExpr.Name.Lexeme);
            getExpr.Object.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitSet(SetExpr setExpr)
        {
            this.nodes.Push(AddNode("SET"));

            AddNode(setExpr.Name.Lexeme);
            setExpr.Object.Accept(this);
            setExpr.Value.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitThis(ThisExpr thisExpr)
        {
            this.nodes.Push(AddNode("THIS"));
            return this.nodes.Pop();
        }

        public object? VisitSuper(SuperExpr superExpr)
        {
            this.nodes.Push(AddNode("SUPER"));
            return this.nodes.Pop();
        }

        public object? VisitList(ListExpr listExpr)
        {
            this.nodes.Push(AddNode("LIST"));
            foreach (var expr in listExpr.Items)
            {
                expr.Accept(this);
            }
            return this.nodes.Pop();
        }

        public object? VisitReturn(ReturnStmt returnStatement)
        {
            this.nodes.Push(AddNode("RETURN"));
            if (returnStatement.Value != null)
                returnStatement.Value.Accept(this);
            return this.nodes.Pop();
        }

        public object? VisitFunction(FunctionStmt function)
        {
            this.nodes.Push(AddNode("FUNCTION"));
            AddNode(function.Name.Lexeme);
            
            this.nodes.Push(AddNode("PARAMETERS"));
            foreach (Token p in function.Parameters)
            {
                AddNode(p.Lexeme);
            }
            this.nodes.Pop();

            this.nodes.Push(AddNode("BODY"));
            foreach (var stmt in function.Body)
                stmt.Accept(this);

            this.nodes.Pop();

            return this.nodes.Pop();
        }

        public object? VisitIfStmt(IfStmt ifStatement)
        {
            this.nodes.Push(AddNode("IF"));
            
            this.nodes.Push(AddNode("CONDITION"));
            ifStatement.Condition.Accept(this);
            this.nodes.Pop();

            this.nodes.Push(AddNode("THEN"));
            ifStatement.ThenBranch.Accept(this);
            this.nodes.Pop();

            if (ifStatement.ElseBranch != null)
            {
                this.nodes.Push(AddNode("ELSE"));
                ifStatement.ElseBranch.Accept(this);
                this.nodes.Pop();
            }

            return this.nodes.Pop();
        }

        public object? VisitWhile(WhileStmt whileStatement)
        {
            this.nodes.Push(AddNode("WHILE"));

            whileStatement.Condition.Accept(this);
            whileStatement.Body.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitBlock(BlockStmt block)
        {
            this.nodes.Push(AddNode("BLOCK"));

            foreach (var stmt in block.Statements)
                stmt.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitVar(VarStmt var)
        {
            this.nodes.Push(AddNode("VAR"));
            
            AddNode(var.Name.Lexeme);

            if (var.Initializer != null)
                var.Initializer.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitPrint(PrintStmt print)
        {
            this.nodes.Push(AddNode("PRINT"));

            print.Expression.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitExpression(ExpressionStmt expression)
        {
            this.nodes.Push(AddNode("EXPRESSION"));

            expression.Expr.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitClass(ClassStmt @class)
        {
            this.nodes.Push(AddNode("CLASS"));

            AddNode(@class.Name.Lexeme);

            if (@class.Superclass != null)
            {
                this.nodes.Push(AddNode("SUPERCLASS"));
                
                @class.Superclass.Accept(this);

                this.nodes.Pop();
            }

            this.nodes.Push(AddNode("METHODS"));
            
            foreach (var method in @class.Methods)
            {
                method.Accept(this);
            }

            this.nodes.Pop();

            return this.nodes.Pop();
        }
    }
}
