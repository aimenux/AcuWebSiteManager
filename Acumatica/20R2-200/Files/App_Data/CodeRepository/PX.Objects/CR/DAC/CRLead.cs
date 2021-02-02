using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CR.Standalone;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.MassProcess;
using PX.Objects.CR.Workflows;
using PX.TM;
using System.Diagnostics;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.BQL;
using PX.SM;

namespace PX.Objects.CR
{
	/// <summary>
	/// Represents a marketing lead or a sales lead.
	/// </summary>
	/// <remarks>
	/// A marketing lead is a person or a company that has potential interest in a product your organization offers.
	/// A sales lead is a person or a company that expresses interest in products your organization offers.
	/// The records of this type are created and edited on the <i>Leads (CR.30.10.00)</i> form,
	/// which corresponds to the <see cref="LeadMaint"/> graph.
	/// Note that this class is a projection of the <see cref="Contact"/> and <see cref="Standalone.CRLead"/> classes.
	/// </remarks>
	[Serializable]
	[PXBreakInheritance]
	[PXCacheName(Messages.Lead)]
	[PXProjection(typeof(
		SelectFrom<Contact>
		.InnerJoin<Standalone.CRLead>
		.On<
			Standalone.CRLead.contactID.IsEqual<Contact.contactID>>
		.Where<
			Contact.contactType.IsEqual<ContactTypesAttribute.lead>>), Persistent = true)]
	[CRCacheIndependentPrimaryGraph(
		typeof(LeadMaint),
		typeof(Select<CRLead,
			Where<CRLead.contactID, Equal<Current<CRLead.contactID>>>>))]
	public class CRLead : Contact
	{
        #region Keys
		public new class PK : PrimaryKeyOf<CRLead>.By<contactID>
		{
			public static CRLead Find(PXGraph graph, int? contactID) => FindBy(graph, contactID);
		}
		#endregion

		#region CRLead

		#region ClassID
		public new abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		[PXDBString(10, IsUnicode = true, BqlField = typeof(Standalone.CRLead.classID))]
		[PXUIField(DisplayName = "Lead Class")]
		[PXDefault(typeof(Search<CRSetup.defaultLeadClassID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(CRLeadClass.classID), DescriptionField = typeof(CRLeadClass.description), CacheGlobal = true)]
		[PXMassMergableField]
		[PXMassUpdatableField]
		public override String ClassID { get; set; }
		#endregion

		#endregion



		#region Contact override

		#region ContactID
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXSelector(typeof(
				SelectFrom<CRLead>
				.LeftJoin<BAccount>
					.On<BAccount.bAccountID.IsEqual<CRLead.bAccountID>>
				.Where<
					BAccount.bAccountID.IsNull
					.Or<Match<BAccount, Current<AccessInfo.userName>>>
				>
				.SearchFor<CRLead.contactID>),
			fieldList: new[]
			{
				typeof(CRLead.memberName),
				typeof(CRLead.displayName),
				typeof(CRLead.fullName),
				typeof(CRLead.salutation),
				typeof(CRLead.eMail),
				typeof(CRLead.phone1),
				typeof(CRLead.status),
				typeof(CRLead.duplicateStatus)
			},
			DescriptionField = typeof(CRLead.memberName),
			Filterable = true)]
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Lead ID", Visibility = PXUIVisibility.Invisible)]
		[PXPersonalDataWarning]
		public override Int32? ContactID { get; set; }
		#endregion

		#region ContactType
		public new abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }

