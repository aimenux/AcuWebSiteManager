using System;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.SM;

namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
	[Serializable]
	[PXPrimaryGraph(typeof(ProjectManagementSetupMaint))]
	[PXCacheName(CacheNames.ProjectManagementPreferences)]
	public class ProjectManagementSetup : BaseCache, IBqlTable
	{
		[PXDefault(AnswerDaysCalculationTypeAttribute.SequentialDays)]
		[PXDBString(1, IsUnicode = true)]
		[AnswerDaysCalculationType]
		[PXUIField(DisplayName = "Due Date Calculation Type", Required = true)]
		public virtual string AnswerDaysCalculationType
		{
			get;
			set;
		}

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = ProjectManagementLabels.CalendarId, Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual string CalendarId
		{
			get;
			set;
		}

		[PXDBString(10, IsUnicode = true)]
		[PXDefault("REQFORINFO")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "RFI Numbering Sequence")]
		public virtual string RequestForInformationNumberingId
		{
			get;
			set;
		}

		[PXDBString(10, IsUnicode = true)]
		[PXDefault("PROISSUE")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Project Issue Numbering Sequence")]
		public virtual string ProjectIssueNumberingId
		{
			get;
			set;
		}

		[PXUIField(DisplayName = "Default Email Notification")]
		[PXDBInt]
		[PXSelector(typeof(Search<Notification.notificationID>), DescriptionField = typeof(Notification.name),
			SubstituteKey = typeof(Notification.name))]
		public virtual int? DefaultEmailNotification
		{
			get;
			set;
		}

		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID,
			Where<EPAssignmentMap.entityType, Equal<ProjectIssueAssignmentMapType>>>))]
		[PXUIField(DisplayName = "Project Issue Assignment Map")]
		public virtual int? ProjectIssueAssignmentMapId
		{
			get;
			set;
		}

		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID,
			Where<EPAssignmentMap.entityType, Equal<RequestForInformationAssignmentMapType>>>))]
		[PXUIField(DisplayName = "RFI Assignment Map")]
		public virtual int? RequestForInformationAssignmentMapId
		{
			get;
			set;
		}

		[PXDBString(10, IsUnicode = true)]
		[PXDefault("DFREPORT")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "DFR Numbering Sequence")]
		public virtual string DailyFieldReportNumberingId
		{
			get;
			set;
		}

		[PXDBInt]
		[PXSelector(typeof(SearchFor<EPAssignmentMap.assignmentMapID>.
			Where<EPAssignmentMap.entityType.IsEqual<DailyFieldReportApprovalMapType>
				.And<EPAssignmentMap.mapType.IsNotEqual<EPMapType.assignment>>>))]
		[PXUIField(DisplayName = "DFR Approval Map")]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.approvalWorkflow>))]
		public virtual int? DailyFieldReportApprovalMapId
		{
			get;
			set;
		}

		[PXDBInt]
		[PXUIField(DisplayName = "Pending Approval Notification")]
		[PXSelector(typeof(Search<Notification.notificationID>))]
		[PXDefault(typeof(SearchFor<Notification.notificationID>
				.Where<Notification.name.IsEqual<DailyFieldReportConstants.Notification.name>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.approvalWorkflow>))]
		public virtual int? PendingApprovalNotification
		{
			get;
			set;
		}

		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Enable History Log")]
		public virtual bool? IsHistoryLogEnabled
		{
			get;
			set;
		}

		[PXDefault("SUBMITTAL")]
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Submittal Numbering Sequence")]
		public virtual string SubmittalNumberingId
		{
			get;
			set;
		}

		public abstract class requestForInformationNumberingId : BqlString.Field<requestForInformationNumberingId>
		{
		}

		public abstract class defaultEmailNotification : BqlInt.Field<defaultEmailNotification>
		{
		}

		public abstract class projectIssueNumberingId : BqlString.Field<projectIssueNumberingId>
		{
		}

		public abstract class projectIssueAssignmentMapId : BqlInt.Field<projectIssueAssignmentMapId>
		{
		}

		public abstract class requestForInformationAssignmentMapId : BqlInt.Field<requestForInformationAssignmentMapId>
		{
		}

		public abstract class answerDaysCalculationType : BqlString.Field<answerDaysCalculationType>
		{
		}

		public abstract class calendarId : BqlString.Field<calendarId>
		{
		}

		public abstract class dailyFieldReportNumberingId : BqlString.Field<dailyFieldReportNumberingId>
		{
		}

		public abstract class dailyFieldReportApprovalMapId : BqlInt.Field<dailyFieldReportApprovalMapId>
		{
		}

		public abstract class pendingApprovalNotification : BqlInt.Field<pendingApprovalNotification>
		{
		}

		public abstract class isHistoryLogEnabled : BqlBool.Field<isHistoryLogEnabled>
		{
		}

		public abstract class submittalNumberingId : BqlString.Field<submittalNumberingId>
		{
		}

		public class ProjectIssueAssignmentMapType : BqlString.Constant<ProjectIssueAssignmentMapType>
		{
			public ProjectIssueAssignmentMapType()
				: base(typeof(ProjectIssue).FullName)
			{
			}
		}

		public class RequestForInformationAssignmentMapType : BqlString.Constant<RequestForInformationAssignmentMapType>
		{
			public RequestForInformationAssignmentMapType()
				: base(typeof(RequestForInformation).FullName)
			{
			}
		}

		public class DailyFieldReportApprovalMapType : BqlString.Constant<DailyFieldReportApprovalMapType>
		{
			public DailyFieldReportApprovalMapType()
				: base(typeof(DailyFieldReport).FullName)
			{
			}
		}
	}
}