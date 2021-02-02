using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.CR;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
    public abstract class InvoiceBaseGraphExtension<TGraph, TInvoice, TInvoiceMapping> : DocumentWithLinesGraphExtension<TGraph, TInvoice, TInvoiceMapping>
        where TGraph : PXGraph
        where TInvoice : InvoiceBase, new()
        where TInvoiceMapping : IBqlMapping
    {
        public PXSelectExtension<Contragent> Contragent;

		protected virtual ContragentMapping GetContragentMapping()
		{
			return new ContragentMapping(typeof(Stub));
		}

		public abstract void SuppressApproval();

		public virtual PXSelectBase<Location> Location { get; }

		public virtual PXSelectBase<CurrencyInfo> CurrencyInfo { get; }

        public PXSelectExtension<InvoiceTran> InvoiceTrans;

		protected virtual InvoiceTranMapping GetInvoiceTranMapping()
		{
			return new InvoiceTranMapping(typeof(Stub));
		}

		public PXSelectExtension<GenericTaxTran> TaxTrans;

		protected virtual GenericTaxTranMapping GetGenericTaxTranMapping()
		{
			return new GenericTaxTranMapping(typeof(Stub));
		}

		public PXSelectExtension<LineTax> LineTaxes;

		protected virtual LineTaxMapping GetLineTaxMapping()
		{
			return new LineTaxMapping(typeof(Stub));
		}

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

		[PXHidden]
		protected class Stub: IBqlTable { }
    }
}
