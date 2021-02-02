using PX.Api.ContractBased.Models;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Shopify.API.REST;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;
using Location = PX.Objects.CR.Standalone.Location;

namespace PX.Commerce.Shopify
{
    public class SPCustomerEntityBucket : EntityBucketBase, IEntityBucket
    {
        public IMappedEntity Primary { get => Customer; }
        public IMappedEntity[] Entities => new IMappedEntity[] { Customer };
        public CustomerLocation ConnectorGeneratedAddress;
        public MappedCustomer Customer;
        public List<MappedLocation> CustomerAddresses;
        public MappedLocation CustomerAddress;

    }

    public class SPCustomerRestrictor : BCBaseRestrictor, IRestrictor
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

    [BCProcessor(typeof(SPConnector), BCEntitiesAttribute.Customer, BCCaptions.Customer,
        IsInternal = false,
        Direction = SyncDirection.Bidirect,
        PrimaryDirection = SyncDirection.Import,
        PrimarySystem = PrimarySystem.Extern,
        PrimaryGraph = typeof(PX.Objects.AR.CustomerMaint),
        ExternTypes = new Type[] { typeof(CustomerData) },
        LocalTypes = new Type[] { typeof(Customer) },
        AcumaticaPrimaryType = typeof(PX.Objects.AR.Customer),
        AcumaticaPrimarySelect = typeof(PX.Objects.AR.Customer.acctCD),
        URL = "customers/{0}"

    )]
    [BCProcessorRealtime(PushSupported = true, HookSupported = true,
        PushSources = new String[] { "BC-PUSH-Customers", "BC-PUSH-Locations" },
        WebHookType = typeof(WebHookMessage),
        WebHooks = new String[]
        {
            "customers/create",
            "customers/update",
            "customers/delete",
            "customers/disable",
            "customers/enable"
        })]
    public class SPCustomerProcessor : SPLocationBaseProcessor<SPCustomerProcessor, SPCustomerEntityBucket, MappedCustomer>, IProcessor
    {
        bool isLocationActive;

        #region Initialization
        public override void Initialise(IConnector iconnector, ConnectorOperation operation)
        {
            base.Initialise(iconnector, operation);
            isLocationActive = ConnectorHelper.GetConnectorBinding(operation.ConnectorType, operation.Binding).ActiveEntities.Any(x => x == BCEntitiesAttribute.Address);
            if (isLocationActive)
            {
                locationProcessor = PXGraph.CreateInstance<SPLocationProcessor>();
                ((SPLocationProcessor)locationProcessor).Initialise(iconnector, operation.Clone().With(_ => { _.EntityType = BCEntitiesAttribute.Address; return _; }));
            }
        }
        #endregion

        #region Common
        public override MappedCustomer PullEntity(Guid? localID, Dictionary<string, object> externalInfo)
        {
            Customer impl = cbapi.GetByID<Customer>(localID);
            if (impl == null) return null;

            MappedCustomer obj = new MappedCustomer(impl, impl.SyncID, impl.SyncTime);

            return obj;
        }

        //TODO
        public override MappedCustomer PullEntity(String externID, String externalInfo)
        {
            CustomerData data = customerDataProvider.GetByID(externID);
            if (data == null) return null;

            MappedCustomer obj = new MappedCustomer(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));
            return obj;
        }

