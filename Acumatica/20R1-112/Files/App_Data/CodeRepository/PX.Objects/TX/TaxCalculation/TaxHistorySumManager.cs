using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;

namespace PX.Objects.TX
{
	public class TaxHistorySumManager
	{
		private const int StartRow = 0;
		private const int TotalRows = 1;

		public static void UpdateTaxHistorySums(PXGraph graph, RoundingManager rmanager, string taxPeriodId, int? revisionId, int? organizationID, int? branchID, Func<TaxReportLine, bool> ShowTaxReportLine = null)
		{
			if (!rmanager.IsRequireRounding)
				return;

			PXCache cache = graph.Caches[typeof(TaxHistory)];

			using (new PXReadBranchRestrictedScope(organizationID.SingleToArray(), branchID.SingleToArrayOrNull(), requireAccessForAllSpecified:true))
			{
				PXResultset<TaxHistory> lines = GetTaxHistoryLines(graph, rmanager.CurrentVendor.BAccountID, taxPeriodId, revisionId);

				if (lines.Count == 0)
					return;

				if (organizationID == null)
				{
					Branch branch = PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.SelectSingleBound(graph, null, branchID);
					organizationID = branch?.OrganizationID;
				}

				TaxPeriod period = TaxYearMaint.GetTaxPeriodByKey(graph, organizationID, rmanager.CurrentVendor.BAccountID,
					taxPeriodId);

				Company company = PXSelect<Company>.Select(graph);

				PXResult<Currency, CurrencyRateByDate> curyWithRateSet = GetCurrencyAndRateByDate(graph, rmanager.CurrentVendor, company, period);
				Currency currency = curyWithRateSet;
				CurrencyRateByDate rateByDate = currency.CuryID != company.BaseCuryID ? curyWithRateSet : null;

				TaxBucketsCalculation taxBucketsAggregatesCalc = 
					new TaxBucketsCalculation(TaxReportLineType.TaxAmount, graph, rmanager, currency, rateByDate, ShowTaxReportLine);

				TaxBucketsCalculation taxableBucketsAggregatesCalc = 
					new TaxBucketsCalculation(TaxReportLineType.TaxableAmount, graph, rmanager, currency, rateByDate, ShowTaxReportLine);

				taxBucketsAggregatesCalc.CalculateTaxBuckets(lines);
				taxableBucketsAggregatesCalc.CalculateTaxBuckets(lines);
			}

			cache.Persist(PXDBOperation.Insert);
			cache.Persisted(isAborted: false);
		}

		private static PXResultset<TaxHistory> GetTaxHistoryLines(PXGraph graph, int? curVendorBaccountID, string taxPeriodId, int? revisionId)
		{
			PXResultset<TaxHistory> lines =
				PXSelectJoinGroupBy<TaxHistory,
					InnerJoin<
						TaxReportLine, 
							On<TaxReportLine.vendorID, Equal<TaxHistory.vendorID>,
							And<TaxReportLine.lineNbr, Equal<TaxHistory.lineNbr>>>>,
				Where<
					TaxHistory.vendorID, Equal<Required<TaxHistory.vendorID>>,
						And<TaxHistory.taxPeriodID, Equal<Required<TaxHistory.taxPeriodID>>,
						And<TaxHistory.revisionID, Equal<Required<TaxHistory.revisionID>>>>>,
				Aggregate<
						GroupBy<TaxHistory.branchID, 
						GroupBy<TaxReportLine.lineNbr,
						GroupBy<TaxReportLine.netTax, 
						Sum<TaxHistory.reportFiledAmt>>>>>>
			.Select(graph, curVendorBaccountID, taxPeriodId, revisionId);

			return lines;
		}

		private static PXResult<Currency, CurrencyRateByDate> GetCurrencyAndRateByDate(PXGraph graph, Vendor curVendor, Company company, TaxPeriod period)
		{
			PXCache<CurrencyFilter> filterCache = graph.Caches<CurrencyFilter>();
			filterCache.Clear();
			filterCache.Insert(new CurrencyFilter
			{
				FromCuryID = curVendor.CuryID ?? company.BaseCuryID,
				ToCuryID = company.BaseCuryID
			});

			var curyWithRateSet = (PXResult<Currency, CurrencyRateByDate>)
				PXSelectJoin<Currency,
					LeftJoin<
						CurrencyRateByDate,
							On<CurrencyRateByDate.fromCuryID, Equal<Currency.curyID>,
								And<CurrencyRateByDate.toCuryID, Equal<Required<Company.baseCuryID>>,
								And<CurrencyRateByDate.curyRateType, Equal<Required<CurrencyRateByDate.curyRateType>>,
								And<CurrencyRateByDate.curyEffDate, LessEqual<Required<CurrencyRateByDate.curyEffDate>>,
								And<
									Where<
										  CurrencyRateByDate.nextEffDate, Greater<Required<CurrencyRateByDate.curyEffDate>>,
										Or<CurrencyRateByDate.nextEffDate, IsNull>>>>>>>>,
					Where<Currency.curyID, Equal<Required<Currency.curyID>>>>
				.SelectWindowed(graph, StartRow, TotalRows, company.BaseCuryID, curVendor.CuryRateTypeID, 
								period.EndDate, period.EndDate, curVendor.CuryID ?? company.BaseCuryID);

			return curyWithRateSet;
		}

