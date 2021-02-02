using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Extensions;

using FABookHist = PX.Objects.FA.Overrides.AssetProcess.FABookHist;
using PX.Objects.Common;
using PX.Objects.FA.DepreciationMethods;
using PX.Objects.FA.DepreciationMethods.Parameters;
using static PX.Objects.FA.DepreciationMethods.DepreciationMethodBase;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.FA
{
	public class DepreciationCalculation : PXGraph<DepreciationCalculation, FADepreciationMethod>
	{
		protected int _Precision;

		protected FADestination _Destination = FADestination.Calculated;

		public enum FADestination
		{
			Calculated,
			Tax179,
			Bonus
		}
		
		#region Selects Declaration

		public PXSelect<FADepreciationMethod> DepreciationMethod;
		public PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Current<FADepreciationMethod.methodID>>>> 	DepreciationMethodLines;
		public PXSetup<FASetup> FASetup;
		public PXSelect<FABookHistory> RawHistory;
		public PXSelect<FABookHist> BookHistory;
		public PXSelect<FABookBalance> BookBalance;
		public PXSelect<FADetails> FADetails; // to allow persisting
		
		
		#endregion

		#region Constructor
		public DepreciationCalculation() 
		{
			FASetup setup = FASetup.Current;
			PXCache cache = DepreciationMethodLines.Cache;
			cache.AllowDelete = false;
			cache.AllowInsert = false;			
			PXCache methodCache = DepreciationMethod.Cache;
			PXDBCurrencyAttribute.SetBaseCalc<FADepreciationMethod.totalPercents> (methodCache, null, true);

			Currency cury = PXSelectJoin<Currency, InnerJoin<GL.Company, On<GL.Company.baseCuryID, Equal<Currency.curyID>>>>.Select(this);
			_Precision = cury.DecimalPlaces ?? 4;

			Params = new DeprCalcParameters();
		}

		public override void Clear()
		{
			PXDBDecimalAttribute.EnsurePrecision(Caches[typeof(FABookHist)]);
			base.Clear();
		}

		public decimal Round(decimal value)
		{
			return Math.Round(value, _Precision, MidpointRounding.AwayFromZero);
		}
		#endregion
		
		#region Functions

		private Dictionary<string, FABookHist> histdict;

		public virtual bool UseAcceleratedDepreciation(FixedAsset cls, FADepreciationMethod method)
		{
			return (cls.AcceleratedDepreciation == true && method != null && method.IsPureStraightLine) ||
				(method != null && method.IsTableMethod != true && (method.DepreciationMethod == FADepreciationMethod.depreciationMethod.RemainingValue 
																	|| method.DepreciationMethod == FADepreciationMethod.depreciationMethod.RemainingValueByPeriodLength));
		}

		[InjectDependency]
		public IFABookPeriodRepository FABookPeriodRepository { get; set; }
		[InjectDependency]
		public IFABookPeriodUtils FABookPeriodUtils { get; set; }

		public virtual void CalculateDepreciationAddition(FixedAsset cls, FABookBalance bookbal, FADepreciationMethod method, FABookHistory next)
		{
			string bookDeprFromPeriod = bookbal.DeprFromPeriod;
			string additionDeprFromPeriod = FABookPeriodUtils.PeriodPlusPeriodsCount(next.FinPeriodID, -(next.YtdReversed ?? 0), next.BookID, next.AssetID);

			DateTime? additionEndDate = null;
			if (UseAcceleratedDepreciation(cls, method) && string.CompareOrdinal(additionDeprFromPeriod, bookDeprFromPeriod) > 0)
			{
				Params.Fill(this, bookbal, null, method);
				additionEndDate = Params.RecoveryEndDate;
			}

			bookbal.DeprFromPeriod = additionDeprFromPeriod;
			bookbal.DeprFromDate = (additionDeprFromPeriod == bookDeprFromPeriod) ? bookbal.DeprFromDate : FABookPeriodUtils.GetFABookPeriodStartDate(additionDeprFromPeriod, bookbal.BookID, bookbal.AssetID);
			bookbal.DeprToPeriod = FABookPeriodUtils.PeriodPlusPeriodsCount(bookbal.DeprToPeriod, -(bookbal.YtdSuspended ?? 0), bookbal.BookID, bookbal.AssetID);

			if (bookbal.DeprToDate != null)
			{
				FABookPeriod per1 = FABookPeriodRepository.FindFABookPeriodOfDate(bookbal.DeprToDate, bookbal.BookID, bookbal.AssetID);
				FABookPeriod per2 = FABookPeriodRepository.FindOrganizationFABookPeriodByID(bookbal.DeprToPeriod, bookbal.BookID, bookbal.AssetID);

				if (Equals(bookbal.DeprToDate, ((DateTime)per1.EndDate).AddDays(-1)))
				{
					bookbal.DeprToDate = ((DateTime)per2.EndDate).AddDays(-1);
				}
				else
				{
					int days = ((TimeSpan)(bookbal.DeprToDate - per1.StartDate)).Days;
					bookbal.DeprToDate = days < ((TimeSpan)(per2.EndDate - per2.StartDate)).Days ? ((DateTime)per2.StartDate).AddDays(days) : per2.EndDate;
				}

				FABookBalance bookbal2 = PXCache<FABookBalance>.CreateCopy(bookbal);
				bookbal2.DeprToDate = null;

				Params.Fill(this, bookbal2, null, method, additionEndDate);

				if (DateTime.Compare(Params.RecoveryEndDate, (DateTime)bookbal.DeprToDate) < 0)
				{
					bookbal.DeprToDate = Params.RecoveryEndDate;
					bookbal.DeprToPeriod = FABookPeriodRepository.GetFABookPeriodIDOfDate(Params.RecoveryEndDate, bookbal.BookID, bookbal.AssetID);
				}
			}

			bookbal.AcquisitionCost = Round((decimal)(next.PtdDeprBase * bookbal.BusinessUse * 0.01m));
			bookbal.AcquisitionCost -= (additionDeprFromPeriod == bookDeprFromPeriod) ? bookbal.Tax179Amount : 0m;
			bookbal.AcquisitionCost -= (additionDeprFromPeriod == bookDeprFromPeriod) ? bookbal.BonusAmount : 0m;
			bookbal.SalvageAmount = (additionDeprFromPeriod == bookDeprFromPeriod) ? bookbal.SalvageAmount : 0m;

			_Destination = FADestination.Calculated;

			CalculateDepreciation(method, bookbal, additionEndDate);
		}

		public virtual void CalculateDepreciation(FABookBalance assetBalance, string maxPeriodID)
		{
			// We should clear all Ptd/Ytd Calculated amounts for each Book in the FABookHistory table
			// to guarantee correct calculation for each period if DeprFromPeriod value has been changed,
			// or Fixed Asset has been disposed/splitted e.t.c.
			//
			PXUpdate<Set<FABookHistory.ptdCalculated, decimal0, 
				Set<FABookHistory.ytdCalculated, decimal0>>, FABookHistory,
				Where<FABookHistory.assetID, Equal<Required<FABookHistory.assetID>>,
					And<FABookHistory.bookID, Equal<Required<FABookHistory.bookID>>>>>
				.Update(this, assetBalance.AssetID, assetBalance.BookID);

			histdict = new Dictionary<string, FABookHist>();
			foreach (FABookHist item in  PXSelectReadonly<FABookHist, 
				Where<FABookHist.assetID, Equal<Required<FABookHist.assetID>>, 
					And<FABookHist.bookID, Equal<Required<FABookHist.bookID>>>>>
				.Select(this, assetBalance.AssetID, assetBalance.BookID))
			{
				histdict[item.FinPeriodID] = item;
			}

			FADepreciationMethod method = PXSelect<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(this, assetBalance.DepreciationMethodID);

			if (assetBalance.Depreciate == true && (string.IsNullOrEmpty(assetBalance.DeprFromPeriod) || string.IsNullOrEmpty(assetBalance.DeprToPeriod) || string.CompareOrdinal(assetBalance.DeprFromPeriod, assetBalance.DeprToPeriod) > 0))
			{
				FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Current<FABookBalance.bookID>>>>.SelectSingleBound(this, new object[] { assetBalance });
				throw new PXException(Messages.IncorrectDepreciationPeriods, book.BookCode);
			}

			FAClass cls = (PXResult<FixedAsset, FAClass>)PXSelectJoin<FixedAsset,
				LeftJoin<FAClass, On<FAClass.assetID, Equal<FixedAsset.classID>>>,
				Where<FixedAsset.assetID, Equal<Required<FABookBalance.assetID>>>>.Select(this, assetBalance.AssetID).FirstOrDefault();

			FADetails details = PXSelect<FADetails, Where<FADetails.assetID, Equal<Required<FABookBalance.assetID>>>>.Select(this, assetBalance.AssetID);

			string minPeriod = assetBalance.DeprFromPeriod;

			if (maxPeriodID == null || string.CompareOrdinal(maxPeriodID, assetBalance.DeprToPeriod) > 0)
			{
				maxPeriodID = assetBalance.DeprToPeriod;
			}

			PXRowInserting FABookHistRowInserting = delegate(PXCache sender, PXRowInsertingEventArgs e)
			{
				FABookHist item = e.Row as FABookHist;
				if(item == null) return;

				if(string.CompareOrdinal(item.FinPeriodID, maxPeriodID) > 0)
				{
					e.Cancel = true;
				}
			};

			RowInserting.AddHandler<FABookHist>(FABookHistRowInserting);

			foreach (PXResult<FABookHistoryNextPeriod, FABookHistory> res in PXSelectReadonly2<FABookHistoryNextPeriod, InnerJoin<FABookHistory, 
																					On<FABookHistory.assetID, Equal<FABookHistoryNextPeriod.assetID>, 
																						And<FABookHistory.bookID, Equal<FABookHistoryNextPeriod.bookID>, 
																						And<FABookHistory.finPeriodID, Equal<FABookHistoryNextPeriod.nextPeriodID>>>>>, 
																					Where<FABookHistoryNextPeriod.assetID, Equal<Current<FABookBalance.assetID>>, 
																						And<FABookHistoryNextPeriod.bookID, Equal<Current<FABookBalance.bookID>>, 
																						And<FABookHistoryNextPeriod.ptdDeprBase, NotEqual<decimal0>, 
																						And<FABookHistoryNextPeriod.finPeriodID, LessEqual<Current<FABookBalance.deprToPeriod>>>>>>, 
																					OrderBy<Asc<FABookHistoryNextPeriod.finPeriodID>>>.SelectMultiBound(this, new object[] { assetBalance }))
			{
				FABookHistory next = res;
				next.PtdDeprBase = ((FABookHistoryNextPeriod)res).PtdDeprBase;

				FABookBalance bookbal = PXCache<FABookBalance>.CreateCopy(assetBalance);
				string bookDeprFromPeriod = bookbal.DeprFromPeriod;
				string additionDeprFromPeriod = FABookPeriodUtils.PeriodPlusPeriodsCount(next.FinPeriodID, -(next.YtdReversed ?? 0), next.BookID, next.AssetID);

				if (string.CompareOrdinal(additionDeprFromPeriod, minPeriod) < 0)
				{
					minPeriod = additionDeprFromPeriod;
				}
				PXRowInserting AdditionInserting = delegate(PXCache sender, PXRowInsertingEventArgs e)
				{
					FABookHist item = e.Row as FABookHist;
					if (item == null) return;

					if (string.CompareOrdinal(item.FinPeriodID, additionDeprFromPeriod) < 0)
					{
						e.Cancel = true;
					}
				};

				RowInserting.AddHandler<FABookHist>(AdditionInserting);

				CalculateDepreciationAddition(cls, bookbal, method, next);

				if (additionDeprFromPeriod == bookDeprFromPeriod && bookbal.Tax179Amount > 0m)
				{
					FABookHist accuhist = new FABookHist
					{
						AssetID = bookbal.AssetID,
						BookID = bookbal.BookID,
						FinPeriodID = bookDeprFromPeriod
					};

					accuhist = BookHistory.Insert(accuhist);

					accuhist.PtdCalculated += bookbal.Tax179Amount;
					accuhist.YtdCalculated += bookbal.Tax179Amount;
					accuhist.PtdTax179Calculated += bookbal.Tax179Amount;
					accuhist.YtdTax179Calculated += bookbal.Tax179Amount;

					_Destination = FADestination.Tax179;

					bookbal.AcquisitionCost = bookbal.Tax179Amount;
					bookbal.SalvageAmount = 0m;

					CalculateDepreciation(method, bookbal);
				}

				if (additionDeprFromPeriod == bookDeprFromPeriod && bookbal.BonusAmount > 0m)
				{
					FABookHist accuhist = new FABookHist
					{
						AssetID = bookbal.AssetID,
						BookID = bookbal.BookID,
						FinPeriodID = bookDeprFromPeriod
					};

					accuhist = BookHistory.Insert(accuhist);

					accuhist.PtdCalculated += bookbal.BonusAmount;
					accuhist.YtdCalculated += bookbal.BonusAmount;
					accuhist.PtdBonusCalculated += bookbal.BonusAmount;
					accuhist.YtdBonusCalculated += bookbal.BonusAmount;

					_Destination = FADestination.Bonus;

					bookbal.AcquisitionCost = bookbal.BonusAmount;
					bookbal.SalvageAmount = 0m;

					CalculateDepreciation(method, bookbal);
				}

				RowInserting.RemoveHandler<FABookHist>(AdditionInserting);
			}

			PXCache cache = Caches[typeof(FABookHist)];

			List<FABookHist> inserted = new List<FABookHist>((IEnumerable<FABookHist>)cache.Inserted);
			inserted.Sort((a, b) => string.CompareOrdinal(a.FinPeriodID, b.FinPeriodID));

			decimal? running = 0m;
			string lastGoodPeriodID = minPeriod;
			decimal? lastGoodVal = 0m;
			bool lastGoodFlag = true;

			foreach (FABookHist item in inserted)
			{
				item.YtdCalculated += running;
				running += item.PtdCalculated;

				FABookHist existing;
				if (histdict.TryGetValue(item.FinPeriodID, out existing))
				{
					item.PtdDepreciated = existing.PtdDepreciated;
					item.YtdDepreciated = existing.YtdDepreciated;

					if (UseAcceleratedDepreciation(cls, method) 
						&& string.Equals(existing.FinPeriodID, assetBalance.CurrDeprPeriod) 
						&& Math.Abs((decimal)item.YtdCalculated - (decimal)item.PtdCalculated - (decimal)existing.YtdDepreciated) > 0.00005m)
					{
						//previous period YtdCalculated - YtdDepreciated
						decimal? catchup = item.YtdCalculated - item.PtdCalculated - existing.YtdDepreciated;

						FABookBalance bookbal = PXCache<FABookBalance>.CreateCopy(assetBalance);

						FABookHistory next = new FABookHistory()
						{
							AssetID = assetBalance.AssetID,
							FinPeriodID = assetBalance.CurrDeprPeriod,
							BookID = assetBalance.BookID,
							PtdDeprBase = catchup,
							YtdSuspended = existing.YtdSuspended,
							YtdReversed = existing.YtdReversed
						};

						CalculateDepreciationAddition(cls, bookbal, method, next);

						item.YtdCalculated = existing.YtdDepreciated + item.PtdCalculated;
						running = item.YtdCalculated;
					}

					if (lastGoodFlag)
					{
						lastGoodPeriodID = item.FinPeriodID;
						if (UseAcceleratedDepreciation(cls, method) && string.CompareOrdinal(item.FinPeriodID, assetBalance.CurrDeprPeriod) < 0)
						{
							if (Math.Abs((decimal)existing.YtdDepreciated - (decimal)existing.YtdCalculated) >= 0.00005m)
							{
								lastGoodFlag = false;
								continue;
							}
							lastGoodVal = existing.YtdDepreciated;
						}
						else
						{
							if (Math.Abs((decimal)existing.YtdCalculated - (decimal)item.YtdCalculated) >= 0.00005m)
							{
								lastGoodFlag = false;
								continue;
							}
							lastGoodVal = existing.YtdCalculated;
						}
						cache.SetStatus(item, PXEntryStatus.Notchanged);
					}
				}
				else
				{
					//in case of hole found in existing depreciation mark as last good
					if (lastGoodFlag)
					{
						lastGoodPeriodID = item.FinPeriodID;
						lastGoodFlag = false;
					}
				}
			}
			RowInserting.RemoveHandler<FABookHist>(FABookHistRowInserting);

			foreach (FABookHist item in inserted)
			{
				decimal adjusted = 0m;

				if (UseAcceleratedDepreciation(cls, method) && string.CompareOrdinal(item.FinPeriodID, assetBalance.CurrDeprPeriod) < 0)
				{
					item.PtdCalculated = item.PtdDepreciated;

				}

				if (UseAcceleratedDepreciation(cls, method) && string.CompareOrdinal(item.FinPeriodID, lastGoodPeriodID) >= 0)
				{
					FABookHist existing;
					if (histdict.TryGetValue(item.FinPeriodID, out existing))
					{
						adjusted = existing.PtdAdjusted ?? 0m;
					}
				}

				item.YtdCalculated = item.PtdCalculated + adjusted;
				item.PtdDepreciated = 0m;
				item.YtdDepreciated = 0m;
			}

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				string receiptPeriodID = FABookPeriodRepository.GetFABookPeriodIDOfDate(details.ReceiptDate, assetBalance.BookID, assetBalance.AssetID, false) ?? minPeriod;
				PXDatabase.Delete<FABookHistory>(
						new PXDataFieldRestrict<FABookHistory.assetID>(PXDbType.Int, 4, assetBalance.AssetID, PXComp.EQ),
						new PXDataFieldRestrict<FABookHistory.bookID>(PXDbType.Int, 4, assetBalance.BookID, PXComp.EQ),
						new PXDataFieldRestrict<FABookHistory.finPeriodID>(PXDbType.Char, 6, FinPeriodUtils.Min(receiptPeriodID, minPeriod), PXComp.LT));

				if(string.CompareOrdinal(assetBalance.LastDeprPeriod, assetBalance.DeprToPeriod) < 0)
				{
					PXDatabase.Delete<FABookHistory>(
							new PXDataFieldRestrict<FABookHistory.assetID>(PXDbType.Int, 4, assetBalance.AssetID, PXComp.EQ),
							new PXDataFieldRestrict<FABookHistory.bookID>(PXDbType.Int, 4, assetBalance.BookID, PXComp.EQ),
							new PXDataFieldRestrict<FABookHistory.finPeriodID>(PXDbType.Char, 6, assetBalance.DeprToPeriod, PXComp.GT));
				}

				//otherwise PtdDepreciated will be reset to zero on the last period
				if (!lastGoodFlag)
				{
					PXDatabase.Update<FABookHistory>(
							new PXDataFieldRestrict<FABookHistory.assetID>(PXDbType.Int, 4, assetBalance.AssetID, PXComp.EQ),
							new PXDataFieldRestrict<FABookHistory.bookID>(PXDbType.Int, 4, assetBalance.BookID, PXComp.EQ),
							new PXDataFieldRestrict<FABookHistory.finPeriodID>(PXDbType.Char, 6, lastGoodPeriodID, PXComp.GE),
							new PXDataFieldRestrict<FABookHistory.finPeriodID>( PXDbType.Char, 6, maxPeriodID, PXComp.LE),
							new PXDataFieldAssign<FABookHistory.ptdCalculated>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<FABookHistory.ytdCalculated>(PXDbType.Decimal, lastGoodVal),
							new PXDataFieldAssign<FABookHistory.ptdBonusCalculated>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<FABookHistory.ytdBonusCalculated>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<FABookHistory.ptdTax179Calculated>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<FABookHistory.ytdTax179Calculated>( PXDbType.Decimal, 0m),
							new PXDataFieldAssign<FABookHistory.ptdBonusTaken>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<FABookHistory.ytdBonusTaken>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<FABookHistory.ptdTax179Taken>(PXDbType.Decimal, 0m),
							new PXDataFieldAssign<FABookHistory.ytdTax179Taken>(PXDbType.Decimal, 0m)
							);
				}
				Save.Press();

				if (assetBalance.UpdateGL == true)
				{
					string maxClosedPeriod = null;
					foreach (FABookHistory hist in PXSelectJoin<
						FABookHistory,
						InnerJoin<FixedAsset,
							On<FixedAsset.assetID, Equal<FABookHistory.assetID>>,
						InnerJoin<Branch,
							On<Branch.branchID, Equal<FixedAsset.branchID>>,
						InnerJoin<OrganizationFinPeriod,
							On<OrganizationFinPeriod.organizationID, Equal<Branch.organizationID>,
								And<OrganizationFinPeriod.finPeriodID, Equal<FABookHistory.finPeriodID>>>>>>,
						Where<FABookHistory.closed, NotEqual<True>,
							And<OrganizationFinPeriod.fAClosed, Equal<True>, 
							And<FABookHistory.assetID, Equal<Current<FABookBalance.assetID>>,
							And<FABookHistory.bookID, Equal<Current<FABookBalance.bookID>>>>>>>
						.SelectMultiBound(this, new object[] { assetBalance }))
					{
						FABookHist accuhist = new FABookHist
						{
							AssetID = assetBalance.AssetID,
							BookID = assetBalance.BookID,
							FinPeriodID = hist.FinPeriodID
						};

						accuhist = BookHistory.Insert(accuhist);

						accuhist.Closed = true;

						if (maxClosedPeriod == null || string.CompareOrdinal(hist.FinPeriodID, maxClosedPeriod) > 0)
						{
							maxClosedPeriod = hist.FinPeriodID;
						}
					}

					if (string.CompareOrdinal(maxClosedPeriod, assetBalance.DeprFromPeriod) >= 0)
					{
					AssetProcess.SetLastDeprPeriod(BookBalance, assetBalance, maxClosedPeriod);
					}
					AssetProcess.AdjustFixedAssetStatus(this, assetBalance.AssetID);

					Save.Press();
				}

				ts.Complete();
			}
		}

		public virtual void CalculateDepreciation(FADepreciationMethod method, FABookBalance assetBalance, DateTime? additionEndDate = null)
		{
			if (assetBalance.Depreciate != true) return;

			FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Required<FABook.bookID>>>>.Select(this, assetBalance.BookID);
			IYearSetup yearSetup = FABookPeriodRepository.FindFABookYearSetup(book);

			string calculationMethod = method.DepreciationMethod;
			decimal multiPlier = method.DBMultiPlier ?? 0m;
			bool switchToSL = method.SwitchToSL == true;
			bool isTableMethod = method.IsTableMethod == true;
			bool yearlyAccountancy = method.YearlyAccountancy == true;

			Params.Fill(this, assetBalance, book, method, additionEndDate);

			// TODO: Move into DeprCalcParameters.Calculate function
			Params.DepreciationBasis = method.DepreciationMethod == FADepreciationMethod.depreciationMethod.DecliningBalance
				? assetBalance.AcquisitionCost ?? 0m
				: (assetBalance.AcquisitionCost ?? 0m) - (assetBalance.SalvageAmount ?? 0m);

			// TODO: Extract IsValid property
			if (Params.DepreciationPeriodsInYear == 0 ||
				Params.DepreciationBasis == 0m ||
				string.IsNullOrEmpty(calculationMethod) ||
				calculationMethod == FADepreciationMethod.depreciationMethod.DecliningBalance && !isTableMethod && multiPlier == 0m
				) return;

			decimal otherDepreciation = 0m;
			decimal lastDepreciation = 0m;
			decimal rounding = 0m;
			int yearCount = 0;
			DateTime previousEndDate = DateTime.MinValue;

			previousCalculatedPeriods = 0;
			foreach(FABookPeriod per in Params.DeprPeriods)
			{
				int currYear;
				int.TryParse(per.FinYear, out currYear);
				if(yearCount != currYear)	
				{
					FADepreciationMethodLines methodLine = null;
					int yearNumber = currYear - Params.DepreciationStartYear + 1;
					if (method.IsTableMethod == true)
					{
						methodLine =PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Required<FADepreciationMethodLines.methodID>>,
							And<FADepreciationMethodLines.year, Equal<Required<FADepreciationMethodLines.year>>>>>.Select(this, method.MethodID, yearNumber);
						if (methodLine == null)
						{
							throw new PXException(Messages.TableMethodHasNoLineForYear, method.MethodCD, per.FinYear);
						}
					}

					Params.DepreciationPeriodsInYear = Params.GetPeriodsInYear(currYear);
					SetLinePercents(isTableMethod, true, yearlyAccountancy, methodLine, yearNumber, assetBalance, calculationMethod, multiPlier, switchToSL, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);
					yearCount = currYear;
				}
			}
		}

		protected virtual void SetLinePercents(bool isTableMethod, bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, string depreciationMethod, decimal multiPlier, bool switchToSL, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding, ref DateTime previousEndDate)
		{
			if (isTableMethod && yearlyAccountancy)
			{
				Params.AveragingConvention = FAAveragingConvention.FullPeriod;
				SetSLDeprOther(writeToAsset, true, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);
			}
			else
			{
				switch(depreciationMethod)
				{
					case FADepreciationMethod.depreciationMethod.StraightLine:
					case FADepreciationMethod.depreciationMethod.RemainingValue:
					if (Params.AveragingConvention == FAAveragingConvention.FullDay)
					{
						SetSLDeprOther(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);
					}
					else switch (Params.DepreciationPeriodsInYear)
					{
						case 12:
							SetSLDepr12(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case 4:
							SetSLDepr4(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case 2:
							SetSLDepr2(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case 1:
							SetSLDepr1(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						default:
							SetSLDeprOther(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);
							break;
					}
						break;
					case FADepreciationMethod.depreciationMethod.DecliningBalance:
						SetDBDepr(writeToAsset, yearlyAccountancy, line, year, assetBalance, multiPlier, switchToSL, ref rounding, ref previousEndDate);
						break;
					case FADepreciationMethod.depreciationMethod.SumOfTheYearsDigits:
						SetYDDepr(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref rounding, ref previousEndDate);
						break;
					case FADepreciationMethod.depreciationMethod.Dutch1:
						SetNL1Depr(writeToAsset, line, year, assetBalance, ref rounding);
						break;
					case FADepreciationMethod.depreciationMethod.Dutch2:
						SetNL2Depr(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref rounding);
						break;
					case FADepreciationMethod.depreciationMethod.RemainingValueByPeriodLength:
						SetRemainingValueByPeriodLengthDepr(writeToAsset, line, year, assetBalance, ref rounding);
						break;
					case FADepreciationMethod.depreciationMethod.DecliningBalanceByPeriodLength:
						SetDecliningBalanceByPeriodLengthDepr(writeToAsset, yearlyAccountancy, line, year, assetBalance, multiPlier, ref rounding);
						break;
					case FADepreciationMethod.depreciationMethod.AustralianPrimeCost:
						SetAustralianPrimeCostDepr(writeToAsset, year, assetBalance, ref rounding);
						break;
					case FADepreciationMethod.depreciationMethod.AustralianDiminishingValue:
						SetAustralianDiminishingValueDepr(writeToAsset, year, assetBalance, ref rounding);
						break;
					case FADepreciationMethod.depreciationMethod.NewZealandStraightLine:
						SetNewZealandStraightLineDepr(writeToAsset, year, assetBalance, ref rounding);
						break;
					case FADepreciationMethod.depreciationMethod.NewZealandDiminishingValue:
						SetNewZealandDiminishingValueDepr(writeToAsset, year, assetBalance, ref rounding);
						break;
					case FADepreciationMethod.depreciationMethod.NewZealandStraightLineEvenly:
						SetNewZealandStraightLineEvenlyDepr(writeToAsset, year, assetBalance, ref rounding);
						break;
					case FADepreciationMethod.depreciationMethod.NewZealandDiminishingValueEvenly:
						SetNewZealandDiminishingValueEvenlyDepr(writeToAsset, year, assetBalance, ref rounding);
						break;
				}
			}
		}

		private decimal GetRoundingDelta(decimal rounding)
		{
			decimal decimals = (decimal)Math.Pow(0.1, _Precision);
			return rounding > 0m 
				? rounding >= decimals 
					? decimals 
					: 0m 
				: rounding < 0m 
					? rounding <= (- decimals) 
						? - decimals 
						: 0m 
					: 0m;
		}

		private void SetFinalRounding(ref decimal rounding)
		{
			decimal decimals = (decimal)Math.Pow(0.1, _Precision);
			decimal delta  = Params.DepreciationBasis - Params.AccumulatedDepreciation;
			if (delta != decimals && delta != -decimals) return;
				
			decimal centIsAppear = rounding > 0m 
				? Round(rounding) == decimals 
					?  decimals : 0m 
					: rounding < 0m 
						? Round(rounding) == (- decimals) ? -decimals 
						: 0m 
				: 0m;
			if (centIsAppear != delta)
			{
				centIsAppear = delta ==  decimals 
					? decimals 
					: delta == -decimals ? -decimals : 0m;
			}
			rounding = centIsAppear;
		}

		//TODO: presumably not used, need review
		private void SetDepreciationPerPeriod(FADepreciationMethodLines line, bool useRounding, ref decimal rounding)
		{
			if (line == null) return;

			if (useRounding && rounding != 0m)
			{
				SetFinalRounding(ref rounding);
				Params.AccumulatedDepreciation += GetRoundingDelta(rounding);
				rounding -= GetRoundingDelta(rounding);
			}
		}

		private void SetBookDepreciationPerPeriod(FABookBalance assetBalance, int year, int period, decimal value, bool useRounding, ref decimal rounding)
		{
			if (assetBalance?.DeprFromDate == null || string.IsNullOrEmpty(assetBalance.DeprFromPeriod)) return;

			int finYear = Params.DepreciationStartYear + year - 1;
				
			if (useRounding && rounding != 0m)
			{
				SetFinalRounding(ref rounding);
				Params.AccumulatedDepreciation += GetRoundingDelta(rounding);
				value += GetRoundingDelta(rounding);
				rounding -= GetRoundingDelta(rounding);
			}
			value = Round(value);

			string PeriodID = $"{finYear:0000}{period:00}";

			FABookHist hist;
			FABookHist newhist = null;
			if (histdict.TryGetValue(PeriodID, out hist) && hist.YtdSuspended > 0)
			{
				for (int i = 0; i <= hist.YtdSuspended; i++)
				{
					//insert suspended periods + next open
					newhist = new FABookHist
					{
						AssetID = assetBalance.AssetID,
						BookID = assetBalance.BookID,
						FinPeriodID = FABookPeriodUtils.PeriodPlusPeriodsCount(PeriodID, i, hist.BookID, hist.AssetID)
					};
					newhist = BookHistory.Insert(newhist);

					if (newhist != null && !histdict.ContainsKey(newhist.FinPeriodID))
					{ 
						FABookHist copy = PXCache<FABookHist>.CreateCopy(newhist);
						copy.YtdSuspended = hist.YtdSuspended;
						copy.PtdCalculated = 0m;
						copy.YtdCalculated = 0m;
						copy.PtdDepreciated = 0m;
						copy.YtdDepreciated = 0m;

						histdict.Add(newhist.FinPeriodID, copy);
					}
				}
			}
			else
			{
				newhist = new FABookHist
				{
					AssetID = assetBalance.AssetID,
					BookID = assetBalance.BookID,
					FinPeriodID = PeriodID
				};
				newhist = BookHistory.Insert(newhist);
			}

			if(newhist != null)
			{
				switch (_Destination)
				{
					case FADestination.Bonus:
						newhist.PtdBonusTaken += value;
						newhist.YtdBonusTaken += value;
						break;
					case FADestination.Tax179:
						newhist.PtdTax179Taken += value;
						newhist.YtdTax179Taken += value;
						break;
					default:
						newhist.PtdCalculated += value;
						newhist.YtdCalculated += value;
						break;
				}
			}
		}

		protected virtual void WhereToWriteDepreciation(bool writeToAsset, FADepreciationMethodLines methodLine, FABookBalance assetBalance, int year, int period, decimal value, bool useRounding, ref decimal rounding)
		{
			if (writeToAsset != true)
			{
				SetDepreciationPerPeriod(methodLine, useRounding, ref rounding);
			}
			else
			{
				SetBookDepreciationPerPeriod(assetBalance, year, period, value, useRounding, ref rounding);	
			}
		}

		private void SetSLDepr12(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			decimal yearDepreciation = Params.DepreciationBasis / Params.WholeRecoveryPeriods * Params.DepreciationPeriodsInYear;

			if (Params.WholeRecoveryPeriods <= 2 && Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
				Params.RecoveryYears <= 2 && Params.AveragingConvention == FAAveragingConvention.HalfYear ||
				(Params.WholeRecoveryPeriods / DeprCalcParameters.QuartersInYear) <= 2 && Params.AveragingConvention == FAAveragingConvention.HalfQuarter ||
				!(Params.AveragingConvention == FAAveragingConvention.HalfYear ||
					Params.AveragingConvention == FAAveragingConvention.FullYear ||
					Params.AveragingConvention == FAAveragingConvention.HalfQuarter ||
					Params.AveragingConvention == FAAveragingConvention.FullQuarter ||
					Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					Params.AveragingConvention == FAAveragingConvention.FullPeriod ||
					Params.AveragingConvention == FAAveragingConvention.NextPeriod) ||
				Params.RecoveryYears == 1 && 
				!(Params.AveragingConvention == FAAveragingConvention.FullPeriod ||
					Params.AveragingConvention == FAAveragingConvention.NextPeriod ||
					Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					Params.AveragingConvention == FAAveragingConvention.HalfQuarter ||
					Params.AveragingConvention == FAAveragingConvention.FullQuarter ||
					(Params.AveragingConvention == FAAveragingConvention.FullYear && Params.WholeRecoveryPeriods == Params.DepreciationPeriodsInYear))
				) return;

			if (year == 1 && year < Params.RecoveryYears)
			{
				otherDepreciation = yearDepreciation / Params.DepreciationPeriodsInYear;

				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
						SetSLDeprHalfPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.NextPeriod:
						SetSLDeprFullPeriodFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);							
						break;
					case FAAveragingConvention.HalfQuarter: // do not use with other metrics
					SetSLDeprHalfQuarterFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullQuarter: // do not use with other metrics
					SetSLDeprFullQuarterFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, yearDepreciation, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.HalfYear:
						SetSLDeprHalfYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullYear:
					SetSLDeprFullYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation,ref rounding);
						break;
				}
					
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				otherDepreciation = Params.DepreciationBasis / Params.WholeRecoveryPeriods;
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
						SetSLDeprHalfPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprModifiedPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.FullYear:
					case FAAveragingConvention.NextPeriod:
						SetSLDeprFullPeriodFirstYearEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
				}
			}
			else if(year == Params.RecoveryYears)
			{
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprHalfPeriodLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.NextPeriod:
						SetSLDeprFullPeriodLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
					case FAAveragingConvention.HalfQuarter:
						SetSLDeprHalfQuarterLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullQuarter:
						SetSLDeprFullQuarterLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
					case FAAveragingConvention.HalfYear:
						SetSLDeprHalfYearLastYear(writeToAsset, line, year, assetBalance, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullYear:
						SetSLDeprFullYearLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
				}
			}
			else
			{
				SetSLDeprOtherYears(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
			}
		}

		private void SetSLDepr4(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			if (Params.WholeRecoveryPeriods <= 2 && (Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
				Params.AveragingConvention == FAAveragingConvention.HalfQuarter) ||
				Params.RecoveryYears <= 2 && Params.AveragingConvention == FAAveragingConvention.HalfYear ||
				!(Params.AveragingConvention == FAAveragingConvention.HalfYear ||
					Params.AveragingConvention == FAAveragingConvention.FullYear ||
					Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					Params.AveragingConvention == FAAveragingConvention.FullPeriod ||
					Params.AveragingConvention == FAAveragingConvention.NextPeriod ||
					Params.AveragingConvention == FAAveragingConvention.HalfQuarter ||
					Params.AveragingConvention == FAAveragingConvention.FullQuarter) ||
				Params.RecoveryYears == 1 && 
				!(Params.AveragingConvention == FAAveragingConvention.FullPeriod ||
					Params.AveragingConvention == FAAveragingConvention.NextPeriod ||
					Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					Params.AveragingConvention == FAAveragingConvention.HalfQuarter ||
					Params.AveragingConvention == FAAveragingConvention.FullQuarter ||
					(Params.AveragingConvention == FAAveragingConvention.FullYear && Params.WholeRecoveryPeriods == Params.DepreciationPeriodsInYear))
				) return;

			if (year == 1 && year < Params.RecoveryYears)
			{
				otherDepreciation = Params.DepreciationBasis / Params.WholeRecoveryPeriods;
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
					case FAAveragingConvention.HalfQuarter:
						SetSLDeprHalfPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.FullQuarter:
					case FAAveragingConvention.NextPeriod:
						SetSLDeprFullPeriodFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);							
						break;
					case FAAveragingConvention.HalfYear:
						SetSLDeprHalfYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullYear:
					SetSLDeprFullYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
				}
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				otherDepreciation = Params.DepreciationBasis / Params.WholeRecoveryPeriods;
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
					case FAAveragingConvention.HalfQuarter:
						SetSLDeprHalfPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprModifiedPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.NextPeriod:
					case FAAveragingConvention.FullQuarter:
					case FAAveragingConvention.FullYear:
						SetSLDeprFullPeriodFirstYearEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
				}
			}
			else if (year == Params.RecoveryYears)
			{
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
					case FAAveragingConvention.HalfQuarter:
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprHalfPeriodLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.NextPeriod:
					case FAAveragingConvention.FullQuarter:
						SetSLDeprFullPeriodLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
					case FAAveragingConvention.HalfYear:
						SetSLDeprHalfYearLastYear(writeToAsset, line, year, assetBalance, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullYear:
						SetSLDeprFullYearLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
				}
			}
			else
			{
				SetSLDeprOtherYears(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
			}
		}
			
		private void SetSLDepr2(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			decimal totalHalfYears = Params.WholeRecoveryPeriods;
				
			if (totalHalfYears <= 2 && (Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
				Params.AveragingConvention == FAAveragingConvention.FullQuarter) ||
				Params.RecoveryYears <= 2 && Params.AveragingConvention == FAAveragingConvention.HalfYear ||
				!(Params.AveragingConvention == FAAveragingConvention.HalfYear ||
					Params.AveragingConvention == FAAveragingConvention.FullYear ||
					Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					Params.AveragingConvention == FAAveragingConvention.FullQuarter ||
					Params.AveragingConvention == FAAveragingConvention.FullPeriod ||
					Params.AveragingConvention == FAAveragingConvention.NextPeriod) ||
				Params.RecoveryYears == 1 && 
				!(Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					Params.AveragingConvention == FAAveragingConvention.FullQuarter ||
					Params.AveragingConvention == FAAveragingConvention.FullPeriod ||
					Params.AveragingConvention == FAAveragingConvention.NextPeriod ||
					Params.AveragingConvention == FAAveragingConvention.HalfYear ||
					(Params.AveragingConvention == FAAveragingConvention.FullYear && totalHalfYears == Params.DepreciationPeriodsInYear))
				) return;
		
				if (year == 1 && year < Params.RecoveryYears)
				{
					otherDepreciation = Params.DepreciationBasis / totalHalfYears;
					switch(Params.AveragingConvention)
					{
						case FAAveragingConvention.HalfPeriod:
						case FAAveragingConvention.FullQuarter:
							SetSLDeprHalfPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.ModifiedPeriod:
						case FAAveragingConvention.ModifiedPeriod2:
							SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullPeriod:
						case FAAveragingConvention.NextPeriod:
						case FAAveragingConvention.HalfYear:
							SetSLDeprFullPeriodFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);							
							break;
						case FAAveragingConvention.FullYear:
						SetSLDeprFullYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
							break;
					}
				}
				else if (year == 1 && year == Params.RecoveryYears)
				{
					otherDepreciation = Params.DepreciationBasis / totalHalfYears;
					switch(Params.AveragingConvention)
					{
						case FAAveragingConvention.HalfPeriod:
						case FAAveragingConvention.FullQuarter:
							SetSLDeprHalfPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
							break;
						case FAAveragingConvention.ModifiedPeriod:
						case FAAveragingConvention.ModifiedPeriod2:
							SetSLDeprModifiedPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullPeriod:
						case FAAveragingConvention.NextPeriod:
						case FAAveragingConvention.HalfYear:
						case FAAveragingConvention.FullYear:
							SetSLDeprFullPeriodFirstYearEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
							break;
					}
				}
				else if (year == Params.RecoveryYears)
				{
					switch(Params.AveragingConvention)
					{
						case FAAveragingConvention.HalfPeriod:
						case FAAveragingConvention.FullQuarter:
						case FAAveragingConvention.ModifiedPeriod:
						case FAAveragingConvention.ModifiedPeriod2:
							SetSLDeprHalfPeriodLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullPeriod:
						case FAAveragingConvention.NextPeriod:
						case FAAveragingConvention.HalfYear:
							SetSLDeprFullPeriodLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullYear:
							SetSLDeprFullYearLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
							break;
					}
				}
				else
				{
					SetSLDeprOtherYears(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
				}
		}

		private void SetSLDepr1(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			Params.DepreciationStartPeriod = 1;
			Params.DepreciationStopPeriod = 1;
			Params.RecoveryEndPeriod = 1;
				
			if (Params.DepreciationYears <= 2 && (Params.AveragingConvention == FAAveragingConvention.HalfYear || 
				Params.AveragingConvention == FAAveragingConvention.HalfPeriod) ||
				!(Params.AveragingConvention == FAAveragingConvention.HalfYear ||
					Params.AveragingConvention == FAAveragingConvention.FullYear ||
					Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					Params.AveragingConvention == FAAveragingConvention.FullPeriod ||
					Params.AveragingConvention == FAAveragingConvention.NextPeriod) ||
				Params.RecoveryYears == 1 && 
				!(Params.AveragingConvention == FAAveragingConvention.FullPeriod ||
					Params.AveragingConvention == FAAveragingConvention.NextPeriod ||
					Params.AveragingConvention == FAAveragingConvention.FullYear)
				) return;
		
			if (year == 1 && year < Params.RecoveryYears)
			{
				otherDepreciation = Params.DepreciationBasis / Params.DepreciationYears;
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfYear:
					case FAAveragingConvention.HalfPeriod:
						SetSLDeprHalfPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullYear:
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.NextPeriod:
						SetSLDeprFullPeriodFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);							
						break;
				}
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				otherDepreciation = Params.DepreciationBasis / Params.DepreciationYears;
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
					case FAAveragingConvention.HalfYear:
						SetSLDeprHalfPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprModifiedPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullYear:
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.NextPeriod:
						SetSLDeprFullPeriodFirstYearEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
				}
			}
			else if (year == Params.RecoveryYears)
			{
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfYear:
					case FAAveragingConvention.HalfPeriod:
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprHalfPeriodLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullYear:
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.NextPeriod:
						SetSLDeprFullPeriodLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
				}
			}
			else
			{
				SetSLDeprOtherYears(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
			}
		}
			
		private void SetSLDeprOther(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding, ref DateTime previousEndDate)
		{
			if (Params.WholeRecoveryPeriods <= 2 && Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
				Params.RecoveryYears <= 2 && Params.AveragingConvention == FAAveragingConvention.HalfYear ||
				!(Params.AveragingConvention == FAAveragingConvention.HalfYear ||
					Params.AveragingConvention == FAAveragingConvention.FullYear ||
					Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					Params.AveragingConvention == FAAveragingConvention.FullPeriod ||
					Params.AveragingConvention == FAAveragingConvention.NextPeriod ||
					Params.AveragingConvention == FAAveragingConvention.FullDay && (Params.DepreciationStopDate != null && Params.WholeRecoveryDays != 0 && Params.DepreciationStartYear != 0)) ||
				Params.RecoveryYears == 1 &&
				!(Params.AveragingConvention == FAAveragingConvention.HalfPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
					Params.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					Params.AveragingConvention == FAAveragingConvention.FullPeriod ||
					Params.AveragingConvention == FAAveragingConvention.NextPeriod ||
					Params.AveragingConvention == FAAveragingConvention.FullDay && (Params.DepreciationStopDate != null && Params.WholeRecoveryDays != 0 && Params.DepreciationStartYear != 0) ||
					(Params.AveragingConvention == FAAveragingConvention.FullYear && Params.WholeRecoveryPeriods == Params.DepreciationPeriodsInYear))
				) return;

			if (Params.AveragingConvention == FAAveragingConvention.FullDay)
			{
				decimal periodDepr = Params.DepreciationBasis / Params.WholeRecoveryPeriods;
				int deprYear = Params.DepreciationStartYear + year - 1;
				int firstPeriod, lastPeriod;
					
				if (year == 1 && year < Params.RecoveryYears)
				{
					firstPeriod = Params.DepreciationStartPeriod;
					lastPeriod = Params.DepreciationPeriodsInYear;
				}
				else if (year == 1 && year == Params.RecoveryYears)
				{
					firstPeriod = Params.DepreciationStartPeriod;
					lastPeriod = Params.DepreciationStopPeriod;
				}
				else if (year == Params.RecoveryYears)
				{
					firstPeriod = 1;
					lastPeriod = Params.DepreciationStopPeriod;
				}
				else
				{
					firstPeriod = 1;
					lastPeriod = Params.DepreciationPeriodsInYear;
				}

				for (int i = firstPeriod; i <= lastPeriod; i++)
				{
					if (i == Params.DepreciationStartPeriod
					    && i == Params.DepreciationStopPeriod 
					    && deprYear == Params.DepreciationStartYear 
					    && deprYear == int.Parse(Params.DepreciationStopBookPeriod.FinYear)) // first period == last period
					{
						previousEndDate = Params.DepreciationStopDate.Value;
						int allPeriodDays = Params.GetPeriodLength(deprYear, i);
						int deprPeriodDays = Params.GetDaysOnPeriod(Params.DepreciationStartDate, Params.DepreciationStopDate, deprYear, i, ref previousEndDate);
						otherDepreciation = periodDepr * deprPeriodDays / allPeriodDays;
					}
					else if (i == Params.DepreciationStartPeriod && deprYear == Params.DepreciationStartYear) // first period
					{
						int allPeriodDays = Params.GetPeriodLength(deprYear, i);
						int deprPeriodDays = Params.GetDaysOnPeriod(Params.DepreciationStartDate, Params.DepreciationStopDate, deprYear, i, ref previousEndDate);
						otherDepreciation = periodDepr * deprPeriodDays / allPeriodDays;
					}
					else if (i == Params.DepreciationStopPeriod && deprYear == int.Parse(Params.DepreciationStopBookPeriod.FinYear)) // last period
					{
						if (Params.DepreciationStopDate != null)
						{
							int allPeriodDays = Params.GetPeriodLength(deprYear, i);
							int deprPeriodDays = Params.GetDaysOnPeriod(Params.DepreciationStartDate, Params.DepreciationStopDate, deprYear, i, ref previousEndDate);
							otherDepreciation = periodDepr * deprPeriodDays / allPeriodDays;
						}
						else
						{
							otherDepreciation = Params.DepreciationBasis - Params.AccumulatedDepreciation;
						}
					}
					else
					{
						otherDepreciation = periodDepr;
					}

					Params.AccumulatedDepreciation += Round(otherDepreciation);
					rounding += otherDepreciation - Round(otherDepreciation);
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
				}
			}
			else if (year == 1 && year < Params.RecoveryYears)
			{
				otherDepreciation = Params.DepreciationBasis / Params.WholeRecoveryPeriods;
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
						SetSLDeprHalfPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;	
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.NextPeriod:
						SetSLDeprFullPeriodFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);							
						break;
					case FAAveragingConvention.HalfYear:
						SetSLDeprHalfYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullYear:
					SetSLDeprFullYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
				}
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				otherDepreciation = Params.DepreciationBasis / Params.WholeRecoveryPeriods;
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
						SetSLDeprHalfPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprModifiedPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.NextPeriod:
					case FAAveragingConvention.FullYear:
						SetSLDeprFullPeriodFirstYearEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
				}
			}
			else if(year == Params.RecoveryYears)
			{
				switch(Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						SetSLDeprHalfPeriodLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullPeriod:
					case FAAveragingConvention.NextPeriod:
						SetSLDeprFullPeriodLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
					case FAAveragingConvention.HalfYear:
						SetSLDeprHalfYearLastYear(writeToAsset, line, year, assetBalance, ref lastDepreciation, ref rounding);
						break;
					case FAAveragingConvention.FullYear:
						SetSLDeprFullYearLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
						break;
				}
			}
			else
			{
				bool isTableMethod = yearlyAccountancy && line != null && Params.AveragingConvention == FAAveragingConvention.FullPeriod;
				if (isTableMethod)
				{
					otherDepreciation = (line.RatioPerYear ?? 0m) * Params.DepreciationBasis / Params.DepreciationPeriodsInYear;	
				}

				for(int i = 1; i <= Params.DepreciationPeriodsInYear; i++)
				{
					if (isTableMethod)
					{
						Params.AccumulatedDepreciation += Round(otherDepreciation);
						rounding+= otherDepreciation - Round(otherDepreciation);
					}

					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
				}
			}
		}

		private void SetDBDepr(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, decimal multiPlier, bool switchToSL, ref decimal rounding, ref DateTime previousEndDate)
		{
			decimal depreciation;
			decimal yearDepreciation = 0m;
			decimal slYearDepreciation = 0m;
			decimal slAdjustedYearDepr = 0m;
			decimal avg_mult = 1m;
			int yearDays = 0;
	
			int fromPeriod;
			int toPeriod; 

			if (yearlyAccountancy && Params.RecoveryYears > Params.UsefulLife)
			{
				switch (Params.AveragingConvention)
				{
					case FAAveragingConvention.HalfYear:
						avg_mult = 0.5m;
						break;
					case FAAveragingConvention.HalfQuarter:
						avg_mult = (9m - 2m * ((((DateTime)assetBalance.DeprFromDate).Month + 2)/3))/8m;
						break;
				}
			}

			if (yearlyAccountancy)
			{
			yearDepreciation = (Params.DepreciationBasis - Params.AccumulatedDepreciation) * multiPlier / Params.UsefulLife;
			slAdjustedYearDepr = slYearDepreciation = Params.DepreciationBasis / Params.UsefulLife;
			}

			if (year == 1 && year < Params.RecoveryYears)
			{
				yearDepreciation *= avg_mult;
				slAdjustedYearDepr *= avg_mult;
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;	
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if(year == Params.RecoveryYears)
			{
				if (avg_mult < 1.0m)
				{
					slAdjustedYearDepr *= 1m - avg_mult;
				}
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}

			if (year == Params.DepreciationYears)
			{
				toPeriod = Params.DepreciationStopPeriod;
			}

			decimal slDepreciation;
			if (yearlyAccountancy)
			{
				if (Params.AveragingConvention == FAAveragingConvention.FullDay)
				{
					yearDays = 0;
					DateTime periodPreviousEndDate = previousEndDate;
					for(int i = fromPeriod; i <= toPeriod; i++)
					{
						yearDays += Params.GetDaysOnPeriod(Params.DepreciationStartDate, Params.RecoveryEndDate, Params.DepreciationStartYear + year - 1, i, ref previousEndDate);
					}
					previousEndDate = periodPreviousEndDate;
				}

				decimal prevAccumulatedDepreciation = Params.AccumulatedDepreciation;
				decimal slAccumulatedDepreciation = slYearDepreciation * (year - 1) - (1m - avg_mult) * (1m > avg_mult ? slYearDepreciation : 0m);
				for (int i = fromPeriod; i <= toPeriod; i++)
				{
					if (Params.DepreciationBasis == Params.AccumulatedDepreciation) return;
					if (Params.AveragingConvention == FAAveragingConvention.FullDay)
					{
						int periodDays = Params.GetDaysOnPeriod(Params.DepreciationStartDate, Params.DepreciationStopDate, Params.DepreciationStartYear + year - 1, i, ref previousEndDate);
						depreciation   = yearDepreciation * periodDays / yearDays;
					}
					else if (Params.AveragingConvention == FAAveragingConvention.FullPeriod)
					{
						depreciation = yearDepreciation / Params.DepreciationPeriodsInYear;
					}
					else
					{
						depreciation = yearDepreciation / (toPeriod - fromPeriod + 1);
					}
					if (switchToSL)
					{
						decimal DBRate = multiPlier / Params.UsefulLife;
						decimal SLRate = slAdjustedYearDepr / (Params.DepreciationBasis - slAccumulatedDepreciation);
						slDepreciation = SLRate * (Params.DepreciationBasis - prevAccumulatedDepreciation) / (toPeriod - fromPeriod + 1);
						if (SLRate > DBRate)
						{
							depreciation = slDepreciation > (Params.DepreciationBasis - Params.AccumulatedDepreciation)
								? (Params.DepreciationBasis - Params.AccumulatedDepreciation)
								: slDepreciation;
						}
					}

					Params.AccumulatedDepreciation += Round(depreciation);
					if (Round(depreciation) != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
						rounding += depreciation - Round(depreciation);
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
				}
			}
			else
			{
				for (int i = fromPeriod; i <= toPeriod; i++)
				{	
					if (Params.DepreciationBasis == Params.AccumulatedDepreciation || Params.WholeRecoveryPeriods == previousCalculatedPeriods) return;

					depreciation = (Params.DepreciationBasis - Params.AccumulatedDepreciation) * multiPlier / Params.WholeRecoveryPeriods;
					slDepreciation = (Params.DepreciationBasis - Params.AccumulatedDepreciation)/(Params.WholeRecoveryPeriods - previousCalculatedPeriods++);
					depreciation = switchToSL 
						? slDepreciation > depreciation 
							? slDepreciation > (Params.DepreciationBasis - Params.AccumulatedDepreciation) 
								? (Params.DepreciationBasis - Params.AccumulatedDepreciation) 
								: slDepreciation 
							: depreciation 
						: depreciation;

					Params.AccumulatedDepreciation += Round(depreciation);
					if (Round(depreciation) != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
						rounding += depreciation - Round(depreciation);
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
				}
			}
		}

		private void SetNL1Depr(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal rounding)
		{
			int fromPeriod;
			int toPeriod;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if (year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			int depreciateToPeriod = toPeriod;

			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			decimal acquisitionCost = Params.DepreciationBasis + (assetBalance.SalvageAmount ?? 0m);
			for (int i = fromPeriod; i <= depreciateToPeriod; i++)
			{
				if (acquisitionCost == Params.AccumulatedDepreciation) return;
				decimal depreciation = (decimal)((double)(acquisitionCost - Params.AccumulatedDepreciation) * (1 - Math.Pow((double)(assetBalance.SalvageAmount ?? 0) / (double)acquisitionCost, (double)(1m / Params.WholeRecoveryPeriods))));
				Params.AccumulatedDepreciation += Round(depreciation);
				if (Round(depreciation) != (acquisitionCost - Params.AccumulatedDepreciation))
					rounding += depreciation - Round(depreciation);
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
			}
		}

		private void SetNL2Depr(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal rounding)
		{
			int fromPeriod;
			int toPeriod;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if (year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			int depreciateToPeriod = toPeriod;

			if (year == Params.DepreciationYears)
				depreciateToPeriod = Params.DepreciationStopPeriod;

			decimal yearDepreciation = (Params.DepreciationBasis - Params.AccumulatedDepreciation) * (Params.DepreciationMethod.PercentPerYear ?? 0m)*0.01m;
			for (int i = fromPeriod; i <= depreciateToPeriod; i++)
			{
				if (Params.DepreciationBasis == Params.AccumulatedDepreciation) return;
				decimal depreciation = yearlyAccountancy
					? yearDepreciation/Params.DepreciationPeriodsInYear
					: (decimal)((double) (Params.DepreciationBasis - Params.AccumulatedDepreciation)*(1 - Math.Pow(1 - (double) (Params.DepreciationMethod.PercentPerYear ?? 0m)*0.01, 1.0/Params.DepreciationPeriodsInYear)));
				Params.AccumulatedDepreciation += Round(depreciation);
				if (Round(depreciation) != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
					rounding += depreciation - Round(depreciation);
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
			}
		}

		private void SetRemainingValueByPeriodLengthDepr(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal rounding)
		{
			int fromPeriod;
			int toPeriod;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if (year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			int depreciateToPeriod = toPeriod;

			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			for (int i = fromPeriod; i <= depreciateToPeriod; i++)
			{
				int daysInPeriod = Params.GetFinPeriodLengthInDays(year, i);

				decimal depreciation = Params.DepreciationBasis * daysInPeriod / Params.WholeRecoveryDays;

				if (year == Params.RecoveryYears && i == depreciateToPeriod && 
						Math.Abs(Params.DepreciationBasis - Params.AccumulatedDepreciation) < Math.Abs(Math.Truncate(depreciation * 100m) / 100m))
				{
					depreciation = Params.DepreciationBasis - Params.AccumulatedDepreciation;
				}

				decimal roundedDepreciation = Round(depreciation);

				Params.AccumulatedDepreciation += roundedDepreciation;

				if (roundedDepreciation != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
				{
					rounding += depreciation - roundedDepreciation;
				}

				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, roundedDepreciation, true, ref rounding);
			}
		}

		private void SetDecliningBalanceByPeriodLengthDepr(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, decimal multiPlier, ref decimal rounding)
		{
			int fromPeriod;
			int toPeriod;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if (year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			int depreciateToPeriod = toPeriod;

			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			decimal depreciationBasis = 0m;

			if (yearlyAccountancy == true)
			{
				depreciationBasis = (Params.DepreciationBasis - Params.AccumulatedDepreciation);
			}

			for (int i = fromPeriod; i <= depreciateToPeriod; i++)
			{
				decimal dbRate = 1m / Params.UsefulLife;
				int daysInPeriod = Params.GetFinPeriodLengthInDays(year, i);

				if (yearlyAccountancy == false)
				{
					depreciationBasis = (Params.DepreciationBasis - Params.AccumulatedDepreciation);
				}

				decimal depreciation = depreciationBasis * dbRate * multiPlier * daysInPeriod / 365m;

				decimal roundedDepreciation = Round(depreciation);

				if (roundedDepreciation != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
				{
					rounding += depreciation - roundedDepreciation;
				}

				Params.AccumulatedDepreciation += roundedDepreciation;

				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, roundedDepreciation, true, ref rounding);
			}
		}

		private void SetAustralianPrimeCostDepr(bool writeToAsset, int year, FABookBalance assetBalance, ref decimal rounding)
		{
			int fromPeriod;
			int toPeriod;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if (year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			int depreciateToPeriod = toPeriod;

			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}


			for (int i = fromPeriod; i <= depreciateToPeriod; i++)
			{
				int daysOfPeriod = Params.GetFinPeriodLengthInDays(year, i);
				string periodID = $"{Params.DepreciationStartYear + year - 1:D4}{i:D2}";
				decimal depreciation;
				if (periodID == Params.DepreciationStopBookPeriod.FinPeriodID // last depreciation period
					&& Params.RecoveryEndBookPeriod.FinPeriodID == Params.DepreciationStopBookPeriod.FinPeriodID) // non-disposal
				{
					depreciation = Params.DepreciationBasis - Params.AccumulatedDepreciation;
				}
				else
				{
					depreciation = Params.DepreciationBasis * Params.PercentPerYear / 100 * daysOfPeriod / 365m;
				}

				// Rounding
				decimal roundedDepreciation = Round(depreciation);

				if (roundedDepreciation != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
				{
					rounding += depreciation - roundedDepreciation;
				}

				Params.AccumulatedDepreciation += roundedDepreciation;

				WhereToWriteDepreciation(writeToAsset, null, assetBalance, year, i, roundedDepreciation, true, ref rounding);
			}
		}

		private void SetAustralianDiminishingValueDepr(bool writeToAsset, int year, FABookBalance assetBalance, ref decimal rounding)
		{
			int fromPeriod;
			int toPeriod;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if (year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			int depreciateToPeriod = toPeriod;

			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			decimal netValue = Params.DepreciationBasis - Params.AccumulatedDepreciation;

			for (int i = fromPeriod; i <= depreciateToPeriod; i++)
			{
				int daysOfPeriod = Params.GetFinPeriodLengthInDays(year, i);
				decimal depreciation;
				{
					depreciation = netValue * Params.PercentPerYear / 100 * daysOfPeriod / 365m;
				}

				// Rounding
				decimal roundedDepreciation = Round(depreciation);

				if (roundedDepreciation != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
				{
					rounding += depreciation - roundedDepreciation;
				}

				Params.AccumulatedDepreciation += roundedDepreciation;

				WhereToWriteDepreciation(writeToAsset, null, assetBalance, year, i, roundedDepreciation, true, ref rounding);
			}
		}

		private void SetNewZealandStraightLineDepr(bool writeToAsset, int year, FABookBalance assetBalance, ref decimal rounding)
		{
			int fromPeriod;
			int toPeriod;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if (year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			int depreciateToPeriod = toPeriod;

			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			int daysHeldInYear = Params.GetDaysHeldInYear(year, fromPeriod, toPeriod);
			int periodsHeldInYear = toPeriod - fromPeriod + 1;

			decimal shouldBeAccumulatedInYear = Params.DepreciationBasis * Params.PercentPerYear / 100 * periodsHeldInYear / Params.DepreciationPeriodsInYear;
			decimal prevYearAccumulatedDepreciation = Params.AccumulatedDepreciation;

			for (int i = fromPeriod; i <= depreciateToPeriod; i++)
			{
				int daysOfPeriod = Params.GetFinPeriodLengthInDaysFebAlways28(year, i);
				string periodID = $"{Params.DepreciationStartYear + year - 1:D4}{i:D2}";
				decimal depreciation;
				bool useRounding = true;

				if (periodID == Params.DepreciationStopBookPeriod.FinPeriodID // last depreciation period
					&& Params.RecoveryEndBookPeriod.FinPeriodID == Params.DepreciationStopBookPeriod.FinPeriodID) // non-disposal
				{
					depreciation = Params.DepreciationBasis - Params.AccumulatedDepreciation;
				}
				else if (i == depreciateToPeriod)// last depreciation period in year
				{
					//fix year amount without rounding discrepancy
					decimal currentYearAccumulatedDepreciation = Params.AccumulatedDepreciation - prevYearAccumulatedDepreciation;
					decimal shouldBeAccumulatedInCurrentYear;
					if (periodID == Params.DepreciationStopBookPeriod.FinPeriodID // last depreciation period
						&& Params.RecoveryEndBookPeriod.FinPeriodID != Params.DepreciationStopBookPeriod.FinPeriodID) //disposal
					{
						int daysHeldInDisposalYear = Params.GetDaysHeldInYear(year, fromPeriod, depreciateToPeriod);
						shouldBeAccumulatedInCurrentYear = shouldBeAccumulatedInYear * daysHeldInDisposalYear / daysHeldInYear;
					}
					else
					{
						shouldBeAccumulatedInCurrentYear = shouldBeAccumulatedInYear;
					}
					depreciation = shouldBeAccumulatedInCurrentYear - currentYearAccumulatedDepreciation;
					useRounding = false;
				}
				else
				{
					depreciation = shouldBeAccumulatedInYear * daysOfPeriod / daysHeldInYear;
				}

				// Rounding
				decimal roundedDepreciation = Round(depreciation);
				if (roundedDepreciation != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
				{
					rounding += depreciation - roundedDepreciation;
				}

				Params.AccumulatedDepreciation += roundedDepreciation;

				WhereToWriteDepreciation(writeToAsset, null, assetBalance, year, i, roundedDepreciation, useRounding, ref rounding);
			}
		}
		private void SetNewZealandDiminishingValueDepr(bool writeToAsset, int year, FABookBalance assetBalance, ref decimal rounding)
		{
			int fromPeriod;
			int toPeriod;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if (year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			int depreciateToPeriod = toPeriod;

			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			int daysHeldInYear = Params.GetDaysHeldInYear(year, fromPeriod, toPeriod);
			int periodsHeldInYear = toPeriod - fromPeriod + 1;

			decimal netValue = Params.DepreciationBasis - Params.AccumulatedDepreciation;
			decimal shouldBeAccumulatedInYear = netValue * Params.PercentPerYear / 100 * periodsHeldInYear / Params.DepreciationPeriodsInYear;
			decimal prevYearAccumulatedDepreciation = Params.AccumulatedDepreciation;

			for (int i = fromPeriod; i <= depreciateToPeriod; i++)
			{
				int daysOfPeriod = Params.GetFinPeriodLengthInDaysFebAlways28(year, i);
				string periodID = $"{Params.DepreciationStartYear + year - 1:D4}{i:D2}";
				decimal depreciation;
				bool useRounding = true;

				if (i == depreciateToPeriod)// last depreciation period in year
				{
					//fix year amount without rounding discrepancy
					decimal currentYearAccumulatedDepreciation = Params.AccumulatedDepreciation - prevYearAccumulatedDepreciation;
					decimal shouldBeAccumulatedInCurrentYear;
					if (periodID == Params.DepreciationStopBookPeriod.FinPeriodID // last depreciation period
						&& Params.RecoveryEndBookPeriod.FinPeriodID != Params.DepreciationStopBookPeriod.FinPeriodID) //disposal
					{
						int daysHeldInDisposalYear = Params.GetDaysHeldInYear(year, fromPeriod, depreciateToPeriod);
						shouldBeAccumulatedInCurrentYear = shouldBeAccumulatedInYear * daysHeldInDisposalYear / daysHeldInYear;
					}
					else
					{
						shouldBeAccumulatedInCurrentYear = shouldBeAccumulatedInYear;
					}
					depreciation = shouldBeAccumulatedInCurrentYear - currentYearAccumulatedDepreciation;
					useRounding = false;
				}
				else
				{
					depreciation = shouldBeAccumulatedInYear * daysOfPeriod / daysHeldInYear;
				}

				// Rounding
				decimal roundedDepreciation = Round(depreciation);
				if (roundedDepreciation != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
				{
					rounding += depreciation - roundedDepreciation;
				}

				Params.AccumulatedDepreciation += roundedDepreciation;

				WhereToWriteDepreciation(writeToAsset, null, assetBalance, year, i, roundedDepreciation, useRounding, ref rounding);
			}
		}

		private void SetNewZealandStraightLineEvenlyDepr(bool writeToAsset, int year, FABookBalance assetBalance, ref decimal rounding)
		{
			int fromPeriod;
			int toPeriod;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if (year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			int depreciateToPeriod = toPeriod;

			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			decimal periodsHeldInYear = depreciateToPeriod - fromPeriod + 1;
			decimal prevYearAccumulatedDepreciation = Params.AccumulatedDepreciation;

			for (int i = fromPeriod; i <= depreciateToPeriod; i++)
			{
				string periodID = $"{Params.DepreciationStartYear + year - 1:D4}{i:D2}";
				decimal depreciation;
				bool useRounding = true;

				if (periodID == Params.DepreciationStopBookPeriod.FinPeriodID // last depreciation period
					&& Params.RecoveryEndBookPeriod.FinPeriodID == Params.DepreciationStopBookPeriod.FinPeriodID) // non-disposal
				{
					depreciation = Params.DepreciationBasis - Params.AccumulatedDepreciation;
				}
				else if (i == depreciateToPeriod)// last depreciation period in year
				{
					//fix year amount without rounding discrepancy
					decimal currentYearAccumulatedDepreciation = Params.AccumulatedDepreciation - prevYearAccumulatedDepreciation;
					decimal shouldBeAccumulatedInCurrentYear = Params.DepreciationBasis * Params.PercentPerYear / 100 * (periodsHeldInYear / Params.DepreciationPeriodsInYear);
					depreciation = shouldBeAccumulatedInCurrentYear - currentYearAccumulatedDepreciation;
					useRounding = false;
				}
				else
				{
					depreciation = Params.DepreciationBasis * Params.PercentPerYear / 100 / Params.DepreciationPeriodsInYear;
				}

				// Rounding
				decimal roundedDepreciation = Round(depreciation);
				if (roundedDepreciation != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
				{
					rounding += depreciation - roundedDepreciation;
				}

				Params.AccumulatedDepreciation += roundedDepreciation;

				WhereToWriteDepreciation(writeToAsset, null, assetBalance, year, i, roundedDepreciation, useRounding, ref rounding);
			}
		}
		private void SetNewZealandDiminishingValueEvenlyDepr(bool writeToAsset, int year, FABookBalance assetBalance, ref decimal rounding)
		{
			int fromPeriod;
			int toPeriod;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if (year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			int depreciateToPeriod = toPeriod;

			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			decimal periodsHeldInYear = depreciateToPeriod - fromPeriod + 1;
			decimal netValue = Params.DepreciationBasis - Params.AccumulatedDepreciation;
			decimal prevYearAccumulatedDepreciation = Params.AccumulatedDepreciation;

			for (int i = fromPeriod; i <= depreciateToPeriod; i++)
			{
				decimal depreciation;
				bool useRounding = true;

				if (i == depreciateToPeriod)// last depreciation period in year
				{
					//fix year amount without rounding discrepancy
					decimal currentYearAccumulatedDepreciation = Params.AccumulatedDepreciation - prevYearAccumulatedDepreciation;
					decimal shouldBeAccumulatedInCurrentYear = netValue * Params.PercentPerYear / 100 * (periodsHeldInYear / Params.DepreciationPeriodsInYear);
					depreciation = shouldBeAccumulatedInCurrentYear - currentYearAccumulatedDepreciation;

					useRounding = false;
				}
				else
				{
					depreciation = netValue * Params.PercentPerYear / 100 / Params.DepreciationPeriodsInYear;
				}

				// Rounding
				decimal roundedDepreciation = Round(depreciation);
				if (roundedDepreciation != (Params.DepreciationBasis - Params.AccumulatedDepreciation))
				{
					rounding += depreciation - roundedDepreciation;
				}

				Params.AccumulatedDepreciation += roundedDepreciation;

				WhereToWriteDepreciation(writeToAsset, null, assetBalance, year, i, roundedDepreciation, useRounding, ref rounding);
			}
		}

		private void SetYDDepr(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal rounding, ref DateTime previousEndDate)
		{
			if (yearlyAccountancy != true) return;

			decimal wholeYears = Params.WholeRecoveryPeriods / Params.DepreciationPeriodsInYear;
			decimal	sumOfYears = wholeYears * (wholeYears + 1) / 2m;
			decimal remainingYears = wholeYears - Params.YearOfUsefulLife + 1m;
			decimal depreciation;
	
			int fromPeriod;
			int toPeriod;

			decimal yearDepreciation1 = Params.DepreciationBasis * remainingYears  /  sumOfYears;
			decimal yearDepreciation2 = Params.DepreciationBasis * (remainingYears - 1) / sumOfYears;

			if (year == 1 && year < Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.DepreciationPeriodsInYear;	
			}
			else if (year == 1 && year == Params.RecoveryYears)
			{
				fromPeriod = Params.DepreciationStartPeriod;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else if(year == Params.RecoveryYears)
			{
				fromPeriod = 1;
				toPeriod = Params.RecoveryEndPeriod;
			}
			else
			{
				fromPeriod = 1;
				toPeriod = Params.DepreciationPeriodsInYear;
			}
			
			if (year == Params.DepreciationYears)
			{
				toPeriod = Params.DepreciationStopPeriod;
			}

			bool edgeOfUsefulYear = (year == 1);
			switch (Params.AveragingConvention)
			{
				case FAAveragingConvention.FullDay:
					int yearDays = 0;
					DateTime periodPreviousEndDate = previousEndDate;
					for(int i = fromPeriod; i <= toPeriod; i++)
					{
						yearDays += Params.GetDaysOnPeriod(Params.DepreciationStartDate, Params.RecoveryEndDate, Params.DepreciationStartYear + year - 1, i, ref previousEndDate);
					}
					previousEndDate = periodPreviousEndDate;
						
					for(int i = fromPeriod; i <= toPeriod; i++)
					{
						if (Params.DepreciationBasis == Params.AccumulatedDepreciation) return;
						if (Params.AveragingConvention == FAAveragingConvention.FullDay)
						{
							if(edgeOfUsefulYear == false) 
							{
								edgeOfUsefulYear = (i == Params.DepreciationStartPeriod);
								Params.YearOfUsefulLife += edgeOfUsefulYear ? 1 : 0;
							}

							int periodDays = Params.GetDaysOnPeriod(Params.DepreciationStartDate, Params.DepreciationStopDate, Params.DepreciationStartYear + year - 1, i, ref previousEndDate);
							depreciation = (wholeYears - remainingYears + 1m == Params.YearOfUsefulLife ? yearDepreciation1 : yearDepreciation2) * periodDays / yearDays;
							Params.AccumulatedDepreciation += Round(depreciation);
							rounding += depreciation - Round(depreciation);
							WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
						}
					}
					break;
				case FAAveragingConvention.FullPeriod:
					for(int i = fromPeriod; i <= toPeriod; i++)
					{	
						if(edgeOfUsefulYear == false) 
						{
							edgeOfUsefulYear = (i == Params.DepreciationStartPeriod);
							Params.YearOfUsefulLife += edgeOfUsefulYear ? 1 : 0;
						}

						depreciation = (wholeYears - remainingYears + 1m == Params.YearOfUsefulLife ? yearDepreciation1 : yearDepreciation2) / Params.DepreciationPeriodsInYear;
						Params.AccumulatedDepreciation += Round(depreciation);
						rounding += depreciation - Round(depreciation);
						WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
					}
					break;
			}
		}

		private void SetSLDeprHalfPeriodFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			decimal firstDepreciation = Params.DepreciationBasis / (Params.WholeRecoveryPeriods - 1) / 2m;
			lastDepreciation = firstDepreciation;

			decimal checkZero = (Params.WholeRecoveryPeriods - 2);
			rounding = Params.DepreciationBasis - Round(firstDepreciation) * 2m;
			if (checkZero > 0)
			{
				otherDepreciation = (Params.DepreciationBasis - firstDepreciation * 2m) / checkZero;
				rounding -= Round(otherDepreciation) * checkZero;
			}
			int depreciateToPeriod = Params.DepreciationPeriodsInYear;
			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			for(int i = Params.DepreciationStartPeriod; i <= depreciateToPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, Params.DepreciationStartPeriod == i ? firstDepreciation: otherDepreciation, i > Params.DepreciationStartPeriod, ref rounding);
			}
		}
		private void SetSLDeprHalfPeriodFirstYearEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)
		{
			decimal firstDepreciation = Params.DepreciationBasis / (Params.WholeRecoveryPeriods - 1) / 2m;

			decimal checkZero = (Params.WholeRecoveryPeriods - 2);
			if (checkZero > 0 )
			otherDepreciation = (Params.DepreciationBasis - firstDepreciation * 2) / checkZero;

			rounding = Params.DepreciationBasis - Round(firstDepreciation) * 2;

			if (checkZero > 0 )
				rounding -= Round(otherDepreciation) * checkZero;

			for (int i = Params.DepreciationStartPeriod; i <= Params.DepreciationStopPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, Params.DepreciationStartPeriod == i || Params.RecoveryEndPeriod == i ? firstDepreciation : otherDepreciation, i > Params.DepreciationStartPeriod, ref rounding);
			}
		}
		private void SetSLDeprHalfPeriodLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			for (int i = 1; i <= Params.DepreciationStopPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, Params.RecoveryEndPeriod == i ? lastDepreciation : otherDepreciation, true, ref rounding);
			}
		}

		private void SetSLDeprHalfQuarterFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			// use only with metric 12
			decimal deprStartPeriods = (Params.LastDepreciationStartQuarterPeriod - Params.DepreciationStartPeriod + 1);
			decimal deprEndPeriods = (Params.RecoveryEndPeriod - Params.FirstRecoveryEndQuarterPeriod + 1);

			decimal firstDepreciation = Params.DepreciationBasis / (Params.RecoveryYears - 1) / DeprCalcParameters.QuartersInYear / 2m / deprStartPeriods;
			lastDepreciation = Params.DepreciationBasis / (Params.RecoveryYears - 1) / 2m / DeprCalcParameters.QuartersInYear / deprEndPeriods;

			decimal checkZero = Params.WholeRecoveryPeriods - deprStartPeriods - deprEndPeriods;
			if (checkZero > 0 )
				otherDepreciation = (Params.DepreciationBasis - firstDepreciation * deprStartPeriods - lastDepreciation * deprEndPeriods) / checkZero;

			rounding = Params.DepreciationBasis - Round(firstDepreciation) * deprStartPeriods;
			rounding -= Round(lastDepreciation) * deprEndPeriods;
			if (checkZero > 0)
				rounding -= Round(otherDepreciation) * checkZero;

			int depreciateToPeriod = Params.DepreciationPeriodsInYear;
			if (year == Params.DepreciationYears)
				depreciateToPeriod = Params.DepreciationStopPeriod;

			for(int i = Params.DepreciationStartPeriod; i <= depreciateToPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i,
					Params.DepreciationStartPeriod <= i && Params.LastDepreciationStartQuarterPeriod >= i
						? firstDepreciation
						: otherDepreciation,
					i > Params.DepreciationStartPeriod, ref rounding);
			}
		}
		private void SetSLDeprHalfQuarterLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			for (int i = 1; i <= Params.DepreciationStopPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, (Params.RecoveryEndPeriod >= i && Params.FirstRecoveryEndQuarterPeriod <= i) ? lastDepreciation : otherDepreciation, true, ref rounding);
			}
		}

		private void SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			decimal periods = Params.WholeRecoveryPeriods + Params.StartDepreciationMidPeriodRatio + Params.StopDepreciationMidPeriodRatio - 2;
			decimal firstDepreciation = Params.DepreciationBasis / periods * Params.StartDepreciationMidPeriodRatio;
			lastDepreciation = Params.DepreciationBasis / periods * Params.StopDepreciationMidPeriodRatio;

			rounding  = Params.DepreciationBasis - Round(firstDepreciation);
			rounding -= Round(lastDepreciation);

			decimal checkZero = Params.WholeRecoveryPeriods - 2;
				if (checkZero > 0 )
			{
				otherDepreciation = (Params.DepreciationBasis - firstDepreciation - lastDepreciation) / checkZero;
					rounding -= Round(otherDepreciation) * checkZero;
			}

			int depreciateToPeriod = Params.DepreciationPeriodsInYear;
			if (year == Params.DepreciationYears)
				depreciateToPeriod = Params.DepreciationStopPeriod;

			for(int i = Params.DepreciationStartPeriod; i <= depreciateToPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, Params.DepreciationStartPeriod == i ? firstDepreciation : (year == Params.DepreciationYears && i == Params.RecoveryEndPeriod) ? lastDepreciation : otherDepreciation, i > Params.DepreciationStartPeriod, ref rounding);
			}
		}

		private void SetSLDeprModifiedPeriodFirstYearEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			decimal periods = Params.WholeRecoveryPeriods + Params.StartDepreciationMidPeriodRatio + Params.StopDepreciationMidPeriodRatio - 2;
			decimal firstDepreciation = Params.DepreciationBasis / periods * Params.StartDepreciationMidPeriodRatio;
			lastDepreciation = Params.DepreciationBasis / periods * Params.StopDepreciationMidPeriodRatio;

			decimal checkZero = Params.WholeRecoveryPeriods - 2;
			if(checkZero > 0)
			otherDepreciation = (Params.DepreciationBasis - firstDepreciation - lastDepreciation) / checkZero;

			rounding = Params.DepreciationBasis - Round(firstDepreciation);
			rounding -= Round(lastDepreciation);

			if(checkZero > 0)
				rounding -= Round(otherDepreciation) * checkZero;

			for (int i = Params.DepreciationStartPeriod; i <= Params.DepreciationStopPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i,
					Params.DepreciationStartPeriod == i ? firstDepreciation : Params.RecoveryEndPeriod == i ? lastDepreciation : otherDepreciation,
					i > Params.DepreciationStartPeriod, ref rounding);
			}
		}

		private void SetSLDeprFullYearFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)
		{
			decimal firstDepreciation = otherDepreciation * Params.DepreciationPeriodsInYear / (Params.DepreciationPeriodsInYear - Params.DepreciationStartPeriod + 1);

			decimal checkZero = Params.WholeRecoveryPeriods - Params.DepreciationPeriodsInYear;
			if(checkZero > 0)
				otherDepreciation = (Params.DepreciationBasis - firstDepreciation * (Params.DepreciationPeriodsInYear - Params.DepreciationStartPeriod + 1)) / checkZero; 

			rounding  = Params.DepreciationBasis - Round(firstDepreciation) * (Params.DepreciationPeriodsInYear - Params.DepreciationStartPeriod + 1);

			if(checkZero > 0)
				rounding -= Round(otherDepreciation) * checkZero;

			int depreciateToPeriod = Params.DepreciationPeriodsInYear;
			if (year == Params.DepreciationYears)
				depreciateToPeriod = Params.DepreciationStopPeriod;

			for(int i = Params.DepreciationStartPeriod; i <= depreciateToPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, firstDepreciation, i > Params.DepreciationStartPeriod, ref rounding);
			}
		}
		private void SetSLDeprFullYearLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)
		{
			for (int i = 1; i <= Params.DepreciationStopPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
			}
		}

		private void SetSLDeprFullPeriodFirstYearNotEqualLastYear(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)
		{
			bool isTableMethod = yearlyAccountancy && line != null;
			if (isTableMethod)
			{
				otherDepreciation = (line.RatioPerYear ?? 0m) * Params.DepreciationBasis / (Params.DepreciationPeriodsInYear - Params.DepreciationStartPeriod + 1);	
			}
			else
			{
				rounding = Params.DepreciationBasis - Round(otherDepreciation) * Params.WholeRecoveryPeriods;
			}

			int depreciateToPeriod = Params.DepreciationPeriodsInYear;
			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			for(int i = Params.DepreciationStartPeriod; i <= depreciateToPeriod; i++)
			{
				if (isTableMethod)
				{
					Params.AccumulatedDepreciation += Round(otherDepreciation);
					rounding += otherDepreciation - Round(otherDepreciation);
				}

				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, 
					Params.DepreciationStartPeriod >= Params.DepreciationPeriodsInYear || i > Params.DepreciationStartPeriod || yearlyAccountancy, ref rounding);
			}
		}
		private void SetSLDeprFullPeriodFirstYearEqualLastYear(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)	
		{
			bool isTableMethod = yearlyAccountancy && line != null;
			if (isTableMethod)
			{
				otherDepreciation = (line.RatioPerYear ?? 0m) * Params.DepreciationBasis / Params.WholeRecoveryPeriods;	
			}
			else
			{
				rounding = Params.DepreciationBasis - Round(otherDepreciation) * Params.WholeRecoveryPeriods;
			}

			for(int i = Params.DepreciationStartPeriod; i <= Params.DepreciationStopPeriod; i++)
			{
				if (isTableMethod)
				{
					Params.AccumulatedDepreciation += Round(otherDepreciation);
					rounding += otherDepreciation - Round(otherDepreciation);
				}

				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, i > Params.DepreciationStartPeriod || (yearlyAccountancy), ref rounding);
			}
		}
		private void SetSLDeprFullPeriodLastYear(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)	
		{
			bool isTableMethod = yearlyAccountancy && line != null;
			if (isTableMethod)
			{
				otherDepreciation = (line.RatioPerYear ?? 0m) * Params.DepreciationBasis / Params.RecoveryEndPeriod;	
			}

			for(int i = 1; i <= Params.DepreciationStopPeriod; i++)
			{
				if (isTableMethod)
				{
					Params.AccumulatedDepreciation += Round(otherDepreciation);
					rounding += otherDepreciation - Round(otherDepreciation);
				}

				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
			}	
		}

		private void SetSLDeprFullQuarterFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, decimal yearDepreciation, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			// use only with metric 12
			decimal firstDepreciation = yearDepreciation / DeprCalcParameters.QuartersInYear / (Params.LastDepreciationStartQuarterPeriod - Params.DepreciationStartPeriod + 1);
			decimal checkZero = Params.WholeRecoveryPeriods - DeprCalcParameters.MonthsInQuarter;
			if (checkZero > 0)
			{
				otherDepreciation = (Params.DepreciationBasis - firstDepreciation * (Params.LastDepreciationStartQuarterPeriod - Params.DepreciationStartPeriod + 1)) / checkZero;
			}

			rounding  = Params.DepreciationBasis - Round(firstDepreciation) * (Params.LastDepreciationStartQuarterPeriod - Params.DepreciationStartPeriod + 1);
			if (checkZero > 0)
			{
				rounding -= Round(otherDepreciation) * checkZero;
			}

			int depreciateToPeriod = Params.DepreciationPeriodsInYear;

			if (year == Params.DepreciationYears)
			{
				depreciateToPeriod = Params.DepreciationStopPeriod;
			}

			for(int i = Params.DepreciationStartPeriod; i <= depreciateToPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i,
					Params.DepreciationStartPeriod <= i && Params.LastDepreciationStartQuarterPeriod >= i
						? firstDepreciation
						: otherDepreciation,
					i > Params.DepreciationStartPeriod, ref rounding);
			}
		}
		private void SetSLDeprFullQuarterLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)
		{
			for (int i = 1; i <= Params.DepreciationStopPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
			}
		}

		private void SetSLDeprHalfYearFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
		{
			decimal deprStartPeriods = Params.DepreciationPeriodsInYear - Params.DepreciationStartPeriod + 1;
			decimal deprEndPeriods = Params.RecoveryEndPeriod;

			decimal firstDepreciation = Params.DepreciationBasis / (Params.RecoveryYears - 1) / 2m / deprStartPeriods;
			lastDepreciation = Params.DepreciationBasis / (Params.RecoveryYears - 1) / 2m / deprEndPeriods;

			decimal checkZero = Params.WholeRecoveryPeriods - deprStartPeriods - deprEndPeriods;
			if(checkZero > 0)
				otherDepreciation = (Params.DepreciationBasis - firstDepreciation * deprStartPeriods - lastDepreciation * deprEndPeriods) / checkZero;

			rounding = Params.DepreciationBasis - Round(firstDepreciation) * deprStartPeriods;
			rounding -= Round(lastDepreciation) * deprEndPeriods;

			if(checkZero > 0)
				rounding -= Round(otherDepreciation) * checkZero;

			int depreciateToPeriod = Params.DepreciationPeriodsInYear;
			if (year == Params.DepreciationYears)
				depreciateToPeriod = Params.DepreciationStopPeriod;

			for(int i = Params.DepreciationStartPeriod; i <= depreciateToPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, firstDepreciation, i > Params.DepreciationStartPeriod, ref rounding);
			}
		}
		private void SetSLDeprHalfYearLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal lastDepreciation, ref decimal rounding)
		{
			for (int i = 1; i <= Params.DepreciationStopPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, lastDepreciation, true, ref rounding);
			}
		}

		private void SetSLDeprOtherYears(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)	
		{
			int depreciateToPeriod = Params.DepreciationPeriodsInYear;
			if (year == Params.DepreciationYears)
				depreciateToPeriod = Params.DepreciationStopPeriod;

			for(int i = 1; i <= depreciateToPeriod; i++)
			{
				WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
			}
		}
		#endregion

		public DeprCalcParameters Params;

		private int previousCalculatedPeriods; // used only in SetDBDepr

		#region FARefactoring
		public virtual void Calculate(FABookBalance assetBalance, string maxPeriodID = null, PXGraph uiGraph = null)
		{
			if (!PerformCalculation(assetBalance, maxPeriodID, uiGraph))
			{
				CalculateDepreciation(assetBalance, maxPeriodID);
			}
		}

		protected virtual IEnumerable<DepreciationMethodBase> GetDepreciationMethods()
		{
			yield return new StraightLineFullPeriodMethod();
			//yield break;
		}

		public DepreciationMethodBase GetSuitableDepreciationMethod(FABookBalance bookBalance)
		{
			IncomingCalculationParameters incomingParameters = new IncomingCalculationParameters(this, bookBalance);
			if (incomingParameters.Method.IsTableMethod == true)
				return null;

			DepreciationMethodBase[] suitableMethods = GetDepreciationMethods().Where(method => method.IsSuitable(incomingParameters)).ToArray();
			if (suitableMethods.Length > 1)
			{
				throw new PXException(
					Messages.SeveralCompetingDepreciationMethods,
					incomingParameters.CalculationMethod,
					incomingParameters.AveragingConvention,
					string.Join(", ", suitableMethods.Select(methodExtension => methodExtension.GetType().Name)));
			}
			return suitableMethods.FirstOrDefault();
		}

		private bool PerformCalculation(FABookBalance bookBalance, string maxPeriodID = null, PXGraph uiGraph = null)
		{
			if (bookBalance.Depreciate != true)
			{
				return true;
			}

			CheckBalance(bookBalance);
			
			DepreciationMethodBase depreciationMethod = GetSuitableDepreciationMethod(bookBalance);
			if (depreciationMethod == null)
			{
				return false;
				// TODO: throw an exception after obsolete engine elimination
			}

			FAClass assetClass = SelectFrom<FAClass>
				.Where<FAClass.assetID.IsEqual<@P.AsInt>>
				.View
				.Select(this, depreciationMethod.IncomingParameters.FixedAsset.ClassID);
			if(depreciationMethod.IsStraightLine && assetClass.AcceleratedDepreciation == true)
			{
				PXSetPropertyException exception = new PXSetPropertyException(
					Messages.AcceleratedDepreciationFlagIsIrrelevant,
					PXErrorLevel.RowWarning,
					depreciationMethod.IncomingParameters.Method.MethodCD);
				if(uiGraph != null)
				{
					uiGraph.Caches<FABookBalance>().RaiseExceptionHandling<FABookBalance.depreciationMethodID>(bookBalance, null, exception);
				}
				else
				{
					PXProcessing<FABookBalance>.SetWarning(exception);
				}
			}

			ICollection<FADepreciationScheduleItem> depreciationSchedule = CalculateDepreciation(depreciationMethod, maxPeriodID);
			CalculationParameters calculationParameters = depreciationMethod.CalculationParameters;

			FillCalculatedHistory(
				bookBalance,
				depreciationSchedule,
				CalculateDepreciationOfAmount<FABookBalance.tax179Amount>(depreciationMethod, maxPeriodID),
				CalculateDepreciationOfAmount<FABookBalance.bonusAmount>(depreciationMethod, maxPeriodID),
				maxPeriodID
			);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				TrimExtraHistory(depreciationMethod, calculationParameters);
				Save.Press();
				CloseHistoryInClosedPeriods(depreciationMethod);
				ts.Complete();
			}
			return true;
		}

		private void TrimExtraHistory(DepreciationMethodBase depreciationMethod, CalculationParameters calculationParameters)
		{
			FABookBalance bookBalance = depreciationMethod.IncomingParameters.BookBalance;
			FADetails details = depreciationMethod.IncomingParameters.Details;

			string minDepreciateFromPeriodID = bookBalance.DeprFromPeriod;
			if (calculationParameters != null
				&& calculationParameters.Additions != null)
			{
				foreach (FAAddition addition in calculationParameters.Additions)
				{
					string additionDepreciateFromPeriodID = addition?.CalculatedAdditionParameters?.DepreciateFromPeriodID;
					if (!string.IsNullOrEmpty(additionDepreciateFromPeriodID)
						&& string.CompareOrdinal(minDepreciateFromPeriodID, additionDepreciateFromPeriodID) > 0)
					{
						minDepreciateFromPeriodID = additionDepreciateFromPeriodID;
					}
				}
			}

			string receiptPeriodID = 
				FABookPeriodRepository.GetFABookPeriodIDOfDate(details.ReceiptDate, bookBalance.BookID, bookBalance.AssetID, false) 
				?? minDepreciateFromPeriodID;
			PXDatabase.Delete<FABookHistory>(
				new PXDataFieldRestrict<FABookHistory.assetID>(PXDbType.Int, 4, bookBalance.AssetID, PXComp.EQ),
				new PXDataFieldRestrict<FABookHistory.bookID>(PXDbType.Int, 4, bookBalance.BookID, PXComp.EQ),
				new PXDataFieldRestrict<FABookHistory.finPeriodID>(PXDbType.Char, 6, FinPeriodUtils.Min(receiptPeriodID, minDepreciateFromPeriodID), PXComp.LT));

			if (string.CompareOrdinal(bookBalance.LastDeprPeriod, bookBalance.DeprToPeriod) < 0)
			{
				PXDatabase.Delete<FABookHistory>(
					new PXDataFieldRestrict<FABookHistory.assetID>(PXDbType.Int, 4, bookBalance.AssetID, PXComp.EQ),
					new PXDataFieldRestrict<FABookHistory.bookID>(PXDbType.Int, 4, bookBalance.BookID, PXComp.EQ),
					new PXDataFieldRestrict<FABookHistory.finPeriodID>(PXDbType.Char, 6, bookBalance.DeprToPeriod, PXComp.GT));
			}
		}

		private void CloseHistoryInClosedPeriods(DepreciationMethodBase depreciationMethod)
		{
			FABookBalance bookBalance = depreciationMethod.IncomingParameters.BookBalance;

			if (bookBalance.UpdateGL == true)
			{
				string maxClosedPeriod = null;
				foreach (FABookHistory hist in PXSelectJoin<
					FABookHistory,
					InnerJoin<FixedAsset,
						On<FixedAsset.assetID, Equal<FABookHistory.assetID>>,
					InnerJoin<Branch,
						On<Branch.branchID, Equal<FixedAsset.branchID>>,
					InnerJoin<OrganizationFinPeriod,
						On<OrganizationFinPeriod.organizationID, Equal<Branch.organizationID>,
							And<OrganizationFinPeriod.finPeriodID, Equal<FABookHistory.finPeriodID>>>>>>,
					Where<FABookHistory.closed, NotEqual<True>,
						And<OrganizationFinPeriod.fAClosed, Equal<True>,
						And<FABookHistory.assetID, Equal<Current<FABookBalance.assetID>>,
						And<FABookHistory.bookID, Equal<Current<FABookBalance.bookID>>>>>>>
					.SelectMultiBound(this, new object[] { bookBalance }))
				{
					FABookHist accuhist = new FABookHist
					{
						AssetID = bookBalance.AssetID,
						BookID = bookBalance.BookID,
						FinPeriodID = hist.FinPeriodID
					};

					accuhist = BookHistory.Insert(accuhist);

					accuhist.Closed = true;

					if (maxClosedPeriod == null || string.CompareOrdinal(hist.FinPeriodID, maxClosedPeriod) > 0)
					{
						maxClosedPeriod = hist.FinPeriodID;
					}
				}

				AssetProcess.SetLastDeprPeriod(BookBalance, bookBalance, maxClosedPeriod);
				AssetProcess.AdjustFixedAssetStatus(this, bookBalance.AssetID);

				Save.Press();
			}
		}

		public virtual ICollection<FADepreciationScheduleItem> CalculateDepreciation(DepreciationMethodBase depreciationMethod, string maxPeriodID = null)
		{
			return depreciationMethod.CalculateDepreciation(maxPeriodID)
				.Round(
					item => item.DepreciationAmount, 
					(item, value) => item.DepreciationAmount = value, 
					depreciationMethod.IncomingParameters.Precision,
					PXRounder.SpreadType.Flow);
		}

		protected virtual ICollection<FADepreciationScheduleItem> CalculateDepreciationOfAmount<TField>(
			DepreciationMethodBase depreciationMethod,
			string maxPeriodID = null,
			bool considerExistenceIncomingAdditions = true)
			where TField : IBqlField
		{
			#region Contracts
			if (depreciationMethod.IncomingParameters == null)
			{
				throw new ArgumentNullException(nameof(depreciationMethod.IncomingParameters));
			}
			if (depreciationMethod.IncomingParameters.Graph == null)
			{
				throw new ArgumentNullException(nameof(depreciationMethod.IncomingParameters.Graph));
			}
		
			FABookBalance bookBalance = depreciationMethod.IncomingParameters.BookBalance;
			if (bookBalance == null)
			{
				throw new ArgumentNullException(nameof(BookBalance));
			}
			#endregion		

			ICollection<FADepreciationScheduleItem> emptySchedule = new FADepreciationScheduleItem[]{};

			if (considerExistenceIncomingAdditions
			    && (depreciationMethod.IncomingParameters.Additions == null
			        || depreciationMethod.IncomingParameters.Additions.IsEmpty()))
			{
				return emptySchedule;
			}

			PXCache balanceCache = depreciationMethod.IncomingParameters.Graph.Caches<FABookBalance>();
			decimal? amountValue = (decimal?)balanceCache.GetValue<TField>(bookBalance);

			if (amountValue == null || amountValue == 0m) return emptySchedule;

			CalculationParameters calculationParameters = new CalculationParameters(depreciationMethod.IncomingParameters, maxPeriodID)
			{
				Additions = new List<FAAddition>
				{
					new FAAddition(
						amountValue.Value,
						bookBalance.DeprFromPeriod,
						bookBalance.DeprFromDate.Value,
						depreciationMethod.IncomingParameters.Precision)
				},
			};
			return depreciationMethod.CalculateDepreciation(calculationParameters)
				.Round(
					item => item.DepreciationAmount, 
					(item, value) => item.DepreciationAmount = value, 
					depreciationMethod.IncomingParameters.Precision,
					PXRounder.SpreadType.Flow);
		}

		protected virtual void FillCalculatedHistory(
			FABookBalance bookBalance,
			ICollection<FADepreciationScheduleItem> depreciationSchedule,
			ICollection<FADepreciationScheduleItem> section179Schedule,
			ICollection<FADepreciationScheduleItem> bonusSchedule,
			string maxPeriodID = null)
		{
			ClearCalculatedHistory(bookBalance);

			if (depreciationSchedule == null || depreciationSchedule.IsEmpty()) return;

			if (string.IsNullOrEmpty(maxPeriodID))
			{
				maxPeriodID = bookBalance.DeprToPeriod;
			}

			#region Bonus and Section 179 calculated
			FABookHist history = new FABookHist
			{
				AssetID = bookBalance.AssetID,
				BookID = bookBalance.BookID,
				FinPeriodID = bookBalance.DeprFromPeriod
			};
			history = (FABookHist)this.Caches<FABookHist>().Insert(history);
				
			history.PtdBonusCalculated += bookBalance.BonusAmount;
			history.YtdBonusCalculated += bookBalance.BonusAmount;
			history.PtdTax179Calculated += bookBalance.Tax179Amount;
			history.YtdTax179Calculated += bookBalance.Tax179Amount;

			depreciationSchedule.First().DepreciationAmount += (history.PtdBonusCalculated ?? 0m) + (history.PtdTax179Calculated ?? 0m);
			#endregion
	
			foreach (FADepreciationScheduleItem depreciation in depreciationSchedule)
			{
				if (string.CompareOrdinal(depreciation.FinPeriodID, maxPeriodID) > 0) break;
				
				history = new FABookHist
				{
					AssetID = bookBalance.AssetID,
					BookID = bookBalance.BookID,
					FinPeriodID = depreciation.FinPeriodID
				};
				history = (FABookHist)this.Caches<FABookHist>().Insert(history);
				
				history.PtdCalculated += depreciation.DepreciationAmount;
				history.YtdCalculated += depreciation.DepreciationAmount;

			}
			
			foreach (FADepreciationScheduleItem section179 in section179Schedule)
			{
				if (string.CompareOrdinal(section179.FinPeriodID, maxPeriodID) > 0) break;

				history = new FABookHist
				{
					AssetID = bookBalance.AssetID,
					BookID = bookBalance.BookID,
					FinPeriodID = section179.FinPeriodID
				};
				history = (FABookHist)this.Caches<FABookHist>().Insert(history);
				
				history.PtdTax179Taken += section179.DepreciationAmount;
				history.YtdTax179Taken += section179.DepreciationAmount;
			}

			foreach (FADepreciationScheduleItem bonus in bonusSchedule)
			{
				if (string.CompareOrdinal(bonus.FinPeriodID, maxPeriodID) > 0) break;

				history = new FABookHist
				{
					AssetID = bookBalance.AssetID,
					BookID = bookBalance.BookID,
					FinPeriodID = bonus.FinPeriodID
				};
				history = (FABookHist)this.Caches<FABookHist>().Insert(history);
				
				history.PtdBonusTaken += bonus.DepreciationAmount;
				history.YtdBonusTaken += bonus.DepreciationAmount;
			}
		}

		protected virtual void ClearCalculatedHistory(FABookBalance bookBalance)
		{
			// We should clear all Ptd/Ytd Calculated amounts for each Book in the FABookHistory table
			// to guarantee correct calculation for each period if DeprFromPeriod value has been changed,
			// or Fixed Asset has been disposed/splitted e.t.c.
			//
			PXUpdate<
				Set<FABookHistory.ptdTax179Taken, decimal0,
				Set<FABookHistory.ytdTax179Taken, decimal0,
				Set<FABookHistory.ptdBonusTaken, decimal0,
				Set<FABookHistory.ytdBonusTaken, decimal0,
				Set<FABookHistory.ptdTax179Calculated, decimal0,
				Set<FABookHistory.ytdTax179Calculated, decimal0,
				Set<FABookHistory.ptdBonusCalculated, decimal0,
				Set<FABookHistory.ytdBonusCalculated, decimal0,
				Set<FABookHistory.ptdCalculated, decimal0,
				Set<FABookHistory.ytdCalculated, decimal0>>>>>>>>>>, 
				FABookHistory,
				Where<FABookHistory.assetID, Equal<Required<FABookHistory.assetID>>,
					And<FABookHistory.bookID, Equal<Required<FABookHistory.bookID>>>>>
				.Update(this, bookBalance.AssetID, bookBalance.BookID);
		}

		public virtual void CheckBalance(FABookBalance bookBalance)
		{
			if (bookBalance.Depreciate == true 
				&& (string.IsNullOrEmpty(bookBalance.DeprFromPeriod) 
					|| string.IsNullOrEmpty(bookBalance.DeprToPeriod) 
					|| string.CompareOrdinal(bookBalance.DeprFromPeriod, bookBalance.DeprToPeriod) > 0))
			{
				FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Current<FABookBalance.bookID>>>>.SelectSingleBound(this, new object[] { bookBalance });
				throw new PXException(Messages.IncorrectDepreciationPeriods, book.BookCode);
			}
		}
		#endregion
	}

	public class DeprCalcParameters
	{
		public DeprCalcParameters Fill(PXGraph graph, FABookBalance balance, FABook book = null, FADepreciationMethod depreciationMethod = null, DateTime? recoveryEndDate = null)
		{
			if (balance.BookID == null || balance.DeprFromDate == null)
			{
				return this;
			}

			return Fill(
				graph,
				balance.BookID.Value,
				balance.AssetID.Value,
				balance.DepreciationMethodID,
				balance.Depreciate == true,
				balance.UsefulLife ?? 0m,
				balance.PercentPerYear ?? 0m,
				balance.AveragingConvention,
				balance.MidMonthType,
				balance.MidMonthDay ?? 0,
				balance.DeprFromDate.Value,
				balance.DeprToDate,
				recoveryEndDate,
				book,
				depreciationMethod,
				balance.DeprFromYear,
				balance.DeprToYear);
		}

		[Obsolete(Common.Messages.MethodIsObsoleteRemoveInLaterAcumaticaVersions)]
		public DeprCalcParameters Fill(
			PXGraph graph,
			int bookID,
			int assetID,
			int? methodID,
			bool depreciate,
			decimal usefulLife,
			string averagingConvention,
			string midMonthType,
			short midMonthDay,
			DateTime depreciationStartDate,
			DateTime? depreciationStopDate,
			DateTime? recoveryEndDate = null,
			FABook book = null,
			FADepreciationMethod depreciationMethod = null,
			string deprFromYear = null,
			string deprToYear = null)
		{
			return Fill(graph,
				bookID,
				assetID,
				methodID,
				depreciate,
				usefulLife,
				0m,
				averagingConvention,
				midMonthType,
				midMonthDay,
				depreciationStartDate,
				depreciationStopDate,
				recoveryEndDate,
				book,
				depreciationMethod,
				deprFromYear,
				deprToYear);
		}

		public DeprCalcParameters Fill(
			PXGraph graph,
			int bookID,
			int assetID,
			int? methodID,
			bool depreciate,
			decimal usefulLife,
			decimal percentPerYear,
			string averagingConvention,
			string midMonthType,
			short midMonthDay,
			DateTime depreciationStartDate,
			DateTime? depreciationStopDate,
			DateTime? recoveryEndDate = null,
			FABook book = null,
			FADepreciationMethod depreciationMethod = null,
			string deprFromYear = null,
			string deprToYear = null)
		{
			return Init(
				bookID, 
				assetID,
				methodID, 
				depreciate, 
				usefulLife, 
				percentPerYear,
				averagingConvention, 
				midMonthType, midMonthDay, 
				depreciationStartDate, 
				depreciationStopDate, 
				recoveryEndDate, 
				book, 
				depreciationMethod,
				deprFromYear,
				deprToYear)
			.SelectFromDatabase(graph)
			.Calculate();
		}

		public const int MonthsInYear = 12;
		public const int MonthsInQuarter = 3;
		public const int QuartersInYear = 4;

		#region Incoming parameters
		public int BookID { get; set; }
		public int AssetID { get; set; }
		public int? DepreciationMethodID { get; set; }
		public decimal UsefulLife { get; set; }
		public decimal PercentPerYear { get; set; }
		public string AveragingConvention { get; set; }
		public string MidMonthType { get; set; }
		public short MidMonthDay { get; set; }
		public bool Depreciate { get; set; }
		protected DateTime? recoveryEndDate;
		protected string DeprToYear;
		protected string DeprFromYear;
		#endregion

		#region Selected parameters
		public FABook Book { get; set; }
		public FADepreciationMethod DepreciationMethod { get; set; }
		protected IYearSetup YearSetup { get; set; }
		protected SortedList<string, FABookPeriod> Periods;
		protected SortedList<int, FABookYear> Years;
		#endregion

		public IEnumerable<FABookPeriod> DeprPeriods
		{
			get
		{
				int startIdx = Periods.IndexOfKey(DepreciationStartBookPeriod.FinPeriodID);
				int stopIdx = Periods.IndexOfKey(DepreciationStopBookPeriod.FinPeriodID);
				for (int i = startIdx; i <= stopIdx; i++)
			{
					yield return Periods.Values[i];
				}
			}
		}

		#region Calculated parameters

		/// <summary>
		/// The integer field that contains the number of the period when depreciation has been started.
		/// The value of the field does not depend on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter.
		/// </summary>
		/// <value>
		/// Is calculated from the value of the <see cref="DeprCalcParameters.DepreciationStartBookPeriod"/> parameter.
		/// </value>
		public int DepreciationStartPeriod { get; set; }

		/// <summary>
		/// The integer field that contains the number of the period when depreciation is to be stopped.
		/// </summary>
		/// <value>
		/// Generally, the value is taken from the <see cref="DeprCalcParameters.RecoveryEndPeriod"/> parameter.
		/// But if the <see cref="DeprCalcParameters.DepreciationStopDate"/> parameter is overridden during 
		/// <see cref="DepreciationCalculation.CalculateDepreciationAddition"> calculation of depreciation additions</see>,
		/// the value is calculated from the <see cref="DeprCalcParameters.DepreciationStopBookPeriod"/> parameter.
		/// </value>
		public int DepreciationStopPeriod { get; set; }

		/// <summary>
		/// The date when depreciation was started.
		/// The value of the field depends on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter.
		/// </summary>
		/// <value>
		/// Is calculated from the value of the <see cref="DeprCalcParameters.RecoveryStartBookPeriod"/> parameter.
		/// The default value is taken from the <see cref="DeprCalcParameters.DepreciationStartDate"/> parameter.
		/// </value>
		public DateTime RecoveryStartDate { get; set; }

		/// <summary>
		/// The date when depreciation was stopped.
		/// </summary>
		/// <value>
		/// Generally, the value is calculated by the <see cref="DeprCalcParameters.GetDatePlusYears"/> method with 
		/// <see cref="DeprCalcParameters.RecoveryStartDate"/> and <see cref="DeprCalcParameters.UsefulLife"/> parameters.
		/// But if the <see cref="DeprCalcParameters.recoveryEndDate"/> parameter is overridden during 
		/// <see cref="DepreciationCalculation.CalculateDepreciationAddition"> calculation of depreciation additions</see>,
		/// the value is equal to the new value of this parameter.
		/// </value>
		public DateTime RecoveryEndDate { get; set; } = DateTime.MinValue;

		/// <summary>
		/// The period of a book when depreciation was started.
		/// The value of the field depends on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter.
		/// </summary>
		/// <value>
		/// Is calculated from the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter.
		/// The default value is taken from the <see cref="DeprCalcParameters.DepreciationStartBookPeriod"/> parameter.
		/// </value>
		public FABookPeriod RecoveryStartBookPeriod { get; set; }

		/// <summary>
		/// The period of a book when depreciation was stopped.
		/// </summary>
		/// <value>
		/// Is calculated from the value of the <see cref="DeprCalcParameters.RecoveryEndDate"/> parameter.
		/// </value>
		public FABookPeriod RecoveryEndBookPeriod { get; set; }

		/// <summary>
		/// The period of a book when depreciation is to be started.
		/// The value of the field does not depend on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter.
		/// </summary>
		/// <value>
		/// Is calculated from the value of the <see cref="DeprCalcParameters.DepreciationStartDate"/> parameter.
		/// </value>
		public FABookPeriod DepreciationStartBookPeriod { get; set; }

		/// <summary>
		/// The period of a book when depreciation is to be stopped.
		/// </summary>
		/// <value>
		/// Generally, the value is taken from the <see cref="DeprCalcParameters.RecoveryEndBookPeriod"/> parameter.
		/// But if the <see cref="DeprCalcParameters.DepreciationStopDate"/> parameter is overridden during 
		/// <see cref="DepreciationCalculation.CalculateDepreciationAddition"> calculation of depreciation additions</see>,
		/// the value is calculated from the new value of this parameter.
		/// </value>
		public FABookPeriod DepreciationStopBookPeriod { get; set; }

		/// <summary>
		/// The integer field that contains the year when depreciation is to be started.
		/// The value of the field does not depend on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter.
		/// </summary>
		/// <value>
		/// Is calculated from the value of the <see cref="DeprCalcParameters.DepreciationStartBookPeriod"/> parameter.
		/// </value>
		public int DepreciationStartYear { get; set; }

		/// <summary>
		/// The number of periods in the current year.
		/// This parameter can be changed dynamically during 
		/// <see cref="DepreciationCalculation.CalculateDepreciation(FADepreciationMethod, FABookBalance, DateTime?)">
		/// depreciation calculation process</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="FABookYear.FinPeriods"/> field.
		/// </value>
		public int DepreciationPeriodsInYear { get; set; } = 12;

		/// <summary>
		/// The depreciation basis.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="FABookBalance.YtdDeprBase"/> field.
		/// </value>
		public decimal DepreciationBasis { get; set; }

		/// <summary>
		/// The currently accumulated depreciation amount.
		/// This parameter can be changed dynamically during depreciation calculation process.
		/// </summary>
		public decimal AccumulatedDepreciation { get; set; }

		/// <summary>
		/// The integer field that specifies the period when depreciation was stopped.
		/// </summary>
		/// <value>
		/// Is calculated from the value of the <see cref="DeprCalcParameters.RecoveryEndBookPeriod"/> parameter.
		/// </value>
		public int RecoveryEndPeriod { get; set; }

		/// <summary>
		/// The integer field that specifies the number of the period from which the quarter 
		/// of the <see cref="DeprCalcParameters.RecoveryEndBookPeriod"/> period is started.
		/// This parameter is used only for the <see cref="FAAveragingConvention.FullQuarter"/> and 
		/// <see cref="FAAveragingConvention.HalfQuarter"/> averaging conventions.
		/// </summary>
		/// <value>
		/// The value of the field depends on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter
		/// and calculated inside the <see cref="DeprCalcParameters.Calculate"/> method.
		/// </value>
		public int FirstRecoveryEndQuarterPeriod { get; set; }

		/// <summary>
		/// The integer field that specifies the number of the period from which the quarter 
		/// of the <see cref="DeprCalcParameters.DepreciationStopBookPeriod"/> period is started.
		/// This parameter is used only for the <see cref="FAAveragingConvention.FullQuarter"/> and
		/// <see cref="FAAveragingConvention.HalfQuarter"/> averaging conventions.
		/// </summary>
		/// <value>
		/// The value of the field depends on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter
		/// and calculated inside the <see cref="DeprCalcParameters.Calculate"/> method.
		/// </value>
		public int FirstDepreciationStopQuarterPeriod { get; set; }

		/// <summary>
		/// The integer field that specifies the number of the period by which the quarter 
		/// of the <see cref="DeprCalcParameters.DepreciationStartBookPeriod"/> period is finished.
		/// This parameter is used only for the <see cref="FAAveragingConvention.FullQuarter"/> and 
		/// <see cref="FAAveragingConvention.HalfQuarter"/> averaging conventions.
		/// </summary>
		/// <value>
		/// The value of the field depends on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter
		/// and calculated inside the <see cref="DeprCalcParameters.Calculate"/> method.
		/// </value>
		public int LastDepreciationStartQuarterPeriod { get; set; }

		/// <summary>
		/// The number of years between <see cref="DeprCalcParameters.RecoveryEndBookPeriod"/> and
		/// <see cref="DeprCalcParameters.DepreciationStartBookPeriod"/> periods.
		/// </summary>
		/// <value>
		/// The value of the field depends on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter
		/// and calculated inside the <see cref="DeprCalcParameters.Calculate"/> method.
		/// </value>
		public int RecoveryYears { get; set; }

		/// <summary>
		/// The number of years for depreciation.
		/// </summary>
		/// <value>
		/// Generally, the value is taken from the <see cref="DeprCalcParameters.RecoveryYears"/> parameter.
		/// But if the <see cref="DeprCalcParameters.DepreciationStopDate"/> parameter is overridden during 
		/// <see cref="DepreciationCalculation.CalculateDepreciationAddition"> calculation of depreciation additions</see>,
		/// the value is calculated as the number of years between the <see cref= "DeprCalcParameters.DepreciationStopBookPeriod"/> 
		/// and <see cref="DeprCalcParameters.DepreciationStartBookPeriod"/> periods.
		/// </value>
		public int DepreciationYears { get; set; }

		/// <summary>
		/// The decimal value that specifies which part of <see cref="DeprCalcParameters.DepreciationStartPeriod"/> should be used for calculations.
		/// This parameter is used only for <see cref="FAAveragingConvention.ModifiedPeriod"/> and 
		/// <see cref="FAAveragingConvention.ModifiedPeriod2"/> averaging conventions.
		/// </summary>
		/// <value>
		/// See the <see cref="DeprCalcParameters.Calculate"/> method.
		/// Possible values for <see cref="FAAveragingConvention.ModifiedPeriod"/> averaging convention:
		/// 0.5m, 1m
		/// Possible values for <see cref="FAAveragingConvention.ModifiedPeriod2"/> averaging convention:
		/// 0m, 1m
		/// </value>
		public decimal StartDepreciationMidPeriodRatio { get; set; }

		/// <summary>
		/// The decimal value that specifies which part of <see cref="DeprCalcParameters.DepreciationStopPeriod"/> should be used for calculations.
		/// This parameter is used only for <see cref="FAAveragingConvention.ModifiedPeriod"/> and 
		/// <see cref="FAAveragingConvention.ModifiedPeriod2"/> averaging conventions.
		/// </summary>
		/// <value>
		/// See the <see cref="DeprCalcParameters.Calculate"/> method.
		/// For the <see cref="FAAveragingConvention.ModifiedPeriod"/> averaging convention, the value is always equal to 
		/// the value of the <see cref="DeprCalcParameters.StartDepreciationMidPeriodRatio"/> parameter.
		/// For the <see cref="FAAveragingConvention.ModifiedPeriod2"/> averaging convention, the value is always equal to 1m.
		/// </value>
		public decimal StopDepreciationMidPeriodRatio { get; set; }

		/// <summary>
		/// The date when depreciation is to be started.
		/// The value of the field does not depend on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="FABookBalance.DeprFromDate"/> field.
		/// </value>
		public DateTime DepreciationStartDate { get; set; }

		/// <summary>
		/// The date when depreciation is to be stopped.
		/// </summary>
		/// <value>
		/// Generally, the value is taken from the <see cref="DeprCalcParameters.RecoveryEndDate"/> parameter.
		/// The value can be overridden during <see cref= "DepreciationCalculation.CalculateDepreciationAddition" > calculation for depreciation additions</see>.
		/// In this case, the <see cref="DeprCalcParameters.DepreciationStopBookPeriod"/> parameter is calculated from this new value.
		/// </value>
		public DateTime? DepreciationStopDate { get; set; }

		/// <summary>
		/// The number of days between <see cref="DeprCalcParameters.RecoveryStartDate"/> and
		/// <see cref="DeprCalcParameters.RecoveryStartDate"/> dates.
		/// This parameter is used only for <see cref="FAAveragingConvention.FullDay"/> averaging convention.
		/// </summary>
		/// <value>
		/// The value of the field depends on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter
		/// and calculated inside the <see cref="DeprCalcParameters.Calculate"/> method.
		/// </value>
		public int WholeRecoveryDays { get; set; }

		/// <summary>
		/// The number of periods between <see cref="DeprCalcParameters.RecoveryStartBookPeriod"/> and
		/// <see cref="DeprCalcParameters.RecoveryEndBookPeriod"/> periods.
		/// </summary>
		/// <value>
		/// The value of the field depends on the value of the <see cref="DeprCalcParameters.AveragingConvention"/> parameter
		/// and calculated inside the <see cref="DeprCalcParameters.Calculate"/> method.
		/// </value>
		public decimal WholeRecoveryPeriods { get; set; }

		/// <summary>
		/// The current year of useful life.
		/// This parameter can be changed dynamically during depreciation calculation process.
		/// </summary>
		public int YearOfUsefulLife { get; set; } = 1;

		#endregion

		[Obsolete(Common.Messages.MethodIsObsoleteRemoveInLaterAcumaticaVersions)]
		protected DeprCalcParameters Init(
			int bookID,
			int assetID,
			int? methodID,
			bool depreciate,
			decimal usefulLife,
			string averagingConvention,
			string midMonthType,
			short midMonthDay,
			DateTime depreciationStartDate,
			DateTime? depreciationStopDate,
			DateTime? recoveryEndDate = null,
			FABook book = null,
			FADepreciationMethod depreciationMethod = null,
			string deprFromYear = null,
			string deprToYear = null)
		{
			return Init(
				bookID,
				assetID,
				methodID,
				depreciate,
				usefulLife,
				0m,
				averagingConvention,
				midMonthType, midMonthDay,
				depreciationStartDate,
				depreciationStopDate,
				recoveryEndDate,
				book,
				depreciationMethod,
				deprFromYear,
				deprToYear);
		}

		protected DeprCalcParameters Init(
			int bookID,
			int assetID,
			int? methodID,
			bool depreciate,
			decimal usefulLife,
			decimal percentPerYear,
			string averagingConvention,
			string midMonthType,
			short midMonthDay,
			DateTime depreciationStartDate,
			DateTime? depreciationStopDate,
			DateTime? recoveryEndDate = null,
			FABook book = null,
			FADepreciationMethod depreciationMethod = null,
			string deprFromYear = null,
			string deprToYear = null)
		{
			BookID = book?.BookID ?? bookID;
			AssetID = assetID;
			DepreciationMethodID = depreciationMethod?.MethodID ?? methodID;

			Depreciate = depreciate;
			DepreciationStartDate = depreciationStartDate;
			DepreciationStopDate = depreciationStopDate;

			PercentPerYear = percentPerYear;
			if ((depreciationMethod?.DepreciationMethod == FADepreciationMethod.depreciationMethod.AustralianPrimeCost
				|| depreciationMethod?.DepreciationMethod == FADepreciationMethod.depreciationMethod.NewZealandStraightLine
				|| depreciationMethod?.DepreciationMethod == FADepreciationMethod.depreciationMethod.NewZealandStraightLineEvenly)
				&& percentPerYear > 0m)
			{
				UsefulLife = 100m / PercentPerYear;
			}
			else
			{
				UsefulLife = usefulLife;
			}

			switch (depreciationMethod?.DepreciationMethod)
			{
				case FADepreciationMethod.depreciationMethod.AustralianPrimeCost:
					AveragingConvention = FAAveragingConvention.FullDay;
					break;
				case FADepreciationMethod.depreciationMethod.NewZealandStraightLine:
				case FADepreciationMethod.depreciationMethod.NewZealandStraightLineEvenly:
					AveragingConvention = FAAveragingConvention.FullPeriod;
					break;
				default:
					AveragingConvention = averagingConvention;
					break;
			}

			MidMonthType = midMonthType;
			MidMonthDay = midMonthDay;

			Book = book;
			DepreciationMethod = depreciationMethod;

			this.recoveryEndDate = recoveryEndDate;
			DeprFromYear = deprFromYear;
			DeprToYear = deprToYear;

			DepreciationStartPeriod = 0;
			DepreciationStopPeriod = 0;
			RecoveryStartDate = DateTime.MinValue;
			RecoveryEndDate = DateTime.MinValue;
			RecoveryStartBookPeriod = null;
			RecoveryEndBookPeriod = null;
			DepreciationStartBookPeriod = null;
			DepreciationStopBookPeriod = null;
			DepreciationStartYear = 0;
			DepreciationPeriodsInYear = 12;
			DepreciationBasis = 0m;
			AccumulatedDepreciation = 0m;
			RecoveryEndPeriod = 0;
			FirstRecoveryEndQuarterPeriod = 0;
			FirstDepreciationStopQuarterPeriod = 0;
			LastDepreciationStartQuarterPeriod = 0;
			RecoveryYears = 0;
			DepreciationYears = 0;
			StartDepreciationMidPeriodRatio = 0m;
			StopDepreciationMidPeriodRatio = 0m;
			WholeRecoveryDays = 0;
			WholeRecoveryPeriods = 0;
			YearOfUsefulLife = 1;

			return this;
				}
				
		private string GetDBPeriodYearByDate(PXGraph graph, DateTime date)
		{
			FABookPeriod deprToPeriod = graph.GetService<IFABookPeriodRepository>().FindFABookPeriodOfDate(date, BookID, AssetID);
			return deprToPeriod?.FinYear ?? date.Year.ToString(FiscalPeriodSetupCreator.CS_YEAR_FORMAT);
		}

		protected virtual DeprCalcParameters SelectFromDatabase(PXGraph graph)
		{
			Book = Book 
				?? PXSelect<FABook, 
					Where<FABook.bookID, Equal<Required<FABook.bookID>>>>.Select(graph, BookID);
			DepreciationMethod = DepreciationMethod
				?? PXSelect<FADepreciationMethod,
					Where<FADepreciationMethod.methodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(graph, DepreciationMethodID);

			int? organizationID = graph.GetService<IFABookPeriodRepository>().GetFABookPeriodOrganizationID(BookID, AssetID);

			PXSelectBase<FABookPeriod> periodsCmd = new PXSelect<FABookPeriod,
					Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
						And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
						And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>>>>,
					OrderBy<
						Asc<FABookPeriod.finPeriodID>>>(graph);

			PXSelectBase<FABookYear> yearsCmd = new PXSelect<FABookYear,
					Where<FABookYear.bookID, Equal<Required<FABookYear.bookID>>,
						And<FABookYear.organizationID, Equal<Required<FABookYear.organizationID>>>>,
					OrderBy<
						Asc<FABookYear.year>>>(graph);

			List<object> parms = new List<object> { BookID, organizationID };

			string deprFromYear;
			try
			{
				deprFromYear = GetDBPeriodYearByDate(graph, DepreciationStartDate);
			}
			catch(PXFABookPeriodException)
			{
				throw new PXException(Messages.MissingFACalendarAtStartUsefullife, DepreciationStartDate, Book.BookCode);
			}
			if (!string.IsNullOrEmpty(deprFromYear))
			{
				periodsCmd.WhereAnd<Where<FABookPeriod.finYear, GreaterEqual<Required<FABookPeriod.finYear>>>>();
				yearsCmd.WhereAnd<Where<FABookYear.year, GreaterEqual<Required<FABookYear.year>>>>();
				parms.Add((int.Parse(deprFromYear) - 1).ToString());
			}

			string deprToYear;
			DateTime depreciationToDate = recoveryEndDate ?? GetDatePlusYears(DepreciationStartDate, UsefulLife);
			try
			{
				deprToYear = GetDBPeriodYearByDate(graph, depreciationToDate);
			}
			catch (PXFABookPeriodException)
			{
				throw new PXException(Messages.MissingFACalendarAtEndUsefullife, depreciationToDate, Book.BookCode);
			}
			if (!string.IsNullOrEmpty(deprToYear))
			{
				periodsCmd.WhereAnd<Where<FABookPeriod.finYear, LessEqual<Required<FABookPeriod.finYear>>>>();
				yearsCmd.WhereAnd<Where<FABookYear.year, LessEqual<Required<FABookYear.year>>>>();
				parms.Add((int.Parse(deprToYear) + 1).ToString());
			}
			Periods = new SortedList<string, FABookPeriod>(periodsCmd
				.Select(parms.ToArray())
				.RowCast<FABookPeriod>()
				.ToDictionary(period => period.FinPeriodID));
			Years = new SortedList<int, FABookYear>(yearsCmd
				.Select(parms.ToArray())
				.RowCast<FABookYear>()
				.ToDictionary(year => int.Parse(year.Year)));

			YearSetup = graph.GetService<IFABookPeriodRepository>().FindFABookYearSetup(Book);

			return this;
		}

		protected virtual DeprCalcParameters Calculate()
		{
			if (YearSetup.FPType == FiscalPeriodSetupCreator.FPType.Quarter)
			{
				switch (AveragingConvention)
				{
					case FAAveragingConvention.FullQuarter:
						AveragingConvention = FAAveragingConvention.FullPeriod;
						break;
					case FAAveragingConvention.HalfQuarter:
						AveragingConvention = FAAveragingConvention.HalfPeriod;
						break;
				}
			}

			DepreciationStartBookPeriod = GetPeriodFromDate(DepreciationStartDate, false);
			if (DepreciationStartBookPeriod == null)
			{
				throw new PXException(Messages.FABookPeriodsNotDefinedFrom, DepreciationStartDate.ToShortDateString(), DepreciationStartDate.Year, Book.BookCode);
			}

			DepreciationStartYear = int.Parse(DepreciationStartBookPeriod.FinYear);
			DepreciationStartPeriod = int.Parse(DepreciationStartBookPeriod.PeriodNbr);
			DepreciationPeriodsInYear = GetPeriodsInYear(DepreciationStartYear);

			RecoveryStartDate = DepreciationStartDate;
			RecoveryStartBookPeriod = DepreciationStartBookPeriod;

			bool needAdditionalPeriod = false;
			int recoveryStartPeriod1;
			int depreciationStartQuarter;
			decimal depreciationStartPeriodDivide4;

			switch (AveragingConvention)
			{
				case FAAveragingConvention.FullDay:
					break;
				case FAAveragingConvention.ModifiedPeriod:
				case FAAveragingConvention.ModifiedPeriod2:
					RecoveryStartDate = DepreciationStartBookPeriod.StartDate.Value;
					
					switch (MidMonthType)
					{
						case FABook.midMonthType.PeriodDaysHalve:
							if ((DepreciationStartDate - DepreciationStartBookPeriod.StartDate.Value).Days + 1 >
								(DepreciationStartBookPeriod.EndDate.Value - DepreciationStartBookPeriod.StartDate.Value).Days / 2m)
							{
								StartDepreciationMidPeriodRatio = AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ? 0m : 0.5m;
								needAdditionalPeriod = true;
							}
							else
							{
								StartDepreciationMidPeriodRatio = 1m;
							}
							break;
						case FABook.midMonthType.FixedDay:
							if (((DepreciationStartDate - DepreciationStartBookPeriod.StartDate.Value).Days + 1) > MidMonthDay)
							{
								StartDepreciationMidPeriodRatio = AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ? 0m : 0.5m;
								needAdditionalPeriod = true;
							}
							else
							{
								StartDepreciationMidPeriodRatio = 1m;
							}
							break;
						case FABook.midMonthType.NumberOfDays:
							int previousPeriod = DepreciationStartPeriod - 1;
							int previousYear = DepreciationStartYear;
							if (previousPeriod == 0)
							{
								previousYear--;
								previousPeriod = GetPeriodsInYear(previousYear);
							}
								
							FABookPeriod previousBookPeriod = GetBookPeriod(previousPeriod, previousYear);
							if ((DepreciationStartDate - previousBookPeriod.EndDate.Value).Days + 1 > MidMonthDay)
							{
								StartDepreciationMidPeriodRatio = AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ? 0m : 0.5m;
								needAdditionalPeriod = true;
							}
							else
							{
								StartDepreciationMidPeriodRatio = 1m;
							}
							break;
					}

					StopDepreciationMidPeriodRatio = AveragingConvention == FAAveragingConvention.ModifiedPeriod
						? StartDepreciationMidPeriodRatio
						: 1m;
					break;
				case FAAveragingConvention.FullPeriod:
					RecoveryStartDate = DepreciationStartBookPeriod.StartDate.Value;
					break;
				case FAAveragingConvention.NextPeriod:
					string nextPeriodID = GetNextPeriodID(DepreciationStartBookPeriod.FinPeriodID);
					RecoveryStartBookPeriod = GetBookPeriod(nextPeriodID);
					if (RecoveryStartBookPeriod == null)
					{
						throw new PXException(Messages.FABookPeriodsNotDefinedFrom, FABookPeriodIDAttribute.FormatForError(nextPeriodID),
							FinPeriodUtils.FiscalYear(nextPeriodID), Book.BookCode);
					}

					DepreciationStartPeriod = int.Parse(FinPeriodUtils.PeriodInYear(nextPeriodID));
					DepreciationStartYear = int.Parse(FinPeriodUtils.FiscalYear(nextPeriodID));
					RecoveryStartDate = RecoveryStartBookPeriod.StartDate.Value;
					break;
				case FAAveragingConvention.FullQuarter:
					if (DepreciationPeriodsInYear == MonthsInYear)
					{
						depreciationStartPeriodDivide4 = DepreciationStartPeriod / (decimal)MonthsInQuarter;
						depreciationStartQuarter = (int)decimal.Ceiling(depreciationStartPeriodDivide4);
						recoveryStartPeriod1 = (depreciationStartQuarter - 1) * MonthsInQuarter + 1;
						RecoveryStartBookPeriod = GetBookPeriod(recoveryStartPeriod1, DepreciationStartYear);
						if (RecoveryStartBookPeriod == null)
						{
							throw new PXException(Messages.FABookPeriodsNotDefinedFrom,
								$"{recoveryStartPeriod1:00}-{DepreciationStartYear}", DepreciationStartYear, Book.BookCode);
						}

						RecoveryStartDate = RecoveryStartBookPeriod.StartDate.Value;
					}
					break;
				case FAAveragingConvention.FullYear:
					recoveryStartPeriod1 = 1;
					RecoveryStartBookPeriod = GetBookPeriod(recoveryStartPeriod1, DepreciationStartYear);
					if (RecoveryStartBookPeriod == null)
					{
						throw new PXException(Messages.FABookPeriodsNotDefinedFrom,
							$"{recoveryStartPeriod1:00}-{DepreciationStartYear}", DepreciationStartYear, Book.BookCode);
					}

					RecoveryStartDate = RecoveryStartBookPeriod.StartDate.Value;
					break;
				case FAAveragingConvention.HalfPeriod:
					RecoveryStartDate = DepreciationStartBookPeriod.StartDate.Value;
					needAdditionalPeriod = true;
					break;
				case FAAveragingConvention.HalfQuarter:
					if (DepreciationPeriodsInYear == MonthsInYear)
					{
						if (WholeRecoveryPeriods % MonthsInQuarter != 0)
						{
							throw new PXException(Messages.CanNotUseAveragingConventionWhithRecoveryPeriods, AveragingConvention, WholeRecoveryPeriods);
						}

						depreciationStartPeriodDivide4 = DepreciationStartPeriod / (decimal)MonthsInQuarter;
						depreciationStartQuarter = (int)decimal.Ceiling(depreciationStartPeriodDivide4);
						recoveryStartPeriod1 = (depreciationStartQuarter - 1) * MonthsInQuarter + 1;
						RecoveryStartBookPeriod = GetBookPeriod(recoveryStartPeriod1, DepreciationStartYear);
						if (RecoveryStartBookPeriod == null)
						{
							throw new PXException(Messages.FABookPeriodsNotDefinedFrom,
								$"{recoveryStartPeriod1:00}-{DepreciationStartYear}", DepreciationStartYear, Book.BookCode);
						}

						needAdditionalPeriod = DepreciationStartDate == RecoveryStartBookPeriod.StartDate;
						RecoveryStartBookPeriod = DepreciationStartBookPeriod;
					}
					break;
				case FAAveragingConvention.HalfYear:
					recoveryStartPeriod1 = 1;
					RecoveryStartBookPeriod = GetBookPeriod(recoveryStartPeriod1, DepreciationStartYear);
					if (RecoveryStartBookPeriod == null)
					{
						throw new PXException(Messages.FABookPeriodsNotDefinedFrom,
							$"{recoveryStartPeriod1:00}-{DepreciationStartYear}", DepreciationStartYear, Book.BookCode);
					}

					needAdditionalPeriod = DepreciationStartDate == RecoveryStartBookPeriod.StartDate;
					RecoveryStartBookPeriod = DepreciationStartBookPeriod;
					break;
			}

			RecoveryEndDate = recoveryEndDate ?? GetDatePlusYears(RecoveryStartDate, UsefulLife);
			RecoveryEndBookPeriod = GetPeriodFromDate(RecoveryEndDate, false);
			if (RecoveryEndBookPeriod == null)
			{
				throw new PXException(Messages.FABookPeriodsNotDefinedFromTo, DepreciationStartDate.ToShortDateString(),
					RecoveryEndDate.ToShortDateString(), DepreciationStartDate.Year, RecoveryEndDate.Year, Book.BookCode);
			}

			if (needAdditionalPeriod && recoveryEndDate == null)
			{
				string nextPeriodID = GetNextPeriodID(RecoveryEndBookPeriod.FinPeriodID);
				RecoveryEndBookPeriod = GetBookPeriod(nextPeriodID);
				if (RecoveryEndBookPeriod == null)
				{
					throw new PXException(Messages.FABookPeriodsNotDefinedFrom, FABookPeriodIDAttribute.FormatForError(nextPeriodID),
						FinPeriodUtils.FiscalYear(nextPeriodID), Book.BookCode);
				}

				RecoveryEndDate = RecoveryEndBookPeriod.EndDate.Value.AddDays(-1);
			}

			WholeRecoveryPeriods = (PeriodMinusPeriod(RecoveryEndBookPeriod.FinPeriodID, RecoveryStartBookPeriod.FinPeriodID) ?? 0m) + 1m;
			if (!Depreciate) return this;

			int recoveryEndYear = int.Parse(RecoveryEndBookPeriod.FinYear);
			RecoveryEndPeriod = int.Parse(RecoveryEndBookPeriod.PeriodNbr);
			RecoveryYears = recoveryEndYear - DepreciationStartYear + 1;
			WholeRecoveryDays = (RecoveryEndDate - RecoveryStartDate).Days + 1;

			if (DepreciationStopDate != null)
			{
				if (DepreciationStopDate > RecoveryEndDate)
				{
					throw new PXException(Messages.InvalidDeprToDate);
				}

				DepreciationStopBookPeriod = GetPeriodFromDate(DepreciationStopDate.Value, false);
				if (DepreciationStopBookPeriod == null)
				{
					throw new PXException(Messages.FABookPeriodsNotDefinedFromTo, DepreciationStartDate.ToShortDateString(),
						((DateTime)DepreciationStopDate).ToShortDateString(), DepreciationStartDate.Year, ((DateTime)DepreciationStopDate).Year, Book.BookCode);
				}

				int depreciationStopYear = int.Parse(DepreciationStopBookPeriod.FinYear);
				DepreciationYears = depreciationStopYear - DepreciationStartYear + 1;
				DepreciationStopPeriod = int.Parse(DepreciationStopBookPeriod.PeriodNbr);
			}
			else
			{
				DepreciationStopDate = RecoveryEndDate;
				DepreciationStopPeriod = RecoveryEndPeriod;
				DepreciationYears = RecoveryYears;
				DepreciationStopBookPeriod = RecoveryEndBookPeriod;
			}

			switch (AveragingConvention)
			{
				case FAAveragingConvention.FullDay:
					int recoveryStartYear = int.Parse(RecoveryStartBookPeriod.FinYear);
					int recoveryStartPeriod = int.Parse(RecoveryStartBookPeriod.PeriodNbr);

					if (recoveryStartYear == recoveryEndYear && recoveryStartPeriod == RecoveryEndPeriod)
					{
						DateTime previousEndDate = RecoveryEndDate;
						int allPeriodDays = GetPeriodLength(recoveryStartYear, recoveryStartPeriod);
						int deprPeriodDays = GetDaysOnPeriod(RecoveryStartDate, RecoveryEndDate, recoveryStartYear, recoveryStartPeriod, ref previousEndDate);
						WholeRecoveryPeriods = deprPeriodDays / (decimal)allPeriodDays;
					}
					else
					{
					DateTime previousEndDate = DateTime.MinValue;
					int allPeriodDays = GetPeriodLength(recoveryStartYear, recoveryStartPeriod);
					int deprPeriodDays = GetDaysOnPeriod(RecoveryStartDate, RecoveryEndDate, recoveryStartYear, recoveryStartPeriod, ref previousEndDate);
						decimal rate = deprPeriodDays / (decimal) allPeriodDays;
					WholeRecoveryPeriods += rate - 1;

					allPeriodDays = GetPeriodLength(recoveryEndYear, RecoveryEndPeriod);
					deprPeriodDays = GetDaysOnPeriod(RecoveryStartDate, RecoveryEndDate, recoveryEndYear, RecoveryEndPeriod, ref previousEndDate);
						rate = deprPeriodDays / (decimal) allPeriodDays;
					WholeRecoveryPeriods += rate - 1;
					}
					break;
				case FAAveragingConvention.FullQuarter:
				case FAAveragingConvention.HalfQuarter:
					if (DepreciationPeriodsInYear == MonthsInYear)
					{
						decimal recoveryEndPeriodDivide3 = RecoveryEndPeriod / (decimal)MonthsInQuarter;
						int recoveryEndQuarter = (int)decimal.Ceiling(recoveryEndPeriodDivide3);
						FirstRecoveryEndQuarterPeriod = (recoveryEndQuarter - 1) * MonthsInQuarter + 1;

						decimal depreciationStopPeriodDivide3 = DepreciationStopPeriod / (decimal)MonthsInQuarter;
						int depreciationStopQuarter = (int)decimal.Ceiling(depreciationStopPeriodDivide3);
						FirstDepreciationStopQuarterPeriod = (depreciationStopQuarter - 1) * MonthsInQuarter + 1;

						decimal depreciationStartPeriodDivide3 = DepreciationStartPeriod / (decimal)MonthsInQuarter;
						int depreciationStartPeriodQuarter = (int)decimal.Ceiling(depreciationStartPeriodDivide3);
						LastDepreciationStartQuarterPeriod = depreciationStartPeriodQuarter * MonthsInQuarter;
					}
					break;
			}
			return this;
		}

		public FABookPeriod GetPeriodFromDate(DateTime d, bool check = true)
		{
			FABookPeriod period = Periods.Values.FirstOrDefault(p => p.StartDate <= d && p.EndDate > d);
			if (check && period == null)
			{
				throw new PXFABookPeriodException();
			}
			return period;
				}

		[Obsolete("Will be removed in Acumatica 2018R1.")]
		public FABookYear GetBookYear(int Year)
		{
			return Years[Year];
		}

		public int GetPeriodsInYearByNumber(int yearNumber)
		{
			int year = DepreciationStartYear + yearNumber - 1;
			return GetPeriodsInYear(year);
		}

		[Obsolete("Will be removed in Acumatica 2018R1. Use " + nameof(GetPeriodsInYear) + " with one parameter instead.")]
		public int GetPeriodsInYear(FABook book, int Year)
		{
			return GetPeriodsInYear(Year);
		}

		public int GetPeriodsInYear(int year)
		{
			FABookYear faBookYear;
			if (!Years.TryGetValue(year, out faBookYear) || faBookYear.FinPeriods == null)
			{
				throw new PXException(Messages.FABookPeriodsNotDefined, Book.BookCode, year);
			}

			return Periods.Values.Count(period => int.Parse(period.FinYear) == year);
		}

		public FABookPeriod GetBookPeriod(int period, int year)
		{
			return GetBookPeriod($"{year:0000}{period:00}");
			}

		public FABookPeriod GetBookPeriod(string periodID)
		{
			return Periods[periodID];
		}

		[Obsolete("Will be removed in Acumatica 2018R1. Use " + nameof(GetNextPeriodID) + " with one parameter instead.")]
		public string GetNextPeriodID(IYearSetup yearSetup, FABook book, string periodID, int periodsInYear)
		{
			return GetNextPeriodID(periodID);
		}

		public string GetNextPeriodID(string periodID)
		{
			int nextPeriod = int.Parse(FinPeriodUtils.PeriodInYear(periodID)) + 1;
			int year = int.Parse(FinPeriodUtils.FiscalYear(periodID));

			if (nextPeriod > GetPeriodsInYear(year))
			{
				year++;
				nextPeriod = 1;
			}

			return $"{year:0000}{nextPeriod:00}";
		}

		public static DateTime GetDatePlusYears(DateTime DeprFromDate, decimal usefulLife)
		{
			DateTime deprToDate = DeprFromDate;
			if (usefulLife > 0m)
			{
				decimal fullYears = decimal.Truncate(usefulLife);
				deprToDate = DeprFromDate.AddYears((int)fullYears).AddDays(-1);

				decimal diff = usefulLife - fullYears;
				if (diff != 0m)
				{
					decimal nextYear = decimal.Ceiling(usefulLife);
					DateTime nextDate = DeprFromDate.AddYears((int)nextYear).AddDays(-1);

					int days = (int)((nextDate - deprToDate).Days * diff);
					deprToDate = deprToDate.AddDays(days - 1);
				}
			}

			return deprToDate;
		}

		public static int GetFinancialYears(int wholeRecoveryPeriods, int startPeriod, int depreciationPeriodsInYear, bool startPeriodIsWhole)
		{
			if (wholeRecoveryPeriods == 0 || startPeriod == 0) return 0;
			decimal financialYearsToCalendar = (wholeRecoveryPeriods + startPeriod - 1 + (startPeriodIsWhole == false ? 1 : 0)) / (decimal)depreciationPeriodsInYear;
			int financialYears = (int)decimal.Ceiling(financialYearsToCalendar);
			return financialYears;
		}

		public int? PeriodMinusPeriod(string FiscalPeriodID1, string FiscalPeriodID2)
		{
			FABookPeriod first, second;
			if (!Periods.TryGetValue(FiscalPeriodID1, out first) || !Periods.TryGetValue(FiscalPeriodID2, out second))
			{
				throw new PXException(Messages.NoCalendarDefined);
			}

			return Periods.Values.Count(period =>
				string.CompareOrdinal(period.FinPeriodID, FiscalPeriodID1) <= 0 &&
				string.CompareOrdinal(period.FinPeriodID, FiscalPeriodID2) > 0);
		}

		public int GetPeriodLength(int currYear, int currPeriod)
		{
			FABookPeriod period = GetBookPeriod(currPeriod, currYear);
			if (period?.StartDate == null || period.EndDate == null)
			{
				throw new PXException(Messages.FABookPeriodsNotDefined, Book.BookCode, currYear);
			}
			return (period.EndDate.Value - period.StartDate.Value).Days;
		}

		public int GetFinPeriodLengthInDays(int yearNumber, int periodNumberInYear)
		{
			return GetFinPeriodLengthInDays(yearNumber, periodNumberInYear, AveragingConvention);
		}

		public int GetFinPeriodLengthInDaysFebAlways28(int yearNumber, int currPeriod)
		{
			int currentYear = this.DepreciationStartYear + yearNumber - 1;

			FABookPeriod period = GetBookPeriod(currPeriod, currentYear);
			if (period?.StartDate == null || period.EndDate == null)
			{
				throw new PXException(Messages.FABookPeriodsNotDefined, Book.BookCode, currentYear);
			}

			if (YearSetup.PeriodType == FinPeriodType.Month
				&& period.StartDate.Value.DayOfYear == 32) //01 Feb
				return 28;
			else
				return (period.EndDate.Value - period.StartDate.Value).Days;
		}

		public int GetDaysHeldInYear(int year, int fromPeriod, int toPeriod)
		{
			int daysHeldInYear = 0;

			for (int i = fromPeriod; i <= toPeriod; i++)
			{
				int daysOfPeriod = GetFinPeriodLengthInDaysFebAlways28(year, i);
				daysHeldInYear += daysOfPeriod;
			}
			return daysHeldInYear;
		}

			public int GetFinPeriodLengthInDays(int yearNumber, int periodNumberInYear, string averagingConvention)
		{
			int startPeriod = yearNumber == 1 ? this.DepreciationStartPeriod : 1;
			int endPeriod = yearNumber == this.DepreciationYears ? this.DepreciationStopPeriod : GetPeriodsInYearByNumber(yearNumber);
			int currentYear = this.DepreciationStartYear + yearNumber - 1;

			if (averagingConvention == FAAveragingConvention.FullDay && yearNumber == 1 && periodNumberInYear == startPeriod)
			{
				DateTime previousEndDate = DateTime.MinValue;
				DateTime periodEndDate = GetPeriodFromDate(this.RecoveryStartDate).EndDate.Value;
				return GetDaysOnPeriod(this.RecoveryStartDate, periodEndDate, currentYear, periodNumberInYear, ref previousEndDate);
			}
			if (averagingConvention == FAAveragingConvention.FullDay && yearNumber == DepreciationYears && periodNumberInYear == endPeriod)
			{
				DateTime previousEndDate = DateTime.MinValue;
				DateTime periodStartDate = GetPeriodFromDate(DepreciationStopDate ?? RecoveryEndDate).StartDate.Value;
				return GetDaysOnPeriod(periodStartDate, DepreciationStopDate ?? RecoveryEndDate, currentYear, periodNumberInYear, ref previousEndDate) + 1;
			}

			return GetPeriodLength(currentYear, periodNumberInYear);
		}

		public int GetFinYearLengthInDays(int yearNumber)
		{
			var currentYear = Years[this.DepreciationStartYear + yearNumber - 1];
			return (currentYear.EndDate.Value - currentYear.StartDate.Value).Days;
		}

		public int GetDaysOnPeriod(DateTime recoveryStartDate, DateTime? recoveryEndDate, int currYear, int currPeriod, ref DateTime previousEndDate)
		{
			FABookPeriod existBookPeriod = GetBookPeriod(currPeriod, currYear);
			if (existBookPeriod == null)
			{
				throw new PXException(Messages.FABookPeriodsNotDefined, Book.BookCode, currYear);
			}

			DateTime? existPeriodStartDate = existBookPeriod.StartDate;
			DateTime? existPeriodEndDate = existBookPeriod.EndDate;

			int recoveryDays = 0;
			if (recoveryStartDate	 <= existPeriodEndDate &&
				existPeriodStartDate <= recoveryEndDate)
			{
				DateTime? periodStartDate	= existPeriodStartDate	> recoveryStartDate ? existPeriodStartDate	: recoveryStartDate;
				DateTime? periodEndDate		= existPeriodEndDate	< recoveryEndDate	? existPeriodEndDate	: recoveryEndDate;
				recoveryDays  = (periodEndDate.Value - periodStartDate.Value).Days;
				recoveryDays += periodStartDate == previousEndDate
					? periodEndDate == recoveryEndDate ? 1 : 0
					: previousEndDate == DateTime.MinValue ? 0 : 1;
				previousEndDate = periodEndDate.Value;
			}

			return recoveryDays;
		}

		public static DateTime GetRecoveryEndDate(PXGraph graph, FABookBalance assetBalance)
		{
			return new DeprCalcParameters().Fill(graph, assetBalance).RecoveryEndDate;
		}

		public static string GetRecoveryStartPeriod(PXGraph graph, FABookBalance assetBalance)
		{
			DeprCalcParameters parameters = new DeprCalcParameters().Fill(graph, assetBalance);
			string startPeriodID = $"{parameters.DepreciationStartYear:0000}{parameters.DepreciationStartPeriod:00}"; ;

			if (parameters.DepreciationStartYear == 0 && 
				parameters.DepreciationStartPeriod == 0)
		{
				startPeriodID = null;
		}

			if (parameters.StartDepreciationMidPeriodRatio == 0m && 
				(assetBalance.AveragingConvention == FAAveragingConvention.ModifiedPeriod || 
					assetBalance.AveragingConvention == FAAveragingConvention.ModifiedPeriod2))
		{
				startPeriodID = graph.GetService<IFABookPeriodUtils>().PeriodPlusPeriodsCount(startPeriodID, 1, assetBalance.BookID, assetBalance.AssetID);
				}

			// Note that DeprFromPeriod is an extra key, which is
			// restricting FABookBalance query (see CommandPreparing handler).
			// We need this verification to prevent urgent erorr on persist
			// (see AC-111026 for details).
			//
			if (startPeriodID != null &&
				assetBalance.AllowChangeDeprFromPeriod == false &&
				string.CompareOrdinal(assetBalance.DeprFromPeriod, startPeriodID) != 0)
			{
				throw new PXSetPropertyException(Messages.DeprFromPeriodUpdatedWhileDepreciationExists);
			}

			return startPeriodID;
			}
	}
}
