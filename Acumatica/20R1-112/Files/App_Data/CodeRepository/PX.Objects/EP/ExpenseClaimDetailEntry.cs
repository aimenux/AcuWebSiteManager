using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.Common.Extensions;
using PX.Objects.EP.DAC;
using static PX.Objects.Common.UIState;



namespace PX.Objects.EP
{
	public class ExpenseClaimDetailEntry : PXGraph<ExpenseClaimDetailEntry, EPExpenseClaimDetails>
	{
		#region Extensions

		public class ExpenseClaimDetailEntryExt : ExpenseClaimDetailEntryExt<ExpenseClaimDetailEntry>
		{
			public override PXSelectBase<EPExpenseClaimDetails> Receipts => Base.ClaimDetails;

			public override PXSelectBase<EPExpenseClaim> Claim => Base.CurrentClaim;

			public override PXSelectBase<CurrencyInfo> CurrencyInfo => Base.currencyinfo;
		}

		#endregion

		#region Select
		[PXViewName(Messages.ExpenseReceipt)]
		[PXCopyPasteHiddenFields(typeof(EPExpenseClaimDetails.refNbr))]
		public PXSelectJoin<EPExpenseClaimDetails,
						LeftJoin<EPExpenseClaim,
							On<EPExpenseClaim.refNbr, Equal<EPExpenseClaimDetails.refNbr>>,
						LeftJoin<EPEmployee,
							On<EPEmployee.bAccountID, Equal<EPExpenseClaimDetails.employeeID>>>>,
						Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
							Or<EPExpenseClaimDetails.createdByID, Equal<Current<AccessInfo.userID>>,
							Or<EPExpenseClaimDetails.employeeID, WingmanUser<Current<AccessInfo.userID>>,
							Or<EPEmployee.userID, TM.OwnedUser<Current<AccessInfo.userID>>,
							Or<EPExpenseClaimDetails.noteID, Approver<Current<AccessInfo.userID>>,
							Or<EPExpenseClaim.noteID, Approver<Current<AccessInfo.userID>>>>>>>>,
						OrderBy<Desc<EPExpenseClaimDetails.claimDetailID>>> ClaimDetails;

		[PXCopyPasteHiddenFields(typeof(EPExpenseClaimDetails.refNbr))]
		public PXSelect<EPExpenseClaimDetails, Where<EPExpenseClaimDetails.claimDetailID, Equal<Current<EPExpenseClaimDetails.claimDetailID>>>> CurrentClaimDetails;

		[PXCopyPasteHiddenView]
		public PXSelect<EPExpenseClaim, Where<EPExpenseClaim.refNbr, Equal<Optional2<EPExpenseClaimDetails.refNbr>>>> CurrentClaim;
		[PXCopyPasteHiddenView]
		public PXSelect<CurrencyInfo> currencyinfo;
		[PXCopyPasteHiddenView]
		public PXSetup<EPSetup> epsetup;
		[PXCopyPasteHiddenView]
		public PXSetup<GL.GLSetup> glsetup;
		[PXCopyPasteHiddenView]
		public PXSelect<Currency> currency;
		[PXCopyPasteHiddenView]
		public PXSelect<CurrencyList, Where<CurrencyList.isActive, Equal<True>>> currencyList;
		[PXCopyPasteHiddenView]
		public PXSetup<GL.Company> comapny;
		[PXViewName(CR.Messages.Employee)]
		[PXCopyPasteHiddenView]
		public PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaimDetails.employeeID>>>> Employee;

