using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.Tools;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.Reclassification.Common;

namespace PX.Objects.GL.Reclassification.Processing
{
	public class ReclassifyTransactionsProcessor : ReclassifyTransactionsBase<ReclassifyTransactionsProcessor>
	{
		public PXSelect<GLTranForReclassification> GLTranForReclass;

		protected JournalEntry JournalEntryInstance;

		public void ProcessTransForReclassification(List<GLTranForReclassification> origTransForReclass, ReclassGraphState state)
		{
			State = state;

			TransGroupKey? transGroupKeyToPutToExistingBatch = null;

		    JournalEntryInstance = CreateJournalEntry();

		    var hasError = false;
			Batch batchForEditing = null;
			IReadOnlyCollection<ReclassificationItem> reclassItemGroupsToBatchForEditing = null;

			var workOrigTranMap = new Dictionary<GLTranForReclassification, GLTranForReclassification>();

			var allReclassItemsByHeaderKey = BuildReclassificationItems(origTransForReclass, out workOrigTranMap);

			var transForReclass = workOrigTranMap.Keys.ToList();

			if (State.ReclassScreenMode == ReclassScreenMode.Editing)
			{
				transGroupKeyToPutToExistingBatch = DefineTransGroupKeyToPutToExistingBatch(transForReclass);
			}

			PrepareJournalEntryGraph();

			var reclassItemGroups = allReclassItemsByHeaderKey.Values.GroupBy(tranWithCury =>
				new TransGroupKey()
				{
					MasterPeriodID = tranWithCury.HeadTranForReclass.TranPeriodID,
					CuryID = tranWithCury.CuryInfo.CuryID
				});

			foreach (var reclassItemGroup in reclassItemGroups)
			{
				var reclassItems = reclassItemGroup.ToArray();

				if (State.ReclassScreenMode == ReclassScreenMode.Editing
					&& reclassItemGroup.Key.Equals(transGroupKeyToPutToExistingBatch.Value))
				{
					batchForEditing = JournalEntry.FindBatch(JournalEntryInstance, State.EditingBatchModule, State.EditingBatchNbr);
					reclassItemGroupsToBatchForEditing = reclassItems;

					//existing batch must be processed last
					continue;
				}

				hasError |= !ProcessTranForReclassGroup(reclassItems,
														workOrigTranMap,
														origTransForReclass,
														allReclassItemsByHeaderKey);
			}

			if (State.ReclassScreenMode == ReclassScreenMode.Editing)
			{
				hasError |= !ProcessTranForReclassGroup(reclassItemGroupsToBatchForEditing,
														workOrigTranMap,
														origTransForReclass,
														allReclassItemsByHeaderKey,
														batchForEditing);
			}

			if (hasError)
				throw new PXException(ErrorMessages.SeveralItemsFailed);
		}

	    protected virtual JournalEntry CreateJournalEntry()
	    {
	        var journalEntryInstance = CreateInstance<JournalEntry>();

	        journalEntryInstance.Mode |= JournalEntry.Modes.Reclassification;

			journalEntryInstance.GLTranModuleBatNbr.Cache.Adjust<FinPeriodIDAttribute>()
				.For<GLTran.finPeriodID>(attr =>
				{
					attr.RedefaultOnDateChanged = false;
				});

			journalEntryInstance.BatchModule.Cache.Adjust<OpenPeriodAttribute>()
	            .For<Batch.finPeriodID>(attr =>
	            {
	                attr.RedefaultOnDateChanged = false;
	            });

            return journalEntryInstance;
	    }

	    private void PrepareJournalEntryGraph()
		{
			if (State.ReclassScreenMode == ReclassScreenMode.Editing)
			{
				var transForEditing = GetTransReclassTypeSorted(JournalEntryInstance, State.EditingBatchModule, State.EditingBatchNbr);

				foreach (var tran in transForEditing)
				{
					JournalEntryInstance.GLTranModuleBatNbr.Cache.SetStatus(tran, PXEntryStatus.Held);
				}
			}

			JournalEntryInstance.glsetup.Current.RequireControlTotal = false;
		}


