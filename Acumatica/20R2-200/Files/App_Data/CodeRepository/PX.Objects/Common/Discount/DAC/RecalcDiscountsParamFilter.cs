using System;
using PX.Data;

namespace PX.Objects.Common.Discount
{
	/// <summary>
	/// Recalculate Prices and Discounts filter
	/// </summary>
	[Serializable]
	public class RecalcDiscountsParamFilter : IBqlTable
	{
		#region RecalcTarget
		[PXDBString(3, IsFixed = true)]
		[PXDefault(AllLines)]
		[PXStringList(
			new[] {CurrentLine, AllLines},
			new[] {AR.Messages.CurrentLine, AR.Messages.AllLines})]
		[PXUIField(DisplayName = "Recalculate")]
		public virtual String RecalcTarget { get; set; }
		public abstract class recalcTarget : PX.Data.BQL.BqlString.Field<recalcTarget> { }

		public const string CurrentLine = "LNE";
		public const string AllLines = "ALL";
		#endregion
		#region RecalcUnitPrices
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Set Current Unit Prices", Visible = true)]
		public virtual Boolean? RecalcUnitPrices { get; set; }
		public abstract class recalcUnitPrices : PX.Data.BQL.BqlBool.Field<recalcUnitPrices> { }
		#endregion
		#region OverrideManualPrices
		[PXDBBool]
		[PXDefault(false)]
		[PXUIEnabled(typeof(recalcUnitPrices))]
		[PXFormula(typeof(Switch<Case<Where<recalcUnitPrices, Equal<False>>, False>, overrideManualPrices>))]
		[PXUIField(DisplayName = "Override Manual Prices", Visible = true)]
		public virtual Boolean? OverrideManualPrices { get; set; }
		public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
		#endregion
		#region RecalcDiscounts
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Recalculate Discounts")]
		public virtual Boolean? RecalcDiscounts { get; set; }
		public abstract class recalcDiscounts : PX.Data.BQL.BqlBool.Field<recalcDiscounts> { }
		#endregion
		#region OverrideManualDiscounts
		[PXDBBool]
		[PXDefault(false)]
		[PXUIEnabled(typeof(recalcDiscounts))]
		[PXFormula(typeof(Switch<Case<Where<recalcDiscounts, Equal<False>>, False>, overrideManualDiscounts>))]
		[PXUIField(DisplayName = "Override Manual Line Discounts")]
		public virtual Boolean? OverrideManualDiscounts { get; set; }
		public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
		#endregion
		#region OverrideManualDocGroupDiscounts
		[PXDBBool]
		[PXDefault(false)]
		[PXUIEnabled(typeof(recalcDiscounts))]
		[PXFormula(typeof(Switch<Case<Where<recalcDiscounts, Equal<False>>, False>, overrideManualDocGroupDiscounts>))]
		[PXUIField(DisplayName = "Override Manual Group and Document Discounts")]
		public virtual Boolean? OverrideManualDocGroupDiscounts { get; set; }
		public abstract class overrideManualDocGroupDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDocGroupDiscounts> { }
		#endregion
		#region UseRecalcFilter
		[PXDBBool]
		[PXDefault(false)]
		public virtual Boolean? UseRecalcFilter { get; set; }
		public abstract class useRecalcFilter : PX.Data.BQL.BqlBool.Field<useRecalcFilter> { }
		#endregion
	}
}