		[CRReference(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaimDetails.employeeID>>>>))]
		[CRDefaultMailTo(typeof(Select<Contact, Where<Contact.contactID, Equal<Current<EPEmployee.defContactID>>>>))]
		public CRActivityList<EPExpenseClaimDetails>
			Activity;
		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Approval)]
		public EPApprovalAutomation<EPExpenseClaimDetails, EPExpenseClaimDetails.approved, EPExpenseClaimDetails.rejected, EPExpenseClaimDetails.hold, EPSetup> Approval;
		[PXCopyPasteHiddenView]
		public PXSelect<Contract, Where<Contract.contractID, Equal<Current<EPExpenseClaimDetails.contractID>>>> CurrentContract;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<EPTax,
							InnerJoin<Tax, On<Tax.taxID, Equal<EPTax.taxID>>>,
							Where<EPTax.claimDetailID, Equal<Current<EPExpenseClaimDetails.claimDetailID>>>> TaxRows;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<EPTaxTran,
					   InnerJoin<Tax, On<Tax.taxID, Equal<EPTaxTran.taxID>>>,
					   Where<EPTaxTran.claimDetailID, Equal<Current<EPExpenseClaimDetails.claimDetailID>>>> Taxes;

		// We should use read only view here
		// to prevent cache merge because it
		// used only as a shared BQL query.
		// 
		[PXCopyPasteHiddenView]
		public PXSelectReadonly2<EPTaxTran,
						InnerJoin<Tax, On<Tax.taxID, Equal<EPTaxTran.taxID>>>,
							Where<EPTaxTran.claimDetailID, Equal<Current<EPExpenseClaimDetails.claimDetailID>>,
							And<Tax.taxType, Equal<CSTaxType.use>>>> UseTaxes;
		[PXCopyPasteHiddenView]
		public PXSelect<EPTaxAggregate,
					   Where<EPTaxAggregate.refNbr, Equal<Current<EPExpenseClaimDetails.refNbr>>>> TaxAggregate;

		public PXFilter<TaxZoneUpdateAsk> TaxZoneUpdateAskView;

		#endregion

		public ExpenseClaimDetailEntryExt ReceiptEntryExt => FindImplementation<ExpenseClaimDetailEntryExt>();

		#region Action

		public PXAction<EPExpenseClaimDetails> action;
		[PXUIField(DisplayName = Messages.Actions, MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<EPExpenseClaimDetails> Submit;
        [PXUIField(DisplayName = Messages.Submit, MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable submit(PXAdapter adapter)
        {
            return adapter.Get();
        }

		public PXAction<EPExpenseClaimDetails> Claim;
		[PXUIField(DisplayName = Messages.Claim, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable claim(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, delegate ()
			{
				ExpenseClaimDetailMaint.ClaimSingleDetail(CurrentClaimDetails.Current, this.IsContractBasedAPI);
			});
			
			return adapter.Get();
		}

		public PXAction<EPExpenseClaimDetails> SaveTaxZone;
		[PXUIField(DisplayName = "Yes")]
		[PXButton]
		protected virtual IEnumerable saveTaxZone(PXAdapter adapter)
		{
			Employee.Cache.SetValue<EPEmployee.receiptAndClaimTaxZoneID>(Employee.Current, ClaimDetails.Current.TaxZoneID);
			Employee.Update(Employee.Current);
			return adapter.Get();
		}

		public ToggleCurrency<EPExpenseClaimDetails> CurrencyView;



		#endregion

		public ExpenseClaimDetailEntry()
		{
			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });
			PXUIFieldAttribute.SetVisible<EPExpenseClaimDetails.contractID>(ClaimDetails.Cache, null, PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>() || PXAccess.FeatureInstalled<CS.FeaturesSet.projectModule>());
			action.AddMenuAction(Claim);
		}

		public override void InitCacheMapping(Dictionary<Type, Type> map)
		{
			base.InitCacheMapping(map);

			Caches.AddCacheMappingsWithInheritance(this, typeof(CT.Contract));
		}

		public override void Clear(PXClearOption option)
		{
			CurrentClaim.Cache.ClearQueryCache();

			base.Clear(option);
		}

		#region Events
		#region ExpenseClaimDetails

		public virtual void _(Events.RowDeleting<EPExpenseClaimDetails> e)
		{
			if (e.Row.BankTranDate != null)
			{
				throw new PXException(Messages.ExpenseReceiptCannotBeDeletedBecauseItIsMatchedToBankStatement, e.Row.BankTranDate);
			}
		}

		public virtual void _(Events.RowPersisted<EPExpenseClaimDetails> e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				FindImplementation<ExpenseClaimDetailEntryExt>().SetClaimCuryWhenNotInClaim(e.Row, e.Row.RefNbr, e.Row.CorpCardID);

				if (e.Row.ClaimCuryInfoID == e.Row.CardCuryInfoID)
				{
					PXUpdate< 
						Set<EPExpenseClaimDetails.claimCuryInfoID, Required<EPExpenseClaimDetails.claimCuryInfoID>>,
						EPExpenseClaimDetails,
						Where<EPExpenseClaimDetails.claimDetailCD, Equal<Required<EPExpenseClaimDetails.claimDetailCD>>>>
						.Update(this, e.Row.ClaimCuryInfoID, e.Row.ClaimDetailCD);
				}
			}
		}

		public virtual void _(Events.FieldVerifying<EPExpenseClaimDetails.corpCardID> e)
		{
			EPExpenseClaimDetails receipt = (EPExpenseClaimDetails)e.Row;

			int? newCorpCardID = (int?)e.NewValue;

			string refNbr = (string)BqlHelper.GetValuePendingOrRow<EPExpenseClaimDetails.refNbr>(e.Cache, receipt);

			ReceiptEntryExt.VerifyClaimAndCorpCardCurrencies(
				newCorpCardID,
				ReceiptEntryExt.GetParentClaim(refNbr),
				() => e.NewValue = VerifyingHelper.GetNewValueByIncoming(
					e.Cache,
					receipt,
					typeof(EPExpenseClaimDetails.corpCardID).Name,
					e.ExternalCall));
		}

		public virtual void _(Events.FieldVerifying<EPExpenseClaimDetails.paidWith> e)
		{
			EPExpenseClaimDetails receipt = (EPExpenseClaimDetails)e.Row;
			string newPaidWith = (string)e.NewValue;


			decimal? amount = (decimal?)BqlHelper.GetValuePendingOrRow<EPExpenseClaimDetails.curyExtCost>(e.Cache, receipt);

			ReceiptEntryExt.VerifyIsPositiveForCorpCardReceipt(newPaidWith, amount);


			decimal? curyEmployeePart = (decimal?)BqlHelper.GetValuePendingOrRow<EPExpenseClaimDetails.curyEmployeePart>(e.Cache, receipt);

			ReceiptEntryExt.VerifyEmployeePartIsZeroForCorpCardReceipt(newPaidWith, curyEmployeePart);
			

			string refNbr = (string)BqlHelper.GetValuePendingOrRow<EPExpenseClaimDetails.refNbr>(e.Cache, receipt);
			
			ReceiptEntryExt.VerifyEmployeeAndClaimCurrenciesForCash(receipt, newPaidWith, ReceiptEntryExt.GetParentClaim(refNbr));
		}

		protected void EPExpenseClaimDetails_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;
			if (row?.RefNbr != null && epsetup?.Current?.AllowMixedTaxSettingInClaims == false)
			{
				e.NewValue = CurrentClaim.SelectSingle(row.RefNbr).TaxZoneID;
			}
		}
		protected void EPExpenseClaimDetails_TaxCalcMode_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;
			if (row?.RefNbr != null && epsetup?.Current?.AllowMixedTaxSettingInClaims == false)
			{
				e.NewValue = CurrentClaim.SelectSingle(row.RefNbr).TaxCalcMode;
			}
		}
		protected void EPExpenseClaimDetails_ExpenseDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;
			if (row == null || e.NewValue != null)
				return;
			e.NewValue = Accessinfo.BusinessDate;
		}

		protected virtual void EPExpenseClaimDetails_ExpenseAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as EPExpenseClaimDetails;
			if (row?.ContractID.HasValue == false)
			{
				sender.SetDefaultExt<EPExpenseClaimDetails.contractID>(row);
			}
		}

		protected virtual void EPExpenseClaimDetails_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>() && epsetup.Current.ClaimDetailsAssignmentMapID != null;
			e.Cancel = true;
		}
		protected virtual void EPExpenseClaimDetails_ExpenseSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ExpenseClaimDetailEntryExt.ExpenseSubID_FieldDefaulting(sender, e, epsetup.Current.ExpenseSubMask);
		}

		protected virtual void EPExpenseClaimDetails_SalesSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ExpenseClaimDetailEntryExt.SalesSubID_FieldDefaulting(sender, e, epsetup.Current.SalesSubMask);
		}


		protected virtual void EPExpenseClaimDetails_ExpenseDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<EPExpenseClaimDetails.expenseDate, EPExpenseClaimDetails.curyInfoID>(cache, e);
			CurrencyInfoAttribute.SetEffectiveDate<EPExpenseClaimDetails.expenseDate, EPExpenseClaimDetails.cardCuryInfoID>(cache, e);
		}

		protected virtual void EPExpenseClaimDetails_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			EPExpenseClaimDetails document = (EPExpenseClaimDetails)e.Row;

			CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<EPExpenseClaimDetails.curyInfoID>(cache, e.Row);

			if (info != null)
			{
				if (document.CorpCardID != null)
				{
					CashAccount corpCardCashAccount = CACorpCardsMaint.GetCardCashAccount(this, document.CorpCardID);

					cache.SetValueExt<EPExpenseClaimDetails.curyID>(document, corpCardCashAccount.CuryID);
				}
				else
				{
					document.CuryID = info.CuryID;
				}
			}
		}
	
		protected virtual void EPExpenseClaimDetails_RefNbr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;

			if (row != null)
			{
				FindImplementation<ExpenseClaimDetailEntryExt>().RefNbrUpdated(cache, CurrentClaim.Select(row.RefNbr), row, (string)e.OldValue);
			}
		}

        protected virtual void EPExpenseClaimDetails_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;

			if (row != null)
			{
				EPExpenseClaim claim = (EPExpenseClaim)PXSelect<EPExpenseClaim, Where<EPExpenseClaim.refNbr, Equal<Required<EPExpenseClaimDetails.refNbr>>>>.SelectSingleBound(this, new object[] { null }, row.RefNbr);

				bool enabledApprovalReceipt = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>() && epsetup.Current.ClaimDetailsAssignmentMapID != null;
                bool legacyClaim = row.LegacyReceipt == true && !String.IsNullOrEmpty(row.RefNbr);
                bool enabledEditReceipt = (row.Hold == true || !enabledApprovalReceipt) && !legacyClaim;
				bool enabledRefNbr = true;
				bool enabledEmployeeAndBranch = enabledEditReceipt && !(cache.AllowUpdate && !string.IsNullOrEmpty(row.RefNbr));
				bool enabledFinancialDetails = (row.Rejected != true) && (row.Released != true);
				bool NonProject = (CurrentContract.SelectSingle()?.ContractCD ?? PMSetup.DefaultNonProjectCode).Trim() == PMSetup.DefaultNonProjectCode;
				bool claimExist = false;
				bool claimReleased = false;
				if (claim != null)
				{
					claimExist = true;
					bool enabledEditClaim = (row.HoldClaim == true);
					enabledEditReceipt = enabledEditReceipt && enabledEditClaim;
					enabledRefNbr = enabledEditClaim;
					enabledEmployeeAndBranch = false;
					enabledFinancialDetails = enabledFinancialDetails && enabledEditClaim;
					claimReleased = claim.Released == true;

				}
                enabledRefNbr = enabledRefNbr && row.LegacyReceipt == false;

                Approval.AllowSelect = enabledApprovalReceipt;
				Delete.SetEnabled(enabledEditReceipt && claim == null);
				PXUIFieldAttribute.SetEnabled(cache, row, enabledEditReceipt);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.claimDetailID>(cache, row, true);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.refNbr>(cache, row, enabledRefNbr);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.employeeID>(cache, row, enabledEmployeeAndBranch);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.branchID>(cache, row, enabledEmployeeAndBranch);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.expenseAccountID>(cache, row, enabledFinancialDetails);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.expenseSubID>(cache, row, enabledFinancialDetails);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesAccountID>(cache, row, enabledFinancialDetails && (row.Billable == true));
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.salesSubID>(cache, row, enabledFinancialDetails && (row.Billable == true));
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.customerID>(cache, row, NonProject && !claimReleased);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.customerLocationID>(cache, row, NonProject && !claimReleased);
				var taxIsEnabled = row.PaidWith != EPExpenseClaimDetails.paidWith.CardPersonalExpense && row.Released != true && row.BankTranDate == null;
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.taxCategoryID>(cache, row, enabledFinancialDetails && taxIsEnabled);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.curyID>(cache, row, enabledEditReceipt && row.BankTranDate == null);

				action.SetEnabled("Submit", cache.GetStatus(row) != PXEntryStatus.Inserted && row.Hold == true);
				Claim.SetEnabled(cache.GetStatus(row) != PXEntryStatus.Inserted && row.Approved == true && claimExist == false);

				if (row.ContractID != null && (bool)row.Billable && row.TaskID != null)
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, row.TaskID);
					if (task != null && !(bool)task.VisibleInAP)
						cache.RaiseExceptionHandling<EPExpenseClaimDetails.taskID>(e.Row, task.TaskCD, new PXSetPropertyException(PM.Messages.TaskInvisibleInModule, task.TaskCD, GL.BatchModule.AP));
				}

				CurrencyInfo info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<EPExpenseClaimDetails.curyInfoID>>>>.SelectSingleBound(this, new object[] { row });
				if (info != null && info.CuryRateTypeID != null && info.CuryEffDate != null && row.ExpenseDate != null && info.CuryEffDate < row.ExpenseDate)
				{
					CurrencyRateType ratetype = (CurrencyRateType)PXSelectorAttribute.Select<CurrencyInfo.curyRateTypeID>(currencyinfo.Cache, info);
					if (ratetype != null && ratetype.RateEffDays > 0 &&
						((TimeSpan)(row.ExpenseDate - info.CuryEffDate)).Days > ratetype.RateEffDays)
					{
						PXRateIsNotDefinedForThisDateException exc = new PXRateIsNotDefinedForThisDateException(info.CuryRateTypeID, info.BaseCuryID, info.CuryID, (DateTime)row.ExpenseDate);
						cache.RaiseExceptionHandling<EPExpenseClaimDetails.expenseDate>(e.Row, ((EPExpenseClaimDetails)e.Row).ExpenseDate, exc);
					}
				}
				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) && info != null && info.CuryRate == null)
					message = CM.Messages.RateNotFound;
				if (string.IsNullOrEmpty(message))
					cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyID>(e.Row, null, null);
				else
					cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyID>(e.Row, null, new PXSetPropertyException(message, PXErrorLevel.Warning));

				bool allowEdit = this.Accessinfo.UserID == row.CreatedByID;

				if (Employee.Current != null)
				{
					if (!allowEdit && this.Accessinfo.UserID == Employee.Current.UserID)
					{
						allowEdit = true;
					}

					if (!allowEdit)
					{
						EPWingman wingMan = PXSelectJoin<EPWingman,
														InnerJoin<EPEmployee, On<EPWingman.wingmanID, Equal<EPEmployee.bAccountID>>>,
														Where<EPWingman.employeeID, Equal<Required<EPWingman.employeeID>>,
														  And<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>>.Select(this, row.EmployeeID, Accessinfo.UserID);
						if (wingMan != null)
						{
							allowEdit = true;
						}
					}
				}

				//Another conditions in automation steps
				if (!allowEdit)
				{
					action.SetEnabled(MsgNotLocalizable.PutOnHold, false);
				}

				ValidateProjectAndProjectTask(row);

				bool taxSettingsEnabled = enabledEditReceipt 
											&& (epsetup.Current.AllowMixedTaxSettingInClaims == true || CurrentClaimDetails.Current.RefNbr == null)
											&& row.PaidWith != EPExpenseClaimDetails.paidWith.CardPersonalExpense 
											&& row.Released != true 
											&& row.BankTranDate == null;

				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.taxZoneID>(cache, row, taxSettingsEnabled && taxIsEnabled);
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.taxCalcMode>(cache, row, taxSettingsEnabled);
				
				PXUIFieldAttribute.SetVisible<EPExpenseClaimDetails.curyTipAmt>(ClaimDetails.Cache, null, epsetup.Current.NonTaxableTipItem.HasValue || (row.CuryTipAmt ?? 0) != 0);
				Taxes.Cache.SetAllEditPermissions(enabledEditReceipt && row.BankTranDate == null);
				if (row.LegacyReceipt == true)
				{
					RaiseOrHideError<EPExpenseClaimDetails.refNbr>(cache, row, legacyClaim && row.Released == false, Messages.LegacyClaim, PXErrorLevel.Warning, row.RefNbr);
					RaiseOrHideError<EPExpenseClaimDetails.claimDetailID>(cache, row, row.LegacyReceipt == true && row.Released == false && !String.IsNullOrEmpty(row.TaxZoneID), Messages.LegacyReceipt, PXErrorLevel.Warning);
				}

				EPEmployee employeeRow = Employee.Select();
                string taxZoneID = employeeRow == null ? null : ExpenseClaimDetailEntryExt.GetTaxZoneID(this, employeeRow);
                bool notMatchtaxZone = String.IsNullOrEmpty(row.TaxZoneID) && !String.IsNullOrEmpty(taxZoneID);
                RaiseOrHideError<EPExpenseClaimDetails.taxZoneID>(cache,
																	row, 
																	notMatchtaxZone 
																		&& row.Released == false 
																		&& row.PaidWith != EPExpenseClaimDetails.paidWith.CardPersonalExpense,
																	Messages.TaxZoneEmpty, PXErrorLevel.Warning);
				if (UseTaxes.Select().Count != 0)
				{
					cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyTaxTotal>(row, row.CuryTaxTotal,
						new PXSetPropertyException(TX.Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
				}
				else
				{
					cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyTaxTotal>(row, row.CuryTaxTotal, null);
				}
				PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.claimDetailCD>(cache, row, true);

				bool IsMultiCury = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();

				PXUIFieldAttribute.SetVisible<EPExpenseClaimDetails.claimCuryTranAmtWithTaxes>(cache, row, row.IsPaidWithCard && IsMultiCury);
				PXUIFieldAttribute.SetVisible<EPExpenseClaimDetails.cardCuryID>(cache, row, row.CorpCardID != null && IsMultiCury);
			}
        }

		protected virtual void EPExpenseClaimDetails_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;
			if (!string.IsNullOrEmpty(row.RefNbr) && e.Operation != PXDBOperation.Delete)
			{
				EPExpenseClaim claim = CurrentClaim.SelectSingle(row.RefNbr);
				if (claim != null &&
					epsetup.Current.AllowMixedTaxSettingInClaims == false)
				{
					if (claim.TaxZoneID != CurrentClaimDetails.Current.TaxZoneID)
					{
						cache.RaiseExceptionHandling<EPExpenseClaimDetails.taxZoneID>(row, 
							row.TaxZoneID,
							new PXSetPropertyException(Messages.TaxZoneNotMatch, row.ClaimDetailID, claim.RefNbr));
					}
					if (claim.TaxCalcMode != row.TaxCalcMode)
					{
						cache.RaiseExceptionHandling<EPExpenseClaimDetails.taxCalcMode>(row, 
							row.TaxCalcMode,
							new PXSetPropertyException(Messages.TaxCalcModeNotMatch, row.ClaimDetailID, claim.RefNbr));
					}
				}
			}
			if (row != null && e.Operation != PXDBOperation.Delete)
			{
				if (row.Hold == false)
				{
					if (!epsetup.Current.NonTaxableTipItem.HasValue && (row.CuryTipAmt ?? 0) != 0)
					{
						cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyTipAmt>(row,
							row.CuryTipAmt,
							new PXSetPropertyException(Messages.TipItemIsNotDefined));
					}
					if (glsetup.Current.RoundingLimit < Math.Abs(row.TaxRoundDiff ?? 0m))
					{
						throw new PXException(AP.Messages.RoundingAmountTooBig, currencyinfo?.Current?.BaseCuryID, row.TaxRoundDiff,
							PXDBQuantityAttribute.Round(this.glsetup.Current.RoundingLimit));
					}
				}
				if (row.CuryTipAmt >0 && row.CuryExtCost <0 || row.CuryTipAmt < 0 && row.CuryExtCost > 0)
				{
					cache.RaiseExceptionHandling<EPExpenseClaimDetails.curyTipAmt>(row,
						row.CuryTipAmt,
						new PXSetPropertyException(Messages.TipSign));
				}
			}
		}


		protected virtual void EPExpenseClaimDetails_EmployeeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as EPExpenseClaimDetails;
			if (!IsCopyPasteContext && ((row.CuryTranAmt ?? 0) == 0))
			{
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<EPExpenseClaim.curyInfoID>(cache, row);

				if (info != null)
				{
					((EPExpenseClaimDetails)e.Row).CuryID = info.CuryID;
				}
			}
			cache.SetDefaultExt<EPExpenseClaimDetails.taxZoneID>(row);
			cache.SetDefaultExt<EPExpenseClaimDetails.branchID>(row);
		}

		protected virtual void _(Events.FieldUpdated<EPExpenseClaimDetails, EPExpenseClaimDetails.inventoryID> e)
		{
			InventoryItem item = PXSelectorAttribute.Select<InventoryItem.inventoryID>(e.Cache, e.Row) as InventoryItem;
			//e.Cache.SetDefaultExt<EPExpenseClaimDetails.curyUnitCost>(e.Row);
			if (item != null && (epsetup.Current.AllowMixedTaxSettingInClaims == true || CurrentClaimDetails.Current.RefNbr == null))
			{
				e.Cache.SetValueExt<EPExpenseClaimDetails.taxCalcMode>(e.Row, item.TaxCalcMode);
			}
		}

		protected virtual void EPExpenseClaimDetails_CuryEmployeePart_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Decimal? newVal = e.NewValue as Decimal?;
			if (newVal < 0)
				throw new PXSetPropertyException(CS.Messages.FieldShouldNotBeNegative, PXUIFieldAttribute.GetDisplayName<EPExpenseClaimDetails.curyEmployeePart>(cache));
		}

		protected virtual void EPExpenseClaimDetails_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;

			if (row?.CustomerID == null)
				cache.SetValueExt<EPExpenseClaimDetails.customerLocationID>(row, null);
		}

        protected virtual void EPExpenseClaimDetails_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;

            if (row == null)
                return;

	        ExpenseClaimDetailEntryExt.DeleteLegacyTaxRows(this, row.RefNbr);
        }


        protected virtual void EPExpenseClaimDetails_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;
			EPExpenseClaimDetails oldRow = e.OldRow as EPExpenseClaimDetails;

			if (row == null || oldRow == null)
				return;

            if (row.RefNbr != oldRow.RefNbr || row.TaxCategoryID != oldRow.TaxCategoryID || row.TaxCalcMode != oldRow.TaxCalcMode || row.TaxZoneID != oldRow.TaxZoneID)
            {
	            ExpenseClaimDetailEntryExt.DeleteLegacyTaxRows(this, row.RefNbr);
            }

            if (e.ExternalCall && !this.IsMobile)
			{
				if (row.TaxZoneID != oldRow.TaxZoneID && !string.IsNullOrEmpty(row.TaxZoneID))
				{
					EPEmployee employee = Employee.Select();

					string taxZoneID = employee.ReceiptAndClaimTaxZoneID;
					if (string.IsNullOrEmpty(taxZoneID))
					{
						Location location = PXSelect<Location,
							Where<Location.locationID, Equal<Required<EPEmployee.defLocationID>>>>.Select(this, employee.DefLocationID);
						taxZoneID = location?.VTaxZoneID;
					}

					if (row.TaxZoneID != taxZoneID)
					{
						Employee.Current = employee;
						TaxZoneUpdateAskView.View.AskExt();
					}
				}
			}
		}

        #endregion
        #region EPTaxTran
		protected virtual void EPTaxTran_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			EPTaxTran row = (EPTaxTran)e.Row;
			EPExpenseClaimDetails doc = ClaimDetails.Current;
			if (row != null && doc!=null&& e.Operation != PXDBOperation.Delete)
			{
				if (row.CuryTaxAmt > 0 && doc.CuryExtCost < 0 || row.CuryTaxAmt < 0 && doc.CuryExtCost > 0)
				{
					cache.RaiseExceptionHandling<EPTaxTran.curyTaxAmt>(row,
						row.CuryTaxAmt,
						new PXSetPropertyException(Messages.TaxSign));
				}
				if (row.CuryTaxableAmt > 0 && doc.CuryExtCost < 0 || row.CuryTaxableAmt < 0 && doc.CuryExtCost > 0)
				{
					cache.RaiseExceptionHandling<EPTaxTran.curyTaxableAmt>(row,
						row.CuryTaxableAmt,
						new PXSetPropertyException(Messages.TaxableSign));
				}
			}
		}
		#endregion
		#region EPExpenseClaim

		#endregion
		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			EPEmployee employee = (EPEmployee)PXSelect<EPEmployee>.Search<EPEmployee.bAccountID>(this, ClaimDetails.Current != null ? ClaimDetails.Current.EmployeeID : null);
			if (employee != null && employee.CuryID != null)
			{
				e.NewValue = employee.CuryID;
				e.Cancel = true;
			}
			else if (comapny.Current != null)
			{
				e.NewValue = comapny.Current.BaseCuryID;
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (ClaimDetails.Current != null && ClaimDetails.Current.EmployeeID != null)
			{
				EPEmployee employee = (EPEmployee)PXSelect<EPEmployee>.Search<EPEmployee.bAccountID>(this, ClaimDetails.Current != null ? ClaimDetails.Current.EmployeeID : null);
				if (employee != null && employee.CuryRateTypeID != null)
				{
					e.NewValue = employee.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (ClaimDetails.Current != null)
			{
				e.NewValue = ClaimDetails.Current.ExpenseDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null && ClaimDetails.Current != null)
			{
				bool rateenabled = info.AllowUpdate(ClaimDetails.Cache) && ClaimDetails.Current.EmployeeID != null;
				if (rateenabled)
				{
					CurrencyList curyList = (CurrencyList)PXSelectorAttribute.Select<CurrencyInfo.curyID>(cache, info);
					if (curyList != null && curyList.IsFinancial == true)
					{
						EPEmployee employee = (EPEmployee)PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaimDetails.employeeID>>>>.SelectSingleBound(this, new object[] { ClaimDetails.Current });
						rateenabled = employee != null && employee.AllowOverrideRate == true;
					}
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(cache, info, rateenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(cache, info, rateenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(cache, info, rateenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(cache, info, rateenabled);
			}
		}


		public virtual void ValidateProjectAndProjectTask(EPExpenseClaimDetails info)
		{

			if (info != null)
			{
				string errProjectMsg = PXUIFieldAttribute.GetError<EPExpenseClaimDetails.contractID>(ClaimDetails.Cache, info);
				if (!string.IsNullOrEmpty(errProjectMsg) && errProjectMsg.Equals(PXLocalizer.Localize(PM.Messages.ProjectExpired)))
				{
					PXUIFieldAttribute.SetError<EPExpenseClaimDetails.contractID>(ClaimDetails.Cache, info, null);
				}

				if (info.ContractID != null)
				{
					PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<EPExpenseClaimDetails.contractID>>>>.SelectWindowed(this, 0, 1, info.ContractID);
					if (project != null && project.ExpireDate != null && info.ExpenseDate != null)
					{
						if (info.ExpenseDate > project.ExpireDate)
						{
							ClaimDetails.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.contractID>(
									info, info.ContractID,
									new PXSetPropertyException(
									PM.Messages.ProjectExpired,
									PXErrorLevel.Warning));
						}
					}
				}

				string errProjTaskMsg = PXUIFieldAttribute.GetError<EPExpenseClaimDetails.taskID>(ClaimDetails.Cache, info);
				if (!string.IsNullOrEmpty(errProjTaskMsg) && (errProjTaskMsg.Equals(PXLocalizer.Localize(PM.Messages.ProjectTaskExpired))
															|| errProjTaskMsg.Equals(PXLocalizer.Localize(PM.Messages.TaskIsCompleted))))
				{
					PXUIFieldAttribute.SetError<EPExpenseClaimDetails.taskID>(ClaimDetails.Cache, info, null);
				}

				if (info.TaskID != null)
				{
					PMTask projectTask = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<EPExpenseClaimDetails.taskID>>>>.SelectWindowed(this, 0, 1, info.TaskID);
					if (projectTask != null && projectTask.EndDate != null && info.ExpenseDate != null)
					{
						if (info.ExpenseDate > projectTask.EndDate && projectTask.Status != ProjectTaskStatus.Completed)
						{
							ClaimDetails.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.taskID>(
									info, info.TaskID,
									new PXSetPropertyException(
									PM.Messages.ProjectTaskExpired,
									PXErrorLevel.Warning));
						}
						else if (projectTask.Status == ProjectTaskStatus.Completed)
						{
							ClaimDetails.Cache.RaiseExceptionHandling<EPExpenseClaimDetails.taskID>(
									info, info.TaskID,
									new PXSetPropertyException(
									PM.Messages.TaskIsCompleted,
									PXErrorLevel.Warning));
						}
					}
				}
			}
		}
		#endregion
		#endregion
		#region DAC Overrides
		#region TaxZoneUpdateAsk
		[Serializable]
		[PXHidden]
		public partial class TaxZoneUpdateAsk : IBqlTable
		{
		}
		#endregion

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXDefault]
		[PXUIField(DisplayName = "Currency", ErrorHandling = PXErrorHandling.Never)]
		[PXSelector(typeof(CurrencyList.curyID))]
		[CurrencyInfo.CuryID]
		protected virtual void CurrencyInfo_CuryId_CacheAttached(PXCache cache)
		{
		}

		#region EPExpenseClaimDetails
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[EPTax()]
		protected virtual void EPExpenseClaimDetails_TaxCategoryID_CacheAttached(PXCache cache)
		{
		}


		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[GL.Branch(typeof(Search2<
		GL.Branch.branchID,
		InnerJoin<EPEmployee,
			On<GL.Branch.bAccountID, Equal<EPEmployee.parentBAccountID>>>,
		Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaimDetails.employeeID>>>>))]
		protected virtual void EPExpenseClaimDetails_BranchID_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Amount in Card Currency", Enabled = false)]
		protected virtual void _(Events.CacheAttached<EPExpenseClaimDetails.claimCuryTranAmtWithTaxes> e)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
		[PXSubordinateAndWingmenSelector]
		[PXUIField(DisplayName = "Claimed by", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Switch<Case<Where<
			Selector<EPExpenseClaimDetails.refNbr, EPExpenseClaim.employeeID>, IsNotNull,
				And<Current2<EPExpenseClaimDetails.employeeID>, IsNull>>,
			Selector<EPExpenseClaimDetails.refNbr, EPExpenseClaim.employeeID>>, EPExpenseClaimDetails.employeeID>))]
		[PXUIEnabled(typeof(Where<EPExpenseClaimDetails.refNbr, IsNull>))]
		protected virtual void EPExpenseClaimDetails_employeeID_CacheAttached(PXCache cache)
		{
		}

		[CurrencyInfo(ModuleCode = "EP", CuryIDField = "curyID", CuryDisplayName = "Currency")]
		[PXDBLong()]
		protected virtual void EPExpenseClaimDetails_CuryInfoID_CacheAttached(PXCache cache)
		{
		}
		protected virtual void EPExpenseClaimDetails_CuryID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			EPExpenseClaim claim = CurrentClaim.SelectSingle();
			var row = e.Row as EPExpenseClaimDetails;
			if (claim!=null && 
				row != null && 
				claim.CuryInfoID == row.CuryInfoID)
			{
				row.CuryInfoID = null;
				ClaimDetails.Cache.Update(row);
			}
		}
		[PXDBLong()]
		protected virtual void EPExpenseClaimDetails_ClaimCuryInfoID_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Expense Claim", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search2<EPExpenseClaim.refNbr,
                                   LeftJoin<EPTaxAggregate,On<EPTaxAggregate.refNbr, Equal<EPExpenseClaim.refNbr>>>,
								Where2<
                                    Where<
                                        Current<EPExpenseClaimDetails.holdClaim>, Equal<False>,
									    Or<EPExpenseClaim.hold, Equal<True>,
									    And2<Where<EPExpenseClaim.employeeID, Equal<Current2<EPExpenseClaimDetails.employeeID>>,
									    Or<Current2<EPExpenseClaimDetails.employeeID>, IsNull>>,
										    And<Where<Current<EPExpenseClaimDetails.rejected>, Equal<False>>>>>>,
                                    
                                    And2<
                                        Where2<Where<EPExpenseClaim.taxZoneID, Equal<Current2<EPExpenseClaimDetails.taxZoneID>>,
                                            Or<Where<EPExpenseClaim.taxZoneID, IsNull, And<Current2<EPExpenseClaimDetails.taxZoneID>, IsNull>>>>,
                                        And<EPExpenseClaim.taxCalcMode, Equal<Current2<EPExpenseClaimDetails.taxCalcMode>>,
                                        Or<Current<EPSetup.allowMixedTaxSettingInClaims>, Equal<True>>>>,

                                    And<EPTaxAggregate.refNbr, IsNull>>>>),
					new Type[] {typeof(EPExpenseClaim.refNbr),
								typeof(EPExpenseClaim.employeeID),
								typeof(EPExpenseClaim.locationID),
								typeof(EPExpenseClaim.docDate),
								typeof(EPExpenseClaim.docDesc),
								typeof(EPExpenseClaim.curyID),
								typeof(EPExpenseClaim.curyDocBal)},
					DescriptionField = typeof(EPExpenseClaim.docDesc))]
		protected virtual void EPExpenseClaimDetails_RefNbr_CacheAttached(PXCache cache)
		{
		}
		protected virtual void EPExpenseClaimDetails_CuryTipAmt_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as EPExpenseClaimDetails;
			if (row != null)
			{
				if (row.CuryTipAmt != 0)
				{
					var item = (InventoryItem)PXSelect<InventoryItem, 
												Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, epsetup.SelectSingle().NonTaxableTipItem);
					CurrentClaimDetails.SetValueExt<EPExpenseClaimDetails.taxTipCategoryID>(row, item.TaxCategoryID);
				}
				else
				{
					CurrentClaimDetails.SetValueExt<EPExpenseClaimDetails.taxTipCategoryID>(row, null);
				}
			}
		}
		protected virtual void EPExpenseClaimDetails_TaxCategoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPExpenseClaimDetails row = e.Row as EPExpenseClaimDetails;
			if (row!=null)
			{
				row.CuryTaxableAmtFromTax = 0;
				row.TaxableAmtFromTax = 0;
				row.CuryTaxAmt = 0;
				row.TaxAmt = 0;
			}
		}
		#endregion

		#region EPApproval Cahce Attached
		[PXDBDate]
		[PXDefault(typeof(EPExpenseClaimDetails.expenseDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt]
		[PXDefault(typeof(EPExpenseClaimDetails.employeeID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid]
		[PXDefault(typeof(Search<CREmployee.userID,
				Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
					And<Current<EPExpenseClaimDetails.workgroupID>, IsNull>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(EPExpenseClaimDetails.tranDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong]
		[CurrencyInfo(typeof(EPExpenseClaimDetails.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(EPExpenseClaimDetails.curyTranAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(EPExpenseClaimDetails.tranAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region EPSetup Cahce Attached
		[PXInt]
		[PXDBScalar(typeof(Search<EPSetup.claimDetailsAssignmentMapID>))]
		protected virtual void EPSetup_AssignmentMapID_CacheAttached(PXCache cache)
		{
		}

		[PXInt]
		[PXDBScalar(typeof(Search<EPSetup.claimDetailsAssignmentNotificationID>))]
		protected virtual void EPSetup_AssignmentNotificationID_CacheAttached(PXCache cache)
		{
		}

		[PXBool]
		[PXDefault(true)]
		[PXFormula(typeof(IIf<FeatureInstalled<CS.FeaturesSet.approvalWorkflow>, True, False>))]
		protected virtual void EPSetup_IsActive_CacheAttached(PXCache cache)
		{
		}
		#endregion
		#endregion
	}
}