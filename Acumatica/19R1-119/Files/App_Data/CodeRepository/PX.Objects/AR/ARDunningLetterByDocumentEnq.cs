using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace PX.Objects.AR
{
    public class ARDunningLetterByDocumentEnq : PXGraph<ARDunningLetterByDocumentEnq>
    {
		#region Internal Types
		[Serializable]
		public partial class DLByDocumentFilter: IBqlTable
		{
			#region BeginDate
			public abstract class beginDate : PX.Data.BQL.BqlDateTime.Field<beginDate> { }
			protected DateTime? _BeginDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Start Date")]
			public virtual DateTime? BeginDate
			{
				get
				{
					return this._BeginDate;
				}
				set
				{
					this._BeginDate = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
            protected DateTime? _EndDate;
			[PXDBDate()]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "End Date")]
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
            #region BAccountID
            public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            protected Int32? _BAccountID;
            [Customer()]
            public virtual Int32? BAccountID
            {
                get
                {
                    return this._BAccountID;
                }
                set
                {
                    this._BAccountID = value;
                }
            }
            #endregion
            #region LevelFrom
            public abstract class levelFrom : PX.Data.BQL.BqlInt.Field<levelFrom> { }
            protected Int32? _LevelFrom;
            [PXDBInt()]
            [PXUIField(DisplayName = "From")]
            [PXDefault(1,PersistingCheck=PXPersistingCheck.Nothing)]
            public virtual Int32? LevelFrom
            {
                get
                {
                    return this._LevelFrom;
                }
                set
                {
                    this._LevelFrom = value;
                }
            }
            #endregion
            #region LevelTo
            public abstract class levelTo : PX.Data.BQL.BqlInt.Field<levelTo> { }
            protected Int32? _LevelTo;
            [PXDBInt()]
            [PXUIField(DisplayName = "To")]
            [PXDefault(typeof(Search<ARDunningSetup.dunningLetterLevel, Where<True, Equal<True>>, OrderBy<Desc<ARDunningSetup.dunningLetterLevel>>>), PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual Int32? LevelTo
            {
                get
                {
                    return this._LevelTo;
                }
                set
                {
                    this._LevelTo = value;
                }
            }
            #endregion
		}
		#endregion
		#region Member Decalaration + Ctor
		public PXFilter<DLByDocumentFilter> Filter;
		public PXCancel<DLByDocumentFilter> Cancel;	
		
        [PXFilterable]
        public PXSelectJoin<ARDunningLetterDetail, InnerJoin<ARDunningLetter, On<ARDunningLetterDetail.dunningLetterID, Equal<ARDunningLetter.dunningLetterID>>,
        InnerJoin<ARInvoice, On<ARDunningLetterDetail.docType, Equal<ARInvoice.docType>,And<ARDunningLetterDetail.refNbr, Equal<ARInvoice.refNbr>>>>>,
            Where2<Where<ARDunningLetter.bAccountID, Equal<Current<DLByDocumentFilter.bAccountID>>, Or<Current<DLByDocumentFilter.bAccountID>,IsNull>>,
            And<ARDunningLetter.dunningLetterDate, GreaterEqual<Current<DLByDocumentFilter.beginDate>>,
            And<ARDunningLetter.dunningLetterDate, LessEqual<Current<DLByDocumentFilter.endDate>>,
            And2<Where<ARDunningLetter.dunningLetterLevel, GreaterEqual<Current<DLByDocumentFilter.levelFrom>>, 
                    Or<Current<DLByDocumentFilter.levelFrom>, IsNull>>,
            And<Where<ARDunningLetter.dunningLetterLevel, LessEqual<Current<DLByDocumentFilter.levelTo>>,
                    Or<Current<DLByDocumentFilter.levelTo>, IsNull>>>>>>>,OrderBy<Asc<ARInvoice.dueDate>>> EnqResults;
        public PXSelect<ARDunningLetter> letter;
        public PXSelect<ARInvoice> inv;

        public ARDunningLetterByDocumentEnq()
        {
            EnqResults.AllowDelete = false;
            EnqResults.AllowInsert = false;
            EnqResults.AllowUpdate = false;
        }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.DunningLetterStatus)]
		protected virtual void ARDunningLetter_Status_CacheAttached(PXCache sender) { }

		#endregion
		#region Actions
		public PXAction<DLByDocumentFilter> ViewDocument;
        [PXUIField(DisplayName = "View Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable viewDocument(PXAdapter adapter)
        {
            if (EnqResults.Current != null)
            {
                ARInvoice doc = PXSelect<ARInvoice, Where<ARInvoice.refNbr, Equal<Required<ARDunningLetterDetail.refNbr>>, And<ARInvoice.docType, Equal<Required<ARDunningLetterDetail.docType>>>>>.Select(this, EnqResults.Current.RefNbr, EnqResults.Current.DocType);
                if (doc != null)
                {
                    ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
                    graph.Document.Current = doc;
                    PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
                }
            }
            return adapter.Get();
        }
        public PXAction<DLByDocumentFilter> ViewLetter;
        [PXUIField(DisplayName = Messages.ViewDunningLetter, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable viewLetter(PXAdapter adapter)
        {
            if (EnqResults.Current != null)
            {
                var doc = PXSelect<ARDunningLetter, Where<ARDunningLetter.dunningLetterID, Equal<Required<ARDunningLetter.dunningLetterID>>>>.Select(this, EnqResults.Current.DunningLetterID);
                if (doc != null)
                {
                    ARDunningLetterMaint graph = PXGraph.CreateInstance<ARDunningLetterMaint>();
                    graph.Document.Current = doc;
                    PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
                }
            }
            return adapter.Get();
        }
        #endregion
        [PXDBDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName="Document Balance")]
         protected virtual void ARInvoice_DocBal_CacheAttached(PXCache sender) { }
    }
}