using System;
using System.Collections.Generic;

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.PM;
using PX.Objects.Common;
using PX.Objects.Common.Bql;

namespace PX.Objects.PO
{
	

	[System.SerializableAttribute()]
	[PXCacheName(Messages.POReceiptLine)]
	public partial class POReceiptLine : PX.Data.IBqlTable, ILSPrimary, ISortOrder, IPOReturnLineSource
	{
		#region Keys
		public class PK : PrimaryKeyOf<POReceiptLine>.By<receiptNbr, lineNbr>
		{
			public static POReceiptLine Find(PXGraph graph, string receiptNbr, int? lineNbr) => FindBy(graph, receiptNbr, lineNbr);
		}
		public static class FK
		{
			public class Receipt : POReceipt.PK.ForeignKeyOf<POReceiptLine>.By<receiptNbr> { }
			public class OrigReceipt : POReceipt.PK.ForeignKeyOf<POReceiptLine>.By<origReceiptNbr> { }
			public class OrigReceiptLine : POReceiptLine.PK.ForeignKeyOf<POReceiptLine>.By<origReceiptNbr, origReceiptLineNbr> { }
			public class AccrualStatus : POAccrualStatus.PK.ForeignKeyOf<POReceiptLine>.By<pOAccrualRefNoteID, pOAccrualLineNbr, pOAccrualType> { }
			public class OrigPlanType : IN.INPlanType.PK.ForeignKeyOf<POReceiptLine>.By<origPlanType> { }
			public class INSiteStatus : IN.INSiteStatus.PK.ForeignKeyOf<POReceiptLine>.By<inventoryID, subItemID, siteID> { }
			public class INLocationStatus : IN.INLocationStatus.PK.ForeignKeyOf<POReceiptLine>.By<inventoryID, subItemID, siteID, locationID> { }
			public class INLotSerialStatus : IN.INLotSerialStatus.PK.ForeignKeyOf<POReceiptLine>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr> { }
			public class OrigTran : INTran.PK.ForeignKeyOf<POReceiptLine>.By<origDocType, origRefNbr, origLineNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<POReceiptLine>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<POReceiptLine>.By<siteID> { }
			public class ReasonCode : CS.ReasonCode.PK.ForeignKeyOf<POReceiptLine>.By<reasonCode> { }

			public class Order : POOrder.PK.ForeignKeyOf<POReceiptLine>.By<pOType, pONbr> { }
			public class OrderLine : POLine.PK.ForeignKeyOf<POReceiptLine>.By<pOType, pONbr, pOLineNbr> { }
			public class OrderLineR : POLineR.PK.ForeignKeyOf<POReceiptLine>.By<pOType, pONbr, pOLineNbr> { }

