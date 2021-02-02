using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.FS
{
	[System.SerializableAttribute]
	public class FSAppointmentResource : PX.Data.IBqlTable
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
                            Where<FSAppointment.srvOrdType, Equal<Current<FSAppointmentResource.srvOrdType>>,
                                And<FSAppointment.refNbr, Equal<Current<FSAppointmentResource.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt]
        [PXDBLiteDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Ref. Nbr.")]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region SMEquipmentID
        public abstract class SMequipmentID : PX.Data.BQL.BqlInt.Field<SMequipmentID> { }

		[PXDBInt(IsKey = true)]
        [FSSelectorServiceOrderResourceEquipment]
        [PXRestrictor(typeof(Where<FSEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
                        TX.Messages.EQUIPMENT_IS_INSTATUS, typeof(FSEquipment.status))]
		[PXUIField(DisplayName = "Equipment ID")]
        [PXDefault] 
		public virtual int? SMEquipmentID { get; set; }
		#endregion
		#region Comment
		public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }

		[PXDBString(250)]
		[PXUIField(DisplayName = "Comment", Enabled = false)]
		public virtual string Comment { get; set; }
		#endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlInt.Field<qty> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Quantity", Enabled = false)]
        [PXDefault(1)]
        public virtual int? Qty { get; set; }
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

        #region SMEquipmentIDReport
        public abstract class sMEquipmentIDReport : PX.Data.BQL.BqlInt.Field<sMEquipmentIDReport> { }

        [PXInt]
        [PXSelector(typeof(Search<FSEquipment.SMequipmentID,
                           Where<FSEquipment.resourceEquipment, Equal<True>>>),
                           SubstituteKey = typeof(FSEquipment.refNbr),
                           DescriptionField = typeof(FSEquipment.descr))]
        public virtual int? SMEquipmentIDReport { get; set; }
        #endregion
    }
}