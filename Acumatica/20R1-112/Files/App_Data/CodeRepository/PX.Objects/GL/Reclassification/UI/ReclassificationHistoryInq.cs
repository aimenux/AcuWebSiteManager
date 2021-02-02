using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Data;

namespace PX.Objects.GL.Reclassification.UI
{
	public class ReclassificationHistoryInq : PXGraph<ReclassificationHistoryInq>
	{
        public PXAction<GLTranReclHist> reclassify;
        public PXAction<GLTranReclHist> reclassifyAll;
        public PXAction<GLTranReclHist> reclassificationHistory;
        public PXAction<GLTranReclHist> viewBatch;
        public PXAction<GLTranReclHist> viewOrigBatch;

        public PXSelectOrderBy<GLTranReclHist,
								OrderBy<Asc<GLTranReclHist.sortOrder,
                                        Asc<GLTranReclHist.lineNbr>>>> TransView;

        public PXSelect<GLTran> CurrentReclassTranView;
        public PXSelect<GLTranKey> SrcOfReclassTranView;

		public ReclassificationHistoryInq()
		{
			TransView.AllowDelete = false;
			TransView.AllowInsert = false;
			TransView.AllowUpdate = true;

			PXUIFieldAttribute.SetVisible<GLTranReclHist.batchNbr>(TransView.Cache, null);

            //to hide the red asterisk on column header
            PXDefaultAttribute.SetPersistingCheck<GLTranReclHist.branchID>(TransView.Cache, null, PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<GLTranReclHist.accountID>(TransView.Cache, null, PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<GLTranReclHist.subID>(TransView.Cache, null, PXPersistingCheck.Nothing);

            var srcTranKey = SrcTranOfReclassKey();

            if (srcTranKey == null)
                return;

            var srcTran = (GLTranReclHist)PXSelect<GLTranReclHist,
                                Where<GLTranReclHist.module, Equal<Required<GLTran.module>>,
                                        And<GLTranReclHist.batchNbr, Equal<Required<GLTran.batchNbr>>,
                                        And<GLTranReclHist.lineNbr, Equal<Required<GLTran.lineNbr>>>>>>
                                .Select(this,
                                    srcTranKey.Module,
                                    srcTranKey.BatchNbr,
                                    srcTranKey.LineNbr);

            TransView.Cache.SetStatus(srcTran, PXEntryStatus.Updated);

            var hasSplittedTrans = PXSelectReadonly<GLTranReclHist,
                       Where<GLTranReclHist.reclassSourceTranModule, Equal<Required<GLTranReclHist.module>>,
                                   And<GLTranReclHist.reclassSourceTranBatchNbr, Equal<Required<GLTranReclHist.batchNbr>>,
                                   And<GLTranReclHist.reclassSourceTranLineNbr, Equal<Required<GLTranReclHist.lineNbr>>>>>, OrderBy<Asc<GLTranReclHist.reclassSeqNbr>>>
                       .Select(this, srcTran.Module, srcTran.BatchNbr, srcTran.LineNbr).RowCast<GLTranReclHist>().Any(m => m.ReclassType == ReclassType.Split);

            reclassifyAll.SetEnabled(hasSplittedTrans);
            reclassifyAll.SetVisible(hasSplittedTrans);
            reclassificationHistory.SetEnabled(hasSplittedTrans);
            reclassificationHistory.SetVisible(hasSplittedTrans);
            PXUIFieldAttribute.SetVisible<GLTranReclHist.selected>(TransView.Cache, null, hasSplittedTrans);
            PXUIFieldAttribute.SetVisible<GLTranReclHist.origBatchNbr>(TransView.Cache, null, hasSplittedTrans);
            PXUIFieldAttribute.SetVisible<GLTranReclHist.actionDesc>(TransView.Cache, null, hasSplittedTrans);
        }

		protected GLTranKey SrcTranOfReclassKey()
		{
			return SrcOfReclassTranView.Cache.Inserted.Select<GLTranKey>().SingleOrDefault(); 
		}

        protected GLTranKey CurrentNodeOfReclass()
        {
            GLTran currentTran = CurrentReclassTranView.Cache.Inserted.Select<GLTran>().SingleOrDefault();
            
            if(currentTran.IsReclassReverse == true && currentTran.LineNbr % 2 != 0)
            {
                currentTran.LineNbr += 1;
            }

            return new GLTranKey(currentTran);
        }

        protected bool IsCurrentTran(GLTranReclHist tran)
        {
            var currentTran = CurrentReclassTranView.Cache.Inserted.Select<GLTran>().SingleOrDefault();

            return tran.Module == currentTran.Module && tran.BatchNbr == currentTran.BatchNbr && tran.LineNbr == currentTran.LineNbr;
        }

        protected virtual IEnumerable transView()
		{
            TransView.Cache.Clear();

            var srcTranKey = SrcTranOfReclassKey();

			if (srcTranKey == null)
				return new GLTranReclHist[0];

			var srcTran = (GLTranReclHist)PXSelect<GLTranReclHist,
								Where<GLTranReclHist.module, Equal<Required<GLTran.module>>,
										And<GLTranReclHist.batchNbr, Equal<Required<GLTran.batchNbr>>,
										And<GLTranReclHist.lineNbr, Equal<Required<GLTran.lineNbr>>>>>>
								.Select(this,
									srcTranKey.Module,
									srcTranKey.BatchNbr,
									srcTranKey.LineNbr);

            TransView.Cache.SetStatus(srcTran, PXEntryStatus.Updated);

            var currentNodeKey = CurrentNodeOfReclass();

            var res = PXSelect<GLTranReclHist,
                        Where<GLTranReclHist.reclassSourceTranModule, Equal<Required<GLTranReclHist.module>>,
                                    And<GLTranReclHist.reclassSourceTranBatchNbr, Equal<Required<GLTranReclHist.batchNbr>>,
                                    And<GLTranReclHist.reclassSourceTranLineNbr, Equal<Required<GLTranReclHist.lineNbr>>>>>, OrderBy<Asc<GLTranReclHist.reclassSeqNbr>>>
                        .Select(this, srcTran.Module, srcTran.BatchNbr, srcTran.LineNbr);

			bool hasSplitsInChain = false;

			foreach (GLTranReclHist tran in res)
            {
                tran.ParentTran = null;
                tran.ChildTrans = new List<GLTranReclHist>();
                TransView.Cache.SetStatus(tran, PXEntryStatus.Updated);
			}

            foreach (GLTranReclHist tran in res)
            {
                if (tran.IsReclassReverse == true)
                {
                    continue;
                }

                var parent = (GLTranReclHist)TransView.Cache.Locate(new GLTranReclHist(tran.OrigModule, tran.OrigBatchNbr, tran.OrigLineNbr));

                if(parent == null)
                {
                    continue;
                }

                tran.ParentTran = parent;
                parent.ChildTrans.Add(tran);
				hasSplitsInChain |= tran.ReclassType == ReclassType.Split;
				tran.IsSplited = parent.IsSplited == true || tran.ReclassType == ReclassType.Split;
			}

            GLTranReclHist currentTran;
            if (srcTranKey.Equals(currentNodeKey))
            {
                currentTran = srcTran;
            }
            else
            {
                currentTran = (GLTranReclHist)TransView.Cache.Locate(new GLTranReclHist(currentNodeKey.Module, currentNodeKey.BatchNbr, currentNodeKey.LineNbr));
            }

            currentTran.SortOrder = 0;
            currentTran.IsCurrent = true;

            var releasedReversingBatches = JournalEntry.GetReleasedReversingBatches(this, srcTran.Module, srcTran.BatchNbr);

			if (releasedReversingBatches.Any())
			{
				TransView.Cache.RaiseExceptionHandling<GLTranReclHist.batchNbr>(srcTran, null,
					new PXSetPropertyException(Messages.BatchOfTranHasBeenReversed, PXErrorLevel.RowWarning));
			}

            var result = new List<GLTranReclHist>();

            BuildParentList(currentTran, result);
            SortChildTrans(currentTran, hasSplitsInChain);

            result.Add(currentTran);
            if (!srcTranKey.Equals(currentTran))
            {
                var reverseCurrentTran = (GLTranReclHist)TransView.Cache.Locate(new GLTranReclHist(currentNodeKey.Module, currentNodeKey.BatchNbr, currentNodeKey.LineNbr - 1));
                reverseCurrentTran.SortOrder = 0;
                result.Add(reverseCurrentTran);
            }
            result.AddRange(TransView.Cache.Updated.RowCast<GLTranReclHist>().Where(m => m.ReclassSeqNbr > (currentTran.ReclassSeqNbr ?? 0) && m.SortOrder.HasValue));

            PXUIFieldAttribute.SetVisible<GLTranReclHist.curyReclassRemainingAmt>(TransView.Cache, null, result.Any(m => m.CuryReclassRemainingAmt > 0m));

            foreach (var item in result)
            {
                if(srcTranKey.Equals(item))
                {
                    continue;
                }
                if (item.IsParent == true)
                {
                    item.SplitIcon = SplitIcon.Parent;
                }

				if(item.IsSplited == true)
				{
					item.SplitIcon = SplitIcon.Split;
				}

                if (item.ReclassType == ReclassType.Split) // || item.ParentTran?.ReclassType == ReclassType.Split)
                {
                    item.ActionDesc = GL.ReclassType.Split;
                }
                else
                {
                    item.ActionDesc = GL.ReclassType.Common;
                }
            }

            return result;
		}

        protected virtual int? SortChildTrans(GLTranReclHist tran, bool hasSplitsInChain)
        {
            bool isNotIncluded = (tran.ReclassRemainingAmt ?? 0m) == 0m && hasSplitsInChain && !IsCurrentTran(tran);

            var reverseTran = (GLTranReclHist)TransView.Cache.Locate(new GLTranReclHist(tran.Module, tran.BatchNbr, tran.LineNbr - 1));

            if (reverseTran != null)
            {
                reverseTran.SortOrder = tran.SortOrder;
                reverseTran.ParentTran = tran.ParentTran;
				reverseTran.IsSplited = tran.IsSplited;
			}
            else if (tran.ParentTran != null)
            {
                throw new PXException(Messages.ReversingEntryHasNotBeenFound);
            }

            if (!tran.ChildTrans.Any())
            {
                return tran.SortOrder;
            }

            int? lastSortOrder = tran.SortOrder;
            bool splitsDetected = false;

            foreach (GLTranReclHist lowerTran in tran.ChildTrans.Where(m => m.ReclassType == ReclassType.Split))
            {
                lastSortOrder += 1;
                lowerTran.SortOrder = lastSortOrder;
                splitsDetected |= lowerTran.ReclassType == ReclassType.Split;
                lastSortOrder = SortChildTrans(lowerTran, hasSplitsInChain);
            }

			foreach (GLTranReclHist lowerTran in tran.ChildTrans.Where(m => m.ReclassType == ReclassType.Common))
			{
				lastSortOrder += 1;
				lowerTran.SortOrder = lastSortOrder;
				splitsDetected |= lowerTran.ReclassType == ReclassType.Split;
				lastSortOrder = SortChildTrans(lowerTran, hasSplitsInChain);
			}

			if (isNotIncluded)
            {
                tran.SortOrder = null;
				reverseTran.SortOrder = null;
			}

            tran.IsParent = splitsDetected;

            return lastSortOrder;
        }

        protected virtual void BuildParentList(GLTranReclHist tran, List<GLTranReclHist> result)
        {
            if (tran.ParentTran == null)
            {
                return;
            }

            tran.ParentTran.SortOrder = tran.SortOrder - 1;
            tran.ParentTran.IsParent = tran.ReclassType == ReclassType.Split;

            BuildParentList(tran.ParentTran, result);
            
            result.Add(tran.ParentTran);

            var reverseTran = (GLTranReclHist)TransView.Cache.Locate(new GLTranReclHist(tran.ParentTran.Module, tran.ParentTran.BatchNbr, tran.ParentTran.LineNbr -1));

            if (reverseTran == null)
            {
                return;
            }

            reverseTran.SortOrder = tran.ParentTran.SortOrder;
			reverseTran.IsSplited = tran.ParentTran?.IsSplited == true;
			result.Add(reverseTran);
        }

        public override bool IsDirty
		{
			get { return false; }
		}

        [PXUIField(DisplayName = Messages.Reclassify, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable Reclassify(PXAdapter adapter)
		{
            var trans = TransView.Cache.Updated.RowCast<GLTranReclHist>();

            if (trans == null)
            {
                return new GLTran[0];
            }

            if(trans.Any(m => m.ReclassType == ReclassType.Split))
            {
                var selectedTrans = trans.Where(m => m.Selected == true);

                if (selectedTrans.Count() == 0)
                {
                    return adapter.Get();
                }

                ReclassifyTransactionsProcess.OpenForReclassification(selectedTrans.ToList().AsReadOnly(), PXBaseRedirectException.WindowMode.New);

                return adapter.Get();
            }

            var tran = trans.LastOrDefault();

            if (tran.Released == false)
            {
                throw new PXException(Messages.TheTransactionCannotBeReclassifiedBecauseItIsNotReleased);
            }

            ReclassifyTransactionsProcess.OpenForReclassification(new[] { tran }, PXBaseRedirectException.WindowMode.New);

            return adapter.Get();
        }

        [PXUIField(DisplayName = Messages.ReclassifyAll, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable ReclassifyAll(PXAdapter adapter)
        {
            var trans = TransView.Cache.Updated.RowCast<GLTranReclHist>().Where(m => m.SortOrder.HasValue);

            if (trans == null)
            {
                TransView.Cache.Clear();
                return adapter.Get();
            }

			ReclassifyTransactionsProcess.TryOpenForReclassification<GLTran>(trans,
				BatchTypeCode.Normal,
				TransView.View,
				InfoMessages.SomeTransactionsOfTheBatchCannotBeReclassified,
				InfoMessages.NoReclassifiableTransactionsHaveBeenFoundInTheBatch,
				PXBaseRedirectException.WindowMode.New);

			return adapter.Get();
        }

        [PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			var tran = TransView.Current;

			if (tran != null)
			{
				JournalEntry.RedirectToBatch(this, tran.Module, tran.BatchNbr);
			}

			return adapter.Get();
		}

        [PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewOrigBatch(PXAdapter adapter)
        {
            var tran = TransView.Current;

            if (tran != null)
            {
                JournalEntry.RedirectToBatch(this, tran.OrigModule, tran.OrigBatchNbr);
            }

            return adapter.Get();
        }

        [PXUIField(DisplayName = Messages.ReclassificationHistory, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ReclassificationHistory(PXAdapter adapter)
        {
            if (TransView.Current != null)
            {
                OpenForTransaction(TransView.Current);
            }

            return adapter.Get();
        }

        public static void OpenForTransaction(GLTran tran)
		{
			if (tran == null)
				throw new ArgumentNullException("tran");

			var graph = PXGraph.CreateInstance<ReclassificationHistoryInq>();

            graph.CurrentReclassTranView.Insert(tran);

            var srcTrankey = graph.GetSrcTranOfReclassKey(tran);

			graph.SrcOfReclassTranView.Insert(srcTrankey);

			throw new PXRedirectRequiredException(graph, true, string.Empty)
			{
				Mode = PXBaseRedirectException.WindowMode.New
			};
		}

		#region GLTran CacheAttached

		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		protected virtual void GLTran_CuryCreditAmt_CacheAttached(PXCache sender) { }

		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		protected virtual void GLTran_CuryDebitAmt_CacheAttached(PXCache sender) { }

		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		protected virtual void GLTran_CreditAmt_CacheAttached(PXCache sender) { }

		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		protected virtual void GLTran_DebitAmt_CacheAttached(PXCache sender) { }

		#endregion

		protected virtual void GLTranReclHist_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var tran = (GLTranReclHist)e.Row;

            if (tran == null)
            {
                return;
            }

            bool selectEnable = JournalEntry.IsTransactionReclassifiable(tran, BatchTypeCode.Normal, null, PM.ProjectDefaultAttribute.NonProject());
            PXUIFieldAttribute.SetEnabled<GLTranReclHist.selected>(cache, tran, selectEnable);

            if (tran.Released == false && tran.IsReclassReverse == false)
            {
                cache.RaiseExceptionHandling<GLTranReclHist.batchNbr>(tran, null,
                    new PXSetPropertyException(Messages.TheTransactionCannotBeReclassifiedBecauseItIsNotReleased, PXErrorLevel.RowWarning));
            }
        }

        private GLTranKey GetSrcTranOfReclassKey(GLTran tran)
		{
			if (JournalEntry.IsReclassifacationTran(tran))
			{
				return new GLTranKey()
				{
					Module = tran.ReclassSourceTranModule,
					BatchNbr = tran.ReclassSourceTranBatchNbr,
					LineNbr = tran.ReclassSourceTranLineNbr
				};
			}
			else
			{
				return new GLTranKey()
				{
					Module = tran.Module,
					BatchNbr = tran.BatchNbr,
					LineNbr = tran.LineNbr
				};
			}
		}
	}
}