		private bool ProcessTranForReclassGroup(IReadOnlyCollection<ReclassificationItem> reclassItems,
												Dictionary<GLTranForReclassification, GLTranForReclassification> workOrigTranMap,
												List<GLTranForReclassification> origTransForReclass,
												Dictionary<GLTranKey, ReclassificationItem> allReclassItemsByHeaderKey,
												Batch batchForEdit = null)
		{
			Batch reclassBatch;
			List<GLTran> transMovedFromExistingBatch = new List<GLTran>();
			try
			{
                using (PXTransactionScope ts = new PXTransactionScope())
				{
					reclassBatch = BuildReclassificationBatch(reclassItems, transMovedFromExistingBatch, batchForEdit);

					JournalEntryInstance.Actions.PressSave();


					foreach (var tran in transMovedFromExistingBatch)
					{
						JournalEntryInstance.GLTranModuleBatNbr.Delete(tran);
					}

					JournalEntryInstance.Actions.PressSave();


					var reclassItemByOrigBatches = reclassItems.GroupBy(item => new {item.HeadTranForReclass.Module, item.HeadTranForReclass.BatchNbr});

					foreach (var reclassItemGroup in reclassItemByOrigBatches)
					{
						UpdateOrigRecordsByBatches(reclassItemGroup.Key.Module, 
													reclassItemGroup.Key.BatchNbr,
													reclassItemGroup.ToList().AsReadOnly(),
													reclassBatch,
													allReclassItemsByHeaderKey);
					}

					ts.Complete();
				}

				foreach (var tranCuryPair in reclassItems)
				{
					var origTran = workOrigTranMap[tranCuryPair.HeadTranForReclass];

					origTran.ReclassBatchModule = reclassBatch.Module;
					origTran.ReclassBatchNbr = reclassBatch.BatchNbr;
				}

				if (batchForEdit != null)
				{
					State.GLTranForReclassToDelete.Clear();
				}
			}
			catch (Exception ex)
			{
				var exMessage = GetExceptionMessage(ex);

				var message =
					string.Concat(PXMessages.LocalizeNoPrefix(Messages.ReclassificationBatchHasNotBeenCreatedForTheTransaction),
						Environment.NewLine,
						exMessage);

				foreach (var tranCuryPair in reclassItems)
				{
					GLTranForReclass.Cache.RestoreCopy(tranCuryPair.HeadTranForReclass, workOrigTranMap[tranCuryPair.HeadTranForReclass]);

					PXProcessing<GLTranForReclassification>.SetError(origTransForReclass.IndexOf(workOrigTranMap[tranCuryPair.HeadTranForReclass]),
						message);
				}

				JournalEntryInstance.Clear();
				PrepareJournalEntryGraph();

				return false;
			}

			if (GLSetup.Current.AutoReleaseReclassBatch == true
				&& batchForEdit == null)
			{
				try
				{
					JournalEntry.ReleaseBatch(new[] { reclassBatch }, externalPostList: null, unholdBatch: true);
				}
				catch (Exception ex)
				{
					var exMessage = GetExceptionMessage(ex);

					var message = string.Concat(
						PXMessages.LocalizeNoPrefix(Messages.ReclassificationBatchGeneratedForThisTransactionHasNotBeenReleasedOrPosted),
						Environment.NewLine,
						exMessage);


					foreach (var tranCuryPair in reclassItems)
					{
						PXProcessing<GLTranForReclassification>.SetError(
							origTransForReclass.IndexOf(workOrigTranMap[tranCuryPair.HeadTranForReclass]), message);
					}

					return false;
				}
			}

			foreach (var tranCuryPair in reclassItems)
			{
				PXProcessing<GLTranForReclassification>.SetInfo(origTransForReclass.IndexOf(workOrigTranMap[tranCuryPair.HeadTranForReclass]),
					ActionsMessages.RecordProcessed);

                foreach (var split in tranCuryPair.SplittingTransForReclass)
                {
                    PXProcessing<GLTranForReclassification>.SetInfo(origTransForReclass.IndexOf(workOrigTranMap[split]), ActionsMessages.RecordProcessed);
                }
            }

			return true;
		}

