using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.DAC;
using PX.Objects.GL.GraphBaseExtensions;
using PX.Objects.Common;
using PX.Objects.Common.Scopes;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.Attributes;
using PX.Objects.FA;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Common;

namespace PX.Objects.GL
{
	public interface IFinPeriodMaintenanceGraph
	{
		void GenerateCalendar(int? organizationID, int fromYear, int toYear);
	}

	public class MasterFinPeriodMaint : PXGraph<MasterFinPeriodMaint, MasterFinYear>, IFinPeriodMaintenanceGraph
	{
		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

	    [InjectDependency]
	    public IFinPeriodRepository FinPeriodRepository { get; set; }

        public class MasterFinPeriodStatusActionsGraphExtension : FinPeriodStatusActionsGraphBaseExtension<MasterFinPeriodMaint, MasterFinYear>
		{
			public static bool IsActive()
			{
				return PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>();
			}
		}

		public class GenerateMasterCalendarExtension : GenerateCalendarExtensionBase<MasterFinPeriodMaint, MasterFinYear> { }

		public class MassInsertingOfPeriodsScope : FlaggedModeScopeBase<MassInsertingOfPeriodsScope> { }
		public class InsertingOfPreviousYearScope : FlaggedModeScopeBase<InsertingOfPreviousYearScope> { }
		public class YearsGenerationScope : FlaggedModeScopeBase<YearsGenerationScope> { }

		#region Ctor + Buttons
		public MasterFinPeriodMaint()
		{
			FinYearSetup setup = YearSetup.Current;
			if (setup == null)
				throw new PXSetPropertyException(Messages.ConfigDataNotEnteredCMSetupAndFinancialYear);

			FiscalYear.Cache.AllowInsert = true;
			FiscalYear.Cache.AllowUpdate = true;
			FiscalYear.Cache.AllowDelete = true;

			Delete.SetConfirmationMessage(Messages.ConfirmMasterFinYearDeletion);

			// Features.xml can logical OR only :(
			PXUIFieldAttribute.SetVisible<MasterFinPeriod.iNClosed>(
				Periods.Cache, 
				null, 
				PXAccess.FeatureInstalled<FeaturesSet.inventory>() 
					&& PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>());
			PXUIFieldAttribute.SetVisibility<MasterFinPeriod.iNClosed>(
				Periods.Cache,
				null,
				PXAccess.FeatureInstalled<FeaturesSet.inventory>()
					&& PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>()
					? PXUIVisibility.Visible
					: PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisible<MasterFinPeriod.fAClosed>(
				Periods.Cache, 
				null, 
				PXAccess.FeatureInstalled<FeaturesSet.fixedAsset>()
					&& PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>());
			PXUIFieldAttribute.SetVisibility<MasterFinPeriod.fAClosed>(
				Periods.Cache,
				null,
				PXAccess.FeatureInstalled<FeaturesSet.fixedAsset>()
					&& PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>()
					? PXUIVisibility.Visible
					: PXUIVisibility.Invisible);
		}

		public virtual MasterFinYear GeneratePreviousMasterFinYear()
		{
			MasterFinYear insertedYear;
			using (new YearsGenerationScope())
			{
				insertedYear = InsertPreviousMasterFinYear();

                FillCurrentYearWithPeriods(
                    FinPeriod.status.Locked,
                    FinPeriodRepository.FindFirstPeriod(FinPeriod.organizationID.MasterValue, clearQueryCache: true));

				Save.Press();
			}
			return insertedYear;
		}

		public virtual MasterFinYear GenerateNextMasterFinYear()
		{
			MasterFinYear insertedYear;
			using(new YearsGenerationScope())
			{
				insertedYear = FiscalYear.Insert();
				FillCurrentYearWithPeriods(FinPeriod.status.Inactive);
				Save.Press();
			}
			return insertedYear;
		}

		public virtual void GenerateCalendar(int? organizationID, int fromYear, int toYear)
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				FiscalYearSetupMaint setupGraph = CreateInstance<FiscalYearSetupMaint>();
				setupGraph.SetCurrentYearSetup();
				setupGraph.ShiftBackFirstYearTo($"{fromYear:0000}");
				GenerateMasterCalendar(fromYear, toYear);
				ts.Complete();
			}
		}

		protected virtual void GenerateMasterCalendar(int fromYear, int toYear)
		{
			if (HasInsertedYear)
			{
				FiscalYear.Cache.Clear();
			}

			(int firstYear, int lastYear) = FinPeriodUtils.GetFirstLastYearForGeneration(
				FinPeriod.organizationID.MasterValue,
				fromYear,
				toYear,
				clearQueryCache: true);

			MasterFinYear insertedYear;
			if (fromYear < firstYear)
			{
				do
				{
					insertedYear = GeneratePreviousMasterFinYear();
				}
				while (insertedYear != null &&
					string.CompareOrdinal(insertedYear.Year, fromYear.ToString()) > 0);
			}
			if (toYear > lastYear)
			{
				do
				{
					insertedYear = GenerateNextMasterFinYear();
				}
				while (insertedYear != null &&
					string.CompareOrdinal(insertedYear.Year, toYear.ToString()) < 0);
			}
		}

		protected virtual void FillCurrentYearWithPeriods(string status = null, FinPeriod masterFinPeriodStatusSource = null)
		{
			using (new MassInsertingOfPeriodsScope())
			{
				MasterFinPeriod insertedPeriod;
				do
				{
					MasterFinPeriod period = new MasterFinPeriod();

				    if (masterFinPeriodStatusSource != null)
				    {
				        period.Status = masterFinPeriodStatusSource.Status;

				        period.ARClosed = masterFinPeriodStatusSource.ARClosed;
				        period.APClosed = masterFinPeriodStatusSource.APClosed;
				        period.FAClosed = masterFinPeriodStatusSource.FAClosed;
				        period.CAClosed = masterFinPeriodStatusSource.CAClosed;
				        period.INClosed = masterFinPeriodStatusSource.INClosed;
				    }
				    else if (!string.IsNullOrEmpty(status))
					{
						period.Status = status;
						switch (status)
						{
							case FinPeriod.status.Inactive:
							case FinPeriod.status.Open:
								period.APClosed = 
								period.ARClosed = 
								period.CAClosed = 
								period.FAClosed = 
								period.INClosed = false;
								break;
							case FinPeriod.status.Closed:
							case FinPeriod.status.Locked:
								period.APClosed =
								period.ARClosed =
								period.CAClosed =
								period.FAClosed =
								period.INClosed = true;
								break;
						}
					}
					insertedPeriod = Periods.Insert(period);
				}
				while (insertedPeriod != null);
			}
		}

