using AST;

namespace CSharpLox
{
    public class Scanner(string source)
    {
        readonly string _source = source;
        readonly IList<Token> _tokens = [];
        int _start = 0;
        int _current = 0;
        int _line = 1;

        Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>()
        {
            {"and",    TokenType.AND  },
            {"class",  TokenType.CLASS },
            {"else",   TokenType.ELSE },
            {"false",  TokenType.FALSE },
            {"for",    TokenType.FOR },
            {"fun",    TokenType.FUN },
            {"if",     TokenType.IF },
            {"nil",    TokenType.NIL },
            {"or",     TokenType.OR },
            {"print",  TokenType.PRINT },
            {"return", TokenType.RETURN },
            {"super",  TokenType.SUPER },
            {"this",   TokenType.THIS },
            {"true",   TokenType.TRUE },
            {"var",    TokenType.VAR },
            {"while",  TokenType.WHILE }
        };

        public IList<Token> ScanTokens()
        {
            while (!IsEOF())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;
        }

        void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '?': AddToken(TokenType.QUESTION_MARK); break;
                case ':': AddToken(TokenType.COLON); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/': CommentOrSlash(); break;
                case '"': String(); break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;
                case '\n':
                    _line++;
                    break;
                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Lox.Error(_line, "Unexpected character.");
                    }
                    break;
            }
        }

        void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        void AddToken(TokenType type, object? literal)
        {
            string lexeme = _source[_start.._current];
            _tokens.Add(new Token(type, lexeme, literal, _line));
        }

        void CommentOrSlash()
        {
            if (Match('/'))
            {
                // A comment goes until the end of the line.
                while (Peek() != '\n' && !IsEOF()) Advance();
                return;
            }

            if (Match('*'))
            {
                // A comment block /* */
                // Peek * and / right after
                while (!(Peek() == '*' && PeekNext() == '/') && !IsEOF())
                {
                    if (Peek() == '\n') _line++;
                    Advance();
                }

                if (IsEOF())
                {
                    Lox.Error(_line, "Unterminated comment block.");
                    return;
                }

                // Consume the closing * and /
                Advance();
                Advance();
                return;
            }

            AddToken(TokenType.SLASH);
        }

        void String()
        {
            while (Peek() != '"' && !IsEOF())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsEOF())
            {
                Lox.Error(_line, "Unterminated string.");
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            string value = _source[(_start + 1)..(_current-1)];
            AddToken(TokenType.STRING, value);
        }

        void Number()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            string number = _source[_start.._current];
            AddToken(TokenType.NUMBER, double.Parse(number));
        }

        void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = _source[_start.._current];
            TokenType type = _keywords.ContainsKey(text) ? _keywords[text] : TokenType.IDENTIFIER;

            AddToken(type);
        }

        char Advance()
        {
            return _source[_current++];
        }

        char Peek()
        {
            if (IsEOF()) return '\0';
            return _source[_current];
        }

        char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        bool Match(char expected)
        {
            if (IsEOF()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            return true;
        }

        bool IsEOF()
        {
            return _current >= _source.Length;
        }

        static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }
    }
}
