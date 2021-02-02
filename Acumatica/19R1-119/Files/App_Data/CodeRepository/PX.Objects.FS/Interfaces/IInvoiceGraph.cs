using PX.Data;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public interface IInvoiceGraph
    {
        void CreateInvoice(PXGraph graphProcess, List<DocLineExt> docLines, List<DocLineExt> docLinesGrouped, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID, OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted,  OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow);

        FSCreatedDoc PressSave(int batchID, BeforeSaveDelegate beforeSave);

        void Clear();

        PXGraph GetGraph();

        void DeleteDocument(FSCreatedDoc fsCreatedDocRow);

        void CleanPostInfo(PXGraph cleanerGraph, FSPostDet fsPostDetRow);

        List<ErrorInfo> GetErrorInfo();

        bool IsInvoiceProcessRunning { get; set; }
    }
}
