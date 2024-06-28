using AST;

namespace CSharpLox
{
    // Parsing rules:
    // 
    // expression     → conditional (, conditional)*
    // 
    // Conditional resolves to term so equality ? term : term == equality ? term : conditional
    // conditional    → equality | (equality ? term : conditional)
    // equality       → comparison (( "!=" | "==" ) comparison )*
    // comparison     → term(( ">" | ">=" | "<" | "<=" ) term )*
    // term           → factor(( "-" | "+" ) factor )*
    // factor         → unary(( "/" | "*" ) unary )*
    // unary          → ( "!" | "-" ) unary
    //                  | primary
    // primary        → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")"

    public class Parser(IList<Token> tokens)
    {
        private class UnhandledParseError : Exception;

        private class HandledParseError(Token token, string message)
        {
            public string message = message;
            public Token token = token;
        }

        private IList<HandledParseError> _errors = new List<HandledParseError>();

        private readonly IList<Token> _tokens = tokens;
        private int _current = 0;

        public Expr? Parse()
        {
            try
            {
                Expr expr = Expression();

                foreach (var error in _errors)
                {
                    Lox.Error(error.token, error.message);
                }

                return expr;
            }
            catch (UnhandledParseError error)
            {
                return null;
            }
        }

        private Expr Expression()
        {
            Expr expr = Conditional();

            while(Match(TokenType.COMMA))
            {
                expr = CheckForExpression(expr, "Binary operator ',' must have both left and right operands");

                Token oper = Previous();
                Expr right = Conditional();

                right = CheckForExpression(right, "Binary operator ',' must have both left and right operands");

                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Conditional()
        {
            Expr expr = Equality();

            if (!Match(TokenType.QUESTION_MARK)) return expr;

            Token questionMark = Previous();
            Expr right = Term();

            Consume(TokenType.COLON, "Expected ':' after term");

            Token colon = Previous();
            Expr conditional = Conditional();

            right = CheckForExpression(right, "Binary operator ':' must have both left and right operands");
            conditional = CheckForExpression(conditional, "Binary operator ':' must have both left and right operands");

            Expr conditionalSubtree = new Expr.Binary(right, colon, conditional);

            expr = CheckForExpression(expr, "Binary operator '?' must have both left and right operands");
            conditionalSubtree = CheckForExpression(conditionalSubtree, "Binary operator '?' must have both left and right operands");
            return new Expr.Binary(expr, questionMark, conditionalSubtree);
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                expr = CheckForExpression(expr, "Binary operators (!=, ==) must have both left and right operands");

                Token oper = Previous();
                Expr right = Comparison();

                right = CheckForExpression(right, "Binary operators (!=, ==) must have both left and right operands");

                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                expr = CheckForExpression(expr, "Binary operators (<, <=, >, >=) must have both left and right operands");

                Token oper = Previous();
                Expr right = Term();

                right = CheckForExpression(right, "Binary operators (<, <=, >, >=) must have both left and right operands");

                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                expr = CheckForExpression(expr, "Binary operators (-, +) must have both left and right operands");

                Token oper = Previous();
                Expr right = Factor();

                right = CheckForExpression(right, "Binary operators (-, +) must have both left and right operands");

                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                expr = CheckForExpression(expr, "Binary operators (/, *) must have both left and right operands");

                Token oper = Previous();
                Expr right = Unary();

                right = CheckForExpression(right, "Binary operators (/, *) must have both left and right operands");

                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            while (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token oper = Previous();
                Expr right = Unary();

                return new Expr.Unary(oper, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().literal);
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                expr = CheckForExpression(expr, "Expected expression not '('");

                Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression");

                return new Expr.Grouping(expr);
            }

            return new Expr.Nothing("Nothing");
        }

        private Expr CheckForExpression(Expr expr, string errorMessage)
        {
            if (expr is Expr.Nothing)
            {
                HandledParseError error = new (Previous(), errorMessage);
                _errors.Add(error);
            }

            return expr;
        }

        private bool Match(params TokenType[] tokens)
        {
            foreach (var token in tokens)
            {
                if (Check(token))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsEOF()) return false;
            return Peek().type == type;
        }

        private bool IsEOF()
        {
            return Peek().type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private Token Advance()
        {
            if (!IsEOF()) _current++;
            return Previous();
        }

        private Token Consume(TokenType type, string errorMessage)
        {
            if (Check(type)) return Advance();
            throw Error(Peek(), errorMessage);
        }

        private static UnhandledParseError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new UnhandledParseError();
        }
    }
}
