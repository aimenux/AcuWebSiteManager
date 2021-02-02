using System;
using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CR;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Visitor")]
    public class DailyFieldReportVisitor : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? DailyFieldReportVisitorId
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

        [PXDBString(50, IsUnicode = true)]
        [PXDefault]
        [VisitorType.List]
        [PXUIField(DisplayName = "Visitor Type", Required = true)]
        public virtual string VisitorType
        {
            get;
            set;
        }

        [PXDBString(50, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Name", Required = true)]
        public virtual string VisitorName
        {
            get;
            set;
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Business Account")]
        [PXDimensionSelector(BAccountAttribute.DimensionName,
            typeof(SearchFor<BAccountR.bAccountID>.
                Where<Match<AccessInfo.userName.FromCurrent>>),
            typeof(BAccountR.acctCD),
            typeof(BAccountR.acctCD),
            typeof(BAccountR.acctName),
            typeof(BAccount.classID),
            typeof(BAccountR.type),
            typeof(BAccount.parentBAccountID),
            typeof(BAccount.acctReferenceNbr))]
        public virtual int? BusinessAccountId
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Company")]
        public virtual string Company
        {
            get;
            set;
        }

        [PXDefault]
        [PXUIField]
        [DefaultWorkingTimeStart]
        [PXDBDateAndTime(DisplayNameTime = "Arrived", UseTimeZone = false)]
        public virtual DateTime? TimeArrived
        {
            get;
            set;
        }

        [PXUIField]
        [PXDBDateAndTime(DisplayNameTime = "Departed", UseTimeZone = false)]
        [PXDefault]
        [DefaultWorkingTimeEnd]
        [PXUIVerify(typeof(timeDeparted.IsGreater<timeArrived>), PXErrorLevel.Error,
            DailyFieldReportMessages.DepartureTimeMustBeLaterThanArrivalTime,
            CheckOnInserted = false, CheckOnRowSelected = false)]
        public virtual DateTime? TimeDeparted
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Purpose of Visit", Required = true)]
        public virtual string PurposeOfVisit
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Area Visited/Inspected Entity")]
        public virtual string AreaVisited
        {
            get;
            set;
        }

        [PXDBString(255, IsUnicode = true)]
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

        public abstract class dailyFieldReportVisitorId : BqlInt.Field<dailyFieldReportVisitorId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class visitorType : BqlString.Field<visitorType>
        {
        }

        public abstract class visitorName : BqlString.Field<visitorName>
        {
        }

        public abstract class businessAccountId : BqlInt.Field<businessAccountId>
        {
        }

        public abstract class company : BqlString.Field<company>
        {
        }

        public abstract class timeArrived : BqlDateTime.Field<timeArrived>
        {
        }

        public abstract class timeDeparted : BqlDateTime.Field<timeDeparted>
        {
        }

        public abstract class purposeOfVisit : BqlString.Field<purposeOfVisit>
        {
        }

        public abstract class areaVisited : BqlString.Field<areaVisited>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }
    }
}