        public override IEnumerable<MappedCustomer> PullSimilar(IExternEntity entity, out String uniqueField)
        {
            uniqueField = ((CustomerData)entity)?.Email;
            Customer[] impls = null;
            if (uniqueField != null)
            {
                impls = cbapi.GetAll<Customer>(new Customer() { MainContact = new Core.API.Contact() { Email = uniqueField.SearchField() } },
                    filters: GetFilter(Operation.EntityType).LocalFiltersRows.Cast<PXFilterRow>().ToArray());
            }
            else if (uniqueField == null && !string.IsNullOrWhiteSpace(((CustomerData)entity)?.Phone))
            {
                uniqueField = ((CustomerData)entity)?.Phone;
                impls = cbapi.GetAll<Customer>(new Customer() { MainContact = new Core.API.Contact() { Phone1 = uniqueField.SearchField() } },
                    filters: GetFilter(Operation.EntityType).LocalFiltersRows.Cast<PXFilterRow>().ToArray());
            }
            else
                return null;
            if (impls == null) return null;

            return impls.Select(impl => new MappedCustomer(impl, impl.SyncID, impl.SyncTime));
        }
        public override IEnumerable<MappedCustomer> PullSimilar(ILocalEntity entity, out String uniqueField)
        {
            uniqueField = ((Customer)entity)?.MainContact?.Email?.Value;
            string queryField = string.Empty;

            if (uniqueField != null)
            {
                queryField = nameof(CustomerData.Email);
            }
            else if (uniqueField == null && !string.IsNullOrWhiteSpace(((Customer)entity)?.MainContact?.Phone1?.Value))
            {
                queryField = nameof(CustomerData.Phone);
                uniqueField = ((Customer)entity)?.MainContact?.Email?.Value;
            }
            else
                return null;
            IEnumerable<CustomerData> datas = customerDataProvider.GetByQuery(queryField, uniqueField);
            if (datas == null) return null;

            return datas.Select(data => new MappedCustomer(data, data.Id.ToString(), data.DateModifiedAt.ToDate(false)));
        }

		#endregion

