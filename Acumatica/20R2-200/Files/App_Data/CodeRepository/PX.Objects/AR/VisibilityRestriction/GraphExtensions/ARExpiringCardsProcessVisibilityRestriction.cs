using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class ARExpiringCardsProcessVisibilityRestriction : PXGraphExtension<ARExpiringCardsProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.CardsView.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}
	}

	public sealed class ARExpiringCardFilterVisibilityRestriction : PXCacheExtension<ARExpiringCardsProcess.ARExpiringCardFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerClassID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerClassByUserBranches]
		public string CustomerClassID { get; set; }
		#endregion
	}
}
