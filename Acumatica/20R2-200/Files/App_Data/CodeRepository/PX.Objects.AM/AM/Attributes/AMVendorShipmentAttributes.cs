using PX.Data;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.CR;
using System;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    public class AMVendorShipmentContactAttribute : ContactAttribute
    {
        /// <summary>
        /// Ctor. Internaly, it expects POShipContact as a POContact type
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Contact record from which PO Contact is defaulted and for selecting version of POContact, <br/>
        /// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
        /// - so it must include both records. See example above. <br/>
        /// </param>		
		public AMVendorShipmentContactAttribute(Type SelectType)
            : base(typeof(AMVendorShipmentContact.contactID), typeof(AMVendorShipmentContact.isDefaultContact), SelectType)
        {

        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            sender.Graph.FieldVerifying.AddHandler<AMVendorShipmentContact.overrideContact>(Record_Override_FieldVerifying);
        }


        public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
        {
            DefaultContact<AMVendorShipmentContact, AMVendorShipmentContact.contactID>(sender, DocumentRow, Row);
        }
        public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
        {
            CopyContact<AMVendorShipmentContact, AMVendorShipmentContact.contactID>(sender, DocumentRow, SourceRow, clone);
        }
        protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            base.Record_RowSelected(sender, e);
            if (e.Row != null)
            {
                PXUIFieldAttribute.SetEnabled<AMVendorShipmentContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
            }
        }

        public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
        }

        public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
            try
            {
                Contact_IsDefaultContact_FieldVerifying<AMVendorShipmentContact>(sender, e);
            }
            finally
            {
                e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
            }
        }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            AMVendorShipment row = e.Row as AMVendorShipment;
            if (row != null && row.ShipDestType != POShipDestType.Site)
            {
                var errors = PXUIFieldAttribute.GetErrors(sender, e.Row, new PXErrorLevel[] { PXErrorLevel.Error });
                if (errors != null && errors.Count > 0)
                    return;
            }

            base.RowPersisting(sender, e);
        }

        public override void DefaultContact<TContact, TContactID>(PXCache sender, object DocumentRow, object AddressRow)
        {
            int? siteID = (int?)sender.GetValue<AMVendorShipment.siteID>(DocumentRow);
            string shipDestType = (string)sender.GetValue<AMVendorShipment.shipDestType>(DocumentRow);
            PXView view;
            bool issitebranch = false;
            if (siteID != null && shipDestType == POShippingDestination.Site)
            {
                issitebranch = true;
                BqlCommand altSelect = BqlCommand.CreateInstance(typeof(
                    Select2<Contact,
                        InnerJoin<INSite,
                            On2<INSite.FK.Contact,
                            And<INSite.siteID, Equal<Current<AMVendorShipment.siteID>>>>,
                        LeftJoin<AMVendorShipmentContact, On<AMVendorShipmentContact.bAccountID, Equal<Contact.bAccountID>,
                            And<AMVendorShipmentContact.bAccountContactID, Equal<Contact.contactID>,
                            And<AMVendorShipmentContact.revisionID, Equal<Contact.revisionID>,
                            And<AMVendorShipmentContact.isDefaultContact, Equal<boolTrue>>>>>>>,
                        Where<True, Equal<True>>>));
                view = sender.Graph.TypedViews.GetView(altSelect, false);
            }
            else
            {
                view = sender.Graph.TypedViews.GetView(_Select, false);
            }
            int startRow = -1;
            int totalRows = 0;
            bool contactFound = false;

            foreach (PXResult res in view.Select(new object[] { DocumentRow }, null, null, null, null, null, ref startRow, 1, ref totalRows))
            {
                contactFound = DefaultContact<TContact, TContactID>(sender, FieldName, DocumentRow, AddressRow, res);
                break;
            }

            if (!contactFound && !_Required)
                this.ClearRecord(sender, DocumentRow);

            if (!contactFound && _Required && issitebranch)
                throw new SharedRecordMissingException();

        }

    }

    public class AMVendorShipmentAddressAttribute : AddressAttribute
    {
        /// <summary>
        /// Internaly, it expects POShipAddress as a POAddress type. 
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Address record from which PO address is defaulted and for selecting default version of POAddress, <br/>
        /// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
        /// if it exists - so it must include both records. See example above. <br/>
        /// </param>		
		public AMVendorShipmentAddressAttribute(Type SelectType)
            : base(typeof(AMVendorShipmentAddress.addressID), typeof(AMVendorShipmentAddress.isDefaultAddress), SelectType)
        {

        }
        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            sender.Graph.FieldVerifying.AddHandler<AMVendorShipmentAddress.overrideAddress>(Record_Override_FieldVerifying);
        }


        protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            base.Record_RowSelected(sender, e);
            if (e.Row != null)
            {
                PXUIFieldAttribute.SetEnabled<AMVendorShipmentAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
                PXUIFieldAttribute.SetEnabled<AMVendorShipmentAddress.isValidated>(sender, e.Row, false);
            }
        }

        public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
        {
            DefaultAddress<AMVendorShipmentAddress, AMVendorShipmentAddress.addressID>(sender, DocumentRow, Row);
        }

        public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
        {
            CopyAddress<AMVendorShipmentAddress, AMVendorShipmentAddress.addressID>(sender, DocumentRow, SourceRow, clone);
        }
        public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
        }

        public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
            try
            {
                Address_IsDefaultAddress_FieldVerifying<AMVendorShipmentAddress>(sender, e);
            }
            finally
            {
                e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
            }
        }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            AMVendorShipment row = e.Row as AMVendorShipment;
            if (row != null && row.ShipDestType != POShipDestType.Site)
            {
                var errors = PXUIFieldAttribute.GetErrors(sender, e.Row, new PXErrorLevel[] { PXErrorLevel.Error });
                if (errors != null && errors.Count > 0)
                    return;
            }

            base.RowPersisting(sender, e);
        }

        public override void DefaultAddress<TAddress, TAddressID>(PXCache sender, object DocumentRow, object AddressRow)
        {
            int? siteID = (int?)sender.GetValue<AMVendorShipment.siteID>(DocumentRow);
            string shipDestType = (string)sender.GetValue<AMVendorShipment.shipDestType>(DocumentRow);
            PXView view;
            bool issitebranch = false;
            if (siteID != null && shipDestType == POShippingDestination.Site)
            {
                issitebranch = true;
                BqlCommand altSelect = BqlCommand.CreateInstance(
                    typeof(Select2<Address,
                                    InnerJoin<INSite,
                                        On2<INSite.FK.Address,
                                        And<INSite.siteID, Equal<Current<AMVendorShipment.siteID>>>>,
                                    LeftJoin<AMVendorShipmentAddress, On<AMVendorShipmentAddress.bAccountID, Equal<Address.bAccountID>,
                                        And<AMVendorShipmentAddress.bAccountAddressID, Equal<Address.addressID>,
                                        And<AMVendorShipmentAddress.revisionID, Equal<Address.revisionID>,
                                        And<AMVendorShipmentAddress.isDefaultAddress, Equal<boolTrue>>>>>>>,
                                    Where<True, Equal<True>>>));
                view = sender.Graph.TypedViews.GetView(altSelect, false);
            }
            else
            {
                view = sender.Graph.TypedViews.GetView(_Select, false);
            }

            int startRow = -1;
            int totalRows = 0;
            bool addressFind = false;
            foreach (PXResult res in view.Select(new object[] { DocumentRow }, null, null, null, null, null, ref startRow, 1, ref totalRows))
            {
                addressFind = DefaultAddress<TAddress, TAddressID>(sender, FieldName, DocumentRow, AddressRow, res);
                break;
            }

            if (!addressFind && !_Required)
                this.ClearRecord(sender, DocumentRow);

            if (!addressFind && _Required && issitebranch)
                throw new SharedRecordMissingException();
        }


    }
}
