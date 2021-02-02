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
	#region LocationExt
	public class BCLocationExt : PXCacheExtension<Location>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		#region LocationCD
		public abstract class locationCD : PX.Data.BQL.BqlString.Field<locationCD> { }
		protected String _LocationCD;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public virtual String LocationCD
		{
			get
			{
				return this._LocationCD;
			}
			set
			{
				this._LocationCD = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXFieldDescription]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
	}
	#endregion
}