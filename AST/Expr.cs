namespace AST;
public abstract class Expr
{
	public interface IVisitor<T>
	{
		public T Visit(Binary binary);
		public T Visit(Grouping grouping);
		public T Visit(Literal literal);
		public T Visit(Unary unary);
	}
	public class Binary (Expr left, Token oper, Expr right) : Expr
	{
		public readonly Expr left = left;
		public readonly Token oper = oper;
		public readonly Expr right = right;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
	public class Grouping (Expr expression) : Expr
	{
        public readonly Expr expression = expression;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
	public class Literal (object? value) : Expr
	{
        public readonly object? value = value;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
	public class Unary (Token oper, Expr right) : Expr
	{
        public readonly Token oper = oper;
        public readonly Expr right = right;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
	public abstract T Accept<T>(IVisitor<T> visitor);
}
