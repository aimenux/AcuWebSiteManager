using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Linq;

namespace PX.Objects.PR
{
	public class PRCertifiedProjectMaint : PXGraph<PRCertifiedProjectMaint>
	{
		#region Actions
		public PXSave<CertifiedProjectFilter> Save;
		public PXCancel<CertifiedProjectFilter> Cancel;
		#endregion

		[Serializable]
		[PXHidden]
		public partial class CertifiedProjectFilter : IBqlTable
		{
			public abstract class projectID : BqlInt.Field<projectID> { }
			[CertifiedProject(DisplayName = "Project ID",  IsKey = true)]
			public virtual int? ProjectID { get; set; }
		}

		#region Views
		public PXFilter<CertifiedProjectFilter> CertifiedProject;

		public SelectFrom<PMProject>.
			Where<PMProject.contractID.
				IsEqual<CertifiedProjectFilter.projectID.FromCurrent>>.View CurrentProject;

		public SelectFrom<PMLaborCostRate>.
			Where<PMLaborCostRate.projectID.
				IsEqual<CertifiedProjectFilter.projectID.FromCurrent>>.View EarningRates;

		public SelectFrom<PRDeductionAndBenefitProjectPackage>.
			InnerJoin<PRDeductCode>.On<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID.
				IsEqual<PRDeductCode.codeID>>.
			Where<PRDeductionAndBenefitProjectPackage.projectID.
				IsEqual<CertifiedProjectFilter.projectID.FromCurrent>>.View DeductionsAndBenefitsPackage;

		public SelectFrom<PRProjectFringeBenefitRate>
			.InnerJoin<InventoryItem>.On<InventoryItem.inventoryID.IsEqual<PRProjectFringeBenefitRate.laborItemID>>
			.Where<PRProjectFringeBenefitRate.projectID.IsEqual<CertifiedProjectFilter.projectID.FromCurrent>>.View FringeBenefitRates;

		public SelectFrom<PRProjectFringeBenefitRateReducingDeduct>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRProjectFringeBenefitRateReducingDeduct.deductCodeID>>
			.Where<PRProjectFringeBenefitRateReducingDeduct.projectID.IsEqual<CertifiedProjectFilter.projectID.FromCurrent>>.View FringeBenefitRateReducingDeductions;
		#endregion Views

		#region Data view delegates
		public IEnumerable deductionsAndBenefitsPackage()
		{
			PXView bqlSelect = new PXView(this, false, DeductionsAndBenefitsPackage.View.BqlSelect);

			foreach (object objResult in bqlSelect.SelectMulti())
			{
				PXResult<PRDeductionAndBenefitProjectPackage, PRDeductCode> result = objResult as PXResult<PRDeductionAndBenefitProjectPackage, PRDeductCode>;
				if (result != null)
				{
					PRDeductionAndBenefitProjectPackage packageDeduct = (PRDeductionAndBenefitProjectPackage)result;
					PRDeductCode deductCode = (PRDeductCode)result;

					if (packageDeduct.DeductionAndBenefitCodeID != null && deductCode.IsActive != true)
					{
						PXUIFieldAttribute.SetEnabled(DeductionsAndBenefitsPackage.Cache, packageDeduct, false);
						DeductionsAndBenefitsPackage.Cache.RaiseExceptionHandling<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID>(
							packageDeduct,
							packageDeduct.DeductionAndBenefitCodeID,
							new PXSetPropertyException(Messages.DeductCodeInactive, PXErrorLevel.Warning));
					}

					yield return result;
				}
			}
		}

