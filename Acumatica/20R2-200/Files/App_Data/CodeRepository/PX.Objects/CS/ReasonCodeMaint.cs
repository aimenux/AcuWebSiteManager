using System;
using System.Collections.Generic;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.CS
{
	public class ReasonCodeMaint : PXGraph<ReasonCodeMaint, ReasonCode>
	{
		[PXCopyPasteHiddenFields(typeof(ReasonCode.subMaskFinance), typeof(ReasonCode.subMaskInventory))]
		public PXSelect<ReasonCode> reasoncode;
        public PXSelect<INSetup> INSetup;

        protected virtual void ReasonCode_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ReasonCode row = e.Row as ReasonCode;
			if (row == null) return;

			PXUIFieldAttribute.SetVisible<ReasonCode.accountID>(sender, row, !row.Usage.IsIn(ReasonCodeUsages.Sales, ReasonCodeUsages.VendorReturn, ReasonCodeUsages.Transfer));
			PXUIFieldAttribute.SetVisible<ReasonCode.subID>(sender, row, !row.Usage.IsIn(ReasonCodeUsages.Sales, ReasonCodeUsages.VendorReturn, ReasonCodeUsages.Transfer));
			PXUIFieldAttribute.SetVisible<ReasonCode.salesAcctID>(sender, row, row.Usage.IsIn(ReasonCodeUsages.Sales, ReasonCodeUsages.Issue));
			PXUIFieldAttribute.SetVisible<ReasonCode.salesSubID>(sender, row, row.Usage.IsIn(ReasonCodeUsages.Sales, ReasonCodeUsages.Issue));

			if (row.Usage.IsIn(ReasonCodeUsages.Sales, ReasonCodeUsages.VendorReturn, ReasonCodeUsages.Transfer))
			{
				PXUIFieldAttribute.SetVisible<ReasonCode.subMaskInventory>(sender, row, false);
				PXUIFieldAttribute.SetVisible<ReasonCode.subMaskFinance>(sender, row, false);
			}
			else if (row.Usage == ReasonCodeUsages.CreditWriteOff || row.Usage == ReasonCodeUsages.BalanceWriteOff)
			{
				PXUIFieldAttribute.SetVisible<ReasonCode.subMaskInventory>(sender, row, false);
				PXUIFieldAttribute.SetVisible<ReasonCode.subMaskFinance>(sender, row, true);
			}
			else
			{
				PXUIFieldAttribute.SetVisible<ReasonCode.subMaskInventory>(sender, row, true);
				PXUIFieldAttribute.SetVisible<ReasonCode.subMaskFinance>(sender, row, false);
			}

		}

		protected virtual void ReasonCode_Usage_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ReasonCode row = e.Row as ReasonCode;
			if (row == null) return;

			if (row.Usage == ReasonCodeUsages.Sales)
			{
				//not applicable
			}
			else if (row.Usage == ReasonCodeUsages.CreditWriteOff || row.Usage == ReasonCodeUsages.BalanceWriteOff)
			{
				sender.SetDefaultExt<ReasonCode.subMaskFinance>(e.Row);
			}
			else
			{
				sender.SetDefaultExt<ReasonCode.subMaskInventory>(e.Row);
			}
		}


        protected virtual void ReasonCode_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            INSetup inSetup = INSetup.Select();
            var rc = (ReasonCode)e.Row;
            if(rc!=null&&inSetup!=null)
            {                
                if(inSetup.ReceiptReasonCode == rc.ReasonCodeID || inSetup.PIReasonCode == rc.ReasonCodeID || inSetup.IssuesReasonCode == rc.ReasonCodeID || inSetup.AdjustmentReasonCode == rc.ReasonCodeID)
                    throw new PXException(Messages.ReasonCodeInUse);
            }
        }


        protected virtual void ReasonCode_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				bool allowEmptyAccount = ((ReasonCode)e.Row).Usage.IsIn(ReasonCodeUsages.Sales, ReasonCodeUsages.VendorReturn, ReasonCodeUsages.Transfer);
				PXDefaultAttribute.SetPersistingCheck<ReasonCode.accountID>(sender, e.Row, allowEmptyAccount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
				PXDefaultAttribute.SetPersistingCheck<ReasonCode.subID>(sender, e.Row, allowEmptyAccount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			}
		}
	}
}
