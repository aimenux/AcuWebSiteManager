using System;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;

namespace PX.Objects.PR
{
	public class PRPayGroupYearSetupMaint : YearSetupGraph<PRPayGroupYearSetupMaint, PRPayGroupYearSetup, PRPayGroupPeriodSetup, Where<PRPayGroupPeriodSetup.payGroupID, Equal<Current<PRPayGroupYearSetup.payGroupID>>>>
	{
		protected virtual IEnumerable fiscalYearSetup()
		{
			return PXSelectJoin<PRPayGroupYearSetup, LeftJoin<PRPayGroup, On<PRPayGroupYearSetup.payGroupID, Equal<PRPayGroup.payGroupID>>>>.Select(this).RowCast<PRPayGroupYearSetup>();
		}

		#region Overrided functions
		protected override bool IsFiscalYearSetupExists()
		{
			return false;
		}

		protected override bool IsFiscalYearExists()
		{
			PXSelectBase select = new PXSelect<PRPayGroupYear, Where<PRPayGroupYear.payGroupID, Equal<Current<PRPayGroupYearSetup.payGroupID>>>>(this);
			Object result = select.View.SelectSingle();
			return (result != null);
		}

		protected override bool CheckForPartiallyDefinedYear()
		{
			return SelectFrom<PRPayGroupYear>
				.Where<PRPayGroupYear.payGroupID.IsEqual<PRPayGroupYearSetup.payGroupID.FromCurrent>
					.And<PRPayGroupYear.periodsFullyCreated.IsEqual<False>>>.View.Select(this).Any();
		}

		protected override bool AllowDeleteYearSetup(out string errMsg)
		{
			errMsg = Messages.PayGroupPeriodsDefined;
			return !IsFiscalYearExists();
		}

		protected override bool AllowSetupModification(PRPayGroupYearSetup aRow)
		{
			bool isBookPresent = FiscalYearSetup.Current.PayGroupID != null;
			return isBookPresent;
		}
		protected override void ResetPeriods(PRPayGroupYearSetup row, bool skipDatesRecalc)
		{
			base.ResetPeriods(row, skipDatesRecalc);

			if (PayPeriodType.IsSemiMonth(row.PeriodType))
			{
				row.FinPeriods = 24;
			}
		}
		#endregion

		#region Actions

		public PXAction<PRPayGroupYearSetup> DeleteGeneratedPeriods;
		[PXUIField(DisplayName = Messages.DeleteGeneratedPeriods, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable deleteGeneratedPeriods(PXAdapter adapter)
		{
			PRPayGroupYearSetup year = FiscalYearSetup.Current;
			PXLongOperation.StartOperation(this, delegate
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					PXDatabase.Delete<PRPayGroupYear>(new PXDataFieldRestrict("PayGroupID", PXDbType.NVarChar, 15, year.PayGroupID, PXComp.EQ));
					PXDatabase.Delete<PRPayGroupPeriod>(new PXDataFieldRestrict("PayGroupID", PXDbType.NVarChar, 15, year.PayGroupID, PXComp.EQ));
					ts.Complete();
				}
			});
			return adapter.Get();
		}

		#endregion

		#region Row Events

		protected override void TYearSetupOnRowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			base.TYearSetupOnRowSelected(cache, e);
			bool isCustomPeriods = PayPeriodType.IsCustom(row);

			bool allowChangeStartingDate = this.AllowAlterStartDate();
			bool allowAnyModification = this.AllowSetupModification(row);
			var isFixedLengthPeriod = FiscalPeriodSetupCreator.IsFixedLengthPeriod(row.FPType);
			var isBiWeekly = row.FPType == FiscalPeriodSetupCreator.FPType.BiWeek;
			var isSemiMonth = PayPeriodType.IsSemiMonth(row.PeriodType);

			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.begFinYear>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.hasAdjustmentPeriod>(cache, row, false);

			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.firstFinYear>(cache, row, allowChangeStartingDate && allowAnyModification);

