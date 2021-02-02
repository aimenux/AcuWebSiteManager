using System;
using PX.Data;
using PX.Objects.SO;
using System.Collections.Generic;
using PX.Commerce.Core;

namespace PX.Commerce.Objects
{
    [Serializable]
	public class BCSOOrderExt : PXCacheExtension<SOOrder>
    {
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXAppendSelectorColumns(typeof(SOOrder.customerRefNbr))]
		public virtual String OrderNbr { get; set; }

		#region ExternalOrderOriginal
		public abstract class externalOrderOriginal : PX.Data.BQL.BqlBool.Field<externalOrderOriginal> { }
		[PXDBBool()]
		[PXUIField(DisplayName = "External Order Original")]
		public virtual Boolean? ExternalOrderOriginal { get; set; }
		#endregion
	}
}