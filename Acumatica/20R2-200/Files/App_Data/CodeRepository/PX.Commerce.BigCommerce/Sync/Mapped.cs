using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce
{
	#region BCMappedEntity
	public abstract class BCMappedEntity<ExternType, LocalType> : MappedEntity<ExternType, LocalType>
		where ExternType : BCAPIEntity, IExternEntity
		where LocalType : CBAPIEntity, ILocalEntity
	{
		public BCMappedEntity(String entType)
			: base(BCConnector.TYPE, entType)
		{ }
		public BCMappedEntity(BCSyncStatus status)
			:base(status)
		{
		}
		public BCMappedEntity(String entType, LocalType entity, Guid? id, DateTime? timestamp)
			: base(BCConnector.TYPE, entType, entity, id, timestamp)
		{
		}
		public BCMappedEntity(String entType, ExternType entity, String id, DateTime? timestamp)
			: base(BCConnector.TYPE, entType, entity, id, timestamp)
		{
		}
		public BCMappedEntity(String entType, ExternType entity, String id, String hash)
			: base(BCConnector.TYPE, entType, entity, id, hash)
		{
		}
	}
	#endregion

	#region MappedCustomer
	public class MappedCustomer : BCMappedEntity<CustomerData, Customer>
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
	public class MappedLocation : BCMappedEntity<CustomerAddressData, CustomerLocation>
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
	#region MappedCategory
	public class MappedCategory : BCMappedEntity<ProductCategoryData, BCItemSalesCategory>
	{
		public const String TYPE = BCEntitiesAttribute.SalesCategory;

		public MappedCategory()
			: base(TYPE)
		{ }
		public MappedCategory(BCItemSalesCategory entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedCategory(ProductCategoryData entity, String id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedCategory(ProductCategoryData entity, String id, String hash)
			: base(TYPE, entity, id, hash) { }
	}
	#endregion
	#region MappedStockItem
	public class MappedStockItem : BCMappedEntity<ProductData, StockItem>
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
	#region MappedTemplateItem
	public class MappedTemplateItem : BCMappedEntity<ProductData, TemplateItems>
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

	#region MappedNonStockItem
	public class MappedNonStockItem : BCMappedEntity<ProductData, NonStockItem>
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
	#region MappedAvailability
	public class MappedAvailability : BCMappedEntity<ProductQtyData, StorageDetailsResult>
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
		public MappedAvailability(ProductQtyData entity, String id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
			UpdateParentExternTS = true;
		}
	}
	#endregion
	#region MappedProductImage
	public class MappedProductImage : BCMappedEntity<ProductsImageData, ItemImageDetails>
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
		public MappedProductImage(ProductsImageData entity, String id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
		}
	}
	#endregion
	#region MappedSalesOrder
	public class MappedOrder : BCMappedEntity<OrderData, SalesOrder>
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
	public class MappedPayment : BCMappedEntity<OrdersTransactionData, Payment>
	{
		public const String TYPE = BCEntitiesAttribute.Payment;

		public MappedPayment()
			: base(TYPE)
		{ }
		public MappedPayment(Payment entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedPayment(OrdersTransactionData entity, String id, DateTime? timestamp, String hashcode)
			: base(TYPE, entity, id, timestamp) { ExternHash = hashcode; }
	}
	#endregion
	#region MappedShipment
	public class MappedShipment : BCMappedEntity<OrdersShipmentData, BCShipments>
	{
		public const String TYPE = BCEntitiesAttribute.Shipment;

		public MappedShipment()
			: base(TYPE)
		{ }
		public MappedShipment(BCShipments entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedShipment(OrdersShipmentData entity, String id, DateTime? timestamp, String hashcode)
			: base(TYPE, entity, id, timestamp) { ExternHash = hashcode; }
	}
	#endregion
	#region MappedGroup
	public class MappedGroup : BCMappedEntity<CustomerGroupData, CustomerPriceClass>
	{
		public const String TYPE = BCEntitiesAttribute.CustomerPriceClass;

		public MappedGroup()
			: base(TYPE)
		{ }
		public MappedGroup(CustomerPriceClass entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedGroup(CustomerGroupData entity, String id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedGroup(CustomerGroupData entity, String id, String hash)
			: base(TYPE, entity, id, hash) { }
	}
	#endregion
	#region MappedSalesPrice
	public class MappedBaseSalesPrice : BCMappedEntity<BulkPricingWithSalesPrice, SalesPricesInquiry>
	{
		public const String TYPE = BCEntitiesAttribute.SalesPrice;

		public MappedBaseSalesPrice()
			: base(TYPE)
		{ }
		public MappedBaseSalesPrice(SalesPricesInquiry entity, Guid? id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
		}
		public MappedBaseSalesPrice(BulkPricingWithSalesPrice entity, String id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
		}
	}
	#endregion

	#region MappedPriceList
	public class MappedPriceList : BCMappedEntity<PriceList, PriceListSalesPrice>
	{
		public const String TYPE = BCEntitiesAttribute.PriceList;

		public MappedPriceList()
			: base(TYPE)
		{ }
		public MappedPriceList(PriceListSalesPrice entity, Guid? id, DateTime? timestamp,Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
		}
		public MappedPriceList(PriceList entity, String id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
		}
	}
	#endregion
}
