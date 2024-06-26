using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Drawing;
using Color = Microsoft.Msagl.Drawing.Color;
using AST;
using CSharpLox;

namespace ASTVisualizer;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Form form = new BaseForm();
        GViewer viewer = new ();
        Graph graph = new ("graph");

        ASTGraph ast = new (graph);

        string stream = File.ReadAllText("../../../../test.lox");
        Scanner scanner = new Scanner(stream);
        IList<Token> tokens = scanner.ScanTokens();
        Parser parser = new Parser(tokens);

        Expr? expression = parser.Parse();

        ast.Visualize(expression);

        viewer.Graph = graph;
        form.SuspendLayout();

        viewer.Dock = DockStyle.Fill;

        form.Controls.Add(viewer);
        form.ResumeLayout();
        form.ShowDialog();
    }


}
