﻿using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
    [PXTable(IsOptional = true)]
    public class FSxARTran : PXCacheExtension<ARTran>, IPostDocLineExtension
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Source
        public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(Enabled = false)]
        public virtual string Source { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXUIField(DisplayName = "Service Order Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<FSServiceOrder.sOID>), SubstituteKey = typeof(FSServiceOrder.refNbr))]
        [PXDBInt]
        public virtual int? SOID { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXUIField(DisplayName = "Appointment Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<FSAppointment.appointmentID>), SubstituteKey = typeof(FSAppointment.refNbr))]
        [PXDBInt]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region AppDetID
        public abstract class appDetID : PX.Data.BQL.BqlInt.Field<appDetID> { }

        [PXDBInt]
        public virtual int? AppDetID { get; set; }
        #endregion
        #region SODetID
        public abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        [PXDBInt]
        public virtual int? SODetID { get; set; }
        #endregion
        #region AppointmentDate
        public abstract class appointmentDate : PX.Data.BQL.BqlDateTime.Field<appointmentDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Service Appointment Date", Enabled = false)]
        public virtual DateTime? AppointmentDate { get; set; }
        #endregion
        #region ServiceOrderDate
        public abstract class serviceOrderDate : PX.Data.BQL.BqlDateTime.Field<serviceOrderDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Service Order Date", Enabled = false)]
        public virtual DateTime? ServiceOrderDate { get; set; }
        #endregion

        // TODO: rename this to CustomerID
        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Customer ID")]
        [FSSelectorCustomer]
        public virtual int? BillCustomerID { get; set; }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSxARTran.billCustomerID>>>),
                    DescriptionField = typeof(Location.descr), DisplayName = "Location ID", Enabled = false, DirtyRead = true)]
        public virtual int? CustomerLocationID { get; set; }
        #endregion
        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXUIField(DisplayName = "Service Contract ID", Enabled = false, FieldClass = "FSCONTRACT")]
        [PXDBInt]
        public virtual int? ServiceContractID { get; set; }
        #endregion
        #region ContractPeriodID
        public abstract class contractPeriodID : PX.Data.BQL.BqlInt.Field<contractPeriodID> { }

        [PXDBInt]
        public virtual int? ContractPeriodID { get; set; }
        #endregion

        #region Equipment Customization
        #region SuspendedSMEquipmentID
        public abstract class suspendedSMEquipmentID : PX.Data.BQL.BqlInt.Field<suspendedSMEquipmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Suspended Target Equipment ID", FieldClass = FSSetup.EquipmentManagementFieldClass, Enabled = false)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<FSEquipment.SMequipmentID>), SubstituteKey = typeof(FSEquipment.refNbr))]
        public virtual int? SuspendedSMEquipmentID { get; set; }
        #endregion
        #region SMEquipmentID
        public abstract class sMEquipmentID : PX.Data.BQL.BqlInt.Field<sMEquipmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Target Equipment ID", Visible = false, FieldClass = FSSetup.EquipmentManagementFieldClass, Enabled = false)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search2<FSEquipment.SMequipmentID,
                    CrossJoinSingleTable<FSSetup>,
                    Where2<
                        Where<
                            FSSetup.enableAllTargetEquipment, Equal<True>,
                            And<FSEquipment.requireMaintenance, Equal<True>>>,
                    Or<
                        Where<
                            FSEquipment.ownerID, Equal<Current<ARTran.customerID>>,
                            And<FSEquipment.status, Equal<FSEquipment.status.Active>>>>>>),
                    SubstituteKey = typeof(FSEquipment.refNbr),
                    DescriptionField = typeof(FSEquipment.descr))]
        public virtual int? SMEquipmentID { get; set; }
        #endregion
        #region NewTargetEquipmentLineNbr
        public abstract class newTargetEquipmentLineNbr : PX.Data.BQL.BqlInt.Field<newTargetEquipmentLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Model Equipment Line Nbr.", Visible = false, FieldClass = FSSetup.EquipmentManagementFieldClass, Enabled = false)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorNewTargetEquipmentSOInvoice]
        public virtual int? NewTargetEquipmentLineNbr { get; set; }
        #endregion
        #region ComponentID
        public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component ID", FieldClass = FSSetup.EquipmentManagementFieldClass, Enabled = false)]
        [PXSelector(typeof(Search<FSModelTemplateComponent.componentID>), SubstituteKey = typeof(FSModelTemplateComponent.componentCD))]
        public virtual int? ComponentID { get; set; }
        #endregion
        #region EquipmentLineRef
        public abstract class equipmentLineRef : PX.Data.BQL.BqlInt.Field<equipmentLineRef> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component Line Ref.", Visible = false, FieldClass = FSSetup.EquipmentManagementFieldClass, Enabled = false)]
        [FSSelectorEquipmentLineRefSOInvoice]
        public virtual int? EquipmentLineRef { get; set; }
        #endregion
        #region EquipmentAction
        public abstract class equipmentAction : ListField_EquipmentAction
        {
        }

        [PXDBString(2, IsFixed = true)]
        [equipmentAction.ListAtrribute]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Equipment Action", Visible = false, FieldClass = FSSetup.EquipmentManagementFieldClass, Enabled = false)]
        public virtual string EquipmentAction { get; set; }
        #endregion
        #region Comment
        public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }

        [PXDBString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Equipment Action Comment", FieldClass = FSSetup.EquipmentManagementFieldClass, Visible = false, Enabled = false)]
        [SkipSetExtensionVisibleInvisible]
        public virtual string Comment { get; set; }
        #endregion
        #region EquipmentItemClass
        public abstract class equipmentItemClass : PX.Data.BQL.BqlString.Field<equipmentItemClass>
        {
        }

        [PXString(2, IsFixed = true)]
        public virtual string EquipmentItemClass { get; set; }
        #endregion
        #endregion

        #region Mem_PreviousPostID
        public abstract class mem_PreviousPostID : PX.Data.BQL.BqlInt.Field<mem_PreviousPostID> { }

        [PXInt]
        public virtual int? Mem_PreviousPostID { get; set; }
        #endregion
        #region Mem_TableSource
        public abstract class mem_TableSource : PX.Data.BQL.BqlString.Field<mem_TableSource> { }

        [PXString]
        public virtual string Mem_TableSource { get; set; }
        #endregion
    }
}
