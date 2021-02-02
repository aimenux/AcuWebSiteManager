using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CN.CacheExtensions;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Helpers;
using PX.Objects.CN.Subcontracts.PO.CacheExtensions;
using PX.Objects.CN.Subcontracts.SC.DAC;
using PX.Objects.CN.Subcontracts.SC.Descriptor.Attributes;
using PX.Objects.CN.Subcontracts.SC.Views;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PoMessages = PX.Objects.PO.Messages;
using ScMessages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;
using ScInfoMessages = PX.Objects.CN.Subcontracts.SC.Descriptor.InfoMessages;

namespace PX.Objects.CN.Subcontracts.SC.Graphs
{
    public class SubcontractEntry : POOrderEntry
    {
        public CRAttributeList<POOrder> Answers;

        public SubcontractSetup SubcontractSetup;

        public SubcontractEntry()
        {
            FeaturesSetHelper.CheckConstructionFeature();
            UpdateDocumentSummaryFormLayout();
            UpdateDocumentDetailsGridLayout();
            RemoveShippingHandlers();
            AddSubcontractType();
        }

        [PXUIField]
        [PXDeleteButton(ConfirmationMessage = ScInfoMessages.SubcontractWillBeDeleted)]
        protected virtual IEnumerable delete(PXAdapter adapter)
        {
            Document.Delete(Document.Current);
            Save.Press();
            return adapter.Get();
        }

        public override Type PrimaryItemType => typeof(Subcontract);

		public override void InitCacheMapping(Dictionary<Type, Type> map)
		{
			base.InitCacheMapping(map);

			map.Add(typeof(InventoryItem), typeof(InventoryItem));
		}

		protected virtual void _(Events.RowSelecting<POSetup> e)
        {
	        PoSetupExt posetupExt = e.Row.GetExtension<PoSetupExt>();

			e.Row.RequireOrderControlTotal = posetupExt.RequireSubcontractControlTotal;
	        e.Row.OrderRequestApproval = posetupExt.SubcontractRequestApproval;
		}

		protected virtual void _(Events.RowInserting<POShipAddress> args)
        {
            args.Cancel = true;
        }

        protected virtual void _(Events.RowInserting<POShipContact> args)
        {
            args.Cancel = true;
        }

