using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using static PX.Objects.AR.ARInvoiceEntry;
using PX.Objects.Common;
using PX.Objects.IN;
using PX.Objects.PM;
using System.Linq;

namespace PX.Objects.AR
{
	/// <exclude />
	[Serializable]
	public partial class ARRetainageFilter : IBqlTable
	{
	    #region BranchIDARActiveProjectAttibute
	    public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

	    [Branch(PersistingCheck = PXPersistingCheck.Nothing)]
	    public virtual Int32? BranchID { get; set; }
	    #endregion

        #region DocDate
        public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate { get; set; }
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[AROpenPeriod(typeof(ARRetainageFilter.docDate),
			typeof(ARRetainageFilter.branchID),
			useMasterOrganizationIDByDefault: true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FinPeriodID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		[Customer(
			Visibility = PXUIVisibility.SelectorVisible,
			Required = false,
			DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[PM.ActiveProjectOrContractBaseAttribute(typeof(ARInvoice.customerID), FieldClass = PM.ProjectAttribute.DimensionName)]
		public virtual Int32? ProjectID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

		[ProjectTask(typeof(ARRetainageFilter.projectID), FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		public virtual int? ProjectTaskID { get; set; }
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }

		[AccountGroup(FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		public virtual int? AccountGroupID { get; set; }
		#endregion
		#region CostCode
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

		[CostCode(Filterable = false, SkipVerification = true, FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		public virtual int? CostCodeID { get; set; }
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PMInventorySelector(Filterable = true)]
		public virtual int? InventoryID { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[ARInvoiceType.RefNbr(typeof(Search2<Standalone.ARRegisterAlias.refNbr,
			InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<Standalone.ARRegisterAlias.docType>,
				And<ARInvoice.refNbr, Equal<Standalone.ARRegisterAlias.refNbr>>>,
			InnerJoinSingleTable<Customer, On<Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>,
			Where<Standalone.ARRegisterAlias.curyRetainageUnreleasedAmt, Greater<decimal0>,
				And<Standalone.ARRegisterAlias.curyRetainageTotal, Greater<decimal0>,
				And<Standalone.ARRegisterAlias.docType, Equal<ARDocType.invoice>,
				And<Standalone.ARRegisterAlias.retainageApply, Equal<True>,
				And<Standalone.ARRegisterAlias.released, Equal<True>>>>>>,
			OrderBy<Desc<Standalone.ARRegisterAlias.refNbr>>>))]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.Visible)]
		public virtual string RefNbr { get; set; }
		#endregion

		#region ShowBillsWithOpenBalance
		public abstract class showBillsWithOpenBalance : PX.Data.BQL.BqlBool.Field<showBillsWithOpenBalance> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Lines with Open Balance", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ShowBillsWithOpenBalance { get; set; }
		#endregion
		#region RetainageReleasePct
		public abstract class retainageReleasePct : PX.Data.BQL.BqlDecimal.Field<retainageReleasePct> { }

		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "100.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Retainage Percent", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainageReleasePct { get; set; }
		#endregion

		#region CuryRetainageReleasedAmt
		public abstract class curyRetainageReleasedAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageReleasedAmt> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Retainage to Release", FieldClass = nameof(FeaturesSet.Retainage), Enabled = false)]
		public virtual decimal? CuryRetainageReleasedAmt { get; set; }
		#endregion
	}

	[Serializable]
	[PXProjection(typeof(Select2<ARInvoice,
		InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARInvoice.docType>,
			And<ARRegister.refNbr, Equal<ARInvoice.refNbr>>>,
		LeftJoin<ARTran, On<ARRegister.paymentsByLinesAllowed, Equal<True>,
			And<ARTran.tranType, Equal<ARInvoice.docType>,
			And<ARTran.refNbr, Equal<ARInvoice.refNbr>,
			And<ARTran.curyRetainageBal, Greater<decimal0>,
			And<ARTran.curyRetainageAmt, Greater<decimal0>>>>>>,
		LeftJoin<Account, On<ARRegister.paymentsByLinesAllowed, Equal<True>, 
			And<Account.accountID, Equal<ARTran.accountID>>>>>>,
		Where2<
			Where<CurrentValue<ARRetainageFilter.customerID>, IsNull, Or<ARRegister.customerID, Equal<CurrentValue<ARRetainageFilter.customerID>>>>,
			And2<Where<CurrentValue<ARRetainageFilter.projectID>, IsNull, Or<ARInvoice.projectID, Equal<CurrentValue<ARRetainageFilter.projectID>>>>,
			And2<Where<CurrentValue<ARRetainageFilter.branchID>, IsNull, Or<ARRegister.branchID, Equal<CurrentValue<ARRetainageFilter.branchID>>>>,
			And2<Where<CurrentValue<ARRetainageFilter.showBillsWithOpenBalance>, Equal<True>,
				Or<Where<ARRegister.curyDocBal, Equal<decimal0>,
				And<CurrentValue<ARRetainageFilter.showBillsWithOpenBalance>, NotEqual<True>>>>>,
			And<ARRegister.curyRetainageUnreleasedAmt, Greater<decimal0>,
			And<ARRegister.curyRetainageTotal, Greater<decimal0>,
			And<ARRegister.docType, Equal<ARDocType.invoice>,
			And<ARRegister.retainageApply, Equal<True>,
			And<ARRegister.released, Equal<True>,
			And<ARRegister.docDate, LessEqual<CurrentValue<ARRetainageFilter.docDate>>,
			And2<Where<ARRegister.refNbr, Equal<CurrentValue<ARRetainageFilter.refNbr>>,
				Or<CurrentValue<ARRetainageFilter.refNbr>, IsNull>>,

			And<Where<
				ARRegister.paymentsByLinesAllowed, NotEqual<True>, 
				Or2<Where<CurrentValue<ARRetainageFilter.projectTaskID>, IsNull, Or<ARTran.taskID, Equal<CurrentValue<ARRetainageFilter.projectTaskID>>>>,
				And2<Where<CurrentValue<ARRetainageFilter.accountGroupID>, IsNull, Or<Account.accountGroupID, Equal<CurrentValue<ARRetainageFilter.accountGroupID>>>>,
				And2<Where<CurrentValue<ARRetainageFilter.costCodeID>, IsNull, Or<ARTran.costCodeID, Equal<CurrentValue<ARRetainageFilter.costCodeID>>>>,
				And<Where<CurrentValue<ARRetainageFilter.inventoryID>, IsNull, Or<ARTran.inventoryID, Equal<CurrentValue<ARRetainageFilter.inventoryID>>>>>>>>>>>>>>>>>>>>>,
		OrderBy<Asc<ARRegister.refNbr>>>))]
	public partial class ARInvoiceExt : ARInvoice
	{
		#region Key fields

		#region DocType
		public new abstract class docType : IBqlField { }

		[PXDBString(3,
			IsKey = true,
			IsFixed = true,
			BqlField = typeof(ARInvoice.docType))]
		[ARInvoiceType.List]
		[PXUIField(DisplayName = "Type")]
		public override string DocType
		{
			get
			{
				return _DocType;
			}
			set
			{
				_DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : IBqlField { }

		[PXDBString(15,
			IsKey = true,
			IsUnicode = true,
			InputMask = ">CCCCCCCCCCCCCCC",
			BqlField = typeof(ARInvoice.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXSelector(typeof(ARInvoiceExt.refNbr))]
		public override string RefNbr
		{
			get
			{
				return _RefNbr;
			}
			set
			{
				_RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : IBqlField { }

		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXFormula(typeof(IsNull<ARInvoiceExt.aRTranLineNbr, int0>))]
		public virtual int? LineNbr
	{
			get;
			set;
		}
		#endregion

		#endregion

		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		/// <summary>
		/// Code of the <see cref="PX.Objects.CM.Currency">Currency</see> of the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Company.BaseCuryID">company's base currency</see>.
		/// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(ARRegister.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, FieldClass = nameof(FeaturesSet.Multicurrency))]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public override string CuryID
		{
			get;
			set;
		}

		#endregion
		#region DisplayProjectID
		public abstract class displayProjectID : IBqlField { }

		[PXInt]
		[PXUIField(DisplayName = "Project", Enabled = false)]
		[PXSelector(typeof(PMProject.contractID),
			SubstituteKey = typeof(PMProject.contractCD),
			ValidateValue = false)]
		[PXFormula(typeof(Switch<Case<Where<ARInvoiceExt.paymentsByLinesAllowed, Equal<True>>, ARInvoiceExt.aRTranProjectID>, ARInvoiceExt.projectID>))]
		public virtual int? DisplayProjectID
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainageBal
		public abstract class curyRetainageBal : IBqlField { }

		[PXCurrency(typeof(ARInvoiceExt.curyInfoID), typeof(ARInvoiceExt.retainageBal), BaseCalc = false)]
		[PXFormula(typeof(IsNull<ARInvoiceExt.aRTranCuryRetainageBal, ARInvoiceExt.curyRetainageUnreleasedAmt>))]
		public virtual decimal? CuryRetainageBal
		{
			get;
			set;
		}
		#endregion
		#region RetainageBal
		public abstract class retainageBal : IBqlField { }

		[PXBaseCury]
		[PXFormula(typeof(IsNull<ARInvoiceExt.aRTranRetainageBal, ARInvoiceExt.retainageUnreleasedAmt>))]
		public virtual decimal? RetainageBal
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDocAmtWithRetainageTotal
		public new abstract class curyOrigDocAmtWithRetainageTotal : IBqlField { }

		[PXCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.origDocAmtWithRetainageTotal), BaseCalc = false)]
		[PXUIField(DisplayName = "Total Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(IsNull<Add<ARInvoiceExt.aRTranCuryOrigRetainageAmt, ARInvoiceExt.aRTranCuryOrigTranAmt>,
			Add<ARRegister.curyOrigDocAmt, ARRegister.curyRetainageTotal>>))]
		public override decimal? CuryOrigDocAmtWithRetainageTotal
		{
			get;
			set;
		}
		#endregion
		#region OrigDocAmtWithRetainageTotal
		public new abstract class origDocAmtWithRetainageTotal : IBqlField { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Total Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXFormula(typeof(IsNull<Add<ARInvoiceExt.aRTranOrigRetainageAmt, ARInvoiceExt.aRTranOrigTranAmt>,
			Add<ARRegister.curyOrigDocAmt, ARRegister.curyRetainageTotal>>))]
		public override decimal? OrigDocAmtWithRetainageTotal
		{
			get;
			set;
		}
		#endregion

		#region RetainageReleasePct
		public abstract class retainageReleasePct : PX.Data.BQL.BqlDecimal.Field<retainageReleasePct> { }

		[UnboundRetainagePercent(
			typeof(True),
			typeof(ARRetainageFilter.retainageReleasePct),
			typeof(ARInvoiceExt.curyRetainageBal),
			typeof(ARInvoiceExt.curyRetainageReleasedAmt),
			typeof(ARInvoiceExt.retainageReleasePct),
			DisplayName = "Percent to Release")]
		public virtual decimal? RetainageReleasePct
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainageReleasedAmt
		public abstract class curyRetainageReleasedAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageReleasedAmt> { }

		[UnboundRetainageAmount(
			typeof(ARInvoiceExt.curyInfoID),
			typeof(ARInvoiceExt.curyRetainageBal),
			typeof(ARInvoiceExt.curyRetainageReleasedAmt),
			typeof(ARInvoiceExt.retainageReleasedAmt),
			typeof(ARInvoiceExt.retainageReleasePct),
			DisplayName = "Retainage to Release")]
		[PXParent(typeof(Select<ARRetainageFilter>), UseCurrent = true)]
		[PXUnboundFormula(typeof(Switch<Case<Where<ARInvoiceExt.selected, Equal<True>>, ARInvoiceExt.curyRetainageReleasedAmt>, decimal0>), 
			typeof(SumCalc<ARRetainageFilter.curyRetainageReleasedAmt>))]
		public virtual decimal? CuryRetainageReleasedAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageReleasedAmt

		public abstract class retainageReleasedAmt : PX.Data.BQL.BqlDecimal.Field<retainageReleasedAmt> { }
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainageReleasedAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryRetainageUnreleasedCalcAmt
		public abstract class curyRetainageUnreleasedCalcAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageUnreleasedCalcAmt> { }

		[PXCurrency(typeof(ARInvoiceExt.curyInfoID), typeof(ARInvoiceExt.retainageUnreleasedCalcAmt))]
		[PXUIField(DisplayName = "Unreleased Retainage")]
		[PXFormula(typeof(Sub<ARInvoiceExt.curyRetainageBal, ARInvoiceExt.curyRetainageReleasedAmt>))]
		public virtual decimal? CuryRetainageUnreleasedCalcAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageUnreleasedCalcAmt
		public abstract class retainageUnreleasedCalcAmt : PX.Data.BQL.BqlDecimal.Field<retainageUnreleasedCalcAmt> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainageUnreleasedCalcAmt
		{
			get;
			set;
		}
		#endregion
		
		#region ARTran fields

		#region ARTranLineNbr
		public abstract class aRTranLineNbr : IBqlField { }

		[PXDBInt(BqlField = typeof(ARTran.lineNbr))]
		public virtual int? ARTranLineNbr
		{
			get;
			set;
		}
		#endregion
		#region ARTranSortOrder
		public abstract class aRTranSortOrder : IBqlField { }

		/// <summary>
		/// The sort order of the detail line.
		/// </summary>
		[PXDBInt(BqlField = typeof(ARTran.sortOrder))]
		[PXUIField(DisplayName = "Line Nbr.",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		public virtual int? ARTranSortOrder
		{
			get;
			set;
		}
		#endregion
		#region ARTranInventoryID
		public abstract class aRTranInventoryID : IBqlField { }

		[PXDBInt(BqlField = typeof(ARTran.inventoryID))]
		[PXUIField(DisplayName = "Inventory ID",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXSelector(typeof(InventoryItem.inventoryID),
			SubstituteKey = typeof(InventoryItem.inventoryCD),
			ValidateValue = false)]
		public virtual int? ARTranInventoryID
		{
			get;
			set;
		}
		#endregion
		#region ARTranProjectID
		public abstract class aRTranProjectID : IBqlField { }

		[PXDBInt(BqlField = typeof(ARTran.projectID))]
		[PXUIField(DisplayName = "Project",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXSelector(typeof(PMProject.contractID),
			SubstituteKey = typeof(PMProject.contractCD),
			ValidateValue = false)]
		public virtual int? ARTranProjectID
		{
			get;
			set;
		}
		#endregion
		#region ARTranTaskID
		public abstract class aRTranTaskID : IBqlField { }

		[PXDBInt(BqlField = typeof(ARTran.taskID))]
		[PXUIField(DisplayName = "Project Task",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXSelector(typeof(PMTask.taskID),
			SubstituteKey = typeof(PMTask.taskCD),
			ValidateValue = false)]
		public virtual int? ARTranTaskID
		{
			get;
			set;
		}
		#endregion
		#region ARTranCostCodeID
		public abstract class aRTranCostCodeID : IBqlField { }

		[PXDBInt(BqlField = typeof(ARTran.costCodeID))]
		[PXUIField(DisplayName = "Cost Code",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXSelector(typeof(PMCostCode.costCodeID),
			SubstituteKey = typeof(PMCostCode.costCodeCD),
			ValidateValue = false)]
		public virtual int? ARTranCostCodeID
		{
			get;
			set;
		}
		#endregion
		#region ARTranAccountID
		public abstract class aRTranAccountID : IBqlField { }

		[PXDBInt(BqlField = typeof(ARTran.accountID))]
		[PXUIField(DisplayName = "Account",
			Enabled = false,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXSelector(typeof(Account.accountID),
			SubstituteKey = typeof(Account.accountCD),
			ValidateValue = false)]
		public virtual int? ARTranAccountID
		{
			get;
			set;
		}
		#endregion
		#region ARTranCuryOrigRetainageAmt
		public abstract class aRTranCuryOrigRetainageAmt : IBqlField { }

		[PXDBDecimal(BqlField = typeof(ARTran.curyOrigRetainageAmt))]
		public virtual decimal? ARTranCuryOrigRetainageAmt
		{
			get;
			set;
		}
		#endregion
		#region ARTranOrigRetainageAmt
		public abstract class aRTranOrigRetainageAmt : IBqlField { }

		[PXDBDecimal(BqlField = typeof(ARTran.origRetainageAmt))]
		public virtual decimal? ARTranOrigRetainageAmt
		{
			get;
			set;
		}
		#endregion
		#region ARTranCuryRetainageBal
		public abstract class aRTranCuryRetainageBal : IBqlField { }

		[PXDBDecimal(BqlField = typeof(ARTran.curyRetainageBal))]
		public virtual decimal? ARTranCuryRetainageBal
		{
			get;
			set;
		}
		#endregion
		#region ARTranRetainageBal
		public abstract class aRTranRetainageBal : IBqlField { }

		[PXDBDecimal(BqlField = typeof(ARTran.retainageBal))]
		public virtual decimal? ARTranRetainageBal
		{
			get;
			set;
		}
		#endregion
		#region ARTranCuryOrigTranAmt
		public abstract class aRTranCuryOrigTranAmt : IBqlField { }

		[PXDBDecimal(BqlField = typeof(ARTran.curyOrigTranAmt))]
		public virtual decimal? ARTranCuryOrigTranAmt
		{
			get;
			set;
		}
		#endregion
		#region ARTranOrigTranAmt
		public abstract class aRTranOrigTranAmt : IBqlField { }

		[PXDBDecimal(BqlField = typeof(ARTran.origTranAmt))]
		public virtual decimal? ARTranOrigTranAmt
		{
			get;
			set;
		}
		#endregion

		#endregion
	}

	[TableAndChartDashboardType]
	public class ARRetainageRelease : PXGraph<ARRetainageRelease>
	{
		public PXFilter<ARRetainageFilter> Filter;
		public PXCancel<ARRetainageFilter> Cancel;

		[PXFilterable]
		public PXFilteredProcessing<ARInvoiceExt, ARRetainageFilter> DocumentList;

		public PXSetup<ARSetup> ARSetup;

		public PXAction<ARRetainageFilter> viewDocument;
		[PXButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (DocumentList.Current != null)
			{
				PXRedirectHelper.TryRedirect(DocumentList.Cache, DocumentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		protected virtual IEnumerable documentList()
		{
			foreach (ARInvoiceExt doc in PXSelect<ARInvoiceExt>.Select(this))
			{
				bool hasUnreleasedDocument = false;

				foreach (PXResult<ARRetainageInvoice, ARTran> res in PXSelectJoin<ARRetainageInvoice,
					LeftJoin<ARTran, On<ARRetainageInvoice.paymentsByLinesAllowed, Equal<True>,
						And<ARTran.tranType, Equal<ARRetainageInvoice.docType>,
						And<ARTran.refNbr, Equal<ARRetainageInvoice.refNbr>,
						And<ARTran.origLineNbr, Equal<Required<ARTran.origLineNbr>>>>>>>,
				Where<ARRetainageInvoice.isRetainageDocument, Equal<True>,
					And<ARRetainageInvoice.origDocType, Equal<Required<ARInvoice.docType>>,
					And<ARRetainageInvoice.origRefNbr, Equal<Required<ARInvoice.refNbr>>,
						And<ARRetainageInvoice.released, NotEqual<True>>>>>>
					.SelectSingleBound(this, null, doc.ARTranLineNbr, doc.DocType, doc.RefNbr))
				{
					ARRetainageInvoice invoice = res;
					ARTran tran = res;

					if (invoice.PaymentsByLinesAllowed != true ||
						tran.LineNbr != null)
					{
						hasUnreleasedDocument = true;
					}
				}

				if (!hasUnreleasedDocument)
				{
					ARRetainageFilter filter = Filter.Current;
					bool hasProjectTransaction = true;

					if (doc.PaymentsByLinesAllowed != true && 
						(filter.ProjectTaskID != null ||
						filter.AccountGroupID != null ||
						filter.CostCodeID != null ||
						filter.InventoryID != null))
					{
						ARTran projectTran = SearchProjectTransaction(doc);
						hasProjectTransaction = projectTran != null;
					}

					if (hasProjectTransaction)
					{
					yield return doc;
			}
		}
			}
		}

		private ARTran SearchProjectTransaction(ARInvoiceExt doc)
		{
			ARRetainageFilter filter = Filter.Current;
			List<object> parameters = new List<object>();

			var selectProjectTransaction = new PXSelectJoin<ARTran,
				LeftJoin<Account, On<Account.accountID, Equal<ARTran.accountID>>>,
				Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>(this);
			parameters.Add(doc.DocType);
			parameters.Add(doc.RefNbr);

			if (filter.ProjectTaskID != null)
			{
				selectProjectTransaction.WhereAnd<Where<ARTran.taskID, Equal<Required<ARTran.taskID>>>>();
				parameters.Add(filter.ProjectTaskID);
			}

			if (filter.AccountGroupID != null)
			{
				selectProjectTransaction.WhereAnd<Where<Account.accountGroupID, Equal<Required<Account.accountGroupID>>>>();
				parameters.Add(filter.AccountGroupID);
			}

			if (filter.CostCodeID != null)
			{
				selectProjectTransaction.WhereAnd<Where<ARTran.costCodeID, Equal<Required<ARTran.costCodeID>>>>();
				parameters.Add(filter.CostCodeID);
			}

			if (filter.InventoryID != null)
			{
				selectProjectTransaction.WhereAnd<Where<ARTran.inventoryID, Equal<Required<ARTran.inventoryID>>>>();
				parameters.Add(filter.InventoryID);
			}

			return selectProjectTransaction.SelectSingle(parameters.ToArray());
		}

		public ARRetainageRelease()
		{
			ARSetup setup = ARSetup.Current;
		}

		protected virtual void ARRetainageFilter_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARRetainageFilter filter = e.Row as ARRetainageFilter;
			if (filter == null) return;

			var document = PXSelectorAttribute.Select<ARRetainageFilter.refNbr>(sender, filter, e.NewValue);
			if (document is null)
				{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void ARRetainageFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARRetainageFilter filter = e.Row as ARRetainageFilter;
			if (filter == null) return;

			bool isAutoRelease = ARSetup.Current.RetainageInvoicesAutoRelease == true;

			DocumentList.SetProcessDelegate(delegate (List<ARInvoiceExt> list)
				 {
				ARInvoiceEntry graph = CreateInstance<ARInvoiceEntry>();
				ARInvoiceEntryRetainage retainageExt = graph.GetExtension<ARInvoiceEntryRetainage>();

				RetainageOptions retainageOptions = new RetainageOptions();
				retainageOptions.DocDate = filter.DocDate;
				retainageOptions.MasterFinPeriodID = FinPeriodIDAttribute.CalcMasterPeriodID<ARRetainageFilter.finPeriodID>(graph.Caches[typeof(ARRetainageFilter)], filter);

				retainageExt.ReleaseRetainageProc(list, retainageOptions, isAutoRelease);
			});
		}

		protected virtual void _(Events.RowUpdated<ARRetainageFilter> e)
		{
			if (e.Row == null || e.OldRow == null) return;

			if (!e.Cache.ObjectsEqual<
					ARRetainageFilter.branchID, 
					ARRetainageFilter.docDate, 
					ARRetainageFilter.finPeriodID,
					ARRetainageFilter.customerID,
					ARRetainageFilter.projectID,
					ARRetainageFilter.refNbr,
					ARRetainageFilter.showBillsWithOpenBalance>(e.Row, e.OldRow) ||
				!e.Cache.ObjectsEqual<
					ARRetainageFilter.projectTaskID,
					ARRetainageFilter.accountGroupID,
					ARRetainageFilter.costCodeID,
					ARRetainageFilter.inventoryID>(e.Row, e.OldRow))
			{
				DocumentList.Cache.Clear();
				DocumentList.View.Clear();

				e.Row.CuryRetainageReleasedAmt = 0m;
			}
			else if (!e.Cache.ObjectsEqual<ARRetainageFilter.retainageReleasePct>(e.Row, e.OldRow))
			{
				decimal retainageTotal = 0m;

				foreach (ARInvoiceExt item in DocumentList.Select())
				{
					DocumentList.Cache.SetValueExt<ARInvoiceExt.retainageReleasePct>(item, e.Row.RetainageReleasePct);
					retainageTotal += item.Selected == true ? (item.CuryRetainageReleasedAmt ?? 0m) : 0m;
				}

				// Manually calculate amount here, because it is too late for automatic 
				// ARInvoiceExt.CuryRetainageReleasedAmt formula calculation and filter
				// totals won't be updated.
				// 
				e.Row.CuryRetainageReleasedAmt = retainageTotal;
			}
		}

		protected virtual void ARInvoiceExt_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARInvoiceExt invoice = e.Row as ARInvoiceExt;
			if (invoice == null) return;

			PXUIFieldAttribute.SetEnabled(sender, invoice, false);
			PXUIFieldAttribute.SetEnabled<ARInvoiceExt.selected>(sender, invoice, true);
			PXUIFieldAttribute.SetEnabled<ARInvoiceExt.retainageReleasePct>(sender, invoice, true);
			PXUIFieldAttribute.SetEnabled<ARInvoiceExt.curyRetainageReleasedAmt>(sender, invoice, true);
			
			if (invoice.Selected ?? true)
			{
				Dictionary<String, String> errors = PXUIFieldAttribute.GetErrors(sender, invoice, PXErrorLevel.Error);
				if (errors.Count > 0)
				{
					invoice.Selected = false;
					DocumentList.Cache.SetStatus(invoice, PXEntryStatus.Updated);
					sender.RaiseExceptionHandling<ARInvoiceExt.selected>(
						invoice,
						null,
						new PXSetPropertyException(PX.Objects.AP.Messages.ErrorRaised, PXErrorLevel.RowError));

					PXUIFieldAttribute.SetEnabled<ARInvoiceExt.selected>(sender, invoice, false);
				}
			}
		}

		public override bool IsDirty => false;
	}
}
