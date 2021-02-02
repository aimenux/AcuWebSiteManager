using System;
using System.Collections.Generic;
using PX.Objects.CM.Extensions;

namespace PX.Objects.Unit
{
	public class CurrencyServiceMock : IPXCurrencyService
	{
		public int BaseDecimalPlaces()
		{
			return 2;
		}
		public int CuryDecimalPlaces(string curyID)
		{
			return curyID != "JPY" ? 2 : 0;
		}
		public int PriceCostDecimalPlaces()
		{
			return 4;
		}
		public int QuantityDecimalPlaces()
		{
			return 6;
		}
		public string DefaultRateTypeID(string moduleCode)
		{
			return "SPOT";
		}
		public IPXCurrencyRate GetRate(string fromCuryID, string toCuryID, string rateTypeID, DateTime? curyEffDate)
		{
			if (fromCuryID != toCuryID)
			{
				return new CurrencyRate { FromCuryID = fromCuryID, CuryEffDate = DateTime.Today, CuryMultDiv = "M", CuryRate = 1.28m, RateReciprocal = 0.78125m, ToCuryID = toCuryID };
			}
			return new CurrencyRate { FromCuryID = fromCuryID, CuryEffDate = DateTime.Today, CuryMultDiv = "M", CuryRate = 1.00m, RateReciprocal = 1.00m, ToCuryID = toCuryID };
		}
		public int GetRateEffDays(string rateTypeID)
		{
			return 3;
		}
		public string BaseCuryID()
		{
			return "USD";
		}
		public IEnumerable<IPXCurrency> Currencies()
		{
			return new IPXCurrency[]
			{
				new Currency { CuryID = "USD", Description = "Dollar" },
				new Currency { CuryID = "EUR", Description = "Euro" },
				new Currency { CuryID = "JPY", Description = "Yen" }
			};
		}
		public IEnumerable<IPXCurrencyRateType> CurrencyRateTypes()
		{
			return new IPXCurrencyRateType[]
			{
				new CurrencyRateType { CuryRateTypeID = "SPOT", Descr = "Spot" },
				new CurrencyRateType { CuryRateTypeID = "BANK", Descr = "Bank" }
			};
		}
	}
}
