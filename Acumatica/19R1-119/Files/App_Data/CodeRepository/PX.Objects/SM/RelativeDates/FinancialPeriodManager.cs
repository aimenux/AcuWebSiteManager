using System;
using System.Collections.Generic;
using PX.Data;
using PX.Data.RelativeDates;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.SM;

namespace PX.Objects.SM
{
	public class FinancialPeriodManager : IFinancialPeriodManager
	{
		private class FinancialPeriodDefinition : IPrefetchable
		{
			public const string SLOT_KEY = "FinancialPeriodDefinition";

			public static Type[] DependentTables
			{
				get
				{
					return new[] { typeof(FinPeriod) };
				}
			}

			public List<FinPeriod> Periods { get; private set; }

			public FinancialPeriodDefinition()
			{
				Periods = new List<FinPeriod>();
			}

			public void Prefetch()
			{
				Periods.Clear();

				foreach(PXDataRecord record in PXDatabase.SelectMulti<FinPeriod>(
					new PXDataFieldOrder<FinPeriod.finPeriodID>(),
					new PXDataField<FinPeriod.startDate>(),
					new PXDataField<FinPeriod.endDate>(),
					new PXDataFieldValue<FinPeriod.organizationID>(PXDbType.Int, 4, FinPeriod.organizationID.MasterValue)))
				{
					FinPeriod period = new FinPeriod
					{
						StartDate = record.GetDateTime(0),
						EndDate = record.GetDateTime(1)
					};

					Periods.Add(period);
				}
			}
		}

		private FinancialPeriodDefinition Definition
		{
			get
			{
				return PXDatabase.GetSlot<FinancialPeriodDefinition>(FinancialPeriodDefinition.SLOT_KEY, FinancialPeriodDefinition.DependentTables);
			}
		}

		private readonly ITodayUtc todayer;

		private int? _currentIndex;
		private int CurrentIndex
		{
			get
			{
				if (_currentIndex == null)
				{
					_currentIndex = Definition.Periods
									.FindIndex(p => p.StartDate <= todayer.TodayUtc && p.EndDate > todayer.TodayUtc);

					if (_currentIndex == -1)
					{
						throw new PXException(MyMessages.CannotFindFinancialPeriod);
					}
				}

				return _currentIndex.Value;
			}
		}

		public FinancialPeriodManager(ITodayUtc today)
		{
			todayer = today;
		}

		public DateTime GetCurrentFinancialPeriodStart(int shift)
		{
			FinPeriod finPeriod = GetFinancialPeriod(shift);

			return finPeriod.StartDate.Value;
		}

		public DateTime GetCurrentFinancialPeriodEnd(int shift)
		{
			FinPeriod finPeriod = GetFinancialPeriod(shift);

			return finPeriod.EndDate.Value.AddTicks(-1);
		}

		private FinPeriod GetFinancialPeriod(int shift)
		{
			int index = CurrentIndex + shift;

			if (index < 0 || index > Definition.Periods.Count - 1)
			{
				throw new PXException(MyMessages.CannotFindNextFinancialPeriod);
			}

			return Definition.Periods[index];
		}
	}
}
