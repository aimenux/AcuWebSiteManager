using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.CR
{
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

	[Serializable]
    [PXHidden]
	public partial class CRGrams2 : CRGrams
	{
		public new abstract class gramID : PX.Data.BQL.BqlInt.Field<gramID> { }

		public new abstract class validationType : PX.Data.BQL.BqlString.Field<validationType> { }

		public new abstract class entityID : PX.Data.BQL.BqlInt.Field<entityID> { }

		public new abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }

		public new abstract class fieldValue : PX.Data.BQL.BqlString.Field<fieldValue> { }
	}
}
