using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Payroll.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.CS;

namespace PX.Objects.PR
{
	public class PRPayChecksAndAdjustments : PXGraph<PRPayChecksAndAdjustments, PRPayment>
	{
		private bool _IsVoidCheckInProgress = false;
		private bool _AllowUpdatePaymentChildrenRecords = true;

		#region Views
		public PXSetup<PRSetup> PRSetup;
		public SelectFrom<PRPayment>
			.Where<PRPayment.docType.IsEqual<PRPayment.docType.AsOptional>>.View Document;

		public PXSelect<PRPayment,
				Where<PRPayment.docType,
					Equal<Optional<PRPayment.docType>>,
				And<PRPayment.refNbr,
					Equal<Current<PRPayment.refNbr>>>>> CurrentDocument;
		public PXSelect<PREarningDetail,
				Where<PREarningDetail.employeeID,
					Equal<Current<PRPayment.employeeID>>,
				And<PREarningDetail.paymentDocType,
					Equal<Current<PRPayment.docType>>,
				And<PREarningDetail.paymentRefNbr,
					Equal<Current<PRPayment.refNbr>>>>>,
				OrderBy<Asc<PREarningDetail.date, Asc<PREarningDetail.isFringeRateEarning, Asc<PREarningDetail.sortingRecordID, Asc<PREarningDetail.rate>>>>>> Earnings;
		public PXSelect<PRPaymentEarning,
				Where<PRPaymentEarning.docType,
					Equal<Current<PRPayment.docType>>,
				And<PRPaymentEarning.refNbr,
					Equal<Current<PRPayment.refNbr>>>>> SummaryEarnings;
		public SelectFrom<PRPaymentDeduct>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentDeduct.codeID>>
			.Where<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View Deductions;
		public PXSelect<PRPaymentTax,
				Where<PRPaymentTax.docType,
					Equal<Current<PRPayment.docType>>,
				And<PRPaymentTax.refNbr,
					Equal<Current<PRPayment.refNbr>>>>> Taxes;
		public SelectFrom<PRPaymentTaxSplit>
			.Where<PRPaymentTaxSplit.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentTaxSplit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PRPaymentTaxSplit.taxID.IsEqual<PRPaymentTax.taxID.FromCurrent>>>
			.OrderBy<PRPaymentTaxSplit.taxID.Asc>.View TaxSplits;
		public SelectFrom<PRPaymentOvertimeRule>.
			InnerJoin<PROvertimeRule>.
				On<PRPaymentOvertimeRule.overtimeRuleID.IsEqual<PROvertimeRule.overtimeRuleID>>.
			Where<PRPaymentOvertimeRule.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
				And<PRPaymentOvertimeRule.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View PaymentOvertimeRules;
		public SelectFrom<PRBatchOvertimeRule>.
			Where<PRBatchOvertimeRule.batchNbr.IsEqual<PRPayment.payBatchNbr.FromCurrent>>.View BatchOvertimeRules;
		public PXFilter<ExistingPayment> ExistingPayment;
		public PXFilter<ExistingPayrollBatch> ExistingPayrollBatch;
		public PXFilter<TaxUpdateHelpers.UpdateTaxesWarning> UpdateTaxesPopupView;

		public SelectFrom<PRYtdEarnings>.
				Where<PRYtdEarnings.employeeID.IsEqual<PRPayment­.employeeID.FromCurrent>
				.And<PRYtdEarnings.year.IsEqual<P.AsString>>>.View EmployeeYTDEarnings;

		public SelectFrom<PRYtdDeductions>.
				Where<PRYtdDeductions.employeeID.IsEqual<PRPayment­.employeeID.FromCurrent>
				.And<PRYtdDeductions.year.IsEqual<P.AsString>>>.View EmployeeYTDDeductions;

		public SelectFrom<PRYtdTaxes>.
				Where<PRYtdTaxes.employeeID.IsEqual<PRPayment­.employeeID.FromCurrent>
				.And<PRYtdTaxes.year.IsEqual<P.AsString>>>.View EmployeeYTDTaxes;

		public SelectFrom<PRDeductionDetail>.
			Where<PRDeductionDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>.
				And<PRDeductionDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
				And<PRDeductionDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>>.
			OrderBy<PRDeductionDetail.codeID.Asc>.View DeductionDetails;

		public SelectFrom<PRBenefitDetail>.
			Where<PRBenefitDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>.
				And<PRBenefitDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
				And<PRBenefitDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>>.
			OrderBy<PRBenefitDetail.codeID.Asc>.View BenefitDetails;

		public SelectFrom<PRTaxDetail>.
			Where<PRTaxDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>.
				And<PRTaxDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
				And<PRTaxDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>>.
			OrderBy<PRTaxDetail.taxID.Asc>.View TaxDetails;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PRPayment.curyInfoID>>>> CurrencyInfo;

		public PTOHelper.PTOBankSelect.View PTOBanks;

		public SelectFrom<PRPaymentPTOBank>
			.Where<PRPaymentPTOBank.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentPTOBank.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View PaymentPTOBanks;

		public PTOHelper.EmployeePTOHistorySelect.View PTOHistory;

		public SelectFrom<PaymentMethodAccount>
			.Where<PaymentMethodAccount.paymentMethodID.IsEqual<PRPayment.paymentMethodID.FromCurrent>
				.And<PaymentMethodAccount.cashAccountID.IsEqual<PRPayment.cashAccountID.FromCurrent>>>.View PaymentMethodAccount;

		public SelectFrom<PRDeductionAndBenefitUnionPackage>
			.InnerJoin<PMUnion>.On<PMUnion.unionID.IsEqual<PRDeductionAndBenefitUnionPackage.unionID>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID>>
			.Where<PRDeductionAndBenefitUnionPackage.unionID.IsEqual<P.AsString>
				.And<PRDeductionAndBenefitUnionPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>
				.And<PMUnion.isActive.IsEqual<True>>
				.And<PRDeductCode.isActive.IsEqual<True>>
				.And<PRDeductionAndBenefitUnionPackage.laborItemID.IsEqual<P.AsInt>
					.Or<PRDeductionAndBenefitUnionPackage.laborItemID.IsNull
						.And<P.AsInt.IsNull
							.Or<P.AsInt.IsNotInSubselect<SearchFor<PRDeductionAndBenefitUnionPackage.laborItemID>
								.Where<PRDeductionAndBenefitUnionPackage.unionID.IsEqual<P.AsString>
									.And<PRDeductionAndBenefitUnionPackage.laborItemID.IsNotNull>
									.And<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.IsEqual<PRDeductCode.codeID>>
									.And<PRDeductionAndBenefitUnionPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>>>>>>>
			.OrderBy<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.Asc, PRDeductionAndBenefitUnionPackage.effectiveDate.Desc>.View EarningUnionDeductions;

		public SelectFrom<PRDeductionAndBenefitProjectPackage>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID>>
			.Where<PRDeductionAndBenefitProjectPackage.projectID.IsEqual<P.AsInt>
				.And<PRDeductionAndBenefitProjectPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>
				.And<PRDeductCode.isActive.IsEqual<True>>
				.And<PRDeductionAndBenefitProjectPackage.laborItemID.IsEqual<P.AsInt>
					.Or<PRDeductionAndBenefitProjectPackage.laborItemID.IsNull
						.And<P.AsInt.IsNull
							.Or<P.AsInt.IsNotInSubselect<SearchFor<PRDeductionAndBenefitProjectPackage.laborItemID>
								.Where<PRDeductionAndBenefitProjectPackage.projectID.IsEqual<P.AsInt>
									.And<PRDeductionAndBenefitProjectPackage.laborItemID.IsNotNull>
									.And<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID.IsEqual<PRDeductCode.codeID>>
									.And<PRDeductionAndBenefitProjectPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>>>>>>>
			.OrderBy<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID.Asc, PRDeductionAndBenefitProjectPackage.effectiveDate.Desc>.View EarningProjectDeductions;

		public SelectFrom<PRDirectDepositSplit>
			.Where<PRDirectDepositSplit.docType.IsEqual<PRPayment.docType.AsOptional>
				.And<PRDirectDepositSplit.refNbr.IsEqual<PRPayment.refNbr.AsOptional>>>.View DirectDepositSplits;

		public SelectFrom<PREmployee>.Where<PREmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View CurrentEmployee;

		public SelectFrom<PRPaymentWCPremium>
			.InnerJoin<PMWorkCode>.On<PMWorkCode.workCodeID.IsEqual<PRPaymentWCPremium.workCodeID>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentWCPremium.deductCodeID>>
			.Where<PRPaymentWCPremium.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentWCPremium.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View WCPremiums;

		public SelectFrom<PaymentMethod>.Where<PaymentMethod.paymentMethodID.IsEqual<PRPayment.paymentMethodID.AsOptional>>.View PaymentMethod;

		public SelectFrom<CABatchDetail>
			.InnerJoin<CABatch>
				.On<CABatch.batchNbr.IsEqual<CABatchDetail.batchNbr>>
			.Where<CABatchDetail.origDocType.IsEqual<PRPayment.docType.FromCurrent>
			.And<CABatchDetail.origRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
			.And<CABatchDetail.origModule.IsEqual<BatchModule.modulePR>>>.View DirectDepositBatchAndDetails;

		public SelectFrom<PRPaymentProjectPackageDeduct>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentProjectPackageDeduct.deductCodeID>>
			.LeftJoin<PRDeductionAndBenefitProjectPackage>.On<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID.IsEqual<PRPaymentProjectPackageDeduct.deductCodeID>
				.And<PRDeductionAndBenefitProjectPackage.projectID.IsEqual<PRPaymentProjectPackageDeduct.projectID>>
				.And<PRDeductionAndBenefitProjectPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>
				.And<PRDeductionAndBenefitProjectPackage.laborItemID.IsEqual<PRPaymentProjectPackageDeduct.laborItemID>
					.Or<PRDeductionAndBenefitProjectPackage.laborItemID.IsNull
						.And<PRPaymentProjectPackageDeduct.laborItemID.IsNull>>>>
			.Where<PRPaymentProjectPackageDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentProjectPackageDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View ProjectPackageDeductions;

		public SelectFrom<PRPaymentUnionPackageDeduct>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentUnionPackageDeduct.deductCodeID>>
			.LeftJoin<PRDeductionAndBenefitUnionPackage>.On<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.IsEqual<PRPaymentUnionPackageDeduct.deductCodeID>
				.And<PRDeductionAndBenefitUnionPackage.unionID.IsEqual<PRPaymentUnionPackageDeduct.unionID>>
				.And<PRDeductionAndBenefitUnionPackage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>
				.And<PRDeductionAndBenefitUnionPackage.laborItemID.IsEqual<PRPaymentUnionPackageDeduct.laborItemID>
					.Or<PRDeductionAndBenefitUnionPackage.laborItemID.IsNull
						.And<PRPaymentUnionPackageDeduct.laborItemID.IsNull>>>>
			.Where<PRPaymentUnionPackageDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentUnionPackageDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View UnionPackageDeductions;

		public SelectFrom<PRPaymentFringeBenefit>
			.Where<PRPaymentFringeBenefit.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeBenefit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>
			.OrderBy<PRPaymentFringeBenefit.projectID.Asc, PRPaymentFringeBenefit.laborItemID.Asc, PRPaymentFringeBenefit.projectTaskID.Asc>.View PaymentFringeBenefits;

		public SelectFrom<PRPaymentFringeBenefitDecreasingRate>
			.Where<PRPaymentFringeBenefitDecreasingRate.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeBenefitDecreasingRate.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PRPaymentFringeBenefitDecreasingRate.projectID.IsEqual<PRPaymentFringeBenefit.projectID.FromCurrent>>
				.And<PRPaymentFringeBenefitDecreasingRate.laborItemID.IsEqual<PRPaymentFringeBenefit.laborItemID.FromCurrent>>
				.And<PRPaymentFringeBenefitDecreasingRate.projectTaskID.IsEqual<PRPaymentFringeBenefit.projectTaskID.FromCurrent>
					.Or<PRPaymentFringeBenefitDecreasingRate.projectTaskID.IsNull
						.And<PRPaymentFringeBenefit.projectTaskID.FromCurrent.IsNull>>>>.View PaymentFringeBenefitsDecreasingRate;

		public SelectFrom<PRPaymentFringeBenefitDecreasingRate>
			.Where<PRPaymentFringeBenefitDecreasingRate.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeBenefitDecreasingRate.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View AllPaymentFringeBenefitsDecreasingRate;

		public SelectFrom<PRPaymentFringeEarningDecreasingRate>
			.Where<PRPaymentFringeEarningDecreasingRate.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeEarningDecreasingRate.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PRPaymentFringeEarningDecreasingRate.projectID.IsEqual<PRPaymentFringeBenefit.projectID.FromCurrent>>
				.And<PRPaymentFringeEarningDecreasingRate.laborItemID.IsEqual<PRPaymentFringeBenefit.laborItemID.FromCurrent>>
				.And<PRPaymentFringeEarningDecreasingRate.projectTaskID.IsEqual<PRPaymentFringeBenefit.projectTaskID.FromCurrent>
					.Or<PRPaymentFringeEarningDecreasingRate.projectTaskID.IsNull
						.And<PRPaymentFringeBenefit.projectTaskID.FromCurrent.IsNull>>>>.View PaymentFringeEarningsDecreasingRate;

		public SelectFrom<PRPaymentFringeEarningDecreasingRate>
			.Where<PRPaymentFringeEarningDecreasingRate.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeEarningDecreasingRate.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View AllPaymentFringeEarningsDecreasingRate;

		public SelectFrom<PRTaxUpdateHistory>.View UpdateHistory;
		#endregion

		private void UpdatePayrollBatch(string payBatchNumber, int? employeeID)
		{
			PRPayBatchEntry payBatchEntryGraph = CreateInstance<PRPayBatchEntry>();
			payBatchEntryGraph.UpdatePayrollBatch(payBatchNumber, employeeID);
		}

		public PRPayChecksAndAdjustments()
		{
			Action.AddMenuAction(Calculate);
			Action.AddMenuAction(PrintCheck);
			Action.AddMenuAction(CancelPrint);
			Action.AddMenuAction(Release);
			Action.AddMenuAction(VoidPayment);

			Deductions.AllowDelete = false;

			DeductionDetails.Cache.AllowInsert = false;
			DeductionDetails.Cache.AllowDelete = false;

			SummaryEarnings.AllowInsert =
			SummaryEarnings.AllowUpdate =
			SummaryEarnings.AllowDelete =

			TaxSplits.AllowInsert =
			TaxSplits.AllowDelete = false;

			PaymentPTOBanks.AllowInsert = false;
			PaymentPTOBanks.AllowDelete = false;

			PaymentOvertimeRules.AllowInsert = false;
			PaymentOvertimeRules.AllowDelete = false;

			DirectDepositSplits.AllowInsert = false;
			DirectDepositSplits.AllowUpdate = false;
			DirectDepositSplits.AllowDelete = false;

			PaymentFringeBenefits.AllowInsert = false;
			PaymentFringeBenefits.AllowUpdate = false;
			PaymentFringeBenefits.AllowDelete = false;
			PaymentFringeBenefitsDecreasingRate.AllowInsert = false;
			PaymentFringeBenefitsDecreasingRate.AllowUpdate = false;
			PaymentFringeBenefitsDecreasingRate.AllowDelete = false;
			PaymentFringeEarningsDecreasingRate.AllowInsert = false;
			PaymentFringeEarningsDecreasingRate.AllowUpdate = false;
			PaymentFringeEarningsDecreasingRate.AllowDelete = false;
		}

		#region Data View Delegates
		protected IEnumerable deductions()
		{
			PXView viewSelect = new PXView(this, true, Deductions.View.BqlSelect);
			HashSet<(int?, string)> paymentDeductIDs = new HashSet<(int?, string)>();
			List<PRPaymentDeduct> queryResults = new List<PRPaymentDeduct>();

			foreach (PRPaymentDeduct record in Deductions.Cache.Cached)
			{
				if (record.CodeID != null &&
					!paymentDeductIDs.Contains((record.CodeID, record.Source)) &&
					(Deductions.Cache.GetStatus(record) == PXEntryStatus.Updated || Deductions.Cache.GetStatus(record) == PXEntryStatus.Inserted))
				{
					paymentDeductIDs.Add((record.CodeID, record.Source));
					queryResults.Add(record);
				}
			}

			foreach (PRPaymentDeduct paymentDeduct in viewSelect.SelectMulti()
				.Select(x => (PXResult<PRPaymentDeduct, PRDeductCode>)x)
				.Where(x => !paymentDeductIDs.Contains((((PRPaymentDeduct)x).CodeID, ((PRPaymentDeduct)x).Source))))
			{
				var record = Deductions.Cache.Locate(paymentDeduct) ?? paymentDeduct;
				if (Deductions.Cache.GetStatus(record) != PXEntryStatus.Deleted)
				{
					paymentDeductIDs.Add((paymentDeduct.CodeID, paymentDeduct.Source));
					queryResults.Add(paymentDeduct);
				}
			}

			IEnumerable<PRDeductCode> deductCodes = SelectFrom<PRDeductCode>.View.Select(this).FirstTableItems;
			IEnumerable<PRDeductCode> activeDeductCodes = deductCodes.Where(x => x.IsActive == true);
			PRPayment currentPayment = CurrentDocument.Current;
			foreach (PRPaymentDeduct paymentDeduct in queryResults)
			{
				bool deductionNotActive = paymentDeduct.CodeID != null && !activeDeductCodes.Any(x => x.CodeID == paymentDeduct.CodeID);
				if (paymentDeduct.Source == PaymentDeductionSourceAttribute.WorkCode)
				{
					var stateWCEarningsQuery = new SelectFrom<PREarningDetail>
						.InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREarningDetail.locationID>>
						.InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>
						.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<P.AsInt>>
						.Where<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>
							.And<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>>
							.And<PREarningDetail.workCodeID.IsNotNull>
							.And<Address.state.IsEqual<PRDeductCode.state>>>.View(this);

					paymentDeduct.IsActive = (stateWCEarningsQuery.Select(paymentDeduct.CodeID).Any_() && !deductionNotActive) ||
						WCPremiums.Select().FirstTableItems.Any(x => x.DeductCodeID == paymentDeduct.CodeID);
					PXUIFieldAttribute.SetEnabled<PRPaymentDeduct.isActive>(Deductions.Cache, paymentDeduct, false);
				}
				else if (currentPayment.Released == false && currentPayment.Printed == false && deductionNotActive)
				{
					if (currentPayment.DocType != PayrollType.Adjustment && currentPayment.DocType != PayrollType.VoidCheck)
					{
						bool oldIsActive = paymentDeduct.IsActive ?? false;
						paymentDeduct.IsActive = false;
						PXUIFieldAttribute.SetEnabled(Deductions.Cache, paymentDeduct, false);
						if (oldIsActive)
						{
							currentPayment.Calculated = false;
							CurrentDocument.Update(currentPayment);
						}
					}
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<PRPaymentDeduct.isActive>(Deductions.Cache, paymentDeduct, true);
				}

				if (currentPayment.Released == false && currentPayment.Printed == false && deductionNotActive)
				{
					Deductions.Cache.RaiseExceptionHandling<PREmployeeDeduct.codeID>(
						paymentDeduct,
						paymentDeduct.CodeID,
						new PXSetPropertyException(Messages.DeductCodeInactive, PXErrorLevel.Warning));
				}

				PRDeductCode deductCode = deductCodes.Where(x => x.CodeID == paymentDeduct.CodeID).FirstOrDefault();
				if (deductCode != null)
				{
					yield return new PXResult<PRPaymentDeduct, PRDeductCode>(paymentDeduct, deductCode);
				}
			}
		}

