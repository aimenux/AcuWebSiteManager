using PX.Data;

namespace PX.Objects.FS
{
    public class LicenseTypeMaint : PXGraph<LicenseTypeMaint, FSLicenseType>
    {
        [PXImport(typeof(FSLicenseType))]
        public PXSelect<FSLicenseType> LicenseTypeRecords;

        #region PrivateMethods

        private void EnableDisable(PXCache cache, FSLicenseType fsLicenseTypeRow)
        {
            bool enabledDisable = false;
            enabledDisable = fsLicenseTypeRow.ValidIn == ID.LicenseType_ValidIn.COUNTRY_STATE_CITY;

            PXUIFieldAttribute.SetEnabled<FSLicenseType.countryID>(cache, fsLicenseTypeRow, enabledDisable);
            PXUIFieldAttribute.SetEnabled<FSLicenseType.state>(cache, fsLicenseTypeRow, enabledDisable);
            PXUIFieldAttribute.SetEnabled<FSLicenseType.city>(cache, fsLicenseTypeRow, enabledDisable);
            PXUIFieldAttribute.SetVisible<FSLicenseType.countryID>(cache, fsLicenseTypeRow, enabledDisable);
            PXUIFieldAttribute.SetVisible<FSLicenseType.state>(cache, fsLicenseTypeRow, enabledDisable);
            PXUIFieldAttribute.SetVisible<FSLicenseType.city>(cache, fsLicenseTypeRow, enabledDisable);
            PXDefaultAttribute.SetPersistingCheck<FSLicenseType.countryID>(
                                                                        cache,
                                                                        fsLicenseTypeRow,
                                                                        enabledDisable ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            enabledDisable = fsLicenseTypeRow.ValidIn == ID.LicenseType_ValidIn.GEOGRAPHICAL_ZONE;

            PXUIFieldAttribute.SetEnabled<FSLicenseType.geoZoneID>(cache, fsLicenseTypeRow, enabledDisable);
            PXUIFieldAttribute.SetVisible<FSLicenseType.geoZoneID>(cache, fsLicenseTypeRow, enabledDisable);
            PXDefaultAttribute.SetPersistingCheck<FSLicenseType.geoZoneID>(
                                                                        cache,
                                                                        fsLicenseTypeRow,
                                                                        enabledDisable ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            PXUIFieldAttribute.SetEnabled<FSLicenseType.ownerType>(cache, fsLicenseTypeRow, cache.GetStatus(fsLicenseTypeRow) == PXEntryStatus.Inserted);
        }

        private void SetNullByValidIn(FSLicenseType fsLicenseTypeRow)
        {
            if (fsLicenseTypeRow.ValidIn != ID.LicenseType_ValidIn.GEOGRAPHICAL_ZONE)
            {
                fsLicenseTypeRow.GeoZoneID = null;
            }

            if (fsLicenseTypeRow.ValidIn != ID.LicenseType_ValidIn.COUNTRY_STATE_CITY)
            {
                fsLicenseTypeRow.CountryID = null;
                fsLicenseTypeRow.State = null;
                fsLicenseTypeRow.City = null;
            }
        }

        #endregion
        #region LicenseTypeEventHandlers

        protected virtual void FSLicenseType_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EnableDisable(cache, (FSLicenseType)e.Row);
        }

        protected virtual void FSLicenseType_ValidIn_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetNullByValidIn((FSLicenseType)e.Row);
        }

        #endregion
    }
}