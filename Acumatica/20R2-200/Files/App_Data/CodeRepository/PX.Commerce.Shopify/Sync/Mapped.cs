using PX.Commerce.Shopify.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify
{
	#region SPMappedEntity
	public abstract class SPMappedEntity<ExternType, LocalType> : MappedEntity<ExternType, LocalType>
		where ExternType : BCAPIEntity, IExternEntity
		where LocalType : CBAPIEntity, ILocalEntity
	{
		public SPMappedEntity(String entType)
			: base(SPConnector.TYPE, entType)
		{ }
		public SPMappedEntity(BCSyncStatus status)
			: base(status)
		{
		}
		public SPMappedEntity(String entType, LocalType entity, Guid? id, DateTime? timestamp)
			: base(SPConnector.TYPE, entType, entity, id, timestamp)
		{
		}
		public SPMappedEntity(String entType, ExternType entity, String id, DateTime? timestamp)
			: base(SPConnector.TYPE, entType, entity, id, timestamp)
		{
		}
		public SPMappedEntity(String entType, ExternType entity, String id, String hash)
			: base(SPConnector.TYPE, entType, entity, id, hash)
		{
		}
	}
	#endregion

	#region MappedCustomer
	public class MappedCustomer : SPMappedEntity<CustomerData, Customer>
	{
		public const String TYPE = BCEntitiesAttribute.Customer;

		public MappedCustomer()
			: base(TYPE)
		{ }
		public MappedCustomer(Customer entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedCustomer(CustomerData entity, String id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
	}
	#endregion
	#region MappedLocation
	public class MappedLocation : SPMappedEntity<CustomerAddressData, CustomerLocation>
	{
		public const String TYPE = BCEntitiesAttribute.Address;

		public MappedLocation()
			: base(TYPE)
		{ }
		public MappedLocation(BCSyncStatus status)
			: base(status) { }
		public MappedLocation(CustomerLocation entity, Guid? id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
		}
		public MappedLocation(CustomerAddressData entity, String id, String hash, Int32? parent)
			: base(TYPE, entity, id, hash)
		{
			ParentID = parent;
		}
	}
	#endregion
	//#region MappedCategory
	//public class MappedCategory : SPMappedEntity<ProductCategoryData, BCItemSalesCategory>
	//{
	//	public const String TYPE = BCEntitiesAttribute.SalesCategory;

	//	public MappedCategory()
	//		: base(TYPE)
	//	{ }
	//	public MappedCategory(BCItemSalesCategory entity, Guid? id, DateTime? timestamp)
	//		: base(TYPE, entity, id, timestamp) { }
	//	public MappedCategory(ProductCategoryData entity, String id, DateTime? timestamp)
	//		: base(TYPE, entity, id, timestamp) { }
	//	public MappedCategory(ProductCategoryData entity, String id, String hash)
	//		: base(TYPE, entity, id, hash) { }
	//}
	//#endregion
	#region MappedStockItem
	public class MappedStockItem : SPMappedEntity<ProductData, StockItem>
	{
		public const String TYPE = BCEntitiesAttribute.StockItem;

		public MappedStockItem()
			: base(TYPE)
		{ }
		public MappedStockItem(StockItem entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedStockItem(ProductData entity, String id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
	}
	#endregion
	#region MappedNonStockItem
	public class MappedNonStockItem : SPMappedEntity<ProductData, NonStockItem>
	{
		public const String TYPE = BCEntitiesAttribute.NonStockItem;

		public MappedNonStockItem()
			: base(TYPE)
		{ }
		public MappedNonStockItem(NonStockItem entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedNonStockItem(ProductData entity, String id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
	}
	#endregion

	#region MappedTemplateItem
	public class MappedTemplateItem : SPMappedEntity<ProductData, TemplateItems>
	{
		public const String TYPE = BCEntitiesAttribute.ProductWithVariant;

		public MappedTemplateItem()
			: base(TYPE)
		{ }
		public MappedTemplateItem(TemplateItems entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedTemplateItem(ProductData entity, String id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
	}
	#endregion

	#region MappedAvailability
	public class MappedAvailability : SPMappedEntity<InventoryLevelData, StorageDetailsResult>
	{
		public const String TYPE = BCEntitiesAttribute.ProductAvailability;

		public MappedAvailability()
			: base(TYPE)
		{ }
		public MappedAvailability(StorageDetailsResult entity, Guid? id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
			UpdateParentExternTS = true;
		}
		public MappedAvailability(InventoryLevelData entity, String id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
			UpdateParentExternTS = true;
		}
	}
	#endregion
	#region MappedProductImage
	public class MappedProductImage : SPMappedEntity<ProductImageData, ItemImageDetails>
	{
		public const String TYPE = BCEntitiesAttribute.ProductImage;

		public MappedProductImage()
			: base(TYPE)
		{ }
		public MappedProductImage(ItemImageDetails entity, Guid? id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
		}
		public MappedProductImage(ProductImageData entity, String id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
		}
	}
	#endregion
	#region MappedSalesOrder
	public class MappedOrder : SPMappedEntity<OrderData, SalesOrder>
	{
		public const String TYPE = BCEntitiesAttribute.Order;

		public MappedOrder()
			: base(TYPE)
		{ }
		public MappedOrder(SalesOrder entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedOrder(OrderData entity, String id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
	}
	#endregion
	#region MappedPayment
	public class MappedPayment : SPMappedEntity<OrderTransaction, Payment>
	{
		public const String TYPE = BCEntitiesAttribute.Payment;

		public MappedPayment()
			: base(TYPE)
		{ }
		public MappedPayment(Payment entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedPayment(OrderTransaction entity, String id, DateTime? timestamp, String hashcode)
			: base(TYPE, entity, id, timestamp) { ExternHash = hashcode; }
	}
	#endregion
	#region MappedShipment
	public class MappedShipment : SPMappedEntity<FulfillmentData, BCShipments>
	{
		public const String TYPE = BCEntitiesAttribute.Shipment;

		public MappedShipment()
			: base(TYPE)
		{ }
		public MappedShipment(BCShipments entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedShipment(FulfillmentData entity, String id, DateTime? timestamp, String hashcode)
			: base(TYPE, entity, id, timestamp) { ExternHash = hashcode; }
	}
	#endregion
	#region MappedRefunds
	public class MappedRefunds : SPMappedEntity<OrderData, SalesOrder>
	{
		public const String TYPE = BCEntitiesAttribute.OrderRefunds;

		public MappedRefunds()
			: base(TYPE)
		{ }
		public MappedRefunds(SalesOrder entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedRefunds(OrderData entity, String id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
	}
	#endregion
	//#region MappedSalesPrice
	//public class MappedBaseSalesPrice : SPMappedEntity<ProductsBulkPricingRules, SalesPriceDetail>
	//{
	//	public const String TYPE = BCEntitiesAttribute.SalesPrice;

	//	public MappedBaseSalesPrice()
	//		: base(TYPE)
	//	{ }
	//	public MappedBaseSalesPrice(SalesPriceDetail entity, Guid? id, DateTime? timestamp, Int32? parent)
	//		: base(TYPE, entity, id, timestamp)
	//	{
	//		ParentID = parent;
	//	}
	//	public MappedBaseSalesPrice(ProductsBulkPricingRules entity, String id, DateTime? timestamp, Int32? parent)
	//		: base(TYPE, entity, id, timestamp)
	//	{
	//		ParentID = parent;
	//	}
	//}
	//#endregion
}
