using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CR;

namespace PX.Objects.AR
{
	public sealed class ARInvoiceVisibilityRestriction : PXCacheExtension<ARInvoice>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}
		#region BranchID
		/// <summary>
		/// The identifier of the <see cref="Branch">branch</see> to which the document belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0)]
		[PXFormula(typeof(Switch<
				Case<Where<ARInvoice.customerLocationID, IsNotNull,
						And<Selector<ARInvoice.customerLocationID, Location.cBranchID>, IsNotNull>>,
					Selector<ARInvoice.customerLocationID, Location.cBranchID>,
				Case<Where<ARInvoice.customerID, IsNotNull, 
						And<Not<Selector<ARInvoice.customerID, Customer.cOrgBAccountID>, RestrictByBranch<Current2<ARInvoice.branchID>>>>>,
					Null,
				Case<Where<Current2<ARInvoice.branchID>, IsNotNull>,
					Current2<ARInvoice.branchID>>>>,
				Current<AccessInfo.branchID>>))]
		public Int32? BranchID{get;	set;}
		#endregion

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(branchID: typeof(ARInvoice.branchID))]
		public int? CustomerID { get; set; }
		#endregion
	}
}