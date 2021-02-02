using System;
using System.Collections;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.CT;
using PX.Objects.CR;

namespace PX.Objects.CS
{
	public class CSCalendarMaint : PXGraph<CSCalendarMaint, CSCalendar>
	{
		[Serializable]
		[PXHidden]
		public partial class CSCalendarExceptionsParamsParameters : IBqlTable
		{
			#region YearID
			public abstract class yearID : PX.Data.BQL.BqlInt.Field<yearID> { }
			protected Int32? _YearID;
			[PXInt()]
			[PXUIField(DisplayName = "Year", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search4<CSCalendarExceptions.yearID,
				Where<CSCalendarExceptions.calendarID, Equal<Current<CSCalendar.calendarID>>>,
				Aggregate<GroupBy<CSCalendarExceptions.yearID>>>))]
			public virtual Int32? YearID
			{
				get
				{
					return this._YearID;
				}
				set
				{
					this._YearID = value;
				}
			}
			#endregion
		}

		public PXSelect<CSCalendar> Calendar;
		public PXSelect<CSCalendar,
					Where<CSCalendar.calendarID, Equal<Current<CSCalendar.calendarID>>>> CalendarDetails;
		public PXFilter<CSCalendarExceptionsParamsParameters> Filter;
		public PXSelect<CSCalendarExceptions> CSCalendarExceptions;

