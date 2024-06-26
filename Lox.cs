using AST;

namespace CSharpLox
{
    static class Lox
    {
        static bool hadError = false;

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
        }

        static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string? line = Console.ReadLine();
                if (line == null) break;
                if (line == "test") RunFile("../../../test.lox");

                Run(line);
                hadError = false;
            }
        }

        static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            IList<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);

            if (hadError) return;

            Expr? expression = parser.Parse();
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
            Console.Error.WriteLine($"[line {line}] Error {where} : {message}");
            hadError = true;
        }
    }
}