		protected virtual void UpdateOrigRecordsByBatches(string origModule, 
															string origBatchNbr, 
															IReadOnlyCollection<ReclassificationItem> reclassificationItems,
															Batch reclassBatch,
															Dictionary<GLTranKey, ReclassificationItem> allReclassItemsByHeaderKey)
		{
			JournalEntryInstance.Clear(PXClearOption.ClearAll);
			JournalEntryInstance.SelectTimeStamp();

			JournalEntryInstance.BatchModule.Current = JournalEntry.FindBatch(JournalEntryInstance, origModule, origBatchNbr);

			List<int?> origLineNbrs = reclassificationItems.Select(m => m.HeadTranForReclass.LineNbr).ToList();

			const int portionSize = 2;

			decimal rawPortionCount = reclassificationItems.Count / (decimal)portionSize;

			int portionCount = (int) Math.Ceiling(rawPortionCount);

			for (int i = 0; i < portionCount; i++)
			{
				int curPortionSize = i <= rawPortionCount - 1 
											? portionSize 
											: reclassificationItems.Count % portionSize;

				var lineNbrs = origLineNbrs.GetRange(i * portionSize, curPortionSize).ToArray();

				var transInBatch = JournalEntry.GetTrans(JournalEntryInstance, origModule, origBatchNbr, lineNbrs);

				foreach (GLTran origTran in transInBatch)
				{
					origTran.ReclassBatchNbr = reclassBatch.BatchNbr;
					origTran.ReclassBatchModule = reclassBatch.Module;

					ReclassificationItem reclassItem = allReclassItemsByHeaderKey[new GLTranKey(origTran)];

					if (origTran.ReclassTotalCount == null)
					{
						origTran.ReclassTotalCount = 0;
					}

					origTran.ReclassTotalCount += reclassItem.NewReclassifyingTrans.Count;

					JournalEntryInstance.GLTranModuleBatNbr.Update(origTran);
				}
			}

			JournalEntryInstance.Save.Press();
		}

		private TransGroupKey DefineTransGroupKeyToPutToExistingBatch(IReadOnlyCollection<GLTranForReclassification> transForReclass)
		{
			string minMasterPeriodIDForEditing = null;
			string minMasterPeriodIDForBatchCury = null;

			string minMasterPeriodID = null;
			string minCuryIDForMinMasterPeriodID = null;

			foreach (var tranForReclass in transForReclass)
			{
				if (tranForReclass.ReclassRowType == ReclassRowTypes.Editing)
				{
					if (minMasterPeriodIDForEditing == null ||
						string.CompareOrdinal(tranForReclass.TranPeriodID, minMasterPeriodIDForEditing) < 0)
					{
						minMasterPeriodIDForEditing = tranForReclass.TranPeriodID;
					}
				}

				if (tranForReclass.CuryID == State.EditingBatchCuryID)
				{
					if (minMasterPeriodIDForBatchCury == null ||
						string.CompareOrdinal(tranForReclass.TranPeriodID, minMasterPeriodIDForBatchCury) < 0)
					{
						minMasterPeriodIDForBatchCury = tranForReclass.TranPeriodID;
					}
				}

				var compareRes = string.CompareOrdinal(tranForReclass.TranPeriodID, minMasterPeriodID);
				if (minMasterPeriodID == null || compareRes < 0)
				{
					minMasterPeriodID = tranForReclass.TranPeriodID;
					minCuryIDForMinMasterPeriodID = tranForReclass.CuryID;
				}
				if (compareRes == 0 && string.CompareOrdinal(tranForReclass.CuryID, minCuryIDForMinMasterPeriodID) < 0)
				{
					minCuryIDForMinMasterPeriodID = tranForReclass.CuryID;
				}
			}

			//Existing batch must be contain old trans with old or min period, or new trans with old curyID
			var groupDiscriminatorsByPriority = new TransGroupKeyDiscriminatorPair[3];

			groupDiscriminatorsByPriority[0] = new TransGroupKeyDiscriminatorPair(tranForReclass => tranForReclass.ReclassRowType == ReclassRowTypes.Editing && tranForReclass.TranPeriodID == State.EditingBatchMasterPeriodID);
			groupDiscriminatorsByPriority[1] = new TransGroupKeyDiscriminatorPair(tranForReclass => tranForReclass.ReclassRowType == ReclassRowTypes.Editing && tranForReclass.TranPeriodID == minMasterPeriodIDForEditing);
			groupDiscriminatorsByPriority[2] = new TransGroupKeyDiscriminatorPair(tranForReclass => tranForReclass.CuryID == State.EditingBatchCuryID && tranForReclass.TranPeriodID == minMasterPeriodIDForBatchCury);

			foreach (var tranForReclass in transForReclass)
			{
				for (int i = 0; i < groupDiscriminatorsByPriority.Length; i++)
				{
					var discriminator = groupDiscriminatorsByPriority[i].Discriminator;
					if (discriminator(tranForReclass))
					{
						if (i == 0)
						{
							return new TransGroupKey()
							{
								MasterPeriodID = tranForReclass.TranPeriodID,
								CuryID = tranForReclass.CuryID
							};
						}

						var curKey = groupDiscriminatorsByPriority[i].TransGroupKey;

						curKey.CuryID = tranForReclass.CuryID;
						curKey.MasterPeriodID = tranForReclass.TranPeriodID;
					}
				}
			}

			for (int i = 1; i < groupDiscriminatorsByPriority.Length; i++)
			{
				if (groupDiscriminatorsByPriority[i].TransGroupKey.CuryID != null)
					return groupDiscriminatorsByPriority[i].TransGroupKey;
			}

			//All trans are new and with other curyID, take min by finPeriodID, CuryID
			return new TransGroupKey()
			{
				MasterPeriodID = minMasterPeriodID,
				CuryID = minCuryIDForMinMasterPeriodID
			};
		}

