using System;
using System.Collections;
using PX.Data;
using PX.Objects.CN.Subcontracts.PM.CacheExtensions;
using PX.Objects.CN.Subcontracts.PM.Descriptor;
using PX.Objects.CN.Subcontracts.PO.CacheExtensions;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PmMessages = PX.Objects.CN.Subcontracts.PM.Descriptor.Messages;
using ScMessages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PM.GraphExtensions
{
    public class ChangeOrderEntryExt : PXGraphExtension<ChangeOrderEntry>
    {
        public override void Initialize()
        {
            SetDisplayNames();
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXOverride]
        public virtual POOrderEntry CreateTarget(POOrder order)
        {
            return order.OrderType == POOrderType.RegularSubcontract
                ? CreateSubcontractEntry()
                : PXGraph.CreateInstance<POOrderEntry>();
        }

        [PXOverride]
        public virtual PMChangeOrderLine CreateChangeOrderLine(POLinePM poLine,
            Func<POLinePM, PMChangeOrderLine> baseMethod)
        {
            var line = baseMethod(poLine);
            CopyCommitmentType(poLine, line);
            return line;
        }

        [PXUIField(DisplayName = PX.Objects.PM.Messages.ViewCommitments,
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
        public IEnumerable ViewCommitments(PXAdapter adapter)
        {
            if (GetCurrentCommitmentType() == CommitmentBaseType.Subcontract)
            {
                var graph = PXGraph.CreateInstance<SubcontractEntry>();
                graph.Document.Current = GetCurrentPurchaseOrder();
                throw new PXRedirectRequiredException(graph, string.Empty)
                {
                    Mode = PXBaseRedirectException.WindowMode.NewWindow
                };
            }
            return Base.ViewCommitments(adapter);
        }

        [PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
        [PXSelector(typeof(Search4<POLine.orderNbr,
            Where<POLine.orderType, In3<POOrderType.regularOrder, POOrderType.regularSubcontract>,
                And<POLine.projectID, Equal<Current<PMChangeOrder.projectID>>,
                And<POLine.cancelled, Equal<False>,
                And<POLine.completed, Equal<False>,
                And<Where<Current<ChangeOrderEntry.POLineFilter.vendorID>, IsNull,
                    Or<POLine.vendorID, Equal<Current<ChangeOrderEntry.POLineFilter.vendorID>>>>>>>>>,
            Aggregate<GroupBy<POLine.orderType,
                GroupBy<POLine.orderNbr,
                GroupBy<POLine.vendorID>>>>>),
            typeof(POLine.orderType), typeof(POLine.orderNbr), typeof(POLine.vendorID))]
        protected virtual void PoLineFilter_PoOrderNbr_CacheAttached(PXCache cache)
        {
        }

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLineCLExt.commitmentType> args)
		{
            if (args.Row is PMChangeOrderLine line)
            {
                var extension = PXCache<PMChangeOrderLine>.GetExtension<PMChangeOrderLineCLExt>(line);
                line.POOrderType = GetPurchaseOrderType(extension.CommitmentType);
                if (args.ExternalCall && extension.CommitmentType != (string) args.OldValue)
                {
                    line.POOrderNbr = null;
                }
            }
        }

        protected virtual void _(Events.RowPersisting<PMChangeOrderLine> args)
        {
            var line = args.Row;
            if (line == null || args.Operation == PXDBOperation.Delete)
            {
                return;
            }
            var extension = PXCache<PMChangeOrderLine>.GetExtension<PMChangeOrderLineCLExt>(line);
            if (extension.CommitmentType != CommitmentBaseType.PurchaseOrder)
            {
                ValidateInventoryItem(args, line);
            }
        }

        protected virtual void _(Events.RowSelected<POLinePM> args)
        {
            var line = args.Row;
            if (line == null)
            {
                return;
            }
            var extension = PXCache<POLinePM>.GetExtension<PoLinePmExt>(line);
            extension.CommitmentType = GetCommitmentType(line.OrderType);
        }

        private static void CopyCommitmentType(POLinePM purchaseOrderLine, PMChangeOrderLine changeOrderLine)
        {
            var changeOrderLineExtension =
                PXCache<PMChangeOrderLine>.GetExtension<PMChangeOrderLineCLExt>(changeOrderLine);
            var purchaseOrderLineExtension = PXCache<POLinePM>.GetExtension<PoLinePmExt>(purchaseOrderLine);
            changeOrderLineExtension.CommitmentType = purchaseOrderLineExtension.CommitmentType;
        }

        private static string GetPurchaseOrderType(string commitmentType)
        {
            return commitmentType == CommitmentBaseType.PurchaseOrder
                ? POOrderType.RegularOrder
                : POOrderType.RegularSubcontract;
        }

        private static string GetCommitmentType(string purchaseOrderType)
        {
            return purchaseOrderType == POOrderType.RegularSubcontract
                ? CommitmentBaseType.Subcontract
                : CommitmentBaseType.PurchaseOrder;
        }

        private static SubcontractEntry CreateSubcontractEntry()
        {
            var subcontractEntry = PXGraph.CreateInstance<SubcontractEntry>();
            var poSetupExt = subcontractEntry.POSetup.Current.GetExtension<PoSetupExt>();
            poSetupExt.RequireSubcontractControlTotal = false;
            return subcontractEntry;
        }

        private void ValidateInventoryItem(Events.RowPersisting<PMChangeOrderLine> args, PMChangeOrderLine line)
        {
            var inventoryItem = GetInventoryItem(line.InventoryID);
            if (inventoryItem != null && (inventoryItem.StkItem == true || inventoryItem.NonStockReceipt == true))
            {
                args.Cache.RaiseExceptionHandling<PMChangeOrderLine.inventoryID>(line, inventoryItem.InventoryCD,
                    new PXSetPropertyException(ScMessages.InvalidInventoryItemMessage, PXErrorLevel.Error));
            }
        }

        private InventoryItem GetInventoryItem(int? inventoryId)
        {
            return new PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(Base)
                .SelectSingle(inventoryId);
        }

        private void SetDisplayNames()
        {
            PXUIFieldAttribute.SetDisplayName<PMChangeOrderLine.pOOrderNbr>(Base.Details.Cache,
                PmMessages.ChangeOrders.CommitmentNbr);
            PXUIFieldAttribute.SetDisplayName<PMChangeOrderLine.pOLineNbr>(Base.Details.Cache,
                PmMessages.ChangeOrders.CommitmentLineNbr);
            PXUIFieldAttribute.SetDisplayName<ChangeOrderEntry.POLineFilter.pOOrderNbr>(
                Base.AvailablePOLineFilter.Cache, PmMessages.ChangeOrders.CommitmentNbr);
            PXUIFieldAttribute.SetDisplayName<POLinePM.orderNbr>(Base.AvailablePOLines.Cache,
                PmMessages.ChangeOrders.CommitmentNbr);
            PXUIFieldAttribute.SetDisplayName<POLinePM.lineNbr>(Base.AvailablePOLines.Cache,
                PmMessages.ChangeOrders.CommitmentLineNbr);
        }

        private string GetCurrentCommitmentType()
        {
            return Base.Details.Current != null
                ? PXCache<PMChangeOrderLine>.GetExtension<PMChangeOrderLineCLExt>(Base.Details.Current).CommitmentType
                : null;
        }

        private POOrder GetCurrentPurchaseOrder()
        {
            var query = new PXSelect<POOrder,
                Where<POOrder.orderType, Equal<Current<PMChangeOrderLine.pOOrderType>>,
                    And<POOrder.orderNbr, Equal<Current<PMChangeOrderLine.pOOrderNbr>>>>>(Base);
            return query.SelectSingle();
        }
    }
}