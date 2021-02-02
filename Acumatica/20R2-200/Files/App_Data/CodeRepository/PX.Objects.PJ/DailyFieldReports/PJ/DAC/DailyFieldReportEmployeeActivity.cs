using System;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Employee Activity")]
    public class DailyFieldReportEmployeeActivity : IBqlTable
    {
        [PXDBIdentity]
        public virtual int? DailyFieldReportEmployeeActivityId
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDBGuid(IsKey = true)]
        [PXParent(typeof(SelectFrom<EPActivityApprove>
            .Where<EPActivityApprove.noteID.IsEqual<employeeActivityId>>))]
        public virtual Guid? EmployeeActivityId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportEmployeeActivityId : BqlInt.Field<dailyFieldReportEmployeeActivityId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class employeeActivityId : BqlGuid.Field<employeeActivityId>
        {
        }
    }
}