        protected virtual void _(Events.RowUpdating<POOrder> args)
        {
            var purchaseOrder = args.NewRow;
            if (purchaseOrder != null)
            {
                purchaseOrder.OrderType = POOrderType.RegularSubcontract;
            }
        }

        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        protected virtual void PoOrder_SiteId_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(Objects.PO.PO.NumberingAttribute))]
        [AutoNumber(typeof(PoSetupExt.subcontractNumberingID), typeof(POOrder.orderDate))]
        protected virtual void PoOrder_OrderNbr_CacheAttached(PXCache cache)
        {
        }

        protected virtual void _(Events.RowInserted<POLine> args)
        {
            var orderLine = args.Row;
            if (orderLine != null)
            {
                orderLine.LineType = POLineType.Service;
            }
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(POLineInventoryItemAttribute))]
        [SubcontractLineInventoryItem(Filterable = true)]
        [PXCustomizeSelectorColumns(
            typeof(InventoryItem.inventoryCD),
            typeof(InventoryItem.descr),
            typeof(InventoryItem.itemClassID),
            typeof(InventoryItem.itemStatus),
            typeof(InventoryItem.itemType),
            typeof(InventoryItem.baseUnit),
            typeof(InventoryItem.salesUnit),
            typeof(InventoryItem.purchaseUnit),
            typeof(InventoryItem.basePrice),
            typeof(InventoryItemExt.isUsedInProject))]
        protected virtual void PoLine_InventoryId_CacheAttached(PXCache cache)
        {
        }

        protected virtual void _(Events.RowSelected<POLine> args)
        {
            Transactions.View.RequestRefresh();
            var subcontractLine = args.Row;
            if (subcontractLine != null)
            {
                var error = PXUIFieldAttribute.GetErrorOnly<POLine.inventoryID>(args.Cache, args.Row);
                if (error == null)
                {
                    ValidateInventoryItem(subcontractLine, PXErrorLevel.Warning);
                }
            }
        }

        protected virtual void _(Events.RowPersisting<POOrder> args)
        {
            var subcontract = args.Row;
            if (subcontract != null)
            {
                var subcontractLines = Transactions.Select(subcontract.OrderNbr);
                foreach (var subcontractLine in subcontractLines)
                {
                    ValidateNonStockInventoryItem(subcontractLine);
                    ValidateInventoryItem(subcontractLine, PXErrorLevel.Error);
                }
            }
        }

        protected virtual void _(Events.FieldSelecting<InventoryItemExt.isUsedInProject> args)
        {
            if (args.Row is InventoryItem inventoryItem)
            {
                var subcontractLine = Transactions.Current;
                args.ReturnValue = IsInventoryItemUsedInProject(subcontractLine, inventoryItem.InventoryID);
            }
        }

        protected virtual void _(Events.FieldUpdated<POLine.projectID> args)
        {
            Caches[typeof(SubcontractInventoryItem)].ClearQueryCache();
        }

        protected virtual void _(Events.FieldUpdated<POLine.taskID> args)
        {
            Caches[typeof(SubcontractInventoryItem)].ClearQueryCache();
        }

        protected virtual void _(Events.FieldUpdated<POLine.costCodeID> args)
        {
            Caches[typeof(SubcontractInventoryItem)].ClearQueryCache();
        }

        protected virtual void _(Events.FieldUpdated<POLine.expenseAcctID> args)
        {
            Caches[typeof(SubcontractInventoryItem)].ClearQueryCache();
        }

        protected override void POOrder_ExpectedDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            if (Transactions.Any())
            {
                Document.Ask(PoMessages.Warning, ScMessages.SubcontractStartDateChangeConfirmation,
                    MessageButtons.YesNo, MessageIcon.Question);
            }
        }

        protected override void POOrder_OrderDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            if (Transactions.Any())
            {
                Document.Ask(PoMessages.Warning, ScMessages.SubcontractDateChangeConfirmation,
                    MessageButtons.YesNo, MessageIcon.Question);
            }
        }

        protected override void POOrder_OrderType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs args)
        {
            args.NewValue = POOrderType.RegularSubcontract;
        }

        protected override void POOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs args)
        {
            base.POOrder_RowSelected(cache, args);
            if (POSetup.Current != null && args.Row is POOrder order)
            {
                var setupExtension = POSetup.Current.GetExtension<PoSetupExt>();
                SetDefaultPurchaseOrderPreferences();
                UpdatePurchaseOrderBasedOnPreferences(cache, order, setupExtension);
            }
        }

        protected override void POOrder_RowDeleting(PXCache cache, PXRowDeletingEventArgs args)
        {
            var purchaseOrder = (POOrder) args.Row;
            if (purchaseOrder == null)
            {
                return;
            }
            ValidateSubcontractOnDelete(purchaseOrder);
        }

        protected override void POOrder_RowUpdated(PXCache cache, PXRowUpdatedEventArgs args)
        {
            if (args.Row is POOrder order)
            {
                SetControlTotalIfRequired(cache, order);
                base.POOrder_RowUpdated(cache, args);
            }
        }

        protected override void POLine_CuryUnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs args)
        {
            if (!SkipCostDefaulting && args.Row is POLine subcontractLine)
            {
                var subcontract = Document.Current;
                args.NewValue = GetCurrencyUnitCost(subcontract, subcontractLine, cache);
                if (subcontractLine.InventoryID != null && subcontract?.VendorID != null)
                {
                    APVendorPriceMaint.CheckNewUnitCost<POLine, POLine.curyUnitCost>(
                        cache, subcontractLine, args.NewValue);
                }
            }
        }

        private void SetControlTotalIfRequired(PXCache cache, POOrder order)
        {
            if (order.Cancelled == false
                && POSetup.Current.RequireOrderControlTotal == false
                && order.CuryOrderTotal != order.CuryControlTotal)
            {
                var controlTotal = order.CuryOrderTotal.IsNullOrZero()
                    ? decimal.Zero
                    : order.CuryOrderTotal;
                cache.SetValueExt<POOrder.curyControlTotal>(order, controlTotal);
            }
        }

        private void ValidateInventoryItem(POLine subcontractLine, PXErrorLevel errorLevel)
        {
            if (!IsInventoryItemValid(subcontractLine))
            {
                var inventoryItem = GetInventoryItem(subcontractLine.InventoryID);
                Transactions.Cache.RaiseException<POLine.inventoryID>(subcontractLine,
                    ScMessages.ItemIsNotPresentedInTheProjectBudget,
                    inventoryItem.InventoryCD, errorLevel);
            }
            else
            {
                Transactions.Cache.ClearFieldErrors<POLine.inventoryID>(subcontractLine);
            }
        }

        private bool IsInventoryItemValid(POLine subcontractLine)
        {
            if (subcontractLine.InventoryID == null || !ShouldValidateInventoryItem() ||
                subcontractLine.ProjectID == null || ProjectDefaultAttribute.IsNonProject(subcontractLine.ProjectID)) return true;
          
            var project = GetProject(subcontractLine.ProjectID);
            if (IsNonProjectAccountGroupsAllowed(project) || !IsItemLevel(project)) return true;
            var result = IsInventoryItemUsedInProject(subcontractLine, subcontractLine.InventoryID);
            return result;
        }

        private bool ShouldValidateInventoryItem()
        {
            var originalSubcontract = (POOrder)Document.Cache.GetOriginal(Document.Current);
            if (originalSubcontract == null)
            {
                return true;
            }
            return (originalSubcontract.Status != POOrderStatus.Balanced || Document.Current.Status != POOrderStatus.Hold) &&
                (originalSubcontract.Status == POOrderStatus.Hold || originalSubcontract.Status == POOrderStatus.Balanced);
        }

        private bool IsInventoryItemUsedInProject(POLine subcontractLine, int? inventoryID)
        {
            if (subcontractLine.ProjectID == null || ProjectDefaultAttribute.IsNonProject(subcontractLine.ProjectID)) return false;

            var project = GetProject(subcontractLine.ProjectID);
            if (!IsItemLevel(project)) return false;

            var query = new PXSelect<PMCostBudget, Where<PMCostBudget.projectID, Equal<Required<PMCostBudget.projectID>>>>(this);
            var parameters = new List<object>();
            parameters.Add(subcontractLine.ProjectID);

            if (subcontractLine.TaskID.HasValue)
            {
                query.WhereAnd<Where<PMCostBudget.projectTaskID, Equal<Required<PMCostBudget.projectTaskID>>>>();
                parameters.Add(subcontractLine.TaskID);
            }

            var accountGroupID = GetAccountGroup(subcontractLine, inventoryID);
            if (accountGroupID.HasValue)
            {
                query.WhereAnd<Where<PMCostBudget.accountGroupID, Equal<Required<PMCostBudget.accountGroupID>>>>();
                parameters.Add(accountGroupID);
            }

            if (subcontractLine.CostCodeID.HasValue)
            {
                query.WhereAnd<Where<PMCostBudget.costCodeID, Equal<Required<PMCostBudget.costCodeID>>>>();
                parameters.Add(subcontractLine.CostCodeID);
            }

            var result = query.Select(parameters.ToArray()).AsEnumerable()
                .Any(b => ((PMCostBudget)b).InventoryID == inventoryID);
            return result;
        }

        private static void UpdatePurchaseOrderBasedOnPreferences(PXCache cache, POOrder order, PoSetupExt setup)
        {
            order.RequestApproval = setup.SubcontractRequestApproval;
            var isControlTotalVisible = setup.RequireSubcontractControlTotal == true;
            PXUIFieldAttribute.SetVisible<POOrder.curyControlTotal>(cache, order, isControlTotalVisible);
        }

        private void SetDefaultPurchaseOrderPreferences()
        {
            POSetup.Current.UpdateSubOnOwnerChange = false;
            POSetup.Current.AutoReleaseAP = false;
            POSetup.Current.UpdateSubOnOwnerChange = false;
        }

        private void UpdateDocumentSummaryFormLayout()
        {
            PXUIFieldAttribute.SetDisplayName<POOrder.orderNbr>(Document.Cache,
                ScMessages.Subcontract.SubcontractNumber);
            PXUIFieldAttribute.SetDisplayName<POOrder.expectedDate>(Document.Cache, ScMessages.Subcontract.StartDate);
            PXUIFieldAttribute.SetDisplayName<POOrder.curyOrderTotal>(Document.Cache,
                ScMessages.Subcontract.SubcontractTotal);
            PXUIFieldAttribute.SetDisplayName<POLine.receivedQty>(Transactions.Cache,
                ScMessages.Subcontract.ReceivedQty);
        }

        private void UpdateDocumentDetailsGridLayout()
        {
            PXUIFieldAttribute.SetVisible<POOrder.orderType>(Document.Cache, null, false);
            PXUIFieldAttribute.SetDisplayName<POLine.promisedDate>(Transactions.Cache,
                ScMessages.Subcontract.StartDate);
            PXUIFieldAttribute.SetVisible<POLine.discountSequenceID>(Transactions.Cache, null, true);
            PXUIFieldAttribute.SetVisible<POLine.rcptQtyAction>(Transactions.Cache, null, false);
            PXUIFieldAttribute.SetVisible<POLine.requestedDate>(Transactions.Cache, null, false);
            PXUIFieldAttribute.SetVisible<POLine.promisedDate>(Transactions.Cache, null, false);
            PXUIFieldAttribute.SetVisible<POLine.completed>(Transactions.Cache, null, false);
            PXUIFieldAttribute.SetVisible<POLine.cancelled>(Transactions.Cache, null, false);
            PXUIFieldAttribute.SetVisible<POLine.pONbr>(Transactions.Cache, null, false);
            PXUIFieldAttribute.SetVisible<POLine.baseOrderQty>(Transactions.Cache, null, true);
            PXUIFieldAttribute.SetVisible<POLine.receivedQty>(Transactions.Cache, null, false);
        }

        private void RemoveShippingHandlers()
        {
            RowInserted.RemoveHandler<POOrder>(POOrder_RowInserted);
            FieldUpdated.RemoveHandler<POOrder.shipDestType>(POOrder_ShipDestType_FieldUpdated);
            FieldUpdated.RemoveHandler<POOrder.siteID>(POOrder_SiteID_FieldUpdated);
            FieldUpdated.RemoveHandler<POOrder.shipToLocationID>(POOrder_ShipToLocationID_FieldUpdated);
        }

        private decimal? GetCurrencyUnitCost(POOrder subcontract, POLine subcontractLine, PXCache cache)
        {
            if (subcontractLine.ManualPrice == true || subcontractLine.UOM == null ||
                subcontractLine.InventoryID == null || subcontract?.VendorID == null)
            {
                return subcontractLine.CuryUnitCost.GetValueOrDefault();
            }
            var currencyInfo = currencyinfo.Search<CurrencyInfo.curyInfoID>(subcontract.CuryInfoID);
            return APVendorPriceMaint.CalculateUnitCost(
                cache, subcontractLine.VendorID, subcontract.VendorLocationID, subcontractLine.InventoryID,
                subcontractLine.SiteID, currencyInfo, subcontractLine.UOM, subcontractLine.OrderQty,
                subcontract.OrderDate.GetValueOrDefault(), subcontractLine.CuryUnitCost);
        }

        private void AddSubcontractType()
        {
            var allowedValues = POOrderType.RegularSubcontract.CreateArray();
            PXStringListAttribute.AppendList<POOrder.orderType>(Document.Cache, null, allowedValues, allowedValues);
        }

        private void ValidateSubcontractOnDelete(POOrder purchaseOrder)
        {
            if (purchaseOrder.Hold != true && purchaseOrder.Behavior == POBehavior.ChangeOrder)
            {
                throw new PXException(ScMessages.CanNotDeleteWithChangeOrderBehavior);
            }
            if (GetSubcontractReceiptsCount(purchaseOrder) > 0)
            {
                throw new PXException(ScMessages.SubcontractHasReceiptsAndCannotBeDeleted);
            }
            if (GetSubcontractBillsReleasedCount(purchaseOrder) > 0)
            {
                throw new PXException(ScMessages.SubcontractHasBillsReleasedAndCannotBeDeleted);
            }
            if (GetSubcontractBillsGeneratedCount(purchaseOrder) > 0)
            {
                throw new PXException(ScMessages.SubcontractHasBillsGeneratedAndCannotBeDeleted);
            }
            Transactions.View.SetAnswer(ScMessages.SubcontractLineLinkedToSalesOrderLine, WebDialogResult.OK);
        }

        private void ValidateNonStockInventoryItem(POLine subcontractLine)
        {
            var cache = Caches[typeof(POLine)];
            if (subcontractLine.InventoryID != null)
            {
                var inventoryItem = GetInventoryItem(subcontractLine.InventoryID);
                if (inventoryItem.NonStockReceipt.GetValueOrDefault() &&
                    inventoryItem.StkItem.GetValueOrDefault())
                {
                    var exceptionMessage = new PXSetPropertyException<POLine.inventoryID>(
                        ScMessages.InvalidInventoryItemMessage, PXErrorLevel.Error);
                    cache.RaiseExceptionHandling<POLine.inventoryID>(subcontractLine,
                        inventoryItem.InventoryCD, exceptionMessage);
                }
            }
        }

        private int? GetSubcontractReceiptsCount(POOrder purchaseOrder)
        {
            return PXSelectGroupBy<POOrderReceipt,
                Where<POOrderReceipt.pONbr, Equal<Required<POOrder.orderNbr>>,
                    And<POOrderReceipt.pOType, Equal<Required<POOrder.orderType>>>>,
                Aggregate<Count>>.Select(this, purchaseOrder.OrderNbr, purchaseOrder.OrderType).RowCount;
        }

        private int? GetSubcontractBillsReleasedCount(POOrder purchaseOrder)
        {
            return PXSelectGroupBy<APTran,
                Where<APTran.pONbr, Equal<Required<POOrder.orderNbr>>,
                    And<APTran.pOOrderType, Equal<Required<POOrder.orderType>>,
                    And<APTran.released, Equal<True>>>>,
                Aggregate<Count>>.Select(this, purchaseOrder.OrderNbr, purchaseOrder.OrderType).RowCount;
        }

        private int? GetSubcontractBillsGeneratedCount(POOrder purchaseOrder)
        {
            return PXSelectGroupBy<APTran,
                Where<APTran.pONbr, Equal<Required<POOrder.orderNbr>>,
                    And<APTran.pOOrderType, Equal<Required<POOrder.orderType>>>>,
                Aggregate<Count>>.Select(this, purchaseOrder.OrderNbr, purchaseOrder.OrderType).RowCount;
        }

        private bool IsNonProjectAccountGroupsAllowed(PMProject project)
        {
            var projectExt = PXCache<Contract>.GetExtension<ContractExt>(project);
            return projectExt.AllowNonProjectAccountGroups == true;
        }

        private bool IsItemLevel(PMProject project)
        {
            var result = project.CostBudgetLevel == BudgetLevels.Item || project.CostBudgetLevel == BudgetLevels.Detail;
            return result;
        }

        private int? GetAccountGroup(POLine subcontractLine, int? inventoryID)
        {
            InventoryItem item = GetInventoryItem(inventoryID);
            if (item.StkItem == true && item.COGSAcctID != null)
            {
                Account account = (Account)PXSelectorAttribute.Select<InventoryItem.cOGSAcctID>(Caches[typeof(InventoryItem)], item);
                if (account != null && account.AccountGroupID != null)
                    return account.AccountGroupID;
            }
            else
            {
                Account account = (Account)PXSelectorAttribute.Select<POLine.expenseAcctID>(Transactions.Cache, subcontractLine);
                if (account != null && account.AccountGroupID != null)
                    return account.AccountGroupID;
            }
            return null;
        }

        private InventoryItem GetInventoryItem(int? inventoryID)
        {
            return new PXSelect<InventoryItem,
                    Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(this)
                .SelectSingle(inventoryID);
        }

        private PMProject GetProject(int? projectID)
        {
            var result = new PXSelect<PMProject,
              Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>(this).SelectSingle(projectID);
            return result;
        }
    }
}