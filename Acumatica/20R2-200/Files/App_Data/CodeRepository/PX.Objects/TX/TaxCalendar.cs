using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.TX.Data;
using PX.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.TX
{
	public class TaxCalendar
	{
		public class CreationParams
		{
			public int OrganizationID { get; set; }

			public int TaxAgencyID { get; set; }

			public string TaxPeriodType { get; set; }

			public DateTime StartDateTime { get; set; }

			public int? TaxYearNumber { get; set; }

			public int? PeriodCount { get; set; }

            public DateTime? BaseDate { get; set; }

			public static CreationParams FromTaxYear(TaxYear taxYear)
			{
				return new CreationParams()
				{
					OrganizationID = taxYear.OrganizationID.Value,
					TaxAgencyID = taxYear.VendorID.Value,
					TaxPeriodType = taxYear.TaxPeriodType,
					StartDateTime = taxYear.StartDate.Value,
					TaxYearNumber = Convert.ToInt32(taxYear.Year),
					PeriodCount = taxYear.PeriodsCount
				};
			}
		}
	}

	public class TaxCalendar<TTaxYear, TTaxPeriod>: TaxCalendar
		where TTaxYear: TaxYear, new()
		where TTaxPeriod : TaxPeriod, new()
	{
		protected readonly PXGraph Graph;

		public TaxCalendar(PXGraph graph)
		{
			Graph = graph;
		}

		public virtual TaxPeriod GetOrCreateCurrentTaxPeriod(PXCache taxYearCache, PXCache taxPeriodCache, int? organizationID, int? taxAgencyID)
		{
			TaxPeriod taxper = null;

			Vendor vendor = VendorMaint.GetByID(Graph, taxAgencyID);

			taxper = TaxYearMaint.FindPreparedPeriod(Graph, organizationID, taxAgencyID);
			if (taxper != null)
			{
				return taxper;
			}

			TaxTran first_tran = ReportTax.GetEarliestNotReportedTaxTran(Graph, vendor, organizationID, null);

			DateTime? first_date = first_tran != null && first_tran.TranDate != null
				? first_tran.TranDate
				: Graph.Accessinfo.BusinessDate;

			if (first_tran != null && vendor.TaxReportFinPeriod == true)
			{
				first_date = first_tran.FinDate;
			}

			TaxPeriod lastTaxPeriod = TaxYearMaint.FindLastTaxPeriod(Graph, organizationID, taxAgencyID);

			if (lastTaxPeriod?.Status == TaxPeriodStatus.Closed)
			{
				CreateAndAddToCache(taxYearCache, taxPeriodCache, organizationID, vendor);
			}

			taxper = TaxYearMaint.FinTaxPeriodByDate(Graph, organizationID, taxAgencyID, first_date);

			if (taxper != null && (vendor.UpdClosedTaxPeriods == true ||
								   taxper.Status == TaxPeriodStatus.Dummy ||
								   taxper.Status == TaxPeriodStatus.Open))
			{
				return taxper;
			}

			taxper = TaxYearMaint.FindFirstOpenTaxPeriod(Graph, organizationID, taxAgencyID);
			if (taxper != null)
			{
				return taxper;
			}

			if (first_date != null)
			{
				CreateAndAddToCache(taxYearCache, taxPeriodCache, organizationID, vendor, first_date.Value.Year, first_date);

				taxper = TaxYearMaint.FinTaxPeriodByDate(Graph, organizationID, taxAgencyID, first_date);

				if (taxper != null)
				{
					taxper.Status = first_tran == null || first_tran.TranDate == null
										? TaxPeriodStatus.Dummy
										: taxper.Status;
				}
				else
				{
					taxper = TaxYearMaint.FinTaxPeriodByDate(Graph, organizationID, taxAgencyID, Graph.Accessinfo.BusinessDate);

					if (taxper == null)
					{
						taxper = taxPeriodCache.Cached.Cast<TTaxPeriod>()
												.Where(period => period.Status == TaxPeriodStatus.Open)
												.OrderBy(period => period.TaxPeriodID)
												.FirstOrDefault();
					}
				}
			}

			return taxper;
		}

		public virtual TaxYearWithPeriods<TTaxYear, TTaxPeriod> CreateByFinancialPeriod(int? organizationID, int? taxAgencyID, DateTime? date, int? periodCount)
		{
			var taxYearWithPeriods = new TaxYearWithPeriods<TTaxYear, TTaxPeriod>();

			foreach (PXResult<OrganizationFinYear, OrganizationFinPeriod> res in
												PXSelectJoin<OrganizationFinYear,
															InnerJoin<OrganizationFinPeriod,
																On<OrganizationFinPeriod.finYear, Equal<OrganizationFinYear.year>,
																And<OrganizationFinPeriod.organizationID, Equal<OrganizationFinYear.organizationID>>>>,
															Where<OrganizationFinYear.startDate, LessEqual<Required<OrganizationFinYear.startDate>>,
																And<OrganizationFinYear.organizationID, Equal<Required<OrganizationFinYear.organizationID>>>>,
															OrderBy<Desc<OrganizationFinYear.year>>>.Select(Graph, (object)date, organizationID))
			{
				OrganizationFinYear finYear = res;

				if (taxYearWithPeriods.TaxYear == null)
				{
					taxYearWithPeriods.TaxYear = new TTaxYear
					{
						Year = finYear.Year,
						StartDate = finYear.StartDate,
						VendorID = taxAgencyID,
						OrganizationID = organizationID,
						TaxPeriodType = VendorTaxPeriodType.FiscalPeriod,
						PlanPeriodsCount = 0,
						Filed = false
					};
				}
				else if (object.Equals(finYear.Year, taxYearWithPeriods.TaxYear.Year) == false)
				{
					break;
				}

				taxYearWithPeriods.TaxYear.PlanPeriodsCount++;

				if (periodCount == null
				    || taxYearWithPeriods.TaxPeriods.Count <= periodCount)

				{
					OrganizationFinPeriod finPeriod = res;

					taxYearWithPeriods.TaxPeriods.Add(new TTaxPeriod
					{
					    OrganizationID = taxYearWithPeriods.TaxYear.OrganizationID,
                        TaxYear = finPeriod.FinYear,
						TaxPeriodID = finPeriod.FinPeriodID,
						StartDate = finPeriod.StartDate,
						EndDate = finPeriod.EndDate,
						VendorID = taxAgencyID
					});
				}
			}

		    if (taxYearWithPeriods.TaxYear != null)
		    {
		        taxYearWithPeriods.TaxYear.PeriodsCount = periodCount ?? taxYearWithPeriods.TaxYear.PlanPeriodsCount;
		    }

		    return taxYearWithPeriods;
		}

		/// <summary>
		/// Creates <see cref="TaxPeriod"/> and <see cref="TaxYear"/> records based on specified tax calendar periods configuration (not on financial periods).
		/// </summary>
		/// <param name="organizationID">The key of a master <see cref="Organization"/> record.</param>
		/// <param name="taxAgencyID">The key of a tax agency (<see cref="Vendor"/>).</param>
		/// <param name="taxPeriodType">Value of <see cref="VendorTaxPeriodType"/>.</param>
		/// <param name="startDate">Defines starting a day and a month of the target year and a year if the previous year does not exist.</param>
		/// <param name="taxYear">It is defined when previous <see cref="TaxYear"/> record exists and it is needed to create subsequent records.</param>
		/// <param name="periodCount">It is defined if <see cref="TaxYear"/> record has custom period count.</param>
		public virtual TaxYearWithPeriods<TTaxYear, TTaxPeriod> CreateByIndependentTaxCalendar(CreationParams creationParams)
		{
			short planPeriodsCount = 0;

			switch (creationParams.TaxPeriodType)
			{
				case VendorTaxPeriodType.Monthly:
					planPeriodsCount = 12;
					break;
				case VendorTaxPeriodType.SemiMonthly:
					planPeriodsCount = 24;
					break;
				case VendorTaxPeriodType.Quarterly:
					planPeriodsCount = 4;
					break;
				case VendorTaxPeriodType.Yearly:
					planPeriodsCount = 1;
					break;
				case VendorTaxPeriodType.BiMonthly:
					planPeriodsCount = 6;
					break;
				case VendorTaxPeriodType.SemiAnnually:
					planPeriodsCount = 2;
					break;
			}

			int calendarYear = creationParams.StartDateTime.Year;
			int month = creationParams.StartDateTime.Month;

			var taxYearWithPeriods = new TaxYearWithPeriods<TTaxYear, TTaxPeriod>
			{
				TaxYear = new TTaxYear
				{
					OrganizationID = creationParams.OrganizationID,
					VendorID = creationParams.TaxAgencyID,
					Year = (creationParams.TaxYearNumber ?? calendarYear).ToString(),
					StartDate = creationParams.StartDateTime,
					TaxPeriodType = creationParams.TaxPeriodType,
					PlanPeriodsCount = planPeriodsCount,
					Filed = false
				}
			};

			taxYearWithPeriods.TaxYear.PeriodsCount = creationParams.PeriodCount ?? planPeriodsCount;

			for (int i = 1; i <= planPeriodsCount && i <= taxYearWithPeriods.TaxYear.PeriodsCount; i++)
			{
				TTaxPeriod newTaxPeriod = new TTaxPeriod
				{
					OrganizationID = taxYearWithPeriods.TaxYear.OrganizationID,
					VendorID = taxYearWithPeriods.TaxYear.VendorID,
					TaxYear = taxYearWithPeriods.TaxYear.Year,
					TaxPeriodID = taxYearWithPeriods.TaxYear.Year + (i < 10 ? "0" : "") + i,
					Status = TaxPeriod.status.DefaultStatus,
					Filed = false
				};

				if (planPeriodsCount <= 12)
				{
					newTaxPeriod.StartDate = new DateTime(calendarYear, month, 1);
					month += 12 / (int)planPeriodsCount;
					if (month > 12)
					{
						month -= 12;
						calendarYear++;
					}
					newTaxPeriod.EndDate = new DateTime(calendarYear, month, 1);
				}
				else if (planPeriodsCount == 24)
				{
					newTaxPeriod.StartDate = (i % 2 == 1) ? new DateTime(calendarYear, month, 1) : new DateTime(calendarYear, month, 16);
					if (i % 2 == 0)
					{
						month++;
						if (month > 12)
						{
							month -= 12;
							calendarYear++;
						}
					}
					newTaxPeriod.EndDate = (i % 2 == 1) ? new DateTime(calendarYear, month, 16) : new DateTime(calendarYear, month, 1);
				}

				taxYearWithPeriods.TaxPeriods.Add(newTaxPeriod);
			}

			return taxYearWithPeriods;
		}

		public virtual TaxYearWithPeriods<TTaxYear, TTaxPeriod> CreateWithCorrespondingTaxPeriodType(CreationParams creationParams)
		{
			return creationParams.TaxPeriodType == VendorTaxPeriodType.FiscalPeriod
				? CreateByFinancialPeriod(creationParams.OrganizationID,
											creationParams.TaxAgencyID,
											creationParams.StartDateTime,
											creationParams.PeriodCount)
				: CreateByIndependentTaxCalendar(creationParams);
		}

		public virtual void CreateAndAddToCache(PXCache taxYearCache, PXCache taxPeriodCache, int? branchID, Vendor vendor, int? calendarYear = null, DateTime? baseDate = null)
		{
			TaxYearWithPeriods<TTaxYear, TTaxPeriod> taxYearWithPeriods = Create(branchID, vendor, calendarYear, baseDate);

			taxYearCache.Insert(taxYearWithPeriods.TaxYear);

			foreach (TTaxPeriod taxPeriod in taxYearWithPeriods.TaxPeriods)
			{
				taxPeriodCache.Insert(taxPeriod);
			}
		}

		public virtual TaxYearWithPeriods<TTaxYear, TTaxPeriod> Create(int? organizationID, Vendor vendor, int? calendarYear = null, DateTime? baseDate = null)
		{
			Graph.Caches[typeof(TaxYear)].Clear();
			Graph.Caches[typeof(TaxYear)].ClearQueryCacheObsolete();
			Graph.Caches[typeof(TaxPeriod)].Clear();
			Graph.Caches[typeof(TaxPeriod)].ClearQueryCacheObsolete();

			TTaxYear lastYear = null;

			TaxYear taxYear = TaxYearMaint.FindLastTaxYear(Graph, organizationID, vendor.BAccountID);

			if (taxYear != null)
			{
				lastYear = new TTaxYear();
				Graph.Caches[typeof (TaxYear)].RestoreCopy(lastYear, taxYear);
			}

			CreationParams creationParams = new CreationParams()
			{
				OrganizationID = organizationID.Value,
				TaxAgencyID = vendor.BAccountID.Value,
			};

			if (lastYear != null)
			{
				creationParams.TaxPeriodType = lastYear.TaxPeriodType;

				TaxPeriod lastPeriod = TaxYearMaint.FindLastTaxPeriod(Graph, organizationID, vendor.BAccountID);

				if (lastPeriod != null)
				{
					int yearNumber = Convert.ToInt32(lastYear.Year); 

					if (lastPeriod.TaxYear != lastYear.Year)
					{
						creationParams.PeriodCount = lastYear.PeriodsCount;
					}
					else
					{
						yearNumber++;
					}

					creationParams.StartDateTime = lastPeriod.EndDate.Value;
					creationParams.TaxYearNumber = yearNumber;

					return CreateWithCorrespondingTaxPeriodType(creationParams);
				}
				else
				{
					//no existing periods - only header
					creationParams.StartDateTime = lastYear.StartDate.Value;
					creationParams.TaxYearNumber = Convert.ToInt32(lastYear.Year);
					creationParams.PeriodCount = lastYear.PeriodsCount;

					return CreateWithCorrespondingTaxPeriodType(creationParams);
				}
			}
			else
			{
				FillCalendarDataWhenLastOrganizationTaxYearDoesNotExist(calendarYear.Value, vendor, creationParams, baseDate);

				return CreateWithCorrespondingTaxPeriodType(creationParams);
			}
		}

		public void FillCalendarDataWhenLastOrganizationTaxYearDoesNotExist(int calendarYear, Vendor vendor, CreationParams creationParams, DateTime? baseDate = null)
		{
			TTaxYear otherOrganizationLastYear = null;

		    if (vendor.TaxPeriodType != VendorTaxPeriodType.FiscalPeriod)
		    {
		        TaxYear taxYear = PXSelect<TaxYear,
		                Where<TaxYear.vendorID, Equal<Required<TaxYear.vendorID>>>,
		                OrderBy<Desc<TaxYear.year>>>
		            .SelectWindowed(Graph, 0, 1, vendor.BAccountID);

		        if (taxYear != null)
		        {
		            otherOrganizationLastYear = new TTaxYear();
		            Graph.Caches[typeof(TaxYear)].RestoreCopy(otherOrganizationLastYear, taxYear);
		        }
            }

			if (otherOrganizationLastYear != null)
			{
				int yearShift = Convert.ToInt32(otherOrganizationLastYear.Year) - otherOrganizationLastYear.StartDate.Value.Year;

				creationParams.StartDateTime = new DateTime(calendarYear - yearShift,
															otherOrganizationLastYear.StartDate.Value.Month,
															otherOrganizationLastYear.StartDate.Value.Day);

                creationParams.TaxPeriodType = vendor.TaxPeriodType ?? otherOrganizationLastYear.TaxPeriodType;
			}
			else
			{
				creationParams.TaxPeriodType = vendor.TaxPeriodType;
	
			    if (creationParams.TaxPeriodType != VendorTaxPeriodType.FiscalPeriod)
			    {
			        creationParams.StartDateTime = new DateTime(calendarYear, 1, 1);
                }
			    else
			    {
			        creationParams.StartDateTime = baseDate ?? new DateTime(calendarYear, 1, 1);
                }
			}

			creationParams.TaxYearNumber = calendarYear;
		}

		/// <summary>
		/// Returns TaxPeriod form the given date.
		/// </summary>
		[Obsolete("The method is obsolete and will be removed in Acumatica 8.0.")]
		public static string GetPeriod(int vendorID, DateTime? fromdate)
		{
			using (PXDataRecord record = PXDatabase.SelectSingle<TaxPeriod>(
				new PXDataField(typeof(TaxPeriod.taxPeriodID).Name),
				new PXDataFieldValue(typeof(TaxPeriod.vendorID).Name, PXDbType.Int, vendorID),
				new PXDataFieldValue(typeof(TaxPeriod.startDate).Name, PXDbType.SmallDateTime, 4, fromdate, PXComp.LE),
				new PXDataFieldValue(typeof(TaxPeriod.endDate).Name, PXDbType.SmallDateTime, 4, fromdate, PXComp.GT)))
			{
				if (record != null)
				{
					return record.GetString(0);
				}
			}
			return null;
		}
	}
}