		protected IEnumerable paymentPTOBanks()
		{
			PXView viewSelect = new PXView(this, true, PaymentPTOBanks.View.BqlSelect);
			HashSet<string> paymentBankIDs = new HashSet<string>();
			List<PRPaymentPTOBank> queryResults = new List<PRPaymentPTOBank>();

			foreach (PRPaymentPTOBank record in PaymentPTOBanks.Cache.Cached)
			{
				if (!paymentBankIDs.Contains(record.BankID) &&
					(PaymentPTOBanks.Cache.GetStatus(record) == PXEntryStatus.Updated || PaymentPTOBanks.Cache.GetStatus(record) == PXEntryStatus.Inserted))
				{
					paymentBankIDs.Add(record.BankID);
					queryResults.Add(record);
				}
			}

			foreach (PRPaymentPTOBank bank in viewSelect.SelectMulti().Where(x => !paymentBankIDs.Contains(((PRPaymentPTOBank)x).BankID)))
			{
				var record = PaymentPTOBanks.Cache.Locate(bank) ?? bank;
				if (PaymentPTOBanks.Cache.GetStatus(record) != PXEntryStatus.Deleted)
				{
					paymentBankIDs.Add(bank.BankID);
					queryResults.Add(bank);
				}
			}

			if (CurrentDocument.Current?.DocType == PayrollType.Adjustment)
			{
				foreach (object record in queryResults)
				{
					PXUIFieldAttribute.SetEnabled<PRPaymentPTOBank.isActive>(PaymentPTOBanks.Cache, record, true);
					yield return record;
				}
			}
			else
			{
				PXResultset<PREmployeePTOBank> employeeBanks = new SelectFrom<PREmployeePTOBank>
					.Where<PREmployeePTOBank.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View(this).Select();

				foreach (PRPaymentPTOBank bank in queryResults)
				{
					PREmployeePTOBank employeeBank = employeeBanks.FirstOrDefault(x => ((PREmployeePTOBank)x).BankID == bank.BankID);
					PXUIFieldAttribute.SetEnabled<PRPaymentPTOBank.isActive>(PaymentPTOBanks.Cache, bank, employeeBank?.IsActive ?? false);
					yield return bank;
				}
			}
		}

		protected IEnumerable taxSplits()
		{
			if (CurrentDocument.Current.DocType == PayrollType.Adjustment && Taxes.Current != null)
			{
				List<object> existingSplits = new PXView(this, false, TaxSplits.View.BqlSelect).SelectMulti();
				foreach (int wageType in PRTypeSelectorAttribute.GetAll<PRWage>().Where(x => x.HasDescription).Select(x => x.ID))
				{
					if (!existingSplits.Any(x => ((PRPaymentTaxSplit)x).WageType == wageType && ((PRPaymentTaxSplit)x).TaxID == Taxes.Current.TaxID))
					{
						TaxSplits.Update(new PRPaymentTaxSplit()
						{
							TaxID = Taxes.Current.TaxID,
							WageType = wageType
						});
					}
				}
			}

			return null;
		}

		public IEnumerable wCPremiums()
		{
			PXView bqlSelect = new PXView(this, false, WCPremiums.View.BqlSelect);

			foreach (object objResult in bqlSelect.SelectMulti())
			{
				PXResult<PRPaymentWCPremium, PMWorkCode, PRDeductCode> result = objResult as PXResult<PRPaymentWCPremium, PMWorkCode, PRDeductCode>;
				if (result != null)
				{
					PRPaymentWCPremium premium = (PRPaymentWCPremium)result;
					PRDeductCode deductCode = (PRDeductCode)result;

					if (CurrentDocument.Current.Released == false && CurrentDocument.Current.Printed == false &&
						premium.DeductCodeID != null && deductCode.IsActive != true)
					{
						WCPremiums.Cache.RaiseExceptionHandling<PRPaymentWCPremium.deductCodeID>(
							premium,
							premium.DeductCodeID,
							new PXSetPropertyException(Messages.DeductCodeInactive, PXErrorLevel.Warning));
					}

					yield return result;
				}
			}
		}

		public IEnumerable projectPackageDeductions()
		{
			PXView bqlSelect = new PXView(this, false, ProjectPackageDeductions.View.BqlSelect);

			foreach (IGrouping<int?, PXResult<PRPaymentProjectPackageDeduct, PRDeductCode, PRDeductionAndBenefitProjectPackage>> resultGroup in bqlSelect.SelectMulti()
				.Cast<PXResult<PRPaymentProjectPackageDeduct, PRDeductCode, PRDeductionAndBenefitProjectPackage>>()
				.GroupBy(x => ((PRPaymentProjectPackageDeduct)x).RecordID))
			{
				PRPaymentProjectPackageDeduct packageDeduct = resultGroup.First();
				PRDeductCode deductCode = resultGroup.First();
				PRDeductionAndBenefitProjectPackage package = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitProjectPackage)x).EffectiveDate).First();

				if (CurrentDocument.Current.Released == false && CurrentDocument.Current.Printed == false &&
					packageDeduct.DeductCodeID != null)
				{
					if (deductCode.IsActive != true)
					{
						ProjectPackageDeductions.Cache.RaiseExceptionHandling<PRPaymentProjectPackageDeduct.deductCodeID>(
							packageDeduct,
							packageDeduct.DeductCodeID,
							new PXSetPropertyException(Messages.DeductCodeInactive, PXErrorLevel.Warning));
					}

					if (packageDeduct.ProjectID != null && package.RecordID == null)
					{
						ProjectPackageDeductions.Cache.RaiseExceptionHandling<PRPaymentProjectPackageDeduct.projectID>(
							packageDeduct,
							packageDeduct.ProjectID,
							new PXSetPropertyException(Messages.CantFindProjectPackageDeduct, PXErrorLevel.RowWarning));
					}
				}

