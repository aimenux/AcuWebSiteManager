using Autofac;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.Objects.CS;
using PX.Objects.Extensions.ContactAddress;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Compilation;

namespace PX.Objects.FS
{
    public class BranchLocationMaint : PXGraph<BranchLocationMaint, FSBranchLocation>
    {
        #region Selects
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        public PXSelect<FSBranchLocation> BranchLocationRecords;

        public PXSelect<FSBranchLocation, Where<FSBranchLocation.branchLocationID, Equal<Current<FSBranchLocation.branchLocationID>>>> CurrentBranchLocation;

        [PXViewName(TX.TableName.FSADDRESS)]
        public PXSelect<FSAddress, Where<FSAddress.addressID, Equal<Current<FSBranchLocation.branchLocationAddressID>>>> BranchLocation_Address;

        [PXViewName(TX.TableName.FSCONTACT)]
        public PXSelect<FSContact, Where<FSContact.contactID, Equal<Current<FSBranchLocation.branchLocationContactID>>>> BranchLocation_Contact;

        public PXSelect<FSRoom, 
                        Where<
                            FSRoom.branchLocationID, Equal<Current<FSBranchLocation.branchLocationID>>>> 
                        RoomRecords;

        public PXSelect<PX.Objects.GL.Branch, Where<PX.Objects.GL.Branch.branchID, Equal<Current<FSBranchLocation.branchID>>>> Branch;
        #endregion

