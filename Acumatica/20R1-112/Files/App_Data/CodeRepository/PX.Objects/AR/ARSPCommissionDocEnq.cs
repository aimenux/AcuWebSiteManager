using System;
using System.Collections.Generic;
using System.Collections;
using PX.Data;
using PX.Objects.AR.Standalone;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CM;

namespace PX.Objects.AR
{
	[Serializable]
	public partial class ARSPCommnDocResult : IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(3, IsKey = true)]
		[PXUIField(DisplayName = "Doc. Type")]
		[ARDocType.List()]
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
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
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
		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		protected String _AdjdDocType;
		[PXDBString(3, IsKey = true)]
		[PXUIField(DisplayName = "Original Doc. Type")]
		[ARDocType.List()]
		public virtual String AdjdDocType
		{
			get
			{
				return this._AdjdDocType;
			}
			set
			{
				this._AdjdDocType = value;
			}
		}
		#endregion
		#region AdjdRefNbr
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
		protected String _AdjdRefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Original Ref. Nbr.")]
		public virtual String AdjdRefNbr
		{
			get
			{
				return this._AdjdRefNbr;
			}
			set
			{
				this._AdjdRefNbr = value;
			}
		}
		#endregion
		#region AdjNbr
		public abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		protected Int32? _AdjNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		public virtual Int32? AdjNbr
		{
			get
			{
				return this._AdjNbr;
			}
			set
			{
				this._AdjNbr = value;
			}
		}
		#endregion
		#region CommnPct
		public abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }
		protected Decimal? _CommnPct;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commission %")]
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
		#region CommnAmt
		public abstract class commnAmt : PX.Data.BQL.BqlDecimal.Field<commnAmt> { }
		protected Decimal? _CommnAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commission Amount")]
		public virtual Decimal? CommnAmt
		{
			get
			{
				return this._CommnAmt;
			}
			set
			{
				this._CommnAmt = value;
			}
		}
		#endregion
		#region CommnblAmt
		public abstract class commnblAmt : PX.Data.BQL.BqlDecimal.Field<commnblAmt> { }
		protected Decimal? _CommnblAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commissionable Amount")]
		public virtual Decimal? CommnblAmt
		{
			get
			{
				return this._CommnblAmt;
			}
			set
			{
				this._CommnblAmt = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), DisplayName = "Customer ID")]
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
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<ARSPCommnDocResult.customerID>>>), DescriptionField = typeof(Location.descr))]
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
		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		protected Decimal? _OrigDocAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Doc. Amount")]
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
	}

	[Serializable]
	public partial class SPDocFilter : IBqlTable
	{
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		[SalesPerson(DescriptionField = typeof(SalesPerson.descr))]
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
		#region Period
		public abstract class commnPeriod : PX.Data.BQL.BqlString.Field<commnPeriod> { }
		protected String _CommnPeriod;
		[PXDefault]
		[ARCommissionPeriodID()]
		[PXUIField(DisplayName = "Commission Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CommnPeriod
		{
			get
			{
				return this._CommnPeriod;
			}
			set
			{
				this._CommnPeriod = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
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
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<SPDocFilter.customerID>>>), DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
	}

    [PX.Objects.GL.TableAndChartDashboardType]
	public class ARSPCommissionDocEnq : PXGraph<ARSPCommissionDocEnq>
	{
		#region Ctor + overrides
		public ARSPCommissionDocEnq() 
		{
			ARSetup setup = ARSetup.Current;
			this.SPDocs.Cache.AllowDelete = false;
			this.SPDocs.Cache.AllowUpdate = false;
			this.SPDocs.Cache.AllowInsert = false;
		}

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region PublicMembers
		public PXFilter<SPDocFilter> Filter;
		public PXCancel<SPDocFilter> Cancel;
		[PXFilterable]
		public PXSelect<ARSPCommnDocResult> SPDocs;
		#endregion

        #region Actions
        public PXAction<SPDocFilter> viewDocument;
        [PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            if (SPDocs.Current != null)
                RedirectToDoc(SPDocs.Current.DocType, SPDocs.Current.RefNbr);
            return Filter.Select();
        }

        public PXAction<SPDocFilter> viewOrigDocument;
        [PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewOrigDocument(PXAdapter adapter)
        {
            if (SPDocs.Current != null)
            {
                ARSPCommnDocResult current = SPDocs.Current;

                if (current.DocType == ARDocType.CreditMemo && string.IsNullOrEmpty(current.AdjdDocType))
                {
                    RedirectToDoc(current.DocType, current.RefNbr);
                }

                if (string.IsNullOrEmpty(current.AdjdDocType) == false)
                {
                    RedirectToDoc(current.AdjdDocType, current.AdjdRefNbr);
                }
                throw new PXException(Messages.OriginalDocumentIsNotSet);

            }
            return Filter.Select();
        }

		#endregion

		#region Delegates
		protected virtual IEnumerable spdocs()
		{
			List<ARSPCommnDocResult> res = new List<ARSPCommnDocResult>();
			SPDocFilter filter = Filter.Current;

			if (filter?.CommnPeriod != null)
			{
				PXSelectBase<ARSalesPerTran> sel = new PXSelectJoin<ARSalesPerTran,
					InnerJoin<ARRegister, On<ARSalesPerTran.docType, Equal<ARRegister.docType>,
						And<ARSalesPerTran.refNbr, Equal<ARRegister.refNbr>>>,
						InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ARRegister.customerID>,
						And<Match<Customer, Current<AccessInfo.userName>>>>>>,
							Where<ARSalesPerTran.actuallyUsed, Equal<BQLConstants.BitOn>>>(this);

				if (filter.SalesPersonID != null)
				{
					sel.WhereAnd<Where<ARSalesPerTran.salespersonID, Equal<Current<SPDocFilter.salesPersonID>>>>();
				}
				if (filter.CommnPeriod != null)
				{
					sel.WhereAnd<Where<ARSalesPerTran.commnPaymntPeriod, Equal<Current<SPDocFilter.commnPeriod>>>>();
				}
				if (filter.CustomerID != null)
				{
					sel.WhereAnd<Where<ARRegister.customerID, Equal<Current<SPDocFilter.customerID>>>>();
				}
				if (filter.LocationID != null)
				{
					sel.WhereAnd<Where<ARRegister.customerLocationID, Equal<Current<SPDocFilter.locationID>>>>();
				}

				PXView view = sel.View;
				int startRow = PXView.StartRow;
				int totalRows = 0;
				List<object> windowedSelection = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
				PXView.StartRow = 0;

				foreach (PXResult<ARSalesPerTran, ARRegister, Customer> it in windowedSelection)
				{
					ARSPCommnDocResult m = new ARSPCommnDocResult();
					Copy(m, ((ARSalesPerTran)it));
					Copy(m, ((ARRegister)it));
					res.Add(m);
				}
			}
			return res;
		}

		#endregion

		#region Filter Events
		public virtual void SPDocFilter_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			SPDocFilter row = (SPDocFilter)e.Row;
			//row.LocationID = null;
			cache.SetDefaultExt<SPDocFilter.locationID>(e.Row);
			cache.SetValuePending<SPDocFilter.locationID>(e.Row, null);

		}
		public virtual void SPDocFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            SPDocFilter row = (SPDocFilter)e.Row;
			//PXUIFieldAttribute.SetEnabled<SPDocFilter.locationID>(cache, row, (row.CustomerID.HasValue));
		}
		#endregion

		#region Utility Functions
		protected virtual void Copy(ARSPCommnDocResult aDst, ARSalesPerTran aSrc) 
		{
			aDst.RefNbr = aSrc.RefNbr;
			aDst.DocType = aSrc.DocType;
			aDst.AdjdDocType = (aSrc.AdjdDocType!= ARDocType.Undefined)? aSrc.AdjdDocType: String.Empty;
			aDst.AdjdRefNbr = aSrc.AdjdRefNbr;
			aDst.AdjNbr = aSrc.AdjNbr;
			aDst.CommnPct = aSrc.CommnPct;
			aDst.CommnAmt = aSrc.CommnAmt;
			aDst.CommnblAmt = aSrc.CommnblAmt;
		}
		protected virtual void Copy(ARSPCommnDocResult aDst, ARRegister aSrc)
		{
			aDst.CustomerID = aSrc.CustomerID;
			aDst.CustomerLocationID = aSrc.CustomerLocationID;
			aDst.OrigDocAmt = aSrc.OrigDocAmt;		
		}

		protected virtual void RedirectToDoc(string aDocType, string aRefNbr)
		{
			Dictionary<string, string> alltypes = new ARDocType.ListAttribute().ValueLabelDic;
			if(new ARInvoiceType.ListAttribute().ValueLabelDic.ContainsKey(aDocType))
			{
				ARInvoice doc = ARDocumentEnq.FindDoc<ARInvoice>(this, aDocType, aRefNbr);
				if (doc != null)
				{
					ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
					graph.Document.Current = doc;
					throw new PXRedirectRequiredException(graph, true, "Document"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
				}
				throw new PXException(Messages.DocumentNotFound, alltypes[aDocType], aRefNbr);
			}
			if (new ARPaymentType.ListAttribute().ValueLabelDic.ContainsKey(aDocType))
			{
				ARPayment doc = ARDocumentEnq.FindDoc<ARPayment>(this, aDocType, aRefNbr);
				if (doc != null)
				{
					ARPaymentEntry graph = PXGraph.CreateInstance<ARPaymentEntry>();
					graph.Document.Current = doc;
					throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				throw new PXException(Messages.DocumentNotFound, alltypes[aDocType], aRefNbr);
			}
			if (new ARCashSaleType.ListAttribute().ValueLabelDic.ContainsKey(aDocType))
			{
				ARCashSale doc = ARDocumentEnq.FindDoc<ARCashSale>(this, aDocType, aRefNbr);
				if (doc != null)
				{
					ARCashSaleEntry graph = PXGraph.CreateInstance<ARCashSaleEntry>();
					graph.Document.Current = doc;
					throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				throw new PXException(Messages.DocumentNotFound, alltypes[aDocType], aRefNbr);
			}
			throw new PXException(Messages.UnknownDocumentType);
		}
		#endregion

		public PXSetup<ARSetup> ARSetup;
	}
}
