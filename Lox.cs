using AST;
using static CSharpLox.Interpreter;

namespace CSharpLox
{
    public static class Lox
    {
        static bool hadError = false;
        static bool hadRuntimeError = false;

        const string TEST_FILE_PATH = "../../../test.lox";

        static StreamWriter? _streamWriter;
        readonly static Interpreter _interpreter = new Interpreter();

        static int Main(string[] args)
        {
            try
            {
                if (args.Length > 1)
                {
                    Console.WriteLine("Usage cslox [script]");
                    return 64;
                }

                if (args.Length == 1)
                {
                    RunFile(args[0]);
                    return 0;
                }
                
                RunPrompt();
                return 0;
            }
            catch (Exception ex)
            {
                Error(0, ex.Message + "\n" + ex.StackTrace);
                return -1;
            }
        }
        
        static void RunFile(string filePath)
        {
            string stream = File.ReadAllText(filePath);

            Run(stream);
            if (hadError) Environment.Exit(65);
            if (hadRuntimeError) Environment.Exit(70);
        }

        static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string? line = Console.ReadLine();
                if (line == null) break;
                if (line == "test") RunFile(TEST_FILE_PATH);
                else Run(line);
                hadError = false;
            }
        }

        static void Run(string source)
        {
            Expr? expression = Parse(source);
            if (hadError) return;

            _interpreter.Interpret(expression);
        }

        public static Expr? Parse(string source, string errorFilePath = "")
        {
            if (errorFilePath != "")
            {
                _streamWriter = new StreamWriter(errorFilePath);
            }

            Scanner scanner = new Scanner(source);
            IList<Token> tokens = scanner.ScanTokens();

            if (hadError) return null;

            Parser parser = new Parser(tokens);
            var result = parser.Parse();

            if (_streamWriter != null) _streamWriter.Close();

            return result;
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, "at end", message);
            }
            else
            {
                Report(token.line, $"at '{token.lexeme}'", message);
            }
        }

        static void Report(int line, string where, string message)
        {
            string errorMessage = $"[line {line}] Error {where} : {message}";

            if (_streamWriter != null)
            {
                _streamWriter.WriteLine(errorMessage);
                return;
            }

            Console.Error.WriteLine(errorMessage);
            hadError = true;
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"Runtime Error: {error.message} at '{error.token.lexeme}' \n[line {error.token.line}]");
            hadRuntimeError = true;
        }
    }
}
