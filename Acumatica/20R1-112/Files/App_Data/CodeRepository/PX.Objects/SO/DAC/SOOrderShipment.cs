using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.SO
{
	using System;
	using PX.Common;
	using PX.Data;
	using PX.Objects.Common;
	using PX.Objects.IN;
	using PX.Objects.AR;
	using PX.Objects.CR;
	using PX.Objects.CS;
	using PX.Objects.CM;
	using POReceipt = PX.Objects.PO.POReceipt;
	using POReceiptLine = PX.Objects.PO.POReceiptLine;
	using POReceiptEntry = PX.Objects.PO.POReceiptEntry;
    using System.Linq;
    using PX.Data.BQL;
    using System.Collections.Generic;
    using PX.Objects.SO.Attributes;
    using PX.Objects.Common.Attributes;

    [PXPrimaryGraph(new Type[] {
					typeof(SOShipmentEntry),
					typeof(POReceiptEntry),
					typeof(SOInvoiceEntry)},
				new Type[] {
					typeof(Select2<SOShipment, 
						LeftJoin<INSite, 
							On<SOShipment.FK.Site>>, 
						Where<SOShipment.noteID, Equal<Current<SOOrderShipment.shippingRefNoteID>>, 
							And<SOShipmentType.dropShip, NotEqual<Current<SOOrderShipment.shipmentType>>, 
							And<Where<INSite.siteID, IsNull, Or<Match<INSite, Current<AccessInfo.userName>>>>>>>>),
					typeof(Select<POReceipt, Where<POReceipt.noteID, Equal<Current<SOOrderShipment.shippingRefNoteID>>, And<SOShipmentType.dropShip, Equal<Current<SOOrderShipment.shipmentType>>>>>) ,
					typeof(Select<ARInvoice, Where<ARInvoice.noteID, Equal<Current<SOOrderShipment.shippingRefNoteID>>, And<ARInvoice.origModule, Equal<GL.BatchModule.moduleSO>>>>),
					},
					UseParent = false)]
	[Serializable()]
	[PXCacheName(Messages.SOOrderShipment)]
	public partial class SOOrderShipment : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOOrderShipment>.By<shipmentType, shipmentNbr, orderType, orderNbr>
		{
			public static SOOrderShipment Find(PXGraph graph, string shipmentType, string shipmentNbr, string orderType, string orderNbr)
				=> FindBy(graph, shipmentType, shipmentNbr, orderType, orderNbr);
		}
		public static class FK
		{
			public class Order : SOOrder.PK.ForeignKeyOf<SOOrderShipment>.By<orderType, orderNbr> { }
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOOrderShipment>.By<orderType> { }
			public class OrderTypeOperation : SOOrderTypeOperation.PK.ForeignKeyOf<SOOrderShipment>.By<orderType, operation> { }
			public class Invoice : SOInvoice.PK.ForeignKeyOf<SOOrderShipment>.By<invoiceType, invoiceNbr> { }
			public class ARRegister : AR.ARRegister.PK.ForeignKeyOf<SOOrderShipment>.By<invoiceType, invoiceNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<SOOrderShipment>.By<siteID> { }
			public class ShipAddress : SOShipmentAddress.PK.ForeignKeyOf<SOOrderShipment>.By<shipAddressID> { }
			public class ShipContact : SOShipmentContact.PK.ForeignKeyOf<SOOrderShipment>.By<shipContactID> { }
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
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
		[PXSelector(typeof(Search<SOOrderType.orderType>))]
		[PXDefault()]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsKey = true, InputMask = "", IsUnicode = true)]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<SOOrderShipment.orderType>>>>))]
		[PXParent(typeof(FK.Order))]
		[PXDefault()]
		[PXFormula(null, typeof(CountCalc<SOShipment.orderCntr>))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		[PXUIField(DisplayName = "Operation")]
		[SOOperation.List]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region ShippingRefNoteID
		public abstract class shippingRefNoteID : PX.Data.BQL.BqlGuid.Field<shippingRefNoteID> { }
		[PXDBGuid(IsKey = true)]
		[PXDefault(typeof(SOShipment.noteID))]
		public virtual Guid? ShippingRefNoteID
		{
			get;
			set;
		}

		#endregion
		#region DisplayShippingRefNoteID
		public abstract class displayShippingRefNoteID : PX.Data.BQL.BqlGuid.Field<displayShippingRefNoteID> { }
		[ShippingRefNote]
		[PXFormula(typeof(shippingRefNoteID))]
		[PXUIField(DisplayName = "Document Nbr.")]
		public virtual Guid? DisplayShippingRefNoteID
		{
			get;
			set;
		}
		#endregion
		#region ShipmentType
		public abstract class shipmentType : PX.Data.BQL.BqlString.Field<shipmentType> { }
		protected String _ShipmentType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(SOShipment.shipmentType))]
		[SOShipmentType.List()]
		[PXUIField(DisplayName = "Shipment Type")]
		public virtual String ShipmentType
		{
			get
			{
				return this._ShipmentType;
			}
			set
			{
				this._ShipmentType = value;
			}
		}
		#endregion
		#region ShipmentNbr
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		protected String _ShipmentNbr;
		[PXDBString(15, InputMask = "", IsUnicode = true)]
		[PXDBLiteDefault(typeof(SOShipment.shipmentNbr), DefaultForInsert = false, DefaultForUpdate = false)]
		[PXUIField(DisplayName = "Shipment Nbr.", Visible = false, Enabled = false)]
		[PXParent(typeof(Select<SOShipment, Where<SOShipment.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>, And<SOShipment.shipmentType, Equal<Current<SOOrderShipment.shipmentType>>>>>))]
		public virtual String ShipmentNbr
		{
			get
			{
				return this._ShipmentNbr;
			}
			set
			{
				this._ShipmentNbr = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer(typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>))] // TODO: remove fake Where after AC-101187
		[CustomerOrOrganizationRestrictor(typeof(shipmentType))]
		[PXDefault(typeof(SOShipment.customerID))]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		protected Int32? _CustomerLocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<SOOrderShipment.customerID>>>), DescriptionField = typeof(Location.descr))]
		[PXDefault(typeof(SOShipment.customerLocationID))]
		public virtual Int32? CustomerLocationID
		{
			get
			{
				return this._CustomerLocationID;
			}
			set
			{
				this._CustomerLocationID = value;
			}
		}
		#endregion
		#region ShipAddressID
		public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID> { }
		protected Int32? _ShipAddressID;
		[PXDBInt()]
		[PXDBDefault(typeof(SOShipment.shipAddressID), DefaultForInsert = false, DefaultForUpdate = false)]
		public virtual Int32? ShipAddressID
		{
			get
			{
				return this._ShipAddressID;
			}
			set
			{
				this._ShipAddressID = value;
			}
		}
		#endregion
		#region ShipContactID
		public abstract class shipContactID : PX.Data.BQL.BqlInt.Field<shipContactID> { }
		protected Int32? _ShipContactID;
		[PXDBInt()]
		[PXDBDefault(typeof(SOShipment.shipContactID), DefaultForInsert = false, DefaultForUpdate = false)]
		public virtual Int32? ShipContactID
		{
			get
			{
				return this._ShipContactID;
			}
			set
			{
				this._ShipContactID = value;
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
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		protected DateTime? _ShipDate;
		[PXDBDate()]
		[PXDefault(typeof(SOShipment.shipDate))]
		[PXUIField(DisplayName="Shipment Date")]
		public virtual DateTime? ShipDate
		{
			get
			{
				return this._ShipDate;
			}
			set
			{
				this._ShipDate = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr))]
		[PXDefault(typeof(SOShipment.siteID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDBInt()]
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
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
			}
		}
		#endregion
		#region Confirmed
		public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
		protected Boolean? _Confirmed;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Confirmed
		{
			get
			{
				return this._Confirmed;
			}
			set
			{
				this._Confirmed = value;
			}
		}
		#endregion
		#region ShipComplete
		public abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }
		protected String _ShipComplete;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Search<SOOrder.shipComplete, Where<SOOrder.orderType, Equal<Current<SOOrderShipment.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>))]
		public virtual String ShipComplete
		{
			get
			{
				return this._ShipComplete;
			}
			set
			{
				this._ShipComplete = value;
			}
		}
		#endregion
		#region ShipmentQty
		public abstract class shipmentQty : PX.Data.BQL.BqlDecimal.Field<shipmentQty> { }
		protected Decimal? _ShipmentQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName="Shipped Qty.", Enabled = false)]
		public virtual Decimal? ShipmentQty
		{
			get
			{
				return this._ShipmentQty;
			}
			set
			{
				this._ShipmentQty = value;
			}
		}
		#endregion
		#region ShipmentWeight
		public abstract class shipmentWeight : PX.Data.BQL.BqlDecimal.Field<shipmentWeight> { }
		protected Decimal? _ShipmentWeight;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Shipped Weight", Enabled = false)]
		public virtual Decimal? ShipmentWeight
		{
			get
			{
				return this._ShipmentWeight;
			}
			set
			{
				this._ShipmentWeight = value;
			}
		}
		#endregion
		#region ShipmentVolume
		public abstract class shipmentVolume : PX.Data.BQL.BqlDecimal.Field<shipmentVolume> { }
		protected Decimal? _ShipmentVolume;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Shipped Volume", Enabled = false)]
		public virtual Decimal? ShipmentVolume
		{
			get
			{
				return this._ShipmentVolume;
			}
			set
			{
				this._ShipmentVolume = value;
			}
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Line Total")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal
		{
			get
			{
				return this._LineTotal;
			}
			set
			{
				this._LineTotal = value;
			}
		}
		#endregion
		#region InvoiceType
		public abstract class invoiceType : PX.Data.BQL.BqlString.Field<invoiceType> { }
		protected String _InvoiceType;
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Invoice Type", Enabled = false)]
		[ARDocType.List()]
		public virtual String InvoiceType
		{
			get
			{
				return this._InvoiceType;
			}
			set
			{
				this._InvoiceType = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Invoice Nbr.", Enabled = false)]
		[PXParent(typeof(Select<SOInvoice, Where<SOInvoice.docType, Equal<Current<SOOrderShipment.invoiceType>>, And<SOInvoice.refNbr, Equal<Current<SOOrderShipment.invoiceNbr>>>>>))]
		[PXDBLiteDefault(typeof(ARInvoice.refNbr), DefaultForInsert = false, DefaultForUpdate = false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<SOInvoice.refNbr, Where<SOInvoice.docType, Equal<Current<SOOrderShipment.invoiceType>>>>), DirtyRead = true)]
		[PXUnboundFormula(typeof(Switch<Case<Where<Current2<SOOrderShipment.invoiceNbr>, IsNull, And<SOOrderShipment.createARDoc, Equal<True>>>, int1>, int0>), typeof(SumCalc<SOShipment.unbilledOrderCntr>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<Current2<SOOrderShipment.invoiceNbr>, IsNotNull, And<SOOrderShipment.invoiceReleased, Equal<False>>>, int1>, int0>), typeof(SumCalc<SOShipment.billedOrderCntr>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<Current2<SOOrderShipment.invoiceNbr>, IsNotNull, And<SOOrderShipment.invoiceReleased, Equal<False>>>, int1>, int0>), typeof(SumCalc<SOOrder.billedCntr>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<Current2<SOOrderShipment.invoiceNbr>, IsNotNull, And<SOOrderShipment.invoiceReleased, Equal<True>>>, int1>, int0>), typeof(SumCalc<SOShipment.releasedOrderCntr>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<Current2<SOOrderShipment.invoiceNbr>, IsNotNull, And<SOOrderShipment.invoiceReleased, Equal<True>>>, int1>, int0>), typeof(SumCalc<SOOrder.releasedCntr>))]
		public virtual String InvoiceNbr
		{
			get
			{
				return this._InvoiceNbr;
			}
			set
			{
				this._InvoiceNbr = value;
			}
		}
		#endregion
		#region InvoiceReleased
		public abstract class invoiceReleased : PX.Data.BQL.BqlBool.Field<invoiceReleased> { }
		protected Boolean? _InvoiceReleased;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? InvoiceReleased
		{
			get
			{
				return this._InvoiceReleased;
			}
			set
			{
				this._InvoiceReleased = value;
			}
		}
		#endregion
		#region CreateINDoc
		public abstract class createINDoc : PX.Data.BQL.BqlBool.Field<createINDoc> { }
		protected Boolean? _CreateINDoc;
		/// <summary>
		/// The flag indicates that the Order Shipment contains at least one line with a Stock Item, therefore an Inventory Document should be created.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? CreateINDoc
		{
			get
			{
				return this._CreateINDoc;
			}
			set
			{
				this._CreateINDoc = value;
			}
		}
		#endregion
		#region CreateARDoc
		public abstract class createARDoc : PX.Data.BQL.BqlBool.Field<createARDoc> { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXFormula(typeof(Switch<Case<Where<Selector<SOOrderShipment.orderType, SOOrderType.aRDocType>, NotEqual<ARDocType.noUpdate>>, True>, False>))]
		public virtual Boolean? CreateARDoc
		{
			get;
			set;
		}
		#endregion
		#region InvtDocType
		public abstract class invtDocType : PX.Data.BQL.BqlString.Field<invtDocType> { }
		protected String _InvtDocType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Inventory Doc. Type", Enabled = false)]
		[INDocType.List()]
		public virtual String InvtDocType
		{
			get
			{
				return this._InvtDocType;
			}
			set
			{
				this._InvtDocType = value;
			}
		}
		#endregion
		#region InvtRefNbr
		public abstract class invtRefNbr : PX.Data.BQL.BqlString.Field<invtRefNbr> { }
		protected String _InvtRefNbr;
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Inventory Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<SOOrderShipment.invtDocType>>>>))]
		public virtual String InvtRefNbr
		{
			get
			{
				return this._InvtRefNbr;
			}
			set
			{
				this._InvtRefNbr = value;
			}
		}
		#endregion
		#region OrderFreightAllocated
		public abstract class orderFreightAllocated : PX.Data.BQL.BqlBool.Field<orderFreightAllocated> { }
		/// <summary>
		/// The flag indicates that the Order Freight Price is fully allocated to the Invoice.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? OrderFreightAllocated
		{
			get;
			set;
		}
		#endregion
        #region HasDetailDeleted
        public abstract class hasDetailDeleted : PX.Data.BQL.BqlBool.Field<hasDetailDeleted> { }
        protected Boolean? _HasDetailDeleted;
        [PXBool()]
        public virtual Boolean? HasDetailDeleted
        {
            get
            {
                return this._HasDetailDeleted;
            }
            set
            {
                this._HasDetailDeleted = value;
            }
        }
        #endregion
		#region HasUnhandledErrors
		public abstract class hasUnhandledErrors : IBqlField
		{
		}
		/// <summary>
		/// The flag indicates that during adding the Order Shipment into the Invoice one or more errors
		/// have occured, therefore the state of the Invoice may be inconsistent.
		/// </summary>
		[PXBool]
		public virtual bool? HasUnhandledErrors
		{
			get;
			set;
		}
		#endregion
        #region ShipmentNoteID
        public abstract class shipmentNoteID : PX.Data.BQL.BqlGuid.Field<shipmentNoteID> { }
		protected Guid? _ShipmentNoteID;
		[PXDBGuid(IsImmutable = true)]
		[PXDefault(typeof(SOShipment.noteID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Guid? ShipmentNoteID
		{
			get
			{
				return this._ShipmentNoteID;
			}
			set
			{
				this._ShipmentNoteID = value;
			}
		}
		#endregion
		#region InvtNoteID
		[PXDBGuid(IsImmutable = true)]
		public virtual Guid? InvtNoteID { get; set; }
		public abstract class invtNoteID : PX.Data.BQL.BqlGuid.Field<invtNoteID> { }
		#endregion
		#region OrderNoteID
		public abstract class orderNoteID : PX.Data.BQL.BqlGuid.Field<orderNoteID> { }
		[CopiedNoteID(typeof(SOOrder))]
		public virtual Guid? OrderNoteID
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
		#region BillShipmentSeparately
		public abstract class billShipmentSeparately : PX.Data.BQL.BqlBool.Field<billShipmentSeparately>
        {
		}
		/// <summary>
		/// When set to <c>true</c> indicates that the shipment should be invoiced by a separate invoice.
		/// </summary>
		[PXBool]
		public virtual bool? BillShipmentSeparately
		{
			get;
			set;
		}
		#endregion

		#region Methods
		public static implicit operator SOOrderShipment(SOOrder item)
		{
			SOOrderShipment ret = new SOOrderShipment();
			ret.OrderType = item.OrderType;
			ret.OrderNbr = item.OrderNbr;
			ret.ShipAddressID = item.ShipAddressID;
			ret.ShipContactID = item.ShipContactID;
			ret.ShipmentType = null;
			ret.ShipmentNbr = Constants.NoShipmentNbr;
			ret.ShippingRefNoteID = null;
			ret.Operation = item.DefaultOperation;
			ret.ShipDate = item.ShipDate;
			ret.CustomerID = item.CustomerID;
			ret.CustomerLocationID = item.CustomerLocationID;
			ret.SiteID = null;
			ret.ShipmentQty = item.OrderQty;
			ret.ShipmentVolume = item.OrderVolume;
			ret.ShipmentWeight = item.OrderWeight;
			ret.LineTotal = item.LineTotal;
			//confirmed should be true for non-shipping orders
			ret.Confirmed = true;
			ret.CreateINDoc = true;
			ret.OrderNoteID = item.NoteID;

			return ret;
		}
		
		public static implicit operator SOOrderShipment(PXResult<POReceipt, SOOrder, POReceiptLine> item)
		{
			SOOrderShipment ret = new SOOrderShipment();
			ret.OrderType = ((SOOrder)item).OrderType;
			ret.OrderNbr = ((SOOrder)item).OrderNbr;
			ret.ShipAddressID = ((SOOrder)item).ShipAddressID;
			ret.ShipContactID = ((SOOrder)item).ShipContactID;
			ret.ShipmentType = INDocType.DropShip;
			ret.ShipmentNbr = ((POReceiptLine)item).ReceiptNbr;
			ret.ShippingRefNoteID = ((POReceipt)item).NoteID;
			ret.Operation = SOOperation.Issue;
			ret.ShipDate = ((POReceiptLine)item).ReceiptDate;
			ret.CustomerID = ((SOOrder)item).CustomerID;
			ret.CustomerLocationID = ((SOOrder)item).CustomerLocationID;
			ret.SiteID = null;
			ret.ShipmentWeight = ((POReceiptLine)item).ExtWeight;
			ret.ShipmentVolume = ((POReceiptLine)item).ExtVolume;
			ret.ShipmentQty = ((POReceiptLine)item).ReceiptQty;
			ret.LineTotal = 0m;
			//confirmed should be true for non-shipping orders
			ret.Confirmed = true;
			ret.CreateINDoc = true;
			ret.OrderNoteID = ((SOOrder)item).NoteID;

			return ret;
		}
		#endregion
	}

	public class SOShipmentType : INDocType
	{
		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Issue, Messages.Normal),
					Pair(DropShip, IN.Messages.DropShip),
					Pair(Transfer, IN.Messages.Transfer),
					Pair(Invoice, IN.Messages.Invoice),
				}) {}
		}

		public class ShortListAttribute : PXStringListAttribute
		{
			public ShortListAttribute() : base(
				new[]
				{
					Pair(Issue, Messages.Normal),
					Pair(Transfer, IN.Messages.Transfer),
				}) {}
		}
	}

	namespace Navigate
	{
		/// <summary>
		/// SOOrderShipment DAC for proper navigation purposes. Keys are overriden: OrderType, OrderNbr, ShipmentType, ShipmentNbr
		/// </summary>
		[PXPrimaryGraph(new Type[]
			{
				typeof(SOShipmentEntry),
				typeof(POReceiptEntry),
				typeof(SOInvoiceEntry)
			},
			new Type[]
			{
				typeof(Select2<SOShipment, 
					LeftJoin<INSite, 
						On<SOShipment.FK.Site>>, 
					Where<SOShipment.noteID, Equal<Current<SOOrderShipment.shippingRefNoteID>>, 
						And<SOShipmentType.dropShip, NotEqual<Current<SOOrderShipment.shipmentType>>, 
						And<Where<INSite.siteID, IsNull, Or<Match<INSite, Current<AccessInfo.userName>>>>>>>>),
				typeof(Select<POReceipt, Where<POReceipt.noteID, Equal<Current<SOOrderShipment.shippingRefNoteID>>, And<SOShipmentType.dropShip, Equal<Current<SOOrderShipment.shipmentType>>>>>),
				typeof(Select<ARInvoice, Where<ARInvoice.noteID, Equal<Current<SOOrderShipment.shippingRefNoteID>>, And<ARInvoice.origModule, Equal<GL.BatchModule.moduleSO>>>>),
			},
			UseParent = false)]
		[Serializable]
		[PXHidden]
		public partial class SOOrderShipment : IBqlTable
		{
			#region OrderType
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
			[PXDBString(2, IsKey = true, IsFixed = true)]
			public virtual string OrderType
			{
				get;
				set;
			}
			#endregion
			#region OrderNbr
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
			[PXDBString(15, IsKey = true, IsUnicode = true)]
			public virtual string OrderNbr
			{
				get;
				set;
			}
			#endregion
			#region ShippingRefNoteID
			public abstract class shippingRefNoteID : PX.Data.BQL.BqlGuid.Field<shippingRefNoteID> { }
			[PXDBGuid]
			public virtual Guid? ShippingRefNoteID
			{
				get;
				set;
			}
			#endregion
			#region ShipmentType
			public abstract class shipmentType : PX.Data.BQL.BqlString.Field<shipmentType> { }
			[PXDBString(1, IsKey = true, IsFixed = true)]
			public virtual string ShipmentType
			{
				get;
				set;
			}
			#endregion
			#region ShipmentNbr
			public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
			[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
			public virtual string ShipmentNbr
			{
				get;
				set;
			}
			#endregion
		}
	}
}
