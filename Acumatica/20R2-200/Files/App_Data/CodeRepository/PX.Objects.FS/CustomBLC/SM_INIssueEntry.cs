using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using static PX.Objects.FS.MessageHelper;

namespace PX.Objects.FS
{
    public class SM_INIssueEntry : PXGraphExtension<INIssueEntry>, IInvoiceGraph
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Event Handlers
        protected virtual void _(Events.RowPersisting<INRegister> e)
        {
            if (e.Row == null || SharedFunctions.isFSSetupSet(Base) == false)
            {
                return;
            }

            ValidatePostBatchStatus(e.Operation, ID.Batch_PostTo.IN, e.Row.DocType, e.Row.RefNbr);
        }
        #endregion

        #region Invoicing Methods
        public virtual bool IsInvoiceProcessRunning { get; set; }

        public virtual List<ErrorInfo> GetErrorInfo()
        {
            return MessageHelper.GetErrorInfo<INTran, FSxINTran>(Base.issue.Cache, Base.issue.Current, Base.transactions);
        }
        public virtual string GetLineDisplayHint(PXGraph graph, string lineRefNbr, string lineDescr, int? inventoryID)
        {
            return MessageHelper.GetLineDisplayHint(graph, lineRefNbr, lineDescr, inventoryID);
        }

