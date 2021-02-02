using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public class GeoZoneMaint : PXGraph<GeoZoneMaint, FSGeoZone>
    {
        [PXImport(typeof(FSGeoZone))]
        public PXSelect<FSGeoZone> GeoZoneRecords;

        [PXImport(typeof(FSGeoZone))]
        public PXSelect<FSGeoZoneEmp,
               Where<
                   FSGeoZoneEmp.geoZoneID, Equal<Current<FSGeoZone.geoZoneID>>>> GeoZoneEmpRecords;

        [PXImport(typeof(FSGeoZone))]
        public PXSelect<FSGeoZonePostalCode,
               Where<
                   FSGeoZonePostalCode.geoZoneID, Equal<Current<FSGeoZone.geoZoneID>>>> GeoZonePostalCodeRecords;

        #region PrivateMethods

        private void EnableDisableGeoZonePostalCode(PXCache cache, FSGeoZonePostalCode fsGeoZonePostalCodeRow)
        {
            PXUIFieldAttribute.SetEnabled<FSGeoZonePostalCode.postalCode>
                    (cache, fsGeoZonePostalCodeRow, string.IsNullOrEmpty(fsGeoZonePostalCodeRow.PostalCode));
        }

        private void EnableDisableGeoZoneEmployee(PXCache cache, FSGeoZoneEmp fsGeoZoneEmpRow)
        {
            PXUIFieldAttribute.SetEnabled<FSGeoZoneEmp.employeeID>
                    (cache, fsGeoZoneEmpRow, fsGeoZoneEmpRow.EmployeeID == null);
        }

        #endregion        
        #region GeoZoneEmpEventHandlers

        protected virtual void FSGeoZoneEmp_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EnableDisableGeoZoneEmployee(cache, (FSGeoZoneEmp)e.Row);
        }

        #endregion
        #region GeoZonePostalCodeEventHandlers

        protected virtual void FSGeoZonePostalCode_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EnableDisableGeoZonePostalCode(cache, (FSGeoZonePostalCode)e.Row);
        }        

        #endregion        
    }
}
