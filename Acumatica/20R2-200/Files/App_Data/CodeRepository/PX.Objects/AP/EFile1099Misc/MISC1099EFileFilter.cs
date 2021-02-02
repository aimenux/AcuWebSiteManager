using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;

namespace PX.Objects.AP
{
	public class AP1099PayerTreeSelect : PXSelectOrganizationTree
	{
		public AP1099PayerTreeSelect(PXGraph graph) : base(graph) { }

		public AP1099PayerTreeSelect(PXGraph graph, Delegate handler) : base(graph, handler) { }

		public override IEnumerable tree([PXString] string AcctCD)
		{
			List<BranchItem> result = new List<BranchItem>();
			foreach (PXResult<BAccountR, Branch, Organization> row in
				SelectFrom<BAccountR>
					.LeftJoin<Branch>
						.On<BAccountR.bAccountID.IsEqual<Branch.bAccountID>>
					.InnerJoin<Organization>
						.On<Branch.organizationID.IsEqual<Organization.organizationID>
							.Or<BAccountR.bAccountID.IsEqual<Organization.bAccountID>>>
					.Where<Brackets<Branch.branchID.IsNull
							.Or<Branch.bAccountID.IsEqual<Organization.bAccountID>>
							.Or<Branch.branchID.IsNotNull
								.And<Organization.reporting1099ByBranches.IsEqual<True>>>>
						.And<MatchWithBranch<Branch.branchID>>
						.And<MatchWithOrganization<Organization.organizationID>>
						.And<Branch.branchID.IsNull.Or<Branch.active.IsEqual<True>>>
						.And<Organization.organizationID.IsNull.Or<Organization.active.IsEqual<True>>>>
					.View
					.Select(_Graph))
			{
				BAccountR bAccount = row;
				Branch branch = row;
				Organization organization = row;

				BranchItem item = new BranchItem
				{
					BAccountID = bAccount.BAccountID,
					AcctCD = bAccount.AcctCD,
					AcctName = bAccount.AcctName
				};

				if (branch?.BAccountID != null && organization.BAccountID != branch.BAccountID)
				{
					item.ParentBAccountID = PXAccess.GetParentOrganization(branch.BranchID).BAccountID;
				}

				result.Add(item);
			}
			return result;
		}
	}

	public class AP1099ReportingPayerTreeSelect : PXSelectOrganizationTree
	{
		public AP1099ReportingPayerTreeSelect(PXGraph graph) : base(graph) {}

		public AP1099ReportingPayerTreeSelect(PXGraph graph, Delegate handler) : base(graph, handler) {}

		public override IEnumerable tree([PXString] string AcctCD)
		{
			List<BranchItem> result = new List<BranchItem>();
			List<(Branch Branch, BAccountR BAccount)> branches =
				SelectFrom<Branch>
					.InnerJoin<BAccountR>
						.On<Branch.bAccountID.IsEqual<BAccountR.bAccountID>>
					.Where<Branch.active.IsEqual<True>
						.And<MatchWithBranch<Branch.branchID>>>
				.View
				.Select(_Graph)
				.AsEnumerable()
				.Cast<PXResult<Branch, BAccountR>>()
				.Select(row => ((Branch)row, (BAccountR)row))
				.ToList();

			foreach (PXResult<Organization, BAccountR> orgBAccountPair in
				SelectFrom<Organization>
					.InnerJoin<BAccountR>
						.On<Organization.bAccountID.IsEqual<BAccountR.bAccountID>>
					.Where<Organization.active.IsEqual<True>
						.And<Organization.reporting1099.IsEqual<True>
							.Or<Organization.reporting1099ByBranches.IsEqual<True>>>
						.And<MatchWithOrganization<Organization.organizationID>>>
				.View
				.Select(_Graph))
			{
				Organization organization = orgBAccountPair;
				BAccountR orgBAccount = orgBAccountPair;
				bool addOrganization = true;

				if (organization.Reporting1099ByBranches == true)
				{
					addOrganization = false;
					foreach((Branch Branch, BAccountR BAccount) branchBAccountPair in branches
						.Where(pair => pair.Branch.OrganizationID == organization.OrganizationID && pair.Branch.Reporting1099 == true))
					{
						addOrganization = true;
						result.Add(new BranchItem
						{
							BAccountID = branchBAccountPair.BAccount.BAccountID,
							AcctCD = branchBAccountPair.BAccount.AcctCD,
							AcctName = branchBAccountPair.BAccount.AcctName,
							ParentBAccountID = organization.BAccountID
						});
					}
				}

				if(addOrganization)
				{
					result.Add(new BranchItem
					{
						BAccountID = orgBAccount.BAccountID,
						AcctCD = orgBAccount.AcctCD,
						AcctName = orgBAccount.AcctName,
					});
				}
			}
			return result;
		}
	}

