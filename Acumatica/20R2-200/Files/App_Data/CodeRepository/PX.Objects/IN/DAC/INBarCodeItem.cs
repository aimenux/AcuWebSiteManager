using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.CS;
using PX.SM;
using PX.Data;

namespace PX.Objects.IN
{
    [Serializable]
	public class INBarCodeItem : IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INBarCodeItem>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INBarCodeItem>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INBarCodeItem>.By<siteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INBarCodeItem>.By<locationID> { }
		}
        #endregion
        #region BarCode
		public abstract class barCode : PX.Data.BQL.BqlString.Field<barCode> { }
		protected String _BarCode;
		[PXDBString(255)]
		[PXUIField(DisplayName = "Bar Code")]
		public virtual String BarCode
		{
			get
			{
				return this._BarCode;
			}
			set
			{
				this._BarCode = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(Filterable = true)]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(typeof(INBarCodeItem.inventoryID))]
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current2<INBarCodeItem.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>))]
		[PXFormula(typeof(Default<INBarCodeItem.inventoryID>))]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<INBarCodeItem.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(INBarCodeItem.inventoryID))]
		[PXFormula(typeof(Default<INBarCodeItem.inventoryID>))]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.POSiteAvail(typeof(INBarCodeItem.inventoryID), typeof(INBarCodeItem.subItemID))]
		[PXFormula(typeof(Default<INBarCodeItem.inventoryID>))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[PXDefault]
		[Location(typeof(INBarCodeItem.siteID), Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Default<INBarCodeItem.siteID>))]
		[PXFormula(typeof(Default<INBarCodeItem.inventoryID>))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion		
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[PXDefault]
		[LotSerialNbr]
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
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[PXDefault(typeof(Search<INItemLotSerial.expireDate,
			Where<INItemLotSerial.inventoryID, Equal<Current<INBarCodeItem.inventoryID>>,
			And<INItemLotSerial.lotSerialNbr, Equal<Current<INBarCodeItem.lotSerialNbr>>>>>))]
		[PXDBDate(InputMask = "d", DisplayMask = "d")]
		[PXUIField(DisplayName = "Expiration Date")]
		[PXFormula(typeof(Default<INBarCodeItem.inventoryID>))]
		[PXFormula(typeof(Default<INBarCodeItem.lotSerialNbr>))]		
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
		#region ReasonCode
		public abstract class reasonCode : PX.Data.IBqlField
		{
		}
		protected String _ReasonCode;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		public virtual String ReasonCode
		{
			get
			{
				return this._ReasonCode;
			}
			set
			{
				this._ReasonCode = value;
			}
		}
		#endregion

		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;

		[PXDBQuantity(typeof(INBarCodeItem.uOM), typeof(INBarCodeItem.baseQty), HandleEmptyKey = true, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "1.0")]
		[PXUIField(DisplayName = "Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}		
		#endregion
		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		protected Decimal? _BaseQty;

		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion			
		#region ByOne
		public abstract class byOne : PX.Data.BQL.BqlBool.Field<byOne> { }
		protected Boolean? _ByOne;
		[PXDBBool()]
        [PXUIField(DisplayName = "Add One Unit Per Bar Code")]
		[PXDefault(typeof(INSetup.addByOneBarcode), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? ByOne
		{
			get
			{
				return this._ByOne;
			}
			set
			{
				this._ByOne = value;
			}
		}
		#endregion
		#region AutoAddLine
		public abstract class autoAddLine : PX.Data.BQL.BqlBool.Field<autoAddLine> { }
		protected Boolean? _AutoAddLine;
		[PXDBBool()]
		[PXDefault(typeof(INSetup.autoAddLineBarcode), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Auto Add Line")]
		public virtual Boolean? AutoAddLine
		{
			get
			{
				return this._AutoAddLine;
			}
			set
			{
				this._AutoAddLine = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(255)]
		[PXUIField(DisplayName = "")]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
	}
}
