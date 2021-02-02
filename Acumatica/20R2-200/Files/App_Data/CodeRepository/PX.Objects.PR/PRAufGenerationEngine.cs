using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.PR.AUF;
using PX.Payroll;
using PX.Payroll.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using EPEmployee = PX.Objects.EP.EPEmployee;

namespace PX.Objects.PR
{
	[PXHidden]
	public class PRAufGenerationEngine : PXGraph<PRAufGenerationEngine>
	{
		private Dictionary<string, SymmetryToAatrixTaxMapping> _TaxIDMapping;
		private Dictionary<string, int> _WCMapping;
		private List<PimRecord> _PayrollItemMapping;
		private List<CjiRecord> _JobItemMapping;

		protected virtual byte[] GenerateAufInternal(int orgBAccountID, PRGovernmentReport reportToGenerate)
		{
			using (var prTypeCache = new PRTypeSelectorBaseAttribute.PRTypeSelectorCache(new Type[] { typeof(PRWage), typeof(PRBenefit) }))
			{
				SetCurrents(orgBAccountID, reportToGenerate);

				_WCMapping = new Dictionary<string, int>();

				DatRecord datRecord = CreateDateRecord();

				// First create reference items that are used by company or employee records
				_TaxIDMapping = MapTaxes();
				_PayrollItemMapping = CreatePimRecords().ToList();
				if (ShouldGenerateCertifiedRecords())
				{
				_JobItemMapping = CreateCompanyJobRecords(datRecord).ToList();
				}

				List<EmpRecord> empList = CreateEmpRecords(datRecord).ToList();

				AufFormatter formatter = new AufFormatter()
				{
					Dat = datRecord,
					PimList = _PayrollItemMapping.OrderBy(x => x.PimID).ToList(),
					Cmp = CreateCompanyRecord(datRecord, empList),
					EmpList = empList
				};

				return formatter.GenerateAufFile();
			}
		}

		static public void GenerateAuf(int orgBAccountID, PRGovernmentReport reportToGenerate)
		{
			try
			{
				PRAufGenerationEngine instance = PXGraph.CreateInstance<PRAufGenerationEngine>();
				PXLongOperation.SetCustomInfo(instance.GenerateAufInternal(orgBAccountID, reportToGenerate));
			}
			catch (Exception e)
			{
				PXLongOperation.SetCustomInfo(new PXException(e.Message));
			}
		}

		#region Views
		public SelectFrom<PRGovernmentReport>.View Report;

		public SelectFrom<BAccountR>
			.Where<BAccountR.bAccountID.IsEqual<P.AsInt>>.View OrgAccount;

		public SelectFrom<Address>
			.Where<Address.addressID.IsEqual<BAccountR.defAddressID.FromCurrent>>.View OrgAddress;

		public SelectFrom<Contact>
			.Where<Contact.contactID.IsEqual<BAccountR.defContactID.FromCurrent>>.View OrgContact;

		public SelectFrom<Country>
			.InnerJoin<Address>.On<Address.addressID.IsEqual<BAccountR.defAddressID.FromCurrent>>
			.Where<Country.countryID.IsEqual<Address.countryID>>.View OrgCountry;

		public SelectFrom<EPEmployee>
			.InnerJoin<Branch>.On<Branch.bAccountID.IsEqual<EPEmployee.parentBAccountID>>
			.Where<Where<Branch.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>.View TotalEmployees;

		public SelectFrom<PRCompanyTaxAttribute>
			.Where<PRCompanyTaxAttribute.aatrixMapping.IsEqual<P.AsInt>>.View CompanyTaxAttribute;

		public SelectFrom<EPEmployee>
			.InnerJoin<Contact>.On<Contact.contactID.IsEqual<EPEmployee.defContactID>>
			.LeftJoin<Address>.On<Address.addressID.IsEqual<EPEmployee.defAddressID>>
			.LeftJoin<Country>.On<Country.countryID.IsEqual<Address.countryID>>
			.LeftJoin<PREmployeeDirectDeposit>.On<PREmployeeDirectDeposit.bAccountID.IsEqual<EPEmployee.bAccountID>>
			.InnerJoin<PRPayment>.On<PRPayment.employeeID.IsEqual<EPEmployee.bAccountID>>
			.Where<Brackets<PRPayment.transactionDate.IsGreaterEqual<P.AsDateTime>
				.And<PRPayment.transactionDate.IsLessEqual<P.AsDateTime>>
				.And<PRGovernmentReport.reportType.FromCurrent.IsNotEqual<certifiedReportType>>
				.Or<PRPayment.startDate.IsGreaterEqual<P.AsDateTime>
					.And<PRPayment.endDate.IsLessEqual<P.AsDateTime>>
					.And<PRGovernmentReport.reportType.FromCurrent.IsEqual<certifiedReportType>>>>
				.And<PRPayment.released.IsEqual<True>>
				.And<Where<PRPayment.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>>
			.AggregateTo<GroupBy<EPEmployee.bAccountID>>.View PeriodEmployees;

		public SelectFrom<PREmployee>
			.InnerJoin<PREmployeeClass>.On<PREmployeeClass.employeeClassID.IsEqual<PREmployee.employeeClassID>>
			.InnerJoin<PRPayGroup>.On<PRPayGroup.payGroupID.IsEqual<PREmployee.payGroupID>>
			.InnerJoin<PRPayGroupYearSetup>.On<PRPayGroupYearSetup.payGroupID.IsEqual<PRPayGroup.payGroupID>>
			.Where<PREmployee.bAccountID.IsEqual<EPEmployee.bAccountID.FromCurrent>>.View PayrollEmployee;

		public SelectFrom<EPPosition>
			.InnerJoin<EPEmployeePosition>.On<EPEmployeePosition.positionID.IsEqual<EPPosition.positionID>>
			.Where<EPEmployeePosition.employeeID.IsEqual<EPEmployee.bAccountID.FromCurrent>
				.And<EPEmployeePosition.startDate.IsLessEqual<P.AsDateTime>>
				.And<Where<EPEmployeePosition.endDate.IsNull
					.Or<EPEmployeePosition.endDate.IsGreater<P.AsDateTime>>>>>
			.OrderBy<EPEmployeePosition.startDate.Desc>.View EmployeeTitles;

		public SelectFrom<PMWorkCode>
			.InnerJoin<PREmployee>.On<PREmployee.workCodeID.IsEqual<PMWorkCode.workCodeID>>
			.Where<PREmployee.bAccountID.IsEqual<EPEmployee.bAccountID.FromCurrent>>.View EmployeeWorkCode;

		public SelectFrom<PREmployeeEarning>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREmployeeEarning.typeCD>>
			.Where<PREmployeeEarning.bAccountID.IsEqual<EPEmployee.bAccountID.FromCurrent>
				.And<PREmployeeEarning.isActive.IsEqual<True>>
				.And<PREmployeeEarning.startDate.IsLessEqual<P.AsDateTime>>
				.And<Where<PREmployeeEarning.endDate.IsNull
					.Or<PREmployeeEarning.endDate.IsGreater<P.AsDateTime>>>>
				.And<EPEarningType.isOvertime.IsEqual<False>>
				.And<PREarningType.isPiecework.IsEqual<False>>
				.And<PREarningType.isAmountBased.IsEqual<False>>
				.And<PREarningType.wageTypeCD.IsEqual<P.AsInt>>>
			.OrderBy<PREmployeeEarning.startDate.Desc>.View EmployeeEarningRates;

		public SelectFrom<PREmployeeAttribute>
			.Where<PREmployeeAttribute.bAccountID.IsEqual<EPEmployee.bAccountID.AsOptional>>.View EmployeeAttributes;

		public SelectFrom<PREmployeeTaxAttribute>
			.Where<PREmployeeTaxAttribute.bAccountID.IsEqual<EPEmployee.bAccountID.FromCurrent>>.View EmployeeTaxAttributes;

		public SelectFrom<PMProject>
			.LeftJoin<PMAddress>.On<PMAddress.addressID.IsEqual<PMProject.billAddressID>>
			.LeftJoin<PREarningDetail>.On<PREarningDetail.projectID.IsEqual<PMProject.contractID>>
			.LeftJoin<PRPayment>.On<PRPayment.refNbr.IsEqual<PREarningDetail.paymentRefNbr>
				.And<PRPayment.docType.IsEqual<PREarningDetail.paymentDocType>>>
			.LeftJoin<PREmployee>.On<PREmployee.bAccountID.IsEqual<PRPayment.employeeID>>
			.Where<Brackets<PRPayment.startDate.IsGreaterEqual<P.AsDateTime>
					.And<PRPayment.endDate.IsLessEqual<P.AsDateTime>>
					.And<PRPayment.released.IsEqual<True>>
					.And<PREmployee.exemptFromCertifiedReporting.IsNotEqual<True>>
					.And<PREarningDetail.certifiedJob.IsEqual<True>>
					.And<Where<PRPayment.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>
					.Or<PMProjectExtension.fileEmptyCertifiedReport.IsEqual<True>
						.And<PMProject.isActive.IsEqual<True>>
						.And<PMProject.nonProject.IsEqual<False>>
						.And<PMProject.certifiedJob.IsEqual<True>>>>
				.And<PMProject.baseType.IsEqual<CT.CTPRType.project>>>
			.AggregateTo<GroupBy<PMProject.contractID>>.View PeriodProjects;

		public SelectFrom<PMRevenueBudget>
			.Where<PMRevenueBudget.projectID.IsEqual<PMProject.contractID.FromCurrent>
				.And<PMRevenueBudget.type.IsEqual<GL.AccountType.income>>>
			.AggregateTo<GroupBy<PMRevenueBudget.projectID, Sum<PMRevenueBudget.curyActualAmount>>>.View ProjectTotalBudget;

		public SelectFrom<PMTask>
			.Where<PMTask.projectID.IsEqual<PMProject.contractID.FromCurrent>>.View ProjectTasks;

		public SelectFrom<PRPayment>
			.LeftJoin<PRPaymentTaxSplit>.On<PRPaymentTaxSplit.refNbr.IsEqual<PRPayment.refNbr>
				.And<PRPaymentTaxSplit.docType.IsEqual<PRPayment.docType>>>
			.LeftJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRPaymentTaxSplit.taxID>>
			.Where<PRPayment.employeeID.IsEqual<EPEmployee.bAccountID.FromCurrent>
				.And<PRPayment.transactionDate.IsGreaterEqual<P.AsDateTime>
				.And<PRPayment.transactionDate.IsLessEqual<P.AsDateTime>>
					.And<PRGovernmentReport.reportType.FromCurrent.IsNotEqual<certifiedReportType>>
					.Or<PRPayment.startDate.IsGreaterEqual<P.AsDateTime>
						.And<PRPayment.endDate.IsLessEqual<P.AsDateTime>>
						.And<PRGovernmentReport.reportType.FromCurrent.IsEqual<certifiedReportType>>>>
				.And<PRPayment.released.IsEqual<True>>
				.And<Where<PRPayment.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>>
			.OrderBy<PRPayment.transactionDate.Asc>.View EmployeePayments;

