using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;

namespace PX.Objects.SO.GraphExtensions.ARPaymentEntryExt
{
	public class Correction : ARAdjustCorrectionExtension<ARPaymentEntry>
	{
		public delegate void CreatePaymentDelegate(ARInvoice ardoc, CurrencyInfo info, DateTime? paymentDate, string aFinPeriod, bool overrideDesc);
		[PXOverride]
		public virtual void CreatePayment(ARInvoice ardoc, CurrencyInfo info, DateTime? paymentDate, string aFinPeriod, bool overrideDesc,
			CreatePaymentDelegate baseMethod)
		{
			if (ardoc.IsUnderCorrection == true)
			{
				throw new PXException(Messages.CantCreateApplicationToInvoiceUnderCorrection, ardoc.RefNbr);
			}

			baseMethod(ardoc, info, paymentDate, aFinPeriod, overrideDesc);
		}

		[PXOverride]
		public virtual void ReverseApplicationProc(ARAdjust application, ARPayment payment, Action<ARAdjust, ARPayment> baseMethod)
		{
			if (payment?.IsCancellation == true)
			{
				throw new PXException(Messages.CantReverseCancellationApplication, application.AdjgRefNbr, application.AdjdRefNbr);
			}

			baseMethod(application, payment);
		}
	}
}
