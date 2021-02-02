using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CT
{
	public class ExpiringContractsEngVisibilityRestriction : PXGraphExtension<ExpiringContractsEng>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.ContractsView.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}
	}

	public sealed class ExpiringContractFilterVisibilityRestriction : PXCacheExtension<ExpiringContractsEng.ExpiringContractFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerClassByUserBranches]
		public string CustomerClassID { get; set; }
	}
}