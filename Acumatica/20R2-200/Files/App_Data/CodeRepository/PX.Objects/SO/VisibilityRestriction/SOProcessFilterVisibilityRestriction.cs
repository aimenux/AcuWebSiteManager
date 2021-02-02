﻿using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.SO
{
	public sealed class SOProcessFilterVisibilityRestriction : PXCacheExtension<SOProcessFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? CustomerID { get; set; }
		#endregion
	}
}