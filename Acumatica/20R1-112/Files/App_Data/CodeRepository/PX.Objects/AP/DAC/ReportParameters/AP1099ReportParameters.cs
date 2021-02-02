using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.GL.Attributes;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;

namespace PX.Objects.AP.DAC.ReportParameters
{
	public class AP1099ReportParameters: IBqlTable
	{
		#region OrganizationID

		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(false)]
		public virtual int? OrganizationID { get; set; }

		#endregion
		
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[Branch(
			sourceType: null,
			searchType: typeof(SearchFor<Branch.branchID>
			.In<
				SelectFrom<Branch>
					.InnerJoin<Organization>
						.On<Branch.organizationID.IsEqual<Organization.organizationID>
							.And<Organization.reporting1099ByBranches.IsEqual<True>>>
					.Where<Branch.organizationID.IsEqual<AP1099ReportParameters.organizationID.AsOptional.NoDefault>
						.And<MatchWithBranch<Branch.branchID>>>>),
			useDefaulting: false,
			addDefaultAttribute: false,
			onlyActive: false)]
		public int? BranchID { get; set; }
		#endregion

		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }

		[PXDBString(4, IsFixed = true)]
		[PXUIField(DisplayName = "1099 Year")]
		[PXSelector(typeof(Search4<AP1099Year.finYear,
			Where<AP1099Year.organizationID, Equal<Optional2<AP1099ReportParameters.organizationID>>,
				Or<Optional2<AP1099ReportParameters.organizationID>, IsNull>>,
			Aggregate<GroupBy<AP1099Year.finYear>>>))]
		public virtual String FinYear { get; set; }
		#endregion

		#region PayerBAccountID
		public abstract class payerBAccountID : Data.BQL.BqlInt.Field<payerBAccountID> { }
		[Payer1099Selector]
		public virtual int? PayerBAccountID
		{
			get;
			set;
		}
		#endregion

		#region FinYearByPayer
		public abstract class finYearByPayer : PX.Data.BQL.BqlString.Field<finYearByPayer> { }

		[PXDBString(4, IsFixed = true)]
		[PXUIField(DisplayName = "1099 Year")]
		// TODO: Do not remove commented attribute
		// This is temporary example for AC-134076, comments must be romoved after fix
		/*
		[PXSelector(typeof(SearchFor<AP1099Year.finYear>
			.In<
				SelectFrom<AP1099Year>
					.InnerJoin<Organization>
						.On<AP1099Year.organizationID.IsEqual<Organization.organizationID>>
					.InnerJoin<Branch>
						.On<Organization.organizationID.IsEqual<Branch.organizationID>>
					.Where<Organization.bAccountID.IsEqual<AP1099ReportParameters.payerBAccountID.AsOptional.NoDefault>
						.Or<Branch.bAccountID.IsEqual<AP1099ReportParameters.payerBAccountID.AsOptional.NoDefault>>>
					.AggregateTo<
						GroupBy<AP1099Year.finYear>>>))]
		*/
		[PXSelector(typeof(Search5<AP1099Year.finYear,
			InnerJoin<Organization, On<AP1099Year.organizationID.IsEqual<Organization.organizationID>>,
			InnerJoin<Branch, On<Organization.organizationID.IsEqual<Branch.organizationID>>>>,
			Where<Organization.bAccountID.IsEqual<AP1099ReportParameters.payerBAccountID.AsOptional.NoDefault>
				.Or<Branch.bAccountID.IsEqual<AP1099ReportParameters.payerBAccountID.AsOptional.NoDefault>>>,
			Aggregate<GroupBy<AP1099Year.finYear>>>))]
		public virtual string FinYearByPayer { get; set; }

		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : IBqlField { }

		[OrganizationTree(
			sourceOrganizationID: null,
			sourceBranchID: null,
			treeDataMember: typeof(AP1099PayerTreeSelect),
			onlyActive: true,
			SelectionMode = OrganizationTreeAttribute.SelectionModes.Branches)]
		public int? OrgBAccountID { get; set; }
		#endregion

	}
}
