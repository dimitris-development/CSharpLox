namespace AST;
public abstract class Stmt
{
	public interface IVisitor<T>
	{
		public T Visit(Expression expression);
		public T Visit(Print print);
		public T Visit(Var var);
	}
	public interface IVisitor
	{
		public void Visit(Expression expression);
		public void Visit(Print print);
		public void Visit(Var var);
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
	public class Var (Token name, Expr? initializer) : Stmt
	{
		public readonly Token name = name;
		public readonly Expr? initializer = initializer;

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
