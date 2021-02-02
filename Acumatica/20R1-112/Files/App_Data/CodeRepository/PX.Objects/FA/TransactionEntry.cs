using System;
using PX.Data;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.CS;
using PX.Objects.GL;
using FABookHist = PX.Objects.FA.Overrides.AssetProcess.FABookHist;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.FA
{
	[TableAndChartDashboardType]
	public class TransactionEntry : PXGraph<TransactionEntry, FARegister>
	{
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		[InjectDependency]
		public IFABookPeriodRepository FABookPeriodRepository { get; set; }

		#region Cache Attached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(FATran.tranType.PurchasingPlus)]
		protected virtual void _(Events.CacheAttached<FATran.tranType> e) { }

		[SubAccount(typeof(FATran.creditAccountID), typeof(FATran.branchID), DisplayName = "Credit Subaccount", Visibility = PXUIVisibility.Visible, Filterable = true)]
		protected virtual void FATran_CreditSubID_CacheAttached(PXCache sender) {}

		[SubAccount(typeof(FATran.debitAccountID), typeof(FATran.branchID), DisplayName = "Debit Subaccount", Visibility = PXUIVisibility.Visible, Filterable = true)]
		protected virtual void FATran_DebitSubID_CacheAttached(PXCache sender) {}

		[PXDBScalar(typeof(Search<FixedAsset.depreciable, Where<FixedAsset.assetID, Equal<FATran.assetID>>>))]
		protected virtual void FATran_Depreciable_CacheAttached(PXCache sender) { }
		#endregion

		#region Selects Declaration

		public PXSelect<FARegister> Document;
		public PXSelect<FAAccrualTran> Additions;
		public PXSelect<FATran, Where<FATran.refNbr, Equal<Current<FARegister.refNbr>>>> Trans;
		public PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Optional<FATran.assetID>>>> Asset;
		public PXSelect<FADetails, Where<FADetails.assetID, Equal<Optional<FATran.assetID>>>> assetdetails;
		public PXSelect<FABookBalance, 
			Where<FABookBalance.assetID, Equal<Optional<FABookBalance.assetID>>, 
				And<FABookBalance.bookID, Equal<Optional<FABookBalance.bookID>>>>> bookbalances;
		public PXSelect<FABookHist> bookhistory;
		public PXSelect<FALocationHistory,
			Where<FALocationHistory.assetID, Equal<Optional<FATran.assetID>>>,
			OrderBy<Desc<FALocationHistory.revisionID>>> locationHistory; 
		public PXSetup<FASetup> fasetup;
		#endregion

		#region State

		readonly DocumentList<FARegister> _created;
		public DocumentList<FARegister> created
		{
			get
			{
				return _created;
			}
		}

		readonly Dictionary<string, bool> _debit_enable = new Dictionary<string, bool>
																{
																	{FATran.tranType.PurchasingPlus,    false},
																	{FATran.tranType.PurchasingMinus,   true},
																	{FATran.tranType.DepreciationPlus,  true},
																	{FATran.tranType.DepreciationMinus,	false},
																	{FATran.tranType.CalculatedPlus,	true},
																	{FATran.tranType.CalculatedMinus,	false},
																	{FATran.tranType.SalePlus,          true},
																	{FATran.tranType.SaleMinus,         false},
																};

		public bool UpdateGL
		{
			get
			{
				return fasetup.Current.UpdateGL == true;
			}
		}
		#endregion

		#region Ctor
		public TransactionEntry()
		{
			FASetup setup = fasetup.Current;
			_created = new DocumentList<FARegister>(this);
		}
		#endregion

		#region Runtime

		public static void SegregateRegister(PXGraph graph, int BranchID, string Origin, string PeriodID, DateTime? DocDate, string descr, DocumentList<FARegister> created)
		{
			PXCache doccache = graph.Caches[typeof(FARegister)];
			PXCache trancache = graph.Caches[typeof(FATran)];

			if (trancache.IsInsertedUpdatedDeleted)
			{
				graph.Actions.PressSave();
				if (doccache.Current != null && created.Find(doccache.Current) == null)
			{
					created.Add((FARegister)doccache.Current);
				}
				graph.Clear();
			}

			FARegister register = created.Find<FARegister.branchID, FARegister.origin, FARegister.finPeriodID>(BranchID, Origin, PeriodID) ?? new FARegister();

			if (register.RefNbr != null)
			{
				FARegister newreg = PXSelect<FARegister, Where<FARegister.refNbr, Equal<Current<FARegister.refNbr>>>>.SelectSingleBound(graph, new object[]{register});
				if (newreg.DocDesc != descr)
				{
					newreg.DocDesc = string.Empty;
					doccache.Update(newreg);
				}
				doccache.Current = newreg;
			}
			else
			{
				graph.Clear();

				register = new FARegister
				{
					Hold = false,
					BranchID = BranchID,
					Origin = Origin,
					FinPeriodID = PeriodID,
					DocDate = DocDate,
					DocDesc = descr
				};
				doccache.Insert(register);
			}
		}

		public override void Persist()
		{
			base.Persist();

			if(Document.Current != null)
			{
				FARegister existed = created.Find(Document.Current);
				if (existed == null)
				{
					created.Add(Document.Current);
				}
				else
				{
					Document.Cache.RestoreCopy(existed, Document.Current);
				}
			}
		}
		#endregion

		#region Funcs
		protected virtual void DefaultingAccSub(PXFieldDefaultingEventArgs e, Dictionary<String, Int32?> accs)
		{
			var trn = (FATran)(e.Row);
			if (trn == null || trn.AssetID == null || trn.TranType == null) return;
			try
			{
				e.NewValue = accs[trn.TranType];
				e.Cancel = true;
			}
			catch (KeyNotFoundException) { }
		}

		protected virtual void DefaultingAllAccounts(PXCache sender, FATran trn)
		{
			sender.SetDefaultExt<FATran.debitAccountID>(trn);
			sender.SetDefaultExt<FATran.debitSubID>(trn);
			sender.SetDefaultExt<FATran.creditAccountID>(trn);
			sender.SetDefaultExt<FATran.creditSubID>(trn);
		}

		protected virtual void SetCurrentAsset(FATran trn)
		{
			if (Asset.Current == null || Asset.Current.AssetID != trn.AssetID)
			{
				Asset.Current = Asset.SelectSingle(trn.AssetID);
			}
			if (assetdetails.Current == null || assetdetails.Current.AssetID != trn.AssetID)
			{
				assetdetails.Current = assetdetails.SelectSingle(trn.AssetID);
			}
		}

		protected virtual void ToggleAccounts(PXCache sender, FATran trn)
		{
			if (trn != null && trn.TranType != null)
			{
				bool enable;
				if (_debit_enable.TryGetValue(trn.TranType, out enable))
				{
					PXUIFieldAttribute.SetEnabled<FATran.creditAccountID>(sender, trn, !enable);
					PXUIFieldAttribute.SetEnabled<FATran.creditSubID>(sender, trn, !enable);
					PXUIFieldAttribute.SetEnabled<FATran.debitAccountID>(sender, trn, enable);
					PXUIFieldAttribute.SetEnabled<FATran.debitSubID>(sender, trn, enable);
				}
				if(trn.TranType == FATran.tranType.PurchasingPlus)
				{
					PXUIFieldAttribute.SetEnabled<FATran.creditAccountID>(sender, trn, false);
					PXUIFieldAttribute.SetEnabled<FATran.creditSubID>(sender, trn, false);
					PXUIFieldAttribute.SetEnabled<FATran.debitAccountID>(sender, trn, false);
					PXUIFieldAttribute.SetEnabled<FATran.debitSubID>(sender, trn, false);
				}
			}
		}
		#endregion

		#region Events
		protected virtual void FATran_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void FATran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			FATran tran = (FATran) e.Row;
			e.Cancel |= tran.TranAmt == 0m && (!string.IsNullOrEmpty(tran.MethodDesc) || tran.TranType != "C+" && tran.TranType != "P+" && tran.TranType != "S+" && tran.TranType != "S-" && tran.Origin != FARegister.origin.Adjustment);

			SetCurrentAsset(tran);
			tran.Depreciable = Asset.Current?.Depreciable;
		}

		protected virtual void FATran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FATran trn = (FATran)e.Row;
			if (trn == null) return;

			if (sender.AllowUpdate && trn.Origin != FARegister.origin.Adjustment)
			{
				PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<FATran.tranDesc>(sender, e.Row, true);
			}

			ToggleAccounts(sender, trn);

			FARegister reg = Document.Current;

			if (reg == null || trn.Depreciable == null) return;

			if (reg.Origin == FARegister.origin.Adjustment)
			{
				if (trn.Depreciable == true)
					PXStringListAttribute.SetList<FATran.tranType>(Trans.Cache, trn, new FATran.tranType.AdjustmentListAttribute().AllowedValues, new FATran.tranType.AdjustmentListAttribute().AllowedLabels);
				else
					PXStringListAttribute.SetList<FATran.tranType>(Trans.Cache, trn, new FATran.tranType.NonDepreciableListAttribute().AllowedValues, new FATran.tranType.NonDepreciableListAttribute().AllowedLabels);
			}

			Trans.Cache.AllowInsert = reg.Origin == FARegister.origin.Adjustment;
		}

		protected virtual void FATran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			FATran tran = e.Row as FATran;
			if (tran != null && tran.TranType == FATran.tranType.TransferPurchasing && tran.Released != true)
			{
				SetCurrentAsset(tran);
				FixedAsset asset = PXCache<FixedAsset>.CreateCopy(Asset.Current);
				FADetails details = PXCache<FADetails>.CreateCopy(assetdetails.Current);

				FALocationHistory lastLocation = locationHistory.SelectSingle(tran.AssetID);
				locationHistory.Delete(lastLocation);

				FALocationHistory restoringLocation = locationHistory.SelectSingle(tran.AssetID);
				if (restoringLocation == null)
				{
					throw new PXException(Messages.PrevLocationRevisionNotFound, asset.AssetID);
				}

				details.LocationRevID = restoringLocation.RevisionID;
				assetdetails.Update(details);

				asset.ClassID = restoringLocation.ClassID ?? asset.ClassID;
				asset.BranchID = restoringLocation.BranchID ?? asset.BranchID;
				asset.FAAccountID = restoringLocation.FAAccountID ?? asset.FAAccountID;
				asset.FASubID = restoringLocation.FASubID ?? asset.FASubID;
				asset.AccumulatedDepreciationAccountID = restoringLocation.AccumulatedDepreciationAccountID ?? asset.AccumulatedDepreciationAccountID;
				asset.AccumulatedDepreciationSubID = restoringLocation.AccumulatedDepreciationSubID ?? asset.AccumulatedDepreciationSubID;
				asset.DepreciatedExpenseAccountID = restoringLocation.DepreciatedExpenseAccountID ?? asset.DepreciatedExpenseAccountID;
				asset.DepreciatedExpenseSubID = restoringLocation.DepreciatedExpenseSubID ?? asset.DepreciatedExpenseSubID;
				asset.DisposalAccountID = restoringLocation.DisposalAccountID;
				asset.DisposalSubID = restoringLocation.DisposalSubID;
				asset.GainAcctID = restoringLocation.GainAcctID ?? asset.GainAcctID;
				asset.GainSubID = restoringLocation.GainSubID ?? asset.GainSubID;
				asset.LossAcctID = restoringLocation.LossAcctID ?? asset.LossAcctID;
				asset.LossSubID = restoringLocation.LossSubID ?? asset.LossSubID;
				Asset.Update(asset);
			}

			if (tran != null && tran.TranType == FATran.tranType.PurchasingPlus && tran.Released != true)
			{
				var lastLocation = locationHistory.SelectSingle(tran.AssetID);
				if (lastLocation?.RefNbr != null)
				{
					lastLocation.RefNbr = null;
					locationHistory.Update(lastLocation);
				}
			}
		}

		protected virtual void FATran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var tran = (FATran)e.Row;
			if (tran != null
				&& tran.Origin != FARegister.origin.Reversal
				&& (tran.TranType == FATran.tranType.DepreciationMinus
					|| tran.TranType == FATran.tranType.DepreciationPlus))
			{
				AccountAttribute.VerifyAccountIsNotControl<FATran.debitAccountID>(sender, e);
			}
		}

		protected virtual void FATran_AssetID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FATran trn = (FATran)e.Row;
			if (trn == null) return;

			SetCurrentAsset(trn);
			sender.SetDefaultExt<FATran.branchID>(trn);
			DefaultingAllAccounts(sender, trn);
			trn.Depreciable = Asset.Current?.Depreciable;
		}

		protected virtual void FATran_BookID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<FATran.finPeriodID>(e.Row);
		}

		protected virtual void FATran_TranType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DefaultingAllAccounts(sender, (FATran)e.Row);

			object newValue = ((FATran)e.Row).FinPeriodID;
			try
			{
				sender.RaiseFieldVerifying<FATran.finPeriodID>(e.Row, ref newValue);
			}
			catch (PXSetPropertyException ex)
			{
				sender.SetValue<FATran.finPeriodID>(e.Row, null);
				sender.RaiseExceptionHandling<FATran.finPeriodID>(e.Row, newValue, ex);
			}
		}

		protected bool IsDefaulting;

		protected virtual void FATran_FinPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			IsDefaulting = true;
		}

		protected virtual void FATran_FinPeriodID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			try
			{
				if (e.Row != null && IsDefaulting)
				{
					object newValue = FinPeriodIDFormattingAttribute.FormatForStoring((string)e.NewValue);
					sender.RaiseFieldVerifying<FATran.finPeriodID>(e.Row, ref newValue);
				}
			}
			finally
			{
				IsDefaulting = false;
			}
		}

		protected virtual void FATran_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FATran tran = (FATran) e.Row;
			if(tran?.AssetID == null || tran.BookID == null || tran.TranDate == null) return;

			try
			{
				FABookPeriod period = PXSelect<
					FABookPeriod,
					Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
						And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
						And<FABookPeriod.finPeriodID, Equal<Required<FABookPeriod.finPeriodID>>>>>>
					.Select(this, tran.BookID, FABookPeriodRepository.GetFABookPeriodOrganizationID(tran.BookID, tran.AssetID), (string)e.NewValue);
				if (period == null)
				{
					throw new PXSetPropertyException(Messages.NoPeriodsDefined);
				}

				FABookBalance bal = bookbalances.SelectSingle(tran.AssetID, tran.BookID);


				if ((tran.TranType == FATran.tranType.DepreciationPlus || tran.TranType == FATran.tranType.DepreciationMinus) && tran.Origin == FARegister.origin.Adjustment)
				{
					if (!string.IsNullOrEmpty(bal.CurrDeprPeriod) && string.CompareOrdinal((string)e.NewValue, bal.CurrDeprPeriod) >= 0)
					{
						throw new PXSetPropertyException(CS.Messages.Entry_LT, FinPeriodIDFormattingAttribute.FormatForError(bal.CurrDeprPeriod));
					}
					if (!string.IsNullOrEmpty(bal.LastDeprPeriod) && string.CompareOrdinal((string)e.NewValue, bal.LastDeprPeriod) > 0)
					{
						throw new PXSetPropertyException(CS.Messages.Entry_LE, FinPeriodIDFormattingAttribute.FormatForError(bal.LastDeprPeriod));
					}
					if(!string.IsNullOrEmpty(bal.DeprFromPeriod) && string.CompareOrdinal((string)e.NewValue, bal.DeprFromPeriod) < 0)
					{
						throw new PXSetPropertyException(CS.Messages.Entry_GE, FinPeriodIDFormattingAttribute.FormatForError(bal.DeprFromPeriod));
					}
				}
			}
			catch (PXSetPropertyException)
			{
				e.NewValue = FinPeriodIDAttribute.FormatForDisplay((string)e.NewValue);
				throw;
			}
		}

		protected virtual void FATran_DebitAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Asset.Current == null) return;
			var accs = new Dictionary<String, Int32?>
						{
							{FATran.tranType.PurchasingPlus, Asset.Current.FAAccountID},
							{FATran.tranType.PurchasingMinus, Asset.Current.FAAccrualAcctID},
							{FATran.tranType.DepreciationPlus, Asset.Current.DepreciatedExpenseAccountID},
							{FATran.tranType.DepreciationMinus, Asset.Current.AccumulatedDepreciationAccountID},
							{FATran.tranType.CalculatedPlus, Asset.Current.DepreciatedExpenseAccountID},
							{FATran.tranType.CalculatedMinus, Asset.Current.AccumulatedDepreciationAccountID},
							{FATran.tranType.AdjustingDeprPlus, Asset.Current.DepreciatedExpenseAccountID},
							{FATran.tranType.AdjustingDeprMinus, Asset.Current.AccumulatedDepreciationAccountID},
							{FATran.tranType.ReconciliationPlus, Asset.Current.FAAccrualAcctID},
							{FATran.tranType.ReconciliationMinus, Asset.Current.FAAccrualAcctID},
							{FATran.tranType.PurchasingReversal, Asset.Current.FAAccountID},
						};

			DefaultingAccSub(e, accs);
		}

		protected virtual void FATran_DebitAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var tran = (FATran)e.Row;
			if (tran != null
				&& e.ExternalCall 
				&& (tran.TranType == FATran.tranType.DepreciationMinus 
					|| tran.TranType == FATran.tranType.DepreciationPlus))
			{
				AccountAttribute.VerifyAccountIsNotControl<FATran.debitAccountID>(sender, e);
			}
		}

		protected virtual void FATran_DebitSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Asset.Current == null) return;
			var accs = new Dictionary<String, Int32?>
						{
							{FATran.tranType.PurchasingPlus, Asset.Current.FASubID},
							{FATran.tranType.PurchasingMinus, Asset.Current.FAAccrualSubID},
							{FATran.tranType.DepreciationPlus, Asset.Current.DepreciatedExpenseSubID},
							{FATran.tranType.DepreciationMinus, Asset.Current.AccumulatedDepreciationSubID},
							{FATran.tranType.CalculatedPlus, Asset.Current.DepreciatedExpenseSubID},
							{FATran.tranType.CalculatedMinus, Asset.Current.AccumulatedDepreciationSubID},
							{FATran.tranType.AdjustingDeprPlus, Asset.Current.DepreciatedExpenseSubID},
							{FATran.tranType.AdjustingDeprMinus, Asset.Current.AccumulatedDepreciationSubID},
							{FATran.tranType.ReconciliationPlus, Asset.Current.FAAccrualSubID},
							{FATran.tranType.ReconciliationMinus, Asset.Current.FAAccrualSubID},
							{FATran.tranType.PurchasingReversal, Asset.Current.FASubID},
						};

			DefaultingAccSub(e, accs);
		}

		protected virtual void FATran_CreditAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Asset.Current == null) return;
			var accs = new Dictionary<String, Int32?>
						{
							{FATran.tranType.PurchasingPlus, Asset.Current.FAAccrualAcctID},
							{FATran.tranType.PurchasingMinus, Asset.Current.FAAccountID},
							{FATran.tranType.DepreciationPlus, Asset.Current.AccumulatedDepreciationAccountID},
							{FATran.tranType.DepreciationMinus, Asset.Current.DepreciatedExpenseAccountID},
							{FATran.tranType.CalculatedPlus, Asset.Current.AccumulatedDepreciationAccountID},
							{FATran.tranType.CalculatedMinus, Asset.Current.DepreciatedExpenseAccountID},
							{FATran.tranType.AdjustingDeprPlus, Asset.Current.AccumulatedDepreciationAccountID},
							{FATran.tranType.AdjustingDeprMinus, Asset.Current.DepreciatedExpenseAccountID},
							{FATran.tranType.ReconciliationPlus, Asset.Current.FAAccrualAcctID},
							{FATran.tranType.ReconciliationMinus, Asset.Current.FAAccrualAcctID},
							{FATran.tranType.PurchasingReversal, Asset.Current.FAAccrualAcctID},
						};

			DefaultingAccSub(e, accs);
		}

		protected virtual void FATran_CreditSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Asset.Current == null) return;
			var accs = new Dictionary<String, Int32?>
						{
							{FATran.tranType.PurchasingPlus, Asset.Current.FAAccrualSubID},
							{FATran.tranType.PurchasingMinus, Asset.Current.FASubID},
							{FATran.tranType.DepreciationPlus, Asset.Current.AccumulatedDepreciationSubID},
							{FATran.tranType.DepreciationMinus, Asset.Current.DepreciatedExpenseSubID},
							{FATran.tranType.CalculatedPlus, Asset.Current.AccumulatedDepreciationSubID},
							{FATran.tranType.CalculatedMinus, Asset.Current.DepreciatedExpenseSubID},
							{FATran.tranType.AdjustingDeprPlus, Asset.Current.AccumulatedDepreciationSubID},
							{FATran.tranType.AdjustingDeprMinus, Asset.Current.DepreciatedExpenseSubID},
							{FATran.tranType.ReconciliationPlus, Asset.Current.FAAccrualSubID},
							{FATran.tranType.ReconciliationMinus, Asset.Current.FAAccrualSubID},
							{FATran.tranType.PurchasingReversal, Asset.Current.FAAccrualSubID},
						};

			DefaultingAccSub(e, accs);
		}

		protected virtual void FARegister_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var reg = (FARegister)(e.Row);
			if (reg == null) return;

			if (reg.Released == true)
			{
				PXUIFieldAttribute.SetEnabled(cache, reg, false);
				cache.AllowDelete = false;
				cache.AllowUpdate = false;
				Trans.Cache.AllowDelete = false;
				Trans.Cache.AllowUpdate = false;
				Trans.Cache.AllowInsert = false;
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, reg, true);
				PXUIFieldAttribute.SetEnabled<FARegister.status>(cache, reg, false);
				cache.AllowDelete = true;
				cache.AllowUpdate = true;
				Trans.Cache.AllowDelete = true;
				Trans.Cache.AllowUpdate = true;
				Trans.Cache.AllowInsert = true;
			}

			PXUIFieldAttribute.SetEnabled<FARegister.origin>(cache, reg, false);
			PXUIFieldAttribute.SetVisible<FARegister.reason>(cache, reg, reg.Origin == FARegister.origin.Disposal || reg.Origin == FARegister.origin.Transfer);
		}

		protected virtual void FARegister_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null && PXLongOperation.GetCurrentItem() == null) //Calculate totals only not in long operation
			{
				using (new PXConnectionScope())
				{
					PXFormulaAttribute.CalcAggregate<FATran.tranAmt>(Trans.Cache, e.Row, true);
				}
			}
		}

		protected virtual void FARegister_DocDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FARegister reg = e.Row as FARegister;
			if (reg == null || reg.Origin != FARegister.origin.Adjustment) return;
			
			PXCache cache = sender.Graph.Caches<FATran>();
			foreach (FATran tran in PXParentAttribute.SelectSiblings(cache, null, typeof (FARegister)).OfType<FATran>().Select(PXCache<FATran>.CreateCopy))
			{
				tran.TranDate = reg.DocDate;
				cache.Update(tran);
			}
		}
	
		#endregion

		#region Buttons
		public PXAction<FARegister> release;
		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<FARegister> list = new List<FARegister>();
			foreach (FARegister fadoc in adapter.Get<FARegister>().Where(fadoc => fadoc.Hold != true && fadoc.Released != true))
			{
				cache.Update(fadoc);
				list.Add(fadoc);
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			Save.Press();
			PXLongOperation.StartOperation(this, () => AssetTranRelease.ReleaseDoc(list, false));
			return list;
		}

		public PXAction<FARegister> viewBatch;
		[PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			if (Trans.Current != null)
			{
				JournalEntry graph = CreateInstance<JournalEntry>();
				graph.BatchModule.Current = graph.BatchModule.Search<Batch.batchNbr>(Trans.Current.BatchNbr, BatchModule.FA);
				if (graph.BatchModule.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, "ViewBatch") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<FARegister> viewAsset;
		[PXUIField(DisplayName = Messages.ViewAsset, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewAsset(PXAdapter adapter)
		{
			if (Trans.Current != null)
			{
				AssetMaint graph = CreateInstance<AssetMaint>();
				graph.CurrentAsset.Current = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FATran.assetID>>>>.Select(this);
				if (graph.CurrentAsset.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, "ViewAsset") { Mode = PXBaseRedirectException.WindowMode.Same };
				}
			}
			return adapter.Get();
		}

		public PXAction<FARegister> viewBook;
		[PXUIField(DisplayName = Messages.ViewBook, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBook(PXAdapter adapter)
		{
			if (Trans.Current != null)
			{
				BookMaint graph = CreateInstance<BookMaint>();
				graph.Book.Current = PXSelect<FABook, Where<FABook.bookID, Equal<Current<FATran.bookID>>>>.Select(this);
				if (graph.Book.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, "ViewBook") { Mode = PXBaseRedirectException.WindowMode.Same };
				}
			}
			return adapter.Get();
		}
		#endregion
	}
}