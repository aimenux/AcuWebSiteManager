using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.TM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.EP
{
	[Serializable]
	[PXCacheName(Messages.EPTimeActivitiesSummary)]
	public class EPTimeActivitiesSummary : IBqlTable
	{
		#region Selected
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		#endregion

		#region ContactID
		[SubordinateOwner(IsKey = true)]
		[PXUIField(DisplayName = "Employee")]
		[PXDefault]
		[PXParent(typeof(Select<EPWeeklyCrewTimeActivity,
			Where<EPWeeklyCrewTimeActivity.workgroupID, Equal<Current<EPTimeActivitiesSummary.workgroupID>>,
				And<EPWeeklyCrewTimeActivity.week, Equal<Current<EPTimeActivitiesSummary.week>>>>>),
			ParentCreate = true,
			LeaveChildren = true)]
		public virtual int? ContactID { get; set; }
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		#endregion

		#region WorkgroupID
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Workgroup", Enabled = false)]
		//PXDefault instead of PXDBDefault otherwise insertion fails in EPWeeklyCrewTimeEntry.workgroupTimeSummary delegate. Children needs to exist even without parent.
		[PXDefault(typeof(EPWeeklyCrewTimeActivity.workgroupID))]
		[PXWorkgroupSelector]
		public virtual int? WorkgroupID { get; set; }
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		#endregion

		#region Week
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Week", Enabled = false)]
		//PXDefault instead of PXDBDefault otherwise insertion fails in EPWeeklyCrewTimeEntry.workgroupTimeSummary delegate. Children needs to exist even without parent.
		[PXDefault(typeof(EPWeeklyCrewTimeActivity.week))]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
		public virtual int? Week { get; set; }
		public abstract class week : PX.Data.BQL.BqlInt.Field<week> { }
		#endregion

		#region MondayTime
		[PXDBInt]
		[PXUIField(DisplayName = "Monday", Enabled = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? MondayTime { get; set; }
		public abstract class mondayTime : PX.Data.BQL.BqlInt.Field<mondayTime> { }
		#endregion

		#region TuesdayTime
		[PXDBInt]
		[PXUIField(DisplayName = "Tuesday", Enabled = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? TuesdayTime { get; set; }
		public abstract class tuesdayTime : PX.Data.BQL.BqlInt.Field<tuesdayTime> { }
		#endregion

		#region WednesdayTime
		[PXDBInt]
		[PXUIField(DisplayName = "Wednesday", Enabled = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? WednesdayTime { get; set; }
		public abstract class wednesdayTime : PX.Data.BQL.BqlInt.Field<wednesdayTime> { }
		#endregion

		#region ThursdayTime
		[PXDBInt]
		[PXUIField(DisplayName = "Thursday", Enabled = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? ThursdayTime { get; set; }
		public abstract class thursdayTime : PX.Data.BQL.BqlInt.Field<thursdayTime> { }
		#endregion

		#region FridayTime
		[PXDBInt]
		[PXUIField(DisplayName = "Friday", Enabled = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? FridayTime { get; set; }
		public abstract class fridayTime : PX.Data.BQL.BqlInt.Field<fridayTime> { }
		#endregion

		#region SaturdayTime
		[PXDBInt]
		[PXUIField(DisplayName = "Saturday", Enabled = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? SaturdayTime { get; set; }
		public abstract class saturdayTime : PX.Data.BQL.BqlInt.Field<saturdayTime> { }
		#endregion

		#region SundayTime
		[PXDBInt]
		[PXUIField(DisplayName = "Sunday", Enabled = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? SundayTime { get; set; }
		public abstract class sundayTime : PX.Data.BQL.BqlInt.Field<sundayTime> { }
		#endregion

		#region TotalRegularTime
		[PXDBInt]
		[PXUIField(DisplayName = "Total Regular Time", Enabled = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? TotalRegularTime { get; set; }
		public abstract class totalRegularTime : PX.Data.BQL.BqlInt.Field<totalRegularTime> { }
		#endregion

		#region TotalBillableTime
		[PXDBInt]
		[PXUIField(DisplayName = "Total Billable Time", Enabled = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? TotalBillableTime { get; set; }
		public abstract class totalBillableTime : PX.Data.BQL.BqlInt.Field<totalBillableTime> { }
		#endregion

		#region TotalOvertime
		[PXDBInt]
		[PXUIField(DisplayName = "Total Overtime", Enabled = false, Visible = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? TotalOvertime { get; set; }
		public abstract class totalOvertime : PX.Data.BQL.BqlInt.Field<totalOvertime> { }
		#endregion

		#region TotalBillableOvertime
		[PXDBInt]
		[PXUIField(DisplayName = "Total Billable Overtime", Enabled = false, Visible = false)]
		[PXTimeList]
		[PXDefault(0)]
		public virtual int? TotalBillableOvertime { get; set; }
		public abstract class totalBillableOvertime : PX.Data.BQL.BqlInt.Field<totalBillableOvertime> { }
		#endregion

		#region Status
		[PXString(5, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[PXDBScalar(typeof(SearchFor<EPCompanyTreeMember.membershipType>
			.Where<EPCompanyTreeMember.workGroupID.IsEqual<EPTimeActivitiesSummary.workgroupID>
				.And<EPCompanyTreeMember.contactID.IsEqual<EPTimeActivitiesSummary.contactID>>>))]
		[WorkgroupMemberStatus]
		public virtual string Status { get; set; }
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion

		#region IsMemberActive
		[PXBool]
		[PXDBScalar(typeof(SearchFor<EPCompanyTreeMember.active>
			.Where<EPCompanyTreeMember.workGroupID.IsEqual<EPTimeActivitiesSummary.workgroupID>
				.And<EPCompanyTreeMember.contactID.IsEqual<EPTimeActivitiesSummary.contactID>>>))]
		public virtual bool? IsMemberActive { get; set; }
		public abstract class isMemberActive : PX.Data.BQL.BqlBool.Field<isMemberActive> { }
		#endregion

		#region IsWithoutActivities
		[PXBool]
		[PXFormula(typeof(False.When<EPTimeActivitiesSummary.totalRegularTime.IsNotEqual<Zero>
				.Or<EPTimeActivitiesSummary.totalOvertime.IsNotEqual<Zero>>>
			.Else<True>))]
		public virtual bool? IsWithoutActivities { get; set; }
		public abstract class isWithoutActivities : PX.Data.BQL.BqlBool.Field<isWithoutActivities> { }
		#endregion

		#region System Columns
		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion System Columns
	}
}
