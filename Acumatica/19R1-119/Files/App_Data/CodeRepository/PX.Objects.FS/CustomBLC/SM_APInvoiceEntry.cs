using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class SM_APInvoiceEntry : PXGraphExtension<APInvoiceEntry>, IInvoiceGraph
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public virtual bool IsFSIntegrationEnabled()
        {
            APInvoice apInvoiceRow = Base.Document.Current;

            if (apInvoiceRow.CreatedByScreenID.Substring(0, 2) == "FS")
            {
                return true;
            }

            return false;
        }

        #region Event Handlers
        #region APInvoice Event Handlers
        protected virtual void APInvoice_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null || SharedFunctions.isFSSetupSet(Base) == false)
            {
                return;
            }

            APInvoice apInvoiceRow = (APInvoice)e.Row;

            ValidatePostBatchStatus(e.Operation, ID.Batch_PostTo.AP, apInvoiceRow.DocType, apInvoiceRow.RefNbr);
        }
        protected virtual void APInvoice_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.Row == null || SharedFunctions.isFSSetupSet(Base) == false)
            {
                return;
            }

            APInvoice apInvoiceRow = (APInvoice)e.Row;
            if (e.Operation == PXDBOperation.Delete
                    && e.TranStatus == PXTranStatus.Open)
            {
                InvoicingFunctions.CleanPostingInfoLinkedToDoc(apInvoiceRow);
            }
        }
        #endregion
        #region APTran Event Handlers
        protected virtual void APTran_Qty_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || SharedFunctions.isFSSetupSet(Base) == false)
            {
                return;
            }

            APTran apTranRow = (APTran)e.Row;

            if (IsLineCreatedFromAppSO(Base, Base.Document.Current, apTranRow, typeof(APTran.qty).Name) == true)
            {
                throw new PXSetPropertyException(TX.Error.NO_UPDATE_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
            }
        }

        protected virtual void APTran_InventoryID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || SharedFunctions.isFSSetupSet(Base) == false)
            {
                return;
            }

            APTran apTranRow = (APTran)e.Row;

            if (IsLineCreatedFromAppSO(Base, Base.Document.Current, apTranRow, typeof(APTran.inventoryID).Name) == true)
            {
                throw new PXSetPropertyException(TX.Error.NO_UPDATE_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
            }
        }

        protected virtual void APTran_UOM_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || SharedFunctions.isFSSetupSet(Base) == false)
            {
                return;
            }

            APTran apTranRow = (APTran)e.Row;

            if (IsLineCreatedFromAppSO(Base, Base.Document.Current, apTranRow, typeof(APTran.uOM).Name) == true)
            {
                throw new PXSetPropertyException(TX.Error.NO_UPDATE_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
            }
        }

        protected virtual void APTran_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (e.Row == null || SharedFunctions.isFSSetupSet(Base) == false)
            {
                return;
            }

            APTran apTranRow = (APTran)e.Row;

            if (e.ExternalCall == true)
            {
                if (IsLineCreatedFromAppSO(Base, Base.Document.Current, apTranRow, null) == true)
                {
                    throw new PXException(TX.Error.NO_DELETION_ALLOWED_DOCLINE_LINKED_TO_APP_SO);
                }
            }
        }

        protected virtual void APTran_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            bool fsIntegrationEnabled = IsFSIntegrationEnabled();
            DACHelper.SetExtensionVisibleInvisible<FSxAPTran>(cache, e, fsIntegrationEnabled, false);

            if (e.Row == null)
            {
                return;
            }
        }
        
        protected virtual void APTran_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            APTran apTranRow = (APTran)e.Row;

            //The CheckCacheState needs to be run when the transaction is aborted to verify the status of the fields
            //for the current row in order to catch exceptions thrown by other modules to allow them be shown in the
            //Generate Invoice screen.
            if (e.TranStatus == PXTranStatus.Aborted && IsInvoiceProcessRunning == true)
            {
                SharedFunctions.CheckCacheState(cache, apTranRow);
            }
        }
        #endregion
        #endregion
        #region Invoicing Methods
        public virtual bool IsInvoiceProcessRunning { get; set; }

        public virtual List<ErrorInfo> GetErrorInfo()
        {
            List<ErrorInfo> errorList = new List<ErrorInfo>();
            ErrorInfo errorInfo;

            string headerErrorMessage = SharedFunctions.CheckCacheState(Base.Document.Cache, Base.Document.Current, true);
            bool headerHasError = string.IsNullOrEmpty(headerErrorMessage) == false;

            foreach (APTran apTranRow in Base.Transactions.Select())
            {
                string errorMessage = SharedFunctions.CheckCacheState(Base.Transactions.Cache, apTranRow, true, true, true);

                if (string.IsNullOrEmpty(errorMessage) == false || headerHasError)
                {
                    FSxAPTran fsxAPTRanRow = Base.Transactions.Cache.GetExtension<FSxAPTran>(apTranRow);

                    errorInfo = new ErrorInfo()
                    {
                        HeaderError = headerHasError,
                        SOID = fsxAPTRanRow.SOID,
                        AppointmentID = fsxAPTRanRow.AppointmentID,
                        ErrorMessage = (headerHasError ? headerErrorMessage : errorMessage) + Environment.NewLine
                    };

                    errorList.Add(errorInfo);
                    if (headerHasError)
                    {
                        break;
                    }
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

            FSServiceOrder fsServiceOrderRow = docLinesGrouped[0].fsServiceOrder;
            FSSrvOrdType fsSrvOrdTypeRow = docLinesGrouped[0].fsSrvOrdType;
            FSPostDoc fsPostDocRow = docLinesGrouped[0].fsPostDoc;
            FSAppointment fsAppointmentRow = docLinesGrouped[0].fsAppointment;

            Vendor vendorRow = SharedFunctions.GetVendorRow(graphProcess, fsServiceOrderRow.BillCustomerID);

            if (vendorRow == null)
            {
                throw new PXException(TX.Error.AP_POSTING_VENDOR_NOT_FOUND);
            }

            Base.FieldDefaulting.AddHandler<APInvoice.branchID>((sender, e) =>
            {
                e.NewValue = fsServiceOrderRow.BranchID;
                e.Cancel = true;
            });

            APInvoice apInvoiceRow = new APInvoice();

            if (invtMult >= 0)
            {
                apInvoiceRow.DocType = APDocType.DebitAdj;
            }
            else
            {
                apInvoiceRow.DocType = APDocType.Invoice;
            }

            apInvoiceRow.DocDate = invoiceDate;
            apInvoiceRow.FinPeriodID = invoiceFinPeriodID;
            apInvoiceRow = PXCache<APInvoice>.CreateCopy(Base.Document.Insert(apInvoiceRow));
            initialHold = apInvoiceRow.Hold;
            apInvoiceRow.NoteID = null;
            PXNoteAttribute.GetNoteIDNow(Base.Document.Cache, apInvoiceRow);
            apInvoiceRow.VendorID = fsServiceOrderRow.BillCustomerID;
            apInvoiceRow.VendorLocationID = fsServiceOrderRow.BillLocationID;
            apInvoiceRow.CuryID = fsServiceOrderRow.CuryID;
            apInvoiceRow.TaxZoneID = fsAppointmentRow != null ? fsAppointmentRow.TaxZoneID : fsServiceOrderRow.TaxZoneID;

            apInvoiceRow.SuppliedByVendorLocationID = fsServiceOrderRow.BillLocationID;

            string termsID = InvoicingFunctions.GetTermsIDFromCustomerOrVendor(graphProcess, null, fsServiceOrderRow.BillCustomerID);
            if (termsID == null)
            {
                termsID = fsSrvOrdTypeRow.DfltTermIDAP;
            }

            apInvoiceRow.FinPeriodID = invoiceFinPeriodID;
            apInvoiceRow.TermsID = termsID;
            apInvoiceRow.DocDesc = fsServiceOrderRow.DocDesc;
            apInvoiceRow.Hold = true;
            apInvoiceRow = Base.Document.Update(apInvoiceRow);
            apInvoiceRow.TaxCalcMode = PX.Objects.TX.TaxCalculationMode.TaxSetting;
            apInvoiceRow = Base.Document.Update(apInvoiceRow);

            InvoicingFunctions.SetContactAndAddress(Base, fsServiceOrderRow);

            if (onDocumentHeaderInserted != null)
            {
                onDocumentHeaderInserted(Base, apInvoiceRow);
            }

            IDocLine docLine = null;
            APTran apTranRow = null;
            FSxAPTran fsxAPTranRow = null;

            foreach (DocLineExt docLineExt in docLinesGrouped)
            {
                docLine = docLineExt.docLine;
                fsPostDocRow = docLineExt.fsPostDoc;
                fsServiceOrderRow = docLineExt.fsServiceOrder;
                fsSrvOrdTypeRow = docLineExt.fsSrvOrdType;
                fsAppointmentRow = docLineExt.fsAppointment;

                apTranRow = new APTran();
                apTranRow = Base.Transactions.Insert(apTranRow);

                Base.Transactions.Cache.SetValueExt<APTran.branchID>(apTranRow, docLine.BranchID);
                Base.Transactions.Cache.SetValueExt<APTran.inventoryID>(apTranRow, docLine.InventoryID);
                Base.Transactions.Cache.SetValueExt<APTran.uOM>(apTranRow, docLine.UOM);

                if (docLine.AcctID != null)
                {
                    Base.Transactions.Cache.SetValueExt<APTran.accountID>(apTranRow, docLine.AcctID);
                }

                if (docLine.SubID != null)
                {
                    try
                    {
                        Base.Transactions.Cache.SetValueExt<APTran.subID>(apTranRow, docLine.SubID);
                    }
                    catch (PXException)
                    {
                        apTranRow.SubID = null;
                    }
                }
                else
                {
                    InvoicingFunctions.SetCombinedSubID(
                                        graphProcess,
                                        Base.Transactions.Cache,
                                        null,
                                        apTranRow,
                                        null,
                                        fsSrvOrdTypeRow,
                                        apTranRow.BranchID,
                                        apTranRow.InventoryID,
                                        apInvoiceRow.VendorLocationID,
                                        fsServiceOrderRow.BranchLocationID,
                                        fsServiceOrderRow.SalesPersonID,
                                        docLine.IsService);
                }

                if (docLine.ProjectID != null && docLine.ProjectTaskID != null)
                {
                    PMTask pmTaskRow = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(graphProcess, docLine.ProjectTaskID);
                    Base.Transactions.Cache.SetValueExt<APTran.taskID>(apTranRow, pmTaskRow.TaskCD);
                }

                Base.Transactions.Cache.SetValueExt<APTran.qty>(apTranRow, docLine.GetQty(FieldType.BillableField));
                Base.Transactions.Cache.SetValueExt<APTran.tranDesc>(apTranRow, docLine.TranDesc);

                apTranRow = Base.Transactions.Update(apTranRow);
                Base.Transactions.Cache.SetValueExt<APTran.curyUnitCost>(apTranRow, docLine.CuryUnitPrice * invtMult);

                if (docLine.ProjectID != null)
                {
                    Base.Transactions.Cache.SetValueExt<APTran.projectID>(apTranRow, docLine.ProjectID);
                }

                Base.Transactions.Cache.SetValueExt<APTran.taxCategoryID>(apTranRow, docLine.TaxCategoryID);
                Base.Transactions.Cache.SetValueExt<APTran.costCodeID>(apTranRow, docLine.CostCodeID);

                if (docLine.IsBillable == false)
                {
                    Base.Transactions.Cache.SetValueExt<APTran.manualDisc>(apTranRow, true);
                    Base.Transactions.Cache.SetValueExt<APTran.curyDiscAmt>(apTranRow, docLine.GetQty(FieldType.BillableField) * docLine.CuryUnitPrice * invtMult);
                }

                fsxAPTranRow = Base.Transactions.Cache.GetExtension<FSxAPTran>(apTranRow);

                fsxAPTranRow.Source = docLine.BillingBy;
                fsxAPTranRow.SOID = fsServiceOrderRow.SOID;
                fsxAPTranRow.ServiceOrderDate = fsServiceOrderRow.OrderDate;
                fsxAPTranRow.BillCustomerID = fsServiceOrderRow.BillCustomerID;
                fsxAPTranRow.CustomerLocationID = fsServiceOrderRow.BillLocationID;
                fsxAPTranRow.SODetID = docLine.PostSODetID;
                fsxAPTranRow.AppointmentID = docLine.PostAppointmentID;
                fsxAPTranRow.AppointmentDate = fsAppointmentRow?.ExecutionDate;
                fsxAPTranRow.AppDetID = docLine.PostAppDetID;

                if (docLine.BillingBy == ID.Billing_By.APPOINTMENT)
                {
                    //// TODO
                    ////fsxAPTranRow.AppointmentDate = new DateTime(
                    ////                                fsAppointmentRow.ActualDateTimeBegin.Value.Year,
                    ////                                fsAppointmentRow.ActualDateTimeBegin.Value.Month,
                    ////                                fsAppointmentRow.ActualDateTimeBegin.Value.Day,
                    ////                                0,
                    ////                                0,
                    ////                                0);
                }

                fsxAPTranRow.Mem_PreviousPostID = docLine.PostID;
                fsxAPTranRow.Mem_TableSource = docLine.SourceTable;

                SharedFunctions.CopyNotesAndFiles(Base.Transactions.Cache, apTranRow, docLine, fsSrvOrdTypeRow);
                fsPostDocRow.DocLineRef = apTranRow = Base.Transactions.Update(apTranRow);

                if (onTransactionInserted != null)
                {
                    onTransactionInserted(Base, apTranRow);
                }
            }

            apInvoiceRow = Base.Document.Update(apInvoiceRow);

            if (Base.APSetup.Current.RequireControlTotal == true)
            {
                Base.Document.Cache.SetValueExt<APInvoice.curyOrigDocAmt>(apInvoiceRow, apInvoiceRow.CuryDocBal);
            }

            if (initialHold != true)
            {
                Base.Document.Cache.SetValueExt<APInvoice.hold>(apInvoiceRow, false);
            }

            apInvoiceRow = Base.Document.Update(apInvoiceRow);
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

            APInvoiceEntryExternalTax TaxGraphExt = Base.GetExtension<APInvoiceEntryExternalTax>();

            if (TaxGraphExt != null)
            {
                TaxGraphExt.SkipTaxCalcAndSave();
            }
            else
            {
            Base.Save.Press();
            }

            string docType = Base.Document.Current.DocType;
            string refNbr = Base.Document.Current.RefNbr;

            // Reload APInvoice to get the current value of IsTaxValid
            Base.Clear();
            Base.Document.Current = PXSelect<APInvoice, Where<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>.Select(Base, docType, refNbr);
            APInvoice apInvoiceRow = Base.Document.Current;

            var fsCreatedDocRow = new FSCreatedDoc()
            {
                BatchID = batchID,
                PostTo = ID.Batch_PostTo.AP,
                CreatedDocType = apInvoiceRow.DocType,
                CreatedRefNbr = apInvoiceRow.RefNbr
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
            Base.Document.Current = Base.Document.Search<APInvoice.refNbr>(fsCreatedDocRow.CreatedRefNbr, fsCreatedDocRow.CreatedDocType);

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
                Set<FSPostInfo.aPLineNbr, Null,
                Set<FSPostInfo.apRefNbr, Null,
                Set<FSPostInfo.apDocType, Null,
                Set<FSPostInfo.aPPosted, False>>>>,
            FSPostInfo,
            Where<
                FSPostInfo.postID, Equal<Required<FSPostInfo.postID>>,
                And<FSPostInfo.aPPosted, Equal<True>>>>
            .Update(cleanerGraph, fsPostDetRow.PostID);
        }

        public virtual bool IsLineCreatedFromAppSO(PXGraph cleanerGraph, object document, object lineDoc, string fieldName)
        {
            if (document == null || lineDoc == null)
            {
                return false;
            }

            string refNbr = ((APInvoice)document).RefNbr;
            string docType = ((APInvoice)document).DocType;
            int? lineNbr = ((APTran)lineDoc).LineNbr;

            return PXSelect<FSPostInfo,
                    Where<FSPostInfo.apRefNbr, Equal<Required<FSPostInfo.apRefNbr>>,
                          And<FSPostInfo.apDocType, Equal<Required<FSPostInfo.apDocType>>,
                          And<FSPostInfo.aPLineNbr, Equal<Required<FSPostInfo.aPLineNbr>>,
                          And<FSPostInfo.aPPosted, Equal<True>>>>>>
            .Select(cleanerGraph, refNbr, docType, lineNbr).Count() > 0;
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
