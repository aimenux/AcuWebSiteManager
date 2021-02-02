using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Objects.GL;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	public class PRLocationExtAddress : PXCacheExtension<LocationExtAddress>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.payrollModule>();
		}

		#region CMPPayrollSubID
		public abstract class cMPPayrollSubID : BqlInt.Field<cMPPayrollSubID> { }
		[SubAccount(BqlField = typeof(Standalone.PRBranchLocation.cMPPayrollSubID), DisplayName = "Payroll Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PRBranchSubRequired(GLAccountSubSource.Branch)]
		public virtual Int32? CMPPayrollSubID { get; set; }
		#endregion
	}

	[Serializable]
	[PXTable(typeof(Location.locationID), typeof(Location.bAccountID), IsOptional = true)]
	public class PRBranchLocation : PXCacheExtension<Location>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.payrollModule>();
		}

		#region CMPPayrollSubID
		public abstract class cMPPayrollSubID : BqlInt.Field<cMPPayrollSubID> { }
		[SubAccount(DisplayName = "Payroll Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PRBranchSubRequired(GLAccountSubSource.Branch)]
		public virtual Int32? CMPPayrollSubID { get; set; }
		#endregion
	}
}

namespace PX.Objects.PR.Standalone
{
	[Serializable]
	[PXTable(typeof(Location.locationID), typeof(Location.bAccountID), IsOptional = true)]
	public class PRBranchLocation : PXCacheExtension<CR.Standalone.Location>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.payrollModule>();
		}

		#region CMPPayrollSubID
		public abstract class cMPPayrollSubID : BqlInt.Field<cMPPayrollSubID> { }
		[SubAccount(DisplayName = "Payroll Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? CMPPayrollSubID { get; set; }
		#endregion
	}
}

