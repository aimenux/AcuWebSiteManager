using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class SM_VendorMaint : PXGraphExtension<VendorMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Event Handlers

        #region Vendor
        protected virtual void _(Events.RowPersisting<Vendor> e)
        {
            if (e.Row == null)
            {
                return;
            }

            var vendorRow = (Vendor)e.Row;
            FSxVendor fsxVendorRow = e.Cache.GetExtension<FSxVendor>(vendorRow);

            if (e.Operation != PXDBOperation.Delete)
            {
                LicenseHelper.CheckStaffMembersLicense(e.Cache.Graph, vendorRow.BAccountID, fsxVendorRow.SDEnabled, vendorRow.Status);
            }
        }
        #endregion

        #endregion
    }
}