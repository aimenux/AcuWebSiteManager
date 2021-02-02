using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
    public abstract class PaidInvoiceGraphExtension<TGraph>: InvoiceBaseGraphExtension<TGraph, PaidInvoice, PaidInvoiceMapping> 
        where TGraph : PXGraph
    {
    }
}
