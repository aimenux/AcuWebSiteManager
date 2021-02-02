using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.Payroll.Data;

namespace PX.Objects.PR
{
	public class PRDedBenCodeMaint : PXGraph<PRDedBenCodeMaint, PRDeductCode>
	{
		#region Views

		public PXSelect<PRDeductCode> Document;
		public SelectFrom<PRDeductCode>.Where<PRDeductCode.codeID.IsEqual<PRDeductCode.codeID.AsOptional>>.View CurrentDocument;

		public PXSelect<PRDeductCodeDetail,
			Where<PRDeductCodeDetail.codeID, Equal<Current<PRDeductCode.codeID>>>> DeductCodeTaxes;

		public SelectFrom<PRPaymentDeduct>
			.InnerJoin<PRPayment>.On<PRPayment.refNbr.IsEqual<PRPaymentDeduct.refNbr>
				.And<PRPayment.docType.IsEqual<PRPaymentDeduct.docType>>>
			.Where<PRPayment.printed.IsEqual<False>
				.And<PRPayment.released.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.adjustment>>
				.And<PRPaymentDeduct.isActive.IsEqual<True>>
				.And<PRPaymentDeduct.codeID.IsEqual<P.AsInt>>>.View EditablePaymentDeductions;

		public SelectFrom<PRPayment>.View Payments;

		public SelectFrom<PRWorkCompensationBenefitRate>
			.InnerJoin<PMWorkCode>.On<PMWorkCode.workCodeID.IsEqual<PRWorkCompensationBenefitRate.workCodeID>>
			.Where<PRWorkCompensationBenefitRate.deductCodeID.IsEqual<PRDeductCode.codeID.FromCurrent>>.View WorkCompensationRates;

		public SelectFrom<PRDeductCodeEarningIncreasingWage>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PRDeductCodeEarningIncreasingWage.applicableTypeCD>>
			.Where<PRDeductCodeEarningIncreasingWage.deductCodeID.IsEqual<PRDeductCode.codeID.FromCurrent>>.View EarningsIncreasingWage;

		public SelectFrom<PRDeductCodeBenefitIncreasingWage>
			.Where<PRDeductCodeBenefitIncreasingWage.deductCodeID.IsEqual<PRDeductCode.codeID.FromCurrent>>.View BenefitsIncreasingWage;

		public SelectFrom<PRDeductCodeTaxIncreasingWage>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRDeductCodeTaxIncreasingWage.applicableTaxID>>
			.Where<PRDeductCodeTaxIncreasingWage.deductCodeID.IsEqual<PRDeductCode.codeID.FromCurrent>>.View TaxesIncreasingWage;

		public SelectFrom<PRDeductCodeDeductionDecreasingWage>
			.Where<PRDeductCodeDeductionDecreasingWage.deductCodeID.IsEqual<PRDeductCode.codeID.FromCurrent>>.View DeductionsDecreasingWage;

		public SelectFrom<PRDeductCodeTaxDecreasingWage>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRDeductCodeTaxDecreasingWage.applicableTaxID>>
			.Where<PRDeductCodeTaxDecreasingWage.deductCodeID.IsEqual<PRDeductCode.codeID.FromCurrent>>.View TaxesDecreasingWage;

