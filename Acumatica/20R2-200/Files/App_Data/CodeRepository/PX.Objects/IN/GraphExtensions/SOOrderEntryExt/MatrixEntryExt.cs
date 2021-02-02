using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.Interfaces;
using PX.Objects.SO;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;

namespace PX.Objects.IN.GraphExtensions.SOOrderEntryExt
{
	public class MatrixEntryExt : Matrix.GraphExtensions.SmartPanelExt<SOOrderEntry, SOOrder>
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
			=> Base.Transactions.Update((SOLine)line);

		protected override void CreateNewLine(int? siteID, int? inventoryID, decimal qty)
			=> CreateNewLine(siteID, inventoryID, null, qty);

		protected override void CreateNewLine(int? siteID, int? inventoryID, string taxCategoryID, decimal qty)
		{
			SOLine newline = PXCache<SOLine>.CreateCopy(Base.Transactions.Insert(new SOLine()));
			newline.SiteID = siteID;
			newline.InventoryID = inventoryID;
			newline = PXCache<SOLine>.CreateCopy(Base.Transactions.Update(newline));
			if (newline.RequireLocation != true)
				newline.LocationID = null;
			newline = PXCache<SOLine>.CreateCopy(Base.Transactions.Update(newline));
			newline.Qty = qty;
			newline = Base.Transactions.Update(newline);

			if (!string.IsNullOrEmpty(taxCategoryID))
			{
				Base.Transactions.Cache.SetValueExt<SOLine.taxCategoryID>(newline, taxCategoryID);
				newline = Base.Transactions.Update(newline);
			}
		}

		protected override bool IsDocumentOpen()
			=> Base.Transactions.Cache.AllowInsert;

		protected override void DeductAllocated(SiteStatus allocated, IMatrixItemLine line)
		{
			SOLine soLine = (SOLine)line;

			allocated.QtyAvail += soLine.LineQtyAvail;
			allocated.QtyHardAvail += soLine.LineQtyHardAvail;
		}

		protected override string GetAvailabilityMessage(int? siteID, InventoryItem item, SiteStatus availability)
		{
			if (Base.lsselect.IsAllocationEntryEnabled)
			{
				decimal? allocated = GetLines(siteID, item.InventoryID).Sum(l => ((SOLine)l).LineQtyHardAvail);
				return PXMessages.LocalizeFormatNoPrefix(SO.Messages.Availability_AllocatedInfo,
						item.BaseUnit, FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail), FormatQty(allocated));
			}
			else
				return PXMessages.LocalizeFormatNoPrefix(Messages.Availability_Info,
						item.BaseUnit, FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));
		}

		protected override int? GetQtyPrecision()
		{
			object returnValue = null;
			Base.Transactions.Cache.RaiseFieldSelecting<SOOrder.orderQty>(null, ref returnValue, true);
			if (returnValue is PXDecimalState state)
				return state.Precision;
			return null;
		}

		protected override bool IsItemStatusDisabled(InventoryItem item)
		{
			return base.IsItemStatusDisabled(item) || item?.ItemStatus == InventoryItemStatus.NoSales;
		}
	}
}
