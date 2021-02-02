using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.TX.Descriptor;
using PX.Objects.Common.Extensions;
using PX.Objects;
using PX.Objects.TX;

namespace PX.Objects.TX
{
	public class CurrencyRatesProvider
	{
		private Dictionary<string, Dictionary<DateTime, CurrencyRate>> _rates;

		private string _toCuryID;
		private string _rateType;
		public CurrencyRatesProvider(string rateTypeID, string toCuryID)
		{
			if (String.IsNullOrEmpty(rateTypeID))
			{
				throw new ArgumentNullException("rateTypeID");
			}

			if (String.IsNullOrEmpty(toCuryID))
			{
				throw new ArgumentNullException("toCuryID");
			}

			_rateType = rateTypeID;
			_toCuryID = toCuryID;
			_rates = new Dictionary<string, Dictionary<DateTime, CurrencyRate>>();
		}

		private DateTime GetLowerDateBoundForRatesSelection(PXGraph graph, IEnumerable<string> fromCuryIDs, DateTime minDate)
		{
			var ratesBottomDateBounds = PXSelectGroupBy<CurrencyRate,
			  Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
				And<CurrencyRate.fromCuryID, In<Required<CurrencyRate.fromCuryID>>,
				And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
				And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
			  Aggregate<
				GroupBy<CurrencyRate.fromCuryID, Max<CurrencyRate.curyEffDate>>>>
			  .Select(graph, _toCuryID, fromCuryIDs.ToArray(), _rateType, minDate);

			return ratesBottomDateBounds.RowCast<CurrencyRate>().Select(cr => cr.CuryEffDate).Min() ?? minDate;
		}

		public void Fill(PXGraph graph, IEnumerable<string> fromCuryIDs, DateTime minDate, DateTime maxDate)
		{
			fromCuryIDs = new HashSet<string>(fromCuryIDs);

			if (fromCuryIDs.Any(c => c != null) == false)
			{
				return;
			}

			foreach (var cury in fromCuryIDs)
			{
				_rates[cury] = new Dictionary<DateTime, CurrencyRate>();
			}

			var minDateToSelect = GetLowerDateBoundForRatesSelection(graph, fromCuryIDs, minDate);

			var rates = PXSelect<CurrencyRate,
			  Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
				And<CurrencyRate.fromCuryID, In<Required<CurrencyRate.fromCuryID>>,
				And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
				And<Where<CurrencyRate.curyEffDate, GreaterEqual<Required<CurrencyRate.curyEffDate>>,
					And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyRate.curyEffDate>>,
					Or<CurrencyRate.curyEffDate, IsNull>>>>>>>,
			  OrderBy<Desc<CurrencyRate.curyEffDate>>>
			  .Select(graph, _toCuryID, fromCuryIDs.ToArray(), _rateType, minDateToSelect, maxDate);

			foreach (CurrencyRate rate in rates)
			{
				var date = rate.CuryEffDate == null || rate.CuryEffDate < minDate ? minDate : rate.CuryEffDate.Value;

				while (_rates[rate.FromCuryID].ContainsKey(date) == false && date <= maxDate)
				{
					_rates[rate.FromCuryID][date] = rate;
					date = date.AddDays(1);
				}
			}
		}

		public CurrencyRate GetRate(string fromCuryID, DateTime date)
		{
			if (_toCuryID == fromCuryID)
				return new CurrencyRate
				{
					FromCuryID = fromCuryID,
					ToCuryID = _toCuryID,
					CuryRate = 1.0M,
					RateReciprocal = 1.0M,
					CuryMultDiv = CuryMultDivType.Mult,
					CuryEffDate = date,
					CuryRateType = _rateType
				};

			if (!_rates.ContainsKey(fromCuryID))
			{
				throw new PXException(Messages.RatesForCurrencyWereNotProvided, fromCuryID);
			}

			if(_rates[fromCuryID].Count == 0)
			{
				return null;
			}

			try
			{
				return _rates[fromCuryID][date];
			}
			catch (Exception)
			{
				throw new PXException(Messages.RateForCurrenciesAndDateNotFound, fromCuryID, _toCuryID, date.ToShortDateString());
			}
		}
	}
}