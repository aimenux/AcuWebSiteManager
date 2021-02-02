using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	/// <summary>
	/// Realizes aggregating amounts of transactions with enabled summary post flag.
	/// </summary>
	public class SummaryPostingController
	{
		private readonly JournalEntry _journalEntry;
		private readonly CASetup _caSetup;

		private bool _shouldBeNormalized;

		private bool _isSummaryTransactionsLoaded;

		private GLTranGroupComparer _groupComparer;

		private IEqualityComparer<GLTran> _glTranCustomComparer;

		/// <summary>
		/// Groups of transactions which are candidates to be aggregate. Not any transactions may be summarized.
		/// Dictionary<GroupKey, Dictionary<Key, GLTran>>
		/// Internal dictionary is used for fast getting of transaction by DAC key.
		/// </summary>
		private Dictionary<GLTran, Dictionary<GLTran, GLTran>> _summaryTransactionsGroups;

		public SummaryPostingController(JournalEntry journalEntry, CASetup caSetup)
		{
			_journalEntry = journalEntry;
			_caSetup = caSetup;

			var glTranCache = (PXCache<GLTran>) journalEntry.GLTranModuleBatNbr.Cache;

			_glTranCustomComparer = glTranCache.GetKeyComparer();
			_groupComparer = new GLTranGroupComparer(_caSetup);
			_summaryTransactionsGroups = new Dictionary<GLTran, Dictionary<GLTran, GLTran>>(_groupComparer);
		}

		/// <summary>
		///  true - suitable summary transaction has been found and amounts have been aggregated.
		///	 false - nothing has been done.
		/// </summary>
		public bool TryAggregateToSummaryTransaction(GLTran tran)
		{
			if (!IsSummaryPostingAllowed(tran))
				return false;

			var isExistingBatch = _journalEntry.BatchModule.Cache.GetStatus(_journalEntry.BatchModule.Current) !=
			                      PXEntryStatus.Inserted;

			if (isExistingBatch && !_isSummaryTransactionsLoaded)
			{
				RecreateGroupsBySummaryTransFromDB();
			}
			else
			{
				NormalizeIfNeeded();
			}

			Dictionary<GLTran, GLTran> summaryTransGroup;

			if (!_summaryTransactionsGroups.TryGetValue(tran, out summaryTransGroup))
				return false;

			return TryAggregateToFirstSuitableTran(_journalEntry.GLTranModuleBatNbr.Cache, tran, summaryTransGroup.Values);
		}

		private bool IsSummaryPostingAllowed(GLTran tran)
		{
			if (tran.SummPost != true)
				return false;

			if (tran.TaskID != null)
				return false;

			if (tran.ZeroPost != false && tran.CATranID == null)
			{
				var account = (Account)PXSelect<Account,
											Where<Account.accountID, Equal<Required<Account.accountID>>>>
											.Select(_journalEntry, tran.AccountID);

				if (account != null
				    && account.PostOption != AccountPostOption.Summary)
					return false;
			}

			return true;
		}

		private void RecreateGroupsBySummaryTransFromDB()
		{
			var loadedSummaryTrans = _journalEntry.GLTranModuleBatNbr.SearchAll<Asc<GLTran.summPost>>(new object [] {true})
																		.RowCast<GLTran>();

			_summaryTransactionsGroups = new Dictionary<GLTran, Dictionary<GLTran, GLTran>>(_groupComparer);

			foreach (var tran in loadedSummaryTrans)
			{
				AddSummaryTransaction(tran);
			}

			_isSummaryTransactionsLoaded = true;
		}

		public void AddSummaryTransaction(GLTran tran)
		{
			NormalizeIfNeeded();

			Dictionary<GLTran, GLTran> summaryTransGroup;

			if (_summaryTransactionsGroups.TryGetValue(tran, out summaryTransGroup))
			{
				summaryTransGroup.Add(tran, tran);
			}
			else
			{
				summaryTransGroup = new Dictionary<GLTran, GLTran>(_glTranCustomComparer) {{tran, tran}};

				_summaryTransactionsGroups.Add(tran, summaryTransGroup);
			}
		}

		public void RemoveIfNeeded(GLTran tran)
		{
			NormalizeIfNeeded();

			Dictionary<GLTran, GLTran> summaryTransGroup;

			if (_summaryTransactionsGroups.TryGetValue(tran, out summaryTransGroup)
			    && summaryTransGroup.ContainsKey(tran))
			{
				if (summaryTransGroup.Count == 1)
				{
					_summaryTransactionsGroups.Remove(tran);
				}
				else
				{
					summaryTransGroup.Remove(tran);
				}
			}
		}

		public void ShouldBeNormalized()
		{
			_shouldBeNormalized = true;
		}

		private void NormalizeIfNeeded()
		{
			if (!_shouldBeNormalized)
				return;

			var newSummaryTransactionsGroups = new Dictionary<GLTran, Dictionary<GLTran, GLTran>>(_groupComparer);

			foreach (var summaryTransGroupKvp in _summaryTransactionsGroups)
			{
				newSummaryTransactionsGroups.Add(summaryTransGroupKvp.Key, new Dictionary<GLTran, GLTran>(summaryTransGroupKvp.Value, _glTranCustomComparer));
			}

			_summaryTransactionsGroups = newSummaryTransactionsGroups;

			_shouldBeNormalized = false;
		}

		public void ResetState()
		{
			_summaryTransactionsGroups.Clear();
			
			_shouldBeNormalized = false;
			_isSummaryTransactionsLoaded = false;
		}

		public static bool TryAggregateToFirstSuitableTran(PXCache sender, GLTran tran, IEnumerable<GLTran> summaryTrans)
		{
			foreach (GLTran summ_tran in summaryTrans)
			{
				if (TryAggregateToTran(sender, tran, summ_tran))
				{
					return true; 
				}
				else
				{
					continue;
				}
				
			}
			return false;
		}

		public static bool TryAggregateToTran(PXCache sender, GLTran tran, GLTran summ_tran)
		{
				PXParentAttribute.SetParent(sender, summ_tran, typeof(Batch), sender.Graph.Caches[typeof(Batch)].Current);

				GLTran copy_tran = PXCache<GLTran>.CreateCopy(summ_tran);
				copy_tran.CuryCreditAmt += tran.CuryCreditAmt;
				copy_tran.CreditAmt += tran.CreditAmt;
				copy_tran.CuryDebitAmt += tran.CuryDebitAmt;
				copy_tran.DebitAmt += tran.DebitAmt;

				if (tran.ZeroPost == false)
				{
					copy_tran.ZeroPost = false;
				}

				if ((copy_tran.CuryDebitAmt - copy_tran.CuryCreditAmt) > 0m && (copy_tran.DebitAmt - copy_tran.CreditAmt) < 0m ||
					(copy_tran.CuryDebitAmt - copy_tran.CuryCreditAmt) < 0m && (copy_tran.DebitAmt - copy_tran.CreditAmt) > 0m)
				{
				return false;
				}

				PostGraph.NormalizeAmounts(copy_tran);

				if (copy_tran.CuryDebitAmt == 0m &&
					copy_tran.CuryCreditAmt == 0m &&
					copy_tran.DebitAmt == 0m &&
					copy_tran.CreditAmt == 0m &&
					copy_tran.ZeroPost != true)
				{
					sender.Delete(copy_tran);
				}
				else
				{
					if (!object.Equals(copy_tran.TranDesc, tran.TranDesc))
					{
						copy_tran.TranDesc = Messages.SummaryTranDesc;
					}

					copy_tran.Qty = 0m;
					copy_tran.UOM = null;
					copy_tran.InventoryID = null;
					copy_tran.TranLineNbr = null;

					sender.Update(copy_tran);
				}
				return true;
			}

		private class GLTranGroupComparer : IEqualityComparer<GLTran>
		{
			private readonly CASetup _caSetup;
			private readonly bool _isSubFeatureOn;

			public GLTranGroupComparer(CASetup caSetup)
			{
				_caSetup = caSetup;
				_isSubFeatureOn = PXAccess.FeatureInstalled<FeaturesSet.subAccount>();
			}

			#region IEqualityComparer<GLTran> Members

			public bool Equals(GLTran a, GLTran b)
			{
				var isEqual =
					a.SummPost == b.SummPost &&
					a.Module == b.Module &&
					a.BatchNbr == b.BatchNbr &&
					a.RefNbr == b.RefNbr &&
					a.CuryInfoID == b.CuryInfoID &&
					a.BranchID == b.BranchID &&
					a.AccountID == b.AccountID &&
					a.SubID == b.SubID &&
					a.ReclassificationProhibited == b.ReclassificationProhibited &&
					(a.CATranID == b.CATranID || a.CATranID == null && b.CATranID == null) &&
					(a.ProjectID == b.ProjectID || a.ProjectID == null && b.ProjectID == null) &&
					(a.TaskID == b.TaskID || a.TaskID == null && b.TaskID == null) &&
					(a.CostCodeID == b.CostCodeID || a.CostCodeID == null && b.CostCodeID == null);

				if (IsCATransferOnTransitAccount(a) && CATranType.IsTransfer(b.TranType)
					|| a.Module == GL.BatchModule.IN)
				{
					return isEqual;
				}

				return isEqual && a.TranType == b.TranType;
			}

			public int GetHashCode(GLTran obj)
			{
				unchecked
				{
					int ret = 17;
					ret = ret * 23 + obj.SummPost.GetHashCode();
					ret = ret * 23 + obj.Module.GetHashCode();
					ret = ret * 23 + obj.BatchNbr.GetHashCode();
					ret = ret * 23 + obj.RefNbr.GetHashCode();
					ret = ret * 23 + obj.CuryInfoID.GetHashCode();
					ret = ret * 23 + obj.BranchID.GetHashCode();
					ret = ret * 23 + obj.AccountID.GetHashCode();
					ret = ret * 23 + obj.SubID.GetHashCode();
					ret = ret * 23 + obj.ReclassificationProhibited.GetHashCode();

					if (IsCATransferOnTransitAccount(obj)
						|| obj.Module == GL.BatchModule.IN)
					{
						return ret;
					}

					return ret * 23 + obj.TranType.GetHashCode();
				}
			}

			private bool IsCATransferOnTransitAccount(GLTran a)
			{
				return a.Module == GL.BatchModule.CA
					   && a.AccountID == _caSetup?.TransitAcctId
					   && (a.SubID == _caSetup?.TransitSubID || !_isSubFeatureOn)
					   && CATranType.IsTransfer(a.TranType);
			}

			#endregion
		}
	}
}
