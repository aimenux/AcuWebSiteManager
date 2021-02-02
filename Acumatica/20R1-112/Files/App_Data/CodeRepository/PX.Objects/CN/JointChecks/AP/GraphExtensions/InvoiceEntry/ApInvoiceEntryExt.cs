using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.AP.Services;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.BillsAndAdjustmentsServices;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.InvoiceEntry
{
    public class ApInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
    {
        [PXCopyPasteHiddenFields(
            typeof(JointPayee.jointAmountPaid),
            typeof(JointPayee.jointBalance))]
        [PXViewDetailsButton(typeof(Vendor), typeof(Select<Vendor,
            Where<Vendor.bAccountID, Equal<Current<JointPayee.jointPayeeInternalId>>>>))]
        public PXSelect<JointPayee> JointPayees;

        private JointAmountOwedValidationService jointAmountOwedValidationService;

        public Guid? OriginalBillNoteId =>
            IsRetainageBill
                ? InvoiceDataProvider
                    .GetInvoice(Base, Base.Document.Current.OrigDocType, Base.Document.Current.OrigRefNbr)?.NoteID
                : Base.Document.Current?.NoteID;

        private bool IsRetainageBill => Base.Document.Current?.IsRetainageDocument == true;

        private APInvoiceJCExt InvoiceExtension =>
            PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(Base.Document.Current);

        public APTranCalculationService ApTranCalculationService;

		public IEnumerable jointPayees()
        {
            return new PXSelect<JointPayee,
                Where<JointPayee.billId, Equal<Required<JointPayee.billId>>>>(Base).Select(OriginalBillNoteId);
        }

        public override void Initialize()
        {
			AddEmptyJointPayeesValidationHandlers();

            InitializeValidationService(Base.Document.Current?.PaymentsByLinesAllowed == true);

            ApTranCalculationService = new APTranCalculationService(Base);

			base.Initialize();
		}

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() &&
                !SiteMapExtension.IsTaxBillsAndAdjustmentsScreenId();
        }

        [PXOverride]
        public virtual void Persist(Action baseHandler)
        {
            if (Base.Document.Current?.Status == APDocStatus.PendingApproval)
            {
                ValidateEmptyJointPayees();
            }
            baseHandler();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<Vendor.type, NotEqual<BAccountType.employeeType>>),
            JointCheckMessages.OnlyVendorsShouldBeAllowed)]
        protected virtual void JointPayee_JointPayeeInternalId_CacheAttached(PXCache cache)
        {
        }

		protected virtual void _(Events.FieldSelecting<JointPayee, JointPayee.billLineAmount> args)
        {
	        APInvoice document = Base.Document.Current;

			if (args.Row != null && document != null && document.PaymentsByLinesAllowed == true && args.Row.BillLineNumber != null)
			{
				string docType = null;
				string refNbr = null;

				if (document.IsRetainageDocument == true)
				{
					docType = document.OrigDocType;
					refNbr = document.OrigRefNbr;
				}
				else
				{
					docType = document.DocType;
					refNbr = document.RefNbr;
				}

				decimal? curyOrigTranAmt = ApTranCalculationService.CalcCuryOrigTranAmts(
						docType, 
						refNbr,
						args.Row.BillLineNumber.SingleToArray())
					.Values.Single();

				args.Row.BillLineAmount = curyOrigTranAmt;
                args.ReturnValue = curyOrigTranAmt;
            }
        }

        protected virtual void _(Events.RowSelected<APInvoice> args)
        {
            var invoice = args.Row;
            if (invoice == null)
            {
                return;
            }
            PXUIFieldAttribute.SetVisible<APTran.curyTranBal>(Base.Transactions.Cache, null, true);
            var isBill = invoice.DocType == APDocType.Invoice;
            ClearJointPayeesIfRequired(isBill);
            UpdateDocumentFormLayout(isBill);
        }

        protected virtual void _(Events.FieldUpdated<JointPayee, JointPayee.jointPayeeInternalId> args)
        {
            if (args.Row != null && args.NewValue == null)
            {
                JointPayees.View.RequestRefresh();
            }
        }

        protected virtual void _(Events.RowSelected<JointPayeePayment> args)
        {
            var jointPayeePayment = args.Row;
            if (jointPayeePayment != null)
            {
                PXUIFieldAttribute.SetVisible<JointPayeePayment.billLineNumber>(args.Cache, null,
                    Base.Document.Current.PaymentsByLinesAllowed.GetValueOrDefault());
                PXUIFieldAttribute.SetDisplayName<JointPayeePayment.jointAmountToPay>(
                    args.Cache, JointCheckLabels.JointAmountToPay);
            }
        }

        protected void _(Events.FieldUpdated<APTran.curyTranAmt> args)
        {
            if (args.Row is APTran apTran)
            {
                var jointPayees = JointPayees.SelectMain().Where(jp => jp.BillLineNumber == apTran.LineNbr);
                jointPayees.ForEach(jp => jp.BillLineAmount = apTran.CuryTranAmt);
            }
        }

        protected void _(Events.RowUpdated<APTax> args)
        {
	        APInvoice document = Base.Document.Current;

			if (document != null && document.PaymentsByLinesAllowed == true && args.Row is APTax apTax)
	        {
		        decimal? curyOrigTranAmt = ApTranCalculationService.CalcCuryOrigTranAmts(
				        apTax.TranType,
				        apTax.RefNbr,
				        apTax.LineNbr.SingleToArray())
			        .Values.Single();

		        var jointPayees = JointPayees.SelectMain().Where(jp => jp.BillLineNumber == apTax.LineNbr);
		        jointPayees.ForEach(jp => jp.BillLineAmount = curyOrigTranAmt);
	        }
        }

		protected virtual void _(Events.FieldUpdated<APInvoice, APInvoiceJCExt.isJointPayees> args)
        {
            if (!ShouldDeleteJointPayees())
            {
                return;
            }
            var jointPayees = JointPayees.Select().FirstTableItems.ToList();
            jointPayees.ForEach(jointPayee => JointPayees.Delete(jointPayee));
        }

        protected virtual void _(Events.RowPersisting<APInvoice> args)
        {
            if (args.Operation == PXDBOperation.Delete)
            {
                return;
            }
            if (args.Operation == PXDBOperation.Insert)
            {
                args.Cache.GetExtension<APInvoiceJCExt>(args.Row).IsPaymentCycleWorkflow = false;
            }
            if (!IsRetainageBill)
            {
                JointPayees.SelectMain().ForEach(jointAmountOwedValidationService.ValidateAmountOwed);
                jointAmountOwedValidationService.ValidateJointPayeesAmountOwedWithUnreleasedPayments();
			}
        }

        protected virtual void _(Events.RowDeleting<APInvoice> args)
        {
            JointPayees.Cache.Clear();
        }

        protected virtual void _(Events.RowInserting<JointPayee> args)
        {
            var jointPayee = args.Row;
            if (jointPayee == null)
            {
                return;
            }
            if (Base.IsCopyPasteContext && !JointPayees.AllowInsert)
            {
                args.Cancel = true;
                return;
            }
            InitializeValidationService(Base.Document.Current?.PaymentsByLinesAllowed == true);
            jointPayee.BillId = Base.Document.Current?.NoteID;
            jointPayee.JointAmountPaid = 0m;
            jointAmountOwedValidationService.RecalculateTotalJointAmount();
        }

        protected virtual void _(Events.FieldDefaulting<JointPayee.billLineNumber> args)
        {
            var isPaymentsByLinesAllowed = Base.CurrentDocument.Current.PaymentsByLinesAllowed.GetValueOrDefault();
            if (!isPaymentsByLinesAllowed)
            {
                args.NewValue = 0;
            }
        }

        protected virtual void _(Events.FieldUpdated<JointPayee, JointPayee.jointAmountOwed> args)
        {
            var jointPayee = args.Row;
            InitializeValidationService(Base.Document.Current?.PaymentsByLinesAllowed == true &&
                jointPayee?.BillLineNumber != null);
            jointAmountOwedValidationService.RecalculateTotalJointAmount();
        }

        protected virtual void _(Events.RowDeleted<JointPayee> args)
        {
            jointAmountOwedValidationService.RecalculateTotalJointAmount();
        }

        protected virtual void _(Events.RowDeleting<APTran> args)
        {
            var transaction = args.Row;
            var jointPayees = JointPayees.SelectMain().Where(jp => jp.BillLineNumber == transaction.LineNbr).ToList();
            if (jointPayees.Any() && Base.Document.Cache.Deleted.Empty_())
            {
                if (IsLineUnlinkConfirmed())
                {
                    UnlinkBillLine(jointPayees);
                }
                else
                {
                    args.Cancel = true;
                    Base.Transactions.View.RequestRefresh();
                }
            }
        }

		public virtual void _(Events.FieldVerifying<JointPayee.jointAmountOwed> e)
		{
			JointPayee payee = (JointPayee)e.Row;

			VerifyJointAmountOwed(e.Cache, payee, (decimal?) e.NewValue);
		}

		public virtual void _(Events.RowPersisting<JointPayee> e)
		{
			if ((e.Operation == PXDBOperation.Insert
			     || e.Operation == PXDBOperation.Update)
			    && e.Row.BillLineNumber != null
				&& !IsRetainageBill)
			{
				VerifyJointAmountOwed(e.Cache, e.Row, e.Row.JointAmountOwed);
			}
		}

		public virtual void VerifyJointAmountOwed(PXCache cache, JointPayee payee, decimal? amountOwed)
		{
			ProcessingResult result =
				jointAmountOwedValidationService.ValidateJointAmountOwedWithUnreleasedPayments(
					payee.BillLineNumber,
					payee.SingleToList(),
					amountOwed,
					true);

			if (result.HasWarningOrError)
			{
				cache.RaiseExceptionHandling<JointPayee.jointAmountOwed>(
					payee,
					amountOwed,
					new PXSetPropertyException(result.GetGeneralMessage()));
			}
		}

		private bool IsLineUnlinkConfirmed()
        {
            return Base.Transactions.Ask(SharedMessages.Warning, JointCheckMessages.UnlinkedApBillLineFromJointPayees,
                MessageButtons.YesNo, MessageIcon.Warning) == WebDialogResult.Yes;
        }

        private void UnlinkBillLine(IEnumerable<JointPayee> jointPayees)
        {
            foreach (var jointPayee in jointPayees)
            {
                jointPayee.BillLineNumber = null;
                JointPayees.Cache.Update(jointPayee);
            }
        }

        private void AddEmptyJointPayeesValidationHandlers()
        {
            Base.approve.ButtonTogglingEvents += EmptyJointPayeesValidationHandler;
            Base.release.ButtonTogglingEvents += EmptyJointPayeesValidationHandler;
            Base.prebook.ButtonTogglingEvents += EmptyJointPayeesValidationHandler;
        }

        private void EmptyJointPayeesValidationHandler(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            ValidateEmptyJointPayees();
        }

        private bool ShouldDeleteJointPayees()
        {
            if (Base.Document.Current == null || InvoiceExtension.IsJointPayees == true || !JointPayees.Any())
            {
                return false;
            }
            if (IsDeletingJointPayeesConfirmed())
            {
                return true;
            }
            InvoiceExtension.IsJointPayees = true;
            return false;
        }

        private bool IsDeletingJointPayeesConfirmed()
        {
            var confirmationDialogResult = JointPayees.Ask(SharedMessages.Warning,
                JointCheckMessages.DeleteJointPayeesWarning, MessageButtons.YesNo, MessageIcon.Warning);
            return confirmationDialogResult == WebDialogResult.Yes;
        }

        private void UpdateDocumentFormLayout(bool isBill)
        {
            UpdateJointPayeeCheckboxAvailability(isBill);
            UpdatePaymentsByLinesAllowedCheckboxAvailability();
            UpdateJointPayeesTabAvailability();
            UpdateJointPayeesTabVisibility();
            UpdateActionsAvailability(isBill);
        }

        private void UpdateActionsAvailability(bool isBill)
        {
            if (isBill && InvoiceExtension.IsJointPayees.GetValueOrDefault())
            {
                Base.action.SetEnabled(JointCheckActions.AddToSchedule, false);
            }
        }

        private void UpdateJointPayeesTabAvailability()
        {
            var isAmountEditable = Base.Document.Cache.GetEnabled<APInvoice.curyOrigDocAmt>(Base.Document.Current);
            JointPayees.AllowInsert = isAmountEditable && !IsRetainageBill;
            JointPayees.AllowDelete = isAmountEditable && !IsRetainageBill;
            JointPayees.AllowUpdate = isAmountEditable && !IsRetainageBill;
        }

        private void UpdateJointPayeesTabVisibility()
        {
            var isPaymentsByLinesAllowed = Base.CurrentDocument.Current.PaymentsByLinesAllowed.GetValueOrDefault();
            PXUIFieldAttribute.SetVisible<JointPayee.billLineNumber>(JointPayees.Cache, null, isPaymentsByLinesAllowed);
            PXUIFieldAttribute.SetVisible<JointPayee.billLineAmount>(JointPayees.Cache, null, isPaymentsByLinesAllowed);
            PXUIFieldAttribute.SetRequired<JointPayee.billLineNumber>(JointPayees.Cache, isPaymentsByLinesAllowed);
        }

        private void UpdateJointPayeeCheckboxAvailability(bool isBill)
        {
            PXUIFieldAttribute.SetVisible<APInvoiceJCExt.isJointPayees>(Base.Document.Cache, null, isBill);
            if (IsRetainageBill)
            {
                PXUIFieldAttribute.SetEnabled<APInvoiceJCExt.isJointPayees>(Base.Document.Cache, null, false);
            }
        }

        private void UpdatePaymentsByLinesAllowedCheckboxAvailability()
        {
            if (JointPayees.Any())
            {
                PXUIFieldAttribute.SetEnabled<APInvoice.paymentsByLinesAllowed>(Base.Document.Cache, null, false);
            }
        }

        private void ClearJointPayeesIfRequired(bool isBill)
        {
            if (isBill)
            {
                return;
            }
            InvoiceExtension.IsJointPayees = false;
            JointPayees.Cache.Clear();
        }

        private void ValidateEmptyJointPayees()
        {
            var isJointPayees = InvoiceExtension.IsJointPayees.GetValueOrDefault();
            var hasJointPayees = JointPayees.Any();
            if (isJointPayees && !hasJointPayees)
            {
                throw new PXSetPropertyException(JointCheckMessages.JointPayeesRequired);
            }
        }

        private void InitializeValidationService(bool condition)
        {
            jointAmountOwedValidationService = condition
                ? new JointAmountOwedPerLineValidationService(JointPayees, Base)
                : new JointAmountOwedValidationService(JointPayees, Base);
        }
    }
}