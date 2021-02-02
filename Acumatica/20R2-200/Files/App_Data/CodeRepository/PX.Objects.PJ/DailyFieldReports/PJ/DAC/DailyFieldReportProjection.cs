using System;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [Serializable]
    [PXProjection(typeof(Select<DailyFieldReport>))]
    public class DailyFieldReportProjection : IBqlTable
    {
        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId> { }
        [PXDBInt(BqlField = typeof(DailyFieldReport.dailyFieldReportId))]
        [PXSelector(typeof(Search<dailyFieldReportId>), SubstituteKey = typeof(dailyFieldReportCd))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportCd : BqlString.Field<dailyFieldReportCd> { }
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC", BqlField = typeof(DailyFieldReport.dailyFieldReportCd))]
        [PXSelector(typeof(dailyFieldReportCd))]
        public virtual string DailyFieldReportCd
        {
            get;
            set;
        }
    }
}