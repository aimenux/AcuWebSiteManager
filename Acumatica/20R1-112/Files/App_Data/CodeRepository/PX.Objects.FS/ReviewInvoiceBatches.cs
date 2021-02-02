using PX.Data;

namespace PX.Objects.FS
{
    public class ReviewInvoiceBatches : PXGraph<ReviewInvoiceBatches>
    {
        #region Select
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        public PXCancel<FSPostBatch> Cancel;

        [PXFilterable]
        public PXProcessing<FSPostBatch,
               Where<
                   FSPostBatch.status, Equal<FSPostBatch.status.temporary>>,
               OrderBy<
                   Asc<FSPostBatch.createdDateTime>>> Batches;
        #endregion

        public ReviewInvoiceBatches()
        {
            Batches.SetProcessDelegate<PostBatchEntry>(
                delegate(PostBatchEntry processor, FSPostBatch fsPostBatchRow)
                {
                    processor.Clear();
                    processor.DeletePostingBatch(fsPostBatchRow);
                });

            Batches.SetProcessCaption(TX.ButtonDisplays.DeleteProc);
            Batches.SetProcessAllCaption(TX.ButtonDisplays.DeleteAllProc);
        }
    }
}
