using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        #region Events
        protected virtual void ARInvoice_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            SM_ARInvoiceEntry.SetUnpersistedFSInfo(cache, e);
        }

        protected virtual void SOInvoice_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
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

        protected virtual void ARTran_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            bool fsIntegrationEnabled = IsFSIntegrationEnabled();
            DACHelper.SetExtensionVisibleInvisible<FSxARTran>(cache, e, fsIntegrationEnabled, false);

            if (e.Row == null)
            {
                return;
            }
        }
        #endregion

        #region Invoicing Methods
        public virtual bool IsInvoiceProcessRunning { get; set; }

        public virtual List<ErrorInfo> GetErrorInfo()
        {
            List<ErrorInfo> errorList = new List<ErrorInfo>();
            ErrorInfo errorInfo;

            foreach (ARTran arTranRow in Base.Transactions.Select())
            {
                string errorMessage = SharedFunctions.CheckCacheState(Base.Transactions.Cache, arTranRow, true, true, true);

                if (string.IsNullOrEmpty(errorMessage) == false)
                {
                    FSxARTran fsxARTranRow = Base.Transactions.Cache.GetExtension<FSxARTran>(arTranRow);

                    errorInfo = new ErrorInfo()
                    {
                        SOID = fsxARTranRow.SOID,
                        AppointmentID = fsxARTranRow.AppointmentID,
                        ErrorMessage = errorMessage + Environment.NewLine
                    };

                    errorList.Add(errorInfo);
                }
            }

            return errorList;
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
            }
            else
            {
                arInvoiceRow.DocType = ARInvoiceType.CreditMemo;
            }

            arInvoiceRow.DocDate = invoiceDate;
            arInvoiceRow.FinPeriodID = invoiceFinPeriodID;
            arInvoiceRow.InvoiceNbr = fsServiceOrderRow.CustPORefNbr;
            arInvoiceRow = Base.Document.Insert(arInvoiceRow);
            initialHold = arInvoiceRow.Hold;
            arInvoiceRow.NoteID = null;
            PXNoteAttribute.GetNoteIDNow(Base.Document.Cache, arInvoiceRow);

            Base.Document.Cache.SetValueExt<ARInvoice.hold>(arInvoiceRow, true);
            Base.Document.Cache.SetValueExt<ARInvoice.customerID>(arInvoiceRow, fsServiceOrderRow.BillCustomerID);
            Base.Document.Cache.SetValueExt<ARInvoice.customerLocationID>(arInvoiceRow, fsServiceOrderRow.BillLocationID);
            Base.Document.Cache.SetValueExt<ARInvoice.curyID>(arInvoiceRow, fsServiceOrderRow.CuryID);

            Base.Document.Cache.SetValueExt<ARInvoice.taxZoneID>(arInvoiceRow, fsAppointmentRow != null ? fsAppointmentRow.TaxZoneID : fsServiceOrderRow.TaxZoneID);

            string termsID = InvoicingFunctions.GetTermsIDFromCustomerOrVendor(graphProcess, fsServiceOrderRow.BillCustomerID, null);
            if (termsID != null)
            {
                Base.Document.Cache.SetValueExt<ARInvoice.termsID>(arInvoiceRow, termsID);
            }
            else
            {
                Base.Document.Cache.SetValueExt<ARInvoice.termsID>(arInvoiceRow, fsSrvOrdTypeRow.DfltTermIDARSO);
            }

            if (fsServiceOrderRow.ProjectID != null)
            {
                Base.Document.Cache.SetValueExt<ARInvoice.projectID>(arInvoiceRow, fsServiceOrderRow.ProjectID);
            }

            Base.Document.Cache.SetValueExt<ARInvoice.docDesc>(arInvoiceRow, fsServiceOrderRow.DocDesc);
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
            List<SharedClasses.SOARLineEquipmentComponent> componentList = new List<SharedClasses.SOARLineEquipmentComponent>();
            int? acctID;

            foreach (DocLineExt docLineExt in docLines)
            {
                docLine = docLineExt.docLine;
                FSSODetSplit fsSODetSplitRow = docLineExt.fsSODetSplit;
                fsPostDocRow = docLineExt.fsPostDoc;
                fsServiceOrderRow = docLineExt.fsServiceOrder;
                fsSrvOrdTypeRow = docLineExt.fsSrvOrdType;
                fsAppointmentRow = docLineExt.fsAppointment;

                arTranRow = new ARTran();
                arTranRow = Base.Transactions.Insert(arTranRow);

                Base.Transactions.Cache.SetValueExt<ARTran.branchID>(arTranRow, docLine.BranchID);
                Base.Transactions.Cache.SetValueExt<ARTran.inventoryID>(arTranRow, docLine.InventoryID);

                if (docLine.ProjectID != null && docLine.ProjectTaskID != null)
                {
                    PMTask pmTaskRow = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(graphProcess, docLine.ProjectTaskID);
                    Base.Transactions.Cache.SetValueExt<ARTran.taskID>(arTranRow, pmTaskRow.TaskCD);
                }

                Base.Transactions.Cache.SetValueExt<ARTran.uOM>(arTranRow, fsSODetSplitRow.SplitLineNbr > 0 ? fsSODetSplitRow.UOM : docLine.UOM);
                Base.Transactions.Cache.SetValueExt<ARTran.siteID>(arTranRow, fsSODetSplitRow.SplitLineNbr > 0 ? fsSODetSplitRow.SiteID : docLine.SiteID);
                Base.Transactions.Cache.SetValueExt<ARTran.locationID>(arTranRow, fsSODetSplitRow.SplitLineNbr > 0 ? fsSODetSplitRow.LocationID : docLine.SiteLocationID);

                if (docLine.IsService == true || fsAppointmentRow != null)
                {
                    Base.Transactions.Cache.SetValueExt<ARTran.qty>(arTranRow, docLine.GetQty(FieldType.BillableField));
                }
                else
                {
                    Base.Transactions.Cache.SetValueExt<ARTran.qty>(arTranRow, fsSODetSplitRow.SplitLineNbr > 0 ? fsSODetSplitRow.Qty : docLine.GetQty(FieldType.BillableField));
                }

                Base.Transactions.Cache.SetValueExt<ARTran.tranDesc>(arTranRow, docLine.TranDesc);

                fsPostDocRow.DocLineRef = arTranRow = Base.Transactions.Update(arTranRow);

                Base.Transactions.Cache.SetValueExt<ARTran.salesPersonID>(arTranRow, fsAppointmentRow == null ? fsServiceOrderRow.SalesPersonID : fsAppointmentRow.SalesPersonID);

                if (docLine.AcctID != null)
                {
                    acctID = docLine.AcctID;
                }
                else
                {
                    acctID = (int?)ServiceOrderCore.Get_TranAcctID_DefaultValue(
                        graphProcess,
                        fsSrvOrdTypeRow.SalesAcctSource,
                        docLine.InventoryID,
                        fsServiceOrderRow);
                }

                Base.Transactions.Cache.SetValueExt<ARTran.accountID>(arTranRow, acctID);

                if (docLine.SubID != null)
                {
                    try
                    {
                        Base.Transactions.Cache.SetValueExt<ARTran.subID>(arTranRow, docLine.SubID);
                    }
                    catch (PXException)
                    {
                        arTranRow.SubID = null;
                    }
                }
                else
                {
                    InvoicingFunctions.SetCombinedSubID(
                                        graphProcess,
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

                Base.Transactions.Cache.SetValueExt<ARTran.manualPrice>(arTranRow, docLine.ManualPrice);
                Base.Transactions.Cache.SetValueExt<ARTran.curyUnitPrice>(arTranRow, docLine.CuryUnitPrice * invtMult);

                Base.Transactions.Cache.SetValueExt<ARTran.taxCategoryID>(arTranRow, docLine.TaxCategoryID);
                //Base.Transactions.Cache.SetValueExt<ARTran.curyExtPrice>(arTranRow, docLine.GetTranAmt(FieldType.BillableField) * invtMult);
                Base.Transactions.Cache.SetValueExt<ARTran.commissionable>(arTranRow, fsAppointmentRow?.Commissionable ?? fsServiceOrderRow.Commissionable ?? false);

                Base.Transactions.Cache.SetValueExt<ARTran.costCodeID>(arTranRow, docLine.CostCodeID);

                if (docLine.IsBillable == false)
                {
                    Base.Transactions.Cache.SetValueExt<ARTran.manualDisc>(arTranRow, true);
                    Base.Transactions.Cache.SetValueExt<ARTran.curyDiscAmt>(arTranRow, docLine.GetQty(FieldType.BillableField) * docLine.CuryUnitPrice * invtMult);
                }

                fsxARTranRow = Base.Transactions.Cache.GetExtension<FSxARTran>(arTranRow);
                fsxARTranRow.Source = docLine.BillingBy;
                fsxARTranRow.SOID = fsServiceOrderRow.SOID;
                fsxARTranRow.ServiceOrderDate = fsServiceOrderRow.OrderDate;
                fsxARTranRow.BillCustomerID = fsServiceOrderRow.BillCustomerID;
                fsxARTranRow.CustomerLocationID = fsServiceOrderRow.BillLocationID;
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
                    Base.Transactions.Cache.SetValueExt<ARTran.lotSerialNbr>(arTranRow, docLine.LotSerialNbr);
                }
                else if (fsSODetSplitRow.SplitLineNbr > 0 && !string.IsNullOrEmpty(fsSODetSplitRow.LotSerialNbr))
                {
                    Base.Transactions.Cache.SetValueExt<ARTran.lotSerialNbr>(arTranRow, fsSODetSplitRow.LotSerialNbr);
                }

                if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
                {
                    if (docLine.EquipmentAction != null)
                    {
                        Base.Transactions.SetValueExt<FSxARTran.equipmentAction>(arTranRow, docLine.EquipmentAction);
                        Base.Transactions.SetValueExt<FSxARTran.sMEquipmentID>(arTranRow, docLine.SMEquipmentID);
                        Base.Transactions.SetValueExt<FSxARTran.equipmentLineRef>(arTranRow, docLine.EquipmentLineRef);

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
                Base.Document.Cache.SetValueExt<ARInvoice.curyOrigDocAmt>(arInvoiceRow, arInvoiceRow.CuryDocBal);
            }

            if (initialHold != true)
            {
                Base.Document.Cache.SetValueExt<ARInvoice.hold>(arInvoiceRow, false);
            }

            arInvoiceRow = Base.Document.Update(arInvoiceRow);
        }

        public virtual FSCreatedDoc PressSave(int batchID, BeforeSaveDelegate beforeSave)
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
                    Where<FSPostInfo.sOInvRefNbr, Equal<Required<FSPostInfo.sOInvRefNbr>>,
                      And<FSPostInfo.sOInvDocType, Equal<Required<FSPostInfo.sOInvDocType>>,
                      And<FSPostInfo.sOInvLineNbr, Equal<Required<FSPostInfo.sOInvLineNbr>>,
                      And<FSPostInfo.sOInvPosted, Equal<True>>>>>>
            .Select(cleanerGraph, refNbr, docType, lineNbr).Count() > 0;
        }
        #endregion
    }
}
