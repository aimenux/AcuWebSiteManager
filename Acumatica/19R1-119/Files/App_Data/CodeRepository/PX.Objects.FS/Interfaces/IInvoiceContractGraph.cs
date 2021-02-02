using PX.Data;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public interface IInvoiceContractGraph
    {
        FSContractPostDoc CreateInvoiceByContract(PXGraph graphProcess, DateTime? invoiceDate, string invoiceFinPeriodID, FSContractPostBatch fsContractPostBatchRow, FSServiceContract fsServiceContractRow, FSContractPeriod fsContractPeriodRow, List<ContractInvoiceLine> invoiceLines);
    }
}
