using Lox.Ast;
using Lox.Parser;
using Lox.Runtime;

namespace LoxEditor
{
    internal class AstTreeWriter(TreeView treeView, IReporter reporter) : IExprVisitor<object?>, IStmtVisitor<object?>
    {
        private static readonly Color KeywordColour = Color.BlueViolet;
        private static readonly Color NonKeywordColour = Color.DimGray;
        private static readonly Color ErrorColour = Color.DarkRed;

        private Stack<TreeNode> nodes = new();

        public TreeNode AddNode(string text, Color colour)
        {
            TreeNode expressionNode = new(text);
            expressionNode.ForeColor = colour;
            nodes.Peek().Nodes.Add(expressionNode);
            return expressionNode;
        }

        public void Display(List<Stmt> statements)
        {
            treeView.BeginUpdate();
            this.nodes.Clear();
            treeView.Nodes.Clear();
            TreeNode root = new TreeNode("PROGRAM");
            root.ForeColor = KeywordColour;
            treeView.Nodes.Add(root);

            this.nodes.Push(root);

            foreach (Stmt? stmt in statements)
            {
                if (stmt != null)
                    stmt.Accept(this);
                else
                    AddNode("NULL!", ErrorColour);
            }

            treeView.ExpandAll();
            treeView.EndUpdate();
        }

        public object? VisitAssignment(AssignmentExpr assignment)
        {
            this.nodes.Push(AddNode("ASSIGNMENT", KeywordColour));

            AddNode(assignment.Name.Lexeme, NonKeywordColour);
            
            assignment.Value.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitBinary(BinaryExpr binary)
        {
            this.nodes.Push(AddNode("BINARY", KeywordColour));

            binary.Left.Accept(this);
            AddNode(binary.Operator.Lexeme, NonKeywordColour);
            binary.Right.Accept(this);
            
            return this.nodes.Pop();
        }

        public object? VisitLogical(LogicalExpr logical)
        {
            this.nodes.Push(AddNode("LOGICAL", KeywordColour));
            
