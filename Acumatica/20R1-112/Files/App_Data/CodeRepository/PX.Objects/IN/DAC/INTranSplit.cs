using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common.Attributes;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.INTranSplit)]
	public partial class INTranSplit : PX.Data.IBqlTable, ILSDetail
	{
		#region Keys
		public class PK : PrimaryKeyOf<INTranSplit>.By<docType, refNbr, lineNbr, splitLineNbr>
		{
			public static INTranSplit Find(PXGraph graph, string docType, string refNbr, int? lineNbr, int? splitLineNbr) =>
				FindBy(graph, docType, refNbr, lineNbr, splitLineNbr);
		}
		public static class FK
		{
			public class Register : INRegister.PK.ForeignKeyOf<INTranSplit>.By<docType, refNbr> { }
			public class Tran : INTran.PK.ForeignKeyOf<INTranSplit>.By<docType, refNbr, lineNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INTranSplit>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INTranSplit>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INTranSplit>.By<siteID> { }
			public class ToSite : INSite.PK.ForeignKeyOf<INTranSplit>.By<toSiteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<INTranSplit>.By<locationID> { }
			public class ToLocation : INLocation.PK.ForeignKeyOf<INTranSplit>.By<toLocationID> { }
			public class CostSubItem : INSubItem.PK.ForeignKeyOf<INTranSplit>.By<costSubItemID> { }
			public class CostSite : INCostSite.PK.ForeignKeyOf<INTranSplit>.By<costSiteID> { }
			public class ItemPlan : INItemPlan.PK.ForeignKeyOf<INTranSplit>.By<planID> { }
			public class LotSerialStatus : INLotSerialStatus.PK.ForeignKeyOf<INTranSplit>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr> { }
			public class ShipLineSplit : SO.SOShipLineSplit.PK.ForeignKeyOf<INTranSplit>.By<shipmentNbr, shipmentLineNbr, shipmentLineSplitNbr> { }
		}
		#endregion
		#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(typeof(INTran.docType))]
		[PXParent(typeof(FK.Register))]
		[PXUIField(DisplayName = "Type")]
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
		[PXDefault(typeof(INTran.tranType))]
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
		[PXUIField(DisplayName = "Ref. Number")]
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INRegister.refNbr))]
        [PXParent(typeof(FK.Tran))]
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
		[PXDefault(typeof(INTran.lineNbr))]
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
		#region POLineType
		public abstract class pOLineType : PX.Data.BQL.BqlString.Field<pOLineType> { }
		protected String _POLineType;
		[PXDBString(2)]
		[PXDefault(typeof(INTran.pOLineType), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region SOLineType
		public abstract class sOLineType : PX.Data.BQL.BqlString.Field<sOLineType> { }
		protected String _SOLineType;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(typeof(INTran.sOLineType), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String SOLineType
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
		#region TransferType
		public abstract class transferType : PX.Data.BQL.BqlString.Field<transferType> { }
		protected String _TransferType;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(INRegister.transferType), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TransferType
		{
			get
			{
				return this._TransferType;
			}
			set
			{
				this._TransferType = value;
			}
		}
		#endregion
		#region ToSiteID
		public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
		protected Int32? _ToSiteID;
        [PXDBInt()]
		[PXDefault(typeof(INTran.toSiteID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDBInt()]
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
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(INRegister.lineCntr))]
		[PXUIField(DisplayName = "Split Line Number")]
		public virtual Int32? SplitLineNbr
		{
			get
			{
				return this._SplitLineNbr;
			}
			set
			{
				this._SplitLineNbr = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDBDefault(typeof(INRegister.tranDate))]
		[PXUIField(DisplayName = "Transaction Date")]
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
		[PXDefault(typeof(INTran.invtMult))]
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
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(Visible = false)]
		[PXDefault(typeof(INTran.inventoryID))]
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
		#region IsStockItem
		public bool? IsStockItem
		{
			get
			{
				return true;
			}
			set { }
		}
		#endregion
		#region ValMethod
		public abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
		[PXString(1, IsFixed = true)]
		[PXFormula(typeof(Selector<INTranSplit.inventoryID, InventoryItem.valMethod>))]
		public virtual String ValMethod
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[IN.SubItem(typeof(INTranSplit.inventoryID),
			typeof(LeftJoin<INSiteStatus,
				On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
				And<INSiteStatus.inventoryID, Equal<Optional<INTranSplit.inventoryID>>,
				And<INSiteStatus.siteID, Equal<Optional<INTranSplit.siteID>>>>>>))]
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<INTranSplit.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>))]
		[PXFormula(typeof(Default<INTranSplit.inventoryID>))]
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
		[PXDBInt()]
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
		#region CostSiteID
		public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
		protected Int32? _CostSiteID;
		[PXDBInt()]
		public virtual Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region FromSiteID
		public abstract class fromSiteID : PX.Data.BQL.BqlInt.Field<fromSiteID> { }
		protected Int32? _FromSiteID;
		[PXInt()]
		public virtual Int32? FromSiteID
		{
			get
			{
				return this._FromSiteID;
			}
			set
			{
				this._FromSiteID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site()]
		[PXDefault(typeof(INTran.siteID))]
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
		#region FromLocationID
		public abstract class fromLocationID : PX.Data.BQL.BqlInt.Field<fromLocationID> { }
		protected Int32? _FromLocationID;
		[PXInt()]
		public virtual Int32? FromLocationID
		{
			get
			{
				return this._FromLocationID;
			}
			set
			{
				this._FromLocationID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[IN.LocationAvail(typeof(INTranSplit.inventoryID), typeof(INTranSplit.subItemID), typeof(INTranSplit.siteID), typeof(INTranSplit.tranType), typeof(INTranSplit.invtMult))]
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[INLotSerialNbr(typeof(INTranSplit.inventoryID), typeof(INTranSplit.subItemID), typeof(INTranSplit.locationID), typeof(INTran.lotSerialNbr), FieldClass = "LotSerial")]
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
		#region LotSerClassID
		public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
		protected String _LotSerClassID;
		[PXString(10, IsUnicode = true)]
		public virtual String LotSerClassID
		{
			get
			{
				return this._LotSerClassID;
			}
			set
			{
				this._LotSerClassID = value;
			}
		}
		#endregion
		#region AssignedNbr
		public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }
		protected String _AssignedNbr;
		[PXString(30, IsUnicode = true)]
		public virtual String AssignedNbr
		{
			get
			{
				return this._AssignedNbr;
			}
			set
			{
				this._AssignedNbr = value;
			}
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
        [INExpireDate(typeof(INTranSplit.inventoryID), FieldClass = "LotSerial")]
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
		[DBConditionalModifiedDateTime(typeof(released), true)]
		public virtual DateTime? ReleasedDateTime
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
        [INUnit(typeof(INTranSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
		[PXDefault]
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
		[PXDBQuantity(typeof(INTranSplit.uOM), typeof(INTranSplit.baseQty), InventoryUnitType.BaseUnit)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
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
		[PXDBQuantity()]
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
		#region QtyIn
		public abstract class qtyIn : PX.Data.BQL.BqlDecimal.Field<qtyIn> { }

		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<invtMult, LessEqual<short0>>, decimal0>, baseQty>), typeof(decimal))]
		public virtual decimal? QtyIn { get; set; }
		#endregion

		#region QtyOut
		public abstract class qtyOut : PX.Data.BQL.BqlDecimal.Field<qtyOut> { }

		[PXQuantity]
		[PXDBCalced(typeof(Switch<Case<Where<invtMult, GreaterEqual<short0>>, decimal0>, baseQty>), typeof(decimal))]
		public virtual decimal? QtyOut { get; set; }
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
        #region OrigPlanType
        public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
		[PXDBString(2, IsFixed = true)]
		[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
        [PXDefault(typeof(INTran.origPlanType), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String OrigPlanType
		{
			get;
			set;
		}
		#endregion
        #region IsFixedInTransit
        public abstract class isFixedInTransit : PX.Data.BQL.BqlBool.Field<isFixedInTransit> { }
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? IsFixedInTransit
        {
            get;
            set;
        }
        #endregion
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
        [PXDBLong(IsImmutable = true)]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion
		#region TotalQty
		public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalQty
		{
			get;
			set;
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalCost
		{
			get;
			set;
		}
		#endregion
        #region AdditionalCost
        public abstract class additionalCost : PX.Data.BQL.BqlDecimal.Field<additionalCost> { }
        protected Decimal? _AdditionalCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? AdditionalCost
        {
            get
            {
                return this._AdditionalCost;
            }
            set
            {
                this._AdditionalCost = value;
            }
        }
        #endregion		
        #region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBPriceCostCalced(typeof(Switch<Case<Where<INTranSplit.totalQty, Equal<decimal0>>, decimal0>, Div<INTranSplit.totalCost, INTranSplit.totalQty>>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
		[PXPriceCost()]
		[PXUIField(DisplayName = "Unit Cost", Enabled = false)]
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
		#region EstCost
		public abstract class estCost : PX.Data.IBqlField
		{
		}
		[PXDBPriceCostCalced(typeof(Switch<Case<Where<INTranSplit.totalQty, Equal<decimal0>>, decimal0>, Div<Mult<INTranSplit.baseQty, INTranSplit.totalCost>, INTranSplit.totalQty>>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
		[PXPriceCost()]
		[PXUIField(DisplayName = "Estimated Cost", Enabled = false)]
		public virtual Decimal? EstCost
		{
			get;
			set;
		}
		#endregion
		#region SkipCostUpdate
        public abstract class skipCostUpdate : PX.Data.BQL.BqlBool.Field<skipCostUpdate> { }
        protected Boolean? _SkipCostUpdate;
        [PXBool()]
        public virtual Boolean? SkipCostUpdate
        {
            get
            {
                return this._SkipCostUpdate;
            }
            set
            {
                this._SkipCostUpdate = value;
            }
        }
        #endregion
        #region SkipQtyValidation
        [PXBool]
        public virtual Boolean? SkipQtyValidation { get; set; }
        public abstract class skipQtyValidation : PX.Data.BQL.BqlBool.Field<skipQtyValidation> { }
        #endregion

        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXInt]
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
		[PXInt]
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

		#region ShipmentNbr
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		#endregion
		#region ShipmentLineNbr
		[PXDBInt]
		public virtual Int32? ShipmentLineNbr { get; set; }
		public abstract class shipmentLineNbr : PX.Data.BQL.BqlInt.Field<shipmentLineNbr> { }
		#endregion
		#region ShipmentLineSplitNbr
		[PXDBInt]
		public virtual Int32? ShipmentLineSplitNbr { get; set; }
		public abstract class shipmentLineSplitNbr : PX.Data.BQL.BqlInt.Field<shipmentLineSplitNbr> { }
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

    //added for join purpose
    [System.SerializableAttribute()]
    [PXHidden]
    public partial class INTranSplit2 : INTranSplit
    {
        #region DocType
        public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        #endregion
        #region RefNbr
        public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        #endregion
        #region LineNbr
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion
        #region SubItemID
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        #endregion
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        #endregion
        #region BaseQty
        public new abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
        #endregion
    }

    public class INTranSplitCostComparer : System.Collections.Generic.IEqualityComparer<INTranSplit>
	{
		public INTranSplitCostComparer()
		{
		}

		#region IEqualityComparer<INTranSplit> Members

		public bool Equals(INTranSplit x, INTranSplit y)
		{
			return x.DocType == y.DocType 
				&& x.RefNbr == y.RefNbr
				&& x.LineNbr == y.LineNbr
				&& x.CostSiteID == y.CostSiteID
				&& x.CostSubItemID == y.CostSubItemID
				&& (x.ValMethod != INValMethod.Specific || x.LotSerialNbr == y.LotSerialNbr);
		}

		public int GetHashCode(INTranSplit obj)
		{
			unchecked
			{
				int ret = 17;
				ret = ret * 23 + obj.DocType.GetHashCode();
				ret = ret * 23 + obj.RefNbr.GetHashCode();
				ret = ret * 23 + obj.LineNbr.GetHashCode();
				ret = ret * 23 + obj.CostSiteID.GetHashCode();
				return ret * 23 + obj.CostSubItemID.GetHashCode();
			}
		}

		#endregion
	}

	[PXCacheName(Messages.INTranDetail)]
	[PXProjection(typeof(Select2<INTranSplit,
		InnerJoin<INTran, 
			On<INTranSplit.FK.Tran>>, Where<INTranSplit.released, Equal<True>>>))]
    [Serializable]
	public partial class INTranDetail : PX.Data.IBqlTable
	{
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(INTranSplit.tranType))]
		[PXUIField(DisplayName = "Transaction Type")]
		[INTranType.List()]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTranSplit.refNbr))]
		[PXUIField(DisplayName = "Ref. Number")]
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
		[PXDBInt(IsKey = true, BqlField = typeof(INTranSplit.lineNbr))]
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
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(INTranSplit.splitLineNbr))]
		public virtual Int32? SplitLineNbr
		{
			get
			{
				return this._SplitLineNbr;
			}
			set
			{
				this._SplitLineNbr = value;
			}
		}
		#endregion 
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate(BqlField = typeof(INTranSplit.tranDate))]
		[PXUIField(DisplayName = "Transaction Date")]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[GL.FinPeriodID(BqlField = typeof(INTran.finPeriodID))]
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
		[GL.FinPeriodID(BqlField = typeof(INTran.tranPeriodID))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(BqlField = typeof(INTranSplit.inventoryID))]
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
		#region CostSubItemID
		public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
		protected Int32? _CostSubItemID;
		[SubItem(BqlField = typeof(INTranSplit.costSubItemID))]
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
		#region CostSiteID
		public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
		protected Int32? _CostSiteID;
		[PXDBInt(BqlField = typeof(INTranSplit.costSiteID))]
		public virtual Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort(BqlField = typeof(INTranSplit.invtMult))]
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
		#region SumQty
		public abstract class sumQty : PX.Data.BQL.BqlDecimal.Field<sumQty> { }
		protected Decimal? _SumQty;
		[PXDBQuantity(BqlField = typeof(INTranSplit.totalQty))]
		public virtual Decimal? SumQty
		{
			get
			{
				return this._SumQty;
			}
			set
			{
				this._SumQty = value;
			}
		}
		#endregion
		#region SumTranCost
		public abstract class sumTranCost : PX.Data.BQL.BqlDecimal.Field<sumTranCost> { }
		protected Decimal? _SumTranCost;
		[CM.PXDBBaseCury(BqlField = typeof(INTranSplit.totalCost))]
		public virtual Decimal? SumTranCost
		{
			get
			{
				return this._SumTranCost;
			}
			set
			{
				this._SumTranCost = value;
			}
		}
		#endregion
		////INTranSplit
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(BqlField = typeof(INTranSplit.subItemID))]
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
		[IN.Site(BqlField = typeof(INTranSplit.siteID))]
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
		[IN.Location(BqlField = typeof(INTranSplit.locationID))]
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
		[PXDBString(100, IsUnicode = true, BqlField = typeof(INTranSplit.lotSerialNbr))]
		[PXUIField(DisplayName = "Lot/Serial Number")]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDBString(6, IsUnicode = true, BqlField = typeof(INTranSplit.uOM))]
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
		[PXDBQuantity(BqlField = typeof(INTranSplit.qty))]
		[PXUIField(DisplayName = "Qty.")]
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
		[PXDBQuantity(BqlField = typeof(INTranSplit.baseQty))]
		[PXUIField(DisplayName = "Base Qty.")]
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
		#region TranCost
		public abstract class tranCost : PX.Data.BQL.BqlDecimal.Field<tranCost> { }
		protected Decimal? _TranCost;
		[CM.PXBaseCury()]
		public virtual Decimal? TranCost
		{
            [PXDependsOnFields(typeof(tranType), typeof(sumTranCost), typeof(sumQty), typeof(baseQty))]
			get
			{
				if (TranType == INTranType.Adjustment || TranType == INTranType.StandardCostAdjustment || TranType == INTranType.NegativeCostAdjustment)
				{
					return SumTranCost;
				}
				else if (SumQty == 0m)
				{
					return 0m;
				}
				else
				{
					return BaseQty * SumTranCost / SumQty;
				}
			}
			set
			{
				this._TranCost = value;
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select5<INCostStatus,
		InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>,
		CrossJoin<CommonSetup>>, 
		Aggregate<GroupBy<INCostStatus.inventoryID, GroupBy<INCostStatus.costSiteID, GroupBy<INCostSubItemXRef.subItemID, Sum<INCostStatus.qtyOnHand, Sum<INCostStatus.totalCost>>>>>>>))]
    [Serializable]
	public partial class INSiteCostStatus : IBqlTable 
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.inventoryID))]
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
		[PXDBInt(IsKey = true, BqlField = typeof(INCostSubItemXRef.subItemID))]
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
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.costSiteID))]
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
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
        [PXDBQuantity(BqlField = typeof(INCostStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected Decimal? _TotalCost;
        [CM.PXDBBaseCury(BqlField = typeof(INCostStatus.totalCost))]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region DecPlPrcCst
		public abstract class decPlPrcCst : PX.Data.BQL.BqlShort.Field<decPlPrcCst> { }
		protected Int16? _DecPlPrcCst;
		[PXDBShort(BqlField = typeof(CommonSetup.decPlPrcCst))]
		public virtual Int16? DecPlPrcCst
		{
			get
			{
				return this._DecPlPrcCst;
			}
			set
			{
				this._DecPlPrcCst = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXPriceCost()]
		public virtual Decimal? UnitCost
		{
            [PXDependsOnFields(typeof(qtyOnHand), typeof(totalCost), typeof(decPlPrcCst))]
			get
			{
				return (this.QtyOnHand == null || this.TotalCost == null) ? (decimal?)null : (this.QtyOnHand != 0m) ? Math.Round((decimal)this.TotalCost / (decimal)this.QtyOnHand, (int)this.DecPlPrcCst, MidpointRounding.AwayFromZero) : 0m;
			}
			set
			{
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select5<INCostStatus,
		InnerJoin<INLocation, 
			On<INLocation.locationID, Equal<INCostStatus.costSiteID>>,
		InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>,
		CrossJoin<CommonSetup>>>,
		Aggregate<GroupBy<INCostStatus.inventoryID, GroupBy<INCostSubItemXRef.subItemID, GroupBy<INCostStatus.costSiteID, Sum<INCostStatus.qtyOnHand, Sum<INCostStatus.totalCost>>>>>>>))]
    [Serializable]
	public partial class INLocationCostStatus : IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.inventoryID))]
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
		[PXDBInt(IsKey = true, BqlField = typeof(INCostSubItemXRef.subItemID))]
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
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.costSiteID))]
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
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField = typeof(INCostStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected Decimal? _TotalCost;
		[CM.PXDBBaseCury(BqlField = typeof(INCostStatus.totalCost))]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region DecPlPrcCst
		public abstract class decPlPrcCst : PX.Data.BQL.BqlShort.Field<decPlPrcCst> { }
		protected Int16? _DecPlPrcCst;
		[PXDBShort(BqlField = typeof(CommonSetup.decPlPrcCst))]
		public virtual Int16? DecPlPrcCst
		{
			get
			{
				return this._DecPlPrcCst;
			}
			set
			{
				this._DecPlPrcCst = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXPriceCost()]
		public virtual Decimal? UnitCost
		{
			[PXDependsOnFields(typeof(qtyOnHand), typeof(totalCost), typeof(decPlPrcCst))]
			get
			{
                return (this.QtyOnHand == null || this.TotalCost == null) ? (decimal?)null : (this.QtyOnHand != 0m) ? Math.Round((decimal)this.TotalCost / (decimal)this.QtyOnHand, (int)this.DecPlPrcCst, MidpointRounding.AwayFromZero) : 0m;
            }
			set
			{
			}
		}
		#endregion
	}

    [PXProjection(typeof(Select5<INCostStatus,
		InnerJoin<INCostSubItemXRef, On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>,
		CrossJoin<CommonSetup>>,
		Aggregate<GroupBy<INCostStatus.inventoryID, GroupBy<INCostSubItemXRef.subItemID, GroupBy<INCostStatus.costSiteID, GroupBy<INCostStatus.lotSerialNbr, Sum<INCostStatus.qtyOnHand, Sum<INCostStatus.totalCost>>>>>>>>))]
    [Serializable]
	public partial class INLotSerialCostStatus : IBqlTable
	{ 
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.inventoryID))]
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
		[PXDBInt(IsKey = true, BqlField = typeof(INCostSubItemXRef.subItemID))]
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
		[PXDBInt(IsKey = true, BqlField = typeof(INCostStatus.costSiteID))]
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true, IsKey = true, BqlField = typeof(INCostStatus.lotSerialNbr))]
		[PXDefault()]
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
        [PXDBQuantity(BqlField = typeof(INCostStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected Decimal? _TotalCost;
        [CM.PXDBBaseCury(BqlField = typeof(INCostStatus.totalCost))]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region DecPlPrcCst
		public abstract class decPlPrcCst : PX.Data.BQL.BqlShort.Field<decPlPrcCst> { }
		protected Int16? _DecPlPrcCst;
		[PXDBShort(BqlField = typeof(CommonSetup.decPlPrcCst))]
		public virtual Int16? DecPlPrcCst
		{
			get
			{
				return this._DecPlPrcCst;
			}
			set
			{
				this._DecPlPrcCst = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXPriceCost()]
		public virtual Decimal? UnitCost
		{
            [PXDependsOnFields(typeof(qtyOnHand), typeof(totalCost), typeof(decPlPrcCst))]
			get
			{
                return (this.QtyOnHand == null || this.TotalCost == null) ? (decimal?)null : (this.QtyOnHand != 0m) ? Math.Round((decimal)this.TotalCost / (decimal)this.QtyOnHand, (int)this.DecPlPrcCst, MidpointRounding.AwayFromZero) : 0m;
            }
			set
			{
			}
		}
		#endregion
	}
}
