using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.CS;
using PX.Objects.CM;

using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Tools;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Common;
using System.IO.Compression;
using PX.Objects.Common.Attributes;
using PX.Objects.CR;
using PX.Data.BQL;
using PX.Objects.AR;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class APDocumentEnq : PXGraph<APDocumentEnq>
	{
		#region Internal Types
		[Serializable]
		public partial class APDocumentFilter : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
		    [Organization(false, Required = false)]
			public int? OrganizationID { get; set; }
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

			[BranchOfOrganization(typeof(APDocumentFilter.organizationID), false)]
			public int? BranchID { get; set; }
			#endregion

		    #region OrgBAccountID
		    public abstract class orgBAccountID : IBqlField { }

		    [OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
		    public int? OrgBAccountID { get; set; }
		    #endregion
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			protected Int32? _VendorID;
			[Vendor(DescriptionField = typeof(Vendor.acctName))]
			[PXDefault()]
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
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			protected Int32? _AccountID;
			[GL.Account(null, typeof(Search5<Account.accountID,
					InnerJoin<APHistory, On<Account.accountID, Equal<APHistory.accountID>>>,
					Where<Match<Current<AccessInfo.userName>>>,
					Aggregate<GroupBy<Account.accountID>>>),
			   DisplayName = "AP Account", DescriptionField = typeof(GL.Account.description))]
			public virtual Int32? AccountID
			{
				get
				{
					return this._AccountID;
				}
				set
				{
					this._AccountID = value;
				}
			}
			#endregion
			#region SubCD
			public abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }
			protected String _SubCD;
			[PXDBString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "AP Subaccount", Visibility = PXUIVisibility.Invisible, FieldClass = SubAccountAttribute.DimensionName)]
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

			[PXBool]
			[PXUIField(DisplayName = Common.Messages.UseMasterCalendar)]
			[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.multipleCalendarsSupport>))]
			public bool? UseMasterCalendar { get; set; }
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			protected String _FinPeriodID;
			[PXDefault()]
			[AnyPeriodFilterable(null, null,
				branchSourceType: typeof(APDocumentFilter.branchID),
				organizationSourceType: typeof(APDocumentFilter.organizationID),
				useMasterCalendarSourceType: typeof(APDocumentFilter.useMasterCalendar),
				redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
			[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible, Required = false)]
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
			[APDocType.List()]
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
			[PXCury(typeof(APDocumentFilter.curyID))]
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
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Balance by Documents",Enabled=false)]
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
			#region CuryVendorBalance
			public abstract class curyVendorBalance : PX.Data.BQL.BqlDecimal.Field<curyVendorBalance> { }
			protected Decimal? _CuryVendorBalance;
			[PXCury(typeof(APDocumentFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Current Balance (Currency)", Enabled = false)]
			public virtual Decimal? CuryVendorBalance
			{
				get
				{
					return this._CuryVendorBalance;
				}
				set
				{
					this._CuryVendorBalance = value;
				}
			}
			#endregion
			#region VendorBalance
			public abstract class vendorBalance : PX.Data.BQL.BqlDecimal.Field<vendorBalance> { }
			protected Decimal? _VendorBalance;
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Current Balance", Enabled = false)]
			public virtual Decimal? VendorBalance
			{
				get
				{
					return this._VendorBalance;
				}
				set
				{
					this._VendorBalance = value;
				}
			}
			#endregion

			#region CuryVendorRetainedBalance
			public abstract class curyVendorRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyVendorRetainedBalance> { }
			[PXCury(typeof(APDocumentFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Retained Balance (Currency)", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? CuryVendorRetainedBalance
			{
				get;
				set;
			}
			#endregion
			#region VendorRetainedBalance
			public abstract class vendorRetainedBalance : PX.Data.BQL.BqlDecimal.Field<vendorRetainedBalance> { }

			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Retained Balance", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? VendorRetainedBalance
			{
				get;
				set;
			}
			#endregion

			#region CuryVendorDepositsBalance

			public abstract class curyVendorDepositsBalance : PX.Data.BQL.BqlDecimal.Field<curyVendorDepositsBalance> { }
			protected Decimal? _CuryVendorDepositsBalance;
			[PXCury(typeof(APDocumentFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Prepayments Balance (Currency)", Enabled = false)]
			public virtual Decimal? CuryVendorDepositsBalance
			{
				get
				{
					return this._CuryVendorDepositsBalance;
				}
				set
				{
					this._CuryVendorDepositsBalance = value;
				}
			}
			#endregion
			#region VendorDepositsBalance

			public abstract class vendorDepositsBalance : PX.Data.BQL.BqlDecimal.Field<vendorDepositsBalance> { }
			protected Decimal? _VendorDepositsBalance;
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Prepayment Balance", Enabled = false)]
			public virtual Decimal? VendorDepositsBalance
			{
				get
				{
					return this._VendorDepositsBalance;
				}
				set
				{
					this._VendorDepositsBalance = value;
				}
			}
			#endregion

			#region CuryDifference
			public abstract class curyDifference : PX.Data.BQL.BqlDecimal.Field<curyDifference> { }

			[PXCury(typeof(APDocumentFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Balance Discrepancy (Currency)", Enabled = false)]
			public virtual Decimal? CuryDifference
			{
				get
				{
					return (this._CuryBalanceSummary - (this._CuryVendorBalance + this._CuryVendorDepositsBalance));
				}
			}
			#endregion
			#region Difference
			public abstract class difference : PX.Data.BQL.BqlDecimal.Field<difference> { }

			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Balance Discrepancy", Enabled = false)]
			public virtual Decimal? Difference
			{
				get
				{
					return (this._BalanceSummary - (this._VendorBalance + this._VendorDepositsBalance));
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
				VendorBalance = Decimal.Zero;
				VendorDepositsBalance= Decimal.Zero;
				BalanceSummary = Decimal.Zero;
				CuryVendorBalance = Decimal.Zero;
				CuryVendorDepositsBalance = Decimal.Zero;
				CuryBalanceSummary = Decimal.Zero;
				CuryVendorRetainedBalance = Decimal.Zero;
				VendorRetainedBalance = Decimal.Zero;
			}
		}

		[Serializable()]
		[PXPrimaryGraph(typeof(APDocumentEnq), Filter = typeof(APDocumentFilter))]
		[PXCacheName(Messages.APHistoryForReport)]
		public partial class APHistoryForReport : APHistory { }

		[Serializable()]
		public partial class APDocumentResult : APRegister
		{
			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			[PXDBString(3, IsKey = true, IsFixed = true)]
			[PXDefault()]
			[APDocType.List()]
			[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
			public override String DocType
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
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXDefault()]
			[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
			[PXSelector(typeof(Search<APRegister.refNbr>), Filterable = true)]
			public override String RefNbr
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
			#region CuryOrigDocAmt
			[PXDefault(TypeCode.Decimal, "0.0")]
			//[PXDBCury(typeof(APRegister.curyInfoID))]
			[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.origDocAmt))]
			[PXUIField(DisplayName = "Currency Origin. Amount")]
			public override Decimal? CuryOrigDocAmt
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
			[PXDBBaseCury()]
			[PXUIField(DisplayName = "Origin. Amount")]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? OrigDocAmt
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
			#region RGOLAmt
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName="RGOL Amount")]
			public override Decimal? RGOLAmt
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
			#region ExtRefNbr
			public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
			protected String _ExtRefNbr;
			[PXString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "Vendor Invoice Nbr./Payment Nbr.")]
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
			#region PaymentMethodID
			public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
			protected String _PaymentMethodID;
			[PXString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Payment Method")]
		//	[PXSelector(typeof(PX.Objects.CA.PaymentMethod.paymentMethodID))]
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
			
			#region CuryBegBalance
			public abstract class curyBegBalance : PX.Data.BQL.BqlDecimal.Field<curyBegBalance> { }
			protected Decimal? _CuryBegBalance;
			[PXCurrency(typeof(APRegister.curyInfoID),typeof(APDocumentResult.begBalance))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Currency Period Beg. Balance")]
			public virtual Decimal? CuryBegBalance
			{
				get
				{
					return this._CuryBegBalance;
				}
				set
				{
					this._CuryBegBalance = value;
				}
			}
			#endregion
			#region BegBalance
			public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
			protected Decimal? _BegBalance;
			[PXBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Period Beg. Balance")]
			public virtual Decimal? BegBalance
			{
				get
				{
					return this._BegBalance;
				}
				set
				{
					this._BegBalance = value;
				}
			}
			#endregion
			#region CuryDiscActTaken			
			public abstract class curyDiscActTaken : PX.Data.BQL.BqlDecimal.Field<curyDiscActTaken> { }
			protected Decimal? _CuryDiscActTaken;
			[PXDefault(TypeCode.Decimal, "0.0")]
			//[PXCury(typeof(APRegister.curyID))]
			[PXCurrency(typeof(APRegister.curyInfoID), typeof(APDocumentResult.discActTaken))]
			[PXUIField(DisplayName = "Currency Cash Discount Taken")]
			public virtual Decimal? CuryDiscActTaken
			{
				get
				{
					return this._CuryDiscActTaken;
				}
				set
				{
					this._CuryDiscActTaken = value;
				}
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
				get
				{
					return this._DiscActTaken;
				}
				set
				{
					this._DiscActTaken = value;
				}
			}
			#endregion		
			#region CuryActualBalance
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.docBal), BaseCalc = false)]
			[PXUIField(DisplayName = "Currency Balance")]
			public override Decimal? CuryDocBal
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
			#region ActualBalance
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Balance")]
			public override Decimal? DocBal
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
			#region APTurnover
			public abstract class aPTurnover : PX.Data.BQL.BqlDecimal.Field<aPTurnover> { }

			/// <summary>
			/// Expected GL turnover for the document.
			/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
			/// </summary>
			[PXBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "AP Turnover")]
			public virtual decimal? APTurnover
			{
				get;
				set;
			}
			#endregion
			#region CuryTaxWheld			
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.taxWheld))]
			[PXUIField(DisplayName = "Currency Tax Withheld")]
			public override Decimal? CuryTaxWheld
			{
				get
				{
					return this._CuryTaxWheld;
				}
				set
				{
					this._CuryTaxWheld = value;
				}
			}
			
			#endregion
			#region TaxWheld
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]			
			[PXUIField(DisplayName = "Tax Withheld")]
			public override Decimal? TaxWheld
			{
				get
				{
					return this._TaxWheld;
				}
				set
				{
					this._TaxWheld = value;
				}
			}

			#endregion
			#region Status
			public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			[PXDBString(1, IsFixed = true)]
			[PXDefault(APDocStatus.Hold)]
			[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[APDocStatus.List]
			public override string Status { get; set; }

			#endregion
			#region Adjusted
			public abstract class adjusted : PX.Data.BQL.BqlBool.Field<adjusted> { }
			protected Boolean? _Adjusted;

			/// <summary>
			/// When set to <c>true</c> indicates that the document was Adjusted.
			/// </summary>
			[PXBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Adjusted", Visible = false)]
			[PXDBCalced(typeof(
					Switch<Case<Where<
						Exists<Select<
						APAdjust, 
						Where<APAdjust.adjgDocType, Equal<APDocumentResult.docType>, 
							And<APAdjust.adjgRefNbr, Equal<APDocumentResult.refNbr>>>>>>, 
						True>, False>), 
				typeof(bool))]
			public virtual Boolean? Adjusted
			{
				get;
				set;
			}
			#endregion
			#region Adjusting
			public abstract class adjusting : PX.Data.BQL.BqlBool.Field<adjusting> { }

			/// <summary>
			/// When set to <c>true</c> indicates that the document was Adjusting.
			/// </summary>
			[PXBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Adjusting", Visible = false)]
			[PXDBCalced(typeof(
				Switch<Case<Where<
						Exists<Select<
							APAdjust,
							Where<APAdjust.adjdDocType, Equal<APDocumentResult.docType>,
								And<APAdjust.adjdRefNbr, Equal<APDocumentResult.refNbr>>>>>>,
					True>, False>),
				typeof(bool))]			
			
			public virtual Boolean? Adjusting
			{
				get;
				set;
			}
			#endregion
			#region Retainage
			public abstract class retainage : PX.Data.BQL.BqlBool.Field<retainage> { }

			/// <summary>
			/// When set to <c>true</c> indicates that the document was Retenage.
			/// </summary>
			[PXBool()]
			public Boolean? Retainage
			{
				get;
				set;
			}
			#endregion
		}
		#endregion

		#region Views/Selects

		public PXSetup<APSetup> APSetup;
		public PXSetup<Company> Company;
		public PXFilter<APDocumentFilter> Filter;
		
		[PXFilterable]
		public PXSelectJoinOrderBy<
			APDocumentResult, 
			LeftJoin<APInvoice, 
				On<APDocumentResult.docType, Equal<APInvoice.docType>,
				And<APDocumentResult.refNbr, Equal<APInvoice.refNbr>>>>, 
			OrderBy<Desc<APDocumentResult.docDate>>> Documents;
		
		protected virtual IEnumerable documents()
		{
			
			APDocumentFilter header = Filter.Current;
			var documentResult = new PXResultset<APDocumentResult, APInvoice>();
			if (header == null || header.VendorID == null)
			{
				return documentResult;
			}
			if (header.FilterDetails != null)
			{
				PXFieldState state = Filter.Cache.GetStateExt<APDocumentFilter.filterDetails>(header) as PXFieldState;
				if (state != null && state.Value != null && state.Value is IEnumerable)
					return state.Value as IEnumerable;
			}
			FinPeriodIDAttribute.SetMasterPeriodID<Batch.finPeriodID>(Filter.Cache, header);
			PXSelectBase<APDocumentResult> sel = new PXSelectReadonly2<APDocumentResult,
				InnerJoin<Sub, On<Sub.subID, Equal<APDocumentResult.aPSubID>>,
				LeftJoin<APInvoice, On<APInvoice.docType, Equal<APDocumentResult.docType>,
				And<APInvoice.refNbr, Equal<APDocumentResult.refNbr>>>,
				LeftJoin<APPayment, On<APPayment.docType, Equal<APDocumentResult.docType>,
				And<APPayment.refNbr, Equal<APDocumentResult.refNbr>>>>>>,
				Where<APRegister.vendorID, Equal<Current<APDocumentFilter.vendorID>>>,
				OrderBy<Desc<APDocumentResult.docDate>>>(this);

			if (header.OrgBAccountID != null) 
			{
				sel.WhereAnd<Where<APRegister.branchID, Inside<Current<APDocumentFilter.orgBAccountID>>>>(); //MatchWithOrg
			}

			if (header.AccountID != null)
			{
				sel.WhereAnd<Where<APRegister.aPAccountID, Equal<Current<APDocumentFilter.accountID>>>>();
			}

			if ((header.IncludeUnreleased ?? false) == false)
			{
				sel.WhereAnd<Where<Where<APRegister.released, Equal<True>, Or<APRegister.prebooked, Equal<True>>>>>();
			}
			else
			{
				sel.WhereAnd<Where<APRegister.scheduled, Equal<False>, And<Where<APRegister.voided, Equal<False>, Or<APRegister.released, Equal<True>>>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(header.SubCD))
			{
				sel.WhereAnd<Where<Sub.subCD, Like<Current<APDocumentFilter.subCDWildcard>>>>();
			}

			if (header.DocType != null)
			{
				sel.WhereAnd<Where<APRegister.docType, Equal<Current<APDocumentFilter.docType>>>>();
			}

			if (header.CuryID != null)
			{
				sel.WhereAnd<Where<APRegister.curyID, Equal<Current<APDocumentFilter.curyID>>>>();
			}

			PXSelectBase<APAdjust> selectAdjusted = new PXSelectJoin<APAdjust,
			        InnerJoin<Branch,
			            On<APAdjust.adjgBranchID, Equal<Branch.branchID>>,
			        InnerJoin<FinPeriod,
			            On<Branch.organizationID, Equal<FinPeriod.organizationID>,
			                And<FinPeriod.finPeriodID, Equal<Current<APDocumentFilter.finPeriodID>>>>>>,
				Where2<Where<APAdjust.adjgDocType, Equal<Required<APDocumentResult.docType>>>,
					And<APAdjust.adjgRefNbr, Equal<Required<APDocumentResult.refNbr>>>>>(this);

			PXSelectBase<APAdjust> selectAdjusting = new PXSelectJoin<APAdjust, 
				    LeftJoin<APDocumentResult, 
				        On<APAdjust.adjgDocType, Equal<APDocumentResult.docType>, 
					        And<APAdjust.adjgRefNbr, Equal<APDocumentResult.refNbr>>>,
			        InnerJoin<Branch,
			            On<APAdjust.adjdBranchID, Equal<Branch.branchID>>,
			        InnerJoin<FinPeriod,
			            On<Branch.organizationID, Equal<FinPeriod.organizationID>,
			                And<FinPeriod.finPeriodID, Equal<Current<APDocumentFilter.finPeriodID>>>>>>>,
												Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
												And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>>>>(this);
													
			PXSelectBase<APInvoice> selectRetainageBills = new PXSelect<APInvoice,
				Where<APInvoice.origDocType, Equal<Required<APInvoice.docType>>,
					And<APInvoice.origRefNbr, Equal<Required<APInvoice.refNbr>>,
					And<APInvoice.released, Equal<True>,
					And<APInvoice.isRetainageDocument, Equal<True>>>>>>(this);
													
			bool byPeriod = (header.FinPeriodID != null);
			if (!byPeriod)
			{
				if (header.ShowAllDocs == false)
				{
					sel.WhereAnd<Where<APRegister.openDoc, Equal<True>>>();
				}
			}
			else
			{
			    if (header.UseMasterCalendar == true)
			    {
			        sel.WhereAnd<Where<APRegister.tranPeriodID, LessEqual<Current<APDocumentFilter.finPeriodID>>>>();
			        sel.WhereAnd<Where<APRegister.closedTranPeriodID, GreaterEqual<Current<APDocumentFilter.finPeriodID>>,
										Or<APRegister.closedTranPeriodID, IsNull>>>();

			        selectAdjusted.WhereAnd<Where<APAdjust.adjgTranPeriodID, LessEqual<Current<APDocumentFilter.finPeriodID>>>>();
			        selectAdjusting.WhereAnd<Where<APAdjust.adjgTranPeriodID, LessEqual<Current<APDocumentFilter.finPeriodID>>>>();

			        selectRetainageBills.WhereAnd<Where<APInvoice.tranPeriodID, LessEqual<Current<APDocumentFilter.finPeriodID>>>>();
			    }
			    else
			    {
			        sel.WhereAnd<Where<APRegister.finPeriodID, LessEqual<Current<APDocumentFilter.finPeriodID>>>>();
			        sel.WhereAnd<Where<APRegister.closedFinPeriodID, GreaterEqual<Current<APDocumentFilter.finPeriodID>>,
			            Or<APRegister.closedFinPeriodID, IsNull>>>();

			        selectAdjusted.WhereAnd<Where<APAdjust.adjgTranPeriodID, LessEqual<FinPeriod.masterFinPeriodID>>>();

			        selectAdjusting.WhereAnd<Where<APAdjust.adjgTranPeriodID, LessEqual<FinPeriod.masterFinPeriodID>>>();

			        selectRetainageBills.WhereAnd<Where<APInvoice.finPeriodID, LessEqual<Current<APDocumentFilter.finPeriodID>>>>();
			    }
			}

			bool anyDoc = false;
			handledDocuments = new Dictionary<AR.ARDocKey, APDocumentResult>();

			foreach (PXResult<APDocumentResult, Sub, APInvoice, APPayment> reg in sel.Select())
			{
				APDocumentResult res = (APDocumentResult)Documents.Cache.CreateCopy((APDocumentResult)reg);
				APInvoice invoice = reg;
				APPayment payment = reg;

				bool? isPaying = res.Paying;
				if (!isPaying.HasValue)
				{
					continue;
				}

				if (!(invoice.RefNbr == null && res.Voided == true && payment.DocType == APDocType.Prepayment))
					if (res.Voided == true && res.Prebooked == true && res.DocType != APDocType.QuickCheck) continue;

				if (res.DocType == APDocType.Prepayment && (!string.IsNullOrEmpty(invoice.RefNbr) && string.IsNullOrEmpty(payment.RefNbr)))
				{
					//Prepayment requiest- should not be cosidered if there is no "payment" part of it;
					//
					continue;
				}

				if (isPaying == true)
				{
					res.ExtRefNbr = ((res.DocType == APDocType.Prepayment || res.DocType == APDocType.DebitAdj)
							&& !string.IsNullOrEmpty(invoice.RefNbr)) ? invoice.InvoiceNbr : payment.ExtRefNbr;
					res.PaymentMethodID = payment.PaymentMethodID;
				}
				else
				{
					res.ExtRefNbr = invoice.InvoiceNbr;
					res.PaymentMethodID = null;
				}

				bool isPrepayment = (res.DocType == APDocType.Prepayment)
					&& (!string.IsNullOrEmpty(invoice.RefNbr))
					&& (!string.IsNullOrEmpty(payment.RefNbr));

				if (isPrepayment)
				{
					res.OrigDocAmt = 0m;
					res.CuryOrigDocAmt = 0m;
				}

				if (byPeriod)
				{
				    string documentPeriod = header.UseMasterCalendar == true ? res.TranPeriodID : res.FinPeriodID;

                    bool createdInPeriod = (documentPeriod == header.FinPeriodID);

					#region APTurnover calculation

					decimal? aPTurnover = documentPeriod == header.FinPeriodID &&
						res.Released == true
							? res.OrigDocAmt * res.SignBalance
							: 0m;

					AdjustAPTurnover(res, aPTurnover);

					#endregion

					res.DocBal = res.OrigDocAmt;
					res.CuryDocBal = res.CuryOrigDocAmt;
					res.BegBalance = (documentPeriod == header.FinPeriodID) ? 0m : res.OrigDocAmt;
					res.CuryBegBalance = (documentPeriod == header.FinPeriodID) ? 0m : res.CuryOrigDocAmt;
					res.DiscActTaken = 0m;
					res.CuryDiscActTaken = 0m;
					res.RGOLAmt = 0m;
					res.WhTaxBal = res.OrigWhTaxAmt;
					res.CuryWhTaxBal = res.CuryOrigWhTaxAmt;
					res.RetainageUnreleasedAmt = res.RetainageTotal;
					res.CuryRetainageUnreleasedAmt = res.CuryRetainageTotal;

					if (invoice != null && invoice.InstallmentCntr != null)
					{
						// Filter out master record for multiple installments.
						//
						continue;
					}

					//Scan payments, which were applyed to invoice (for invoices).
					//
					if (res.Adjusting == true)
					{
						foreach (PXResult<APAdjust, APDocumentResult, Branch, FinPeriod> it in selectAdjusting.Select(
							res.DocType, res.RefNbr))
					{
						APAdjust adjustment = it;
						APDocumentResult adjustingDocument = it;
					    FinPeriod documentRelatedFilterFinPeriod = it;

						if (adjustment.Released != true && header.IncludeUnreleased != true) continue;

						decimal? applicationAmount = 0m;
						decimal? curyApplicationAmount = 0m;

						if (isPrepayment)
						{
							if (adjustingDocument.Paying == true)
							{
								applicationAmount = adjustingDocument.DocType != APDocType.Refund
										? adjustment.AdjAmt + adjustment.AdjDiscAmt + adjustment.AdjWhTaxAmt +
										  adjustment.SignedRGOLAmt
									: -adjustment.AdjAmt - adjustment.SignedRGOLAmt;
								curyApplicationAmount = adjustingDocument.DocType != APDocType.Refund
										? adjustment.CuryAdjdAmt + adjustment.CuryAdjdDiscAmt +
										  adjustment.CuryAdjdWhTaxAmt
									: -adjustment.CuryAdjdAmt;
								res.RGOLAmt += adjustment.RGOLAmt;
							}
							else
							{
								applicationAmount = -adjustment.AdjAmt;
								curyApplicationAmount = -adjustment.CuryAdjdAmt;
							}

							res.DocBal += applicationAmount;
							res.CuryDocBal += curyApplicationAmount;

								if (adjustment.AdjgTranPeriodID !=
								    GetFilterMasterPeriodForAdjust(header, documentRelatedFilterFinPeriod))
							{
								res.BegBalance += adjustingDocument.Paying == true 
									? adjustment.AdjAmt + adjustment.AdjDiscAmt + adjustment.SignedRGOLAmt 
									: -adjustment.AdjAmt;
									res.CuryBegBalance -= adjustingDocument.Paying == true
										? adjustment.CuryAdjdAmt + adjustment.CuryAdjdDiscAmt
									: -adjustment.CuryAdjdAmt;
							}
						}
						else
						{
								decimal AdjDiscAmt = res.DocType == APDocType.QuickCheck
									? 0m
									: (adjustment.AdjDiscAmt ?? 0m);
								decimal CuryAdjdDiscAmt = res.DocType == APDocType.QuickCheck
									? 0m
									: (adjustment.CuryAdjdDiscAmt ?? 0m);

								applicationAmount = adjustment.AdjAmt + AdjDiscAmt + adjustment.AdjWhTaxAmt +
								                    adjustment.SignedRGOLAmt;
								curyApplicationAmount =
									adjustment.CuryAdjdAmt + CuryAdjdDiscAmt + adjustment.CuryAdjdWhTaxAmt;

							res.DocBal -= applicationAmount;
							res.CuryDocBal -= curyApplicationAmount;

							res.WhTaxBal -= adjustment.AdjWhTaxAmt;
							res.CuryWhTaxBal -= adjustment.CuryAdjdWhTaxAmt;

							res.TaxWheld += adjustment.AdjWhTaxAmt;
							res.CuryTaxWheld += adjustment.CuryAdjdWhTaxAmt;

							res.DiscActTaken += AdjDiscAmt;
							res.CuryDiscActTaken += CuryAdjdDiscAmt;

							res.RGOLAmt += adjustment.RGOLAmt;

								if (adjustment.AdjgTranPeriodID !=
								    GetFilterMasterPeriodForAdjust(header, documentRelatedFilterFinPeriod))
							{
								res.BegBalance -= adjustment.AdjAmt + AdjDiscAmt + adjustment.AdjWhTaxAmt;
									res.CuryBegBalance -=
										adjustment.CuryAdjdAmt + CuryAdjdDiscAmt + adjustment.CuryAdjdWhTaxAmt;
							}
						}

						#region APTurnover calculation

							aPTurnover = adjustment.AdjgTranPeriodID ==
							             GetFilterMasterPeriodForAdjust(header, documentRelatedFilterFinPeriod) &&
							adjustment.Released == true
								? applicationAmount * adjustment.AdjdTBSign
								: 0m;

						AdjustAPTurnover(adjustingDocument, aPTurnover);

						#endregion
					}
					}

					// Scan invoices, to which were  payment was applied (for checks).
					// Quick checks must be excluded - applications for them are already considered in previous part.
					//
					if (res.Adjusted == true)
					{
						foreach (PXResult<APAdjust, Branch, FinPeriod> row in selectAdjusted.Select(res.DocType,
							res.RefNbr))
					{
					    APAdjust adjustment = row;
                        FinPeriod documentRelatedFilterFinPeriod = row;

                        if (adjustment.IsSelfAdjustment()
					    || adjustment.Released != true && header.IncludeUnreleased != true) continue;

						decimal? applicationAmount = adjustment.AdjAmt;
						decimal? curyApplicationAmount = adjustment.CuryAdjgAmt;

						res.DocBal -= applicationAmount * adjustment.AdjgBalSign;
						res.CuryDocBal -= curyApplicationAmount * adjustment.AdjgBalSign;

							if (adjustment.AdjgTranPeriodID !=
							    GetFilterMasterPeriodForAdjust(header, documentRelatedFilterFinPeriod))
						{
							res.BegBalance -= adjustment.AdjAmt * adjustment.AdjgBalSign + adjustment.SignedRGOLAmt;
							res.CuryBegBalance -= adjustment.CuryAdjgAmt * adjustment.AdjgBalSign;
						}

						#region APTurnover calculation

							aPTurnover = adjustment.AdjgTranPeriodID ==
							             GetFilterMasterPeriodForAdjust(header, documentRelatedFilterFinPeriod) &&
							adjustment.Released == true
								? applicationAmount * adjustment.AdjgTBSign
								: 0m;

						AdjustAPTurnover(res, aPTurnover);

						#endregion
					}
					}
					
					if (res.Retainage == true)
					{
						foreach (APInvoice retainageBill in selectRetainageBills.Select(res.DocType, res.RefNbr))
						{
								res.RetainageUnreleasedAmt -= retainageBill.OrigDocAmt * retainageBill.SignAmount;
								res.CuryRetainageUnreleasedAmt -= retainageBill.CuryOrigDocAmt * retainageBill.SignAmount;
						}
					}
					
					if ((res.Voided == true || res.DocType == APDocType.VoidCheck) &&
					string.CompareOrdinal(header.FinPeriodID, header.UseMasterCalendar == true ? res.ClosedTranPeriodID : res.ClosedFinPeriodID) >= 0)
					{
						res.DocBal = 0m;
						res.CuryDocBal = 0m;
					}

					//Filter out Documents which were not change during the period
					if (res.BegBalance == 0m 
						&& res.DocBal == 0m
						&& res.RetainageUnreleasedAmt == 0m
						&& !createdInPeriod)
						continue; 
				}
				else
				{
					res.CuryDiscActTaken = res.CuryDiscTaken ?? 0m;
					res.DiscActTaken = res.DiscTaken ?? 0m;
				}

				anyDoc = true;
				res.RGOLAmt = -res.RGOLAmt;

				if (res.SignAmount.Value.Equals(-1m))
				{
					InvertValuesSign(res);
				}

				documentResult.Add(new PXResult<APDocumentResult, APInvoice>(res,
					new APInvoice()
					{
						DocType = invoice.DocType,
						RefNbr = invoice.RefNbr,
						InvoiceNbr = invoice.InvoiceNbr,
						SuppliedByVendorID = invoice.SuppliedByVendorID
					}));
			}
			viewDocument.SetEnabled(anyDoc);
			Filter.Cache.SetValueExt<APDocumentFilter.filterDetails>(header, documentResult);
			return documentResult;
		}
		
		private Dictionary<AR.ARDocKey, APDocumentResult> handledDocuments;

		/// <summary>
		/// The property to get all selected documents including their applications
		/// with calculated expected GL turnover.
		/// </summary>
		public virtual Dictionary<AR.ARDocKey, APDocumentResult> HandledDocuments
		{
			get { return handledDocuments ?? new Dictionary<AR.ARDocKey, APDocumentResult>(); }
		}

		/// <summary>
		/// The method to calculate expected GL turnover values for all selected
		/// documents and their applications.
		/// Note, that <see cref="handledDocuments"></see> dictionary includes
		/// all documents showed on the UI, moreover all adjusting documents
		/// whom applications may produce GL transactions. We need all such documents
		/// to reconcile correctly turnover values between GL and AP modules because all
		/// GL transactions producing by applications are always marked as adjusting 
		/// document transactions.
		/// </summary>
		private void AdjustAPTurnover(
			APDocumentResult res,
			decimal? aRTurnover)
		{
			APDocumentResult handledDocument;
			AR.ARDocKey key = new AR.ARDocKey(res.DocType, res.RefNbr);

			if (handledDocuments.TryGetValue(key, out handledDocument))
			{
				handledDocument.APTurnover += aRTurnover;

				if (!Equals(handledDocument, res))
				{
					res.APTurnover = handledDocument.APTurnover;
					handledDocuments[key] = res;
				}
			}
			else
			{
				res.APTurnover = aRTurnover;
				handledDocuments.Add(key, res);
			}
		}

	    protected virtual string GetFilterMasterPeriodForAdjust(APDocumentFilter header, FinPeriod documentRelatedFilterFinPeriod)
	    {
	        if (header.UseMasterCalendar == true)
	        {
	            return header.FinPeriodID;
	        }
	        else
	        {
	            return documentRelatedFilterFinPeriod.MasterFinPeriodID;
	        }
	    }

		protected virtual IEnumerable filter()
		{
			PXCache cache = this.Caches[typeof(APDocumentFilter)];
			if (cache != null)
			{
				APDocumentFilter filter = cache.Current as APDocumentFilter;
				if (filter != null)
				{
					if (filter.RefreshTotals == true)
					{					
						filter.ClearSummary();
						foreach (APDocumentResult it in this.Documents.Select())
						{
							Aggregate(filter, it);
						}
						filter.RefreshTotals = false;
					}
					if (filter.VendorID != null)
					{
						APVendorBalanceEnq balanceBO = PXGraph.CreateInstance<APVendorBalanceEnq>();
						APVendorBalanceEnq.APHistoryFilter histFilter = balanceBO.Filter.Current;
						APVendorBalanceEnq.Copy(histFilter, filter);
						if (histFilter.FinPeriodID == null)
							histFilter.FinPeriodID = balanceBO.GetLastActivityPeriod(filter.VendorID);
						balanceBO.Filter.Update(histFilter);

						APVendorBalanceEnq.APHistorySummary summary = balanceBO.Summary.Select();
						SetSummary(filter, summary);
					}
				}
			}
			yield return cache.Current;
			cache.IsDirty = false;
		}
		#endregion
		
		#region Ctor +  Overrides
		public APDocumentEnq()
		{
			APSetup setup = this.APSetup.Current;
			Company company = this.Company.Current;
			Documents.Cache.AllowDelete = false;
			Documents.Cache.AllowInsert = false;
			Documents.Cache.AllowUpdate = false;

			PXUIFieldAttribute.SetVisibility<APRegister.finPeriodID>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<APRegister.vendorID>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<APRegister.vendorLocationID>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<APRegister.curyDiscBal>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<APRegister.curyOrigDocAmt>(Documents.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<APRegister.curyDiscTaken>(Documents.Cache, null, PXUIVisibility.Visible);

			this.actionsFolder.MenuAutoOpen = true;
			this.actionsFolder.AddMenuAction(this.createInvoice);
			this.actionsFolder.AddMenuAction(this.createPayment);
			this.actionsFolder.AddMenuAction(this.payDocument);

			this.reportsFolder.MenuAutoOpen = true;
			this.reportsFolder.AddMenuAction(this.aPBalanceByVendorReport);
			this.reportsFolder.AddMenuAction(this.vendorHistoryReport);
			this.reportsFolder.AddMenuAction(this.aPAgedPastDueReport);
			this.reportsFolder.AddMenuAction(this.aPAgedOutstandingReport);
			this.reportsFolder.AddMenuAction(this.aPRegisterReport);
		}
		
		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Original Document")]
		protected virtual void APDocumentResult_OrigRefNbr_CacheAttached(PXCache sender) { }
		#endregion

		#region Actions
		public PXAction<APDocumentFilter> refresh;
		public PXCancel<APDocumentFilter> Cancel;
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<APDocumentFilter> viewDocument;
		public PXAction<APDocumentFilter> previousPeriod;
		public PXAction<APDocumentFilter> nextPeriod;

		public PXAction<APDocumentFilter> actionsFolder;
		public PXAction<APDocumentFilter> createInvoice;
		public PXAction<APDocumentFilter> createPayment;
		public PXAction<APDocumentFilter> payDocument;

		public PXAction<APDocumentFilter> reportsFolder;
		public PXAction<APDocumentFilter> aPBalanceByVendorReport;
		public PXAction<APDocumentFilter> vendorHistoryReport;
		public PXAction<APDocumentFilter> aPAgedPastDueReport;
		public PXAction<APDocumentFilter> aPAgedOutstandingReport;
		public PXAction<APDocumentFilter> aPRegisterReport;


		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		public IEnumerable Refresh(PXAdapter adapter)
		{
			this.Filter.Current.FilterDetails = null;
			this.Filter.Current.RefreshTotals = true;
			return adapter.Get();
		}

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (this.Documents.Current != null)
			{
				PXRedirectHelper.TryRedirect(Documents.Cache, Documents.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return Filter.Select();
		}

		public PXAction<APDocumentFilter> viewOriginalDocument;
		[PXButton]
		public virtual IEnumerable ViewOriginalDocument(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				APInvoiceEntry graph = PXGraph.CreateInstance<APInvoiceEntry>();
				APRegister origDoc = PXSelect<APRegister,
					Where<APRegister.refNbr, Equal<Required<APRegister.origRefNbr>>,
						And<APRegister.docType, Equal<Required<APRegister.origDocType>>>>>
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
			APDocumentFilter filter = Filter.Current as APDocumentFilter;

			filter.UseMasterCalendar = (filter.OrganizationID == null && filter.BranchID == null);
			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);
			FinPeriod prevPeriod = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.FinPeriodID, looped: true);
			filter.FinPeriodID = prevPeriod != null ? prevPeriod.FinPeriodID : null;
			filter.RefreshTotals = true;
			filter.FilterDetails = null;

			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable NextPeriod(PXAdapter adapter)
		{
			APDocumentFilter filter = Filter.Current as APDocumentFilter;

			filter.UseMasterCalendar = (filter.OrganizationID == null && filter.BranchID == null);
			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);
			FinPeriod nextPeriod = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.FinPeriodID, looped: true);
			filter.FinPeriodID = nextPeriod != null ? nextPeriod.FinPeriodID : null;
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

		[PXUIField(DisplayName = Messages.NewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable CreateInvoice(PXAdapter adapter)
		{
			if (Filter.Current?.VendorID != null)
			{
				APInvoiceEntry invoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
				invoiceEntry.Clear();

				APInvoice newDoc = invoiceEntry.Document.Insert(new APInvoice());
				newDoc.VendorID = Filter.Current.VendorID;

				object oldVendorValue = null;
				object newVendorValue = Filter.Current.VendorID;

				invoiceEntry.Document.Cache.RaiseFieldVerifying<APInvoice.vendorID>(
					newDoc,
					ref newVendorValue);

				invoiceEntry.Document.Cache.RaiseFieldUpdated<APInvoice.vendorID>(
					newDoc,
					oldVendorValue);

				throw new PXRedirectRequiredException(invoiceEntry, nameof(CreateInvoice));
			}

			return Filter.Select();
		}
			
		[PXUIField(DisplayName = Messages.NewPayment, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable CreatePayment(PXAdapter adapter)
		{
			if (Filter.Current?.VendorID != null)
			{
				APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();
				paymentEntry.Clear();

				APPayment newDoc = paymentEntry.Document.Insert(new APPayment());
				newDoc.VendorID = Filter.Current.VendorID;

				object oldVendorValue = null;
				object newVendorValue = Filter.Current.VendorID;

				paymentEntry.Document.Cache.RaiseFieldVerifying<APPayment.vendorID>(
					newDoc,
					ref newVendorValue);

				paymentEntry.Document.Cache.RaiseFieldUpdated<APPayment.vendorID>(
					newDoc,
					oldVendorValue);

				throw new PXRedirectRequiredException(paymentEntry, nameof(CreatePayment));
			}

			return Filter.Select();
		}

		[PXUIField(DisplayName = Messages.APPayBill, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable PayDocument(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				if (this.Documents.Current.Payable != true)
					throw new PXException(Messages.Only_Invoices_MayBe_Payed);

				if (this.Documents.Current.Status != APDocStatus.Open)
					throw new PXException(Messages.Only_Open_Documents_MayBe_Processed);
								
				APInvoice doc = this.FindInvoice(Documents.Current);
				if (doc != null)
				{
					APPaymentEntry graph = PXGraph.CreateInstance<APPaymentEntry>();
					graph.Clear();
					try
					{
						graph.CreatePayment(doc);
					}
					catch (AdjustedNotFoundException)
					{
						APAdjust foundAPAdjust = PXSelect<APAdjust,
							Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
							And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
							And<APAdjust.released, Equal<False>>>>>.
							Select(graph, doc.DocType, doc.RefNbr);
						if (foundAPAdjust != null)
						{
							graph.Clear();
							graph.Document.Current = graph.Document.Search<APPayment.refNbr>(foundAPAdjust.AdjgRefNbr, foundAPAdjust.AdjgDocType);
							throw new PXRedirectRequiredException(graph, "PayInvoice");
						}
					}
					throw new PXRedirectRequiredException(graph, "PayInvoice");
				}
			}
			return Filter.Select();
		}

		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Reportsfolder(PXAdapter adapter)
		{
			return adapter.Get();
		}
		
		[PXUIField(DisplayName = Messages.APBalanceByVendorReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable APBalanceByVendorReport(PXAdapter adapter)
		{
			APDocumentFilter filter = Filter.Current;

			if (filter != null)
			{
				Dictionary<string, string> parameters = GetBasicReportParameters(filter);
				if (!string.IsNullOrEmpty(filter.FinPeriodID))
				{
					parameters["PeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.FinPeriodID);
				}
				parameters["UseMasterCalendar"] = filter.UseMasterCalendar == true ? true.ToString() : false.ToString();
				throw new PXReportRequiredException(parameters, "AP632500", Messages.APBalanceByVendorReport);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.VendorHistoryReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable VendorHistoryReport(PXAdapter adapter)
		{
			APDocumentFilter filter = Filter.Current;
			if (filter != null)
			{
				Dictionary<string, string> parameters = GetBasicReportParameters(filter);
				if (!string.IsNullOrEmpty(filter.FinPeriodID))
				{
					parameters["FromPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.FinPeriodID);
					parameters["ToPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.FinPeriodID);
				}
				throw new PXReportRequiredException(parameters, "AP652000", Messages.VendorHistoryReport);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.APAgedPastDueReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable APAgedPastDueReport(PXAdapter adapter)
		{
			APDocumentFilter filter = Filter.Current;
			if (filter != null)
			{
				Dictionary<string, string> parameters = GetBasicReportParameters(filter);
				throw new PXReportRequiredException(parameters, "AP631000", Messages.APAgedPastDueReport);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.APAgedOutstandingReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable APAgedOutstandingReport(PXAdapter adapter)
		{
			APDocumentFilter filter = Filter.Current;
			if (filter != null)
			{
				Dictionary<string, string> parameters = GetBasicReportParameters(filter);
				throw new PXReportRequiredException(parameters, "AP631500", Messages.APAgedOutstandingReport);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.APRegisterReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable APRegisterReport(PXAdapter adapter)
		{
			APDocumentFilter filter = Filter.Current;
			if (filter != null)
			{
				Dictionary<string, string> parameters = GetBasicReportParameters(filter);
				if (!string.IsNullOrEmpty(filter.FinPeriodID))
				{
					parameters["StartPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.FinPeriodID);
					parameters["EndPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.FinPeriodID);
				}
				throw new PXReportRequiredException(parameters, "AP621500", Messages.APRegisterReport);
			}
			return adapter.Get();
		}

		private Dictionary<string, string> GetBasicReportParameters(APDocumentFilter filter)
		{
			BAccountR bAccount = SelectFrom<BAccountR>
				.Where<BAccountR.bAccountID.IsEqual<@P.AsInt>>
				.View
				.Select(this, filter.OrgBAccountID);

			return new Dictionary<string, string>
			{
				{ "VendorID" , VendorMaint.FindByID(this, filter.VendorID)?.AcctCD },
				{ "OrgBAccountID", bAccount.AcctCD },
			};
		}

		#endregion

		#region Events Handlers
		public virtual void APDocumentFilter_AccountID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APDocumentFilter header = e.Row as APDocumentFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.AccountID = null;
			}
		}
		public virtual void APDocumentFilter_CuryID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APDocumentFilter header = e.Row as APDocumentFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.CuryID = null;
			}
		}
		public virtual void APDocumentFilter_SubID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		public virtual void APDocumentFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if(cache.ObjectsEqual<APDocumentFilter.organizationID>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.orgBAccountID>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.branchID>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.vendorID>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.finPeriodID>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.masterFinPeriodID>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.showAllDocs>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.includeUnreleased>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.accountID>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.subCD>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.subCDWildcard>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.docType>(e.Row, e.OldRow) &&
			   cache.ObjectsEqual<APDocumentFilter.curyID>(e.Row, e.OldRow))
			{
				return;
			}
			(e.Row as APDocumentFilter).RefreshTotals = true;
			(e.Row as APDocumentFilter).FilterDetails = null;			
		}
		public virtual void APDocumentFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			
			APDocumentFilter row = (APDocumentFilter)e.Row;
			bool byPeriod   = (row.FinPeriodID != null);
			PXCache docCache = Documents.Cache;
			bool isMCFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			bool isForeignCurrencySelected = String.IsNullOrEmpty(row.CuryID) == false && (row.CuryID != this.Company.Current.BaseCuryID);
			bool isBaseCurrencySelected = String.IsNullOrEmpty(row.CuryID) == false && (row.CuryID == this.Company.Current.BaseCuryID);

			PXUIFieldAttribute.SetVisible<APDocumentFilter.showAllDocs>(cache, row, !byPeriod);
			PXUIFieldAttribute.SetVisible<APDocumentFilter.curyID>(cache, row, isMCFeatureInstalled);
			PXUIFieldAttribute.SetVisible<APDocumentFilter.curyBalanceSummary>(cache, row, isMCFeatureInstalled && isForeignCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentFilter.curyDifference>(cache, row, isMCFeatureInstalled && isForeignCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentFilter.curyVendorBalance>(cache, row, isMCFeatureInstalled && isForeignCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentFilter.curyVendorRetainedBalance>(cache, row, isMCFeatureInstalled && isForeignCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentFilter.curyVendorDepositsBalance>(cache, row, isMCFeatureInstalled && isForeignCurrencySelected);

			PXUIFieldAttribute.SetVisible<APDocumentResult.curyID>(docCache, null, isMCFeatureInstalled);
			PXUIFieldAttribute.SetVisible<APDocumentResult.rGOLAmt>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentResult.curyBegBalance>(docCache, null, byPeriod && isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentResult.begBalance>(docCache, null, byPeriod);
			PXUIFieldAttribute.SetVisible<APDocumentResult.curyOrigDocAmt>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentResult.curyDocBal>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentResult.curyDiscActTaken>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);			
			PXUIFieldAttribute.SetVisible<APDocumentResult.curyTaxWheld>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentResult.curyRetainageTotal>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentResult.curyOrigDocAmtWithRetainageTotal>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
			PXUIFieldAttribute.SetVisible<APDocumentResult.curyRetainageUnreleasedAmt>(docCache, null, isMCFeatureInstalled && !isBaseCurrencySelected);
								
			//PXUIFieldAttribute.SetEnabled<APDocumentResult.status>(docCache, null, true);//???
			//PXUIFieldAttribute.SetEnabled<APDocumentResult.curyDocBal>(docCache, null, true);//???

			aPBalanceByVendorReport.SetEnabled(row.VendorID != null);
			vendorHistoryReport.SetEnabled(row.VendorID != null);
			aPAgedPastDueReport.SetEnabled(row.VendorID != null);
			aPAgedOutstandingReport.SetEnabled(row.VendorID != null);
			aPRegisterReport.SetEnabled(row.VendorID != null);
		}
		#endregion

		#region Utility Functions - internal
		protected virtual void SetSummary(APDocumentFilter aDest, APVendorBalanceEnq.APHistorySummary aSrc)
		{
			aDest.VendorBalance = aSrc.BalanceSummary;
			aDest.VendorDepositsBalance = aSrc.DepositsSummary;
			aDest.CuryVendorBalance = aSrc.CuryBalanceSummary;
			aDest.CuryVendorDepositsBalance = aSrc.CuryDepositsSummary;
		}

		protected virtual void Aggregate(APDocumentFilter aDest, APDocumentResult aSrc)
		{
			aDest.BalanceSummary += aSrc.DocBal ?? Decimal.Zero; 
			aDest.CuryBalanceSummary += aSrc.CuryDocBal ?? Decimal.Zero;
			aDest.VendorRetainedBalance += aSrc.RetainageUnreleasedAmt ?? Decimal.Zero;
			aDest.CuryVendorRetainedBalance += aSrc.CuryRetainageUnreleasedAmt ?? Decimal.Zero;
		}

		protected virtual APInvoice FindInvoice(APDocumentResult aRes) 
		{
			APInvoice doc = FindInvoice(this,aRes.DocType, aRes.RefNbr);
			return doc;
		}
		protected virtual APPayment FindPayment(APDocumentResult aRes)
		{
			return FindPayment(this, aRes.DocType, aRes.RefNbr);
		}
		protected virtual void InvertValuesSign(APDocumentResult aRes)
		{
			aRes.OrigDocAmt = -aRes.OrigDocAmt;
			aRes.DocBal = -aRes.DocBal;
			aRes.BegBalance = -aRes.BegBalance;
			aRes.DiscActTaken = -aRes.DiscActTaken;
			aRes.DiscTaken = -aRes.DiscTaken;
			aRes.DiscBal = -aRes.DiscBal;
			aRes.OrigDiscAmt = -aRes.OrigDiscAmt;
			aRes.OrigWhTaxAmt = -aRes.OrigWhTaxAmt;
			aRes.WhTaxBal = -aRes.WhTaxBal;
			aRes.TaxWheld = -aRes.TaxWheld;
			aRes.RetainageUnreleasedAmt = -aRes.RetainageUnreleasedAmt;
			aRes.CuryOrigDocAmt = -aRes.CuryOrigDocAmt;
			aRes.CuryDocBal = -aRes.CuryDocBal;
			aRes.CuryBegBalance = -aRes.CuryBegBalance;
			aRes.CuryDiscActTaken = -aRes.CuryDiscActTaken;
			aRes.CuryDiscTaken = -aRes.CuryDiscTaken;
			aRes.CuryOrigDiscAmt = -aRes.CuryOrigDiscAmt;
			aRes.CuryDiscBal = -aRes.CuryDiscBal;
			aRes.CuryWhTaxBal = -aRes.CuryWhTaxBal;
			aRes.CuryOrigWhTaxAmt = -aRes.CuryOrigWhTaxAmt;
			aRes.CuryTaxWheld = -aRes.CuryTaxWheld;
			aRes.CuryRetainageUnreleasedAmt = -aRes.CuryRetainageUnreleasedAmt;

		}
	
		#endregion

		#region Utility Functions - public
		public static APInvoice FindInvoice(PXGraph aGraph, string aDocType, string apRefNbr) 
		{
			APInvoice doc = PXSelect<APInvoice,
						Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
						And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>
						.Select(aGraph, aDocType, apRefNbr);
			return doc;
		}
		public static APPayment FindPayment(PXGraph aGraph, string aDocType, string apRefNbr)
		{
			APPayment doc = PXSelect<APPayment,
						Where<APPayment.docType, Equal<Required<APPayment.docType>>,
						And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>
						.Select(aGraph, aDocType, apRefNbr);
			return doc;
		}
		
		[Obsolete("Obsoilete. Will be removed in Acumatica ERP 2019R1")]
		public static bool? IsInvoiceType(string aDocType) 
		{
			return APDocType.Payable(aDocType);			
		}
		#endregion
	}
}
