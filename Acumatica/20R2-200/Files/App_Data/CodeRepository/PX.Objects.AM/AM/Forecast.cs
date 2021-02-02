using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// MRP Forecast Graph
    /// </summary>
    public class Forecast : PXGraph<Forecast>
    {
        public PXSave<AMForecast> Save;
        public PXCancel<AMForecast> Cancel;
        public PXInsert<AMForecast> Insert;
        public PXDelete<AMForecast> Delete;
        public PXCopyPasteAction<AMForecast> CopyPaste;

        [PXFilterable]
        [PXImport(typeof(AMForecast))]
        public PXSelect<AMForecast> ForecastRecords;
        public PXSetup<AMRPSetup> setup;
        [PXHidden]
        public PXSetup<Numbering>.Where<Numbering.numberingID.IsEqual<AMRPSetup.forecastNumberingID.FromCurrent>> ForecastNumbering;

        public Forecast()
        {
            var mrpSetup = setup.Current;

            // Forecast numbering id is optional unless you want to use forecast you must have a value entered
            if (mrpSetup == null || string.IsNullOrWhiteSpace(mrpSetup.ForecastNumberingID))
            {
                throw new PXSetupNotEnteredException(AM.Messages.GetLocal(AM.Messages.ForecastNumberSequenceSetupNotEntered), typeof(AMRPSetup), AM.Messages.GetLocal(AM.Messages.MrpSetup));
            }

            PXUIFieldAttribute.SetVisible<AMForecast.forecastID>(ForecastRecords.Cache, null, IsForecastIdEnabled);
        }

        /// <summary>
        /// IS the forecast id field enabled for the user? (Linked to manual numbering for the forecast numbering sequence)
        /// </summary>
        public bool IsForecastIdEnabled
        {
            get
            {
                if (ForecastNumbering == null)
                {
                    return false;
                }

                if (ForecastNumbering.Current == null)
                {
                    ForecastNumbering.Current = ForecastNumbering.Select();

                    if (ForecastNumbering.Current == null)
                    {
                        return false;
                    }
                }

                return ForecastNumbering.Current.UserNumbering.GetValueOrDefault();
            }
        }

        protected virtual void AMForecast_BeginDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetEndDate(cache, (AMForecast)e.Row);
        }

        protected virtual void AMForecast_Interval_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetEndDate(cache, (AMForecast)e.Row);
        }

        protected virtual void AMForecast_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            PXUIFieldAttribute.SetEnabled<AMForecast.forecastID>(cache, null, IsForecastIdEnabled);
        }

        protected virtual void AMForecast_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            var amforecast = (AMForecast)e.Row;

            if (amforecast == null)
            {
                return;
            }

            if (amforecast.Qty.GetValueOrDefault() <= 0)
            {
                cache.RaiseExceptionHandling<AMForecast.qty>(e.Row, amforecast.Qty, new PXSetPropertyException(AM.Messages.QuantityGreaterThanZero));
            }
            if (!Common.Dates.StartBeforeEnd(amforecast.BeginDate, amforecast.EndDate))
            {
                cache.RaiseExceptionHandling<AMForecast.endDate>(e.Row, amforecast.EndDate, 
                    new PXSetPropertyException(AM.Messages.MustBeGreaterThan, 
                        PXUIFieldAttribute.GetDisplayName<AMForecast.endDate>(ForecastRecords.Cache),
                        PXUIFieldAttribute.GetDisplayName<AMForecast.beginDate>(ForecastRecords.Cache)));
            }
        }

        /// <summary>
        /// Send the end date based on the current interval/begin date
        /// </summary>
        protected virtual void SetEndDate(PXCache cache, AMForecast forecast)
        {
            if (forecast == null
                || Common.Dates.IsDateNull(forecast.BeginDate)
                || string.IsNullOrWhiteSpace(forecast.Interval))
            {
                return;
            }

            var endDate = GetEndDate(cache, forecast);
            cache.SetValueExt<AMForecast.endDate>(forecast, endDate);
        }

        /// <summary>
        /// Get the calculated forecast end date
        /// </summary>
        public virtual DateTime? GetEndDate(PXCache cache, AMForecast forecast)
        {
            if (forecast == null
                || Common.Dates.IsDateNull(forecast.BeginDate)
                || string.IsNullOrWhiteSpace(forecast.Interval))
            {
                return forecast == null || forecast.BeginDate == null ? Accessinfo.BusinessDate : forecast.BeginDate;
            }

            return ForecastInterval.GetEndDate(forecast.Interval, forecast.BeginDate.GetValueOrDefault());
        }

        protected virtual void AMForecast_EndDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            var row = (AMForecast) e.Row;
            if (row == null)
            {
                e.NewValue = Accessinfo.BusinessDate;
                return;
            }

            e.NewValue = GetEndDate(cache, row);
        }

        /// <summary>
        /// Converts a forecast record into the intervals that would be used for MRP Planning
        /// </summary>
        /// <param name="forecast">Valid forecast row</param>
        /// <returns>List of paired start/end dates</returns>
        public static List<Tuple<DateTime, DateTime>> GetForecastIntervals(AMForecast forecast)
        {
            if (forecast == null || forecast.BeginDate == null || forecast.EndDate == null)
            {
                return null;
            }

            DateTime beginDate = forecast.BeginDate ?? Common.Dates.EndOfTimeDate;
            DateTime endDate = forecast.EndDate ?? Common.Dates.BeginOfTimeDate;
            var intervals = new List<Tuple<DateTime, DateTime>>();

            if (string.IsNullOrWhiteSpace(forecast.Interval) || forecast.Interval == ForecastInterval.OneTime)
            {
                intervals.Add(new Tuple<DateTime, DateTime>(beginDate, endDate));
                return intervals;
            }

            DateTime intervalEndDate = endDate;
            while (beginDate < intervalEndDate)
            {
                endDate = ForecastInterval.GetEndDate(forecast.Interval, beginDate);

                intervals.Add(new Tuple<DateTime, DateTime>(beginDate, endDate));

                beginDate = endDate.AddDays(1);
            }

            return intervals;
        }

        protected virtual void AMForecast_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            if (IsForecastIdEnabled)
            {
                return;
            }

            // Temp key to allow multiple inserts before persisting. When persisting and auto number it will swap the value for us.
            var insertedCounter = cache.Inserted.Count() + 1;
            ((AMForecast) e.Row).ForecastID = $"-{insertedCounter}";
        }
    }
}