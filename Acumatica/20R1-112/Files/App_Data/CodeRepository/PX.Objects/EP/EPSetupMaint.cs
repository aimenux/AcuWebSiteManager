using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;
using System.Collections;
using PX.Objects.GL.FinPeriods;
using PX.Objects.PR.Standalone;
using System.Linq;

namespace PX.Objects.EP
{
	public class EPSetupMaint : PXGraph<EPSetupMaint>
	{
		#region Selects Declartion
		public PXSelect<EPSetup>
			Setup;

		public PXSelectReadonly<CMSetup>
			cmsetup;

		public PXSetup<AP.APSetup>
			APSetup;

		public
			PXSelect<WikiPageSimple,
			Where<WikiPageSimple.pageID, Equal<WikiPageSimple.pageID>>,
			OrderBy<Desc<WikiPageSimple.articleType, Asc<WikiPageSimple.number>>>> Articles;

		public PXFilter<EPWeekFilter> WeekFilter;

		public PXSelect<EPCustomWeek, Where<EPCustomWeek.year, Equal<Current<EPWeekFilter.year>>>> CustomWeek;

		public PXFilter<EPGenerateWeeksDialog> GenerateWeeksDialog;
		#endregion

		private bool isGenerate = false;

		public EPSetupMaint()
		{
			if (APSetup.Current == null)
				throw new PXArgumentException("APSetup");

			FieldDefaulting.AddHandler<IN.InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });
		}

		protected virtual IEnumerable articles(string PageID)
		{
			Guid parentID = PX.Common.GUID.CreateGuid(PageID) ?? Guid.Empty;

			PXResultset<WikiPageSimple> result =
				PXSelect<WikiPageSimple,
							Where<WikiPageSimple.parentUID, Equal<Required<WikiPageSimple.parentUID>>>>.Select(this, parentID);

			foreach (WikiPageSimple art in result)
			{
				if (PXSiteMap.WikiProvider.GetAccessRights(art.PageID.Value) >= PXWikiRights.Select)
				{
					PXWikiMapNode node = (PXWikiMapNode)PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(art.PageID.GetValueOrDefault());
					art.Title = node == null ? art.Name : node.Title;
					yield return art;
				}
			}
		}

		#region Actions

		public PXAction<EPSetup> generateWeeks;
		[PXButton()]
		[PXUIField(DisplayName = "Generate Weeks", Visible = true)]
		public IEnumerable GenerateWeeks(PXAdapter adapter)
		{
			if (GenerateWeeksDialog.Current != null && WeekFilter.Current.Year != null)
			{
				DateTime startDate;
				int number;
				int year;
				GetNextUsingWeek(out startDate, out number, out year);
				DateTime? lastUsingDate = GetLasttUsingWeek();
				if (lastUsingDate != null)
					year = Math.Max(year, lastUsingDate.Value.Year);

				GenerateWeeksDialog.Current.FromDate = startDate;
				GenerateWeeksDialog.Current.TillDate = new DateTime(year, 12, 31);
				GenerateWeeksDialog.AskExt();
			}

			return adapter.Get();
		}

		public PXAction<EPSetup> generateWeeksOk;

		[PXButton()]
		[PXUIField(DisplayName = "Generate Weeks")]
		public IEnumerable GenerateWeeksOK(PXAdapter adapter)
		{
			DateTime current;
			int Year;
			int weekNumber;
			isGenerate = true;

			GetNextUsingWeek(out current, out weekNumber, out Year);
			DateTime? LastUsingWeek = GetLasttUsingWeek();

			object fromDate = GenerateWeeksDialog.Current.FromDate;
			object tillDate = GenerateWeeksDialog.Current.TillDate;


			bool isFeryfing = GenerateWeeksDialog.Cache.RaiseFieldVerifying<EPGenerateWeeksDialog.fromDate>(GenerateWeeksDialog.Current, ref fromDate);
			isFeryfing = isFeryfing && GenerateWeeksDialog.Cache.RaiseFieldVerifying<EPGenerateWeeksDialog.tillDate>(GenerateWeeksDialog.Current, ref tillDate);

			if (GenerateWeeksDialog.VerifyRequired() && isFeryfing)
			{
				DateTime? oldCurrent = null; //Use to check Infinite Loop
				DateTime CalendarEndOfWeek;
				while (current <= GenerateWeeksDialog.Current.TillDate.Value)
				{
					int CalendarWeekNumber = PX.Data.EP.PXDateTimeInfo.GetWeekNumber(current);
					int EndOfWeekYear = current.Year;
					if (CalendarWeekNumber == 1 && current.Month == 12) //End Of Week in next Year
						EndOfWeekYear++;
					if (CalendarWeekNumber > 31 && current.Month == 1) //Begin Of Week in previos Year
						EndOfWeekYear--;
					CalendarEndOfWeek = PX.Data.EP.PXDateTimeInfo.GetWeekStart(EndOfWeekYear, CalendarWeekNumber).AddDays(6d);

					if (LastUsingWeek == null || LastUsingWeek < current)
					{
						if (GenerateWeeksDialog.Current.CutOffDayOne == EPGenerateWeeksDialog.CutOffDayListAttribute.FixedDayOfMonth)
						{
							if (current.Day <= GenerateWeeksDialog.Current.DayOne && GenerateWeeksDialog.Current.DayOne < CalendarEndOfWeek.Day)
								CalendarEndOfWeek = new DateTime(current.Year, current.Month, GenerateWeeksDialog.Current.DayOne.Value);
						}
						else if (GenerateWeeksDialog.Current.CutOffDayOne == EPGenerateWeeksDialog.CutOffDayListAttribute.EndOfMonth)
						{
							if (current.Year == CalendarEndOfWeek.Year)
							{
								if (current.Month < CalendarEndOfWeek.Month)
									CalendarEndOfWeek = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));
							}
							else if (current.Year < CalendarEndOfWeek.Year)
							{
								CalendarEndOfWeek = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));
							}
						}

						if (GenerateWeeksDialog.Current.CutOffDayTwo == EPGenerateWeeksDialog.CutOffDayListAttribute.FixedDayOfMonth)
						{
							if (current.Day <= GenerateWeeksDialog.Current.DayTwo && GenerateWeeksDialog.Current.DayTwo < CalendarEndOfWeek.Day)
								CalendarEndOfWeek = new DateTime(current.Year, current.Month, GenerateWeeksDialog.Current.DayTwo.Value);
						}
						else if (GenerateWeeksDialog.Current.CutOffDayTwo == EPGenerateWeeksDialog.CutOffDayListAttribute.EndOfMonth)
						{
							if (current.Month != CalendarEndOfWeek.Month)
								CalendarEndOfWeek = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));
						}
				
					}

					if (CalendarWeekNumber == 1 && current.Year < CalendarEndOfWeek.Year)
					{
						weekNumber = 1;
						Year++;
					}

					EPCustomWeek week = new EPCustomWeek { StartDate = GetBeginOfDate(current), EndDate = GetEndOfDate(CalendarEndOfWeek), IsActive = true, Year = Year, Number = weekNumber };
					CustomWeek.Cache.Insert(week);

					if (weekNumber != 1 && (current.Year < CalendarEndOfWeek.AddDays(1d).Year || Year < current.Year))
					{
						weekNumber = 1;
						Year++;
					}
					else
						weekNumber++;

					current = GetBeginOfDate(CalendarEndOfWeek.AddDays(1d));
					if (oldCurrent == current)
						throw new PXException(Messages.InfiniteLoop);
					oldCurrent = current;
				}
			}
			return adapter.Get();
		}

		#endregion

		#region Buttons Declaration

		public PXSave<EPSetup> Save;
		public PXCancel<EPSetup> Cancel;

		#endregion

		#region Default Instance Accessors

		public CMSetup CMSETUP
		{
			get
			{
				CMSetup setup = cmsetup.Select();
				if (setup == null)
				{
					setup = new CMSetup();
				}
				return setup;
			}
		}

		#endregion

		#region Static utils
		public static string GetPostingOption(PXGraph graph, EPSetup setup, int? employeeID)
		{
			if (PXSelect<PREmployee>.Search<PREmployee.bAccountID>(graph, employeeID).Any())
			{
				return EPPostOptions.DoNotPost;
			}

			return setup.PostingOption;
		}
		#endregion

		#region Events

		protected virtual void EPSetup_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPSetup row = e.Row as EPSetup;
			if (row != null)
			{
                PXUIFieldAttribute.SetEnabled<EPSetup.useReceiptAccountForTips>(cache, row, row.NonTaxableTipItem.HasValue);
				PXUIFieldAttribute.SetEnabled<EPSetup.offBalanceAccountGroupID>(cache, row, row.PostingOption == EPPostOptions.PostToOffBalance);
				if (row.CustomWeek == true)
				{
					EPCustomWeek lastWeek = (EPCustomWeek)PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.weekID>>>.SelectSingleBound(this, null);
					PXUIFieldAttribute.SetEnabled<EPSetup.customWeek>(cache, row, lastWeek == null);
				}

				bool claimApprovalVisible = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>() &&
				                            PXAccess.FeatureInstalled<FeaturesSet.expenseManagement>();

				PXUIFieldAttribute.SetVisible<EPSetup.claimDetailsAssignmentMapID>(cache, null, claimApprovalVisible);
				PXUIFieldAttribute.SetVisible<EPSetup.claimAssignmentMapID>(cache, null, claimApprovalVisible);
				PXUIFieldAttribute.SetVisible<EPSetup.claimDetailsAssignmentNotificationID>(cache, null, claimApprovalVisible);
			}
		}

	    protected virtual void EPSetup_PostToOffBalance_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
	    {
	        EPSetup row = e.Row as EPSetup;
	        if (row == null) return;

	        if (row.PostingOption != EPPostOptions.PostToOffBalance)
	            row.OffBalanceAccountGroupID = null;

	    }

		protected virtual void EPSetup_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = e.Row as EPSetup;
			if (row == null || e.Operation == PXDBOperation.Delete) return;
			if (row.PostingOption == EPPostOptions.PostToOffBalance && row.OffBalanceAccountGroupID == null)
			{
				if (cache.RaiseExceptionHandling<EPSetup.offBalanceAccountGroupID>(e.Row, row.OffBalanceAccountGroupID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<EPSetup.offBalanceAccountGroupID>(cache))))
				{
					throw new PXRowPersistingException(typeof(EPSetup.offBalanceAccountGroupID).Name, row.OffBalanceAccountGroupID, ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<EPSetup.offBalanceAccountGroupID>(cache));
				}
			}
			if (row.CustomWeek == true)
			{
				DateTime? firstUsing = GetFirstActivityDate();
				DateTime? lastUsing = GetLasttUsingWeek();

				if (firstUsing != null && lastUsing != null)
				{
					EPCustomWeek firstWeek = PXSelectOrderBy<EPCustomWeek, OrderBy<Asc<EPCustomWeek.weekID>>>.SelectWindowed(this, 0, 1);
					EPCustomWeek lastWeek = PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.weekID>>>.SelectWindowed(this, 0, 1);

					foreach (EPCustomWeek week in CustomWeek.Cache.Inserted)
					{
						if (lastWeek == null)
						{
							lastWeek = week;
						}

						if (week.WeekID > lastWeek.WeekID)
						{
							lastWeek = week;
						}
					}

					if (firstWeek == null || firstUsing < firstWeek.StartDate || lastWeek == null || lastWeek.EndDate < lastUsing)
						throw new PXRowPersistingException(typeof(EPSetup.customWeek).Name, row.CustomWeek, Messages.CustomWeekNotCreated, firstUsing, lastUsing);
				}
			}
		}

		

	protected virtual void EPSetup_OvertimeMultiplier_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null || (decimal)(e.NewValue) <= 0m)
			{
				throw new PXSetPropertyException(Messages.ValueMustBeGreaterThanZero);
			}
		}

		protected virtual void EPSetup_DefaultActivityType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null)
				return;
			EPActivityType activityType = (EPActivityType) PXSelect<EPActivityType>.Search<EPActivityType.type>(this, e.NewValue);
			if (activityType == null || activityType.RequireTimeByDefault != true)
			{
				throw new PXSetPropertyException(Messages.defaultActivityTypeNoTracTime, PXErrorLevel.Error);
			}

		}
		protected virtual void EPSetup_NonTaxableTipItem_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null)
			{
				TX.TaxCategory category = PXSelectJoin<TX.TaxCategory,
					InnerJoin<IN.InventoryItem,
						On<IN.InventoryItem.taxCategoryID, Equal<TX.TaxCategory.taxCategoryID>>>,
					Where<IN.InventoryItem.inventoryID, Equal<Required<EPSetup.nonTaxableTipItem>>,
					And<TX.TaxCategory.active, Equal<True>>>>.Select(this, e.NewValue);
				if (category != null && category.TaxCategoryID != null && category.TaxCatFlag != null)
				{
					PXSelectBase<TX.TaxRev> cmd;
					if (category.TaxCatFlag == true)
					{
						cmd = new PXSelectJoin<TX.TaxRev,
						LeftJoin<TX.TaxCategoryDet,
							On<TX.TaxRev.taxID, Equal<TX.TaxCategoryDet.taxID>,
							And<TX.TaxCategoryDet.taxCategoryID, Equal<Required<TX.TaxCategory.taxCategoryID>>>>>,
						Where<TX.TaxRev.taxRate, NotEqual<decimal0>,
						And<TX.TaxRev.taxRate, Greater<decimal0>,
						And<TX.TaxCategoryDet.taxCategoryID, IsNull>>>>(this);
					}
					else
					{
						cmd = new PXSelectJoin<TX.TaxRev,
						InnerJoin<TX.TaxCategoryDet,
							On<TX.TaxRev.taxID, Equal<TX.TaxCategoryDet.taxID>,
							And<TX.TaxCategoryDet.taxCategoryID, Equal<Required<TX.TaxCategory.taxCategoryID>>>>>,
						Where<TX.TaxRev.taxRate, NotEqual<decimal0>,
						And<TX.TaxRev.taxRate, Greater<decimal0>>>>(this);
					}
					if (cmd.Select(category.TaxCategoryID).Count > 0)
					{
						string inventoryCD = (string)PXSelectorAttribute.GetField(sender, e.Row, nameof(EPSetup.NonTaxableTipItem), e.NewValue, typeof(IN.InventoryItem.inventoryCD).Name);
						e.NewValue = inventoryCD;
						throw new PXSetPropertyException<EPSetup.nonTaxableTipItem>(Messages.TipItemAssociatedWithTaxes, PXErrorLevel.Error);
					}
					else
					{
						sender.RaiseExceptionHandling<EPSetup.nonTaxableTipItem>(e.Row, e.NewValue, null);
					}
				}
				else
				{
					sender.RaiseExceptionHandling<EPSetup.nonTaxableTipItem>(e.Row, e.NewValue, null);
				}
			}
		}

		protected virtual void EPWeekFilter_Year_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPWeekFilter row = (EPWeekFilter)e.Row;
			if (row == null)
				return;
			DateTime startDate;
			int number;
			int year;
			GetNextUsingWeek(out startDate, out number, out year);
			e.NewValue = year;
		}
		protected virtual void EPCustomWeek_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			EPCustomWeek row = (EPCustomWeek)e.Row;
			if (!isGenerate)
			{
				if (row != null)
				{
					if (row.StartDate != null && row.EndDate != null && row.StartDate.Value.Year != row.Year.Value && row.EndDate.Value.Year != row.Year.Value)
						throw new PXException(Messages.EndOfYear);
				}
				EPCustomWeek lastWeek = PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.year, Desc<EPCustomWeek.number>>>>.SelectSingleBound(this, new object[] { });
				if (lastWeek != null && lastWeek.EndDate == null)
				{
					throw new PXException(Messages.IncrorrectPrevWeek);
				}
			}
		}

		protected virtual void EPCustomWeek_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPCustomWeek row = (EPCustomWeek)e.Row;
			if (row != null)
			{
				EPCustomWeek lastWeek = (EPCustomWeek)PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.weekID>>>.Select(this);
				if (lastWeek != null)
				{
					bool readOnly = lastWeek.WeekID > row.WeekID;
					PXUIFieldAttribute.SetReadOnly(cache, row, true);
					PXUIFieldAttribute.SetReadOnly<EPCustomWeek.endDate>(cache, row, readOnly);
					PXUIFieldAttribute.SetReadOnly<EPCustomWeek.isFullWeek>(cache, row, readOnly);
					PXUIFieldAttribute.SetReadOnly<EPCustomWeek.startDate>(cache, row, row.Year != null && row.StartDate != null && (row.StartDate.Value.AddDays(-6d).Year <= row.Year.Value && row.Year.Value <= row.StartDate.Value.AddDays(6d).Year));
				}
			}
		}

		protected virtual void EPCustomWeek_StartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPCustomWeek row = (EPCustomWeek)e.Row;
			if (row == null)
				return;
			// EPCustomWeek lastWeek = PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.weekID>>>.Select(this); //TODO: why dont worked?
			EPCustomWeek lastWeek = PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.year, Desc<EPCustomWeek.number>>>>.SelectSingleBound(this, new object[] { });

			if (lastWeek != null && lastWeek.EndDate != null)
			{
				e.NewValue = GetBeginOfDate(lastWeek.EndDate.Value.AddDays(1d));
			}
		}

		protected virtual void EPCustomWeek_EndDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPCustomWeek row = (EPCustomWeek)e.Row;
			if (row == null || row.StartDate == null)
				return;
			e.NewValue = GetEndOfDate(row.StartDate.Value.AddDays(6d));
		}

		protected virtual void EPCustomWeek_EndDate_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if(e.NewValue != null)
				e.NewValue = GetEndOfDate((DateTime)e.NewValue);
		}

		protected virtual void EPCustomWeek_StartDate_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null)
				e.NewValue = GetBeginOfDate((DateTime)e.NewValue);
		}

		protected virtual void EPCustomWeek_Number_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPCustomWeek row = (EPCustomWeek)e.Row;
			if (row == null)
				return;
			EPCustomWeek lastWeek = (EPCustomWeek)PXSelect<EPCustomWeek, Where<EPCustomWeek.year, Equal<Required<EPCustomWeek.year>>>, OrderBy<Desc<EPCustomWeek.number>>>.SelectSingleBound(this, null, row.Year);
			if (lastWeek != null)
				e.NewValue = lastWeek.Number.Value + 1;
			else
				e.NewValue = 1;
		}

		protected virtual void EPCustomWeek_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			EPCustomWeek row = (EPCustomWeek)e.Row;
			if(row == null) return;
			if (cache.GetStatus(row) != PXEntryStatus.Inserted && cache.GetStatus(row) != PXEntryStatus.InsertedDeleted)
			{
				if ((EPTimeCard)PXSelect<EPTimeCard>.Search<EPTimeCard.weekId>(this, row.WeekID) != null || (PMTimeActivity)PXSelect<PMTimeActivity>.Search<PMTimeActivity.weekID>(this, row.WeekID) != null)
					throw new PXException(Messages.WeekInUse);
			}

			EPCustomWeek lastWeek = (EPCustomWeek)PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.weekID>>>.Select(this);
			if (lastWeek != null && lastWeek.WeekID > row.WeekID)
			{
				throw new PXSetPropertyException(Messages.WeekNotLast, PXErrorLevel.RowWarning);
			}


		}

		protected virtual void EPCustomWeek_IsFullWeek_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPCustomWeek row = (EPCustomWeek)e.Row;
			if (row == null)
				return;
			if (row.EndDate != null && row.StartDate != null)
				e.NewValue = row.EndDate.Value.Subtract(row.StartDate.Value).TotalDays > 6d;
			else
				e.NewValue = false;
		}

		protected virtual void EPCustomWeek_EndDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPCustomWeek row = (EPCustomWeek)e.Row;
			if (row == null || e.NewValue == null || row.StartDate == null)
				return;
			if (((DateTime) e.NewValue).Subtract(row.StartDate.Value).TotalDays > 7d)
			{
				sender.RaiseExceptionHandling<EPCustomWeek.endDate>(e.Row, null, new PXException(Messages.StartDateGreaterThanEndDate, PXUIFieldAttribute.GetDisplayName<EPCustomWeek.endDate>(sender), row.StartDate.Value.AddDays(6d)));
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void EPCustomWeek_StartDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPCustomWeek row = (EPCustomWeek)e.Row;
			if (row == null)
				return;
			DateTime StartDate = (DateTime)e.NewValue;
			if (!(StartDate.AddDays(-6d).Year <= row.Year.Value && row.Year.Value <= StartDate.AddDays(6d).Year))
				sender.RaiseExceptionHandling<EPCustomWeek.startDate>(e.Row, null, new PXException(Messages.StartDateWrongYear, row.Year));
		}

		protected virtual void EPGenerateWeeksDialog_TillDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			EPGenerateWeeksDialog row = (EPGenerateWeeksDialog) e.Row;
			if (e.NewValue == null || row == null)
				return;
			if ((DateTime)e.NewValue < row.FromDate)
			{
				e.Cancel = true;
				throw new PXSetPropertyException(ErrorMessages.EndDateLessThanStartDate, PXUIFieldAttribute.GetDisplayName<EPGenerateWeeksDialog.tillDate>(cache), PXUIFieldAttribute.GetDisplayName<EPGenerateWeeksDialog.fromDate>(cache), row.FromDate);
			}
			DateTime? lastUsingDate = GetLasttUsingWeek();
			if ( lastUsingDate != null && (DateTime)e.NewValue < lastUsingDate)
			{
				e.Cancel = true;
				throw new PXSetPropertyException(Messages.ExistsActivitiesLessThanDate, PXUIFieldAttribute.GetDisplayName<EPGenerateWeeksDialog.tillDate>(cache), lastUsingDate);
			}
		}

		protected virtual void EPGenerateWeeksDialog_FromDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			EPGenerateWeeksDialog row = (EPGenerateWeeksDialog)e.Row;
			if (e.NewValue == null || row == null)
				return;
			if ((DateTime)e.NewValue > row.TillDate)
			{
				e.Cancel = true;
				throw new PXSetPropertyException(ErrorMessages.StartDateGreaterThanEndDate, PXUIFieldAttribute.GetDisplayName<EPGenerateWeeksDialog.fromDate>(cache), PXUIFieldAttribute.GetDisplayName<EPGenerateWeeksDialog.tillDate>(cache), row.FromDate);
			}
			DateTime? firstUsingDate = GetFirstUsingWeek();
			EPCustomWeek lastWeek = (EPCustomWeek)PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.weekID>>>.SelectSingleBound(this, null);

			if (firstUsingDate != null && (DateTime)e.NewValue > firstUsingDate && lastWeek == null)
			{
				e.Cancel = true;
				throw new PXSetPropertyException(Messages.ExistsActivitiesGreateThanDate, PXUIFieldAttribute.GetDisplayName<EPGenerateWeeksDialog.fromDate>(cache), firstUsingDate);
			}
		}

		protected virtual void EPGenerateWeeksDialog_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPGenerateWeeksDialog row = (EPGenerateWeeksDialog)e.Row;
			if (row == null)
				return;

			if (row.CutOffDayOne == EPGenerateWeeksDialog.CutOffDayListAttribute.FixedDayOfMonth)
			{
				PXUIFieldAttribute.SetEnabled<EPGenerateWeeksDialog.dayOne>(cache, row, true);
				PXUIFieldAttribute.SetRequired<EPGenerateWeeksDialog.dayOne>(cache, true);
				PXUIFieldAttribute.SetEnabled<EPGenerateWeeksDialog.cutOffDayTwo>(cache, row, true);
				PXDefaultAttribute.SetPersistingCheck<EPGenerateWeeksDialog.dayOne>(cache, row, PXPersistingCheck.NullOrBlank);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<EPGenerateWeeksDialog.cutOffDayTwo>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<EPGenerateWeeksDialog.dayOne>(cache, row, false);
				row.DayOne = null;
				row.DayTwo = null;
				row.CutOffDayTwo = EPGenerateWeeksDialog.CutOffDayListAttribute.None;
			}
			if (row.CutOffDayTwo == EPGenerateWeeksDialog.CutOffDayListAttribute.FixedDayOfMonth)
			{
				PXUIFieldAttribute.SetEnabled<EPGenerateWeeksDialog.dayTwo>(cache, row, true);
				PXUIFieldAttribute.SetRequired<EPGenerateWeeksDialog.dayTwo>(cache, true);
				PXDefaultAttribute.SetPersistingCheck<EPGenerateWeeksDialog.dayTwo>(cache, row, PXPersistingCheck.NullOrBlank);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<EPGenerateWeeksDialog.dayTwo>(cache, row, false);
				row.DayTwo = null;
			}

			EPCustomWeek lastWeek = (EPCustomWeek)PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.number>>>.Select(this);
			if(lastWeek != null)
				PXUIFieldAttribute.SetEnabled<EPGenerateWeeksDialog.fromDate>(cache, row, false);
		}

		protected virtual void EPWeekFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPWeekFilter row = (EPWeekFilter)e.Row;
			if (row == null)
				return;

			DateTime? lastUsing = GetLasttUsingWeek();
			if (lastUsing != null && lastUsing.Value.Year > row.Year)
			{
				CustomWeek.Cache.AllowInsert = false;
				return;
			}

			EPCustomWeek lastWeek = (EPCustomWeek)PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.weekID>>>.SelectSingleBound(this, null);
			if (lastWeek != null && lastWeek.EndDate != null && (lastWeek.EndDate.Value.AddDays(1)).Year != row.Year)
				CustomWeek.Cache.AllowInsert = false;
			else
				CustomWeek.Cache.AllowInsert = true;
		}

		#endregion

		#region Private Methods

		private DateTime? GetFirstUsingWeek()
		{
			DateTime? startDate = GetFirstActivityDate();

			EPTimeCard firstTimeCard =
				  (EPTimeCard)PXSelectOrderBy<EPTimeCard, OrderBy<Asc<EPTimeCard.weekId>>>.SelectSingleBound(this, null);

			if (firstTimeCard != null)
			{
				DateTime timecardStartDate = PXWeekSelectorAttribute.GetWeekStartDate(firstTimeCard.WeekID.Value);
				if (startDate == null)
				{
					startDate = timecardStartDate;
				}
				else
				{
					if (timecardStartDate < startDate.Value)
					{
						startDate = timecardStartDate;
					}
				}
			}

			return startDate;
		}

		private DateTime? GetFirstActivityDate()
		{
			MasterFinYear minYear = PXSelect<MasterFinYear, Where<True, Equal<True>>, OrderBy<Desc<MasterFinYear.year>>>.Select(this);

			int minWeek;
			if (minYear != null && int.TryParse(minYear.Year + "01", out minWeek))
			{
				CRPMTimeActivity firstActivity = PXSelect<CRPMTimeActivity,
					Where<CRPMTimeActivity.weekID, GreaterEqual<Required<CRPMTimeActivity.weekID>>,
						 And<CRPMTimeActivity.trackTime, Equal<True>,
						 And<CRPMTimeActivity.classID, NotEqual<CRActivityClass.emailRouting>,
						 And<CRPMTimeActivity.classID, NotEqual<CRActivityClass.task>,
						And<CRPMTimeActivity.classID, NotEqual<CRActivityClass.events>>>>>>,
					 OrderBy<
						 Asc<CRPMTimeActivity.weekID>>>.Select(this, minWeek);
				if (firstActivity != null)
					return firstActivity.StartDate.Value;
			}

			return null;
		}

		private DateTime? GetLasttUsingWeek()
		{
			DateTime dateFromActivity = new DateTime(1900, 1, 1);
			DateTime startDate = new DateTime(1900, 1, 1);
			EPTimeCard lastTimeCard = (EPTimeCard)PXSelectOrderBy<EPTimeCard, OrderBy<Desc<EPTimeCard.weekId>>>.SelectSingleBound(this, null);
			CRPMTimeActivity lastActivity = PXSelect<CRPMTimeActivity, 
				Where<CRPMTimeActivity.weekID, IsNotNull,
					And<CRPMTimeActivity.trackTime, Equal<True>,
					And<CRPMTimeActivity.classID, NotEqual<CRActivityClass.emailRouting>,
					And<CRPMTimeActivity.classID, NotEqual<CRActivityClass.task>,
					And<CRPMTimeActivity.classID, NotEqual<CRActivityClass.events>>>>>>, 
				OrderBy<
					Desc<CRPMTimeActivity.weekID>>>.SelectSingleBound(this, null);
			if (lastTimeCard != null)
                startDate = PXWeekSelectorAttribute.GetWeekStartDate(lastTimeCard.WeekID.Value);
			if (lastActivity != null)
				dateFromActivity = lastActivity.StartDate.Value;

			startDate = startDate >= dateFromActivity ? startDate : dateFromActivity;

			if (startDate == new DateTime(1900, 1, 1))
				return null;
			else
				return startDate;
		}

		private void GetNextUsingWeek(out DateTime startDate, out int number, out int year)
		{
			EPCustomWeek lastWeek = (EPCustomWeek)PXSelectOrderBy<EPCustomWeek, OrderBy<Desc<EPCustomWeek.weekID>>>.SelectSingleBound(this, null);

			if (lastWeek != null)
			{
				startDate = lastWeek.EndDate.Value.AddDays(1d);
				if (lastWeek.Number > 1 && lastWeek.StartDate.Value.Year < startDate.Year)
					number = 1;
				else
					number = lastWeek.Number.Value + 1;
				year = startDate.Year;
			}
			else
			{
				DateTime? First = GetFirstUsingWeek();
				if (First != null)
					startDate = (DateTime)First;
				else if (WeekFilter.Current != null)
					startDate = new DateTime(WeekFilter.Current.Year.Value, 1, 1);
				else
					startDate = new DateTime(Accessinfo.BusinessDate.Value.Year, 1, 1);

				number = PXWeekSelectorAttribute.GetWeekID(startDate) % 100;
				year = PXWeekSelectorAttribute.GetWeekID(startDate) / 100;
			}
		}

		private DateTime GetEndOfDate(DateTime date)
		{
			return new DateTime(date.Year, date.Month, date.Day, 23, 59, 00);
		}

		private DateTime GetBeginOfDate(DateTime date)
		{
			return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
		}


		#endregion

		#region DAC override
		[PXDBInt]
		[PXUIField(Visible = false)]
		[PXFormula(typeof(Add<Mult<EPCustomWeek.year, decimal100>, EPCustomWeek.number>))]
		protected virtual void EPCustomWeek_WeekID_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Year", Visible = false)]
		[PXDefault(typeof(EPWeekFilter.year))]
		protected virtual void EPCustomWeek_Year_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Number")]
		[PXDefault]
		protected virtual void EPCustomWeek_Number_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region DAC
		[Serializable]
		[PXHidden]
		public partial class EPWeekFilter : IBqlTable
		{
			#region Year

			public abstract class year : PX.Data.BQL.BqlInt.Field<year> { }

			private Int32? _year;
			[PXDBInt]
			[PXUIField(DisplayName = "Year", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search3<MasterFinYear.year, OrderBy<Desc<MasterFinYear.year>>>))]
			public virtual Int32? Year
			{
				get
				{
					return _year;
				}
				set
				{
					_year = value;
				}
			}

			#endregion

		}


		[Serializable]
		[PXHidden]
		public partial class EPGenerateWeeksDialog : IBqlTable
		{
			#region StartDate
			public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }

			[PXDBDate]
			[PXDefault]
			[PXUIField(DisplayName = "From Date")]
			public virtual DateTime? FromDate { get; set; }
			#endregion
			#region TillDate
			public abstract class tillDate : PX.Data.BQL.BqlDateTime.Field<tillDate> { }

			[PXDBDate]
			[PXDefault()]
			[PXUIField(DisplayName = "Until Date")]
			public virtual DateTime? TillDate { get; set; }
			#endregion
			#region CutOffDayOne
			public abstract class cutOffDayOne : PX.Data.BQL.BqlString.Field<cutOffDayOne> { }

			[PXDBString()]
			[CutOffDayList]
			[PXDefault(CutOffDayListAttribute.None)]
			[PXUIField(DisplayName = "Cut Off Day One")]
			public virtual string CutOffDayOne { get; set; }
			#endregion
			#region CutOffDayTwo
			public abstract class cutOffDayTwo : PX.Data.BQL.BqlString.Field<cutOffDayTwo> { }

			[PXDBString(3, IsFixed = true)]
			[CutOffDayList]
			[PXDefault(CutOffDayListAttribute.None)]
			[PXUIField(DisplayName = "Cut Off Day Two")]
			public virtual string CutOffDayTwo { get; set; }
			#endregion
			#region DayOne
			public abstract class dayOne : PX.Data.BQL.BqlInt.Field<dayOne> { }

			[PXDBInt(MinValue = 1, MaxValue = 31)]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Day One")]
			public virtual Int32? DayOne { get; set; }
			#endregion
			#region DayTwo
			public abstract class dayTwo : PX.Data.BQL.BqlInt.Field<dayTwo> { }

			[PXDBInt(MinValue = 1, MaxValue = 31)]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Day Two")]
			public virtual Int32? DayTwo { get; set; }
			#endregion

			public class CutOffDayListAttribute : PXStringListAttribute
			{
				public CutOffDayListAttribute()
					: base(
						new string[] { None, FixedDayOfMonth, EndOfMonth }
						, new string[] { Messages.None, Messages.FixedDayOfMonth, Messages.EndOfMonth }
						)
				{
				}

				public const string None = "NOT";
				public const string FixedDayOfMonth = "FDM";
				public const string EndOfMonth = "EOM";

			}
		}
		#endregion
	}

}