		private Dictionary<GLTranKey, ReclassificationItem> BuildReclassificationItems(ICollection<GLTranForReclassification> transForReclassOrig, 
																						out Dictionary<GLTranForReclassification, GLTranForReclassification> mappedTrans)
		{
			mappedTrans = transForReclassOrig.ToDictionary(origTran => PXCache<GLTranForReclassification>.CreateCopy(origTran), origTran => origTran);

			var reclassItemsByHeaderKeys = new Dictionary<GLTranKey, ReclassificationItem>();

			foreach (var tran in mappedTrans.Keys)
			{
				if (tran.IsSplitting == false)
				{
					var key = new GLTranKey(tran);

					if (reclassItemsByHeaderKeys.ContainsKey(key))
					{
						reclassItemsByHeaderKeys[key].HeadTranForReclass = tran;
					}
					else
					{
						reclassItemsByHeaderKeys.Add(key, new ReclassificationItem());
						reclassItemsByHeaderKeys[key].HeadTranForReclass = tran;
					}

					continue;
				}
				if (reclassItemsByHeaderKeys.ContainsKey(tran.ParentKey))
				{
					reclassItemsByHeaderKeys[tran.ParentKey].SplittingTransForReclass.Add(tran);
				}
				else
				{
					reclassItemsByHeaderKeys.Add(tran.ParentKey, new ReclassificationItem());
					reclassItemsByHeaderKeys[tran.ParentKey].SplittingTransForReclass.Add(tran);
				}
			}

			var reclassItemsByCuryInfoID = reclassItemsByHeaderKeys.Values.GroupBy(m => m.HeadTranForReclass.CuryInfoID);

			foreach (var reclassItemGroup in reclassItemsByCuryInfoID)
			{
				var curyInfo = PXSelect<CurrencyInfo,
										Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
										.Select(this, reclassItemGroup.Key);

				foreach (var reclassItem in reclassItemGroup)
				{
					reclassItem.CuryInfo = curyInfo;
				}
			}

			return reclassItemsByHeaderKeys;
		}

