using AST;

namespace CSharpLox
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor
    {
        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt stmt in statements)
                {
                    Execute(stmt);
                }
            }
            catch (RuntimeError ex)
            {
                Lox.RuntimeError(ex);
            }
        }

        public class RuntimeError(Token token, string message) : Exception
        {
            public readonly Token token = token;
            public readonly string message = message;
        }

        void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public void Visit(Stmt.Expression statementExpression)
        {
            Evaluate(statementExpression.expression);
        }

        public void Visit(Stmt.Print statementPrint)
        {
            object output = Evaluate(statementPrint.expression);
            Console.Write(Stringify(output));
        }

        object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        public object Visit(Expr.Binary binary)
        {
            object left = Evaluate(binary.left);
            object right = Evaluate(binary.right);
            
            switch (binary.oper.type)
            {
                case TokenType.GREATER:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left <= (double)right;
                case TokenType.PLUS:
                    if (left is string && right is string) 
                        return (string)left + (string)right;
                    if (left is double && right is double)
                        return (double)left + (double)right;
                    break;
                case TokenType.MINUS:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left - (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(binary.oper, left, right);
                    return (double)left * (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(binary.oper, left, right);

                    if ((double) right == 0) throw new RuntimeError(binary.oper, "Division by 0");

                    return (double)left / (double)right;
                case TokenType.QUESTION_MARK:
                    return Terminal(binary);
                case TokenType.BANG_EQUAL:
                    return !IsEqual(binary.left, binary.right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(binary.left, binary.right);
            }

            throw new RuntimeError(binary.oper, "Unexpected binary operation");
        }

        public object Visit(Expr.Grouping grouping)
        {
            return Evaluate(grouping.expression);
        }

        public object Visit(Expr.Literal literal)
        {
            return literal.value;
        }

        public object Visit(Expr.Unary unary)
        {
            object right = Evaluate(unary);

            switch (unary.oper.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(unary.oper, right);
                    return - (double) right;
                case TokenType.BANG:
                    return !IsTruthy(right);
            }

            throw new RuntimeError(unary.oper, "Unexpected unary operation");
        }

        public object Visit(Expr.Nothing nothing)
        {
            return "Nothing";
        }



        object Terminal(Expr.Binary binaryExpr)
        {
            object condition = Evaluate(binaryExpr.left);

            if (binaryExpr.right is not Expr.Binary) throw new RuntimeError(binaryExpr.oper, "Unexpected expression to the right of '?'");

            Expr.Binary results = (Expr.Binary)binaryExpr.right;

            if ((bool)condition)
                return Evaluate(results.left);

            return Evaluate(results.right);
        }

        static bool IsTruthy(object right)
        {
            if (right == null) return false;
            if (right.GetType() == typeof(bool)) return (bool) right;
            return true;
        }

        static bool IsEqual(object left, object right)
        {
            if (left == null && right == null) return true;

            return left == right;
        }

        static void CheckNumberOperand(Token oper, object operand)
        {
            if (operand is double) return;

            throw new RuntimeError(oper, "Operand must be a number");
        }

        static void CheckNumberOperands(Token oper, object left, object right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(oper, "Both operands must be numbers");
        }

        static string Stringify(object value)
        {
            if (value == null) return "nil";
            return Convert.ToString(value);
        }
    }
}
