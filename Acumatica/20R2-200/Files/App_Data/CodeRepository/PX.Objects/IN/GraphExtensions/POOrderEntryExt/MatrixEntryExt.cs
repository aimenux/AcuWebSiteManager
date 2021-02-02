using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.Interfaces;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using PX.Objects.PO;

namespace PX.Objects.IN.GraphExtensions.POOrderEntryExt
{
	public class MatrixEntryExt : Matrix.GraphExtensions.SmartPanelExt<POOrderEntry, POOrder>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.matrixItem>();
		}

		protected override IEnumerable<IMatrixItemLine> GetLines(int? siteID, int? inventoryID)
			=> Base.Transactions.SelectMain().Where(l => l.InventoryID == inventoryID && (l.SiteID == siteID || siteID == null));

		protected override IEnumerable<IMatrixItemLine> GetLines(int? siteID, int? inventoryID, string taxCategoryID)
			=> Base.Transactions.SelectMain().Where(l => l.InventoryID == inventoryID && (l.SiteID == siteID || siteID == null) && (l.TaxCategoryID == taxCategoryID || taxCategoryID == null));


		protected override void UpdateLine(IMatrixItemLine line)
			=> Base.Transactions.Update((POLine)line);

		protected override void CreateNewLine(int? siteID, int? inventoryID, decimal qty)
			=> CreateNewLine(siteID, inventoryID, null, qty);

		protected override void CreateNewLine(int? siteID, int? inventoryID, string taxCategoryID, decimal qty)
		{
			POLine newline = PXCache<POLine>.CreateCopy(Base.Transactions.Insert(new POLine()));

			Base.Transactions.Cache.SetValueExt<POLine.inventoryID>(newline, inventoryID);
			newline = PXCache<POLine>.CreateCopy(Base.Transactions.Update(newline));

			if (siteID != null)
			{
				newline.SiteID = siteID;
				newline = PXCache<POLine>.CreateCopy(Base.Transactions.Update(newline));
			}

			newline.OrderQty = qty;
			newline = Base.Transactions.Update(newline);

			if (!string.IsNullOrEmpty(taxCategoryID))
			{
				Base.Transactions.Cache.SetValueExt<POLine.taxCategoryID>(newline, taxCategoryID);
				newline = Base.Transactions.Update(newline);
			}
		}

		protected override bool IsDocumentOpen()
			=> Base.Transactions.Cache.AllowInsert;

		protected override void DeductAllocated(SiteStatus allocated, IMatrixItemLine line)
		{
			POLine poLine = (POLine)line;

			decimal lineQtyAvail = 0m;
			decimal lineQtyHardAvail = 0m;

			decimal signQtyAvail;
			decimal signQtyHardAvail;
			INItemPlanIDAttribute.GetInclQtyAvail<SiteStatus>(Base.Transactions.Cache, poLine, out signQtyAvail, out signQtyHardAvail);

			if (signQtyAvail != 0m)
			{
				lineQtyAvail -= signQtyAvail * (poLine.BaseOrderQty ?? 0m);
			}

			if (signQtyHardAvail != 0m)
			{
				lineQtyHardAvail -= signQtyHardAvail * (poLine.BaseOrderQty ?? 0m);
			}

			allocated.QtyAvail += lineQtyAvail;
			allocated.QtyHardAvail += lineQtyHardAvail;
		}

		protected override string GetAvailabilityMessage(int? siteID, InventoryItem item, SiteStatus allocated)
		{
			return PXMessages.LocalizeFormatNoPrefix(PO.Messages.Availability_POOrder,
						item.BaseUnit,
						FormatQty(allocated.QtyOnHand),
						FormatQty(allocated.QtyAvail),
						FormatQty(allocated.QtyHardAvail),
						FormatQty(allocated.QtyActual),
						FormatQty(allocated.QtyPOOrders));
		}

		protected override int? GetQtyPrecision()
		{
			object returnValue = null;
			Base.Transactions.Cache.RaiseFieldSelecting<POOrder.orderQty>(null, ref returnValue, true);
			if (returnValue is PXDecimalState state)
				return state.Precision;
			return null;
		}

		protected override bool IsItemStatusDisabled(InventoryItem item)
		{
			return base.IsItemStatusDisabled(item) || item?.ItemStatus == InventoryItemStatus.NoPurchases;
		}
	}
}
