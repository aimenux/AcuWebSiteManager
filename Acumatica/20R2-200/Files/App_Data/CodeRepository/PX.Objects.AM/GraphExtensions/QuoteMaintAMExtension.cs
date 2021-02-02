using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
    /// <summary>
    /// Graph extension for CR Quotes page (new in 2018R1)
    /// </summary>
    public class QuoteMaintAMExtension : OpportunityBaseAMExtension<QuoteMaint, CRQuote>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingEstimating>() 
                   || PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingProductConfigurator>();
        }

        /// <summary>
        /// Overriding order by to maintain grouping for configured items and supplemental items
        /// </summary>
        [PXViewName(PX.Objects.CR.Messages.QuoteProducts)]
        [PXImport(typeof(CRQuote))]
        public PXSelect<
            CROpportunityProducts,
            Where<CROpportunityProducts.quoteID, Equal<Current<CRQuote.quoteID>>>,
            OrderBy<
                Asc<CROpportunityProductsExt.aMParentLineNbr,
                Asc<CROpportunityProducts.lineNbr>>>>
            Products;

        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<
            AMEstimateReferenceQuote,
            Where<AMEstimateReferenceQuote.opportunityQuoteID, Equal<Current<CRQuote.quoteID>>>> QuoteDetailEstimateRecords;


        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXParent(typeof(Select<CRQuote,
            Where<CRQuote.quoteID, Equal<Current<AMEstimateReference.opportunityQuoteID>>>>), LeaveChildren = true)]
        protected virtual void AMEstimateReference_EstimateID_CacheAttached(PXCache sender) 
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXLineNbr(typeof(CRQuote.productCntr))] //consume the same ID as products to prevent duplicates in the tax table
        protected virtual void AMEstimateReference_TaxLineNbr_CacheAttached(PXCache sender)
        {
        }

        protected override OpportunityDocumentMapping GetOpportunityDocumentMapping()
        {
            return new OpportunityDocumentMapping(typeof(CRQuote));
        }

        public override bool OrderAllowsEdit => Base.Quote?.Current != null && Base.Quote.AllowUpdate &&
                                                Base.Products.AllowInsert && Base.Products.AllowUpdate &&
                                                !Base.Quote.Current.IsDisabled.GetValueOrDefault();

        [PXOverride]
        public virtual void CopyToQuote(CRQuote currentquote, QuoteMaint.CopyQuoteFilter param, Action<CRQuote, QuoteMaint.CopyQuoteFilter> del)
        {
            try
            {
                del?.Invoke(currentquote, param);
            }
            catch (PXRedirectRequiredException redirect)
            {
                var quoteMaintGraph = (QuoteMaint) redirect.Graph;
                if (quoteMaintGraph == null)
                {
                    throw;
                }
                CopyConfigurationsToNewQuote(currentquote.QuoteID, quoteMaintGraph);

                if (AllowEstimates && ContainsEstimates)
                {
                    // Saving the Quote Graph so QuoteNbr is available 
                    quoteMaintGraph.Actions.PressSave();
                    CopyEstimatesToQuote(currentquote.QuoteID, quoteMaintGraph);

                    EstimateGraphHelper.SetCuryProductsAmount(quoteMaintGraph, false);
                    if (quoteMaintGraph.IsDirty)
                    {
                        quoteMaintGraph.Actions.PressSave();
                    }

                    //cache refresh...
                    quoteMaintGraph.Actions.PressCancel();
                }

                throw;
            }
        }

        protected override AMEstimateReference SetEstimateReference(AMEstimateReference estimateReference)
        {
            if (estimateReference == null)
            {
                throw new ArgumentNullException(nameof(estimateReference));
            }

            var order = Base?.Quote?.Current;
            if (order == null)
            {
                return estimateReference;
            }

            estimateReference.BAccountID = order.BAccountID;
            estimateReference.BranchID = order.BranchID;
            estimateReference.OpportunityID = order.OpportunityID;
            estimateReference.OpportunityQuoteID = order.QuoteID;
            estimateReference.CuryInfoID = order.CuryInfoID;
            estimateReference.QuoteNbr = order.QuoteNbr;

            return estimateReference;
        }

        protected virtual string AppendQuoteNbr(string baseMessage, CRQuote quote)
        {
            return baseMessage == null ? null : $"{baseMessage} {Messages.GetLocal(Messages.Quote)} {quote.QuoteNbr}";
        }

        protected override AMEstimateHistory MakeQuoteEstimateHistory(AMEstimateItem amEstimateItem,
            bool estimateCreated)
        {
            var currentOrder = (CRQuote)Base?.Quote?.Current;
            if (currentOrder == null)
            {
                return null;
            }

            var estimateHistory = base.MakeQuoteEstimateHistory(amEstimateItem, estimateCreated);
            if (estimateHistory == null)
            {
                return null;
            }

            estimateHistory.Description = AppendQuoteNbr(estimateHistory.Description, currentOrder);
            return estimateHistory;
        }


        [PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Insert,
            MapViewRights = PXCacheRights.Insert)]
        [PXButton]
        protected override IEnumerable addEstimate(PXAdapter adapter)
        {
            if (Base?.Quote?.Current == null)
            {
                return adapter.Get();
            }

            if (OrderEstimateItemFilter.AskExt() == WebDialogResult.OK)
            {
                var estimateGraph = AddEstimateToQuote(OrderEstimateItemFilter.Current, Base.Quote.Current);
                if (estimateGraph?.EstimateReferenceRecord?.Current != null)
                {
                    var estRef = estimateGraph.EstimateReferenceRecord.Current;
                    estRef.OpportunityID = Base.Quote.Current.OpportunityID;
                    estRef.OpportunityQuoteID = Base.Quote.Current.QuoteID;
                    estRef.QuoteNbr = Base.Quote.Current.QuoteNbr;
                    estimateGraph.EstimateReferenceRecord.Update(estRef);
                }

                var estGraphHelper = new EstimateGraphHelper(estimateGraph);

                estGraphHelper.PersistQuoteMaint(Base,
                    OrderEstimateItemFilter.Current.AddExisting.GetValueOrDefault()
                        ? EstimateReferenceOrderAction.Add
                        : EstimateReferenceOrderAction.New);

                //press cancel only for "refresh"
                Base.Actions.PressCancel();
            }

            OrderEstimateItemFilter.Cache.Clear();

            return adapter.Get();
        }

        protected override void RemoveEstimateFromQuote(EstimateMaint estimateGraph)
        {
            var currentOrder = (CRQuote) Base?.Quote?.Current;
            if (currentOrder == null || string.IsNullOrWhiteSpace(estimateGraph?.Documents?.Current?.EstimateID))
            {
                return;
            }

            estimateGraph.EstimateHistoryRecords.Insert(new AMEstimateHistory
            {
                EstimateID = estimateGraph.Documents.Current.EstimateID.TrimIfNotNullEmpty(),
                RevisionID = estimateGraph.Documents.Current.RevisionID.TrimIfNotNullEmpty(),
                Description = AppendQuoteNbr(Messages.GetLocal(Messages.EstimateRemovedFromOpportunity,
                        currentOrder.OpportunityID),
                        currentOrder)
            });

            if (estimateGraph.IsDirty && estimateGraph.EstimateReferenceRecord?.Current != null)
            {
                var estGraphHelper = new EstimateGraphHelper(estimateGraph);
                estGraphHelper.PersistQuoteMaintRemove(Base,
                    new List<AMEstimateReference> {estimateGraph.EstimateReferenceRecord.Current});
                //press cancel only for "refresh"
                Base.Actions.PressCancel();
            }
        }

        [PXOverride]
        public virtual void Persist(Action del)
        {
            var currentQuote = Base?.Quote?.Current;

            if (currentQuote != null)
            {
                var quoteStatus = Base.Quote.Cache.GetStatus(currentQuote);

                if (quoteStatus != PXEntryStatus.Notchanged)
                {
                    var currentOpp = Base.Opportunity?.Current;
                    if (currentOpp != null)
                    {
                        ChangeEstimatePrimary<PX.Objects.CR.Standalone.CROpportunity.defQuoteID>(Base.Opportunity.Cache);
                    }

                    ChangeEstimateStatus<CRQuote.quoteID, CRQuote.status>(Base.Quote.Cache);
                }
            }

            // Suppress Cascade Deletion of Configurations
            foreach (CRQuote quote in Base.Quote.Cache.Deleted)
            {
                var singleQuote = PXSelect<CRQuote, Where<CRQuote.opportunityID, Equal<Optional<CRQuote.opportunityID>>>>.SelectSingleBound(Base, null, quote.OpportunityID);
                
                if (singleQuote.Count == 0)
                {
                    SuppressCascadeConfigurationDeletion(quote);
                }
            }

            del?.Invoke();
        }

        /// <summary>
        /// Stop the cascade delete of configurations. This occurs when the last quote (IsPrimary) is deleted and only the Opportunity is remaining. We need to keep the configurations.
        /// </summary>
        /// <param name="quote">The quote being deleted</param>
        protected virtual void SuppressCascadeConfigurationDeletion(CRQuote quote)
        {
            SuppressCascadeDeletion(this.ItemConfiguration.View, quote);

            // The following views are added via ConfigurationSelect - not directly on the graph. Getting View by type
            foreach (var view in Base.ViewNames.Keys.ToList())
            {
                var cacheTypeName = view.Cache.GetItemType().Name;
                if (cacheTypeName == typeof(AMConfigResultsAttribute).Name)
                {
                    SuppressCascadeDeletion(view, quote);
                    continue;
                }

                if (cacheTypeName == typeof(AMConfigResultsFeature).Name)
                {
                    SuppressCascadeDeletion(view, quote);
                    continue;
                }

                if (cacheTypeName == typeof(AMConfigResultsOption).Name)
                {
                    SuppressCascadeDeletion(view, quote);
                    continue;
                }

                if (cacheTypeName == typeof(AMConfigResultsRule).Name)
                {
                    SuppressCascadeDeletion(view, quote);
                }
            }
        }

        /// <summary>
        /// Same as QuoteMaint.SuppressCascadeDeletion however that call is protected so this is a copy paste
        /// </summary>
        protected void SuppressCascadeDeletion(PXView view, object row)
        {
            PXCache cache = Base.Caches[row.GetType()];
            foreach (object rec in view.Cache.Deleted)
            {
                if (view.Cache.GetStatus(rec) == PXEntryStatus.Deleted)
                {
                    bool own = true;
                    foreach (string key in new[] { typeof(CROpportunity.quoteNoteID).Name })
                    {
                        if (!object.Equals(cache.GetValue(row, key), view.Cache.GetValue(rec, key)))
                        {
                            own = false;
                            break;
                        }
                    }
                    if (own)
                    {
                        view.Cache.SetStatus(rec, PXEntryStatus.Notchanged);
                    }
                }
            }
        }

        protected override void PrimaryRowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            var row = (CRQuote)e.Row;
            if (row == null)
            {
                return;
            }

            // Primary Quote can only be deleted when it's the Only Quote
            // Else you need to set another Quote to Primary and delete the now non primary Quote
            if (row.IsPrimary == true)
            {
                // Change to only drop the QuoteNbr so Estimate remains on the Opportunity
                foreach (AMEstimateReference estReference in OpportunityEstimateRecords.Select())
                {
                    estReference.QuoteNbr = null;
                    Base.Caches<AMEstimateReference>().Update(estReference);
                }

                return;
            }

            RemoveEstimateReference(OpportunityEstimateRecords.Select(), AppendQuoteNbr(Messages.GetLocal(
                    Messages.EstimateRemovedFromOpportunity,
                    row.OpportunityID),
                row));
        }

            /// <summary>
            /// Update to estimate reference record for document detail estimates. 
            /// Do not update for estimate tab estimate references.
            /// </summary>
        [PXProjection(typeof(Select2<AMEstimateReference,
            InnerJoin<AM.Standalone.AMEstimatePrimary,
            On<AM.Standalone.AMEstimatePrimary.estimateID, Equal<AMEstimateReference.estimateID>>>>), Persistent = true)]
        [Serializable]
        [PXHidden]
        public class AMEstimateReferenceQuote : IBqlTable
        {
            #region Estimate ID
            public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }
            protected String _EstimateID;
            [PXDBDefault(typeof(AMEstimateItem.estimateID))]
            [EstimateID(IsKey = true, Enabled = false, BqlField = typeof(AMEstimateReference.estimateID))]
            [PXParent(typeof(Select<CRQuote,
                Where<CRQuote.quoteID, Equal<Current<AMEstimateReferenceQuote.opportunityQuoteID>>>>), LeaveChildren = true)]
            public virtual String EstimateID
            {
                get { return this._EstimateID; }
                set { this._EstimateID = value; }
            }
            #endregion
            #region Revision ID
            public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
            protected String _RevisionID;
            [PXDBDefault(typeof(AMEstimateItem.revisionID))]
            [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">AAAAAAAAAA", BqlField = typeof(AMEstimateReference.revisionID))]
            [PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String RevisionID
            {
                get { return this._RevisionID; }
                set { this._RevisionID = value; }
            }
            #endregion
            #region Branch ID 
            public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
            protected Int32? _BranchID;
            [PXDBInt(BqlField = typeof(AMEstimateReference.branchID))]
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
            #region QuoteSource
            public abstract class quoteSource : PX.Data.BQL.BqlInt.Field<quoteSource> { }
            [PXDBInt(BqlField = typeof(AM.Standalone.AMEstimatePrimary.quoteSource))]
            [PXDefault(EstimateSource.Estimate)]
            [PXUIField(DisplayName = "Quote Source", Enabled = false)]
            public virtual int? QuoteSource { get; set; }
            #endregion
            #region CuryInfoID
            public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
            protected Int64? _CuryInfoID;
            [PXDBLong(BqlField = typeof(AMEstimateReference.curyInfoID))]
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
            #region OpportunityID
            public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }
            protected String _OpportunityID;
            [PXDBString(10, IsUnicode = true, BqlField = typeof(AMEstimateReference.opportunityID))]
            [PXUIField(DisplayName = "Quote Opportunity ID")]
            [PXDBDefault(typeof(CRQuote.opportunityID), PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual String OpportunityID
            {
                get
                {
                    return this._OpportunityID;
                }
                set
                {
                    this._OpportunityID = value;
                }
            }
            #endregion
            #region OpportunityQuoteID
            public abstract class opportunityQuoteID : PX.Data.BQL.BqlGuid.Field<opportunityQuoteID> { }
            [PXDBGuid(BqlField = typeof(AMEstimateReference.opportunityQuoteID))]
            [PXUIField(DisplayName = "Opportunity Quote ID", Enabled = false, Visible = false)]
            public virtual Guid? OpportunityQuoteID { get; set; }
            #endregion
            #region QuoteNbr
            public abstract class quoteNbr : PX.Data.BQL.BqlString.Field<quoteNbr> { }
            protected String _QuoteNbr;
            [PXDBString(15, IsUnicode = true, BqlField = typeof(AMEstimateReference.quoteNbr))]
            [PXUIField(DisplayName = "Quote Nbr")]
            public virtual String QuoteNbr
            {
                get
                {
                    return this._QuoteNbr;
                }
                set
                {
                    this._QuoteNbr = value;
                }
            }
            #endregion
            #region TaxLineNbr
            public abstract class taxLineNbr : PX.Data.BQL.BqlInt.Field<taxLineNbr> { }
            protected Int32? _TaxLineNbr;
            [PXDBInt(BqlField = typeof(AMEstimateReference.taxLineNbr))]
            [PXUIField(DisplayName = "Tax Line Nbr.", Visible = false, Enabled = false)]
            public virtual Int32? TaxLineNbr
            {
                get
                {
                    return this._TaxLineNbr;
                }
                set
                {
                    this._TaxLineNbr = value;
                }
            }
            #endregion
            #region Tax Category ID
            public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
            protected String _TaxCategoryID;
            [PXDBString(10, IsUnicode = true, BqlField = typeof(AMEstimateReference.taxCategoryID))]
            [PXUIField(DisplayName = "Tax Category")]
            public virtual String TaxCategoryID
            {
                get
                {
                    return this._TaxCategoryID;
                }
                set
                {
                    this._TaxCategoryID = value;
                }
            }
            #endregion
            #region Order Qty
            public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { } 
            protected Decimal? _OrderQty;
            [PXDBQuantity(BqlField = typeof(AMEstimateReference.orderQty))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Order Qty")]
            public virtual Decimal? OrderQty
            {
                get
                {
                    return this._OrderQty;
                }
                set
                {
                    this._OrderQty = value;
                }
            }
            #endregion
            #region Cury Unit Price
            public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
            protected Decimal? _CuryUnitPrice;
            [PXDBCurrency(typeof(Search<PX.Objects.CS.CommonSetup.decPlPrcCst>), typeof(AMEstimateReferenceQuote.curyInfoID),
                typeof(AMEstimateReferenceQuote.unitPrice), BqlField = typeof(AMEstimateReference.curyUnitPrice))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Unit Price", Enabled = false)]
            public virtual Decimal? CuryUnitPrice
            {
                get
                {
                    return this._CuryUnitPrice;
                }
                set
                {
                    this._CuryUnitPrice = value;
                }
            }
            #endregion
            #region Unit Price
            public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }
            protected Decimal? _UnitPrice;
            [PXDBPriceCost(BqlField = typeof(AMEstimateReference.unitPrice))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Unit Price", Enabled = false)]
            public virtual Decimal? UnitPrice
            {
                get
                {
                    return this._UnitPrice;
                }
                set
                {
                    this._UnitPrice = value;
                }
            }
            #endregion
            #region Cury Ext Price
            public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }
            protected Decimal? _CuryExtPrice;
            [PXDBCurrency(typeof(AMEstimateReferenceQuote.curyInfoID), typeof(AMEstimateReferenceQuote.extPrice),
                BqlField = typeof(AMEstimateReference.curyExtPrice))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Price", Enabled = false)]
            [PXFormula(typeof(Mult<AMEstimateReferenceQuote.curyUnitPrice, AMEstimateReferenceQuote.orderQty>))]
            public virtual Decimal? CuryExtPrice
            {
                get
                {
                    return this._CuryExtPrice;
                }
                set
                {
                    this._CuryExtPrice = value;
                }
            }
            #endregion
            #region Ext Price
            public abstract class extPrice : PX.Data.BQL.BqlDecimal.Field<extPrice> { }
            protected Decimal? _ExtPrice;
            [PXDBBaseCury(BqlField = typeof(AMEstimateReference.extPrice))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Price", Enabled = false)]
            public virtual Decimal? ExtPrice
            {
                get
                {
                    return this._ExtPrice;
                }
                set
                {
                    this._ExtPrice = value;
                }
            }
            #endregion
            #region BAccount ID
            public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
            protected Int32? _BAccountID;
            [PXDBInt(BqlField = typeof(AMEstimateReference.bAccountID))]
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
            #region EstimateStatus
            public abstract class estimateStatus : PX.Data.BQL.BqlInt.Field<estimateStatus> { }
            [PXDBInt(BqlField = typeof(AM.Standalone.AMEstimatePrimary.estimateStatus))]
            [PXDefault(AM.Attributes.EstimateStatus.NewStatus)]
            [PXUIField(DisplayName = "Estimate Status", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual int? EstimateStatus { get; set; }
            #endregion
            #region External Ref Nbr
            public abstract class externalRefNbr : PX.Data.BQL.BqlString.Field<externalRefNbr> { }
            protected String _ExternalRefNbr;
            [PXDBString(15, IsUnicode = true, BqlField = typeof(AMEstimateReference.externalRefNbr))]
            [PXUIField(DisplayName = "Ext. Ref. Nbr.")]
            public virtual String ExternalRefNbr
            {
                get
                {
                    return this._ExternalRefNbr;
                }
                set
                {
                    this._ExternalRefNbr = value;
                }
            }
            #endregion
            #region PEstimateID
            /// <summary>
            /// EstimateID for AMEstimatePrimary
            /// </summary>
            public abstract class pEstimateID : PX.Data.BQL.BqlString.Field<pEstimateID> { }
            /// <summary>
            /// EstimateID for AMEstimatePrimary
            /// </summary>
            [PXExtraKey]
            [EstimateID(Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible, BqlField = typeof(AM.Standalone.AMEstimatePrimary.estimateID))]
            public virtual String PEstimateID
            {
                get { return EstimateID; }
                set { }
            }
            #endregion
            #region IsLockedByQuote
            /// <summary>
            /// When the estimate is linked to specific quote orders, the quote order will drive some fields such as mark as primary which should prevent the user from making changes on the estimate directly
            /// </summary>
            public abstract class isLockedByQuote : PX.Data.BQL.BqlBool.Field<isLockedByQuote> { }
            /// <summary>
            /// When the estimate is linked to specific quote orders, the quote order will drive some fields such as mark as primary which should prevent the user from making changes on the estimate directly
            /// </summary>
            [PXDBBool(BqlField = typeof(AM.Standalone.AMEstimatePrimary.isLockedByQuote))]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Locked by Quote", Enabled = false, Visible = false)]
            public virtual Boolean? IsLockedByQuote { get; set; }
            #endregion
        }
    }
}