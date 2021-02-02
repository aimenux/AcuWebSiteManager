namespace PX.Objects.CR
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
    [PXHidden]
	public partial class CROpportunityContacts : PX.Data.IBqlTable
	{
		#region OpportunityID
		public abstract class opportunityID : PX.Data.BQL.BqlString.Field<opportunityID> { }
		protected String _OpportunityID;
		[PXDBLiteDefault(typeof(CROpportunity.opportunityID))]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Opportunity ID")]
		[PXParent(typeof(Select<CROpportunity, Where<CROpportunity.opportunityID, Equal<Current<CROpportunityContacts.opportunityID>>>>))]
		public virtual String OpportunityID
		{
			get
			{
				return this._OpportunityID;
			}
			set
			{
				this._OpportunityID = value;
			}
		}
		#endregion
		#region ContactRoleID
		public abstract class contactRoleID : PX.Data.BQL.BqlString.Field<contactRoleID> { }
		protected String _ContactRoleID;
		[PXDBString(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Role")]
		[PXStringListAttribute]
		public virtual String ContactRoleID
		{
			get
			{
				return this._ContactRoleID;
			}
			set
			{
				this._ContactRoleID = value;
			}
		}
		#endregion
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		protected Int32? _ContactID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(Search<Contact.contactID,
			Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
				And<Contact.bAccountID, Equal<Optional<CROpportunity.bAccountID>>>>>),
			DescriptionField = typeof(Contact.displayName))]
		public virtual Int32? ContactID
		{
			get
			{
				return this._ContactID;
			}
			set
			{
				this._ContactID = value;
			}
		}
		#endregion
		#region Watcher
		public abstract class watcher : PX.Data.BQL.BqlBool.Field<watcher> { }
		protected Boolean? _Watcher;
		[PXDBBool()]
		[PXUIField(DisplayName = "Email Watcher", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? Watcher
		{
			get
			{
				return this._Watcher;
			}
			set
			{
				this._Watcher = value;
			}
		}
		#endregion
	}
}
