using PX.Data;

namespace PX.Objects.IN.Matrix.DAC.Unbound
{
	[PXCacheName(Messages.EntityMatrixDAC)]
	public class EntryMatrix : IBqlTable
	{
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXInt(IsKey = true)]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region RowAttributeValue
		public abstract class rowAttributeValue : PX.Data.BQL.BqlString.Field<rowAttributeValue> { }
		[PXString(10, IsUnicode = true)]
		public virtual string RowAttributeValue
		{
			get;
			set;
		}
		#endregion
		#region RowAttributeValueDescr
		public abstract class rowAttributeValueDescr : PX.Data.BQL.BqlString.Field<rowAttributeValueDescr> { }
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Attribute Value", Enabled = false)]
		public virtual string RowAttributeValueDescr
		{
			get;
			set;
		}
		#endregion

		#region ColAttributeValues
		public abstract class colAttributeValues : PX.Data.BQL.BqlByteArray.Field<colAttributeValues> { }
		public virtual string[] ColAttributeValues
		{
			get;
			set;
		}
		#endregion
		#region ColAttributeValueDescrs
		public abstract class colAttributeValueDescrs : PX.Data.BQL.BqlByteArray.Field<colAttributeValueDescrs> { }
		public virtual string[] ColAttributeValueDescrs
		{
			get;
			set;
		}
		#endregion

		#region InventoryIDs
		public abstract class inventoryIDs : PX.Data.BQL.BqlByteArray.Field<inventoryIDs> { }
		public virtual int?[] InventoryIDs
		{
			get;
			set;
		}
		#endregion

		#region Quantities
		public abstract class quantities : PX.Data.BQL.BqlByteArray.Field<quantities> { }
		public virtual decimal?[] Quantities
		{
			get;
			set;
		}
		#endregion

		#region Errors
		public abstract class errors : PX.Data.BQL.BqlByteArray.Field<errors> { }
		public virtual string[] Errors
		{
			get;
			set;
		}
		#endregion

		#region AllSelected
		public abstract class allSelected : PX.Data.BQL.BqlBool.Field<allSelected> { }
		[PXBool]
		public virtual bool? AllSelected
		{
			get;
			set;
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlByteArray.Field<selected> { }
		public virtual bool?[] Selected
		{
			get;
			set;
		}
		#endregion

		#region IsPreliminary
		public abstract class isPreliminary : PX.Data.BQL.BqlBool.Field<isPreliminary> { }
		[PXBool]
		public virtual bool? IsPreliminary
		{
			get;
			set;
		}
		#endregion

		#region IsTotal
		public abstract class isTotal : PX.Data.BQL.BqlBool.Field<isTotal> { }
		[PXBool]
		public virtual bool? IsTotal
		{
			get;
			set;
		}
		#endregion

		#region MatrixAvailability
		public abstract class matrixAvailability : PX.Data.BQL.BqlInt.Field<matrixAvailability> { }
		[PXString(IsUnicode = true)]
		public virtual string MatrixAvailability
		{
			get;
			set;
		}
		#endregion

		#region SelectedColumn
		public abstract class selectedColumn : PX.Data.BQL.BqlInt.Field<selectedColumn> { }
		[PXInt]
		public virtual int? SelectedColumn
		{
			get;
			set;
		}
		#endregion
	}
}
