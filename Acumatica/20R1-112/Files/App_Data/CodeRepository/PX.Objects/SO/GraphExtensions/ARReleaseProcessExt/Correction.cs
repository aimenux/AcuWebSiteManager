using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.SO.GraphExtensions.ARReleaseProcessExt
{
	public class Correction : PXGraphExtension<IN.GraphExtensions.ARReleaseProcessExt.ProcessInventory, ARReleaseProcess>
	{
		[PXOverride]
		public virtual void CloseInvoiceAndClearBalances(ARRegister ardoc, int? adjNbr, Action<ARRegister, int?> baseMethod)
		{
			if (ardoc.IsUnderCorrection == true)
			{
				ardoc.Canceled = true;

				PXDatabase.Update<ARTran>(
					new PXDataFieldAssign<ARTran.canceled>(PXDbType.Bit, true),
					new PXDataFieldRestrict<ARTran.tranType>(PXDbType.Char, ardoc.DocType),
					new PXDataFieldRestrict<ARTran.refNbr>(PXDbType.NVarChar, ardoc.RefNbr),
					new PXDataFieldRestrict<ARTran.canceled>(PXDbType.Bit, false));
			}

			baseMethod(ardoc, adjNbr);
		}

		[PXOverride]
		public virtual void OpenInvoiceAndRecoverBalances(ARRegister ardoc, Action<ARRegister> baseMethod)
		{
			if (ardoc.IsUnderCorrection == true)
			{
				throw new PXException(Messages.OnlyCancelCreditMemoCanBeApplied, ardoc.RefNbr);
			}

			baseMethod(ardoc);
		}

		[PXOverride]
		public virtual void ProcessARTranInventory(ARTran n, ARInvoice ardoc, JournalEntry je, Action<ARTran, ARInvoice, JournalEntry> baseMethod)
		{
			if (ardoc.IsCancellation == true)
			{
				if (Base.IsIntegrityCheck) return;

				foreach (INTran intran in Base1.intranselect.View.SelectMultiBound(new object[] { n }))
				{
					intran.ARDocType = null;
					intran.ARRefNbr = null;
					intran.ARLineNbr = null;

					Base1.intranselect.Cache.MarkUpdated(intran);

					Base1.PostShippedNotInvoiced(intran, n, ardoc, je);
				}
			}
			else
			{
				baseMethod(n, ardoc, je);
			}
		}
	}
}
