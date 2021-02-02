using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.TX.Data;
using PX.Objects.TX.Descriptor;
using PX.Web.UI;

namespace PX.Objects.TX
{
	public class TaxYearMaint: PXGraph<TaxYearMaint>
	{
		#region TaxPeriod repository methods

		public static TaxPeriod FindTaxPeriodByKey(PXGraph graph, int? organizationID, int? taxAgencyID, string taxPeriodID)
		{
			return PXSelect<TaxPeriod,
								Where<TaxPeriod.organizationID, Equal<Required<TaxPeriod.organizationID>>,
										And< TaxPeriod.vendorID, Equal<Required<TaxPeriod.vendorID>>,
										And<TaxPeriod.taxPeriodID, Equal<Required<TaxPeriod.taxPeriodID>>>>>>
								.Select(graph, organizationID, taxAgencyID, taxPeriodID);
		}

		public static TaxPeriod GetTaxPeriodByKey(PXGraph graph, int? organizationID, int? taxAgencyID, string taxPeriodID)
		{
			var taxPeriod = FindTaxPeriodByKey(graph, organizationID, taxAgencyID, taxPeriodID);

			if (taxPeriod == null)
				throw new PXException(Messages.ReportingPeriodDoesNotExistForTheTaxAgency,
					taxPeriodID, VendorMaint.GetByID(graph, taxAgencyID).AcctCD.Trim());

			return taxPeriod;
		}

		public static TaxPeriod FindFirstOpenTaxPeriod(PXGraph graph, int? organizationID, int? taxAgencyID)
		{
			return PXSelect<TaxPeriod,
								Where<TaxPeriod.organizationID, Equal<Required<TaxPeriod.organizationID>>,
										And<TaxPeriod.vendorID, Equal<Required<TaxPeriod.vendorID>>,
										And<TaxPeriod.status, Equal<TaxPeriodStatus.open>>>>,
								OrderBy<Asc<TaxPeriod.taxPeriodID>>>
								.SelectWindowed(graph, 0, 1, organizationID, taxAgencyID);
		}

		public static TaxPeriod FindPreparedPeriod(PXGraph graph, int? organizationID, int? taxAgencyID)
		{
			return PXSelect<TaxPeriod,
							Where<TaxPeriod.organizationID, Equal<Required<TaxPeriod.organizationID>>,
									And<TaxPeriod.vendorID, Equal<Required<TaxPeriod.vendorID>>,
									And<TaxPeriod.status, Equal<TaxPeriodStatus.prepared>>>>>
							.Select(graph, organizationID, taxAgencyID);

		}

		public static TaxPeriod FindLastClosedPeriod(PXGraph graph, int? organizationID, int? taxAgencyID)
		{
			return PXSelect<TaxPeriod,
							Where<TaxPeriod.organizationID, Equal<Required<TaxPeriod.organizationID>>,
									And<TaxPeriod.vendorID, Equal<Required<TaxPeriod.vendorID>>,
									And<TaxPeriod.status, Equal<TaxPeriodStatus.closed>>>>,
							OrderBy<Desc<TaxPeriod.taxPeriodID>>>
							.SelectWindowed(graph, 0, 1, organizationID, taxAgencyID);

		}

		public static TaxPeriod FindLastTaxPeriod(PXGraph graph, int? organizationID, int? taxAgencyID)
		{
			return PXSelect<TaxPeriod,
								Where<TaxPeriod.organizationID, Equal<Required<TaxPeriod.organizationID>>,
										And<TaxPeriod.vendorID, Equal<Required<TaxPeriod.vendorID>>>>,
								OrderBy<Desc<TaxPeriod.taxPeriodID>>>
								.SelectWindowed(graph, 0, 1, organizationID, taxAgencyID);
		}

		public static TaxPeriod FinTaxPeriodByDate(PXGraph graph, int? organizationID, int? taxAgencyID, DateTime? date)
		{
			return PXSelect<TaxPeriod,
					   Where<TaxPeriod.organizationID, Equal<Required<TaxPeriodFilter.organizationID>>,
							    And<TaxPeriod.vendorID, Equal<Required<TaxPeriod.vendorID>>,
							    And<TaxPeriod.startDate, LessEqual<Required<TaxPeriod.startDate>>,
							    And<TaxPeriod.endDate, Greater<Required<TaxPeriod.endDate>>>>>>>
							    .Select(graph, organizationID, taxAgencyID, date, date);
		}

		#endregion


		#region TaxYear repository methods

		public static TaxYear FindTaxYearByKey(PXGraph graph, int? organizationID, int? taxAgencyID, string year)
		{
			return PXSelect<TaxYear,
							Where<TaxYear.organizationID, Equal<Required<TaxYear.organizationID>>,
									And<TaxYear.vendorID, Equal<Required<TaxYear.vendorID>>,
									And<TaxYear.year, Equal<Required<TaxYear.year>>>>>>
							.Select(graph, organizationID, taxAgencyID, year);
		}

		public static TaxYear FindLastTaxYear(PXGraph graph, int? branchID, int? taxAgencyID)
		{
			return PXSelect<TaxYear,
							Where<TaxYear.organizationID, Equal<Required<TaxYear.organizationID>>,
									And<TaxYear.vendorID, Equal<Required<TaxYear.vendorID>>>>,
							OrderBy<Desc<TaxYear.year>>>
							.SelectWindowed(graph, 0, 1, branchID, taxAgencyID);
		}

		#endregion


		#region Specific repository methods

		protected static TaxYearEx FindTaxYearExByKey(PXGraph graph, int? organizationID, int? taxAgencyID, string year)
		{
			return PXSelect<TaxYearEx,
							Where<TaxYearEx.organizationID, Equal<Required<TaxYearEx.organizationID>>,
									And<TaxYearEx.vendorID, Equal<Required<TaxYearEx.vendorID>>,
									And<TaxYearEx.year, Equal<Required<TaxYearEx.year>>>>>>
							.Select(graph, organizationID, taxAgencyID, year);
		}

		#endregion


		#region DACs

		public class TaxYearFilter : PX.Data.IBqlTable
		{
			#region OrganizationID

			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

