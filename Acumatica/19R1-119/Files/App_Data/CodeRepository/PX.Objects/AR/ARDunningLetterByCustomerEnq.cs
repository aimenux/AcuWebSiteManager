using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace PX.Objects.AR
{
    public class ARDunningLetterByCustomerEnq : PXGraph<ARDunningLetterByCustomerEnq>
    {

        #region Internal Types
        [Serializable]
        public partial class DLByCustomerFilter : IBqlTable
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
            [Customer]
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
          
        }
        #endregion
        #region Actions
        public PXAction<DLByCustomerFilter> ViewDocument;
        [PXUIField(DisplayName = Messages.ViewDunningLetter, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable viewDocument(PXAdapter adapter)
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
        #region Member Decalaration + Ctor
        public PXFilter<DLByCustomerFilter> Filter;
        public PXCancel<DLByCustomerFilter> Cancel;

        [PXFilterable]
        public PXSelectJoinGroupBy<ARDunningLetter,
            LeftJoin<Customer, On<Customer.bAccountID, Equal<ARDunningLetter.bAccountID>>,
            LeftJoin<ARDunningLetterDetail,On<ARDunningLetterDetail.dunningLetterID,Equal<ARDunningLetter.dunningLetterID>>>>,
            Where2<Where<ARDunningLetter.bAccountID, Equal<Current<DLByCustomerFilter.bAccountID>>, Or<Current<DLByCustomerFilter.bAccountID>, IsNull>>,
                And<ARDunningLetter.dunningLetterDate, GreaterEqual<Current<DLByCustomerFilter.beginDate>>,
                And<ARDunningLetter.dunningLetterDate, LessEqual<Current<DLByCustomerFilter.endDate>>>>>,
            Aggregate<GroupBy<ARDunningLetter.dunningLetterID,Sum<ARDunningLetterDetail.overdueBal,Count<ARDunningLetterDetail.refNbr>>>>,
            OrderBy<Asc<ARDunningLetter.dunningLetterDate>>> EnqResults;

        public ARDunningLetterByCustomerEnq()
        {
            EnqResults.AllowDelete = false;
            EnqResults.AllowInsert = false;
            EnqResults.AllowUpdate = false;
        }
        #endregion
        #region Select delegate
        public virtual IEnumerable enqResults()
        {
            foreach(var result in PXSelectJoinGroupBy<ARDunningLetter,
            LeftJoin<Customer, On<Customer.bAccountID, Equal<ARDunningLetter.bAccountID>>,
            LeftJoin<ARDunningLetterDetail,On<ARDunningLetterDetail.dunningLetterID,Equal<ARDunningLetter.dunningLetterID>>>>,
            Where2<Where<ARDunningLetter.bAccountID, Equal<Current<DLByCustomerFilter.bAccountID>>, Or<Current<DLByCustomerFilter.bAccountID>, IsNull>>,
                And<ARDunningLetter.dunningLetterDate, GreaterEqual<Current<DLByCustomerFilter.beginDate>>,
                And<ARDunningLetter.dunningLetterDate, LessEqual<Current<DLByCustomerFilter.endDate>>>>>,
            Aggregate<GroupBy<ARDunningLetter.dunningLetterID, GroupBy<ARDunningLetter.released, GroupBy<ARDunningLetter.voided, Sum<ARDunningLetterDetail.overdueBal, Count<ARDunningLetterDetail.refNbr>>>>>>,
            OrderBy<Asc<ARDunningLetter.dunningLetterDate>>>.Select(this))
            {
                ARDunningLetter letter = (ARDunningLetter)result;
                letter.DetailsCount = result.RowCount;
                yield return result;
            }
        }
        #endregion
    }
}