		private Batch BuildReclassificationBatch(IReadOnlyCollection<ReclassificationItem> transForReclassItems,
			List<GLTran> transMovedFromExistingBatch,
			Batch batchForEditing = null)
		{
			DateTime earliestNewTranDate = DateTime.MaxValue;
			Batch batch;

		    var representativeTranForReclass = transForReclassItems.First().HeadTranForReclass;
			JournalEntryInstance.BatchModule.Current = null;

			if (batchForEditing == null)
			{
                batch = JournalEntryInstance.BatchModule.Insert(new Batch()
                {
                    BranchID = representativeTranForReclass.NewBranchID,
                    FinPeriodID = representativeTranForReclass.NewFinPeriodID
                });
			}
			else
			{
				batch = batchForEditing;
				JournalEntryInstance.BatchModule.Current = batch;
			}

			//adding or editing of transactions
			foreach (var reclassItem in transForReclassItems)
			{
				IEnumerable<GLTranForReclassification> headTranForReclass = IsReclassAttrChanged(reclassItem.HeadTranForReclass)
																				? reclassItem.HeadTranForReclass.SingleToArray().Union(reclassItem.SplittingTransForReclass)
																				: new GLTranForReclassification[0];

				IEnumerable<GLTranForReclassification> tranForReclassToProcess = headTranForReclass.Union(reclassItem.SplittingTransForReclass);

				foreach (var tranForReclass in tranForReclassToProcess)
                {
					GLTran reclassifyingTran = CreateOrEditReclassTranPair(tranForReclass, reclassItem, transMovedFromExistingBatch);

					if (reclassifyingTran.TranDate < earliestNewTranDate)
	                {
		                earliestNewTranDate = reclassifyingTran.TranDate.Value;
	                }
				}
            }

            //remove deleted trans
            if (batchForEditing != null)
			{
				foreach (var tranForReclassToDel in State.GLTranForReclassToDelete)
				{
					var reverseTran = LocateReverseTran(JournalEntryInstance.GLTranModuleBatNbr.Cache, tranForReclassToDel);
					var reclassifyingTran = LocateReclassifyingTran(JournalEntryInstance.GLTranModuleBatNbr.Cache, tranForReclassToDel);

					JournalEntryInstance.GLTranModuleBatNbr.Delete(reclassifyingTran);
					JournalEntryInstance.GLTranModuleBatNbr.Delete(reverseTran);
				}
			}

			//creating and editing of batch header
			if (batchForEditing == null)
			{
				var batchCuryInfo = PXCache<CurrencyInfo>.CreateCopy(transForReclassItems.First().CuryInfo);
				batchCuryInfo.CuryInfoID = null;
				batchCuryInfo = JournalEntryInstance.currencyinfo.Insert(batchCuryInfo);

				batch.BatchType = BatchTypeCode.Reclassification;
			    batch.BranchID = representativeTranForReclass.NewBranchID;
			    FinPeriodIDAttribute.SetPeriodsByMaster<Batch.finPeriodID>(JournalEntryInstance.BatchModule.Cache, batch,
			        representativeTranForReclass.TranPeriodID);
                batch.LedgerID = representativeTranForReclass.LedgerID;
				batch.Module = BatchModule.GL;
				batch.CuryInfoID = batchCuryInfo.CuryInfoID;
			}
			else
			{
                CurrencyInfo firstCuryInfo = null;
                int? minLineNbr = int.MaxValue;

                foreach (var item in transForReclassItems)
                {
                    var reclassTran = item.ReclassifyingTrans.GetItemWithMin(m => m.LineNbr.Value);
                    if(minLineNbr > reclassTran.LineNbr)
                    {
                        minLineNbr = reclassTran.LineNbr;
                        firstCuryInfo = item.CuryInfo;
                    }
                }

                var batchCuryInfo = PXCache<CurrencyInfo>.CreateCopy(firstCuryInfo);
				batchCuryInfo.CuryInfoID = JournalEntryInstance.currencyInfo.CuryInfoID;
				batchCuryInfo.tstamp = JournalEntryInstance.currencyInfo.tstamp;

				JournalEntryInstance.currencyinfo.Update(batchCuryInfo);
			}

			batch.DateEntered = earliestNewTranDate;
			batch.CuryID = representativeTranForReclass.CuryID;

			if (State.ReclassScreenMode == ReclassScreenMode.Reversing)
			{
				batch.OrigModule = State.OrigBatchModuleToReverse;
				batch.OrigBatchNbr = State.OrigBatchNbrToReverse;
				batch.AutoReverseCopy = true;
			}

			batch = JournalEntryInstance.BatchModule.Update(batch);

			return batch;
		}

        private GLTran CreateOrEditReclassTranPair(GLTranForReclassification tranForReclass,
														ReclassificationItem reclassItem, 
														List<GLTran> transMovedFromExistingBatch)
        {
	        GLTran reclassifyingTran;

			if (tranForReclass.ReclassRowType == ReclassRowTypes.Editing)
            {
                EditReverseTran(tranForReclass, transMovedFromExistingBatch);

	            reclassifyingTran = EditReclassifyingTran(tranForReclass, transMovedFromExistingBatch);
            }
            else
            {
                var newTranCuryInfo = CreateCurrencyInfo(reclassItem.CuryInfo);

                BuildReverseTran(tranForReclass, reclassItem, newTranCuryInfo);

	            reclassifyingTran = BuildReclassifyingTran(tranForReclass, reclassItem, newTranCuryInfo);

	            reclassItem.NewReclassifyingTrans.Add(reclassifyingTran);
			}

	        reclassItem.ReclassifyingTrans.Add(reclassifyingTran);

	        return reclassifyingTran;
        }

