using System;

using PX.Data;

namespace PX.Objects.Common.Attributes
{
	/// <summary>
	/// A <see cref="PXDefault"/>-like attribute supporting arbitrary
	/// BQL formulas as the providers of the default value.
	/// </summary>
	public class FormulaDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
	{
		public virtual IBqlCreator Formula
		{
			get;
			protected set;
		}
		
		public FormulaDefaultAttribute(Type formulaType)
		{
			Formula = PXFormulaAttribute.InitFormula(formulaType);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			object record = e.Row;

			if (e.Row == null || e.NewValue != null) return;

			bool? result = false;
			object value = null;

			BqlFormula.Verify(sender, record, Formula, ref result, ref value);

			if (value != null && value != PXCache.NotSetValue)
			{
				e.NewValue = value;
			}
		}
	}
}
