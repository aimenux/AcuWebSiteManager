using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;

using PX.Common;
using PX.Data;
using PX.Data.EP;

using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.TX;
using PX.Objects.AR.MigrationMode;
using PX.Objects.Common.Abstractions;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents the processing parameters for the Generate VAT Credit 
	/// Memos (AR504500) process, which corresponds to the <see 
	/// cref="ARPPDCreditMemoProcess"/> graph.
	/// </summary>
	[Serializable]
	public partial class ARPPDCreditMemoParameters : IBqlTable
	{
		#region ApplicationDate
		public abstract class applicationDate : PX.Data.BQL.BqlDateTime.Field<applicationDate> { }
		protected DateTime? _ApplicationDate;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual DateTime? ApplicationDate
		{
			get
			{
				return _ApplicationDate;
			}
			set
			{
				_ApplicationDate = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected int? _BranchID;
		[Branch]
		public virtual int? BranchID
		{
			get
			{
				return _BranchID;
			}
			set
			{
				_BranchID = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected int? _CustomerID;
		[Customer]
		public virtual int? CustomerID
		{
			get
			{
				return _CustomerID;
			}
			set
			{
				_CustomerID = value;
			}
		}
		#endregion
		#region GenerateOnePerCustomer
		public abstract class generateOnePerCustomer : PX.Data.BQL.BqlBool.Field<generateOnePerCustomer> { }
		protected bool? _GenerateOnePerCustomer;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Consolidate Credit Memos by Customer", Visibility = PXUIVisibility.Visible)]
		public virtual bool? GenerateOnePerCustomer
		{
			get
			{
				return _GenerateOnePerCustomer;
			}
			set
			{
				_GenerateOnePerCustomer = value;
			}
		}
		#endregion
		#region CreditMemoDate
		public abstract class creditMemoDate : PX.Data.BQL.BqlDateTime.Field<creditMemoDate> { }
		protected DateTime? _CreditMemoDate;
		[PXDBDate]
		[PXFormula(typeof(Switch<Case<Where<ARPPDCreditMemoParameters.generateOnePerCustomer, Equal<True>>, Current<AccessInfo.businessDate>>, Null>))]
		[PXUIField(DisplayName = "Credit Memo Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? CreditMemoDate
		{
			get
			{
				return _CreditMemoDate;
			}
			set
			{
				_CreditMemoDate = value;
			}
		}
		#endregion
		#region FinPeriod
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected string _FinPeriodID;
		[AROpenPeriod(typeof(ARPPDCreditMemoParameters.creditMemoDate), typeof(ARPPDCreditMemoParameters.branchID))]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.Visible)]
		public virtual string FinPeriodID
		{
			get
			{
				return _FinPeriodID;
			}
			set
			{
				_FinPeriodID = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// A projection over <see cref="ARAdjust"/>, which selects
	/// applications of payments to invoices that have been paid
	/// in full and await processing of the cash discount on the 
	/// Generate VAT Credit Memos (AR504500) process.
	/// </summary>
	[PXProjection(typeof(Select2<ARAdjust,
		InnerJoin<AR.ARInvoice, On<AR.ARInvoice.docType, Equal<ARAdjust.adjdDocType>, And<AR.ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>>,
		Where<AR.ARInvoice.released, Equal<True>,
			And<AR.ARInvoice.pendingPPD, Equal<True>,
			And<AR.ARInvoice.openDoc, Equal<True>,
			And<ARAdjust.released, Equal<True>,
			And<ARAdjust.voided, NotEqual<True>,
			And<ARAdjust.pendingPPD, Equal<True>,
			And<ARAdjust.pPDCrMemoRefNbr, IsNull,
			And<Where<ARAdjust.adjgDocType, Equal<ARDocType.payment>,
				Or<ARAdjust.adjgDocType, Equal<ARDocType.prepayment>>>>>>>>>>>>))]
	[Serializable]
	public partial class PendingPPDCreditMemoApp : ARAdjust
	{
		#region Selected
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		#endregion
		#region Index
		public abstract class index : PX.Data.BQL.BqlInt.Field<index> { }
		protected int? _Index;
		[PXInt]
		public virtual int? Index
		{
			get
			{
				return _Index;
			}
			set
			{
				_Index = value;
			}
		}
		#endregion

		#region ARAdjust key fields

		#region PayDocType
		public abstract class payDocType : PX.Data.BQL.BqlString.Field<payDocType> { }
		protected string _PayDocType;
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "", BqlField = typeof(ARAdjust.adjgDocType))]
		public virtual string PayDocType
		{
			get
			{
				return _PayDocType;
			}
			set
			{
				_PayDocType = value;
			}
		}
		#endregion
		#region PayRefNbr
		public abstract class payRefNbr : PX.Data.BQL.BqlString.Field<payRefNbr> { }
		protected string _PayRefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(ARAdjust.adjgRefNbr))]
		public virtual string PayRefNbr
		{
			get
			{
				return _PayRefNbr;
			}
			set
			{
				_PayRefNbr = value;
			}
		}
		#endregion
		#region InvDocType
		public abstract class invDocType : PX.Data.BQL.BqlString.Field<invDocType> { }
		protected string _InvDocType;
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "", BqlField = typeof(ARAdjust.adjdDocType))]
		public virtual string InvDocType
		{
			get
			{
				return _InvDocType;
			}
			set
			{
				_InvDocType = value;
			}
		}
		#endregion
		#region InvRefNbr
		public abstract class invRefNbr : PX.Data.BQL.BqlString.Field<invRefNbr> { }
		protected string _InvRefNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(ARAdjust.adjdRefNbr))]
		public virtual string InvRefNbr
		{
			get
			{
				return _InvRefNbr;
			}
			set
			{
				_InvRefNbr = value;
			}
		}
		#endregion
		
		#endregion
		#region ARInvoice fields

		#region InvCuryID
		public abstract class invCuryID : PX.Data.BQL.BqlString.Field<invCuryID> { }
		protected string _InvCuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(AR.ARInvoice.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string InvCuryID
		{
			get
			{
				return _InvCuryID;
			}
			set
			{
				_InvCuryID = value;
			}
		}
		#endregion

		#region InvCuryInfoID
		public abstract class invCuryInfoID : PX.Data.BQL.BqlLong.Field<invCuryInfoID> { }
		[PXDBLong(BqlField = typeof(AR.ARInvoice.curyInfoID))]
		[CurrencyInfo(ModuleCode = nameof(AR), CuryIDField = nameof(InvCuryID))]
		public virtual long? InvCuryInfoID { get; set; }
		#endregion

		#region InvCustomerLocationID
		public abstract class invCustomerLocationID : PX.Data.BQL.BqlInt.Field<invCustomerLocationID> { }
		protected int? _InvCustomerLocationID;
		[PXDBInt(BqlField = typeof(AR.ARInvoice.customerLocationID))]
		public virtual int? InvCustomerLocationID
		{
			get
			{
				return _InvCustomerLocationID;
			}
			set
			{
				_InvCustomerLocationID = value;
			}
		}
		#endregion
		#region InvTaxZoneID
		public abstract class invTaxZoneID : PX.Data.BQL.BqlString.Field<invTaxZoneID> { }
		protected string _InvTaxZoneID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(AR.ARInvoice.taxZoneID))]
		public virtual string InvTaxZoneID
		{
			get
			{
				return _InvTaxZoneID;
			}
			set
			{
				_InvTaxZoneID = value;
			}
		}
		#endregion
		#region InvTaxCalcMode
		public abstract class invTaxCalcMode : PX.Data.BQL.BqlString.Field<invTaxCalcMode> { }
		[PXDBString(1, IsFixed = true, BqlField = typeof(AR.ARInvoice.taxCalcMode))]
		public virtual string InvTaxCalcMode { get; set; }
		#endregion
		#region InvTermsID
		public abstract class invTermsID : PX.Data.BQL.BqlString.Field<invTermsID> { }
		protected string _InvTermsID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(AR.ARInvoice.termsID))]
		[PXUIField(DisplayName = "Credit Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[Terms(typeof(AR.ARInvoice.docDate), typeof(AR.ARInvoice.dueDate), typeof(AR.ARInvoice.discDate), typeof(AR.ARInvoice.curyOrigDocAmt), typeof(AR.ARInvoice.curyOrigDiscAmt))]
		public virtual string InvTermsID
		{
			get
			{
				return _InvTermsID;
			}
			set
			{
				_InvTermsID = value;
			}
		}
		#endregion
		#region InvCuryOrigDocAmt
		public abstract class invCuryOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<invCuryOrigDocAmt> { }
		protected decimal? _InvCuryOrigDocAmt;
		[PXDBCurrency(typeof(AR.ARInvoice.curyInfoID), typeof(AR.ARInvoice.origDocAmt), BqlField = typeof(AR.ARInvoice.curyOrigDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? InvCuryOrigDocAmt
		{
			get
			{
				return _InvCuryOrigDocAmt;
			}
			set
			{
				_InvCuryOrigDocAmt = value;
			}
		}
		#endregion
		#region InvCuryOrigDiscAmt
		public abstract class invCuryOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<invCuryOrigDiscAmt> { }
		protected decimal? _InvCuryOrigDiscAmt;
		[PXDBCurrency(typeof(AR.ARInvoice.curyInfoID), typeof(AR.ARInvoice.origDiscAmt), BqlField = typeof(AR.ARInvoice.curyOrigDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? InvCuryOrigDiscAmt
		{
			get
			{
				return _InvCuryOrigDiscAmt;
			}
			set
			{
				_InvCuryOrigDiscAmt = value;
			}
		}
		#endregion
		#region InvCuryVatTaxableTotal
		public abstract class invCuryVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<invCuryVatTaxableTotal> { }
		protected decimal? _InvCuryVatTaxableTotal;
		[PXDBCurrency(typeof(AR.ARInvoice.curyInfoID), typeof(AR.ARInvoice.vatTaxableTotal), BqlField = typeof(AR.ARInvoice.curyVatTaxableTotal))]
		[PXUIField(DisplayName = "VAT Taxable Total", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? InvCuryVatTaxableTotal
		{
			get
			{
				return _InvCuryVatTaxableTotal;
			}
			set
			{
				_InvCuryVatTaxableTotal = value;
			}
		}
		#endregion
		#region InvCuryTaxTotal
		public abstract class invCuryTaxTotal : PX.Data.BQL.BqlDecimal.Field<invCuryTaxTotal> { }
		protected decimal? _InvCuryTaxTotal;
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(AR.ARInvoice.taxTotal), BqlField = typeof(AR.ARInvoice.curyTaxTotal))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? InvCuryTaxTotal
		{
			get
			{
				return _InvCuryTaxTotal;
			}
			set
			{
				_InvCuryTaxTotal = value;
			}
		}
		#endregion
		#region InvCuryDocBal
		public abstract class invCuryDocBal : PX.Data.BQL.BqlDecimal.Field<invCuryDocBal> { }
		protected decimal? _InvCuryDocBal;
		[PXDBCurrency(typeof(AR.ARInvoice.curyInfoID), typeof(AR.ARInvoice.docBal), BaseCalc = false, BqlField = typeof(AR.ARInvoice.curyDocBal))]
		public virtual decimal? InvCuryDocBal
		{
			get
			{
				return _InvCuryDocBal;
			}
			set
			{
				_InvCuryDocBal = value;
			}
		}
		#endregion
		
		#endregion
	}

	[Obsolete(nameof(PPDApplicationKey) + "is used instead of this class now")]
	public class PPDCreditMemoKey
	{
		private readonly FieldInfo[] _fields;
		
		public int? BranchID;
		public int? CustomerID;
		public int? CustomerLocationID;
		public string CuryID;
		public decimal? CuryRate;
		public int? ARAccountID;
		public int? ARSubID;
		public string TaxZoneID;

		public PPDCreditMemoKey()
		{
			_fields = GetType().GetFields();
		}

		public override bool Equals(object obj)
		{
			FieldInfo info = _fields.FirstOrDefault(field => !Equals(field.GetValue(this), field.GetValue(obj)));
			return info == null;
		}
		public override int GetHashCode()
		{
			int hashCode = 17;
			_fields.ForEach(field => hashCode = hashCode * 23 + field.GetValue(this).GetHashCode());
			return hashCode;
		}
	}

	[TableAndChartDashboardType]
	public class ARPPDCreditMemoProcess : PXGraph<ARPPDCreditMemoProcess>
	{
		public PXCancel<ARPPDCreditMemoParameters> Cancel;
		public PXFilter<ARPPDCreditMemoParameters> Filter;
		
		[PXFilterable]
		public PXFilteredProcessing<PendingPPDCreditMemoApp, ARPPDCreditMemoParameters> Applications;
		public ARSetupNoMigrationMode arsetup;
        
        public override bool IsDirty
		{
			get { return false; }
        }

	    //public override bool IsProcessing
	    //{
	    //    get { return false;}
	    //    set { }
	    //}

		#region Cache Attached
		[Customer]
		protected virtual void PendingPPDCreditMemoApp_AdjdCustomerID_CacheAttached(PXCache sender) { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[ARInvoiceType.RefNbr(typeof(Search2<
			Standalone.ARRegisterAlias.refNbr,
				InnerJoinSingleTable<ARInvoice, 
					On<ARInvoice.docType, Equal<Standalone.ARRegisterAlias.docType>,
					And<ARInvoice.refNbr, Equal<Standalone.ARRegisterAlias.refNbr>>>,
				InnerJoinSingleTable<Customer, 
					On<Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>,
			Where<
				Standalone.ARRegisterAlias.docType, Equal<Optional<PendingPPDCreditMemoApp.invDocType>>,
				And2<Where<
					Standalone.ARRegisterAlias.origModule, Equal<BatchModule.moduleAR>, 
					Or<Standalone.ARRegisterAlias.released, Equal<True>>>,
				And<Match<Customer, Current<AccessInfo.userName>>>>>, 
			OrderBy<Desc<Standalone.ARRegisterAlias.refNbr>>>))]
		[ARInvoiceType.Numbering]
		[ARInvoiceNbr]
		[PXFieldDescription]
		protected virtual void PendingPPDCreditMemoApp_AdjdRefNbr_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Doc. Date")]
		protected virtual void PendingPPDCreditMemoApp_AdjdDocDate_CacheAttached(PXCache sender) { }

		[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjPPDAmt))]
		[PXUIField(DisplayName = "Cash Discount")]
		protected virtual void PendingPPDCreditMemoApp_CuryAdjdPPDAmt_CacheAttached(PXCache sender) { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Payment Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[ARPaymentType.RefNbr(typeof(Search2<
			Standalone.ARRegisterAlias.refNbr,
				InnerJoinSingleTable<ARPayment, 
					On<ARPayment.docType, Equal<Standalone.ARRegisterAlias.docType>,
					And<ARPayment.refNbr, Equal<Standalone.ARRegisterAlias.refNbr>>>,
				InnerJoinSingleTable<Customer, 
					On<Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>,
			Where<
				Standalone.ARRegisterAlias.docType, Equal<Current<PendingPPDCreditMemoApp.payDocType>>,
				And<Match<Customer, Current<AccessInfo.userName>>>>,
			OrderBy<Desc<Standalone.ARRegisterAlias.refNbr>>>))]
		[ARPaymentType.Numbering]
		[PXFieldDescription]
		protected virtual void PendingPPDCreditMemoApp_AdjgRefNbr_CacheAttached(PXCache sender) { }

		#endregion

		public ARPPDCreditMemoProcess()
		{
			Applications.AllowDelete = true;
			Applications.AllowInsert = false;
			Applications.SetSelected<PendingPPDCreditMemoApp.selected>();
		}

		public virtual IEnumerable applications(PXAdapter adapter)
		{
			ARPPDCreditMemoParameters filter = Filter.Current;
			if (filter == null || filter.ApplicationDate == null || filter.BranchID == null) yield break;

			PXSelectBase<PendingPPDCreditMemoApp> select = new PXSelect<PendingPPDCreditMemoApp,
				Where<PendingPPDCreditMemoApp.adjgDocDate, LessEqual<Current<ARPPDCreditMemoParameters.applicationDate>>,
					And<PendingPPDCreditMemoApp.adjdBranchID, Equal<Current<ARPPDCreditMemoParameters.branchID>>>>>(this);

			if (filter.CustomerID != null)
			{
				select.WhereAnd<Where<PendingPPDCreditMemoApp.customerID, Equal<Current<ARPPDCreditMemoParameters.customerID>>>>();
			}

			foreach (PendingPPDCreditMemoApp res in select.Select())
			{
				yield return res;
			}

			Filter.Cache.IsDirty = false;
		}

		protected virtual void ARPPDCreditMemoParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARPPDCreditMemoParameters filter = (ARPPDCreditMemoParameters)e.Row;
			if (filter == null) return;

			ARSetup setup = arsetup.Current;
			Applications.SetProcessDelegate(list => CreatePPDCreditMemos(sender, filter, setup, list));

			bool generateOnePerCustomer = filter.GenerateOnePerCustomer == true;
			PXUIFieldAttribute.SetEnabled<ARPPDCreditMemoParameters.creditMemoDate>(sender, filter, generateOnePerCustomer);
			PXUIFieldAttribute.SetEnabled<ARPPDCreditMemoParameters.finPeriodID>(sender, filter, generateOnePerCustomer);
			PXUIFieldAttribute.SetRequired<ARPPDCreditMemoParameters.creditMemoDate>(sender, generateOnePerCustomer);
			PXUIFieldAttribute.SetRequired<ARPPDCreditMemoParameters.finPeriodID>(sender, generateOnePerCustomer);
		}

		public virtual void ARPPDCreditMemoParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARPPDCreditMemoParameters row = (ARPPDCreditMemoParameters)e.Row;
			ARPPDCreditMemoParameters oldRow = (ARPPDCreditMemoParameters)e.OldRow;
			if (row == null || oldRow == null) return;

			if (!sender.ObjectsEqual<ARPPDCreditMemoParameters.applicationDate, ARPPDCreditMemoParameters.branchID, ARPPDCreditMemoParameters.customerID>(oldRow, row))
			{
				Applications.Cache.Clear();
				Applications.Cache.ClearQueryCacheObsolete();
			}
		}

		protected virtual void ARPPDCreditMemoParameters_GenerateOnePerCustomer_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARPPDCreditMemoParameters filter = (ARPPDCreditMemoParameters)e.Row;
			if (filter == null) return;

			if (filter.GenerateOnePerCustomer != true && (bool?)e.OldValue == true)
			{
				filter.CreditMemoDate = null;
				filter.FinPeriodID = null;
				
				sender.SetValuePending<ARPPDCreditMemoParameters.creditMemoDate>(filter, null);
				sender.SetValuePending<ARPPDCreditMemoParameters.finPeriodID>(filter, null);
			}
		}

		public static void CreatePPDCreditMemos(PXCache cache, ARPPDCreditMemoParameters filter, ARSetup setup, List<PendingPPDCreditMemoApp> docs)
		{
			int i = 0;
			bool failed = false;

			List<ARRegister> toRelease = new List<ARRegister>();
			ARInvoiceEntry ie = PXGraph.CreateInstance<ARInvoiceEntry>();
			ie.ARSetup.Current = setup;

			if (filter.GenerateOnePerCustomer == true)
			{
				if (filter.CreditMemoDate == null)
					throw new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat,
						PXUIFieldAttribute.GetDisplayName<ARPPDCreditMemoParameters.creditMemoDate>(cache));

				if (filter.FinPeriodID == null)
					throw new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat,
						PXUIFieldAttribute.GetDisplayName<ARPPDCreditMemoParameters.finPeriodID>(cache));

				Dictionary<PPDApplicationKey, List<PendingPPDCreditMemoApp>> dict = new Dictionary<PPDApplicationKey, List<PendingPPDCreditMemoApp>>();
				foreach (PendingPPDCreditMemoApp doc in docs)
				{
					CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(ie, doc.InvCuryInfoID);

					PPDApplicationKey key = new PPDApplicationKey();
					doc.Index = i++;
					key.BranchID = doc.AdjdBranchID;
					key.BAccountID = doc.AdjdCustomerID;
					key.LocationID = doc.InvCustomerLocationID;
					key.CuryID = info.CuryID;
					key.CuryRate = info.CuryRate;
					key.AccountID = doc.AdjdARAcct;
					key.SubID = doc.AdjdARSub;
					key.TaxZoneID = doc.InvTaxZoneID;

					List<PendingPPDCreditMemoApp> list;
					if (!dict.TryGetValue(key, out list))
					{
						dict[key] = list = new List<PendingPPDCreditMemoApp>();
					}

					list.Add(doc);
				}

				foreach (List<PendingPPDCreditMemoApp> list in dict.Values)
				{
					ARInvoice invoice = CreatePPDCreditMemo(ie, filter, list);
					if (invoice != null) { toRelease.Add(invoice); }
					else failed = true;
				}
			}
			else foreach (PendingPPDCreditMemoApp doc in docs)
			{
				List<PendingPPDCreditMemoApp> list = new List<PendingPPDCreditMemoApp>(1);
				doc.Index = i++;
				list.Add(doc);

				ARInvoice invoice = CreatePPDCreditMemo(ie, filter, list);
				if (invoice != null) { toRelease.Add(invoice); }
				else failed = true;
			}

			if (setup.AutoReleasePPDCreditMemo == true && toRelease.Count > 0)
			{
				using (new PXTimeStampScope(null))
				{
					ARDocumentRelease.ReleaseDoc(toRelease, true);
				}
			}

			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
		}

		private static ARInvoice CreatePPDCreditMemo(ARInvoiceEntry ie, ARPPDCreditMemoParameters filter, List<PendingPPDCreditMemoApp> list)
		{
			int index = 0;
			ARInvoice invoice;

			try
			{
				ie.Clear(PXClearOption.ClearAll);
				PXUIFieldAttribute.SetError(ie.Document.Cache, null, null, null);
				
				invoice = ie.CreatePPDCreditMemo(filter, list, ref index);

				PXProcessing<PendingPPDCreditMemoApp>.SetInfo(index, ActionsMessages.RecordProcessed);
			}
			catch (Exception e)
			{
				PXProcessing<PendingPPDCreditMemoApp>.SetError(index, e);
				invoice = null;
			}

			return invoice;
		}

		public static bool CalculateDiscountedTaxes(PXCache cache, ARTaxTran artax, decimal cashDiscPercent)
		{
			bool? result = null;
			object value = null;

			IBqlCreator whereTaxable = (IBqlCreator)Activator.CreateInstance(typeof(WhereTaxable<Required<ARTaxTran.taxID>>));
			whereTaxable.Verify(cache, artax, new List<object> { artax.TaxID }, ref result, ref value);
			
			artax.CuryDiscountedTaxableAmt = cashDiscPercent == 0m
				? artax.CuryTaxableAmt
				: PXDBCurrencyAttribute.RoundCury(cache, artax, 
					(decimal)(artax.CuryTaxableAmt * (1m - cashDiscPercent)));

			artax.CuryDiscountedPrice = cashDiscPercent == 0m
				? artax.CuryTaxAmt
				: PXDBCurrencyAttribute.RoundCury(cache, artax, 
					(decimal)(artax.TaxRate / 100m * artax.CuryDiscountedTaxableAmt));

			return result == true;
		}
	}
}
