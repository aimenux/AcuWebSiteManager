using PX.Api.ContractBased.Models;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Shopify.API.REST;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.Shopify
{
	public abstract class SPLocationBaseProcessor<TGraph, TEntityBucket, TPrimaryMapped> : BCProcessorSingleBase<TGraph, TEntityBucket, TPrimaryMapped>
		where TGraph : PXGraph
		where TEntityBucket : class, IEntityBucket, new()
		where TPrimaryMapped : class, IMappedEntity, new()
	{
		protected SPLocationProcessor locationProcessor;

		protected CustomerRestDataProvider customerDataProvider;
		protected IChildRestDataProvider<CustomerAddressData> customerAddressDataProvider;		
		protected List<Tuple<String, String, String>> formFieldsList;
		protected BCBinding currentBinding;
		protected SPRestClient client;
		public PXSelect<State, Where<State.name, Equal<Required<State.name>>,
			Or<State.stateID, Equal<Required<State.stateID>>>>> states;

		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			currentBinding = GetBinding();
			client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());
			customerAddressDataProvider = new CustomerAddressRestDataProvider(client);
			customerDataProvider = new CustomerRestDataProvider(client);
		}

		protected virtual CustomerLocation MapLocationImport(CustomerAddressData addressObj, MappedCustomer customerObj)
		{
			CustomerLocation locationImpl = new CustomerLocation();
			locationImpl.Custom = locationProcessor == null ? GetCustomFieldsForImport() : locationProcessor.GetCustomFieldsForImport();
			//locationImpl.Customer = customer.AcctCD?.Trim().ValueField();
			locationImpl.LocationName = (string.IsNullOrWhiteSpace(addressObj.Company) ? addressObj.Name : addressObj.Company).ValueField();
			locationImpl.ContactOverride = true.ValueField();
			locationImpl.AddressOverride = true.ValueField();

			//Contact
			Contact contactImpl = locationImpl.LocationContact = new Contact();
			contactImpl.CompanyName = addressObj.Company.ValueField();
			contactImpl.FirstName = addressObj.FirstName.ValueField();
			contactImpl.LastName = addressObj.LastName.ValueField();
			contactImpl.Attention = addressObj.Name.ValueField();
			contactImpl.Phone1 = addressObj.Phone.ValueField();

            //FullName equals to CompanyName field.
			contactImpl.FullName = addressObj.Company.ValueField();
			contactImpl.AddressIsSameAsInAccount = new BooleanValue { Value = false };

			//Address
			Address addressImpl = contactImpl.Address = new Address();
			addressImpl.AddressLine1 = addressObj.Address1.ValueField();
			addressImpl.AddressLine2 = addressObj.Address2.ValueField();
			addressImpl.City = addressObj.City.ValueField();
			addressImpl.Country = addressObj.CountryCode.ValueField();
			if (!string.IsNullOrEmpty(addressObj.ProvinceCode))
			{
					addressImpl.State = addressObj.ProvinceCode.ValueField();
			}
			else
				addressImpl.State = string.Empty.ValueField();
			addressImpl.PostalCode = addressObj.PostalCode?.ToUpperInvariant()?.ValueField();
			return locationImpl;
		}

		protected virtual CustomerAddressData MapLocationExport(MappedLocation addressObj, MappedCustomer customerObj)
		{
			CustomerLocation locationImpl = addressObj.Local;
			CustomerAddressData addressData = new CustomerAddressData();
			
			var result = PXSelectJoin<PX.Objects.CR.Location,
		InnerJoin<PX.Objects.CR.BAccount, On<PX.Objects.CR.Location.locationID, Equal<PX.Objects.CR.BAccount.defLocationID>>>,
		Where<PX.Objects.CR.BAccount.noteID, Equal<Required<PX.Objects.CR.BAccount.noteID>>>>.Select(this, customerObj.LocalID);


			//Contact
			Contact contactImpl = locationImpl.LocationContact;
			addressData.Company = contactImpl.FullName?.Value;
			string fullName = new String[] { contactImpl.Attention?.Value, locationImpl.LocationName?.Value }.FirstNotEmpty();
            addressData.Name = fullName;
			//addressData.FirstName = fullName.FieldsSplit(0, fullName);
			//addressData.LastName = fullName.FieldsSplit(1, fullName);
			addressData.Phone = contactImpl.Phone1?.Value ?? contactImpl.Phone2?.Value;

			//Address
			Address addressImpl = contactImpl.Address;
			addressData.Address1 = addressImpl.AddressLine1?.Value;
			addressData.Address2 = addressImpl.AddressLine2?.Value;
			addressData.City = addressImpl.City?.Value;
			addressData.CountryCode =addressImpl.Country?.Value;
			addressData.Province = addressImpl.State?.Value;
			addressData.PostalCode = addressImpl.PostalCode?.Value;
			if ((result.FirstOrDefault().GetItem<PX.Objects.CR.Location>()).NoteID == locationImpl.NoteID.Value)
				addressData.Default = true;
			else
				addressData.Default = false;
			return addressData;
		}
	}
}
