using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CA
{
	public class CCSynchronizeCardsVisibilityRestriction : PXGraphExtension<CCSynchronizeCards>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.SynchronizeCardPaymentData.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
									Or<CCSynchronizeCard.bAccountID, IsNull>>>();
		}
	}

	public sealed class CCSynchronizeCardVisibilityRestriction : PXCacheExtension<CCSynchronizeCard>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? BAccountID { get; set; }
		#endregion
	}
}
