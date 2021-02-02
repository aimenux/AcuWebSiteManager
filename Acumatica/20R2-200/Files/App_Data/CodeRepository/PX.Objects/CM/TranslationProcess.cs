using System;
using PX.Data;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;

namespace PX.Objects.CM
{
	[TableAndChartDashboardType]
	[Serializable]
	public class TranslationProcess : PXGraph<TranslationProcess>
	{
		#region TableDefinition
		[Serializable]
		public partial class TranslationParams : PX.Data.IBqlTable
		{
			#region TranslDefId
			public abstract class translDefId : PX.Data.BQL.BqlString.Field<translDefId> { }
			protected String _TranslDefId;
			[PXString(10, IsUnicode = true)]
			[PXDefault(typeof(Search2<CMSetup.translDefId,
								LeftJoin<TranslDef,
									On<CMSetup.translDefId, Equal<TranslDef.translDefId>>>,
							Where<TranslDef.active, Equal<True>>>))]
			[PXUIField(DisplayName = "Translation ID", Required = true)]
			[PXSelector(typeof(Search<TranslDef.translDefId, Where<TranslDef.active, Equal<True>>>), DescriptionField = typeof(TranslDef.description))]
			public virtual String TranslDefId
			{
				get
				{
					return this._TranslDefId;
				}
				set
				{
					this._TranslDefId = value;
				}
			}
			#endregion
			#region Description
			public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
			protected String _Description;
			[PXString(60, IsUnicode = true)]
			[PXDefault(typeof(Search<TranslDef.description, Where<TranslDef.translDefId, Equal<Current<TranslationParams.translDefId>>>>))]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
			public virtual String Description
			{
				get
				{
					return this._Description;
				}
				set
				{
					this._Description = value;
				}
			}
			#endregion
			#region SourceLedgerId
			public abstract class sourceLedgerId : PX.Data.BQL.BqlInt.Field<sourceLedgerId> { }
			protected Int32? _SourceLedgerId;
			[PXInt()]
			[PXDefault(typeof(Search<TranslDef.sourceLedgerId, Where<TranslDef.translDefId, Equal<Current<TranslationParams.translDefId>>>>))]
			[PXUIField(DisplayName = "Source Ledger", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD), DescriptionField = typeof(Ledger.descr))]
			public virtual Int32? SourceLedgerId
			{
				get
				{
					return this._SourceLedgerId;
				}
				set
				{
					this._SourceLedgerId = value;
				}
			}
			#endregion
			#region DestLedgerId
			public abstract class destLedgerId : PX.Data.BQL.BqlInt.Field<destLedgerId> { }
			protected Int32? _DestLedgerId;
			[PXInt()]
			[PXDefault(typeof(Search<TranslDef.destLedgerId, Where<TranslDef.translDefId, Equal<Current<TranslationParams.translDefId>>>>))]
			[PXUIField(DisplayName = "Destination Ledger", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD),
				DescriptionField = typeof(Ledger.descr))]
			public virtual Int32? DestLedgerId
			{
				get
				{
					return this._DestLedgerId;
				}
				set
				{
					this._DestLedgerId = value;
				}
			}
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			protected String _FinPeriodID;
			[ClosedPeriod(
				searchType: null,
				sourceType: typeof(AccessInfo.businessDate),
				branchSourceType: typeof(TranslationParams.branchID),
			    useMasterOrganizationIDByDefault: true)]
			[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
			public virtual String FinPeriodID
			{
				get
				{
					return this._FinPeriodID;
				}
				set
				{
					this._FinPeriodID = value;
				}
			}
			#endregion
			#region CuryEffDate
			public abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
			protected DateTime? _CuryEffDate;
			[PXDate()]
			[PXUIField(DisplayName = "Currency Effective Date")]
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
			#region SourceCuryID
			public abstract class sourceCuryID : PX.Data.BQL.BqlString.Field<sourceCuryID> { }
			protected String _SourceCuryID;
			[PXString(5, IsUnicode = true)]
			[PXUIField(DisplayName = "Source Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			[PXDefault(typeof(Search2<Ledger.baseCuryID, InnerJoin<TranslDef, On<Ledger.ledgerID, Equal<TranslDef.sourceLedgerId>>>,
														Where<TranslDef.translDefId, Equal<Current<TranslationParams.translDefId>>>>))]
			public virtual String SourceCuryID
			{
				get
				{
					return this._SourceCuryID;
				}
				set
				{
					this._SourceCuryID = value;
				}
			}
			#endregion
			#region DestCuryID
			public abstract class destCuryID : PX.Data.BQL.BqlString.Field<destCuryID> { }
			protected String _DestCuryID;
			[PXString(5, IsUnicode = true)]
			[PXDefault(typeof(Search2<Ledger.baseCuryID, InnerJoin<TranslDef, On<Ledger.ledgerID, Equal<TranslDef.destLedgerId>>>,
														Where<TranslDef.translDefId, Equal<Current<TranslationParams.translDefId>>>>))]

			[PXUIField(DisplayName = "Destination Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual String DestCuryID
			{
				get
				{
					return this._DestCuryID;
				}
				set
				{
					this._DestCuryID = value;
				}
			}
			#endregion
			#region LastFinPeriodID
			public abstract class lastFinPeriodID : PX.Data.BQL.BqlString.Field<lastFinPeriodID> { }
			protected String _LastFinPeriodID;
			[PXString(6, IsFixed = true)]
			[FinPeriodIDFormatting]
			[PXUIField(DisplayName = "Last Fin. Period", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual String LastFinPeriodID
			{
				get
				{
					return this._LastFinPeriodID;
				}
				set
				{
					this._LastFinPeriodID = value;
				}
			}
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			protected Int32? _BranchID;
			[PXInt()]
			[PXDefault(typeof(Search<TranslDef.branchID, Where<TranslDef.translDefId, Equal<Current<TranslationParams.translDefId>>>>))]
			[PXUIField(DisplayName = "Branch", FieldClass = BranchAttribute._FieldClass,
				Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Required = false)]
			[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD),
				DescriptionField = typeof(Branch.acctName))]
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
		}
		#endregion

		#region Variables
		public PXFilter<TranslationParams> TranslationParamsFilter;
		[PXHidden]
		public PXSelect<TranslDef> TranslationDefinition;
		[PXFilterable]
		public PXSelect<CurrencyRate> TranslationCurrencyRateRecords;
		public PXSetup<CMSetup> CMSetup;
		public PXSetup<CMSetup> TSetup;

		[Obsolete("This field is not used anymore and will be removed in Acumatica ERP 8.0.")]
		public bool translAvailable = false;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }
		#endregion

		#region Buttons
		public PXAction<TranslationParams> cancel;
		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual System.Collections.IEnumerable Cancel(PXAdapter adapter)
		{
			PXLongOperation.ClearStatus(this.UID);
			return adapter.Get();
		}

		public PXAction<TranslationParams> Translate;