		private GLTran BuildReclassifyingTran(GLTranForReclassification tranForReclass, ReclassificationItem reclassItem, CurrencyInfo newTranCuryInfo)
		{
			var reclassifyingTran = JournalEntry.BuildReleasableTransaction(JournalEntryInstance, tranForReclass, JournalEntry.TranBuildingModes.SetLinkToOriginal, newTranCuryInfo);

			SetOrigLineNumber(reclassifyingTran, reclassItem);
			SetReclassifyingTranBusinessAttrs(reclassifyingTran, tranForReclass);
			SetReclassificationLinkingAttrs(reclassifyingTran, tranForReclass, reclassItem);
            SetDependingOnReclassTypeAttrs(reclassifyingTran, tranForReclass);

			bool BaseCalc = (reclassifyingTran.TranClass != GLTran.tranClass.RealizedAndRoundingGOL);
			PXDBCurrencyAttribute.SetBaseCalc<GLTran.curyCreditAmt>(JournalEntryInstance.GLTranModuleBatNbr.Cache, null, BaseCalc);
			PXDBCurrencyAttribute.SetBaseCalc<GLTran.curyDebitAmt>(JournalEntryInstance.GLTranModuleBatNbr.Cache, null, BaseCalc);
			return JournalEntryInstance.GLTranModuleBatNbr.Insert(reclassifyingTran);
		}

		private GLTran EditReclassifyingTran(GLTranForReclassification tranForReclass, List<GLTran> transMovedFromExistingBatch)
		{
            var reclassifyingTran = LocateReclassifyingTran(JournalEntryInstance.GLTranModuleBatNbr.Cache, tranForReclass);

			var anyChanged = reclassifyingTran.BranchID != tranForReclass.NewBranchID
							 || reclassifyingTran.AccountID != tranForReclass.NewAccountID
							 || reclassifyingTran.SubID != tranForReclass.NewSubID
							 || reclassifyingTran.TranDate != tranForReclass.NewTranDate
							 || reclassifyingTran.TranDesc != tranForReclass.NewTranDesc
                             || tranForReclass.CuryNewAmt != null && (reclassifyingTran.CuryDebitAmt + reclassifyingTran.CuryCreditAmt) != tranForReclass.CuryNewAmt;

            if (anyChanged)
            {
                var newTran = CopyTranForMovingIfNeed(reclassifyingTran, transMovedFromExistingBatch);

                if (newTran != null)
                {
                    SetReclassifyingTranBusinessAttrs(newTran, tranForReclass);
                    return JournalEntryInstance.GLTranModuleBatNbr.Insert(newTran);
                }

                SetReclassifyingTranBusinessAttrs(reclassifyingTran, tranForReclass);
                return JournalEntryInstance.GLTranModuleBatNbr.Update(reclassifyingTran);
            }

            return reclassifyingTran;
        }

		private GLTran CopyTranForMovingIfNeed(GLTran tran, List<GLTran> transMovedFromExistingBatch)
		{
			var batch = JournalEntryInstance.BatchModule.Current;

			if (batch.Module != tran.Module || batch.BatchNbr != tran.BatchNbr)
			{
				var oldTran = tran;
				tran = PXCache<GLTran>.CreateCopy(oldTran);

				tran.Module = null;
				tran.BatchNbr = null;
				tran.LineNbr = null;

				transMovedFromExistingBatch.Add(oldTran);

				return tran;
			}

			return null;
		}

        private void EditReverseTran(GLTranForReclassification tranForReclass, List<GLTran> transMovedFromExistingBatch)
		{
			var reverseTran = LocateReverseTran(JournalEntryInstance.GLTranModuleBatNbr.Cache, tranForReclass);

            bool anyChanged;

            if (reverseTran == null && tranForReclass.EditingPairReclassifyingLineNbr == null)
            {
                anyChanged = false;
            }
            else
            {
                anyChanged = reverseTran.TranDate != tranForReclass.NewTranDate
                    || reverseTran.TranDesc != tranForReclass.NewTranDesc
                    || tranForReclass.CuryNewAmt != null && (reverseTran.CuryDebitAmt + reverseTran.CuryCreditAmt) != tranForReclass.CuryNewAmt;
            }


			if (anyChanged)
			{
				var newTran = CopyTranForMovingIfNeed(reverseTran, transMovedFromExistingBatch);

				if (newTran != null)
				{
					SetCommonBusinessAttrs(newTran, tranForReclass);
                    SetReclassAmount(newTran, tranForReclass);

                    JournalEntryInstance.GLTranModuleBatNbr.Insert(newTran);
				}
				else
				{
					SetCommonBusinessAttrs(reverseTran, tranForReclass);
                    SetReclassAmount(reverseTran, tranForReclass);

                    JournalEntryInstance.GLTranModuleBatNbr.Update(reverseTran);
				}
			}
		}