        #region CacheAttached
        #region FSRoom_RoomID
        [PXDefault]
        [PXDBString(10, IsUnicode = true, InputMask = ">AAAAAAAAAA")]
        [PXUIField(DisplayName = "Room ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void FSRoom_RoomID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSContact_EntityType
        [PXDBString(4, IsFixed = true)]
        [PXDefault(ID.ACEntityType.BRANCH_LOCATION)]
        [PXUIField(DisplayName = "Entity Type", Visible = false, Enabled = false)]
        protected virtual void FSContact_EntityType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAddress_EntityType
        [PXDBString(4, IsFixed = true)]
        [PXDefault(ID.ACEntityType.BRANCH_LOCATION)]
        [PXUIField(DisplayName = "Entity Type", Visible = false, Enabled = false)]
        protected virtual void FSAddress_EntityType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Action Buttons
        #region ViewOnMap
        public PXAction<FSBranchLocation> viewMainOnMap;
        [PXUIField(DisplayName = CR.Messages.ViewOnMap)]
        [PXButton]
        public virtual void ViewMainOnMap()
        {
            var address = BranchLocation_Address.SelectSingle();
            if (address != null)
            {
                CR.BAccountUtility.ViewOnMap<FSAddress, FSAddress.countryID>(address);
            }
        }
        #endregion
        #region Open Room
        [PXViewDetailsButton(typeof(FSBranchLocation))]
        public PXAction<FSBranchLocation> openRoom;
        [PXUIField(DisplayName = "Open Room", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable OpenRoom(PXAdapter adapter)
        {
            FSRoom fsRoomRow = RoomRecords.Current;

            if (fsRoomRow != null)
            {
                RoomMaint graphRoomServiceClassMaint = PXGraph.CreateInstance<RoomMaint>();
                graphRoomServiceClassMaint.RoomRecords.Current = graphRoomServiceClassMaint.RoomRecords.Search<FSRoom.roomID>(fsRoomRow.RoomID, fsRoomRow.BranchLocationID);

                throw new PXRedirectRequiredException(graphRoomServiceClassMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }
        #endregion
        #region Validate Address
        public PXAction<FSBranchLocation> validateAddress;
        [PXUIField(DisplayName = "Validate Address", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
        [PXButton]
        public virtual IEnumerable ValidateAddress(PXAdapter adapter)
        {
            foreach (FSBranchLocation current in adapter.Get<FSBranchLocation>())
            {
                if (current != null)
                {
                    FSAddress address = this.BranchLocation_Address.Select();
                    if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
                    {
                        PXAddressValidator.Validate<FSAddress>(this, address, true);
                    }
                }
                yield return current;
            }
        }
        #endregion
        #endregion

        #region FSBranchLocationEventHanldlers
        protected void FSBranchLocation_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSBranchLocation fsBranchLocationRow = (FSBranchLocation)e.Row;

            if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>()
                    && !PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
            {
                fsBranchLocationRow.DfltSiteID = null;
            }
        }

        protected void FSBranchLocation_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSBranchLocation fsBranchLocationRow = (FSBranchLocation)e.Row;
            EnableDisable_ActionButtons(cache, fsBranchLocationRow);

            PXDefaultAttribute.SetPersistingCheck<FSBranchLocation.dfltSiteID>(
                cache,
                fsBranchLocationRow,
                GetPersistingCheckValueForDfltSiteID(PXAccess.FeatureInstalled<FeaturesSet.warehouse>()
                                                        || PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()));
        }

        protected virtual void FSRoom_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoom fsRoomRow = (FSRoom)e.Row;

            if (fsRoomRow.RoomID != null && !string.IsNullOrEmpty(fsRoomRow.Descr))
            {
                PXUIFieldAttribute.SetEnabled<FSRoom.roomID>(cache, fsRoomRow, false);
                PXUIFieldAttribute.SetEnabled<FSRoom.descr>(cache, fsRoomRow, false);
                PXUIFieldAttribute.SetEnabled<FSRoom.floorNbr>(cache, fsRoomRow, false);
            }
        }

        protected virtual void FSRoom_RoomID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            FSRoom fsRoomRow = (FSRoom)e.Row;
            FSRoom fsRoom_tmp = PXSelect<FSRoom,
                                Where<
                                    FSRoom.branchLocationID, Equal<Required<FSRoom.branchLocationID>>,
                                    And<FSRoom.roomID, Equal<Required<FSRoom.roomID>>>>>
                                .Select(cache.Graph, fsRoomRow.BranchLocationID, e.NewValue);

            if (fsRoom_tmp != null)
            { 
                e.Cancel = true;
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// The Action buttons get enabled or disabled.
        /// </summary>
        private void EnableDisable_ActionButtons(PXCache cache, FSBranchLocation fsBranchLocationRow)
        {
            //Validate address action
            if (BranchLocation_Address.Current != null)
            {
                if (BranchLocation_Address.Current.CountryID != null && BranchLocation_Address.Current.City != null
                    && BranchLocation_Address.Current.State != null && BranchLocation_Address.Current.PostalCode != null
                    && BranchLocation_Address.Current.IsValidated == false)
                {
                    validateAddress.SetEnabled(true);
                }
                else
                {
                    validateAddress.SetEnabled(false);
                }
            }
        }

        /// <summary>
        /// Checks if the distribution module is enable and return the corresponding PersistingCheck value.
        /// </summary>
        /// <returns>PXPersistingCheck.NullOrBlank is the distribution module is enabled otherwise returns PXPersistingCheck.Nothing.</returns>
        private PXPersistingCheck GetPersistingCheckValueForDfltSiteID(bool isDistributionModuleEnabled)
        {
            return isDistributionModuleEnabled ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing; 
        }

        #endregion

        #region Extensions
        public class ContactAddress : ContactAddressGraph<BranchLocationMaint, FSContact, FSAddress>
        {
            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(FSBranchLocation))
                {
                    DocumentAddressID = typeof(FSBranchLocation.branchLocationAddressID),
                    DocumentContactID = typeof(FSBranchLocation.branchLocationContactID)
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
                return Base.BranchLocation_Contact.Cache;
            }
            protected override PXCache GetAddressCache()
            {
                return Base.BranchLocation_Address.Cache;
            }
            protected override FSContact GetCurrentContact()
            {
                var contact = Base.BranchLocation_Contact.SelectSingle();
                return contact;
            }
            protected override FSContact GetEtalonContact()
            {
                bool isDirty = Base.BranchLocation_Contact.Cache.IsDirty;
                var contact = Base.BranchLocation_Contact.Insert();
                Base.BranchLocation_Contact.Cache.SetStatus(contact, PXEntryStatus.Held);
                Base.BranchLocation_Contact.Cache.IsDirty = isDirty;
                return contact;
            }

            protected override FSAddress GetCurrentAddress()
            {
                var address = Base.BranchLocation_Address.SelectSingle();
                return address;
            }
            protected override FSAddress GetEtalonAddress()
            {
                bool isDirty = Base.BranchLocation_Address.Cache.IsDirty;
                var address = Base.BranchLocation_Address.Insert();
                Base.BranchLocation_Address.Cache.SetStatus(address, PXEntryStatus.Held);
                Base.BranchLocation_Address.Cache.IsDirty = isDirty;
                return address;
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