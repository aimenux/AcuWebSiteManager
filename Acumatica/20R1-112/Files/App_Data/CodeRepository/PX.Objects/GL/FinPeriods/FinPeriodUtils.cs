using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.FA;
using PX.Objects.GL.DAC.Abstract;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL.FinPeriods
{
	public class FinPeriodUtils : IFinPeriodUtils
	{
		protected PXGraph Graph { get; set; }

		public const int YEAR_LENGTH = 4;
		public const int PERIOD_LENGTH = 2;
		public const int FULL_LENGHT = YEAR_LENGTH + PERIOD_LENGTH;
		public const string FirstPeriodOfYear = "01";

        public FinPeriodUtils(PXGraph graph)
		{
			Graph = graph;
		}

        /// <summary>
        /// Format Period to string that can be used in an error message.
        /// </summary>
        public static string FormatForError(string period)
		{
			return FinPeriodIDFormattingAttribute.FormatForError(period);
		}

		/// <summary>
		/// Format Period to string that can be displayed in the control.
		/// </summary>
		/// <param name="period">Period in database format</param>
		public static string FormatForDisplay(string period)
		{
			return FinPeriodIDFormattingAttribute.FormatForDisplay(period);
		}

		/// <summary>
		/// Format period to database format
		/// </summary>
		/// <param name="period">Period in display format</param>
		/// <returns></returns>
		public static string UnFormatPeriod(string period)
		{
			return FinPeriodIDFormattingAttribute.FormatForStoring(period);
		}

		const string FirstPeriodNumber = "01";

		public static string GetFirstFinPeriodIDOfYear(IYear year)
		{
			return $"{year.Year}{FirstPeriodNumber}";
		}

		public static string GetFirstFinPeriodIDOfYear(string finPeriodID)
		{
			return $"{finPeriodID.Substring(0, 4)}{FirstPeriodNumber}";
		}

		public static string GetYearIDOfPeriod(string periodID) => periodID.Substring(0, YEAR_LENGTH);

		public static string GetNextYearID(string finPeriodId) => $"{Convert.ToInt32(finPeriodId.Substring(0, 4)) + 1}";

		public static string GetPreviousYearID(string finPeriodId) => $"{Convert.ToInt32(finPeriodId.Substring(0, 4)) - 1}";

		

		public static bool FinPeriodEqual(string period1, string period2, FinPeriodComparison comparison)
		{
			if (period1 != null && period2 != null && period1.Length >= FinPeriodUtils.FULL_LENGHT && period2.Length >= FinPeriodUtils.FULL_LENGHT)
			{
				switch (comparison)
				{
					case FinPeriodComparison.Full:
						break;
					case FinPeriodComparison.Year:
						period1 = period1.Substring(0, FinPeriodUtils.YEAR_LENGTH);
						period2 = period2.Substring(0, FinPeriodUtils.YEAR_LENGTH);
						break;
					case FinPeriodComparison.Month:
						period1 = period1.Substring(FinPeriodUtils.YEAR_LENGTH, FinPeriodUtils.PERIOD_LENGTH);
						period2 = period2.Substring(FinPeriodUtils.YEAR_LENGTH, FinPeriodUtils.PERIOD_LENGTH);
						break;
				}
			}

			return string.Equals(period1, period2);
		}

		public static string FiscalYear(string aFiscalPeriod)
		{
			return aFiscalPeriod.Substring(0, YEAR_LENGTH);
		}

		public static string PeriodInYear(string aFiscalPeriod)
		{
			return aFiscalPeriod.Substring(YEAR_LENGTH, PERIOD_LENGTH);
		}

		/// <summary>
		/// Attempts to extract integer values of the financial year and the number of the financial period inside the year
		/// from a Financial Period ID (<see cref="FinPeriod.FinPeriodID"/>).
		/// </summary>
		/// <param name="fiscalPeriodID">The ID of the period to extract parts from</param>
		/// <param name="year">Output: the financial year, to which the period belongs</param>
		/// <param name="periodNbr">Output: the number of the period in its financial year</param>
		/// <returns><c>true</c> upon success or <c>false</c> if failed to parse due to incorrect format of the input period ID</returns>
		public static bool TryParse(string fiscalPeriodID, out int year, out int periodNbr)
		{
			try
			{
				year = int.Parse(FinPeriodUtils.FiscalYear(fiscalPeriodID));
				periodNbr = int.Parse(FinPeriodUtils.PeriodInYear(fiscalPeriodID));
			}
			catch (FormatException)
			{
				year = -1;
				periodNbr = -1;
				return false;
			}
			return true;
		}

		public static string Assemble(string aYear, string aPeriod)
		{
			if (aYear.Length != 4 || aPeriod.Length != 2)
			{
				throw new PXArgumentException((string)null, Messages.YearOrPeriodFormatIncorrect);
			}
			return (aYear + aPeriod);
		}

		public static string OffsetPeriod(string FiscalPeriodID, int counter, int periodsInYear)
		{
			int years = Convert.ToInt32(FiscalPeriodID.Substring(0, 4));
			int periods = Convert.ToInt32(FiscalPeriodID.Substring(4, 2));

			int endYear = years - 1 + (int)decimal.Ceiling((periods + counter) / (decimal)periodsInYear);
			int endPeriod;
			if (periods + counter > 0)
			{
				endPeriod = (endPeriod = (periods + counter) % periodsInYear) == 0 ? periodsInYear : endPeriod;
			}
			else
			{
				endPeriod = (endPeriod = (periods + counter) % periodsInYear) == 0 ? periodsInYear : periodsInYear + endPeriod;
			}

			return $"{endYear:0000}{endPeriod:00}";
		}

		public static string Min(string period1, string period2)
		{
			return string.CompareOrdinal(period1, period2) > 0 ? period2 : period1;
		}

		public static string Max(string period1, string period2)
		{
			return string.CompareOrdinal(period1, period2) > 0 ? period1 : period2;
		}

		public static DateTime Max(DateTime date1, DateTime date2)
		{
			return DateTime.Compare(date1, date2) > 0 ? date1 : date2;
		}

		public static DateTime? Max(DateTime? date1, DateTime? date2)
		{
			if (date1.HasValue && !date2.HasValue)
				return date1;
			if (!date1.HasValue && date2.HasValue)
				return date2;
			if (!date1.HasValue && !date2.HasValue)
				return null;

			return DateTime.Compare((DateTime)date1, (DateTime)date2) > 0 ? date1 : date2;
		}

		public enum FinPeriodComparison
		{
			Full,
			Year,
			Month
		}

		public virtual void VerifyAndSetFirstOpenedFinPeriod<TFinPeriodField, TBranchField>(PXCache rowCache, object row, PXSelectBase<OrganizationFinPeriod> finPeriodView, Type fieldModuleClosed = null)
			where TFinPeriodField : class, IBqlField
			where TBranchField : class, IBqlField
		{
			OrganizationFinPeriod finPeriod = finPeriodView.Current;

			if (finPeriod != null)
			{
				bool isClosed = finPeriod.Status == FinPeriod.status.Closed;

				if (fieldModuleClosed != null)
				{
					isClosed |= (bool?)finPeriodView.Cache.GetValue(finPeriod, fieldModuleClosed.Name) == true;
				}
				bool canPostToClosedPeriod = CanPostToClosedPeriod();
				if (finPeriod.Status == FinPeriod.status.Inactive
					|| finPeriod.Status == FinPeriod.status.Locked
					|| (isClosed && !canPostToClosedPeriod))
				{
					string finPeriodID = (string)rowCache.GetValue<TFinPeriodField>(row);
					int? organizationID = PXAccess.GetParentOrganizationID((int?)rowCache.GetValue<TBranchField>(row));

					OrganizationFinPeriod firstopen = rowCache.Graph.GetService<IFinPeriodRepository>().FindFirstOpenFinPeriod(finPeriodID, organizationID, fieldModuleClosed);

					if (firstopen == null)
					{
						string userPeriod = Mask.Format("##-####", finPeriodView.Cache.GetValueExt<OrganizationFinPeriod.finPeriodID>(finPeriodView.Current).ToString());
						throw new PXSetPropertyException(GL.Messages.NoActivePeriodAfter, userPeriod);
					}

					rowCache.SetValue<TFinPeriodField>(row, firstopen.FinPeriodID);
				}
			}
		}

		public virtual void ValidateFinPeriod(IEnumerable<IAccountable> records, Type fieldModuleClosed = null)
		{
			var recordsByPeriod = records.AsEnumerable().GroupBy(record => record.FinPeriodID);

			ProcessingResult generalResult = new ProcessingResult();

			foreach (var recordsByPeriodGroup in recordsByPeriod)
			{
				int?[] orgnizationIDs = recordsByPeriodGroup.GroupBy(t => PXAccess.GetParentOrganizationID(t.BranchID))
					.Select(g => g.Key)
					.ToArray();

				ValidateFinPeriod(recordsByPeriodGroup.Key, orgnizationIDs, fieldModuleClosed, generalResult);
			}

			generalResult.RaiseIfHasError();
		}

		public void ValidateFinPeriod<T>(IEnumerable<T> records, 
			Func<T, string> getFinPeriodID, 
			Func<T, int?[]> getBranchIDs,
			Type fieldModuleClosed = null)
		{
			var recordsByPeriod = records.GroupBy(record => getFinPeriodID(record));

			ProcessingResult generalResult = new ProcessingResult();

			foreach (var recordsByPeriodGroup in recordsByPeriod)
			{
				int?[] orgnizationIDs = recordsByPeriodGroup
					.SelectMany(record => getBranchIDs(record).Select(PXAccess.GetParentOrganizationID))
					.Distinct()
					.ToArray();

				ValidateFinPeriod(recordsByPeriodGroup.Key, orgnizationIDs, fieldModuleClosed, generalResult);
			}

			generalResult.RaiseIfHasError();
		}

		protected virtual void ValidateFinPeriod(string finPeriodID,
			int?[] orgnizationIDs, 
			Type fieldModuleClosed = null, 
			ProcessingResult generalResult = null)
		{
			if (generalResult == null)
			{
				generalResult = new ProcessingResult();
			}

			ICollection<OrganizationFinPeriod> finPeriods =
								PXSelect<OrganizationFinPeriod,
										Where<OrganizationFinPeriod.organizationID, In<Required<OrganizationFinPeriod.organizationID>>,
											And<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>>>>
										.Select(Graph, orgnizationIDs, finPeriodID)
										.RowCast<OrganizationFinPeriod>()
										.ToArray();

			if (finPeriods.Count != orgnizationIDs.Length)
			{
				string[] organizationCDs = orgnizationIDs.Except(finPeriods.Select(period => period.OrganizationID))
															.Select(PXAccess.GetOrganizationCD)
															.ToArray();

				generalResult.AddErrorMessage(Messages.FinPeriodDoesNotExistForCompanies,
												FinPeriodIDFormattingAttribute.FormatForError(finPeriodID),
												organizationCDs.JoinIntoStringForMessageNoQuotes(20));
			}

			foreach (OrganizationFinPeriod finPeriod in finPeriods)
			{
				ProcessingResult result = CanPostToPeriod(finPeriod, fieldModuleClosed);

				generalResult.Aggregate(result);

				if (generalResult.Messages.Count > 20)
				{
					generalResult.RaiseIfHasError();
				}
			}
		}

		public virtual ProcessingResult CanPostToPeriod(IFinPeriod finPeriod, Type fieldModuleClosed = null)
		{
			ProcessingResult result = new ProcessingResult();
	
			if (finPeriod.Status == FinPeriod.status.Locked)
			{
				result.AddErrorMessage(Messages.FinPeriodIsLockedInCompany,
					FinPeriodIDFormattingAttribute.FormatForError(finPeriod.FinPeriodID),
					PXAccess.GetOrganizationCD(finPeriod.OrganizationID));

				return result;
			}

			if (AllowPostToUnlockedPeriodAnyway)
				return result;

			if (finPeriod.Status == FinPeriod.status.Inactive)
			{
				result.AddErrorMessage(Messages.FinPeriodIsInactiveInCompany, 
										FinPeriodIDFormattingAttribute.FormatForError(finPeriod.FinPeriodID), 
										PXAccess.GetOrganizationCD(finPeriod.OrganizationID));

				return result;
			}

			if (finPeriod.Status == FinPeriod.status.Closed)
			{
				result = HandleErrorThatPeriodIsClosed(finPeriod);

				if (result.HasWarningOrError)
					return result;
			}

			if (fieldModuleClosed != null)
			{
				PXCache cache = Graph.Caches[BqlCommand.GetItemType(fieldModuleClosed)];

				bool? isClosedInModule = (bool) cache.GetValue(finPeriod, fieldModuleClosed.Name);

				if (isClosedInModule == true)
				{
					result = HandleErrorThatPeriodIsClosed(finPeriod);
				}
			}

			return result;
		}

		public bool CanPostToClosedPeriod()
		{
			if (AllowPostToUnlockedPeriodAnyway)
				return true;

			GLSetup setup = PXSelect<GLSetup>.Select(Graph);

			return setup.RestrictAccessToClosedPeriods != true 
					|| !string.IsNullOrEmpty(PredefinedRoles.FinancialSupervisor)
					&& System.Web.Security.Roles.IsUserInRole(PXAccess.GetUserName(), PredefinedRoles.FinancialSupervisor);
		}

		public bool AllowPostToUnlockedPeriodAnyway
		{
			get
			{
				return PX.Common.PXContext.GetSlot<bool>("FinPeriodUtils.AllowPostToUnlockedPeriod");
			}
			set
			{
				PX.Common.PXContext.SetSlot<bool>("FinPeriodUtils.AllowPostToUnlockedPeriod", value);
			}
		}

		protected virtual ProcessingResult HandleErrorThatPeriodIsClosed(IFinPeriod finPeriod)
		{
			ProcessingResult result = new ProcessingResult();

			if (!CanPostToClosedPeriod())
			{
				result.AddErrorMessage(Messages.FinPeriodIsClosedInCompany,
										FinPeriodIDFormattingAttribute.FormatForError(finPeriod.FinPeriodID),
										PXAccess.GetOrganizationCD(finPeriod.OrganizationID));
			}

			return result;
		}

		public virtual OrganizationFinPeriod GetOpenOrganizationFinPeriodInSubledger<TClosedInSubledgerField>(string orgFinPeriodID, int? branchID)
			where TClosedInSubledgerField : IBqlField
		{
			int? organizationID = PXAccess.GetParentOrganizationID(branchID);
			OrganizationFinPeriod orgFinPeriod = PXSelect<
				OrganizationFinPeriod,
				Where<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>,
					And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>,
					And<TClosedInSubledgerField, NotEqual<True>>>>>
				.SelectWindowed(Graph, 0, 1, orgFinPeriodID, organizationID);

			if (orgFinPeriod == null)
			{
				throw new PXException(
					Messages.FiscalPeriodClosedInOrganization, 
					FinPeriodIDFormattingAttribute.FormatForError(orgFinPeriodID), 
					PXAccess.GetOrganizationCD(organizationID));
			}
			return orgFinPeriod;
		}

		protected virtual OrganizationFinPeriod GetNearestOpenOrganizationFinPeriodInSubledgerByOrganization<TClosedInSubledgerField>(string orgFinPeriodID, int? organizationID, Func<bool> additionalCondition = null)
			where TClosedInSubledgerField : IBqlField
		{
			OrganizationFinPeriod orgFinPeriod = PXSelect<
				OrganizationFinPeriod,
				Where<OrganizationFinPeriod.finPeriodID, GreaterEqual<Required<OrganizationFinPeriod.finPeriodID>>,
					And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>,
					And<TClosedInSubledgerField, NotEqual<True>,
					And<OrganizationFinPeriod.startDate, NotEqual<OrganizationFinPeriod.endDate>>>>>,
				OrderBy<Asc<OrganizationFinPeriod.finPeriodID>>>
				.SelectWindowed(Graph, 0, 1, orgFinPeriodID, organizationID);

			if (orgFinPeriod == null && (additionalCondition == null || additionalCondition()))
			{
				throw new PXException(
					Messages.NoOpenPeriodInOrganization,
					PXAccess.GetOrganizationCD(organizationID));
			}
			return orgFinPeriod;
		}

		public virtual OrganizationFinPeriod GetNearestOpenOrganizationFinPeriodInSubledger<TClosedInSubledgerField>(string orgFinPeriodID, int? branchID, Func<bool> additionalCondition = null)
			where TClosedInSubledgerField : IBqlField
		{
			int? organizationID = PXAccess.GetParentOrganizationID(branchID);
			return GetNearestOpenOrganizationFinPeriodInSubledgerByOrganization<TClosedInSubledgerField>(orgFinPeriodID, organizationID, additionalCondition);
		}

		public virtual OrganizationFinPeriod GetNearestOpenOrganizationFinPeriodInSubledger<TClosedInSubledgerField>(IPeriod orgFinPeriod)
			where TClosedInSubledgerField : IBqlField
		{
			return GetNearestOpenOrganizationFinPeriodInSubledgerByOrganization<TClosedInSubledgerField>(orgFinPeriod.FinPeriodID, orgFinPeriod.OrganizationID, null);
		}

		public virtual OrganizationFinPeriod GetOpenOrganizationFinPeriodInFA(string orgFinPeriodID, int? assetID)
		{
			OrganizationFinPeriod orgFinPeriod = PXSelectJoin<
				OrganizationFinPeriod,
				InnerJoin<Branch,
					On<Branch.organizationID, Equal<OrganizationFinPeriod.organizationID>>,
				InnerJoin<FixedAsset,
					On<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>,
					And<FixedAsset.branchID, Equal<Branch.branchID>>>>>,
				Where<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>,
					And<OrganizationFinPeriod.fAClosed, NotEqual<True>>>>
				.SelectWindowed(Graph, 0, 1, assetID, orgFinPeriodID);

			if (orgFinPeriod == null)
			{
				throw new PXException(
					Messages.FiscalPeriodClosed,
					FinPeriodIDFormattingAttribute.FormatForError(orgFinPeriodID));
			}
			return orgFinPeriod;
		}

		public string ComposeFinPeriodID(string yearNumber, string periodNumber) =>
			$"{yearNumber:0000}{periodNumber:00}";

		public (string yearNumber, string periodNumber) ParseFinPeriodID(string finPeriodID)
		{
			return (finPeriodID.Substring(0, 4), finPeriodID.Substring(4, 2));
	}

		public (int firstYear, int lastYear) GetFirstLastYearForGeneration(int? organizationID, int fromYear, int toYear, bool clearQueryCache = false)
		{
			IFinPeriodRepository finPeriodRepository = Graph.GetService<IFinPeriodRepository>();


			int? firstExistingYear = int.TryParse(finPeriodRepository.FindFirstYear(organizationID ?? 0, clearQueryCache)?.Year, out int parsedFirstExistingYear)
				? parsedFirstExistingYear 
				: (int?)null;
			int? lastExistingYear = int.TryParse(finPeriodRepository.FindLastYear(organizationID ?? 0, clearQueryCache)?.Year, out int parsedLastExistingYear)
				? parsedLastExistingYear
				: (int?)null;

			int lastYear = lastExistingYear ?? fromYear - 1;
			int firstYear = firstExistingYear ?? fromYear - 1;

			return (firstYear, lastYear);
}

		public void CheckParametersOfCalendarGeneration(int? organizationID, int fromYear, int toYear)
		{
			(int firstYear, int lastYear) = GetFirstLastYearForGeneration(organizationID, fromYear, toYear);

			if (fromYear > lastYear + 1)
			{
				throw new PXException(Messages.FromYearGreaterThanLastExistingYear, lastYear + 1);
			}
			if (toYear < firstYear - 1)
			{
				throw new PXException(Messages.ToYearLessThanFirstExistingYear, firstYear - 1);
			}
			if (toYear - fromYear + 1 > 99)
			{
				throw new PXException(Messages.CannotGenerateOver99Years);
			}
		}

		public virtual void CopyPeriods<TDAC, TFinPeriodID, TMasterFinPeriodID>(PXCache cache, TDAC src, TDAC dest)
			where TDAC : class, IBqlTable, new()
			where TFinPeriodID : class, IBqlField
			where TMasterFinPeriodID : class, IBqlField
		{
			if (!cache.ObjectsEqual<TFinPeriodID, TMasterFinPeriodID>(src, dest))
			{
				object origFinPeriodID = cache.GetValue<TFinPeriodID>(src);
				string tranPeriodID = (string)cache.GetValue<TMasterFinPeriodID>(src);
				try
				{
					FinPeriodIDAttribute.SetPeriodsByMaster<TFinPeriodID>(cache, dest, tranPeriodID);
					origFinPeriodID = cache.GetValue<TFinPeriodID>(dest);
					cache.RaiseFieldVerifying<TFinPeriodID>(dest, ref origFinPeriodID);
				}
				catch (PXException e)
				{
					cache.RaiseExceptionHandling<TFinPeriodID>(dest, origFinPeriodID, e);
				}
			}
		}
	}
}
