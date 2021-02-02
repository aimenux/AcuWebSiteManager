using PX.Commerce.Core.Model;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class CustomerFormFieldRestDataProvider : RestDataProviderV3
    {
        private const string id_string = "id";

        protected override string GetListUrl { get; } = "v3/customers/form-field-values";

        protected override string GetSingleUrl { get; } = "v3/customers/form-field-values";

        protected override string GetCountUrl { get; } = string.Empty;

        public CustomerFormFieldRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        public CustomerFormFieldData Create(CustomerFormFieldData customersCustomFieldData)
        {
            var newData = base.Update<CustomerFormFieldData>(customersCustomFieldData, new UrlSegments());
            return newData;
        }

        public CustomerFormFieldData Update(CustomerFormFieldData customersCustomFieldData)
        {
            var updateData = base.Update<CustomerFormFieldData>(customersCustomFieldData, new UrlSegments());
            return updateData;
        }

		public List<CustomerFormFieldData> UpdateAll(List<CustomerFormFieldData> customersCustomFieldDataList)
		{
			CustomerFormFieldList response = base.Update<CustomerFormFieldData, CustomerFormFieldList>(customersCustomFieldDataList, new UrlSegments());
			return response?.Data;
		}

		public IEnumerable<CustomerFormFieldData> GetAll()
        {
			return GetAll<CustomerFormFieldData, CustomerFormFieldList>();
        }

        public List<CustomerFormFieldData> GetByCustomerID(string id)
        {
            IFilter filter = null;
            if(int.TryParse(id, out var numId))
            {
                filter = new FilterCustomersCustomFields() { CustomerId = numId };
            }
            return GetAll<CustomerFormFieldData, CustomerFormFieldList>(filter)?.ToList();
        }

        public List<CustomerFormFieldData> GetByCustomerAddressID(string id)
        {
            IFilter filter = null;
            if (int.TryParse(id, out var numId))
            {
                filter = new FilterCustomersCustomFields() { AddressId = numId };
            }
            return GetAll<CustomerFormFieldData, CustomerFormFieldList>(filter)?.ToList();
        }
    }
}
