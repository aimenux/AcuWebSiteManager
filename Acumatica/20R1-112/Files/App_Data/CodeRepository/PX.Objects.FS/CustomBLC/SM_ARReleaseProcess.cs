using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class SM_ARReleaseProcess : PXGraphExtension<ARReleaseProcess>
    {
        #region ItemInfo
        public class ItemInfo
        {
            public virtual string LotSerialNbr { get; set; }
            public virtual string UOM { get; set; }
            public virtual decimal? Qty { get; set; }
            public virtual decimal? BaseQty { get; set; }

            #region Ctors
            public ItemInfo(SOShipLineSplit split)
            {
                LotSerialNbr = split.LotSerialNbr;
                UOM = split.UOM;
                Qty = split.Qty;
                BaseQty = split.BaseQty;
            }
            public ItemInfo(SOLineSplit split)
            {
                LotSerialNbr = split.LotSerialNbr;
                UOM = split.UOM;
                Qty = split.Qty;
                BaseQty = split.BaseQty;
            }
            public ItemInfo(ARTran arTran)
            {
                LotSerialNbr = arTran.LotSerialNbr;
                UOM = arTran.UOM;
                Qty = arTran.Qty;
                BaseQty = arTran.BaseQty;
            }
            #endregion
        }
        #endregion

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public bool processEquipmentAndComponents = false;

        #region Overrides
        public delegate void PersistDelegate();

        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            if (SharedFunctions.isFSSetupSet(Base) == false
                    || PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == false)
            {
                baseMethod();
                return;
            }

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                if (processEquipmentAndComponents)
                {
            ARRegister arRegisterRow = (ARRegister)Base.Caches[typeof(ARRegister)].Current;
            Dictionary<int?, int?> newEquiments = new Dictionary<int?, int?>();
            SMEquipmentMaint graphSMEquipmentMaint = PXGraph.CreateInstance<SMEquipmentMaint>();

                CreateEquipments(graphSMEquipmentMaint, arRegisterRow, newEquiments);
                ReplaceEquipments(graphSMEquipmentMaint, arRegisterRow);
                UpgradeEquipmentComponents(graphSMEquipmentMaint, arRegisterRow, newEquiments);
                CreateEquipmentComponents(graphSMEquipmentMaint, arRegisterRow, newEquiments);
                ReplaceComponents(graphSMEquipmentMaint, arRegisterRow);
            }

            baseMethod();

                ts.Complete();
            }
        }

        public delegate ARRegister OnBeforeReleaseDelegate(ARRegister ardoc);
        
        [PXOverride]
        public virtual ARRegister OnBeforeRelease(ARRegister ardoc, OnBeforeReleaseDelegate del)
        {
            ValidatePostBatchStatus(PXDBOperation.Update, ID.Batch_PostTo.AR, ardoc.DocType, ardoc.RefNbr);

            if (del != null)
            {
                return del(ardoc);
            }

            return null;
        }
        #endregion

        #region Methods
        public virtual void CreateEquipments(SMEquipmentMaint graphSMEquipmentMaint,
                                             ARRegister arRegisterRow,
                                             Dictionary<int?, int?> newEquiments)
        {
            var inventoryItemSet = PXSelectJoin<InventoryItem,
                                   InnerJoin<ARTran,
                                            On<ARTran.inventoryID, Equal<InventoryItem.inventoryID>,
                                            And<ARTran.tranType, Equal<ARDocType.invoice>>>,
                                   LeftJoin<SOLine,
                                            On<SOLine.orderType, Equal<ARTran.sOOrderType>,
                                            And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>,
                                            And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>>>,
                                   Where<
                                        ARTran.tranType, Equal<Required<ARInvoice.docType>>,
                                        And<ARTran.refNbr, Equal<Required<ARInvoice.refNbr>>,
                                        And<FSxEquipmentModel.eQEnabled, Equal<True>,
                                        And<FSxARTran.equipmentAction, Equal<ListField_EquipmentAction.SellingTargetEquipment>,
                                        And<FSxARTran.sMEquipmentID, IsNull,
                                        And<FSxARTran.newTargetEquipmentLineNbr, IsNull,
                                        And<FSxARTran.componentID, IsNull>>>>>>>,
                                   OrderBy<
                                        Asc<ARTran.tranType,
                                        Asc<ARTran.refNbr,
                                        Asc<ARTran.lineNbr>>>>>
                                   .Select(Base, arRegisterRow.DocType, arRegisterRow.RefNbr);

            Create_Replace_Equipments(graphSMEquipmentMaint, inventoryItemSet, arRegisterRow, newEquiments, ID.Equipment_Action.SELLING_TARGET_EQUIPMENT);
        }

        public virtual void UpgradeEquipmentComponents(SMEquipmentMaint graphSMEquipmentMaint,
                                                       ARRegister arRegisterRow,
                                                       Dictionary<int?, int?> newEquiments)
        {
            var inventoryItemSet = PXSelectJoin<InventoryItem,
                                   InnerJoin<ARTran,
                                            On<ARTran.inventoryID, Equal<InventoryItem.inventoryID>,
                                            And<ARTran.tranType, Equal<ARDocType.invoice>>>,
                                    LeftJoin<SOLine,
                                            On<SOLine.orderType, Equal<ARTran.sOOrderType>,
                                            And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>,
                                            And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>>>,
                                   Where<
                                        ARTran.tranType, Equal<Required<ARInvoice.docType>>,
                                        And<ARTran.refNbr, Equal<Required<ARInvoice.refNbr>>,
                                        And<FSxARTran.equipmentAction, Equal<ListField_EquipmentAction.UpgradingComponent>,
                                        And<FSxARTran.sMEquipmentID, IsNull,
                                        And<FSxARTran.newTargetEquipmentLineNbr, IsNotNull,
                                        And<FSxARTran.componentID, IsNotNull,
                                        And<FSxARTran.equipmentLineRef, IsNull>>>>>>>,
                                   OrderBy<
                                        Asc<ARTran.tranType,
                                        Asc<ARTran.refNbr,
                                        Asc<ARTran.lineNbr>>>>>
                                   .Select(Base, arRegisterRow.DocType, arRegisterRow.RefNbr);

            foreach (PXResult<InventoryItem, ARTran, SOLine> bqlResult in inventoryItemSet)
            {
                ARTran arTranRow = (ARTran)bqlResult;
                SOLine soLineRow = (SOLine)bqlResult;
                InventoryItem inventoryItemRow = (InventoryItem)bqlResult;
                FSxARTran fsxARTranRow = PXCache<ARTran>.GetExtension<FSxARTran>(arTranRow);

                int? smEquipmentID = -1;
                if (newEquiments.TryGetValue(fsxARTranRow.NewTargetEquipmentLineNbr, out smEquipmentID))
                {
                    graphSMEquipmentMaint.EquipmentRecords.Current = graphSMEquipmentMaint.EquipmentRecords.Search<FSEquipment.SMequipmentID>(smEquipmentID);

                    FSEquipmentComponent fsEquipmentComponentRow = graphSMEquipmentMaint.EquipmentWarranties.Select().Where(x => ((FSEquipmentComponent)x).ComponentID == fsxARTranRow.ComponentID).FirstOrDefault();

                    if (fsEquipmentComponentRow != null)
                    {
                        fsEquipmentComponentRow.SalesOrderNbr = arTranRow.SOOrderNbr;
                        fsEquipmentComponentRow.SalesOrderType = arTranRow.SOOrderType;
                        fsEquipmentComponentRow.LongDescr = arTranRow.TranDesc;
                        fsEquipmentComponentRow.InvoiceRefNbr = arTranRow.RefNbr;
                        fsEquipmentComponentRow.InstallationDate = arTranRow.TranDate != null ? arTranRow.TranDate : arRegisterRow.DocDate;

                        if (fsxARTranRow != null)
                        {
                            if (fsxARTranRow.AppointmentID != null)
                            {
                                fsEquipmentComponentRow.InstAppointmentID = fsxARTranRow.AppointmentID;
                                fsEquipmentComponentRow.InstallationDate = fsxARTranRow.AppointmentDate;
                            }
                            else if (fsxARTranRow.SOID != null)
                            {
                                fsEquipmentComponentRow.InstServiceOrderID = fsxARTranRow.SOID;
                                fsEquipmentComponentRow.InstallationDate = fsxARTranRow.ServiceOrderDate;
                            }

                            fsEquipmentComponentRow.Comment = fsxARTranRow.Comment;
                        }

                        // Component actions are assumed to always run for only one item per line (BaseQty == 1).
                        fsEquipmentComponentRow.SerialNumber = arTranRow.LotSerialNbr;

                        fsEquipmentComponentRow = graphSMEquipmentMaint.EquipmentWarranties.Update(fsEquipmentComponentRow);

                        graphSMEquipmentMaint.EquipmentWarranties.SetValueExt<FSEquipmentComponent.inventoryID>(fsEquipmentComponentRow, arTranRow.InventoryID);
                        graphSMEquipmentMaint.EquipmentWarranties.SetValueExt<FSEquipmentComponent.salesDate>(fsEquipmentComponentRow, soLineRow != null && soLineRow.OrderDate != null ? soLineRow.OrderDate : arTranRow.TranDate);
                        graphSMEquipmentMaint.Save.Press();
                    }
                }
            }
        }

        public virtual void CreateEquipmentComponents(SMEquipmentMaint graphSMEquipmentMaint,
                                                      ARRegister arRegisterRow,
                                                      Dictionary<int?, int?> newEquiments)
        {
            var inventoryItemSet = PXSelectJoin<InventoryItem,
                                   InnerJoin<ARTran,
                                            On<ARTran.inventoryID, Equal<InventoryItem.inventoryID>,
                                            And<ARTran.tranType, Equal<ARDocType.invoice>>>,
                                   LeftJoin<SOLine,
                                        On<SOLine.orderType, Equal<ARTran.sOOrderType>,
                                        And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>,
                                        And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>>>,
                                   Where<
                                        ARTran.tranType, Equal<Required<ARInvoice.docType>>,
                                        And<ARTran.refNbr, Equal<Required<ARInvoice.refNbr>>,
                                        And<FSxARTran.equipmentAction, Equal<ListField_EquipmentAction.CreatingComponent>,
                                        And<FSxARTran.componentID, IsNotNull,
                                        And<FSxARTran.equipmentLineRef, IsNull>>>>>,
                                   OrderBy<
                                        Asc<ARTran.tranType,
                                        Asc<ARTran.refNbr,
                                        Asc<ARTran.lineNbr>>>>>
                                   .Select(Base, arRegisterRow.DocType, arRegisterRow.RefNbr);

            foreach (PXResult<InventoryItem, ARTran, SOLine> bqlResult in inventoryItemSet)
            {
                ARTran arTranRow = (ARTran)bqlResult;
                InventoryItem inventoryItemRow = (InventoryItem)bqlResult;
                SOLine soLineRow = (SOLine)bqlResult;
                FSxARTran fsxARTranRow = PXCache<ARTran>.GetExtension<FSxARTran>(arTranRow);

                int? smEquipmentID = -1;
                if (fsxARTranRow.NewTargetEquipmentLineNbr != null && fsxARTranRow.SMEquipmentID == null)
                {
                    if (newEquiments.TryGetValue(fsxARTranRow.NewTargetEquipmentLineNbr, out smEquipmentID))
                    {
                        graphSMEquipmentMaint.EquipmentRecords.Current = graphSMEquipmentMaint.EquipmentRecords.Search<FSEquipment.SMequipmentID>(smEquipmentID);
                    }
                }

                if (fsxARTranRow.NewTargetEquipmentLineNbr == null && fsxARTranRow.SMEquipmentID != null)
                {
                    graphSMEquipmentMaint.EquipmentRecords.Current = graphSMEquipmentMaint.EquipmentRecords.Search<FSEquipment.SMequipmentID>(fsxARTranRow.SMEquipmentID);
                }

                if (graphSMEquipmentMaint.EquipmentRecords.Current != null)
                {
                    FSEquipmentComponent fsEquipmentComponentRow = new FSEquipmentComponent();
                    fsEquipmentComponentRow.ComponentID = fsxARTranRow.ComponentID;
                    fsEquipmentComponentRow = graphSMEquipmentMaint.EquipmentWarranties.Insert(fsEquipmentComponentRow);

                    fsEquipmentComponentRow.SalesOrderNbr = arTranRow.SOOrderNbr;
                    fsEquipmentComponentRow.SalesOrderType = arTranRow.SOOrderType;
                    fsEquipmentComponentRow.InvoiceRefNbr = arTranRow.RefNbr;
                    fsEquipmentComponentRow.InstallationDate = arTranRow.TranDate != null ? arTranRow.TranDate : arRegisterRow.DocDate;

                    if (fsxARTranRow != null)
                    {
                        if (fsxARTranRow.AppointmentID != null)
                        {
                            fsEquipmentComponentRow.InstAppointmentID = fsxARTranRow.AppointmentID;
                            fsEquipmentComponentRow.InstallationDate = fsxARTranRow.AppointmentDate;
                        }
                        else if (fsxARTranRow.SOID != null)
                        {
                            fsEquipmentComponentRow.InstServiceOrderID = fsxARTranRow.SOID;
                            fsEquipmentComponentRow.InstallationDate = fsxARTranRow.ServiceOrderDate;
                        }

                        fsEquipmentComponentRow.Comment = fsxARTranRow.Comment;
                    }

                    // Component actions are assumed to always run for only one item per line (BaseQty == 1).
                    fsEquipmentComponentRow.SerialNumber = arTranRow.LotSerialNbr;

                    fsEquipmentComponentRow = graphSMEquipmentMaint.EquipmentWarranties.Update(fsEquipmentComponentRow);

                    graphSMEquipmentMaint.EquipmentWarranties.SetValueExt<FSEquipmentComponent.inventoryID>(fsEquipmentComponentRow, arTranRow.InventoryID);
                    graphSMEquipmentMaint.EquipmentWarranties.SetValueExt<FSEquipmentComponent.salesDate>(fsEquipmentComponentRow, soLineRow != null && soLineRow.OrderDate != null ? soLineRow.OrderDate : arTranRow.TranDate);
                    graphSMEquipmentMaint.Save.Press();
                }
            }
        }

        public virtual void ReplaceEquipments(SMEquipmentMaint graphSMEquipmentMaint, ARRegister arRegisterRow)
        {
            var inventoryItemSet = PXSelectJoin<InventoryItem,
                                   InnerJoin<ARTran,
                                            On<ARTran.inventoryID, Equal<InventoryItem.inventoryID>,
                                            And<ARTran.tranType, Equal<ARDocType.invoice>>>,
                                   LeftJoin<SOLine,
                                        On<SOLine.orderType, Equal<ARTran.sOOrderType>,
                                        And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>,
                                        And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>>>,
                                   Where<
                                        ARTran.tranType, Equal<Required<ARInvoice.docType>>,
                                        And<ARTran.refNbr, Equal<Required<ARInvoice.refNbr>>,
                                        And<FSxEquipmentModel.eQEnabled, Equal<True>,
                                        And<FSxARTran.equipmentAction, Equal<ListField_EquipmentAction.ReplacingTargetEquipment>,
                                        And<FSxARTran.sMEquipmentID, IsNotNull,
                                        And<FSxARTran.newTargetEquipmentLineNbr, IsNull,
                                        And<FSxARTran.componentID, IsNull>>>>>>>,
                                   OrderBy<
                                        Asc<ARTran.tranType,
                                        Asc<ARTran.refNbr,
                                        Asc<ARTran.lineNbr>>>>>
                                   .Select(Base, arRegisterRow.DocType, arRegisterRow.RefNbr);

            Create_Replace_Equipments(graphSMEquipmentMaint, inventoryItemSet, arRegisterRow, null, ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT);
        }

        public virtual void ReplaceComponents(SMEquipmentMaint graphSMEquipmentMaint, ARRegister arRegisterRow)
        {
            var inventoryItemSet = PXSelectJoin<InventoryItem,
                                   InnerJoin<ARTran,
                                            On<ARTran.inventoryID, Equal<InventoryItem.inventoryID>,
                                            And<ARTran.tranType, Equal<ARDocType.invoice>>>,
                                   LeftJoin<SOLine,
                                        On<SOLine.orderType, Equal<ARTran.sOOrderType>,
                                        And<SOLine.orderNbr, Equal<ARTran.sOOrderNbr>,
                                        And<SOLine.lineNbr, Equal<ARTran.sOOrderLineNbr>>>>>>,
                                   Where<
                                        ARTran.tranType, Equal<Required<ARInvoice.docType>>,
                                        And<ARTran.refNbr, Equal<Required<ARInvoice.refNbr>>,
                                        And<FSxEquipmentModel.eQEnabled, Equal<True>,
                                        And<FSxARTran.equipmentAction, Equal<ListField_EquipmentAction.ReplacingComponent>,
                                        And<FSxARTran.sMEquipmentID, IsNotNull,
                                        And<FSxARTran.newTargetEquipmentLineNbr, IsNull,
                                        And<FSxARTran.equipmentLineRef, IsNotNull>>>>>>>,
                                   OrderBy<
                                        Asc<ARTran.tranType,
                                        Asc<ARTran.refNbr,
                                        Asc<ARTran.lineNbr>>>>>
                                   .Select(Base, arRegisterRow.DocType, arRegisterRow.RefNbr);

            foreach (PXResult<InventoryItem, ARTran, SOLine> bqlResult in inventoryItemSet)
            {
                ARTran arTranRow = (ARTran)bqlResult;
                InventoryItem inventoryItemRow = (InventoryItem)bqlResult;
                SOLine soLineRow = (SOLine)bqlResult;
                FSxARTran fsxARTranRow = PXCache<ARTran>.GetExtension<FSxARTran>(arTranRow);

                graphSMEquipmentMaint.EquipmentRecords.Current = graphSMEquipmentMaint.EquipmentRecords.Search<FSEquipment.SMequipmentID>(fsxARTranRow.SMEquipmentID);

                FSEquipmentComponent fsEquipmentComponentRow = graphSMEquipmentMaint.EquipmentWarranties.Select().Where(x => ((FSEquipmentComponent)x).LineNbr == fsxARTranRow.EquipmentLineRef).FirstOrDefault();

                FSEquipmentComponent fsNewEquipmentComponentRow = new FSEquipmentComponent();
                fsNewEquipmentComponentRow.ComponentID = fsxARTranRow.ComponentID;
                fsNewEquipmentComponentRow = graphSMEquipmentMaint.ApplyComponentReplacement(fsEquipmentComponentRow, fsNewEquipmentComponentRow);

                fsNewEquipmentComponentRow.SalesOrderNbr = arTranRow.SOOrderNbr;
                fsNewEquipmentComponentRow.SalesOrderType = arTranRow.SOOrderType;
                fsNewEquipmentComponentRow.InvoiceRefNbr = arTranRow.RefNbr;
                fsNewEquipmentComponentRow.InstallationDate = arTranRow.TranDate != null ? arTranRow.TranDate : arRegisterRow.DocDate;

                if (fsxARTranRow != null)
                {
                    if (fsxARTranRow.AppointmentID != null)
                    {
                        fsNewEquipmentComponentRow.InstAppointmentID = fsxARTranRow.AppointmentID;
                        fsNewEquipmentComponentRow.InstallationDate = fsxARTranRow.AppointmentDate;
                    }
                    else if (fsxARTranRow.SOID != null)
                    {
                        fsNewEquipmentComponentRow.InstServiceOrderID = fsxARTranRow.SOID;
                        fsNewEquipmentComponentRow.InstallationDate = fsxARTranRow.ServiceOrderDate;
                    }

                    fsNewEquipmentComponentRow.Comment = fsxARTranRow.Comment;
                }

                fsNewEquipmentComponentRow.LongDescr = arTranRow.TranDesc;

                // Component actions are assumed to always run for only one item per line (BaseQty == 1).
                fsNewEquipmentComponentRow.SerialNumber = arTranRow.LotSerialNbr;

                fsNewEquipmentComponentRow = graphSMEquipmentMaint.EquipmentWarranties.Update(fsNewEquipmentComponentRow);

                graphSMEquipmentMaint.EquipmentWarranties.SetValueExt<FSEquipmentComponent.inventoryID>(fsNewEquipmentComponentRow, arTranRow.InventoryID);
                graphSMEquipmentMaint.EquipmentWarranties.SetValueExt<FSEquipmentComponent.salesDate>(fsNewEquipmentComponentRow, soLineRow != null && soLineRow.OrderDate != null ? soLineRow.OrderDate : arTranRow.TranDate);
                graphSMEquipmentMaint.Save.Press();
            }
        }

        // TODO: Change PXResultset<InventoryItem> to PXResult<InventoryItem, ARTran, SOLine>
        public virtual void Create_Replace_Equipments(
            SMEquipmentMaint graphSMEquipmentMaint,
            PXResultset<InventoryItem> arTranLines,
            ARRegister arRegisterRow,
            Dictionary<int?, int?> newEquiments,
            string action)
        {
            foreach (PXResult<InventoryItem, ARTran, SOLine> result in arTranLines)
            {
                ARTran arTranRow = (ARTran)result;

                //Fetching the cached data record for ARTran that will be updated later
                arTranRow = PXSelect<ARTran,
                            Where<
                                ARTran.tranType, Equal<Required<ARTran.tranType>>,
                                And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
                                And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>
                            .Select(Base, arTranRow.TranType, arTranRow.RefNbr, arTranRow.LineNbr);

                InventoryItem inventoryItemRow = (InventoryItem)result;
                SOLine soLineRow = (SOLine)result;

                FSEquipment fsEquipmentRow = null;
                FSxARTran fsxARTranRow = PXCache<ARTran>.GetExtension<FSxARTran>(arTranRow);
                FSxEquipmentModel fsxEquipmentModelRow = PXCache<InventoryItem>.GetExtension<FSxEquipmentModel>(inventoryItemRow);

                foreach (ItemInfo itemInfo in GetDifferentItemList(Base, arTranRow, true))
                {
                    SoldInventoryItem soldInventoryItemRow = new SoldInventoryItem();

                    soldInventoryItemRow.CustomerID = arRegisterRow.CustomerID;
                    soldInventoryItemRow.CustomerLocationID = arRegisterRow.CustomerLocationID;
                    soldInventoryItemRow.InventoryID = inventoryItemRow.InventoryID;
                    soldInventoryItemRow.InventoryCD = inventoryItemRow.InventoryCD;
                    soldInventoryItemRow.InvoiceRefNbr = arTranRow.RefNbr;
                    soldInventoryItemRow.InvoiceLineNbr = arTranRow.LineNbr;
                    soldInventoryItemRow.DocType = arRegisterRow.DocType;
                    soldInventoryItemRow.DocDate = arTranRow.TranDate != null ? arTranRow.TranDate : arRegisterRow.DocDate;

                    if (fsxARTranRow != null)
                    {
                        if (fsxARTranRow.AppointmentID != null)
                        {
                            soldInventoryItemRow.DocDate = fsxARTranRow.AppointmentDate;
                        } else if (fsxARTranRow.SOID != null)
                        {
                            soldInventoryItemRow.DocDate = fsxARTranRow.ServiceOrderDate;
                        }
                    }

                    soldInventoryItemRow.Descr = inventoryItemRow.Descr;
                    soldInventoryItemRow.SiteID = arTranRow.SiteID;
                    soldInventoryItemRow.ItemClassID = inventoryItemRow.ItemClassID;
                    soldInventoryItemRow.SOOrderType = arTranRow.SOOrderType;
                    soldInventoryItemRow.SOOrderNbr = arTranRow.SOOrderNbr;
                    soldInventoryItemRow.SOOrderDate = soLineRow.OrderDate;
                    soldInventoryItemRow.EquipmentTypeID = fsxEquipmentModelRow.EquipmentTypeID;

                    soldInventoryItemRow.LotSerialNumber = itemInfo.LotSerialNbr;

                    fsEquipmentRow = SharedFunctions.CreateSoldEquipment(graphSMEquipmentMaint, soldInventoryItemRow, arTranRow, fsxARTranRow, soLineRow, action, inventoryItemRow);
                }

                if (fsEquipmentRow != null)
                {
                    if (fsxARTranRow.SuspendedSMEquipmentID == null
                        && action == ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT)
                    {
                        fsxARTranRow.SuspendedSMEquipmentID = fsxARTranRow.SMEquipmentID;
                    }

                    fsxARTranRow.SMEquipmentID = fsEquipmentRow.SMEquipmentID;
                    Base.ARTran_TranType_RefNbr.Update(arTranRow);

                    if (action == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT)
                    {
                        int? smEquipmentID = -1;
                        if (newEquiments.TryGetValue(arTranRow.LineNbr, out smEquipmentID) == false)
                        {
                            newEquiments.Add(
                                arTranRow.LineNbr,
                                fsEquipmentRow.SMEquipmentID);
                        }
                    }
                    else if (action == ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT)
                    {
                        if (fsxARTranRow != null)
                        {
                            graphSMEquipmentMaint.EquipmentRecords.Current = graphSMEquipmentMaint.EquipmentRecords.Search<FSEquipment.SMequipmentID>(fsxARTranRow.SuspendedSMEquipmentID);
                            graphSMEquipmentMaint.EquipmentRecords.Current.ReplaceEquipmentID = fsEquipmentRow.SMEquipmentID;
                            graphSMEquipmentMaint.EquipmentRecords.Current.Status = ID.Equipment_Status.DISPOSED;
                            graphSMEquipmentMaint.EquipmentRecords.Current.DisposalDate = soLineRow.OrderDate != null ? soLineRow.OrderDate : arTranRow.TranDate;
                            graphSMEquipmentMaint.EquipmentRecords.Current.DispServiceOrderID = fsxARTranRow.SOID;
                            graphSMEquipmentMaint.EquipmentRecords.Current.DispAppointmentID = fsxARTranRow.AppointmentID;
                            graphSMEquipmentMaint.EquipmentRecords.Cache.SetStatus(graphSMEquipmentMaint.EquipmentRecords.Current, PXEntryStatus.Updated);
                            graphSMEquipmentMaint.Save.Press();
                        }
                    }
                }
            }
        }

        public virtual List<ItemInfo> GetDifferentItemList(PXGraph graph, ARTran arTran, bool createDifferentEntriesForQtyGreaterThan1)
        {
            if (arTran.InventoryID == null)
            {
                return null;
            }

            var lotSerialList = new List<ItemInfo>();
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(graph.Caches[typeof(InventoryItem)], arTran.InventoryID);

            if (item == null || ((INLotSerClass)item).LotSerTrack == INLotSerTrack.NotNumbered)
            {
                var itemInfo = new ItemInfo(arTran);

                // Currently equipment don't have UOM specification,
                // so we use BaseQty to create equipment of the base unit.
                itemInfo.UOM = null;
                itemInfo.Qty = null;

                if (createDifferentEntriesForQtyGreaterThan1 == false)
                {
                    lotSerialList.Add(itemInfo);
                }
                else
                {
                    itemInfo.BaseQty = 1;
                    for (int i = 0; i < arTran.BaseQty; i++)
                    {
                        lotSerialList.Add(itemInfo);
                    }
                }
            }
            else if (arTran.SOShipmentNbr != null && arTran.SOShipmentLineNbr != null)
            {
                PXResultset<SOShipLineSplit> lotSerialSplits = PXSelect<SOShipLineSplit,
                    Where<SOShipLineSplit.shipmentNbr, Equal<Required<SOShipLineSplit.shipmentNbr>>,
                        And<SOShipLineSplit.lineNbr, Equal<Required<SOShipLineSplit.lineNbr>>>>>.
                    Select(graph, arTran.SOShipmentNbr, arTran.SOShipmentLineNbr);

                foreach (SOShipLineSplit shipLineSplit in lotSerialSplits)
                {
                    var split = new ItemInfo(shipLineSplit);

                    // Currently equipment don't have UOM specification,
                    // so we use BaseQty to create equipment of the base unit.
                    split.UOM = null;
                    split.Qty = null;

                    if (createDifferentEntriesForQtyGreaterThan1 == false)
                    {
                        lotSerialList.Add(split);
                    }
                    else
                    {
                        split.BaseQty = 1;
                        for (int i = 0; i < shipLineSplit.BaseQty; i++)
                        {
                            lotSerialList.Add(split);
                        }
                    }
                }
            }
            else if (arTran.SOOrderType != null && arTran.SOOrderNbr != null && arTran.SOOrderLineNbr != null)
            {
                PXResultset<SOLineSplit> lotSerialSplits = PXSelect<SOLineSplit,
                    Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
                        And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>,
                        And<SOLineSplit.lineNbr, Equal<Required<SOLineSplit.lineNbr>>>>>>.
                    Select(graph, arTran.SOOrderType, arTran.SOOrderNbr, arTran.SOOrderLineNbr);

                foreach (SOLineSplit soLineSplit in lotSerialSplits.RowCast<SOLineSplit>().Where(e => string.IsNullOrEmpty(e.LotSerialNbr) == false))
                {
                    var split = new ItemInfo(soLineSplit);

                    // Currently equipment don't have UOM specification,
                    // so we use BaseQty to create equipment of the base unit.
                    split.UOM = null;
                    split.Qty = null;

                    if (createDifferentEntriesForQtyGreaterThan1 == false)
                    {
                        lotSerialList.Add(split);
                    }
                    else
                    {
                        split.BaseQty = 1;
                        for (int i = 0; i < soLineSplit.BaseQty; i++)
                        {
                            lotSerialList.Add(split);
                        }
                    }
                }
            }

            return GetVerifiedDifferentItemList(graph, arTran, lotSerialList);
        }

        public virtual List<ItemInfo> GetVerifiedDifferentItemList(PXGraph graph, ARTran arTran, List<ItemInfo> lotSerialList)
        {
            if (lotSerialList == null)
            {
                lotSerialList = new List<ItemInfo>();
            }

            if (lotSerialList.Count > arTran.BaseQty)
            {
                throw new PXException(TX.Error.ThereAreMoreLotSerialNumbersThanQuantitySpecifiedOnTheLine);
            }

            return lotSerialList;
        }

        protected virtual PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? inventoryID)
        {
            if (inventoryID == null)
                return null;
            var inventory = InventoryItem.PK.Find(sender.Graph, inventoryID);
            if (inventory == null)
                throw new PXException(ErrorMessages.ValueDoesntExistOrNoRights, IN.Messages.InventoryItem, inventoryID);
            INLotSerClass lotSerClass;
            if (inventory.StkItem == true)
            {
                lotSerClass = INLotSerClass.PK.Find(sender.Graph, inventory.LotSerClassID);
                if (lotSerClass == null)
                    throw new PXException(ErrorMessages.ValueDoesntExistOrNoRights, IN.Messages.LotSerClass, inventory.LotSerClassID);
            }
            else
            {
                lotSerClass = new INLotSerClass();
            }
            return new PXResult<InventoryItem, INLotSerClass>(inventory, lotSerClass);
        }
        #endregion

        #region Validations
        public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus<ARRegister>(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion
    }
}
