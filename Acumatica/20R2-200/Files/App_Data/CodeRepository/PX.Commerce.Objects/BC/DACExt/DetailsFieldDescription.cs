using PX.Commerce.Core;
using PX.CS;
using PX.Data;
using PX.Data.EP;
using PX.Objects.SO;
using System;

namespace PX.Commerce.Objects
{
	[PXNonInstantiatedExtension]
	public class BCSOLineExt : PXCacheExtension<SOLine>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public virtual String OrderType { get; set; }

		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public virtual String OrderNbr { get; set; }
		#endregion
	}

	public class BCAttributeExt : PXCacheExtension<CSAttribute>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }
		#region Attribute ID
		public abstract class attributeID : PX.Data.BQL.BqlString.Field<attributeID> { }
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public virtual string AttributeID { get; set; }

		#endregion
		
	}

	public class BCAttributeValueExt : PXCacheExtension<CSAttributeDetail>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }
		#region Value ID
		public abstract class valueID : PX.Data.BQL.BqlString.Field<valueID> { }
		protected String _OrderType;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public virtual string ValueID { get; set; }

		#endregion
		
	}
}
