using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CM
{
	[System.SerializableAttribute()]

    public partial class TranslationEnqFilter : PX.Data.IBqlTable
    {
        #region TranslDefId
        public abstract class translDefId : PX.Data.BQL.BqlString.Field<translDefId> { }
        protected String _TranslDefId;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault()]
		[PXUIField(DisplayName = "Translation ID",Required = false)]
		[PXSelector(typeof(TranslDef.translDefId),
		 DescriptionField = typeof(TranslDef.description))]
        public virtual String TranslDefId
        {
            get
            {
                return this._TranslDefId;
            }
            set
            {
                this._TranslDefId = value;
            }
        }
        #endregion
		#region FinPeriodID
			  public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			  protected String _FinPeriodID;
        [ClosedPeriod()]
        [PXDBString(6)]
        [PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible)]
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
        #region Unreleased
        public abstract class unreleased : PX.Data.BQL.BqlBool.Field<unreleased> { }
        protected bool? _Unreleased;
        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Unreleased")]
        public bool? Unreleased
        {
            get
            {
                return _Unreleased;
            }
            set
            {
                _Unreleased = value;
            }
        }
        #endregion
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
        protected bool? _Released;
        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Released")]
        public bool? Released
        {
            get
            {
                return _Released;
            }
            set
            {
                _Released = value;
            }
        }
        #endregion
        #region Voided
        public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
        protected bool? _Voided;
        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Voided")]
        public bool? Voided
        {
            get
            {
                return _Voided;
            }
            set
            {
                _Voided = value;
            }
        }
        #endregion
    }


    public class TranslationUnReleased : PX.Data.BQL.BqlString.Constant<TranslationUnReleased>
	{
        public TranslationUnReleased() : base("U") { ;}
    }

    public class TranslationReleased : PX.Data.BQL.BqlString.Constant<TranslationReleased>
	{
        public TranslationReleased() : base("R") { ;}
    }

    public class TranslationVoided : PX.Data.BQL.BqlString.Constant<TranslationVoided>
	{
        public TranslationVoided() : base("V") { ;}
    }

	[TableAndChartDashboardType]
	public class TranslationEnq : PXGraph<TranslationEnq>
	{
		#region Buttons
        public PXAction<TranslationEnqFilter> cancel;

        [PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
        [PXCancelButton]
        protected virtual IEnumerable Cancel(PXAdapter adapter)
        {
            TranslationHistoryRecords.Cache.Clear();
			Filter.Cache.Clear();
			TimeStamp = null;
            return adapter.Get();
        }
		public PXAction<TranslationEnqFilter> viewTranslatedBatch;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTranslatedBatch(PXAdapter adapter)
		{
			if (this.TranslationHistoryRecords.Current != null)
			{
			if (this.TranslationHistoryRecords.Current.BatchNbr != null)
				{
					JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
					graph.Clear();
					Batch newBatch = new Batch();
					graph.BatchModule.Current = PXSelect<GL.Batch,
							Where<GL.Batch.module, Equal<Required<GL.Batch.module>>,
							And<GL.Batch.batchNbr, Equal<Required<GL.Batch.batchNbr>>>>>
							.Select(this, "CM", TranslationHistoryRecords.Current.BatchNbr);
					throw new PXRedirectRequiredException(graph, true, Messages.ViewTranslationBatch) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return Filter.Select();
		}
		/*
		public PXAction<TranslationEnqFilter> viewReversedBatch;
		[PXUIField(DisplayName = Messages.ViewReversingBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
	    public virtual IEnumerable ViewReversedBatch(PXAdapter adapter)
		{
			if (this.TranslationHistoryRecords.Current != null)
			{
				if (this.TranslationHistoryRecords.Current.ReversedBatchNbr != null)
				{
					JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
					graph.Clear();
					Batch newBatch = new Batch();
					graph.BatchModule.Current = PXSelect<GL.Batch,
							Where<GL.Batch.module, Equal<Required<GL.Batch.module>>,
							And<GL.Batch.batchNbr, Equal<Required<GL.Batch.batchNbr>>>>>
							.Select(this, "CM", TranslationHistoryRecords.Current.ReversedBatchNbr);
					throw new PXRedirectRequiredException(graph, Messages.ViewReversingBatch);
				}
			}
			return Filter.Select();
		}
		*/
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<TranslationEnqFilter> viewDetails;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			if (this.TranslationHistoryRecords.Current != null)
			{
				if (this.TranslationHistoryRecords.Current.ReferenceNbr != null)
				{
					TranslationHistoryMaint graph = PXGraph.CreateInstance<TranslationHistoryMaint>();
					graph.Clear();
					TranslationHistory newHist = new TranslationHistory();
					graph.TranslHistRecords.Current = PXSelect<TranslationHistory,
							Where<TranslationHistory.referenceNbr, Equal<Required<TranslationHistory.referenceNbr>>>>
							.Select(this, TranslationHistoryRecords.Current.ReferenceNbr);
					throw new PXRedirectRequiredException(graph, true, Messages.ViewTranslationDetails) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return Filter.Select();
		}
		#endregion

        public PXFilter<TranslationEnqFilter> Filter;
		[PXFilterable]
		public PXSelect<TranslationHistory> TranslationHistoryRecords;

		public PXSetup<CMSetup> CMSetup;

		public TranslationEnq()
		{
			CMSetup setup = CMSetup.Current;
		}

		protected virtual IEnumerable translationHistoryRecords()
        {
            PXSelectBase<TranslationHistory> cmd;

            TranslationEnqFilter filter = Filter.Current as TranslationEnqFilter;

            this.TranslationHistoryRecords.Cache.AllowInsert = false;
            this.TranslationHistoryRecords.Cache.AllowDelete = false;
            this.TranslationHistoryRecords.Cache.AllowUpdate = false;

            TranslationHistoryRecords.Cache.Clear();

            cmd = new PXSelectOrderBy<TranslationHistory, 
                              OrderBy<Desc<TranslationHistory.finPeriodID, 
                                      Desc<TranslationHistory.translDefId, 
                                      Asc<TranslationHistory.referenceNbr>>>>>(this);

            if (filter.TranslDefId != null)
            {
                cmd.WhereAnd<Where<TranslationHistory.translDefId, Equal<Current<TranslationEnqFilter.translDefId>>>>();
            }

            if (filter.FinPeriodID != null)
            {
                cmd.WhereAnd<Where<TranslationHistory.finPeriodID, Equal<Current<TranslationEnqFilter.finPeriodID>>>>();
            }


            if (filter.Released == false)
            {
                cmd.WhereAnd<Where<TranslationHistory.status, NotEqual<TranslationReleased>>>();
            }

            if (filter.Unreleased == false)
            {
                cmd.WhereAnd<Where<TranslationHistory.status, NotEqual<TranslationUnReleased>>>();
            }

            if (filter.Voided == false)
            {
                cmd.WhereAnd<Where<TranslationHistory.status, NotEqual<TranslationVoided>>>();
            }


            foreach (PXResult<TranslationHistory> thist in cmd.Select())
            {
                yield return thist;
            }

        }

        public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
        {
            return base.ExecuteUpdate(viewName, keys, values, parameters);
        }

        protected virtual void TranslationHistory_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            TranslationHistory tlist = (TranslationHistory)e.Row;

            PXUIFieldAttribute.SetEnabled<TranslationHistory.dateEntered>(cache, tlist, false);
            PXUIFieldAttribute.SetEnabled<TranslationHistory.description>(cache, tlist, false);
            PXUIFieldAttribute.SetEnabled<TranslationHistory.ledgerID>(cache, tlist, false);
            PXUIFieldAttribute.SetEnabled<TranslationHistory.finPeriodID>(cache, tlist, false);
            PXUIFieldAttribute.SetEnabled<TranslationHistory.referenceNbr>(cache, tlist, false);
            PXUIFieldAttribute.SetEnabled<TranslationHistory.batchNbr>(cache, tlist, false);
            //PXUIFieldAttribute.SetEnabled<TranslationHistory.reversedBatchNbr>(cache, tlist, false);
            PXUIFieldAttribute.SetEnabled<TranslationHistory.status>(cache, tlist, false);

        }
   }
}
