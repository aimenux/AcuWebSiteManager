using PX.Data;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSLogActionFilter : IBqlTable
    {
        #region Action
        public abstract class action : ListField_LogActions { }

        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Action")]
        [PXUnboundDefault(ID.LogActions.START)]
        [action.ListAtrribute]
        public virtual string Action { get; set; }
        #endregion
        #region Type
        public abstract class type : Data.BQL.BqlString.Field<type> { }

        [PXString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Logging")]
        [PXUnboundDefault(ID.Type_Log.TRAVEL)]
        [FSLogTypeAction.List]
        public virtual string Type { get; set; }
        #endregion
        #region LogTime
        public abstract class logTime : Data.BQL.BqlDateTime.Field<logTime> { }
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameTime = "Time")]
        [PXUIField(DisplayName = "Time")]
        public virtual DateTime? LogTime { get; set; }
        #endregion
        #region IsTravelAction
        public abstract class isTravelAction : PX.Data.BQL.BqlBool.Field<isTravelAction> { }

        protected Boolean? _IsTravelAction;

        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(Visible = false, Enabled = false)]
        public virtual bool? IsTravelAction
        {
            get
            {
                return Type == ID.Type_Log.TRAVEL;
            }
        }
        #endregion
        #region DetLineRef
        public abstract class detLineRef : Data.BQL.BqlString.Field<detLineRef> { }

        [PXString(4, IsFixed = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIVisible(typeof(Where<
                                Current<action>, Equal<ListField_LogActions.Start>,
                                And<type, NotEqual<FSLogTypeAction.StaffAssignment>,
                                And<type, NotEqual<FSLogTypeAction.SrvBasedOnAssignment>>>>))]
        [PXUIRequired(typeof(Where<
                                Current<action>, Equal<ListField_LogActions.Start>,
                                And<type, NotEqual<FSLogTypeAction.StaffAssignment>,
                                And<type, NotEqual<FSLogTypeAction.SrvBasedOnAssignment>,
                                And<type, NotEqual<FSLogTypeAction.Travel>>>>>))]
        [FSSelectorAppointmentDetLogAction(typeof(isTravelAction))]
        [PXUIField(DisplayName = "Detail Line Ref.")]
        public virtual string DetLineRef { get; set; }
        #endregion
        #region Me
        public abstract class me : PX.Data.BQL.BqlBool.Field<me> { }

        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Perform Action for Me")]
        [PXUIVisible(typeof(Where<type, NotEqual<FSLogTypeAction.SrvBasedOnAssignment>>))]
        public virtual bool? Me { get; set; }
        #endregion
    }
}