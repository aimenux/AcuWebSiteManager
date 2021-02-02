using System;
using System.Collections;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Attributes;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AP;

namespace PX.Objects.CA
{
	public class PaymentMethodMaint : PXGraph<PaymentMethodMaint, PaymentMethod>
	{
		#region Type Override events
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID, Where<CCProcessingCenter.isActive, Equal<True>>>))]
		[PXParent(typeof(Select<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, Equal<Current<CCProcessingCenterPmntMethod.processingCenterID>>>>))]
		[PXUIField(DisplayName = "Proc. Center ID")]
		[DisabledProcCenter(CheckFieldValue = DisabledProcCenterAttribute.CheckFieldVal.ProcessingCenterId)]
		protected virtual void CCProcessingCenterPmntMethod_ProcessingCenterID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CA.PaymentMethod.paymentMethodID))]
		[PXSelector(typeof(Search<CA.PaymentMethod.paymentMethodID>))]
		[PXUIField(DisplayName = "Payment Method")]
		[PXParent(typeof(Select<CA.PaymentMethod, Where<CA.PaymentMethod.paymentMethodID, Equal<Current<CCProcessingCenterPmntMethod.paymentMethodID>>>>))]
		protected virtual void CCProcessingCenterPmntMethod_PaymentMethodID_CacheAttached(PXCache sender)
		{
		}

		/// <summary>
		/// Overriding CashAccount attribute of the DAC <see cref="PaymentMethodAccount.CashAccountID"/> property to suppress CashAccount active property verification by PXRestrictor attribute
		/// which works incorrectly on key fields. Instead in this graph we use verification of <see cref="CashAccount.Active"/> in events.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[CashAccount(suppressActiveVerification: true, IsKey = true, DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible,
					 DescriptionField = typeof(CashAccount.descr))]
		protected virtual void PaymentMethodAccount_CashAccountID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region Public Selects
		public PXSelect<PaymentMethod> PaymentMethod;
		public PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>
			> PaymentMethodCurrent;
		[PXCopyPasteHiddenFields(typeof(PaymentMethodDetail.paymentMethodID))]
		public PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>> Details;

		[PXCopyPasteHiddenFields(typeof(PaymentMethodDetail.paymentMethodID))]
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>
			> DetailsForReceivable;
		[PXCopyPasteHiddenFields(typeof(PaymentMethodDetail.paymentMethodID))]
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
				  Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>
			> DetailsForCashAccount;
		[PXCopyPasteHiddenFields(typeof(PaymentMethodDetail.paymentMethodID))]
		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
				And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
				  Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>,
			OrderBy<Asc<PaymentMethodDetail.orderIndex>>
			> DetailsForVendor;

		// We need this dummy view for correct VendorPaymentMethodDetail
		// records deletion using PXParentAttribute on an appropriate DAC.
		// 
		public PXSelect<VendorPaymentMethodDetail> dummy_VendorPaymentMethodDetail;

		public PXSelectJoin<PaymentMethodAccount,
			InnerJoin<CashAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
			Where<PaymentMethodAccount.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>
			> CashAccounts;
		public PXSelect<CCProcessingCenterPmntMethod, Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>>
			> ProcessingCenters;

		public PXSelect<CCProcessingCenterPmntMethod, Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
			And<CCProcessingCenterPmntMethod.isDefault, Equal<True>>>> DefaultProcCenter;

		#endregion
		public PaymentMethodMaint()
		{
			GLSetup setup = GLSetup.Current;
		}

		public PXSetup<GLSetup> GLSetup;

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			try
			{
				return base.Persist(cacheType, operation);
			}
			catch (PXDatabaseException e)
			{
				if (cacheType == typeof(PaymentMethodAccount)
					&& (operation == PXDBOperation.Delete
						|| operation == PXDBOperation.Command)
					&& (e.ErrorCode == PXDbExceptions.DeleteForeignKeyConstraintViolation
						|| e.ErrorCode == PXDbExceptions.DeleteReferenceConstraintViolation))
				{
					CashAccount ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.
						Select(this, e.Keys[1]);
					string CashAccountCD = ca.CashAccountCD;
					throw new PXException(Messages.CannotDeletePaymentMethodAccount, e.Keys[0], CashAccountCD);
				}
				else
				{
					throw;
				}
			}
		}

		#region Header Events
		protected virtual void PaymentMethod_PaymentType_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{

			bool found = false;
			foreach (PaymentMethodDetail iDet in this.Details.Select())
			{
				found = true; break;
			}
			if (found)
			{

				WebDialogResult res = this.PaymentMethod.Ask(Messages.AskConfirmation, Messages.PaymentMethodDetailsWillReset, MessageButtons.YesNo);
				if (res != WebDialogResult.Yes)
				{
					PaymentMethod row = (PaymentMethod)e.Row;
					e.Cancel = true;
					e.NewValue = row.PaymentType;
				}
			}
		}
		protected virtual void PaymentMethod_PaymentType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			cache.SetDefaultExt<PaymentMethod.aRHasBillingInfo>(row);
		}

		protected virtual void PaymentMethod_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			PaymentMethod row = (PaymentMethod)e.Row;

			bool isCreditCard = (row.PaymentType == PaymentMethodType.CreditCard);
			bool printChecks = (row.APPrintChecks == true);
			bool createBatch = (row.APCreateBatchPayment == true);

			bool useForAP = row.UseForAP.GetValueOrDefault(false);
			bool useForAR = row.UseForAR.GetValueOrDefault(false);

			PXUIFieldAttribute.SetVisible<PaymentMethod.aPCheckReportID>(cache, row, printChecks);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aPStubLines>(cache, row, printChecks);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aPPrintRemittance>(cache, row, printChecks);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aPRemittanceReportID>(cache, row, printChecks);

			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPPrintRemittance>(cache, row, printChecks);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPRemittanceReportID>(cache, row, printChecks && (row.APPrintRemittance == true));
			PXUIFieldAttribute.SetRequired<PaymentMethod.aPRemittanceReportID>(cache, printChecks && (row.APPrintRemittance == true));
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPCheckReportID>(cache, row, printChecks);
			PXUIFieldAttribute.SetRequired<PaymentMethod.aPCheckReportID>(cache, printChecks);

			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPPrintChecks>(cache, row, true);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPStubLines>(cache, row, printChecks);

			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPCreateBatchPayment>(cache, row, !printChecks);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aPBatchExportSYMappingID>(cache, row, createBatch);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aPBatchExportSYMappingID>(cache, row, createBatch);
			PXUIFieldAttribute.SetRequired<PaymentMethod.aPBatchExportSYMappingID>(cache, createBatch);

			PXUIFieldAttribute.SetVisible<PaymentMethod.aPRequirePaymentRef>(cache, row, !printChecks && !createBatch);

			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isExpirationDate>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isIdentifier>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isOwnerName>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.displayMask>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<PaymentMethodDetail.isCCProcessingID>(this.Details.Cache, null, isCreditCard);

			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isExpirationDate>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isIdentifier>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isIdentifier>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.displayMask>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.isCCProcessingID>(this.Details.Cache, null, isCreditCard);
			PXUIFieldAttribute.SetVisible<PaymentMethod.aRDepositAsBatch>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<PaymentMethod.aRDepositAsBatch>(cache, null, false);

			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.useForAP>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPIsDefault>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPAutoNextNbr>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPLastRefNbr>(this.CashAccounts.Cache, null, useForAP);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPBatchLastRefNbr>(this.CashAccounts.Cache, null, useForAP);

			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.useForAR>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRIsDefault>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRAutoNextNbr>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRLastRefNbr>(this.CashAccounts.Cache, null, useForAR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aRIsDefaultForRefund>(this.CashAccounts.Cache, null, useForAR);

			bool showProcCenters = (row.ARIsProcessingRequired == true);
			this.ProcessingCenters.Cache.AllowDelete = showProcCenters;
			this.ProcessingCenters.Cache.AllowUpdate = showProcCenters;
			this.ProcessingCenters.Cache.AllowInsert = showProcCenters;

			PXResultset<CCProcessingCenterPmntMethod> currDefaultProcCenter = DefaultProcCenter.Select();

			if (row.ARIsProcessingRequired == true && currDefaultProcCenter.Count == 0)
			{
				PaymentMethod.Cache.RaiseExceptionHandling<PaymentMethod.aRIsProcessingRequired>(row, row.ARIsProcessingRequired,
					new PXSetPropertyException(Messages.NoProcCenterSetAsDefault, PXErrorLevel.Warning));
			}
			else
			{
				PXFieldState state = (PXFieldState)cache.GetStateExt<PaymentMethod.aRIsProcessingRequired>(row);
				if (state.IsWarning && String.Equals(state.Error, Messages.NoProcCenterSetAsDefault))
				{
					PaymentMethod.Cache.RaiseExceptionHandling<PaymentMethod.aRIsProcessingRequired>(row, null, null);
				}
			}
		}

		protected virtual void PaymentMethod_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			PaymentMethod oldRow = (PaymentMethod)e.OldRow;
			if (oldRow.PaymentType != row.PaymentType)
			{
				foreach (PaymentMethodDetail iDet in this.Details.Select())
				{
					this.Details.Cache.Delete(iDet);
				}
				if (row.PaymentType == PaymentMethodType.CreditCard)
				{
					this.fillCreditCardDefaults();
				}
				row.ARIsOnePerCustomer = row.PaymentType == PaymentMethodType.CashOrCheck;
			}

			if ((oldRow.UseForAR != row.UseForAR) && row.UseForAR.GetValueOrDefault(false) == false)
			{
				row.ARIsProcessingRequired = false;

				foreach (PaymentMethodAccount pma in CashAccounts.Select())
				{
					pma.UseForAR = pma.ARIsDefault = pma.ARIsDefaultForRefund = false;
					CashAccounts.Update(pma);
				}
			}

			if ((oldRow.UseForAP != row.UseForAP) && row.UseForAP.GetValueOrDefault(false) == false)
			{
				foreach (PaymentMethodAccount pma in CashAccounts.Select())
				{
					pma.UseForAP = pma.APIsDefault = false;
					CashAccounts.Update(pma);
				}
			}

		}
		protected virtual void PaymentMethod_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			foreach (PaymentMethodDetail iDet in this.Details.Select())
			{
				this.Details.Cache.Delete(iDet);
			}
			if (row.PaymentType == PaymentMethodType.CreditCard)
			{
				this.fillCreditCardDefaults();
				row.ARIsOnePerCustomer = false;
			}
			row.ARIsOnePerCustomer = row.PaymentType == PaymentMethodType.CashOrCheck;
		}

		protected virtual void _(Events.RowPersisting<PaymentMethod> e)
		{
			if (e.Operation != PXDBOperation.Delete)
			{
				VerifyAPRequirePaymentRefAndAPAdditionalProcessing(e.Row.APRequirePaymentRef, e.Row.APAdditionalProcessing);
			}

			if (e.Operation == PXDBOperation.Delete)
			{
				foreach (PXResult<PaymentMethodAccount, CashAccount> row in CashAccounts.Select())
				{
					VerifyCashAccountLinkOrMethodCanBeDeleted(row);
				}
			}
		}

		protected virtual void PaymentMethod_ARHasBillingInfo_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			if (row.PaymentType == PaymentMethodType.CreditCard)
			{
				e.NewValue = true;
				e.Cancel = true;
			}


		}

		protected virtual void _(Events.FieldVerifying<PaymentMethod.aPAdditionalProcessing> e)
		{
			string newValue = (string) e.NewValue;

			if (newValue != null)
			{
				VerifyAPRequirePaymentRefAndAPAdditionalProcessing(null, newValue);
			}
		}

		protected virtual void _(Events.FieldVerifying<PaymentMethod.aPRequirePaymentRef> e)
		{
			bool? newValue = (bool?)e.NewValue;

			if (newValue != null)
			{
				VerifyAPRequirePaymentRefAndAPAdditionalProcessing(newValue, null);
			}
		}

		protected virtual void PaymentMethod_APAdditionalProcessing_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (PaymentMethod)e.Row;
			switch(row.APAdditionalProcessing)
			{
				case CA.PaymentMethod.aPAdditionalProcessing.PrintChecks:
					row.APPrintChecks = true;
					row.APCreateBatchPayment = false;
					break;
				case CA.PaymentMethod.aPAdditionalProcessing.CreateBatchPayment:
					row.APCreateBatchPayment = true;
					row.APPrintChecks = false;
					break;
				default:
					row.APPrintChecks = false;
					row.APCreateBatchPayment = false;
					break;
			}

			if(row.APPrintChecks == true || row.APCreateBatchPayment == true)
			{
				sender.SetValuePending<PaymentMethod.aPRequirePaymentRef>(row, true);
			}

			if(row.APPrintChecks != true)
			{
				sender.SetDefaultExt<PaymentMethod.aPCheckReportID>(row);
				sender.SetDefaultExt<PaymentMethod.aPStubLines>(row);
				sender.SetDefaultExt<PaymentMethod.aPPrintRemittance>(row);
				sender.SetDefaultExt<PaymentMethod.aPRemittanceReportID>(row);
			}

			if(row.APCreateBatchPayment != true)
			{
				sender.SetDefaultExt<PaymentMethod.aPBatchExportSYMappingID>(row);
			}
		}

		protected virtual void PaymentMethod_APPrintChecks_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PaymentMethod row = (PaymentMethod)e.Row;
			if ((bool)row.APPrintChecks)
			{
				row.APCreateBatchPayment = false;
				row.APCheckReportID = null;
			}
			else
			{
				sender.SetDefaultExt<PaymentMethod.aPCreateBatchPayment>(row);
			}
		}

		public override int ExecuteInsert(string viewName, IDictionary values, params object[] parameters)
		{
			switch (viewName)
			{
				case "DetailsForCashAccount":
					values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForCashAccount;
					break;
				case "DetailsForVendor":
					values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForVendor;
					break;
				case "DetailsForReceivable":
					values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForARCards;
					break;
			}
			return base.ExecuteInsert(viewName, values, parameters);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			string value = (String)values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()];
			if (string.IsNullOrEmpty(value) || value == PaymentMethodDetailUsage.UseForAll)
			{
				switch (viewName)
				{
					case "DetailsForCashAccount":
						keys[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForCashAccount;
						values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForCashAccount;
						break;
					case "DetailsForVendor":
						keys[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForVendor;
						values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForVendor;
						break;
					case "DetailsForReceivable":
						keys[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForARCards;
						values[CS.PXDataUtils.FieldName<PaymentMethodDetail.useFor>()] = PaymentMethodDetailUsage.UseForARCards;
						break;
				}
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		#endregion
		#region Detail Events
		protected virtual void PaymentMethodDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			bool enableID = false;
			PaymentMethodDetail row = e.Row as PaymentMethodDetail;
			if (row == null ||(row!=null && string.IsNullOrEmpty(row.DetailID))) enableID = true;
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.detailID>(cache, e.Row, enableID);

			bool isID = (row!= null) && (row.IsIdentifier ?? false);
			PXUIFieldAttribute.SetEnabled<PaymentMethodDetail.displayMask>(cache, e.Row, isID);
		}

		protected virtual void PaymentMethodDetail_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (errorKey)
			{
				errorKey = false;
				e.Cancel = true;
			}
			else
			{
				PaymentMethodDetail row = (PaymentMethodDetail)e.Row;
				string detID = row.DetailID;
				string UseFor = row.UseFor;

				bool isExist = false;
				foreach (PaymentMethodDetail it in this.Details.Select())
				{
					if ((it.DetailID == detID) && (UseFor == it.UseFor))
					{
						isExist = true;
					}
				}

				if (isExist)
				{
					cache.RaiseExceptionHandling<PaymentMethodDetail.detailID>(e.Row, detID, new PXException(Messages.DuplicatedPaymentMethodDetail));
					e.Cancel = true;
				}
			}
		}

		protected virtual void PaymentMethodDetail_DetailID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			PaymentMethodDetail a = e.Row as PaymentMethodDetail;
			if (a.DetailID != null)
			{
				errorKey = true;
			}
		}
		#endregion

		#region Account Events

		protected virtual void PaymentMethodAccount_PaymentMethodID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PaymentMethodAccount_CashAccountID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			int? newCashAccountID = e.NewValue as int?;

			if (newCashAccountID == null)
				return;

			CashAccount cashAccount = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, newCashAccountID);

			if (cashAccount != null && cashAccount.Active != true)
			{
				string errorMsg = string.Format(CA.Messages.CashAccountInactive, cashAccount.CashAccountCD);
				cache.RaiseExceptionHandling<PaymentMethodAccount.cashAccountID>(e.Row, cashAccount.CashAccountCD, new PXSetPropertyException(errorMsg, PXErrorLevel.Error));
			}
		}

		protected virtual void PaymentMethodAccount_CashAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			cache.SetDefaultExt<PaymentMethodAccount.useForAP>(row);
		}

		protected virtual void PaymentMethodAccount_UseForAP_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			CA.PaymentMethod pm = this.PaymentMethod.Current;

			if (row != null && pm != null)
			{
				e.NewValue = (pm.UseForAP == true);

				if (pm.UseForAP == true && row.CashAccountID.HasValue)
				{
					CashAccount c = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
					e.NewValue = (c != null);
				}

				e.Cancel = true;
			}
		}

		protected virtual void PaymentMethodAccount_UseForAR_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			CA.PaymentMethod pm = this.PaymentMethod.Current;
			e.NewValue = (pm != null) && pm.UseForAR == true;
			e.Cancel = true;
		}

		protected virtual void PaymentMethodAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount) e.Row;

			if (row == null)
				return;

			if (string.IsNullOrEmpty(row.PaymentMethodID) == false)
			{
				PaymentMethod pt = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
				bool enabled = (pt != null) && pt.APCreateBatchPayment == true;
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPBatchLastRefNbr>(cache, row, enabled);
			}

			bool isCashAccountActive = true;
			if (row.CashAccountID.HasValue)
			{
				CashAccount c = PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
				PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAP>(cache, row, (c != null));
				isCashAccountActive = c.Active ?? true;
			}

			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAP>(cache, e.Row, isCashAccountActive);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.useForAR>(cache, e.Row, isCashAccountActive);

			bool enableAP = row.UseForAP & isCashAccountActive ?? false;
			bool enableAR = row.UseForAR & isCashAccountActive ?? false;

			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPIsDefault>(cache, e.Row, enableAP);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPAutoNextNbr>(cache, e.Row, enableAP);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPLastRefNbr>(cache, e.Row, enableAP);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRIsDefault>(cache, e.Row, enableAR);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRIsDefaultForRefund>(cache, e.Row, enableAR);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRAutoNextNbr>(cache, e.Row, enableAR);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aRLastRefNbr>(cache, e.Row, enableAR);
		}

		protected virtual void PaymentMethodAccount_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.NewRow;
			if (row != null)
			{
				PaymentMethodAccount oldrow = (PaymentMethodAccount)e.Row;
				if ((row.UseForAP != oldrow.UseForAP) && !row.UseForAP.GetValueOrDefault(false))
				{
					row.APIsDefault = false;
				}

				if ((row.UseForAR != oldrow.UseForAR) && !row.UseForAR.GetValueOrDefault(false))
				{
					row.ARIsDefault = false;
				}
			}

		}

		protected virtual void PaymentMethodAccount_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;
			PXEntryStatus status = cache.GetStatus(e.Row);

			if (row.CashAccountID != null && status != PXEntryStatus.Inserted && status != PXEntryStatus.InsertedDeleted)
			{

				CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>,
														And<CustomerPaymentMethod.cashAccountID, Equal<Required<CustomerPaymentMethod.cashAccountID>>>>>.SelectWindowed(this, 0, 1, row.PaymentMethodID, row.CashAccountID);
				if (cpm != null)
				{
					throw new PXException(Messages.PaymentMethodAccountIsInUseAndCantBeDeleted);
				}

				CashAccount cashAccount = CashAccount.PK.Find(this, row.CashAccountID);

				VerifyCashAccountLinkOrMethodCanBeDeleted(cashAccount);
			}
		}

		protected virtual void PaymentMethodAccount_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;

			if (row == null)
				return;

			if (!string.IsNullOrEmpty(row.PaymentMethodID) && row.CashAccountID.HasValue)
			{
				foreach (PXResult<PaymentMethodAccount, CashAccount> iRes in this.CashAccounts.Select())
				{
					PaymentMethodAccount paymentMethodAccount = iRes;

					if (!object.ReferenceEquals(row, paymentMethodAccount) && paymentMethodAccount.PaymentMethodID == row.PaymentMethodID &&
						row.CashAccountID == paymentMethodAccount.CashAccountID)
					{
						CashAccount cashAccount = iRes;
						throw new PXSetPropertyException(Messages.DuplicatedCashAccountForPaymentMethod, cashAccount.CashAccountCD);
					}
				}
			}
		}

		protected virtual void PaymentMethodAccount_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PaymentMethodAccount row = (PaymentMethodAccount)e.Row;

			PXDefaultAttribute.SetPersistingCheck<PaymentMethodAccount.aPLastRefNbr>(sender, e.Row, row.APAutoNextNbr == true ? PXPersistingCheck.NullOrBlank
																															  : PXPersistingCheck.Nothing);
			if (row.APAutoNextNbr == true && row.APLastRefNbr == null)
			{
				sender.RaiseExceptionHandling<PaymentMethodAccount.aPAutoNextNbr>(row, row.APAutoNextNbr, new PXSetPropertyException(Messages.SpecifyLastRefNbr, GL.Messages.ModuleAP));
			}

			if (row.ARAutoNextNbr == true && row.ARLastRefNbr == null)
			{
				sender.RaiseExceptionHandling<PaymentMethodAccount.aRAutoNextNbr>(row, row.ARAutoNextNbr, new PXSetPropertyException(Messages.SpecifyLastRefNbr, GL.Messages.ModuleAR));
			}

			CashAccount cashAccount = PXSelectReadonly<CashAccount,
												 Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);

			if (cashAccount != null && cashAccount.Active != true)
			{
                if(e.Operation == PXDBOperation.Update)
                {
                    PaymentMethodAccount origPaymentAccount = PXSelectReadonly<PaymentMethodAccount,
                        Where<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
                            And<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>>>>.Select(this, row.PaymentMethodID, row.CashAccountID);

                    if (origPaymentAccount?.CashAccountID == row.CashAccountID)
                        return;
                }

				string errorMsg = string.Format(CA.Messages.CashAccountInactive, cashAccount.CashAccountCD.Trim());
				sender.RaiseExceptionHandling<PaymentMethodAccount.cashAccountID>(e.Row, cashAccount.CashAccountCD, new PXSetPropertyException(errorMsg, PXErrorLevel.Error));
			}
		}
		#endregion

		#region ProcessingCenter Events
		protected virtual void CCProcessingCenterPmntMethod_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (errorKey)
			{
				errorKey = false;
				e.Cancel = true;
			}
			else
			{
				CCProcessingCenterPmntMethod row = e.Row as CCProcessingCenterPmntMethod;
				string detID = row.ProcessingCenterID;
				bool isExist = false;

				foreach (CCProcessingCenterPmntMethod it in this.ProcessingCenters.Select())
				{
					if (!Object.ReferenceEquals(it, row) && it.ProcessingCenterID == row.ProcessingCenterID)
					{
						isExist = true;
					}
				}

				if (isExist)
				{
					cache.RaiseExceptionHandling<CCProcessingCenterPmntMethod.processingCenterID>(e.Row, detID, new PXException(Messages.ProcessingCenterIsAlreadyAssignedToTheCard));
					e.Cancel = true;
				}
				else
				{
					CCProcessingCenter procCenter = GetProcessingCenterById(row.ProcessingCenterID);
					bool supported = CCProcessingFeatureHelper.IsFeatureSupported(procCenter, CCProcessingFeature.PaymentHostedForm);

					if (supported)
					{
						if (row.IsDefault == false)
						{
							WebDialogResult result = ProcessingCenters.Ask(Messages.DefaultProcessingCenterConfirmation, MessageButtons.YesNo);
							if (result == WebDialogResult.Yes)
							{
								row.IsDefault = true;
							}
						}
					}
				}
			}
		}

		protected virtual void CCProcessingCenterPmntMethod_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Delete)
				return;
			CCProcessingCenterPmntMethod row = (CCProcessingCenterPmntMethod)e.Row;
			CCProcessingCenter processingCenter = GetProcessingCenterById(row.ProcessingCenterID);
			if (processingCenter != null)
			{
				if (CCProcessingFeatureHelper.IsFeatureSupported(processingCenter, CCProcessingFeature.ProfileManagement))
				{
					PaymentMethodDetail ccpid = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<PaymentMethod.paymentMethodID>>,
						And<PaymentMethodDetail.isCCProcessingID, Equal<True>>>>.Select(this);
					if (ccpid == null)
					{
						throw new PXException(Messages.CCPaymentProfileIDNotSetUp);
					}
				}
			}
		}

		protected virtual void CCProcessingCenterPmntMethod_IsDefault_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (!this.ProcessingCenters.Any())
			{
				e.NewValue = true;
			}
		}

		#endregion

		#region Internal Auxillary Functions
		protected virtual void fillCreditCardDefaults()
		{
			this.addDefaultsToDetails(CreditCardAttributes.AttributeName.CCPID, Messages.CCPID);
			PaymentMethodDetail det = this.addDefaultsToDetails(CreditCardAttributes.AttributeName.CardNumber, Messages.CardNumber);
			det.DisplayMask = CreditCardAttributes.MaskDefaults.DefaultIdentifier;
			this.addDefaultsToDetails(CreditCardAttributes.AttributeName.ExpirationDate, Messages.ExpirationDate);
			this.addDefaultsToDetails(CreditCardAttributes.AttributeName.NameOnCard, Messages.NameOnCard);
			this.addDefaultsToDetails(CreditCardAttributes.AttributeName.CCVCode, Messages.CCVCode);
		}

		private PaymentMethodDetail addDefaultsToDetails(CreditCardAttributes.AttributeName aAttr, string aDescr)
		{
			PaymentMethodDetail det = new PaymentMethodDetail();
			ImportDefaults(det, aAttr);
			det.Descr = aDescr;
			det.UseFor = PaymentMethodDetailUsage.UseForARCards;
			det = (PaymentMethodDetail)this.Details.Cache.Insert(det);
			if (PXDBLocalizableStringAttribute.IsEnabled)
			{
				PXDBLocalizableStringAttribute.DefaultTranslationsFromMessage(this.Details.Cache, det, "Descr", aDescr);
			}
			return det;
		}

		private static void ImportDefaults(PaymentMethodDetail aPaymentMethodDetail, CreditCardAttributes.AttributeName aAttr)
		{
			aPaymentMethodDetail.DetailID = CreditCardAttributes.GetID(aAttr);
			aPaymentMethodDetail.EntryMask = CreditCardAttributes.GetMask(aAttr);
			aPaymentMethodDetail.ValidRegexp = CreditCardAttributes.GetValidationRegexp(aAttr);
			aPaymentMethodDetail.IsIdentifier = aAttr == CreditCardAttributes.AttributeName.CardNumber;
			aPaymentMethodDetail.IsExpirationDate = aAttr == CreditCardAttributes.AttributeName.ExpirationDate;
			aPaymentMethodDetail.IsOwnerName = (aAttr == CreditCardAttributes.AttributeName.NameOnCard);
			aPaymentMethodDetail.IsCVV = (aAttr == CreditCardAttributes.AttributeName.CCVCode);
			aPaymentMethodDetail.IsRequired = (aAttr == CreditCardAttributes.AttributeName.CCPID);
			aPaymentMethodDetail.IsEncrypted = (aAttr == CreditCardAttributes.AttributeName.ExpirationDate) 
			                                   || (aAttr == CreditCardAttributes.AttributeName.CardNumber) 
			                                   || (aAttr == CreditCardAttributes.AttributeName.CCVCode);
			aPaymentMethodDetail.IsCCProcessingID = (aAttr == CreditCardAttributes.AttributeName.CCPID);
			aPaymentMethodDetail.OrderIndex = (short)((int)aAttr + 1);
		}

		private CCProcessingCenter GetProcessingCenterById(string id)
		{
			CCProcessingCenter procCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(this, id);
			return procCenter;
		}

		public virtual void VerifyAPRequirePaymentRefAndAPAdditionalProcessing(bool? apRequirePaymentRef, string apAdditionalProcessing)
		{
			if (apRequirePaymentRef == true ||
			    apAdditionalProcessing != CA.PaymentMethod.aPAdditionalProcessing.NotRequired)
			{
				foreach (PXResult<PaymentMethodAccount, CashAccount> row in CashAccounts.Select())
				{
					CashAccount account = row;

					if (account.UseForCorpCard == true)
					{
						throw new PXSetPropertyException(Messages.PaymentAndAdditionalProcessingSettingsHaveWrongValuesPaymentSide);
					}
				}
			}
		}

		public virtual void VerifyCashAccountLinkOrMethodCanBeDeleted(CashAccount cashAccount)
		{
			if (cashAccount.UseForCorpCard == true)
			{
				throw new PXException(Messages.CashAccountLinkOrMethodCannotBeDeleted);
			}
		}

		#endregion
		#region Private members
		private bool errorKey;
		#endregion
	}

	public static class CreditCardAttributes
	{
		public enum AttributeName
		{
			CardNumber = 0,
			ExpirationDate,
			NameOnCard,
			CCVCode,
			CCPID
		}

		public static string GetID(AttributeName aID)
		{
			return IDS[(int)aID];
		}

		public static string GetMask(AttributeName aID)
		{
			return EntryMasks[(int)aID];
		}

		public static string GetValidationRegexp(AttributeName aID)
		{
			return ValidationRegexps[(int)aID];
		}

		public const string CardNumber = "CCDNUM";
		public const string ExpirationDate = "EXPDATE";
		public const string NameOnCard = "NAMEONCC";
		public const string CVV = "CVV";
		public const string CCPID = "CCPID";

		public static class MaskDefaults
		{
			public const string CardNumber = "0000-0000-0000-0000";
			public const string ExpirationDate = "00/0000";
			public const string DefaultIdentifier = "****-****-****-0000";
			public const string CVV = "000";
			public const string CCPID = "";
		}

		public static class ValidationRegexp
		{
			public const string CardNumber = "";
			public const string ExpirationDate = "";
			public const string DefaultIdentifier = "";
			public const string CVV = "";
			public const string CCPID = "";
		}

		#region Private Members
		private static string[] IDS = { CardNumber, ExpirationDate, NameOnCard, CVV, CCPID };
		private static string[] EntryMasks = { MaskDefaults.CardNumber, MaskDefaults.ExpirationDate, String.Empty, MaskDefaults.CVV, MaskDefaults.CCPID };
		private static string[] ValidationRegexps = { ValidationRegexp.CardNumber, ValidationRegexp.ExpirationDate, String.Empty, ValidationRegexp.CVV, ValidationRegexp.CCPID };

		#endregion
	}

}


