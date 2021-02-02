using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CM;

namespace ReconciliationTools
{
	#region Internal Types

	[Serializable]
	public partial class ARGLDiscrepancyByCustomerEnqFilter : ARGLDiscrepancyEnqFilter
	{
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		[Customer(DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID
		{
			get;
			set;
		}
		#endregion
	}

	[Serializable]
	public partial class ARGLDiscrepancyByCustomerEnqResult : ARCustomerBalanceEnq.ARHistoryResult, IDiscrepancyEnqResult
	{
		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

		[PXDimensionSelector(CustomerAttribute.DimensionName, typeof(Customer.acctCD), typeof(acctCD),
			typeof(Customer.acctCD), typeof(Customer.acctName))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
		public override string AcctCD
		{
			get;
			set;
		}
		#endregion
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
		[PXUIField(DisplayName = "AR Turnover")]
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
	public class ARGLDiscrepancyByCustomerEnq : ARGLDiscrepancyEnqGraphBase<ARGLDiscrepancyByAccountEnq, ARGLDiscrepancyByCustomerEnqFilter, ARGLDiscrepancyByCustomerEnqResult>
	{
		#region CacheAttached

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Financial Period")]
		protected virtual void ARGLDiscrepancyByCustomerEnqFilter_PeriodFrom_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault]
		protected virtual void ARGLDiscrepancyByCustomerEnqFilter_AccountID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault]
		protected virtual void ARGLDiscrepancyByCustomerEnqFilter_SubCD_CacheAttached(PXCache sender) { }

		#endregion

		protected override List<ARGLDiscrepancyByCustomerEnqResult> SelectDetails()
		{
			var list = new List<ARGLDiscrepancyByCustomerEnqResult>();
			ARGLDiscrepancyByCustomerEnqFilter header = Filter.Current;

			if (header == null ||
				header.BranchID == null ||
				header.PeriodFrom == null ||
				header.AccountID == null ||
				header.SubCD == null)
			{
				return list;
			}

			#region AR balances

			ARCustomerBalanceEnq graphAR = PXGraph.CreateInstance<ARCustomerBalanceEnq>();
			ARCustomerBalanceEnq.ARHistoryFilter filterAR = PXCache<ARCustomerBalanceEnq.ARHistoryFilter>.CreateCopy(graphAR.Filter.Current);

			filterAR.BranchID = header.BranchID;
			filterAR.CustomerID = header.CustomerID;
			filterAR.Period = header.PeriodFrom;
			filterAR.ARAcctID = header.AccountID;
			filterAR.SubCD = header.SubCD;
			filterAR.IncludeChildAccounts = false;
			filterAR.ShowWithBalanceOnly = false;
			filterAR.SplitByCurrency = false;
			filterAR = graphAR.Filter.Update(filterAR);

			Dictionary<int, ARGLDiscrepancyByCustomerEnqResult> dict = new Dictionary<int, ARGLDiscrepancyByCustomerEnqResult>();

			foreach (ARCustomerBalanceEnq.ARHistoryResult res in graphAR.History.Select())
			{
				ARGLDiscrepancyByCustomerEnqResult result;
				int key = (int)res.CustomerID;
				decimal arTurnover = (res.Balance ?? 0m) - (res.Deposits ?? 0m);

				if (dict.TryGetValue(key, out result))
				{
					result.XXTurnover += arTurnover;
				}
				else
				{
					result = new ARGLDiscrepancyByCustomerEnqResult();
					result.GLTurnover = 0m;
					result.XXTurnover = arTurnover;
					PXCache<ARCustomerBalanceEnq.ARHistoryResult>.RestoreCopy(result, res);
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
					row.Module == BatchModule.AR &&
					row.ReferenceID != null &&
					(header.CustomerID == null || row.ReferenceID == header.CustomerID)))
			{
				ARGLDiscrepancyByCustomerEnqResult result;
				int key = (int)gltran.ReferenceID;
				decimal glTurnover = CalcGLTurnover(gltran);

				if (dict.TryGetValue(key, out result))
				{
					result.GLTurnover += glTurnover;
				}
				else
				{
					Customer customer = PXSelect<Customer, 
						Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.SelectSingleBound(this, null, key);

					if (customer != null)
					{
						result = new ARGLDiscrepancyByCustomerEnqResult();
						result.CustomerID = customer.BAccountID;
						result.AcctCD = customer.AcctCD;
						result.AcctName = customer.AcctName;
						result.GLTurnover = glTurnover;
						result.XXTurnover = 0m;
						dict.Add(key, result);
					}
				}
			}

			#endregion

			list.AddRange(dict.Values.Where(result => 
				header.ShowOnlyWithDiscrepancy != true || 
				result.Discrepancy != 0m));
			return list;
		}

		#region Actions

		public PXAction<ARGLDiscrepancyByCustomerEnqFilter> viewDetails;

		[PXLookupButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			ARGLDiscrepancyByCustomerEnqFilter header = Filter.Current;
			ARGLDiscrepancyByCustomerEnqResult row = Rows.Current;

			if (header != null && row != null)
			{
				ARGLDiscrepancyByDocumentEnq detailsGraph = PXGraph.CreateInstance<ARGLDiscrepancyByDocumentEnq>();
				ARGLDiscrepancyByCustomerEnqFilter filter = detailsGraph.Filter.Current;
				filter.BranchID = header.BranchID;
				filter.CustomerID = row.CustomerID;
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