using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;
using PX.Objects.TX.Descriptor;

namespace PX.Objects.TX.DAC.ReportParameters
{
	public class TaxPeriodReportParameters: IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(false)]
		public int? OrganizationID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[TaxPeriodFilterBranch(typeof(TaxPeriodReportParameters.organizationID), hideBranchField: false)]
		public int? BranchID { get; set; }
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor(typeof(Search<Vendor.bAccountID, Where<Vendor.taxAgency, Equal<True>>>), DisplayName = "Tax Agency")]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		
		#region TaxPeriodID
		public abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }
		protected string _TaxPeriodID;
		[GL.FinPeriodID]
		[PXSelector(typeof(Search<TaxPeriod.taxPeriodID,
				Where<TaxPeriod.organizationID, Equal<Optional2<TaxPeriodForReportShowing.organizationID>>,
					And<TaxPeriod.vendorID, Equal<Optional2<TaxPeriodForReportShowing.vendorID>>,
						And<TaxPeriod.status, NotEqual<TaxPeriodStatus.open>>>>>),
			typeof(TaxPeriod.taxPeriodID), typeof(TaxPeriod.startDateUI), typeof(TaxPeriod.endDateUI), typeof(TaxPeriod.status))]
		[PXUIField(DisplayName = "Tax Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}
			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion

		#region TaxPeriodIDByBAccount
		public abstract class taxPeriodIDByBAccount : PX.Data.BQL.BqlString.Field<taxPeriodID> { }

		[FinPeriodID]
		[PXSelector(typeof(Search<TaxPeriod.taxPeriodID,
				Where<TaxPeriod.organizationID, Suit<Optional2<orgBAccountID>>,
					And<TaxPeriod.vendorID, Equal<Optional2<vendorID>>,
					And<TaxPeriod.status, NotEqual<TaxPeriodStatus.open>>>>>),
			typeof(TaxPeriod.taxPeriodID), typeof(TaxPeriod.startDateUI), typeof(TaxPeriod.endDateUI), typeof(TaxPeriod.status))]
		[PXUIField(DisplayName = "Tax Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string TaxPeriodIDByBAccount
		{
			get;
			set;
		}
		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : IBqlField { }

		[OrganizationTree(
			sourceOrganizationID: typeof(organizationID),
			sourceBranchID: typeof(branchID),
			treeDataMember: typeof(TaxTreeSelect),
			onlyActive: true,
			SelectionMode = OrganizationTreeAttribute.SelectionModes.Branches)]
		public int? OrgBAccountID { get; set; }
		#endregion

	}

	public class TaxTreeSelect : PXSelectOrganizationTree
	{
		public TaxTreeSelect(PXGraph graph) : base(graph) { }

		public TaxTreeSelect(PXGraph graph, Delegate handler) : base(graph, handler) { }

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
								.And<Organization.fileTaxesByBranches.IsEqual<True>>>>
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

}
