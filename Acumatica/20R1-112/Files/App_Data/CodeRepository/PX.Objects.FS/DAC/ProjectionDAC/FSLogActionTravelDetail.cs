using PX.Data;
using PX.Objects.AP;
using PX.Objects.EP;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXProjection(typeof(Select2<FSAppointmentLog,
                         LeftJoin<Vendor,
                         On<
                             Vendor.bAccountID, Equal<FSAppointmentLog.bAccountID>>,
                         LeftJoin<EPEmployee,
                         On<
                             EPEmployee.bAccountID, Equal<FSAppointmentLog.bAccountID>>,
                         LeftJoin<FSAppointmentDet,
                         On<
                             FSAppointmentDet.appointmentID, Equal<FSAppointmentLog.docID>,
                             And<FSAppointmentDet.lineRef, Equal<FSAppointmentLog.detLineRef>>>>>>,
                         Where<
                             FSAppointmentLog.status, Equal<FSAppointmentLog.status.InProcess>,
                             And<FSAppointmentLog.type, Equal<FSLogTypeAction.Travel>>>>))]
    public class FSLogActionTravelDetail : IBqlTable, ILogDetail
    {
        #region DocID
        public abstract class docID : PX.Data.BQL.BqlInt.Field<docID> { }

        [PXDBInt(IsKey = true, BqlField = typeof(FSAppointmentLog.docID))]
        [PXDBLiteDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Ref. Nbr.", Visible = false, Enabled = false)]
        public virtual int? DocID { get; set; }
        #endregion
        #region LineRef
        public abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(3, IsFixed = true, IsKey = true, BqlField = typeof(FSAppointmentLog.lineRef))]
        [PXUIField(DisplayName = "Log Line Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string LineRef { get; set; }
        #endregion
        #region DetLineRef
        public abstract class detLineRef : PX.Data.BQL.BqlString.Field<detLineRef> { }

        [PXDBString(4, IsFixed = true, BqlField = typeof(FSAppointmentLog.detLineRef))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorAppointmentSODetID]
        [PXUIField(DisplayName = "Detail Line Ref.")]
        public virtual string DetLineRef { get; set; }
        #endregion
        #region Descr
        public abstract class descr : Data.BQL.BqlString.Field<descr> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(FSAppointmentLog.descr))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Complete>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.Service>>>))]
        public virtual string Descr { get; set; }
        #endregion
        #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentLog.bAccountID))]
        [PXUIField(DisplayName = "Staff Member")]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.me>, Equal<False>>))]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        public virtual int? BAccountID { get; set; }
        #endregion
        #region Type
        public abstract class type : ListField_Type_Log { }

        [PXDBString(2, IsFixed = true, BqlField = typeof(FSAppointmentLog.type))]
        [type.ListAtrribute]
        public virtual string Type { get; set; }
        #endregion
        #region DateTimeBegin
        public abstract class dateTimeBegin : PX.Data.BQL.BqlDateTime.Field<dateTimeBegin> { }

        [PXDBDateAndTime(UseTimeZone = true, BqlField = typeof(FSAppointmentLog.dateTimeBegin),
                         PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Start Time")]
        [PXUIField(DisplayName = "Start Time", Enabled = false)]
        public virtual DateTime? DateTimeBegin { get; set; }
        #endregion
        #region Travel
        public abstract class travel : PX.Data.BQL.BqlBool.Field<travel> { }

        [PXBool]
        [PXFormula(typeof(Where<type, Equal<type.Travel>>))]
        [PXUIField(DisplayName = "Travel", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Complete>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.Travel>>>))]
        public virtual bool? Travel { get; set; }
        #endregion
        #region InventoryID
        public abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentDet.inventoryID))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search<InventoryItem.inventoryID>), SubstituteKey = typeof(InventoryItem.inventoryCD))]
        [PXUIField(DisplayName = "Inventory ID", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Complete>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.Service>>>))]
        public virtual int? InventoryID { get; set; }
        #endregion
        #region UserID
        public abstract class userID : Data.BQL.BqlGuid.Field<userID> { }
        [PXDBGuid(BqlField = typeof(EPEmployee.userID))]
        [PXUIField(Enabled = false, Visible = false)]
        public virtual Guid? UserID { get; set; }
        #endregion
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXFormula(typeof(Switch<Case<Where<userID, Equal<Current<AccessInfo.userID>>>, True>, False>))]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion
    }
}