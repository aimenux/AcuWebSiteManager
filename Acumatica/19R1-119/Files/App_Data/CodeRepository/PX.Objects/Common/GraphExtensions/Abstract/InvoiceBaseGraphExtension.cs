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
    public abstract class InvoiceBaseGraphExtension<TGraph, TInvoice, TInvoiceMapping> : DocumentWithLinesGraphExtension<TGraph, TInvoice, TInvoiceMapping>
        where TGraph : PXGraph
        where TInvoice : InvoiceBase, new()
        where TInvoiceMapping : IBqlMapping
    {
        #region Lines

        protected override bool ShouldUpdateDetailsOnDocumentUpdated(Events.RowUpdated<TInvoice> e)
        {
            return base.ShouldUpdateDetailsOnDocumentUpdated(e)
                   || !e.Cache.ObjectsEqual<Invoice.headerDocDate, Invoice.curyID>(e.Row, e.OldRow);
        }

        protected override void ProcessLineOnDocumentUpdated(Events.RowUpdated<TInvoice> e,
            DocumentLine line)
        {
            base.ProcessLineOnDocumentUpdated(e, line); ;

            if (!e.Cache.ObjectsEqual<Invoice.headerDocDate, Invoice.curyID>(e.Row, e.OldRow))
            {
                Lines.Cache.MarkUpdated(line);
            }
        }

        #endregion
    }
}
