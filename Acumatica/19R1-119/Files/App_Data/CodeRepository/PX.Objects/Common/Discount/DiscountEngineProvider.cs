using System;
using PX.Data;

namespace PX.Objects.Common.Discount
{
	/// <summary>
	/// Provides instances of discount engines for a specific discounted line type
	/// </summary>
	public class DiscountEngineProvider
	{
		/// <summary>
		/// Get instance of discount engines for a specific discounted line type
		/// </summary>
		/// <typeparam name="TLine">The type of discounted line</typeparam>
		/// <returns></returns>
		public static DiscountEngine<TLine, TDiscountDetail> GetEngineFor<TLine, TDiscountDetail>()
			where TLine : class, IBqlTable, new()
			where TDiscountDetail : class, IBqlTable, IDiscountDetail, new()
			=> EnginesCache<TLine, TDiscountDetail>.Engine;

		/// <summary>
		/// Caches discount engines for a specific discounted line type
		/// </summary>
		/// <typeparam name="TLine">The type of discounted line</typeparam>
		private static class EnginesCache<TLine, TDiscountDetail>
			where TLine : class, IBqlTable, new()
			where TDiscountDetail : class, IBqlTable, IDiscountDetail, new()
		{
			private static readonly Lazy<DiscountEngine<TLine, TDiscountDetail>> _engine = new Lazy<DiscountEngine<TLine, TDiscountDetail>>(PXGraph.CreateInstance<DiscountEngine<TLine, TDiscountDetail>>);

			public static DiscountEngine<TLine, TDiscountDetail> Engine => _engine.Value;
		}
	}
}