		#region Import
		public override void FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
		{
			FilterCustomers filter = new FilterCustomers();
			if (minDateTime != null)
				filter.UpdatedAtMin = minDateTime.Value.ToLocalTime().AddSeconds(-GetBindingExt<BCBindingShopify>().ApiDelaySeconds ?? 0);
			if (maxDateTime != null)
				filter.UpdatedAtMax = maxDateTime.Value.ToLocalTime();

            IEnumerable<CustomerData> datas = customerDataProvider.GetAll(filter);

            int countNum = 0;
            List<IMappedEntity> mappedList = new List<IMappedEntity>();
            foreach (CustomerData data in datas)
            {
                IMappedEntity obj = new MappedCustomer(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));

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
        public override EntityStatus GetBucketForImport(SPCustomerEntityBucket bucket, BCSyncStatus bcstatus)
        {
            CustomerData data = customerDataProvider.GetByID(bcstatus.ExternID);

            MappedCustomer obj = bucket.Customer = bucket.Customer.Set(data, data.Id?.ToString(), data.DateModifiedAt.ToDate(false));
            EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

            return status;
        }

        public override void MapBucketImport(SPCustomerEntityBucket bucket, IMappedEntity existing)
        {
            MappedCustomer customerObj = bucket.Customer;
            Customer customerImpl = customerObj.Local = new Customer();
            customerImpl.Custom = GetCustomFieldsForImport();

            //Existing
            PX.Objects.AR.Customer customer = PXSelect<PX.Objects.AR.Customer, Where<PX.Objects.AR.Customer.noteID, Equal<Required<PX.Objects.AR.Customer.noteID>>>>.Select(this, customerObj.LocalID);

            //General Info
            string firstLastName = (customerObj.Extern.FirstName?.Equals(customerObj.Extern.LastName, StringComparison.InvariantCultureIgnoreCase)) == false ?
                (String.Concat(customerObj.Extern.FirstName ?? string.Empty, " ", customerObj.Extern.LastName ?? string.Empty)).Trim() : customerObj.Extern.FirstName ?? string.Empty;
            customerImpl.AccountRef = APIHelper.ReferenceMake(customerObj.Extern.Id, currentBinding.BindingName).ValueField();
            customerImpl.CurrencyID = customerObj.Extern.Currency.ValueField();
            customerImpl.EnableCurrencyOverride = true.ValueField();
            customerImpl.CustomerClass = customerObj.LocalID == null || existing?.Local == null ? GetBindingExt<BCBindingExt>().CustomerClassID?.ValueField() : null;
            customerImpl.PriceClassID = new StringValue() { Value = null };
        
            //Main Contact
            Core.API.Contact contactImpl = customerImpl.MainContact = new Core.API.Contact();
            bool noEmail = string.IsNullOrWhiteSpace(customerObj.Extern.Email);

            contactImpl.FirstName = customerObj.Extern.FirstName.ValueField();
            contactImpl.LastName = customerObj.Extern.LastName.ValueField();
            contactImpl.Attention = firstLastName.ValueField();
            contactImpl.Email = customerObj.Extern.Email.ValueField();
            contactImpl.Phone1 = customerObj.Extern.Phone.ValueField();
            contactImpl.Note = customerObj.Extern.Note;
            contactImpl.Active = true.ValueField();
            contactImpl.DoNotEmail = (noEmail ? false : true).ValueField();
            customerImpl.SendDunningLettersbyEmail = (noEmail ? false : true).ValueField();
            customerImpl.SendInvoicesbyEmail = false.ValueField();
            customerImpl.SendStatementsbyEmail = (noEmail ? false : true).ValueField();
            customerImpl.PrintDunningLetters = (noEmail ? true : false).ValueField();

            //Address
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
            CustomerAddressData addressObj = null;
            if (customerObj.Extern.Addresses?.Count() > 0)
            {

                addressObj = customerObj.Extern.Addresses.FirstOrDefault(x => x.Default == true);
                if (string.IsNullOrWhiteSpace(customerObj.Extern.Phone) && !string.IsNullOrWhiteSpace(addressObj.Phone))
                {
                    contactImpl.Phone1 = addressObj.Phone.ValueField();
                    contactImpl.Phone2 = string.Empty.ValueField();
                }
                else if (!string.IsNullOrWhiteSpace(addressObj.Phone) && !addressObj.Phone.Equals(customerObj.Extern.Phone, StringComparison.InvariantCultureIgnoreCase))
                {
                    contactImpl.Phone1 = customerObj.Extern.Phone.ValueField();
                    contactImpl.Phone2 = addressObj.Phone.ValueField();
                }
                else if (string.IsNullOrWhiteSpace(customerObj.Extern.Phone) && string.IsNullOrWhiteSpace(addressObj.Phone))
                {
                    contactImpl.Phone1 = string.Empty.ValueField();
                    contactImpl.Phone2 = string.Empty.ValueField();
                }
                addressImpl.AddressLine1 = addressObj.Address1.ValueField();
                addressImpl.AddressLine2 = addressObj.Address2.ValueField();
                addressImpl.City = addressObj.City.ValueField();
                addressImpl.Country = addressObj.CountryCode.ValueField();
                if (!string.IsNullOrEmpty(addressObj.ProvinceCode))
                {
                    addressImpl.State = addressObj.ProvinceCode?.ValueField();
                }
                else
                    addressImpl.State = string.Empty.ValueField();
                addressImpl.PostalCode = addressObj.PostalCode?.ToUpperInvariant()?.ValueField();

                if (isLocationActive)
                {
                    var addressStatus = SelectStatusChildren(customerObj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address)?.ToList();

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
                        locationProcessor.RemapBucketImport(new SPLocationEntityBucket() { Address = mappedLocation }, null);
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
            customerImpl.CustomerName = (firstLastName ?? addressObj?.Company ?? customerObj.ExternID.ToString()).ValueField();
            contactImpl.CompanyName = contactImpl.FullName = string.IsNullOrWhiteSpace(addressObj?.Company) ? customerImpl.CustomerName : addressObj?.Company?.ValueField();
        }

        public override void SaveBucketImport(SPCustomerEntityBucket bucket, IMappedEntity existing, String operation)
        {
            MappedCustomer obj = bucket.Customer;

            Customer impl = cbapi.Put<Customer>(obj.Local, obj.LocalID);

            obj.AddLocal(impl, impl.SyncID, impl.SyncTime);
            UpdateStatus(obj, operation);

            Location location = PXSelectJoin<Location,
            InnerJoin<BAccount, On<Location.locationID, Equal<BAccount.defLocationID>>>,
            Where<BAccount.noteID, Equal<Required<PX.Objects.CR.BAccount.noteID>>>>.Select(this, impl.SyncID);

            if (bucket.ConnectorGeneratedAddress != null)
            {
                CustomerLocation addressImpl = cbapi.Put<CustomerLocation>(bucket.ConnectorGeneratedAddress, location.NoteID);
            }
            if (isLocationActive)
            {
                bool lastmodifiedUpdated = false;
                PX.Objects.AR.CustomerMaint graph;
                List<Location> locations;
                graph = PXGraph.CreateInstance<PX.Objects.AR.CustomerMaint>();
                graph.BAccount.Current = PXSelect<PX.Objects.AR.Customer, Where<PX.Objects.AR.Customer.acctCD,
                                         Equal<Required<PX.Objects.AR.Customer.acctCD>>>>.Select(graph, impl.CustomerID.Value);

                locations = graph.GetExtension<PX.Objects.AR.CustomerMaint.LocationDetailsExt>().Locations.Select().RowCast<Location>().ToList();
                //create/update other address and create status line(including Main)
                var alladdresses = SelectStatusChildren(obj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address)?.ToList();

                if (bucket.CustomerAddresses.Count > 0)
                {
                    if (alladdresses.Count == 0)// we copy into main if its first location
                    {
                        var main = bucket.CustomerAddresses.FirstOrDefault(x => x.Extern.Default == true);

                        if (location != null && main.LocalID == null)
                        {
                            main.LocalID = location.NoteID; //if location already created
                        }
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
                DateTime? date = null;

                if (lastmodifiedUpdated || UpdateDefault(bucket, locations, graph, location))
                {
                    //var select = new PXSelect<PX.Objects.AR.Customer, Where<PX.Objects.AR.Customer.acctCD,
                    //						 Equal<Required<PX.Objects.AR.Customer.acctCD>>>>(graph);
                    //select.Cache.Clear();
                    //select.View.Clear();
                    //select.Cache.ClearQueryCache();
                    //graph.BAccount.Current = select.Select(impl.CustomerID?.Value);
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

        protected bool UpdateDefault(SPCustomerEntityBucket bucket, List<Location> locations, PX.Objects.AR.CustomerMaint graph, Location currentDefault)
        {
            var obj = bucket.Customer;
            if (obj.LocalID != null)
            {
                var addressReferences = SelectStatusChildren(obj.SyncID).Where(s => s.EntityType == BCEntitiesAttribute.Address && s.Deleted == false)?.ToList();
                var deletedValues = addressReferences?.Where(x => bucket.CustomerAddresses.All(y => x.ExternID != y.ExternID)).ToList();
                var mappeddefault = bucket.CustomerAddresses.FirstOrDefault(x => x.Extern.Default == true); // get new default mapped address
                var nextdefault = locations.FirstOrDefault(x => x.NoteID == mappeddefault?.LocalID);
                var defLocationExt = graph.GetExtension<PX.Objects.AR.CustomerMaint.DefLocationExt>();
                var locationDetails = graph.GetExtension<PX.Objects.AR.CustomerMaint.LocationDetailsExt>();
                //mark status for address as deleted for addresses deleted at BC
                if (deletedValues != null && deletedValues.Count > 0)
                {
                    foreach (var value in deletedValues)
                    {
                        DeleteStatus(value, BCSyncOperationAttribute.NotFound);

                        var location = locations.FirstOrDefault(x => x.NoteID == value.LocalID);
                        if (location != null)
                        {
                            location.IsActive = false;
                            locationDetails.Locations.Update(location);
                        }

                    }
                    graph.Actions.PressSave();
                    return true;
                }

                if (graph.BAccount.Current.DefLocationID != nextdefault?.LocationID)//if mapped default and deflocation are not in sync
                {
                    if (nextdefault == null) return false;
                    defLocationExt.DefLocation.Current = locationDetails.Locations.Select().RowCast<Location>()?.ToList()?.FirstOrDefault(x => x.LocationID == nextdefault.LocationID); ;
                    if (defLocationExt.DefLocation.Current.IsActive == false)
                    {
                        if (GetEntity(Operation.EntityType).PrimarySystem == BCSyncSystemAttribute.External)
                            defLocationExt.DefLocation.Current.IsActive = true;
                    }
                    defLocationExt.SetDefaultLocation.Press();
                    graph.Actions.PressSave();
                    return true;
                }
            }

            return false;
        }
        protected Guid? CheckIfExists(CustomerAddressData custAddr, List<PXResult<Location, PX.Objects.CR.Address, PX.Objects.CR.Contact, BAccount>> pXResults, List<MappedLocation> mappedLocations)
        {
            foreach (PXResult<Location, PX.Objects.CR.Address, PX.Objects.CR.Contact, BAccount> loc in pXResults)
            {
                Location location = loc.GetItem<Location>();
                PX.Objects.CR.Address address = loc.GetItem<PX.Objects.CR.Address>();
                PX.Objects.CR.Contact contact = loc.GetItem<PX.Objects.CR.Contact>();
                BAccount account = loc.GetItem<BAccount>();
                string fullName = new String[] { contact.Attention, location.LocationCD }.FirstNotEmpty();
                if (CompareLocation(custAddr, location, address, contact, fullName, account.AcctName))
                {
                    if (!mappedLocations.Any(x => x.LocalID == location.NoteID))
                        return location.NoteID;
                }
            }
            return null;
        }
        protected bool CompareLocation(CustomerAddressData custAddr, Location location, PX.Objects.CR.Address address, PX.Objects.CR.Contact contact, string fullName, string accountName)
        {
            return custAddr.City == address.City
                                            && custAddr.CountryCode?.Trim() == address.CountryID?.Trim()
                                            && custAddr.FirstName?.Trim() == (contact.FirstName ?? fullName.FieldsSplit(0, fullName))?.Trim()
                                            && custAddr.LastName?.Trim() == (contact.LastName ?? fullName.FieldsSplit(1, fullName))?.Trim()
                                            && (custAddr.Phone?.Trim() == contact.Phone1?.Trim() || custAddr.Phone?.Trim() == contact.Phone2?.Trim())
                                            && custAddr.ProvinceCode?.Trim()?.ToUpper() == address.State?.Trim()?.ToUpper()
                                            && (custAddr.Name?.Trim() == contact.FullName?.Trim() || custAddr.Name?.Trim() == accountName?.Trim())
                                            && custAddr.Address1?.Trim() == address.AddressLine1?.Trim()
                                            && custAddr.Address2?.Trim() == address.AddressLine2?.Trim()
                                            && custAddr.PostalCode?.Trim()?.ToUpperInvariant() == address.PostalCode?.Trim()
                                            && custAddr.Company?.Trim() == contact.FullName?.Trim();
        }

        #endregion

        #region Export
        public override void FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters)
        {
            IEnumerable<Core.API.Customer> impls = cbapi.GetAll<Core.API.Customer>(new Core.API.Customer { CustomerID = new StringReturn() }, minDateTime, maxDateTime, filters, GetCustomFieldsForExport());

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

        public override EntityStatus GetBucketForExport(SPCustomerEntityBucket bucket, BCSyncStatus bcstatus)
        {
            Customer impl = cbapi.GetByID<Customer>(bcstatus.LocalID, GetCustomFieldsForExport());
            if (impl == null) return EntityStatus.None;

            MappedCustomer obj = bucket.Customer = bucket.Customer.Set(impl, impl.SyncID, impl.SyncTime);
            EntityStatus status = EnsureStatus(bucket.Customer, SyncDirection.Export);

            return status;
        }

        public override void MapBucketExport(SPCustomerEntityBucket bucket, IMappedEntity existing)
        {
            MappedCustomer customerObj = bucket.Customer;
            MappedLocation addressObj = bucket.CustomerAddress;

            Customer customerImpl = customerObj.Local;
            Core.API.Contact contactImpl = customerImpl.MainContact;
            Core.API.Address addressImpl = contactImpl.Address;
            CustomerData customerData = customerObj.Extern = new CustomerData();

            //Customer
            customerData.Id = customerObj.ExternID?.ToLong();

            //Contact			
            string fullName = new String[] { contactImpl.Attention?.Value, customerImpl.CustomerName?.Value }.FirstNotEmpty();
            customerData.FirstName = fullName.FieldsSplit(0, fullName);
            customerData.LastName = fullName.FieldsSplit(1, fullName);
            customerData.Email = contactImpl.Email?.Value;
            customerData.Phone = contactImpl.Phone1?.Value ?? contactImpl.Phone2?.Value;


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
                        mappedLocation.Extern = MapLocationExport(mappedLocation, customerObj);
                        if (addressReference?.Count == 0 && existing != null)
                        {
                            mappedLocation.ExternID = CheckIfExists(mappedLocation.Local, existing, bucket.CustomerAddresses, customerImpl.CustomerName?.Value);
                        }
                        locationProcessor.RemapBucketExport(new SPLocationEntityBucket() { Address = mappedLocation }, null);
                        bucket.CustomerAddresses.Add(mappedLocation);
                    }

                }
            }
        }
        protected string CheckIfExists(CustomerLocation custAddr, IMappedEntity existing, List<MappedLocation> mappedLocations, string customerName)
        {
            CustomerData data = (CustomerData)existing.Extern;
            foreach (CustomerAddressData address in data.Addresses)
            {
                string fullName = new String[] { custAddr.LocationContact.Attention?.Value, custAddr.LocationName?.Value }.FirstNotEmpty();
                if (CompareLocation(custAddr, address, fullName, customerName))
                {
                    var id = new object[] { data.Id, address.Id }.KeyCombine();
                    if (!mappedLocations.Any(x => x.ExternID == id))
                        return id;
                }
            }
            return null;
        }

        protected bool CompareLocation(CustomerLocation custAddr, CustomerAddressData address, string fullName, string customerName)
        {
            return address.City == custAddr.LocationContact?.Address?.City?.Value
                    && (address.Company?.Trim()?.ToUpper() == custAddr.LocationContact?.CompanyName?.Value?.Trim()?.ToUpper() || address.Company?.Trim()?.ToUpper() == custAddr.LocationContact.FullName?.Value?.Trim()?.ToUpper())
                    && (address.CountryCode?.Trim() == custAddr.LocationContact?.Address?.Country?.Value?.Trim() || address.Country?.Trim() == custAddr.LocationContact?.Address?.Country?.Value?.Trim())
                    && (address.Phone?.Trim() == custAddr.LocationContact?.Phone1?.Value?.Trim() || address.Phone?.Trim() == custAddr.LocationContact?.Phone2?.Value?.Trim())
                    && address.ProvinceCode?.Trim()?.ToUpper() == custAddr.LocationContact?.Address?.State?.Value?.Trim()?.ToUpper()
                    && address.Address1?.Trim()?.ToUpper() == custAddr.LocationContact?.Address?.AddressLine1?.Value?.Trim()?.ToUpper()
                    && address.Address2?.Trim()?.ToUpper() == custAddr.LocationContact?.Address?.AddressLine2?.Value?.Trim()?.ToUpper()
                    && address.PostalCode?.Trim()?.ToUpperInvariant() == custAddr.LocationContact?.Address?.PostalCode?.Value?.Trim()
                    && address.Name?.Trim()?.ToUpper() == fullName?.Trim()?.ToUpper();
        }

        public override object GetAttribute(SPCustomerEntityBucket bucket, string attributeID)
        {
            MappedCustomer obj = bucket.Customer;
            Customer impl = obj.Local;
            return impl.Attributes?.Where(x => string.Equals(x?.Attribute?.Value, attributeID, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
        public override void AddAttributeValue(SPCustomerEntityBucket bucket, string attributeID, object attributeValue)
        {
            MappedCustomer obj = bucket.Customer;
            Customer impl = obj.Local;
            impl.Attributes = impl.Attributes ?? new List<AttributeDetail>();
            AttributeDetail attributeDetail = new AttributeDetail();
            attributeDetail.Attribute = new StringValue() { Value = attributeID };
            attributeDetail.Value = new StringValue() { Value = attributeValue?.ToString() };
            impl.Attributes.Add(attributeDetail);
        }

        public override void SaveBucketExport(SPCustomerEntityBucket bucket, IMappedEntity existing, String operation)
        {
            MappedCustomer obj = bucket.Customer;
            MappedLocation addressObj = bucket.CustomerAddress;

            //Customer
            CustomerData customerData = null;
            try
            {
                if (obj.ExternID == null || existing == null)
                {
                    customerData = customerDataProvider.Create(obj.Extern);
                }
                else
                {
                    customerData = customerDataProvider.Update(obj.Extern);
                }
                obj.AddExtern(customerData, customerData.Id?.ToString(), customerData.DateModifiedAt.ToDate(false));
                UpdateStatus(obj, operation);

            }
            catch (Exception ex)
            {
                throw new PXException(ex.Message);
            }

            // update ExternalRef
            string externalRef = APIHelper.ReferenceMake(customerData.Id?.ToString(), GetBinding().BindingName);

            string[] keys = obj.Local?.AccountRef?.Value?.Split(';');
            if (keys?.Contains(externalRef) != true)
            {
                if (!string.IsNullOrEmpty(obj.Local?.AccountRef?.Value))
                    externalRef = new object[] { obj.Local?.AccountRef?.Value, externalRef }.KeyCombine();

                if (externalRef.Length < 50)
                    PXDatabase.Update<BAccount>(
                                      new PXDataFieldAssign(typeof(BAccount.acctReferenceNbr).Name, PXDbType.NVarChar, externalRef),
                                      new PXDataFieldRestrict(typeof(BAccount.noteID).Name, PXDbType.UniqueIdentifier, obj.Local.NoteID?.Value)
                                      );
            }

            //Address
            if (isLocationActive)
            {
                bool locationUpdated = false;

                foreach (var address in bucket.CustomerAddresses)
                {
                    try
                    {
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
                            string locOperation;
                            locationUpdated = true;
                            CustomerAddressData addressData = null;
                            if (address.ExternID == null || existing == null)
                            {
                                addressData = customerAddressDataProvider.Create(address.Extern, customerData.Id.ToString());
                                locOperation = BCSyncOperationAttribute.ExternInsert;
                            }
                            else
                            {
                                addressData = customerAddressDataProvider.Update(address.Extern, address.ExternID.KeySplit(0), address.ExternID.KeySplit(1));
                                locOperation = BCSyncOperationAttribute.ExternUpdate;
                            }

                            address.AddExtern(addressData, new object[] { customerData.Id, addressData.Id }.KeyCombine(), addressData.CalculateHash());

                            UpdateStatus(address, locOperation);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(Operation.LogScope(address), ex);
                        UpdateStatus(address, BCSyncOperationAttribute.ExternFailed, message: ex.Message);
                    }
                    if (locationUpdated)
                    {
                        customerData = customerDataProvider.GetByID(customerData.Id.ToString());
                        obj.AddExtern(customerData, customerData.Id?.ToString(), customerData.DateModifiedAt.ToDate(false));
                        UpdateStatus(obj, operation);
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
}