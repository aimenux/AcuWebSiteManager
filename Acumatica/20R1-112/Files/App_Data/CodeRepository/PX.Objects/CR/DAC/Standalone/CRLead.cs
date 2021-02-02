using System;
using PX.Data;
using PX.Objects.CR.Workflows;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR.Standalone
{
	/// <exclude/>
	[PXCacheName(Messages.Lead)]
	[PXPrimaryGraph(typeof(LeadMaint))]
	[Serializable]
	public class CRLead : IBqlTable
	{
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt(IsKey = true, BqlField = typeof(contactID))]
		[PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
		[PXDBChildIdentity(typeof(CRLead.contactID))]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[LeadWorkflow.States.List]
		[PXUIEnabled(typeof(Where<EntryStatus, NotEqual<EntryStatus.inserted>>))]
		public virtual String Status { get; set; }
		#endregion

		#region Resolution
		public abstract class resolution : PX.Data.BQL.BqlString.Field<resolution> { }

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Reason", Visibility = PXUIVisibility.SelectorVisible)]
		[PXStringList(new string[0], new string[0], BqlTable = typeof(CRLead))]
		public virtual String Resolution { get; set; }
		#endregion

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Lead Class")]
		[PXDefault(typeof(Search<CRSetup.defaultLeadClassID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(CRLeadClass.classID), DescriptionField = typeof(CRLeadClass.description), CacheGlobal = true)]
		public virtual String ClassID { get; set; }
		#endregion

		#region RefContactID
		public abstract class refContactID : PX.Data.BQL.BqlInt.Field<refContactID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(Search<
				ContactAccount.contactID,
				Where<
					ContactAccount.contactType, Equal<ContactTypesAttribute.person>,
					And<WhereEqualNotNull<ContactAccount.bAccountID, Contact.bAccountID>>>>),
			DescriptionField = typeof(Contact.displayName), Filterable = true, DirtyRead = true)]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		public virtual Int32? RefContactID { get; set; }
		#endregion

		#region OverrideRefContact
		public abstract class overrideRefContact : PX.Data.BQL.BqlBool.Field<overrideRefContact> { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideRefContact { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(
			DescriptionField = typeof(contactID),
			Selector = typeof(contactID))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region Attributes
		public abstract class attributes : BqlAttributes.Field<attributes> { }

		[CRAttributesField(typeof(classID))]
		public virtual string[] Attributes { get; set; }
		#endregion

		#region QualificationDate
		public abstract class qualificationDate : PX.Data.BQL.BqlDateTime.Field<qualificationDate> { }

		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Qualification Date")]
		public virtual DateTime? QualificationDate { get; set; }
		#endregion

		#region ConvertedBy
		public abstract class convertedBy : PX.Data.BQL.BqlGuid.Field<convertedBy> { }

		[PXDBGuid]
		[PXSelector(typeof(Users.pKID), SubstituteKey = typeof(Users.username), DescriptionField = typeof(Users.fullName), CacheGlobal = true, DirtyRead = true, ValidateValue = false)]
		[PXUIField(DisplayName = "Converted By")]
		public virtual Guid? ConvertedBy { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
	}
}
