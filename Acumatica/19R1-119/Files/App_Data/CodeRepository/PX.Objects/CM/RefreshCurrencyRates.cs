using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PX.Objects.CM
{
    public class RefreshCurrencyRates : PXGraph<RefreshCurrencyRates>
    {
        public PXCancel<RefreshFilter> Cancel;
        public PXFilter<RefreshFilter> Filter;
        [PXFilterable]
        public PXFilteredProcessing<RefreshRate, RefreshFilter> CurrencyRateList;

        protected virtual void RefreshFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            RefreshFilter filter = (RefreshFilter)e.Row;
            if (filter != null)
            {
                CurrencyRateList.SetProcessDelegate(list => RefreshRates(filter, list));
            }
        }

        protected virtual void RefreshFilter_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CurrencyRateList.Cache.Clear();
        }

        protected virtual void RefreshFilter_CuryRateTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CurrencyRateList.Cache.Clear();

            if (e.Row != null)
            {
                var filter = (RefreshFilter)e.Row;
                var rateType = (CurrencyRateType)PXSelectorAttribute.Select<RefreshFilter.curyRateTypeID>(sender, filter);
                if (rateType == null || rateType.RefreshOnline.GetValueOrDefault() == false)
                {
                    sender.RaiseExceptionHandling<RefreshFilter.curyRateTypeID>(e.Row, filter.CuryID, new PXSetPropertyException(CM.Messages.CurrencyNotSetupForOnlineRefresh, PXErrorLevel.Warning));
                }
            }
        }

        protected virtual void RefreshFilter_CuryEffDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            DateTime? newValue = (DateTime?)e.NewValue;

            if (!newValue.HasValue || GetUtcSyncDate(newValue.Value) > DateTime.Now.ToUniversalTime().Date)
            {
                throw new PXSetPropertyException(Messages.CurrencyRateRefreshDateInvalid);
            }
        }

        protected virtual IEnumerable currencyRateList()
        {
            foreach (PXResult<CurrencyList, CurrencyRateType> res in PXSelectJoin<CurrencyList,
                CrossJoin<CurrencyRateType>, Where<CurrencyRateType.refreshOnline, Equal<boolTrue>,
                And<CurrencyList.isActive, Equal<boolTrue>,
                And<CurrencyList.curyID, NotEqual<Current<RefreshFilter.curyID>>,
                And<Where<CurrencyRateType.curyRateTypeID, Equal<Current<RefreshFilter.curyRateTypeID>>, Or<Current<RefreshFilter.curyRateTypeID>, IsNull>>>>>>>.Select(this))
            {
                CurrencyList curr = res;
                CurrencyRateType rateType = res;

                RefreshRate rate = new RefreshRate();
                rate.FromCuryID = curr.CuryID;
                rate.CuryRateType = rateType.CuryRateTypeID;
                rate.OnlineRateAdjustment = rateType.OnlineRateAdjustment;

                CurrencyRateList.Cache.SetStatus(rate, PXEntryStatus.Held);
                yield return rate;
            }
        }

        protected virtual string GetApiKey()
        {
            // This function is provided for OEMs that want to provide their own API key
            // -
            return "9bced4075f0e47e084517b5b6b576ad2";
        }

        /// <summary>
        /// Helper method to decide if the user wants the relevant currency rates on
        /// a particular UTC date, or just the "freshest" currency rates -
        /// in the latter case, we need to add the UTC offset to the date given
        /// to avoid currency server error.
        /// </summary>
        /// <param name="requestedDate">The date that the user wants to obtain the currency rates on.</param>
        private static DateTime GetUtcSyncDate(DateTime requestedDate)
        {
            requestedDate = requestedDate.Date;

            TimeSpan userUtcOffset = PX.Common.LocaleInfo.GetTimeZone().UtcOffset;

            DateTime serverUtcDateTime = DateTime.Now.ToUniversalTime();
            DateTime userToday = (serverUtcDateTime + userUtcOffset).Date;

            if (requestedDate == userToday)
                return serverUtcDateTime.Date;
            else
                return requestedDate.Date;
        }

