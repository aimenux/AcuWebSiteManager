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
using PX.Commerce.Shopify.API.REST;
using PX.Api;

namespace PX.Commerce.Shopify
{
	public class CustomerValidator : BCBaseValidator, ISettingsValidator, IExternValidator
	{
		public int Priority { get { return 0; } }

		public virtual void Validate(IProcessor iproc)
		{
			Validate<SPCustomerProcessor>(iproc, (processor) =>
			{
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();
				if (storeExt.CustomerNumberingID == null && BCDimensionMaskAttribute.GetAutoNumbering(CustomerRawAttribute.DimensionName) == null)
					throw new PXException(ShopifyMessages.NoCustomerNumbering);

				if(storeExt.CustomerClassID == null)
				{
					ARSetup arSetup = PXSelect<ARSetup>.Select(processor);
					if (arSetup.DfltCustomerClassID == null)
						throw new PXException(ShopifyMessages.NoCustomerClass);
				}
				
			});
			Validate<SPLocationProcessor>(iproc, (processor) =>
			{
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();
				if (storeExt.CustomerNumberingID == null && BCDimensionMaskAttribute.GetAutoNumbering(CustomerRawAttribute.DimensionName) == null)
					throw new PXException(ShopifyMessages.NoCustomerNumbering);
				if (storeExt.LocationNumberingID == null && BCDimensionMaskAttribute.GetAutoNumbering(LocationIDAttribute.DimensionName) == null)
					throw new PXException(ShopifyMessages.NoLocationNumbering);
				
			});
		}

		public virtual void Validate(IProcessor iproc, IExternEntity ientity)
		{
			Validate<SPCustomerProcessor, CustomerData>(iproc, ientity, (processor, entity) =>
			{
				if(String.IsNullOrWhiteSpace(entity.Email) && String.IsNullOrWhiteSpace(entity.Phone))
					throw new PXException(ShopifyMessages.NoRequiredField, "Email or Phone", "Customer");

				//if (String.IsNullOrWhiteSpace(entity.FirstName) || String.IsNullOrWhiteSpace(entity.LastName))
				//	throw new PXException(ShopifyMessages.NoRequiredField, "Company Name", "Customer");
			});
			Validate<SPCustomerProcessor, CustomerAddressData>(iproc, ientity, (processor, entity) =>
			{
				//if (String.IsNullOrWhiteSpace(entity.Company))
				//	throw new PXException(BigCommerceMessages.NoRequiredField, "Company Name", "Customer");

				//if (String.IsNullOrWhiteSpace(entity.PostalCode))
				//	throw new PXException(ShopifyMessages.NoRequiredField, "Postal Code", "Customer");
			});
			Validate<SPCustomerProcessor, CustomerAddressData>(iproc, ientity, (processor, entity) =>
			{
				//if (String.IsNullOrWhiteSpace(entity.Company))
				//	throw new PXException(BigCommerceMessages.NoRequiredField, "Company Name", "Customer");

				//if (String.IsNullOrWhiteSpace(entity.PostalCode))
				//	throw new PXException(ShopifyMessages.NoRequiredField, "Postal Code", "Location");
			});
		}
	}
}
