using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
    public class INAdjustmentEntryAMExtension : PXGraphExtension<INAdjustmentEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        protected virtual void INRegister_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            if (del != null)
            {
                del(sender, e);
            }

            if (e.Row == null || sender == null || sender.Graph == null)
            {
                return;
            }

            if (((INRegister)e.Row).Released == false
                //&& ((INRegister)e.Row).Hold == true
                && ((INRegister)e.Row).OrigModule == Common.ModuleAM)
            {
                PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
                sender.AllowDelete = false;
                Base.transactions.Cache.AllowDelete = false;
                Base.splits.Cache.AllowDelete = false;
                Base.addInvBySite.SetEnabled(false);
                Base.addInvSelBySite.SetEnabled(false);
            }
        }

        protected virtual void INTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            if (del != null)
            {
                del(sender, e);
            }

            INRegister inRegister = Base.adjustment.Current;
            if (e.Row == null 
                || sender == null 
                || sender.Graph == null
                || inRegister == null)
            {
                return;
            }

            if (((INTran)e.Row).Released == false
                && inRegister.OrigModule == Common.ModuleAM)
            {
                PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
            }
        }
    }
}