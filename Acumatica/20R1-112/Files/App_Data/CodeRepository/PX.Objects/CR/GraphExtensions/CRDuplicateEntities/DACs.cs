using System;
using PX.Data;

namespace PX.Objects.CR.Extensions.CRDuplicateEntities
{
	#region MergeParams

	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class MergeParams : IBqlTable
	{
		#region SourceContactID
		public abstract class sourceEntityID : PX.Data.BQL.BqlInt.Field<sourceEntityID> { }

		[PXDBInt]
		[PXDefault(typeof(DuplicateDocument.contactID))]
		public virtual int? SourceEntityID { get; set; }
		#endregion

		#region TargetEntityID
		public abstract class targetEntityID : PX.Data.BQL.BqlInt.Field<targetEntityID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Target", Required = true)]
		[PXDefault(typeof(Document.key))]
		public virtual int? TargetEntityID { get; set; }
		#endregion
	}

	#endregion

	#region CRGrams

	/// <exclude/>
	[Serializable]
	[PXHidden]
	public partial class CRGrams : IBqlTable
	{
		#region GramID
		public abstract class gramID : PX.Data.BQL.BqlInt.Field<gramID> { }

		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Gram ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? GramID { get; set; }
		#endregion

		#region EntityType
		public abstract class validationType : PX.Data.BQL.BqlString.Field<validationType> { }

		[PXDBString(2)]
		[PXUIField(DisplayName = "Entity Type")]
		[PXDefault(ValidationTypesAttribute.LeadContact)]
		[ValidationTypes]
		public virtual String ValidationType { get; set; }
		#endregion

		#region EntityID
		public abstract class entityID : PX.Data.BQL.BqlInt.Field<entityID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Entity ID")]
		public virtual int? EntityID { get; set; }
		#endregion

		#region FieldName
		public abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }

		[PXDBString(60)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Field Name", Visibility = PXUIVisibility.Visible)]
		public virtual String FieldName { get; set; }
		#endregion

		#region FieldValue
		public abstract class fieldValue : PX.Data.BQL.BqlString.Field<fieldValue> { }

		[PXDBString(60)]
		[PXDefault("")]
		[PXUIField(DisplayName = "Field Value", Visibility = PXUIVisibility.Visible)]
		public virtual String FieldValue { get; set; }
		#endregion

		#region Score
		public abstract class score : PX.Data.BQL.BqlDecimal.Field<score> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "1")]
		[PXUIField(DisplayName = "Score")]
		public virtual decimal? Score { get; set; }

		#endregion
	}

	#endregion

	#region CRDuplicateGrams

	/// <exclude/>
	[Serializable]
	[PXHidden]
	public partial class CRDuplicateGrams : CRGrams
	{
		public new abstract class gramID : PX.Data.BQL.BqlInt.Field<gramID> { }

		public new abstract class validationType : PX.Data.BQL.BqlString.Field<validationType> { }

		public new abstract class entityID : PX.Data.BQL.BqlInt.Field<entityID> { }

		public new abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }

		public new abstract class fieldValue : PX.Data.BQL.BqlString.Field<fieldValue> { }

		public new abstract class score : PX.Data.BQL.BqlDecimal.Field<score> { }
	}

	#endregion

	#region DuplicateContact

	/// <exclude/>
	[Serializable]
	[PXHidden]
	public partial class DuplicateContact : Contact
	{
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Business Account")]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD))]
		public override Int32? BAccountID { get; set; }

		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		public new abstract class contactPriority : PX.Data.BQL.BqlInt.Field<contactPriority> { }
		public new abstract class duplicateStatus : PX.Data.BQL.BqlString.Field<duplicateStatus> { }
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		public new abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }
		public new abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
	}

	#endregion

	#region CRDuplicateRecord

	/// <exclude/>
	[PXVirtual]
	[Serializable]
	[PXHidden]
	public partial class CRDuplicateRecord : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region EntityType
		public abstract class validationType : PX.Data.BQL.BqlString.Field<validationType> { }

		[PXDBString(2, IsKey = true)]
		[PXUIField(DisplayName = "Entity Type")]
		[ValidationTypes]
		public virtual String ValidationType { get; set; }
		#endregion

		#region DuplicateContactID
		public abstract class duplicateContactID : PX.Data.BQL.BqlInt.Field<duplicateContactID> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Duplicate Contact ID", Visibility = PXUIVisibility.Invisible)]
		[PXVirtualSelector(typeof(Contact.contactID))]
		public virtual Int32? DuplicateContactID { get; set; }
		#endregion

		#region DuplicateRefContactID
		public abstract class duplicateRefContactID : PX.Data.BQL.BqlInt.Field<duplicateRefContactID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.Invisible)]
		[PXVirtualSelector(typeof(Contact.contactID), DescriptionField = typeof(Contact.displayName))]
		public virtual Int32? DuplicateRefContactID { get; set; }
		#endregion

		#region DuplicateBAccountID
		public abstract class duplicateBAccountID : PX.Data.BQL.BqlInt.Field<duplicateBAccountID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Duplicate Contact Account ID", Visibility = PXUIVisibility.Invisible)]
		[PXVirtualSelector(typeof(BAccount.bAccountID))]
		public virtual Int32? DuplicateBAccountID { get; set; }
		#endregion

		#region Score
		public abstract class score : PX.Data.BQL.BqlDecimal.Field<score> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "1")]
		[PXUIField(DisplayName = "Score")]
		public virtual decimal? Score { get; set; }
		#endregion

		#region DuplicateContactType
		public abstract class duplicateContactType : PX.Data.BQL.BqlString.Field<duplicateContactType> { }

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Duplicate Contact Type", Visible = false)]
		public virtual String DuplicateContactType { get; set; }
		#endregion
	}

	#endregion
}
