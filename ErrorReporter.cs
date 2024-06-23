namespace CSharpLox
{
    public static class ErrorReporter
    {
        public static void LoxError(string message)
        {
            Report("Lox Error", null, null, message);
        }

        public static void SyntaxError(int line, string where, string message)
        {
            Report("Syntax Error", line, where, message);
        }

        public static void Report(string type, int? line, string? where, string message)
        {
            string lineText = "";
            string whereText = "";

            if (line.HasValue)
            {
                lineText = $"At {line}";
            }

            if (!string.IsNullOrEmpty(where))
            {
                whereText = $"on \"{where}\":";
            }

            Console.Error.WriteLine($"{type}: {lineText}, {whereText} {message}");
        }
    }
}
