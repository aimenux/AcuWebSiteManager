using System;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.IN;
using static PX.Objects.AP.APVendorPriceMaint;

namespace PX.Objects.AP
{
	public class APVendorPriceMaintTemplateItemExtension : PXGraphExtension<APVendorPriceMaint>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.matrixItem>();

		public delegate BqlCommand CreateUnitCostSelectCommandOrig(bool isBaseUOM);
		[PXOverride]
		public virtual BqlCommand CreateUnitCostSelectCommand(bool isBaseUOM, CreateUnitCostSelectCommandOrig baseMethod)
		{
			BqlCommand command = baseMethod(isBaseUOM);

			if (!isBaseUOM)
			{
				command = BqlCommand.AppendJoin<InnerJoin<InventoryItem,
					   On<InventoryItem.inventoryID, Equal<APVendorPrice.inventoryID>>>>(command);
			}

			return command.OrderByNew(typeof(OrderBy<Desc<APVendorPrice.isPromotionalPrice,
						Desc<APVendorPrice.siteID,
						Desc<APVendorPrice.vendorID,
						Asc<InventoryItem.isTemplate,
						Desc<APVendorPrice.breakQty>>>>>>));
		}

		public delegate int?[] GetInventoryIDsOrig(PXCache sender, int? inventoryID);
		[PXOverride]
		public virtual int?[] GetInventoryIDs(PXCache sender, int? inventoryID, GetInventoryIDsOrig baseMethod)
		{
			int? templateInventoryID = InventoryItem.PK.Find(sender.Graph, inventoryID)?.TemplateItemID;

			return templateInventoryID != null ? new int?[] { inventoryID, templateInventoryID } : baseMethod(sender, inventoryID);
		}
	}
}