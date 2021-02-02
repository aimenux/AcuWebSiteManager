using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.AR.BQL;
using PX.Objects.AR.Repositories;
using PX.Objects.CA;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.Common;
using PX.Objects.Common.Attributes;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Tools;
using PX.Objects.Common.MigrationMode;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
	public class ARDocumentEnq : PXGraph<ARDocumentEnq>
	{
		#region Internal Types
		[Serializable]
		public partial class ARDocumentFilter : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

			[Organization(false, Required = false)]
			public int? OrganizationID { get; set; }
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

			[BranchOfOrganization(typeof(ARDocumentFilter.organizationID), false)]
			public int? BranchID { get; set; }
			#endregion
			#region OrgBAccountID
			public abstract class orgBAccountID : IBqlField { }

			[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
			public int? OrgBAccountID { get; set; }
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[PXDefault()]
			[Customer(DescriptionField = typeof(Customer.acctName))]
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
			#region ARAcctID
			public abstract class aRAcctID : PX.Data.BQL.BqlInt.Field<aRAcctID> { }
			protected Int32? _ARAcctID;
			[GL.Account(null, typeof(Search5<Account.accountID,
					InnerJoin<ARHistory, On<Account.accountID, Equal<ARHistory.accountID>>>,
					Where<Match<Current<AccessInfo.userName>>>,
					Aggregate<GroupBy<Account.accountID>>>),
			   DisplayName = "AR Account", DescriptionField = typeof(GL.Account.description))]
			public virtual Int32? ARAcctID
			{
				get
				{
					return this._ARAcctID;
				}
				set
				{
					this._ARAcctID = value;
				}
			}
			#endregion
			#region ARSubID
			public abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
			protected Int32? _ARSubID;
			[GL.SubAccount(DisplayName = "AR Sub.", DescriptionField = typeof(GL.Sub.description))]
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
			#region SubCD
			public abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }
			protected String _SubCD;
			[PXDBString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "AR Subaccount", Visibility = PXUIVisibility.Invisible, FieldClass = SubAccountAttribute.DimensionName)]
			[PXDimension("SUBACCOUNT", ValidComboRequired = false)]
			public virtual String SubCD
			{
				get
				{
					return this._SubCD;
				}
				set
				{
					this._SubCD = value;
				}
			}
			#endregion
			#region UseMasterCalendar
			public abstract class useMasterCalendar : PX.Data.BQL.BqlBool.Field<useMasterCalendar> { }

			[PXDBBool]
			[PXUIField(DisplayName = Common.Messages.UseMasterCalendar)]
			[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.multipleCalendarsSupport>))]
			public bool? UseMasterCalendar { get; set; }
			#endregion

			#region Period
			public abstract class period : PX.Data.BQL.BqlString.Field<period> { }

			[AnyPeriodFilterable(null, null,
				branchSourceType: typeof(ARDocumentFilter.branchID),
				organizationSourceType: typeof(ARDocumentFilter.organizationID),
				useMasterCalendarSourceType: typeof(ARDocumentFilter.useMasterCalendar),
				redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
			[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
			public virtual string Period
				{
				get;
				set;
			}
			#endregion
			#region MasterFinPeriodID
			public abstract class masterFinPeriodID : PX.Data.BQL.BqlString.Field<masterFinPeriodID> { }
			[Obsolete("This is an absolete field. It will be removed in 2019R2")]
			[PeriodID]
			public virtual string MasterFinPeriodID { get; set; }
			#endregion
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			protected String _DocType;
			[PXDBString(3, IsFixed = true)]
			[PXDefault()]
			[ARDocType.List()]
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
			#region ShowAllDocs
			public abstract class showAllDocs : PX.Data.BQL.BqlBool.Field<showAllDocs> { }
			protected bool? _ShowAllDocs;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Show All Documents")]
			public virtual bool? ShowAllDocs
			{
				get
				{
					return this._ShowAllDocs;
				}
				set
				{
					this._ShowAllDocs = value;
				}
			}
			#endregion
			#region IncludeUnreleased
			public abstract class includeUnreleased : PX.Data.BQL.BqlBool.Field<includeUnreleased> { }
			protected bool? _IncludeUnreleased;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Include Unreleased Documents")]
			public virtual bool? IncludeUnreleased
			{
				get
				{
					return this._IncludeUnreleased;
				}
				set
				{
					this._IncludeUnreleased = value;
				}
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXSelector(typeof(CM.Currency.curyID), CacheGlobal = true)]
			[PXUIField(DisplayName = "Currency")]
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
			#region SubCD Wildcard
			public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { };
			[PXDBString(30, IsUnicode = true)]
			public virtual String SubCDWildcard
			{
				get
				{
					return SubCDUtils.CreateSubCDWildcard(this._SubCD, SubAccountAttribute.DimensionName);
				}
			}



			#endregion
			#region RefreshTotals
			public abstract class refreshTotals : PX.Data.BQL.BqlBool.Field<refreshTotals> { }			
			[PXDBBool]
			[PXDefault(true)]
			public bool? RefreshTotals { get; set; }
			#endregion
			#region CuryBalanceSummary
			public abstract class curyBalanceSummary : PX.Data.BQL.BqlDecimal.Field<curyBalanceSummary> { }
			protected Decimal? _CuryBalanceSummary;
			[PXCury(typeof(ARDocumentFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Balance by Documents (Currency)", Enabled = false)]
			public virtual Decimal? CuryBalanceSummary
			{
				get
				{
					return this._CuryBalanceSummary;
				}
				set
				{
					this._CuryBalanceSummary = value;
				}
			}
			#endregion
			#region BalanceSummary
			public abstract class balanceSummary : PX.Data.BQL.BqlDecimal.Field<balanceSummary> { }
			protected Decimal? _BalanceSummary;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Balance by Documents", Enabled = false)]
			public virtual Decimal? BalanceSummary
			{
				get
				{
					return this._BalanceSummary;
				}
				set
				{
					this._BalanceSummary = value;
				}
			}
			#endregion
			#region CuryCustomerBalance
			public abstract class curyCustomerBalance : PX.Data.BQL.BqlDecimal.Field<curyCustomerBalance> { }
			protected Decimal? _CuryCustomerBalance;
			[PXCury(typeof(ARDocumentFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Current Balance (Currency)", Enabled = false)]
			public virtual Decimal? CuryCustomerBalance
			{
				get
				{
					return this._CuryCustomerBalance;
				}
				set
				{
					this._CuryCustomerBalance = value;
				}
			}
			#endregion
			#region CustomerBalance
			public abstract class customerBalance : PX.Data.BQL.BqlDecimal.Field<customerBalance> { }
			protected Decimal? _CustomerBalance;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Current Balance", Enabled = false)]
			public virtual Decimal? CustomerBalance
			{
				get
				{
					return this._CustomerBalance;
				}
				set
				{
					this._CustomerBalance = value;
				}
			}
			#endregion
			#region CuryVCustomerRetainedBalance
			public abstract class curyCustomerRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyCustomerRetainedBalance> { }
			[PXCury(typeof(ARDocumentFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Retained Balance (Currency)", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? CuryCustomerRetainedBalance
			{
				get;
				set;
			}
			#endregion
			#region CustomerRetainedBalance
			public abstract class customerRetainedBalance : PX.Data.BQL.BqlDecimal.Field<customerRetainedBalance> { }

			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Retained Balance", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? CustomerRetainedBalance
			{
				get;
				set;
			}
			#endregion
			#region CuryCustomerDepositsBalance
			public abstract class curyCustomerDepositsBalance : PX.Data.BQL.BqlDecimal.Field<curyCustomerDepositsBalance> { }

			protected Decimal? _CuryCustomerDepositsBalance;
			[PXCury(typeof(ARDocumentFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Prepayments Balance (Currency)", Enabled = false)]
			public virtual Decimal? CuryCustomerDepositsBalance
			{
				get
				{
					return this._CuryCustomerDepositsBalance;
				}
				set
				{
					this._CuryCustomerDepositsBalance = value;
				}
			}
			#endregion
			#region CustomerDepositsBalance

			public abstract class customerDepositsBalance : PX.Data.BQL.BqlDecimal.Field<customerDepositsBalance> { }
			protected Decimal? _CustomerDepositsBalance;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Prepayment Balance", Enabled = false)]
			public virtual Decimal? CustomerDepositsBalance
			{
				get
				{
					return this._CustomerDepositsBalance;
				}
				set
				{
					this._CustomerDepositsBalance = value;
				}
			}
			#endregion
			#region CuryDifference
			public abstract class curyDifference : PX.Data.BQL.BqlDecimal.Field<curyDifference> { }

			[PXCury(typeof(ARDocumentFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Balance Discrepancy (Currency)", Enabled = false)]
			public virtual Decimal? CuryDifference
			{
				[PXDependsOnFields(typeof(ARDocumentFilter.curyCustomerBalance),typeof(ARDocumentFilter.curyBalanceSummary),typeof(ARDocumentFilter.curyCustomerDepositsBalance))]
				get
				{
					return (this._CuryCustomerBalance - this._CuryBalanceSummary + this._CuryCustomerDepositsBalance);
				}
			}
			#endregion
			#region Difference
			public abstract class difference : PX.Data.BQL.BqlDecimal.Field<difference> { }
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Balance Discrepancy", Enabled = false)]
			public virtual Decimal? Difference
			{
				[PXDependsOnFields(typeof(ARDocumentFilter.customerBalance), typeof(ARDocumentFilter.balanceSummary), typeof(ARDocumentFilter.customerDepositsBalance))]
				get
				{
					return (this._CustomerBalance - this._BalanceSummary + this._CustomerDepositsBalance);
				}
			}
			#endregion
			#region IncludeChildAccounts
			public abstract class includeChildAccounts : PX.Data.BQL.BqlBool.Field<includeChildAccounts> { }

			[PXDBBool]
			[PXDefault(typeof(Search<CS.FeaturesSet.parentChildAccount>))]
			[PXUIField(DisplayName = "Include Child Accounts")]
			public virtual bool? IncludeChildAccounts { get; set; }
			#endregion
			#region IncludeGLTurnover
			public abstract class includeGLTurnover : PX.Data.BQL.BqlBool.Field<includeGLTurnover> { }
			protected bool? _IncludeGLTurnover;
			[PXDBBool()]
			[PXDefault(false)]
			public virtual bool? IncludeGLTurnover
			{
				get
				{
					return this._IncludeGLTurnover;
				}
				set
				{
					this._IncludeGLTurnover = value;
				}
			}
			#endregion
			#region FilterDetails		
			public abstract class filterDetails : IBqlField { };
			[PXResultStorage]
			public byte[][] FilterDetails { get; set; }
			#endregion

			public virtual void ClearSummary()
			{
				CustomerBalance = Decimal.Zero;
				BalanceSummary = Decimal.Zero;
				CustomerDepositsBalance = Decimal.Zero;
				CuryCustomerBalance = Decimal.Zero;
				CuryBalanceSummary = Decimal.Zero;
				CuryCustomerDepositsBalance = Decimal.Zero; 
				CuryCustomerRetainedBalance = Decimal.Zero;
				CustomerRetainedBalance = Decimal.Zero;
			}

		}

		[Serializable()]
		[PXPrimaryGraph(typeof(ARDocumentEnq), Filter = typeof(ARDocumentFilter))]
		[PXCacheName(Messages.ARHistoryForReport)]
		public partial class ARHistoryForReport : ARHistory { }

		[Serializable()]
		[PXProjection(typeof(Select<ARRegister>))]
		[PXPrimaryGraph(new Type[] {
				typeof(SO.SOInvoiceEntry),
				typeof(ARCashSaleEntry),
				typeof(ARInvoiceEntry),
				typeof(ARPaymentEntry)
			},
			new Type[] {
				typeof(Select<ARInvoice,
					Where<ARInvoice.docType, Equal<Current<ARDocumentResult.docType>>,
						And<ARInvoice.refNbr, Equal<Current<ARDocumentResult.refNbr>>,
						And<ARInvoice.origModule, Equal<BatchModule.moduleSO>,
						And<ARInvoice.released, Equal<False>>>>>>),
				typeof(Select<Standalone.ARCashSale,
					Where<Standalone.ARCashSale.docType, Equal<Current<ARDocumentResult.docType>>,
						And<Standalone.ARCashSale.refNbr, Equal<Current<ARDocumentResult.refNbr>>>>>),
				typeof(Select<ARInvoice,
					Where<ARInvoice.docType, Equal<Current<ARDocumentResult.docType>>,
						And<ARInvoice.refNbr, Equal<Current<ARDocumentResult.refNbr>>>>>),
				typeof(Select<ARPayment,
					Where<ARPayment.docType, Equal<Current<ARDocumentResult.docType>>,
						And<ARPayment.refNbr, Equal<Current<ARDocumentResult.refNbr>>>>>)
			})]
		public partial class ARDocumentResult : IBqlTable
		{
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType>
			{
				public const int Length = 3;
			}
			
			/// <summary>
			/// The type of the document.
			/// This field is a part of the compound key of the document.
			/// </summary>
			/// <value>
			/// The field can have one of the values described in <see cref="ARDocType.ListAttribute"/>.
			/// </value>
			[PXDBString(docType.Length, IsKey = true, IsFixed = true, BqlTable = typeof(ARRegister))]
			[PXDefault()]
			[ARDocType.List()]
			[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
			public virtual String DocType
			{
				get;
				set;
			}
			#endregion
			#region RefNbr

			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
			}

			/// <summary>
			/// The reference number of the document.
			/// This field is a part of the compound key of the document.
			/// </summary>
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlTable = typeof(ARRegister))]
			[PXDefault()]
			[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
			[PXSelector(typeof(Search<ARRegister.refNbr, Where<ARRegister.docType, Equal<Optional<ARRegister.docType>>>>), Filterable = true)]
			public virtual String RefNbr
			{
				get;
				set;
			}
			#endregion
			#region DisplayDocType
			public abstract class displayDocType : PX.Data.BQL.BqlString.Field<displayDocType> { }
			protected String _DisplayDocType;
			[PXString(3, IsFixed = true)]
			[ARDisplayDocType.List()]
			[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
			public virtual String DisplayDocType
			{
				[PXDependsOnFields(typeof(docType))]
				get => (String.IsNullOrEmpty(this._DisplayDocType) ? this.DocType : this._DisplayDocType);
				set => this._DisplayDocType = value;
			}
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			/// <summary>
			/// The identifier of the <see cref="Branch">branch</see> to which the document belongs.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="Branch.BranchID"/> field.
			/// </value>
			[Branch(BqlTable=typeof(ARRegister))]
			public virtual Int32? BranchID
			{
				get;
				set;
			}
			#endregion
			#region DocDate
			public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
			/// <summary>
			/// The date of the document.
			/// </summary>
			/// <value>
			/// Defaults to the current <see cref="AccessInfo.BusinessDate">Business Date</see>.
			/// </value>
			[PXDBDate(BqlTable = typeof(ARRegister))]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual DateTime? DocDate
			{
				get;
				set;
			}
			#endregion
			#region DocDesc
			public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
			
			/// <summary>
			/// The description of the document.
			/// </summary>
			[PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlTable = typeof(ARRegister))]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String DocDesc
			{
				get;
				set;
			}
			#endregion
			#region DueDate
			public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
			[PXDBDate(BqlTable = typeof(ARRegister))]
			[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual DateTime? DueDate
			{
				get;
				set;
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			/// <summary>
			/// The code of the <see cref="Currency"/> of the document.
			/// </summary>
			/// <value>
			/// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
			/// Corresponds to the <see cref="Currency.CuryID"/> field.
			/// </value>
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlTable = typeof(ARRegister))]
			[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDefault(typeof(Search<Company.baseCuryID>))]
			[PXSelector(typeof(Currency.curyID))]
			public virtual String CuryID
			{
				get;
				set;
			}
			#endregion
			#region CuryInfoID
			public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
			
			/// <summary>
			/// The identifier of the <see cref="CurrencyInfo">CurrencyInfo</see> object associated with the document.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="CurrencyInfoID"/> field.
			/// </value>
			[PXDBLong(BqlTable = typeof(ARRegister))]
			[CurrencyInfo(ModuleCode = BatchModule.AR)]
			public virtual Int64? CuryInfoID
			{
				get;
				set;
			}
			#endregion
			#region OrigDocType
			public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
			
			/// <summary>
			/// The type of the original (source) document.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="DocType"/> field.
			/// </value>
			[PXDBString(3, IsFixed = true, BqlTable=typeof(ARRegister))]
			[ARDocType.List()]
			[PXUIField(DisplayName = "Orig. Doc. Type")]
			public virtual String OrigDocType
			{
				get;
				set;
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
			[PXDBString(15, IsUnicode = true, InputMask = "", BqlTable = typeof(ARRegister))]
			[PXUIField(DisplayName = "Orig. Ref. Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual string OrigRefNbr
			{
				get;
				set;
			}
			#endregion
			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			/// <summary>
			/// The status of the document.
			/// The value of the field is determined by the values of the status flags,
			/// such as <see cref="Hold"/>, <see cref="Released"/>, <see cref="Voided"/>, <see cref="Scheduled"/>.
			/// </summary>
			/// <value>
			/// The field can have one of the values described in <see cref="ARDocStatus.ListAttribute"/>.
			/// Defaults to <see cref="ARDocStatus.Hold"/>.
			/// </value>
			[PXDBString(1, IsFixed = true, BqlTable=typeof(ARRegister))]
			[PXDefault(ARDocStatus.Hold)]
			[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[ARDocStatus.List]
			[PXDependsOnFields(
				typeof(ARRegister.voided),
				typeof(ARRegister.hold),
				typeof(ARRegister.scheduled),
				typeof(ARRegister.released),
				typeof(ARRegister.approved),
				typeof(ARRegister.rejected),
				typeof(ARRegister.dontApprove),
				typeof(ARRegister.openDoc),
				typeof(ARRegister.docType))]
			public virtual string Status
			{
				get;
				set;
			}
			#endregion
			#region TranPeriodID
			public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
			
			/// <summary>
			/// <see cref="FinPeriod">Financial Period</see> of the document.
			/// </summary>
			/// <value>
			/// Determined by the <see cref="ARRegister.DocDate">date of the document</see>. Unlike <see cref="ARRegister.FinPeriodID"/>
			/// the value of this field can't be overriden by user.
			/// </value>
			[PeriodID(BqlTable = typeof(ARRegister))]
			public virtual String TranPeriodID
			{
				get;
				set;
			}
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			
			/// <summary>
			/// <see cref="FinPeriod">Financial Period</see> of the document.
			/// </summary>
			/// <value>
			/// Defaults to the period, to which the <see cref="ARRegister.DocDate"/> belongs, but can be overriden by user.
			/// </value>
			[AROpenPeriod(
				typeof(docDate),
				branchSourceType: typeof(branchID),
				masterFinPeriodIDType: typeof(tranPeriodID),
				IsHeader = true, 
				BqlTable=typeof(ARRegister))]
			[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String FinPeriodID
			{
				get;
				set;
			}
			#endregion
			#region ClosedFinPeriodID
			public abstract class closedFinPeriodID : PX.Data.BQL.BqlString.Field<closedFinPeriodID> { }
			
			/// <summary>
			/// The <see cref="FinancialPeriod">Financial Period</see>, in which the document was closed.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="FinPeriodID"/> field.
			/// </value>
			[FinPeriodID(
				branchSourceType: typeof(branchID),
				masterFinPeriodIDType: typeof(closedTranPeriodID),
				BqlTable = typeof(ARRegister))]
			[PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
			public virtual String ClosedFinPeriodID
			{
				get;
				set;
			}
			#endregion
			#region ClosedTranPeriodID
			public abstract class closedTranPeriodID : PX.Data.BQL.BqlString.Field<closedTranPeriodID> { }
			
			/// <summary>
			/// The <see cref="FinancialPeriod">Financial Period</see>, in which the document was closed.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="TranPeriodID"/> field.
			/// </value>
			[PeriodID(BqlTable = typeof(ARRegister))]
			[PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
			public virtual String ClosedTranPeriodID
			{
				get;
				set;
			}
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
			[PXNote(BqlTable=typeof(ARRegister))]
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
			#region SelfVoidingDoc
			public abstract class selfVoidingDoc : PX.Data.BQL.BqlBool.Field<selfVoidingDoc> { }

			/// <summary>
			/// When <c>true</c>, indicates that the document can be voided only in full and 
			/// it is not allow to delete reversing applications partially.
			/// </summary>
			[PXBool()]
			[PXFormula(typeof(IIf<Where<IsSelfVoiding<docType>>, True, False>))]
			[PXDBCalced(typeof(IIf<Where<IsSelfVoiding<docType>>, True, False>), typeof(bool))]
			public virtual bool? SelfVoidingDoc { get; set; }
			#endregion
			#region OpenDoc
			public abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
			
			/// <summary>
			/// When set to <c>true</c>, indicates that the document is open.
			/// </summary>
			[PXDBBool(BqlTable=typeof(ARRegister))]
			[PXDefault(true)]
			public virtual Boolean? OpenDoc
			{
				get;
				set;
			}
			#endregion
			#region Released
			public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			
			/// <summary>
			/// When set to <c>true</c>, indicates that the document has been released.
			/// </summary>
			[PXDBBool(BqlTable=typeof(ARRegister))]
			[PXDefault(false)]
			public virtual Boolean? Released
			{
				get;
				set;
			}
			#endregion
			#region IsRetainageDocument
			public abstract class isRetainageDocument : PX.Data.BQL.BqlBool.Field<isRetainageDocument> { }

			[PXDBBool(BqlTable=typeof(ARRegister))]
			[PXUIField(DisplayName = "Retainage Invoice", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual bool? IsRetainageDocument
			{
				get;
				set;
			}
			#endregion
			#region ARAccountID
			public abstract class aRAccountID : PX.Data.BQL.BqlInt.Field<aRAccountID> { }
			
			/// <summary>
			/// The identifier of the <see cref="Account">AR account</see> to which the document should be posted.
			/// The Cash account and Year-to-Date Net Income account cannot be selected as the value of this field.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="Account.AccountID"/> field.
			/// </value>
			[PXDefault]
			[Account(typeof(branchID), typeof(Search<Account.accountID,
				Where2<Match<Current<AccessInfo.userName>>,
					And<Account.active, Equal<True>,
						And<Account.isCashAccount, Equal<False>,
							And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
								Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AR Account", 
				BqlTable = typeof(ARRegister))]
			public virtual Int32? ARAccountID
			{
				get;
				set;
			}
			#endregion
			#region ARSubID
			public abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
			
			/// <summary>
			/// The identifier of the <see cref="Sub">subaccount</see> to which the document should be posted.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="Sub.SubID"/> field.
			/// </value>
			[PXDefault]
			[SubAccount(typeof(aRAccountID), DescriptionField = typeof(Sub.description), DisplayName = "AR Subaccount", Visibility = PXUIVisibility.Visible, 
				BqlTable = typeof(ARRegister))]
			public virtual Int32? ARSubID
			{
				get;
				set;
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
			#region BatchNbr
			public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }

			/// <summary>
			/// The number of the <see cref="Batch"/> created from the document on release.
			/// </summary>
			/// <value>
			/// Corresponds to the <see cref="Batch.BatchNbr"/> field.
			/// </value>
			[PXDBString(15, IsUnicode = true, BqlTable=typeof(ARRegister))]
			[PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
			[BatchNbr(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleAR>>>),
				IsMigratedRecordField = typeof(ARRegister.isMigratedRecord))]
			public virtual string BatchNbr
			{
				get;
				set;
			}
			#endregion
			#region CuryOrigDiscAmt
			public abstract class curyOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDiscAmt> { }
			
			/// <summary>
			/// The cash discount entered for the document.
			/// Given in the <see cref="CuryID">currency of the document</see>.
			/// </summary>
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBCurrency(typeof(curyInfoID), typeof(origDiscAmt), BqlTable = typeof(ARRegister))]
			[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Decimal? CuryOrigDiscAmt
			{
				get;
				set;
			}
			#endregion
			#region OrigDiscAmt
			public abstract class origDiscAmt : PX.Data.BQL.BqlDecimal.Field<origDiscAmt> { }
			
			/// <summary>
			/// The cash discount entered for the document.
			/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
			/// </summary>
			[PXDBBaseCury(BqlTable = typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? OrigDiscAmt
			{
				get;
				set;
			}
			#endregion

			#region Scheduled
			public abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
			
			/// <summary>
			/// When set to <c>true</c> indicates that the document is part of a <c>Schedule</c> and serves as a template for generating other documents according to it.
			/// </summary>
			[PXDBBool(BqlTable=typeof(ARRegister))]
			[PXDefault(false)]
			public virtual Boolean? Scheduled
				{
				get;
				set;
				}
			#endregion
			#region Voided
			public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
			
			/// <summary>
			/// When set to <c>true</c> indicates that the document has been voided.
			/// </summary>
			[PXDBBool(BqlTable = typeof(ARRegister))]
			[PXDefault(false)]
			public virtual Boolean? Voided
				{
				get;
				set;
				}
			#endregion
			#region CuryOrigDocAmt
			public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
			
			/// <summary>
			/// The amount of the document.
			/// Given in the <see cref="CuryID">currency of the document</see>.
			/// </summary>
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.origDocAmt),
				BqlTable = typeof(ARRegister))]
			[PXUIField(DisplayName = "Currency Origin. Amount", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Decimal? CuryOrigDocAmt
			{
				get;
				set;
			}
			#endregion
			#region OrigDocAmt
			public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }

			/// <summary>
			/// The amount of the document.
			/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
			/// </summary>
			[PXDBBaseCury(BqlTable = typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Origin. Amount", Visibility = PXUIVisibility.SelectorVisible)]

			public virtual Decimal? OrigDocAmt
			{
				get;
				set;
			}
			#endregion

			#region CuryDiscTaken
			public abstract class curyDiscTaken : PX.Data.BQL.BqlDecimal.Field<curyDiscTaken> { }
			
			/// <summary>
			/// The <see cref="ARAdjust.CuryAdjdDiscAmt">cash discount amount</see> actually applied to the document.
			/// Given in the <see cref="CuryID">currency of the document</see>.
			/// </summary>
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.discTaken), 
				BqlTable = typeof(ARRegister))]
			public virtual Decimal? CuryDiscTaken
				{
				get;
				set;
				}
			#endregion
			#region DiscTaken
			public abstract class discTaken : PX.Data.BQL.BqlDecimal.Field<discTaken> { }
			
			/// <summary>
			/// The <see cref="ARAdjust.CuryAdjdDiscAmt">cash discount amount</see> actually applied to the document.
			/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
			/// </summary>
			[PXDBBaseCury(BqlTable = typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? DiscTaken
				{
				get;
				set;
				}
			#endregion
			#region CuryDiscBal
			public abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
			
			/// <summary>
			/// The cash discount balance of the document.
			/// Given in the <see cref="CuryID">currency of the document</see>.
			/// </summary>
			[PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.discBal), BaseCalc = false, 
				BqlTable = typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? CuryDiscBal
			{
				get;
				set;
			}
			#endregion
			#region DiscBal
			public abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
			
			/// <summary>
			/// The cash discount balance of the document.
			/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
			/// </summary>
			[PXDBBaseCury(BqlTable = typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public virtual Decimal? DiscBal
			{
				get;
				set;
			}
			#endregion
			#region CuryRetainageTotal
			public abstract class curyRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainageTotal> { }

			[PXDBCurrency(typeof(curyInfoID), typeof(retainageTotal),
				BqlTable = typeof(ARRegister))]
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

			[PXDBBaseCury(BqlTable = typeof(ARRegister))]
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

			[PXDBCurrency(typeof(curyInfoID), typeof(retainageUnreleasedAmt),
				BqlTable = typeof(ARRegister))]
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

			[PXDBBaseCury(BqlTable = typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Unreleased Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual decimal? RetainageUnreleasedAmt
			{
				get;
				set;
			}
			#endregion
			#region CuryRetainedTaxTotal
			public abstract class curyRetainedTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxTotal> { }

			[PXDBCurrency(typeof(curyInfoID), typeof(retainedTaxTotal),
				BqlTable = typeof(ARRegister))]
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

			[PXDBBaseCury(BqlTable = typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual decimal? RetainedTaxTotal
				{
				get;
				set;
				}
			#endregion
			#region CuryRetainedDiscTotal
			public abstract class curyRetainedDiscTotal : PX.Data.BQL.BqlDecimal.Field<curyRetainedDiscTotal> { }

			[PXDBCurrency(typeof(curyInfoID), typeof(retainedDiscTotal),
				BqlTable = typeof(ARRegister))]
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

			[PXDBBaseCury(BqlTable = typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual decimal? RetainedDiscTotal
			{
				get;
				set;
			} 
			#endregion
			#region CuryOrigDocAmtWithRetainageTotal
			public abstract class curyOrigDocAmtWithRetainageTotal : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmtWithRetainageTotal> { }

			[PXCury(typeof(curyID))]
			[PXUIField(DisplayName = "Total Amount", FieldClass = nameof(FeaturesSet.Retainage))]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			[PXFormula(typeof(Add<curyOrigDocAmt, curyRetainageTotal>))]
			public virtual decimal? CuryOrigDocAmtWithRetainageTotal
			{
				get;
				set;
			}
			#endregion
			#region OrigDocAmtWithRetainageTotal
			public abstract class origDocAmtWithRetainageTotal : PX.Data.BQL.BqlDecimal.Field<origDocAmtWithRetainageTotal> { }

			[PXBaseCury(BqlTable = typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Total Amount", FieldClass = nameof(FeaturesSet.Retainage))]
			[PXFormula(typeof(Add<origDocAmt, retainageTotal>))]
			public virtual decimal? OrigDocAmtWithRetainageTotal
				{
				get;
				set;
				}
			#endregion
			#region CuryDocBal
			public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }

			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBCurrency(typeof(curyInfoID), typeof(curyDocBal), BqlTable = typeof(ARRegister))]
			[PXUIField(DisplayName = "Currency Balance")]
			public virtual Decimal? CuryDocBal
				{
				get;
				set;
				}
			#endregion
			#region DocBal
			public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }

			[PXDBBaseCury(BqlTable=typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Balance")]
			public virtual Decimal? DocBal
			{
				get;
				set;
			}
			#endregion
			#region ARTurnover
			public abstract class aRTurnover : PX.Data.BQL.BqlDecimal.Field<aRTurnover> { }

			/// <summary>
			/// Expected GL turnover for the document.
			/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
			/// </summary>
			[PXBaseCury(BqlTable=typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "AR Turnover")]
			[PXDBCalced(typeof(
				Mult<
				Switch<
					Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<tranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, origDocAmt, 
					Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<finPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, origDocAmt>>,
					decimal0>,
				Case<Where<ARRegister.released, Equal<False>>, decimal0,
				Case<Where<ARRegister.docType.IsIn<ARDocType.smallBalanceWO, ARDocType.smallCreditWO>>, decimal_1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>
			), typeof(decimal))]
			public virtual decimal? ARTurnover
			{
				get;
				set;
			}
			#endregion
			#region GLTurnover
			public abstract class gLTurnover : PX.Data.BQL.BqlDecimal.Field<gLTurnover> { }

			/// <summary>
			/// Expected GL turnover for the document.
			/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
			/// </summary>
			[PXBaseCury()]
			[PXUIField(DisplayName = "GL Turnover")]
			public virtual decimal? GLTurnover
			{
				get;
				set;
			}
			#endregion
			#region RGOLAmt
			public abstract class rGOLAmt : PX.Data.BQL.BqlDecimal.Field<rGOLAmt> { }

			[PXDBBaseCury(BqlTable = typeof(ARRegister))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "RGOL Amount")]
			public virtual Decimal? RGOLAmt
			{
				get;
				set;
			}
			#endregion
			#region ExtRefNbr
			public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
			[PXString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "Customer Invoice Nbr./Payment Nbr.")]
			public virtual String ExtRefNbr
			{
				get;
				set;
			}
			#endregion
			#region PaymentMethodID
			public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
			[PXString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXUIField(DisplayName = "Payment Method")]
			public virtual String PaymentMethodID
			{
				get;
				set;
			}
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

			[Customer(Enabled = false, Visible = false, DescriptionField = typeof(Customer.acctName), BqlTable = typeof(ARRegister))]
			public virtual Int32? CustomerID
			{
				get;
				set;
			}
			#endregion
			#region CuryBegBalance
			public abstract class curyBegBalance : PX.Data.BQL.BqlDecimal.Field<curyBegBalance> { }
			protected Decimal? _CuryBegBalance;
			[PXCury(typeof(curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Currency Period Beg. Balance")]
			[PXDBCalced(typeof(
				Switch<
					Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<tranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>,decimal0, 
					Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<finPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal0>>,
					curyOrigDocAmt>
				), typeof(decimal))]
			public virtual Decimal? CuryBegBalance
			{
				get;
				set;
			}
			#endregion
			#region BegBalance
			public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
			[PXBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Period Beg. Balance")]
			[PXDBCalced(typeof(
				Switch<
					Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<tranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal0,
						Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<finPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal0>>,
					origDocAmt>
			), typeof(decimal))]
			public virtual Decimal? BegBalance
			{
				get;
				set;
			}
			#endregion
			
			#region CuryDiscActTaken
			public abstract class curyDiscActTaken : PX.Data.BQL.BqlDecimal.Field<curyDiscActTaken> { }
			protected Decimal? _CuryDiscActTaken;
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXCury(typeof(ARRegister.curyID))]
			[PXUIField(DisplayName = "Currency Cash Discount Taken")]
			public virtual Decimal? CuryDiscActTaken
			{
				get;
				set;
			}
			#endregion
			#region DiscActTaken
			public abstract class discActTaken : PX.Data.BQL.BqlDecimal.Field<discActTaken> { }
			protected Decimal? _DiscActTaken;
			[PXBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Cash Discount Taken")]
			public virtual Decimal? DiscActTaken
			{
				get;
				set;
			}
			#endregion
			
			#region Payable
			public virtual Boolean? Payable
			{
				[PXDependsOnFields(typeof(isComplementary))]
				get
				{

					if(this.IsComplementary != true )
						return  ARDocType.Payable(this.DocType);
					else
						return (ARDocType.Payable(this.DocType) == false); //Invert type
				}
				set
				{
				}
			}
			#endregion
			#region Paying
			public virtual Boolean? Paying
			{
				[PXDependsOnFields(typeof(isComplementary))]
				get
				{
					if (this.IsComplementary != true)
						return (ARDocType.Payable(this.DocType) == false);
					else
						return (ARDocType.Payable(this.DocType) == false) == false; //Invert Sign
				}
				set
				{
				}
			}
			#endregion
			#region SignBalance
			public abstract class signBalance : PX.Data.IBqlField { }
			[PXDecimal()]
			[PXDependsOnFields(typeof(docType))]
		    [PXDBCalced(typeof(
				Case<Where<ARRegister.docType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO, ARDocType.cashReturn>>, decimal1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO, ARDocType.cashSale>>, decimal_1
				>>), typeof(Decimal))]
			public virtual Decimal? SignBalance
			{ get; set; }
			#endregion
			#region IsComplementary
			public abstract class isComplementary : PX.Data.BQL.BqlBool.Field<isComplementary> { }

			public Boolean? IsComplementary
			{
				get;
				set;
			}
			#endregion
			#region IsTurn
			public abstract class isTurn : PX.Data.BQL.BqlBool.Field<isTurn> { }

			public Boolean? IsTurn
			{
				get;
				set;
			}
			#endregion
		}

		[System.SerializableAttribute()]
		[PXHidden()]
		public partial class ARAdjust : PX.Objects.AR.ARAdjust
				{
			public new abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }

			public new abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }

			public new abstract class adjgDocDate : PX.Data.BQL.BqlDateTime.Field<adjgDocDate> { }

			public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

			public new abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }

			public new abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }

			public new abstract class curyAdjdAmt : PX.Data.BQL.BqlString.Field<curyAdjdAmt> { }

			public new abstract class curyAdjdDiscAmt : PX.Data.BQL.BqlString.Field<curyAdjdDiscAmt> { }

			public new abstract class curyAdjdWOAmt : PX.Data.BQL.BqlString.Field<curyAdjdWOAmt> { }

			public new abstract class rGOLAmt : PX.Data.BQL.BqlString.Field<rGOLAmt> { }

			public new abstract class adjAmt : PX.Data.BQL.BqlString.Field<adjAmt> { }

			public new abstract class adjDiscAmt : PX.Data.BQL.BqlString.Field<adjDiscAmt> { }

			public new abstract class adjWOAmt : PX.Data.BQL.BqlString.Field<adjWOAmt> { }

			public new abstract class curyAdjgAmt : PX.Data.BQL.BqlString.Field<curyAdjgAmt> { }
			

			#region LineTotalAdjusted
			public abstract class lineTotalAdjusted : PX.Data.BQL.BqlDecimal.Field<lineTotalAdjusted> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Sub<
					Mult<
						Switch<
							Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
							Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
							Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
							Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
							Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
							Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
						Add<ARAdjust.adjAmt, Add<ARAdjust.adjDiscAmt, ARAdjust.adjWOAmt>>>,
					ARAdjust.rGOLAmt>
				), typeof(Decimal))]
			public virtual Decimal? LineTotalAdjusted { get; set; }
			#endregion

			#region CuryLineTotalAdjusted
			public abstract class curyLineTotalAdjusted : PX.Data.BQL.BqlDecimal.Field<curyLineTotalAdjusted> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<
					Switch<
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
					Add<ARAdjust.curyAdjdAmt, Add<ARAdjust.curyAdjdDiscAmt, ARAdjust.curyAdjdWOAmt>>>
				), typeof(Decimal))]
			public virtual Decimal? CuryLineTotalAdjusted { get; set; }
			#endregion

			#region LineTotalAdjusting
			public abstract class lineTotalAdjusting : PX.Data.BQL.BqlDecimal.Field<lineTotalAdjusting> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<
					Switch<
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
					ARAdjust.adjAmt>
				), typeof(Decimal))]
			public virtual Decimal? LineTotalAdjusting { get; set; }
			#endregion

			#region LineTotalRGOL
			public abstract class lineTotalRGOL : PX.Data.BQL.BqlDecimal.Field<lineTotalRGOL> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<
					Switch<
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
					ARAdjust.rGOLAmt>
			), typeof(Decimal))]
			public virtual Decimal? LineTotalRGOL { get; set; }
			#endregion

			#region CuryLineTotalAdjusting
			public abstract class curyLineTotalAdjusting : PX.Data.BQL.BqlDecimal.Field<curyLineTotalAdjusting> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<
					Switch<
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
					ARAdjust.curyAdjgAmt>
				), typeof(Decimal))]
			public virtual Decimal? CuryLineTotalAdjusting { get; set; }
			#endregion
			/// Turn for last period
			#region Adjusted
			public abstract class adjusted : PX.Data.BQL.BqlDecimal.Field<adjusted> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Switch<Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<adjgTranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1,
					Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<adjgTranPeriodID, Equal<FinPeriod.masterFinPeriodID>>>, decimal1>>,
					decimal0>
				), typeof(Decimal))]
			public virtual Decimal? Adjusted { get; set; }
			#endregion

			#region TurnAdjusted
			public abstract class turnAdjusted : PX.Data.BQL.BqlDecimal.Field<turnAdjusted> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<Switch<Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<adjgTranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1,
					Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<adjgTranPeriodID, Equal<FinPeriod.masterFinPeriodID>>>, decimal1>>,
					decimal0>,
				Sub<
					Mult<
						Switch<
							Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
							Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
							Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
							Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
							Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
							Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
						Add<ARAdjust.adjAmt, Add<ARAdjust.adjDiscAmt, ARAdjust.adjWOAmt>>>,
					ARAdjust.rGOLAmt>>
				), typeof(Decimal))]
			public virtual Decimal? TurnAdjusted { get; set; }
			#endregion

			#region CuryTurnAdjusted
			public abstract class curyTurnAdjusted : PX.Data.BQL.BqlDecimal.Field<curyTurnAdjusted> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<Switch<Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<adjgTranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1,
							Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<adjgTranPeriodID, Equal<FinPeriod.masterFinPeriodID>>>, decimal1>>,
						decimal0>, 
				Mult<
					Switch<
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
					Add<ARAdjust.curyAdjdAmt, Add<ARAdjust.curyAdjdDiscAmt, ARAdjust.curyAdjdWOAmt>>>>
				), typeof(Decimal))]
			public virtual Decimal? CuryTurnAdjusted { get; set; }
			#endregion

			#region Adjusting
			public abstract class adjusting : PX.Data.BQL.BqlDecimal.Field<adjusting> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Switch<Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<adjgTranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1,
						Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<adjgTranPeriodID, Equal<FinPeriod.masterFinPeriodID>>>, decimal1>>,
					decimal0>
			), typeof(Decimal))]
			public virtual Decimal? Adjusting { get; set; }
			#endregion

			#region TurnAdjusting
			public abstract class turnAdjusting : PX.Data.BQL.BqlDecimal.Field<turnAdjusting> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<Switch<Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<adjgTranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1,
							Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<adjgTranPeriodID, Equal<FinPeriod.masterFinPeriodID>>>, decimal1>>,
						decimal0>,
				Mult<
					Switch<
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
					ARAdjust.adjAmt>>
				), typeof(Decimal))]
			public virtual Decimal? TurnAdjusting { get; set; }
			#endregion

			#region CuryTurnAdjusting
			public abstract class curyTurnAdjusting : PX.Data.BQL.BqlDecimal.Field<curyTurnAdjusting> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<Switch<Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<adjgTranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1,
							Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<adjgTranPeriodID, Equal<FinPeriod.masterFinPeriodID>>>, decimal1>>,
						decimal0>,
				Mult<
					Switch<
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
					ARAdjust.curyAdjgAmt>>
				), typeof(Decimal))]
			public virtual Decimal? CuryTurnAdjusting { get; set; }
			#endregion

			#region TurnRGOL
			public abstract class turnRGOL : PX.Data.BQL.BqlDecimal.Field<turnRGOL> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<Switch<Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<adjgTranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1,
							Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<adjgTranPeriodID, Equal<FinPeriod.masterFinPeriodID>>>, decimal1>>,
						decimal0>,
				Mult<
					Switch<
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>,
					ARAdjust.rGOLAmt>>
			), typeof(Decimal))]
			public virtual Decimal? TurnRGOL { get; set; }
			#endregion

			#region TurnARAdjusting
			public abstract class turnARAdjusting : PX.Data.BQL.BqlDecimal.Field<turnARAdjusting> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<Switch<Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<adjgTranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1,
							Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<adjgTranPeriodID, Equal<FinPeriod.masterFinPeriodID>>>, decimal1>>,
						decimal0>,
				Mult<
					Switch<
						Case<Where<ARAdjust.adjBatchNbr, IsNull>, decimal0,
						Case<Where<ARAdjust.adjgBranchID, Equal<ARAdjust.adjdBranchID>, And<ARAdjust.adjBatchNbr, Equal<ARDocumentResult.batchNbr>>>, decimal0,
						Case<Where<ARAdjust.adjdDocType.IsIn<ARDocType.smallBalanceWO, ARDocType.smallCreditWO, ARDocType.refund>>, decimal0,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.refund>>, decimal0,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>>>>>,
					ARAdjust.adjAmt>>
				), typeof(Decimal))]
			public virtual Decimal? TurnARAdjusting { get; set; }
			#endregion

			#region CuryTurnARAdjusting
			public abstract class curyTurnARAdjusting : PX.Data.BQL.BqlDecimal.Field<curyTurnARAdjusting> { }
			[PXDecimal(4)]
			[PXDBCalced(typeof(
				Mult<Switch<Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<adjgTranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1,
							Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<adjgTranPeriodID, Equal<FinPeriod.masterFinPeriodID>>>, decimal1>>,
						decimal0>,
				Mult<
					Switch<
						Case<Where<ARAdjust.adjBatchNbr, IsNull>, decimal0,
						Case<Where<ARAdjust.adjgBranchID, Equal<ARAdjust.adjdBranchID>, And<ARAdjust.adjBatchNbr, Equal<ARDocumentResult.batchNbr>>>, decimal0,
						Case<Where<ARAdjust.adjdDocType.IsIn<ARDocType.smallBalanceWO, ARDocType.smallCreditWO, ARDocType.refund>>, decimal0,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.refund>>, decimal0,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.payment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.prepayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsEqual<ARDocType.voidPayment>.And<ARAdjust.adjdDocType.IsEqual<ARDocType.creditMemo>>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
						Case<Where<ARAdjust.adjgDocType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>>>>>>,
					ARAdjust.curyAdjgAmt>>
				), typeof(Decimal))]
			public virtual Decimal? CuryTurnARAdjusting { get; set; }
			#endregion
				}
		
		[System.SerializableAttribute()]
		[PXHidden()]
		public partial class GLTran : GL.GLTran
		{
			#region BranchID
			public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			#endregion
			#region Module
			public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
			#endregion
			#region BatchNbr
			public new abstract class batchNbr : IBqlField { }
			#endregion
			#region LineNbr
			public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			#endregion
			#region LedgerID
			public new abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
			#endregion
			#region AccountID
			public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			#endregion
			#region SubID
			public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
			#endregion
			#region CreditAmt
			public new abstract class creditAmt : PX.Data.BQL.BqlDecimal.Field<creditAmt> { }
			#endregion
			#region DebitAmt
			public new abstract class debitAmt : PX.Data.BQL.BqlDecimal.Field<debitAmt> { }
			#endregion
			#region Posted
			public new abstract class posted : PX.Data.BQL.BqlBool.Field<posted> { }
			#endregion
			#region TranType
			public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
			#endregion
			#region RefNbr
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			#endregion
			#region ReferenceID
			public new abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }
			#endregion
			#region GLTurnover
			public abstract class gLTurnover : PX.Data.BQL.BqlDecimal.Field<gLTurnover> { }
			[PXBaseCury()]
			[PXDBCalced(typeof(
				Mult<
					Switch<
						Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<True>, And<tranPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1,
						Case<Where<CurrentValue<ARDocumentFilter.useMasterCalendar>, Equal<False>, And<finPeriodID, Equal<CurrentValue<ARDocumentFilter.period>>>>, decimal1>>,
						decimal0>,
					Sub<debitAmt, creditAmt>>
			), typeof(decimal))]
			public virtual decimal? GLTurnover
				{
				get;
				set;
				}
			#endregion
			}

		[System.SerializableAttribute()]
		[PXHidden()]
		public partial class ARRetainage : ARRegister
		{
			#region OrigDocType
			public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
			#endregion
			#region OrigRefNbr
			public new abstract class origRefNbr : IBqlField { }
			#endregion
			#region Released
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			#endregion
			#region IsRetainageDocument
			public new abstract class isRetainageDocument : PX.Data.BQL.BqlBool.Field<isRetainageDocument> { }
			#endregion
			#region FinPeriodID
			public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			#endregion
			#region TranPeriodID
			public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
			#endregion
			#region CuryOrigDocAmt
			public new abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }

			/// <summary>
			/// The amount of the document.
			/// Given in the <see cref="CuryID">currency of the document</see>.
			/// </summary>
			[PXDecimal()]
			[PXUIField(DisplayName = "Currency Origin. Amount", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDBCalced(typeof(
				Mult<
				ARRegister.curyOrigDiscAmt,
				Case<Where<ARRegister.released, Equal<False>>, decimal0,
				Case<Where<ARRegister.docType.IsIn<ARDocType.smallBalanceWO, ARDocType.smallCreditWO>>, decimal_1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>
			), typeof(decimal))]
			public override Decimal? CuryOrigDocAmt
			{
				get;
				set;
			}
			#endregion
			#region OrigDocAmt
			public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }

			/// <summary>
			/// The amount of the document.
			/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
			/// </summary>
			[PXDecimal()]
			[PXUIField(DisplayName = "Origin. Amount", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDBCalced(typeof(
				Mult<
				ARRegister.origDocAmt,
				Case<Where<ARRegister.released, Equal<False>>, decimal0,
				Case<Where<ARRegister.docType.IsIn<ARDocType.smallBalanceWO, ARDocType.smallCreditWO>>, decimal_1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO>>, decimal1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.creditMemo, ARDocType.payment, ARDocType.prepayment, ARDocType.voidPayment, ARDocType.smallBalanceWO>>, decimal_1,
				Case<Where<ARRegister.docType.IsIn<ARDocType.cashSale, ARDocType.cashReturn>>, decimal0>>>>>>
			), typeof(decimal))]
			public override Decimal? OrigDocAmt
				{
				get;
				set;
			}
			#endregion

				}
		public class Ref
		{
			[PXHidden]
			public class ARInvoice : IBqlTable
			{
				#region DocType
				public abstract class docType : PX.Data.BQL.BqlString.Field<docType>
				{
					public const int Length = 3;
				}

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
					get;
					set;
			}
			#endregion
				#region RefNbr

				public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
			{
				}

				/// <summary>
				/// The reference number of the document.
				/// This field is a part of the compound key of the document.
				/// </summary>
				[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
				[PXDefault()]
				[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
				[PXSelector(typeof(Search<ARRegister.refNbr, Where<ARRegister.docType, Equal<Optional<ARRegister.docType>>>>), Filterable = true)]
				public virtual String RefNbr
				{
					get;
					set;
				}
				#endregion
				#region InvoiceNbr
				public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
				protected String _InvoiceNbr;

				/// <summary>
				/// The original reference number or ID assigned by the customer to the customer document.
				/// </summary>
				[PXDBString(40, IsUnicode = true)]
				[PXUIField(DisplayName = "Customer Order", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
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
				#region InstallmentCntr
				public abstract class installmentCntr : PX.Data.BQL.BqlShort.Field<installmentCntr> { }
				protected short? _InstallmentCntr;

				/// <summary>
				/// The counter of <see cref="TermsInstallment">installments</see> associated with the document.
				/// </summary>
				[PXDBShort()]
				public virtual short? InstallmentCntr
			{
				get 
				{
						return this._InstallmentCntr;
				}
				set
				{
						this._InstallmentCntr = value;
				}
			}
			#endregion

			}
			[PXHidden]
			public class ARPayment : IBqlTable
			{
				#region DocType
				public abstract class docType : PX.Data.BQL.BqlString.Field<docType>
				{
					public const int Length = 3;
				}

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
				get;
				set;
			}
			#endregion
				#region RefNbr

				public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
				{
				}

			/// <summary>
				/// The reference number of the document.
				/// This field is a part of the compound key of the document.
			/// </summary>
				[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
				[PXDefault()]
				[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
				[PXSelector(typeof(Search<ARRegister.refNbr, Where<ARRegister.docType, Equal<Optional<ARRegister.docType>>>>), Filterable = true)]
				public virtual String RefNbr
			{
				get;
				set;
			}
			#endregion
				#region PaymentMethodID
				public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
				protected String _PaymentMethodID;
				[PXDBString(10, IsUnicode = true)]
				[PXSelector(typeof(PaymentMethod.paymentMethodID), DescriptionField = typeof(PaymentMethod.descr))]
				[PXUIField(DisplayName = "Payment Method", Enabled = false)]

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
				#region ExtRefNbr
				public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
				protected String _ExtRefNbr;
				[PXDBString(40, IsUnicode = true)]
				[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
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
		}
		}
		private sealed class ARDisplayDocType : ARDocType
		{
			public const string CashReturnInvoice = "RCI";
			public const string CashSaleInvoice = "CSI";
			public new class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Invoice, DebitMemo, CreditMemo, Payment, VoidPayment, Prepayment, Refund, VoidRefund, FinCharge, SmallBalanceWO, SmallCreditWO, CashSale, CashReturn, CashSaleInvoice, CashReturnInvoice },
					new string[] { Messages.Invoice, Messages.DebitMemo, Messages.CreditMemo, Messages.Payment, Messages.VoidPayment, Messages.Prepayment, Messages.Refund, Messages.VoidRefund, Messages.FinCharge, Messages.SmallBalanceWO, Messages.SmallCreditWO, Messages.CashSale, Messages.CashReturn,Messages.CashSaleInvoice,Messages.CashReturnInvoice }) { }
			}
		}
		private sealed class decimalZero : PX.Data.BQL.BqlDecimal.Constant<decimalZero>
		{
			public decimalZero()
				: base(Decimal.Zero)
			{
			}
		}

		#endregion

		#region Ctor
		public ARDocumentEnq()
		{
			ARSetup setup = this.ARSetup.Current;
			Company company = this.Company.Current;

			Documents.Cache.AllowDelete = false;
			Documents.Cache.AllowInsert = false;
			Documents.Cache.AllowUpdate = false;

			PXUIFieldAttribute.SetVisibility<ARRegister.finPeriodID>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<ARRegister.customerID>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<ARRegister.customerLocationID>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<ARRegister.curyDiscBal>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<ARRegister.curyOrigDocAmt>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<ARRegister.curyDiscTaken>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisible<ARRegister.customerID>(Documents.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>());

			this.actionsfolder.MenuAutoOpen = true;
			this.actionsfolder.AddMenuAction(this.createInvoice);
			this.actionsfolder.AddMenuAction(this.createPayment);
			this.actionsfolder.AddMenuAction(this.payDocument);

			this.reportsfolder.MenuAutoOpen = true;
			this.reportsfolder.AddMenuAction(this.aRBalanceByCustomerReport);
			this.reportsfolder.AddMenuAction(this.customerHistoryReport);
			this.reportsfolder.AddMenuAction(this.aRAgedPastDueReport);
			this.reportsfolder.AddMenuAction(this.aRAgedOutstandingReport);
			this.reportsfolder.AddMenuAction(this.aRRegisterReport);

			CustomerRepository = new CustomerRepository(this);
		}
		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Actions
		public PXAction<ARDocumentFilter> refresh;
		public PXCancel<ARDocumentFilter> Cancel;
		[Obsolete("Will be removed in Acumatica 2019R1")]
		public PXAction<ARDocumentFilter> viewDocument;
		public PXAction<ARDocumentFilter> viewOriginalDocument;
		public PXAction<ARDocumentFilter> previousPeriod;
		public PXAction<ARDocumentFilter> nextPeriod;

		public PXAction<ARDocumentFilter> actionsfolder;

		public PXAction<ARDocumentFilter> createInvoice;
		public PXAction<ARDocumentFilter> createPayment;
		public PXAction<ARDocumentFilter> payDocument;

		public PXAction<ARDocumentFilter> reportsfolder;
		public PXAction<ARDocumentFilter> aRBalanceByCustomerReport;
		public PXAction<ARDocumentFilter> customerHistoryReport;
		public PXAction<ARDocumentFilter> aRAgedPastDueReport;
		public PXAction<ARDocumentFilter> aRAgedOutstandingReport;
		public PXAction<ARDocumentFilter> aRRegisterReport;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		#endregion

		protected CustomerRepository CustomerRepository;

		#region Action Delegates
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		public IEnumerable Refresh(PXAdapter adapter)
		{
			this.Filter.Current.FilterDetails = null;
			this.Filter.Current.RefreshTotals = true;
			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton()]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (this.Documents.Current != null)
			{
				PXRedirectHelper.TryRedirect(Documents.Cache, Documents.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return Filter.Select();
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewOriginalDocument(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
				ARRegister origDoc = PXSelect<ARRegister,
					Where<ARRegister.refNbr, Equal<Required<ARRegister.origRefNbr>>,
						And<ARRegister.docType, Equal<Required<ARRegister.origDocType>>>>>
					.SelectSingleBound(graph, null, Documents.Current.OrigRefNbr, Documents.Current.OrigDocType);
				if (origDoc != null)
				{
					PXRedirectHelper.TryRedirect(graph.Document.Cache, origDoc, "Document", PXRedirectHelper.WindowMode.NewWindow);
				}
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable PreviousPeriod(PXAdapter adapter)
		{
			ARDocumentFilter filter = Filter.Current as ARDocumentFilter;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);

			FinPeriod prevPeriod = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.Period, looped: true);

			filter.Period = prevPeriod?.FinPeriodID;
			filter.RefreshTotals = true;
			filter.FilterDetails = null;

			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable NextPeriod(PXAdapter adapter)
		{
			ARDocumentFilter filter = Filter.Current as ARDocumentFilter;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);

			FinPeriod nextPeriod = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.Period, looped: true);

			filter.Period = nextPeriod?.FinPeriodID;
			filter.RefreshTotals = true;
			filter.FilterDetails = null;

			return adapter.Get();
		}

		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Actionsfolder(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Reportsfolder(PXAdapter adapter)
		{
			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.NewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable CreateInvoice(PXAdapter adapter)
		{
			if (this.Filter.Current != null)
			{
				if (this.Filter.Current.CustomerID != null)
				{
					ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
					invoiceEntry.Clear();

					ARInvoice newDoc = invoiceEntry.Document.Insert(new ARInvoice());
					newDoc.CustomerID = this.Filter.Current.CustomerID;

					object oldCustomerValue = null;
					object newCustomerValue = Filter.Current.CustomerID;

					invoiceEntry.Document.Cache.RaiseFieldVerifying<ARInvoice.customerID>(
						newDoc,
						ref newCustomerValue);

					invoiceEntry.Document.Cache.RaiseFieldUpdated<ARInvoice.customerID>(
						newDoc,
						oldCustomerValue);

					throw new PXRedirectRequiredException(invoiceEntry, "CreateInvoice");
				}
			}
			return Filter.Select();
		}

		[PXUIField(DisplayName = Messages.NewPayment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable CreatePayment(PXAdapter adapter)
		{
			if (this.Filter.Current != null)
			{
				if (this.Filter.Current.CustomerID != null)
				{
					ARPaymentEntry graph = PXGraph.CreateInstance<ARPaymentEntry>();
					graph.Clear();
					ARPayment newDoc = graph.Document.Insert(new ARPayment());
					newDoc.CustomerID = this.Filter.Current.CustomerID;
					graph.Document.Cache.RaiseFieldUpdated<ARPayment.customerID>(newDoc, null);

					throw new PXRedirectRequiredException(graph, "CreatePayment");
				}
			}
			return Filter.Select();
		}


		[PXUIField(DisplayName = Messages.EnterPayment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable PayDocument(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				if (this.Documents.Current.Payable != true)
					throw new PXException(Messages.Only_Invoices_MayBe_Payed);

				if (this.Documents.Current.Status != ARDocStatus.Open)
					throw new PXException(AP.Messages.Only_Open_Documents_MayBe_Processed);

				ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
				ARInvoice inv = FindDoc<ARInvoice>(Documents.Current);
				if (inv != null)
				{
					graph.Document.Current = inv;
					graph.PayInvoice(adapter);
				}
			}
			return Filter.Select();
		}

		#endregion

		#region Report Actions Delegates

		[PXUIField(DisplayName = Messages.ARBalanceByCustomerReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ARBalanceByCustomerReport(PXAdapter adapter)
		{
			ARDocumentFilter filter = Filter.Current;

			if (filter != null)
			{
				Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<ARDocumentFilter.customerID>>>>.Select(this);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				if (!string.IsNullOrEmpty(filter.Period))
				{
					parameters["PeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.Period);
				}
				parameters["CustomerID"] = customer.AcctCD;
				parameters["UseMasterCalendar"] = filter.UseMasterCalendar==true?true.ToString():false.ToString();
				throw new PXReportRequiredException(parameters, "AR632500", PXBaseRedirectException.WindowMode.NewWindow , Messages.ARBalanceByCustomerReport);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.CustomerHistoryReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable CustomerHistoryReport(PXAdapter adapter)
		{
			ARDocumentFilter filter = Filter.Current;
			if (filter != null)
			{
				Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<ARDocumentFilter.customerID>>>>.Select(this);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				if (!string.IsNullOrEmpty(filter.Period))
				{
					parameters["FromPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.Period);
					parameters["ToPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.Period);
				}
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR652000",PXBaseRedirectException.WindowMode.NewWindow, Messages.CustomerHistoryReport);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.ARAgedPastDueReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ARAgedPastDueReport(PXAdapter adapter)
		{
			ARDocumentFilter filter = Filter.Current;
			if (filter != null)
			{
				Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<ARDocumentFilter.customerID>>>>.Select(this);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR631000", PXBaseRedirectException.WindowMode.NewWindow, Messages.ARAgedPastDueReport);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.ARAgedOutstandingReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ARAgedOutstandingReport(PXAdapter adapter)
		{
			ARDocumentFilter filter = Filter.Current;
			if (filter != null)
			{
				Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<ARDocumentFilter.customerID>>>>.Select(this);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR631500", PXBaseRedirectException.WindowMode.NewWindow, Messages.ARAgedOutstandingReport);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.ARRegisterReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ARRegisterReport(PXAdapter adapter)
		{
			ARDocumentFilter filter = Filter.Current;
			if (filter != null)
			{
				Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<ARDocumentFilter.customerID>>>>.Select(this);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				if (!string.IsNullOrEmpty(filter.Period))
				{
					parameters["StartPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.Period);
					parameters["EndPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.Period);
				}
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR621500", PXBaseRedirectException.WindowMode.NewWindow, Messages.ARRegisterReport);
			}
			return adapter.Get();
		}
		#endregion
	
		#region Selects
		public PXFilter<ARDocumentFilter> Filter;
		[PXFilterable]
		public PXSelectOrderBy<ARDocumentResult, OrderBy<Desc<ARDocumentResult.docDate>>> Documents;
		public PXSetup<ARSetup> ARSetup;
		public PXSetup<Company> Company;

		#endregion

		#region Select Delegates

		protected virtual IEnumerable documents()
		{
			ARDocumentFilter header = Filter.Current;
			var result = new List<ARDocumentResult>();
			if (header == null)
			{
				return result;
			}
			if (header.FilterDetails != null)
			{
				PXFieldState state = Filter.Cache.GetStateExt<ARDocumentFilter.filterDetails>(header) as PXFieldState;
				if (state != null && state.Value != null && state.Value is IEnumerable)
					return state.Value as IEnumerable;
			}

			int?[] relevantCustomerIDs = null;

			if (Filter.Current?.CustomerID != null && Filter.Current?.IncludeChildAccounts == true)
			{
				relevantCustomerIDs = CustomerFamilyHelper
					.GetCustomerFamily<Override.BAccount.consolidatingBAccountID>(this, Filter.Current.CustomerID)
					.Where(customerInfo => customerInfo.BusinessAccount.BAccountID != null)
					.Select(customerInfo => customerInfo.BusinessAccount.BAccountID)
					.ToArray();
			}
			else if (Filter.Current?.CustomerID != null)
			{
				relevantCustomerIDs = new[] { Filter.Current.CustomerID };
			}

			PXSelectBase<ARDocumentResult> baseSel = new PXSelectReadonly<
				ARDocumentResult,
				Where<ARDocumentResult.customerID, In<Required<ARDocumentResult.customerID>>>,
				OrderBy<Asc<ARDocumentResult.docType, Asc<ARDocumentResult.refNbr>>>>
				(this);
			BqlCommand sel = baseSel.View.BqlSelect;
			
			if (header.OrgBAccountID != null)
			{
				sel = sel.WhereAnd<Where<ARDocumentResult.branchID, Inside<Current<ARDocumentFilter.orgBAccountID>>>>(); //MatchWithOrg
			}
			
			if (header.ARAcctID != null)
			{
				sel = sel.WhereAnd<Where<ARDocumentResult.aRAccountID, Equal<Current<ARDocumentFilter.aRAcctID>>>>();
			}

			if (header.ARSubID != null)
			{
				sel = sel.WhereAnd<Where<ARDocumentResult.aRSubID, Equal<Current<ARDocumentFilter.aRSubID>>>>();
			}

			if ((header.IncludeUnreleased ?? false) == false)
			{
				sel = sel.WhereAnd<Where<ARDocumentResult.released, Equal<True>>>();
			}
			else
			{
				sel = sel.WhereAnd<Where<ARDocumentResult.scheduled, Equal<False>, 
					And<Where<ARDocumentResult.voided, Equal<False>, 
							Or<ARDocumentResult.released, Equal<True>>>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(header.SubCD))
			{
				sel = BqlCommand.AppendJoin<InnerJoin<Sub,On<Sub.subID, Equal<ARDocumentResult.aRSubID>>>>(sel);
				sel = sel.WhereAnd<Where<Sub.subCD, Like<Current<ARDocumentFilter.subCDWildcard>>>>();
			}

			if (header.DocType != null)
			{
				sel = sel.WhereAnd<Where<ARDocumentResult.docType, Equal<Current<ARDocumentFilter.docType>>>>();
			}

			if (header.CuryID != null)
			{
				sel = sel.WhereAnd<Where<ARDocumentResult.curyID, Equal<Current<ARDocumentFilter.curyID>>>>();
			}
			bool byPeriod = (header.Period != null);
			BqlCommand mainSel = sel;

			FetchDetails(mainSel, relevantCustomerIDs, false,
				out List<object> documents,
				out List<object> adjusting,
				out List<object> adjusted,
				out List<object> retainage,
				out List<object> glTurn);

			bool anyDoc = false;


			foreach (PXResult<ARDocumentResult> reg in documents)
			{
				ARDocumentResult res = (ARDocumentResult)Documents.Cache.CreateCopy((ARDocumentResult)reg);
				res.IsComplementary = false;
				Ref.ARInvoice invoice = PXResult.Unwrap<Ref.ARInvoice>(reg);
				Ref.ARPayment payment = PXResult.Unwrap<Ref.ARPayment>(reg);

				// Invalid/unknown data - skip record - user notification.
				//
				if (!res.Paying.HasValue) continue;
				
				if (header.IncludeGLTurnover != true &&( res.DocType == ARDocType.CashSale || res.DocType == ARDocType.CashReturn))
				{
					// Artificial split of CashSale record on invoice and payment parts.
					//
					ARDocumentResult invPart = (ARDocumentResult)Documents.Cache.CreateCopy(res);
					invPart.DisplayDocType = res.DocType == ARDocType.CashSale
						? ARDisplayDocType.CashSaleInvoice
						: ARDisplayDocType.CashReturnInvoice;
					invPart.IsComplementary = true;
					invPart.SignBalance = -invPart.SignBalance;
					ARDocumentResult copy1 = HandleDocument(invPart, payment, invoice, header, adjusted,
						adjusting, retainage, glTurn, byPeriod);
					if (copy1 != null)
					{
						result.Add(copy1);
					}
				}

				ARDocumentResult copy = HandleDocument(res, payment, invoice, header, adjusted,
					adjusting, retainage, glTurn, byPeriod);
				if (copy != null)
				{
					result.Add(copy);
				}

				anyDoc = true;
			}

			if (byPeriod == true && header.IncludeGLTurnover == true && header.BranchID != null)
			{
				FetchDetails(mainSel, relevantCustomerIDs, true,
					out documents,
					out adjusted,
					out adjusting,
					out retainage,
					out glTurn);
				foreach (PXResult<ARDocumentResult> reg in documents)
				{
					ARDocumentResult res = (ARDocumentResult) Documents.Cache.CreateCopy((ARDocumentResult) reg);
					res.IsComplementary = false;
					Ref.ARInvoice invoice = PXResult.Unwrap<Ref.ARInvoice>(reg);
					Ref.ARPayment payment = PXResult.Unwrap<Ref.ARPayment>(reg);

					ARDocumentResult copy = HandleDocument(res, payment, invoice, header, adjusted,
						adjusting, retainage, glTurn, byPeriod);
				if (copy != null)
				{
					result.Add(copy);
				}

				anyDoc = true;
			}
			}

			viewDocument.SetEnabled(anyDoc);
			Filter.Cache.SetValueExt<ARDocumentFilter.filterDetails>(header, result);
			return result;
		}

		protected virtual void FetchDetails(BqlCommand baseSelect, 
			int?[] relevantCustomerIDs,
			bool byApplication,
			out List<object> documents,
			out List<object> adjusting,
			out List<object> adjusted,
			out List<object> retainage,
			out List<object> glTurn)
		{
			adjusting = null;
			adjusted = null;
			retainage = null;
			glTurn = null;
			var header = this.Filter.Current;
			bool byPeriod = (header.Period != null);
			BqlCommand selectDocument = baseSelect;
			int[] branchIDs = null;
			if(byApplication)
			{
				selectDocument = selectDocument.WhereAnd<
					Where<Exists<Select<AR.ARAdjust,
						Where<AR.ARAdjust.adjgDocType, Equal<ARDocumentResult.docType>,
							And<AR.ARAdjust.adjgRefNbr, Equal<ARDocumentResult.refNbr>,
							And<AR.ARAdjust.adjdBranchID, Equal<Current<ARDocumentFilter.branchID>>,
							And<AR.ARAdjust.adjgTranPeriodID, Equal<Current<ARDocumentFilter.period>>,
							And<AR.ARAdjust.adjdBranchID, NotEqual<AR.ARAdjust.adjgBranchID>>>>>>
					>>>>();
			}
			else 
			{ 
				if (header.BranchID != null)
				{
					selectDocument = selectDocument.WhereAnd<Where<ARDocumentResult.branchID, Equal<Current<ARDocumentFilter.branchID>>>>();
				}
				else if (header.OrganizationID != null)
				{
					branchIDs = PXAccess.GetChildBranchIDs(header.OrganizationID, false);

					selectDocument = selectDocument.WhereAnd<Where<ARDocumentResult.branchID, In<Required<ARDocumentResult.branchID>>,
						And<MatchWithBranch<ARDocumentResult.branchID>>>>();
				}
			}
			
			if (!byPeriod)
			{
				if (header.ShowAllDocs == false)
		{
					selectDocument = selectDocument.WhereAnd<Where<ARDocumentResult.openDoc, Equal<True>>>();
				}
		}
			else
			{
				if (header.UseMasterCalendar == true)
				{
					selectDocument = selectDocument.WhereAnd<Where<ARDocumentResult.tranPeriodID, LessEqual<Current<ARDocumentFilter.period>>>>();
					selectDocument = selectDocument.WhereAnd<Where<ARDocumentResult.closedTranPeriodID, GreaterEqual<Current<ARDocumentFilter.period>>,
											Or<ARDocumentResult.closedTranPeriodID, IsNull>>>();

				}
				else
				{
					selectDocument = selectDocument.WhereAnd<Where<ARDocumentResult.finPeriodID, LessEqual<Current<ARDocumentFilter.period>>>>();
					selectDocument = selectDocument.WhereAnd<Where<ARDocumentResult.closedFinPeriodID, GreaterEqual<Current<ARDocumentFilter.period>>,
											Or<ARDocumentResult.closedFinPeriodID, IsNull>>>();
				}
			}
			BqlCommand selectAdjusted =
				BqlCommand.AppendJoin<
					InnerJoin<ARAdjust,
						On<ARAdjust.adjgDocType, Equal<ARDocumentResult.docType>,
							And<ARAdjust.adjgRefNbr, Equal<ARDocumentResult.refNbr>>>,
					InnerJoin<Branch,
						On<ARAdjust.adjgBranchID, Equal<Branch.branchID>>,
					InnerJoin<FinPeriod,
						On<Branch.organizationID, Equal<FinPeriod.organizationID>,
						And<FinPeriod.finPeriodID, Equal<Current<ARDocumentFilter.period>>>>>>>>(selectDocument);

			BqlCommand selectAdjusting =
				BqlCommand.AppendJoin<
					InnerJoin<ARAdjust,
						On<ARAdjust.adjdDocType, Equal<ARDocumentResult.docType>,
							And<ARAdjust.adjdRefNbr, Equal<ARDocumentResult.refNbr>>>,
					InnerJoin<Branch,
						On<ARAdjust.adjdBranchID, Equal<Branch.branchID>>,
					InnerJoin<FinPeriod,
						On<Branch.organizationID, Equal<FinPeriod.organizationID>,
						And<FinPeriod.finPeriodID, Equal<Current<ARDocumentFilter.period>>>>>>>>(selectDocument);


			BqlCommand selectRetainageInvoices =
				BqlCommand.AppendJoin<
					InnerJoin<ARRetainage,
						On<ARRetainage.origDocType, Equal<ARDocumentResult.docType>,
							And<ARRetainage.origRefNbr, Equal<ARDocumentResult.refNbr>,
							And<ARRetainage.released, Equal<True>,
							And<ARRetainage.isRetainageDocument, Equal<True>>>>>>>(selectDocument);
			BqlCommand  selectGLTurn =
				BqlCommand.AppendJoin<
					InnerJoin<GLTran,
						On<GLTran.module.IsIn<BatchModule.moduleGL, BatchModule.moduleAR>.
							And<GLTran.tranType.IsEqual<ARDocumentResult.docType>>.
							And<GLTran.tranType.IsEqual<ARDocumentResult.docType>>.
							And<GLTran.refNbr.IsEqual<ARDocumentResult.refNbr>>.
							And<GLTran.branchID.IsEqual<ARDocumentFilter.branchID.FromCurrent>>.
							And<GLTran.accountID.IsEqual<ARDocumentResult.aRAccountID>>.
							And<GLTran.subID.IsEqual<ARDocumentResult.aRSubID>>.
							And<GLTran.referenceID.IsEqual<ARDocumentResult.customerID>>.
							And<GLTran.posted.IsEqual<True>>>>> (selectDocument);

			selectDocument = BqlCommand.AppendJoin<
					LeftJoin<Ref.ARInvoice,
						On<Ref.ARInvoice.docType, Equal<ARDocumentResult.docType>,
						And<Ref.ARInvoice.refNbr, Equal<ARDocumentResult.refNbr>>>,
					LeftJoin<Ref.ARPayment,
						On<Ref.ARPayment.docType, Equal<ARDocumentResult.docType>,
						And<Ref.ARPayment.refNbr, Equal<ARDocumentResult.refNbr>>>>>>(selectDocument);
			var pars = new List<object>() { };

			if (byPeriod)
			{
				if (header.UseMasterCalendar == true)
		{

					selectAdjusted = selectAdjusted.WhereAnd<Where<ARAdjust.adjgTranPeriodID, LessEqual<Current<ARDocumentFilter.period>>>>();
					selectAdjusting = selectAdjusting.WhereAnd<Where<ARAdjust.adjgTranPeriodID, LessEqual<Current<ARDocumentFilter.period>>>>();

					selectRetainageInvoices = selectRetainageInvoices.WhereAnd<Where<ARRetainage.tranPeriodID, LessEqual<Current<ARDocumentFilter.period>>>>();
				}
				else
			{

					selectAdjusted = selectAdjusted.WhereAnd<Where<ARAdjust.adjgTranPeriodID, LessEqual<FinPeriod.masterFinPeriodID>>>();

					selectAdjusting = selectAdjusting.WhereAnd<Where<ARAdjust.adjgTranPeriodID, LessEqual<FinPeriod.masterFinPeriodID>>>();

					selectRetainageInvoices = selectRetainageInvoices.WhereAnd<Where<ARRetainage.finPeriodID, LessEqual<Current<ARDocumentFilter.period>>>>();
				}
			}

			selectAdjusting = selectAdjusting.AggregateNew<
				Aggregate<
					GroupBy<ARDocumentResult.docType,
					GroupBy<ARDocumentResult.refNbr,
						Sum<ARAdjust.lineTotalAdjusted,
						Sum<ARAdjust.curyLineTotalAdjusted,
						Sum<ARAdjust.turnAdjusted,
						Sum<ARAdjust.curyTurnAdjusted,
						Sum<ARAdjust.lineTotalAdjusting,
						Sum<ARAdjust.curyLineTotalAdjusting,
						Sum<ARAdjust.turnAdjusting,
						Sum<ARAdjust.curyTurnAdjusting,
						Sum<ARAdjust.turnRGOL,
						Sum<ARAdjust.turnARAdjusting,
						Sum<ARAdjust.adjDiscAmt,
						Sum<ARAdjust.curyAdjdDiscAmt,
						Sum<ARAdjust.rGOLAmt,
						Sum<ARAdjust.lineTotalRGOL,
						Sum<ARAdjust.adjusting,
						Sum<ARAdjust.adjusted
						>>>>>>>>>>>>>>>>>>>>();
			selectAdjusted = selectAdjusted.AggregateNew<
				Aggregate<
					GroupBy<ARDocumentResult.docType,
					GroupBy<ARDocumentResult.refNbr,
						Sum<ARAdjust.lineTotalAdjusted,
						Sum<ARAdjust.curyLineTotalAdjusted,
						Sum<ARAdjust.turnAdjusted,
						Sum<ARAdjust.curyTurnAdjusted,
						Sum<ARAdjust.lineTotalAdjusting,
						Sum<ARAdjust.curyLineTotalAdjusting,
						Sum<ARAdjust.turnAdjusting,
						Sum<ARAdjust.curyTurnAdjusting,
						Sum<ARAdjust.turnRGOL,
						Sum<ARAdjust.turnARAdjusting,
						Sum<ARAdjust.curyTurnARAdjusting,
						Sum<ARAdjust.adjDiscAmt,
						Sum<ARAdjust.curyAdjdDiscAmt,
						Sum<ARAdjust.rGOLAmt,
						Sum<ARAdjust.lineTotalRGOL
					>>>>>>>>>>>>>>>>>>>();
			selectGLTurn = selectGLTurn.AggregateNew<
				Aggregate<
					GroupBy<ARDocumentResult.docType,
					GroupBy<ARDocumentResult.refNbr,
						Sum<GLTran.gLTurnover
						>>>>>();
			PXView documentView = new PXView(this, true, selectDocument);
			if (byPeriod)
				{
				if (!byApplication)
				{
					var applicationFields = new List<Type>()
					{
						typeof(ARDocumentResult.docType),
						typeof(ARDocumentResult.refNbr),
						typeof(ARAdjust.lineTotalAdjusted),
						typeof(ARAdjust.curyLineTotalAdjusted),
						typeof(ARAdjust.lineTotalAdjusting),
						typeof(ARAdjust.curyLineTotalAdjusting),
						typeof(ARAdjust.turnAdjusted),
						typeof(ARAdjust.curyTurnAdjusted),
						typeof(ARAdjust.turnAdjusting),
						typeof(ARAdjust.curyTurnAdjusting),
						typeof(ARAdjust.turnARAdjusting),
						typeof(ARAdjust.curyTurnARAdjusting),
						typeof(ARAdjust.adjDiscAmt),
						typeof(ARAdjust.curyAdjdDiscAmt),
						typeof(ARAdjust.rGOLAmt),
						typeof(ARAdjust.turnRGOL),
						typeof(ARAdjust.lineTotalRGOL),
						typeof(ARAdjust.adjusting),
						typeof(ARAdjust.adjusted)
					};
					PXView adjustingView = new PXView(this, true, selectAdjusting);
					PXView adjustedView = new PXView(this, true, selectAdjusted);
					using (new PXFieldScope(adjustingView, applicationFields))
						adjusting = adjustingView.SelectMulti(relevantCustomerIDs, branchIDs);
					using (new PXFieldScope(adjustedView, applicationFields))
						adjusted = adjustedView.SelectMulti(relevantCustomerIDs, branchIDs);
					if (PXAccess.FeatureInstalled<FeaturesSet.retainage>())
					{
						PXView retainageView = new PXView(this, true, selectRetainageInvoices);
						using (new PXFieldScope(retainageView, new List<Type>()
						{
							typeof(ARDocumentResult.docType),
							typeof(ARDocumentResult.refNbr),
							typeof(ARRetainage.origDocAmt),
							typeof(ARRetainage.curyOrigDocAmt)
						}))
							retainage = retainageView.SelectMulti(relevantCustomerIDs, branchIDs);
				}
			}

				if (this.Filter.Current.IncludeGLTurnover == true)
				{
					var glTurnFields = new List<Type>()
			{
						typeof(ARDocumentResult.docType),
						typeof(ARDocumentResult.refNbr),
						typeof(GLTran.gLTurnover)
					};
					PXView glTurnView = new PXView(this, true, selectGLTurn);
					using (new PXFieldScope(glTurnView, glTurnFields))
						glTurn = glTurnView.SelectMulti(relevantCustomerIDs, branchIDs);
			}
		}
			var restrictedFields = new List<Type>()
			{
				typeof(ARDocumentResult),
				typeof(Ref.ARInvoice),
				typeof(Ref.ARPayment),
			};

			using (var scope = new PXFieldScope(documentView, restrictedFields, false))
			{
				documents = documentView.SelectMulti(relevantCustomerIDs, branchIDs);
			}
		}
		protected virtual ARDocumentResult HandleDocument(
			ARDocumentResult aDoc, 
			Ref.ARPayment payment, 
			Ref.ARInvoice invoice, 
			ARDocumentFilter header, 
			List<object> selectAdjusted,
			List<object> selectAdjusting,
			List<object> selectRetainageInvoices,
			List<object> selectGLTrun,
			bool byPeriod) 
		{
			ARDocumentResult res = aDoc;

			if (res.Paying == true)
			{
				res.ExtRefNbr = (res.DocType == ARDocType.CreditMemo && !string.IsNullOrEmpty(invoice.RefNbr)) 
					? invoice.InvoiceNbr 
					: payment.ExtRefNbr;
				res.PaymentMethodID = payment.PaymentMethodID;
				res.DueDate = null;
			}
			else
			{
				res.ExtRefNbr = invoice.InvoiceNbr;
				res.PaymentMethodID = null;
			}
			
			bool isCashSale = (res.DocType == ARDocType.CashSale || res.DocType == ARDocType.CashReturn);

			if (byPeriod)
			{
			    string documentPeriod = header.UseMasterCalendar == true ? res.TranPeriodID : res.FinPeriodID;

				// Skip Cash sales created outside the period - they can't balance change  in it.
				// 
				if (!(documentPeriod == header.Period) && isCashSale) return null;  

				res.OrigDocAmt = res.SignBalance * res.OrigDocAmt;
				res.CuryOrigDocAmt = res.SignBalance * res.CuryOrigDocAmt;
				int includeBalance = isCashSale ? 0 : 1;
				res.DocBal = includeBalance * res.OrigDocAmt;
				res.CuryDocBal = includeBalance * res.CuryOrigDocAmt;
				res.IsTurn = documentPeriod == header.Period;
				res.BegBalance = res.IsTurn == true ? 0m : includeBalance*res.OrigDocAmt;
				res.CuryBegBalance = res.IsTurn == true ? 0m : includeBalance*res.CuryOrigDocAmt;
				res.DiscActTaken = 0m;
				res.CuryDiscActTaken = 0m;
				res.RGOLAmt = 0m;
				res.RetainageUnreleasedAmt = res.SignBalance * res.RetainageTotal;
				res.CuryRetainageUnreleasedAmt = res.SignBalance * res.CuryRetainageTotal;
				res.GLTurnover = 0m;
				PXResult <ARDocumentResult> adjusting = PopDocument(res, selectAdjusting);
				PXResult<ARDocumentResult> adjusted = PopDocument(res, selectAdjusted);
				PXResult<ARDocumentResult> retainage = PopDocument(res, selectRetainageInvoices);
				PXResult<ARDocumentResult> glTurn = PopDocument(res, selectGLTrun);

				// Filter out master record for multiple installments.
				// 
				if (invoice != null && invoice.InstallmentCntr != null) return null;

                // Scan payments, which were applyed to invoice (for invoices).
			    //
				if (adjusting != null)
				    {
					ARAdjust adjustment = PXResult.Unwrap<ARAdjust>(adjusting);
				    /*
					    // Reversals should not be counted in Small-Credit Balance. it is always zero.
					    // 
					    if (adjustment.IsSelfAdjustment() && res.IsComplementary != true ||
					        adjustment.Released != true && header.IncludeUnreleased != true ||
					        adjustment.AdjdDocType == ARDocType.SmallCreditWO &&
					        adjustment.AdjgDocType == ARDocType.VoidPayment) continue;
					*/

				    res.DocBal += adjustment.LineTotalAdjusted;
				    res.CuryDocBal += adjustment.CuryLineTotalAdjusted;
				    res.DiscActTaken += res.SignBalance * adjustment.AdjDiscAmt;
					    res.RGOLAmt += adjustment.RGOLAmt;
				    res.CuryDiscActTaken += res.SignBalance * adjustment.CuryAdjdDiscAmt;
				    res.BegBalance += adjustment.LineTotalAdjusted - adjustment.TurnAdjusted;
				    res.CuryBegBalance += adjustment.CuryLineTotalAdjusted - adjustment.CuryTurnAdjusted;
					if(adjustment.Adjusting > 0)
					    {
						res.IsTurn = true;
				    }
			    }

			    // Scan invoices, to which were  payment was applied (for checks).
				// 
				
				if (adjusted != null && res.IsComplementary != true)
					{
					ARAdjust adjustment = PXResult.Unwrap<ARAdjust>(adjusted);

					//if (adjustment.Released != true && header.IncludeUnreleased != true) continue;

					res.DocBal -= adjustment.LineTotalAdjusting;
					res.CuryDocBal -= adjustment.CuryLineTotalAdjusting;

					res.BegBalance -= adjustment.LineTotalAdjusting - adjustment.TurnAdjusting;
					res.CuryBegBalance -= adjustment.CuryLineTotalAdjusting - adjustment.CuryTurnAdjusting;
					if (adjustment.Adjusted > 0)
						{
						res.IsTurn = true;
						}
						#region ARTurnover calculation
					res.ARTurnover -= adjustment.TurnARAdjusting - adjustment.TurnRGOL;
						#endregion
					}

				if (retainage != null)
				{
					ARRetainage retainageInovice = PXResult.Unwrap<ARRetainage>(retainage);
					res.RetainageUnreleasedAmt -= retainageInovice.OrigDocAmt;
					res.CuryRetainageUnreleasedAmt -= retainageInovice.CuryOrigDocAmt;
				}

				if (glTurn != null)
					{
					GLTran tran = PXResult.Unwrap<GLTran>(glTurn);
					res.GLTurnover = (res.DocType == ARDocType.SmallBalanceWO ? -tran.GLTurnover : tran.GLTurnover ) ?? 0m;
					}
				if ((res.Voided == true || res.DocType == ARDocType.VoidPayment) &&
					string.CompareOrdinal(header.Period, header.UseMasterCalendar == true ? res.ClosedTranPeriodID : res.ClosedFinPeriodID) >= 0)
				{
					res.DocBal = 0m;
					res.CuryDocBal = 0m;
				}

				if (res.OrigDocAmt == 0m
					|| (res.BegBalance == 0m
						&& res.DocBal == 0m
						&& res.RetainageUnreleasedAmt == 0m
						&& res.RGOLAmt == 0m
						&& res.IsTurn != true))
					return null;
			}
			else
			{
				res.OrigDocAmt = res.SignBalance * res.OrigDocAmt;
				res.CuryOrigDocAmt = res.SignBalance * res.CuryOrigDocAmt;
				res.DocBal = res.SignBalance * res.DocBal;
				res.CuryDocBal = res.SignBalance * res.CuryDocBal;
				bool isDiscNotTaken = isCashSale && res.IsComplementary != true;
				res.CuryDiscActTaken = isDiscNotTaken ? 0m : res.SignBalance * res.CuryDiscTaken ?? 0m;
				res.DiscActTaken = isDiscNotTaken ? 0m : res.SignBalance * res.DiscTaken ?? 0m;
				res.ARTurnover = 0m;
			}

			res.RGOLAmt = -res.RGOLAmt;
			//SetValuesSign(res);
			
			return res;
		}
		protected PXResult<ARDocumentResult> PopDocument(ARDocumentResult doc,  List<object> list)
		{
			if (list == null || list.Count == 0 || !(list[0] is PXResult<ARDocumentResult> rec)) return null;
			ARDocumentResult source = rec;
			if (source.DocType == doc.DocType && source.RefNbr == doc.RefNbr)
			{
				list.RemoveAt(0);
				return rec;
			}
			else
			{
				return null;
			}
		}
	    protected virtual string GetFilterMasterPeriodForAdjust(ARDocumentFilter header, FinPeriod documentRelatedFilterFinPeriod)
	    {
	        if (header.UseMasterCalendar == true)
	        {
	            return header.Period;
	        }
	        else
	        {
	            return documentRelatedFilterFinPeriod.MasterFinPeriodID;
	        }
        }

        protected virtual IEnumerable filter()
		{
			PXCache cache = this.Caches[typeof(ARDocumentFilter)];
			if (cache != null)
			{
				ARDocumentFilter filter = cache.Current as ARDocumentFilter;

				if (filter != null)
				{
					if (filter.RefreshTotals == true)
					{
						filter.ClearSummary();
						foreach (ARDocumentResult it in Documents.Select())
						{
							Aggregate(filter, it);
						}

						filter.RefreshTotals = false;
					}

					if (filter.CustomerID != null)
					{
						ARCustomerBalanceEnq balanceBO = PXGraph.CreateInstance<ARCustomerBalanceEnq>();
						ARCustomerBalanceEnq.ARHistoryFilter histFilter = balanceBO.Filter.Current;
                        ARCustomerBalanceEnq.Copy(histFilter, filter);
                        if (histFilter.Period == null)
							histFilter.Period = balanceBO.GetLastActivityPeriod(filter.CustomerID, filter.IncludeChildAccounts == true);

						balanceBO.Filter.Update(histFilter);

						ARCustomerBalanceEnq.ARHistorySummary summary = balanceBO.Summary.Select();
						SetSummary(filter, summary);
					}
				}

				yield return cache.Current;
				cache.IsDirty = false;
			}
		}
		#endregion

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Original Document")]
		protected virtual void ARDocumentResult_OrigRefNbr_CacheAttached(PXCache sender) { }
		
		#endregion

		#region Events Handlers
		public virtual void ARDocumentFilter_ARAcctID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			ARDocumentFilter header = e.Row as ARDocumentFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.ARAcctID = null;
			}
		}

		public virtual void ARDocumentFilter_ARSubID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			ARDocumentFilter header = e.Row as ARDocumentFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.ARSubID = null;
			}
		}

		public virtual void ARDocumentFilter_CuryID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			ARDocumentFilter header = e.Row as ARDocumentFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.CuryID = null;
			}
		}

		public virtual void ARDocumentFilter_SubCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		public virtual void ARDocumentFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (cache.ObjectsEqual<ARDocumentFilter.organizationID>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.branchID>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.customerID>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.period>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.masterFinPeriodID>(e.Row, e.OldRow) &&
				cache.ObjectsEqual<ARDocumentFilter.useMasterCalendar>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.showAllDocs>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.includeUnreleased>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.aRAcctID>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.aRSubID>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.subCD>(e.Row, e.OldRow) &&
				cache.ObjectsEqual<ARDocumentFilter.subCDWildcard>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.docType>(e.Row, e.OldRow) &&
			    cache.ObjectsEqual<ARDocumentFilter.includeChildAccounts>(e.Row, e.OldRow) &&
				cache.ObjectsEqual<ARDocumentFilter.curyID>(e.Row, e.OldRow))
			{
				return;
			}
			
			(e.Row as ARDocumentFilter).RefreshTotals = true;
			(e.Row as ARDocumentFilter).FilterDetails = null;
		}
		public virtual void ARDocumentFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARDocumentFilter row = (ARDocumentFilter)e.Row;
			if (row == null) return;
			PXCache docCache = this.Documents.Cache;

			bool byPeriod = (row.Period != null);

			bool isMCFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			bool isForeignCurrencySelected = String.IsNullOrEmpty(row.CuryID) == false && (row.CuryID != this.Company.Current.BaseCuryID);
			bool isBaseCurrencySelected = String.IsNullOrEmpty(row.CuryID) == false && (row.CuryID == this.Company.Current.BaseCuryID);

			PXUIFieldAttribute.SetVisible<ARDocumentFilter.showAllDocs>(cache, row, !byPeriod);
			PXUIFieldAttribute.SetVisible<ARDocumentFilter.includeChildAccounts>(cache, row, PXAccess.FeatureInstalled<CS.FeaturesSet.parentChildAccount>());

			PXUIFieldAttribute.SetVisible<ARDocumentFilter.curyID>(cache, row, isMCFeatureInstalled);
			PXUIFieldAttribute.SetVisible<ARDocumentFilter.curyBalanceSummary>(cache, row, isMCFeatureInstalled && isForeignCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentFilter.curyDifference>(cache, row, isMCFeatureInstalled && isForeignCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentFilter.curyCustomerBalance>(cache, row, isMCFeatureInstalled && isForeignCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentFilter.curyCustomerRetainedBalance>(cache, row, isMCFeatureInstalled && isForeignCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentFilter.curyCustomerDepositsBalance>(cache, row, isMCFeatureInstalled && isForeignCurrencySelected);

			PXUIFieldAttribute.SetVisible<ARDocumentResult.curyID>(docCache, null, isMCFeatureInstalled);
			PXUIFieldAttribute.SetVisible<ARDocumentResult.rGOLAmt>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentResult.curyBegBalance>(docCache, null, byPeriod && isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentResult.begBalance>(docCache, null, byPeriod);
			PXUIFieldAttribute.SetVisible<ARDocumentResult.curyOrigDocAmt>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentResult.curyDocBal>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentResult.curyDiscActTaken>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentResult.curyRetainageTotal>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentResult.curyOrigDocAmtWithRetainageTotal>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<ARDocumentResult.curyRetainageUnreleasedAmt>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);

			Customer customer = null;

			if (row.CustomerID != null)
			{
				customer = CustomerRepository.FindByID(row.CustomerID);
			}

			createInvoice.SetEnabled(customer != null &&
				(customer.Status == BAccount.status.Active
				|| customer.Status == BAccount.status.OneTime));

			bool isPaymentAllowed = customer != null && customer.Status != BAccount.status.Inactive;

			createPayment.SetEnabled(isPaymentAllowed);
			payDocument.SetEnabled(isPaymentAllowed);

			aRBalanceByCustomerReport.SetEnabled(row.CustomerID != null);
			customerHistoryReport.SetEnabled(row.CustomerID != null);
			aRAgedPastDueReport.SetEnabled(row.CustomerID != null);
			aRAgedOutstandingReport.SetEnabled(row.CustomerID != null);
			aRRegisterReport.SetEnabled(row.CustomerID != null);
		
		}
		#endregion

		#region Utility Functions - internal
		protected virtual void SetSummary(ARDocumentFilter aDest, ARCustomerBalanceEnq.ARHistorySummary aSrc)
		{
			aDest.CustomerBalance = aSrc.BalanceSummary;
			aDest.CustomerDepositsBalance = aSrc.DepositsSummary;
			aDest.CuryCustomerBalance = aSrc.CuryBalanceSummary;
			aDest.CuryCustomerDepositsBalance = aSrc.CuryDepositsSummary;
		}

		protected virtual void Aggregate(ARDocumentFilter aDest, ARDocumentResult aSrc)
		{
			if (string.IsNullOrEmpty(aDest.Period) && aSrc.Released == false)
			{
				aDest.BalanceSummary += aSrc.OrigDocAmt ?? Decimal.Zero;
				aDest.CuryBalanceSummary += aSrc.CuryOrigDocAmt ?? Decimal.Zero;
			}
			else
			{
				aDest.BalanceSummary += aSrc.DocBal ?? Decimal.Zero;
				aDest.CuryBalanceSummary += aSrc.CuryDocBal ?? Decimal.Zero;
				aDest.CustomerRetainedBalance += aSrc.RetainageUnreleasedAmt ?? Decimal.Zero;
				aDest.CuryCustomerRetainedBalance += aSrc.CuryRetainageUnreleasedAmt ?? Decimal.Zero;
			}
		}

		protected TDoc FindDoc<TDoc>(ARDocumentResult aRes)
			where TDoc : ARRegister, new()
		{
			return FindDoc<TDoc>(this, aRes.DocType, aRes.RefNbr);
		}

		protected virtual void SetValuesSign(ARDocumentResult aRes)
		{
			if (aRes.SignBalance.HasValue)
			{
				decimal sign = aRes.SignBalance.Value;
				aRes.OrigDocAmt *= sign  ;
				aRes.DocBal *= sign;
				aRes.BegBalance *= sign;
				aRes.DiscActTaken *= sign;
				aRes.DiscTaken *= sign;
				aRes.OrigDiscAmt *= sign;
				aRes.DiscBal *= sign;
				aRes.RetainageUnreleasedAmt *= sign;
				aRes.CuryOrigDocAmt *= sign;
				aRes.CuryDocBal *= sign;
				aRes.CuryBegBalance *= sign;
				aRes.CuryDiscActTaken *= sign;
				aRes.CuryDiscTaken *= sign;
				aRes.CuryOrigDiscAmt *= sign;
				aRes.CuryDiscBal *= sign;
				aRes.CuryRetainageUnreleasedAmt *= sign;
			}
		}

		#endregion

		#region Utility Functions - public
		public static TDoc FindDoc<TDoc>(PXGraph aGraph, string aDocType, string apRefNbr)
			where TDoc : ARRegister, new()
		{
			return PXSelect<TDoc,
				Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
					And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>
				.Select(aGraph, aDocType, apRefNbr);
		}

		[Obsolete("Obsoilete. Will be removed in Acumatica ERP 2019R1")]
		public static bool? IsInvoiceType(string aDocType)
		{
			if (aDocType == ARDocType.Invoice || aDocType == ARDocType.DebitMemo || aDocType == ARDocType.FinCharge) return true;
			if (aDocType == ARDocType.Payment || aDocType == ARDocType.CreditMemo || aDocType == ARDocType.Refund || aDocType == ARDocType.VoidPayment) return false;
			return null;
		}
		#endregion
	}
}

