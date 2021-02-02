using PX.Objects.PJ.Common.Descriptor.Attributes;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CN.Common.DAC;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.TM;
using System;
using System.Collections.Generic;
using PX.Objects.PJ.Submittals.PJ.Descriptor;

namespace PX.Objects.PJ.Submittals.PJ.DAC
{
	[Serializable]
	[PXPrimaryGraph(typeof(Graphs.SubmittalEntry))]
	[PXCacheName("Submittal")]
	public class PJSubmittal : BaseCache,
		IBqlTable,
		IProjectManagementDocumentBase,
		IAssign
	{
		#region SubmittalID
		public abstract class submittalID : PX.Data.BQL.BqlString.Field<submittalID>
		{
		}

		[PXFieldDescription]
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[SubmittalIDOfLastRevisionSelector(ValidateValue = false)]
		[PXUIField(DisplayName = "Submittal ID", 
			Visibility = PXUIVisibility.SelectorVisible,
			Required = true)]
		[PXDefault]
		[SubmittalAutoNumber]
		public virtual string SubmittalID
		{
			get;
			set;
		}
		#endregion

		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Revision ID", 
			Visibility = PXUIVisibility.SelectorVisible,
			Required = true)]
		[PXFieldDescription]
		[SubmittalRevisionIDSelector(typeof(submittalID))]
		public virtual int? RevisionID
		{
			get;
			set;
		}
		#endregion

		#region TypeID

		public abstract class typeID : PX.Data.BQL.BqlInt.Field<typeID>
		{
		}

		[PXDBInt]
		[PXUIField(DisplayName = "Submittal Type")]
		[PXSelector(typeof(SearchFor<PJSubmittalType.submittalTypeID>),
			SubstituteKey = typeof(PJSubmittalType.typeName))]
		[PXForeignReference(typeof(Field<typeID>.IsRelatedTo<PJSubmittalType.submittalTypeID>))]
		public virtual int? TypeID
		{
			get;
			set;
		}
		#endregion

		#region Summary
		public abstract class summary : PX.Data.BQL.BqlString.Field<summary>
		{
		}

		[PXFieldDescription]
		[PXDBString(255, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", 
			Visibility = PXUIVisibility.SelectorVisible,
			Required = true)]
		public virtual string Summary
		{
			get;
			set;
		}
		#endregion

		#region Status

		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public const string 
				New = "N",
				Open = "O",
				Closed = "C";

			public class Labels : ILabelProvider
			{
				private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
				{
					{ New, ProjectManagementMessages.New },
					{ Open, ProjectManagementMessages.Open },
					{ Closed, ProjectManagementMessages.Closed },
				};

				public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;
			}
		}

		[PXDBString(1, IsFixed = true)]
		[PXDefault(status.New)]
		[PXUIField(DisplayName = "Status", Required = true)]
		[LabelList(typeof(status.Labels))]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region Reason
		public abstract class reason : PX.Data.BQL.BqlString.Field<reason> 
		{
			public const string
				New = "New",
				Revision = "Revision",
				Issued = "Issued",
				Submitted = "Submitted",
				PendingApproval = "PendingApproval",
				Approved = "Approved",
				ApprovedAsNoted = "ApprovedAsNoted",
				Rejected = "Rejected",
				Canceled = "Canceled",
				ReviseAndResubmit = "ReviseAndResubmit";
		}

		[PXDBString(20)]
		[PXDefault(reason.New)]
		[PXStringList(new string[0], new string[0])]
		[PXUIField(DisplayName = "Reason", Required = false)]
		public virtual string Reason
		{
			get;
			set;
		}
		#endregion

		#region IsLastRevision
		public abstract class isLastRevision : PX.Data.BQL.BqlBool.Field<isLastRevision>
		{
		}

		[PXDBBool]
		[PXDefault(true)]
		public virtual bool? IsLastRevision
		{
			get;
			set;
		}
		#endregion

		#region ProjectID
		public abstract class projectId : PX.Data.BQL.BqlInt.Field<projectId>
		{
		}

		[PXDefault]
		[PXUIField(DisplayName = "Project")]
		[ProjectBase]
		[PXRestrictor(typeof(Where<PMProject.nonProject, Equal<False>>), PM.Messages.NonProjectCodeIsInvalid)]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.ProjectIsCanceled)]
		public virtual int? ProjectId
		{
			get;
			set;
		}
		#endregion

		#region ProjectTaskID
		public abstract class projectTaskId : PX.Data.BQL.BqlInt.Field<projectTaskId>
		{
		}

		[PXUIField(DisplayName = "Project Task")]
		[ProjectTask(typeof(projectId), AllowNullIfContract = true, AllowNull = true)]
		public virtual int? ProjectTaskId
		{
			get;
			set;
		}
		#endregion

		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}

		[CostCode(null, 
			typeof(projectTaskId), 
			DisplayName = "Cost Code",
			AllowNullValue = true)]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion

		#region SpecificationInfo
		public abstract class specificationInfo : PX.Data.BQL.BqlString.Field<specificationInfo>
		{
		}

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Specification")]
		public virtual string SpecificationInfo
		{
			get;
			set;
		}
		#endregion

		#region SpecificationSection
		public abstract class specificationSection : PX.Data.BQL.BqlString.Field<specificationSection> { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Spec. Section")]
		public virtual string SpecificationSection
		{
			get;
			set;
		}
		#endregion

		#region DateOnSite
		public abstract class dateOnSite : PX.Data.BQL.BqlDateTime.Field<dateOnSite> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Date Required on Site")]
		public virtual DateTime? DateOnSite
		{
			get;
			set;
		}
		#endregion

		#region DateCreated
		public abstract class dateCreated : PX.Data.BQL.BqlDateTime.Field<dateCreated> { }

		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date Created")]
		public virtual DateTime? DateCreated
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

		#region DateClosed
		public abstract class dateClosed : PX.Data.BQL.BqlDateTime.Field<dateClosed> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Date Closed", Enabled = false)]
		public virtual DateTime? DateClosed
		{
			get;
			set;
		}
		#endregion

		#region DaysOverdue
		public abstract class daysOverdue : PX.Data.BQL.BqlInt.Field<daysOverdue> { }

		[PXInt]
		[PXUIField(DisplayName = "Days Overdue", Enabled = false)]
		[DaysOverdue(typeof(dueDate), typeof(dateClosed))]
		public virtual int? DaysOverdue
		{
			[PXDependsOnFields(typeof(dueDate), typeof(dateClosed))]
			get;
			set;
		}
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		[Owner]
		[PXDefault(typeof(AccessInfo.contactID))]
		public virtual int? OwnerID
		{
			get;
			set;
		}
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlGuid.Field<workgroupID> { }

		[PXInt]
		public virtual int? WorkgroupID
		{
			get;
			set;
		}
		#endregion

		#region CurrentWorkflowItemContact

		public abstract class currentWorkflowItemContactID : PX.Data.BQL.BqlInt.Field<currentWorkflowItemContactID> { }
		[PXDBInt]
		[PXDefault]
		[PXSelector(typeof(Search<Contact.contactID>), DescriptionField = typeof(Contact.displayName))]
		[PXUIField(DisplayName = SubmittalMessage.BallInCourt, Enabled = false)]
		public virtual int? CurrentWorkflowItemContactID
		{
			get;
			set;
		}
		#endregion

		#region CurrentWorkflowItemLineNbr

		public abstract class currentWorkflowItemLineNbr : PX.Data.BQL.BqlInt.Field<currentWorkflowItemLineNbr> { }
		[PXDBInt]
		public virtual int? CurrentWorkflowItemLineNbr
		{
			get;
			set;
		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description
		{
			get;
			set;
		}
		#endregion

		#region System Columns

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[SubmittalSearchable]
		[PXNote(
			DescriptionField = typeof(submittalID),
			ShowInReferenceSelector = true,
			Selector = typeof(Search<submittalID>),
			FieldList = new[]
			{
				typeof(submittalID),
				typeof(summary),
				typeof(status)
			})]
		public override Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }

		[PXDBTimestamp()]
		public override Byte[] Tstamp
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdById : PX.Data.BQL.BqlGuid.Field<createdById> { }

		[PXDBCreatedByID]
		public override Guid? CreatedById
		{
			get;
			set;
		}
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenId : PX.Data.BQL.BqlString.Field<createdByScreenId> { }

		[PXDBCreatedByScreenID()]
		public override String CreatedByScreenId
		{
			get;
			set;
		}
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public override DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedById : PX.Data.BQL.BqlGuid.Field<lastModifiedById> { }

		[PXDBLastModifiedByID]
		public override Guid? LastModifiedById
		{
			get;
			set;
		}
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenId : PX.Data.BQL.BqlString.Field<lastModifiedByScreenId> { }

		[PXDBLastModifiedByScreenID()]
		public override String LastModifiedByScreenId
		{
			get;
			set;
		}

		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public override DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion

		#endregion
	}
}