		public IEnumerable fringeBenefitRateReducingDeductions()
		{
			PXView bqlSelect = new PXView(this, false, FringeBenefitRateReducingDeductions.View.BqlSelect);

			foreach (PXResult<PRProjectFringeBenefitRateReducingDeduct, PRDeductCode> result in bqlSelect.SelectMulti())
			{
				PRProjectFringeBenefitRateReducingDeduct rateReducingDeduct = result;
				PRDeductCode deductCode = result;
				if (deductCode.IsActive != true)
				{
					rateReducingDeduct.IsActive = false;
					PXUIFieldAttribute.SetEnabled(FringeBenefitRateReducingDeductions.Cache, rateReducingDeduct, false);
					FringeBenefitRateReducingDeductions.Cache.RaiseExceptionHandling<PRProjectFringeBenefitRateReducingDeduct.deductCodeID>(
						rateReducingDeduct,
						rateReducingDeduct.DeductCodeID,
						new PXSetPropertyException(Messages.DeductCodeInactive, PXErrorLevel.Warning));
				}

				yield return result;
			}
		}
		#endregion Data view delegates

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PMLaborCostRateType.Certified)]
		protected virtual void _(Events.CacheAttached<PMLaborCostRate.type> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(
			SearchFor<InventoryItem.descr>.
			Where<InventoryItem.inventoryID.IsEqual<PMLaborCostRate.inventoryID.FromCurrent>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<PMLaborCostRate.inventoryID>))]
		protected virtual void _(Events.CacheAttached<PMLaborCostRate.description> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		[PXDefault(typeof(CertifiedProjectFilter.projectID))]
		protected virtual void _(Events.CacheAttached<PMLaborCostRate.projectID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		[PXDefault(typeof(CertifiedProjectFilter.projectID))]
		protected virtual void _(Events.CacheAttached<PRDeductionAndBenefitProjectPackage.projectID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		[PXDefault(typeof(CertifiedProjectFilter.projectID))]
		protected virtual void _(Events.CacheAttached<PRProjectFringeBenefitRate.projectID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
		[PXDefault(typeof(CertifiedProjectFilter.projectID))]
		protected virtual void _(Events.CacheAttached<PRProjectFringeBenefitRateReducingDeduct.projectID> e) { }	

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCheckUnique(typeof(PMLaborCostRate.projectID), typeof(PMLaborCostRate.inventoryID), typeof(PMLaborCostRate.taskID), ClearOnDuplicate = false)]
		protected virtual void _(Events.CacheAttached<PMLaborCostRate.effectiveDate> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXDBPriceCostAttribute), nameof(PXDBPriceCostAttribute.MinValue), 0)]
		protected virtual void _(Events.CacheAttached<PMLaborCostRate.rate> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Deduction Calculation Method")]
		[DeductionCalcMethodDisplay]
		protected virtual void _(Events.CacheAttached<PRDeductCode.dedCalcType> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Contribution Calculation Method")]
		[BenefitCalcMethodDisplay]
		protected virtual void _(Events.CacheAttached<PRDeductCode.cntCalcType> e) { }
		#endregion CacheAttached

		#region Events
		protected virtual void _(Events.RowPersisting<PRDeductionAndBenefitProjectPackage> e)
		{
			PRDeductCode deductCode = PXSelectorAttribute.Select<PRDeductionAndBenefitProjectPackage.deductionAndBenefitCodeID>(e.Cache, e.Row) as PRDeductCode;
			if (deductCode != null)
			{
				if ((deductCode.DedCalcType == DedCntCalculationMethod.FixedAmount || deductCode.DedCalcType == DedCntCalculationMethod.AmountPerHour)
					&& deductCode.ContribType != ContributionType.EmployerContribution && e.Row.DeductionAmount == null)
				{
					e.Row.DeductionAmount = 0m;
				}
				else if ((deductCode.DedCalcType == DedCntCalculationMethod.PercentOfGross || deductCode.DedCalcType == DedCntCalculationMethod.PercentOfCustom)
					&& deductCode.ContribType != ContributionType.EmployerContribution && e.Row.DeductionRate == null)
				{
					e.Row.DeductionRate = 0m;
				}

				if ((deductCode.CntCalcType == DedCntCalculationMethod.FixedAmount || deductCode.CntCalcType == DedCntCalculationMethod.AmountPerHour)
					&& deductCode.ContribType != ContributionType.EmployeeDeduction && e.Row.BenefitAmount == null)
				{
					e.Row.BenefitAmount = 0m;
				}
				else if ((deductCode.CntCalcType == DedCntCalculationMethod.PercentOfGross || deductCode.CntCalcType == DedCntCalculationMethod.PercentOfCustom)
					&& deductCode.ContribType != ContributionType.EmployeeDeduction && e.Row.BenefitRate == null)
				{
					e.Row.BenefitRate = 0m;
				}
			}
		}
		#endregion Events
	}
}
