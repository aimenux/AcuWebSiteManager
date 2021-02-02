using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.EP.GraphExtensions
{
    public class ExpenseClaimDetailEntryProjectAvailabilityExt : PXGraphExtension<ExpenseClaimDetailEntryExt,
        ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt, ExpenseClaimDetailEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public virtual void _(Events.RowSelected<EPExpenseClaimDetails> args, PXRowSelected baseHandler)
        {
            baseHandler(args.Cache, args.Args);
            if (args.Row is EPExpenseClaimDetails claimDetails &&
                args.Cache.GetEnabled<EPExpenseClaimDetails.contractID>(claimDetails))
            {
                var isEditable = Base2.IsProjectEditable();
                PXUIFieldAttribute.SetEnabled<EPExpenseClaimDetails.contractID>(args.Cache, claimDetails, isEditable);
            }
        }
    }
}