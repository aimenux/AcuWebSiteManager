using System;
using System.Collections.Generic;
using PX.CCProcessingBase;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public class CustomerDataReader : ICustomerDataReader
	{
		private PXGraph _graph;
		private int? _customerID;
		private string _customerCD;
		private int? _pminstanceID;
		private string _prefixForCustomerCD;

		public CustomerDataReader(CCProcessingContext context)
		{
			PXGraph graph = context.callerGraph;
			int? customerId = context.aCustomerID;
			string customerCD = context.aCustomerCD;
			int? pminstanceID = context.aPMInstanceID;

			if (graph == null)
			{
				throw new ArgumentNullException(nameof(graph));
			}
			if (customerId == null && String.IsNullOrEmpty(customerCD))
			{
				throw new ArgumentNullException(nameof(customerId), $"Either {nameof(customerId)} or {nameof(customerCD)} must be not null");
			}

			_graph = graph;
			_customerID = customerId;
			_customerCD = customerCD;
			_pminstanceID = pminstanceID;

			_prefixForCustomerCD = context.PrefixForCustomerCD; 
		}

		void ICustomerDataReader.ReadData(Dictionary<string, string> aData)
		{
			PXResult<Customer, CR.Address, CR.Contact> result = null;
			PXResult<CustomerPaymentMethod> pmResult = null;
			if (_customerID != 0)
			{
				result = (PXResult<Customer, CR.Address, CR.Contact>) PXSelectJoin<Customer,
					LeftJoin<CR.Address, On<CR.Address.addressID, Equal<Customer.defBillAddressID>>,
						LeftJoin<CR.Contact, On<CR.Contact.contactID, Equal<Customer.defBillContactID>>>>,
					Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(_graph, _customerID);
			}
			else if (!string.IsNullOrEmpty(_customerCD))
			{
				result = (PXResult<Customer, CR.Address, CR.Contact>) PXSelectJoin<Customer,
					LeftJoin<CR.Address, On<CR.Address.addressID, Equal<Customer.defBillAddressID>>,
						LeftJoin<CR.Contact, On<CR.Contact.contactID, Equal<Customer.defBillContactID>>>>,
					Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>.Select(_graph, _customerCD);
			}


			if (_pminstanceID != null)
			{
				pmResult = PXSelect<CustomerPaymentMethod,
							Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>
						.Select(_graph, _pminstanceID);
			}

			if (result != null)
			{
				Customer customer = result;
				Address billAddress = result;
				Contact billContact = result;
				if (pmResult != null)
				{
					CustomerPaymentMethod customerPaymentMethod = pmResult;
					Address address = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(_graph,
						customerPaymentMethod.BillAddressID);

					if (address != null)
					{
						billAddress = address;
					}

					Contact contact = PXSelect<Contact, Where<CR.Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(_graph,
						customerPaymentMethod.BillContactID);

					if (contact != null)
					{
						billContact = contact;
					}

					if (pmResult != null && !string.IsNullOrEmpty(((CustomerPaymentMethod)pmResult).CustomerCCPID))
					{
						aData[Key_Customer_CCProcessingID] = ((CustomerPaymentMethod)pmResult).CustomerCCPID;
					}
				}
				aData[key_CustomerCD] = (_prefixForCustomerCD ?? String.Empty) + (customer.AcctCD ?? String.Empty);
				aData[key_CustomerName] = customer.AcctName;

				aData[key_Country] = billAddress?.CountryID;
				aData[key_State] = billAddress?.State;
				aData[key_City] = billAddress?.City;
				aData[key_Address] = CCProcessingHelper.ExtractStreetAddress(billAddress);
				aData[key_PostalCode] = billAddress?.PostalCode;

				aData[key_Customer_FirstName] = billContact?.FirstName;
				aData[key_Customer_LastName] = billContact?.LastName;
				aData[key_Phone] = billContact?.Phone1;
				aData[key_Fax] = billContact?.Fax;
				aData[key_Email] = billContact?.EMail;
			}
		}

		string ICustomerDataReader.Key_CustomerName
		{
			get { return key_CustomerName; }
		}

		string ICustomerDataReader.Key_CustomerCD
		{
			get { return key_CustomerCD; }
		}

		string ICustomerDataReader.Key_Customer_FirstName
		{
			get { return key_Customer_FirstName; }
		}

		string ICustomerDataReader.Key_Customer_LastName
		{
			get { return key_Customer_LastName; }
		}

		string ICustomerDataReader.Key_BillAddr_Country
		{
			get { return key_Country; }
		}

		string ICustomerDataReader.Key_BillAddr_State
		{
			get { return key_State; }
		}

		string ICustomerDataReader.Key_BillAddr_City
		{
			get { return key_City; }
		}

		string ICustomerDataReader.Key_BillAddr_Address
		{
			get { return key_Address; }
		}

		string ICustomerDataReader.Key_BillAddr_PostalCode
		{
			get { return key_PostalCode; }
		}
		string ICustomerDataReader.Key_BillContact_Phone
		{
			get { return key_Phone; }
		}
		string ICustomerDataReader.Key_BillContact_Fax
		{
			get { return key_Fax; }
		}

		string ICustomerDataReader.Key_BillContact_Email
		{
			get { return key_Email; }
		}

		string ICustomerDataReader.Key_Customer_CCProcessingID
		{
			get { return Key_Customer_CCProcessingID; }
		}

		#region Private Constants
		private const string key_CustomerCD = "CustomerCD";
		private const string key_CustomerName = "CustomerName";
		private const string key_Customer_FirstName = "CustomerFirstName";
		private const string key_Customer_LastName = "CustomerLastName";
		private const string key_Country = "CountryID";
		private const string key_State = "State";
		private const string key_City = "City";
		private const string key_Address = "Address";
		private const string key_PostalCode = "PostalCode";
		private const string key_Phone = "Phone";
		private const string key_Fax = "Fax";
		private const string key_Email = "Email";
		private const string Key_Customer_CCProcessingID = "CCProcessingID";
		#endregion


	}
}