		private void BuildReverseTran(GLTranForReclassification tranForReclass, ReclassificationItem reclassItem, CurrencyInfo newTranCuryInfo)
		{
            var reverseTran = JournalEntry.BuildReverseTran(JournalEntryInstance, tranForReclass, JournalEntry.TranBuildingModes.SetLinkToOriginal, newTranCuryInfo);

			reverseTran.IsReclassReverse = true;
			SetOrigLineNumber(reverseTran, reclassItem);
			SetCommonBusinessAttrs(reverseTran, tranForReclass);
			SetReclassificationLinkingAttrs(reverseTran, tranForReclass, reclassItem);
            SetDependingOnReclassTypeAttrs(reverseTran, tranForReclass);

			bool BaseCalc = (tranForReclass.TranClass != GLTran.tranClass.RealizedAndRoundingGOL);
			PXDBCurrencyAttribute.SetBaseCalc<GLTran.curyCreditAmt>(JournalEntryInstance.GLTranModuleBatNbr.Cache, null, BaseCalc);
			PXDBCurrencyAttribute.SetBaseCalc<GLTran.curyDebitAmt>(JournalEntryInstance.GLTranModuleBatNbr.Cache, null, BaseCalc);
			JournalEntryInstance.GLTranModuleBatNbr.Insert(reverseTran);
		}

		protected virtual void SetOrigLineNumber(GLTran tran, ReclassificationItem reclassItem)
		{
			tran.OrigLineNbr = reclassItem.HeadTranForReclass.LineNbr;
		}

		private void SetCommonBusinessAttrs(GLTran tran, GLTranForReclassification tranForReclassification)
		{
			tran.TranDate = tranForReclassification.NewTranDate;
			tran.TranDesc = tranForReclassification.NewTranDesc;
		}

		private void SetReclassifyingTranBusinessAttrs(GLTran reclassifyingTran, GLTranForReclassification tranForReclass)
		{
			reclassifyingTran.BranchID = tranForReclass.NewBranchID;
			reclassifyingTran.AccountID = tranForReclass.NewAccountID;

			if (tranForReclass.NewSubCD != null)
			{
				JournalEntryInstance.GLTranModuleBatNbr.SetValueExt<GLTran.subID>(reclassifyingTran, tranForReclass.NewSubCD);
			}
			else
			{
				reclassifyingTran.SubID = tranForReclass.NewSubID;
			}

			SetCommonBusinessAttrs(reclassifyingTran, tranForReclass);
			SetReclassAmount(reclassifyingTran, tranForReclass);
		}

		private void SetReclassificationLinkingAttrs(GLTran tran, GLTranForReclassification tranForReclassification, ReclassificationItem reclassItem)
		{
			if (JournalEntry.IsReclassifacationTran(tranForReclassification))
			{
				tran.ReclassSourceTranModule = tranForReclassification.ReclassSourceTranModule;
				tran.ReclassSourceTranBatchNbr = tranForReclassification.ReclassSourceTranBatchNbr;
				tran.ReclassSourceTranLineNbr = tranForReclassification.ReclassSourceTranLineNbr;
				tran.ReclassSeqNbr = tranForReclassification.ReclassSeqNbr + 1;
			}
			else
			{
				tran.ReclassSourceTranModule = tranForReclassification.Module;
				tran.ReclassSourceTranBatchNbr = tranForReclassification.BatchNbr;
				tran.ReclassSourceTranLineNbr = reclassItem.HeadTranForReclass.LineNbr;
				tran.ReclassSeqNbr = 1;
			}

			tran.ReclassOrigTranDate = tranForReclassification.TranDate;
		}

        private void SetDependingOnReclassTypeAttrs(GLTran tran, GLTranForReclassification tranForReclassification)
        {
            tran.ReclassType = tranForReclassification.IsSplitting ? ReclassType.Split : ReclassType.Common;
            SetReclassAmount(tran, tranForReclassification);
        }

