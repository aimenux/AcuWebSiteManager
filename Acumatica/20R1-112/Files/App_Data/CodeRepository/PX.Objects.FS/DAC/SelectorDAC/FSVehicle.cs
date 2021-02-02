using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    [System.Serializable]
    [PXPrimaryGraph(typeof(VehicleMaintBridge))]
    public class FSVehicle : FSEquipment
    {
        #region SMEquipmentID
        public new abstract class SMequipmentID : PX.Data.BQL.BqlInt.Field<SMequipmentID> { }
        #endregion

        #region RefNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Vehicle ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FSEquipment.refNbr, Where<FSEquipment.isVehicle, Equal<True>>>),
                    new Type[]
                    {
                        typeof(FSEquipment.refNbr), 
                        typeof(FSEquipment.status),
                        typeof(FSEquipment.descr),
                        typeof(FSEquipment.registrationNbr),
                        typeof(FSEquipment.manufacturerModelID),
                        typeof(FSEquipment.manufacturerID),
                        typeof(FSEquipment.manufacturingYear),
                        typeof(FSEquipment.color)
                    },
                    DescriptionField = typeof(FSEquipment.descr))]
        [AutoNumber(typeof(Search<FSSetup.equipmentNumberingID>), typeof(AccessInfo.businessDate))]
        public override string RefNbr { get; set; }
        #endregion

        #region SerialNumber
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "VIN")]
        public override string SerialNumber { get; set; }
        #endregion

        #region RegistrationNbr
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "License Nbr.")]
        public override string RegistrationNbr { get; set; }
        #endregion
        #region Attributes
        /// <summary>
        /// A service field, which is necessary for the <see cref="CSAnswers">dynamically 
        /// added attributes</see> defined at the <see cref="FSVehicleType">Vehicle
        /// screen</see> level to function correctly.
        /// </summary>
        [CRAttributesField(typeof(FSVehicle.vehicleTypeCD), typeof(FSVehicle.noteID))]
        public override string[] Attributes { get; set; }
        #endregion

        #region SourceID
        public new abstract class sourceID : PX.Data.BQL.BqlInt.Field<sourceID> { }
        #endregion

        #region SourceType
        public new abstract class sourceType : ListField_SourceType_Equipment
        {
        }
        #endregion

        #region Memory Helper
        #region VehicleTypeCD
        // Needed for attributes
        public abstract class vehicleTypeCD : PX.Data.BQL.BqlString.Field<vehicleTypeCD> { }

        [PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", IsFixed = true)]
        [PXUIField(Visible = false)]
        [PXDBScalar(typeof(Search<FSVehicleType.vehicleTypeCD, Where<FSVehicleType.vehicleTypeID, Equal<FSVehicle.vehicleTypeID>>>))]
        [PXDefault(typeof(Search<FSVehicleType.vehicleTypeCD, Where<FSVehicleType.vehicleTypeID, Equal<Current<FSVehicle.vehicleTypeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string VehicleTypeCD { get; set; }
        #endregion
        #endregion
    }
}
