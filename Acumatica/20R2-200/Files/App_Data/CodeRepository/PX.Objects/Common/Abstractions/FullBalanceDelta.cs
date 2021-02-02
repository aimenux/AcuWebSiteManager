namespace PX.Objects.Common
{
	public struct FullBalanceDelta
	{
		/// <summary>
		/// The unsigned amount (in adjusted document currency)
		/// on which the adjusted document balance is affected
		/// and which is not induced by a symmetrical change
		/// in the adjusting document balance. For example,
		/// it may include RGOL, write-offs, cash discounts etc.
		/// </summary>
		public decimal CurrencyAdjustedExtraAmount;
		/// <summary>
		/// The unsigned amount (in adjusting document currency)
		/// on which the adjusted document balance is affected
		/// and which is not induced by a symmetrical change
		/// in the adjusting document balance. For example,
		/// it may include RGOL, write-offs, cash discounts etc.
		/// </summary>
		public decimal CurrencyAdjustingExtraAmount;
		/// <summary>
		/// The unsigned amount (in base currency)
		/// on which the adjusted document balance is affected
		/// and which is not induced by a symmetrical change
		/// in the adjusting document balance. For example,
		/// it may include RGOL, write-offs, cash discounts etc.
		/// </summary>
		public decimal BaseAdjustedExtraAmount;
		/// <summary>
		/// The full unsigned amount (in document currency) 
		/// on which the adjusted document balance is affected
		/// by the application.
		/// </summary>
		public decimal CurrencyAdjustedBalanceDelta;
		/// <summary>
		/// The full unsigned amount (in document currency)
		/// on which the adjusting document balance is affected
		/// by the application.
		/// </summary>
		public decimal CurrencyAdjustingBalanceDelta;
		/// <summary>
		/// The full unsigned amount (in base currency)
		/// on which the adjusted document balance is affected
		/// by the application.
		/// </summary>
		public decimal BaseAdjustedBalanceDelta;
		/// <summary>
		/// The full unsigned amount (in base currency)
		/// on which the adjusting document balance is affected
		/// by the application.
		/// </summary>
		public decimal BaseAdjustingBalanceDelta;
	}
}