		protected virtual void CSCalendarExceptions_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CSCalendarExceptions row = (CSCalendarExceptions)e.Row;
			if (row.CalendarID == null && Calendar.Current != null)
			{
				row.CalendarID = Calendar.Current.CalendarID;
			}
			if (row.Date.HasValue)
			{
				row.YearID = row.Date.Value.Year;
				row.DayOfWeek = (int)row.Date.Value.DayOfWeek + 1;
			}
			else
			{
				row.YearID = Accessinfo.BusinessDate.Value.Year;
				row.DayOfWeek = (int)Accessinfo.BusinessDate.Value.DayOfWeek + 1;
			}
		}

		protected virtual void CSCalendarExceptions_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			CSCalendarExceptions row = (CSCalendarExceptions)e.NewRow;

			if (row.Date.HasValue)
			{
				row.YearID = row.Date.Value.Year;
				row.DayOfWeek = (int)row.Date.Value.DayOfWeek + 1;
			}
			else
			{
				row.YearID = Accessinfo.BusinessDate.Value.Year;
				row.DayOfWeek = (int)Accessinfo.BusinessDate.Value.DayOfWeek + 1;
			}
		}

		protected virtual void CSCalendar_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			CSCalendar calendar = (CSCalendar)e.NewRow;

			if (calendar == null)
				return;

			CalendarHelper.EnsureUnpaidTimeValid(calendar);
		}

		protected virtual void CSCalendar_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CSCalendar row = e.Row as CSCalendar;
			if (row != null)
			{
				EPEmployeeClass refEmpClass = PXSelect<EPEmployeeClass, Where<EPEmployeeClass.calendarID, Equal<Current<CSCalendar.calendarID>>>>.SelectWindowed(this, 0, 1);
				if (refEmpClass != null)
				{
					e.Cancel = true;
					throw new PXException(Messages.ReferencedByEmployeeClass, refEmpClass.VendorClassID);
				}

				Carrier refCarrier = PXSelect<Carrier, Where<Carrier.calendarID, Equal<Current<CSCalendar.calendarID>>>>.SelectWindowed(this, 0, 1);
				if (refCarrier != null)
				{
					e.Cancel = true;
					throw new PXException(Messages.ReferencedByCarrier, refCarrier.CarrierID);
				}

				Contract refContract = PXSelect<Contract, Where<Contract.calendarID, Equal<Current<CSCalendar.calendarID>>>>.SelectWindowed(this, 0, 1);
				if (refContract != null)
				{
					e.Cancel = true;
					throw new PXException(Messages.ReferencedByContract, refContract.ContractID);
				}

				EPEmployee refEmployee = PXSelect<EPEmployee, Where<EPEmployee.calendarID, Equal<Current<CSCalendar.calendarID>>>>.SelectWindowed(this, 0, 1);
				if (refEmployee != null)
				{
					e.Cancel = true;
					throw new PXException(Messages.ReferencedByEmployee, refEmployee.ClassID);
				}
			}
		}

		protected virtual void CSCalendar_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CSCalendar row = (CSCalendar)e.Row;
			PXDefaultAttribute.SetPersistingCheck<CSCalendar.sunStartTime>(sender, e.Row, row.SunWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CSCalendar.sunEndTime>(sender, e.Row, row.SunWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<CSCalendar.monStartTime>(sender, e.Row, row.MonWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CSCalendar.monEndTime>(sender, e.Row, row.MonWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<CSCalendar.tueStartTime>(sender, e.Row, row.TueWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CSCalendar.tueEndTime>(sender, e.Row, row.TueWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<CSCalendar.wedStartTime>(sender, e.Row, row.WedWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CSCalendar.wedEndTime>(sender, e.Row, row.WedWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<CSCalendar.thuStartTime>(sender, e.Row, row.ThuWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CSCalendar.thuEndTime>(sender, e.Row, row.ThuWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<CSCalendar.friStartTime>(sender, e.Row, row.FriWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CSCalendar.friEndTime>(sender, e.Row, row.FriWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<CSCalendar.satStartTime>(sender, e.Row, row.SatWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CSCalendar.satEndTime>(sender, e.Row, row.SatWorkDay == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}

		protected virtual void CSCalendar_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			CSCalendar row = (CSCalendar)e.Row;

			PXUIFieldAttribute.SetEnabled<CSCalendar.sunStartTime>(Calendar.Cache, row, row.SunWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.sunEndTime>(Calendar.Cache, row, row.SunWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.sunUnpaidTime>(Calendar.Cache, row, row.SunWorkDay ?? false);

			PXUIFieldAttribute.SetEnabled<CSCalendar.monStartTime>(Calendar.Cache, row, row.MonWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.monEndTime>(Calendar.Cache, row, row.MonWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.monUnpaidTime>(Calendar.Cache, row, row.MonWorkDay ?? false);

			PXUIFieldAttribute.SetEnabled<CSCalendar.tueStartTime>(Calendar.Cache, row, row.TueWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.tueEndTime>(Calendar.Cache, row, row.TueWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.tueUnpaidTime>(Calendar.Cache, row, row.TueWorkDay ?? false);

			PXUIFieldAttribute.SetEnabled<CSCalendar.wedStartTime>(Calendar.Cache, row, row.WedWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.wedEndTime>(Calendar.Cache, row, row.WedWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.wedUnpaidTime>(Calendar.Cache, row, row.WedWorkDay ?? false);

			PXUIFieldAttribute.SetEnabled<CSCalendar.thuStartTime>(Calendar.Cache, row, row.ThuWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.thuEndTime>(Calendar.Cache, row, row.ThuWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.thuUnpaidTime>(Calendar.Cache, row, row.ThuWorkDay ?? false);

			PXUIFieldAttribute.SetEnabled<CSCalendar.friStartTime>(Calendar.Cache, row, row.FriWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.friEndTime>(Calendar.Cache, row, row.FriWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.friUnpaidTime>(Calendar.Cache, row, row.FriWorkDay ?? false);

			PXUIFieldAttribute.SetEnabled<CSCalendar.satStartTime>(Calendar.Cache, row, row.SatWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.satEndTime>(Calendar.Cache, row, row.SatWorkDay ?? false);
			PXUIFieldAttribute.SetEnabled<CSCalendar.satUnpaidTime>(Calendar.Cache, row, row.SatWorkDay ?? false);
		}

		protected virtual IEnumerable cSCalendarExceptions()
		{
			CSCalendarExceptionsParamsParameters header = Filter.Current;
			if (header == null)
			{
				yield break;
			}

			foreach (CSCalendarExceptions calend in PXSelect<CSCalendarExceptions,
				Where<CSCalendarExceptions.calendarID, Equal<Current<CSCalendar.calendarID>>>>
				.Select(this))
			{
				if ((header.YearID.HasValue && header.YearID == calend.YearID) ||
					header.YearID.HasValue == false)
				{
					yield return calend;
				}
			}
		}
	}
}
