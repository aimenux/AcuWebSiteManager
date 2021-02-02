using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.GL;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
    public abstract class PaymentGraphExtension<TGraph> : PXGraphExtension<TGraph>,
        IDocumentWithFinDetailsGraphExtension
        where TGraph : PXGraph
    {
        protected abstract PaymentMapping GetPaymentMapping();

        protected abstract AdjustMapping GetAdjustMapping();

        /// <summary>A mapping-based view of the <see cref="Payment" /> data.</summary>
        public PXSelectExtension<Payment> Documents;
        /// <summary>A mapping-based view of the <see cref="Adjust" /> data.</summary>
        public PXSelectExtension<Adjust> Adjustments;

        public List<int?> GetOrganizationIDsInDetails()
        {
            return Adjustments
                .Select()
                .SelectMany(row => GetAdjustBranchIDs((Adjust) row))
                .Select(PXAccess.GetParentOrganizationID)
                .Distinct()
                .ToList();
        }

        protected virtual IEnumerable<int?> GetAdjustBranchIDs(Adjust adjust)
        {
            yield return adjust.AdjdBranchID;
            yield return adjust.AdjgBranchID;
        }

        protected virtual void _(Events.RowUpdated<Payment> e)
        {
            if (!e.Cache.ObjectsEqual<Payment.adjDate, Payment.adjTranPeriodID, Payment.curyID, Payment.branchID>(e.Row, e.OldRow))
            {
                foreach (Adjust adjust in Adjustments.Select())
                {
                    if (!e.Cache.ObjectsEqual<Payment.branchID>(e.Row, e.OldRow))
                    {
                        Adjustments.Cache.SetDefaultExt<Adjust.adjgBranchID>(adjust);
                    }

                    if (!e.Cache.ObjectsEqual<Payment.adjTranPeriodID>(e.Row, e.OldRow))
                    {
                        FinPeriodIDAttribute.DefaultPeriods<Adjust.adjgFinPeriodID>(Adjustments.Cache, adjust);
                    }	                
					
                    (Adjustments.Cache as PXModelExtension<Adjust>)?.UpdateExtensionMapping(adjust);

                    Adjustments.Cache.MarkUpdated(adjust);
                }
            }
        }
    }
}
