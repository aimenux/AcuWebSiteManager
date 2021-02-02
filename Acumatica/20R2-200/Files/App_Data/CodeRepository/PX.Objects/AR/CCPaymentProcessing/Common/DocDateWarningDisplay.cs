using System;
using PX.Data;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public sealed class DocDateWarningDisplay : IPXCustomInfo
	{
		public void Complete(PXLongRunStatus status, PXGraph graph)
		{
			if (status == PXLongRunStatus.Completed && graph is ARPaymentEntry)
			{
				((ARPaymentEntry)graph).RowSelected.AddHandler<ARPayment>((sender, e) =>
				{
					ARPayment payment = e.Row as ARPayment;
					if (payment != null && payment.Released == false && DateTime.Compare(payment.AdjDate.Value, _NewDate) != 0)
					{
						sender.RaiseExceptionHandling<ARPayment.adjDate>(payment, payment.AdjDate, new PXSetPropertyException(Messages.ApplicationDateChanged, PXErrorLevel.Warning));
					}
				});
			}
		}
		private readonly DateTime _NewDate;
		public DocDateWarningDisplay(DateTime newDate)
		{
			_NewDate = newDate;
		}
	}
}
