using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INItemLotSerial)]
    public partial class INItemLotSerial : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemLotSerial>.By<inventoryID, lotSerialNbr>
		{
			public static INItemLotSerial Find(PXGraph graph, int? inventoryID, string lotSerialNbr) => FindBy(graph, inventoryID, lotSerialNbr);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemLotSerial>.By<inventoryID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(IsKey = true)]
		[PXDefault()]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion				
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[PXDefault()]
		[LotSerialNbr(IsKey = true)]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion
		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		protected Decimal? _QtyAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvail
		{
			get
			{
				return this._QtyAvail;
			}
			set
			{
				this._QtyAvail = value;
			}
		}
		#endregion
		#region QtyHardAvail
		public abstract class qtyHardAvail : PX.Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
		protected Decimal? _QtyHardAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Hard Available")]
		public virtual Decimal? QtyHardAvail
		{
			get
			{
				return this._QtyHardAvail;
			}
			set
			{
				this._QtyHardAvail = value;
			}
		}
		#endregion
		#region QtyActual
		public abstract class qtyActual : PX.Data.BQL.BqlDecimal.Field<qtyActual> { }
		protected decimal? _QtyActual;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available for Issue")]
		public virtual decimal? QtyActual
		{
			get { return _QtyActual; }
			set { _QtyActual = value; }
		}
		#endregion
		#region QtyInTransit
		public abstract class qtyInTransit : PX.Data.BQL.BqlDecimal.Field<qtyInTransit> { }
		protected Decimal? _QtyInTransit;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtyInTransit
		{
			get
			{
				return this._QtyInTransit;
			}
			set
			{
				this._QtyInTransit = value;
			}
		}
		#endregion
		#region QtyOrig
		public abstract class qtyOrig : PX.Data.BQL.BqlDecimal.Field<qtyOrig> { }
		/// <summary>
		/// The field is used only for When Used Serial Numbers. It stores the quantity of the first IN Document
		/// released the serial number (<c>-1</c>, <c>1</c>) or <c>null</c> if it was not released yet. 
		/// </summary>
		[PXDBQuantity]
		public virtual decimal? QtyOrig { get; set; }
		#endregion
		#region QtyOnReceipt
		public abstract class qtyOnReceipt : PX.Data.BQL.BqlDecimal.Field<qtyOnReceipt> { }
		/// <summary>
		/// The field is used to store receipts Qty separately from issues.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyOnReceipt
		{
			get;
			set;
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Expiry Date")]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region UpdateExpireDate
		protected bool? _UpdateExpireDate;
		[PXBool()]
		public virtual bool? UpdateExpireDate
		{
			get
			{
				return this._UpdateExpireDate;
			}
			set
			{
				this._UpdateExpireDate = value;
			}
		}
		#endregion		
		#region LotSerTrack
		public abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
		protected String _LotSerTrack;
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public virtual String LotSerTrack
		{
			get
			{
				return this._LotSerTrack;
			}
			set
			{
				this._LotSerTrack = value;
			}
		}
		#endregion
		#region LotSerAssign
		public abstract class lotSerAssign : PX.Data.BQL.BqlString.Field<lotSerAssign> { }
		protected String _LotSerAssign;
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public virtual String LotSerAssign
		{
			get
			{
				return this._LotSerAssign;
			}
			set
			{
				this._LotSerAssign = value;
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
