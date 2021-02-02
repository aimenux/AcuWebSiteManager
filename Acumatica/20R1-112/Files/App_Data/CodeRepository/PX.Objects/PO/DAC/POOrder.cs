using System;
using System.Collections.Generic;

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

using PX.SM;
using PX.TM;

using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.EP;
using PX.Objects.PM;

using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.Common.Bql;
using PX.Objects.Common;
using PX.Common;

namespace PX.Objects.PO
{

	[Serializable]
	[PXPrimaryGraph(typeof(POOrderEntry))]
	[PXCacheName(Messages.POOrder)]
	[PXEMailSource]
	public partial class POOrder : PX.Data.IBqlTable, PX.Data.EP.IAssign
	{
		#region Keys
		public class PK : PrimaryKeyOf<POOrder>.By<orderType, orderNbr>
		{
			public static POOrder Find(PXGraph graph, string orderType, string orderNbr) => FindBy(graph, orderType, orderNbr);
		}
		public static class FK
		{
			public class RemitAddress : PORemitAddress.PK.ForeignKeyOf<POOrder>.By<remitAddressID> { }
			public class RemitContact : PORemitContact.PK.ForeignKeyOf<POOrder>.By<remitContactID> { }
			public class SOOrderType : SO.SOOrderType.PK.ForeignKeyOf<POOrder>.By<sOOrderType> { }
			public class SOOrder : SO.SOOrder.PK.ForeignKeyOf<POOrder>.By<sOOrderType, sOOrderNbr> { }
			public class RQRequisition : RQ.RQRequisition.PK.ForeignKeyOf<POOrder>.By<rQReqNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<POOrder>.By<siteID> { }
			public class ShipAddress : POShipAddress.PK.ForeignKeyOf<POOrder>.By<shipAddressID> { }
			public class ShipContact : POShipContact.PK.ForeignKeyOf<POOrder>.By<shipContactID> { }
			public class FOBPoint : CS.FOBPoint.PK.ForeignKeyOf<POOrder>.By<fOBPoint> { }
			public class Vendor : BAccount.PK.ForeignKeyOf<POOrder>.By<vendorID> { }
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
		[GL.Branch(typeof(Coalesce<
			Search<Location.vBranchID, Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>,
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
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(POOrderType.RegularOrder)]
		[POOrderType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
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
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PO.Numbering()]
		[PO.RefNbr(typeof(Search2<POOrder.orderNbr,
			LeftJoinSingleTable<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>,
			And<Match<Vendor, Current<AccessInfo.userName>>>>>,
			Where<POOrder.orderType, Equal<Optional<POOrder.orderType>>,
			And<Vendor.bAccountID, IsNotNull>>,
			OrderBy<Desc<POOrder.orderNbr>>>), Filterable = true)]
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
		#region Behavior
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		[PXUIField(DisplayName = "Workflow", FieldClass = PM.PMChangeOrder.FieldClass, Visible = false)]
		[PXDefault(POBehavior.Standard)]
		[POBehavior.List]
		public virtual String Behavior
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[POVendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
		[PXDefault]
		[PXForeignReference(typeof(FK.Vendor))]
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
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		protected Int32? _VendorLocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>,
			And<Location.isActive, Equal<True>,
			And<MatchWithBranch<Location.vBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID, 
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>, 
			Where<BAccountR.bAccountID, Equal<Current<POOrder.vendorID>>, 
				And<CRLocation.isActive, Equal<True>,
				And<MatchWithBranch<CRLocation.vBranchID>>>>>,
			Search<CRLocation.locationID,
			Where<CRLocation.bAccountID, Equal<Current<POOrder.vendorID>>,
			And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>))]
		[PXForeignReference(typeof(Field<vendorLocationID>.IsRelatedTo<Location.locationID>))]
		public virtual Int32? VendorLocationID
		{
			get
			{
				return this._VendorLocationID;
			}
			set
			{
				this._VendorLocationID = value;
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
		#region ExpectedDate
		public abstract class expectedDate : PX.Data.BQL.BqlDateTime.Field<expectedDate> { }
		protected DateTime? _ExpectedDate;

		[PXDBDate()]
		[PXDefault(typeof(POOrder.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Promised On")]
		public virtual DateTime? ExpectedDate
		{
			get
			{
				return this._ExpectedDate;
			}
			set
			{
				this._ExpectedDate = value;
			}
		}
		#endregion
		#region ExpirationDate
		public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }
		protected DateTime? _ExpirationDate;

		[PXDBDate()]
		[PXDefault(typeof(POOrder.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expires On")]
		public virtual DateTime? ExpirationDate
		{
			get
			{
				return this._ExpirationDate;
			}
			set
			{
				this._ExpirationDate = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(POOrderStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[POOrderStatus.List()]
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
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		[PXNoUpdate]
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
		[PXUIField(DisplayName = "Reject", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
		#region RequestApproval
		public abstract class requestApproval : PX.Data.BQL.BqlBool.Field<requestApproval> { }
		protected Boolean? _RequestApproval;
		[PXBool()]
		[PXUIField(DisplayName = "Request Approval", Visible = false)]
		public virtual Boolean? RequestApproval
		{
			get
			{
				return this._RequestApproval;
			}
			set
			{
				this._RequestApproval = value;
			}
		}
		#endregion
		#region Cancelled
		public abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }
		protected Boolean? _Cancelled;
		[PXDBBool()]
		[PXUIField(DisplayName = "Cancel", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
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
		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is up to date", Enabled = false)]
		public virtual Boolean? IsTaxValid
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.PO, "{0} - {2}", new Type[] { typeof(POOrder.orderNbr), typeof(POOrder.vendorID), typeof(Vendor.acctName) },
		   new Type[] { typeof(POOrder.vendorRefNbr), typeof(POOrder.orderDesc) },
		   NumberFields = new Type[] { typeof(POOrder.orderNbr) },
		   Line1Format = "{0}{1:d}{2}{3}{4}", Line1Fields = new Type[] { typeof(POOrder.orderType), typeof(POOrder.orderDate), typeof(POOrder.status), typeof(POOrder.vendorRefNbr), typeof(POOrder.expectedDate) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(POOrder.orderDesc) },
		   MatchWithJoin = typeof(InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POOrder.vendorID>>>),
		   SelectForFastIndexing = typeof(Select2<POOrder, InnerJoin<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>>>>)
		)]
		[PXNote(new Type[0], ShowInReferenceSelector = true, Selector = typeof(
			Search2<
				POOrder.orderNbr,
			LeftJoinSingleTable<Vendor, 
				On<POOrder.vendorID, Equal<Vendor.bAccountID>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>>,
			Where<
				Vendor.bAccountID, IsNotNull,
				And<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>>,
			OrderBy<
				Desc<POOrder.orderNbr>>>))]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<GL.Company.baseCuryID>))]
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
		[CurrencyInfo(ModuleCode = GL.BatchModule.PO)]
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
		#region VendorRefNbr
		public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }
		protected String _VendorRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vendor Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String VendorRefNbr
		{
			get
			{
				return this._VendorRefNbr;
			}
			set
			{
				this._VendorRefNbr = value;
			}
		}
		#endregion
		#region CuryOrderTotal
		public abstract class curyOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyOrderTotal> { }
		protected Decimal? _CuryOrderTotal;

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.orderTotal))]
		[PXUIField(DisplayName = "Order Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Total")]
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
		#region CuryControlTotal
		public abstract class curyControlTotal : PX.Data.BQL.BqlDecimal.Field<curyControlTotal> { }
		protected Decimal? _CuryControlTotal;
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.controlTotal))]
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
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Order Qty")]
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
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		protected Decimal? _CuryLineTotal;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.lineTotal))]
		[PXUIField(DisplayName = "Line Total", Visibility = PXUIVisibility.SelectorVisible)]
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
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Line Total")]
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

