using System.Collections.Generic;
using PX.Common;
using PX.Data.EP;
using System;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.TM;
using PX.Objects.TX;
using PX.Objects.AR;
using PX.Objects.PM;
using PX.Objects.GL;
using PX.Objects.CR.Workflows;


namespace PX.Objects.CR.Standalone
{
	/// <exclude/>
	public partial class CROpportunity : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected { get; set; }
		#endregion

		#region OpportunityID
		public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }

		public const int OpportunityIDLength = 10;

		[PXDBString(OpportunityIDLength, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Opportunity ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(CRSetup.opportunityNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search3<CROpportunity.opportunityID,
				OrderBy<Desc<CROpportunity.opportunityID>>>),
			new[] { typeof(CROpportunity.opportunityID),
				typeof(CROpportunity.status),
				typeof(CROpportunity.closeDate),
				typeof(CROpportunity.stageID),
				typeof(CROpportunity.classID),
				},
			Filterable = true)]
		[PXFieldDescription]
		public virtual String OpportunityID { get; set; }
		#endregion

		#region leadID
		public abstract class leadID : PX.Data.BQL.BqlGuid.Field<leadID> { }
		[PXDBGuid]
		[PXUIField(DisplayName = "Source Lead", Enabled = false)]
		[PXSelector(typeof(
			Search<Contact.noteID,
				Where<Contact.contactType.IsEqual<ContactTypesAttribute.lead>>>),
			DescriptionField = typeof(Contact.displayName))]
		public virtual Guid? LeadID { get; set; }
		#endregion

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Class ID")]
		[PXDefault]
		[PXSelector(typeof(CROpportunityClass.cROpportunityClassID),
			DescriptionField = typeof(CROpportunityClass.description), CacheGlobal = true)]
		[PXMassUpdatableField]
		public virtual String ClassID { get; set; }
		#endregion

		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }
		[PXDBString(255, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Subject { get; set; }
		#endregion

		#region Details
		public abstract class details : PX.Data.BQL.BqlString.Field<details> { }

		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Details")]
		public virtual String Details { get; set; }
		#endregion

		#region CloseDate
		public abstract class closeDate : PX.Data.BQL.BqlDateTime.Field<closeDate> { }

		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXMassUpdatableField]
		[PXUIField(DisplayName = "Estimation", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? CloseDate { get; set; }
		#endregion

		#region StageChangedDate
		public abstract class stageChangedDate : PX.Data.BQL.BqlDateTime.Field<stageChangedDate> { }

		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Stage Change Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? StageChangedDate { get; set; }
		#endregion

		#region StageID
		public abstract class stageID : PX.Data.BQL.BqlString.Field<stageID> { }

		[PXDBString(2)]
		[PXUIField(DisplayName = "Stage")]
		[CROpportunityStages(typeof(classID), typeof(stageChangedDate), OnlyActiveStages = true)]
		[PXDefault]
		[PXMassUpdatableField]
		public virtual String StageID { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[OpportunityWorkflow.States.List]
		[PXDefault]
		public virtual string Status { get; set; }

		#endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion

		#region Resolution
		public abstract class resolution : PX.Data.BQL.BqlString.Field<resolution> { }

		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[0], new string[0])]
		[PXUIField(DisplayName = "Reason")]
		[PXMassUpdatableField]
		public virtual String Resolution { get; set; }
		#endregion

		#region AssignDate
		public abstract class assignDate : PX.Data.BQL.BqlDateTime.Field<assignDate> { }

		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Assignment Date")]
		public virtual DateTime? AssignDate { get; set; }
		#endregion

		#region ClosingDate
		public abstract class closingDate : PX.Data.BQL.BqlDateTime.Field<closingDate> { }

		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Closing Date")]
		public virtual DateTime? ClosingDate { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(
			DescriptionField = typeof(CROpportunity.opportunityID),
			Selector = typeof(CROpportunity.opportunityID)
			)]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.Visible, Visible = true)]
		[PXMassUpdatableField]
		[CRMSources]
		public virtual string Source { get; set; }
		#endregion

		#region ExternalRef
		public abstract class externalRef : PX.Data.BQL.BqlString.Field<externalRef> { }

		[PXDBString(255, IsFixed = true)]
		[PXUIField(DisplayName = "External Ref.")]
		public virtual string ExternalRef { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		[PXUIField(DisplayName = "Created By")]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = "Date Created", Enabled = false)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		[PXUIField(DisplayName = "Last Modified By")]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = "Last Modified Date", Enabled = false)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion

		#region DefQuoteID
		public abstract class defQuoteID : PX.Data.BQL.BqlGuid.Field<defQuoteID> { }

		[PXDBGuid]
		[PXDefault()]
		public virtual Guid? DefQuoteID { get; set; }
		#endregion
	}
}
