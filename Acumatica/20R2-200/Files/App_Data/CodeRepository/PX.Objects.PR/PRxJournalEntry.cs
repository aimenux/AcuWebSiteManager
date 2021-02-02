using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.PR
{
	public class PRxJournalEntry : PXGraphExtension<JournalEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		public delegate IDocGraphCreator GetGraphCreatorDelegate(string tranModule, string batchType);
		[PXOverride]
		public virtual IDocGraphCreator GetGraphCreator(string tranModule, string batchType, GetGraphCreatorDelegate baseMethod)
		{
			if (tranModule == BatchModule.PR)
			{
				return new PRDocGraphCreator();
			}

			return baseMethod(tranModule, batchType);
		}
	}
}
