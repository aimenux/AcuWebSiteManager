using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.DR
{
	public class DeferredCodeMaint : PXGraph<DeferredCodeMaint, DRDeferredCode>
	{
		public PXSelect<DRDeferredCode> deferredcode;
		public PXSetup<DRSetup> Setup;

		public DeferredCodeMaint()
		{
			DRSetup setup = Setup.Current;
		}

		private void SetPeriodicallyControlsState(PXCache cache, DRDeferredCode s)
		{
			PXUIFieldAttribute.SetEnabled<DRDeferredCode.fixedDay>(cache, s, s.ScheduleOption == DRScheduleOption.ScheduleOptionFixedDate);
		}

		protected virtual void DRDeferredCode_Method_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRDeferredCode row = e.Row as DRDeferredCode;

			if (row != null && row.Method == DeferredMethodType.CashReceipt)
			{
				row.AccountType = DeferredAccountType.Income;
			}

			if (DeferredMethodType.RequiresTerms(row))
			{
				sender.SetValueExt<DRDeferredCode.accountType>(row, DeferredAccountType.Income);
				sender.SetDefaultExt<DRDeferredCode.startOffset>(row);
				sender.SetDefaultExt<DRDeferredCode.occurrences>(row);
			}
			else
			{
				sender.SetDefaultExt<DRDeferredCode.recognizeInPastPeriods>(row);
			}

			if (row.Method == DeferredMethodType.CashReceipt)
			{
				sender.SetValueExt<DRDeferredCode.accountType>(row, DeferredAccountType.Income);
			}
		}

		protected virtual void DRDeferredCode_MultiDeliverableArrangement_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRDeferredCode row = e.Row as DRDeferredCode;

			if (row != null && row.MultiDeliverableArrangement == true)
			{
				foreach (var field in new Type[] {
					typeof(DRDeferredCode.method),
					typeof(DRDeferredCode.reconNowPct),
					typeof(DRDeferredCode.startOffset),
					typeof(DRDeferredCode.occurrences),
					typeof(DRDeferredCode.accountType),
					typeof(DRDeferredCode.accountSource),
					typeof(DRDeferredCode.deferralSubMaskAR),
					typeof(DRDeferredCode.deferralSubMaskAP),
					typeof(DRDeferredCode.copySubFromSourceTran),
					typeof(DRDeferredCode.accountID),
					typeof(DRDeferredCode.subID),
					typeof(DRDeferredCode.frequency),
					typeof(DRDeferredCode.scheduleOption),
					typeof(DRDeferredCode.fixedDay)
				})
				{
					sender.SetDefaultExt(row, field.Name);
				}
			}
		}

		protected virtual void DRDeferredCode_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var code = e.Row as DRDeferredCode;
			if (code == null) return;

			PXUIFieldAttribute.SetVisible(sender, code, null, true);

			SetPeriodicallyControlsState(sender, code);

			var accountIsOptional = code.MultiDeliverableArrangement == true || code.AccountSource == DeferralAccountSource.Item;
			var subIsOptional = code.MultiDeliverableArrangement == true || code.CopySubFromSourceTran == true;

			bool isFlexible = DeferredMethodType.RequiresTerms(code);

			PXUIFieldAttribute.SetEnabled<DRDeferredCode.accountType>(sender, code, !isFlexible && code.Method != DeferredMethodType.CashReceipt);
			PXUIFieldAttribute.SetEnabled<DRDeferredCode.startOffset>(sender, code, !isFlexible);
			PXUIFieldAttribute.SetEnabled<DRDeferredCode.occurrences>(sender, code, !isFlexible);
			PXUIFieldAttribute.SetEnabled<DRDeferredCode.deferralSubMaskAR>(sender, code, code.CopySubFromSourceTran != true);
			PXUIFieldAttribute.SetEnabled<DRDeferredCode.deferralSubMaskAP>(sender, code, code.CopySubFromSourceTran != true);

			PXUIFieldAttribute.SetVisible<DRDeferredCode.deferralSubMaskAR>(sender, code, code.AccountType == DeferredAccountType.Income);
			PXUIFieldAttribute.SetVisible<DRDeferredCode.deferralSubMaskAP>(sender, code, code.AccountType == DeferredAccountType.Expense);
			PXUIFieldAttribute.SetVisible<DRDeferredCode.recognizeInPastPeriods>(sender, code, isFlexible);

			if (code.MultiDeliverableArrangement == true)
			{
				PXUIFieldAttribute.SetVisible(sender, code, null, false);
				PXUIFieldAttribute.SetVisible<DRDeferredCode.deferredCodeID>(sender, code, true);
				PXUIFieldAttribute.SetVisible<DRDeferredCode.description>(sender, code, true);
				PXUIFieldAttribute.SetVisible<DRDeferredCode.multiDeliverableArrangement>(sender, code, true);
				PXUIFieldAttribute.SetVisible<DRDeferredCode.active>(sender, code, true);
			}
		}

		protected virtual void DRDeferredCode_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var code = e.Row as DRDeferredCode;

			if (code == null)
				return;

			var inventory = new PXSelect<InventoryItem, Where<InventoryItem.deferredCode, Equal<Current<DRDeferredCode.deferredCodeID>>>>(this).SelectSingle();
			var component = new PXSelect<INComponent, Where<INComponent.deferredCode, Equal<Current<DRDeferredCode.deferredCodeID>>>>(this).SelectSingle();
			var unreleasedARTran = new PXSelect<AR.ARTran, Where<AR.ARTran.released, Equal<False>, And<AR.ARTran.deferredCode, Equal<Current<DRDeferredCode.deferredCodeID>>>>>(this).SelectSingle();
			var unreleasedAPTran = new PXSelect<AP.APTran, Where<AP.APTran.released, Equal<False>, And<AP.APTran.deferredCode, Equal<Current<DRDeferredCode.deferredCodeID>>>>>(this).SelectSingle();

			var checks = new List<Tuple<string, object>>
			{
				new Tuple<string, object>(IN.Messages.InventoryItem, inventory),
				new Tuple<string, object>(IN.Messages.Component, component),
				new Tuple<string, object>(AR.Messages.ARTran, unreleasedARTran),
				new Tuple<string, object>(AP.Messages.APTran, unreleasedAPTran)
			};

			foreach (var check in checks)
			{
				if (check.Item2 != null)
					throw new PXException(Messages.CodeInUseCantDelete, PXMessages.LocalizeNoPrefix(check.Item1));
			}
		}

		protected virtual void DRDeferredCode_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			DRDeferredCode deferralCode = e.Row as DRDeferredCode;

			PXDefaultAttribute.SetPersistingCheck<DRDeferredCode.accountID>(sender, deferralCode, deferralCode.MultiDeliverableArrangement == true 
				|| deferralCode.AccountSource == DeferralAccountSource.Item ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<DRDeferredCode.subID>(sender, deferralCode, deferralCode.MultiDeliverableArrangement == true 
				|| deferralCode.CopySubFromSourceTran == true ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

			if (!DeferredMethodType.RequiresTerms(deferralCode) &&
				deferralCode.MultiDeliverableArrangement != true &&
				deferralCode.Occurrences == 0)
			{
				sender.RaiseExceptionHandling<DRDeferredCode.occurrences>(
					e.Row,
					deferralCode.Occurrences,
					new PXSetPropertyException(Messages.NoZeroOccurrencesForRecognitionMethod));
			}
		}

		private void CheckAccountType(PXCache sender, object Row, Int32? AccountID, string AccountType)
		{
			Account account = null;

			if (AccountID == null)
			{
				account = (Account)PXSelectorAttribute.Select<DRDeferredCode.accountID>(sender, Row);
			}
			else
			{
				account = (Account)PXSelectorAttribute.Select<DRDeferredCode.accountID>(sender, Row, AccountID);
			}

			if (account != null && AccountType == "E" && account.Type != "A")
			{
				sender.RaiseExceptionHandling<DRDeferredCode.accountID>(Row, account.AccountCD, new PXSetPropertyException(CS.Messages.AccountTypeWarn, PXErrorLevel.Warning, GL.Messages.Asset));
			}
			if (account != null && AccountType == "I" && account.Type != "L")
			{
				sender.RaiseExceptionHandling<DRDeferredCode.accountID>(Row, account.AccountCD, new PXSetPropertyException(CS.Messages.AccountTypeWarn, PXErrorLevel.Warning, GL.Messages.Liability));
			}
		}

		protected virtual void DRDeferredCode_AccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CheckAccountType(sender, e.Row, (Int32?)e.NewValue, ((DRDeferredCode)e.Row).AccountType);
		}

		protected virtual void DRDeferredCode_AccountType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CheckAccountType(sender, e.Row, null, (string)e.NewValue);
		}

		protected virtual void DRDeferredCode_MultiDeliverableArrangement_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var code = e.Row as DRDeferredCode;

			if (code != null && (bool?)e.NewValue != true && code.MultiDeliverableArrangement == true)
			{
				IN.InventoryItem splittedItem = new PXSelect<InventoryItem, Where<InventoryItem.deferredCode, Equal<Current<DRDeferredCode.deferredCodeID>>>>(this).SelectSingle();

				if (splittedItem != null)
					throw new PXSetPropertyException<DRDeferredCode.multiDeliverableArrangement>(Messages.MDAInUse);
			}
		}
	}
}
