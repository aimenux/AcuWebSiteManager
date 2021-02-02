using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PR
{
	public class PRDedBenCodeMaintExt : PXGraphExtension<PRDedBenCodeMaint>
	{
		#region Views
		public SelectFrom<PRAcaDeductCoverageInfo>
			.Where<PRAcaDeductCoverageInfo.deductCodeID.IsEqual<PRDeductCode.codeID.FromCurrent>>.View AcaInformation;
		#endregion Views

		#region Events
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		protected virtual void PRDeductCode_AcaApplicable_CacheAttached(PXCache sender) { }
		#endregion Events
	}
}