			/// <summary>
			/// The reference to the <see cref="Organization"/> record to which the record belongs.
			/// </summary>
			[Organization(Required = true, IsDBField = false)]
			public virtual Int32? OrganizationID { get; set; }
			
			#endregion

			#region VendorID

			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

			/// <summary>
			/// The reference to the tax agency (<see cref="Vendor"/>) record to which the record belongs.
			/// </summary>
			[TaxAgencyActive(Required = true)]
			public virtual Int32? VendorID { get; set; }

			#endregion

			#region Year

			public abstract class year : PX.Data.BQL.BqlString.Field<year> { }

			[PXUIField(DisplayName = "Tax Year", Required = true)]
			[PXString(4, IsFixed = true)]
			[TaxYearSelector]
			public virtual String Year { get; set; }

			#endregion

			#region ShortTaxYear

			public abstract class shortTaxYear : PX.Data.BQL.BqlBool.Field<shortTaxYear> { }

			[PXBool]
			[PXUIField(DisplayName = "Short Tax Year")]
			public virtual bool? ShortTaxYear { get; set; }
			
			#endregion

			#region StartDate

			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

			[PXDate()]
			[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
			public virtual DateTime? StartDate { get; set; }

			#endregion

			#region TaxPeriodType

			public abstract class taxPeriodType : PX.Data.BQL.BqlString.Field<taxPeriodType> { }

			protected String _TaxPeriodType;

			[PXDBString(1)]
			[PXDefault(VendorTaxPeriodType.Monthly)]
			[PXUIField(DisplayName = "Tax Period Type")]
			[VendorTaxPeriodType.List()]
			public virtual String TaxPeriodType
			{
				get { return this._TaxPeriodType; }
				set { this._TaxPeriodType = value; }
			}

			#endregion

			[PXBool]
			public bool? IsStartDateEditable { get; set; }

			[PXBool]
			public bool? IsTaxPeriodTypeEditable { get; set; }

			protected class TaxYearSelector : PXCustomSelectorAttribute
			{
				public TaxYearSelector() : base(typeof(Search<TaxYear.year,
														Where<TaxYear.organizationID, Equal<Current<TaxYearFilter.organizationID>>,
																And<TaxYear.vendorID, Equal<Current<TaxYearFilter.vendorID>>>>,
														OrderBy<Desc<TaxYear.year>>>),
												typeof(TaxYear.year))
				{
				}

				protected virtual IEnumerable GetRecords()
				{
					var taxYearFilter = (TaxYearFilter)_Graph.Caches[typeof(TaxYearFilter)].Current;

					if (taxYearFilter.OrganizationID == null || taxYearFilter.VendorID == null)
						return new object[0];

					PXCache taxYearCache = _Graph.Caches[typeof (TaxYear)];

					string viewName = $"{GenerateViewName()}origSelect_";

					PXView view;

					if (!_Graph.Views.TryGetValue(viewName, out view))
					{
						view = new PXView(_Graph, !_DirtyRead, _OriginalSelect);

						_Graph.Views[viewName] = view;
					}

					List<TaxYear> taxYears = view.SelectMultiBound(new object[] { taxYearFilter })
													.RowCast<TaxYear>()
													.ToList();

					if (taxYears.Any())
					{
						TaxPeriod lastPeriod = FindLastTaxPeriod(_Graph, taxYearFilter.OrganizationID, taxYearFilter.VendorID);

						TaxYear lastYear = taxYears.First();

						if (lastPeriod != null && lastPeriod.TaxYear == lastYear.Year)
						{
							lastYear = (TaxYear) taxYearCache.CreateCopy(lastYear);

							lastYear.Year = (Convert.ToInt32(lastYear.Year) + 1).ToString();

							taxYears.Insert(0, lastYear);
						}
					}
					else
					{
						Vendor vendor = VendorMaint.GetByID(_Graph, taxYearFilter.VendorID);

						TaxTran earliestTaxTran = ReportTax.GetEarliestNotReportedTaxTran(_Graph, vendor, taxYearFilter.OrganizationID, null);

						if (earliestTaxTran != null)
						{
							TaxCalendar<TaxYear, TaxPeriod> taxCalendar = new TaxCalendar<TaxYear, TaxPeriod>(_Graph);

							DateTime? tranDate = vendor.TaxReportFinPeriod == true
																? earliestTaxTran.FinDate
																: earliestTaxTran.TranDate;

							TaxYearWithPeriods<TaxYear, TaxPeriod> taxYearWithPeriods = taxCalendar.Create(taxYearFilter.OrganizationID,
																											vendor,
																											tranDate.Value.Year);

							taxYears.Add(taxYearWithPeriods.TaxYear);
						}
						else
						{
							List<string> years = PXSelect<FinYear, Where<FinYear.organizationID, Equal<Required<FinPeriod.organizationID>>>>.Select(_Graph, taxYearFilter.OrganizationID)
																	.RowCast<FinYear>()
																	.Select(finYear => finYear.Year)
																	.ToList();

							years.Insert(0, (Convert.ToInt32(years.First()) - 1).ToString());
							years.Add((Convert.ToInt32(years.Last()) + 1).ToString());

							taxYears = years.Select(year => new TaxYear() {Year = year}).ToList();
						}
					}

					return taxYears;
				}
			}
		}

		public class TaxYearEx : TaxYear
		{
			public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
			public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			public new abstract class year : PX.Data.BQL.BqlString.Field<year> { }

			[PXBool]
			public bool? TaxPeriodsInDBExist { get; set; }

			[PXBool]
			public bool? Existing { get; set; }
		}

		public class TaxPeriodEx : TaxPeriod
		{
			public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
			public new abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }
			public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			public new abstract class taxYear : PX.Data.BQL.BqlString.Field<taxYear> { }
			public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }

			[PXBaseCury]
			[PXUIField(DisplayName = Messages.NetTaxAmount)]
			public decimal? NetTaxAmt { get; set; }
		}

		#endregion


		#region Actions

		public PXSave<TaxYearFilter>Save;
		public PXCancel<TaxYearFilter> Cancel;
		public PXDelete<TaxYearFilter> Delete;

