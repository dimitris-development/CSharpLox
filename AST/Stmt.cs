namespace AST;
public abstract class Stmt
{
	public interface IVisitor<T>
	{
		public T Visit(Expression expression);
		public T Visit(Print print);
	}
	public interface IVisitor
	{
		public void Visit(Expression expression);
		public void Visit(Print print);
	}
	public class Expression (Expr expression) : Stmt
	{
		public readonly Expr expression = expression;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
	public class Print (Expr expression) : Stmt
	{
		public readonly Expr expression = expression;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
	public abstract T Accept<T>(IVisitor<T> visitor);

	public abstract void Accept(IVisitor visitor);
}
