using System;
using System.Collections;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO.DAC.Projections;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	public class AddReturnLineToDirectInvoice : PXGraphExtension<SOInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.advancedSOInvoices>();
		}

		[PXCopyPasteHiddenView]
		public PXSelect<ARTranForDirectInvoice,
		  Where<ARTranForDirectInvoice.customerID, Equal<Current<ARInvoice.customerID>>>>
		  arTranList;

		public PXAction<ARInvoice> selectARTran;
		[PXUIField(DisplayName = "Add Return Line", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SelectARTran(PXAdapter adapter)
		{
			if (Base.Document.Cache.AllowUpdate)
				arTranList.AskExt();
			return adapter.Get();
		}

		public PXAction<ARInvoice> addARTran;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddARTran(PXAdapter adapter)
		{
			foreach (ARTranForDirectInvoice origTran in arTranList.Cache.Updated)
			{
				if (origTran.Selected != true) continue;

				var tran = (ARTran)Base.Transactions.Cache.CreateInstance();
				tran.InventoryID = origTran.InventoryID;
				tran.SubItemID = origTran.SubItemID;
				tran.SiteID = origTran.SiteID;
				tran.LocationID = origTran.LocationID;
				tran.LotSerialNbr = origTran.LotSerialNbr;
				tran.ExpireDate = origTran.ExpireDate;
				tran.UOM = origTran.UOM;
				tran.Qty = INTranType.InvtMultFromInvoiceType(Base.Document.Current.DocType) * Math.Abs(origTran.Qty ?? 0m);
				tran.CuryUnitPrice = origTran.CuryUnitPrice;
				tran.OrigInvoiceType = origTran.TranType;
				tran.OrigInvoiceNbr = origTran.RefNbr;
				tran.OrigInvoiceLineNbr = origTran.LineNbr;  // not necessary

				Base.InsertInvoiceDirectLine(tran);

				origTran.Selected = false;
			}
			return adapter.Get();
		}

		protected void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = (ARInvoice)e.Row;
			selectARTran.SetEnabled(Base.Document.AllowDelete && Base.Transactions.AllowInsert && row?.CustomerID != null);
		}
	}
}
