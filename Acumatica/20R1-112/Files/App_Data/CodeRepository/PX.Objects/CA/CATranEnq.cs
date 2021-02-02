using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.TX;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CA
{
	[Serializable]
	[TableAndChartDashboardType]
	public class CATranEnq : PXGraph<CATranEnq>
	{
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		#region Internal Type definition
		//Alias for CATran
		[Serializable]
		public partial class CATranExt : CATran
		{
			#region DepositType
			public abstract class depositType : PX.Data.BQL.BqlString.Field<depositType> { }

			[PXString(3, IsFixed = true)]
			public virtual string DepositType
			{
				get;
				set;
			}
			#endregion
			#region DepositNbr
			public abstract class depositNbr : PX.Data.BQL.BqlString.Field<depositNbr> { }

			[PXString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "CA Deposit Nbr.", Enabled = false)]
			public virtual string DepositNbr
			{
				get;
				set;
			}
			#endregion
			#region CuryDebitAmt
			protected decimal? _CuryDebitAmt;
			[PXDecimal]
			[PXUIField(DisplayName = "Receipt")]
			public override decimal? CuryDebitAmt
			{
				[PXDependsOnFields(typeof(drCr), typeof(curyTranAmt))]
				get
				{
					if (_CuryDebitAmt != null)
					{
						return _CuryDebitAmt;
					}
					else
					{
						return base.CuryDebitAmt;
					}
				}
				set
				{
					_CuryDebitAmt = value;
				}
			}
			#endregion
			#region CuryCreditAmt
			protected decimal? _CuryCreditAmt;
			[PXDecimal]
			[PXUIField(DisplayName = "Disbursement")]
			public override decimal? CuryCreditAmt
			{
				[PXDependsOnFields(typeof(drCr), typeof(curyTranAmt))]
				get
				{
					if (_CuryCreditAmt != null)
					{
						return _CuryCreditAmt;
					}
					else
					{
						return base.CuryCreditAmt;
					}
				}
				set
				{
					_CuryCreditAmt = value;
				}
			}
			#endregion
			#region tstamp
			public new class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
			{
			}
			[PXDBTimestamp(RecordComesFirst = true)]
			public override byte[] tstamp
			{
				get;
				set;
			}
			#endregion
		}
		#endregion

		#region Buttons
		public PXSave<CAEnqFilter> Save;
		#region Cancel

		public PXAction<CAEnqFilter> cancel;
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			CATranListRecords.Cache.Clear();
			CATranListRecords.Cache.ClearQueryCacheObsolete();
			Filter.Cache.Clear();
			Caches[typeof(CADailySummary)].Clear();
			TimeStamp = null;

			PXLongOperation.ClearStatus(this.UID);
			return adapter.Get();
		}
		#endregion


		#region Button Release
		public PXAction<CAEnqFilter> Release;
		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			CASetup setup = casetup.Current;
			CAEnqFilter filter = Filter.Current;

			List<CATran> tranList = new List<CATran>();

			foreach (CATran transToRelease in PXSelect<CATran, Where2<Where<Required<CAEnqFilter.includeUnreleased>, Equal<boolTrue>,
														   Or<CATran.released, Equal<boolTrue>>>,
													And<CATran.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
													And<CATran.tranDate, Between<Required<CAEnqFilter.startDate>, Required<CAEnqFilter.endDate>>>>>,
								 OrderBy<Asc<CATran.tranDate, Asc<CATran.extRefNbr, Asc<CATran.tranID>>>>>.Select(this, filter.IncludeUnreleased, filter.AccountID, filter.StartDate, filter.EndDate))
			{
				if (transToRelease.Selected == true)
				{
					tranList.Add(transToRelease);
				}
			}
			Save.Press();
			if (tranList.Count == 0)
			{
				throw new PXException(Messages.NoDocumentSelected);
			}
			else
			{
				PXLongOperation.StartOperation(this, delegate() { CATrxRelease.GroupReleaseTransaction(tranList, setup.ReleaseAP == true, setup.ReleaseAR == true, true); });
			}
			return adapter.Get();
		}
		#endregion

		#region Button Clear
		public PXAction<CAEnqFilter> Clearence;
		[PXUIField(DisplayName = "Clear", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable clearence(PXAdapter adapter)
		{
			CAEnqFilter filter = Filter.Current;
			CATranExt newrow = null;
			foreach (CATranExt transToClear in PXSelect<CATranExt, Where2<Where<Required<CAEnqFilter.includeUnreleased>, Equal<boolTrue>,
														   Or<CATranExt.released, Equal<boolTrue>>>,
													And<CATranExt.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
													And<CATranExt.tranDate, Between<Required<CAEnqFilter.startDate>, Required<CAEnqFilter.endDate>>>>>,
								 OrderBy<Asc<CATranExt.tranDate, Asc<CATranExt.extRefNbr, Asc<CATranExt.tranID>>>>>.Select(this, filter.IncludeUnreleased, filter.AccountID, filter.StartDate, filter.EndDate))
			{
				if (transToClear.Reconciled != true)
				{
					newrow = PXCache<CATranExt>.CreateCopy(transToClear);
					newrow.Cleared = true;
					CATranListRecords.Cache.Update(newrow);
				}
			}
			Save.Press();
			return adapter.Get();
		}
		#endregion

		#region viewDoc
		public PXAction<CAEnqFilter> viewDoc;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewDoc(PXAdapter adapter)
		{
			CATran.Redirect(Filter.Cache, CATranListRecords.Current);
			return Filter.Select();
		}

		public PXAction<CAEnqFilter> viewRecon;
		[PXUIField(DisplayName = "View Reconciliation Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewRecon(PXAdapter adapter)
		{
			if (CATranListRecords.Current.ReconNbr != null)
			{
				CAReconEntry graph = PXGraph.CreateInstance<CAReconEntry>();
				CARecon recon = PXSelect<CARecon, Where<CARecon.reconNbr, Equal<Required<CATran.reconNbr>>>>.Select(graph, CATranListRecords.Current.ReconNbr);
				if (recon != null)
				{
					graph.CAReconRecords.Current = recon;
					throw new PXRedirectRequiredException(graph, "Reconciliation");
				}
			}
			return Filter.Select();
		}

		public PXAction<CAEnqFilter> doubleClick;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable DoubleClick(PXAdapter adapter)
		{

			CAEnqFilter filterCur = Filter.Current;

			if (filterCur.ShowSummary == true)
			{
				CATran tran = (CATran)(CATranListRecords.Current);
				filterCur.LastStartDate = filterCur.StartDate;
				filterCur.LastEndDate = filterCur.EndDate;
				filterCur.StartDate = tran.TranDate;
				filterCur.EndDate = tran.TranDate;
				filterCur.ShowSummary = false;

				CATranListRecords.Cache.Clear();
				Caches[typeof(CADailySummary)].Clear();
			}

			return adapter.Get();
		}

		public PXAction<CAEnqFilter> viewBatch;
		[PXUIField(DisplayName = "View Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			Batch batch = PXSelect<Batch,
							Where<Batch.module, Equal<Required<Batch.module>>, And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>
							.Select(this, CATranListRecords.Current.OrigModule, CATranListRecords.Current.BatchNbr);

			if (batch != null)
			{
				var batchGraph = PXGraph.CreateInstance<JournalEntry>();
				batchGraph.BatchModule.Current = batch;
				PXRedirectHelper.TryRedirect(batchGraph, PXRedirectHelper.WindowMode.NewWindow);
			}

			return adapter.Get();
		}
		#endregion

		#region addDet
		public PXAction<CAEnqFilter> AddDet;
		[PXUIField(DisplayName = "Create Transaction", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable addDet(PXAdapter adapter)
		{
			AddFilter.AskExt(true);
			if (AddFilter.View.Answer == WebDialogResult.OK)
			{
				using (new PXTimeStampScope(this.TimeStamp))
				{
					CATran catran = AddTrxFilter.VerifyAndCreateTransaction(this, AddFilter, currencyinfo);
					if (catran != null)
					{
						CATranExt copy = new CATranExt();
						PXCache<CATran>.RestoreCopy(copy, catran);
						CATranListRecords.Update(copy);
						Save.Press();
					}
				}
				CATranListRecords.Cache.Clear();
				Caches[typeof(CADailySummary)].Clear();
				Filter.Current.BegBal = null;
			}
			AddFilter.Cache.Clear();
			return adapter.Get();
		}
		#endregion
		#endregion

		#region Selects
		public PXFilter<CAEnqFilter> Filter;
		public PXFilter<AddTrxFilter> AddFilter;
		public PXSelectReadonly<CADailySummary,
			Where<CADailySummary.cashAccountID, Equal<Current<CAEnqFilter.accountID>>,
			And<CADailySummary.tranDate, Between<Current<CAEnqFilter.startDate>, Current<CAEnqFilter.endDate>>>>>
			CATranListSummarized;

		[PXFilterable]
		public PXSelectJoinOrderBy<CATranExt, LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<CATranExt.referenceID>>>, OrderBy<Asc<CATranExt.tranDate>>> CATranListRecords;
		public PXSelect<CurrencyInfo> currencyinfo;
		public ToggleCurrency<CAEnqFilter> CurrencyView;
		public PXSelect<CAAdj, Where<CAAdj.adjTranType, Equal<CATranType.cAAdjustment>, And<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>>>> caadj_adjRefNbr;
		public PXSelect<CASplit, Where<CASplit.adjTranType, Equal<CATranType.cAAdjustment>, And<CASplit.adjRefNbr, Equal<Required<CASplit.adjRefNbr>>>>> casplit_adjRefNbr;
		public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CAEnqFilter.accountID>>>> cashaccount;

		public PXSetup<CASetup> casetup;
		public PXSetup<APSetup> apsetup;
		public PXSetup<ARSetup> arsetup;
		#endregion

		#region Functions

		public override void Persist()
		{
			List<CATran> list = new List<CATran>((IEnumerable<CATran>)this.Caches[typeof(CATran)].Updated);

			using (var ts = new PXTransactionScope())
			{
				base.Persist();
				foreach (CATran tran in list)
				{
					if (tran.Reconciled != true)
					{
						CAReconEntry.UpdateClearedOnSourceDoc(tran);
					}
				}
				ts.Complete(this);
			}
			Caches[typeof(CM.CurrencyInfo)].SetStatus(Caches[typeof(CM.CurrencyInfo)].Current, PXEntryStatus.Inserted);
		}

		public CATranEnq()
		{
			PXUIFieldAttribute.SetVisible<CATranExt.reconNbr>(CATranListRecords.Cache, null, false);
			CASetup setup = casetup.Current;
		}

		public override void Clear()
		{
			AddFilter.Current.TranDate = null;
			AddFilter.Current.FinPeriodID = null;
			AddFilter.Current.CuryInfoID = null;
			base.Clear();
		}

		public virtual void GetRange(DateTime date, string Range, int? cashAccountID, out DateTime? RangeStart, out DateTime? RangeEnd)
		{
			switch (Range)
			{
				case "W":
					RangeStart = date.AddDays(-1 * (PXDateTime.DayOfWeekOrdinal(date.DayOfWeek) - 1));
					RangeEnd = date.AddDays(7 - PXDateTime.DayOfWeekOrdinal(date.DayOfWeek));
					return;
				case "M":
					RangeStart = new DateTime(date.Year, date.Month, 1);
					RangeEnd = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
					return;
				case "P":
					CashAccount cashAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, cashAccountID);
					int? organizationID = PXAccess.GetParentOrganizationID(cashAccount?.BranchID);
					var period = FinPeriodRepository.FindFinPeriodByDate(date, organizationID);
					RangeStart = period?.StartDate;
					RangeEnd = period?.EndDate;
					return;
				case "D":
				default:
					RangeStart = date;
					RangeEnd = date;
					return;
			}
		}

		#endregion

		#region Execute Select

		protected virtual IEnumerable filter()
		{
			PXCache cache = Caches[typeof(CAEnqFilter)];
			if (cache != null)
			{
				CAEnqFilter filter = cache.Current as CAEnqFilter;
				if (filter != null)
				{
					if (filter.StartDate == null || filter.EndDate == null)
					{
						DateTime? startDate;
						DateTime? endDate;
					
						GetRange((DateTime)this.Accessinfo.BusinessDate, casetup.Current.DateRangeDefault, filter.AccountID, out startDate, out endDate);
						filter.StartDate = startDate;
						filter.EndDate = endDate;
					}
					if (filter.AccountID != null && filter.StartDate != null)
					{
						CADailySummary begBal = PXSelectGroupBy<CADailySummary,
													Where<CADailySummary.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
													And<CADailySummary.tranDate, Less<Required<CAEnqFilter.startDate>>>>,
													Aggregate<Sum<CADailySummary.amtReleasedClearedCr,
														Sum<CADailySummary.amtReleasedClearedDr,
														Sum<CADailySummary.amtReleasedUnclearedCr,
														Sum<CADailySummary.amtReleasedUnclearedDr,
														Sum<CADailySummary.amtUnreleasedClearedCr,
														Sum<CADailySummary.amtUnreleasedClearedDr,
														Sum<CADailySummary.amtUnreleasedUnclearedCr,
														Sum<CADailySummary.amtUnreleasedUnclearedDr,
														GroupBy<CADailySummary.cashAccountID>>>>>>>>>>>
													.Select(this, filter.AccountID, filter.StartDate);

						if ((begBal == null) || (begBal.CashAccountID == null))
						{
							filter.BegBal = (decimal)0.0;
							filter.BegClearedBal = (decimal)0.0;
						}
						else
						{
							filter.BegBal = begBal.AmtReleasedClearedDr -
															begBal.AmtReleasedClearedCr +
															begBal.AmtReleasedUnclearedDr -
															begBal.AmtReleasedUnclearedCr;

							filter.BegClearedBal = begBal.AmtReleasedClearedDr -
													 begBal.AmtReleasedClearedCr;

							if (filter.IncludeUnreleased == true)
							{
								filter.BegBal += begBal.AmtUnreleasedClearedDr -
												 begBal.AmtUnreleasedClearedCr +
												 begBal.AmtUnreleasedUnclearedDr -
												 begBal.AmtUnreleasedUnclearedCr;

								filter.BegClearedBal += begBal.AmtUnreleasedClearedDr -
																		begBal.AmtUnreleasedClearedCr;
							}
						}
						filter.DebitTotal = 0m;
						filter.CreditTotal = 0m;
						filter.DebitClearedTotal = 0m;
						filter.CreditClearedTotal = 0m;
						int startRow = 0;
						int totalRows = 0;
						foreach (PXResult<CATranExt> res in CATranListRecords.View.Select(PXView.Currents, PXView.Parameters, new object[0], new string[0], new bool[0], CATranListRecords.View.GetExternalFilters(), ref startRow, 0, ref totalRows))
						{
							CATranExt tran = (CATranExt)res;
							filter.DebitTotal += tran.CuryDebitAmt;
							filter.CreditTotal += tran.CuryCreditAmt;
							filter.DebitClearedTotal += tran.CuryClearedDebitAmt;
							filter.CreditClearedTotal += tran.CuryClearedCreditAmt;
						}
						filter.EndBal = filter.BegBal + filter.DebitTotal - filter.CreditTotal;
						filter.EndClearedBal = filter.BegClearedBal + filter.DebitClearedTotal - filter.CreditClearedTotal;
					}
				}
			}
			yield return cache.Current;
			cache.IsDirty = false;
		}

		public virtual IEnumerable cATranListRecords()
		{
			CAEnqFilter filter = Filter.Current;

			List<PXResult<CATranExt, BAccountR>> result = new List<PXResult<CATranExt, BAccountR>>();
			if (filter != null && filter.ShowSummary == true)
			{
				long? id = 0;
				int startRow = 0;
				int totalRows = 0;
				foreach (CADailySummary daily in CATranListSummarized.View.Select(null, null, new object[PXView.SortColumns.Length], PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, 0, ref totalRows))
				{
					CATranExt tran = new CATranExt();
					id++;
					tran.TranID = id;
					tran.CashAccountID = daily.CashAccountID;
					tran.TranDate = daily.TranDate;

					tran.CuryDebitAmt = daily.AmtReleasedClearedDr + daily.AmtReleasedUnclearedDr;
					tran.CuryCreditAmt = daily.AmtReleasedClearedCr + daily.AmtReleasedUnclearedCr;

					if (Filter.Current.IncludeUnreleased == true)
					{
						tran.CuryDebitAmt += daily.AmtUnreleasedClearedDr + daily.AmtUnreleasedUnclearedDr;
						tran.CuryCreditAmt += daily.AmtUnreleasedClearedCr + daily.AmtUnreleasedUnclearedCr;
					}
					tran.DayDesc = TM.EPCalendarFilter.CalendarTypeAttribute.GetDayName(((DateTime)tran.TranDate).DayOfWeek);
					result.Add(new PXResult<CATranExt, BAccountR>(tran, new BAccountR()));
				}
			}
			else
			{
				Dictionary<long, CAMessage> listMessages = PXLongOperation.GetCustomInfo(this.UID) as Dictionary<long, CAMessage>;
				PXSelectBase<CATranExt> cmd = new PXSelectJoin<CATranExt, LeftJoin<ARPayment, On<ARPayment.docType, Equal<CATranExt.origTranType>,
											And<ARPayment.refNbr, Equal<CATran.origRefNbr>>>,
												LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<CATranExt.referenceID>>>>,
										Where2<Where<Current<CAEnqFilter.includeUnreleased>, Equal<boolTrue>,
																	Or<CATran.released, Equal<boolTrue>>>,
															And<CATranExt.cashAccountID, Equal<Current<CAEnqFilter.accountID>>,
															And<CATranExt.tranDate, Between<Current<CAEnqFilter.startDate>, Current<CAEnqFilter.endDate>>>>>,
											OrderBy<Asc<CATranExt.tranDate, Asc<CATranExt.extRefNbr, Asc<CATranExt.tranID>>>>>(this);
				int startRow = 0;
				int totalRows = 0;
				foreach (PXResult<CATranExt, ARPayment, BAccountR> iRes in cmd.View.Select(null, null, new object[PXView.SortColumns.Length], PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, 0, ref totalRows))
				{

					CATranExt tran = iRes;
					ARPayment payment = iRes;
					BAccountR baccount = iRes;
					tran.DayDesc = TM.EPCalendarFilter.CalendarTypeAttribute.GetDayName(((DateTime)tran.TranDate).DayOfWeek);
					tran.DepositNbr = payment.DepositNbr;
					tran.DepositType = payment.DepositType;
					if (listMessages != null)
					{
						CAMessage message;
						if (listMessages.TryGetValue(tran.TranID.Value, out message))
						{
							if (message != null && message.Key == tran.TranID)
							{
								CATranListRecords.Cache.RaiseExceptionHandling<CATran.origRefNbr>(tran, tran.OrigRefNbr, new PXSetPropertyException(message.Message, message.ErrorLevel));
							}
						}
					}
					result.Add(new PXResult<CATranExt, BAccountR>(tran, baccount));
				}
			}

			decimal curBalance = 0;
			if (filter != null && filter.BegBal != null)
				curBalance = (decimal)filter.BegBal;
			PXView.ReverseOrder = false;
			foreach (PXResult<CATranExt, BAccountR> it in PXView.Sort(result))
			{
				CATran tran = it;
				tran.BegBal = curBalance;
				tran.EndBal = tran.BegBal + tran.CuryDebitAmt - tran.CuryCreditAmt;
				curBalance = (decimal)tran.EndBal;
				CATranListRecords.Cache.Hold(tran);
			}
			return result;
		}

		#endregion

		#region CurrencyInfo Events
		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
			{
				CashAccount cashAcc = cashaccount.Current;
				if (cashAcc != null && !string.IsNullOrEmpty(cashAcc.CuryRateTypeID))
				{
					e.NewValue = cashAcc.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}
		#endregion

		#region CAEnqFilter Events

		protected virtual void CAEnqFilter_StartDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			DateTime? startDate;
			DateTime? endDate;
			CAEnqFilter filter = (CAEnqFilter)e.Row;
			GetRange((DateTime)Accessinfo.BusinessDate, casetup.Current.DateRangeDefault, filter?.AccountID, out startDate, out endDate);
			e.NewValue = startDate;
		}
		protected virtual void CAEnqFilter_EndDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			DateTime? startDate;
			DateTime? endDate;
			CAEnqFilter filter = (CAEnqFilter)e.Row;
			GetRange((DateTime)this.Accessinfo.BusinessDate, casetup.Current.DateRangeDefault, filter?.AccountID, out startDate, out endDate);
			e.NewValue = endDate;
		}

		protected virtual void CAEnqFilter_ShowSummary_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CAEnqFilter filter = e.Row as CAEnqFilter;

			if (filter.ShowSummary == true)
			{
				if (filter.LastEndDate != null)
				{
					filter.StartDate = filter.LastStartDate;
					filter.EndDate = filter.LastEndDate;
					filter.LastStartDate = null;
					filter.LastEndDate = null;
				}
			}
		}

		protected virtual void CAEnqFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CAEnqFilter filter = (CAEnqFilter)e.Row;
			if (filter == null) return;

			bool ShowSummaryNotChecked = filter.ShowSummary != true;

			bool CashAccountNeedReconcilation = (cashaccount.Current != null) && (bool)cashaccount.Current.Reconcile;
			if (cashaccount.Current != null && AddFilter.Current != null && AddFilter.Current.CashAccountID != filter.AccountID)
			{
				AddFilter.Cache.SetValueExt<AddTrxFilter.cashAccountID>(AddFilter.Current, cashaccount.Current.CashAccountCD);
			}
			AddDet.SetEnabled(filter.AccountID != null);
			PXCache tranCache = CATranListRecords.Cache;
			tranCache.AllowInsert = false;
			tranCache.AllowUpdate = (ShowSummaryNotChecked);
			tranCache.AllowDelete = (ShowSummaryNotChecked);

			PXUIFieldAttribute.SetVisible<CATran.selected>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.hold>(tranCache, null, false);
			PXUIFieldAttribute.SetVisible<CATran.status>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.origModule>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.origRefNbr>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.origTranType>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.extRefNbr>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.batchNbr>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.finPeriodID>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.tranDesc>(tranCache, null, ShowSummaryNotChecked);

			PXUIFieldAttribute.SetVisible<CATran.referenceName>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.reconciled>(tranCache, null, ShowSummaryNotChecked && CashAccountNeedReconcilation);
			PXUIFieldAttribute.SetVisible<CATran.clearDate>(tranCache, null, ShowSummaryNotChecked && CashAccountNeedReconcilation);
			PXUIFieldAttribute.SetVisible<CATran.cleared>(tranCache, null, ShowSummaryNotChecked && CashAccountNeedReconcilation);
			PXUIFieldAttribute.SetVisible<CATran.dayDesc>(tranCache, null, !ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATranExt.dayDesc>(tranCache, null, !ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATranExt.depositNbr>(tranCache, null, ShowSummaryNotChecked);
			PXUIFieldAttribute.SetVisible<CATran.referenceID>(tranCache, null, ShowSummaryNotChecked);
			PXCache bAcctCache = this.Caches[typeof(BAccountR)];
			PXUIFieldAttribute.SetVisible<BAccountR.acctName>(bAcctCache, null, ShowSummaryNotChecked);

			Clearence.SetEnabled(CashAccountNeedReconcilation);
			AddFilter.Cache.RaiseRowSelected(AddFilter.Current);

			bool operationExists = PXLongOperation.Exists(UID);
			Save.SetEnabled(!operationExists);
			Release.SetEnabled(!operationExists);
			Clearence.SetEnabled(!operationExists);

		}

		protected virtual void CAEnqFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CAEnqFilter filter = e.Row as CAEnqFilter;
			AddTrxFilter addFilter = AddFilter.Current;
			CurrencyInfo curInfo = currencyinfo.Current;

			CATranListRecords.Cache.Clear();
			Caches[typeof(CADailySummary)].Clear();

			if (!e.ExternalCall)
			{
				DateTime? startDate;
				DateTime? endDate;
				GetRange((DateTime)Accessinfo.BusinessDate, casetup.Current.DateRangeDefault, filter.AccountID, out startDate, out endDate);
				if (filter != null)
				{
					filter.StartDate = startDate;
					filter.EndDate = endDate;
				}
			}
		}


		protected virtual void CAEnqFilter_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CAEnqFilter filter = e.Row as CAEnqFilter;
			if (filter == null || filter.AccountID == null)
				return;
			cashaccount.Current = (CashAccount)PXSelectorAttribute.Select<CAEnqFilter.accountID>(sender, filter);
			sender.SetDefaultExt<CAEnqFilter.curyID>(filter);
			AddFilter.Cache.SetValueExt<AddTrxFilter.cashAccountID>(AddFilter.Current, filter.AccountID);

			if (filter != null && filter.ShowSummary != true)
			{
				bool needReselect = false;
				foreach (CATranExt tran in PXSelect<CATranExt, Where2<Where<Required<CAEnqFilter.includeUnreleased>, Equal<boolTrue>,
																   Or<CATran.released, Equal<boolTrue>>>,
															And<CATranExt.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
															And<CATranExt.tranDate, Between<Required<CAEnqFilter.startDate>, Required<CAEnqFilter.endDate>>>>>,
										 OrderBy<Asc<CATranExt.tranDate, Asc<CATranExt.extRefNbr, Asc<CATranExt.tranID>>>>>.Select(this, filter.IncludeUnreleased, filter.AccountID, filter.StartDate, filter.EndDate))
				{
					if (tran.Selected == true)
					{
						tran.Selected = false;
						CATranListRecords.Update(tran);
						needReselect = true;
					}
				}
				if (needReselect == true)
					Save.Press();
			}
		}

		#endregion

		#region CATran Events
		protected virtual void CATranExt_ClearDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if ((bool)((CATran)e.Row).Cleared && (e.NewValue == null))
			{
				throw new PXSetPropertyException(Messages.ClearedDateNotAvailable);
			}
		}

		protected virtual void CATranExt_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CATran catran = (CATran)e.Row;

			if (catran == null) return;

			bool cashAccountNeedReconcilation = (cashaccount.Current != null) && (bool)cashaccount.Current.Reconcile;
			bool isClearEnable = catran.Reconciled != true;

			PXUIFieldAttribute.SetEnabled(cache, catran, false);
			PXUIFieldAttribute.SetEnabled<CATran.selected>(cache, catran, true);
			PXUIFieldAttribute.SetEnabled<CATran.reconciled>(cache, catran, false);
			PXUIFieldAttribute.SetEnabled<CATran.cleared>(cache, catran, isClearEnable && cashAccountNeedReconcilation);
			PXUIFieldAttribute.SetEnabled<CATran.clearDate>(cache, catran, isClearEnable && cashAccountNeedReconcilation && (catran.Cleared == true));

		}

		protected virtual void CATranExt_Cleared_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CATran caTran = (CATran)e.Row;
			if (caTran.Cleared == true)
			{
				caTran.ClearDate = this.Accessinfo.BusinessDate;

				if (Filter.Current.IncludeUnreleased == true || caTran.Released == true)
				{
					Filter.Current.DebitClearedTotal += caTran.CuryDebitAmt;
					Filter.Current.CreditClearedTotal += caTran.CuryCreditAmt;
					Filter.Current.EndClearedBal += (caTran.CuryDebitAmt - caTran.CuryCreditAmt);
				}
			}
			else
			{
				caTran.ClearDate = null;

				if (Filter.Current.IncludeUnreleased == true || caTran.Released == true)
				{
					Filter.Current.DebitClearedTotal -= caTran.CuryDebitAmt;
					Filter.Current.CreditClearedTotal -= caTran.CuryCreditAmt;
					Filter.Current.EndClearedBal -= (caTran.CuryDebitAmt - caTran.CuryCreditAmt);
				}
			}
		}

		protected virtual void CATranExt_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CATran tran = (CATran)e.Row;

			if (tran.Released == true ||
				tran.OrigModule != BatchModule.CA ||
				tran.OrigTranType != CATranType.CAAdjustment)
			{
				throw new PXException(ErrorMessages.CantDeleteRecord);
			}

			CAAdj adj = caadj_adjRefNbr.Select(tran.OrigRefNbr);
			if (adj != null)
			{
				caadj_adjRefNbr.Delete(adj);
			}
			foreach (CASplit split in casplit_adjRefNbr.Select(tran.OrigRefNbr))
			{
				casplit_adjRefNbr.Delete(split);
			}
		}

		#endregion
	}

	[Serializable]
	public partial class CAEnqFilter : IBqlTable
	{
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[PXDefault]
		[CashAccount()]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<CAEnqFilter.accountID>>>>))]
		[PXSelector(typeof(CM.Currency.curyID))]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
		public virtual DateTime? StartDate
		{
			get;
			set;
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		[PXDBDate]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
		public virtual DateTime? EndDate
		{
			get;
			set;
		}
		#endregion

		#region LastStartDate
		public abstract class lastStartDate : PX.Data.BQL.BqlDateTime.Field<lastStartDate> { }

		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Last Start Date", Visible = false)]
		public virtual DateTime? LastStartDate
		{
			get;
			set;
		}
		#endregion
		#region LastEndDate
		public abstract class lastEndDate : PX.Data.BQL.BqlDateTime.Field<lastEndDate> { }

		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "End Date", Visible = false)]
		public virtual DateTime? LastEndDate
		{
			get;
			set;
		}
		#endregion

		#region ShowSummary
		public abstract class showSummary : PX.Data.BQL.BqlBool.Field<showSummary> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Summary")]
		public virtual bool? ShowSummary
		{
			get;
			set;
		}
		#endregion
		#region IncludeUnreleased
		public abstract class includeUnreleased : PX.Data.BQL.BqlBool.Field<includeUnreleased> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Include Unreleased")]
		public virtual bool? IncludeUnreleased
		{
			get;
			set;
		}
		#endregion
		#region BegBal
		public abstract class begBal : PX.Data.BQL.BqlDecimal.Field<begBal> { }

		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
												Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Balance", Enabled = false)]
		public virtual decimal? BegBal
		{
			get;
			set;
		}
		#endregion
		#region CreditTotal
		public abstract class creditTotal : PX.Data.BQL.BqlDecimal.Field<creditTotal> { }

		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
											Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Credit Total", Enabled = false)]
		public virtual decimal? CreditTotal
		{
			get;
			set;
		}
		#endregion
		#region DebitTotal
		public abstract class debitTotal : PX.Data.BQL.BqlDecimal.Field<debitTotal> { }

		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
										Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Debit Total", Enabled = false)]
		public virtual decimal? DebitTotal
		{
			get;
			set;
		}
		#endregion
		#region EndBal
		public abstract class endBal : PX.Data.BQL.BqlDecimal.Field<endBal> { }

		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
									Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance", Enabled = false)]
		public virtual Decimal? EndBal
		{
			get;
			set;
		}
		#endregion
		#region BegClearedBal
		public abstract class begClearedBal : PX.Data.BQL.BqlDecimal.Field<begClearedBal> { }

		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
									Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Balance", Enabled = false)]
		public virtual decimal? BegClearedBal
		{
			get;
			set;
		}
		#endregion
		#region CreditClearedTotal
		public abstract class creditClearedTotal : PX.Data.BQL.BqlDecimal.Field<creditClearedTotal> { }

		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
									Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Credit Total", Enabled = false)]
		public virtual decimal? CreditClearedTotal
		{
			get;
			set;
		}
		#endregion
		#region DebitClearedTotal
		public abstract class debitClearedTotal : PX.Data.BQL.BqlDecimal.Field<debitClearedTotal> { }

		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
									Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Acct. Debit Total", Enabled = false)]
		public virtual decimal? DebitClearedTotal
		{
			get;
			set;
		}
		#endregion
		#region EndClearedBal
		public abstract class endClearedBal : PX.Data.BQL.BqlDecimal.Field<endClearedBal> { }

		[PXDecimal(typeof(Search<CM.Currency.decimalPlaces,
									Where<Currency.curyID, Equal<Current<CAEnqFilter.curyID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance", Enabled = false)]
		public virtual decimal? EndClearedBal
		{
			get;
			set;
		}
		#endregion
	}
}
