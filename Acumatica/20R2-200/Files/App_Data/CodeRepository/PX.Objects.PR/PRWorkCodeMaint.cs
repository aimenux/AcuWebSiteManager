using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PM;
using System.Collections;

namespace PX.Objects.PR
{
	public class PRWorkCodeMaint : PXGraph<PRWorkCodeMaint>
	{
		[PXImport(typeof(PMWorkCode))]
		public PXSelect<PMWorkCode> WorkCompensationCodes;
		public PXSavePerRow<PMWorkCode> Save;
		public PXCancel<PMWorkCode> Cancel;

		public SelectFrom<PRWorkCompensationBenefitRate>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRWorkCompensationBenefitRate.deductCodeID>>
			.Where<PRWorkCompensationBenefitRate.workCodeID.IsEqual<PMWorkCode.workCodeID.FromCurrent>>.View WorkCompensationRates;

		public SelectFrom<PRDeductCode>
			.Where<PRDeductCode.isWorkersCompensation.IsEqual<True>
				.And<PRDeductCode.isActive.IsEqual<True>>>.View WCDeductions;

		#region Data View Delegates
		public IEnumerable workCompensationRates()
		{
			PXView bqlSelect = new PXView(this, false, WorkCompensationRates.View.BqlSelect);

			foreach (object objResult in bqlSelect.SelectMulti())
			{
				PXResult<PRWorkCompensationBenefitRate, PRDeductCode> result = objResult as PXResult<PRWorkCompensationBenefitRate, PRDeductCode>;
				if (result != null)
				{
					PRWorkCompensationBenefitRate packageDeduct = (PRWorkCompensationBenefitRate)result;
					PRDeductCode deductCode = (PRDeductCode)result;

					if (packageDeduct.DeductCodeID != null && deductCode.IsActive != true)
					{
						packageDeduct.IsActive = false;
						PXUIFieldAttribute.SetEnabled(WorkCompensationRates.Cache, packageDeduct, false);
						WorkCompensationRates.Cache.RaiseExceptionHandling<PRWorkCompensationBenefitRate.deductCodeID>(
							packageDeduct,
							packageDeduct.DeductCodeID,
							new PXSetPropertyException(Messages.DeductCodeInactive, PXErrorLevel.Warning));
					}

					yield return result;
				}
			}
		}
		#endregion Data View Delegates

		#region Events
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PMWorkCodeAttribute))]
		[PXDBString(PMWorkCode.workCodeID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(PMWorkCode.workCodeID.FromCurrent))]
		public void _(Events.CacheAttached<PRWorkCompensationBenefitRate.workCodeID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Benefit Calculation Method")]
		public void _(Events.CacheAttached<PRDeductCode.cntCalcType> e) { }

		public void _(Events.RowSelected<PRWorkCompensationBenefitRate> e)
		{
			if (e.Row == null)
			{
				return;
			}

			PXUIFieldAttribute.SetEnabled<PRWorkCompensationBenefitRate.deductCodeID>(e.Cache, e.Row, e.Row.DeductCodeID == null);
		}

		public void _(Events.RowInserted<PMWorkCode> e)
		{
			foreach (PRDeductCode wcDeduction in WCDeductions.Select())
			{
				PRWorkCompensationBenefitRate workCompensationRate = new PRWorkCompensationBenefitRate()
				{
					DeductCodeID = wcDeduction.CodeID
				};
				WorkCompensationRates.Insert(workCompensationRate);
			}
		}

		public void _(Events.RowDeleted<PMWorkCode> e)
		{
			SelectFrom<PRWorkCompensationBenefitRate>.Where<PRWorkCompensationBenefitRate.workCodeID.IsEqual<P.AsString>>.View
				.Select(this, e.Row.WorkCodeID)
				.ForEach(x => WorkCompensationRates.Delete(x));
		}

		public void _(Events.FieldUpdated<PMWorkCode.isActive> e)
		{
			PMWorkCode row = e.Row as PMWorkCode;
			if (row == null)
			{
				return;
			}

			if (e.NewValue.Equals(true))
			{
				foreach (PRDeductCode result in SelectFrom<PRDeductCode>
					.LeftJoin<PRWorkCompensationBenefitRate>.On<PRWorkCompensationBenefitRate.workCodeID.IsEqual<P.AsString>
						.And<PRWorkCompensationBenefitRate.deductCodeID.IsEqual<PRDeductCode.codeID>>>
					.Where<PRWorkCompensationBenefitRate.deductCodeID.IsNull
						.And<PRDeductCode.isWorkersCompensation.IsEqual<True>>
						.And<PRDeductCode.isActive.IsEqual<True>>>.View.Select(this, row.WorkCodeID))
				{
					PRWorkCompensationBenefitRate newRate = new PRWorkCompensationBenefitRate()
					{
						WorkCodeID = row.WorkCodeID,
						DeductCodeID = result.CodeID
					};

					WorkCompensationRates.Insert(newRate);
				}
			}
		}
		#endregion Events
	}
}
