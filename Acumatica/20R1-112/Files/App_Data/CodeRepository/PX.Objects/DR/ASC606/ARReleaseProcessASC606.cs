using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.DR
{
	public class ARReleaseProcessASC606 : PXGraphExtension<ARReleaseProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.aSC606>();
		}

		public delegate DRProcess CreateDRProcessDelegate();
		[PXOverride]
		public virtual DRProcess CreateDRProcess(CreateDRProcessDelegate baseMethod)
		{
			return PXGraph.CreateInstance<DRSingleProcess>();
		}
	}
}
