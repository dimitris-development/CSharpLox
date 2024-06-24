namespace ASTGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string outputDir = "AST";

            DefineAst(outputDir, "Expr", [
              "Binary   : Expr left, Token oper, Expr right",
              "Grouping : Expr expression",
              "Literal  : object value",
              "Unary    : Token oper, Expr right"
            ]);
        }

        static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string targetDirectory = Path.Combine([currentDirectory, "..\\..\\..\\", outputDir]);
            string fullPath = Path.Combine(targetDirectory, $"{baseName}.cs");

            Directory.CreateDirectory(targetDirectory);

            using (StreamWriter sw = File.CreateText(fullPath))
            {
                sw.WriteLine("namespace CSharpLox;");
                sw.WriteLine($"public abstract class {baseName} {{");

                foreach (string type in types)
                {
                    string className = type.Split(":")[0].Trim();
                    string fields = type.Split(":")[1].Trim();
                    DefineType(sw, className, fields);
                }

                sw.WriteLine($"}}");
            }
        }

        static void DefineType(StreamWriter sw, string className, string fields)
        {
            sw.WriteLine($"\tpublic class {className} ({fields}) {{");

            string[] fieldList = fields.Split(", ");

            foreach (string field in fieldList) 
            {
                string type = field.Split(" ")[0];
                string name = field.Split(" ")[1];
                
                sw.WriteLine($"\t\treadonly {type} _{name} = {name};");
            }

            sw.WriteLine($"\t}}");
        }
    }
}
