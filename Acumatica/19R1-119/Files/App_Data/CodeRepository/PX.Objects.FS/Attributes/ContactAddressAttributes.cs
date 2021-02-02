using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    public abstract class FSContactAttribute : ContactAttribute, IPXRowUpdatedSubscriber
    {
        #region State
        BqlCommand _DuplicateSelect = BqlCommand.CreateInstance(typeof(Select<FSContact, Where<FSContact.bAccountID, Equal<Required<FSContact.bAccountID>>, And<FSContact.bAccountContactID, Equal<Required<FSContact.bAccountContactID>>, And<FSContact.revisionID, Equal<Required<FSContact.revisionID>>, And<FSContact.isDefaultContact, Equal<boolTrue>>>>>>));
        #endregion
        #region Ctor
        public FSContactAttribute(Type AddressIDType, Type IsDefaultAddressType, Type SelectType)
            : base(AddressIDType, IsDefaultAddressType, SelectType)
        {
        }
        #endregion
        #region Implementation

        public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            bool previsDirty = sender.IsDirty;
            base.RowInserted(sender, e);
            sender.IsDirty = previsDirty;
        }                

        public override void DefaultContact<TContact, TContactID>(PXCache sender, object DocumentRow, object ContactRow)
        {
            PXView view = null;
            object parm = null;

            if (sender.GetValue<FSManufacturer.contactID> (DocumentRow) != null)
            {
                parm = sender.GetValue<FSManufacturer.contactID>(DocumentRow);
                BqlCommand Select = BqlCommand.CreateInstance(typeof(
                        Select2<Contact,
                 LeftJoin<FSContact,
                     On<FSContact.bAccountID,
                         Equal<Contact.bAccountID>,
                         And<FSContact.bAccountContactID,
                             Equal<Contact.contactID>,
                             And<FSContact.revisionID, Equal<Contact.revisionID>,
                                 And<FSContact.isDefaultContact, Equal<boolTrue>>>>>>,
                    Where<Contact.contactID, Equal<Required<FSManufacturer.contactID>>>>));
                view = sender.Graph.TypedViews.GetView(Select, false);
            }
            else if(sender.GetValue<FSManufacturer.locationID>(DocumentRow) != null)
            {
                parm = sender.GetValue<FSManufacturer.locationID>(DocumentRow);
                BqlCommand Select = BqlCommand.CreateInstance(typeof(
                                                                Select2<
                                                                    Contact,
                                                                    LeftJoin<Location, 
                                                                        On<Location.locationID, Equal<Required<FSManufacturer.locationID>>>,
                                                                    LeftJoin<FSContact,
                                                                        On<FSContact.bAccountID, Equal<Contact.bAccountID>,
                                                                            And<FSContact.bAccountContactID, Equal<Contact.contactID>,
                                                                            And<FSContact.revisionID, Equal<Contact.revisionID>,
                                                                            And<FSContact.isDefaultContact, Equal<boolTrue>>>>>>>,
                                                                    Where<Contact.contactID, Equal<Location.defContactID>>>));
                view = sender.Graph.TypedViews.GetView(Select, false);
            }

            if (view != null)
            {
                int startRow = -1;
                int totalRows = 0;

                bool contactFound = false;
                foreach (PXResult res in view.Select(new object[] { DocumentRow }, new [] {parm}, null, null, null, null, ref startRow, 1, ref totalRows))
                {
                    contactFound = DefaultContact<TContact, TContactID>(sender, FieldName, DocumentRow, ContactRow, res);
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

        public override void Record_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && ((FSContact)e.Row).IsDefaultContact == true)
            {
                PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
                view.Clear();

                FSContact prev_address = (FSContact)view.SelectSingle(((FSContact)e.Row).BAccountID, ((FSContact)e.Row).BAccountContactID, ((FSContact)e.Row).RevisionID);
                if (prev_address != null)
                {
                    _KeyToAbort = sender.GetValue(e.Row, _RecordID);
                    object newkey = sender.Graph.Caches[typeof(FSContact)].GetValue(prev_address, _RecordID);

                    PXCache cache = sender.Graph.Caches[_ItemType];

                    foreach (object data in cache.Updated)
                    {
                        object datakey = cache.GetValue(data, _FieldOrdinal);
                        if (Equals(_KeyToAbort, datakey))
                        {
                            cache.SetValue(data, _FieldOrdinal, newkey);
                        }
                    }

                    _KeyToAbort = null;
                    e.Cancel = true;
                    return;
                }
            }
            base.Record_RowPersisting(sender, e);
        }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            object key = sender.GetValue(e.Row, _FieldOrdinal);
            if (key != null)
            {
                PXCache cache = sender.Graph.Caches[_RecordType];
                if (Convert.ToInt32(key) < 0)
                {
                    foreach (object data in cache.Inserted)
                    {
                        object datakey = cache.GetValue(data, _RecordID);
                        if (Equals(key, datakey))
                        {
                            if (((FSContact)data).IsDefaultContact == true)
                            {
                                PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
                                view.Clear();

                                FSContact prev_address = (FSContact)view.SelectSingle(((FSContact)data).BAccountID, ((FSContact)data).BAccountContactID, ((FSContact)data).RevisionID);

                                if (prev_address != null)
                                {
                                    _KeyToAbort = sender.GetValue(e.Row, _FieldOrdinal);
                                    object id = sender.Graph.Caches[typeof(FSContact)].GetValue(prev_address, _RecordID);
                                    sender.SetValue(e.Row, _FieldOrdinal, id);
                                }
                            }
                            break;
                        }
                    }
                }
            }
            base.RowPersisting(sender, e);
        }
        
        public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (_Required && sender.GetValue(e.Row, _FieldOrdinal) == null)
            {
                using (ReadOnlyScope rs = new ReadOnlyScope(sender.Graph.Caches[_RecordType]))
                {
                    object record = sender.Graph.Caches[_RecordType].Insert();
                    object recordid = sender.Graph.Caches[_RecordType].GetValue(record, _RecordID);

                    sender.SetValue(e.Row, _FieldOrdinal, recordid);
                }
            }
        }        
        #endregion
    }

    public abstract class FSAddressAttribute : AddressAttribute, IPXRowUpdatedSubscriber
    {
        #region State
        BqlCommand _DuplicateSelect = BqlCommand.CreateInstance(typeof(Select<FSAddress, Where<FSAddress.bAccountID, Equal<Required<FSAddress.bAccountID>>, And<FSAddress.bAccountAddressID, Equal<Required<FSAddress.bAccountAddressID>>, And<FSAddress.revisionID, Equal<Required<FSAddress.revisionID>>, And<FSAddress.isDefaultAddress, Equal<boolTrue>>>>>>));
        #endregion
        #region Ctor
        public FSAddressAttribute(Type AddressIDType, Type IsDefaultAddressType, Type SelectType)
            : base(AddressIDType, IsDefaultAddressType, SelectType)
        {
        }
        #endregion

        public override void DefaultAddress<TAddress, TAddressID>(PXCache sender, object DocumentRow, object AddressRow)            
        {
            PXView view = null;
            object param = null;
            if (sender.GetValue<FSManufacturer.contactID>(DocumentRow) != null)
            {
                param = sender.GetValue<FSManufacturer.contactID>(DocumentRow);
                BqlCommand Select = BqlCommand.CreateInstance(typeof(
                                                                Select2<
                                                                    Address,
                                                                    LeftJoin<Contact, 
                                                                        On<Contact.defAddressID, Equal<Address.addressID>>,
                                                                    LeftJoin<FSAddress,
                                                                        On<FSAddress.bAccountID, Equal<Contact.bAccountID>,
                                                                            And<FSAddress.bAccountAddressID, Equal<Address.addressID>,
                                                                            And<FSAddress.revisionID, Equal<Address.revisionID>,
                                                                            And<FSAddress.isDefaultAddress, Equal<boolTrue>>>>>>>,
                                                                    Where<Contact.contactID, Equal<Required<FSManufacturer.contactID>>>>));
                view = sender.Graph.TypedViews.GetView(Select, false);                
            }
            else if (sender.GetValue<FSManufacturer.locationID>(DocumentRow) != null)
            {
                param = sender.GetValue<FSManufacturer.locationID>(DocumentRow);
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
                bool addressFind = false;
                foreach (PXResult res in view.Select(new object[] { DocumentRow }, new [] {param}, null, null, null, null, ref startRow, 1, ref totalRows))
                {
                    addressFind = DefaultAddress<TAddress, TAddressID>(sender, FieldName, DocumentRow, AddressRow, res);
                    break;
                }

                if (!addressFind && !_Required)
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

        #region Implementation
        public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            bool previsDirty = sender.Graph.Caches[_RecordType].IsDirty;
            base.RowInserted(sender, e);
            sender.Graph.Caches[_RecordType].IsDirty = previsDirty;
        }

        public override void Record_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && ((FSAddress)e.Row).IsDefaultAddress == true)
            {
                PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
                view.Clear();

                FSAddress prev_address = (FSAddress)view.SelectSingle(((FSAddress)e.Row).BAccountID, ((FSAddress)e.Row).BAccountAddressID, ((FSAddress)e.Row).RevisionID);
                if (prev_address != null)
                {
                    _KeyToAbort = sender.GetValue(e.Row, _RecordID);
                    object newkey = sender.Graph.Caches[typeof(FSAddress)].GetValue(prev_address, _RecordID);

                    PXCache cache = sender.Graph.Caches[_ItemType];

                    foreach (object data in cache.Updated)
                    {
                        object datakey = cache.GetValue(data, _FieldOrdinal);
                        if (Equals(_KeyToAbort, datakey))
                        {
                            cache.SetValue(data, _FieldOrdinal, newkey);
                        }
                    }

                    _KeyToAbort = null;
                    e.Cancel = true;
                    return;
                }
            }
            base.Record_RowPersisting(sender, e);
        }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            object key = sender.GetValue(e.Row, _FieldOrdinal);
            if (key != null)
            {
                PXCache cache = sender.Graph.Caches[_RecordType];
                if (Convert.ToInt32(key) < 0)
                {
                    foreach (object data in cache.Inserted)
                    {
                        object datakey = cache.GetValue(data, _RecordID);
                        if (Equals(key, datakey))
                        {
                            if (((FSAddress)data).IsDefaultAddress == true)
                            {
                                PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
                                view.Clear();

                                FSAddress prev_address = (FSAddress)view.SelectSingle(((FSAddress)data).BAccountID, ((FSAddress)data).BAccountAddressID, ((FSAddress)data).RevisionID);

                                if (prev_address != null)
                                {
                                    _KeyToAbort = sender.GetValue(e.Row, _FieldOrdinal);
                                    object id = sender.Graph.Caches[typeof(FSAddress)].GetValue(prev_address, _RecordID);
                                    sender.SetValue(e.Row, _FieldOrdinal, id);
                                }
                            }
                            break;
                        }
                    }
                }
            }
            base.RowPersisting(sender, e);
        }

        public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (_Required && sender.GetValue(e.Row, _FieldOrdinal) == null)
            {
                using (ReadOnlyScope rs = new ReadOnlyScope(sender.Graph.Caches[_RecordType]))
                {
                    object record = sender.Graph.Caches[_RecordType].Insert();
                    object recordid = sender.Graph.Caches[_RecordType].GetValue(record, _RecordID);

                    sender.SetValue(e.Row, _FieldOrdinal, recordid);
                }
            }
        }        
        #endregion
    }

    public class FSDocumentContactAttribute : FSContactAttribute
    {
        public FSDocumentContactAttribute(Type SelectType)
            : base(typeof(FSContact.contactID), typeof(FSContact.isDefaultContact), SelectType)
        {
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            sender.Graph.FieldVerifying.AddHandler<FSContact.overrideContact>(Record_Override_FieldVerifying);
        }

        public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
        {
            DefaultContact<FSContact, FSContact.contactID>(sender, DocumentRow, Row);
        }

        public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
        {
            CopyContact<FSContact, FSContact.contactID>(sender, DocumentRow, SourceRow, clone);
        }

        public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
        }

        public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
            try
            {
                Contact_IsDefaultContact_FieldVerifying<FSContact>(sender, e);
            }
            finally
            {
                e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
            }
        }

        protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            base.Record_RowSelected(sender, e);

            if (e.Row != null)
            {
                PXUIFieldAttribute.SetEnabled<FSContact.overrideContact>(sender, e.Row, true);
            }
        }
    }

    public class FSDocumentAddressAttribute : FSAddressAttribute, IPXRowUpdatedSubscriber
    {
        public FSDocumentAddressAttribute(Type SelectType)
            : base(typeof(FSAddress.addressID), typeof(FSAddress.isDefaultAddress), SelectType)
        {
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            sender.Graph.FieldVerifying.AddHandler<FSAddress.overrideAddress>(Record_Override_FieldVerifying);
        }

        public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
        {
            DefaultAddress<FSAddress, FSAddress.addressID>(sender, DocumentRow, Row);
        }

        public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
        {
            CopyAddress<FSAddress, FSAddress.addressID>(sender, DocumentRow, SourceRow, clone);
        }

        public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
        }

        public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
            try
            {
                Address_IsDefaultAddress_FieldVerifying<FSAddress>(sender, e);
            }
            finally
            {
                e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
            }
        }

        protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            base.Record_RowSelected(sender, e);

            if (e.Row != null)
            {
                PXUIFieldAttribute.SetEnabled<FSAddress.overrideAddress>(sender, e.Row, true);
                PXUIFieldAttribute.SetEnabled<FSAddress.isValidated>(sender, e.Row, false);
            }
        }       
    }
}
