﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.AR;
using PX.Objects.GL;

namespace ReconciliationTools
{
	[TableAndChartDashboardType]
	public class ARGLDiscrepancyByAccountEnq : ARGLDiscrepancyEnqGraphBase<ARGLDiscrepancyByAccountEnq, ARGLDiscrepancyEnqFilter, DiscrepancyByAccountEnqResult>
	{
		#region CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault]
		protected virtual void ARGLDiscrepancyEnqFilter_AccountID_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "AR Turnover")]
		protected virtual void DiscrepancyByAccountEnqResult_XXTurnover_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Non-AR Transactions")]
		protected virtual void DiscrepancyByAccountEnqResult_NonXXTrans_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Financial Period")]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		protected virtual void DiscrepancyByAccountEnqResult_FinPeriodID_CacheAttached(PXCache sender) { }

		#endregion

		protected override List<DiscrepancyByAccountEnqResult> SelectDetails()
		{
			var list = new List<DiscrepancyByAccountEnqResult>();
			ARGLDiscrepancyEnqFilter header = Filter.Current;

			if (header == null ||
				header.BranchID == null ||
				header.PeriodFrom == null ||
				header.PeriodTo == null ||
				header.AccountID == null)
			{
				return list;
			}

			#region GL balances

			AccountByPeriodEnq graphGL = PXGraph.CreateInstance<AccountByPeriodEnq>();
			AccountByPeriodFilter filterGL = PXCache<AccountByPeriodFilter>.CreateCopy(graphGL.Filter.Current);

			graphGL.Filter.Cache.SetDefaultExt<AccountByPeriodFilter.ledgerID>(filterGL);
			filterGL.BranchID = header.BranchID;
			filterGL.SubID = header.SubCD;
			filterGL.StartPeriodID = header.PeriodFrom;
			filterGL.EndPeriodID = header.PeriodTo;
			filterGL.AccountID = header.AccountID;
			filterGL = graphGL.Filter.Update(filterGL);

			Dictionary<DiscrepancyByAccountEnqResultKey, DiscrepancyByAccountEnqResult> dict =
				new Dictionary<DiscrepancyByAccountEnqResultKey, DiscrepancyByAccountEnqResult>();

			foreach (GLTranR gltran in graphGL.GLTranEnq.Select())
			{
				DiscrepancyByAccountEnqResult result;
				DiscrepancyByAccountEnqResultKey key = new DiscrepancyByAccountEnqResultKey(gltran);

				bool isARTran = gltran.Module == BatchModule.AR &&
					!string.IsNullOrEmpty(gltran.TranType) &&
					!string.IsNullOrEmpty(gltran.RefNbr) &&
					gltran.ReferenceID != null;

				decimal glTurnover = CalcGLTurnover(gltran);
				decimal nonARTrans = isARTran ? 0m : glTurnover;

				if (dict.TryGetValue(key, out result))
				{
					result.GLTurnover += glTurnover;
					result.NonXXTrans += nonARTrans;
				}
				else
				{
					result = new DiscrepancyByAccountEnqResult();
					result.GLTurnover = glTurnover;
					result.XXTurnover = 0m;
					result.NonXXTrans = nonARTrans;
					graphGL.GLTranEnq.Cache.RestoreCopy(result, gltran);
					dict.Add(key, result);
				}
			}

			#endregion

			#region AR balances

			ARCustomerBalanceEnq graphAR = PXGraph.CreateInstance<ARCustomerBalanceEnq>();
			ARCustomerBalanceEnq.ARHistoryFilter filterAR = PXCache<ARCustomerBalanceEnq.ARHistoryFilter>.CreateCopy(graphAR.Filter.Current);

			filterAR.BranchID = header.BranchID;
			filterAR.IncludeChildAccounts = false;
			filterAR.ShowWithBalanceOnly = false;
			filterAR.SplitByCurrency = false;

			foreach (KeyValuePair<DiscrepancyByAccountEnqResultKey, DiscrepancyByAccountEnqResult> pair in dict)
			{
				DiscrepancyByAccountEnqResultKey key = pair.Key;
				DiscrepancyByAccountEnqResult result = pair.Value;

				filterAR.Period = pair.Key.FinPeriodID;
				filterAR.ARAcctID = pair.Key.AccountID;
				filterAR.ARSubID = pair.Key.SubID;
				filterAR = graphAR.Filter.Update(filterAR);

				foreach (ARCustomerBalanceEnq.ARHistoryResult res in graphAR.History.Select())
				{
					result.XXTurnover += (res.Balance ?? 0m) - (res.Deposits ?? 0m);
				}
			}

			#endregion

			list.AddRange(dict.Values.Where(result =>
				header.ShowOnlyWithDiscrepancy != true ||
				result.Discrepancy != 0m));
			return list;
		}

		#region Actions

		public PXAction<ARGLDiscrepancyEnqFilter> viewDetails;

		[PXLookupButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			ARGLDiscrepancyEnqFilter header = Filter.Current;
			DiscrepancyByAccountEnqResult row = Rows.Current;

			if (header != null && row != null)
			{
				ARGLDiscrepancyByCustomerEnq detailsGraph = PXGraph.CreateInstance<ARGLDiscrepancyByCustomerEnq>();
				ARGLDiscrepancyByCustomerEnqFilter filter = detailsGraph.Filter.Current;

				filter.BranchID = row.BranchID;
				filter.PeriodFrom = row.FinPeriodID;
				filter.AccountID = row.AccountID;
				filter.SubCD = GetSubCD(row.SubID);
				filter.ShowOnlyWithDiscrepancy = header.ShowOnlyWithDiscrepancy;
				detailsGraph.Filter.Select();

				PXRedirectHelper.TryRedirect(detailsGraph, PXRedirectHelper.WindowMode.New);
			}

			return adapter.Get();
		}

		#endregion
	}
}