using static AST.Expr;
using Microsoft.Msagl.Drawing;
using AST;

namespace ASTVisualizer;

public class ASTGraph (Graph graph) : IVisitor<string>
{
    Graph _graph = graph;

    public string Visit(Binary binary)
    {
        return CreateNode($"Binary {binary.oper}", binary.left, binary.right);
    }

    public string Visit(Grouping grouping)
    {
        return CreateNode("Grouping", grouping.expression);
    }

    public string Visit(Literal literal)
    {
        return CreateNode(literal);
    }

    public string Visit(Unary unary)
    {
        return CreateNode($"Unary {unary.oper}", unary.right);
    }

    public string Visualize(Expr expr)
    {
        return expr.Accept(this);
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

    private string CreateNode(Literal literal)
    {
        string nodeId = Guid.NewGuid().ToString();
        Node node = _graph.AddNode(nodeId);
        string label = Convert.ToString(literal.value) ?? "nil";

        node.LabelText = label;

        return nodeId;
    }
}