			PXUIFieldAttribute.SetVisible<PRPayGroupYearSetup.endYearDayOfWeek>(cache, row, isFixedLengthPeriod);
			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.endYearDayOfWeek>(cache, row, isFixedLengthPeriod && allowChangeStartingDate && allowAnyModification);

			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.transactionsStartDate>(cache, row, !isFixedLengthPeriod && allowChangeStartingDate && allowAnyModification);
			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.periodsStartDate>(cache, row, !isFixedLengthPeriod && allowChangeStartingDate && allowAnyModification);

			PXUIFieldAttribute.SetVisible<PRPayGroupYearSetup.secondTransactionsStartDate>(cache, row, isSemiMonth && allowChangeStartingDate && allowAnyModification);
			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.secondTransactionsStartDate>(cache, row, isSemiMonth && allowChangeStartingDate && allowAnyModification);
			PXDefaultAttribute.SetPersistingCheck<PRPayGroupYearSetup.secondTransactionsStartDate>(cache, row, isSemiMonth ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetVisible<PRPayGroupYearSetup.secondPeriodsStartDate>(cache, row, isSemiMonth && allowChangeStartingDate && allowAnyModification);
			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.secondPeriodsStartDate>(cache, row, isSemiMonth && allowChangeStartingDate && allowAnyModification);
			PXDefaultAttribute.SetPersistingCheck<PRPayGroupYearSetup.secondPeriodsStartDate>(cache, row, isSemiMonth ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXUIFieldAttribute.SetVisible<PRPayGroupYearSetup.tranDayOfWeek>(cache, row, isFixedLengthPeriod);
			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.tranDayOfWeek>(cache, row, isFixedLengthPeriod && allowChangeStartingDate && allowAnyModification);

			PXUIFieldAttribute.SetVisible<PRPayGroupYearSetup.tranWeekDiff>(cache, row, isFixedLengthPeriod);
			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.tranWeekDiff>(cache, row, isFixedLengthPeriod && allowChangeStartingDate && allowAnyModification);

			PXUIFieldAttribute.SetVisible<PRPayGroupYearSetup.isSecondWeekOfYear>(cache, row, isBiWeekly);
			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.isSecondWeekOfYear>(cache, row, isBiWeekly && allowChangeStartingDate && allowAnyModification);

			PXUIFieldAttribute.SetEnabled<PRPayGroupYearSetup.finPeriods>(cache, row, !isFixedLengthPeriod && allowChangeStartingDate && allowAnyModification && (isCustomPeriods && !this.HasPartiallyDefinedYear && !PayPeriodType.IsSemiMonth(row.PeriodType)));

			PXUIFieldAttribute.SetEnabled<PRPayGroupPeriodSetup.endDateUI>(this.Periods.Cache, null, !isFixedLengthPeriod && isCustomPeriods);
			PXUIFieldAttribute.SetEnabled<PRPayGroupPeriodSetup.transactionDate>(this.Periods.Cache, null, !isFixedLengthPeriod && isCustomPeriods);
		}

		protected override void TYearSetupOnRowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			base.TYearSetupOnRowInserting(cache, e);

			var firstDayOfYear = GetFirstDayOfYear(row.FirstFinYear);

			row.BegFinYear = firstDayOfYear;
			row.PeriodsStartDate = firstDayOfYear;
			row.TransactionsStartDate = firstDayOfYear;
		}

		protected override void TPeriodSetupOnRowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			base.TPeriodSetupOnRowInserting(cache, e);

			var row = (PRPayGroupPeriodSetup)e.Row;
			if (row != null)
			{
				var currentYearSetup = this.FiscalYearSetup.Current;

				int periodNbr;
				if (PayPeriodType.IsSemiMonth(currentYearSetup.PeriodType) && int.TryParse(row.PeriodNbr, out periodNbr))
				{
					var isFirstPeriod = periodNbr % 2 != 0;

					if (isFirstPeriod)
					{
						row.EndDate = CalculateFixPeriodSetupTransactionDate(row.StartDate, currentYearSetup.PeriodsStartDate, currentYearSetup.SecondPeriodsStartDate);
						row.TransactionDate = CalculateFixPeriodSetupTransactionDate(row.StartDate, currentYearSetup.PeriodsStartDate, currentYearSetup.TransactionsStartDate);
					}
					else
					{
						row.EndDate = CalculateFixPeriodSetupTransactionDate(row.StartDate, currentYearSetup.SecondPeriodsStartDate, currentYearSetup.PeriodsStartDate?.AddMonths(1));
						row.TransactionDate = CalculateFixPeriodSetupTransactionDate(row.StartDate, currentYearSetup.SecondPeriodsStartDate, currentYearSetup.SecondTransactionsStartDate);
					}

					row.Descr = GetSemiMonthlyDescription(row.StartDate, row.EndDate);
				}
				else
				{
					row.TransactionDate = CalculateFixPeriodSetupTransactionDate(row.StartDate, currentYearSetup.PeriodsStartDate, currentYearSetup.TransactionsStartDate);
				}
			}
		}

