using PX.Data;
using System;
using Autofac;
using System.Collections.Generic;

namespace PX.Objects.PO.Services.AmountDistribution
{
	#region Auxiliary Types
	public enum DistributionMethod
	{
		RemainderToBiggestLine,
		RemainderToLastLine,
		AccumulateRemainderToNonZeroLine
	}

	public interface IAmountDistributionService<Item> where Item : class, IAmountItem
	{
		DistributionResult<Item> Distribute();
	}

	public interface IAmountItem
	{
		decimal Weight { get; }
		decimal? Amount { get; set; }
		decimal? CuryAmount { get; set; }
	}

	public class DistributionParameter<ItemType>
		where ItemType : class, IAmountItem
	{
		public IEnumerable<ItemType> Items;
		public decimal? Amount;
		public decimal? CuryAmount;
		public object CuryRow;
		public PXCache CacheOfCuryRow;
		public Func<ItemType, decimal?, decimal?, ItemType> OnValueCalculated;
		public Action<ItemType, decimal?, decimal?, decimal?, decimal?> OnRoundingDifferenceApplied;
		public Func<ItemType, decimal?, decimal?, Tuple<decimal?, decimal?>> ReplaceAmount;
	}

	public class DistributionResult<ItemType>
		where ItemType : class, IAmountItem
	{
		public bool Successful;
	}
	#endregion

	public class AmountDistributionFactory
	{
		public virtual IAmountDistributionService<Item> CreateService<Item>(DistributionMethod method, DistributionParameter<Item> distributeParameter)
			where Item : class, IAmountItem
		{
			switch (method)
			{
				case DistributionMethod.RemainderToBiggestLine:
					return new RemainderToBiggestLineService<Item>(distributeParameter);
				case DistributionMethod.RemainderToLastLine:
					return new RemainderToLastLineService<Item>(distributeParameter);
				case DistributionMethod.AccumulateRemainderToNonZeroLine:
					return new RemainderToLastLineService<Item>(distributeParameter);
				default:
					throw new NotImplementedException();
			}
		}

		public class ServiceRegistration : Module
		{
			protected override void Load(ContainerBuilder builder)
				=> builder.RegisterType<AmountDistributionFactory>().AsSelf().PreserveExistingDefaults();
		}
	}
}