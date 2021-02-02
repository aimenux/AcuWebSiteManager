using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.PM
{
	public class PMQuoteMaintVisibilityRestriction : PXGraphExtension<PMQuoteMaint>
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
			baseMethod.Invoke(isImportSimple, script, containers);

			// We need to process fields together that are related to the Branch and Customer for proper validation. For this:
			// 1) set the right order of the fields
			// 2) insert dependent fields after the BranchID field
			// 3) all fields must belong to the same view.

			string branchViewName = nameof(PMQuoteMaint.QuoteCurrent) + ": 1";
			string customerViewName = nameof(PMQuoteMaint.Quote);

			(string name, string viewName) branch = (nameof(PMQuote.BranchID), branchViewName);

			List<(string name, string viewName)> fieldList = new List<(string name, string viewName)>();
			fieldList.Add((nameof(PMQuote.BAccountID), customerViewName));
			fieldList.Add((nameof(PMQuote.LocationID), branchViewName));

			Common.Utilities.SetDependentFieldsAfterBranch(script, branch, fieldList);
		}
	}
}