		public static decimal RecalcCurrency(Currency cury, CurrencyRate rate, decimal value)
		{
			if (rate.CuryRate == null || rate.CuryRate == 0)
			{
				return 0;
			}

			decimal convertedValue = 0m;

			if (rate.CuryMultDiv == CuryMultDivType.Mult)
			{
				convertedValue = value * rate.CuryRate.Value;
			}
			else if (rate.CuryMultDiv == CuryMultDivType.Div)
			{
				convertedValue = value / rate.CuryRate.Value;
			}

			return Math.Round(convertedValue, cury.DecimalPlaces ?? 2, MidpointRounding.AwayFromZero);
		}

		public static PXResultset<TaxReportLine, TaxHistory> GetPreviewReport(PXGraph graph, Vendor vendor, 
																			  PXResultset<TaxReportLine, TaxHistory> records, 
																			  Func<TaxReportLine, bool> ShowTaxReportLine = null)
        {
            if (records.Count == 0)
                return records;

            const bool calcWithZones = true;
            int vendorbAccountID = vendor.BAccountID.Value;
            RoundingManager rmanager = new RoundingManager(vendor);

            Dictionary<int, List<int>> taxAggregatesDict = TaxReportMaint.AnalyseBuckets(graph,
                vendorbAccountID,
                TaxReportLineType.TaxAmount,
                calcWithZones,
                ShowTaxReportLine) ?? new Dictionary<int, List<int>>();

            Dictionary<int, List<int>> taxableAggregatesDict = TaxReportMaint.AnalyseBuckets(graph,
                vendorbAccountID,
                TaxReportLineType.TaxableAmount,
                calcWithZones,
                ShowTaxReportLine) ?? new Dictionary<int, List<int>>();

            var recordsByLineNumberTable = new Dictionary<int, PXResult<TaxReportLine, TaxHistory>>();

            foreach (PXResult<TaxReportLine, TaxHistory> record in records)
            {
                TaxReportLine taxLine = record;
				TaxHistory taxHistory = record;

				taxHistory.ReportUnfiledAmt = rmanager.Round(taxHistory.ReportUnfiledAmt);
                recordsByLineNumberTable[taxLine.LineNbr.Value] = record;
            }

			CalculateReportUnfiledAmtForAggregatedTaxLines(taxAggregatesDict, recordsByLineNumberTable);
			CalculateReportUnfiledAmtForAggregatedTaxLines(taxableAggregatesDict, recordsByLineNumberTable);
			return records;
        }

		private static void CalculateReportUnfiledAmtForAggregatedTaxLines(Dictionary<int, List<int>> aggregatesTable,
			Dictionary<int, PXResult<TaxReportLine, TaxHistory>> recordsByLineNumberTable)
		{
			if (aggregatesTable == null)
				return;

			foreach (KeyValuePair<int, List<int>> aggregateWithLines in aggregatesTable)
			{
				int aggregateLineNumber = aggregateWithLines.Key;
				List<int> componentLinesNumbers = aggregateWithLines.Value;
				
				if (!recordsByLineNumberTable.ContainsKey(aggregateLineNumber))
					continue;

				TaxHistory aggrTaxHistory = recordsByLineNumberTable[aggregateLineNumber];
				aggrTaxHistory.ReportUnfiledAmt = SumComponentLinesAmounts(componentLinesNumbers, recordsByLineNumberTable);
			}
		}

		private static decimal? SumComponentLinesAmounts(List<int> componentLinesNumbers, 
			Dictionary<int, PXResult<TaxReportLine, TaxHistory>> recordsByLineNumberTable)
		{
			decimal? sum = 0m;

			foreach (int line in componentLinesNumbers.Where(l => recordsByLineNumberTable.ContainsKey(l)))
			{
				PXResult<TaxReportLine, TaxHistory> currline = recordsByLineNumberTable[line];
				TaxReportLine taxLine = (TaxReportLine)currline;
				TaxHistory taxHistory = (TaxHistory)currline;

				if (taxHistory.ReportUnfiledAmt != null)
				{
					sum += taxLine.LineMult * taxHistory.ReportUnfiledAmt;
				}
			}

			return sum;
		}

