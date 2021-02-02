using PX.Data;
using System;

namespace PX.Objects.PR
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DedAndBenPackageEventSubscriberAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		private Type _deductionAmountField;
		private Type _deductionRateField;
		private Type _benefitAmountField;
		private Type _benefitRateField;
		private Type _effectiveDateField;

		public DedAndBenPackageEventSubscriberAttribute(Type deductionAmountField, Type deductionRateField, Type benefitAmountField, Type benefitRateField, Type effectiveDateField)
		{
			_deductionAmountField = deductionAmountField;
			_deductionRateField = deductionRateField;
			_benefitAmountField = benefitAmountField;
			_benefitRateField = benefitRateField;
			_effectiveDateField = effectiveDateField;
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null)
				return;

			sender.SetDefaultExt(e.Row, _deductionAmountField.Name);
			sender.SetDefaultExt(e.Row, _deductionRateField.Name);
			sender.SetDefaultExt(e.Row, _benefitAmountField.Name);
			sender.SetDefaultExt(e.Row, _benefitRateField.Name);
			sender.SetValue(e.Row, _effectiveDateField.Name, null);
		}
	}
}
