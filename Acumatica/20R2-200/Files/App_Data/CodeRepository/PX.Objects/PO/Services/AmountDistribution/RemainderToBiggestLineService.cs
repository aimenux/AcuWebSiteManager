using PX.Objects.CM;
using System;
using System.Linq;

namespace PX.Objects.PO.Services.AmountDistribution
{
	public class RemainderToBiggestLineService<Item> : IAmountDistributionService<Item>
		where Item : class, IAmountItem
	{
		public RemainderToBiggestLineService(DistributionParameter<Item> distributeParameter)
		{
			_context = distributeParameter;
		}

		protected DistributionParameter<Item> _context;
		protected Item _biggestLine;
		protected decimal? _biggestLineWeight;

		public virtual DistributionResult<Item> Distribute()
		{
			decimal sumOfWeights = GetSumOfWeight();

			if (sumOfWeights == 0m)
				return new DistributionResult<Item>() { Successful = false };

			decimal? sumOfAmt = 0m;
			decimal? curySumOfAmt = 0m;
			Clear();

			foreach (Item item in _context.Items)
			{
				ProcessItem(item, sumOfWeights, ref sumOfAmt, ref curySumOfAmt);
			}

			decimal? roundingDifference = _context.Amount - sumOfAmt;
			decimal? curyRoundingDifference = _context.CuryAmount - curySumOfAmt;
			if (roundingDifference != 0m || curyRoundingDifference != 0m)
				DistributeRoundingDifference(_biggestLine, roundingDifference, curyRoundingDifference);

			return new DistributionResult<Item>() { Successful = true };
		}

		protected virtual void Clear()
		{
			_biggestLine = null;
			_biggestLineWeight = null;
		}

		protected virtual decimal GetSumOfWeight()
		{
			return _context.Items.Sum(i => i.Weight);
		}

		protected virtual void ProcessItem(Item item, decimal? sumOfWeight, ref decimal? sumOfAmt, ref decimal? curySumOfAmt)
		{
			SetBiggestLine(item);

			CalculateAmount(item.Weight, sumOfWeight, out decimal? currentAmount, out decimal? curyCurrentAmount);
			ReplaceAmount(item, ref currentAmount, ref curyCurrentAmount);
			RoundAmount(item, ref currentAmount, ref curyCurrentAmount);
			ValueCalculated(item, currentAmount, curyCurrentAmount);

			sumOfAmt += currentAmount;
			curySumOfAmt += curyCurrentAmount;
		}

		protected virtual void CalculateAmount(decimal weight, decimal? sumOfWeight, out decimal? currentAmount, out decimal? curyCurrentAmount)
		{
			currentAmount = (_context.Amount * weight) / sumOfWeight;
			curyCurrentAmount = (_context.CuryAmount * weight) / sumOfWeight;
		}

		protected virtual void ReplaceAmount(Item item, ref decimal? currentAmount, ref decimal? curyCurrentAmount)
		{
			if (_context.ReplaceAmount != null)
			{
				var newAmount = _context.ReplaceAmount(item, currentAmount, curyCurrentAmount);
				currentAmount = newAmount.Item1;
				curyCurrentAmount = newAmount.Item2;
			}
		}

		protected virtual void RoundAmount(Item item, ref decimal? currentAmount, ref decimal? curyCurrentAmount)
		{
			currentAmount = RoundAmountDecimal(currentAmount, false);
			curyCurrentAmount = RoundAmountDecimal(curyCurrentAmount, true);
		}

		protected virtual void ValueCalculated(Item item, decimal? currentAmount, decimal? curyCurrentAmount)
		{
			item.Amount = currentAmount;
			item.CuryAmount = curyCurrentAmount;

			var updatedItem = _context.OnValueCalculated?.Invoke(item, currentAmount, curyCurrentAmount);

			if (updatedItem != null && item == _biggestLine)
				_biggestLine = updatedItem;
		}

		protected virtual void SetBiggestLine(Item item)
		{
			if (_biggestLine == null || item.Weight > _biggestLineWeight)
			{
				_biggestLine = item;
				_biggestLineWeight = item.Weight;
			}
		}

		protected virtual void DistributeRoundingDifference(Item item, decimal? roundingDifference, decimal? curyRoundingDifference)
		{
			decimal? oldValue = item.Amount;
			decimal? curyOldValue = item.Amount;

			item.Amount += roundingDifference;
			item.CuryAmount += curyRoundingDifference;

			_context.OnRoundingDifferenceApplied?.Invoke(item, item.Amount, item.CuryAmount, oldValue, curyOldValue);
		}

		protected virtual decimal? RoundAmountDecimal(decimal? currentAmount, bool curyValue)
		{
			if (curyValue)
			{
				if (_context.CacheOfCuryRow == null || _context.CuryRow == null)
					return currentAmount;

				return PXDBCurrencyAttribute.RoundCury(_context.CacheOfCuryRow, _context.CuryRow, currentAmount ?? 0m);
			}
			else
			{
				if (_context.CacheOfCuryRow == null)
					return currentAmount;

				return PXDBCurrencyAttribute.BaseRound(_context.CacheOfCuryRow.Graph, currentAmount ?? 0m);
			}
		}
	}
}
