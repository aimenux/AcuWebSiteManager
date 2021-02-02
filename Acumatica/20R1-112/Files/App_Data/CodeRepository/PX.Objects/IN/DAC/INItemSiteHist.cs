using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INItemSiteHist)]
	public partial class INItemSiteHist : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemSiteHist>.By<inventoryID, siteID, subItemID, locationID, finPeriodID>
		{
			public static INItemSiteHist Find(PXGraph graph, int? inventoryID, int? siteID, int? subItemID, int? locationID, string finPeriodID)
				=> FindBy(graph, inventoryID, siteID, subItemID, locationID, finPeriodID);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemSiteHist>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<INItemSiteHist>.By<siteID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INItemSiteHist>.By<subItemID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INItemSiteHist>.By<locationID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(IsKey = true)]
		[PXDefault()]
		[PXForeignReference(typeof(FK.InventoryItem))]
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
		[SubItem(IsKey = true)]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(IsKey = true)]
		[PXDefault()]
		[PXForeignReference(typeof(FK.Site))]
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
		[IN.Location(IsKey = true)]
		[PXDefault()]
		[PXForeignReference(typeof(FK.Location))]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;
		[PXString(6, IsFixed = true)]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
		#region FinPtdQtyIssued
		public abstract class finPtdQtyIssued : PX.Data.BQL.BqlDecimal.Field<finPtdQtyIssued> { }
		protected Decimal? _FinPtdQtyIssued;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Issued")]
		public virtual Decimal? FinPtdQtyIssued
		{
			get
			{
				return this._FinPtdQtyIssued;
			}
			set
			{
				this._FinPtdQtyIssued = value;
			}
		}
		#endregion
		#region FinPtdQtyReceived
		public abstract class finPtdQtyReceived : PX.Data.BQL.BqlDecimal.Field<finPtdQtyReceived> { }
		protected Decimal? _FinPtdQtyReceived;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Received")]
		public virtual Decimal? FinPtdQtyReceived
		{
			get
			{
				return this._FinPtdQtyReceived;
			}
			set
			{
				this._FinPtdQtyReceived = value;
			}
		}
		#endregion
		#region FinBegQty
		public abstract class finBegQty : PX.Data.BQL.BqlDecimal.Field<finBegQty> { }
		protected Decimal? _FinBegQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Beginning Qty.")]
		public virtual Decimal? FinBegQty
		{
			get
			{
				return this._FinBegQty;
			}
			set
			{
				this._FinBegQty = value;
			}
		}
		#endregion
		#region FinYtdQty
		public abstract class finYtdQty : PX.Data.BQL.BqlDecimal.Field<finYtdQty> { }
		protected Decimal? _FinYtdQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Ending Qty.")]
		public virtual Decimal? FinYtdQty
		{
			get
			{
				return this._FinYtdQty;
			}
			set
			{
				this._FinYtdQty = value;
			}
		}
		#endregion
		#region FinPtdQtySales
		public abstract class finPtdQtySales : PX.Data.BQL.BqlDecimal.Field<finPtdQtySales> { }
		protected Decimal? _FinPtdQtySales;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Sales")]
		public virtual Decimal? FinPtdQtySales
		{
			get
			{
				return this._FinPtdQtySales;
			}
			set
			{
				this._FinPtdQtySales = value;
			}
		}
		#endregion
		#region FinPtdQtyCreditMemos
		public abstract class finPtdQtyCreditMemos : PX.Data.BQL.BqlDecimal.Field<finPtdQtyCreditMemos> { }
		protected Decimal? _FinPtdQtyCreditMemos;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Memos")]
		public virtual Decimal? FinPtdQtyCreditMemos
		{
			get
			{
				return this._FinPtdQtyCreditMemos;
			}
			set
			{
				this._FinPtdQtyCreditMemos = value;
			}
		}
		#endregion
		#region FinPtdQtyDropShipSales
		public abstract class finPtdQtyDropShipSales : PX.Data.BQL.BqlDecimal.Field<finPtdQtyDropShipSales> { }
		protected Decimal? _FinPtdQtyDropShipSales;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Drop Ship Sales")]
		public virtual Decimal? FinPtdQtyDropShipSales
		{
			get
			{
				return this._FinPtdQtyDropShipSales;
			}
			set
			{
				this._FinPtdQtyDropShipSales = value;
			}
		}
		#endregion
		#region FinPtdQtyTransferIn
		public abstract class finPtdQtyTransferIn : PX.Data.BQL.BqlDecimal.Field<finPtdQtyTransferIn> { }
		protected Decimal? _FinPtdQtyTransferIn;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Transfer In")]
		public virtual Decimal? FinPtdQtyTransferIn
		{
			get
			{
				return this._FinPtdQtyTransferIn;
			}
			set
			{
				this._FinPtdQtyTransferIn = value;
			}
		}
		#endregion
		#region FinPtdQtyTransferOut
		public abstract class finPtdQtyTransferOut : PX.Data.BQL.BqlDecimal.Field<finPtdQtyTransferOut> { }
		protected Decimal? _FinPtdQtyTransferOut;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Transfer Out")]
		public virtual Decimal? FinPtdQtyTransferOut
		{
			get
			{
				return this._FinPtdQtyTransferOut;
			}
			set
			{
				this._FinPtdQtyTransferOut = value;
			}
		}
		#endregion
		#region FinPtdQtyAssemblyIn
		public abstract class finPtdQtyAssemblyIn : PX.Data.BQL.BqlDecimal.Field<finPtdQtyAssemblyIn> { }
		protected Decimal? _FinPtdQtyAssemblyIn;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Assembly In")]
		public virtual Decimal? FinPtdQtyAssemblyIn
		{
			get
			{
				return this._FinPtdQtyAssemblyIn;
			}
			set
			{
				this._FinPtdQtyAssemblyIn = value;
			}
		}
		#endregion
		#region FinPtdQtyAssemblyOut
		public abstract class finPtdQtyAssemblyOut : PX.Data.BQL.BqlDecimal.Field<finPtdQtyAssemblyOut> { }
		protected Decimal? _FinPtdQtyAssemblyOut;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Assembly Out")]
		public virtual Decimal? FinPtdQtyAssemblyOut
		{
			get
			{
				return this._FinPtdQtyAssemblyOut;
			}
			set
			{
				this._FinPtdQtyAssemblyOut = value;
			}
		}
		#endregion
		#region FinPtdQtyAdjusted
		public abstract class finPtdQtyAdjusted : PX.Data.BQL.BqlDecimal.Field<finPtdQtyAdjusted> { }
		protected Decimal? _FinPtdQtyAdjusted;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Adjusted")]
		public virtual Decimal? FinPtdQtyAdjusted
		{
			get
			{
				return this._FinPtdQtyAdjusted;
			}
			set
			{
				this._FinPtdQtyAdjusted = value;
			}
		}
		#endregion
		#region TranPtdQtyReceived
		public abstract class tranPtdQtyReceived : PX.Data.BQL.BqlDecimal.Field<tranPtdQtyReceived> { }
		protected Decimal? _TranPtdQtyReceived;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Received")]
		public virtual Decimal? TranPtdQtyReceived
		{
			get
			{
				return this._TranPtdQtyReceived;
			}
			set
			{
				this._TranPtdQtyReceived = value;
			}
		}
		#endregion
		#region TranPtdQtyIssued
		public abstract class tranPtdQtyIssued : PX.Data.BQL.BqlDecimal.Field<tranPtdQtyIssued> { }
		protected Decimal? _TranPtdQtyIssued;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Issued")]
		public virtual Decimal? TranPtdQtyIssued
		{
			get
			{
				return this._TranPtdQtyIssued;
			}
			set
			{
				this._TranPtdQtyIssued = value;
			}
		}
		#endregion
		#region TranPtdQtySales
		public abstract class tranPtdQtySales : PX.Data.BQL.BqlDecimal.Field<tranPtdQtySales> { }
		protected Decimal? _TranPtdQtySales;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Sales")]
		public virtual Decimal? TranPtdQtySales
		{
			get
			{
				return this._TranPtdQtySales;
			}
			set
			{
				this._TranPtdQtySales = value;
			}
		}
		#endregion
		#region TranPtdQtyCreditMemos
		public abstract class tranPtdQtyCreditMemos : PX.Data.BQL.BqlDecimal.Field<tranPtdQtyCreditMemos> { }
		protected Decimal? _TranPtdQtyCreditMemos;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Memos")]
		public virtual Decimal? TranPtdQtyCreditMemos
		{
			get
			{
				return this._TranPtdQtyCreditMemos;
			}
			set
			{
				this._TranPtdQtyCreditMemos = value;
			}
		}
		#endregion
		#region TranPtdQtyDropShipSales
		public abstract class tranPtdQtyDropShipSales : PX.Data.BQL.BqlDecimal.Field<tranPtdQtyDropShipSales> { }
		protected Decimal? _TranPtdQtyDropShipSales;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Drop Ship Sales")]
		public virtual Decimal? TranPtdQtyDropShipSales
		{
			get
			{
				return this._TranPtdQtyDropShipSales;
			}
			set
			{
				this._TranPtdQtyDropShipSales = value;
			}
		}
		#endregion
		#region TranPtdQtyTransferIn
		public abstract class tranPtdQtyTransferIn : PX.Data.BQL.BqlDecimal.Field<tranPtdQtyTransferIn> { }
		protected Decimal? _TranPtdQtyTransferIn;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Transfer In")]
		public virtual Decimal? TranPtdQtyTransferIn
		{
			get
			{
				return this._TranPtdQtyTransferIn;
			}
			set
			{
				this._TranPtdQtyTransferIn = value;
			}
		}
		#endregion
		#region TranPtdQtyTransferOut
		public abstract class tranPtdQtyTransferOut : PX.Data.BQL.BqlDecimal.Field<tranPtdQtyTransferOut> { }
		protected Decimal? _TranPtdQtyTransferOut;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Transfer Out")]
		public virtual Decimal? TranPtdQtyTransferOut
		{
			get
			{
				return this._TranPtdQtyTransferOut;
			}
			set
			{
				this._TranPtdQtyTransferOut = value;
			}
		}
		#endregion
		#region TranPtdQtyAssemblyIn
		public abstract class tranPtdQtyAssemblyIn : PX.Data.BQL.BqlDecimal.Field<tranPtdQtyAssemblyIn> { }
		protected Decimal? _TranPtdQtyAssemblyIn;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Assembly In")]
		public virtual Decimal? TranPtdQtyAssemblyIn
		{
			get
			{
				return this._TranPtdQtyAssemblyIn;
			}
			set
			{
				this._TranPtdQtyAssemblyIn = value;
			}
		}
		#endregion
		#region TranPtdQtyAssemblyOut
		public abstract class tranPtdQtyAssemblyOut : PX.Data.BQL.BqlDecimal.Field<tranPtdQtyAssemblyOut> { }
		protected Decimal? _TranPtdQtyAssemblyOut;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Assembly Out")]
		public virtual Decimal? TranPtdQtyAssemblyOut
		{
			get
			{
				return this._TranPtdQtyAssemblyOut;
			}
			set
			{
				this._TranPtdQtyAssemblyOut = value;
			}
		}
		#endregion
		#region TranPtdQtyAdjusted
		public abstract class tranPtdQtyAdjusted : PX.Data.BQL.BqlDecimal.Field<tranPtdQtyAdjusted> { }
		protected Decimal? _TranPtdQtyAdjusted;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Adjusted")]
		public virtual Decimal? TranPtdQtyAdjusted
		{
			get
			{
				return this._TranPtdQtyAdjusted;
			}
			set
			{
				this._TranPtdQtyAdjusted = value;
			}
		}
		#endregion
		#region TranBegQty
		public abstract class tranBegQty : PX.Data.BQL.BqlDecimal.Field<tranBegQty> { }
		protected Decimal? _TranBegQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Qty.")]
		public virtual Decimal? TranBegQty
		{
			get
			{
				return this._TranBegQty;
			}
			set
			{
				this._TranBegQty = value;
			}
		}
		#endregion
		#region TranYtdQty
		public abstract class tranYtdQty : PX.Data.BQL.BqlDecimal.Field<tranYtdQty> { }
		protected Decimal? _TranYtdQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Qty.")]
		public virtual Decimal? TranYtdQty
		{
			get
			{
				return this._TranYtdQty;
			}
			set
			{
				this._TranYtdQty = value;
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

	[System.SerializableAttribute()]
	[PXCacheName(Messages.INItemSiteHistByPeriod)]
	[PXProjection(typeof(Select5<INItemSiteHist,
		InnerJoin<MasterFinPeriod, On<MasterFinPeriod.finPeriodID, GreaterEqual<INItemSiteHist.finPeriodID>>>,
			Aggregate<GroupBy<INItemSiteHist.inventoryID,
				GroupBy<INItemSiteHist.subItemID,
				GroupBy<INItemSiteHist.siteID,
				GroupBy<INItemSiteHist.locationID,
				Max<INItemSiteHist.finPeriodID,
				GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>))]
	public partial class INItemSiteHistByPeriod : PX.Data.IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(IsKey = true, BqlField = typeof(INItemSiteHist.inventoryID))]
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
		[SubItem(IsKey = true, BqlField = typeof(INItemSiteHist.subItemID))]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(IsKey = true, BqlField = typeof(INItemSiteHist.siteID))]
		[PXDefault()]
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
		[IN.Location(IsKey = true, BqlField = typeof(INItemSiteHist.locationID))]
		[PXDefault()]
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
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;
		[FinPeriodID(BqlField = typeof(INItemSiteHist.finPeriodID))]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
		[PXUIField(DisplayName = "Fin. Period")]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
	}
}
