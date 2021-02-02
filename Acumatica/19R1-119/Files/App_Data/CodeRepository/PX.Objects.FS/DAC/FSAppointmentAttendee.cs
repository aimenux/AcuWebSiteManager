using System;
using PX.Data;

namespace PX.Objects.FS
{
    [Serializable]
	public class FSAppointmentAttendee : PX.Data.IBqlTable
	{
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSAppointment.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Appointment Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSAppointment.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSAppointment,
                            Where<FSAppointment.srvOrdType, Equal<Current<FSAppointmentAttendee.srvOrdType>>,
                                And<FSAppointment.refNbr, Equal<Current<FSAppointmentAttendee.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }
		[PXDBInt]
        [PXDBLiteDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Ref. Nbr.")]
		public virtual int? AppointmentID { get; set; }
		#endregion
		#region AttendeeID
		public abstract class attendeeID : PX.Data.BQL.BqlInt.Field<attendeeID> { }
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual int? AttendeeID { get; set; }
		#endregion
		#region Comment
		public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Comment")]
		public virtual string Comment { get; set; }
		#endregion
		#region Confirmed
		public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Confirmed")]
		public virtual bool? Confirmed { get; set; }
		#endregion
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		[PXDBInt]
        [PXUIField(DisplayName = "Contact ID")]
        [FSSelectorAttendeeContact]
        [PXFormula(typeof(Default<FSAppointmentAttendee.customerID>))]
		public virtual int? ContactID { get; set; }
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Customer ID")]
        [FSSelectorBusinessAccount_CU_PR_VC]
		public virtual int? CustomerID { get; set; }
		#endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public virtual Guid? NoteID { get; set; }
        #endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion

        #region Mem_CustomerContactName
        public abstract class mem_CustomerContactName : PX.Data.BQL.BqlString.Field<mem_CustomerContactName> { }
        [PXString]
        [PXUIField(DisplayName = "Attendee Name", Enabled = false)]
        public virtual string Mem_CustomerContactName { get; set; }
        #endregion
        #region Mem_EMail
        public abstract class mem_EMail : PX.Data.BQL.BqlString.Field<mem_EMail> { }
        [PXString]
        [PXUIField(DisplayName = "Email", Enabled = false)]
        public virtual string Mem_EMail { get; set; }
        #endregion
        #region Mem_Phone1
        public abstract class mem_Phone1 : PX.Data.BQL.BqlString.Field<mem_Phone1> { }
        [PXString]
        [PXUIField(DisplayName = "Phone", Enabled = false)]
        public virtual string Mem_Phone1 { get; set; }
        #endregion
	}
}
