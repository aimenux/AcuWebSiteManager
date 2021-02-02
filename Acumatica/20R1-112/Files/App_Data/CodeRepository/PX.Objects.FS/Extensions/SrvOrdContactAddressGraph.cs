using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.Extensions.ContactAddress;

namespace PX.Objects.FS
{
    public enum ContacAddressCallerEnum
    {
        Insert,
        BranchLocationID,
        BAccountID,
        ContactID,
        LocationID
    }

    public class SrvOrdContactAddressGraph<TGraph> : ContactAddressGraph<TGraph>
        where TGraph : PXGraph
    {
        protected override DocumentMapping GetDocumentMapping()
        {
            return new DocumentMapping(typeof(FSServiceOrder))
            {
                DocumentAddressID = typeof(FSServiceOrder.serviceOrderAddressID),
                DocumentContactID = typeof(FSServiceOrder.serviceOrderContactID)
            };
        }

        protected override DocumentContactMapping GetDocumentContactMapping()
        {
            return new DocumentContactMapping(typeof(FSContact)) { EMail = typeof(FSContact.email) };
        }

        protected override DocumentAddressMapping GetDocumentAddressMapping()
        {
            return new DocumentAddressMapping(typeof(FSAddress));
        }

        protected virtual PXSelectBase<FSContact> GetContactView()
        {
            if (Base is ServiceOrderEntry)
            {
                return ((ServiceOrderEntry)(PXGraph)Base).ServiceOrder_Contact;
            }

            if (Base is AppointmentEntry)
            {
                return ((AppointmentEntry)(PXGraph)Base).ServiceOrder_Contact;
            }

            return null;
        }

        protected virtual PXSelectBase<FSAddress> GetAddressView()
        {
            if (Base is ServiceOrderEntry)
            {
                return ((ServiceOrderEntry)(PXGraph)Base).ServiceOrder_Address;
            }

            if (Base is AppointmentEntry)
            {
                return ((AppointmentEntry)(PXGraph)Base).ServiceOrder_Address;
            }

            return null;
        }

        protected virtual PXSelectBase<FSSrvOrdType> GetSrvOrdTypeView()
        {
            if (Base is ServiceOrderEntry)
            {
                return ((ServiceOrderEntry)(PXGraph)Base).ServiceOrderTypeSelected;
            }

            if (Base is AppointmentEntry)
            {
                return ((AppointmentEntry)(PXGraph)Base).ServiceOrderTypeSelected;
            }

            return null;
        }

        protected override PXCache GetContactCache()
        {
            return GetContactView().Cache;
        }

        protected override PXCache GetAddressCache()
        {
            return GetAddressView().Cache;
        }

        protected override IPersonalContact GetCurrentContact()
        {
            var contact = GetContactView().SelectSingle();
            return contact;
        }

        protected override IPersonalContact GetEtalonContact()
        {
            bool isDirty = GetContactView().Cache.IsDirty;
            var contact = GetContactView().Insert();
            GetContactView().Cache.SetStatus(contact, PXEntryStatus.Held);
            GetContactView().Cache.IsDirty = isDirty;
            return contact;
        }

        protected override IAddress GetCurrentAddress()
        {
            var address = GetAddressView().SelectSingle();
            return address;
        }

        protected override IAddress GetEtalonAddress()
        {
            bool isDirty = GetAddressView().Cache.IsDirty;
            var address = GetAddressView().Insert();
            GetAddressView().Cache.SetStatus(address, PXEntryStatus.Held);
            GetAddressView().Cache.IsDirty = isDirty;
            return address;
        }

        protected override IPersonalContact GetCurrentShippingContact()
        {
            return null;
        }
        protected override IPersonalContact GetEtalonShippingContact()
        {
            return null;
        }
        protected override IAddress GetCurrentShippingAddress()
        {
            return null;
        }
        protected override IAddress GetEtalonShippingAddress()
        {
            return null;
        }
        protected override PXCache GetShippingContactCache()
        {
            return null;
        }
        protected override PXCache GetShippingAddressCache()
        {
            return null;
        }


        protected override bool IsThereSomeContactAddressSourceValue(PXCache cache, Extensions.ContactAddress.Document row)
        {
            FSSrvOrdType fSSrvOrdTypeRow = (FSSrvOrdType)GetSrvOrdTypeView().Current;
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)cache.GetMain<Extensions.ContactAddress.Document>(row);

            if (fSSrvOrdTypeRow == null || fsServiceOrderRow == null)
                return false;

            return (
                (fSSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BUSINESS_ACCOUNT
                    && (row.LocationID != null)
                )
                ||
                (fSSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.CUSTOMER_CONTACT
                    && (row.ContactID != null)
                )
                ||
                (fSSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BRANCH_LOCATION
                    && fsServiceOrderRow.BranchLocationID != null
                )
            );
        }