		public PXAction<MasterFinYear> AutoFill;
		[PXButton(Tooltip = Messages.GeneratePeriodsToolTip)]
		[PXUIField(DisplayName = Messages.GeneratePeriods, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable autoFill(PXAdapter adapter)
		{
			FillCurrentYearWithPeriods();
			return adapter.Get();
		}

		protected virtual MasterFinYear InsertPreviousMasterFinYear()
		{
			using (new InsertingOfPreviousYearScope())
			{
				return FiscalYear.Insert();
			}
		}

		protected virtual void ModifyEndYear()
		{
			FinPeriodSaveDialog saveDialog = SaveDialog.Current;
			MasterFinYear year = FiscalYear.Current;
			FinYearSetup setupYear = YearSetup.Current;
			MasterFinPeriod lastPeriod = null;

			if (year != null)
			{
				foreach (MasterFinPeriod period in Periods.Select(year.Year))
				{
					if (lastPeriod == null || ((period.StartDate.Value > lastPeriod.StartDate.Value || period.EndDate.Value > lastPeriod.EndDate.Value) && period.StartDate.Value != period.EndDate))
					{
						lastPeriod = period;
					}
				}
			}

			DateTime lastPeriodCalculatedEndDate = (DateTime)lastPeriod.EndDate;
			if (IsWeekBasedPeriod(setupYear.PeriodType) && setupYear.PeriodsStartDate.Value.DayOfWeek != lastPeriod.EndDate.Value.DayOfWeek && !(bool)saveDialog.MoveDayOfWeek)
			{
				int daysBack = setupYear.PeriodsStartDate.Value.DayOfWeek - lastPeriod.EndDate.Value.DayOfWeek;
				int daysForward = 7 - lastPeriod.EndDate.Value.DayOfWeek - setupYear.PeriodsStartDate.Value.DayOfWeek;

				if (Math.Abs(daysBack) < daysForward && lastPeriod.EndDate.Value.AddDays(daysBack) > lastPeriod.StartDate.Value)
				{
					lastPeriodCalculatedEndDate = lastPeriod.EndDate.Value.AddDays(daysBack);
				}
				else
				{
					lastPeriodCalculatedEndDate = lastPeriod.EndDate.Value.AddDays(daysForward);
				}
				lastPeriod.EndDate = lastPeriodCalculatedEndDate;
				lastPeriod = Periods.Update(lastPeriod);
			}

			switch (saveDialog.Method)
			{
				case FinPeriodSaveDialog.method.UpdateFinYearSetup:
				{
					DateTime? begFinYear;
					DateTime? periodsStartDate;

					TimeSpan dateDifference = (TimeSpan)(year.EndDate - lastPeriod.EndDate);

					if (!FiscalPeriodSetupCreator.IsFixedLengthPeriod(setupYear.FPType))
					{
						if (IsLeapDayPresent(year.EndDate.Value, lastPeriod.EndDate.Value) && !IsLeapDayPresent(setupYear.BegFinYear.Value, setupYear.BegFinYear.Value - dateDifference))
							dateDifference = (dateDifference.Days > 0) ? dateDifference.Subtract(new TimeSpan(1, 0, 0, 0)) : dateDifference.Add(new TimeSpan(1, 0, 0, 0));
						else if (IsLeapDayPresent(year.EndDate.Value, lastPeriod.EndDate.Value, 28) && IsLeapDayPresent(setupYear.BegFinYear.Value, setupYear.BegFinYear.Value - dateDifference))
							dateDifference = (dateDifference.Days > 0) ? dateDifference.Add(new TimeSpan(1, 0, 0, 0)) : dateDifference.Subtract(new TimeSpan(1, 0, 0, 0));
					}

					begFinYear = setupYear.BegFinYear - dateDifference;
					periodsStartDate = setupYear.PeriodsStartDate - dateDifference;

					setupYear.BegFinYear = begFinYear;
					setupYear.PeriodsStartDate = periodsStartDate;
					setupYear.EndYearDayOfWeek = (int)setupYear.PeriodsStartDate.Value.DayOfWeek + 1;

					FiscalYearSetupMaint yearSetup = CreateInstance<FiscalYearSetupMaint>();
					yearSetup.FiscalYearSetup.Update(setupYear);
					if (!FiscalPeriodSetupCreator.IsFixedLengthPeriod(setupYear.FPType))
					{
						yearSetup.AutoFill.Press();
					}
					yearSetup.Save.Press();

					year.BegFinYearHist = begFinYear;
					year.PeriodsStartDateHist = periodsStartDate;
					year.EndDate = lastPeriod.EndDate;
					FiscalYear.Update(year);
					break;
				}
				case FinPeriodSaveDialog.method.UpdateNextYearStart:
					{
						year.EndDate = lastPeriod.EndDate;
						FiscalYear.Update(year);
						break;
					}
				case FinPeriodSaveDialog.method.ExtendLastPeriod:
					{
						lastPeriod.EndDate = year.EndDate;
						Periods.Update(lastPeriod);
						break;
					}
			}
		}

		#endregion

		#region Selects
		public PXFilter<FinPeriodSaveDialog> SaveDialog;
		public PXSelect<MasterFinYear> FiscalYear;

		public PXSelect<
			MasterFinPeriod, 
			Where<MasterFinPeriod.finYear, Equal<Optional<MasterFinYear.year>>>, 
			OrderBy<
				Asc<MasterFinPeriod.periodNbr>>> 
			Periods;

		public PXSelect<OrganizationFinYear> OrganizationYear;
		public PXSelect<OrganizationFinPeriod> OrganizationPeriods;

		public PXSelectReadonly3<FinPeriodSetup, OrderBy<Asc<FinPeriodSetup.periodNbr>>> PeriodsSetup;
		public PXSetup<FinYearSetup> YearSetup;

		#endregion

		#region Fin Year Events

		protected virtual void _(Events.RowDeleting<MasterFinYear> e)
		{
			MasterFinYear masterFinYear = e.Row;

			VerifyMasterFinYearForDelete(masterFinYear);

			if (masterFinYear.StartDate.Value.Month != YearSetup.Current.PeriodsStartDate.Value.Month ||
			  masterFinYear.StartDate.Value.Day != YearSetup.Current.PeriodsStartDate.Value.Day)
			{
				FinYearSetup setupYear = YearSetup.Current;
				MasterFinYear prevYear = PXSelect<
					MasterFinYear, 
					Where<MasterFinYear.year, Less<Current<MasterFinYear.year>>>, 
					OrderBy<
						Desc<MasterFinYear.year>>>
					.SelectWindowed(this, 0, 1);
				if (prevYear != null)
				{
					setupYear.BegFinYear = prevYear.BegFinYearHist;
					setupYear.PeriodsStartDate = prevYear.PeriodsStartDateHist;
					FiscalYearSetupMaint yearSetup = CreateInstance<FiscalYearSetupMaint>();
					yearSetup.FiscalYearSetup.Update(setupYear);
					if (!FiscalPeriodSetupCreator.IsFixedLengthPeriod(((FinYearSetup)YearSetup.Select()).FPType))
					{
						yearSetup.FiscalYearSetup.Current = setupYear;
						yearSetup.AutoFill.Press();
						yearSetup.Save.Press();
					}
					yearSetup.Save.Press();
				}
			}
		}

		protected virtual void VerifyMasterFinYearForDelete(MasterFinYear masterFinYear)
		{
			if (!IsLastMasterFinYear(masterFinYear))
			{
				throw new PXException(Messages.OnlyLastYearCanBeDeleted);
			}

			if (IsMasterFinYearUsed(masterFinYear))
			{
				throw new PXException(Messages.PeriodAlreadyUsed);
			}
		}

		protected bool IsMasterFinYearUsed(MasterFinYear masterFinYear)
		{
			return Periods
				.Select(masterFinYear.Year)
				.RowCast<MasterFinPeriod>()
				.Any(period => period.Status == FinPeriod.status.Closed
					|| period.Status == FinPeriod.status.Locked
					|| period.DateLocked == true)
				|| IsMasterFinYearReferenced(masterFinYear)
				|| IsMasterFinYearUsedInOrganization(masterFinYear.Year)
				;
		}

		private bool IsMasterFinYearReferenced(MasterFinYear masterFinYear)
		{
			using (new PXConnectionScope())
			{
				bool anyBatch = PXSelectReadonly2<Batch,
				InnerJoin<Branch,
					On<Batch.branchID, Equal<Branch.branchID>>,
				InnerJoin<OrganizationFinPeriod,
					On<Branch.organizationID, Equal<OrganizationFinPeriod.organizationID>,
					And<Batch.finPeriodID, Equal<OrganizationFinPeriod.finPeriodID>>>,
				InnerJoin<MasterFinPeriod,
						On<OrganizationFinPeriod.masterFinPeriodID, Equal<MasterFinPeriod.finPeriodID>>>>>,
					Where<MasterFinPeriod.finYear, Equal<Required<MasterFinYear.year>>>>
					.Select(this, masterFinYear.Year).Any(); // Use Linq to Select TOP 1 without ORDER BY
		
				if (anyBatch) { return true;}

				bool anyTran = PXSelectReadonly2<GLTran,
					InnerJoin<Branch,
						On<GLTran.branchID, Equal<Branch.branchID>>,
					InnerJoin<OrganizationFinPeriod,
						On<Branch.organizationID, Equal<OrganizationFinPeriod.organizationID>,
						And<GLTran.finPeriodID, Equal<OrganizationFinPeriod.finPeriodID>>>,
					InnerJoin<MasterFinPeriod,
						On<OrganizationFinPeriod.masterFinPeriodID, Equal<MasterFinPeriod.finPeriodID>>>>>,
					Where<MasterFinPeriod.finYear, Equal<Required<MasterFinYear.year>>>>
					.Select(this, masterFinYear.Year).Any(); // Use Linq to Select TOP 1 without ORDER BY

				return anyTran;
			}
		}

		private bool IsMasterFinPeriodReferenced(string finPeriodID)
		{
			using (new PXConnectionScope())
			{

				bool anyBatch = PXSelectReadonly2<Batch,
					InnerJoin<Branch,
						On<Batch.branchID, Equal<Branch.branchID>>,
					InnerJoin<OrganizationFinPeriod,
						On<Branch.organizationID, Equal<OrganizationFinPeriod.organizationID>,
							And<Batch.finPeriodID, Equal<OrganizationFinPeriod.finPeriodID>>>,
					InnerJoin<MasterFinPeriod,
						On<OrganizationFinPeriod.masterFinPeriodID, Equal<MasterFinPeriod.finPeriodID>>>>>,
					Where<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>
					.Select(this, finPeriodID).Any(); // Use Linq to Select TOP 1 without ORDER BY

				if (anyBatch) { return true; }

				bool anyTran = PXSelectReadonly2<GLTran,
					InnerJoin<Branch,
						On<GLTran.branchID, Equal<Branch.branchID>>,
					InnerJoin<OrganizationFinPeriod,
						On<Branch.organizationID, Equal<OrganizationFinPeriod.organizationID>,
							And<GLTran.finPeriodID, Equal<OrganizationFinPeriod.finPeriodID>>>,
					InnerJoin<MasterFinPeriod,
						On<OrganizationFinPeriod.masterFinPeriodID, Equal<MasterFinPeriod.finPeriodID>>>>>,
					Where<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>
					.Select(this, finPeriodID).Any(); // Use Linq to Select TOP 1 without ORDER BY

				return anyTran;
			}
		}

		private bool IsMasterFinYearUsedInOrganization(string year)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>())
			{
				return PXSelectReadonly2<OrganizationFinPeriod,
						InnerJoin<MasterFinPeriod, On<OrganizationFinPeriod.masterFinPeriodID, Equal<MasterFinPeriod.finPeriodID>>>,
						Where<MasterFinPeriod.finYear, Equal<Required<MasterFinPeriod.finYear>>>>
					.SelectSingleBound(this, null, year)
					.RowCast<OrganizationFinPeriod>()
					.Any();
			}
			return false;
		}

		private bool IsLastMasterFinYear(MasterFinYear masterFinYear)
		{
			return !PXSelectReadonly<
				MasterFinYear,
				Where<MasterFinYear.year, Greater<Current<MasterFinYear.year>>>>
				.SelectSingleBound(this, new object[] { masterFinYear })
				.RowCast<MasterFinYear>()
				.Any();

		}

