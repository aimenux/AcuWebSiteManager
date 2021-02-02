using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GDPR;
using System;

namespace PX.Objects.FS
{
    public class FSSrvOrdContactAttribute : FSDocumentContactAttribute
    {
        public FSSrvOrdContactAttribute(Type SelectType)
            : base(SelectType)
        {
        }

        public override void DefaultContact<TContact, TContactID>(PXCache sender, object DocumentRow, object ContactRow)
        {
            PXView view = null;
            object parm = null;

            FSSrvOrdType fsSrvOrdTypeRow = null;
            bool isBranchLocation = false;

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
                            && sender.GetValue<FSServiceOrder.branchLocationID>(DocumentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.branchLocationID>(DocumentRow);

                BqlCommand Select = BqlCommand.CreateInstance(typeof(
                                    Select2<
                                        FSBLOCContact,
                                        InnerJoin<FSBranchLocation,
                                            On<FSBranchLocation.branchLocationContactID, Equal<FSBLOCContact.contactID>>>,
                                        Where<FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>));
                view = sender.Graph.TypedViews.GetView(Select, false);
                isBranchLocation = true;
            }
            else if (fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.CUSTOMER_CONTACT
                            && sender.GetValue<FSServiceOrder.contactID>(DocumentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.contactID>(DocumentRow);
                BqlCommand Select = BqlCommand.CreateInstance(typeof(
                        Select2<
                            Contact,
                            LeftJoin<FSContact,
                                On<FSContact.bAccountID,
                                 Equal<Contact.bAccountID>,
                                And<FSContact.bAccountContactID,
                                     Equal<Contact.contactID>,
                                And<FSContact.revisionID, Equal<Contact.revisionID>,
                                And<FSContact.isDefaultContact, Equal<boolTrue>,
                                And<FSContact.entityType, Equal<FSContact.entityType.ServiceOrder>>>>>>>,
                            Where<Contact.contactID, Equal<Required<FSServiceOrder.contactID>>>>));
                        view = sender.Graph.TypedViews.GetView(Select, false);
            }

            else if (fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BUSINESS_ACCOUNT
                            && sender.GetValue<FSServiceOrder.locationID>(DocumentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.locationID>(DocumentRow);
                BqlCommand Select = BqlCommand.CreateInstance(typeof(
                    Select2<
                        Contact,
                        LeftJoin<Location, 
                            On<Location.locationID, Equal<Required<FSServiceOrder.locationID>>>,
                        LeftJoin<FSContact,
                            On<FSContact.bAccountID,
                             Equal<Contact.bAccountID>,
                            And<FSContact.bAccountContactID,
                                 Equal<Contact.contactID>,
                            And<FSContact.revisionID, Equal<Contact.revisionID>,
                            And<FSContact.isDefaultContact, Equal<boolTrue>,
                            And<FSContact.entityType, Equal<FSContact.entityType.ServiceOrder>>>>>>>>,
                        Where<Contact.contactID, Equal<Location.defContactID>>>));
                        view = sender.Graph.TypedViews.GetView(Select, false);
            }

            if (view != null)
            {
                int startRow = -1;
                int totalRows = 0;

                bool contactFound = false;
                foreach (PXResult res in view.Select(new object[] { DocumentRow }, new[] { parm }, null, null, null, null, ref startRow, 1, ref totalRows))
                {
                    if (isBranchLocation)
                    {
                        contactFound = DefaultBLOCContact<FSBLOCContact, FSBLOCContact.contactID>(sender, FieldName, DocumentRow, ContactRow, res);
                    }
                    else
                    {
                        contactFound = DefaultContact<TContact, TContactID>(sender, FieldName, DocumentRow, ContactRow, res);
                    }
                    break;
                }
                if (!contactFound && !_Required)
                    ClearRecord(sender, DocumentRow);
            }

            else
            {
                ClearRecord(sender, DocumentRow);
                if (_Required && sender.GetValue(DocumentRow, _FieldOrdinal) == null)
                {
                    using (ReadOnlyScope rs = new ReadOnlyScope(sender.Graph.Caches[_RecordType]))
                    {
                        object record = sender.Graph.Caches[_RecordType].Insert();
                        object recordid = sender.Graph.Caches[_RecordType].GetValue(record, _RecordID);

                        sender.SetValue(DocumentRow, _FieldOrdinal, recordid);
                    }
                }
            }
        }

        public static bool DefaultBLOCContact<TContact, TContactID>(PXCache sender, string FieldName, object DocumentRow, object ContactRow, object SourceRow)
            where TContact : class, IBqlTable, IContact, new()
            where TContactID : IBqlField
        {
            bool contactFound = false;
            if (SourceRow != null)
            {
                FSContact contact = ContactRow as FSContact;

                if (contact == null)
                {
                    contact = ((FSContact)PXSelect<
                                FSContact, 
                                Where<
                                        FSContact.contactID, Equal<Required<TContactID>>>>
                                .Select(sender.Graph, sender.GetValue(DocumentRow, FieldName)));
                }

                if (PXResult.Unwrap<FSContact>(SourceRow)?.ContactID == null || sender.GetValue(DocumentRow, FieldName) == null)
                {
                    if (contact == null || contact.ContactID > 0)
                    {
                        contact = new FSContact();
                    }

                    contact.BAccountContactID = PXResult.Unwrap<TContact>(SourceRow).ContactID;
                    contact.BAccountID = PXResult.Unwrap<TContact>(SourceRow).BAccountID;
                    contact.RevisionID = PXResult.Unwrap<TContact>(SourceRow).RevisionID;
                    contact.IsDefaultContact = true;
                    contact.FullName = PXResult.Unwrap<TContact>(SourceRow).FullName;
                    contact.Salutation = PXResult.Unwrap<TContact>(SourceRow).Salutation;
                    contact.Attention = PXResult.Unwrap<TContact>(SourceRow).Attention;
                    contact.Title = PXResult.Unwrap<TContact>(SourceRow).Title;
                    contact.Phone1 = PXResult.Unwrap<TContact>(SourceRow).Phone1;
                    contact.Phone1Type = PXResult.Unwrap<TContact>(SourceRow).Phone1Type;
                    contact.Phone2 = PXResult.Unwrap<TContact>(SourceRow).Phone2;
                    contact.Phone2Type = PXResult.Unwrap<TContact>(SourceRow).Phone2Type;
                    contact.Phone3 = PXResult.Unwrap<TContact>(SourceRow).Phone3;
                    contact.Phone3Type = PXResult.Unwrap<TContact>(SourceRow).Phone3Type;
                    contact.Fax = PXResult.Unwrap<TContact>(SourceRow).Fax;
                    contact.FaxType = PXResult.Unwrap<TContact>(SourceRow).FaxType;
                    contact.Email = PXResult.Unwrap<TContact>(SourceRow).Email;

                    contactFound = contact.BAccountContactID != null && contact.BAccountID != null && contact.RevisionID != null;

                    if (contact.ContactID == null)
                    {
                        contact = (FSContact)sender.Graph.Caches[typeof(FSContact)].Insert(contact);
                        sender.SetValue(DocumentRow, FieldName, contact.ContactID);
                    }
                    else if (ContactRow == null)
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
                    sender.SetValue(DocumentRow, FieldName, PXResult.Unwrap<TContact>(SourceRow).ContactID);
                    contactFound = PXResult.Unwrap<FSContact>(SourceRow).ContactID != null;
                }
            }

            return contactFound;
        }
    }

    public class FSSrvOrdAddressAttribute : FSDocumentAddressAttribute
    {
        public FSSrvOrdAddressAttribute(Type SelectType)
            : base(SelectType)
        {
        }

        public override void DefaultAddress<TAddress, TAddressID>(PXCache sender, object DocumentRow, object AddressRow)
        {
            PXView view = null;
            object parm = null;

            FSSrvOrdType fsSrvOrdTypeRow = null;
            bool isBranchLocation = false;

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
                            && sender.GetValue<FSServiceOrder.branchLocationID>(DocumentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.branchLocationID>(DocumentRow);

                BqlCommand Select = BqlCommand.CreateInstance(typeof(
                                    Select2<
                                        FSBLOCAddress,
                                        InnerJoin<FSBranchLocation,
                                            On<FSBranchLocation.branchLocationAddressID, Equal<FSBLOCAddress.addressID>>>,
                                        Where<FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>));
                view = sender.Graph.TypedViews.GetView(Select, false);
                isBranchLocation = true;
            }
            else if (fsSrvOrdTypeRow != null
                         && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.CUSTOMER_CONTACT
                             && sender.GetValue<FSServiceOrder.contactID>(DocumentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.contactID>(DocumentRow);
                BqlCommand Select = BqlCommand.CreateInstance(typeof(
                    Select2<Address,
                    LeftJoin<Contact, On<Contact.defAddressID, Equal<Address.addressID>>,
                           LeftJoin<FSAddress,
                         On<FSAddress.bAccountID,
                             Equal<Contact.bAccountID>,
                             And<FSAddress.bAccountAddressID,
                                 Equal<Address.addressID>,
                                 And<FSAddress.revisionID, Equal<Address.revisionID>,
                                     And<FSAddress.isDefaultAddress, Equal<boolTrue>>>>>>>,
                     Where<Contact.contactID, Equal<Required<FSServiceOrder.contactID>>>>));
                        view = sender.Graph.TypedViews.GetView(Select, false);
            }

            else if (fsSrvOrdTypeRow != null
                        && fsSrvOrdTypeRow.AppAddressSource == ID.SrvOrdType_AppAddressSource.BUSINESS_ACCOUNT
                            && sender.GetValue<FSServiceOrder.locationID>(DocumentRow) != null)
            {
                parm = sender.GetValue<FSServiceOrder.locationID>(DocumentRow);
                BqlCommand Select = BqlCommand.CreateInstance(typeof(
                    Select2<Address,
                    LeftJoin<Contact, On<Contact.defAddressID, Equal<Address.addressID>>,
                        LeftJoin<Location, On<Location.locationID, Equal<Required<FSManufacturer.locationID>>>,
                           LeftJoin<FSAddress,
                         On<FSAddress.bAccountID,
                             Equal<Contact.bAccountID>,
                             And<FSAddress.bAccountAddressID,
                                 Equal<Address.addressID>,
                                 And<FSAddress.revisionID, Equal<Address.revisionID>,
                                     And<FSAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>,
                    Where<Address.addressID, Equal<Location.defAddressID>>>));
                view = sender.Graph.TypedViews.GetView(Select, false);
            }

            if (view != null)
            {
                int startRow = -1;
                int totalRows = 0;
                bool addressFound = false;
                foreach (PXResult res in view.Select(new object[] { DocumentRow }, new[] { parm }, null, null, null, null, ref startRow, 1, ref totalRows))
                {
                    if (isBranchLocation)
                    {
                        addressFound = DefaultBLOCAddress<FSBLOCAddress, FSBLOCAddress.addressID>(sender, FieldName, DocumentRow, AddressRow, res);
                    }
                    else
                    {
                        addressFound = DefaultAddress<TAddress, TAddressID>(sender, FieldName, DocumentRow, AddressRow, res);
                    }
                    

                    break;
                }

                if (!addressFound && !_Required)
                    this.ClearRecord(sender, DocumentRow);
            }

            else
            {
                ClearRecord(sender, DocumentRow);
                if (_Required && sender.GetValue(DocumentRow, _FieldOrdinal) == null)
                {
                    using (ReadOnlyScope rs = new ReadOnlyScope(sender.Graph.Caches[_RecordType]))
                    {
                        object record = sender.Graph.Caches[_RecordType].Insert();
                        object recordid = sender.Graph.Caches[_RecordType].GetValue(record, _RecordID);

                        sender.SetValue(DocumentRow, _FieldOrdinal, recordid);
                    }
                }
            }
        }


        public static bool DefaultBLOCAddress<TAddress, TAddressID>(PXCache sender, string FieldName, object DocumentRow, object AddressRow, object SourceRow)
            where TAddress : class, IBqlTable, IAddress, new()
            where TAddressID : IBqlField
        {
            bool addressFound = false;
            if (SourceRow != null)
            {
                FSAddress address = AddressRow as FSAddress;

                if (address == null)
                {
                    address = ((FSAddress)PXSelect<
                                FSAddress,
                                Where<
                                        FSAddress.addressID, Equal<Required<TAddressID>>>>
                                .Select(sender.Graph, sender.GetValue(DocumentRow, FieldName)));
                }

                if (PXResult.Unwrap<FSAddress>(SourceRow)?.AddressID == null || sender.GetValue(DocumentRow, FieldName) == null)
                {
                    if (address == null || address.AddressID > 0)
                    {
                        address = new FSAddress();
                    }

                    address.BAccountAddressID = PXResult.Unwrap<TAddress>(SourceRow).AddressID;
                    address.BAccountID = PXResult.Unwrap<TAddress>(SourceRow).BAccountID;
                    address.RevisionID = PXResult.Unwrap<TAddress>(SourceRow).RevisionID;
                    address.IsDefaultAddress = true;
                    address.AddressLine1 = PXResult.Unwrap<TAddress>(SourceRow).AddressLine1;
                    address.AddressLine2 = PXResult.Unwrap<TAddress>(SourceRow).AddressLine2;
                    address.AddressLine3 = PXResult.Unwrap<TAddress>(SourceRow).AddressLine3;
                    address.City = PXResult.Unwrap<TAddress>(SourceRow).City;
                    address.State = PXResult.Unwrap<TAddress>(SourceRow).State;
                    address.PostalCode = PXResult.Unwrap<TAddress>(SourceRow).PostalCode;
                    address.CountryID = PXResult.Unwrap<TAddress>(SourceRow).CountryID;
                    address.IsValidated = PXResult.Unwrap<TAddress>(SourceRow).IsValidated;
                    addressFound = address.BAccountAddressID != null && address.BAccountID != null && address.RevisionID != null;

                    if (address.AddressID == null)
                    {
                        address = (FSAddress)sender.Graph.Caches[typeof(FSAddress)].Insert(address);
                        sender.SetValue(DocumentRow, FieldName, address.AddressID);
                    }
                    else if (AddressRow == null)
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
                    sender.SetValue(DocumentRow, FieldName, PXResult.Unwrap<TAddress>(SourceRow).AddressID);
                    addressFound = PXResult.Unwrap<FSAddress>(SourceRow).AddressID != null;
                }
            }

            return addressFound;
        }
    }
}
