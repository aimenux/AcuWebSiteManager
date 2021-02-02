using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	public class ARExpiredCardsProcessVisibilityRestriction : PXGraphExtension<ARExpiredCardsProcess>
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

	public sealed class ARExpiredCardFilterVisibilityRestriction : PXCacheExtension<ARExpiredCardsProcess.ARExpiredCardFilter>
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