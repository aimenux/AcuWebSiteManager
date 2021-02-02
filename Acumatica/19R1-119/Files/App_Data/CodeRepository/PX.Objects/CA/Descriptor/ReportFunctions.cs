using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Aging;
using PX.Objects.Common.Extensions;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.CA.Descriptor
{
	public class ReportFunctions
	{
		//=Payments.GetAPPaymentInfo('102000', 'FEDWRE', '1' , 'V000213')
		//                           ^-Cash Account
		//                                     ^-Payment Method
		//                                               ^- Detail ID
		//                                                     ^- Vendror ID
		public string GetAPPaymentInfo(
			string accountCD,
			string paymentMethodID,
			string detailID,
			string acctCD)
		{
			return GetAPPaymentInfo(accountCD, paymentMethodID, detailID, acctCD, null);
		}

		public string GetAPPaymentInfo(
			string accountCD,
			string paymentMethodID,
			string detailID,
			string acctCD,
			string locationCD)
		{
			VendorPaymentMethodDetail detail = PXSelectJoin<VendorPaymentMethodDetail,
															InnerJoin<BAccount, On<BAccount.bAccountID, Equal<VendorPaymentMethodDetail.bAccountID>>,
															InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<VendorPaymentMethodDetail.paymentMethodID>>,
															InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<VendorPaymentMethodDetail.paymentMethodID>,
																And<PaymentMethodDetail.detailID, Equal<VendorPaymentMethodDetail.detailID>,
																And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
																	Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>,
															InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
															InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>,
															InnerJoin<Account, On<Account.accountID, Equal<CashAccount.accountID>>,
															LeftJoin<Location, On<Location.bAccountID, Equal<BAccount.bAccountID>, And<Location.locationCD, Equal<Required<Location.locationCD>>>>>>>>>>>,
															Where<Account.accountCD, Equal<Required<Account.accountCD>>,
																And<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>,
																And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>,
																And<BAccount.acctCD, Equal<Required<BAccount.acctCD>>,
																And<Where2<Where<Location.locationID, IsNotNull>, And<VendorPaymentMethodDetail.locationID, Equal<Location.locationID>,
																	Or2<Where<Location.locationID, IsNull>, And<VendorPaymentMethodDetail.locationID, Equal<BAccount.defLocationID>>>>>>>>>>>
											   .Select(PXGraph.CreateInstance<BusinessAccountMaint>(),
													   locationCD,
													   accountCD,
													   paymentMethodID,
													   detailID,
													   acctCD);

			return detail != null ? detail.DetailValue : string.Empty;
		}

		//=Payments.GetARPaymentInfo('102000', 'FEDWRE', '1', 'C000213')
		//                           ^-Cash Account
		//                                     ^-Payment Method
		//                                               ^- Detail ID
		//                                                    ^- Customer ID
		public string GetARPaymentInfo(
			string accountCD,
			string paymentMethodID,
			string detailID,
			string pMInstanceID)
		{
			int actualPMInstanceID;
			if (!int.TryParse(pMInstanceID, out actualPMInstanceID))
			{
				throw new PXArgumentException("pMInstanceID", Messages.ValueMInstanceIdError);
			}

			CustomerPaymentMethodDetail detail = PXSelectJoin<CustomerPaymentMethodDetail,
															  InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>>,
															  InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
																  And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
																  And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
															  InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
															  InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>,
															  InnerJoin<Account, On<Account.accountID, Equal<CashAccount.accountID>>>>>>>,
															  Where<Account.accountCD, Equal<Required<Account.accountCD>>,
																  And<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>,
																  And<CustomerPaymentMethodDetail.detailID, Equal<Required<CustomerPaymentMethodDetail.detailID>>,
																  And<CustomerPaymentMethodDetail.pMInstanceID, Equal<Required<CustomerPaymentMethodDetail.pMInstanceID>>>>>>>
												 .Select(PXGraph.CreateInstance<CustomerPaymentMethodMaint>(),
														 accountCD,
														 paymentMethodID,
														 detailID,
														 pMInstanceID);

			return detail != null ? detail.Value : string.Empty;
		}

		//=Payments.GetRemitPaymentInfo('102000', 'FEDWRE', '1')
		//                              ^-Cash Account
		//                                        ^-Payment Method
		//                                                  ^- Detail ID
		public string GetRemitPaymentInfo(
			string accountCD,
			string paymentMethodID,
			string detailID)
		{
			CashAccountPaymentMethodDetail detail = PXSelectJoin<CashAccountPaymentMethodDetail,
																 InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CashAccountPaymentMethodDetail.paymentMethodID>>,
																 InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CashAccountPaymentMethodDetail.paymentMethodID>,
																	 And<PaymentMethodDetail.detailID, Equal<CashAccountPaymentMethodDetail.detailID>,
																	 And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
																		 Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>,
																 InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
																 InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CashAccountPaymentMethodDetail.accountID>,
																	And<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>>>>>,
																 Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>,
																	 And<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>,
																	 And<CashAccountPaymentMethodDetail.detailID, Equal<Required<CashAccountPaymentMethodDetail.detailID>>>>>>
													.Select(PXGraph.CreateInstance<CashAccountMaint>(),
															accountCD,
															paymentMethodID,
															detailID);

			return detail != null ? detail.DetailValue : string.Empty;
		}

		private class CurrencyRateCollection : ConcurrentBag<CurrencyRate>, PX.Common.IPXCompanyDependent
		{
		}

		private ConcurrentBag<CurrencyRate> GetCachedCurrencyRates()
		{
			ConcurrentBag<CurrencyRate> rates = PX.Common.PXContext.GetSlot<CurrencyRateCollection>();
			if (rates == null)
			{
				rates = PX.Common.PXContext.SetSlot<CurrencyRateCollection>(PXDatabase.GetSlot<CurrencyRateCollection>(typeof(CM.CurrencyRate).FullName, typeof(CurrencyRate)));
			}
			return rates;
		}

		private CurrencyRate FindCurrencyRate(ConcurrentBag<CurrencyRate> rates, string fromCury, string toCury, string rateType, DateTime effectiveDate)
		{
			CM.CurrencyRate foundRate = null;
			if (rates.Count != 0)
			{
				foreach (CurrencyRate rate in rates)
				{
					if (rate.CuryRateType == (string)rateType &&
						rate.FromCuryID == (string)fromCury &&
						rate.ToCuryID == (string)toCury)
					{
						if (rate.CuryEffDate == (DateTime)effectiveDate)
						{
							foundRate = rate;
							break;
						}
						else if (rate.CuryEffDate < (DateTime)effectiveDate && ((foundRate != null && rate.CuryEffDate > foundRate.CuryEffDate) || foundRate == null))
						{
							foundRate = rate;
						}
					}
				}
			}
			return foundRate;
		}

		private CurrencyRate FindCurrencyRate(string fromCury, string toCury, string rateType, DateTime effectiveDate)
		{
			ConcurrentBag<CurrencyRate> rates = GetCachedCurrencyRates();

			CM.CurrencyRate foundRate = FindCurrencyRate(rates, fromCury, toCury, rateType, (DateTime)effectiveDate);
			if (foundRate == null)
			{
				PXGraph graph = new PXGraph();
				PXResultset<CurrencyRate> currencyRates = PXSelect<CurrencyRate, Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
				And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
				And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
				And<CurrencyRate.curyEffDate, GreaterEqual<Required<CurrencyRate.curyEffDate>>>>>>,
				OrderBy<Asc<CM.CurrencyRate.curyEffDate>>>.SelectWindowed(graph, 0, 100, fromCury, toCury, rateType, effectiveDate);

				if (currencyRates.Count == 0)
				{
					currencyRates = PXSelect<CurrencyRate, Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
					And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
					And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
					And<CurrencyRate.curyEffDate, Less<Required<CurrencyRate.curyEffDate>>>>>>,
					OrderBy<Desc<CurrencyRate.curyEffDate>>>.SelectWindowed(graph, 0, 1, fromCury, toCury, rateType, effectiveDate);
				}

				foreach (CurrencyRate rate in currencyRates)
				{
					((IProducerConsumerCollection<CurrencyRate>)rates).TryAdd(rate);
				}

				foundRate = FindCurrencyRate(rates, fromCury, toCury, rateType, (DateTime)effectiveDate);
			}
			return foundRate;
		}

		public object CuryConvCury(object fromCury, object toCury, object rateType, object baseval, object effectiveDate)
		{
			CM.CurrencyRate foundRate = FindCurrencyRate((string)fromCury, (string)toCury, (string)rateType, (DateTime)effectiveDate);
			decimal curyval;
			if (foundRate != null)
			{
				decimal rate;
				try
				{
					rate = (decimal)foundRate.CuryRate;
				}
				catch (InvalidOperationException)
				{
					throw new PXRateNotFoundException();
				}
				if (rate == 0.0m)
				{
					rate = 1.0m;
				}
				bool mult = foundRate.CuryMultDiv != "D";
				curyval = mult ? (decimal)baseval * rate : (decimal)baseval / rate;
			}
			else
			{
				curyval = baseval == null ? 0m : (decimal)baseval;
			}
			return curyval;
		}

		public object CuryConvBase(object fromCury, object rateType, object curyval, object effectiveDate)
		{
			Company company = PXSelect<Company>.Select(new PXGraph());
			object baseval = CuryConvCury(fromCury, company.BaseCuryID, rateType, curyval, effectiveDate);
			return baseval != null ? (decimal)baseval : 0m;
		}

		const int NUMBER_OF_AGING_BUCKETS = 5;

	    [Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2019R2)]
	    public string GetBucketDescriptionForAgedReport(
	        DateTime? reportDate,
	        int? dayBucketBoundary0,
	        int? dayBucketBoundary1,
	        int? dayBucketBoundary2,
	        int? dayBucketBoundary3,
	        bool? isByFinancialPeriod,
	        bool? isForwardAging,
	        int? bucketIndex
	    ) => GetBucketDescriptionForAgedReport(
	        reportDate,
	        dayBucketBoundary0,
	        dayBucketBoundary1,
	        dayBucketBoundary2,
	        dayBucketBoundary3,
	        isByFinancialPeriod,
	        isForwardAging,
	        bucketIndex,
	        FinPeriod.organizationID.MasterValue,
	        true);


        public string GetBucketDescriptionForAgedReport(
			DateTime? reportDate,
			int? dayBucketBoundary0,
			int? dayBucketBoundary1,
			int? dayBucketBoundary2,
			int? dayBucketBoundary3,
			bool? isByFinancialPeriod,
			bool? isForwardAging,
			int? bucketIndex,
            int calendarOrganizationID,
            bool usePeriodDescription)
		{
			if (reportDate == null
				|| dayBucketBoundary0 == null
				|| dayBucketBoundary1 == null
				|| dayBucketBoundary2 == null
				|| dayBucketBoundary3 == null
				|| bucketIndex == null)
			{
				return null;
			}

			AgingDirection agingDirection = isForwardAging == true
				? AgingDirection.Forward
				: AgingDirection.Backwards;

			if (isByFinancialPeriod == true)
			{
				try
				{
					PXGraph graph = new PXGraph();
					IEnumerable<string> bucketDescriptions = AgingEngine.GetPeriodAgingBucketDescriptions(
						graph.GetService<IFinPeriodRepository>(),
						reportDate.Value,
						agingDirection,
						NUMBER_OF_AGING_BUCKETS,
					    calendarOrganizationID,
					    usePeriodDescription);

					return bucketDescriptions.ElementAtOrDefault(bucketIndex.Value);
				}
				catch (PXFinPeriodException)
				{
					throw new PXFinPeriodException(
						isForwardAging == true
							? AR.Messages.UnableToCalculateBucketNamesPeriodsAfterwardsNotDefined
							: AR.Messages.UnableToCalculateBucketNamesPeriodsPrecedingNotDefined);
				}
			}
			else
			{
				IEnumerable<string> bucketDescriptions = AgingEngine.GetDayAgingBucketDescriptions(
					agingDirection,
					new int[]
					{
						dayBucketBoundary0 ?? 0,
						dayBucketBoundary1 ?? 0,
						dayBucketBoundary2 ?? 0,
						dayBucketBoundary3 ?? 0
					},
					true);

				return bucketDescriptions.ElementAtOrDefault(bucketIndex.Value);
			}
		}

	    [Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2019R2)]
        public object GetBucketNumberForAgedReport(
	        DateTime? reportDate,
	        DateTime? dateToAge,
	        int? dayBucketBoundary0,
	        int? dayBucketBoundary1,
	        int? dayBucketBoundary2,
	        int? dayBucketBoundary3,
	        bool? isByFinancialPeriod,
	        bool? isForwardAging) => GetBucketNumberForAgedReport(
	        reportDate,
	        dateToAge,
	        dayBucketBoundary0,
	        dayBucketBoundary1, 
	        dayBucketBoundary2, 
	        dayBucketBoundary3,
	        isByFinancialPeriod,
	        isForwardAging,
	        FinPeriod.organizationID.MasterValue);


        public object GetBucketNumberForAgedReport(
			DateTime? reportDate,
			DateTime? dateToAge,
			int? dayBucketBoundary0,
			int? dayBucketBoundary1,
			int? dayBucketBoundary2,
			int? dayBucketBoundary3,
			bool? isByFinancialPeriod,
			bool? isForwardAging,
			int organizationID)
		{
			if (reportDate == null
				|| dayBucketBoundary0 == null
				|| dayBucketBoundary1 == null
				|| dayBucketBoundary2 == null
				|| dayBucketBoundary3 == null)
			{
				return null;
			}

			AgingDirection agingDirection = isForwardAging == true
				? AgingDirection.Forward
				: AgingDirection.Backwards;

			PXGraph graph = new PXGraph();

			return isByFinancialPeriod == true
				? AgingEngine.AgeByPeriods(
					reportDate.Value,
					dateToAge.Value,
					graph.GetService<IFinPeriodRepository>(),
					agingDirection,
					NUMBER_OF_AGING_BUCKETS,
			        organizationID)
				: AgingEngine.AgeByDays(
					reportDate.Value,
					dateToAge.Value,
					agingDirection,
					dayBucketBoundary0 ?? 0,
					dayBucketBoundary1 ?? 0,
					dayBucketBoundary2 ?? 0,
					dayBucketBoundary3 ?? 0);
		}

		/// <summary>
		/// Retrieves the financial period with the same <see cref="MasterFinPeriod.PeriodNbr"/>
		/// as the one specified, but residing in the previous financial year.
		/// If no such period exists, returns <c>null</c>.
		/// </summary>
		public string GetSamePeriodInPreviousYear(string financialPeriodID)
		{
			if (financialPeriodID == null)
			{
				return null;
			}

			PXGraph graph = PXGraph.CreateInstance<MasterFinPeriodMaint>();

			try
			{
				IFinPeriodRepository finPeriodRepository = graph.GetService<IFinPeriodRepository>();

				string resultingPeriodID = finPeriodRepository.GetSamePeriodInPreviousYear(financialPeriodID, FinPeriod.organizationID.MasterValue);

				return FinPeriodIDFormattingAttribute.FormatForDisplay(resultingPeriodID);
			}
			catch (PXFinPeriodException)
			{
				return null;
			}
		}

		/// <summary>
		/// Retrieves the first financial period of the year corresponding
		/// to the financial period specified.
		/// If no such period exists, returns <c>null</c>.
		/// </summary>
		public string GetFirstPeriodOfYear(string financialPeriodID)
		{
			if (financialPeriodID == null)
			{
				return null;
			}

			PXGraph graph = PXGraph.CreateInstance<MasterFinPeriodMaint>();

			string firstPeriodOfYear = FinPeriodUtils.GetFirstFinPeriodIDOfYear(financialPeriodID);

			IFinPeriodRepository finPeriodRepository = graph.GetService<IFinPeriodRepository>();

			if (!finPeriodRepository.PeriodExists(financialPeriodID, FinPeriod.organizationID.MasterValue))
			{
				return null;
			}

			return FinPeriodIDFormattingAttribute.FormatForDisplay(firstPeriodOfYear);
		}

		/// <summary>
		/// Returns a value indicating whether the two specified financial
		/// period IDs belong to the same financial year.
		/// </summary>
		public bool ArePeriodsInSameYear(string firstPeriodID, string secondPeriodID)
		{
			string firstPeriodYear = GetPeriodYear(firstPeriodID);
			string secondPeriodYear = GetPeriodYear(secondPeriodID);

			if (firstPeriodYear == null || secondPeriodYear == null)
			{
				return false;
			}
			else
			{
				return string.Compare(
					firstPeriodYear,
					secondPeriodYear,
					StringComparison.OrdinalIgnoreCase) == 0;
			}
		}

		/// <summary>
		/// Extracts the year of the financial period
		/// from the specified financial period ID.
		/// </summary>
		public string GetPeriodYear(string financialPeriodID)
			=> financialPeriodID?.Substring(0, 4);

		/// <summary>
		/// Extracts the number of the financial period
		/// from the specified financial period ID.
		/// </summary>
		public string GetPeriodNumber(string financialPeriodID)
			=> financialPeriodID?.Substring(4, 2);

		/// <summary>
		/// Get the localizable amount description for applications in 
		/// AR statement (AR641500 and AR642000) reports.
		/// </summary>
		public string GetAmountDescriptionForStatementApplication(
			decimal? writeOffAmount,
			decimal? cashDiscountAmount,
			decimal? gainLossAmount,
			string localeName)
		{
			string amountDescription = string.Empty;

			if (writeOffAmount != 0 && cashDiscountAmount != 0 && gainLossAmount != 0)
			{
				amountDescription = AR.Messages.WriteOffDiscountGainLossAmountFor;
			}
			else if (writeOffAmount != 0 && cashDiscountAmount.IsNullOrZero() && gainLossAmount != 0)
			{
				amountDescription = AR.Messages.WriteOffGainLossAmountFor;
			}
			else if (writeOffAmount.IsNullOrZero() && cashDiscountAmount != 0 && gainLossAmount != 0)
			{
				amountDescription = AR.Messages.DiscountGainLossAmountFor;
			}
			else if (writeOffAmount != 0 && cashDiscountAmount != 0 && gainLossAmount.IsNullOrZero())
			{
				amountDescription = AR.Messages.WriteOffDiscountAmountFor;
			}
			else if (gainLossAmount != 0)
			{
				amountDescription = AR.Messages.GainLossAmountFor;
			}
			else if (cashDiscountAmount != 0)
			{
				amountDescription = AR.Messages.CashDiscountAmountFor;
			}
			else if (writeOffAmount != 0)
			{
				amountDescription = AR.Messages.WriteOffAmountFor;
			}

			using (new PXLocaleScope(localeName))
			{
				return PXMessages.LocalizeNoPrefix(amountDescription);
			}
		}

		/// <summary>
		/// Gets the localized "applied to" string for AR641500 and AR642000 AR statement
		/// reports.
		/// </summary>
		public string AppliedToLocalized(string localeName)
		{
			using (new PXLocaleScope(localeName))
			{
				return PXMessages.LocalizeNoPrefix(AR.Messages.AppliedTo);
			}
		}

		/// <summary>
		/// Using the specified statement cycle aging settings and information about 
		/// various dates, returns the relevant date value to be compared to the aging 
		/// date to age the corresponding document in AR Aged reports. 
		/// </summary>
		/// <param name="invoiceDueDate">
		/// The due date of the invoice document. When it is <c>null</c>, the document 
		/// is considered a credit document, and the end result will be affected
		/// by the <paramref name="ageCredits"/> parameter.
		/// </param>
		public DateTime? GetDocumentDateForAgedReport(
			DateTime? agingDate,
			DateTime? invoiceDueDate,
			DateTime? documentDate,
			string ageBasedOnType,
			bool? ageCredits)
		{
			if (ageBasedOnType == AgeBasedOnType.DueDate)
			{
				if (invoiceDueDate != null)
				{
					return invoiceDueDate;
				}
				else if (ageCredits == true)
				{
					return documentDate;
				}
				else
				{
					return agingDate;
				}
			}

			if (ageBasedOnType == AgeBasedOnType.DocDate)
			{
				if (invoiceDueDate == null
					&& ageCredits != true)
				{
					return agingDate;
				}
				else
				{
					return documentDate;
				}
			}

			return agingDate;
		}
	}
}
