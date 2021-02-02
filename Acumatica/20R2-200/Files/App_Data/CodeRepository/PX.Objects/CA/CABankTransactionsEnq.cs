using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.GL;


namespace PX.Objects.CA
{
    public class CABankTransactionsEnq : PXGraph<CABankTransactionsEnq>
    {
        #region Internal Classes definitions
        [Serializable]
        public partial class Filter : IBqlTable
        {
            #region StartDate
            public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
            protected DateTime? _StartDate;
            [PXDBDate()]
            [PXDefault()]
            [PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
            public virtual DateTime? StartDate
            {
                get
                {
                    return this._StartDate;
                }
                set
                {
                    this._StartDate = value;
                }
            }
            #endregion
            #region CashAccountID
            public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
            protected Int32? _CashAccountID;
            [CashAccount()]
            [PXDefault]
            public virtual int? CashAccountID
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
            #region EndDate
            public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
            protected DateTime? _EndDate;
            [PXDBDate()]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "To Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
            public virtual DateTime? EndDate
            {
                get
                {
                    return this._EndDate;
                }
                set
                {
                    this._EndDate = value;
                }
            }
            #endregion
            #region HeaderRefNbr
            public abstract class headerRefNbr : PX.Data.BQL.BqlString.Field<headerRefNbr> { }
            protected String _HeaderRefNbr;
            [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
            [PXSelector(typeof(Search<CABankTranHeader.refNbr, Where<CABankTranHeader.cashAccountID, Equal<Current<Filter.cashAccountID>>,
                                                                And<CABankTranHeader.tranType, Equal<Current<Filter.tranType>>>>>),
                                                                typeof(CABankTranHeader.docDate))]
            [PXUIField(DisplayName = "Statement Nbr.")]
            public virtual String HeaderRefNbr
            {
                get
                {
                    return this._HeaderRefNbr;
                }
                set
                {
                    this._HeaderRefNbr = value;
                }
            }
            #endregion
            #region TranType
            public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
            protected String _TranType;
            [PXString(1, IsFixed = true)]
            [PXDefault(typeof(CABankTranType.statement))]
            [CABankTranType.List()]
            [PXUIField(DisplayName = "Statement Type", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String TranType
            {
                get
                {
                    return this._TranType;
                }
                set
                {
                    this._TranType = value;
                }
            }
            #endregion
        }

		[PXHidden]
		public class CABankTranExt: CABankTran
	    {
		    public new abstract class tranID : PX.Data.BQL.BqlInt.Field<tranID> { }
		    public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
			public new abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		    public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		    public new abstract class processed : PX.Data.BQL.BqlBool.Field<processed> { }
		    public new abstract class headerRefNbr : PX.Data.BQL.BqlString.Field<headerRefNbr> { }

			#region MatchedModule
			public abstract class matchedModule : PX.Data.BQL.BqlString.Field<matchedModule>
		    {
		    }
		    [PXString(2, IsFixed = true)]
		    [PXUIField(DisplayName = "Module")]
		    [BatchModule.FullList]
			public virtual string MatchedModule
		    {
			    get;
			    set;
		    }
			#endregion
			#region MatchedDocType
			public abstract class matchedDocType : PX.Data.BQL.BqlString.Field<matchedDocType>
		    {
		    }
		    [PXString(3, IsFixed = true)]
		    [PXUIField(DisplayName = Messages.Type)]
			[CAAPARTranType.ListByModule(typeof(matchedModule))]
			public virtual string MatchedDocType
			{
			    get;
			    set;
		    }
			#endregion
			#region MatchedRefNbr
			public abstract class matchedRefNbr : PX.Data.BQL.BqlString.Field<matchedRefNbr>
		    {
		    }
		    [PXString(15, IsUnicode = true)]
		    [PXUIField(DisplayName = Messages.RefNbr)]
		    public virtual string MatchedRefNbr
			{
			    get;
			    set;
		    }
			#endregion
		    #region MatchedReferenceID
		    public abstract class matchedReferenceID : PX.Data.BQL.BqlInt.Field<matchedReferenceID>
		    {
		    }
		    [PXInt]
		    [PXSelector(typeof(BAccountR.bAccountID),
			    SubstituteKey = typeof(BAccountR.acctCD),
			    DescriptionField = typeof(BAccountR.acctName))]
		    [PXUIField(DisplayName = "Business Account")]
		    public virtual int? MatchedReferenceID
			{
			    get;
			    set;
		    }
		    #endregion
		}

		#endregion
		#region Selects
		public PXFilter<Filter> TranFilter;
        public PXSelect<CABankTranExt> Result;
        public PXSelect<CATran> Trans;
		public PXSelect<CR.BAccountR> BAccountCache;
        #endregion

        public CABankTransactionsEnq()
        {
            Result.AllowDelete = false;
            Result.AllowInsert = false;
            Result.AllowUpdate = false;

			PXUIFieldAttribute.SetVisible<CABankTran.invoiceInfo>(Result.Cache, null, true);
			PXUIFieldAttribute.SetVisible<CABankTran.entryTypeID>(Result.Cache, null, true);
			PXUIFieldAttribute.SetVisible<CABankTran.status>(Result.Cache, null, true);

        }
        protected virtual IEnumerable result()
        {
            PXSelectBase<CABankTranExt> cmd = new PXSelectJoin<CABankTranExt, LeftJoin<CABankTranMatch,
                     On<CABankTranMatch.tranID, Equal<CABankTranExt.tranID>,
                        And<CABankTranMatch.tranType, Equal<CABankTranExt.tranType>>>,
                        LeftJoin<CATran, On<CATran.tranID, Equal<CABankTranMatch.cATranID>>>>,
                     Where<CABankTranExt.cashAccountID, Equal<Current<Filter.cashAccountID>>,
                        And<CABankTranExt.tranDate, GreaterEqual<Current<Filter.startDate>>,
                        And<CABankTranExt.tranDate, LessEqual<Current<Filter.endDate>>,
                        And<CABankTranExt.tranType, Equal<Current<Filter.tranType>>,
                        And<CABankTranExt.processed, Equal<True>,
                        And<Where<CABankTranExt.headerRefNbr, Equal<Current<Filter.headerRefNbr>>, Or<Current<Filter.headerRefNbr>, IsNull>>>>>>>>>(this);
            foreach (PXResult<CABankTranExt, CABankTranMatch, CATran> res in cmd.Select())
            {
	            CABankTranExt tran = (CABankTranExt)res;
				CABankTranMatch match = (CABankTranMatch)res;
				tran.MatchedToInvoice = CABankTransactionsMaint.IsMatchedToInvoice(tran, match);
	            tran.MatchedToExpenseReceipt = CABankTransactionsMaint.IsMatchedToExpenseReceipt(match);
				CATran catran = (CATran)res;
				if (catran.OrigModule == null)
				{
					catran.OrigModule = match.DocModule;
				}
				if (catran.OrigTranType == null)
				{
					catran.OrigTranType = match.DocType;
				}
				if (catran.OrigRefNbr == null)
				{
					catran.OrigRefNbr = match.DocRefNbr;
				}
				if (catran.ReferenceID == null)
				{
					catran.ReferenceID = tran.BAccountID;
				}
				if (catran.OrigModule == null)
				{
					catran.OrigModule = match.DocModule;
				}
				if (catran.OrigTranType == null)
				{
					catran.OrigTranType = match.DocType;
				}
				if (catran.OrigRefNbr == null)
				{
					catran.OrigRefNbr = match.DocRefNbr;
				}
				if (catran.ReferenceID == null)
				{
					catran.ReferenceID = tran.BAccountID;
				}

	            if (tran.MatchedToExpenseReceipt == true && match.CATranID == null)
	            {
		            tran.MatchedModule = match.DocModule;
		            tran.MatchedDocType = match.DocType;
		            tran.MatchedRefNbr = match.DocRefNbr;
		            tran.MatchedReferenceID = match.ReferenceID;
				}
	            else
	            {
		            tran.MatchedModule = catran.OrigModule;
					tran.MatchedDocType = catran.OrigTranType;
		            tran.MatchedRefNbr = catran.OrigRefNbr;
		            tran.MatchedReferenceID = catran.ReferenceID;
				}

                yield return res;
            }
        }
        #region Actions
        public PXAction<Filter> viewDoc;
        [PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable ViewDoc(PXAdapter adapter)
        {
            CABankTranMatch match = 
	            PXSelect<CABankTranMatch, 
		            Where<CABankTranMatch.tranID, Equal<Required<CABankTran.tranID>>,
                        And<CABankTranMatch.tranType, Equal<Required<CABankTran.tranType>>>>>
		        .Select(this, Result.Current.TranID, Result.Current.TranType);

	        if (match.DocModule == BatchModule.EP)
	        {
		        RedirectionToOrigDoc.TryRedirect(match.DocType, match.DocRefNbr, match.DocModule);
	        }
	        else
	        {
		        CATran tran = null;
		        if (match.DocModule == BatchModule.AP && match.DocType == CATranType.CABatch)
		        {
			        tran = new CATran()
			        {
				        OrigTranType = CATranType.CABatch,
				        OrigModule = BatchModule.AP,
				        OrigRefNbr = match.DocRefNbr
			        };
		        }
		        else
		        {
			        tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CABankTranMatch.cATranID>>>>.Select(this, match.CATranID);
		        }


		        CATran.Redirect(TranFilter.Cache, tran);
			}
            return adapter.Get();
        }
        public PXAction<Filter> viewStatement;
		[PXUIField(DisplayName = "View Statement", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewStatement(PXAdapter adapter)
		{
			CABankTranHeader header = PXSelect<CABankTranHeader,
					Where<CABankTranHeader.cashAccountID, Equal<Current<Filter.cashAccountID>>,
					And<CABankTranHeader.refNbr, Equal<Current<CABankTran.headerRefNbr>>,
					And<CABankTranHeader.tranType, Equal<Current<CABankTran.tranType>>>>>>.Select(this);
			CABankTransactionsImport graph;
			if (Result.Current.TranType == CABankTranType.PaymentImport)
			{
				graph = PXGraph.CreateInstance<CABankTransactionsImportPayments>();
			}
			else
			{
				graph = PXGraph.CreateInstance<CABankTransactionsImport>();
			}
			graph.Header.Current = header;
			graph.SelectedDetail.Current = graph.SelectedDetail.Search<CABankTran.tranID>(Result.Current.TranID);
			throw new PXRedirectRequiredException(graph, true, "Import") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}
		#endregion

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "DisplayName", CR.Messages.BAccountName)]
		protected virtual void BAccountR_AcctName_CacheAttached(PXCache sender) { }

	}
    public class CABankTransactionsEnqPayments : CABankTransactionsEnq
    {
        [PXString(1, IsFixed = true)]
        [PXDefault(typeof(CABankTranType.paymentImport))]
        [CABankTranType.List()]
        [PXUIField(DisplayName = "Statement Type", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void Filter_TranType_CacheAttached(PXCache sender) { }
    }
}