        protected void SetDefaultContactAndAddress(PXCache cache, object Row, int? oldContactID, int? oldLocationID, 
                                                            int? oldBranchLocationID, ContacAddressCallerEnum callerID)
        {
            Extensions.ContactAddress.Document row;
            FSServiceOrder fsServiceOrderRow = null;
            if (Row is FSServiceOrder)
            {
                row = cache.GetExtension<Extensions.ContactAddress.Document>(Row);
                fsServiceOrderRow = (FSServiceOrder)Row;
            }
            else
            {
                row = Row as Extensions.ContactAddress.Document;
                fsServiceOrderRow = (FSServiceOrder)cache.GetMain<Extensions.ContactAddress.Document>(row);
            }
            if (row == null) return;

            Contact oldContact = null;
            Address oldAddress = null;

            FSSrvOrdType fsSrvOrdTypeRow = null;

            if (cache.Graph is ServiceOrderEntry)
            {
                fsSrvOrdTypeRow = ((ServiceOrderEntry)cache.Graph).ServiceOrderTypeSelected.Current;
            }
            else if (cache.Graph is AppointmentEntry)
            {
                fsSrvOrdTypeRow = ((AppointmentEntry)cache.Graph).ServiceOrderTypeSelected.Current;
            }

            if (fsSrvOrdTypeRow != null
                   && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BRANCH_LOCATION
                       && oldBranchLocationID != null)
            {
                oldContact = ContactAddressHelper.GetContact(
                                (FSContact)PXSelectJoin<
                                    FSContact,
                                    InnerJoin<FSBranchLocation,
                                        On<FSBranchLocation.branchLocationContactID, Equal<FSContact.contactID>>>,
                                    Where<FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                    .Select(Base, oldBranchLocationID));
                oldAddress = ContactAddressHelper.GetAddress(
                                (FSAddress)PXSelectJoin<
                                    FSAddress,
                                    InnerJoin<FSBranchLocation,
                                        On<FSBranchLocation.branchLocationAddressID, Equal<FSAddress.addressID>>>,
                                    Where<FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                    .Select(Base, oldBranchLocationID));
            }
            else if (fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.CUSTOMER_CONTACT
                        && oldContactID != null)
            {
                oldContact = PXSelect<Contact,
                   Where<Contact.contactID, Equal<Required<Extensions.ContactAddress.Document.contactID>>>>.Select(Base, oldContactID);
                oldAddress = PXSelectJoin<Address,
                    LeftJoin<Contact, On<Contact.defAddressID, Equal<Address.addressID>>>,
                    Where<Contact.contactID, Equal<Required<Extensions.ContactAddress.Document.contactID>>>>.Select(Base, oldContactID);
            }
            else if (fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BUSINESS_ACCOUNT
                        && oldLocationID != null)
            {
                oldContact = PXSelectJoin<Contact,
                    LeftJoin<Location, On<Location.locationID, Equal<Required<Extensions.ContactAddress.Document.locationID>>>>,
                    Where<Contact.contactID, Equal<Location.defContactID>>>.Select(Base, oldLocationID);
                oldAddress = PXSelectJoin<Address,
                    LeftJoin<Contact, On<Contact.defAddressID, Equal<Address.addressID>>,
                    LeftJoin<Location, On<Location.locationID, Equal<Required<Extensions.ContactAddress.Document.locationID>>>>>,
                   Where<Address.addressID, Equal<Location.defAddressID>>>.Select(Base, oldLocationID);
            }

            bool CallDefaults = false;

            switch (callerID)
            {
                case ContacAddressCallerEnum.Insert:
                    CallDefaults = true;
                    break;
                case ContacAddressCallerEnum.BranchLocationID:
                    CallDefaults = fsSrvOrdTypeRow?.AppAddressSource == ID.SrvOrdType_AppAddressSource.BRANCH_LOCATION
                                        && fsServiceOrderRow.BranchLocationID != null;
                    break;
                case ContacAddressCallerEnum.BAccountID:
                    CallDefaults = fsSrvOrdTypeRow?.AppAddressSource != ID.SrvOrdType_AppAddressSource.BRANCH_LOCATION
                                        && fsServiceOrderRow.CustomerID != null;
                    break;
                case ContacAddressCallerEnum.ContactID:
                    CallDefaults = fsSrvOrdTypeRow?.AppAddressSource == ID.SrvOrdType_AppAddressSource.CUSTOMER_CONTACT
                                        && fsServiceOrderRow.ContactID != null;
                    break;
                case ContacAddressCallerEnum.LocationID:
                    CallDefaults = fsSrvOrdTypeRow?.AppAddressSource == ID.SrvOrdType_AppAddressSource.BUSINESS_ACCOUNT
                                        && fsServiceOrderRow.LocationID != null;
                    break;
            }

            if (CallDefaults)
            {
                ChangedData forContactInfo = new ChangedData();
                forContactInfo.OldContact = oldContact;
                forContactInfo.OldAddress = oldAddress;

                DefaultRecords(row, forContactInfo, new ChangedData(false));
            }
        }

