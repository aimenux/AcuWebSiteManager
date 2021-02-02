using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.Extensions.PaymentTransaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.Automation;
using ARRegisterAlias = PX.Objects.AR.Standalone.ARRegisterAlias;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
	public class CreatePaymentExt : CreatePaymentExtBase<SOOrderEntry, SOOrder, SOAdjust>
	{
		bool isReqPrepaymentCalculationInProgress = false;

		#region Buttons

		[PXUIField(DisplayName = Messages.CreatePayment, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, Tooltip = Messages.CreatePayment)]
		protected override IEnumerable CreateDocumentPayment(PXAdapter adapter)
		{
			CheckTermsInstallmentType();
			return base.CreateDocumentPayment(adapter);
		}

		public PXAction<SOOrder> createOrderPrepayment;
		[PXUIField(DisplayName = Messages.CreatePrepayment, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, Tooltip = Messages.CreatePrepayment)]
		protected virtual IEnumerable CreateOrderPrepayment(PXAdapter adapter)
		{
			CheckTermsInstallmentType();
			if (AskCreatePaymentDialog(Messages.CreatePrepayment) == WebDialogResult.OK)
				CreatePayment(QuickPayment.Current, ARPaymentType.Prepayment, false);

			return adapter.Get();
		}

		public virtual void CheckTermsInstallmentType()
		{
			Terms terms = Terms.PK.Find(Base, Base.Document.Current.TermsID);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single)
			{
				throw new PXSetPropertyException(AR.Messages.PrepaymentAppliedToMultiplyInstallments);
			}
		}

		#endregion // Buttons

		#region SOOrder events

		protected virtual void _(Events.RowSelected<SOOrder> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			bool paymentsAndApplicationsEnabled = Base.soordertype.Current?.CanHaveApplications ?? false;
			bool inserted = eventArgs.Cache.GetStatus(eventArgs.Row) == PXEntryStatus.Inserted;

			bool createPaymentEnabled = paymentsAndApplicationsEnabled && !inserted &&
				((eventArgs.Row.Approved ?? eventArgs.Row.DontApprove == true) || eventArgs.Row.Hold == true) &&
				eventArgs.Row.Completed != true &&
				eventArgs.Row.Cancelled != true;

			createDocumentPayment.SetEnabled(createPaymentEnabled);
			createOrderPrepayment.SetEnabled(createPaymentEnabled);
			importDocumentPayment.SetEnabled(createPaymentEnabled);

			eventArgs.Cache.Adjust<PXUIFieldAttribute>(eventArgs.Row)
				.For<SOOrder.curyPrepaymentReqAmt>(a => a.Enabled = eventArgs.Row.OverridePrepayment == true)
				.SameFor<SOOrder.prepaymentReqPct>();

			PXUIFieldAttribute.SetEnabled<SOOrder.prepaymentReqSatisfied>(eventArgs.Cache, eventArgs.Row, false);

			bool isReqPrepaymentEnabled = GetRequiredPrepaymentEnabled(eventArgs.Row);

			eventArgs.Cache.Adjust<PXUIFieldAttribute>(eventArgs.Row)
				.For<SOOrder.prepaymentReqPct>(a => a.Visible = isReqPrepaymentEnabled)
				.SameFor<SOOrder.curyPrepaymentReqAmt>()
				.SameFor<SOOrder.overridePrepayment>()
				.SameFor<SOOrder.prepaymentReqSatisfied>();

			PXSetPropertyException setPropertyException = null;

			if (eventArgs.Row.HasLegacyCCTran == true)
			{
				setPropertyException = new PXSetPropertyException(
					Messages.CantProcessSOBecauseItHasLegacyCCTran, PXErrorLevel.Warning, eventArgs.Row.OrderType, eventArgs.Row.OrderNbr);
			}

			eventArgs.Cache.RaiseExceptionHandling<SOOrder.curyPaymentTotal>(
				eventArgs.Row, eventArgs.Row.CuryPaymentTotal, setPropertyException);
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.cancelled> eventArgs)
		{
			if ((bool?)eventArgs.NewValue == true)
			{
				ThrowExceptionIfDocumentHasLegacyCCTran();

				SOAdjust adj = GetPaymentLinkToOrder(eventArgs.Row);
				if (adj != null)
				{
					throw new PXSetPropertyException(Messages.OrderWithAppliedPaymentsCantBeCancelled, eventArgs.Row.OrderNbr, adj.AdjgRefNbr);
				}
			}
		}

		protected virtual void _(Events.RowDeleting<SOOrder> eventArgs)
		{
			SOAdjust adj = GetPaymentLinkToOrder(eventArgs.Row);
			if (adj != null)
			{
				throw new PXException(Messages.OrderWithAppliedPaymentsCantBeDeleted, eventArgs.Row.OrderNbr, adj.AdjgRefNbr);
			}
		}

		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.termsID> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			eventArgs.Cache.SetDefaultExt<SOOrder.overridePrepayment>(eventArgs.Row);
			eventArgs.Cache.SetDefaultExt<SOOrder.prepaymentReqPct>(eventArgs.Row);
		}

		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.overridePrepayment> eventArgs)
		{
			if (eventArgs.Row.OverridePrepayment != (bool?)eventArgs.OldValue)
			{
				if (eventArgs.Row.DontApprove != true && eventArgs.Row.Approved == true)
					eventArgs.Cache.SetValueExt<SOOrder.approved>(eventArgs.Row, false);

				if (eventArgs.Row.OverridePrepayment != true)
					eventArgs.Cache.SetDefaultExt<SOOrder.prepaymentReqPct>(eventArgs.Row);
			}
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.prepaymentReqPct> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			decimal? newValue = (decimal?)eventArgs.NewValue;

			// TODO: SOCreatePayment: Investigate why if we used "throw PXSetPropertyException" instead of RaiseExceptionHandling,
			// the system doesn't clear error message when an user change the value of the override flag. (Or changed the value of the prepayment percent)
			PXSetPropertyException setPropertyException = null;

			if (newValue < 0m)
				setPropertyException = new PXSetPropertyException<SOOrder.prepaymentReqPct>(Messages.PrepaymentPercentShouldBeMoreZero);

			if (newValue > 100m)
				setPropertyException = new PXSetPropertyException<SOOrder.prepaymentReqPct>(Messages.PrepaymentPercentShouldBeLess100);

			eventArgs.Cache.RaiseExceptionHandling<SOOrder.prepaymentReqPct>(eventArgs.Row, newValue, setPropertyException);
		}

		protected virtual void _(Events.FieldVerifying<SOOrder, SOOrder.curyPrepaymentReqAmt> eventArgs)
		{
			if (eventArgs.Row == null || !GetRequiredPrepaymentEnabled(eventArgs.Row))
				return;

			decimal? newValue = (decimal?)eventArgs.NewValue;

			PXSetPropertyException setPropertyException = null;

			if (newValue < 0m)
				setPropertyException = new PXSetPropertyException<SOOrder.curyPrepaymentReqAmt>(Messages.PrepaymentShouldBeMoreZero);

			if (newValue > eventArgs.Row.CuryOrderTotal)
				setPropertyException = new PXSetPropertyException<SOOrder.curyPrepaymentReqAmt>(
					Messages.PrepaymentShouldBeLessOrderTotalAmount, eventArgs.Row.CuryOrderTotal);

			eventArgs.Cache.RaiseExceptionHandling<SOOrder.curyPrepaymentReqAmt>(eventArgs.Row, newValue, setPropertyException);
		}

		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.prepaymentReqPct> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			if (eventArgs.Row.OverridePrepayment == true)
				SetAmountByPercent(eventArgs.Cache, eventArgs.Row);
		}

		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.curyOrderTotal> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			if (eventArgs.Row.OverridePrepayment == true)
				SetAmountByPercent(eventArgs.Cache, eventArgs.Row);
		}

		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.curyPrepaymentReqAmt> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			if (eventArgs.Row.OverridePrepayment == true && !Base.IsCopyPasteContext && !Base.IsCopyOrder)
				SetPercentByAmount(eventArgs.Cache, eventArgs.Row);
		}

		protected virtual void _(Events.RowUpdated<SOOrder> eventArgs)
		{
			if (!eventArgs.Cache.ObjectsEqual<SOOrder.curyPrepaymentReqAmt, SOOrder.curyPaymentOverall, SOOrder.completed>(eventArgs.Row, eventArgs.OldRow))
			{
				if (eventArgs.Row.CuryPaymentOverall >= eventArgs.Row.CuryPrepaymentReqAmt)
					eventArgs.Row.SatisfyPrepaymentRequirements(Base);
				else
					eventArgs.Row.ViolatePrepaymentRequirements(Base);
			}

			if (!eventArgs.Cache.ObjectsEqual<SOOrder.paymentsNeedValidationCntr>(eventArgs.Row, eventArgs.OldRow))
			{
				if (eventArgs.Row.PaymentsNeedValidationCntr == 0 && eventArgs.OldRow.PaymentsNeedValidationCntr != null)
					SOOrder.Events.Select(e => e.LostLastPaymentInPendingProcessing).FireOn(Base, eventArgs.Row);
				else if (eventArgs.OldRow.PaymentsNeedValidationCntr < eventArgs.Row.PaymentsNeedValidationCntr)
					SOOrder.Events.Select(e => e.ObtainedPaymentInPendingProcessing).FireOn(Base, eventArgs.Row);
			}
		}

		#endregion // SOOrder events

		#region SOAdjust events

		protected virtual void _(Events.RowDeleting<SOAdjust> eventArgs)
		{
			if (eventArgs.Row?.CuryAdjdBilledAmt > 0)
			{
				throw new PXException(Messages.PaymentsCantBeRemovedTransferedToInvoice, eventArgs.Row.AdjgRefNbr);
			}
		}

		#endregion // SOAdjust events

		#region Override methods

		protected override PXSelectBase<SOAdjust> GetAdjustView()
			=> Base.Adjustments;

		protected override PXSelectBase<SOAdjust> GetAdjustView(ARPaymentEntry paymentEntry)
			=> paymentEntry.SOAdjustments;

		protected override ARSetup GetARSetup()
			=> Base.arsetup.Current;

		protected override CustomerClass GetCustomerClass()
			=> Base.customerclass.SelectSingle();

		protected override void SetCurrentDocument(SOOrderEntry graph, SOOrder document)
		{
			graph.Document.Current = graph.Document.Search<SOOrder.orderNbr>(document.RefNbr, document.OrderType);
		}

		protected override void AddAdjust(ARPaymentEntry paymentEntry, SOOrder document)
		{
			var newAdjust = new SOAdjust()
			{
				AdjdOrderType = document.OrderType,
				AdjdOrderNbr = document.OrderNbr
			};

			paymentEntry.SOAdjustments.Insert(newAdjust);
		}

		protected override void VerifyAdjustments(ARPaymentEntry paymentEntry, string actionName)
		{
			SOOrder document = Base.Document.Current;
			ARPayment payment = paymentEntry.Document.Current;

			if (IsMultipleApplications(paymentEntry, out ARPaymentTotals paymentTotals, out SOInvoice invoice))
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

			if (IsPaymentLinkedToInvoiceWithTheSameOrder(paymentTotals, invoice))
			{
				if (actionName == nameof(CaptureDocumentPayment))
				{
					if (payment.DocType == ARDocType.Payment)
						throw new PXException(Messages.CaptureTransferedToInvoicePaymentError, invoice.RefNbr, document.OrderNbr, payment.RefNbr);
					else
						throw new PXException(Messages.CaptureTransferedToInvoicePrepaymentError, invoice.RefNbr, document.OrderNbr, payment.RefNbr);
				}
				else
				{
					if (payment.DocType == ARDocType.Payment)
						throw new PXException(Messages.VoidTransferedToInvoicePaymentError, invoice.RefNbr, document.OrderNbr, payment.RefNbr);
					else
						throw new PXException(Messages.VoidTransferedToInvoicePrepaymentError, invoice.RefNbr, document.OrderNbr, payment.RefNbr);
				}
			}
		}

		protected override void PrepareForCreateCCPayment(SOOrder doc)
		{
			base.PrepareForCreateCCPayment(doc);
			CheckTermsInstallmentType();
		}

		protected override bool CanVoid(SOAdjust adjust, ARPayment payment)
		{
			return base.CanVoid(adjust, payment) &&
				Base.Document.Current?.Completed == false &&
				Base.Document.Current?.Cancelled == false;
		}

		protected override bool CanCapture(SOAdjust adjust, ARPayment payment)
		{
			return base.CanCapture(adjust, payment) &&
				Base.Document.Current?.Completed == false &&
				Base.Document.Current?.Cancelled == false;
		}

		protected override string GetDocumentDescr(SOOrder document) => document.OrderDesc;

		protected override void ThrowExceptionIfDocumentHasLegacyCCTran()
		{
			SOOrder doc = Base.Document.Current;

			if (doc?.HasLegacyCCTran == true)
				throw new PXException(Messages.CantProcessSOBecauseItHasLegacyCCTran, doc.OrderType, doc.OrderNbr);
		}

		protected override Type GetPaymentMethodField()
			=> typeof(SOOrder.paymentMethodID);

		protected override bool IsCashSale()
			=> Base.IsCashSale;

		protected override string GetCCPaymentIsNotSupportedMessage()
			=> Messages.CCPaymentMethodIsNotSupportedInSOCashSale;

		protected override Type GetDocumentPMInstanceIDField()
			=> typeof(SOOrder.pMInstanceID);

		#endregion // Override methods

		#region Methods

		protected virtual SOAdjust GetPaymentLinkToOrder(SOOrder order)
		{
			if (order?.CuryPaymentTotal > 0m)
			{
				foreach (PXResult<SOAdjust, ARRegisterAlias, ARPayment, CurrencyInfo> res
					in Base.Adjustments_Raw.View.SelectMultiBound(new object[] { order }))
				{
					SOAdjust adj = res;
					if (adj?.Voided == false && adj.CuryAdjdAmt > 0m)
					{
						return adj;
					}
				}
			}

			return null;
		}

		protected virtual bool IsPaymentLinkedToInvoiceWithTheSameOrder(ARPaymentTotals paymentTotals, SOInvoice invoice)
		{
			return (paymentTotals != null && invoice != null &&
				paymentTotals.AdjdOrderType == invoice.SOOrderType &&
				paymentTotals.AdjdOrderNbr == invoice.SOOrderNbr);
		}

		protected virtual bool GetRequiredPrepaymentEnabled(SOOrder order)
		{
			return order?.Behavior == SOBehavior.SO &&
				!Base.IsTransferOrder &&
				!Base.IsNoAROrder &&
				order.TermsID != null &&
				(order.OverridePrepayment == true || order.PrepaymentReqPct > 0m);
		}

		protected virtual void SetAmountByPercent(PXCache cache, SOOrder order)
		{
			if (!isReqPrepaymentCalculationInProgress)
			{
				try
				{
					isReqPrepaymentCalculationInProgress = true;

					decimal? prepaymentAmount = 0m;

					if (order.PrepaymentReqPct == null ||
						order.PrepaymentReqPct < 0m ||
						order.PrepaymentReqPct > 100m)
					{
						prepaymentAmount = null;
					}
					else if (order.PrepaymentReqPct != 0m)
					{
						prepaymentAmount = order.CuryOrderTotal * order.PrepaymentReqPct / 100.0m;
						prepaymentAmount = PXCurrencyAttribute.RoundCury(cache, order, (decimal)prepaymentAmount);
					}
					cache.SetValueExt<SOOrder.curyPrepaymentReqAmt>(order, prepaymentAmount);
				}
				finally
				{
					isReqPrepaymentCalculationInProgress = false;
				}
			}
		}

		protected virtual void SetPercentByAmount(PXCache cache, SOOrder order)
		{
			const int PercentPrecision = 2;

			if (!isReqPrepaymentCalculationInProgress)
			{
				try
				{
					isReqPrepaymentCalculationInProgress = true;

					decimal? prepaymentPercent = 0m;

					if (order.CuryPrepaymentReqAmt == null ||
						order.CuryPrepaymentReqAmt > order.CuryOrderTotal ||
						order.CuryPrepaymentReqAmt < 0m)
					{
						prepaymentPercent = null;
					}
					else if (order.CuryPrepaymentReqAmt != 0m && (order.CuryOrderTotal ?? 0m) != 0m)
					{
						prepaymentPercent = order.CuryPrepaymentReqAmt * 100.0m / order.CuryOrderTotal;
						if (prepaymentPercent > 100.0m)
							prepaymentPercent = 100.0m;

						prepaymentPercent = PXCurrencyAttribute.Round(cache, order, (decimal)prepaymentPercent, CMPrecision.TRANCURY, PercentPrecision);
					}
					cache.SetValueExt<SOOrder.prepaymentReqPct>(order, prepaymentPercent);
				}
				finally
				{
					isReqPrepaymentCalculationInProgress = false;
				}
			}
		}

		[PXOverride]
		public virtual void CopyPasteGetScript(bool isImportSimple, List<PX.Api.Models.Command> script, List<PX.Api.Models.Container> containers,
			Action<bool, List<PX.Api.Models.Command>, List<PX.Api.Models.Container>> baseMethod)
		{
			var moveFields = new string[]
			{
				nameof(SOOrder.OverridePrepayment),
				nameof(SOOrder.PrepaymentReqPct),
				nameof(SOOrder.CuryPrepaymentReqAmt),
			};

			foreach (var field in moveFields)
			{
				int commandIndex = script.FindIndex(command => field.Equals(command.FieldName, StringComparison.OrdinalIgnoreCase));

				if (commandIndex != -1)
				{
					Api.Models.Command cmdCustField = script[commandIndex];
					Api.Models.Container cntCustField = containers[commandIndex];

					script.Remove(cmdCustField);
					containers.Remove(cntCustField);

					script.Add(cmdCustField);
					containers.Add(cntCustField);
				}
			}

			baseMethod?.Invoke(isImportSimple, script, containers);
		}

		#endregion // Methods
	}
}