		public PXMenuAction<TaxYearFilter> ActionsMenu;

		public PXAction<TaxYearFilter> StubAction;

		public PXAction<TaxYearFilter> RedirectToReleaseTaxReport;
		public PXAction<TaxYearFilter> RedirectToPrepareTaxReport;
		

		public PXMenuAction<TaxYearFilter> ReportsMenu;

		
		public PXAction<TaxYearFilter> RedirectToTaxDetailsReport;
		public PXAction<TaxYearFilter> RedirectToTaxSummaryReport;

		public PXAction<TaxYearFilter> AddPeriod;
		public PXAction<TaxYearFilter> DeletePeriod;

		public PXAction<TaxYearFilter> ViewTaxPeriodDetails;

		#endregion


		#region Views

		public PXFilter<TaxYearFilter> TaxYearFilterSelectView;

		public PXSelect<TaxPeriod> TaxPeriodSelectView;

		public PXSelect<TaxPeriodEx> TaxPeriodExSelectView;

		public PXSelect<TaxYear> TaxYearSelectView;

		public PXSelect<TaxYearEx> TaxYearExSelectView;

		#endregion


		protected TaxCalendar<TaxYear, TaxPeriod> TaxCalendar;

		protected TaxCalendar<TaxYearEx, TaxPeriodEx> TaxCalendarEx;

		public override bool IsDirty => TaxYearExSelectView.Cache.IsDirty || TaxPeriodExSelectView.Cache.IsDirty;

		public TaxYearMaint()
		{
			TaxCalendar = new TaxCalendar<TaxYear, TaxPeriod>(this);
			TaxCalendarEx = new TaxCalendar<TaxYearEx, TaxPeriodEx>(this);

			Delete.StateSelectingEvents += DeleteButtonFieldSelectingHandler;
			AddPeriod.StateSelectingEvents += AddPeriodButtonFieldSelectingHandler;
			DeletePeriod.StateSelectingEvents += DeletePeriodButtonFieldSelectingHandler;

			ActionsMenu.AddMenuAction(RedirectToPrepareTaxReport);
			ActionsMenu.AddMenuAction(RedirectToReleaseTaxReport);
			
			ReportsMenu.AddMenuAction(RedirectToTaxSummaryReport);
			ReportsMenu.AddMenuAction(RedirectToTaxDetailsReport);

			PXUIFieldAttribute.SetReadOnly(TaxPeriodExSelectView.Cache, null, true);
		}

		protected virtual IEnumerable taxPeriodExSelectView()
		{
			TaxYearEx taxYear = TaxYearExSelectView.Current;

			if (taxYear == null)
				return new object[0];

			return GetActualStoredTaxPeriods();
		}

		public override void Persist()
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				TaxYearEx taxYear = TaxYearExSelectView.Current;

				if (TaxYearExSelectView.Cache.GetStatus(taxYear) == PXEntryStatus.Updated
				    && taxYear.Existing != true)
				{
					TaxYearExSelectView.Cache.SetStatus(taxYear, PXEntryStatus.Inserted);
				}

				List<TaxPeriodEx> persitableToBeShownPeriods = TaxPeriodExSelectView.Cache.Cached.Cast<TaxPeriodEx>()
																									.Where(period =>
																									{
																										PXEntryStatus status = TaxPeriodExSelectView.Cache.GetStatus(period);
																										return status == PXEntryStatus.Inserted
																										       || status == PXEntryStatus.Updated;
																									})
																									.ToList();

				TaxPeriodEx lastPersistablePeriod = TaxPeriodExSelectView.Cache.Cached.Cast<TaxPeriodEx>()
																						.Where(period =>
																						{
																							PXEntryStatus status = TaxPeriodExSelectView.Cache.GetStatus(period);
																							return status == PXEntryStatus.Inserted
																								   || status == PXEntryStatus.Updated
																								   || status == PXEntryStatus.Deleted;
																						})
																						.OrderByDescending(period => period.TaxPeriodID)
																						.FirstOrDefault();

				if (lastPersistablePeriod != null)
				{
					TaxYear yearAfterLastPersistable = PXSelect<TaxYear,
																	Where<TaxYear.organizationID, Equal<Required<TaxYear.organizationID>>,
																			And<TaxYear.vendorID, Equal<Required<TaxYear.vendorID>>,
																			And<TaxYear.year, Greater<Required<TaxYear.year>>>>>>
																	.SelectWindowed(this, 0, 1,
																					lastPersistablePeriod.OrganizationID,
																					lastPersistablePeriod.VendorID,
																					lastPersistablePeriod.TaxYear);

					if (yearAfterLastPersistable != null)
					{
						TaxYearFilterSelectView.Ask(Messages.SubsequentTaxYearsWillBeDeleted, MessageButtons.OK);

						PXDatabase.Delete<TaxYear>(
						new PXDataFieldRestrict(typeof(TaxYear.organizationID).Name, PXDbType.Int, 4, taxYear.OrganizationID, PXComp.EQ),
						new PXDataFieldRestrict(typeof(TaxYear.vendorID).Name, PXDbType.Int, 4, taxYear.VendorID, PXComp.EQ),
						new PXDataFieldRestrict(typeof(TaxYear.year).Name, PXDbType.Char, 4, lastPersistablePeriod.TaxYear, PXComp.GT));

						PXDatabase.Delete<TaxPeriod>(
						new PXDataFieldRestrict(typeof(TaxPeriod.organizationID).Name, PXDbType.Int, 4, taxYear.OrganizationID, PXComp.EQ),
						new PXDataFieldRestrict(typeof(TaxPeriod.vendorID).Name, PXDbType.Int, 4, taxYear.VendorID, PXComp.EQ),
						new PXDataFieldRestrict(typeof(TaxPeriod.taxPeriodID).Name, PXDbType.Char, 6, lastPersistablePeriod.TaxPeriodID, PXComp.GT));
					}
				}

				base.Persist();

				ts.Complete();

