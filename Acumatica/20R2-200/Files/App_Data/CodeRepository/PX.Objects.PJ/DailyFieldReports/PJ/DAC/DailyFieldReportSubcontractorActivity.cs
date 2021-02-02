using System;
using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.FS;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Subcontractor Activity")]
    public class DailyFieldReportSubcontractorActivity : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? SubcontractorId
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

        [Subcontractor]
        public virtual int? VendorId
        {
            get;
            set;
        }

        [PXString]
        [PXFormula(typeof(Selector<vendorId, Vendor.acctName>))]
        public virtual string VendorName
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDefault]
        [PXSelector(typeof(SearchFor<PMTask.taskID>.
                Where<PMTask.projectID.IsEqual<DailyFieldReport.projectId.FromCurrent>.
                    And<PMTask.type.IsNotEqual<ProjectTaskType.revenue>>>),
            typeof(PMTask.taskCD), typeof(PMTask.description), typeof(PMTask.status),
            SubstituteKey = typeof(PMTask.taskCD),
            DescriptionField = typeof(PMTask.description))]
        [PXUIField(DisplayName = "Project Task")]
        public virtual int? ProjectTaskID
        {
            get;
            set;
        }

        [CostCode(null, typeof(projectTaskID), DisplayName = "Cost Code", Required = true)]
        public virtual int? CostCodeId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDefault]
        [PXUIVerify(typeof(numberOfWorkers.IsGreater<SharedClasses.int_0>), PXErrorLevel.Error,
            DailyFieldReportMessages.ValueMustBePositive, CheckOnInserted = false, CheckOnRowSelected = false)]
        [PXUIField(DisplayName = "Number of Workers")]
        public virtual int? NumberOfWorkers
        {
            get;
            set;
        }

        [PXUIField]
        [PXDBDateAndTime(DisplayNameTime = "Arrived", UseTimeZone = false)]
        [DefaultWorkingTimeStart]
        [WorkingHoursReference]
        public virtual DateTime? TimeArrived
        {
            get;
            set;
        }

        [PXUIField]
        [PXDBDateAndTime(DisplayNameTime = "Departed", UseTimeZone = false)]
        [DefaultWorkingTimeEnd]
        [WorkingHoursReference]
        [PXUIVerify(typeof(timeDeparted.IsGreater<timeArrived>), PXErrorLevel.Error,
            DailyFieldReportMessages.DepartureTimeMustBeLaterThanArrivalTime,
            CheckOnInserted = false, CheckOnRowSelected = false)]
        public virtual DateTime? TimeDeparted
        {
            get;
            set;
        }

        /// <summary>
        /// Represents time in minutes.
        /// </summary>
        [PXDBInt]
        [PXDefault(9 * 60)]
        [PXTimeList]
        [PXUIField(DisplayName = "Working Hours")]
        public virtual int? WorkingTimeSpent
        {
            get;
            set;
        }

        public int DefaultWorkingTimeSpent
        {
            get
            {
                var defaultWorkingHours = TimeDeparted.GetValueOrDefault().Subtract(TimeArrived.GetValueOrDefault());
                return (int) defaultWorkingHours.TotalMinutes;
            }
        }

        [PXDBInt]
        [PXTimeList]
        [PXDependsOnFields(typeof(numberOfWorkers), typeof(workingTimeSpent))]
        [PXUIField(DisplayName = "Working Hours Total", Enabled = false)]
        public virtual int? TotalWorkingTimeSpent => NumberOfWorkers * WorkingTimeSpent;

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

        public abstract class subcontractorId : BqlInt.Field<subcontractorId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class vendorId : BqlInt.Field<vendorId>
        {
        }

        public abstract class projectTaskID : BqlInt.Field<projectTaskID>
        {
        }

        public abstract class costCodeId : BqlInt.Field<costCodeId>
        {
        }

        public abstract class numberOfWorkers : BqlInt.Field<numberOfWorkers>
        {
        }

        public abstract class timeArrived : BqlDateTime.Field<timeArrived>
        {
        }

        public abstract class timeDeparted : BqlDateTime.Field<timeDeparted>
        {
        }

        public abstract class workingTimeSpent : BqlInt.Field<workingTimeSpent>
        {
        }

        public abstract class totalWorkingTimeSpent : BqlInt.Field<totalWorkingTimeSpent>
        {
        }

        public abstract class description : BqlString.Field<description>
        {
        }
    }
}