	[Serializable]
	public class MISC1099EFileFilter : IBqlTable
	{
		#region OrganizationID

		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		// Selector need for slot
		[Organization(
			onlyActive: true,
			defaultingSource: typeof(Coalesce<
				SearchFor<Organization.organizationID>
				.In<
					SelectFrom<Organization>
					.InnerJoin<Branch>
						.On<Organization.organizationID.IsEqual<Branch.organizationID>>
					.Where<Branch.branchID.IsEqual<AccessInfo.branchID.FromCurrent>
						.And<Organization.reporting1099.IsEqual<True>
							.Or<Organization.reporting1099ByBranches.IsEqual<True>>>
						.And<MatchWithBranch<Branch.branchID>>>>,
				SearchFor<Organization.organizationID>
				.In<
					SelectFrom<Organization>
					.InnerJoin<Branch>
						.On<Organization.organizationID.IsEqual<Branch.organizationID>>
					.Where<Brackets<Organization.reporting1099.IsEqual<True>
							.Or<Organization.reporting1099ByBranches.IsEqual<True>>>
						.And<MatchWithBranch<Branch.branchID>>>>>),
			DisplayName = "Transmitter Company")]
		[PXRestrictor(
			typeof(Where<Organization.reporting1099.IsEqual<True>
				.Or<Organization.reporting1099ByBranches.IsEqual<True>>>), 
			Messages.EFilingIsAvailableOnlyCompaniesWithEnabled1099)]
		public virtual int? OrganizationID { get; set; }

		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		// Selector need for slot
		[Branch(
			sourceType: null,
			searchType: typeof(SearchFor<Branch.branchID>
			.In<
				SelectFrom<Branch>
					.InnerJoin<Organization>
						.On<Branch.organizationID.IsEqual<Organization.organizationID>
							.And<Organization.reporting1099ByBranches.IsEqual<True>>>
					.Where<Branch.organizationID.IsEqual<MISC1099EFileFilter.organizationID.FromCurrent>
						.And<MatchWithBranch<Branch.branchID>>>>),
			useDefaulting: false,
			DisplayName = "Transmitter Branch")]
		[PXRestrictor(typeof(Where<Branch.reporting1099.IsEqual<True>>), Messages.EFilingIsAvailableOnlyBranchWithEnabled1099)]
		[PXUIEnabled(typeof(Where<Selector<organizationID, Organization.reporting1099ByBranches>, Equal<True>>))]
		[PXUIRequired(typeof(Where<Selector<organizationID, Organization.reporting1099ByBranches>, Equal<True>>))]
		public virtual int? BranchID { get; set; }
		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : IBqlField { }

		[OrganizationTree(
			sourceOrganizationID: typeof(organizationID),
			sourceBranchID: typeof(branchID),
			treeDataMember: typeof(AP1099ReportingPayerTreeSelect),
			onlyActive: true,
			DisplayName = "Transmitter",
			SelectionMode = OrganizationTreeAttribute.SelectionModes.Branches)]
		public int? OrgBAccountID { get; set; }
		#endregion

		#region FinYear

		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		protected String _FinYear;
		[PXDBString(4, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "1099 Year", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<AP1099Year.finYear,
			Where<AP1099Year.organizationID, Equal<Optional<MISC1099EFileFilter.organizationID>>>>))]
		public virtual String FinYear
		{
			get
			{
				return this._FinYear;
			}
			set
			{
				this._FinYear = value;
			}
		}
		#endregion
		#region Include
		public abstract class include : PX.Data.BQL.BqlString.Field<include>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { TransmitterOnly, AllMarkedOrganizations },
					new string[] { Messages.TransmitterOnly, Messages.AllMarkedOrganizations })
				{ }
			}
			public const string TransmitterOnly = "T";
			public const string AllMarkedOrganizations = "A";

			public class transmitterOnly : PX.Data.BQL.BqlString.Constant<transmitterOnly>
			{
				public transmitterOnly() : base(TransmitterOnly) { }
			}

			public class allMarkedOrganizations : PX.Data.BQL.BqlString.Constant<allMarkedOrganizations>
			{
				public allMarkedOrganizations() : base(AllMarkedOrganizations) { }
			}
		}
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Prepare For")]
		[include.List]
		[PXDefault(include.TransmitterOnly)]
		public virtual string Include { get; set; }
		#endregion
		#region Box7
		public abstract class box7 : PX.Data.BQL.BqlString.Field<box7>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Box7All, Box7Equal, Box7NotEqual },
					new string[] { Messages.Box7All, Messages.Box7Equal, Messages.Box7NotEqual }) { }
			}

			public const string Box7All = "AL";
			public const string Box7Equal = "EQ";
			public const string Box7NotEqual = "NE";

			public class box7All : PX.Data.BQL.BqlString.Constant<box7All>
			{
				public box7All() : base(Box7All) { }
			}

			public class box7Equal : PX.Data.BQL.BqlString.Constant<box7Equal>
			{
				public box7Equal() : base(Box7Equal) { }
			}

			public class box7NotEqual : PX.Data.BQL.BqlString.Constant<box7NotEqual>
			{
				public box7NotEqual() : base(Box7NotEqual) { }
			}

			public class box7Nbr : PX.Data.BQL.BqlShort.Constant<box7Nbr>
			{
				public box7Nbr() : base(7) { }
			}
		}

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "NEC (Box 7)")]
		[box7.List]
		[PXDefault(box7.Box7All)]
		public virtual string Box7 { get; set; }
		#endregion

		#region IsPriorYear

		public abstract class isPriorYear : PX.Data.BQL.BqlBool.Field<isPriorYear> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Prior Year")]
		public virtual bool? IsPriorYear { get; set; }

		#endregion

		#region IsCorrectionReturn

		public abstract class isCorrectionReturn : PX.Data.BQL.BqlBool.Field<isCorrectionReturn> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Correction File")]
		public virtual bool? IsCorrectionReturn { get; set; }

		#endregion

		#region IsLastFiling

		public abstract class isLastFiling : PX.Data.BQL.BqlBool.Field<isLastFiling> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Last Filing")]
		public virtual bool? IsLastFiling { get; set; }

		#endregion

		#region ReportingDirectSalesOnly

		public abstract class reportingDirectSalesOnly : PX.Data.BQL.BqlBool.Field<reportingDirectSalesOnly> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Direct Sales Only")]
		public virtual bool? ReportingDirectSalesOnly { get; set; }

		#endregion

		#region IsTestMode

		public abstract class isTestMode : PX.Data.BQL.BqlBool.Field<isTestMode> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Test File")]
		public virtual bool? IsTestMode { get; set; }

		#endregion
	}
}