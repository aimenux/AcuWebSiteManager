using System;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.SM
{
	[Serializable]
	[PXSubstitute(GraphType = typeof(MyProfileMaint))]
	public class Users : PX.SM.Users
	{
		[PXDBString(50)]
		[PXUIField(DisplayName = "Phone", Visibility = PXUIVisibility.SelectorVisible)]
		[PhoneValidation]
		public override string Phone
		{
			get
			{
				return base.Phone;
			}
			set
			{
				base.Phone = value;
			}
		}
	}
}
