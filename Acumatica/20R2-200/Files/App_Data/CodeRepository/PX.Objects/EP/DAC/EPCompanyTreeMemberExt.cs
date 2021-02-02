using PX.Data;
using PX.Objects.CR;
using PX.TM;

namespace PX.Objects.EP
{
	public sealed class EPCompanyTreeMemberExt : PXCacheExtension<EPCompanyTreeMember>
	{
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(Search<Contact.contactID, Where<Contact.contactType, Equal<ContactTypesAttribute.employee>>>), DescriptionField = typeof(Contact.displayName), SelectorMode = PXSelectorMode.DisplayModeText)]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.SelectorVisible)]
		public int? ContactID { get; set; }
		#endregion
	}
}