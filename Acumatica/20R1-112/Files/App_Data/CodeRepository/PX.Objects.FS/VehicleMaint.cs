using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using System;

namespace PX.Objects.FS
{
    public class VehicleMaint : PXGraph<VehicleMaint, EPEquipment>
    {
        #region Selects
        public PXSelectJoin<EPEquipment,
               LeftJoin<FSVehicle,
               On<
                   FSVehicle.sourceID, Equal<EPEquipment.equipmentID>,
                   And<FSVehicle.sourceType, Equal<FSVehicle.sourceType.Vehicle>>>>,
               Where<
                   FSVehicle.SMequipmentID, IsNotNull>> EPEquipmentRecords;

        public PXSelect<FSVehicle,
               Where<
                   FSVehicle.sourceID, Equal<Current<EPEquipment.equipmentID>>,
                   And<FSVehicle.sourceType, Equal<FSVehicle.sourceType.Vehicle>>>> VehicleSelected;

        [PXViewName(CR.Messages.Answers)]
        public CRAttributeList<FSVehicle> Answers;

        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;
        #endregion

        public VehicleMaint()
            : base()
        {
            if (SetupRecord.Current == null
                    || SetupRecord.Current.EquipmentNumberingID == null)
            {
                throw new PXSetupNotEnteredException(TX.Error.EQUIPMENT_NUMBERING_SEQUENCE_MISSING_IN_X, typeof(FSEquipmentSetup), DACHelper.GetDisplayName(typeof(FSEquipmentSetup)));
            }
        }

