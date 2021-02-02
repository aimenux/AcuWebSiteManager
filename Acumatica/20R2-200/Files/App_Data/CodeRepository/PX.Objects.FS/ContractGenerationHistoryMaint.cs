using PX.Data;

namespace PX.Objects.FS
{
    public class ContractGenerationHistoryMaint : PXGraph<ContractGenerationHistoryMaint, FSContractGenerationHistory>
    {
        [PXImport(typeof(FSContractGenerationHistory))]
        public PXSelectOrderBy<FSContractGenerationHistory,
               OrderBy
                    <Asc<FSContractGenerationHistory.generationID>>> ContractGenerationHistoryRecords;
    }
}