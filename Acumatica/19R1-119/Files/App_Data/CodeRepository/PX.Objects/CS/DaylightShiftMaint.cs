using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PX.Common;
using PX.Data;

namespace PX.Objects.CS
{
	#region Year

	[Serializable]
	[PXHidden]
	public sealed class Year : IBqlTable
	{
		#region Nbr

		public abstract class nbr : PX.Data.BQL.BqlInt.Field<nbr> { }

		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Name")]
		public Int32? Nbr { get; set; }

		#endregion
	}

	#endregion

	#region DaylightSelectorAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DaylightSelectorAttribute : PXCustomSelectorAttribute
	{
		public DaylightSelectorAttribute()
			: base(typeof(Year.nbr), typeof(Year.nbr))
		{
		}

		public IEnumerable GetRecords()
		{
			var currentYear = DateTime.Today.Year;
			const int range = 30;
			var start = currentYear - range;
			var end = currentYear + range;
			for (int i = start; i < end; i++)
				yield return new Year { Nbr = i };
		}
	}

	#endregion

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CurrentYearByDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
	{
		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = DateTime.Today.Year;
		}
	}

	#region DaylightShiftFilter

	[Serializable]
	[PXCacheName(Messages.CalendarYear)]
	public partial class DaylightShiftFilter : IBqlTable
	{
		#region Year

		public abstract class year : PX.Data.BQL.BqlInt.Field<year> { }

		[PXInt]
		[PXUIField(DisplayName = "Year")]
		[DaylightSelector]
		[CurrentYearByDefault]
		public virtual Int32? Year { get; set; }

		#endregion
	}

	#endregion

	public class DaylightShiftMaint : PXGraph<DaylightShiftMaint>
	{
		private static readonly HybridDictionary _timeZonesHashtable;

		#region Selects

		public PXFilter<DaylightShiftFilter>
			Filter;

		public PXSelect<DaylightShift,
			Where<DaylightShift.year, Equal<Current<DaylightShiftFilter.year>>>,
			OrderBy<Asc<DaylightShift.originalShift>>>
			Calendar;

		#endregion

		#region Ctors

		static DaylightShiftMaint()
		{
			_timeZonesHashtable = new HybridDictionary();
			foreach (PXTimeZoneInfo zone in PXTimeZoneInfo.GetSystemTimeZones())
				_timeZonesHashtable.Add(zone.Id, zone);
		}

		#endregion

		#region Actions

		public PXSave<DaylightShiftFilter> Save;

		public PXCancel<DaylightShiftFilter> Cancel;

		public PXAction<DaylightShiftFilter> Previous;
		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select)]
		[PXPreviousButton]
		protected IEnumerable previous(PXAdapter adapter)
		{
			Filter.Current.Year = Filter.Current.Year.Return(_ => (int)_ - 1, DateTime.Today.Year);
			return adapter.Get();
		}

		public PXAction<DaylightShiftFilter> Next;
		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select)]
		[PXNextButton]
		protected IEnumerable next(PXAdapter adapter)
		{
			Filter.Current.Year = Filter.Current.Year.Return(_ => (int)_ + 1, DateTime.Today.Year);
			return adapter.Get();
		}

		#endregion

		#region Data Handlers

		protected IEnumerable calendar()
		{
			if (Filter.Current.With(_ => _.Year) == null) yield break;

			HybridDictionary hashtable = new HybridDictionary();
			foreach (DaylightShift row in PXSelect<DaylightShift,
				Where<DaylightShift.year, Equal<Current<DaylightShiftFilter.year>>,
					And<DaylightShift.isActive, Equal<True>>>>.
				Select(this))
			{
				var timeZoneInfo = (PXTimeZoneInfo)_timeZonesHashtable[row.TimeZone];
				row.OriginalShift = timeZoneInfo.BaseUtcOffset.TotalMinutes;
				yield return row;

				hashtable.Add(row.TimeZone, row);
			}
			if (hashtable.Count < _timeZonesHashtable.Count)
			{
				var provider = new SystemTimeRegionProvider();
				foreach (DictionaryEntry entry in _timeZonesHashtable)
				{
					var id = entry.Key;
					if (!hashtable.Contains(id))
					{
						var timeZoneInfo = (PXTimeZoneInfo)entry.Value;
						var year = (int)Filter.Current.Year;
						var row = new DaylightShift
						{
							Year = year,
							TimeZone = (string)id,
							TimeZoneDescription = PXMessages.LocalizeFormatNoPrefix(timeZoneInfo.DisplayName),
							IsActive = false,
							OriginalShift = timeZoneInfo.BaseUtcOffset.TotalMinutes
						};
						var dts = provider.FindTimeRegionByTimeZone((string)id).
							With(_ => new DaylightSavingTime(year, _));
						if (dts != null && dts.IsActive)
						{
							row.ToDate = dts.End;
							row.FromDate = dts.Start;
							row.Shift = (int)dts.DaylightOffset.TotalMinutes;
						}
						yield return row;
					}
				}
			}
		}

		#endregion

		#region Event Handlers

		protected virtual void DaylightShift_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			DaylightShift row = e.Row as DaylightShift;
			if (row == null || row.Year == null || row.TimeZone == null) return;

			bool isEditable = row.IsActive == true;
			if (isEditable)
			{
				DefaultEditableRow(row);
			}
		}

		protected virtual void DaylightShift_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			DaylightShift row = e.Row as DaylightShift;
			if (row == null || row.Year == null || row.TimeZone == null) return;

			bool isEditable = row.IsActive == true;
			if (isEditable)
			{
				DefaultEditableRow(row);
			}
			else
			{
				DaylightSavingTime dts = GetDST(row.TimeZone, (int) row.Year);
				if (dts != null && dts.IsActive)
				{
					row.FromDate = dts.Start;
					row.ToDate = dts.End;
					row.Shift = (int) dts.DaylightOffset.TotalMinutes;
				}
				else
				{
					row.ToDate = null;
					row.FromDate = null;
					row.Shift = null;
				}
			}
		}

		private DaylightSavingTime GetDST(string timeZone, int year)
		{
			return new SystemTimeRegionProvider().
					FindTimeRegionByTimeZone(timeZone).
					With(_ => new DaylightSavingTime(year, _));
		}

		private void DefaultEditableRow(DaylightShift row)
		{
			DaylightShift savedRow = PXSelectReadonly<DaylightShift,
					Where<DaylightShift.year, Equal<Current<DaylightShiftFilter.year>>,
						And<DaylightShift.timeZone, Equal<Required<DaylightShift.timeZone>>>>>.SelectWindowed(this, 0, 1, row.TimeZone);

			DateTime newDate = DateTime.UtcNow.Date;
			newDate = new DateTime((int)row.Year, newDate.Month, newDate.Day) + new TimeSpan(12, 0, 0);
			DateTime start = newDate;
			DateTime end = newDate.AddDays(1D);
			int shift = 60;

			DaylightSavingTime dts = GetDST(row.TimeZone, newDate.Year);
			if (dts != null && dts.IsActive)
			{
				start = dts.Start;
				end = dts.End;
				shift = (int)dts.DaylightOffset.TotalMinutes;
			}

			if (savedRow != null)
			{
				row.Year = savedRow.Year;
				if (row.ToDate == null || row.ToDate == end)
					row.ToDate = savedRow.ToDate;
				if (row.FromDate == null || row.FromDate == start)
					row.FromDate = savedRow.FromDate;
				if (row.Shift == null || row.Shift == shift)
					row.Shift = savedRow.Shift;
			}
			else
			{
				if (row.Year == null) row.Year = newDate.Year;
				if (row.ToDate == null) row.ToDate = end;
				if (row.FromDate == null) row.FromDate = start;
				if (row.Shift == null) row.Shift = shift;
			}
		}

		protected virtual void DaylightShift_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			DaylightShift row = e.Row as DaylightShift;
			if (row == null || row.Year == null || row.TimeZone == null || row.IsActive == true) return;

			DaylightSavingTime dts = GetDST(row.TimeZone, (int)row.Year);
			if ((dts != null && dts.IsActive 
				&& row.FromDate == dts.Start && row.ToDate == dts.End && row.Shift == (int)dts.DaylightOffset.TotalMinutes)
				||
				(row.FromDate == null && row.ToDate == null && row.Shift == null))
			{
				DaylightShift savedRow = PXSelectReadonly<DaylightShift,
					Where<DaylightShift.year, Equal<Current<DaylightShiftFilter.year>>,
						And<DaylightShift.timeZone, Equal<Required<DaylightShift.timeZone>>>>>.SelectWindowed(this, 0, 1, row.TimeZone);
				row.FromDate = savedRow.FromDate;
				row.ToDate = savedRow.ToDate;
				row.Shift = savedRow.Shift;
			}
		}

		protected virtual void DaylightShift_ToDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as DaylightShift;
			if (row == null) return;
			if (row.ToDate == null && row.IsActive == true)
				throw new PXSetPropertyException<DaylightShift.toDate>(Messages.CannotBeEmpty);
		}

		protected virtual void DaylightShift_FromDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as DaylightShift;
			if (row == null) return;
			if (row.FromDate == null && row.IsActive == true)
				throw new PXSetPropertyException<DaylightShift.fromDate>(Messages.CannotBeEmpty);
		}

		protected virtual void DaylightShift_Shift_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as DaylightShift;
			if (row == null) return;
			if (row.Shift == null && row.IsActive == true)
				throw new PXSetPropertyException<DaylightShift.shift>(Messages.CannotBeEmpty);
		}

		protected virtual void DaylightShift_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as DaylightShift;
			if (row == null) return;

			PXTimeZoneInfo timeZoneInfo = (PXTimeZoneInfo)_timeZonesHashtable[row.TimeZone];
			row.TimeZoneDescription = PXMessages.LocalizeFormatNoPrefix(timeZoneInfo.DisplayName);

			var isEditable = row.IsActive == true;
			PXUIFieldAttribute.SetEnabled<DaylightShift.toDate>(sender, row, isEditable);
			PXUIFieldAttribute.SetEnabled<DaylightShift.fromDate>(sender, row, isEditable);
			PXUIFieldAttribute.SetEnabled<DaylightShift.shift>(sender, row, isEditable);

			if (isEditable)
			{
				if (row.ToDate != null && row.FromDate != null && ((DateTime)row.ToDate) <= ((DateTime)row.FromDate))
					sender.RaiseExceptionHandling<DaylightShift.toDate>(row, row.ToDate, new PXSetPropertyException(Messages.IncorrectFromDate));
			}
		}

		#endregion
	}
}
