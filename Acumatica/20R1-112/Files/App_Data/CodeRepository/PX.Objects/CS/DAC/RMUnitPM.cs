using System;
using PX.Data;
using PX.CS;

namespace PX.Objects.CS
{
	[Serializable]
	public partial class RMUnitPM : PXCacheExtension<RMUnitGL, RMUnit>
	{		
		#region DataSourceID
		public abstract class dataSourceID : PX.Data.BQL.BqlInt.Field<dataSourceID> { }
		protected Int32? _DataSourceID;
		[RMDataSourcePM()]
		public virtual Int32? DataSourceID
		{
			get
			{
				return this._DataSourceID;
			}
			set
			{
				this._DataSourceID = value;
			}
		}
		#endregion
	}
}
