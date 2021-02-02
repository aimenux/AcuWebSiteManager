using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.Objects.Extensions;

namespace PX.Objects.SO.GraphExtensions.ARPaymentEntryExt
{
    public class AffectedSOOrdersWithPaymentInPendingProcessingExt : ProcessAffectedEntitiesInPrimaryGraphBase<AffectedSOOrdersWithPaymentInPendingProcessingExt, ARPaymentEntry, SOOrder, SOOrderEntry>
    {
        private PXCache<SOOrder> orders => Base.Caches<SOOrder>();
        protected override bool PersistInSameTransaction => false;
        private readonly IDictionary<(string orderType, string orderNbr), int?> _oldPaymentsNeedValidationCntrValues = new Dictionary<(string orderType, string orderNbr), int?>();

        protected override bool EntityIsAffected(SOOrder order)
        {
            int? oldPaymentsNeedValidationCntr = (int?)orders.GetValueOriginal<SOOrder.paymentsNeedValidationCntr>(order);
            if (!Equals(oldPaymentsNeedValidationCntr, order.PaymentsNeedValidationCntr))
            {
                _oldPaymentsNeedValidationCntrValues[(order.OrderType, order.OrderNbr)] = oldPaymentsNeedValidationCntr;
                return true;
            }
            else
                return false;
        }

        protected override void ProcessAffectedEntity(SOOrderEntry orderEntry, SOOrder order)
        {
            int? oldPaymentsNeedValidationCntr = _oldPaymentsNeedValidationCntrValues[(order.OrderType, order.OrderNbr)];
            if (order.PaymentsNeedValidationCntr == 0 && oldPaymentsNeedValidationCntr != null)
                SOOrder.Events.Select(e => e.LostLastPaymentInPendingProcessing).FireOn(orderEntry, order);
            else if (oldPaymentsNeedValidationCntr < order.PaymentsNeedValidationCntr)
                SOOrder.Events.Select(e => e.ObtainedPaymentInPendingProcessing).FireOn(orderEntry, order);
        }

        protected override SOOrder ActualizeEntity(SOOrderEntry orderEntry, SOOrder order) => SOOrder.PK.Find(orderEntry, order);
    }
}