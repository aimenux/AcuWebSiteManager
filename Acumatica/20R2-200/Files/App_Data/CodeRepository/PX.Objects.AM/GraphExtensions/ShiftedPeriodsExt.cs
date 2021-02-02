using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;

namespace PX.Objects.AM.GraphExtensions
{
    public class AMBatchShiftedPeriodsExt : AMShiftedPeriodsExt<AMBatchEntryBase, AMBatch, AMBatch.tranDate, AMBatch.tranPeriodID, AMMTran>
    {
        public override void Initialize()
        {
            base.Initialize();

            Documents = new PXSelectExtension<Document>(Base.AMBatchDataMember);
            Lines = new PXSelectExtension<DocumentLine>(Base.AMMTranDataMember);
        }
    }

    public class AMBatchSimpleShiftedPeriodsExt : AMShiftedPeriodsExt<AMBatchSimpleEntryBase, AMBatch, AMBatch.tranDate, AMBatch.tranPeriodID, AMMTran>
    {
        public override void Initialize()
        {
            base.Initialize();

            Documents = new PXSelectExtension<Document>(Base.AMBatchDataMember);
            Lines = new PXSelectExtension<DocumentLine>(Base.AMMTranDataMember);
        }
    }

    public class DisassemblyEntryShiftedPeriodsExt : AMShiftedPeriodsExt<DisassemblyEntry, AMDisassembleBatch, AMDisassembleBatch.tranDate, AMDisassembleBatch.tranPeriodID, AMDisassembleTran>
    {
        public override void Initialize()
        {
            base.Initialize();

            Documents = new PXSelectExtension<Document>(Base.Document);
            Lines = new PXSelectExtension<DocumentLine>(Base.MaterialTransactionRecords);
        }
    }

    /// <summary>
    /// MFG implementation to Support of Different Financial Calendars (2019R1)
    /// </summary>
    public abstract class AMShiftedPeriodsExt<TGraph, TDocument, THeaderDocDate, THeaderTranPeriodID, TDocumentLine> : DocumentWithLinesGraphExtension<TGraph>
        where TGraph : PXGraph
        where TDocument : IBqlTable
        where TDocumentLine : IBqlTable
        where THeaderTranPeriodID : IBqlField
        where THeaderDocDate : IBqlField
    {
        protected override DocumentMapping GetDocumentMapping()
        {
            return new DocumentMapping(typeof(TDocument))
            {
                HeaderTranPeriodID = typeof(THeaderTranPeriodID),
                HeaderDocDate = typeof(THeaderDocDate)
            };
        }

        protected override DocumentLineMapping GetDocumentLineMapping()
        {
            return new DocumentLineMapping(typeof(TDocumentLine));
        }
    }
}
