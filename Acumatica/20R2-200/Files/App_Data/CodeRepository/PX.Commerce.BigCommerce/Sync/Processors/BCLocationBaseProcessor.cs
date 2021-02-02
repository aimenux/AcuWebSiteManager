using PX.Api.ContractBased.Models;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.BigCommerce
{
	public abstract class BCLocationBaseProcessor<TGraph, TEntityBucket, TPrimaryMapped> : BCProcessorSingleBase<TGraph, TEntityBucket, TPrimaryMapped>
		where TGraph : PXGraph
		where TEntityBucket : class, IEntityBucket, new()
		where TPrimaryMapped : class, IMappedEntity, new()
	{
		protected BCLocationProcessor locationProcessor;

		protected CustomerRestDataProviderV3 customerDataProviderV3;
		protected CustomerAddressRestDataProviderV3 customerAddressDataProviderV3;
		protected CustomerFormFieldRestDataProvider customerFormFieldRestDataProvider;
		protected List<Tuple<String, String, String>> formFieldsList;
		protected List<States> States;
		protected StoreStatesProvider statesProvider;
		protected BCRestClient client;

		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			customerDataProviderV3 = new CustomerRestDataProviderV3(client);
			customerAddressDataProviderV3 = new CustomerAddressRestDataProviderV3(client);
			customerFormFieldRestDataProvider = new CustomerFormFieldRestDataProvider(client);
			statesProvider = new StoreStatesProvider(client);
			formFieldsList = ConnectorHelper.GetConnectorSchema(operation.ConnectorType, operation.Binding, operation.EntityType)?.FormFields ?? new List<Tuple<string, string, string>>();
			if(States ==null)
				States = statesProvider.Get();

		}

		protected virtual CustomerLocation MapLocationImport(CustomerAddressData addressObj, MappedCustomer customerObj)
		{
			CustomerLocation locationImpl = new CustomerLocation();
			locationImpl.Custom = locationProcessor == null ? GetCustomFieldsForImport() : locationProcessor.GetCustomFieldsForImport();

			//Location
			string firstLastName = CustomerNameResolver(addressObj.FirstName, addressObj.LastName, (int)customerObj.Extern.Id);
			locationImpl.LocationName = (String.IsNullOrEmpty(addressObj.Company) ? firstLastName : addressObj.Company).ValueField();
			locationImpl.ContactOverride = true.ValueField();
			locationImpl.AddressOverride = true.ValueField();
			//Contact
			Contact contactImpl = locationImpl.LocationContact = new Contact();
			contactImpl.FirstName = addressObj.FirstName.ValueField();
			contactImpl.LastName = addressObj.LastName.ValueField();
			contactImpl.Attention = firstLastName.ValueField();
			contactImpl.Phone1 = new StringValue { Value = addressObj.Phone };
			contactImpl.FullName = locationImpl.LocationName;
			contactImpl.AddressIsSameAsInAccount = new BooleanValue { Value = false };

			//Address
			Address addressImpl = contactImpl.Address = new Address();
			addressImpl.AddressLine1 = addressObj.Address1.ValueField();
			addressImpl.AddressLine2 = addressObj.Address2.ValueField();
			addressImpl.City = addressObj.City.ValueField();
			addressImpl.Country = addressObj.CountryCode.ValueField();
			if (!string.IsNullOrEmpty(addressObj.State))
			{
				addressImpl.State = States?.FirstOrDefault(x => x.State == addressObj.State)?.StateID?.ValueField() ?? addressObj.State.ValueField();
			}
			else
				addressImpl.State = string.Empty.ValueField();
			addressImpl.PostalCode = addressObj.PostalCode?.ToUpperInvariant()?.ValueField();
			return locationImpl;
		}
	
		protected virtual CustomerAddressData MapLocationExport(MappedLocation addressObj, string customerName)
		{
			CustomerLocation locationImpl = addressObj.Local;
			CustomerAddressData addressData = new CustomerAddressData();

			
			//Contact
			Contact contactImpl = locationImpl.LocationContact;
			addressData.Company = contactImpl.FullName?.Value ?? customerName ;
			string fullName = new String[] {
				contactImpl.Attention?.Value,
				locationImpl.LocationName?.Value,
			 customerName,
			}.FirstNotEmpty();
			addressData.FirstName = fullName.FieldsSplit(0, fullName);
			addressData.LastName = fullName.FieldsSplit(1, fullName);
			addressData.Phone = contactImpl.Phone1?.Value ?? contactImpl.Phone2?.Value;

			//Address
			Address addressImpl = contactImpl.Address;
			addressData.Address1 = addressImpl.AddressLine1?.Value;
			addressData.Address2 = addressImpl.AddressLine2?.Value;
			addressData.City = addressImpl.City?.Value;
			addressData.CountryCode = addressImpl.Country?.Value;
			if (!string.IsNullOrEmpty(addressImpl.State?.Value))
			{
				addressData.State = States?.FirstOrDefault(x => x.StateID == addressImpl.State?.Value || x.State.ToUpper() == addressImpl.State?.Value?.ToUpper())?.State;
				
			}
			else
				addressData.State = string.Empty;
			addressData.PostalCode = addressImpl.PostalCode?.Value;
			return addressData;
		}

		public virtual string CustomerNameResolver(string firstName, string lastName, int id)
		{
			if (String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName))
				throw new PXException(BCMessages.CustomerNameIsEmpty, id);
			if (firstName.Equals(lastName))
				return firstName;
			return String.Concat(firstName, " ", lastName);
		}


	}
}
