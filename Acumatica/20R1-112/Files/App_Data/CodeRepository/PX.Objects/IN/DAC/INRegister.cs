using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.Common.Attributes;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.SO;
using PX.Objects.PO;

namespace PX.Objects.IN
{
	[Serializable]
	[PXPrimaryGraph(new Type[] {
					typeof(INReceiptEntry),
					typeof(INIssueEntry),
					typeof(INTransferEntry),
					typeof(INAdjustmentEntry),
					typeof(KitAssemblyEntry),
					typeof(KitAssemblyEntry)},
					new Type[] {
					typeof(Where<INRegister.docType, Equal<INDocType.receipt>>),
					typeof(Where<INRegister.docType, Equal<INDocType.issue>>),
					typeof(Where<INRegister.docType, Equal<INDocType.transfer>>),
					typeof(Where<INRegister.docType, Equal<INDocType.adjustment>>),
					typeof(Select<INKitRegister, Where<INKitRegister.docType, Equal<INDocType.production>, And<INKitRegister.refNbr, Equal<Current<INRegister.refNbr>>>>>),
					typeof(Select<INKitRegister, Where<INKitRegister.docType, Equal<INDocType.disassembly>, And<INKitRegister.refNbr, Equal<Current<INRegister.refNbr>>>>>),
					})]
	[INRegisterCacheName(Messages.Register)]
	public partial class INRegister : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INRegister>.By<docType, refNbr>
		{
			public static INRegister Find(PXGraph graph, string docType, string refNbr) => FindBy(graph, docType, refNbr);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INRegister>.By<siteID> { }
			public class ToSite : INSite.PK.ForeignKeyOf<INRegister>.By<toSiteID> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<INRegister>.By<branchID> { }
			public class KitTran : INTran.PK.ForeignKeyOf<INRegister>.By<docType, refNbr, kitLineNbr> { }
			public class KitSpecHdr : INKitSpecHdr.PK.ForeignKeyOf<INRegister>.By<kitInventoryID, kitRevisionID> { }
		}
        #endregion
        #region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
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
		[Branch()]
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
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType>
		{
			public const string DisplayName = "Document Type";
		}
		protected String _DocType;
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[INDocType.List()]
		[PXUIField(DisplayName = docType.DisplayName, Enabled = false, Visibility=PXUIVisibility.SelectorVisible)]
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
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
			public const string DisplayName = "Reference Nbr.";
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName= refNbr.DisplayName, Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Optional<INRegister.docType>>>, OrderBy<Desc<INRegister.refNbr>>>), Filterable = true)]
		[INDocType.Numbering()]
		[PX.Data.EP.PXFieldDescription]
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
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule>
		{
            public const string PI = "PI";

			public class List : PXStringListAttribute
			{
				public List() : base(
					new[]
					{
						Pair(BatchModule.SO, GL.Messages.ModuleSO),
						Pair(BatchModule.PO, GL.Messages.ModulePO),
						Pair(BatchModule.IN, GL.Messages.ModuleIN),
						Pair(PI, Messages.ModulePI),
						Pair(BatchModule.AP, GL.Messages.ModuleAP),
					}) {}
			}
		}
		protected String _OrigModule;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(GL.BatchModule.IN)]
		[PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[origModule.List()]
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
		#region SrcDocType
		public abstract class srcDocType : PX.Data.BQL.BqlString.Field<srcDocType> { }
		/// <summary>
		/// The field is used for consolidation by source document (Shipment or Invoice for direct stock item lines) of IN Issues created from one Invoice
		/// </summary>
		[PXString(3)]
		public virtual string SrcDocType { get; set; }
		#endregion
		#region SrcRefNbr
		public abstract class srcRefNbr : PX.Data.BQL.BqlString.Field<srcRefNbr> { }
		/// <summary>
		/// The field is used for consolidation by source document (Shipment or Invoice for direct stock item lines) of IN Issues created from one Invoice
		/// </summary>
		[PXString(15, IsUnicode = true)]
		public virtual string SrcRefNbr { get; set; }
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site(DisplayName = "Warehouse ID", DescriptionField=typeof(INSite.descr))]
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
		#region ToSiteID
		public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
		protected Int32? _ToSiteID;
		[IN.ToSite(DisplayName = "To Warehouse ID",DescriptionField = typeof(INSite.descr))]
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
		#region TransferType
		public abstract class transferType : PX.Data.BQL.BqlString.Field<transferType> { }
		protected String _TransferType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INTransferType.OneStep)]
		[INTransferType.List()]
		[PXUIField(DisplayName = "Transfer Type")]
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
		#region TransferNbr
		public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }
		protected String _TransferNbr;
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Transfer Nbr.")]
        /// <summary>
        /// Field used in INReceiptEntry screen. 
        /// Unbound, calculated field. Filled up only on correspondent screen.
        /// </summary>
		public virtual String TransferNbr
		{
			get
			{
				return this._TransferNbr;
			}
			set
			{
				this._TransferNbr = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName="Description", Visibility=PXUIVisibility.SelectorVisible)]
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
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		[NoUpdateDBField(NoInsert = true)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXDefault(typeof(INSetup.holdEntry))]
		[PXUIField(DisplayName="Hold")]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName="Status", Visibility=PXUIVisibility.SelectorVisible, Enabled=false)]
		[INDocStatus.List()]
		public virtual String Status
		{
			[PXDependsOnFields(typeof(released), typeof(hold))]
			get
			{
				return this._Status;
			}
			set
			{
				//this._Status = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
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
		[INOpenPeriod(
			sourceType: typeof(INRegister.tranDate), 
			branchSourceType: typeof(INRegister.branchID), 
			masterFinPeriodIDType: typeof(INRegister.tranPeriodID),
			IsHeader = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected Int32? _LineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion
		#region TotalQty
		public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }
		protected Decimal? _TotalQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Qty.", Visibility=PXUIVisibility.SelectorVisible, Enabled=false)]
		public virtual Decimal? TotalQty
		{
			get
			{
				return this._TotalQty;
			}
			set
			{
				this._TotalQty = value;
			}
		}
		#endregion
		#region TotalAmount
		public abstract class totalAmount : PX.Data.BQL.BqlDecimal.Field<totalAmount> { }
		protected Decimal? _TotalAmount;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Amount", Visibility=PXUIVisibility.SelectorVisible, Enabled=false)]
		public virtual Decimal? TotalAmount
		{
			get
			{
				return this._TotalAmount;
			}
			set
			{
				this._TotalAmount = value;
			}
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected Decimal? _TotalCost;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
		#region ControlQty
		public abstract class controlQty : PX.Data.BQL.BqlDecimal.Field<controlQty> { }
		protected Decimal? _ControlQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Control Qty.")]
		public virtual Decimal? ControlQty
		{
			get
			{
				return this._ControlQty;
			}
			set
			{
				this._ControlQty = value;
			}
		}
		#endregion
		#region ControlAmount
		public abstract class controlAmount : PX.Data.BQL.BqlDecimal.Field<controlAmount> { }
		protected Decimal? _ControlAmount;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Amount")]
		public virtual Decimal? ControlAmount
		{
			get
			{
				return this._ControlAmount;
			}
			set
			{
				this._ControlAmount = value;
			}
		}
		#endregion
		#region ControlCost
		public abstract class controlCost : PX.Data.BQL.BqlDecimal.Field<controlCost> { }
		protected Decimal? _ControlCost;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Cost")]
		public virtual Decimal? ControlCost
		{
			get
			{
				return this._ControlCost;
			}
			set
			{
				this._ControlCost = value;
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleIN>>>))]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "External Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion			
		#region KitInventoryID
		public abstract class kitInventoryID : PX.Data.BQL.BqlInt.Field<kitInventoryID> { }
		protected Int32? _KitInventoryID;
		[PXDBInt]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID>), typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
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
		#region KitRevisionID
		public abstract class kitRevisionID : PX.Data.BQL.BqlString.Field<kitRevisionID> { }
		protected String _KitRevisionID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Revision")]
		public virtual String KitRevisionID
		{
			get
			{
				return this._KitRevisionID;
			}
			set
			{
				this._KitRevisionID = value;
			}
		}
		#endregion
		#region KitLineNbr
		public abstract class kitLineNbr : PX.Data.BQL.BqlInt.Field<kitLineNbr> { }
		protected Int32? _KitLineNbr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? KitLineNbr
		{
			get
			{
				return this._KitLineNbr;
			}
			set
			{
				this._KitLineNbr = value;
			}
		}
		#endregion
		#region SOOrderType
		public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
		protected String _SOOrderType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "SO Order Type", Visible = false)]
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
		[PXUIField(DisplayName = "SO Order Nbr.", Visible = false, Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<INRegister.sOOrderType>>>>))]
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
		[PXUIField(DisplayName = "SO Shipment Nbr.", Visible = false, Enabled = false)]
		[PXSelector(typeof(Search<SOShipment.shipmentNbr>))]
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
		[PXUIField(DisplayName = "PO Receipt Nbr.", Visible = false, Enabled = false)]
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
		#region PIID
		public abstract class pIID : Data.BQL.BqlString.Field<pIID>
		{
		}

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName="PI Count Reference Nbr.", IsReadOnly = true)]
		[PXSelector(typeof(Search<INPIHeader.pIID>))]
		public virtual String PIID
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.IN, "{0}: {1}", new Type[] { typeof(INRegister.docType), typeof(INRegister.refNbr) },
			new Type[] { typeof(INRegister.tranDesc), typeof(INRegister.extRefNbr), typeof(INRegister.transferNbr) },
			NumberFields = new Type[] { typeof(INRegister.refNbr) },
			Line1Format = "{0}{1:d}{2}{3}{4}", Line1Fields = new Type[] { typeof(INRegister.extRefNbr), typeof(INRegister.tranDate), typeof(INRegister.transferType), typeof(INRegister.transferNbr), typeof(INRegister.status) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(INRegister.tranDesc) },
			WhereConstraint = typeof(Where<INRegister.docType, NotEqual<INDocType.production>, And<INRegister.docType, NotEqual<INDocType.disassembly>>>)
		)]
		[PXNote(DescriptionField = typeof(INRegister.refNbr), 
			Selector = typeof(INRegister.refNbr))]
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
		#region IsPPVTran
		public abstract class isPPVTran : PX.Data.BQL.BqlBool.Field<isPPVTran> { }
		protected bool? _IsPPVTran = false;
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsPPVTran
		{
			get
			{
				return _IsPPVTran;
			}
			set
			{
				_IsPPVTran = value;
			}
		}
		#endregion
		#region IsTaxAdjustmentTran
		public abstract class isTaxAdjustmentTran : PX.Data.BQL.BqlBool.Field<isTaxAdjustmentTran> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsTaxAdjustmentTran
		{
			get;
			set;
		}
		#endregion
		#region Methods
		protected virtual void SetStatus()
		{
			if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = INDocStatus.Hold;
			}
			else if (this._Released != null && this._Released == false)
			{
				this._Status = INDocStatus.Balanced;
			}
			else 
			{
				this._Status = INDocStatus.Released;
			}
		}
		#endregion
	}

    [PXProjection(typeof(Select4<INTransitLineStatus, Where<INTransitLineStatus.qtyOnHand, Greater<Zero>>, Aggregate<GroupBy<INTransitLineStatus.transferNbr>>>))]
    [Serializable]
    [PXHidden]
	public partial class INTransferInTransit : IBqlTable
	{
        #region TransferNbr
        public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }
        protected String _TransferNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTransitLineStatus.transferNbr))]
        public virtual String TransferNbr
        {
            get
            {
                return this._TransferNbr;
            }
            set
            {
                this._TransferNbr = value;
            }
        }
        #endregion

        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
        protected Guid? _RefNoteID;

        [PXNote(BqlField = typeof(INTransitLineStatus.refNoteID))]
        public virtual Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        #endregion
    }

    [PXProjection(typeof(Select<INTransitLineStatus, Where<INTransitLineStatus.qtyOnHand, Greater<Zero>>>))]
    [Serializable]
    [PXHidden]
    public partial class INTranInTransit : IBqlTable
    {
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        protected String _RefNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTransitLineStatus.transferNbr))]
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
        [PXDBInt(IsKey = true, BqlField = typeof(INTransitLineStatus.transferLineNbr))]
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
        #region OrigModule
        public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
        protected String _OrigModule;
        [PXDBString(2, IsFixed = true, BqlField = typeof(INTransitLineStatus.origModule))]
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
    }


    public class INDocType
	{
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(
					typeof(INRegister.docType),
					typeof(INRegister.tranDate),
					Pair(Issue).To<INSetup.issueNumberingID>(),
					Pair(Receipt).To<INSetup.receiptNumberingID>(),
					Pair(Transfer).To<INSetup.receiptNumberingID>(),
					Pair(Adjustment).To<INSetup.adjustmentNumberingID>(),
					Pair(Production).To<INSetup.kitAssemblyNumberingID>(),
					Pair(Change).To<INSetup.kitAssemblyNumberingID>(),
					Pair(Disassembly).To<INSetup.kitAssemblyNumberingID>()) { }
		}

		public class ListAttribute : PXStringListAttribute
	    {
		    public ListAttribute() : base(
			    new[]
				{
					Pair(Issue, Messages.Issue),
					Pair(Receipt, Messages.Receipt),
					Pair(Transfer, Messages.Transfer),
					Pair(Adjustment, Messages.Adjustment),
					Pair(Production, Messages.Production),
					Pair(Disassembly, Messages.Disassembly),
				}) {}
	    }

	    public class KitListAttribute : PXStringListAttribute
	    {
		    public KitListAttribute() : base(
			    new[]
				{
					Pair(Production, Messages.Production),
					Pair(Disassembly, Messages.Disassembly),
				}) {}
	    }

	    public class SOListAttribute : PXStringListAttribute
	    {
		    public SOListAttribute() : base(
			    new[]
				{
					Pair(Issue, Messages.Issue),
					Pair(Receipt, Messages.Receipt),
					Pair(Transfer, Messages.Transfer),
					Pair(Adjustment, Messages.Adjustment),
					Pair(Production, Messages.Production),
					Pair(Disassembly, Messages.Disassembly),
					Pair(DropShip, Messages.DropShip),
				}) {}
	    }

	    public const string Undefined = "0";
		public const string Issue = "I";
		public const string Receipt = "R";
		public const string Transfer = "T";
		public const string Adjustment = "A";
		public const string Production = "P";
		public const string Change = "C";
		public const string Disassembly = "D";
		public const string DropShip = "H";
		public const string Invoice = "N";

		public class undefined : PX.Data.BQL.BqlString.Constant<undefined>
		{
			public undefined() : base(Undefined) { ;}
		}
		
		public class issue : PX.Data.BQL.BqlString.Constant<issue>
		{
			public issue() : base(Issue) { ;}
		}

		public class receipt : PX.Data.BQL.BqlString.Constant<receipt>
		{
			public receipt() : base(Receipt) { ;}
		}

		public class transfer : PX.Data.BQL.BqlString.Constant<transfer>
		{
			public transfer() : base(Transfer) { ;}
		}

		public class adjustment : PX.Data.BQL.BqlString.Constant<adjustment>
		{
			public adjustment() : base(Adjustment) { ;}
		}

		public class production : PX.Data.BQL.BqlString.Constant<production>
		{
			public production() : base(Production) { ;}
		}
		public class change : PX.Data.BQL.BqlString.Constant<change>
		{
			public change() : base(Change) { ;}
		}
		public class disassembly : PX.Data.BQL.BqlString.Constant<disassembly>
		{
			public disassembly() : base(Disassembly) { ;}
		}
		public class dropShip : PX.Data.BQL.BqlString.Constant<dropShip>
		{
			public dropShip() : base(DropShip) { ;}
		}
	}

	public class INDocStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Hold, Messages.Hold),
					Pair(Balanced, Messages.Balanced),
					Pair(Released, Messages.Released),
				}) {}
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Released = "R";

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { ;}
		}

		public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
		{
			public balanced() : base(Balanced) { ;}
		}

		public class released : PX.Data.BQL.BqlString.Constant<released>
		{
			public released() : base(Released) { ;}
		}

	}

	public class INTransferType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(OneStep, Messages.OneStep),
					Pair(TwoStep, Messages.TwoStep),
				}) {}
		}

		public const string OneStep = "1";
		public const string TwoStep = "2";


		public class oneStep : PX.Data.BQL.BqlString.Constant<oneStep>
		{
			public oneStep() : base(OneStep) { ;}
		}

		public class twoStep : PX.Data.BQL.BqlString.Constant<twoStep>
		{
			public twoStep() : base(TwoStep) { ;}
		}
	}
}