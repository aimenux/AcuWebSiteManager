using System;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Note")]
    public class DailyFieldReportNote : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? DailyFieldReportNoteId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDBDateAndTime(DisplayNameTime = "Time", UseTimeZone = false)]
        [PXUIField(Enabled = false)]
        public virtual DateTime? Time
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Description")]
        public virtual string Description
        {
            get;
            set;
        }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "Last Modified By", Enabled = false)]
        public override Guid? LastModifiedById
        {
            get;
            set;
        }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modification Date", Enabled = false)]
        public override DateTime? LastModifiedDateTime
        {
            get;
            set;
        }

        [PXNote]
        public override Guid? NoteID
        {
            get;
            set;
        }

        public abstract class dailyFieldReportNoteId : BqlInt.Field<dailyFieldReportNoteId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class time : BqlDateTime.Field<time>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }
    }
}