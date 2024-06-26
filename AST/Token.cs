namespace AST
{
    public class Token(
        TokenType type,
        string lexeme,
        object? literal,
        int line
    )
    {
        public readonly TokenType type = type;
        public readonly string lexeme = lexeme;
        public readonly object? literal = literal;
        public readonly int line = line;

        override public string ToString()
        {
            return type + " " + lexeme + " " + literal;
        }
    }
}
