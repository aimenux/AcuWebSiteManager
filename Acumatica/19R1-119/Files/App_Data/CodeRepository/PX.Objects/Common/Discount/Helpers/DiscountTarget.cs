using PX.Data;

namespace PX.Objects.Common.Discount
{
	/// <summary>
	/// Discount Targets
	/// </summary>
	public static class DiscountTarget
	{
		public const string Customer = "CU";
		public const string CustomerAndInventory = "CI";
		public const string CustomerAndInventoryPrice = "CP";
		public const string CustomerPrice = "CE";
		public const string CustomerPriceAndInventory = "PI";
		public const string CustomerPriceAndInventoryPrice = "PP";
		public const string CustomerAndBranch = "CB";
		public const string CustomerPriceAndBranch = "PB";

		public const string Warehouse = "WH";
		public const string WarehouseAndInventory = "WI";
		public const string WarehouseAndCustomer = "WC";
		public const string WarehouseAndInventoryPrice = "WP";
		public const string WarehouseAndCustomerPrice = "WE";

		public const string Branch = "BR";

		public const string Vendor = "VE";
		public const string VendorAndInventory = "VI";
		public const string VendorAndInventoryPrice = "VP";
		public const string VendorLocation = "VL";
		public const string VendorLocationAndInventory = "LI";

		public const string Inventory = "IN";
		public const string InventoryPrice = "IE";

		public const string Unconditional = "UN";

		public class customer : PX.Data.BQL.BqlString.Constant<customer> { public customer() : base(Customer) { } }
		public class customerAndInventory : PX.Data.BQL.BqlString.Constant<customerAndInventory> { public customerAndInventory() : base(CustomerAndInventory) { } }
		public class customerAndInventoryPrice : PX.Data.BQL.BqlString.Constant<customerAndInventoryPrice> { public customerAndInventoryPrice() : base(CustomerAndInventoryPrice) { } }
		public class customerPrice : PX.Data.BQL.BqlString.Constant<customerPrice> { public customerPrice() : base(CustomerPrice) { } }
		public class customerPriceAndInventory : PX.Data.BQL.BqlString.Constant<customerPriceAndInventory> { public customerPriceAndInventory() : base(CustomerPriceAndInventory) { } }
		public class customerPriceAndInventoryPrice : PX.Data.BQL.BqlString.Constant<customerPriceAndInventoryPrice> { public customerPriceAndInventoryPrice() : base(CustomerPriceAndInventoryPrice) { } }
		public class customerAndBranch : PX.Data.BQL.BqlString.Constant<customerAndBranch> { public customerAndBranch() : base(CustomerAndBranch) { } }
		public class customerPriceAndBranch : PX.Data.BQL.BqlString.Constant<customerPriceAndBranch> { public customerPriceAndBranch() : base(CustomerPriceAndBranch) { } }

		public class warehouse : PX.Data.BQL.BqlString.Constant<warehouse> { public warehouse() : base(Warehouse) { } }
		public class warehouseAndInventory : PX.Data.BQL.BqlString.Constant<warehouseAndInventory> { public warehouseAndInventory() : base(WarehouseAndInventory) { } }
		public class warehouseAndCustomer : PX.Data.BQL.BqlString.Constant<warehouseAndCustomer> { public warehouseAndCustomer() : base(WarehouseAndCustomer) { } }
		public class warehouseAndInventoryPrice : PX.Data.BQL.BqlString.Constant<warehouseAndInventoryPrice> { public warehouseAndInventoryPrice() : base(WarehouseAndInventoryPrice) { } }
		public class warehouseAndCustomerPrice : PX.Data.BQL.BqlString.Constant<warehouseAndCustomerPrice> { public warehouseAndCustomerPrice() : base(WarehouseAndCustomerPrice) { } }

		public class branch : PX.Data.BQL.BqlString.Constant<branch> { public branch() : base(Branch) { } }

		public class vendor : PX.Data.BQL.BqlString.Constant<vendor> { public vendor() : base(Vendor) { } }
		public class vendorAndInventory : PX.Data.BQL.BqlString.Constant<vendorAndInventory> { public vendorAndInventory() : base(VendorAndInventory) { } }
		public class vendorAndInventoryPrice : PX.Data.BQL.BqlString.Constant<vendorAndInventoryPrice> { public vendorAndInventoryPrice() : base(VendorAndInventoryPrice) { } }
		public class vendorLocation : PX.Data.BQL.BqlString.Constant<vendorLocation> { public vendorLocation() : base(VendorLocation) { } }
		public class vendorLocationAndInventory : PX.Data.BQL.BqlString.Constant<vendorLocationAndInventory> { public vendorLocationAndInventory() : base(VendorLocationAndInventory) { } }

		public class inventory : PX.Data.BQL.BqlString.Constant<inventory> { public inventory() : base(Inventory) { } }
		public class inventoryPrice : PX.Data.BQL.BqlString.Constant<inventoryPrice> { public inventoryPrice() : base(InventoryPrice) { } }

		public class unconditional : PX.Data.BQL.BqlString.Constant<unconditional> { public unconditional() : base(Unconditional) { } }
	}
}