using System;
using System.Linq;
using System.Collections.Generic;

using PX.Data;

using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common;

namespace PX.Objects.FA
{
	public class GenerationPeriods : PXGraph<GenerationPeriods>
	{
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFABookPeriodRepository FABookPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public PXCancel<BoundaryYears> Cancel;
		public PXFilter<BoundaryYears> Years;

		[PXFilterable]
		public PXFilteredProcessingOrderBy<
			FAOrganizationBook, BoundaryYears, 
			OrderBy<
				Desc<FAOrganizationBook.updateGL, 
				Asc<FAOrganizationBook.bookID>>>> Books;

		public PXSelect<FABookYear> bookyears;
		public PXSelect<FABookPeriod> bookperiods;

		public PXSetup<FinYearSetup> FinYearSetup;

		private FABookPeriodIndex _FAMasterBookPeriodIndex= new FABookPeriodIndex();

		public GenerationPeriods()
		{
			FinYearSetup setup = FinYearSetup.Current;
		}

		protected virtual void BoundaryYears_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				BoundaryYears filter = (BoundaryYears)e.Row;
				Books.SetProcessDelegate(list => GeneratePeriods(filter, list));
			}
		}

		protected virtual void BoundaryYears_ToYear_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			int? maxyear = null;
			foreach (FAOrganizationBook book in Books.Select())
			{
				int? currentLastYear = book.LastCalendarYear != null ? int.Parse(book.LastCalendarYear) : (int?)null;
				if (currentLastYear != null && book.LastCalendarYear != null && (maxyear == null || maxyear < currentLastYear))
				{
					maxyear = (int)currentLastYear;
				}
			}

			if (maxyear == null)
			{
				maxyear = int.Parse(PXSelect<FinYearSetup>.Select(this).RowCast<FinYearSetup>().First().FirstFinYear) - 1;
			}

			e.NewValue = ((int)maxyear + 1).ToString(FiscalPeriodSetupCreator.CS_YEAR_FORMAT);
		}

		protected virtual void BoundaryYears_FromYear_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			int? minyear = null;
			foreach (FAOrganizationBook book in Books.Select())
			{
				int? currentLastYear = book.LastCalendarYear != null ? int.Parse(book.LastCalendarYear) : (int?)null;
				if (currentLastYear != null && book.LastCalendarYear != null && (minyear == null || minyear > currentLastYear))
				{
					minyear = (int)currentLastYear;
				}
			}

			if (minyear == null)
			{
				minyear = int.Parse(PXSelect<FinYearSetup>.Select(this).RowCast<FinYearSetup>().First().FirstFinYear) - 1;
		}

			e.NewValue = ((int)minyear + 1).ToString(FiscalPeriodSetupCreator.CS_YEAR_FORMAT);
		}

		public static void GeneratePeriods(BoundaryYears filter, List<FAOrganizationBook> books)
		{
			GenerationPeriods graph = CreateInstance<GenerationPeriods>();
			graph.GeneratePeriodsProc(filter, books);
		}

		protected virtual void FAOrganizationBook_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
			FAOrganizationBook book = (FAOrganizationBook) e.Row;
            if (book == null) return;

			PXSetPropertyException disableRowException = null;
			FABookYearSetup setup = PXSelect<FABookYearSetup, Where<FABookYearSetup.bookID, Equal<Current<FAOrganizationBook.bookID>>>>.SelectSingleBound(this, new object[] { book });
            if(setup == null && book.UpdateGL != true)
            {
				disableRowException =  new PXSetPropertyException(Messages.CalendarSetupNotFound, PXErrorLevel.RowWarning, book.BookCode);
            }

			OrganizationFinYear nearestFinYear = FinPeriodRepository.FindNearestOrganizationFinYear(book.OrganizationID, "1900");
			if (disableRowException == null &&
				book.OrganizationID != FinPeriod.organizationID.MasterValue &&
				book.UpdateGL == true && 
				nearestFinYear == null)
			{
				disableRowException = new PXSetPropertyException(GL.Messages.OrganizationCalendarDoesNotExist, PXErrorLevel.RowWarning, PXAccess.GetOrganizationCD(book.OrganizationID));
			}

			if(disableRowException != null)
			{
				PXUIFieldAttribute.SetEnabled<FAOrganizationBook.selected>(sender, book, false);
				sender.RaiseExceptionHandling<FAOrganizationBook.selected>(book, null, disableRowException);
			}
		}

		protected virtual void _(Events.RowSelected<FABookYear> e)
		{
			if (e.Row == null) return;
			PXDefaultAttribute.SetPersistingCheck<FABookYear.startMasterFinPeriodID>(e.Cache, e.Row, e.Row.OrganizationID == 0 ? PXPersistingCheck.Nothing : PXPersistingCheck.Null);
		}

		protected virtual void _(Events.RowSelecting<FABookPeriod> e)
		{
			if (e.Row == null) return;
			FABookPeriod period = e.Row;

			if (period.OrganizationID == FinPeriod.organizationID.MasterValue) 
			{
				_FAMasterBookPeriodIndex.Add(period);
			}
		}

		protected virtual void _(Events.RowSelected<FABookPeriod> e)
		{
			if (e.Row == null) return;
			PXDefaultAttribute.SetPersistingCheck<FABookPeriod.masterFinPeriodID>(e.Cache, e.Row, e.Row.OrganizationID == 0 ? PXPersistingCheck.Nothing : PXPersistingCheck.Null);
		}

		protected virtual void _(Events.RowInserted<FABookPeriod> e)
		{
			if (e.Row == null) return;
			FABookPeriod period = e.Row;

			if (period.OrganizationID == FinPeriod.organizationID.MasterValue)
			{
				_FAMasterBookPeriodIndex.Add(period);
			}
		}

		protected virtual void _(Events.RowDeleted<FABookPeriod> e)
		{
			if (e.Row == null) return;
			FABookPeriod period = e.Row;

			if (period.OrganizationID == FinPeriod.organizationID.MasterValue)
			{
				_FAMasterBookPeriodIndex.Remove(period);
			}
		}

		protected virtual void AddFieldDefaultingHandler<FieldID>(int? id, Dictionary<(Type, int), PXFieldDefaulting> delegates)
			where FieldID : IBqlField
		{
			PXFieldDefaulting @delegate = null;
			if(!delegates.TryGetValue((typeof(FieldID), id ?? 0), out @delegate))
			{
				@delegate = (cache, e) =>
				{
					e.NewValue = id;
				};
				delegates.Add((typeof(FieldID), id ?? 0), @delegate);
			}
			FieldDefaulting.AddHandler<FieldID>(@delegate);
		}

		protected virtual void RemoveFieldDefaultingHandler<FieldID>(int? id, Dictionary<(Type, int), PXFieldDefaulting> delegates)
			where FieldID : IBqlField
		{
			FieldDefaulting.RemoveHandler<FieldID>(delegates[(typeof(FieldID), id ?? 0)]);
		}

		public virtual void AddFieldDefaultingHandlers(FABook book, Dictionary<(Type, int), PXFieldDefaulting> delegates)
		{
			AddFieldDefaultingHandler<FABookYear.bookID>(book.BookID, delegates);
			AddFieldDefaultingHandler<FABookPeriod.bookID>(book.BookID, delegates);
		}

		public virtual void RemoveFieldDefaultingHandlers(FABook book, Dictionary<(Type, int), PXFieldDefaulting> delegates)
		{
			RemoveFieldDefaultingHandler<FABookYear.bookID>(book.BookID, delegates);
			RemoveFieldDefaultingHandler<FABookPeriod.bookID>(book.BookID, delegates);
		}

		protected virtual void GenerateFAMasterCalendarInYearRange(FABook book, string fromYearNumber, string toYearNumber)
		{
			if(string.Compare(fromYearNumber, toYearNumber) > 0)
			{
				Utilities.Swap(ref fromYearNumber, ref toYearNumber);
			}
			GenerateFAMasterCalendarUpToYear(book, fromYearNumber);
			GenerateFAMasterCalendarToYear(book, toYearNumber);
		}

		protected virtual void GenerateFAMasterCalendarUpToYear(FABook book, string fromYearNumber)
		{
			//Shift back start calrndar year in calendar setup
			IYearSetupMaintenanceGraph setupGraph = book.UpdateGL == true
				? CreateInstance<FiscalYearSetupMaint>() as IYearSetupMaintenanceGraph
				: CreateInstance<FABookYearSetupMaint>() as IYearSetupMaintenanceGraph;

			setupGraph.SetCurrentYearSetup(new object[] { book.BookID });
			setupGraph.ShiftBackFirstYearTo($"{fromYearNumber:0000}");

			FABookYear firstBookYear = FABookPeriodRepository.FindFirstFABookYear(
				book.BookID,
				FinPeriod.organizationID.MasterValue,
				clearQueryCache: false,
				mergeCache: true);

			FABookYear newYear = new FABookYear { Year = firstBookYear?.Year };
			while (newYear != null && (newYear.Year == null || string.Compare(newYear.Year, fromYearNumber) > 0))
			{
				newYear = CreatePreviousMasterFABookYear(book, firstBookYear);
				firstBookYear = newYear;
			}
		}

		protected virtual void GenerateFAMasterCalendarToYear(FABook book, string toYearNumber)
		{
			FABookYear lastBookYear = FABookPeriodRepository.FindLastFABookYear(
				book.BookID,
				FinPeriod.organizationID.MasterValue,
				clearQueryCache: false,
				mergeCache: true);
			FABookYear newYear = new FABookYear { Year = lastBookYear?.Year };

			while (newYear != null && (newYear.Year == null || string.Compare(newYear.Year, toYearNumber) < 0))
			{
				newYear = CreateNextMasterFABookYear(book, lastBookYear);
				lastBookYear = newYear;
			}
		}

		protected virtual FABookYear CreateNextMasterFABookYear(FABook book, FABookYear lastMasterFABookYear = null)
		{
			IYearSetup yearSetup = FABookPeriodRepository.FindFABookYearSetup(book, clearQueryCache: true);
			IEnumerable<IPeriodSetup> periodsSetup = FABookPeriodRepository.FindFABookPeriodSetup(book, clearQueryCache: true);

			if (lastMasterFABookYear == null)
			{
				lastMasterFABookYear = FABookPeriodRepository.FindLastFABookYear(
					book.BookID,
					FinPeriod.organizationID.MasterValue,
					mergeCache: true);
			}
			return FiscalYearCreator<FABookYear, FABookPeriod>.CreateNextYear(this, yearSetup, periodsSetup, lastMasterFABookYear);
		}

		protected virtual FABookYear CreatePreviousMasterFABookYear(FABook book, FABookYear firstMasterFABookYear = null)
		{
			IYearSetup yearSetup = FABookPeriodRepository.FindFABookYearSetup(book, clearQueryCache: true);
			IEnumerable<IPeriodSetup> periodsSetup = FABookPeriodRepository.FindFABookPeriodSetup(book, clearQueryCache: true);

			if (firstMasterFABookYear == null)
			{
				firstMasterFABookYear = FABookPeriodRepository.FindFirstFABookYear(
					book.BookID,
					FinPeriod.organizationID.MasterValue,
					mergeCache: true);
			}
			return FiscalYearCreator<FABookYear, FABookPeriod>.CreatePrevYear(this, yearSetup, periodsSetup, firstMasterFABookYear);
		}

		protected virtual void GenerateFAOrganizationCalendarInYearRange(FAOrganizationBook book, string fromYearNumber, string toYearNumber)
		{
			InitializeFAOrganizationCalendar(book, fromYearNumber);
			GenerateFAOrganizationCalendarUpToYear(book, fromYearNumber);
			GenerateFAOrganizationCalendarToYear(book, toYearNumber);
		}

		protected virtual void InitializeFAOrganizationCalendar(FAOrganizationBook book, string yearNumber)
		{
			if(FABookPeriodRepository.FindFirstFABookYear(book.BookID, book.OrganizationID, clearQueryCache: true) != null)
			{
				return;
			}

			OrganizationFinYear nearestFinYear = FinPeriodRepository.FindNearestOrganizationFinYear(book.OrganizationID, yearNumber);
			string organizationFABookYearNumber = nearestFinYear.Year;
			string startMasterFABookPeriodYearNumber = GL.FinPeriods.FinPeriodUtils.FiscalYear(nearestFinYear.StartMasterFinPeriodID);


			GenerateFAMasterCalendarInYearRange(book, organizationFABookYearNumber, startMasterFABookPeriodYearNumber);

			FABookYear startMasterYear = FABookPeriodRepository.FindMasterFABookYearByID(book, organizationFABookYearNumber, mergeCache: true);
			FABookPeriod startMasterPeriod = FABookPeriodRepository.FindMasterFABookPeriodByID(book, nearestFinYear.StartMasterFinPeriodID, mergeCache: true);

			GenerateSingleOrganizationFABookYear(book, startMasterYear, startMasterPeriod);
		}

		// TODO: Share it
		// This function copy-pasted from OrganizationFinPeriodMaint.CopyOrganizationFinPeriodFromMaster
		protected virtual FABookPeriod CopyOrganizationFABookPeriodFromMaster(FAOrganizationBook book, FABookPeriod masterFABookPeriod, string yearNumber = null, string periodNumber = null)
		{
			string organizationFABookPeriodID = FinPeriodUtils.ComposeFinPeriodID(yearNumber, periodNumber) ?? masterFABookPeriod.FinPeriodID;
			return new FABookPeriod
			{
				BookID = masterFABookPeriod.BookID,
				OrganizationID = book.OrganizationID,
				FinPeriodID = organizationFABookPeriodID,
				MasterFinPeriodID = masterFABookPeriod.FinPeriodID,
				FinYear = yearNumber ?? masterFABookPeriod.FinYear,
				PeriodNbr = periodNumber ?? masterFABookPeriod.PeriodNbr,
				Custom = masterFABookPeriod.Custom,
				DateLocked = masterFABookPeriod.DateLocked,
				StartDate = masterFABookPeriod.StartDate,
				EndDate = masterFABookPeriod.EndDate,
				Descr = masterFABookPeriod.Descr,
			};
		}

		// TODO: Remove it after adjustment periods elimination from FA
		// Adjustment periods do not make sense in FA
		// This function copy-pasted from OrganizationFinPeriodMaint.GenerateAdjustmentOrganizationFinPeriod
		protected virtual FABookPeriod GenerateAdjustmentOrganizationFABookPeriod(FAOrganizationBook book, FABookPeriod prevOrganizationFABookPeriod)
		{
			(string masterYearNumber, string masterPeriodNumber) = FinPeriodUtils.ParseFinPeriodID(prevOrganizationFABookPeriod.FinPeriodID);
			FABookYear masterFABookYear = FABookPeriodRepository.FindMasterFABookYearByID(book, masterYearNumber, clearQueryCache: false, mergeCache: true);
			string adjustmentMasterFinPeriodID = $"{masterYearNumber:0000}{masterFABookYear.FinPeriods:00}";

			(string yearNumber, string periodNumber) = FinPeriodUtils.ParseFinPeriodID(prevOrganizationFABookPeriod.FinPeriodID);
			periodNumber = $"{int.Parse(periodNumber) + 1:00}";
			return new FABookPeriod
			{
				BookID = book.BookID,
				OrganizationID = book.OrganizationID,
				FinPeriodID = FinPeriodUtils.ComposeFinPeriodID(yearNumber, periodNumber),
				MasterFinPeriodID = adjustmentMasterFinPeriodID,
				FinYear = yearNumber,
				PeriodNbr = periodNumber,
				Custom = prevOrganizationFABookPeriod.Custom,
				DateLocked = prevOrganizationFABookPeriod.DateLocked,
				StartDate = prevOrganizationFABookPeriod.EndDate,
				EndDate = prevOrganizationFABookPeriod.EndDate,
				Descr = GL.Messages.AdjustmentPeriod,
			};
		}

		// TODO: Share it
		// This function copy-pasted from OrganizationFinPeriodMaint.GenerateSingleOrganizationFinYear
		protected virtual FABookYear GenerateSingleOrganizationFABookYear(FAOrganizationBook book, FABookYear startMasterYear, FABookPeriod startMasterPeriod)
		{
			if (startMasterYear == null)
			{
				throw new ArgumentNullException(nameof(startMasterYear));
			}
			if (startMasterPeriod == null)
			{
				throw new ArgumentNullException(nameof(startMasterPeriod));
			}

			FABookYear newOrganizationFABookYear = (FABookYear)this.Caches<FABookYear>().Insert(
				new FABookYear
				{
					BookID = book.BookID,
					OrganizationID = book.OrganizationID,
					Year = startMasterYear.Year,
					FinPeriods = startMasterYear.FinPeriods,
					StartMasterFinPeriodID = startMasterPeriod.FinPeriodID,
					StartDate = startMasterPeriod.StartDate,
				});

			short periodNumber = 1;
			FABookPeriod sourceMasterFABookPeriod = startMasterPeriod;
			int periodsCountForCopy = (int)newOrganizationFABookYear.FinPeriods;
			IYearSetup yearSetup = FABookPeriodRepository.FindFABookYearSetup(book, clearQueryCache: true);
			if (yearSetup.HasAdjustmentPeriod == true)
			{
				periodsCountForCopy--;
			}
			FABookPeriod newOrganizationFABookPeriod = null;
			while (periodNumber <= periodsCountForCopy)
			{
				newOrganizationFABookPeriod = (FABookPeriod)this.Caches<FABookPeriod>().Insert(
					CopyOrganizationFABookPeriodFromMaster(book, sourceMasterFABookPeriod, newOrganizationFABookYear.Year, $"{periodNumber:00}"));

				if (periodNumber < periodsCountForCopy) // no need to search for the next master period if last organization period is generated
				{
					string sourceMasterFinPeriodID = sourceMasterFABookPeriod.FinPeriodID;
					while ((sourceMasterFABookPeriod = _FAMasterBookPeriodIndex.FindNextNonAdjustmentMasterFABookPeriod(book.BookID, sourceMasterFinPeriodID)) == null)
					{
						CreateNextMasterFABookYear(book);
					}
				}

				periodNumber++;
			}
			newOrganizationFABookYear.EndDate = newOrganizationFABookPeriod.EndDate;
			if (yearSetup.HasAdjustmentPeriod == true)
			{
				this.Caches<FABookPeriod>().Insert(GenerateAdjustmentOrganizationFABookPeriod(book, newOrganizationFABookPeriod));
			}

			return newOrganizationFABookYear;
		}

		protected virtual void GenerateFAOrganizationCalendarUpToYear(FAOrganizationBook book, string fromYearNumber)
		{
			FABookYear firstOrganizationFABookYear = FABookPeriodRepository.FindFirstFABookYear(
				book.BookID,
				book.OrganizationID,
				clearQueryCache: false,
				mergeCache: true);

			FABookYear newYear = new FABookYear { Year = firstOrganizationFABookYear?.Year };
			while (newYear != null && string.Compare(newYear.Year, fromYearNumber) > 0)
			{
				newYear = CreatePreviousOrganizationFABookYear(book, firstOrganizationFABookYear);
				firstOrganizationFABookYear = newYear;
			}
		}

		protected virtual void GenerateFAOrganizationCalendarToYear(FAOrganizationBook book, string toYearNumber)
		{
			FABookYear lastOrganizationFABookYear = FABookPeriodRepository.FindLastFABookYear(
				book.BookID,
				book.OrganizationID,
				clearQueryCache: false,
				mergeCache: true);

			FABookYear newYear = new FABookYear { Year = lastOrganizationFABookYear?.Year };
			while (newYear != null && string.Compare(newYear.Year, toYearNumber) < 0)
			{
				newYear = CreateNextOrganizationFABookYear(book, lastOrganizationFABookYear);
				lastOrganizationFABookYear = newYear;
			}
		}

		// TODO: Share it
		// This function copy-pasted from OrganizationFinPeriodMaint.GenerateNextOrganizationFinYear
		protected virtual FABookYear CreateNextOrganizationFABookYear(FAOrganizationBook book, FABookYear lastOrganizationFABookYear)
		{
			string generatedYearNumber = $"{int.Parse(lastOrganizationFABookYear.Year) + 1:0000}";
			FABookYear masterFABookYear;
			while ((masterFABookYear = FABookPeriodRepository.FindMasterFABookYearByID(book, generatedYearNumber, clearQueryCache: false, mergeCache: true)) == null)
			{
				GenerateFAMasterCalendarInYearRange(
					book,
					FABookPeriodRepository.FindLastFABookYear(
						book.BookID,
						FinPeriod.organizationID.MasterValue,
						clearQueryCache: false,
						mergeCache: true).Year,
					generatedYearNumber);
			}

			short generatedFinPeriodsCount = (short)masterFABookYear.FinPeriods;
			if (FABookPeriodRepository.FindFABookYearSetup(book, clearQueryCache: true).HasAdjustmentPeriod == true)
				{
				generatedFinPeriodsCount--;
			}

			FABookPeriod lastNonAdjustmentOrgFABookPeriod = FABookPeriodRepository.FindLastNonAdjustmentOrganizationFABookPeriodOfYear(
				book, 
				lastOrganizationFABookYear.Year, 
				clearQueryCache: false,
				mergeCache: true);
			int generatedMasterYearNumber = int.Parse(lastNonAdjustmentOrgFABookPeriod.FinYear);

			List<FABookPeriod> masterFABookPeriodsPeriods;

			PXSelectBase<FABookPeriod> select = new PXSelect<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<FinPeriod.organizationID.masterValue>,
					And<FABookPeriod.finPeriodID, Greater<Required<FABookPeriod.finPeriodID>>,
					And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>>>>>,
				OrderBy<
					Asc<FABookPeriod.finPeriodID>>>(this);

			while ((masterFABookPeriodsPeriods = select
				.SelectWindowed(0, generatedFinPeriodsCount, book.BookID, lastNonAdjustmentOrgFABookPeriod.MasterFinPeriodID)
				.RowCast<FABookPeriod>()
				.ToList()).Count < generatedFinPeriodsCount)
					{
				generatedMasterYearNumber++;

				GenerateFAMasterCalendarInYearRange(
					book,
					FABookPeriodRepository.FindLastFABookYear(
						book.BookID,
						FinPeriod.organizationID.MasterValue,
						clearQueryCache: false,
						mergeCache: true).Year,
					generatedMasterYearNumber.ToString(FiscalPeriodSetupCreator.CS_YEAR_FORMAT));
					}

			FABookPeriod startMasterFABookPeriod = masterFABookPeriodsPeriods.First();
			return GenerateSingleOrganizationFABookYear(book, masterFABookYear, startMasterFABookPeriod);
		}

		// TODO: Share it
		// This function copy-pasted from OrganizationFinPeriodMaint.GeneratePreviousOrganizationFinYear
		protected virtual FABookYear CreatePreviousOrganizationFABookYear(FAOrganizationBook book, FABookYear firsOrganizationFABookyear)
		{
			string generatedYearNumber = $"{int.Parse(firsOrganizationFABookyear.Year) - 1:0000}";
			FABookYear masterFABookYear;
			while ((masterFABookYear = FABookPeriodRepository.FindMasterFABookYearByID(book, generatedYearNumber, clearQueryCache: false, mergeCache: true)) == null)
			{
				GenerateFAMasterCalendarInYearRange(
					book,
					generatedYearNumber,
					FABookPeriodRepository.FindFirstFABookYear(
						book.BookID,
						FinPeriod.organizationID.MasterValue,
						clearQueryCache: false,
						mergeCache: true).Year);
			}

			short generatedFinPeriodsCount = (short)masterFABookYear.FinPeriods;
			if (FABookPeriodRepository.FindFABookYearSetup(book, clearQueryCache: true).HasAdjustmentPeriod == true)
				{
				generatedFinPeriodsCount--;
			}

			int generatedMasterYearNumber = int.Parse(GL.FinPeriods.FinPeriodUtils.FiscalYear(firsOrganizationFABookyear.StartMasterFinPeriodID));

			List<FABookPeriod> masterFABookPeriods;

			PXSelectBase<FABookPeriod> select = new PXSelect<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<FinPeriod.organizationID.masterValue>,
					And<FABookPeriod.finPeriodID, Less<Required<FABookPeriod.finPeriodID>>,
					And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>>>>>,
				OrderBy<
					Desc<FABookPeriod.finPeriodID>>>(this);

			while ((masterFABookPeriods = select
				.SelectWindowed(0, generatedFinPeriodsCount, book.BookID, firsOrganizationFABookyear.StartMasterFinPeriodID)
				.RowCast<FABookPeriod>()
				.ToList()).Count < generatedFinPeriodsCount)
					{
				generatedMasterYearNumber--;
				GenerateFAMasterCalendarInYearRange(
					book,
					generatedMasterYearNumber.ToString(FiscalPeriodSetupCreator.CS_YEAR_FORMAT),
					FABookPeriodRepository.FindFirstFABookYear(
						book.BookID, 
						FinPeriod.organizationID.MasterValue, 
						clearQueryCache: false, 
						mergeCache: true).Year);
			}

			FABookPeriod startMasterFABookPeriod = masterFABookPeriods.Last();
			return GenerateSingleOrganizationFABookYear(book, masterFABookYear, startMasterFABookPeriod);
					}

		public virtual void GeneratePeriodsProc(BoundaryYears filter, List<FAOrganizationBook> books)
		{
			Dictionary<(Type, int), PXFieldDefaulting> delegates = new Dictionary<(Type, int), PXFieldDefaulting>();

			List<FAOrganizationBook> nonPostingBooks = books
				.Where(book => book.UpdateGL != true)
				.ToList();

			// Fill _FAMasterBookPeriodIndex by RowSelecting
			new Select<FABookPeriod, Where<FABookPeriod.organizationID, Equal<FinPeriod.organizationID.masterValue>>>()
				.CreateView(this, false, true)
				.SelectMulti();


			using (PXTransactionScope transaction = new PXTransactionScope())
			{
				// Generate calrndars for non-posting books
				Exception exception = null;
				foreach (FAOrganizationBook nonPostingBook in nonPostingBooks)
				{
					PXProcessing<FAOrganizationBook>.SetCurrentItem(nonPostingBook);
					try
					{
						AddFieldDefaultingHandlers(nonPostingBook, delegates);

						GenerateFAMasterCalendarInYearRange(nonPostingBook, filter.FromYear, filter.ToYear);

						RemoveFieldDefaultingHandlers(nonPostingBook, delegates);
					}
					catch (Exception exc)
					{
						exception = exc;
					}

					if (exception != null)
					{
						PXProcessing<FAOrganizationBook>.SetError(exception);
				}
				else
				{
						PXProcessing<FAOrganizationBook>.SetProcessed();
					}
				}

				bool IsMultipleCalendarsSupported = PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>();
				FAOrganizationBook masterPostingBook = books.Where(book => book.UpdateGL == true && book.OrganizationID == FinPeriod.organizationID.MasterValue).FirstOrDefault();
				//Collect the posting organization to calendar generation
				List <FAOrganizationBook> postingBooks;
				if(IsMultipleCalendarsSupported) // generate selected organization posting calendars
					{
					if (masterPostingBook != null)
					{
						throw new PXException(Messages.UnexpectedMasterCalendarOfPostingBook);
					}
					postingBooks = books
						.Where(book => book.UpdateGL == true && book.OrganizationID != 0)
						.ToList();
				}
				else // generate all organization posting calendars
				{
					postingBooks = new List<FAOrganizationBook>();
					foreach (PXResult<FABook, Organization> result in PXSelectReadonly2<
						FABook,
						CrossJoin<Organization>,
						Where<FABook.updateGL, Equal<True>>>
						.Select(this))
					{
						FABook book = result;
						Organization organization = result;
						FAOrganizationBook organizationBook = new FAOrganizationBook();
						PXCache<FABook>.RestoreCopy(organizationBook, book);

						organizationBook.RawOrganizationID = organization.OrganizationID;
						organizationBook.OrganizationID = organizationBook.RawOrganizationID ?? FinPeriod.organizationID.MasterValue;
						organizationBook.OrganizationCD = organization.OrganizationCD;

						postingBooks.Add(organizationBook);
					}
				}

				// Generate needed posting book calendars
				exception = null;
				foreach (FAOrganizationBook postingOrganizationBook in postingBooks)
				{
					PXProcessing<FAOrganizationBook>.SetCurrentItem(IsMultipleCalendarsSupported ? postingOrganizationBook : masterPostingBook);
					try
					{
						AddFieldDefaultingHandlers(postingOrganizationBook, delegates);

						GenerateFAOrganizationCalendarInYearRange(postingOrganizationBook, filter.FromYear, filter.ToYear);

						RemoveFieldDefaultingHandlers(postingOrganizationBook, delegates);
					}
					catch (Exception exc)
					{
						exception = exc;
					}

					if (IsMultipleCalendarsSupported)
					{
						if (exception != null)
						{
							PXProcessing<FAOrganizationBook>.SetError(exception);
						}
						else
						{
							PXProcessing<FAOrganizationBook>.SetProcessed();
						}
					}
				}

				if (!IsMultipleCalendarsSupported)
				{
					if (exception != null)
					{
						PXProcessing<FAOrganizationBook>.SetError(exception);
					}
					else
					{
						PXProcessing<FAOrganizationBook>.SetProcessed();
					}
				}

				Actions.PressSave();
				transaction.Complete();
		}
	}

		// Index for FABookPeriod by BookID, FinPeriodID
		private class FABookPeriodIndex 
		{
			private Dictionary<int?, SortedDictionary<string, FABookPeriod>> _periods = new Dictionary<int?, SortedDictionary<string, FABookPeriod>>();

			public void Add(FABookPeriod period) 
			{
				SortedDictionary<string, FABookPeriod> periods = null;
				if (!_periods.TryGetValue(period.BookID, out periods))
				{
					periods = new SortedDictionary<string, FABookPeriod>();
					_periods.Add(period.BookID, periods);
				}
				periods.Add(period.FinPeriodID, period);
			}

			public bool Remove(FABookPeriod period) 
			{
				SortedDictionary<string, FABookPeriod> periods = null;
				if (!_periods.TryGetValue(period.BookID, out periods))
				{
					periods = new SortedDictionary<string, FABookPeriod>();
					_periods.Add(period.BookID, periods);
				}
				periods.Remove(period.FinPeriodID);
				if (periods.Count == 0) 
				{
					return _periods.Remove(period.BookID);
				}
				return false;
			}

			public FABookPeriod FindNextNonAdjustmentMasterFABookPeriod(int? BookID, string prevFABookPeriodID) 
			{
				SortedDictionary<string, FABookPeriod> periods = null;
				if (!_periods.TryGetValue(BookID, out periods))
					return null;

				return periods.Where(i=> String.Compare(i.Key, prevFABookPeriodID) > 0 && i.Value.StartDate!=i.Value.EndDate).FirstOrDefault().Value;
			}
		}
	}

	public class FirstCalendarYear<BookIDValue, OrganizationIDValue> : BqlFormulaEvaluator<BookIDValue, OrganizationIDValue>, IBqlOperand
		where BookIDValue : IBqlOperand
		where OrganizationIDValue : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			int? bookID = (int?)parameters[typeof(BookIDValue)];
			int? organizationID = (int?)parameters[typeof(OrganizationIDValue)];

			// TODO: Rework to call of FABookPeriodRepository.FindFirstFABookYear
			return PXSelect<
				FABookYear,
				Where<FABookYear.bookID, Equal<Required<FABook.bookID>>,
					And<FABookYear.organizationID, Equal<Required<FABookYear.organizationID>>>>,
				OrderBy<
					Asc<FABookYear.year>>>
				.SelectSingleBound(cache.Graph, new object[] { }, bookID, organizationID)
				.RowCast<FABookYear>()
				.FirstOrDefault()
				?.Year;
		}
	}

	public class LastCalendarYear<BookIDValue, OrganizationIDValue> : BqlFormulaEvaluator<BookIDValue, OrganizationIDValue>, IBqlOperand
		where BookIDValue : IBqlOperand
		where OrganizationIDValue : IBqlOperand
		{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
			{
			int? bookID = (int?)parameters[typeof(BookIDValue)];
			int? organizationID = (int?)parameters[typeof(OrganizationIDValue)];
			
			// TODO: Rework to call of FABookPeriodRepository.FindLastFABookYear
			return PXSelect<
				FABookYear,
				Where<FABookYear.bookID, Equal<Required<FABook.bookID>>,
					And<FABookYear.organizationID, Equal<Required<FABookYear.organizationID>>>>,
				OrderBy<
					Desc<FABookYear.year>>>
				.SelectSingleBound(cache.Graph, new object[] { }, bookID, organizationID)
				.RowCast<FABookYear>()
				.FirstOrDefault()
				?.Year;
			}
	}

	[Serializable]
	public partial class BoundaryYears : IBqlTable
			{
		#region FromYear
		public abstract class fromYear : IBqlField { }

		/// <summary>
		/// The financial year starting from which the periods will be generated in the system.
		/// </summary>
		[PXString(4, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "From Year")]
		public virtual string FromYear { get; set; }
		#endregion
		#region ToYear
		public abstract class toYear : IBqlField { }

		/// <summary>
		/// The financial year till which the periods will be generated in the system.
		/// </summary>
		[PXString(4, IsFixed = true)]
		[PXDefault("")]
		[PXUIField(DisplayName = "To Year")]
		public virtual string ToYear { get; set; }
		#endregion
			}

	// DO NOT DELETE. FEATURE UNDER CONSTRUCTION
	// FAOrganizationBook 1

	[Serializable]
	[PXProjection(typeof(Select2<
		FABook,
		LeftJoin<Organization,
			On<
			Where2<FeatureInstalled<FeaturesSet.multipleCalendarsSupport>,
				And<FABook.updateGL, Equal<True>>>>>>))]
	public partial class FAOrganizationBook : FABook
	{
		public new abstract class selected : IBqlField { }
		public new abstract class bookID : IBqlField { }

		#region BookCode
		public new abstract class bookCode : IBqlField { }
		/// <summary>
		/// A string identifier, which contains a key value. This field is also a selector for navigation.
		/// </summary>
		/// <value>The value can be entered only manually.</value>
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC", BqlTable = typeof(FABook))]
		[PXUIField(DisplayName = "Book ID")]
		public override string BookCode { get; set; }
		#endregion

		#region RawOrganizationID
		public abstract class rawOrganizationID : IBqlField { }

		[PXDBInt(BqlField = typeof(Organization.organizationID))]
		public virtual int? RawOrganizationID { get; set; }
		#endregion

		#region OrganizationID
		public abstract class organizationID : IBqlField { }

		[PXInt(IsKey = true)]
		[PXFormula(typeof(IsNull<FAOrganizationBook.rawOrganizationID, FinPeriod.organizationID.masterValue>))]
		public virtual int? OrganizationID { get; set; }
		#endregion

		#region OrganizationCD
		public abstract class organizationCD : IBqlField { }

		//[PXDimension("BIZACCT")]
		[PXDBString(30, IsUnicode = true, InputMask = "", BqlTable = typeof(Organization))]
		[PXUIField(DisplayName = "Company ID")]
		[PXUIVisible(typeof(Where<FeatureInstalled<FeaturesSet.multipleCalendarsSupport>>))]
		public virtual string OrganizationCD { get; set; }
		#endregion


		#region FirstCalendarYear
		public abstract class firstCalendarYear : IBqlField { }
		/// <summary>
		/// The first year of calendar for the book.
		/// </summary>
		/// <value>
		/// This is an unbound information field.
		/// </value>
		[PXString(4, IsFixed = true)]
		[PXUIField(DisplayName = "First Calendar Year", Enabled = false)]
		[PXFormula(typeof(FirstCalendarYear<FAOrganizationBook.bookID, FAOrganizationBook.organizationID>))]
		// TODO: Rework to PXDBScalar after AC-122596
		/*[PXDBScalar(typeof(Search<
			FABookYear.year,
			Where<FABookYear.bookID, Equal<FABook.bookID>,
				And<FABookYear.organizationID, Equal<IsNull<Organization.organizationID, FinPeriod.organizationID.masterValue>>>>,
			OrderBy<
				Asc<FABookYear.year>>>))]
		*/
		public virtual string FirstCalendarYear { get; set; }
		#endregion
		#region LastCalendarYear
		public abstract class lastCalendarYear : IBqlField { }
		/// <summary>
		/// The last year of calendar for the book.
		/// </summary>
		/// <value>
		/// This is an unbound information field.
		/// </value>
		[PXString(4, IsFixed = true)]
		[PXUIField(DisplayName = "Last Calendar Year", Enabled = false)]
		[PXFormula(typeof(LastCalendarYear<FAOrganizationBook.bookID, FAOrganizationBook.organizationID>))]
		// TODO: Rework to PXDBScalar after AC-122596
		/*[PXDBScalar(typeof(Search<
			FABookYear.year,
			Where<FABookYear.bookID, Equal<FABook.bookID>,
				And<FABookYear.organizationID, Equal<IsNull<Organization.organizationID, FinPeriod.organizationID.masterValue>>>>,
			OrderBy<
				Desc<FABookYear.year>>>))]*/
		public virtual string LastCalendarYear { get; set; }
		#endregion
		}

	// DO NOT DELETE. FEATURE UNDER CONSTRUCTION
	// FAOrganizationBook 2

	/*
	[Serializable]
	[PXProjection(typeof(Select2<
		FABook, 
		LeftJoin<Organization, 
			On<
			Where2<FeatureInstalled<FeaturesSet.multipleCalendarsSupport>, 
				And<FABook.updateGL, Equal<True>>>>>>))]
	public partial class FAOrganizationBook : IBqlTable
	{
		#region Selected
		public abstract class selected : IBqlField { }
		/// <summary>
		/// An unbound service field, which indicates that the book is marked for processing.
		/// </summary>
		/// <value>
		/// If the value of the field is <c>true</c>, the book will be processed; otherwise, the book will not be processed.
		/// </value>
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		#endregion
		#region BookID
		public abstract class bookID : IBqlField { }
		/// <summary>
		/// The identifier of the book.
		/// The identifier is used for foreign references; it can be negative for newly inserted records.
		/// </summary>
		/// <value>
		/// A unique integer number.
		/// </value>
		[PXDBInt(BqlTable = typeof(FABook))]
		public virtual int? BookID { get; set; }
		#endregion
		#region BookCode
		public abstract class bookCode : IBqlField { }
		/// <summary>
		/// A string identifier, which contains a key value. This field is also a selector for navigation.
		/// </summary>
		/// <value>The value can be entered only manually.</value>
		[PXDefault]
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC", BqlTable = typeof(FABook))]
		[PXUIField(DisplayName = "Book ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
		public virtual string BookCode { get; set; }
		#endregion

		#region RawOrganizationID
		public abstract class rawOrganizationID : IBqlField { }

		[PXDBInt(BqlField = typeof(Organization.organizationID))]
		public virtual int? RawOrganizationID { get; set; }
		#endregion

		#region OrganizationID
		public abstract class organizationID : IBqlField { }

		[PXInt(IsKey = true)]
		[PXFormula(typeof(IsNull<FAOrganizationBook.rawOrganizationID, FinPeriod.organizationID.masterValue>))]
		public virtual int? OrganizationID { get; set; }
		#endregion

		#region OrganizationCD
		public abstract class organizationCD : IBqlField { }

		//[PXDimension("BIZACCT")]
		[PXDBString(30, IsUnicode = true, InputMask = "", BqlTable = typeof(Organization))]
		[PXUIField(DisplayName = "Company ID")]
		[PXUIVisible(typeof(Where<FeatureInstalled<FeaturesSet.multipleCalendarsSupport>>))]
		public virtual string OrganizationCD { get; set; }
		#endregion

		#region Description
		public abstract class description : IBqlField { }
		/// <summary>
		/// The description of the book.
		/// </summary>
		[PXDBString(60, IsUnicode = true, BqlTable = typeof(FABook))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		public virtual string Description { get; set; }
		#endregion
		#region UpdateGL
		public abstract class updateGL : IBqlField { }
		/// <summary>
		/// A flag that determines whether the book posts FA transaction data to the General Ledger module.
		/// </summary>
		[PXDBBool(BqlTable = typeof(FABook))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Posting Book")]
		public virtual bool? UpdateGL { get; set; }
		#endregion

		#region FirstCalendarYear
		public abstract class firstCalendarYear : IBqlField { }
		/// <summary>
		/// The first year of calendar for the book.
		/// </summary>
		/// <value>
		/// This is an unbound information field.
		/// </value>
		[PXString(4, IsFixed = true)]
		[PXUIField(DisplayName = "First Calendar Year", Enabled = false)]
		[PXDBScalar(typeof(Search<
			FABookYear.year,
			Where<FABookYear.bookID, Equal<FABook.bookID>,
				And<FABookYear.organizationID, Equal<IsNull<Organization.organizationID, FinPeriod.organizationID.masterValue>>>>,
			OrderBy<
				Asc<FABookYear.year>>>))]
		public virtual string FirstCalendarYear { get; set; }
		#endregion
		#region LastCalendarYear
		public abstract class lastCalendarYear : IBqlField { }
		/// <summary>
		/// The last year of calendar for the book.
		/// </summary>
		/// <value>
		/// This is an unbound information field.
		/// </value>
		[PXString(4, IsFixed = true)]
		[PXUIField(DisplayName = "Last Calendar Year", Enabled = false)]
		[PXDBScalar(typeof(Search<
			FABookYear.year,
			Where<FABookYear.bookID, Equal<FABook.bookID>,
				And<FABookYear.organizationID, Equal<IsNull<Organization.organizationID, FinPeriod.organizationID.masterValue>>>>,
			OrderBy<
				Desc<FABookYear.year>>>))]
		public virtual string LastCalendarYear { get; set; }
		#endregion
	}
	*/
}