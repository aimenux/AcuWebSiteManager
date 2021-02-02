using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.JointChecks.AP.Services;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN.PhysicalInventory;

namespace PX.Objects.Common
{
	public class ReportFunctions
	{
        #region Common

	    public bool FeatureInstalled(object feature)
	    {
	        return PXAccess.FeatureInstalled((string)feature);
	    }

        #endregion

        #region GL

        public object GetOrganizationIDByCD(object organizationCD)
	    {
	        return PXAccess.GetOrganizationID((string)organizationCD);
	    }

	    public object GetOrganizationCDByID(object organizationID)
	    {
	        return PXAccess.GetOrganizationCD((int?)organizationID);
	    }

        public object GetBranchIDByCD(object branchCD)
	    {
	        return PXAccess.GetBranchID((string) branchCD);
	    }

	    public object GetBranchCDByID(object branchID)
	    {
	        return PXAccess.GetBranchCD((int?)branchID);
	    }

        public object GetParentOrganizationID(object branchID)
	    {
	        return PXAccess.GetParentOrganizationID((int?) branchID);
	    }

        public object GetOrganizationFinPeriodIDForMaster(object organizationID, object masterFinPeriodID)
	    {
	        var finPeriodRepository = PXGraphClassExtensions.GetService<IFinPeriodRepository>(null);

	        return finPeriodRepository.FindFinPeriodIDByMasterPeriodID(
	            (int?)organizationID,
	            (string) masterFinPeriodID,
	            readAllAndCacheToPXContext: true);
	    }

		private PXAccess.Organization GetOrganizationByBAccountCD(string bAccountCD)
		{
			PXGraph graph = PXGraph.CreateInstance<PXGraph>();
			BAccountR bAccount = SelectFrom<BAccountR>
				.Where<BAccountR.acctCD.IsEqual<@P.AsString>>
				.View.Select(graph, bAccountCD);

			return PXAccess.GetOrganizationByBAccountID(bAccount?.BAccountID)
				?? PXAccess.GetParentOrganization(PXAccess.GetBranchByBAccountID(bAccount?.BAccountID)?.BranchID);
		}

		public object GetOrganizationCDByBAccountCD(object bAccountCD)
		{
			return GetOrganizationByBAccountCD((string)bAccountCD)?.OrganizationCD;
		}

		public object GetOrganizationIDByBAccountCD(object bAccountCD)
		{
			return GetOrganizationByBAccountCD((string)bAccountCD)?.OrganizationID;
		}
		#endregion

		#region IN

		public object GetFullItemClassDescription(object itemClassCD)
		{
			string cd = itemClassCD as string;
			if (cd == null) return null;
			return IN.ItemClassTree.Instance.GetFullItemClassDescription(cd);
		}

		public bool IsInventoryLocationLocked(int? siteID, int? locationID, int? inventoryID)
		{
			if (siteID == null || locationID == null || inventoryID == null)
				return false;

			return new PILocksInspector(siteID.Value).IsInventoryLocationLocked(inventoryID, locationID, null);
		}
		#endregion

		#region CN
		private readonly Lazy<JointCheckPrintService> jointCheckPrintService = new Lazy<JointCheckPrintService>(() => new JointCheckPrintService());

		public bool ShouldShowJointPayees(string documentType, string referenceNumber)
		{
			return jointCheckPrintService.Value.DoJointPayeePaymentsWithPositiveAmountExist(documentType, referenceNumber);
		}

		public string GetJointPayeesSingleLine(string documentType, string referenceNumber)
		{
			return jointCheckPrintService.Value.GetJointPayeesSingleLine(documentType, referenceNumber);
		}

		public string GetJointPayeesMultiline(string documentType, string referenceNumber)
		{
			return jointCheckPrintService.Value.GetJointPayeesMultiline(documentType, referenceNumber);
		}
		#endregion
	}
}
