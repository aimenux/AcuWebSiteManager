using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Linq;

namespace PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel
{
    /// <summary>
    /// This class implements graph extension to use special dialogs called Smart Panel to perform "ADD PO LINE" (Screen AP301000)
    /// </summary>
    [Serializable]
    public class AddPOOrderLineExtension : PXGraphExtension<LinkLineExtension, APInvoiceEntry>
    {
        #region Data Members

        [PXCopyPasteHiddenView]
        [PXViewName(Messages.POLine)]
        public PXSelect<POLineRS> poorderlineslist;

        public PXFilter<POOrderFilter> orderfilter;

        #endregion

        #region Initialize

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
        }

        public override void Initialize()
        {
            base.Initialize();

            poorderlineslist.Cache.AllowDelete = false;
            poorderlineslist.Cache.AllowInsert = false;
        }

        #endregion

        #region Actions

        public PXAction<APInvoice> addPOOrderLine;
        [PXUIField(DisplayName = Messages.AddPOOrderLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true, FieldClass = "DISTR")]
        [PXLookupButton]
        [APMigrationModeDependentActionRestriction(
            restrictInMigrationMode: true,
            restrictForRegularDocumentInMigrationMode: true,
            restrictForUnreleasedMigratedDocumentInNormalMode: true)]
        public virtual IEnumerable AddPOOrderLine(PXAdapter adapter)
        {
            Base.checkTaxCalcMode();
            if (Base.Document.Current != null &&
                Base.Document.Current.DocType == APDocType.Invoice &&
                Base.Document.Current.Released == false &&
                Base.Document.Current.Prebooked == false &&
                poorderlineslist.AskExt(
                    (graph, view) =>
                    {
                        orderfilter.Cache.ClearQueryCache();
                        orderfilter.View.Clear();
                        orderfilter.Cache.Clear();

                        poorderlineslist.Cache.ClearQueryCache();
                        poorderlineslist.View.Clear();
                        poorderlineslist.Cache.Clear();
                    }, true) == WebDialogResult.OK)
            {
                return AddPOOrderLine2(adapter);
            }
            return adapter.Get();
        }
        public PXAction<APInvoice> addPOOrderLine2;
        [PXUIField(DisplayName = Messages.AddPOOrderLine, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        [APMigrationModeDependentActionRestriction(
            restrictInMigrationMode: true,
            restrictForRegularDocumentInMigrationMode: true,
            restrictForUnreleasedMigratedDocumentInNormalMode: true)]
        public virtual IEnumerable AddPOOrderLine2(PXAdapter adapter)
        {
            Base.updateTaxCalcMode();
            if (Base.Document.Current != null &&
                Base.Document.Current.DocType == APDocType.Invoice &&
                Base.Document.Current.Released == false &&
                Base.Document.Current.Prebooked == false)
            {

                var selectedLines = poorderlineslist.Cache.Updated.RowCast<POLineRS>().Where(t => t.Selected == true).ToList();
                Base.ProcessPOOrderLines(selectedLines);
                poorderlineslist.Cache.Clear();
            }
            return adapter.Get();
        }


        #endregion

        #region Events

        #region APInvoice Events

        protected virtual void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            APInvoice document = e.Row as APInvoice;
            if (document == null) return;

            var invoiceState = Base.GetDocumentState(cache, document);

            addPOOrderLine.SetVisible(invoiceState.IsDocumentInvoice);

            PXUIFieldAttribute.SetEnabled(this.poorderlineslist.Cache, null, false);

            
            bool allowAddPOOrderLine = invoiceState.IsDocumentEditable &&
                invoiceState.AllowAddPOByProject &&
                Base.vendor.Current != null &&
                !invoiceState.IsRetainageDebAdj;

            addPOOrderLine.SetEnabled(allowAddPOOrderLine);

            PXUIFieldAttribute.SetEnabled<POLineRS.selected>(this.poorderlineslist.Cache, null, allowAddPOOrderLine);
        }

        #endregion

        #region Selecting override

        public virtual IEnumerable pOOrderLinesList()
        {
            APInvoice doc = Base.Document.Current;
            var filter = orderfilter.Current;

            if (doc?.VendorID == null
                || doc.VendorLocationID == null
                || doc.DocType != APDocType.Invoice
                    && doc.DocType != APDocType.DebitAdj)
            {
                return Enumerable.Empty<POLineRS>();
            }

            PXSelectBase<POLineRS> cmd = new PXSelectJoinGroupBy<
                POLineRS,
                LeftJoin<APTran,
                        On<APTran.pOOrderType, Equal<POLineRS.orderType>,
                                                And<APTran.pONbr, Equal<POLineRS.orderNbr>,
                                                And<APTran.pOLineNbr, Equal<POLineRS.lineNbr>,
                            And<APTran.receiptNbr, IsNull,
                            And<APTran.receiptLineNbr, IsNull,
                            And<APTran.released, Equal<False>>>>>>>>,
                Where<POLineRS.orderType, NotEqual<POOrderType.blanket>,
                    And<POLineRS.pOAccrualType, Equal<POAccrualType.order>,
                    And<POLineRS.cancelled, NotEqual<True>,
                    And<POLineRS.closed, NotEqual<True>,
                    And<POLineRS.orderType, NotEqual<POOrderType.standardBlanket>,
                    And<POLineRS.curyID, Equal<Current<APInvoice.curyID>>,
                    And<POLineRS.status, In3<POOrderStatus.open, POOrderStatus.completed>,
                    And2<Where<APTran.refNbr, IsNull, Or<APTran.refNbr, Equal<Current<APInvoice.refNbr>>>>,
                    And<Where<Current<POOrderFilter.orderNbr>, IsNull, Or<POLineRS.orderNbr, Equal<Current<POOrderFilter.orderNbr>>>>>>>>>>>>>,
                    Aggregate
                        <GroupBy<POLineRS.orderType,
                        GroupBy<POLineRS.orderNbr,
                        GroupBy<POLineRS.lineNbr>>>>>(Base);

            if (Base.APSetup.Current.RequireSingleProjectPerDocument == true)
            {
				cmd.WhereAnd<Where<POLineRS.projectID, Equal<Current<APInvoice.projectID>>>>();
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>())
            {
                cmd.WhereAnd<Where<POLineRS.vendorID, Equal<Current<APInvoice.suppliedByVendorID>>,
                    And<POLineRS.vendorLocationID, Equal<Current<APInvoice.suppliedByVendorLocationID>>,
                    And<POLineRS.payToVendorID, Equal<Current<APInvoice.vendorID>>>>>>();
            }
            else
            {
                cmd.WhereAnd<Where<POLineRS.vendorID, Equal<Current<APInvoice.vendorID>>,
                    And<POLineRS.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>>>>();
            }

            if (filter.ShowBilledLines != true)
            {
                cmd.WhereAnd<Where<POLineRS.unbilledQty, Greater<decimal0>,
                    Or<POLineRS.curyUnbilledAmt, Greater<decimal0>>>>();
            }

            var currentOrders = Base.Transactions.Select().RowCast<APTran>().Select(t => new { t.POOrderType, POOrderNbr = t.PONbr, t.POLineNbr }).ToList();

            var orderLinesResult = cmd.View.SelectMultiBound(new object[] { doc, filter });
            var orderLines = orderLinesResult.RowCast<POLineRS>()
                .Where(t => !currentOrders.Contains(new
                {
                    POOrderType = t.OrderType,
                    POOrderNbr = t.OrderNbr,
                    POLineNbr = t.LineNbr
                }));

            return orderLines.ToList();
        }

        #endregion

        #endregion
    }
}
