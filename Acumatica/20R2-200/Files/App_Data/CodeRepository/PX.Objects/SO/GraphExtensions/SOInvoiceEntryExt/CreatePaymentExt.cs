using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.Extensions.PaymentTransaction;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	public class CreatePaymentExt : CreatePaymentExtBase<SOInvoiceEntry, ARInvoiceEntry, ARInvoice, ARAdjust2, ARAdjust>
	{
		public override void Initialize()
		{
			base.Initialize();

			PXAction action = Base.Actions["action"];
			if (action != null)
			{
				// this action is never used for invoices
				action.AddMenuAction(createAndAuthorizePayment);
				action.SetVisible(nameof(CreateAndAuthorizePayment), false);
			}
		}

		#region Document events

		protected virtual void _(Events.RowSelected<ARInvoice> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			bool inserted = eventArgs.Cache.GetStatus(eventArgs.Row) == PXEntryStatus.Inserted;

			bool createPaymentEnabled = !inserted &&
				eventArgs.Row.Status.IsIn(ARDocStatus.Hold, ARDocStatus.CCHold, ARDocStatus.CreditHold, ARDocStatus.Balanced) &&
				eventArgs.Row.CuryUnpaidBalance > 0m;

			createDocumentPayment.SetEnabled(createPaymentEnabled);
			importDocumentPayment.SetEnabled(createPaymentEnabled);
		}

		protected virtual void _(Events.RowSelected<SOInvoice> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			bool allowPaymentInfo = Base.Document.Cache.AllowUpdate && eventArgs.Row.DocType.IsIn(ARDocType.CashSale, ARDocType.CashReturn, ARDocType.Invoice);
			bool isPMInstanceRequired = false;

			if (allowPaymentInfo && (String.IsNullOrEmpty(eventArgs.Row.PaymentMethodID) == false))
			{
				PaymentMethod pm = PaymentMethod.PK.Find(Base, eventArgs.Row.PaymentMethodID);
				isPMInstanceRequired = (pm?.IsAccountNumberRequired == true);
			}

			PXUIFieldAttribute.SetEnabled<SOInvoice.paymentMethodID>(Base.SODocument.Cache, eventArgs.Row, allowPaymentInfo);
			PXUIFieldAttribute.SetEnabled<SOInvoice.pMInstanceID>(Base.SODocument.Cache, eventArgs.Row, allowPaymentInfo && isPMInstanceRequired);

			PXSetPropertyException setPropertyException = null;

			if (eventArgs.Row?.HasLegacyCCTran == true)
			{
				setPropertyException = new PXSetPropertyException(
					Messages.CantProcessSOInvoiceBecauseItHasLegacyCCTran, PXErrorLevel.Warning, eventArgs.Row.RefNbr);
			}

			if (Base.Document.Current != null)
			{
				Base.Document.Cache.RaiseExceptionHandling<ARInvoice.curyPaymentTotal>(
					Base.Document.Current, Base.Document.Current.CuryPaymentTotal, setPropertyException);
			}
		}

		#endregion // Document events

		#region SOQuickPayment events

		#region SOQuickPayment CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Coalesce<
						Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
								And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
								Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>,
								  And<CustomerPaymentMethod.isActive, Equal<True>,
								  And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOQuickPayment.paymentMethodID>>>>>>,
						Search<CustomerPaymentMethod.pMInstanceID,
								Where<CustomerPaymentMethod.bAccountID, Equal<Current<ARInvoice.customerID>>,
									And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOQuickPayment.paymentMethodID>>,
									And<CustomerPaymentMethod.isActive, Equal<True>>>>, OrderBy<Desc<CustomerPaymentMethod.expirationDate, Desc<CustomerPaymentMethod.pMInstanceID>>>>>)
						, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current2<ARInvoice.customerID>>,
			And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOQuickPayment.paymentMethodID>>,
			And<Where<CustomerPaymentMethod.isActive, Equal<boolTrue>, Or<CustomerPaymentMethod.pMInstanceID,
					Equal<Current<SOQuickPayment.pMInstanceID>>>>>>>>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
		protected virtual void _(Events.CacheAttached<SOQuickPayment.pMInstanceID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.cashAccountID,
									InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CustomerPaymentMethod.cashAccountID>,
										And<PaymentMethodAccount.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
										And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
									Where<CustomerPaymentMethod.bAccountID, Equal<Current<ARInvoice.customerID>>,
										And<CustomerPaymentMethod.pMInstanceID, Equal<Current2<SOQuickPayment.pMInstanceID>>>>>,
								Search2<CashAccount.cashAccountID,
								InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
									And<PaymentMethodAccount.useForAR, Equal<True>,
									And<PaymentMethodAccount.aRIsDefault, Equal<True>,
									And<PaymentMethodAccount.paymentMethodID, Equal<Current2<SOQuickPayment.paymentMethodID>>>>>>>,
									Where<CashAccount.branchID, Equal<Current<ARInvoice.branchID>>,
										And<Match<Current<AccessInfo.userName>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(ARInvoice.branchID), typeof(Search2<CashAccount.cashAccountID,
				InnerJoin<PaymentMethodAccount,
					On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
						And<PaymentMethodAccount.useForAR, Equal<True>,
						And<PaymentMethodAccount.paymentMethodID,
						Equal<Current2<SOQuickPayment.paymentMethodID>>>>>>,
						Where<Match<Current<AccessInfo.userName>>>>), SuppressCurrencyValidation = false, Required = true)]
		protected virtual void _(Events.CacheAttached<SOQuickPayment.cashAccountID> eventArgs)
		{
		}

		#endregion // SOQuickPayment CacheAttached

		#endregion // SOQuickPayment events

		#region Override methods

		public override void AuthorizePayment(ARAdjust2 adjustment, ARPaymentEntry paymentEntry, ARPaymentEntry.PaymentTransaction paymentTransaction)
		{
			paymentEntry.ForcePaymentApp = true;
			using (new ForcePaymentAppScope())
				base.AuthorizePayment(adjustment, paymentEntry, paymentTransaction);
		}

		public override void VoidPayment(ARAdjust2 adjustment, ARPaymentEntry paymentEntry, ARPaymentEntry.PaymentTransaction paymentTransaction)
		{
			paymentEntry.IgnoreNegativeOrderBal = true; /// TODO: SOCreatePayment: Temporary fix ARPayment bug (AC-159389), after fix we should remove this code.

			// TODO: SOCreatePayment: Temporary fix ARPayment bug (AC-163765), after fix we should remove this code.
			if (adjustment.IsCCPayment == true && paymentEntry.Document.Current?.IsCCAuthorized == true)
			{
				ARAdjust paymentAdjust = paymentEntry.Adjustments.Locate(adjustment);
				if (paymentAdjust != null)
				{
					paymentEntry.Adjustments.Cache.SetValueExt<ARAdjust.curyAdjgAmt>(paymentAdjust, 0m);
					paymentEntry.Adjustments.Update(paymentAdjust);
				}
			}

			base.VoidPayment(adjustment, paymentEntry, paymentTransaction);
		}

		public override void CapturePayment(ARAdjust2 adjustment, ARPaymentEntry paymentEntry, ARPaymentEntry.PaymentTransaction paymentTransaction)
		{
			paymentEntry.IgnoreNegativeOrderBal = true; // TODO: SOCreatePayment: Temporary fix ARPayment bug (AC-159389), after fix we should remove this method.
			paymentEntry.ForcePaymentApp = true;
			using (new ForcePaymentAppScope())
				base.CapturePayment(adjustment, paymentEntry, paymentTransaction);
		}

		protected override void RemoveUnappliedBalance(ARPaymentEntry paymentEntry)
		{
			SOAdjust soadjust = paymentEntry.SOAdjustments.Select().AsEnumerable()
				.Where(a => a.GetItem<SOAdjust>().CuryAdjgAmt > 0m)
				.SingleOrDefault();

			if (soadjust != null)
			{
				paymentEntry.SOAdjustments.SetValueExt<SOAdjust.curyAdjgAmt>(soadjust, 0m);
				paymentEntry.SOAdjustments.Update(soadjust);
			}

			PXFormulaAttribute.CalcAggregate<SOAdjust.curyAdjgAmt>(paymentEntry.SOAdjustments.Cache, paymentEntry.Document.Current, false);

			base.RemoveUnappliedBalance(paymentEntry);
		}

		protected override PXSelectBase<ARAdjust2> GetAdjustView()
			=> Base.Adjustments;

		protected override PXSelectBase<ARAdjust> GetAdjustView(ARPaymentEntry paymentEntry)
			=> paymentEntry.Adjustments;

		protected override ARSetup GetARSetup()
			=> Base.arsetup.Current;

		protected override CustomerClass GetCustomerClass()
			=> Base.customerclass.SelectSingle();

		protected override void SetCurrentDocument(SOInvoiceEntry graph, ARInvoice document)
		{
			graph.Document.Current = graph.Document.Search<ARInvoice.refNbr>(document.RefNbr, document.DocType);
		}

		protected override void AddAdjust(ARPaymentEntry paymentEntry, ARInvoice document)
		{
			var newAdjust = new ARAdjust2()
			{
				AdjdRefNbr = document.RefNbr,
				AdjdDocType = document.DocType
			};

			paymentEntry.Adjustments.Insert(newAdjust);
		}

		protected override void VerifyAdjustments(ARPaymentEntry paymentEntry, string actionName)
		{
			ARPayment payment = paymentEntry.Document.Current;

			if (IsMultipleApplications(paymentEntry))
			{
				if (actionName == nameof(CaptureDocumentPayment))
				{
					if (payment.DocType == ARDocType.Payment)
						throw new PXException(Messages.CapturePaymentWithMultipleApplicationsError, payment.RefNbr);
					else
						throw new PXException(Messages.CapturePrepaymentWithMultipleApplicationsError, payment.RefNbr);
				}
				else
				{
					if (payment.DocType == ARDocType.Payment)
						throw new PXException(Messages.VoidPaymentWithMultipleApplicationsError, payment.RefNbr);
					else
						throw new PXException(Messages.VoidPrepaymentWithMultipleApplicationsError, payment.RefNbr);
				}
			}
		}

		protected override string GetDocumentDescr(ARInvoice document) => document.DocDesc;

		protected override bool CanVoid(ARAdjust2 adjust, ARPayment payment)
			=> base.CanVoid(adjust, payment) && adjust.Hold != true;

		protected override void ThrowExceptionIfDocumentHasLegacyCCTran()
		{
			SOInvoice doc = Base.SODocument.Select();

			if (doc?.HasLegacyCCTran == true)
				throw new PXException(Messages.CantProcessSOInvoiceBecauseItHasLegacyCCTran, doc.RefNbr);
		}

		[PXOverride]
		public virtual ARInvoiceState GetDocumentState(PXCache cache, ARInvoice doc, Func<PXCache, ARInvoice, ARInvoiceState> baseMethod)
		{
			ARInvoiceState state = baseMethod(cache, doc);
			var invoice = (SOInvoice)Base.SODocument.View.SelectMultiBound(new[] { doc }).FirstOrDefault();
			state.LoadDocumentsEnabled &= invoice?.HasLegacyCCTran != true;
			state.AllowUpdateAdjustments &= invoice?.HasLegacyCCTran != true;

			return state;
		}

		protected override Type GetPaymentMethodField()
			=> typeof(SOInvoice.paymentMethodID);

		protected override bool IsCashSale()
			=> Base.Document.Current?.DocType.IsIn(ARInvoiceType.CashSale, ARInvoiceType.CashReturn) == true;

		protected override string GetCCPaymentIsNotSupportedMessage()
			=> Messages.CCPaymentMethodIsNotSupportedInSOInvoiceCashSale;

		protected override Type GetDocumentPMInstanceIDField()
			=> typeof(ARInvoice.pMInstanceID);

		#endregion // Override methods
	}
}
