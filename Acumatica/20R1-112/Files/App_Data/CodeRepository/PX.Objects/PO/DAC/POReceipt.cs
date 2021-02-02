using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.PO
{
	using System;
	using PX.Data;
	using PX.Objects.AP;
	using PX.Objects.CS;
	using PX.Objects.CR;
	using PX.Objects.CM;
	using PX.Objects.IN;
	using PX.Objects.GL;
	using PX.Objects.PM;
	using CRLocation = PX.Objects.CR.Standalone.Location;
	using PX.Objects.Common.Bql;
	using PX.Objects.Common;

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(POReceiptEntry))]
	[PXEMailSource]
	[PXCacheName(Messages.POReceipt)]
	public partial class POReceipt : PX.Data.IBqlTable, PX.Data.EP.IAssign
	{
		#region Keys
		public class PK : PrimaryKeyOf<POReceipt>.By<receiptNbr>
		{
			public static POReceipt Find(PXGraph graph, string receiptNbr) => FindBy(graph, receiptNbr);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<POReceipt>.By<siteID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<POReceipt>.By<vendorID> { }
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
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		protected String _ReceiptType;
		[PXDBString(2, IsFixed = true, IsKey = true,InputMask="")]
		[PXDefault(POReceiptType.POReceipt)]
		[POReceiptType.List()]
		[PXUIField(DisplayName = "Type")]
        [PX.Data.EP.PXFieldDescription]
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
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr>
		{
			public const string DisplayName = "Receipt Nbr.";
		}
		protected String _ReceiptNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[POReceiptType.Numbering()]
		[POReceiptType.RefNbr(typeof(Search2<POReceipt.receiptNbr,
			LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<POReceipt.vendorID>>>,
			Where<POReceipt.receiptType, Equal<Optional<POReceipt.receiptType>>,
			  And<Where<Vendor.bAccountID, IsNull,
				 Or<Match<Vendor, Current<AccessInfo.userName>>>>>>,
			OrderBy<Desc<POReceipt.receiptNbr>>>), Filterable = true)]
		[PXUIField(DisplayName = receiptNbr.DisplayName, Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        [IN.Site(DisplayName = "Warehouse", Required = true)]
		[PXForeignReference(typeof(FK.Site))]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>>))]
		public int? SiteID
        {
            get;
            set;
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor(
			typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
			Visibility = PXUIVisibility.SelectorVisible, 
			CacheGlobal = true, 
			Filterable = true)]
		[VerndorNonEmployeeOrOrganizationRestrictor(typeof(POReceipt.receiptType))]
		[PXRestrictor(
			typeof(Where<
				Vendor.status, IsNull, 
				Or<Vendor.status, Equal<BAccount.status.active>, 
				Or<Vendor.status, Equal<BAccount.status.oneTime>>>>), 
			AP.Messages.VendorIsInStatus, 
			typeof(Vendor.status))]
		[PXDefault]	
		[PXFormula(typeof(Switch<Case<Where<POReceipt.receiptType, Equal<POReceiptType.transferreceipt>>, Selector<POReceipt.siteID, Selector<INSite.branchID, Branch.branchCD>>>, POReceipt.vendorID>))]
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
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POReceipt.vendorID>>,
			And<Location.isActive, Equal<True>,
			And<MatchWithBranch<Location.vBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
			Where<BAccountR.bAccountID, Equal<Current<POReceipt.vendorID>>,
				And<CRLocation.isActive, Equal<True>,
				And<MatchWithBranch<CRLocation.vBranchID>>>>>,
			Search<CRLocation.locationID,
			Where<CRLocation.bAccountID, Equal<Current<POReceipt.vendorID>>,
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
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        protected Int32? _BranchID;
        [GL.Branch(typeof(Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>), IsDetail = false)]
        [PXFormula(typeof(Switch<Case<Where<POReceipt.receiptType, Equal<POReceiptType.transferreceipt>>, Selector<POReceipt.siteID, Selector<INSite.branchID, Branch.branchCD>>, Case<Where<Selector<POReceipt.vendorLocationID, Location.vBranchID>, IsNotNull>, Selector<POReceipt.vendorLocationID, Selector<Location.vBranchID, Branch.branchCD>>>>, POReceipt.branchID>))]
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
		#region ReceiptDate
		public abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate>
		{
			public const string DisplayName = "Date";
		}
		protected DateTime? _ReceiptDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = receiptDate.DisplayName, Visibility = PXUIVisibility.SelectorVisible)]
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
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		protected DateTime? _InvoiceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Bill Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(POReceipt.receiptDate))]
		[PXDefault(typeof(POReceipt.receiptDate),PersistingCheck = PXPersistingCheck.Nothing)]
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
		[POOpenPeriod(
			sourceType: typeof(POReceipt.receiptDate), 
			branchSourceType: typeof(POReceipt.branchID),
			masterFinPeriodIDType: typeof(POReceipt.tranPeriodID),
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
	    public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID>
	    {
	    }
		
		[PeriodID]
	    public virtual String TranPeriodID { get; set; }
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true, typeof(Search<POSetup.holdReceipts>))]		
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
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXUIField(DisplayName = "Released")]
		[PXDefault(false,PersistingCheck =PXPersistingCheck.Nothing)]
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
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(POReceiptStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[POReceiptStatus.List()]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.PO, "{0}: {1} - {3}", new Type[] { typeof(POReceipt.pOType), typeof(POReceipt.receiptNbr), typeof(POReceipt.vendorID), typeof(Vendor.acctName) },
		   new Type[] { typeof(POReceipt.invoiceNbr) },
		   NumberFields = new Type[] { typeof(POReceipt.receiptNbr) },
		   Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(POReceipt.receiptDate), typeof(POReceipt.status), typeof(POReceipt.invoiceNbr) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(POReceipt.orderQty) },
		   MatchWithJoin = typeof(InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POReceipt.vendorID>>>),
		   SelectForFastIndexing = typeof(Select2<POReceipt, InnerJoin<Vendor, On<POReceipt.vendorID, Equal<Vendor.bAccountID>>>>)
		)]
		[PXNote(new Type[0], ShowInReferenceSelector = true, Selector = typeof(
			Search2<
				POReceipt.receiptNbr,
			LeftJoinSingleTable<Vendor, 
				On<Vendor.bAccountID, Equal<POReceipt.vendorID>>>,
			Where<
				Vendor.bAccountID, IsNull,
				Or<Match<Vendor, Current<AccessInfo.userName>>>>,
			OrderBy<
				Desc<POReceipt.receiptNbr>>>))]
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
		[CurrencyInfo(ModuleCode = BatchModule.PO)]
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
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Qty.")]
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
		#region ControlQty
		public abstract class controlQty : PX.Data.BQL.BqlDecimal.Field<controlQty> { }
		protected Decimal? _ControlQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName="Control Qty.")]
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
		#region APDocType
		public abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }
		protected String _APDocType;
		[PXDBString(3,IsFixed = true)]
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[APDocType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]		
		public virtual String APDocType
		{
			get
			{
				return this._APDocType;
			}
			set
			{
				this._APDocType = value;
			}
		}
		#endregion
		#region APRefNbr
		public abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr> { }
		protected String _APRefNbr;
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[APInvoiceType.RefNbr(typeof(Search2<AP.Standalone.APRegisterAlias.refNbr,
			InnerJoinSingleTable<APInvoice, On<APInvoice.docType, Equal<AP.Standalone.APRegisterAlias.docType>,
				And<APInvoice.refNbr, Equal<AP.Standalone.APRegisterAlias.refNbr>>>,
			InnerJoin<Vendor, On<AP.Standalone.APRegisterAlias.vendorID, Equal<Vendor.bAccountID>>>>,
			Where<AP.Standalone.APRegisterAlias.docType, Equal<Current<POReceipt.aPDocType>>,
			And<Vendor.bAccountID,Equal<Current<POReceipt.vendorID>>>>>), Filterable = true)]
		
		public virtual String APRefNbr
		{
			get
			{
				return this._APRefNbr;
			}
			set
			{
				this._APRefNbr = value;
			}
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
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<POReceipt.invtDocType>>>>))]
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
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vendor Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		[POVendorRefNbr()]
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
		#region AutoCreateInvoice
		public abstract class autoCreateInvoice : PX.Data.BQL.BqlBool.Field<autoCreateInvoice> { }
		protected Boolean? _AutoCreateInvoice;
		[PXDBBool()]
		[PXUIField(DisplayName = "Create Bill", Visibility = PXUIVisibility.Visible)]
        [PXFormula(typeof(Switch<Case<Where<POReceipt.receiptType, Equal<POReceiptType.transferreceipt>>, False>, Current<POSetup.autoCreateInvoiceOnReceipt>>))]
        [PXDefault()]
		public virtual Boolean? AutoCreateInvoice
		{
			get
			{
				return this._AutoCreateInvoice;
			}
			set
			{
				this._AutoCreateInvoice = value;
			}
		}
		#endregion
		#region ReturnOrigCost
		public abstract class returnOrigCost : PX.Data.BQL.BqlBool.Field<returnOrigCost> { }
		[PXDBBool]
		[PXDefault(typeof(Switch<Case<Where<POReceipt.receiptType, Equal<POReceiptType.poreturn>>, Current<POSetup.returnOrigCost>>, False>))]
		[PXUIField(DisplayName = "Process Return with Original Cost")]
		[PXUIEnabled(typeof(Where<POReceipt.receiptType, Equal<POReceiptType.poreturn>, And<POReceipt.released, Equal<False>>>))]
		public virtual bool? ReturnOrigCost
		{
			get;
			set;
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
		#region VendorID_Vendor_acctName
		public abstract class vendorID_Vendor_acctName : PX.Data.BQL.BqlString.Field<vendorID_Vendor_acctName> { }
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;
		[PXDBInt]
		[PXDefault(typeof(Vendor.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PX.TM.PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
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
		[PXDefault(typeof(Vendor.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PX.TM.PXOwnerSelector(typeof(POReceipt.workgroupID))]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
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

		#region UnbilledOrderQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty> { }
		protected Decimal? _UnbilledQty;
		[PXDBQuantity()]
		[PXUIField(DisplayName = "Unbilled Quantity")]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region ReceiptWeight
		public abstract class receiptWeight : PX.Data.BQL.BqlDecimal.Field<receiptWeight> { }
		protected Decimal? _ReceiptWeight;
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Weight", Visible = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ReceiptWeight
		{
			get
			{
				return this._ReceiptWeight;
			}
			set
			{
				this._ReceiptWeight = value;
			}
		}
		#endregion
		#region ReceiptVolume
		public abstract class receiptVolume : PX.Data.BQL.BqlDecimal.Field<receiptVolume> { }
		protected Decimal? _ReceiptVolume;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Volume", Visible = false)]
		public virtual Decimal? ReceiptVolume
		{
			get
			{
				return this._ReceiptVolume;
			}
			set
			{
				this._ReceiptVolume = value;
			}
		}
		#endregion
	
		#region POType
		public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
		protected String _POType;
		[PXDBString(2, IsFixed = true)]
		[POOrderType.List()]
		[PXUIField(DisplayName = "Order Type")]
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
		#region ShipToBAccountID
		public abstract class shipToBAccountID : PX.Data.BQL.BqlInt.Field<shipToBAccountID> { }
		protected Int32? _ShipToBAccountID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Ship To")]		
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
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POReceipt.shipToBAccountID>>>), DescriptionField = typeof(Location.descr))]
		
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

		#region Methods
		protected virtual void SetStatus()
		{
			if (this._Released != null && (bool)this._Released)
			{
				this._Status = POReceiptStatus.Released;
			}
			else if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = POReceiptStatus.Hold;
			}
			else
			{
				this._Status = POReceiptStatus.Balanced;
			}
		}

		public virtual string GetAPDocType() 
		{
			return (this.ReceiptType == POReceiptType.POReceipt ? AP.APDocType.Invoice : AP.APDocType.DebitAdj);
		}
		#endregion

		#region IAssign Members

		int? PX.Data.EP.IAssign.WorkgroupID
		{
			get
			{
				return WorkgroupID; 
			}
			set
			{
				WorkgroupID = value; 
			}
		}

		Guid? PX.Data.EP.IAssign.OwnerID
		{
			get
			{
				return OwnerID; 
			}
			set
			{
				OwnerID = value; 
			}
		}

		#endregion

		#region CuryOrderTotal
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class curyOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyOrderTotal> { }
		[PXDecimal(4)]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual decimal? CuryOrderTotal
		{
			get { return null; }
			set { }
		}
		#endregion
		#region CuryControlTotal
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class curyControlTotal : PX.Data.BQL.BqlDecimal.Field<curyControlTotal> { }
		[PXDecimal(4)]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual decimal? CuryControlTotal
		{
			get { return null; }
			set { }
		}
		#endregion

		#region InventoryDocType
		public abstract class inventoryDocType : PX.Data.BQL.BqlString.Field<inventoryDocType> { }
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "IN Doc. Type", Enabled = false)]
		[INDocType.List()]
		public virtual String InventoryDocType
		{
			get;
			set;
		}
		#endregion
		#region InventoryRefNbr
		public abstract class inventoryRefNbr : PX.Data.BQL.BqlString.Field<inventoryRefNbr> { }
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "IN Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<POReceipt.inventoryDocType>>>>))]
		public virtual String InventoryRefNbr
		{
			get;
			set;
		}
		#endregion

		#region WMSSingleOrder
		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? WMSSingleOrder { get; set; }
		public abstract class wMSSingleOrder : PX.Data.BQL.BqlBool.Field<wMSSingleOrder> { }
		#endregion
		#region OrigPONbr
		[PXDBString(15, IsUnicode = true)]
		public virtual String OrigPONbr { get; set; }
		public abstract class origPONbr : PX.Data.BQL.BqlString.Field<origPONbr> { }
		#endregion
	}


	public static class POReceiptType
	{
        /// <summary>
        /// Specialized selector for POReceipt ReceiptNbr.<br/>
        /// By default, defines the following set of columns for the selector:<br/>
        /// POReceipt.receiptNbr, receiptDate,<br/>
        /// status, vendorID, vendorID_Vendor_acctName,<br/>
        /// vendorLocationID, curyID, curyOrderTotal,<br/>
        /// <example>
        /// [POReceiptType.RefNbr(typeof(Search2<POReceipt.receiptNbr,
		/// 	LeftJoin<Vendor, On<Vendor.bAccountID, Equal<POReceipt.vendorID>>>,
		/// 	Where<POReceipt.receiptType, Equal<Optional<POReceipt.receiptType>>,
		///       And<Where<Vendor.bAccountID, IsNull,
		/// 	  Or<Match<Vendor, Current<AccessInfo.userName>>>>>>>), Filterable = true)]
        /// </example>
        /// </summary>            
		public class RefNbrAttribute : PXSelectorAttribute
		{
            /// <summary>
            /// Default Ctor
            /// </summary>
            /// <param name="SearchType"> Must be IBqlSearch type, pointing to POReceipt.refNbr</param>
            public RefNbrAttribute(Type SearchType)
				: base(SearchType,
				typeof(POReceipt.receiptNbr),
                typeof(POReceipt.invoiceNbr),
                typeof(POReceipt.receiptDate),
				typeof(POReceipt.vendorID),
				typeof(POReceipt.vendorID_Vendor_acctName),
				typeof(POReceipt.vendorLocationID),
				typeof(POReceipt.status),
				typeof(POReceipt.curyID))
			{
			}
		}

        /// <summary>
        /// Specialized version of the AutoNumber attribute for POReceipts<br/>
        /// It defines how the new numbers are generated for the PO Receipt. <br/>
        /// References POReceipt.receiptDate fields of the document,<br/>
        /// and also define a link between  numbering ID's defined in PO Setup:<br/>
        /// namely POSetup.receiptNumberingID for any receipt types<br/>        
        /// </summary>		
		public class NumberingAttribute: AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(POSetup.receiptNumberingID), typeof(POReceipt.receiptDate)) { ; }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(POReceipt, "Receipt"),
					Pair(POReturn, "Return"),
					Pair(TransferReceipt, "Transfer Receipt"),
				}) {}
		}

		public const string TransferReceipt = "RX";
		public const string POReceipt = "RT";
		public const string POReturn = "RN";
		
		public class poreceipt : PX.Data.BQL.BqlString.Constant<poreceipt>
		{
			public poreceipt() : base(POReceipt) { }
		}

		public class poreturn : PX.Data.BQL.BqlString.Constant<poreturn>
		{
			public poreturn() : base(POReturn) { }
		}

        public class transferreceipt : PX.Data.BQL.BqlString.Constant<transferreceipt>
		{
            public transferreceipt() : base(TransferReceipt) { }
        }

		public static string GetINTranType(string aReceiptType)
		{
			string planType = string.Empty;
			switch (aReceiptType)
			{
				case POReceipt:
                case TransferReceipt:
                    return INTranType.Receipt;
				case POReturn: 
                    return INTranType.Issue;
			}
			return planType;
		}
	}

	public class POReceiptStatus
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
			public balanced() : base(Balanced) { }
		}

		public class released : PX.Data.BQL.BqlString.Constant<released>
		{
			public released() : base(Released) { }
		}

		
	}


    public partial class POReceiptBillingReport : PX.Data.IBqlTable
    {
        #region ReceiptNbr
        public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
        protected String _ReceiptNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault()]
        [POReceiptType.Numbering()]
        [POReceiptType.RefNbr(typeof(Search2<POReceipt.receiptNbr,
			LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<POReceipt.vendorID>>>,
            Where<POReceipt.receiptType, Equal<Optional<POReceipt.receiptType>>, And<POReceipt.released, Equal<True>,
              And<Where<Vendor.bAccountID, IsNull,
                 Or<Match<Vendor, Current<AccessInfo.userName>>>>>>>,
            OrderBy<Desc<POReceipt.receiptNbr>>>), Filterable = true)]
        [PXUIField(DisplayName = "Receipt Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
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
    }
}