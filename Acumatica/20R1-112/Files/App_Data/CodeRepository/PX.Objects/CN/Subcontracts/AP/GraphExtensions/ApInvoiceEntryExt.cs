using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Subcontracts.AP.Descriptor;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.PO;
using ApMessages = PX.Objects.CN.Subcontracts.AP.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.AP.GraphExtensions
{
    public class ApInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
    {
        public PXAction<APInvoice> ViewSubcontract;
        public PXAction<APInvoice> ViewPurchaseOrder;

        public delegate void InvoicePoOrderDelegate(POOrder order, bool createNew, bool keepOrderTaxes = false);

        public delegate bool EnableRetainageDelegate();

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() &&
                !SiteMapExtension.IsTaxBillsAndAdjustmentsScreenId();
        }

        [PXOverride]
        public virtual bool EnableRetainage(EnableRetainageDelegate baseMethod)
        {
            if (Base.Document.Current.RetainageApply == true)
            {
                return false;
            }
            Base.Document.Current.RetainageApply = true;
            Base.Document.Cache.SetDefaultExt<APRegister.defRetainagePct>(Base.Document.Current);
            Base.Document.Cache.RaiseExceptionHandling<APInvoice.retainageApply>(Base.Document.Current, true,
                new PXSetPropertyException(ApMessages.AutoApplyRetainageCheckBox, PXErrorLevel.Warning));
            return true;
        }

        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXLookupButton]
        public virtual void viewSubcontract()
        {
            var graph = PXGraph.CreateInstance<SubcontractEntry>();
            ViewPoEntity(graph, ApMessages.ViewSubcontract, x => x.OrderType == POOrderType.RegularSubcontract);
        }

        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        public virtual void viewPurchaseOrder()
        {
            var graph = PXGraph.CreateInstance<POOrderEntry>();
            ViewPoEntity(graph, ApMessages.ViewPoOrder, x => x.OrderType != POOrderType.RegularSubcontract);
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXStringListAttribute))]
        [LinkLineSelectedModeList]
        protected virtual void LinkLineFilter_SelectedMode_CacheAttached(PXCache cache)
        {
        }

        protected virtual void _(Events.FieldSelecting<APTran, APTran.pOLineNbr> args)
        {
            if (IsSubcontract(args.Row))
            {
                args.ReturnValue = null;
            }
        }

        protected virtual void _(Events.FieldSelecting<APTran, APTran.pOOrderType> args)
        {
            if (IsSubcontract(args.Row))
            {
                args.ReturnValue = null;
            }
        }

        protected virtual void _(Events.FieldSelecting<APTran, APTran.pONbr> args)
        {
            if (IsSubcontract(args.Row))
            {
                args.ReturnValue = null;
            }
        }

        protected void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs args, PXRowSelected baseHandler)
        {
            if (!(args.Row is APInvoice))
            {
                return;
            }
            baseHandler(cache, args);
            Base.Document.Cache.AllowUpdate = true;
        }

        private void ViewPoEntity(POOrderEntry graph, string message, Func<POOrder, bool> checkPurchaseOrderType)
        {
            if (Base.Transactions.Current == null)
            {
                return;
            }
            var purchaseOrder = GetPurchaseOrder(Base.Transactions.Current);
            if (purchaseOrder == null || !checkPurchaseOrderType(purchaseOrder))
            {
                return;
            }
            graph.Document.Current = purchaseOrder;
            throw new PXRedirectRequiredException(graph, message)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private static bool IsSubcontract(APTran apTran)
        {
            return apTran?.POOrderType == POOrderType.RegularSubcontract;
        }

        private POOrder GetPurchaseOrder(APTran apTran)
        {
            var query = new PXSelect<POOrder,
                Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
                    And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>(Base);
            return query.SelectSingle(apTran.POOrderType, apTran.PONbr);
        }
    }
}
