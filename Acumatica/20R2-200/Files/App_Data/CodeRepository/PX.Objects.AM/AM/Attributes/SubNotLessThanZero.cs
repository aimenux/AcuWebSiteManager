using System.Collections.Generic;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Returns the subtraction of <tt>Operand2</tt> from <tt>Operand1</tt> not less than decimal zero.
    /// Manufacturing generated class (sourced from PX.Data.Sub)
    /// </summary>
    /// <typeparam name="Operand1">A field, constant, or function of decimal type</typeparam>
    /// <typeparam name="Operand2">A field, constant, or function of decimal type</typeparam>
    public sealed class SubNotLessThanZero<Operand1, Operand2> : BqlFunction, IBqlOperand, IBqlCreator
        where Operand1 : IBqlOperand
        where Operand2 : IBqlOperand
    {
        IBqlCreator _formula = new Switch<Case<Where<Sub<Operand1, Operand2>, LessEqual<decimal0>>, decimal0>, Sub<Operand1, Operand2>>();

        public bool AppendExpression(ref PX.Data.SQLTree.SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
        {
            return _formula.AppendExpression(ref exp, graph, info, selection);
        }

        public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
        {
            _formula.Verify(cache, item, pars, ref result, ref value);
        }
    }
}
