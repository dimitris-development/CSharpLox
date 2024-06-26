namespace CSharpLox.AST;
public abstract class Expr
{
	public interface IVisitor<T>
	{
		public T Visit(Binary _binary);
		public T Visit(Grouping _grouping);
		public T Visit(Literal _literal);
		public T Visit(Unary _unary);
	}
	public class Binary (Expr left, Token oper, Expr right) : Expr
	{
		readonly Expr _left = left;
		readonly Token _oper = oper;
		readonly Expr _right = right;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
	public class Grouping (Expr expression) : Expr
	{
		readonly Expr _expression = expression;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
	public class Literal (object? value) : Expr
	{
		readonly object? _value = value;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
	public class Unary (Token oper, Expr right) : Expr
	{
		readonly Token _oper = oper;
		readonly Expr _right = right;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
	public abstract T Accept<T>(IVisitor<T> visitor);
}