		#endregion

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDBDefault(typeof(PRDeductCode.codeID))]
		public void _(Events.CacheAttached<PRWorkCompensationBenefitRate.deductCodeID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXUIVisibleAttribute))]
		[PXUIVisible(typeof(Where<PRDeductCode.contribType.FromCurrent.IsNotEqual<ContributionType.employerContribution>>))]
		public void _(Events.CacheAttached<PRWorkCompensationBenefitRate.deductionRate> e) { }
		#endregion

		#region Row Event Handlers
		protected virtual void PRDeductCode_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PRDeductCode row = (PRDeductCode)e.Row;
			if (row == null)
				return;

			DeductCodeTaxes.Cache.AllowInsert = SubjectToTaxes.IsFromList(row.IncludeType);

			EarningsIncreasingWage.Cache.AllowInsert = AllowEarningsIncreasingWages(row) && SubjectToTaxes.IsFromList(row.EarningsIncreasingWageIncludeType);
			EarningsIncreasingWage.Cache.AllowUpdate = AllowEarningsIncreasingWages(row) && SubjectToTaxes.IsFromList(row.EarningsIncreasingWageIncludeType);
			EarningsIncreasingWage.Cache.AllowDelete = AllowEarningsIncreasingWages(row) && SubjectToTaxes.IsFromList(row.EarningsIncreasingWageIncludeType);
			BenefitsIncreasingWage.Cache.AllowInsert = AllowBenefitsIncreasingWages(row) && SubjectToTaxes.IsFromList(row.BenefitsIncreasingWageIncludeType);
			BenefitsIncreasingWage.Cache.AllowUpdate = AllowBenefitsIncreasingWages(row) && SubjectToTaxes.IsFromList(row.BenefitsIncreasingWageIncludeType);
			BenefitsIncreasingWage.Cache.AllowDelete = AllowBenefitsIncreasingWages(row) && SubjectToTaxes.IsFromList(row.BenefitsIncreasingWageIncludeType);
			TaxesIncreasingWage.Cache.AllowInsert = AllowTaxesIncreasingWages(row) && SubjectToTaxes.IsFromList(row.TaxesIncreasingWageIncludeType);
			TaxesIncreasingWage.Cache.AllowUpdate = AllowTaxesIncreasingWages(row) && SubjectToTaxes.IsFromList(row.TaxesIncreasingWageIncludeType);
			TaxesIncreasingWage.Cache.AllowDelete = AllowTaxesIncreasingWages(row) && SubjectToTaxes.IsFromList(row.TaxesIncreasingWageIncludeType);
			DeductionsDecreasingWage.Cache.AllowInsert = AllowDeductionsDecreasingWages(row) && SubjectToTaxes.IsFromList(row.DeductionsDecreasingWageIncludeType);
			DeductionsDecreasingWage.Cache.AllowUpdate = AllowDeductionsDecreasingWages(row) && SubjectToTaxes.IsFromList(row.DeductionsDecreasingWageIncludeType);
			DeductionsDecreasingWage.Cache.AllowDelete = AllowDeductionsDecreasingWages(row) && SubjectToTaxes.IsFromList(row.DeductionsDecreasingWageIncludeType);
			TaxesDecreasingWage.Cache.AllowInsert = AllowTaxesDecreasingWages(row) && SubjectToTaxes.IsFromList(row.TaxesDecreasingWageIncludeType);
			TaxesDecreasingWage.Cache.AllowUpdate = AllowTaxesDecreasingWages(row) && SubjectToTaxes.IsFromList(row.TaxesDecreasingWageIncludeType);
			TaxesDecreasingWage.Cache.AllowDelete = AllowTaxesDecreasingWages(row) && SubjectToTaxes.IsFromList(row.TaxesDecreasingWageIncludeType);
		}

		protected virtual void PRDeductCode_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if ((PREmployeeDeduct)PXSelect<PREmployeeDeduct, Where<PREmployeeDeduct.codeID, Equal<Current<PRDeductCode.codeID>>>>.SelectWindowed(this, 0, 1) != null)
			{
				throw new PXException(Messages.DeductionCodeIsInUseAndCantBeDeleted);
			}
		}

		protected virtual void _(Events.RowPersisting<PRDeductCode> e)
		{
			var row = (PRDeductCode)e.Row;
			if (row == null)
			{
				return;
			}

			if (e.Cache.GetStatus(row) == PXEntryStatus.Updated)
			{
				PRAcaDeductCode acaDeduct = row.GetExtension<PRAcaDeductCode>();
				if (!object.Equals(e.Cache.GetValueOriginal<PRDeductCode.contribType>(row), row.ContribType) ||
					!object.Equals(e.Cache.GetValueOriginal<PRDeductCode.isGarnishment>(row), row.IsGarnishment) ||
					!object.Equals(e.Cache.GetValueOriginal<PRDeductCode.affectsTaxes>(row), row.AffectsTaxes) ||
					!object.Equals(e.Cache.GetValueOriginal<PRDeductCode.isWorkersCompensation>(row), row.IsWorkersCompensation) ||
					!object.Equals(e.Cache.GetValueOriginal<PRDeductCode.isCertifiedProject>(row), row.IsCertifiedProject) ||
					!object.Equals(e.Cache.GetValueOriginal<PRDeductCode.isUnion>(row), row.IsUnion) ||
					!object.Equals(e.Cache.GetValueOriginal<PRDeductCode.isPayableBenefit>(row), row.IsPayableBenefit) ||
					!object.Equals(e.Cache.GetValueOriginal<PRDeductCode.state>(row), row.State) ||
					!object.Equals(e.Cache.GetValueOriginal<PRDeductCode.dedCalcType>(row), row.DedCalcType) ||
					!object.Equals(e.Cache.GetValueOriginal<PRDeductCode.cntCalcType>(row), row.CntCalcType) ||
					(acaDeduct != null && !object.Equals(e.Cache.GetValueOriginal<PRAcaDeductCode.acaApplicable>(row), acaDeduct.AcaApplicable)))
				{
					PXReferentialIntegrityCheckAttribute refAttr = e.Cache.GetAttributesOfType<PXReferentialIntegrityCheckAttribute>(row, nameof(PRDeductCode.codeID)).FirstOrDefault();
					if (refAttr != null)
					{
						foreach (Reference reference in refAttr.TableMergedReferencesInspector.GetIncomingReferencesApplicableTo(typeof(PRDeductCode))
							.Where(x => x.ReferenceOrigin == ReferenceOrigin.DeclareReferenceAttribute))
						{
							if (reference.SelectChildren(this, row).Any())
							{
								throw new PXException(Messages.DeductCodeInUse);
							}
						}
					}
				} 
			}
		}

		public void _(Events.RowSelected<PRWorkCompensationBenefitRate> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PRWorkCompensationBenefitRate.workCodeID>(e.Cache, e.Row, e.Row.WorkCodeID == null);
		}

		protected virtual void _(Events.RowUpdating<PRDeductCode> e)
		{
			PRDeductCode row = e.NewRow;
			if (row == null)
			{
				return;
			}

			if (row.IsWorkersCompensation == true)
			{
				row.AffectsTaxes = false;
				row.DedPercent = 0m;
				row.DedMaxFreqType = DeductionMaxFrequencyType.NoMaximum;
				row.CntPercent = 0m;
				row.CntMaxFreqType = DeductionMaxFrequencyType.NoMaximum;
			}
			else
			{
				row.State = null;
			}

			if (row.ContribType == ContributionType.EmployeeDeduction)
			{
				e.Cache.SetDefaultExt<PRDeductCode.cntCalcType>(row);
				row.CntAmount = null;
				row.CntPercent = null;
				e.Cache.SetDefaultExt<PRDeductCode.cntMaxFreqType>(row);
				row.CntMaxAmount = null;
			}
			else if (row.ContribType == ContributionType.EmployerContribution)
			{
				e.Cache.SetDefaultExt<PRDeductCode.dedCalcType>(row);
				row.DedAmount = null;
				row.DedPercent = null;
				e.Cache.SetDefaultExt<PRDeductCode.dedMaxFreqType>(row);
				row.DedMaxAmount = null;
			}

			if (row.ContribType == ContributionType.EmployeeDeduction || row.ContribType == ContributionType.BothDeductionAndContribution)
			{
				if (row.DedCalcType == DedCntCalculationMethod.FixedAmount || row.DedCalcType == DedCntCalculationMethod.AmountPerHour)
				{
					row.DedPercent = null;
				}
				else
				{
					row.DedAmount = null;
				}

				if (row.DedMaxFreqType == DeductionMaxFrequencyType.NoMaximum)
				{
					row.DedMaxAmount = null;
				}
			}

			if (row.ContribType == ContributionType.EmployerContribution || row.ContribType == ContributionType.BothDeductionAndContribution)
			{
				row.IsGarnishment = false;
				if (row.CntCalcType == DedCntCalculationMethod.FixedAmount || row.CntCalcType == DedCntCalculationMethod.AmountPerHour)
				{
					row.CntPercent = null;
				}
				else
				{
					row.CntAmount = null;
				}

				if (row.CntMaxFreqType == DeductionMaxFrequencyType.NoMaximum)
				{
					row.CntMaxAmount = null;
				}
			}

			if (row.ContribType == ContributionType.EmployeeDeduction || row.IsPayableBenefit == true || row.IsWorkersCompensation == true)
			{
				row.NoFinancialTransaction = false;
			}
		}

		public void _(Events.RowDeleted<PRDeductCode> e)
		{
			if (e.Row.IsWorkersCompensation == true)
			{
				DeleteWorkCodeRates(e.Row.CodeID);
			}
		}
		#endregion Row Event Handlers 

		#region Field Event Handlers
		protected virtual void PRDeductCode_IncludeType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			PRDeductCode row = (PRDeductCode)e.Row;
			if (row == null)
				return;

			if (!SubjectToTaxes.IsFromList(row.IncludeType))
			{
				foreach (PRDeductCodeDetail item in DeductCodeTaxes.Select())
				{
					DeductCodeTaxes.Delete(item);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PRDeductCode.isWorkersCompensation> e)
		{
			PRDeductCode row = e.Row as PRDeductCode;
			if (row == null)
			{
				return;
			}


			if (row.IsWorkersCompensation != true)
			{
				DeleteWorkCodeRates(row.CodeID);
			}
			else
			{
				if (row.ContribType == ContributionType.EmployeeDeduction)
				{
					row.ContribType = null;
				}
				List<string> allowedCalculationMethods = new List<string>() { DedCntCalculationMethod.PercentOfGross, DedCntCalculationMethod.PercentOfCustom, DedCntCalculationMethod.AmountPerHour };
				if (!allowedCalculationMethods.Contains(row.DedCalcType))
				{
					row.DedCalcType = null;
				}

				if (!allowedCalculationMethods.Contains(row.CntCalcType))
				{
					row.CntCalcType = null;
				}

				foreach (PMWorkCode workCode in SelectFrom<PMWorkCode>.Where<PMWorkCode.isActive.IsEqual<True>>.View.Select(this))
				{
					PRWorkCompensationBenefitRate workCodeRate = new PRWorkCompensationBenefitRate()
					{
						WorkCodeID = workCode.WorkCodeID
					};
					WorkCompensationRates.Insert(workCodeRate);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PRDeductCode.affectsTaxes> e)
		{
			PRDeductCode row = (PRDeductCode)e.Row;
			if (row == null)
			{
				return;
			}

			if (row.AffectsTaxes == true)
			{
				row.IncludeType = SubjectToTaxes.PerTaxEngine;
			}
			else
			{
				row.IncludeType = SubjectToTaxes.None;
			}
		}

		protected virtual void _(Events.FieldUpdated<PRDeductCode.benefitTypeCD> e)
		{
			PRDeductCode row = e.Row as PRDeductCode;
			if (row == null)
			{
				return;
			}

			int? benefitType = row.BenefitTypeCD;
			if (benefitType != null)
			{
				int? defaultReportType = PRTypeSelectorAttribute.GetDefaultReportingType<PRBenefit>(benefitType.Value);
				if (defaultReportType != null)
				{
					Payroll.TaxCategory? defaultReportTypeScope = PRReportingTypeSelectorAttribute.GetReportingTypeScope<PRBenefit>(defaultReportType.Value);
					int? normalReportType = PRReportingTypeSelectorAttribute.GetDefaultID<PRBenefit>();
					e.Cache.SetValue<PRDeductCode.dedReportType>(
						row, 
						(defaultReportTypeScope == Payroll.TaxCategory.Any || defaultReportTypeScope == Payroll.TaxCategory.Employee) ? defaultReportType : normalReportType);
					e.Cache.SetValue<PRDeductCode.cntReportType>(
						row,
						(defaultReportTypeScope == Payroll.TaxCategory.Any || defaultReportTypeScope == Payroll.TaxCategory.Employer) ? defaultReportType : normalReportType);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PRDeductCode.isActive> e)
		{
			PRDeductCode row = e.Row as PRDeductCode;
			if (row == null)
			{
				return;
			}

			if (!e.NewValue.Equals(true))
			{
				HashSet<string> updatedPayments = new HashSet<string>();
				foreach (PXResult<PRPaymentDeduct, PRPayment> result in EditablePaymentDeductions.Select(row.CodeID))
				{
					PRPaymentDeduct paymentDeduct = (PRPaymentDeduct)result;
					PRPayment payment = (PRPayment)result;

					paymentDeduct.IsActive = false;
					EditablePaymentDeductions.Update(paymentDeduct);

					string paymentRef = Caches[typeof(PRPayment)].GetValueExt<PRPayment.paymentDocAndRef>(payment) as string;
					updatedPayments.Add(paymentRef);
					payment.Calculated = false;
					Payments.Update(payment);
				}

				if (updatedPayments.Any())
				{
					StringBuilder concatUpdatedPayments = new StringBuilder();
					foreach (string paymentRef in updatedPayments)
					{
						if (concatUpdatedPayments.Length != 0)
						{
							concatUpdatedPayments.Append(" ");
						}
						concatUpdatedPayments.Append(paymentRef);
					}
					e.Cache.RaiseExceptionHandling<PRDeductCode.isActive>(
						row,
						e.NewValue,
						new PXSetPropertyException(Messages.PaymentDeductInactivated, PXErrorLevel.Warning, concatUpdatedPayments.ToString())); 
				}
			}
			else if (row.IsWorkersCompensation == true)
			{
				foreach (PMWorkCode result in SelectFrom<PMWorkCode>
					.LeftJoin<PRWorkCompensationBenefitRate>.On<PRWorkCompensationBenefitRate.deductCodeID.IsEqual<P.AsInt>
						.And<PRWorkCompensationBenefitRate.workCodeID.IsEqual<PMWorkCode.workCodeID>>>
					.Where<PRWorkCompensationBenefitRate.workCodeID.IsNull
						.And<PMWorkCode.isActive.IsEqual<True>>>.View.Select(this, row.CodeID))
				{
					PRWorkCompensationBenefitRate newRate = new PRWorkCompensationBenefitRate()
					{
						WorkCodeID = result.WorkCodeID,
						DeductCodeID = row.CodeID
					};

					WorkCompensationRates.Insert(newRate);
				}
			}
			
		}

		public virtual void _(Events.FieldSelecting<PRWorkCompensationBenefitRate.isActive> e)
		{
			PMWorkCode workCode = PXSelectorAttribute.Select<PRWorkCompensationBenefitRate.workCodeID>(e.Cache, e.Row) as PMWorkCode;
			if (workCode?.IsActive != true)
			{
				e.ReturnValue = false;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldUpdated<PRDeductCode.dedCalcType> e)
		{
			UpdateAvailableWage(e.Cache, e.Row as PRDeductCode);
		}

		protected virtual void _(Events.FieldUpdated<PRDeductCode.cntCalcType> e)
		{
			UpdateAvailableWage(e.Cache, e.Row as PRDeductCode);
		}

		protected virtual void _(Events.FieldUpdated<PRDeductCode.isPayableBenefit> e)
		{
			UpdateAvailableWage(e.Cache, e.Row as PRDeductCode);
		}
		#endregion
		
		#region Helpers
		private void DeleteWorkCodeRates(int? deductCodeID)
		{
			SelectFrom<PRWorkCompensationBenefitRate>.Where<PRWorkCompensationBenefitRate.deductCodeID.IsEqual<P.AsInt>>.View
				.Select(this, deductCodeID)
				.ForEach(x => WorkCompensationRates.Delete(x));
		}

		protected void UpdateAvailableWage(PXCache cache, PRDeductCode row)
		{
			if (!AllowEarningsIncreasingWages(row))
			{
				EarningsIncreasingWage.Select().ForEach(x => EarningsIncreasingWage.Delete(x));
				cache.SetDefaultExt<PRDeductCode.earningsIncreasingWageIncludeType>(row);
			}

			if (!AllowBenefitsIncreasingWages(row))
			{
				BenefitsIncreasingWage.Select().ForEach(x => BenefitsIncreasingWage.Delete(x));
				cache.SetDefaultExt<PRDeductCode.benefitsIncreasingWageIncludeType>(row);
			}

			if (!AllowTaxesIncreasingWages(row))
			{
				TaxesIncreasingWage.Select().ForEach(x => TaxesIncreasingWage.Delete(x));
				cache.SetDefaultExt<PRDeductCode.taxesIncreasingWageIncludeType>(row);
			}

			if (!AllowDeductionsDecreasingWages(row))
			{
				DeductionsDecreasingWage.Select().ForEach(x => DeductionsDecreasingWage.Delete(x));
				cache.SetDefaultExt<PRDeductCode.deductionsDecreasingWageIncludeType>(row);
			}

			if (!AllowTaxesDecreasingWages(row))
			{
				TaxesDecreasingWage.Select().ForEach(x => TaxesDecreasingWage.Delete(x));
				cache.SetDefaultExt<PRDeductCode.taxesDecreasingWageIncludeType>(row);
			}
		}

		protected bool AllowEarningsIncreasingWages(PRDeductCode row) => row.ShowApplicableWageTab == true;
		protected bool AllowBenefitsIncreasingWages(PRDeductCode row) => row.ShowApplicableWageTab == true && row.IsPayableBenefit != true;
		protected bool AllowTaxesIncreasingWages(PRDeductCode row) => row.ShowApplicableWageTab == true && row.IsPayableBenefit != true;
		protected bool AllowDeductionsDecreasingWages(PRDeductCode row) => row.ShowApplicableWageTab == true && row.IsPayableBenefit != true;
		protected bool AllowTaxesDecreasingWages(PRDeductCode row) => row.ShowApplicableWageTab == true && row.IsPayableBenefit != true;
		#endregion Helpers
	}
}
