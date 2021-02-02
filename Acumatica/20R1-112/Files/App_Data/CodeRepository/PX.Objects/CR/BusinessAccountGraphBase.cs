using System;
using System.Collections;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	[Serializable]
	public class BusinessAccountGraphBase<Base, Primary, WhereClause> : PXGraph<BusinessAccountGraphBase<Base, Primary, WhereClause>>
		where Base : BAccount, new()
		where Primary : class, IBqlTable, new()
		where WhereClause : class, IBqlWhere, new()
	{

		#region Internal types
		//This type is used internally to prevent extension of BAccont to derived classes in SQL selects
		[Serializable]
		public class BAccountItself : BAccount { }
		#endregion

		#region Ctor + Public Selects
		public BusinessAccountGraphBase()
		{
			PXUIFieldAttribute.SetDisplayName<Contact.salutation>(DefContact.Cache, CR.Messages.Attention);
			Locations.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.accountLocations>();
		}

		[PXViewName(Messages.BAccount)]
		public PXSelect<Base, Where2<Match<Current<AccessInfo.userName>>, And<WhereClause>>> BAccount;
		public virtual PXSelectBase<Base> BAccountAccessor => BAccount;

		protected virtual void InitDefAddress(Address aAddress) { }

		[PXHidden]
		public PXSelect<Location> BaseLocations;
		public PXSelect<Address, Where<Address.bAccountID, Equal<Current<BAccount.bAccountID>>>> Addresses;
		[PXViewName(Messages.Address)]
		public PXSelect<Address, Where<Address.bAccountID, Equal<Current<BAccount.bAccountID>>, And<Address.addressID, Equal<Current<BAccount.defAddressID>>>>> DefAddress;
		public PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<BAccount.bAccountID>>>> Contacts;

		public PXSelect<LocationExtAddress, Where<LocationExtAddress.locationBAccountID, Equal<Current<BAccount.bAccountID>>>> Locations;
		[PXViewName(Messages.Location)]
		public PXSelect<LocationExtAddress, Where<LocationExtAddress.locationBAccountID, Equal<Current<BAccount.bAccountID>>,
			And<LocationExtAddress.locationID, Equal<Current<BAccount.defLocationID>>>>> DefLocation;
		public PXSelect<Location, Where<Location.bAccountID, Equal<Current<BAccount.bAccountID>>>, OrderBy<Asc<Location.locationID>>> IntLocations;

		public PXSelect<
				ContactExtAddress, 
			Where<
				ContactExtAddress.contactBAccountID, Equal<Current<BAccount.bAccountID>>,
				And<ContactExtAddress.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>,
				And<ContactExtAddress.contactType, NotEqual<ContactTypesAttribute.lead>>>>> ExtContacts;

		public PXSelect<Contact, Where<Contact.bAccountID, Equal<Current<BAccount.bAccountID>>, And<Contact.contactID, Equal<Current<BAccount.defContactID>>>>> DefContact;
		public PXSelectJoin<Contact, InnerJoin<Location, On<Contact.bAccountID, Equal<Location.bAccountID>,
									And<Contact.contactID, Equal<Location.defContactID>>>>, Where<Location.bAccountID, Equal<Current<BAccount.bAccountID>>,
									And<Location.locationID, Equal<Current<BAccount.defLocationID>>>>> DefLocationContact;

		public PXSelect<BAccountItself, Where<BAccount.bAccountID, Equal<Optional<BAccount.bAccountID>>>> CurrentBAccountItself;

		public PXSetup<GL.Company> cmpany;

		#endregion

		#region Buttons

		#region Standard Buttons
		public PXSave<Primary> Save;
		public PXAction<Primary> cancel;
		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable Cancel(PXAdapter a)
		{
			foreach (Primary e in (new PXCancel<Primary>(this, "Cancel")).Press(a))
			{
				BAccount acct = e as BAccount;
				if (acct != null)
				{
					if (BAccountAccessor.Cache.GetStatus(e) == PXEntryStatus.Inserted)
					{
						BAccount e1 = PXSelectReadonly<BAccountItself, Where<BAccountItself.acctCD, Equal<Required<BAccountItself.acctCD>>,
										And<BAccountItself.bAccountID, NotEqual<Required<BAccountItself.bAccountID>>>>>.Select(this, acct.AcctCD, acct.BAccountID);
						if (e1 != null && (e1.BAccountID != acct.BAccountID))
						{
							BAccountAccessor.Cache.RaiseExceptionHandling<BAccount.acctCD>(e, null, new PXSetPropertyException(EP.Messages.BAccountExists));
						}
					}
				}
				yield return e;
			}
		}
		public PXInsert<Primary> Insert;
		public PXCopyPasteAction<Primary> Edit;
		public PXDelete<Primary> Delete;
		public PXFirst<Primary> First;
		public PXPrevious<Primary> Prev;
		public PXNext<Primary> Next;
		public PXLast<Primary> Last;

		#endregion

		public PXAction<Base> viewContact;
		[PXUIField(DisplayName = Messages.ViewContact,
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewContact(PXAdapter adapter)
		{
			if (this.ExtContacts.Current != null && this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				ContactExtAddress current = this.ExtContacts.Current;
				ContactMaint graph = PXGraph.CreateInstance<ContactMaint>();
				graph.Contact.Current = graph.Contact.Search<Contact.contactID>(current.ContactID, this.BAccountAccessor.Current.AcctCD);
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXDBAction<Base> newContact;
		[PXUIField(DisplayName = Messages.AddContact, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewContact(PXAdapter adapter)
		{
			if (this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				ContactMaint graph = PXGraph.CreateInstance<ContactMaint>();
				Contact cont = new Contact();
				cont.BAccountID = this.BAccountAccessor.Current.BAccountID;
				cont = graph.Contact.Insert(cont);

				CRContactClass ocls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Current<Contact.classID>>>>
					.SelectSingleBound(this, new object[] { cont });
				if (ocls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
				{
					cont.WorkgroupID = this.BAccountAccessor.Current.WorkgroupID;
					cont.OwnerID = this.BAccountAccessor.Current.OwnerID;
				}

				cont = graph.Contact.Update(cont);

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<Base> viewLocation;
		[PXUIField(DisplayName = Messages.ViewLocation, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewLocation(PXAdapter adapter)
		{
			if (this.Locations.Current != null && this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				LocationExtAddress current = this.Locations.Current;
				LocationMaint graph = CreateLocationGraph();
				graph.Location.Current = graph.Location.Search<Location.locationID>(current.LocationID, this.BAccountAccessor.Current.AcctCD);
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		protected virtual LocationMaint CreateLocationGraph()
		{
			return CreateInstance<CustomerLocationMaint>();
		}

		protected virtual SelectedLocation CreateSelectedLocation()
		{
			return new SelectedLocation();
		}

		public PXDBAction<Base> newLocation;
		[PXUIField(DisplayName = Messages.AddNewLocation, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry, OnClosingPopup = PXSpecialButtonType.Cancel)]
		public virtual IEnumerable NewLocation(PXAdapter adapter)
		{
			if (BAccountAccessor.Cache.GetStatus(BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				LocationMaint graph = CreateLocationGraph();
				SelectedLocation loc = CreateSelectedLocation();
				loc.BAccountID = BAccountAccessor.Current.BAccountID;
				graph.Location.Insert(loc);
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}


		public PXAction<Base> setDefault;
		[PXUIField(DisplayName = Messages.SetDefault, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable SetDefault(PXAdapter adapter)
		{
			Base acct = BAccountAccessor.Current;
			LocationExtAddress loc = Locations.Current;
			if (loc != null && acct != null)
			{
				if (loc.IsActive != true)
				{
					Locations.Cache.RaiseExceptionHandling<LocationExtAddress.isActive>(loc, null, new PXSetPropertyException(Messages.DefaultLocationCanNotBeNotActive, typeof(LocationExtAddress.isActive).Name));
				}
				else
				{
					SetAsDefault(loc);
				}
			}
			return adapter.Get();
		}

		public PXAction<Base> viewMainOnMap;
		public PXAction<Base> viewDefLocationOnMap;

		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewMainOnMap(PXAdapter adapter)
		{
			Address addr = (Address)this.DefAddress.Select();
			BAccountUtility.ViewOnMap(addr);
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable ViewDefLocationOnMap(PXAdapter adapter)
		{
			LocationExtAddress addr = (LocationExtAddress)this.DefLocation.Select();
			BAccountUtility.ViewOnMap(addr);
			return adapter.Get();
		}


		#region Buttons

		public PXAction<Base> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (Base bacct in adapter.Get<Base>())
			{
				bool needSave = false;
				Save.Press();

				if (bacct != null)
				{
					Address address = this.DefAddress.Current;
					if (address != null && address.IsValidated == false)
					{
						if (PXAddressValidator.Validate<Address>(this, address, true))
						{
							needSave = true;
						}
					}

					LocationExtAddress locAddress = this.DefLocation.Current;
					if (locAddress != null && locAddress.IsValidated == false && locAddress.DefAddressID != address.AddressID)
					{
						if (PXAddressValidator.Validate<LocationExtAddress>(this, locAddress, true))
						{
							needSave = true;
						}
					}

					if (needSave)
					{
						this.Save.Press();
					}

					yield return bacct;
				}
			}
		}
		#endregion

		#endregion

		#region Select Delegates

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected virtual IEnumerable defLocationContact()
		{
			Contact cnt = null;
			if (this.DefLocation.Current == null)
			{
				this.DefLocation.Current = this.DefLocation.Select();
			}
			LocationExtAddress defLocation = this.DefLocation.Current;

			if (defLocation != null && defLocation.DefContactID != null)
			{
				cnt = FindContact(defLocation.DefContactID);
				if (cnt != null)
				{
					if ((defLocation.IsContactSameAsMain ?? false) == true)
					{
						cnt = PXCache<Contact>.CreateCopy(cnt);
						PXUIFieldAttribute.SetEnabled(this.DefLocationContact.Cache, cnt, false);
					}
					else
					{
						PXUIFieldAttribute.SetEnabled(this.DefLocationContact.Cache, cnt, true);
					}
					return new Contact[] { cnt };
				}
			}
			return new object[0];
		}

		#endregion

		#region BAccount Event Handlers

		protected virtual void OnBAccountRowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			BAccount row = (BAccount)e.Row;
			if (row != null)
			{
				//Inserting Default Address record
				if (row.DefAddressID == null)
				{
					Address addr = new Address();
					addr = this.Addresses.Insert(addr);
					this.InitDefAddress(addr);
					row.DefAddressID = addr.AddressID;
					this.Addresses.Cache.IsDirty = false;
				}

				// Inserting Default Contact record		   
				if (row.DefContactID == null)
				{
					Contact contact = new Contact();
					contact.ContactType = ContactTypesAttribute.BAccountProperty;
					contact = this.Contacts.Insert(contact);
					row.DefContactID = contact.ContactID;
					this.Contacts.Cache.IsDirty = false;
				}
				// Inserting default locaiton record
				if (row.DefLocationID == null)
				{
					LocationExtAddress location = new LocationExtAddress();
					switch (row.Type)
					{
						case BAccountType.VendorType:
							location.LocType = LocTypeList.VendorLoc;
							break;
						case BAccountType.CustomerType:
							location.LocType = LocTypeList.CustomerLoc;
							break;
						case BAccountType.CombinedType:
							location.LocType = LocTypeList.CombinedLoc;
							break;
						case BAccountType.EmpCombinedType:
							location.LocType = LocTypeList.CustomerLoc;
							break;
						default:
							location.LocType = LocTypeList.CompanyLoc;
							break;
					}
					// Location CD need to be formatted accorfing to segmented key mask prior inserting
					object cd = PXMessages.LocalizeNoPrefix(Messages.DefaultLocationCD);
					this.Locations.Cache.RaiseFieldUpdating<LocationExtAddress.locationCD>(location, ref cd);
					location.LocationCD = (string)cd;
					location.Descr = PXMessages.LocalizeNoPrefix(Messages.DefaultLocationDescription);
					location.IsDefault = true;
					location.IsAddressSameAsMain = true;
					location.IsContactSameAsMain = true;
					location = (LocationExtAddress)this.Locations.Cache.Insert(location);
					row.DefLocationID = location.LocationID;
					location.IsDefault = true;
					this.Locations.Cache.IsDirty = false;
					// Updating BAccount and clearing inserted status
					this.BAccountAccessor.Cache.Update(row);
					this.BAccountAccessor.Cache.IsDirty = false;

				}
			}
		}

		public override void Persist()
		{
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null && acct.DefAddressID != null)
			{
				Address addr = this.FindAddress(acct.DefAddressID);
				if (addr != null)
				{
					if (this.Addresses.Cache.GetStatus(addr) != PXEntryStatus.Notchanged
						|| Locations.Cache.IsDirty || ExtContacts.Cache.IsDirty)
					{
						foreach (LocationExtAddress loc in Locations.Select())
						{
							if (loc.IsAddressSameAsMain == true || loc.AddressID == acct.DefAddressID)
							{
								PXCache<Address>.RestoreCopy(loc, addr);
								loc.AddressID = null;
								loc.NoteID = null;
							}
						}
						foreach (ContactExtAddress cont in ExtContacts.Select())
						{
							if (cont.IsAddressSameAsMain == true || cont.AddressID == acct.DefAddressID)
							{
								PXCache<Address>.RestoreCopy(cont, addr);
								cont.AddressID = null;
								cont.NoteID = null;
							}
						}
					}
				}
			}

			base.Persist();
		}

		protected virtual void OnBAccountAcctNameFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			BAccount row = (BAccount)e.Row;

			if (this.DefContact.Current != null)
			{
				this.DefContact.Current.FullName = row.AcctName;
				this.DefContact.Cache.Update(this.DefContact.Current);
			}
		}

		#endregion

		#region Ext Location Events handlers

		protected virtual void LocationExtAddress_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.Row;
			if (row != null)
			{
				BAccount account = this.BAccountAccessor.Current;
				bool isSameAsMain = false;
				bool isContactSameAsMain = false;
				bool isDefault = false;
				if (account != null)
				{
					isSameAsMain = account.DefAddressID.HasValue && (row.DefAddressID == account.DefAddressID);
					if (!row.DefContactID.HasValue)
					{
						row.DefContactID = account.DefContactID;
					}
					isContactSameAsMain = account.DefContactID.HasValue && (row.DefContactID == account.DefContactID);
					isDefault = (row.LocationID == account.DefLocationID);
				}

				PXUIFieldAttribute.SetEnabled<LocationExtAddress.locationCD>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<LocationExtAddress.addressLine1>(cache, row, !isSameAsMain);
				PXUIFieldAttribute.SetEnabled<LocationExtAddress.addressLine2>(cache, row, !isSameAsMain);
				PXUIFieldAttribute.SetEnabled<LocationExtAddress.addressLine3>(cache, row, !isSameAsMain);
				PXUIFieldAttribute.SetEnabled<LocationExtAddress.city>(cache, row, !isSameAsMain);
				PXUIFieldAttribute.SetEnabled<LocationExtAddress.countryID>(cache, row, !isSameAsMain);
				PXUIFieldAttribute.SetEnabled<LocationExtAddress.state>(cache, row, !isSameAsMain);
				PXUIFieldAttribute.SetEnabled<LocationExtAddress.addressType>(cache, row, !isSameAsMain);
				PXUIFieldAttribute.SetEnabled<LocationExtAddress.postalCode>(cache, row, !isSameAsMain);

				row.IsDefault = isDefault;
				row.IsAddressSameAsMain = isSameAsMain;
				row.IsContactSameAsMain = isContactSameAsMain;
			}
		}

		protected virtual void LocationExtAddress_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.NewRow;
			LocationExtAddress oldRow = (LocationExtAddress)e.Row;

			BAccount account = this.BAccountAccessor.Current;
			if ((row.IsAddressSameAsMain ?? false) != (oldRow.IsAddressSameAsMain ?? false))
			{
				//Flag is changed
				if ((row.IsAddressSameAsMain ?? false) == true)
				{
					if (row.DefAddressID != account.DefAddressID)
					{
						Addresses.Delete(row);
						this.SetDefAddressTo(row);
						row.DefAddressID = row.AddressID;
						row.AddressID = null;
					}
				}
				else
				{
					//Create new copy  - based on default address
					if (row.DefAddressID == account.DefAddressID) //Copy only first time
					{
						this.SetDefAddressTo(row);
						Address addr = new Address();
						PXCache<Address>.RestoreCopy(addr, row);

						//row.AddressID = this.GetNextAddressID();
						addr.AddressID = null;
						addr.BAccountID = account.BAccountID;
						addr = (Address)this.Addresses.Cache.Insert(addr);
						row.DefAddressID = addr.AddressID;
						row.AddressID = null;

					}
				}
			}
			if ((oldRow.IsDefault ?? false) != (row.IsDefault ?? false) &&
				row.IsDefault == true)
			{
				if (row.LocationID == account.DefLocationID && row.IsActive != true)
				{
					cache.RaiseExceptionHandling<Location.isActive>(row, null, new PXSetPropertyException(CR.Messages.DefaultLocationCanNotBeNotActive, typeof(Location.isActive).Name));
				}
				else
					this.SetAsDefault(row);
			}
		}

		protected virtual void LocationExtAddress_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = (LocationExtAddress)e.Row;
			var oldRow = (LocationExtAddress)e.OldRow;
			
			var addressId = row.DefAddressID;
			var oldAddressId = oldRow.DefAddressID;

			var addressCache = this.Caches[typeof(Address)];
			if (!object.Equals(addressId, oldAddressId) 
				&& !object.Equals(this.BAccountAccessor.Current.DefAddressID, oldAddressId))
			{
				var oldAddress = (Address)PXSelect<Address,
							Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(this, oldAddressId);
				if (oldAddress != null)
				{
					var oldIsDirty = addressCache.IsDirty;
					addressCache.Delete(oldAddress);
					addressCache.IsDirty = oldIsDirty;
				}
			}
		}

		protected virtual void LocationExtAddress_IsContactSameAsMain_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.Row;
			BAccount account = this.BAccountAccessor.Current;
			if (row != null)
			{
				if ((row.IsContactSameAsMain ?? false) == true)
				{
					if (row.DefContactID != account.DefContactID)
					{
						Contact contact = this.DefLocationContact.Current;
						if (contact != null && contact.ContactID == row.DefContactID)
						{
							this.DefLocationContact.Delete(contact);
						}
						row.DefContactID = account.DefContactID;
					}
				}
				else
				{
					if (row.DefContactID == account.DefContactID)
					{
						Contact cont = new Contact();
						Contact defContact = this.FindContact(account.DefContactID);
						PXCache<Contact>.RestoreCopy(cont, defContact);
						cont.ContactType = ContactTypesAttribute.BAccountProperty;
						cont.BAccountID = account.BAccountID;
						cont.ContactID = null;
					    cont.NoteID = null;
						cont = (Contact)this.DefLocationContact.Cache.Insert(cont);
						row.DefContactID = cont.ContactID;
					}
				}

			}
		}

		protected virtual void LocationExtAddress_PostalCode_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.Row;
			if (row.IsAddressSameAsMain ?? false)
			{
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_CountryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Address addr = (Address)e.Row;
			if ((string)e.OldValue != addr.CountryID)
			{
				addr.State = null;
			}
		}

		protected virtual void LocationExtAddress_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			LocationExtAddress row = (LocationExtAddress)e.Row;
			BAccount acct = this.BAccountAccessor.Current;
			if (acct == null && (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				cache.SetStatus(e.Row, PXEntryStatus.InsertedDeleted);
				e.Cancel = true;
			}
		}

		protected virtual void LocationExtAddress_VAPAccountLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = sender.GetValue<LocationExtAddress.locationID>(e.Row);
			e.Cancel = (e.Row != null);
		}

		protected virtual void LocationExtAddress_CARAccountLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = sender.GetValue<LocationExtAddress.locationID>(e.Row);
			e.Cancel = (e.Row != null);
		}

		protected virtual void LocationExtAddress_VPaymentInfoLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = sender.GetValue<LocationExtAddress.locationID>(e.Row);
			e.Cancel = (e.Row != null);
		}

		object _KeyToAbort = null;

		protected virtual void LocationExtAddress_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					if ((int?)sender.GetValue<LocationExtAddress.vAPAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<LocationExtAddress.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VAPAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<LocationExtAddress.vAPAccountLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<LocationExtAddress.vPaymentInfoLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<LocationExtAddress.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VPaymentInfoLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<LocationExtAddress.vPaymentInfoLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<LocationExtAddress.cARAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<LocationExtAddress.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("CARAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<LocationExtAddress.cARAccountLocationID>(e.Row, _KeyToAbort);
					}
				}
				else
				{
					if (e.TranStatus == PXTranStatus.Aborted)
					{
						if (object.Equals(_KeyToAbort, sender.GetValue<LocationExtAddress.vAPAccountLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<LocationExtAddress.locationID>(e.Row);
							sender.SetValue<LocationExtAddress.vAPAccountLocationID>(e.Row, KeyAborted);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<LocationExtAddress.vPaymentInfoLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<LocationExtAddress.locationID>(e.Row);
							sender.SetValue<LocationExtAddress.vPaymentInfoLocationID>(e.Row, KeyAborted);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<LocationExtAddress.cARAccountLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<LocationExtAddress.locationID>(e.Row);
							sender.SetValue<LocationExtAddress.cARAccountLocationID>(e.Row, KeyAborted);
						}
					}
					_KeyToAbort = null;
				}
			}
		}

		#endregion

		#region Ext Contact event handlers

		protected virtual void ContactExtAddress_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ContactExtAddress row = (ContactExtAddress)e.Row;
			BAccount account = this.BAccountAccessor.Current;
			bool IsAddressSameAsMain = false;
			bool isDefault = false;
			if (account != null && row != null)
			{
				IsAddressSameAsMain = account.DefAddressID.HasValue && (row.DefAddressID == account.DefAddressID);
				isDefault = (row.ContactID == account.DefContactID);
				row.IsAddressSameAsMain = IsAddressSameAsMain;
				row.IsDefault = isDefault;
			}
			PXUIFieldAttribute.SetEnabled<ContactExtAddress.addressLine1>(cache, row, !IsAddressSameAsMain);
			PXUIFieldAttribute.SetEnabled<ContactExtAddress.addressLine2>(cache, row, !IsAddressSameAsMain);
			PXUIFieldAttribute.SetEnabled<ContactExtAddress.addressLine3>(cache, row, !IsAddressSameAsMain);
			PXUIFieldAttribute.SetEnabled<ContactExtAddress.city>(cache, row, !IsAddressSameAsMain);
			PXUIFieldAttribute.SetEnabled<ContactExtAddress.countryID>(cache, row, !IsAddressSameAsMain);
			PXUIFieldAttribute.SetEnabled<ContactExtAddress.state>(cache, row, !IsAddressSameAsMain);
			PXUIFieldAttribute.SetEnabled<ContactExtAddress.addressType>(cache, row, !IsAddressSameAsMain);
			PXUIFieldAttribute.SetEnabled<ContactExtAddress.postalCode>(cache, row, !IsAddressSameAsMain);

		}

		protected virtual void ContactExtAddress_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			ContactExtAddress row = (ContactExtAddress)e.Row;
			BAccount account = this.BAccountAccessor.Current;
			bool isDefault = false;
			if (account != null)
			{
				if (row.ContactID == account.DefContactID)
				{
					isDefault = true;
					ContactExtAddress defRow = null;
					foreach (ContactExtAddress it in this.ExtContacts.Select())
					{
						if (!object.ReferenceEquals(it, row))
						{
							defRow = it;
							break;
						}
					}
					this.SetAsDefault(defRow);
				}
			}
			if (isDefault)
			{
				this.ExtContacts.View.RequestRefresh();
			}
		}

		protected virtual void ContactExtAddress_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{

			ContactExtAddress row = (ContactExtAddress)e.NewRow;
			ContactExtAddress oldRow = (ContactExtAddress)e.Row;
			BAccount account = this.BAccountAccessor.Current;
			if ((row.IsAddressSameAsMain ?? false) != (oldRow.IsAddressSameAsMain ?? false))
			{
				//Flag is changed
				if ((row.IsAddressSameAsMain ?? false) == true)
				{
					if (row.DefAddressID != account.DefAddressID)
					{
						Addresses.Delete(row);
						this.SetDefAddressTo(row);
						row.DefAddressID = row.AddressID;
						row.AddressID = null;
					}
				}
				else
				{
					//Insert copy of default address
					if (row.DefAddressID == account.DefAddressID) //Copy only first time
					{
						this.SetDefAddressTo(row);
						row.AddressID = null;
						row.DefAddressID = row.AddressID;
					}
				}
			}

			if ((oldRow.IsDefault ?? false) != (row.IsDefault ?? false) &&
				(row.IsDefault ?? false) == true)
			{
				this.SetAsDefault(row);
			}
		}

		protected virtual void ContactExtAddress_IsMain_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			ContactExtAddress row = (ContactExtAddress)e.Row;
			e.NewValue = true;
		}

		#endregion

		#region Address Events handlerd

		protected virtual void Address_CountryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Address addr = (Address)e.Row;
			if ((string)e.OldValue != addr.CountryID)
			{
				addr.State = null;
			}
		}

		protected virtual void Address_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			Address addr = (Address)e.Row;
			if (e.Operation == PXDBOperation.Insert)
			{

				LocationExtAddress defLoc = this.DefLocation.Current; //Only current Location can update address
				BAccount account = this.BAccountAccessor.Current;
				if (defLoc == null)
				{
					defLoc = this.DefLocation.Select();
				}
				if (account != null && defLoc != null && addr.AddressID == defLoc.DefAddressID)
				{
					bool isSameAsMain = (account.DefAddressID == defLoc.DefAddressID);
					if (isSameAsMain == false)
					{
						Address copy = (Address)this.Addresses.Cache.CreateCopy(addr);
						this.Addresses.Cache.RestoreCopy(addr, defLoc);
						addr.AddressID = copy.AddressID;
						addr.BAccountID = copy.BAccountID;
						addr.AddressType = copy.AddressType;
						addr.NoteID = copy.NoteID;
					}
				}
			}
		}

        protected virtual void Address_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            this.UpdateLocations((Address)e.Row);
        }

        private void UpdateLocations(Address addr)
        {
            BAccount account = this.BAccountAccessor.Current;           
            
            foreach (LocationExtAddress iLocation in this.Locations.Select())
            {
                if (iLocation != null && iLocation.DefAddressID == addr.AddressID) 
                {
                    PXCache<Address>.RestoreCopy(iLocation, addr);
                    iLocation.DefAddressID = iLocation.AddressID;
                    iLocation.AddressID = null;
                    iLocation.NoteID = null;
                }
            }

            
        }


		#endregion

		#region Soap-related event handlers

		protected virtual void ContactExtAddress_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.ExternalCall && BAccountAccessor.Cache.GetStatus(BAccountAccessor.Current) == PXEntryStatus.Inserted)
			{
				Contact cont = new Contact();
				cont = Contacts.Insert(cont);
				foreach (string f in Contacts.Cache.Fields)
				{
					object v = cache.GetValue(e.Row, f);
					if (v != null)
					{
						Contacts.Cache.SetValue(cont, f, v);
					}
				}
				cache.Remove(e.Row);
				cont.BAccountID = BAccountAccessor.Current.BAccountID;

				foreach (LocationExtAddress loc in Locations.Cache.Inserted)
				{
					if (loc.IsContactSameAsMain != true && (loc.DefContactID == null || loc.DefContactID == BAccountAccessor.Current.DefContactID))
					{
						loc.DefContactID = cont.ContactID;
						break;
					}
				}
			}
		}

		protected virtual void LocationExtAddress_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (e.ExternalCall && BAccountAccessor.Cache.GetStatus(BAccountAccessor.Current) == PXEntryStatus.Inserted)
			{
				LocationExtAddress existing = (LocationExtAddress)cache.Locate(e.Row);
				if (existing != null)
				{
					cache.Delete(existing);
				}
			}
		}

		protected virtual void LocationExtAddress_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.ExternalCall && BAccountAccessor.Cache.GetStatus(BAccountAccessor.Current) == PXEntryStatus.Inserted)
			{
				LocationExtAddress loc = null;
				if (BAccountAccessor.Current.DefLocationID != null)
				{
					foreach (LocationExtAddress l in cache.Inserted)
					{
						if (l.BAccountID == BAccountAccessor.Current.BAccountID && l.LocationID == BAccountAccessor.Current.DefLocationID)
						{
							loc = l;
							break;
						}
					}
					if (loc != null)
					{
						int? defContactID = loc.DefContactID;
						cache.RestoreCopy(loc, e.Row);
						loc.LocationID = BAccountAccessor.Current.DefLocationID;
						loc.DefContactID = defContactID;
						cache.Remove(e.Row);
					}
					else
					{
						loc = (LocationExtAddress)e.Row;
						loc.LocationID = BAccountAccessor.Current.DefLocationID;
						cache.Normalize();
					}
				}
				else
				{
					loc = (LocationExtAddress)e.Row;
					BAccountAccessor.Current.DefLocationID = loc.LocationID;
				}
				if (loc != null && loc.IsAddressSameAsMain != true)
				{
					Address addr = DefAddress.Insert(loc);
					loc.DefAddressID = addr.AddressID;
				}
			}
		}

		protected virtual void Contact_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.ExternalCall && BAccountAccessor.Cache.GetStatus(BAccountAccessor.Current) == PXEntryStatus.Inserted)
			{
				BAccountAccessor.Current.DefContactID = DetailInserted<Contact>(cache, (Contact)e.Row, BAccountAccessor.Current.DefContactID);
			}
		}

		protected virtual void Contact_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (e.ExternalCall)
			{//We assuming that only contacts of type ContactTypes.BAccountProperty may be inserted  using this graph  
				((Contact)e.Row).ContactType = ContactTypesAttribute.BAccountProperty;
			}
		}

		protected virtual void Address_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			Address row = e.Row as Address;
		    if (row != null)
		    {
		        if (e.ExternalCall && BAccountAccessor.Cache.GetStatus(BAccountAccessor.Current) == PXEntryStatus.Inserted)
		        {
		            BAccountAccessor.Current.DefAddressID = DetailInserted<Address>(cache, row,
		                BAccountAccessor.Current.DefAddressID);
		        }
		        else
		        {
                    if (BAccount.Current.DefAddressID == null)
                    {
                        BAccount.Current.DefAddressID = row.AddressID;
                        Caches[typeof(BAccount)].MarkUpdated(BAccount.Current);
                    }
                    else
                    {
                        var ret = DefAddress.SelectSingle();
                        if (ret == null)
                        {
                            BAccountAccessor.Current.DefAddressID = row.AddressID;
                            Caches[typeof(Base)].MarkUpdated(BAccountAccessor.Current);
                        }
                    }
		        }
		    }
		}

		protected virtual int? DetailInserted<Table>(PXCache cache, Table row, int? existing)
			where Table : class, IBqlTable, new()
		{
			if (existing != null)
			{
				Table table = new Table();
				cache.SetValue<BAccount.bAccountID>(table, BAccountAccessor.Current.BAccountID);
				cache.SetValue(table, cache.Keys[0], existing);
				table = (Table)cache.Locate(table);
				if (table != null)
				{
					cache.RestoreCopy(table, row);
					cache.SetValue(table, cache.Keys[0], existing);
					cache.Remove(row);
					cache.Current = table;
				}
				else
				{
					cache.SetValue(row, cache.Keys[0], existing);
					cache.Normalize();
				}
				return existing;
			}
			else
			{
				return (int?)cache.GetValue(row, cache.Keys[0]);
			}
		}
		#endregion

		#region Utility Functions

		protected virtual void SetAsDefault(LocationExtAddress row)
		{
			Base account = this.BAccountAccessor.Current;
			if (account != null)
			{
				int? defLocationID = (row != null) ? row.LocationID : null;
				if (account.DefLocationID != defLocationID)
				{
					LocationExtAddress current = null;

					foreach (LocationExtAddress other in PXParentAttribute.SelectSiblings(Locations.Cache, row))
					{
						if (!object.Equals(other.LocationID, other.VAPAccountLocationID))
						{
							current = PXSelect<LocationExtAddress, Where<LocationExtAddress.bAccountID, Equal<Required<CR.BAccount.bAccountID>>, And<LocationExtAddress.locationID, Equal<Required<CR.BAccount.defLocationID>>>>>.Select(this, account.BAccountID, account.DefLocationID);

							other.VAPAccountLocationID = row.LocationID;
							if (object.Equals(other, row) && current != null)
							{
								other.VAPAccountID = current.VAPAccountID;
								other.VAPSubID = current.VAPSubID;
								other.VRetainageAcctID = current.VRetainageAcctID;
								other.VRetainageSubID = current.VRetainageSubID;
							}

							Locations.Cache.MarkUpdated(other);
							Locations.Cache.IsDirty = true;
						}

						if (!object.Equals(other.LocationID, other.VPaymentInfoLocationID))
						{
							current = PXSelect<LocationExtAddress, Where<LocationExtAddress.bAccountID, Equal<Required<CR.BAccount.bAccountID>>, And<LocationExtAddress.locationID, Equal<Required<CR.BAccount.defLocationID>>>>>.Select(this, account.BAccountID, account.DefLocationID);

							other.VPaymentInfoLocationID = row.LocationID;
							if (object.Equals(other, row) && current != null)
							{
								other.VCashAccountID = current.VCashAccountID;
								other.VPaymentMethodID = current.VPaymentMethodID;
								other.VPaymentLeadTime = current.VPaymentLeadTime;
								other.VSeparateCheck = current.VSeparateCheck;
								other.VRemitAddressID = current.VRemitAddressID;
								other.VRemitContactID = current.VRemitContactID;
							}

							Locations.Cache.MarkUpdated(other);
							Locations.Cache.IsDirty = true;
						}

						if (!object.Equals(other.LocationID, other.CARAccountLocationID))
						{
							current = PXSelect<LocationExtAddress, Where<LocationExtAddress.bAccountID, Equal<Required<CR.BAccount.bAccountID>>, And<LocationExtAddress.locationID, Equal<Required<CR.BAccount.defLocationID>>>>>.Select(this, account.BAccountID, account.DefLocationID);

							other.CARAccountLocationID = row.LocationID;
							if (object.Equals(other, row) && current != null)
							{
								other.CARAccountID = current.CARAccountID;
								other.CARSubID = current.CARSubID;
								other.CRetainageAcctID = current.CRetainageAcctID;
								other.CRetainageSubID = current.CRetainageSubID;
							}

							Locations.Cache.MarkUpdated(other);
							Locations.Cache.IsDirty = true;
						}
					}

					account.DefLocationID = defLocationID;
					BAccountAccessor.Update(account);
				}
			}
		}

		protected virtual void SetAsDefault(ContactExtAddress row)
		{
			BAccount account = this.BAccountAccessor.Current;
			PXCache cache = this.Locations.Cache;
			if (account != null)
			{
				int? defContactID = (row != null) ? row.ContactID : null;
				if (account.DefContactID != defContactID)
				{
					account.DefContactID = defContactID;
					this.BAccountAccessor.View.RequestRefresh();
					this.BAccountAccessor.Cache.MarkUpdated(account);
				}
			}
		}

		protected void SetDefAddressTo<T>(T aDest) where T : Address, IDefAddressAccessor
		{
			BAccount acct = this.BAccountAccessor.Current;
			if (acct != null)
			{
				aDest.DefAddressID = acct.DefAddressID;
				if (acct.DefAddressID.HasValue)
				{
					Address addr = this.FindAddress(acct.DefAddressID);
					PXCache<Address>.RestoreCopy(aDest, addr);
				}
			}
		}

		protected virtual Address FindAddress(int? aId)
		{
			if (aId.HasValue)
			{
				foreach (Address it in this.Addresses.Select())
				{
					if (it.AddressID == aId) return it;
				}
			}
			return null;
		}

		protected virtual Contact FindContact(int? aId)
		{
			if (aId.HasValue)
			{
				foreach (Contact it in this.Contacts.Select())
				{
					if (it.ContactID == aId) return it;
				}
			}
			return null;
		}

		protected void ChangeBAccountType(BAccount descendantEntity, string type)
		{
			BAccountItself baccount = CurrentBAccountItself.SelectSingle(descendantEntity.BAccountID);
			if (baccount != null)
			{
				baccount.Type = type;
				CurrentBAccountItself.Update(baccount);
			}
		}

		#endregion
	}

	public static class BAccountUtility
	{
		public static BAccount FindAccount(PXGraph graph, int? aBAccountID)
		{
			BAccount acct = null;
			if (aBAccountID.HasValue)
			{
				PXSelectBase<BAccount> sel = new PXSelectReadonly<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>(graph);
				acct = (BAccount)sel.View.SelectSingle(aBAccountID);
			}
			return acct;

		}
		public static void ViewOnMap(Address aAddr)
		{
			PX.Data.MapRedirector map = SitePolicy.CurrentMapRedirector;
			if (map != null && aAddr != null)
			{
				PXGraph graph = new PXGraph();
				Country country = PXSelectorAttribute.Select<Address.countryID>(graph.Caches[typeof(Address)], aAddr) as Country;
				map.ShowAddress(country != null ? country.Description : aAddr.CountryID, aAddr.State, aAddr.City, aAddr.PostalCode, aAddr.AddressLine1, aAddr.AddressLine2, aAddr.AddressLine3);
			}

		}

        public static void ViewOnMap(CRAddress aAddr)
        {
            PX.Data.MapRedirector map = SitePolicy.CurrentMapRedirector;
            if (map != null && aAddr != null)
            {
                PXGraph graph = new PXGraph();
                Country country = PXSelectorAttribute.Select<CRAddress.countryID>(graph.Caches[typeof(CRAddress)], aAddr) as Country;
                map.ShowAddress(country != null ? country.Description : aAddr.CountryID, aAddr.State, aAddr.City, aAddr.PostalCode, aAddr.AddressLine1, aAddr.AddressLine2, null);
            }
        }

        public static void ViewOnMap<TAddress, FCountryID>(CS.IAddress aAddr)
            where TAddress : class, IBqlTable, CS.IAddress, new()
            where FCountryID : IBqlField
        {
            PX.Data.MapRedirector map = SitePolicy.CurrentMapRedirector;
            if (map != null && aAddr != null)
            {
                PXGraph graph = new PXGraph();
                Country country = PXSelectorAttribute.Select<FCountryID>(graph.Caches[typeof(TAddress)], aAddr) as Country;
                map.ShowAddress(country != null ? country.Description : aAddr.CountryID, aAddr.State, aAddr.City, aAddr.PostalCode, aAddr.AddressLine1, aAddr.AddressLine2, null);
            }
        }
    }


	/// <exclude/>
	[System.SerializableAttribute()]
	public partial class BAccountR : BAccount
	{
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion
		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		[PXDimensionSelector("BIZACCT",
			typeof(Search2<BAccountR.acctCD,
					LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>,
					LeftJoin<Address, On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>>>,
				Where2<Where<BAccountR.type, Equal<BAccountType.customerType>,
					Or<BAccountR.type, Equal<BAccountType.prospectType>,
					Or<BAccountR.type, Equal<BAccountType.combinedType>,
					Or<BAccountR.type, Equal<BAccountType.vendorType>>>>>,
					And<Match<Current<AccessInfo.userName>>>>>),
			typeof(BAccountR.acctCD),
			typeof(BAccountR.acctCD), typeof(BAccountR.acctName), typeof(BAccountR.type), typeof(BAccountR.classID), typeof(BAccountR.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail),
			DescriptionField = typeof(BAccountR.acctName))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override String AcctCD
		{
			get
			{
				return this._AcctCD;
			}
			set
			{
				this._AcctCD = value;
			}
		}
		#endregion
		#region AcctName
		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion
		#region DefLocationID
		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		#endregion
		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		#endregion
		#region IsCustomerOrCombined
		public new abstract class isCustomerOrCombined : PX.Data.BQL.BqlBool.Field<isCustomerOrCombined> { }
		#endregion
		#region ParentBAccountID
		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		#endregion
		#region DefContactID
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Search<Contact.contactID>))]
		[PXUIField(DisplayName = "Default Contact", Visibility = PXUIVisibility.Invisible)]
		public override Int32? DefContactID
		{
			get
			{
				return base.DefContactID;
			}
			set
			{
				base.DefContactID = value;
			}
		}
		#endregion
		#region DefAddressID
		public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
        #endregion
        #region ViewInCrm
        public new abstract class viewInCrm : PX.Data.BQL.BqlBool.Field<viewInCrm> { }        
        [PXBool]
        [PXUIField(DisplayName = "View In CRM")]
        public new virtual bool? ViewInCrm
        {
            get
            {
                return this._ViewInCrm;
            }
            set
            {
                this._ViewInCrm = value;
            }
        }
        #endregion

		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new string[] { Active, Hold, HoldPayments, Inactive, OneTime, CreditHold },
						new string[] { Messages.Active, Messages.Hold, Messages.HoldPayments, Messages.Inactive, Messages.OneTime, Messages.CreditHold })
				{ }
			}

			public const string Active = "A";
			public const string Hold = "H";
			public const string HoldPayments = "P";
			public const string Inactive = "I";
			public const string OneTime = "T";
			public const string CreditHold = "C";

			public class active : PX.Data.BQL.BqlString.Constant<active>
			{
				public active() : base(Active) {; }
			}
			public class hold : PX.Data.BQL.BqlString.Constant<hold>
			{
				public hold() : base(Hold) {; }
			}
			public class holdPayments : PX.Data.BQL.BqlString.Constant<holdPayments>
			{
				public holdPayments() : base(HoldPayments) {; }
			}
			public class inactive : PX.Data.BQL.BqlString.Constant<inactive>
			{
				public inactive() : base(Inactive) {; }
			}
			public class oneTime : PX.Data.BQL.BqlString.Constant<oneTime>
			{
				public oneTime() : base(OneTime) {; }
			}
			public class creditHold : PX.Data.BQL.BqlString.Constant<creditHold>
			{
				public creditHold() : base(CreditHold) {; }
			}
		}
		#endregion
    }

	/// <exclude/>
	[Serializable]
	[PXPrimaryGraph(typeof(BusinessAccountMaint))]
	[CRCacheIndependentPrimaryGraphList(new Type[] {typeof(CR.BusinessAccountMaint)},
		new Type[] {typeof(Select<BAccount, 
			Where<BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>, 
					Or<Current<BAccount.bAccountID>, Less<Zero>>>>)
		})]
    [PXHidden]
	public partial class BAccountCRM : BAccount
	{
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion
		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		[PXDimensionSelector("BIZACCT",
			typeof(Search2<BAccountCRM.acctCD,
					LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccountCRM.bAccountID>, And<Contact.contactID, Equal<BAccountCRM.defContactID>>>,
					LeftJoin<Address, On<Address.bAccountID, Equal<BAccountCRM.bAccountID>, And<Address.addressID, Equal<BAccountCRM.defAddressID>>>>>,
				Where2<Where<BAccountCRM.type, Equal<BAccountType.customerType>,
					Or<BAccountCRM.type, Equal<BAccountType.prospectType>,
					Or<BAccountCRM.type, Equal<BAccountType.combinedType>,
					Or<BAccountCRM.type, Equal<BAccountType.vendorType>>>>>,
					And<Match<Current<AccessInfo.userName>>>>>),
			typeof(BAccountCRM.acctCD),
			typeof(BAccountCRM.acctCD), typeof(BAccountCRM.acctName), typeof(BAccountCRM.type), typeof(BAccountCRM.classID), typeof(BAccountCRM.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override String AcctCD
		{
			get
			{
				return this._AcctCD;
			}
			set
			{
				this._AcctCD = value;
			}
		}
		#endregion
		#region AcctName
		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion
		#region DefLocationID
		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		#endregion
		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		#endregion
		#region ParentBAccountID
		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		#endregion
		#region DefContactID
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Search<Contact.contactID>))]
		public override Int32? DefContactID
		{
			get
			{
				return base.DefContactID;
			}
			set
			{
				base.DefContactID = value;
			}
		}
		#endregion
		#region DefAddressID
		public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		#endregion
	}
}