		protected virtual void _(Events.RowPersisting<PRPayGroupYearSetup> e)
		{
			if (this.AutoFill.GetEnabled() && !Periods.Select().FirstTableItems.Any())
			{
				PXUIFieldAttribute.SetError<PRPayGroupYearSetup.finPeriods>(e.Cache, e.Row, Messages.PeriodsNotGenerated, e.Row.FinPeriods.ToString());
			}
		}

		#endregion

		#region Field Events

		#region Field Defaulting

		protected virtual void PRPayGroupYearSetup_BegFinYear_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null || string.IsNullOrEmpty(row.PeriodType))
				return;

			if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(row.FPType))
			{
				e.NewValue = CalculateWeeklyBegFinYear(row.PeriodsStartDate, row.TransactionsStartDate, row.PeriodLength);
			}
			else
			{
				e.NewValue = row.PeriodsStartDate;
			}
		}

		protected virtual void PRPayGroupYearSetup_TransactionsStartDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null || string.IsNullOrEmpty(row.PeriodType))
				return;

			if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(row.FPType))
			{
				e.NewValue = GetFirstDayOfYearOccuringOnDayOfWeek(row.FirstFinYear, row.TranDayOfWeek, row.IsSecondWeekOfYear);
			}
			else
			{
				e.NewValue = GetFirstDayOfYear(row.FirstFinYear);
			}
		}

		protected virtual void PRPayGroupYearSetup_SecondTransactionsStartDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null || string.IsNullOrEmpty(row.PeriodType))
				return;

			if (PayPeriodType.IsSemiMonth(row.PeriodType))
			{
				e.NewValue = row.TransactionsStartDate?.AddDays(14);
			}
		}

		protected override void TYearSetupOnPeriodsStartDateFieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null || string.IsNullOrEmpty(row.PeriodType))
				return;

			if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(row.FPType))
			{
				e.NewValue = CalculatePeriodsStartDate(row.TransactionsStartDate, row.EndYearDayOfWeek, row.TranWeekDiff);
			}
			else
			{
				e.NewValue = GetFirstDayOfYear(row.FirstFinYear);
			}
		}

		protected virtual void PRPayGroupYearSetup_SecondPeriodsStartDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null || string.IsNullOrEmpty(row.PeriodType))
				return;

			if (PayPeriodType.IsSemiMonth(row.PeriodType))
			{
				e.NewValue = row.PeriodsStartDate?.AddDays(14);
			}
		}

		#endregion

		#region Field Verifying

		protected override void TYearSetupOnPeriodsStartDateFieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			var newDate = (DateTime?)e.NewValue;
			if (row == null || string.IsNullOrEmpty(row.PeriodType))
				return;

			if (!newDate.HasValue)
			{
				e.NewValue = row.PeriodsStartDate;
				e.Cancel = true;
			}
			else
			{
				if (!FiscalPeriodSetupCreator.IsFixedLengthPeriod(row.FPType))
				{
					base.TYearSetupOnPeriodsStartDateFieldVerifying(cache, e);
				}
			}
		}

		protected virtual void PRPayGroupYearSetup_SecondPeriodsStartDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			var newDate = (DateTime?)e.NewValue;
			if (row == null || !PayPeriodType.IsSemiMonth(row.PeriodType) || !row.PeriodsStartDate.HasValue)
				return;

			if (!newDate.HasValue)
			{
				e.NewValue = row.SecondPeriodsStartDate;
				e.Cancel = true;
			}
			else
			{
				var dayDiff = (newDate - row.PeriodsStartDate.Value).Value.Days;
				if (dayDiff < 10 || dayDiff > 20)
					throw new PXSetPropertyException(Messages.CalendarSemiMonthlySecondPeriodsOutOfRange);
			}
		}

		protected virtual void PRPayGroupYearSetup_TransactionsStartDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;

			var newDate = (DateTime?)e.NewValue;
			if (row == null)
				return;

			if (!newDate.HasValue)
			{
				e.NewValue = row.TransactionsStartDate;
				e.Cancel = true;
			}
			else
			{
				var yearNbr = GetYearNbr(row.FirstFinYear);
				if (!yearNbr.HasValue)
					return;

				if (newDate.Value.Year != yearNbr)
					throw new PXSetPropertyException(Messages.CalendarTransactionsDateOtherYear);

				if (!FiscalPeriodSetupCreator.IsFixedLengthPeriod(row.FPType))
				{
					if ((row.FPType == FiscalPeriodSetupCreator.FPType.Month || PayPeriodType.IsSemiMonth(row.PeriodType)) && newDate.Value.Month > 1)
						throw new PXSetPropertyException(Messages.CalendarMonthlyTransactionsDateOutOfRange);
					else if (row.FPType == FiscalPeriodSetupCreator.FPType.BiMonth && newDate.Value.Month > 2)
						throw new PXSetPropertyException(Messages.CalendarBiMonthlyTransactionsDateOutOfRange);
					else if (row.FPType == FiscalPeriodSetupCreator.FPType.Quarter && newDate.Value.Month > 3)
						throw new PXSetPropertyException(Messages.CalendarQuarterTransactionsDateOutOfRange);
					//If 1 period, the year validation will take care of it. If we let that validation run, we won't be able to select the last day of the year.
					else if (PayPeriodType.IsCustom(row) && row.FinPeriods > 1)
					{
						var maxDayOfYear = (short)(Math.Floor((DateTime.IsLeapYear(yearNbr.Value) ? 366m : 365m) / row.FinPeriods.Value));
						if (newDate.Value.DayOfYear >= maxDayOfYear)
							throw new PXSetPropertyException(Messages.CalendarCustomTransactionsDateOutOfRange, new DateTime(yearNbr.Value, 1, 1).AddDays(maxDayOfYear - 1));
					}
				}
			}
		}

		protected virtual void PRPayGroupYearSetup_SecondTransactionsStartDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			var newDate = (DateTime?)e.NewValue;
			if (row == null || !PayPeriodType.IsSemiMonth(row.PeriodType) || !row.TransactionsStartDate.HasValue)
				return;

			if (!newDate.HasValue)
			{
				e.NewValue = row.SecondTransactionsStartDate;
				e.Cancel = true;
			}
			else
			{
				var dayDiff = (newDate - row.TransactionsStartDate.Value).Value.Days;
				if (newDate.Value.Month > 1)
					throw new PXSetPropertyException(Messages.CalendarMonthlyTransactionsDateOutOfRange);
				else if (dayDiff < 10 || dayDiff > 20)
					throw new PXSetPropertyException(Messages.CalendarSemiMonthlySecondTransactionsOutOfRange);
			}
		}

		protected virtual void PRPayGroupPeriodSetup_TransactionDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var row = (PRPayGroupPeriodSetup)e.Row;
			var yearSetup = this.FiscalYearSetup.Current;
			var newDate = (DateTime?)e.NewValue;
			if (row == null || yearSetup == null)
				return;

			if (!newDate.HasValue)
			{
				e.NewValue = row.TransactionDate;
				e.Cancel = true;
			}
			else
			{
				var yearNbr = GetYearNbr(yearSetup.FirstFinYear);
				if (!yearNbr.HasValue)
					return;

				if (newDate.Value.Year != yearNbr)
					throw new PXSetPropertyException(Messages.CalendarTransactionsDateOtherYear);
			}
		}

		#endregion

		#region Field Udpated

		protected virtual void PRPayGroupYearSetup_FirstFinYear_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			ResetDates(cache, row);
		}

		protected virtual void PRPayGroupYearSetup_PeriodsStartDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			cache.SetDefaultExt<PRPayGroupYearSetup.begFinYear>(row);

			ResetPeriods(row, true);
		}

		protected virtual void PRPayGroupYearSetup_TransactionsStartDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			ResetPeriods(row, true);
		}
		protected virtual void PRPayGroupYearSetup_FinPeriods_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			ResetPeriods(row, true);
		}

		protected virtual void PRPayGroupYearSetup_TranDayOfWeek_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			ResetDates(cache, row);
		}
		protected virtual void PRPayGroupYearSetup_TranWeekDiff_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			ResetDates(cache, row);
		}

		protected virtual void PRPayGroupYearSetup_IsSecondWeekOfYear_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			ResetDates(cache, row);
		}

		protected override void TYearSetupOnPeriodTypeFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			row.AdjustToPeriodStart = false;
			row.IsSecondWeekOfYear = false;
			base.TYearSetupOnPeriodTypeFieldUpdated(cache, e);

			ResetDates(cache, row);
		}

		protected override void TYearSetupOnEndYearDayOfWeekFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = (PRPayGroupYearSetup)e.Row;
			if (row == null)
				return;

			ResetDates(cache, row);
		}

		protected override void TYearSetupOnBegFinYearFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			//Do Nothing
		}

		protected override void TYearSetupOnHasAdjustmentPeriodFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			//Do Nothing
		}

		protected override void TYearSetupOnBelongsToNextYearFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			//Do Nothing
		}

		protected override void TYearSetupOnEndYearCalcMethodFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			//Do Nothing
		}

		#endregion

		#region Bulk Setters

		public virtual void ResetDates(PXCache cache, PRPayGroupYearSetup row)
		{
			cache.SetDefaultExt<PRPayGroupYearSetup.transactionsStartDate>(row);
			cache.SetDefaultExt<PRPayGroupYearSetup.secondTransactionsStartDate>(row);
			cache.SetDefaultExt<PRPayGroupYearSetup.periodsStartDate>(row);
			cache.SetDefaultExt<PRPayGroupYearSetup.secondPeriodsStartDate>(row);
			cache.SetDefaultExt<PRPayGroupYearSetup.begFinYear>(row);
		}

		#endregion

		#endregion

		#region Static Calculations

		public static DateTime? CalculateFixLengthTransactionDate(DateTime? startDate, DateTime? setupStartDate, DateTime? setupTransactionDate)
		{
			if (!startDate.HasValue || !setupStartDate.HasValue || !setupTransactionDate.HasValue)
				return null;

			return startDate.Value.AddDays((setupTransactionDate.Value - setupStartDate.Value).Days);
		}

		public static DateTime? CalculateFixPeriodSetupTransactionDate(DateTime? startDate, DateTime? setupStartDate, DateTime? setupTransactionDate)
		{
			if (!startDate.HasValue || !setupStartDate.HasValue || !setupTransactionDate.HasValue)
				return null;

			var diffMonth = GetMonthDifference(startDate.Value, setupStartDate.Value);
			return AdjustDateForEndOfMonth(setupTransactionDate.Value, setupTransactionDate.Value.AddMonths(diffMonth));
		}

		public static DateTime? CalculateTransactionDateFromTemplate(string periodFinYear, string setupFinYear, DateTime? setupTransactionDate)
		{
			var periodYearNbr = GetYearNbr(periodFinYear);
			var setupYearNbr = GetYearNbr(setupFinYear);
			if (!periodYearNbr.HasValue || !setupYearNbr.HasValue || !setupTransactionDate.HasValue)
				return null;

			return AdjustDateForEndOfMonth(setupTransactionDate.Value, setupTransactionDate.Value.AddYears(periodYearNbr.Value - setupYearNbr.Value));
		}

		public static DateTime? CalculatePeriodsStartDate(DateTime? transactionsStartDate, int? endYearDayOfWeek, int? tranWeekDiff)
		{
			if (!transactionsStartDate.HasValue || !endYearDayOfWeek.HasValue)
				return null;

			int daysToAdd = 0;
			daysToAdd = endYearDayOfWeek.Value - OneBasedDayOfWeek.GetOneBasedDayOfWeek(transactionsStartDate.Value.DayOfWeek);
			if (daysToAdd > 0)
				daysToAdd -= 7;

			if (tranWeekDiff.HasValue)
				daysToAdd -= tranWeekDiff.Value * 7;

			return transactionsStartDate.Value.AddDays(daysToAdd);
		}

		public static DateTime? CalculateWeeklyBegFinYear(DateTime? periodsStartDate, DateTime? transactionsStartDate, int? customPeriodLength)
		{
			if (!periodsStartDate.HasValue || !transactionsStartDate.HasValue)
				return null;

			var firstDayOfYear = new DateTime(transactionsStartDate.Value.Year, 1, 1);
			var dayDiff = (transactionsStartDate.Value - firstDayOfYear).Days;
			return periodsStartDate.Value.AddDays(Math.Floor(customPeriodLength.Value / 2.0) - dayDiff);
		}

		public static DateTime AdjustDateForEndOfMonth(DateTime originalDate, DateTime newDate)
		{
			//If is last day of month, force the date to be on the last day of the new month.
			if (originalDate.Day == DateTime.DaysInMonth(originalDate.Year, originalDate.Month))
			{
				newDate = new DateTime(newDate.Year, newDate.Month, DateTime.DaysInMonth(newDate.Year, newDate.Month));
			}

			return newDate;
		}

		public static DateTime? GetFirstDayOfYearOccuringOnDayOfWeek(string year, int? dayOfWeek, bool? isSecondWeekOfYear)
		{
			var firstDayForYear = GetFirstDayOfYear(year);
			if (!firstDayForYear.HasValue || !dayOfWeek.HasValue)
				return null;

			//Our list goes from 1 (Sunday) to 7 (saturday) while the   
			//standard dotnet one ranges 0 (Sunday) to 6 (Saturday).
			var dayOfWeekDateTimeFormat = dayOfWeek.Value - 1;

			var start = new DateTime(firstDayForYear.Value.Year, 1, 1);
			int daysToAdd = (dayOfWeekDateTimeFormat - (int)start.DayOfWeek + 7) % 7;

			//For Biweekly payroll only. Period are not always the same and may fall 
			//on different intervals depending on the company.
			if (isSecondWeekOfYear ?? false)
				daysToAdd += 7;

			return start.AddDays(daysToAdd);
		}

		public static DateTime? GetFirstDayOfYear(string year)
		{
			var yearNbr = GetYearNbr(year);
			if (!yearNbr.HasValue)
				return null;

			return new DateTime(yearNbr.Value, 1, 1);
		}

		public static int? GetYearNbr(string year)
		{
			int yearNbr;
			if (!int.TryParse(year, out yearNbr) || yearNbr < 0 || yearNbr > 9999)
				return null;

			return yearNbr;
		}

		public static int GetMonthDifference(DateTime lValue, DateTime rValue)
		{
			return (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
		}

		public static string GetSemiMonthlyDescription(DateTime? startDate, DateTime? endDate)
		{
			if (!startDate.HasValue || !endDate.HasValue)
				return null;

			var halfDayOfMonth = new DateTime(startDate.Value.Year, startDate.Value.Month, DateTime.DaysInMonth(startDate.Value.Year, startDate.Value.Month) / 2);
			var firstDayOfNextMonth = new DateTime(startDate.Value.Year, startDate.Value.Month, 1).AddMonths(1);

			DateTime firstMonthApplicableMonth;
			TimeSpan firstHalfOverlap;
			TimeSpan secondHalfOverlap;
			if (startDate.Value < halfDayOfMonth)
			{
				firstMonthApplicableMonth = halfDayOfMonth;
				firstHalfOverlap = halfDayOfMonth - startDate.Value;
				secondHalfOverlap = endDate.Value - halfDayOfMonth;
			}
			else
			{
				firstMonthApplicableMonth = firstDayOfNextMonth;
				firstHalfOverlap = endDate.Value - firstDayOfNextMonth;
				secondHalfOverlap = firstDayOfNextMonth - startDate.Value;
			}

			if (firstHalfOverlap > secondHalfOverlap)
				return PXMessages.LocalizeFormatNoPrefix(Messages.SemiMonthlyFirstHalfDescr, firstMonthApplicableMonth);
			else
				return PXMessages.LocalizeFormatNoPrefix(Messages.SemiMonthlySecondHalfDescr, halfDayOfMonth);
		}

		#endregion

	}
}