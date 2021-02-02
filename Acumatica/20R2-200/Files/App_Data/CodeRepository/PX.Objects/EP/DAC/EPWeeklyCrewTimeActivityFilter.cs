using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.Objects.EP
{
	[Serializable]
	[PXCacheName(Messages.EPWeeklyCrewTimeActivityFilter)]
	public class EPWeeklyCrewTimeActivityFilter : IBqlTable
	{
		#region ProjectID
		[ActiveProjectOrContractBase]
		public virtual int? ProjectID { get; set; }
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		#endregion

		#region ProjectTaskID
		[ProjectTask(typeof(EPWeeklyCrewTimeActivityFilter.projectID))]
		public virtual int? ProjectTaskID { get; set; }
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		#endregion

		#region Day
		[PXInt]
		[PXUIField(DisplayName = "Day")]
		[EP.DayOfWeek]
		public virtual int? Day { get; set; }
		public abstract class day : PX.Data.BQL.BqlInt.Field<day> { }
		#endregion

		#region RegularTime
		[PXInt]
		[PXUIField(DisplayName = "Time", Enabled = false)]
		[PXUnboundDefault(0)]
		public virtual int? RegularTime { get; set; }
		public abstract class regularTime : PX.Data.BQL.BqlInt.Field<regularTime> { }
		#endregion

		#region Overtime
		[PXInt]
		[PXUIField(DisplayName = "Overtime", Enabled = false)]
		[PXUnboundDefault(0)]
		public virtual int? Overtime { get; set; }
		public abstract class overtime : PX.Data.BQL.BqlInt.Field<overtime> { }
		#endregion

		#region TotalTime
		// For some reasons, PXFormula wasn't working, so I'm using property calculation.
		[PXInt]
		[PXUIField(DisplayName = "Total Time", Enabled = false)]
		[PXUnboundDefault(0)]
		[PXDependsOnFields(typeof(EPWeeklyCrewTimeActivityFilter.regularTime), typeof(EPWeeklyCrewTimeActivityFilter.overtime))]
		public virtual int? TotalTime => RegularTime.GetValueOrDefault() + Overtime.GetValueOrDefault();
		public abstract class totalTime : PX.Data.BQL.BqlInt.Field<totalTime> { }
		#endregion

		#region BillableTime
		[PXInt]
		[PXUIField(DisplayName = "Billable", Enabled = false)]
		[PXUnboundDefault(0)]
		public virtual int? BillableTime { get; set; }
		public abstract class billableTime : PX.Data.BQL.BqlInt.Field<billableTime> { }
		#endregion

		#region BillableOvertime
		[PXInt]
		[PXUIField(DisplayName = "Billable Overtime", Enabled = false)]
		[PXUnboundDefault(0)]
		public virtual int? BillableOvertime { get; set; }
		public abstract class billableOvertime : PX.Data.BQL.BqlInt.Field<billableOvertime> { }
		#endregion

		#region TotalBillableTime
		[PXInt]
		[PXUIField(DisplayName = "Total Billable Time", Enabled = false)]
		[PXUnboundDefault(0)]
		[PXDependsOnFields(typeof(EPWeeklyCrewTimeActivityFilter.billableTime), typeof(EPWeeklyCrewTimeActivityFilter.billableOvertime))]
		public virtual int? TotalBillableTime => BillableTime.GetValueOrDefault() + BillableOvertime.GetValueOrDefault();
		public abstract class totalBillableTime : PX.Data.BQL.BqlInt.Field<totalBillableTime> { }
		#endregion

		#region TotalWorkgroupMembers
		[PXInt]
		[PXUIField(DisplayName = "Workgroup Members", Enabled = false)]
		public virtual int? TotalWorkgroupMembers { get; set; }
		public abstract class totalWorkgroupMembers : PX.Data.BQL.BqlInt.Field<totalWorkgroupMembers> { }
		#endregion

		#region TotalWorkgroupMembersWithActivities
		[PXInt]
		[PXUIField(DisplayName = "Members with Activities", Enabled = false)]
		public virtual int? TotalWorkgroupMembersWithActivities { get; set; }
		public abstract class totalWorkgroupMembersWithActivities : PX.Data.BQL.BqlInt.Field<totalWorkgroupMembersWithActivities> { }
		#endregion

		#region ShowAllMembers
		[PXBool]
		[PXUIField(DisplayName = "Show All Members")]
		[PXUnboundDefault(false)]
		public virtual bool? ShowAllMembers { get; set; }
		public abstract class showAllMembers : PX.Data.BQL.BqlBool.Field<showAllMembers> { }
		#endregion
	}
}