			public class SOOrder : SO.SOOrder.PK.ForeignKeyOf<POReceiptLine>.By<sOOrderType, sOOrderNbr> { }
			public class SOOrderLine : SO.SOLine.PK.ForeignKeyOf<POReceiptLine>.By<sOOrderType, sOOrderNbr, sOOrderLineNbr> { }
			public class SOShipment : SO.SOShipment.PK.ForeignKeyOf<POReceiptLine>.By<sOShipmentNbr> { }

		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(typeof(POReceipt.branchID))]
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
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType>
		{
			public const int Length = 2;
		}
		protected String _ReceiptType;
		[PXUIField(DisplayName = "Type")]
		[PXDBString(receiptType.Length, IsFixed = true)]
		[PXDBDefault(typeof(POReceipt.receiptType))]
		public virtual String ReceiptType
		{
			get
			{
				return this._ReceiptType;
			}
			set
			{
				this._ReceiptType = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(POReceipt.receiptNbr))]
		[PXParent(typeof(Select<POReceipt, Where<POReceipt.receiptType, Equal<Current<POReceiptLine.receiptType>>, And<POReceipt.receiptNbr, Equal<Current<POReceiptLine.receiptNbr>>>>>))]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr>
		{
			public class PreventEditAccrualAcctIfPOReceiptLineExists : PreventEditOf<POLine.pOAccrualAcctID, POLine.pOAccrualSubID>.On<POOrderEntry>
				.IfExists<Select<POReceiptLine,
					Where<POReceiptLine.pOType, Equal<Current<POLine.orderType>>,
						And<POReceiptLine.pONbr, Equal<Current<POLine.orderNbr>>,
						And<POReceiptLine.pOLineNbr, Equal<Current<POLine.lineNbr>>>>>>>
			{
				protected override string CreateEditPreventingReason(GetEditPreventingReasonArgs arg, object l, string fld, string tbl, string foreignTbl)
				{
					var line = (POReceiptLine)l;
					var docTypeDict = new POReceiptType.ListAttribute().ValueLabelDic;
					return PXMessages.LocalizeFormat(Messages.AccrualAcctUsedInPOReceipt, docTypeDict[line.ReceiptType], line.ReceiptNbr);
				}
			}
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(POReceipt.lineCntr))]
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
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;
		[PXDBInt]
		[PXUIField(DisplayName = AP.APTran.sortOrder.DispalyName, Visible = false, Enabled = false)]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort()]
		[PXDefault()]
		[PXUIField(DisplayName = "Inventory Multiplier")]
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
					Select<POReceiptLine,
					Where<inventoryID, Equal<Current<InventoryItem.inventoryID>>,
						And2<Where2<POLineType.Goods.Contains<lineType>, Or<POLineType.NonStocks.Contains<lineType>>>,
						And<released, NotEqual<True>>>>>>
			{ }
		}
		protected Int32? _InventoryID;
		[POReceiptLineInventory(typeof(receiptType), Filterable = true)]
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
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType>
		{
			public const int Length = 2;
		}
		protected String _LineType;
		[PXDBString(lineType.Length, IsFixed = true)]
		[PXDefault(POLineType.Service)]
		[POReceiptLineTypeList(typeof(POReceiptLine.inventoryID))]
		[PXUIField(DisplayName = "Line Type")]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
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
		[PXDefault(typeof(IsNull<Selector<POReceiptLine.inventoryID, InventoryItem.accrueCost>, False>))]
		[PXUIField(DisplayName = "Accrue Cost", Enabled = false, Visible = false)]
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
		#region TranType

		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

		public string TranType
		{
			[PXDependsOnFields(typeof(receiptType))]
			get
			{
				return POReceiptType.GetINTranType(this._ReceiptType);
			}
		}
		#endregion
		#region TranDate
		public virtual DateTime? TranDate
		{
			get { return this._ReceiptDate; }
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor(
			typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
			CacheGlobal = true,
			Filterable = true)]
		[VerndorNonEmployeeOrOrganizationRestrictor]
		[PXDBDefault(typeof(POReceipt.vendorID))]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region ReceiptDate
		public abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
		protected DateTime? _ReceiptDate;

		[PXDBDate()]
		[PXDBDefault(typeof(POReceipt.receiptDate))]
		public virtual DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(typeof(POReceiptLine.inventoryID))]
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current2<POReceiptLine.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<POReceiptLine.inventoryID>))]
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

		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<POReceiptLine.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing, CacheGlobal = true)]
        [INUnit(typeof(POReceiptLine.inventoryID))]
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
		#region POType
		public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
		protected String _POType;
		[PXDBString(2, IsFixed = true)]
		[POOrderType.List()]
		[PXUIField(DisplayName = "PO Order Type")]
		public virtual String POType
		{
			get
			{
				return this._POType;
			}
			set
			{
				this._POType = value;
			}
		}
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr>
		{
			public const int Length = 15;
		}
		protected String _PONbr;
		[PXDBString(pONbr.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "PO Order Nbr.")]
		[PO.RefNbr(typeof(Search2<POOrder.orderNbr,
			LeftJoinSingleTable<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>,
			And<Match<Vendor, Current<AccessInfo.userName>>>>>,
			Where<POOrder.orderType, Equal<Optional<POReceiptLine.pOType>>,
			And<Vendor.bAccountID, IsNotNull>>,
			OrderBy<Desc<POOrder.orderNbr>>>), Filterable = true)]
		public virtual String PONbr
		{
			get
			{
				return this._PONbr;
			}
			set
			{
				this._PONbr = value;
			}
		}
		#endregion
		#region POLineNbr
		public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }
		protected Int32? _POLineNbr;
		[PXDBInt()]
		[PXParent(typeof(FK.OrderLineR))]
		[PXUIField(DisplayName = "PO Line Nbr.")]
		public virtual Int32? POLineNbr
		{
			get
			{
				return this._POLineNbr;
			}
			set
			{
				this._POLineNbr = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.POSiteAvail(typeof(POReceiptLine.inventoryID), typeof(POReceiptLine.subItemID))]
		[PXDefault(typeof(Coalesce<Search<CR.Location.vSiteID,
			Where<CR.Location.locationID, Equal<Current2<POReceipt.vendorLocationID>>,
				And<CR.Location.bAccountID, Equal<Current2<POReceipt.vendorID>>>>>,
					Search<InventoryItem.dfltSiteID, Where<InventoryItem.inventoryID, Equal<Current2<POReceiptLine.inventoryID>>>>>))]
		[PXForeignReference(typeof(FK.Site))]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<POReceipt.branchID>>>))]
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
		[POLocationAvail(typeof(POReceiptLine.inventoryID), typeof(POReceiptLine.subItemID), typeof(POReceiptLine.siteID), typeof(POReceiptLine.tranType), typeof(POReceiptLine.invtMult), KeepEntry = false)]
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
		[POLotSerialNbrAttribute(typeof(POReceiptLine.inventoryID), typeof(POReceiptLine.subItemID), typeof(POReceiptLine.locationID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region AllowComplete
		public abstract class allowComplete : PX.Data.BQL.BqlBool.Field<allowComplete> { }
		protected Boolean? _AllowComplete;
		[PXBool()]
		[PXUIField(DisplayName = "Complete PO Line", Visibility = PXUIVisibility.Service, Visible = true)]		
		public virtual Boolean? AllowComplete
		{
			get
			{
				return this._AllowComplete;				
			}
			set
			{
				this._AllowComplete = value;
				
			}
		}
		#endregion
		#region AllowOpen
		public abstract class allowOpen : PX.Data.BQL.BqlBool.Field<allowOpen> { }
		protected Boolean? _AllowOpen;
		[PXBool()]
		[PXUIField(DisplayName = "Open PO Line", Visibility = PXUIVisibility.Service, Visible = true)]
		public virtual Boolean? AllowOpen
		{
			get
			{
				return this._AllowOpen;
			}
			set
			{
				this._AllowOpen = value;

			}
		}
		#endregion
				
		#region ReceiptQty
		public abstract class receiptQty : PX.Data.BQL.BqlDecimal.Field<receiptQty> { }
		protected Decimal? _ReceiptQty;

		[PXDBQuantity(typeof(POReceiptLine.uOM), typeof(POReceiptLine.baseReceiptQty), InventoryUnitType.PurchaseUnit, HandleEmptyKey = true, MinValue = 0, ConvertToDecimalVerifyUnits = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<POReceipt.orderQty>))]
		[PXUIField(DisplayName = "Receipt Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReceiptQty
		{
			get
			{
				return this._ReceiptQty;
			}
			set
			{
				this._ReceiptQty = value;
			}
		}

		public virtual Decimal? Qty
		{
			get
			{
				return this._ReceiptQty;
			}
			set
			{
				this._ReceiptQty = value;
			}
		}
		#endregion
		#region BaseReceiptQty
		public abstract class baseReceiptQty : PX.Data.BQL.BqlDecimal.Field<baseReceiptQty> { }
		protected Decimal? _BaseReceiptQty;

		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Receipt Qty.", Visible = false, Enabled = false)]
		public virtual Decimal? BaseReceiptQty
		{
			get
			{
				return this._BaseReceiptQty;
			}
			set
			{
				this._BaseReceiptQty = value;
			}
		}
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseReceiptQty;
			}
			set
			{
				this._BaseReceiptQty = value;
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
		#region BaseMultReceiptQty
		public abstract class baseMultReceiptQty : PX.Data.BQL.BqlDecimal.Field<baseMultReceiptQty> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Mult<POReceiptLine.baseReceiptQty, POReceiptLine.invtMult>))]
		[PXFormula(null, typeof(SumCalc<POLineR.baseReceivedQty>))]
		public virtual Decimal? BaseMultReceiptQty
		{
			get;
			set;
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
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(POReceipt.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;

		[PXUIField(DisplayName = "Unit Cost", Visible = false)]
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		//[PXDefault(typeof(Search<InventoryItem.lastCost, Where<InventoryItem.inventoryID, Equal<Current<POReceiptLine.inventoryID>>>>))]
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
        #region ManualPrice
        public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }
        protected Boolean? _ManualPrice;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Cost", Visible = false)]
        public virtual Boolean? ManualPrice
        {
            get
            {
                return this._ManualPrice;
            }
            set
            {
                this._ManualPrice = value;
            }
        }
        #endregion
        #region GroupDiscountRate
        public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
        protected Decimal? _GroupDiscountRate;
        [PXDBDecimal(18)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual Decimal? GroupDiscountRate
        {
            get
            {
                return this._GroupDiscountRate;
            }
            set
            {
                this._GroupDiscountRate = value;
            }
        }
        #endregion
        #region DocumentDiscountRate
        public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
        protected Decimal? _DocumentDiscountRate;
        [PXDBDecimal(18)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual Decimal? DocumentDiscountRate
        {
            get
            {
                return this._DocumentDiscountRate;
            }
            set
            {
                this._DocumentDiscountRate = value;
            }
        }
        #endregion

		#region TranUnitCost
		public abstract class tranUnitCost : PX.Data.BQL.BqlDecimal.Field<tranUnitCost> { }
		[PXDBDecimal(6)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? TranUnitCost
			{
			get;
			set;
		}
		#endregion
		#region TranCost
		public abstract class tranCost : PX.Data.BQL.BqlDecimal.Field<tranCost> { }
		[PXDBBaseCury]
		[PXDefault]
		[PXFormula(typeof(Mult<POReceiptLine.receiptQty,
			Switch<Case<Where<POReceiptLine.manualPrice, Equal<True>, Or<POReceiptLine.tranUnitCost, IsNull>>, POReceiptLine.unitCost>, POReceiptLine.tranUnitCost>>))]
		[PXUIField(DisplayName = "Estimated IN Ext. Cost", Enabled = false, Visible = false)]
		public virtual decimal? TranCost
		{
			get;
			set;
		}
		#endregion
		#region TranCostFinal
		public abstract class tranCostFinal : PX.Data.BQL.BqlDecimal.Field<tranCostFinal> { }
		[PXDBBaseCury]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Final IN Ext. Cost", Enabled = false, Visible = false)]
		public virtual decimal? TranCostFinal
			{
			get;
			set;
		}
		#endregion		

		#region ReasonCode
		public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		protected String _ReasonCode;
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, In3<ReasonCodeUsages.issue, ReasonCodeUsages.vendorReturn>>>),
			DescriptionField = typeof(ReasonCode.descr))]
		[PXUIField(DisplayName = "Reason Code", Visible = false)]
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
		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		protected Int32? _ExpenseAcctID;
		[Account(typeof(POReceiptLine.branchID),
			DisplayName = "Account",
			Visibility = PXUIVisibility.Visible,
			Filterable = false,
			DescriptionField = typeof(Account.description),
			Visible = false,
			AvoidControlAccounts = true,
			SuppressCurrencyValidation = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ExpenseAcctID
		{
			get
			{
				return this._ExpenseAcctID;
			}
			set
			{
				this._ExpenseAcctID = value;
			}
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		protected Int32? _ExpenseSubID;
		[SubAccount(typeof(POReceiptLine.expenseAcctID), typeof(POReceiptLine.branchID), DisplayName = "Sub.", Visibility = PXUIVisibility.Visible, Filterable = true, Visible = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ExpenseSubID
		{
			get
			{
				return this._ExpenseSubID;
			}
			set
			{
				this._ExpenseSubID = value;
			}
		}
		#endregion
		#region POAccrualAcctID
		public abstract class pOAccrualAcctID : PX.Data.BQL.BqlInt.Field<pOAccrualAcctID> { }
		protected Int32? _POAccrualAcctID;
		[Account(typeof(POReceiptLine.branchID), DisplayName = "Accrual Account", Filterable = false, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.PO)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? POAccrualAcctID
		{
			get
			{
				return this._POAccrualAcctID;
			}
			set
			{
				this._POAccrualAcctID = value;
			}
		}
		#endregion
		#region POAccrualSubID
		public abstract class pOAccrualSubID : PX.Data.BQL.BqlInt.Field<pOAccrualSubID> { }
		protected Int32? _POAccrualSubID;
		[SubAccount(typeof(POReceiptLine.pOAccrualAcctID), typeof(POReceiptLine.branchID), DisplayName = "Accrual Sub.", Filterable = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? POAccrualSubID
		{
			get
			{
				return this._POAccrualSubID;
			}
			set
			{
				this._POAccrualSubID = value;
			}
		}
		#endregion
		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PXDBString(50, IsUnicode = true, InputMask = "")]
		public virtual String AlternateID
		{
			get
			{
				return this._AlternateID;
			}
			set
			{
				this._AlternateID = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Transaction Descr.", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search<InventoryItem.descr, Where<InventoryItem.inventoryID, Equal<Current<POReceiptLine.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing, CacheGlobal = true)]
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
		#region UnitWeight
		public abstract class unitWeight : PX.Data.BQL.BqlDecimal.Field<unitWeight> { }
		protected Decimal? _UnitWeight;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.baseWeight, Where<InventoryItem.inventoryID, Equal<Current<POReceiptLine.inventoryID>>, And<InventoryItem.baseWeight, IsNotNull>>>))]
		[PXUIField(DisplayName = "Unit Weight")]
		public virtual Decimal? UnitWeight
		{
			get
			{
				return this._UnitWeight;
			}
			set
			{
				this._UnitWeight = value;
			}
		}
		#endregion
		#region UnitVolume
		public abstract class unitVolume : PX.Data.BQL.BqlDecimal.Field<unitVolume> { }
		protected Decimal? _UnitVolume;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.baseVolume, Where<InventoryItem.inventoryID, Equal<Current<POReceiptLine.inventoryID>>, And<InventoryItem.baseVolume, IsNotNull>>>))]
		public virtual Decimal? UnitVolume
		{
			get
			{
				return this._UnitVolume;
			}
			set
			{
				this._UnitVolume = value;
			}
		}
		#endregion
		#region ExtWeight
		public abstract class extWeight : PX.Data.BQL.BqlDecimal.Field<extWeight> { }
		protected Decimal? _ExtWeight;

		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Weight", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Mult<Row<POReceiptLine.baseReceiptQty,POReceiptLine.receiptQty>, POReceiptLine.unitWeight>), typeof(SumCalc<POReceipt.receiptWeight>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ExtWeight
		{
			get
			{
				return this._ExtWeight;
			}
			set
			{
				this._ExtWeight = value;
			}
		}
		#endregion
		#region ExtVolume
		public abstract class extVolume : PX.Data.BQL.BqlDecimal.Field<extVolume> { }
		protected Decimal? _ExtVolume;

		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Volume", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Mult<Row<POReceiptLine.baseReceiptQty, POReceiptLine.receiptQty>, POReceiptLine.unitVolume>), typeof(SumCalc<POReceipt.receiptVolume>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ExtVolume
		{
			get
			{
				return this._ExtVolume;
			}
			set
			{
				this._ExtVolume = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[POProjectDefault(typeof(POReceiptLine.lineType))]
		[PXRestrictor(typeof(Where<PM.PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PM.PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInPO, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBaseAttribute(Visible = false)]
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
		[ActiveProjectTask(typeof(POReceiptLine.projectID), BatchModule.PO, DisplayName = "Project Task", Visible = false)]
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
		[CostCode(ReleasedField = typeof(released))]
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
		
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
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
	
		#region OrigBranchID
		[Obsolete("This field has been deprecated and will be removed in the later Acumatica versions.")]

		public abstract class origBranchID : PX.Data.BQL.BqlInt.Field<origBranchID> { }
		protected Int32? _OrigBranchID;
		[PXDBInt()]
		[Obsolete("This field has been deprecated and will be removed in the later Acumatica versions.")]
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
		/// Denormalization of <see cref="INTransitLine.ToLocationID"/>
		/// </summary>
		[PXDBInt]
		public virtual Int32? OrigToLocationID { get; set; }
		public abstract class origToLocationID : PX.Data.BQL.BqlInt.Field<origToLocationID> { }
		#endregion
		#region OrigIsLotSerial
		/// <summary>
		/// Denormalization of <see cref="INTransitLine.IsLotSerial"/>
		/// </summary>
		[PXDBBool]
		public virtual Boolean? OrigIsLotSerial { get; set; }
		public abstract class origIsLotSerial : PX.Data.BQL.BqlBool.Field<origIsLotSerial> { }
		#endregion
		#region OrigNoteID
		/// <summary>
		/// Denormalization of <see cref="INTransitLine.NoteID"/>
		/// </summary>
		[PXDBGuid]
		public virtual Guid? OrigNoteID { get; set; }
		public abstract class origNoteID : PX.Data.BQL.BqlGuid.Field<origNoteID> { }
		#endregion
		#region OrigIsFixedInTransit
		/// <summary>
		/// Denormalization of <see cref="INTransitLine.IsFixedInTransit"/>
		/// </summary>
		[PXDBBool]
		public virtual Boolean? OrigIsFixedInTransit { get; set; }
		public abstract class origIsFixedInTransit : PX.Data.BQL.BqlBool.Field<origIsFixedInTransit> { }
		#endregion

        #region SOOrderType
        public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
        protected String _SOOrderType;
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Transfer Order Type", Enabled = false)]
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
        [PXUIField(DisplayName = "Transfer Order Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<SO.SOOrder.orderNbr, Where<SO.SOOrder.orderType, Equal<Current<POReceiptLine.sOOrderType>>>>))]
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
        [PXUIField(DisplayName = "Transfer Line Nbr.", Enabled = false)]
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
		[PXUIField(DisplayName = "Transfer Shipment Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SO.SOShipment.shipmentNbr, Where<SO.SOShipment.shipmentType, Equal<Current<POReceiptLine.sOShipmentType>>>>))]
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

        #region OrigPlanType
        public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
        protected String _OrigPlanType;
        [PXDBString(2, IsFixed = true)]
        [PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
        public virtual String OrigPlanType
        {
            get
            {
                return this._OrigPlanType;
            }
            set
            {
                this._OrigPlanType = value;
            }
        }
        #endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXUIField(DisplayName = "Released")]
		[PXDefault(false)]
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
		#region INReleased
		/// <summary>
		/// Sets up when the <see cref="INTran.Released"/> sets up (on <see cref="INDocumentRelease.ReleaseDoc(System.Collections.Generic.List{INRegister},bool)"/>)
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? INReleased { get; set; }
		public abstract class iNReleased : PX.Data.BQL.BqlBool.Field<iNReleased> { }
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
		
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[POExpireDateAttribute(typeof(POReceiptLine.inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
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

		#region OrigOrderQty
		public abstract class origOrderQty : PX.Data.BQL.BqlDecimal.Field<origOrderQty> { }
		protected Decimal? _OrigOrderQty;
		[PXQuantity()]
		[PXUIField(DisplayName = "Ordered Qty.")]
		[PXDependsOnFields(typeof(pOType), typeof(pONbr), typeof(pOLineNbr), typeof(inventoryID), typeof(uOM),
			typeof(origRefNbr), typeof(origLineNbr), typeof(origDocType))]
		public virtual Decimal? OrigOrderQty
		{
			get
			{
				return this._OrigOrderQty;
			}
			set
			{
				this._OrigOrderQty = value;
			}
		}
		#endregion
		#region OpenOrderQty
		public abstract class openOrderQty : PX.Data.BQL.BqlDecimal.Field<openOrderQty> { }
		protected Decimal? _OpenOrderQty;
		[PXQuantity()]
		[PXUIField(DisplayName = "Open Qty.")]
		[PXDependsOnFields(typeof(pOType), typeof(pONbr), typeof(pOLineNbr), typeof(inventoryID), typeof(uOM),
			typeof(origRefNbr), typeof(origLineNbr), typeof(origDocType))]
		public virtual Decimal? OpenOrderQty
		{
			get
			{
				return this._OpenOrderQty;
			}
			set
			{
				this._OpenOrderQty = value;
			}
		}
		#endregion

		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty> { }
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(typeof(POReceiptLine.uOM), typeof(POReceiptLine.baseUnbilledQty), HandleEmptyKey = true)]
		[PXFormula(typeof(POReceiptLine.receiptQty), typeof(SumCalc<POReceipt.unbilledQty>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion
		#region BaseUnbilledQty
		public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty> { }
		protected Decimal? _BaseUnbilledQty;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseUnbilledQty
		{
			get
			{
				return this._BaseUnbilledQty;
			}
			set
			{
				this._BaseUnbilledQty = value;
			}
		}
		#endregion
		#region BillPPVAmt
		public abstract class billPPVAmt : PX.Data.BQL.BqlDecimal.Field<billPPVAmt> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BillPPVAmt
		{
			get;
			set;
		}
		#endregion

		#region POAccrualType
		public abstract class pOAccrualType : PX.Data.BQL.BqlString.Field<pOAccrualType> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(Objects.PO.POAccrualType.Receipt)]
		[POAccrualType.List]
		[PXUIField(DisplayName = "Billing Based On", Enabled = false)]
		public virtual string POAccrualType
		{
			get;
			set;
		}
		#endregion
		#region POAccrualRefNoteID
		public abstract class pOAccrualRefNoteID : PX.Data.BQL.BqlGuid.Field<pOAccrualRefNoteID> { }
		[PXDBGuid]
		[PXDefault(typeof(POReceipt.noteID))]
		public virtual Guid? POAccrualRefNoteID
		{
			get;
			set;
		}
		#endregion
		#region POAccrualLineNbr
		public abstract class pOAccrualLineNbr : PX.Data.BQL.BqlInt.Field<pOAccrualLineNbr> { }
		[PXDBInt]
		[PXDefault(typeof(Current<POReceiptLine.lineNbr>))]
		public virtual int? POAccrualLineNbr
		{
			get;
			set;
		}
		#endregion

		#region OrigReceiptNbr
		public abstract class origReceiptNbr : PX.Data.BQL.BqlString.Field<origReceiptNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "PO Receipt Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.receiptNbr, Equal<Current<POReceiptLine.origReceiptNbr>>>>), ValidateValue = false)]
		public virtual string OrigReceiptNbr
		{
			get;
			set;
		}
		#endregion
		#region OrigReceiptLineNbr
		public abstract class origReceiptLineNbr : PX.Data.BQL.BqlInt.Field<origReceiptLineNbr> { }
		[PXDBInt]
		[PXUIField(DisplayName = "PO Receipt Line Nbr.", Enabled = false, Visible = false)]
		public virtual int? OrigReceiptLineNbr
		{
			get;
			set;
		}
		#endregion
		#region OrigReceiptLineType
		public abstract class origReceiptLineType : PX.Data.BQL.BqlString.Field<origReceiptLineType> { }
		[PXDBString(lineType.Length, IsFixed = true)]
		public virtual String OrigReceiptLineType
        {
			get;
			set;
        }
        #endregion
		#region BaseOrigQty
		public abstract class baseOrigQty : PX.Data.BQL.BqlDecimal.Field<baseOrigQty> { }
		[PXDBQuantity]
		public virtual decimal? BaseOrigQty
        {
			get;
			set;
        }
		#endregion
		#region BaseReturnedQty
		public abstract class baseReturnedQty : PX.Data.BQL.BqlDecimal.Field<baseReturnedQty> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BaseReturnedQty
		{
			get;
			set;
		}
		#endregion
		#region ReturnedQty
		public abstract class returnedQty : PX.Data.BQL.BqlDecimal.Field<returnedQty> { }
		[PXQuantity]
		[PXUIField(DisplayName = "Returned Qty.", Enabled = false)]
		public virtual decimal? ReturnedQty
		{
			get;
			set;
		}
		#endregion

		#region IsSkItem
		public abstract class isStkItem : PX.Data.BQL.BqlBool.Field<isStkItem> { }
		[PXBool]
		[PXUIField(DisplayName = "Is stock", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
		[PXFormula(typeof(Selector<POReceiptLine.inventoryID, InventoryItem.stkItem>))]
		public virtual Boolean? IsStkItem
		{
			get;
			set;
		}
		#endregion
		#region IsKit
		public abstract class isKit : PX.Data.BQL.BqlBool.Field<isKit> { }
		[PXBool]
		[PXUIField(DisplayName = "Is a Kit", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
		[PXFormula(typeof(Selector<POReceiptLine.inventoryID, InventoryItem.kitItem>))]
		public virtual Boolean? IsKit
		{
			get;
			set;
		}
		#endregion

        #region LastBaseReceivedQty

        protected Decimal? _LastBaseReceivedQty;

		public virtual Decimal? LastBaseReceivedQty
		{
			get
			{
				return this._LastBaseReceivedQty;
			}
			set
			{
				this._LastBaseReceivedQty = value;
			}
		}
		#endregion

		#region IsLSEntryBlocked
		public abstract class isLSEntryBlocked : PX.Data.BQL.BqlBool.Field<isLSEntryBlocked> { }
		protected Boolean? _IsLSEntryBlocked;
		[PXBool()]
		public virtual Boolean? IsLSEntryBlocked
		{
			get
			{
				return this._IsLSEntryBlocked;
			}
			set
			{
				this._IsLSEntryBlocked = value;

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

		#region Methods
		public virtual bool IsStockItem() 
		{
			return this.LineType == POLineType.GoodsForInventory || 
				this.LineType == POLineType.GoodsForSalesOrder ||
                this.LineType == POLineType.GoodsForServiceOrder ||
			       this.LineType == POLineType.GoodsForReplenishment ||
			       this.LineType == POLineType.GoodsForManufacturing;
		}
		public virtual bool IsNonStockItem()
		{
			return this.LineType == POLineType.NonStock ||
			       this.LineType == POLineType.NonStockForSalesOrder ||
                   this.LineType == POLineType.NonStockForServiceOrder ||
			       this.LineType == POLineType.NonStockForManufacturing;
		} 
		public static implicit operator POReceiptLineSplit(POReceiptLine item)
		{
			POReceiptLineSplit ret = new POReceiptLineSplit();
			ret.ReceiptType = item.ReceiptType;
			ret.ReceiptNbr = item.ReceiptNbr;
			ret.LineType = item.LineType;
			ret.LineNbr = item.LineNbr;
			ret.SplitLineNbr = (short)1;
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.SubItemID = item.SubItemID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;
			ret.ExpireDate = item.ExpireDate;
			ret.Qty = item.Qty;
			ret.UOM = item.UOM;
			ret.ExpireDate= item.ExpireDate;
			ret.BaseQty = item.BaseQty;
			ret.InvtMult = item.InvtMult;
            ret.OrigPlanType = item.OrigPlanType;

			return ret;
		}
		public static implicit operator POReceiptLine(POReceiptLineSplit item)
		{
			POReceiptLine ret = new POReceiptLine();
			ret.ReceiptNbr = item.ReceiptNbr;
			ret.LineNbr = item.LineNbr;
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.SubItemID = item.SubItemID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;
			ret.Qty = item.Qty;
			ret.UOM = item.UOM;
			ret.ExpireDate = item.ExpireDate;
			ret.BaseQty = item.BaseQty;
			ret.InvtMult = item.InvtMult;
            ret.OrigPlanType = item.OrigPlanType;
			return ret;
		}
		
		#endregion

		#region CuryLineAmt
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }
		[PXDecimal(4)]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual decimal? CuryLineAmt
		{
			get { return null; }
			set { }
		}
		#endregion
		#region CuryUnitCost
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }
		[PXDecimal(4)]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual decimal? CuryUnitCost
		{
			get { return null; }
			set { }
		}
		#endregion
	}

	[PXHidden]
	public partial class POReceiptLine2 : POReceiptLine
	{
		#region ReceiptNbr
		public new abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		#endregion
		#region LineNbr
		public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion

		#region OrigReceiptNbr
		public new abstract class origReceiptNbr : PX.Data.BQL.BqlString.Field<origReceiptNbr> { }
		#endregion
		#region OrigReceiptLineNbr
		public new abstract class origReceiptLineNbr : PX.Data.BQL.BqlInt.Field<origReceiptLineNbr> { }
		#endregion
		#region OrigReceiptLineType
		public new abstract class origReceiptLineType : PX.Data.BQL.BqlString.Field<origReceiptLineType> { }
		#endregion
	}
}