using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class FSSrvOrdTypeAttributeList : IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXString(4, IsKey = true, InputMask = ">AAAA", IsFixed = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion

        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXInt()]
        public virtual int? SOID { get; set; }
        #endregion

        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXInt()]
        public virtual int? AppointmentID { get; set; }
        #endregion

        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXInt()]
        public virtual int? ScheduleID { get; set; }
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXNote(new Type[0], BqlField = typeof(FSServiceOrder.noteID))]
        public virtual Guid? NoteID { get; set; }
        #endregion

        #region Attributes
        /// <summary>
        /// A service field, which is necessary for the <see cref="CSAnswers">dynamically 
        /// added attributes</see> defined at the <see cref="FSSrvOrdType">customer 
        /// class</see> level to function correctly.
        /// </summary>
        [CRAttributesField(typeof(FSSrvOrdTypeAttributeList.srvOrdType), typeof(FSSrvOrdTypeAttributeList.noteID))]
        public virtual string[] Attributes { get; set; }
        #endregion
    }
}
