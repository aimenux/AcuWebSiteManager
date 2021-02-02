using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    public class FSSrvOrdContactAttribute : FSDocumentContactAttribute
    {
        public FSSrvOrdContactAttribute(Type selectType)
            : base(selectType)
        {
        }

        public override void DefaultContact<TContact, TContactID>(PXCache sender, object documentRow, object contactRow)
        {
            PXView view = null;
            object parm = null;
            FSSrvOrdType fsSrvOrdTypeRow = null;
            bool isBranchLocation = false;
            BqlCommand select;

            if (sender.Graph is ServiceOrderEntry)
            {
                fsSrvOrdTypeRow = ((ServiceOrderEntry)sender.Graph).ServiceOrderTypeSelected.Current;
            }
            else if(sender.Graph is AppointmentEntry)
            {
                fsSrvOrdTypeRow = ((AppointmentEntry)sender.Graph).ServiceOrderTypeSelected.Current;
            }

            if (fsSrvOrdTypeRow != null
                    && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BRANCH_LOCATION
                        && sender.GetValue<FSServiceOrder.branchLocationID>(documentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.branchLocationID>(documentRow);

                select = BqlCommand.CreateInstance(typeof(
                         Select2<FSBLOCContact,
                         InnerJoin<FSBranchLocation,
                            On<
                                FSBranchLocation.branchLocationContactID, Equal<FSBLOCContact.contactID>>>,
                         Where<
                             FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>));

                view = sender.Graph.TypedViews.GetView(select, false);
                isBranchLocation = true;
            }
            else if (fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.CUSTOMER_CONTACT
                            && sender.GetValue<FSServiceOrder.contactID>(documentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.contactID>(documentRow);

                select = BqlCommand.CreateInstance(typeof(
                         Select2<Contact,
                         LeftJoin<FSContact,
                            On<
                                FSContact.bAccountID, Equal<Contact.bAccountID>,
                                And<FSContact.bAccountContactID, Equal<Contact.contactID>,
                                And<FSContact.revisionID, Equal<Contact.revisionID>,
                                And<FSContact.isDefaultContact, Equal<boolTrue>,
                                And<FSContact.entityType, Equal<FSContact.entityType.ServiceOrder>>>>>>>,
                         Where<
                             Contact.contactID, Equal<Required<FSServiceOrder.contactID>>>>));

                view = sender.Graph.TypedViews.GetView(select, false);
            }
            else if (fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BUSINESS_ACCOUNT
                            && sender.GetValue<FSServiceOrder.locationID>(documentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.locationID>(documentRow);

                select = BqlCommand.CreateInstance(typeof(
                         Select2<Contact,
                         LeftJoin<Location, 
                            On<
                                Location.locationID, Equal<Required<FSServiceOrder.locationID>>>,
                         LeftJoin<FSContact,
                            On<
                                FSContact.bAccountID, Equal<Contact.bAccountID>,
                                And<FSContact.bAccountContactID, Equal<Contact.contactID>,
                                And<FSContact.revisionID, Equal<Contact.revisionID>,
                                And<FSContact.isDefaultContact, Equal<boolTrue>,
                                And<FSContact.entityType, Equal<FSContact.entityType.ServiceOrder>>>>>>>>,
                         Where<
                             Contact.contactID, Equal<Location.defContactID>>>));

                view = sender.Graph.TypedViews.GetView(select, false);
            }

            if (view != null)
            {
                int startRow = -1;
                int totalRows = 0;
                bool contactFound = false;

                foreach (PXResult res in view.Select(new object[] { documentRow }, new[] { parm }, null, null, null, null, ref startRow, 1, ref totalRows))
                {
                    if (isBranchLocation)
                    {
                        contactFound = DefaultBLOCContact<FSBLOCContact, FSBLOCContact.contactID>(sender, FieldName, documentRow, contactRow, res);
                    }
                    else
                    {
                        contactFound = DefaultContact<TContact, TContactID>(sender, FieldName, documentRow, contactRow, res);
                    }

                    break;
                }

                if (!contactFound && !_Required)
                {
                    ClearRecord(sender, documentRow);
                }
            }
            else
            {
                ClearRecord(sender, documentRow);

                if (_Required && sender.GetValue(documentRow, _FieldOrdinal) == null)
                {
                    using (ReadOnlyScope rs = new ReadOnlyScope(sender.Graph.Caches[_RecordType]))
                    {
                        object record = sender.Graph.Caches[_RecordType].Insert();
                        object recordID = sender.Graph.Caches[_RecordType].GetValue(record, _RecordID);
                        sender.SetValue(documentRow, _FieldOrdinal, recordID);
                    }
                }
            }
        }

        public static bool DefaultBLOCContact<TContact, TContactID>(PXCache sender, string fieldName, object documentRow, object contactRow, object sourceRow)
            where TContact : class, IBqlTable, IContact, new()
            where TContactID : IBqlField
        {
            bool contactFound = false;

            if (sourceRow != null)
            {
                FSContact contact = contactRow as FSContact;

                if (contact == null)
                {
                    contact = PXSelect<FSContact, 
                              Where<
                                  FSContact.contactID, Equal<Required<TContactID>>>>
                              .Select(sender.Graph, sender.GetValue(documentRow, fieldName));
                }

                if (PXResult.Unwrap<FSContact>(sourceRow)?.ContactID == null || sender.GetValue(documentRow, fieldName) == null)
                {
                    if (contact == null || contact.ContactID > 0)
                    {
                        contact = new FSContact();
                    }

                    contact.BAccountContactID = PXResult.Unwrap<TContact>(sourceRow).ContactID;
                    contact.BAccountID = PXResult.Unwrap<TContact>(sourceRow).BAccountID;
                    contact.RevisionID = PXResult.Unwrap<TContact>(sourceRow).RevisionID;
                    contact.IsDefaultContact = true;
                    contact.FullName = PXResult.Unwrap<TContact>(sourceRow).FullName;
                    contact.Salutation = PXResult.Unwrap<TContact>(sourceRow).Salutation;
                    contact.Attention = PXResult.Unwrap<TContact>(sourceRow).Attention;
                    contact.Title = PXResult.Unwrap<TContact>(sourceRow).Title;
                    contact.Phone1 = PXResult.Unwrap<TContact>(sourceRow).Phone1;
                    contact.Phone1Type = PXResult.Unwrap<TContact>(sourceRow).Phone1Type;
                    contact.Phone2 = PXResult.Unwrap<TContact>(sourceRow).Phone2;
                    contact.Phone2Type = PXResult.Unwrap<TContact>(sourceRow).Phone2Type;
                    contact.Phone3 = PXResult.Unwrap<TContact>(sourceRow).Phone3;
                    contact.Phone3Type = PXResult.Unwrap<TContact>(sourceRow).Phone3Type;
                    contact.Fax = PXResult.Unwrap<TContact>(sourceRow).Fax;
                    contact.FaxType = PXResult.Unwrap<TContact>(sourceRow).FaxType;
                    contact.Email = PXResult.Unwrap<TContact>(sourceRow).Email;

                    contactFound = contact.BAccountContactID != null && contact.BAccountID != null && contact.RevisionID != null;

                    if (contact.ContactID == null)
                    {
                        contact = (FSContact)sender.Graph.Caches[typeof(FSContact)].Insert(contact);
                        sender.SetValue(documentRow, fieldName, contact.ContactID);
                    }
                    else if (contactRow == null)
                    {
                        sender.Graph.Caches[typeof(FSContact)].Update(contact);
                    }
                }
                else
                {
                    if (contact != null && contact.ContactID < 0)
                    {
                        sender.Graph.Caches[typeof(FSContact)].Delete(contact);
                    }

                    sender.SetValue(documentRow, fieldName, PXResult.Unwrap<TContact>(sourceRow).ContactID);
                    contactFound = PXResult.Unwrap<FSContact>(sourceRow).ContactID != null;
                }
            }

            return contactFound;
        }
    }

    public class FSSrvOrdAddressAttribute : FSDocumentAddressAttribute
    {
        public FSSrvOrdAddressAttribute(Type selectType)
            : base(selectType)
        {
        }

        public override void DefaultAddress<TAddress, TAddressID>(PXCache sender, object documentRow, object addressRow)
        {
            PXView view = null;
            object parm = null;
            FSSrvOrdType fsSrvOrdTypeRow = null;
            bool isBranchLocation = false;
            BqlCommand select;

            if (sender.Graph is ServiceOrderEntry)
            {
                fsSrvOrdTypeRow = ((ServiceOrderEntry)sender.Graph).ServiceOrderTypeSelected.Current;
            }
            else if (sender.Graph is AppointmentEntry)
            {
                fsSrvOrdTypeRow = ((AppointmentEntry)sender.Graph).ServiceOrderTypeSelected.Current;
            }

            if (fsSrvOrdTypeRow != null
                    && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BRANCH_LOCATION
                        && sender.GetValue<FSServiceOrder.branchLocationID>(documentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.branchLocationID>(documentRow);

                select = BqlCommand.CreateInstance(typeof(
                         Select2<FSBLOCAddress,
                         InnerJoin<FSBranchLocation,
                            On<
                                FSBranchLocation.branchLocationAddressID, Equal<FSBLOCAddress.addressID>>>,
                         Where<
                             FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>));

                view = sender.Graph.TypedViews.GetView(select, false);
                isBranchLocation = true;
            }
            else if (fsSrvOrdTypeRow != null
                         && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.CUSTOMER_CONTACT
                             && sender.GetValue<FSServiceOrder.contactID>(documentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.contactID>(documentRow);

                select = BqlCommand.CreateInstance(typeof(
                         Select2<Address,
                         LeftJoin<Contact,
                            On<
                                Contact.defAddressID, Equal<Address.addressID>>,
                         LeftJoin<FSAddress,
                            On<
                                FSAddress.bAccountID, Equal<Contact.bAccountID>,
                                And<FSAddress.bAccountAddressID, Equal<Address.addressID>,
                                And<FSAddress.revisionID, Equal<Address.revisionID>,
                                And<FSAddress.isDefaultAddress, Equal<boolTrue>>>>>>>,
                     Where<
                         Contact.contactID, Equal<Required<FSServiceOrder.contactID>>>>));

                view = sender.Graph.TypedViews.GetView(select, false);
            }

            else if (fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BUSINESS_ACCOUNT
                            && sender.GetValue<FSServiceOrder.locationID>(documentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.locationID>(documentRow);

                select = BqlCommand.CreateInstance(typeof(
                         Select2<Address,
                         LeftJoin<Contact,
                            On<
                                Contact.defAddressID, Equal<Address.addressID>>,
                         LeftJoin<Location,
                            On<
                                Location.locationID, Equal<Required<FSManufacturer.locationID>>>,
                         LeftJoin<FSAddress,
                            On<
                                FSAddress.bAccountID, Equal<Contact.bAccountID>,
                                And<FSAddress.bAccountAddressID, Equal<Address.addressID>,
                                And<FSAddress.revisionID, Equal<Address.revisionID>,
                                And<FSAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>,
                         Where<
                             Address.addressID, Equal<Location.defAddressID>>>));

                view = sender.Graph.TypedViews.GetView(select, false);
            }

            if (view != null)
            {
                int startRow = -1;
                int totalRows = 0;
                bool addressFound = false;

                foreach (PXResult res in view.Select(new object[] { documentRow }, new[] { parm }, null, null, null, null, ref startRow, 1, ref totalRows))
                {
                    if (isBranchLocation)
                    {
                        addressFound = DefaultBLOCAddress<FSBLOCAddress, FSBLOCAddress.addressID>(sender, FieldName, documentRow, addressRow, res);
                    }
                    else
                    {
                        addressFound = DefaultAddress<TAddress, TAddressID>(sender, FieldName, documentRow, addressRow, res);
                    }
                    
                    break;
                }

                if (!addressFound && !_Required)
                {
                    this.ClearRecord(sender, documentRow);
                }
            }
            else
            {
                ClearRecord(sender, documentRow);

                if (_Required && sender.GetValue(documentRow, _FieldOrdinal) == null)
                {
                    using (ReadOnlyScope rs = new ReadOnlyScope(sender.Graph.Caches[_RecordType]))
                    {
                        object record = sender.Graph.Caches[_RecordType].Insert();
                        object recordID = sender.Graph.Caches[_RecordType].GetValue(record, _RecordID);
                        sender.SetValue(documentRow, _FieldOrdinal, recordID);
                    }
                }
            }
        }

        public static bool DefaultBLOCAddress<TAddress, TAddressID>(PXCache sender, string fieldName, object documentRow, object addressRow, object sourceRow)
            where TAddress : class, IBqlTable, IAddress, new()
            where TAddressID : IBqlField
        {
            bool addressFound = false;

            if (sourceRow != null)
            {
                FSAddress address = addressRow as FSAddress;

                if (address == null)
                {
                    address = PXSelect<FSAddress,
                              Where<
                                  FSAddress.addressID, Equal<Required<TAddressID>>>>
                              .Select(sender.Graph, sender.GetValue(documentRow, fieldName));
                }

                if (PXResult.Unwrap<FSAddress>(sourceRow)?.AddressID == null || sender.GetValue(documentRow, fieldName) == null)
                {
                    if (address == null || address.AddressID > 0)
                    {
                        address = new FSAddress();
                    }

                    address.BAccountAddressID = PXResult.Unwrap<TAddress>(sourceRow).AddressID;
                    address.BAccountID = PXResult.Unwrap<TAddress>(sourceRow).BAccountID;
                    address.RevisionID = PXResult.Unwrap<TAddress>(sourceRow).RevisionID;
                    address.IsDefaultAddress = true;
                    address.AddressLine1 = PXResult.Unwrap<TAddress>(sourceRow).AddressLine1;
                    address.AddressLine2 = PXResult.Unwrap<TAddress>(sourceRow).AddressLine2;
                    address.AddressLine3 = PXResult.Unwrap<TAddress>(sourceRow).AddressLine3;
                    address.City = PXResult.Unwrap<TAddress>(sourceRow).City;
                    address.State = PXResult.Unwrap<TAddress>(sourceRow).State;
                    address.PostalCode = PXResult.Unwrap<TAddress>(sourceRow).PostalCode;
                    address.CountryID = PXResult.Unwrap<TAddress>(sourceRow).CountryID;
                    address.IsValidated = PXResult.Unwrap<TAddress>(sourceRow).IsValidated;
                    addressFound = address.BAccountAddressID != null && address.BAccountID != null && address.RevisionID != null;

                    if (address.AddressID == null)
                    {
                        address = (FSAddress)sender.Graph.Caches[typeof(FSAddress)].Insert(address);
                        sender.SetValue(documentRow, fieldName, address.AddressID);
                    }
                    else if (addressRow == null)
                    {
                        sender.Graph.Caches[typeof(FSAddress)].Update(address);
                    }
                }
                else
                {
                    if (address != null && address.AddressID < 0)
                    {
                        sender.Graph.Caches[typeof(FSAddress)].Delete(address);
                    }

                    sender.SetValue(documentRow, fieldName, PXResult.Unwrap<TAddress>(sourceRow).AddressID);
                    addressFound = PXResult.Unwrap<FSAddress>(sourceRow).AddressID != null;
                }
            }

            return addressFound;
        }
    }
}
