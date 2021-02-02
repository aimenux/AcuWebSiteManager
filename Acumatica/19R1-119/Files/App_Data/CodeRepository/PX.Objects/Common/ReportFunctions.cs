using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;

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

        #endregion

        #region IN

        public object GetFullItemClassDescription(object itemClassCD)
		{
			string cd = itemClassCD as string;
			if (cd == null) return null;
			return IN.ItemClassTree.Instance.GetFullItemClassDescription(cd);
		}

		#endregion
	}
}
