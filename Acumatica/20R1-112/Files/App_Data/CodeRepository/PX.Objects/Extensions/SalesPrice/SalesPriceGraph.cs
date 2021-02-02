using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Objects.AR;
using PX.Objects.IN;

namespace PX.Objects.Extensions.SalesPrice
{
    /// <summary>A generic graph extension that defines the functionality for working with multiple price lists.</summary>
    /// <typeparam name="TGraph">A <see cref="PX.Data.PXGraph" /> type.</typeparam>
    /// <typeparam name="TPrimary">A DAC (a <see cref="PX.Data.IBqlTable" /> type).</typeparam>
    public abstract class SalesPriceGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
            where TGraph : PXGraph
            where TPrimary : class, IBqlTable, new()
    {

        /// <summary>A class that defines the default mapping of the <see cref="Document" /> mapped cache extension to a DAC.</summary>
        protected class DocumentMapping : IBqlMapping
        {
            /// <exclude />
            protected Type _extension = typeof(Document);
            /// <exclude />
            public Type Extension => _extension;

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
            public Type BAccountID = typeof(Document.bAccountID);
            /// <exclude />
            public Type CuryInfoID = typeof(Document.curyInfoID);
            /// <exclude />
            public Type DocumentDate = typeof(Document.documentDate);

        }

        /// <summary>A class that defines the default mapping of the <see cref="Detail" /> mapped cache extension to a DAC.</summary>
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
            public Type Descr = typeof(Detail.descr);
            /// <exclude />
            public Type SiteID = typeof(Detail.siteID);
            /// <exclude />
            public Type UOM = typeof(Detail.uOM);
            /// <exclude />
            public Type Quantity = typeof(Detail.quantity);
            /// <exclude />
            public Type CuryUnitPrice = typeof(Detail.curyUnitPrice);
            /// <exclude />
            public Type CuryLineAmount = typeof(Detail.curyLineAmount);
            /// <exclude />
            public Type ManualPrice = typeof(Detail.manualPrice);
            /// <exclude />
            public Type IsFree = typeof(Detail.isFree);
        }

        /// <summary>A class that defines the default mapping of the <see cref="PriceClassSource" /> mapped cache extension to a DAC.</summary>
        protected class PriceClassSourceMapping : IBqlMapping
        {
            /// <exclude />
            public Type Extension => typeof(PriceClassSource);
            /// <exclude />
            protected Type _table;
            /// <exclude />
            public Type Table => _table;

            /// <summary>Creates the default mapping of the <see cref="PriceClassSource" /> mapped cache extension to the specified table.</summary>
            /// <param name="table">A DAC.</param>
            public PriceClassSourceMapping(Type table)
            {
                _table = table;
            }
            /// <exclude />
            public Type PriceClassID = typeof(PriceClassSource.priceClassID);
        }

        /// <summary>Returns the mapping of the <see cref="Document" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
        /// <example>
        ///   <code title="Example" description="The following code shows the method that overrides the GetDocumentMapping() method in the implementation class. The method returns the default mapping of the Document mapped cache extension to the CROpportunity DAC." lang="CS">
        /// protected override DocumentMapping GetDocumentMapping()
        /// {
        ///       return new DocumentMapping(typeof(CROpportunity));
        /// }</code>
        /// </example>
        protected abstract DocumentMapping GetDocumentMapping();

        /// <summary>Returns the mapping of the <see cref="Detail" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
        /// <example>
        ///   <code title="Example" description="The following code shows the method that overrides the GetDetailMapping() method in the implementation class. The method overrides the default mapping of the Detail mapped cache extension to a DAC: For the CROpportunityProducts DAC, the mapping of the CuryLineAmount and Descr fields is changed." lang="CS">
        /// protected override DetailMapping GetDetailMapping()
        /// {
        ///     return new DetailMapping(typeof(CROpportunityProducts)) { CuryLineAmount = typeof(CROpportunityProducts.curyAmount), Descr = typeof(CROpportunityProducts.transactionDescription)};
        /// }</code>
        /// </example>
        protected abstract DetailMapping GetDetailMapping();

