using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Objects.PO;
using System;

namespace PX.Objects.AP
{
	public class VerndorNonEmployeeOrOrganizationRestrictorAttribute : PXRestrictorAttribute
	{
		public VerndorNonEmployeeOrOrganizationRestrictorAttribute() :
			base(typeof(Where<BAccountR.type, In3<BAccountType.branchType, BAccountType.organizationType,
				BAccountType.organizationBranchCombinedType, BAccountType.vendorType, BAccountType.combinedType>>),
			Messages.VendorNonEmployeeOrOrganization)
		{
		}

		public VerndorNonEmployeeOrOrganizationRestrictorAttribute(Type receiptType) :
			base(BqlTemplate.OfCondition<
				Where<Current<BqlPlaceholder.A>, NotEqual<POReceiptType.transferreceipt>, And<Vendor.status, IsNotNull,
						And<BAccountR.type, In3<BAccountType.vendorType, BAccountType.combinedType>,
					Or<Current<BqlPlaceholder.A>, Equal<POReceiptType.transferreceipt>, And<BAccountR.type, In3<BAccountType.branchType,
						BAccountType.organizationType, BAccountType.organizationBranchCombinedType, BAccountType.combinedType>>>>>>>
				.Replace<BqlPlaceholder.A>(receiptType)
				.ToType(),
			Messages.VendorNonEmployeeOrOrganizationDependingOnReceiptType)
		{
		}
	}
}
