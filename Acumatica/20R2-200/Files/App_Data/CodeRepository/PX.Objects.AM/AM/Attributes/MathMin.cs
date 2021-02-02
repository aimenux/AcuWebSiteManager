using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// BqlFunction performing Math.Min(Operand1, Operand2)
    /// </summary>
    public sealed class MathMin<Operand1, Operand2> : BqlFunction, IBqlOperand, IBqlCreator
        where Operand1 : IBqlOperand
        where Operand2 : IBqlOperand
    {
        IBqlCreator _formula = new Switch<Case<Where<Operand1, LessEqual<Operand2>>, Operand1>, Operand2>();

        public bool AppendExpression(ref PX.Data.SQLTree.SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
            => _formula.AppendExpression(ref exp, graph, info, selection);

        public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
        {
            _formula.Verify(cache, item, pars, ref result, ref value);
        }
    }
}