        #region CacheAttached
        #region EPEquipment_EquipmentCD
        [PXDefault]
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXSelector(
            typeof(
                Search2<EPEquipment.equipmentCD,
                InnerJoin<FSVehicle,
                    On<FSVehicle.sourceID, Equal<EPEquipment.equipmentID>,
                    And<FSVehicle.sourceType, Equal<FSVehicle.sourceType.Vehicle>>>>>),
            new Type[] {
                    typeof(EPEquipment.equipmentCD),
                    typeof(FSVehicle.status),
                    typeof(FSVehicle.descr),
                    typeof(FSVehicle.registrationNbr),
                    typeof(FSVehicle.manufacturerModelID),
                    typeof(FSVehicle.manufacturerID),
                    typeof(FSVehicle.manufacturingYear),
                    typeof(FSVehicle.color)
        })]
        [AutoNumber(typeof(Search<FSSetup.equipmentNumberingID>), typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Vehicle ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void EPEquipment_EquipmentCD_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region EPEquipment_BranchLocationID
        [PXDBInt]
        [PXDefault(
            typeof(
                Search<FSxUserPreferences.dfltBranchLocationID,
                Where<
                    PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
                    And<PX.SM.UserPreferences.defBranchID, Equal<Current<AccessInfo.branchID>>>>>))]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID,
                           Where<
                                FSBranchLocation.branchID, Equal<Current<AccessInfo.branchID>>>>),
                    SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
                    DescriptionField = typeof(FSBranchLocation.descr))]
        protected virtual void EPEquipment_BranchLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSVehicle_RefNbr
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
        protected virtual void FSVehicle_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSVehicle_SourceID
        [PXDBInt]
        [PXDBDefault(typeof(EPEquipment.equipmentID))]
        [PXDBChildIdentity(typeof(EPEquipment.equipmentID))]
        protected virtual void FSVehicle_SourceID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSVehicle_SourceType
        [PXDBString(3, IsFixed = true)]
        [PXDefault(ID.SourceType_Equipment.VEHICLE)]
        [PXUIField(DisplayName = "Source Type", Enabled = false)]
        [FSVehicle.sourceType.ListAtrribute]
        public virtual void FSVehicle_SourceType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSVehicle_IsVehicle
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Is Vehicle", Enabled = false)]
        protected virtual void FSVehicle_IsVehicle_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSVehicle_VehicleTypeID
        [PXDBInt]
        [PXUIField(DisplayName = "Vehicle Type ID")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(FSVehicleType.vehicleTypeID), SubstituteKey = typeof(FSVehicleType.vehicleTypeCD))]
        public virtual void FSVehicle_VehicleTypeID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSVehicle_RequireMaintenance
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Target Equipment")]
        protected virtual void FSVehicle_RequireMaintenance_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Event Handlers

        #region FSVehicle

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSVehicle, FSVehicle.vehicleTypeID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSVehicle fsVehicleRow = (FSVehicle)e.Row;
            e.Cache.SetDefaultExt<FSVehicle.vehicleTypeCD>(fsVehicleRow);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSVehicle> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSVehicle> e)
        {
        }

        protected virtual void _(Events.RowInserting<FSVehicle> e)
        {
            if (e.Row == null || EPEquipmentRecords.Current == null)
            {
                return;
            }

            FSVehicle fsVehicleRow = (FSVehicle)e.Row;
            fsVehicleRow.RefNbr = EPEquipmentRecords.Current.EquipmentCD;
        }

        protected virtual void _(Events.RowInserted<FSVehicle> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSVehicle> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSVehicle> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSVehicle fsVehicleRow = (FSVehicle)e.Row;

            if (EquipmentHelper.UpdateEPEquipmentWithFSEquipment(EPEquipmentRecords.Cache, EPEquipmentRecords.Current, e.Cache, fsVehicleRow))
            {
                EPEquipmentRecords.Cache.Update(EPEquipmentRecords.Current);
            }
        }

        protected virtual void _(Events.RowDeleting<FSVehicle> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSVehicle> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSVehicle> e)
        {
            if (e.Row == null || EPEquipmentRecords.Current == null)
            {
                return;
            }

            FSVehicle fsVehicleRow = (FSVehicle)e.Row;

            if (e.Operation != PXDBOperation.Delete && fsVehicleRow.IsVehicle == true)
            {
                LicenseHelper.CheckVehiclesLicense(e.Cache.Graph, fsVehicleRow.SMEquipmentID, fsVehicleRow.Status);
            }

            if (e.Operation == PXDBOperation.Insert)
            {
                fsVehicleRow.RefNbr = EPEquipmentRecords.Current.EquipmentCD;
                fsVehicleRow.SourceRefNbr = EPEquipmentRecords.Current.EquipmentCD;
                fsVehicleRow.SourceID = EPEquipmentRecords.Current.EquipmentID;
            }
        }

        protected virtual void _(Events.RowPersisted<FSVehicle> e)
        {
        }

        #endregion

        #region FSVehicle

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<EPEquipment, EPEquipment.fixedAssetID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEquipment epEquipmentRow = (EPEquipment)e.Row;

            if (epEquipmentRow.FixedAssetID != null)
            {
                EquipmentHelper.SetDefaultValuesFromFixedAsset(VehicleSelected.Cache, VehicleSelected.Current, epEquipmentRow.FixedAssetID);
            }
        }


        #endregion

        protected virtual void _(Events.RowSelecting<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowSelected<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowInserting<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowInserted<EPEquipment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEquipment epEquipmentRow = (EPEquipment)e.Row;

            // Inserting the corresponding Equipment record
            if (VehicleSelected.Current == null)
            {
                FSVehicle fsVehicleRow = new FSVehicle();
                fsVehicleRow = VehicleSelected.Insert(fsVehicleRow);
                fsVehicleRow.SourceID = epEquipmentRow.EquipmentID;
                this.VehicleSelected.Cache.IsDirty = false;
            }
        }

        protected virtual void _(Events.RowUpdating<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowUpdated<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowDeleting<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowDeleted<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowPersisting<EPEquipment> e)
        {
        }

        protected virtual void _(Events.RowPersisted<EPEquipment> e)
        {
            if (e.Row == null || VehicleSelected.Current == null)
            {
                return;
            }

            EPEquipment epEquipmentRow = (EPEquipment)e.Row;

            if (epEquipmentRow.EquipmentID != VehicleSelected.Current.SourceID)
            {
                return;
            }

            if (e.Operation == PXDBOperation.Delete && e.TranStatus == PXTranStatus.Open)
            {
                try
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        VehicleSelected.Delete(VehicleSelected.Current);
                        VehicleSelected.Cache.Persist(PXDBOperation.Delete);
                        ts.Complete();
                    }

                    VehicleSelected.Cache.Persisted(false);
                }
                catch
                {
                    VehicleSelected.Cache.Persisted(true);
                    throw;
                }
            }
        }

        #endregion

        #endregion
    }
}