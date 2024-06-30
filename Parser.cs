using AST;

namespace CSharpLox
{
    // Parsing rules:
    // 
    // program        → declaration* EOF
    // declaration    → varDecl | statement
    // varDecl        → "var" IDENTIFIER ("=" expression)? ";" 
    // statement      → exprStmt | printStmt
    // exprStmt       → expression ";"
    // printStmt      → "print" expression ";"
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
    // primary        → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER

    public class Parser(IList<Token> tokens)
    {
        class UnhandledParseError : Exception;

        class HandledParseError(Token token, string message)
        {
            public string message = message;
            public Token token = token;
        }

        IList<HandledParseError> _errors = new List<HandledParseError>();

        readonly IList<Token> _tokens = tokens;
        int _current = 0;

        public List<Stmt> Parse()
        {
            List<Stmt> result = new List<Stmt>();

            while (!IsEOF())
            {
                var decl = Declaration();

                // Ignore empty declaration because they're caused by parsing errors.
                if (decl == null) continue;

                result.Add(decl);
            }

            foreach (var error in _errors)
            {
                Lox.Error(error.token, error.message);
            }

            return result;
        }

        Stmt? Declaration()
        {
            try
            {
                if (Match(TokenType.VAR)) return VarDeclaration();

                return Statement();
            }
            catch (UnhandledParseError error)
            {
                Synchronize();
                return null;
            }
        }

        Stmt VarDeclaration()
        {
            Token token = Consume(TokenType.IDENTIFIER, "Expected variable name");

            Expr? initiliazer = null;

            if (Match(TokenType.EQUAL))
            {
                initiliazer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");

            return new Stmt.Var(token, initiliazer);
        }

        Stmt Statement()
        {
            if (Match(TokenType.PRINT)) return PrintStatement();

            return ExpressionStatement();
        }

        Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expected semicolon at the end of a statement");

            return new Stmt.Print(value);
        }

        Stmt ExpressionStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expected semicolon at the end of a statement");

            return new Stmt.Expression(value);
        }

        Expr Expression()
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

        Expr Conditional()
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

        Expr Equality()
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

        Expr Comparison()
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

        Expr Term()
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

        Expr Factor()
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

        Expr Unary()
        {
            while (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token oper = Previous();
                Expr right = Unary();

                return new Expr.Unary(oper, right);
            }

            return Primary();
        }

        Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().literal);
            }

            if (Match(TokenType.IDENTIFIER)) return new Expr.Variable(Previous());

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                expr = CheckForExpression(expr, "Expected expression not '('");

                Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression");

                return new Expr.Grouping(expr);
            }

            return new Expr.Nothing("Nothing");
        }

        Expr CheckForExpression(Expr expr, string errorMessage)
        {
            if (expr is Expr.Nothing)
            {
                HandledParseError error = new (Previous(), errorMessage);
                _errors.Add(error);
            }

            return expr;
        }

        bool Match(params TokenType[] tokens)
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

        bool Check(TokenType type)
        {
            if (IsEOF()) return false;
            return Peek().type == type;
        }

        bool IsEOF()
        {
            return Peek().type == TokenType.EOF;
        }

        Token Peek()
        {
            return _tokens[_current];
        }

        Token Previous()
        {
            return _tokens[_current - 1];
        }

        Token Advance()
        {
            if (!IsEOF()) _current++;
            return Previous();
        }

        Token Consume(TokenType type, string errorMessage)
        {
            if (Check(type)) return Advance();
            throw Error(Peek(), errorMessage);
        }

        void Synchronize()
        {
            Advance();

            while(!IsEOF())
            {
                if (Previous().type == TokenType.SEMICOLON) return;

                switch (Peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.WHILE:
                    case TokenType.IF:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }

        static UnhandledParseError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new UnhandledParseError();
        }
    }
}