        public virtual void CreateInvoice(PXGraph graphProcess, List<DocLineExt> docLines, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID, OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted, OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow)
        {
            if (docLines.Count == 0)
            {
                return;
            }

            bool? initialHold = false;

            FSServiceOrder fsServiceOrderRow = docLines[0].fsServiceOrder;
            FSSrvOrdType fsSrvOrdTypeRow = docLines[0].fsSrvOrdType;
            FSPostDoc fsPostDocRow = docLines[0].fsPostDoc;
            FSAppointment fsAppointmentRow = docLines[0].fsAppointment;

            Base.FieldDefaulting.AddHandler<INRegister.branchID>((sender, e) =>
            {
                e.NewValue = fsServiceOrderRow.BranchID;
                e.Cancel = true;
            });

            INRegister inRegisterRow = new INRegister();

            inRegisterRow.DocType = INDocType.Issue;
            AutoNumberHelper.CheckAutoNumbering(Base, Base.insetup.SelectSingle().IssueNumberingID);

            inRegisterRow.TranDate = invoiceDate;
            inRegisterRow.FinPeriodID = invoiceFinPeriodID;
            inRegisterRow.TranDesc = fsAppointmentRow != null ? fsAppointmentRow.DocDesc : fsServiceOrderRow.DocDesc;

            inRegisterRow = PXCache<INRegister>.CreateCopy(Base.issue.Insert(inRegisterRow));

            initialHold = inRegisterRow.Hold;
            inRegisterRow.NoteID = null;
            PXNoteAttribute.GetNoteIDNow(Base.issue.Cache, inRegisterRow);

            Base.issue.Cache.SetValueExtIfDifferent<INRegister.hold>(inRegisterRow, true);

            inRegisterRow = Base.issue.Update(inRegisterRow);

            if (onDocumentHeaderInserted != null)
            {
                onDocumentHeaderInserted(Base, inRegisterRow);
            }

            IDocLine docLine = null;
            FSSODet soDet = null;
            FSAppointmentDet appointmentDet = null;
            INTran inTranRow = null;
            FSxINTran fsxINTranRow = null;
            PMTask pmTaskRow = null;
            List<DocLineExt> stockDocLines = docLines.Where(x => x.docLine.LineType == ID.LineType_ALL.INVENTORY_ITEM && x.docLine.InventoryID != null).ToList();

            foreach(DocLineExt line in stockDocLines)
            {
                docLine = line.docLine;
                soDet = line.fsSODet;
                appointmentDet = line.fsAppointmentDet;
                fsPostDocRow = line.fsPostDoc;
                fsServiceOrderRow = line.fsServiceOrder;
                fsSrvOrdTypeRow = line.fsSrvOrdType;
                fsAppointmentRow = line.fsAppointment;
                pmTaskRow = line.pmTask;

                inTranRow = new INTran();

                inTranRow.BranchID = docLine.BranchID;
                inTranRow.TranType = INTranType.Issue;

                inTranRow = PXCache<INTran>.CreateCopy(Base.transactions.Insert(inTranRow));

                inTranRow.InventoryID = docLine.InventoryID;
                inTranRow.UOM = docLine.UOM;

                pmTaskRow = line.pmTask;

                if (pmTaskRow != null && pmTaskRow.Status == ProjectTaskStatus.Completed)
                {
                    throw new PXException(TX.Error.POSTING_PMTASK_ALREADY_COMPLETED, fsServiceOrderRow.RefNbr, GetLineDisplayHint(Base, docLine.LineRef, docLine.TranDesc, docLine.InventoryID), pmTaskRow.TaskCD);
                }

                if (docLine.ProjectID != null && docLine.ProjectTaskID != null)
                {
                    inTranRow.ProjectID = docLine.ProjectID;
                    inTranRow.TaskID = docLine.ProjectTaskID;
                }

                inTranRow.SiteID = docLine.SiteID;
                inTranRow.LocationID = docLine.SiteLocationID;
                inTranRow.TranDesc = docLine.TranDesc;
                inTranRow.CostCodeID = docLine.CostCodeID;
                inTranRow.ReasonCode = fsSrvOrdTypeRow.ReasonCode;

                inTranRow = PXCache<INTran>.CreateCopy(Base.transactions.Update(inTranRow));

                bool isLotSerialRequired = SharedFunctions.IsLotSerialRequired(Base.transactions.Cache, inTranRow.InventoryID);
                bool validateLotSerQty = false;

                if (isLotSerialRequired == true)
                {
                    INTranSplit currentSplit = Base.splits.Select();
                    if (currentSplit != null)
                    {
                        Base.splits.Delete(currentSplit);
                    }
                }

                if (isLotSerialRequired == true && fsAppointmentRow == null) // is posted by Service Order
                {
                    foreach (FSSODetSplit split in PXSelect<FSSODetSplit,
                                                   Where<
                                                        FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                                        And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                                        And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>>>>,
                                                   OrderBy<Asc<FSSODetSplit.splitLineNbr>>>
                                                   .Select(Base, soDet.SrvOrdType, soDet.RefNbr, soDet.LineNbr))
                    {
                        if (split.Completed == false && string.IsNullOrEmpty(split.LotSerialNbr) == false)
                        {
                            INTranSplit newSplit = new INTranSplit();
                            newSplit = (INTranSplit)Base.splits.Cache.CreateCopy(Base.splits.Insert(newSplit));

                            newSplit.SiteID = split.SiteID != null ? split.SiteID : newSplit.SiteID;
                            newSplit.LocationID = split.LocationID != null ? split.LocationID : newSplit.LocationID;
                            newSplit.LotSerialNbr = split.LotSerialNbr;
                            newSplit.Qty = split.Qty;

                            newSplit = Base.splits.Update(newSplit);
                            validateLotSerQty = true;
                        }
                    }

                    inTranRow = (INTran)Base.transactions.Cache.CreateCopy(Base.transactions.Current);
                }
                else
                {
                    foreach (FSApptLineSplit split in PXSelect<FSApptLineSplit,
                                                       Where<
                                                            FSApptLineSplit.srvOrdType, Equal<Required<FSApptLineSplit.srvOrdType>>,
                                                            And<FSApptLineSplit.apptNbr, Equal<Required<FSApptLineSplit.apptNbr>>,
                                                            And<FSApptLineSplit.lineNbr, Equal<Required<FSApptLineSplit.lineNbr>>>>>,
                                                       OrderBy<Asc<FSApptLineSplit.splitLineNbr>>>
                                                       .Select(Base, appointmentDet.SrvOrdType, appointmentDet.RefNbr, appointmentDet.LineNbr))
                    {
                        if (string.IsNullOrEmpty(split.LotSerialNbr) == false)
                        {
                            INTranSplit newSplit = new INTranSplit();
                            newSplit = (INTranSplit)Base.splits.Cache.CreateCopy(Base.splits.Insert(newSplit));

                            newSplit.SiteID = split.SiteID != null ? split.SiteID : newSplit.SiteID;
                            newSplit.LocationID = split.LocationID != null ? split.LocationID : newSplit.LocationID;
                            newSplit.LotSerialNbr = split.LotSerialNbr;
                            newSplit.Qty = split.Qty;

                            newSplit = Base.splits.Update(newSplit);
                            validateLotSerQty = true;
                        }
                    }

                    inTranRow = (INTran)Base.transactions.Cache.CreateCopy(Base.transactions.Current);
                }

                if (validateLotSerQty == false)
                {
                    inTranRow.Qty = docLine.GetQty(FieldType.BillableField);
                }
                else if (inTranRow.Qty != docLine.GetQty(FieldType.BillableField))
                {
                    throw new PXException(TX.Error.QTY_POSTED_ERROR);
                }

                inTranRow.UnitPrice = (docLine.IsFree == false ? docLine.CuryUnitPrice * invtMult : 0m);
                inTranRow.TranAmt = docLine.GetTranAmt(FieldType.BillableField) * invtMult;

                fsxINTranRow = Base.transactions.Cache.GetExtension<FSxINTran>(inTranRow);
                fsxINTranRow.Source = docLine.BillingBy;
                fsxINTranRow.SOID = fsServiceOrderRow.SOID;
                fsxINTranRow.BillCustomerID = fsServiceOrderRow.CustomerID;
                fsxINTranRow.CustomerLocationID = fsServiceOrderRow.LocationID;
                fsxINTranRow.SODetID = docLine.PostSODetID;
                fsxINTranRow.AppointmentID = docLine.PostAppointmentID;
                fsxINTranRow.AppointmentDate = fsAppointmentRow?.ExecutionDate;
                fsxINTranRow.AppDetID = docLine.PostAppDetID;

                SharedFunctions.CopyNotesAndFiles(Base.transactions.Cache, inTranRow, docLine, fsSrvOrdTypeRow);

                fsPostDocRow.INDocLineRef = inTranRow = Base.transactions.Update(inTranRow);

                if (onTransactionInserted != null)
                {
                    onTransactionInserted(Base, inTranRow);
                }
            }

            inRegisterRow = Base.issue.Update(inRegisterRow);

            if (Base.insetup.Current?.RequireControlTotal == true)
            {
                Base.issue.Cache.SetValueExtIfDifferent<INRegister.controlQty>(inRegisterRow, inRegisterRow.TotalQty);
                Base.issue.Cache.SetValueExtIfDifferent<INRegister.controlAmount>(inRegisterRow, inRegisterRow.TotalAmount);
            }

            if (initialHold != true)
            {
                Base.issue.Cache.SetValueExtIfDifferent<INRegister.hold>(inRegisterRow, false);
            }

            inRegisterRow = Base.issue.Update(inRegisterRow);
        }

