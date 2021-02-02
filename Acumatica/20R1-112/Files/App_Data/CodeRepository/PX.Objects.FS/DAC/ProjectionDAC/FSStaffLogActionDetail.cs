using PX.Data;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXProjection(typeof(Select2<FSAppointmentEmployee,
                         LeftJoin<Vendor,
                         On<
                             Vendor.bAccountID, Equal<FSAppointmentEmployee.employeeID>>,
                         LeftJoin<EPEmployee,
                         On<
                             EPEmployee.bAccountID, Equal<FSAppointmentEmployee.employeeID>>,
                         LeftJoin<FSAppointmentDet,
                         On<
                             FSAppointmentDet.lineRef, Equal<FSAppointmentEmployee.serviceLineRef>,
                             And<FSAppointmentDet.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>>>>>>))]
    public class FSStaffLogActionDetail : IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true, BqlField = typeof(FSAppointmentEmployee.srvOrdType))]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSAppointment.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "", BqlField = typeof(FSAppointmentEmployee.refNbr))]
        [PXUIField(DisplayName = "Appointment Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSAppointment.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSAppointment,
                            Where<FSAppointment.srvOrdType, Equal<Current<FSAppointmentEmployee.srvOrdType>>,
                                And<FSAppointment.refNbr, Equal<Current<FSAppointmentEmployee.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region DocID
        public abstract class docID : PX.Data.BQL.BqlInt.Field<docID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentEmployee.appointmentID))]
        [PXDBLiteDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Ref. Nbr.", Visible = false, Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.StaffAssignment>>>))]
        public virtual int? DocID { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true, BqlField = typeof(FSAppointmentEmployee.lineNbr))]
        [PXLineNbr(typeof(FSAppointment))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentEmployee.employeeID))]
        [PXUIField(DisplayName = "Staff Member")]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIVisible(typeof(Where<
                                Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                                And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.StaffAssignment>,
                                And<Current<FSLogActionFilter.me>, Equal<False>>>>))]
        public virtual int? BAccountID { get; set; }
        #endregion
        #region LineRef
        public abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(3, IsFixed = true, BqlField = typeof(FSAppointmentEmployee.lineRef))]
        [PXUIField(DisplayName = "Staff Line Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.StaffAssignment>>>))]
        public virtual string LineRef { get; set; }
        #endregion
        #region InventoryID
        public abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentDet.inventoryID))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<InventoryItem.inventoryID>), SubstituteKey = typeof(InventoryItem.inventoryCD))]
        [PXUIField(DisplayName = "Inventory ID", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.StaffAssignment>>>))]
        public virtual int? InventoryID { get; set; }
        #endregion
        #region Descr
        public abstract class descr : Data.BQL.BqlString.Field<descr> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(FSAppointmentDet.tranDesc))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.StaffAssignment>>>))]
        public virtual string Descr { get; set; }
        #endregion
        #region EstimatedDuration
        public abstract class estimatedDuration : PX.Data.BQL.BqlInt.Field<estimatedDuration> { }

        [FSDBTimeSpanLong(BqlField = typeof(FSAppointmentDet.estimatedDuration))]
        [PXUIField(DisplayName = "Estimated Duration")]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.StaffAssignment>>>))]
        public virtual int? EstimatedDuration { get; set; }
        #endregion
        #region UserID
        public abstract class userID : Data.BQL.BqlGuid.Field<userID> { }
        [PXDBGuid(BqlField = typeof(EPEmployee.userID))]
        [PXUIField(Enabled = false, Visible = false)]
        public virtual Guid? UserID { get; set; }
        #endregion
        #region DetLineRef
        public abstract class detLineRef : PX.Data.BQL.BqlString.Field<detLineRef> { }

        [PXDBString(4, IsFixed = true, BqlField = typeof(FSAppointmentDet.lineRef))]
        [PXUIField(DisplayName = "Detail Line Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string DetLineRef { get; set; }
        #endregion
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        [PXFormula(typeof(Switch<Case<Where<userID, Equal<Current<AccessInfo.userID>>>, True>, False>))]
        [PXUIVisible(typeof(Where<
                                Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                                And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.StaffAssignment>>>))]
        public virtual bool? Selected { get; set; }
        #endregion
    }
}
