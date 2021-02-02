using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM.Attributes
{
    public class MrpNumbering
    {
        public class ForecastAttribute : AutoNumberAttribute
        {
            public ForecastAttribute()
                : base(typeof(AMRPSetup.forecastNumberingID), typeof(AMForecast.endDate))
            {
            }
        }

        public class MpsAttribute : AutoNumberAttribute
        {
            public MpsAttribute()
                : base(typeof(Search<AMMPSType.mpsNumberingID, Where<AMMPSType.mPSTypeID, Equal<Current<AMMPS.mPSTypeID>>>>), typeof(AMMPS.planDate))
            {
            }
        }
    }
}