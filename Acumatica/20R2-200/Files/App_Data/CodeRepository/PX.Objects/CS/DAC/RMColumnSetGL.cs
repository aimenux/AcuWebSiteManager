using System;
using PX.Data;
using PX.CS;

namespace PX.Objects.CS
{
	[Serializable]
	[PXPrimaryGraph(typeof(RMColumnSetMaint))]
	public partial class RMColumnSetGL : PXCacheExtension<RMColumnSet>
	{		
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected String _Type;
		[PXDBString(2, IsFixed = true)]
		[RMType.List()]
		[PXDefault(CS.RMType.GL)]
		[PXUIField(DisplayName = "Type", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion			
	}
}
