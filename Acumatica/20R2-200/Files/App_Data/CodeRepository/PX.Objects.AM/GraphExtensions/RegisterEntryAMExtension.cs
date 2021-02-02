using PX.Data;
using PX.Objects.CR;
using PX.Objects.PM;
using System;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM.GraphExtensions
{
    public class RegisterEntryAMExtension : PXGraphExtension<RegisterEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        [PXOverride]
        public virtual PMTran CreateTransaction(PMTimeActivity timeActivity, int? employeeID, DateTime date, int? timeSpent, int? timeBillable, decimal? cost, decimal? overtimeMult,
            Func<PMTimeActivity, int?, DateTime, int?, int?, decimal?, decimal?, PMTran> method)
        {
            var timeActExt = PXCache<PMTimeActivity>.GetExtension<PMTimeActivityExt>(timeActivity);
            if(timeActExt != null && timeActExt.AMIsProd == true)
            {
                return null;
            }

            return method?.Invoke(timeActivity, employeeID, date, timeSpent, timeBillable, cost, overtimeMult);
        }
    }
}