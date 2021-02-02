using System;
using PX.Data;

namespace PX.Objects.Common.Discount.Mappers
{
	public abstract class LineEntitiesFields : DiscountedLineMapperBase
	{
		public virtual int? InventoryID { get; set; }
		public virtual int? CustomerID { get; set; }
		public virtual int? SiteID { get; set; }
		public virtual int? BranchID { get; set; }
		public virtual int? VendorID { get; set; }

		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		public abstract class suppliedByVendorID : PX.Data.BQL.BqlInt.Field<suppliedByVendorID> { }

		protected LineEntitiesFields(PXCache cache, object row) : base(cache, row) { }

		public static LineEntitiesFields GetMapFor<TLine>(TLine line, PXCache cache)
			=> new LineEntitiesFields<inventoryID, customerID, siteID, branchID, vendorID, suppliedByVendorID>(cache, line);
	}

	public class LineEntitiesFields<InventoryField, CustomerField, SiteField, BranchField, VendorField>
		: LineEntitiesFields
		where InventoryField : IBqlField
		where CustomerField : IBqlField
		where SiteField : IBqlField
		where BranchField : IBqlField
		where VendorField : IBqlField
	{
		public LineEntitiesFields(PXCache cache, object row) : base(cache, row) { }

		public override Type GetField<T>()
		{
			if (typeof(T) == typeof(inventoryID))
			{
				return typeof(InventoryField);
			}
			if (typeof(T) == typeof(customerID))
			{
				return typeof(CustomerField);
			}
			if (typeof(T) == typeof(siteID))
			{
				return typeof(SiteField);
			}
			if (typeof(T) == typeof(branchID))
			{
				return typeof(BranchField);
			}
			if (typeof(T) == typeof(vendorID))
			{
				return typeof(VendorField);
			}
			return null;
		}

		public override int? InventoryID => (int?)Cache.GetValue<InventoryField>(MappedLine);
		public override int? CustomerID => (int?)Cache.GetValue<CustomerField>(MappedLine);
		public override int? SiteID => (int?)Cache.GetValue<SiteField>(MappedLine);
		public override int? BranchID => (int?)Cache.GetValue<BranchField>(MappedLine);
		public override int? VendorID => (int?)Cache.GetValue<VendorField>(MappedLine);
	}

	public class LineEntitiesFields<InventoryField, CustomerField, SiteField, BranchField, VendorField, SuppliedByVendorField>
		: LineEntitiesFields<InventoryField, CustomerField, SiteField, BranchField, VendorField>
		where InventoryField : IBqlField
		where CustomerField : IBqlField
		where SiteField : IBqlField
		where BranchField : IBqlField
		where VendorField : IBqlField
		where SuppliedByVendorField : IBqlField
	{
		public LineEntitiesFields(PXCache cache, object row) : base(cache, row) { }

		public override int? VendorID => (int?)Cache.GetValue<SuppliedByVendorField>(MappedLine) ?? (int?)Cache.GetValue<VendorField>(MappedLine);
	}
}