using Autofac;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.Objects.CS;
using PX.Objects.Extensions.ContactAddress;
using System;
using System.Collections.Generic;
using System.Web.Compilation;

namespace PX.Objects.FS
{
    public class ManufacturerMaint : PXGraph<ManufacturerMaint, FSManufacturer>
    {
        #region Selects / Views
        public PXSelect<FSManufacturer> ManufacturerRecords;

        public PXSelect<FSManufacturer, Where<FSManufacturer.manufacturerID, Equal<Current<FSManufacturer.manufacturerID>>>> CurrentManufacturer;

        [PXViewName(TX.TableName.FSCONTACT)]
        public PXSelect<FSContact, Where<FSContact.contactID, Equal<Current<FSManufacturer.manufacturerContactID>>>> Manufacturer_Contact;

        [PXViewName(TX.TableName.FSADDRESS)]
        public PXSelect<FSAddress, Where<FSAddress.addressID, Equal<Current<FSManufacturer.manufacturerAddressID>>>> Manufacturer_Address;
        #endregion

        #region Actions

        public PXAction<FSManufacturer> viewMainOnMap;
        [PXUIField(DisplayName = CR.Messages.ViewOnMap)]
        [PXButton]
        public virtual void ViewMainOnMap()
        {
            var address = Manufacturer_Address.SelectSingle();
            if (address != null)
            {
                CR.BAccountUtility.ViewOnMap<FSAddress, FSAddress.countryID>(address);
            }
        }
        #endregion

        #region CacheAttached
        #region FSContact_EntityType
        [PXDBString(4, IsFixed = true)]
        [PXDefault(ID.ACEntityType.MANUFACTURER)]
        [PXUIField(DisplayName = "Entity Type", Visible = false, Enabled = false)]
        protected virtual void FSContact_EntityType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAddress_EntityType
        [PXDBString(4, IsFixed = true)]
        [PXDefault(ID.ACEntityType.MANUFACTURER)]
        [PXUIField(DisplayName = "Entity Type", Visible = false, Enabled = false)]
        protected virtual void FSAddress_EntityType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Event Handlers
        #region FSManufacturer
        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<FSManufacturer> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSManufacturer> e)
        {
            if (e.Row == null) return;

            FSManufacturer row = e.Row as FSManufacturer;

            Caches[typeof(FSContact)].AllowUpdate = row.AllowOverrideContactAddress == true;
            Caches[typeof(FSAddress)].AllowUpdate = row.AllowOverrideContactAddress == true;

            PXUIFieldAttribute.SetEnabled<FSManufacturer.allowOverrideContactAddress>(e.Cache, row, !(row.ContactID == null));
        }

        protected virtual void _(Events.RowInserting<FSManufacturer> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSManufacturer> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSManufacturer> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSManufacturer> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSManufacturer> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSManufacturer> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSManufacturer> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSManufacturer> e)
        {
        }
        #endregion
        #endregion

        #region Extensions
        public class ContactAddress : ContactAddressGraph<ManufacturerMaint>
        {
            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(FSManufacturer))
                {
                    DocumentAddressID = typeof(FSManufacturer.manufacturerAddressID),
                    DocumentContactID = typeof(FSManufacturer.manufacturerContactID)
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
            protected override PXCache GetContactCache()
            {
                return Base.Manufacturer_Contact.Cache;
            }
            protected override PXCache GetAddressCache()
            {
                return Base.Manufacturer_Address.Cache;
            }
            protected override IPersonalContact GetCurrentContact()
            {
                var contact = Base.Manufacturer_Contact.SelectSingle();
                return contact;
            }
            protected override IPersonalContact GetEtalonContact()
            {
                bool isDirty = Base.Manufacturer_Contact.Cache.IsDirty;
                var contact = Base.Manufacturer_Contact.Insert();
                Base.Manufacturer_Contact.Cache.SetStatus(contact, PXEntryStatus.Held);
                Base.Manufacturer_Contact.Cache.IsDirty = isDirty;
                return contact;
            }

            protected override IAddress GetCurrentAddress()
            {
                var address = Base.Manufacturer_Address.SelectSingle();
                return address;
            }
            protected override IAddress GetEtalonAddress()
            {
                bool isDirty = Base.Manufacturer_Address.Cache.IsDirty;
                var address = Base.Manufacturer_Address.Insert();
                Base.Manufacturer_Address.Cache.SetStatus(address, PXEntryStatus.Held);
                Base.Manufacturer_Address.Cache.IsDirty = isDirty;
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
        }

        public class ServiceRegistration : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.ActivateOnApplicationStart<ExtensionSorting>();
            }
            private class ExtensionSorting
            {
                private static readonly Dictionary<Type, int> _order = new Dictionary<Type, int>
                {
                    {typeof(ContactAddress), 1},
                    /*{typeof(MultiCurrency), 2},
                    {typeof(SalesPrice), 3},
                    {typeof(Discount), 4},
                    {typeof(SalesTax), 5},*/
                };
                public ExtensionSorting()
                {
                    PXBuildManager.SortExtensions += (list) => PXBuildManager.PartialSort(list, _order);
                }
            }
        }
        #endregion
    }
}