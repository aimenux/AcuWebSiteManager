using PX.Data;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public delegate void OnDocumentHeaderInsertedDelegate(PXGraph graph, IBqlTable row);

    public delegate void OnTransactionInsertedDelegate(PXGraph graph, IBqlTable row);

    public delegate void BeforeSaveDelegate(PXGraph graph);

    public delegate void AfterCreateInvoiceDelegate(PXGraph graph, FSCreatedDoc fsCreatedDocRow);

    public interface IInvoiceProcessGraph
    {
        OnDocumentHeaderInsertedDelegate OnDocumentHeaderInserted { get; set; }

        OnTransactionInsertedDelegate OnTransactionInserted { get; set; }

        BeforeSaveDelegate BeforeSave { get; set; }

        AfterCreateInvoiceDelegate AfterCreateInvoice { get; set; }

        void Clear(PXClearOption option);

        PXGraph GetGraph();

        List<DocLineExt> GetInvoiceLines(Guid currentProcessID, int billingCycleID, string groupKey, bool getOnlyTotal, out decimal? invoiceTotal, string postTo);

        void UpdateSourcePostDoc(PXCache<FSPostDet> cacheFSPostDet, FSPostBatch fsPostBatchRow, FSPostDoc fsPostDocRow);
    }
}
