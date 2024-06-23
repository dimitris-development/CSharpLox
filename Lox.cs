namespace CSharpLox
{
    public static class Lox
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

            foreach (Token token in tokens)
            {
                Console.WriteLine(token.ToString());
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }
    }
}
