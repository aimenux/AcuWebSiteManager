using System;
using PX.Data;
using PX.SM;

namespace PX.Objects.EP
{
	[Serializable]
	[PXSubstitute(GraphType = typeof(EPEventMaint), ParentType = typeof(Users))]
	[PXSubstitute(GraphType = typeof(EPCalendarSync), ParentType = typeof(Users))]
    [PXHidden]
	public partial class EPUsers : Users
	{
		private string _employerEmail;
		private bool _employerEmailLoaded;
		public abstract class employerEmail : PX.Data.BQL.BqlString.Field<employerEmail> { }
		public string EmployerEmail
		{
			[PXDependsOnFields(typeof(pKID),typeof(email))]
			get
			{
				if (!_employerEmailLoaded)
				{
					var set = PXSelectJoin<CR.Contact,
						InnerJoin<EPEmployee, On<CR.Contact.bAccountID, Equal<EPEmployee.parentBAccountID>,
								  And<CR.Contact.contactID, Equal<EPEmployee.defContactID>>>>,
						Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.
							Select(new PXGraph(), this._PKID);
					if (set != null && set.Count > 0) _employerEmail = ((CR.Contact)set[0]).EMail;
					_employerEmailLoaded = true;
				}
				return _employerEmail;
			}
		}

		public override string Email
		{
			[PXDependsOnFields(typeof(employerEmail))]
			get
			{
				return string.IsNullOrEmpty(base.Email) ? EmployerEmail : base.Email;
			}
			set
			{
				base.Email = value;
			}
		}
	}
}
