namespace CSharpLox;
public abstract class Expr
{
    public class Binary(Expr left, Token @operator, Expr right)
    {
        readonly Expr _left = left;
        readonly Token _operator = @operator;
        readonly Expr _right = right;
    }
}

