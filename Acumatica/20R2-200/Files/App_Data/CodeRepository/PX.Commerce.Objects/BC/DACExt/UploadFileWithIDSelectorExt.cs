using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.CR;
using PX.Data.EP;
using PX.Objects.IN;
using PX.SM;
using PX.Commerce.Core;

namespace PX.Commerce.Objects
{
	#region UploadFileWithIDSelectorExt
	[PXNonInstantiatedExtension]
	public sealed class BCUploadFileWithIDSelectorExt : PXCacheExtension<UploadFileWithIDSelector>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		#region FileID
		public abstract class fileID : PX.Data.BQL.BqlGuid.Field<fileID> { }
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public Guid? FileID { get; set; }
		#endregion
	}
	#endregion
}