		protected virtual void MasterFinYear_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			MasterFinYear year = (MasterFinYear)e.Row;
			if (year == null) return;

			MasterFinYear nextYear = GetNextMasterFinYear(year);
			PXUIFieldAttribute.SetEnabled<MasterFinYear.customPeriods>(cache, year, (!year.CustomPeriods ?? true) && nextYear == null);

			Periods.Cache.AllowDelete = (year.CustomPeriods == true) && nextYear == null;

			bool isInsertedCustomYear = HasInsertedYear && year.CustomPeriods == true;
			AutoFill.SetVisible(isInsertedCustomYear);

			GenerateMasterCalendarExtension generateExtension = GetExtension<GenerateMasterCalendarExtension>();
			if (generateExtension != null)
			{
				generateExtension.GenerateYears.SetEnabled(!isInsertedCustomYear && Periods.Cache.IsDirty != true);
			}

			PXResult<MasterFinYear, FinYearSetup> first = (PXResult<MasterFinYear, FinYearSetup>)PXSelectJoinOrderBy<
				MasterFinYear, 
				InnerJoin<FinYearSetup, 
					On<FinYearSetup.firstFinYear, LessEqual<MasterFinYear.year>>>, 
				OrderBy<
					Asc<MasterFinYear.year>>>
				.Select(this);

			bool customPeriodsEnabled = year.CustomPeriods ?? false;

