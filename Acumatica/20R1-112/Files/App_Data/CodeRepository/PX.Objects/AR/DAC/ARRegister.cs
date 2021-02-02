using System;
using System.Collections.Generic;
using System.Diagnostics;

using PX.Data;

using PX.Objects.AR.BQL;
using PX.Objects.Common;
using PX.Objects.Common.Abstractions;
using PX.Objects.Common.MigrationMode;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CR;

using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;
using CRLocation = PX.Objects.CR.Standalone.Location;
using IRegister = PX.Objects.CM.IRegister;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AR
{
	/// <exclude/>
	public class ARDocStatus : ILabelProvider
	{
		private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ CreditHold, Messages.CreditHold },
			{ CCHold, Messages.CCHold },
			{ Hold, Messages.Hold },
			{ Balanced, Messages.Balanced },
			{ Voided, Messages.Voided },
			{ Scheduled, Messages.Scheduled },
			{ Open, Messages.Open },
			{ Closed, Messages.Closed },
			{ PendingPrint, Messages.PendingPrint },
			{ PendingEmail, Messages.PendingEmail },
			{ Reserved, Messages.Reserved },
			{ PendingApproval, Messages.PendingApproval },
			{ Rejected, Messages.Rejected },
			{ Canceled, Messages.Canceled },
		};

		public static readonly string[] Values = 
		{
			CreditHold,
			CCHold,
			Hold,
			Balanced,
			Voided,
			Scheduled,
			Open,
			Closed,
			PendingPrint,
			PendingEmail,
			Reserved,
			PendingApproval,
			Rejected,
			Canceled,
		};

		public static readonly string[] Labels = 
		{
			Messages.CreditHold,
			Messages.CCHold,
			Messages.Hold,
			Messages.Balanced,
			Messages.Voided,
			Messages.Scheduled,
			Messages.Open,
			Messages.Closed,
			Messages.PendingPrint,
			Messages.PendingEmail,
			Messages.Reserved,
			Messages.PendingApproval,
			Messages.Rejected,
			Messages.Canceled,
		};

		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;

		public class ListAttribute : LabelListAttribute
		{
			public ListAttribute() : base(_valueLabelPairs)
			{ }
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Voided = "V";
		public const string Scheduled = "S";
		public const string Open = "N";
		public const string Closed = "C";
		public const string PendingPrint = "P";
		public const string PendingEmail = "E";
		public const string CreditHold = "R";
		public const string CCHold = "W";
		public const string Reserved = "Z";
		public const string PendingApproval = "D";
		public const string Rejected = "J";
		public const string Canceled = "L";

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { ;}
		}

		public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
		{
			public balanced() : base(Balanced) { ;}
		}

		public class voided : PX.Data.BQL.BqlString.Constant<voided>
		{
			public voided() : base(Voided) { ;}
		}

		public class scheduled : PX.Data.BQL.BqlString.Constant<scheduled>
		{
			public scheduled() : base(Scheduled) { ;}
		}

		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) { ;}
		}

		public class closed : PX.Data.BQL.BqlString.Constant<closed>
		{
			public closed() : base(Closed) { ;}
		}

		public class pendingPrint : PX.Data.BQL.BqlString.Constant<pendingPrint>
		{
			public pendingPrint() : base(PendingPrint) { ;}
		}

		public class pendingEmail : PX.Data.BQL.BqlString.Constant<pendingEmail>
		{
			public pendingEmail() : base(PendingEmail) { ;}
		}

		public class cCHold : PX.Data.BQL.BqlString.Constant<cCHold>
		{
			public cCHold() : base(CCHold) { ;}
		}

		public class creditHold : PX.Data.BQL.BqlString.Constant<creditHold>
		{
			public creditHold() : base(CreditHold) { ;}
		}

		public class reserved : PX.Data.BQL.BqlString.Constant<reserved>
		{
			public reserved(): base(Reserved) { }
		}
		public class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
		{
			public pendingApproval() : base(PendingApproval) { }
		}
		public class rejected : PX.Data.BQL.BqlString.Constant<rejected>
		{
			public rejected() : base(Rejected) { }
		}
		public class canceled : PX.Data.BQL.BqlString.Constant<canceled>
		{
			public canceled() : base(Canceled) { }
		}
	}

	/// <summary>
	/// !REV!
	/// The base class for all Accounts Receivable documents.
	/// Provides the fields common to documents of <see cref="ARInvoice"/>, <see cref="ARPayment"/> and <see cref="ARCashSaleEntry"/> types.
	/// </summary>
	[PXPrimaryGraph(new Type[] {
		typeof(SO.SOInvoiceEntry),
		typeof(ARCashSaleEntry),
		typeof(ARInvoiceEntry),
		typeof(ARPaymentEntry),
		typeof(ARInvoiceEntry)	// Default value
	},
		new Type[] {
		typeof(Select<ARInvoice,
			Where<ARInvoice.docType, Equal<Current<ARRegister.docType>>,
				And<ARInvoice.refNbr, Equal<Current<ARRegister.refNbr>>,
				And<ARInvoice.origModule, Equal<BatchModule.moduleSO>,
				And<ARInvoice.released, Equal<False>>>>>>),
		typeof(Select<ARCashSale,
			Where<ARCashSale.docType, Equal<Current<ARRegister.docType>>,
			And<ARCashSale.refNbr, Equal<Current<ARRegister.refNbr>>>>>),
		typeof(Select<ARInvoice,
			Where<ARInvoice.docType, Equal<Current<ARRegister.docType>>,
			And<ARInvoice.refNbr, Equal<Current<ARRegister.refNbr>>>>>),
		typeof(Select<ARPayment,
			Where<ARPayment.docType, Equal<Current<ARRegister.docType>>,
			And<ARPayment.refNbr, Equal<Current<ARRegister.refNbr>>>>>),
		typeof(Select<ARInvoice,
			Where<True, Equal<False>>>)
		})]
	[Serializable]
	[ARRegisterCacheName(Messages.ARDocument)]
	[DebuggerDisplay("DocType = {DocType}, RefNbr = {RefNbr}")]
	[PXEMailSource]
	public partial class ARRegister : PX.Data.IBqlTable, IRegister, IAssign, IBalance, IDocumentKey
	{
		#region Keys
		/// <exclude/>
		public class PK : PrimaryKeyOf<ARRegister>.By<docType, refNbr>
		{
			public static ARRegister Find(PXGraph graph, string docType, string refNbr) => FindBy(graph, docType, refNbr);
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;

		/// <summary>
		/// Indicates whether the record is selected for processing.
		/// </summary>
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

		/// <summary>
		/// The identifier of the <see cref="Branch">branch</see> to which the document belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
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
			public const int Length = 3;
		}
		protected String _DocType;

		/// <summary>
		/// The type of the document.
		/// This field is a part of the compound key of the document.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ARDocType.ListAttribute"/>.
		/// </value>
		[PXDBString(docType.Length, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARDocType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
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

		[PXString]
		[PXUIFieldAttribute(DisplayName = "Document Type (Internal)")]
		public string InternalDocType
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return DocType;
			}
		}
		#endregion
		#region PrintDocType
		public abstract class printDocType : PX.Data.BQL.BqlString.Field<printDocType> { }

		/// <summary>
		/// The type of the document for printing, which is used in reports.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ARDocType.PrintListAttribute"/>.
		/// </value>
		[PXString(3, IsFixed = true)]
		[ARDocType.PrintList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual String PrintDocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;

		/// <summary>
		/// The reference number of the document.
		/// This field is a part of the compound key of the document.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search<ARRegister.refNbr, Where<ARRegister.docType, Equal<Optional<ARRegister.docType>>>>),Filterable=true)]
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
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		protected String _OrigModule;

		/// <summary>
		/// The module from which the document originates.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="BatchModule.FullListAttribute"/>.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(GL.BatchModule.AR)]
		[PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[GL.BatchModule.FullList()]
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
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		protected DateTime? _DocDate;

		/// <summary>
		/// The date of the document.
		/// </summary>
		/// <value>
		/// Defaults to the current <see cref="AccessInfo.BusinessDate">Business Date</see>.
		/// </value>
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region OrigDocDate
		public abstract class origDocDate : PX.Data.BQL.BqlDateTime.Field<origDocDate> { }
		protected DateTime? _OrigDocDate;

		/// <summary>
		/// The date of the original document (e.g. the one reversed by this document).
		/// </summary>
		[PXDBDate()]
		public virtual DateTime? OrigDocDate
		{
			get
			{
				return this._OrigDocDate;
			}
			set
			{
				this._OrigDocDate = value;
			}
		}
		#endregion
		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		protected DateTime? _DueDate;

		/// <summary>
		/// The due date of the document.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		protected String _TranPeriodID;

		/// <summary>
		/// <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see> of the document.
		/// </summary>
		/// <value>
		/// Determined by the <see cref="ARRegister.DocDate">date of the document</see>. Unlike <see cref="ARRegister.FinPeriodID"/>
		/// the value of this field can't be overriden by user.
		/// </value>
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;

		/// <summary>
		/// <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see> of the document.
		/// </summary>
		/// <value>
		/// Defaults to the period, to which the <see cref="ARRegister.DocDate"/> belongs, but can be overriden by user.
		/// </value>
		[AROpenPeriod(
			typeof(ARRegister.docDate),
			branchSourceType: typeof(ARRegister.branchID),
			masterFinPeriodIDType: typeof(ARRegister.tranPeriodID),
			IsHeader = true)]
		[PXDefault()]
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
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;

		/// <summary>
		/// The identifier of the <see cref="Customer"/> record associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName),Filterable=true, TabOrder=2)]
		[PXDefault]
		[PXForeignReference(typeof(Field<ARRegister.customerID>.IsRelatedTo<BAccount.bAccountID>))]
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
		#region CustomerID_Customer_acctName
		public abstract class customerID_Customer_acctName : PX.Data.BQL.BqlString.Field<customerID_Customer_acctName> { }
		#endregion
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		protected Int32? _CustomerLocationID;

		/// <summary>
		/// Identifier of the <see cref="Location"/> of the Customer.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="BAccount.DefLocationID">Default Location</see> of the <see cref="CustomerID">Customer</see> if it is specified,
		/// or to the first found <see cref="Location"/>, associated with the Customer.
		/// Corresponds to the <see cref="Location.LocationID"/> field.
		/// </value>
		[LocationID(typeof(Where<Location.bAccountID, Equal<Optional<ARRegister.customerID>>,
			And<Location.isActive, Equal<True>,
			And<MatchWithBranch<Location.cBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, TabOrder = 3)]
		[PXDefault(typeof(Coalesce<
			Search2<BAccountR.defLocationID,
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
			Where<BAccountR.bAccountID, Equal<Current<ARRegister.customerID>>,
				And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.cBranchID>>>>>,
			Search<CRLocation.locationID,
			Where<CRLocation.bAccountID, Equal<Current<ARRegister.customerID>>,
			And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.cBranchID>>>>>>))]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<ARRegister.customerID>.IsRelatedTo<Location.bAccountID>,
				Field<ARRegister.customerLocationID>.IsRelatedTo<Location.locationID>
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;

		/// <summary>
		/// The code of the <see cref="Currency"/> of the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// Corresponds to the <see cref="Currency.CuryID"/> field.
		/// </value>
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
		#region ARAccountID
		public abstract class aRAccountID : PX.Data.BQL.BqlInt.Field<aRAccountID> { }
		protected Int32? _ARAccountID;

		/// <summary>
		/// The identifier of the <see cref="Account">AR account</see> to which the document should be posted.
		/// The Cash account and Year-to-Date Net Income account cannot be selected as the value of this field.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXDefault]
		[Account(typeof(ARRegister.branchID), typeof(Search<Account.accountID,
					Where2<Match<Current<AccessInfo.userName>>,
						 And<Account.active, Equal<True>,
						 And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
						  Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>), DisplayName = "AR Account",
			ControlAccountForModule = ControlAccountModule.AR)]
		public virtual Int32? ARAccountID
		{
			get
			{
				return this._ARAccountID;
			}
			set
			{
				this._ARAccountID = value;
			}
		}
		#endregion
		#region ARSubID
		public abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
		protected Int32? _ARSubID;

		/// <summary>
		/// The identifier of the <see cref="Sub">subaccount</see> to which the document should be posted.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXDefault]
		[SubAccount(typeof(ARRegister.aRAccountID), DescriptionField = typeof(Sub.description), DisplayName = "AR Subaccount", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? ARSubID
		{
			get
			{
				return this._ARSubID;
			}
			set
			{
				this._ARSubID = value;
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected Int32? _LineCntr;

		/// <summary>
		/// The counter of the document lines, which is used <i>internally</i> to assign
		/// <see cref="ARTran.LineNbr">numbers</see> to newly created <see cref="ARTran">lines</see>.
		/// We do not recommended that you rely on this field to determine the exact number of lines, 
		/// which might not be reflected by the value of this field under various conditions.
		/// </summary>
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
		#region AdjCntr
		public abstract class adjCntr : PX.Data.BQL.BqlInt.Field<adjCntr> { }
		protected int? _AdjCntr;

		/// <summary>
		/// The counter of the document applications, which is used <i>internally</i> to assign
		/// <see cref="ARAdjust.AdjNbr">numbers</see> to newly created <see cref="ARAdjust">lines</see>.
		/// The value is used to determine old and new applications.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? AdjCntr
		{
			get
			{
				return _AdjCntr;
			}
			set
			{
				_AdjCntr = value;
			}
		}
		#endregion
		#region AdjCntr
		public abstract class drSchedCntr : PX.Data.BQL.BqlInt.Field<drSchedCntr> { }

		[PXDBInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? DRSchedCntr { get; set; }
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;

		/// <summary>
		/// The identifier of the <see cref="CurrencyInfo">CurrencyInfo</see> object associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CurrencyInfoID"/> field.
		/// </value>
		[PXDBLong()]
		[CurrencyInfo(ModuleCode = BatchModule.AR)]
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
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		protected Decimal? _CuryOrigDocAmt;

		/// <summary>
		/// The amount of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.origDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryOrigDocAmt
		{
			get
			{
				return this._CuryOrigDocAmt;
			}
			set
			{
				this._CuryOrigDocAmt = value;
			}
		}
		#endregion
		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		protected Decimal? _OrigDocAmt;

		/// <summary>
		/// The amount of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigDocAmt
		{
			get
			{
				return this._OrigDocAmt;
			}
			set
			{
				this._OrigDocAmt = value;
			}
		}
		#endregion
		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		protected Decimal? _CuryDocBal;

		/// <summary>
		/// The open balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.docBal), BaseCalc=false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryDocBal
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
		#endregion
		#region DocBal
		public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		protected Decimal? _DocBal;

		/// <summary>
		/// The open balance of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DocBal
		{
			get
			{
				return this._DocBal;
			}
			set
			{
				this._DocBal = value;
			}
		}
		#endregion

		#region CuryInitDocBal
		public abstract class curyInitDocBal : PX.Data.BQL.BqlDecimal.Field<curyInitDocBal> { }

		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.initDocBal))]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual decimal? CuryInitDocBal
		{
			get;
			set;
		}
		#endregion
		#region InitDocBal
		public abstract class initDocBal : PX.Data.BQL.BqlDecimal.Field<initDocBal> { }

		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? InitDocBal
		{
			get;
			set;
		}
		#endregion
		#region DisplayCuryInitDocBal
		public abstract class displayCuryInitDocBal : PX.Data.BQL.BqlDecimal.Field<displayCuryInitDocBal> { }

		/// <summary>
		/// The non database field, displaying an entered in migration mode 
		/// balance of the document <see cref="ARRegister.CuryInitDocBal">.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// Added to configure the different visibility of one field on one DAC cache.
		/// </summary>
		[PXDBCalced(typeof(ARRegister.curyInitDocBal), typeof(decimal))]
		[PXCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.initDocBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Migrated Balance", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? DisplayCuryInitDocBal
		{
			get;
			set;
		}
		#endregion

		#region CuryOrigDiscAmt
		public abstract class curyOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDiscAmt> { }
		protected Decimal? _CuryOrigDiscAmt;

		/// <summary>
		/// The cash discount entered for the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.origDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryOrigDiscAmt
		{
			get
			{
				return this._CuryOrigDiscAmt;
			}
			set
			{
				this._CuryOrigDiscAmt = value;
			}
		}
		#endregion
		#region OrigDiscAmt
		public abstract class origDiscAmt : PX.Data.BQL.BqlDecimal.Field<origDiscAmt> { }
		protected Decimal? _OrigDiscAmt;

		/// <summary>
		/// The cash discount entered for the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigDiscAmt
		{
			get
			{
				return this._OrigDiscAmt;
			}
			set
			{
				this._OrigDiscAmt = value;
			}
		}
		#endregion
		#region CuryDiscTaken
		public abstract class curyDiscTaken : PX.Data.BQL.BqlDecimal.Field<curyDiscTaken> { }
		protected Decimal? _CuryDiscTaken;

		/// <summary>
		/// The <see cref="ARAdjust.CuryAdjdDiscAmt">cash discount amount</see> actually applied to the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.discTaken))]
		public virtual Decimal? CuryDiscTaken
		{
			get
			{
				return this._CuryDiscTaken;
			}
			set
			{
				this._CuryDiscTaken = value;
			}
		}
		#endregion
		#region DiscTaken
		public abstract class discTaken : PX.Data.BQL.BqlDecimal.Field<discTaken> { }
		protected Decimal? _DiscTaken;

		/// <summary>
		/// The <see cref="ARAdjust.CuryAdjdDiscAmt">cash discount amount</see> actually applied to the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscTaken
		{
			get
			{
				return this._DiscTaken;
			}
			set
			{
				this._DiscTaken = value;
			}
		}
		#endregion
		#region CuryDiscBal
		public abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
		protected Decimal? _CuryDiscBal;

		/// <summary>
		/// The cash discount balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.discBal), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryDiscBal
		{
			get
			{
				return this._CuryDiscBal;
			}
			set
			{
				this._CuryDiscBal = value;
			}
		}
		#endregion
		#region DiscBal
		public abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
		protected Decimal? _DiscBal;

		/// <summary>
		/// The cash discount balance of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscBal
		{
			get
			{
				return this._DiscBal;
			}
			set
			{
				this._DiscBal = value;
			}
		}
		#endregion

		#region DocDisc
		public abstract class docDisc : PX.Data.BQL.BqlDecimal.Field<docDisc> { }
		protected Decimal? _DocDisc;

		/// <summary>
		/// The <see cref="ARInvoiceDiscountDetail.DiscountAmt">document discount total</see> (without group discounts).
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
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

		/// <summary>
		/// The <see cref="ARInvoiceDiscountDetail.DiscountAmt">document discount total</see> (without group discounts).
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.docDisc))]
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

		#region CuryChargeAmt
		public abstract class curyChargeAmt : PX.Data.BQL.BqlDecimal.Field<curyChargeAmt> { }
		protected Decimal? _CuryChargeAmt;

		/// <summary>
		/// The total of all finance charges applied to the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.chargeAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Finance Charges", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? CuryChargeAmt
		{
			get
			{
				return this._CuryChargeAmt;
			}
			set
			{
				this._CuryChargeAmt = value;
			}
		}
		#endregion
		#region ChargeAmt
		public abstract class chargeAmt : PX.Data.BQL.BqlDecimal.Field<chargeAmt> { }
		protected Decimal? _ChargeAmt;

		/// <summary>
		/// The total of all finance charges applied to the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ChargeAmt
		{
			get
			{
				return this._ChargeAmt;
			}
			set
			{
				this._ChargeAmt = value;
			}
		}
		#endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		protected String _DocDesc;

		/// <summary>
		/// The description of the document.
		/// </summary>
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String DocDesc
		{
			get
			{
				return this._DocDesc;
			}
			set
			{
				this._DocDesc = value;
			}
		}
		#endregion

		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		protected string _TaxCalcMode;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TX.TaxCalculationMode.TaxSetting, typeof(Search<Location.cTaxCalcMode, Where<Location.bAccountID, Equal<Current<ARRegister.customerID>>,
			And<Location.locationID, Equal<Current<ARRegister.customerLocationID>>>>>))]
		[TX.TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode { get; set; }
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
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedDateTime]
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

		#region DocClass
		public abstract class docClass : PX.Data.BQL.BqlString.Field<docClass> { }

		/// <summary>
		/// Reserved for internal use.
		/// The read-only class of the document determined by the <see cref="DocType"/>.
		/// Affects the way the document is posted to the General Ledger.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="GLTran.TranClass"/> field.
		/// </value>
		[PXString(1, IsFixed = true)]
		public virtual string DocClass
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.DocClass(_DocType);
			}
			set
			{
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }

		/// <summary>
		/// The number of the <see cref="Batch"/> created from the document on release.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Batch.BatchNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[BatchNbr(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleAR>>>),
			IsMigratedRecordField = typeof(ARRegister.isMigratedRecord))]
		public virtual string BatchNbr
		{
			get;
			set;
		}
		#endregion
		#region BatchSeq
		public abstract class batchSeq : PX.Data.BQL.BqlShort.Field<batchSeq> { }
		protected Int16? _BatchSeq;

		/// <summary>
		/// The batch sequence number.
		/// The field is not used.
		/// </summary>
		[PXDBShort()]
		[PXDefault((short)0)]
		public virtual Int16? BatchSeq
		{
			get
			{
				return this._BatchSeq;
			}
			set
			{
				this._BatchSeq = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;

		/// <summary>
		/// When set to <c>true</c>, indicates that the document has been released.
		/// </summary>
		[PXDBBool()]
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
		#region ReleasedToVerify
		/// <exclude/>
		public abstract class releasedToVerify : PX.Data.BQL.BqlBool.Field<releasedToVerify> { }
		/// <summary>
		/// When set, on persist checks, that the document has the corresponded <see cref="Released"/> original value.
		/// When not set, on persist checks, that <see cref="Released"/> value is not changed.
		/// Throws an error otherwise.
		/// </summary>
		[PX.Objects.Common.Attributes.PXDBRestrictionBool(typeof(released))]
		public virtual Boolean? ReleasedToVerify
		{
			get;
			set;
		}
		#endregion
		#region OpenDoc
		public abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		protected Boolean? _OpenDoc;

		/// <summary>
		/// When set to <c>true</c>, indicates that the document is open.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
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
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;

		/// <summary>
		/// When set to <c>true</c> indicates that the document is on hold and thus cannot be released.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true, typeof(ARSetup.holdEntry))]
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
		#region Scheduled
		public abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		protected Boolean? _Scheduled;

		/// <summary>
		/// When set to <c>true</c> indicates that the document is part of a <c>Schedule</c> and serves as a template for generating other documents according to it.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Scheduled
		{
			get
			{
				return this._Scheduled;
			}
			set
			{
				this._Scheduled = value;
			}
		}
		#endregion
		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		protected Boolean? _Voided;

		/// <summary>
		/// When set to <c>true</c> indicates that the document has been voided.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion
		#region SelfVoidingDoc
		public abstract class selfVoidingDoc : PX.Data.BQL.BqlBool.Field<selfVoidingDoc> { }

		/// <summary>
		/// When <c>true</c>, indicates that the document can be voided only in full and 
		/// it is not allow to delete reversing applications partially.
		/// </summary>
		[PXBool()]
		[PXDefault(false)]
		[PXFormula(typeof(IIf<Where<IsSelfVoiding<docType>>, True, False>))]
		public virtual bool? SelfVoidingDoc { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
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
		#region ClosedDate
		public abstract class closedDate : PX.Data.BQL.BqlDateTime.Field<closedDate> { }

		/// <summary>
		/// The date of the last application.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Closed Date", Visibility = PXUIVisibility.Invisible)]
		public virtual DateTime? ClosedDate { get; set; }

		#endregion
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;

		/// <summary>
		/// !REV!
		/// </summary>
		[PXDBGuid()]
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
		#region ClosedFinPeriodID
		public abstract class closedFinPeriodID : PX.Data.BQL.BqlString.Field<closedFinPeriodID> { }
		protected String _ClosedFinPeriodID;

		/// <summary>
		/// The <see cref="FinancialPeriod">Financial Period</see>, in which the document was closed.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="FinPeriodID"/> field.
		/// </value>
		[FinPeriodID(
			branchSourceType: typeof(ARRegister.branchID),
			masterFinPeriodIDType: typeof(ARRegister.closedTranPeriodID))]
		[PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
		public virtual String ClosedFinPeriodID
		{
			get
			{
				return this._ClosedFinPeriodID;
			}
			set
			{
				this._ClosedFinPeriodID = value;
			}
		}
		#endregion
		#region ClosedTranPeriodID
		public abstract class closedTranPeriodID : PX.Data.BQL.BqlString.Field<closedTranPeriodID> { }
		protected String _ClosedTranPeriodID;

		/// <summary>
		/// The <see cref="FinancialPeriod">Financial Period</see>, in which the document was closed.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="TranPeriodID"/> field.
		/// </value>
		[PeriodID()]
		[PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
		public virtual String ClosedTranPeriodID
		{
			get
			{
				return this._ClosedTranPeriodID;
			}
			set
			{
				this._ClosedTranPeriodID = value;
			}
		}
		#endregion
		#region RGOLAmt
		public abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt> { }
		protected Decimal? _RGOLAmt;

		/// <summary>
		/// Realized Gain or Loss amount associated with the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? RGOLAmt
		{
			get
			{
				return this._RGOLAmt;
			}
			set
			{
				this._RGOLAmt = value;
			}
		}
		#endregion
		#region CuryRoundDiff
		public abstract class curyRoundDiff : PX.Data.BQL.BqlDecimal.Field<curyRoundDiff> { }

		/// <summary>
		/// The difference between the original amount of the document and the rounded amount.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// Applicable only if <see cref="FeaturesSet.InvoiceRounding">Invoice Rounding</see> feature is enabled.
		/// </summary>
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.roundDiff), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Rounding Diff.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public decimal? CuryRoundDiff
		{
			get;
			set;
		}
		#endregion
		#region RoundDiff
		public abstract class roundDiff : PX.Data.BQL.BqlDecimal.Field<roundDiff> { }

		/// <summary>
		/// The difference between the original amount of the document and the rounded amount,
		/// in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// The field is used only if the <see cref="FeaturesSet.InvoiceRounding">Invoice Rounding</see> feature is enabled.
		/// </summary>
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? RoundDiff
		{
			get;
			set;
		}
		#endregion
		#region Payable

		/// <summary>
		/// A read-only field, which indicates (if set to <c>true</c>) that the document is payable.
		/// The value of the field depends on the<see cref="DocType"/> field and is opposite to the value of the <see cref="Paying"/> field.
		/// </summary>
		public virtual Boolean? Payable
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.Payable(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region Paying

		/// <summary>
		/// Read-only field indicating whether the document is paying. Depends solely on the <see cref="DocType"/> field.
		/// Opposite of the <see cref="Payable"/> field.
		/// </summary>
		public virtual Boolean? Paying
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return (ARDocType.Payable(this._DocType) == false);
			}
			set
			{
			}
		}
		#endregion
		#region SortOrder

		/// <summary>
		/// Read-only field determining the sort order for AR documents based on the <see cref="DocType"/> field.
		/// </summary>
		public virtual Int16? SortOrder
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.SortOrder(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region SignBalance

		/// <summary>
		/// Read-only field indicating the sign of the document's impact on AR balance .
		/// Depends solely on the <see cref="DocType"/> field.
		/// </summary>
		/// <value>
		/// Possible values are: <c>1</c>, <c>-1</c> or <c>0</c>.
		/// </value>
		public virtual Decimal? SignBalance
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.SignBalance(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region SignAmount

		/// <summary>
		/// Read-only field indicating the sign of the document amount.
		/// Depends solely on the <see cref="DocType"/> field.
		/// </summary>
		/// <value>
		/// Possible values are: <c>1</c>, <c>-1</c> or <c>0</c>.
		/// </value>
		public virtual Decimal? SignAmount
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.SignAmount(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		protected string _ScheduleID;

		/// <summary>
		/// Identifier of the <see cref="Schedule" />, associated with the document.
		/// In case <see cref="Scheduled"/> is <c>true</c>, the field points to the Schedule, to which the document belongs as a template.
		/// Otherwise, the field points to the Schedule, from which this document was generated, if any.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Schedule.ScheduleID"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		public virtual string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
		#region ImpRefNbr
		public abstract class impRefNbr : PX.Data.BQL.BqlString.Field<impRefNbr> { }
		protected String _ImpRefNbr;

		/// <summary>
		/// Implementation specific reference number of the document.
		/// This field is neither filled nor used by the core Acumatica itself, but may be utilized by customizations or extensions.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		public virtual String ImpRefNbr
		{
			get
			{
				return this._ImpRefNbr;
			}
			set
			{
				this._ImpRefNbr = value;
			}
		}
		#endregion
		#region StatementDate
		public abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
		protected DateTime? _StatementDate;

		/// <summary>
		/// The date of the <see cref="ARStatement">Customer Statement</see>, in which the document is reported.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARStatement.StatementDate"/> field.
		/// </value>
		[PXDBDate()]
		public virtual DateTime? StatementDate
		{
			get
			{
				return this._StatementDate;
			}
			set
			{
				this._StatementDate = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;

		/// <summary>
		/// The identifier of the <see cref="CustSalesPeople">salesperson</see> to whom the document belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CustSalesPeople.SalesPersonID"/> field.
		/// </value>
		[SalesPerson()]
		[PXDefault(typeof(Search<CustDefSalesPeople.salesPersonID, Where<CustDefSalesPeople.bAccountID, Equal<Current<ARRegister.customerID>>, And<CustDefSalesPeople.locationID, Equal<Current<ARRegister.customerLocationID>>, And<CustDefSalesPeople.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<ARRegister.salesPersonID>.IsRelatedTo<SalesPerson.salesPersonID>))]
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
		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }

		/// <summary>
		/// When <c>true</c>, indicates that the amount of tax calculated with the external Tax Engine(Avalara) is up to date.
		/// If this field equals <c>false</c>, the document was updated since last synchronization with the Tax Engine
		/// and taxes might need recalculation.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is up to date", Enabled = false)]
		public virtual Boolean? IsTaxValid
		{
			get;
			set;
		}
		#endregion
		#region IsTaxPosted
		public abstract class isTaxPosted : PX.Data.BQL.BqlBool.Field<isTaxPosted> { }

		/// <summary>
		/// When <c>true</c>, indicates that the tax information was successfully commited to the external Tax Engine(Avalara).
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is posted/commited to the external Tax Engine(Avalara)", Enabled = false)]
		public virtual Boolean? IsTaxPosted
		{
			get;
			set;
		}
		#endregion
		#region IsTaxSaved
		public abstract class isTaxSaved : PX.Data.BQL.BqlBool.Field<isTaxSaved> { }

		/// <summary>
		/// Indicates whether the tax information related to the document was saved to the external Tax Engine (Avalara).
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is saved in external Tax Engine(Avalara)", Enabled = false)]
		public virtual Boolean? IsTaxSaved
		{
			get;
			set;
		}
		#endregion
		#region NonTaxable
		public abstract class nonTaxable : PX.Data.BQL.BqlBool.Field<nonTaxable> { }
		/// <summary>
		/// Get or set NonTaxable that mark current document does not impose sales taxes.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Non-Taxable", Enabled = false)]
		public virtual Boolean? NonTaxable
		{
			get;
			set;
		}
		#endregion
		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		protected String _OrigDocType;

		/// <summary>
		/// The type of the original (source) document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="DocType"/> field.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[ARDocType.List()]
		[PXUIField(DisplayName = "Orig. Doc. Type")]
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

		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

		/// <summary>
		/// The reference number of the original (source) document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="RefNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Orig. Ref. Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual string OrigRefNbr
		{
			get;
			set;
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected string _Status;

		/// <summary>
		/// The status of the document.
		/// The value of the field is determined by the values of the status flags,
		/// such as <see cref="Hold"/>, <see cref="Released"/>, <see cref="Voided"/>, <see cref="Scheduled"/>.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ARDocStatus.ListAttribute"/>.
		/// Defaults to <see cref="ARDocStatus.Hold"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ARDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ARDocStatus.List]
		[SetStatus]
		[PXDependsOnFields(
			typeof(ARRegister.voided),
			typeof(ARRegister.hold),
			typeof(ARRegister.scheduled),
			typeof(ARRegister.released),
			typeof(ARRegister.approved),
			typeof(ARRegister.rejected),
			typeof(ARRegister.canceled),
			typeof(ARRegister.dontApprove),
			typeof(ARRegister.openDoc),
			typeof(ARRegister.pendingProcessing),
			typeof(ARRegister.docType))]
		public virtual string Status
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

		#region CuryDiscountedDocTotal
		public abstract class curyDiscountedDocTotal : PX.Data.BQL.BqlDecimal.Field<curyDiscountedDocTotal> { }
		protected decimal? _CuryDiscountedDocTotal;

		/// <summary>
		/// The discounted amount of the document.
		/// Given in the <see cref="CuryID"> currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.discountedDocTotal))]
		[PXUIField(DisplayName = "Discounted Doc. Total", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryDiscountedDocTotal
		{
			get
			{
				return _CuryDiscountedDocTotal;
			}
			set
			{
				_CuryDiscountedDocTotal = value;
			}
		}
		#endregion
		#region DiscountedDocTotal
		public abstract class discountedDocTotal : PX.Data.BQL.BqlDecimal.Field<discountedDocTotal> { }
		protected decimal? _DiscountedDocTotal;

		/// <summary>
		/// The discounted amount of the document.
		/// Given in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// </summary>
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? DiscountedDocTotal
		{
			get
			{
				return _DiscountedDocTotal;
			}
			set
			{
				_DiscountedDocTotal = value;
			}
		}
		#endregion
		#region CuryDiscountedTaxableTotal
		public abstract class curyDiscountedTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyDiscountedTaxableTotal> { }
		protected decimal? _CuryDiscountedTaxableTotal;

		/// <summary>
		/// The total taxable amount reduced on early payment, according to cash discount.
		/// Given in the <see cref="CuryID"> currency of the document</see>.
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.discountedTaxableTotal))]
		[PXUIField(DisplayName = "Discounted Taxable Total", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryDiscountedTaxableTotal
		{
			get
			{
				return _CuryDiscountedTaxableTotal;
			}
			set
			{
				_CuryDiscountedTaxableTotal = value;
			}
		}
		#endregion
		#region DiscountedTaxableTotal
		public abstract class discountedTaxableTotal : PX.Data.BQL.BqlDecimal.Field<discountedTaxableTotal> { }
		protected decimal? _DiscountedTaxableTotal;

		/// <summary>
		/// The total taxable amount reduced on early payment, according to cash discount.
		/// Given in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// </summary>
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? DiscountedTaxableTotal
		{
			get
			{
				return _DiscountedTaxableTotal;
			}
			set
			{
				_DiscountedTaxableTotal = value;
			}
		}
		#endregion
		#region CuryDiscountedPrice
		public abstract class curyDiscountedPrice : PX.Data.BQL.BqlDecimal.Field<curyDiscountedPrice> { }
		protected decimal? _CuryDiscountedPrice;

		/// <summary>
		/// The total tax amount reduced on early payment, according to cash discount.
		/// Given in the <see cref="CuryID"> currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.discountedPrice))]
		[PXUIField(DisplayName = "Tax on Discounted Price", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryDiscountedPrice
		{
			get
			{
				return _CuryDiscountedPrice;
			}
			set
			{
				_CuryDiscountedPrice = value;
			}
		}
		#endregion
		#region DiscountedPrice
		public abstract class discountedPrice : PX.Data.BQL.BqlDecimal.Field<discountedPrice> { }
		protected decimal? _DiscountedPrice;

		/// <summary>
		/// The total tax amount reduced on early payment, according to cash discount.
		/// Given in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// </summary>
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? DiscountedPrice
		{
			get
			{
				return _DiscountedPrice;
			}
			set
			{
				_DiscountedPrice = value;
			}
		}
		#endregion
		#region HasPPDTaxes
		public abstract class hasPPDTaxes : PX.Data.BQL.BqlBool.Field<hasPPDTaxes> { }
		protected bool? _HasPPDTaxes;

		/// <summary>
		/// If set to <c>true</c>, indicates that the document has the taxes that reduce cash discount taxable amount on early payment.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? HasPPDTaxes
		{
			get
			{
				return _HasPPDTaxes;
			}
			set
			{
				_HasPPDTaxes = value;
			}
		}
		#endregion
		#region PendingPPD
		public abstract class pendingPPD : PX.Data.BQL.BqlBool.Field<pendingPPD> { }
		protected bool? _PendingPPD;

		/// <summary>
		/// If set to <c>true</c>, indicates that the document has been fully paid and 
		/// to close the document, you need to apply the cash discount by generating a 
		/// credit memo on the Generate VAT Credit Memos (AR504500) form.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? PendingPPD
		{
			get
			{
				return _PendingPPD;
			}
			set
			{
				_PendingPPD = value;
			}
		}
		#endregion
		#region IsMigratedRecord
		public abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the record has been created 
		/// in migration mode without affecting GL module.
		/// </summary>
		[MigratedRecord(typeof(ARSetup.migrationMode))]
		public virtual bool? IsMigratedRecord
		{
			get;
			set;
		}
		#endregion
		#region PaymentsByLinesAllowed
		public abstract class paymentsByLinesAllowed : PX.Data.BQL.BqlBool.Field<paymentsByLinesAllowed> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the record has been created 
		/// with activated <see cref="FeaturesSet.PaymentsByLines"/> feature and
		/// such document allow payments by lines.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Pay by Line",
			Visibility = PXUIVisibility.Visible,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXDefault(false)]
		public virtual bool? PaymentsByLinesAllowed
		{
			get;
			set;
		}
		#endregion
		#region ApproverID
		public abstract class approverID : PX.Data.BQL.BqlGuid.Field<approverID> { }
		[PXDBGuid()]
		[PX.TM.PXOwnerSelector()]
		[PXUIField(DisplayName = "Owner")]
		public virtual Guid? ApproverID { get; set; }
		#endregion
		#region ApproverWorkgroupID
		public abstract class approverWorkgroupID : PX.Data.BQL.BqlInt.Field<approverWorkgroupID> { }
		[PXDBInt]
		[PX.TM.PXCompanyTreeSelector]
		[PXUIField(DisplayName = Messages.ApprovalWorkGroupID, Enabled = false)]
		public virtual int? ApproverWorkgroupID { get; set; }
		#endregion
		#region IAssignable
		int? IAssign.WorkgroupID {
			get { return ApproverWorkgroupID; }
			set { ApproverWorkgroupID = value; }
		}

		Guid? IAssign.OwnerID
		{
			get { return ApproverID; }
			set { ApproverID = value; }
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Approved
		{
			get;
			set;
		}
		#endregion
		#region Rejected
		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Rejected { get; set; }
		#endregion
		#region DontApprove
		public abstract class dontApprove : PX.Data.BQL.BqlBool.Field<dontApprove> { }
		/// <summary>
		/// Indicates that the current document should be excluded from the 
		/// approval process. Maintenance of this property is on graph level.
		/// </summary>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? DontApprove
		{
			get;
			set;
		}
		#endregion

		#region RetainageAcctID
		public abstract class retainageAcctID : PX.Data.BQL.BqlInt.Field<retainageAcctID> { }

		[Account(typeof(ARRegister.branchID), DisplayName = "Retainage Receivable Account", DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.AR)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? RetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region RetainageSubID
		public abstract class retainageSubID : PX.Data.BQL.BqlInt.Field<retainageSubID> { }

		[SubAccount(typeof(ARRegister.retainageAcctID), typeof(ARRegister.branchID), true, DisplayName = "Retainage Receivable Sub.", DescriptionField = typeof(Sub.description))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? RetainageSubID
		{
			get;
			set;
		}
		#endregion
		#region RetainageApply
		public abstract class retainageApply : PX.Data.BQL.BqlBool.Field<retainageApply> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Apply Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? RetainageApply
		{
			get;
			set;
		}
		#endregion
		#region IsRetainageDocument
		public abstract class isRetainageDocument : PX.Data.BQL.BqlBool.Field<isRetainageDocument> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Retainage Invoice", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? IsRetainageDocument
		{
			get;
			set;
		}
		#endregion
		#region DefRetainagePct
		public abstract class defRetainagePct : PX.Data.BQL.BqlDecimal.Field<defRetainagePct> { }

		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Default Retainage Percent", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? DefRetainagePct
		{
			get;
			set;
		}
		#endregion
		#region CuryLineRetainageTotal
		public abstract class curyLineRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyLineRetainageTotal> { }

		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.lineRetainageTotal))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryLineRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region LineRetainageTotal
		public abstract class lineRetainageTotal : PX.Data.BQL.BqlDecimal.Field<lineRetainageTotal> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? LineRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainageTotal
		public abstract class curyRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainageTotal> { }

		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.retainageTotal))]
		[PXUIField(DisplayName = "Original Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainageTotal
		public abstract class retainageTotal : PX.Data.BQL.BqlDecimal.Field<retainageTotal> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Original Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainageUnreleasedAmt
		public abstract class curyRetainageUnreleasedAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageUnreleasedAmt> { }

		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.retainageUnreleasedAmt))]
		[PXUIField(DisplayName = "Unreleased Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryRetainageUnreleasedAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageUnreleasedAmt
		public abstract class retainageUnreleasedAmt : PX.Data.BQL.BqlDecimal.Field<retainageUnreleasedAmt> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Unreleased Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainageUnreleasedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainageReleased
		public abstract class curyRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyRetainageReleased> { }

		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.retainageReleased))]
		[PXUIField(DisplayName = "Released Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXFormula(typeof(Switch<Case<Where<docType, Equal<ARDocType.creditMemo>>, decimal0>, Sub<curyRetainageTotal, curyRetainageUnreleasedAmt>>))]
		public virtual decimal? CuryRetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region RetainageReleasedAmt
		public abstract class retainageReleased : PX.Data.BQL.BqlDecimal.Field<retainageReleased> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Released Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainedTaxTotal
		public abstract class curyRetainedTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxTotal> { }

		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.retainedTaxTotal))]
		[PXUIField(DisplayName = "Tax on Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryRetainedTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainedTaxTotal
		public abstract class retainedTaxTotal : PX.Data.BQL.BqlDecimal.Field<retainedTaxTotal> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainedTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainedDiscTotal
		public abstract class curyRetainedDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainedDiscTotal> { }

		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.retainedDiscTotal))]
		[PXUIField(DisplayName = "Discount on Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryRetainedDiscTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainedDiscTotal
		public abstract class retainedDiscTotal : PX.Data.BQL.BqlDecimal.Field<retainedDiscTotal> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainedDiscTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainageUnpaidTotal
		public abstract class curyRetainageUnpaidTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainageUnpaidTotal> { }

		[PXCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.retainageUnpaidTotal))]
		[PXUIField(DisplayName = "Unpaid Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryRetainageUnpaidTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainageUnpaidTotal
		public abstract class retainageUnpaidTotal : PX.Data.BQL.BqlDecimal.Field<retainageUnpaidTotal> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainageUnpaidTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainagePaidTotal
		public abstract class curyRetainagePaidTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainagePaidTotal> { }

		[PXCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.retainagePaidTotal))]
		[PXUIField(DisplayName = "Paid Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryRetainagePaidTotal
		{
			get;
			set;
		}
		#endregion
		#region RetainagePaidTotal
		public abstract class retainagePaidTotal : PX.Data.BQL.BqlDecimal.Field<retainagePaidTotal> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainagePaidTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDocAmtWithRetainageTotal
		public abstract class curyOrigDocAmtWithRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmtWithRetainageTotal> { }

		[PXCury(typeof(ARRegister.curyID))]
		[PXUIField(DisplayName = "Total Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Add<ARRegister.curyOrigDocAmt, ARRegister.curyRetainageTotal>))]
		public virtual decimal? CuryOrigDocAmtWithRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region OrigDocAmtWithRetainageTotal
		public abstract class origDocAmtWithRetainageTotal : PX.Data.BQL.BqlDecimal.Field<origDocAmtWithRetainageTotal> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Total Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXFormula(typeof(Add<ARRegister.origDocAmt, ARRegister.retainageTotal>))]
		public virtual decimal? OrigDocAmtWithRetainageTotal
		{
			get;
			set;
		}
		#endregion

		#region IsCancellation
		/// <exclude/>
		public abstract class isCancellation : Data.BQL.BqlBool.Field<isCancellation>
		{
		}
		/// <summary>
		/// When set to <c>true</c>, indicates that the invoice is a cancellation invoice (credit memo).
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsCancellation
		{
			get;
			set;
		}
		#endregion
		#region IsCorrection
		/// <exclude/>
		public abstract class isCorrection : Data.BQL.BqlBool.Field<isCorrection>
		{
		}
		/// <summary>
		/// When set to <c>true</c>, indicates that the invoice is a correction invoice.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Correction Inv.", Visible = false, Enabled = false)]
		public virtual bool? IsCorrection
		{
			get;
			set;
		}
		#endregion
		#region IsUnderCorrection
		/// <exclude/>
		public abstract class isUnderCorrection : Data.BQL.BqlBool.Field<isUnderCorrection>
		{
		}
		/// <summary>
		/// When set to <c>true</c>, indicates that Cancel or Correct action was applied to the invoice.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsUnderCorrection
		{
			get;
			set;
		}
		#endregion
		#region Canceled
		/// <exclude/>
		public abstract class canceled : Data.BQL.BqlBool.Field<canceled>
		{
		}
		/// <summary>
		/// When set to <c>true</c>, indicates that the invoice was canceled or corrected.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Canceled
		{
			get;
			set;
		}
		#endregion
		#region PendingProcessing
		public abstract class pendingProcessing : PX.Data.BQL.BqlBool.Field<pendingProcessing> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? PendingProcessing
		{
			get;
			set;
		}
		#endregion
		internal string WarningMessage { get; set; }


		public class SetStatusAttribute : PXEventSubscriberAttribute, IPXRowUpdatingSubscriber, IPXRowInsertingSubscriber
		{
			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				sender.Graph.FieldUpdating.AddHandler(
					sender.GetItemType(),
					nameof(ARRegister.hold),
					(cache, e) =>
				{
					PXBoolAttribute.ConvertValue(e);

					ARRegister item = e.Row as ARRegister;
					if (item != null)
					{
						StatusSet(cache, item, (bool?)e.NewValue);
					}
				});

				sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), nameof(ARRegister.Status), (cache, e) => { e.NewValue = cache.GetValue<ARRegister.status>(e.Row); });
				sender.Graph.RowSelecting.AddHandler(sender.GetItemType(), RowSelecting);
				sender.Graph.RowSelected.AddHandler(
						sender.GetItemType(),
						(cache, e) =>
						{
							ARRegister document = e.Row as ARRegister;

							if (document != null)
							{
								StatusSet(cache, document, document.Hold);
							}
						});
			}

            protected virtual void StatusSet(PXCache cache, ARRegister item, bool? holdVal)
            {
				if (item.Canceled == true)
				{
					item.Status = ARDocStatus.Canceled;
					return;
				}
                if (item.Voided == true)
                {
                    item.Status = ARDocStatus.Voided;
	                return;
                }
                if (item.Hold == true)
                {
	                if (item.Released == true)
	                {
		                item.Status = ARDocStatus.Reserved;
	                }
	                else
	                {
		                item.Status = ARDocStatus.Hold;
					}
	                return;
                }
                if (item.Scheduled  == true)
                {
                    item.Status = ARDocStatus.Scheduled;
					return;
                }
				if (item.Rejected == true)
				{
					item.Status = ARDocStatus.Rejected;
					return;
				}
				if (item.Released != true)
                {
					if (item.Approved != true 
						&& item.DontApprove != true 
						&& (item.DocType == ARDocType.CashReturn 
							|| item.DocType == ARDocType.Refund 
							|| item.DocType == ARDocType.Invoice 
							|| item.DocType == ARDocType.DebitMemo 
							|| item.DocType == ARDocType.CreditMemo))
					{
						item.Status = ARDocStatus.PendingApproval;
					}
					else
					{
						if (item.PendingProcessing == true)
						{
							item.Status = ARDocStatus.CCHold;
						}
						else
						{
							item.Status = ARDocStatus.Balanced;
						}
					}
				
					return;
                }
	            if (item.OpenDoc == true)
	            {
		            item.Status = ARDocStatus.Open;
					return;
	            }
	            if (item.OpenDoc == false)
	            {
					item.Status = ARDocStatus.Closed;
					return;
				}
            }

			public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
			{
				ARRegister item = (ARRegister)e.Row;
				if (item != null)
					StatusSet(sender, item, item.Hold);
			}

			public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
			{
				ARRegister item = (ARRegister)e.Row;
				StatusSet(sender, item, item.Hold);
			}

			public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
			{
				ARRegister item = (ARRegister)e.NewRow;
				StatusSet(sender, item, item.Hold);
			}
		}
	}

	[Serializable]
	[ARRegisterCacheName(Messages.ARDocument)]
	public partial class ARRegisterReport : ARRegister
	{
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		public new abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
		public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		public new abstract class isRetainageDocument : PX.Data.BQL.BqlBool.Field<isRetainageDocument> { }

		#region SignBalance
		/// <summary>
		/// Read-only field indicating the sign of the document's impact on AR balance .
		/// Depends solely on the <see cref="DocType"/> field.
		/// </summary>
		/// <value>
		/// Possible values are: <c>1</c>, <c>-1</c> or <c>0</c>.
		/// </value>
		public abstract class signBalance : PX.Data.BQL.BqlDecimal.Field<signBalance> { }
		[PXDecimal()]
		[PXDependsOnFields(typeof(docType))]
		[PXFormula(typeof(
				Case<Where<ARRegister.docType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>))]
		[PXDBCalced(typeof(
				Case<Where<ARRegister.docType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>), typeof(Decimal))]
		public override Decimal? SignBalance
		{ get; set; }
		#endregion
		#region SignAmount
		public new abstract class signAmount : PX.Data.BQL.BqlDecimal.Field<signAmount> { }
		[PXDecimal()]
		[PXDependsOnFields(typeof(docType))]
		[PXFormula(typeof(
				Case<Where<ARRegister.docType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO, ARDocType.cashSale>>, decimal1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO, ARDocType.cashReturn>>, decimal_1>>))]
		[PXDBCalced(typeof(
				Case<Where<ARRegister.docType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO, ARDocType.cashSale>>, decimal1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO, ARDocType.cashReturn>>, decimal_1>>), typeof(Decimal))]
		public override Decimal? SignAmount
		{ get; set; }
		#endregion
		#region signReleasedRetainage
		public abstract class signReleasedRetainage : PX.Data.BQL.BqlDecimal.Field<signReleasedRetainage> { }
		[PXDecimal(4)]
		[PXDBCalced(typeof(
			Mult<
				ARRegisterReport.signAmount,
				ARRegisterReport.origDocAmt>
		), typeof(Decimal))]
		public virtual Decimal? SignReleasedRetainage { get; set; }
		#endregion
	}

	[PXProjection(typeof(Select2<ARRegister,
		InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ARRegister.customerID>>>>))]
	[PXBreakInheritance]
	[Serializable]
	public partial class ARRegisterAccess : Customer
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARRegister.docType))]
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
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(ARRegister.refNbr))]
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
		#region Scheduled
		public abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
		protected Boolean? _Scheduled;
		[PXDBBool(BqlField = typeof(ARRegister.scheduled))]
		public virtual Boolean? Scheduled
		{
			get
			{
				return this._Scheduled;
			}
			set
			{
				this._Scheduled = value;
			}
		}
		#endregion
		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		protected string _ScheduleID;
		[PXDBString(15, IsUnicode = true, BqlField = typeof(ARRegister.scheduleID))]
		public virtual string ScheduleID
		{
			get
			{
				return this._ScheduleID;
			}
			set
			{
				this._ScheduleID = value;
			}
		}
		#endregion
	}

	[PXProjection(typeof(
		SelectFrom<ARRegister>.
			CrossJoin<PX.SM.DateInfo>.
			LeftJoin<ARAdjustReport>.
				On<ARRegister.docType.IsEqual<ARAdjustReport.adjdDocType>.
					And<ARRegister.refNbr.IsEqual<ARAdjustReport.adjdRefNbr>>.
					And<ARAdjustReport.adjgDocDate.IsLessEqual<PX.SM.DateInfo.date>>.
					And<ARAdjustReport.adjdDocType.IsNotEqual<ARAdjustReport.adjgDocType>.Or<ARAdjustReport.adjdRefNbr.IsNotEqual<ARAdjustReport.adjgRefNbr>>>
				>.
		Where<Brackets<ARAdjustReport.released.IsEqual<True>.Or<ARAdjustReport.released.IsNull>>>.
		AggregateTo<
			GroupBy<ARRegister.docType>,
			GroupBy<ARRegister.refNbr>,
			GroupBy<PX.SM.DateInfo.date>,
			Sum<ARAdjustReport.lineTotalAdjusted>,
			Sum<ARAdjustReport.curyLineTotalAdjusted>
		>))]
	[Serializable]
	[PXCacheName("ARAdjustedBalanceAtDate")]
	public partial class ARAdjustedBalanceAtDate : IBqlTable
	{
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARRegister.docType))]
		public virtual String DocType { get; set; }

		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(ARRegister.refNbr))]
		public virtual String RefNbr { get; set; }

		public abstract class submissionDate : PX.Data.BQL.BqlType<Data.BQL.IBqlDateTime, DateTime>.Field<submissionDate> { }

		[PXDBDate(IsKey = true, BqlField = typeof(PX.SM.DateInfo.date))]
		public virtual DateTime? SubmissionDate { get; set; }

		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }

		[PXDBCalced(typeof(
			Sub<
				Mult<
					Switch<
						Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
					Add<ARAdjustReport.adjAmt, Add<ARAdjustReport.adjDiscAmt, ARAdjustReport.adjWOAmt>>>,
				ARAdjustReport.rGOLAmt>
			), typeof(Decimal))]
		[PXDecimal(4, BqlTable = typeof(ARAdjustReport))]
		public virtual Decimal? LineTotal { get; set; }

		[PXDBCalced(typeof(
			Mult<
				Switch<
					Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
					Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
				Add<ARAdjustReport.curyAdjdAmt, Add<ARAdjustReport.curyAdjdDiscAmt, ARAdjustReport.curyAdjdWOAmt>>>
			), typeof(Decimal))]
		[PXDecimal(4, BqlTable = typeof(ARAdjustReport))]
		public virtual Decimal? CuryLineTotal { get; set; }
	}


	[PXProjection(typeof(
		SelectFrom<ARRegister>.
			CrossJoin<PX.SM.DateInfo>.
			LeftJoin<ARAdjustReport>.
				On<ARRegister.docType.IsEqual<ARAdjustReport.adjgDocType>.
					And<ARRegister.refNbr.IsEqual<ARAdjustReport.adjgRefNbr>>.
					And<ARAdjustReport.adjgDocDate.IsLessEqual<PX.SM.DateInfo.date>>
				>.
		Where<ARAdjustReport.released.IsEqual<True>.Or<ARAdjustReport.released.IsNull>>.
		AggregateTo<
			GroupBy<ARRegister.docType>,
			GroupBy<ARRegister.refNbr>,
			GroupBy<PX.SM.DateInfo.date>,
			Sum<ARAdjustReport.lineTotalAdjusting>,
			Sum<ARAdjustReport.curyLineTotalAdjusting>
		>))]
	[Serializable]
	[PXCacheName("ARAdjustingBalanceAtDate")]
	public partial class ARAdjustingBalanceAtDate : IBqlTable
	{
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARRegister.docType))]
		public virtual String DocType { get; set; }

		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(ARRegister.refNbr))]
		public virtual String RefNbr { get; set; }

		public abstract class submissionDate : PX.Data.BQL.BqlType<Data.BQL.IBqlDateTime, DateTime>.Field<submissionDate> { }

		[PXDBDate(IsKey = true, BqlField = typeof(PX.SM.DateInfo.date))]
		public virtual DateTime? SubmissionDate { get; set; }

		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }

		[PXDBCalced(typeof(
			Mult<
				Switch<
					Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
					Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
				ARAdjustReport.adjAmt>
			), typeof(Decimal))]
		[PXDecimal(4, BqlTable = typeof(ARAdjustReport))]
		public virtual Decimal? LineTotal { get; set; }

		[PXDBCalced(typeof(
			Mult<
				Switch<
					Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjustReport.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
					Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
					Case<Where<ARAdjustReport.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
				ARAdjustReport.curyAdjgAmt>
			), typeof(Decimal))]
		[PXDecimal(4, BqlTable = typeof(ARAdjustReport))]
		public virtual Decimal? CuryLineTotal { get; set; }
	}

	[PXProjection(typeof(
		SelectFrom<ARRegister>.
			CrossJoin<PX.SM.DateInfo>.
			LeftJoin<ARRegisterReport>.
				On<ARRegister.docType.IsEqual<ARRegisterReport.origDocType>.
					And<ARRegister.refNbr.IsEqual<ARRegisterReport.origRefNbr>>.
					And<ARRegisterReport.docDate.IsLessEqual<PX.SM.DateInfo.date>>.
					And<ARRegisterReport.released.IsEqual<True>>.
					And<ARRegisterReport.isRetainageDocument.IsEqual<True>>
				>.
		AggregateTo<
			GroupBy<ARRegister.docType>,
			GroupBy<ARRegister.refNbr>,
			GroupBy<PX.SM.DateInfo.date>,
			Sum<ARRegisterReport.signReleasedRetainage>
		>))]
	[Serializable]
	[PXCacheName("ARInvoiceRetainageBalanceAtDate")]
	public partial class ARInvoiceRetainageBalanceAtDate : IBqlTable
	{
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARRegister.docType))]
		public virtual String DocType { get; set; }

		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(ARRegister.refNbr))]
		public virtual String RefNbr { get; set; }

		public abstract class submissionDate : PX.Data.BQL.BqlType<Data.BQL.IBqlDateTime, DateTime>.Field<submissionDate> { }

		[PXDBDate(IsKey = true, BqlField = typeof(PX.SM.DateInfo.date))]
		public virtual DateTime? SubmissionDate { get; set; }

		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }

		[PXDBCalced(typeof(
			Mult<
				ARRegisterReport.signAmount,
				ARRegisterReport.origDocAmt>
		), typeof(Decimal))]
		[PXDecimal(4, BqlTable = typeof(ARRegisterReport))]
		public virtual Decimal? LineTotal { get; set; }
	}

	[PXProjection(typeof(
		SelectFrom<ARInvoice>.
		Where<ARInvoice.released.IsEqual<True>.
			And<ARInvoice.isRetainageDocument.IsEqual<True>>>
		))]
	[Serializable]
	[PXCacheName("ARInvoiceRetainageDocument")]
	public partial class ARInvoiceRetainageDocument : IBqlTable
	{
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARInvoice.docType))]
		public virtual String DocType { get; set; }

		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(ARInvoice.refNbr))]
		public virtual String RefNbr { get; set; }

		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARInvoice.origDocType))]
		public virtual String OrigDocType { get; set; }

		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(ARInvoice.origRefNbr))]
		public virtual String OrigRefNbr { get; set; }

		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

		[PXDBDate(IsKey = true, BqlField = typeof(ARInvoice.docDate))]
		public virtual DateTime? DocDate { get; set; }

		public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }

		[PXDBDecimal(4, BqlField = typeof(ARInvoice.docBal))]
		public virtual Decimal? DocBal { get; set; }
	}

	[Serializable]
	public partial class ARRegisterAR622000 : IBqlTable
	{
		#region RefNbr
		[PXSelector(typeof(Search2<ARRegister.refNbr,
			InnerJoinSingleTable<Customer, On<ARRegister.customerID, Equal<Customer.bAccountID>>>,
			Where<ARRegister.docType, Equal<Optional<ARRegister.docType>>,
			And<ARRegister.released, Equal<True>,
			And2<Where<ARRegister.finPeriodID, GreaterEqual<Optional<ARRegister.finPeriodID>>, Or<Optional<ARRegister.closedFinPeriodID>, IsNull>>,
			And2<Where<ARRegister.finPeriodID, LessEqual<Optional<ARRegister.tranPeriodID>>, Or<Optional<ARRegister.closedTranPeriodID>, IsNull>>,
			And<Match<Customer, Current<AccessInfo.userName>>>>>>>, OrderBy<Desc<ARRegister.refNbr>>>), Filterable = true)]
		public String RefNbr { get; set; }
		#endregion
	}

	[Serializable]
	public partial class ARRegisterAR610500 : IBqlTable
	{
		#region RefNbr
		[PXSelector(typeof(Search2<ARRegister.refNbr,
			InnerJoinSingleTable<Customer, On<ARRegister.customerID, Equal<Customer.bAccountID>>>,
			Where<ARRegister.docType, Equal<Optional<ARRegister.docType>>,
				And2<Where<ARRegister.finPeriodID, GreaterEqual<Optional<ARRegister.finPeriodID>>, Or<Optional<ARRegister.closedFinPeriodID>, IsNull>>,
				And2<Where<ARRegister.finPeriodID, LessEqual<Optional<ARRegister.tranPeriodID>>, Or<Optional<ARRegister.closedTranPeriodID>, IsNull>>,
				And2<Where<ARRegister.hold, Equal<False>, 
					And<ARRegister.scheduled, Equal<False>, 
					And<ARRegister.voided, Equal<False>>>>, 
				And<Match<Customer, Current<AccessInfo.userName>>>>>>>, 
			OrderBy<Desc<ARRegister.refNbr>>>), Filterable = true)]
		public String RefNbr { get; set; }
		#endregion
	}
}
