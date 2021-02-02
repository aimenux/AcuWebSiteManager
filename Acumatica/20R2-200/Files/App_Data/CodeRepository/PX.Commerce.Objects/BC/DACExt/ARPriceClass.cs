using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Commerce.Core;

namespace PX.Commerce.Objects
{
	#region BCARPriceClassExt
	[PXNonInstantiatedExtension]
	public class BCARPriceClassExt : PXCacheExtension<ARPriceClass>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		#region PriceClassID
		public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public virtual string PriceClassID { get; set; }
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