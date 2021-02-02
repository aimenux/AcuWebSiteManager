using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.CM.Extensions
{
	public interface IPXCurrencyService
	{
		int BaseDecimalPlaces();
		int CuryDecimalPlaces(string curyID);
		int PriceCostDecimalPlaces();
		int QuantityDecimalPlaces();
		string DefaultRateTypeID(string moduleCode);
		IPXCurrencyRate GetRate(string fromCuryID, string toCuryID, string rateTypeID, DateTime? curyEffDate);
		int GetRateEffDays(string rateTypeID);
		string BaseCuryID();
		IEnumerable<IPXCurrency> Currencies();
		IEnumerable<IPXCurrencyRateType> CurrencyRateTypes();
	}

	public class DatabaseCurrencyService : IPXCurrencyService
	{
		protected PXGraph Graph;
		public DatabaseCurrencyService(PXGraph graph)
		{
			Graph = graph;
		}
		public int BaseDecimalPlaces()
		{
			Currency c = PXSelectJoin<Currency,
				InnerJoin<GL.Company, On<GL.Company.baseCuryID, Equal<Currency.curyID>>>>
				.Select(Graph);
			return c?.DecimalPlaces ?? 2;
		}
		public int CuryDecimalPlaces(string curyID)
		{
			CurrencyList c = PXSelect<CurrencyList,
				Where<CurrencyList.curyID, Equal<Required<CurrencyList.curyID>>>>
				.Select(Graph, curyID);
			return c?.DecimalPlaces ?? 2;
		}
		public int PriceCostDecimalPlaces()
		{
			CS.CommonSetup c = PXSelect<CS.CommonSetup>
				.Select(Graph);
			return c?.DecPlPrcCst ?? 2;
		}
		public int QuantityDecimalPlaces()
		{
			CS.CommonSetup c = PXSelect<CS.CommonSetup>
				.Select(Graph);
			return c?.DecPlQty ?? 2;
		}
		public string DefaultRateTypeID(string moduleCode)
		{
			string rateType = null;
			CMSetup CMSetup = (CMSetup)Graph.Caches[typeof(CMSetup)].Current;
			if (CMSetup == null)
			{
				CMSetup = PXSelectReadonly<CMSetup>.Select(Graph);
			}
			if (CMSetup != null && PXAccess.FeatureInstalled<CS.FeaturesSet.multicurrency>())
			{
				switch (moduleCode)
				{
					case GL.BatchModule.CA:
						rateType = CMSetup.CARateTypeDflt;
						break;
					case GL.BatchModule.AP:
						rateType = CMSetup.APRateTypeDflt;
						break;
					case GL.BatchModule.AR:
						rateType = CMSetup.ARRateTypeDflt;
						break;
					case GL.BatchModule.GL:
						rateType = CMSetup.GLRateTypeDflt;
						break;
					case GL.BatchModule.PM:
						rateType = CMSetup.PMRateTypeDflt;
						break;
					default:
						rateType = null;
						break;
				}
			}
			return rateType;
		}
		public IPXCurrencyRate GetRate(string fromCuryID, string toCuryID, string rateTypeID, DateTime? curyEffDate)
		{
			CurrencyRate c = PXSelectReadonly<CurrencyRate,
							Where<CurrencyRate.toCuryID, Equal<Required<CurrencyInfo.baseCuryID>>,
							And<CurrencyRate.fromCuryID, Equal<Required<CurrencyInfo.curyID>>,
							And<CurrencyRate.curyRateType, Equal<Required<CurrencyInfo.curyRateTypeID>>,
							And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyInfo.curyEffDate>>>>>>,
							OrderBy<Desc<CurrencyRate.curyEffDate>>>.SelectWindowed(Graph, 0, 1, toCuryID, fromCuryID, rateTypeID, curyEffDate);
			return c;
		}
		public int GetRateEffDays(string rateTypeID)
		{
			CurrencyRateType c = PXSelect<CurrencyRateType,
				Where<CurrencyRateType.curyRateTypeID, Equal<Required<CurrencyRateType.curyRateTypeID>>>>
				.Select(Graph, rateTypeID);
			return c?.RateEffDays ?? 0;
		}
		public string BaseCuryID()
		{
			GL.Company c = PXSelect<GL.Company>
				.Select(Graph);
			return c?.BaseCuryID;
		}
		public IEnumerable<IPXCurrency> Currencies()
		{
			foreach (Currency c in PXSelect<Currency>.Select(Graph))
			{
				yield return c;
			}
		}
		public IEnumerable<IPXCurrencyRateType> CurrencyRateTypes()
		{
			foreach (CurrencyRateType c in PXSelect<CurrencyRateType>.Select(Graph))
			{
				yield return c;
			}
		}
	}
}
