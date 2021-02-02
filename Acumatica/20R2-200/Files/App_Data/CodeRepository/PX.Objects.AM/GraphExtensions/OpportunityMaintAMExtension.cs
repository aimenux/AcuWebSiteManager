using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.AM.GraphExtensions
{
    using AM;
    using AM.Attributes;

    /// <summary>
    /// Graph extension for Opportunities page
    /// </summary>
    public class OpportunityMaintAMExtension : OpportunityBaseAMExtension<OpportunityMaint, CROpportunity>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingEstimating>()
                   || PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingProductConfigurator>();
        }

        /// <summary>
        /// Overriding order by to maintain grouping for configured items and supplemental items
        /// </summary>
        [PXViewName(PX.Objects.CR.Messages.OpportunityProducts)]
        [PXImport(typeof(CROpportunity))]
        public PXSelect<
            CROpportunityProducts,
            Where<CROpportunityProducts.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>,
            OrderBy<
                Asc<CROpportunityProductsExt.aMParentLineNbr,
                Asc<CROpportunityProducts.lineNbr>>>>
            Products;

        [PXHidden]
        [PXCopyPasteHiddenView]
        public PXSelect<
            AMEstimateReferenceOpportunity,
            Where<AMEstimateReferenceOpportunity.opportunityID, Equal<Current<CROpportunity.opportunityID>>
                >> OpportunityDetailEstimateRecords;

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXParent(typeof(Select<CROpportunity,
            Where<CROpportunity.defQuoteID, Equal<Current<AMEstimateReference.opportunityQuoteID>>>>), LeaveChildren = true)]
        protected virtual void AMEstimateReference_EstimateID_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXLineNbr(typeof(CROpportunity.productCntr))] //consume the same ID as products to prevent duplicates in the tax table
        protected virtual void AMEstimateReference_TaxLineNbr_CacheAttached(PXCache sender)
        {
        }

        protected override OpportunityDocumentMapping GetOpportunityDocumentMapping()
        {
            return new OpportunityDocumentMapping(typeof(CROpportunity))
            {
                QuoteID = typeof(CROpportunity.defQuoteID)
            };
        }

        public override bool OrderAllowsEdit => Base.Opportunity.AllowUpdate && Base.Products.AllowInsert && Base.Products.AllowUpdate;

        /// <summary>
        /// Overriding standard Opportunity graph create invoice to prevent creation when estimates exist for the current opportunity
        /// </summary>
        public PXAction<CROpportunity> createInvoice;
        [PXUIField(DisplayName = PX.Objects.CR.Messages.CreateInvoice, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
        public virtual IEnumerable CreateInvoice(PXAdapter adapter)
        {
            var opportunity = Base.Opportunity.Current;
            if (opportunity == null)
            {
                return adapter.Get();
            }

            if (ContainsEstimates)
            {
                throw new PXException(PX.Objects.CR.Messages.StockitemsCanNotBeIncludedIninvoice);
            }

            return Base.CreateInvoice(adapter);
        }

        [PXOverride]
        public virtual void CreateNewQuote(CROpportunity opportunity, OpportunityMaint.CreateQuotesFilter param, WebDialogResult result, Action<CROpportunity, OpportunityMaint.CreateQuotesFilter, WebDialogResult> del)
        {
            PXRedirectRequiredException redirect = null;
            try
            {
                del?.Invoke(opportunity, param, result);
            }
            catch (PXRedirectRequiredException e)
            {
                redirect = e;
            }

            var quoteGraph = (QuoteMaint)redirect?.Graph ?? PXGraph.CreateInstance<QuoteMaint>();

            if (redirect == null && quoteGraph != null)
            {
                // Find the last created quote for the given opportunity
                quoteGraph.Quote.Current = PXSelect<CRQuote,
                    Where<CRQuote.opportunityID, Equal<Required<CRQuote.opportunityID>>>,
                    OrderBy<Desc<CRQuote.createdDateTime>>
                    >.SelectWindowed(Base,0, 1, opportunity.OpportunityID);
            }

            if (quoteGraph?.Quote?.Current == null)
            {
                if (redirect != null)
                {
                    throw redirect;
                }
                return;
            }

            CopyConfigurationsToNewQuote(opportunity.QuoteNoteID, quoteGraph);
            quoteGraph.Actions.PressSave();

            if (ContainsEstimates)
            {
                if (redirect != null)
                {
                    // Saving the Quote Graph so QuoteNbr is available because When Redirect the Graph is not saved
                    quoteGraph.Actions.PressSave();
                }

                CopyEstimatesToQuote(opportunity.QuoteNoteID, quoteGraph);

                if (redirect != null)
                {
                    //cache refresh...
                    quoteGraph.Actions.PressCancel();
                }
            }

            if (redirect != null)
            {
                throw redirect;
            }
        }

        /// <summary>
        /// Indicates if estimates are allowed for Order type from the create order process
        /// </summary>
        protected bool CreateSalesOrderTypeAllowsEstimates
        {
            get
            {
                SOOrderType orderType = PXSelect<SOOrderType,
                    Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>
                    >>.Select(Base, Base.CreateOrderParams.Current.OrderType);

                var orderTypeExt = orderType.GetExtension<SOOrderTypeExt>();

                if (orderTypeExt != null)
                {
                    return orderTypeExt.AMEstimateEntry.GetValueOrDefault();
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates if configurations are allowed for Order type from the create order process
        /// </summary>
        protected bool CreateSalesOrderFilterAllowsConfigurations
        {
            get
            {
                SOOrderType orderType = PXSelect<SOOrderType,
                    Where<SOOrderType.orderType, Equal<Required<SOOrderType.orderType>>
                    >>.Select(Base, Base.CreateOrderParams.Current.OrderType);

                var orderTypeExt = orderType.GetExtension<SOOrderTypeExt>();

                if (orderTypeExt != null)
                {
                    return orderTypeExt.AMConfigurationEntry.GetValueOrDefault() && AllowConfigurations;
                }

                return false;
            }
        }

        protected virtual void CROpportunity_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            del?.Invoke(sender, e);

            if (!AllowEstimates || !ContainsEstimates)
            {
                return;
            }

            // enable create sales order as the base call will disable if no products entered
            // Including the other logic for create sales order - removing the checks for products
            CRQuote primaryQt = Base.PrimaryQuoteQuery.SelectSingle();
            bool hasQuotes = primaryQt != null;
            Base.createSalesOrder.SetEnabled(!hasQuotes || primaryQt.QuoteType == CRQuoteTypeAttribute.Distribution);
        }

        public virtual void CreateSalesOrderFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            del?.Invoke(sender, e);

            var row = (OpportunityMaint.CreateSalesOrderFilter)e.Row;
            if (row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetVisible<CRCreateSalesOrderFilterExt.aMIncludeEstimate>(sender, row, AllowEstimates && ContainsEstimates);
            PXUIFieldAttribute.SetEnabled<CRCreateSalesOrderFilterExt.aMIncludeEstimate>(sender, row, ContainsEstimates);
            PXUIFieldAttribute.SetVisible<CRCreateSalesOrderFilterExt.aMCopyConfigurations>(sender, row, CreateSalesOrderFilterAllowsConfigurations);
        }

        [PXOverride]
        public virtual void DoCreateSalesOrder(OpportunityMaint.CreateSalesOrderFilter param, Action<OpportunityMaint.CreateSalesOrderFilter> del)
        {
            if (del == null)
            {
                return;
            }

            var paramExtension = param?.GetExtension<CRCreateSalesOrderFilterExt>();

            var canConvertEstimates = paramExtension != null && paramExtension.AMIncludeEstimate.GetValueOrDefault() && ContainsEstimates;
            if (canConvertEstimates && !CurrentEstimatesValidForSalesDetails(out var ex))
            {
                if (ex == null)
                {
                    ex = new PXException(AM.Messages.UnableToConvertEstimatesForOpportunity,
                        Base.Opportunity.Current == null ? "?" : Base.Opportunity.Current.OpportunityID.TrimIfNotNullEmpty());
                }
                throw ex;
            }

            //Add estimate to document details tab if the order type doesn't allow estimates
            if(canConvertEstimates && !CreateSalesOrderTypeAllowsEstimates && AllowEstimates)
            {
                PXGraph.InstanceCreated.AddHandler<SOOrderEntry>(graph =>
                {
                    graph.RowInserted.AddHandler<SOOrder>((cache, e) =>
                    {
                        // SetTaxCalc required when adding to order from opportunity but in no other add to order scenario. 
                        //  This is due to taxes not working when adding from oppiroty but does work when from sales order or estimate. 
                        SOTaxAttribute.SetTaxCalc<SOLine.taxCategoryID>(graph.Transactions.Cache, null, PX.Objects.TX.TaxCalc.ManualCalc);
                        SOTaxAttribute.SetTaxCalc<SOOrder.freightTaxCategoryID>(graph.Document.Cache, null, PX.Objects.TX.TaxCalc.ManualCalc);

                        AddEstimatesToSalesOrder(graph);
                    });
                });
            }

            if (paramExtension != null && paramExtension.AMCopyConfigurations.GetValueOrDefault() && CreateSalesOrderFilterAllowsConfigurations)
            {
                PXGraph.InstanceCreated.AddHandler<SOOrderEntry>(graph =>
                {

                    graph.RowInserting.AddHandler<SOLine>((cache, args) =>
                    {
                        var soLine = (SOLine)args.Row;
                        if (soLine == null)
                        {
                            return;
                        }
                        CROpportunityProducts opProduct = PXResult<CROpportunityProducts>.Current;
                        if (opProduct == null)
                        {
                            return;
                        }

                        var opProductExt = PXCache<CROpportunityProducts>.GetExtension<CROpportunityProductsExt>(opProduct);
                        var soLineExt = PXCache<SOLine>.GetExtension<SOLineExt>(soLine);
                        soLineExt.AMOrigParentLineNbr = opProductExt.AMParentLineNbr;
                        soLineExt.AMIsSupplemental = opProductExt.AMIsSupplemental;

                        if (!opProductExt.AMIsSupplemental.GetValueOrDefault())
                        {
                            return;
                        }

                        // ***
                        // *** Continue only for supplemental line items coming from an opp. product
                        // ***

                        var parent = ConfigSupplementalItemsHelper.FindParentConfigLineByOrigParentLineNbr(cache, soLine);
                        if (parent == null)
                        {
                            return;
                        }

                        soLineExt.AMParentLineNbr = parent.LineNbr;
                        soLine.SortOrder = parent.SortOrder;
#if DEBUG
                        AMDebug.TraceWriteMethodName($"SOLine[{soLine.OrderType}-{soLine.OrderNbr}-{soLine.LineNbr}] AMOrigParentLineNbr={opProductExt.AMParentLineNbr} ; AMParentLineNbr={soLineExt.AMParentLineNbr}");
#endif
                        //Check for inserted supps already and delete those lines that match by parent (no orig parent) and inventory item
                        foreach (var supSoLine in ConfigSupplementalItemsHelper.GetInsertedSupplementalLinesByParent((SOOrderEntry)cache.Graph, parent.LineNbr))
                        {
                            var supSoLineExt = supSoLine.GetExtension<SOLineExt>();
                            if (supSoLine.InventoryID == opProduct.InventoryID
                                //Sups inserted from config copy do not have an orig line pointing to a product from opportunity... we want only these sups...
                                && supSoLineExt != null && supSoLineExt.AMOrigParentLineNbr == null)
                            {
                                var copy = (SOLine)cache.CreateCopy(supSoLine);
                                copy.InventoryID = opProduct.InventoryID;
                                copy.SubItemID = opProduct.SubItemID;
                                copy.TranDesc = opProduct.Descr;
                                copy.OrderQty = opProduct.Quantity;
                                copy.UOM = opProduct.UOM;
                                copy.CuryUnitPrice = opProduct.CuryUnitPrice;
                                //copy.CuryLineAmt = opProduct.CuryAmount;
                                copy.TaxCategoryID = opProduct.TaxCategoryID;
                                copy.SiteID = opProduct.SiteID;
                                copy.IsFree = opProduct.IsFree;
                                copy.ProjectID = opProduct.ProjectID;
                                copy.TaskID = opProduct.TaskID;
                                copy.ManualPrice = Base.CreateOrderParams?.Current?.RecalculatePrices != true;
                                copy.ManualDisc = Base.CreateOrderParams?.Current?.RecalculateDiscounts != true;
                                copy.CuryDiscAmt = opProduct.CuryDiscAmt;
                                copy.DiscAmt = opProduct.DiscAmt;
                                copy.DiscPct = opProduct.DiscPct;

                                var copyExt = copy.GetExtension<SOLineExt>();
                                if (copyExt != null)
                                {
                                    copyExt.AMOrigParentLineNbr = opProductExt.AMParentLineNbr;
                                }
                                copy = (SOLine)cache.Update(copy);

                                PXNoteAttribute.CopyNoteAndFiles(Products.Cache, opProduct, cache, copy, Base.Setup.Current);
                            }
                        }

                        //CANCEL ALL COPIED SUPS FROM PRODUCTS AS THE COPY CONFIG WILL RE-ADD THE CORRECT SUPS TO ACCOUNT FOR CONFIG CHANGES...
                        args.Cancel = true;
                    });
                });

                PXGraph.InstanceCreated.AddHandler<SOOrderEntry>(graph =>
                {
                    graph.RowUpdated.AddHandler<SOLine>((cache, args) =>
                    {
                        var row = (SOLine)args.Row;
                        var graphExt = cache.Graph.GetExtension<SOOrderEntryAMExtension>();
                        if (row == null || graphExt?.ItemConfiguration == null)
                        {
                            return;
                        }

                        var rowExt = row.GetExtension<SOLineExt>();
                        if (rowExt == null)
                        {
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(rowExt.AMConfigurationID))
                        {
                            //Most likely not a configured line...
                            return;
                        }

                        graphExt.ItemConfiguration.RemoveSOLineHandlers();

                        CROpportunityProducts opProduct = PXResult<CROpportunityProducts>.Current;
                        if (opProduct == null)
                        {
                            return;
                        }
#if DEBUG
                        AMDebug.TraceWriteMethodName($"SOLine[{row.GetRowKeyValues(graph)}] Product[{opProduct.GetRowKeyValues(graph)}] IsSup={rowExt.AMIsSupplemental}");
#endif

                        AMConfigurationResults fromConfigResult = PXSelect<
                            AMConfigurationResults,
                            Where<AMConfigurationResults.opportunityQuoteID, Equal<Required<AMConfigurationResults.opportunityQuoteID>>,
                                And<AMConfigurationResults.opportunityLineNbr, Equal<Required<AMConfigurationResults.opportunityLineNbr>>>>>
                            .Select(cache.Graph, opProduct.QuoteID, opProduct.LineNbr);

                        if (fromConfigResult == null)
                        {
                            return;
                        }

                        AMConfigurationResults toConfigResult = graphExt.ItemConfiguration.Select();

                        if (toConfigResult == null)
                        {
                            return;
                        }

                        graphExt.CopyConfiguration(row, toConfigResult, fromConfigResult);
                    });
                });
            }

            del?.Invoke(param);
        }

        [PXUIField(DisplayName = Messages.Add, MapEnableRights = PXCacheRights.Insert,
            MapViewRights = PXCacheRights.Insert)]
        [PXButton]
        protected override IEnumerable addEstimate(PXAdapter adapter)
        {
            if (Base?.Opportunity?.Current == null)
            {
                return adapter.Get();
            }

            if (OrderEstimateItemFilter.AskExt() == WebDialogResult.OK)
            {
                var estimateGraph = AddEstimateToQuote(OrderEstimateItemFilter.Current, Base.Opportunity.Current);
                var estimateItem = estimateGraph?.Documents?.Current;
                if (estimateItem == null)
                {
                    OrderEstimateItemFilter.Cache.Clear();
                    return adapter.Get();
                }

                if (estimateGraph?.EstimateReferenceRecord?.Current != null)
                {
                    var estRef = estimateGraph.EstimateReferenceRecord.Current;
                    estRef.OpportunityID = Base.Opportunity.Current.OpportunityID;
                    estRef.OpportunityQuoteID = Base.Opportunity.Current.QuoteNoteID;

                    if (PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.salesQuotes>())
                    {
                        //TODO (2018R2) Confirm this works...
                        var crQuote = Base.Quotes.Select().ToFirstTableList().FirstOrDefault(x => x.QuoteID == Base.Opportunity.Current.QuoteNoteID);
                        if (!string.IsNullOrWhiteSpace(crQuote?.QuoteNbr))
                        {
                            estRef.QuoteNbr = crQuote.QuoteNbr;
                            estimateItem.IsLockedByQuote = true;
                            estimateGraph.Documents.Update(estimateItem);
                        }
                    }

                    estimateGraph.EstimateReferenceRecord.Update(estRef);
                }

                var estGraphHelper = new EstimateGraphHelper(estimateGraph);

                estGraphHelper.PersistOpportunityMaint(Base,
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
            var currentOrder = (CROpportunity)Base?.Opportunity?.Current;
            if (currentOrder == null || string.IsNullOrWhiteSpace(estimateGraph?.Documents?.Current?.EstimateID))
            {
                return;
            }

            estimateGraph.EstimateHistoryRecords.Insert(new AMEstimateHistory
            {
                EstimateID = estimateGraph.Documents.Current.EstimateID.TrimIfNotNullEmpty(),
                RevisionID = estimateGraph.Documents.Current.RevisionID.TrimIfNotNullEmpty(),
                Description = Messages.GetLocal(Messages.EstimateRemovedFromOpportunity,
                    currentOrder.OpportunityID)
            });

            if (estimateGraph.IsDirty && estimateGraph.EstimateReferenceRecord?.Current != null)
            {
                var estGraphHelper = new EstimateGraphHelper(estimateGraph);
                estGraphHelper.PersistOpportunityMaintRemove(Base, new List<AMEstimateReference> { estimateGraph.EstimateReferenceRecord.Current });
                //press cancel only for "refresh"
                Base.Actions.PressCancel();
            }
        }

        /// <summary>
        /// If an Opportunity or Quote is being deleted and contains estimate detail 
        /// we need to make sure the estimate has no reference to the Opportunity or Quote
        /// </summary>
        protected virtual void RemoveEstimateReferences()
        {
            foreach (CROpportunity opportunity in Base.Opportunity.Cache.Deleted)
            {
                foreach (AMEstimateReferenceOpportunity estimateRef in PXSelect<AMEstimateReferenceOpportunity,
                    Where<AMEstimateReferenceOpportunity.opportunityID, Equal<Required<AMEstimateReferenceOpportunity.opportunityID>>
                    >>.Select(Base, opportunity.OpportunityID))
                {
                    if (estimateRef == null || !estimateRef.OpportunityID.EqualsWithTrim(opportunity.OpportunityID))
                    {
                        continue;
                    }

                    estimateRef.OpportunityID = null;
                    estimateRef.OpportunityQuoteID = null;
                    estimateRef.QuoteNbr = null;
                    estimateRef.QuoteSource = EstimateSource.Estimate;
                    estimateRef.EstimateStatus = EstimateStatus.Canceled;
                    estimateRef.IsLockedByQuote = false;
                    OpportunityDetailEstimateRecords.Update(estimateRef);
                }
            }
        }

        [PXOverride]
        public virtual void Persist(Action del)
        {
            RemoveEstimateReferences();

            ChangeEstimatePrimary<CROpportunity.defQuoteID>(Base.Quotes.Cache);

            del?.Invoke();
        }

        public PXAction<CROpportunity> primaryQuote;
        [PXUIField(DisplayName = PX.Objects.CR.Messages.MarkAsPrimary)]
        [PXButton]
        public virtual IEnumerable PrimaryQuote(PXAdapter adapter)
        {
            if (Base.Quotes.Current?.IsPrimary != true)
            {
                ChangeEstimatePrimary(Base.Quotes.Current?.QuoteID, Base.Opportunity.Current?.DefQuoteID);
            }

            return Base.PrimaryQuote(adapter);
        }

        protected override void PrimaryRowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            var row = (CROpportunity)e.Row;
            if (row == null)
            {
                return;
            }

            RemoveEstimateReference(OpportunityEstimateRecords.Select(), Messages.GetLocal(
                Messages.EstimateRemovedFromOpportunity,
                row.OpportunityID));
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
        public class AMEstimateReferenceOpportunity : IBqlTable
        {
            #region Estimate ID
            public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }
            protected String _EstimateID;
            [PXDBDefault(typeof(AMEstimateItem.estimateID))]
            [EstimateID(IsKey = true, Enabled = false, BqlField = typeof(AMEstimateReference.estimateID))]
            [PXParent(typeof(Select<CROpportunity,
                Where<CROpportunity.opportunityID, Equal<Current<AMEstimateReferenceOpportunity.opportunityID>>>>), LeaveChildren = true)]
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
            [PXUIField(DisplayName = "Opportunity ID")]
            [PXDBDefault(typeof(CROpportunity.opportunityID), PersistingCheck = PXPersistingCheck.Nothing)]
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
            [PXDBDefault(typeof(CROpportunity.quoteNoteID), PersistingCheck = PXPersistingCheck.Nothing)]
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
            [PXDBCurrency(typeof(Search<PX.Objects.CS.CommonSetup.decPlPrcCst>), typeof(AMEstimateReferenceOpportunity.curyInfoID), 
                typeof(AMEstimateReferenceOpportunity.unitPrice), BqlField = typeof(AMEstimateReference.curyUnitPrice))]
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
            [PXDBCurrency(typeof(AMEstimateReferenceOpportunity.curyInfoID), typeof(AMEstimateReferenceOpportunity.extPrice), BqlField = typeof(AMEstimateReference.curyExtPrice))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Price", Enabled = false)]
            [PXFormula(typeof(Mult<AMEstimateReferenceOpportunity.curyUnitPrice, AMEstimateReferenceOpportunity.orderQty>))]
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
            [EstimateID(Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible, 
                BqlField = typeof(AM.Standalone.AMEstimatePrimary.estimateID))]
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
