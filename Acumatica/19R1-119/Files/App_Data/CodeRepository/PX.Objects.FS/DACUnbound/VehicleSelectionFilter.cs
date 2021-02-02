using System;
using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class VehicleSelectionFilter : IBqlTable
    {
        #region RouteDocumentID
        public abstract class routeDocumentID : PX.Data.BQL.BqlInt.Field<routeDocumentID> { }

        [PXInt]
        [PXUIField(DisplayName = "Route Document ID", Enabled = false)]
        [PXSelector(typeof(FSRouteDocument.routeDocumentID), SubstituteKey = typeof(FSRouteDocument.refNbr))]
        public virtual int? RouteDocumentID { get; set; }
        #endregion
        #region ShowUnassignedVehicles
        public abstract class showUnassignedVehicles : PX.Data.BQL.BqlBool.Field<showUnassignedVehicles> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Show Available Vehicles for this Route only")]
        public virtual bool? ShowUnassignedVehicles { get; set; }
        #endregion
        #region VehicleTypeID
        public abstract class vehicleTypeID : PX.Data.BQL.BqlInt.Field<vehicleTypeID> { }

        [PXInt]
        public virtual int? VehicleTypeID { get; set; }
        #endregion
    }
}
