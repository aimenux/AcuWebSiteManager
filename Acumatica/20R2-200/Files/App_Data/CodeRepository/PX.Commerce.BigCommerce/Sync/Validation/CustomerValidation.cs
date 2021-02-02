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
using PX.Api;

namespace PX.Commerce.BigCommerce
{
	public class CustomerValidator : BCBaseValidator, ISettingsValidator, IExternValidator
	{
		public int Priority { get { return 0; } }

		public virtual void Validate(IProcessor iproc)
		{
			Validate<BCCustomerProcessor>(iproc, (processor) =>
			{
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();
				if (storeExt.CustomerNumberingID == null && BCDimensionMaskAttribute.GetAutoNumbering(CustomerRawAttribute.DimensionName) == null)
					throw new PXException(BigCommerceMessages.NoCustomerNumbering);

				if(storeExt.CustomerClassID == null)
				{
					ARSetup arSetup = PXSelect<ARSetup>.Select(processor);
					if (arSetup.DfltCustomerClassID == null)
						throw new PXException(BigCommerceMessages.NoCustomerClass);
				}

			});
			Validate<BCLocationProcessor>(iproc, (processor) =>
			{
				BCBindingExt storeExt = processor.GetBindingExt<BCBindingExt>();
				if (storeExt.CustomerNumberingID == null && BCDimensionMaskAttribute.GetAutoNumbering(CustomerRawAttribute.DimensionName) == null)
					throw new PXException(BigCommerceMessages.NoCustomerNumbering);
				if (storeExt.LocationNumberingID == null && BCDimensionMaskAttribute.GetAutoNumbering(LocationIDAttribute.DimensionName) == null)
					throw new PXException(BigCommerceMessages.NoLocationNumbering);

			});
		}

		public virtual void Validate(IProcessor iproc, IExternEntity ientity)
		{
			Validate<BCCustomerProcessor, CustomerData>(iproc, ientity, (processor, entity) =>
			{
				if(String.IsNullOrWhiteSpace(entity.Email))
					throw new PXException(BigCommerceMessages.NoRequiredField, "Email", "Customer");

				if (String.IsNullOrWhiteSpace(entity.FirstName) || String.IsNullOrWhiteSpace(entity.LastName))
					throw new PXException(BigCommerceMessages.NoRequiredField, "Company Name", "Customer");
			});
			Validate<BCCustomerProcessor, CustomerAddressData>(iproc, ientity, (processor, entity) =>
			{
				//if (String.IsNullOrWhiteSpace(entity.Company))
				//	throw new PXException(BigCommerceMessages.NoRequiredField, "Company Name", "Customer");

				if (String.IsNullOrWhiteSpace(entity.PostalCode))
					throw new PXException(BigCommerceMessages.NoRequiredField, "Postal Code", "Customer");
			});
			Validate<BCLocationProcessor, CustomerAddressData>(iproc, ientity, (processor, entity) =>
			{
				//if (String.IsNullOrWhiteSpace(entity.Company))
				//	throw new PXException(BigCommerceMessages.NoRequiredField, "Company Name", "Customer");

				if (String.IsNullOrWhiteSpace(entity.PostalCode))
					throw new PXException(BigCommerceMessages.NoRequiredField, "Postal Code", "Location");
			});
		}
	}
}
