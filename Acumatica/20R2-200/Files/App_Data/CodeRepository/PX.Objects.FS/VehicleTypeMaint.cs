using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public class VehicleTypeMaint : PXGraph<VehicleTypeMaint, FSVehicleType>
    {
        #region Select
        public PXSelect<FSVehicleType> VehicleTypeRecords;

        public PXSelect<FSVehicleType,
               Where<
                   FSVehicleType.vehicleTypeID, Equal<Current<FSVehicleType.vehicleTypeID>>>> VehicleTypeSelected;

        [PXViewName(CR.Messages.Attributes)]
        public CSAttributeGroupList<FSVehicleType, FSVehicle> Mapping;
        #endregion

        #region CacheAttached
        #region FSVehicleType_VehicleTypeCD
        [PXDefault]
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", IsFixed = true)]
        [NormalizeWhiteSpace]
        [PXUIField(DisplayName = "Vehicle Type ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSVehicleType.vehicleTypeCD>))]
        protected virtual void FSVehicleType_VehicleTypeCD_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region EntityClassID
        [PXDBString(15, IsUnicode = true, IsKey = true, IsFixed = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Entity Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void CSAttributeGroup_EntityClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }
}