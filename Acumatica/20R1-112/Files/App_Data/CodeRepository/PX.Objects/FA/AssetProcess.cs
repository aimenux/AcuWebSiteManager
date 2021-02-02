using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using PX.Common;
using PX.Data;
using PX.Objects.FA.Overrides.AssetProcess;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.Common.Extensions;
using PX.Objects.GL.Helpers;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.FA
{
	[Obsolete(Common.InternalMessages.ClassIsObsoleteRemoveInAcumatica2019R1)]
	public class PXMassProcessException : Common.PXMassProcessException
	{
		public PXMassProcessException(int ListIndex, Exception InnerException)
			: base(ListIndex, InnerException)
		{ }

		public PXMassProcessException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
	}

	[Serializable]
	public class AssetTranRelease : PXGraph<AssetTranRelease>
	{
		public PXCancel<ReleaseFilter> Cancel;
		public PXFilter<ReleaseFilter> Filter;
		[PXFilterable]
		[PX.SM.PXViewDetailsButton(typeof(FARegister.refNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
		public PXFilteredProcessing<FARegister, ReleaseFilter, Where<True, Equal<True>>, OrderBy<Desc<FARegister.selected, Asc<FARegister.finPeriodID>>>> FADocumentList;
		public PXSelect<FATran> Trans;
		public PXSetup<FASetup> fasetup;

		protected virtual void FARegister_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				using (new PXConnectionScope())
				{
					PXFormulaAttribute.CalcAggregate<FATran.tranAmt>(Trans.Cache, e.Row, true);
				}
			}
		}

		public IEnumerable fADocumentList(PXAdapter adapter)
		{
			ReleaseFilter filter = Filter.Current;

			PXSelectBase<FARegister> cmd = new PXSelect<FARegister, Where<FARegister.released, Equal<False>, And<FARegister.hold, Equal<False>>>>(this);
			if (filter.Origin != null)
			{
				cmd.WhereAnd<Where<FARegister.origin, Equal<Current<ReleaseFilter.origin>>>>();
			}
			return cmd.Select();
		}

		public AssetTranRelease()
		{
			if(fasetup.Current.UpdateGL != true)
			{
				throw new PXSetupNotEnteredException<FASetup>(Messages.OperationNotWorkInInitMode, PXUIFieldAttribute.GetDisplayName<FASetup.updateGL>(fasetup.Cache));
			}

			FADocumentList.SetProcessDelegate(list => ReleaseDoc(list, true));
			FADocumentList.SetProcessCaption(Messages.Release);
			FADocumentList.SetProcessAllCaption(Messages.ReleaseAll);
		}

		public static DocumentList<Batch> ReleaseDoc(List<FARegister> list, bool isMassProcess)
		{
			return ReleaseDoc(list, isMassProcess, true);
		}

		public static DocumentList<Batch> ReleaseDoc(List<FARegister> list, bool isMassProcess, bool AutoPost)
		{
			bool failed = false;

			AssetProcess rg = CreateInstance<AssetProcess>();
			JournalEntry je = CreateInstance<JournalEntry>();
			PostGraph pg = CreateInstance<PostGraph>();

			List<int> batchbind = new List<int>();

			DocumentList<Batch> batchlist = new DocumentList<Batch>(rg);

			//list.Sort((a, b) => string.CompareOrdinal(a.FinPeriodID, b.FinPeriodID));

			for (int i = 0; i < list.Count; i++)
			{
				FARegister doc = list[i];
				try
				{
					rg.Clear();
					
					rg.ProcessAssetTran(je, doc, batchlist);

					if (je.BatchModule.Current != null && batchlist.Find(je.BatchModule.Current) == null)
					{
						batchlist.Add(je.BatchModule.Current);
						batchbind.Add(i);
					}

					if (isMassProcess)
					{
						PXProcessing<FARegister>.SetInfo(i, ActionsMessages.RecordProcessed);
					}
				}
				catch (Exception e)
				{
					batchlist.Remove(je.BatchModule.Current);
					je.Clear();
					if (isMassProcess)
					{
						PXProcessing<FARegister>.SetError(i, e);
						failed = true;
					}
					else
					{
						throw new PXMassProcessException(i, e);
					}
				}
			}

			for (int i = 0; i < batchlist.Count; i++)
			{
				Batch batch = batchlist[i];
				try
				{
					if (rg.AutoPost && AutoPost)
					{
						pg.Clear();
						pg.PostBatchProc(batch);
					}
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						failed = true;
						PXProcessing<FARegister>.SetError(batchbind[i], e);
					}
					else
					{
						throw new PXMassProcessException(batchbind[i], e);
					}
				}
			}
			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
			return rg.AutoPost ? batchlist : new DocumentList<Batch>(rg);
		}

		[Serializable]
		public partial class ReleaseFilter:IBqlTable
		{
			#region Origin
			public abstract class origin : PX.Data.BQL.BqlString.Field<origin> { }
			protected String _Origin;
			[PXDBString(1, IsFixed = true)]
			[FARegister.origin.List]
			[PXUIField(DisplayName = "Origin")]
			public virtual String Origin
			{
				get
				{
					return _Origin;
				}
				set
				{
					_Origin = value;
				}
			}
			#endregion
		}
	}

	public class AssetProcess : PXGraph<AssetProcess>
	{
		public PXSelect<FABookHist> bookhist;
		public PXSelect<FARegister> register;
		public PXSelect<FATran> booktran;
		public PXSelect<FixedAsset> fixedasset;
		public PXSelect<FABookBalance> bookbalance;
		public PXSelect<FADetails> fadetail;
		public PXSelect<FAAccrualTran> accrualtran;
		public PXSetup<FASetup> fasetup;
		public PXSetup<GLSetup> glsetup;

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }
		[InjectDependency]
		public IFABookPeriodUtils FABookPeriodUtils { get; set; }
		
		public JournalEntry je = PXGraph.CreateInstance<JournalEntry>();

		public bool UpdateGL
		{
			get
			{
				return fasetup.Current.UpdateGL == true;
			}
		}

		public bool AutoPost
		{
			get
			{
				return fasetup.Current.AutoPost == true;
			}
		}

		public bool SummPost
		{
			get
			{
				return fasetup.Current.SummPost == true;
			}
		}

		public bool SummPostDepr
		{
			get
			{
				return fasetup.Current.SummPostDepreciation == true;
			}
		}

		public AssetProcess()
		{
			FASetup record = fasetup.Select();
		}

		public override void Clear()
		{
			base.Clear();
			je.Clear();
		}

		#region Repository methods

		public static FixedAsset GetSourceForNewAccounts(PXGraph graph, FixedAsset asset)
		{
			int? classID = asset.AssetID;

			if (asset.OldClassID != null && asset.OldClassID != asset.ClassID)
			{
				classID = asset.ClassID;
			}
			else
			{
				var details = PXSelect<FADetails,
					Where<FADetails.assetID, Equal<Current<FixedAsset.assetID>>>>
					.SelectSingleBound(graph, new object[] { asset })
					.RowCast<FADetails>().FirstOrDefault();

				var location = FAHelper.GetCurrentLocation(graph, details);
				var prevLocation = FAHelper.GetPrevLocation(graph, location);
				if (prevLocation != null && prevLocation.ClassID != asset.ClassID)
				{
					classID = asset.ClassID;
				}
			}

			FixedAsset faclass = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(graph, classID);
			if (faclass == null)
			{
				throw new PXException(ErrorMessages.ValueDoesntExist, Messages.AssetClass, asset.ClassID);
			}

			return faclass;
		}

		#endregion


		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		protected virtual void _(Events.CacheAttached<FATran.tranPeriodID> e)
		{
		}

		public static void SuspendAsset(FixedAsset asset)
		{
			TransactionEntry docgraph = PXGraph.CreateInstance<TransactionEntry>();

			FADetails assetdet = PXSelect<FADetails, Where<FADetails.assetID, Equal<Required<FADetails.assetID>>>>.Select(docgraph, asset.AssetID);
			assetdet.Status = FixedAssetStatus.Suspended;

			docgraph.assetdetails.Update(assetdet);

			asset.Status = FixedAssetStatus.Suspended;
			asset.Suspended = true;

			docgraph.Asset.Update(asset);

			foreach (FABookBalance item in PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectMultiBound(docgraph, new object[] { asset })
				.RowCast<FABookBalance>()
				.Where(item => item.Status != FixedAssetStatus.FullyDepreciated))
				{
					if (item.CurrDeprPeriod == null)
					{
						throw new PXException(Messages.CurrentDeprPeriodIsNull);
					}

					item.Status = FixedAssetStatus.Suspended;

					docgraph.bookbalances.Update(item);
				}
			docgraph.Actions.PressSave();
		}

		public static void UnsuspendAsset(FixedAsset asset, string CurrentPeriod)
		{
			TransactionEntry docgraph = CreateInstance<TransactionEntry>();

			FADetails assetdet = PXSelect<FADetails, Where<FADetails.assetID, Equal<Required<FADetails.assetID>>>>.Select(docgraph, asset.AssetID);
			assetdet.Status = FixedAssetStatus.Active;

			docgraph.assetdetails.Update(assetdet);

			asset.Status = FixedAssetStatus.Active;
			asset.Suspended = false;

			docgraph.Asset.Update(asset);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				docgraph.Actions.PressSave();

				OrganizationFinPeriod currentperiod = docgraph
					.FinPeriodUtils
					.GetOpenOrganizationFinPeriodInSubledger<OrganizationFinPeriod.fAClosed>(CurrentPeriod, asset.BranchID);

				foreach (FABookBalance bookBalance in PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectMultiBound(docgraph, new object[] { asset })
					.RowCast<FABookBalance>()
					.Where(item => item.Status == FixedAssetStatus.Suspended))
					{
						bookBalance.Status = FixedAssetStatus.Active;

						if (bookBalance.CurrDeprPeriod == null)
						{
							throw new PXException(Messages.CurrentDeprPeriodIsNull);
						}

						string CurrentBookPeriod = bookBalance.UpdateGL == true ? CurrentPeriod : docgraph.GetService<IFABookPeriodRepository>().GetFABookPeriodIDOfDate(currentperiod.StartDate, bookBalance.BookID, bookBalance.AssetID);

					if (string.Equals(CurrentBookPeriod, bookBalance.CurrDeprPeriod))
						{
							docgraph.bookbalances.Update(bookBalance);
							docgraph.Actions.PressSave();
						}
						else
						{
							int? periods = docgraph.GetService<IFABookPeriodUtils>().PeriodMinusPeriod(CurrentBookPeriod, bookBalance.CurrDeprPeriod, bookBalance.BookID, bookBalance.AssetID);

							if (periods == null)
							{
								throw new PXException(Messages.CannotChangeCurrentPeriod, FABookPeriodIDAttribute.FormatForError(CurrentBookPeriod), FABookPeriodIDAttribute.FormatForError(bookBalance.CurrDeprPeriod));
							}

							List<FABookBalance> balances = new List<FABookBalance>();
							balances.Add(bookBalance);
							SuspendAssetForPeriods(balances, (int)periods);
						}
					}
				ts.Complete();
			}
		}

		public static void SuspendBalanceForPeriods(TransactionEntry graph, FABookBalance bookbal, int Periods)
		{
			if (Periods < 1)
			{
				return;
			}

			IFABookPeriodUtils fABookPeriodUtils = graph.GetService<IFABookPeriodUtils>();

			FABookHist hist = graph.bookhistory.Insert(new FABookHist
			{
				AssetID = bookbal.AssetID,
				BookID = bookbal.BookID,
				FinPeriodID = fABookPeriodUtils.PeriodPlusPeriodsCount(bookbal.CurrDeprPeriod, -(int) bookbal.YtdSuspended, bookbal.BookID, bookbal.AssetID)
			});
			hist.YtdSuspended = Periods;

			for (int i = 0; i < Periods; i++)
			{
				hist = graph.bookhistory.Insert(new FABookHist
				{
					AssetID = bookbal.AssetID,
					BookID = bookbal.BookID,
					FinPeriodID = fABookPeriodUtils.PeriodPlusPeriodsCount(bookbal.CurrDeprPeriod, i, bookbal.BookID, bookbal.AssetID)
				});
				hist.YtdReversed = (i == 0) ? Periods : 0;
				hist.Suspended = true;
				hist.Closed = true;
			}

			graph.bookhistory.Insert(new FABookHist
			{
				AssetID = bookbal.AssetID,
				BookID = bookbal.BookID,
				FinPeriodID = fABookPeriodUtils.PeriodPlusPeriodsCount(bookbal.CurrDeprPeriod, Periods, bookbal.BookID, bookbal.AssetID)
			});

			FABookBalance copy = PXCache<FABookBalance>.CreateCopy(bookbal);
			if (!string.IsNullOrEmpty(copy.CurrDeprPeriod))
			{
				copy.CurrDeprPeriod = fABookPeriodUtils.PeriodPlusPeriodsCount(copy.CurrDeprPeriod, Periods, copy.BookID, copy.AssetID);
			}
			if (!string.IsNullOrEmpty(copy.LastDeprPeriod))
			{
				copy.LastDeprPeriod = fABookPeriodUtils.PeriodPlusPeriodsCount(copy.LastDeprPeriod, Periods, copy.BookID, copy.AssetID);
			}
			if (!string.IsNullOrEmpty(copy.DeprToPeriod))
			{
				copy.DeprToPeriod = fABookPeriodUtils.PeriodPlusPeriodsCount(copy.DeprToPeriod, Periods, copy.BookID, copy.AssetID);
				copy.LastPeriod = copy.DeprToPeriod;
			}
			copy.DeprToDate = null;

			graph.bookbalances.Update(copy);
		}

		public static void SuspendAssetForPeriods(IEnumerable<FABookBalance> balances, int Periods)
		{
			TransactionEntry docgraph = CreateInstance<TransactionEntry>();

			foreach (FABookBalance bookbal in balances)
			{
				SuspendBalanceForPeriods(docgraph, bookbal, Periods);
			}

			docgraph.Actions.PressSave();
		}

		public static void SuspendAssetToPeriod(IEnumerable<FABookBalance> balances, DateTime? Date, string PeriodID)
		{
			TransactionEntry docgraph = CreateInstance<TransactionEntry>();
			
			foreach (FABookBalance bookbal in balances)
			{
				// open period by assetID
				OrganizationFinPeriod periodTo = docgraph
					.FinPeriodUtils
					.GetOpenOrganizationFinPeriodInFA(PeriodID, bookbal.AssetID);

				Date = Date ?? periodTo.StartDate;

				if (bookbal.LastDeprPeriod != null && string.CompareOrdinal(bookbal.LastDeprPeriod, bookbal.DeprToPeriod) >= 0) continue;

				string BookPeriodID = bookbal.UpdateGL == true ? PeriodID : docgraph.GetService<IFABookPeriodRepository>().GetFABookPeriodIDOfDate(Date, bookbal.BookID, bookbal.AssetID);
				int Periods = docgraph.GetService<IFABookPeriodUtils>().PeriodMinusPeriod(BookPeriodID, bookbal.CurrDeprPeriod, bookbal.BookID, bookbal.AssetID) ?? 0;
				SuspendBalanceForPeriods(docgraph, bookbal, Periods);
			}

			docgraph.Actions.PressSave();
		}

		public abstract class HashView<TView, T> : PXView
			where TView : PXView
		{
			public static TView GetView(PXGraph graph)
			{ 
				PXView view;
				string viewname = "_" + typeof(TView).Name + "_";
				if (!graph.Views.TryGetValue(viewname, out view))
				{
					graph.Views[viewname] = view = (PXView)Activator.CreateInstance(typeof(TView), graph);
				}
				return (TView)view;
			}

			protected HashSet<T> _set;

			protected HashView(PXGraph graph, BqlCommand select)
				: base(graph, true, select)
			{
				object[] list = graph.TypedViews.GetView(this.BqlSelect, true).SelectMulti().ToArray();
				Initialize(list);
			}

			protected abstract void Initialize(object[] list);
 
			public virtual bool Contains(T item)
			{
				return _set.Contains(item);
			}

			public override void Clear()
			{
			}
		}

		public class UnreleasedView : HashView<UnreleasedView, int?>
		{
			public UnreleasedView(PXGraph graph)
				: base(graph, new Select4<FATran, Where<FATran.released, Equal<False>>, Aggregate<GroupBy<FATran.assetID, GroupBy<FATran.bookID>>>>())
			{
			}

			protected override void Initialize(object[] list)
			{
				_set = new HashSet<int?>(Array.ConvertAll(list, a => ((FATran)a).AssetID));
			}
		}

		public static bool UnreleasedTransactionsExistsForAsset(PXGraph graph, Int32? assetID)
		{
			//it is to expensive to search for AssetID,BookID combo
			UnreleasedView unreleased = UnreleasedView.GetView(graph);

			return unreleased.Contains(assetID);
		}

		public static void RestrictAdditonDeductionForCalcMethod(PXGraph graph, int? assetID, string method)
		{
			if (BookBalanceWithDeprMethodExistsForAsset(graph, assetID, method))
			{
				FADepreciationMethod deprMethod = PXSelect<
						FADepreciationMethod,
						Where<FADepreciationMethod.depreciationMethod, Equal<Required<FADepreciationMethod.depreciationMethod>>>>
						.SelectSingleBound(graph, null, method);

				throw new PXException(Messages.AdditionsDisabledForCalcMethod,
										PXStringListAttribute.GetLocalizedLabel<FADepreciationMethod.depreciationMethod>(graph.Caches[typeof(FADepreciationMethod)], deprMethod));
			}
		}

		public static bool BookBalanceWithDeprMethodExistsForAsset(PXGraph graph, int? assetID, string method)
		{
			FABookBalance balanceWithDeprMethod = PXSelectJoin<FABookBalance,
				InnerJoin<FADepreciationMethod, On<FADepreciationMethod.methodID, Equal<FABookBalance.depreciationMethodID>>>,
				Where<FABookBalance.assetID, Equal<Required<FixedAsset.assetID>>,
					And<FADepreciationMethod.depreciationMethod, Equal<Required<FADepreciationMethod.depreciationMethod>>>>>
				.SelectSingleBound(graph, null, assetID, method);

			return balanceWithDeprMethod != null;
		}

		public static void CheckUnreleasedTransactions(PXGraph graph, Int32? assetID)
		{
			if (UnreleasedTransactionsExistsForAsset(graph, assetID))
			{
				FixedAsset asset = SelectFrom<FixedAsset>.Where<FixedAsset.assetID.IsEqual<@P.AsInt>>
					.View.ReadOnly.SelectSingleBound(graph, null, assetID);
				throw new PXException(Messages.AssetHasUnreleasedTran, asset.AssetCD);
			}
		}

		public static void CheckUnreleasedTransactions(PXGraph graph, FixedAsset asset)
		{
			UnreleasedView unreleased = UnreleasedView.GetView(graph);
			if (unreleased.Contains(asset.AssetID))
			{
				throw new PXException(Messages.AssetHasUnreleasedTran, asset.AssetCD);
			}
		}

		public static void ThrowDisabled_Dispose(PXGraph graph, FixedAsset asset, FADetails det, FASetup fasetup, DateTime disposalDate, string disposalPeriodID, bool deprBeforeDisposal)
		{
			if (det.ReceiptDate != null && DateTime.Compare((DateTime)det.ReceiptDate, disposalDate) > 0)
			{
				throw new PXException(Messages.AcquisitionAfterDisposal);
			}

			if (det.IsReconciled == false && fasetup.ReconcileBeforeDisposal == true)
			{
				throw new PXException(Messages.CanNotDisposeUnreconciledAsset);
			}

			foreach (FABookBalance bookbal in PXSelectReadonly<FABookBalance, Where<FABookBalance.assetID, Equal<Required<FABookBalance.assetID>>>>.Select(graph, asset.AssetID))
			{
				string disposalPeriod = bookbal.UpdateGL == true ? disposalPeriodID : graph.GetService<IFABookPeriodRepository>().GetFABookPeriodIDOfDate(disposalDate, bookbal.BookID, bookbal.AssetID);

				CheckUnreleasedTransactions(graph, bookbal.AssetID);

				if (bookbal.LastDeprPeriod != bookbal.DeprToPeriod && bookbal.Depreciate == true && // live depreciable asset
					string.CompareOrdinal(disposalPeriod, bookbal.CurrDeprPeriod) > 0 && deprBeforeDisposal &&
					fasetup.AutoReleaseDepr != true) 
				{
					throw new PXException(Messages.AssetShouldBeDeprToPeriod, FinPeriodIDAttribute.FormatForError(string.Format("{0}", Math.Min(int.Parse(disposalPeriod), int.Parse(bookbal.DeprToPeriod)))));
				}
				if (string.CompareOrdinal(bookbal.LastDeprPeriod, disposalPeriod) > 0)
				{
					throw new PXException(Messages.AssetDisposedInPastPeriod);
				}
			}
		}

		public static void ThrowDisabled_Transfer(PXGraph graph, FixedAsset asset, FADetailsTransfer det)
		{
			CheckUnreleasedTransactions(graph, asset);

			if (string.IsNullOrEmpty(det.TransferPeriodID))
			{
				FABookBalance bookbal = PXSelectReadonly<FABookBalance, Where<FABookBalance.assetID, Equal<Required<FABookBalance.assetID>>>, OrderBy<Desc<FABookBalance.updateGL>>>.SelectWindowed(graph, 0, 1, asset.AssetID);
				if (bookbal != null && string.IsNullOrEmpty(bookbal.LastDeprPeriod) == false && bookbal.UpdateGL == true)
				{
					throw new PXException(GL.Messages.NoOpenPeriod);
				}
			}

		}

		protected static Dictionary<string, string> reversalTranType = new Dictionary<string, string>() 
		{ 
			{FATran.tranType.PurchasingPlus, FATran.tranType.PurchasingMinus},
			{FATran.tranType.PurchasingMinus, FATran.tranType.PurchasingPlus},
			{FATran.tranType.DepreciationPlus, FATran.tranType.DepreciationMinus},
			{FATran.tranType.DepreciationMinus, FATran.tranType.DepreciationPlus},
			{FATran.tranType.ReconciliationPlus, FATran.tranType.ReconciliationMinus},
			{FATran.tranType.ReconciliationMinus, FATran.tranType.ReconciliationPlus},
			{FATran.tranType.SalePlus, FATran.tranType.SaleMinus},
			{FATran.tranType.SaleMinus, FATran.tranType.SalePlus},
			{FATran.tranType.PurchasingReversal, FATran.tranType.PurchasingDisposal},
			{FATran.tranType.PurchasingDisposal, FATran.tranType.PurchasingReversal},
			{FATran.tranType.AdjustingDeprPlus, FATran.tranType.AdjustingDeprMinus},
			{FATran.tranType.AdjustingDeprMinus, FATran.tranType.AdjustingDeprPlus},
			{FATran.tranType.TransferPurchasing, FATran.tranType.TransferPurchasing},
			{FATran.tranType.TransferDepreciation, FATran.tranType.TransferDepreciation},
		};

		protected static Dictionary<string, string> disposalreversalTranType = new Dictionary<string, string>() 
		{ 
			{FATran.tranType.PurchasingDisposal, FATran.tranType.PurchasingReversal},
			{FATran.tranType.AdjustingDeprMinus, FATran.tranType.AdjustingDeprPlus},
			{FATran.tranType.SalePlus, FATran.tranType.SaleMinus},
			{FATran.tranType.DepreciationPlus, FATran.tranType.DepreciationMinus},
			{FATran.tranType.DepreciationMinus, FATran.tranType.DepreciationPlus},
		};

		public static DocumentList<FARegister> ReverseDisposal(FixedAsset asset, DateTime? revDate, string revPeriodID)
		{
			TransactionEntry docgraph = CreateInstance<TransactionEntry>();
			FARegister doc = docgraph.Document.Insert(new FARegister()
														 {
															 BranchID = asset.BranchID,
															 Origin = FARegister.origin.DisposalReversal,
															 DocDesc = PXMessages.LocalizeFormatNoPrefix(Messages.DocDescDispReversal, asset.AssetCD)
														 });

			FARegister disposalRegister = GetDisposalRegister(docgraph, asset.AssetID);
			foreach (PXResult<FATran, FABook> res in PXSelectJoin<
				FATran,
				LeftJoin<FABook,
					On<FATran.bookID, Equal<FABook.bookID>>>,
				Where<FATran.refNbr, Equal<Current<FARegister.refNbr>>>>
				.SelectMultiBound(docgraph, new object[] { disposalRegister }))
			{
				FATran tran = res;
				FABook book = res;
				revPeriodID = revPeriodID ?? tran.FinPeriodID;
				if (book.UpdateGL != true)
			{
					revPeriodID = docgraph.GetService<IFABookPeriodRepository>().FindFABookPeriodOfDateByBranchID(revDate ?? tran.TranDate, tran.BookID, tran.BranchID).FinPeriodID;
				}
				FATran copy = new FATran
				{
					RefNbr = doc.RefNbr,
					LineNbr = null,
					DebitAccountID = tran.CreditAccountID,
					DebitSubID = tran.CreditSubID,
					CreditAccountID = tran.DebitAccountID,
					CreditSubID = tran.DebitSubID,
					TranDesc = docgraph.MakeLocalizedDescription<FATran.tranDesc>(Messages.TranDescDispReversal, tran.TranDesc),
					Released = false,
					TranAmt = tran.TranAmt,
					RGOLAmt = tran.RGOLAmt,
					AssetID = tran.AssetID,
					BookID = tran.BookID,
					TranType = disposalreversalTranType[tran.TranType],
					TranDate = revDate ?? tran.TranDate,
					FinPeriodID = revPeriodID ?? tran.FinPeriodID,
				};
				copy = docgraph.Trans.Insert(copy);
			}
			if (docgraph.Trans.Cache.IsInsertedUpdatedDeleted)
			{
				docgraph.Actions.PressSave();
			}
			if (docgraph.fasetup.Current.AutoReleaseReversal == true)
			{
				PXLongOperation.StartOperation(docgraph, delegate { AssetTranRelease.ReleaseDoc(new List<FARegister> { doc }, false); });
			}
			return docgraph.created;
		}

		public static DocumentList<FARegister> ReverseAsset(FixedAsset asset, FASetup fasetup)
		{
			TransactionEntry docgraph = CreateInstance<TransactionEntry>();
			FARegister doc = docgraph.Document.Insert(new FARegister
															{
																BranchID = asset.BranchID,
																Origin = FARegister.origin.Reversal, 
																DocDesc = PXMessages.LocalizeFormatNoPrefix(Messages.DocDescReversal, asset.AssetCD)
															});
			foreach (FATran copy in PXSelect<FATran, Where<FATran.assetID, Equal<Current<FixedAsset.assetID>>, And<FATran.released, Equal<True>>>>
				.SelectMultiBound(docgraph, new object[]{asset})
				.RowCast<FATran>()
				.Select(tran => new FATran
								  {
									  RefNbr = doc.RefNbr,
									  LineNbr = null,
									  DebitAccountID = tran.CreditAccountID,
									  DebitSubID = tran.CreditSubID,
									  CreditAccountID = tran.DebitAccountID,
									  CreditSubID = tran.DebitSubID,
									  TranDesc = docgraph.MakeLocalizedDescription<FATran.tranDesc>(Messages.TranDescReversal, tran.TranDesc),
									  Released = false,
									  TranAmt = tran.TranAmt, 
									  RGOLAmt = tran.TranAmt,
									  AssetID = tran.AssetID,
									  BookID = tran.BookID,
									  FinPeriodID = tran.FinPeriodID,
									  TranType = reversalTranType[tran.TranType],
									  GLTranID = tran.GLTranID,
									  BranchID = tran.IsTransfer ? tran.SrcBranchID : tran.BranchID,
									  SrcBranchID = tran.IsTransfer ? tran.BranchID : tran.SrcBranchID,
								  }))
			{
				docgraph.Trans.Insert(copy);
			}
			if (docgraph.Trans.Cache.IsInsertedUpdatedDeleted)
			{
				docgraph.Actions.PressSave();
			}
			if (docgraph.fasetup.Current.AutoReleaseReversal == true)
			{
				PXLongOperation.StartOperation(docgraph, () => AssetTranRelease.ReleaseDoc(new List<FARegister> {doc}, false));
			}
			return docgraph.created;
		}

		public static List<FABookBalance> PrepareDisposal(TransactionEntry docgraph, FixedAsset asset, FADetails assetdet, bool IsMassProcess, bool deprBeforeDisposal)
		{
			PXSelectBase<FABookBalance> bookbalSelect = new PXSelectReadonly<FABookBalance,
				Where<FABookBalance.assetID, Equal<Required<FixedAsset.assetID>>>>(docgraph);

			List<FABookBalance> books = new List<FABookBalance>();
			string disposalPeriod = assetdet.DisposalPeriodID ??
									docgraph.FinPeriodRepository.FindFinPeriodByDate(assetdet.DisposalDate, PXAccess.GetParentOrganizationID(asset.BranchID))?.FinPeriodID;
			
			foreach (FABookBalance bookbal in bookbalSelect.Select(assetdet.AssetID))
			{
				if (bookbal.Depreciate != true || !deprBeforeDisposal)
				{
					SuspendAssetToPeriod(new List<FABookBalance>() { bookbal }, assetdet.DisposalDate, disposalPeriod);
				}

				//all previous reads should be readonly!! otherwise SuspendAsset changes would not be read.
				FABookBalance copy = PXSelect<FABookBalance, 
					Where<FABookBalance.assetID, Equal<Current<FABookBalance.assetID>>, 
						And<FABookBalance.bookID, Equal<Current<FABookBalance.bookID>>>>>
					.SelectSingleBound(docgraph, new object[] { bookbal });

				copy.DisposalPeriodID = (bookbal.UpdateGL == true && assetdet.DisposalPeriodID != null) 
					? assetdet.DisposalPeriodID 
					: docgraph.GetService<IFABookPeriodRepository>().GetFABookPeriodIDOfDate(assetdet.DisposalDate, copy.BookID, copy.AssetID);
				copy.OrigDeprToDate = copy.DeprToDate;
				copy.DisposalAmount = assetdet.SaleAmount;

				books.Add(docgraph.bookbalances.Update(copy));
			}

			docgraph.Actions.PressSave();
			docgraph.Clear();

			if (asset.Depreciable == true && deprBeforeDisposal)
			{
				DepreciateAsset(books, assetdet.DisposalDate, disposalPeriod, IsMassProcess, false);

				/// Rows should be refreshed, because some fields 
				/// can be affected by the depreciate process.
				/// Note, that non db fields should be filled again.
				/// 
				books = bookbalSelect.Select(assetdet.AssetID)
					.RowCast<FABookBalance>()
					.ToList();
				books.ForEach(bal => bal.DisposalAmount = assetdet.SaleAmount);
			}

			Dictionary<int?, FABookBalance> origin = SetDeprTo(books, assetdet.DisposalDate);
			CalculateAsset(books, null);
			ResetDeprTo(books, origin);

			return books;
		}

		protected static Dictionary<int?, FABookBalance> SetDeprTo(List<FABookBalance> books, DateTime? deprToDate)
		{
			Dictionary<int?, FABookBalance> origin = new Dictionary<int?, FABookBalance>();
			foreach (FABookBalance balance in books.Where(balance => balance != null && balance.BookID != null && string.CompareOrdinal(balance.DeprToPeriod, balance.DisposalPeriodID) > 0))
			{
				origin.Add(balance.BookID, new FABookBalance{DeprToDate = balance.DeprToDate, DeprToPeriod = balance.DeprToPeriod});
				balance.DeprToDate = deprToDate;
				balance.DeprToPeriod = balance.DisposalPeriodID;
			}
			return origin;
		}

		protected static void ResetDeprTo(List<FABookBalance> books, Dictionary<int?, FABookBalance> origin)
		{
			foreach (FABookBalance balance in books)
			{
				FABookBalance orig;
				if (origin.TryGetValue(balance.BookID, out orig))
				{
					balance.DeprToDate = orig.DeprToDate;
					balance.DeprToPeriod = orig.DeprToPeriod;
				}
			}
		}

		public static DocumentList<FARegister> DisposeAsset(PXResultset<FixedAsset, FADetails> assets, FASetup fasetup, bool IsMassProcess, bool deprBeforeDisposal, string reason)
		{
			TransactionEntry docgraph = CreateInstance<TransactionEntry>();
			foreach (PXResult<FixedAsset, FADetails> res in assets)
			{
				FixedAsset asset = res;
				FADetails assetdet = res;

				PXProcessing<FixedAsset>.SetCurrentItem(asset);
				try
				{
					ThrowDisabled_Dispose(docgraph, asset, assetdet, fasetup, (DateTime)assetdet.DisposalDate, assetdet.DisposalPeriodID, deprBeforeDisposal);

					List<FABookBalance> books = PrepareDisposal(docgraph, asset, assetdet, IsMassProcess, deprBeforeDisposal);
					DisposeAsset(docgraph, asset, books, fasetup, reason);

					PXProcessing<FixedAsset>.SetProcessed();
				}
				catch (Exception ex)
				{
					if (IsMassProcess)
					{
						PXProcessing<FixedAsset>.SetError(ex);
					}
					else
					{
						throw;
					}
				}
			}
			
			if (docgraph.fasetup.Current.AutoReleaseDisp == true && docgraph.created.Count > 0)
			{
				AssetTranRelease.ReleaseDoc(docgraph.created, IsMassProcess);
			}
			return docgraph.created;
		}

		public static void DisposeAsset(TransactionEntry docgraph, FixedAsset asset, IEnumerable<FABookBalance> books, FASetup setup, string reason)
		{
			FADetails det = PXSelect<FADetails, Where<FADetails.assetID, Equal<Current<FixedAsset.assetID>>>>.SelectSingleBound(docgraph, new object[] { asset });
			if(setup.SummPost == true)
			{
				TransactionEntry.SegregateRegister(docgraph, (int)asset.BranchID, FARegister.origin.Disposal, null, det.DisposalDate, "", docgraph.created);
			}
			else
			{
				FARegister doc = docgraph.Document.Insert(new FARegister() 
																{ 
																	Origin = FARegister.origin.Disposal, 
																	DocDate = det.DisposalDate, 
																	BranchID = asset.BranchID
																});
			}

			FARegister currdoc = docgraph.Document.Current;
			if(currdoc != null && string.IsNullOrEmpty(currdoc.Reason))
			{
				currdoc.Reason = reason;
				docgraph.Document.Update(currdoc);
			}
		   
			foreach (FABookBalance bal in books)
			{
				FABookBalance bookbal = PXSelect<
					FABookBalance, 
					Where<FABookBalance.assetID, Equal<Current<FABookBalance.assetID>>, 
						And<FABookBalance.bookID, Equal<Current<FABookBalance.bookID>>>>>
					.SelectSingleBound(docgraph, new object[] { bal });

				FABookPeriod bookperiod = PXSelect<
					FABookPeriod, 
					Where<FABookPeriod.finPeriodID, Equal<Required<FABookBalance.deprToPeriod>>,
						And<FABookPeriod.bookID, Equal<Required<FABookBalance.bookID>>,
						And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>>>>>
					.SelectSingleBound(
						docgraph, 
						new object[] { },
						bookbal.DeprToPeriod,
						bookbal.BookID,
						docgraph.GetService<IFABookPeriodRepository>().GetFABookPeriodOrganizationID(bookbal));

				if (bookbal.DeprToDate == null && bookperiod != null && bookperiod.EndDate != null)
				{
					bookbal.DeprToDate = bookperiod.EndDate.Value.AddDays(-1);
				}
				bookbal.DisposalAmount = bal.DisposalAmount;

				string period;
				if(bookbal.Depreciate == true)
				{
					period = string.CompareOrdinal(bookbal.DeprToPeriod, bookbal.DisposalPeriodID) > 0
						? bookbal.DisposalPeriodID
						: bookbal.DeprToPeriod;
					if (!string.IsNullOrEmpty(bookbal.LastPeriod) &&
						string.CompareOrdinal(bookbal.LastPeriod, bookbal.DeprToPeriod) > 0 &&
						string.CompareOrdinal(bookbal.LastPeriod, period) > 0)
					{
						period = bookbal.LastPeriod;
					}
				}
				else
				{
					period = bookbal.DisposalPeriodID;
				}
				FABookHistory hist = PXSelect<FABookHistory, Where<FABookHistory.assetID, Equal<Current<FABookBalance.assetID>>, And<FABookHistory.bookID, Equal<Current<FABookBalance.bookID>>, And<FABookHistory.finPeriodID, Equal<Required<FABookBalance.deprToPeriod>>>>>>.SelectSingleBound(docgraph, new object[]{bookbal}, period);

				if (hist == null)
				{
					throw new PXException();
				}
				if (string.CompareOrdinal(bookbal.LastDeprPeriod, bookbal.DisposalPeriodID) > 0)
				{
					throw new PXException(Messages.AssetDisposedInPastPeriod);
				}
				if (hist.Suspended == true)
				{
					throw new PXException(Messages.AssetDisposedInSuspendedPeriod);
				}

				{
					FATran tran = new FATran
					{
						AssetID = bookbal.AssetID,
						BookID = bookbal.BookID,
						TranDate = det.DisposalDate,
						FinPeriodID = bookbal.DisposalPeriodID,
						TranType = FATran.tranType.AdjustingDeprMinus,
						TranAmt = (hist.YtdTax179 - hist.YtdTax179Taken),
						MethodDesc = Messages.MethodDescTax179
					};
					tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescDepreciationRecap, docgraph.Trans.GetValueExt<FATran.assetID>(tran));
					docgraph.Trans.Insert(tran);

					hist.YtdCalculated -= tran.TranAmt;
					hist.YtdDepreciated -= tran.TranAmt;
				}

				{
					FATran tran = new FATran
					{
						AssetID = bookbal.AssetID,
						BookID = bookbal.BookID,
						TranDate = det.DisposalDate,
						FinPeriodID = bookbal.DisposalPeriodID,
						TranType = FATran.tranType.AdjustingDeprMinus,
						TranAmt = (hist.YtdBonus - hist.YtdBonusTaken),
						MethodDesc = Messages.MethodDescBonus
					};
					tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescDepreciationRecap, docgraph.Trans.GetValueExt<FATran.assetID>(tran));
					docgraph.Trans.Insert(tran);

					hist.YtdCalculated -= tran.TranAmt;
					hist.YtdDepreciated -= tran.TranAmt;
				}


				int? GainLossAcctID;
				int? GainLossSubID;

				decimal? RGOLAmt;
				if (setup.DepreciateInDisposalPeriod == true && bookbal.Status == FixedAssetStatus.Active)
				{
					RGOLAmt = bookbal.DisposalAmount - (hist.YtdAcquired - (bookbal.Depreciate == true ? hist.YtdCalculated : 0m));
				}
				else
				{
					RGOLAmt = bookbal.DisposalAmount - (hist.YtdAcquired - (bookbal.Depreciate == true ? hist.YtdDepreciated : 0m));
				}
					

				if (RGOLAmt > 0m)
				{
					GainLossAcctID = asset.GainAcctID;
					GainLossSubID = asset.GainSubID;
				}
				else
				{
					GainLossAcctID = asset.LossAcctID;
					GainLossSubID = asset.LossSubID;
				}

				#region Sect 179 Recap


				#endregion

				decimal? tranAmt = hist.YtdCalculated - hist.YtdDepreciated;
				bool isPositive = tranAmt > 0m;
				tranAmt = Math.Abs((decimal)tranAmt);
				if (setup.DepreciateInDisposalPeriod == true 
					&& bookbal.Status == FixedAssetStatus.Active 
					&& bookbal.Depreciate == true 
					&& tranAmt != 0m)
				{
					FATran tran = new FATran
					{
						AssetID = hist.AssetID,
						BookID = hist.BookID,
						TranDate = det.DisposalDate,
						FinPeriodID = bookbal.DisposalPeriodID,
						TranAmt = tranAmt
					};
					tran.TranType = isPositive
										? (bookbal.CurrDeprPeriod == tran.FinPeriodID
											   ? FATran.tranType.CalculatedPlus
											   : FATran.tranType.DepreciationPlus)
										: (bookbal.CurrDeprPeriod == tran.FinPeriodID
											   ? FATran.tranType.CalculatedMinus
											   : FATran.tranType.DepreciationMinus);
					tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescDisposalAdj, docgraph.Trans.GetValueExt<FATran.assetID>(tran));

					docgraph.Trans.Insert(tran);
				}

				{
					FATran tran = new FATran
					{
						AssetID = bookbal.AssetID,
						BookID = bookbal.BookID,
						TranDate = det.DisposalDate,
						FinPeriodID = bookbal.DisposalPeriodID,
						TranType = FATran.tranType.PurchasingDisposal,
						CreditAccountID = asset.FAAccountID,
						CreditSubID = asset.FASubID,
						DebitAccountID = GainLossAcctID,
						DebitSubID = GainLossSubID,
						TranAmt = hist.YtdAcquired
					};
					tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescCostDisposal, docgraph.Trans.GetValueExt<FATran.assetID>(tran));

					docgraph.Trans.Insert(tran);
				}

				#region NewZealand methods
				//Revert part of Depreciation for NewZealand methods
				decimal? reverseTranSum = 0;

				if (bookbal.Depreciate == true)
				{
					FADepreciationMethod DeprMethod = PXSelect<
						FADepreciationMethod,
						Where<FADepreciationMethod.methodID, Equal<Required<FADepreciationMethod.methodID>>>>
						.SelectSingleBound(docgraph, null, bookbal.DepreciationMethodID);
					if (DeprMethod.IsNewZealandMethod)
					{
						reverseTranSum = NewZelandAdditionDisposal(docgraph, asset, det, bookbal);
						RGOLAmt -= reverseTranSum;
					}
				}
				#endregion

				if (bookbal.Depreciate == true)
				{
					FATran tran = new FATran
					{
						AssetID = bookbal.AssetID,
						BookID = bookbal.BookID,
						TranDate = det.DisposalDate,
						FinPeriodID = bookbal.DisposalPeriodID,
						TranType = FATran.tranType.AdjustingDeprMinus,
						DebitAccountID = asset.AccumulatedDepreciationAccountID,
						DebitSubID = asset.AccumulatedDepreciationSubID,
						ReclassificationOnDebitProhibited = true,
						CreditAccountID = GainLossAcctID,
						CreditSubID = GainLossSubID,
						TranAmt = (docgraph.fasetup.Current.DepreciateInDisposalPeriod == true 
							&& bookbal.Status == FixedAssetStatus.Active
							? hist.YtdCalculated 
							: hist.YtdDepreciated) - reverseTranSum
					};
					tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescDeprDisposal, docgraph.Trans.GetValueExt<FATran.assetID>(tran));

					docgraph.Trans.Insert(tran);
				}

				{
					FATran tran = new FATran
					{
						AssetID = bookbal.AssetID,
						BookID = bookbal.BookID,
						TranDate = det.DisposalDate,
						FinPeriodID = bookbal.DisposalPeriodID,
						TranType = FATran.tranType.SalePlus
					};

					if (bookbal.DisposalAmount >= 0m)
					{
						tran.DebitAccountID = asset.DisposalAccountID;
						tran.DebitSubID = asset.DisposalSubID;
						tran.CreditAccountID = GainLossAcctID;
						tran.CreditSubID = GainLossSubID;
					}
					else
					{
						tran.DebitAccountID = GainLossAcctID;
						tran.DebitSubID = GainLossSubID;
						tran.CreditAccountID = asset.DisposalAccountID;
						tran.CreditSubID = asset.DisposalSubID;
					}

					tran.TranAmt = Math.Abs((decimal)bookbal.DisposalAmount);
					tran.RGOLAmt = RGOLAmt;
					tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescSale, docgraph.Trans.GetValueExt<FATran.assetID>(tran));

					docgraph.Trans.Insert(tran);
				}

			}

			if (docgraph.Trans.Cache.IsInsertedUpdatedDeleted)
			{
				docgraph.Actions.PressSave();
			}
		}

		public static decimal? NewZelandAdditionDisposal(TransactionEntry docgraph, FixedAsset asset, FADetails det, FABookBalance bookbal)
		{
			string firstFinPeriodIDOfYear = GL.FinPeriods.FinPeriodUtils.GetFirstFinPeriodIDOfYear(bookbal.DisposalPeriodID);
			decimal? reverseTranSum = 0;

			List<FATran> result = new List<FATran>();

			foreach (FATran transaction in SelectFrom<FATran>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.bookID.IsEqual<@P.AsInt>>
					.And<FATran.finPeriodID.IsGreaterEqual<@P.AsString>>
					.And<FATran.tranType.IsEqual<FATran.tranType.depreciationPlus>
						.Or<FATran.tranType.IsEqual<FATran.tranType.depreciationMinus>
						.Or<FATran.tranType.IsEqual<FATran.tranType.calculatedPlus>
						.Or<FATran.tranType.IsEqual<FATran.tranType.calculatedMinus>>>>>>
				.View
				.Select(docgraph, asset.AssetID, bookbal.BookID, firstFinPeriodIDOfYear))
			{
				if (transaction.TranType == FATran.tranType.DepreciationMinus
					|| transaction.TranType == FATran.tranType.CalculatedMinus)
				{
					//reverse tran to D+ or C+ state for aggregating
					int? tempCreditAccountID = transaction.CreditAccountID;
					int? tempCreditSubID = transaction.CreditSubID;
					transaction.CreditAccountID = transaction.DebitAccountID;
					transaction.CreditSubID = transaction.DebitSubID;
					transaction.DebitAccountID = tempCreditAccountID;
					transaction.DebitSubID = tempCreditSubID;
					transaction.TranAmt = -transaction.TranAmt;
				}
				result.Add(transaction);
			}

			var aggregatedResult = result.GroupBy(x => new { x.CreditAccountID, x.CreditSubID, x.DebitAccountID, x.DebitSubID })
			.Select(g => new
			{
				g.Key.CreditAccountID,
				g.Key.CreditSubID,
				g.Key.DebitAccountID,
				g.Key.DebitSubID,
				tranAmt = g.Sum(x => x.TranAmt)
			});

			foreach (var item in aggregatedResult)
			{
				FATran reverseTran = new FATran
				{
					AssetID = bookbal.AssetID,
					BookID = bookbal.BookID,
					TranDate = det.DisposalDate,
					FinPeriodID = bookbal.DisposalPeriodID,
					DebitAccountID = item.CreditAccountID,
					DebitSubID = item.CreditSubID,
					CreditAccountID = item.DebitAccountID,
					CreditSubID = item.DebitSubID,

					TranAmt = item.tranAmt
				};
				if (bookbal.CurrDeprPeriod == reverseTran.FinPeriodID)
					reverseTran.TranType = "C-";
				else
				{
					reverseTran.TranType = "D-";
				}

				reverseTran.TranDesc = docgraph.MakeLocalizedDescription<FATran.tranDesc>(Messages.TranDescReversal, (string)docgraph.Trans.GetValueExt<FATran.assetID>(reverseTran));
				reverseTranSum += item.tranAmt;
				docgraph.Trans.Insert(reverseTran);
			}

			return reverseTranSum;
		}

		public static DocumentList<FARegister> SplitAsset(FixedAsset asset, DateTime? splitDate, string splitPeriodID, bool deprBeforeSplit, Dictionary<FixedAsset, decimal> splits)
		{
			TransactionEntry docgraph = CreateInstance<TransactionEntry>();
			OrganizationFinPeriod splitPeriod = docgraph
				.FinPeriodUtils
				.GetOpenOrganizationFinPeriodInSubledger<OrganizationFinPeriod.fAClosed>(splitPeriodID, asset.BranchID);
			splitDate = splitDate ?? splitPeriod.StartDate;

			FARegister doc = docgraph.Document.Insert(new FARegister
															{ 
																BranchID = asset.BranchID,
																Origin = FARegister.origin.Split, 
																DocDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescSplit, asset.AssetCD) 
															});
			string desc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescSplit, asset.AssetCD);
			foreach (KeyValuePair<FixedAsset, decimal> split in splits)
			{
				foreach (FABookBalance sbal in PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(docgraph, split.Key.AssetID))
				{
					FABookBalance bal = PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Required<FixedAsset.assetID>>, And<FABookBalance.bookID, Equal<Required<FABook.bookID>>>>>.Select(docgraph, asset.AssetID, sbal.BookID);
					string splitPeriodIDByDate = docgraph.GetService<IFABookPeriodRepository>().GetFABookPeriodIDOfDate(splitDate, bal.BookID, bal.AssetID);
					string tranPeriodID = bal.UpdateGL == true ? splitPeriod.FinPeriodID : splitPeriodIDByDate;

					if(deprBeforeSplit)
					{
						DepreciateAsset(new List<FABookBalance>() { bal }, splitDate, tranPeriodID, false, false);
					}
					else
					{
						SuspendAssetToPeriod(new List<FABookBalance>() { bal }, splitDate, tranPeriodID);
						CalculateAsset(new List<FABookBalance>() { bal }, null);
					}

					// reselect after suspend/depreciation
					bal = PXSelectReadonly<FABookBalance, Where<FABookBalance.assetID, Equal<Required<FixedAsset.assetID>>, And<FABookBalance.bookID, Equal<Required<FABook.bookID>>>>>.Select(docgraph, asset.AssetID, sbal.BookID);
					sbal.CurrDeprPeriod = bal.CurrDeprPeriod;
					sbal.LastDeprPeriod = bal.LastDeprPeriod;
					sbal.LastPeriod = bal.LastPeriod;
					sbal.DeprToPeriod = bal.DeprToPeriod;
					docgraph.bookbalances.Update(sbal);

					int? prev_ytdsuspened = 0;
					int? prev_ytdreversed = 0;
					decimal? rounding = sbal.AcquisitionCost;

					FATran prev_tran = null;
					FATran prev_tran2 = null;

					string lastPeriod = null;
					if (string.IsNullOrEmpty(bal.DeprToPeriod))
					{
						lastPeriod = PXSelect<FABookHistory, 
							Where<FABookHistory.assetID, Equal<Required<FABookBalance.assetID>>, 
								And<FABookHistory.bookID, Equal<Required<FABookBalance.bookID>>>>, 
							OrderBy<Desc<FABookHistory.finPeriodID>>>
							.Select(docgraph, bal.AssetID, bal.BookID)
							.RowCast<FABookHistory>()
							.FirstOrDefault()
							.With(h => h.FinPeriodID);
					}
					foreach (FABookHist item in PXSelectReadonly<FABookHist, 
						Where<FABookHist.assetID, Equal<Required<FABookHist.assetID>>, 
							And<FABookHist.bookID, Equal<Required<FABookHist.bookID>>, 
							And<FABookHist.finPeriodID, LessEqual<Required<FABookHist.finPeriodID>>>>>, 
						OrderBy<Asc<FABookHist.finPeriodID>>>.Select(docgraph, bal.AssetID, bal.BookID, lastPeriod ?? bal.DeprToPeriod))
					{
						if ((!string.IsNullOrEmpty(bal.LastDeprPeriod) 
								|| item.Suspended == true) 
							&& (string.CompareOrdinal(item.FinPeriodID, bal.CurrDeprPeriod) <= 0
								|| bal.Status == FixedAssetStatus.FullyDepreciated))
						{
							FABookHist newhist = docgraph.bookhistory.Insert(new FABookHist
							{
								AssetID = sbal.AssetID,
								BookID = sbal.BookID,
								FinPeriodID = item.FinPeriodID
							});

							newhist.Suspended = item.Suspended;
							newhist.Closed = item.Closed;
							newhist.YtdSuspended = item.YtdSuspended;
							newhist.YtdReversed = item.YtdReversed;

							newhist.YtdSuspended -= prev_ytdsuspened;
							newhist.YtdReversed -= prev_ytdreversed;

							prev_ytdsuspened = item.YtdSuspended;
							prev_ytdreversed = item.YtdReversed;
						}

						if (item.PtdDeprBase != 0m)
						{
							string purchasingTranPeriodID = tranPeriodID;
							if (!string.IsNullOrEmpty(bal.DeprToPeriod)
								&& string.CompareOrdinal(purchasingTranPeriodID, bal.DeprToPeriod) > 0)
							{
								purchasingTranPeriodID = bal.DeprToPeriod;
							}

							FATran tran = new FATran
							{
								AssetID = bal.AssetID,
								BookID = bal.BookID,
								TranType = FATran.tranType.PurchasingMinus,
								TranDate = splitDate,
								FinPeriodID = purchasingTranPeriodID,
								TranPeriodID = item.FinPeriodID,
								TranAmt = PXCurrencyAttribute.BaseRound(docgraph, item.PtdDeprBase * split.Value / 100m)
							};

							prev_tran = docgraph.Trans.Insert(tran);

							tran = new FATran
							{
								AssetID = sbal.AssetID,
								BookID = sbal.BookID,
								TranType = FATran.tranType.PurchasingPlus,
								TranDate = splitDate,
								FinPeriodID = purchasingTranPeriodID,
								TranPeriodID = item.FinPeriodID,
								TranAmt = PXCurrencyAttribute.BaseRound(docgraph, item.PtdDeprBase * split.Value / 100m),
								TranDesc = desc
							};

							prev_tran2 = docgraph.Trans.Insert(tran);

							rounding -= tran.TranAmt;
						}
					}

					if (rounding != 0m && prev_tran != null && prev_tran2 != null)
					{
						prev_tran.TranAmt += rounding;
						prev_tran2.TranAmt += rounding;
					}

					if (bal.YtdReconciled != null)
					{
					docgraph.Trans.Insert(new FATran
					{
						AssetID = asset.AssetID,
						BookID = bal.BookID,
						TranDate = splitDate,
						FinPeriodID = tranPeriodID,
						TranType = FATran.tranType.ReconciliationMinus,
						TranAmt = PXDBCurrencyAttribute.BaseRound(docgraph, (decimal)(bal.YtdReconciled * split.Value / 100m)),
						TranDesc = desc
					});

						docgraph.Trans.Insert(new FATran
						{
							AssetID = split.Key.AssetID,
							BookID = sbal.BookID,
							TranDate = splitDate,
							FinPeriodID = tranPeriodID,
							TranType = FATran.tranType.ReconciliationPlus,
							TranAmt = PXDBCurrencyAttribute.BaseRound(docgraph, (decimal)(bal.YtdReconciled * split.Value / 100m)),
							TranDesc = desc
						});
					}

					if (bal.YtdDepreciated != null)
					{
						string deprTranPeriodID = tranPeriodID;
						if (string.CompareOrdinal(deprTranPeriodID, bal.DeprFromPeriod) < 0)
						{
							deprTranPeriodID = bal.DeprFromPeriod;
						}
						else if (!string.IsNullOrEmpty(bal.DeprToPeriod) 
							&& string.CompareOrdinal(deprTranPeriodID, bal.DeprToPeriod) > 0)
						{
							deprTranPeriodID = bal.DeprToPeriod;
						}

					docgraph.Trans.Insert(new FATran
					{
						AssetID = asset.AssetID,
						BookID = bal.BookID,
						TranDate = splitDate,
							FinPeriodID = deprTranPeriodID,
						TranType = FATran.tranType.AdjustingDeprMinus,
						TranAmt = PXDBCurrencyAttribute.BaseRound(docgraph, (decimal)(bal.YtdDepreciated * split.Value / 100m)),
						TranDesc = desc,
						CreditAccountID = asset.DepreciatedExpenseAccountID,
						CreditSubID = asset.DepreciatedExpenseSubID,
						ReclassificationOnCreditProhibited = true,
						DebitAccountID = asset.AccumulatedDepreciationAccountID,
						DebitSubID = asset.AccumulatedDepreciationSubID,
						ReclassificationOnDebitProhibited = true
					});

					docgraph.Trans.Insert(new FATran
					{
						AssetID = split.Key.AssetID,
						BookID = sbal.BookID,
						TranDate = splitDate,
							FinPeriodID = deprTranPeriodID,
						TranType = FATran.tranType.AdjustingDeprPlus,
						TranAmt = PXDBCurrencyAttribute.BaseRound(docgraph, (decimal)(bal.YtdDepreciated * split.Value / 100m)),
						TranDesc = desc,
						CreditAccountID = split.Key.AccumulatedDepreciationAccountID,
						CreditSubID = split.Key.AccumulatedDepreciationSubID,
						ReclassificationOnCreditProhibited = true,
						DebitAccountID = split.Key.DepreciatedExpenseAccountID,
						DebitSubID = split.Key.DepreciatedExpenseSubID,
						ReclassificationOnDebitProhibited = true
					});
				}
			}
			}

			DocumentList<Batch> batchlist = new DocumentList<Batch>(docgraph);
			using (PXTransactionScope ts = new PXTransactionScope())
			{

				if (docgraph.Trans.Cache.IsInsertedUpdatedDeleted)
				{
					// Salvage amount updating after all depreciations
					PXUpdate<Set<FADetails.salvageAmount, Required<FADetails.salvageAmount>>,
						FADetails,
						Where<FADetails.assetID.IsEqual<@P.AsInt>>>
						.Update(docgraph, asset.SalvageAmtAfterSplit, asset.AssetID);

					PXUpdate<Set<FABookBalance.salvageAmount, Required<FABookBalance.salvageAmount>>,
						FABookBalance,
						Where<FABookBalance.assetID.IsEqual<@P.AsInt>>>
						.Update(docgraph, asset.SalvageAmtAfterSplit, asset.AssetID);

					docgraph.Actions.PressSave();
				}
				if (docgraph.fasetup.Current.AutoReleaseSplit == true && docgraph.created.Count > 0)
				{
					batchlist = AssetTranRelease.ReleaseDoc(docgraph.created, false, false);
				}
				ts.Complete(docgraph);
			}

			PostGraph pg = CreateInstance<PostGraph>();
			foreach (Batch batch in batchlist)
			{
				pg.Clear();
				pg.PostBatchProc(batch);
			}

			return docgraph.created;
		}

		public static void AcquireAsset(TransactionEntry docgraph, int BranchID, IDictionary<FABookBalance, OperableTran> booktrn, FARegister register)
		{
			if (register != null)
			{
				docgraph.Document.Current = register;

				if (register.BranchID != BranchID)
				{
					FARegister registerCopy = PXCache<FARegister>.CreateCopy(register);

					registerCopy.BranchID = BranchID;

					docgraph.Document.Update(registerCopy);
				}
			}
			else
			{
				TransactionEntry.SegregateRegister(docgraph, BranchID, FARegister.origin.Purchasing, null, null, "", docgraph.created);
				register = docgraph.Document.Current;
			}

			docgraph.Asset.Cache.Clear();
			docgraph.Clear(PXClearOption.ClearQueriesOnly);

			foreach (KeyValuePair<FABookBalance, OperableTran> book in booktrn)
			{
				FixedAsset asset = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(docgraph, book.Key.AssetID);
				FADetails details = PXSelect<FADetails, Where<FADetails.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(docgraph, book.Key.AssetID);

				if (book.Value.Op == PXDBOperation.Update && (!docgraph.UpdateGL || register.Released == false))
				{
					docgraph.Document.Cache.MarkUpdated(register);

					PXSelect<
						FATran,
						Where<FATran.assetID, Equal<Required<FATran.assetID>>,
							And<FATran.bookID, Equal<Required<FATran.bookID>>>>>
						.Select(docgraph, book.Key.AssetID, book.Key.BookID)
						.RowCast<FATran>()
						.ForEach(tran => docgraph.Trans.Delete(tran));

					PXDatabase.Delete<FABookHistory>(
						new PXDataFieldRestrict<FABookHistory.assetID>(book.Key.AssetID), 
						new PXDataFieldRestrict<FABookHistory.bookID>(book.Key.BookID));
					
					book.Value.Op = PXDBOperation.Insert;
					docgraph.Document.Update(docgraph.Document.Current);
				}

				if (string.IsNullOrEmpty(book.Key.LastDeprPeriod))
				{
					if (book.Value.Op == PXDBOperation.Delete || book.Value.Op == PXDBOperation.Update)
					{
						PXDatabase.Delete<FABookHistory>(
							new PXDataFieldRestrict<FABookHistory.assetID>(book.Key.AssetID),
							new PXDataFieldRestrict<FABookHistory.bookID>(book.Key.BookID));
					}
					if ((book.Value.Op == PXDBOperation.Insert && asset.SplittedFrom == null) || book.Value.Op == PXDBOperation.Update)
					{
						FABookHist hist = new FABookHist
						{
							AssetID = book.Key.AssetID,
							BookID = book.Key.BookID,
							FinPeriodID = book.Key.DeprFromPeriod
						};

						hist = docgraph.bookhistory.Insert(hist);

						FABookBalance bookBal = docgraph.bookbalances.SelectSingle(book.Key.AssetID, book.Key.BookID);
						bookBal.CurrDeprPeriod = book.Key.DeprFromPeriod;
						docgraph.bookbalances.Update(bookBal);
					}
				}

				switch (book.Value.Op)
				{
					case PXDBOperation.Insert:
						if(asset.SplittedFrom != null)
						{
							continue;
						}

						FATran purchasingTran = new FATran
						{
							AssetID = book.Key.AssetID,
							BookID = book.Key.BookID,
							TranDate = details.ReceiptDate,
							TranAmt = book.Key.AcquisitionCost,
							TranType = FATran.tranType.PurchasingPlus,
							Released = !docgraph.UpdateGL
						};
						purchasingTran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescPurchase, docgraph.Trans.GetValueExt<FATran.assetID>(purchasingTran));
						purchasingTran = docgraph.Trans.Insert(purchasingTran);
						
						if (!docgraph.UpdateGL)
						{
							FATran depreciationTran = null;

							if (!string.IsNullOrEmpty(book.Key.LastDeprPeriod))
							{
								{
									FATran tran = new FATran
									{
										AssetID = book.Key.AssetID,
										BookID = book.Key.BookID,
										TranDate = book.Key.DeprFromDate,
										FinPeriodID = book.Key.DeprFromPeriod,
										TranAmt = book.Key.Tax179Amount,
										TranType = FATran.tranType.DepreciationPlus,
										MethodDesc = Messages.MethodDescTax179,
										Released = true
									};
									tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescDepreciation,
									docgraph.Trans.GetValueExt<FATran.assetID>(tran));
									docgraph.Trans.Insert(tran);
								}

								{
									FATran tran = new FATran
									{
										AssetID = book.Key.AssetID,
										BookID = book.Key.BookID,
										TranDate = book.Key.DeprFromDate,
										FinPeriodID = book.Key.DeprFromPeriod,
										TranAmt = book.Key.BonusAmount,
										TranType = FATran.tranType.DepreciationPlus,
										MethodDesc = Messages.MethodDescBonus,
										Released = true
									};
									tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescDepreciation,
									docgraph.Trans.GetValueExt<FATran.assetID>(tran));
									docgraph.Trans.Insert(tran);
								}

								{
									FATran tran = new FATran
									{
										AssetID = book.Key.AssetID,
										BookID = book.Key.BookID,
										TranDate = docgraph.GetService<IFABookPeriodUtils>().GetFABookPeriodEndDate(book.Key.LastDeprPeriod, book.Key.BookID, book.Key.AssetID),
										FinPeriodID = book.Key.LastDeprPeriod,
										TranAmt = book.Key.YtdDepreciated - (book.Key.Tax179Amount + book.Key.BonusAmount),
										TranType = FATran.tranType.DepreciationPlus,
										Released = true
									};
									tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescDepreciation,
									docgraph.Trans.GetValueExt<FATran.assetID>(tran));
									depreciationTran = docgraph.Trans.Insert(tran);
								}
							}

							{
								FATran tran = new FATran
								{
									AssetID = book.Key.AssetID,
									BookID = book.Key.BookID,
									TranDate = details.ReceiptDate,
									TranAmt = book.Key.AcquisitionCost,
									TranType = FATran.tranType.ReconciliationPlus,
									Released = true
								};
								docgraph.Trans.Insert(tran);
								
								asset.IsAcquired = true;
								docgraph.Asset.Update(asset);
							}

							docgraph.Document.Current.Released = true;

							{
								FABookHist hist = new FABookHist
								{
									AssetID = purchasingTran.AssetID,
									BookID = purchasingTran.BookID,
									FinPeriodID = purchasingTran.FinPeriodID
								};

								hist = docgraph.bookhistory.Insert(hist);

								hist.PtdAcquired += purchasingTran.TranAmt;
								hist.YtdAcquired += purchasingTran.TranAmt;
								hist.YtdBal += purchasingTran.TranAmt;

								if (docgraph.fasetup.Current.UpdateGL != true)
								{
									hist.PtdReconciled += book.Key.AcquisitionCost;
									hist.YtdReconciled += book.Key.AcquisitionCost;
								}

								hist = new FABookHist
								{
									AssetID = book.Key.AssetID,
									BookID = book.Key.BookID,
									FinPeriodID = book.Key.DeprFromPeriod
								};

								hist = docgraph.bookhistory.Insert(hist);

								hist.PtdDeprBase += book.Key.AcquisitionCost;
								hist.YtdDeprBase += book.Key.AcquisitionCost;
								hist.PtdDepreciated += book.Key.Tax179Amount + book.Key.BonusAmount;
								hist.YtdDepreciated += book.Key.Tax179Amount + book.Key.BonusAmount;

								hist.YtdBal -= book.Key.Tax179Amount + book.Key.BonusAmount;

								hist.PtdTax179 += book.Key.Tax179Amount;
								hist.YtdTax179 += book.Key.Tax179Amount;

								hist.PtdBonus += book.Key.BonusAmount;
								hist.YtdBonus += book.Key.BonusAmount;
								hist.Closed = depreciationTran != null;
							}

							KeyValuePair<FABookBalance, OperableTran> b = book;
							foreach (FABookHist hist in PXSelect<
								FABookPeriod, 
								Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>, 
									And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
									And<FABookPeriod.finPeriodID, Greater<Required<FABookPeriod.finPeriodID>>,
									And<FABookPeriod.finPeriodID, Less<Required<FABookPeriod.finPeriodID>>,
									And<FABookPeriod.endDate, Greater<FABookPeriod.startDate>>>>>>>
								.Select(
									docgraph, 
									book.Key.BookID, 
									docgraph.GetService<IFABookPeriodRepository>().GetFABookPeriodOrganizationID(book.Key), 
									book.Key.DeprFromPeriod, 
									book.Key.LastDeprPeriod)
								.RowCast<FABookPeriod>()
								.Select(per => new FABookHist
									{
										AssetID = b.Key.AssetID,
										BookID = b.Key.BookID,
										FinPeriodID = per.FinPeriodID
									})
								.Select(hist => docgraph.bookhistory.Insert(hist)))
							{
								hist.Closed = true;
							}

							if (!string.IsNullOrEmpty(book.Key.LastDeprPeriod))
							{
								{
									FABookHist hist = new FABookHist
									{
										AssetID = book.Key.AssetID,
										BookID = book.Key.BookID,
										FinPeriodID = book.Key.LastDeprPeriod
									};

									hist = docgraph.bookhistory.Insert(hist);

									hist.PtdDepreciated += book.Key.YtdDepreciated - (book.Key.Tax179Amount + book.Key.BonusAmount);
									hist.YtdDepreciated += book.Key.YtdDepreciated - (book.Key.Tax179Amount + book.Key.BonusAmount);
									hist.YtdBal -= book.Key.YtdDepreciated - (book.Key.Tax179Amount + book.Key.BonusAmount);
									hist.Closed = true;
								}

								if (string.CompareOrdinal(book.Key.LastDeprPeriod, book.Key.DeprToPeriod) < 0)
								{
									FABookHist hist = new FABookHist
									{
										AssetID = book.Key.AssetID,
										BookID = book.Key.BookID,
										FinPeriodID = docgraph.GetService<IFABookPeriodUtils>().PeriodPlusPeriodsCount(book.Key.LastDeprPeriod, 1, book.Key.BookID, book.Key.AssetID)
									};

									hist = docgraph.bookhistory.Insert(hist);

									hist.Closed = false;

									FABookBalance bookBal = docgraph.bookbalances.SelectSingle(book.Key.AssetID, book.Key.BookID);
									bookBal.CurrDeprPeriod = docgraph.GetService<IFABookPeriodUtils>().PeriodPlusPeriodsCount(book.Key.LastDeprPeriod, 1, book.Key.BookID, book.Key.AssetID);
									bookBal.InitPeriod = book.Key.DeprFromPeriod;
									bookBal.LastPeriod = book.Key.DeprToPeriod;
									docgraph.bookbalances.Update(bookBal);
								}

								if (string.CompareOrdinal(book.Key.LastDeprPeriod, book.Key.DeprToPeriod) == 0)
								{
									FABookBalance bookBal = docgraph.bookbalances.SelectSingle(book.Key.AssetID, book.Key.BookID);
									bookBal.Status = FixedAssetStatus.FullyDepreciated;
									bookBal.CurrDeprPeriod = null;
									bookBal.InitPeriod = book.Key.DeprFromPeriod;
									bookBal.LastPeriod = book.Key.DeprToPeriod;
									docgraph.bookbalances.Update(bookBal);

									if (IsFullyDepreciatedAsset(docgraph, bookBal.AssetID))
									{
										details.Status = FixedAssetStatus.FullyDepreciated;
										details.Hold = false;
										docgraph.assetdetails.Update(details);
									}
								}
							}
						}

						break;
					case PXDBOperation.Update:
						FATran copy = PXCache<FATran>.CreateCopy(book.Value.Tran);
						copy.TranDate = details.ReceiptDate;
						docgraph.Trans.Cache.SetDefaultExt<FATran.finPeriodID>(copy);
						copy.TranAmt = book.Key.AcquisitionCost;
						docgraph.Trans.Update(copy);
						break;
					case PXDBOperation.Delete:
						docgraph.Trans.Delete(book.Value.Tran);
						break;
				}
			}

			if (docgraph.Trans.Cache.IsInsertedUpdatedDeleted)
			{
				// docgraph.Actions.PressSave() executes with external call mark and raises unnecessary events
				// (TransactionEntry.FARegister_RowSelected in this case) that lead to error.
				// PXAction.Press() executes without external call mark.
				docgraph.Save.Press();
			}
		}

		public static bool CalculateAsset(IEnumerable<FABookBalance> books, string maxPeriodID)
		{
			bool success = true;

			DepreciationCalculation calc = CreateInstance<DepreciationCalculation>();

			foreach (FABookBalance bookbal in books)
			{
				calc.Clear();
				PXProcessing<FABookBalance>.SetCurrentItem(bookbal);
				try
				{
					calc.Calculate(bookbal, maxPeriodID);

					if (PXProcessing<FABookBalance>.GetItemMessage() == null)
					{
						PXProcessing<FABookBalance>.SetProcessed();
					}
				}
				catch (PXException ex)
				{
					PXProcessing<FABookBalance>.SetError(ex);
					success = false;
				}
			}

			return success;
		}

		public static bool DepreciateAsset(IEnumerable<FABookBalance> books, DateTime? DateTo, string PeriodTo, bool IsMassProcess)
		{
			return DepreciateAsset(books, DateTo, PeriodTo, IsMassProcess, true);
		}

		public static bool DepreciateAsset(IEnumerable<FABookBalance> books, DateTime? DateTo, string PeriodTo, bool IsMassProcess, bool IncludeLastPeriod)
		{
			bool success = true;
			TransactionEntry docgraph = PXGraph.CreateInstance<TransactionEntry>();
			DepreciationCalculation calc = CreateInstance<DepreciationCalculation>();

			foreach (FABookBalance item in books)
			{
				// open period by assetID
				OrganizationFinPeriod periodTo = docgraph
					.FinPeriodUtils
					.GetOpenOrganizationFinPeriodInFA(PeriodTo, item.AssetID);

				PXProcessing<FABookBalance>.SetCurrentItem(item);

				try
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						FABookBalance bookbal = PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Current<FABookBalance.assetID>>, 
							And<FABookBalance.bookID, Equal<Current<FABookBalance.bookID>>>>>.SelectSingleBound(docgraph, new object[] { item });

						calc.Clear();
						calc.Calculate(bookbal, PeriodTo);

						string BookPeriodTo = bookbal.UpdateGL != true && DateTo != null ?
							docgraph.GetService<IFABookPeriodRepository>().GetFABookPeriodIDOfDate(DateTo, bookbal.BookID, bookbal.AssetID) :
							PeriodTo;

						if (!IncludeLastPeriod)
						{
							BookPeriodTo = docgraph.GetService<IFABookPeriodUtils>().PeriodPlusPeriodsCount(BookPeriodTo, -1, bookbal.BookID, bookbal.AssetID);
						}
						
						foreach (PXResult<FABookHistory, FixedAsset, Branch, FABook, FABookPeriod, FADetails> res in PXSelectJoin<
							FABookHistory, 
							InnerJoin<FixedAsset, 
								On<FixedAsset.assetID, Equal<FABookHistory.assetID>>,
							InnerJoin<Branch, 
								On<FixedAsset.branchID, Equal<Branch.branchID>>,
							InnerJoin<FABook,
								On<FABookHistory.bookID, Equal<FABook.bookID>>,
							InnerJoin<FABookPeriod,
								On<FABookPeriod.bookID, Equal<FABookHistory.bookID>,
								And<FABookPeriod.organizationID, Equal<IIf<Where<FABook.updateGL, Equal<True>>, Branch.organizationID, FinPeriod.organizationID.masterValue>>,
								And<FABookPeriod.finPeriodID, Equal<FABookHistory.finPeriodID>>>>, 
							LeftJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>>>>>>,
							Where<FABookHistory.assetID, Equal<Current<FABookBalance.assetID>>,
								And<FABookHistory.bookID, Equal<Current<FABookBalance.bookID>>,
								And<FABookHistory.finPeriodID, GreaterEqual<Required<FABookHistory.finPeriodID>>,
								And<FABookHistory.finPeriodID, LessEqual<Required<FABookHistory.finPeriodID>>,
								And<FABookHistory.closed, NotEqual<True>,
								And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>>>>>>>,
							OrderBy<
								Asc<FABookHistory.finPeriodID>>>
							.SelectMultiBound(docgraph, new object[] { bookbal }, bookbal.CurrDeprPeriod, BookPeriodTo))
						{
							FABookHistory hist = res;
							FABookPeriod period = res;
							FixedAsset asset = res;
							FADetails details = res;

							TransactionEntry.SegregateRegister(docgraph, (int)asset.BranchID, FARegister.origin.Depreciation, period.FinPeriodID, null, null, docgraph.created);

							FATran prev_tran;
							{
								FATran tran = new FATran
								{
									AssetID = hist.AssetID,
									BookID = hist.BookID,
									TranDate = period.EndDate.Value.AddDays(-1),
									FinPeriodID = period.FinPeriodID,
									TranAmt = hist.FinPeriodID == bookbal.CurrDeprPeriod
										? hist.YtdCalculated - hist.YtdDepreciated
										: hist.PtdCalculated + hist.PtdAdjusted + hist.PtdDeprDisposed,
									TranType = FATran.tranType.CalculatedPlus
								};
								tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescDepreciation, asset.AssetCD);
								
								// fill currents to avoid redundant selects in SetCurrentAsset method
								docgraph.Asset.Current = asset;
								docgraph.assetdetails.Current = details;
								prev_tran = docgraph.Trans.Insert(tran);
							}

							if (hist.FinPeriodID == bookbal.DeprFromPeriod && (hist.YtdTax179Calculated - hist.YtdTax179) > 0m)
							{
								prev_tran.TranAmt -= (hist.YtdTax179Calculated - hist.YtdTax179);

								FATran tran = new FATran
								{
									AssetID = hist.AssetID,
									BookID = hist.BookID,
									TranDate = period.EndDate.Value.AddDays(-1),
									FinPeriodID = period.FinPeriodID,
									TranAmt = bookbal.Tax179Amount,
									TranType = FATran.tranType.CalculatedPlus,
									MethodDesc = Messages.MethodDescTax179
								};
								tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescDepreciation, asset.AssetCD);

								// fill currents to avoid redundant selects in SetCurrentAsset method
								docgraph.Asset.Current = asset;
								docgraph.assetdetails.Current = details;
								docgraph.Trans.Insert(tran);
							}

							if (hist.FinPeriodID == bookbal.DeprFromPeriod && (hist.YtdBonusCalculated - hist.YtdBonus) > 0m)
							{
								prev_tran.TranAmt -= (hist.YtdBonusCalculated - hist.YtdBonus);

								FATran tran = new FATran
								{
									AssetID = hist.AssetID,
									BookID = hist.BookID,
									TranDate = period.EndDate.Value.AddDays(-1),
									FinPeriodID = period.FinPeriodID,
									TranAmt = bookbal.BonusAmount,
									TranType = FATran.tranType.CalculatedPlus,
									MethodDesc = Messages.MethodDescBonus
								};
								tran.TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescDepreciation, asset.AssetCD);

								// fill currents to avoid redundant selects in SetCurrentAsset method
								docgraph.Asset.Current = asset;
								docgraph.assetdetails.Current = details;
								docgraph.Trans.Insert(tran);
							}
						}
						if (docgraph.Trans.Cache.IsInsertedUpdatedDeleted)
						{
							docgraph.Actions.PressSave();
							docgraph.Clear(); // prevent OutOfMemoryException, clear cloned attributes
						}
						ts.Complete();
					}

					if (PXProcessing<FABookBalance>.GetItemMessage() == null)
					{
						PXProcessing<FABookBalance>.SetProcessed();
					}
				}
				catch (Exception ex)
				{
					PXProcessing<FABookBalance>.SetError(ex);
					success = false;
				}
			}

			if (docgraph.fasetup.Current.AutoReleaseDepr == true && docgraph.created.Count > 0)
			{
				docgraph.created.Sort((a, b) => string.Compare(a.FinPeriodID, b.FinPeriodID, StringComparison.Ordinal));
				AssetTranRelease.ReleaseDoc(docgraph.created, IsMassProcess); 
			}

			return success;
		}

		public static void SetLastDeprPeriod(PXSelectBase<FABookBalance> bookBalances, FABookBalance bookBal, string LastDeprPeriod)
		{
			if (LastDeprPeriod == null) return;
			LastDeprPeriod = string.CompareOrdinal(LastDeprPeriod, bookBal.DeprToPeriod) > 0 ? bookBal.DeprToPeriod : LastDeprPeriod;
			bookBal = (FABookBalance)bookBalances.Cache.Locate(bookBal) ?? bookBal;
			if (string.CompareOrdinal(bookBal.LastDeprPeriod, LastDeprPeriod) < 0)
			{
				bookBal.LastDeprPeriod = LastDeprPeriod;
				bookBal.CurrDeprPeriod = string.CompareOrdinal(bookBal.LastDeprPeriod, bookBal.DeprToPeriod) < 0 
					? bookBalances.Cache.Graph.GetService<IFABookPeriodUtils>().PeriodPlusPeriodsCount(bookBal.LastDeprPeriod, 1, bookBal.BookID, bookBal.AssetID) 
					: null;
				if(string.CompareOrdinal(bookBal.LastDeprPeriod, bookBal.DeprToPeriod) == 0 && 
					bookBal.CurrDeprPeriod == null)
				{
					bookBal.Status = FixedAssetStatus.FullyDepreciated;
				}
				bookBalances.Update(bookBal);
			}
		}

		public static string GetFixedAssetStatus(PXGraph graph, FADetails details)
		{
			FixedAsset asset = SelectFrom<FixedAsset>
				.Where<FixedAsset.assetID.IsEqual<@P.AsInt>>
				.View
				.Select(graph, details.AssetID);

			if(asset == null)
			{
				throw new PXException(ErrorMessages.ValueDoesntExist, nameof(FADetails.AssetID), details.AssetID);
			}

			string status = FixedAssetStatus.Active;

			if (details.Hold == true)
			{
				status = FixedAssetStatus.Hold;
			}
			else if (asset.Suspended == true)
			{
				status = FixedAssetStatus.Suspended;
			}
			else if (IsFullyDepreciatedAsset(graph, details.AssetID))
			{
				status = FixedAssetStatus.FullyDepreciated;
			}

			return status;
		}

		public static void AdjustFixedAssetStatus(PXGraph graph, int? assetID)
		{
			FADetails details = PXSelect<FADetails, Where<FADetails.assetID, Equal<Required<FADetails.assetID>>>>.Select(graph, assetID);
			if (details == null) return; // FixedAsset and FADetails object already deleted, nothing to adjust a status

			string status = GetFixedAssetStatus(graph, details);
			if (details.Status != status)
			{
				details.Status = status;
				graph.Caches<FADetails>().Update(details);
			}
		}

		public virtual void DataChecking(FARegister doc)
		{
			if (fasetup.Current.UpdateGL != true && doc.Origin != FARegister.origin.Purchasing)
			{
				throw new PXException(Messages.CannotReleaseInInitializeMode);
			}

			foreach (PXResult<FATran, FABook, Branch, FABookPeriod> tranPeriod in
				SelectFrom<FATran>
					.InnerJoin<FABook>.On<FATran.bookID.IsEqual<FABook.bookID>>
					.InnerJoin<Branch>.On<FATran.branchID.IsEqual<Branch.branchID>>
					.InnerJoin<FABookPeriod>.On<FATran.finPeriodID.IsEqual<FABookPeriod.finPeriodID>
						.And<Branch.organizationID.IsEqual<FABookPeriod.organizationID>
						.And<FABook.bookID.IsEqual<FABookPeriod.bookID>>>>
					.Where<FATran.refNbr.IsEqual<@P.AsString>
						.And<FABook.updateGL.IsEqual<True>>>
					.AggregateTo<GroupBy<FABookPeriod.masterFinPeriodID>>
					.View.Select(this, doc.RefNbr))
			{

				FABookPeriod period = (FABookPeriod)tranPeriod;

				FinPeriod finPeriod =
					SelectFrom<FinPeriod>
						.InnerJoin<FABookPeriod>.On<FinPeriod.finPeriodID.IsEqual<FABookPeriod.finPeriodID>>
					.Where<FinPeriod.organizationID.IsEqual<FinPeriod.organizationID.masterValue>
						.And<FinPeriod.startDate.IsNotEqual<FinPeriod.endDate>>
						.And<FinPeriod.finPeriodID.IsEqual<@P.AsString>>
						.And<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID.masterValue>>
						.And<FABookPeriod.startDate.IsNotEqual<FABookPeriod.endDate>>
						.And<FABookPeriod.bookID.IsEqual<@P.AsInt>>
						.And<
							FinPeriod.startDate.IsNotEqual<FABookPeriod.startDate>
							.Or<FinPeriod.endDate.IsNotEqual<FABookPeriod.endDate>>
						>>
					.View.SelectSingleBound(this, null, period.MasterFinPeriodID, period.BookID);

				if (finPeriod != null)
				{
					throw new PXException(Messages.PostingBookNotMatchFinPeriodInGL, FinPeriodIDAttribute.FormatForError(period.FinPeriodID), PXAccess.GetOrganizationCD(period.OrganizationID));
				}

			}
		}

		public virtual void ProcessAssetTran(JournalEntry je, FARegister doc, DocumentList<Batch> created)
		{
			if (doc == null) return;

			DataChecking(doc);

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				foreach (PXResult<FATran, FixedAsset, FADetails, FABook, FABookBalance, FABookHist> res in PXSelectJoin<FATran, InnerJoin<FixedAsset, On<FixedAsset.assetID, Equal<FATran.assetID>>, 
																																InnerJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>, 
																																InnerJoin<FABook, On<FABook.bookID, Equal<FATran.bookID>>, 
																																InnerJoin<FABookBalance, On<FABookBalance.assetID, Equal<FATran.assetID>, 
																																	And<FABookBalance.bookID, Equal<FATran.bookID>>>, 
																																LeftJoin<FABookHist, On<FABookHist.assetID, Equal<FATran.assetID>, 
																																	And<FABookHist.bookID, Equal<FATran.bookID>, 
																																	And<FABookHist.finPeriodID, Equal<FATran.finPeriodID>>>>>>>>>, 
																																Where<FATran.refNbr, Equal<Required<FARegister.refNbr>>, 
																																	And<FATran.released, Equal<False>>>>.Select(this, doc.RefNbr))
				{
					FATran fatran = res;
					FixedAsset asset = res;
					FADetails det = res;
					FABook book = res;
					FABookBalance bookbal = res;
					FABookHist posthist = res;

					if (det.Hold == true)
					{
						throw new PXException(Messages.TranPostedOnHold);
					}

					if (det.Status == FixedAssetStatus.Disposed && fatran.Origin == FARegister.origin.Reversal)
					{
						throw new PXException(Messages.CantReverseDisposedAsset, asset.AssetCD);
					}

					if (det.Status != FixedAssetStatus.Disposed && fatran.Origin == FARegister.origin.DisposalReversal)
					{
						throw new PXException(Messages.CantReverseDisposal, asset.AssetCD, new FixedAssetStatus.ListAttribute().ValueLabelDic[det.Status]);
					}
					
					if (posthist.Suspended == true 
						&& fatran.TranType != FATran.tranType.PurchasingPlus 
						&& fatran.TranType != FATran.tranType.PurchasingMinus
						&& fatran.Origin != FARegister.origin.Reversal)
					{
						throw new PXException(Messages.TranPostedToSuspendedPeriod, booktran.Cache.GetValueExt<FATran.bookID>(fatran));
					}

					FADepreciationMethod method = PXSelect<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(this, bookbal.DepreciationMethodID);
					if ((fatran.TranType == FATran.tranType.CalculatedPlus || fatran.TranType == FATran.tranType.CalculatedMinus) && method == null)
					{
						throw new PXException(Messages.DepreciationMethodDoesNotExist);
					}

					FixedAsset cls = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.classID>>>>.Select(this, asset.ClassID);

					if (doc.Origin == FARegister.origin.DisposalReversal)
					{
						// DONE: Call CloseFABookHistory(toPeriod)
						this.CloseFABookHistory(bookbal, fatran.FinPeriodID);
					}

					int? organizationID = PXAccess.GetParentOrganizationID(fatran.BranchID);
					PXResultset<FABookPeriod> closedPeriods = new PXResultset<FABookPeriod>();
					if (doc.Origin == FARegister.origin.Transfer && !string.IsNullOrEmpty(bookbal.CurrDeprPeriod))
					{
						closedPeriods.AddRange(PXSelect<
							FABookPeriod, 
							Where<FABookPeriod.bookID, Equal<Required<FATran.bookID>>,
								And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
								And<FABookPeriod.finPeriodID, GreaterEqual<Required<FABookPeriod.finPeriodID>>,
								And<FABookPeriod.finPeriodID, Less<Required<FABookPeriod.finPeriodID>>,
								And<FABookPeriod.endDate, Greater<FABookPeriod.startDate>>>>>>>
							.Select(this, fatran.BookID, organizationID, bookbal.CurrDeprPeriod, fatran.FinPeriodID));
					}
					if (doc.Origin == FARegister.origin.Transfer && string.IsNullOrEmpty(bookbal.CurrDeprPeriod) && !string.IsNullOrEmpty(bookbal.LastDeprPeriod))
					{
						closedPeriods.AddRange(PXSelect<
							FABookPeriod, 
							Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
								And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
								And<FABookPeriod.finPeriodID, GreaterEqual<Required<FABookPeriod.finPeriodID>>,
								And<FABookPeriod.finPeriodID, Less<Required<FABookPeriod.finPeriodID>>,
								And<FABookPeriod.endDate, Greater<FABookPeriod.startDate>>>>>>>
							.Select(this, fatran.BookID, organizationID, bookbal.LastDeprPeriod, fatran.FinPeriodID));
					}
					if (doc.Origin == FARegister.origin.Split && fatran.TranType == FATran.tranType.DepreciationPlus)
					{
						closedPeriods.AddRange(PXSelect<
							FABookPeriod, 
							Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>, 
								And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
								And<FABookPeriod.finPeriodID, GreaterEqual<Required<FABookPeriod.finPeriodID>>, 
								And<FABookPeriod.finPeriodID, Less<Required<FABookPeriod.finPeriodID>>, 
								And<FABookPeriod.endDate, Greater<FABookPeriod.startDate>>>>>>>
							.Select(this, fatran.BookID, organizationID, bookbal.DeprFromPeriod, fatran.FinPeriodID));
					}
					if ((fatran.TranType == FATran.tranType.PurchasingDisposal || (doc.Origin == FARegister.origin.Split && (fatran.TranType == FATran.tranType.DepreciationPlus || fatran.TranType == FATran.tranType.DepreciationMinus))) && string.Compare(fatran.FinPeriodID, bookbal.DeprToPeriod) > 0 && bookbal.Status == FixedAssetStatus.FullyDepreciated)
					{
						closedPeriods.AddRange(PXSelect<
							FABookPeriod, 
							Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
								And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
								And<FABookPeriod.finPeriodID, Greater<Required<FABookPeriod.finPeriodID>>,
								And<FABookPeriod.finPeriodID, LessEqual<Required<FABookPeriod.finPeriodID>>,
								And<FABookPeriod.endDate, Greater<FABookPeriod.startDate>>>>>>>
							.Select(this, fatran.BookID, organizationID, bookbal.DeprToPeriod, fatran.FinPeriodID));
					}
					if (fatran.TranType == FATran.tranType.PurchasingReversal)
					{
						FATran disptran = PXSelect<
							FATran, 
							Where<FATran.assetID, Equal<Current<FATran.assetID>>, 
								And<FATran.bookID, Equal<Current<FATran.bookID>>, 
								And<FATran.tranType, Equal<FATran.tranType.purchasingDisposal>, 
								And<FATran.finPeriodID, LessEqual<Current<FATran.finPeriodID>>>>>>, 
							OrderBy<
								Desc<FATran.finPeriodID>>>
							.SelectSingleBound(this, new object[] { fatran });

						closedPeriods.AddRange(PXSelect<
							FABookPeriod, 
							Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
								And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
								And<FABookPeriod.finPeriodID, Greater<Required<FABookPeriod.finPeriodID>>,
								And<FABookPeriod.finPeriodID, Less<Required<FABookPeriod.finPeriodID>>,
								And<FABookPeriod.endDate, Greater<FABookPeriod.startDate>>>>>>>
							.Select(this, fatran.BookID, organizationID, disptran != null ? disptran.FinPeriodID : fatran.FinPeriodID, fatran.FinPeriodID));
					}
					if ((fatran.TranType == FATran.tranType.CalculatedPlus || fatran.TranType == FATran.tranType.CalculatedMinus) &&
						bookbal.Status == FixedAssetStatus.FullyDepreciated && method.IsPureStraightLine)
					{
						closedPeriods.AddRange(PXSelect<
							FABookPeriod, 
							Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
								And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
								And<FABookPeriod.finPeriodID, Greater<Required<FABookPeriod.finPeriodID>>,
								And<FABookPeriod.finPeriodID, Less<Required<FABookPeriod.finPeriodID>>,
								And<FABookPeriod.endDate, Greater<FABookPeriod.startDate>>>>>>>
							.Select(this, fatran.BookID, organizationID, bookbal.DeprToPeriod, fatran.FinPeriodID));
					}

					if (closedPeriods != null)
					{
						// DONE: Call CloseFABookHistory(periods)
						string maxClosedPeriod = this.CloseFABookHistory(bookbal, closedPeriods.RowCast<FABookPeriod>().Select(p => p.FinPeriodID));
						SetLastDeprPeriod(bookbalance, bookbal, maxClosedPeriod);
					}

					if (doc.Origin == FARegister.origin.Reversal)
					{
						if (book.UpdateGL == true)
						{
							// Nearest open period
							OrganizationFinPeriod glperiod = FinPeriodUtils
								.GetNearestOpenOrganizationFinPeriodInSubledger<OrganizationFinPeriod.fAClosed>(
									fatran.FinPeriodID,
									fatran.BranchID);

							if (string.CompareOrdinal(glperiod.FinPeriodID, fatran.FinPeriodID) > 0)
							{
								fatran.FinPeriodID = glperiod.FinPeriodID;
							}
						}
					}
					else if (fatran.TranType == "P+" || fatran.TranType == "P-" ||
						fatran.TranType == "R+" || fatran.TranType == "R-")
					{
						if (book.UpdateGL == true)
						{
							// Nearest open period
							OrganizationFinPeriod period = FinPeriodUtils
								.GetNearestOpenOrganizationFinPeriodInSubledger<OrganizationFinPeriod.fAClosed>(
									fatran.FinPeriodID,
									fatran.BranchID);

							fatran.FinPeriodID = period.FinPeriodID;
						}
					}

					if (fatran.TranType == "R+" || fatran.TranType == "R-")
					{
						if(string.CompareOrdinal(fatran.TranPeriodID, bookbal.DeprFromPeriod) < 0)
						{
							fatran.TranPeriodID = bookbal.DeprFromPeriod;
						}
						if (!string.IsNullOrEmpty(bookbal.DeprToPeriod) && string.CompareOrdinal(fatran.TranPeriodID, bookbal.DeprToPeriod) > 0)
						{
							fatran.TranPeriodID = bookbal.DeprToPeriod;
						}
					}

					if (fatran.TranType == "P+" || fatran.TranType == "P-")
					{
						if (doc.Origin != FARegister.origin.Split && UseAcceleratedDepreciation(cls, method))
						{
							if (string.CompareOrdinal(fatran.TranPeriodID, bookbal.CurrDeprPeriod) < 0)
							{
								fatran.TranPeriodID = bookbal.CurrDeprPeriod;
							}
							else if (book.UpdateGL == true)
							{
								// Nearest open period
								OrganizationFinPeriod period = FinPeriodUtils
									.GetNearestOpenOrganizationFinPeriodInSubledger<OrganizationFinPeriod.fAClosed>(
										fatran.TranPeriodID,
										fatran.BranchID);

								if (string.CompareOrdinal(fatran.TranPeriodID, period.FinPeriodID) < 0)
								{
									fatran.TranPeriodID = period.FinPeriodID;
								}
							}
						}

						if (string.CompareOrdinal(fatran.TranPeriodID, bookbal.DeprFromPeriod) < 0)
						{
							fatran.TranPeriodID = bookbal.DeprFromPeriod;
						}

						string maxClosedPeriod = null;

						foreach (PXResult<FABookPeriod, OrganizationFinPeriod> p in PXSelectJoin<
							FABookPeriod, 
							LeftJoin<OrganizationFinPeriod,
								On<FABookPeriod.finPeriodID, Equal<OrganizationFinPeriod.finPeriodID>,
								And<FABookPeriod.organizationID, Equal<OrganizationFinPeriod.organizationID>>>>,
							Where<FABookPeriod.bookID, Equal<Required<FATran.bookID>>,
								And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>,
								And<FABookPeriod.finPeriodID, GreaterEqual<Required<FABookBalance.deprFromPeriod>>,
								And<FABookPeriod.finPeriodID, LessEqual<Required<FATran.finPeriodID>>,
								And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>>>>>>>
							.Select(this, fatran.BookID, bookbal.DeprFromPeriod, fatran.FinPeriodID, PXAccess.GetParentOrganizationID(fatran.BranchID)))
						{
							FABookPeriod bookperiod = p;
							OrganizationFinPeriod finperiod = p;

							FABookHist createdHist = new FABookHist
							{
								AssetID = fatran.AssetID,
								BookID = fatran.BookID,
								FinPeriodID = bookperiod.FinPeriodID
							};
							createdHist = bookhist.Insert(createdHist);
							createdHist.Closed = finperiod.FAClosed == true;

							if ((maxClosedPeriod == null || (string.CompareOrdinal(bookperiod.FinPeriodID, maxClosedPeriod)) > 0 ) && createdHist.Closed == true)
							{
								maxClosedPeriod = bookperiod.FinPeriodID;
							}
						}
						SetLastDeprPeriod(bookbalance, bookbal, maxClosedPeriod);
					}

					switch (fatran.TranType)
					{
						case "P+":
							if (!fatran.IsOriginReversal || fatran.DebitAccountID == null)
							{
								fatran.DebitAccountID = asset.FAAccountID;
								fatran.DebitSubID = asset.FASubID;
							}
							if (!fatran.IsOriginReversal || fatran.CreditAccountID == null)
							{
								fatran.CreditAccountID = asset.FAAccrualAcctID;
								fatran.CreditSubID = asset.FAAccrualSubID;
							}

							break;

						case "PR":
							if (fatran.DebitAccountID == null)
							{
								fatran.DebitAccountID = asset.FAAccountID;
								fatran.DebitSubID = asset.FASubID;
							}

							if (fatran.CreditAccountID == null)
							{
								fatran.CreditAccountID = asset.FAAccrualAcctID;
								fatran.CreditSubID = asset.FAAccrualSubID;
							}
							break;

						case "P-":
							if (!fatran.IsOriginReversal || fatran.CreditAccountID == null)
							{
								fatran.CreditAccountID = asset.FAAccountID;
								fatran.CreditSubID = asset.FASubID;
							}
							if (!fatran.IsOriginReversal || fatran.DebitAccountID == null)
							{
								fatran.DebitAccountID = asset.FAAccrualAcctID;
								fatran.DebitSubID = asset.FAAccrualSubID;
							}
							break;

						case "PD":
							if (fatran.CreditAccountID == null)
							{
								fatran.CreditAccountID = asset.FAAccountID;
								fatran.CreditSubID = asset.FASubID;
							}

							if (fatran.DebitAccountID == null)
							{
								throw new PXException(ErrorMessages.FieldIsEmpty, typeof(FATran.creditAccountID).Name);
							}
							break;

						case "R+":
							if (!fatran.IsOriginReversal || fatran.DebitAccountID == null)
							{
								fatran.DebitAccountID = asset.FAAccrualAcctID;
								fatran.DebitSubID = asset.FAAccrualSubID;
							}
							if (fatran.CreditAccountID == null)
							{
								fatran.CreditAccountID = asset.FAAccrualAcctID;
								fatran.CreditSubID = asset.FAAccrualSubID;
							}

							break;
						case "R-":
							if (!fatran.IsOriginReversal || fatran.CreditAccountID == null)
							{
								fatran.CreditAccountID = asset.FAAccrualAcctID;
								fatran.CreditSubID = asset.FAAccrualSubID;
							}
							if (fatran.DebitAccountID == null)
							{
								fatran.DebitAccountID = asset.FAAccrualAcctID;
								fatran.DebitSubID = asset.FAAccrualSubID;
							}

							break;
						case "C+":
						case "D+":
						case "A+":
							if (fatran.CreditAccountID == null)
							{
								fatran.CreditAccountID = asset.AccumulatedDepreciationAccountID;
								fatran.CreditSubID = asset.AccumulatedDepreciationSubID;
							}
							if (fatran.DebitAccountID == null)
							{
								fatran.DebitAccountID = asset.DepreciatedExpenseAccountID;
								fatran.DebitSubID = asset.DepreciatedExpenseSubID;
							}
							break;
						case "C-":
						case "D-":
						case "A-":
							if (fatran.DebitAccountID == null)
							{
								fatran.DebitAccountID = asset.AccumulatedDepreciationAccountID;
								fatran.DebitSubID = asset.AccumulatedDepreciationSubID;
							}
							if (fatran.CreditAccountID == null)
							{
								fatran.CreditAccountID = asset.DepreciatedExpenseAccountID;
								fatran.CreditSubID = asset.DepreciatedExpenseSubID;
							}
							break;
						case "S+":
							if (fatran.DebitAccountID == null)
							{
								throw new PXException(ErrorMessages.FieldIsEmpty, typeof (FATran.debitAccountID).Name);
							}
							if (fatran.CreditAccountID == null)
							{
								throw new PXException(ErrorMessages.FieldIsEmpty, typeof(FATran.creditAccountID).Name);
							}
							break;
						case "S-":
							if (fatran.DebitAccountID == null)
							{
								throw new PXException(ErrorMessages.FieldIsEmpty, typeof(FATran.debitAccountID).Name);
							}

							if (fatran.CreditAccountID == null)
							{
								throw new PXException(ErrorMessages.FieldIsEmpty, typeof(FATran.creditAccountID).Name);
							}
							break;
						case "TD":
							if(!fatran.IsOriginReversal)
								fatran.TranAmt = bookbal.YtdDepreciated;
							asset = (FixedAsset)fixedasset.Cache.Locate(asset) ?? asset;
							asset.AccumulatedDepreciationAccountID = fatran.CreditAccountID;
							asset.AccumulatedDepreciationSubID = fatran.CreditSubID;
							fixedasset.Update(asset);
							break;
						case "TP":
							if (!fatran.IsOriginReversal)
								fatran.TranAmt = bookbal.YtdAcquired;
							asset = (FixedAsset)fixedasset.Cache.Locate(asset) ?? asset;
							asset.FAAccountID = fatran.DebitAccountID;
							asset.FASubID = fatran.DebitSubID;
							asset.BranchID = fatran.BranchID;
							fixedasset.Update(asset);
							break;
					}

					FABookHist hist = new FABookHist
						{
							AssetID = fatran.AssetID,
							BookID = fatran.BookID,
							FinPeriodID = fatran.FinPeriodID
						};

					hist = bookhist.Insert(hist);

					switch (fatran.TranType)
					{
						case "P+":
							hist.PtdAcquired += fatran.TranAmt;
							hist.YtdAcquired += fatran.TranAmt;
							hist.YtdBal += fatran.TranAmt;
							break;
						case "P-":
							hist.PtdAcquired -= fatran.TranAmt;
							hist.YtdAcquired -= fatran.TranAmt;
							hist.YtdBal -= fatran.TranAmt;
							break;
						case "PR":
							hist.YtdBal += fatran.TranAmt;
							break;
						case "PD":
							hist.YtdBal -= fatran.TranAmt;
							break;
						case FATran.tranType.CalculatedPlus:
							if (string.IsNullOrEmpty(bookbal.CurrDeprPeriod) && string.IsNullOrEmpty(bookbal.LastDeprPeriod) 
								|| !string.IsNullOrEmpty(bookbal.CurrDeprPeriod)
									&& !string.Equals(bookbal.CurrDeprPeriod, hist.FinPeriodID)
									&& !IsDepreciatedInCurrentDeprPeriodNotByDisposal(this, asset.AssetID, bookbal.BookID, bookbal.CurrDeprPeriod))
							{
								throw new PXException(
									Messages.CalculatedDepreciationPostedFuturePeriod, 
									asset.AssetCD, 
									book.BookCode, 
									FinPeriodIDAttribute.FormatForError(hist.FinPeriodID),
									FinPeriodIDAttribute.FormatForError(bookbal.CurrDeprPeriod));
							}
							fatran.TranType = FATran.tranType.DepreciationPlus;
							switch (fatran.MethodDesc)
							{
								case Messages.MethodDescTax179:
									hist.PtdTax179 += fatran.TranAmt;
									hist.YtdTax179 += fatran.TranAmt;
									break;
								case Messages.MethodDescBonus:
									hist.PtdBonus += fatran.TranAmt;
									hist.YtdBonus += fatran.TranAmt;
									break;
							}
							hist.PtdDepreciated += fatran.TranAmt;
							hist.YtdDepreciated += fatran.TranAmt;
							hist.YtdBal -= fatran.TranAmt;
							hist.Closed = true;
							if(bookbal.Status == FixedAssetStatus.FullyDepreciated && method.IsPureStraightLine)
							{
								hist.PtdCalculated += fatran.TranAmt;
								hist.YtdCalculated += fatran.TranAmt;
							}
							break;
						case "A+":
							if (fatran.Origin == FARegister.origin.DisposalReversal)
							{
								hist.PtdDeprDisposed += fatran.TranAmt;
							}
							else
							{
								hist.PtdAdjusted += fatran.TranAmt;
							}
							hist.YtdDepreciated += fatran.TranAmt;
							hist.YtdBal -= fatran.TranAmt;
							break;
						case "D+":
							if (string.IsNullOrEmpty(bookbal.CurrDeprPeriod) && string.IsNullOrEmpty(bookbal.LastDeprPeriod) 
								|| !string.IsNullOrEmpty(bookbal.CurrDeprPeriod) && string.CompareOrdinal(bookbal.CurrDeprPeriod, hist.FinPeriodID) <= 0 && !fatran.IsOriginReversal)
							{
								throw new PXException(Messages.DepreciationAdjustmentPostedOpenPeriod);
							}
							switch (fatran.MethodDesc)
							{
								case Messages.MethodDescTax179:
									hist.PtdTax179Recap -= fatran.TranAmt;
									hist.YtdTax179Recap -= fatran.TranAmt;
									break;
								case Messages.MethodDescBonus:
									hist.PtdBonusRecap -= fatran.TranAmt;
									hist.YtdBonusRecap -= fatran.TranAmt;
									break;
							}
							hist.PtdDepreciated += fatran.TranAmt;
							hist.YtdDepreciated += fatran.TranAmt;
							hist.YtdBal -= fatran.TranAmt;
							break;
						case FATran.tranType.CalculatedMinus:
							if (string.IsNullOrEmpty(bookbal.CurrDeprPeriod) && string.IsNullOrEmpty(bookbal.LastDeprPeriod) 
								|| !string.IsNullOrEmpty(bookbal.CurrDeprPeriod) && !string.Equals(bookbal.CurrDeprPeriod, hist.FinPeriodID))
							{
								throw new PXException(
									Messages.CalculatedDepreciationPostedFuturePeriod,
									asset.AssetCD,
									book.BookCode,
									FinPeriodIDAttribute.FormatForError(hist.FinPeriodID),
									FinPeriodIDAttribute.FormatForError(bookbal.CurrDeprPeriod));
							}
							fatran.TranType = FATran.tranType.DepreciationMinus;
							switch (fatran.MethodDesc)
							{
								case Messages.MethodDescTax179:
									hist.PtdTax179 -= fatran.TranAmt;
									hist.YtdTax179 -= fatran.TranAmt;
									break;
								case Messages.MethodDescBonus:
									hist.PtdBonus -= fatran.TranAmt;
									hist.YtdBonus -= fatran.TranAmt;
									break;
							}
							hist.PtdDepreciated -= fatran.TranAmt;
							hist.YtdDepreciated -= fatran.TranAmt;
							hist.YtdBal += fatran.TranAmt;
							hist.Closed = true;
							if (bookbal.Status == FixedAssetStatus.FullyDepreciated && method.IsPureStraightLine)
							{
								hist.PtdCalculated -= fatran.TranAmt;
								hist.YtdCalculated -= fatran.TranAmt;
							}
							break;
						case "A-":
							if (fatran.Origin == FARegister.origin.Disposal)
							{
								hist.PtdDeprDisposed -= fatran.TranAmt;
							}
							else
							{
								hist.PtdAdjusted -= fatran.TranAmt;
							}
							hist.YtdDepreciated -= fatran.TranAmt;
							hist.YtdBal += fatran.TranAmt;
							break;
						case "D-":
							if (string.IsNullOrEmpty(bookbal.CurrDeprPeriod) && string.IsNullOrEmpty(bookbal.LastDeprPeriod)
								|| !string.IsNullOrEmpty(bookbal.CurrDeprPeriod) && string.CompareOrdinal(bookbal.CurrDeprPeriod, hist.FinPeriodID) <= 0 && !fatran.IsOriginReversal)
							{
								throw new PXException(Messages.DepreciationAdjustmentPostedOpenPeriod);
							}
							if (fatran.Origin == FARegister.origin.DisposalReversal)
							{
								FABookHistory h = PXSelectReadonly<
									FABookHistory,
									Where<FABookHistory.assetID, Equal<Required<FABookHistory.assetID>>,
										And<FABookHistory.bookID, Equal<Required<FABookHistory.bookID>>,
										And<FABookHistory.finPeriodID, Equal<Required<FABookHistory.finPeriodID>>>>>>
									.Select(this, hist.AssetID, hist.BookID, hist.FinPeriodID);

								decimal sum = 0;
								foreach (FATran transaction in SelectFrom<FATran>
									.Where<FATran.assetID.IsEqual<@P.AsInt>
										.And<FATran.bookID.IsEqual<@P.AsInt>>
										.And<FATran.origin.IsEqual<FARegister.origin.disposalReversal>>
										.And<FATran.finPeriodID.IsEqual<@P.AsString>>
										.And<FATran.tranType.IsEqual<FATran.tranType.depreciationPlus>
											.Or<FATran.tranType.IsEqual<FATran.tranType.depreciationMinus>>>>
									.View
									.Select(this, hist.AssetID, hist.BookID, hist.FinPeriodID))
								{
									if (transaction.TranType == FATran.tranType.DepreciationMinus)
										sum += transaction.TranAmt ?? 0;
									else if (transaction.TranType == FATran.tranType.DepreciationPlus)
										sum -= transaction.TranAmt ?? 0;
								}

								if (h != null && h.PtdDepreciated == sum)
								{
									hist.Reopen = true;
									hist.Closed = false;
								}
							}

							switch (fatran.MethodDesc)
							{
								case Messages.MethodDescTax179:
									hist.PtdTax179Recap += fatran.TranAmt;
									hist.YtdTax179Recap += fatran.TranAmt;
									break;
								case Messages.MethodDescBonus:
									hist.PtdBonusRecap += fatran.TranAmt;
									hist.YtdBonusRecap += fatran.TranAmt;
									break;
							}
							hist.PtdDepreciated -= fatran.TranAmt;
							hist.YtdDepreciated -= fatran.TranAmt;
							hist.YtdBal += fatran.TranAmt;
							break;
						case "S+":
							hist.PtdDisposalAmount += fatran.TranAmt;
							hist.YtdDisposalAmount += fatran.TranAmt;

							hist.PtdRGOL += fatran.RGOLAmt;
							hist.YtdRGOL += fatran.RGOLAmt;
							break;
						case "S-":
							hist.PtdDisposalAmount -= fatran.TranAmt;
							hist.YtdDisposalAmount -= fatran.TranAmt;

							hist.PtdRGOL -= fatran.RGOLAmt;
							hist.YtdRGOL -= fatran.RGOLAmt;
							break;
						case "TP":
						case "TD":
							if (string.CompareOrdinal(fatran.FinPeriodID, bookbal.DeprToPeriod) > 0)
							{
								//do not create open history outside depreciation schedule of the asset
								bookhist.Cache.SetStatus(hist, PXEntryStatus.Notchanged);
							}
							break;
					}

					hist = new FABookHist
						{
							AssetID = fatran.AssetID,
							BookID = fatran.BookID,
							FinPeriodID = fatran.TranPeriodID
						};
					hist = bookhist.Insert(hist);

					switch (fatran.TranType)
					{
						case "P+":
							hist.PtdDeprBase += fatran.TranAmt;
							hist.YtdDeprBase += fatran.TranAmt;
							break;
						case "P-":
							hist.PtdDeprBase -= fatran.TranAmt;
							hist.YtdDeprBase -= fatran.TranAmt;
							break;
						case "R+":
							hist.PtdReconciled += fatran.TranAmt;
							hist.YtdReconciled += fatran.TranAmt;
							break;
						case "R-":
							hist.PtdReconciled -= fatran.TranAmt;
							hist.YtdReconciled -= fatran.TranAmt;
							break;
						case "TP":
						case "TD":
							if (!string.IsNullOrEmpty(bookbal.DeprToPeriod) && string.CompareOrdinal(fatran.TranPeriodID, bookbal.DeprToPeriod) > 0)
							{
								if (!string.IsNullOrEmpty(bookbal.CurrDeprPeriod))
								{
									throw new PXException(Messages.ActiveAssetTransferedPastDeprToPeriod, FinPeriodIDAttribute.FormatForError(bookbal.DeprToPeriod));
								}
								//do not create open history outside depreciation schedule of the asset
								bookhist.Cache.SetStatus(hist, PXEntryStatus.Notchanged);
							}
							if (!fatran.IsOriginReversal)
							{
								if (!string.IsNullOrEmpty(bookbal.CurrDeprPeriod) && string.CompareOrdinal(bookbal.CurrDeprPeriod, fatran.TranPeriodID) > 0)
								{
									throw new PXException(Messages.ActiveAssetTransferedBeforePeriod, FinPeriodIDAttribute.FormatForError(bookbal.CurrDeprPeriod));
								}
								if (!string.IsNullOrEmpty(bookbal.LastDeprPeriod) && string.CompareOrdinal(bookbal.LastDeprPeriod, fatran.TranPeriodID) >= 0)
								{
									throw new PXException(Messages.FullyDepreciatedAssetTransferedBeforePeriod, FinPeriodIDAttribute.FormatForError(bookbal.LastDeprPeriod));
								}
							}
							break;
					}

					PXSelectBase<FABookBalance> cmd = new PXSelect<FABookBalance, Where<FABookBalance.assetID, Equal<Required<FABookBalance.assetID>>, 
						And<FABookBalance.status, NotEqual<Required<FABookBalance.status>>>>>(this); 
					switch (doc.Origin)
					{
						case FARegister.origin.Purchasing:
							bookbal = (FABookBalance)bookbalance.Cache.Locate(bookbal) ?? bookbal;
							
							bookbal.InitPeriod = bookbal.DeprFromPeriod;
							bookbal.LastPeriod = bookbal.DeprToPeriod;

							if ((fatran.TranType == FATran.tranType.DepreciationMinus || fatran.TranType == FATran.tranType.DepreciationPlus)
								&& string.CompareOrdinal(fatran.FinPeriodID, bookbal.LastPeriod) > 0)
							{
								bookbal.LastPeriod = fatran.FinPeriodID;
								bookbal.LastDeprPeriod = fatran.FinPeriodID;
							}
							
							bookbalance.Update(bookbal);
							break;
						case FARegister.origin.Disposal:
							bookbal = (FABookBalance) bookbalance.Cache.Locate(bookbal) ?? bookbal;
							bookbal.CurrDeprPeriod = null;
							bookbal.Status = FixedAssetStatus.Disposed;
							if (fatran.TranType == FATran.tranType.DepreciationPlus || fatran.TranType == FATran.tranType.DepreciationMinus)
							{
								bookbal.LastDeprPeriod = fatran.FinPeriodID;
							}
							if (string.CompareOrdinal(bookbal.DeprToPeriod, bookbal.DisposalPeriodID) > 0)
							{
								bookbal.DeprToDate = det.DisposalDate;
								bookbal.DeprToPeriod = bookbal.DisposalPeriodID;
							}
							bookbal.LastPeriod = bookbal.DisposalPeriodID;
							bookbalance.Update(bookbal);

							cmd.View.Clear();
							if (cmd.SelectWindowed(0, 1, bookbal.AssetID, FixedAssetStatus.Disposed).Count == 0)
							{
								det = (FADetails)fadetail.Cache.Locate(det) ?? det;
								det.Status = FixedAssetStatus.Disposed;
								fadetail.Update(det);
							}
							break;
						case FARegister.origin.Depreciation:
							bookbal = (FABookBalance)bookbalance.Cache.Locate(bookbal) ?? bookbal;
							if (string.Equals(bookbal.DeprToPeriod, fatran.FinPeriodID))
							{
								bookbal.LastDeprPeriod = fatran.FinPeriodID;
								bookbal.CurrDeprPeriod = null;
								bookbal.Status = FixedAssetStatus.FullyDepreciated;
								bookbalance.Update(bookbal);

								if (IsFullyDepreciatedAsset(this, bookbal.AssetID))
								{
									det.Status = FixedAssetStatus.FullyDepreciated;
									fadetail.Update(det);
								}
							}
							else
							{
								bookbal.LastDeprPeriod = fatran.FinPeriodID;
								bookbal.CurrDeprPeriod = FABookPeriodUtils.PeriodPlusPeriodsCount(bookbal.LastDeprPeriod, 1, bookbal.BookID, bookbal.AssetID);
								bookbalance.Update(bookbal);

								hist = bookhist.Insert(new FABookHist
								{
									AssetID = bookbal.AssetID,
									BookID = bookbal.BookID,
									FinPeriodID = bookbal.CurrDeprPeriod
								});
							}
							break;
						case FARegister.origin.Reversal:
							bookbal = (FABookBalance)bookbalance.Cache.Locate(bookbal) ?? bookbal;
							bookbal.Status = FixedAssetStatus.Reversed;
							bookbal.CurrDeprPeriod = null;
							bookbalance.Update(bookbal);

							cmd.View.Clear();
							if (cmd.SelectWindowed(0, 1, bookbal.AssetID, FixedAssetStatus.Reversed).Count == 0)
							{
								det = (FADetails)fadetail.Cache.Locate(det) ?? det;
								det.Status = FixedAssetStatus.Reversed;
								fadetail.Update(det);
							}

							FAAccrualTran accrual = PXSelect<FAAccrualTran, Where<FAAccrualTran.tranID, Equal<Current<FATran.gLtranID>>>>.SelectSingleBound(this, new object[] { fatran });
							if (accrual != null)
							{
								FAAccrualTran copy = (FAAccrualTran)accrualtran.Cache.CreateCopy(accrual);
								copy.ClosedAmt -= fatran.TranAmt;
								copy.ClosedQty--;
								copy.OpenAmt += fatran.TranAmt;
								copy.OpenQty++;
								accrualtran.Update(copy);
							}
							break;
						case FARegister.origin.DisposalReversal:
							det.DisposalDate = null;
							det.DisposalMethodID = null;
							det.SaleAmount = null;

							bookbal = (FABookBalance)bookbalance.Cache.Locate(bookbal) ?? bookbal;
							bookbal.DeprToDate = bookbal.OrigDeprToDate;
							bookbalance.Cache.SetDefaultExt<FABookBalance.deprToPeriod>(bookbal);
							FABookHistory lhist = PXSelect<FABookHistory, Where<FABookHistory.assetID, Equal<Current<FABookBalance.assetID>>, And<FABookHistory.bookID, Equal<Current<FABookBalance.bookID>>, And<FABookHistory.ytdReversed, Greater<int0>>>>, OrderBy<Desc<FABookHistory.finPeriodID>>>.SelectSingleBound(this, new object[] { bookbal });
							if(lhist != null)
							{
								bookbal.DeprToPeriod = bookbal.Depreciate == true
									? FABookPeriodUtils.PeriodPlusPeriodsCount(bookbal.DeprToPeriod,lhist.YtdReversed ?? 0, bookbal.BookID, bookbal.AssetID)
									: null;
							}

							bool isFullyDepreciated = string.CompareOrdinal(bookbal.LastDeprPeriod, bookbal.DeprToPeriod) >= 0;
							bookbal.Status = bookbal.Depreciate == true && isFullyDepreciated ? FixedAssetStatus.FullyDepreciated : FixedAssetStatus.Active;

							bookbal.CurrDeprPeriod = bookbal.Depreciate != true 
														? bookbal.DeprFromPeriod 
														: isFullyDepreciated 
															? null 
															: IsDepreciatedInCurrentDeprPeriodNotByDisposal(this, asset.AssetID, bookbal.BookID, hist.FinPeriodID)
																? FABookPeriodUtils.PeriodPlusPeriodsCount(hist.FinPeriodID, 1, bookbal.BookID, bookbal.AssetID)
															: hist.FinPeriodID;
							bookbal.LastDeprPeriod = bookbal.Depreciate != true 
														? null 
														: isFullyDepreciated 
															? bookbal.DeprToPeriod 
															: FABookPeriodUtils.PeriodPlusPeriodsCount(bookbal.CurrDeprPeriod, -1, bookbal.BookID, bookbal.AssetID);

							bookbal.LastPeriod = bookbal.DeprToPeriod ?? fatran.FinPeriodID;
							bookbalance.Update(bookbal);

							det = (FADetails)fadetail.Cache.Locate(det) ?? det;
							det.Status = bookbal.Status == FixedAssetStatus.FullyDepreciated && IsFullyDepreciatedAsset(this, bookbal.AssetID)
								? FixedAssetStatus.FullyDepreciated
								: FixedAssetStatus.Active;
							fadetail.Update(det);
							break;
						case FARegister.origin.Split:
							if ((fatran.TranType == FATran.tranType.PurchasingPlus || fatran.TranType == FATran.tranType.PurchasingMinus) && string.CompareOrdinal(fatran.FinPeriodID, bookbal.LastPeriod) > 0)
							{
								bookbal.LastPeriod = fatran.FinPeriodID;
								bookbalance.Update(bookbal);
							}
							break;
						case FARegister.origin.Reconcilliation:
							if ((fatran.TranType == FATran.tranType.DepreciationMinus || fatran.TranType == FATran.tranType.DepreciationPlus)
								&& string.CompareOrdinal(fatran.FinPeriodID, bookbal.LastPeriod) > 0)
							{
								bookbal.LastPeriod = fatran.FinPeriodID;
								bookbal.LastDeprPeriod = fatran.FinPeriodID;
								bookbalance.Update(bookbal);
							}

							// Set CurrDeprPeriod here for the none-depreciable asset 
							// to correctly show its current cost after the additions 
							// in reports and on the "Balance" tab.
							// For more details see AC - 118518.
							// 
							if (asset.Depreciable != true && 
								(fatran.TranType == FATran.tranType.PurchasingPlus || fatran.TranType == FATran.tranType.PurchasingMinus) &&
								string.CompareOrdinal(fatran.FinPeriodID, bookbal.CurrDeprPeriod) > 0)
							{
								bookbal.CurrDeprPeriod = fatran.FinPeriodID;
								bookbalance.Update(bookbal);
							}

							break;
					}

					if (UpdateGL && book.UpdateGL == true && doc.Origin != FARegister.origin.Split)
					{
						SetControlAccountFlags(fatran);

						bool summaryPost = SummPost || (SummPostDepr && fatran.Origin == FARegister.origin.Depreciation);
						SegregateBatch(je, asset.BranchID, fatran.TranDate, fatran.FinPeriodID, doc.DocDesc, created);
						{
							GLTran tran = new GLTran();
							tran.SummPost = summaryPost;
							tran.ReclassificationProhibited = fatran.ReclassificationOnDebitProhibited;
							tran.AccountID = fatran.DebitAccountID;
							tran.SubID = fatran.DebitSubID;
							tran.CuryDebitAmt = fatran.TranAmt;
							tran.CuryCreditAmt = 0m;
							tran.DebitAmt = fatran.TranAmt;
							tran.CreditAmt = 0m;
							tran.TranType = fatran.TranType;
							tran.Released = true;
							tran.TranDesc = fatran.TranDesc;
							tran.RefNbr = fatran.RefNbr;
							tran.TranLineNbr = summaryPost ? null : fatran.LineNbr;
							switch (fatran.TranType)
							{
								case FATran.tranType.ReconciliationPlus:
									tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(); 
									break;
								case FATran.tranType.ReconciliationMinus:
									ApplyOriginalBranchAndProject(doc, fatran, tran);
									break;
								case FATran.tranType.TransferDepreciation:
									tran.BranchID = fatran.SrcBranchID;
									break;
								default:
									tran.BranchID = fatran.BranchID;
									break;
							}

							tran = je.GLTranModuleBatNbr.Insert(tran);
						}

						{
							GLTran tran = new GLTran();
							tran.SummPost = summaryPost;
							tran.ReclassificationProhibited = fatran.ReclassificationOnCreditProhibited;
							tran.AccountID = fatran.CreditAccountID;
							tran.SubID = fatran.CreditSubID;
							tran.CuryDebitAmt = 0m;
							tran.CuryCreditAmt = fatran.TranAmt;
							tran.DebitAmt = 0m;
							tran.CreditAmt = fatran.TranAmt;
							tran.TranType = fatran.TranType;
							tran.Released = true;
							tran.TranDesc = fatran.TranDesc;
							tran.RefNbr = fatran.RefNbr;
							tran.TranLineNbr = summaryPost ? null : fatran.LineNbr;
							switch (fatran.TranType)
							{
								case FATran.tranType.ReconciliationPlus:
									ApplyOriginalBranchAndProject(doc, fatran, tran);
									break;
								case FATran.tranType.ReconciliationMinus:
									tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
									break;
								case FATran.tranType.TransferPurchasing:
									tran.BranchID = fatran.SrcBranchID;
									break;
								default:
									tran.BranchID = fatran.BranchID;
									break;
							}

							tran = je.GLTranModuleBatNbr.Insert(tran);
						}

						if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
						{
							je.Save.Press();

							if (je.BatchModule.Current != null) 
							{
								fatran.BatchNbr = je.BatchModule.Current.BatchNbr;
								
								if (created.Find(je.BatchModule.Current) == null)
								{
									created.Add(je.BatchModule.Current);
								}
							}
						}

						doc.Posted |= (fatran.BatchNbr != null);
					}

					fatran.Released = true;
					booktran.Update(fatran);

					if(fatran.TranType == FATran.tranType.PurchasingPlus)
					{
						asset.IsAcquired = true;
						fixedasset.Update(asset);
					}
				}

				doc.Released = true;
				register.Update(doc);

				this.Actions.PressSave();
				//CalculateAsset(books);

				ts.Complete();
			}
		}

		private DepreciationCalculation depreciationCalculationGraph;
		private bool UseAcceleratedDepreciation(FixedAsset cls, FADepreciationMethod method)
					=> (depreciationCalculationGraph = depreciationCalculationGraph ?? PXGraph.CreateInstance<DepreciationCalculation>()).UseAcceleratedDepreciation(cls, method);

		private void ApplyOriginalBranchAndProject(FARegister faregister, FATran fatran, GLTran gltran)
		{
			GLTran orig = PXSelect<GLTran, Where<GLTran.tranID, Equal<Current<FATran.gLtranID>>>>.SelectSingleBound(this, new object[] { fatran });
			if (orig != null)
			{
				gltran.BranchID = orig.BranchID;
			}
			else if (faregister.Origin == FARegister.origin.Split || faregister.Origin == FARegister.origin.Reversal)
			{
				gltran.BranchID = fatran.BranchID;
			}
			else
			{
				throw new PXException(Messages.InvalidReconTran);
			}
			if (orig?.ProjectID != null)
			{
				gltran.ProjectID = orig.ProjectID;
				gltran.TaskID = orig.TaskID;
			}
		}

		protected static bool IsDepreciatedInCurrentDeprPeriodNotByDisposal(PXGraph graph, int? assetID, int? bookID, string finPeriodID)
		{
			FARegister disposalRegister = GetDisposalRegister(graph, assetID);
			FATran depreciationTran = PXSelectReadonly<
				FATran,
				Where<FATran.assetID, Equal<Required<FixedAsset.assetID>>,
					And<FATran.bookID, Equal<Required<FATran.bookID>>,
					And<FATran.released, Equal<True>,
					And<FATran.finPeriodID, Equal<Required<FATran.finPeriodID>>,
					And<FATran.tranType, Equal<FATran.tranType.depreciationPlus>,
					And<FATran.refNbr, NotEqual<Required<FATran.refNbr>>>>>>>>>
				.SelectSingleBound(graph, null, assetID, bookID, finPeriodID, disposalRegister?.RefNbr);

			return depreciationTran != null;
		}

		protected static FARegister GetDisposalRegister(PXGraph graph, int? assetID)
		{
			return PXSelectReadonly2<
				FARegister,
				LeftJoin<FATran,
					On<FARegister.refNbr, Equal<FATran.refNbr>>>,
				Where<FATran.assetID, Equal<Required<FixedAsset.assetID>>,
					And<FARegister.origin, Equal<FARegister.origin.disposal>,
					And<FARegister.released, Equal<True>>>>,
				OrderBy<
					Desc<FARegister.docDate>>>
				.SelectSingleBound(graph, null, assetID);
		}

		private static readonly string[] NonReclassifiableTranTypes =
		{
			FATran.tranType.PurchasingPlus, FATran.tranType.PurchasingMinus,
			FATran.tranType.ReconciliationPlus, FATran.tranType.ReconciliationMinus,
			FATran.tranType.TransferPurchasing, FATran.tranType.TransferDepreciation,
			FATran.tranType.PurchasingReversal
		};

		private static readonly string[] FullyReclassifiableTranTypes =
		{
			FATran.tranType.SalePlus, FATran.tranType.SaleMinus
		};

		protected virtual void SetControlAccountFlags(FATran fatran)
		{
			if (NonReclassifiableTranTypes.Contains(fatran.TranType))
			{
				CheckControlAccountFlagsIsNotDefined(fatran);

				fatran.ReclassificationOnDebitProhibited = true;
				fatran.ReclassificationOnCreditProhibited = true;
			}
			else if (FullyReclassifiableTranTypes.Contains(fatran.TranType))
			{
				CheckControlAccountFlagsIsNotDefined(fatran);

				fatran.ReclassificationOnDebitProhibited = false;
				fatran.ReclassificationOnCreditProhibited = false;
			}
			else if (fatran.TranType == FATran.tranType.PurchasingDisposal)
			{
				CheckControlAccountFlagsIsNotDefined(fatran);

				fatran.ReclassificationOnDebitProhibited = true;
				fatran.ReclassificationOnCreditProhibited = false;//GainLoss
			}
			else if (fatran.TranType == FATran.tranType.AdjustingDeprPlus)
			{
				if (fatran.ReclassificationOnDebitProhibited == null)
				{
					fatran.ReclassificationOnDebitProhibited = false;//Depreciated Expense
				}
				if (fatran.ReclassificationOnCreditProhibited == null)
				{
					fatran.ReclassificationOnCreditProhibited = true;//Accumulated Depretiation
				}
			}
			else if (fatran.TranType == FATran.tranType.AdjustingDeprMinus)
			{
				if (fatran.ReclassificationOnDebitProhibited == null)
				{
					fatran.ReclassificationOnDebitProhibited = true;//Accumulated Depretiation
				}
				if (fatran.ReclassificationOnCreditProhibited == null)
				{
					fatran.ReclassificationOnCreditProhibited = false;//Depreciated Expense or GainLoss
				}
			}
			else if (fatran.TranType == FATran.tranType.DepreciationPlus)
			{
				CheckControlAccountFlagsIsNotDefined(fatran);

				fatran.ReclassificationOnDebitProhibited = false;//Depreciated Expense
				fatran.ReclassificationOnCreditProhibited = true;//Accumulated Depretiation
			}
			else if (fatran.TranType == FATran.tranType.DepreciationMinus)
			{
				CheckControlAccountFlagsIsNotDefined(fatran);

				fatran.ReclassificationOnDebitProhibited = true; //Accumulated Depretiation
				fatran.ReclassificationOnCreditProhibited = false; //Depreciated Expense
			}
			else
			{
				throw new PXException(Messages.FlagHasValueOnButItMustBeUndefinedForTransactionType,
					Caches[typeof (FATran)].GetValueExt<FATran.tranType>(fatran), fatran.ToString());
			}
		}

		private void CheckControlAccountFlagsIsNotDefined(FATran fatran)
		{
			if (fatran.ReclassificationOnDebitProhibited != null)
				throw new PXException(Messages.FlagsOfControlAccountsWereNotDefinedForType,
					typeof (FATran.reclassificationOnDebitProhibited).Name, fatran.ToString(),
					Caches[typeof (FATran)].GetValueExt<FATran.tranType>(fatran));

			if (fatran.ReclassificationOnCreditProhibited != null)
				throw new PXException(Messages.FlagsOfControlAccountsWereNotDefinedForType,
					typeof (FATran.reclassificationOnCreditProhibited).Name, fatran.ToString(),
					Caches[typeof (FATran)].GetValueExt<FATran.tranType>(fatran));
		}

		public static bool IsFullyDepreciatedAsset(PXGraph graph, int? AssetID)
		{
			FABookBalance bookbal = PXSelect<FABookBalance,
				Where<FABookBalance.assetID, Equal<Required<FABookBalance.assetID>>,
				And<FABookBalance.status, NotEqual<FixedAssetStatus.fullyDepreciated>>>>.SelectSingleBound(graph, null, AssetID);

			return bookbal == null;
		}

		private static void SegregateBatch(JournalEntry je, int? BranchID, DateTime? DocDate, string FinPeriodID, string descr, DocumentList<Batch> created)
		{
			if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
			{
				je.Clear();
			}

			Batch fabatch = created.Find<Batch.branchID, Batch.finPeriodID>(BranchID, FinPeriodID);

			if (fabatch != null)
			{
				if (!je.BatchModule.Cache.ObjectsEqual(je.BatchModule.Current, fabatch))
				{
					je.Clear();
				}

				if (fabatch.Description != descr)
				{
					fabatch.Description = "";
					je.BatchModule.Update(fabatch);
				}
				je.BatchModule.Current = fabatch;
			}
			else
			{
				je.Clear();

				fabatch = new Batch();
				fabatch.Module = "FA";
				fabatch.Status = "U";
				fabatch.Released = true;
				fabatch.Hold = false;			
				fabatch.TranPeriodID = FinPeriodID;
				fabatch.BranchID = BranchID;
				fabatch.DebitTotal = 0m;
				fabatch.CreditTotal = 0m;
				fabatch.Description = descr;
				fabatch = je.BatchModule.Insert(fabatch);
				fabatch.DateEntered = DocDate;
				fabatch.FinPeriodID = FinPeriodID;
			    FinPeriodIDAttribute.SetMasterPeriodID<Batch.finPeriodID>(je.BatchModule.Cache, fabatch);
			}
		}

		public static void TransferAsset(PXGraph graph, FixedAsset asset, FALocationHistory location, ref FARegister register)
		{
			PXCache lochistoryCache = graph.Caches[typeof (FALocationHistory)];
			PXCache assetCache = graph.Caches[typeof (FixedAsset)];
			PXCache registerCache = graph.Caches[typeof(FARegister)];
			PXCache transactCache = graph.Caches[typeof(FATran)];
			FASetup fasetup = (FASetup)graph.Caches[typeof(FASetup)].Current;

			if (!assetCache.IsInsertedUpdatedDeleted)
			{
				assetCache.ClearQueryCacheObsolete();
				assetCache.Clear();
				graph.Caches<FixedAsset>().Current = 
					asset = PXSelectReadonly<FixedAsset, Where<FixedAsset.assetCD, Equal<Current<FixedAsset.assetCD>>>>.SelectSingleBound(graph, new object[] {asset});
			}

			var oldAccounts = GetOldAccounts(graph, asset, location);
			var newAccounts = GetNewAccounts(graph, assetCache, lochistoryCache, fasetup, asset, location);

			FALocationHistory oldLocation = PXCache<FALocationHistory>.CreateCopy(location);
			newAccounts.CopyAccountsTo(location);

			if (IsTransfer(asset, oldLocation, location))
			{
				lochistoryCache.MarkUpdated(location);
			}

			foreach (PXResult<FARegister, FATran> res in PXSelectJoin<FARegister, InnerJoin<FATran,
				On<FARegister.refNbr, Equal<FATran.refNbr>>>,
				Where<FARegister.released, NotEqual<True>,
					And<FARegister.origin, Equal<FARegister.origin.transfer>,
					And<FATran.assetID, Equal<Current<FixedAsset.assetID>>>>>>.SelectMultiBound(graph, new object[] { asset }))
			{
				FARegister openreg = res;
				FATran opentran = res;
				register = register ?? openreg;
				transactCache.Delete(opentran);
			location.RefNbr = null;
			}

			PXResultset<FABookBalance> balset;
			if (fasetup.UpdateGL == true &&
				(RequireTransferAsset(oldAccounts, newAccounts) || RequireTransferDepreciation(asset, oldAccounts, newAccounts)) && 
				(balset = PXSelectJoin<FABookBalance, LeftJoin<FABook, On<FABookBalance.bookID, Equal<FABook.bookID>>>,
							Where<FABookBalance.assetID, Equal<Current<FixedAsset.assetID>>,
							And<FABook.updateGL, Equal<True>>>>.SelectMultiBound(graph, new object[]{asset})).Count > 0)
			{
				if (register == null)
				{
					register = (FARegister)registerCache.Insert(new FARegister
					{
						BranchID = newAccounts.BranchID,
						Origin = FARegister.origin.Transfer,
						DocDate = location.TransactionDate
					});
				}
				else
				{
					registerCache.Current = register;
				}
				register.Reason = location.Reason;

				if (asset.IsAcquired == true && !string.IsNullOrEmpty(location.PeriodID) && oldAccounts.BranchID != null)
				{
					foreach (FABookBalance bal in balset)
					{
						if (RequireTransferAsset(oldAccounts, newAccounts))
						{
							InsertTransferAssetTransaction(transactCache, ref register, bal, asset, location, oldAccounts, newAccounts);
						}

						if (RequireTransferDepreciation(asset, oldAccounts, newAccounts))
						{
							InsertTransferDepreciationTransaction(transactCache, ref register, bal, asset, location, oldAccounts, newAccounts);
						}

						location.RefNbr = register.RefNbr;
					}
				}
			}

			UpdateAssetAccounts(assetCache, asset, oldAccounts, newAccounts);

			#region garbage collection

			FALocationHistory prevLocation = graph.GetPrevLocation(location);
			if (prevLocation != null && location.RefNbr == null && !graph.IsLocationChanged(location, prevLocation) && !graph.IsAccountsChanged(location, prevLocation))
			{
				graph.Caches<FALocationHistory>().Delete(location);
				FALocationHistory updated = PXCache<FALocationHistory>.CreateCopy(location);
				updated.RevisionID = prevLocation.RevisionID;
				updated.TransactionDate = prevLocation.TransactionDate;
				updated.PeriodID = prevLocation.PeriodID;
				updated.RefNbr = prevLocation.RefNbr;
				graph.Caches<FALocationHistory>().Update(updated);

				FADetails details = PXSelect<FADetails, Where<FADetails.assetID, Equal<Current<FALocationHistory.assetID>>>>.SelectSingleBound(graph, new object[] {location});
				details.LocationRevID = updated.RevisionID;
				graph.Caches<FADetails>().Update(details);
			}

			// Clear empty unreleased transfer
			FATran existing = PXSelect<FATran, Where<FATran.refNbr, Equal<Current<FARegister.refNbr>>>>.SelectSingleBound(graph, new object[] {registerCache.Current});
			if (existing == null)
			{
				registerCache.Delete(registerCache.Current);
			}

			#endregion
		}
		protected static FAAccounts GetOldAccounts(PXGraph graph, FixedAsset asset, FALocationHistory location)
		{
			var origAsset = graph.Caches[typeof(FixedAsset)].GetOriginal(asset) as FixedAsset ?? asset;

			if (location.RefNbr == null)
				return new FAAccounts(origAsset);

			FARegister register = PXSelect<FARegister, Where<FARegister.refNbr, Equal<Required<FARegister.refNbr>>>>.SelectSingleBound(graph, null, location.RefNbr);
			if (register == null || register.Released == true)
				return new FAAccounts(origAsset);

			var prevLocation = graph.GetPrevLocation(location);

			return (prevLocation != null) ? new FAAccounts(prevLocation) : new FAAccounts(origAsset);
		}
		protected static FAAccounts GetNewAccounts(PXGraph graph, PXCache assetCache, PXCache locationsCache, FASetup fasetup, FixedAsset asset, FALocationHistory location)
		{
			FAAccounts result;

			// in case of changing class we shoul get the new accounts from class config
			if (asset.OldClassID != null && asset.OldClassID != asset.ClassID)
			{
				FixedAsset faclass = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.assetID>>>>.Select(graph, asset.ClassID);
				if (faclass == null)
				{
					throw new PXException(ErrorMessages.ValueDoesntExist, Messages.AssetClass, asset.ClassID);
				}

				result = new FAAccounts(faclass);
				result.DisposalSubID = AssetMaint.MakeSubID<FixedAsset.proceedsSubMask, FixedAsset.disposalSubID>(assetCache, asset);
				result.GainSubID = AssetMaint.MakeSubID<FixedAsset.gainLossSubMask, FixedAsset.gainSubID>(assetCache, asset);
				result.LossSubID = AssetMaint.MakeSubID<FixedAsset.gainLossSubMask, FixedAsset.lossSubID>(assetCache, asset);

				result.BranchID = location.BranchID;

				if (fasetup.UpdateGL == true)
				{
					result.FASubID = AssetMaint.MakeSubID<FixedAsset.fASubMask, FixedAsset.fASubID>(assetCache, asset);
					result.AccumulatedDepreciationSubID = AssetMaint.MakeSubID<FixedAsset.accumDeprSubMask, FixedAsset.accumulatedDepreciationSubID>(assetCache, asset);
					result.DepreciatedExpenseSubID = AssetMaint.MakeSubID<FixedAsset.accumDeprSubMask, FixedAsset.accumulatedDepreciationSubID>(assetCache, asset);
				}
				else
				{
					result.FASubID = (int?)assetCache.GetValue<FixedAsset.fASubID>(faclass);
					result.AccumulatedDepreciationSubID = (int?)assetCache.GetValue<FixedAsset.accumulatedDepreciationSubID>(faclass);
					result.DepreciatedExpenseSubID = (int?)assetCache.GetValue<FixedAsset.depreciatedExpenseSubID>(faclass);
				}
			}
			// otherwise, we shoud keep the original accounts,
			// should recalculate subaccounts - ex. if the department is changed
			// and we should restore FA Account and/or Accum.Depr account from the latest location history,
			// if we have changed class before (in this case we have transaction number)
			else
			{
				result = new FAAccounts(asset);
				result.BranchID = location.BranchID;

				FixedAsset origAsset = assetCache.GetOriginal(asset) as FixedAsset;

				if (assetCache.ObjectsEqual<FixedAsset.fAAccountID>(asset, origAsset))
					result.FAAccountID = location.FAAccountID;

				if (assetCache.ObjectsEqual<FixedAsset.fASubID>(asset, origAsset))
					result.FASubID = location.FASubID;

				if (assetCache.ObjectsEqual<FixedAsset.accumulatedDepreciationAccountID>(asset, origAsset))
					result.AccumulatedDepreciationAccountID = location.AccumulatedDepreciationAccountID;

				if (assetCache.ObjectsEqual<FixedAsset.accumulatedDepreciationSubID>(asset, origAsset))
					result.AccumulatedDepreciationSubID = location.AccumulatedDepreciationSubID;

				FALocationHistory origLocation =
					(locationsCache.GetStatus(location) == PXEntryStatus.Inserted) ?
						origLocation = graph.GetPrevLocation(location) :
						locationsCache.GetOriginal(location) as FALocationHistory;

				if (origLocation == null || !locationsCache.ObjectsEqual<FALocationHistory.locationID, FALocationHistory.department>(origLocation, location))
				{
					if (assetCache.ObjectsEqual<FixedAsset.fASubID>(asset, origAsset))
						result.FASubID = AssetMaint.MakeSubID<FixedAsset.fASubMask, FixedAsset.fASubID>(assetCache, asset);

					if (assetCache.ObjectsEqual<FixedAsset.accumulatedDepreciationSubID>(asset, origAsset))
						result.AccumulatedDepreciationSubID = AssetMaint.MakeSubID<FixedAsset.accumDeprSubMask, FixedAsset.accumulatedDepreciationSubID>(assetCache, asset);

					if (assetCache.ObjectsEqual<FixedAsset.depreciatedExpenseSubID>(asset, origAsset))
						result.DepreciatedExpenseSubID = AssetMaint.MakeSubID<FixedAsset.deprExpenceSubMask, FixedAsset.depreciatedExpenseSubID>(assetCache, asset);

					if (assetCache.ObjectsEqual<FixedAsset.disposalSubID>(asset, origAsset))
						result.DisposalSubID = AssetMaint.MakeSubID<FixedAsset.proceedsSubMask, FixedAsset.disposalSubID>(assetCache, asset);

					if (assetCache.ObjectsEqual<FixedAsset.gainSubID>(asset, origAsset))
						result.GainSubID = AssetMaint.MakeSubID<FixedAsset.gainLossSubMask, FixedAsset.gainSubID>(assetCache, asset);

					if (assetCache.ObjectsEqual<FixedAsset.lossSubID>(asset, origAsset))
						result.LossSubID = AssetMaint.MakeSubID<FixedAsset.gainLossSubMask, FixedAsset.lossSubID>(assetCache, asset);
				}
			}

			return result;
		}

		protected static bool IsTransfer(FixedAsset asset, FALocationHistory oldLocation, FALocationHistory newLocation)
		{
			return
				newLocation.FAAccountID != oldLocation.FAAccountID ||
				newLocation.FASubID != oldLocation.FASubID ||
				newLocation.DisposalAccountID != oldLocation.DisposalAccountID ||
				newLocation.DisposalSubID != oldLocation.DisposalSubID ||
				newLocation.GainAcctID != oldLocation.GainAcctID ||
				newLocation.GainSubID != oldLocation.GainSubID ||
				newLocation.LossAcctID != oldLocation.LossAcctID ||
				newLocation.LossSubID != oldLocation.LossSubID ||

				(newLocation.AccumulatedDepreciationAccountID != oldLocation.AccumulatedDepreciationAccountID ||
				newLocation.AccumulatedDepreciationSubID != oldLocation.AccumulatedDepreciationSubID ||
				newLocation.DepreciatedExpenseAccountID != oldLocation.DepreciatedExpenseAccountID ||
				newLocation.DepreciatedExpenseSubID != oldLocation.DepreciatedExpenseSubID) && asset.Depreciable == true;
		}
		protected static bool RequireTransferAsset(FAAccounts oldAccounts, FAAccounts newAccounts)
		{
			return
				newAccounts.BranchID != oldAccounts.BranchID ||
				newAccounts.FAAccountID != oldAccounts.FAAccountID ||
				newAccounts.FASubID != oldAccounts.FASubID;
		}
		protected static bool RequireTransferDepreciation(FixedAsset asset, FAAccounts oldAccounts, FAAccounts newAccounts)
		{
			return
				asset.Depreciable == true &&
				(
					newAccounts.BranchID != oldAccounts.BranchID ||
					newAccounts.AccumulatedDepreciationAccountID != oldAccounts.AccumulatedDepreciationAccountID ||
					newAccounts.AccumulatedDepreciationSubID != oldAccounts.AccumulatedDepreciationSubID
				);
		}
		protected static void InsertTransferAssetTransaction(PXCache transactCache, ref FARegister register, FABookBalance bal, FixedAsset asset, FALocationHistory location, FAAccounts oldAccounts, FAAccounts newAccounts)
		{
			var tran = new FATran
			{
				AssetID = asset.AssetID,
				BookID = bal.BookID,
				TranType = FATran.tranType.TransferPurchasing,
				FinPeriodID = location.PeriodID ?? bal.CurrDeprPeriod ?? bal.LastPeriod,
				TranDate = location.TransactionDate,
				CreditAccountID = oldAccounts.FAAccountID,
				CreditSubID = oldAccounts.FASubID,
				DebitAccountID = newAccounts.FAAccountID,
				DebitSubID = newAccounts.FASubID,
				TranAmt = bal.YtdAcquired,
				TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescTransferPurchasing, asset.AssetCD),
				BranchID = newAccounts.BranchID,
				SrcBranchID = oldAccounts.BranchID,
				LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<FATran.lineNbr>(transactCache, register),
			};
			var inserted = (FATran)transactCache.Insert(tran);
		}
		protected static void InsertTransferDepreciationTransaction(PXCache transactCache, ref FARegister register, FABookBalance bal, FixedAsset asset, FALocationHistory location, FAAccounts oldAccounts, FAAccounts newAccounts)
		{
			var tran = new FATran
			{
				AssetID = asset.AssetID,
				BookID = bal.BookID,
				TranType = FATran.tranType.TransferDepreciation,
				FinPeriodID = location.PeriodID ?? bal.CurrDeprPeriod ?? bal.LastPeriod,
				TranDate = location.TransactionDate,
				CreditAccountID = newAccounts.AccumulatedDepreciationAccountID,
				CreditSubID = newAccounts.AccumulatedDepreciationSubID,
				DebitAccountID = oldAccounts.AccumulatedDepreciationAccountID,
				DebitSubID = oldAccounts.AccumulatedDepreciationSubID,
				TranAmt = bal.YtdDepreciated,
				TranDesc = PXMessages.LocalizeFormatNoPrefix(Messages.TranDescTransferDepreciation, asset.AssetCD),
				BranchID = newAccounts.BranchID,
				SrcBranchID = oldAccounts.BranchID,
				LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<FATran.lineNbr>(transactCache, register)
			};
			var inserted = (FATran)transactCache.Insert(tran);
		}
		protected static void UpdateAssetAccounts(PXCache assetCache, FixedAsset asset, FAAccounts oldAccounts, FAAccounts newAccounts)
		{
			FixedAsset copy = (FixedAsset)assetCache.CreateCopy(asset);
			newAccounts.CopyMinorAccountsTo(copy);
			if (
				newAccounts.DisposalAccountID != oldAccounts.DisposalAccountID ||
				newAccounts.DisposalSubID != oldAccounts.DisposalSubID ||
				newAccounts.GainAcctID != oldAccounts.GainAcctID ||
				newAccounts.GainSubID != oldAccounts.GainSubID ||
				newAccounts.LossAcctID != oldAccounts.LossAcctID ||
				newAccounts.LossSubID != oldAccounts.LossSubID ||

				asset.Depreciable == true &&
				(
					newAccounts.DepreciatedExpenseAccountID != oldAccounts.DepreciatedExpenseAccountID ||
					newAccounts.DepreciatedExpenseSubID != oldAccounts.DepreciatedExpenseSubID
				))
			{
				assetCache.Update(copy);
			}
		}
		protected class FAAccounts
		{
			public int? ClassID { get; set; }

			public int? BranchID { get; set; }

			public int? FAAccountID { get; set; }
			public int? FASubID { get; set; }
			public int? AccumulatedDepreciationAccountID { get; set; }
			public int? AccumulatedDepreciationSubID { get; set; }
			public int? DepreciatedExpenseAccountID { get; set; }
			public int? DepreciatedExpenseSubID { get; set; }
			public int? DisposalAccountID { get; set; }
			public int? DisposalSubID { get; set; }
			public int? GainAcctID { get; set; }
			public int? GainSubID { get; set; }
			public int? LossAcctID { get; set; }
			public int? LossSubID { get; set; }

			public FAAccounts(FixedAsset asset)
			{
				ClassID = (asset.RecordType == FARecordType.ClassType) ? asset.AssetID : asset.ClassID;
				BranchID = asset.BranchID;
				FAAccountID = asset.FAAccountID;
				FASubID = asset.FASubID;
				AccumulatedDepreciationAccountID = asset.AccumulatedDepreciationAccountID;
				AccumulatedDepreciationSubID = asset.AccumulatedDepreciationSubID;
				DepreciatedExpenseAccountID = asset.DepreciatedExpenseAccountID;
				DepreciatedExpenseSubID = asset.DepreciatedExpenseSubID;
				DisposalAccountID = asset.DisposalAccountID;
				DisposalSubID = asset.DisposalSubID;
				GainAcctID = asset.GainAcctID;
				GainSubID = asset.GainSubID;
				LossAcctID = asset.LossAcctID;
				LossSubID = asset.LossSubID;
			}

			public FAAccounts(FALocationHistory history)
			{
				ClassID = history.ClassID;
				BranchID = history.LocationID;
				FAAccountID = history.FAAccountID;
				FASubID = history.FASubID;
				AccumulatedDepreciationAccountID = history.AccumulatedDepreciationAccountID;
				AccumulatedDepreciationSubID = history.AccumulatedDepreciationSubID;
				DepreciatedExpenseAccountID = history.DepreciatedExpenseAccountID;
				DepreciatedExpenseSubID = history.DepreciatedExpenseSubID;
				DisposalAccountID = history.DisposalAccountID;
				DisposalSubID = history.DisposalSubID;
				GainAcctID = history.GainAcctID;
				GainSubID = history.GainSubID;
				LossAcctID = history.LossAcctID;
				LossSubID = history.LossSubID;
			}

			public void CopyAccountsTo(FALocationHistory location)
			{
				location.ClassID = ClassID;
				location.BranchID = BranchID;
				location.FAAccountID = FAAccountID;
				location.FASubID = FASubID;
				location.AccumulatedDepreciationAccountID = AccumulatedDepreciationAccountID;
				location.AccumulatedDepreciationSubID = AccumulatedDepreciationSubID;
				location.DepreciatedExpenseAccountID = DepreciatedExpenseAccountID;
				location.DepreciatedExpenseSubID = DepreciatedExpenseSubID;
				location.DisposalAccountID = DisposalAccountID;
				location.DisposalSubID = DisposalSubID;
				location.GainAcctID = GainAcctID;
				location.GainSubID = GainSubID;
				location.LossAcctID = LossAcctID;
				location.LossSubID = LossSubID;
			}

			public void CopyMinorAccountsTo(FixedAsset asset)
			{
				asset.DepreciatedExpenseAccountID = DepreciatedExpenseAccountID;
				asset.DepreciatedExpenseSubID = DepreciatedExpenseSubID;
				asset.DisposalAccountID = DisposalAccountID;
				asset.DisposalSubID = DisposalSubID;
				asset.GainAcctID = GainAcctID;
				asset.GainSubID = GainSubID;
				asset.LossAcctID = LossAcctID;
				asset.LossSubID = LossSubID;
			}
		}
	}

	public static class FAHelper
	{
		public static string CloseFABookHistory(this PXGraph graph, FABookBalance bal, IEnumerable<string> periodIDs)
		{
			string maxPeriodID = null;
			foreach (string id in periodIDs.Where(id => !string.IsNullOrEmpty(id)))
			{
				if (maxPeriodID == null)
				{
					graph.EnsureCachePersistence<FABookHist>();
				}

				FABookHist closeHist = new FABookHist
				{
					AssetID = bal.AssetID,
					BookID = bal.BookID,
					FinPeriodID = id,
					Closed = true
				};

				closeHist = (FABookHist)graph.Caches<FABookHist>().Insert(closeHist);

				if (maxPeriodID == null || string.CompareOrdinal(id, maxPeriodID) > 0)
				{
					maxPeriodID = id;
				}
			}
			return maxPeriodID;
		}

		public static string CloseFABookHistory(this PXGraph graph, FABookBalance bal, string toPeriod, bool force = false)
		{
			FABookHistory hist = PXSelect<FABookHistory, 
				Where<FABookHistory.assetID, Equal<Required<FABookBalance.assetID>>, 
					And<FABookHistory.bookID, Equal<Required<FABookBalance.bookID>>,
					And<FABookHistory.closed, NotEqual<True>>>>,
				OrderBy<Asc<FABookHistory.finPeriodID>>>.Select(graph, bal.AssetID, bal.BookID);

			string periodID = force
				? bal.DeprFromPeriod
				: (hist != null && string.CompareOrdinal(hist.FinPeriodID, bal.HistPeriod) > 0
					? hist.FinPeriodID
					: bal.HistPeriod);

			List<string> periodIDs = new List<string>();
			while (!string.IsNullOrEmpty(periodID) && string.CompareOrdinal(periodID, toPeriod) < 0)
			{
				periodIDs.Add(periodID);
				periodID = graph.GetService<IFABookPeriodUtils>().GetNextFABookPeriodID(periodID, bal.BookID, bal.AssetID);
			}
			return graph.CloseFABookHistory(bal, periodIDs);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FALocationHistory GetCurrentLocation(this PXGraph graph, FADetails details)
		{
			FALocationHistory current = PXSelect<FALocationHistory, 
				Where<FALocationHistory.assetID, Equal<Current<FADetails.assetID>>, 
					And<FALocationHistory.revisionID, Equal<Current<FADetails.locationRevID>>>>>
				.SelectSingleBound(graph, new object[] {details});
			return current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FALocationHistory GetPrevLocation(this PXGraph graph, FALocationHistory current)
		{
			FALocationHistory prev = PXSelect<FALocationHistory,
				Where<FALocationHistory.assetID, Equal<Current<FALocationHistory.assetID>>,
					And<FALocationHistory.revisionID, Less<Current<FALocationHistory.revisionID>>>>,
				OrderBy<Desc<FALocationHistory.revisionID>>>
				.SelectSingleBound(graph, new object[] { current });
			return prev;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsLocationChanged(this PXGraph graph, FALocationHistory current, FALocationHistory prev=null)
		{
			prev = prev ?? graph.GetPrevLocation(current);
			return prev != null &&!graph.Caches<FALocationHistory>().ObjectsEqual<
				FALocationHistory.locationID, 
				FALocationHistory.buildingID,
				FALocationHistory.floor, 
				FALocationHistory.room,
				FALocationHistory.employeeID,
				FALocationHistory.department>(prev, current);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAccountsChanged(this PXGraph graph, FALocationHistory current, FALocationHistory prev = null)
		{
			prev = prev ?? graph.GetPrevLocation(current);
			return prev != null && !graph.Caches<FALocationHistory>().ObjectsEqual<
				FALocationHistory.locationID,
				FALocationHistory.fAAccountID,
				FALocationHistory.fASubID,
				FALocationHistory.accumulatedDepreciationAccountID,
				FALocationHistory.accumulatedDepreciationSubID>(prev ?? graph.GetPrevLocation(current), current);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool WillAccountsBeChanged(this PXGraph graph, FixedAsset current, FixedAsset prev,
			out int? fAAccountID,
			out int? fASubID,
			out int? accumulatedDepreciationAccountID,
			out int? accumulatedDepreciationSubID)
		{
			fAAccountID = null;
			fASubID = null;
			accumulatedDepreciationAccountID = null;
			accumulatedDepreciationSubID = null;

			if (graph.Caches<FixedAsset>().ObjectsEqual<
					FixedAsset.classID,
					FixedAsset.fAAccountID,
					FixedAsset.fASubID,
					FixedAsset.accumulatedDepreciationAccountID,
					FixedAsset.accumulatedDepreciationSubID>(prev, current))
				return false;

			if (graph.Caches<FixedAsset>().ObjectsEqual<FixedAsset.classID>(prev, current))
			{
				fAAccountID = current.FAAccountID;
				fASubID = current.FASubID;
				accumulatedDepreciationAccountID = current.AccumulatedDepreciationAccountID;
				accumulatedDepreciationSubID = current.AccumulatedDepreciationSubID;
			}
			else
			{
				var assetCache = graph.Caches[typeof(FixedAsset)];
				var fasetup = (FASetup)graph.Caches[typeof(FASetup)].Current;
				var faclass = AssetProcess.GetSourceForNewAccounts(graph, current);

				fAAccountID = (int?)assetCache.GetValue<FixedAsset.fAAccountID>(faclass);
				accumulatedDepreciationAccountID = (int?)assetCache.GetValue<FixedAsset.accumulatedDepreciationAccountID>(faclass);

				if (fasetup.UpdateGL == true)
				{
					fASubID = AssetMaint.MakeSubID<FixedAsset.fASubMask, FixedAsset.fASubID>(assetCache, current);
					accumulatedDepreciationSubID = AssetMaint.MakeSubID<FixedAsset.accumDeprSubMask, FixedAsset.accumulatedDepreciationSubID>(assetCache, current);
				}
				else
				{
					fASubID = (int?)assetCache.GetValue<FixedAsset.fASubID>(faclass);
					accumulatedDepreciationSubID = (int?)assetCache.GetValue<FixedAsset.accumulatedDepreciationSubID>(faclass);
				}
			}

			return true;
		}
	}
}

namespace PX.Objects.FA.Overrides.AssetProcess
{
	public class FABookHistAccumAttribute : PXAccumulatorAttribute
	{
		public FABookHistAccumAttribute()
			: base( 
			new Type[] {
				typeof(FABookHist.ytdCalculated),
				typeof(FABookHist.ytdBal),
				typeof(FABookHist.ytdBal),
				typeof(FABookHist.ytdDeprBase),
				typeof(FABookHist.ytdDeprBase),
				typeof(FABookHist.ytdBonus),
				typeof(FABookHist.ytdBonusTaken),
				typeof(FABookHist.ytdBonusCalculated),
				typeof(FABookHist.ytdBonusRecap),
				typeof(FABookHist.ytdTax179),
				typeof(FABookHist.ytdTax179Taken),
				typeof(FABookHist.ytdTax179Calculated),
				typeof(FABookHist.ytdTax179Recap),
				typeof(FABookHist.ytdAcquired),
				typeof(FABookHist.ytdDepreciated),
				typeof(FABookHist.ytdDisposalAmount),
				typeof(FABookHist.ytdRGOL),
				typeof(FABookHist.ytdSuspended),
				typeof(FABookHist.ytdReversed),
				typeof(FABookHist.ytdReconciled),
			}, 
			new Type[] {
				typeof(FABookHist.ytdCalculated),
				typeof(FABookHist.begBal),
				typeof(FABookHist.ytdBal),
				typeof(FABookHist.begDeprBase),
				typeof(FABookHist.ytdDeprBase),
				typeof(FABookHist.ytdBonus),
				typeof(FABookHist.ytdBonusTaken),
				typeof(FABookHist.ytdBonusCalculated),
				typeof(FABookHist.ytdBonusRecap),
				typeof(FABookHist.ytdTax179),
				typeof(FABookHist.ytdTax179Taken),
				typeof(FABookHist.ytdTax179Calculated),
				typeof(FABookHist.ytdTax179Recap),
				typeof(FABookHist.ytdAcquired),
				typeof(FABookHist.ytdDepreciated),
				typeof(FABookHist.ytdDisposalAmount),
				typeof(FABookHist.ytdRGOL),
				typeof(FABookHist.ytdSuspended),
				typeof(FABookHist.ytdReversed),
				typeof(FABookHist.ytdReconciled),
			})
		{ 
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			FABookHist hist = (FABookHist)row;

			columns.Update<FABookHist.closed>(hist.Closed,
											  hist.Closed == true || hist.Reopen == true
												  ? PXDataFieldAssign.AssignBehavior.Replace
												  : PXDataFieldAssign.AssignBehavior.Initialize);

			columns.Update<FABookHist.suspended>(hist.Suspended,
												 hist.Suspended == true
													 ? PXDataFieldAssign.AssignBehavior.Replace
													 : PXDataFieldAssign.AssignBehavior.Initialize);

			columns.Update<FABookHist.ytdSuspended>(hist.YtdSuspended, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<FABookHist.ytdReversed>(hist.YtdReversed, PXDataFieldAssign.AssignBehavior.Summarize);

			columns.Update<FABookHist.createdByID>(hist.CreatedByID, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<FABookHist.createdDateTime>(hist.CreatedDateTime, PXDataFieldAssign.AssignBehavior.Initialize);
			columns.Update<FABookHist.createdByScreenID>(hist.CreatedByScreenID, PXDataFieldAssign.AssignBehavior.Initialize);

			columns.Update<FABookHist.lastModifiedByID>(hist.LastModifiedByID, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<FABookHist.lastModifiedDateTime>(hist.LastModifiedDateTime, PXDataFieldAssign.AssignBehavior.Replace);
			columns.Update<FABookHist.lastModifiedByScreenID>(hist.LastModifiedByScreenID, PXDataFieldAssign.AssignBehavior.Replace);

			return true;
		}
	}

	[Serializable()]
	[FABookHistAccum()]
	public partial class FABookHist : FABookHistory
	{
		#region AssetID
		public new abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? AssetID
		{
			get
			{
				return this._AssetID;
			}
			set
			{
				this._AssetID = value;
			}
		}
		#endregion
		#region BookID
		public new abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? BookID
		{
			get
			{
				return this._BookID;
			}
			set
			{
				this._BookID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[PXDBString(6, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public override String FinPeriodID
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
	}
}
