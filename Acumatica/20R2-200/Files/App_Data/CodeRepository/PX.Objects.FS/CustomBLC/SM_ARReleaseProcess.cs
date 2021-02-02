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

            //// Invoice
            ARRegister arRegisterRow = (ARRegister)Base.Caches[typeof(ARRegister)].Current;
            Dictionary<int?, int?> newEquiments = new Dictionary<int?, int?>();
            SMEquipmentMaint graphSMEquipmentMaint = PXGraph.CreateInstance<SMEquipmentMaint>();

            if (processEquipmentAndComponents)
            {
                CreateEquipments(graphSMEquipmentMaint, arRegisterRow, newEquiments);
                ReplaceEquipments(graphSMEquipmentMaint, arRegisterRow);
                UpgradeEquipmentComponents(graphSMEquipmentMaint, arRegisterRow, newEquiments);
                CreateEquipmentComponents(graphSMEquipmentMaint, arRegisterRow, newEquiments);
                ReplaceComponents(graphSMEquipmentMaint, arRegisterRow);
            }

            baseMethod();
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

                fsNewEquipmentComponentRow.SerialNumber = arTranRow.LotSerialNbr;

                fsNewEquipmentComponentRow = graphSMEquipmentMaint.EquipmentWarranties.Update(fsNewEquipmentComponentRow);

                graphSMEquipmentMaint.EquipmentWarranties.SetValueExt<FSEquipmentComponent.inventoryID>(fsNewEquipmentComponentRow, arTranRow.InventoryID);
                graphSMEquipmentMaint.EquipmentWarranties.SetValueExt<FSEquipmentComponent.salesDate>(fsNewEquipmentComponentRow, soLineRow != null && soLineRow.OrderDate != null ? soLineRow.OrderDate : arTranRow.TranDate);
                graphSMEquipmentMaint.Save.Press();
            }
        }

        public virtual void Create_Replace_Equipments(SMEquipmentMaint graphSMEquipmentMaint,
                                                      PXResultset<InventoryItem> inventoryItemSet,
                                                      ARRegister arRegisterRow,
                                                      Dictionary<int?, int?> newEquiments,
                                                      string action)
        {
            foreach (PXResult<InventoryItem, ARTran, SOLine> bqlResult in inventoryItemSet)
            {
                ARTran arTranRow = (ARTran)bqlResult;

                //Fetching the cached data record for ARTran that will be updated later
                arTranRow = PXSelect<ARTran,
                            Where<
                                ARTran.tranType, Equal<Required<ARTran.tranType>>,
                                And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
                                And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>
                            .Select(Base, arTranRow.TranType, arTranRow.RefNbr, arTranRow.LineNbr);

                InventoryItem inventoryItemRow = (InventoryItem)bqlResult;
                SOLine soLineRow = (SOLine)bqlResult;

                FSEquipment fsEquipmentRow = null;
                FSxARTran fsxARTranRow = PXCache<ARTran>.GetExtension<FSxARTran>(arTranRow);
                FSxEquipmentModel fsxEquipmentModelRow = PXCache<InventoryItem>.GetExtension<FSxEquipmentModel>(inventoryItemRow);

                for (int i = 0; i < arTranRow.Qty; i++)
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
                    soldInventoryItemRow.LotSerialNumber = arTranRow.LotSerialNbr;

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
        #endregion

        #region Validations
        public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus<ARRegister>(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion
    }
}