				yield return new PXResult<PRPaymentProjectPackageDeduct, PRDeductCode, PRDeductionAndBenefitProjectPackage>(packageDeduct, deductCode, package);
			}
		}

		public IEnumerable unionPackageDeductions()
		{
			PXView bqlSelect = new PXView(this, false, UnionPackageDeductions.View.BqlSelect);

			foreach (IGrouping<int?, PXResult<PRPaymentUnionPackageDeduct, PRDeductCode, PRDeductionAndBenefitUnionPackage>> resultGroup in bqlSelect.SelectMulti()
				.Cast<PXResult<PRPaymentUnionPackageDeduct, PRDeductCode, PRDeductionAndBenefitUnionPackage>>()
				.GroupBy(x => ((PRPaymentUnionPackageDeduct)x).RecordID))
			{
				PRPaymentUnionPackageDeduct packageDeduct = resultGroup.First();
				PRDeductCode deductCode = resultGroup.First();
				PRDeductionAndBenefitUnionPackage package = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitUnionPackage)x).EffectiveDate).First();

				if (CurrentDocument.Current.Released == false && CurrentDocument.Current.Printed == false &&
					packageDeduct.DeductCodeID != null)
				{
					if (deductCode.IsActive != true)
					{
						UnionPackageDeductions.Cache.RaiseExceptionHandling<PRPaymentUnionPackageDeduct.deductCodeID>(
							packageDeduct,
							packageDeduct.DeductCodeID,
							new PXSetPropertyException(Messages.DeductCodeInactive, PXErrorLevel.Warning));
					}

					if (packageDeduct.UnionID != null && package.RecordID == null)
					{
						UnionPackageDeductions.Cache.RaiseExceptionHandling<PRPaymentUnionPackageDeduct.unionID>(
							packageDeduct,
							packageDeduct.UnionID,
							new PXSetPropertyException(Messages.CantFindUnionPackageDeduct, PXErrorLevel.RowWarning));
					}
				}

				yield return new PXResult<PRPaymentUnionPackageDeduct, PRDeductCode, PRDeductionAndBenefitUnionPackage>(packageDeduct, deductCode, package);
			}
		}
		#endregion Data View Delegates

		#region CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PaymentRegularAmount(nameof(Earnings))]
		protected virtual void _(Events.CacheAttached<PRPayment.regularAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(EmployeeAttribute), nameof(EmployeeAttribute.IsKey), true)]
		[PXDBDefault(typeof(PRPayment.employeeID))]
		protected virtual void PREarningDetail_EmployeeID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXDBStringAttribute), nameof(PXDBStringAttribute.IsKey), true)]
		[PXDBDefault(typeof(PRPayment.docType))]
		protected virtual void PREarningDetail_PaymentDocType_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXDBStringAttribute), nameof(PXDBStringAttribute.IsKey), true)]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		protected virtual void PREarningDetail_PaymentRefNbr_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(PRPayment.payBatchNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void PREarningDetail_BatchNbr_CacheAttached(PXCache sender) { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Deduction Code")]
		[PRUniqueDeductionCodeSelector(typeof(PRDeductCode.codeID), SubstituteKey = typeof(PRDeductCode.codeCD), DescriptionField = typeof(PRDeductCode.description))]
		protected virtual void PRPaymentDeduct_CodeID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(null, typeof(SumCalc<PRPayment.dedAmount>))]
		protected virtual void _(Events.CacheAttached<PRDeductionDetail.amount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(null, typeof(SumCalc<PRPayment.benefitAmount>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<Selector<PRBenefitDetail.codeID, PRDeductCode.isPayableBenefit>, Equal<True>>, PRBenefitDetail.amount>, decimal0>),
			typeof(SumCalc<PRPayment.payableBenefitAmount>))]
		protected virtual void _(Events.CacheAttached<PRBenefitDetail.amount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXUnboundDefaultAttribute))]
		[PXUnboundDefault(typeof(SelectFrom<PREmployee>
			.Where<PREmployee.bAccountID.IsEqual<PREmployeePTOBank.bAccountID.FromCurrent>>
			.SearchFor<PREmployee.employeeClassID>))]
		protected virtual void _(Events.CacheAttached<PREmployeePTOBank.employeeClassID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDecimalAttribute))]
		[PXDBDecimal]
		protected virtual void _(Events.CacheAttached<PREarningDetail.hours> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDecimalAttribute))]
		[PXDBDecimal]
		protected virtual void _(Events.CacheAttached<PREarningDetail.units> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBDateAttribute))]
		[DateInPeriod(typeof(PRPayment), typeof(PRPayment.startDate), typeof(PRPayment.endDate), nameof(Earnings))]
		protected virtual void _(Events.CacheAttached<PREarningDetail.date> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), false)]
		protected virtual void _(Events.CacheAttached<PRPaymentTaxSplit.taxID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Deduction Calculation Method")]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXUIRequiredAttribute))]
		[DeductionCalcMethodDisplay]
		protected virtual void _(Events.CacheAttached<PRDeductCode.dedCalcType> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Contribution Calculation Method")]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXUIRequiredAttribute))]
		[BenefitCalcMethodDisplay]
		protected virtual void _(Events.CacheAttached<PRDeductCode.cntCalcType> e) { }
		#endregion

		#region Actions
		public PXAction<PRPayment> Action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(MenuAutoOpen = true)]
		public virtual void action() { }

		public PXAction<PRPayment> Calculate;
		[PXUIField(DisplayName = Messages.Calculate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable calculate(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<PRPayment> list = new List<PRPayment>();
			foreach (PRPayment pmt in adapter.Get<PRPayment>())
			{
				cache.Update(pmt);
				list.Add(pmt);
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}

			// Pull list of union and project deductions in case deductions packages were updated after creation of earning details
			Earnings.Select().FirstTableItems.ForEach(x =>
			{
				AddUnionDeductions(x, false);
				AddProjectDeductions(x, false);
			});

			//Deleting all details to prevent validation of empty Accounts before saving
			Taxes.Select().ForEach(x => Taxes.Delete(x));
			DeductionDetails.Select().FirstTableItems.ForEach(x => DeductionDetails.Delete(x));
			BenefitDetails.Select().FirstTableItems.ForEach(x => BenefitDetails.Delete(x));
			WCPremiums.Select().FirstTableItems.ForEach(x => WCPremiums.Delete(x));
			ProjectPackageDeductions.Select().FirstTableItems.ForEach(x => ProjectPackageDeductions.Delete(x));
			UnionPackageDeductions.Select().FirstTableItems.ForEach(x => UnionPackageDeductions.Delete(x));
			PaymentFringeBenefits.Select().FirstTableItems.ForEach(x => PaymentFringeBenefits.Delete(x));
			AllPaymentFringeBenefitsDecreasingRate.Select().FirstTableItems.ForEach(x => AllPaymentFringeBenefitsDecreasingRate.Delete(x));
			AllPaymentFringeEarningsDecreasingRate.Select().FirstTableItems.ForEach(x => AllPaymentFringeEarningsDecreasingRate.Delete(x));

			InsertPaidCarryoverEarnings();
			Save.Press();

			PXLongOperation.StartOperation(this, delegate ()
			{
				foreach (var bank in PaymentPTOBanks.Select())
				{
					CalculatePTO(bank);
				}
				PRCalculationEngine.Run(list, false);
				this.Actions.PressSave();
			});
			return list;
		}

		public PXAction<PRPayment> PrintCheck;
		[PXUIField(DisplayName = "Print Check", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual void printCheck()
		{
			VerifyDeductionBenefitAndTaxDetails();
			SetCostAssignmentType(Document.Current);
			Actions.PressSave();

			var payment = Document.Current;
			if (payment.DocType == PayrollType.Adjustment && payment.Calculated != true)
			{
				PRCalculationEngine calculationEngine = CreateInstance<PRCalculationEngine>();
				calculationEngine.SetDirectDepositSplit(payment);
				calculationEngine.Persist();
			}

			var graph = PXGraph.CreateInstance<PRPrintChecks>();
			PrintChecksFilter filterCopy = PXCache<PrintChecksFilter>.CreateCopy(graph.Filter.Current);
			filterCopy.CashAccountID = payment.CashAccountID;
			filterCopy.PaymentMethodID = payment.PaymentMethodID;
			graph.Filter.Cache.Update(filterCopy);

			payment.Selected = true;
			graph.PaymentList.Cache.Update(payment);
			graph.PaymentList.Cache.IsDirty = false;
			throw new PXRedirectRequiredException(graph, "Preview");
		}

		public PXAction<PRPayment> VoidPayment;
		[PXUIField(DisplayName = Messages.Void, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable voidPayment(PXAdapter adapter)
		{
			List<PRPayment> paymentList = new List<PRPayment>();

			if (Document.Current != null &&
				Document.Current.Released == true &&
				Document.Current.Voided == false &&
				Document.Current.DocType != PayrollType.VoidCheck)
			{
				PRPayment voidcheck = CurrentDocument.Select(PayrollType.VoidCheck);
				if (voidcheck != null)
				{
					paymentList.Add(voidcheck);
					return paymentList;
				}

				PRPayment doc = PXCache<PRPayment>.CreateCopy(Document.Current);
				try
				{
					_IsVoidCheckInProgress = true;
					VoidCheckProc(doc);
				}
				catch (PXSetPropertyException)
				{
					Clear();
					Document.Current = doc;
					throw;
				}
				finally
				{
					_IsVoidCheckInProgress = false;
				}

				Actions.PressSave();
				paymentList.Add(Document.Current);
				return paymentList;
			}
			return Document.Select();
		}

		public PXAction<PRPayment> CopySelectedEarningDetailLine;
		[PXUIField(DisplayName = "Copy Selected Entry", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable copySelectedEarningDetailLine(PXAdapter adapter)
		{
			EarningDetailHelper.CopySelectedEarningDetailRecord(Earnings.Cache);
			return adapter.Get();
		}

		public PXAction<PRPayment> ViewOvertimeRules;
		[PXUIField(DisplayName = "Overtime Rules", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void viewOvertimeRules()
		{
			PaymentOvertimeRules.AskExt();
		}

		public PXAction<PRPayment> RevertOvertimeCalculation;
		[PXUIField(DisplayName = "Revert Overtime Calculations and Close", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable revertOvertimeCalculation(PXAdapter adapter)
		{
			RevertPaymentOvertimeCalculation(this, Document.Current, Earnings.View);

			return adapter.Get();
		}

		public PXAction<PRPayment> Release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<PRPayment> list = new List<PRPayment>();
			foreach (PRPayment pmt in adapter.Get<PRPayment>())
			{
				if (pmt.Status != PaymentStatus.Released)
				{
					if (pmt.DocType == PayrollType.Adjustment && pmt.Calculated != true)
					{
						Document.Current = pmt;
						VerifyDeductionBenefitAndTaxDetails();
					}

					if (pmt.LaborCostSplitType == null)
					{
						SetCostAssignmentType(pmt);
					}
					DefaultDescription(cache, pmt);
					cache.Update(pmt);
					list.Add(pmt);
				}
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			Save.Press();
			PXLongOperation.StartOperation(this, delegate () { PRDocumentProcess.ReleaseDoc(list, false); });
			return list;
		}

		public PXAction<PRPayment> ViewDeductionDetails;
		[PXUIField(DisplayName = "Deduction Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void viewDeductionDetails()
		{
			DeductionDetails.AskExt();
		}

		public PXAction<PRPayment> ViewBenefitDetails;
		[PXUIField(DisplayName = "Benefit Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void viewBenefitDetails()
		{
			BenefitDetails.AskExt();
		}

		public PXAction<PRPayment> ViewTaxDetails;
		[PXUIField(DisplayName = "Tax Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void viewTaxDetails()
		{
			TaxDetails.AskExt();
		}

		public PXAction<PRPayment> ViewOriginalDocument;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewOriginalDocument(PXAdapter adapter)
		{
			var document = Document.Search<PRPayment.refNbr>(Document.Current.OrigRefNbr, Document.Current.OrigDocType).FirstTableItems.FirstOrDefault();
			if (document != null)

			{
				PXRedirectHelper.TryRedirect(this, document, PXRedirectHelper.WindowMode.NewWindow);
			}

			return adapter.Get();
		}

		public PXAction<PRPayment> ViewDirectDepositSplits;
		[PXUIField(DisplayName = "View Direct Deposit Splits", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual void viewDirectDepositSplits()
		{
			DirectDepositSplits.AskExt();
		}

		public PXAction<PRPayment> ViewExistingPayment;
		[PXUIField(DisplayName = "View Existing Paycheck", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewExistingPayment(PXAdapter adapter)
		{
			PRPayment existingPayment = SelectFrom<PRPayment>.
				Where<PRPayment.docType.IsEqual<P.AsString>.
					And<PRPayment.refNbr.IsEqual<P.AsString>>>.View.
					SelectSingleBound(this, null, ExistingPayment.Current.DocType, ExistingPayment.Current.RefNbr);

			Clear();
			if (existingPayment != null)
				PXRedirectHelper.TryRedirect(this, existingPayment, PXRedirectHelper.WindowMode.Same);

			return adapter.Get();
		}

		public PXAction<PRPayment> ViewExistingPayrollBatch;
		[PXUIField(DisplayName = "View Existing Payroll Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewExistingPayrollBatch(PXAdapter adapter)
		{
			PRBatch existingPayrollBatch = SelectFrom<PRBatch>.Where<PRBatch.batchNbr.IsEqual<P.AsString>>.View.
				SelectSingleBound(this, null, ExistingPayrollBatch.Current.BatchNbr);

			if (existingPayrollBatch != null)
			{
				Clear();
				PRPayBatchEntry payBatchEntryGraph = CreateInstance<PRPayBatchEntry>();
				PXRedirectHelper.TryRedirect(payBatchEntryGraph, existingPayrollBatch, PXRedirectHelper.WindowMode.Same);
			}

			return adapter.Get();
		}

		public PXAction<PRPayment> CancelPrint;
		[PXUIField(DisplayName = "Cancel Print", MapViewRights = PXCacheRights.Update, MapEnableRights = PXCacheRights.Update)]
		[PXButton]
		protected virtual void cancelPrint()
		{
			PaymentMethod paymentMethod = PaymentMethod.SelectSingle();
			var paymentMethodExt = paymentMethod.GetExtension<PRxPaymentMethod>();
			bool isPrintCheck = paymentMethodExt.PRPrintChecks == true;

			bool containsNotMatchingDeducts = GetChildDeductRecordsWithSourceNotMatching(
				out List<PRPaymentDeduct> notMatchingPaymentDeducts,
				out List<PRPaymentProjectPackageDeduct> notMatchingProjectPackages,
				out List<PRPaymentUnionPackageDeduct> notMatchingUnionPackages,
				out List<PRPaymentWCPremium> notMatchingWorkCodePackages);

			string message = isPrintCheck ? Messages.CancelPrintedCheckWarning : Messages.RemoveFromDirectDepositWarning;
			if (Document.Current.DocType == PayrollType.Adjustment && containsNotMatchingDeducts)
			{
				message += Messages.DeductsWillBeRemoved;
			}
			message += Messages.PressOK;

			if (Document.Ask(message, MessageButtons.OKCancel) == WebDialogResult.OK)
			{
				using (var scope = new PXTransactionScope())
				{
					if (isPrintCheck)
					{
						Document.Current.ExtRefNbr = null;
					}
					else
					{
						var batchUpdateGraph = PXGraph.CreateInstance<PRCABatchUpdate>();
						foreach (PXResult<CABatchDetail, CABatch> result in DirectDepositBatchAndDetails.Select())
						{
							if (batchUpdateGraph.Document.Current == null)
							{
								batchUpdateGraph.Document.Current = result;
							}

							DirectDepositBatchAndDetails.Delete(result);
						}

						if (SelectFrom<CABatchDetail>
							.Where<CABatchDetail.batchNbr.IsEqual<P.AsString>>
							.View.Select(this, batchUpdateGraph.Document.Current.BatchNbr).RowCount == 0)
						{
							batchUpdateGraph.Document.DeleteCurrent();
						}
						else
						{
							Persist();
							batchUpdateGraph.RecalcTotals();
							batchUpdateGraph.Document.Update(batchUpdateGraph.Document.Current);
						}

						batchUpdateGraph.Persist();

						DirectDepositSplits.Select().ForEach(x => DirectDepositSplits.Delete(x));
					}

					if (containsNotMatchingDeducts)
					{
						DeleteChildDeductRecordsWithSourceNotMatching(notMatchingPaymentDeducts, notMatchingProjectPackages, notMatchingUnionPackages, notMatchingWorkCodePackages);
					}

					Document.Current.Printed = false;
					Document.Update(Document.Current);
					Actions.PressSave();
					scope.Complete();
				}
			}
		}

		public PXAction<PRPayment> ViewProjectDeductionAndBenefitPackages;
		[PXUIField(DisplayName = "Deduction and Benefit Packages", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void viewProjectDeductionAndBenefitPackages()
		{
			ProjectPackageDeductions.AskExt();
		}

		public CatchRightsErrorAction<PRPayment> CheckTaxUpdateTimestamp;
		[PXButton]
		[PXUIField(Visible = false)]
		public virtual void checkTaxUpdateTimestamp()
		{
			if (!TaxUpdateHelpers.CheckTaxUpdateTimestamp(UpdateHistory.View))
			{
				UpdateTaxesPopupView.Current.Message = Messages.TaxUpdateNeeded;
				UpdateTaxesPopupView.AskExt();
			}
		}

		public PXAction<PRPayment> RedirectTaxMaintenance;
		[PXButton]
		[PXUIField(DisplayName = "Tax Maintenance")]
		protected virtual IEnumerable redirectTaxMaintenance(PXAdapter adapter)
		{
			PXRedirectHelper.TryRedirect(CreateInstance<PRTaxMaintenance>(), PXRedirectHelper.WindowMode.Same);
			return adapter.Get();
		}

		public PXAction<PRPayment> DeleteEarningDetail;
		[PXUIField(DisplayName = "Delete", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual void deleteEarningDetail()
		{
			if (Earnings.Current.IsPayingCarryover == true)
			{
				WebDialogResult promptResult = Earnings.Ask(Messages.AskSkipCarryoverPayments, MessageButtons.YesNo);
				Document.Current.AutoPayCarryover = promptResult == WebDialogResult.No;
			}

			Earnings.DeleteCurrent();
		}

		#endregion

		#region Events
		protected void _(Events.RowSelected<PRPayment> e)
		{
			if (e.Row == null) return;

			PXEntryStatus currentRecordStatus = e.Cache.GetStatus(e.Row);
			e.Cache.Adjust<PXUIFieldAttribute>(e.Row).
				For<PRPayment.payGroupID>(field => field.Enabled = currentRecordStatus == PXEntryStatus.Inserted).
				SameFor<PRPayment.payPeriodID>().
				SameFor<PRPayment.employeeID>();

			bool enableDetailEdit = e.Row.DocType == PayrollType.Adjustment && e.Row.Released == false && e.Row.Printed == false;
			bool exemptFromCertifiedReporting = ((PREmployee)PXSelectorAttribute.Select<PRPayment.employeeID>(e.Cache, e.Row))?.ExemptFromCertifiedReporting == true;

			Document.Cache.AllowUpdate = e.Row.Released == false;
			Document.Cache.AllowDelete = (e.Row.Released == false && e.Row.Printed == false);
			PaymentPTOBanks.Cache.AllowUpdate = (e.Row.Released == false && e.Row.Printed == false);
			Earnings.Cache.AllowInsert = (e.Row.TransactionDate != null && e.Row.EmployeeID != null && e.Row.Released == false && e.Row.Printed == false);
			Earnings.Cache.AllowUpdate = e.Row.Released == false;
			Earnings.Cache.AllowDelete = (e.Row.Released == false && e.Row.Printed == false);
			Deductions.Cache.AllowInsert = (e.Row.TransactionDate != null && e.Row.EmployeeID != null && e.Row.Released == false && e.Row.Printed == false);
			Deductions.Cache.AllowUpdate = (e.Row.Released == false && e.Row.Printed == false);
			DeductionDetails.Cache.AllowInsert = enableDetailEdit;
			DeductionDetails.Cache.AllowUpdate = e.Row.Released == false;
			DeductionDetails.Cache.AllowDelete = enableDetailEdit;
			BenefitDetails.Cache.AllowInsert = enableDetailEdit;
			BenefitDetails.Cache.AllowUpdate = e.Row.Released == false;
			BenefitDetails.Cache.AllowDelete = enableDetailEdit;
			Taxes.Cache.AllowInsert = enableDetailEdit && e.Row.TransactionDate != null && e.Row.EmployeeID != null;
			Taxes.Cache.AllowUpdate = enableDetailEdit;
			Taxes.Cache.AllowDelete = enableDetailEdit;
			TaxDetails.Cache.AllowInsert = enableDetailEdit;
			TaxDetails.Cache.AllowUpdate = e.Row.Released == false;
			TaxDetails.Cache.AllowDelete = enableDetailEdit;
			TaxSplits.Cache.AllowUpdate = enableDetailEdit;
			PaymentOvertimeRules.Cache.AllowUpdate = (e.Row.ExemptFromOvertimeRules == false && e.Row.Released != true && e.Row.Printed == false);
			WCPremiums.Cache.AllowInsert = enableDetailEdit && e.Row.TransactionDate != null && e.Row.EmployeeID != null;
			WCPremiums.Cache.AllowUpdate = enableDetailEdit;
			WCPremiums.Cache.AllowDelete = enableDetailEdit;
			UnionPackageDeductions.Cache.AllowInsert = enableDetailEdit && e.Row.TransactionDate != null && e.Row.EmployeeID != null;
			UnionPackageDeductions.Cache.AllowUpdate = enableDetailEdit;
			UnionPackageDeductions.Cache.AllowDelete = enableDetailEdit;
			ProjectPackageDeductions.Cache.AllowInsert = enableDetailEdit && e.Row.TransactionDate != null && e.Row.EmployeeID != null;
			ProjectPackageDeductions.Cache.AllowUpdate = enableDetailEdit;
			ProjectPackageDeductions.Cache.AllowDelete = enableDetailEdit;

			Calculate.SetEnabled(IsCalculateActionEnabled(e.Row));
			PrintCheck.SetEnabled(e.Row.Hold == false && e.Row.Printed == false && (e.Row.Calculated == true || e.Row.DocType == PayrollType.Adjustment) &&
				e.Row.NetAmount > 0 && PRSetup.Current.UpdateGL == true);
			Release.SetEnabled(IsReleaseActionEnabled(e.Row, PRSetup.Current.UpdateGL ?? false));
			VoidPayment.SetEnabled(e.Row.Status == PaymentStatus.Released);
			CopySelectedEarningDetailLine.SetEnabled(Earnings.Cache.AllowInsert);
			ViewOvertimeRules.SetEnabled(e.Row.ExemptFromOvertimeRules == false);
			CancelPrint.SetEnabled(e.Row.Status == PaymentStatus.CheckPrintedOrPaid);
			CheckTaxUpdateTimestamp.SetEnabled(true);
			RedirectTaxMaintenance.SetEnabled(true);

			if (e.Row.DocType == PayrollType.VoidCheck)
			{
				Document.Cache.AllowUpdate = (e.Row.Released == false);
				Document.Cache.AllowDelete = (e.Row.Released == false);
				DeductionDetails.Cache.AllowUpdate = false;
				BenefitDetails.Cache.AllowUpdate = false;
				TaxDetails.Cache.AllowUpdate = false;
				VoidPayment.SetEnabled(false);
				Calculate.SetEnabled(false);
				PrintCheck.SetEnabled(false);
				CopySelectedEarningDetailLine.SetEnabled(false);
				ViewOvertimeRules.SetEnabled(false);
			}

			if (e.Row.Printed == true)
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PRPayment.docType>(e.Cache, e.Row);
				PXUIFieldAttribute.SetEnabled<PRPayment.refNbr>(e.Cache, e.Row);
				PXUIFieldAttribute.SetEnabled<PRPayment.hold>(e.Cache, e.Row, e.Row.DocType == PayrollType.VoidCheck);
			}
		}

		protected void _(Events.RowInserted<PRPayment> e)
		{
			// Force PXNoteAttribute to create a Note record referencing the NoteID field,
			// so that project transactions contain the payment's RefNbr
			PXNoteAttribute.GetNoteID(e.Cache, e.Row, nameof(PRPayment.noteID));
		}

		protected void _(Events.RowSelected<PRPaymentEarning> e)
		{
			PRSetup setup = PRSetup.Current;
			if (setup.PayRateDecimalPlaces != null)
				PXDBDecimalAttribute.SetPrecision(e.Cache, nameof(PRPaymentEarning.rate), setup.PayRateDecimalPlaces);
		}

		protected void _(Events.RowPersisting<PRPayment> e)
		{
			foreach (PREarningDetail earning in Earnings.Select())
			{
				object value = earning.Amount;
				Earnings.Cache.RaiseFieldVerifying<PREarningDetail.amount>(earning, ref value);
			}

			if (!PXUIFieldAttribute.GetErrors(Earnings.Cache, null).Any())
			{
				DeleteEmptySummaryEarnings(SummaryEarnings.View, Earnings.Cache);
			}

			//Verify if tax details account values are empty
			TaxDetails.Select().FirstTableItems.ForEach(detailRow =>
			{
				object expenseAccountValue = detailRow.ExpenseAccountID;
				TaxDetails.Cache.RaiseFieldVerifying<PRTaxDetail.expenseAccountID>(detailRow, ref expenseAccountValue);
				object liabilityAccountValue = detailRow.LiabilityAccountID;
				TaxDetails.Cache.RaiseFieldVerifying<PRTaxDetail.liabilityAccountID>(detailRow, ref liabilityAccountValue);
			});
		}

		public void _(Events.FieldUpdated<PRPayment.employeeID> e)
		{
			var row = e.Row as PRPayment;
			if (row == null || _IsVoidCheckInProgress)
			{
				return;
			}

			UpdateChildrenRecords(row);

			PREmployee employee = CurrentEmployee.SelectSingle();
			if (employee != null && !string.Equals(employee.PayGroupID, row.PayGroupID))
				e.Cache.SetValueExt<PRPayment.payGroupID>(e.Row, employee.PayGroupID);

			if (string.IsNullOrWhiteSpace(row.PayPeriodID))
				e.Cache.SetDefaultExt<PRPayment.payPeriodID>(row);

			UpdatePaymentOvertimeRules(Document.Current.ApplyOvertimeRules ?? false);

			if (row.DocType == PayrollType.VoidCheck)
			{
				e.Cache.Adjust<EmployeeActiveInPayGroupAttribute>(e.Row).For<PRPayment.employeeID>(x => x.FilterActive = true);
			}
		}

		public void _(Events.FieldUpdated<PRPayment.payGroupID> e)
		{
			var row = e.Row as PRPayment;
			if (row == null)
			{
				return;
			}

			PREmployee employee = CurrentEmployee.SelectSingle();
			if (employee != null && !string.Equals(employee.PayGroupID, row.PayGroupID))
				row.EmployeeID = null;

			e.Cache.SetDefaultExt<PRPayment.payPeriodID>(row);
		}

		public void _(Events.FieldDefaulting<PRPayment.payPeriodID> e)
		{
			var row = e.Row as PRPayment;
			if (row == null || row.PayGroupID == null || row.EmployeeID == null)
			{
				return;
			}

			var latestPayment = SelectFrom<PRPayment>
				.Where<PRPayment.payGroupID.IsEqual<@P.AsString>
					.And<PRPayment.employeeID.IsEqual<@P.AsInt>>>
				.OrderBy<PRPayment.payPeriodID.Desc>.View.Select(this, row.PayGroupID, row.EmployeeID).FirstTableItems.FirstOrDefault();

			if (latestPayment?.PayPeriodID != null)
			{
				PRPayGroupPeriod nextPayPeriod = PRPayPeriodMaint.FindNextPayPeriod(this, row.PayGroupID, latestPayment.PayPeriodID);
				if (nextPayPeriod != null &&
					GetExistingPayment(row.EmployeeID, nextPayPeriod.FinPeriodID) == null &&
					GetExistingPayrollBatch(row.EmployeeID, nextPayPeriod.FinPeriodID) == null)
				{
					e.NewValue = FinPeriodIDFormattingAttribute.FormatForDisplay(nextPayPeriod.FinPeriodID);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PRPayment.applyOvertimeRules> e)
		{
			bool? applyOvertimeRulesNewValue = e.NewValue as bool?;
			if (!Equals(applyOvertimeRulesNewValue, e.OldValue))
				UpdatePaymentOvertimeRules(applyOvertimeRulesNewValue ?? false);
		}

		protected virtual void _(Events.FieldUpdated<PRPayment.payPeriodID> e)
		{
			if (!Equals(e.NewValue, e.OldValue))
				UpdatePaymentOvertimeRules(Document.Current.ApplyOvertimeRules ?? false);

			PRPayment row = e.Row as PRPayment;
			if (row != null && !_IsVoidCheckInProgress)
				UpdateChildrenRecords(row);
		}

		private void UpdatePaymentOvertimeRules(bool applyOvertimeRules)
		{
			if (Document.Current.DocType == PayrollType.VoidCheck)
				return;

			DeletePaymentOvertimeRules();
			if (applyOvertimeRules)
				InsertPaymentOvertimeRules();
		}

		private void InsertPaymentOvertimeRules()
		{
			if (Document.Current.ExemptFromOvertimeRules == true || Document.Current.IsWeeklyOrBiWeeklyPeriod == null)
				return;

			PXResultset<PRBatchOvertimeRule> batchOvertimeRules = BatchOvertimeRules.Select();
			PXResultset<PROvertimeRule> activeOvertimeRules =
				SelectFrom<PROvertimeRule>.
				Where<PROvertimeRule.isActive.IsEqual<True>>.View.Select(this);

			bool paymentOvertimeRulesModified = false;
			bool weeklyOvertimeRulesAllowed = Document.Current.IsWeeklyOrBiWeeklyPeriod == true;
			if (!batchOvertimeRules.Any_())
			{
				foreach (PROvertimeRule overtimeRule in activeOvertimeRules)
				{
					PRPaymentOvertimeRule paymentOvertimeRule = new PRPaymentOvertimeRule
					{
						OvertimeRuleID = overtimeRule.OvertimeRuleID,
						IsActive = weeklyOvertimeRulesAllowed || overtimeRule.RuleType == PROvertimeRuleType.Daily
					};
					PaymentOvertimeRules.Update(paymentOvertimeRule);
					paymentOvertimeRulesModified = true;
				}
			}
			else
			{
				Dictionary<string, PROvertimeRule> currentActiveOvertimeRules =
					activeOvertimeRules.Select(item => (PROvertimeRule)item).ToDictionary(item => item.OvertimeRuleID);
				foreach (PRBatchOvertimeRule batchOvertimeRule in batchOvertimeRules)
				{
					PROvertimeRule currentOvertimeRule;
					if (!currentActiveOvertimeRules.TryGetValue(batchOvertimeRule.OvertimeRuleID, out currentOvertimeRule))
						continue;

					PRPaymentOvertimeRule paymentOvertimeRule = new PRPaymentOvertimeRule
					{
						OvertimeRuleID = batchOvertimeRule.OvertimeRuleID,
						IsActive = batchOvertimeRule.IsActive.GetValueOrDefault() && (weeklyOvertimeRulesAllowed || currentOvertimeRule.RuleType == PROvertimeRuleType.Daily)
					};

					PaymentOvertimeRules.Update(paymentOvertimeRule);
					paymentOvertimeRulesModified = true;
				}
			}

			if (paymentOvertimeRulesModified)
				Document.Current.Calculated = false;
		}

		private void DeletePaymentOvertimeRules()
		{
			bool paymentOvertimeRulesModified = false;
			PaymentOvertimeRules.Select().ForEach(paymentOvertimeRule =>
			{
				PaymentOvertimeRules.Delete(paymentOvertimeRule);
				paymentOvertimeRulesModified = true;
			});

			if (paymentOvertimeRulesModified)
				Document.Current.Calculated = false;
		}

		public void _(Events.RowPersisted<PRPayment> e)
		{
			if (!e.Cache.Deleted.Any_())
				return;

			string payBatchNumber = e.Row?.PayBatchNbr;
			int? employeeID = e.Row?.EmployeeID;

			if (string.IsNullOrWhiteSpace(payBatchNumber) || employeeID == null)
				return;

			UpdatePayrollBatch(payBatchNumber, employeeID);
		}

		public void _(Events.RowPersisted<PREarningDetail> e)
		{
			if (e.TranStatus != PXTranStatus.Completed)
				return;

			if (Document.Current == null)
				return;

			string payBatchNumber = Document.Current.PayBatchNbr;
			int? employeeID = Document.Current.EmployeeID;

			if (string.IsNullOrWhiteSpace(payBatchNumber))
				return;

			UpdatePayrollBatch(payBatchNumber, employeeID);
		}

		public void _(Events.RowInserted<PRPaymentEarning> e)
		{
			var row = e.Row as PRPaymentEarning;
			if (row == null)
			{
				return;
			}

			UpdateSummaryEarning(this, Document.Current, row);
		}

		public void _(Events.FieldUpdated<PRPaymentDeduct.codeID> e)
		{
			var row = e.Row as PRPaymentDeduct;
			if (row == null)
			{
				return;
			}

			UpdateSummaryDeductions(this, Document.Current, row);
		}

		protected void _(Events.RowSelected<PRPaymentDeduct> e)
		{
			var row = e.Row as PRPaymentDeduct;
			if (row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PRPaymentDeduct.codeID>(e.Cache, row, row.CodeID == null);
			PXUIFieldAttribute.SetEnabled<PRPaymentDeduct.saveOverride>(e.Cache, row, row.Source == PaymentDeductionSourceAttribute.EmployeeSettings);
		}

		protected virtual void _(Events.FieldVerifying<PREarningDetail.hours> e)
		{
			CheckForNegative<PREarningDetail.hours>((decimal?)e.NewValue);
		}

		protected virtual void _(Events.FieldVerifying<PREarningDetail.units> e)
		{
			CheckForNegative<PREarningDetail.units>((decimal?)e.NewValue);
		}

		protected virtual void _(Events.FieldVerifying<PREarningDetail.amount> e)
		{
			PREarningType earningType = (PXSelectorAttribute.Select(e.Cache, e.Row, nameof(PREarningDetail.typeCD)) as EPEarningType)?.GetExtension<PREarningType>();
			CheckForNegative<PREarningDetail.amount>((decimal?)e.NewValue, () => earningType?.IsAmountBased == true);
		}

		protected virtual void _(Events.FieldVerifying<PRPaymentDeduct.dedAmount> e)
		{
			CheckForNegative<PRPaymentDeduct.dedAmount>((decimal?)e.NewValue);
		}

		protected virtual void _(Events.FieldVerifying<PRPaymentDeduct.cntAmount> e)
		{
			CheckForNegative<PRPaymentDeduct.cntAmount>((decimal?)e.NewValue);
		}

		protected virtual void _(Events.FieldVerifying<PRPayment.employeeID> e)
		{
			CheckExistingPaychecksAndBatches(e, e.NewValue as int?, Document.Current.PayPeriodID);
		}

		protected virtual void _(Events.FieldVerifying<PRPayment.payPeriodID> e)
		{
			CheckExistingPaychecksAndBatches(e, Document.Current.EmployeeID, e.NewValue as string);
		}

		public void _(Events.FieldUpdated<PRPaymentTax.taxAmount> e)
		{
			var row = e.Row as PRPaymentTax;
			if (row == null || !(CurrentDocument.Current.DocType == PayrollType.Adjustment || CurrentDocument.Current.DocType == PayrollType.VoidCheck))
			{
				return;
			}

			RecreateTaxDetails(row, row.TaxID);
		}

		public void _(Events.FieldUpdated<PRPaymentTax.taxID> e)
		{
			var row = e.Row as PRPaymentTax;
			if (row == null || e.OldValue == null || CurrentDocument.Current.DocType != PayrollType.Adjustment)
			{
				return;
			}

			RecreateTaxDetails(row, (int?)e.OldValue);
			TaxSplits.Select().FirstTableItems.Where(x => x.TaxID == (int?)e.OldValue).ForEach(x => TaxSplits.Delete(x));
		}

		public void _(Events.FieldUpdated<PRPaymentDeduct.isActive> e)
		{
			var row = e.Row as PRPaymentDeduct;
			//if e.ExternalCall is false, the PRPaymentDeduct row was created by creation of detail line, so we can skip this event to not recreate the same detail line.
			if (row == null || !e.ExternalCall || CurrentDocument.Current.DocType != PayrollType.Adjustment)
			{
				return;
			}

			RecreateDeductionDetails(row);
			RecreateBenefitDetails(row);
		}

		public void _(Events.FieldUpdated<PRPaymentDeduct.dedAmount> e)
		{
			var row = e.Row as PRPaymentDeduct;
			//if e.ExternalCall is false, the PRPaymentDeduct row was created by creation of detail line, so we can skip this event to not recreate the same detail line.
			if (row == null || !e.ExternalCall || CurrentDocument.Current.DocType != PayrollType.Adjustment)
			{
				return;
			}

			RecreateDeductionDetails(row);
		}

		public void _(Events.FieldUpdated<PRPaymentDeduct.cntAmount> e)
		{
			var row = e.Row as PRPaymentDeduct;
			//if e.ExternalCall is false, the PRPaymentDeduct row was created by creation of detail line, so we can skip this event to not recreate the same detail line.
			if (row == null || !e.ExternalCall || CurrentDocument.Current.DocType != PayrollType.Adjustment)
			{
				return;
			}

			RecreateBenefitDetails(row);
		}

		//Skips RefNbr verification during Void process as the RefNbr is already set
		protected virtual void _(Events.FieldVerifying<PRPayment.refNbr> e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		public void _(Events.RowSelected<PRPaymentPTOBank> e)
		{
			var row = e.Row as PRPaymentPTOBank;
			if (row == null)
			{
				return;
			}

			bool enableCondition = CurrentDocument.Current.DocType == PayrollType.Adjustment && CurrentDocument.Current.Released == false &&
				CurrentDocument.Current.Printed == false;
			PXUIFieldAttribute.SetEnabled<PRPaymentPTOBank.accrualAmount>(e.Cache, row, enableCondition);
			PXUIFieldAttribute.SetEnabled<PRPaymentPTOBank.disbursementAmount>(e.Cache, row, enableCondition);

			if (row.AvailableAmount < 0)
			{
				PXUIFieldAttribute.SetWarning<PRPaymentPTOBank.availableAmount>(e.Cache, row, Messages.PTOUsedExceedsAvailable);
			}
		}

		public void _(Events.FieldUpdated<PRPaymentPTOBank.accrualAmount> e)
		{
			PRPaymentPTOBank row = e.Row as PRPaymentPTOBank;
			if (row == null)
				return;

			if (row.AccrualAmount != 0)
			{
				e.Cache.SetValue<PRPaymentPTOBank.isActive>(row, true);
			}
		}

		public void _(Events.FieldUpdating<PRPaymentPTOBank.accrualAmount> e)
		{
			var row = e.Row as PRPaymentPTOBank;
			if (row == null || e.NewValue == null)
			{
				return;
			}

			var newValue = (decimal)e.NewValue;
			if (newValue + row.AccumulatedAmount > row.AccrualLimit)
			{
				e.NewValue = row.AccrualLimit - row.AccumulatedAmount;
			}
		}

		public void _(Events.FieldUpdated<PRPayment.paymentMethodID> e)
		{
			Document.Current.Calculated = false;
		}

		public void _(Events.RowInserted<PREarningDetail> e)
		{
			OnEarningDetailInserted(e.Row);
		}

		public void _(Events.RowUpdated<PREarningDetail> e)
		{
			Document.Current.Calculated = false;
			DeleteEmptySummaryEarnings(SummaryEarnings.View, e.Cache);
		}

		public void _(Events.RowDeleted<PREarningDetail> e)
		{
			Document.Current.Calculated = false;
			DeleteEmptySummaryEarnings(SummaryEarnings.View, e.Cache);
		}

		public void _(Events.RowInserted<PRPaymentDeduct> e)
		{
			Document.Current.Calculated = false;
		}

		public void _(Events.RowUpdated<PRPaymentDeduct> e)
		{
			Document.Current.Calculated = false;
		}

		public void _(Events.RowDeleted<PRPaymentDeduct> e)
		{
			Document.Current.Calculated = false;
		}

		public void _(Events.RowInserted<PRPaymentTax> e)
		{
			Document.Current.Calculated = false;
		}

		public void _(Events.RowUpdated<PRPaymentTax> e)
		{
			Document.Current.Calculated = false;
		}

		public void _(Events.RowDeleted<PRPaymentTax> e)
		{
			Document.Current.Calculated = false;
		}

		protected void _(Events.FieldUpdated<PREarningDetail.unionID> e)
		{
			AddUnionDeductions(e.Row as PREarningDetail, true);
		}

		protected void _(Events.FieldUpdated<PREarningDetail.projectID> e)
		{
			AddProjectDeductions(e.Row as PREarningDetail, true);
			e.Cache.SetDefaultExt<PREarningDetail.certifiedJob>(e.Row);
		}

		protected void _(Events.FieldUpdated<PREarningDetail.labourItemID> e)
		{
			AddUnionDeductions(e.Row as PREarningDetail, true);
			AddProjectDeductions(e.Row as PREarningDetail, true);
		}

		protected void _(Events.RowSelected<PREarningDetail> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, e.Row.IsPayingCarryover == false);
			if (e.Row.IsFringeRateEarning == true || e.Row.BaseOvertimeRecordID != null)
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
			}
			else if (Document.Current.Printed == true && Document.Current.Released == false)
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PREarningDetail.accountID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PREarningDetail.subID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PREarningDetail.costCodeID>(e.Cache, e.Row, true);
			}

			PXUIFieldAttribute.SetWarning<PREarningDetail.hours>(e.Cache, e.Row, e.Row.IsPayingCarryover == true ? Messages.CarryoverPaidWithThisEarningLine : string.Empty);
		}

		protected void _(Events.RowSelected<PRDeductionDetail> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PRDeductionDetail.codeID>(e.Cache, e.Row, e.Row.CodeID == null);

			if (Document.Current.Printed == true && Document.Current.Released == false)
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PRDeductionDetail.accountID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PRDeductionDetail.subID>(e.Cache, e.Row, true);
			}
		}

		public void _(Events.RowSelected<PRPaymentOvertimeRule> e)
		{
			if (e.Row == null || !BatchOvertimeRules.AllowUpdate)
				return;

			bool overtimeRuleEnabled = Document.Current.IsWeeklyOrBiWeeklyPeriod == true || e.Row.RuleType == PROvertimeRuleType.Daily;
			PXUIFieldAttribute.SetEnabled(PaymentOvertimeRules.Cache, e.Row, overtimeRuleEnabled);
			if (!overtimeRuleEnabled)
				PXUIFieldAttribute.SetWarning<PRPaymentOvertimeRule.overtimeRuleID>(PaymentOvertimeRules.Cache, e.Row, Messages.WeeklyOvertimeRulesApplyToWeeklyPeriods);
		}

		protected void _(Events.RowUpdated<PRPaymentWCPremium> e)
		{
			if (e.Row?.DeductCodeID == null || string.IsNullOrEmpty(e.Row?.WorkCodeID))
			{
				return;
			}

			if (Document.Current.DocType == PayrollType.VoidCheck || e.Cache.ObjectsEqual<
				PRPaymentWCPremium.workCodeID,
				PRPaymentWCPremium.deductCodeID,
				PRPaymentWCPremium.deductionAmount,
				PRPaymentWCPremium.amount>(e.Row, e.OldRow))
			{
				return;
			}

			RecreateWorkCompensationDeductionSummaryAndDetails();
		}

		protected void _(Events.RowInserted<PRPaymentWCPremium> e)
		{
			if (e.Row?.DeductCodeID == null || string.IsNullOrEmpty(e.Row?.WorkCodeID))
			{
				return;
			}

			RecreateWorkCompensationDeductionSummaryAndDetails();
		}

		protected void _(Events.RowDeleted<PRPaymentWCPremium> e)
		{
			if (e.Row?.DeductCodeID == null || string.IsNullOrEmpty(e.Row?.WorkCodeID) || !e.ExternalCall)
			{
				return;
			}

			RecreateWorkCompensationDeductionSummaryAndDetails();
		}

		protected void _(Events.RowUpdated<PRPaymentProjectPackageDeduct> e)
		{
			if (e.Row?.DeductCodeID == null || e.Row?.ProjectID == null || !e.ExternalCall)
			{
				return;
			}

			if (Document.Current.DocType == PayrollType.VoidCheck || e.Cache.ObjectsEqual<
				PRPaymentProjectPackageDeduct.projectID,
				PRPaymentProjectPackageDeduct.laborItemID,
				PRPaymentProjectPackageDeduct.deductCodeID,
				PRPaymentProjectPackageDeduct.deductionAmount,
				PRPaymentProjectPackageDeduct.benefitAmount>(e.Row, e.OldRow))
			{
				return;
			}

			RecreateProjectDeductionSummaryAndDetails();
		}

		protected void _(Events.RowInserted<PRPaymentProjectPackageDeduct> e)
		{
			if (e.Row?.DeductCodeID == null || e.Row?.ProjectID == null || !e.ExternalCall)
			{
				return;
			}

			RecreateProjectDeductionSummaryAndDetails();
		}

		protected void _(Events.RowDeleted<PRPaymentProjectPackageDeduct> e)
		{
			if (e.Row?.DeductCodeID == null || e.Row?.ProjectID == null || !e.ExternalCall)
			{
				return;
			}

			RecreateProjectDeductionSummaryAndDetails();
		}

		protected void _(Events.RowUpdated<PRPaymentUnionPackageDeduct> e)
		{
			if (e.Row?.DeductCodeID == null || e.Row?.UnionID == null || !e.ExternalCall)
			{
				return;
			}

			if (Document.Current.DocType == PayrollType.VoidCheck || e.Cache.ObjectsEqual<
				PRPaymentUnionPackageDeduct.unionID,
				PRPaymentUnionPackageDeduct.laborItemID,
				PRPaymentUnionPackageDeduct.deductCodeID,
				PRPaymentUnionPackageDeduct.deductionAmount,
				PRPaymentUnionPackageDeduct.benefitAmount>(e.Row, e.OldRow))
			{
				return;
			}

			RecreateUnionDeductionSummaryAndDetails();
		}

		protected void _(Events.RowInserted<PRPaymentUnionPackageDeduct> e)
		{
			if (e.Row?.DeductCodeID == null || e.Row?.UnionID == null || !e.ExternalCall)
			{
				return;
			}

			RecreateUnionDeductionSummaryAndDetails();
		}

		protected void _(Events.RowDeleted<PRPaymentUnionPackageDeduct> e)
		{
			if (e.Row?.DeductCodeID == null || e.Row?.UnionID == null || !e.ExternalCall)
			{
				return;
			}

			RecreateUnionDeductionSummaryAndDetails();
		}

		protected void _(Events.RowPersisting<PRPaymentDeduct> e)
		{
			if (e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted && e.Row.CodeID == null)
			{
				e.Cancel = true;
			}
		}

		protected void _(Events.FieldUpdated<PRPaymentDeduct.saveOverride> e)
		{
			PRPaymentDeduct row = e.Row as PRPaymentDeduct;
			if (row == null || !e.NewValue.Equals(false))
			{
				return;
			}

			e.Cache.SetDefaultExt<PRPaymentDeduct.dedAmount>(row);
			e.Cache.SetDefaultExt<PRPaymentDeduct.cntAmount>(row);
		}

		protected void _(Events.RowSelected<PRBenefitDetail> e)
		{
			var row = e.Row as PRBenefitDetail;
			if (row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PRBenefitDetail.codeID>(e.Cache, row, row.CodeID == null);

			if (Document.Current.Printed == true && Document.Current.Released == false)
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PRBenefitDetail.liabilityAccountID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PRBenefitDetail.liabilitySubID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PRBenefitDetail.expenseAccountID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PRBenefitDetail.expenseSubID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PRBenefitDetail.costCodeID>(e.Cache, e.Row, true);
			}
		}

		protected void _(Events.RowSelected<PRTaxDetail> e)
		{
			var row = e.Row as PRTaxDetail;
			if (row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PRTaxDetail.taxID>(e.Cache, row, row.TaxID == null);
			PXUIFieldAttribute.SetError<PRTaxDetail.amount>(e.Cache, row, row.AmountErrorMessage, row.Amount.ToString());

			if (Document.Current.Printed == true && Document.Current.Released == false)
			{
				PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PRTaxDetail.liabilityAccountID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PRTaxDetail.liabilitySubID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PRTaxDetail.expenseAccountID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PRTaxDetail.expenseSubID>(e.Cache, e.Row, true);
				PXUIFieldAttribute.SetEnabled<PRTaxDetail.costCodeID>(e.Cache, e.Row, true);
			}
		}

		protected void _(Events.FieldUpdated<PRDeductionDetail.amount> e)
		{
			PRDeductionDetail row = e.Row as PRDeductionDetail;
			if (row == null || Document.Current.DocType != PayrollType.Adjustment || !e.ExternalCall)
			{
				return;
			}

			AdjustDeductionSummary(row.CodeID);
		}

		protected void _(Events.RowInserted<PRDeductionDetail> e)
		{
			if (e.Row == null || Document.Current.DocType != PayrollType.Adjustment || !e.ExternalCall)
			{
				return;
			}

			AdjustDeductionSummary(e.Row.CodeID);
		}

		protected void _(Events.RowDeleted<PRDeductionDetail> e)
		{
			if (e.Row == null || Document.Current.DocType != PayrollType.Adjustment || !e.ExternalCall)
			{
				return;
			}

			AdjustDeductionSummary(e.Row.CodeID);
		}

		protected void _(Events.FieldUpdated<PRBenefitDetail.amount> e)
		{
			PRBenefitDetail row = e.Row as PRBenefitDetail;
			if (row == null || Document.Current.DocType != PayrollType.Adjustment || !e.ExternalCall)
			{
				return;
			}

			AdjustBenefitSummary(row.CodeID);
		}

		protected void _(Events.RowInserted<PRBenefitDetail> e)
		{
			if (e.Row == null || Document.Current.DocType != PayrollType.Adjustment || !e.ExternalCall)
			{
				return;
			}

			AdjustBenefitSummary(e.Row.CodeID);
		}

		protected void _(Events.RowDeleted<PRBenefitDetail> e)
		{
			if (e.Row == null || Document.Current.DocType != PayrollType.Adjustment || !e.ExternalCall)
			{
				return;
			}

			AdjustBenefitSummary(e.Row.CodeID);
		}

		protected void _(Events.FieldUpdated<PRTaxDetail.amount> e)
		{
			PRTaxDetail row = e.Row as PRTaxDetail;
			if (row == null || Document.Current.DocType != PayrollType.Adjustment || !e.ExternalCall)
			{
				return;
			}

			Document.Current.Calculated = false;
			VerifyTaxDetails(row.TaxID, false);
		}

		protected void _(Events.RowDeleting<PRTaxDetail> e)
		{
			if (e.Row == null || Document.Current.DocType != PayrollType.Adjustment || !e.ExternalCall)
			{
				return;
			}

			if (e.Row.Amount != 0)
			{
				VerifyTaxDetails(e.Row.TaxID, false);
			}
		}

		protected void _(Events.RowDeleted<PRTaxDetail> e)
		{
			if (e.Row == null || Document.Current.DocType != PayrollType.Adjustment || !e.ExternalCall)
			{
				return;
			}

			Document.Current.Calculated = false;

			IEnumerable<PRPaymentTaxSplit> taxSplits = TaxSplits.Select().FirstTableItems.Where(x => x.TaxID == e.Row.TaxID);
			IEnumerable<PRTaxDetail> taxDetails = TaxDetails.Select().FirstTableItems.Where(x => x.TaxID == e.Row.TaxID);

			if (!taxDetails.Any())
			{
				// Deleted all tax details for a given TaxID, set tax summary and splits to 0.
				taxSplits.ForEach(x => TaxSplits.Cache.SetValue<PRPaymentTaxSplit.taxAmount>(x, 0m));
				taxSplits.ForEach(x => TaxSplits.Update(x));
			}
		}

		protected void _(Events.FieldUpdated<PRBenefitDetail.labourItemID> e)
		{
			PRBenefitDetail row = e.Row as PRBenefitDetail;
			if (row == null)
			{
				return;
			}

			DefaultBenefitExpenseAcctSub(e.Cache, row);
		}

		protected void _(Events.FieldUpdated<PRBenefitDetail.earningTypeCD> e)
		{
			PRBenefitDetail row = e.Row as PRBenefitDetail;
			if (row == null)
			{
				return;
			}

			DefaultBenefitExpenseAcctSub(e.Cache, row);
		}

		protected void _(Events.FieldUpdated<PRTaxDetail.labourItemID> e)
		{
			PRTaxDetail row = e.Row as PRTaxDetail;
			if (row == null)
			{
				return;
			}

			DefaultTaxExpenseAcctSub(e.Cache, row);
		}

		protected void _(Events.FieldUpdated<PRTaxDetail.earningTypeCD> e)
		{
			PRTaxDetail row = e.Row as PRTaxDetail;
			if (row == null)
			{
				return;
			}

			DefaultTaxExpenseAcctSub(e.Cache, row);
		}

		protected void _(Events.FieldVerifying<PRPayment.paymentMethodID> e)
		{
			PRPayment row = e.Row as PRPayment;
			if (row == null || row.EmployeeID == null || e.NewValue == null)
			{
				return;
			}

			var paymentMethod = PaymentMethod.SelectSingle(e.NewValue);
			var paymentMethodExt = paymentMethod.GetExtension<PRxPaymentMethod>();
			if (paymentMethodExt.PRPrintChecks == false && !SelectFrom<PREmployeeDirectDeposit>
				.Where<PREmployeeDirectDeposit.bAccountID.IsEqual<P.AsInt>>.View.Select(this, row.EmployeeID).Any())
			{
				throw new PXSetPropertyException<PRPayment.paymentMethodID>(Messages.NoBankAccountForDirectDeposit);
			}
		}

		protected void _(Events.FieldUpdated<PRPayment.docType> e)
		{
			if (e.NewValue.Equals(PayrollType.VoidCheck))
			{
				// Allow void checks to be created for inactive employees
				e.Cache.Adjust<EmployeeActiveInPayGroupAttribute>(e.Row).For<PRPayment.employeeID>(x => x.FilterActive = false);
			}
		}

		#endregion Events

		#region Helpers
		public PRPayment InsertNewPayment(PRPayment payment)
		{
			_AllowUpdatePaymentChildrenRecords = false;
			Document.Current = payment;
			payment = Document.Insert(payment);
			if (payment.FinPeriodID == null)
			{
				throw new PXException(Messages.CantFindPostingPeriod, PRPayGroupPeriodIDAttribute.FormatForError(payment.PayPeriodID));
			}
			UpdatePaymentOvertimeRules(payment.ApplyOvertimeRules ?? true);
			_AllowUpdatePaymentChildrenRecords = true;
			UpdateChildrenRecords(payment);
			Actions.PressSave();
			return payment;
		}

		public virtual void OnEarningDetailInserted(PREarningDetail row)
		{
			Document.Current.Calculated = false;

			AddUnionDeductions(row, true);
			AddProjectDeductions(row, true);
		}

		private void UpdateChildrenRecords(PRPayment row)
		{
			if (!_AllowUpdatePaymentChildrenRecords)
				return;

			RecreateSummaryEarnings(row);
			RecreateSummaryDeductions(row);
			RecreateSummaryTaxes(row);

			if (row.TransactionDate.HasValue)
			{
				foreach (PRPaymentEarning summary in SummaryEarnings.Select())
				{
					UpdateSummaryEarning(this, Document.Current, summary);
				}
				CreatePTOBanks(row);
			}
		}

		public static void DeleteEmptySummaryEarnings(PXView summaryEarningsView, PXCache earningDetailCache)
		{
			foreach (PRPaymentEarning row in summaryEarningsView.SelectMulti().Select(x =>
			{
				if (x is PRPaymentEarning)
				{
					return x as PRPaymentEarning;
				}
				else
				{
					return ((PXResult)x)[0] as PRPaymentEarning;
				}
			}))
			{
				if (row.Amount == 0 && row.MTDAmount == 0 && row.QTDAmount == 0 && row.YTDAmount == 0 &&
					!PXParentAttribute.SelectChildren(earningDetailCache, row, typeof(PRPaymentEarning)).Any())
				{
					summaryEarningsView.Cache.Delete(row);
				}
			}
		}

		/// <summary>
		/// Recalculates earnings summary MTD, QTD, YTD amounts according to current document values
		/// </summary>
		/// <param name="row"></param>
		public static void UpdateSummaryEarning(PXGraph graph, PRPayment payment, PRPaymentEarning summaryEarning)
		{
			List<PRYtdEarnings> results = SelectFrom<PRYtdEarnings>.
				Where<PRYtdEarnings.employeeID.IsEqual<PRPayment­.employeeID.FromCurrent>
					.And<PRYtdEarnings.typeCD.IsEqual<P.AsString>
					.And<PRYtdEarnings.locationID.IsEqual<P.AsInt>>
					.And<PRYtdEarnings.year.IsEqual<P.AsString>>>>.View.Select(graph, summaryEarning.TypeCD, summaryEarning.LocationID, payment.TransactionDate.Value.Year).Select(x => (PRYtdEarnings)x).ToList();

			if (results.Any())
			{
				summaryEarning.MTDAmount = results.SingleOrDefault(x => x.Month == payment.TransactionDate.Value.Month)?.Amount ?? 0;
				summaryEarning.YTDAmount = results.Sum(x => x.Amount ?? 0);
				var quarterMonths = PRDateTime.GetQuarterMonths(payment.TransactionDate.Value);
				summaryEarning.QTDAmount = results.Join(quarterMonths, result => result.Month, month => month, (result, month) => result).Sum(result => result.Amount ?? 0);
			}
			else
			{
				summaryEarning.MTDAmount = 0;
				summaryEarning.YTDAmount = 0;
				summaryEarning.QTDAmount = 0;
			}
		}

		/// <summary>
		/// Adds Earning Summary rows for amount already accumulated by the Employee in previous pays
		/// </summary>
		/// <param name="row"></param>
		private void RecreateSummaryEarnings(PRPayment row)
		{
			if (row.TransactionDate != null && row.EmployeeID != null)
			{
				SummaryEarnings.Select().ForEach(x => SummaryEarnings.Delete(x));
				foreach (PRYtdEarnings ytdEarning in EmployeeYTDEarnings.Select(row.TransactionDate.Value.Year))
				{
					var summary = new PRPaymentEarning();
					summary.TypeCD = ytdEarning.TypeCD;
					summary.LocationID = ytdEarning.LocationID;
					summary.Amount = 0;
					summary.Hours = 0;
					SummaryEarnings.Insert(summary);
				}
			}
		}

		/// <summary>
		/// Recalculates deduction summary YTD amounts according to current document values
		/// </summary>
		/// <param name="row"></param>
		public static void UpdateSummaryDeductions(PXGraph graph, PRPayment payment, PRPaymentDeduct summaryDeduct)
		{
			var result = (PRYtdDeductions)SelectFrom<PRYtdDeductions>
				.Where<PRYtdDeductions.employeeID.IsEqual<PRPayment­.employeeID.FromCurrent>
					.And<PRYtdDeductions.codeID.IsEqual<P.AsInt>
					.And<PRYtdDeductions.year.IsEqual<P.AsString>>>>.View.Select(graph, summaryDeduct.CodeID, payment.TransactionDate.Value.Year);
			summaryDeduct.YtdAmount = result?.Amount ?? 0;
			summaryDeduct.EmployerYtdAmount = result?.EmployerAmount ?? 0;
		}

		/// <summary>
		/// Adds deduction Summary rows for amount already accumulated by the Employee in previous pays
		/// </summary>
		/// <param name="row"></param>
		private void RecreateSummaryDeductions(PRPayment row)
		{
			if (row.TransactionDate != null && row.EmployeeID != null)
			{
				Deductions.Select().ForEach(x => Deductions.Delete(x));
				var inserted = new HashSet<int>();

				// 1. Add deductions that are active in the employee as active by default. If pay checks comes from batch and deduction in inactive
				// in batch, add as inactive.
				PXSelectJoin<PREmployeeDeduct,
					InnerJoin<PRDeductCode, On<PRDeductCode.codeID, Equal<PREmployeeDeduct.codeID>>,
					LeftJoin<PRBatchDeduct, On<PRBatchDeduct.codeID, Equal<PREmployeeDeduct.codeID>,
						And<PRBatchDeduct.batchNbr, Equal<Current<PRPayment.payBatchNbr>>>>>>,
					Where<PREmployeeDeduct.isActive, Equal<True>,
						And<PREmployeeDeduct.bAccountID, Equal<Current<PRPayment.employeeID>>,
						And<PRDeductCode.isActive, Equal<True>,
						And<PREmployeeDeduct.startDate, LessEqual<Current<PRPayment.transactionDate>>,
						And<Where<PREmployeeDeduct.endDate, GreaterEqual<Current<PRPayment.transactionDate>>,
							Or<PREmployeeDeduct.endDate, IsNull>>>>>>>>.Select(this).Select(x => (PXResult<PREmployeeDeduct, PRDeductCode, PRBatchDeduct>)x)
					.ForEach(r =>
					{
						int? codeID = ((PRDeductCode)r).CodeID;
						var summary = new PRPaymentDeduct();
						summary.CodeID = codeID;
						summary.IsActive = !(((PRBatchDeduct)r).IsEnabled == false);
						Deductions.Insert(summary);
						inserted.Add(codeID.GetValueOrDefault());
					});

				// 2. Add deductions for which there is a YTD tally as inactive
				foreach (PRYtdDeductions ytdDeduction in EmployeeYTDDeductions.Select(row.TransactionDate.Value.Year))
				{
					var codeID = ytdDeduction.CodeID;

					// YTD values will be set by UpdateSummaryDeductions through CodeID FieldUpdated event
					var summary = new PRPaymentDeduct();
					summary.CodeID = codeID;
					if (!inserted.Contains(codeID.GetValueOrDefault()))
					{
						summary.IsActive = false;
					}
					Deductions.Insert(summary);
				}
			}
		}

		private void RecreateDeductionDetails(PRPaymentDeduct row)
		{
			DeductionDetails.Select().FirstTableItems.Where(x => x.CodeID == row.CodeID)
				.ForEach(x => DeductionDetails.Delete(x));

			PRCalculationEngine.CreateDeductionDetail(this, DeductionDetails.Cache, row, Earnings.Select().FirstTableItems);
		}

		private void RecreateBenefitDetails(PRPaymentDeduct row)
		{
			if (row.NoFinancialTransaction == true)
			{
				return;
			}

			BenefitDetails.Select().FirstTableItems.Where(x => x.CodeID == row.CodeID)
				.ForEach(x => BenefitDetails.Delete(x));

			PRCalculationEngine.CreateBenefitDetail(this, BenefitDetails.Cache, row, Earnings.Select().FirstTableItems);
		}

		private void RecreateTaxDetails(PRPaymentTax summary, int? deleteTaxID)
		{
			TaxDetails.Select().FirstTableItems.Where(x => x.TaxID == deleteTaxID).ForEach(x => TaxDetails.Delete(x));

			Dictionary<int?, PRPaymentTaxSplit> taxSplits = TaxSplits.Select().FirstTableItems.Where(x => x.TaxID == summary.TaxID).ToDictionary(k => k.WageType, v => v);
			PRTaxCode taxCode = (PRTaxCode)PXSelectorAttribute.Select<PRPaymentTax.taxID>(Taxes.Cache, summary);
			PRCalculationEngine.CreateTaxDetail(this, taxCode, taxSplits, Earnings.Select().FirstTableItems);
		}

		/// <summary>
		/// Adds taxes summary rows for amount already accumulated by the Employee in previous pays
		/// </summary>
		/// <param name="row"></param>
		private void RecreateSummaryTaxes(PRPayment row)
		{
			if (row.TransactionDate != null && row.EmployeeID != null)
			{
				Taxes.Select().ForEach(x => Taxes.Delete(x));
				foreach (PRYtdTaxes ytdTaxes in EmployeeYTDTaxes.Select(row.TransactionDate.Value.Year))
				{
					var summary = new PRPaymentTax();
					summary.TaxID = ytdTaxes.TaxID;
					Taxes.Insert(summary);
				}
			}
		}

		private void RecreateWorkCompensationDeductionSummaryAndDetails()
		{
			List<PRPaymentDeduct> wcDeductions = Deductions.Select().FirstTableItems.Where(x => x.Source == PaymentDeductionSourceAttribute.WorkCode).ToList();
			wcDeductions.ForEach(x =>
			{
				x.CntAmount = 0m;
				Deductions.Update(x);
			});

			Dictionary<int?, (decimal deductionAmount, decimal benefitAmount)> premiumTotalsByDeductCode = new Dictionary<int?, (decimal, decimal)>();
			foreach (PRPaymentWCPremium premium in WCPremiums.Select().FirstTableItems.Where(x => x.DeductionAmount != 0 || x.Amount != 0))
			{
				decimal deductionAmount = 0;
				decimal benefitAmount = 0;
				if (premiumTotalsByDeductCode.ContainsKey(premium.DeductCodeID))
				{
					(deductionAmount, benefitAmount) = premiumTotalsByDeductCode[premium.DeductCodeID];
				}
				deductionAmount += premium.DeductionAmount.GetValueOrDefault();
				benefitAmount += premium.Amount.GetValueOrDefault();
				premiumTotalsByDeductCode[premium.DeductCodeID] = (deductionAmount, benefitAmount);
			}

			foreach (KeyValuePair<int?, (decimal deductionAmount, decimal benefitAmount)> kvp in premiumTotalsByDeductCode)
			{
				PRPaymentDeduct deduction = wcDeductions.FirstOrDefault(x => x.CodeID == kvp.Key);
				if (deduction == null)
				{
					deduction = new PRPaymentDeduct()
					{
						CodeID = kvp.Key,
						Source = PaymentDeductionSourceAttribute.WorkCode
					};
					wcDeductions.Add(deduction);
				}
				deduction.IsActive = true;
				deduction.DedAmount = kvp.Value.deductionAmount;
				deduction.CntAmount = kvp.Value.benefitAmount;
				Deductions.Update(deduction);
			}

			wcDeductions.ForEach(x =>
			{
				RecreateDeductionDetails(x);
				RecreateBenefitDetails(x);
			});
		}

		private void RecreateProjectDeductionSummaryAndDetails()
		{
			List<PRPaymentDeduct> projectDeductions = Deductions.Select().FirstTableItems.Where(x => x.Source == PaymentDeductionSourceAttribute.CertifiedProject).ToList();
			projectDeductions.ForEach(x =>
			{
				x.DedAmount = 0m;
				x.CntAmount = 0m;
				Deductions.Update(x);
			});

			Dictionary<int?, (decimal dedAmount, decimal cntAmount)> totalsByDeductCode = new Dictionary<int?, (decimal, decimal)>();
			foreach (IGrouping<int?, PRPaymentProjectPackageDeduct> packageDeductGroup in ProjectPackageDeductions.Select().FirstTableItems
				.Where(x => x.DeductionAmount != 0 || x.BenefitAmount != 0)
				.GroupBy(x => x.RecordID))
			{
				PRPaymentProjectPackageDeduct packageDeduct = packageDeductGroup.First();
				decimal dedAmount = 0m;
				decimal cntAmount = 0m;
				if (totalsByDeductCode.ContainsKey(packageDeduct.DeductCodeID))
				{
					dedAmount = totalsByDeductCode[packageDeduct.DeductCodeID].dedAmount;
					cntAmount = totalsByDeductCode[packageDeduct.DeductCodeID].cntAmount;
				}
				dedAmount += packageDeduct.DeductionAmount.GetValueOrDefault();
				cntAmount += packageDeduct.BenefitAmount.GetValueOrDefault();
				totalsByDeductCode[packageDeduct.DeductCodeID] = (dedAmount, cntAmount);
			}

			foreach (KeyValuePair<int?, (decimal dedAmount, decimal cntAmount)> kvp in totalsByDeductCode)
			{
				PRPaymentDeduct deduction = projectDeductions.FirstOrDefault(x => x.CodeID == kvp.Key);
				if (deduction == null)
				{
					deduction = new PRPaymentDeduct()
					{
						CodeID = kvp.Key,
						Source = PaymentDeductionSourceAttribute.CertifiedProject
					};
					projectDeductions.Add(deduction);
				}
				deduction.IsActive = true;
				deduction.DedAmount = kvp.Value.dedAmount;
				deduction.CntAmount = kvp.Value.cntAmount;
				Deductions.Update(deduction);
			}

			projectDeductions.ForEach(x =>
			{
				RecreateDeductionDetails(x);
				RecreateBenefitDetails(x);
			});
		}

		private void RecreateUnionDeductionSummaryAndDetails()
		{
			List<PRPaymentDeduct> unionDeductions = Deductions.Select().FirstTableItems.Where(x => x.Source == PaymentDeductionSourceAttribute.Union).ToList();
			unionDeductions.ForEach(x =>
			{
				x.DedAmount = 0m;
				x.CntAmount = 0m;
				Deductions.Update(x);
			});

			Dictionary<int?, (decimal dedAmount, decimal cntAmount)> totalsByDeductCode = new Dictionary<int?, (decimal, decimal)>();
			foreach (IGrouping<int?, PRPaymentUnionPackageDeduct> packageDeductGroup in UnionPackageDeductions.Select().FirstTableItems
				.Where(x => x.DeductionAmount != 0 || x.BenefitAmount != 0)
				.GroupBy(x => x.RecordID))
			{
				PRPaymentUnionPackageDeduct packageDeduct = packageDeductGroup.First();
				decimal dedAmount = 0m;
				decimal cntAmount = 0m;
				if (totalsByDeductCode.ContainsKey(packageDeduct.DeductCodeID))
				{
					dedAmount = totalsByDeductCode[packageDeduct.DeductCodeID].dedAmount;
					cntAmount = totalsByDeductCode[packageDeduct.DeductCodeID].cntAmount;
				}
				dedAmount += packageDeduct.DeductionAmount.GetValueOrDefault();
				cntAmount += packageDeduct.BenefitAmount.GetValueOrDefault();
				totalsByDeductCode[packageDeduct.DeductCodeID] = (dedAmount, cntAmount);
			}

			foreach (KeyValuePair<int?, (decimal dedAmount, decimal cntAmount)> kvp in totalsByDeductCode)
			{
				PRPaymentDeduct deduction = unionDeductions.FirstOrDefault(x => x.CodeID == kvp.Key);
				if (deduction == null)
				{
					deduction = new PRPaymentDeduct()
					{
						CodeID = kvp.Key,
						Source = PaymentDeductionSourceAttribute.Union
					};
					unionDeductions.Add(deduction);
				}
				deduction.IsActive = true;
				deduction.DedAmount = kvp.Value.dedAmount;
				deduction.CntAmount = kvp.Value.cntAmount;
				Deductions.Update(deduction);
			}

			unionDeductions.ForEach(x =>
			{
				RecreateDeductionDetails(x);
				RecreateBenefitDetails(x);
			});
		}

		private void CreatePTOBanks(PRPayment row)
		{
			if (row.EmployeeID != null)
			{
				PaymentPTOBanks.Select().ForEach(x => PaymentPTOBanks.Delete(x));
				foreach (IPTOBank bank in GetEmployeeBanks())
				{
					PTOHelper.GetPTOHistory(this, Document.Current.TransactionDate.Value, row.EmployeeID.Value, bank, out decimal accumulatedAmount, out decimal usedAmount, out decimal availableAmount);

					var paymentBank = new PRPaymentPTOBank();
					paymentBank.BankID = bank.BankID;
					paymentBank.AccrualRate = bank.AccrualRate;
					paymentBank.AccrualLimit = bank.AccrualLimit;
					paymentBank.AccumulatedAmount = accumulatedAmount;
					paymentBank.UsedAmount = usedAmount;
					paymentBank.AvailableAmount = availableAmount;

					var ptoBank = PXSelectorAttribute.Select(Caches[bank.GetType()], bank, nameof(PRPTOBank.bankID)) as PRPTOBank;
					paymentBank.IsCertifiedJob = ptoBank.IsCertifiedJobAccrual;
					paymentBank.IsActive = bank.IsActive == true && ptoBank.IsActive == true;

					PaymentPTOBanks.Insert(paymentBank);
				}
			}
		}

		protected virtual void ResetBankInfo(PRPaymentPTOBank paymentBank, IPTOBank bankInfo)
		{
			paymentBank.AccrualAmount = 0;
			paymentBank.DisbursementAmount = 0;
			paymentBank.CarryoverAmount = 0;
			paymentBank.FrontLoadingAmount = 0;
			paymentBank.PaidCarryoverAmount = 0;
			paymentBank.AccrualLimit = bankInfo.AccrualLimit;

			PTOHelper.GetPTOHistory(this, Document.Current.TransactionDate.Value, Document.Current.EmployeeID.Value, bankInfo, out decimal accumulated, out decimal used, out decimal available);
			paymentBank.AccumulatedAmount = accumulated;
			paymentBank.UsedAmount = used;
			paymentBank.AvailableAmount = available;
		}

		private void CalculatePTO(PRPaymentPTOBank bank)
		{
			if (bank.IsActive == true)
			{
				HashSet<string> accrueTimeOff = new HashSet<string>();
				foreach (EPEarningType earningType in SelectFrom<EPEarningType>.View.Select(this).FirstTableItems)
				{
					PREarningType prEarningType = earningType?.GetExtension<PREarningType>();
					if (prEarningType == null || prEarningType.AccruePTO == true)
						accrueTimeOff.Add(earningType.TypeCD);
				}

				IEnumerable<PREarningDetail> earningDetails = Earnings.Select().FirstTableItems;
				if (bank.IsCertifiedJob == true)
				{
					earningDetails = earningDetails.Where(x => x.CertifiedJob == true);
				}
				earningDetails = earningDetails.Where(x => accrueTimeOff.Contains(x.TypeCD));

				var result = PTOBanks.SelectWindowed(0, 1, Document.Current.EmployeeID, bank.BankID).Cast<PXResult<PRPTOBank, PREmployee, PREmployeeClassPTOBank, PREmployeePTOBank>>().FirstOrDefault();
				IPTOBank sourceBank = PTOHelper.GetSourceBank(result, result, result);
				if (sourceBank?.StartDate == null)
				{
					throw new PXException(Messages.BankNotFound, bank.BankID);
				}

				ResetBankInfo(bank, sourceBank);
				bank.AccrualAmount = bank.AccrualRate / 100 * earningDetails.Sum(x =>
				{
					if (x.Date != null && x.Date.Value.Date >= sourceBank.StartDate.Value.Date)
					{
						return x.Hours.GetValueOrDefault() * (CurrentDocument.Current.DocType == PayrollType.VoidCheck ? -1 : 1);
					}
					return 0m;
				}) ?? 0;
				decimal newAccrualAmount = bank.AccrualAmount.GetValueOrDefault();
				decimal newDisbursementAmount = 0m;

				PTOHelper.PTOYearSummary yearSummary = PTOHelper.GetPTOYearSummary(this, Document.Current.TransactionDate.Value, Document.Current.EmployeeID.Value, sourceBank);
				if (Document.Current.DocType == PayrollType.Regular)
				{
					//Process Carryover, Front-Loading and paid carryover
					if (yearSummary.ProcessedCarryover == false)
					{
						bank.CarryoverAmount += PTOHelper.CalculateAmountToCarryover(this, Document.Current.EmployeeID, sourceBank, yearSummary.StartDate, yearSummary.EndDate);
						bank.ProcessedCarryover = true;
						newAccrualAmount += bank.CarryoverAmount.GetValueOrDefault();
					}
					if (yearSummary.ProcessedFrontLoading == false)
					{
						bank.FrontLoadingAmount += sourceBank.FrontLoadingAmount.GetValueOrDefault();
						bank.ProcessedFrontLoading = true;
						newAccrualAmount += bank.FrontLoadingAmount.GetValueOrDefault();
					}
					if (bank.ProcessedPaidCarryover == true)
					{
						PREarningDetail detail = Earnings.Select().FirstTableItems.SingleOrDefault(x => x.IsPayingCarryover == true && x.TypeCD == bank.EarningTypeCD);
						bank.PaidCarryoverAmount += detail == null ? 0m : detail.Hours.GetValueOrDefault();
						newDisbursementAmount += bank.PaidCarryoverAmount.GetValueOrDefault();
					}
					if (sourceBank.AccrualLimit != null && newAccrualAmount + bank.AccumulatedAmount.GetValueOrDefault() > sourceBank.AccrualLimit)
					{
						decimal diff = sourceBank.AccrualLimit.GetValueOrDefault() - bank.AccumulatedAmount.GetValueOrDefault()
							- bank.FrontLoadingAmount.GetValueOrDefault() - bank.CarryoverAmount.GetValueOrDefault();
						bank.AccrualAmount = Math.Max(diff, 0m);
					}
				}

				//Calculate Disbursement Amount
				var disbursmentEarnings = earningDetails.Where(x => x.TypeCD == bank.EarningTypeCD && x.IsPayingCarryover == false);
				if (disbursmentEarnings.Any())
				{
					bank.DisbursementAmount = disbursmentEarnings.Sum(x => x.Hours.GetValueOrDefault());
					newDisbursementAmount += bank.DisbursementAmount.GetValueOrDefault();
					decimal carryoverAmount = 0;
					if (PTOHelper.IsFirstRegularPaycheckOfYear(this, yearSummary.StartDate, yearSummary.EndDate))
					{
						carryoverAmount = PTOHelper.CalculateAmountToCarryover(this, Document.Current.EmployeeID, sourceBank, yearSummary.StartDate, yearSummary.EndDate);
					}

					if (sourceBank.DisburseFromCarryover == true && newDisbursementAmount > bank.AvailableAmount + carryoverAmount)
					{
						throw new PXException(Messages.NotEnoughLastYearCarryover, bank.BankID);
					}
					else if (sourceBank.AllowNegativeBalance == false && newDisbursementAmount > bank.AvailableAmount + newAccrualAmount + carryoverAmount)
					{
						throw new PXException(Messages.NotEnoughPTOAvailable, bank.EarningTypeCD, bank.BankID);
					}
				}
			}

			PaymentPTOBanks.Update(bank);
		}

		public void InsertPaidCarryoverEarnings()
		{
			Earnings.Select().FirstTableItems.Where(x => x.IsPayingCarryover == true).ForEach(x => Earnings.Delete(x));
			foreach (var row in PaymentPTOBanks.Select().FirstTableItems.Where(x => x.ProcessedPaidCarryover == true))
			{
				row.ProcessedPaidCarryover = false;
				PaymentPTOBanks.Update(row);
			}
			if (Document.Current.AutoPayCarryover == false)
			{
				return;
			}

			foreach (IPTOBank bank in GetEmployeeBanks())
			{
				PTOHelper.PTOYearSummary ptoYearSummary = PTOHelper.GetPTOYearSummary(this, Document.Current.TransactionDate.Value, Document.Current.EmployeeID.Value, bank);
				if (ptoYearSummary.ProcessedPaidCarryover == false
					&& Document.Current.DocType == PayrollType.Regular
					&& bank.CarryoverType == CarryoverType.PaidOnTimeLimit
					&& (bank.CarryoverPayMonthLimit.GetValueOrDefault() == 12
						|| ptoYearSummary.StartDate.AddMonths(bank.CarryoverPayMonthLimit.GetValueOrDefault()) <= Document.Current.TransactionDate.Value))
				{
					PRPTOBank ptoBank = PXSelectorAttribute.Select(Caches[bank.GetType()], bank, nameof(bank.BankID)) as PRPTOBank;
					bool isFirstPaycheckOfYear = PTOHelper.IsFirstRegularPaycheckOfYear(this, ptoYearSummary.StartDate, ptoYearSummary.EndDate);

					decimal carryoverAmount = ptoYearSummary.CarryoverAmount.GetValueOrDefault();
					//For 12 months limit, look at last year instead
					if (bank.CarryoverPayMonthLimit.GetValueOrDefault() == 12)
					{
						ptoYearSummary = PTOHelper.GetPTOYearSummary(this, Document.Current.TransactionDate.Value.AddYears(-1), Document.Current.EmployeeID.Value, bank);
						carryoverAmount = ptoYearSummary.CarryoverAmount.GetValueOrDefault();
					}
					//For first paycheck, calculate what carryover would be
					else if (isFirstPaycheckOfYear)
					{
						carryoverAmount = PTOHelper.CalculateAmountToCarryover(this, Document.Current.EmployeeID, bank, ptoYearSummary.StartDate, ptoYearSummary.EndDate);
					}

					decimal usedOnPaycheck = Earnings.Select().FirstTableItems.Where(x => x.TypeCD == ptoBank.EarningTypeCD).Sum(x => x.Hours.GetValueOrDefault());
					decimal carryoverLeftover = carryoverAmount - ptoYearSummary.TotalDecreasedAmount - usedOnPaycheck;
					if (carryoverLeftover > 0)
					{
						var detail = new PREarningDetail();
						detail.TypeCD = ptoBank.EarningTypeCD;
						detail.Hours = carryoverLeftover;
						detail.Date = Document.Current.EndDate;
						detail.IsPayingCarryover = true;
						detail = Earnings.Insert(detail);

						PRPaymentPTOBank row = PaymentPTOBanks.Select().FirstTableItems.SingleOrDefault(x => x.BankID == bank.BankID);
						if (row != null)
						{
							row.ProcessedPaidCarryover = true;
							PaymentPTOBanks.Update(row);
						}
					}
				}
			}
		}

		public static void RevertPaymentOvertimeCalculation(PXGraph graph, PRPayment document, PXView earningDetailView)
		{
			IEnumerable<PREarningDetail> paymentEarningDetails = earningDetailView.SelectMulti()
				.Select(x => (PREarningDetail)(x is PXResult ? ((PXResult)x)[0] : x));
			bool overtimeRecordsExist = false;
			foreach (PREarningDetail overtimeEarningDetail in paymentEarningDetails)
			{
				if (overtimeEarningDetail.BaseOvertimeRecordID == null)
					continue;

				PREarningDetail baseEarningDetail = null;
				foreach (PREarningDetail earningDetail in paymentEarningDetails)
				{
					if (earningDetail.RecordID != overtimeEarningDetail.BaseOvertimeRecordID)
						continue;

					baseEarningDetail = earningDetail;
				}

				if (baseEarningDetail == null)
				{
					earningDetailView.Cache.Delete(overtimeEarningDetail);
					PXTrace.WriteWarning(Messages.InconsistentBaseEarningDetailRecord, overtimeEarningDetail.BaseOvertimeRecordID, overtimeEarningDetail.RecordID);
					continue;
				}

				using (PXTransactionScope transactionScope = new PXTransactionScope())
				{
					if (overtimeEarningDetail.IsFringeRateEarning != true)
					{
						baseEarningDetail.Hours = baseEarningDetail.Hours.GetValueOrDefault() + overtimeEarningDetail.Hours.GetValueOrDefault();
						earningDetailView.Cache.Update(baseEarningDetail);
					}

					earningDetailView.Cache.Delete(overtimeEarningDetail);
					transactionScope.Complete(graph);
				}
				overtimeRecordsExist = true;
			}

			if (overtimeRecordsExist)
			{
				document.Calculated = false;
				graph.Actions.PressSave();
			}
		}

		private void AddUnionDeductions(PREarningDetail row, bool forceActive)
		{
			if (string.IsNullOrEmpty(row?.UnionID))
			{
				return;
			}

			foreach (PRDeductionAndBenefitUnionPackage unionDeduction in EarningUnionDeductions.Select(row.UnionID, row.LabourItemID, row.LabourItemID, row.LabourItemID, row.UnionID))
			{
				AddPackageDeduction(unionDeduction.DeductionAndBenefitCodeID, PaymentDeductionSourceAttribute.Union, forceActive);
			}
		}

		private void AddProjectDeductions(PREarningDetail row, bool forceActive)
		{
			if (row?.ProjectID == null || CurrentEmployee.SelectSingle()?.ExemptFromCertifiedReporting == true)
			{
				return;
			}

			foreach (PRDeductionAndBenefitProjectPackage projectDeduction in EarningProjectDeductions.Select(row.ProjectID, row.LabourItemID, row.LabourItemID, row.LabourItemID, row.ProjectID))
			{
				AddPackageDeduction(projectDeduction.DeductionAndBenefitCodeID, PaymentDeductionSourceAttribute.CertifiedProject, forceActive);
			}
		}

		private void AddPackageDeduction(int? deductCodeID, string source, bool forceActive)
		{
			PRPaymentDeduct existingDeduct = Deductions.Select()
				.SingleOrDefault(x => ((PRPaymentDeduct)x).CodeID == deductCodeID && ((PRPaymentDeduct)x).Source == source);

			if (existingDeduct != null)
			{
				existingDeduct.IsActive = (existingDeduct.IsActive == true) || forceActive;
				Deductions.Update(existingDeduct);
			}
			else
			{
				PRPaymentDeduct deduct = new PRPaymentDeduct();
				deduct.CodeID = deductCodeID;
				deduct.IsActive = true;
				deduct.Source = source;
				Deductions.Insert(deduct);
			}
		}

		private void CheckForNegative<TField>(decimal? newValue, Func<bool> showError = null) where TField : IBqlField
		{
			if ((showError == null || showError()) &&
				newValue < 0 && CurrentDocument.Current.DocType != PayrollType.VoidCheck && CurrentDocument.Current.DocType != PayrollType.Adjustment)
			{
				throw new PXSetPropertyException<TField>(Messages.InvalidNegative, PXErrorLevel.Error);
			}
		}

		private void CheckExistingPaychecksAndBatches<F>(Events.FieldVerifying<F> e, int? employeeID, string payPeriodID) where F : class, IBqlField
		{
			ExistingPayment.Cache.Clear();
			PRPayment existingPayment = GetExistingPayment(employeeID, payPeriodID);
			if (existingPayment != null)
			{
				ExistingPayment.Current.DocType = existingPayment.DocType;
				ExistingPayment.Current.RefNbr = existingPayment.RefNbr;
				ExistingPayment.Current.Message = PXMessages.LocalizeFormat(Messages.EmployeeAlreadyAddedToAnotherPaycheck, existingPayment.PaymentDocAndRef);
				ExistingPayment.AskExt();
				e.NewValue = null;
				e.Cancel = true;
				return;
			}

			ExistingPayrollBatch.Cache.Clear();
			PRBatch existingPayrollBatch = GetExistingPayrollBatch(employeeID, payPeriodID);
			if (existingPayrollBatch != null)
			{
				ExistingPayrollBatch.Current.BatchNbr = existingPayrollBatch.BatchNbr;
				ExistingPayrollBatch.Current.Message = PXMessages.LocalizeFormat(Messages.EmployeeAlreadyAddedToBatch, existingPayrollBatch.BatchNbr);
				ExistingPayrollBatch.AskExt();
				return;
			}
		}

		private PRPayment GetExistingPayment(int? employeeID, string payPeriodID)
		{
			if (Document.Current.DocType != PayrollType.Regular ||
				!string.IsNullOrWhiteSpace(Document.Current.PayBatchNbr) ||
				employeeID == null ||
				string.IsNullOrWhiteSpace(payPeriodID))
			{
				return null;
			}

			PRPayment existingRegularPaymentWithSamePayPeriod =
				SelectFrom<PRPayment>.
					Where<PRPayment.refNbr.IsNotEqual<P.AsString>.
						And<PRPayment.docType.IsEqual<PayrollType.regular>>.
						And<PRPayment.payPeriodID.IsEqual<P.AsString>>.
						And<PRPayment.employeeID.IsEqual<P.AsInt>>.
						And<PRPayment.voided.IsNotEqual<True>>>.View.
					SelectSingleBound(this, null, Document.Current.RefNbr, payPeriodID, employeeID);

			return existingRegularPaymentWithSamePayPeriod;
		}

		private PRBatch GetExistingPayrollBatch(int? employeeID, string payPeriodID)
		{
			if (Document.Current.DocType != PayrollType.Regular ||
				!string.IsNullOrWhiteSpace(Document.Current.PayBatchNbr) ||
				employeeID == null ||
				string.IsNullOrWhiteSpace(payPeriodID))
			{
				return null;
			}

			PRBatch existingRegularBatchWithSamePayPeriod =
				SelectFrom<PRBatch>.
					InnerJoin<PRBatchEmployee>.On<PRBatch.batchNbr.IsEqual<PRBatchEmployee.batchNbr>>.
					Where<PRBatch.open.IsNotEqual<True>.
						And<PRBatch.closed.IsNotEqual<True>.
						And<PRBatch.payrollType.IsEqual<PayrollType.regular>>.
						And<PRBatch.payPeriodID.IsEqual<P.AsString>>.
						And<PRBatchEmployee.employeeID.IsEqual<P.AsInt>>>>.View.
					SelectSingleBound(this, null, payPeriodID, employeeID);

			return existingRegularBatchWithSamePayPeriod;
		}

		private void UpdateWCPremiumRate(PRPaymentWCPremium row)
		{
			if (!string.IsNullOrEmpty(row.WorkCodeID) && row.DeductCodeID != null && CurrentDocument.Current.DocType != PayrollType.VoidCheck)
			{
				PRWorkCompensationBenefitRate rate = new SelectFrom<PRWorkCompensationBenefitRate>
					.InnerJoin<PMWorkCode>.On<PMWorkCode.workCodeID.IsEqual<PRWorkCompensationBenefitRate.workCodeID>>
					.Where<PRWorkCompensationBenefitRate.workCodeID.IsEqual<P.AsString>
						.And<PRWorkCompensationBenefitRate.deductCodeID.IsEqual<P.AsInt>>
						.And<PMWorkCode.isActive.IsEqual<True>>
						.And<PRWorkCompensationBenefitRate.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>
					.OrderBy<PRWorkCompensationBenefitRate.effectiveDate.Desc>.View(this).SelectSingle(row.WorkCodeID, row.DeductCodeID);

				if (rate != null)
				{
					WCPremiums.Cache.SetValueExt<PRPaymentWCPremium.deductionRate>(row, rate.DeductionRate);
					WCPremiums.Cache.SetValueExt<PRPaymentWCPremium.rate>(row, rate.Rate);
				}
			}
		}

		public static void DefaultDescription(PXCache cache, PRPayment payment)
		{
			if (string.IsNullOrWhiteSpace(payment.DocDesc))
			{
				EPEmployee employee = (EPEmployee)PXSelectorAttribute.Select<PRPayment.employeeID>(cache, payment);
				payment.DocDesc = PXMessages.LocalizeFormatNoPrefix(Messages.DefaultPaymentDescriptionFormat, employee.AcctName, PRPayGroupPeriodIDAttribute.FormatForError(payment.PayPeriodID));
			}
		}

		private void SetCostAssignmentType(PRPayment payment)
		{
			PRCalculationEngine.PRCalculationEngineUtils calculationUtils = PXGraph.CreateInstance<PRCalculationEngine.PRCalculationEngineUtils>();

			(bool splitByEarningType, bool splitByLaborItem) = calculationUtils.GetExpenseSplitSettings(
				PRSetup.Cache,
				PRSetup.Current,
				typeof(PRSetup.earningsAcctDefault),
				typeof(PRSetup.earningsSubMask),
				PREarningsAcctSubDefault.MaskEarningType,
				PREarningsAcctSubDefault.MaskLaborItem);
			bool earningsAssignedToProject = PRSetup.Current.ProjectCostAssignment != ProjectCostAssignmentType.NoCostAssigned;
			bool earningsAssignedToLaborItem = splitByLaborItem;
			bool earningsAssignedToEarningType = splitByEarningType;
			CostAssignmentSetting earningSetting = new CostAssignmentSetting(earningsAssignedToProject, earningsAssignedToLaborItem, earningsAssignedToEarningType);

			(splitByEarningType, splitByLaborItem) = calculationUtils.GetExpenseSplitSettings(
				PRSetup.Cache,
				PRSetup.Current,
				typeof(PRSetup.benefitExpenseAcctDefault),
				typeof(PRSetup.benefitExpenseSubMask),
				PRBenefitExpenseAcctSubDefault.MaskEarningType,
				PRBenefitExpenseAcctSubDefault.MaskLaborItem);
			bool benefitsAssignedToProject = PRSetup.Current.ProjectCostAssignment == ProjectCostAssignmentType.WageLaborBurdenAssigned;
			bool benefitsAssignedToLaborItem = splitByLaborItem;
			bool benefitsAssignedToEarningType = splitByEarningType;
			CostAssignmentSetting benefitSetting = new CostAssignmentSetting(benefitsAssignedToProject, benefitsAssignedToLaborItem, benefitsAssignedToEarningType);

			(splitByEarningType, splitByLaborItem) = calculationUtils.GetExpenseSplitSettings(
				PRSetup.Cache,
				PRSetup.Current,
				typeof(PRSetup.taxExpenseAcctDefault),
				typeof(PRSetup.taxExpenseSubMask),
				PRTaxExpenseAcctSubDefault.MaskEarningType,
				PRTaxExpenseAcctSubDefault.MaskLaborItem);
			bool taxesAssignedToProject = PRSetup.Current.ProjectCostAssignment == ProjectCostAssignmentType.WageLaborBurdenAssigned;
			bool taxesAssignedToLaborItem = splitByLaborItem;
			bool taxesAssignedToEarningType = splitByEarningType;
			CostAssignmentSetting taxSetting = new CostAssignmentSetting(taxesAssignedToProject, taxesAssignedToLaborItem, taxesAssignedToEarningType);

			payment.LaborCostSplitType = CostAssignmentType.GetCode(earningSetting, benefitSetting, taxSetting);
			Document.Update(payment);
		}

		private void AdjustDeductionSummary(int? codeID)
		{
			PRPaymentDeduct paymentDeduct = Deductions.Select().FirstTableItems.FirstOrDefault(x => x.CodeID == codeID);
			decimal? detailTotalAmount = DeductionDetails.Select().FirstTableItems.Where(x => x.CodeID == codeID).Sum(x => x.Amount);
			if (detailTotalAmount != paymentDeduct?.DedAmount)
			{
				paymentDeduct = paymentDeduct ??
					new PRPaymentDeduct()
					{
						CodeID = codeID
					};
				paymentDeduct.IsActive = true;
				if (string.IsNullOrEmpty(paymentDeduct.Source) || paymentDeduct.Source == DeductionSourceListAttribute.EmployeeSettings)
				{
					paymentDeduct.SaveOverride = true;
				}
				paymentDeduct.DedAmount = detailTotalAmount;
				Deductions.Update(paymentDeduct);
			}
		}

		private void AdjustBenefitSummary(int? codeID)
		{
			PRPaymentDeduct paymentDeduct = Deductions.Select().FirstTableItems.FirstOrDefault(x => x.CodeID == codeID);
			decimal? detailTotalAmount = BenefitDetails.Select().FirstTableItems.Where(x => x.CodeID == codeID).Sum(x => x.Amount);
			if (detailTotalAmount != paymentDeduct?.CntAmount)
			{
				paymentDeduct = paymentDeduct ??
					new PRPaymentDeduct()
					{
						CodeID = codeID
					};
				paymentDeduct.IsActive = true;
				if (string.IsNullOrEmpty(paymentDeduct.Source) || paymentDeduct.Source == DeductionSourceListAttribute.EmployeeSettings)
				{
					paymentDeduct.SaveOverride = true;
				}
				paymentDeduct.CntAmount = detailTotalAmount;
				Deductions.Update(paymentDeduct);
			}
		}

		private void VerifyDeductionBenefitAndTaxDetails()
		{
			List<PRDeductionDetail> deductionDetails = DeductionDetails.Select().FirstTableItems.ToList();
			List<PRBenefitDetail> benefitDetails = BenefitDetails.Select().FirstTableItems.ToList();
			List<PRPaymentDeduct> benefitSummaries = Deductions.Select().FirstTableItems.ToList();
			Dictionary<int?, PRDeductCode> deductCodes = SelectFrom<PRDeductCode>.View.Select(this).FirstTableItems.ToDictionary(k => k.CodeID, v => v);

			List<PRPaymentProjectPackageDeduct> projectPackages = ProjectPackageDeductions.Select().FirstTableItems.ToList();
			HashSet<int?> projectDeductCodes = new HashSet<int?>();
			projectPackages.ForEach(x => projectDeductCodes.Add(x.DeductCodeID));
			deductionDetails.Where(x => DeductionSourceListAttribute.GetSource(deductCodes[x.CodeID]) == DeductionSourceListAttribute.CertifiedProject)
				.ForEach(x => projectDeductCodes.Add(x.CodeID));
			benefitDetails.Where(x => DeductionSourceListAttribute.GetSource(deductCodes[x.CodeID]) == DeductionSourceListAttribute.CertifiedProject)
				.ForEach(x => projectDeductCodes.Add(x.CodeID));
			projectDeductCodes.ForEach(x => VerifyDeductionAndBenefitDetailsForProject(projectPackages, deductionDetails, benefitDetails, benefitSummaries, deductCodes[x]));

			List<PRPaymentUnionPackageDeduct> unionPackages = UnionPackageDeductions.Select().FirstTableItems.ToList();
			HashSet<int?> unionDeductCodes = new HashSet<int?>();
			unionPackages.ForEach(x => unionDeductCodes.Add(x.DeductCodeID));
			deductionDetails.Where(x => DeductionSourceListAttribute.GetSource(deductCodes[x.CodeID]) == DeductionSourceListAttribute.Union)
				.ForEach(x => unionDeductCodes.Add(x.CodeID));
			benefitDetails.Where(x => DeductionSourceListAttribute.GetSource(deductCodes[x.CodeID]) == DeductionSourceListAttribute.Union)
				.ForEach(x => unionDeductCodes.Add(x.CodeID));
			unionDeductCodes.ForEach(x => VerifyDeductionAndBenefitDetailsForUnion(unionPackages, deductionDetails, benefitDetails, benefitSummaries, deductCodes[x]));

			List<PRPaymentWCPremium> wcPackages = WCPremiums.Select().FirstTableItems.ToList();
			HashSet<int?> wcDeductCodes = new HashSet<int?>();
			wcPackages.ForEach(x => wcDeductCodes.Add(x.DeductCodeID));
			deductionDetails.Where(x => DeductionSourceListAttribute.GetSource(deductCodes[x.CodeID]) == DeductionSourceListAttribute.WorkCode)
				.ForEach(x => wcDeductCodes.Add(x.CodeID));
			benefitDetails.Where(x => DeductionSourceListAttribute.GetSource(deductCodes[x.CodeID]) == DeductionSourceListAttribute.WorkCode)
				.ForEach(x => wcDeductCodes.Add(x.CodeID));
			wcDeductCodes.ForEach(x => VerifyDeductionAndBenefitDetailsForWC(wcPackages, deductionDetails, benefitDetails, x, deductCodes[x].CodeCD));

			List<PRPaymentTax> taxSummaries = Taxes.Select().FirstTableItems.ToList();
			List<PRTaxDetail> taxDetails = TaxDetails.Select().FirstTableItems.ToList();

			HashSet<int?> taxIds = new HashSet<int?>();
			taxSummaries.ForEach(x => taxIds.Add(x.TaxID));
			taxDetails.ForEach(x => taxIds.Add(x.TaxID));
			taxIds.ForEach(x => VerifyTaxDetails(taxSummaries, taxDetails, x, true));
		}

		private void VerifyDeductionAndBenefitDetailsForProject(
			IEnumerable<PRPaymentProjectPackageDeduct> projectPackages,
			List<PRDeductionDetail> deductionDetails,
			IEnumerable<PRBenefitDetail> benefitDetails,
			IEnumerable<PRPaymentDeduct> benefitSummaries,
			PRDeductCode deductCode)
		{
			decimal? packageTotalDeductionAmount = projectPackages.Sum(x => x.DeductCodeID == deductCode.CodeID ? x.DeductionAmount : 0m);
			decimal? detailTotalDeductionAmount = deductionDetails.Sum(x => x.CodeID == deductCode.CodeID ? x.Amount : 0m);
			if (packageTotalDeductionAmount != detailTotalDeductionAmount)
			{
				throw new PXException(Messages.DeductionDetailSumDoesntMatchProject, deductCode.CodeCD, packageTotalDeductionAmount);
			}

			decimal? packageTotalBenefitAmount = projectPackages.Sum(x => x.DeductCodeID == deductCode.CodeID ? x.BenefitAmount : 0m);
			decimal fringeAmountInBenefit = GetFringeAmountInBenefit(deductCode.CodeID).GetValueOrDefault();
			if (deductCode.NoFinancialTransaction == true)
			{
				decimal? summaryBenefitAmount = benefitSummaries.FirstOrDefault(x => x.CodeID == deductCode.CodeID)?.CntAmount;
				if ((packageTotalBenefitAmount + fringeAmountInBenefit) != summaryBenefitAmount)
				{
					throw new PXException(Messages.BenefitSummarySumDoesntMatchProject, deductCode.CodeCD, summaryBenefitAmount, fringeAmountInBenefit);
				}
			}
			else
			{
				decimal? detailBenefitTotalAmount = benefitDetails.Sum(x => x.CodeID == deductCode.CodeID ? x.Amount : 0m);
				if ((packageTotalBenefitAmount + fringeAmountInBenefit) != detailBenefitTotalAmount)
				{
					throw new PXException(Messages.BenefitDetailSumDoesntMatchProject, deductCode.CodeCD, packageTotalBenefitAmount, fringeAmountInBenefit);
				}
			}
		}

		private void VerifyDeductionAndBenefitDetailsForUnion(
			IEnumerable<PRPaymentUnionPackageDeduct> unionPackages,
			List<PRDeductionDetail> deductionDetails,
			IEnumerable<PRBenefitDetail> benefitDetails,
			IEnumerable<PRPaymentDeduct> benefitSummaries,
			PRDeductCode deductCode)
		{
			decimal? packageTotalDeductionAmount = unionPackages.Sum(x => x.DeductCodeID == deductCode.CodeID ? x.DeductionAmount : 0m);
			decimal? detailTotalDeductionAmount = deductionDetails.Sum(x => x.CodeID == deductCode.CodeID ? x.Amount : 0m);
			if (packageTotalDeductionAmount != detailTotalDeductionAmount)
			{
				throw new PXException(Messages.DeductionDetailSumDoesntMatchUnion, deductCode.CodeCD, packageTotalDeductionAmount);
			}

			decimal? packageTotalBenefitAmount = unionPackages.Sum(x => x.DeductCodeID == deductCode.CodeID ? x.BenefitAmount : 0m);
			if (deductCode.NoFinancialTransaction == true)
			{
				decimal? summaryBenefitAmount = benefitSummaries.FirstOrDefault(x => x.CodeID == deductCode.CodeID)?.CntAmount;
				if (packageTotalBenefitAmount != summaryBenefitAmount)
				{
					throw new PXException(Messages.BenefitSummarySumDoesntMatchUnion, deductCode.CodeCD, packageTotalBenefitAmount);
				}
			}
			else
			{
				decimal? detailBenefitTotalAmount = benefitDetails.Sum(x => x.CodeID == deductCode.CodeID ? x.Amount : 0m);
				if (packageTotalBenefitAmount != detailBenefitTotalAmount)
				{
					throw new PXException(Messages.BenefitDetailSumDoesntMatchUnion, deductCode.CodeCD, packageTotalBenefitAmount);
				}
			}
		}

		private void VerifyDeductionAndBenefitDetailsForWC(
			List<PRPaymentWCPremium> wcPackages,
			List<PRDeductionDetail> deductionDetails,
			List<PRBenefitDetail> benefitDetails,
			int? codeID,
			string codeCD)
		{
			decimal? packageTotalDeductionAmount = wcPackages.Sum(x => x.DeductCodeID == codeID ? x.DeductionAmount : 0m);
			decimal? detailTotalDeductionAmount = deductionDetails.Sum(x => x.CodeID == codeID ? x.Amount : 0m);
			if (packageTotalDeductionAmount != detailTotalDeductionAmount)
			{
				throw new PXException(Messages.DeductionDetailSumDoesntMatchWC, codeCD, packageTotalDeductionAmount);
			}

			decimal? packageTotalBenefitAmount = wcPackages.Sum(x => x.DeductCodeID == codeID ? x.Amount : 0m);
			decimal? detailTotalBenefitAmount = benefitDetails.Sum(x => x.CodeID == codeID ? x.Amount : 0m);
			if (packageTotalBenefitAmount != detailTotalBenefitAmount)
			{
				throw new PXException(Messages.DeductionDetailSumDoesntMatchWC, codeCD, packageTotalBenefitAmount);
			}
		}

		private decimal? GetFringeAmountInBenefit(int? deductCodeID)
		{
			return new SelectFrom<PRPaymentFringeBenefit>
				.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PRPaymentFringeBenefit.projectID>>
				.Where<PRPaymentFringeBenefit.docType.IsEqual<PRPayment.docType.FromCurrent>
					.And<PRPaymentFringeBenefit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
					.And<PMProjectExtension.benefitCodeReceivingFringeRate.IsEqual<P.AsInt>>>
				.AggregateTo<Sum<PRPaymentFringeBenefit.fringeAmountInBenefit>>.View(this).SelectSingle(deductCodeID)?.FringeAmountInBenefit;
		}

		private void VerifyTaxDetails(List<PRPaymentTax> taxSummaries, List<PRTaxDetail> taxDetails, int? taxID, bool throwException)
		{
			PRPaymentTax taxSummary = taxSummaries.FirstOrDefault(x => x.TaxID == taxID);
			List<PRTaxDetail> matchingTaxDetails = taxDetails.Where(x => x.TaxID == taxID).ToList();
			decimal? detailTotalAmount = matchingTaxDetails.Sum(x => x.Amount);

			if (detailTotalAmount != (taxSummary?.TaxAmount ?? 0m))
			{
				string taxCD = taxID.ToString();
				if (taxSummary != null)
				{
					taxCD = (PXSelectorAttribute.Select<PRPaymentTax.taxID>(Taxes.Cache, taxSummary) as PRTaxCode).TaxCD;
				}
				else
				{
					PRTaxDetail matchingTaxDetail = matchingTaxDetails.FirstOrDefault();
					if (matchingTaxDetail != null)
					{
						taxCD = (PXSelectorAttribute.Select<PRTaxDetail.taxID>(TaxDetails.Cache, matchingTaxDetail) as PRTaxCode).TaxCD;
					}
				}

				if (throwException)
				{
					throw new PXException(Messages.TaxDetailSumDoesntMatch, taxCD, taxSummary.TaxAmount);
				}
				else
				{
					foreach (PRTaxDetail detail in matchingTaxDetails)
					{
						detail.AmountErrorMessage = PXMessages.LocalizeFormat(Messages.TaxDetailSumDoesntMatch, taxCD, taxSummary.TaxAmount);
						TaxDetails.Cache.Update(detail);
						TaxDetails.View.RequestRefresh();
					}
				}
			}
			else
			{
				foreach (PRTaxDetail detail in matchingTaxDetails)
				{
					detail.AmountErrorMessage = null;
					TaxDetails.Cache.Update(detail);
					TaxDetails.View.RequestRefresh();
				}
			}
		}

		private void VerifyTaxDetails(int? taxID, bool throwException)
		{
			List<PRPaymentTax> taxSummaries = Taxes.Select().FirstTableItems.ToList();
			List<PRTaxDetail> taxDetails = TaxDetails.Select().FirstTableItems.ToList();

			VerifyTaxDetails(taxSummaries, taxDetails, taxID, throwException);
		}

		private void DefaultBenefitExpenseAcctSub(PXCache cache, PRBenefitDetail row)
		{
			if (row.ExpenseAccountID == null)
			{
				cache.SetDefaultExt<PRBenefitDetail.expenseAccountID>(row);
			}

			if (row.ExpenseSubID == null)
			{
				cache.SetDefaultExt<PRBenefitDetail.expenseSubID>(row);
			}
		}

		private void DefaultTaxExpenseAcctSub(PXCache cache, PRTaxDetail row)
		{
			if (row.ExpenseAccountID == null)
			{
				cache.SetDefaultExt<PRTaxDetail.expenseAccountID>(row);
			}

			if (row.ExpenseSubID == null)
			{
				cache.SetDefaultExt<PRTaxDetail.expenseSubID>(row);
			}
		}

		private bool IsCalculateActionEnabled(PRPayment payment)
		{
			return
				payment.DocType != PayrollType.VoidCheck &&
				payment.TransactionDate != null &&
				payment.Hold == false &&
				payment.Released == false &&
				payment.Printed == false &&
				payment.GrossAmount > 0;
		}

		private bool IsReleaseActionEnabled(PRPayment payment, bool updateGL)
		{
			if (payment.Hold != false || payment.Released != false || payment.TransactionDate == null)
				return false;

			if (payment.EmployeeID == null || payment.PaymentMethodID == null || payment.CashAccountID == null)
				return false;

			switch (payment.DocType)
			{
				case PayrollType.VoidCheck:
				case PayrollType.Adjustment:
					return true;
				case PayrollType.Regular:
				case PayrollType.Special:
					{
						if (payment.Printed == true && payment.GrossAmount > 0)
							return true;
						break;
					}
			}

			return !updateGL && (payment.GrossAmount <= 0 || payment.Calculated == true);
		}

		public IEnumerable<IPTOBank> GetEmployeeBanks()
		{
			var queryResult = SelectFrom<PREmployeePTOBank>
				.InnerJoin<PREmployee>.On<PREmployee.bAccountID.IsEqual<PREmployeePTOBank.bAccountID>>
				.LeftJoin<PREmployeeClassPTOBank>.On<PREmployeeClassPTOBank.employeeClassID.IsEqual<PREmployee.employeeClassID>
					.And<PREmployeeClassPTOBank.bankID.IsEqual<PREmployeePTOBank.bankID>>>
				.InnerJoin<PRPTOBank>.On<PRPTOBank.bankID.IsEqual<PREmployeePTOBank.bankID>>
				.Where<PREmployeePTOBank.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>
				.View.Select(this)
				.Select(x => (PXResult<PREmployeePTOBank, PREmployee, PREmployeeClassPTOBank, PRPTOBank>)x);

			return queryResult.ToList().Select(x => PTOHelper.GetSourceBank(x, x, x)).Where(x => x.StartDate <= Document.Current.TransactionDate);
		}

		protected virtual bool GetChildDeductRecordsWithSourceNotMatching(
			out List<PRPaymentDeduct> paymentDeducts,
			out List<PRPaymentProjectPackageDeduct> projectPackages,
			out List<PRPaymentUnionPackageDeduct> unionPackages,
			out List<PRPaymentWCPremium> workCodePackages)
		{
			paymentDeducts = Deductions.Select()
				.Select(x => (PXResult<PRPaymentDeduct, PRDeductCode>)x)
				.Where(x => !DeductionMatchesSource(x, ((PRPaymentDeduct)x).Source))
				.Select(x => (PRPaymentDeduct)x)
				.ToList();
			projectPackages = ProjectPackageDeductions.Select()
				.Select(x => (PXResult<PRPaymentProjectPackageDeduct, PRDeductCode>)x)
				.Where(x => !DeductionMatchesSource(x, DeductionSourceListAttribute.CertifiedProject))
				.Select(x => (PRPaymentProjectPackageDeduct)x)
				.ToList();
			unionPackages = UnionPackageDeductions.Select()
				.Select(x => (PXResult<PRPaymentUnionPackageDeduct, PRDeductCode>)x)
				.Where(x => !DeductionMatchesSource(x, DeductionSourceListAttribute.Union))
				.Select(x => (PRPaymentUnionPackageDeduct)x)
				.ToList();
			workCodePackages = WCPremiums.Select()
				.Select(x => (PXResult<PRPaymentWCPremium, PMWorkCode, PRDeductCode>)x)
				.Where(x => !DeductionMatchesSource(x, DeductionSourceListAttribute.WorkCode))
				.Select(x => (PRPaymentWCPremium)x)
				.ToList();

			return paymentDeducts.Any() || projectPackages.Any() || unionPackages.Any() || workCodePackages.Any();
		}
		protected virtual void DeleteChildDeductRecordsWithSourceNotMatching(
			List<PRPaymentDeduct> paymentDeducts,
			List<PRPaymentProjectPackageDeduct> projectPackages,
			List<PRPaymentUnionPackageDeduct> unionPackages,
			List<PRPaymentWCPremium> workCodePackages)
		{
			HashSet<int?> affectedDeductCodeIds = new HashSet<int?>();

			foreach (PRPaymentDeduct paymentDeduct in paymentDeducts)
			{
				Deductions.Delete(paymentDeduct);
				affectedDeductCodeIds.Add(paymentDeduct.CodeID);
			}

			foreach (PRPaymentProjectPackageDeduct projectPackage in projectPackages)
			{
				ProjectPackageDeductions.Delete(projectPackage);
				affectedDeductCodeIds.Add(projectPackage.DeductCodeID);
			}

			foreach (PRPaymentUnionPackageDeduct unionPackage in unionPackages)
			{
				UnionPackageDeductions.Delete(unionPackage);
				affectedDeductCodeIds.Add(unionPackage.DeductCodeID);
			}

			foreach (PRPaymentWCPremium workCodePackage in workCodePackages)
			{
				WCPremiums.Delete(workCodePackage);
				affectedDeductCodeIds.Add(workCodePackage.DeductCodeID);
			}

			foreach (PRDeductionDetail deductionDetail in DeductionDetails.Select().FirstTableItems.Where(x => affectedDeductCodeIds.Contains(x.CodeID)))
			{
				DeductionDetails.Delete(deductionDetail);
			}

			foreach (PRBenefitDetail benefitDetail in BenefitDetails.Select().FirstTableItems.Where(x => affectedDeductCodeIds.Contains(x.CodeID)))
			{
				BenefitDetails.Delete(benefitDetail);
			}

			Document.Current.Calculated = false;
		}

		protected virtual bool DeductionMatchesSource(PRDeductCode deductCode, string source)
		{
			return DeductionSourceListAttribute.GetSource(deductCode) == source;
		}

		#endregion Helpers

		public virtual void VoidCheckProc(PRPayment doc)
		{
			Clear(PXClearOption.PreserveTimeStamp);
			Document.View.Answer = WebDialogResult.No;

			foreach (PXResult<PRPayment, CurrencyInfo> res in PRPayment_CurrencyInfo.Select(this, doc.DocType, doc.RefNbr))
			{
				doc = res;
				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy(res);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = PXCache<CurrencyInfo>.CreateCopy(CurrencyInfo.Insert(info));

				var payment = new PRPayment
				{
					DocType = PayrollType.VoidCheck,
					RefNbr = doc.RefNbr,
					CuryInfoID = info.CuryInfoID,
				};
				Document.Insert(payment);

				payment = PXCache<PRPayment>.CreateCopy(res);
				payment.DocType = PayrollType.VoidCheck;
				payment.CuryInfoID = info.CuryInfoID;
				payment.CATranID = null;
				payment.Released = false;
				payment.NoteID = Guid.NewGuid();
				//Set original document reference
				payment.OrigDocType = doc.DocType;
				payment.OrigRefNbr = doc.RefNbr;

				Document.Cache.SetDefaultExt<PRPayment.hold>(payment);
				payment.BatchNbr = null;
				payment.TotalEarnings = 0;
				payment.GrossAmount = 0;
				payment.DedAmount = 0;
				payment.TaxAmount = 0;
				payment.PayableBenefitAmount = 0;
				payment.BenefitAmount = 0;
				payment.EmployerTaxAmount = 0;
				payment = Document.Update(payment);

				if (info != null)
				{
					CurrencyInfo b_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PRPayment.curyInfoID>>>>.Select(this);
					b_info.CuryID = info.CuryID;
					b_info.CuryEffDate = info.CuryEffDate;
					b_info.CuryRateTypeID = info.CuryRateTypeID;
					b_info.CuryRate = info.CuryRate;
					b_info.RecipRate = info.RecipRate;
					b_info.CuryMultDiv = info.CuryMultDiv;
					CurrencyInfo.Update(b_info);
				}
			}

			foreach (PREarningDetail earningDetail in SelectFrom<PREarningDetail>.
				Where<PREarningDetail.paymentDocType.IsEqual<P.AsString>.
					And<PREarningDetail.paymentRefNbr.IsEqual<P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr))
			{
				PREarningDetail copy = PXCache<PREarningDetail>.CreateCopy(earningDetail);
				copy.PaymentDocType = PayrollType.VoidCheck;
				copy.Released = false;
				copy.BatchNbr = null;
				copy.Hours = -1 * copy.Hours;
				copy.Units = -1 * copy.Units;
				copy.Amount = -1 * copy.Amount;
				copy = Earnings.Update(copy);
			}

			foreach (PRDeductionDetail deductionDetail in SelectFrom<PRDeductionDetail>.
				Where<PRDeductionDetail.paymentDocType.IsEqual<P.AsString>.
					And<PRDeductionDetail.paymentRefNbr.IsEqual<P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr))
			{
				PRDeductionDetail copy = PXCache<PRDeductionDetail>.CreateCopy(deductionDetail);
				copy.RecordID = null;
				copy.PaymentDocType = PayrollType.VoidCheck;
				copy.Released = false;
				copy.BatchNbr = null;
				copy.Amount = -1 * copy.Amount;

				DeductionDetails.Update(copy);
			}

			foreach (PRBenefitDetail benefitDetail in SelectFrom<PRBenefitDetail>.
				Where<PRBenefitDetail.paymentDocType.IsEqual<P.AsString>.
					And<PRBenefitDetail.paymentRefNbr.IsEqual<P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr))
			{
				PRBenefitDetail copy = PXCache<PRBenefitDetail>.CreateCopy(benefitDetail);
				copy.RecordID = null;
				copy.PaymentDocType = PayrollType.VoidCheck;
				copy.Released = false;
				copy.BatchNbr = null;
				copy.Amount = -1 * copy.Amount;

				BenefitDetails.Update(copy);
			}

			foreach (PRPaymentDeduct deduction in SelectFrom<PRPaymentDeduct>.
				Where<PRPaymentDeduct.docType.IsEqual<P.AsString>.
					And<PRPaymentDeduct.refNbr.IsEqual<P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr))
			{
				PRPaymentDeduct copy = PXCache<PRPaymentDeduct>.CreateCopy(deduction);
				copy.DocType = PayrollType.VoidCheck;
				copy.DedAmount = -1 * copy.DedAmount;
				copy.CntAmount = -1 * copy.CntAmount;

				Deductions.Update(copy);
			}

			foreach (PRPaymentTaxSplit taxSplit in SelectFrom<PRPaymentTaxSplit>.
				Where<PRPaymentTaxSplit.docType.IsEqual<P.AsString>.
					And<PRPaymentTaxSplit.refNbr.IsEqual<P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr))
			{
				PRPaymentTaxSplit copy = PXCache<PRPaymentTaxSplit>.CreateCopy(taxSplit);
				copy.RecordID = null;
				copy.DocType = PayrollType.VoidCheck;
				copy.TaxAmount = -1 * copy.TaxAmount;
				copy.WageBaseAmount = -1 * copy.WageBaseAmount;
				copy.WageBaseHours = -1 * copy.WageBaseHours;
				copy.WageBaseGrossAmt = -1 * copy.WageBaseGrossAmt;
				copy.SubjectCommissionAmount = -1 * copy.SubjectCommissionAmount;

				TaxSplits.Update(copy);
			}

			foreach (PRTaxDetail taxDetail in SelectFrom<PRTaxDetail>.
				Where<PRTaxDetail.paymentDocType.IsEqual<P.AsString>.
					And<PRTaxDetail.paymentRefNbr.IsEqual<P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr))
			{
				PRTaxDetail copy = PXCache<PRTaxDetail>.CreateCopy(taxDetail);
				copy.RecordID = null;
				copy.PaymentDocType = PayrollType.VoidCheck;
				copy.Released = false;
				copy.BatchNbr = null;
				copy.Amount = -1 * copy.Amount;

				TaxDetails.Update(copy);
			}

			foreach (PRPaymentPTOBank ptoBank in SelectFrom<PRPaymentPTOBank>
				.Where<PRPaymentPTOBank.docType.IsEqual<P.AsString>
					.And<PRPaymentPTOBank.refNbr.IsEqual<P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr))
			{
				PRPaymentPTOBank copy = PXCache<PRPaymentPTOBank>.CreateCopy(ptoBank);
				copy.DocType = PayrollType.VoidCheck;
				copy.AccrualAmount = -1 * copy.AccrualAmount;
				copy.DisbursementAmount = -1 * copy.DisbursementAmount;
				copy.FrontLoadingAmount = -1 * copy.FrontLoadingAmount;
				copy.CarryoverAmount = -1 * copy.CarryoverAmount;
				copy.PaidCarryoverAmount = -1 * copy.PaidCarryoverAmount;

				PaymentPTOBanks.Update(copy);
			}

			foreach (PRDirectDepositSplit split in DirectDepositSplits.Select(doc.DocType, doc.RefNbr))
			{
				PRDirectDepositSplit copy = PXCache<PRDirectDepositSplit>.CreateCopy(split);
				copy.DocType = PayrollType.VoidCheck;
				copy.CATranID = null;
				copy.Amount = -1 * copy.Amount;

				DirectDepositSplits.Update(copy);
			}

			foreach (PRPaymentWCPremium premium in SelectFrom<PRPaymentWCPremium>
				.Where<PRPaymentWCPremium.docType.IsEqual<P.AsString>
					.And<PRPaymentWCPremium.refNbr.IsEqual<P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr))
			{
				PRPaymentWCPremium copy = PXCache<PRPaymentWCPremium>.CreateCopy(premium);
				copy.DocType = PayrollType.VoidCheck;
				copy.DeductionAmount = -1 * copy.DeductionAmount;
				copy.Amount = -1 * copy.Amount;
				copy.RegularWageBaseAmount = -1 * copy.RegularWageBaseAmount;
				copy.OvertimeWageBaseAmount = -1 * copy.OvertimeWageBaseAmount;
				copy.WageBaseAmount = -1 * copy.WageBaseAmount;
				copy.RegularWageBaseHours = -1 * copy.RegularWageBaseHours;
				copy.OvertimeWageBaseHours = -1 * copy.OvertimeWageBaseHours;
				copy.WageBaseHours = -1 * copy.WageBaseHours;

				WCPremiums.Update(copy);
			}

			foreach (PRPaymentProjectPackageDeduct packageDeduct in SelectFrom<PRPaymentProjectPackageDeduct>
				.Where<PRPaymentProjectPackageDeduct.docType.IsEqual<P.AsString>
					.And<PRPaymentProjectPackageDeduct.refNbr.IsEqual<P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr))
			{
				PRPaymentProjectPackageDeduct copy = PXCache<PRPaymentProjectPackageDeduct>.CreateCopy(packageDeduct);
				copy.DocType = PayrollType.VoidCheck;
				copy.DeductionAmount = -1 * copy.DeductionAmount;
				copy.BenefitAmount = -1 * copy.BenefitAmount;
				copy.RegularWageBaseAmount = -1 * copy.RegularWageBaseAmount;
				copy.OvertimeWageBaseAmount = -1 * copy.OvertimeWageBaseAmount;
				copy.WageBaseAmount = -1 * copy.WageBaseAmount;
				copy.RegularWageBaseHours = -1 * copy.RegularWageBaseHours;
				copy.OvertimeWageBaseHours = -1 * copy.OvertimeWageBaseHours;
				copy.WageBaseHours = -1 * copy.WageBaseHours;

				ProjectPackageDeductions.Update(copy);
			}

			foreach (PRPaymentUnionPackageDeduct packageDeduct in SelectFrom<PRPaymentUnionPackageDeduct>
				.Where<PRPaymentUnionPackageDeduct.docType.IsEqual<P.AsString>
					.And<PRPaymentUnionPackageDeduct.refNbr.IsEqual<P.AsString>>>.View.Select(this, doc.DocType, doc.RefNbr))
			{
				PRPaymentUnionPackageDeduct copy = PXCache<PRPaymentUnionPackageDeduct>.CreateCopy(packageDeduct);
				copy.DocType = PayrollType.VoidCheck;
				copy.DeductionAmount = -1 * copy.DeductionAmount;
				copy.BenefitAmount = -1 * copy.BenefitAmount;
				copy.RegularWageBaseAmount = -1 * copy.RegularWageBaseAmount;
				copy.OvertimeWageBaseAmount = -1 * copy.OvertimeWageBaseAmount;
				copy.WageBaseAmount = -1 * copy.WageBaseAmount;
				copy.RegularWageBaseHours = -1 * copy.RegularWageBaseHours;
				copy.OvertimeWageBaseHours = -1 * copy.OvertimeWageBaseHours;
				copy.WageBaseHours = -1 * copy.WageBaseHours;

				UnionPackageDeductions.Update(copy);
			}
		}
	}

	public class PRPayment_CurrencyInfo : SelectFrom<PRPayment>.
	InnerJoin<CurrencyInfo>.On<CurrencyInfo.curyInfoID.IsEqual<PRPayment.curyInfoID>>.
	Where<PRPayment.docType.IsEqual<P.AsString>.
		And<PRPayment.refNbr.IsEqual<P.AsString>>>.View
	{
		public PRPayment_CurrencyInfo(PXGraph graph)
			: base(graph)
		{
		}
	}

	//TODO 29-04-2019 Find a way to not show already selected value in selector popup
	public class PRUniqueDeductionCodeSelector : PXSelectorAttribute
	{
		public PRUniqueDeductionCodeSelector(Type type) : base(type)
		{
		}

		public PRUniqueDeductionCodeSelector(Type type, params Type[] fieldList) : base(type, fieldList)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_LookupSelect = _LookupSelect.WhereAnd(typeof(Where<PRDeductCode.isActive.IsEqual<True>
				.Or<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>>));
			CreateView(sender);
		}

		public override void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(cache, e);
			var row = e.Row as PRPaymentDeduct;
			if (row != null)
			{
				var code = (PRDeductCode)SelectFrom<PRDeductCode>.Where<PRDeductCode.codeID.IsEqual<P.AsInt>>.View.Select(cache.Graph, row.CodeID);
				if (code != null)
				{
					e.ReturnValue = code.CodeCD;
				}
			}
		}

		/// <summary>
		/// Overrides PXSelectorAttribute validation that checks if field value exists in the system. 
		/// Instead of using GetRecords() to validate possible values, this is checking from cache.
		/// It also validates that inserted lines doesn't have same CodeID as an existing line.
		/// </summary>
		/// <remark>
		/// A cache.Locate(e.Row) that returns a row with CodeID == null  means we are inserting a new line, otherwise an existing line is being updated.
		/// Validation should only occurs when inserting new lines.
		/// </remark>
		public override void SubstituteKeyFieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null)
			{
				var graph = (PRPayChecksAndAdjustments)cache.Graph;

				PRDeductCode deductionCode;
				if (e.NewValue is string)
				{
					deductionCode = SelectFrom<PRDeductCode>.Where<PRDeductCode.codeCD.IsEqual<P.AsString>>.View.SelectWindowed(graph, 0, 1, e.NewValue);
				}
				else
				{
					deductionCode = SelectFrom<PRDeductCode>.Where<PRDeductCode.codeID.IsEqual<P.AsInt>>.View.SelectWindowed(graph, 0, 1, e.NewValue);
				}
				var locatedRow = (PRPaymentDeduct)cache.Locate(e.Row);
				if (locatedRow != null && locatedRow.CodeID == null && deductionCode != null)
				{
					//Check if there is already another row with same CodeID
					if (graph.Deductions.Select().FirstTableItems.Any(x => x.CodeID == deductionCode.CodeID && x.Source == PaymentDeductionSourceAttribute.EmployeeSettings))
					{
						throw new PXException(Messages.DuplicateDeductionCode);
					}
				}

				if (deductionCode != null)
				{
					e.NewValue = deductionCode.CodeID;
					e.Cancel = true;
					return;
				}
			}

			base.SubstituteKeyFieldUpdating(cache, e);
		}
	}

	[Serializable]
	[PXHidden]
	public class ExistingPayment : IBqlTable
	{
		#region DocType
		public abstract class docType : BqlString.Field<docType> { }
		[PXString(3, IsFixed = true)]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : BqlString.Field<refNbr> { }
		[PXString(15, IsUnicode = true)]
		public string RefNbr { get; set; }
		#endregion
		#region Message
		public abstract class message : BqlString.Field<message> { }
		[PXString]
		public string Message { get; set; }
		#endregion
	}

	[Serializable]
	[PXHidden]
	public class ExistingPayrollBatch : IBqlTable
	{
		#region BatchNbr
		public abstract class batchNbr : BqlString.Field<batchNbr> { }
		[PXString(15, IsUnicode = true)]
		public string BatchNbr { get; set; }
		#endregion
		#region Message
		public abstract class message : BqlString.Field<message> { }
		[PXString]
		public string Message { get; set; }
		#endregion
	}
}