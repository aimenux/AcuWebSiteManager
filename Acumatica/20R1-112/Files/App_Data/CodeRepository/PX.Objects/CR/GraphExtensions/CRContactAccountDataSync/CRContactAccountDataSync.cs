using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.CR.Extensions.CRContactAccountDataSync
{
	/// <exclude/>
	public abstract class CRContactAccountDataSync<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		#region Views

		#region Document Mapping
		protected class DocumentMapping : IBqlMapping
		{
			public Type Extension => typeof(Document);
			protected Type _table;
			public Type Table => _table;

			public DocumentMapping(Type table)
			{
				_table = table;
			}
			public Type OverrideRefContact = typeof(Document.overrideRefContact);
			public Type RefContactID = typeof(Document.refContactID);
			public Type BAccountID = typeof(Document.bAccountID);
		}
		protected abstract DocumentMapping GetDocumentMapping();
		#endregion

		#region Document Contact Mapping
		protected class DocumentContactMapping : IBqlMapping
		{
			public Type Extension => typeof(DocumentContact);
			protected Type _table;
			public Type Table => _table;

			public DocumentContactMapping(Type table)
			{
				_table = table;
			}
			public Type FullName = typeof(DocumentContact.fullName);
			public Type Title = typeof(DocumentContact.title);
			public Type FirstName = typeof(DocumentContact.firstName);
			public Type LastName = typeof(DocumentContact.lastName);
			public Type Salutation = typeof(DocumentContact.salutation);
			public Type Attention = typeof(DocumentContact.attention);
			public Type Email = typeof(DocumentContact.email);
			public Type Phone1 = typeof(DocumentContact.phone1);
			public Type Phone1Type = typeof(DocumentContact.phone1Type);
			public Type Phone2 = typeof(DocumentContact.phone2);
			public Type Phone2Type = typeof(DocumentContact.phone2Type);
			public Type Phone3 = typeof(DocumentContact.phone3);
			public Type Phone3Type = typeof(DocumentContact.phone3Type);
			public Type Fax = typeof(DocumentContact.fax);
			public Type FaxType = typeof(DocumentContact.faxType);
			public Type OverrideContact = typeof(DocumentContact.overrideContact);

			public Type ConsentAgreement = typeof(DocumentContact.consentAgreement);
			public Type ConsentDate = typeof(DocumentContact.consentDate);
			public Type ConsentExpirationDate = typeof(DocumentContact.consentExpirationDate);

			public Type DefAddressID = typeof(DocumentContact.defAddressID);
		}
		protected abstract DocumentContactMapping GetDocumentContactMapping();
		#endregion

		#region Document Address Mapping
		protected class DocumentAddressMapping : IBqlMapping
		{
			public Type Extension => typeof(DocumentAddress);
			protected Type _table;
			public Type Table => _table;

			public DocumentAddressMapping(Type table)
			{
				_table = table;
			}
			public Type OverrideAddress = typeof(DocumentAddress.overrideAddress);
			public Type AddressLine1 = typeof(DocumentAddress.addressLine1);
			public Type AddressLine2 = typeof(DocumentAddress.addressLine2);
			public Type AddressLine3 = typeof(DocumentAddress.addressLine3);
			public Type City = typeof(DocumentAddress.city);
			public Type CountryID = typeof(DocumentAddress.countryID);
			public Type State = typeof(DocumentAddress.state);
			public Type PostalCode = typeof(DocumentAddress.postalCode);
			public Type IsValidated = typeof(DocumentAddress.isValidated);
		}
		protected abstract DocumentAddressMapping GetDocumentAddressMapping();
		#endregion

		public PXSelectExtension<Document> Documents;
		public PXSelectExtension<DocumentContact> Contacts;
		public PXSelectExtension<DocumentAddress> Addresses;

		public PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Current<Document.bAccountID>>>> ExistingAccount;

		#endregion

		#region Events

		protected virtual void _(Events.RowSelected<Document> e)
		{
			Document row = e.Row;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<Document.overrideRefContact>(e.Cache, row, row.RefContactID != null || row.BAccountID != null);

			if (row.RefContactID != null && row.BAccountID != null)
			{
				Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.SelectSingleBound(Base, null, row.RefContactID);

				if (contact != null && contact.BAccountID != row.BAccountID)
				{
					e.Cache.RaiseExceptionHandling<Document.refContactID>(row, row.RefContactID, new PXSetPropertyException(Messages.ContractBAccountDiffer, PXErrorLevel.Warning));
				}
				else
				{
					e.Cache.RaiseExceptionHandling<Document.refContactID>(row, null, null);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<Document, Document.overrideRefContact> e)
		{
			if (e.Row == null || e.Row.OverrideRefContact != false || e.ExternalCall == false)
				return;

			if (e.Row.RefContactID != null)
			{
				FillFromContact();
			}
			else if (e.Row.BAccountID != null)
			{
				FillFromAccount();
			}
		}

		protected virtual void _(Events.FieldUpdated<Document, Document.refContactID> e)
		{
			if (e.Row == null
				|| e.Row.RefContactID == e.OldValue as int?
				|| Base.IsImport
				|| Base.IsContractBasedAPI
				|| Base.UnattendedMode)
				return;

			if (e.Row.RefContactID == null)
			{
				e.Cache.RaiseFieldUpdated<Document.bAccountID>(e.Row, e.Row.BAccountID);

				return;
			}

			WebDialogResult dialogResult = Documents.View.Ask(null, Messages.Warning, Messages.ReplaceContactInfoFromContact,
				MessageButtons.AbortRetryIgnore,
				new Dictionary<WebDialogResult, string>()
				{
						{ WebDialogResult.Abort, "Yes" },
						{ WebDialogResult.Retry, "No" },
						{ WebDialogResult.Ignore, "Cancel" }
				}, MessageIcon.Warning);

			if (dialogResult == WebDialogResult.Abort)
			{
				FillFromContact();

				Documents.Cache.SetValue<Document.overrideRefContact>(Documents.Current, false);
			}
			else if (dialogResult == WebDialogResult.Retry)
			{
				Documents.Cache.SetValue<Document.overrideRefContact>(Documents.Current, true);
			}
			else if (dialogResult == WebDialogResult.Ignore)
			{
				Documents.Cache.SetValue<Document.refContactID>(Documents.Current, e.OldValue);
			}
		}

		protected virtual void _(Events.FieldUpdated<Document, Document.bAccountID> e)
		{
			if (e.Row == null
				//|| e.Row.BAccountID == e.OldValue as int?
				|| e.Row.RefContactID != null && Documents.View.Answer != WebDialogResult.Ignore
				|| Base.IsImport
				|| Base.IsContractBasedAPI
				|| Base.UnattendedMode)
				return;

			if (e.Row.BAccountID == null && e.Row.RefContactID == null)
			{
				e.Cache.SetValueExt<Document.overrideRefContact>(e.Row, false);

				return;
			}

			WebDialogResult dialogResult = Documents.View.Ask(null, Messages.Warning, Messages.ReplaceContactInfoFromAccount,
				MessageButtons.AbortRetryIgnore,
				new Dictionary<WebDialogResult, string>()
				{
					{ WebDialogResult.Abort, "Yes" },
					{ WebDialogResult.Retry, "No" },
					{ WebDialogResult.Ignore, "Cancel" }
				}, MessageIcon.Warning);

			if (dialogResult == WebDialogResult.Abort)
			{
				FillFromAccount();

				Documents.Cache.SetValue<Document.overrideRefContact>(Documents.Current, false);
			}
			else if (dialogResult == WebDialogResult.Retry)
			{
				Documents.Cache.SetValue<Document.overrideRefContact>(Documents.Current, true);
			}
			else if (dialogResult == WebDialogResult.Ignore)
			{
				Documents.Cache.SetValue<Document.bAccountID>(Documents.Current, e.OldValue);
			}
		}

		#endregion

		#region Methods

		private void FillFromContact()
		{
			Contact relatedContact = PXSelect<
					Contact,
				Where<
					Contact.contactID, Equal<Current<Document.refContactID>>>>
				.SelectSingleBound(Base, new[] { Documents.Current });

			Address relatedContactAddress = PXSelect<
					Address,
				Where<
					Address.addressID, Equal<Current<Contact.defAddressID>>>>
				.SelectSingleBound(Base, new[] { relatedContact });

			DocumentContact docContact = Contacts.Cache.Current as DocumentContact;
			DocumentAddress docAddress = Addresses.SelectSingle(docContact.DefAddressID);

			FillToDocumentContact(Contacts.Cache, docContact, relatedContact);
			FillToDocumentAddress(Addresses.Cache, docAddress, relatedContactAddress);
		}

		private void FillFromAccount()
		{
			Contact relatedContact = PXSelectJoin<
					Contact,
				InnerJoin<BAccount,
					On<BAccount.defContactID, Equal<Contact.contactID>>>,
				Where<
					BAccount.bAccountID, Equal<Current<Document.bAccountID>>>>
				.SelectSingleBound(Base, new[] { Documents.Current });

			Address relatedContactAddress = PXSelectJoin<
					Address,
				InnerJoin<BAccount,
					On<BAccount.defAddressID, Equal<Address.addressID>>>,
				Where<
					BAccount.bAccountID, Equal<Current<Document.bAccountID>>>>
				.SelectSingleBound(Base, new[] { relatedContact });

			DocumentContact docContact = Contacts.Cache.Current as DocumentContact;
			DocumentAddress docAddress = Addresses.SelectSingle(docContact.DefAddressID);

			FillToDocumentContact(Contacts.Cache, docContact, relatedContact);
			FillToDocumentAddress(Addresses.Cache, docAddress, relatedContactAddress);
		}

		private static void FillToDocumentContact(PXCache docContactCache, DocumentContact docContact, Contact contact)
		{
			if (contact == null || docContact == null)
				return;

			docContactCache.SetValue<DocumentContact.fullName>(docContact, contact.FullName);
			docContactCache.SetValue<DocumentContact.title>(docContact, contact.Title);
			docContactCache.SetValue<DocumentContact.firstName>(docContact, contact.FirstName);
			docContactCache.SetValue<DocumentContact.lastName>(docContact, contact.LastName);
			docContactCache.SetValue<DocumentContact.salutation>(docContact, contact.Salutation);
			docContactCache.SetValue<DocumentContact.attention>(docContact, contact.Attention);
			docContactCache.SetValue<DocumentContact.email>(docContact, contact.EMail);
			docContactCache.SetValue<DocumentContact.webSite>(docContact, contact.WebSite);
			docContactCache.SetValue<DocumentContact.phone1>(docContact, contact.Phone1);
			docContactCache.SetValue<DocumentContact.phone1Type>(docContact, contact.Phone1Type);
			docContactCache.SetValue<DocumentContact.phone2>(docContact, contact.Phone2);
			docContactCache.SetValue<DocumentContact.phone2Type>(docContact, contact.Phone2Type);
			docContactCache.SetValue<DocumentContact.phone3>(docContact, contact.Phone3);
			docContactCache.SetValue<DocumentContact.phone3Type>(docContact, contact.Phone3Type);
			docContactCache.SetValue<DocumentContact.fax>(docContact, contact.Fax);
			docContactCache.SetValue<DocumentContact.faxType>(docContact, contact.FaxType);

			docContactCache.SetValue<DocumentContact.consentAgreement>(docContact, contact.ConsentAgreement);
			docContactCache.SetValue<DocumentContact.consentDate>(docContact, contact.ConsentDate);
			docContactCache.SetValue<DocumentContact.consentExpirationDate>(docContact, contact.ConsentExpirationDate);
		}

		private static void FillToDocumentAddress(PXCache docAddressCache, DocumentAddress docAddress, Address address)
		{
			if (address == null || docAddress == null)
				return;

			docAddressCache.SetValue<DocumentAddress.addressLine1>(docAddress, address.AddressLine1);
			docAddressCache.SetValue<DocumentAddress.addressLine2>(docAddress, address.AddressLine2);
			docAddressCache.SetValue<DocumentAddress.city>(docAddress, address.City);
			docAddressCache.SetValue<DocumentAddress.countryID>(docAddress, address.CountryID);
			docAddressCache.SetValue<DocumentAddress.state>(docAddress, address.State);
			docAddressCache.SetValue<DocumentAddress.postalCode>(docAddress, address.PostalCode);

			docAddressCache.Update(docAddress);
		}

		#endregion
	}
}