			if (customPeriodsEnabled)
			{
				year.FinPeriods = (short?)PXSelect<
					MasterFinPeriod,
					Where<MasterFinPeriod.finYear, Equal<Required<MasterFinPeriod.finYear>>>>
					.Select(this, year.Year)
					.Count;
			}
			PXUIFieldAttribute.SetVisible(Periods.Cache, typeof(MasterFinPeriod.length).Name, customPeriodsEnabled);
			PXUIFieldAttribute.SetVisible(Periods.Cache, typeof(MasterFinPeriod.isAdjustment).Name, customPeriodsEnabled);
		}

		protected virtual void MasterFinYear_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (HasInsertedYear)
			{
				e.Cancel = true;
				return;
			}
			MasterFinYear yr = (MasterFinYear)e.Row;
			bool result = InsertingOfPreviousYearScope.IsActive ? CreatePrevYear(yr) : CreateNextYear(yr);
			if (!result)
			{
				e.Cancel = true;
			}
		}
		#endregion

		#region Fin Period Events
		protected virtual void MasterFinPeriod_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			MasterFinPeriod period = (MasterFinPeriod)e.Row;
			MasterFinYear year = FiscalYear.Current;

			MasterFinPeriod lastPeriod = PXSelect<
				MasterFinPeriod, 
				Where<MasterFinPeriod.finYear, Equal<Current<MasterFinYear.year>>>, 
				OrderBy<
					Desc<MasterFinPeriod.periodNbr>>>
				.SelectWindowed(this, 0, 1);

			int yearNbr = year.StartDate.Value.Year;

			period.Custom = FiscalYear.Current.CustomPeriods == true && e.ExternalCall;

			FiscalPeriodCreator<MasterFinYear, MasterFinPeriod> creator = 
				new FiscalPeriodCreator<MasterFinYear, MasterFinPeriod>(
					YearSetup.Current, 
					year.Year, 
					year.StartDate.Value, 
					PeriodsSetup
						.Select()
						.RowCast<FinPeriodSetup>())
				{
					Graph = this
				};
			if (!creator.fillNextPeriod(period, lastPeriod) && period.Custom != true)
			{
				if (MassInsertingOfPeriodsScope.IsActive)
				{
					e.Cancel = true;
				}
				else
				{
					throw new PXException(Messages.DataInconsistent);
				}
			}
		}

		protected virtual void MasterFinPeriod_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			MasterFinPeriod row = (MasterFinPeriod)e.Row;
			MasterFinYear year = FiscalYear.Current;
			if (row.EndDate < row.StartDate)
			{
				row.EndDateUI = row.StartDate;
				PXUIFieldAttribute.SetError<MasterFinPeriod.endDateUI>(cache, row, Messages.FiscalPeriodEndDateLessThanStartDate);
			}
		}

		protected virtual void MasterFinPeriod_EndDateUI_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			MasterFinPeriod row = (MasterFinPeriod)e.Row;
			if (row == null) return;
			if ((DateTime)e.NewValue < row.StartDateUI)
			{
				throw new PXSetPropertyException(Messages.FiscalPeriodEndDateLessThanStartDate);
			}
		}

		protected virtual void MasterFinPeriod_EndDateUI_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			MasterFinPeriod row = (MasterFinPeriod)e.Row;
			MasterFinYear year = FiscalYear.Current;
			PXSelectBase<
				MasterFinPeriod> nextPeriodSelect = new PXSelect<MasterFinPeriod, 
				Where<MasterFinPeriod.finYear, Equal<Current<MasterFinYear.year>>, 
					And<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>>(this);
			MasterFinPeriod nextPeriod = nextPeriodSelect.Select((int.Parse(row.FinPeriodID) + 1).ToString());
			if (nextPeriod != null && 
				e.OldValue != null && 
				row.EndDateUI != null && 
				(row.EndDateUI < (DateTime)e.OldValue || (row.EndDateUI > (DateTime)e.OldValue && row.EndDate.Value.AddDays(1) < nextPeriod.EndDate)))
			{
				if (nextPeriod.StartDate == nextPeriod.EndDate)
					nextPeriod.EndDate = row.EndDate;
				nextPeriod.StartDate = row.EndDate;
				nextPeriod.Custom = true;
				Periods.Update(nextPeriod);
			}
			else
			{
				do
				{
					if (nextPeriod != null && nextPeriod.EndDate <= row.EndDate)
					{
						MasterFinPeriod tmpPeriod = nextPeriodSelect.Select((int.Parse(nextPeriod.FinPeriodID) + 1).ToString());
						if (nextPeriod.StartDate != nextPeriod.EndDate || (nextPeriod.StartDate == nextPeriod.EndDate && tmpPeriod != null))
						{
							Periods.Delete(nextPeriod);
						}
						else if (tmpPeriod == null)
						{
							break;
						}
						nextPeriod = tmpPeriod;
					}
				}
				while (nextPeriod != null && nextPeriod.EndDate <= row.EndDate);
				if (nextPeriod != null)
				{
					List<MasterFinPeriod> periodsLeft = new List<MasterFinPeriod>();
					foreach (MasterFinPeriod period in PXSelect<
						MasterFinPeriod, 
						Where<MasterFinPeriod.finYear, Equal<Current<MasterFinYear.year>>, 
							And<MasterFinPeriod.finPeriodID, Greater<Required<MasterFinPeriod.finPeriodID>>>>>
						.Select(this, (int.Parse(row.FinPeriodID).ToString())))
					{
						periodsLeft.Add(period);
						Periods.Delete(period);
					}

					if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(this.YearSetup.Current.FPType))
					{
						foreach (MasterFinPeriod period in periodsLeft)
						{
							Periods.Insert(period);
						}
					}
					else
					{
						foreach (MasterFinPeriod period in periodsLeft)
						{
							MasterFinPeriod newPeriod = new MasterFinPeriod();
							newPeriod = Periods.Insert(period);
							Periods.Cache.SetDefaultExt<MasterFinPeriod.noteID>(newPeriod);
							if (period.StartDate > row.EndDate)
							{
								newPeriod.StartDate = period.StartDate;
							}
							else
							{
								newPeriod.StartDate = row.EndDate;
							}
							newPeriod.EndDate = period.EndDate;
							if (period.StartDate == period.EndDate)
								newPeriod.EndDate = newPeriod.StartDate;
							newPeriod.Descr = period.Descr;
							Periods.Update(newPeriod);
						}
					}
				}
			}
			Periods.View.RequestRefresh();
			row.Custom = true;
		}

		protected virtual void MasterFinPeriod_IsAdjustment_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			MasterFinPeriod row = (MasterFinPeriod)e.Row;
			if (e.NewValue != null && (bool)e.NewValue == true)
			{
				row.EndDate = row.StartDate;
				Periods.Update(row);
			}
		}

		protected virtual void MasterFinPeriod_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			MasterFinPeriod masterFinPeriod = (MasterFinPeriod)e.Row;
			if (masterFinPeriod == null) return;
			MasterFinYear year = this.FiscalYear.Current;
			if (year == null) return;
			MasterFinYear nextYear = this.GetNextMasterFinYear(year);
			bool customPeriodsEnabled = FiscalYear.Current.CustomPeriods ?? false;
			if (customPeriodsEnabled && nextYear == null &&
				PXSelect<
					MasterFinPeriod, 
					Where<MasterFinPeriod.periodNbr, GreaterEqual<Required<MasterFinPeriod.periodNbr>>,
						And<MasterFinPeriod.finYear, Equal<Current<MasterFinYear.year>>,
						And<Where<MasterFinPeriod.dateLocked, Equal<True>,
							Or<MasterFinPeriod.status, Equal<FinPeriod.status.open>>>>>>>
					.Select(this, masterFinPeriod.PeriodNbr)
					.Count == 0)
			{
				PXUIFieldAttribute.SetEnabled<MasterFinPeriod.endDateUI>(cache, masterFinPeriod, true);
			}

			if (year.FinPeriods != null 
				&& !string.IsNullOrWhiteSpace(masterFinPeriod.PeriodNbr) 
				&& masterFinPeriod.IsAdjustment != true)
			{
				PXUIFieldAttribute.SetEnabled<MasterFinPeriod.isAdjustment>(
					cache, 
					masterFinPeriod,
					year.FinPeriods == Int16.Parse(masterFinPeriod.PeriodNbr) 
						&& nextYear == null);
			}

			bool isInsertedCustomYear = HasInsertedYear && year.CustomPeriods == true;
			Periods.Cache.AllowInsert = (year.CustomPeriods == true) && nextYear == null && !HasAdjustmentPeriod;
			AutoFill.SetEnabled(isInsertedCustomYear && !HasInsertedPeriods);

		}

		protected virtual void MasterFinYear_CustomPeriods_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (Boolean.Parse(e.NewValue.ToString()) == true)
			{
				if (FiscalYear.Ask(Messages.FiscalYearCustomPeriodsMessageTitle, Messages.FiscalYearCustomPeriodsMessage, MessageButtons.YesNo, MessageIcon.Warning) == WebDialogResult.No)
				{
					e.NewValue = false;
				}
			}
		}

		protected virtual void MasterFinPeriod_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			MasterFinPeriod period = (MasterFinPeriod)e.Row;
			PXEntryStatus status = this.Periods.Cache.GetStatus(period);
			PXEntryStatus yearStatus = this.FiscalYear.Cache.GetStatus(this.FiscalYear.Current);
			bool isYearDeleting = PXEntryStatus.InsertedDeleted == yearStatus || PXEntryStatus.Deleted == yearStatus;
			if (status == PXEntryStatus.Notchanged || status == PXEntryStatus.Updated || status == PXEntryStatus.Deleted)
			{
				if (period.Status == FinPeriod.status.Closed
					|| period.Status == FinPeriod.status.Locked 
					|| period.DateLocked == true)
				{
					throw new PXException(Messages.PeriodAlreadyUsed);
				}
			}
			int periodNbr = 0;
			bool isIntialized = int.TryParse(period.PeriodNbr, out periodNbr);
			if (isIntialized)
			{
				bool isLastPeriod = (this.GetNextPeriod(period) == null);
				bool hasLaterPeriods = false;
				//Check inserted records
				if (isLastPeriod)
				{
					string year = period.FinYear;
					foreach (MasterFinPeriod iPer in this.Periods.Select(year))
					{
						if (iPer.StartDate.Value > period.StartDate.Value)
						{
							hasLaterPeriods = false;
							break;
						}
					}
				}
				if (((!(isYearDeleting || isLastPeriod)) || hasLaterPeriods) && e.ExternalCall)
				{
					throw new PXException(Messages.DeleteSubseqPeriods);
				}
			}
		}

		protected virtual void MasterFinPeriod_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			MasterFinPeriod fp = (MasterFinPeriod)e.Row;

			string periodID = string.Concat(fp.FinYear, fp.PeriodNbr);
			if (fp.FinPeriodID != periodID)
			{
				throw new PXException(Messages.InconsistentFinPeriodID, FinPeriodIDFormattingAttribute.FormatForError(fp.FinPeriodID), fp.FinYear, fp.PeriodNbr);
			}

			if (e.Operation.Command() == PXDBOperation.Delete &&
				IsMasterFinPeriodReferenced(fp.FinPeriodID))
				{
					throw new PXException(Messages.FinancialPeriodCanNotBeDeleted, fp.FinPeriodID);
				}
			}

		#endregion

		#region FinPeriodSaveDialog events
		protected virtual void FinPeriodSaveDialog_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FinPeriodSaveDialog row = (FinPeriodSaveDialog)e.Row;
			if (row == null) return;
			MasterFinYear year = FiscalYear.Current;
			if (year == null) return;
			FinYearSetup setupYear = YearSetup.Current;
			if (setupYear == null) return;
			MasterFinPeriod lastPeriod = null;
			int count = 0;
			//Scan existing periods
			if (year != null)
			{
				foreach (MasterFinPeriod iPer in Periods.Select(year.Year))
				{
					count++;
					PXEntryStatus status = Periods.Cache.GetStatus(iPer);
					if (lastPeriod == null)
						lastPeriod = iPer;
					else
						if ((iPer.StartDate.Value > lastPeriod.StartDate.Value || iPer.EndDate.Value > lastPeriod.EndDate.Value) && iPer.StartDate.Value != iPer.EndDate.Value)
							lastPeriod = iPer;
				}
			}
			if (lastPeriod == null) return;
			Dictionary<string, string> allowed = new FinPeriodSaveDialog.method.ListAttribute().ValueLabelDic;
			if (!FiscalPeriodSetupCreator.IsFixedLengthPeriod(((FinYearSetup)YearSetup.Select()).FPType))
			{
				allowed.Remove(FinPeriodSaveDialog.method.UpdateNextYearStart);
			}
			if (year.EndDate < lastPeriod.EndDate)
			{
				allowed.Remove(FinPeriodSaveDialog.method.ExtendLastPeriod);
			}

			PXStringListAttribute.SetList<FinPeriodSaveDialog.method>(sender, e.Row,
														   allowed.Keys.ToArray(),
														   allowed.Values.ToArray());

			DateTime lastPeriodCalculatedEndDate = new DateTime();
			if (IsWeekBasedPeriod(setupYear.PeriodType) && setupYear.PeriodsStartDate.Value.DayOfWeek != lastPeriod.EndDate.Value.DayOfWeek)
			{
				PXUIFieldAttribute.SetVisible<FinPeriodSaveDialog.moveDayOfWeek>(sender, row, true);
				int daysBack = setupYear.PeriodsStartDate.Value.DayOfWeek - lastPeriod.EndDate.Value.DayOfWeek;
				int daysForward = 7 - lastPeriod.EndDate.Value.DayOfWeek - setupYear.PeriodsStartDate.Value.DayOfWeek;

				if (Math.Abs(daysBack) < daysForward && lastPeriod.EndDate.Value.AddDays(daysBack) > lastPeriod.StartDate.Value)
				{
					lastPeriodCalculatedEndDate = lastPeriod.EndDate.Value.AddDays(daysBack);
				}
				else
				{
					lastPeriodCalculatedEndDate = lastPeriod.EndDate.Value.AddDays(daysForward);
				}
			}

			switch (row.Method)
			{
				case FinPeriodSaveDialog.method.UpdateNextYearStart:
					{
						row.MethodDescription = PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodModifyNextYear, lastPeriod.EndDate.Value.ToShortDateString());
						if (IsWeekBasedPeriod(setupYear.PeriodType) && setupYear.PeriodsStartDate.Value.DayOfWeek != lastPeriod.EndDate.Value.DayOfWeek)
						{
							if (!(bool)row.MoveDayOfWeek)
							{
								row.MethodDescription = PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodModifyNextYear, lastPeriodCalculatedEndDate.ToShortDateString());
								row.MethodDescription += PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodEndDateMoveWarning, lastPeriodCalculatedEndDate.AddDays(-1).ToShortDateString(), setupYear.PeriodsStartDate.Value.DayOfWeek.ToString());
							}
							else
								row.MethodDescription += PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodWeekStartWarning, setupYear.PeriodsStartDate.Value.DayOfWeek.ToString(), lastPeriod.EndDate.Value.DayOfWeek.ToString());
						}
						break;
					}
				case FinPeriodSaveDialog.method.UpdateFinYearSetup:
					{
						TimeSpan dateDifference = (TimeSpan)(lastPeriod.EndDate - year.EndDate);

						int daysToAdd = 0;
						if (!FiscalPeriodSetupCreator.IsFixedLengthPeriod(((FinYearSetup)YearSetup.Select()).FPType))
						{
							if (IsLeapDayPresent(year.EndDate.Value, lastPeriod.EndDate.Value) && !IsLeapDayPresent(setupYear.BegFinYear.Value, setupYear.BegFinYear.Value + dateDifference))
								daysToAdd = -1;
							else if (IsLeapDayPresent(year.EndDate.Value, lastPeriod.EndDate.Value, 28) && IsLeapDayPresent(setupYear.BegFinYear.Value, setupYear.BegFinYear.Value + dateDifference))
								daysToAdd = 1;
						}

						if ((lastPeriod.EndDate - year.EndDate).Value.Days > 0)
						{
							if (IsCalendarBasedPeriod(setupYear.PeriodType)) row.MethodDescription = PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodModifyNextYearSetupDate, setupYear.BegFinYear.Value.AddDays(Math.Abs((lastPeriod.EndDate - year.EndDate).Value.Days) + daysToAdd).ToString("MMMM dd"));
							else row.MethodDescription = PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodModifyNextYearSetupForward, (lastPeriod.EndDate - year.EndDate).Value.Days + daysToAdd);
						}
						else
						{
							if (IsCalendarBasedPeriod(setupYear.PeriodType)) row.MethodDescription = PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodModifyNextYearSetupDate, setupYear.BegFinYear.Value.AddDays(-Math.Abs((lastPeriod.EndDate - year.EndDate).Value.Days) - daysToAdd).ToString("MMMM dd"));
							else row.MethodDescription = PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodModifyNextYearSetupBack, Math.Abs((lastPeriod.EndDate - year.EndDate).Value.Days) + daysToAdd);
						}
						if (IsWeekBasedPeriod(setupYear.PeriodType) && setupYear.PeriodsStartDate.Value.DayOfWeek != lastPeriod.EndDate.Value.DayOfWeek)
						{
							if (!(bool)row.MoveDayOfWeek)
							{
								if (lastPeriodCalculatedEndDate != year.EndDate && (lastPeriod.EndDate - year.EndDate).Value.Days != 0)
								{
									if ((lastPeriod.EndDate - year.EndDate).Value.Days > 0)
									{
										row.MethodDescription = PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodModifyNextYearSetupForward, (lastPeriodCalculatedEndDate - year.EndDate).Value.Days);
									}
									else
									{
										row.MethodDescription = PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodModifyNextYearSetupBack, Math.Abs((lastPeriodCalculatedEndDate - year.EndDate).Value.Days));
									}
								}
								else
								{
									row.MethodDescription = Messages.FiscalPeriodMethodModifyNextYearNoChange;
								}
								row.MethodDescription += PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodEndDateMoveWarning, lastPeriodCalculatedEndDate.AddDays(-1).ToShortDateString(), setupYear.PeriodsStartDate.Value.DayOfWeek.ToString());
							}
							else
							{
								row.MethodDescription += PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodWeekStartWarning, setupYear.PeriodsStartDate.Value.DayOfWeek.ToString(), lastPeriod.EndDate.Value.DayOfWeek.ToString());
							}
						}
						break;
					}
				case FinPeriodSaveDialog.method.ExtendLastPeriod:
					{
						row.MethodDescription = PXMessages.LocalizeFormatNoPrefix(Messages.FiscalPeriodMethodExtendLastPeriod, year.EndDate.Value.AddDays(-1).ToShortDateString());
						break;
					}
			}
		}

		#endregion

		[Obsolete("The MasterFinPeriodMaint.SynchronizeBaseAndOrganizationPeriods must be deleted in 2019r2")]
		public void SynchronizeBaseAndOrganizationPeriods()
		{
			SynchronizeMasterAndOrganizationPeriods();
		}

		public void SynchronizeMasterAndOrganizationPeriods()
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>()) return;

			PXCache<MasterFinYear> masterYearCache = this.Caches<MasterFinYear>();
			PXCache<MasterFinPeriod> masterPeriodCache = this.Caches<MasterFinPeriod>();
			PXCache<OrganizationFinYear> orgYearCache = this.Caches<OrganizationFinYear>();
			PXCache<OrganizationFinPeriod> orgPeriodCache = this.Caches<OrganizationFinPeriod>();

			// Do not clear the OrganizationFinYear and OrganizationFinPeriod caches. 
			// They may contain the records deleted by PXParentAttribute.

			int[] organisationIDs = null;
			PXSelectBase<Organization> organizationsSelect = new PXSelect<Organization>(this);
			using (new PXReadBranchRestrictedScope())
			{
				foreach (Organization organization in organizationsSelect.Select(organisationIDs))
				{
					PXProcessing.SetCurrentItem(organization);

					try
					{
						foreach (MasterFinYear updatedMasterYear in masterYearCache.Updated)
						{
							OrganizationFinYear updatableOrgYear = PXSelect<
								OrganizationFinYear,
								Where<OrganizationFinYear.organizationID, Equal<Required<Organization.organizationID>>,
									And<OrganizationFinYear.year, Equal<Required<MasterFinYear.year>>>>>
								.Select(this, organization.OrganizationID, updatedMasterYear.Year);

							if (updatableOrgYear == null)
							{
								throw new PXException(Messages.InconsistentCentralizedOrganizationYear, updatedMasterYear.Year, organization.OrganizationCD);
							}

							updatableOrgYear.StartMasterFinPeriodID = GL.FinPeriods.FinPeriodUtils.GetFirstFinPeriodIDOfYear(updatedMasterYear);
							updatableOrgYear.FinPeriods = updatedMasterYear.FinPeriods;
							updatableOrgYear.StartDate = updatedMasterYear.StartDate;
							updatableOrgYear.EndDate = updatedMasterYear.EndDate;
							orgYearCache.Update(updatableOrgYear);
						}

						foreach (MasterFinYear insertedMasterYear in masterYearCache.Inserted)
						{
							OrganizationFinYear insertedOrgYear = (OrganizationFinYear)orgYearCache.Insert(new OrganizationFinYear
							{
								OrganizationID = organization.OrganizationID,
								Year = insertedMasterYear.Year,
								FinPeriods = insertedMasterYear.FinPeriods,
								StartMasterFinPeriodID = GL.FinPeriods.FinPeriodUtils.GetFirstFinPeriodIDOfYear(insertedMasterYear),
								StartDate = insertedMasterYear.StartDate,
								EndDate = insertedMasterYear.EndDate
							});

							if (insertedOrgYear == null)
							{
								throw new PXException(Messages.CannotInsertOrganizationYear, insertedMasterYear.Year, organization.OrganizationCD);
							}
						}

						foreach (MasterFinPeriod deletedMasterPeriod in masterPeriodCache.Deleted)
						{
							OrganizationFinPeriod deletingOrgPeriod = PXSelect<
								OrganizationFinPeriod,
								Where<OrganizationFinPeriod.organizationID, Equal<Required<Organization.organizationID>>,
									And<OrganizationFinPeriod.masterFinPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>>
								.Select(this, organization.OrganizationID, deletedMasterPeriod.FinPeriodID);

							if (deletingOrgPeriod == null)
							{
								throw new PXException(
									Messages.InconsistentCentralizedOrganizationPeriod,
									FinPeriodIDFormattingAttribute.FormatForError(deletedMasterPeriod.FinPeriodID),
									organization.OrganizationCD);
							}

							orgPeriodCache.Delete(deletingOrgPeriod);
						}

						bool isCentralizedManagement = PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>();

						foreach (MasterFinPeriod updatedMasterPeriod in masterPeriodCache.Updated)
						{
							OrganizationFinPeriod updatableOrgPeriod = PXSelect<
								OrganizationFinPeriod,
								Where<OrganizationFinPeriod.organizationID, Equal<Required<Organization.organizationID>>,
									And<OrganizationFinPeriod.masterFinPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>>
								.Select(this, organization.OrganizationID, updatedMasterPeriod.FinPeriodID);

							if (updatableOrgPeriod == null)
							{
								throw new PXException(
									Messages.InconsistentCentralizedOrganizationPeriod,
									FinPeriodIDFormattingAttribute.FormatForError(updatedMasterPeriod.FinPeriodID),
									organization.OrganizationCD);
							}

							updatableOrgPeriod.MasterFinPeriodID = updatedMasterPeriod.FinPeriodID;
							updatableOrgPeriod.FinYear = updatedMasterPeriod.FinYear;
							updatableOrgPeriod.PeriodNbr = updatedMasterPeriod.PeriodNbr;
							updatableOrgPeriod.Custom = updatedMasterPeriod.Custom;
							updatableOrgPeriod.DateLocked = updatedMasterPeriod.DateLocked;
							updatableOrgPeriod.StartDate = updatedMasterPeriod.StartDate;
							updatableOrgPeriod.EndDate = updatedMasterPeriod.EndDate;

							updatableOrgPeriod.Descr = updatedMasterPeriod.Descr;
							PXDBLocalizableStringAttribute.CopyTranslations<MasterFinPeriod.descr, OrganizationFinPeriod.descr>(
								Periods.Cache,
								updatedMasterPeriod,
								orgPeriodCache,
								updatableOrgPeriod);

							if (isCentralizedManagement)
							{
								updatableOrgPeriod.Status = updatedMasterPeriod.Status;
								updatableOrgPeriod.ARClosed = updatedMasterPeriod.ARClosed;
								updatableOrgPeriod.APClosed = updatedMasterPeriod.APClosed;
								updatableOrgPeriod.FAClosed = updatedMasterPeriod.FAClosed;
								updatableOrgPeriod.CAClosed = updatedMasterPeriod.CAClosed;
								updatableOrgPeriod.INClosed = updatedMasterPeriod.INClosed;
							}

							orgPeriodCache.Update(updatableOrgPeriod);
						}

						foreach (MasterFinPeriod insertedMasterPeriod in masterPeriodCache.Inserted)
						{
							OrganizationFinPeriod insertedOrgPeriod = (OrganizationFinPeriod)orgPeriodCache.Insert(new OrganizationFinPeriod
							{
								OrganizationID = organization.OrganizationID,
								FinPeriodID = insertedMasterPeriod.FinPeriodID,
								MasterFinPeriodID = insertedMasterPeriod.FinPeriodID,
								FinYear = insertedMasterPeriod.FinYear,
								PeriodNbr = insertedMasterPeriod.PeriodNbr,
								Custom = insertedMasterPeriod.Custom,
								DateLocked = insertedMasterPeriod.DateLocked,
								StartDate = insertedMasterPeriod.StartDate,
								EndDate = insertedMasterPeriod.EndDate,
								Descr = insertedMasterPeriod.Descr,
							});

							PXDBLocalizableStringAttribute.CopyTranslations<MasterFinPeriod.descr, OrganizationFinPeriod.descr>(
								Periods.Cache,
								insertedMasterPeriod,
								orgPeriodCache,
								insertedOrgPeriod);

							if (isCentralizedManagement || YearsGenerationScope.IsActive)
							{
								insertedOrgPeriod.Status = insertedMasterPeriod.Status;
								insertedOrgPeriod.ARClosed = insertedMasterPeriod.ARClosed;
								insertedOrgPeriod.APClosed = insertedMasterPeriod.APClosed;
								insertedOrgPeriod.FAClosed = insertedMasterPeriod.FAClosed;
								insertedOrgPeriod.CAClosed = insertedMasterPeriod.CAClosed;
								insertedOrgPeriod.INClosed = insertedMasterPeriod.INClosed;
							}

							if (insertedOrgPeriod == null)
							{
								throw new PXException(
									Messages.CannotInsertOrganizationPeriod,
									FinPeriodIDFormattingAttribute.FormatForError(insertedMasterPeriod.FinPeriodID),
									organization.OrganizationCD);
							}
						}
					}
					catch (Exception exc)
					{
						PXProcessing.SetError(exc);
					}
					PXProcessing.SetProcessed();
				}
			}
		}
		
		public override void Persist()
		{
			MasterFinYear year = FiscalYear.Current;
			IEnumerable<MasterFinPeriod> changedPeriods = null;

			using (PXTransactionScope dbTran = new PXTransactionScope())
			{
				if (year != null) // not deleting
				{
					if (Periods.Cache.IsInsertedUpdatedDeleted)
					{
						if (IsYearPeriodsNotMatch(year.Year))
						{
							if (Periods.Ask(Messages.FiscalYearCustomPeriodsMessageTitle, Messages.FinPeriodsChange, MessageButtons.YesNo, MessageIcon.Warning) == WebDialogResult.No)
							{
								return;
							}
						}
					}

					MasterFinPeriod lastPeriod = null;
					PXResultset<MasterFinPeriod> allPeriods = Periods.Select();
					foreach (MasterFinPeriod period in allPeriods)
					{
						if (period.EndDate < period.StartDate)
						{
							throw new PXException(Messages.FiscalPeriodEndDateLessThanStartDate);
						}

						if (lastPeriod == null ||
							period.StartDate.Value > lastPeriod.StartDate.Value ||
							period.EndDate.Value > lastPeriod.EndDate.Value ||
							Int32.Parse(period.FinPeriodID) > Int32.Parse(lastPeriod.FinPeriodID))
						{
							lastPeriod = period;
						}
					}

					if (lastPeriod == null)
					{
						throw new PXException(Messages.FiscalPeriodNoPeriods);
					}

					foreach (MasterFinPeriod period in allPeriods)
					{
						if (period.StartDate == period.EndDate && period != lastPeriod)
						{
							Periods.Cache.RaiseExceptionHandling<MasterFinPeriod.endDateUI>(
								period,
								Periods.Current.EndDateUI,
							new PXSetPropertyException(Messages.FiscalPeriodAdjustmentPeriodError, PXErrorLevel.RowError));
							throw new PXException(Messages.FiscalPeriodAdjustmentPeriodError);
						}
					}

					if (year.EndDate != lastPeriod.EndDate)
					{
						if (SaveDialog.AskExtFullyValid(
							(graph, viewName) =>
							{
								SaveDialog.Current.Message = PXMessages.LocalizeFormatNoPrefix(
									Messages.FiscalPeriodEndDateNotEqualFinYearEndDate,
									lastPeriod.EndDate.Value.AddDays(-1).ToShortDateString(),
									year.EndDate.Value.AddDays(-1).ToShortDateString());
							},
							DialogAnswerType.Positive))
						{
							ModifyEndYear();
						}
						else
							return;
					}
					else
					{
						changedPeriods = GetUpdatedPeriods();
					}

					SynchronizeMasterAndOrganizationPeriods();
				}
				
				base.Persist();

				if (changedPeriods != null)
				{
					UpdateTaxTranFinDate(changedPeriods);
				}

				dbTran.Complete();
			}
		}

		private bool IsYearPeriodsNotMatch(string year)
		{
			foreach (FABookPeriod bookPeriod in
							SelectFrom<FABookPeriod>
								.InnerJoin<FABook>.On<FABook.bookID.IsEqual<FABookPeriod.bookID>>
							.Where<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID.masterValue>
								.And<FABookPeriod.startDate.IsNotEqual<FABookPeriod.endDate>>
								.And<FABook.updateGL.IsEqual<True>>
								.And<FABookPeriod.finYear.IsEqual<@P.AsString>>
							>.View.Select(this, year))
			{
				MasterFinPeriod updatedPeriod = Periods.Cache.Updated.Cast<MasterFinPeriod>().FirstOrDefault(p => p.FinPeriodID == bookPeriod.FinPeriodID);
				if (updatedPeriod != null &&
					(updatedPeriod.StartDate != bookPeriod.StartDate || updatedPeriod.EndDate != bookPeriod.EndDate))
				{
					return true;
				}

				MasterFinPeriod insertedPeriod = Periods.Cache.Inserted.Cast<MasterFinPeriod>().FirstOrDefault(p => p.FinPeriodID == bookPeriod.FinPeriodID);
				if (insertedPeriod != null &&
					(insertedPeriod.StartDate != bookPeriod.StartDate || insertedPeriod.EndDate != bookPeriod.EndDate))
				{
					return true;
				}

				MasterFinPeriod deletedPeriod = Periods.Cache.Deleted.Cast<MasterFinPeriod>().FirstOrDefault(p => p.FinPeriodID == bookPeriod.FinPeriodID);
				if (deletedPeriod != null)
				{
					return true;
				}
			}

			return false;
		}

		private IEnumerable<MasterFinPeriod> GetUpdatedPeriods()
		{
			var olds = PXSelectReadonly<MasterFinPeriod, Where<MasterFinPeriod.finYear, Equal<Current<MasterFinYear.year>>>>.Select(this).RowCast<MasterFinPeriod>();
			var news = this.Periods.Select().RowCast<MasterFinPeriod>();

			return olds.Join(news, fp => fp.FinPeriodID, fp => fp.FinPeriodID, (o, n) => new Tuple<MasterFinPeriod, MasterFinPeriod>(o, n))
				.Where(pair => pair.Item1.EndDate != pair.Item2.EndDate)
				.Select(pair => pair.Item2);
		}

		private void UpdateTaxTranFinDate(IEnumerable<MasterFinPeriod> periods)
		{
			foreach(var period in periods)
			{
				PXUpdate<Set<TX.TaxTran.finDate, Required<TX.TaxTran.finDate>>,
					TX.TaxTran,
					Where<TX.TaxTran.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>,
						And<TX.TaxTran.taxPeriodID, IsNull>>>.Update(this, period.EndDate.Value.AddDays(-1), period.FinPeriodID);
			}
		}

		#region Year Utility Functions
		private bool CreateNextYear(MasterFinYear newYear)
		{
			PXSelectBase select = new PXSelectReadonly<FinYearSetup>(this);
			select.View.Clear();
			FinYearSetup setupValue = (GL.FinYearSetup)select.View.SelectSingle();

			MasterFinYear lastYear = FindLatestYear();
			return FiscalYearCreator<MasterFinYear, MasterFinPeriod>.CreateNextYear(setupValue, lastYear, newYear);
		}

		private bool CreatePrevYear(MasterFinYear newYear)
		{
			PXSelectBase select = new PXSelectReadonly<FinYearSetup>(this);
			select.View.Clear();
			FinYearSetup setupValue = (FinYearSetup)select.View.SelectSingle();

			MasterFinYear earliestYear = FindEarliestYear();

			return FiscalYearCreator<MasterFinYear, MasterFinPeriod>.CreatePrevYear(setupValue, earliestYear, newYear);
		}

		private bool HasInsertedYear => FiscalYear.Cache.Inserted.Cast<MasterFinYear>().Any();
		private bool HasInsertedPeriods => Periods.Cache.Inserted.Cast<MasterFinPeriod>().Any();
		private bool HasAdjustmentPeriod => Periods.Select().RowCast<MasterFinPeriod>().Any(period => period.IsAdjustment == true);

		// TODO: move to IFinPeriodRepository
		private MasterFinYear FindLatestYear()
		{
			PXSelectBase<MasterFinYear> selectLastYear = new PXSelectReadonly3<MasterFinYear, OrderBy<Desc<MasterFinYear.year>>>(this);

			//// Compelled technomagic. Query cache fails again.
			//// The second result of FindLatestYear() is the same as first even after new year persisting.
			selectLastYear.View.Clear();

			return selectLastYear.View.SelectSingle() as MasterFinYear;
		}

		// TODO: move to IFinPeriodRepository
		private MasterFinYear FindEarliestYear()
		{
			PXSelectBase<MasterFinYear> selectFirstYear = new PXSelectReadonly3<MasterFinYear, OrderBy<Asc<MasterFinYear.year>>>(this);

			//// Compelled technomagic. Query cache fails again. 
			//// The second result of FindEarliestYear() is the same as first even after new year persisting.
			selectFirstYear.View.Clear();

			return selectFirstYear.View.SelectSingle() as MasterFinYear;
		}

		//Returns next period (from db) or null if there is none
		// TODO: move to IFinPeriodRepository
		private MasterFinYear GetNextMasterFinYear(MasterFinYear aYear)
		{
			PXSelectBase select = new PXSelect<MasterFinYear, 
				Where<MasterFinYear.startDate, Greater<Required<MasterFinYear.startDate>>>, 
				OrderBy<
					Asc<MasterFinPeriod.startDate>>>(this);
			MasterFinYear result = (MasterFinYear)select.View.SelectSingle(aYear.StartDate.Value);
			return result;
		}

		private bool IsWeekBasedPeriod(string periodType)
		{
			return periodType == FinPeriodType.Week
				|| periodType == FinPeriodType.BiWeek
				|| periodType == FinPeriodType.FourWeek
				|| periodType == FinPeriodType.FourFourFive
				|| periodType == FinPeriodType.FourFiveFour
				|| periodType == FinPeriodType.FiveFourFour;
		}

		private bool IsCalendarBasedPeriod(string periodType)
		{
			return periodType == FinPeriodType.BiMonth
				|| periodType == FinPeriodType.Month
				|| periodType == FinPeriodType.Quarter;
		}

		private bool IsLeapDayPresent(DateTime date1, DateTime date2, int leapDay = 29)
		{
			bool leapDayPresent = false;
			DateTime leapDate1 = DateTime.MinValue;
			DateTime leapDate2 = DateTime.MinValue;
			if (leapDay == 29)
			{
				if (DateTime.IsLeapYear(date1.Year))
					leapDate1 = new DateTime(date1.Year, 2, leapDay);
				if (DateTime.IsLeapYear(date2.Year))
					leapDate2 = new DateTime(date2.Year, 2, leapDay);
			}
			else
			{
				leapDate1 = new DateTime(date1.Year, 2, leapDay);
				leapDate2 = new DateTime(date2.Year, 2, leapDay);
			}

			if (((date1 > date2 && (leapDate1 > date2 && date1 >= leapDate1)) || (date1 < date2 && (leapDate1 <= date2 && date1 < leapDate1))) || 
				((date1 > date2 && (leapDate2 > date2 && date1 >= leapDate2)) || (date1 < date2 && (leapDate2 <= date2 && date1 < leapDate2))))
			{
				leapDayPresent = true;
			}

			return leapDayPresent;
		}

		#endregion

		#region MasterFinPeriod Utility Functions
		//Returns next period (from db) or null if there is none
		private MasterFinPeriod GetNextPeriod(MasterFinPeriod aPeriod)
		{
			PXSelectBase select = new PXSelect<MasterFinPeriod, 
				Where<MasterFinPeriod.startDate, Greater<Required<MasterFinPeriod.startDate>>>, 
				OrderBy<Asc<MasterFinPeriod.startDate>>>(this);
			MasterFinPeriod result = (MasterFinPeriod)select.View.SelectSingle(aPeriod.StartDate.Value);
			return result;
		}
		#endregion
	}

	public class FiscalYearCreator<TYear, TPeriod> : FiscalPeriodCreator<TYear, TPeriod>
		where TYear : class, GL.IYear, IBqlTable, new()
		where TPeriod : class, GL.IPeriod, IBqlTable, new()
	{

		public FiscalYearCreator(IYearSetup aSetup, string aYear, DateTime? startDate, IEnumerable<IPeriodSetup> aPeriodSetup)
			: base(aSetup, aYear, startDate, aPeriodSetup)
		{
		}

		public static bool CreateNextYear(IYearSetup setupValue, TYear lastYear, TYear newYear)
		{
			if (setupValue == null)
				throw new PXSetPropertyException(Messages.ConfigDataNotEntered);
			int yearNumber = -1;
			if (lastYear != null)
			{
				if (!lastYear.EndDate.HasValue)
				{
					FiscalPeriodSetupCreator<TPeriod> lastYearCreator = new FiscalPeriodSetupCreator<TPeriod>(setupValue, lastYear.Year, lastYear.StartDate.Value,  setupValue.BegFinYear.Value);
					newYear.StartDate = lastYearCreator.YearEnd;
				}
				else
					newYear.StartDate = lastYear.EndDate;
				yearNumber = int.Parse(lastYear.Year);
				yearNumber++;

				string yearNumberAsString = FiscalPeriodSetupCreator.FormatYear(yearNumber);
				DateTime yearStartDate = CalcYearStartDate(setupValue, yearNumberAsString);
				FiscalPeriodSetupCreator<TPeriod> newYearCreator = new FiscalPeriodSetupCreator<TPeriod>(
					setupValue,
					yearNumberAsString,
					newYear.StartDate.Value,
					yearStartDate);
				newYear.EndDate = newYearCreator.YearEnd;
				newYear.FinPeriods = newYearCreator.ActualNumberOfPeriods;
				newYear.Year = FiscalPeriodSetupCreator.FormatYear(yearNumber);
			}
			else
			{
				CreateFromSetup(setupValue, newYear);
			}
			return true;
		}

		public static bool CreatePrevYear(IYearSetup setupValue, TYear firstYear, TYear newYear)
		{
			if (setupValue == null)
				throw new PXSetPropertyException(Messages.ConfigDataNotEntered);

			int yearNumber = -1;
			int setupFirstYear = string.IsNullOrEmpty(setupValue.FirstFinYear) ? setupValue.BegFinYear.Value.Year : int.Parse(setupValue.FirstFinYear); // No need to increment in this case
			if (firstYear != null)
			{
				yearNumber = int.Parse(firstYear.Year);
				yearNumber--;
				if (yearNumber < setupFirstYear)
					return false;
				FiscalPeriodSetupCreator<FinPeriodSetup> creator = new FiscalPeriodSetupCreator<FinPeriodSetup>(setupValue);
				string yearNumberAsString = FiscalPeriodSetupCreator.FormatYear(yearNumber);
				DateTime periodsStartingDate = creator.CalcPeriodsStartDate(yearNumberAsString);
				DateTime yearStartingDate = CalcYearStartDate(setupValue, yearNumberAsString);
				newYear.StartDate = periodsStartingDate;
				FiscalPeriodSetupCreator<FinPeriodSetup> prevYearCreator = new FiscalPeriodSetupCreator<FinPeriodSetup>(
					setupValue,
					yearNumberAsString, 
					newYear.StartDate.Value,
					yearStartingDate);
				newYear.EndDate = prevYearCreator.YearEnd;
				newYear.FinPeriods = prevYearCreator.ActualNumberOfPeriods;
				newYear.Year = FiscalPeriodSetupCreator.FormatYear(yearNumber);
			}
			else
			{
				CreateFromSetup(setupValue, newYear);
			}
			return true;
		}

		public static void CreateFromSetup(IYearSetup setupValue, TYear newYear)
		{
			FiscalPeriodSetupCreator<TPeriod> creator = new FiscalPeriodSetupCreator<TPeriod>(setupValue);
			newYear.StartDate = new DateTime(setupValue.PeriodsStartDate.Value.Year, setupValue.PeriodsStartDate.Value.Month, setupValue.PeriodsStartDate.Value.Day);
			newYear.EndDate = creator.YearEnd;
			newYear.Year = setupValue.FirstFinYear;
			newYear.FinPeriods = FiscalPeriodSetupCreator.IsFixedLengthPeriod(setupValue.FPType) ? (short)creator.ActualNumberOfPeriods : setupValue.FinPeriods;
		}

		public static TYear CreateNextYear(PXGraph graph, IYearSetup setupValue, IEnumerable<IPeriodSetup> periods, TYear lastYear)
		{
			TYear newYear = new TYear();
			CreateNextYear(setupValue, lastYear, newYear);
			if ((newYear = (TYear)graph.Caches[typeof(TYear)].Insert(newYear)) != null)
			{
				CreatePeriods(graph.Caches[typeof(TPeriod)], periods, newYear.Year, newYear.StartDate, setupValue);
			}

			return newYear;
		}

		public static TYear CreatePrevYear(PXGraph graph, IYearSetup setupValue, IEnumerable<IPeriodSetup> periods, TYear firstYear)
		{
			TYear newYear = new TYear();
			CreatePrevYear(setupValue, firstYear, newYear);
			if ((newYear = (TYear)graph.Caches[typeof(TYear)].Insert(newYear)) != null)
			{
				CreatePeriods(graph.Caches[typeof(TPeriod)], periods, newYear.Year, newYear.StartDate, setupValue);
			}

			return newYear;
		}

		public static DateTime CalcYearStartDate(IYearSetup aSetup, string aYearNumber)
		{
			int yearDelta = aSetup.BegFinYear.Value.Year - int.Parse(aSetup.FirstFinYear);
			return new DateTime(int.Parse(aYearNumber) + yearDelta, aSetup.BegFinYear.Value.Month, aSetup.BegFinYear.Value.Day);
		}
	}

	public class FiscalPeriodCreator<TYear, TPeriod>
		where TYear : class, GL.IYear, IBqlTable, new()
		where TPeriod : class, GL.IPeriod, IBqlTable, new()
	{
		public FiscalPeriodCreator(IYearSetup aYearSetup, string aFinYear, DateTime? aPeriodsStartDate, IEnumerable<IPeriodSetup> aPeriods)
		{
			this.yearSetup = aYearSetup;
			this.finYear = aFinYear;
			this.periodsStartDate = aPeriodsStartDate.Value;
			this.periodsSetup = aPeriods;
			//Calc internal variables
			this.yearStartDate = FiscalYearCreator<TYear, TPeriod>.CalcYearStartDate(this.yearSetup, aFinYear);
			this.firstPeriodYear = this.periodsStartDate.Year;
		}

		public PXGraph Graph;

		public virtual void CreatePeriods(PXCache cache)
		{
			Graph = Graph ?? cache.Graph;

			TPeriod iPer = null;
			int count = 0;
			const int maxCount = 1000;
			do
			{
				iPer = this.createNextPeriod(iPer);
				if (iPer != null)
				{
					cache.Insert(iPer);
					count++;
				}
			}
			while (iPer != null && count < maxCount);
			if (count >= maxCount)
				throw new PXException(Messages.ERR_InfiniteLoopDetected);
		}

		public static void CreatePeriods(PXCache cache, IEnumerable<IPeriodSetup> aPeriods, string aYear, DateTime? YearStartDate, IYearSetup aYearSetup)
		{
			new FiscalPeriodCreator<TYear, TPeriod>(aYearSetup, aYear, YearStartDate, aPeriods).CreatePeriods(cache);
		}


		public virtual bool fillNextPeriod(TPeriod result, TPeriod current)
		{
			if (FiscalPeriodSetupCreator.IsFixedLengthPeriod(yearSetup.FPType)
				|| (current != null && (current.Custom ?? false))
				|| (result != null && (result.Custom ?? false)))
			{
				if (!this.FinPeriodCreator.fillNextPeriod(result, current)) return false;
				result.DateLocked = false;
				result.FinYear = finYear;
				result.FinPeriodID = result.FinYear + result.PeriodNbr;
				return true;
			}
			else
			{
				return fillNextFromTemplate(result, current);
			}
		}

		protected virtual bool fillNextFromTemplate(TPeriod result, TPeriod current)
		{
			IPeriodSetup source = null;
			IPeriodSetup prev = null;
			int yearDelta = 0;
			//This algorithm relies on the fact that FiscalPeriodSetups are sorted by PeriodNbr in accending order;
			foreach (IPeriodSetup res in this.periodsSetup)
			{
				IPeriodSetup fps = res;
				yearDelta += GetYearDelta(fps, prev); //accumulate year change for all previous periods 
				if (IsEqual(prev, current))
				{
					source = fps;
					break;
				}
				prev = fps;
			}
			if (source == null) return false;
			int adjustedYear = this.firstPeriodYear + yearDelta;
			CopyFrom(result, source, adjustedYear, Graph);
			result.DateLocked = false;
			return true;
		}

		static int GetYearDelta(IPeriodSetup current, IPeriodSetup prev)
		{
			if (prev != null)
				return (current.StartDate.Value.Year - prev.StartDate.Value.Year);
			return 0;
		}

		static int GetYearDelta(IPeriodSetup current)
		{
			return (current.EndDate.Value.Year - current.StartDate.Value.Year);
		}

		static bool IsEqual(IPeriodSetup op1, TPeriod op2)
		{
			if (op1 == null || op2 == null) return (op1 == null && op2 == null);
			return op1.PeriodNbr == op2.PeriodNbr;
		}

		public virtual TPeriod createNextPeriod(TPeriod current)
		{
			TPeriod result = new TPeriod();
			if (!this.fillNextPeriod(result, current)) return null;
			return result;
		}


		/// <summary>
		/// This function fills result based on aSetup (as template) and current year
		/// Start and End Date of result are adjusted using the following alorithm only month/date parts are used, aYear is used as year ;
		/// If there is year break inside aSetup period, Dates are adjusted accordingly
		/// </summary>
		/// <param name="result">Result of operation (must be not null)</param>
		/// <param name="aSetup">Setup - used as template. ReadOnly.</param>
		/// <param name="aYear">Year</param>
		private void CopyFrom(TPeriod result, IPeriodSetup aSetup, int aYear, PXGraph graph)
		{
			result.PeriodNbr = aSetup.PeriodNbr;
			int newYear = aYear + GetYearDelta(aSetup);
			result.StartDate = new DateTime(aYear, aSetup.StartDate.Value.Month, Math.Min(DateTime.DaysInMonth(aYear, aSetup.StartDate.Value.Month), aSetup.StartDate.Value.Day));
			result.EndDate = new DateTime(newYear, aSetup.EndDate.Value.Month, Math.Min(DateTime.DaysInMonth(newYear, aSetup.EndDate.Value.Month), aSetup.EndDate.Value.Day));
			result.Descr = aSetup.Descr;
			result.FinYear = result.FinYear ?? finYear;
			result.FinPeriodID = result.FinYear + result.PeriodNbr;

			var sourceCache = graph.Caches[aSetup.GetType()];
			var destCache = graph.Caches[result.GetType()];
			PXDBLocalizableStringAttribute.CopyTranslations<FinPeriodSetup.descr, MasterFinPeriod.descr>(sourceCache, aSetup, destCache, result);
		}

		private int firstPeriodYear;
		private string finYear;
		private DateTime yearStartDate;
		private DateTime periodsStartDate;
		private IYearSetup yearSetup;
		private IEnumerable<IPeriodSetup> periodsSetup;
		private FiscalPeriodSetupCreator<TPeriod> _finPeriodCreator = null;

		private FiscalPeriodSetupCreator<TPeriod> FinPeriodCreator
		{
			get
			{
				if (_finPeriodCreator == null)
				{
					DateTime yearStartingDate = yearStartDate;
					_finPeriodCreator = new FiscalPeriodSetupCreator<TPeriod>(
						yearSetup,
						FiscalPeriodSetupCreator.FormatYear(firstPeriodYear),
						periodsStartDate,
						yearStartingDate);
				}
				return _finPeriodCreator;
			}
		}
	}

	[Serializable]
	public partial class FinPeriodSaveDialog : IBqlTable
	{
		#region Message
		[PXString()]
		[PXUIField(Enabled = false)]
		public virtual String Message { get; set; }
		#endregion
		#region Method
		public abstract class method : PX.Data.BQL.BqlString.Field<method>
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
				new[] { UpdateNextYearStart, UpdateFinYearSetup, ExtendLastPeriod },
				new[] { Messages.FinPeriodUpdateNextYearStart, Messages.FinPeriodUpdateFinYearSetup, Messages.FinPeriodExtendLastPeriod }) { }
			}

			public const string UpdateNextYearStart = "N";
			public const string UpdateFinYearSetup = "Y";
			public const string ExtendLastPeriod = "E";
			public const string ShortenLastPeriod = "S";

			public class updateNextYearStart : PX.Data.BQL.BqlString.Constant<updateNextYearStart>
			{
				public updateNextYearStart() : base(UpdateNextYearStart) { }
			}
			public class updateFinYearSetup : PX.Data.BQL.BqlString.Constant<updateFinYearSetup>
			{
				public updateFinYearSetup() : base(UpdateFinYearSetup) { }
			}
			public class extendLastPeriod : PX.Data.BQL.BqlString.Constant<extendLastPeriod>
			{
				public extendLastPeriod() : base(ExtendLastPeriod) { }
			}
			public class shortenLastPeriod : PX.Data.BQL.BqlString.Constant<shortenLastPeriod>
			{
				public shortenLastPeriod() : base(ShortenLastPeriod) { }
			}
			#endregion
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(method.UpdateFinYearSetup)]
		[method.List]
		[PXUIField(DisplayName = "Update Method")]
		public virtual String Method { get; set; }
		#endregion
		#region MoveDayOfWeek
		public abstract class moveDayOfWeek : PX.Data.BQL.BqlBool.Field<moveDayOfWeek> { }
		protected bool? _MoveDayOfWeek;
		[PXBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Move start day of financial period ", Enabled = true, Required = false, Visible = false)]
		public virtual bool? MoveDayOfWeek
		{
			get
			{
				return this._MoveDayOfWeek;
			}
			set
			{
				this._MoveDayOfWeek = value;
			}
		}
		#endregion
		#region MethodDescription
		[PXString()]
		[PXDefault(Messages.FiscalPeriodMethodModifyNextYear)]
		[PXUIField(Enabled = false, Required=false)]
		public virtual String MethodDescription { get; set; }
		#endregion
	}

	[Serializable]
	public partial class FinPeriodGenerateParameters : IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : IBqlField { }

		[PXUIField(DisplayName = "Organization", Enabled = false)]
		[PXUIVisible(typeof(
			Where<FinPeriodGenerateParameters.organizationID, IsNotNull, 
				And<FinPeriodGenerateParameters.organizationID, NotEqual<int0>>>))]
		[Organization]
		public virtual int? OrganizationID { get; set; }
		#endregion

		#region FromYear
		public abstract class fromYear : PX.Data.BQL.BqlString.Field<fromYear> { }

		/// <summary>
		/// The financial year starting from which the periods will be generated in the system.
		/// </summary>
		[PXString(4, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "From Year")]
		public virtual string FromYear { get; set; }
		#endregion
		#region ToYear
		public abstract class toYear : PX.Data.BQL.BqlString.Field<toYear> { }

		/// <summary>
		/// The financial year till which the periods will be generated in the system.
		/// </summary>
		[PXString(4, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "To Year")]
		public virtual string ToYear { get; set; }
		#endregion

		#region FirstFinYear
		public abstract class firstFinYear : PX.Data.BQL.BqlString.Field<firstFinYear> { }
		/// <summary>
		/// The financial year starting from which the periods will be generated in the system.
		/// </summary>
		[PXString(4, IsFixed = true)]
		[PXUIField(DisplayName = "First Year", Enabled = false)]
		public virtual string FirstFinYear { get; set; }
		#endregion
		#region LastFinYear
		public abstract class lastFinYear : PX.Data.BQL.BqlString.Field<lastFinYear> { }

		/// <summary>
		/// The financial year till which the periods will be generated in the system.
		/// </summary>
		[PXString(4, IsFixed = true)]
		[PXUIField(DisplayName = "Last Year", Enabled = false)]
		public virtual string LastFinYear { get; set; }
		#endregion

	}
}
