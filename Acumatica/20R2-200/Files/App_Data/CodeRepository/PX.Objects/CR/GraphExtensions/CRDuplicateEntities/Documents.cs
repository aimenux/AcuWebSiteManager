using PX.Data;
using System;

namespace PX.Objects.CR.Extensions.CRDuplicateEntities
{
	/// <exclude/>
	[PXHidden]
	public class Document : PXMappedCacheExtension
	{
		#region Key
		public abstract class key : PX.Data.BQL.BqlInt.Field<key> { }
		public virtual int? Key { get; set; }
		#endregion

		#region DefContactID
		public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		public virtual int? DefContactID { get; set; }
		#endregion

		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		public virtual int? DefAddressID { get; set; }
		#endregion
	}

	/// <exclude/>
	[PXHidden]
	public class DuplicateDocument : PXMappedCacheExtension
	{
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		public virtual int? ContactID { get; set; }
		#endregion

		#region RefContactID
		public abstract class refContactID : PX.Data.BQL.BqlInt.Field<refContactID> { }
		public virtual int? RefContactID { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public virtual int? BAccountID { get; set; }
		#endregion

		#region ContactType
		public abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }
		public virtual String ContactType { get; set; }
		#endregion

		#region DuplicateStatus
		public abstract class duplicateStatus : PX.Data.BQL.BqlString.Field<duplicateStatus> { }
		public virtual string DuplicateStatus { get; set; }
		#endregion

		#region DuplicateFound
		public abstract class duplicateFound : PX.Data.BQL.BqlBool.Field<duplicateFound> { }
		public virtual bool? DuplicateFound { get; set; }
		#endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		public virtual bool? IsActive { get; set; }
		#endregion

		#region Email
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		public virtual string Email { get; set; }
		#endregion

		#region GrammValidationDateTime
		public abstract class grammValidationDateTime : PX.Data.BQL.BqlDateTime.Field<grammValidationDateTime> { }
		public virtual DateTime? GrammValidationDateTime { get; set; }
		#endregion
	}
}