        /// <summary>Returns the mapping of the <see cref="PriceClassSource" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
        /// <example>
        ///   <code title="Example" description="The following code shows the method that overrides the GetPriceClassSourceMapping() method in the implementation class. The method overrides the default mapping of the PriceClassSource mapped cache extension to a DAC: For the Location DAC, the PriceClassID field of the mapped cache extension is mapped to the cPriceClassID field of the DAC; other fields of the extension are mapped by default." lang="CS">
        /// protected override PriceClassSourceMapping GetPriceClassSourceMapping()
        /// {
        ///    return new PriceClassSourceMapping(typeof(Location)) {PriceClassID = typeof(Location.cPriceClassID)};
        /// }</code>
        /// </example>
        protected abstract PriceClassSourceMapping GetPriceClassSourceMapping();

        /// <summary>A mapping-based view of the <see cref="Document" /> data.</summary>
        public PXSelectExtension<Document> Documents;
        /// <summary>A mapping-based view of the <see cref="Detail" /> data.</summary>
        public PXSelectExtension<Detail> Details;
        /// <summary>A mapping-based view of the <see cref="PriceClassSource" /> data.</summary>
        public PXSelectExtension<PriceClassSource> PriceClassSource;

        /// <summary>The current record of the <see cref="CM.CurrencyInfo">CurrencyInfo</see> object associated with the document</summary>
        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<Document.curyInfoID>>>> currencyinfo;

       
        /// <summary>Returns the sales price for the specified detail row.</summary>
        /// <param name="row">A detail line.</param>
        /// <param name="sender">A cache object.</param>
        protected virtual decimal GetPrice(PXCache sender, Detail row)
        {
            string customerPriceClass = ARPriceClass.EmptyPriceClass;
            PriceClassSource source = PriceClassSource.Select();
            if (!string.IsNullOrEmpty(source?.PriceClassID))
                customerPriceClass = source.PriceClassID;

            Document doc = Documents.Current;
            CurrencyInfo info = SelectCurrencyInfo(doc);
            return
                   ARSalesPriceMaint.CalculateSalesPrice(sender, customerPriceClass, doc.BAccountID,
                       row.InventoryID, row.SiteID, info.GetCM(),
                  row.UOM, row.Quantity, doc.DocumentDate ?? sender.Graph.Accessinfo.BusinessDate.Value, row.CuryUnitPrice) ?? 0m;
        }

        /// <summary>Returns the current <see cref="CM.CurrencyInfo" /> record associated with the specified document.</summary>
        /// <param name="doc">A document.</param>
        protected virtual CurrencyInfo SelectCurrencyInfo(Document doc)
        {
            if (doc.CuryInfoID == null) return null;
            CurrencyInfo result = currencyinfo.Cache.Current as CurrencyInfo;                     
            return result != null && doc.CuryInfoID == result.CuryInfoID ? result : currencyinfo.SelectSingle();
        }

        /// <summary>The FieldUpdated2 event handler for the <see cref="Detail.InventoryID" /> field. When the InventoryID field value is changed, <see cref="Detail.Descr" />, <see cref="Detail.UOM" />,
        /// <see cref="Detail.Quantity" /> are assigned the default values.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.FieldUpdated<Detail, Detail.inventoryID> e)
        {
            if (e.ExternalCall)
                e.Cache.SetValue<Detail.curyUnitPrice>(e.Row, 0m);

            e.Cache.SetDefaultExt<Detail.descr>(e.Row);
            e.Cache.RaiseExceptionHandling<Detail.uOM>(e.Row, null, null);
            e.Cache.SetDefaultExt<Detail.uOM>(e.Row);
            e.Cache.SetDefaultExt<Detail.quantity>(e.Row);
        }

