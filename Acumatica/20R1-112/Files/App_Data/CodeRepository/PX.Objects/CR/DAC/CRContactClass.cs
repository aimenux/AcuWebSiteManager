using System;
using PX.Data;
using PX.Objects.EP;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR
{
	/// <exclude/>
	[PXCacheName(Messages.LeadClass)]
	[PXPrimaryGraph(typeof(CRContactClassMaint))]
	[Serializable]
	public class CRContactClass : CRBaseClass, IBqlTable, ITargetToLead, ITargetToAccount, ITargetToOpportunity
	{
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		[PXSelector(typeof(CRContactClass.classID))]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public virtual string ClassID { get; set; }
		#endregion

		#region IsInternal

		public abstract class isInternal : PX.Data.BQL.BqlBool.Field<isInternal> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Internal", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? IsInternal { get; set; }

		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(250, IsUnicode = true)]
		public virtual string Description { get; set; }
		#endregion

		#region DefaultOwner
		public abstract class defaultOwner : PX.Data.BQL.BqlInt.Field<defaultOwner> { }

		[PXDBString]
		[PXUIField(DisplayName = "Default Owner")]
		[CRDefaultOwner]
		public override string DefaultOwner { get; set; }
		#endregion

		#region DefaultAssignmentMapID
		public abstract class defaultAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultAssignmentMapID> { }

		[PXDBInt]
		[PXSelector(typeof(Search<
				EPAssignmentMap.assignmentMapID,
			Where<
				EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeContact>>>),
			typeof(EPAssignmentMap.assignmentMapID),
			typeof(EPAssignmentMap.name),
			DescriptionField = typeof(EPAssignmentMap.name)
			)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<defaultOwner, Equal<CRDefaultOwnerAttribute.assignmentMap>>))]
		[PXUIEnabled(typeof(Where<defaultOwner, Equal<CRDefaultOwnerAttribute.assignmentMap>>))]
		[PXUIField(DisplayName = "Assignment Map")]
		public override int? DefaultAssignmentMapID { get; set; }
		#endregion

		#region TargetLeadClassID
		public abstract class targetLeadClassID : PX.Data.BQL.BqlString.Field<targetLeadClassID> { }

		[PXSelector(typeof(CRLeadClass.classID))]
		[PXUIField(DisplayName = "Lead Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true)]
		public virtual string TargetLeadClassID { get; set; }
		#endregion

		#region TargetBAccountClassID
		public abstract class targetBAccountClassID : PX.Data.BQL.BqlString.Field<targetBAccountClassID> { }

		[PXSelector(typeof(CRCustomerClass.cRCustomerClassID))]
		[PXUIField(DisplayName = "Account Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true)]
		public virtual string TargetBAccountClassID { get; set; }
		#endregion

		#region TargetOpportunityClassID
		public abstract class targetOpportunityClassID : PX.Data.BQL.BqlString.Field<targetOpportunityClassID> { }

		[PXSelector(typeof(CROpportunityClass.cROpportunityClassID))]
		[PXUIField(DisplayName = "Opportunity Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBString(10, IsUnicode = true)]
		public virtual string TargetOpportunityClassID { get; set; }
		#endregion

		#region TargetOpportunityStage
		public abstract class targetOpportunityStage : PX.Data.BQL.BqlString.Field<targetOpportunityStage> { }

		[PXDBString(2)]
		[PXUIField(DisplayName = "Opportunity Stage")]
		[CROpportunityStages(typeof(targetOpportunityClassID), OnlyActiveStages = true)]
		public virtual string TargetOpportunityStage { get; set; }
		#endregion

		#region DefaultEMailAccount
		public abstract class defaultEMailAccountID : PX.Data.BQL.BqlInt.Field<defaultEMailAccountID> { }

		[PXSelector(typeof(EMailAccount.emailAccountID), typeof(EMailAccount.description), DescriptionField = typeof(EMailAccount.description))]
		[PXUIField(DisplayName = "Default Email Account")]
		[PXDBInt]
		public virtual int? DefaultEMailAccountID { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
