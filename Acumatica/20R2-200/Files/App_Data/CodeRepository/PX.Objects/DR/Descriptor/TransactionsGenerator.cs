using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.DR.Descriptor;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.DR
{
	public class TransactionsGenerator
	{
		protected PXGraph _graph;
		protected DRDeferredCode _code;
		protected Func<decimal, decimal> _roundingFunction;
		private IFinPeriodRepository _finPeriodRepository;

		/// <param name="roundingFunction">
		/// An optional parameter specifying a function that would be used to round
		/// the calculated transaction amounts. If <c>null</c>, the generator will use
		/// <see cref="PXDBCurrencyAttribute.BaseRound(PXGraph, decimal)"/> by default.
		/// </param>
		/// <param name="financialPeriodProvider">
		/// An optional parameter specifying an object that would be used to manipulate
		/// financial periods, e.g. extract a start date or an end date for a given period ID.
		/// If <c>null</c>, the generator will use <see cref="FinancialPeriodProvider.Default"/>. 
		/// </param>
		public TransactionsGenerator(
			PXGraph graph, 
			DRDeferredCode code, 
			IFinPeriodRepository finPeriodRepository = null,
			Func<decimal, decimal> roundingFunction = null)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));
			if (code == null) throw new ArgumentNullException(nameof(code));

			_graph = graph;
			_code = code;
			_roundingFunction = roundingFunction ?? (rawAmount => PXDBCurrencyAttribute.BaseRound(_graph, rawAmount));
			_finPeriodRepository = finPeriodRepository ?? _graph.GetService<IFinPeriodRepository>();
		}

		public virtual IList<DRScheduleTran> GenerateTransactions(
			DRSchedule deferralSchedule, 
			DRScheduleDetail scheduleDetail)
		{
			if (deferralSchedule == null) throw new ArgumentNullException(nameof(deferralSchedule));
			if (scheduleDetail == null) throw new ArgumentNullException(nameof(scheduleDetail));

			if(PXAccess.FeatureInstalled<CS.FeaturesSet.aSC606>())
			{
				ValidateTerms(scheduleDetail);
			}
			else
			{
				ValidateTerms(deferralSchedule);
			}
			

			int? organizationID = PXAccess.GetParentOrganizationID(scheduleDetail.BranchID);
			decimal defAmount = scheduleDetail.TotalAmt.Value;

			List<DRScheduleTran> list = new List<DRScheduleTran>();

			short lineCounter = 0;
			if (_code.ReconNowPct.Value > 0)
			{
				decimal recNowRaw = defAmount * _code.ReconNowPct.Value * 0.01m;
				decimal recNow = _roundingFunction(recNowRaw);
				defAmount -= recNow;

				lineCounter++;

				DRScheduleTran nowTran = new DRScheduleTran
				{
					BranchID = scheduleDetail.BranchID,
					AccountID = scheduleDetail.AccountID,
					SubID = scheduleDetail.SubID,
					Amount = recNow,
					RecDate = deferralSchedule.DocDate,
					FinPeriodID = scheduleDetail.FinPeriodID,
					TranPeriodID = scheduleDetail.TranPeriodID,

					LineNbr = lineCounter,
					DetailLineNbr = scheduleDetail.DetailLineNbr,
					ScheduleID = scheduleDetail.ScheduleID,
					ComponentID = scheduleDetail.ComponentID,
					Status = GetStatus(),
				};

				list.Add(nowTran);
			}

			bool isFlexibleDeferralCode = DeferredMethodType.RequiresTerms(_code.Method);

			DateTime? termStartDate = null;
			DateTime? termEndDate = null;
			if (isFlexibleDeferralCode)
			{
				bool isASC606 = PXAccess.FeatureInstalled<CS.FeaturesSet.aSC606>();
				termStartDate = isASC606 ? scheduleDetail.TermStartDate.Value : deferralSchedule.TermStartDate.Value;
				termEndDate = isASC606 ? scheduleDetail.TermEndDate.Value : deferralSchedule.TermEndDate.Value;
			}

			DateTime documentDate = deferralSchedule.DocDate.Value;
			FinPeriod documentFinPeriod = _finPeriodRepository.FindFinPeriodByDate(documentDate, organizationID);
			int occurrences = isFlexibleDeferralCode ? CalcOccurrences(termStartDate.Value, termEndDate.Value, organizationID) : _code.Occurrences.Value;

			List<DRScheduleTran> deferredList = new List<DRScheduleTran>(_code.Occurrences.Value);
			FinPeriod deferredPeriod = null;

			for (int i = 0; i < occurrences; i++)
			{
				try
				{
					if (deferredPeriod == null)
					{
						deferredPeriod = isFlexibleDeferralCode ? 
							_finPeriodRepository.FindFinPeriodByDate(termStartDate, organizationID):
							_finPeriodRepository.GetOffsetPeriod(scheduleDetail.FinPeriodID, _code.StartOffset.Value, organizationID);
					}
					else
					{
						deferredPeriod = _finPeriodRepository.GetOffsetPeriod(deferredPeriod.FinPeriodID, _code.Frequency.Value, organizationID);
					}
				}
				catch (PXFinPeriodException ex)
				{
					throw new PXException(ex, Messages.NoFinPeriod, _code.DeferredCodeID);
				}

				lineCounter++;

				DateTime recognitionDate = GetRecognitionDate(
					deferredPeriod.FinPeriodID,
					minimumDate: isFlexibleDeferralCode ? termStartDate.Value : documentDate,
					maximumDate: isFlexibleDeferralCode ? termEndDate : null,
					organizationID: organizationID);

				FinPeriod finPeriod = _finPeriodRepository.FindFinPeriodByDate(recognitionDate, organizationID);

				DRScheduleTran deferralTransaction = new DRScheduleTran
				{
					BranchID = scheduleDetail.BranchID,
					AccountID = scheduleDetail.AccountID,
					SubID = scheduleDetail.SubID,
					RecDate = recognitionDate,
					FinPeriodID = finPeriod?.FinPeriodID,
					TranPeriodID = finPeriod?.MasterFinPeriodID,
					LineNbr = lineCounter,
					DetailLineNbr = scheduleDetail.DetailLineNbr,
					ScheduleID = scheduleDetail.ScheduleID,
					ComponentID = scheduleDetail.ComponentID,
					Status = GetStatus(),
                };

				deferredList.Add(deferralTransaction);
			}

			SetAmounts(deferredList, defAmount, deferralSchedule.DocDate, termStartDate, termEndDate, organizationID);

			if (DeferredMethodType.RequiresTerms(_code) &&
				_code.RecognizeInPastPeriods != true)
			{
				// Adjust recognition dates and financial periods 
				// that are in the past relative to the document date.
				// -
				foreach (DRScheduleTran transaction in deferredList
					.Where(transaction => transaction.RecDate < documentDate))
				{
					transaction.RecDate = documentDate;
					transaction.FinPeriodID = documentFinPeriod?.FinPeriodID;
					transaction.TranPeriodID = documentFinPeriod?.MasterFinPeriodID;
				}
			}

			list.AddRange(deferredList);
			
			return list;
		}
	    protected virtual string GetStatus()
	    {
	        return _code.Method == DeferredMethodType.CashReceipt
	            ? DRScheduleTranStatus.Projected
	            : DRScheduleTranStatus.Open;
	    }

        /// <summary>
        /// If applicable, creates a single related transaction for all original posted transactions
        /// whose recognition date is earlier than (or equal to) the current document date.
        /// Does not set any amounts.
        /// </summary>
        /// <param name="transactionList">
        /// Transaction list where the new transaction will be put (if created).
        /// </param>
        /// <param name="lineCounter">
        /// Transaction line counter. Will be incremented if any transactions are created by this procedure.
        /// </param>
        private void AddRelatedTransactionForPostedBeforeDocumentDate(
			IList<DRScheduleTran> transactionList,
			DRScheduleDetail relatedScheduleDetail,
			IEnumerable<DRScheduleTran> originalPostedTransactions,
			int? branchID,
			ref short lineCounter)
		{
			IEnumerable<DRScheduleTran> originalTransactionsPostedBeforeDocumentDate =
				originalPostedTransactions.Where(transaction => transaction.RecDate <= relatedScheduleDetail.DocDate);

			if (originalTransactionsPostedBeforeDocumentDate.Any())
			{
				++lineCounter;

				DRScheduleTran relatedTransaction = new DRScheduleTran
				{
					BranchID = branchID,
					AccountID = relatedScheduleDetail.AccountID,
					SubID = relatedScheduleDetail.SubID,
					RecDate = relatedScheduleDetail.DocDate,
					FinPeriodID = relatedScheduleDetail.FinPeriodID,
					TranPeriodID = relatedScheduleDetail.TranPeriodID,
					LineNbr = lineCounter,
					DetailLineNbr = relatedScheduleDetail.DetailLineNbr,
					ScheduleID = relatedScheduleDetail.ScheduleID,
					ComponentID = relatedScheduleDetail.ComponentID,
					Status = DRScheduleTranStatus.Open
				};

				transactionList.Add(relatedTransaction);
			}
		}

		/// <summary>
		/// Adds a related transaction for every original transaction
		/// in <paramref name="originalTransactions"/> using information
		/// from the provided related <see cref="DRScheduleDetail"/>.
		/// Does not set any transaction amounts.
		/// </summary>
		/// <param name="transactionList">
		/// Transaction list where the new transaction will be put (if created).
		/// </param>
		/// <param name="lineCounter">
		/// Transaction line counter. Will be incremented if any transactions are created by this procedure.
		/// </param>
		private void AddRelatedTransactions(
			IList<DRScheduleTran> transactionList,
			DRScheduleDetail relatedScheduleDetail,
			IEnumerable<DRScheduleTran> originalTransactions,
			int? branchID,
			ref short lineCounter)
		{
			int? organizationID = PXAccess.GetParentOrganizationID(branchID);

			foreach (DRScheduleTran originalTransaction in originalTransactions)
			{
				++lineCounter;

				DRScheduleTran relatedTransaction = new DRScheduleTran
				{
					BranchID = branchID,
					AccountID = relatedScheduleDetail.AccountID,
					SubID = relatedScheduleDetail.SubID,
					LineNbr = lineCounter,
					DetailLineNbr = relatedScheduleDetail.DetailLineNbr,
					ScheduleID = relatedScheduleDetail.ScheduleID,
					ComponentID = relatedScheduleDetail.ComponentID,
					Status = DRScheduleTranStatus.Open
				};

				var maxDate = originalTransaction.RecDate.Value < relatedScheduleDetail.DocDate.Value
					? relatedScheduleDetail.DocDate
					: originalTransaction.RecDate;

				relatedTransaction.RecDate = maxDate;

				var maxPeriod = string.CompareOrdinal(originalTransaction.FinPeriodID, relatedScheduleDetail.FinPeriodID) < 0
					? relatedScheduleDetail.FinPeriodID
					: originalTransaction.FinPeriodID;

				FinPeriod maxFinPeriod = _finPeriodRepository.GetByID(maxPeriod, organizationID);

				relatedTransaction.FinPeriodID = maxFinPeriod.FinPeriodID;
				relatedTransaction.TranPeriodID = maxFinPeriod.MasterFinPeriodID;

				transactionList.Add(relatedTransaction);
			}
		}

		/// <summary>
		/// During the related transaction generation, checks that the original open transaction
		/// collection doesn't contain any non-open transactions.
		/// </summary>
		private static void ValidateOpenTransactions(IEnumerable<DRScheduleTran> originalOpenTransactions)
		{
			if (originalOpenTransactions == null) return;

			if (originalOpenTransactions.Any(transaction => 
				transaction.Status != DRScheduleTranStatus.Open &&
				transaction.Status != DRScheduleTranStatus.Projected))
			{
				throw new PXArgumentException(
					nameof(originalOpenTransactions),
					Messages.CollectionContainsPostedTransactions);
			}
		}

		/// <summary>
		/// During the related transaction generation, checks that the original posted transaction
		/// collection doesn't contain any non-posted transactions.
		/// </summary>
		private static void ValidatePostedTransactions(IEnumerable<DRScheduleTran> originalPostedTransactions)
		{
			if (originalPostedTransactions == null) return;

			if (originalPostedTransactions.Any(transaction => transaction.Status != DRScheduleTranStatus.Posted))
			{
				throw new PXArgumentException(
					nameof(originalPostedTransactions),
					Messages.CollectionContainsNonPostedTransactions);
			}
		}

		/// <summary>
		/// Generates the related transactions given the list of original 
		/// </summary>
		/// <param name="relatedScheduleDetail">
		/// The schedule detail
		/// to which the related transactions will pertain.
		/// </param>
		/// <param name="originalOpenTransactions">
		/// Original transactions in the Open (or Projected) status.
		/// </param>
		/// <param name="originalPostedTransactions">
		/// Original transactions in the Posted status.
		/// </param>
		/// <param name="amountToDistributeForUnposted">
		/// Amount to distribute among the related transactions that are
		/// created for original Open transactions.
		/// </param>
		/// <param name="amountToDistributeForPosted">
		/// Amount to distribute among the related transactions that are
		/// created for original Posted transactions.
		/// </param>
		/// <param name="branchID">
		/// Branch ID for the related transactions.
		/// </param>
		/// <returns></returns>
		public virtual IList<DRScheduleTran> GenerateRelatedTransactions(
			DRScheduleDetail relatedScheduleDetail,
			IEnumerable<DRScheduleTran> originalOpenTransactions,
			IEnumerable<DRScheduleTran> originalPostedTransactions,
			decimal amountToDistributeForUnposted,
			decimal amountToDistributeForPosted,
			int? branchID)
		{
			ValidateOpenTransactions(originalOpenTransactions);
			ValidatePostedTransactions(originalPostedTransactions);

			List<DRScheduleTran> transactionList = new List<DRScheduleTran>();

			short lineCounter = 0;
			int transactionsAddedDuringPreviousStep;

			// Handle posted transactions
			// -
			if (originalPostedTransactions != null && originalPostedTransactions.Any())
			{
				decimal originalPostedTransactionsTotal =
					originalPostedTransactions.Sum(transaction => transaction.Amount ?? 0);

				AddRelatedTransactionForPostedBeforeDocumentDate(
					transactionList,
					relatedScheduleDetail,
					originalPostedTransactions,
					branchID,
					ref lineCounter);

				transactionsAddedDuringPreviousStep = lineCounter;

				decimal originalPostedBeforeDocumentDateSum = originalPostedTransactions
					.Where(transaction => transaction.RecDate <= relatedScheduleDetail.DocDate)
					.Sum(transaction => transaction.Amount ?? 0);

				decimal multiplier = originalPostedTransactionsTotal == 0 && amountToDistributeForPosted == 0 ? 1 : amountToDistributeForPosted / originalPostedTransactionsTotal;

				// Amount to be distributed across other transactions that
				// are related to the original posted transactions.
				// -
				decimal residualPostedAmount = amountToDistributeForPosted;

				if (transactionList.Any())
				{
					transactionList[0].Amount = multiplier * originalPostedBeforeDocumentDateSum;
					residualPostedAmount -= transactionList[0].Amount ?? 0;
				}

				IEnumerable<DRScheduleTran> originalPostedAfterDocumentDate =
					originalPostedTransactions.Where(transaction => transaction.RecDate > relatedScheduleDetail.DocDate);

				AddRelatedTransactions(
					transactionList,
					relatedScheduleDetail,
					originalPostedAfterDocumentDate,
					branchID,
					ref lineCounter);
				
				// Set amounts for related transactions
				// -
				decimal relatedTransactionTotal = 0;
				
				if (originalPostedAfterDocumentDate.Any())
				{
					originalPostedAfterDocumentDate.SkipLast(1).ForEach((originalTransaction, i) =>
					{
						decimal rawTransactionAmount = multiplier * originalTransaction.Amount ?? 0;
						transactionList[transactionsAddedDuringPreviousStep + i].Amount = _roundingFunction(rawTransactionAmount);

						relatedTransactionTotal += transactionList[transactionsAddedDuringPreviousStep + i].Amount ?? 0;
					});

					transactionList[transactionList.Count - 1].Amount = residualPostedAmount - relatedTransactionTotal;
				}
			}

			transactionsAddedDuringPreviousStep = lineCounter;

			// Handle open transactions
			// -
			if (originalOpenTransactions != null && originalOpenTransactions.Any())
			{
				decimal originalOpenTransactionsTotal = 
					originalOpenTransactions.Sum(transaction => transaction.Amount ?? 0);

				AddRelatedTransactions(
					transactionList,
					relatedScheduleDetail,
					originalOpenTransactions,
					branchID,
					ref lineCounter);

				// Set amounts for related transactions
				// -
				decimal multiplier = originalOpenTransactionsTotal == 0 && amountToDistributeForUnposted == 0 ? 1 : amountToDistributeForUnposted / originalOpenTransactionsTotal;
				decimal relatedTransactionTotal = 0;

				originalOpenTransactions.SkipLast(1).ForEach((originalTransaction, i) =>
				{
					decimal rawTransactionAmount = multiplier * originalTransaction.Amount.Value;
					transactionList[transactionsAddedDuringPreviousStep + i].Amount = _roundingFunction(rawTransactionAmount);

					relatedTransactionTotal += transactionList[transactionsAddedDuringPreviousStep + i].Amount ?? 0;
				});

				transactionList[transactionList.Count - 1].Amount = amountToDistributeForUnposted - relatedTransactionTotal;
			}
			else if (amountToDistributeForUnposted > 0)
			{
				++lineCounter;

				DRScheduleTran deferralTransaction = new DRScheduleTran
				{
					Amount = amountToDistributeForUnposted,
					BranchID = branchID,
					AccountID = relatedScheduleDetail.AccountID,
					SubID = relatedScheduleDetail.SubID,
					RecDate = relatedScheduleDetail.DocDate,
					FinPeriodID = relatedScheduleDetail.FinPeriodID,
					TranPeriodID = relatedScheduleDetail.TranPeriodID,
					LineNbr = lineCounter,
					DetailLineNbr = relatedScheduleDetail.DetailLineNbr,
					ScheduleID = relatedScheduleDetail.ScheduleID,
					ComponentID = relatedScheduleDetail.ComponentID,
					Status = DRScheduleTranStatus.Open
				};

				transactionList.Add(deferralTransaction);
			}

			return transactionList;
		}

		/// <summary>
		/// Checks the presence and consistency of deferral term start / end dates,
		/// as well as ensures that the document date is no later than the Term End Date 
		/// in case recognizing in past periods is forbidden. 
		/// </summary>
		/// <param name="deferralSchedule">
		/// Deferral schedule from which the document date will be taken.
		/// </param>
		protected virtual void ValidateTerms(DRSchedule deferralSchedule)
		{
			if (!DeferredMethodType.RequiresTerms(_code)) return;

			if (!deferralSchedule.TermStartDate.HasValue ||
				!deferralSchedule.TermEndDate.HasValue)
			{
				throw new PXException(Messages.CannotGenerateTransactionsWithoutTerms);
			}

			if (deferralSchedule.TermStartDate > deferralSchedule.TermEndDate)
			{
				throw new PXException(
					Messages.TermCantBeNegative, 
					deferralSchedule.TermEndDate, 
					deferralSchedule.TermStartDate);
			}
		}

		protected virtual void ValidateTerms(DRScheduleDetail deferralDetail)
		{
			if (!DeferredMethodType.RequiresTerms(_code)) return;

			if (!deferralDetail.TermStartDate.HasValue ||
				!deferralDetail.TermEndDate.HasValue)
			{
				throw new PXException(Messages.CannotGenerateTransactionsWithoutTerms);
			}

			if (deferralDetail.TermStartDate > deferralDetail.TermEndDate)
			{
				throw new PXException(
					Messages.TermCantBeNegative,
					deferralDetail.TermEndDate,
					deferralDetail.TermStartDate);
			}
		}

		protected virtual int CalcOccurrences(DateTime startDate, DateTime endDate, int? organizationID)
		{
			return _finPeriodRepository
				.PeriodsBetweenInclusive(startDate, endDate, organizationID)
				.Where((_, periodIndex) => periodIndex % _code.Frequency == 0)
				.Count();
		}

		/// <summary>
		/// Returns the appropriate recognition date in a given financial period,
		/// taking into account the <see cref="DRScheduleOption"/> settings of the
		/// deferral code, as well as the absolute recognition date boundaries provided
		/// as arguments to this method.
		/// </summary>
		/// <param name="finPeriod">The financial period in which recognition must happen.</param>
		/// <param name="minimumDate">The earliest date where recognition is allowed.</param>
		/// <param name="maximumDate">The latest date where recognition is allowed.</param>
		/// <returns></returns>
		protected DateTime GetRecognitionDate(string finPeriod, DateTime minimumDate, DateTime? maximumDate, int? organizationID)
		{
			DateTime date = minimumDate;

			switch (_code.ScheduleOption)
			{
				case DRScheduleOption.ScheduleOptionStart:
					date = _finPeriodRepository.PeriodStartDate(finPeriod, organizationID);
					break;

				case DRScheduleOption.ScheduleOptionEnd:
					date = _finPeriodRepository.PeriodEndDate(finPeriod, organizationID);
					break;

				case DRScheduleOption.ScheduleOptionFixedDate:
					DateTime startDate = _finPeriodRepository.PeriodStartDate(finPeriod, organizationID);
					DateTime endDate = _finPeriodRepository.PeriodEndDate(finPeriod, organizationID);

					if (_code.FixedDay.Value <= startDate.Day)
						date = startDate;
					else if (_code.FixedDay.Value >= endDate.Day)
						date = endDate;
					else
						date = new DateTime(startDate.Year, startDate.Month, _code.FixedDay.Value);
					break;
			}

			if (date < minimumDate && _code.StartOffset >= 0)
				return minimumDate;
			else if (date > maximumDate)
				return maximumDate.Value;

			return date;
		}

	    protected virtual void SetAmounts(IList<DRScheduleTran> deferredTransactions, decimal deferredAmount, DateTime? docDate, DateTime? termStartDate, DateTime? termEndDate, int? organizationID)
		{
			switch (_code.Method)
			{
				case DeferredMethodType.CashReceipt:
				case DeferredMethodType.EvenPeriods:
					SetAmountsEvenPeriods(deferredTransactions, deferredAmount);
					break;
				case DeferredMethodType.ProrateDays:
					SetAmountsProrateDays(deferredTransactions, deferredAmount, docDate.Value, organizationID);
					break;
				case DeferredMethodType.ExactDays:
					SetAmountsExactDays(deferredTransactions, deferredAmount, organizationID);
					break;
				case DeferredMethodType.FlexibleProrateDays:
					SetAmountsFlexibleProrateByDays(deferredTransactions, deferredAmount, termStartDate.Value, termEndDate.Value, organizationID);
					break;
				case DeferredMethodType.FlexibleExactDays:
					SetAmountsFlexibleByDays(deferredTransactions, deferredAmount, termStartDate.Value, termEndDate.Value, organizationID);
					break;
			}
		}

	    protected void SetAmountsEvenPeriods(IList<DRScheduleTran> deferredTransactions, decimal deferredAmount)
		{
			if (deferredTransactions.Count > 0)
			{
				decimal amtRaw = deferredAmount / deferredTransactions.Count;
				decimal amt = _roundingFunction(amtRaw);
				decimal recAmt = 0;
				for (int i = 0; i < deferredTransactions.Count - 1; i++)
				{
					deferredTransactions[i].Amount = amt;
					recAmt += amt;
				}

				deferredTransactions[deferredTransactions.Count - 1].Amount = deferredAmount - recAmt;
			}
		}

		private void SetAmountsProrateDays(
			IList<DRScheduleTran> deferredTransactions, 
			decimal deferredAmount, 
			DateTime documentDate,
			int? organizationID)
		{
			if (deferredTransactions.Count > 0)
			{
				if (deferredTransactions.Count == 1)
				{
					deferredTransactions[0].Amount = deferredAmount;
				}
				else
				{
					string currentPeriod = _finPeriodRepository.FindFinPeriodByDate(documentDate, organizationID)?.FinPeriodID;
					DateTime startDateOfCurrentPeriod = _finPeriodRepository.PeriodStartDate(currentPeriod, organizationID);

					if (_code.StartOffset > 0 || startDateOfCurrentPeriod == documentDate)
					{
						SetAmountsEvenPeriods(deferredTransactions, deferredAmount);
					}
					else
					{
						// Returns the last day of the month with time 12:00 AM!
						// -
						DateTime endDateOfCurrentPeriod = _finPeriodRepository.PeriodEndDate(currentPeriod, organizationID);
						TimeSpan spanOfCurrentPeriod = endDateOfCurrentPeriod.Subtract(startDateOfCurrentPeriod);

						// One is added to the time because 30.12.2009 12:00 AM minus 01.12.2009 12:00 AM will give 29.
						// -
						int daysInPeriod = spanOfCurrentPeriod.Days + 1;

						TimeSpan span = endDateOfCurrentPeriod.Subtract(documentDate);

						// +1?
						// -
						int daysInFirstPeriod = span.Days;

						decimal amtRaw = deferredAmount / (deferredTransactions.Count - 1);
						decimal firstRaw = amtRaw * daysInFirstPeriod / daysInPeriod;

						decimal amt = _roundingFunction(amtRaw);
						decimal firstAmt = _roundingFunction(firstRaw);

						deferredTransactions[0].Amount = firstAmt;

						decimal recAmt = firstAmt;
						for (int i = 1; i < deferredTransactions.Count - 1; i++)
						{
							deferredTransactions[i].Amount = amt;
							recAmt += amt;
						}

						deferredTransactions[deferredTransactions.Count - 1].Amount = deferredAmount - recAmt;
					}
				}
			}
		}

		private void SetAmountsExactDays(IList<DRScheduleTran> deferredTransactions, decimal deferredAmount, int? organizationID)
		{
			int totalDays = 0;
			foreach (DRScheduleTran row in deferredTransactions)
			{
				DateTime endDateOfPeriod = _finPeriodRepository.PeriodEndDate(row.FinPeriodID, organizationID);
				DateTime startDateOfPeriod = _finPeriodRepository.PeriodStartDate(row.FinPeriodID, organizationID);
				TimeSpan spanOfPeriod = endDateOfPeriod.Subtract(startDateOfPeriod);
				totalDays += spanOfPeriod.Days + 1;
			}

			decimal amountPerDay = deferredAmount / totalDays;

			decimal recAmt = 0;
			for (int i = 0; i < deferredTransactions.Count - 1; i++)
			{
				DateTime endDateOfPeriod = _finPeriodRepository.PeriodEndDate(deferredTransactions[i].FinPeriodID, organizationID);
				DateTime startDateOfPeriod = _finPeriodRepository.PeriodStartDate(deferredTransactions[i].FinPeriodID, organizationID);
				TimeSpan spanOfPeriod = endDateOfPeriod.Subtract(startDateOfPeriod);

				decimal amountRaw = (spanOfPeriod.Days + 1) * amountPerDay;
				decimal amount = _roundingFunction(amountRaw);

				deferredTransactions[i].Amount = amount;
				recAmt += amount;
			}

			deferredTransactions[deferredTransactions.Count - 1].Amount = deferredAmount - recAmt;
		}

		private void SetAmountsFlexibleByDays(
			IList<DRScheduleTran> deferredTransactions,
			decimal deferredAmount,
			DateTime startDate,
			DateTime endDate,
			int? organizationID)
		{
			if (deferredTransactions.Any() == false)
				return;

			var days = (int)(endDate - startDate).TotalDays + 1;
			var dailyRate = deferredAmount / days;

			if(deferredTransactions.Count > 1)
			{
				var firstTran = deferredTransactions[0];
				var secondTran = deferredTransactions[1];
				int daysUnderFirstTran = (int)_finPeriodRepository.PeriodStartDate(secondTran.FinPeriodID, organizationID).Subtract(startDate).TotalDays;
				firstTran.Amount = _roundingFunction(daysUnderFirstTran * dailyRate);
			}

			var transactionsButFirst = deferredTransactions.Skip(1);
			foreach (var item in transactionsButFirst.Zip(transactionsButFirst.Skip(1), (tran, next) => new { Tran = tran, NextTran = next }))
			{
				var start = _finPeriodRepository.PeriodStartDate(item.Tran.FinPeriodID, organizationID);
				var end = _finPeriodRepository.PeriodStartDate(item.NextTran.FinPeriodID, organizationID);
				int daysCovered = (int)end.Subtract(start).TotalDays;

				item.Tran.Amount = _roundingFunction(daysCovered * dailyRate);
			}

			var lastTran = deferredTransactions.Last();
			lastTran.Amount = 0; //to ensure correct Sum below
			lastTran.Amount = deferredAmount - deferredTransactions.Sum(t => t.Amount);
		}

		private void SetAmountsFlexibleProrateByDays(
			IList<DRScheduleTran> deferredTransactions,
			decimal deferredAmount,
			DateTime startDate,
			DateTime endDate,
			int? organizationID)
		{
			var coveredPeriods = _finPeriodRepository.PeriodsBetweenInclusive(startDate, endDate, organizationID);

			var firstPeriodPortion = 1.0m - PeriodProportionByDays(startDate, organizationID, shift: -1);
			var lastPeriodPortion = PeriodProportionByDays(endDate, organizationID);

			var periodsTotal = coveredPeriods.Count() == 1 ? 1
				: coveredPeriods.Count() - 2 + firstPeriodPortion + lastPeriodPortion;

			var periodlyRate = deferredAmount / periodsTotal;

			foreach (var item in deferredTransactions.Zip(deferredTransactions.Skip(1), (tran, next) => new { Tran = tran, NextTran = next }))
			{
				var periodsCount = (decimal)coveredPeriods
					.SkipWhile(p => p.FinPeriodID != item.Tran.FinPeriodID)
					.TakeWhile(p => p.FinPeriodID != item.NextTran.FinPeriodID)
					.Count();

				if (item.Tran.FinPeriodID == coveredPeriods.First().FinPeriodID)
				{
					periodsCount += firstPeriodPortion - 1.0m;
				}

				item.Tran.Amount = _roundingFunction(periodlyRate * periodsCount);
			}

			var lastTran = deferredTransactions.Last();
			lastTran.Amount = 0;
			var lastAmount = deferredAmount - deferredTransactions.Sum(t => t.Amount);
			lastTran.Amount = _roundingFunction(lastAmount.Value);
		}

		#region Periods and Dates

		private decimal PeriodProportionByDays(DateTime date, int? organizationID, int shift = 0)
		{
			FinPeriod finPeriod = _finPeriodRepository.FindFinPeriodByDate(date, organizationID);
            if (finPeriod == null)
                throw new PXException(Messages.TermStartDateOrTermEndDatedoNotExist, _code.DeferredCodeID);
			var start = _finPeriodRepository.PeriodStartDate(finPeriod.FinPeriodID, organizationID);
			var end = _finPeriodRepository.PeriodEndDate(finPeriod.FinPeriodID, organizationID);

			int days = (int)end.Subtract(start).TotalDays + 1;
			return (decimal)(date.Subtract(start).TotalDays + 1 + shift) / days;
		}

		#endregion
	}
}
