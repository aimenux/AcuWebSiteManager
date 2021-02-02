using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.GL;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
    public abstract class InvoiceGraphExtension<TGraph>: InvoiceBaseGraphExtension<TGraph, Invoice, InvoiceMapping> 
        where TGraph : PXGraph
    {
        protected abstract Adjust2Mapping GetAdjust2Mapping();

        public PXSelectExtension<Adjust2> AppliedAdjustments;

        protected override void _(Events.RowUpdated<Invoice> e)
        {
            base._(e);

            if (ShouldUpdateAdjustmentsOnDocumentUpdated(e))
            {
                foreach (Adjust2 adjust in AppliedAdjustments.Select())
                {
                    if (!e.Cache.ObjectsEqual<Invoice.branchID>(e.Row, e.OldRow))
                    {
                        AppliedAdjustments.Cache.SetDefaultExt<Adjust2.adjdBranchID>(adjust);
                    }

                    if (!e.Cache.ObjectsEqual<Invoice.headerTranPeriodID>(e.Row, e.OldRow))
                    {
                        FinPeriodIDAttribute.DefaultPeriods<Adjust2.adjgFinPeriodID>(AppliedAdjustments.Cache, adjust);
                        FinPeriodIDAttribute.DefaultPeriods<Adjust2.adjdFinPeriodID>(AppliedAdjustments.Cache, adjust);
                    }

                    (AppliedAdjustments.Cache as PXModelExtension<Adjust2>)?.UpdateExtensionMapping(adjust);

                    AppliedAdjustments.Cache.MarkUpdated(adjust);
                }
            }
        }

        #region Adjustments

        protected virtual bool ShouldUpdateAdjustmentsOnDocumentUpdated(Events.RowUpdated<Invoice> e)
        {
            return ShouldUpdateDetailsOnDocumentUpdated(e);
        }

        #endregion
    }
}