        /// <summary>The FieldDefaulting2 event handler for the <see cref="Detail.Descr" /> field.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.FieldDefaulting<Detail, Detail.descr> e)
        {
            Document doc = Documents.Current;
            Customer customer = PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(Base, doc.BAccountID);
            InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<Detail.inventoryID>(Details.Cache, e.Row);

            e.NewValue = PXDBLocalizableStringAttribute.GetTranslation(Base.Caches[typeof(InventoryItem)],
                item, nameof(InventoryItem.Descr), customer?.LocaleName);
        }

        /// <summary>The FieldDefaulting2 event handler for the <see cref="Detail.CuryUnitPrice" /> field.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.FieldDefaulting<Detail, Detail.curyUnitPrice> e)
        {
            Detail row = e.Row;
            if (row != null)
            {
                e.NewValue = (decimal?) (!Base.IsCopyPasteContext ? GetPrice(e.Cache, row) : 0m) ?? 0m;              
            }
        }

        /// <summary>The FieldUpdated2 event handler for the <see cref="Detail.UOM" /> field. When the UOM field value is changed, <see cref="Detail.CuryUnitPrice" /> is assinged a new value.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.FieldUpdated<Detail, Detail.uOM> e)
        {
            if (e.Row.ManualPrice != true && e.Row.IsFree != true && !e.Cache.Graph.IsCopyPasteContext)
            {
                e.Cache.SetDefaultExt<Detail.curyUnitPrice>(e.Row);
            }
        }
        /// <summary>The FieldUpdated2 event handler for the <see cref="Detail.Quantity" /> field. When the Quantity field value is changed, <see cref="Detail.CuryUnitPrice" /> is assinged a new value.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.FieldUpdated<Detail, Detail.quantity> e)
        {
            if (e.Row.ManualPrice != true && e.Row.IsFree != true && !e.Cache.Graph.IsCopyPasteContext)
            {
                e.Cache.SetDefaultExt<Detail.curyUnitPrice>(e.Row);
            }
        }
        /// <summary>The FieldUpdated2 event handler for the <see cref="Detail.IsFree" /> field. When the IsFree field value is changed, <see cref="Detail.CuryUnitPrice" /> is assinged a new value.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.FieldUpdated<Detail, Detail.isFree> e)
        {
            if (e.Row != null)
            {
                if (e.Row.IsFree == true)
                {
                    e.Cache.SetValueExt<Detail.curyUnitPrice>(e.Row, 0m);
                }
                else
                {
                    if (e.Row.ManualPrice != true && !e.Cache.Graph.IsCopyPasteContext)
                    {
                        e.Cache.SetDefaultExt<Detail.curyUnitPrice>(e.Row);
                    }
                }
            }
        }

        /// <summary>The RowUpdated event handler for the <see cref="Detail" /> mapped cache extension.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.RowUpdated<Detail> e)
        {

            if ((e.ExternalCall || e.Cache.Graph.IsImport)
                    && e.Cache.ObjectsEqual<Detail.inventoryID>(e.Row, e.OldRow) && e.Cache.ObjectsEqual<Detail.uOM>(e.Row, e.OldRow)
                    && e.Cache.ObjectsEqual<Detail.quantity>(e.Row, e.OldRow) && e.Cache.ObjectsEqual<Detail.branchID>(e.Row, e.OldRow)
                    && (!e.Cache.ObjectsEqual<Detail.curyUnitPrice>(e.Row, e.OldRow) || !e.Cache.ObjectsEqual<Detail.curyLineAmount>(e.Row, e.OldRow)))
            {
                e.Cache.SetValueExt<Detail.manualPrice>(e.Row, true);
            }
        }

        /// <summary>The RowSelected event handler for the <see cref="Detail" /> mapped cache extension.</summary>
        /// <param name="e">Parameters of the event</param>
        protected virtual void _(Events.RowSelected<Detail> e)
        {
            PXUIFieldAttribute.SetEnabled<Detail.curyUnitPrice>(e.Cache, e.Row, e.Row?.IsFree != true);
        }
    }
}
