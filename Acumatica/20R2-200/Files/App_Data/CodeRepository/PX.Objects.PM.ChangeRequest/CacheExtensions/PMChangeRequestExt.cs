using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM.ChangeRequest
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.ChangeRequest)]
	[PXPrimaryGraph(typeof(ChangeRequestEntry))]
	[Serializable]
	[PXEMailSource]
	public class PMChangeRequestExt : PXCacheExtension<PX.Objects.PM.PMChangeRequest>
	{
	}
}
