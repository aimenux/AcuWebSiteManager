using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.SO
{
	public class SOOrderEntryVisibilityRestriction : PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public delegate void CopyPasteGetScriptDelegate(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers);

		[PXOverride]
		public void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers,
			CopyPasteGetScriptDelegate baseMethod)
		{
			// We need to process fields together that are related to the Branch and Customer for proper validation. For this:
			// 1) set the right order of the fields
			// 2) insert dependent fields after the BranchID field
			// 3) all fields must belong to the same view.

			string branchViewName = nameof(SOOrderEntry.CurrentDocument) + ": 1";
			string customerViewName = nameof(SOOrderEntry.Document);

			(string Name, string ViewName) branch = (nameof(SOOrder.BranchID), branchViewName);

			List<(string Name, string ViewName)> fieldList = new List<(string Name, string ViewName)>();
			fieldList.Add((nameof(SOOrder.CustomerID), customerViewName));
			fieldList.Add((nameof(SOOrder.CustomerLocationID), customerViewName));

			Common.Utilities.SetDependentFieldsAfterBranch(script, branch, fieldList);

			baseMethod.Invoke(isImportSimple, script, containers);
		}
	}


	public sealed class SOOrderVisibilityRestriction : PXCacheExtension<SOOrder>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BranchID
		[Branch(typeof(AccessInfo.branchID), IsDetail = false)]
		[PXFormula(typeof(Switch<
				Case<Where<SOOrder.customerLocationID, IsNotNull,
						And<Selector<SOOrder.customerLocationID, Location.cBranchID>, IsNotNull>>,
					Selector<SOOrder.customerLocationID, Location.cBranchID>,
				Case<Where<SOOrder.customerID, IsNotNull,
						And<Not<Selector<SOOrder.customerID, Customer.cOrgBAccountID>, RestrictByBranch<Current2<SOOrder.branchID>>>>>,
					Null,
				Case<Where<Current2<SOOrder.branchID>, IsNotNull>,
					Current2<SOOrder.branchID>>>>,
				Current<AccessInfo.branchID>>))]
		public int? BranchID { get; set; }
		#endregion

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccountR.cOrgBAccountID), branchID: typeof(SOOrder.branchID))]
		public int? CustomerID { get; set; }
		#endregion
	}
}