		public SelectFrom<PREarningDetail>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.LeftJoin<PMProject>.On<PMProject.contractID.IsEqual<PREarningDetail.projectID>>
			.LeftJoin<PMWorkCode>.On<PMWorkCode.workCodeID.IsEqual<PREarningDetail.workCodeID>>
			.Where<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>>
				.And<PREarningDetail.unitType.IsEqual<UnitType.hour>>
				.And<Where<PMProject.baseType.IsEqual<CT.CTPRType.project>
					.Or<PMProject.baseType.IsEqual<CT.CTPRType.projectTemplate>>
					.Or<PMProject.baseType.IsNull>>>>.View EmployeePaycheckHourlyEarnings;

		public SelectFrom<PMLaborCostRate>
			.Where<PMLaborCostRate.type.IsEqual<PMLaborCostRateType.certified>
				.And<PMLaborCostRate.inventoryID.IsEqual<P.AsInt>>
				.And<PMLaborCostRate.projectID.IsEqual<P.AsInt>>
				.And<PMLaborCostRate.effectiveDate.IsLessEqual<P.AsDateTime>>
				.And<PMLaborCostRate.taskID.IsEqual<P.AsInt>
					.Or<PMLaborCostRate.taskID.IsNull>>>
			.OrderBy<PMLaborCostRate.taskID.Desc, PMLaborCostRate.effectiveDate.Desc>.View PrevailingWage;

		public SelectFrom<PRTaxCode>.View TaxCodes;

		public SelectFrom<PRPaymentEarning>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PRPaymentEarning.typeCD>>
			.Where<PRPaymentEarning.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentEarning.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View PaymentEarnings;

		public SelectFrom<PRPaymentDeduct>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentDeduct.codeID>>
			.Where<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View PaymentDeductions;

		public SelectFrom<PRPaymentFringeBenefitDecreasingRate>
			.InnerJoin<PRPayment>.On<PRPayment.refNbr.IsEqual<PRPaymentFringeBenefitDecreasingRate.refNbr>
				.And<PRPayment.docType.IsEqual<PRPaymentFringeBenefitDecreasingRate.docType>>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentFringeBenefitDecreasingRate.deductCodeID>>
			.LeftJoin<BAccount>.On<BAccount.bAccountID.IsEqual<PRDeductCode.bAccountID>>
			.LeftJoin<Address>.On<Address.addressID.IsEqual<BAccount.defAddressID>>
			.LeftJoin<Contact>.On<Contact.contactID.IsEqual<BAccount.defContactID>>
			.Where<PRPaymentFringeBenefitDecreasingRate.benefitRate.IsNotEqual<decimal0>
				.And<PRPayment.startDate.IsGreaterEqual<P.AsDateTime>>
				.And<PRPayment.endDate.IsLessEqual<P.AsDateTime>>
				.And<PRPayment.released.IsEqual<True>>
				.And<Where<PRPayment.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>>.View PeriodCertifiedBenefits;

		public SelectFrom<PRPaymentFringeBenefit>
			.InnerJoin<PRPayment>.On<PRPayment.refNbr.IsEqual<PRPaymentFringeBenefit.refNbr>
				.And<PRPayment.docType.IsEqual<PRPaymentFringeBenefit.docType>>>
			.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PRPaymentFringeBenefit.projectID>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PMProjectExtension.benefitCodeReceivingFringeRate>>
			.LeftJoin<BAccount>.On<BAccount.bAccountID.IsEqual<PRDeductCode.bAccountID>>
			.LeftJoin<Address>.On<Address.addressID.IsEqual<BAccount.defAddressID>>
			.LeftJoin<Contact>.On<Contact.contactID.IsEqual<BAccount.defContactID>>
			.Where<PRDeductCode.certifiedReportType.IsNotNull
				.And<PRPayment.startDate.IsGreaterEqual<P.AsDateTime>>
				.And<PRPayment.endDate.IsLessEqual<P.AsDateTime>>
				.And<PRPayment.released.IsEqual<True>>
				.And<Where<PRPayment.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>>.View PeriodFringeDestinationBenefits;

		public SelectFrom<PRAcaCompanyYearlyInformation>
			.Where<PRAcaCompanyYearlyInformation.orgBAccountID.IsEqual<BAccountR.bAccountID.FromCurrent>
				.And<PRAcaCompanyYearlyInformation.year.IsEqual<PRGovernmentReport.year.FromCurrent>>>.View AcaCompanyYearlyInformation;

		public SelectFrom<PRAcaCompanyMonthlyInformation>
			.Where<PRAcaCompanyMonthlyInformation.orgBAccountID.IsEqual<BAccountR.bAccountID.FromCurrent>
				.And<PRAcaCompanyMonthlyInformation.year.IsEqual<PRGovernmentReport.year.FromCurrent>>>.View AcaCompanyMonthlyInformation;

		public SelectFrom<PRAcaAggregateGroupMember>
			.Where<PRAcaAggregateGroupMember.orgBAccountID.IsEqual<BAccountR.bAccountID.FromCurrent>
				.And<PRAcaAggregateGroupMember.year.IsEqual<PRGovernmentReport.year.FromCurrent>>>.View AcaAggregateGroupMembers;

		public SelectFrom<PRAcaEmployeeMonthlyInformation>
			.Where<PRAcaEmployeeMonthlyInformation.orgBAccountID.IsEqual<BAccountR.bAccountID.FromCurrent>
				.And<PRAcaEmployeeMonthlyInformation.year.IsEqual<PRGovernmentReport.year.FromCurrent>>
				.And<PRAcaEmployeeMonthlyInformation.employeeID.IsEqual<EPEmployee.bAccountID.FromCurrent>>>.View AcaEmployeeMonthlyInformation;

		public SelectFrom<PRDeductCode>
			.LeftJoin<PRAcaDeductCoverageInfo>.On<PRAcaDeductCoverageInfo.deductCodeID.IsEqual<PRDeductCode.codeID>>
			.Where<PRDeductCode.isActive.IsEqual<True>
				.And<PRAcaDeductCode.acaApplicable.IsEqual<True>>>.View AcaDeductions;

		public SelectFrom<PRPaymentTaxSplit>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRPaymentTaxSplit.taxID>>
			.Where<PRPaymentTaxSplit.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentTaxSplit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View PaymentTaxSplits;

		public SelectFrom<PRPaymentWCPremium>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentWCPremium.deductCodeID>>
			.Where<PRPaymentWCPremium.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentWCPremium.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PRPaymentWCPremium.amount.IsNotEqual<decimal0>
					.Or<PRPaymentWCPremium.deductionAmount.IsNotEqual<decimal0>>>>.View PaymentWCPremiums;

		public SelectFrom<PRTaxDetail>
			.InnerJoin<APInvoice>.On<APInvoice.docType.IsEqual<PRTaxDetail.apInvoiceDocType>
				.And<APInvoice.refNbr.IsEqual<PRTaxDetail.apInvoiceRefNbr>>>
			.InnerJoin<APAdjust>.On<APAdjust.adjdDocType.IsEqual<APInvoice.docType>
				.And<APAdjust.adjdRefNbr.IsEqual<APInvoice.refNbr>>>
			.InnerJoin<APPayment>.On<APPayment.docType.IsEqual<APAdjust.adjgDocType>
				.And<APPayment.refNbr.IsEqual<APAdjust.adjgRefNbr>>>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRTaxDetail.taxID>>
			.InnerJoin<PRPayment>.On<PRPayment.docType.IsEqual<PRTaxDetail.paymentDocType>
				.And<PRPayment.refNbr.IsEqual<PRTaxDetail.paymentRefNbr>>>
			.Where<PRPayment.transactionDate.IsEqual<P.AsDateTime>
				.And<APInvoice.released.IsEqual<True>>>
			.AggregateTo<GroupBy<PRTaxCode.taxID>, GroupBy<APPayment.adjDate>, Sum<PRTaxDetail.amount>>.View PaymentSettledTaxes;

		public PXSetup<PRSetup> PayrollPreferences;

		public SelectFrom<PREarningDetail>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.Where<PREarningDetail.paymentDocType.IsEqual<P.AsString>
				.And<PREarningDetail.paymentRefNbr.IsEqual<P.AsString>>
				.And<PREarningDetail.projectID.IsEqual<P.AsInt>>
				.And<PREarningDetail.isFringeRateEarning.IsEqual<False>>>
			.AggregateTo<Sum<PREarningDetail.hours>>.View PaymentProjectHours;
		#endregion Views

		#region Helpers
		private void SetCurrents(int orgBAccountID, PRGovernmentReport reportToGenerate)
		{
			Report.Current = reportToGenerate;
			OrgAccount.Current = OrgAccount.SelectSingle(orgBAccountID);
		}

		protected virtual Dictionary<string, SymmetryToAatrixTaxMapping> MapTaxes()
		{
			List<PX.Payroll.Data.PRTaxType> configuredTaxes = new List<Payroll.Data.PRTaxType>();
			foreach (PRTaxCode tax in TaxCodes.Select())
			{
				configuredTaxes.Add(new PX.Payroll.Data.PRTaxType()
				{
					TypeName = tax.TypeName,
					UniqueTaxID = tax.TaxUniqueCode,
					TaxJurisdiction = TaxJurisdiction.GetTaxJurisdiction(tax.JurisdictionLevel),
					TaxCategory = TaxCategory.GetTaxCategory(tax.TaxCategory)
				});
			}

            var payrollClient = new PayrollTaxClient();
            return payrollClient.GetAatrixTaxMapping(configuredTaxes);
		}