		private class TaxBucketsCalculation
		{
			private const bool CalcWithZones = true;

			private readonly Dictionary<int, List<int>> aggregatesTable;
			private readonly Dictionary<int, List<int>> linesWithRelatedAggregatesTable;
			private readonly string taxType;
			private readonly RoundingManager roundingManager;
			private readonly PXGraph graph;
			private readonly Currency currency;
			private readonly CurrencyRateByDate rateByDate;

			private readonly Dictionary<int, TaxLinesWithHistoryPerBranch> netHistoryByBranchID = 
				new Dictionary<int, TaxLinesWithHistoryPerBranch>();

			private readonly Dictionary<int, TaxLinesWithRoundAmountsPerBranch> roundedNetAmtByBranchID = 
				new Dictionary<int, TaxLinesWithRoundAmountsPerBranch>();

			public TaxBucketsCalculation(string taxTypeForCalculation, PXGraph aGraph, RoundingManager rmanager, 
										 Currency aCurrency, CurrencyRateByDate curyRateByDate, 
										 Func<TaxReportLine, bool> showTaxReportLine)
			{
				taxType = taxTypeForCalculation;
				roundingManager = rmanager;
				graph = aGraph;
				currency = aCurrency;
				rateByDate = curyRateByDate;
				int curVendorBaccountID = roundingManager.CurrentVendor.BAccountID.Value;

				aggregatesTable = TaxReportMaint.AnalyseBuckets(graph, curVendorBaccountID, taxTypeForCalculation, CalcWithZones, showTaxReportLine);
				linesWithRelatedAggregatesTable = TransposeDictionary(aggregatesTable);
			}

			public void CalculateTaxBuckets(PXResultset<TaxHistory> taxLines)
			{
				if (aggregatesTable == null || linesWithRelatedAggregatesTable == null)
					return;

				netHistoryByBranchID.Clear();
				roundedNetAmtByBranchID.Clear();

				foreach (PXResult<TaxHistory, TaxReportLine> rec in taxLines)
				{
					TaxReportLine line = rec;
					TaxHistory record = rec;

					if (line.LineType != taxType || record.BranchID == null)
						continue;

					int branchID = record.BranchID.Value;
					int lineNbr = record.LineNbr.Value;

					if (aggregatesTable.ContainsKey(lineNbr))
					{
						if (!netHistoryByBranchID.ContainsKey(branchID))
						{
							netHistoryByBranchID[branchID] = new TaxLinesWithHistoryPerBranch();
						}

						netHistoryByBranchID[branchID][lineNbr] = rec;
					}
					else
					{
						decimal? roundedAmount = roundingManager.Round(record.ReportFiledAmt);
						ProcessTaxRecord(record, line, roundedAmount);
					}
				}

				foreach (KeyValuePair<int, TaxLinesWithRoundAmountsPerBranch> branchIdWithLinesAndNetAmountsTable in roundedNetAmtByBranchID)
				{
					TaxLinesWithHistoryPerBranch branchTaxLinesAndHistory = netHistoryByBranchID[branchIdWithLinesAndNetAmountsTable.Key];
					TaxLinesWithRoundAmountsPerBranch branchTaxLinesWithRoundedAmounts = branchIdWithLinesAndNetAmountsTable.Value;
					var aggregateLinesInBranch = aggregatesTable.Keys.Where(aggrLineNumber => branchTaxLinesAndHistory.ContainsTaxLine(aggrLineNumber));

					foreach (int aggregateLineNumber in aggregateLinesInBranch)
					{
						TaxHistory aggrLineRecord = branchTaxLinesAndHistory[aggregateLineNumber];
						TaxReportLine aggrLine = branchTaxLinesAndHistory[aggregateLineNumber];
						decimal? roundedAmount = branchTaxLinesWithRoundedAmounts[aggregateLineNumber];

						ProcessTaxRecord(aggrLineRecord, aggrLine, roundedAmount);
					}
				}
			}

			/// <summary>
			/// Transpose input dictionary. Each key in result was a value in initial dictionary and corresponding entry in result consists of the keys from initial dictionary 
			/// which lists contained that entry's key.
			/// </summary>
			/// <returns/>	
			private static Dictionary<int, List<int>> TransposeDictionary(Dictionary<int, List<int>> oldDict)
			{
				if (oldDict == null)
				{
					return null;
				}

				Dictionary<int, List<int>> newDict = oldDict.SelectMany(pair => pair.Value.Select(val => Tuple.Create(val, pair.Key)))
															.GroupBy(keySelector: tuple => tuple.Item1, elementSelector: tuple => tuple.Item2)
															.ToDictionary(group => group.Key, group => group.ToList());
				return newDict;
			}