        protected virtual void _(Events.FieldUpdated<FSServiceOrder, FSServiceOrder.branchLocationID> e)
        {
            FSServiceOrder row = e.Row;
            if (row == null) return;

            SetDefaultContactAndAddress(e.Cache, e.Row, row.ContactID, row.LocationID, (int?)e.OldValue, ContacAddressCallerEnum.BranchLocationID);
        }

        protected override void _(Events.FieldSelecting<Extensions.ContactAddress.Document, Extensions.ContactAddress.Document.allowOverrideContactAddress> e)
        {
            var row = e.Row as Extensions.ContactAddress.Document;
            if (row == null) return;

            FSSrvOrdType fSSrvOrdTypeRow = (FSSrvOrdType)GetSrvOrdTypeView().Current;
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Cache.GetMain<Extensions.ContactAddress.Document>(row);

            if (fSSrvOrdTypeRow == null || fsServiceOrderRow == null)
                return;

            if (
                (fSSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BUSINESS_ACCOUNT
                    && (row.LocationID == null)
                )
                ||
                (fSSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.CUSTOMER_CONTACT
                    && (row.ContactID == null)
                )
                ||
                (fSSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BRANCH_LOCATION
                    && fsServiceOrderRow.BranchLocationID == null
                )
            )
            {
                e.ReturnValue = false;
            }
        }

        protected override void _(Events.FieldUpdated<Extensions.ContactAddress.Document, Extensions.ContactAddress.Document.contactID> e)
        {
            Extensions.ContactAddress.Document row = e.Row;
            if (row == null) return;

            int? branchLocationID = (int?)e.Cache.GetValue<FSServiceOrder.branchLocationID>(row);
            SetDefaultContactAndAddress(e.Cache, e.Row, (int?)e.OldValue, row.LocationID, branchLocationID, ContacAddressCallerEnum.ContactID);
        }

        protected virtual void _(Events.RowSelected<Extensions.ContactAddress.Document> e)
        {
            if (Base is ServiceOrderEntry)
            {
                ((ServiceOrderEntry)(PXGraph)Base).viewDirectionOnMap.SetEnabled(true);
            }
            else if (Base is AppointmentEntry)
            {
                ((AppointmentEntry)(PXGraph)Base).viewDirectionOnMap.SetEnabled(true);
            }
        }

        protected override void _(Events.RowInserted<Extensions.ContactAddress.Document> e)
        {
            var row = e.Row as Extensions.ContactAddress.Document;
            if (row == null) return;

            bool oldContactDirty = GetContactCache().IsDirty;
            bool oldAddressDirty = GetAddressCache().IsDirty;

            int? branchLocationID = (int?)e.Cache.GetValue<FSServiceOrder.branchLocationID>(row);
            SetDefaultContactAndAddress(e.Cache, e.Row, row.ContactID, row.LocationID, branchLocationID, ContacAddressCallerEnum.Insert);

            GetContactCache().IsDirty = oldContactDirty;
            GetAddressCache().IsDirty = oldAddressDirty;
        }

        protected override void _(Events.FieldUpdated<Extensions.ContactAddress.Document, Extensions.ContactAddress.Document.locationID> e)
        {
            Extensions.ContactAddress.Document row = e.Row;
            if (row == null) return;

            int? branchLocationID = (int?)e.Cache.GetValue<FSServiceOrder.branchLocationID>(row);
            SetDefaultContactAndAddress(e.Cache, e.Row, row.ContactID, (int?)e.OldValue, branchLocationID, ContacAddressCallerEnum.LocationID);
        }

        protected override void _(Events.FieldUpdated<Extensions.ContactAddress.Document, Extensions.ContactAddress.Document.bAccountID> e)
        {
            Extensions.ContactAddress.Document row = e.Row;
            if (row == null) return;

            int? branchLocationID = (int?)e.Cache.GetValue<FSServiceOrder.branchLocationID>(row);
            SetDefaultContactAndAddress(e.Cache, e.Row, row.ContactID, row.LocationID, branchLocationID, ContacAddressCallerEnum.BAccountID);
        }
    }
}