				foreach (TaxPeriodEx taxPeriod in persitableToBeShownPeriods)
				{
					TaxPeriodExSelectView.Cache.SetStatus(taxPeriod, PXEntryStatus.Held);
				}
			}			
		}


		#region Button state handlers

		protected virtual void DeleteButtonFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = PXButtonState.CreateDefaultState<TaxYearFilter>(e.ReturnState);

			TaxPeriod anyTaxPeriodInYear = PXSelect<TaxPeriod,
													Where<TaxPeriod.organizationID, Equal<Current<TaxYearEx.organizationID>>,
															And<TaxPeriod.vendorID, Equal<Current<TaxYearEx.vendorID>>,
															And<TaxPeriod.taxYear, Equal<Current<TaxYearEx.year>>>>>>
													.SelectSingleBound(this, new object[]{ TaxYearExSelectView.Current });

			((PXButtonState) e.ReturnState).Enabled = TaxYearExSelectView.Current != null 
															&& TaxYearExSelectView.Current.Existing == true
															&& anyTaxPeriodInYear == null;
		}

		protected virtual void DeletePeriodButtonFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PeriodButtonsFieldSelectingHandler(e);

			((PXButtonState) e.ReturnState).Enabled &= TaxYearExSelectView.Current != null 
														&& TaxYearExSelectView.Current.PeriodsCount > 0
														&& GetActualStoredTaxPeriods().Last().Status == TaxPeriodStatus.Open;
		}

		protected virtual void AddPeriodButtonFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PeriodButtonsFieldSelectingHandler(e);

			((PXButtonState) e.ReturnState).Enabled &= TaxYearExSelectView.Current != null 
														&& TaxYearExSelectView.Current.PeriodsCount < TaxYearExSelectView.Current.PlanPeriodsCount;
		}

		#endregion


		#region Event handlers

		protected virtual void TaxYearFilter_ShortTaxYear_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			if ((bool?)e.NewValue == true)
			{
				if (TaxYearFilterSelectView.Ask(Messages.AreSureToShortenTaxYear, MessageButtons.YesNo) != WebDialogResult.Yes)
				{
					e.NewValue = null;
				}
			}
		}

		protected virtual void TaxYearEx_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			TaxYearEx taxYear = e.Row as TaxYearEx;

			if (taxYear == null)
				return;

			if (e.TranStatus == PXTranStatus.Completed)
			{
				TaxYearFilter taxYearFilter = TaxYearFilterSelectView.Current;

				if (taxYearFilter != null)
				{
					taxYearFilter.ShortTaxYear = !(taxYear.PlanPeriodsCount == taxYear.PeriodsCount);
					PXUIFieldAttribute.SetEnabled<TaxYearFilter.shortTaxYear>(cache, null, taxYearFilter.ShortTaxYear != true);
				}

				taxYear.Existing = true;
			}
		}

		protected virtual void TaxYearFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			TaxYearFilter taxYearFilter = e.Row as TaxYearFilter;

			if (taxYearFilter == null)
				return;
			
			PXUIFieldAttribute.SetEnabled<TaxYearFilter.startDate>(cache, null, TaxYearExSelectView.Current != null);
			PXUIFieldAttribute.SetEnabled<TaxYearFilter.taxPeriodType>(cache, null, TaxYearExSelectView.Current != null);

			taxYearFilter.ShortTaxYear |= TaxYearExSelectView.Current?.PeriodsCount < TaxYearExSelectView.Current?.PlanPeriodsCount;
			PXUIFieldAttribute.SetEnabled<TaxYearFilter.shortTaxYear>(cache, null,
				taxYearFilter.ShortTaxYear != true && TaxYearExSelectView.Current != null);

			PXUIFieldAttribute.SetEnabled<TaxYearFilter.startDate>(cache, null, taxYearFilter.IsStartDateEditable == true);
			PXUIFieldAttribute.SetEnabled<TaxYearFilter.taxPeriodType>(cache, null, taxYearFilter.IsTaxPeriodTypeEditable == true);

			TaxPeriodExSelectView.Cache.AllowDelete = taxYearFilter.ShortTaxYear == true;
			TaxPeriodExSelectView.Cache.AllowInsert = taxYearFilter.ShortTaxYear == true;
		}

		protected virtual void TaxYearFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			TaxYearFilter taxYearFilter = e.Row as TaxYearFilter;
			TaxYearFilter oldTaxYearFilter = e.OldRow as TaxYearFilter;

			if (taxYearFilter == null || oldTaxYearFilter == null)
				return;

			if (!TaxYearFilterSelectView.Cache.ObjectsEqual<TaxYearFilter.organizationID, TaxYearFilter.vendorID>(taxYearFilter, oldTaxYearFilter))
			{
				if (taxYearFilter.OrganizationID != null
				    && taxYearFilter.VendorID != null)
				{
					TaxPeriod defaultTaxPeriod = TaxCalendar.GetOrCreateCurrentTaxPeriod(TaxYearSelectView.Cache,
																							TaxPeriodSelectView.Cache,
																							taxYearFilter.OrganizationID,
																							taxYearFilter.VendorID);

					taxYearFilter.Year = defaultTaxPeriod?.TaxYear;

				    Vendor vendor = VendorMaint.GetByID(this, taxYearFilter.VendorID);

				    if (vendor.TaxPeriodType == VendorTaxPeriodType.FiscalPeriod)
				    {
				        TaxYear taxYear = PXSelect<TaxYear, Where<TaxYear.year, Equal<Required<TaxYear.year>>>>
				                                .Select(this, taxYearFilter.Year);

				        taxYearFilter.StartDate = taxYear?.StartDate;

				    }

					TaxYearSelectView.Cache.Clear();
					TaxYearSelectView.Cache.ClearQueryCacheObsolete();
					TaxPeriodSelectView.Cache.Clear();
					TaxPeriodSelectView.Cache.ClearQueryCacheObsolete();
				}
				else
				{
					taxYearFilter.Year = null;
				}
			}

			if (taxYearFilter.OrganizationID != null 
				&& taxYearFilter.VendorID != null
				&& taxYearFilter.Year != null)
			{
				TaxYearEx currentTaxYear = null;

				if (!TaxYearFilterSelectView.Cache.ObjectsEqual<TaxYearFilter.organizationID, TaxYearFilter.vendorID, TaxYearFilter.year>(
					taxYearFilter, oldTaxYearFilter))
				{
					TaxYearExSelectView.Cache.Clear();
					TaxYearExSelectView.Cache.ClearQueryCacheObsolete();
					TaxPeriodExSelectView.Cache.Clear();
					TaxPeriodExSelectView.Cache.ClearQueryCacheObsolete();

					currentTaxYear = GetOrCreateTaxYearWithPeriodsInCacheByFilter(taxYearFilter);

				    if (currentTaxYear == null)
				        return;

					taxYearFilter.TaxPeriodType = currentTaxYear.TaxPeriodType;
					taxYearFilter.StartDate = currentTaxYear.StartDate;
					taxYearFilter.ShortTaxYear = currentTaxYear.PlanPeriodsCount != currentTaxYear.PeriodsCount;

					taxYearFilter.IsTaxPeriodTypeEditable = IsTaxPeriodTypeEditable(taxYearFilter);
				}
				else
				{
					currentTaxYear = TaxYearExSelectView.Current;

					if (oldTaxYearFilter.TaxPeriodType != taxYearFilter.TaxPeriodType
						|| oldTaxYearFilter.StartDate != taxYearFilter.StartDate)
					{

						if (taxYearFilter.TaxPeriodType == VendorTaxPeriodType.FiscalPeriod && taxYearFilter.StartDate != null)
						{
							FinYear finYear = PXSelect<FinYear, 
									Where<FinYear.startDate, Equal<Required<FinYear.startDate>>, 
										And<FinYear.year, Equal<Required<FinYear.year>>,
										And<FinYear.organizationID, Equal<Required<FinPeriod.organizationID>>>>>>
								.SelectWindowed(this, 0, 1, taxYearFilter.StartDate, taxYearFilter.Year, taxYearFilter.OrganizationID);

							if (finYear == null)
								throw new PXException(Messages.TaxYearStartDateDoesNotMatch);
						}
						if (oldTaxYearFilter.TaxPeriodType != taxYearFilter.TaxPeriodType)
						{
							taxYearFilter.ShortTaxYear = false;
							currentTaxYear.PeriodsCount = null;
						}

						currentTaxYear.TaxPeriodType = taxYearFilter.TaxPeriodType;
						currentTaxYear.StartDate = taxYearFilter.StartDate;

						TaxYearExSelectView.Update(currentTaxYear);

						if (currentTaxYear.StartDate != null && currentTaxYear.TaxPeriodType != null)
						{
							RegeneratePeriodsAndPutIntoCache(currentTaxYear);
						}
					}
				}

				TaxYearExSelectView.Current = currentTaxYear;
			}
			else
			{
				TaxYearExSelectView.Current = null;

				taxYearFilter.TaxPeriodType = null;
				taxYearFilter.StartDate = null;
				taxYearFilter.ShortTaxYear = null;
				taxYearFilter.IsStartDateEditable = null;
			}

		    taxYearFilter.IsStartDateEditable = IsStartDateEditable(taxYearFilter);
        }

		#endregion


		#region Action Handlers

		[PXDeleteButton(ConfirmationType = PXConfirmationType.Always, ClosePopup = false, ImageKey = Sprite.Main.Remove)]
		[PXUIField(DisplayName = ActionsMessages.Delete, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
		protected virtual IEnumerable delete(PXAdapter adapter)
		{
			TaxYearExSelectView.Delete(TaxYearExSelectView.Current);

			Actions.PressSave();

			TaxYearFilterSelectView.Current.Year = null;

			TaxYearFilterSelectView.Update(TaxYearFilterSelectView.Current);

			return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = ActionsMessages.Delete, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
		protected virtual IEnumerable deletePeriod(PXAdapter adapter)
		{
			TaxYearEx taxYear = TaxYearExSelectView.Current;

			if (taxYear == null)
				return adapter.Get();

			TaxPeriodEx lastPeriod = GetActualStoredTaxPeriods().Last();

			if (taxYear.TaxPeriodsInDBExist == true)
			{
				TaxPeriodExSelectView.Cache.Delete(lastPeriod);
			}
			else
			{
				TaxPeriodExSelectView.Cache.Remove(lastPeriod);
			}

			taxYear.PeriodsCount--;

			TaxYearExSelectView.Update(taxYear);

			return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = ActionsMessages.Insert, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		protected virtual IEnumerable addPeriod(PXAdapter adapter)
		{
			TaxYearEx taxYear = TaxYearExSelectView.Current;

			if (taxYear == null)
				return adapter.Get();

			taxYear.PeriodsCount++;

			TaxYearExSelectView.Update(taxYear);

			TaxCalendar.CreationParams creationParams = TX.TaxCalendar.CreationParams.FromTaxYear(taxYear);

			TaxYearWithPeriods<TaxYearEx, TaxPeriodEx> taxYearWithPeriods =
				TaxCalendarEx.CreateWithCorrespondingTaxPeriodType(creationParams);

			TaxPeriodEx addedTaxPeriod = taxYearWithPeriods.TaxPeriods.Last();

			if (TaxPeriodExSelectView.Cache.GetStatus(addedTaxPeriod) == PXEntryStatus.Deleted)
			{
				TaxPeriodExSelectView.Cache.SetStatus(addedTaxPeriod, PXEntryStatus.Held);
			}
			else
			{
				addedTaxPeriod = (TaxPeriodEx)TaxPeriodExSelectView.Cache.Insert(addedTaxPeriod);

				if (taxYear.TaxPeriodsInDBExist != true)
				{
					TaxPeriodExSelectView.Cache.SetStatus(addedTaxPeriod, PXEntryStatus.Held);
				}
			}

			return adapter.Get();
		}

		[PXUIField(DisplayName = Common.Messages.Actions)]
		[PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable actionsMenu(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = Messages.PrepareTaxReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable redirectToPrepareTaxReport(PXAdapter adapter)
		{
			if (TaxYearFilterSelectView.Current?.OrganizationID != null
			    && TaxYearFilterSelectView.Current.VendorID != null)
			{
				ReportTax reportTax = PXGraph.CreateInstance<ReportTax>();

				TaxPeriodFilter taxPeriodFilter = (TaxPeriodFilter)reportTax.Period_Header.Cache.CreateCopy(reportTax.Period_Header.Current);

				taxPeriodFilter.OrganizationID = TaxYearFilterSelectView.Current.OrganizationID;
				taxPeriodFilter.VendorID = TaxYearFilterSelectView.Current.VendorID;

				reportTax.Period_Header.Update(taxPeriodFilter);

				throw new PXRedirectRequiredException(reportTax, true, string.Empty)
				{
					Mode = PXBaseRedirectException.WindowMode.Same
				};
			}

			return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = Messages.ReleaseTaxReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable redirectToReleaseTaxReport(PXAdapter adapter)
		{
			if (TaxYearFilterSelectView.Current?.OrganizationID != null
			    && TaxYearFilterSelectView.Current.VendorID != null)
			{
				ReportTaxReview reportTaxReview = PXGraph.CreateInstance<ReportTaxReview>();

				TaxPeriodFilter taxPeriodFilter = (TaxPeriodFilter)reportTaxReview.Period_Header.Cache.CreateCopy(reportTaxReview.Period_Header.Current);

				taxPeriodFilter.OrganizationID = TaxYearFilterSelectView.Current.OrganizationID;
				taxPeriodFilter.VendorID = TaxYearFilterSelectView.Current.VendorID;

				reportTaxReview.Period_Header.Update(taxPeriodFilter);

				throw new PXRedirectRequiredException(reportTaxReview, true, string.Empty)
				{
					Mode = PXBaseRedirectException.WindowMode.Same
				};
			}

			return adapter.Get();
		}

		[PXUIField(DisplayName = Common.Messages.Reports)]
		[PXButton(MenuAutoOpen = true)]
		protected virtual IEnumerable reportsMenu(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = Messages.TaxSummary, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable redirectToTaxSummaryReport(PXAdapter adapter)
		{
			TryRedirectToReport("TX621000");

			return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = Messages.TaxDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable redirectToTaxDetailsReport(PXAdapter adapter)
		{
			TryRedirectToReport("TX620500");

			return adapter.Get();
		}

		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable viewTaxPeriodDetails(PXAdapter adapter) 
		{
			ReportTaxDetail graph = CreateInstance<ReportTaxDetail>();

			TaxPeriodEx taxPeriod = TaxPeriodExSelectView.Current;

			graph.History_Header.Current.OrganizationID = taxPeriod.OrganizationID;
			graph.History_Header.Current.BranchID = null;
			graph.History_Header.Current.VendorID = taxPeriod.VendorID;
			graph.History_Header.Current.TaxPeriodID = taxPeriod.TaxPeriodID;

			TaxReportLine line = PXSelect<TaxReportLine, Where<TaxReportLine.vendorID, Equal<Required<TaxReportLine.vendorID>>,
																And<TaxReportLine.netTax, Equal<True>>>>
														 .Select(this, taxPeriod.VendorID);

			graph.History_Header.Current.LineNbr = line?.LineNbr;

			throw new PXRedirectRequiredException(graph, String.Empty)
			{
				Mode = PXBaseRedirectException.WindowMode.Same
			};
		}

		#endregion


		#region Service Methods

		protected virtual void TryRedirectToReport(string reportPage)
		{
			if (TaxYearFilterSelectView.Current?.OrganizationID != null
				&& TaxYearFilterSelectView.Current.VendorID != null
				&& TaxPeriodExSelectView.Current != null)
			{
				Organization organization =
					OrganizationMaint.FindOrganizationByID(this, TaxYearFilterSelectView.Current.OrganizationID);

				BAccountR bAccount = SelectFrom<BAccountR>
					.Where<BAccountR.bAccountID.IsEqual<@P.AsInt>>
					.View
					.Select(this, organization.BAccountID);

				var reportParams = new Dictionary<string, string>
				{
					["OrgBAccountID"] = bAccount.AcctCD,
					["VendorID"] = VendorMaint.GetByID(this, TaxYearFilterSelectView.Current.VendorID).AcctCD,
					["TaxPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(TaxPeriodExSelectView.Current.TaxPeriodID)
				};

				throw new PXRedirectWithReportException(this, new PXReportRequiredException(reportParams, reportPage, string.Empty),
					string.Empty);
			}
		}

		protected virtual bool IsStartDateEditable(TaxYearFilter taxYearFilter)
		{
		    if (taxYearFilter.OrganizationID == null
		        || taxYearFilter.VendorID == null)
		        return false;

			TaxYear anyTaxYearExcludingSelected = PXSelect<TaxYear,
																Where<TaxYear.organizationID, Equal<Required<TaxYear.organizationID>>,
																		And<TaxYear.vendorID, Equal<Required<TaxYear.vendorID>>,
																		And<TaxYear.year, NotEqual<Required<TaxYear.year>>>>>>
																.SelectWindowed(this, 0, 1,
																		(int)taxYearFilter.OrganizationID,
																		(int)taxYearFilter.VendorID,
																		taxYearFilter.Year);

			TaxPeriod anyNotOpenedPeriod = PXSelect<TaxPeriod,
													Where<TaxPeriod.organizationID, Equal<Required<TaxPeriod.organizationID>>,
															And<TaxPeriod.vendorID, Equal<Required<TaxPeriod.vendorID>>,
															And<TaxPeriod.status, NotEqual<TaxPeriodStatus.open>>>>>
													.SelectWindowed(this, 0, 1,
														(int)taxYearFilter.OrganizationID,
														(int)taxYearFilter.VendorID);

		    return anyTaxYearExcludingSelected == null
		           && anyNotOpenedPeriod == null
		           && taxYearFilter.TaxPeriodType != VendorTaxPeriodType.FiscalPeriod;
		}

		protected virtual bool IsTaxPeriodTypeEditable(TaxYearFilter taxYearFilter)
		{
			TaxYear lastTaxYear = FindLastTaxYear(this, taxYearFilter.OrganizationID, taxYearFilter.VendorID);

			if (lastTaxYear == null)
				return true;

			if (string.CompareOrdinal(taxYearFilter.Year, lastTaxYear.Year) < 0)
				return false;

			TaxPeriod anyNotOpenedPeriod = PXSelect<TaxPeriod,
													Where<TaxPeriod.organizationID, Equal<Required<TaxPeriod.organizationID>>,
															And<TaxPeriod.vendorID, Equal<Required<TaxPeriod.vendorID>>,
															And<TaxPeriod.taxYear, Equal<Required<TaxPeriod.taxYear>>,
															And<TaxPeriod.status, NotEqual<TaxPeriodStatus.open>>>>>>
													.SelectWindowed(this, 0, 1,
															taxYearFilter.OrganizationID,
															taxYearFilter.VendorID,
															taxYearFilter.Year);

			return anyNotOpenedPeriod == null;
		}

		protected virtual TaxYearEx GetOrCreateTaxYearWithPeriodsInCacheByFilter(TaxYearFilter taxYearFilter)
		{
			TaxYearEx currentTaxYear = FindTaxYearExByKey(this,
														taxYearFilter.OrganizationID,
														taxYearFilter.VendorID,
														taxYearFilter.Year);

			List<TaxPeriodEx> createdTaxPeriods = null;

			if (currentTaxYear != null)
			{
				currentTaxYear.Existing = true;

				TaxPeriodEx anyExistingTaxPeriod = PXSelect<TaxPeriodEx,
																Where<TaxPeriodEx.organizationID, Equal<Required<TaxPeriodEx.organizationID>>,
																		And<TaxPeriodEx.vendorID, Equal<Required<TaxPeriodEx.vendorID>>,
																		And<TaxPeriodEx.taxYear, Equal<Required<TaxPeriodEx.taxYear>>>>>>
																.SelectWindowed(this, 0, 1,
																		(int) taxYearFilter.OrganizationID,
																		(int) taxYearFilter.VendorID,
																		taxYearFilter.Year);



				currentTaxYear.TaxPeriodsInDBExist = anyExistingTaxPeriod != null;

				if (currentTaxYear.TaxPeriodsInDBExist == true)
				{
					using (new PXReadBranchRestrictedScope(currentTaxYear.OrganizationID.SingleToArray(), null, restrictByAccessRights:false))
					{
						#pragma warning disable PX1015
						IEnumerable<PXResult<TaxPeriodEx, TaxReportLine, TaxHistory>> periodsWithData = 
												PXSelectJoinGroupBy<TaxPeriodEx,
																LeftJoin<TaxReportLine,
																		On<TaxPeriodEx.vendorID, Equal<TaxReportLine.vendorID>,
																			And<TaxReportLine.netTax, Equal<True>>>,
																LeftJoin<TaxHistory,
																		On<TaxPeriodEx.vendorID, Equal<TaxHistory.vendorID>,
																			And<TaxReportLine.lineNbr, Equal<TaxHistory.lineNbr>,
																			And<TaxPeriodEx.taxPeriodID, Equal<TaxHistory.taxPeriodID>>>>>>,
																Where<TaxPeriodEx.organizationID, Equal<Current<TaxYearEx.organizationID>>,
																		And<TaxPeriodEx.vendorID, Equal<Current<TaxYearEx.vendorID>>,
																		And<TaxPeriodEx.taxYear, Equal<Current<TaxYearEx.year>>>>>,
																Aggregate<GroupBy<TaxPeriodEx.taxPeriodID,
																					Sum<TaxHistory.reportFiledAmt>>>>
																.SelectMultiBound(this, new object[] { currentTaxYear }, currentTaxYear.OrganizationID).AsEnumerable()
																.Cast<PXResult<TaxPeriodEx, TaxReportLine, TaxHistory>>();
						// The two extra arguments passed to the BQL Select are required to avoid platfrom bug. 
						// Query cache doesn't take into consideration PXReadBranchRestrictedScope. See AC-57858 for more details
						#pragma warning restore PX1015

						createdTaxPeriods = new List<TaxPeriodEx>();

						foreach (PXResult<TaxPeriodEx, TaxReportLine, TaxHistory> periodWithData in periodsWithData)
						{
							TaxHistory taxHistory = periodWithData;
							TaxPeriodEx taxPeriod = periodWithData;

							taxPeriod.NetTaxAmt = taxHistory.ReportFiledAmt;

							if (TaxPeriodExSelectView.Cache.Locate(taxPeriod) != null)
							{
								TaxPeriodExSelectView.Cache.Remove(taxPeriod);
							}

							TaxPeriodExSelectView.Cache.SetStatus(taxPeriod, PXEntryStatus.Held);
						}
					}
				}
				else
				{
					TaxCalendar.CreationParams creationParams = TX.TaxCalendar.CreationParams.FromTaxYear(currentTaxYear);

					TaxYearWithPeriods<TaxYearEx, TaxPeriodEx> taxYearWithPeriods =
						TaxCalendarEx.CreateWithCorrespondingTaxPeriodType(creationParams);

					createdTaxPeriods = taxYearWithPeriods.TaxPeriods;
				}
			}
			else
			{
			    Vendor vendor = VendorMaint.GetByID(this, taxYearFilter.VendorID);

			    TaxYearWithPeriods<TaxYearEx, TaxPeriodEx> taxYearWithPeriods =
			        TaxCalendarEx.Create(
			            taxYearFilter.OrganizationID,
			            VendorMaint.GetByID(this, taxYearFilter.VendorID),
			            Convert.ToInt32(taxYearFilter.Year),
			            vendor.TaxPeriodType == VendorTaxPeriodType.FiscalPeriod ? taxYearFilter.StartDate : null);

				taxYearWithPeriods.TaxYear = (TaxYearEx) TaxYearExSelectView.Cache.Insert(taxYearWithPeriods.TaxYear);
				TaxYearExSelectView.Cache.SetStatus(taxYearWithPeriods.TaxYear, PXEntryStatus.Held);

				currentTaxYear = taxYearWithPeriods.TaxYear;
				createdTaxPeriods = taxYearWithPeriods.TaxPeriods;
			}

			if (createdTaxPeriods != null)
			{
				foreach (TaxPeriodEx taxPeriod in createdTaxPeriods)
				{
					TaxPeriodEx taxPeriodEx = (TaxPeriodEx) TaxPeriodExSelectView.Cache.Insert(taxPeriod);
					TaxPeriodExSelectView.Cache.SetStatus(taxPeriodEx, PXEntryStatus.Held);

					TaxPeriodExSelectView.Cache.IsDirty = false;
					TaxYearExSelectView.Cache.IsDirty = false;
				}
			}

			return currentTaxYear;
		}

		protected virtual void RegeneratePeriodsAndPutIntoCache(TaxYearEx currentTaxYear)
		{
			TaxCalendar.CreationParams creationParams = TX.TaxCalendar.CreationParams.FromTaxYear(currentTaxYear);

			TaxYearWithPeriods<TaxYearEx, TaxPeriodEx> newTaxYearWithPeriods =
				TaxCalendarEx.CreateWithCorrespondingTaxPeriodType(creationParams);

			newTaxYearWithPeriods.TaxYear.Existing = currentTaxYear.Existing;
			newTaxYearWithPeriods.TaxYear.TaxPeriodsInDBExist = currentTaxYear.TaxPeriodsInDBExist;

			newTaxYearWithPeriods.TaxYear.Year = currentTaxYear.Year;
			currentTaxYear = TaxYearExSelectView.Update(newTaxYearWithPeriods.TaxYear);

			Dictionary<string, TaxPeriodEx> existingOldTaxPeriods =
												PXSelect<TaxPeriodEx,
														Where<TaxPeriodEx.organizationID, Equal<Required<TaxPeriodEx.organizationID>>,
																And<TaxPeriodEx.vendorID, Equal<Required<TaxPeriodEx.vendorID>>,
																And<TaxPeriodEx.taxYear, Equal<Required<TaxPeriodEx.taxYear>>>>>>
														.Select(this,
																currentTaxYear.OrganizationID,
																currentTaxYear.VendorID,
																currentTaxYear.Year)
														.RowCast<TaxPeriodEx>()
														.ToDictionary(period => period.TaxPeriodID, period => period);

			if (existingOldTaxPeriods.Any())
			{
				currentTaxYear.TaxPeriodsInDBExist = true;

				foreach (TaxPeriodEx newTaxPeriod in newTaxYearWithPeriods.TaxPeriods)
				{
					if (existingOldTaxPeriods.ContainsKey(newTaxPeriod.TaxPeriodID))
					{
						TaxPeriodExSelectView.Update(newTaxPeriod);
					}
					else
					{
						TaxPeriodExSelectView.Insert(newTaxPeriod);
					}
				}

				//delete existing exessive

				HashSet<string> newPeriodSet =
					new HashSet<string>(newTaxYearWithPeriods.TaxPeriods.Select(period => period.TaxPeriodID));

				IEnumerable<TaxPeriodEx> existingTaxPeriodsToDelete =
					existingOldTaxPeriods.Values.Where(period => !newPeriodSet.Contains(period.TaxPeriodID));

				foreach (TaxPeriodEx taxPeriod in existingTaxPeriodsToDelete)
				{
					TaxPeriodExSelectView.Delete(taxPeriod);
				}
			}
			else
			{
				currentTaxYear.TaxPeriodsInDBExist = false;

				TaxPeriodExSelectView.Cache.Clear();

				foreach (TaxPeriodEx taxPeriod in newTaxYearWithPeriods.TaxPeriods)
				{
					TaxPeriodEx insertedTaxPeriod = TaxPeriodExSelectView.Insert(taxPeriod);
					TaxPeriodExSelectView.Cache.SetStatus(insertedTaxPeriod, PXEntryStatus.Held);
				}
			}
		}

		protected virtual void PeriodButtonsFieldSelectingHandler(PXFieldSelectingEventArgs e)
		{
			e.ReturnState = PXButtonState.CreateDefaultState<TaxYearFilter>(e.ReturnState);

			bool enabled = false;

			TaxYearFilter taxYearFilter = TaxYearFilterSelectView.Current;

			if (taxYearFilter?.ShortTaxYear == true)
			{
				TaxPeriodEx notOpenPeriodInNextYear = PXSelect<TaxPeriodEx,
																Where<TaxPeriodEx.organizationID, Equal<Required<TaxPeriodEx.organizationID>>,
																		And<TaxPeriodEx.vendorID, Equal<Required<TaxPeriodEx.vendorID>>,
																		And<TaxPeriodEx.taxYear, Greater<Required<TaxPeriodEx.taxYear>>,
																		And<TaxPeriodEx.status, NotEqual<TaxPeriodStatus.open>>>>>>
																.SelectWindowed(this, 0, 1,
																				taxYearFilter.OrganizationID,
																				taxYearFilter.VendorID,
																				taxYearFilter.Year);

				enabled = notOpenPeriodInNextYear == null;
			}

			((PXButtonState)e.ReturnState).Enabled = enabled;
		}

		protected virtual IEnumerable<TaxPeriodEx> GetActualStoredTaxPeriods()
		{
			return TaxPeriodExSelectView.Cache.Cached.Cast<TaxPeriodEx>()
														.Where(period =>
														{
															PXEntryStatus status = TaxPeriodExSelectView.Cache.GetStatus(period);
															return status == PXEntryStatus.Held
																   || status == PXEntryStatus.Inserted
																   || status == PXEntryStatus.Updated;
														})
														.OrderBy(period => period.TaxPeriodID);
		}

		public static bool PrepearedTaxPeriodForVendorExists(PXGraph graph, int? vendorID)
		{
			var prepearedTaxPeriod = (TaxPeriod)PXSelect<TaxPeriod,
												Where<TaxPeriod.vendorID, Equal<Required<TaxPeriod.vendorID>>,
														And<TaxPeriod.status, Equal<TaxPeriodStatus.prepared>>>>
												.Select(graph, vendorID);

			return prepearedTaxPeriod != null;
		}

		#endregion
	}
}
