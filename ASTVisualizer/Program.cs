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
        
        Expr? expression = Lox.Parse(stream, "../../../../test.lox.log");

        ast.Visualize(expression);

        viewer.Graph = graph;
        form.SuspendLayout();

        viewer.Dock = DockStyle.Fill;

        form.Controls.Add(viewer);
        form.ResumeLayout();
        form.ShowDialog();
    }


}
