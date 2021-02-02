using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.PM;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common.Bql;
using PX.Objects.Common;
using PX.Objects.Common.Attributes;

namespace PX.Objects.IN
{
    [System.SerializableAttribute()]
    [PXCacheName(Messages.INTran)]
	public partial class INTran : PX.Data.IBqlTable, ILSPrimary
	{
		#region Keys
		public class PK : PrimaryKeyOf<INTran>.By<docType, refNbr, lineNbr>
		{
			public static INTran Find(PXGraph graph, string docType, string refNbr, int? lineNbr) => FindBy(graph, docType, refNbr, lineNbr);
		}
		public static class FK
		{
			public class Register : INRegister.PK.ForeignKeyOf<INTran>.By<docType, refNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INTran>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INTran>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INTran>.By<siteID> { }
			public class ToSite : INSite.PK.ForeignKeyOf<INTran>.By<toSiteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INTran>.By<locationID> { }
			public class ToLocation : INLocation.PK.ForeignKeyOf<INTran>.By<toLocationID> { }
			public class BAccount : CR.BAccount.PK.ForeignKeyOf<INTran>.By<bAccountID> { }
			public class ARTran : AR.ARTran.PK.ForeignKeyOf<INTran>.By<aRDocType, aRRefNbr, aRLineNbr> { }
			public class OrigTran : INTran.PK.ForeignKeyOf<INTran>.By<origDocType, origRefNbr, origLineNbr> { }
			public class SOShipLine : SO.SOShipLine.PK.ForeignKeyOf<INTran>.By<sOShipmentNbr, sOShipmentLineNbr> { }
			public class SOLine : SO.SOLine.PK.ForeignKeyOf<INTran>.By<sOOrderType, sOOrderNbr, sOOrderLineNbr> { }
			public class POReceiptLine : PO.POReceiptLine.PK.ForeignKeyOf<INTran>.By<pOReceiptNbr, pOReceiptLineNbr> { }
			public class ReasonCode : CS.ReasonCode.PK.ForeignKeyOf<INTran>.By<reasonCode> { }
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        protected bool? _Selected;
        [PXBool]
        [PXFormula(typeof(False))]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(typeof(INRegister.branchID))]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXUIField(DisplayName = INRegister.docType.DisplayName)]
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(typeof(INRegister.docType))]
		[INDocType.List()]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
        #region OrigModule
        public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
        protected String _OrigModule;
        [PXString(2)]       
        public virtual String OrigModule
        {
            get
            {
                return this._OrigModule;
            }
            set
            {
                this._OrigModule = value;
            }
        }
        #endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXDBString(3, IsFixed = true)]
		[PXDefault()]
		[INTranType.List()]
		[PXUIField(DisplayName="Tran. Type")]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INRegister.refNbr))]
		[PXParent(typeof(FK.Register))]
		[PXUIField(DisplayName = INRegister.refNbr.DisplayName)]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(INRegister.lineCntr))]
		[PXUIField(DisplayName = "Line Number")]
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
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDBDefault(typeof(INRegister.tranDate))]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort()]
		[PXDefault()]
		[PXUIField(DisplayName = "Multiplier")]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
			public class InventoryBaseUnitRule :
				InventoryItem.baseUnit.PreventEditIfExists<
					Select<INTran,
					Where<inventoryID, Equal<Current<InventoryItem.inventoryID>>,
						And<released, NotEqual<True>>>>>
			{ }
		}
		protected Int32? _InventoryID;
		[PXDefault()]
		[StockItem(DisplayName="Inventory ID")]
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
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<INTran.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[IN.SubItem(
			typeof(INTran.inventoryID),
			typeof(LeftJoin<INSiteStatus,
				On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
				And<INSiteStatus.inventoryID, Equal<Optional<INTran.inventoryID>>,
				And<INSiteStatus.siteID, Equal<Optional<INTran.siteID>>>>>>))]
		[PXFormula(typeof(Default<INTran.inventoryID>))]
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
		[IN.SiteAvail(typeof(INTran.inventoryID), typeof(INTran.subItemID))]
		[PXDefault(typeof(INRegister.siteID))]
		[PXForeignReference(typeof(FK.Site))]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<INRegister.branchID>>>))]
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
		[IN.LocationAvail(typeof(INTran.inventoryID), typeof(INTran.subItemID), typeof(INTran.siteID), typeof(INTran.tranType), typeof(INTran.invtMult))]
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
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDBInt]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region DestBranchID
		public abstract class destBranchID : PX.Data.BQL.BqlInt.Field<destBranchID> { }

		[PXDBInt()]
		public virtual Int32? DestBranchID
		{
			get;
			set;
		}
		#endregion
		#region OrigBranchID
		[Obsolete("This field has been deprecated and will be removed in the later Acumatica versions. Please use INTran.destBranchID instead.")]
		public abstract class origBranchID : PX.Data.BQL.BqlInt.Field<origBranchID> { }
		protected Int32? _OrigBranchID;
		[PXDBInt()]
		[Obsolete("This field has been deprecated and will be removed in the later Acumatica versions. Please use INTran.DestBranchID instead.")]
		public virtual Int32? OrigBranchID
		{
			get
			{
				return this._OrigBranchID;
			}
			set
			{
				this._OrigBranchID = value;
			}
		}
		#endregion
		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		protected String _OrigDocType;
		[PXDBString(1, IsFixed = true)]
		public virtual String OrigDocType
		{
			get
			{
				return this._OrigDocType;
			}
			set
			{
				this._OrigDocType = value;
			}
		}
		#endregion
		#region OrigTranType
		public abstract class origTranType : PX.Data.BQL.BqlString.Field<origTranType> { }
		protected String _OrigTranType;
		[PXDBString(3, IsFixed = true)]
		public virtual String OrigTranType
		{
			get
			{
				return this._OrigTranType;
			}
			set
			{
				this._OrigTranType = value;
			}
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		protected String _OrigRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Receipt Nbr.")]
		[PXVerifySelector(typeof(Search2<INCostStatus.receiptNbr,
			InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>,
			InnerJoin<INLocation, On<INLocation.locationID, Equal<Optional<INTran.locationID>>>>>,
			Where<INCostStatus.inventoryID, Equal<Optional<INTran.inventoryID>>,
			And<INCostSubItemXRef.subItemID, Equal<Optional<INTran.subItemID>>,
			And<
			Where<INCostStatus.costSiteID, Equal<Optional<INTran.siteID>>, And<INLocation.isCosted, Equal<boolFalse>,
			Or<INCostStatus.costSiteID, Equal<Optional<INTran.locationID>>>>>>>>>), VerifyField = false)]
		public virtual String OrigRefNbr
		{
			get
			{
				return this._OrigRefNbr;
			}
			set
			{
				this._OrigRefNbr = value;
			}
		}
		#endregion
		#region OrigLineNbr
		public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
		protected Int32? _OrigLineNbr;
		[PXDBInt()]
		public virtual Int32? OrigLineNbr
		{
			get
			{
				return this._OrigLineNbr;
			}
			set
			{
				this._OrigLineNbr = value;
			}
		}
		#endregion
		#region OrigToLocationID
		/// <summary>
		/// Denormalization of <see cref="INTranSplit.ToLocationID"/>
		/// </summary>
		[PXDBInt]
		public virtual Int32? OrigToLocationID { get; set; }
		public abstract class origToLocationID : PX.Data.BQL.BqlInt.Field<origToLocationID> { }
		#endregion
		#region OrigIsLotSerial
		/// <summary>
		/// Denormalization of <see cref="INTranSplit.LotSerialNbr"/>
		/// </summary>
		[PXDBBool]
		public virtual Boolean? OrigIsLotSerial { get; set; }
		public abstract class origIsLotSerial : PX.Data.BQL.BqlBool.Field<origIsLotSerial> { }
		#endregion
		#region OrigNoteID
		/// <summary>
		/// Denormalization of <see cref="INRegister.NoteID"/>
		/// </summary>
		[PXDBGuid]
		public virtual Guid? OrigNoteID { get; set; }
		public abstract class origNoteID : PX.Data.BQL.BqlGuid.Field<origNoteID> { }
		#endregion
		#region AcctID
		public abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }
		protected Int32? _AcctID;
		[Account()]
		public virtual Int32? AcctID
		{
			get
			{
				return this._AcctID;
			}
			set
			{
				this._AcctID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(typeof(INTran.acctID))]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region ReclassificationProhibited
		public abstract class reclassificationProhibited : PX.Data.BQL.BqlBool.Field<reclassificationProhibited> { }
		protected Boolean? _ReclassificationProhibited;

		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? ReclassificationProhibited
		{
			get
			{
				return this._ReclassificationProhibited;
			}
			set
			{
				this._ReclassificationProhibited = value;
			}
		}
		#endregion
		#region InvtAcctID
		public abstract class invtAcctID : PX.Data.BQL.BqlInt.Field<invtAcctID> { }
		protected Int32? _InvtAcctID;
		[Account()]
		public virtual Int32? InvtAcctID
		{
			get
			{
				return this._InvtAcctID;
			}
			set
			{
				this._InvtAcctID = value;
			}
		}
		#endregion
		#region InvtSubID
		public abstract class invtSubID : PX.Data.BQL.BqlInt.Field<invtSubID> { }
		protected Int32? _InvtSubID;
		[SubAccount(typeof(INTran.invtAcctID))]
		public virtual Int32? InvtSubID
		{
			get
			{
				return this._InvtSubID;
			}
			set
			{
				this._InvtSubID = value;
			}
		}
		#endregion
		#region COGSAcctID
		public abstract class cOGSAcctID : PX.Data.BQL.BqlInt.Field<cOGSAcctID> { }
		protected Int32? _COGSAcctID;
		[Account()]
		public virtual Int32? COGSAcctID
		{
			get
			{
				return this._COGSAcctID;
			}
			set
			{
				this._COGSAcctID = value;
			}
		}
		#endregion
		#region COGSSubID
		public abstract class cOGSSubID : PX.Data.BQL.BqlInt.Field<cOGSSubID> { }
		protected Int32? _COGSSubID;
		[SubAccount(typeof(INTran.cOGSAcctID))]
		public virtual Int32? COGSSubID
		{
			get
			{
				return this._COGSSubID;
			}
			set
			{
				this._COGSSubID = value;
			}
		}
		#endregion
		#region ToSiteID
		public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
		protected Int32? _ToSiteID;
		[IN.ToSite()]
		[PXForeignReference(typeof(FK.ToSite))]
		public virtual Int32? ToSiteID
		{
			get
			{
				return this._ToSiteID;
			}
			set
			{
				this._ToSiteID = value;
			}
		}
		#endregion
		#region ToLocationID
		public abstract class toLocationID : PX.Data.BQL.BqlInt.Field<toLocationID> { }
		protected Int32? _ToLocationID;
		[IN.LocationAvail(typeof(INTran.inventoryID), typeof(INTran.subItemID), typeof(INTran.toSiteID), false, false, true, DisplayName = "To Location ID")]
		public virtual Int32? ToLocationID
		{
			get
			{
				return this._ToLocationID;
			}
			set
			{
				this._ToLocationID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[INLotSerialNbr(typeof(INTran.inventoryID), typeof(INTran.subItemID), typeof(INTran.locationID), PersistingCheck = PXPersistingCheck.Nothing, FieldClass = "LotSerial")]
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
		[INExpireDate(typeof(INTran.inventoryID), PersistingCheck=PXPersistingCheck.Nothing, FieldClass="LotSerial")]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault()]
		[INUnit(typeof(INTran.inventoryID))]
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
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity(typeof(INTran.uOM), typeof(INTran.baseQty), InventoryUnitType.BaseUnit)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName="Quantity")]
		[PXFormula(null, typeof(SumCalc<INRegister.totalQty>))]
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
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region ReleasedDateTime
		public abstract class releasedDateTime : PX.Data.BQL.BqlDateTime.Field<releasedDateTime> { }
		[PXUIField(DisplayName = "Release Date", Visible = true)]
		[DBConditionalModifiedDateTime(typeof(released), true)]
		public virtual DateTime? ReleasedDateTime
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FinPeriodID(
			branchSourceType: typeof(INTran.branchID),
			masterFinPeriodIDType: typeof(INTran.tranPeriodID),
			headerMasterFinPeriodIDType: typeof(INRegister.tranPeriodID))]
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
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		protected String _TranPeriodID;
		[PeriodID]
		public virtual String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }
		protected Decimal? _UnitPrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<INItemSite.basePrice, Where<INItemSite.inventoryID, Equal<Current<INTran.inventoryID>>, And<INItemSite.siteID, Equal<Current<INTran.siteID>>>>>))]
		[PXUIField(DisplayName = "Unit Price")]
		public virtual Decimal? UnitPrice
		{
			get
			{
				return this._UnitPrice;
			}
			set
			{
				this._UnitPrice = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		protected Decimal? _TranAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Price")]
		[PXFormula(typeof(Mult<INTran.qty, INTran.unitPrice>))]
		public virtual Decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region ExactCost
		public abstract class exactCost : PX.Data.BQL.BqlBool.Field<exactCost> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? ExactCost
		{
			get;
			set;
		}
		#endregion

		#region ManualCost
		public abstract class manualCost : PX.Data.BQL.BqlBool.Field<manualCost> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Cost", Visible = false, Enabled = false)]
		public virtual bool? ManualCost
		{
			get;
			set;
		}
		#endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBPriceCost(MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<
            Search<INItemSite.tranUnitCost, Where<INItemSite.inventoryID, Equal<Current<INTran.inventoryID>>, And<INItemSite.siteID, Equal<Current<INTran.siteID>>>>>,
            Search<INItemCost.tranUnitCost, Where<INItemCost.inventoryID, Equal<Current<INTran.inventoryID>>>>>))]
        [PXUIField(DisplayName="Unit Cost")]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region AvgCost
		public abstract class avgCost : PX.Data.BQL.BqlDecimal.Field<avgCost> { }
		protected Decimal? _AvgCost;
		[PXPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<INItemSite.avgCost, Where<INItemSite.inventoryID, Equal<Current<INTran.inventoryID>>, And<INItemSite.siteID, Equal<Current<INTran.siteID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? AvgCost
		{
			get
			{
				return this._AvgCost;
			}
			set
			{
				this._AvgCost = value;
			}
		}
		#endregion
		#region TranCost
		public abstract class tranCost : PX.Data.BQL.BqlDecimal.Field<tranCost> { }
		protected Decimal? _TranCost;
		[PXDBBaseCury(MinValue = 0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName="Ext. Cost")]
		[PXFormula(typeof(Mult<INTran.qty, INTran.unitCost>), typeof(SumCalc<INRegister.totalCost>))]
		public virtual Decimal? TranCost
		{
			get
			{
				return this._TranCost;
			}
			set
			{
				this._TranCost = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName="Description")]
		[PXDefault(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INTran.inventoryID>>>>),
            SourceField = typeof(InventoryItem.descr), CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region AccrueCost
		public abstract class accrueCost : PX.Data.BQL.BqlBool.Field<accrueCost> { }
		protected Boolean? _AccrueCost;
		/// <summary>
		/// When set to <c>true</c>, indicates that cost will be processed using expense accrual account.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? AccrueCost
		{
			get
			{
				return this._AccrueCost;
			}
			set
			{
				this._AccrueCost = value;
			}
		}
		#endregion
		#region ReasonCode
		public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		protected String _ReasonCode;
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID>))]
		[PXRestrictor(typeof(Where<ReasonCode.usage, Equal<Optional<INTran.docType>>>), Messages.ReasonCodeDoesNotMatch)]
		[PXUIField(DisplayName="Reason Code")]
		[PXForeignReference(typeof(FK.ReasonCode))]
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
		#region OrigQty
		public abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
		protected Decimal? _OrigQty;
		[PXDBCalced(typeof(Minus<INTran.qty>), typeof(decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigQty
		{
			get
			{
				return this._OrigQty;
			}
			set
			{
				this._OrigQty = value;
			}
		}
		#endregion
		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		protected Decimal? _BaseQty;
		[PXDBQuantity()]
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
        #region MaxTransferBaseQty
        public abstract class maxTransferBaseQty : PX.Data.BQL.BqlDecimal.Field<maxTransferBaseQty> { }
        protected Decimal? _MaxTransferBaseQty;
        [PXDBQuantity()]
        public virtual Decimal? MaxTransferBaseQty
        {
            get
            {
                return this._MaxTransferBaseQty;
            }
            set
            {
                this._MaxTransferBaseQty = value;
            }
        }
        #endregion
        #region UnassignedQty
        public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }
		protected Decimal? _UnassignedQty;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnassignedQty
		{
			get
			{
				return this._UnassignedQty;
			}
			set
			{
				this._UnassignedQty = value;
			}
		}
		#endregion
		#region CostedQty
		public abstract class costedQty : PX.Data.BQL.BqlDecimal.Field<costedQty> { }
		protected Decimal? _CostedQty;
		[PXDecimal(6)]
		[PXFormula(typeof(decimal0))]
		public virtual Decimal? CostedQty
		{
			get
			{
				return this._CostedQty;
			}
			set
			{
				this._CostedQty = value;
			}
		}
		#endregion
		#region OrigTranCost
		public abstract class origTranCost : PX.Data.BQL.BqlDecimal.Field<origTranCost> { }
		protected Decimal? _OrigTranCost;
		[PXBaseCury()]
		public virtual Decimal? OrigTranCost
		{
			get
			{
				return this._OrigTranCost;
			}
			set
			{
				this._OrigTranCost = value;
			}
		}
		#endregion
		#region OrigTranAmt
		public abstract class origTranAmt : PX.Data.BQL.BqlDecimal.Field<origTranAmt> { }
		protected Decimal? _OrigTranAmt;
		[PXBaseCury()]
		public virtual Decimal? OrigTranAmt
		{
			get
			{
				return this._OrigTranAmt;
			}
			set
			{
				this._OrigTranAmt = value;
			}
		}
		#endregion
      
		#region ARDocType
		public abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }
		protected String _ARDocType;
		[PXDBString(3)]
		public virtual String ARDocType
		{
			get
			{
				return this._ARDocType;
			}
			set
			{
				this._ARDocType = value;
			}
		}
		#endregion
		#region ARRefNbr
		public abstract class aRRefNbr : PX.Data.BQL.BqlString.Field<aRRefNbr> { }
		protected String _ARRefNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String ARRefNbr
		{
			get
			{
				return this._ARRefNbr;
			}
			set
			{
				this._ARRefNbr = value;
			}
		}
		#endregion
		#region ARLineNbr
		public abstract class aRLineNbr : PX.Data.BQL.BqlInt.Field<aRLineNbr> { }
		protected Int32? _ARLineNbr;
		[PXDBInt()]
		public virtual Int32? ARLineNbr
		{
			get
			{
				return this._ARLineNbr;
			}
			set
			{
				this._ARLineNbr = value;
			}
		}
		#endregion
		#region SOOrderType
		public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
		protected String _SOOrderType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName="SO Order Type",  Visible = false)]
		[PXSelector(typeof(Search<SOOrderType.orderType>))]
		public virtual String SOOrderType
		{
			get
			{
				return this._SOOrderType;
			}
			set
			{
				this._SOOrderType = value;
			}
		}
		#endregion
		#region SOOrderNbr
		public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
		protected String _SOOrderNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName="SO Order Nbr.",  Visible = false, Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<INTran.sOOrderType>>>>))]
		public virtual String SOOrderNbr
		{
			get
			{
				return this._SOOrderNbr;
			}
			set
			{
				this._SOOrderNbr = value;
			}
		}
		#endregion
		#region SOOrderLineNbr
		public abstract class sOOrderLineNbr : PX.Data.BQL.BqlInt.Field<sOOrderLineNbr> { }
		protected Int32? _SOOrderLineNbr;
		[PXDBInt()]
		public virtual Int32? SOOrderLineNbr
		{
			get
			{
				return this._SOOrderLineNbr;
			}
			set
			{
				this._SOOrderLineNbr = value;
			}
		}
		#endregion
		#region SOShipmentType
		public abstract class sOShipmentType : PX.Data.BQL.BqlString.Field<sOShipmentType> { }
		protected String _SOShipmentType;
		[PXDBString(1, IsFixed = true)]
		public virtual String SOShipmentType
		{
			get
			{
				return this._SOShipmentType;
			}
			set
			{
				this._SOShipmentType = value;
			}
		}
		#endregion
		#region SOShipmentNbr
		public abstract class sOShipmentNbr : PX.Data.BQL.BqlString.Field<sOShipmentNbr> { }
		protected String _SOShipmentNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName="SO Shipment Nbr.",  Visible = false, Enabled = false)]
		[PXSelector(typeof(Search<Objects.SO.Navigate.SOOrderShipment.shipmentNbr,
			Where<Objects.SO.Navigate.SOOrderShipment.orderType, Equal<Current<INTran.sOOrderType>>,
			And<Objects.SO.Navigate.SOOrderShipment.orderNbr, Equal<Current<INTran.sOOrderNbr>>,
			And<Objects.SO.Navigate.SOOrderShipment.shipmentType, Equal<Current<INTran.sOShipmentType>>>>>>))]
		public virtual String SOShipmentNbr
		{
			get
			{
				return this._SOShipmentNbr;
			}
			set
			{
				this._SOShipmentNbr = value;
			}
		}
		#endregion
		#region SOShipmentLineNbr
		public abstract class sOShipmentLineNbr : PX.Data.BQL.BqlInt.Field<sOShipmentLineNbr> { }
		protected Int32? _SOShipmentLineNbr;
		[PXDBInt()]
		public virtual Int32? SOShipmentLineNbr
		{
			get
			{
				return this._SOShipmentLineNbr;
			}
			set
			{
				this._SOShipmentLineNbr = value;
			}
		}
		#endregion
		#region SOLineType
		public abstract class sOLineType : PX.Data.BQL.BqlString.Field<sOLineType> { }
		protected string _SOLineType;
		[PXDBString(2, IsFixed = true)]
		public virtual string SOLineType
		{
			get
			{
				return this._SOLineType;
			}
			set
			{
				this._SOLineType = value;
			}
		}
		#endregion
        #region UpdateShippedNotInvoiced
        public abstract class updateShippedNotInvoiced : PX.Data.BQL.BqlBool.Field<updateShippedNotInvoiced> { }
        protected Boolean? _UpdateShippedNotInvoiced;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? UpdateShippedNotInvoiced
        {
            get
            {
                return this._UpdateShippedNotInvoiced;
            }
            set
            {
                this._UpdateShippedNotInvoiced = value;
            }
        }
        #endregion
        #region POReceiptType
        public abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }
        protected String _POReceiptType;
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "PO Receipt Type", Visible = false, Enabled = false)]
        public virtual String POReceiptType
        {
            get
            {
                return this._POReceiptType;
            }
            set
            {
                this._POReceiptType = value;
            }
        }
        #endregion
		#region POReceiptNbr
		public abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }
		protected String _POReceiptNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "PO Receipt Nbr.",  Visible = false, Enabled = false)]
		[PXSelector(typeof(Search<POReceipt.receiptNbr>))]
		public virtual String POReceiptNbr
		{
			get
			{
				return this._POReceiptNbr;
			}
			set
			{
				this._POReceiptNbr = value;
			}
		}
		#endregion
		#region POReceiptLineNbr
		public abstract class pOReceiptLineNbr : PX.Data.BQL.BqlInt.Field<pOReceiptLineNbr> { }
		protected Int32? _POReceiptLineNbr;
		[PXDBInt()]
		public virtual Int32? POReceiptLineNbr
		{
			get
			{
				return this._POReceiptLineNbr;
			}
			set
			{
				this._POReceiptLineNbr = value;
			}
		}
		#endregion
		#region POLineType
		public abstract class pOLineType : PX.Data.BQL.BqlString.Field<pOLineType> { }
		protected String _POLineType;
		[PXDBString(2, IsFixed = true)]
		public virtual String POLineType
		{
			get
			{
				return this._POLineType;
			}
			set
			{
				this._POLineType = value;
			}
		}
		#endregion
		#region AssyType
		public abstract class assyType : PX.Data.BQL.BqlString.Field<assyType> { }
		protected String _AssyType;
		[PXDBString(1, IsFixed = true)]
		public virtual String AssyType
		{
			get
			{
				return this._AssyType;
			}
			set
			{
				this._AssyType = value;
			}
		}
		#endregion
        #region OrigPlanType
        public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
        [PXDBString(2, IsFixed = true)]
        [PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
        public virtual String OrigPlanType
        {
            get;
            set;
        }
        #endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
        [ProjectDefault(BatchModule.IN)]
		[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInIN, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBaseAttribute]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveProjectTask(typeof(INTran.projectID), BatchModule.IN, DisplayName = "Project Task")]
		[PXForeignReference(typeof(Field<taskID>.IsRelatedTo<PMTask.taskID>))]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
        #endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[CostCode(null, typeof(taskID), null, ReleasedField = typeof(released))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion

        #region ReceiptedBaseQty
        public abstract class receiptedBaseQty : PX.Data.BQL.BqlDecimal.Field<receiptedBaseQty> { }
        protected Decimal? _ReceiptedBaseQty;
        [PXQuantity]
        [PXUIField(DisplayName = "Received Base Qty.", Visible = false, Enabled = true, IsReadOnly = true)]
        public virtual Decimal? ReceiptedBaseQty
        {
            get
            {
                return this._ReceiptedBaseQty;
            }
            set
            {
                this._ReceiptedBaseQty = value;
            }
        }
        #endregion
        #region INTransitBaseQty
        public abstract class iNTransitBaseQty : PX.Data.BQL.BqlDecimal.Field<iNTransitBaseQty> { }
        protected Decimal? _INTransitBaseQty;
        [PXQuantity]
        [PXUIField(DisplayName = "In-Transit Base Qty.", Visible = false, Enabled = true, IsReadOnly = true)]
        public virtual Decimal? INTransitBaseQty
        {
            get
            {
                return this._INTransitBaseQty;
            }
            set
            {
                this._INTransitBaseQty = value;
            }
        }
        #endregion
        #region ReceiptedQty
        public abstract class receiptedQty : PX.Data.BQL.BqlDecimal.Field<receiptedQty> { }
        protected Decimal? _ReceiptedQty;
        [PXQuantity(typeof(INTran.uOM), typeof(INTran.receiptedBaseQty))]
        [PXUIField(DisplayName = "Received Qty.", Visible = false, Enabled = true, IsReadOnly = true)]
        public virtual Decimal? ReceiptedQty
        {
            get
            {
                return this._ReceiptedQty;
            }
            set
            {
                this._ReceiptedQty = value;
            }
        }
        #endregion
        #region INTransitQty
        public abstract class iNTransitQty : PX.Data.BQL.BqlDecimal.Field<iNTransitQty> { }
        protected Decimal? _INTransitQty;
        [PXQuantity(typeof(INTran.uOM), typeof(INTran.iNTransitBaseQty))]
        [PXUIField(DisplayName = "In-Transit Qty.", Visible = false, Enabled = true, IsReadOnly = true)]
        public virtual Decimal? INTransitQty
        {
            get
            {
                return this._INTransitQty;
            }
            set
            {
                this._INTransitQty = value;
            }
        }
        #endregion

        #region IsCostUnmanaged
        public abstract class isCostUnmanaged : PX.Data.BQL.BqlBool.Field<isCostUnmanaged> { }
        protected Boolean? _IsCostUnmanaged;
        [PXDBBool()]
        public virtual Boolean? IsCostUnmanaged
        {
            get
            {
                return this._IsCostUnmanaged;
            }
            set
            {
                this._IsCostUnmanaged = value;
            }
        }
        #endregion
        #region HasMixedProjectTasks
        public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }
		protected bool? _HasMixedProjectTasks;
		/// <summary>
		/// Returns true if the splits associated with the line has mixed ProjectTask values.
		/// This field is used to validate the record on persist. 
		/// </summary>
		[PXBool]
		[PXFormula(typeof(False))]
		public virtual bool? HasMixedProjectTasks
		{
			get
			{
				return _HasMixedProjectTasks;
			}
			set
			{
				_HasMixedProjectTasks = value;
			}
		}
        #endregion
		#region IsComponentItem
		public abstract class isComponentItem : IBqlField { }
		[PXDBBool()]
		[PXDefault(false)]
		public bool? IsComponentItem
		{
			get;
			set;
		}
		#endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region PIID
		public abstract class pIID : Data.BQL.BqlString.Field<pIID>
		{
		}

		[PXDBString(15, IsUnicode = true)]
		public virtual String PIID
		{
			get;
			set;
		}
		#endregion
		#region PILineNbr
		public abstract class pILineNbr : Data.BQL.BqlInt.Field<pILineNbr>
		{
		}

		[PXDBInt]
		[PXUIField(DisplayName = "PI Line Nbr.", Enabled = false, Visible = false)]
		public virtual int? PILineNbr
		{
			get;
			set;
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
		#region Unbound Properties
		#region SalesMult
		public abstract class salesMult : PX.Data.BQL.BqlShort.Field<salesMult> { }
		[PXShort()]
		public virtual Int16? SalesMult
		{
			[PXDependsOnFields(typeof(tranType))]
			get
			{
				return INTranType.SalesMult(this._TranType);
			}
			set
			{
			}
		}
		#endregion
		#endregion
		#region Methods
		public static implicit operator INTranSplit(INTran item)
		{
			INTranSplit ret = new INTranSplit();
			ret.DocType = item.DocType;
			ret.TranType = item.TranType;
			ret.RefNbr = item.RefNbr;
			ret.LineNbr = item.LineNbr;
			ret.SplitLineNbr = 1;
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.SubItemID = item.SubItemID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;
			ret.ExpireDate = item.ExpireDate;
			if (item.Qty < 0m)
			{
				ret.Qty = -item.Qty;
				ret.BaseQty = -item.BaseQty;
				ret.InvtMult = (short?)-item.InvtMult;
			}
			else
			{
				ret.Qty = item.Qty;
				ret.BaseQty = item.BaseQty;
				ret.InvtMult = item.InvtMult;
			}
			ret.UOM = item.UOM;
			ret.TranDate = item.TranDate;
			ret.Released = item.Released;
            ret.POLineType = item.POLineType;
			ret.SOLineType = item.SOLineType;
            ret.ToSiteID = item.ToSiteID;
            ret.ToLocationID = item.ToLocationID;
			ret.OrigModule = item.OrigModule;

			return ret;
		}
		public static implicit operator INTran(INTranSplit item)
		{
			INTran ret = new INTran();
			ret.DocType = item.DocType;
			ret.TranType = item.TranType;
			ret.RefNbr = item.RefNbr;
			ret.LineNbr = item.LineNbr;
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.SubItemID = item.SubItemID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;
			ret.Qty = item.Qty;
			ret.UOM = item.UOM;
			ret.TranDate = item.TranDate;
			ret.BaseQty = item.BaseQty;
			ret.InvtMult = item.InvtMult;
			ret.Released = item.Released;
            ret.POLineType = item.POLineType;
			ret.SOLineType = item.SOLineType;
            ret.ToSiteID = item.ToSiteID;
            ret.ToLocationID = item.ToLocationID;
			ret.OrigModule = item.OrigModule;

			return ret;
		}
		#endregion
	}

    /// <summary>
    /// Added for join purpose
    /// </summary>
    [System.SerializableAttribute()]
    [PXHidden]
    public partial class INTran2 : INTran
    {
        #region Released
        public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
        #endregion
        #region DocType
        public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        #endregion
        #region OrigModule
        public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
        #endregion
        #region RefNbr
        public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        #endregion
        #region LineNbr
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion
        #region OrigTranType
        public new abstract class origTranType : PX.Data.BQL.BqlString.Field<origTranType> { }
        #endregion
        #region OrigRefNbr
        public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

        #endregion
        #region OrigLineNbr
        public new abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
        #endregion
		#region POReceiptType
		public new abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }
		#endregion
		#region POReceiptNbr
		public new abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }
		#endregion
		#region POReceiptLineNbr
		public new abstract class pOReceiptLineNbr : PX.Data.BQL.BqlInt.Field<pOReceiptLineNbr> { }
		#endregion
    }

    public class INTranType
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues => _AllowedValues;
			public string[] AllowedLabels => _AllowedLabels;

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels) : base(AllowedValues, AllowedLabels) {}
			public CustomListAttribute(Tuple<string,string>[] valuesToLabels) : base(valuesToLabels) {}
		}

		public class ListAttribute : CustomListAttribute
		{
		    public ListAttribute() : base(
			    new[]
				{
					Pair(Receipt, Messages.Receipt),
					Pair(Issue, Messages.Issue),
					Pair(Return, Messages.Return),
					Pair(Adjustment, Messages.Adjustment),
					Pair(Transfer, Messages.Transfer),
					Pair(Invoice, Messages.Invoice),
					Pair(DebitMemo, Messages.DebitMemo),
					Pair(CreditMemo, Messages.CreditMemo),
					Pair(Assembly, Messages.Assembly),
					Pair(Disassembly, Messages.Disassembly),
					Pair(StandardCostAdjustment, Messages.StandardCostAdjustment),
					Pair(NegativeCostAdjustment, Messages.NegativeCostAdjustment),
					Pair(ReceiptCostAdjustment, Messages.ReceiptCostAdjustment),
				}) {}
		}

		public class IssueListAttribute : CustomListAttribute
		{
		    public IssueListAttribute() : base(
			    new[]
				{
					Pair(Issue, Messages.Issue),
					Pair(Return, Messages.Return),
					Pair(Invoice, Messages.Invoice),
					Pair(DebitMemo, Messages.DebitMemo),
					Pair(CreditMemo, Messages.CreditMemo),
				}) {}
		}

		public class SOListAttribute : CustomListAttribute
		{
		    public SOListAttribute() : base(
			    new[]
				{
					Pair(Issue, Messages.Issue),
					Pair(Return, Messages.Return),
					Pair(Transfer, Messages.Transfer),
					Pair(Invoice, Messages.Invoice),
					Pair(DebitMemo, Messages.DebitMemo),
					Pair(CreditMemo, Messages.CreditMemo),
					Pair(NoUpdate, Messages.NoUpdate),
				}) {}
		}

		public class SONonARListAttribute : CustomListAttribute
		{
		    public SONonARListAttribute() : base(
			    new[]
				{
					Pair(Issue, Messages.Issue),
					Pair(Return, Messages.Return),
					Pair(Transfer, Messages.Transfer),
					Pair(NoUpdate, Messages.NoUpdate),
				}) {}
		}

		public const string Assembly = "ASY";
		public const string Disassembly = "DSY";
		public const string Receipt = "RCP";
		public const string Issue = "III";
		public const string Return = "RET";
		public const string Adjustment = "ADJ";
		public const string Transfer = "TRX";
		public const string Invoice = "INV";
		public const string DebitMemo = "DRM";
		public const string CreditMemo = "CRM";
		public const string StandardCostAdjustment = "ASC";
		public const string NegativeCostAdjustment = "NSC";
		public const string ReceiptCostAdjustment = "RCA";

		public const string NoUpdate = "UND";

		public class adjustment : PX.Data.BQL.BqlString.Constant<adjustment>
		{
			public adjustment() : base(Adjustment) { ;}
		}

		public class receipt : PX.Data.BQL.BqlString.Constant<receipt>
		{
			public receipt() : base(Receipt) { ;}
		}

		public class issue : PX.Data.BQL.BqlString.Constant<issue>
		{
			public issue() : base(Issue) { ;}
		}

		public class transfer : PX.Data.BQL.BqlString.Constant<transfer>
		{
			public transfer() : base(Transfer) { ;}
		}

		public class return_ : PX.Data.BQL.BqlString.Constant<return_>
		{
			public return_() : base(Return) { ;}
		}

		public class invoice : PX.Data.BQL.BqlString.Constant<invoice>
		{
			public invoice() : base(Invoice) { ;}
		}

		public class creditMemo : PX.Data.BQL.BqlString.Constant<creditMemo>
		{
			public creditMemo() : base(CreditMemo) { ;}
		}

		public class debitMemo : PX.Data.BQL.BqlString.Constant<debitMemo>
		{
			public debitMemo() : base(DebitMemo) { ;}
		}

		public class assembly : PX.Data.BQL.BqlString.Constant<assembly>
		{
			public assembly() : base(Assembly) { ;}
		}

		public class disassembly : PX.Data.BQL.BqlString.Constant<disassembly>
		{
			public disassembly() : base(Disassembly) { ;}
		}

		public class standardCostAdjustment : PX.Data.BQL.BqlString.Constant<standardCostAdjustment>
		{
			public standardCostAdjustment() : base(StandardCostAdjustment) { ;}
		}

		public class negativeCostAdjustment : PX.Data.BQL.BqlString.Constant<negativeCostAdjustment>
		{
			public negativeCostAdjustment() : base(NegativeCostAdjustment) { ;}
		}

		public class receiptCostAdjustment : PX.Data.BQL.BqlString.Constant<receiptCostAdjustment>
		{
			public receiptCostAdjustment() : base(ReceiptCostAdjustment) { ;}
		}

		public class noUpdate : PX.Data.BQL.BqlString.Constant<noUpdate>
		{
			public noUpdate() : base(NoUpdate) { ; }
		}

		public static string DocType(string TranType)
		{
			switch (TranType)
			{
				case Issue:
				case Invoice:
				case DebitMemo:
				case CreditMemo:
				case Return:
					return INDocType.Issue;
				case Transfer:
					return INDocType.Transfer;
				case Receipt:
					return INDocType.Receipt;
				default:
					return INDocType.Undefined;
			}
		}

		public static Int16? InvtMult(string TranType)
		{
			switch (TranType)
			{
				case Issue:
				case Invoice:
				case DebitMemo:
				case Transfer:
				case Assembly:
				case Disassembly:
					return -1;
				case Receipt:
				case Return:
				case CreditMemo:
				case Adjustment:
				case StandardCostAdjustment:
				case NegativeCostAdjustment:
					return 1;
				case ReceiptCostAdjustment:
				case NoUpdate:
					return 0;
				default:
					return null;
			}
		}

		public static Int16? SalesMult(string TranType)
		{
			switch (TranType)
			{
				case Invoice:
				case DebitMemo:
					return 1;
				case CreditMemo:
					return -1;
				default:
					return null;
			}
		}

		public static string TranTypeFromInvoiceType(string docType, decimal? qtySign)
		{
			bool negQty = (qtySign < 0m);
			switch (docType)
			{
				case AR.ARDocType.Invoice:
				case AR.ARDocType.CashSale:
					return !negQty ? Invoice : CreditMemo;
				case AR.ARDocType.DebitMemo:
					return !negQty ? DebitMemo : CreditMemo;
				case AR.ARDocType.CreditMemo:
				case AR.ARDocType.CashReturn:
					return !negQty ? CreditMemo : Invoice;
				default:
					return null;
			}
		}

		public static short? InvtMultFromInvoiceType(string docType)
		{
			switch (docType)
			{
				case AR.ARDocType.Invoice:
				case AR.ARDocType.DebitMemo:
				case AR.ARDocType.CashSale:
					return -1;
				case AR.ARDocType.CreditMemo:
				case AR.ARDocType.CashReturn:
					return 1;
				default:
					return null;
			}
		}
	}

	public class INAssyType
	{

		public const string KitTran = "K";
		public const string CompTran = "C";
		public const string OverheadTran = "O";


		public class kitTran : PX.Data.BQL.BqlString.Constant<kitTran>
		{
			public kitTran() : base(KitTran) { ;}
		}

		public class compTran : PX.Data.BQL.BqlString.Constant<compTran>
		{
			public compTran() : base(CompTran) { ;}
		}

		public class overheadTran : PX.Data.BQL.BqlString.Constant<overheadTran>
		{
			public overheadTran() : base(OverheadTran) { ;}
		}
	}

    


}
