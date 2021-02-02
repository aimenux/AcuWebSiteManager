using PX.Data;
using System;
using PX.Objects.CR;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.PJ.RequestsForInformation.PM.DAC;

namespace PX.Objects.PJ.Common.DAC
{
	[Serializable]
	[PXPrimaryGraph(new Type[] { typeof(ContactMaint) },
		new Type[] { typeof(Select<Contact, 
			Where<Contact.contactID, Equal<Current<ContactForCurrentProject.contactID>>>>) })]

	[PXProjection(typeof(
	Select2<Contact,
		LeftJoin<ProjectContact,
			On<ProjectContact.contactId, Equal<Contact.contactID>,
		And<ProjectContact.projectId, Equal<CurrentValue<CurrentProject.projectID>>>>>>))]
	public class ContactForCurrentProject : IBqlTable
	{
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt(IsKey = true, BqlField = typeof(Contact.contactID))]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region DisplayName
		public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }

		[PXDBString(BqlField = typeof(Contact.displayName))]
		[PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String DisplayName { get; set; }
		#endregion

		#region JobTitle
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }

		[PXDBString(BqlField = typeof(Contact.salutation))]
		[PXUIField(DisplayName = "Job Title", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Salutation { get; set; }
		#endregion

		#region CompanyName
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }

		[PXDBString(BqlField = typeof(Contact.fullName))]
		[PXUIField(DisplayName = "Company Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual String FullName { get; set; }
		#endregion

		#region EMail
		public abstract class eMail : PX.Data.BQL.BqlString.Field<eMail> { }

		[PXDBString(BqlField = typeof(Contact.eMail))]
		[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String EMail { get; set; }
		#endregion

		#region Phone1
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }

		[PXDBString(BqlField = typeof(Contact.phone1))]
		[PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Phone1 { get; set; }
		#endregion

		#region ContactType
		public abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }

		[PXDBString(BqlField = typeof(Contact.contactType))]
		[ContactTypes]
		[PXUIField(DisplayName = "Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ContactType { get; set; }
		#endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool(BqlField = typeof(Contact.isActive))]
		public virtual bool? IsActive { get; set; }
		#endregion

		#region ProjectId
		public abstract class projectId : PX.Data.BQL.BqlInt.Field<projectId>
		{
		}

		[PXDBInt(BqlField = typeof(ProjectContact.projectId))]
		public int? ProjectId
		{
			get;
			set;
		}

		#endregion

		#region IsRelatedToProject
		public abstract class isRelatedToProject : IBqlField { }
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ContactForCurrentProject.projectId, IsNotNull>, True>, False>), typeof(bool))]
		[PXUIField(DisplayName = "Is Related To Project Contact", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public bool? IsRelatedToProject { get; set; }
		#endregion
	}
}
