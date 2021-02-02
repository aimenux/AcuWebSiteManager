using PX.Data;
using PX.Objects.CM;
using PX.Objects.IN;
using System;

namespace PX.Objects.PO
{
	/// <summary>
	/// POOrder + Unbilled Service Items Projection
	/// </summary>
	[Serializable]
	[PXBreakInheritance]
	public partial class POOrderRS : POOrder
	{
		#region Selected
			public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
		}
		#endregion

		#region orderNbr
		public new abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr>
		{
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID>
		{
		}
		#endregion
		#region CuryUnbilledOrderTotal
			public abstract new class curyUnbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<curyUnbilledOrderTotal>
		{
		}
		[PXDBCurrency(typeof(POOrderRS.curyInfoID), typeof(POOrderRS.unbilledOrderTotal))]
		[PXUIField(DisplayName = "Unbilled Amt.", Enabled = false)]
		public override decimal? CuryUnbilledOrderTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderTotal
			public abstract new class unbilledOrderTotal : PX.Data.BQL.BqlDecimal.Field<unbilledOrderTotal>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override decimal? UnbilledOrderTotal
		{
			get;
			set;
		}
		#endregion
		#region UnbilledOrderQty
			public abstract new class unbilledOrderQty : PX.Data.BQL.BqlDecimal.Field<unbilledOrderQty>
		{
		}
		[PXDBQuantity]
		[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
		public override decimal? UnbilledOrderQty
		{
			get;
			set;
		}
		#endregion
	}
}