		/// <summary>
		/// The type of the lead.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="ContactTypesAttribute"/> class.
		/// The default value is <see cref="ContactTypesAttribute.Lead"/>.
		/// This field must be specified at the initialization stage and not be changed afterwards.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(ContactTypesAttribute.Lead)]
		[ContactTypes]
		[PXUIField(DisplayName = "Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public override String ContactType { get; set; }
		#endregion

		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		/// <inheritdoc cref="Contact.BAccountID"/>
		[PXDBInt]
		[CRContactBAccountDefault]
		[PXFormula(typeof(Default<refContactID>))]
		[PXDefault(typeof(IIf<Where<refContactID, IsNotNull>,
			Selector<refContactID, Contact.bAccountID>,
			bAccountID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<BAccount,
			Where<
				BAccount.bAccountID, Equal<Current<CRLead.bAccountID>>,
				And<BAccount.type, NotEqual<BAccountType.combinedType>>>>))]
		[PXUIField(DisplayName = "Business Account")]
		[PXSelector(typeof(
			SelectFrom<BAccount>
			.LeftJoin<Contact>
				.On<Contact.contactID.IsEqual<BAccount.defContactID>>
			.LeftJoin<Address>
				.On<Address.addressID.IsEqual<BAccount.defAddressID>>
			.SearchFor<BAccount.bAccountID>),
			fieldList: new[]
			{
				typeof(BAccount.bAccountID),
				typeof(BAccount.acctCD),
				typeof(BAccount.acctName),
				typeof(BAccount.type),
				typeof(Contact.phone1),
				typeof(Address.city),
				typeof(Address.state),
				typeof(BAccount.status)
			}, 
			SubstituteKey = typeof(BAccount.acctCD),
			DescriptionField = typeof(BAccount.acctName),
			DirtyRead = true)]
		[PXMassUpdatableField]
		public override Int32? BAccountID { get; set; }
		#endregion

		#region FullName
		public new abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }

		/// <summary>
		/// The name of the company the contact works for.
		/// </summary>
		/// <value>
		/// Either this field or the <see cref="Contact.LastName"/> field must be specified to create the lead.
		/// </value>
		[PXMassMergableField]
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public override String FullName { get; set; }
		#endregion

		#region Source
		public new abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		/// <summary>
		/// The source of the lead.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="CRMSourcesAttribute"/> class.
		/// The value of the field is automatically changed when the <see cref="ClassID"/> property is changed.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source")]
		[CRMSources(BqlTable = typeof(CRLead))]
		[PXMassMergableField]
		[PXFormula(typeof(
			Use<Selector<CRLead.classID, CRLeadClass.defaultSource>>.AsString
				.When<False.IsEqual<Use<IsImport>.AsBool>
					.And<Brackets<EntryStatus.IsEqual<EntryStatus.inserted>.Or<CRLead.source.FromCurrent.IsNull>>>>
			.Else<CRLead.source>
		))]
		public override string Source { get; set; }
		#endregion

		#region OwnerID
		public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		/// <inheritdoc/>
		[Owner(typeof(workgroupID))]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public override int? OwnerID { get; set; }

		#endregion

		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <inheritdoc/>
		[PXSearchable(
			category: SM.SearchCategory.CR,
			titlePrefix: "{0}: {1}",
			titleFields: new []
			{
				typeof(CRLead.contactType),
				typeof(CRLead.displayName)
			},
			fields: new []
			{
				typeof(CRLead.fullName),
				typeof(CRLead.eMail),
				typeof(CRLead.phone1),
				typeof(CRLead.phone2),
				typeof(CRLead.phone3),
				typeof(CRLead.webSite)
			},
			WhereConstraint = typeof(Where<
				CRLead.contactType
					.IsNotIn<
						ContactTypesAttribute.bAccountProperty,
						ContactTypesAttribute.employee>>),
			MatchWithJoin = typeof(LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRLead.bAccountID>>>),
			Line1Format = "{0}{1}{2}{3}",
			Line1Fields = new []
			{
				typeof(CRLead.fullName),
				typeof(CRLead.salutation),
				typeof(CRLead.phone1),
				typeof(CRLead.eMail)
			},
			Line2Format = "{1}{2}{3}",
			Line2Fields = new []
			{
				typeof(CRLead.defAddressID),
				typeof(Address.displayName),
				typeof(Address.city),
				typeof(Address.state),
				typeof(Address.countryID)
			})]
		[PXUniqueNote(
			DescriptionField = typeof(CRLead.displayName),
			Selector = typeof(CRLead.contactID),
			ShowInReferenceSelector = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public override Guid? NoteID { get; set; }
		#endregion

		public new abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		public new abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		public new abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }
		public new abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }
		public new abstract class memberName : PX.Data.BQL.BqlString.Field<memberName> { }
		public new abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }
		public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		public new abstract class duplicateFound : PX.Data.BQL.BqlBool.Field<duplicateFound> { }
		public new abstract class duplicateStatus : PX.Data.BQL.BqlString.Field<duplicateStatus> { }
		public new abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }
		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		public new abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }
		#endregion



		#region CRLead

		#region LeadContactID
		public abstract class leadContactID : PX.Data.BQL.BqlInt.Field<leadContactID> { }

		/// <summary>
		/// This field is required for the proper work of <see cref="PXProjectionAttribute"/>,
		/// because the <see cref="ContactID"/> field is a key there must be another field
		/// that is marked with <see cref="PXExtraKeyAttribute"/>.
		/// </summary>
		/// <value>
		/// Always returns <see cref="ContactID"/>.
		/// </value>
		[PXExtraKey()]
		[PXDBInt(BqlField = typeof(Standalone.CRLead.contactID))]
		[PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? LeadContactID
		{
			get
			{
				return this.ContactID;
			}
		}
		#endregion

		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <inheritdoc cref="Standalone.CRLead.Status"/>
		[PXDBString(1, IsFixed = true, BqlField = typeof(Standalone.CRLead.status))]
		[PXDefault]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[LeadWorkflow.States.List(BqlTable = typeof(CRLead))]
		[PXUIEnabled(typeof(Where<EntryStatus, NotEqual<EntryStatus.inserted>, Or<IsImport, Equal<True>>>))]
		public override String Status { get; set; }
		#endregion

		#region Resolution
		public new abstract class resolution : PX.Data.BQL.BqlString.Field<resolution> { }

		/// <inheritdoc cref="Standalone.CRLead.Resolution"/>
		[PXDBString(2, IsFixed = true, BqlField = typeof(Standalone.CRLead.resolution))]
		[PXUIField(DisplayName = "Reason", Visibility = PXUIVisibility.SelectorVisible)]
		[PXStringList(new string[0], new string[0], BqlTable = typeof(CRLead))]
		public override String Resolution { get; set; }
		#endregion

		#region RefContactID
		public abstract class refContactID : PX.Data.BQL.BqlInt.Field<refContactID> { }

		/// <inheritdoc cref="Standalone.CRLead.RefContactID"/>
		[PXDBInt(BqlField = typeof(Standalone.CRLead.refContactID))]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(SearchFor<ContactAccount.contactID>.In<
				SelectFrom<ContactAccount>
				.Where<
					ContactAccount.contactType.IsEqual<ContactTypesAttribute.person>
					.And<Brackets<
						CRLead.bAccountID.FromCurrent.IsNull
						.Or<CRLead.bAccountID.FromCurrent.IsEqual<ContactAccount.bAccountID>>>>>>),
			fieldList: new[]
			{
				typeof(ContactAccount.memberName),
				typeof(ContactAccount.displayName),
				typeof(ContactAccount.fullName),
				typeof(ContactAccount.salutation),
				typeof(ContactAccount.eMail),
				typeof(ContactAccount.phone1),
				typeof(ContactAccount.isActive)
			},
			DescriptionField = typeof(Contact.displayName), 
			Filterable = true, 
			DirtyRead = true)]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		public virtual Int32? RefContactID { get; set; }
		#endregion

		#region OverrideRefContact
		public abstract class overrideRefContact : PX.Data.BQL.BqlBool.Field<overrideRefContact> { }

		/// <inheritdoc cref="Standalone.CRLead.OverrideRefContact"/>
		[PXDBBool(BqlField = typeof(Standalone.CRLead.overrideRefContact))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Null)]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideRefContact { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <inheritdoc cref="Standalone.CRLead.Description"/>
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Standalone.CRLead.description))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description { get; set; }
		#endregion

		#region LeadNoteID
		public abstract class leadNoteID : PX.Data.BQL.BqlInt.Field<leadNoteID> { }

		/// <summary>
		/// The note identifier field of the <see cref="Standalone.CRLead"/> table
		/// (unlike the <see cref="NoteID"/> field of the <see cref="Contact"/> table).
		/// </summary>
		/// <value>
		/// Always returns <see cref="NoteID"/>.
		/// </value>
		[PXDBGuid(BqlField = typeof(Standalone.CRLead.noteID))]
		public virtual Guid? LeadNoteID
		{
			get
			{
				return this.NoteID;
			}
		}
		#endregion

		#region QualificationDate
		public abstract class qualificationDate : PX.Data.BQL.BqlDateTime.Field<qualificationDate> { }

		/// <inheritdoc cref="Standalone.CRLead.QualificationDate"/>
		[PXDBDate(PreserveTime = true, BqlField = typeof(Standalone.CRLead.qualificationDate))]
		[PXUIField(DisplayName = "Qualification Date")]
		public virtual DateTime? QualificationDate { get; set; }
		#endregion

		#region ConvertedBy
		public abstract class convertedBy : PX.Data.BQL.BqlGuid.Field<convertedBy> { }

		/// <exclude/>
		[PXDBGuid(BqlField = typeof(Standalone.CRLead.convertedBy))]
		[PXSelector(typeof(Users.pKID), SubstituteKey = typeof(Users.username), DescriptionField = typeof(Users.fullName), CacheGlobal = true, DirtyRead = true, ValidateValue = false)]
		[PXUIField(DisplayName = "Converted By")]
		public virtual Guid? ConvertedBy { get; set; }
		#endregion

		#region LeadCreatedByID
		public abstract class leadCreatedByID : PX.Data.BQL.BqlGuid.Field<leadCreatedByID> { }

		/// <exclude/>
		[PXDBCreatedByID(BqlField = typeof(Standalone.CRLead.createdByID))]
		public virtual Guid? LeadCreatedByID { get; set; }
		#endregion

		#region LeadCreatedByScreenID
		public abstract class leadCreatedByScreenID : PX.Data.BQL.BqlString.Field<leadCreatedByScreenID> { }

		/// <exclude/>
		[PXDBCreatedByScreenID(BqlField = typeof(Standalone.CRLead.createdByScreenID))]
		public virtual String LeadCreatedByScreenID { get; set; }
		#endregion

		#region LeadCreatedDateTime
		public abstract class leadCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<leadCreatedDateTime> { }

		/// <exclude/>
		[PXDBCreatedDateTime(BqlField = typeof(Standalone.CRLead.createdDateTime))]
		public virtual DateTime? LeadCreatedDateTime { get; set; }
		#endregion

		#region LeadLastModifiedByID
		public abstract class leadLastModifiedByID : PX.Data.BQL.BqlGuid.Field<leadLastModifiedByID> { }

		/// <exclude/>
		[PXDBLastModifiedByID(BqlField = typeof(Standalone.CRLead.lastModifiedByID))]
		public virtual Guid? LeadLastModifiedByID { get; set; }
		#endregion

		#region LeadLastModifiedByScreenID
		public abstract class leadLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<leadLastModifiedByScreenID> { }

		/// <exclude/>
		[PXDBLastModifiedByScreenID(BqlField = typeof(Standalone.CRLead.lastModifiedByScreenID))]
		public virtual String LeadLastModifiedByScreenID { get; set; }
		#endregion

		#region LeadLastModifiedDateTime
		public abstract class leadLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<leadLastModifiedDateTime> { }

		[PXDBLastModifiedDateTime(BqlField = typeof(Standalone.CRLead.lastModifiedDateTime))]
		public virtual DateTime? LeadLastModifiedDateTime { get; set; }
		#endregion

		#endregion


		#region Attributes
		public new abstract class attributes : BqlAttributes.Field<attributes> { }

		/// <summary>
		/// The attributes available for the current contact.
		/// The field is preserved for internal use.
		/// </summary>
		[CRAttributesField(typeof(CRLead.classID))]
		public override string[] Attributes { get; set; }
		#endregion
	}
}