		protected static string getCompanyFinPeriodID(int? BranchID, string FinPeriodID, IFinPeriodRepository FinPeriodRepository)
		{
			bool isMultipleCalendarsSupportEnabled = PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>();
			bool isNeedConvertToCompanyPeriod = isMultipleCalendarsSupportEnabled && BranchID == null;

			return
				isNeedConvertToCompanyPeriod
					? FinPeriodRepository.
						GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(PXAccess.GetBranchID()), FinPeriodID)
						.GetValueOrRaiseError()
						.FinPeriodID
					: FinPeriodID;
		}

		[PXUIField(DisplayName = Messages.CreateTranslation, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable translate(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(TranslationParams)];
			TranslationParams parameters = TranslationParamsFilter.Current;

			if (parameters.TranslDefId == null || parameters.FinPeriodID == null || parameters.CuryEffDate == null)
			{
				return adapter.Get();
			}

			if (parameters.BranchID != null)
			{
				FinPeriod finPeriod =	
					FinPeriodRepository
					.GetByID(parameters.FinPeriodID, PXAccess.GetParentOrganizationID(parameters.BranchID));

				FinPeriodUtils.CanPostToPeriod(finPeriod).RaiseIfHasError();
			}

			var destinationLedger = (Ledger)PXSelectorAttribute.Select<TranslationParams.destLedgerId>(TranslationParamsFilter.Cache, parameters);
			var destinationCurrency = (Currency)PXSelectorAttribute.Select<Ledger.baseCuryID>(Caches[typeof(Ledger)], destinationLedger);

			if (destinationCurrency == null ||
				destinationCurrency.TranslationGainAcctID == null || destinationCurrency.TranslationGainSubID == null ||
				destinationCurrency.TranslationLossAcctID == null || destinationCurrency.TranslationLossSubID == null)
			{
				throw new PXException(Messages.TranslationDestinationCurrencyMissingGOLAccounts, destinationCurrency?.CuryID ?? "");
			}

			TranslationHistory existHistory = (TranslationHistory)PXSelect<TranslationHistory,
																		Where<TranslationHistory.ledgerID, Equal<Required<TranslationHistory.ledgerID>>,
																		  And<TranslationHistory.status, NotEqual<TranslationStatus.released>,
																		  And<TranslationHistory.status, NotEqual<TranslationStatus.voided>>>>,
																		OrderBy<Desc<TranslationHistory.finPeriodID>>>
												.Select(this, parameters.DestLedgerId);

			if (existHistory != null)
			{
				throw new PXException(Messages.NotReleasedTranslationExists, existHistory.ReferenceNbr);
			}

			existHistory = (TranslationHistory)PXSelectJoin<TranslationHistory,
													LeftJoin<Batch,
														On<TranslationHistory.batchNbr, Equal<Batch.batchNbr>,
															And<Batch.module, Equal<BatchModule.moduleCM>>>>,
													Where2<Where<TranslationHistory.ledgerID, Equal<Required<TranslationHistory.ledgerID>>,
															And<Batch.status, NotEqual<BatchStatus.posted>>>,
														And<Where<TranslationHistory.branchID, Equal<Required<TranslationHistory.branchID>>,
															Or<TranslationHistory.branchID, IsNull,
															Or<Required<TranslationHistory.branchID>, IsNull>>>>>>
												.SelectSingleBound(this, null, parameters.DestLedgerId, parameters.BranchID, parameters.BranchID);
			if (existHistory != null)
			{
				throw new PXException(Messages.NotPostedBatchesExists);
			}

			string companyPeriodID = getCompanyFinPeriodID(parameters.BranchID, parameters.FinPeriodID, FinPeriodRepository);
			
			existHistory = (TranslationHistory)PXSelect<TranslationHistory,
													Where<TranslationHistory.finPeriodID, Greater<Required<TranslationHistory.finPeriodID>>,
													  And<TranslationHistory.ledgerID, Equal<Required<TranslationHistory.ledgerID>>,
													  And<TranslationHistory.released, Equal<boolTrue>>>>,
													OrderBy<Desc<TranslationHistory.finPeriodID>>>
							.Select(this, companyPeriodID, parameters.DestLedgerId);

			if (existHistory != null)
			{
				cache.RaiseExceptionHandling<TranslationParams.finPeriodID>(
					parameters,
					PXFieldState.UnwrapValue(TranslationParamsFilter.GetValueExt<TranslationParams.finPeriodID>(parameters)),
					new PXSetPropertyException(Messages.ReleasedTranslationExistsInGreaterPeriod, existHistory.ReferenceNbr, PXErrorLevel.Warning));
			}

			if (PXLongOperation.Exists(UID))
			{
				throw new PXException(GL.Messages.PrevOperationNotCompleteYet);
			}
			try
			{
				TranslationDefinitionMaint graph = PXGraph.CreateInstance<TranslationDefinitionMaint>();
				TranslDef def = PXSelect<TranslDef, Where<TranslDef.translDefId, Equal<Required<TranslDef.translDefId>>>>.Select(this, parameters.TranslDefId);
				if (def != null)
				{
					foreach (TranslDefDet det in PXSelect<TranslDefDet, Where<TranslDefDet.translDefId, Equal<Required<TranslDef.translDefId>>>>.Select(this, parameters.TranslDefId))
					{
						PXCache detCache = graph.Caches[typeof(TranslDefDet)];
						graph.CheckDetail(detCache, det, def.Active == true, (int)parameters.DestLedgerId, def, new Exception(Messages.TransactionCanNotBeCreated));
					}
				}
			}
			catch (Exception)
			{
				throw new Exception(Messages.TranslationDefinitionHasSomeCrossIntervals);
			}


			List<CurrencyRate> rateList = new List<CurrencyRate>();
			foreach (CurrencyRate rate in TranslationCurrencyRateRecords.Select())
			{
				rateList.Add(rate);
			}
			PXLongOperation.StartOperation(this, delegate () { TranslHistCreate(parameters, rateList); });
			return adapter.Get();
		}
		#endregion

		#region Functions
		protected virtual IEnumerable translationCurrencyRateRecords()
		{
			List<CurrencyRate> rateList = new List<CurrencyRate>();

            if (TranslationParamsFilter.Current.TranslDefId == null || TranslationParamsFilter.Current.FinPeriodID == null)
                return rateList;

            foreach (PXResult<TranslDef, TranslDefDet> res
							in PXSelectJoinGroupBy<TranslDef, InnerJoin<TranslDefDet, On<TranslDefDet.translDefId, Equal<TranslDef.translDefId>>>,
													 Where<TranslDef.translDefId, Equal<Current<TranslationParams.translDefId>>>,
													 Aggregate<GroupBy<TranslDefDet.rateTypeId>>>.Select(this))
			{
				TranslDef def = (TranslDef)res;
				Ledger sourceLedger = PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(this, def.SourceLedgerId);
				Ledger destLedger = PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(this, def.DestLedgerId);
				if (sourceLedger == null || destLedger == null || sourceLedger.BaseCuryID == null || destLedger.BaseCuryID == null)
					throw new Exception(Messages.TranslationDefinitionLedgerNotFound);

				CurrencyRate rate = (CurrencyRate)PXSelect<CurrencyRate, Where<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
																											And<CurrencyRate.curyEffDate, LessEqual<Current<TranslationParams.curyEffDate>>,
																											And<Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
																												  And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>>>>>>,
																			OrderBy<Desc<CurrencyRate.curyEffDate>>>
																				.Select(this, ((TranslDefDet)res).RateTypeId, sourceLedger.BaseCuryID, destLedger.BaseCuryID);
				if (rate != null)
				{
					rateList.Add(rate);
				}

			}
			return rateList;
		}

		protected static void TranslHistCreate(TranslationParams parameters, List<CurrencyRate> rateList)
		{
			TranslationHistoryMaint TransHistGraph = PXGraph.CreateInstance<TranslationHistoryMaint>();

			Ledger sLedger = PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(TransHistGraph, parameters.SourceLedgerId);
			Ledger dLedger = PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(TransHistGraph, parameters.DestLedgerId);
			Currency dcurr = PXSelect<Currency, Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(TransHistGraph, dLedger.BaseCuryID);
			CMSetup cmsetup = PXSelect<CMSetup>.Select(TransHistGraph);
			GLSetup glsetup = PXSelect<GLSetup>.Select(TransHistGraph);
			IFinPeriodRepository finPeriodRepository = TransHistGraph.GetService<IFinPeriodRepository>();
			string companyPeriodID = getCompanyFinPeriodID(parameters.BranchID, parameters.FinPeriodID, finPeriodRepository);

		    TranslationHistory hist = new TranslationHistory();

            if (parameters.BranchID == null)
            {
                hist.BranchID = TransHistGraph.Accessinfo.BranchID;
                hist.FinPeriodID =
                    finPeriodRepository.GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(hist.BranchID),
                        parameters.FinPeriodID).GetValueOrRaiseError().FinPeriodID;
            }
            else
            {
                hist.BranchID = parameters.BranchID;
                hist.FinPeriodID = parameters.FinPeriodID;
            }
			
			hist.CuryEffDate = parameters.CuryEffDate;
			hist.Description = parameters.Description;
			hist.LedgerID = parameters.DestLedgerId;
			hist.Status = "U";
			hist.TranslDefId = parameters.TranslDefId;
			hist = TransHistGraph.TranslHistRecords.Insert(hist);

			Dictionary<int?, decimal?> totalAmount = new Dictionary<int?, decimal?>();

			foreach (TranslDefDet defdet in PXSelect<TranslDefDet,
											   Where<TranslDefDet.translDefId, Equal<Required<TranslDefDet.translDefId>>>>.
											   Select(TransHistGraph, parameters.TranslDefId))
			{
				CurrencyRate rate = null;

				PXCache defdetcache = TransHistGraph.Caches[typeof(TranslDefDet)];
				PXCache curycache = TransHistGraph.Caches[typeof(CurrencyInfo)];

				string AcctFromCD = TranslDefDet.GetAcct(defdetcache.Graph, (Int32)defdet.AccountIdFrom);
				string SubFromCD = (defdet.SubIdFrom != null ? TranslDefDet.GetSub(defdetcache.Graph, (Int32)defdet.SubIdFrom) : null);
				string AcctToCD = TranslDefDet.GetAcct(defdetcache.Graph, (Int32)defdet.AccountIdTo);
				string SubToCD = (defdet.SubIdTo != null ? TranslDefDet.GetSub(defdetcache.Graph, (Int32)defdet.SubIdTo) : null);
				for (int i = 0; i < rateList.Count; i++)
				{
					CurrencyRate tRate = rateList[i];
					if ((tRate.FromCuryID == sLedger.BaseCuryID &&
						 tRate.ToCuryID == dLedger.BaseCuryID &&
						 tRate.CuryRateType == defdet.RateTypeId)
					   )
					{
						rate = tRate;
						break;
					}
				}

				CurrencyInfo info = new CurrencyInfo();

				info.CuryID = sLedger.BaseCuryID;
				info.BaseCuryID = dLedger.BaseCuryID;
				info.CuryEffDate = parameters.CuryEffDate;
				info.CuryRateTypeID = defdet.RateTypeId;

				if (rate != null)
				{
					info.CuryMultDiv = rate.CuryMultDiv;
					info.CuryRate = rate.CuryRate;
				}
				else if (object.Equals(info.CuryID, info.BaseCuryID))
				{
					info.CuryMultDiv = "M";
					info.CuryRate = 1m;
				}
				else
				{
					info.CuryMultDiv = null;
					info.CuryRate = null;
				}

				if (defdet.CalcMode == 1) //YTD Balance
				{
					PXSelectBase<GLHistoryByPeriod> cmd = new
								PXSelectJoin<GLHistoryByPeriod,
												InnerJoin<Account, On<Account.accountID, Equal<GLHistoryByPeriod.accountID>>,
												InnerJoin<Sub, On<Sub.subID, Equal<GLHistoryByPeriod.subID>>,
												CrossJoin<GLSetup,
												InnerJoin<GLHistory, On<GLHistory.ledgerID, Equal<GLHistoryByPeriod.ledgerID>,
																   And<GLHistory.branchID, Equal<GLHistoryByPeriod.branchID>,
																	And<GLHistory.accountID, Equal<GLHistoryByPeriod.accountID>,
																	And<GLHistory.subID, Equal<GLHistoryByPeriod.subID>,
																	And<GLHistory.finPeriodID, Equal<GLHistoryByPeriod.lastActivityPeriod>>>>>>>>>>,
												Where<GLHistoryByPeriod.ledgerID, Equal<Required<GLHistoryByPeriod.ledgerID>>,
												  And<GLHistoryByPeriod.finPeriodID, Equal<Required<GLHistoryByPeriod.finPeriodID>>,
												  And<GLHistoryByPeriod.accountID, NotEqual<GLSetup.ytdNetIncAccountID>,
												  And<GLHistoryByPeriod.accountID, NotEqual<Required<GLHistoryByPeriod.accountID>>,
												  And<GLHistoryByPeriod.accountID, NotEqual<Required<GLHistoryByPeriod.accountID>>,
												  And2<Where<Account.type, Equal<AccountType.asset>,
														Or<Account.type, Equal<AccountType.liability>,
														Or<GLHistoryByPeriod.lastActivityPeriod, GreaterEqual<Required<GLHistoryByPeriod.lastActivityPeriod>>>>>,
												  And<Where<Account.accountCD, Between<Required<Account.accountCD>, Required<Account.accountCD>>,
														 And<Account.accountCD, NotEqual<Required<Account.accountCD>>,
														 And<Account.accountCD, NotEqual<Required<Account.accountCD>>,
														 Or<Account.accountCD, Equal<Required<Account.accountCD>>,
														And<Account.accountCD, Less<Required<Account.accountCD>>,
														And2<Where<Required<Sub.subCD>, IsNull,
																Or<Required<Sub.subCD>, IsNotNull,
															   And<Sub.subCD, GreaterEqual<Required<Sub.subCD>>>>>,
														 Or<Account.accountCD, Equal<Required<Account.accountCD>>,
														And<Account.accountCD, Greater<Required<Account.accountCD>>,
														And2<Where<Required<Sub.subCD>, IsNull,
															   Or<Required<Sub.subCD>, IsNotNull,
															  And<Sub.subCD, LessEqual<Required<Sub.subCD>>>>>,
														 Or<Account.accountCD, Equal<Required<Account.accountCD>>,
														And<Account.accountCD, Equal<Required<Account.accountCD>>,
														And<Where<Required<Sub.subCD>, IsNull,
															  And<Required<Sub.subCD>, IsNull,
															   Or<Required<Sub.subCD>, IsNotNull,
															  And<Required<Sub.subCD>, IsNotNull,
															  And<Sub.subCD, Between<Required<Sub.subCD>, Required<Sub.subCD>>>>>>>>>>>>>>>>>>>>>>>>>>>(TransHistGraph);

					foreach (PXResult<GLHistoryByPeriod, Account, Sub, GLSetup, GLHistory> res in cmd.Select(
							parameters.SourceLedgerId,
							companyPeriodID,
							dcurr.TranslationGainAcctID,
							dcurr.TranslationLossAcctID,
							companyPeriodID.Substring(0, 4) + "01",
							AcctFromCD,
							AcctToCD,
							AcctFromCD,
							AcctToCD,
							AcctFromCD,
							AcctToCD,
							SubFromCD,
							SubFromCD,
							SubFromCD,
							AcctToCD,
							AcctFromCD,
							SubToCD,
							SubToCD,
							SubToCD,
							AcctFromCD,
							AcctToCD,
							SubFromCD,
							SubToCD,
							SubFromCD,
							SubToCD,
							SubFromCD,
							SubToCD))
					{
						GLHistory accthist = (GLHistory)res;
						Account account = (Account)res;

						if (parameters.BranchID == null || parameters.BranchID == accthist.BranchID)
						{
							decimal YtdSource = (decimal)accthist.FinYtdBalance;
							decimal YtdTranslated;
							decimal YtdCalculated;
							try
							{
								PXDBCurrencyAttribute.CuryConvBase(curycache, info, YtdSource, out YtdTranslated);
							}
							catch (PXRateNotFoundException)
							{
								throw new PXRateIsNotDefinedForThisDateException(info);
							}
							decimal YtdOrig = 0;
							YtdCalculated = YtdTranslated;

							if (YtdCalculated == 0m)
							{
								continue;
							}

							TranslationHistoryDetails histdet = new TranslationHistoryDetails();
							histdet.LedgerID = dLedger.LedgerID;
							histdet.BranchID = accthist.BranchID;
							histdet.AccountID = accthist.AccountID;
							histdet.SubID = accthist.SubID;
							histdet.FinPeriodID = companyPeriodID;
							histdet.CalcMode = defdet.CalcMode;
							histdet.SourceAmt = YtdSource;
							histdet.TranslatedAmt = YtdTranslated;
							histdet.OrigTranslatedAmt = YtdOrig;
							histdet.CuryID = info.BaseCuryID;
							histdet.CuryEffDate = info.CuryEffDate;
							histdet.RateTypeID = info.CuryRateTypeID;
							histdet.CuryMultDiv = info.CuryMultDiv;
							histdet.CuryRate = info.CuryRate;
							histdet.LineType = "T";
							histdet.LineNbr = 0;
							histdet.ReferenceNbr = hist.ReferenceNbr;

							if (account.Type == AccountType.Asset || account.Type == AccountType.Expense)
							{
								histdet.DebitAmt = ((1 + Math.Sign(YtdCalculated)) / 2) * Math.Abs(YtdCalculated);
								histdet.CreditAmt = ((1 - Math.Sign(YtdCalculated)) / 2) * Math.Abs(YtdCalculated);
							}
							else
							{
								histdet.DebitAmt = ((1 - Math.Sign(YtdCalculated)) / 2) * Math.Abs(YtdCalculated);
								histdet.CreditAmt = ((1 + Math.Sign(YtdCalculated)) / 2) * Math.Abs(YtdCalculated); ;
							}

							histdet = TransHistGraph.TranslHistDetRecords.Insert(histdet);

							if (totalAmount.ContainsKey(histdet.BranchID))
							{
								totalAmount[histdet.BranchID] += histdet.DebitAmt - histdet.CreditAmt;
							}
							else
							{
								totalAmount[histdet.BranchID] = histdet.DebitAmt - histdet.CreditAmt;
							}
						}
					}
					foreach (PXResult<GLHistoryByPeriod, Account, Sub, GLSetup, GLHistory> res in cmd.Select(
							  parameters.DestLedgerId,
							  companyPeriodID,
							  dcurr.TranslationGainAcctID,
							  dcurr.TranslationLossAcctID,
							  companyPeriodID.Substring(0, 4) + "01",
							  AcctFromCD,
							  AcctToCD,
							  AcctFromCD,
							  AcctToCD,
							  AcctFromCD,
							  AcctToCD,
							  SubFromCD,
							  SubFromCD,
							  SubFromCD,
							  AcctToCD,
							  AcctFromCD,
							  SubToCD,
							  SubToCD,
							  SubToCD,
							  AcctFromCD,
							  AcctToCD,
							  SubFromCD,
							  SubToCD,
							  SubFromCD,
							  SubToCD,
							  SubFromCD,
							  SubToCD))
					{
						GLHistory accthist = (GLHistory)res;
						Account account = (Account)res;

						if (parameters.BranchID == null || parameters.BranchID == accthist.BranchID)
						{
							decimal YtdSource = 0;
							decimal YtdTranslated = 0;
							decimal YtdCalculated;
							decimal YtdOrig = 0;
							YtdCalculated = YtdTranslated;
							if (accthist != null && accthist.FinYtdBalance != null)
							{
								YtdCalculated -= (decimal)accthist.FinYtdBalance;
								YtdOrig += (decimal)accthist.FinYtdBalance;
							}

							TranslationHistoryDetails histdet = new TranslationHistoryDetails();
							histdet.ReferenceNbr = hist.ReferenceNbr;
							histdet.BranchID = accthist.BranchID;
							histdet.AccountID = accthist.AccountID;
							histdet.SubID = accthist.SubID;
							histdet.LineType = "T";
							histdet.LedgerID = dLedger.LedgerID;
							histdet.FinPeriodID = companyPeriodID;
							histdet.CalcMode = defdet.CalcMode;
							histdet.SourceAmt = YtdSource;
							histdet.TranslatedAmt = YtdTranslated;
							histdet.OrigTranslatedAmt = YtdOrig;
							histdet.CuryID = info.BaseCuryID;
							histdet.CuryEffDate = info.CuryEffDate;
							histdet.RateTypeID = info.CuryRateTypeID;
							histdet.CuryMultDiv = info.CuryMultDiv;
							histdet.CuryRate = info.CuryRate;
							histdet.LineNbr = 0;

							TranslationHistoryDetails existing = null;
							if ((existing = TransHistGraph.TranslHistDetRecords.Locate(histdet)) != null)
							{
								histdet = PXCache<TranslationHistoryDetails>.CreateCopy(existing);
								histdet.OrigTranslatedAmt += YtdOrig;
								if (account.Type == AccountType.Asset || account.Type == AccountType.Expense)
								{
									YtdCalculated += (decimal)histdet.DebitAmt - (decimal)histdet.CreditAmt;
								}
								else
								{
									YtdCalculated -= (decimal)histdet.DebitAmt - (decimal)histdet.CreditAmt;
								}
								totalAmount[histdet.BranchID] -= histdet.DebitAmt - histdet.CreditAmt;
							}

							if (YtdCalculated == 0m)
							{
								if (existing != null)
								{
									TransHistGraph.TranslHistDetRecords.Delete(histdet);
								}
								continue;
							}

							if (account.Type == AccountType.Asset || account.Type == AccountType.Expense)
							{
								histdet.DebitAmt = ((1 + Math.Sign(YtdCalculated)) / 2) * Math.Abs(YtdCalculated);
								histdet.CreditAmt = ((1 - Math.Sign(YtdCalculated)) / 2) * Math.Abs(YtdCalculated);
							}
							else
							{
								histdet.DebitAmt = ((1 - Math.Sign(YtdCalculated)) / 2) * Math.Abs(YtdCalculated);
								histdet.CreditAmt = ((1 + Math.Sign(YtdCalculated)) / 2) * Math.Abs(YtdCalculated); ;
							}

							if (existing == null)
							{
								histdet = TransHistGraph.TranslHistDetRecords.Insert(histdet);
							}
							else
							{
								histdet = TransHistGraph.TranslHistDetRecords.Update(histdet);
							}

							if (totalAmount.ContainsKey(histdet.BranchID))
							{
								totalAmount[histdet.BranchID] += histdet.DebitAmt - histdet.CreditAmt;
							}
							else
							{
								totalAmount[histdet.BranchID] = histdet.DebitAmt - histdet.CreditAmt;
							}
						}
					}
				}
				else
				{
					PXSelectBase<GLHistory> cmd = new
								PXSelectJoin<GLHistory,
												InnerJoin<Account, On<Account.accountID, Equal<GLHistory.accountID>>,
												InnerJoin<Sub, On<Sub.subID, Equal<GLHistory.subID>>,
												CrossJoin<GLSetup>>>,
												Where<GLHistory.ledgerID, Equal<Required<GLHistory.ledgerID>>,
												  And<GLHistory.finPeriodID, Equal<Required<GLHistory.finPeriodID>>,
												  And<GLHistory.accountID, NotEqual<GLSetup.ytdNetIncAccountID>,
												  And<GLHistory.accountID, NotEqual<Required<GLHistory.accountID>>,
												  And<GLHistory.accountID, NotEqual<Required<GLHistory.accountID>>,
												  And<Where<Account.accountCD, Between<Required<Account.accountCD>, Required<Account.accountCD>>,
														 And<Account.accountCD, NotEqual<Required<Account.accountCD>>,
														 And<Account.accountCD, NotEqual<Required<Account.accountCD>>,
														 Or<Account.accountCD, Equal<Required<Account.accountCD>>,
														And<Account.accountCD, Less<Required<Account.accountCD>>,
														And2<Where<Required<Sub.subCD>, IsNull,
																Or<Required<Sub.subCD>, IsNotNull,
															   And<Sub.subCD, GreaterEqual<Required<Sub.subCD>>>>>,
														 Or<Account.accountCD, Equal<Required<Account.accountCD>>,
														And<Account.accountCD, Greater<Required<Account.accountCD>>,
														And2<Where<Required<Sub.subCD>, IsNull,
															   Or<Required<Sub.subCD>, IsNotNull,
															  And<Sub.subCD, LessEqual<Required<Sub.subCD>>>>>,
														 Or<Account.accountCD, Equal<Required<Account.accountCD>>,
														And<Account.accountCD, Equal<Required<Account.accountCD>>,
														And<Where<Required<Sub.subCD>, IsNull,
															  And<Required<Sub.subCD>, IsNull,
															   Or<Required<Sub.subCD>, IsNotNull,
															  And<Required<Sub.subCD>, IsNotNull,
															  And<Sub.subCD, Between<Required<Sub.subCD>, Required<Sub.subCD>>>>>>>>>>>>>>>>>>>>>>>>>>(TransHistGraph);

					foreach (PXResult<GLHistory, Account> res in cmd.Select(
							parameters.SourceLedgerId,
							companyPeriodID,
							dcurr.TranslationGainAcctID,
							dcurr.TranslationLossAcctID,
							AcctFromCD,
							AcctToCD,
							AcctFromCD,
							AcctToCD,
							AcctFromCD,
							AcctToCD,
							SubFromCD,
							SubFromCD,
							SubFromCD,
							AcctToCD,
							AcctFromCD,
							SubToCD,
							SubToCD,
							SubToCD,
							AcctFromCD,
							AcctToCD,
							SubFromCD,
							SubToCD,
							SubFromCD,
							SubToCD,
							SubFromCD,
							SubToCD))
					{
						GLHistory accthist = (GLHistory)res;
						Account account = (Account)res;

						if (parameters.BranchID == null || parameters.BranchID == accthist.BranchID)
						{
							decimal PtdSource = (decimal)(accthist.FinPtdDebit - accthist.FinPtdCredit);
							decimal PtdTranslated;
							decimal PtdCalculated;
							try
							{
								PXDBCurrencyAttribute.CuryConvBase(curycache, info, PtdSource, out PtdTranslated);
							}
							catch (PXRateNotFoundException)
							{
								throw new PXRateIsNotDefinedForThisDateException(info);
							}
							decimal PtdOrig = 0;
							PtdCalculated = PtdTranslated;

							if (PtdCalculated == 0m)
							{
								continue;
							}

							TranslationHistoryDetails histdet = new TranslationHistoryDetails();
							histdet.LedgerID = dLedger.LedgerID;
							histdet.BranchID = accthist.BranchID;
							histdet.AccountID = accthist.AccountID;
							histdet.SubID = accthist.SubID;
							histdet.FinPeriodID = companyPeriodID;
							histdet.CalcMode = defdet.CalcMode;
							histdet.SourceAmt = PtdSource;
							histdet.TranslatedAmt = PtdTranslated;
							histdet.OrigTranslatedAmt = PtdOrig;
							histdet.CuryID = info.BaseCuryID;
							histdet.CuryEffDate = info.CuryEffDate;
							histdet.RateTypeID = info.CuryRateTypeID;
							histdet.CuryMultDiv = info.CuryMultDiv;
							histdet.CuryRate = info.CuryRate;
							histdet.LineType = "T";
							histdet.LineNbr = 0;
							histdet.ReferenceNbr = hist.ReferenceNbr;

							histdet.DebitAmt = ((1 + Math.Sign(PtdCalculated)) / 2) * Math.Abs(PtdCalculated);
							histdet.CreditAmt = ((1 - Math.Sign(PtdCalculated)) / 2) * Math.Abs(PtdCalculated);

							histdet = TransHistGraph.TranslHistDetRecords.Insert(histdet);
							if (totalAmount.ContainsKey(histdet.BranchID))
							{
								totalAmount[histdet.BranchID] += histdet.DebitAmt - histdet.CreditAmt;
							}
							else
							{
								totalAmount[histdet.BranchID] = histdet.DebitAmt - histdet.CreditAmt;
							}
						}
					}

					foreach (PXResult<GLHistory, Account> res in cmd.Select(
							parameters.DestLedgerId,
							companyPeriodID,
							dcurr.TranslationGainAcctID,
							dcurr.TranslationLossAcctID,
							AcctFromCD,
							AcctToCD,
							AcctFromCD,
							AcctToCD,
							AcctFromCD,
							AcctToCD,
							SubFromCD,
							SubFromCD,
							SubFromCD,
							AcctToCD,
							AcctFromCD,
							SubToCD,
							SubToCD,
							SubToCD,
							AcctFromCD,
							AcctToCD,
							SubFromCD,
							SubToCD,
							SubFromCD,
							SubToCD,
							SubFromCD,
							SubToCD))
					{
						GLHistory accthist = (GLHistory)res;
						Account account = (Account)res;

						if (parameters.BranchID == null || parameters.BranchID == accthist.BranchID)
						{
							decimal PtdSource = 0;
							decimal PtdTranslated = 0;
							decimal PtdCalculated;
							decimal PtdOrig = 0;
							PtdCalculated = PtdTranslated;

							if (accthist != null)
							{
								PtdCalculated -= (decimal)(accthist.FinPtdDebit - accthist.FinPtdCredit);
								PtdOrig += (decimal)(accthist.FinPtdDebit - accthist.FinPtdCredit);
							}

							TranslationHistoryDetails histdet = new TranslationHistoryDetails();
							histdet.ReferenceNbr = hist.ReferenceNbr;
							histdet.BranchID = accthist.BranchID;
							histdet.AccountID = accthist.AccountID;
							histdet.SubID = accthist.SubID;
							histdet.LineType = "T";
							histdet.LedgerID = dLedger.LedgerID;
							histdet.FinPeriodID = companyPeriodID;
							histdet.CalcMode = defdet.CalcMode;
							histdet.SourceAmt = PtdSource;
							histdet.TranslatedAmt = PtdTranslated;
							histdet.OrigTranslatedAmt = PtdOrig;
							histdet.CuryID = info.BaseCuryID;
							histdet.CuryEffDate = info.CuryEffDate;
							histdet.RateTypeID = info.CuryRateTypeID;
							histdet.CuryMultDiv = info.CuryMultDiv;
							histdet.CuryRate = info.CuryRate;
							histdet.LineNbr = 0;

							TranslationHistoryDetails existing = null;
							if ((existing = TransHistGraph.TranslHistDetRecords.Locate(histdet)) != null)
							{
								histdet = PXCache<TranslationHistoryDetails>.CreateCopy(existing);
								histdet.OrigTranslatedAmt += PtdOrig;
								PtdCalculated += (decimal)histdet.DebitAmt - (decimal)histdet.CreditAmt;
								totalAmount[histdet.BranchID] -= histdet.DebitAmt - histdet.CreditAmt;
							}

							if (PtdCalculated == 0m)
							{
								if (existing != null)
								{
									TransHistGraph.TranslHistDetRecords.Delete(histdet);
								}
								continue;
							}

							histdet.DebitAmt = ((1 + Math.Sign(PtdCalculated)) / 2) * Math.Abs(PtdCalculated);
							histdet.CreditAmt = ((1 - Math.Sign(PtdCalculated)) / 2) * Math.Abs(PtdCalculated);

							if (existing == null)
							{
								histdet = TransHistGraph.TranslHistDetRecords.Insert(histdet);
							}
							else
							{
								histdet = TransHistGraph.TranslHistDetRecords.Update(histdet);
							}

							if (totalAmount.ContainsKey(histdet.BranchID))
							{
								totalAmount[histdet.BranchID] += histdet.DebitAmt - histdet.CreditAmt;
							}
							else
							{
								totalAmount[histdet.BranchID] = histdet.DebitAmt - histdet.CreditAmt;
							}
						}
					}
				}
			}

			foreach (KeyValuePair<int?, decimal?> pair in totalAmount)
			{
				if (Math.Abs((decimal)pair.Value) > 0)
				{
					TranslationHistoryDetails histdet = new TranslationHistoryDetails();
					if (Math.Sign((decimal)pair.Value) == 1)
					{
						histdet.AccountID = dcurr.TranslationGainAcctID;
						histdet.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.translationGainSubID>(TransHistGraph, pair.Key, dcurr);
					}
					else
					{
						histdet.AccountID = dcurr.TranslationLossAcctID;
						histdet.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.translationLossSubID>(TransHistGraph, pair.Key, dcurr);
					}

					histdet.BranchID = pair.Key;
					histdet.FinPeriodID = companyPeriodID;
					histdet.CalcMode = 0;
					histdet.CuryID = dLedger.BaseCuryID;
					histdet.CuryEffDate = parameters.CuryEffDate;
					histdet.CuryRate = (decimal)1.0;
					histdet.RateTypeID = cmsetup.GLRateTypeDflt;
					histdet.LineNbr = 0;
					histdet.LineType = "G";
					histdet.DebitAmt = ((1 - Math.Sign((decimal)pair.Value)) / 2) * Math.Abs((decimal)pair.Value);
					histdet.CreditAmt = ((1 + Math.Sign((decimal)pair.Value)) / 2) * Math.Abs((decimal)pair.Value);
					histdet.ReferenceNbr = hist.ReferenceNbr;
					histdet = TransHistGraph.TranslHistDetRecords.Insert(histdet);
				}
			}
			if (!TransHistGraph.TranslHistDetRecords.Cache.IsInsertedUpdatedDeleted)
			{
				throw new PXException(Messages.ThereAreNoTransactionsMade);
			}
			else
			{
				hist.ControlTot = hist.CreditTot;
				TransHistGraph.TranslHistRecords.Update(hist);
				TransHistGraph.Save.Press();

				using (new PXTimeStampScope(null))
				{
					TransHistGraph.Clear();
					TransHistGraph.TranslHistRecords.Current = PXSelect<TranslationHistory,
										Where<TranslationHistory.referenceNbr, Equal<Required<TranslationHistory.referenceNbr>>>>.Select(TransHistGraph, hist.ReferenceNbr);
					throw new PXRedirectRequiredException(TransHistGraph, "Translation Record");
				}
			}
		}
		public TranslationProcess()
		{
			CMSetup setup = CMSetup.Current;
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
				throw new Exception(Messages.MultiCurrencyNotActivated);
		}
		#endregion

		#region TranslationParams Events
		protected virtual void TranslationParams_TranslDefID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			TranslationParams row = (TranslationParams)e.Row;
			if (row != null && row.TranslDefId != null)

			cache.SetDefaultExt<TranslationParams.sourceLedgerId>(row);
			cache.SetDefaultExt<TranslationParams.destLedgerId>(row);
			cache.SetDefaultExt<TranslationParams.sourceCuryID>(row);
			cache.SetDefaultExt<TranslationParams.destCuryID>(row);
			cache.SetDefaultExt<TranslationParams.description>(row);
			cache.SetDefaultExt<TranslationParams.branchID>(row);
			cache.SetDefaultExt<TranslationParams.finPeriodID>(row);
			cache.SetDefaultExt<TranslationParams.lastFinPeriodID>(row);
		}

		protected virtual void TranslationParams_CuryEffDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			TranslationParams row = (TranslationParams)e.Row;
			if (row == null) return;

			bool useMasterCalendar = (row.BranchID == null);
			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(row.BranchID, useMasterCalendar);
			FinPeriod period = FinPeriodRepository.FindByID(calendarOrganizationID, row.FinPeriodID);

			if (period != null && period.EndDate != null)
			{
				e.NewValue = ((DateTime)period.EndDate).AddDays(-1);
				e.Cancel = true;
			}
		}

		protected virtual void TranslationParams_LastFinPeriodID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			TranslationParams row = (TranslationParams)e.Row;
			if (row == null) return;
			if (row.TranslDefId != null)
			{
				FinPeriod period = PXSelectJoin<FinPeriod,
					InnerJoin<Branch,
						On<FinPeriod.organizationID, Equal<Branch.organizationID>>,
					InnerJoin<TranslationHistory,
						On<TranslationHistory.finPeriodID, Equal<FinPeriod.finPeriodID>,
						And<TranslationHistory.branchID, Equal<Branch.branchID>>>>>,
					Where<TranslationHistory.translDefId, Equal<Required<TranslationParams.translDefId>>>,
					OrderBy<Desc<FinPeriod.finPeriodID>>>.SelectSingleBound(cache.Graph, new object[0], row.TranslDefId);
				if (period != null)
				{
					e.NewValue = period.FinPeriodID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void TranslationParams_FinPeriodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<TranslationParams.curyEffDate>(e.Row);
		}

		protected virtual void TranslationParams_FinPeriodID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			// Disable selector's FieldVerifying
			e.Cancel = true;
		}

		[Obsolete("This handler is not used anymore and will be removed in Acumatica ERP 8.0.")]
		protected virtual void TranslationParams_CuryEffDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{ }

		protected virtual void VerifyCurrencyEffectiveDate(PXCache sender, TranslationParams translationParameters)
		{
			if (translationParameters?.CuryEffDate == null) return;

			DateTime curyEffDate = translationParameters.CuryEffDate.Value;
			
			bool useMasterCalendar = (translationParameters.BranchID == null);
			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(translationParameters.BranchID, useMasterCalendar);
			FinPeriod currentPeriod = FinPeriodRepository.FindByID(calendarOrganizationID, translationParameters.FinPeriodID);

			if (currentPeriod == null) return;

			bool dateBelongsToFinancialPeriod =
				currentPeriod.IsAdjustment == true
				&& curyEffDate == currentPeriod.EndDate.Value.AddDays(-1)
				||
				currentPeriod.IsAdjustment != true
				&& curyEffDate >= currentPeriod.StartDate
				&& curyEffDate < currentPeriod.EndDate;

			if (currentPeriod != null && !dateBelongsToFinancialPeriod)
			{
				sender.RaiseExceptionHandling<TranslationParams.curyEffDate>(
					translationParameters,
					curyEffDate,
					new PXSetPropertyException(Messages.DateNotBelongFinancialPeriod));
			}
		}

		protected virtual void VerifyFinancialPeriodID(PXCache sender, int? branchID, string finPeriodID)
		{
			bool useMasterCalendar = (branchID == null);

			if (useMasterCalendar) return;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(null, branchID, useMasterCalendar);
			FinPeriod finPeriod = FinPeriodRepository.FindByID(calendarOrganizationID, finPeriodID);

			if (finPeriod != null)
			{
				ProcessingResult result = FinPeriodUtils.CanPostToPeriod(finPeriod);
				if (result.HasWarningOrError)
				{
					throw new PXSetPropertyException(result.GeneralMessage);
				}
			}
		}
		protected virtual void VerifyFinancialPeriodID(PXCache sender, TranslationParams translationParameters)
		{
			try
			{
				VerifyFinancialPeriodID(sender, translationParameters.BranchID, translationParameters.FinPeriodID);
			}
			catch (PXException exc)
			{
				sender.RaiseExceptionHandling<TranslationParams.finPeriodID>(
					translationParameters,
					FinPeriodIDFormattingAttribute.FormatForDisplay(translationParameters.FinPeriodID),
					exc);
			}
		}

		protected virtual void TranslDef_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				using (new PXConnectionScope())
				{
					TranslDef row = (TranslDef)e.Row;
					if (row == null) return;
					if (row.SourceLedgerId != null)
					{
						Ledger sLedger = PXSelectReadonly<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(this, row.SourceLedgerId);
						row.SourceCuryID = sLedger?.BaseCuryID;
					}

					if (row.DestLedgerId != null)
					{
						Ledger dLedger = PXSelectReadonly<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(this, row.DestLedgerId);
						row.DestCuryID = dLedger?.BaseCuryID;
					}
				}
			}
		}

		[Obsolete("This handler is not used anymore and will be removed in Acumatica ERP 8.0.")]
		protected virtual void TranslationParams_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{ }

		protected virtual void TranslationParams_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is TranslationParams translationParameters)) return;

			PXUIFieldAttribute.SetError<TranslationParams.curyEffDate>(sender, translationParameters, null);
			VerifyCurrencyEffectiveDate(sender, translationParameters);

			PXUIFieldAttribute.SetError<TranslationParams.finPeriodID>(sender, translationParameters, null);
			VerifyFinancialPeriodID(sender, translationParameters);

			Translate.SetEnabled(
				!PXUIFieldAttribute.GetErrors(sender, translationParameters).Any() 
				&& translationParameters.TranslDefId != null
                && translationParameters.FinPeriodID != null);
		}
		#endregion

		#region CurrencyRate Events
		protected virtual void CurrencyRate_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CurrencyRate rate = (CurrencyRate)e.Row;
			if (rate == null) return;
			cache.AllowDelete = false;
			cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled(cache, rate, false);
			PXUIFieldAttribute.SetEnabled<CurrencyRate.curyRate>(cache, rate, true);
			PXUIFieldAttribute.SetEnabled<CurrencyRate.curyMultDiv>(cache, rate, true);
			PXUIFieldAttribute.SetEnabled<CurrencyRate.rateReciprocal>(cache, rate, true);
		}
		protected virtual void CurrencyRate_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion

	}

	public class TranslationHistoryMaint : PXGraph<TranslationHistoryMaint>
	{
		#region Buttons
		public PXSave<TranslationHistory> Save;
		public PXCancel<TranslationHistory> Cancel;
		public PXInsert<TranslationHistory> Insert;
		public PXDelete<TranslationHistory> Delete;
		public PXFirst<TranslationHistory> First;

		#region Button Previos
		public PXAction<TranslationHistory> previous;
		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		protected virtual IEnumerable Previous(PXAdapter adapter)
		{
			foreach (TranslationHistory trx in (new PXPrevious<TranslationHistory>(this, "Prev")).Press(adapter))
			{
				if (TranslHistRecords.Cache.GetStatus(trx) == PXEntryStatus.Inserted)
				{
					return Last.Press(adapter);
				}
				else
				{
					return new object[] { trx };
				}
			}
			return new object[0];
		}
		#endregion

		#region Button Next
		public PXAction<TranslationHistory> next;
		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		protected virtual IEnumerable Next(PXAdapter adapter)
		{
			foreach (TranslationHistory trx in (new PXNext<TranslationHistory>(this, "Next")).Press(adapter))
			{
				if (TranslHistRecords.Cache.GetStatus(trx) == PXEntryStatus.Inserted)
				{
					return First.Press(adapter);
				}
				else
				{
					return new object[] { trx };
				}
			}
			return new object[0];
		}
		#endregion

		public PXLast<TranslationHistory> Last;

		#region Button Release
		public PXAction<TranslationHistory> Release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(TranslationHistory)];
			TranslationHistory header = TranslHistRecords.Current as TranslationHistory;

			if (header == null || header.Status != "U")
				return adapter.Get();

			if (PXLongOperation.Exists(UID))
			{
				throw new PXException(GL.Messages.PrevOperationNotCompleteYet);
			}

			Save.Press();

			var graph = CreateInstance<TranslationHistoryMaint>();
			PXLongOperation.StartOperation(this, delegate() { graph.CreateBatch(header, true); });

			return adapter.Get();
		}
		#endregion

		#region ViewBatch
		public PXAction<TranslationHistory> viewBatch;
		[PXUIField(DisplayName = Messages.ViewTranslationBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			if (TranslHistRecords.Current != null && TranslHistRecords.Current.BatchNbr != null)
			{
				JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
				graph.BatchModule.Current = graph.BatchModule.Search<Batch.batchNbr>(TranslHistRecords.Current.BatchNbr, GL.BatchModule.CM);
				throw new PXRedirectRequiredException(graph, Messages.ViewTranslationBatch);
			}
			return adapter.Get();
		}
		#endregion
		#endregion

		#region Variables
		public PXSelect<TranslationHistory> TranslHistRecords;
		public PXProcessing<TranslationHistoryDetails> TranslHistDetRecords;
		public PXSetup<CMSetup> TranslationSetup;
		public PXSetup<GLSetup> GLSetup;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }
		#endregion

		#region SelectExecute
		protected virtual IEnumerable translHistDetRecords()
		{
			KeyValuePair<PXErrorLevel, string>?[] statuses = PXLongOperation.GetCustomInfo(this.UID) as KeyValuePair<PXErrorLevel, string>?[];
			int i = 0;

			foreach (TranslationHistoryDetails histDet in PXSelect<TranslationHistoryDetails,
															Where<TranslationHistoryDetails.referenceNbr, Equal<Current<TranslationHistory.referenceNbr>>>,
															OrderBy<Asc<TranslationHistoryDetails.lineNbr>>>.Select(this))
			{
				if (statuses != null && i < statuses.Length && statuses[i] != null)
				{
					TranslHistDetRecords.Cache.RaiseExceptionHandling<TranslationHistoryDetails.accountID>(histDet, histDet.AccountID, new PXSetPropertyException(statuses[i].Value.Value, statuses[i].Value.Key));
				}
				i++;
				yield return histDet;
			}
		}
		#endregion

		#region Functions
		public TranslationHistoryMaint()
		{
			CMSetup setup = TranslationSetup.Current;

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
			{
				throw new Exception(Messages.MultiCurrencyNotActivated);
			}

			TranslHistRecords.Cache.AllowInsert = false;

			TranslHistDetRecords.Cache.AllowInsert = false;
			TranslHistDetRecords.Cache.AllowDelete = true;

			Release.SetEnabled(false);

			this.reportsFolder.MenuAutoOpen = true;
			this.reportsFolder.AddMenuAction(this.translationDetailsReport);
		}

		protected virtual void UpdateGain(PXCache sender, int? BranchID, decimal? diff)
		{
			TranslationHistoryDetails det = PXSelect<TranslationHistoryDetails, Where<TranslationHistoryDetails.referenceNbr, Equal<Current<TranslationHistory.referenceNbr>>, And<TranslationHistoryDetails.lineType, Equal<TranslationLineType.gainLoss>, And<TranslationHistoryDetails.branchID, Equal<Required<TranslationHistoryDetails.branchID>>>>>>.Select(sender.Graph, BranchID);

			if (det != null && diff != 0m)
			{
				det = PXCache<TranslationHistoryDetails>.CreateCopy(det);

				if (det.DebitAmt > 0)
				{
					det.DebitAmt -= diff;
				}
				else
				{
					det.CreditAmt += diff;
				}

				if (det.DebitAmt < 0)
				{
					det.CreditAmt = -1m * det.DebitAmt;
				}
				else if (det.CreditAmt < 0)
				{
					det.DebitAmt = -1m * det.CreditAmt;
				}

				TranslHistDetRecords.Update(det);
			}
		}
		public virtual bool UpdateControlTotal(PXCache cache, TranslationHistory hist)
		{
			bool requireTotal = (GLSetup.Current.RequireControlTotal == true);
			bool ret = true;
			if (requireTotal != true)
			{
				if (hist.CreditTot != null && hist.CreditTot != 0)
					cache.SetValue<TranslationHistory.controlTot>(hist, hist.CreditTot);
				else if (hist.DebitTot != null && hist.DebitTot != 0)
					cache.SetValue<TranslationHistory.controlTot>(hist, hist.DebitTot);
				else
					cache.SetValue<TranslationHistory.controlTot>(hist, 0m);
			}
			else
			{
				if ((hist.DebitTot != 0 || hist.CreditTot != 0) && hist.CreditTot == 0)
				{
					cache.RaiseExceptionHandling<TranslationHistory.controlTot>(hist, hist.ControlTot, new PXSetPropertyException(Messages.TranslationHistoryIsOutOfBalance));
					ret = false;
				}
				else
				{
					cache.RaiseExceptionHandling<TranslationHistory.controlTot>(hist, hist.ControlTot, null);
				}

				if (hist.CreditTot != hist.ControlTot)
				{
					cache.RaiseExceptionHandling<TranslationHistory.controlTot>(hist, hist.ControlTot, new PXSetPropertyException(Messages.TranslationHistoryIsOutOfBalance));
					ret = false;
				}
				else
				{
					cache.RaiseExceptionHandling<TranslationHistory.controlTot>(hist, hist.ControlTot, null);
				}
				if (hist.DebitTot != hist.ControlTot)
				{
					cache.RaiseExceptionHandling<TranslationHistory.controlTot>(hist, hist.ControlTot, new PXSetPropertyException(Messages.TranslationHistoryIsOutOfBalance));
					ret = false;
				}
				else
				{
					cache.RaiseExceptionHandling<TranslationHistory.controlTot>(hist, hist.ControlTot, null);
				}
			}
			return ret;
		}

		public void CreateBatch(TranslationHistory thist, bool setError)
		{
			TranslationHistoryMaint grapth = PXGraph.CreateInstance<TranslationHistoryMaint>();
			grapth.Clear();

			if (thist.Released == true) return;
			TranslationHistory previosHistory = (TranslationHistory)PXSelect<TranslationHistory,
																Where<TranslationHistory.finPeriodID, LessEqual<Required<TranslationHistory.finPeriodID>>,
																  And<TranslationHistory.ledgerID, Equal<Required<TranslationHistory.ledgerID>>,
																  And<TranslationHistory.released, Equal<boolFalse>,
																  And<TranslationHistory.referenceNbr, NotEqual<Required<TranslationHistory.referenceNbr>>>>>>,
																OrderBy<Desc<TranslationHistory.finPeriodID>>>
										.Select(grapth, thist.FinPeriodID, thist.LedgerID, thist.ReferenceNbr);
			
			if (previosHistory != null)
			{
				throw new PXException(Messages.TranslationOnPreviosPeriodNotReleased);
			}
			List<TranslationHistoryDetails> tHistDetList = new List<TranslationHistoryDetails>();
			foreach (TranslationHistoryDetails histDet in PXSelect<TranslationHistoryDetails,
															Where<TranslationHistoryDetails.referenceNbr, Equal<Required<TranslationHistory.referenceNbr>>>,
															OrderBy<Asc<TranslationHistoryDetails.lineNbr>>>.Select(grapth, thist.ReferenceNbr))
			{
				tHistDetList.Add(histDet);
			}
			if (tHistDetList.Count == 0)
				throw new Exception(Messages.NothingSelected);
			CreateBatchByHistDet(thist, tHistDetList, setError);
		}

		public virtual void CreateBatchByHistDet(TranslationHistory thist, List<TranslationHistoryDetails> thistDetList, bool setError)
		{
			TranslationHistoryMaint grapth = PXGraph.CreateInstance<TranslationHistoryMaint>();
			grapth.Clear();
			if (setError == true)
				PXLongOperation.SetCustomInfo(new KeyValuePair<PXErrorLevel, string>?[thistDetList.Count]);
			CMSetup cmSetup = PXSelect<CMSetup>.Select(grapth);

			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			je.Clear();

			Ledger ledger = PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(grapth, thist.LedgerID);

			CurrencyInfo info = new CurrencyInfo();
			info.CuryID = ledger.BaseCuryID;
			info.BaseCuryID = ledger.BaseCuryID;
			info.CuryEffDate = thist.CuryEffDate;
			info.CuryRateTypeID = null;
			info.CuryRate = (decimal)1.0;
			info = je.currencyinfo.Insert(info);

			Batch batch = new Batch();

		    batch.BranchID = thist.BranchID;
            batch.LedgerID = ledger.LedgerID;
			batch.Module = "CM";
			batch.Status = "U";
			batch.Released = true;
			batch.Hold = false;
			batch.CuryID = ledger.BaseCuryID;
			batch.CuryInfoID = info.CuryInfoID;
			batch.DateEntered = thist.DateEntered;
		    batch.FinPeriodID = thist.FinPeriodID;
            batch.CuryID = ledger.BaseCuryID;

			var openPeriodAttr = je.Caches<Batch>()
				.GetAttributes<Batch.finPeriodID>()
				.OfType<OpenPeriodAttribute>()
				.Single();

			//call validation clearly, 
			//because after insert into cache FinPeriod will be silently replaced by OpenPeriodAttr, if error occur 
			openPeriodAttr.IsValidPeriod(je.Caches<Batch>(), batch, batch.FinPeriodID);

			batch = je.BatchModule.Insert(batch);

			bool needRelease = true;
			Exception externalException = null;

			FinPeriodUtils.ValidateFinPeriod(thistDetList.Where(x => x.Released != true));

			for (int i = 0; i < thistDetList.Count; i++)
			{
				TranslationHistoryDetails thistdet = thistDetList[i];

				if (thistdet.Released == true) continue;

				try
				{
					CurrencyInfo detinfo = new CurrencyInfo();
					detinfo.CuryID = ledger.BaseCuryID;
					detinfo.BaseCuryID = ledger.BaseCuryID;
					detinfo.CuryEffDate = thistdet.CuryEffDate;
					detinfo.CuryRateTypeID = thistdet.RateTypeID;
					detinfo.CuryMultDiv = thistdet.CuryMultDiv;
					detinfo.CuryRate = (decimal)1.0;
					detinfo = je.currencyinfo.Insert(detinfo);

					GLTran tran = new GLTran();
					tran.BranchID = thistdet.BranchID;
					tran.AccountID = thistdet.AccountID;
					tran.SubID = thistdet.SubID;
					tran.CuryInfoID = detinfo.CuryInfoID;
					tran.CuryCreditAmt = thistdet.CreditAmt;
					tran.CuryDebitAmt = thistdet.DebitAmt;
					tran.CreditAmt = thistdet.CreditAmt;
					tran.DebitAmt = thistdet.DebitAmt;
					tran.TranType = "TRN";
					tran.TranClass = thistdet.LineType;
					tran.TranDate = thist.DateEntered;
					tran.TranDesc = thist.Description;
					tran.RefNbr = thistdet.ReferenceNbr;
					tran = je.GLTranModuleBatNbr.Insert(tran);
					if (tran != null)
					{
						thistdet.LineNbr = tran.LineNbr;
						grapth.TranslHistDetRecords.Update(thistdet);
					}
				}
				catch (Exception e)
				{
					if (setError == true)
						PXProcessing<TranslationHistoryDetails>.SetError(i, e);
					needRelease = false;
					externalException = e;
				}
			}

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (needRelease == true)
				{
					je.Save.Press();
					for (int i = 0; i < thistDetList.Count; i++)
					{
						TranslationHistoryDetails thistdet = thistDetList[i];
						try
						{
							thistdet.BatchNbr = batch.BatchNbr;
							thistdet.Released = true;
							grapth.TranslHistDetRecords.Update(thistdet);
							if (setError == true)
								PXProcessing<TranslationHistoryDetails>.SetInfo(i, Messages.GLTranSuccessfullyCreated);
						}
						catch (Exception e)
						{
							if (setError == true)
								PXProcessing<TranslationHistoryDetails>.SetError(i, e);
							needRelease = false;
							externalException = e;
						}
					}
					if (needRelease == true)
					{
						thist.Released = true;
						thist.Status = "R";
						thist.BatchNbr = batch.BatchNbr;
						grapth.TranslHistRecords.Update(thist);
						grapth.Save.Press();
					}
				}
				else
				{
					throw externalException;
				}
				ts.Complete();
			}

			if (needRelease == true)
			{
				if (cmSetup.AutoPostOption == true)
				{
					PostGraph pg = PXGraph.CreateInstance<PostGraph>();
					pg.TimeStamp = batch.tstamp;
					pg.PostBatchProc(batch);
				}
			}
		}

		
		#endregion

		#region TranslationHistory Events
		protected virtual void TranslationHistory_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			TranslationHistory hist = (TranslationHistory)e.Row;
			if (hist == null) return;

			PXUIFieldAttribute.SetEnabled<TranslationHistory.controlTot>(cache, hist, hist.Status != TranslationStatus.Released);
			switch (hist.Status)
			{
				case "U":
					Release.SetEnabled(UpdateControlTotal(cache, hist));
					Save.SetEnabled(true);
					Delete.SetEnabled(true);
					break;
				case "R":
				case "V":
					Release.SetEnabled(false);
					Save.SetEnabled(false);
					Delete.SetEnabled(false);
					break;
				case "H":
					Release.SetEnabled(false);
					Save.SetEnabled(true);
					Delete.SetEnabled(true);
					break;
			}
			bool allowUpdate = (hist.Status == "U" && !PXLongOperation.Exists(this.UID));
			TranslHistDetRecords.Cache.AllowUpdate = allowUpdate;
			if (allowUpdate)
			{
				PXUIFieldAttribute.SetEnabled<TranslationHistoryDetails.creditAmt>(TranslHistDetRecords.Cache, null);
				PXUIFieldAttribute.SetEnabled<TranslationHistoryDetails.debitAmt>(TranslHistDetRecords.Cache, null);
			}

			translationDetailsReport.SetEnabled(!string.IsNullOrEmpty(hist.ReferenceNbr));
		}

		protected virtual void TranslationHistory_LedgerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			TranslationHistory hist = (TranslationHistory)e.Row;
			if (hist != null)
			{
				if (hist.LedgerID != null)
				{
					Ledger dLedger = PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(this, hist.LedgerID);
					hist.DestCuryID = dLedger.BaseCuryID;
				}
			}
		}
		protected virtual void TranslationHistory_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			TranslationHistory hist = (TranslationHistory)e.Row;
			switch (e.Operation & PXDBOperation.Command)
			{
				case PXDBOperation.Delete:
					if (hist.Status != "U")
					{
						throw new PXException(PX.Objects.CM.Messages.TransactionCanNotBeDeleted);
					}
					break;
				case PXDBOperation.Insert:
					if (this.Accessinfo.ScreenID == "CM.30.40.00")
						e.Cancel = true;
					break;
			}
		}

		protected virtual void TranslationHistory_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			TranslationHistory hist = (TranslationHistory)e.Row;
			UpdateControlTotal(cache, hist);
		}
		#endregion

		#region TranslationHistoryDetails Events
		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		protected virtual void TranslationHistoryDetails_DebitAmt_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			TranslationHistoryDetails row = (TranslationHistoryDetails)e.Row;
			if (this.Accessinfo.ScreenID == "CM.30.40.00")
				row.CreditAmt = (decimal)0.0;
		}

		protected virtual void TranslationHistoryDetails_CreditAmt_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			TranslationHistoryDetails row = (TranslationHistoryDetails)e.Row;
			if (this.Accessinfo.ScreenID == "CM.30.40.00")
				row.DebitAmt = (decimal)0.0;
		}

		protected virtual void TranslationHistoryDetails_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			TranslationHistoryDetails hist = (TranslationHistoryDetails)e.Row;
			TranslationHistoryDetails oldHist = (TranslationHistoryDetails)e.OldRow;
			if (hist.LineType != TranslationLineType.GainLoss)
			{
				decimal? diff = hist.DebitAmt - hist.CreditAmt - oldHist.DebitAmt + oldHist.CreditAmt;
				UpdateGain(sender, hist.BranchID, diff);
			}
		}

		protected virtual void TranslationHistoryDetails_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			TranslationHistoryDetails oldHist = (TranslationHistoryDetails)e.Row;
			if (oldHist.LineType != TranslationLineType.GainLoss)
			{
				decimal? diff = -oldHist.DebitAmt + oldHist.CreditAmt;
				UpdateGain(sender, oldHist.BranchID, diff);
			}
		}
		#endregion

		#region Actions

		public PXAction<TranslationHistory> reportsFolder;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Reportsfolder(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<TranslationHistory> translationDetailsReport;
		[PXUIField(DisplayName = Messages.TranslationDetailsReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable TranslationDetailsReport(PXAdapter adapter)
		{
			if (TranslHistRecords.Current != null && TranslHistRecords.Current.ReferenceNbr != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["ReferenceNbr"] = TranslHistRecords.Current.ReferenceNbr;
				parameters["PeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(TranslHistRecords.Current.FinPeriodID);

				throw new PXReportRequiredException(parameters, "CM651500", Messages.TranslationDetailsReport);
			}
			return adapter.Get();
		}

		#endregion
	}
}