        public virtual FSCreatedDoc PressSave(int batchID, List<DocLineExt> docLines, BeforeSaveDelegate beforeSave)
        {
            if (Base.issue.Current == null)
            {
                throw new SharedClasses.TransactionScopeException();
            }

            if (beforeSave != null)
            {
                beforeSave(Base);
            }

            Base.Save.Press();

            INRegister inRegisterRow = Base.issue.Current != null ? Base.issue.Current : Base.issue.Select();

            if (docLines != null && docLines.Count > 0)
            {
                FSSrvOrdType fsSrvOrdTypeRow = docLines[0].fsSrvOrdType;

                if (fsSrvOrdTypeRow != null && fsSrvOrdTypeRow.ReleaseIssueOnInvoice == true)
                {
                    if (inRegisterRow.Hold == true)
                    {
                        Base.issue.Cache.SetValueExtIfDifferent<INRegister.hold>(inRegisterRow, false);

                        inRegisterRow = Base.issue.Update(inRegisterRow);
                    }

                    Base.release.Press();
                }
            }

            string docType = Base.issue.Current.DocType;
            string refNbr = Base.issue.Current.RefNbr;

            var fsCreatedDocRow = new FSCreatedDoc()
            {
                BatchID = batchID,
                PostTo = ID.Batch_PostTo.IN,
                CreatedDocType = inRegisterRow.DocType,
                CreatedRefNbr = inRegisterRow.RefNbr
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
            Base.issue.Current = Base.issue.Search<INRegister.refNbr>(fsCreatedDocRow.CreatedRefNbr);

            if (Base.issue.Current != null)
            {
                if (Base.issue.Current.RefNbr == fsCreatedDocRow.CreatedRefNbr)
                {
                    Base.Delete.Press();
                }
            }
        }

        public virtual void CleanPostInfo(PXGraph cleanerGraph, FSPostDet fsPostDetRow)
        {
            PXUpdate<
                Set<FSPostInfo.iNLineNbr, Null,
                Set<FSPostInfo.iNRefNbr, Null,
                Set<FSPostInfo.iNDocType, Null,
                Set<FSPostInfo.iNPosted, False>>>>,
            FSPostInfo,
            Where<
                FSPostInfo.postID, Equal<Required<FSPostInfo.postID>>,
                And<FSPostInfo.iNPosted, Equal<True>>>>
            .Update(cleanerGraph, fsPostDetRow.PostID);
        }

        public virtual void UpdateCostAndPrice(List<DocLineExt> docLines)
        {
        }
        #endregion

        #region Validations
        public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion
    }
}