        #region DiscTot
        public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }
        protected Decimal? _DiscTot;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        [PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.discTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount Total", Enabled = true)]
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
        [PXCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.docDisc))]
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

		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		protected Decimal? _CuryTaxTotal;

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.taxTotal))]
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
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
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

        #region CuryVatExemptTotal
        public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
        protected Decimal? _CuryVatExemptTotal;
        [PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.vatExemptTotal))]
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
        [PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.vatTaxableTotal))]
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

        #region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Location.vTaxZoneID, Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vendor Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TX.TaxZone.taxZoneID), DescriptionField = typeof(TX.TaxZone.descr), Filterable = true)]
		[PXRestrictor(typeof(Where<TX.TaxZone.isManualVATZone, Equal<False>>), TX.Messages.CantUseManualVAT)]
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
		protected string _TaxCalcMode;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TX.TaxCalculationMode.TaxSetting, typeof(Search<Location.vTaxCalcMode, Where<Location.bAccountID, Equal<Current<vendorID>>,
			And<Location.locationID, Equal<Current<vendorLocationID>>>>>))]
		[TX.TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode
		{
			get { return this._TaxCalcMode; }
			set { this._TaxCalcMode = value; }
		}
		#endregion

		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(
			typeof(Search<Vendor.termsID, 
				Where2<FeatureInstalled<FeaturesSet.vendorRelations>, 
						And<Vendor.bAccountID, Equal<Current<POOrder.payToVendorID>>,
					Or2<Not<FeatureInstalled<FeaturesSet.vendorRelations>>,
						And<Vendor.bAccountID, Equal<Current<POOrder.vendorID>>>>>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.vendor>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		public virtual string TermsID { get; set; }
		#endregion

		#region RemitAddressID
		public abstract class remitAddressID : PX.Data.BQL.BqlInt.Field<remitAddressID> { }
		protected Int32? _RemitAddressID;
		[PXDBInt()]
		[PORemitAddress(typeof(Select2<Location,
			InnerJoin<Address, On<Address.bAccountID, Equal<Location.bAccountID>, And<Address.addressID, Equal<Location.defAddressID>>>,
			LeftJoin<PORemitAddress, On<PORemitAddress.bAccountID, Equal<Address.bAccountID>, 
						And<PORemitAddress.bAccountAddressID, Equal<Address.addressID>,
				And<PORemitAddress.revisionID, Equal<Address.revisionID>, And<PORemitAddress.isDefaultAddress, Equal<boolTrue>>>>>>>,
			Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>))]
		[PXUIField()]
		public virtual Int32? RemitAddressID
		{
			get
			{
				return this._RemitAddressID;
			}
			set
			{
				this._RemitAddressID = value;
			}
		}
		#endregion
		#region RemitContactID
		public abstract class remitContactID : PX.Data.BQL.BqlInt.Field<remitContactID> { }
		protected Int32? _RemitContactID;
		[PXDBInt()]
		[PORemitContact(typeof(Select2<Location,
		    InnerJoin<Contact, On<Contact.bAccountID, Equal<Location.bAccountID>, And<Contact.contactID, Equal<Location.defContactID>>>,
		    LeftJoin<PORemitContact, On<PORemitContact.bAccountID, Equal<Contact.bAccountID>, 
				And<PORemitContact.bAccountContactID, Equal<Contact.contactID>,
		        And<PORemitContact.revisionID, Equal<Contact.revisionID>, 
				And<PORemitContact.isDefaultContact, Equal<boolTrue>>>>>>>,
		    Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>))]
		public virtual Int32? RemitContactID
		{
			get
			{
				return this._RemitContactID;
			}
			set
			{
				this._RemitContactID = value;
			}
		}
		#endregion

		#region SOOrderType
		public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
		protected String _SOOrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXSelector(typeof(Search<SO.SOOrderType.orderType, Where<SO.SOOrderType.active, Equal<boolTrue>>>))]
		[PXUIField(DisplayName = "Sales Order Type", Visibility = PXUIVisibility.SelectorVisible)]		
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
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]		
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<SO.SOOrder.orderNbr, Where<SO.SOOrder.orderType, Equal<Current<POOrder.sOOrderType>>>>))]
		[PXFormula(typeof(Default<POOrder.sOOrderType>))]
		[PXUIField(DisplayName = "Sales Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]					
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
		
		#region BLType
		public abstract class bLType : PX.Data.BQL.BqlString.Field<bLType> { }
		protected String _BLType;
		[PXDBString(2, IsFixed = true)]
		public virtual String BLType
		{
			get
			{
				return this._BLType;
			}
			set
			{
				this._BLType = value;
			}
		}
		#endregion
		#region BLOrderNbr
		public abstract class bLOrderNbr : PX.Data.BQL.BqlString.Field<bLOrderNbr> { }
		protected String _BLOrderNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String BLOrderNbr
		{
			get
			{
				return this._BLOrderNbr;
			}
			set
			{
				this._BLOrderNbr = value;
			}
		}
		#endregion
		#region RQReqNbr
		public abstract class rQReqNbr : PX.Data.BQL.BqlString.Field<rQReqNbr> { }
		protected String _RQReqNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName="Requisition Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(PX.Objects.RQ.RQRequisition.reqNbr))]
		public virtual String RQReqNbr
		{
			get
			{
				return this._RQReqNbr;
			}
			set
			{
				this._RQReqNbr = value;
			}
		}
		#endregion
		#region OrderDesc
		public abstract class orderDesc : PX.Data.BQL.BqlString.Field<orderDesc> { }
		protected String _OrderDesc;
		[PXDBString(60, IsUnicode = true)]
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
		#region ShipDestType
		public abstract class shipDestType : PX.Data.BQL.BqlString.Field<shipDestType> { }
		protected String _ShipDestType;
		[PXDBString(1, IsFixed = true)]
		[POShippingDestination.List()]
		[PXFormula(typeof(Switch<
			Case<Where<Current<POOrder.orderType>, Equal<POOrderType.dropShip>>, POShippingDestination.customer, 
			Case<Where<Selector<POOrder.vendorLocationID, Location.vSiteID>, IsNotNull>, POShippingDestination.site>>, 
			POShippingDestination.company>))]
		[PXUIField(DisplayName = "Shipping Destination Type")]
		public virtual String ShipDestType
		{
			get
			{
				return this._ShipDestType;
			}
			set
			{
				this._ShipDestType = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;

		[Site(DescriptionField = typeof(INSite.descr))]
		[PXDefault((object)null, typeof(Coalesce<Search<Location.vSiteID, Where<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
													And<Location.bAccountID, Equal<Current<POOrder.vendorID>>,
													And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>>,
										Search<INSite.siteID, Where<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
													And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.Site))]
		[InterBranchRestrictor(typeof(Where2<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>,
			Or<Current<POOrder.orderType>, Equal<POOrderType.standardBlanket>>>))]
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
        #region SiteIdErrorMessage
        public abstract class siteIdErrorMessage : PX.Data.BQL.BqlString.Field<siteIdErrorMessage> { }
        [PXString(150, IsUnicode = true)]
        public virtual string SiteIdErrorMessage { get; set; }
        #endregion
		#region ShipToBAccountID
		public abstract class shipToBAccountID : PX.Data.BQL.BqlInt.Field<shipToBAccountID> { }
		protected Int32? _ShipToBAccountID;
		[PXDBInt()]
		[PXSelector(typeof(
			Search2<BAccount2.bAccountID,
			LeftJoin<Vendor, On<
				Vendor.bAccountID, Equal<BAccount2.bAccountID>,
				And<Match<Vendor, Current<AccessInfo.userName>>>>,
			LeftJoin<AR.Customer, On<
				AR.Customer.bAccountID, Equal<BAccount2.bAccountID>,
				And<Match<AR.Customer, Current<AccessInfo.userName>>>>,
			LeftJoin<GL.Branch, On<
				GL.Branch.bAccountID, Equal<BAccount2.bAccountID>,
				And<Match<GL.Branch, Current<AccessInfo.userName>>>>>>>,
			Where<
				Vendor.bAccountID, IsNotNull, And<Optional<POOrder.shipDestType>, Equal<POShippingDestination.vendor>,
					And2<Where<BAccount2.type, In3<BAccountType.vendorType, BAccountType.combinedType>, Or<Optional<POOrder.orderType>, NotEqual<POOrderType.dropShip>>>,
			Or<Where<GL.Branch.bAccountID, IsNotNull, And<Optional<POOrder.shipDestType>, Equal<POShippingDestination.company>,
				Or<Where<AR.Customer.bAccountID, IsNotNull, And<Optional<POOrder.shipDestType>, Equal<POShippingDestination.customer>>>>>>>>>>>),
				typeof(BAccount.acctCD), typeof(BAccount.acctName), typeof(BAccount.type), typeof(BAccount.acctReferenceNbr), typeof(BAccount.parentBAccountID),
			SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName), CacheGlobal = true)]
		[PXUIField(DisplayName = "Ship To")]
		[PXDefault((object)null, typeof(Search<GL.Branch.bAccountID, Where<GL.Branch.branchID, Equal<Current<POOrder.branchID>>, And<Optional<POOrder.shipDestType>, Equal<POShippingDestination.company>>>>),PersistingCheck=PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<POOrder.shipToBAccountID>.IsRelatedTo<BAccount.bAccountID>))]
		public virtual Int32? ShipToBAccountID
		{
			get
			{
				return this._ShipToBAccountID;
			}
			set
			{
				this._ShipToBAccountID = value;
			}
		}
		#endregion
		#region ShipToLocationID
		public abstract class shipToLocationID : PX.Data.BQL.BqlInt.Field<shipToLocationID> { }
		protected Int32? _ShipToLocationID;

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POOrder.shipToBAccountID>>>), DescriptionField = typeof(Location.descr))]
		[PXDefault((object)null, 
			typeof(Search<BAccount2.defLocationID,
					Where<BAccount2.bAccountID, Equal<Optional<POOrder.shipToBAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Shipping Location")]
		[PXForeignReference(typeof(Field<shipToLocationID>.IsRelatedTo<Location.locationID>))]
		public virtual Int32? ShipToLocationID
		{
			get
			{
				return this._ShipToLocationID;
			}
			set
			{
				this._ShipToLocationID = value;
			}
		}
		#endregion

		#region ShipAddressID
		public abstract class shipAddressID : PX.Data.BQL.BqlInt.Field<shipAddressID> { }
		protected Int32? _ShipAddressID;
		[PXDBInt()]
		[POShipAddress(typeof(Select2<Address,
					InnerJoin<CRLocation, On<Address.bAccountID, Equal<CRLocation.bAccountID>,
						And<Address.addressID, Equal<CRLocation.defAddressID>,
						And<Current<POOrder.shipDestType>, NotEqual<POShippingDestination.site>,
						And<CRLocation.bAccountID, Equal<Current<POOrder.shipToBAccountID>>,
						And<CRLocation.locationID, Equal<Current<POOrder.shipToLocationID>>>>>>>,
					LeftJoin<POShipAddress, On<POShipAddress.bAccountID, Equal<Address.bAccountID>,
						And<POShipAddress.bAccountAddressID, Equal<Address.addressID>,
						And<POShipAddress.revisionID, Equal<Address.revisionID>,
						And<POShipAddress.isDefaultAddress, Equal<boolTrue>>>>>>>,
					Where<True, Equal<True>>>))]
		[PXUIField()]
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
		[POShipContact(typeof(Select2<Contact,
					InnerJoin<CRLocation, On<Contact.bAccountID, Equal<CRLocation.bAccountID>,
						And<Contact.contactID, Equal<CRLocation.defContactID>,
						And<Current<POOrder.shipDestType>, NotEqual<POShippingDestination.site>,
						And<CRLocation.bAccountID, Equal<Current<POOrder.shipToBAccountID>>,
						And<CRLocation.locationID, Equal<Current<POOrder.shipToLocationID>>>>>>>,
					LeftJoin<POShipContact, On<POShipContact.bAccountID, Equal<Contact.bAccountID>,
						And<POShipContact.bAccountContactID, Equal<Contact.contactID>,
						And<POShipContact.revisionID, Equal<Contact.revisionID>,
						And<POShipContact.isDefaultContact, Equal<boolTrue>>>>>>>,
					Where<True, Equal<True>>>))]
		[PXUIField()]
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
		#region VendorID_Vendor_acctName
		public abstract class vendorID_Vendor_acctName : PX.Data.BQL.BqlString.Field<vendorID_Vendor_acctName> { }
		#endregion

		#region OpenOrderQty
		public abstract class openOrderQty : PX.Data.BQL.BqlDecimal.Field<openOrderQty> { }
		[PXDBQuantity]
		[PXUIField(DisplayName = "Open Quantity", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OpenOrderQty
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledOrderTotal
		public abstract class curyUnbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledOrderTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.unbilledOrderTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Amount", Enabled = false)]
		public virtual Decimal? CuryUnbilledOrderTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderTotal
		public abstract class unbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<unbilledOrderTotal> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledOrderTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledLineTotal
		public abstract class curyUnbilledLineTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledLineTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.unbilledLineTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Line Total", Enabled = false)]
		public virtual Decimal? CuryUnbilledLineTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledLineTotal
		public abstract class unbilledLineTotal : PX.Data.BQL.BqlDecimal.Field<unbilledLineTotal> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledLineTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledTaxTotal
		public abstract class curyUnbilledTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledTaxTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.unbilledTaxTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Tax Total", Enabled = false)]
		public virtual Decimal? CuryUnbilledTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledTaxTotal
		public abstract class unbilledTaxTotal : PX.Data.BQL.BqlDecimal.Field<unbilledTaxTotal> { }
		protected Decimal? _OpenTaxTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderQty
		public abstract class unbilledOrderQty : PX.Data.BQL.BqlDecimal.Field<unbilledOrderQty> { }
		[PXDBQuantity()]
		[PXUIField(DisplayName = "Unbilled Quantity")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledOrderQty
		{
			get;
			set;
		}
		#endregion

		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXDBInt()]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
		[PXSubordinateSelector]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region OwnerWorkgroupID
		public abstract class ownerWorkgroupID : PX.Data.BQL.BqlInt.Field<ownerWorkgroupID> { }
		protected int? _OwnerWorkgroupID;
		[PXDBInt]
		[PXSelector(typeof(Search5<EPCompanyTree.workGroupID,
			InnerJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.workGroupID, Equal<EPCompanyTree.workGroupID>>,
			InnerJoin<EPEmployee, On<EPCompanyTreeMember.userID, Equal<EPEmployee.userID>>>>,
			Where<EPEmployee.bAccountID, Equal<Current<POOrder.employeeID>>>,
			Aggregate<GroupBy<EPCompanyTree.workGroupID, GroupBy<EPCompanyTree.description>>>>), 
			SubstituteKey = typeof(EPCompanyTree.description))]
		[PXUIField(DisplayName = "Workgroup ID", Enabled = false)]
		public virtual int? OwnerWorkgroupID
		{
			get
			{
				return this._OwnerWorkgroupID;
			}
			set
			{
				this._OwnerWorkgroupID = value;
			}
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;
		[PXInt]		
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID>), SubstituteKey = typeof(EPCompanyTree.description))]
		[PXUIField(DisplayName = "Approval Workgroup ID", Enabled=false)]
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
		[PXGuid()]		
		[PX.TM.PXOwnerSelector]
		[PXUIField(DisplayName = "Approver", Enabled = false)]
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
		#region DontPrint
		public abstract class dontPrint : PX.Data.BQL.BqlBool.Field<dontPrint> { }
		protected Boolean? _DontPrint;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Don't Print")]
		public virtual Boolean? DontPrint
		{
			get
			{
				return this._DontPrint;
			}
			set
			{
				this._DontPrint = value;
			}
		}
		#endregion
		#region Printed
		public abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
		protected Boolean? _Printed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Printed")]
		public virtual Boolean? Printed
		{
			get
			{
				return this._Printed;
			}
			set
			{
				this._Printed = value;
			}
		}
		#endregion
		#region DontEmail
		public abstract class dontEmail : PX.Data.BQL.BqlBool.Field<dontEmail> { }
		protected Boolean? _DontEmail;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Don't Email")]
		public virtual Boolean? DontEmail
		{
			get
			{
				return this._DontEmail;
			}
			set
			{
				this._DontEmail = value;
			}
		}
		#endregion
		#region Emailed
		public abstract class emailed : PX.Data.BQL.BqlBool.Field<emailed> { }
		protected Boolean? _Emailed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Emailed")]
		public virtual Boolean? Emailed
		{
			get
			{
				return this._Emailed;
			}
			set
			{
				this._Emailed = value;
			}
		}
		#endregion				
		#region PrintedExt
		public abstract class printedExt : PX.Data.BQL.BqlBool.Field<printedExt> { }
		[PXBool()]
		public virtual Boolean? PrintedExt
		{
			[PXDependsOnFields(typeof(dontPrint),typeof(printed))]
			get
			{
				return this._DontPrint == true || this._Printed == true;
			}
		}
		#endregion
		#region EmailedExt
		public abstract class emailedExt : PX.Data.BQL.BqlBool.Field<emailedExt> { }
		[PXBool()]
		public virtual Boolean? EmailedExt
		{
			[PXDependsOnFields(typeof(dontEmail),typeof(emailed))]
			get
			{
				return this._DontEmail == true || this._Emailed == true;
			}
		}
		#endregion
		#region FOBPoint
		public abstract class fOBPoint : PX.Data.BQL.BqlString.Field<fOBPoint> { }
		protected String _FOBPoint;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "FOB Point")]
		[PXSelector(typeof(Search<FOBPoint.fOBPointID>), DescriptionField = typeof(FOBPoint.description), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.vFOBPointID, 
		             Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>,
							And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXSelector(typeof(Search<Carrier.carrierID>), CacheGlobal = true)]
		[PXDefault(typeof(Search<Location.vCarrierID, 
							Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, 
				            And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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

		#region PayToVendorID
		public abstract class payToVendorID : PX.Data.BQL.BqlInt.Field<payToVendorID> { }
		/// <summary>
		/// A reference to the <see cref="Vendor"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the vendor, whom the AP bill will belong to. 
		/// </value>
		[PXFormula(typeof(Validate<POOrder.curyID>))]
		[POOrderPayToVendor(CacheGlobal = true, Filterable = true)]
		[PXDefault]
		[PXForeignReference(typeof(Field<POOrder.payToVendorID>.IsRelatedTo<Vendor.bAccountID>))]
		public virtual int? PayToVendorID { get; set; }
		#endregion
		#region PrepaymentPct
		public abstract class prepaymentPct : Data.BQL.BqlDecimal.Field<prepaymentPct>
		{
		}
		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Prepayment Percent")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? PrepaymentPct
		{
			get;
			set;
		}
		#endregion

		#region OrderWeight
		public abstract class orderWeight : PX.Data.BQL.BqlDecimal.Field<orderWeight> { }
		protected Decimal? _OrderWeight;
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Weight", Visible = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXUIField(DisplayName = "Volume", Visible=false)]
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

		#region LockCommitment
		public abstract class lockCommitment : PX.Data.BQL.BqlBool.Field<lockCommitment> { }
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? LockCommitment
		{
			get;
			set;
		}
		#endregion

		#region RetainageApply
		public abstract class retainageApply : PX.Data.BQL.BqlBool.Field<retainageApply> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Apply Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXFormula(typeof(Switch<
			Case<Where<Current<POOrder.orderType>, Equal<POOrderType.dropShip>>, boolFalse>,
			Selector<POOrder.vendorID, Vendor.retainageApply>>))]
		public virtual bool? RetainageApply
		{
			get;
			set;
		}
		#endregion
		#region DefRetainagePct
		public abstract class defRetainagePct : PX.Data.BQL.BqlDecimal.Field<defRetainagePct> { }
		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Retainage Percent", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXFormula(typeof(Selector<POOrder.vendorID, Vendor.retainagePct>))]
		public virtual decimal? DefRetainagePct
		{
			get;
			set;
		}
		#endregion
		#region CuryLineRetainageTotal
		public abstract class curyLineRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyLineRetainageTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.lineRetainageTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryLineRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region LineRetainageTotal
		public abstract class lineRetainageTotal : PX.Data.BQL.BqlDecimal.Field<lineRetainageTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? LineRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainedTaxTotal
		public abstract class curyRetainedTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.retainedTaxTotal))]
		[PXUIField(DisplayName = "Tax on Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryRetainedTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainedTaxTotal
		public abstract class retainedTaxTotal : PX.Data.BQL.BqlDecimal.Field<retainedTaxTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RetainedTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainedDiscTotal
		public abstract class curyRetainedDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainedDiscTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.retainedDiscTotal))]
		[PXUIField(DisplayName = "Discount on Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryRetainedDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainedDiscTotal
		public abstract class retainedDiscTotal : PX.Data.BQL.BqlDecimal.Field<retainedDiscTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RetainedDiscTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainageTotal
		public abstract class curyRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainageTotal> { }
		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.retainageTotal))]
		[PXUIField(DisplayName = "Retainage Total", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainageTotal
		public abstract class retainageTotal : PX.Data.BQL.BqlDecimal.Field<retainageTotal> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RetainageTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryPrepaidTotal
		public abstract class curyPrepaidTotal : Data.BQL.BqlDecimal.Field<curyPrepaidTotal>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(prepaidTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Prepayment Total", Enabled = false)]
		public virtual decimal? CuryPrepaidTotal
		{
			get;
			set;
		}
		#endregion
		#region PrepaidTotal
		public abstract class prepaidTotal : Data.BQL.BqlDecimal.Field<prepaidTotal>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? PrepaidTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryUnprepaidTotal
		public abstract class curyUnprepaidTotal : Data.BQL.BqlDecimal.Field<curyUnprepaidTotal>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(unprepaidTotal))]
		[PXFormula(typeof(Maximum<decimal0, Sub<curyUnbilledOrderTotal, curyPrepaidTotal>>))]
		[PXUIField(DisplayName = "Unpaid Amount", Enabled = false)]
		public virtual decimal? CuryUnprepaidTotal
		{
			get;
			set;
		}
		#endregion
		#region UnprepaidTotal
		public abstract class unprepaidTotal : Data.BQL.BqlDecimal.Field<unprepaidTotal>
		{
		}
		[PXDBBaseCury]
		public virtual decimal? UnprepaidTotal
		{
			get;
			set;
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[ProjectDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInPO, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBase]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion

		#region UpdateVendorCost
		public abstract class updateVendorCost : PX.Data.BQL.BqlBool.Field<updateVendorCost> { }
		[PXBool]
		[PXFormula(typeof(boolTrue))]
		public virtual Boolean? UpdateVendorCost
		{
			get;
			set;
		}
		#endregion
		
		#region DisableAutomaticDiscountCalculation
		public abstract class disableAutomaticDiscountCalculation : PX.Data.BQL.BqlBool.Field<disableAutomaticDiscountCalculation> { }
		protected Boolean? _DisableAutomaticDiscountCalculation;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable Automatic Discount Update")]
		public virtual Boolean? DisableAutomaticDiscountCalculation
		{
			get { return this._DisableAutomaticDiscountCalculation; }
			set { this._DisableAutomaticDiscountCalculation = value; }
		}
		#endregion

		#region OrderBasedAPBill
		public abstract class orderBasedAPBill : PX.Data.BQL.BqlBool.Field<orderBasedAPBill> { }
		[PXBool]
		[PXFormula(typeof(Selector<POOrder.vendorLocationID, Location.vAllowAPBillBeforeReceipt>))]
		[PXUIField(DisplayName = "Allow AP Bill Before Receipt", Enabled = false)]
		public virtual bool? OrderBasedAPBill
		{
			get;
			set;
		}
		#endregion
		#region POAccrualType
		public abstract class pOAccrualType : PX.Data.BQL.BqlString.Field<pOAccrualType> { }
		[PXString(1, IsFixed = true)]
		[PXFormula(typeof(Switch<Case<Where<POOrder.orderBasedAPBill, Equal<True>>, POAccrualType.order>, POAccrualType.receipt>))]
		[POAccrualType.List]
		[PXUIField(DisplayName = "Billing Based On", Enabled = false)]
		public virtual string POAccrualType
		{
			get;
			set;
		}
		#endregion
		#region LinesStatusUpdated
		public abstract class linesStatusUpdated : PX.Data.BQL.BqlBool.Field<linesStatusUpdated> { }
		[PXBool()]
		public virtual bool? LinesStatusUpdated
		{
			get;
			set;
		}
		#endregion
		#region HasUsedLine
		public abstract class hasUsedLine : PX.Data.BQL.BqlBool.Field<hasUsedLine> { }
		[PXBool()]
		public virtual bool? HasUsedLine
		{
			get;
			set;
		}
		#endregion


		#region IAssign Members

		int? PX.Data.EP.IAssign.WorkgroupID
		{
			get { return WorkgroupID; }
			set { WorkgroupID = value; }
		}

		Guid? PX.Data.EP.IAssign.OwnerID
		{
			get { return OwnerID; }
			set { OwnerID = value; }
		}


		#endregion		
	}

	public class POOrderType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder),
					Pair(DropShip, Messages.DropShip),
					Pair(Blanket, Messages.Blanket),
					Pair(StandardBlanket, Messages.StandardBlanket),
				}) {}
		}

		public class PrintListAttribute : PXStringListAttribute
		{
			public PrintListAttribute() : base(
				new[]
				{
					Pair(Blanket, Messages.PrintBlanket),
					Pair(StandardBlanket, Messages.PrintStandardBlanket),
					Pair(RegularOrder, Messages.PrintRegularOrder),
					Pair(DropShip, Messages.PrintDropShip),
				}) {}
		}

		public class StatdardBlanketList : PXStringListAttribute
		{
			public StatdardBlanketList() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder),
					Pair(Blanket, Messages.Blanket),
					Pair(StandardBlanket, Messages.StandardBlanket),
				}) {}
		}

		public class StatdardAndRegularList : PXStringListAttribute
		{
			public StatdardAndRegularList() : base(
				new[]
				{
					Pair(StandardBlanket, Messages.StandardBlanket),
					Pair(RegularOrder, Messages.RegularOrder),
				}) {}
		}

		public class BlanketList : PXStringListAttribute
		{
			public BlanketList() : base(
				new[]
				{
					Pair(Blanket, Messages.Blanket),
					Pair(StandardBlanket, Messages.StandardBlanket),
				}) {}
		}

		/// <summary>
		/// Selector. Provides list of "Regular" Purchase Orders types.
		/// Include RegularOrder, DropShip.
		/// </summary>
		public class RegularDropShipListAttribute : PXStringListAttribute
		{
			public RegularDropShipListAttribute() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder),
					Pair(DropShip, Messages.DropShip),
				}) {}
		}

		/// <summary>
		/// Selector. Defines a list of Purchase Order types, which are allowed <br/>
		/// for use in the SO module: RegularOrder, Blanket, DropShip, Transfer.<br/>
		/// </summary>
		public class RBDListAttribute : PXStringListAttribute
		{
			public RBDListAttribute() : base(
				new[]
				{
					Pair(RegularOrder, Messages.RegularOrder),
					Pair(Blanket, Messages.Blanket),
					Pair(DropShip, Messages.DropShip),
				}) {}
		}

		//public const string Transfer = "TR";
		public const string Blanket = "BL";
		public const string StandardBlanket = "SB";
		public const string RegularOrder = "RO";
		public const string RegularSubcontract = "RS";//Reserved for Construction
		public const string DropShip = "DP";

		public class blanket : PX.Data.BQL.BqlString.Constant<blanket>
		{
			public blanket() : base(Blanket) { }
		}

		public class standardBlanket : PX.Data.BQL.BqlString.Constant<standardBlanket>
		{
			public standardBlanket() : base(StandardBlanket) { }
		}

		public class regularOrder : PX.Data.BQL.BqlString.Constant<regularOrder>
		{
			public regularOrder() : base(RegularOrder) { }
		}

		//Reserved for Construction
		public class regularSubcontract : PX.Data.BQL.BqlString.Constant<regularSubcontract>
		{
			public regularSubcontract() : base(RegularSubcontract) { }
		}

		public class dropShip : PX.Data.BQL.BqlString.Constant<dropShip>
		{
			public dropShip() : base(DropShip) { }
		}
		/*
		public class transfer : PX.Data.BQL.BqlString.Constant<transfer>
		{
			public transfer() : base(Transfer) { }
		}
        */
		public static bool IsUseBlanket(string orderType)
		{
			return orderType == RegularOrder || orderType == DropShip;
		}

		public static bool IsNormalType(string orderType)
		{
			return orderType == RegularOrder || orderType == RegularSubcontract;
		}
	}

	public class POOrderStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Hold, Messages.Hold),
					Pair(Balanced, EP.Messages.Balanced),
					Pair(Voided, EP.Messages.Voided),
					Pair(Open, Messages.Open),
					Pair(PendingPrint, Messages.PendingPrint),
					Pair(PendingEmail, Messages.PendingEmail),
					Pair(Cancelled, Messages.Cancelled),
					Pair(Completed, Messages.Completed),
					Pair(Closed, Messages.Closed),
					Pair(Printed, Messages.Printed),
				}) {}
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Voided = "V";
		public const string Open = "N";
		public const string PendingPrint = "D";
		public const string PendingEmail = "E";
		public const string Completed = "M";
		public const string Closed = "C";
		public const string Printed = "P";
		public const string Cancelled = "L";

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { }
		}

		public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
		{
			public balanced() : base(Balanced) { }
		}

		public class voided : PX.Data.BQL.BqlString.Constant<voided>
		{
			public voided() : base(Voided) { }
		}

		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) { }
		}

		public class completed : Data.BQL.BqlString.Constant<completed>
		{
			public completed() : base(Completed) { }
		}

		public class closed : PX.Data.BQL.BqlString.Constant<closed>
		{
			public closed() : base(Closed) { }
		}

		public class printed : PX.Data.BQL.BqlString.Constant<printed>
		{
			public printed() : base(Printed) { }
		}
		public class pendingPrint : PX.Data.BQL.BqlString.Constant<pendingPrint>
		{
			public pendingPrint() : base(PendingPrint) { }
		}
		public class cancelled : PX.Data.BQL.BqlString.Constant<cancelled>
		{
			public cancelled() : base(Cancelled) { }
		}
	}

	public class POShippingDestination
	{
		public const string CompanyLocation = "L";
		public const string Customer = "C";
		public const string Vendor = "V";
		public const string Site = "S";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(CompanyLocation, Messages.ShipDestCompanyLocation),
					Pair(Customer, Messages.ShipDestCustomer),
					Pair(Vendor, Messages.ShipDestVendor),
					Pair(Site, Messages.ShipDestSite),
				}) {}
		}

		public class company : PX.Data.BQL.BqlString.Constant<company>
		{
			public company() : base(CompanyLocation) { }
		}

		public class customer : PX.Data.BQL.BqlString.Constant<customer>
		{
			public customer() : base(Customer) { }
		}

		public class vendor : PX.Data.BQL.BqlString.Constant<vendor>
		{
			public vendor() : base(Vendor) { }
		}

		public class site : PX.Data.BQL.BqlString.Constant<site>
		{
			public site() : base(Site) { }
		}


	}

	public class POBehavior
	{
		public const string Standard = "S";
		public const string ChangeOrder = "C";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
			{
					Pair(Standard, Messages.POBehavior_Standard),
					Pair(ChangeOrder, Messages.POBehavior_ChangeOrder)
			}
				)
			{ }
		}

		public class standard : PX.Data.BQL.BqlString.Constant<standard>
		{
			public standard() : base(Standard) { }
		}

		public class changeOrder : PX.Data.BQL.BqlString.Constant<changeOrder>
		{
			public changeOrder() : base(ChangeOrder) { }
		}
	}

	public class PO
	{
        /// <summary>
        /// Specialized selector for POOrder RefNbr.<br/>
        /// By default, defines the following set of columns for the selector:<br/>
        /// POOrder.orderType, orderNbr, orderDate,<br/>
		/// status,vendorID, vendorID_Vendor_acctName,<br/>
		/// vendorLocationID, curyID, curyOrderTotal,<br/>
		/// vendorRefNbr, sOOrderType, sOOrderNbr<br/>
        /// <example>
        /// [PO.RefNbr(typeof(Search2<POOrder.orderNbr,
		///   LeftJoin<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>,
		///	   And<Match<Vendor, Current<AccessInfo.userName>>>>>,
		///	  Where<POOrder.orderType, Equal<Optional<POOrder.orderType>>,
		///	   And<Where<POOrder.orderType, Equal<POOrderType.transfer>,
		///	   Or<Vendor.bAccountID, IsNotNull>>>>>), Filterable = true)]
        /// </example>
        /// </summary>
		public class RefNbrAttribute : PXSelectorAttribute
		{
            /// <summary>
            /// Default Ctor
            /// </summary>
            /// <param name="SearchType"> Must be IBqlSearch type, pointing to POOrder.refNbr</param>
			public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(POOrder.orderType),
				typeof(POOrder.orderNbr),
                typeof(POOrder.vendorRefNbr),
                typeof(POOrder.orderDate),
				typeof(POOrder.status),
				typeof(POOrder.vendorID),
				typeof(POOrder.vendorID_Vendor_acctName),
				typeof(POOrder.vendorLocationID),
				typeof(POOrder.curyID),
				typeof(POOrder.curyOrderTotal),
				typeof(POOrder.sOOrderType),
				typeof(POOrder.sOOrderNbr))
			{                
			}
		}

        /// <summary>
        /// Specialized version of the AutoNumber attribute for POOrders<br/>
        /// It defines how the new numbers are generated for the PO Order. <br/>
        /// References POOrder.docType and POOrder.docDate fields of the document,<br/>
        /// and also define a link between  numbering ID's defined in PO Setup and POOrder types:<br/>
        /// namely POSetup.regularPONumberingID, POSetup.regularPONumberingID for POOrderType.RegularOrder, POOrderType.DropShip, and POOrderType.StandardBlanket,<br/>
        /// and POSetup.standardPONumberingID for POOrderType.Blanket and POOrderType.Transfer<br/>
        /// </summary>		
		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(POOrder.orderType), typeof(POOrder.orderDate),
					new string[] { POOrderType.RegularOrder, POOrderType.DropShip, POOrderType.StandardBlanket, POOrderType.Blanket },
					new Type[] { typeof(POSetup.regularPONumberingID), typeof(POSetup.regularPONumberingID), typeof(POSetup.regularPONumberingID), typeof(POSetup.standardPONumberingID) }) { ; }
		}
	}
}