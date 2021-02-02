using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.CA;
using PX.Objects.PM;
using System.Diagnostics;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.SO.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.Common.Attributes;

namespace PX.Objects.SO
{
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(SOOrderEntry))]
	[PXEMailSource]
	[PXCacheName(Messages.SOOrder)]
	[DebuggerDisplay("OrderType = {OrderType}, OrderNbr = {OrderNbr}")]
    public partial class SOOrder : PX.Data.IBqlTable, PX.Data.EP.IAssign, IFreightBase, ICCAuthorizePayment, ICCCapturePayment, IInvoice
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOOrder>.By<orderType, orderNbr>
		{
			public static SOOrder Find(PXGraph graph, string orderType, string orderNbr) => FindBy(graph, orderType, orderNbr);
		}
		public static class FK
		{
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOOrder>.By<orderType> { }
			public class BillAddress : SOBillingAddress.PK.ForeignKeyOf<SOOrder>.By<billAddressID> { }
			public class ShipAddress : SOShippingAddress.PK.ForeignKeyOf<SOOrder>.By<shipAddressID> { }
			public class BillContact : SOBillingContact.PK.ForeignKeyOf<SOOrder>.By<billContactID> { }
			public class ShipContact : SOShippingContact.PK.ForeignKeyOf<SOOrder>.By<shipContactID> { }
			public class FreightTaxCategory: TaxCategory.PK.ForeignKeyOf<SOOrder>.By<freightTaxCategoryID> { }
			public class FOBPoint : CS.FOBPoint.PK.ForeignKeyOf<SOOrder>.By<fOBPoint> { }
			public class Invoice : SOInvoice.PK.ForeignKeyOf<SOOrder>.By<aRDocType, invoiceNbr> { }
			public class ShipTerms : CS.ShipTerms.PK.ForeignKeyOf<SOOrder>.By<shipTermsID> { }
			public class ShippingZone : CS.ShippingZone.PK.ForeignKeyOf<SOOrder>.By<shipZoneID> { }
			public class Carrier : CS.Carrier.PK.ForeignKeyOf<SOOrder>.By<shipVia> { }
			public class DefaultSite : INSite.PK.ForeignKeyOf<SOOrder>.By<defaultSiteID> { }
			public class DestinationSite : INSite.PK.ForeignKeyOf<SOOrder>.By<destinationSiteID> { }
			public class OrigOrderType : SOOrderType.PK.ForeignKeyOf<SOOrder>.By<origOrderType> { }
			public class OrigOrder : SOOrder.PK.ForeignKeyOf<SOOrder>.By<origOrderType, origOrderNbr> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool()]
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
		[PXDBString(2, IsKey = true, IsFixed = true, InputMask=">aa")]
		[PXDefault(SOOrderTypeConstants.SalesOrder, typeof(SOSetup.defaultOrderType))]
        [PXSelector(typeof(Search5<SOOrderType.orderType,
            InnerJoin<SOOrderTypeOperation, On2<SOOrderTypeOperation.FK.OrderType, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>,
            LeftJoin<SOSetupApproval, On<SOOrderType.orderType, Equal<SOSetupApproval.orderType>>>>,
            Aggregate<GroupBy<SOOrderType.orderType>>>))]
        [PXRestrictor(typeof(Where<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, Or<FeatureInstalled<FeaturesSet.warehouse>>>), ErrorMessages.ElementDoesntExist, typeof(SOOrderType.orderType))]
        [PXRestrictor(typeof(Where<SOOrderType.requireAllocation, NotEqual<True>, Or<AllocationAllowed>>), ErrorMessages.ElementDoesntExist, typeof(SOOrderType.orderType))]
        [PXRestrictor(typeof(Where<SOOrderType.active,Equal<True>>), null)]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
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
		#region Behavior
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		protected String _Behavior;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXDefault(typeof(Search<SOOrderType.behavior, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
		[PXUIField(DisplayName = "Behavior", Enabled = false, IsReadOnly = true)]
		[SOBehavior.List()]
		public virtual String Behavior
		{
			get
			{
				return this._Behavior;
			}
			set
			{
				this._Behavior = value;
			}
		}
		#endregion
		#region ARDocType
		public abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }
		protected String _ARDocType;
		[PXString(ARRegister.docType.Length, IsFixed = true)]
		[PXFormula(typeof(Selector<SOOrder.orderType, SOOrderType.aRDocType>))]
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
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[SO.RefNbr(typeof(Search2<SOOrder.orderNbr,
			LeftJoinSingleTable<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>,
				And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>,
			Where<SOOrder.orderType, Equal<Optional<SOOrder.orderType>>,
				And<Where<Customer.bAccountID, IsNotNull,
					Or<Exists<Select<SOOrderType,
						Where<SOOrderType.orderType, Equal<SOOrder.orderType>,
							And<SOOrderType.aRDocType, Equal<ARDocType.noUpdate>,
							And<SOOrderType.behavior, Equal<SOBehavior.sO>>>>>>>>>>,
			 OrderBy<Desc<SOOrder.orderNbr>>>), Filterable = true)]
		[SO.Numbering()]
		[PX.Data.EP.PXFieldDescription]
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
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[PXDefault]
        [CustomerActive(
			typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
			Visibility = PXUIVisibility.SelectorVisible, 
			DescriptionField = typeof(Customer.acctName), 
			Filterable = true)]
		[CustomerOrOrganizationInNoUpdateDocRestrictor]
		[PXForeignReference(typeof(Field<SOOrder.customerID>.IsRelatedTo<BAccount.bAccountID>))]

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
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<Location.isActive, Equal<True>,
			And<MatchWithBranch<Location.cBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
			Where<BAccountR.bAccountID, Equal<Current<SOOrder.customerID>>,
				And<CRLocation.isActive, Equal<True>,
				And<MatchWithBranch<CRLocation.cBranchID>>>>>,
			Search<CRLocation.locationID,
			Where<CRLocation.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.cBranchID>>>>>>))]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<SOOrder.customerID>.IsRelatedTo<Location.bAccountID>,
				Field<SOOrder.customerLocationID>.IsRelatedTo<Location.locationID>
			>))]
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
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(typeof(Coalesce<
			Search<Location.cBranchID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>,
			Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false)]
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
		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? OrderDate
		{
			get
			{
				return this._OrderDate;
			}
			set
			{
				this._OrderDate = value;
			}
		}
		#endregion
		#region CustomerOrderNbr
		public abstract class customerOrderNbr : PX.Data.BQL.BqlString.Field<customerOrderNbr> { }
		protected String _CustomerOrderNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[CustomerOrderNbr]
		public virtual String CustomerOrderNbr
		{
			get
			{
				return this._CustomerOrderNbr;
			}
			set
			{
				this._CustomerOrderNbr = value;
			}
		}
		#endregion
		#region CustomerRefNbr
		public abstract class customerRefNbr : PX.Data.BQL.BqlString.Field<customerRefNbr> { }
		protected String _CustomerRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "External Reference")]
		public virtual String CustomerRefNbr
		{
			get
			{
				return this._CustomerRefNbr;
			}
			set
			{
				this._CustomerRefNbr = value;
			}
		}
		#endregion
		#region CancelDate
		public abstract class cancelDate : PX.Data.BQL.BqlDateTime.Field<cancelDate> { }
		protected DateTime? _CancelDate;
		[PXDBDate()]
		[PXFormula(typeof(Switch<Case<Where<MaxDate, Less<Add<SOOrder.orderDate, Selector<SOOrder.orderType, SOOrderType.daysToKeep>>>>, MaxDate>, Add<SOOrder.orderDate, Selector<SOOrder.orderType, SOOrderType.daysToKeep>>>))]
		[PXUIField(DisplayName = "Cancel By", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? CancelDate
		{
			get
			{
				return this._CancelDate;
			}
			set
			{
				this._CancelDate = value;
			}
		}
		#endregion
		#region RequestDate
		public abstract class requestDate : PX.Data.BQL.BqlDateTime.Field<requestDate> { }
		protected DateTime? _RequestDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Requested On", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? RequestDate
		{
			get
			{
				return this._RequestDate;
			}
			set
			{
				this._RequestDate = value;
			}
		}
		#endregion
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		protected DateTime? _ShipDate;
		[PXDBDate()]
		[PXFormula(typeof(DateMinusDaysNotLessThenDate<SOOrder.requestDate, IsNull<Selector<Current<SOOrder.customerLocationID>, Location.cLeadTime>, decimal0>, SOOrder.orderDate>))]
		[PXUIField(DisplayName = "Sched. Shipment", Visibility = PXUIVisibility.SelectorVisible)]
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
        #region DontApprove
        public abstract class dontApprove : PX.Data.BQL.BqlBool.Field<dontApprove> { }
        protected Boolean? _DontApprove;
        [PXBool()]
        [PXFormula(typeof(Switch<Case<Where<Current<SOSetup.orderRequestApproval>, Equal<True>>, Selector<SOOrder.orderType, SOSetupApproval.nonExistence>>, True>))]
        [PXUIField(DisplayName = "Don't Approve", Visible = false, Enabled = false)]
        public virtual Boolean? DontApprove
        {
            get
            {
                return this._DontApprove;
            }
            set
            {
                this._DontApprove = value;
            }
        }
        #endregion

        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;
        [PXDBBool()]
        [PXDefault(false, typeof(Search<SOOrderType.holdEntry, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
        [PXUIField(DisplayName = "Hold")]
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
        #region Approved
        public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
        protected Boolean? _Approved;
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Approved", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Boolean? Approved
        {
            get
            {
                return this._Approved;
            }
            set
            {
                this._Approved = value;
            }
        }
        #endregion
        #region Rejected
        public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
        protected bool? _Rejected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? Rejected
        {
            get
            {
                return _Rejected;
            }
            set
            {
                _Rejected = value;
            }
        }
		#endregion

		#region Emailed
		public abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }

		/// <summary>
		/// Indicates whether the document has been emailed to the <see cref="CustomerID">Customer</see>.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Emailed")]
		public virtual Boolean? Emailed
		{
			get; set;
		}
		#endregion

		#region CreditHold
		public abstract class creditHold : PX.Data.BQL.BqlBool.Field<creditHold> { }
		protected Boolean? _CreditHold;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Credit Hold")]
		public virtual Boolean? CreditHold
		{
			get
			{
				return this._CreditHold;
			}
			set
			{
				this._CreditHold = value;
			}
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		protected Boolean? _Completed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Completed")]
		public virtual Boolean? Completed
		{
			get
			{
				return this._Completed;
			}
			set
			{
				this._Completed = value;
			}
		}
		#endregion
		#region Cancelled
		public abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }
		protected Boolean? _Cancelled;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Canceled")]
		public virtual Boolean? Cancelled
		{
			get
			{
				return this._Cancelled;
			}
			set
			{
				this._Cancelled = value;
			}
		}
		#endregion
		#region OpenDoc
		public abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		protected Boolean? _OpenDoc;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? OpenDoc
		{
			get
			{
				return this._OpenDoc;
			}
			set
			{
				this._OpenDoc = value;
			}
		}
		#endregion
		#region ShipmentDeleted
		public abstract class shipmentDeleted : PX.Data.BQL.BqlBool.Field<shipmentDeleted> { }
		protected Boolean? _ShipmentDeleted;
		[PXBool()]
		public virtual Boolean? ShipmentDeleted
		{
			get
			{
				return this._ShipmentDeleted;
			}
			set
			{
				this._ShipmentDeleted = value;
			}
		}
		#endregion
		#region BackOrdered
		public abstract class backOrdered : PX.Data.BQL.BqlBool.Field<backOrdered> { }
		protected Boolean? _BackOrdered;
		[PXBool()]
		[PXUIField(DisplayName = "BackOrdered")]
		public virtual Boolean? BackOrdered
		{
			get
			{
				return this._BackOrdered;
			}
			set
			{
				this._BackOrdered = value;
			}
		}
		#endregion
		#region LastSiteID
		public abstract class lastSiteID : PX.Data.BQL.BqlInt.Field<lastSiteID> { }
		protected Int32? _LastSiteID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Last Shipment Site")]
		public virtual Int32? LastSiteID
		{
			get
			{
				return this._LastSiteID;
			}
			set
			{
				this._LastSiteID = value;
			}
		}
		#endregion
		#region LastShipDate
		public abstract class lastShipDate : PX.Data.BQL.BqlDateTime.Field<lastShipDate> { }
		protected DateTime? _LastShipDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Shipment Date")]
		public virtual DateTime? LastShipDate
		{
			get
			{
				return this._LastShipDate;
			}
			set
			{
				this._LastShipDate = value;
			}
		}
		#endregion
		#region BillSeparately
		public abstract class billSeparately : PX.Data.BQL.BqlBool.Field<billSeparately> { }
		protected Boolean? _BillSeparately;
		[PXDBBool()]
		[PXDefault(typeof(Search<SOOrderType.billSeparately, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
		[PXUIField(DisplayName = "Bill Separately")]
		public virtual Boolean? BillSeparately
		{
			get
			{
				return this._BillSeparately;
			}
			set
			{
				this._BillSeparately = value;
			}
		}
		#endregion
		#region ShipSeparately
		public abstract class shipSeparately : PX.Data.BQL.BqlBool.Field<shipSeparately> { }
		protected Boolean? _ShipSeparately;
		[PXDBBool()]
		[PXDefault(typeof(Search<SOOrderType.shipSeparately,Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
		[PXUIField(DisplayName = "Ship Separately")]
		public virtual Boolean? ShipSeparately
		{
			get
			{
				return this._ShipSeparately;
			}
			set
			{
				this._ShipSeparately = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected string _Status;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[SOOrderStatus.List()]
		[PXDefault()]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.SO, "{0}: {1} - {3}", new Type[] { typeof(SOOrder.orderType), typeof(SOOrder.orderNbr), typeof(SOOrder.customerID), typeof(Customer.acctName) },
		   new Type[] { typeof(SOOrder.customerRefNbr), typeof(SOOrder.customerOrderNbr), typeof(SOOrder.orderDesc) },
		   NumberFields = new Type[] { typeof(SOOrder.orderNbr) },
		   Line1Format = "{0:d}{1}{2}{3}", Line1Fields = new Type[] { typeof(SOOrder.orderDate), typeof(SOOrder.status), typeof(SOOrder.customerRefNbr), typeof(SOOrder.customerOrderNbr) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(SOOrder.orderDesc) },
		   MatchWithJoin = typeof(InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<SOOrder.customerID>>>),
		   SelectForFastIndexing = typeof(Select2<SOOrder, InnerJoin<Customer, On<SOOrder.customerID, Equal<Customer.bAccountID>>>>)
	   )]
		[PXNote(new Type[0], ShowInReferenceSelector = true, Selector = typeof(
			Search2<
				SOOrder.orderNbr,
			LeftJoinSingleTable<Customer,
				On<SOOrder.customerID, Equal<Customer.bAccountID>,
				And<Where<Match<Customer, Current<AccessInfo.userName>>>>>>,
			Where<
				Customer.bAccountID, IsNotNull,
				Or<Exists<
					Select<
						SOOrderType,
					Where<
						SOOrderType.orderType, Equal<SOOrder.orderType>,
						And<SOOrderType.aRDocType, Equal<ARDocType.noUpdate>>>>>>>,
			OrderBy<
				Desc<SOOrder.orderNbr>>>))]
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
		#region BilledCntr
		public abstract class billedCntr : PX.Data.BQL.BqlInt.Field<billedCntr> { }
		protected Int32? _BilledCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? BilledCntr
		{
			get
			{
				return this._BilledCntr;
			}
			set
			{
				this._BilledCntr = value;
			}
		}
		#endregion
		#region ReleasedCntr
		public abstract class releasedCntr : PX.Data.BQL.BqlInt.Field<releasedCntr> { }
		protected Int32? _ReleasedCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? ReleasedCntr
		{
			get
			{
				return this._ReleasedCntr;
			}
			set
			{
				this._ReleasedCntr = value;
			}
		}
		#endregion
		#region PaymentCntr
		public abstract class paymentCntr : PX.Data.BQL.BqlInt.Field<paymentCntr> { }
		protected Int32? _PaymentCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? PaymentCntr
		{
			get
			{
				return this._PaymentCntr;
			}
			set
			{
				this._PaymentCntr = value;
			}
		}
		#endregion
		#region OrderDesc
		public abstract class orderDesc : PX.Data.BQL.BqlString.Field<orderDesc> { }
		protected String _OrderDesc;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String OrderDesc
		{
			get
			{
				return this._OrderDesc;
			}
			set
			{
				this._OrderDesc = value;
			}
		}
		#endregion
		#region BillAddressID
		public abstract class billAddressID : PX.Data.BQL.BqlInt.Field<billAddressID> { }
		protected Int32? _BillAddressID;
		[PXDBInt()]
		[SOBillingAddress(typeof(Select2<BAccountR, 
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, 
			LeftJoin<Customer, On<Customer.bAccountID, Equal<BAccountR.bAccountID>>,
			InnerJoin<Address, On<Address.bAccountID, Equal<BAccountR.bAccountID>, 
			                  And<Where2<Where<Customer.bAccountID, IsNotNull, 
												             And<Address.addressID, Equal<Customer.defBillAddressID>>>,
																Or<Where<Customer.bAccountID, IsNull, 
																		 And<Address.addressID, Equal<BAccountR.defAddressID>>>>>>>,			
			LeftJoin<SOBillingAddress, On<SOBillingAddress.customerID, Equal<Address.bAccountID>, And<SOBillingAddress.customerAddressID, Equal<Address.addressID>, And<SOBillingAddress.revisionID, Equal<Address.revisionID>, And<SOBillingAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>>, 
			Where<BAccountR.bAccountID, Equal<Current<SOOrder.customerID>>>>))]
		public virtual Int32? BillAddressID
		{
			get
			{
				return this._BillAddressID;
			}
			set
			{
				this._BillAddressID = value;
			}
		}
		#endregion
		#region ShipAddressID
		public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID> { }
		protected Int32? _ShipAddressID;
		[PXDBInt()]
        [SOShippingAddress(typeof(
                    Select2<Address,
                        InnerJoin<CRLocation,
				  On<CRLocation.bAccountID, Equal<Address.bAccountID>,
				 And<Address.addressID, Equal<CRLocation.defAddressID>,
					 And<CRLocation.bAccountID, Equal<Current<SOOrder.customerID>>,
                            And<CRLocation.locationID, Equal<Current<SOOrder.customerLocationID>>>>>>,
                        LeftJoin<SOShippingAddress,
                            On<SOShippingAddress.customerID, Equal<Address.bAccountID>,
                            And<SOShippingAddress.customerAddressID, Equal<Address.addressID>,
                            And<SOShippingAddress.revisionID, Equal<Address.revisionID>,
                            And<SOShippingAddress.isDefaultAddress, Equal<True>>>>>>>,
                        Where<True, Equal<True>>>))]
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
		#region BillContactID
		public abstract class billContactID : PX.Data.BQL.BqlInt.Field<billContactID> { }
		protected Int32? _BillContactID;
		[PXDBInt()]
		[SOBillingContact(typeof(Select2<BAccountR,
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>,
			LeftJoin<Customer, On<Customer.bAccountID, Equal<BAccountR.bAccountID>>,
			InnerJoin<Contact, On<Contact.bAccountID, Equal<BAccountR.bAccountID>, 
			                  And<Where2<Where<Customer.bAccountID, IsNotNull, 
												             And<Contact.contactID, Equal<Customer.defBillContactID>>>,
																Or<Where<Customer.bAccountID, IsNull, 
																		 And<Contact.contactID, Equal<BAccountR.defContactID>>>>>>>,
			LeftJoin<SOBillingContact, On<SOBillingContact.customerID, Equal<Contact.bAccountID>, And<SOBillingContact.customerContactID, Equal<Contact.contactID>, And<SOBillingContact.revisionID, Equal<Contact.revisionID>, And<SOBillingContact.isDefaultContact, Equal<boolTrue>>>>>>>>>,
			Where<BAccountR.bAccountID, Equal<Current<SOOrder.customerID>>>>))]
		public virtual Int32? BillContactID
		{
			get
			{
				return this._BillContactID;
			}
			set
			{
				this._BillContactID = value;
			}
		}
		#endregion
		#region ShipContactID
		public abstract class shipContactID : PX.Data.BQL.BqlInt.Field<shipContactID> { }
		protected Int32? _ShipContactID;
		[PXDBInt()]
		[SOShippingContact(typeof(Select2<Contact,
                    InnerJoin<CRLocation,
				  On<CRLocation.bAccountID, Equal<Contact.bAccountID>,
				 And<Contact.contactID, Equal<CRLocation.defContactID>,
					 And<CRLocation.bAccountID, Equal<Current<SOOrder.customerID>>,
                        And<CRLocation.locationID, Equal<Current<SOOrder.customerLocationID>>>>>>,
                    LeftJoin<SOShippingContact,
                        On<SOShippingContact.customerID, Equal<Contact.bAccountID>,
			     And<SOShippingContact.customerContactID, Equal<Contact.contactID>, 
					 And<SOShippingContact.revisionID, Equal<Contact.revisionID>, 
                        And<SOShippingContact.isDefaultContact, Equal<True>>>>>>>
                    , Where<True, Equal<True>>>))]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo()]
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
        #region DiscTot
        public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }
        protected Decimal? _DiscTot;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Discount Total")]
		public virtual Decimal? DiscTot
        {
            get
            {
                return this._DiscTot;
            }
            set
            {
                this._DiscTot = value;
            }
        }
        #endregion
        #region CuryDiscTot
        public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }
        protected Decimal? _CuryDiscTot;
        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.discTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount Total")]
        public virtual Decimal? CuryDiscTot
        {
            get
            {
                return this._CuryDiscTot;
            }
            set
            {
                this._CuryDiscTot = value;
            }
        }
        #endregion
        #region DocDisc
        public abstract class docDisc : PX.Data.BQL.BqlDecimal.Field<docDisc> { }
        protected Decimal? _DocDisc;
        [PXBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? DocDisc
        {
            get
            {
                return this._DocDisc;
            }
            set
            {
                this._DocDisc = value;
            }
        }
        #endregion
        #region CuryDocDisc
        public abstract class curyDocDisc : PX.Data.BQL.BqlDecimal.Field<curyDocDisc> { }
        protected Decimal? _CuryDocDisc;
        [PXCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.docDisc))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Document Discount", Enabled = true)]
        public virtual Decimal? CuryDocDisc
        {
            get
            {
                return this._CuryDocDisc;
            }
            set
            {
                this._CuryDocDisc = value;
            }
        }
        #endregion
        #region CuryOrderTotal
		public abstract class curyOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyOrderTotal> { }
		protected Decimal? _CuryOrderTotal;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.orderTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Total")]
		public virtual Decimal? CuryOrderTotal
		{
			get
			{
				return this._CuryOrderTotal;
			}
			set
			{
				this._CuryOrderTotal = value;
			}
		}
		#endregion
		#region OrderTotal
		public abstract class orderTotal : PX.Data.BQL.BqlDecimal.Field<orderTotal> { }
		protected Decimal? _OrderTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? OrderTotal
		{
			get
			{
				return this._OrderTotal;
			}
			set
			{
				this._OrderTotal = value;
			}
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		protected Decimal? _CuryLineTotal;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.lineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Total")]
		public virtual Decimal? CuryLineTotal
		{
			get
			{
				return this._CuryLineTotal;
			}
			set
			{
				this._CuryLineTotal = value;
			}
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		protected Decimal? _LineTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
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


        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
        protected Decimal? _CuryVatExemptTotal;
        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.vatExemptTotal))]
        [PXUIField(DisplayName = "VAT Exempt Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatExemptTotal
        {
            get
            {
                return this._CuryVatExemptTotal;
            }
            set
            {
                this._CuryVatExemptTotal = value;
            }
        }
        #endregion

        #region VatExemptTaxTotal
        public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }
        protected Decimal? _VatExemptTotal;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatExemptTotal
        {
            get
            {
                return this._VatExemptTotal;
            }
            set
            {
                this._VatExemptTotal = value;
            }
        }
        #endregion
        
        #region CuryVatTaxableTotal
        public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }
        protected Decimal? _CuryVatTaxableTotal;
        [PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.vatTaxableTotal))]
        [PXUIField(DisplayName = "VAT Taxable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryVatTaxableTotal
        {
            get
            {
                return this._CuryVatTaxableTotal;
            }
            set
            {
                this._CuryVatTaxableTotal = value;
            }
        }
        #endregion

        #region VatTaxableTotal
        public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }
        protected Decimal? _VatTaxableTotal;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? VatTaxableTotal
        {
            get
            {
                return this._VatTaxableTotal;
            }
            set
            {
                this._VatTaxableTotal = value;
            }
        }
        #endregion

		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		protected Decimal? _CuryTaxTotal;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.taxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Total")]
		public virtual Decimal? CuryTaxTotal
		{
			get
			{
				return this._CuryTaxTotal;
			}
			set
			{
				this._CuryTaxTotal = value;
			}
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }
		protected Decimal? _TaxTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? TaxTotal
		{
			get
			{
				return this._TaxTotal;
			}
			set
			{
				this._TaxTotal = value;
			}
		}
		#endregion
		#region CuryPremiumFreightAmt
		public abstract class curyPremiumFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyPremiumFreightAmt> { }
		protected Decimal? _CuryPremiumFreightAmt;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.premiumFreightAmt))]
		[PXUIField(DisplayName = "Premium Freight Price")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryPremiumFreightAmt
		{
			get
			{
				return this._CuryPremiumFreightAmt;
			}
			set
			{
				this._CuryPremiumFreightAmt = value;
			}
		}
		#endregion
		#region PremiumFreightAmt
		public abstract class premiumFreightAmt : PX.Data.BQL.BqlDecimal.Field<premiumFreightAmt> { }
		protected Decimal? _PremiumFreightAmt;
		[PXDBDecimal(4)]
		public virtual Decimal? PremiumFreightAmt
		{
			get
			{
				return this._PremiumFreightAmt;
			}
			set
			{
				this._PremiumFreightAmt = value;
			}
		}
		#endregion		
		#region CuryFreightCost
		public abstract class curyFreightCost : PX.Data.BQL.BqlDecimal.Field<curyFreightCost> { }
		protected Decimal? _CuryFreightCost;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.freightCost))]
		[PXUIField(DisplayName = "Freight Cost", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFreightCost
		{
			get
			{
				return this._CuryFreightCost;
			}
			set
			{
				this._CuryFreightCost = value;
			}
		}
		#endregion
		#region FreightCost
		public abstract class freightCost : PX.Data.BQL.BqlDecimal.Field<freightCost> { }
		protected Decimal? _FreightCost;
		[PXDBDecimal(4)]
		public virtual Decimal? FreightCost
		{
			get
			{
				return this._FreightCost;
			}
			set
			{
				this._FreightCost = value;
			}
		}
		#endregion
		#region FreightCostIsValid
		public abstract class freightCostIsValid : PX.Data.BQL.BqlBool.Field<freightCostIsValid> { }
		protected Boolean? _FreightCostIsValid;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Freight Cost Is up-to-date", Enabled=false)]
		public virtual Boolean? FreightCostIsValid
		{
			get
			{
				return this._FreightCostIsValid;
			}
			set
			{
				this._FreightCostIsValid = value;
			}
		}
		#endregion
		#region IsPackageValid
		public abstract class isPackageValid : PX.Data.BQL.BqlBool.Field<isPackageValid> { }
		protected Boolean? _IsPackageValid;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsPackageValid
		{
			get
			{
				return this._IsPackageValid;
			}
			set
			{
				this._IsPackageValid = value;
			}
		}
		#endregion
		#region OverrideFreightAmount
		public abstract class overrideFreightAmount : PX.Data.BQL.BqlBool.Field<overrideFreightAmount> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Freight Price")]
		public virtual bool? OverrideFreightAmount
		{
			get;
			set;
		}
		#endregion
		#region FreightAmountSource
		public abstract class freightAmountSource : PX.Data.BQL.BqlString.Field<freightAmountSource> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[FreightAmountSource]
		[PXUIField(DisplayName = "Invoice Freight Price Based On", Enabled = false)]
		[PXFormula(typeof(Switch<Case<Where<SOOrder.overrideFreightAmount, Equal<True>>, FreightAmountSourceAttribute.orderBased>,
			IsNull<Selector<SOOrder.shipTermsID, ShipTerms.freightAmountSource>, FreightAmountSourceAttribute.shipmentBased>>))]
		public virtual string FreightAmountSource
		{
			get;
			set;
		}
		#endregion
		#region CuryFreightAmt
		public abstract class curyFreightAmt : PX.Data.BQL.BqlDecimal.Field<curyFreightAmt> { }
		protected Decimal? _CuryFreightAmt;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.freightAmt))]
		[PXUIField(DisplayName = "Freight Price", Enabled = false)]
		[PXUIVerify(typeof(Where<SOOrder.curyFreightAmt, GreaterEqual<decimal0>>), PXErrorLevel.Error, CS.Messages.Entry_GE, typeof(decimal0))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFreightAmt
		{
			get
			{
				return this._CuryFreightAmt;
			}
			set
			{
				this._CuryFreightAmt = value;
			}
		}
		#endregion
		#region FreightAmt
		public abstract class freightAmt : PX.Data.BQL.BqlDecimal.Field<freightAmt> { }
		protected Decimal? _FreightAmt;
		[PXDBDecimal(4)]
		public virtual Decimal? FreightAmt
		{
			get
			{
				return this._FreightAmt;
			}
			set
			{
				this._FreightAmt = value;
			}
		}
		#endregion
		#region CuryFreightTot
		public abstract class curyFreightTot : PX.Data.BQL.BqlDecimal.Field<curyFreightTot> { }
		protected Decimal? _CuryFreightTot;
		[PXDBCurrency(typeof(SOOrder.curyInfoID),typeof(SOOrder.freightTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Add<SOOrder.curyPremiumFreightAmt, SOOrder.curyFreightAmt>))]
		[PXUIField(DisplayName = "Freight Total")]
		public virtual Decimal? CuryFreightTot
		{
			get
			{
				return this._CuryFreightTot;
			}
			set
			{
				this._CuryFreightTot = value;
			}
		}
		#endregion
		#region FreightTot
		public abstract class freightTot : PX.Data.BQL.BqlDecimal.Field<freightTot> { }
		protected Decimal? _FreightTot;
		[PXDBDecimal(4)]
		public virtual Decimal? FreightTot
		{
			get
			{
				return this._FreightTot;
			}
			set
			{
				this._FreightTot = value;
			}
		}
		#endregion
		#region FreightTaxCategoryID
		public abstract class freightTaxCategoryID : PX.Data.BQL.BqlString.Field<freightTaxCategoryID> { }
		protected String _FreightTaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Freight Tax Category", Visibility = PXUIVisibility.Visible)]
		[SOOrderTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), typeof(taxCalcMode), TaxCalc = TaxCalc.ManualLineCalc)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Search<Carrier.taxCategoryID, Where<Carrier.carrierID, Equal<Current<SOOrder.shipVia>>>>), SearchOnDefault = false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String FreightTaxCategoryID
		{
			get
			{
				return this._FreightTaxCategoryID;
			}
			set
			{
				this._FreightTaxCategoryID = value;
			}
		}
		#endregion
		#region CuryMiscTot
		public abstract class curyMiscTot : PX.Data.BQL.BqlDecimal.Field<curyMiscTot> { }
		protected Decimal? _CuryMiscTot;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.miscTot))]
		[PXUIField(DisplayName = "Misc. Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryMiscTot
		{
			get
			{
				return this._CuryMiscTot;
			}
			set
			{
				this._CuryMiscTot = value;
			}
		}
		#endregion
		#region MiscTot
		public abstract class miscTot : PX.Data.BQL.BqlDecimal.Field<miscTot> { }
		protected Decimal? _MiscTot;
		[PXDBDecimal(4)]
		public virtual Decimal? MiscTot
		{
			get
			{
				return this._MiscTot;
			}
			set
			{
				this._MiscTot = value;
			}
		}
		#endregion
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBQuantity()]
		[PXUIField(DisplayName="Ordered Qty.")]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? OrderQty
		{
			get
			{
				return this._OrderQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion
		#region OrderWeight
		public abstract class orderWeight : PX.Data.BQL.BqlDecimal.Field<orderWeight> { }
		protected Decimal? _OrderWeight;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Weight", Enabled = false)]
		public virtual Decimal? OrderWeight
		{
			get
			{
				return this._OrderWeight;
			}
			set
			{
				this._OrderWeight = value;
			}
		}
		#endregion
		#region OrderVolume
		public abstract class orderVolume : PX.Data.BQL.BqlDecimal.Field<orderVolume> { }
		protected Decimal? _OrderVolume;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Volume", Enabled = false)]
		public virtual Decimal? OrderVolume
		{
			get
			{
				return this._OrderVolume;
			}
			set
			{
				this._OrderVolume = value;
			}
		}
		#endregion
		#region CuryOpenOrderTotal
		public abstract class curyOpenOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyOpenOrderTotal> { }
		protected Decimal? _CuryOpenOrderTotal;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.openOrderTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Amount", Enabled = false)]
		public virtual Decimal? CuryOpenOrderTotal
		{
			get
			{
				return this._CuryOpenOrderTotal;
			}
			set
			{
				this._CuryOpenOrderTotal = value;
			}
		}
		#endregion
		#region OpenOrderTotal
		public abstract class openOrderTotal : PX.Data.BQL.BqlDecimal.Field<openOrderTotal> { }
		protected Decimal? _OpenOrderTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenOrderTotal
		{
			get
			{
				return this._OpenOrderTotal;
			}
			set
			{
				this._OpenOrderTotal = value;
			}
		}
		#endregion
		#region CuryOpenLineTotal
		public abstract class curyOpenLineTotal : PX.Data.BQL.BqlDecimal.Field<curyOpenLineTotal> { }
		protected Decimal? _CuryOpenLineTotal;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.openLineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Line Total")]
		public virtual Decimal? CuryOpenLineTotal
		{
			get
			{
				return this._CuryOpenLineTotal;
			}
			set
			{
				this._CuryOpenLineTotal = value;
			}
		}
		#endregion
		#region OpenLineTotal
		public abstract class openLineTotal : PX.Data.BQL.BqlDecimal.Field<openLineTotal> { }
		protected Decimal? _OpenLineTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenLineTotal
		{
			get
			{
				return this._OpenLineTotal;
			}
			set
			{
				this._OpenLineTotal = value;
			}
		}
		#endregion
		#region CuryOpenDiscTotal
		public abstract class curyOpenDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyOpenDiscTotal> { }

		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.openDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryOpenDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region OpenDiscTotal
		public abstract class openDiscTotal : PX.Data.BQL.BqlDecimal.Field<openDiscTotal> { }

		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenDiscTotal 
		{
			get;
			set;
		}
		#endregion
		#region CuryOpenTaxTotal
		public abstract class curyOpenTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyOpenTaxTotal> { }
		protected Decimal? _CuryOpenTaxTotal;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.openTaxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unshipped Tax Total")]
		public virtual Decimal? CuryOpenTaxTotal
		{
			get
			{
				return this._CuryOpenTaxTotal;
			}
			set
			{
				this._CuryOpenTaxTotal = value;
			}
		}
		#endregion
		#region OpenTaxTotal
		public abstract class openTaxTotal : PX.Data.BQL.BqlDecimal.Field<openTaxTotal> { }
		protected Decimal? _OpenTaxTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenTaxTotal
		{
			get
			{
				return this._OpenTaxTotal;
			}
			set
			{
				this._OpenTaxTotal = value;
			}
		}
		#endregion
		#region OpenOrderQty
		public abstract class openOrderQty : PX.Data.BQL.BqlDecimal.Field<openOrderQty> { }
		protected Decimal? _OpenOrderQty;
		[PXDBQuantity()]
		[PXUIField(DisplayName = "Unshipped Quantity")]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region CuryUnbilledOrderTotal
		public abstract class curyUnbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledOrderTotal> { }
		protected Decimal? _CuryUnbilledOrderTotal;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unbilledOrderTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Amount", Enabled = false)]
		public virtual Decimal? CuryUnbilledOrderTotal
		{
			get
			{
				return this._CuryUnbilledOrderTotal;
			}
			set
			{
				this._CuryUnbilledOrderTotal = value;
			}
		}
		#endregion
		#region UnbilledOrderTotal
		public abstract class unbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<unbilledOrderTotal> { }
		protected Decimal? _UnbilledOrderTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledOrderTotal
		{
			get
			{
				return this._UnbilledOrderTotal;
			}
			set
			{
				this._UnbilledOrderTotal = value;
			}
		}
		#endregion
		#region CuryUnbilledLineTotal
		public abstract class curyUnbilledLineTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledLineTotal> { }
		protected Decimal? _CuryUnbilledLineTotal;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unbilledLineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Line Total")]
		public virtual Decimal? CuryUnbilledLineTotal
		{
			get
			{
				return this._CuryUnbilledLineTotal;
			}
			set
			{
				this._CuryUnbilledLineTotal = value;
			}
		}
		#endregion
		#region UnbilledLineTotal
		public abstract class unbilledLineTotal : PX.Data.BQL.BqlDecimal.Field<unbilledLineTotal> { }
		protected Decimal? _UnbilledLineTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledLineTotal
		{
			get
			{
				return this._UnbilledLineTotal;
			}
			set
			{
				this._UnbilledLineTotal = value;
			}
		}
		#endregion
		#region CuryUnbilledMiscTot
		public abstract class curyUnbilledMiscTot : PX.Data.BQL.BqlDecimal.Field<curyUnbilledMiscTot> { }
		protected Decimal? _CuryUnbilledMiscTot;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unbilledMiscTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Misc. Total")]
		public virtual Decimal? CuryUnbilledMiscTot
		{
			get
			{
				return this._CuryUnbilledMiscTot;
			}
			set
			{
				this._CuryUnbilledMiscTot = value;
			}
		}
		#endregion
		#region UnbilledMiscTot
		public abstract class unbilledMiscTot : PX.Data.BQL.BqlDecimal.Field<unbilledMiscTot> { }
		protected Decimal? _UnbilledMiscTot;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledMiscTot
		{
			get
			{
				return this._UnbilledMiscTot;
			}
			set
			{
				this._UnbilledMiscTot = value;
			}
		}
		#endregion
		#region CuryUnbilledTaxTotal
		public abstract class curyUnbilledTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledTaxTotal> { }
		protected Decimal? _CuryUnbilledTaxTotal;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unbilledTaxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Tax Total")]
		public virtual Decimal? CuryUnbilledTaxTotal
		{
			get
			{
				return this._CuryUnbilledTaxTotal;
			}
			set
			{
				this._CuryUnbilledTaxTotal = value;
			}
		}
		#endregion
		#region UnbilledTaxTotal
		public abstract class unbilledTaxTotal : PX.Data.BQL.BqlDecimal.Field<unbilledTaxTotal> { }
		protected Decimal? _UnbilledTaxTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledTaxTotal
		{
			get
			{
				return this._UnbilledTaxTotal;
			}
			set
			{
				this._UnbilledTaxTotal = value;
			}
		}
		#endregion
		#region CuryUnbilledDiscTotal
		public abstract class curyUnbilledDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledDiscTotal> { }

		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unbilledDiscTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryUnbilledDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledDiscTotal
		public abstract class unbilledDiscTotal : PX.Data.BQL.BqlDecimal.Field<unbilledDiscTotal> { }

		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledDiscTotal 
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderQty
		public abstract class unbilledOrderQty : PX.Data.BQL.BqlDecimal.Field<unbilledOrderQty> { }
		protected Decimal? _UnbilledOrderQty;
		[PXDBQuantity()]
		[PXUIField(DisplayName = "Unbilled Quantity")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledOrderQty
		{
			get
			{
				return this._UnbilledOrderQty;
			}
			set
			{
				this._UnbilledOrderQty = value;
			}
		}
		#endregion
		#region CuryControlTotal
		public abstract class curyControlTotal : PX.Data.BQL.BqlDecimal.Field<curyControlTotal> { }
		protected Decimal? _CuryControlTotal;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.controlTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Total")]
		public virtual Decimal? CuryControlTotal
		{
			get
			{
				return this._CuryControlTotal;
			}
			set
			{
				this._CuryControlTotal = value;
			}
		}
		#endregion
		#region ControlTotal
		public abstract class controlTotal : PX.Data.BQL.BqlDecimal.Field<controlTotal> { }
		protected Decimal? _ControlTotal;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ControlTotal
		{
			get
			{
				return this._ControlTotal;
			}
			set
			{
				this._ControlTotal = value;
			}
		}
		#endregion
		#region CuryPaymentTotal
		public abstract class curyPaymentTotal : PX.Data.BQL.BqlDecimal.Field<curyPaymentTotal> { }
		protected Decimal? _CuryPaymentTotal;
		[PXDBCalced(typeof(decimal0), typeof(decimal))]
		[PXCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.paymentTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Payment Total", Enabled=false)]
		public virtual Decimal? CuryPaymentTotal
		{
			get
			{
				return this._CuryPaymentTotal;
			}
			set
			{
				this._CuryPaymentTotal = value;
			}
		}
		#endregion
		#region PaymentTotal
		public abstract class paymentTotal : PX.Data.BQL.BqlDecimal.Field<paymentTotal> { }
		protected Decimal? _PaymentTotal;
		[PXDBCalced(typeof(decimal0), typeof(decimal))]
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? PaymentTotal
		{
			get
			{
				return this._PaymentTotal;
			}
			set
			{
				this._PaymentTotal = value;
			}
		}
		#endregion
		#region OverrideTaxZone
		public abstract class overrideTaxZone : PX.Data.BQL.BqlBool.Field<overrideTaxZone> { }
		protected Boolean? _OverrideTaxZone;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Tax Zone")]
		public virtual Boolean? OverrideTaxZone
		{
			get
			{
				return this._OverrideTaxZone;
			}
			set
			{
				this._OverrideTaxZone = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXRestrictor(typeof(Where<TaxZone.isManualVATZone, Equal<False>>), TX.Messages.CantUseManualVAT)]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<Location.cTaxCalcMode, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode { get; set; }
		#endregion
		#region AvalaraCustomerUsageType
		public abstract class avalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<avalaraCustomerUsageType> { }
		protected String _AvalaraCustomerUsageType;
		[PXDefault(
			TXAvalaraCustomerUsageType.Default,
			typeof(Search<Location.cAvalaraCustomerUsageType,
					Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
						And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Entity Usage Type")]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String AvalaraCustomerUsageType
		{
			get
			{
				return this._AvalaraCustomerUsageType;
			}
			set
			{
				this._AvalaraCustomerUsageType = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[ProjectDefault(BatchModule.SO,typeof(Search<Location.cDefProjectID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInSO, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBaseAttribute(typeof(SOOrder.customerID))]
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
		#region ShipComplete
		public abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }
		protected String _ShipComplete;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SOShipComplete.CancelRemainder)]
		[SOShipComplete.List()]
		[PXUIField(DisplayName = "Shipping Rule")]
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
		#region FOBPoint
		public abstract class fOBPoint : PX.Data.BQL.BqlString.Field<fOBPoint> { }
		protected String _FOBPoint;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		[PXSelector(typeof(Search<FOBPoint.fOBPointID>), DescriptionField = typeof(FOBPoint.description), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.cFOBPointID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
		public virtual String FOBPoint
		{
			get
			{
				return this._FOBPoint;
			}
			set
			{
				this._FOBPoint = value;
			}
		}
		#endregion
		#region ShipVia
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
		protected String _ShipVia;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>), typeof(Carrier.carrierID), typeof(Carrier.description), typeof(Carrier.isCommonCarrier), typeof(Carrier.confirmationRequired), typeof(Carrier.packageRequired), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.cCarrierID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ShipVia
		{
			get
			{
				return this._ShipVia;
			}
			set
			{
				this._ShipVia = value;
			}
		}
		#endregion
		#region WillCall
		[PXBool]
		[PXFormula(typeof(Switch<Case2<Where<Selector<shipVia, Carrier.isCommonCarrier>, NotEqual<True>>, True>, False>))]
		[PXUIField(DisplayName = "Will Call", IsReadOnly = true)]
		public bool? WillCall { get; set; }
		public abstract class willCall : PX.Data.BQL.BqlBool.Field<willCall> { }
		#endregion
		#region PackageLineCntr
		public abstract class packageLineCntr : PX.Data.BQL.BqlInt.Field<packageLineCntr> { }
		protected Int32? _PackageLineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? PackageLineCntr
		{
			get
			{
				return this._PackageLineCntr;
			}
			set
			{
				this._PackageLineCntr = value;
			}
		}
		#endregion
		#region PackageWeight
		public abstract class packageWeight : PX.Data.BQL.BqlDecimal.Field<packageWeight> { }
		protected Decimal? _PackageWeight;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Package Weight", Enabled=false)]
		public virtual Decimal? PackageWeight
		{
			get
			{
				return this._PackageWeight;
			}
			set
			{
				this._PackageWeight = value;
			}
		}
		#endregion
		#region UseCustomerAccount
		public abstract class useCustomerAccount : PX.Data.BQL.BqlBool.Field<useCustomerAccount> { }
		protected Boolean? _UseCustomerAccount;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Customer's Account")]
		public virtual Boolean? UseCustomerAccount
		{
			get
			{
				return this._UseCustomerAccount;
			}
			set
			{
				this._UseCustomerAccount = value;
			}
		}
		#endregion
		#region Resedential
		public abstract class resedential : PX.Data.BQL.BqlBool.Field<resedential> { }
		protected Boolean? _Resedential;
		[PXDBBool()]
		[PXDefault(typeof(Search<Location.cResedential, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[PXUIField(DisplayName = "Residential Delivery")]
		public virtual Boolean? Resedential
		{
			get
			{
				return this._Resedential;
			}
			set
			{
				this._Resedential = value;
			}
		}
		#endregion
		#region SaturdayDelivery
		public abstract class saturdayDelivery : PX.Data.BQL.BqlBool.Field<saturdayDelivery> { }
		protected Boolean? _SaturdayDelivery;
		[PXDBBool()]
		[PXDefault(typeof(Search<Location.cSaturdayDelivery, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[PXUIField(DisplayName = "Saturday Delivery")]
		public virtual Boolean? SaturdayDelivery
		{
			get
			{
				return this._SaturdayDelivery;
			}
			set
			{
				this._SaturdayDelivery = value;
			}
		}
		#endregion
		#region GroundCollect
		public abstract class groundCollect : PX.Data.BQL.BqlBool.Field<groundCollect> { }
		protected Boolean? _GroundCollect;
		[PXDBBool()]
		[PXDefault(typeof(Search<Location.cGroundCollect, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[PXUIField(DisplayName = "Ground Collect")]
		public virtual Boolean? GroundCollect
		{
			get
			{
				return this._GroundCollect;
			}
			set
			{
				this._GroundCollect = value;
			}
		}
		#endregion
		#region Insurance
		public abstract class insurance : PX.Data.BQL.BqlBool.Field<insurance> { }
		protected Boolean? _Insurance;
		[PXDBBool()]
		[PXDefault(typeof(Search<Location.cInsurance, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>))]
		[PXUIField(DisplayName = "Insurance")]
		public virtual Boolean? Insurance
		{
			get
			{
				return this._Insurance;
			}
			set
			{
				this._Insurance = value;
			}
		}
		#endregion
		#region Priority
		public abstract class priority : PX.Data.BQL.BqlShort.Field<priority> { }
		protected Int16? _Priority;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName="Priority")]
		public virtual Int16? Priority
		{
			get
			{
				return this._Priority;
			}
			set
			{
				this._Priority = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		[SalesPerson(DisplayName = "Default Salesperson")]
		[PXDefault(typeof(Search<CustDefSalesPeople.salesPersonID, Where<CustDefSalesPeople.bAccountID, Equal<Current<SOOrder.customerID>>, And<CustDefSalesPeople.locationID, Equal<Current<SOOrder.customerLocationID>>, And<CustDefSalesPeople.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<SOOrder.salesPersonID>.IsRelatedTo<SalesPerson.salesPersonID>))]
		public virtual Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion
		#region CommnPct
		public abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }
		protected Decimal? _CommnPct;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? CommnPct
		{
			get
			{
				return this._CommnPct;
			}
			set
			{
				this._CommnPct = value;
			}
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<SOOrder.customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[Terms(typeof(SOOrder.invoiceDate), typeof(SOOrder.dueDate), typeof(SOOrder.discDate), typeof(SOOrder.curyOrderTotal), typeof(SOOrder.curyTermsDiscAmt))]
		public virtual String TermsID
		{
			get
			{
				return this._TermsID;
			}
			set
			{
				this._TermsID = value;
			}
		}
		#endregion
		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		protected DateTime? _DueDate;
		[PXDBDate()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Due Date")]
		public virtual DateTime? DueDate
		{
			get
			{
				return this._DueDate;
			}
			set
			{
				this._DueDate = value;
			}
		}
		#endregion
		#region DiscDate
		public abstract class discDate : PX.Data.BQL.BqlDateTime.Field<discDate> { }
		protected DateTime? _DiscDate;
		[PXDBDate()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DiscDate
		{
			get
			{
				return this._DiscDate;
			}
			set
			{
				this._DiscDate = value;
			}
		}
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Invoice Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		[SOInvoiceNbr()]
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
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		protected DateTime? _InvoiceDate;
		[PXDBDate()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Invoice Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Default<SOOrder.orderDate>))]
		public virtual DateTime? InvoiceDate
		{
			get
			{
				return this._InvoiceDate;
			}
			set
			{
				this._InvoiceDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SOFinPeriod(typeof(SOOrder.invoiceDate), typeof(SOOrder.branchID))]
		//[AROpenPeriod(typeof(SOOrder.invoiceDate))]
		[PXUIField(DisplayName = "Post Period")]
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
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;
		[PXDBInt]
		[PXDefault(typeof(Customer.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PX.TM.PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Enabled = false)]
		public virtual int? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		protected Guid? _OwnerID;
		[PXDBGuid()]
		[PXDefault(typeof(Coalesce<
			Search<CREmployee.userID, Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>, And<CREmployee.status, NotEqual<BAccount.status.inactive>>>>, 
            Search<BAccount.ownerID, Where<BAccount.bAccountID, Equal<Current<SOOrder.customerID>>>>>), 
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PX.TM.PXOwnerSelector(typeof(workgroupID))]
        [PXUIField(DisplayName = "Owner")]
		public virtual Guid? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXInt()]
        [PXFormula(typeof(Switch<Case<Where<SOOrder.ownerID, IsNotNull>, Selector<SOOrder.ownerID, PX.TM.PXOwnerSelectorAttribute.EPEmployee.bAccountID>>, Null>))]
        public virtual Int32? EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this._EmployeeID = value;
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp]
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
		#region CustomerID_Customer_acctName
		public abstract class customerID_Customer_acctName : PX.Data.BQL.BqlString.Field<customerID_Customer_acctName> { }
		#endregion
		#region CuryTermsDiscAmt
		public abstract class curyTermsDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyTermsDiscAmt> { }
		protected Decimal? _CuryTermsDiscAmt = 0m;
		[PXDecimal(4)]
		public virtual Decimal? CuryTermsDiscAmt
		{
			get
			{
				return this._CuryTermsDiscAmt;
			}
			set
			{
				this._CuryTermsDiscAmt = value;
			}
		}
		#endregion
		#region TermsDiscAmt
		public abstract class termsDiscAmt : PX.Data.BQL.BqlDecimal.Field<termsDiscAmt> { }
		protected Decimal? _TermsDiscAmt = 0m;
		[PXDecimal(4)]
		public virtual Decimal? TermsDiscAmt
		{
			get
			{
				return this._TermsDiscAmt;
			}
			set
			{
				this._TermsDiscAmt = value;
			}
		}
		#endregion
		#region ShipTermsID
		public abstract class shipTermsID : PX.Data.BQL.BqlString.Field<shipTermsID>
		{
			public class PreventEditIfSOExists : PreventEditOf<ShipTerms.freightAmountSource>.On<ShipTermsMaint>
				.IfExists<Select<SOOrder, Where<SOOrder.shipTermsID, Equal<Current<ShipTerms.shipTermsID>>>>>
			{
				protected override string CreateEditPreventingReason(GetEditPreventingReasonArgs arg, object so, string fld, string tbl, string foreignTbl)
				{
					return PXMessages.LocalizeFormat(Messages.ShipTermsUsedInSO, ((SOOrder)so).OrderType, ((SOOrder)so).OrderNbr);
				}
			}
		}
		protected String _ShipTermsID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Terms")]
		[PXSelector(typeof(ShipTerms.shipTermsID), DescriptionField = typeof(ShipTerms.description), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.cShipTermsID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ShipTermsID
		{
			get
			{
				return this._ShipTermsID;
			}
			set
			{
				this._ShipTermsID = value;
			}
		}
		#endregion
		#region ShipZoneID
		public abstract class shipZoneID : PX.Data.BQL.BqlString.Field<shipZoneID> { }
		protected String _ShipZoneID;
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Zone")]
		[PXSelector(typeof(ShippingZone.zoneID), DescriptionField = typeof(ShippingZone.description), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.cShipZoneID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ShipZoneID
		{
			get
			{
				return this._ShipZoneID;
			}
			set
			{
				this._ShipZoneID = value;
			}
		}
		#endregion
		#region InclCustOpenOrders
		public abstract class inclCustOpenOrders : PX.Data.BQL.BqlBool.Field<inclCustOpenOrders> { }
		protected Boolean? _InclCustOpenOrders;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? InclCustOpenOrders
		{
			get
			{
				return this._InclCustOpenOrders;
			}
			set
			{
				this._InclCustOpenOrders = value;
			}
		}
		#endregion
		#region ShipmentCntr
		public abstract class shipmentCntr : PX.Data.BQL.BqlInt.Field<shipmentCntr> { }
		protected Int32? _ShipmentCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? ShipmentCntr
		{
			get
			{
				return this._ShipmentCntr;
			}
			set
			{
				this._ShipmentCntr = value;
			}
		}
		#endregion
		#region OpenShipmentCntr
		public abstract class openShipmentCntr : PX.Data.BQL.BqlInt.Field<openShipmentCntr> { }
		protected Int32? _OpenShipmentCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? OpenShipmentCntr
		{
			get
			{
				return this._OpenShipmentCntr;
			}
			set
			{
				this._OpenShipmentCntr = value;
			}
		}
		#endregion
		#region OpenLineCntr
		public abstract class openLineCntr : PX.Data.BQL.BqlInt.Field<openLineCntr> { }
		protected Int32? _OpenLineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? OpenLineCntr
		{
			get
			{
				return this._OpenLineCntr;
			}
			set
			{
				this._OpenLineCntr = value;
			}
		}
		#endregion
		#region DefaultSiteID
		public abstract class defaultSiteID : PX.Data.BQL.BqlInt.Field<defaultSiteID> { }
		protected Int32? _DefaultSiteID;
		[IN.Site(DisplayName = "Preferred Warehouse ID", DescriptionField = typeof(INSite.descr))]
		[PXDefault(typeof(Search<Location.cSiteID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<defaultSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? DefaultSiteID
		{
			get
			{
				return this._DefaultSiteID;
			}
			set
			{
				this._DefaultSiteID = value;
			}
		}
		#endregion
		#region DestinationSiteID
		public abstract class destinationSiteID : PX.Data.BQL.BqlInt.Field<destinationSiteID> { }
		protected Int32? _DestinationSiteID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[IN.ToSite(typeof(INTransferType.twoStep), typeof(SOOrder.branchID), DisplayName = "Destination Warehouse", DescriptionField = typeof(INSite.descr))]
		[PXForeignReference(typeof(Field<destinationSiteID>.IsRelatedTo<INSite.siteID>))]
		public virtual Int32? DestinationSiteID
		{
			get
			{
				return this._DestinationSiteID;
			}
			set
			{
				this._DestinationSiteID = value;
			}
		}
		#endregion
        #region DestinationSiteIdErrorMessage
        public abstract class destinationSiteIdErrorMessage : PX.Data.BQL.BqlString.Field<destinationSiteIdErrorMessage> { }
        [PXString(150, IsUnicode = true)]
        public virtual string DestinationSiteIdErrorMessage { get; set; }
        #endregion
		#region DefaultOperation
		public abstract class defaultOperation : PX.Data.BQL.BqlString.Field<defaultOperation> { }
		protected String _DefaultOperation;
		[PXString(SOOrderType.defaultOperation.Length, IsFixed = true)]
		[PXFormula(typeof(Selector<SOOrder.orderType, SOOrderType.defaultOperation>))]
		public virtual String DefaultOperation
		{
			get
			{
				return this._DefaultOperation;
			}
			set
			{
				this._DefaultOperation = value;
			}
		}
		#endregion
		#region OrigOrderType
		public abstract class origOrderType : PX.Data.BQL.BqlString.Field<origOrderType> { }
		protected String _OrigOrderType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName="Orig. Order Type", Enabled=false)]
		public virtual String OrigOrderType
		{
			get
			{
				return this._OrigOrderType;
			}
			set
			{
				this._OrigOrderType = value;
			}
		}
		#endregion
		#region OrigOrderNbr
		public abstract class origOrderNbr : PX.Data.BQL.BqlString.Field<origOrderNbr> { }
		protected String _OrigOrderNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Orig. Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<SOOrder.origOrderType>>>>))]
		public virtual String OrigOrderNbr
		{
			get
			{
				return this._OrigOrderNbr;
			}
			set
			{
				this._OrigOrderNbr = value;
			}
		}
		#endregion
		#region ManDisc
		public abstract class manDisc : PX.Data.BQL.BqlDecimal.Field<manDisc> { }
		protected Decimal? _ManDisc;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ManDisc
		{
			get
			{
				return this._ManDisc;
			}
			set
			{
				this._ManDisc = value;
			}
		}
		#endregion
		#region CuryManDisc
		public abstract class curyManDisc : PX.Data.BQL.BqlDecimal.Field<curyManDisc> { }
		protected Decimal? _CuryManDisc;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.manDisc))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Manual Total")]
		public virtual Decimal? CuryManDisc
		{
			get
			{
				return this._CuryManDisc;
			}
			set
			{
				this._CuryManDisc = value;
			}
		}
		#endregion
		#region ApprovedCredit
		public abstract class approvedCredit : PX.Data.BQL.BqlBool.Field<approvedCredit> { }
		protected Boolean? _ApprovedCredit;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? ApprovedCredit
		{
			get
			{
				return this._ApprovedCredit;
			}
			set
			{
				this._ApprovedCredit = value;
			}
		}
		#endregion
		#region ApprovedCreditAmt
		public abstract class approvedCreditAmt : PX.Data.BQL.BqlDecimal.Field<approvedCreditAmt> { }
		protected Decimal? _ApprovedCreditAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ApprovedCreditAmt
		{
			get
			{
				return this._ApprovedCreditAmt;
			}
			set
			{
				this._ApprovedCreditAmt = value;
			}
		}
		#endregion
		#region DefaultSiteID_INSite_descr
		public abstract class defaultSiteID_INSite_descr : PX.Data.BQL.BqlString.Field<defaultSiteID_INSite_descr> { }
		#endregion
		#region ShipVia_Carrier_description
		public abstract class shipVia_Carrier_description : PX.Data.BQL.BqlString.Field<shipVia_Carrier_description> { }
		#endregion
        #region PaymentMethodID
        public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
        protected String _PaymentMethodID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.paymentMethodID, InnerJoin<Customer, On<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>,
                                        Where<Customer.bAccountID, Equal<Current<SOOrder.customerID>>,
                                              And<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>>>>,
                                   Search<Customer.defPaymentMethodID,
                                         Where<Customer.bAccountID, Equal<Current<SOOrder.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<PaymentMethod.paymentMethodID, 
                                Where<PaymentMethod.isActive, Equal<boolTrue>,
                                And<PaymentMethod.useForAR, Equal<boolTrue>>>>), DescriptionField = typeof(PaymentMethod.descr))]
        [PXUIFieldAttribute(DisplayName = "Payment Method")]        
        public virtual String PaymentMethodID
        {
            get
            {
                return this._PaymentMethodID;
            }
            set
            {
                this._PaymentMethodID = value;
            }
        }
        #endregion
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		protected Int32? _PMInstanceID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(CustomerPaymentMethod.pMInstanceID))]
		[PXUIField(DisplayName = "Card/Account No")]
		[PXDefault(typeof(Coalesce<
                        Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
                                And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
                                Where<Customer.bAccountID, Equal<Current2<SOOrder.customerID>>,
									And<CustomerPaymentMethod.isActive,Equal<True>,
									And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOOrder.paymentMethodID>>>>>>,
                        Search<CustomerPaymentMethod.pMInstanceID,
                                Where<CustomerPaymentMethod.bAccountID, Equal<Current2<SOOrder.customerID>>,
                                    And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOOrder.paymentMethodID>>,
                                    And<CustomerPaymentMethod.isActive, Equal<True>>>>,
								OrderBy<Desc<CustomerPaymentMethod.expirationDate, 
									Desc<CustomerPaymentMethod.pMInstanceID>>>>>)
                        , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current2<SOOrder.customerID>>,
            And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOOrder.paymentMethodID>>,
            And<Where<CustomerPaymentMethod.isActive, Equal<boolTrue>, Or<CustomerPaymentMethod.pMInstanceID,
                    Equal<Current<SOOrder.pMInstanceID>>>>>>>>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
		[DeprecatedProcessing]
		[DisabledProcCenter]
		public virtual Int32? PMInstanceID
		{
			get
			{
				return this._PMInstanceID;
			}
			set
			{
				this._PMInstanceID = value;
			}
		}
		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected Int32? _CashAccountID;

		[PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.cashAccountID, 
									InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID,Equal<CustomerPaymentMethod.cashAccountID>,
										And<PaymentMethodAccount.paymentMethodID,Equal<CustomerPaymentMethod.paymentMethodID>,
										And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
									Where<CustomerPaymentMethod.bAccountID, Equal<Current<SOOrder.customerID>>,
										And<CustomerPaymentMethod.pMInstanceID, Equal<Current2<SOOrder.pMInstanceID>>>>>,
								Search2<CashAccount.cashAccountID,
                                InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
                                    And<PaymentMethodAccount.useForAR, Equal<True>,
                                    And<PaymentMethodAccount.aRIsDefault, Equal<True>,
                                    And<PaymentMethodAccount.paymentMethodID, Equal<Current2<SOOrder.paymentMethodID>>>>>>>,
                                    Where<CashAccount.branchID,Equal<Current<SOOrder.branchID>>,
										And<Match<Current<AccessInfo.userName>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [CashAccount(typeof(SOOrder.branchID), typeof(Search2<CashAccount.cashAccountID, 
                InnerJoin<PaymentMethodAccount, 
                    On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
                        And<PaymentMethodAccount.useForAR,Equal<True>,
                        And<PaymentMethodAccount.paymentMethodID, 
                        Equal<Current2<SOOrder.paymentMethodID>>>>>>, 
                        Where<Match<Current<AccessInfo.userName>>>>), SuppressCurrencyValidation = false)]
		public virtual Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Ref.", Enabled = false)]
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
		#region CreatePMInstance
		public abstract class createPMInstance : PX.Data.BQL.BqlBool.Field<createPMInstance> { }
		protected bool? _CreatePMInstance = false;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "New Card")]
		public virtual bool? CreatePMInstance
		{
			get
			{
				return _CreatePMInstance;
			}
			set
			{
				_CreatePMInstance = value;
			}
		}
		#endregion
		#region PreAuthTranNumber
		public abstract class preAuthTranNumber : PX.Data.BQL.BqlString.Field<preAuthTranNumber> { }
		protected String _PreAuthTranNumber;
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Pre-Auth. Nbr.",Enabled=false)]
		public virtual String PreAuthTranNumber
		{
			get
			{
				return this._PreAuthTranNumber;
			}
			set
			{
				this._PreAuthTranNumber = value;
			}
		}
		#endregion

		#region CaptureTranNumber
		public abstract class captureTranNumber : PX.Data.BQL.BqlString.Field<captureTranNumber> { }
		protected String _CaptureTranNumber;
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Capture Tran. Nbr.", Enabled = false)]
		public virtual String CaptureTranNumber
		{
			get
			{
				return this._CaptureTranNumber;
			}
			set
			{
				this._CaptureTranNumber = value;
			}
		}
		#endregion		
		#region TranNbr
		public abstract class cCAuthTranNbr : PX.Data.BQL.BqlInt.Field<cCAuthTranNbr> { }
		protected Int32? _CCAuthTranNbr;
		[PXInt()]
		[PXUIField(DisplayName = "CC Tran. Nbr.")]
		public virtual Int32? CCAuthTranNbr
		{
			get
			{
				return this._CCAuthTranNbr;
			}
			set
			{
				this._CCAuthTranNbr = value;
			}
		}
		#endregion	
		#region CCPaymentStateDescr
		public abstract class cCPaymentStateDescr : PX.Data.BQL.BqlString.Field<cCPaymentStateDescr> { }
		protected String _CCPaymentStateDescr;
		[PXString(255)]
		[PXUIField(DisplayName = "Processing Status", Enabled = false)]
		public virtual String CCPaymentStateDescr
		{
			get
			{
				return this._CCPaymentStateDescr;
			}
			set
			{
				this._CCPaymentStateDescr = value;
			}
		}
		#endregion	
		#region CCCardNumber
		public abstract class cCCardNumber : PX.Data.BQL.BqlString.Field<cCCardNumber> { }
		protected String _CCCardNumber;
		[PXString(255)]
		[PXUIField(DisplayName = "CC Number")]
		public virtual String CCCardNumber
		{
			get
			{
				return this._CCCardNumber;
			}
			set
			{
				this._CCCardNumber = value;
			}
		}
		#endregion	

		#region CuryUnpaidBalance
		public abstract class curyUnpaidBalance : PX.Data.BQL.BqlDecimal.Field<curyUnpaidBalance> { }
		[PXCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.unpaidBalance))]
        [PXFormula(typeof(Sub<Sub<
            Switch<Case<Where<Add<SOOrder.releasedCntr, SOOrder.billedCntr>, Equal<int0>>, SOOrder.curyOrderTotal>,
            SOOrder.curyUnbilledOrderTotal>,
            SOOrder.curyPaymentTotal>,
			Switch<Case<Where<SOOrder.isCCCaptured, Equal<True>>, SOOrder.curyCCCapturedAmt>, SOOrder.curyCCPreAuthAmount>>))]
        [PXUIField(DisplayName = "Unpaid Balance", Enabled = false)]
		public decimal? CuryUnpaidBalance
		{
			get;
			set;
		}
		#endregion
		#region UnpaidBalance
		public abstract class unpaidBalance : PX.Data.BQL.BqlDecimal.Field<unpaidBalance> { }
		[PXBaseCury()]
		public decimal? UnpaidBalance
		{
			get;
			set;
		}
		#endregion

		#region ICCPayment Members

		decimal? ICCPayment.CuryDocBal
		{
			get
			{
				decimal CuryBal = (CuryUnpaidBalance ?? 0m) + (CuryCCPreAuthAmount ?? 0m);
				return CuryBal > 0m ? CuryBal : 0m;
			}
			set
			{
			}
		}
		

		string ICCPayment.DocType
		{
			get
			{
				return null;
			}
			set
			{
				
			}
		}

		string ICCPayment.RefNbr
		{
			get
			{
				return null;
			}
			set
			{
				
			}
		}

		string ICCPayment.OrigDocType
		{
			get { return this.OrderType; }
		}

		string ICCPayment.OrigRefNbr
		{
			get { return this.OrderNbr; }
		}
		bool? ICCPayment.Released => false;
		#endregion
		#region PCResponseReasonText
		public abstract class pCResponseReasonText : PX.Data.BQL.BqlString.Field<pCResponseReasonText> { }
		protected String _PCResponseReasonText;
		[PXString(255)]
		[PXUIField(DisplayName = "PC Response Reason", Enabled = false)]
		public virtual String PCResponseReasonText
		{
			get
			{
				return this._PCResponseReasonText;
			}
			set
			{
				this._PCResponseReasonText = value;
			}
		}
		#endregion	

		#region IsCCAuthorized
		public abstract class isCCAuthorized : PX.Data.BQL.BqlBool.Field<isCCAuthorized> { }
		protected bool? _IsCCAuthorized = false;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "CC Authorized")]
		public virtual bool? IsCCAuthorized
		{
			get
			{
				return _IsCCAuthorized;
			}
			set
			{
				_IsCCAuthorized = value;
			}
		}
		#endregion
		#region CCAuthExpirationDate
		public abstract class cCAuthExpirationDate : PX.Data.BQL.BqlDateTime.Field<cCAuthExpirationDate> { }
		protected DateTime? _CCAuthExpirationDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Auth. expires on")]
		public virtual DateTime? CCAuthExpirationDate
		{
			get
			{
				return _CCAuthExpirationDate;
			}
			set
			{
				_CCAuthExpirationDate = value;
			}
		}
		#endregion
		#region CCPreAuthAmount
		public abstract class curyCCPreAuthAmount : PX.Data.BQL.BqlDecimal.Field<curyCCPreAuthAmount> { }
		protected Decimal? _CuryCCPreAuthAmount;
		[PXDBCury(typeof(SOOrder.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pre-Authorized Amount", Enabled = false)]
		public virtual Decimal? CuryCCPreAuthAmount
		{
			get
			{
				return this._CuryCCPreAuthAmount;
			}
			set
			{
				this._CuryCCPreAuthAmount = value;
			}
		}
		#endregion		
		#region IsCCCaptured
		public abstract class isCCCaptured : PX.Data.BQL.BqlBool.Field<isCCCaptured> { }
		protected bool? _IsCCCaptured = false;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "CC Captured")]
		public virtual bool? IsCCCaptured
		{
			get
			{
				return _IsCCCaptured;
			}
			set
			{
				_IsCCCaptured = value;
			}
		}
		#endregion
		#region CuryCCCapturedAmt
		public abstract class curyCCCapturedAmt : PX.Data.BQL.BqlDecimal.Field<curyCCCapturedAmt> { }
		protected Decimal? _CuryCCCapturedAmt;
		[PXDBCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.cCCapturedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Captured Amount", Enabled = false)]
		public virtual Decimal? CuryCCCapturedAmt
		{
			get
			{
				return this._CuryCCCapturedAmt;
			}
			set
			{
				this._CuryCCCapturedAmt = value;
			}
		}
		#endregion
		#region CCCapturedAmt
		public abstract class cCCapturedAmt : PX.Data.BQL.BqlDecimal.Field<cCCapturedAmt> { }
		protected Decimal? _CCCapturedAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]		
		public virtual Decimal? CCCapturedAmt
		{
			get
			{
				return this._CCCapturedAmt;
			}
			set
			{
				this._CCCapturedAmt = value;
			}
		}
		#endregion
		#region IsCCCaptureFailed
		public abstract class isCCCaptureFailed : PX.Data.BQL.BqlBool.Field<isCCCaptureFailed> { }
		protected bool? _IsCCCaptureFailed = false;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "CC Capture Failed")]
		public virtual bool? IsCCCaptureFailed
		{
			get
			{
				return _IsCCCaptureFailed;
			}
			set
			{
				_IsCCCaptureFailed = value;
			}
		}
		#endregion

		/// <summary>
		/// Allows to select CC by Payment Profile ID (main usage: API)
		/// </summary>
		public abstract class paymentProfileID : PX.Data.BQL.BqlString.Field<paymentProfileID> { }
		[PXUIField(DisplayName = "Payment Profile ID", Visible = false)]
		[PXString(IsUnicode = true)]
		public string PaymentProfileID
		{
			get;
			set;
		}

		#region IsManualPackage
		public abstract class isManualPackage : PX.Data.BQL.BqlBool.Field<isManualPackage> { }
		protected bool? _IsManualPackage = false;
		[PXDBBool()]
		[PXUIField(DisplayName = "Manual Packaging")]
		public virtual bool? IsManualPackage
		{
			get
			{
				return _IsManualPackage;
			}
			set
			{
				_IsManualPackage = value;
			}
		}
		#endregion
		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is up to date", Enabled = false)]
		public virtual Boolean? IsTaxValid
		{
			get; set;
		}
		#endregion
		#region IsOpenTaxValid
		public abstract class isOpenTaxValid : PX.Data.BQL.BqlBool.Field<isOpenTaxValid> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsOpenTaxValid
		{
			get;
			set;
		}
		#endregion
		#region IsUnbilledTaxValid
		public abstract class isUnbilledTaxValid : PX.Data.BQL.BqlBool.Field<isUnbilledTaxValid> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsUnbilledTaxValid
		{
			get;
			set;
		}
		#endregion
		#region IsFreightTaxValid
		public abstract class isFreightTaxValid : PX.Data.BQL.BqlBool.Field<isFreightTaxValid> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Freight Tax is up to date", Enabled = false)]
		public virtual Boolean? IsFreightTaxValid
		{
			get;
			set;
		}
		#endregion
		#region IInvoice Members
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		protected decimal? _CuryDocBal;
        [PXFormula(typeof(Switch<Case<Where<Add<SOOrder.releasedCntr, SOOrder.billedCntr>, Equal<int0>>, SOOrder.curyOrderTotal>,
                        SOOrder.curyUnbilledOrderTotal>))]
        [PXCurrency(typeof(SOOrder.curyInfoID), typeof(SOOrder.docBal))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? CuryDocBal
		{
			get
			{
				return this._CuryDocBal;
			}
			set
			{
				this._CuryDocBal = value;
			}
		}
		public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		protected decimal? _DocBal;
        [PXFormula(typeof(Switch<Case<Where<Add<SOOrder.releasedCntr, SOOrder.billedCntr>, Equal<int0>>, SOOrder.orderTotal>,
            SOOrder.unbilledOrderTotal>))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? DocBal
		{
			get
			{
				return this._DocBal;
			}
			set
			{
				this._DocBal = value; ;
			}
		}

		public decimal? CuryDiscBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? DiscBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? CuryWhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public decimal? WhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
			}
		}

		public string DocType
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

		public string RefNbr
		{
			get
			{
				return this.OrderNbr;
			}
			set
			{
				this.OrderNbr = value;
			}
		}

		public string OrigModule
		{
			get { return null; }
			set {  }
		}

		public decimal? CuryOrigDocAmt
		{
			get { return null; }
			set { }
		}

		public decimal? OrigDocAmt
		{
			get { return null; }
			set { }
		}

		public DateTime? DocDate
		{
			get { return null; }
			set { }
		}

		public string DocDesc
		{
			get { return null; }
			set {  }
		}
		#endregion
		#region RefTranExtNbr
		public abstract class refTranExtNbr : PX.Data.BQL.BqlString.Field<refTranExtNbr> { }
		protected String _RefTranExtNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<ExternalTransaction.tranNumber,
			Where<ExternalTransaction.pMInstanceID, Equal<Current<SOOrder.pMInstanceID>>,
				And<ExternalTransaction.processingStatus, Equal<ExtTransactionProcStatusCode.captureSuccess>>>,
			OrderBy<Desc<ExternalTransaction.transactionID>>>),
			typeof(ExternalTransaction.transactionID), typeof(ExternalTransaction.docType), typeof(ExternalTransaction.refNbr), typeof(ExternalTransaction.amount))]
		[PXUIField(DisplayName = "Orig. PC Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String RefTranExtNbr
		{
			get
			{
				return this._RefTranExtNbr;
			}
			set
			{
				this._RefTranExtNbr = value;
			}
		}
		#endregion
		#region DisableAutomaticDiscountCalculation
		public abstract class disableAutomaticDiscountCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		protected Boolean? _DisableAutomaticDiscountCalculation;
		[PXDBBool]
		[PXDefault(false, typeof(Search<SOOrderType.disableAutomaticDiscountCalculation, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>>>))]
		[PXUIField(DisplayName = "Disable Automatic Discount Update")]
		public virtual Boolean? DisableAutomaticDiscountCalculation
		{
			get { return this._DisableAutomaticDiscountCalculation; }
			set { this._DisableAutomaticDiscountCalculation = value; }
		}
		#endregion

		#region IAssign Members
		int? PX.Data.EP.IAssign.WorkgroupID { get; set; }
		Guid? PX.Data.EP.IAssign.OwnerID { get; set; }
		#endregion
	}

	public class SOOrderTypeConstants
	{
		public const string SalesOrder = "SO";
		public const string Invoice = "IN";
		public const string DebitMemo = "DM";
		public const string CreditMemo = "CM";
		public const string StandardOrder = "ST";
		public const string TransferOrder = "TR";
		public const string RMAOrder = "RM";
		public const string QuoteOrder = "QT";
		public class salesOrder : PX.Data.BQL.BqlString.Constant<salesOrder> { public salesOrder():base(SalesOrder){}}
		public class transferOrder : PX.Data.BQL.BqlString.Constant<transferOrder> { public transferOrder() : base(TransferOrder) { } }
		public class rmaOrder : PX.Data.BQL.BqlString.Constant<rmaOrder> { public rmaOrder() : base(RMAOrder) { } }
		public class quoteOrder : PX.Data.BQL.BqlString.Constant<quoteOrder> { public quoteOrder() : base(QuoteOrder) { } }
        public class invoiceOrder : PX.Data.BQL.BqlString.Constant<invoiceOrder> { public invoiceOrder() : base(Invoice) { } }
		public class creditMemo : PX.Data.BQL.BqlString.Constant<creditMemo> { public creditMemo() : base(CreditMemo) { } }
	}

	public class SO 
	{
		/// <summary>
		/// Specialized selector for SOOrder RefNbr.<br/>
		/// By default, defines the following set of columns for the selector:<br/>
		/// SOOrder.orderNbr,SOOrder.orderDate, SOOrder.customerID,<br/>
		/// SOOrder.customerID_Customer_acctName, SOOrder.customerLocationID,<br/>
		/// SOOrder.curyID, SOOrder.curyOrderTotal, SOOrder.status,SOOrder.invoiceNbr<br/>
		/// </summary>
		public class RefNbrAttribute : PXSelectorAttribute
		{
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(SOOrder.orderNbr),
                typeof(SOOrder.customerOrderNbr),
                typeof(SOOrder.orderDate),
				typeof(SOOrder.customerID),
				typeof(SOOrder.customerID_Customer_acctName),
				typeof(SOOrder.customerLocationID),
				typeof(SOOrder.curyID),
				typeof(SOOrder.curyOrderTotal),
				typeof(SOOrder.status),
				typeof(SOOrder.invoiceNbr))
			{
			}
		}

		/// <summary>
		/// Specialized for SOOrder version of the <see cref="AutoNumberAttribute"/><br/>
		/// It defines how the new numbers are generated for the SO Order. <br/>
		/// References SOOrder.orderDate fields of the document,<br/>
		/// and also define a link between  numbering ID's defined in SO Order Type: namely SOOrderType.orderNumberingID. <br/>        
		/// </summary>		
		public class NumberingAttribute : AutoNumberAttribute
		{
            public NumberingAttribute()
                : base(typeof(Search<SOOrderType.orderNumberingID, Where<SOOrderType.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderType.active, Equal<True>>>>), typeof(SOOrder.orderDate))
            {; }
        }
    }

	public class SOOrderStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Open, Messages.Open),
					Pair(Hold, Messages.Hold),
					Pair(PendingApproval, EP.Messages.Balanced),
					Pair(Voided, EP.Messages.Voided),
					Pair(CreditHold, Messages.CreditHold),
					Pair(Completed, Messages.Completed),
					Pair(Cancelled, Messages.Cancelled),
					Pair(BackOrder, Messages.BackOrder),
					Pair(Shipping, Messages.Shipping),
					Pair(Invoiced, Messages.Invoiced),
				}) {}
		}

		public class ListWithoutOrdersAttribute : PXStringListAttribute
		{
			public ListWithoutOrdersAttribute() : base(
				new[]
				{

					Pair(Open, Messages.Open),
					Pair(Hold, Messages.Hold),
					Pair(PendingApproval, EP.Messages.Balanced),
					Pair(Voided, EP.Messages.Voided),
					Pair(CreditHold, Messages.CreditHold),
					Pair(Completed, Messages.Completed),
					Pair(Cancelled, Messages.Cancelled),
					Pair(BackOrder, Messages.BackOrder),
					Pair(Shipping, Messages.Shipping),
					Pair(Invoiced, Messages.Invoiced),
				}) {}
		}

		public const string Open = "N";
		public const string Hold = "H";
		public const string PendingApproval = "P";
		public const string Voided = "V";
		public const string CreditHold = "R";
		public const string Completed = "C";
		public const string Cancelled = "L";
		public const string BackOrder = "B";
		public const string Shipping = "S";
		public const string Invoiced = "I";

        public class voided : PX.Data.BQL.BqlString.Constant<voided>
		{
            public voided() : base(Voided) {}
        }
        public class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
		{
            public pendingApproval() : base(PendingApproval) {}
        }
		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) { ;}
		}

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { ;}
		}

		public class creditHold : PX.Data.BQL.BqlString.Constant<creditHold>
		{
			public creditHold() : base(CreditHold) { ;}
		}

		public class completed : PX.Data.BQL.BqlString.Constant<completed>
		{
			public completed() : base(Completed) { ;}
		}

		public class cancelled : PX.Data.BQL.BqlString.Constant<cancelled>
		{
			public cancelled() : base(Cancelled) { ;}
		}

		public class backOrder : PX.Data.BQL.BqlString.Constant<backOrder>
		{
			public backOrder() : base(BackOrder) { ;}
		}

		public class shipping : PX.Data.BQL.BqlString.Constant<shipping>
		{
			public shipping() : base(Shipping) { ;}
		}

		public class invoiced : PX.Data.BQL.BqlString.Constant<invoiced>
		{
			public invoiced() : base(Invoiced) { ;}
		}
	}
}