			private void ProcessTaxRecord(TaxHistory record, TaxReportLine taxLine, decimal? roundedTaxAmount)
			{
				PXCache taxHistoryCache = graph.Caches[typeof(TaxHistory)];

				decimal? filedAmount = record.ReportFiledAmt;
				int lineNbr = record.LineNbr.Value;
				int branchID = record.BranchID.Value;

				if (filedAmount != roundedTaxAmount)
				{
					TaxHistory taxHistory = CreateDeltaHistory(record, roundedTaxAmount);
					taxHistoryCache.Insert(taxHistory);
				}

				if (!linesWithRelatedAggregatesTable.ContainsKey(lineNbr))
					return;

				List<int> relatedAggregateLineNumbers = linesWithRelatedAggregatesTable[lineNbr];

				foreach (int aggrLineNumber in relatedAggregateLineNumbers)
				{
					decimal amount = (roundedTaxAmount ?? 0m) * (taxLine.LineMult ?? 0m);
					AddTaxAmountToAggregateLine(branchID, aggrLineNumber, amount);
				}
			}

			private TaxHistory CreateDeltaHistory(TaxHistory original, decimal? roundedAmount)
			{
				TaxHistory delta = new TaxHistory
				{
					BranchID = original.BranchID,
					VendorID = original.VendorID,
					CuryID = original.CuryID,
					TaxID = string.Empty,
					TaxPeriodID = original.TaxPeriodID,
					LineNbr = original.LineNbr,
					RevisionID = original.RevisionID,
					ReportFiledAmt = roundedAmount - original.ReportFiledAmt,
				};

				if (rateByDate == null)
					delta.FiledAmt = delta.ReportFiledAmt;
				else
				{
					if (rateByDate.CuryRate == null)
						throw new PXException(CM.Messages.RateNotFound);

					delta.FiledAmt = RecalcCurrency(currency, rateByDate, delta.ReportFiledAmt.GetValueOrDefault());
				}

				if (delta.ReportFiledAmt > 0)
				{
					delta.AccountID = currency.RoundingLossAcctID;
					delta.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(graph, delta.BranchID, currency);
				}
				else
				{
					delta.AccountID = currency.RoundingGainAcctID;
					delta.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(graph, delta.BranchID, currency);
				}

				if (delta.AccountID == null || delta.SubID == null)
					throw new PXException(Messages.CannotPrepareReportDefineRoundingAccounts);

				return delta;
			}

			private void AddTaxAmountToAggregateLine(int branchID, int aggrLineNumber, decimal amount)
			{
				if (!roundedNetAmtByBranchID.ContainsKey(branchID))
				{
					roundedNetAmtByBranchID[branchID] = new TaxLinesWithRoundAmountsPerBranch();
				}

				decimal oldAmount = roundedNetAmtByBranchID[branchID].TryGetTaxAmount(aggrLineNumber) ?? 0m;
				roundedNetAmtByBranchID[branchID][aggrLineNumber] = oldAmount + amount;
			}


			/// <summary>
			/// A class which contains a table with tax lines and history per branch.
			/// </summary>
			private class TaxLinesWithHistoryPerBranch
			{
				private readonly Dictionary<int, PXResult<TaxHistory, TaxReportLine>> taxLines =
					new Dictionary<int, PXResult<TaxHistory, TaxReportLine>>();

				public PXResult<TaxHistory, TaxReportLine> this[int lineNumber]
				{
					get
					{
						PXResult<TaxHistory, TaxReportLine> result;
						return taxLines.TryGetValue(lineNumber, out result) ? result : null;
					}
					set
					{
						taxLines[lineNumber] = value;
					}
				}

				public bool ContainsTaxLine(int lineNumber) => taxLines.ContainsKey(lineNumber);
			}


			/// <summary>
			/// A class which represents a table of tax lines with corresponding rounded tax amounts per branch.
			/// </summary>
			private class TaxLinesWithRoundAmountsPerBranch
			{
				private readonly Dictionary<int, decimal> taxLinesWithAmounts = new Dictionary<int, decimal>();

				public decimal this[int lineNumber]
				{
					get
					{
						return taxLinesWithAmounts[lineNumber];
					}
					set
					{
						taxLinesWithAmounts[lineNumber] = value;
					}
				}

				/// <summary>
				/// Tries to get rounded tax amount by tax line number without raising exception.
				/// </summary>
				/// <param name="lineNumber">The line number.</param>
				/// <returns/>
				public decimal? TryGetTaxAmount(int lineNumber)
				{
					decimal amount;
					return taxLinesWithAmounts.TryGetValue(lineNumber, out amount) ? amount : (decimal?)null;
				}
			}
		}
	}
}

