using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static PX.Objects.FS.MessageHelper;

namespace PX.Objects.FS
{
    public class SM_SOInvoiceEntry : PXGraphExtension<SOInvoiceEntry>, IInvoiceGraph
    {
        #region Functions
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public virtual bool IsFSIntegrationEnabled()
        {
            if (IsActive() == false)
            {
                return false;
            }


            ARInvoice arInvoiceRow = Base.Document.Current;
            FSxARInvoice fsxARInvoiceRow = Base.Document.Cache.GetExtension<FSxARInvoice>(arInvoiceRow);

            return SM_ARInvoiceEntry.IsFSIntegrationEnabled(arInvoiceRow, fsxARInvoiceRow);
        }
        #endregion

        public PXAction<ARInvoice> release;
        [PXUIField(DisplayName = "Release", Visible = false)]
        [PXButton]
        public IEnumerable Release(PXAdapter adapter)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
            {
                PXGraph.InstanceCreated.AddHandler<ARReleaseProcess>((graph) =>
                {
                    graph.GetExtension<SM_ARReleaseProcess>().processEquipmentAndComponents = true;
                });
            }
            return Base.release.Press(adapter);
        }

        #region Events Handlers

        #region ARInvoice

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<ARInvoice> e)
        {
            SM_ARInvoiceEntry.SetUnpersistedFSInfo(e.Cache, e.Args);
        }

        protected virtual void _(Events.RowSelected<ARInvoice> e)
        {
        }

        protected virtual void _(Events.RowInserting<ARInvoice> e)
        {
        }

        protected virtual void _(Events.RowInserted<ARInvoice> e)
        {
        }

        protected virtual void _(Events.RowUpdating<ARInvoice> e)
        {
        }

        protected virtual void _(Events.RowUpdated<ARInvoice> e)
        {
        }

        protected virtual void _(Events.RowDeleting<ARInvoice> e)
        {
        }

        protected virtual void _(Events.RowDeleted<ARInvoice> e)
        {
        }

        protected virtual void _(Events.RowPersisting<ARInvoice> e)
        {
        }

        protected virtual void _(Events.RowPersisted<ARInvoice> e)
        {
        }

        #endregion

        #region SOInvoice

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<SOInvoice> e)
        {
        }

        protected virtual void _(Events.RowSelected<SOInvoice> e)
        {
        }

        protected virtual void _(Events.RowInserting<SOInvoice> e)
        {
        }

        protected virtual void _(Events.RowInserted<SOInvoice> e)
        {
        }

        protected virtual void _(Events.RowUpdating<SOInvoice> e)
        {
        }

        protected virtual void _(Events.RowUpdated<SOInvoice> e)
        {
        }

        protected virtual void _(Events.RowDeleting<SOInvoice> e)
        {
        }

        protected virtual void _(Events.RowDeleted<SOInvoice> e)
        {
        }

        protected virtual void _(Events.RowPersisting<SOInvoice> e)
        {
        }

        protected virtual void _(Events.RowPersisted<SOInvoice> e)
        {
            if (e.Row == null)
            {
                return;
            }

            SOInvoice soInvoiceRow = (SOInvoice)e.Row;

            if (e.TranStatus == PXTranStatus.Open)
            {
                if (e.Operation == PXDBOperation.Delete)
                {
                    InvoicingFunctions.CleanPostingInfoLinkedToDoc(soInvoiceRow);
                }
            }
        }

        #endregion

        #region ARTran

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<ARTran> e)
        {
        }

        protected virtual void _(Events.RowSelected<ARTran> e)
        {
            bool fsIntegrationEnabled = IsFSIntegrationEnabled();
            DACHelper.SetExtensionVisibleInvisible<FSxARTran>(e.Cache, e.Args, fsIntegrationEnabled, false);

            if (e.Row == null)
            {
                return;
            }
        }

        protected virtual void _(Events.RowInserting<ARTran> e)
        {
        }

        protected virtual void _(Events.RowInserted<ARTran> e)
        {
        }

        protected virtual void _(Events.RowUpdating<ARTran> e)
        {
        }

        protected virtual void _(Events.RowUpdated<ARTran> e)
        {
        }

        protected virtual void _(Events.RowDeleting<ARTran> e)
        {
        }

        protected virtual void _(Events.RowDeleted<ARTran> e)
        {
        }

        protected virtual void _(Events.RowPersisting<ARTran> e)
        {
        }

        protected virtual void _(Events.RowPersisted<ARTran> e)
        {
            if (e.Row == null)
            {
                return;
            }

            ARTran arTranRow = (ARTran)e.Row;

            // We call here cache.GetStateExt for every field when the transaction is aborted
            // to set the errors in the fields and then the Generate Invoice screen can read them
            if (e.TranStatus == PXTranStatus.Aborted && IsInvoiceProcessRunning == true)
            {
                MessageHelper.GetRowMessage(e.Cache, arTranRow, false, false);
            }
        }

        #endregion

        #endregion

        #region Invoicing Methods
        public virtual bool IsInvoiceProcessRunning { get; set; }

        public virtual List<ErrorInfo> GetErrorInfo()
        {
            return MessageHelper.GetErrorInfo<ARTran, FSxARTran>(Base.Document.Cache, Base.Document.Current, Base.Transactions);
        }

        public virtual void CreateInvoice(PXGraph graphProcess, List<DocLineExt> docLines, List<DocLineExt> docLinesGrouped, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID, OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted, OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow)
        {
            if (docLinesGrouped.Count == 0)
            {
                return;
            }

            bool? initialHold = false;

            FSServiceOrder fsServiceOrderRow = docLines[0].fsServiceOrder;
            FSSrvOrdType fsSrvOrdTypeRow = docLines[0].fsSrvOrdType;
            FSPostDoc fsPostDocRow = docLines[0].fsPostDoc;
            FSAppointment fsAppointmentRow = docLines[0].fsAppointment;

            Base.FieldDefaulting.AddHandler<ARInvoice.branchID>((sender, e) =>
            {
                e.NewValue = fsServiceOrderRow.BranchID;
                e.Cancel = true;
            });

            ARInvoice arInvoiceRow = new ARInvoice();

            if (invtMult >= 0)
            {
                arInvoiceRow.DocType = ARInvoiceType.Invoice;
                AutoNumberHelper.CheckAutoNumbering(Base, Base.ARSetup.SelectSingle().InvoiceNumberingID);
            }
            else
            {
                arInvoiceRow.DocType = ARInvoiceType.CreditMemo;
                AutoNumberHelper.CheckAutoNumbering(Base, Base.ARSetup.SelectSingle().CreditAdjNumberingID);
            }

            arInvoiceRow.DocDate = invoiceDate;
            arInvoiceRow.FinPeriodID = invoiceFinPeriodID;
            arInvoiceRow.InvoiceNbr = fsServiceOrderRow.CustPORefNbr;
            arInvoiceRow = Base.Document.Insert(arInvoiceRow);
            initialHold = arInvoiceRow.Hold;
            arInvoiceRow.NoteID = null;
            PXNoteAttribute.GetNoteIDNow(Base.Document.Cache, arInvoiceRow);

            Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.hold>(arInvoiceRow, true);
            Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.customerID>(arInvoiceRow, fsServiceOrderRow.BillCustomerID);
            Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.customerLocationID>(arInvoiceRow, fsServiceOrderRow.BillLocationID);
            Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.curyID>(arInvoiceRow, fsServiceOrderRow.CuryID);

            Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.taxZoneID>(arInvoiceRow, fsAppointmentRow != null ? fsAppointmentRow.TaxZoneID : fsServiceOrderRow.TaxZoneID);
            Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.taxCalcMode>(arInvoiceRow, fsAppointmentRow != null ? fsAppointmentRow.TaxCalcMode : fsServiceOrderRow.TaxCalcMode);

            string termsID = InvoicingFunctions.GetTermsIDFromCustomerOrVendor(graphProcess, fsServiceOrderRow.BillCustomerID, null);
            if (termsID != null)
            {
                Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.termsID>(arInvoiceRow, termsID);
            }
            else
            {
                Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.termsID>(arInvoiceRow, fsSrvOrdTypeRow.DfltTermIDARSO);
            }

            if (fsServiceOrderRow.ProjectID != null)
            {
                Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.projectID>(arInvoiceRow, fsServiceOrderRow.ProjectID);
            }

            Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.docDesc>(arInvoiceRow, fsServiceOrderRow.DocDesc);
            arInvoiceRow.FinPeriodID = invoiceFinPeriodID;
            arInvoiceRow = Base.Document.Update(arInvoiceRow);
            
	        InvoicingFunctions.SetContactAndAddress(Base, fsServiceOrderRow);

			if (onDocumentHeaderInserted != null)
            {
                onDocumentHeaderInserted(Base, arInvoiceRow);
            }

            IDocLine docLine = null;
            ARTran arTranRow = null;
            FSxARTran fsxARTranRow = null;
            PMTask pmTaskRow = null;
            List<SharedClasses.SOARLineEquipmentComponent> componentList = new List<SharedClasses.SOARLineEquipmentComponent>();
            int? acctID;
            int? pivotAppointmentDetID = -1;

            foreach (DocLineExt docLineExt in docLines)
            {
                docLine = docLineExt.docLine;
                if (docLineExt.fsAppointment != null)
                {
                    if (pivotAppointmentDetID != docLineExt.docLine.LineID)
                    {
                        pivotAppointmentDetID = docLineExt.docLine.LineID;
                    }
                    else
                    {
                        continue;
                    }
                }

                FSSODetSplit fsSODetSplitRow = docLineExt.fsSODetSplit;
                fsPostDocRow = docLineExt.fsPostDoc;
                fsServiceOrderRow = docLineExt.fsServiceOrder;
                fsSrvOrdTypeRow = docLineExt.fsSrvOrdType;
                fsAppointmentRow = docLineExt.fsAppointment;

                arTranRow = new ARTran();
                arTranRow = Base.Transactions.Insert(arTranRow);

                arTranRow = (ARTran)Base.Transactions.Cache.CreateCopy(arTranRow);

                arTranRow.BranchID = docLine.BranchID;
                arTranRow.InventoryID = docLine.InventoryID;

                pmTaskRow = docLineExt.pmTask;

                if (pmTaskRow != null && pmTaskRow.Status == ProjectTaskStatus.Completed)
                {
                    throw new PXException(TX.Error.POSTING_PMTASK_ALREADY_COMPLETED, fsServiceOrderRow.RefNbr, docLine.LineRef, pmTaskRow.TaskCD);
                }

                if (docLine.ProjectID != null && docLine.ProjectTaskID != null)
                {
                    arTranRow.TaskID = docLine.ProjectTaskID;
                }

                arTranRow.UOM = fsSODetSplitRow != null && fsSODetSplitRow.SplitLineNbr > 0 ? fsSODetSplitRow.UOM : docLine.UOM;

                arTranRow.SiteID = fsSODetSplitRow != null && fsSODetSplitRow.SplitLineNbr > 0 ? fsSODetSplitRow.SiteID : docLine.SiteID;

                arTranRow = Base.Transactions.Update(arTranRow);

                Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.locationID>(arTranRow, fsSODetSplitRow != null && fsSODetSplitRow.SplitLineNbr > 0 ? fsSODetSplitRow.LocationID : docLine.SiteLocationID);

                if (docLine.IsService == true || fsAppointmentRow != null)
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.qty>(arTranRow, docLine.GetQty(FieldType.BillableField));
                }
                else
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.qty>(arTranRow, fsSODetSplitRow != null && fsSODetSplitRow.SplitLineNbr > 0 ? fsSODetSplitRow.Qty : docLine.GetQty(FieldType.BillableField));
                }

                Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.tranDesc>(arTranRow, docLine.TranDesc);

                fsPostDocRow.DocLineRef = arTranRow = Base.Transactions.Update(arTranRow);

                Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.salesPersonID>(arTranRow, fsAppointmentRow == null ? fsServiceOrderRow.SalesPersonID : fsAppointmentRow.SalesPersonID);

                if (docLine.AcctID != null)
                {
                    acctID = docLine.AcctID;
                }
                else
                {
                    acctID = ServiceOrderCore.Get_TranAcctID_DefaultValue(graphProcess,
                                                                          fsSrvOrdTypeRow.SalesAcctSource,
                                                                          docLine.InventoryID,
                                                                          docLine.SiteID,
                                                                          fsServiceOrderRow);
                }

                Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.accountID>(arTranRow, acctID);

                if (docLine.SubID != null)
                {
                    try
                    {
                        Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.subID>(arTranRow, docLine.SubID);
                    }
                    catch (PXException)
                    {
                        arTranRow.SubID = null;
                    }
                }
                else
                {
                    InvoicingFunctions.SetCombinedSubID(graphProcess,
                                                        Base.Transactions.Cache,
                                                        arTranRow,
                                                        null,
                                                        null,
                                                        fsSrvOrdTypeRow,
                                                        arTranRow.BranchID,
                                                        arTranRow.InventoryID,
                                                        arInvoiceRow.CustomerLocationID,
                                                        fsServiceOrderRow.BranchLocationID,
                                                        fsServiceOrderRow.SalesPersonID,
                                                        docLine.IsService);
                }

                Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.manualPrice>(arTranRow, docLine.ManualPrice);
                Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.curyUnitPrice>(arTranRow, docLine.CuryUnitPrice * invtMult);

                Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.taxCategoryID>(arTranRow, docLine.TaxCategoryID);
                Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.commissionable>(arTranRow, fsAppointmentRow?.Commissionable ?? fsServiceOrderRow.Commissionable ?? false);
                Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.costCodeID>(arTranRow, docLine.CostCodeID);

                if (docLine.IsBillable == false)
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.manualDisc>(arTranRow, true);
                    Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.curyDiscAmt>(arTranRow, docLine.GetQty(FieldType.BillableField) * docLine.CuryUnitPrice * invtMult);
                }
                else
                {
                    decimal? qty = docLine.GetQty(FieldType.BillableField);
                    decimal? extPrice = qty != 0 ? ((docLine.CuryBillableExtPrice * invtMult) / qty) * arTranRow.Qty : 0m;

                    Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.curyExtPrice>(arTranRow, extPrice);
                    Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.discPct>(arTranRow, docLine.DiscPct);
                }


                if (fsAppointmentRow != null && !string.IsNullOrEmpty(docLine.LotSerialNbr))
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.lotSerialNbr>(arTranRow, docLine.LotSerialNbr);
                }
                else if (fsSODetSplitRow.SplitLineNbr > 0 && !string.IsNullOrEmpty(fsSODetSplitRow.LotSerialNbr))
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.lotSerialNbr>(arTranRow, fsSODetSplitRow.LotSerialNbr);
                }

                fsxARTranRow = Base.Transactions.Cache.GetExtension<FSxARTran>(arTranRow);
                fsxARTranRow.Source = docLine.BillingBy;
                fsxARTranRow.SOID = fsServiceOrderRow.SOID;
                fsxARTranRow.ServiceOrderDate = fsServiceOrderRow.OrderDate;

                fsxARTranRow.BillCustomerID = fsServiceOrderRow.CustomerID;
                fsxARTranRow.CustomerLocationID = fsServiceOrderRow.LocationID;

                fsxARTranRow.SODetID = docLine.PostSODetID;
                fsxARTranRow.AppointmentID = docLine.PostAppointmentID;
                fsxARTranRow.AppointmentDate = fsAppointmentRow?.ExecutionDate;
                fsxARTranRow.AppDetID = docLine.PostAppDetID;

                fsxARTranRow.Mem_PreviousPostID = docLine.PostID;
                fsxARTranRow.Mem_TableSource = docLine.SourceTable;

                SharedFunctions.CopyNotesAndFiles(Base.Transactions.Cache, arTranRow, docLine, fsSrvOrdTypeRow);
                fsPostDocRow.DocLineRef = arTranRow = Base.Transactions.Update(arTranRow);

                if (fsAppointmentRow != null && !string.IsNullOrEmpty(docLine.LotSerialNbr))
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.lotSerialNbr>(arTranRow, docLine.LotSerialNbr);
                }
                else if (fsSODetSplitRow != null && fsSODetSplitRow.SplitLineNbr > 0 && !string.IsNullOrEmpty(fsSODetSplitRow.LotSerialNbr))
                {
                    Base.Transactions.Cache.SetValueExtIfDifferent<ARTran.lotSerialNbr>(arTranRow, fsSODetSplitRow.LotSerialNbr);
                }

                if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
                {
                    if (docLine.EquipmentAction != null)
                    {
                        Base.Transactions.Cache.SetValueExtIfDifferent<FSxARTran.equipmentAction>(arTranRow, docLine.EquipmentAction);
                        Base.Transactions.Cache.SetValueExtIfDifferent<FSxARTran.sMEquipmentID>(arTranRow, docLine.SMEquipmentID);
                        Base.Transactions.Cache.SetValueExtIfDifferent<FSxARTran.equipmentLineRef>(arTranRow, docLine.EquipmentLineRef);

                        fsxARTranRow.Comment = docLine.Comment;

                        if (docLine.EquipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT
                            || ((docLine.EquipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                 || docLine.EquipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                 || docLine.EquipmentAction == ID.Equipment_Action.NONE)
                                    && string.IsNullOrEmpty(docLine.NewTargetEquipmentLineNbr) == false))
                        {
                            componentList.Add(new SharedClasses.SOARLineEquipmentComponent(docLine, arTranRow, fsxARTranRow));
                        }
                        else
                        {
                            fsxARTranRow.ComponentID = docLine.ComponentID;
                        }
                    }
                }

                if (onTransactionInserted != null)
                {
                    onTransactionInserted(Base, arTranRow);
                }
            }

            if (componentList.Count > 0)
            {
                //Assigning the NewTargetEquipmentLineNbr field value for the component type records
                foreach (SharedClasses.SOARLineEquipmentComponent currLineModel in componentList.Where(x => x.equipmentAction == ID.Equipment_Action.SELLING_TARGET_EQUIPMENT))
                {
                    foreach (SharedClasses.SOARLineEquipmentComponent currLineComponent in componentList.Where(x => (x.equipmentAction == ID.Equipment_Action.CREATING_COMPONENT
                                                                                                                    || x.equipmentAction == ID.Equipment_Action.UPGRADING_COMPONENT
                                                                                                                    || x.equipmentAction == ID.Equipment_Action.NONE)))
                    {
                        if (currLineComponent.sourceNewTargetEquipmentLineNbr == currLineModel.sourceLineRef)
                        {
                            currLineComponent.fsxARTranRow.ComponentID = currLineComponent.componentID;
                            currLineComponent.fsxARTranRow .NewTargetEquipmentLineNbr = currLineModel.currentLineRef;
                        }
                    }
                }
            }

            arInvoiceRow = Base.Document.Update(arInvoiceRow);

            if (Base.ARSetup.Current.RequireControlTotal == true)
            {
                Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.curyOrigDocAmt>(arInvoiceRow, arInvoiceRow.CuryDocBal);
            }

            if (initialHold != true || quickProcessFlow != PXQuickProcess.ActionFlow.NoFlow)
            {
                Base.Document.Cache.SetValueExtIfDifferent<ARInvoice.hold>(arInvoiceRow, false);
            }

            arInvoiceRow = Base.Document.Update(arInvoiceRow);
        }

        public virtual FSCreatedDoc PressSave(int batchID, List<DocLineExt> docLines, BeforeSaveDelegate beforeSave)
        {
            if (Base.Document.Current == null)
            {
                throw new SharedClasses.TransactionScopeException();
            }

            if (beforeSave != null)
            {
                beforeSave(Base);
            }

            Base.SelectTimeStamp();
            Base.Save.Press();

            ARInvoice arInvoice = Base.Document.Current;

            var fsCreatedDocRow = new FSCreatedDoc()
            {
                BatchID = batchID,
                PostTo = ID.Batch_PostTo.SI,
                CreatedDocType = arInvoice.DocType,
                CreatedRefNbr = arInvoice.RefNbr
            };

            return fsCreatedDocRow;
        }

        public virtual void Clear()
        {
            Base.Clear(PXClearOption.ClearAll);
        }

        public virtual PXGraph GetGraph()
        {
            return Base;
        }

        public virtual void DeleteDocument(FSCreatedDoc fsCreatedDocRow)
        {
            Base.Document.Current = Base.Document.Search<ARInvoice.refNbr>(fsCreatedDocRow.CreatedRefNbr, fsCreatedDocRow.CreatedDocType);

            if (Base.Document.Current != null)
            {
                if (Base.Document.Current.RefNbr == fsCreatedDocRow.CreatedRefNbr
                        && Base.Document.Current.DocType == fsCreatedDocRow.CreatedDocType)
                {
                    Base.Delete.Press();
                }
            }
        }

        public virtual void CleanPostInfo(PXGraph cleanerGraph, FSPostDet fsPostDetRow)
        {
            PXUpdate<
                Set<FSPostInfo.sOInvLineNbr, Null,
                Set<FSPostInfo.sOInvRefNbr, Null,
                Set<FSPostInfo.sOInvDocType, Null,
                Set<FSPostInfo.sOInvPosted, False>>>>,
            FSPostInfo,
            Where<
                FSPostInfo.postID, Equal<Required<FSPostInfo.postID>>,
                And<FSPostInfo.sOInvPosted, Equal<True>>>>
            .Update(cleanerGraph, fsPostDetRow.PostID);
        }

        public virtual bool IsLineCreatedFromAppSO(PXGraph cleanerGraph, object document, object lineDoc, string fieldName)
        {
            if (document == null || lineDoc == null)
            {
                return false;
            }

            string refNbr = ((ARInvoice)document).RefNbr;
            string docType = ((ARInvoice)document).DocType;
            int? lineNbr = ((ARTran)lineDoc).LineNbr;

            return PXSelect<FSPostInfo,
                   Where<
                       FSPostInfo.sOInvRefNbr, Equal<Required<FSPostInfo.sOInvRefNbr>>,
                       And<FSPostInfo.sOInvDocType, Equal<Required<FSPostInfo.sOInvDocType>>,
                       And<FSPostInfo.sOInvLineNbr, Equal<Required<FSPostInfo.sOInvLineNbr>>,
                       And<FSPostInfo.sOInvPosted, Equal<True>>>>>>
                   .Select(cleanerGraph, refNbr, docType, lineNbr).Count() > 0;
        }

        public virtual void UpdateCostAndPrice(List<DocLineExt> docLines)
        {
        }
        #endregion
    }
}
