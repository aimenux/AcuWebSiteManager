using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.CM;

namespace ReconciliationTools
{
	#region Internal Types

	[Serializable]
	public partial class APGLDiscrepancyByVendorEnqFilter : APGLDiscrepancyEnqFilter
	{
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[Vendor(DescriptionField = typeof(Vendor.acctName))]
		public virtual int? VendorID
		{
			get;
			set;
		}
		#endregion
	}

	[Serializable]
	public partial class APGLDiscrepancyByVendorEnqResult : APVendorBalanceEnq.APHistoryResult, IDiscrepancyEnqResult
	{
		#region GLTurnover
		public abstract class gLTurnover : PX.Data.BQL.BqlDecimal.Field<gLTurnover> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "GL Turnover")]
		public virtual decimal? GLTurnover
		{
			get;
			set;
		}
		#endregion
		#region XXTurnover
		public abstract class xXTurnover : PX.Data.BQL.BqlDecimal.Field<xXTurnover> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "AP Turnover")]
		public virtual decimal? XXTurnover
		{
			get;
			set;
		}
		#endregion
		#region Discrepancy
		public abstract class discrepancy : PX.Data.BQL.BqlDecimal.Field<discrepancy> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Discrepancy")]
		public virtual decimal? Discrepancy
		{
			get
			{
				return GLTurnover - XXTurnover;
			}
		}
		#endregion
	}

	#endregion

	[TableAndChartDashboardType]
	public class APGLDiscrepancyByVendorEnq : APGLDiscrepancyEnqGraphBase<APGLDiscrepancyByAccountEnq, APGLDiscrepancyByVendorEnqFilter, APGLDiscrepancyByVendorEnqResult>
	{
		#region CacheAttached

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Financial Period")]
		protected virtual void APGLDiscrepancyByVendorEnqFilter_PeriodFrom_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault]
		protected virtual void APGLDiscrepancyByVendorEnqFilter_AccountID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault]
		protected virtual void APGLDiscrepancyByVendorEnqFilter_SubCD_CacheAttached(PXCache sender) { }

		#endregion

		protected override List<APGLDiscrepancyByVendorEnqResult> SelectDetails()
		{
			var list = new List<APGLDiscrepancyByVendorEnqResult>();
			APGLDiscrepancyByVendorEnqFilter header = Filter.Current;

			if (header == null ||
				header.BranchID == null ||
				header.PeriodFrom == null ||
				header.AccountID == null ||
				header.SubCD == null)
			{
				return list;
			}

			#region AP balances

			APVendorBalanceEnq graphAP = PXGraph.CreateInstance<APVendorBalanceEnq>();
			APVendorBalanceEnq.APHistoryFilter filterAP = PXCache<APVendorBalanceEnq.APHistoryFilter>.CreateCopy(graphAP.Filter.Current);

			filterAP.BranchID = header.BranchID;
			filterAP.VendorID = header.VendorID;
			filterAP.FinPeriodID = header.PeriodFrom;
			filterAP.AccountID = header.AccountID;
			filterAP.SubID = header.SubCD;
			filterAP.ByFinancialPeriod = true;
			filterAP.ShowWithBalanceOnly = false;
			filterAP.SplitByCurrency = false;
			filterAP = graphAP.Filter.Update(filterAP);

			Dictionary<int, APGLDiscrepancyByVendorEnqResult> dict = new Dictionary<int, APGLDiscrepancyByVendorEnqResult>();

			foreach (APVendorBalanceEnq.APHistoryResult res in graphAP.History.Select())
			{
				APGLDiscrepancyByVendorEnqResult result;
				int key = (int)res.VendorID;
				decimal apTurnover = (res.Balance ?? 0m) - (res.Deposits ?? 0m);

				if (dict.TryGetValue(key, out result))
				{
					result.XXTurnover += apTurnover;
				}
				else
				{
					result = new APGLDiscrepancyByVendorEnqResult();
					result.GLTurnover = 0m;
					result.XXTurnover = apTurnover;
					PXCache<APVendorBalanceEnq.APHistoryResult>.RestoreCopy(result, res);
					dict.Add(key, result);
				}
			}

			#endregion

			#region GL balances

			AccountByPeriodEnq graphGL = PXGraph.CreateInstance<AccountByPeriodEnq>();
			AccountByPeriodFilter filterGL = PXCache<AccountByPeriodFilter>.CreateCopy(graphGL.Filter.Current);

			graphGL.Filter.Cache.SetDefaultExt<AccountByPeriodFilter.ledgerID>(filterGL);
			filterGL.BranchID = header.BranchID;
			filterGL.StartPeriodID = header.PeriodFrom;
			filterGL.EndPeriodID = header.PeriodFrom;
			filterGL.AccountID = header.AccountID;
			filterGL.SubID = header.SubCD;
			filterGL = graphGL.Filter.Update(filterGL);

			foreach (GLTranR gltran in graphGL.GLTranEnq.Select()
				.RowCast<GLTranR>()
				.Where(row =>
					row.Module == BatchModule.AP &&
					row.ReferenceID != null &&
					(header.VendorID == null || row.ReferenceID == header.VendorID)))
			{
				APGLDiscrepancyByVendorEnqResult result;
				int key = (int)gltran.ReferenceID;
				decimal glTurnover = CalcGLTurnover(gltran);

				if (dict.TryGetValue(key, out result))
				{
					result.GLTurnover += glTurnover;
				}
				else
				{
					Vendor vendor = PXSelect<Vendor,
						Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.SelectSingleBound(this, null, key);

					if (vendor != null)
					{
						result = new APGLDiscrepancyByVendorEnqResult();
						result.VendorID = vendor.BAccountID;
						result.AcctCD = vendor.AcctCD;
						result.AcctName = vendor.AcctName;
						result.GLTurnover = glTurnover;
						result.XXTurnover = 0m;
						dict.Add(key, result);
					}
				}
			}

			#endregion

			list.AddRange( dict.Values.Where(result =>
				header.ShowOnlyWithDiscrepancy != true ||
				result.Discrepancy != 0m));
			return list;
		}

		#region Actions

		public PXAction<APGLDiscrepancyByVendorEnqFilter> viewDetails;

		[PXLookupButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			APGLDiscrepancyByVendorEnqFilter header = Filter.Current;
			APGLDiscrepancyByVendorEnqResult row = Rows.Current;

			if (header != null && row != null)
			{
				APGLDiscrepancyByDocumentEnq detailsGraph = PXGraph.CreateInstance<APGLDiscrepancyByDocumentEnq>();
				APGLDiscrepancyByVendorEnqFilter filter = detailsGraph.Filter.Current;
				filter.BranchID = header.BranchID;
				filter.VendorID = row.VendorID;
				filter.PeriodFrom = header.PeriodFrom;
				filter.AccountID = header.AccountID;
				filter.SubCD = header.SubCD;
				filter.ShowOnlyWithDiscrepancy = header.ShowOnlyWithDiscrepancy;
				detailsGraph.Filter.Select();

				PXRedirectHelper.TryRedirect(detailsGraph, PXRedirectHelper.WindowMode.New);
			}

			return adapter.Get();
		}

		#endregion
	}
}