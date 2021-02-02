using PX.Data;

namespace PX.Objects.FS
{
    public class ContractPostPeriodEntry : PXGraph<ContractPostPeriodEntry, FSContractPostDoc>
    {
        public PXSelect<FSContractPostDoc> ContractPostDocRecord;

        public PXSelect<FSContractPostDet> ContractPostDetRecords;
    }
}