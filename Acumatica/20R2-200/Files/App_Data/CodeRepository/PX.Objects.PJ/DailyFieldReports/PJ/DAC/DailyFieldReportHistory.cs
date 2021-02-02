using System;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report History")]
    public class DailyFieldReportHistory : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public int? DailyFieldReportHistoryId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId>>))]
        public int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "File Name", Enabled = false)]
        public string FileName
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Comment")]
        public string Comment
        {
            get;
            set;
        }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Completed By", Enabled = false)]
        public override Guid? CreatedById
        {
            get;
            set;
        }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Completed Date", Enabled = false)]
        public override DateTime? CreatedDateTime
        {
            get;
            set;
        }

        public abstract class dailyFieldReportHistoryId : BqlInt.Field<dailyFieldReportHistoryId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class fileName : BqlString.Field<fileName>
        {
        }

        public abstract class comment : BqlString.Field<comment>
        {
        }

        public abstract class noteID : BqlGuid.Field<noteID>
        {
        }

        public abstract class createdById : BqlGuid.Field<createdById>
        {
        }

        public abstract class createdDateTime : BqlDateTime.Field<createdDateTime>
        {
        }

        public abstract class tstamp : BqlByteArray.Field<tstamp>
        {
        }

        public abstract class createdByScreenId : BqlString.Field<createdByScreenId>
        {
        }

        public abstract class lastModifiedById : BqlGuid.Field<lastModifiedById>
        {
        }

        public abstract class lastModifiedByScreenId : BqlString.Field<lastModifiedByScreenId>
        {
        }

        public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime>
        {
        }
    }
}
