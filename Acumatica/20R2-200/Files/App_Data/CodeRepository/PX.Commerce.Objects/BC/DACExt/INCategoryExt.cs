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
	#region INCategoryExt
	[PXNonInstantiatedExtension]
	public class BCINCategoryExt : PXCacheExtension<INCategory>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		#region CategoryID
		public abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }
		protected int? _CategoryID;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public virtual int? CategoryID { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public virtual String Description { get; set; }
		#endregion
	}
	#endregion
}