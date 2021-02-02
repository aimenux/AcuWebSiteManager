using PX.Data;
using PX.Objects.CS;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AR
{
	public class ARInvoiceEntryVisibilityRestriction : PXGraphExtension<ARInvoiceEntry>
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

			string branchViewName = nameof(ARInvoiceEntry.CurrentDocument);
			string customerViewName = nameof(ARInvoiceEntry.Document);

			(string name, string viewName) branch = (nameof(ARInvoice.BranchID), branchViewName);

			List<(string name, string viewName)> fieldList = new List<(string name, string viewName)>();
			fieldList.Add((nameof(ARInvoice.CustomerID), customerViewName));
			fieldList.Add((nameof(ARInvoice.CustomerLocationID), customerViewName));

			Common.Utilities.SetDependentFieldsAfterBranch(script, branch, fieldList);
		}
	}
}
