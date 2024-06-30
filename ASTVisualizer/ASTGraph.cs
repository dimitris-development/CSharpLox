using Microsoft.Msagl.Drawing;
using AST;

namespace ASTVisualizer;

public class ASTGraph (Graph graph) : Expr.IVisitor<string>, Stmt.IVisitor<string>
{
    Graph _graph = graph;

    public string Visit(Stmt.Print print)
    {
        return CreateNode("Print", print.expression);
    }

    public string Visit(Stmt.Expression expr)
    {
        return CreateNode("Expression", expr.expression);
    }

    public string Visit(Expr.Binary binary)
    {
        return CreateNode($"Binary {binary.oper}", binary.left, binary.right);
    }

    public string Visit(Expr.Grouping grouping)
    {
        return CreateNode("Grouping", grouping.expression);
    }

    public string Visit(Expr.Literal literal)
    {
        string label = Convert.ToString(literal.value) ?? "nil";
        return CreateNode(label);
    }

    public string Visit(Expr.Unary unary)
    {
        return CreateNode($"Unary {unary.oper}", unary.right);
    }

    public string Visit(Expr.Nothing nothing)
    {
        return CreateNode("Nothing");
    }

    public string Visualize(Stmt stmt)
    {
        return stmt.Accept(this);
    }

    private string CreateNode(string name, params Expr[] exprs)
    {
        string nodeId = Guid.NewGuid().ToString();
        Node node = _graph.AddNode(nodeId);
        node.LabelText = name;

        foreach (Expr expr in exprs)
        {
            _graph.AddEdge(nodeId, expr.Accept(this));
        }

        return nodeId;
    }

    private string CreateNode(string name)
    {
        string nodeId = Guid.NewGuid().ToString();
        Node node = _graph.AddNode(nodeId);

        node.LabelText = name;

        return nodeId;
    }
}

