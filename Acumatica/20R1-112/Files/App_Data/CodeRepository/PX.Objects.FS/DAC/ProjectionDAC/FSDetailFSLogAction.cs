using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXProjection(typeof(Select<FSAppointmentDet,
                         Where<
                             FSAppointmentDet.lineType, Equal<FSLineType.Service>,
                             And<FSAppointmentDet.isCanceledNotPerformed, NotEqual<True>>>>))]
    public class FSDetailFSLogAction : IBqlTable
    {
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.SrvBasedOnAssignment>>>))]
        public virtual bool? Selected { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true, BqlField = typeof(FSAppointmentDet.srvOrdType))]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSAppointment.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "", BqlField = typeof(FSAppointmentDet.refNbr))]
        [PXUIField(DisplayName = "Appointment Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSAppointment.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSAppointment,
                         Where<
                             FSAppointment.srvOrdType, Equal<Current<FSAppointmentDet.srvOrdType>>,
                         And<
                             FSAppointment.refNbr, Equal<Current<FSAppointmentDet.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentDet.appointmentID))]
        [PXDBLiteDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region AppDetID
        public abstract class appDetID : PX.Data.BQL.BqlInt.Field<appDetID> { }

        [PXDBIdentity(BqlField = typeof(FSAppointmentDet.appDetID))]
        public virtual int? AppDetID { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true, BqlField = typeof(FSAppointmentDet.lineNbr))]
        [PXLineNbr(typeof(FSAppointment.lineCntr))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region SODetID
        public abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        [PXDBInt(BqlField = typeof(FSAppointmentDet.sODetID))]
        [PXCheckUnique(Where = typeof(Where<appointmentID, Equal<Current<FSAppointment.appointmentID>>>))]
        [PXUIField(DisplayName = "Service Order Line Ref.", Visible = false)]
        [FSSelectorSODetID]
        public virtual int? SODetID { get; set; }
        #endregion
        #region LineRef
        public abstract class lineRef : Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(4, IsFixed = true, BqlField = typeof(FSAppointmentDet.lineRef))]
        [PXUIField(DisplayName = "Detail Line Ref.", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.SrvBasedOnAssignment>>>))]
        public virtual string LineRef { get; set; }
        #endregion
        #region InventoryID
        public abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBInt(BqlField = typeof(FSAppointmentDet.inventoryID))]
        [PXSelector(typeof(Search<InventoryItem.inventoryID>), SubstituteKey = typeof(InventoryItem.inventoryCD))]
        [PXUIField(DisplayName = "Inventory ID", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.SrvBasedOnAssignment>>>))]
        public virtual int? InventoryID { get; set; }
        #endregion
        #region Descr
        public abstract class descr : Data.BQL.BqlString.Field<descr> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(FSAppointmentDet.tranDesc))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.SrvBasedOnAssignment>>>))]
        public virtual string Descr { get; set; }
        #endregion
        #region EstimatedDuration
        public abstract class estimatedDuration : Data.BQL.BqlInt.Field<estimatedDuration> { }

        [FSDBTimeSpanLong(BqlField = typeof(FSAppointmentDet.estimatedDuration))]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimated Duration", Enabled = false)]
        [PXUIVisible(typeof(Where<Current<FSLogActionFilter.action>, Equal<ListField_LogActions.Start>,
                            And<Current<FSLogActionFilter.type>, Equal<FSLogTypeAction.SrvBasedOnAssignment>>>))]
        public virtual int? EstimatedDuration { get; set; }
        #endregion
    }
}
