using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.FA;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.GL
{
	public class FiscalYearSetupMaint : YearSetupGraph<FiscalYearSetupMaint, FinYearSetup, FinPeriodSetup>
	{
		#region Implementation

		protected override bool IsFiscalYearExists()
		{
			return PXSelect<MasterFinYear>
				.SelectSingleBound(this, new object[] { })
				.RowCast<MasterFinYear>()
				.Any();
		}

		protected override bool IsFiscalYearSetupExists()
		{
			return PXSelectGroupBy<FinYearSetup, Aggregate<Count>>.Select(this).RowCount > 0;
		}

		protected override bool CheckForPartiallyDefinedYear()
		{
			//Validates if there are FiscalYears with incompleted set of periods periods - where not all the periods are defined;
			MasterFinYear unclosedYear = null;
			foreach (MasterFinYear year in PXSelect<MasterFinYear>.Select(this))
			{
				int count = 0;
				foreach (MasterFinPeriod fp in PXSelect<MasterFinPeriod, Where<MasterFinPeriod.finYear, Equal<Required<MasterFinYear.year>>>>.Select(this, year.Year))
				{
					count++;
				}
				if (count < year.FinPeriods)
				{
					unclosedYear = year;
					break;
				}
			}
			return (unclosedYear != null);
		}

		protected override bool AllowDeleteYearSetup(out string errMsg)
		{
			errMsg = null;
			if (IsFiscalYearExists())
			{
				errMsg = Messages.FiscalPeriodsDefined;
			}
			else
			{
				PXSelectBase select = new PXSelect<FABookYear>(this);
				Object result = select.View.SelectSingle();
				if (result != null)
				{
					errMsg = Messages.FABookPeriodsDefined;
				}
			}

			return (errMsg == null);
		}

		#endregion
	}

	public interface IYear
	{
		String Year
		{
			get;
			set;
		}
		Int16? FinPeriods
		{
			get;
			set;
		}
		DateTime? StartDate
		{
			get;
			set;
		}
		DateTime? EndDate
		{
			get;
			set;
		}
	}

	public interface IFinYear : IYear
	{
		int? OrganizationID
		{
			get;
			set;
		}
	}

		public interface IYearSetup
	{
		String FirstFinYear
		{
			get;
			set;
		}
		DateTime? BegFinYear
		{
			get;
			set;
		}
		DateTime? PeriodsStartDate
		{
			get;
			set;
		}
		Int16? FinPeriods
		{
			get;
			set;
		}
		Int16? PeriodLength
		{
			get;
			set;
		}
		Boolean? UserDefined
		{
			get;
			set;
		}
		string PeriodType
		{
			get;
			set;
		}
		bool? HasAdjustmentPeriod
		{
			get;
			set;
		}
		bool? AdjustToPeriodStart
		{
			get;
			set;
		}
		bool? BelongsToNextYear
		{
			get;
			set;
		}

		FiscalPeriodSetupCreator.FPType FPType
		{
			get;
		}

		bool IsFixedLengthPeriod
		{
			get;
		}

		String EndYearCalcMethod
		{
			get;
			set;
		}

		int? EndYearDayOfWeek
		{
			get;
			set;
		}
	}

	public interface IPeriod : IPeriodSetup
	{
		string FinYear
		{
			get;
			set;
		}
		string FinPeriodID
		{
			get;
			set;
		}
		int? OrganizationID { get; set; }

		bool? DateLocked
		{
			get;
			set;
		}
	}

	public interface IFinPeriod : IPeriod
	{
		string Status { get; set; }
	}

	public interface IPeriodSetup
	{
		String PeriodNbr
		{
			get;
			set;
		}
		DateTime? StartDate
		{
			get;
			set;
		}
		DateTime? EndDate
		{
			get;
			set;
		}
		String Descr
		{
			get;
			set;
		}
		Boolean? Custom
		{
			get;
			set;
		}
	}

	public abstract class YearSetupGraph<TGraph, TYearSetup, TPeriodSetup> : YearSetupGraphBase<TGraph, TYearSetup, TPeriodSetup, PXSelect<TPeriodSetup>>
		where TGraph : PXGraph
		where TYearSetup : class, IYearSetup, IBqlTable, new()
		where TPeriodSetup : class, IPeriodSetup, IBqlTable, new()
	{
	}

	public abstract class YearSetupGraph<TGraph, TYearSetup, TPeriodSetup, Where> : YearSetupGraphBase<TGraph, TYearSetup, TPeriodSetup, PXSelect<TPeriodSetup, Where>>
		where TGraph : PXGraph
		where TYearSetup : class, IYearSetup, IBqlTable, new()
		where TPeriodSetup : class, IPeriodSetup, IBqlTable, new()
		where Where : IBqlWhere, new()
	{
	}

	public interface IYearSetupMaintenanceGraph
	{
		void SetCurrentYearSetup(object[] key);
		void ShiftBackFirstYearTo(string yearNumber);
	}

	public abstract class YearSetupGraphBase<TGraph, TYearSetup, TPeriodSetup, TPeriodSetupSelect> : PXGraph<TGraph>, IYearSetupMaintenanceGraph
		where TGraph : PXGraph
		where TYearSetup : class, IYearSetup, IBqlTable, new()
		where TPeriodSetup : class, IPeriodSetup, IBqlTable, new()
		where TPeriodSetupSelect : PXSelectBase<TPeriodSetup>
	{
		public abstract class YearSetup
		{
			public abstract class belongsToNextYear : PX.Data.BQL.BqlBool.Field<belongsToNextYear> { }
			public abstract class begFinYear : PX.Data.BQL.BqlDateTime.Field<begFinYear> { }
			public abstract class finPeriods : PX.Data.BQL.BqlShort.Field<finPeriods> { }
			public abstract class userDefined : PX.Data.BQL.BqlBool.Field<userDefined> { }
			public abstract class adjustToPeriodStart : PX.Data.BQL.BqlBool.Field<adjustToPeriodStart> { }
			public abstract class periodLength : PX.Data.BQL.BqlShort.Field<periodLength> { }
			public abstract class periodsStartDate : PX.Data.BQL.BqlDateTime.Field<periodsStartDate> { }
			public abstract class periodType : PX.Data.BQL.BqlString.Field<periodType> { }
			public abstract class hasAdjustmentPeriod : PX.Data.BQL.BqlBool.Field<hasAdjustmentPeriod> { }
			public abstract class endYearCalcMethod : PX.Data.BQL.BqlString.Field<endYearCalcMethod> { }
			public abstract class endYearDayOfWeek : PX.Data.BQL.BqlInt.Field<endYearDayOfWeek> { }
			public abstract class yearLastDayOfWeek : PX.Data.IBqlField { }
		}

		public abstract class PeriodSetup
		{
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			public abstract class endDateUI : PX.Data.BQL.BqlDateTime.Field<endDateUI> { }
		}

		#region Ctor + public members
		public YearSetupGraphBase()
			: base()
		{
			this.FiscalYearSetup.Cache.AllowDelete = true;

			if (this.IsFiscalYearSetupExists())
			{
				//Prevent insertion of the new record if one exists	
				this.FiscalYearSetup.Cache.AllowInsert = false;
			}

			this.FieldVerifying.AddHandler(typeof(TPeriodSetup), typeof(PeriodSetup.endDate).Name, TPeriodSetupOnEndDateFieldVerifying);
			this.RowDeleted.AddHandler(typeof(TPeriodSetup), TPeriodSetupOnRowDeleted);
			this.RowDeleting.AddHandler(typeof(TPeriodSetup), TPeriodSetupOnRowDeleting);
			this.RowInserted.AddHandler(typeof(TPeriodSetup), TPeriodSetupOnRowInserted);
			this.RowInserting.AddHandler(typeof(TPeriodSetup), TPeriodSetupOnRowInserting);
			this.RowUpdated.AddHandler(typeof(TPeriodSetup), TPeriodSetupOnRowUpdated);

			this.FieldUpdated.AddHandler(typeof(TYearSetup), typeof(YearSetup.begFinYear).Name, TYearSetupOnBegFinYearFieldUpdated);
			this.FieldUpdated.AddHandler(typeof(TYearSetup), typeof(YearSetup.belongsToNextYear).Name, TYearSetupOnBelongsToNextYearFieldUpdated);
			this.FieldUpdated.AddHandler(typeof(TYearSetup), typeof(YearSetup.adjustToPeriodStart).Name, TYearSetupOnAdjustToPeriodStartFieldUpdated);
			this.FieldUpdated.AddHandler(typeof(TYearSetup), typeof(YearSetup.periodType).Name, TYearSetupOnPeriodTypeFieldUpdated);
			this.FieldUpdated.AddHandler(typeof(TYearSetup), typeof(YearSetup.hasAdjustmentPeriod).Name, TYearSetupOnHasAdjustmentPeriodFieldUpdated);
			this.FieldUpdated.AddHandler(typeof(TYearSetup), typeof(YearSetup.endYearDayOfWeek).Name, TYearSetupOnEndYearDayOfWeekFieldUpdated);
			this.FieldUpdated.AddHandler(typeof(TYearSetup), typeof(YearSetup.endYearCalcMethod).Name, TYearSetupOnEndYearCalcMethodFieldUpdated);
			this.FieldVerifying.AddHandler(typeof(TYearSetup), typeof(YearSetup.periodsStartDate).Name, TYearSetupOnPeriodsStartDateFieldVerifying);
			this.FieldDefaulting.AddHandler(typeof(TYearSetup), typeof(YearSetup.periodsStartDate).Name, TYearSetupOnPeriodsStartDateFieldDefaulting);

			this.RowDeleting.AddHandler(typeof(TYearSetup), TYearSetupOnRowDeleting);
			this.RowInserting.AddHandler(typeof(TYearSetup), TYearSetupOnRowInserting);
			this.RowPersisting.AddHandler(typeof(TYearSetup), TYearSetupOnRowPersisting);
			this.RowSelected.AddHandler(typeof(TYearSetup), TYearSetupOnRowSelected);
		}

		public PXSave<TYearSetup> Save;
		public PXCancel<TYearSetup> Cancel;
		public PXDelete<TYearSetup> Delete;

		public PXFirst<TYearSetup> First;
		public PXPrevious<TYearSetup> Previous;
		public PXNext<TYearSetup> Next;
		public PXLast<TYearSetup> Last;

		public PXAction<TYearSetup> AutoFill;
		public PXSelect<TYearSetup> FiscalYearSetup;
		public TPeriodSetupSelect Periods;
		private bool massUpdateFlag = false;

		public PXAction<TYearSetup> decrFirstYear;
		[PXUIField(DisplayName = Messages.DecremenFirstYear, Visible = true)]
		[PXButton]
		protected virtual IEnumerable DecrFirstYear(PXAdapter adapter)
		{
			WebDialogResult answer = this.FiscalYearSetup.Ask(this.FiscalYearSetup.Current,
				Messages.ImportantConfirmation, adapter.View.Graph.GetType() == typeof(FiscalYearSetupMaint) ?
				Messages.FirstFinYearDecrementConfirmation : Messages.FirstFinYearDecrementConfirmationGeneric,
				MessageButtons.YesNo, MessageIcon.Question);
			if (answer == WebDialogResult.Yes)
			{
				return DecrementFirstYear(adapter.Get<TYearSetup>());
			}
			return adapter.Get();
		}

		private IEnumerable<TYearSetup> DecrementFirstYear(IEnumerable<TYearSetup> yearTemplates)
		{
			foreach (TYearSetup year in yearTemplates)
			{

				if (year.BegFinYear != null)
				{
					DoIncrementFirstYear(year, true);
					yield return year;
				}
			}
		}

		public virtual void SetCurrentYearSetup(object[] key = null)
		{
			FiscalYearSetup.Current = FiscalYearSetup.Current ?? FiscalYearSetup.SelectSingle();
		}

		public virtual void ShiftBackFirstYearTo(string yearNumber)
		{
			if(FiscalYearSetup.Current == null)
			{
				throw new ArgumentNullException(nameof(FiscalYearSetup.Current));
			}

			string firstFinYear = FiscalYearSetup.Current.FirstFinYear;
			if (string.Compare(FiscalYearSetup.Current.FirstFinYear, yearNumber) > 0)
			{
				TYearSetup yearSetup = null;
				while (string.CompareOrdinal(FiscalYearSetup.Current.FirstFinYear, yearNumber) > 0)
				{
					yearSetup = DecrementFirstYear(new TYearSetup[] { FiscalYearSetup.Current }).FirstOrDefault();
				}
				Save.Press();
			}
		}

		#endregion

		#region Action deledates

		[PXButton]
		[PXUIField(DisplayName = Messages.GeneratePeriods, MapEnableRights = PXCacheRights.Update)]
		public virtual IEnumerable autoFill(PXAdapter adapter)
		{
			TYearSetup year = (TYearSetup)this.FiscalYearSetup.Current;
			if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(year.FPType))
			{
				throw new PXException(Messages.PeriodTemplatesCanotBeGeneratedForThisPeriodType);
			}
			this.AutoFillPeriods();
			return adapter.Get();
		}

		#endregion

		#region TYearSetupEvents
		protected virtual void TYearSetupOnRowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			TYearSetup fes = (TYearSetup)e.Row;
			if (fes == null) return;

			if (!fes.BelongsToNextYear.HasValue)
			{
				fes.BelongsToNextYear = false;
				if (fes.BegFinYear.HasValue && !string.IsNullOrEmpty(fes.FirstFinYear))
				{
					int firstYear = int.Parse(fes.FirstFinYear);
					fes.BelongsToNextYear = (firstYear == (fes.BegFinYear.Value.Year + 1));
				}
			}
			bool allowChangeStartingDate = this.AllowAlterStartDate();
			bool allowAnyModification = this.AllowSetupModification(fes);

			bool isFixedLengthPeriod = FiscalPeriodSetupCreator.IsFixedLengthPeriod(fes.FPType);
			bool isFixedVariableLengthPeriod = fes.FPType == FiscalPeriodSetupCreator.FPType.FourFourFive || fes.FPType == FiscalPeriodSetupCreator.FPType.FourFiveFour || fes.FPType == FiscalPeriodSetupCreator.FPType.FiveFourFour;

			bool isCustomPeriods = (fes.FPType == FiscalPeriodSetupCreator.FPType.Custom);


			if (!isFixedLengthPeriod)
			{
				fes.AdjustToPeriodStart = fes.PeriodsStartDate.HasValue && fes.BegFinYear.HasValue && fes.BegFinYear != fes.PeriodsStartDate;
			}

			PXUIFieldAttribute.SetEnabled<YearSetup.belongsToNextYear>(cache, fes, allowChangeStartingDate && allowAnyModification);
			PXUIFieldAttribute.SetEnabled<YearSetup.begFinYear>(cache, fes, allowChangeStartingDate && allowAnyModification);

			PXUIFieldAttribute.SetEnabled<YearSetup.finPeriods>(cache, fes, !isFixedLengthPeriod && allowAnyModification && (isCustomPeriods && !HasPartiallyDefinedYear));
			PXUIFieldAttribute.SetEnabled<YearSetup.userDefined>(cache, fes, false);
			PXUIFieldAttribute.SetEnabled<PeriodSetup.endDateUI>(Periods.Cache, null, !isFixedLengthPeriod && (isCustomPeriods));
			decrFirstYear.SetEnabled(!allowChangeStartingDate && allowAnyModification);

			if (!isCustomPeriods)
			{
				this.Periods.Cache.AllowUpdate = !isFixedLengthPeriod && allowAnyModification;
			}
			else
			{
				this.Periods.Cache.AllowUpdate = !isFixedLengthPeriod && allowAnyModification && !this.HasPartiallyDefinedYear;
			}

			//Insert/Delete periods on Financial Periods screen
			this.Periods.Cache.AllowInsert = false;
			this.Periods.Cache.AllowDelete = false;

			PXUIFieldAttribute.SetVisible<YearSetup.adjustToPeriodStart>(cache, fes, !isFixedLengthPeriod && !isCustomPeriods);
			PXUIFieldAttribute.SetVisible<YearSetup.finPeriods>(cache, fes, !isFixedLengthPeriod || isFixedVariableLengthPeriod);
			PXUIFieldAttribute.SetVisible<YearSetup.userDefined>(cache, fes, false);
			PXUIFieldAttribute.SetVisible<YearSetup.periodsStartDate>(cache, fes, true);
			PXUIFieldAttribute.SetEnabled<YearSetup.periodsStartDate>(cache, fes, isFixedLengthPeriod && allowChangeStartingDate && allowAnyModification && fes.FPType != FiscalPeriodSetupCreator.FPType.Week);
			PXUIFieldAttribute.SetVisible<YearSetup.periodLength>(cache, fes, isFixedLengthPeriod && !isFixedVariableLengthPeriod);
			PXUIFieldAttribute.SetEnabled<YearSetup.periodLength>(cache, fes, isFixedLengthPeriod && fes.FPType == FiscalPeriodSetupCreator.FPType.FixedLength && allowChangeStartingDate && allowAnyModification);
			PXUIFieldAttribute.SetEnabled<YearSetup.periodType>(cache, fes, allowChangeStartingDate && allowAnyModification);
			PXUIFieldAttribute.SetEnabled<YearSetup.adjustToPeriodStart>(cache, fes, allowChangeStartingDate && allowAnyModification && isFixedLengthPeriod);
			PXUIFieldAttribute.SetEnabled<YearSetup.hasAdjustmentPeriod>(cache, fes, allowChangeStartingDate && allowAnyModification && !isCustomPeriods);
			PXUIFieldAttribute.SetVisible<YearSetup.hasAdjustmentPeriod>(cache, fes, !isCustomPeriods);

			PXUIFieldAttribute.SetVisible<YearSetup.endYearCalcMethod>(cache, fes, fes.FPType == FiscalPeriodSetupCreator.FPType.Week);
			PXUIFieldAttribute.SetEnabled<YearSetup.endYearCalcMethod>(cache, fes, fes.FPType == FiscalPeriodSetupCreator.FPType.Week && allowAnyModification);
			PXUIFieldAttribute.SetVisible<YearSetup.endYearDayOfWeek>(cache, fes, fes.FPType == FiscalPeriodSetupCreator.FPType.Week);
			PXUIFieldAttribute.SetEnabled<YearSetup.endYearDayOfWeek>(cache, fes, fes.FPType == FiscalPeriodSetupCreator.FPType.Week && allowAnyModification);
			PXUIFieldAttribute.SetVisible<YearSetup.yearLastDayOfWeek>(cache, fes, fes.FPType == FiscalPeriodSetupCreator.FPType.Week && fes.EndYearCalcMethod != EndYearMethod.Calendar);

			this.AutoFill.SetEnabled(!isFixedLengthPeriod && allowAnyModification);
		}
		protected virtual void TYearSetupOnRowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			string errMsg;
			if (!this.AllowDeleteYearSetup(out errMsg))
			{
				throw new PXException(errMsg);
			}
			this.isMassDelete = true;
		}
		protected virtual void TYearSetupOnRowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			TYearSetup yearSetup = (TYearSetup)e.Row;
			DateTime businessDate = this.Accessinfo.BusinessDate.Value;

			if (!yearSetup.BegFinYear.HasValue)
				yearSetup.BegFinYear = new DateTime(businessDate.Year, 1, 1);
			RecalcFirstYear(yearSetup);
			//Instance of this._periodCreator can't be used here - row is not inserted yet
			if (yearSetup.FinPeriods.HasValue == false || yearSetup.FinPeriods.Value <= 0)
				yearSetup.FinPeriods = FiscalPeriodSetupCreator<TPeriodSetup>.GetFiscalPeriodsNbr(yearSetup.FPType, yearSetup.HasAdjustmentPeriod.Value);
			yearSetup.PeriodLength = FiscalPeriodSetupCreator.GetPeriodLength(yearSetup.FPType);
			RecalcPeriodStartDate(yearSetup);
		}
		protected virtual void TYearSetupOnRowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			if (PXDBOperation.Insert == e.Operation || PXDBOperation.Update == e.Operation)
			{
				TYearSetup fes = (TYearSetup)e.Row;
				int count = 0;
				const string CS_FIRST_FISCAL_PERIOD = "01";
				if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(fes.FPType) == false)
				{
					TPeriodSetup firstPeriod = null;
					foreach (TPeriodSetup fps in this.Periods.Select())
					{
						if (firstPeriod == null && (fps.PeriodNbr == CS_FIRST_FISCAL_PERIOD)) firstPeriod = fps;
						count++;
					}
					if (fes.FinPeriods != count)
					{
						if (cache.RaiseExceptionHandling<YearSetup.finPeriods>(e.Row, fes.FinPeriods, new PXSetPropertyException(Messages.NotAllFiscalPeriodsDefined, PXErrorLevel.RowError)))
						{
							throw new PXRowPersistingException(typeof(YearSetup.finPeriods).Name, fes.FinPeriods, Messages.NotAllFiscalPeriodsDefined);
						}
					}
				}
				else
				{
					string message;
					if (fes.FPType != FiscalPeriodSetupCreator.FPType.Week && !ValidatePeriodsStartDate(fes, fes.PeriodsStartDate.Value, out message))
					{
						cache.RaiseExceptionHandling<YearSetup.periodsStartDate>(fes, fes.PeriodsStartDate, new PXSetPropertyException(message));
					}
				}
			}
		}
		protected virtual void TYearSetupOnBegFinYearFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (FiscalYearSetup.Current == null) return;
			TYearSetup year = (TYearSetup)e.Row;
			RecalcFirstYear(year);
			if (!this.skipPeriodReset)
				ResetPeriods(year, false);
			if (year.FPType == FiscalPeriodSetupCreator.FPType.Week)
			{
				year.PeriodsStartDate = AdjustPeriodStartOnDate(year);
			}

		}
		protected virtual void TYearSetupOnAdjustToPeriodStartFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (FiscalYearSetup.Current == null) return;
			TYearSetup year = (TYearSetup)e.Row;
			RecalcFirstYear(year);
			ResetPeriods(year, false);
		}
		protected virtual void TYearSetupOnBelongsToNextYearFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			TYearSetup year = (TYearSetup)e.Row;
			RecalcFirstYear(year);
		}
		protected virtual void TYearSetupOnPeriodTypeFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (FiscalYearSetup.Current == null) return;
			TYearSetup row = (TYearSetup)e.Row;
			this.ResetPeriods(row, false);
			row.EndYearCalcMethod = EndYearMethod.Calendar;
		}

		protected virtual void TYearSetupOnEndYearCalcMethodFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (FiscalYearSetup.Current == null) return;
			TYearSetup row = (TYearSetup)e.Row;
			row.PeriodsStartDate = AdjustPeriodStartOnDate(row);
		}

		protected virtual void TYearSetupOnEndYearDayOfWeekFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (FiscalYearSetup.Current == null) return;
			TYearSetup row = (TYearSetup)e.Row;
			row.PeriodsStartDate = AdjustPeriodStartOnDate(row);
		}

		protected virtual void TYearSetupOnPeriodsStartDateFieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			TYearSetup row = (TYearSetup)e.Row;
			DateTime newDate = (DateTime)e.NewValue;
			string message;
			if (row.FPType != FiscalPeriodSetupCreator.FPType.Week && !ValidatePeriodsStartDate(row, newDate, out message))
			{
				e.NewValue = row.PeriodsStartDate;
				throw new PXSetPropertyException(message);
			}
		}

		protected virtual void TYearSetupOnPeriodsStartDateFieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			TYearSetup row = (TYearSetup)e.Row;
			if (row != null && row.BegFinYear.HasValue)
			{
				if (row.FPType == FiscalPeriodSetupCreator.FPType.Week
					|| row.FPType == FiscalPeriodSetupCreator.FPType.BiWeek
						|| row.FPType == FiscalPeriodSetupCreator.FPType.FourWeek)
				{
					DayOfWeek dow = row.BegFinYear.Value.DayOfWeek;
					const int weekLength = 7;
					int distance = dow - DayOfWeek.Monday;
					if (distance < 0)
					{
						int distance1 = distance + weekLength;
						distance = Math.Abs(distance) < Math.Abs(distance1) ? distance : distance1;
					}
					else
					{
						int distance1 = distance - weekLength;
						distance = Math.Abs(distance) < Math.Abs(distance1) ? distance : distance1;
					}
					e.NewValue = row.BegFinYear.Value.AddDays(distance);
				}
				else
					e.NewValue = row.BegFinYear;
			}
		}

		protected virtual void TYearSetupOnHasAdjustmentPeriodFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (FiscalYearSetup.Current == null) return;
			TYearSetup row = (TYearSetup)e.Row;
			this.ResetPeriods(row, true);
		}

		protected virtual void RecalcFirstYear(TYearSetup row)
		{
			int newYear = row.BegFinYear.Value.Year;
			if ((row.BelongsToNextYear ?? false))
			{
				newYear++;
			}
			string yearID = FiscalPeriodSetupCreator.FormatYear(newYear);// newYear.ToString(FiscalPeriodSetupCreator.);
			if (yearID != row.FirstFinYear)
			{
				row.FirstFinYear = yearID;
			}
		}

		protected virtual void ResetPeriods(TYearSetup row, bool skipDatesRecalc)
		{
			this.DeletePeriods();
			this._periodCreator = null;
			if (!skipDatesRecalc)
				RecalcPeriodStartDate(row);
			row.PeriodLength = FiscalPeriodSetupCreator.GetPeriodLength(row.FPType);
			row.FinPeriods = this.PeriodCreator?.ActualNumberOfPeriods;
		}

		protected virtual void RecalcPeriodStartDate(TYearSetup row)
		{
			if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(row.FPType) == false)
			{
				row.PeriodsStartDate = FiscalPeriodSetupCreator<TPeriodSetup>.GetStartDate(row.FPType, row.BegFinYear.Value, row.AdjustToPeriodStart.Value);
			}
			else
			{
				if (row.FPType == FiscalPeriodSetupCreator.FPType.Week
					|| row.FPType == FiscalPeriodSetupCreator.FPType.BiWeek
						|| row.FPType == FiscalPeriodSetupCreator.FPType.FourWeek)
				{
					DayOfWeek dow = row.BegFinYear.Value.DayOfWeek;
					int distance = DayOfWeek.Sunday - dow;
					const int weekLength = 7;
					if (distance < 0)
					{
						int distance1 = distance + weekLength;
						distance = Math.Abs(distance) < Math.Abs(distance1) ? distance : distance1;
					}
					else
					{
						int distance1 = distance - weekLength;
						distance = Math.Abs(distance) < Math.Abs(distance1) ? distance : distance1;
					}
					row.PeriodsStartDate = row.BegFinYear.Value.AddDays(distance);
				}
				else
					row.PeriodsStartDate = row.BegFinYear;
			}
		}

		protected virtual bool ValidatePeriodsStartDate(TYearSetup row, DateTime aPSDValue, out string message)
		{
			message = "";
			if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(row.FPType)
				&& row.PeriodLength.HasValue)
			{
				TimeSpan delta = aPSDValue - row.BegFinYear.Value;
				int length = delta.Days;
				int halfPeriod = (int)Math.Floor(row.PeriodLength.Value / 2.0m);
				if (Math.Abs(length) > (halfPeriod))
				{
					message = PXMessages.LocalizeFormatNoPrefix(Messages.PeriodsStartingDateIsTooFarFromYearStartingDate, halfPeriod);
					return false;
				}
			}
			return true;
		}
		#endregion

		#region TPeriodSetupEvents
		protected virtual void TPeriodSetupOnRowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			if (!this.isMassDelete)
			{
				TPeriodSetup fp = (TPeriodSetup)e.Row;
				TYearSetup fes = (TYearSetup)this.FiscalYearSetup.Current;
				if (!fes.UserDefined.Value)
				{
					throw new PXException(Messages.ModifyPeriodsInUserDefinedMode);
				}
				PXEntryStatus status = this.Periods.Cache.GetStatus(fp);
				PXEntryStatus yearStatus = this.FiscalYearSetup.Cache.GetStatus(fes);
				bool isYearDeleting = (PXEntryStatus.InsertedDeleted == yearStatus || PXEntryStatus.Deleted == yearStatus);
				bool hasLaterPeriods = false;

				//Check inserted records
				foreach (TPeriodSetup iPer in this.Periods.Select())
				{
					if (int.Parse(iPer.PeriodNbr) > int.Parse(fp.PeriodNbr))
					{
						hasLaterPeriods = true;
						break;
					}
				}
				if ((!isYearDeleting) && hasLaterPeriods)
				{
					throw new PXException(Messages.DeleteSubseqPeriods);
				}
			}
		}
		protected virtual void TPeriodSetupOnRowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			TPeriodSetup period = (TPeriodSetup)e.Row;
			TYearSetup year = (TYearSetup)this.FiscalYearSetup.Current;
			int count = 0;
			TPeriodSetup lastPeriod = null;

			//Scan existing periods
			foreach (TPeriodSetup iPer in this.Periods.Select())
			{
				count++;
				if (lastPeriod == null)
					lastPeriod = iPer;
				else
					if (iPer.StartDate.Value > lastPeriod.StartDate.Value)
					lastPeriod = iPer;
			}
			if (string.IsNullOrEmpty(period.PeriodNbr)) //Preventing of re-initialization of the row
			{
				int yearNbr = year.BegFinYear.Value.Year;
				if ((year.UserDefined ?? false) && (((year.FinPeriods ?? 0) < 1) && (year.PeriodLength ?? 0) < 5))
				{
					throw new PXException(Messages.RowCanNotInserted);
				}
				bool result = this.PeriodCreator.fillNextPeriod(period, lastPeriod);
				if (!result)
				{
					this.isLastInsertingPeriod = true;
					if (this.isAutoInsert)
						e.Cancel = true;
					else
						throw new PXException(Messages.AllTheFinPeriodsAreAlreadyInserted);
				}
				else if (PXDBLocalizableStringAttribute.IsEnabled)
				{
					string[] translations = this.Periods.Cache.GetValueExt(period, "DescrTranslations") as string[];
					if (translations != null && translations.Length > 0)
					{
						translations[0] = period.Descr;
						this.Periods.Cache.SetValueExt(period, "DescrTranslations", translations);
					}
				}
			}
		}
		protected virtual void TPeriodSetupOnRowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			TPeriodSetup newRow = (TPeriodSetup)e.Row;
			TPeriodSetup oldRow = (TPeriodSetup)e.OldRow;
			if (!this.massUpdateFlag)
			{
				if (oldRow.EndDate.Value != newRow.EndDate.Value)
				{
					if (e.ExternalCall)
					{
						if (newRow.StartDate == ((FinPeriodSetup)cache.Current).EndDateUI)
						{
							newRow.EndDate = newRow.StartDate;
						}
					}
					//Date is changed - we need to update next row start date
					int periodNbr = int.Parse(newRow.PeriodNbr);
					periodNbr++;
					string key = periodNbr.ToString("00");
					foreach (TPeriodSetup fps in this.Periods.Select())
					{
						if (fps.PeriodNbr == key)
						{
							fps.StartDate = newRow.EndDate;
							if (fps.EndDate < fps.StartDate)
								fps.EndDate = fps.StartDate;
							this.Periods.Cache.SetStatus(fps, PXEntryStatus.Updated);
							this.Periods.View.RequestRefresh();
							break;
						}
					}
				}
			}
		}

		protected virtual void TPeriodSetupOnEndDateFieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			TPeriodSetup fps = (TPeriodSetup)e.Row;
			DateTime? newValue = (DateTime?)e.NewValue;
			if (newValue.HasValue && fps.StartDate.HasValue)
			{
				if (newValue < fps.StartDate.Value)
				{
					throw new PXSetPropertyException(Messages.EndDateGreaterEqualStartDate);
				}
				if (newValue > this.PeriodCreator.YearEnd)
				{
					throw new PXSetPropertyException(Messages.PeriodEndDateIsAfterYearEnd);
				}
			}
		}
		protected virtual void TPeriodSetupOnRowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			TYearSetup row = (TYearSetup)this.FiscalYearSetup.Current;
			if (this.FiscalYearSetup.Cache.GetStatus(row) == PXEntryStatus.Notchanged)
				Caches[typeof(TYearSetup)].SetStatus(row, PXEntryStatus.Updated);
		}
		protected virtual void TPeriodSetupOnRowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			TYearSetup row = (TYearSetup)this.FiscalYearSetup.Current;
			if (this.FiscalYearSetup.Cache.GetStatus(row) == PXEntryStatus.Notchanged)
				Caches[typeof(TYearSetup)].SetStatus(row, PXEntryStatus.Updated);
		}
		#endregion

		#region Internal Utlity functions
		private void DoIncrementFirstYear(TYearSetup yearSetup, bool negative)
		{
			int increment = negative ? -1 : 1;
			DateTime? newStartDate = yearSetup.BegFinYear.Value.AddYears(increment);
			FiscalPeriodSetupCreator<TPeriodSetup> creator = new FiscalPeriodSetupCreator<TPeriodSetup>(yearSetup);
			try
			{
				this.skipPeriodReset = true;
				this.FiscalYearSetup.Cache.SetValueExt<YearSetup.begFinYear>(yearSetup, newStartDate);
			}
			finally
			{
				this.skipPeriodReset = false;
			}
			if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(yearSetup.FPType) == true)
			{
				DateTime? newPeriodStartDate = creator.CalcPeriodsStartDate(yearSetup.FirstFinYear);
				yearSetup.PeriodsStartDate = newPeriodStartDate;
			}
			else
			{
				RecalcPeriodStartDate(yearSetup);
			}

			try
			{
				this.skipPeriodReset = true;
				TYearSetup yr = (TYearSetup)this.FiscalYearSetup.Cache.Update(yearSetup);
			}
			finally
			{
				this.skipPeriodReset = false;
			}
			if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(yearSetup.FPType) == false)
			{
				massUpdateFlag = true;
				foreach (TPeriodSetup iRow in this.Periods.Select())
				{
					TPeriodSetup copy = (TPeriodSetup)this.Periods.Cache.CreateCopy(iRow);
					copy.StartDate = copy.StartDate.Value.AddYears(increment);
					copy.EndDate = copy.EndDate.Value.AddYears(increment);
					copy = this.Periods.Update(copy);
				}
				massUpdateFlag = false;
			}
		}

		protected abstract bool IsFiscalYearExists();
		protected abstract bool IsFiscalYearSetupExists();
		protected abstract bool CheckForPartiallyDefinedYear();
		protected abstract bool AllowDeleteYearSetup(out string errMsg);

		protected virtual bool AllowAlterStartDate()
		{
			return !this.IsFiscalYearExists();
		}

		protected virtual bool AllowSetupModification(TYearSetup aRow)
		{
			return true;
		}

		private bool AutoFillPeriods()
		{
			TYearSetup setup = (TYearSetup)this.FiscalYearSetup.Current;
			int yearNbr = int.Parse(setup.FirstFinYear);
			if (yearNbr < 0 || yearNbr > 9999) return false;
			if (!this.DeletePeriods()) return false;
			int count = 0;
			const int maxCount = 1000;
			this.isAutoInsert = true;
			try
			{
				do
				{
					this.isLastInsertingPeriod = false;
					TPeriodSetup period = new TPeriodSetup();
					this.Periods.Cache.Insert(period);
					count++;
				}
				while (!this.isLastInsertingPeriod && count < maxCount);
			}
			finally
			{
				this.isAutoInsert = false;
			}
			if (count >= maxCount)
				throw new PXException(Messages.ERR_InfiniteLoopDetected);
			return true;
		}
		private bool DeletePeriods()
		{
			isMassDelete = true;
			try
			{
				///Add code to remove all the rows from this.Periods here
				foreach (TPeriodSetup fps in this.Periods.Select())
				{
					this.Periods.Cache.Delete(fps);
				}
			}
			finally { isMassDelete = false; };
			return true;
		}

		private DateTime? AdjustPeriodStartOnDate(TYearSetup row)
		{
			if (row.FPType == FiscalPeriodSetupCreator.FPType.Week)
			{
				if (row.EndYearCalcMethod == EndYearMethod.LastDay)
				{
					int daysToAdd = 0;
					if ((int)row.BegFinYear.Value.DayOfWeek + 1 != row.EndYearDayOfWeek)
					{
						if ((int)row.BegFinYear.Value.DayOfWeek + 1 > (int)row.EndYearDayOfWeek)
						{
							daysToAdd = (int)row.BegFinYear.Value.DayOfWeek + 1 - (int)row.EndYearDayOfWeek;
						}
						else
						{
							daysToAdd = (int)row.BegFinYear.Value.DayOfWeek + 1 - (int)row.EndYearDayOfWeek + 7;
						}
						daysToAdd = System.Math.Abs(daysToAdd) * (-1);
					}
					return row.BegFinYear.Value.AddDays(daysToAdd);
				}
				else
				{
					int daysToAdd = 0;
					daysToAdd = (int)row.EndYearDayOfWeek - 1 - (int)row.BegFinYear.Value.DayOfWeek;
					if (System.Math.Abs(daysToAdd) > Math.Floor(7 / 2.0))
					{
						daysToAdd = daysToAdd > 0 ? daysToAdd - 7 : daysToAdd + 7;
					}
					return row.BegFinYear.Value.AddDays(daysToAdd);
				}
			}
			return row.PeriodsStartDate;
		}
		#endregion

		#region Internal variables
		private const FiscalPeriodSetupCreator.FPType defaultPeriodType = FiscalPeriodSetupCreator.FPType.Month;
		private bool isLastInsertingPeriod;
		private bool isMassDelete = false;
		private bool isAutoInsert = false;
		private bool skipPeriodReset = false;

		private bool? _hasPartiallyDefinedYear = null;
		//Only to cache request result.
		protected bool HasPartiallyDefinedYear
		{
			get
			{
				if (this._hasPartiallyDefinedYear == null)
				{
					this._hasPartiallyDefinedYear = this.CheckForPartiallyDefinedYear();
				}
				return this._hasPartiallyDefinedYear.Value;
			}
		}


		private FiscalPeriodSetupCreator<TPeriodSetup> PeriodCreator
		{
			get
			{
				if (this._periodCreator == null)
				{
					TYearSetup yearSetup = FiscalYearSetup.Current;
					if (yearSetup != null)
					{
						if (yearSetup.FPType != FiscalPeriodSetupCreator.FPType.Custom)
						{
							this._periodCreator = new FiscalPeriodSetupCreator<TPeriodSetup>(yearSetup);
						}
						else
						{
							short numberOfPeriods = (yearSetup.FinPeriods ?? 0);
							this._periodCreator = new FiscalPeriodSetupCreator<TPeriodSetup>(yearSetup.FirstFinYear, yearSetup.BegFinYear.Value, numberOfPeriods);
						}
					}
				}
				return this._periodCreator;
			}
		}
		FiscalPeriodSetupCreator<TPeriodSetup> _periodCreator;
		#endregion
	}

	public abstract class FiscalPeriodSetupCreator
	{
		public enum FPType
		{
			Month = 0,
			BiMonth,
			Quarter,
			Week,
			BiWeek,
			Decade,
			FixedLength,
			FourWeek,
			FourFourFive,
			FourFiveFour,
			FiveFourFour,
			Custom,
		};
		public const string CS_YEAR_FORMAT = "0000";
		public const string CS_PERIOD_NBR_FORMAT = "00";

		public static bool IsFixedLengthPeriod(FPType aType)
		{
			return (aType == FPType.Week
					|| aType == FPType.BiWeek
					|| aType == FPType.FourWeek
					|| aType == FPType.Decade
					|| aType == FPType.FixedLength
					|| aType == FPType.FourFourFive
					|| aType == FPType.FourFiveFour
					|| aType == FPType.FiveFourFour
					);
		}
		public static short? GetPeriodLength(FPType aType)
		{
			return GetPeriodLength(aType, 0);
		}
		public static short? GetPeriodLength(FPType aType, int periodNum)
		{
			short? result = null;
			const short weekLength = 7;
			switch (aType)
			{
				case FPType.Week: result = weekLength; break;
				case FPType.BiWeek: result = weekLength * 2; break;
				case FPType.FourWeek: result = weekLength * 4; break;
				case FPType.Decade: result = 10; break;
				case FPType.FourFourFive: result = (short)((periodNum != 0 && periodNum % 3 == 0) ? weekLength * 5 : weekLength * 4); break;
				case FPType.FourFiveFour: result = (short)((periodNum != 0 && (periodNum + 1) % 3 == 0) ? weekLength * 5 : weekLength * 4); break;
				case FPType.FiveFourFour: result = (short)((periodNum != 0 && (periodNum + 2) % 3 == 0) ? weekLength * 5 : weekLength * 4); break;
			}
			return result;
		}
		public static string FormatYear(DateTime aDate)
		{
			return aDate.Year.ToString(CS_YEAR_FORMAT);
		}
		public static string FormatYear(int aYear)
		{
			return aYear.ToString(CS_YEAR_FORMAT);
		}
	}
	public class FiscalPeriodSetupCreator<TPeriodSetup> : FiscalPeriodSetupCreator
		where TPeriodSetup : class, IPeriodSetup, new()
	{
		#region Ctor
		public FiscalPeriodSetupCreator(IYearSetup yearSetup)
			:this(yearSetup, yearSetup.FirstFinYear, yearSetup.PeriodsStartDate.Value, yearSetup.BegFinYear.Value)
		{
		}

		public FiscalPeriodSetupCreator(IYearSetup yearSetup, string aFiscalYear, DateTime aPeriodsStartDate, DateTime aYearStartDate)
			:this(aFiscalYear, 
				aPeriodsStartDate,
				yearSetup.FPType,
				yearSetup.HasAdjustmentPeriod.Value,
				yearSetup.AdjustToPeriodStart.Value,
				yearSetup.PeriodLength,
				aYearStartDate,
				yearSetup.EndYearCalcMethod,
				yearSetup.EndYearDayOfWeek.Value)
		{
			AdjustFiscalPeriodsCount(yearSetup);
		}

		public FiscalPeriodSetupCreator(string aFiscalYear, DateTime aPeriodsStartDate, FPType aPeriodType, bool aHasAdjPeriod, bool aAdjStartDate, short? aPeriodLength, DateTime aYearStartDate, string aEndYearCalcMethod, int? aEndYearDayOfWeek)
		{
			//Algorithm for the week probably has to adjusted to consider DOW of starting date - additional parameter is required
			this.periodType = aPeriodType;
			this.adjustStartDate = (this.periodType == FPType.Custom || IsFixedLengthPeriod(this.periodType)) ? false : aAdjStartDate;
			this.hasAdjustmentPeriod = (this.periodType == FPType.Custom) ? false : aHasAdjPeriod;
			if (IsFixedLengthPeriod(aPeriodType))
			{
				this.customPeriodLength = (aPeriodType == FPType.FixedLength) ? aPeriodLength : GetPeriodLength(aPeriodType);
				this.fixedNumberOfPeriods = false;
				this.fixedEndOfYear = false;
			}
			else
			{
				this.customPeriodLength = null;
				this.fixedNumberOfPeriods = true;
				this.fixedEndOfYear = true;
				short numOfPeriods = GetFiscalPeriodsNbr(this.periodType, this.hasAdjustmentPeriod);
				this.numberOfPeriods = (short)numOfPeriods > 0 ? numOfPeriods : (short)0;
			}
			this.yearStartingDate = aYearStartDate;

			if (this.periodType == FPType.Week && aEndYearCalcMethod != EndYearMethod.Calendar)
			{
				switch (aEndYearCalcMethod)
				{
					case EndYearMethod.LastDay:
						{
							int daysToAdd = 0;
							if ((int)aYearStartDate.AddYears(FiscalYearLength).DayOfWeek != aEndYearDayOfWeek)
							{
								if ((int)aYearStartDate.AddYears(FiscalYearLength).DayOfWeek > (int)aEndYearDayOfWeek)
								{
									daysToAdd = (int)aYearStartDate.AddYears(FiscalYearLength).DayOfWeek - (int)aEndYearDayOfWeek;
								}
								else
								{
									daysToAdd = (int)aYearStartDate.AddYears(FiscalYearLength).DayOfWeek - (int)aEndYearDayOfWeek + weekLength;
								}
								daysToAdd = System.Math.Abs(daysToAdd) * (-1);
							}
							this.yearEndingDate = aYearStartDate.AddYears(FiscalYearLength).AddDays(daysToAdd);
							this.periodsStartDate = aPeriodsStartDate;
							this.periodsEndDate = this.periodsStartDate.AddYears(FiscalYearLength).AddDays(daysToAdd);
							break;
						}
					case EndYearMethod.NearestDay:
						{
							int daysToAdd = 0;
							if ((int)aYearStartDate.AddYears(FiscalYearLength).DayOfWeek != aEndYearDayOfWeek)
							{
								daysToAdd = (int)aEndYearDayOfWeek - (int)aYearStartDate.AddYears(FiscalYearLength).DayOfWeek;
								if (System.Math.Abs(daysToAdd) > Math.Floor(weekLength / 2.0))
								{
									daysToAdd = daysToAdd > 0 ? daysToAdd - weekLength : daysToAdd + weekLength;
								}
							}
							this.yearEndingDate = aYearStartDate.AddYears(FiscalYearLength).AddDays(daysToAdd);
							this.periodsStartDate = aPeriodsStartDate;
							this.periodsEndDate = this.periodsStartDate.AddYears(FiscalYearLength).AddDays(daysToAdd);
							break;
						}
				}
			}
			else
			{
				this.yearEndingDate = aYearStartDate.AddYears(FiscalYearLength);
				this.periodsStartDate = aPeriodsStartDate;
				this.periodsEndDate = this.periodsStartDate.AddYears(FiscalYearLength);
			}

			this.actualNumberOfPeriods = this.numberOfPeriods;

			if (!this.fixedEndOfYear && this.customPeriodLength.HasValue && this.customPeriodLength > 0)
			{
				TimeSpan yearLength = this.yearEndingDate - this.periodsStartDate;
				short periodLength = (short)this.customPeriodLength.Value;
				short numOfPeriods = (short)(yearLength.Days / periodLength);
				int remainder = yearLength.Days % periodLength;
				if (remainder > Math.Floor(this.customPeriodLength.Value / 2.0))
					numOfPeriods++;
				this.periodsEndDate = this.periodsStartDate.AddDays(numOfPeriods * periodLength);
				this.actualNumberOfPeriods = (short)(numOfPeriods + (this.hasAdjustmentPeriod ? 1 : 0));
			}

			if (aPeriodType == FPType.FourFourFive || aPeriodType == FPType.FourFiveFour || aPeriodType == FPType.FiveFourFour)
			{
				TimeSpan remainderLength = this.yearEndingDate - this.periodsStartDate.AddDays(weekLength * 52);
				this.periodsEndDate = remainderLength.Days > Math.Floor(weekLength / 2.0) ? this.periodsStartDate.AddDays(weekLength * 53) : this.periodsStartDate.AddDays(weekLength * 52);
				this.fixedNumberOfPeriods = true;
				this.actualNumberOfPeriods = this.numberOfPeriods = GetFiscalPeriodsNbr(this.periodType, this.hasAdjustmentPeriod);
			}

			this.fiscalYear = aFiscalYear;
		}

		public FiscalPeriodSetupCreator(string aFiscalYear, DateTime aPeriodsStartingDate, short aNumberOfPeriods)
		{
			this.numberOfPeriods = aNumberOfPeriods > 0 ? aNumberOfPeriods : (short)1;
			this.customPeriodLength = (short)(Math.Round((366.0m / this.numberOfPeriods), 0, MidpointRounding.AwayFromZero));
			this.fixedNumberOfPeriods = true;
			this.periodsStartDate = aPeriodsStartingDate;
			this.periodsEndDate = this.periodsStartDate.AddYears(FiscalYearLength);
			this.fiscalYear = aFiscalYear;
			this.periodType = FPType.Custom;
			this.hasAdjustmentPeriod = false;
			this.actualNumberOfPeriods = this.numberOfPeriods;
		}
		#endregion

		#region Public instance function & Properties
		public virtual bool fillNextPeriod(TPeriodSetup result, TPeriodSetup current)
		{
			int periodNbr = (current != null) ? int.Parse(current.PeriodNbr) : 0;
			if (current != null)
				result.StartDate = new DateTime(current.EndDate.Value.Year, current.EndDate.Value.Month, current.EndDate.Value.Day);
			else
				result.StartDate = GetStartDate(this.periodType, this.periodsStartDate, this.adjustStartDate);
			periodNbr++;

			//bool isOutOfYear = this.fixedEndOfYear ? (result.StartDate >= this.periodsEndDate) : result.StartDate > this.periodsEndDate.AddDays(-Math.Floor((this.customPeriodLength??0)/2.0));
			bool isOutOfYear;
			if (!result.Custom ?? true)
			{
				isOutOfYear = (result.StartDate >= this.periodsEndDate);
			}
			else
			{
				isOutOfYear = false;
			}
			bool isLast = this.fixedNumberOfPeriods ? (periodNbr == this.numberOfPeriods) : false;
			if (result.EndDate == null)
			{
				if (!result.Custom ?? true)
				{
					result.EndDate = isLast ? this.periodsEndDate : GetEndDate(this.periodType, result.StartDate.Value, this.yearEndingDate, (this.customPeriodLength ?? 0), periodNbr);
				}
				else
				{
					result.EndDate = result.StartDate;
				}
			}

			if (isOutOfYear)
			{
				if (!this.hasAdjustmentPeriod) return false; //
				if (current != null && current.StartDate == result.StartDate) return false; //Adjusted period isalready inserted 
				result.EndDate = result.StartDate;
			}
			else
			{
				if ((result.EndDate > this.periodsEndDate && this.fixedEndOfYear) ||
					(this.hasAdjustmentPeriod && this.fixedEndOfYear && periodNbr == this.numberOfPeriods - 1)) result.EndDate = this.periodsEndDate;
			}
			result.PeriodNbr = (periodNbr).ToString(CS_PERIOD_NBR_FORMAT);
			result.Descr = GetPeriodDefaultDescr(this.periodType, result, this.hasAdjustmentPeriod);
			return true;
		}
		public virtual TPeriodSetup createNextPeriod(TPeriodSetup current)
		{
			TPeriodSetup result = new TPeriodSetup();
			if (!this.fillNextPeriod(result, current)) return null;
			return result;
		}
		public DateTime YearEnd
		{
			get { return this.periodsEndDate; }
		}
		public short ActualNumberOfPeriods
		{
			get
			{
				return actualNumberOfPeriods;
			}
		}
		public virtual DateTime CalcPeriodsStartDate(string aFiscalYear)
		{
			int yearNum0 = int.Parse(this.fiscalYear);
			int yearNum1 = int.Parse(aFiscalYear);
			if (yearNum0 == yearNum1)
				return this.periodsStartDate;
			int delta = yearNum1 - yearNum0;
			DateTime yearStartingDate = this.yearStartingDate.AddYears(delta);
			if (IsFixedLengthPeriod(this.periodType) && this.fixedEndOfYear == false)
			{
				TimeSpan length = yearStartingDate - this.periodsStartDate;
				int periodLength = (int)this.customPeriodLength.Value;
				int sign = length.Days >= 0 ? 1 : -1;
				int numOfPeriods = (length.Days * sign) / periodLength;
				int remainder = (length.Days * sign) % periodLength;
				if (remainder > Math.Floor(this.customPeriodLength.Value / 2.0))
					numOfPeriods++;
				return this.periodsStartDate.AddDays((numOfPeriods * periodLength) * sign);
			}
			else
			{
				return this.periodsStartDate.AddYears(delta);
			}
		}

		#endregion

		#region Public static Functions
		//Calculates Period end date
		public static DateTime GetEndDate(FPType aType, DateTime aStartDate, DateTime aYearEndingDate, short aCustomLength, int aPeriodNum)
		{
			if (aType == FPType.Month || aType == FPType.BiMonth || aType == FPType.Quarter)
			{
				int length = (aType == FPType.Month) ? 1 : ((aType == FPType.BiMonth) ? 2 : 3);
				return aStartDate.AddMonths(length);
			}
			else if (IsFixedLengthPeriod(aType))
			{
				int length = (aType == FPType.Custom || aType == FPType.FixedLength ? (int)aCustomLength : (int)GetPeriodLength(aType, aPeriodNum));

				if ((aType == FPType.FourFourFive || aType == FPType.FourFiveFour || aType == FPType.FiveFourFour)
					&& aPeriodNum == 12 && ((TimeSpan)(aYearEndingDate - aStartDate.AddDays(length))).Days > Math.Floor(weekLength / 2.0))
				{
					length += weekLength;
				}
				return aStartDate.AddDays(length);
			}
			else if (aType == FPType.Custom)
			{
				return aStartDate.AddDays(aCustomLength);
			}
			return aStartDate;
		}

		//Calculates Periods Starting Date 
		public static DateTime GetStartDate(FPType aType, DateTime aStartDate, bool aAdjustStartDate)
		{
			if (aAdjustStartDate)
			{
				switch (aType)
				{
					case FPType.BiMonth:
					case FPType.Month:
					case FPType.Quarter:
						return new DateTime(aStartDate.Year, aStartDate.Month, 1);
					default:
						throw new PXException(Messages.NoDateAdjustment, aType.ToString());
				}
			}
			else
				return aStartDate;
		}

		//Returns number of periods for Fixed Period Number Type
		public static short GetFiscalPeriodsNbr(FPType aType, bool hasAdjustemntPeriod)
		{
			short periods = -2;
			switch (aType)
			{
				case FPType.Month: periods = 12; break;
				case FPType.BiMonth: periods = 6; break;
				case FPType.Quarter: periods = 4; break;
				case FPType.Custom: periods = 1; break;
				case FPType.FourFourFive: periods = 12; break;
				case FPType.FourFiveFour: periods = 12; break;
				case FPType.FiveFourFour: periods = 12; break;
			}
			if (hasAdjustemntPeriod) periods++;
			return periods;
		}

		private void AdjustFiscalPeriodsCount(IYearSetup yearSetup)
		{
			if(yearSetup.FPType == FPType.Custom)
			{
				actualNumberOfPeriods = yearSetup.FinPeriods ?? 1;
			}
		}


		/// <summary>
		/// Returns default description for a record;
		/// Algorithm require aRecord to be filled before calling of this function
		/// </summary>
		private static string GetPeriodDefaultDescr(FPType aType, TPeriodSetup aRecord, bool aHasAdjustPeriod)
		{
			if ((aRecord.EndDate == aRecord.StartDate) && aHasAdjustPeriod)
			{
				return PXMessages.LocalizeNoPrefix(Messages.AdjustmentPeriod);
			}

			switch (aType)
			{
				case FPType.Quarter:
					return PXMessages.LocalizeFormatNoPrefix(Messages.QuarterDescr, aRecord.PeriodNbr);
				case FPType.Month:
					return aRecord.StartDate.Value.ToString("MMMM");
				default:
					return PXMessages.LocalizeFormatNoPrefix(Messages.PeriodDescr, aRecord.PeriodNbr);

			}
		}

		#endregion
		#region Internal Variables
		private const int FiscalYearLength = 1;  //Length of the fiscal year (in years);
		private const short weekLength = 7;
		private bool hasAdjustmentPeriod;
		private FPType periodType;
		private DateTime yearStartingDate;
		private DateTime yearEndingDate;
		private DateTime periodsStartDate;
		private DateTime periodsEndDate;
		private bool adjustStartDate;
		private short? customPeriodLength = null;
		private short numberOfPeriods = 0;
		private short actualNumberOfPeriods;
		private bool fixedNumberOfPeriods = false;
		private bool fixedEndOfYear = false;
		private readonly string fiscalYear;
		#endregion
	}
}