using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[Serializable]
	public partial class INCostSubItemXRef : PX.Data.IBqlTable
	{
		#region Keys
        public class PK : PrimaryKeyOf<INCostSubItemXRef>.By<subItemID, costSubItemID>
		{
			public static INCostSubItemXRef Find(PXGraph graph, int? subItemID, int? costSubItemID) => FindBy(graph, subItemID, costSubItemID);
		}
		public static class FK
		{
			public class SubItem : IN.INSubItem.PK.ForeignKeyOf<INCostSubItemXRef>.By<subItemID> { }
			public class CostSubItem : IN.INSubItem.PK.ForeignKeyOf<INCostSubItemXRef>.By<costSubItemID> { }
		}
        #endregion
        #region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region CostSubItemID
		public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
		protected Int32? _CostSubItemID;
		[SubItem(IsKey = true)]
		public virtual Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}
