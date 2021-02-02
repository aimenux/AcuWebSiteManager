using PX.Data;

namespace PX.Objects.Common
{
	public class Maximum<Expr1, Expr2> : IIf<Where<Expr1, Less<Expr2>>, Expr2, Expr1>
		where Expr1 : IBqlOperand
		where Expr2 : IBqlOperand
	{
	}

	public class Minimum<Expr1, Expr2> : IIf<Where<Expr1, Greater<Expr2>>, Expr2, Expr1>
		where Expr1 : IBqlOperand
		where Expr2 : IBqlOperand
	{
	}
}
