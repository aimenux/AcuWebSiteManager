using PX.Data;
using System;

namespace PX.Objects.IN
{
	[Serializable()]
	[PXHidden]
	public partial class InventoryQtyByPlanType : IBqlTable
	{
		#region PlanType
		public abstract class planType : PX.Data.BQL.BqlString.Field<planType>
        {
		}
		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "Plan Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual String PlanType
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty>
        {
		}
		[PXDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual Decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included>
        {
		}
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Included", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual Boolean? Included
		{
			get;
			set;
		}
		#endregion
		#region IsTotal
		public abstract class isTotal : PX.Data.BQL.BqlBool.Field<isTotal>
        {
		}
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "IsTotal", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true, Visible = false)]
		public virtual Boolean? IsTotal
		{
			get;
			set;
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder>
        {
		}
		[PXInt()]
		[PXDefault(0)]
		public virtual int? SortOrder
		{
			get;
			set;
		}
		#endregion
	}
}