#if !AZURE
        [System.Net.WebPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif
        public virtual void RefreshRates(RefreshFilter filter, List<RefreshRate> list)
        {
            DateTime date = GetUtcSyncDate(filter.CuryEffDate.Value);
            Dictionary<string, decimal> rates = GetRatesFromService(filter, list, date);

            CuryRateMaint graph = PXGraph.CreateInstance<CuryRateMaint>();
            graph.Filter.Current.ToCurrency = filter.CuryID;
            graph.Filter.Current.EffDate = date;

            bool hasError = false;

            for (int i = 0; i < list.Count; i++)
            {
                RefreshRate rr = list[i];

                if (rates.ContainsKey(rr.FromCuryID))
                {
                    CurrencyRate curyRate = (CurrencyRate)graph.CuryRateRecordsEntry.Insert();
                    curyRate.FromCuryID = rr.FromCuryID;
                    curyRate.ToCuryID = filter.CuryID;
                    curyRate.CuryRateType = rr.CuryRateType;
                    curyRate.CuryRate = rates[rr.FromCuryID] * (1 + rr.OnlineRateAdjustment.GetValueOrDefault(0) / 100);
                    curyRate.CuryMultDiv = CuryMultDivType.Div;
                    rr.CuryRate = curyRate.CuryRate;
                    graph.CuryRateRecordsEntry.Update(curyRate);

                    PXProcessing<RefreshRate>.SetInfo(i, ActionsMessages.RecordProcessed);
                }
                else
                {
                    PXProcessing<RefreshRate>.SetError(i, PXMessages.LocalizeFormatNoPrefixNLA(Messages.NoOnlyRatesFoundForCurrency, rr.FromCuryID));
                    hasError = true;
                }
            }

            graph.Actions.PressSave();

            if (hasError)
            {
                throw new PXOperationCompletedWithErrorException(Messages.CurrencyRateFailedToRefresh);
            }
        }

        /// <summary>
        /// Receive Currency Rates from external service
        /// </summary>
        /// <param name="filter">RefreshCurrency Rates Parameters (to get ToCurrency)</param>
        /// <param name="list">Rates to update (For overrides only: to switch services for different currencies etc.)</param>
        /// <param name="date">Date to pass to external service</param>
        /// <returns>Rate value for each currency returned by service</returns>
        public virtual Dictionary<string, decimal> GetRatesFromService(RefreshFilter filter, List<RefreshRate> list, DateTime date)
        {
            string ratesRequestURL = String.Format(
                "http://openexchangerates.org/api/time-series.json?app_id={0}&base={1}&start={2:yyyy-MM-dd}&end={2:yyyy-MM-dd}",
                GetApiKey(),
                filter.CuryID,
                date);

            PXTrace.WriteInformation("Refresh rates URL: " + ratesRequestURL);

            var client = new WebClient();
            var response = client.DownloadString(new Uri(ratesRequestURL));

            JObject json = (JObject)JsonConvert.DeserializeObject(response);
            if (json == null)
            {
                throw new PXException(Messages.CurrencyRateJsonError, response);
            }
            JToken rates = json.SelectToken(String.Format("rates.{0:yyyy-MM-dd}", date), true);
            return rates.Children().Cast<JProperty>().ToDictionary(p => p.Name, p => p.Value.Value<decimal>());
        }
    }

	[Serializable]
	public partial class RefreshFilter : PX.Data.IBqlTable
	{
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask=">LLLLL")]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXUIField(DisplayName = "To Currency")]
		[PXSelector(typeof(Search<Currency.curyID>))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		protected DateTime? _CuryEffDate;
		[PXDBDate(MinValue="01/01/2000")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Currency Effective Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? CuryEffDate
		{
			get
			{
				return this._CuryEffDate;
			}
			set
			{
				this._CuryEffDate = value;
			}
		}
		#endregion
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
		protected String _CuryRateTypeID;
		[PXDBString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "Rate Type")]
		[PXSelector(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		public virtual String CuryRateTypeID
		{
			get
			{
				return this._CuryRateTypeID;
			}
			set
			{
				this._CuryRateTypeID = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// A non-mapped temporary DAC used to implement the Refresh Currency Rates form (CM507000) and process (<see cref="RefreshCurrencyRates"/>).
	/// The records of this type are generated in the <see cref="RefreshCurrencyRates.currencyRateList"/> view based on
	/// the <see cref="CurrencyList"/> and <see cref="CurrencyRateType"/> records. When processing is invoked,
	/// the <see cref="RefreshCurrencyRates.RefreshRates(RefreshFilter, List{RefreshRate}, string)"/> action uses the data in these records
	/// (currency code, rate type and adjustment rate) along with the rates received from the external service to create new currency rates 
	/// in the system (<see cref="CurrencyRate"/>).
	/// </summary>
	[Serializable]
	public partial class RefreshRate : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region FromCuryID
		public abstract class fromCuryID : PX.Data.BQL.BqlString.Field<fromCuryID> { }
		protected String _FromCuryID;

		[PXDBString(5, IsKey=true, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "From Currency", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isActive, Equal<True>>>))]
		public virtual String FromCuryID
		{
			get
			{
				return this._FromCuryID;
			}
			set
			{
				this._FromCuryID = value;
			}
		}
		#endregion
		#region CuryRateType
		public abstract class curyRateType : PX.Data.BQL.BqlString.Field<curyRateType> { }
		protected String _CuryRateType;

		[PXDBString(6, IsKey=true, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Currency Rate Type", Visibility = PXUIVisibility.Visible, Required = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID), DescriptionField = typeof(CurrencyRateType.descr))]
		public virtual String CuryRateType
		{
			get
			{
				return this._CuryRateType;
			}
			set
			{
				this._CuryRateType = value;
			}
		}
		#endregion
		#region OnlineRateAdjustment
		public abstract class onlineRateAdjustment : PX.Data.BQL.BqlDecimal.Field<onlineRateAdjustment> { }
		private decimal? _OnlineRateAdjustment;
		[PXDBDecimal(2)]
		[PXUIField(DisplayName = "Online Rate Adjustment (%)")]
		public virtual decimal? OnlineRateAdjustment
		{
			get
			{
				return _OnlineRateAdjustment;
			}
			set
			{
				_OnlineRateAdjustment = value;
			}
		}
		#endregion
		#region CuryRate
		public abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		protected Decimal? _CuryRate;

		[PXDBDecimal(8, MinValue = 0d)]
		[PXDefault()]
		[PXUIField(DisplayName = "Currency Rate", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual Decimal? CuryRate
		{
			get
			{
				return this._CuryRate;
			}
			set
			{
				this._CuryRate = value;
			}
		}
		#endregion
	}

}
