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
            string targetDirectory = Path.Combine([currentDirectory, "..\\..\\..\\..\\", outputDir]);
            string fullPath = Path.Combine(targetDirectory, $"{baseName}.cs");

            Directory.CreateDirectory(targetDirectory);

            using (StreamWriter sw = File.CreateText(fullPath))
            {
                sw.WriteLine("namespace CSharpLox.AST;");
                sw.WriteLine($"public abstract class {baseName}");
                sw.WriteLine("{");

                DefineVisitor(sw, baseName, types);

                foreach (string type in types)
                {
                    string className = type.Split(":")[0].Trim();
                    string fields = type.Split(":")[1].Trim();
                    DefineType(sw, baseName, className, fields);
                }

                sw.WriteLine("\tpublic abstract T Accept<T>(IVisitor<T> visitor);");
                sw.WriteLine("}");
            }
        }

        static void DefineVisitor(StreamWriter sw, string baseName, List<string> types)
        {
            sw.WriteLine($"\tpublic interface IVisitor<T>");
            sw.WriteLine("\t{");

            foreach (string type in types)
            {
                string className = type.Split(":")[0].Trim();

                sw.WriteLine($"\t\tpublic T Visit({className} _{className.ToLower()});");
            }

            sw.WriteLine("\t}");
        }

        static void DefineType(StreamWriter sw, string baseName, string className, string fields)
        {
            sw.WriteLine($"\tpublic class {className} ({fields}) : {baseName}");
            sw.WriteLine("\t{");

            string[] fieldList = fields.Split(", ");

            foreach (string field in fieldList) 
            {
                string type = field.Split(" ")[0];
                string name = field.Split(" ")[1];
                
                sw.WriteLine($"\t\treadonly {type} _{name} = {name};");
            }

            sw.WriteLine();
            sw.WriteLine("\t\tpublic override T Accept<T>(IVisitor<T> visitor)");
            sw.WriteLine("\t\t{");
            sw.WriteLine($"\t\t\treturn visitor.Visit(this);");
            sw.WriteLine("\t\t}");
            sw.WriteLine("\t}");
        }
    }
}
