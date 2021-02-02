namespace PX.Objects.CR
{
	using System;
	using PX.Data;

	[System.SerializableAttribute()]
    [PXHidden]
	public partial class CRCaseContacts : PX.Data.IBqlTable
	{
		#region CaseID
		public abstract class caseID : PX.Data.BQL.BqlInt.Field<caseID> { }
		protected Int32? _CaseID;
		[PXDBLiteDefault(typeof(CRCase.caseID))]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Case ID")]
		[PXSelector(typeof(CRCase.caseID))]
		[PXParent(typeof(Select<CRCase, Where<CRCase.caseID, Equal<Current<CRCaseContacts.caseID>>>>))]
		public virtual Int32? CaseID
		{
			get
			{
				return this._CaseID;
			}
			set
			{
				this._CaseID = value;
			}
		}
		#endregion
		#region EMail
		public abstract class eMail : PX.Data.BQL.BqlString.Field<eMail> { }
		protected String _EMail;
		[PXDBEmail(IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault()]
		//[PXParentSearch()]
		public virtual String EMail
		{
			get
			{
				return this._EMail;
			}
			set
			{
				this._EMail = value;
			}
		}
		#endregion
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		protected Int32? _ContactID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(Search<Contact.contactID, Where<Contact.contactType, Equal<ContactTypesAttribute.person>, 
		    And<Where<Contact.eMail, Equal<Optional<CRCaseContacts.eMail>>,
			   Or<Optional<CRCaseContacts.eMail>, IsNull>>>>>))]
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
		#region WatchTypeID
		public abstract class watchTypeID : PX.Data.BQL.BqlString.Field<watchTypeID> { }
		protected String _WatchTypeID;
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Watch Type")]
		[PXStringList(new string[] { "A", "N", "O", "C" }, new string[] { "All", "New", "Opened", "Closed" })]
		public virtual String WatchTypeID
		{
			get
			{
				return this._WatchTypeID;
			}
			set
			{
				this._WatchTypeID = value;
			}
		}
		#endregion		
	}
}
