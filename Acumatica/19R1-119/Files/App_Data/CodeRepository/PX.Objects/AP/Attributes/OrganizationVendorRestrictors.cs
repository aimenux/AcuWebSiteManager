using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.AP
{
	class VerndorNonEmployeeOrOrganizationRestrictorAttribute : PXRestrictorAttribute
	{
		public VerndorNonEmployeeOrOrganizationRestrictorAttribute() : 
			base(typeof(Where<
				BAccountR.type, Equal<BAccountType.branchType>,
				Or<BAccountR.type, Equal<BAccountType.organizationType>,
				Or<BAccountR.type, Equal<BAccountType.organizationBranchCombinedType>,
				Or<BAccountR.type, Equal<BAccountType.vendorType>, 
				Or<BAccountR.type, Equal<BAccountType.combinedType>>>>>>),
			Messages.VerndorNonEmployeeOrOrganization)
		{
		}
	}
}
