using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using System;
using System.Collections.Generic;

namespace PX.Objects.Extensions.ContactAddress
{
    public abstract class ContactAddressGraph<TGraph> : PXGraphExtension<TGraph>
        where TGraph : PXGraph
    {
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
			public Type ContactID = typeof(Document.contactID);
			public Type DocumentContactID = typeof(Document.documentContactID);
			public Type DocumentAddressID = typeof(Document.documentAddressID);
			public Type ShipContactID = typeof(Document.shipContactID);
			public Type ShipAddressID = typeof(Document.shipAddressID);
			public Type LocationID = typeof(Document.locationID);
			public Type BAccountID = typeof(Document.bAccountID);
			public Type AllowOverrideContactAddress = typeof(Document.allowOverrideContactAddress);
			public Type AllowOverrideShippingContactAddress = typeof(Document.allowOverrideShippingContactAddress);
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
			public Type EMail = typeof(DocumentContact.email);
			public Type Phone1 = typeof(DocumentContact.phone1);
			public Type Phone1Type = typeof(DocumentContact.phone1Type);
			public Type Phone2 = typeof(DocumentContact.phone2);
			public Type Phone2Type = typeof(DocumentContact.phone2Type);
			public Type Phone3 = typeof(DocumentContact.phone3);
			public Type Phone3Type = typeof(DocumentContact.phone3Type);
			public Type Fax = typeof(DocumentContact.fax);
			public Type FaxType = typeof(DocumentContact.faxType);
            public Type IsDefaultContact = typeof(DocumentContact.isDefaultContact);
            public Type OverrideContact = typeof(DocumentContact.overrideContact);
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
            public Type IsDefaultAddress = typeof(DocumentAddress.isDefaultAddress);
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

		protected virtual void _(Events.FieldUpdated<Document, Document.contactID> e)
		{
			var row = e.Row as Document;
			if (row == null) return;

			Contact oldContact = null;
			Address oldAddress = null;
			ChangedData forContactInfo = new ChangedData();
			ChangedData forShippingInfo = new ChangedData(false);

			if (e.OldValue != null)
			{
				oldContact = PXSelect<Contact,
				   Where<Contact.contactID, Equal<Required<Document.contactID>>>>.Select(Base, (int?)e.OldValue);
				oldAddress = PXSelectJoin<Address,
					LeftJoin<Contact, On<Contact.defAddressID, Equal<Address.addressID>>>,
					Where<Contact.contactID, Equal<Required<Document.contactID>>>>.Select(Base, (int?)e.OldValue);
			}
			else if (row.LocationID != null)
			{
				oldContact = PXSelectJoin<Contact,
					LeftJoin<Location, On<Location.locationID, Equal<Current<Document.locationID>>>>,
					Where<Contact.contactID, Equal<Location.defContactID>>>.Select(Base);
				oldAddress = PXSelectJoin<Address,
					LeftJoin<Contact, On<Contact.defAddressID, Equal<Address.addressID>>,
					LeftJoin<Location, On<Location.locationID, Equal<Current<Document.locationID>>>>>,
				   Where<Address.addressID, Equal<Location.defAddressID>>>.Select(Base);
			}

			forContactInfo.OldContact = oldContact;
			forContactInfo.OldAddress = oldAddress;

			DefaultRecords(row, forContactInfo, forShippingInfo);
		}
	
		protected virtual void _(Events.RowInserted<Document> e)
		{
			var row = e.Row as Document;
			if (row == null) return;

			bool oldContactDirty = GetContactCache().IsDirty;
			bool oldAddressDirty = GetAddressCache().IsDirty;

			Contact oldContact = null;
			Address oldAddress = null;

			if (row.ContactID != null)
			{
				oldContact = PXSelect<Contact,
				   Where<Contact.contactID, Equal<Current<Document.contactID>>>>.Select(Base);
				oldAddress = PXSelectJoin<Address,
					LeftJoin<Contact, On<Contact.defAddressID, Equal<Address.addressID>>>,
					Where<Contact.contactID, Equal<Current<Document.contactID>>>>.Select(Base);
			}
			else if (row.LocationID != null)
			{
				oldContact = PXSelectJoin<Contact,
					LeftJoin<Location, On<Location.locationID, Equal<Current<Document.locationID>>>>,
					Where<Contact.contactID, Equal<Location.defContactID>>>.Select(Base);
				oldAddress = PXSelectJoin<Address,
					LeftJoin<Contact, On<Contact.defAddressID, Equal<Address.addressID>>,
					LeftJoin<Location, On<Location.locationID, Equal<Current<Document.locationID>>>>>,
				   Where<Address.addressID, Equal<Location.defAddressID>>>.Select(Base);
			}

			ChangedData forContactInfo = new ChangedData();
			forContactInfo.OldContact = oldContact;
			forContactInfo.OldAddress = oldAddress;

			DefaultRecords(row, forContactInfo, new ChangedData(false));

			GetContactCache().IsDirty = oldContactDirty;
			GetAddressCache().IsDirty = oldAddressDirty;
		}

		protected virtual void _(Events.FieldUpdated<Document, Document.locationID> e)
		{
			var row = e.Row as Document;
			if (row == null) return;

			ChangedData forShippingInfo = new ChangedData();
			ChangedData forContactInfo = new ChangedData(false);

			int? oldLocationID = (int?)e.OldValue;

			if (oldLocationID != null)
			{
				forShippingInfo.OldContact = PXSelectJoin<Contact,
					LeftJoin<Location,
						On<Location.locationID, Equal<Required<Document.locationID>>>>,
					Where<Contact.contactID, Equal<Location.defContactID>>>.Select(Base, oldLocationID);
				forShippingInfo.OldAddress = PXSelectJoin<Address,
					LeftJoin<Contact,
						On<Contact.defAddressID, Equal<Address.addressID>>,
					LeftJoin<Location,
						On<Location.locationID, Equal<Required<Document.locationID>>>>>,
					Where<Address.addressID, Equal<Location.defAddressID>>>.Select(Base, oldLocationID);
			}

			Location oldLocation = PXSelect<Location, Where<Location.locationID,
				Equal<Required<Location.locationID>>>>.
				Select(Base, oldLocationID);
            if (row.BAccountID != oldLocation?.BAccountID)
                forContactInfo = forShippingInfo;

            if (row.LocationID != null)
                DefaultRecords(row, forContactInfo, forShippingInfo);
		}
		protected virtual void _(Events.FieldUpdated<Document, Document.bAccountID> e)
		{
			var row = e.Row as Document;
			if (row == null) return;

			ChangedData forContactInfo = new ChangedData();
			ChangedData forShippingInfo = new ChangedData();

			if (row.BAccountID != null)
			{
				int? oldBAccountID = (int?)e.OldValue;
				int? oldLocationID = null;

				var baccount = (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(Base, oldBAccountID);

				if (baccount != null)
				{
					Location oldLocation = PXSelect<Location, Where<Location.locationID,
						Equal<Required<Location.locationID>>>>.
						Select(Base, baccount.DefLocationID);
					if (oldLocation != null)
					{
						oldLocationID = oldLocation.LocationID;
					}
				}

				if (row.ContactID != null)
				{
					forContactInfo.OldContact = PXSelect<Contact,
						Where<Contact.contactID, Equal<Current<Document.contactID>>>>.Select(Base);

					forContactInfo.OldAddress = PXSelectJoin<Address,
						LeftJoin<Contact, On<Contact.defAddressID, Equal<Address.addressID>>>,
						Where<Contact.contactID, Equal<Current<Document.contactID>>>>.Select(Base);
				}
				if (oldLocationID != null)
				{
					forShippingInfo.OldContact = PXSelectJoin<Contact,
						LeftJoin<Location,
							On<Location.locationID, Equal<Required<Document.locationID>>>>,
						Where<Contact.contactID, Equal<Location.defContactID>>>.Select(Base, oldLocationID);

					forShippingInfo.OldAddress = PXSelectJoin<Address,
						LeftJoin<Contact,
							On<Contact.defAddressID, Equal<Address.addressID>>,
						LeftJoin<Location,
							On<Location.locationID, Equal<Required<Document.locationID>>>>>,
						Where<Address.addressID, Equal<Location.defAddressID>>>.Select(Base, oldLocationID);

					if (row.ContactID == null)
					{
						forContactInfo = forShippingInfo;
					}
				}
			}
			DefaultRecords(row, forContactInfo, forShippingInfo);
		}
		protected virtual void _(Events.FieldDefaulting<Document, Document.allowOverrideContactAddress> e)
		{
			var row = e.Row as Document;
			if (row == null) return;

			if (Contacts.Current != null)
				Contacts.Cache.SetValue<DocumentContact.overrideContact>(Contacts.Current, row.AllowOverrideContactAddress);

			if (Addresses.Current != null)
				Addresses.Cache.SetValue<DocumentAddress.overrideAddress>(Addresses.Current, row.AllowOverrideContactAddress);
		}

		protected virtual void _(Events.FieldUpdated<Document, Document.allowOverrideContactAddress> e)
		{
			var row = e.Row as Document;
			if (row == null) return;

            DocumentAddress address = Addresses.SelectSingle();
            DocumentContact contact = Contacts.SelectSingle();
		    

		    if (contact != null)
		    {
                Contacts.Cache.SetValue<DocumentContact.overrideContact>(contact, row.AllowOverrideContactAddress);
                PXCache cache = GetContactCache();
                if (cache != null)
                {
                    IPersonalContact crContact = GetCurrentContact();

                    if (crContact != null)
                    {
                        cache.SetValue<CRContact.isDefaultContact>(crContact, row.AllowOverrideContactAddress != true);
                    }
                }
            }
		    if (address != null)
		    {
                Addresses.Cache.SetValue<DocumentAddress.overrideAddress>(address, row.AllowOverrideContactAddress);
                PXCache cache = GetAddressCache();
		        if (cache != null)
		        {
                    IAddress crAddress = GetCurrentAddress();

                    if (crAddress != null)
                    {
                        cache.SetValue<CRAddress.isDefaultAddress>(crAddress, row.AllowOverrideContactAddress != true);
                    }
                }
            }

            Addresses.Cache.Update(address);
		    Contacts.Cache.Update(contact);
        }

		protected virtual void _(Events.FieldUpdated<Document, Document.allowOverrideShippingContactAddress> e)
		{
			var row = e.Row as Document;
			if (row == null) return;

			PXCache cacheShippingContact = GetShippingContactCache();
			if (cacheShippingContact != null)
			{
				PXCache cacheContact = GetContactCache();
				if (cacheContact.Current == null)
					cacheContact.Current = GetCurrentContact();

				if (cacheShippingContact.Current == null)
				{
					cacheShippingContact.Current = GetCurrentShippingContact();
				}
                IPersonalContact crShippingContact = cacheShippingContact.Current as IPersonalContact;
				if (crShippingContact != null)
				{
					cacheShippingContact.SetValueExt<CRShippingContact.overrideContact>(crShippingContact, row.AllowOverrideShippingContactAddress);
					if (cacheShippingContact.Current == null)
					{
						cacheShippingContact.Clear();
						cacheShippingContact.Current = GetCurrentShippingContact();
					}
				}
			}

			PXCache cacheShippingAddress = GetShippingAddressCache();
			if (cacheShippingAddress != null)
			{
				PXCache cacheAddress = GetAddressCache();
				if (cacheAddress.Current == null)
					cacheAddress.Current = GetCurrentAddress();

                IAddress crShippingAddress = GetCurrentShippingAddress();
				if (crShippingAddress != null)
				{
					cacheShippingAddress.SetValueExt<CRShippingAddress.overrideAddress>(crShippingAddress, row.AllowOverrideShippingContactAddress);
					if (cacheShippingAddress.Current == null)
					{
						cacheShippingAddress.Clear();
						cacheShippingAddress.Current = GetCurrentShippingAddress();
					}
				}
			}
		}

		protected virtual void _(Events.FieldSelecting<Document, Document.allowOverrideContactAddress> e)
		{
			var row = e.Row as Document;
			if (row == null) return;

			if (row.BAccountID == null && row.LocationID == null && row.ContactID == null)
				e.ReturnValue = false;
		}

		protected virtual void _(Events.RowUpdated<DocumentAddress> e)
		{
			DocumentAddress row = e.Row as DocumentAddress;
			if (row == null) return;
			Document doc = Documents.Cache.Current as Document;
			if (doc == null) return;

			if (doc.BAccountID == null && doc.ContactID == null)
			{
				row.IsValidated = false;
			}
		}

        protected virtual bool IsThereSomeContactAddressSourceValue(PXCache cache, Document row)
        {
            return (row.LocationID != null || row.ContactID != null);
        }

        protected virtual void DefaultRecords(Document row, ChangedData changedForContactInfo, ChangedData changedForShippingInfo)
		{
			PXCache cache = Documents.Cache;
			
			bool needAskFromContactAddress = AskForConfirmationForAddress(row, changedForContactInfo);
			bool needAskFromShippingContactAddress = AskForConfirmationForShippingAddress(row, changedForShippingInfo);
						
			if ((needAskFromContactAddress || needAskFromShippingContactAddress) && (row.LocationID != null || row.ContactID != null))
			{
				string message = needAskFromContactAddress && needAskFromShippingContactAddress ?
										CR.Messages.ReplaceContactDetailsAndShippingInfo :
										needAskFromContactAddress ? CR.Messages.ReplaceContactDetails :
										CR.Messages.ReplaceShippingInfo;
				WebDialogResult dialogResult = this.Documents.View.Ask((object)null, CR.Messages.Warning, message,
					MessageButtons.AbortRetryIgnore,
					new Dictionary<WebDialogResult, string>()
					{
						{ WebDialogResult.Abort, "Yes" },
						{ WebDialogResult.Retry, "No" },
						{ WebDialogResult.Ignore, "Cancel" }
					}, MessageIcon.Warning);
				
				if (dialogResult == WebDialogResult.Abort)
				{
					if (needAskFromContactAddress)
					{
						CRAddressAttribute.DefaultRecord<Document.documentAddressID>(cache, cache.GetMain(row));
						CRContactAttribute.DefaultRecord<Document.documentContactID>(cache, cache.GetMain(row));
						cache.SetValue<Document.allowOverrideContactAddress>(row, false);
					}
					if (needAskFromShippingContactAddress)
					{
						CRShippingContactAttribute.DefaultRecord<Document.shipContactID>(cache, cache.GetMain(row));
						CRShippingAddressAttribute.DefaultRecord<Document.shipAddressID>(cache, cache.GetMain(row));
						cache.SetValue<Document.allowOverrideShippingContactAddress>(row, false);
					}
				}
				else if (dialogResult == WebDialogResult.Ignore)
				{
					cache.SetValue<Document.bAccountID>(row, cache.GetValueOriginal<Document.bAccountID>(cache.GetMain(row)));
					cache.SetValue<Document.contactID>(row, cache.GetValueOriginal<Document.contactID>(cache.GetMain(row)));
				}
				else if (dialogResult == WebDialogResult.Retry)
				{
					if (row.AllowOverrideContactAddress == false)
					{
						CRAddressAttribute.DefaultRecord<Document.documentAddressID>(cache, cache.GetMain(row));
						CRContactAttribute.DefaultRecord<Document.documentContactID>(cache, cache.GetMain(row));
						cache.SetValue<Document.allowOverrideContactAddress>(row, false);
					}
					else if (row.AllowOverrideShippingContactAddress == false)
					{
						CRShippingContactAttribute.DefaultRecord<Document.shipContactID>(cache, cache.GetMain(row));
						CRShippingAddressAttribute.DefaultRecord<Document.shipAddressID>(cache, cache.GetMain(row));
						cache.SetValue<Document.allowOverrideShippingContactAddress>(row, false);
					}
				}
			}
			else
			{
                if (IsThereSomeContactAddressSourceValue(cache, row))
                {
					if (changedForContactInfo.CanBeReplace)
					{
						CRAddressAttribute.DefaultRecord<Document.documentAddressID>(cache, cache.GetMain(row));
						CRContactAttribute.DefaultRecord<Document.documentContactID>(cache, cache.GetMain(row));
						cache.SetValue<Document.allowOverrideContactAddress>(row, false);
					}

					if (changedForShippingInfo.CanBeReplace)
					{
						CRShippingContactAttribute.DefaultRecord<Document.shipContactID>(cache, cache.GetMain(row));
						CRShippingAddressAttribute.DefaultRecord<Document.shipAddressID>(cache, cache.GetMain(row));
						cache.SetValue<Document.allowOverrideShippingContactAddress>(row, false);
					}
				}
			}

			if (row.LocationID == null && row.ContactID == null && row.BAccountID == null)
			{
				if (row.AllowOverrideContactAddress == false)
				{
					CRAddressAttribute.DefaultRecord<Document.documentAddressID>(cache, cache.GetMain(row));
					CRContactAttribute.DefaultRecord<Document.documentContactID>(cache, cache.GetMain(row));
					cache.SetValue<Document.allowOverrideContactAddress>(row, false);
				}
				if (row.AllowOverrideShippingContactAddress == false)
				{
					CRShippingContactAttribute.DefaultRecord<Document.shipContactID>(cache, cache.GetMain(row));
					CRShippingAddressAttribute.DefaultRecord<Document.shipAddressID>(cache, cache.GetMain(row));
					cache.SetValue<Document.allowOverrideShippingContactAddress>(row, false);
					PXCache cacheContact = GetContactCache();
					var contact = GetCurrentContact();
					cacheContact.RaiseRowUpdated(contact, contact);
					PXCache cacheAddress = GetAddressCache();
					var address = GetCurrentAddress();
					cacheAddress.RaiseRowUpdated(address, address);
				}
			}

			if (IsDefaultContactAdress())
			{
				cache.SetValue<Document.allowOverrideContactAddress>(row, true);
			}
		}

		protected virtual bool AskForConfirmationForAddress(Document row, ChangedData data)
		{
			return row.AllowOverrideContactAddress == true &&
											 !IsDefaultContactAdress() &&
											 IsThereSomeContactAddressSourceValue(Documents.Cache, row) &&
											 !IsContactAddressNoChanged(data);
		}
		protected virtual bool AskForConfirmationForShippingAddress(Document row, ChangedData data)
		{
			return row.AllowOverrideShippingContactAddress == true &&
													 !IsDefaultShippingContactAdress() &&
													 !IsShippingContactAddressNoChanged(data);
		}

		protected abstract IPersonalContact GetCurrentContact();
        protected abstract IPersonalContact GetCurrentShippingContact();
		protected abstract IPersonalContact GetEtalonContact();
		protected abstract IPersonalContact GetEtalonShippingContact();
		protected abstract IAddress GetCurrentAddress();
        protected abstract IAddress GetCurrentShippingAddress();
		protected abstract IAddress GetEtalonAddress();
		protected abstract IAddress GetEtalonShippingAddress();
	    protected abstract PXCache GetContactCache();
	    protected abstract PXCache GetAddressCache();
		protected abstract PXCache GetShippingContactCache();
		protected abstract PXCache GetShippingAddressCache();

        protected bool IsDefaultContactAdress()
		{
			var currentContact = GetCurrentContact();
			var currentAddress = GetCurrentAddress();

			if (currentContact != null && currentAddress != null)
			{
				var etalonAddress = GetEtalonAddress();
				var etalonContact = GetEtalonContact();

				if (currentContact.FullName != etalonContact.FullName)
					return false;
				if (currentContact.Title != etalonContact.Title)
					return false;
				if (currentContact.FirstName != etalonContact.FirstName)
					return false;
				if (currentContact.LastName != etalonContact.LastName)
					return false;
				if (currentContact.Salutation != etalonContact.Salutation)
					return false;
				if (currentContact.Attention != etalonContact.Attention)
					return false;
				if (currentContact.Email != etalonContact.Email)
					return false;
				if (currentContact.Phone1 != etalonContact.Phone1)
					return false;
				if (currentContact.Phone1Type != etalonContact.Phone1Type)
					return false;
				if (currentContact.Phone2 != etalonContact.Phone2)
					return false;
				if (currentContact.Phone2Type != etalonContact.Phone2Type)
					return false;
				if (currentContact.Phone3 != etalonContact.Phone3)
					return false;
				if (currentContact.Phone3Type != etalonContact.Phone3Type)
					return false;
				if (currentContact.Fax != etalonContact.Fax)
					return false;
				if (currentContact.FaxType != etalonContact.FaxType)
					return false;

				if (currentAddress.AddressLine1 != etalonAddress.AddressLine1)
					return false;
				if (currentAddress.AddressLine2 != etalonAddress.AddressLine2)
					return false;
				if (currentAddress.City != etalonAddress.City)
					return false;
				if (currentAddress.State != etalonAddress.State)
					return false;
				if (currentAddress.CountryID != etalonAddress.CountryID)
					return false;
				if (currentAddress.PostalCode != etalonAddress.PostalCode)
					return false;
			}
			return true;
		}
		protected bool IsContactAddressNoChanged(ChangedData replaceData)
		{
			if (replaceData == null || !replaceData.CanBeReplace)
				return true;

			Contact etalonContact = replaceData.OldContact;
			Address etalonAddress = replaceData.OldAddress;

			if (etalonContact == null || etalonAddress == null)
			{
				return false;
			}

			var contact = GetCurrentContact();
			var address = GetCurrentAddress();

			if (contact != null && address != null)
			{
				if (contact.FullName != etalonContact.FullName)
					return false;
				if (contact.Title != etalonContact.Title)
					return false;
				if (contact.LastName != etalonContact.LastName)
					return false;
				if (contact.FirstName != etalonContact.FirstName)
					return false;
				if (contact.Salutation != etalonContact.Salutation)
					return false;
				if (contact.Attention != etalonContact.Attention)
					return false;
				if (contact.Email != etalonContact.EMail)
					return false;
				if (contact.Phone1 != etalonContact.Phone1)
					return false;
				if (contact.Phone1Type != etalonContact.Phone1Type)
					return false;
				if (contact.Phone2 != etalonContact.Phone2)
					return false;
				if (contact.Phone2Type != etalonContact.Phone2Type)
					return false;
				if (contact.Phone3 != etalonContact.Phone3)
					return false;
				if (contact.Phone3Type != etalonContact.Phone3Type)
					return false;
				if (contact.Fax != etalonContact.Fax)
					return false;
				if (contact.FaxType != etalonContact.FaxType)
					return false;

				if (address.AddressLine1 != etalonAddress.AddressLine1)
					return false;
				if (address.AddressLine2 != etalonAddress.AddressLine2)
					return false;
				if (address.City != etalonAddress.City)
					return false;
				if (address.State != etalonAddress.State)
					return false;
				if (address.CountryID != etalonAddress.CountryID)
					return false;
				if (address.PostalCode != etalonAddress.PostalCode)
					return false;
			}
			else
			{
				return false;
			}
			return true;
		}
		protected bool IsDefaultShippingContactAdress()
		{
			var currentShippingContact = GetCurrentShippingContact();
			var currentShippingAddress = GetCurrentShippingAddress();

			if (currentShippingContact != null && currentShippingAddress != null)
			{
				var etalonShippingAddress = GetEtalonShippingAddress();
				var etalonShippingContact = GetEtalonShippingContact();

				if (currentShippingContact.FullName != etalonShippingContact.FullName)
					return false;
				if (currentShippingContact.Attention != etalonShippingContact.Attention)
					return false;
				if (currentShippingContact.Email != etalonShippingContact.Email)
					return false;
				if (currentShippingContact.Phone1 != etalonShippingContact.Phone1)
					return false;
				if (currentShippingContact.Phone1Type != etalonShippingContact.Phone1Type)
					return false;
				if (currentShippingContact.Phone2 != etalonShippingContact.Phone2)
					return false;
				if (currentShippingContact.Phone2Type != etalonShippingContact.Phone2Type)
					return false;

				if (currentShippingAddress.AddressLine1 != etalonShippingAddress.AddressLine1)
					return false;
				if (currentShippingAddress.AddressLine2 != etalonShippingAddress.AddressLine2)
					return false;
				if (currentShippingAddress.City != etalonShippingAddress.City)
					return false;
				if (currentShippingAddress.State != etalonShippingAddress.State)
					return false;
				if (currentShippingAddress.CountryID != etalonShippingAddress.CountryID)
					return false;
				if (currentShippingAddress.PostalCode != etalonShippingAddress.PostalCode)
					return false;
			}
			return true;
		}

		protected bool IsShippingContactAddressNoChanged(ChangedData replaceData)
		{
			if (replaceData == null || !replaceData.CanBeReplace)
				return true;

			Contact etalonContact = replaceData.OldContact;
			Address etalonAddress = replaceData.OldAddress;

			if (etalonContact == null || etalonAddress == null)
			{
				return false;
			}

			var shippingContact = GetCurrentShippingContact();
			var shippingAddress = GetCurrentShippingAddress();

			if (shippingContact != null && shippingAddress != null)
			{
				if (shippingContact.FullName != etalonContact.FullName)
					return false;
				if (shippingContact.Attention != etalonContact.Attention)
					return false;
				if (shippingContact.Email != etalonContact.EMail)
					return false;
				if (shippingContact.Phone1 != etalonContact.Phone1)
					return false;
				if (shippingContact.Phone1Type != etalonContact.Phone1Type)
					return false;
				if (shippingContact.Phone2 != etalonContact.Phone2)
					return false;
				if (shippingContact.Phone2Type != etalonContact.Phone2Type)
					return false;

				if (shippingAddress.AddressLine1 != etalonAddress.AddressLine1)
					return false;
				if (shippingAddress.AddressLine2 != etalonAddress.AddressLine2)
					return false;
				if (shippingAddress.City != etalonAddress.City)
					return false;
				if (shippingAddress.State != etalonAddress.State)
					return false;
				if (shippingAddress.CountryID != etalonAddress.CountryID)
					return false;
				if (shippingAddress.PostalCode != etalonAddress.PostalCode)
					return false;
			}
			else
			{
				return false;
			}
			return true;
		}

		protected class ChangedData
		{
			public ChangedData(bool canBeReplace = true)
			{
				CanBeReplace = canBeReplace;
			}
			public bool CanBeReplace { get; set; }

			public Contact OldContact { get; set; }

			public Address OldAddress { get; set; }
		}
	}
}
