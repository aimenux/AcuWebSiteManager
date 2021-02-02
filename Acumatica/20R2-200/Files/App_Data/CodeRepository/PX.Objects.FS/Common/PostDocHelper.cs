using PX.Data;
using PX.Objects.SO;
using System;

namespace PX.Objects.FS
{
    public static class DocGenerationHelper
    {
        public static void ValidatePostBatchStatus<TDocument>(PXGraph graph, PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
            where TDocument : class, IBqlTable, new()
        {
            if (dbOperation == PXDBOperation.Update
                && graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.INVOICE_BY_APPOINTMENT)
                && graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.INVOICE_BY_SERVICE_ORDER)
            )
            {
                FSCreatedDoc fsCreatedDocRow = PXSelectJoin<FSCreatedDoc,
                                                InnerJoin<FSPostBatch, On<FSCreatedDoc.batchID, Equal<FSPostBatch.batchID>>>,
                                                Where<
                                                    FSPostBatch.status, Equal<FSPostBatch.status.temporary>,
                                                    And<FSPostBatch.postTo, Equal<Required<FSPostBatch.postTo>>,
                                                    And<FSCreatedDoc.createdDocType, Equal<Required<FSCreatedDoc.createdDocType>>,
                                                    And<FSCreatedDoc.createdRefNbr, Equal<Required<FSCreatedDoc.createdRefNbr>>>>>>>
                                             .Select(graph, postTo, createdDocType, createdRefNbr);

                if (fsCreatedDocRow != null)
                {
                    PXProcessing<TDocument>.SetError(TX.Error.CANNOT_UPDATE_DOCUMENT_BECAUSE_BATCH_STATUS_IS_TEMPORARY);
                    throw new PXException(TX.Error.CANNOT_UPDATE_DOCUMENT_BECAUSE_BATCH_STATUS_IS_TEMPORARY);
                }
            }
        }

        [Obsolete("Remove in major version")]
        public static void ValidatePostBatchStatus(PXGraph graph, PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            if (dbOperation == PXDBOperation.Update
                && graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.INVOICE_BY_APPOINTMENT)
                && graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.INVOICE_BY_SERVICE_ORDER)
            )
            {
                FSCreatedDoc fsCreatedDocRow = PXSelectJoin<FSCreatedDoc,
                                                InnerJoin<FSPostBatch, On<FSCreatedDoc.batchID, Equal<FSPostBatch.batchID>>>,
                                                Where<
                                                    FSPostBatch.status, Equal<FSPostBatch.status.temporary>,
                                                    And<FSCreatedDoc.postTo, Equal<Required<FSCreatedDoc.postTo>>,
                                                    And<FSCreatedDoc.createdDocType, Equal<Required<FSCreatedDoc.createdDocType>>,
                                                    And<FSCreatedDoc.createdRefNbr, Equal<Required<FSCreatedDoc.createdRefNbr>>>>>>>
                                             .Select(graph, postTo, createdDocType, createdRefNbr);

                if (fsCreatedDocRow != null)
                {
                    throw new PXException(TX.Error.CANNOT_UPDATE_DOCUMENT_BECAUSE_BATCH_STATUS_IS_TEMPORARY);
                }
            }
        }
    }
}
