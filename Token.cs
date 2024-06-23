namespace CSharpLox
{
    public class Token(
        TokenType type,
        string lexeme,
        object? literal,
        int line
    )
    {
        readonly TokenType _type = type;
        readonly string _lexeme = lexeme;
        readonly object? _literal = literal;
        readonly int _line = line;

        override public string ToString()
        {
            return _type + " " + _lexeme + " " + _literal;
        }
    }
}
