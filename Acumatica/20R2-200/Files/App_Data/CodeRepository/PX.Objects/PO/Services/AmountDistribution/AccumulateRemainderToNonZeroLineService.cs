namespace PX.Objects.PO.Services.AmountDistribution
{
	public class AccumulateRemainderToNonZeroLineService<Item> : RemainderToBiggestLineService<Item>
		where Item : class, IAmountItem
	{
		public AccumulateRemainderToNonZeroLineService(DistributionParameter<Item> distributeParameter)
			: base(distributeParameter)
		{
		}

		protected decimal? _accumulatedAmount;
		protected decimal? _curyAccumulatedAmount;

		protected override void Clear()
		{
			_accumulatedAmount = 0m;
			_curyAccumulatedAmount = 0m;
			base.Clear();
		}

		protected override void RoundAmount(Item item, ref decimal? currentAmount, ref decimal? curyCurrentAmount)
		{
			currentAmount += _accumulatedAmount;
			curyCurrentAmount += _curyAccumulatedAmount;

			var oldAmount = currentAmount;
			var curyOldAmount = curyCurrentAmount;

			base.RoundAmount(item, ref currentAmount, ref curyCurrentAmount);

			_accumulatedAmount = oldAmount - currentAmount;
			_curyAccumulatedAmount = curyOldAmount - curyCurrentAmount;
		}
	}
}