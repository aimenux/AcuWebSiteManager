using System;

using PX.Data;
using PX.Objects.GL.DAC;
using PX.Objects.CR;

namespace PX.Objects.GL.Attributes
{
	[PXDBInt]
	[PXInt]
	[PXUIField(DisplayName = Messages.Company, FieldClass = "BRANCH")]
	public class OrganizationAttribute : AcctSubAttribute 
		{
		public const string _DimensionName = "BIZACCT";

		protected static readonly Type _selectorSource;
		protected static readonly Type _defaultingSource;

		static OrganizationAttribute() {
			_selectorSource = typeof(Search<Organization.organizationID, Where<MatchWithOrganization<Organization.organizationID>>>);

			_defaultingSource = typeof(Search2<Organization.organizationID,
						InnerJoin<Branch,
							On<Organization.organizationID, Equal<Branch.organizationID>>>,
						Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>,
							And<MatchWithBranch<Branch.branchID>>>>);
		}

		public OrganizationAttribute(bool onlyActive = true)
		    : this(onlyActive, _selectorSource, _defaultingSource)
		{

		}

		public OrganizationAttribute(bool onlyActive, Type defaultingSource)
			: this(onlyActive, _selectorSource, defaultingSource)
		{

		}

		public OrganizationAttribute(bool onlyActive, Type selectorSource, Type defaultingSource)
		{
			PXDimensionSelectorAttribute attr =
				new PXDimensionSelectorAttribute(_DimensionName,
					selectorSource,
					typeof(Organization.organizationCD),
					typeof(Organization.organizationCD), typeof(Organization.organizationName))
				{
					ValidComboRequired = true,
					DescriptionField = typeof(Organization.organizationName)
				};

			_Attributes.Add(attr);

			_Attributes.Add(defaultingSource != null ? new PXDefaultAttribute(defaultingSource) : new PXDefaultAttribute());

			if (onlyActive)
			{
				_Attributes.Add(new PXRestrictorAttribute(typeof(Where<Organization.active, Equal<True>>), Messages.TheCompanyIsInactive));
			}

			Initialize();
		}


	}

	public class CustomerVendorRestrictorAttribute : PXRestrictorAttribute
	{
		public CustomerVendorRestrictorAttribute() : 
			base(
				typeof(Where<BAccountR.type, NotEqual<BAccountType.branchType>,
					And<BAccountR.type, NotEqual<BAccountType.organizationType>,
					And<BAccountR.type, NotEqual<BAccountType.organizationBranchCombinedType>,
					And<BAccountR.type, NotEqual<BAccountType.prospectType>>>>>),
				Messages.CustomerVendor)
		{
		}
	}

}
