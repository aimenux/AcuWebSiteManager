using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.Common;
using System;
using System.Collections.Generic;
using PX.Objects.PJ.Common.DAC;

namespace PX.Objects.PJ.Submittals.PJ.DAC
{
	[Serializable]
	[PXCacheName("Submittal Workflow Item")]
	public class PJSubmittalWorkflowItem : PX.Data.IBqlTable
	{
		#region RefNbr
		public abstract class submittalID : PX.Data.BQL.BqlString.Field<submittalID>
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(PJSubmittal.submittalID))]
		[PXParent(typeof(Select<PJSubmittal,
			Where<PJSubmittal.submittalID, Equal<Current<PJSubmittalWorkflowItem.submittalID>>,
				And<PJSubmittal.revisionID, Equal<Current<PJSubmittalWorkflowItem.revisionID>>>>>))]
		public virtual string SubmittalID
		{
			get;
			set;
		}
		#endregion

		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID>
		{
		}

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PJSubmittal.revisionID))]
		public virtual int? RevisionID
		{
			get;
			set;
		}
		#endregion

		#region LineNbr

		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr>
		{
		}

		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PJSubmittal))]
		[PXUIField(DisplayName = "Line Nbr.")]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.Visible,
			Required = true)]
		[PXSelector(typeof(Search<ContactForCurrentProject.contactID,
				Where<ContactForCurrentProject.contactType,
					In3<ContactTypesAttribute.person, ContactTypesAttribute.employee>>>),
			DescriptionField = typeof(ContactForCurrentProject.displayName),
			Filterable = true)]
		[PXRestrictor(typeof(Where<ContactForCurrentProject.isActive, Equal<True>>),
			RequestForInformationMessages.OnlyActiveContactsAreAllowed)]
		public virtual int? ContactID
		{
			get;
			set;
		}
		#endregion

		#region Role
		public abstract class role : PX.Data.BQL.BqlString.Field<role>
		{
			public const string Submitter = "S";
			public const string Approver = "A";
			public const string Reviewer = "R";

			public class Labels : ILabelProvider
			{
				private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
				{
					{ Submitter, ProjectManagementMessages.SubmitterLabel },
					{ Approver, ProjectManagementMessages.ApproverLabel },
					{ Reviewer, ProjectManagementMessages.ReviewerLabel },
				};

				public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
			}
		}

		[PXDefault]
		[PXDBString(1, IsFixed = true)]
		[LabelList(typeof(role.Labels))]
		[PXUIField(DisplayName = "Role", Required = true)]
		public virtual string Role
		{
			get;
			set;
		}
		#endregion

		#region Status

		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public const string Planned = "Planned";
			public const string Pending = "Pending";
			public const string Completed = "Completed";
			public const string Canceled = "Canceled";
			public const string Approved = "Approved";
			public const string Rejected = "Rejected";

			public class FullLabels : ILabelProvider
			{
				private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
				{
					{ Planned, ProjectManagementMessages.Planned },
					{ Pending, ProjectManagementMessages.Pending },
					{ Completed, ProjectManagementMessages.Completed },
					{ Approved, ProjectManagementMessages.Approved },
					{ Rejected, ProjectManagementMessages.Rejected },
					{ Canceled, ProjectManagementMessages.Canceled }
				};

				public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
			}

			public class ApproverLabels : ILabelProvider
			{
				private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
				{
					{ Planned, ProjectManagementMessages.Planned },
					{ Pending, ProjectManagementMessages.Pending },
					{ Approved, ProjectManagementMessages.Approved },
					{ Rejected, ProjectManagementMessages.Rejected },
					{ Canceled, ProjectManagementMessages.Canceled }
				};

				public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
			}

			public class SubmitterReviewerLabels : ILabelProvider
			{
				private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
				{
					{ Planned, ProjectManagementMessages.Planned },
					{ Pending, ProjectManagementMessages.Pending },
					{ Completed, ProjectManagementMessages.Completed },
					{ Canceled, ProjectManagementMessages.Canceled }
				};

				public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
			}

		}

		[PXDBString(10)]
		[PXDefault(status.Planned)]
		[LabelList(typeof(status.FullLabels))]
		[PXUIField(DisplayName = "Status")]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get;
			set;
		}
		#endregion

		#region DaysForReview
		public abstract class daysForReview : PX.Data.BQL.BqlDateTime.Field<daysForReview> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Days for Review")]
		public virtual int? DaysForReview
		{
			get;
			set;
		}
		#endregion

		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Due Date")]
		public virtual DateTime? DueDate
		{
			get;
			set;
		}
		#endregion

		#region CompletionDate
		public abstract class completionDate : PX.Data.BQL.BqlDateTime.Field<completionDate> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Completion Date")]
		public virtual DateTime? CompletionDate
		{
			get;
			set;
		}
		#endregion

		#region DateReceived
		public abstract class dateReceived : PX.Data.BQL.BqlDateTime.Field<dateReceived> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Date Received")]
		public virtual DateTime? DateReceived
		{
			get;
			set;
		}
		#endregion

		#region DateSent
		public abstract class dateSent : PX.Data.BQL.BqlDateTime.Field<dateSent> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Date Sent")]
		public virtual DateTime? DateSent
		{
			get;
			set;
		}
		#endregion

		#region CanDelete
		public abstract class canDelete : PX.Data.BQL.BqlBool.Field<canDelete> { }

		[PXBool]
		public virtual bool? CanDelete { get; set; }
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXUIField]
		public virtual bool? Selected { get; set; }
		#endregion

		#region System Columns

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}

		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion

		#endregion
	}
}
