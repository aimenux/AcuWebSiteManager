using PX.Data;
using PX.Objects.PO;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class CreatePurchaseOrderProcess : PXGraph<CreatePurchaseOrderProcess>
    {
        public CreatePurchaseOrderProcess()
        {
            PXUIFieldAttribute.SetEnabled<POEnabledFSSODet.poVendorID>(LinesToPO.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<POEnabledFSSODet.poVendorLocationID>(LinesToPO.Cache, null, true);

            CreatePurchaseOrderProcess graphCreatePurchaseOrderProcess;

            LinesToPO.SetProcessDelegate(
                delegate(List<POEnabledFSSODet> poEnabledFSSODetRows)
                {
                    graphCreatePurchaseOrderProcess = PXGraph.CreateInstance<CreatePurchaseOrderProcess>();

                    List<List<POEnabledFSSODet>> groupedFSSODetRows = poEnabledFSSODetRows
                        .GroupBy(u => new { u.POVendorID, u.POVendorLocationID })
                        .Select(grp => grp.ToList())
                        .ToList();

                    this.processLines(poEnabledFSSODetRows, graphCreatePurchaseOrderProcess, groupedFSSODetRows);
                });
        }
        
        #region Select
        public PXFilter<CreatePOFilter> Filter;
        public PXCancel<CreatePOFilter> Cancel;
        public PXAction<CreatePOFilter> viewDocument;
        public PXSetup<POSetup> poSetup;


        [PXFilterable]
        [PXViewDetailsButton(typeof(CreatePOFilter))]
        public
            PXFilteredProcessing<POEnabledFSSODet, CreatePOFilter,
            Where2<
                Where<CurrentValue<CreatePOFilter.inventoryID>, IsNull,
                Or<POEnabledFSSODet.inventoryID, Equal<CurrentValue<CreatePOFilter.inventoryID>>>>,
            And2<
                Where<CurrentValue<CreatePOFilter.itemClassID>, IsNull,
                Or<POEnabledFSSODet.inventoryItemClassID, Equal<CurrentValue<CreatePOFilter.itemClassID>>>>,
            And2<
                Where<CurrentValue<CreatePOFilter.siteID>, IsNull,
                Or<POEnabledFSSODet.siteID, Equal<CurrentValue<CreatePOFilter.siteID>>>>,
            And2<
                Where<CurrentValue<CreatePOFilter.poVendorID>, IsNull,
                Or<POEnabledFSSODet.poVendorID, Equal<CurrentValue<CreatePOFilter.poVendorID>>>>,
            And2<
                Where<CurrentValue<CreatePOFilter.customerID>, IsNull,
                Or<POEnabledFSSODet.srvCustomerID, Equal<CurrentValue<CreatePOFilter.customerID>>>>,
            And2<
                Where<CurrentValue<CreatePOFilter.srvOrdType>, IsNull,
                Or<POEnabledFSSODet.srvOrdType, Equal<CurrentValue<CreatePOFilter.srvOrdType>>>>,
            And2<
                Where<CurrentValue<CreatePOFilter.sORefNbr>, IsNull,
                Or<POEnabledFSSODet.refNbr, Equal<CurrentValue<CreatePOFilter.sORefNbr>>>>,
            And<
                POEnabledFSSODet.orderDate, LessEqual<CurrentValue<CreatePOFilter.upToDate>>>>>>>>>>,
            OrderBy<Asc<POEnabledFSSODet.poVendorID>>>
            LinesToPO;
        #endregion

        #region Actions
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXEditDetailButton]
        public virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            LinesToPO.Cache.IsDirty = false;

            POEnabledFSSODet poEnabledFSSODetRow = LinesToPO.Current;

            if (poEnabledFSSODetRow == null
                    || poEnabledFSSODetRow.SrvOrdType == null
                        || poEnabledFSSODetRow.RefNbr == null)
            {
                return adapter.Get();
            }

            ServiceOrderEntry graphServiceOrderEntry = PXGraph.CreateInstance<ServiceOrderEntry>();

            graphServiceOrderEntry.ServiceOrderRecords.Current =
                graphServiceOrderEntry.ServiceOrderRecords
                    .Search<FSServiceOrder.refNbr>(poEnabledFSSODetRow.RefNbr, poEnabledFSSODetRow.SrvOrdType);

            PXRedirectHelper.TryRedirect(graphServiceOrderEntry, PXRedirectHelper.WindowMode.Popup);

            return adapter.Get();
        }
        #endregion
        
        #region PrivateMethods
        private void processLines(
            List<POEnabledFSSODet> originalItemList,
            PXGraph<CreatePurchaseOrderProcess> graphCreatePurchaseOrderByServiceOrder, 
            List<List<POEnabledFSSODet>> groupedFSSODetRows)
        {
            if (groupedFSSODetRows.Count == 0)
            {
                return;
            }

            POOrder poOrderRow = null;
            POLine poLineRow = null;
            POEnabledFSSODet firstPOEnabledFSSODetRow = null;

            PXGraph processGraph = new PXGraph();
            POOrderEntry graphPOOrderEntry = CreateInstance<POOrderEntry>();

            foreach (List<POEnabledFSSODet> itemGroup in groupedFSSODetRows)
            {
                if (!this.IsThisItemGroupValid(itemGroup, originalItemList))
                {
                    continue;
                }

                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    try
                    {
                        processGraph.Clear(PXClearOption.ClearAll);
                        graphPOOrderEntry.Clear(PXClearOption.ClearAll);

                        this.CreatePOOrderDocument(graphPOOrderEntry, itemGroup, poOrderRow, poLineRow, firstPOEnabledFSSODetRow);

                        foreach (POEnabledFSSODet poEnabledFSSODetRow in itemGroup)
                        {
                            PXUpdate<
                                Set<FSSODet.poType, Required<FSSODet.poType>,
                                Set<FSSODet.poNbr, Required<FSSODet.poNbr>,
                                Set<FSSODet.poLineNbr, Required<FSSODet.poLineNbr>,
                                Set<FSSODet.poStatus, Required<FSSODet.poStatus>,
                                Set<FSSODet.poCompleted, Required<FSSODet.poCompleted>,
                                Set<FSSODet.poVendorID, Required<FSSODet.poVendorID>,
                                Set<FSSODet.poVendorLocationID, Required<FSSODet.poVendorLocationID>>>>>>>>,
                              FSSODet,
                            Where<
                                FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>,
                                And<FSSODet.poNbr, IsNull>>>
                            .Update(
                                    processGraph,
                                    poEnabledFSSODetRow.POType,
                                    graphPOOrderEntry.Document.Current.OrderNbr,
                                    poEnabledFSSODetRow.POLineNbr,
                                    graphPOOrderEntry.Document.Current.Status,
                                    poEnabledFSSODetRow.POCompleted,
                                    poEnabledFSSODetRow.POVendorID,
                                    poEnabledFSSODetRow.POVendorLocationID,
                                    poEnabledFSSODetRow.SODetID);

                            PXProcessing<POEnabledFSSODet>.SetInfo(originalItemList.IndexOf(poEnabledFSSODetRow), TX.Messages.RECORD_PROCESSED_SUCCESSFULLY);
                            poEnabledFSSODetRow.PONbrCreated = graphPOOrderEntry.Document.Current.OrderNbr;
                        }

                        ts.Complete();
                    }
                    catch (Exception e)
                    {
                        ts.Dispose();

                        foreach (POEnabledFSSODet poEnabledFSSODetRow in itemGroup)
                        {
                            PXProcessing<POEnabledFSSODet>.SetError(originalItemList.IndexOf(poEnabledFSSODetRow), e);
                        }
                    }  
                }
            }
        }

        private bool IsThisItemGroupValid(List<POEnabledFSSODet> itemGroup, List<POEnabledFSSODet> originalItemList)
        {
            if (itemGroup[0].POVendorID == null
                                    || itemGroup[0].POVendorLocationID == null)
            {
                foreach (POEnabledFSSODet poEnabledFSSODetRow in itemGroup)
                {
                    PXProcessing<POEnabledFSSODet>.SetWarning(originalItemList.IndexOf(poEnabledFSSODetRow), TX.Error.MISSING_VENDOR_OR_LOCATION);
                }

                return false;
            }

            return true;
        }
        
        private void CreatePOOrderDocument(
            POOrderEntry graphPOOrderEntry,
            List<POEnabledFSSODet> itemGroup,
            POOrder poOrderRow,
            POLine poLineRow,
            POEnabledFSSODet firstPOEnabledFSSODetRow)
        {
            firstPOEnabledFSSODetRow = itemGroup[0];

            this.InitializePOOrderDocument(graphPOOrderEntry, poOrderRow, firstPOEnabledFSSODetRow);

            foreach (POEnabledFSSODet poEnabledFSSODetRow in itemGroup)
            {
                this.InsertPOLine(graphPOOrderEntry, poLineRow, poEnabledFSSODetRow);
            }

            graphPOOrderEntry.Save.Press(); 
        }
        
        private void InitializePOOrderDocument(POOrderEntry graphPOOrderEntry, POOrder poOrderRow, POEnabledFSSODet firstPOEnabledFSSODetRow)
        {
            poOrderRow = new POOrder();
            poOrderRow = graphPOOrderEntry.Document.Current = graphPOOrderEntry.Document.Insert(poOrderRow);
            poOrderRow.OrderType = POOrderType.RegularOrder;
            poOrderRow.VendorID = firstPOEnabledFSSODetRow.POVendorID;
            poOrderRow.VendorLocationID = firstPOEnabledFSSODetRow.POVendorLocationID;
            graphPOOrderEntry.Document.Update(poOrderRow);
        }
        
        private void InsertPOLine(POOrderEntry graphPOOrderEntry, POLine poLineRow, POEnabledFSSODet poEnabledFSSODetRow)
        {
            poLineRow = new POLine()
            {
                BranchID = poEnabledFSSODetRow.BranchID
            };

            poLineRow = graphPOOrderEntry.Transactions.Current = graphPOOrderEntry.Transactions.Insert(poLineRow);

            poLineRow.InventoryID = poEnabledFSSODetRow.InventoryID;
            poLineRow.SiteID = poEnabledFSSODetRow.SiteID;
            poLineRow.OrderQty = poEnabledFSSODetRow.EstimatedQty;
            poLineRow.ProjectID = poEnabledFSSODetRow.ProjectID;
            poLineRow.TaskID = poEnabledFSSODetRow.ProjectTaskID;
            poLineRow.CuryUnitCost = poEnabledFSSODetRow.UnitCost;
            poLineRow = graphPOOrderEntry.Transactions.Update(poLineRow);

            poEnabledFSSODetRow.POType = poLineRow.OrderType;
            poEnabledFSSODetRow.POLineNbr = poLineRow.LineNbr;
            poEnabledFSSODetRow.POCompleted = poLineRow.Completed;

            this.CopyNotesAndAttachments(graphPOOrderEntry.Transactions.Cache, poLineRow, poEnabledFSSODetRow);
        }

        private void CopyNotesAndAttachments(PXCache cache, POLine poLineRow, POEnabledFSSODet poEnabledFSSODetRow)
        {
            if (poSetup.Current != null
                && (poSetup.Current.CopyLineNotesFromServiceOrder == true
                    || poSetup.Current.CopyLineAttachmentsFromServiceOrder == true))
            {
                SharedFunctions.CopyNotesAndFiles(
                    new PXCache<FSSODet>(cache.Graph), 
                    cache,
                    poEnabledFSSODetRow,
                    poLineRow,
                    poSetup.Current.CopyLineNotesFromServiceOrder,
                    poSetup.Current.CopyLineAttachmentsFromServiceOrder);
            }
        }

        #endregion
    }
}
