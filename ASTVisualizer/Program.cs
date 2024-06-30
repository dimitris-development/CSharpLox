using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Drawing;
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
        
        List<Stmt> statements = Lox.Parse(stream, "../../../../test.lox.log");

        foreach (Stmt stmt in statements)
        {
            ast.Visualize(stmt);
        }

        viewer.Graph = graph;
        form.SuspendLayout();

        viewer.Dock = DockStyle.Fill;

        form.Controls.Add(viewer);
        form.ResumeLayout();
        form.ShowDialog();
    }


}
