using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.AR.Overrides.ARDocumentRelease;
using PX.Objects.Common;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.CM
{
	[TableAndChartDashboardType]
	public class RevalueARAccounts : RevalueAcountsBase<RevaluedARHistory>
	{
		public PXCancel<RevalueFilter> Cancel;
		public PXFilter<RevalueFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<RevaluedARHistory, RevalueFilter, Where<boolTrue, Equal<boolTrue>>, OrderBy<Asc<RevaluedARHistory.accountID, Asc<RevaluedARHistory.subID, Asc<RevaluedARHistory.customerID>>>>> ARAccountList;
		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSetup<ARSetup> arsetup;
		public PXSetup<CMSetup> cmsetup;
		public PXSetup<Company> company;

		public RevalueARAccounts()
		{
			var curARSetup = arsetup.Current;
			var curCMSetup = cmsetup.Current;
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true) 
				throw new Exception(Messages.MultiCurrencyNotActivated);
			ARAccountList.SetProcessCaption(Messages.Revalue);
			ARAccountList.SetProcessAllVisible(false);

			PXUIFieldAttribute.SetEnabled<RevaluedARHistory.finPtdRevalued>(ARAccountList.Cache, null, true);
		}

		protected virtual void RevalueFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RevalueFilter filter = (RevalueFilter)e.Row;
			if (filter != null)
			{
				ARAccountList.SetProcessDelegate(delegate (List<RevaluedARHistory> list)
				{
					var graph = CreateInstance<RevalueARAccounts>();
					graph.Revalue(filter, list);
				}
				);
			}
		}

		protected virtual void RevalueFilter_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (((RevalueFilter)e.Row).CuryEffDate != null)
			{
				((RevalueFilter)e.Row).CuryEffDate = ((DateTime)((RevalueFilter)e.Row).CuryEffDate).AddDays(-1);
			}
		}

		protected virtual void RevalueFilter_FinPeriodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RevalueFilter.curyEffDate>(e.Row);

			if (((RevalueFilter)e.Row).CuryEffDate != null)
			{
				((RevalueFilter)e.Row).CuryEffDate = ((DateTime)((RevalueFilter)e.Row).CuryEffDate).AddDays(-1);
			}
			ARAccountList.Cache.Clear();
		}

		protected virtual void RevalueFilter_CuryEffDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAccountList.Cache.Clear();
		}

		protected virtual void RevalueFilter_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAccountList.Cache.Clear();
		}

		protected virtual void RevalueFilter_TotalRevalued_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				decimal val = 0m;
				foreach (RevaluedARHistory res in ARAccountList.Cache.Updated)
				{
					if ((bool)res.Selected)
					{
						val += (decimal)res.FinPtdRevalued;
					}
				}

				if (val == 0)
				{
					TimeSpan timespan;
					Exception ex;
					if (PXLongOperation.GetStatus(UID, out timespan, out ex) == PXLongRunStatus.Completed)
					{
						Filter.Cache.RaiseExceptionHandling<RevalueFilter.totalRevalued>(e.Row, val,
							new PXSetPropertyException(Messages.NoRevaluationEntryWasMade, PXErrorLevel.Warning));
					}
				}
				else
				{
					e.ReturnValue = val;
					e.Cancel = true;
				}
			}
		}

		public virtual IEnumerable araccountlist()
		{
			foreach (PXResult<ARHistoryByPeriod, RevaluedARHistory, Customer, Branch, FinPeriod, ARHistoryLastRevaluation> res in 
									PXSelectJoin<ARHistoryByPeriod,
													InnerJoin<RevaluedARHistory, 
														On<RevaluedARHistory.customerID, Equal<ARHistoryByPeriod.customerID>, 
															And<RevaluedARHistory.branchID, Equal<ARHistoryByPeriod.branchID>, 
															And<RevaluedARHistory.accountID, Equal<ARHistoryByPeriod.accountID>, 
															And<RevaluedARHistory.subID, Equal<ARHistoryByPeriod.subID>, 
															And<RevaluedARHistory.curyID, Equal<ARHistoryByPeriod.curyID>, 
															And<RevaluedARHistory.finPeriodID, Equal<ARHistoryByPeriod.lastActivityPeriod>>>>>>>,
													InnerJoin<Customer,
														On<Customer.bAccountID, Equal<ARHistoryByPeriod.customerID>>,
													InnerJoin<Branch,
														On<Branch.branchID, Equal<ARHistoryByPeriod.branchID>>,
													InnerJoin<FinPeriod,
														On<ARHistoryByPeriod.finPeriodID, Equal<FinPeriod.finPeriodID>,
														And<Branch.organizationID, Equal<FinPeriod.organizationID>>>,
													LeftJoin<ARHistoryLastRevaluation,
														On<ARHistoryByPeriod.customerID, Equal<ARHistoryLastRevaluation.customerID>,
															And<ARHistoryByPeriod.branchID, Equal<ARHistoryLastRevaluation.branchID>,
															And<ARHistoryByPeriod.accountID, Equal<ARHistoryLastRevaluation.accountID>,
															And<ARHistoryByPeriod.subID, Equal<ARHistoryLastRevaluation.subID>,
															And<ARHistoryByPeriod.curyID, Equal<ARHistoryLastRevaluation.curyID>>>>>>>>>>>,
													Where<ARHistoryByPeriod.curyID, Equal<Current<RevalueFilter.curyID>>,
															And<FinPeriod.masterFinPeriodID, Equal<Current<RevalueFilter.finPeriodID>>,
															And<Where<RevaluedARHistory.curyFinYtdBalance, NotEqual<decimal0>, 
																		Or<RevaluedARHistory.finPtdRevalued, NotEqual<decimal0>>>>>>>
													.Select(this))
			{
				ARHistoryByPeriod histbyper = res;
				ARHistoryLastRevaluation lastRevaluationPeriod = res;
				RevaluedARHistory hist = PXCache<RevaluedARHistory>.CreateCopy(res);
				RevaluedARHistory existing;
                Customer cust = res;

				if ((existing = ARAccountList.Locate(hist)) != null)
				{
					yield return existing;
					continue;
				}
				else
				{
					ARAccountList.Cache.SetStatus(hist, PXEntryStatus.Held);
				}

				hist.CustomerClassID = cust.CustomerClassID;
				hist.CuryRateTypeID = cmsetup.Current.ARRateTypeReval ?? cust.CuryRateTypeID;

				if (string.IsNullOrEmpty(hist.CuryRateTypeID))
				{
					ARAccountList.Cache.RaiseExceptionHandling<RevaluedGLHistory.curyRateTypeID>(hist, null, new PXSetPropertyException(Messages.RateTypeNotFound));
				}
				else
				{
					CurrencyRate curyrate = PXSelect<CurrencyRate,
						Where<CurrencyRate.fromCuryID, Equal<Current<RevalueFilter.curyID>>,
						And<CurrencyRate.toCuryID, Equal<Current<Company.baseCuryID>>,
						And<CurrencyRate.curyRateType, Equal<Required<Customer.curyRateTypeID>>,
						And<CurrencyRate.curyEffDate, LessEqual<Current<RevalueFilter.curyEffDate>>>>>>,
						OrderBy<Desc<CurrencyRate.curyEffDate>>>.Select(this, hist.CuryRateTypeID);

					if (curyrate == null || curyrate.CuryMultDiv == null)
					{
						hist.CuryMultDiv = "M";
						hist.CuryRate = 1m;
						hist.RateReciprocal = 1m;
						hist.CuryEffDate = Filter.Current.CuryEffDate;
						ARAccountList.Cache.RaiseExceptionHandling<RevaluedARHistory.curyRate>(hist, 1m, new PXSetPropertyException(Messages.RateNotFound, PXErrorLevel.RowWarning));
					}
					else
					{
						hist.CuryRate = curyrate.CuryRate;
						hist.RateReciprocal = curyrate.RateReciprocal;
						hist.CuryEffDate = curyrate.CuryEffDate;
						hist.CuryMultDiv = curyrate.CuryMultDiv;
					}

					CurrencyInfo info = new CurrencyInfo();
					info.BaseCuryID = company.Current.BaseCuryID;
					info.CuryID = hist.CuryID;
					info.CuryMultDiv = hist.CuryMultDiv;
					info.CuryRate = hist.CuryRate;

					//hist.CuryFinYtdBalance -= hist.CuryFinYtdDeposits;
					//hist.FinYtdBalance -= hist.FinYtdDeposits;

					decimal baseval;
					PXCurrencyAttribute.CuryConvBase(currencyinfo.Cache, info, (decimal)hist.CuryFinYtdBalance, out baseval);
					hist.FinYtdRevalued = baseval;
					hist.FinPrevRevalued = string.Equals(histbyper.FinPeriodID, histbyper.LastActivityPeriod) ? hist.FinPtdRevalued : 0m;
					hist.FinPtdRevalued = hist.FinYtdRevalued - hist.FinPrevRevalued - hist.FinYtdBalance;
					hist.LastRevaluedFinPeriodID = lastRevaluationPeriod?.LastActivityPeriod;

				}
				yield return hist;
			}
		}

		public void Revalue(RevalueFilter filter, List<RevaluedARHistory> list)
		{
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			PostGraph pg = PXGraph.CreateInstance<PostGraph>();
			PXCache cache = je.Caches[typeof(CuryARHist)];
			PXCache basecache = je.Caches[typeof(ARHist)];
			je.Views.Caches.Add(typeof(CuryARHist));
			je.Views.Caches.Add(typeof(ARHist));
			
			string extRefNbrNumbering = je.CMSetup.Current.ExtRefNbrNumberingID;			
			if (string.IsNullOrEmpty(extRefNbrNumbering) == false)
			{
				RevaluationRefNbrHelper helper = new RevaluationRefNbrHelper(extRefNbrNumbering);
				helper.Subscribe(je);
			}

			DocumentList <Batch> created = new DocumentList<Batch>(je);

			Currency currency = PXSelect<Currency, Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(je, filter.CuryID);

			bool hasErrors = false;

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				foreach (RevaluedARHistory hist in list)
				{
                    PXProcessing<RevaluedARHistory>.SetCurrentItem(hist);
					if (hist.FinPtdRevalued == 0m)
					{
                        PXProcessing<RevaluedARHistory>.SetProcessed();
						continue;
					}

					string FinPeriod = 
						FinPeriodRepository.GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(hist.BranchID), filter.FinPeriodID)
						.Result
						.FinPeriodID;

					ProcessingResult result = CheckFinPeriod(FinPeriod, hist.BranchID);
					if (!result.IsSuccess)
					{
						hasErrors = true;
						continue;
					}

					if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
					{
						je.Save.Press();

						if (created.Find(je.BatchModule.Current) == null)
						{
							created.Add(je.BatchModule.Current);
						}
					}

					Batch cmbatch = created.Find<Batch.branchID>(hist.BranchID) ?? new Batch();
					if (cmbatch.BatchNbr == null)
					{
						je.Clear();

						CurrencyInfo info = new CurrencyInfo();
						info.CuryID = hist.CuryID;
						info.CuryEffDate = hist.CuryEffDate;
						info.BaseCalc = false;
						info = je.currencyinfo.Insert(info) ?? info;

						cmbatch = new Batch();
						cmbatch.BranchID = hist.BranchID;
						cmbatch.Module = "CM";
						cmbatch.Status = "U";
						cmbatch.AutoReverse = true;
						cmbatch.Released = true;
						cmbatch.Hold = false;
						cmbatch.DateEntered = filter.CuryEffDate;
						FinPeriodIDAttribute.SetPeriodsByMaster<Batch.finPeriodID>(je.BatchModule.Cache, cmbatch, filter.FinPeriodID);

						cmbatch.CuryID = hist.CuryID;
						cmbatch.CuryInfoID = info.CuryInfoID;
						cmbatch.DebitTotal = 0m;
						cmbatch.CreditTotal = 0m;
						cmbatch.Description = filter.Description;
						je.BatchModule.Insert(cmbatch);

						CurrencyInfo b_info = je.currencyinfo.Select();
						if (b_info != null)
						{
							b_info.CuryID = hist.CuryID;
							b_info.CuryEffDate = hist.CuryEffDate;
							b_info.CuryRateTypeID = hist.CuryRateTypeID;
							b_info.CuryRate = hist.CuryRate;
							b_info.RecipRate = hist.RateReciprocal;
							b_info.CuryMultDiv = hist.CuryMultDiv;
							je.currencyinfo.Update(b_info);
						}
					}
					else
					{
						if (!je.BatchModule.Cache.ObjectsEqual(je.BatchModule.Current, cmbatch))
						{
							je.Clear();
						}

						je.BatchModule.Current = je.BatchModule.Search<Batch.batchNbr>(cmbatch.BatchNbr, cmbatch.Module);
					}

					{
						GLTran tran = new GLTran();
						tran.SummPost = false;
						tran.AccountID = currency.ARProvAcctID ?? hist.AccountID;
						tran.SubID = currency.ARProvSubID ?? hist.SubID;
						tran.CuryDebitAmt = 0m;
						tran.CuryCreditAmt = 0m;

						tran.DebitAmt = (hist.FinPtdRevalued < 0m) ? 0m : hist.FinPtdRevalued;
						tran.CreditAmt = (hist.FinPtdRevalued < 0m) ? -1m * hist.FinPtdRevalued : 0m;

						tran.TranType = "REV";
						tran.TranClass = AccountType.Asset;
						tran.RefNbr = string.Empty;
						tran.TranDesc = filter.Description;
					    FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, filter.FinPeriodID);
						tran.TranDate = filter.CuryEffDate;
						tran.CuryInfoID = null;
						tran.Released = true;
						tran.ReferenceID = hist.CustomerID;

						je.GLTranModuleBatNbr.Insert(tran);
					}

                    CustomerClass custclass = PXSelectReadonly<CustomerClass, Where<CustomerClass.customerClassID, Equal<Required<CustomerClass.customerClassID>>>>.Select(je, hist.CustomerClassID);

                    if (custclass == null)
                    {
                        custclass = new CustomerClass();
                    }

                    if (custclass.UnrealizedGainAcctID == null)
                    {
                        custclass.UnrealizedGainSubID = null;
                    }

                    if (custclass.UnrealizedLossAcctID == null)
                    {
                        custclass.UnrealizedLossSubID = null;
                    }

					{
						GLTran tran = new GLTran();
						tran.SummPost = true;
						tran.ZeroPost = false;
						tran.CuryDebitAmt = 0m;
						tran.CuryCreditAmt = 0m;

						if (je.BatchModule.Current.DebitTotal > je.BatchModule.Current.CreditTotal)
						{
							tran.AccountID = custclass.UnrealizedGainAcctID ?? currency.UnrealizedGainAcctID;
                            tran.SubID = custclass.UnrealizedGainSubID ?? GainLossSubAccountMaskAttribute.GetSubID<Currency.unrealizedGainSubID>(je, hist.BranchID, currency);
							tran.DebitAmt = 0m;
							tran.CreditAmt = (je.BatchModule.Current.DebitTotal - je.BatchModule.Current.CreditTotal);
						}
						else
						{
                            tran.AccountID = custclass.UnrealizedLossAcctID ?? currency.UnrealizedLossAcctID;
                            tran.SubID = custclass.UnrealizedLossSubID ?? GainLossSubAccountMaskAttribute.GetSubID<Currency.unrealizedLossSubID>(je, hist.BranchID, currency);
							tran.DebitAmt = (je.BatchModule.Current.CreditTotal - je.BatchModule.Current.DebitTotal);
							tran.CreditAmt = 0m;
						}

						tran.TranType = "REV";
						tran.TranClass = GLTran.tranClass.UnrealizedAndRevaluationGOL;
						tran.RefNbr = string.Empty;
						tran.TranDesc = filter.Description;
						tran.Released = true;
						tran.ReferenceID = null;

						je.GLTranModuleBatNbr.Insert(tran);
					}

					{
						CuryARHist arhist = new CuryARHist();
						arhist.BranchID = hist.BranchID;
						arhist.AccountID = hist.AccountID;
						arhist.SubID = hist.SubID;
						arhist.FinPeriodID = filter.FinPeriodID;
						arhist.CustomerID = hist.CustomerID;
						arhist.CuryID = hist.CuryID;

						arhist = (CuryARHist)cache.Insert(arhist);
						arhist.FinPtdRevalued += hist.FinPtdRevalued;
					}

					{
						ARHist arhist = new ARHist();
						arhist.BranchID = hist.BranchID;
						arhist.AccountID = hist.AccountID;
						arhist.SubID = hist.SubID;
						arhist.FinPeriodID = filter.FinPeriodID;
						arhist.CustomerID = hist.CustomerID;

						arhist = (ARHist)basecache.Insert(arhist);
						arhist.FinPtdRevalued += hist.FinPtdRevalued;
					}

					PXProcessing<RevaluedARHistory>.SetProcessed();
				}

				if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
				{
					je.Save.Press();

					if (created.Find(je.BatchModule.Current) == null)
					{
						created.Add(je.BatchModule.Current);
					}
				}

				ts.Complete();
			}

            //Clean current to prevent set exception to the last item
            PXProcessing<RevaluedARHistory>.SetCurrentItem(null);

			CMSetup cmsetup = PXSelect<CMSetup>.Select(je);

			for (int i = 0; i < created.Count; i++)
			{
				if (cmsetup.AutoPostOption == true)
				{
					pg.Clear();
					pg.PostBatchProc(created[i]);
				}
			}

			if (hasErrors)
			{
				throw new PXException(ErrorMessages.SeveralItemsFailed);
			}

            if (created.Count > 0)
			{
                je.BatchModule.Current = created[created.Count - 1];
                throw new PXRedirectRequiredException(je, "Preview");
            }

            decimal val = 0m;
            foreach (RevaluedARHistory res in ARAccountList.Cache.Updated)
            {
                if ((bool)res.Selected)
                {
                    val += (decimal)res.FinPtdRevalued;
                }
            }

            if (val == 0)
            {
                throw new PXOperationCompletedWithWarningException(Messages.NoRevaluationEntryWasMade);
            }
		}
	}

	public class RevaluationRefNbrHelper
	{
		private Dictionary<string, string> _batchKeys;
		private string extRefNbrNumbering;		

		public RevaluationRefNbrHelper(string aExtRefNbrNumbering)  
		{
			this._batchKeys = new Dictionary<string, string>();
			this.extRefNbrNumbering = aExtRefNbrNumbering;
		}

		public void Subscribe(JournalEntry graph)
		{
			graph.RowPersisting.AddHandler<GLTran>(OnRowPersisting);
			graph.RowInserting.AddHandler<GLTran>(OnRowInserting);
		}

		public void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			GLTran tran = (GLTran)e.Row;
			if (tran != null && ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete))
			{
				AssignRefNbr(sender, tran, generateIfNew: true);
			}
		}

		public void OnRowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			GLTran tran = (GLTran)e.Row;
			if (tran != null)
			{

				/// Assigning RefNbr on RowInserting is important because this field is used as a criterion for
				/// summarizing GL entries (<see cref="JournalEntry.GLTranComparer"/>).
				/// If it is not populated by <see cref="JournalEntry.GLTran_RowInserting"/>, summary posting won't work properly.
				AssignRefNbr(sender, tran, generateIfNew: false);
			}
		}

		/// <summary>
		/// Assigns the <see cref="GLTran.RefNbr"/>.
		/// The method either generates a reference number based on the specified numbering sequence (<see cref="extRefNbrNumbering"/>),
		/// or takes it from the BatchNbr-RefNbr dictionary (<see cref="_batchKeys"/>).
		/// If the number is generated it is also stored in the dictionary.
		/// </summary>
		/// <param name="sender">cache of type <see cref="GLTran"/>. Will be used to get a cache of type <see cref="Batch"/>.</param>
		/// <param name="tran">transaction, to which the RefNbr is assigned</param>
		/// <param name="generateIfNew">specifies whether a RefNbr must be generated based on the sequence if the RefNbr is not found for the current Batch</param>
		private void AssignRefNbr(PXCache sender, GLTran tran, bool generateIfNew)
		{
				PXCache batchCache = sender.Graph.Caches[typeof(Batch)];
				Batch batch = (Batch)batchCache.Current;
				if (batch != null && string.IsNullOrEmpty(batch.BatchNbr) == false)
				{
					string batchNbr = batch.BatchNbr;
					if (string.IsNullOrEmpty(tran.RefNbr))
					{
					string extRefNbr = null;
					if (!_batchKeys.TryGetValue(batchNbr, out extRefNbr) && generateIfNew)
						{
							extRefNbr = AutoNumberAttribute.GetNextNumber(sender, tran, extRefNbrNumbering, tran.TranDate);
							_batchKeys.Add(batchNbr, extRefNbr);
						}

					if (extRefNbr != null)
					{
                        tran.RefNbr = extRefNbr;
					}

                        PXDBLiteDefaultAttribute.SetDefaultForInsert<GLTran.refNbr>(sender, tran, false);
					}
				}
			}
		}

	/// <summary>
	/// The DAC represents a view into the base <see cref="CuryARHistory"/> entity and provides information required for
	/// the AR History Revaluation process (see <see cref="RevalueARAccounts"/>).
	/// Records of this type are used to select and display data from <see cref="CuryARHistory"/> prior to revaluation process (see <see cref="RevalueARAccounts.apaccountlist"/>).
	/// The revaluation process itself is also performed based on these records and makes adjustments and generates transactions
	/// from them (see <see cref="RevalueARAccounts.Revalue(RevalueFilter, List{RevaluedARHistory})/>.
	[Serializable]
	[PXBreakInheritance()]
	public partial class RevaluedARHistory : CuryARHistory
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
		protected String _CuryRateTypeID;
		[PXString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency Rate Type")]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		public virtual String CuryRateTypeID
		{
			get
			{
				return this._CuryRateTypeID;
			}
			set
			{
				this._CuryRateTypeID = value;
			}
		}
		#endregion
		#region CuryRate
		public abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		protected Decimal? _CuryRate = 1m;
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Currency Rate")]
		public virtual Decimal? CuryRate
		{
			get
			{
				return this._CuryRate;
			}
			set
			{
				this._CuryRate = value;
			}
		}
		#endregion
		#region RateReciprocal
		public abstract class rateReciprocal : PX.Data.BQL.BqlDecimal.Field<rateReciprocal> { }
		protected Decimal? _RateReciprocal;

		[PXDecimal(8)]
		public virtual Decimal? RateReciprocal
		{
			get
			{
				return this._RateReciprocal;
			}
			set
			{
				this._RateReciprocal = value;
			}
		}
		#endregion
		#region CuryMultDiv
		public abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
		protected String _CuryMultDiv = "M";
		[PXString(1, IsFixed = true)]
		public virtual String CuryMultDiv
		{
			get
			{
				return this._CuryMultDiv;
			}
			set
			{
				this._CuryMultDiv = value;
			}
		}
		#endregion
		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		protected DateTime? _CuryEffDate;

		[PXDate()]
		public virtual DateTime? CuryEffDate
		{
			get
			{
				return this._CuryEffDate;
			}
			set
			{
				this._CuryEffDate = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account(IsKey = true, DescriptionField = typeof(Account.description))]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[SubAccount(IsKey = true, DescriptionField = typeof(Sub.description))]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(IsKey = true, IsDetail = true)]
		public override Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[Customer(IsKey = true, DescriptionField = typeof(Customer.acctName))]
		public override Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
        #region CustomerClassID
        public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
        protected String _CustomerClassID;
        [PXString(10, IsUnicode = true)]
        [PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
        [PXUIField(DisplayName = "Customer Class", Enabled = false)]
        public virtual String CustomerClassID
        {
            get
            {
                return this._CustomerClassID;
            }
            set
            {
                this._CustomerClassID = value;
            }
        }
        #endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region CuryFinYtdBalance
		public new abstract class curyFinYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdBalance> { }
		[PXDBCury(typeof(RevaluedARHistory.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Foreign Currency Balance", Enabled = false)]
		public override Decimal? CuryFinYtdBalance
		{
			get
			{
				return this._CuryFinYtdBalance;
			}
			set
			{
				this._CuryFinYtdBalance = value;
			}
		}
		#endregion
		#region FinYtdBalance
		public new abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Balance", Enabled = false)]
		public override Decimal? FinYtdBalance
		{
			get
			{
				return this._FinYtdBalance;
			}
			set
			{
				this._FinYtdBalance = value;
			}
		}
		#endregion
		#region FinPrevRevalued
		public abstract class finPrevRevalued : PX.Data.BQL.BqlDecimal.Field<finPrevRevalued> { }
		protected Decimal? _FinPrevRevalued;
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "PTD Gain or Loss", Enabled = false)]
		public virtual Decimal? FinPrevRevalued
		{
			get
			{
				return this._FinPrevRevalued;
			}
			set
			{
				this._FinPrevRevalued = value;
			}
		}
		#endregion
		#region FinYtdRevalued
		public abstract class finYtdRevalued : PX.Data.BQL.BqlDecimal.Field<finYtdRevalued> { }
		protected Decimal? _FinYtdRevalued;
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revalued Balance", Enabled = false)]
		[PXFormula(typeof(Add<Add<RevaluedARHistory.finYtdBalance, RevaluedARHistory.finPrevRevalued>, RevaluedARHistory.finPtdRevalued>))]
		public virtual Decimal? FinYtdRevalued
		{
			get
			{
				return this._FinYtdRevalued;
			}
			set
			{
				this._FinYtdRevalued = value;
			}
		}
		#endregion
		#region FinPtdRevalued
		public new abstract class finPtdRevalued : PX.Data.BQL.BqlDecimal.Field<finPtdRevalued> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Difference", Enabled = true)]
		public override Decimal? FinPtdRevalued
		{
			get
			{
				return this._FinPtdRevalued;
			}
			set
			{
				this._FinPtdRevalued = value;
			}
		}
		#endregion
		#region LastRevaluedFinPeriodID
		public abstract class lastRevaluedFinPeriodID : PX.Data.BQL.BqlString.Field<lastRevaluedFinPeriodID> { }

		[PXUIField(DisplayName = "Last Revaluation Period")]
		[FinPeriodID(IsDBField = false)]
		public virtual String LastRevaluedFinPeriodID { get; set; }
		#endregion
	}

	/// <summary>
	/// A view into the <see cref="CuryARHistory"/> entity, used by the Revalue AR History process
	/// to determine the most recent period when revaluation of a particular account and subaccount
	/// was performed (see <see cref="RevalueARAccounts.araccountlist"/>).
	/// </summary>
	[Serializable]
	[PXProjection(typeof(Select4<CuryARHistory,
		Where<CuryARHistory.finPtdRevalued, NotEqual<decimal0>>,
		Aggregate<
			GroupBy<CuryARHistory.branchID,
			GroupBy<CuryARHistory.customerID,
			GroupBy<CuryARHistory.accountID,
			GroupBy<CuryARHistory.subID,
			GroupBy<CuryARHistory.curyID,
			Max<CuryARHistory.finPeriodID>>>>>>>>))]
	public partial class ARHistoryLastRevaluation : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(IsKey = true, BqlField = typeof(CuryARHistory.branchID))]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer(IsKey = true, BqlField = typeof(CuryARHistory.customerID))]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(IsKey = true, BqlField = typeof(CuryARHistory.accountID))]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(IsKey = true, BqlField = typeof(CuryARHistory.subID))]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL", BqlField = typeof(CuryARHistory.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;
		[GL.FinPeriodID(BqlField = typeof(CuryARHistory.finPeriodID))]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
	}
}
