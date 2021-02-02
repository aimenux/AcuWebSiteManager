using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Objects.SO;

namespace PX.Objects.AR
{
	class CustomerOrOrganizationRestrictorAttribute : PXRestrictorAttribute
	{
		public CustomerOrOrganizationRestrictorAttribute() :
			base(
				typeof(Where<Customer.type, IsNotNull,
					Or<BAccountR.type, In3<BAccountType.branchType, BAccountType.organizationBranchCombinedType, BAccountType.organizationType>>>),
				Messages.CustomerOrOrganization)
		{
		}

		public CustomerOrOrganizationRestrictorAttribute(Type shipmentTypeField)
			: base(BqlTemplate.OfCondition<
					Where<Current<BqlPlaceholder.A>, NotEqual<SOShipmentType.transfer>, And<Customer.type, IsNotNull,
						Or<Current<BqlPlaceholder.A>, Equal<SOShipmentType.transfer>,
						And<BAccountR.type, In3<BAccountType.branchType, BAccountType.organizationBranchCombinedType, BAccountType.organizationType>>>>>>
					.Replace<BqlPlaceholder.A>(shipmentTypeField)
					.ToType(),
				Messages.CustomerOrOrganizationDependingOnShipmentType)
		{
		}
	}

	class CustomerOrOrganizationInNoUpdateDocRestrictorAttribute : PXRestrictorAttribute
	{
		public CustomerOrOrganizationInNoUpdateDocRestrictorAttribute() :
			base(
				typeof(Where<Customer.type, IsNotNull,
					Or<Current<SOOrder.aRDocType>, Equal<ARDocType.noUpdate>,
					And<Current<SOOrder.behavior>, Equal<SOOrderTypeConstants.salesOrder>,
					And<BAccountR.type, In3<BAccountType.branchType, BAccountType.organizationBranchCombinedType, BAccountType.organizationType>>>>>),
				Messages.CustomerOrOrganization)
		{
		}
	}
}
