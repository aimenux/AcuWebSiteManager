using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.SO;
using Messages = PX.Objects.AR.Messages;

namespace PX.Objects.Extensions.Discount
{
    /// <summary>The generic graph extension that includes the functionality for applying discounts.</summary>
    public abstract class DiscountGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
            where TGraph : PXGraph
            where TPrimary : class, IBqlTable, new()
    {
        /// <summary>The CacheAttached event handler for the <see cref="Discount.curyInfoID" /> field of the <see cref="Discount" /> mapped cache extension. You must override this method in
        /// the implementation class of the base graph.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <example>
        ///   <code title="Example" description="The following code shows sample implementation of the method in the implementation class for the OpportunityMaint graph." lang="CS">
        /// [CurrencyInfo(typeof(CROpportunity.curyInfoID))]
        /// [PXMergeAttributes]
        /// public override void Discount_CuryInfoID_CacheAttached(PXCache sender)
        /// {
        /// }</code>
        /// </example>
        public abstract void Discount_CuryInfoID_CacheAttached(PXCache sender);
        /// <summary>The CacheAttached event handler for the <see cref="Discount.discountID" /> field of the <see cref="Discount" /> mapped cache extension. You must override this method in
        /// the implementation class of the base graph.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <example>
        ///   <code title="Example" description="The following code shows sample implementation of the method in the implementation class for the OpportunityMaint graph." lang="CS">
        /// [PXSelector(typeof(Search&lt;ARDiscount.discountID, 
        ///     Where&lt;ARDiscount.type, NotEqual&lt;DiscountType.LineDiscount&gt;, 
        ///       And&lt;ARDiscount.applicableTo, NotEqual&lt;DiscountTarget.warehouse&gt;, 
        ///       And&lt;ARDiscount.applicableTo, NotEqual&lt;DiscountTarget.warehouseAndCustomer&gt;,
        ///       And&lt;ARDiscount.applicableTo, NotEqual&lt;DiscountTarget.warehouseAndCustomerPrice&gt;, 
        ///       And&lt;ARDiscount.applicableTo, NotEqual&lt;DiscountTarget.warehouseAndInventory&gt;, 
        ///       And&lt;ARDiscount.applicableTo, NotEqual&lt;DiscountTarget.warehouseAndInventoryPrice&gt;&gt;&gt;&gt;&gt;&gt;&gt;&gt;))]
        /// [PXMergeAttributes]
        /// public override void Discount_DiscountID_CacheAttached(PXCache sender)
        /// {
        /// }</code>
        /// </example>
        public abstract void Discount_DiscountID_CacheAttached(PXCache sender);


        /// <summary>Defines the default mapping of the <see cref="Document" /> mapped cache extension to a DAC.</summary>
        protected class DocumentMapping : IBqlMapping
        {
            /// <exclude />
            public Type Extension => typeof(Document);
            /// <exclude />
            protected Type _table;
            /// <exclude />
            public Type Table => _table;

            /// <summary>Creates the default mapping of the <see cref="Document" /> mapped cache extension to the specified table.</summary>
            /// <param name="table">A DAC.</param>
            public DocumentMapping(Type table)
            {
                _table = table;
            }
            /// <exclude />
            public Type BranchID = typeof(Document.branchID);
            /// <exclude />
            public Type CuryID = typeof(Document.curyID);
            /// <exclude />
            public Type CuryInfoID = typeof(Document.curyInfoID);
            /// <exclude />
            public Type CuryOrigDiscAmt = typeof(Document.curyOrigDiscAmt);
            /// <exclude />
            public Type OrigDiscAmt = typeof(Document.origDiscAmt);
            /// <exclude />
            public Type CuryDiscTaken = typeof(Document.curyDiscTaken);
            /// <exclude />
            public Type DiscTaken = typeof(Document.discTaken);
            /// <exclude />
            public Type CuryDiscBal = typeof(Document.curyDiscBal);
            /// <exclude />
            public Type DiscBal = typeof(Document.discBal);
            /// <exclude />
            public Type DiscTot = typeof(Document.discTot);
            /// <exclude />
            public Type CuryDiscTot = typeof(Document.curyDiscTot);
            /// <exclude />
            public Type DocDisc = typeof(Document.docDisc);
            /// <exclude />
            public Type CuryDocDisc = typeof(Document.curyDocDisc);
            /// <exclude />
            public Type CuryDiscountedDocTotal = typeof(Document.curyDiscountedDocTotal);
            /// <exclude />
            public Type DiscountedDocTotal = typeof(Document.discountedDocTotal);
            /// <exclude />
            public Type CurydiscountedPrice = typeof(Document.curyDiscountedPrice);
            /// <exclude />
            public Type DiscountedPrice = typeof(Document.discountedPrice);
            /// <exclude />
            public Type LocationID = typeof(Document.locationID);
            /// <exclude />
            public Type DocumentDate = typeof(Document.documentDate);
            /// <exclude />
            public Type CuryLinetotal = typeof(Document.curyLineTotal);
            /// <exclude />
            public Type LineTotal = typeof(Document.lineTotal);
            /// <exclude />
            public Type CuryMiscTot = typeof(Document.curyMiscTot);
            /// <exclude />
            public Type CustomerID = typeof(Document.customerID);
        }

        /// <summary>Defines the default mapping of the <see cref="Detail" /> mapped cache extension to a DAC.</summary>
        protected class DetailMapping : IBqlMapping
        {
            /// <exclude />
            public Type Extension => typeof(Detail);
            /// <exclude />
            protected Type _table;
            /// <exclude />
            public Type Table => _table;

            /// <summary>Creates the default mapping of the <see cref="Detail" /> mapped cache extension to the specified table.</summary>
            /// <param name="table">A DAC.</param>
            public DetailMapping(Type table)
            {
                _table = table;
            }
            /// <exclude />
            public Type BranchID = typeof(Detail.branchID);
            /// <exclude />
            public Type InventoryID = typeof(Detail.inventoryID);
            /// <exclude />
            public Type SiteID = typeof(Detail.siteID);
            /// <exclude />
            public Type CustomerID = typeof(Detail.customerID);
            /// <exclude />
            public Type VendorID = typeof(Detail.vendorID);
            /// <exclude />
            public Type СuryInfoID = typeof(Detail.curyInfoID);

            /// <exclude />
            public Type Quantity = typeof(Detail.quantity);
            /// <exclude />
            public Type CuryUnitPrice = typeof(Detail.curyUnitPrice);
            /// <exclude />
            public Type CuryExtPrice = typeof(Detail.curyExtPrice);
            /// <exclude />
            public Type CuryLineAmount = typeof(Detail.curyLineAmount);
            /// <exclude />
            public Type UOM = typeof(Detail.uOM);
            /// <exclude />
            public Type GroupDiscountRate = typeof(Detail.groupDiscountRate);
            /// <exclude />
            public Type DocumentDiscountRate = typeof(Detail.documentDiscountRate);

            /// <exclude />
            public Type CuryDiscAmt = typeof(Detail.curyDiscAmt);
            /// <exclude />
            public Type DiscPct = typeof(Detail.discPct);
            /// <exclude />
            public Type DiscountID = typeof(Detail.discountID);
            /// <exclude />
            public Type DiscountSequenceID = typeof(Detail.discountSequenceID);
            /// <exclude />
            public Type IsFree = typeof(Detail.isFree);
            /// <exclude />
            public Type ManualDisc = typeof(Detail.manualDisc);
            /// <exclude />
            public Type ManualPrice = typeof(Detail.manualPrice);
            /// <exclude />
            public Type LineType = typeof(Detail.lineType);
			/// <exclude />
			public Type TaxCategoryID = typeof(Detail.taxCategoryID);
	        /// <exclude />
			public Type FreezeManualDisc = typeof(Detail.freezeManualDisc);
		}

        /// <summary>Defines the default mapping of the <see cref="Discount" /> mapped cache extension to a DAC.</summary>
        protected class DiscountMapping : IBqlMapping
        {
            /// <exclude />
            public Type Extension => typeof(Discount);
            /// <exclude />
            protected Type _table;
            /// <exclude />
            public Type Table => _table;

            /// <summary>Creates the default mapping of the <see cref="Discount" /> mapped cache extension to the specified table.</summary>
            /// <param name="table">A DAC.</param>
            public DiscountMapping(Type table)
            {
                _table = table;
            }
			/// <exclude />
			public Type RecordID = typeof(Discount.recordID);
			/// <exclude />
			public Type LineNbr = typeof(Discount.lineNbr);
            /// <exclude />
            public Type SkipDiscount = typeof(Discount.skipDiscount);
            /// <exclude />
            public Type DiscountID = typeof(Discount.discountID);
            /// <exclude />
            public Type DiscountSequenceID = typeof(Discount.discountSequenceID);
            /// <exclude />
            public Type Type = typeof(Discount.type);
            /// <exclude />
            public Type DiscountableAmt = typeof(Discount.discountableAmt);
            /// <exclude />
            public Type CuryDiscountableAmt = typeof(Discount.curyDiscountableAmt);
            /// <exclude />
            public Type DiscountableQty = typeof(Discount.discountableQty);
            /// <exclude />
            public Type DiscountAmt = typeof(Discount.discountAmt);
            /// <exclude />
            public Type CuryDiscountAmt = typeof(Discount.curyDiscountAmt);
            /// <exclude />
            public Type DiscountPct = typeof(Discount.discountPct);
            /// <exclude />
            public Type FreeItemID = typeof(Discount.freeItemID);
            /// <exclude />
            public Type FreeItemQty = typeof(Discount.freeItemQty);
            /// <exclude />
            public Type IsManual = typeof(Discount.isManual);
        }

        /// <summary>Returns the mapping of the <see cref="Document" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
        /// <example>
        ///   <code title="Example" description="The following code shows the method that overrides the GetDocumentMapping() method in the implementation class. The  method overrides the default mapping of the %Document% mapped cache extension to a DAC: For the CROpportunity DAC, the CuryDiscTot field of the mapped cache extension is mapped to the curyDocDiscTot of the DAC; other fields of the extension are mapped by default." lang="CS">
        /// protected override DocumentMapping GetDocumentMapping()
        /// {
        ///    return new DocumentMapping(typeof(CROpportunity)){CuryDiscTot = typeof(CROpportunity.curyDocDiscTot) };
        /// }</code>
        /// </example>
        protected abstract DocumentMapping GetDocumentMapping();

        /// <summary>Returns the mapping of the <see cref="Detail" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
        /// <example>
        ///   <code title="Example" description="The following code shows the method that overrides the GetDetailMapping() method in the implementation class. The method overrides the default mapping of the Detail mapped cache extension to a DAC: For the CROpportunityProducts DAC, mapping of the two fields of the mapped cache extension is overriden." lang="C#">
        /// protected override DetailMapping GetDetailMapping()
        /// {
        ///    return new DetailMapping(typeof(CROpportunityProducts)) { CuryLineAmount = typeof(CROpportunityProducts.curyAmount), Quantity = typeof(CROpportunityProducts.quantity)};
        /// }</code>
        /// </example>
        protected abstract DetailMapping GetDetailMapping();

        /// <summary>Returns the mapping of the <see cref="Discount" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
        /// <example>
        ///   <code title="Example" description="The following code shows the method that overrides the GetDiscountMapping() method in the implementation class. The method returns the defaul mapping of the %Discount% mapped cache extension to the CROpportunityDiscountDetail DAC." lang="CS">
        /// protected override DiscountMapping GetDiscountMapping()
        /// {
        ///    return new DiscountMapping(typeof(CROpportunityDiscountDetail));
        /// }</code>
        /// </example>
        protected abstract DiscountMapping GetDiscountMapping();

        /// <summary>A mapping-based view of the <see cref="Document" /> data.</summary>
        public PXSelectExtension<Document> Documents;
        /// <summary>A mapping-based view of the <see cref="Detail" /> data.</summary>
        public PXSelectExtension<Detail> Details;
        /// <summary>A mapping-based view of the <see cref="Discount" /> data.</summary>
        public PXSelectExtension<Discount> Discounts;

        /// <summary>The filter for the <strong>Recalculate Prices</strong> action (<see cref="recalculateDiscountsAction" />).</summary>
        public PXFilter<RecalcDiscountsParamFilter> recalcdiscountsfilter;

        /// <summary>The <strong>Recalculate Prices</strong> action.</summary>
        public PXAction<TPrimary> recalculateDiscountsAction;

	    public DiscountEngine<Detail, Discount> DiscountEngineGraph => DiscountEngineProvider.GetEngineFor<Detail, Discount>();

		[PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable RecalculateDiscountsAction(PXAdapter adapter)
        {
            if (!adapter.ExternalCall || recalcdiscountsfilter.AskExt() == WebDialogResult.OK)
            {
                DiscountEngineGraph.RecalculatePricesAndDiscounts(
                    Details.Cache,
                    Details,
                    Details.Current,
                    Discounts,
                    Documents.Current.LocationID ?? 0,
                    Documents.Current.DocumentDate,
                    recalcdiscountsfilter.Current,
					DiscountEngine.DefaultARDiscountCalculationParameters);
				//IsDirty for extension is not passed to graph.
	            if (Base.IsDirty == false && Details.Cache.IsDirty)
	            {
		            //Documents.Cache.Update(this.Documents.Current);
					var origDocument = Documents.Cache.GetMain(this.Documents.Current);
		            Base.Caches[origDocument.GetType()].Update(origDocument);
					
				}
            }
            return adapter.Get();
        }


        /// <summary>The method overrides the <tt>Persist</tt> method of the base graph.</summary>
        [PXOverride]
        public virtual void Persist(Action del)
        {
            if (Documents.Current != null && !Discounts.Any() && (Documents.Current.CuryDiscTot ?? 0m) != 0m)
            {
                AddDiscount(Documents.Cache, Documents.Current);
			}

			DiscountEngineGraph.ValidateDiscountDetails(Discounts);

            del();
        }
        /// <summary>Sets the default discount account and subaccount for the specified detail line. You must override this method in the implementation class for a particular
        /// graph.</summary>
        /// <param name="discount">The detail line.</param>
        protected abstract void DefaultDiscountAccountAndSubAccount(Detail discount);

        /// <summary>You must override this property in the implementation class for a particular graph.</summary>
        /// <example>
        ///   <code title="Example" description="The following code shows sample implemetation of the method in the implementation class for a particular graph." lang="CS">
        /// protected override bool AddDocumentDiscount =&gt; true;</code>
        /// </example>
        protected abstract bool AddDocumentDiscount
        {
            get;
        }

        /// <summary>Adds the discount to the specified document.</summary>
        public virtual void AddDiscount(PXCache sender, Document row)
        {
	        if (GetDetailMapping().FreezeManualDisc == typeof(Detail.freezeManualDisc))
		        return;

			Detail discount = Details.Select();
            if (discount == null)
            {
                discount = (Detail)Details.Cache.CreateInstance();
                discount.FreezeManualDisc = true;
                discount = (Detail)Details.Cache.Insert(discount);
            }

            Detail old_row = (Detail)Details.Cache.CreateCopy(discount);

            discount.CuryLineAmount = row.CuryDiscTot;
            discount.TaxCategoryID = null;

            DefaultDiscountAccountAndSubAccount(discount);

            if (Details.Cache.GetStatus(discount) == PXEntryStatus.Notchanged)
            {
                Details.Cache.SetStatus(discount, PXEntryStatus.Updated);
            }

            discount.ManualDisc = true; //escape SOManualDiscMode.RowUpdated
            Details.Cache.RaiseRowUpdated(discount, old_row);

            decimal auotDocDisc = DiscountEngineGraph.GetTotalGroupAndDocumentDiscount(Discounts);
            if (auotDocDisc == discount.CuryLineAmount)
            {
                discount.ManualDisc = false;
            }
        }

		#region Detail handlers        
		/// <summary>The FieldUpdated event handler for the <see cref="Detail.DiscountID" /> field. When the DiscountID field value is changed, the discount is recalculated for the current line.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.FieldUpdated<Detail, Detail.discountID> e)
        {
            var doc = Documents.Current;

            if (e.ExternalCall && e.Row != null)
            {
                DiscountEngineGraph.UpdateManualLineDiscount(Details.Cache,
                    Details, e.Row, Discounts, doc.BranchID, doc.LocationID ?? 0,
                    doc.DocumentDate ?? (new PXGraph()).Accessinfo.BusinessDate,
					DiscountEngine.DefaultARDiscountCalculationParameters);

                Details.Cache.RaiseFieldUpdated<Detail.curyDiscAmt>(e.Row, 0);
            }
        }
	    /// <summary>The FieldUpdated event handler for the <see cref="Detail.IsFree" /> field. When the IsFree field value is changed, the price and discount are recalculated for the current line.</summary>
	    /// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.FieldUpdated<Detail, Detail.isFree> e)
	    {
			if (e.Row != null)
			{
				if (e.Row.IsFree == true)
				{
					e.Cache.SetValueExt<Detail.curyUnitPrice>(e.Row, 0m);
					e.Cache.SetValueExt<Detail.discPct>(e.Row, 0m);
					e.Cache.SetValueExt<Detail.curyDiscAmt>(e.Row, 0m);
					if (e.ExternalCall)
						e.Cache.SetValueExt<Detail.manualDisc>(e.Row, true);
				}
				else
				{
					e.Cache.SetDefaultExt<Detail.curyUnitPrice>(e.Row);
				}
			}
		}

        /// <summary>The RowSelected event handler for the <see cref="Detail" /> mapped cache extension.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.RowSelected<Detail> e)
        {
            if (e.Row == null) return;

            PXUIFieldAttribute.SetEnabled<Detail.manualDisc>(Details.Cache, e.Row, e.Row.IsFree != true);

            bool autoFreeItem = e.Row.ManualDisc != true && e.Row.IsFree == true;

            if (autoFreeItem)
            {
                PXUIFieldAttribute.SetEnabled(Details.Cache, e.Row, false);
                PXUIFieldAttribute.SetEnabled<Detail.siteID>(Details.Cache, e.Row);
            }
            PXUIFieldAttribute.SetEnabled<Detail.quantity>(Details.Cache, e.Row, !autoFreeItem);
            PXUIFieldAttribute.SetEnabled<Detail.isFree>(Details.Cache, e.Row, !autoFreeItem && e.Row.InventoryID != null);
        }

		/// <summary>The RowUpdated event handler for the <see cref="Detail" /> mapped cache extension. When the value of any field of <see cref="Detail"/> is changed, the discount is recalulated for the current line.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.RowUpdated<Detail> e)
		{
			if (!Details.Cache.ObjectsEqual<Detail.branchID>(e.Row, e.OldRow) && !Details.Cache.ObjectsEqual<Detail.inventoryID>(e.Row, e.OldRow) ||
					!Details.Cache.ObjectsEqual<Detail.siteID>(e.Row, e.OldRow) || !Details.Cache.ObjectsEqual<Detail.quantity>(e.Row, e.OldRow) ||
					!Details.Cache.ObjectsEqual<Detail.curyUnitPrice>(e.Row, e.OldRow) || !Details.Cache.ObjectsEqual<Detail.curyExtPrice>(e.Row, e.OldRow) ||
					!Details.Cache.ObjectsEqual<Detail.curyDiscAmt>(e.Row, e.OldRow) || !Details.Cache.ObjectsEqual<Detail.discPct>(e.Row, e.OldRow) ||
					!Details.Cache.ObjectsEqual<Detail.manualDisc>(e.Row, e.OldRow) || !Details.Cache.ObjectsEqual<Detail.discountID>(e.Row, e.OldRow))
			{
				if (e.Row.ManualDisc != true)
				{
					if (e.OldRow.ManualDisc == true)//Manual Discount Unckecked
					{
						if (e.Row.IsFree == true)
						{
							ResetQtyOnFreeItem(e.Cache.Graph, e.Row);
						}
					}
					if (e.Row.IsFree == true)
					{
						DiscountEngineGraph.ClearDiscount(e.Cache, e.Row);
					}
				}

				RecalculateDiscounts(e.Cache, e.Row, e.Row);
			}
        }

		/// <summary>The RowInserted event handler for the <see cref="Detail" /> mapped cache extension. When a <see cref="Detail"/> line is inserted, the discount is recalulated.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.RowInserted<Detail> e)
        {
            if (e.Row != null && e.Row.ManualDisc != true)
            {
                RecalculateDiscounts(e.Cache, e.Row, e.Row);
            }
        }

		/// <summary>The RowDeleted event handler for the <see cref="Detail" /> mapped cache extension. When a <see cref="Detail"/> line is deleted, the discount is recalulated.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.RowDeleted<Detail> e)
        {
            bool autoFreeItem = e.Row.ManualDisc != true && e.Row.IsFree == true;
            if (autoFreeItem) return;

            var documentCache = Documents.Cache;
            if (documentCache.Current != null && documentCache.GetStatus(documentCache.Current) != PXEntryStatus.Deleted)
            {
                var doc = Documents.Current;
                DiscountEngineGraph.RecalculateGroupAndDocumentDiscounts(Details.Cache,
                    Details, null, Discounts, doc.BranchID, doc.LocationID ?? 0, doc.DocumentDate ?? (new PXGraph()).Accessinfo.BusinessDate, DiscountEngine.DefaultARDiscountCalculationParameters);
                RecalculateTotalDiscount();
                RefreshFreeItemLines(Details.Cache);
            }
        }
		#endregion

		#region Document handlers
		/// <summary>The RowUpdated event handler for the <see cref="Document" /> mapped cache extension.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.RowUpdated<Document> e)
        {
            if (e.Row == null) return;

            if (e.ExternalCall && (!Documents.Cache.ObjectsEqual<Document.documentDate>(e.OldRow, e.Row)))
            {
                DiscountEngineGraph.AutoRecalculatePricesAndDiscounts(Details.Cache,
                    Details, null, Discounts, e.Row.LocationID ?? 0, e.Row.DocumentDate ?? (new PXGraph()).Accessinfo.BusinessDate, DiscountEngine.DefaultARDiscountCalculationParameters);
            }

			if (e.ExternalCall && Documents.Cache.GetStatus(e.Row) != PXEntryStatus.Deleted && !Documents.Cache.ObjectsEqual<Document.curyDiscTot>(e.OldRow, e.Row))
			{
				var current = Documents.Current;
				DiscountEngineGraph.CalculateDocumentDiscountRate(Details.Cache,
					Details, null, null, Discounts, current.CuryLineTotal ?? 0m, current.CuryDiscTot ?? 0m);
			}

            if (e.Row.CustomerID != null && e.Row.CuryDiscTot != null && e.Row.CuryDiscTot > 0 && e.Row.CuryLineTotal != null && e.Row.CuryMiscTot != null && (e.Row.CuryLineTotal > 0 || e.Row.CuryMiscTot > 0))
            {
                decimal discountLimit = DiscountEngineGraph.GetDiscountLimit(Documents.Cache, e.Row.CustomerID);
                if (((e.Row.CuryLineTotal + e.Row.CuryMiscTot) / 100 * discountLimit) < e.Row.CuryDiscTot)
                    PXUIFieldAttribute.SetWarning<Document.curyDiscTot>(Documents.Cache, e.Row, string.Format(Messages.DocDiscountExceedLimit, discountLimit));
            }

            if (!Documents.Cache.ObjectsEqual<Document.locationID>(e.OldRow, e.Row) || !Documents.Cache.ObjectsEqual<Document.documentDate>(e.OldRow, e.Row))
            {
                RecalcDiscountsParamFilter recalcFilter =
                    new RecalcDiscountsParamFilter
                    {
                        OverrideManualDiscounts = false,
                        OverrideManualPrices = false,
                        RecalcDiscounts = true,
                        RecalcUnitPrices = true,
                        RecalcTarget = RecalcDiscountsParamFilter.AllLines
                    };
                DiscountEngineGraph.RecalculatePricesAndDiscounts(Details.Cache,
                    Details, null, Discounts, e.Row.LocationID ?? 0, e.Row.DocumentDate ?? (new PXGraph()).Accessinfo.BusinessDate, recalcFilter, DiscountEngine.DefaultARDiscountCalculationParameters);
                RecalculateTotalDiscount();

            }
        }
        #endregion

        #region DiscountDetail events
        /// <summary>The RowSelected event handler for the <see cref="Discount" /> mapped cache extension.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.RowSelected<Discount> e)
        {
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<Discount.skipDiscount>(Discounts.Cache, e.Row, e.Row.DiscountID != null && e.Row.Type != DiscountType.ExternalDocument);
				PXUIFieldAttribute.SetEnabled<Discount.discountID>(Discounts.Cache, e.Row, e.Row.Type != DiscountType.ExternalDocument);
				PXUIFieldAttribute.SetEnabled<Discount.discountSequenceID>(Discounts.Cache, e.Row, e.Row.Type != DiscountType.ExternalDocument);
				PXUIFieldAttribute.SetEnabled<Discount.curyDiscountAmt>(Discounts.Cache, e.Row, e.Row.Type == DiscountType.ExternalDocument || e.Row.Type == DiscountType.Document);
				PXUIFieldAttribute.SetEnabled<Discount.discountPct>(Discounts.Cache, e.Row, e.Row.Type == DiscountType.ExternalDocument || e.Row.Type == DiscountType.Document);
			}

            PXDefaultAttribute.SetPersistingCheck<Discount.discountID>(Discounts.Cache, e.Row, PXPersistingCheck.Nothing);

            if (Documents.Current != null)
                Documents.Cache.SetValueExt<Document.curyDocDisc>(Documents.Current, DiscountEngineGraph.GetTotalGroupAndDocumentDiscount(Discounts, true));
        }

        /// <summary>The RowInserted event handler for the <see cref="Discount" /> mapped cache extension.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.RowInserted<Discount> e)
        {
            if (!DiscountEngineGraph.IsInternalDiscountEngineCall && e.Row != null)
            {
				if (e.Row.DiscountID != null)
				{
					DiscountEngineGraph.InsertManualDocGroupDiscount(Details.Cache, Details, Discounts, e.Row, e.Row.DiscountID, null,
						Documents.Current.BranchID, Documents.Current.LocationID ?? 0, Documents.Current.DocumentDate, DiscountEngine.DefaultARDiscountCalculationParameters);
					RefreshTotalsAndFreeItems(Discounts.Cache);
					(Discounts.Cache as PXModelExtension<Discount>)?.UpdateExtensionMapping(e.Row);
				}

				if (DiscountEngineGraph.SetExternalManualDocDiscount(Details.Cache, Details, Discounts, e.Row, null, DiscountEngine.DefaultARDiscountCalculationParameters))
				{
					RefreshTotalsAndFreeItems(Discounts.Cache);
					(Discounts.Cache as PXModelExtension<Discount>)?.UpdateExtensionMapping(e.Row);
				}
			}
        }
        /// <summary>The RowUpdated event handler for the <see cref="Discount" /> mapped cache extension.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.RowUpdated<Discount> e)
        {
            if (!DiscountEngineGraph.IsInternalDiscountEngineCall && e.Row != null)
            {
                if (!Discounts.Cache.ObjectsEqual<Discount.skipDiscount>(e.Row, e.OldRow))
                {
                    DiscountEngineGraph.UpdateDocumentDiscount(Details.Cache, Details, Discounts, Documents.Current.BranchID, Documents.Current.LocationID ?? 0, Documents.Current.DocumentDate, e.Row.Type != DiscountType.Document, DiscountEngine.DefaultARDiscountCalculationParameters);
                    RefreshTotalsAndFreeItems(Discounts.Cache);
                    (Discounts.Cache as PXModelExtension<Discount>)?.UpdateExtensionMapping(e.Row);
                }
                if (!Discounts.Cache.ObjectsEqual<Discount.discountID>(e.Row, e.OldRow) || !Discounts.Cache.ObjectsEqual<Discount.discountSequenceID>(e.Row, e.OldRow))
                {
                    e.Row.IsManual = true;
                    DiscountEngineGraph.InsertManualDocGroupDiscount(Details.Cache, Details, Discounts, e.Row, e.Row.DiscountID, Discounts.Cache.ObjectsEqual<Discount.discountID>(e.Row, e.OldRow) ? e.Row.DiscountSequenceID : null,
                        Documents.Current.BranchID, Documents.Current.LocationID ?? 0, Documents.Current.DocumentDate, DiscountEngine.DefaultARDiscountCalculationParameters);
                    RefreshTotalsAndFreeItems(Discounts.Cache);
                    (Discounts.Cache as PXModelExtension<Discount>)?.UpdateExtensionMapping(e.Row);
                }

				if (DiscountEngineGraph.SetExternalManualDocDiscount(Details.Cache, Details, Discounts, e.Row, e.OldRow, DiscountEngine.DefaultARDiscountCalculationParameters))
				{
					RefreshTotalsAndFreeItems(Discounts.Cache);
					(Discounts.Cache as PXModelExtension<Discount>)?.UpdateExtensionMapping(e.Row);
				}
			}
        }
        /// <summary>The RowDeleted event handler for the <see cref="Discount" /> mapped cache extension.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.RowDeleted<Discount> e)
        {
            if (!DiscountEngineGraph.IsInternalDiscountEngineCall && e.Row != null)
            {
                DiscountEngineGraph.UpdateDocumentDiscount(Details.Cache, Details, Discounts, Documents.Current.BranchID, Documents.Current.LocationID ?? 0, Documents.Current.DocumentDate, (e.Row.Type != null && e.Row.Type != DiscountType.Document && e.Row.Type != DiscountType.ExternalDocument), DiscountEngine.DefaultARDiscountCalculationParameters);
                (Discounts.Cache as PXModelExtension<Discount>)?.UpdateExtensionMapping(e.Row);
            }

			RefreshTotalsAndFreeItems(Discounts.Cache);
        }

		protected virtual void _(Events.RowPersisting<Discount> e)
		{
			bool isExternalDiscount = e.Row.Type == DiscountType.ExternalDocument;

			PXDefaultAttribute.SetPersistingCheck<SOOrderDiscountDetail.discountID>(Discounts.Cache, e.Row, isExternalDiscount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<SOOrderDiscountDetail.discountSequenceID>(Discounts.Cache, e.Row, isExternalDiscount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
		}

		private void RecalculateTotalDiscount()
        {
            if (Documents.Current != null)
            {
                Document old_row = (Document)Documents.Cache.CreateCopy(Documents.Current);
                Documents.Cache.SetValueExt<Document.curyDiscTot>(Documents.Current, DiscountEngineGraph.GetTotalGroupAndDocumentDiscount(Discounts));
                Documents.Cache.RaiseRowUpdated(Documents.Current, old_row);
            }
        }


        /// <summary>The FieldVerifying2 event handler for the <see cref="Discount.DiscountSequenceID" /> field.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.FieldVerifying<Discount, Discount.discountSequenceID> e)
        {
            if (!e.ExternalCall)
            {
                e.Cancel = true;
            }
        }
        /// <summary>The FieldVerifying2 event handler for the <see cref="Discount.DiscountID" /> field.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.FieldVerifying<Discount, Discount.discountID> e)
        {
            if (!e.ExternalCall)
            {
                e.Cancel = true;
            }
        }
        #endregion

        #region Helper methods

        private void ResetQtyOnFreeItem(PXGraph graph, Detail line)
        {

            PXView view = new PXView(graph, false, Discounts.View.BqlSelect.WhereAnd<Where<Discount.freeItemID, Equal<Required<Discount.freeItemID>>>>());
            decimal? qtyTotal = 0;
            foreach (IDiscountDetail item in view.SelectMulti(line.InventoryID))
            {
                if (item.SkipDiscount != true && item.FreeItemID != null && item.FreeItemQty != null && item.FreeItemQty.Value > 0)
                {
                    qtyTotal += item.FreeItemQty.Value;
                }
            }

            Details.Cache.SetValueExt<Detail.quantity>(line, qtyTotal);
        }

        private void RefreshFreeItemLines(PXCache sender)
        {
            if (sender.Graph.IsImport && !sender.Graph.IsMobile)
                return;

            Dictionary<int, decimal> groupedByInventory = new Dictionary<int, decimal>();
            //PXSelectBase<Discount> select = new PXSelect<Discount,
            //	Where<Discount.orderType, Equal<Current<CROpportunity.orderType>>,
            //	And<Discount.orderNbr, Equal<Current<CROpportunity.orderNbr>>,
            //	//And<Discount.skipDiscount, NotEqual<boolTrue>>
            //	>>>(_Graph);

            foreach (Discount item in Discounts.Select())
            {
                if (item.FreeItemID != null && item.SkipDiscount != true)
                {
                    if (groupedByInventory.ContainsKey(item.FreeItemID.Value))
                    {
                        groupedByInventory[item.FreeItemID.Value] += item.FreeItemQty ?? 0;
                    }
                    else
                    {
                        groupedByInventory.Add(item.FreeItemID.Value, item.FreeItemQty ?? 0);
                    }

                }

            }

            bool freeItemChanged = false;

            #region Delete Unvalid FreeItems
            foreach (Detail line in Details.Cache.Cached)
            {
                var status = Details.Cache.GetStatus(line);
                if (status == PXEntryStatus.Deleted || status == PXEntryStatus.InsertedDeleted)
                    continue;

                if (line.IsFree != true)
                    continue;

                if (line.ManualDisc == false && line.InventoryID != null)
                {
                    if (groupedByInventory.ContainsKey(line.InventoryID.Value))
                    {
                        if (groupedByInventory[line.InventoryID.Value] == 0)
                        {
                            Details.Cache.Delete(line);
                            freeItemChanged = true;
                        }
                    }
                    else
                    {
                        Details.Cache.Delete(line);
                        freeItemChanged = true;
                    }
                }
            }

            #endregion

            foreach (KeyValuePair<int, decimal> kv in groupedByInventory)
            {
                object source;
                var freeLine = GetFreeLineByItemID(kv.Key, out source);

                if (freeLine == null)
                {
                    if (kv.Value > 0)
                    {
                        Detail line = new Detail
                        {
                            InventoryID = kv.Key,
                            IsFree = true,
                            Quantity = kv.Value
                        };
                        //line.ShipComplete = SOShipComplete.CancelRemainder;
                        //Need to ckeck
                        Details.Cache.Insert(line);


                        freeItemChanged = true;
                    }
                }
                else if(freeLine.Quantity != kv.Value)
                {
                    Detail copy = (Detail)Details.Cache.CreateCopy(freeLine);
                    copy.Quantity = kv.Value;
                    Details.Cache.Update(copy);
                    freeItemChanged = true;
                }
            }

            if (freeItemChanged)
            {
                Details.View.RequestRefresh();
            }
        }

        private Detail GetFreeLineByItemID(int? inventoryID, out object source)
        {
            source = null;
            foreach (Detail line in Details.Cache.Cached)
            {

                var status = Details.Cache.GetStatus(line);
                if (status == PXEntryStatus.Deleted || status == PXEntryStatus.InsertedDeleted)
                    continue;

                if (line.IsFree == true && line.InventoryID == inventoryID && line.ManualDisc != true)
                {
                    source = line;
                    return line;
                }
            }
            return null;
        }

        /// <summary>Recalculates discounts for the specified detail line.</summary>
        /// <param name="sender">A cache object.</param>
        /// <param name="line">The line for which the discounts should be recalculated.</param>
        protected virtual void RecalculateDiscounts(PXCache sender, Detail line, object source)
        {
            if (line.InventoryID != null && line.Quantity != null && line.CuryLineAmount != null && line.IsFree != true)
            {
                Document doc = Documents.Current;

                DiscountEngineGraph.SetDiscounts(Details.Cache,
                    Details, line, Discounts, doc.BranchID ?? 0, doc.LocationID ?? 0, doc.CuryID, doc.DocumentDate ?? Base.Accessinfo.BusinessDate, null, DiscountEngine.DefaultARDiscountCalculationParameters);

                RecalculateTotalDiscount();
                RefreshFreeItemLines(sender);
				RecalculateDetailsDiscount(Details.Cache);
			}
        }

        public virtual void RefreshTotalsAndFreeItems(PXCache sender)
        {
            RecalculateTotalDiscount();
            RefreshFreeItemLines(sender);
			RecalculateDetailsDiscount(Details.Cache);
        }
		
		private static bool IsInstanceOfGenericType(Type genericType, object instance)
		{
			Type type = instance.GetType();
			while (type != null)
			{
				if (type.IsGenericType &&
					type.GetGenericTypeDefinition() == genericType)
				{
					return true;
				}
				type = type.BaseType;
			}
			return false;
		}

		private void RecalculateDetailsDiscount(PXCache sender)
		{
			foreach (Detail det in Details.Select())
			{
				Details.Cache.RaiseFieldUpdated<Detail.documentDiscountRate>(det, det.DocumentDiscountRate);
			}

			var tgraph = sender.Graph.GetType();
			var extensions = tgraph.GetField("Extensions", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(sender.Graph) as PXGraphExtension[];
			var ext = extensions?.FirstOrDefault(extension => IsInstanceOfGenericType(typeof(Extensions.SalesTax.TaxBaseGraph<,>), extension));

			if (ext == null) return;

			var method = ext.GetType().GetMethod("RecalcTaxes", BindingFlags.NonPublic | BindingFlags.Instance);
			method?.Invoke(ext, null);
		}

        #endregion
    }
}

