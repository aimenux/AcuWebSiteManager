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

        public virtual void CreateInvoice(PXGraph graphProcess, List<DocLineExt> docLines, List<DocLineExt> docLinesGrouped, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID, OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted, OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow)
        {
            if (docLinesGrouped.Count == 0)
            {
                return;
            }

            bool? initialHold = false;

            FSServiceOrder fsServiceOrderRow = docLinesGrouped[0].fsServiceOrder;
            FSSrvOrdType fsSrvOrdTypeRow = docLinesGrouped[0].fsSrvOrdType;
            FSPostDoc fsPostDocRow = docLinesGrouped[0].fsPostDoc;
            FSAppointment fsAppointmentRow = docLinesGrouped[0].fsAppointment;

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
            INTran inTranRow = null;
            FSxINTran fsxINTranRow = null;
            PMTask pmTaskRow = null;

            List<GroupDocLineExt> singleLines =
                    docLines.Where(x => x.docLine.LineType == ID.LineType_ALL.INVENTORY_ITEM).GroupBy(
                        x => new { x.docLine.DocID, x.docLine.LineID },
                        (key, group)
                        => new GroupDocLineExt(key.DocID, key.LineID, group.ToList())).ToList();

            foreach (GroupDocLineExt singleLine in singleLines)
            {
                DocLineExt docLineExt = singleLine.Group.First();

                docLine = docLineExt.docLine;
                fsPostDocRow = docLineExt.fsPostDoc;
                fsServiceOrderRow = docLineExt.fsServiceOrder;
                fsSrvOrdTypeRow = docLineExt.fsSrvOrdType;
                fsAppointmentRow = docLineExt.fsAppointment;

                inTranRow = new INTran();

                inTranRow.BranchID = docLine.BranchID;
                inTranRow.TranType = INTranType.Issue;

                inTranRow = PXCache<INTran>.CreateCopy(Base.transactions.Insert(inTranRow));

                inTranRow.InventoryID = docLine.InventoryID;
                inTranRow.UOM = docLine.UOM;

                pmTaskRow = docLineExt.pmTask;

                if (pmTaskRow != null && pmTaskRow.Status == ProjectTaskStatus.Completed)
                {
                    throw new PXException(TX.Error.POSTING_PMTASK_ALREADY_COMPLETED, fsServiceOrderRow.RefNbr, docLine.LineRef, pmTaskRow.TaskCD);
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

                INTranSplit currentSplit = Base.splits.Select();

                if (fsAppointmentRow == null)
                {
                    bool qtyAssigned = false;

                    if (currentSplit != null 
                        && singleLine.Group != null 
                        && singleLine.Group.Count > 0)
                    {
                        Base.splits.Delete(currentSplit);
                    }

                    foreach (DocLineExt splitLine in singleLine.Group)
                    {
                        if (splitLine.fsSODetSplit.SplitLineNbr != null && splitLine.fsSODetSplit.Completed == false)
                        {
                            INTranSplit split = new INTranSplit();
                            split = Base.splits.Insert(split);
                            INTranSplit copySplit = (INTranSplit)Base.splits.Cache.CreateCopy(split);

                            copySplit.SiteID = splitLine.fsSODetSplit.SiteID != null ? splitLine.fsSODetSplit.SiteID : copySplit.SiteID;
                            copySplit.LocationID = splitLine.fsSODetSplit.LocationID != null ? splitLine.fsSODetSplit.LocationID : copySplit.LocationID;
                            copySplit.LotSerialNbr = splitLine.fsSODetSplit.LotSerialNbr;
                            copySplit.Qty = splitLine.fsSODetSplit.Qty;

                            split = Base.splits.Update(copySplit);
                            qtyAssigned = true;
                        }
                    }

                    inTranRow = (INTran)Base.transactions.Cache.CreateCopy(Base.transactions.Current);

                    if (qtyAssigned == false)
                    {
                        inTranRow.Qty = docLine.GetQty(FieldType.BillableField);
                    }
                    else if (inTranRow.Qty != docLine.GetQty(FieldType.BillableField))
                    {
                        throw new PXException(TX.Error.QTY_POSTED_ERROR);
                    }
                }
                else
                {
                    bool qtyAssigned = false;
                    if (string.IsNullOrEmpty(docLine.LotSerialNbr) == false)
                    {
                        if (currentSplit != null) 
                        { 
                            Base.splits.Delete(currentSplit);
                        }

                        INTranSplit split = new INTranSplit();
                        split = Base.splits.Insert(split);
                        INTranSplit copySplit = (INTranSplit)Base.splits.Cache.CreateCopy(split);

                        copySplit.SiteID = docLine.SiteID;
                        copySplit.LocationID = docLine.SiteLocationID != null ? docLine.SiteLocationID : copySplit.LocationID;
                        copySplit.LotSerialNbr = docLine.LotSerialNbr != null ? docLine.LotSerialNbr : copySplit.LotSerialNbr;
                        copySplit.Qty = docLine.GetQty(FieldType.BillableField);

                        split = Base.splits.Update(copySplit);
                        qtyAssigned = true;
                    }

                    inTranRow = (INTran)Base.transactions.Cache.CreateCopy(Base.transactions.Current);

                    if (qtyAssigned == false)
                    {
                        inTranRow.Qty = docLine.GetQty(FieldType.BillableField);
                    }
                    else if (inTranRow.Qty != docLine.GetQty(FieldType.BillableField))
                    {
                        throw new PXException(TX.Error.QTY_POSTED_ERROR);
                    }
                }

                inTranRow.UnitPrice = docLine.CuryUnitPrice * invtMult;
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