		private void SetReclassAmount(GLTran tran, GLTranForReclassification rtran)
		{
			if (rtran.IsSplitting == false && rtran.IsSplitted == false)
			{
				return;
			}

			if (rtran.IsSplitting)
            {
                if(tran.IsReclassReverse == true)
                {
                    tran.CuryDebitAmt = rtran.SourceCuryCreditAmt != 0m ? rtran.CuryNewAmt : 0m;
                    tran.CuryCreditAmt = rtran.SourceCuryDebitAmt != 0m ? rtran.CuryNewAmt : 0m;
                    return;
                }

                tran.CuryDebitAmt = rtran.SourceCuryDebitAmt != 0m ? rtran.CuryNewAmt : 0m;
                tran.CuryCreditAmt = rtran.SourceCuryCreditAmt != 0m ? rtran.CuryNewAmt : 0m;
                return;
            }

            tran.CuryDebitAmt = tran.CuryDebitAmt != 0m ? rtran.CuryNewAmt : 0m;
            tran.CuryCreditAmt = tran.CuryCreditAmt != 0m ? rtran.CuryNewAmt : 0m;
        }

        private GLTran LocateReclassifyingTran(PXCache cache, GLTranForReclassification tranForReclass)
		{
			var reclassifyingTran = new GLTran()
			{
				Module = State.EditingBatchModule,
				BatchNbr = State.EditingBatchNbr,
				LineNbr = tranForReclass.EditingPairReclassifyingLineNbr
			};

			return (GLTran)cache.Locate(reclassifyingTran);
		}

		private GLTran LocateReverseTran(PXCache cache, GLTranForReclassification tranForReclass)
		{
			var reverseTran = new GLTran()
			{
				Module = State.EditingBatchModule,
				BatchNbr = State.EditingBatchNbr,
				LineNbr = tranForReclass.EditingPairReclassifyingLineNbr - 1
			};

			return (GLTran)cache.Locate(reverseTran);
		}


		#region Service Methods

		private CurrencyInfo CreateCurrencyInfo(CurrencyInfo curyInfo)
		{
			var info = PXCache<CurrencyInfo>.CreateCopy(curyInfo);

			info.CuryInfoID = null;
			info.IsReadOnly = true;
			info.BaseCalc = true;

			info = JournalEntryInstance.currencyinfo.Insert(info);

			return info;
		}

		private string GetExceptionMessage(Exception ex)
		{
			var outerEx = ex as PXOuterException;

			return outerEx != null
				? string.Concat(outerEx.Message, ";", string.Join(";", outerEx.InnerMessages))
				: ex.Message;
		}
		
		#endregion


		#region DTOs

		public struct TransGroupKey
		{
			public string MasterPeriodID { get; set; }
			public string CuryID { get; set; }

			public override bool Equals(Object obj)
			{
				if (obj == null || GetType() != obj.GetType())
					return false;

				var p = (TransGroupKey)obj;
				return (MasterPeriodID == p.MasterPeriodID) && (CuryID == p.CuryID);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int ret = 17;

					ret = ret * 23 + MasterPeriodID.GetHashCode();
					return ret * 23 + CuryID.GetHashCode();
				}
			}
		}

		public class TransGroupKeyDiscriminatorPair
		{
			public TransGroupKey TransGroupKey { get; set; }
			public Func<GLTranForReclassification, bool> Discriminator { get; set; }

			public TransGroupKeyDiscriminatorPair(Func<GLTranForReclassification, bool> discriminator)
			{
				TransGroupKey = new TransGroupKey();
				Discriminator = discriminator;
			}
		}

		public class ReclassificationItem
		{
			public GLTranForReclassification HeadTranForReclass;
            public List<GLTranForReclassification> SplittingTransForReclass;
            public CurrencyInfo CuryInfo;
			public List<GLTran> ReclassifyingTrans;
			public List<GLTran> NewReclassifyingTrans;

			public ReclassificationItem(GLTranForReclassification tran, List<GLTranForReclassification> splittingTransForReclass, CurrencyInfo curyInfo)
			{
				HeadTranForReclass = tran;
                SplittingTransForReclass = splittingTransForReclass;
                CuryInfo = curyInfo;
                ReclassifyingTrans = new List<GLTran>();
				NewReclassifyingTrans = new List<GLTran>();
			}

			public ReclassificationItem()
			{
				SplittingTransForReclass = new List<GLTranForReclassification>();
				ReclassifyingTrans = new List<GLTran>();
				NewReclassifyingTrans = new List<GLTran>();
			}
		}
		
		#endregion
	}
}
