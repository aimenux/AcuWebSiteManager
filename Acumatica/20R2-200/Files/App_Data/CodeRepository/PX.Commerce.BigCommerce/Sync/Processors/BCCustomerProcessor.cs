using PX.Api.ContractBased.Models;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;
using Location = PX.Objects.CR.Standalone.Location;

namespace PX.Commerce.BigCommerce
{
	public class BCCustomerEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary { get => Customer; }
		public override IMappedEntity[] PreProcessors { get => new IMappedEntity[] { CustomerPriceClass }; }
		public IMappedEntity[] Entities => new IMappedEntity[] { Customer };

		public CustomerLocation ConnectorGeneratedAddress;
		public MappedCustomer Customer;
		public MappedGroup CustomerPriceClass;
		public List<MappedLocation> CustomerAddresses;
	}

	public class BCCustomerRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped)
		{
			return base.Restrict<MappedCustomer>(mapped, delegate (MappedCustomer obj)
			{
				BCBindingExt bindingExt = processor.GetBindingExt<BCBindingExt>();
				PX.Objects.AR.Customer guestCustomer = bindingExt.GuestCustomerID != null ? PX.Objects.AR.Customer.PK.Find((PXGraph)processor, bindingExt.GuestCustomerID) : null;

				if (guestCustomer != null && obj.Local != null && obj.Local.CustomerID?.Value != null)
				{
					if (guestCustomer.AcctCD.Trim() == obj.Local.CustomerID?.Value)
					{
						return new FilterResult(FilterStatus.Invalid, 
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogCustomerSkippedGuest, obj.Local.CustomerID?.Value ?? obj.Local.SyncID.ToString()));
					}
				}
				return null;
			});
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped)
		{
			return null;
		}
	}

	[BCProcessor(typeof(BCConnector), BCEntitiesAttribute.Customer, BCCaptions.Customer,
		IsInternal = false,
		Direction = SyncDirection.Bidirect,
		PrimaryDirection = SyncDirection.Bidirect,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.AR.CustomerMaint),
		ExternTypes = new Type[] { typeof(CustomerData) },
		LocalTypes = new Type[] { typeof(Core.API.Customer) },
		AcumaticaPrimaryType = typeof(PX.Objects.AR.Customer),
		AcumaticaPrimarySelect = typeof(PX.Objects.AR.Customer.acctCD),
		URL = "customers/{0}/edit"
	)]
	[BCProcessorRealtime(PushSupported = true, HookSupported = true,
		PushSources = new String[] { "BC-PUSH-Customers", "BC-PUSH-Locations" },
		WebHookType = typeof(WebHookCustomerAddress),
		WebHooks = new String[]
		{
			"store/customer/created",
			"store/customer/updated",
			"store/customer/deleted",
			"store/customer/address/created",
			"store/customer/address/updated",
			"store/customer/address/deleted"
		})]
	public class BCCustomerProcessor : BCLocationBaseProcessor<BCCustomerProcessor, BCCustomerEntityBucket, MappedCustomer>, IProcessor
	{
		bool isLocationActive;
		protected CustomerPriceClassRestDataProvider customerPriceClassDataProvider;		

		#region Initialization
		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			customerPriceClassDataProvider = new CustomerPriceClassRestDataProvider(client);
			isLocationActive = ConnectorHelper.GetConnectorBinding(operation.ConnectorType, operation.Binding).ActiveEntities.Any(x => x == BCEntitiesAttribute.Address);
			if (isLocationActive)
			{
				locationProcessor = PXGraph.CreateInstance<BCLocationProcessor>();
				((BCLocationProcessor)locationProcessor).Initialise(iconnector, operation.Clone().With(_ => { _.EntityType = BCEntitiesAttribute.Address; return _; }), States);
			}
		}
		#endregion

		#region Common
		public override MappedCustomer PullEntity(Guid? localID, Dictionary<string, object> fields)
		{
			Core.API.Customer impl = cbapi.GetByID<Core.API.Customer>(localID);
			if (impl == null) return null;

			MappedCustomer obj = new MappedCustomer(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}
		public override MappedCustomer PullEntity(string externID, string jsonObject)
		{
			FilterCustomers filter = new FilterCustomers { Include = "addresses,formfields" };
			filter.Id = externID.KeySplit(0);
			CustomerData data = customerDataProviderV3.GetAll(filter).FirstOrDefault();
			if (data == null) return null;

			MappedCustomer obj = new MappedCustomer(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());
			if (isLocationActive) //location is active then check if anything is changed and  force set pending
			{

				BCSyncStatus customerStatus = PXSelect<BCSyncStatus,
						Where<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
							And<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>,
							And<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>,
							And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>
						.Select(this, Operation.ConnectorType, Operation.Binding, BCEntitiesAttribute.Customer, data.Id.ToString());
				if (customerStatus == null) return obj;
				bool changed = IsLocationModified(customerStatus.SyncID, data.Addresses);
				if (changed)
					obj.ExternTimeStamp = DateTime.MaxValue;
			}
			return obj;
		}

		public override IEnumerable<MappedCustomer> PullSimilar(IExternEntity entity, out string uniqueField)
		{
			uniqueField = ((CustomerData)entity)?.Email;
			if (uniqueField == null) return null;

			Core.API.Customer[] impls = cbapi.GetAll<Core.API.Customer>(new Core.API.Customer() { MainContact = new Core.API.Contact() { Email = uniqueField.SearchField(), ReturnBehavior = ReturnBehavior.OnlySpecified } },
				filters: GetFilter(Operation.EntityType).LocalFiltersRows.Cast<PXFilterRow>());
			if (impls == null) return null;

			return impls.Select(impl => new MappedCustomer(impl, impl.SyncID, impl.SyncTime));
		}
		public override IEnumerable<MappedCustomer> PullSimilar(ILocalEntity entity, out string uniqueField)
		{
			uniqueField = ((Core.API.Customer)entity)?.MainContact?.Email?.Value;
			if (uniqueField == null) return null;
			FilterCustomers filter = new FilterCustomers { Include = "addresses,formfields", Email = uniqueField };
			IEnumerable<CustomerData> datas = customerDataProviderV3.GetAll(filter);
			if (datas == null) return null;

			return datas.Select(data => new MappedCustomer(data, data.Id.ToString(), data.DateModifiedUT.ToDate()));
		}

		public override void AddExternCustomField(BCCustomerEntityBucket entity, string cFiledName, object cFieldValue)
		{
			if (cFiledName.Contains(BCConstants.FormFields))
			{
				var fieldsName = cFiledName.Split('|');
				if (fieldsName.Length < 2) return;
				if (entity.Customer.Extern.FormFields == null) entity.Customer.Extern.FormFields = new List<CustomerFormFieldData>();

				dynamic value = cFieldValue;
				var collectionType = cFieldValue.GetType().IsGenericType &&
					(cFieldValue.GetType().GetGenericTypeDefinition() == typeof(List<>) ||
					 cFieldValue.GetType().GetGenericTypeDefinition() == typeof(IList<>));
				var formFieldType = formFieldsList.Where(x => x.Item1 == Operation.EntityType && x.Item2 == fieldsName[1].Trim()).FirstOrDefault()?.Item3;
				if (collectionType && formFieldType == BCCustomerFormFieldsAttribute.JArray)
				{
					value = new List<string>();
					foreach (object item in (IList<object>)cFieldValue)
					{
						((List<string>)value).Add(Convert.ToString(item));
					}
				}
				else if (!collectionType && formFieldType == BCCustomerFormFieldsAttribute.JArray)
				{
					var strValue = Convert.ToString(cFieldValue);
					if (strValue.Length > 1 && strValue.Contains(","))
						value = strValue.Split(',');
					else if (strValue.Length > 1 && strValue.Contains(";"))
						value = strValue.Split(';');
					else
						value = strValue == null ? new List<string>() : new List<string> { strValue };
				}
				else if (collectionType && formFieldType == BCCustomerFormFieldsAttribute.Value)
				{
					value = string.Empty;
					foreach (object item in (IList<object>)cFieldValue)
					{
						value = (string)value + (Convert.ToString(item));
					}
				}
				else
				{
					value = cFieldValue == null ? string.Empty : Convert.ToString(cFieldValue);
				}
				entity.Customer.Extern.FormFields.Add(new CustomerFormFieldData()
				{
					CustomerId = entity.Customer.Extern.Id,
					Name = fieldsName[1].Trim(),
					Value = value
				});
			}
		}

		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			FilterCustomers filter = new FilterCustomers { Include = "addresses,formfields" };
			if (minDateTime != null)
				filter.MinDateModified = minDateTime;
			if (maxDateTime != null)
				filter.MaxDateModified = maxDateTime;
			IEnumerable<CustomerData> datas = customerDataProviderV3.GetAll(filter);

			BCEntity entity = GetEntity(Operation.EntityType);
			int countNum = 0;
			List<IMappedEntity> mappedList = new List<IMappedEntity>();

			foreach (CustomerData data in datas)
			{
				IMappedEntity obj = new MappedCustomer(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());
				if (entity.EntityType != obj.EntityType)
					entity = GetEntity(obj.EntityType);

				mappedList.Add(obj);
				countNum++;

				if (countNum % BatchFetchCount == 0)
				{
					ProcessMappedListForImport(ref mappedList);
				}
			}
			if (mappedList.Any())
			{
				ProcessMappedListForImport(ref mappedList);
			}

		}

		public override EntityStatus GetBucketForImport(BCCustomerEntityBucket bucket, BCSyncStatus bcstatus)
		{
			FilterCustomers filter = new FilterCustomers { Include = "addresses,formfields", Id = bcstatus.ExternID };
			CustomerData data = customerDataProviderV3.GetAll(filter).FirstOrDefault();
			if (data == null) return EntityStatus.None;

			MappedCustomer obj = bucket.Customer = bucket.Customer.Set(data, data.Id?.ToString(), data.DateModifiedUT.ToDate());
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);
			if (isLocationActive && status == EntityStatus.Syncronized)
			{
				bool changed = IsLocationModified(obj.SyncID, data.Addresses);

				if (changed)
					status = EnsureStatus(obj, SyncDirection.Import, resync: changed);

			}

			if (data.CustomerGroupId != null && data.CustomerGroupId > 0 && GetEntity(BCEntitiesAttribute.CustomerPriceClass)?.IsActive == true)
			{
				var priceClass = customerPriceClassDataProvider.GetByID(data.CustomerGroupId.ToString());
				if (priceClass != null)
				{
					MappedGroup customerPriceClassObj = bucket.CustomerPriceClass = bucket.CustomerPriceClass.Set(priceClass, priceClass.Id?.ToString(), priceClass.CalculateHash());
					EntityStatus priceClassStatus = EnsureStatus(customerPriceClassObj, SyncDirection.Import);
				}
			}

			return status;
		}

		public override void MapBucketImport(BCCustomerEntityBucket bucket, IMappedEntity existing)
		{
			MappedCustomer customerObj = bucket.Customer;
			Core.API.Customer customerImpl = customerObj.Local = new Core.API.Customer();
			customerImpl.Custom = GetCustomFieldsForImport();

			//General Info
			string firstLastName = CustomerNameResolver(customerObj.Extern.FirstName, customerObj.Extern.LastName, (int)customerObj.Extern.Id);
			customerImpl.CustomerName = (string.IsNullOrEmpty(customerObj.Extern.Company) ? firstLastName : customerObj.Extern.Company).ValueField();
			customerImpl.AccountRef = APIHelper.ReferenceMake(customerObj.Extern.Id, GetBinding().BindingName).ValueField();
			customerImpl.CustomerClass = customerObj.LocalID == null || existing?.Local == null ? GetBindingExt<BCBindingExt>().CustomerClassID?.ValueField() : null;
			if (customerObj.Extern.CustomerGroupId > 0)
			{
				PX.Objects.AR.ARPriceClass priceClass = PXSelectJoin<PX.Objects.AR.ARPriceClass,
				LeftJoin<BCSyncStatus, On<PX.Objects.AR.ARPriceClass.noteID, Equal<BCSyncStatus.localID>>>,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
					And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.Select(this, BCEntitiesAttribute.CustomerPriceClass, customerObj.Extern.CustomerGroupId?.ToString());
				if (GetEntity(BCEntitiesAttribute.CustomerPriceClass)?.IsActive == true)
				{
					if (priceClass == null) throw new PXException(BCMessages.PriceClassNotSyncronizedForItem, customerObj.Extern.CustomerGroupId?.ToString());
				}
				customerImpl.PriceClassID = priceClass?.PriceClassID?.ValueField();
			}
			else
				customerImpl.PriceClassID = new StringValue() { Value = null };
			//Main Contact
			Core.API.Contact contactImpl = customerImpl.MainContact = new Core.API.Contact();
			contactImpl.FullName = customerImpl.CustomerName; //FullName is mapped to the CompanyName
			contactImpl.Attention = firstLastName.ValueField();
			contactImpl.Email = customerObj.Extern.Email.ValueField();
			contactImpl.Phone2 = customerObj.Extern.Phone.ValueField();
			contactImpl.Active = true.ValueField();
			Core.API.Address addressImpl = contactImpl.Address = new Core.API.Address();
			bucket.CustomerAddresses = new List<MappedLocation>();
			StringValue shipVia = null;
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			if (bindingExt.CustomerClassID != null)
			{
				PX.Objects.AR.CustomerClass customerClass = PXSelect<PX.Objects.AR.CustomerClass, Where<PX.Objects.AR.CustomerClass.customerClassID, Equal<Required<PX.Objects.AR.CustomerClass.customerClassID>>>>.Select(this, bindingExt.CustomerClassID);
				if (customerClass != null)
				{
					addressImpl.Country = customerClass.CountryID.ValueField(); // no address is present then set country from customer class
					shipVia = customerClass.ShipVia.ValueField();

				}
			}
			if (customerObj.Extern.Addresses?.Count() > 0)
			{
				var addressStatus = SelectStatusChildren(customerObj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address)?.ToList();
				// Get from all available address in BC  which is not marked as deleted
				CustomerAddressData addressObj = customerObj.Extern.Addresses.FirstOrDefault(x => ValidDefaultAddress(x, addressStatus));
				contactImpl.Phone1 = addressObj.Phone.ValueField();
				//Address
				addressImpl.AddressLine1 = addressObj.Address1.ValueField();
				addressImpl.AddressLine2 = addressObj.Address2.ValueField();
				addressImpl.City = addressObj.City.ValueField();
				addressImpl.Country = addressObj.CountryCode.ValueField();
				addressImpl.DefaultId = addressObj.Id.ValueField(); // to use this address to set as default
				if (!string.IsNullOrEmpty(addressObj.State))
				{
					addressImpl.State = States?.FirstOrDefault(x => x.State == addressObj.State)?.StateID?.ValueField() ?? addressObj.State.ValueField();
				}
				else
					addressImpl.State = string.Empty.ValueField();
				addressImpl.PostalCode = addressObj.PostalCode?.ToUpperInvariant()?.ValueField();
				if (isLocationActive)
				{
					List<PXResult<Location, PX.Objects.CR.Address, PX.Objects.CR.Contact, BAccount>> pXResult = null;
					if (existing != null && addressStatus?.Count() == 0)// merge locations for clearsync or if no sync status hystory exists for existing customer /Locations
					{
						pXResult = PXSelectJoin<Location, InnerJoin<PX.Objects.CR.Address, On<Location.defAddressID, Equal<PX.Objects.CR.Address.addressID>>,
						   InnerJoin<PX.Objects.CR.Contact, On<Location.defContactID, Equal<PX.Objects.CR.Contact.contactID>>,
						   InnerJoin<BAccount, On<Location.bAccountID, Equal<BAccount.bAccountID>>>>>
						   , Where<BAccount.noteID, Equal<Required<BAccount.noteID>>>>.Select(this, existing?.LocalID).
						   Cast<PXResult<Location, PX.Objects.CR.Address, PX.Objects.CR.Contact, BAccount>>().ToList();
					}
					foreach (CustomerAddressData externLocation in customerObj.Extern.Addresses)
					{
						MappedLocation mappedLocation = new MappedLocation(externLocation, new object[] { customerObj.ExternID, externLocation.Id }.KeyCombine(), externLocation.CalculateHash(), customerObj.SyncID);

						mappedLocation.Local = MapLocationImport(externLocation, customerObj);
						if (addressObj.Id == externLocation.Id && !addressStatus.Any(y => y.ExternID.KeySplit(1) == externLocation.Id.ToString()))
							mappedLocation.Local.ShipVia = shipVia;
						locationProcessor.RemapBucketImport(new BCLocationEntityBucket() { Address = mappedLocation }, null);
						if (pXResult != null)
							mappedLocation.LocalID = CheckIfExists(mappedLocation.Extern, pXResult, bucket.CustomerAddresses);

						bucket.CustomerAddresses.Add(mappedLocation);
					}

				}
			}
			else if (existing == null)
			{
				bucket.ConnectorGeneratedAddress = new CustomerLocation();
				bucket.ConnectorGeneratedAddress.ShipVia = shipVia;
				bucket.ConnectorGeneratedAddress.ContactOverride = true.ValueField();
				bucket.ConnectorGeneratedAddress.AddressOverride = true.ValueField();
			}
		}

		public override void SaveBucketImport(BCCustomerEntityBucket bucket, IMappedEntity existing, string operation)
		{
			MappedCustomer obj = bucket.Customer;
			// Create/update customer with main/address
			Core.API.Customer impl = cbapi.Put<Core.API.Customer>(obj.Local, obj.LocalID);
			var newDefault = obj.Local.MainContact.Address.DefaultId?.Value;

			obj.AddLocal(impl, impl.SyncID, impl.SyncTime);
			UpdateStatus(obj, operation);

			if (bucket.ConnectorGeneratedAddress != null)
			{
				PX.Objects.CR.Location location = PXSelectJoin<PX.Objects.CR.Location,
			InnerJoin<PX.Objects.CR.BAccount, On<PX.Objects.CR.Location.locationID, Equal<PX.Objects.CR.BAccount.defLocationID>>>,
			Where<PX.Objects.CR.BAccount.noteID, Equal<Required<PX.Objects.CR.BAccount.noteID>>>>.Select(this, impl.SyncID);

				CustomerLocation addressImpl = cbapi.Put<CustomerLocation>(bucket.ConnectorGeneratedAddress, location.NoteID);
			}

			if (isLocationActive)
			{
				CustomerMaint graph;
				List<Location> locations;
				graph = PXGraph.CreateInstance<PX.Objects.AR.CustomerMaint>();
				graph.BAccount.Current = PXSelect<PX.Objects.AR.Customer, Where<PX.Objects.AR.Customer.acctCD,
										 Equal<Required<PX.Objects.AR.Customer.acctCD>>>>.Select(graph, impl.CustomerID.Value);

				locations = graph.GetExtension<CustomerMaint.LocationDetailsExt>().Locations.Select().RowCast<Location>().ToList();
				//create/update other address and create status line(including Main)
				var alladdresses = SelectStatusChildren(obj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address)?.ToList();
				Location location = graph.GetExtension<CustomerMaint.DefLocationExt>().DefLocation.SelectSingle();
				bool lastmodifiedUpdated = false;

				if (bucket.CustomerAddresses.Count > 0)
				{
					if (alladdresses.Count == 0)// we copy into main if its first location
					{
						var main = bucket.CustomerAddresses.FirstOrDefault();

						if (location != null && main.LocalID == null)
						{
							main.LocalID = location.NoteID; //if location already created
						}
					}
					foreach (var loc in bucket.CustomerAddresses)
					{
						try
						{
							loc.Local.Customer = impl.CustomerID;
							var status = EnsureStatus(loc, SyncDirection.Import, persist: false);
							if (loc.LocalID == null)
							{
								loc.Local.ShipVia = location.CCarrierID.ValueField();
							}
							else if (loc.LocalID != null && !locations.Any(x => x.NoteID == loc.LocalID)) continue; // means deletd location
							if (status == EntityStatus.Pending || Operation.SyncMethod == SyncMode.Force)
							{
								lastmodifiedUpdated = true;
								CustomerLocation addressImpl = cbapi.Put<CustomerLocation>(loc.Local, loc.LocalID);

								loc.AddLocal(addressImpl, addressImpl.SyncID, addressImpl.SyncTime);
								UpdateStatus(loc, operation);
							}
						}
						catch (Exception ex)
						{
							LogError(Operation.LogScope(loc), ex);
							UpdateStatus(loc, BCSyncOperationAttribute.LocalFailed, message: ex.Message);
						}

					}
				}

				DateTime? date = null;

				if (lastmodifiedUpdated || UpdateDefault(bucket, newDefault, locations, graph, location))
				{
					List<PXDataField> fields = new List<PXDataField>();
					fields.Add(new PXDataField(nameof(BAccount.lastModifiedDateTime)));
					fields.Add(new PXDataFieldValue(nameof(BAccount.acctCD), impl.CustomerID?.Value));
					using (PXDataRecord rec = PXDatabase.SelectSingle(typeof(BAccount), fields.ToArray()))
					{
						if (rec != null)
						{
							date = rec.GetDateTime(0);
							if (date != null)
							{
								date = PXTimeZoneInfo.ConvertTimeFromUtc(date.Value, LocaleInfo.GetTimeZone());

							}
						}
					}
				}
				obj.AddLocal(impl, impl.SyncID, date ?? graph.BAccount.Current.LastModifiedDateTime);
				UpdateStatus(obj, operation);
			}
		}

		protected virtual bool IsLocationModified(int? syncID, IList<CustomerAddressData> addresses, List<BCSyncStatus> addressReference = null)
		{
			bool changed = false;
			if (addressReference == null)
				addressReference = SelectStatusChildren(syncID).Where(s => s.EntityType == BCEntitiesAttribute.Address && s.Deleted == false && s.PendingSync == false)?.ToList();

			if (addresses != null && addressReference != null)
			{
				if (addressReference?.Count == addresses?.Count)
					foreach (var address in addresses)
					{
						var localObj = addressReference.FirstOrDefault(x => x.ExternID?.KeySplit(1) == address.Id.ToString());
						if (localObj == null) { changed = true; break; }
						if (localObj.ExternHash != address.CalculateHash()) { changed = true; break; }
					}
				else
					changed = true;
			}
			else
				changed = true;
			return changed;
		}

		protected override void ProcessMappedListForImport(ref List<IMappedEntity> mappedList)
		{
			var externIDs = mappedList.Select(x => x.ExternID).ToArray();

			List<BCSyncStatus> bcSyncStatusList = GetBCSyncStatusResult(mappedList.FirstOrDefault()?.EntityType, null, null, externIDs).Select(x => x.GetItem<BCSyncStatus>()).ToList();

			var syncIds = bcSyncStatusList.Select(x => x.SyncID)?.ToArray();
			IEnumerable<BCSyncStatus> addressReference = null;
			if (syncIds != null)
				addressReference = SelectStatusChildren(syncIds, BCEntitiesAttribute.Address);

			foreach (MappedCustomer oneMapped in mappedList)
			{
				EntityStatus status = EnsureStatusBulk(bcSyncStatusList, oneMapped, SyncDirection.Import);
				if (status == EntityStatus.Syncronized && isLocationActive)
				{
					var addressStatus = addressReference?.Where(s => s.ParentSyncID == oneMapped.SyncID && s.Deleted == false && s.PendingSync == false)?.ToList();
					bool changed = IsLocationModified(oneMapped.SyncID, oneMapped.Extern?.Addresses, addressStatus);
					if (changed)
					{
						oneMapped.ExternTimeStamp = DateTime.MaxValue;
						EnsureStatusBulk(bcSyncStatusList, oneMapped, SyncDirection.Import, false);
					}
				}
			}
			mappedList.Clear();
		}

		protected Guid? CheckIfExists(CustomerAddressData custAddr, List<PXResult<Location, PX.Objects.CR.Address, PX.Objects.CR.Contact, BAccount>> pXResults, List<MappedLocation> mappedLocations)
		{
			foreach (PXResult<Location, PX.Objects.CR.Address, PX.Objects.CR.Contact, BAccount> loc in pXResults)
			{
				Location location = loc.GetItem<Location>();
				PX.Objects.CR.Address address = loc.GetItem<PX.Objects.CR.Address>();
				PX.Objects.CR.Contact contact = loc.GetItem<PX.Objects.CR.Contact>();
				string stateID = null;
				if (!string.IsNullOrEmpty(custAddr.State))
				{
					stateID = States?.FirstOrDefault(x => x.State == custAddr.State)?.StateID ?? custAddr.State;
				}
				else
					stateID = string.Empty;
				string fullName = new String[] { contact.Attention, location.LocationCD }.FirstNotEmpty();
				if (CompareLocation(custAddr, location, address, contact, stateID, fullName))
				{
					if (!mappedLocations.Any(x => x.LocalID == location.NoteID))// as BC can have multiple same address 
						return location.NoteID;
				}
			}
			return null;
		}

		protected bool CompareLocation(CustomerAddressData custAddr, Location location, PX.Objects.CR.Address address, PX.Objects.CR.Contact contact, string stateID, string fullName)
		{
			return custAddr.City == address.City
											&& (string.IsNullOrEmpty(custAddr.Company) || custAddr.Company.Trim() == contact.FullName?.Trim() || custAddr.Company == location.LocationCD?.Trim())
											&& custAddr.CountryCode?.Trim() == address.CountryID?.Trim()
											&& custAddr.FirstName?.Trim() == (contact.FirstName ?? fullName.FieldsSplit(0, fullName))?.Trim()
											&& custAddr.LastName?.Trim() == (contact.LastName ?? fullName.FieldsSplit(1, fullName))?.Trim()
											&& (string.IsNullOrEmpty(custAddr.Phone) || custAddr.Phone?.Trim() == contact.Phone1?.Trim())
											&& stateID == address.State?.Trim()
											&& custAddr.Address1?.Trim() == address.AddressLine1?.Trim()
											&& custAddr.Address2?.Trim() == address.AddressLine2?.Trim()
											&& custAddr.PostalCode?.Trim()?.ToUpperInvariant() == address.PostalCode?.Trim();
		}

		protected bool UpdateDefault(BCCustomerEntityBucket bucket, int? newDefault, List<Location> locations, CustomerMaint graph, Location currentDefault)
		{
			var obj = bucket.Customer;
			if (obj.LocalID != null)
			{
				var addressReferences = SelectStatusChildren(obj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address && s.Deleted == false)?.ToList();
				var deletedValues = addressReferences?.Where(x => bucket.CustomerAddresses.All(y => x.ExternID != y.ExternID)).ToList();
				var mappeddefault = bucket.CustomerAddresses.FirstOrDefault(x => x.Extern.Id == newDefault); // get new default mapped address
				var nextdefault = locations.FirstOrDefault(x => x.NoteID == mappeddefault?.LocalID);
				var defLocationExt = graph.GetExtension<CustomerMaint.DefLocationExt>();
				var locationDetails = graph.GetExtension<CustomerMaint.LocationDetailsExt>();
				//mark status for address as deleted for addresses deleted at BC
				if (deletedValues != null && deletedValues.Count > 0)
				{

					foreach (var value in deletedValues)
					{
						DeleteStatus(value, BCSyncOperationAttribute.NotFound);

						var location = locations.FirstOrDefault(x => x.NoteID == value.LocalID);
						if (location != null)
						{
							if (location.LocationID == graph.BAccount.Current.DefLocationID) //is default
							{
								if (nextdefault == null) continue;

								defLocationExt.DefLocation.Current = locationDetails.Locations.Select().RowCast<Location>()?
														.ToList()?.FirstOrDefault(x => x.LocationID == nextdefault.LocationID);

								defLocationExt.SetDefaultLocation.Press();

								graph.Actions.PressSave();
							}
							location.IsActive = false;
							defLocationExt.DefLocation.Update(location);
						}

					}
					graph.Actions.PressSave();
					return true;

				}

				else if (graph.BAccount.Current.DefLocationID != nextdefault?.LocationID && nextdefault != null)//if mapped default and deflocation are not in sync
				{
					defLocationExt.DefLocation.Current = locationDetails.Locations.Select().RowCast<Location>()?.ToList()?.FirstOrDefault(x => x.LocationID == nextdefault.LocationID); ;
					defLocationExt.SetDefaultLocation.Press();
					graph.Actions.PressSave();
					currentDefault.IsActive = false;
					defLocationExt.DefLocation.Update(currentDefault);
					graph.Actions.PressSave();
					return true;
				}
			}
			return false;
		}

		protected bool ValidDefaultAddress(CustomerAddressData x, List<BCSyncStatus> addressStatus)
		{
			if (!addressStatus.Any(y => y.ExternID.KeySplit(1) == x.Id.ToString())) return true;
			else if (addressStatus.Any(y => y.ExternID.KeySplit(1) == x.Id.ToString() && y.Deleted == false)) return true;
			return true;
		}
		#endregion

		#region Export
		public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			IEnumerable<Core.API.Customer> impls = cbapi.GetAll<Core.API.Customer>(new Core.API.Customer { CustomerID = new StringReturn() },
				minDateTime, maxDateTime, filters, GetCustomFieldsForExport());

			int countNum = 0;
			List<IMappedEntity> mappedList = new List<IMappedEntity>();
			foreach (Core.API.Customer impl in impls)
			{
				IMappedEntity obj = new MappedCustomer(impl, impl.SyncID, impl.SyncTime);

				mappedList.Add(obj);
				countNum++;
				if (countNum % BatchFetchCount == 0)
				{
					ProcessMappedListForExport(ref mappedList);
				}
			}
			if (mappedList.Any())
			{
				ProcessMappedListForExport(ref mappedList);
			}
		}

		public override EntityStatus GetBucketForExport(BCCustomerEntityBucket bucket, BCSyncStatus bcstatus)
		{
			Core.API.Customer impl = cbapi.GetByID<Core.API.Customer>(bcstatus.LocalID, GetCustomFieldsForExport());
			if (impl == null) return EntityStatus.None;

			MappedCustomer obj = bucket.Customer = bucket.Customer.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(bucket.Customer, SyncDirection.Export);
			if (!string.IsNullOrEmpty(impl.PriceClassID?.Value) && GetEntity(BCEntitiesAttribute.CustomerPriceClass)?.IsActive == true)
			{
				BCSyncStatus priceCLassStatus = PXSelectJoin<BCSyncStatus,
				LeftJoin<PX.Objects.AR.ARPriceClass, On<BCSyncStatus.localID, Equal<PX.Objects.AR.ARPriceClass.noteID>>>,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
					And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
					And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
					And<PX.Objects.AR.ARPriceClass.priceClassID, Equal<Required<PX.Objects.AR.ARPriceClass.priceClassID>>>>>>>.Select(this, BCEntitiesAttribute.CustomerPriceClass, impl.PriceClassID?.Value);
				if (priceCLassStatus?.ExternID == null)
				{
					CustomerPriceClass priceClass = cbapi.Get<CustomerPriceClass>(new CustomerPriceClass() { PriceClassID = impl.PriceClassID });
					if (priceClass != null)
					{
						MappedGroup priceClassobj = bucket.CustomerPriceClass = bucket.CustomerPriceClass.Set(priceClass, priceClass.SyncID, priceClass.SyncTime);
						EntityStatus priceClassStatus = EnsureStatus(priceClassobj, SyncDirection.Export);
					}
				}
			}

			return status;
		}

		public override void MapBucketExport(BCCustomerEntityBucket bucket, IMappedEntity existing)
		{
			MappedCustomer customerObj = bucket.Customer;

			Core.API.Customer customerImpl = customerObj.Local;
			Core.API.Contact contactImpl = customerImpl.MainContact;
			Core.API.Address addressImpl = contactImpl.Address;
			CustomerData customerData = customerObj.Extern = new CustomerData();

			//Customer
			customerData.Id = customerObj.ExternID.ToInt();
			customerData.Company = contactImpl.FullName?.Value ?? customerImpl.CustomerName?.Value;

			//Contact			
			string fullName = new String[] { contactImpl.Attention?.Value, customerImpl.CustomerName?.Value }.FirstNotEmpty();
			customerData.FirstName = fullName.FieldsSplit(0, fullName);
			customerData.LastName = fullName.FieldsSplit(1, fullName);
			customerData.Email = contactImpl.Email?.Value;
			customerData.Phone = contactImpl.Phone2?.Value ?? contactImpl.Phone1?.Value;

			if (!string.IsNullOrEmpty(customerImpl.PriceClassID?.Value))
			{
				BCSyncStatus status = PXSelectJoin<BCSyncStatus,
					LeftJoin<PX.Objects.AR.ARPriceClass, On<BCSyncStatus.localID, Equal<PX.Objects.AR.ARPriceClass.noteID>>>,
					Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<PX.Objects.AR.ARPriceClass.priceClassID, Equal<Required<PX.Objects.AR.ARPriceClass.priceClassID>>>>>>>.Select(this, BCEntitiesAttribute.CustomerPriceClass, customerImpl.PriceClassID?.Value);
				if (GetEntity(BCEntitiesAttribute.CustomerPriceClass)?.IsActive == true)
				{
					if (status?.ExternID == null) throw new PXException(BCMessages.PriceClassNotSyncronizedForItem, customerImpl.PriceClassID?.Value);
				}
				customerData.CustomerGroupId = status?.ExternID?.ToInt();
			}
			else
				customerData.CustomerGroupId = 0;

			//Address
			if (isLocationActive)
			{
				bucket.CustomerAddresses = new List<MappedLocation>();
				var addressReference = SelectStatusChildren(customerObj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address)?.ToList();

				var customerLocations = cbapi.GetAll<CustomerLocation>(new CustomerLocation()
				{
					Customer = new StringSearch() { Value = customerObj.Local.CustomerID.Value },
					LocationName = new StringReturn(),
					ReturnBehavior = ReturnBehavior.OnlySpecified,
					LocationContact = new Core.API.Contact()
					{
						ReturnBehavior = ReturnBehavior.OnlySpecified,
						Phone1 = new StringReturn(),
						Phone2 = new StringReturn(),
						FullName = new StringReturn(),
						Attention = new StringReturn(),
						Address = new Core.API.Address() { ReturnBehavior = ReturnBehavior.All }
					},
				}, mappedCustomFields: locationProcessor.GetCustomFieldsForExport());

				foreach (CustomerLocation customerLocation in customerLocations)
				{
					var mapped = addressReference.FirstOrDefault(x => x.LocalID == customerLocation.NoteID.Value);
					if (mapped == null || (mapped != null && mapped.Deleted == false))
					{
						if (mapped == null && customerLocation.LocationContact.Address.City?.Value == null && customerLocation.LocationContact.Address.AddressLine1?.Value == null
							 && customerLocation.LocationContact.Address.AddressLine2?.Value == null && customerLocation.LocationContact.Address.PostalCode?.Value == null)
							continue;// connector generated address
						MappedLocation mappedLocation = new MappedLocation(customerLocation, customerLocation.NoteID.Value, customerLocation.LastModifiedDateTime.Value, customerObj.SyncID);
						mappedLocation.Extern = MapLocationExport(mappedLocation, customerObj.Local.CustomerName?.Value);
						if (addressReference?.Count == 0 && existing != null)
						{
							mappedLocation.ExternID = CheckIfExists(mappedLocation.Local, existing, bucket.CustomerAddresses);
						}
						locationProcessor.RemapBucketExport(new BCLocationEntityBucket() { Address = mappedLocation }, null);
						bucket.CustomerAddresses.Add(mappedLocation);
					}

				}
			}
		}
		protected string CheckIfExists(CustomerLocation custAddr, IMappedEntity existing, List<MappedLocation> mappedLocations)
		{
			CustomerData data = (CustomerData)existing.Extern;
			foreach (CustomerAddressData address in data.Addresses)
			{
				string stateID = null;
				if (!string.IsNullOrEmpty(address.State))
				{
					stateID = States?.FirstOrDefault(x => x.State == address.State)?.StateID ?? address.State;
				}
				else
					stateID = string.Empty;
				string fullName = new String[] { custAddr.LocationContact.Attention?.Value, custAddr.LocationName?.Value }.FirstNotEmpty();
				if (CompareLocation(custAddr, address, stateID, fullName))
				{
					var id = new object[] { data.Id, address.Id }.KeyCombine();
					if (!mappedLocations.Any(x => x.ExternID == id))// as BC can have multiple same address 
						return id;
				}
			}
			return null;
		}

		protected bool CompareLocation(CustomerLocation custAddr, CustomerAddressData address, string stateID, string fullName)
		{
			return address.City == custAddr.LocationContact?.Address?.City?.Value
											&& (address.Company?.Trim() == custAddr.LocationContact?.FullName?.Value?.Trim() || address.Company == custAddr.LocationName?.Value?.Trim())
											&& (address.CountryCode?.Trim() == custAddr.LocationContact?.Address?.Country?.Value?.Trim() || address.Country?.Trim() == custAddr.LocationContact?.Address?.Country?.Value?.Trim())
											&& address.FirstName?.Trim() == fullName.FieldsSplit(0, fullName)?.Trim()
											&& address.LastName?.Trim() == fullName.FieldsSplit(1, fullName)?.Trim()
											&& (address.Phone?.Trim() == custAddr.LocationContact?.Phone1?.Value?.Trim() || address.Phone?.Trim() == custAddr.LocationContact?.Phone2?.Value?.Trim())
											&& stateID == custAddr.LocationContact?.Address?.State?.Value?.Trim()
											&& address.Address1?.Trim() == custAddr.LocationContact?.Address?.AddressLine1?.Value?.Trim()
											&& address.Address2?.Trim() == custAddr.LocationContact?.Address?.AddressLine2?.Value?.Trim()
											&& address.PostalCode?.Trim()?.ToUpperInvariant() == custAddr.LocationContact?.Address?.PostalCode?.Value?.Trim();
		}

		public override object GetExternCustomField(BCCustomerEntityBucket bucket, string viewName, string fieldName)
		{
			MappedCustomer obj = bucket.Customer;
			Core.API.Customer impl = obj.Local;
			//Get the Customer Form Fields
			if (viewName.EndsWith(BCConstants.FormFields) && !string.IsNullOrWhiteSpace(fieldName))
			{
				IList<CustomerFormFieldData> customerFormFields = obj.Extern.FormFields;
				if (customerFormFields?.Count > 0)
				{
					var formField = customerFormFields.Where(x => x.Name.Equals(fieldName)).FirstOrDefault();
					var formFieldType = formFieldsList.Where(x => x.Item1 == Operation.EntityType && x.Item2 == formField?.Name).FirstOrDefault()?.Item3;
					if (formFieldType == BCCustomerFormFieldsAttribute.JArray)
					{
						var fieldValues = formField?.Value as Newtonsoft.Json.Linq.JArray;
						if (fieldValues == null || fieldValues.Count == 0) return null;
						return string.Join(",", fieldValues.ToObject<string[]>());
					}
					return formField?.Value;
				}
			}
			return null;
		}
		public override object GetAttribute(BCCustomerEntityBucket bucket, string attributeID)
		{
			MappedCustomer obj = bucket.Customer;
			Core.API.Customer impl = obj.Local;
			return impl.Attributes?.Where(x => string.Equals(x?.Attribute?.Value, attributeID, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
		public override void AddAttributeValue(BCCustomerEntityBucket bucket, string attributeID, object attributeValue)
		{
			MappedCustomer obj = bucket.Customer;
			Core.API.Customer impl = obj.Local;
			impl.Attributes = impl.Attributes ?? new List<AttributeDetail>();
			AttributeDetail attributeDetail = new AttributeDetail();
			attributeDetail.Attribute = new StringValue() { Value = attributeID };
			attributeDetail.Value = new StringValue() { Value = attributeValue?.ToString() };
			impl.Attributes.Add(attributeDetail);
		}

		public override void SaveBucketExport(BCCustomerEntityBucket bucket, IMappedEntity existing, string operation)
		{
			MappedCustomer obj = bucket.Customer;

			//Customer
			CustomerData customerData = null;
			IList<CustomerFormFieldData> formFields = obj.Extern.FormFields;
			//If customer form fields are not empty, it could cause an error		
			obj.Extern.FormFields = null;
			if (obj.ExternID == null || existing == null)
				customerData = customerDataProviderV3.Create(obj.Extern);
			else
				customerData = customerDataProviderV3.Update(obj.Extern);
			if (formFields?.Count > 0 && customerData.Id != null)
			{
				formFields.All(x => { x.CustomerId = customerData.Id; x.AddressId = null; x.Value = x.Value ?? string.Empty; return true; });
				customerData.FormFields = customerFormFieldRestDataProvider.UpdateAll((List<CustomerFormFieldData>)formFields);
			}
			obj.AddExtern(customerData, customerData.Id?.ToString(), customerData.DateModifiedUT.ToDate());

			UpdateStatus(obj, operation);

			#region Update ExternalRef
			string externalRef = APIHelper.ReferenceMake(customerData.Id?.ToString(), GetBinding().BindingName);

			string[] keys = obj.Local?.AccountRef?.Value?.Split(';');
			if (keys?.Contains(externalRef) != true)
			{
				if (!string.IsNullOrEmpty(obj.Local?.AccountRef?.Value))
					externalRef = new object[] { obj.Local?.AccountRef?.Value, externalRef }.KeyCombine();

				if (externalRef.Length < 50 && obj.Local.SyncID != null)
					PXDatabase.Update<BAccount>(
								  new PXDataFieldAssign(typeof(BAccount.acctReferenceNbr).Name, PXDbType.NVarChar, externalRef),
								  new PXDataFieldRestrict(typeof(BAccount.noteID).Name, PXDbType.UniqueIdentifier, obj.Local.NoteID?.Value)
								  );
			}
			#endregion

			//Address
			if (isLocationActive)
			{
				foreach (var address in bucket.CustomerAddresses)
				{
					try
					{
						address.Extern.CustomerId = customerData.Id;
						if (address.Local?.Active?.Value == false)
						{
							BCSyncStatus bCSyncStatus = PXSelect<BCSyncStatus,
									Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
									And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
									And<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>,
									And<BCSyncStatus.localID, Equal<Required<BCSyncStatus.localID>>>>>>>.Select(this, BCEntitiesAttribute.Address, address.LocalID);
							if (bCSyncStatus == null) continue; //New inactive location should not be synced
						}
						var status = EnsureStatus(address, SyncDirection.Export, persist: false);

						if (status == EntityStatus.Pending || Operation.SyncMethod == SyncMode.Force)
						{
							CustomerAddressData addressData = null;
							formFields = address.Extern.FormFields;
							address.Extern.FormFields = null;
							string locOperation;
							if (address.ExternID == null || existing == null)
							{
								addressData = customerAddressDataProviderV3.Create(address.Extern);
								locOperation = BCSyncOperationAttribute.ExternInsert;
							}
							else
							{
								address.Extern.Id = address.ExternID.KeySplit(1).ToInt();
								addressData = customerAddressDataProviderV3.Update(address.Extern);
								locOperation = BCSyncOperationAttribute.ExternUpdate;
							}
							if (formFields?.Count > 0 && addressData.Id != null)
							{
								formFields.All(x => { x.AddressId = addressData.Id; x.CustomerId = null; return true; });
								addressData.FormFields = customerFormFieldRestDataProvider.UpdateAll((List<CustomerFormFieldData>)formFields);
							}
							addressData.FormFields = addressData.FormFields == null ? new List<CustomerFormFieldData>() : addressData.FormFields;
							address.AddExtern(addressData, new object[] { customerData.Id, addressData.Id }.KeyCombine(), addressData.CalculateHash());
							address.ParentID = obj.SyncID;

							UpdateStatus(address, locOperation);
						}
					}
					catch (Exception ex)
					{

						LogError(Operation.LogScope(address), ex);
						UpdateStatus(address, BCSyncOperationAttribute.ExternFailed, message: ex.Message); 
					}
				}

				var addressReferences = SelectStatusChildren(obj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address && s.Deleted == false)?.ToList();
				var deletedValues = addressReferences?.Where(x => bucket.CustomerAddresses.All(y => x.LocalID != y.LocalID)).ToList();

				if (deletedValues != null && deletedValues.Count > 0)
				{
					foreach (var value in deletedValues)
					{
						DeleteStatus(value, BCSyncOperationAttribute.NotFound);
					}
				}
			}

		}

		#endregion
	}
}
