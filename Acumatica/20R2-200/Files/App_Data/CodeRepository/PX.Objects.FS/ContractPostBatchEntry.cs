using PX.Data;
using System;

namespace PX.Objects.FS
{
    public class ContractPostBatchEntry : PXGraph<ContractPostBatchEntry, FSContractPostBatch>
    {
        public PXSelect<FSContractPostBatch> PostBatchRecords;

        protected virtual FSContractPostBatch InitFSPostBatch(DateTime? invoiceDate, string postTo, DateTime? upToDate, string invoicePeriodID)
        {
            FSContractPostBatch fsPostBatchRow = new FSContractPostBatch();

            fsPostBatchRow.InvoiceDate = new DateTime(invoiceDate.Value.Year, invoiceDate.Value.Month, invoiceDate.Value.Day, 0, 0, 0);
            fsPostBatchRow.PostTo = postTo;
            fsPostBatchRow.UpToDate = upToDate.HasValue == true ? new DateTime(upToDate.Value.Year, upToDate.Value.Month, upToDate.Value.Day, 0, 0, 0) : upToDate;
            fsPostBatchRow.FinPeriodID = invoicePeriodID;

            return fsPostBatchRow;
        }

        public virtual FSContractPostBatch CreatePostingBatch(DateTime? upToDate, DateTime? invoiceDate, string invoiceFinPeriodID, string postTo)
        {
            FSContractPostBatch fsPostBatchRow = InitFSPostBatch(invoiceDate, postTo, upToDate, invoiceFinPeriodID);

            PostBatchRecords.Current = PostBatchRecords.Insert(fsPostBatchRow);
            Save.Press();

            return PostBatchRecords.Current;
        }

        public virtual void DeletePostingBatch(FSContractPostBatch fsPostBatchRow)
        {
            if (fsPostBatchRow.ContractPostBatchID < 0)
            {
                return;
            }

            PostBatchRecords.Current = PostBatchRecords.Search<FSContractPostBatch.contractPostBatchID>(fsPostBatchRow.ContractPostBatchID);

            if (PostBatchRecords.Current == null || PostBatchRecords.Current.ContractPostBatchID != fsPostBatchRow.ContractPostBatchID)
            {
                return;
            }

            using (var ts = new PXTransactionScope())
            {
                PXDatabase.Delete<FSCreatedDoc>(
                        new PXDataFieldRestrict<FSCreatedDoc.batchID>(fsPostBatchRow.ContractPostBatchID));

                PostBatchRecords.Delete(fsPostBatchRow);
                Save.Press();

                ts.Complete();
            }
        }
    }
}