using System;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
    /// <summary>
    /// Manufacturing extension for Inventory Close Financial Periods (IN509000)
    /// </summary>
    public class INClosingProcessAMExtension : PXGraphExtension<INClosingProcess>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        [PXOverride]
        public virtual ProcessingResult CheckOpenDocuments(IFinPeriod finPeriod, Func<IFinPeriod, ProcessingResult> del)
        {
            var results = del?.Invoke(finPeriod) ?? new ProcessingResult();

            if (HasUnreleasedAMDocuments(finPeriod))
            {
                results.AddErrorMessage(PX.Objects.AM.Messages.GetLocal(PX.Objects.AM.Messages.PeriodHasUnreleasedProductionDocs, FinPeriodIDAttribute.FormatForError(finPeriod.FinPeriodID)));
            }

            return results;
        }

        protected virtual bool HasUnreleasedAMDocuments(IFinPeriod finPeriod)
        {
            AMBatch result = PXSelect<
                AMBatch,
                Where<AMBatch.released, NotEqual<True>,
                    And<AMBatch.finPeriodID, Equal<Required<AMBatch.finPeriodID>>>>>
                .SelectWindowed(Base, 0, 1, finPeriod.FinPeriodID);
            return result?.BatNbr != null;
        }
    }
}
