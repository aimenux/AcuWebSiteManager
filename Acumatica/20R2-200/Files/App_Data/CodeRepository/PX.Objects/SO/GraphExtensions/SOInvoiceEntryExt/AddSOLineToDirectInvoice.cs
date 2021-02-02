using System.Collections;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.SO.DAC.Projections;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	public class AddSOLineToDirectInvoice : PXGraphExtension<SOInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.advancedSOInvoices>();
		}

		public PXSelect<SOLineForDirectInvoice,
			  Where<SOLineForDirectInvoice.customerID, Equal<Current<ARInvoice.customerID>>>>
			  soLineList;

		public PXAction<ARInvoice> selectSOLine;
		[PXUIField(DisplayName = "Add SO Line", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SelectSOLine(PXAdapter adapter)
		{
			if (Base.Document.Cache.AllowUpdate)
				soLineList.AskExt();
			return adapter.Get();
		}

		public PXAction<ARInvoice> addSOLine;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddSOLine(PXAdapter adapter)
		{
			foreach (SOLineForDirectInvoice sol in soLineList.Cache.Updated)
			{
				if (sol.Selected != true) continue;

				var tran = (ARTran)Base.Transactions.Cache.CreateInstance();
				tran.SOOrderType = sol.OrderType;
				tran.SOOrderNbr = sol.OrderNbr;
				tran.SOOrderLineNbr = sol.LineNbr;

				Base.InsertInvoiceDirectLine(tran);

				sol.Selected = false;
			}
			return adapter.Get();
		}

		protected virtual void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = (ARInvoice)e.Row;
			selectSOLine.SetEnabled(Base.Document.AllowDelete && Base.Transactions.AllowInsert && row?.CustomerID != null);
		}
	}
}
