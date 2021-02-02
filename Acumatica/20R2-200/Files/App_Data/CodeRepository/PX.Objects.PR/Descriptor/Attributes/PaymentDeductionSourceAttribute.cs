using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;

namespace PX.Objects.PR
{
	public class PaymentDeductionSourceAttribute : DeductionSourceListAttribute, IPXFieldDefaultingSubscriber
	{
		private Type _DeductCodeIDField;

		public PaymentDeductionSourceAttribute(Type deductCodeIDField = null)
		{
			_DeductCodeIDField = deductCodeIDField;
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PRDeductCode deductCode = (PRDeductCode)PXSelectorAttribute.Select(sender, e.Row, _DeductCodeIDField.Name);
			if (deductCode != null)
			{
				e.NewValue = GetSource(deductCode);
			}
		}
	}
}
