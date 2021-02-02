using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.CS;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CM;
using static PX.Objects.AP.APDocumentEnq;

namespace ReconciliationTools
{
	#region Internal Types

	[Serializable]
	public partial class APGLDiscrepancyByDocumentEnqResult : APDocumentResult, IDiscrepancyEnqResult
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
	public class APGLDiscrepancyByDocumentEnq : APGLDiscrepancyEnqGraphBase<APGLDiscrepancyByAccountEnq, APGLDiscrepancyByVendorEnqFilter, APGLDiscrepancyByDocumentEnqResult>
	{

		public APGLDiscrepancyByDocumentEnq()
		{
			PXUIFieldAttribute.SetRequired<APGLDiscrepancyByDocumentEnqResult.refNbr>(Caches[typeof(APGLDiscrepancyByDocumentEnqResult)], false);
			PXUIFieldAttribute.SetRequired<APGLDiscrepancyByDocumentEnqResult.finPeriodID>(Caches[typeof(APGLDiscrepancyByDocumentEnqResult)], false);
		}

		#region CacheAttached

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Financial Period")]
		protected virtual void APGLDiscrepancyByVendorEnqFilter_PeriodFrom_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault]
		protected virtual void APGLDiscrepancyByVendorEnqFilter_VendorID_CacheAttached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Original Amount")]
		protected virtual void APGLDiscrepancyByDocumentEnqResult_OrigDocAmt_CacheAttached(PXCache sender) { }

		#endregion

		public PXAction<APGLDiscrepancyByVendorEnqFilter> viewDocument;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (this.Rows.Current != null)
			{
				PXRedirectHelper.TryRedirect(Rows.Cache, Rows.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return Filter.Select();
		}

		protected override List<APGLDiscrepancyByDocumentEnqResult> SelectDetails()
		{
			var list = new List<APGLDiscrepancyByDocumentEnqResult>();
			APGLDiscrepancyByVendorEnqFilter header = Filter.Current;

			if (header == null ||
				header.BranchID == null ||
				header.PeriodFrom == null ||
				header.VendorID == null)
			{
				return list;
			}

			#region AP balances

			APDocumentEnq graphAP = PXGraph.CreateInstance<APDocumentEnq>();
			APDocumentEnq.APDocumentFilter filterAP = PXCache<APDocumentEnq.APDocumentFilter>.CreateCopy(graphAP.Filter.Current);

			filterAP.BranchID = header.BranchID;
			filterAP.VendorID = header.VendorID;
			filterAP.FinPeriodID = header.PeriodFrom;
			filterAP.AccountID = header.AccountID;
			filterAP.SubCD = header.SubCD;
			filterAP = graphAP.Filter.Update(filterAP);

			Dictionary<ARDocKey, APGLDiscrepancyByDocumentEnqResult> dict = new Dictionary<ARDocKey, APGLDiscrepancyByDocumentEnqResult>();
			HashSet<int?> accountIDs = new HashSet<int?>();
			HashSet<int?> subAccountIDs = new HashSet<int?>();
			graphAP.Documents.Select();

			foreach (KeyValuePair<ARDocKey, APDocumentResult> pair in graphAP.HandledDocuments)
			{
				ARDocKey key = pair.Key;
				APDocumentResult handledDocument = pair.Value;
				APGLDiscrepancyByDocumentEnqResult result;

				if (dict.TryGetValue(key, out result))
				{
					result.XXTurnover += (handledDocument.APTurnover ?? 0m);
				}
				else
				{
					result = new APGLDiscrepancyByDocumentEnqResult();
					result.GLTurnover = 0m;
					result.XXTurnover = (handledDocument.APTurnover ?? 0m);
					PXCache<APDocumentResult>.RestoreCopy(result, handledDocument);
					dict.Add(key, result);
				}

				accountIDs.Add(result.APAccountID);
			}

			#endregion

			#region GL balances

			AccountByPeriodEnq graphGL = PXGraph.CreateInstance<AccountByPeriodEnq>();
			AccountByPeriodFilter filterGL = PXCache<AccountByPeriodFilter>.CreateCopy(graphGL.Filter.Current);

			graphGL.Filter.Cache.SetDefaultExt<AccountByPeriodFilter.ledgerID>(filterGL);
			filterGL.BranchID = header.BranchID;
			filterGL.StartPeriodID = header.PeriodFrom;
			filterGL.EndPeriodID = header.PeriodFrom;
			filterGL.SubID = header.SubCD;
			filterGL = graphGL.Filter.Update(filterGL);

			foreach (int? accountID in accountIDs)
			{
				filterGL.AccountID = accountID;
				filterGL = graphGL.Filter.Update(filterGL);

				foreach (GLTranR gltran in graphGL.GLTranEnq.Select()
					.RowCast<GLTranR>()
					.Where(row =>
						row.Module == BatchModule.AP &&
						row.ReferenceID == header.VendorID))
				{
					ARDocKey key = new ARDocKey(gltran.TranType, gltran.RefNbr);
					APGLDiscrepancyByDocumentEnqResult result;

					if (dict.TryGetValue(key, out result))
					{
						decimal glTurnover = CalcGLTurnover(gltran);
						result.GLTurnover += glTurnover;
					}
				}
			}

			#endregion

			list.AddRange( dict.Values.Where(result =>
				header.ShowOnlyWithDiscrepancy != true ||
				result.Discrepancy != 0m));
			return list;
		}
	}
}