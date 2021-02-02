using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.INKitSpecNonStkDet)]
	public partial class INKitSpecNonStkDet : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INKitSpecNonStkDet>.By<kitInventoryID, revisionID, lineNbr>
		{
			public static INKitSpecNonStkDet Find(PXGraph graph, int? kitInventoryID, string revisionID, int? lineNbr)
				=> FindBy(graph, kitInventoryID, revisionID, lineNbr);
		}
		public static class FK
		{
			public class KitInventoryItem : InventoryItem.PK.ForeignKeyOf<INKitSpecNonStkDet>.By<kitInventoryID> { }
			public class CompInventoryItem : InventoryItem.PK.ForeignKeyOf<INKitSpecNonStkDet>.By<compInventoryID> { }
			public class KitSpecHdr : INKitSpecHdr.PK.ForeignKeyOf<INKitSpecNonStkDet>.By<kitInventoryID, revisionID> { }
		}
		#endregion
		#region KitInventoryID
		public abstract class kitInventoryID : PX.Data.BQL.BqlInt.Field<kitInventoryID> { }
		protected Int32? _KitInventoryID;
        [Inventory(IsKey = true, DisplayName = "Inventory ID")]
		[PXRestrictor(typeof(Where<InventoryItem.kitItem, Equal<boolTrue>>), Messages.InventoryItemIsNotaKit)]
		[PXDefault(typeof(INKitSpecHdr.kitInventoryID))]
		public virtual Int32? KitInventoryID
		{
			get
			{
				return this._KitInventoryID;
			}
			set
			{
				this._KitInventoryID = value;
			}
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
		protected String _RevisionID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(INKitSpecHdr.revisionID))]
		[PXParent(typeof(FK.KitSpecHdr))]
		public virtual String RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(INKitSpecHdr))]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region CompInventoryID
		public abstract class compInventoryID : PX.Data.BQL.BqlInt.Field<compInventoryID> { }
		protected Int32? _CompInventoryID;
		[NonStockItem(DisplayName = "Component ID")]
		[PXRestrictor(typeof(Where<InventoryItem.kitItem, Equal<boolFalse>>), Messages.NonStockKitInKit)]
		[PXDefault()]
		[PXForeignReference(typeof(FK.CompInventoryItem))]
		public virtual Int32? CompInventoryID
		{
			get
			{
				return this._CompInventoryID;
			}
			set
			{
				this._CompInventoryID = value;
			}
		}
		#endregion

		#region DfltCompQty
		public abstract class dfltCompQty : PX.Data.BQL.BqlDecimal.Field<dfltCompQty> { }
		protected Decimal? _DfltCompQty;
		[PXDBQuantity(typeof(uOM), typeof(baseDfltCompQty), InventoryUnitType.BaseUnit, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Component Qty.")]
		public virtual Decimal? DfltCompQty
		{
			get
			{
				return this._DfltCompQty;
			}
			set
			{
				this._DfltCompQty = value;
			}
		}
		#endregion
		#region BaseDfltCompQty
		public abstract class baseDfltCompQty : PX.Data.BQL.BqlDecimal.Field<baseDfltCompQty> { }
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? BaseDfltCompQty
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecNonStkDet.compInventoryID>>>>))]
		[INUnit(typeof(INKitSpecNonStkDet.compInventoryID))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region AllowQtyVariation
		public abstract class allowQtyVariation : PX.Data.BQL.BqlBool.Field<allowQtyVariation> { }
		protected Boolean? _AllowQtyVariation;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Component Qty. Variance")]
		public virtual Boolean? AllowQtyVariation
		{
			get
			{
				return this._AllowQtyVariation;
			}
			set
			{
				this._AllowQtyVariation = value;
			}
		}
		#endregion
		#region MinCompQty
		public abstract class minCompQty : PX.Data.BQL.BqlDecimal.Field<minCompQty> { }
		protected Decimal? _MinCompQty;
		[PXDBQuantity(MinValue = 0)]
		//[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Min. Component Qty.")]
		public virtual Decimal? MinCompQty
		{
			get
			{
				return this._MinCompQty;
			}
			set
			{
				this._MinCompQty = value;
			}
		}
		#endregion
		#region MaxCompQty
		public abstract class maxCompQty : PX.Data.BQL.BqlDecimal.Field<maxCompQty> { }
		protected Decimal? _MaxCompQty;
		[PXDBQuantity(MinValue = 0)]
		//[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Max. Component Qty.")]
		public virtual Decimal? MaxCompQty
		{
			get
			{
				return this._MaxCompQty;
			}
			set
			{
				this._MaxCompQty = value;
			}
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
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
