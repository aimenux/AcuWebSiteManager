using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Objects.FS
{
    public abstract class SMMultiCurrencyGraph<TGraph, TPrimary> : MultiCurrencyGraph<TGraph, TPrimary>
        where TGraph : PXGraph
        where TPrimary : class, IBqlTable, new()
    {
        protected override CurySourceMapping GetCurySourceMapping()
        {
            return new CurySourceMapping(typeof(Customer));
        }

        protected override void _(Events.RowSelected<Extensions.MultiCurrency.Document> e)
        {
            base._(e);

            if (e.Row == null)
            {
                return;
            }

            PXUIFieldAttribute.SetVisible<Extensions.MultiCurrency.Document.curyID>(e.Cache, e.Row, ExtensionHelper.IsMultyCurrencyEnabled);
            var doc = Documents.Cache.GetMain<Extensions.MultiCurrency.Document>(e.Row);

            if (doc is FSServiceOrder)
            {
                ServiceOrderEntry graphServiceOrder = (ServiceOrderEntry)Documents.Cache.Graph;
                PXUIFieldAttribute.SetEnabled<Extensions.MultiCurrency.Document.curyID>(e.Cache, e.Row, graphServiceOrder.ServiceOrderAppointments.Select().Count == 0);
            }
            else if (doc is FSAppointment)
            {
                FSAppointment fsAppointmentRow = (FSAppointment)doc;
                PXUIFieldAttribute.SetEnabled<Extensions.MultiCurrency.Document.curyID>(e.Cache, e.Row, fsAppointmentRow.SOID < 0);
            }
        }

		protected override void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.moduleCode> e)
		{
			e.NewValue = "AR";
		}
	}
}
