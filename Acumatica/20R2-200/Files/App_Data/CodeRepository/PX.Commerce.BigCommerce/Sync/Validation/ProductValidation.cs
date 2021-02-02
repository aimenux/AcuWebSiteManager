using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Commerce.Core;
using PX.Commerce.Objects;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Commerce.BigCommerce.API.REST;
using PX.Objects.IN;
using PX.Commerce.BigCommerce.API.WebDAV;

namespace PX.Commerce.BigCommerce
{
	public class ProductValidator : BCBaseValidator, ISettingsValidator, IExternValidator
	{
		public int Priority { get { return 0; } }

		public virtual void Validate(IProcessor iproc)
		{
			Validate<BCImageProcessor>(iproc, (processor) =>
			{
				BCBindingBigCommerce binding = processor.GetBindingExt<BCBindingBigCommerce>();
				BCWebDavClient webDavClient = BCConnector.GetWebDavClient(binding);
				var folder = webDavClient.GetFolder();
				if (folder == null) throw new PXException(BigCommerceMessages.TestConnectionFolderNotFound);
			});
			Validate<BCStockItemProcessor>(iproc, (processor) =>
			{
				BCEntity entity = processor.GetEntity();
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();
				if (entity.Direction != BCSyncDirectionAttribute.Export)
				{
					if (storeExt.InventoryNumberingID == null && BCDimensionMaskAttribute.GetAutoNumbering(PX.Objects.IN.StockItemAttribute.DimensionName) == null)
						throw new PXException(BigCommerceMessages.NoStockNumbering);

					if (storeExt.StockItemClassID == null)
					{
						INSetup inSetup = PXSelect<INSetup>.Select(processor);
						if (inSetup.DfltStkItemClassID == null)
							throw new PXException(BigCommerceMessages.NoStockItemClass);
					}
				}
			});
			Validate<BCNonStockItemProcessor>(iproc, (processor) =>
			{
				BCEntity entity = processor.GetEntity();
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();
				if (entity.Direction != BCSyncDirectionAttribute.Export)
				{
					if (storeExt.InventoryNumberingID == null && BCDimensionMaskAttribute.GetAutoNumbering(PX.Objects.IN.NonStockItemAttribute.DimensionName) == null)
						throw new PXException(BigCommerceMessages.NoNonStockNumbering);

					if (storeExt.NonStockItemClassID == null)
					{
						INSetup inSetup = PXSelect<INSetup>.Select(processor);
						if (inSetup.DfltNonStkItemClassID == null)
							throw new PXException(BigCommerceMessages.NoNonStockItemClass);
					}
				}
			});
			Validate<BCAvailabilityProcessor>(iproc, (processor) =>
			{
				BCBindingExt store = processor.GetBindingExt<BCBindingExt>();
			});
		}

		public virtual void Validate(IProcessor iproc, IExternEntity ientity)
		{
			Validate<BCStockItemProcessor, ProductData>(iproc, ientity, (processor, entity) =>
			{
				BCBindingExt store = processor.GetBindingExt<BCBindingExt>();
				if (entity.Categories.Count <= 0)
				{
					if (String.IsNullOrEmpty(store.StockSalesCategoriesIDs))
						throw new PXException(BigCommerceMessages.NoStockItemCategory);
					else throw new PXException(BigCommerceMessages.CategoriesNotSynced);
				}
			});
			Validate<BCNonStockItemProcessor, ProductData>(iproc, ientity, (processor, entity) =>
			{
				BCBindingExt store = processor.GetBindingExt<BCBindingExt>();
				if (entity.Categories.Count <= 0)
				{
					if (String.IsNullOrEmpty(store.NonStockSalesCategoriesIDs))
						throw new PXException(BigCommerceMessages.NoNonStockItemCategory);
					else throw new PXException(BigCommerceMessages.CategoriesNotSynced);
				}
			});
			Validate<BCTemplateItemProcessor, ProductData>(iproc, ientity, (processor, entity) =>
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.matrixItem>() == false)
					throw new PXException(BCMessages.MatrixFeatureRequired);
				BCBindingExt store = processor.GetBindingExt<BCBindingExt>();
				if (entity.Categories.Count <= 0)
				{
					if (entity.Type == ProductsType.Digital.ToEnumMemberAttrValue() &&  String.IsNullOrEmpty(store.NonStockSalesCategoriesIDs))
						throw new PXException(BigCommerceMessages.NoNonStockItemCategory);
					else if(entity.Type == ProductsType.Physical.ToEnumMemberAttrValue() && String.IsNullOrEmpty(store.StockSalesCategoriesIDs))
						throw new PXException(BigCommerceMessages.NoStockItemCategory);
					else throw new PXException(BigCommerceMessages.CategoriesNotSynced);
				}
			});
		}
	}
}
