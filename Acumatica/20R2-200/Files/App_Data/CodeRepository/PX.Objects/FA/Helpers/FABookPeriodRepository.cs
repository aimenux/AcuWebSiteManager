using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.FA
{
	public class FABookPeriodRepository : IFABookPeriodRepository
	{
		protected virtual Dictionary<int, FABook> FABooks => PXDatabase.GetSlot<FABookCollection>(nameof(FABookCollection), typeof(FABook)).Books;

		protected readonly PXGraph Graph;

		public FABookPeriodRepository(PXGraph graph)
		{
			Graph = graph;
		}

		public static void CheckNotNullIDContract(int? id, string name)
		{
			if (id == null)
			{
				throw new ArgumentNullException(name);
			}
		}

		public static void CheckNotNullObjectContract(object obj, string name)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(name);
			}
		}

		public static void CheckNotNullStringContract(string str, string name)
		{
			if (str == null)
			{
				throw new ArgumentNullException(name);
			}
		}

		public static void CheckNotNullOrEmptyStringContract(string str, string name)
		{
			if (string.IsNullOrEmpty(str))
			{
				throw new ArgumentNullException(name);
			}
		}

		public FABook FindFABook(int? bookID)
		{
			CheckNotNullIDContract(bookID, nameof(bookID));

			FABooks.TryGetValue((int)bookID, out FABook book);
			return book;
		}

		public bool IsPostingFABook(int? bookID)
		{
			FABook book = FindFABook(bookID);
			if (book == null)
			{
				throw new ArgumentOutOfRangeException(nameof(bookID));
			}

			return book.UpdateGL == true;
		}

		public int GetFABookPeriodOrganizationID(int? bookID, int? assetID, bool check = true)
		{
			if (check)
			{
				CheckNotNullIDContract(bookID, nameof(bookID));
				CheckNotNullIDContract(assetID, nameof(assetID));
			}

			if (bookID == null || !IsPostingFABook(bookID))
			{
				return FinPeriod.organizationID.MasterValue;
			}
			else
			{
				FixedAsset asset = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(Graph, assetID);
				if (asset == null)
				{
					throw new ArgumentOutOfRangeException(nameof(assetID));
				}

				return (int)PXAccess.GetParentOrganizationID(asset.BranchID);
			}
		}

		public int GetFABookPeriodOrganizationID(FABookBalance balance, bool check = true)
		{
			return GetFABookPeriodOrganizationID(balance.BookID, balance.AssetID, check);
		}


		public IYearSetup FindFABookYearSetup(FABook book, bool clearQueryCache = false)
		{
			BqlCommand selectCommand;
			if (book.UpdateGL == true)
			{
				selectCommand = new Select<FinYearSetup>();
			}
			else
			{
				selectCommand = new Select<
					FABookYearSetup,
					Where<FABookYearSetup.bookID, Equal<Required<FABook.bookID>>>>();
			}
			PXView view = new PXView(Graph, true, selectCommand);
			if (clearQueryCache)
			{
				view.Clear();
			}
			return view.SelectSingle(book.BookID) as IYearSetup;
		}

		public IYearSetup FindFABookYearSetup(int? bookID, bool clearQueryCache = false)
		{
			return FindFABookYearSetup(FindFABook(bookID), clearQueryCache);
		}

		public IEnumerable<IPeriodSetup> FindFABookPeriodSetup(FABook book, bool clearQueryCache = false)
		{
			BqlCommand selectCommand;
			if (book.UpdateGL == true)
			{
				selectCommand = new Select3<FinPeriodSetup, OrderBy<Asc<FinPeriodSetup.periodNbr>>>();
			}
			else
			{
				selectCommand = new Select<
					FABookPeriodSetup,
					Where<FABookPeriodSetup.bookID, Equal<Required<FABook.bookID>>>, 
					OrderBy<Asc<FABookPeriodSetup.periodNbr>>>();
			}
			PXView view = new PXView(Graph, true, selectCommand);
			if (clearQueryCache)
			{
				view.Clear();
			}
			return view.SelectMulti(book.BookID).Cast<IPeriodSetup>();
		}

		public IEnumerable<IPeriodSetup> FindFABookPeriodSetup(int? bookID, bool clearQueryCache = false)
		{
			return FindFABookPeriodSetup(FindFABook(bookID), clearQueryCache);
		}

		public FABookYear FindFirstFABookYear(int? bookID, int? organizationID, bool clearQueryCache = false, bool mergeCache = false)
		{
			CheckNotNullIDContract(bookID, nameof(bookID));
			CheckNotNullIDContract(organizationID, nameof(organizationID));

			return new Select<
				FABookYear,
				Where<FABookYear.bookID, Equal<Required<FABook.bookID>>,
					And<FABookYear.organizationID, Equal<Required<Organization.organizationID>>>>,
				OrderBy<
					Asc<FABookYear.year>>>()
				.CreateView(Graph, clearQueryCache, mergeCache)
				.SelectSingle(bookID, organizationID)
					as FABookYear;
		}

		public FABookYear FindLastFABookYear(int? bookID, int? organizationID, bool clearQueryCache = false, bool mergeCache = false)
		{
			CheckNotNullIDContract(bookID, nameof(bookID));
			CheckNotNullIDContract(organizationID, nameof(organizationID));

			return new Select<
				FABookYear,
				Where<FABookYear.bookID, Equal<Required<FABook.bookID>>,
					And<FABookYear.organizationID, Equal<Required<Organization.organizationID>>>>,
				OrderBy<
					Desc<FABookYear.year>>>()
				.CreateView(Graph, clearQueryCache, mergeCache)
				.SelectSingle(bookID, organizationID)
					as FABookYear;
		}

		public FABookYear FindMasterFABookYearByID(FABook book, string yearNumber, bool clearQueryCache = false, bool mergeCache = false)
		{
			CheckNotNullObjectContract(book, nameof(book));
			CheckNotNullOrEmptyStringContract(yearNumber, nameof(yearNumber));

			return new Select<
				FABookYear,
				Where<FABookYear.bookID, Equal<Required<FABook.bookID>>,
					And<FABookYear.organizationID, Equal<FinPeriod.organizationID.masterValue>,
					And<FABookYear.year, Equal<Required<FABookYear.year>>>>>>()
				.CreateView(Graph, clearQueryCache, mergeCache)
				.SelectSingle(book.BookID, yearNumber)
					as FABookYear;
		}

		public FABookPeriod FindMasterFABookPeriodByID(FABook book, string periodID, bool clearQueryCache = false, bool mergeCache = false)
		{
			CheckNotNullObjectContract(book, nameof(book));
			return FindMasterFABookPeriodByID(book.BookID, periodID, clearQueryCache, mergeCache);
		}

		protected FABookPeriod FindMasterFABookPeriodByID(int? bookID, string periodID, bool clearQueryCache = false, bool mergeCache = false)
		{
			CheckNotNullObjectContract(bookID, nameof(bookID));
			CheckNotNullOrEmptyStringContract(periodID, nameof(periodID));

 			return new Select<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABook.bookID>>,
					And<FABookPeriod.organizationID, Equal<FinPeriod.organizationID.masterValue>,
					And<FABookPeriod.finPeriodID, Equal<Required<FABookPeriod.finPeriodID>>>>>>()
				.CreateView(Graph, clearQueryCache, mergeCache)
				.SelectSingle(bookID, periodID)
					as FABookPeriod;
		}

		public FABookPeriod FindNextNonAdjustmentMasterFABookPeriod(FABook book, string prevFABookPeriodID, bool clearQueryCache = false, bool mergeCache = false)
		{
			CheckNotNullObjectContract(book, nameof(book));
			CheckNotNullOrEmptyStringContract(prevFABookPeriodID, nameof(prevFABookPeriodID));

			return new Select<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABook.bookID>>,
					And<FABookPeriod.organizationID, Equal<FinPeriod.organizationID.masterValue>,
					And<FABookPeriod.finPeriodID, Greater<Required<FABookPeriod.finPeriodID>>,
					And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>>>>>,
				OrderBy<
					Asc<FABookPeriod.finPeriodID>>>()
				.CreateView(Graph, clearQueryCache, mergeCache)
				.SelectSingle(book.BookID, prevFABookPeriodID)
					as FABookPeriod;
		}

		public FABookPeriod FindLastNonAdjustmentOrganizationFABookPeriodOfYear(FAOrganizationBook book, string finYear, bool clearQueryCache = false, bool mergeCache = false)
		{
			CheckNotNullObjectContract(book, nameof(book));
			CheckNotNullOrEmptyStringContract(finYear, nameof(finYear));

			return new Select<
				FABookPeriod,
				Where<FABookPeriod.finYear, Equal<Required<FABookPeriod.finYear>>,
					And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
					And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>>>>>,
				OrderBy<
					Desc<FABookPeriod.finPeriodID>>>()
			.CreateView(Graph, clearQueryCache, mergeCache)
			.SelectSingle(finYear, book.BookID, book.OrganizationID)
				as FABookPeriod;
		}

		public FABookPeriod FindFABookPeriodOfDate(DateTime? date, int? bookID, int? assetID, bool check = true)
		{
			if (check)
			{
				CheckNotNullObjectContract(date, nameof(date));
				CheckNotNullIDContract(bookID, nameof(bookID));
				CheckNotNullIDContract(assetID, nameof(assetID));
			}

			FABookPeriod period = (FABookPeriod)new Select<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
					And<FABookPeriod.startDate, LessEqual<Required<FABookPeriod.startDate>>,
					And<FABookPeriod.endDate, Greater<Required<FABookPeriod.endDate>>>>>>>()
			.CreateView(Graph, clearQueryCache: true, mergeCache: false)
			.SelectSingle(
				bookID, 
				GetFABookPeriodOrganizationID(bookID, assetID, check), 
				date, 
				date);

			if (check && period == null)
			{
				throw new PXFABookPeriodException();
			}

			return period;
		}

		public FABookPeriod FindFABookPeriodOfDateByBranchID(DateTime? date, int? bookID, int? branchID, bool check = true)
		{
			CheckNotNullObjectContract(date, nameof(date));
			CheckNotNullIDContract(bookID, nameof(bookID));
			CheckNotNullIDContract(branchID, nameof(branchID));

			FABookPeriod period = PXSelect<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
					And<FABookPeriod.startDate, LessEqual<Required<FABookPeriod.startDate>>,
					And<FABookPeriod.endDate, Greater<Required<FABookPeriod.endDate>>>>>>>
				.Select(
					Graph, 
					bookID, 
					IsPostingFABook(bookID) 
						? PXAccess.GetParentOrganizationID(branchID) 
						: FinPeriod.organizationID.MasterValue, 
					date, 
					date);
			if (check && period == null)
			{
				throw new PXFABookPeriodException();
			}

			return period;
		}

		public string GetFABookPeriodIDOfDate(DateTime? date, int? bookID, int? assetID, bool check = true)
		{
			if (check)
			{
				CheckNotNullObjectContract(date, nameof(date));
				CheckNotNullIDContract(bookID, nameof(bookID));
				CheckNotNullIDContract(assetID, nameof(assetID));
			}

			return FindFABookPeriodOfDate(date, bookID, assetID, check)?.FinPeriodID;
		}

		public short GetQuarterNumberOfDate(DateTime? date, int? bookID, int? assetID)
		{
			CheckNotNullObjectContract(date, nameof(date));
			CheckNotNullIDContract(bookID, nameof(bookID));
			CheckNotNullIDContract(assetID, nameof(assetID));

			decimal PeriodNbr = Convert.ToDecimal(FindFABookPeriodOfDate(date, bookID, assetID).PeriodNbr);
			IYearSetup yearSetup = FindFABookYearSetup(bookID);
			switch (yearSetup.FPType)
			{
				case FiscalPeriodSetupCreator.FPType.Month:
					return (short)decimal.Ceiling(PeriodNbr / 3);
				case FiscalPeriodSetupCreator.FPType.Quarter:
					return (short)PeriodNbr;
				default:
					throw new PXException(Messages.QuarterIsUndefined);
			}
		}

		public short GetPeriodNumberOfDate(DateTime? date, int? bookID, int? assetID)
		{
			return Convert.ToInt16(FindFABookPeriodOfDate(date, bookID, assetID).PeriodNbr);
		}

		public int GetYearNumberOfDate(DateTime? date, int? bookID, int? assetID)
		{
			return Convert.ToInt32(FindFABookPeriodOfDate(date, bookID, assetID).FinYear);
		}

		public FABookPeriod FindOrganizationFABookPeriodByID(string periodID, int? bookID, int? assetID, bool clearQueryCache = false, bool mergeCache = false)
		{
			return new Select<
				FABookPeriod,
				Where<FABookPeriod.finPeriodID, Equal<Required<FABookPeriod.finPeriodID>>,
					And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>>>>>()
				.CreateView(Graph, clearQueryCache, mergeCache)
				.SelectSingle(periodID, bookID, GetFABookPeriodOrganizationID(bookID, assetID))
					as FABookPeriod;
		}

	    public FABookPeriod FindMappedPeriod(FABookPeriod.Key fromPeriodKey, FABookPeriod.Key toPeriodKey)
	    {
	        if (fromPeriodKey.BookID != toPeriodKey.BookID)
	            return null;

	        if (!IsPostingFABook(fromPeriodKey.BookID))
	            return null;

	        FABookPeriod oldPeriod = FindByKey(fromPeriodKey.BookID, fromPeriodKey.OrganizationID, fromPeriodKey.PeriodID);

	        return FindPeriodByMasterPeriodID(toPeriodKey.BookID, toPeriodKey.OrganizationID, oldPeriod?.MasterFinPeriodID);
	    }

	    public FABookPeriod FindByKey(int? bookID, int? organizaionID, string periodID)
	    {
	        return PXSelect<FABookPeriod,
	                Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
	                    And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
	                    And<FABookPeriod.finPeriodID, Equal<Required<FABookPeriod.finPeriodID>>>>>>
	                .Select(Graph, bookID, organizaionID, periodID);
	    }

	    public FABookPeriod FindPeriodByMasterPeriodID(int? bookID, int? organizaionID, string masterPeriodID)
	    {
	        return PXSelect<FABookPeriod,
	                Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
	                    And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
	                        And<FABookPeriod.masterFinPeriodID, Equal<Required<FABookPeriod.masterFinPeriodID>>>>>>
	            .Select(Graph, bookID, organizaionID, masterPeriodID);
	    }

		public virtual FABookPeriod GetMappedFABookPeriod(int? bookID, int? sourceOrganizationID, string sourcefinPeriodID, int? targetOrganizationID)
		{
			return FindMappedFABookPeriod(bookID, sourceOrganizationID, sourcefinPeriodID, targetOrganizationID)
				.ThisOrRaiseIfHasError()
				.Result;
		}

		public virtual ProcessingResult<FABookPeriod> FindMappedFABookPeriod(int? bookID, int? sourceOrganizationID, string sourcefinPeriodID, int? targetOrganizationID)
		{
			FABookPeriod sourceFABookPeriod = FindByKey(bookID, sourceOrganizationID, sourcefinPeriodID);

			ProcessingResult<FABookPeriod> result = ProcessingResult<FABookPeriod>.CreateSuccess(sourceFABookPeriod);

			if (sourceFABookPeriod == null)
			{
				string errorMessage = PXMessages.LocalizeFormat(
					Messages.PeriodDoesNotExistForBookAndCompany,
					PeriodIDAttribute.FormatForError(sourcefinPeriodID),
					FindFABook(bookID).BookCode,
					PXAccess.GetOrganizationCD(sourceOrganizationID));

				result.AddErrorMessage(errorMessage);
			}
			else if (IsPostingFABook(bookID) && sourceOrganizationID != targetOrganizationID)
			{
				result = GetFABookPeriodByMasterPeriodID(bookID, targetOrganizationID, sourceFABookPeriod?.MasterFinPeriodID);
			}
			
			return result;
		}

		public virtual ProcessingResult<FABookPeriod> FindMappedFABookPeriodUsingFinPeriod(int? bookID, int? sourceOrganizationID, string sourcefinPeriodID, int? targetOrganizationID)
		{
			IFinPeriodRepository finPeriodRepository = Graph.GetService<IFinPeriodRepository>();

			string targetFinPeriodID = finPeriodRepository.GetMappedPeriod(sourceOrganizationID, sourcefinPeriodID, targetOrganizationID)?.FinPeriodID;
			FABookPeriod targetFABookPeriod = FindByKey(bookID, targetOrganizationID, targetFinPeriodID);

			ProcessingResult<FABookPeriod> result = ProcessingResult<FABookPeriod>.CreateSuccess(targetFABookPeriod);

			if (targetFABookPeriod == null)
			{
				string errorMessage = PXMessages.LocalizeFormat(
					Messages.PeriodDoesNotExistForBookAndCompany,
					PeriodIDAttribute.FormatForError(sourcefinPeriodID),
					FindFABook(bookID).BookCode,
					PXAccess.GetOrganizationCD(sourceOrganizationID));

				result.AddErrorMessage(errorMessage);
			}

			return result;
		}

		public virtual FABookPeriod GetMappedFABookPeriodByBranches(int? bookID, int? sourceBranchID, string sourcefinPeriodID, int? targetBranchID)
		{
			return GetMappedFABookPeriod(
				bookID,
				PXAccess.GetParentOrganizationID(sourceBranchID),
				sourcefinPeriodID,
				PXAccess.GetParentOrganizationID(targetBranchID));
		}

		public virtual ProcessingResult<FABookPeriod> GetFABookPeriodByMasterPeriodID(int? bookID, int? organizationID, string masterFinPeriodID)
		{
			FABookPeriod period = SelectFrom<FABookPeriod>
				.Where<FABookPeriod.bookID.IsEqual<@P.AsInt>
					.And<FABookPeriod.organizationID.IsEqual<@P.AsInt>>
					.And<FABookPeriod.masterFinPeriodID.IsEqual<@P.AsString>>>
				.View
				.ReadOnly
				.Select(Graph, bookID, organizationID, masterFinPeriodID);

			ProcessingResult<FABookPeriod> result = ProcessingResult<FABookPeriod>.CreateSuccess(period);

			if (period == null)
			{
				string errorMessage = PXMessages.LocalizeFormatNoPrefix(
					GL.Messages.RelatedFinPeriodsForMasterDoesNotExistForCompany,
					PeriodIDAttribute.FormatForError(masterFinPeriodID),
					PXAccess.GetOrganizationCD(organizationID));

				result.AddErrorMessage(errorMessage);
			}

			return result;
		}
	}
}
