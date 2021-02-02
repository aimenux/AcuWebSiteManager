using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CT
{
	public class TemplateMaintVisibilityRestriction : PXGraphExtension<TemplateMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.Contracts.Join<LeftJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>>();
			Base.Contracts.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}
	}
	public sealed class ContractTemplateVisibilityRestriction : PXCacheExtension<ContractTemplate>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region ContractCD
		[PXRemoveBaseAttribute(typeof(RestrictCustomerByUserBranches))]
		public string ContractCD { get; set; }
		#endregion
	}
}