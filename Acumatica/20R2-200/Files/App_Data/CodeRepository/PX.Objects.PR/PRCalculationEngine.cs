using PX.Api.Payroll;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.Objects.PR.Utility;
using PX.Payroll.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		#region Members

		protected PaymentCalculationInfoCollection PaymentsToProcess { get; private set; }

		private List<PRPayment> _PaymentList;
		private bool _IsMassProcess;
		private CalculationResultInfo<PRPayChecksAndAdjustments, PRPaymentDeduct> _CalculationErrors;

		private Lazy<PRCalculationEngineUtils> _CalculationUtils = new Lazy<PRCalculationEngineUtils>(() => PXGraph.CreateInstance<PRCalculationEngineUtils>());

		#endregion

		#region DataView

		public PXSelect<PRPayment> Payments;

		public SelectFrom<PREarningDetail>.
			InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREarningDetail.locationID>>.
			InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>.
			InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>.
			Where<PREarningDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>.
				And<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>>.
				And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.
			OrderBy<PREarningDetail.date.Asc, PREarningDetail.sortingRecordID.Asc, PREarningDetail.rate.Asc>.View PaymentEarningDetails;

		public SelectFrom<PRPaymentOvertimeRule>.
			InnerJoin<PROvertimeRule>.
				On<PRPaymentOvertimeRule.overtimeRuleID.IsEqual<PROvertimeRule.overtimeRuleID>>.
			InnerJoin<EPEarningType>
				.On<EPEarningType.typeCD.IsEqual<PROvertimeRule.disbursingTypeCD>>.
			Where<PRPaymentOvertimeRule.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
				And<PRPaymentOvertimeRule.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>.
				And<PRPaymentOvertimeRule.isActive.IsEqual<True>>.
				And<PROvertimeRule.isActive.IsEqual<True>>>.
			OrderBy<PROvertimeRule.overtimeMultiplier.Desc, PROvertimeRule.dailyThreshold.Asc>.View OvertimeRulesForCalculation;

		public SelectFrom<Address>
			.InnerJoin<EPEmployee>.On<EPEmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>
			.Where<Address.addressID.IsEqual<EPEmployee.defAddressID>>.View CurrentEmployeeResidenceAddress;

		public PXSelectJoin<PRPaymentEarning,
				InnerJoin<EPEarningType,
					On<PRPaymentEarning.typeCD,
						Equal<EPEarningType.typeCD>>,
				InnerJoin<PRLocation,
					On<PRPaymentEarning.locationID,
						Equal<PRLocation.locationID>>,
				InnerJoin<Address,
					On<PRLocation.addressID,
						Equal<Address.addressID>>>>>,
				Where<PRPaymentEarning.docType,
						Equal<Current<PRPayment.docType>>,
					And<PRPaymentEarning.refNbr,
						Equal<Current<PRPayment.refNbr>>>>> Earnings;

		public PXSelectJoin<PRTaxCode,
				LeftJoin<PREarningTypeDetail,
					On<PRTaxCode.taxID,
						Equal<PREarningTypeDetail.taxID>,
					And<PREarningTypeDetail.typecd,
						Equal<Required<PREarningTypeDetail.typecd>>>>>> EarningTaxList;

		public PXSelectJoin<PRPaymentDeduct,
				InnerJoin<PRDeductCode,
					On<PRPaymentDeduct.codeID, Equal<PRDeductCode.codeID>>,
				LeftJoin<PREmployeeDeduct,
					On<PREmployeeDeduct.bAccountID, Equal<Current<PRPayment.employeeID>>,
					And<PREmployeeDeduct.codeID, Equal<PRPaymentDeduct.codeID>>>>>,
				Where<PRPaymentDeduct.docType, Equal<Current<PRPayment.docType>>,
				And<PRPaymentDeduct.refNbr, Equal<Current<PRPayment.refNbr>>,
				And2<Where<PRPaymentDeduct.saveOverride, Equal<True>,
					Or<PRPaymentDeduct.source, NotEqual<PaymentDeductionSourceAttribute.employeeSetting>,
					Or<Where<PREmployeeDeduct.codeID, IsNull,
					Or<Where<PREmployeeDeduct.startDate, LessEqual<Current<PRPayment.transactionDate>>,
						And<Where<PREmployeeDeduct.endDate, GreaterEqual<Current<PRPayment.transactionDate>>,
							Or<PREmployeeDeduct.endDate, IsNull>>>>>>>>>,
				And<PRPaymentDeduct.isActive, Equal<True>>>>>> Deductions;

		public SelectFrom<PRPaymentDeduct>
			.Where<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View AllPaymentDeductions;

		public PXSelectJoin<PRTaxCode,
				LeftJoin<PRDeductCodeDetail,
					On<PRTaxCode.taxID,
						Equal<PRDeductCodeDetail.taxID>,
					And<PRDeductCodeDetail.codeID,
						Equal<Required<PRDeductCodeDetail.codeID>>>>>> DeductionTaxList;

		public SelectFrom<PRDeductionDetail>.
			Where<PRDeductionDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>.
			And<PRDeductionDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
			And<PRDeductionDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>>.View DeductionDetails;

		public SelectFrom<PRBenefitDetail>.
			Where<PRBenefitDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>.
			And<PRBenefitDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
			And<PRBenefitDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>>.View BenefitDetails;

		public SelectFrom<PREmployeeTax>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PREmployeeTax.taxID>>
			.Where<PREmployeeTax.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>
				.And<PREmployeeTax.isActive.IsEqual<True>>>.View TaxSettings;

		public SelectFrom<PRTaxCodeAttribute>
			.InnerJoin<PREmployeeTax>.On<PREmployeeTax.taxID.IsEqual<PRTaxCodeAttribute.taxID>>
			.LeftJoin<PREmployeeTaxAttribute>.On<PREmployeeTaxAttribute.taxID.IsEqual<PRTaxCodeAttribute.taxID>
				.And<PREmployeeTaxAttribute.settingName.IsEqual<PRTaxCodeAttribute.settingName>>
				.And<PREmployeeTaxAttribute.typeName.IsEqual<PRTaxCodeAttribute.typeName>>
				.And<PREmployeeTaxAttribute.bAccountID.IsEqual<PREmployeeTax.bAccountID>>>
			.Where<PRTaxCodeAttribute.taxID.IsEqual<P.AsInt>
				.And<PREmployeeTax.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>>.View TaxSettingAttributes;

		public SelectFrom<PRCompanyTaxAttribute>
			.InnerJoin<PREmployeeTax>.On<PREmployeeTax.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PREmployeeTax.taxID>
				.And<PRTaxCode.taxState.IsEqual<PRCompanyTaxAttribute.state>>
				.Or<PRCompanyTaxAttribute.state.IsEqual<LocationConstants.Federal>>>
			.LeftJoin<PREmployeeAttribute>.On<PREmployeeAttribute.settingName.IsEqual<PRCompanyTaxAttribute.settingName>
				.And<PREmployeeAttribute.typeName.IsEqual<PRCompanyTaxAttribute.typeName>>
				.And<PREmployeeAttribute.bAccountID.IsEqual<PREmployeeTax.bAccountID>>>
			.AggregateTo<GroupBy<PRCompanyTaxAttribute.settingName>>.View EmployeeAttributes;

		public PXSelect<PRPaymentTax,
				Where<PRPaymentTax.docType,
						Equal<Current<PRPayment.docType>>,
					And<PRPaymentTax.refNbr,
						Equal<Current<PRPayment.refNbr>>>>> PaymentTaxes;

		public PXSelect<PRPaymentTaxSplit,
				Where<PRPaymentTaxSplit.docType,
						Equal<Current<PRPayment.docType>>,
					And<PRPaymentTaxSplit.refNbr,
						Equal<Current<PRPayment.refNbr>>,
				And<PRPaymentTaxSplit.taxID,
					Equal<Current<PRPaymentTax.taxID>>>>>> PaymentTaxesSplit;

		public SelectFrom<PRYtdTaxes>
			.Where<PRYtdTaxes.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
			.And<PRYtdTaxes.taxID.IsEqual<P.AsInt>
			.And<PRYtdTaxes.year.IsEqual<P.AsString>>>>.View YTDTaxes;

		public SelectFrom<PRPeriodTaxes>
			.Where<PRPeriodTaxes.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
			.And<PRPeriodTaxes.taxID.IsEqual<P.AsInt>>
			.And<PRPeriodTaxes.year.IsEqual<P.AsString>>>.View PeriodTaxes;

		public SelectFrom<PRTaxDetail>.
			Where<PRTaxDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>.
			And<PRTaxDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
			And<PRTaxDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>>.View TaxDetails;

		public SelectFrom<PRYtdDeductions>
			.Where<PRYtdDeductions.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
			.And<PRYtdDeductions.codeID.IsEqual<P.AsInt>
			.And<PRYtdDeductions.year.IsEqual<P.AsString>>>>.View YtdDeduction;

		public SelectFrom<PRPayGroupPeriod>
			.InnerJoin<PRPayGroupYear>.On<PRPayGroupYear.payGroupID.IsEqual<PRPayGroupPeriod.payGroupID>
				.And<PRPayGroupYear.year.IsEqual<PRPayGroupPeriod.finYear>>>
			.Where<PRPayGroupPeriod.payGroupID.IsEqual<PRPayment.payGroupID.FromCurrent>
			.And<PRPayGroupPeriod.finPeriodID.IsEqual<PRPayment.payPeriodID.FromCurrent>>>.View PayPeriod;

		public SelectFrom<PRPayGroupYear>
			.Where<PRPayGroupYear.payGroupID.IsEqual<PRPayment.payGroupID.FromCurrent>
			.And<PRPayGroupYear.year.IsEqual<P.AsString>>>.View PayrollYear;

		public FringeBenefitApplicableEarningsQuery FringeBenefitApplicableEarnings;

		public SelectFrom<PRProjectFringeBenefitRateReducingDeduct>
			.Where<PRProjectFringeBenefitRateReducingDeduct.projectID.IsEqual<P.AsInt>
				.And<PRProjectFringeBenefitRateReducingDeduct.isActive.IsEqual<True>>>.View FringeBenefitRateReducingDeductions;

		public SelectFrom<PREarningDetail>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PREarningDetail.projectID.IsEqual<P.AsInt>>>
			.AggregateTo<Sum<PREarningDetail.hours>>.View ProjectHours;

		public SelectFrom<PRSetup>.View PayrollPreferences;

		public ProjectDeductionQuery ProjectDeductions;

		public UnionDeductionQuery UnionDeductions;

		public SelectFrom<PREarningDetail>
			.InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREarningDetail.locationID>>
			.InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.state.IsEqual<Address.state>
				.And<PRDeductCode.isWorkersCompensation.IsEqual<True>>>
			.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PREarningDetail.workCodeID.IsNotNull>
				.And<PREarningType.isWCCCalculation.IsEqual<True>>>
			.AggregateTo<GroupBy<PREarningDetail.workCodeID>, GroupBy<Address.state>, GroupBy<EPEarningType.isOvertime>, Sum<PREarningDetail.amount>, Sum<PREarningDetail.hours>>.View WorkCodeEarnings;

		public SelectFrom<PRPaymentWCPremium>
			.Where<PRPaymentWCPremium.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentWCPremium.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View WCPremiums;

		public SelectFrom<PRPaymentProjectPackageDeduct>
			.Where<PRPaymentProjectPackageDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentProjectPackageDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View ProjectPackageDeductions;

		public SelectFrom<PRPaymentUnionPackageDeduct>
			.Where<PRPaymentUnionPackageDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentUnionPackageDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View UnionPackageDeductions;

		public SelectFrom<PRWorkCompensationBenefitRate>
			.InnerJoin<PMWorkCode>.On<PMWorkCode.workCodeID.IsEqual<PRWorkCompensationBenefitRate.workCodeID>>
			.Where<PRWorkCompensationBenefitRate.workCodeID.IsEqual<P.AsString>
				.And<PRWorkCompensationBenefitRate.deductCodeID.IsEqual<P.AsInt>>
				.And<PMWorkCode.isActive.IsEqual<True>>
				.And<PRWorkCompensationBenefitRate.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>
			.OrderBy<PRWorkCompensationBenefitRate.effectiveDate.Desc>.View WorkCodeRate;

		public SelectFrom<PREarningDetail>
			.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PREarningDetail.isFringeRateEarning.IsEqual<True>>>.View CalculatedEarnings;

		public SelectFrom<PRDirectDepositSplit>
			.Where<PRDirectDepositSplit.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRDirectDepositSplit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View DirectDepositSplits;

		public SelectFrom<PREarningDetail>
			.InnerJoin<PRPaymentEarning>.On<PRPaymentEarning.refNbr.IsEqual<PREarningDetail.paymentRefNbr>
				.And<PRPaymentEarning.docType.IsEqual<PREarningDetail.paymentDocType>>
				.And<PRPaymentEarning.locationID.IsEqual<PREarningDetail.locationID>>
				.And<PRPaymentEarning.typeCD.IsEqual<PREarningDetail.typeCD>>>
			.InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREarningDetail.locationID>>
			.InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.Where<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>>
				.And<PREarningDetail.isFringeRateEarning.IsEqual<True>>>
			.AggregateTo<GroupBy<PREarningDetail.typeCD>, GroupBy<PREarningDetail.locationID>, Sum<PREarningDetail.amount>, Sum<PREarningDetail.hours>>.View FringeBenefitCalculatedEarnings;

		public SelectFrom<PRPayment>
			.Where<PRPayment.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
				.And<PRPayment.transactionDate.IsLess<PRPayment.transactionDate.FromCurrent>>
				.And<PRPayment.released.IsNotEqual<True>>>
			.OrderBy<PRPayment.transactionDate.Asc>.View OlderUnreleasedPayments;

		public SelectFrom<PMLaborCostRate>
			.Where<PMLaborCostRate.type.IsEqual<PMLaborCostRateType.certified>
				.And<PMLaborCostRate.projectID.IsEqual<P.AsInt>>
				.And<PMLaborCostRate.inventoryID.IsEqual<P.AsInt>>
				.And<PMLaborCostRate.effectiveDate.IsLessEqual<P.AsDateTime>>
				.And<PMLaborCostRate.taskID.IsEqual<P.AsInt>
					.Or<PMLaborCostRate.taskID.IsNull>>>
			.OrderBy<PMLaborCostRate.taskID.Desc, PMLaborCostRate.effectiveDate.Desc>.View PrevailingWage;

		public SelectFrom<PRPaymentFringeBenefit>
			.Where<PRPaymentFringeBenefit.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeBenefit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View PaymentFringeBenefits;

		public SelectFrom<PRPaymentFringeBenefitDecreasingRate>
			.Where<PRPaymentFringeBenefitDecreasingRate.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeBenefitDecreasingRate.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PRPaymentFringeBenefitDecreasingRate.projectID.IsEqual<PRPaymentFringeBenefit.projectID.FromCurrent>>
				.And<PRPaymentFringeBenefitDecreasingRate.laborItemID.IsEqual<PRPaymentFringeBenefit.laborItemID.FromCurrent>>
				.And<PRPaymentFringeBenefitDecreasingRate.projectTaskID.IsEqual<PRPaymentFringeBenefit.projectTaskID.FromCurrent>
					.Or<PRPaymentFringeBenefitDecreasingRate.projectTaskID.IsNull
						.And<PRPaymentFringeBenefit.projectTaskID.FromCurrent.IsNull>>>>.View PaymentFringeBenefitsDecreasingRate;

		public SelectFrom<PRPaymentFringeEarningDecreasingRate>
			.Where<PRPaymentFringeEarningDecreasingRate.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeEarningDecreasingRate.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PRPaymentFringeEarningDecreasingRate.projectID.IsEqual<PRPaymentFringeBenefit.projectID.FromCurrent>>
				.And<PRPaymentFringeEarningDecreasingRate.laborItemID.IsEqual<PRPaymentFringeBenefit.laborItemID.FromCurrent>>
				.And<PRPaymentFringeEarningDecreasingRate.projectTaskID.IsEqual<PRPaymentFringeBenefit.projectTaskID.FromCurrent>
					.Or<PRPaymentFringeEarningDecreasingRate.projectTaskID.IsNull
						.And<PRPaymentFringeBenefit.projectTaskID.FromCurrent.IsNull>>>>.View PaymentFringeEarningsDecreasingRate;
		#endregion

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PaymentRegularAmount(nameof(PaymentEarningDetails))]
		protected virtual void _(Events.CacheAttached<PRPayment.regularAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(BenExpenseAccountAttribute), nameof(BenExpenseAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRBenefitDetail.expenseAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(BenExpenseSubAccountAttribute), nameof(BenExpenseSubAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRBenefitDetail.expenseSubID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(TaxExpenseAccountAttribute), nameof(TaxExpenseAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRTaxDetail.expenseAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(TaxExpenseSubAccountAttribute), nameof(TaxExpenseSubAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRTaxDetail.expenseSubID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(TaxLiabilityAccountAttribute), nameof(TaxLiabilityAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRTaxDetail.liabilityAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(null, typeof(SumCalc<PRPayment.dedAmount>))]
		protected virtual void _(Events.CacheAttached<PRDeductionDetail.amount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(null, typeof(SumCalc<PRPayment.benefitAmount>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<Selector<PRBenefitDetail.codeID, PRDeductCode.isPayableBenefit>, Equal<True>>, PRBenefitDetail.amount>, decimal0>),
			typeof(SumCalc<PRPayment.payableBenefitAmount>))]
		protected virtual void _(Events.CacheAttached<PRBenefitDetail.amount> e) { }
		#endregion CacheAttached

		#region Events
		public void _(Events.FieldUpdated<PRPaymentTax.taxID> e)
		{
			var row = e.Row as PRPaymentTax;
			if (row == null)
			{
				return;
			}

			UpdateTax(row);
		}
		#endregion Events

		#region Static Accessors

		public static void Run(List<PRPayment> payments, bool isMassProcess = true)
		{
			var calculationEngine = PXGraph.CreateInstance<PRCalculationEngine>();
			calculationEngine._IsMassProcess = isMassProcess;
			calculationEngine._PaymentList = payments;
			calculationEngine.Run();
		}


		public static void CreateTaxDetail(PXGraph graph, PRTaxCode taxCode, Dictionary<int?, PRPaymentTaxSplit> matchingTaxSplits, IEnumerable<PREarningDetail> earnings)
		{
			PXGraph.CreateInstance<PRCalculationEngineUtils>().CreateTaxDetail(graph, taxCode, matchingTaxSplits, earnings, out TaxEarningDetailsSplits _);
		}

		public static void CreateDeductionDetail(PXGraph graph, PXCache deductionDetailViewCache, PRPaymentDeduct deduction, IEnumerable<PREarningDetail> earnings)
		{
			PXGraph.CreateInstance<PRCalculationEngineUtils>().CreateDeductionDetail(graph, deductionDetailViewCache, deduction, earnings);
		}

		public static void CreateBenefitDetail(PXGraph graph, PXCache benefitDetailViewCache, PRPaymentDeduct deduction, IEnumerable<PREarningDetail> earnings)
		{
			PXGraph.CreateInstance<PRCalculationEngineUtils>().CreateBenefitDetail(graph, benefitDetailViewCache, deduction, earnings);
		}

		#endregion

		#region Run

		protected virtual void Run()
		{
			PayrollPreferences.Current = PayrollPreferences.SelectSingle();

			DeleteEmptyEarnings();
			DeleteCalculatedEarningLines();
			AddMissingLocationCodes();
			PaymentsToProcess = ValidateInputs();

			UpdateYTDValues();
			CalculatePaymentOvertimeRules();
			var calculations = CalculatePayroll();
			SavePayrollCalculations(calculations);
			CalculatePostTaxBenefits(calculations);
			CalculateFringeBenefitRates(calculations);
			SetDirectDepositSplit(calculations);
			SetCalculatedStatus(calculations);
		}

		#endregion

		#region Prepare Data
		protected virtual void DeleteEmptyEarnings()
		{
			foreach (PRPayment payment in _PaymentList)
			{
				Payments.Current = payment;
				foreach (PREarningDetail zeroEarning in PaymentEarningDetails.Select().FirstTableItems.Where(x => x.Amount == 0))
				{
					PaymentEarningDetails.Delete(zeroEarning);
				}

				PRPayChecksAndAdjustments.DeleteEmptySummaryEarnings(Earnings.View, PaymentEarningDetails.Cache);
			}
		}

		protected virtual void DeleteCalculatedEarningLines()
		{
			List<PRPayment> updatedPayments = new List<PRPayment>();
			foreach (PRPayment payment in _PaymentList)
			{
				Payments.Current = payment;
				RegularAmountAttribute.EnforceEarningDetailUpdate<PRPayment.regularAmount>(Payments.Cache, Payments.Current, false);
				PRPayChecksAndAdjustments.RevertPaymentOvertimeCalculation(this, payment, PaymentEarningDetails.View);
				CalculatedEarnings.Select().ForEach(x => CalculatedEarnings.Delete(x));
				RegularAmountAttribute.EnforceEarningDetailUpdate<PRPayment.regularAmount>(Payments.Cache, Payments.Current, true);
				updatedPayments.Add(Payments.Current);
			}

			Actions.PressSave();
			_PaymentList = updatedPayments;
		}

		protected void AddMissingLocationCodes()
		{
			IEnumerable<Address> addressesWithoutLocationCode = new List<Address>();
			TaxLocationHelpers.AddressEqualityComparer comparer = new TaxLocationHelpers.AddressEqualityComparer();
			foreach (PRPayment payment in _PaymentList)
			{
				Payments.Current = payment;
				addressesWithoutLocationCode = addressesWithoutLocationCode.Union(
					Earnings.Select().ToList()
						.Select(x => (Address)x[typeof(Address)])
						.Where(x => string.IsNullOrWhiteSpace(x.TaxLocationCode))
						.Distinct(x => x.AddressID),
					comparer);

				PREmployee employee = PXSelectorAttribute.Select<PRPayment.employeeID>(Payments.Cache, payment) as PREmployee;
				Address employeeAddress = (Address)PXSelectorAttribute.Select<EPEmployee.defAddressID>(Caches[typeof(EPEmployee)], employee);
				if (string.IsNullOrWhiteSpace(employeeAddress.TaxLocationCode))
				{
					addressesWithoutLocationCode = addressesWithoutLocationCode.Union(new List<Address> { employeeAddress }, comparer);
				}
			}

			if (addressesWithoutLocationCode.Any())
			{
				try
				{
					TaxLocationHelpers.UpdateAddressLocationCodes(addressesWithoutLocationCode.ToList(), new PayrollTaxClient());
				}
				catch { } // If location update fails, error will be thrown from ValidateInputs()
			}
		}

		//ToDo: Review, which additional checks might be necessary here, AC-146235
		protected virtual PaymentCalculationInfoCollection ValidateInputs()
		{
			HashSet<string> activeEarningTypes = new HashSet<string>(
				SelectFrom<EPEarningType>.
				Where<EPEarningType.isActive.IsEqual<True>>.View.
				Select(this).FirstTableItems.Select(item => item.TypeCD));

			var validPayments = new PaymentCalculationInfoCollection();
			foreach (PRPayment payment in _PaymentList)
			{
				Payments.Current = payment;
				string errorMessage = null;

				PRPayment olderUnreleasedPayment = OlderUnreleasedPayments.Select().FirstOrDefault();
				if (olderUnreleasedPayment != null)
				{
					errorMessage = PXMessages.LocalizeFormat(
						Messages.OlderPaymentIsUnreleased,
						(PXSelectorAttribute.Select<PRPayment.employeeID>(Payments.Cache, payment) as EPEmployee).AcctCD,
						Payments.Cache.GetValueExt<PRPayment.paymentDocAndRef>(olderUnreleasedPayment));
				}
				else
				{
					PREmployee employee = PXSelectorAttribute.Select<PRPayment.employeeID>(Payments.Cache, payment) as PREmployee;
					IEnumerable<PREmployeeClassWorkLocation> employeeClassWorkLocations = null;
					IEnumerable<PREmployeeWorkLocation> employeeWorkLocations = null;
					if (employee.LocationUseDflt == true)
					{
						employeeClassWorkLocations = SelectFrom<PREmployeeClassWorkLocation>
							.Where<PREmployeeClassWorkLocation.employeeClassID.IsEqual<P.AsString>>.View.Select(this, employee.EmployeeClassID).FirstTableItems;
					}
					else
					{
						employeeWorkLocations = SelectFrom<PREmployeeWorkLocation>
							.Where<PREmployeeWorkLocation.employeeID.IsEqual<P.AsInt>>.View.Select(this, employee.BAccountID).FirstTableItems;
					}

					foreach (PXResult<PREarningDetail, PRLocation, Address> result in PaymentEarningDetails.Select())
					{
						PREarningDetail earningDetail = (PREarningDetail)result;
						PRLocation workLocation = (PRLocation)result;
						Address locationAddress = (Address)result;

						if (earningDetail.Amount < 0)
						{
							errorMessage = Messages.CantCalculateNegative;
							break;
						}

						if (earningDetail.Date == null || earningDetail.Date < payment.StartDate || earningDetail.Date > payment.EndDate)
						{
							errorMessage = DateInPeriodAttribute.GetIncorrectDateMessage(payment.StartDate.GetValueOrDefault(), payment.EndDate.GetValueOrDefault());
							break;
						}

						if (!activeEarningTypes.Contains(earningDetail.TypeCD))
						{
							errorMessage = PXMessages.LocalizeFormat(Messages.EarningDetailsWithInactiveEarningTypes, earningDetail.TypeCD);
							break;
						}

						if (string.IsNullOrWhiteSpace(locationAddress.TaxLocationCode))
						{
							errorMessage = PXMessages.LocalizeFormat(Messages.AddressNotRecognized, workLocation.LocationCD);
							break;
						}

						if (employee.LocationUseDflt == true && !employeeClassWorkLocations.Any(x => x.LocationID == workLocation.LocationID))
						{
							errorMessage = PXMessages.LocalizeFormat(Messages.LocationNotSetInEmployeeClass, workLocation.LocationCD, employee.EmployeeClassID);
							break;
						}

						if (employee.LocationUseDflt != true && !employeeWorkLocations.Any(x => x.LocationID == workLocation.LocationID))
						{
							errorMessage = PXMessages.LocalizeFormat(Messages.LocationNotSetInEmployee, workLocation.LocationCD, employee.AcctCD);
							break;
						}
					}

					if (errorMessage == null && SelectFrom<PRPaymentDeduct>
							.Where<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>
								.And<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
								.And<PRPaymentDeduct.isActive.IsEqual<True>>
								.And<PRPaymentDeduct.saveOverride.IsEqual<True>>
								.And<PRPaymentDeduct.dedAmount.IsLess<decimal0>
									.Or<PRPaymentDeduct.cntAmount.IsLess<decimal0>>>>.View.Select(this).Any())
					{

						errorMessage = Messages.CantCalculateNegative;
					}

					if (errorMessage == null)
					{
						Address employeeAddress = (Address)PXSelectorAttribute.Select<EPEmployee.defAddressID>(Caches[typeof(EPEmployee)], employee);
						if (string.IsNullOrWhiteSpace(employeeAddress.TaxLocationCode))
						{
							errorMessage = PXMessages.LocalizeFormat(Messages.AddressNotRecognized, employee.AcctCD);
						}
					}

					if (errorMessage == null &&
						new SelectFrom<PREmployeeTax>.Where<PREmployeeTax.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View(this).SelectSingle() == null)
					{
						errorMessage = Messages.EmployeeHasNoTaxes;
					}
				}

				if (errorMessage != null)
				{
					if (_IsMassProcess)
					{
						PXProcessing<PRPayment>.SetError(_PaymentList.IndexOf(payment), errorMessage);
					}
					else
					{
						throw new PXException(errorMessage);
					}
				}
				else
				{
					validPayments.Add(payment);
				}
			}
			return validPayments;
		}

		protected virtual void UpdateYTDValues()
		{
			foreach (PRPayment payment in PaymentsToProcess.GetAllPayments())
			{
				Payments.Current = payment;

				Earnings.Select().ForEach(x =>
				{
					PRPayChecksAndAdjustments.UpdateSummaryEarning(this, payment, x);
					Earnings.Update(x);
				});
				AllPaymentDeductions.Select().ForEach(x =>
				{
					PRPayChecksAndAdjustments.UpdateSummaryDeductions(this, payment, x);
					AllPaymentDeductions.Update(x);
				});
				PaymentTaxes.Select().ForEach(x => PaymentTaxes.Cache.SetDefaultExt<PRPaymentTax.ytdAmount>(x));
			}

			Actions.PressSave();
		}

		protected IEnumerable<PRPayrollCalculation> CalculatePayroll()
		{
			_CalculationErrors = new CalculationResultInfo<PRPayChecksAndAdjustments, PRPaymentDeduct>();
			PXLongOperation.SetCustomInfo(_CalculationErrors);

			using (var payrollAssemblyScope = new PXPayrollAssemblyScope<PayrollCalculationProxy>())
			{
				foreach (PRPayment payment in PaymentsToProcess.GetAllPayments())
				{
					Payments.Current = payment;
					var payrollBase = CreatePayrollBase(payment);

					List<PRWage> wages = PrepareWageData().ToList();

					var benefits = PrepareTaxableBenefitData(true);
					var refNbr = payrollAssemblyScope
									.Proxy
									.AddPayroll(
										payrollBase,
										wages,
										benefits.ToList());

					PopulateTaxSettingData(refNbr, payrollAssemblyScope);
				}

				return payrollAssemblyScope
							.Proxy
							.CalculatePayroll();
			}
		}

		protected virtual PRPayrollBase CreatePayrollBase(PRPayment payment)
		{
			PXResult<PRPayGroupPeriod, PRPayGroupYear> result = (PXResult<PRPayGroupPeriod, PRPayGroupYear>)PayPeriod.Select();
			PRPayGroupPeriod period = (PRPayGroupPeriod)result;
			PRPayGroupYear year = (PRPayGroupYear)result;

			PXResultset<PRPayGroupPeriod> yearPeriods = SelectFrom<PRPayGroupPeriod>
				.Where<PRPayGroupPeriod.payGroupID.IsEqual<PRPayGroupYear.payGroupID.FromCurrent>
					.And<PRPayGroupPeriod.finYear.IsEqual<PRPayGroupYear.year.FromCurrent>>>
				.OrderBy<PRPayGroupPeriod.transactionDate.Asc, PRPayGroupPeriod.startDate.Asc>.View.SelectMultiBound(this, new object[] { year });

			// Since periods shifted from other years have an alpha value for the PeriodNbr field, order all periods in year and find the
			// position in the resulting list.
			int periodNbr = yearPeriods.FirstTableItems.Select(x => x.FinPeriodID).ToList().IndexOf(period?.FinPeriodID);

			if (period?.FinPeriodID == null || year?.FinPeriods == null || periodNbr == -1)
			{
				throw new PXException(Messages.InvalidPayPeriod);
			}

			var payroll = new PRPayrollBase()
			{
				ReferenceNbr = payment.PaymentDocAndRef,
				PayDate = payment.TransactionDate.GetValueOrDefault(),
				PayPeriodNumber = (short)periodNbr,
				PayPeriodsPerYear = year.FinPeriods.Value
			};

			return payroll;
		}

		#region Earnings / Wage

		protected virtual IEnumerable<PRWage> PrepareWageData()
		{
			foreach (PXResult<PRPaymentEarning, EPEarningType, PRLocation, Address> earning in Earnings.Select())
			{
				yield return CreateWage((PRPaymentEarning)earning, (EPEarningType)earning, (Address)earning);
			}
		}

		protected virtual PRWage CreateWage(PRPaymentEarning paymentEarning, EPEarningType earningType, Address address)
		{
			var prEarningType = PXCache<EPEarningType>.GetExtension<PREarningType>(earningType);

			var wage = prEarningType.IncludeType == SubjectToTaxes.PerTaxEngine
					   ? new PRWage()
					   : CreateCustomWage(earningType.TypeCD, prEarningType.IncludeType);

			wage.Name = paymentEarning.TypeCD;
			wage.LocationCode = address.TaxLocationCode;
			wage.TaxMunicipalCode = address.TaxMunicipalCode;
			wage.TaxSchoolDistrictCode = address.TaxSchoolCode;
			wage.WageType = prEarningType.WageTypeCD.GetValueOrDefault();
			wage.IsCommission = PayrollPreferences.Current.CommissionType == earningType.TypeCD;
			wage.Hours = paymentEarning.Hours.GetValueOrDefault();
			wage.Amount = paymentEarning.Amount.GetValueOrDefault();
			wage.MTDAmount = paymentEarning?.MTDAmount ?? 0;
			wage.QTDAmount = paymentEarning?.QTDAmount ?? 0;
			wage.YTDAmount = paymentEarning?.YTDAmount ?? 0;

			PaymentsToProcess[Payments.Current].NetIncomeAccumulator += wage.Amount;

			return wage;
		}

		protected virtual PRCustomWage CreateCustomWage(string typeCD, string includeType)
		{
			var customWage = new PRCustomWage();
			customWage.NotSubjectToTaxCalculationMethod = SubjectToTaxes.Get(includeType);

			if (customWage.NotSubjectToTaxCalculationMethod == PRCustomItemCalculationMethod.FromList)
			{
				customWage.NotSubjectToTaxUniqueTaxIDs = GetWageTaxList(typeCD, includeType == SubjectToTaxes.AllButList).ToArray();
			}

			return customWage;
		}

		protected virtual IEnumerable<string> GetWageTaxList(string typeCD, bool useReturnList)
		{
			if (useReturnList)
				EarningTaxList.WhereNew<Where<PREarningTypeDetail.typecd, IsNotNull>>();
			else
				EarningTaxList.WhereNew<Where<PREarningTypeDetail.typecd, IsNull>>();

			foreach (PRTaxCode taxCode in EarningTaxList.Select(typeCD))
			{
				yield return taxCode.TaxUniqueCode;
			}
		}

		#endregion

		#region Deductions / Benefits 

		protected virtual IEnumerable<PRBenefit> PrepareTaxableBenefitData(bool firstPass)
		{
			Dictionary<int?, PRBenefit> taxableBenefits = new Dictionary<int?, PRBenefit>();
			Dictionary<string, bool> containsFringePayout = new Dictionary<string, bool>();
			foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> deduction in GetDeductions(true))
			{
				PRDeductCode deductCode = (PRDeductCode)deduction;
				PRBenefit benefit = CreateBenefit(deduction, deductCode, deduction, firstPass);
				taxableBenefits[deductCode.CodeID] = benefit;
				containsFringePayout[deductCode.CodeCD] = ((PRPaymentDeduct)deduction).ContainsFringePayout ?? false;

				DedBenAmount nominal = new DedBenAmount();
				nominal.DeductionAmount = benefit.Percent > 0 ? benefit.Percent * Payments.Current.GrossAmount / 100 : benefit.Amount;
				nominal.BenefitAmount = benefit.EmployerPercent > 0 ? benefit.EmployerPercent * Payments.Current.GrossAmount / 100 : benefit.EmployerAmount;
				// Calculate proportion of benefit that comes from Employee/Benefit setting, project, union and WC so that
				// we can map calculation results later.
				PaymentsToProcess[Payments.Current].NominalTaxableDedBenAmounts[deductCode.CodeID] = nominal;
			}

			return taxableBenefits.Values.OrderBy(x => containsFringePayout[x.CodeCD] ? 0 : 1);
		}

		protected virtual PRBenefit CreateBenefit(
			PRPaymentDeduct paymentDeduction,
			PRDeductCode deductionCode,
			PREmployeeDeduct employeeDeduction,
			bool firstPass)
		{
			bool hasEmployeeDedOverride = employeeDeduction != null && employeeDeduction.DedUseDflt == false;
			bool hasEmployeeCntOverride = employeeDeduction != null && employeeDeduction.CntUseDflt == false;
			bool usePaymentDeductAmounts = paymentDeduction.SaveOverride == true || !firstPass;

			PRBenefit benefit = deductionCode.IncludeType == SubjectToTaxes.PerTaxEngine
				? new PRBenefit()
				: CreateCustomBenefit(deductionCode.CodeID, deductionCode.IncludeType);

			benefit.CodeCD = deductionCode.CodeCD;
			benefit.BenefitType = deductionCode.BenefitTypeCD.GetValueOrDefault();
			benefit.AllowSupplementalElection = false;
			benefit.ProrateUsingStateWages = true;
			benefit.YTDAmount = paymentDeduction.YtdAmount.GetValueOrDefault();
			benefit.EmployerYTDAmount = paymentDeduction.EmployerYtdAmount.GetValueOrDefault();

			string dedMaximumFrequency = GetDedMaxFreqTypeValue(deductionCode, employeeDeduction);
			string cntMaximumFrequency = GetCntMaxFreqTypeValue(deductionCode, employeeDeduction);
			benefit.Limits = null;
			if (!usePaymentDeductAmounts &&
				(dedMaximumFrequency != DeductionMaxFrequencyType.NoMaximum ||
				cntMaximumFrequency != DeductionMaxFrequencyType.NoMaximum))
			{
				benefit.Limits = new PRBenefitLimit()
				{
					MaximumFrequency = DeductionMaxFrequencyType.ToEnum(dedMaximumFrequency),
					MaximumAmount = hasEmployeeDedOverride ? employeeDeduction.DedMaxAmount : deductionCode.DedMaxAmount,
					YtdAmount = paymentDeduction.YtdAmount.GetValueOrDefault(),
					EmployerMaximumFrequency = DeductionMaxFrequencyType.ToEnum(cntMaximumFrequency),
					EmployerMaximumAmount = hasEmployeeCntOverride ? employeeDeduction.CntMaxAmount : deductionCode.CntMaxAmount,
					EmployerYtdAmount = paymentDeduction.EmployerYtdAmount.GetValueOrDefault()
				};
			}

			if (usePaymentDeductAmounts)
			{
				benefit.Amount = paymentDeduction.DedAmount.GetValueOrDefault();
				benefit.EmployerAmount = paymentDeduction.CntAmount.GetValueOrDefault();
				return benefit;
			}
			else
			{
				decimal? employeeAmount = null;
				decimal? employeePercent = null;
				decimal? employerAmount = null;
				decimal? employerPercent = null;

				switch (paymentDeduction.Source)
				{
					case PaymentDeductionSourceAttribute.EmployeeSettings:
						if (deductionCode.DedCalcType == DedCntCalculationMethod.FixedAmount ||
							(deductionCode.DedCalcType == DedCntCalculationMethod.PercentOfGross && deductionCode.DedApplicableEarnings == DedBenApplicableEarningsAttribute.TotalEarnings))
						{
							employeeAmount = hasEmployeeDedOverride ? employeeDeduction.DedAmount : deductionCode.DedAmount;
							employeePercent = hasEmployeeDedOverride ? employeeDeduction.DedPercent : deductionCode.DedPercent;
						}
						else if (deductionCode.DedCalcType == DedCntCalculationMethod.AmountPerHour)
						{
							employeeAmount = (hasEmployeeDedOverride ? employeeDeduction.DedAmount : deductionCode.DedAmount) * GetDedBenApplicableHours(deductionCode, ContributionType.EmployeeDeduction);
						}
						else
						{
							employeeAmount = (hasEmployeeDedOverride ? employeeDeduction.DedPercent : deductionCode.DedPercent) * GetDedBenApplicableAmount(deductionCode, ContributionType.EmployeeDeduction);
						}

						if (deductionCode.CntCalcType == DedCntCalculationMethod.FixedAmount ||
							(deductionCode.CntCalcType == DedCntCalculationMethod.PercentOfGross && deductionCode.CntApplicableEarnings == DedBenApplicableEarningsAttribute.TotalEarnings))
						{
							employerAmount = hasEmployeeCntOverride ? employeeDeduction.CntAmount : deductionCode.CntAmount;
							employerPercent = hasEmployeeDedOverride ? employeeDeduction.CntPercent : deductionCode.CntPercent;
						}
						else if (deductionCode.CntCalcType == DedCntCalculationMethod.AmountPerHour)
						{
							employerAmount = (hasEmployeeCntOverride ? employeeDeduction.CntAmount : deductionCode.CntAmount) * GetDedBenApplicableHours(deductionCode, ContributionType.EmployerContribution);
						}
						else
						{
							employerAmount = (hasEmployeeDedOverride ? employeeDeduction.CntPercent : deductionCode.CntPercent) * GetDedBenApplicableAmount(deductionCode, ContributionType.EmployerContribution);
						}
						break;
					case PaymentDeductionSourceAttribute.CertifiedProject:
						HashSet<(int?, int?, int?)> projectPackageDeductionsApplied = new HashSet<(int?, int?, int?)>();

						foreach (IGrouping<int?, PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType>> resultGroup in ProjectDeductions.Select(deductionCode.CodeID)
							.Select(x => (PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType>)x)
							.GroupBy(x => ((PREarningDetail)x).RecordID))
						{
							PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType> result = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitProjectPackage)x).EffectiveDate).First();
							PREarningDetail earning = result;
							PRDeductionAndBenefitProjectPackage package = result;
							EPEarningType earningType = result;

							PackageDedBenCalculation calculation = new PackageDedBenCalculation(earning, earningType);
							switch (deductionCode.DedCalcType)
							{
								case DedCntCalculationMethod.FixedAmount:
									if (package.DeductionAmount > 0 && !projectPackageDeductionsApplied.Contains((deductionCode.CodeID, package.ProjectID, package.LaborItemID)))
									{
										calculation.DeductionAmount = package.DeductionAmount;
									}
									break;
								case DedCntCalculationMethod.PercentOfGross:
									if (package.DeductionRate > 0)
									{
										calculation.DeductionAmount = package.DeductionRate * GetDedBenApplicableAmount(deductionCode, ContributionType.EmployeeDeduction, earning) / 100;
									}
									break;
								case DedCntCalculationMethod.AmountPerHour:
									if (package.DeductionAmount > 0)
									{
										calculation.DeductionAmount = package.DeductionAmount * GetDedBenApplicableHours(deductionCode, ContributionType.EmployeeDeduction, earning);
									}
									break;
								default:
									throw new PXException(Messages.PercentOfNetInCertifiedProject);
							}

							switch (deductionCode.CntCalcType)
							{
								case DedCntCalculationMethod.FixedAmount:
									if (package.BenefitAmount > 0 && !projectPackageDeductionsApplied.Contains((deductionCode.CodeID, package.ProjectID, package.LaborItemID)))
									{
										calculation.BenefitAmount = package.BenefitAmount;
									}
									break;
								case DedCntCalculationMethod.PercentOfGross:
									if (package.BenefitRate > 0)
									{
										calculation.BenefitAmount = package.BenefitRate * GetDedBenApplicableAmount(deductionCode, ContributionType.EmployerContribution, earning) / 100;
									}
									break;
								case DedCntCalculationMethod.AmountPerHour:
									if (package.BenefitAmount > 0)
									{
										calculation.BenefitAmount = package.BenefitAmount * GetDedBenApplicableHours(deductionCode, ContributionType.EmployerContribution, earning);
									}
									break;
								default:
									throw new PXException(Messages.PercentOfNetInCertifiedProject);
							}

							RecordProjectPackageNominalAmounts(package, calculation);
							employeeAmount = employeeAmount.GetValueOrDefault() + calculation.DeductionAmount;
							employerAmount = employerAmount.GetValueOrDefault() + calculation.BenefitAmount;
							projectPackageDeductionsApplied.Add((deductionCode.CodeID, package.ProjectID, package.LaborItemID));
						}
						break;
					case PaymentDeductionSourceAttribute.Union:
						HashSet<(int?, string, int?)> unionPackageDeductionsApplied = new HashSet<(int?, string, int?)>();

						foreach (IGrouping<int?, PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType>> resultGroup in UnionDeductions.Select(deductionCode.CodeID)
							.Select(x => (PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType>)x)
							.GroupBy(x => ((PREarningDetail)x).RecordID))
						{
							PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType> result = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitUnionPackage)x).EffectiveDate).First();
							PREarningDetail earning = result;
							PRDeductionAndBenefitUnionPackage package = result;
							EPEarningType earningType = result;

							PackageDedBenCalculation calculation = new PackageDedBenCalculation(earning, earningType);
							if (deductionCode.ContribType != ContributionType.EmployerContribution)
							{
								switch (deductionCode.DedCalcType)
								{
									case DedCntCalculationMethod.FixedAmount:
										if (package.DeductionAmount > 0 && !unionPackageDeductionsApplied.Contains((deductionCode.CodeID, package.UnionID, package.LaborItemID)))
										{
											calculation.DeductionAmount = package.DeductionAmount;
										}
										break;
									case DedCntCalculationMethod.PercentOfGross:
										if (package.DeductionRate > 0)
										{
											calculation.DeductionAmount = package.DeductionRate * GetDedBenApplicableAmount(deductionCode, ContributionType.EmployeeDeduction, earning) / 100;
										}
										break;
									case DedCntCalculationMethod.AmountPerHour:
										if (package.DeductionAmount > 0)
										{
											calculation.DeductionAmount = package.DeductionAmount * GetDedBenApplicableHours(deductionCode, ContributionType.EmployeeDeduction, earning);
										}
										break;
									default:
										throw new PXException(Messages.PercentOfNetInUnion);
								}
							}

							if (deductionCode.ContribType != ContributionType.EmployeeDeduction)
							{
								switch (deductionCode.CntCalcType)
								{
									case DedCntCalculationMethod.FixedAmount:
										if (package.BenefitAmount > 0 && !unionPackageDeductionsApplied.Contains((deductionCode.CodeID, package.UnionID, package.LaborItemID)))
										{
											calculation.BenefitAmount = package.BenefitAmount;
										}
										break;
									case DedCntCalculationMethod.PercentOfGross:
										if (package.BenefitRate > 0)
										{
											calculation.BenefitAmount = package.BenefitRate * GetDedBenApplicableAmount(deductionCode, ContributionType.EmployerContribution, earning) / 100;
										}
										break;
									case DedCntCalculationMethod.AmountPerHour:
										if (package.BenefitAmount > 0)
										{
											calculation.BenefitAmount = package.BenefitAmount * GetDedBenApplicableHours(deductionCode, ContributionType.EmployerContribution, earning);
										}
										break;
									default:
										throw new PXException(Messages.PercentOfNetInUnion);
								}
							}

							RecordUnionPackageNominalAmounts(package, calculation);
							employeeAmount = employeeAmount.GetValueOrDefault() + calculation.DeductionAmount;
							employerAmount = employerAmount.GetValueOrDefault() + calculation.BenefitAmount;
							unionPackageDeductionsApplied.Add((deductionCode.CodeID, package.UnionID, package.LaborItemID));
						}
						break;
				}

				employeeAmount = employeeAmount.HasValue ? Math.Max(employeeAmount.Value, 0m) : employeeAmount;
				employerAmount = employerAmount.HasValue ? Math.Max(employerAmount.Value, 0m) : employerAmount;
				benefit.Amount = employeeAmount.GetValueOrDefault();
				benefit.Percent = employeePercent;
				benefit.EmployerAmount = employerAmount.GetValueOrDefault();
				benefit.EmployerPercent = employerPercent;

				return benefit;
			}
		}

		protected virtual PRCustomBenefit CreateCustomBenefit(int? codeID, string includeType)
		{
			var customBenefit = new PRCustomBenefit();
			customBenefit.PreTaxCalculationMethod = SubjectToTaxes.Get(includeType);

			if (customBenefit.PreTaxCalculationMethod == PRCustomItemCalculationMethod.FromList)
			{
				customBenefit.PreTaxUniqueTaxIDs = GetBenefitTaxList(codeID, includeType == SubjectToTaxes.AllButList).ToArray();
			}

			return customBenefit;
		}

		protected virtual IEnumerable<string> GetBenefitTaxList(int? codeID, bool useReturnList)
		{
			if (useReturnList)
				DeductionTaxList.WhereNew<Where<PRDeductCodeDetail.codeID, IsNotNull>>();
			else
				DeductionTaxList.WhereNew<Where<PRDeductCodeDetail.codeID, IsNull>>();

			foreach (PRTaxCode taxCode in DeductionTaxList.Select(codeID))
			{
				yield return taxCode.TaxUniqueCode;
			}
		}

		#endregion

		#region Tax Settings

		protected virtual void PopulateTaxSettingData(string referenceNbr, PXPayrollAssemblyScope<PayrollCalculationProxy> payrollAssemblyScope)
		{
			foreach (PXResult<PREmployeeTax, PRTaxCode> taxEmployeeCompanySetting in TaxSettings.Select())
			{
				var employeeTaxSetting = (PREmployeeTax)taxEmployeeCompanySetting;
				var companyTaxSetting = (PRTaxCode)taxEmployeeCompanySetting;
				Dictionary<string, string> taxSettingDictionary = CreateTaxSettingDictionary(companyTaxSetting.TaxID);

				PRYtdTaxes ytdRecord = YTDTaxes.SelectSingle(employeeTaxSetting.TaxID, Payments.Current.TransactionDate.Value.Year);
				IEnumerable<PRPeriodTaxes> periodTaxes = PeriodTaxes.Select(employeeTaxSetting.TaxID, Payments.Current.TransactionDate.Value.Year).Select(x => (PRPeriodTaxes)x);
				PRPayGroupYear payYear = PayrollYear.SelectSingle(Payments.Current.TransactionDate.Value.Year.ToString());
				PRPayGroupPeriod period = PayPeriod.SelectSingle();
				DayOfWeek payWeekStart = payYear.StartDate.Value.DayOfWeek == DayOfWeek.Saturday ? DayOfWeek.Sunday : payYear.StartDate.Value.DayOfWeek + 1;
				int weekOfYear = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(Payments.Current.TransactionDate.Value, CalendarWeekRule.FirstDay, payWeekStart);
				int[] quarterMonths = PRDateTime.GetQuarterMonths(Payments.Current.TransactionDate.Value);

				decimal periodAmount = periodTaxes.FirstOrDefault(x => x.PeriodNbr == period?.PeriodNbrAsInt)?.Amount ?? 0;
				decimal wtdAmount = periodTaxes.Where(x => x.Week == weekOfYear).Sum(x => x?.Amount ?? 0);
				decimal mtdAmount = periodTaxes.Where(x => x.Month == Payments.Current.TransactionDate.Value.Month).Sum(x => x.Amount ?? 0);
				decimal qtdAmount = periodTaxes.Join(quarterMonths, result => result.Month, month => month, (result, month) => result).Sum(result => result.Amount ?? 0);

				payrollAssemblyScope.Proxy.AddTaxSetting(
					referenceNbr,
					companyTaxSetting.TaxUniqueCode,
					periodAmount,
					wtdAmount,
					mtdAmount,
					qtdAmount,
					ytdRecord?.Amount ?? 0m,
					ytdRecord?.TaxableWages ?? 0m,
					ytdRecord?.MostRecentWH ?? 0m,
					companyTaxSetting,
					taxSettingDictionary);
			}

			SetEmployeeSettings(referenceNbr, payrollAssemblyScope);
		}

		protected virtual Dictionary<string, string> CreateTaxSettingDictionary(int? taxID)
		{
			var settingDictionary = new Dictionary<string, string>();

			foreach (PXResult<PRTaxCodeAttribute, PREmployeeTax, PREmployeeTaxAttribute> taxSettingAttribute in TaxSettingAttributes.Select(taxID))
			{
				var companyTaxSettingAttribute = (PRTaxCodeAttribute)taxSettingAttribute;
				var employeeTaxSettingAttribute = (PREmployeeTaxAttribute)taxSettingAttribute;

				string value = employeeTaxSettingAttribute?.TaxID == null || employeeTaxSettingAttribute?.UseDefault == true || companyTaxSettingAttribute.AllowOverride != true ?
					companyTaxSettingAttribute.Value :
					employeeTaxSettingAttribute?.Value;
				if (string.IsNullOrEmpty(value) && companyTaxSettingAttribute.Required == true)
				{
					throw new PXException(Messages.RequiredTaxSettingNullInCalculate);
				}

				settingDictionary.Add(companyTaxSettingAttribute.SettingName, value);
			}

			return settingDictionary;
		}

		protected virtual void SetEmployeeSettings(string referenceNbr, PXPayrollAssemblyScope<PayrollCalculationProxy> payrollAssemblyScope)
		{
			// Reflective mapping is by type/state, so group attributes by those two fields and create one reflective mapper for each grouping. 
			foreach (var attributeGroup in EmployeeAttributes.Select()
				.Select(x => (PXResult<PRCompanyTaxAttribute, PREmployeeTax, PRTaxCode, PREmployeeAttribute>)x)
				.GroupBy(x => new { ((PRCompanyTaxAttribute)x).State, ((PRCompanyTaxAttribute)x).TypeName }))
			{
				var settingDictionary = new Dictionary<string, string>();
				PREmployeeSettingMapper reflectiveMapper = null;

				foreach (PXResult<PRCompanyTaxAttribute, PREmployeeTax, PRTaxCode, PREmployeeAttribute> taxAttribute in attributeGroup)
				{
					var companyAttribute = (PRCompanyTaxAttribute)taxAttribute;
					var employeeAttribute = (PREmployeeAttribute)taxAttribute;

					string value = employeeAttribute?.BAccountID == null || employeeAttribute?.UseDefault == true || companyAttribute.AllowOverride != true ?
						companyAttribute.Value :
						employeeAttribute?.Value;
					if (string.IsNullOrEmpty(value) && companyAttribute.Required == true)
					{
						throw new PXException(Messages.RequiredTaxSettingNullInCalculate);
					}

					settingDictionary.Add(companyAttribute.SettingName, value);

					if (reflectiveMapper == null)
					{
						reflectiveMapper = new PREmployeeSettingMapper(companyAttribute, companyAttribute);
					}
				}

				payrollAssemblyScope.Proxy.AddEmployeeSetting(referenceNbr, reflectiveMapper, settingDictionary);
			}

			payrollAssemblyScope.Proxy.SetEmployeeResidenceLocationCode(
				referenceNbr,
				CurrentEmployeeResidenceAddress.SelectSingle()?.TaxLocationCode);
		}

		#endregion

		#endregion

		#region Calculations

		protected virtual void SavePayrollCalculations(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			foreach (var payrollCalculation in payrollCalculations)
			{
				Payments.Current = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;

				SaveTaxCalculations(payrollCalculation);
				SaveTaxableBenefitCalculations(payrollCalculation);

				PaymentsToProcess.UpdatePayment(Payments.Current);
			}

			this.Actions.PressSave();
		}

		protected virtual void SetDirectDepositSplit(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			foreach (var payrollCalculation in payrollCalculations)
			{
				PRPayment foundPayment = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
				PRPayment payment = SelectFrom<PRPayment>
									.Where<PRPayment.docType.IsEqual<P.AsString>
									.And<PRPayment.refNbr.IsEqual<P.AsString>>>
									.View.Select(this, foundPayment.DocType, foundPayment.RefNbr);

				SetDirectDepositSplit(payment);
				PaymentsToProcess.UpdatePayment(Payments.Current);
			}

			Actions.PressSave();
		}

		public virtual void SetDirectDepositSplit(PRPayment payment)
		{
			Payments.Current = payment;
			DirectDepositSplits.Select().ForEach(x => DirectDepositSplits.Delete(x));
			if (payment.NetAmount > 0)
			{
				var paymentMethod = (PaymentMethod)PXSelectorAttribute.Select<PRPayment.paymentMethodID>(Payments.Cache, payment);
				var paymentMethodExt = paymentMethod.GetExtension<PRxPaymentMethod>();
				if (paymentMethodExt.PRPrintChecks == false)
				{
					PRDirectDepositSplit remainderRow = null;
					bool anyBankAccounts = false;
					decimal total = 0m;
					foreach (PREmployeeDirectDeposit employeeDDRow in SelectFrom<PREmployeeDirectDeposit>
						.Where<PREmployeeDirectDeposit.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>
						.OrderBy<Asc<PREmployeeDirectDeposit.sortOrder>>.View.Select(this))
					{
						anyBankAccounts = true;
						var split = new PRDirectDepositSplit();
						split.LineNbr = employeeDDRow.LineNbr;
						split.BankAcctNbr = employeeDDRow.BankAcctNbr;
						split.BankRoutingNbr = employeeDDRow.BankRoutingNbr;
						split.BankAcctType = employeeDDRow.BankAcctType;
						split.BankName = employeeDDRow.BankName;

						if (employeeDDRow.Amount != null)
						{
							split.Amount = employeeDDRow.Amount;
						}
						else if (employeeDDRow.Percent != null)
						{
							split.Amount = (employeeDDRow.Percent / 100) * payment.NetAmount;
						}

						if (total + split.Amount > payment.NetAmount)
						{
							split.Amount = payment.NetAmount - total;
						}
						split = DirectDepositSplits.Insert(split);
						total += split.Amount ?? 0m;

						if (employeeDDRow.GetsRemainder == true)
						{
							remainderRow = split;
						}
					}

					if (!anyBankAccounts)
					{
						throw new PXException(Messages.NoBankAccountForDirectDeposit);
					}

					var remainingAmount = payment.NetAmount - total;
					if (remainingAmount > 0 && remainderRow != null)
					{
						remainderRow.Amount += remainingAmount;
						remainderRow = DirectDepositSplits.Update(remainderRow);
					}
				}
			}
		}

		protected virtual void SaveTaxCalculations(PRPayrollCalculation payrollCalculation)
		{
			var taxSettings = TaxSettings.Select()
				.Select(x => (PXResult<PREmployeeTax, PRTaxCode>)x)
				.ToDictionary(k => ((PRTaxCode)k).TaxUniqueCode, v => (PRTaxCode)v);

			foreach (var taxCalculation in payrollCalculation.TaxCalculations)
			{
				var paymentTax = GetPaymentTax(taxCalculation, taxSettings);
			}
		}

		protected virtual PRPaymentTax GetPaymentTax(
			PRTaxCalculation taxCalculation,
			Dictionary<string, PRTaxCode> taxSettings)
		{
			PRTaxCode taxSetting;
			if (taxSettings.TryGetValue(taxCalculation.TaxCode, out taxSetting))
			{
				PRPaymentTax paymentTax = new SelectFrom<PRPaymentTax>
					.Where<PRPaymentTax.docType.IsEqual<PRPayment.docType.FromCurrent>
						.And<PRPaymentTax.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
						.And<PRPaymentTax.taxID.IsEqual<P.AsInt>>>.View(this).SelectSingle(taxSetting.TaxID);
				if (paymentTax == null)
				{
					paymentTax = PaymentTaxes.Update(new PRPaymentTax { TaxID = taxSetting.TaxID });
				}
				PaymentTaxes.Current = paymentTax;

				Dictionary<int?, PRPaymentTaxSplit> paymentTaxSplits = SelectFrom<PRPaymentTaxSplit>
					.Where<PRPaymentTaxSplit.docType.IsEqual<PRPayment.docType.FromCurrent>
						.And<PRPaymentTaxSplit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
						.And<PRPaymentTaxSplit.taxID.IsEqual<PRPaymentTax.taxID.FromCurrent>>>.View.Select(this).FirstTableItems.ToDictionary(k => k.WageType, v => v);
				UpdatePaymentTaxSplits(taxCalculation.Details, paymentTaxSplits);
				_CalculationUtils.Value.CreateTaxDetail(this, taxSetting, paymentTaxSplits, PaymentEarningDetails.Select().FirstTableItems, out TaxEarningDetailsSplits splitTaxAmounts);

				paymentTax.TaxCategory = taxSetting.TaxCategory;
				PaymentTaxes.Update(paymentTax);
				PaymentsToProcess[Payments.Current].TaxesSplitByEarning[paymentTax.TaxID] = splitTaxAmounts;

				if (paymentTax.TaxCategory == TaxCategory.EmployeeWithholding && paymentTax.TaxAmount != 0)
				{
					PaymentsToProcess[Payments.Current].NetIncomeAccumulator -= paymentTax.TaxAmount.Value;
				}

				return paymentTax;
			}
			else
			{
				throw new PXException(Messages.TaxCodeNotSetUp, taxCalculation.TaxCode);
			}

		}

		protected virtual void UpdatePaymentTaxSplits(IEnumerable<PRTaxCalculationDetail> taxCalculationDetails, Dictionary<int?, PRPaymentTaxSplit> taxSplits)
		{
			foreach (IGrouping<int, PRTaxCalculationDetail> split in taxCalculationDetails.GroupBy(x => x.WageType))
			{
				PRPaymentTaxSplit record;
				if (!taxSplits.TryGetValue(split.Key, out record))
				{
					record = PaymentTaxesSplit.Insert(new PRPaymentTaxSplit());
					record.WageType = split.Key;
					record.TaxAmount = 0m;
					record.WageBaseAmount = 0m;
					record.WageBaseHours = 0m;
					record.WageBaseGrossAmt = 0m;
					record.SubjectCommissionAmount = 0m;
				}

				record.TaxAmount = record.TaxAmount.GetValueOrDefault() + split.Sum(x => x.Amount);
				record.WageBaseAmount = record.WageBaseAmount.GetValueOrDefault() + split.Sum(x => x.SubjectWageAmount);
				record.WageBaseHours = record.WageBaseHours.GetValueOrDefault() + split.Sum(x => x.SubjectHours);
				record.WageBaseGrossAmt = record.WageBaseGrossAmt.GetValueOrDefault() + split.Sum(x => x.GrossSubjectWageAmount);
				record.SubjectCommissionAmount = record.SubjectCommissionAmount.GetValueOrDefault() + split.Sum(x => x.SubjectCommissionAmount);

				if (record.TaxAmount > 0)
				{
					PaymentTaxesSplit.Update(record);
					taxSplits[split.Key] = record;
				}
			}
		}

		protected virtual void SaveTaxableBenefitCalculations(PRPayrollCalculation payrollCalculation)
		{
			foreach (PRBenefitCalculation benefitCalculation in payrollCalculation.BenefitCalculations)
			{
				PaymentsToProcess[Payments.Current].NetIncomeAccumulator = Math.Max(PaymentsToProcess[Payments.Current].NetIncomeAccumulator - benefitCalculation.CalculatedAmount, 0m);

				PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> result = GetDeductions(null, benefitCalculation.CodeCD).FirstOrDefault();
				if (result != null)
				{
					PRPaymentDeduct paymentDeduct = result;
					paymentDeduct.DedAmount = benefitCalculation.CalculatedAmount;
					paymentDeduct.CntAmount = benefitCalculation.EmployerCalculatedAmount;

					ValidateBenefitsFromTaxEngine(paymentDeduct, result, result, benefitCalculation);
					Deductions.Update(result);

					List<PREarningDetail> paymentEarnings = PaymentEarningDetails.Select().FirstTableItems.ToList();
					PaymentsToProcess[Payments.Current].TaxableDeductionsAndBenefitsSplitByEarning[paymentDeduct.CodeID] =
						_CalculationUtils.Value.SplitDedBenAmountsPerEarning(this, paymentDeduct, paymentEarnings);
				}
			}
		}

		protected virtual void ValidateBenefitsFromTaxEngine(PRPaymentDeduct paymentDeduct, PRDeductCode deductCode, PREmployeeDeduct employeeDeduct, PRBenefitCalculation calculationResult)
		{
			PREmployee employee = SelectFrom<PREmployee>.Where<PREmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View
				.SelectSingleBound(this, new object[] { Payments.Current });
			decimal minNetIncome = employee?.NetPayMin ?? 0m;
			if (PaymentsToProcess[Payments.Current].NetIncomeAccumulator < minNetIncome && Payments.Current.DocType != PayrollType.Adjustment)
			{
				_CalculationErrors.AddError<PRPaymentDeduct.dedAmount>(
									paymentDeduct,
									paymentDeduct.DedAmount,
									Messages.DeductionCausesNetPayBelowMin);
			}

			if (Math.Abs(PaymentsToProcess[Payments.Current].NominalTaxableDedBenAmounts[deductCode.CodeID].DeductionAmount.GetValueOrDefault() - calculationResult.CalculatedAmount) > 0.01m)
			{
				_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
					paymentDeduct,
					paymentDeduct.DedAmount,
					Messages.DeductionAdjustedByTaxEngine);
			}

			bool hasEmployeeDedOverride = employeeDeduct != null && employeeDeduct.DedUseDflt == false;
			string dedMaximumFrequency = GetDedMaxFreqTypeValue(deductCode, employeeDeduct);
			if (dedMaximumFrequency == DeductionMaxFrequencyType.PerCalendarYear)
			{
				decimal? maximumAmount = hasEmployeeDedOverride ? employeeDeduct.DedMaxAmount : deductCode.DedMaxAmount;
				decimal? newYtd = paymentDeduct.YtdAmount.GetValueOrDefault() + calculationResult.CalculatedAmount;
				if (newYtd >= maximumAmount)
				{
					_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
						paymentDeduct,
						paymentDeduct.DedAmount,
						Messages.DeductionMaxLimitExceededWarn, Messages.PerCalendarYear, newYtd > maximumAmount ? Messages.Exceeded : Messages.Reached);
				}
			}
			else if (dedMaximumFrequency == DeductionMaxFrequencyType.PerPayPeriod)
			{
				decimal? maximumAmount = hasEmployeeDedOverride ? employeeDeduct.DedMaxAmount : deductCode.DedMaxAmount;
				if (calculationResult.CalculatedAmount >= maximumAmount)
				{
					_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
						paymentDeduct,
						paymentDeduct.DedAmount,
						Messages.DeductionMaxLimitExceededWarn, Messages.PerCalendarYear, calculationResult.CalculatedAmount > maximumAmount ? Messages.Exceeded : Messages.Reached);
				}
			}

			if (Math.Abs(PaymentsToProcess[Payments.Current].NominalTaxableDedBenAmounts[deductCode.CodeID].BenefitAmount.GetValueOrDefault() - calculationResult.EmployerCalculatedAmount) > 0.01m)
			{
				_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
					paymentDeduct,
					paymentDeduct.CntAmount,
					Messages.BenefitAdjustedByTaxEngine);
			}

			bool hasEmployeeCntOverride = employeeDeduct != null && employeeDeduct.CntUseDflt == false;
			string cntMaximumFrequency = GetCntMaxFreqTypeValue(deductCode, employeeDeduct);
			if (cntMaximumFrequency == DeductionMaxFrequencyType.PerCalendarYear)
			{
				decimal? maximumAmount = hasEmployeeCntOverride ? employeeDeduct.CntMaxAmount : deductCode.CntMaxAmount;
				decimal? newYtd = paymentDeduct.EmployerYtdAmount.GetValueOrDefault() + calculationResult.EmployerCalculatedAmount;
				if (newYtd >= maximumAmount)
				{
					_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
						paymentDeduct,
						paymentDeduct.CntAmount,
						Messages.BenefitMaxLimitExceededWarn, Messages.PerCalendarYear, newYtd > maximumAmount ? Messages.Exceeded : Messages.Reached);
				}
			}
			else if (cntMaximumFrequency == DeductionMaxFrequencyType.PerPayPeriod)
			{
				decimal? maximumAmount = hasEmployeeCntOverride ? employeeDeduct.CntMaxAmount : deductCode.CntMaxAmount;
				if (calculationResult.EmployerCalculatedAmount >= maximumAmount)
				{
					_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
						paymentDeduct,
						paymentDeduct.CntAmount,
						Messages.BenefitMaxLimitExceededWarn, Messages.PerCalendarYear, calculationResult.EmployerCalculatedAmount > maximumAmount ? Messages.Exceeded : Messages.Reached);
				}
			}
		}

		protected virtual void CalculatePostTaxBenefits(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			foreach (var payrollCalculation in payrollCalculations)
			{
				Payments.Current = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
				CalculatePostTaxBenefitsForCurrent();
				CreateDeductionDetails();
				CreateBenefitDetails();
				InsertProjectPackageDetails();
				InsertUnionPackageDetails();
				PaymentsToProcess.UpdatePayment(Payments.Current);
			}

			this.Actions.PressSave();
		}

		protected virtual void CalculatePostTaxBenefitsForCurrent()
		{
			PREmployee employee = SelectFrom<PREmployee>.Where<PREmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View
				.SelectSingleBound(this, new object[] { Payments.Current });
			decimal minNetIncome = employee?.NetPayMin ?? 0m;
			string deductionSplitMethod = employee?.DedSplitType;
			decimal maxPercentOfNetForGarnishments = employee?.GrnMaxPctNet ?? 100m;

			decimal garnishmentTotal;
			CalculateProjectUnionPostTaxBenefits(minNetIncome, deductionSplitMethod, maxPercentOfNetForGarnishments, out garnishmentTotal);
			CalculateWorkersCompensation(minNetIncome);

			var deductionSequences = SeparateDeductionsBySequence();

			// Calculate deductions by sequence, respecting minNetIncome and maxPercentOfNetForGarnishments
			var nominalAmounts = new Dictionary<int, (decimal deductionAmount, decimal contributionAmount)>();
			foreach (var sequence in deductionSequences)
			{
				// Initial pass to determine whether deductions exceed limits
				decimal sequenceDeductionTotal = 0m;
				decimal sequenceGarnishmentTotal = 0m;
				int garnishmentsInSequence = 0;
				foreach (var deduction in sequence.Value)
				{
					(decimal deductionAmount, decimal contributionAmount) nominal = CalculateRegularBenefitAmount(
							(PRDeductCode)deduction,
							(PREmployeeDeduct)deduction,
							(PRPaymentDeduct)deduction);
					nominalAmounts[((PRDeductCode)deduction).CodeID.Value] = nominal;
					sequenceDeductionTotal += nominal.deductionAmount;
					if (((PRDeductCode)deduction).IsGarnishment == true)
					{
						sequenceGarnishmentTotal += nominal.deductionAmount;
						garnishmentsInSequence++;
					}
				}

				// Second pass to actually set deductions and benefits
				bool limitDeductions = PaymentsToProcess[Payments.Current].NetIncomeAccumulator - sequenceDeductionTotal <= minNetIncome;
				decimal deductionAmountRemaining = Math.Max(PaymentsToProcess[Payments.Current].NetIncomeAccumulator - minNetIncome, 0m);
				bool limitGarnishments = garnishmentTotal + sequenceGarnishmentTotal > PaymentsToProcess[Payments.Current].NetIncomeAccumulator * maxPercentOfNetForGarnishments / 100;
				decimal garnishmentAmountRemaining = Math.Max(PaymentsToProcess[Payments.Current].NetIncomeAccumulator * (1 - maxPercentOfNetForGarnishments / 100) * maxPercentOfNetForGarnishments / 100 - garnishmentTotal, 0m);
				foreach (var deduction in sequence.Value)
				{
					bool isGarnishment = ((PRDeductCode)deduction).IsGarnishment == true;
					decimal adjustedAmount = nominalAmounts[((PRDeductCode)deduction).CodeID.Value].deductionAmount;
					if (limitDeductions && adjustedAmount > 0 && Payments.Current.DocType != PayrollType.Adjustment)
					{
						adjustedAmount = AdjustBenefitAmountForSequenceSplit(
							adjustedAmount,
							deductionAmountRemaining,
							sequenceDeductionTotal,
							sequence.Value.Count,
							(PRPaymentDeduct)deduction,
							deductionSplitMethod,
							Messages.DeductionCausesNetPayBelowMin,
							Messages.DeductionAdjustedForNetPayMin);
					}

					if (isGarnishment && limitGarnishments && adjustedAmount > 0 && Payments.Current.DocType != PayrollType.Adjustment)
					{
						adjustedAmount = AdjustBenefitAmountForSequenceSplit(
							adjustedAmount,
							garnishmentAmountRemaining,
							sequenceGarnishmentTotal,
							garnishmentsInSequence,
							(PRPaymentDeduct)deduction,
							deductionSplitMethod,
							Messages.GarnishmentCausesPercentOfNetAboveMax,
							Messages.GarnishmentAdjustedForPercentOfNetMax);
					}

					((PRPaymentDeduct)deduction).DedAmount = adjustedAmount;
					PaymentsToProcess[Payments.Current].NetIncomeAccumulator = Math.Max(PaymentsToProcess[Payments.Current].NetIncomeAccumulator - adjustedAmount, 0m);
					if (isGarnishment)
					{
						garnishmentTotal += adjustedAmount;
					}

					((PRPaymentDeduct)deduction).CntAmount = nominalAmounts[((PRDeductCode)deduction).CodeID.Value].contributionAmount;
				}
			}

			// All calculations are done, perform update.
			foreach (var sequence in deductionSequences)
			{
				foreach (var deduction in sequence.Value)
				{
					Deductions.Update(deduction);
				}
			}
		}

		protected virtual void CalculateProjectUnionPostTaxBenefits(
			decimal minNetIncome,
			string deductionSplitMethod,
			decimal maxPercentOfNetForGarnishments,
			out decimal garnishmentAdjustedTotal)
		{
			IEnumerable<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>> dedResults = GetDeductions(false);
			decimal deductionTotal = 0m;
			decimal garnishmentTotal = 0m;

			foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> result in dedResults)
			{
				var paymentDeduct = (PRPaymentDeduct)result;
				var deductCode = (PRDeductCode)result;
				var employeeDeduct = (PREmployeeDeduct)result;
				if (paymentDeduct.Source != PaymentDeductionSourceAttribute.EmployeeSettings)
				{
					(paymentDeduct.DedAmount, paymentDeduct.CntAmount) = CalculateRegularBenefitAmount(
						deductCode,
						employeeDeduct,
						paymentDeduct);

					deductionTotal += paymentDeduct.DedAmount ?? 0m;
					if (deductCode.IsGarnishment == true)
					{
						garnishmentTotal += paymentDeduct.DedAmount ?? 0m;
					}
				}
			}

			bool limitDeductions = PaymentsToProcess[Payments.Current].NetIncomeAccumulator - deductionTotal <= minNetIncome;
			decimal deductionAmountRemaining = Math.Max(PaymentsToProcess[Payments.Current].NetIncomeAccumulator - minNetIncome, 0m);
			bool limitGarnishments = garnishmentTotal > PaymentsToProcess[Payments.Current].NetIncomeAccumulator * maxPercentOfNetForGarnishments / 100;
			decimal garnishmentAmountRemaining = PaymentsToProcess[Payments.Current].NetIncomeAccumulator * (1 - maxPercentOfNetForGarnishments / 100) * maxPercentOfNetForGarnishments / 100;
			var amountsPerCode = new Dictionary<int, (decimal deductionAmount, decimal contributionAmount)>();
			garnishmentAdjustedTotal = 0m;
			foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> dedResult in dedResults)
			{
				PRDeductCode deductCode = (PRDeductCode)dedResult;
				PRPaymentDeduct paymentDeduct = (PRPaymentDeduct)dedResult;

				bool isGarnishment = deductCode.IsGarnishment == true;
				if (limitDeductions && paymentDeduct.DedAmount > 0 && Payments.Current.DocType != PayrollType.Adjustment)
				{
					paymentDeduct.DedAmount = AdjustBenefitAmountForSequenceSplit(
						paymentDeduct.DedAmount.Value,
						deductionAmountRemaining,
						deductionTotal,
						dedResults.Count(),
						paymentDeduct,
						deductionSplitMethod,
						Messages.DeductionCausesNetPayBelowMin,
						Messages.DeductionAdjustedForNetPayMin);
				}

				if (isGarnishment && limitGarnishments && paymentDeduct.DedAmount > 0 && Payments.Current.DocType != PayrollType.Adjustment)
				{
					paymentDeduct.DedAmount = AdjustBenefitAmountForSequenceSplit(
						paymentDeduct.DedAmount.Value,
						garnishmentAmountRemaining,
						garnishmentTotal,
						dedResults.Count(x => ((PRDeductCode)x).IsGarnishment == true),
						paymentDeduct,
						deductionSplitMethod,
						Messages.GarnishmentCausesPercentOfNetAboveMax,
						Messages.GarnishmentAdjustedForPercentOfNetMax);
				}

				PaymentsToProcess[Payments.Current].NetIncomeAccumulator = Math.Max(PaymentsToProcess[Payments.Current].NetIncomeAccumulator - (paymentDeduct.DedAmount ?? 0m), 0m);

				if (isGarnishment)
				{
					garnishmentAdjustedTotal += paymentDeduct.DedAmount.Value;
				}

				Deductions.Update(paymentDeduct);
			}
		}

		protected virtual (decimal deductionAmount, decimal contributionAmount) CalculateRegularBenefitAmount(
			PRDeductCode deductCode,
			PREmployeeDeduct employeeDeduct,
			PRPaymentDeduct paymentDeduct)
		{

			(decimal deductionCalculatedAmount, decimal contributionCalculatedAmount) =
				CalculateRegularBenefitNominalAmount(deductCode, employeeDeduct, paymentDeduct);


			(deductionCalculatedAmount, contributionCalculatedAmount) = AdjustSingleBenefitAmount(
				deductionCalculatedAmount,
				contributionCalculatedAmount,
				deductCode,
				employeeDeduct,
				paymentDeduct);

			return (deductionCalculatedAmount, contributionCalculatedAmount);
		}

		protected virtual (decimal deductionNominalAmout, decimal benefitNominalAmout) CalculateRegularBenefitNominalAmount(
			PRDeductCode deductCode,
			PREmployeeDeduct employeeDeduct,
			PRPaymentDeduct paymentDeduct)
		{
			decimal deductionCalculatedAmount = 0m;
			decimal contributionCalculatedAmount = 0m;
			if (paymentDeduct.SaveOverride == true)
			{
				deductionCalculatedAmount = paymentDeduct.DedAmount ?? 0m;
				contributionCalculatedAmount = paymentDeduct.CntAmount ?? 0m;
			}
			else if (paymentDeduct.Source == PaymentDeductionSourceAttribute.EmployeeSettings)
			{
				if (deductCode.ContribType == ContributionType.BothDeductionAndContribution || deductCode.ContribType == ContributionType.EmployeeDeduction)
				{
					decimal? deductionAmount = employeeDeduct != null && employeeDeduct.DedUseDflt == false ?
								employeeDeduct.DedAmount :
								deductCode.DedAmount;
					decimal? deductionPercent = employeeDeduct != null && employeeDeduct.DedUseDflt == false ?
						employeeDeduct.DedPercent :
						deductCode.DedPercent;
					switch (deductCode.DedCalcType)
					{
						case DedCntCalculationMethod.FixedAmount:
							if (deductionAmount == null)
							{
								throw new PXException(Messages.DedCalculationErrorFormat, Messages.FixedAmount);
							}

							deductionCalculatedAmount = (decimal)deductionAmount;
							break;
						case DedCntCalculationMethod.AmountPerHour:
							if (deductionAmount == null)
							{
								throw new PXException(Messages.DedCalculationErrorFormat, Messages.AmountPerHour);
							}

							deductionCalculatedAmount = (decimal)deductionAmount * GetDedBenApplicableHours(deductCode, ContributionType.EmployeeDeduction);
							break;
						case DedCntCalculationMethod.PercentOfGross:
						case DedCntCalculationMethod.PercentOfCustom:
							if (deductionPercent == null)
							{
								throw new PXException(Messages.DedCalculationErrorFormat, new DedCntCalculationMethod.ListAttribute().ValueLabelDic[deductCode.DedCalcType]);
							}

							deductionCalculatedAmount = (decimal)deductionPercent * GetDedBenApplicableAmount(deductCode, ContributionType.EmployeeDeduction) / 100;
							break;
						case DedCntCalculationMethod.PercentOfNet:
							if (deductionPercent == null)
							{
								throw new PXException(Messages.DedCalculationErrorFormat, Messages.PercentOfNet);
							}

							deductionCalculatedAmount = (decimal)deductionPercent * PaymentsToProcess[Payments.Current].NetIncomeAccumulator / 100;
							break;
					}
				}

				if (deductCode.ContribType == ContributionType.BothDeductionAndContribution || deductCode.ContribType == ContributionType.EmployerContribution)
				{
					decimal? contributionAmount = employeeDeduct != null && employeeDeduct.CntUseDflt == false ?
								employeeDeduct.CntAmount :
								deductCode.CntAmount;
					decimal? contributionPercent = employeeDeduct != null && employeeDeduct.CntUseDflt == false ?
						employeeDeduct.CntPercent :
						deductCode.CntPercent;
					switch (deductCode.CntCalcType)
					{
						case DedCntCalculationMethod.FixedAmount:
							if (contributionAmount == null)
							{
								throw new PXException(Messages.CntCalculationErrorFormat, Messages.FixedAmount);
							}

							contributionCalculatedAmount = (decimal)contributionAmount;
							break;
						case DedCntCalculationMethod.AmountPerHour:
							if (contributionAmount == null)
							{
								throw new PXException(Messages.CntCalculationErrorFormat, Messages.AmountPerHour);
							}

							contributionCalculatedAmount = (decimal)contributionAmount * GetDedBenApplicableHours(deductCode, ContributionType.EmployerContribution);
							break;
						case DedCntCalculationMethod.PercentOfGross:
						case DedCntCalculationMethod.PercentOfCustom:
							if (contributionPercent == null)
							{
								throw new PXException(Messages.CntCalculationErrorFormat, new DedCntCalculationMethod.ListAttribute().ValueLabelDic[deductCode.CntCalcType]);
							}

							contributionCalculatedAmount = (decimal)contributionPercent * GetDedBenApplicableAmount(deductCode, ContributionType.EmployerContribution) / 100;
							break;
						case DedCntCalculationMethod.PercentOfNet:
							if (contributionPercent == null)
							{
								throw new PXException(Messages.CntCalculationErrorFormat, Messages.PercentOfNet);
							}

							contributionCalculatedAmount = (decimal)contributionPercent * PaymentsToProcess[Payments.Current].NetIncomeAccumulator / 100;
							break;
					}
				}
			}
			else
			{
				switch (paymentDeduct.Source)
				{
					case PaymentDeductionSourceAttribute.CertifiedProject:
						HashSet<(int?, int?, int?)> projectPackageDeductionsApplied = new HashSet<(int?, int?, int?)>();
						foreach (IGrouping<int?, PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType>> resultGroup in ProjectDeductions.Select(deductCode.CodeID)
							.Select(x => (PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType>)x)
							.GroupBy(x => ((PREarningDetail)x).RecordID))
						{
							PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType> result = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitProjectPackage)x).EffectiveDate).First();
							PREarningDetail earning = result;
							PRDeductionAndBenefitProjectPackage package = result;
							EPEarningType earningType = result;

							PackageDedBenCalculation calculation = new PackageDedBenCalculation(earning, earningType);
							switch (deductCode.DedCalcType)
							{
								case DedCntCalculationMethod.FixedAmount:
									if (!projectPackageDeductionsApplied.Contains((deductCode.CodeID, package.ProjectID, package.LaborItemID)))
									{
										calculation.DeductionAmount = package.DeductionAmount.GetValueOrDefault();
									}
									break;
								case DedCntCalculationMethod.PercentOfGross:
								case DedCntCalculationMethod.PercentOfCustom:
									decimal earningApplicableAmount = GetDedBenApplicableAmount(deductCode, ContributionType.EmployeeDeduction, earning);
									calculation.DeductionAmount = (package.DeductionRate * earningApplicableAmount / 100).GetValueOrDefault();
									break;
								case DedCntCalculationMethod.AmountPerHour:
									decimal earningApplicableHours = GetDedBenApplicableHours(deductCode, ContributionType.EmployeeDeduction, earning);
									calculation.DeductionAmount = (package.DeductionAmount * earningApplicableHours).GetValueOrDefault();
									break;
								default:
									throw new PXException(Messages.PercentOfNetInCertifiedProject);
							}

							switch (deductCode.CntCalcType)
							{
								case DedCntCalculationMethod.FixedAmount:
									if (!projectPackageDeductionsApplied.Contains((deductCode.CodeID, package.ProjectID, package.LaborItemID)))
									{
										calculation.BenefitAmount = package.BenefitAmount.GetValueOrDefault();
									}
									break;
								case DedCntCalculationMethod.PercentOfGross:
								case DedCntCalculationMethod.PercentOfCustom:
									decimal earningApplicableAmount = GetDedBenApplicableAmount(deductCode, ContributionType.EmployerContribution, earning);
									calculation.BenefitAmount = (package.BenefitRate * earningApplicableAmount / 100).GetValueOrDefault();
									break;
								case DedCntCalculationMethod.AmountPerHour:
									decimal earningApplicableHours = GetDedBenApplicableHours(deductCode, ContributionType.EmployerContribution, earning);
									calculation.BenefitAmount = (package.BenefitAmount * earningApplicableHours).GetValueOrDefault();
									break;
								default:
									throw new PXException(Messages.PercentOfNetInCertifiedProject);
							}

							RecordProjectPackageNominalAmounts(package, calculation);
							deductionCalculatedAmount += calculation.DeductionAmount.GetValueOrDefault();
							contributionCalculatedAmount += calculation.BenefitAmount.GetValueOrDefault();
							projectPackageDeductionsApplied.Add((deductCode.CodeID, package.ProjectID, package.LaborItemID));
						}
						break;
					case PaymentDeductionSourceAttribute.Union:
						HashSet<(int?, string, int?)> unionPackageDeductionsApplied = new HashSet<(int?, string, int?)>();
						foreach (IGrouping<int?, PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType>> resultGroup in UnionDeductions.Select(deductCode.CodeID)
							.Select(x => (PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType>)x)
							.GroupBy(x => ((PREarningDetail)x).RecordID))
						{
							PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType> result = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitUnionPackage)x).EffectiveDate).First();
							PREarningDetail earning = result;
							PRDeductionAndBenefitUnionPackage package = result;
							EPEarningType earningType = result;

							PackageDedBenCalculation calculation = new PackageDedBenCalculation(earning, earningType);
							if (deductCode.ContribType != ContributionType.EmployerContribution)
							{
								switch (deductCode.DedCalcType)
								{
									case DedCntCalculationMethod.FixedAmount:
										if (!unionPackageDeductionsApplied.Contains((deductCode.CodeID, package.UnionID, package.LaborItemID)))
										{
											calculation.DeductionAmount = package.DeductionAmount.GetValueOrDefault();
										}
										break;
									case DedCntCalculationMethod.PercentOfGross:
									case DedCntCalculationMethod.PercentOfCustom:
										decimal earningApplicableAmount = GetDedBenApplicableAmount(deductCode, ContributionType.EmployeeDeduction, earning);
										calculation.DeductionAmount = (package.DeductionRate * earningApplicableAmount / 100).GetValueOrDefault();
										break;
									case DedCntCalculationMethod.AmountPerHour:
										decimal earningApplicableHours = GetDedBenApplicableHours(deductCode, ContributionType.EmployeeDeduction, earning);
										calculation.DeductionAmount = (package.DeductionAmount * earningApplicableHours).GetValueOrDefault();
										break;
									default:
										throw new PXException(Messages.PercentOfNetInUnion);
								}
							}

							if (deductCode.ContribType != ContributionType.EmployeeDeduction)
							{
								switch (deductCode.CntCalcType)
								{
									case DedCntCalculationMethod.FixedAmount:
										if (!unionPackageDeductionsApplied.Contains((deductCode.CodeID, package.UnionID, package.LaborItemID)))
										{
											calculation.BenefitAmount = package.BenefitAmount.GetValueOrDefault();
										}
										break;
									case DedCntCalculationMethod.PercentOfGross:
									case DedCntCalculationMethod.PercentOfCustom:
										decimal earningApplicableAmount = GetDedBenApplicableAmount(deductCode, ContributionType.EmployerContribution, earning);
										calculation.BenefitAmount = (package.BenefitRate * earningApplicableAmount / 100).GetValueOrDefault();
										break;
									case DedCntCalculationMethod.AmountPerHour:
										decimal earningApplicableHours = GetDedBenApplicableHours(deductCode, ContributionType.EmployerContribution, earning);
										calculation.BenefitAmount = (package.BenefitAmount * earning.Hours).GetValueOrDefault();
										break;
									default:
										throw new PXException(Messages.PercentOfNetInUnion);
								}
							}

							RecordUnionPackageNominalAmounts(package, calculation);
							deductionCalculatedAmount += calculation.DeductionAmount.GetValueOrDefault();
							contributionCalculatedAmount += calculation.BenefitAmount.GetValueOrDefault();
							unionPackageDeductionsApplied.Add((deductCode.CodeID, package.UnionID, package.LaborItemID));
						}
						break;
				}
			}

			return (Math.Max(deductionCalculatedAmount, 0), Math.Max(contributionCalculatedAmount, 0));
		}

		protected virtual (decimal deductionAmout, decimal benefitAmout) AdjustSingleBenefitAmount(
			decimal deductionCalculatedAmount,
			decimal contributionCalculatedAmount,
			PRDeductCode deductCode,
			PREmployeeDeduct employeeDeduct,
			PRPaymentDeduct paymentDeduct)
		{
			// Lazy load
			PRYtdDeductions ytdTally = null;

			string dedMaximumFrequency = GetDedMaxFreqTypeValue(deductCode, employeeDeduct);
			if (dedMaximumFrequency != DeductionMaxFrequencyType.NoMaximum)
			{
				decimal? maxAmount = employeeDeduct != null && employeeDeduct.DedUseDflt == false ?
					employeeDeduct.DedMaxAmount :
					deductCode.DedMaxAmount;
				if (maxAmount != null)
				{
					if (dedMaximumFrequency == DeductionMaxFrequencyType.PerPayPeriod && deductionCalculatedAmount > maxAmount)
					{
						if (paymentDeduct.SaveOverride == true)
						{
							// Leave deduction amount as is but generate a warning that period maximum has been exceeded
							_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
								paymentDeduct,
								paymentDeduct.DedAmount,
								Messages.DeductionMaxLimitExceededWarn, Messages.PerPayPeriod, Messages.Exceeded);
						}
						else
						{
							// Adjust deduction amount to respect maximum and generate a warning
							deductionCalculatedAmount = Math.Max(maxAmount.Value, 0m);
							_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
								paymentDeduct,
								paymentDeduct.DedAmount,
								Messages.DeductionMaxLimitExceededWarn, Messages.PerPayPeriod, Messages.Reached);
						}
					}
					else if (dedMaximumFrequency == DeductionMaxFrequencyType.PerCalendarYear)
					{
						decimal ytdAmount = 0m;
						ytdTally = YtdDeduction.SelectSingle(deductCode.CodeID, Payments.Current.TransactionDate.Value.Year.ToString());
						if (ytdTally != null)
						{
							ytdAmount = ytdTally.Amount ?? 0m;
						}

						if (deductionCalculatedAmount + ytdAmount > maxAmount)
						{
							if (paymentDeduct.SaveOverride == true)
							{
								// Leave deduction amount as is but generate a warning that annual maximum has been exceeded
								_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
									paymentDeduct,
									paymentDeduct.DedAmount,
									Messages.DeductionMaxLimitExceededWarn, Messages.PerCalendarYear, Messages.Exceeded);
							}
							else
							{
								// Adjust deduction amount to respect maximum and generate a warning
								deductionCalculatedAmount = Math.Max(maxAmount.Value - ytdAmount, 0m);
								_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
									paymentDeduct,
									paymentDeduct.DedAmount,
									Messages.DeductionMaxLimitExceededWarn, Messages.PerCalendarYear, Messages.Reached);
							}
						}
					}
				}
			}

			if (deductCode.IsGarnishment == true && employeeDeduct?.GarnOrigAmount > 0 &&
				deductionCalculatedAmount > employeeDeduct.GarnOrigAmount - (employeeDeduct.GarnPaidAmount ?? 0m))
			{
				deductionCalculatedAmount = Math.Max(employeeDeduct.GarnOrigAmount.Value - (employeeDeduct.GarnPaidAmount ?? 0m), 0m);
			}

			string cntMaximumFrequency = GetCntMaxFreqTypeValue(deductCode, employeeDeduct);
			if (cntMaximumFrequency != DeductionMaxFrequencyType.NoMaximum)
			{
				decimal? maxAmount = employeeDeduct != null && employeeDeduct.CntUseDflt != null && !employeeDeduct.CntUseDflt.Value ?
					employeeDeduct.CntMaxAmount :
					deductCode.CntMaxAmount;
				if (maxAmount != null)
				{
					if (cntMaximumFrequency == DeductionMaxFrequencyType.PerPayPeriod && contributionCalculatedAmount > maxAmount)
					{
						if (paymentDeduct.SaveOverride == true)
						{
							// Leave contribution amount as is but generate a warning that period maximum has been exceeded
							_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
								paymentDeduct,
								paymentDeduct.CntAmount,
								Messages.BenefitMaxLimitExceededWarn, Messages.PerPayPeriod, Messages.Exceeded);
						}
						else
						{
							// Adjust contribution amount to respect maximum and generate a warning
							contributionCalculatedAmount = Math.Max(maxAmount.Value, 0m);
							_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
								paymentDeduct,
								paymentDeduct.CntAmount,
								Messages.BenefitMaxLimitExceededWarn, Messages.PerPayPeriod, Messages.Reached);
						}
					}
					else if (cntMaximumFrequency == DeductionMaxFrequencyType.PerCalendarYear)
					{
						decimal ytdAmount = 0m;
						if (ytdTally == null)
						{
							ytdTally = YtdDeduction.SelectSingle(deductCode.CodeID, Payments.Current.TransactionDate.Value.Year.ToString());
						}

						if (ytdTally != null)
						{
							ytdAmount = ytdTally.EmployerAmount ?? 0m;
						}

						if (contributionCalculatedAmount + ytdAmount > maxAmount)
						{
							if (paymentDeduct.SaveOverride == true)
							{
								// Leave contribution amount as is but generate a warning that annual maximum has been exceeded
								_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
									paymentDeduct,
									paymentDeduct.CntAmount,
									Messages.BenefitMaxLimitExceededWarn, Messages.PerCalendarYear, Messages.Exceeded);
							}
							else
							{
								// Adjust contribution amount to respect maximum and generate a warning
								contributionCalculatedAmount = Math.Max(maxAmount.Value - ytdAmount, 0m);
								_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
									paymentDeduct,
									paymentDeduct.CntAmount,
									Messages.BenefitMaxLimitExceededWarn, Messages.PerCalendarYear, Messages.Reached);
							}
						}
					}
				}
			}

			return (deductionCalculatedAmount, contributionCalculatedAmount);
		}

		protected virtual SortedDictionary<int, List<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>>> SeparateDeductionsBySequence()
		{
			var deductionSequences = new SortedDictionary<int, List<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>>>();

			foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> deduction in GetDeductions(false))
			{
				if (((PRPaymentDeduct)deduction).Source == PaymentDeductionSourceAttribute.EmployeeSettings)
				{
					var employeeDeduct = (PREmployeeDeduct)deduction;
					int sequenceNumber = employeeDeduct != null && employeeDeduct.Sequence != null ?
						(int)employeeDeduct.Sequence :
						int.MaxValue;
					if (!deductionSequences.ContainsKey(sequenceNumber))
					{
						deductionSequences[sequenceNumber] = new List<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>>();
					}

					deductionSequences[sequenceNumber].Add(deduction);
				}
			}

			return deductionSequences;
		}

		protected virtual decimal AdjustBenefitAmountForSequenceSplit(decimal initialAmount, decimal amountRemainingInSequence, decimal sequenceTotal, int numberInSequence, PRPaymentDeduct deduction, string splitMethod, string errorMessage, string warningMessage)
		{
			decimal adjustedAmount = 0m;
			if (deduction.SaveOverride == true)
			{
				// Overridden deduction amount can't be adjusted => throw error
				_CalculationErrors.AddError<PRPaymentDeduct.dedAmount>(
					deduction,
					deduction.DedAmount,
					errorMessage);
			}
			else
			{
				// Adjust deduction amount so that Minimum net pay is respected and generate a warning
				if (splitMethod == DeductionSplitType.Even && numberInSequence != 0)
				{
					adjustedAmount = amountRemainingInSequence / numberInSequence;
				}
				else if (sequenceTotal != 0)
				{
					adjustedAmount = initialAmount / sequenceTotal * amountRemainingInSequence;
				}

				_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
					deduction,
					adjustedAmount,
					warningMessage);
			}

			return adjustedAmount;
		}

		protected virtual void CreateDeductionDetails()
		{
			foreach (PRPaymentDeduct deduction in GetDeductions().Select(x => (PRPaymentDeduct)x))
			{
				_CalculationUtils.Value.CreateDeductionDetail(this, DeductionDetails.Cache, deduction, PaymentEarningDetails.Select().FirstTableItems.ToList());
			}
		}

		protected virtual void CreateBenefitDetails()
		{
			foreach (PRPaymentDeduct deduction in GetDeductions().Select(x => (PRPaymentDeduct)x))
			{
				_CalculationUtils.Value.CreateBenefitDetail(this, BenefitDetails.Cache, deduction, PaymentEarningDetails.Select().FirstTableItems.ToList());
			}
		}

		protected virtual void CalculateFringeBenefitRates(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			foreach (PRPayrollCalculation payrollCalculation in payrollCalculations)
			{
				PRPayment payment = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
				Payments.Current = payment;

				PaymentsToProcess[Payments.Current].FringeRateReducingBenefits = new Dictionary<FringeBenefitDecreasingRateKey, PRPaymentFringeBenefitDecreasingRate>();
				PaymentsToProcess[Payments.Current].FringeRateReducingEarnings = new Dictionary<FringeEarningDecreasingRateKey, PRPaymentFringeEarningDecreasingRate>();

				List<FringeSourceEarning> fringeRates = new List<FringeSourceEarning>();
				foreach (IGrouping<int?, PXResult<PREarningDetail, PRProjectFringeBenefitRate, PMProject>> result in FringeBenefitApplicableEarnings.Select()
					.Select(x => (PXResult<PREarningDetail, PRProjectFringeBenefitRate, PMProject>)x)
						// It's important to keep ordering in the GroupBy result. PX.Data.SQL's implementation of Enumerable.GroupBy doesn't preserve order,
						// while System.Collections.Generic's implementation does preserve order.
						// Force immediate query evaluation by calling ToList so that System.Collections.Generic's GroupBy is called.
						.ToList()
						.GroupBy(x => ((PREarningDetail)x).RecordID))
				{
					PREarningDetail earning = result.First();
					PRProjectFringeBenefitRate fringeBenefit = result.First();
					PMProject project = result.First();

					if (earning.Hours > 0)
					{
						decimal fringeRate = Math.Max(fringeBenefit.Rate.GetValueOrDefault() - GetFringeReducingRate(earning, project), 0m);
						fringeRates.Add(new FringeSourceEarning(earning, project, fringeRate, fringeBenefit.Rate.GetValueOrDefault()));
							} 
				}

				PaymentsToProcess[payment].FringeAmountsPerProject = SplitFringeRates(fringeRates);
				PaymentsToProcess[payment].FringeRates = fringeRates;

				PaymentsToProcess[Payments.Current].FringeRateReducingBenefits.Values.ForEach(x => PaymentFringeBenefitsDecreasingRate.Insert(x));
				PaymentsToProcess[Payments.Current].FringeRateReducingEarnings.Values.ForEach(x => PaymentFringeEarningsDecreasingRate.Insert(x));

				PaymentsToProcess.UpdatePayment(Payments.Current);
			}

			Actions.PressSave();
			AdjustFringePayoutBenefits();
			ApplyFringeRates();
			Actions.PressSave();
			CalculateTaxesOnFringeEarnings(payrollCalculations);
			Actions.PressSave();
		}

		protected virtual decimal GetFringeReducingRate(PREarningDetail earning, PMProject project)
		{
			decimal reducingRate = GetFringeReducingRateFromBenefits(earning, project.ContractID)
				+ GetFringeReducingRateFromExcessWage(earning, project);

			// reducingRate is used to calculate PRPaymentFringeBenefit.CalculatedFringeRate, which is PXDecimal with default precision (2)
			return Math.Round(reducingRate, 2, MidpointRounding.AwayFromZero);
		}

		protected virtual decimal GetFringeReducingRateFromBenefits(PREarningDetail earning, int? projectID)
		{
			decimal reducingRate = 0m;
			IEnumerable<PRPaymentDeduct> paymentDeducts = GetDeductions().Select(x => (PRPaymentDeduct)x);

			foreach (PRProjectFringeBenefitRateReducingDeduct reducingDeduct in FringeBenefitRateReducingDeductions.Select(projectID))
			{
				decimal applicableHours = GetFringeApplicableHours(projectID, reducingDeduct.AnnualizationException == true);
				if (applicableHours == 0)
				{
					continue;
				}

				decimal benefitAmount = paymentDeducts.FirstOrDefault(x => x.CodeID == reducingDeduct.DeductCodeID)?.CntAmount ?? 0;

				if (benefitAmount == 0)
				{
					continue;
					} 
				reducingRate += benefitAmount / applicableHours;

				FringeBenefitDecreasingRateKey key = new FringeBenefitDecreasingRateKey(earning.ProjectID, reducingDeduct.DeductCodeID, earning.LabourItemID, earning.ProjectTaskID);
				if (!PaymentsToProcess[Payments.Current].FringeRateReducingBenefits.ContainsKey(key))
				{
					PaymentsToProcess[Payments.Current].FringeRateReducingBenefits[key] = new PRPaymentFringeBenefitDecreasingRate()
					{
						ProjectID = earning.ProjectID,
						DeductCodeID = reducingDeduct.DeductCodeID,
						LaborItemID = earning.LabourItemID,
						ProjectTaskID = earning.ProjectTaskID,
						ApplicableHours = applicableHours,
						Amount = benefitAmount
					};
				}
			}

			return reducingRate;
		}

		protected virtual decimal GetFringeReducingRateFromExcessWage(PREarningDetail earning, PMProject project)
		{
			Dictionary<string, WagesAbovePrevailing> excessWages = new Dictionary<string, WagesAbovePrevailing>();
			foreach (PXResult<PREarningDetail, PRLocation, Address, EPEarningType> result in PaymentEarningDetails.Select().ToList()
				.Select(x => (PXResult<PREarningDetail, PRLocation, Address, EPEarningType>)x)
				.Where(x => ((PREarningDetail)x).IsFringeRateEarning != true &&
					((PREarningDetail)x).ProjectID == earning.ProjectID &&
					((PREarningDetail)x).ProjectTaskID == earning.ProjectTaskID &&
					((PREarningDetail)x).LabourItemID == earning.LabourItemID &&
					((EPEarningType)x).OvertimeMultiplier != 0))
			{
				PREarningDetail excessWageEarning = result;
				EPEarningType excessWageEarningType = result;
				decimal? straightTimeRate = excessWageEarning.Rate / excessWageEarningType.OvertimeMultiplier;
				decimal? prevailingRate = PrevailingWage.SelectSingle(excessWageEarning.ProjectID, excessWageEarning.LabourItemID, excessWageEarning.Date, excessWageEarning.ProjectTaskID)?.Rate;
				if (straightTimeRate > prevailingRate)
				{
					if (!excessWages.ContainsKey(excessWageEarningType.TypeCD))
					{
						excessWages[excessWageEarningType.TypeCD] = new WagesAbovePrevailing();
					}
					excessWages[excessWageEarningType.TypeCD].Add(prevailingRate, excessWageEarning.Amount, excessWageEarning.Hours);
				}
			}

			decimal applicableHours = GetFringeApplicableHours(
				project.ContractID,
				Caches[typeof(PMProject)].GetExtension<PMProjectExtension>(project).WageAbovePrevailingAnnualizationException == true);
			if (applicableHours == 0)
			{
				return 0;
			}

			foreach (KeyValuePair<string, WagesAbovePrevailing> kvp in excessWages)
			{
				FringeEarningDecreasingRateKey key = new FringeEarningDecreasingRateKey(earning.ProjectID, kvp.Key, earning.LabourItemID, earning.ProjectTaskID, true);
				if (!PaymentsToProcess[Payments.Current].FringeRateReducingEarnings.ContainsKey(key))
				{
					PaymentsToProcess[Payments.Current].FringeRateReducingEarnings[key] = new PRPaymentFringeEarningDecreasingRate()
					{
						ProjectID = earning.ProjectID,
						EarningTypeCD = kvp.Key,
						LaborItemID = earning.LabourItemID,
						ProjectTaskID = earning.ProjectTaskID,
						ApplicableHours = applicableHours,
						Amount = kvp.Value.ExcessWageAmount,
						ActualPayRate = kvp.Value.EffectivePayRate,
						PrevailingWage = kvp.Value.EffectivePrevailingRate
					};
				}
			}

			return excessWages.Values.Sum(x => x.ExcessWageAmount) / applicableHours;
		}

		protected virtual Dictionary<int?, FringeAmountInfo> SplitFringeRates(
			List<FringeSourceEarning> fringeRates)
		{
			Dictionary<int?, FringeAmountInfo> projectFringeAmounts = new Dictionary<int?, FringeAmountInfo>();

			foreach (IGrouping<int?, FringeSourceEarning> projectGroup in fringeRates.GroupBy(x => x.Project.ContractID))
			{
				decimal totalProjectFringeAmount = projectGroup.Sum(x => x.Earning.Hours.Value * x.CalculatedFringeRate);
				int? destinationBenefitCodeID = projectGroup.First().Project.GetExtension<PMProjectExtension>().BenefitCodeReceivingFringeRate;
				if (destinationBenefitCodeID != null)
				{
					decimal projectFringeAmountAsBenefit = totalProjectFringeAmount;

					IEnumerable<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>> deductions = GetDeductions(codeID: destinationBenefitCodeID);
					PRDeductCode deductCode = deductions.FirstOrDefault() ??
						new SelectFrom<PRDeductCode>.Where<PRDeductCode.codeID.IsEqual<P.AsInt>>.View(this).SelectSingle(destinationBenefitCodeID);

					PRYtdDeductions ytd = YtdDeduction.SelectSingle(deductCode.CodeID, Payments.Current.TransactionDate.Value.Year.ToString());
					decimal? benefitOnPaycheck = deductions.Sum(x => ((PRPaymentDeduct)x).CntAmount.GetValueOrDefault());
					decimal? benefitYtd = benefitOnPaycheck + (ytd?.EmployerAmount ?? 0);

					string cntMaxFreqType = ((PREmployeeDeduct)deductions.FirstOrDefault())?.CntMaxFreqType ?? deductCode.CntMaxFreqType;
					decimal? cntMaxAmount = ((PREmployeeDeduct)deductions.FirstOrDefault())?.CntMaxAmount ?? deductCode.CntMaxAmount;
					if (cntMaxFreqType == DeductionMaxFrequencyType.PerPayPeriod && benefitOnPaycheck + projectFringeAmountAsBenefit > cntMaxAmount)
					{
						projectFringeAmountAsBenefit = (cntMaxAmount - benefitOnPaycheck).GetValueOrDefault();
					}
					else if (cntMaxFreqType == DeductionMaxFrequencyType.PerCalendarYear && benefitYtd + projectFringeAmountAsBenefit > cntMaxAmount)
					{
						projectFringeAmountAsBenefit = (cntMaxAmount - benefitYtd).GetValueOrDefault();
					}

					if (projectFringeAmountAsBenefit > 0)
					{
						PRPaymentDeduct projectPaymentDeduct = deductions.FirstOrDefault(x => ((PRPaymentDeduct)x).Source == PaymentDeductionSourceAttribute.CertifiedProject) ??
							new PRPaymentDeduct()
							{
								DocType = Payments.Current.DocType,
								RefNbr = Payments.Current.RefNbr,
								CodeID = deductCode.CodeID,
								Source = PaymentDeductionSourceAttribute.CertifiedProject,
								CntAmount = 0
							};
						projectPaymentDeduct = Deductions.Locate(projectPaymentDeduct) ?? projectPaymentDeduct;
						projectPaymentDeduct.IsActive = true;
						projectPaymentDeduct.CntAmount += projectFringeAmountAsBenefit;
						Deductions.Update(projectPaymentDeduct);
						projectFringeAmounts[projectGroup.Key] = new FringeAmountInfo(deductCode, totalProjectFringeAmount, projectFringeAmountAsBenefit);
						continue;
					}
				}
				projectFringeAmounts[projectGroup.Key] = new FringeAmountInfo(null, totalProjectFringeAmount, 0);
			}

			return projectFringeAmounts;
		}

		protected virtual decimal GetFringeApplicableHours(int? projectID, bool annualizationException)
		{
			if (annualizationException)
			{
				return ProjectHours.SelectSingle(projectID).Hours ?? 0m;
			}

			PREmployee employee = PXSelectorAttribute.Select<PRPayment.employeeID>(Payments.Cache, Payments.Current) as PREmployee;
			if (employee.StdWeeksPerYear == null || employee.StdWeeksPerYear == 0 || Payments.Current.StartDate == null || Payments.Current.EndDate == null)
			{
				return 0m;
			}

			decimal hoursPerYear;
			if (employee.OverrideHoursPerYearForCertified == true && employee.HoursPerYearForCertified != null)
			{
				hoursPerYear = (decimal)employee.HoursPerYearForCertified;
			}
			else
			{
				hoursPerYear = AnnualBaseForCertifiedAttribute.GetHoursPerYear(Caches[typeof(PREmployee)], employee);
			}

			decimal weeksInPaycheck = (decimal)((Payments.Current.EndDate.Value.Date - Payments.Current.StartDate.Value.Date).TotalDays + 1d) / 7m;
			return hoursPerYear / employee.StdWeeksPerYear.Value * weeksInPaycheck;
		}

		protected virtual void ApplyFringeRates()
		{
			foreach (PaymentCalculationInfo paymentInfo in PaymentsToProcess.Where(x => x.FringeAmountsPerProject.Any()))
			{
				Dictionary<PaymentFringeBenefitKey, PRPaymentFringeBenefit> paymentFringeBenefits = PaymentFringeBenefits.Select().FirstTableItems
					.Where(x => x.DocType == paymentInfo.Payment.DocType && x.RefNbr == paymentInfo.Payment.RefNbr)
					.ToDictionary(k => new PaymentFringeBenefitKey(k.ProjectID, k.LaborItemID, k.ProjectTaskID), v => v);

				Dictionary<int?, FringeAmountInfo> fringeAmountsPerProject = paymentInfo.FringeAmountsPerProject;
				List<FringeSourceEarning> fringeRates = paymentInfo.FringeRates;

				HashSet<int?> deductCodesWithFringeAdded = new HashSet<int?>();
				foreach (IGrouping<int?, FringeSourceEarning> projectGroup in fringeRates.GroupBy(x => x.Project.ContractID))
				{
					if (fringeAmountsPerProject.ContainsKey(projectGroup.Key))
					{
						decimal totalFringeAmount = fringeAmountsPerProject[projectGroup.Key].TotalProjectFringeAmount;
						decimal fringeAmountAsBenefit = fringeAmountsPerProject[projectGroup.Key].ProjectFringeAmountAsBenefit;
						decimal fringeAmountAsEarning = totalFringeAmount - fringeAmountAsBenefit;

						if (fringeAmountAsBenefit > 0)
						{
							deductCodesWithFringeAdded.Add(fringeAmountsPerProject[projectGroup.Key].DeductCode.CodeID);
						}

						if (fringeAmountAsEarning > 0 && totalFringeAmount != 0)
						{
							foreach (FringeSourceEarning sourceEarningInfo in projectGroup.Where(x => x.CalculatedFringeRate != 0))
							{
								InsertFringeRate(sourceEarningInfo.Earning, sourceEarningInfo.CalculatedFringeRate * fringeAmountAsEarning / totalFringeAmount);
							}
						}

						decimal totalProjectCalculatedFringeAmount = projectGroup.Sum(x => x.CalculatedFringeRate * x.Earning.Hours.GetValueOrDefault());
						foreach (var fringeBenefitGrouping in projectGroup.GroupBy(x => new { x.Earning.LabourItemID, x.Earning.ProjectTaskID }))
						{
							int? projectID = projectGroup.Key;
							int? laborItemID = fringeBenefitGrouping.Key.LabourItemID;
							int? projectTaskID = fringeBenefitGrouping.Key.ProjectTaskID;

							PaymentFringeBenefitKey key = new PaymentFringeBenefitKey(projectID, laborItemID, projectTaskID);
							if (!paymentFringeBenefits.TryGetValue(key, out PRPaymentFringeBenefit fringeBenefit))
							{
								fringeBenefit = PaymentFringeBenefits.Insert(new PRPaymentFringeBenefit()
								{
									ProjectID = projectID,
									LaborItemID = laborItemID,
									ProjectTaskID = projectTaskID
								});
							}
							fringeBenefit.ApplicableHours = fringeBenefitGrouping.Sum(x => x.Earning.Hours.Value);
							fringeBenefit.ProjectHours = projectGroup.Sum(x => x.Earning.Hours.Value);
							fringeBenefit.FringeRate = fringeBenefitGrouping.First().SetupFringeRate;
							fringeBenefit.FringeAmountInBenefit = totalProjectCalculatedFringeAmount == 0 ?
								0 : fringeAmountAsBenefit * fringeBenefitGrouping.Sum(x => x.CalculatedFringeRate * x.Earning.Hours.Value) / totalProjectCalculatedFringeAmount;
							PaymentFringeBenefits.Update(fringeBenefit);
							paymentFringeBenefits[key] = fringeBenefit;
						}
					}
				}

				Actions.PressSave();
				BenefitDetails.Select().FirstTableItems.Where(x => deductCodesWithFringeAdded.Contains(x.CodeID)).ForEach(x => BenefitDetails.Delete(x));
				deductCodesWithFringeAdded.ForEach(deductCodeID =>
					_CalculationUtils.Value.CreateBenefitDetail(
						this,
						BenefitDetails.Cache,
						GetDeductions(null, null, deductCodeID).FirstOrDefault(),
						PaymentEarningDetails.Select().FirstTableItems.ToList()));
			}
		}

		protected virtual void InsertFringeRate(PREarningDetail originalEarning, decimal fringeRate)
		{
			PXCache cache = Caches[typeof(PREarningDetail)];
			PREarningDetail fringeEarning = EarningDetailHelper.CreateEarningDetailCopy(cache, originalEarning);

			fringeEarning.IsFringeRateEarning = true;
			fringeEarning.Rate = fringeRate;
			fringeEarning.ManualRate = false;
			fringeEarning.UnitType = UnitType.Hour;
			cache.SetDefaultExt<PREarningDetail.amount>(fringeEarning);

			cache.Update(fringeEarning);
		}

		protected virtual void AdjustFringePayoutBenefits()
		{
			List<PRPayment> paymentsToCalculate = PaymentsToProcess
				.Where(x => x.FringeAmountsPerProject.Values.Any(y => y.DeductCode?.AffectsTaxes == true && y.ProjectFringeAmountAsBenefit > 0))
				.Select(x => x.Payment).ToList();

			if (!paymentsToCalculate.Any())
			{
				return;
			}

			using (var payrollAssemblyScope = new PXPayrollAssemblyScope<PayrollCalculationProxy>())
			{
				foreach (PRPayment payment in paymentsToCalculate)
				{
					Payments.Current = payment;
					string refNbr = payrollAssemblyScope.Proxy.AddPayroll(CreatePayrollBase(payment), new List<PRWage>(), PrepareTaxableBenefitData(false).ToList());
					payrollAssemblyScope.Proxy.SetEmployeeResidenceLocationCode(refNbr, CurrentEmployeeResidenceAddress.SelectSingle()?.TaxLocationCode);
				}

				foreach (var payrollCalculation in payrollAssemblyScope.Proxy.CalculatePayroll())
				{
					PRPayment payment = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
					Payments.Current = payment;
					IEnumerable<PRPaymentDeduct> paymentDeductions = AllPaymentDeductions.Select().FirstTableItems;
					foreach (PRBenefitCalculation benefitCalculation in payrollCalculation.BenefitCalculations)
					{
						decimal benefitMissing = benefitCalculation.EmployerOriginalAmount - benefitCalculation.EmployerCalculatedAmount;
						if (benefitMissing <= 0)
						{
							continue;
						}

						IEnumerable<KeyValuePair<int?, FringeAmountInfo>> applicableFringeBenefits =
							PaymentsToProcess[payrollCalculation.ReferenceNbr].FringeAmountsPerProject.Where(x => x.Value.DeductCode?.CodeCD == benefitCalculation.CodeCD);
						if (!applicableFringeBenefits.Any())
						{
							continue;
						}

						int? codeID = applicableFringeBenefits.First().Value.DeductCode.CodeID;
						decimal totalFringeAmountForBenefit = applicableFringeBenefits.Sum(x => x.Value.ProjectFringeAmountAsBenefit);
						if (totalFringeAmountForBenefit != 0)
						{
							Dictionary<int?, FringeAmountInfo> newAmounts = new Dictionary<int?, FringeAmountInfo>();
							foreach (KeyValuePair<int?, FringeAmountInfo> kvp in applicableFringeBenefits)
							{
								decimal adjustedBenefitAmount = kvp.Value.ProjectFringeAmountAsBenefit - benefitMissing * kvp.Value.ProjectFringeAmountAsBenefit / totalFringeAmountForBenefit;
								newAmounts[kvp.Key] = new FringeAmountInfo(kvp.Value.DeductCode, kvp.Value.TotalProjectFringeAmount, adjustedBenefitAmount);
							}
							newAmounts.ForEach(kvp => PaymentsToProcess[payrollCalculation.ReferenceNbr].FringeAmountsPerProject[kvp.Key] = kvp.Value);
						}

						PRPaymentDeduct adjustedPaymentDeduct = paymentDeductions.First(x => x.CodeID == codeID && x.Source == PaymentDeductionSourceAttribute.CertifiedProject);
						adjustedPaymentDeduct.CntAmount -= benefitMissing;
						_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
							adjustedPaymentDeduct,
							adjustedPaymentDeduct.CntAmount,
							Messages.BenefitAdjustedByTaxEngine);
						AllPaymentDeductions.Update(adjustedPaymentDeduct);
					}
					PaymentsToProcess.UpdatePayment(Payments.Current);
				}
			}
		}

		protected virtual void CalculateTaxesOnFringeEarnings(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			using (var payrollAssemblyScope = new PXPayrollAssemblyScope<PayrollCalculationProxy>())
			{
				bool calculateNeeded = false;
				foreach (var payrollCalculation in payrollCalculations)
				{
					PRPayment payment = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
					Payments.Current = payment;

					if (FringeBenefitCalculatedEarnings.Select().Any())
					{
						PaymentTaxes.Select().ForEach(x => PaymentTaxes.Delete(x));
						calculateNeeded = true;
						string refNbr = payrollAssemblyScope.Proxy.AddPayroll(CreatePayrollBase(payment), PrepareWageData().ToList(), PrepareTaxableBenefitData(false).ToList());
						PopulateTaxSettingData(refNbr, payrollAssemblyScope);
					}
				}

				if (calculateNeeded)
				{
					PaymentTaxesSplit.Cache.Persist(PXDBOperation.Delete);
					PaymentTaxesSplit.Cache.Clear();

					foreach (var payrollCalculation in payrollAssemblyScope.Proxy.CalculatePayroll())
					{
						Payments.Current = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
						SaveTaxCalculations(payrollCalculation);
						PaymentsToProcess.UpdatePayment(Payments.Current);
					}
				}
			}
		}

		protected virtual void CalculateWorkersCompensation(decimal minNetIncome)
		{
			List<PRPaymentDeduct> wcDeductions = ZeroOutWCDeductions().ToList();

			Dictionary<WCPremiumKey, WCPremiumDetails> premiumDetails = new Dictionary<WCPremiumKey, WCPremiumDetails>();
			foreach (PXResult<PREarningDetail, PRLocation, Address, EPEarningType, PRDeductCode> result in WorkCodeEarnings.Select())
			{
				PREarningDetail earningDetail = result;
				PRDeductCode deductCode = result;
				string state = ((Address)result).State;
				bool isOvertime = ((EPEarningType)result).IsOvertime == true;

				WCPremiumDetails calculationDetails = new WCPremiumDetails();
				calculationDetails.RegularHours = isOvertime ? 0m : earningDetail.Hours.GetValueOrDefault();
				calculationDetails.RegularWageBase = isOvertime ? 0m : earningDetail.Amount.GetValueOrDefault();
				calculationDetails.OvertimeHours = isOvertime ? earningDetail.Hours.GetValueOrDefault() : 0m;
				calculationDetails.OvertimeWageBase = isOvertime ? earningDetail.Amount.GetValueOrDefault() : 0m;
				calculationDetails.ApplicableEarningAmountForDed = GetDedBenApplicableAmount(deductCode, ContributionType.EmployeeDeduction);
				calculationDetails.ApplicableEarningAmountForBen = GetDedBenApplicableAmount(deductCode, ContributionType.EmployerContribution);
				calculationDetails.ApplicableEarningHoursForDed = GetDedBenApplicableHours(deductCode, ContributionType.EmployeeDeduction);
				calculationDetails.ApplicableEarningHoursForBen = GetDedBenApplicableHours(deductCode, ContributionType.EmployerContribution);

				WCPremiumKey key = new WCPremiumKey(earningDetail.WorkCodeID, state);
				if (premiumDetails.ContainsKey(key))
				{
					calculationDetails.Add(premiumDetails[key]);
				}
				premiumDetails[key] = calculationDetails;
			}

			PXResultset<PRDeductCode> wcDeductCodes = SelectFrom<PRDeductCode>
				.Where<PRDeductCode.isWorkersCompensation.IsEqual<True>
					.And<PRDeductCode.isActive.IsEqual<True>>>.View.Select(this);
			foreach (KeyValuePair<WCPremiumKey, WCPremiumDetails> kvp in premiumDetails)
			{
				PRDeductCode deductCode = wcDeductCodes.FirstOrDefault(x => ((PRDeductCode)x).State == kvp.Key.State);
				if (deductCode != null)
				{
					PRWorkCompensationBenefitRate rate = WorkCodeRate.SelectSingle(kvp.Key.WorkCodeID, deductCode.CodeID);
					if (rate != null)
					{
						decimal benefitAmount = 0m;
						decimal deductionAmount = 0m;
						switch (deductCode.CntCalcType)
						{
							case DedCntCalculationMethod.PercentOfGross:
							case DedCntCalculationMethod.PercentOfCustom:
								benefitAmount = rate.Rate.GetValueOrDefault() * kvp.Value.ApplicableEarningAmountForBen / 100;
								break;
							case DedCntCalculationMethod.AmountPerHour:
								benefitAmount = rate.Rate.GetValueOrDefault() * kvp.Value.ApplicableEarningHoursForBen;
								break;
						}

						if (deductCode.ContribType != ContributionType.EmployerContribution)
						{
							switch (deductCode.DedCalcType)
							{
								case DedCntCalculationMethod.PercentOfGross:
								case DedCntCalculationMethod.PercentOfCustom:
									decimal applicableAmount = GetDedBenApplicableAmount(deductCode, ContributionType.EmployeeDeduction);
									deductionAmount = rate.DeductionRate.GetValueOrDefault() * kvp.Value.ApplicableEarningAmountForDed / 100;
									break;
								case DedCntCalculationMethod.AmountPerHour:
									decimal applicableHours = GetDedBenApplicableHours(deductCode, ContributionType.EmployeeDeduction);
									deductionAmount = rate.DeductionRate.GetValueOrDefault() * kvp.Value.ApplicableEarningHoursForDed;
									break;
							}
						}

						PRPaymentWCPremium premium = new PRPaymentWCPremium()
						{
							WorkCodeID = kvp.Key.WorkCodeID,
							DeductCodeID = deductCode.CodeID,
							DeductionRate = rate.DeductionRate,
							Rate = rate.Rate,
							RegularWageBaseHours = kvp.Value.RegularHours,
							OvertimeWageBaseHours = kvp.Value.OvertimeHours,
							RegularWageBaseAmount = kvp.Value.RegularWageBase,
							OvertimeWageBaseAmount = kvp.Value.OvertimeWageBase,
							DeductionAmount = deductionAmount,
							Amount = benefitAmount
						};
						premium = WCPremiums.Insert(premium);

						PaymentsToProcess[Payments.Current].NetIncomeAccumulator -= premium.DeductionAmount.GetValueOrDefault();

						PRPaymentDeduct deduction = wcDeductions.FirstOrDefault(x => x.CodeID == deductCode.CodeID);
						if (deduction == null)
						{
							deduction = new PRPaymentDeduct()
							{
								CodeID = deductCode.CodeID,
								Source = PaymentDeductionSourceAttribute.WorkCode,
								DedAmount = 0,
								CntAmount = 0
							};

							wcDeductions.Add(deduction);
						}
						deduction.IsActive = true;
						deduction.DedAmount += premium.DeductionAmount.GetValueOrDefault();
						deduction.CntAmount += premium.Amount;

						if (premium.DeductionAmount > 0 && PaymentsToProcess[Payments.Current].NetIncomeAccumulator < minNetIncome)
						{
							// WC deduction amount can't be adjusted => throw error
							_CalculationErrors.AddError<PRPaymentDeduct.dedAmount>(
								deduction,
								deduction.DedAmount,
								Messages.DeductionCausesNetPayBelowMin);
						}
					}
				}
			}

			foreach (PRPaymentDeduct deduction in wcDeductions)
			{
				Deductions.Update(deduction);
			}
		}

		protected virtual IEnumerable<PRPaymentDeduct> ZeroOutWCDeductions()
		{
			foreach (PRPaymentDeduct deduct in SelectFrom<PRPaymentDeduct>
				.Where<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
					.And<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>>
					.And<PRPaymentDeduct.source.IsEqual<PaymentDeductionSourceAttribute.workCode>>>.View.Select(this))
			{
				deduct.DedAmount = 0m;
				deduct.CntAmount = 0m;
				yield return deduct;
			}
		}

		protected virtual void SetCalculatedStatus(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			foreach (var payrollCalculation in payrollCalculations)
			{
				PRPayment payment = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
				payment.Calculated = !_CalculationErrors.HasChildError(Deductions.Cache, Payments.Cache, payment);
				PaymentsToProcess.UpdatePayment(payment);
				Payments.Update(payment);
			}

			this.Actions.PressSave();
		}

		#endregion Calculations

		#region Overtime Rules Calculation

		protected virtual void CalculatePaymentOvertimeRules()
		{
			foreach (PRPayment payment in PaymentsToProcess.GetAllPayments())
			{
				Payments.Current = payment;
				RegularAmountAttribute.EnforceEarningDetailUpdate<PRPayment.regularAmount>(Payments.Cache, Payments.Current, false);

				//ToDo: Add InnerJoin<PREarningDetail>, AC-145691
				Dictionary<int?, string> locationStates =
				SelectFrom<PRLocation>.InnerJoin<Address>.
					On<PRLocation.addressID.IsEqual<Address.addressID>>.View.Select(this).
				Select(item => (PXResult<PRLocation, Address>)item).
				ToDictionary(location => ((PRLocation)location).LocationID, address => ((Address)address).State);

				foreach (PXResult<PRPaymentOvertimeRule, PROvertimeRule, EPEarningType> rule in OvertimeRulesForCalculation.Select())
				{
					PROvertimeRule overtimeRule = rule;
					EPEarningType overtimeEarningType = rule;
					Dictionary<DateTime, List<PREarningDetail>> possibleOvertimeEarningDetails =
						GetPossibleOvertimeEarningDetails(overtimeRule, overtimeEarningType, locationStates);

					if (overtimeRule.RuleType == PROvertimeRuleType.Daily)
						ProcessDailyOvertimeRule(overtimeRule, possibleOvertimeEarningDetails);
					else if (overtimeRule.RuleType == PROvertimeRuleType.Weekly)
						ProcessWeeklyOvertimeRule(overtimeRule, possibleOvertimeEarningDetails);
				}

				RegularAmountAttribute.EnforceEarningDetailUpdate<PRPayment.regularAmount>(Payments.Cache, Payments.Current, true);
				PaymentsToProcess.UpdatePayment(Payments.Current);
			}
		}

		protected virtual Dictionary<DateTime, List<PREarningDetail>> GetPossibleOvertimeEarningDetails(PROvertimeRule overtimeRule, EPEarningType overtimeEarningType, Dictionary<int?, string> locationStates)
		{
			var baseEarningDetailRecords = new Dictionary<DateTime, List<PREarningDetail>>();

			foreach (PREarningDetail earningDetail in PaymentEarningDetails.Select())
			{
				if (earningDetail.BaseOvertimeRecordID != null || earningDetail.IsOvertime == true)
					continue;

				if (earningDetail.IsPiecework == true || earningDetail.IsAmountBased == true)
					continue;

				if (overtimeRule.RuleType == PROvertimeRuleType.Daily && overtimeRule.WeekDay != null &&
					(DayOfWeek)overtimeRule.WeekDay != earningDetail.Date.GetValueOrDefault().DayOfWeek)
					continue;

				if (earningDetail.TypeCD != Caches[typeof(EPEarningType)].GetExtension<PREarningType>(overtimeEarningType)?.RegularTypeCD)
					continue;

				if (!string.IsNullOrWhiteSpace(overtimeRule.State))
				{
					string locationState;
					if (!locationStates.TryGetValue(earningDetail.LocationID, out locationState) || !string.Equals(locationState, overtimeRule.State))
						continue;
				}

				if (overtimeRule.UnionID != null && !string.Equals(earningDetail.UnionID, overtimeRule.UnionID))
					continue;

				if (overtimeRule.ProjectID != null && earningDetail.ProjectID != overtimeRule.ProjectID)
					continue;

				if (earningDetail.Date == null)
					continue;

				DateTime earningDetailDay = earningDetail.Date.Value;
				if (!baseEarningDetailRecords.ContainsKey(earningDetailDay))
					baseEarningDetailRecords[earningDetailDay] = new List<PREarningDetail>();

				baseEarningDetailRecords[earningDetailDay].Add(earningDetail);
			}

			return baseEarningDetailRecords;
		}

		protected virtual void ProcessDailyOvertimeRule(PROvertimeRule overtimeRule, Dictionary<DateTime, List<PREarningDetail>> possibleOvertimeEarningDetails)
		{
			foreach (var dailyEarningDetails in possibleOvertimeEarningDetails)
				CheckOvertimeHours(dailyEarningDetails.Value, overtimeRule);
		}

		protected virtual void ProcessWeeklyOvertimeRule(PROvertimeRule overtimeRule, Dictionary<DateTime, List<PREarningDetail>> possibleOvertimeEarningDetails)
		{
			if (!possibleOvertimeEarningDetails.Any() || Payments.Current.StartDate == null)
				return;

			PRPayGroupYearSetup payGroupYearSetup =
				SelectFrom<PRPayGroupYearSetup>.
				Where<PRPayGroupYearSetup.payGroupID.IsEqual<P.AsString>>.View.Select(this, Payments.Current.PayGroupID);

			if (payGroupYearSetup.EndYearDayOfWeek == null)
				return;

			DayOfWeek overtimePeriodStartDay =
				OneBasedDayOfWeek.GetZeroBasedDayOfWeek(payGroupYearSetup.EndYearDayOfWeek.Value);

			DateTime firstEarningDetailDate = possibleOvertimeEarningDetails.First().Key;
			DateTime lastEarningDetailDate = possibleOvertimeEarningDetails.Last().Key;
			DateTime currentPeriodStartDate = Payments.Current.StartDate.Value;

			if (currentPeriodStartDate > firstEarningDetailDate)
				currentPeriodStartDate = firstEarningDetailDate;

			while (currentPeriodStartDate.DayOfWeek != overtimePeriodStartDay)
				currentPeriodStartDate = currentPeriodStartDate.AddDays(-1);

			while (currentPeriodStartDate <= lastEarningDetailDate)
			{
				DateTime nextWeekStartDate = currentPeriodStartDate.AddDays(7);

				List<PREarningDetail> weeklyEarningDetails =
					possibleOvertimeEarningDetails.Where(item => item.Key >= currentPeriodStartDate && item.Key < nextWeekStartDate).
					SelectMany(item => item.Value).ToList();

				CheckOvertimeHours(weeklyEarningDetails, overtimeRule);

				currentPeriodStartDate = nextWeekStartDate;
			}
		}

		protected virtual void CheckOvertimeHours(List<PREarningDetail> earningDetails, PROvertimeRule overtimeRule)
		{
			decimal totalHours = earningDetails.Sum(item => item.Hours.GetValueOrDefault());
			decimal overtimeHours = totalHours - overtimeRule.OvertimeThreshold.GetValueOrDefault();

			if (overtimeHours <= 0 || totalHours == 0)
				return;

			if (earningDetails.Count == 1)
			{
				SaveOvertimeCalculation(earningDetails[0], overtimeRule, overtimeHours);
				return;
			}

			foreach (PREarningDetail baseEarningDetail in earningDetails)
			{
				decimal proratedOvertimeHours = baseEarningDetail.Hours.GetValueOrDefault() * overtimeHours / totalHours;
				SaveOvertimeCalculation(baseEarningDetail, overtimeRule, proratedOvertimeHours);
			}
		}

		protected virtual void SaveOvertimeCalculation(PREarningDetail baseEarningDetail, PROvertimeRule overtimeRule, decimal overtimeHours)
		{
			using (PXTransactionScope transactionScope = new PXTransactionScope())
			{
				PREarningDetail overtimeEarningDetailRecord = PXCache<PREarningDetail>.CreateCopy(baseEarningDetail);

				baseEarningDetail.Hours = baseEarningDetail.Hours.GetValueOrDefault() - overtimeHours;
				PaymentEarningDetails.Update(baseEarningDetail);

				overtimeEarningDetailRecord.RecordID = null;
				overtimeEarningDetailRecord.IsOvertime = null;
				overtimeEarningDetailRecord.Amount = null;
				overtimeEarningDetailRecord.TypeCD = overtimeRule.DisbursingTypeCD;
				overtimeEarningDetailRecord.Hours = overtimeHours;
				if (baseEarningDetail.ManualRate == true)
					overtimeEarningDetailRecord.Rate = baseEarningDetail.Rate * overtimeRule.OvertimeMultiplier;
				overtimeEarningDetailRecord.IsRegularRate = false;
				overtimeEarningDetailRecord.BaseOvertimeRecordID = baseEarningDetail.RecordID;

				// Get the list of null fields that will be defaulted by Insert, but should keep their null value in the overtime record.
				PXCache cache = Caches[typeof(PREarningDetail)];
				List<string> nulledFields = new List<string>();
				foreach (string field in cache.Fields)
				{
					if (!cache.Keys.Contains(field) && cache.GetValue(overtimeEarningDetailRecord, field) == null)
					{
						if (cache.GetAttributesOfType<PXDefaultAttribute>(overtimeEarningDetailRecord, field).Any(x => x.PersistingCheck == PXPersistingCheck.Nothing))
						{
							nulledFields.Add(field);
						}
					}
				}

				overtimeEarningDetailRecord = PaymentEarningDetails.Insert(overtimeEarningDetailRecord);
				foreach (string nulledField in nulledFields)
				{
					cache.SetValue(overtimeEarningDetailRecord, nulledField, null);
				}
				PaymentEarningDetails.Update(overtimeEarningDetailRecord);

				transactionScope.Complete(this);
				Actions.PressSave();
			}
		}

		#endregion Overtime Calculation Rules

		#region Helpers
		/// <summary>
		/// Recalculates taxes YTD amounts according to current document date
		/// </summary>
		/// <param name="row"></param>
		protected virtual void UpdateTax(PRPaymentTax row)
		{
			var result = (PRYtdTaxes)YTDTaxes.Select(row.TaxID, Payments.Current.TransactionDate.Value.Year);
			row.YtdAmount = result?.Amount ?? 0;
		}

		protected virtual IEnumerable<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>> GetDeductions(bool? taxable = null, string codeCD = null, int? codeID = null)
		{
			foreach (var group in Deductions.Select()
				.Select(x => (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>)x)
				.ToList()
				.Where(x => (taxable == null || ((PRDeductCode)x).AffectsTaxes == taxable) &&
					(codeCD == null || ((PRDeductCode)x).CodeCD == codeCD) &&
					(codeID == null || ((PRDeductCode)x).CodeID == codeID))
				.GroupBy(x => new { ((PRDeductCode)x).CodeID, ((PRPaymentDeduct)x).Source }))
			{
				yield return group.OrderByDescending(x => ((PREmployeeDeduct)x).StartDate).First();
			}
		}

		protected string GetDedMaxFreqTypeValue(PRDeductCode deductionCode, PREmployeeDeduct employeeDeduction)
		{
			return employeeDeduction != null && employeeDeduction.DedUseDflt == false ? employeeDeduction.DedMaxFreqType : deductionCode.DedMaxFreqType;
		}

		protected string GetCntMaxFreqTypeValue(PRDeductCode deductionCode, PREmployeeDeduct employeeDeduction)
		{
			return employeeDeduction != null && employeeDeduction.CntUseDflt == false ? employeeDeduction.CntMaxFreqType : deductionCode.CntMaxFreqType;
		}

		protected virtual void RecordProjectPackageNominalAmounts(PRDeductionAndBenefitProjectPackage package, PackageDedBenCalculation calculation)
		{
			var packageKey = new ProjectDedBenPackageKey(package.ProjectID, package.DeductionAndBenefitCodeID, package.LaborItemID);
			if (!PaymentsToProcess[Payments.Current].NominalProjectPackageAmounts.ContainsKey(packageKey))
			{
				PaymentsToProcess[Payments.Current].NominalProjectPackageAmounts[packageKey] = calculation;
			}
			else
			{
				PaymentsToProcess[Payments.Current].NominalProjectPackageAmounts[packageKey].Add(calculation);
			}
		}

		protected virtual void RecordUnionPackageNominalAmounts(PRDeductionAndBenefitUnionPackage package, PackageDedBenCalculation calculation)
		{
			var packageKey = new UnionDedBenPackageKey(package.UnionID, package.DeductionAndBenefitCodeID, package.LaborItemID);
			if (!PaymentsToProcess[Payments.Current].NominalUnionPackageAmounts.ContainsKey(packageKey))
			{
				PaymentsToProcess[Payments.Current].NominalUnionPackageAmounts[packageKey] = calculation;
			}
			else
			{
				PaymentsToProcess[Payments.Current].NominalUnionPackageAmounts[packageKey].Add(calculation);
			}
		}

		protected virtual void InsertProjectPackageDetails()
		{
			if (PaymentsToProcess[Payments.Current].NominalProjectPackageAmounts.Any())
			{
				foreach (PRPaymentDeduct paymentDeduct in Deductions.Select().FirstTableItems.Where(x => x.Source == PaymentDeductionSourceAttribute.CertifiedProject))
				{
					IEnumerable<KeyValuePair<ProjectDedBenPackageKey, PackageDedBenCalculation>> nominals =
						PaymentsToProcess[Payments.Current].NominalProjectPackageAmounts.Where(x => x.Key.DeductCodeID == paymentDeduct.CodeID);
					decimal totalNominalDeductionAmount = nominals.Sum(x => x.Value.DeductionAmount.GetValueOrDefault());
					decimal totalActualDeductionAmount = paymentDeduct.DedAmount.GetValueOrDefault();
					decimal deductionFactor = totalNominalDeductionAmount > 0 ? totalActualDeductionAmount / totalNominalDeductionAmount : 0m;
					decimal totalNominalBenefitAmount = nominals.Sum(x => x.Value.BenefitAmount.GetValueOrDefault());
					decimal totalActualBenefitAmount = paymentDeduct.CntAmount.GetValueOrDefault();
					decimal benefitFactor = totalNominalBenefitAmount > 0 ? totalActualBenefitAmount / totalNominalBenefitAmount : 0m;

					foreach (KeyValuePair<ProjectDedBenPackageKey, PackageDedBenCalculation> kvp in nominals)
					{
						if (kvp.Value.DeductionAmount > 0 && deductionFactor > 0 ||
							kvp.Value.BenefitAmount > 0 && benefitFactor > 0)
						{
							PRPaymentProjectPackageDeduct detail = new PRPaymentProjectPackageDeduct()
							{
								ProjectID = kvp.Key.ProjectID,
								LaborItemID = kvp.Key.LaborItemID,
								DeductCodeID = kvp.Key.DeductCodeID,
								RegularWageBaseHours = kvp.Value.RegularHours,
								OvertimeWageBaseHours = kvp.Value.OvertimeHours,
								RegularWageBaseAmount = kvp.Value.RegularHoursAmount,
								OvertimeWageBaseAmount = kvp.Value.OvertimeHoursAmount,
								DeductionAmount = kvp.Value.DeductionAmount * deductionFactor,
								BenefitAmount = kvp.Value.BenefitAmount * benefitFactor
							};
							ProjectPackageDeductions.Insert(detail);
						}
					}
				}
			}
		}
		
		protected virtual void InsertUnionPackageDetails()
		{
			if (PaymentsToProcess[Payments.Current].NominalUnionPackageAmounts.Any())
			{
				foreach (PRPaymentDeduct paymentDeduct in Deductions.Select().FirstTableItems.Where(x => x.Source == PaymentDeductionSourceAttribute.Union))
				{
					IEnumerable<KeyValuePair<UnionDedBenPackageKey, PackageDedBenCalculation>> nominals =
						PaymentsToProcess[Payments.Current].NominalUnionPackageAmounts.Where(x => x.Key.DeductCodeID == paymentDeduct.CodeID);
					decimal totalNominalDeductionAmount = nominals.Sum(x => x.Value.DeductionAmount.GetValueOrDefault());
					decimal totalActualDeductionAmount = paymentDeduct.DedAmount.GetValueOrDefault();
					decimal deductionFactor = totalNominalDeductionAmount > 0 ? totalActualDeductionAmount / totalNominalDeductionAmount : 0m;
					decimal totalNominalBenefitAmount = nominals.Sum(x => x.Value.BenefitAmount.GetValueOrDefault());
					decimal totalActualBenefitAmount = paymentDeduct.CntAmount.GetValueOrDefault();
					decimal benefitFactor = totalNominalBenefitAmount > 0 ? totalActualBenefitAmount / totalNominalBenefitAmount : 0m;

					foreach (KeyValuePair<UnionDedBenPackageKey, PackageDedBenCalculation> kvp in nominals)
					{
						if (kvp.Value.DeductionAmount > 0 && deductionFactor > 0 ||
							kvp.Value.BenefitAmount > 0 && benefitFactor > 0)
						{
							PRPaymentUnionPackageDeduct detail = new PRPaymentUnionPackageDeduct()
							{
								UnionID = kvp.Key.UnionID,
								LaborItemID = kvp.Key.LaborItemID,
								DeductCodeID = kvp.Key.DeductCodeID,
								RegularWageBaseHours = kvp.Value.RegularHours,
								OvertimeWageBaseHours = kvp.Value.OvertimeHours,
								RegularWageBaseAmount = kvp.Value.RegularHoursAmount,
								OvertimeWageBaseAmount = kvp.Value.OvertimeHoursAmount,
								DeductionAmount = kvp.Value.DeductionAmount * deductionFactor,
								BenefitAmount = kvp.Value.BenefitAmount * benefitFactor
							};
							UnionPackageDeductions.Insert(detail);
						}
					}
				}
			}
		}
		
		private decimal GetDedBenApplicableAmount(PRDeductCode deductCode, string contributionType)
		{
			return _CalculationUtils.Value.GetDedBenApplicableAmount(
				this,
				deductCode,
				contributionType,
				PaymentEarningDetails.Select().FirstTableItems,
				PaymentsToProcess[Payments.Current].TaxesSplitByEarning,
				PaymentsToProcess[Payments.Current].TaxableDeductionsAndBenefitsSplitByEarning);
		}

		private decimal GetDedBenApplicableAmount(PRDeductCode deductCode, string contributionType, PREarningDetail earning)
		{
			return _CalculationUtils.Value.GetDedBenApplicableAmount(
				this,
				deductCode,
				contributionType,
				earning,
				PaymentsToProcess[Payments.Current].TaxesSplitByEarning,
				PaymentsToProcess[Payments.Current].TaxableDeductionsAndBenefitsSplitByEarning);
		}

		private decimal GetDedBenApplicableHours(PRDeductCode deductCode, string contributionType)
		{
			return _CalculationUtils.Value.GetDedBenApplicableHours(
				this,
				deductCode,
				contributionType,
				PaymentEarningDetails.Select().FirstTableItems);
		}

		private decimal GetDedBenApplicableHours(PRDeductCode deductCode, string contributionType, PREarningDetail earning)
		{
			return _CalculationUtils.Value.GetDedBenApplicableHours(
				this,
				deductCode,
				contributionType,
				earning);
		}
		#endregion Helpers
	}
}