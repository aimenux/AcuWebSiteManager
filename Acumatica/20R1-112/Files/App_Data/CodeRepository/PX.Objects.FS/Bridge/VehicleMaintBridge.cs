using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    public class VehicleMaintBridge : PXGraph<VehicleMaintBridge, FSVehicle>
    {
        #region Selects
        public PXSelect<FSVehicle,
               Where<
                   FSVehicle.isVehicle, Equal<True>>> VehicleRecords;
        #endregion

        #region Event Handlers

        protected virtual void _(Events.RowSelected<FSVehicle> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSVehicle fsVehicleRow = e.Row;

            VehicleMaint graphVehicleMaint = PXGraph.CreateInstance<VehicleMaint>();

            if (fsVehicleRow.SMEquipmentID != null)
            {
                graphVehicleMaint.EPEquipmentRecords.Current = PXSelectJoin<EPEquipment,
                                                               InnerJoin<FSEquipment,
                                                                    On<
                                                                        FSEquipment.sourceID, Equal<EPEquipment.equipmentID>,
                                                                        And<FSEquipment.sourceType, Equal<FSEquipment.sourceType.Vehicle>>>>,
                                                               Where<
                                                                   FSEquipment.SMequipmentID, Equal<Required<FSEquipment.SMequipmentID>>>>
                                                               .Select(graphVehicleMaint, fsVehicleRow.SMEquipmentID);
            }

            throw new PXRedirectRequiredException(graphVehicleMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion
    }
}
