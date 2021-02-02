using System;
using PX.Data;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[PXHidden]
	public partial class DocumentContact : PXMappedCacheExtension
	{
		#region FullName
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
		public virtual String FullName { get; set; }
		#endregion

		#region Title
		public abstract class title : PX.Data.BQL.BqlString.Field<title> { }
		public virtual String Title { get; set; }
		#endregion

		#region FirstName
		public abstract class firstName : PX.Data.BQL.BqlString.Field<firstName> { }
		public virtual String FirstName { get; set; }
		#endregion

		#region LastName
		public abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }
		public virtual String LastName { get; set; }
		#endregion

		#region Salutation
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }
		public virtual String Salutation { get; set; }
		#endregion

		#region Attention
		public abstract class attention : PX.Data.BQL.BqlString.Field<attention> { }
		public virtual String Attention { get; set; }
		#endregion

		#region Email
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		public virtual String Email { get; set; }
		#endregion

		#region WebSite
		public abstract class webSite : PX.Data.BQL.BqlString.Field<webSite> { }
		public virtual String WebSite { get; set; }
		#endregion

		#region Phone1
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }
		public virtual String Phone1 { get; set; }
		#endregion

		#region Phone1Type
		public abstract class phone1Type : PX.Data.BQL.BqlString.Field<phone1Type> { }
		public virtual String Phone1Type { get; set; }
		#endregion

		#region Phone2
		public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }
		public virtual String Phone2 { get; set; }
		#endregion

		#region Phone2Type
		public abstract class phone2Type : PX.Data.BQL.BqlString.Field<phone2Type> { }
		public virtual String Phone2Type { get; set; }
		#endregion

		#region Phone3
		public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }
		public virtual String Phone3 { get; set; }
		#endregion

		#region Phone3Type
		public abstract class phone3Type : PX.Data.BQL.BqlString.Field<phone3Type> { }
		public virtual String Phone3Type { get; set; }
		#endregion

		#region Fax
		public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }
		public virtual String Fax { get; set; }
		#endregion

		#region FaxType
		public abstract class faxType : PX.Data.BQL.BqlString.Field<faxType> { }
		public virtual String FaxType { get; set; }
		#endregion

		#region IsDefaultContact
		public abstract class isDefaultContact : PX.Data.BQL.BqlBool.Field<isDefaultContact> { }
		public virtual Boolean? IsDefaultContact { get; set; }
		#endregion

		#region OverrideContact
		public abstract class overrideContact : PX.Data.BQL.BqlBool.Field<overrideContact> { }
		public virtual Boolean? OverrideContact { get; set; }
		#endregion

		#region ConsentAgreement
		public abstract class consentAgreement : PX.Data.BQL.BqlBool.Field<consentAgreement> { }
		public virtual bool? ConsentAgreement { get; set; }
		#endregion

		#region ConsentDate
		public abstract class consentDate : PX.Data.BQL.BqlDateTime.Field<consentDate> { }
		public virtual DateTime? ConsentDate { get; set; }
		#endregion

		#region ConsentExpirationDate
		public abstract class consentExpirationDate : PX.Data.BQL.BqlDateTime.Field<consentExpirationDate> { }
		public virtual DateTime? ConsentExpirationDate { get; set; }
		#endregion

		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		public virtual int? DefAddressID { get; set; }
		#endregion
	}
}
