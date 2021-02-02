using PX.Data;
using System;
using System.Collections.Generic;
using static PX.Objects.FS.MessageHelper;

namespace PX.Objects.FS
{
    public interface IInvoiceGraph
    {
        void CreateInvoice(PXGraph graphProcess, List<DocLineExt> docLines, List<DocLineExt> docLinesGrouped, short invtMult, DateTime? invoiceDate, string invoiceFinPeriodID, OnDocumentHeaderInsertedDelegate onDocumentHeaderInserted,  OnTransactionInsertedDelegate onTransactionInserted, PXQuickProcess.ActionFlow quickProcessFlow);

        FSCreatedDoc PressSave(int batchID, List<DocLineExt> docLines, BeforeSaveDelegate beforeSave);

        void Clear();

        PXGraph GetGraph();

        void DeleteDocument(FSCreatedDoc fsCreatedDocRow);

        void CleanPostInfo(PXGraph cleanerGraph, FSPostDet fsPostDetRow);

        void UpdateCostAndPrice(List<DocLineExt> docLines);

        List<ErrorInfo> GetErrorInfo();

        bool IsInvoiceProcessRunning { get; set; }
    }
}
