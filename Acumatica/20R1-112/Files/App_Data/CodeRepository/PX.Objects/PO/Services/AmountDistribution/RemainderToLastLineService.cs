namespace PX.Objects.PO.Services.AmountDistribution
{
	public class RemainderToLastLineService<Item> : RemainderToBiggestLineService<Item>
		where Item : class, IAmountItem
	{
		public RemainderToLastLineService(DistributionParameter<Item> distributeParameter)
			: base(distributeParameter)
		{
		}

		protected Item _lastLine;

		protected override void Clear()
		{ 
			_lastLine = null;
			base.Clear();
		}

		protected override void ProcessItem(Item item, decimal? sumOfWeight, ref decimal? sumOfAmt, ref decimal? curySumOfAmt)
		{
			base.ProcessItem(item, sumOfWeight, ref sumOfAmt, ref curySumOfAmt);
			SetLastLine(item);
		}

		protected virtual void SetLastLine(Item item)
		{
			if (item.Weight > 0m)
				_lastLine = item;
		}

		protected override void DistributeRoundingDifference(Item item, decimal? roundingDifference, decimal? curyRoundingDifference)
			=> base.DistributeRoundingDifference(_lastLine, roundingDifference, curyRoundingDifference);
	}
}
