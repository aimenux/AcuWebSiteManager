using System;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.CA
{
    [Serializable]
    [Obsolete(Common.Messages.AttributeIsObsoleteAndWillBeRemoved2020R2)]
    public class PXVendorCustomerWithCreditSelectorAttribute : PXVendorCustomerSelectorAttribute
    {
        public PXVendorCustomerWithCreditSelectorAttribute(Type batchModule) : base(batchModule)
        {
        }

        public PXVendorCustomerWithCreditSelectorAttribute(Type batchModule, Type curyID) : base(batchModule, curyID)
        {
        }

        public PXVendorCustomerWithCreditSelectorAttribute(Type batchModule, bool onlyActive) : base(batchModule,
            onlyActive)
        {
        }

        public PXVendorCustomerWithCreditSelectorAttribute(Type batchModule, Type curyID, bool onlyActive) : base(
            batchModule,
            curyID, onlyActive)
        {
        }

        protected override void GetRecords()
        {
            PXView view = PXView.View;
            base.GetRecords();
            view.WhereAnd(
                typeof(Where<BAccountR.status, In3<BAccountR.status.active, BAccountR.status.oneTime,
                    BAccountR.status.holdPayments, BAccountR.status.creditHold>>));
        }
    }
}