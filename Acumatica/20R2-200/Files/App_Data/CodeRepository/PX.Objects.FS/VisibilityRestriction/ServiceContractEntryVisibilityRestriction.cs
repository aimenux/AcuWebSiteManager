using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
	public class ServiceContractEntryVisibilityRestriction : PXGraphExtension<ServiceContractEntry>
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

			string branchViewName = nameof(ServiceContractEntry.ServiceContractRecords) + ": 1";
			string customerViewName = nameof(ServiceContractEntry.ServiceContractRecords);

			(string name, string viewName) branch = (nameof(FSServiceContract.BranchID), branchViewName);

			List<(string name, string viewName)> fieldList = new List<(string name, string viewName)>();
			fieldList.Add((nameof(FSServiceContract.CustomerID), customerViewName));
			fieldList.Add((nameof(FSServiceContract.CustomerLocationID), customerViewName));
			fieldList.Add((nameof(FSServiceContract.BillCustomerID), branchViewName));
			fieldList.Add((nameof(FSServiceContract.BillLocationID), branchViewName));

			Common.Utilities.SetDependentFieldsAfterBranch(script, branch, fieldList);
		}

		public virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.billCustomerID> e)
		{
			if (e.Row == null)
				return;

			FSServiceContract fSServiceContractRow = (FSServiceContract)e.Row;
			Base.SetBillTo(fSServiceContractRow);
		}

		public virtual void _(Events.FieldUpdated<FSServiceContract, FSServiceContract.branchID> e)
		{
			if (e.Row == null)
				return;

			FSServiceContract fSServiceContractRow = (FSServiceContract)e.Row;
			Base.SetBillTo(fSServiceContractRow);
		}


	}
}
