using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.SO;
using System;

namespace PX.Objects.FS
{
    public class ContractPostPeriodEntry : PXGraph<ContractPostPeriodEntry, FSContractPostDoc>
    {
        public PXSelect<FSContractPostDoc> ContractPostDocRecord;

        public PXSelect<FSContractPostDet> ContractPostDetRecords;
    }
}