		protected virtual DatRecord CreateDateRecord()
		{
			PRGovernmentReport reportToGenerate = Report.Current;
			DatRecord record = null;
			int? reportYear = !string.IsNullOrEmpty(reportToGenerate.Year) ? int.Parse(reportToGenerate.Year) : new int?();

			switch (reportToGenerate.ReportingPeriod)
			{
				case GovernmentReportingPeriod.Annual:
					record = new DatRecord(new DateTime(reportYear.Value, 1, 1), new DateTime(reportYear.Value, 12, 31));
					break;
				case GovernmentReportingPeriod.Quarterly:
					record = new DatRecord(
						new DateTime(reportYear.Value, (reportToGenerate.Quarter.Value - 1) * 3 + 1, 1),
						new DateTime(reportYear.Value, (reportToGenerate.Quarter.Value - 1) * 3 + 3, DateTime.DaysInMonth(reportYear.Value, (reportToGenerate.Quarter.Value - 1) * 3 + 3)))
					{
						Quarter = reportToGenerate.Quarter
					};
					break;
				case GovernmentReportingPeriod.Monthly:
					record = new DatRecord(
						new DateTime(reportYear.Value, reportToGenerate.Month.Value, 1),
						new DateTime(reportYear.Value, reportToGenerate.Month.Value, DateTime.DaysInMonth(reportYear.Value, reportToGenerate.Month.Value)))
					{
						Month = reportToGenerate.Month
					};
					break;
				case GovernmentReportingPeriod.DateRange:
					record = new DatRecord(reportToGenerate.DateFrom.Value, reportToGenerate.DateTo.Value);
					break;
			}

			return record;
		}

		protected virtual CmpRecord CreateCompanyRecord(DatRecord reportPeriod, IEnumerable<EmpRecord> empList)
		{
			Address address = OrgAddress.SelectSingle();
			Contact contact = OrgContact.SelectSingle();

			CmpRecord record = new CmpRecord()
			{
				Name = OrgAccount.Current.AcctName,
				AddressLine1 = address?.AddressLine1,
				AddressLine2 = address?.AddressLine2,
				City = address?.City,
				StateAbbr = address?.State,
				ZipCode = address?.PostalCode,
				Country = OrgCountry.SelectSingle()?.Description,
				CountryAbbr = address?.CountryID,
				NonUSPostalCode = address?.PostalCode,
				BranchName = OrgAccount.Current.AcctCD.TrimEnd(),
				TaxArea = CompanyTaxAttribute.SelectSingle(AatrixField.CMP.TaxArea)?.Value,
				PhoneNumber = contact?.Phone1,
				PhoneExtension = contact?.Phone2,
				FaxNumber = contact?.Fax,
				IndustryCode = CompanyTaxAttribute.SelectSingle(AatrixField.CMP.IndustryCode)?.Value,
				Ein = OrgAccount.Current.TaxRegistrationID,
				NumberOfEmployees = TotalEmployees.Select().Count,
				ContactEmail = contact?.EMail,
				NonUSState = address?.State,
				NonUSNationalID = OrgAccount.Current.TaxRegistrationID,
				KindOfEmployer = CompanyTaxAttribute.SelectSingle(AatrixField.CMP.KindOfEmployer)?.Value?.First(),

				// Children records
				GtoList = CreateGeneralCompanyRecords(empList).OrderBy(x => x.CheckDate).ToList()
			};

			if (ShouldGenerateCertifiedRecords())
			{
				record.CjiList = _JobItemMapping.OrderBy(x => x.JobID).ToList();
				record.CfbList = CreateCompanyCertifiedBenefitRecords(reportPeriod).OrderBy(x => x.BenefitID).ToList();
			}

			if (ShouldGenerateAcaRecords())
			{
				record.Ale = CreateCompanyAleRecord();

				if (record.Ale != null)
				{
					if (record.Ale.IsAggregateGroupMember == true)
					{
						record.AggList = CreateCompanyAggRecords().ToList();
					}

					if (record.Ale.IsDesignatedGovernmentEntity == true)
					{
						record.Dge = CreateCompanyDgeRecord(record);
					}
				}
			}

			return record;
		}

		protected virtual AleRecord CreateCompanyAleRecord()
		{
			PRAcaCompanyYearlyInformation companyYear = AcaCompanyYearlyInformation.SelectSingle();
			if (companyYear != null)
			{
				string isDesignatedGovernmentEntity = CompanyTaxAttribute.SelectSingle(AatrixField.ALE.IsDesignatedGovernmentEntity)?.Value;

				bool offersMinimumCoverage = false;
				foreach (IGrouping<int, PXResult<PRDeductCode, PRAcaDeductCoverageInfo>> dedResult in AcaDeductions
					.Select()
					.Select(x => (PXResult<PRDeductCode, PRAcaDeductCoverageInfo>)x)
					.GroupBy(x => ((PRDeductCode)x).CodeID.Value))
				{
					if (AcaOfferOfCoverage.MeetsMinimumCoverageRequirement(dedResult.Select(x => (PRAcaDeductCoverageInfo)x)))
					{
						offersMinimumCoverage = true;
						break;
					}
				}

				AleRecord record = new AleRecord()
				{
					IsDesignatedGovernmentEntity = string.IsNullOrEmpty(isDesignatedGovernmentEntity) ? new bool?() : bool.Parse(isDesignatedGovernmentEntity),
					IsAggregateGroupMember = companyYear.IsPartOfAggregateGroup,
					IsSelfInsured = false,
					UsesCoeQualifyingOfferMethod = true,
					UsesCoe98PctMethod = true,
					MecIndicator = offersMinimumCoverage,
					IsAuthoritativeTransmittal = companyYear.IsAuthoritativeTransmittal
				};

				foreach (PRAcaCompanyMonthlyInformation companyMonth in AcaCompanyMonthlyInformation.Select())
				{
					int monthIndex = companyMonth.Month.Value - 1;

					if (companyMonth.SelfInsured == true)
					{
						record.IsSelfInsured = true;
					}

					if (companyMonth.CertificationOfEligibility != AcaCertificationOfEligibility.QualifyingOfferMethod)
					{
						record.UsesCoeQualifyingOfferMethod = false;
					}

					if (companyMonth.CertificationOfEligibility != AcaCertificationOfEligibility.NinetyEightPctMethod)
					{
						record.UsesCoe98PctMethod = false;
					}

					if (companyMonth.NumberOfFte != null)
					{
						record.FteCount[monthIndex] = companyMonth.NumberOfFte.Value;
					}

					if (companyMonth.NumberOfEmployees != null)
					{
						record.EmployeeCount[monthIndex] = companyMonth.NumberOfEmployees.Value;
					}
				}

				return record;
			}

			return null;
		}

		protected virtual DgeRecord CreateCompanyDgeRecord(CmpRecord cmp)
		{
			Contact contact = OrgContact.SelectSingle();

			DgeRecord record = new DgeRecord(cmp.Name, cmp.Ein)
			{
				AddressLine1 = cmp.AddressLine1,
				AddressLine2 = cmp.AddressLine2,
				City = cmp.City,
				StateAbbr = cmp.StateAbbr,
				ZipCode = cmp.ZipCode,
				NonUSState = cmp.NonUSState,
				NonUSPostalCode = cmp.NonUSPostalCode,
				Country = cmp.Country,
				CountryAbbr = cmp.CountryAbbr,
				ContactFirstName = contact?.FirstName,
				ContactPhoneNumber = contact?.Phone1,
				ContactPhoneExtension = contact?.Phone2,
				ContactMiddleName = contact?.MidName,
				ContactLastName = contact?.LastName
			};

			return record;
		}

		protected virtual IEnumerable<AggRecord> CreateCompanyAggRecords()
		{
			foreach (PRAcaAggregateGroupMember member in AcaAggregateGroupMembers.Select())
			{
				AggRecord record = new AggRecord(member.HighestMonthlyFteNumber)
				{
					MemberName = member.MemberCompanyName.TrimEnd(),
					MemberEin = member.MemberEin
				};

				yield return record;
			}
		}