            logical.Left.Accept(this);
            AddNode(logical.Operator.Lexeme, NonKeywordColour);
            logical.Right.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitGrouping(GroupingExpr grouping)
        {
            this.nodes.Push(AddNode("GROUPING", KeywordColour));
            
            grouping.Expression.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitLiteral(LiteralExpr literal)
        {
            this.nodes.Push(AddNode("LITERAL", KeywordColour));

            AddNode(reporter.Stringify(literal.Value), NonKeywordColour);

            return this.nodes.Pop();
        }

        public object? VisitUnary(UnaryExpr unary)
        {
            this.nodes.Push(AddNode("UNARY", KeywordColour));

            AddNode(unary.Operator.Lexeme, NonKeywordColour);
            unary.Right.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitVariable(VariableExpr variable)
        {
            this.nodes.Push(AddNode("VARIABLE", KeywordColour));

            AddNode(variable.Name.Lexeme, NonKeywordColour);

            return this.nodes.Pop();
        }

        public object? VisitCall(CallExpr call)
        {
            this.nodes.Push(AddNode("CALL", KeywordColour));

            call.Callee.Accept(this);

            this.nodes.Push(AddNode("ARGUMENTS", KeywordColour));
            foreach (var arg in call.Arguments)
            {
                arg.Accept(this);
            }

            this.nodes.Pop();

            return this.nodes.Pop();
        }

        public object? VisitLambda(LambdaExpression lambda)
        {
            this.nodes.Push(AddNode("LAMBDA", KeywordColour));

            if (lambda.Parameters.Any())
            {
                this.nodes.Push(AddNode("PARAMETERS", KeywordColour));
                foreach (Token p in lambda.Parameters)
                {
                    AddNode(p.Lexeme, NonKeywordColour);
                }

                this.nodes.Pop();
            }
            
            this.nodes.Push(AddNode("BODY", KeywordColour));
            foreach (var stmt in lambda.Body)
                stmt.Accept(this);

            this.nodes.Pop();

            return this.nodes.Pop();
        }

        public object? VisitGet(GetExpr getExpr)
        {
            this.nodes.Push(AddNode("GET", KeywordColour));

            AddNode(getExpr.Name.Lexeme, NonKeywordColour);
            getExpr.Object.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitSet(SetExpr setExpr)
        {
            this.nodes.Push(AddNode("SET", KeywordColour));

            AddNode(setExpr.Name.Lexeme, NonKeywordColour);
            setExpr.Object.Accept(this);
            setExpr.Value.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitThis(ThisExpr thisExpr)
        {
            this.nodes.Push(AddNode("SELF", KeywordColour));
            return this.nodes.Pop();
        }

        public object? VisitSuper(SuperExpr superExpr)
        {
            this.nodes.Push(AddNode("BASE", KeywordColour));
            return this.nodes.Pop();
        }

        public object? VisitList(ListExpr listExpr)
        {
            this.nodes.Push(AddNode("LIST", KeywordColour));
            foreach (var expr in listExpr.Items)
            {
                expr.Accept(this);
            }
            return this.nodes.Pop();
        }

        public object? VisitReturn(ReturnStmt returnStatement)
        {
            this.nodes.Push(AddNode("RETURN", KeywordColour));
            if (returnStatement.Value != null)
                returnStatement.Value.Accept(this);
            return this.nodes.Pop();
        }

        public object? VisitFunction(FunctionStmt function)
        {
            this.nodes.Push(AddNode("FUNCTION", KeywordColour));
            AddNode(function.Name.Lexeme, NonKeywordColour);
            
            this.nodes.Push(AddNode("PARAMETERS", KeywordColour));
            foreach (Token p in function.Parameters)
            {
                AddNode(p.Lexeme, NonKeywordColour);
            }
            this.nodes.Pop();

            this.nodes.Push(AddNode("BODY", KeywordColour));
            foreach (var stmt in function.Body)
                stmt.Accept(this);

            this.nodes.Pop();

            return this.nodes.Pop();
        }

        public object? VisitBreak(BreakStmt breakStmt)
        {
            return AddNode("BREAK", KeywordColour);
        }

        public object? VisitContinue(ContinueStmt continueStmt)
        {
            return AddNode("CONTINUE", KeywordColour);
        }

        public object? VisitIfStmt(IfStmt ifStatement)
        {
            this.nodes.Push(AddNode("IF", KeywordColour));
            
            this.nodes.Push(AddNode("CONDITION", KeywordColour));
            ifStatement.Condition.Accept(this);
            this.nodes.Pop();

            this.nodes.Push(AddNode("THEN", KeywordColour));
            ifStatement.ThenBranch.Accept(this);
            this.nodes.Pop();

            if (ifStatement.ElseBranch != null)
            {
                this.nodes.Push(AddNode("ELSE", KeywordColour));
                ifStatement.ElseBranch.Accept(this);
                this.nodes.Pop();
            }

            return this.nodes.Pop();
        }

        public object? VisitWhile(WhileStmt whileStatement)
        {
            this.nodes.Push(AddNode("WHILE", KeywordColour));

            whileStatement.Condition.Accept(this);
            whileStatement.Body.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitBlock(BlockStmt block)
        {
            this.nodes.Push(AddNode("BLOCK", KeywordColour));

            foreach (var stmt in block.Statements)
                stmt.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitVar(VarStmt var)
        {
            this.nodes.Push(AddNode("LET", KeywordColour));
            
            AddNode(var.Name.Lexeme, NonKeywordColour);

            if (var.Initializer != null)
                var.Initializer.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitPrint(PrintStmt print)
        {
            this.nodes.Push(AddNode("PRINT", KeywordColour));

            print.Expression.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitExpression(ExpressionStmt expression)
        {
            this.nodes.Push(AddNode("EXPRESSION", KeywordColour));

            expression.Expr.Accept(this);

            return this.nodes.Pop();
        }

        public object? VisitClass(ClassStmt @class)
        {
            this.nodes.Push(AddNode("CLASS", KeywordColour));

            AddNode(@class.Name.Lexeme, NonKeywordColour);

            if (@class.Superclass != null)
            {
                this.nodes.Push(AddNode("SUPERCLASS", KeywordColour));
                
                @class.Superclass.Accept(this);

                this.nodes.Pop();
            }

            this.nodes.Push(AddNode("METHODS", KeywordColour));
            
            foreach (var method in @class.Methods)
            {
                method.Accept(this);
            }

            this.nodes.Pop();

            return this.nodes.Pop();
        }

        public object? VisitFor(ForStatement forStatement)
        {
            this.nodes.Push(AddNode("FOR", KeywordColour));
            
            this.nodes.Push(AddNode("INIT", KeywordColour));
            if (forStatement.Initializer != null)
                forStatement.Initializer.Accept(this);
            this.nodes.Pop();
            
            this.nodes.Push(AddNode("CONDITION", KeywordColour));
            if (forStatement.Condition != null)
                forStatement.Condition.Accept(this);
            this.nodes.Pop();

            this.nodes.Push(AddNode("INCREMENT", KeywordColour));
            if (forStatement.Increment != null)
                forStatement.Increment.Accept(this);
            this.nodes.Pop();

            this.nodes.Push(AddNode("BODY", KeywordColour));
            forStatement.Body.Accept(this);

            this.nodes.Pop();

            return this.nodes.Pop();
        }
    }
}