		protected virtual IEnumerable<EmpRecord> CreateEmpRecords(DatRecord reportPeriod)
		{
			foreach (PXResult<EPEmployee, Contact, Address, Country, PREmployeeDirectDeposit> employeeResult 
				in PeriodEmployees.Select(reportPeriod.FirstDate, reportPeriod.LastDate, reportPeriod.FirstDate, reportPeriod.LastDate))
			{
				EPEmployee employee = (EPEmployee)employeeResult;
				PeriodEmployees.Current = employee;

				Contact contactInfo = (Contact)employeeResult;
				Address address = (Address)employeeResult;
				Country country = (Country)employeeResult;
				PREmployeeDirectDeposit dd = (PREmployeeDirectDeposit)employeeResult;

				var payrollEmployeeResult = (PXResult<PREmployee, PREmployeeClass, PRPayGroup, PRPayGroupYearSetup>)PayrollEmployee.Select()?[0];
				PREmployee prEmployee = (PREmployee)payrollEmployeeResult;
				PREmployeeClass employeeClass = (PREmployeeClass)payrollEmployeeResult;
				PRPayGroupYearSetup payGroupSetup = (PRPayGroupYearSetup)payrollEmployeeResult;

				EmploymentDates employmentDates = EmploymentHistoryHelper.GetEmploymentDates(this, prEmployee.BAccountID, reportPeriod.LastDate);

				int? regularWageType = PRTypeSelectorAttribute.GetDefaultID<PRWage>();

				string title = EmployeeTitles.SelectSingle(reportPeriod.LastDate, reportPeriod.LastDate)?.Description;
				PMWorkCode workCode = EmployeeWorkCode.SelectSingle();

				PXResultset<PREmployeeAttribute> employeeAttributes = EmployeeAttributes.Select();
				string IsFemaleAsStr = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.IsFemale)?.Value;
				string IsDisableAsStr = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.IsDisabled)?.Value;
				string FederalExemptionsAsStr = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.FederalExemptions)?.Value;
				string IsFullTimeAsStr = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.IsFullTime)?.Value;
				string HasHealthBenefitsAsStr = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.HasHealthBenefits)?.Value;
				string IsSeasonalAsStr = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.IsSeasonal)?.Value;
				string IsStatutoryEmployeeAsStr = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.IsStatutoryEmployee)?.Value;
				string HasRetirementPlanAsStr = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.HasRetirementPlan)?.Value;
				string HasThirdPartySickPayAsStr = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.HasThirdPartySickPay)?.Value;
				string HasElectronicW2AsStr = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.HasElectronicW2)?.Value;

				bool exemptFromCertifiedReporting = prEmployee.ExemptFromCertifiedReporting == true;

				EmpRecord record = new EmpRecord(employee.AcctCD.TrimEnd())
				{
					FirstName = contactInfo.FirstName,
					MiddleName = contactInfo.MidName,
					LastName = contactInfo.LastName,
					SocialSecurityNumber = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.SocialSecurityNumber)?.Value,
					AddressLine1 = address?.AddressLine1,
					City = address?.City,
					StateAbbr = address?.State,
					ZipCode = address?.PostalCode,
					Country = country?.Description,
					CountryAbbr = address?.CountryID,
					NonUSPostalCode = address?.PostalCode,
					IsFemale = string.IsNullOrEmpty(IsFemaleAsStr) ? new bool?() : bool.Parse(IsFemaleAsStr),
					IsDisabled = string.IsNullOrEmpty(IsDisableAsStr) ? new bool?() : bool.Parse(IsDisableAsStr),
					HireDate = employmentDates.InitialHireDate,
					FireDate = employmentDates.TerminationDateAuf,
					BirthDate = contactInfo.DateOfBirth,
					PayRate = EmployeeEarningRates.SelectSingle(reportPeriod.LastDate, reportPeriod.LastDate, regularWageType)?.PayRate.Value,
					FederalExemptions = string.IsNullOrEmpty(FederalExemptionsAsStr) ? new int?() : int.Parse(FederalExemptionsAsStr),
					IsHourlyPay = (prEmployee.EmpTypeUseDflt == true ? employeeClass.EmpType : prEmployee.EmpType) == EmployeeType.Hourly,
					IsFullTime = string.IsNullOrEmpty(IsFullTimeAsStr) ? new bool?() : bool.Parse(IsFullTimeAsStr),
					Title = title,
					StateOfHireAbbr = OrgAddress.SelectSingle()?.State,
					WorkType = workCode?.Description,
					HasHealthBenefits = string.IsNullOrEmpty(HasHealthBenefitsAsStr) ? new bool?() : bool.Parse(HasHealthBenefitsAsStr),
					PhoneNumber = contactInfo.Phone1,
					IsSeasonal = string.IsNullOrEmpty(IsSeasonalAsStr) ? new bool?() : bool.Parse(IsSeasonalAsStr),
					WorkersCompClass = workCode?.WorkCodeID,
					MaritalStatus = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.MaritalStatus)?.Value,
					EmployeeID = employee.AcctCD.TrimEnd(),
					IsStatutoryEmployee = string.IsNullOrEmpty(IsStatutoryEmployeeAsStr) ? new bool?() : bool.Parse(IsStatutoryEmployeeAsStr),
					HasRetirementPlan = string.IsNullOrEmpty(HasRetirementPlanAsStr) ? new bool?() : bool.Parse(HasRetirementPlanAsStr),
					HasThirdPartySickPay = string.IsNullOrEmpty(HasThirdPartySickPayAsStr) ? new bool?() : bool.Parse(HasThirdPartySickPayAsStr),
					HasDirectDeposit = dd.BankAcctNbr != null,
					AddressLine2 = address?.AddressLine2,
					Email = contactInfo.EMail,
					HasElectronicW2 = string.IsNullOrEmpty(HasElectronicW2AsStr) ? new bool?() : bool.Parse(HasElectronicW2AsStr),
					NonUSState = address?.State,
					RehireDate = employmentDates.RehireDateAuf,
					EmploymentCode = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.EmploymentCode)?.Value,
					FullOccupationalTitle = title,
					NonUSNationalID = null,
					CppExempt = false,
					EmploymentInsuranceExempt = false,
					ProvincialParentalInsurancePlanExempt = false,
					StateExemptions = employeeAttributes.FirstTableItems
						.Where(x => x.AatrixMapping == AatrixField.EMP.StateExemptions && x.State == Report.Current.State)
						.Sum(x => x.Value == null ? 0 : int.Parse(x.Value)),
					Ethnicity = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.EMP.Ethnicity)?.Value,

					// Children records
					GenList = CreateGeneralEmployeeRecords(reportPeriod, payGroupSetup.PeriodType == PayPeriodType.Week, exemptFromCertifiedReporting).OrderBy(x => x.CheckDate).ToList()
				};

				if (ShouldGenerateCertifiedRecords() && !exemptFromCertifiedReporting)
				{
					record.EfbList = CreateEmployeeCertifiedBenefitRecords(reportPeriod).OrderBy(x => x.BenefitID).ToList();
				}

				if (ShouldGenerateAcaRecords())
				{
					(record.Ecv, record.Eci) = CreateEmployeeAcaRecords(record);
				}

				yield return record;
			}
		}

		protected virtual IEnumerable<CjiRecord> CreateCompanyJobRecords(DatRecord reportPeriod)
		{
			foreach (PXResult<PMProject, PMAddress> projectResult
				in PeriodProjects.Select(reportPeriod.FirstDate, reportPeriod.LastDate))
			{
				PMProject project = projectResult;
				PMAddress projectAddress = projectResult;
				PeriodProjects.Current = project;


				decimal? completedPercent = ProjectTasks.Select().ToList().Average(x => ProjectEntry.GetTaskCompletionPercentage(this, x));

				var record = new CjiRecord(project.ContractID.Value)
				{
					ProjectName = project.Description,
					ProjectNumber = project.ContractCD.TrimEnd(),
					ProjectAddress = ConcatAddress(projectAddress),
					ContractAmount = ProjectTotalBudget.SelectSingle()?.CuryActualAmount,
					EstimatedCompletionDate = project.ExpireDate,
					EstimatedPercentComplete = completedPercent == null ? new int?() : (int)Math.Round(completedPercent.Value),
					TypeOfWork = project.Description,
					ProjectCity = projectAddress?.City,
					ProjectStateAbbr = projectAddress?.State,
					ProjectZipCode = projectAddress?.PostalCode
				};

				yield return record;
			}
		}

		protected virtual IEnumerable<GenRecord> CreateGeneralEmployeeRecords(DatRecord reportPeriod, bool isWeeklyEmployee, bool exemptFromCertifiedReporting)
		{
			string FicaTaxUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.Fica).Key;
			string ERFicaTaxUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.EmployerFica).Key;
			string MedicareUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.Medicare).Key;
			string ERMedicareUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.EmployerMedicare).Key;
			string AdditionalMedicareUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.AdditionalMedicare).Key;
			string FitUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.Fit).Key;
			string FutaUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.Futa).Key;

			foreach (var paymentResult in EmployeePayments
				.Select(reportPeriod.FirstDate, reportPeriod.LastDate, reportPeriod.FirstDate, reportPeriod.LastDate)
				.Select(x => (PXResult<PRPayment, PRPaymentTaxSplit, PRTaxCode>)x)
				.GroupBy(x => new { ((PRPayment)x).DocType, ((PRPayment)x).RefNbr }))
			{
				PRPayment payment = (PRPayment)paymentResult.First();
				EmployeePayments.Current = payment;

				var record = new GenRecord(payment.TransactionDate.Value)
				{
					GrossPay = payment.GrossAmount,
					NetPay = payment.NetAmount,
					SSWages = 0m,
					SSWithheld = 0m,
					MedicareWages = 0m,
					MedicareWithheld = 0m,
					FederalWages = 0m,
					FederalWithheld = 0m,
					TaxableFutaWages = 0m,
					SSTips = 0m,
					FutaLiability = 0m,
					TotalFutaWages = 0m,
					PeriodStart = payment.StartDate,
					PeriodEnd = payment.EndDate,
					SSEmployerMatch = 0m,
					MedicareEmployerMatch = 0m,
					AdditionalMedicareTax = 0m,
					AdditionalMedicareWages = 0m,

					// Children records
					EsiList = CreateEsiRecords().OrderBy(x => x.PimID).ToList(),
					EliList = CreateTaxEliRecords().OrderBy(x => x.PimID).ToList()
				};

				foreach (PXResult<PRPayment, PRPaymentTaxSplit, PRTaxCode> taxResult in paymentResult)
				{
					if ((PRPaymentTaxSplit)taxResult != null && (PRTaxCode)taxResult != null)
					{
						if (((PRTaxCode)taxResult).TaxUniqueCode == FicaTaxUniqueID && PRTypeSelectorAttribute.GetAatrixMapping<PRWage>(((PRPaymentTaxSplit)taxResult).WageType.GetValueOrDefault()) != AatrixMiscInfo.IsTipWageType)
						{
							record.SSWages += ((PRPaymentTaxSplit)taxResult).WageBaseAmount;
						}
						if (((PRTaxCode)taxResult).TaxUniqueCode == FicaTaxUniqueID)
						{
							record.SSWithheld += ((PRPaymentTaxSplit)taxResult).TaxAmount;
						}
						if (((PRTaxCode)taxResult).TaxUniqueCode == MedicareUniqueID)
						{
							record.MedicareWages += ((PRPaymentTaxSplit)taxResult).WageBaseAmount;
							record.MedicareWithheld += ((PRPaymentTaxSplit)taxResult).TaxAmount;
						}
						if (((PRTaxCode)taxResult).TaxUniqueCode == FitUniqueID)
						{
							record.FederalWages += ((PRPaymentTaxSplit)taxResult).WageBaseAmount;
							record.FederalWithheld += ((PRPaymentTaxSplit)taxResult).TaxAmount;
						}
						if (((PRTaxCode)taxResult).TaxUniqueCode == FutaUniqueID)
						{
							record.TaxableFutaWages += ((PRPaymentTaxSplit)taxResult).WageBaseAmount;
							record.FutaLiability += ((PRPaymentTaxSplit)taxResult).TaxAmount;
							record.TotalFutaWages += ((PRPaymentTaxSplit)taxResult).WageBaseGrossAmt;
						}
						if (((PRTaxCode)taxResult).TaxUniqueCode == FicaTaxUniqueID && PRTypeSelectorAttribute.GetAatrixMapping<PRWage>(((PRPaymentTaxSplit)taxResult).WageType.GetValueOrDefault()) == AatrixMiscInfo.IsTipWageType)
						{
							record.SSTips += ((PRPaymentTaxSplit)taxResult).WageBaseAmount;
						}
						if (((PRTaxCode)taxResult).TaxUniqueCode == ERFicaTaxUniqueID)
						{
							record.SSEmployerMatch += ((PRPaymentTaxSplit)taxResult).TaxAmount;
						}
						if (((PRTaxCode)taxResult).TaxUniqueCode == ERMedicareUniqueID)
						{
							record.MedicareEmployerMatch += ((PRPaymentTaxSplit)taxResult).TaxAmount;
						}
						if (((PRTaxCode)taxResult).TaxUniqueCode == AdditionalMedicareUniqueID)
						{
							record.AdditionalMedicareTax += ((PRPaymentTaxSplit)taxResult).TaxAmount;
							record.AdditionalMedicareWages += ((PRPaymentTaxSplit)taxResult).WageBaseAmount;
						}
					}
				}

				if (isWeeklyEmployee && ShouldGenerateCertifiedRecords() && !exemptFromCertifiedReporting)
				{
					record.EjwList = CreateJobWeekRecords(record, paymentResult).OrderBy(x => x.JobID).ToList();
				}

				yield return record;
			}
		}

		protected virtual IEnumerable<GtoRecord> CreateGeneralCompanyRecords(IEnumerable<EmpRecord> empList)
		{
			foreach (var paymentGroup in empList.SelectMany(x => x.GenList).GroupBy(x => new { x.PeriodStart, x.PeriodEnd, x.CheckDate }))
			{
				GtoRecord record = new GtoRecord(paymentGroup.Key.CheckDate)
				{
					GrossPay = 0m,
					NetPay = 0m,
					SSWages = 0m,
					SSLiability = 0m,
					MedicareWages = 0m,
					FederalWages = 0m,
					FederalWHLiability = 0m,
					TaxableFutaWages = 0m,
					FutaLiability = 0m,
					SSTips = 0m,
					TotalFutaWages = 0m,
					PeriodStart = paymentGroup.Key.PeriodStart,
					PeriodEnd = paymentGroup.Key.PeriodEnd,
					SSEmployerMatch = 0m,
					MedicareEmployerMatch = 0m,
					AdditionalMedicareTax = 0m,
					AdditionalMedicareWages = 0m,

					// Children records
					CsiList = CreateCsiRecords(empList, paymentGroup.Key.PeriodStart.Value, paymentGroup.Key.PeriodEnd.Value, paymentGroup.Key.CheckDate)
						.OrderBy(x => x.PimID)
						.ToList(),
					CliList = CreateCliRecords(empList, paymentGroup.Key.PeriodStart.Value, paymentGroup.Key.PeriodEnd.Value, paymentGroup.Key.CheckDate)
						.OrderBy(x => x.PimID)
						.ToList(),
					CspList = CreateCspList(paymentGroup.Key.CheckDate).ToList(),
					ClpList = CreateClpList(paymentGroup.Key.CheckDate).ToList()
				};

				foreach (GenRecord general in paymentGroup)
				{
					record.GrossPay += general.GrossPay;
					record.NetPay += general.NetPay;
					record.SSWages += general.SSWages;
					record.SSLiability += general.SSWithheld + general.SSEmployerMatch;
					record.MedicareWages += general.MedicareWages;
					record.FederalWages += general.FederalWages;
					record.FederalWHLiability += general.FederalWithheld;
					record.TaxableFutaWages += general.TaxableFutaWages;
					record.FutaLiability += general.FutaLiability;
					record.SSTips += general.SSTips;
					record.TotalFutaWages += general.TotalFutaWages;
					record.SSEmployerMatch += general.SSEmployerMatch;
					record.MedicareEmployerMatch += general.MedicareEmployerMatch;
					record.AdditionalMedicareTax += general.AdditionalMedicareTax;
					record.AdditionalMedicareWages += general.AdditionalMedicareWages;
				}

				yield return record;
			}
		}

		protected virtual IEnumerable<EjwRecord> CreateJobWeekRecords(GenRecord general, IEnumerable<PXResult<PRPayment, PRPaymentTaxSplit, PRTaxCode>> taxSplits)
		{
			Dictionary<int, EjwRecord> records = new Dictionary<int, EjwRecord>();

			IEnumerable<PXResult<PREarningDetail, EPEarningType, PMProject, PMWorkCode>> earningResults = 
				EmployeePaycheckHourlyEarnings.Select().Select(x => (PXResult<PREarningDetail, EPEarningType, PMProject, PMWorkCode>)x);

			decimal totalHoursDay1 = 0m;
			decimal totalHoursDay2 = 0m;
			decimal totalHoursDay3 = 0m;
			decimal totalHoursDay4 = 0m;
			decimal totalHoursDay5 = 0m;
			decimal totalHoursDay6 = 0m;
			decimal totalHoursDay7 = 0m;
			foreach (PREarningDetail earningDetail in earningResults)
			{
				if (earningDetail.Date.Value.Date == general.PeriodStart.Value.Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay1 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(1, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay2 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(2, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay3 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(3, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay4 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(4, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay5 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(5, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay6 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(6, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay7 += earningDetail.Hours.GetValueOrDefault();
				}
			}

			foreach (PXResult<PREarningDetail, EPEarningType, PMProject, PMWorkCode> earningResult in
				earningResults.Where(x => ((PREarningDetail)x).CertifiedJob == true && ((PMProject)x).ContractID != null))
			{
				PREarningDetail earningDetail = (PREarningDetail)earningResult;
				EPEarningType earningType = (EPEarningType)earningResult;
				PMProject project = (PMProject)earningResult;
				PMWorkCode workCode = (PMWorkCode)earningResult;

				CjiRecord itemMap = _JobItemMapping.FirstOrDefault(x => x.JobID == project.ContractID);
				if (itemMap != null)
				{
					EjwRecord record;
					bool recordExists = records.TryGetValue(project.ContractID.Value, out record);
					if (!recordExists)
					{
						record = new EjwRecord(project.ContractID.Value, general.PeriodEnd.Value)
						{
							WorkClassification = workCode.Description,
							JobGross = earningDetail.Amount,
							TotalGross = general.GrossPay,
							RegularHoursDay1 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 0, false),
							RegularHoursDay2 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 1, false),
							RegularHoursDay3 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 2, false),
							RegularHoursDay4 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 3, false),
							RegularHoursDay5 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 4, false),
							RegularHoursDay6 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 5, false),
							RegularHoursDay7 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 6, false),
							OvertimeHoursDay1 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 0, true),
							OvertimeHoursDay2 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 1, true),
							OvertimeHoursDay3 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 2, true),
							OvertimeHoursDay4 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 3, true),
							OvertimeHoursDay5 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 4, true),
							OvertimeHoursDay6 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 5, true),
							OvertimeHoursDay7 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 6, true),
							CheckNumber = EmployeePayments.Current.RefNbr,
							WorkClassificationCode = workCode.WorkCodeID,
							CheckDate = general.CheckDate,
							TotalHoursDay1 = totalHoursDay1,
							TotalHoursDay2 = totalHoursDay2,
							TotalHoursDay3 = totalHoursDay3,
							TotalHoursDay4 = totalHoursDay4,
							TotalHoursDay5 = totalHoursDay5,
							TotalHoursDay6 = totalHoursDay6,
							TotalHoursDay7 = totalHoursDay7
						};

						records[project.ContractID.Value] = record;
					}
					else
					{
						record.JobGross += earningDetail.Amount;
						record.RegularHoursDay1 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 0, false);
						record.RegularHoursDay2 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 1, false);
						record.RegularHoursDay3 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 2, false);
						record.RegularHoursDay4 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 3, false);
						record.RegularHoursDay5 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 4, false);
						record.RegularHoursDay6 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 5, false);
						record.RegularHoursDay7 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 6, false);
						record.OvertimeHoursDay1 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 0, true);
						record.OvertimeHoursDay2 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 1, true);
						record.OvertimeHoursDay3 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 2, true);
						record.OvertimeHoursDay4 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 3, true);
						record.OvertimeHoursDay5 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 4, true);
						record.OvertimeHoursDay6 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 5, true);
						record.OvertimeHoursDay7 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 6, true);
					}
				}
			}

			foreach (EjwRecord record in records.Values)
			{
				record.FederalWithholding = general.FederalWithheld;

					record.StateWithholding = 0m;
					record.Sui = 0m;
					record.Sdi = 0m;
					foreach (PXResult<PRPayment, PRPaymentTaxSplit, PRTaxCode> taxSplit in taxSplits)
					{
						if (_TaxIDMapping.Where(kvp => kvp.Value.Field == AatrixTax.Sit).Select(y => y.Key).Contains(((PRTaxCode)taxSplit).TaxUniqueCode) && ((PRTaxCode)taxSplit).TaxState == Report.Current.State)
						{
						record.StateWithholding += ((PRPaymentTaxSplit)taxSplit).TaxAmount;
						}
						if (_TaxIDMapping.Where(kvp => kvp.Value.Field == AatrixTax.Sui).Select(y => y.Key).Contains(((PRTaxCode)taxSplit).TaxUniqueCode) && ((PRTaxCode)taxSplit).TaxState == Report.Current.State)
						{
						record.Sui += ((PRPaymentTaxSplit)taxSplit).TaxAmount;
						}
						if (_TaxIDMapping.Where(kvp => kvp.Value.Field == AatrixTax.Sdi).Select(y => y.Key).Contains(((PRTaxCode)taxSplit).TaxUniqueCode) && ((PRTaxCode)taxSplit).TaxState == Report.Current.State)
						{
						record.Sdi += ((PRPaymentTaxSplit)taxSplit).TaxAmount;
						}
					}

				record.SSWithholding = general.SSWithheld;
				record.MedicareWithholding = general.MedicareWithheld;
				record.OtherDeductions = general.GrossPay - general.NetPay -
						record.FederalWithholding - record.StateWithholding - record.Sui - record.Sdi - record.SSWithholding - record.MedicareWithholding;

				IEnumerable<PXResult<PREarningDetail, EPEarningType, PMProject, PMWorkCode>> jobEarnings = earningResults
					.Where(x => ((PREarningDetail)x).CertifiedJob == true && ((PMProject)x).ContractID == record.JobID);

				decimal jobTotalRegularHours = (record.RegularHoursDay1 + record.RegularHoursDay2 + record.RegularHoursDay3 + record.RegularHoursDay4 +
					record.RegularHoursDay5 + record.RegularHoursDay6 + record.RegularHoursDay7).Value;
				decimal jobTotalOvertimeHours = (record.OvertimeHoursDay1 + record.OvertimeHoursDay2 + record.OvertimeHoursDay3 + record.OvertimeHoursDay4 +
					record.OvertimeHoursDay5 + record.OvertimeHoursDay6 + record.OvertimeHoursDay7).Value;

				decimal jobTotalRegularWages = 0m;				
				decimal jobTotalRegularFringe = 0m;
				decimal jobTotalOvertimeWages = 0m;
				decimal jobTotalOvertimeFringe = 0m;
				foreach (PXResult<PREarningDetail, EPEarningType, PMProject, PMWorkCode> jobEarning in jobEarnings)
				{
					PREarningDetail earningDetail = jobEarning;
					EPEarningType earningType = jobEarning;

					if (earningType.IsOvertime != true)
					{
						if (earningDetail.IsFringeRateEarning == true)
						{
							jobTotalRegularFringe += earningDetail.Amount.GetValueOrDefault();
						}
						else
						{
							jobTotalRegularWages += earningDetail.Amount.GetValueOrDefault();
						}
					}
					else
					{
						if (earningDetail.IsFringeRateEarning == true)
					{
							jobTotalOvertimeFringe += earningDetail.Amount.GetValueOrDefault();
					}
						else
					{
							jobTotalOvertimeWages += earningDetail.Amount.GetValueOrDefault();
						}
					}
				}

				if (jobTotalRegularHours > 0)
				{
					record.RegularHourlyRate = jobTotalRegularWages / jobTotalRegularHours;
					record.CashFringeRegularHourlyRate = jobTotalRegularFringe / jobTotalRegularHours;
				}

				if (jobTotalOvertimeHours > 0)
				{
					record.OvertimeHourlyRate = jobTotalOvertimeWages / jobTotalOvertimeHours;
					record.CashFringeOvertimeHourlyRate = jobTotalOvertimeFringe / jobTotalOvertimeHours;
				}

				// Prevailing
				record.IsPrevailingWage = false;
				bool allSameLabourItem = jobEarnings.GroupBy(x => ((PREarningDetail)x).LabourItemID).Count() == 1;
				bool allSameRate = jobEarnings.Where(x => ((EPEarningType)(PXResult<PREarningDetail, EPEarningType>)x).IsOvertime != true && ((PREarningDetail)x).IsFringeRateEarning != true).GroupBy(x => ((PREarningDetail)x).Rate).Count() == 1;
				if (allSameLabourItem && allSameRate)
				{
					PMLaborCostRate prevailingWage = PrevailingWage.SelectSingle(jobEarnings.Select(x => ((PREarningDetail)x).LabourItemID).First(), record.JobID, record.CheckDate, jobEarnings.Select(x => ((PREarningDetail)x).ProjectTaskID).First());
					record.IsPrevailingWage = prevailingWage?.Rate == record.RegularHourlyRate;
				}
			}

			return records.Values;
		}

		protected virtual IEnumerable<PimRecord> CreatePimRecords()
		{
			return CreateEarningPimRecords()
				.Union(CreateBenefitPimRecords(), new PimRecordComparer())
				.Union(CreateTaxPimRecords(), new PimRecordComparer());
		}

		protected virtual IEnumerable<PimRecord> CreateEarningPimRecords()
		{
			foreach (PRTypeMeta prType in PRReportingTypeSelectorAttribute.GetAll<PRWage>().Where(x => x.AatrixMapping?.IsDirectMapping == true))
			{
				PimRecord record = new PimRecord(prType.Name)
				{
					Description = prType.Description,
					AatrixTaxType = prType.ID
				};

				yield return record;
			}
		}

		protected virtual IEnumerable<PimRecord> CreateBenefitPimRecords()
		{
			foreach (PRTypeMeta prType in PRReportingTypeSelectorAttribute.GetAll<PRBenefit>().Where(x => x.AatrixMapping?.IsDirectMapping == true))
			{
				PimRecord record = new PimRecord(prType.Name)
				{
					Description = prType.Description,
					AatrixTaxType = prType.ID
				};

				yield return record;
			}

			foreach (KeyValuePair<string, SymmetryToAatrixTaxMapping> kvp in _TaxIDMapping.Where(kvp => kvp.Value.IsWorkersCompensation && kvp.Value.State != null))
			{
				int aatrixID = kvp.Value.TaxItemMappings.First().aatrixID;
				PimRecord record = new PimRecord(kvp.Key, aatrixID)
				{
					Description = PXMessages.LocalizeFormatNoPrefix(Messages.WorkersCompensationFormat, kvp.Value.State.Abbr),
					AatrixTaxType = aatrixID
				};

				_WCMapping[kvp.Value.State.Abbr] = record.PimID;

				yield return record;
			}
		}

		protected virtual IEnumerable<PimRecord> CreateTaxPimRecords()
		{
			foreach (PRTaxCode tax in TaxCodes.Select())
			{
				if (_TaxIDMapping[tax.TaxUniqueCode]?.TaxItemMappings != null)
				{
					foreach (int aatrixTaxID in _TaxIDMapping[tax.TaxUniqueCode].TaxItemMappings.Select(x => x.aatrixID).Except(_WCMapping.Values))
					{
						PimRecord record = new PimRecord(tax.TaxCD.TrimEnd(), aatrixTaxID)
						{
							Description = tax.Description,
							AatrixTaxType = aatrixTaxID,
							State = tax.TaxState,
							AccountNumber = tax.GovtRefNbr
						};

						yield return record;
					}
				}
			}
		}

		protected virtual IEnumerable<CsiRecord> CreateCsiRecords(IEnumerable<EmpRecord> empList, DateTime periodStart, DateTime periodEnd, DateTime checkDate)
		{
			foreach (IGrouping<int, EsiRecord> esiGroup in empList
				.SelectMany(x => x.GenList)
				.Where(x => x.PeriodStart == periodStart && x.PeriodEnd == periodEnd && x.CheckDate == checkDate)
				.SelectMany(x => x.EsiList)
				.GroupBy(x => x.PimID))
			{
				CsiRecord record = new CsiRecord(checkDate, esiGroup.Key, esiGroup.First().State, periodStart, periodEnd);
				foreach (EsiRecord esi in esiGroup)
				{
					record.TotalWagesAndTips += esi.TotalWagesAndTips;
					record.TaxableWagesAndTips += esi.TaxableWagesAndTips;
					record.TaxableTips += esi.TaxableTips;
					record.WithholdingAmount += esi.WithholdingAmount;
					record.Hours += esi.Hours;
				}

				if (record.TaxableWagesAndTips > 0)
				{
					record.Rate = record.WithholdingAmount / record.TaxableWagesAndTips * 100;
				}

				yield return record;
			}
		}

		protected virtual IEnumerable<CliRecord> CreateCliRecords(IEnumerable<EmpRecord> empList, DateTime periodStart, DateTime periodEnd, DateTime checkDate)
		{
			foreach (IGrouping<int, EliRecord> eliGroup in empList
				.SelectMany(x => x.GenList)
				.Where(x => x.PeriodStart == periodStart && x.PeriodEnd == periodEnd && x.CheckDate == checkDate)
				.SelectMany(x => x.EliList)
				.GroupBy(x => x.PimID))
			{
				CliRecord record = new CliRecord(checkDate, eliGroup.Key, eliGroup.First().State, periodStart, periodEnd);
				foreach (EliRecord eli in eliGroup)
				{
					record.TotalWagesAndTips += eli.TotalWagesAndTips;
					record.TaxableWagesAndTips += eli.TaxableWagesAndTips;
					record.TaxableTips += eli.TaxableTips;
					record.WithholdingAmount += eli.WithholdingAmount;
					record.Hours += eli.Hours;
				}

				if (record.TaxableWagesAndTips > 0)
				{
					record.Rate = record.WithholdingAmount / record.TaxableWagesAndTips * 100;
				}

				yield return record;
			}
		}

		protected virtual IEnumerable<EsiRecord> CreateEsiRecords()
		{
			Dictionary<int, EsiRecord> esiRecords = new Dictionary<int, EsiRecord>();
			CreateEarningEsiRecords(ref esiRecords);
			CreateBenefitEsiRecords(ref esiRecords);
			CreateTaxEsiRecords(ref esiRecords);
			CreateWCEsiRecords(ref esiRecords);
			return esiRecords.Values;
		}

		protected virtual void CreateEarningEsiRecords(ref Dictionary<int, EsiRecord> records)
		{
			PXResultset<PREmployeeAttribute> employeeAttributes = EmployeeAttributes.Select();
			PXResultset<PRPaymentEarning> paymentEarnings = PaymentEarnings.Select();

			Dictionary<int, List<PXResult<PRPaymentEarning, EPEarningType>>> earningsByReportType = new Dictionary<int, List<PXResult<PRPaymentEarning, EPEarningType>>>();
			foreach (PXResult<PRPaymentEarning, EPEarningType> result in paymentEarnings)
			{
				PRPaymentEarning earning = (PRPaymentEarning)result;
				PREarningType earningType = ((EPEarningType)result).GetExtension<PREarningType>();

				if (earning.Amount > 0 && earningType.ReportType.HasValue)
				{
					List<PXResult<PRPaymentEarning, EPEarningType>> earningsForReportType;
					if (!earningsByReportType.TryGetValue(earningType.ReportType.Value, out earningsForReportType))
					{
						earningsForReportType = new List<PXResult<PRPaymentEarning, EPEarningType>>();
						earningsByReportType[earningType.ReportType.Value] = earningsForReportType;
					}
					earningsForReportType.Add(result);
				}
			}

			foreach (PRTypeMeta prType in PRReportingTypeSelectorAttribute.GetAll<PRWage>().Where(x => x.AatrixMapping?.IsDirectMapping == true))
			{
				List<PXResult<PRPaymentEarning, EPEarningType>> earningsForReportType;
				if (earningsByReportType.TryGetValue(prType.ID, out earningsForReportType))
				{
					int pimID = Math.Abs(prType.Name.GetHashCode());
					if (!records.TryGetValue(pimID, out EsiRecord record))
					{
						record = new EsiRecord(EmployeePayments.Current.TransactionDate.Value, pimID, null, EmployeePayments.Current.StartDate, EmployeePayments.Current.EndDate)
						{
							Allowances = decimal.Parse(employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.ESI.PuertoRicoTotalAllowances)?.Value ?? "0.0")
						};
						records[pimID] = record;
					}

					record.WithholdingAmount += earningsForReportType.Sum(x => ((PRPaymentEarning)x).Amount);
					foreach (PXResult<PRPaymentEarning, EPEarningType> paymentEarning in paymentEarnings)
					{
						record.TotalWagesAndTips += ((PRPaymentEarning)paymentEarning).Amount;
						record.TaxableWagesAndTips += ((PRPaymentEarning)paymentEarning).Amount;
						if (PRTypeSelectorAttribute.GetAatrixMapping<PRWage>(((EPEarningType)paymentEarning).GetExtension<PREarningType>().WageTypeCD.GetValueOrDefault()) == AatrixMiscInfo.IsTipWageType)
						{
							record.TaxableTips += ((PRPaymentEarning)paymentEarning).Amount;
						}
						record.Hours += ((PRPaymentEarning)paymentEarning).Hours;
						if (((PRPaymentEarning)paymentEarning).TypeCD == PayrollPreferences.Current.CommissionType)
						{
							record.Commissions += ((PRPaymentEarning)paymentEarning).Amount;
						}
					}
				}
			}
		}

		protected virtual void CreateBenefitEsiRecords(ref Dictionary<int, EsiRecord> records)
		{
			PXResultset<PREmployeeAttribute> employeeAttributes = EmployeeAttributes.Select();

			Dictionary<int, decimal> applicableAmountsByBenefitType = new Dictionary<int, decimal>();
			foreach (PXResult<PRPaymentDeduct, PRDeductCode> dedBen in PaymentDeductions.Select())
			{
				PRPaymentDeduct deduct = (PRPaymentDeduct)dedBen;
				PRDeductCode deductCode = (PRDeductCode)dedBen;

				if (deductCode.DedReportType.HasValue)
				{
					PX.Payroll.TaxCategory? reportTypeScope = PRReportingTypeSelectorAttribute.GetReportingTypeScope<PRBenefit>(deductCode.DedReportType.Value);
					if ((reportTypeScope == Payroll.TaxCategory.Employee || reportTypeScope == Payroll.TaxCategory.Any) && deduct.DedAmount > 0)
					{
						if (applicableAmountsByBenefitType.ContainsKey(deductCode.DedReportType.Value))
						{
							applicableAmountsByBenefitType[deductCode.DedReportType.Value] += deduct.DedAmount.Value;
						}
						else
						{
							applicableAmountsByBenefitType[deductCode.DedReportType.Value] = deduct.DedAmount.Value;
						}
					}
				}

				if (deductCode.CntReportType.HasValue)
				{
					PX.Payroll.TaxCategory? reportTypeScope = PRReportingTypeSelectorAttribute.GetReportingTypeScope<PRBenefit>(deductCode.CntReportType.Value);
					if ((reportTypeScope == Payroll.TaxCategory.Employer || reportTypeScope == Payroll.TaxCategory.Any) && deduct.CntAmount > 0)
					{
						if (applicableAmountsByBenefitType.ContainsKey(deductCode.CntReportType.Value))
						{
							applicableAmountsByBenefitType[deductCode.CntReportType.Value] += deduct.CntAmount.Value;
						}
						else
						{
							applicableAmountsByBenefitType[deductCode.CntReportType.Value] = deduct.CntAmount.Value;
						}
					}
				}
			}

			foreach (PRTypeMeta prType in PRReportingTypeSelectorAttribute.GetAll<PRBenefit>().Where(x => x.AatrixMapping?.IsDirectMapping == true))
			{
				decimal dedBenAmount;
				if (applicableAmountsByBenefitType.TryGetValue(prType.ID, out dedBenAmount))
				{
					int pimID = Math.Abs(prType.Name.GetHashCode());
					if (!records.TryGetValue(pimID, out EsiRecord record))
					{
						record = new EsiRecord(EmployeePayments.Current.TransactionDate.Value, pimID, null, EmployeePayments.Current.StartDate, EmployeePayments.Current.EndDate)
						{
							Allowances = decimal.Parse(employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.ESI.PuertoRicoTotalAllowances)?.Value ?? "0.0")
						};
						records[pimID] = record;
					}

					record.WithholdingAmount += dedBenAmount;
					foreach (PXResult<PRPaymentEarning, EPEarningType> paymentEarning in PaymentEarnings.Select())
					{
						record.TotalWagesAndTips += ((PRPaymentEarning)paymentEarning).Amount;
						record.TaxableWagesAndTips += ((PRPaymentEarning)paymentEarning).Amount;
						if (PRTypeSelectorAttribute.GetAatrixMapping<PRWage>(((EPEarningType)paymentEarning).GetExtension<PREarningType>().WageTypeCD.GetValueOrDefault()) == AatrixMiscInfo.IsTipWageType)
						{
							record.TaxableTips += ((PRPaymentEarning)paymentEarning).Amount;
						}
						record.Hours += ((PRPaymentEarning)paymentEarning).Hours;
						if (((PRPaymentEarning)paymentEarning).TypeCD == PayrollPreferences.Current.CommissionType)
						{
							record.Commissions += ((PRPaymentEarning)paymentEarning).Amount;
						}
					}
				}
			}
		}

		protected virtual void CreateTaxEsiRecords(ref Dictionary<int, EsiRecord> records)
		{
			PXResultset<PREmployeeAttribute> employeeAttributes = EmployeeAttributes.Select();
			foreach (PXResult<PRPaymentTaxSplit, PRTaxCode> taxResult in PaymentTaxSplits.Select())
			{
				PRPaymentTaxSplit taxSplit = (PRPaymentTaxSplit)taxResult;
				PRTaxCode tax = (PRTaxCode)taxResult;

				if (tax.JurisdictionLevel == TaxJurisdiction.State && _TaxIDMapping[tax.TaxUniqueCode]?.TaxItemMappings != null)
				{
					foreach ((int aatrixTaxID, bool _) in _TaxIDMapping[tax.TaxUniqueCode].TaxItemMappings)
					{
						if (!records.TryGetValue(aatrixTaxID, out EsiRecord record))
						{
							record = new EsiRecord(EmployeePayments.Current.TransactionDate.Value, aatrixTaxID, tax.TaxState, EmployeePayments.Current.StartDate, EmployeePayments.Current.EndDate)
							{
								Allowances = decimal.Parse(employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.ESI.PuertoRicoTotalAllowances)?.Value ?? "0.0")
							};
							records[aatrixTaxID] = record;
						}

						record.TotalWagesAndTips += taxSplit.WageBaseGrossAmt;
						record.TaxableWagesAndTips += taxSplit.WageBaseAmount;
						if (PRTypeSelectorAttribute.GetAatrixMapping<PRWage>(taxSplit.WageType.GetValueOrDefault()) == AatrixMiscInfo.IsTipWageType)
						{
							record.TaxableTips += taxSplit.WageBaseAmount;
						}
						record.WithholdingAmount += taxSplit.TaxAmount;
						record.Hours += taxSplit.WageBaseHours;
						record.Commissions += taxSplit.SubjectCommissionAmount ?? 0m;
					}
				}
			}
		}

		protected virtual void CreateWCEsiRecords(ref Dictionary<int, EsiRecord> records)
		{
			PXResultset<PREmployeeAttribute> employeeAttributes = EmployeeAttributes.Select();
			foreach (PXResult<PRPaymentWCPremium, PRDeductCode> result in PaymentWCPremiums.Select())
			{
				PRPaymentWCPremium premium = result;
				PRDeductCode deductCode = result;

				if (_WCMapping.TryGetValue(deductCode.State, out int aatrixTaxID))
				{
					if (!records.TryGetValue(aatrixTaxID, out EsiRecord record))
					{
						record = new EsiRecord(EmployeePayments.Current.TransactionDate.Value, aatrixTaxID, deductCode.State, EmployeePayments.Current.StartDate, EmployeePayments.Current.EndDate)
						{
							Allowances = decimal.Parse(employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.ESI.PuertoRicoTotalAllowances)?.Value ?? "0.0")
						};
						records[aatrixTaxID] = record;
					}

					record.TotalWagesAndTips += premium.WageBaseAmount;
					record.TaxableWagesAndTips += premium.WageBaseAmount;
					record.WithholdingAmount += premium.Amount.GetValueOrDefault() + premium.DeductionAmount.GetValueOrDefault();
					record.Hours += premium.WageBaseHours;
				}
			}
		}

		protected virtual IEnumerable<EliRecord> CreateTaxEliRecords()
		{
			PXResultset<PREmployeeTaxAttribute> employeeTaxAttributes = EmployeeTaxAttributes.Select();

			Dictionary<int, EliRecord> records = new Dictionary<int, EliRecord>();
			foreach (PXResult<PRPaymentTaxSplit, PRTaxCode> taxResult in PaymentTaxSplits.Select())
			{
				PRPaymentTaxSplit taxSplit = (PRPaymentTaxSplit)taxResult;
				PRTaxCode tax = (PRTaxCode)taxResult;
				bool isResident = bool.Parse(employeeTaxAttributes.FirstTableItems
					.FirstOrDefault(x => x.AatrixMapping == AatrixField.ELI.IsResidentTax && x.TaxID == tax.TaxID)?.Value ?? "false");

				if (tax.JurisdictionLevel != TaxJurisdiction.State &&
					tax.JurisdictionLevel != TaxJurisdiction.Federal && 
					_TaxIDMapping[tax.TaxUniqueCode]?.TaxItemMappings != null)
				{
					bool residentSplitTax = TaxIsSplitByResidency(tax.TaxUniqueCode);
					foreach ((int aatrixTaxID, bool isResidentTax) in _TaxIDMapping[tax.TaxUniqueCode].TaxItemMappings)
					{
						if (!residentSplitTax || isResident == isResidentTax)
						{
							if (!records.TryGetValue(aatrixTaxID, out EliRecord record))
							{
								record = new EliRecord(EmployeePayments.Current.TransactionDate.Value, aatrixTaxID, tax.TaxState, EmployeePayments.Current.StartDate, EmployeePayments.Current.EndDate);
								records[aatrixTaxID] = record;
							}

							record.TotalWagesAndTips += taxSplit.WageBaseGrossAmt;
							record.TaxableWagesAndTips += taxSplit.WageBaseAmount;
							if (PRTypeSelectorAttribute.GetAatrixMapping<PRWage>(taxSplit.WageType.GetValueOrDefault()) == AatrixMiscInfo.IsTipWageType)
							{
								record.TaxableTips += taxSplit.WageBaseAmount;
							}
							record.WithholdingAmount += taxSplit.TaxAmount;
							record.Hours += taxSplit.WageBaseHours; 
						}
					}
				}
			}

			return records.Values;
		}

		protected virtual IEnumerable<CfbRecord> CreateCompanyCertifiedBenefitRecords(DatRecord reportPeriod)
		{
			HashSet<int?> reportedBenefits = new HashSet<int?>();
			foreach (IGrouping<int?, PXResult<PRPaymentFringeBenefitDecreasingRate, PRPayment, PRDeductCode, BAccount, Address, Contact>> resultGroup in
				PeriodCertifiedBenefits.Select(reportPeriod.FirstDate, reportPeriod.LastDate)
				.Select(x => (PXResult<PRPaymentFringeBenefitDecreasingRate, PRPayment, PRDeductCode, BAccount, Address, Contact>)x)
				.GroupBy(x => ((PRDeductCode)x).CodeID))
			{
				PRDeductCode benefit = resultGroup.First();
				CfbRecord record = CreateCfbRecordWithVendor(benefit, resultGroup.First(), resultGroup.First());
				reportedBenefits.Add(benefit.CodeID);
				yield return record;
			}

			foreach (IGrouping<int?, PXResult<PRPaymentFringeBenefit, PRPayment, PMProject, PRDeductCode, BAccount, Address, Contact>> resultGroup in PeriodFringeDestinationBenefits.Select(reportPeriod.FirstDate, reportPeriod.LastDate)
				.Select(x => (PXResult<PRPaymentFringeBenefit, PRPayment, PMProject, PRDeductCode, BAccount, Address, Contact>)x)
				.Where(x => !reportedBenefits.Contains(((PRDeductCode)x).CodeID))
				.GroupBy(x => ((PRDeductCode)x).CodeID))
			{
				PRDeductCode benefit = resultGroup.First();
				CfbRecord record = CreateCfbRecordWithVendor(benefit, resultGroup.First(), resultGroup.First());
				reportedBenefits.Add(benefit.CodeID);
				yield return record;
			}
		}

		protected virtual CfbRecord CreateCfbRecordWithVendor(PRDeductCode benefit, Address vendorAddress, Contact vendorContact)
		{
			return new CfbRecord(benefit.CodeCD.TrimEnd(), (char)benefit.CertifiedReportType)
				{
					VendorAddress = ConcatAddress(vendorAddress),
					VendorCity = vendorAddress?.City,
					VendorState = vendorAddress?.State,
					VendorZipCode = vendorAddress?.PostalCode,
					VendorPhone = vendorContact?.Phone1
				};
		}

		protected virtual IEnumerable<EfbRecord> CreateEmployeeCertifiedBenefitRecords(DatRecord reportPeriod)
		{
			foreach (var resultGroup in PeriodCertifiedBenefits.Select(reportPeriod.FirstDate, reportPeriod.LastDate)
					.Select(x => (PXResult<PRPaymentFringeBenefitDecreasingRate, PRPayment, PRDeductCode>)x)
					.Where(x => ((PRPayment)x).EmployeeID == PeriodEmployees.Current.BAccountID)
					.GroupBy(x => new { ((PRDeductCode)x).CodeID, ((PRPaymentFringeBenefitDecreasingRate)x).ProjectID }))
			{
				PRPaymentFringeBenefitDecreasingRate fringeBenefit = resultGroup.First();
				PRDeductCode deductCode = resultGroup.First();
				yield return new EfbRecord(deductCode.CodeCD.TrimEnd())
				{
					HourlyRate = fringeBenefit.BenefitRate
				};
				}

			foreach (var resultGroup in PeriodFringeDestinationBenefits.Select(reportPeriod.FirstDate, reportPeriod.LastDate)
				.Select(x => (PXResult<PRPaymentFringeBenefit, PRPayment, PMProject, PRDeductCode, BAccount, Address, Contact>)x)
				.ToList()
				.Where(x => ((PRPayment)x).EmployeeID == PeriodEmployees.Current.BAccountID && ((PRPaymentFringeBenefit)x).FringeAmountInBenefit != 0 && ((PRPaymentFringeBenefit)x).ProjectHours != 0)
				.GroupBy(x => new { ((PRDeductCode)x).CodeID, ((PRPaymentFringeBenefit)x).ProjectID }))
			{
				PRPaymentFringeBenefit fringeBenefit = resultGroup.First();
				PRDeductCode deductCode = resultGroup.First();
				yield return new EfbRecord(deductCode.CodeCD.TrimEnd())
				{
					HourlyRate = resultGroup.Sum(x => ((PRPaymentFringeBenefit)x).FringeAmountInBenefit) / fringeBenefit.ProjectHours
				};
			}
		}

		protected virtual IEnumerable<CspRecord> CreateCspList(DateTime checkDate)
		{
			Dictionary<int, CspRecord> records = new Dictionary<int, CspRecord>(); ;
			foreach (PXResult<PRTaxDetail, APInvoice, APAdjust, APPayment, PRTaxCode> result in PaymentSettledTaxes.Select(checkDate))
			{
				APPayment apPayment = (APPayment)result;
				PRTaxCode tax = (PRTaxCode)result;
				PRTaxDetail taxDetail = (PRTaxDetail)result;

				if (taxDetail.Amount > 0 && tax.JurisdictionLevel == TaxJurisdiction.State && _TaxIDMapping[tax.TaxUniqueCode]?.TaxItemMappings != null)
				{
					foreach ((int aatrixTaxID, bool _) in _TaxIDMapping[tax.TaxUniqueCode].TaxItemMappings)
					{
						CspRecord record;
						if (!records.TryGetValue(aatrixTaxID, out record))
						{
							record = new CspRecord(checkDate, aatrixTaxID)
							{
								State = tax.TaxState,
								PaymentAmount = 0m,
								PaymentDate = apPayment.AdjDate
							};

							records[aatrixTaxID] = record;
						}

						record.PaymentAmount += taxDetail.Amount;
					}
				}
			}

			return records.Values;
		}

		protected virtual IEnumerable<ClpRecord> CreateClpList(DateTime checkDate)
		{
			Dictionary<int, ClpRecord> records = new Dictionary<int, ClpRecord>(); ;
			foreach (PXResult<PRTaxDetail, APInvoice, APAdjust, APPayment, PRTaxCode, PRPayment> result in PaymentSettledTaxes.Select(checkDate))
			{
				APPayment apPayment = (APPayment)result;
				PRTaxCode tax = (PRTaxCode)result;
				PRTaxDetail taxDetail = (PRTaxDetail)result;
				PRPayment prPayment = (PRPayment)result;

				if (taxDetail.Amount > 0 &&
					tax.JurisdictionLevel != TaxJurisdiction.State &&
					tax.JurisdictionLevel != TaxJurisdiction.Federal &&
					_TaxIDMapping[tax.TaxUniqueCode]?.TaxItemMappings != null)
				{
					bool residentSplitTax = TaxIsSplitByResidency(tax.TaxUniqueCode);
					bool isEmployeeResident = bool.Parse(EmployeeTaxAttributes.Select(prPayment.EmployeeID).FirstTableItems
						.FirstOrDefault(x => x.AatrixMapping == AatrixField.ELI.IsResidentTax && x.TaxID == tax.TaxID)?.Value ?? "false");
					foreach ((int aatrixTaxID, bool isResidentTax) in _TaxIDMapping[tax.TaxUniqueCode].TaxItemMappings)
					{
						if (!residentSplitTax || isEmployeeResident == isResidentTax)
						{
							ClpRecord record;
							if (!records.TryGetValue(aatrixTaxID, out record))
							{
								record = new ClpRecord(checkDate, aatrixTaxID)
								{
									State = tax.TaxState,
									PaymentAmount = 0m,
									PaymentDate = apPayment.AdjDate
								};

								records[aatrixTaxID] = record;
							}

							record.PaymentAmount += taxDetail.Amount; 
						}
					}
				}
			}

			return records.Values;
		}
		
		protected virtual (EcvRecord, EciRecord) CreateEmployeeAcaRecords(EmpRecord emp)
		{
			PXResultset<PRAcaEmployeeMonthlyInformation> employeeMonthlyRecords = AcaEmployeeMonthlyInformation.Select();
			if (employeeMonthlyRecords.Any())
			{
				PXResultset<PREmployeeAttribute> employeeAttributes = EmployeeAttributes.Select();

				EcvRecord ecv = new EcvRecord()
				{
					ElectronicOnly = bool.Parse(employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.ECV.ElectronicAcaForms)?.Value ?? "false"),
					PolicyOriginCode = employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.ECV.AcaPolicyOrigin)?.Value?[0],
					SelfInsuredEmployee = bool.Parse(employeeAttributes.FirstTableItems.FirstOrDefault(x => x.AatrixMapping == AatrixField.ECV.EmployeeSelfInsured)?.Value ?? "false")
				};

				EciRecord eci = new EciRecord(emp.EmployeeID)
				{
					SocialSecurityNumber = emp.SocialSecurityNumber,
					BirthDate = emp.BirthDate,
					FirstName = emp.FirstName,
					MiddleName = emp.MiddleName,
					LastName = emp.LastName,
					NameSuffix = emp.NameSuffix
				};

				foreach (PRAcaEmployeeMonthlyInformation employeeMonth in employeeMonthlyRecords)
				{
					int monthIndex = employeeMonth.Month.Value - 1;

					ecv.OfferOfCoverageCode[monthIndex] = employeeMonth.OfferOfCoverage;
					ecv.MinimumIndividualContribution[monthIndex] = employeeMonth.MinimumIndividualContribution;
					ecv.SafeHarborCode[monthIndex] = employeeMonth.Section4980H;
					eci.CoverageIndicator[monthIndex] = AcaOfferOfCoverage.MeetsMinimumCoverageRequirement(employeeMonth.OfferOfCoverage);
					if (eci.CoverageIndicator[monthIndex] == true && (ecv.PlanStartMonth == null || ecv.PlanStartMonth > employeeMonth.Month.Value))
					{
						ecv.PlanStartMonth = employeeMonth.Month.Value;
					}
				}

				return (ecv, eci);
			}

			return (null, null);
		}

		protected virtual string ConcatAddress(IAddressBase address)
		{
			if (address == null)
			{
				return null;
			}

			if (string.IsNullOrEmpty(address.AddressLine2))
			{
				return address.AddressLine1;
			}

			return address.AddressLine1 + ' ' + address.AddressLine2;
		}

		protected virtual decimal? GetHoursForDay(EPEarningType earningType, PREarningDetail earningDetail, DateTime periodStart, int dayOffset, bool overtime)
		{
			if (earningDetail.IsFringeRateEarning == true)
			{
				return 0m;
			}

			bool overtimeCondition;
			if (overtime)
			{
				overtimeCondition = earningType.IsOvertime == true;
			}
			else
			{
				overtimeCondition = earningType.IsOvertime != true;
			}

			return (overtimeCondition && earningDetail.Date.Value.Date == (periodStart + new TimeSpan(dayOffset, 0, 0, 0)).Date) ? earningDetail.Hours : 0m;
		}

		private bool TaxIsSplitByResidency(string uniqueTaxID)
		{
			List<(int _, bool isResidentTax)> aatrixMappings = _TaxIDMapping[uniqueTaxID]?.TaxItemMappings.ToList();
			return aatrixMappings != null && aatrixMappings.Count == 2 && aatrixMappings[0].isResidentTax != aatrixMappings[1].isResidentTax;
		}
		
		protected virtual bool ShouldGenerateAcaRecords()
		{
			return Report.Current.ReportingPeriod == GovernmentReportingPeriod.Annual && AcaCompanyYearlyInformation.SelectSingle() != null;
		}

		protected virtual bool ShouldGenerateCertifiedRecords()
		{
			return Report.Current.ReportType == PRGovernmentReport.CertifiedReportType;
		}
		#endregion Helpers
	}
}