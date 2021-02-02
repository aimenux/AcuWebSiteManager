﻿namespace PX.Objects.FS
{
	using System;
	using PX.Data;

    [System.SerializableAttribute]
	public class FSSOAttendee : PX.Data.IBqlTable
	{
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSServiceOrder.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Service Order Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSServiceOrder.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSServiceOrder,
                            Where<FSServiceOrder.srvOrdType, Equal<Current<FSSOAttendee.srvOrdType>>,
                                And<FSServiceOrder.refNbr, Equal<Current<FSSOAttendee.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
		#region AttendeeID
		public abstract class attendeeID : PX.Data.BQL.BqlInt.Field<attendeeID> { }
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual int? AttendeeID { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }
        [PXDBInt]
        [PXDBLiteDefault(typeof(FSServiceOrder.sOID))]
        [PXUIField(DisplayName = "Service Order Ref. Nbr.")]
        public virtual int? SOID { get; set; }
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
        [PXFormula(typeof(Default<FSSOAttendee.customerID>))]
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

        #region GetStaticSOAttendeeRows
        /// <summary>
        /// Executes a static select to retrieve a PXResulset instance with FSSOAttendee records associated to a service order
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="sOID"></param>
        /// <returns></returns>
        public static PXResultset<FSSOAttendee> GetStaticSOAttendeeRows(PXGraph graph, int? sOID)
        {
            PXResultset<FSSOAttendee> bqlResultSet = new PXResultset<FSSOAttendee>();

            var fsSOAttendeeSet = PXSelect<FSSOAttendee,
                                  Where<
                                        FSSOAttendee.sOID, Equal<Required<FSSOAttendee.sOID>>>>
                                  .Select(graph, sOID);

            foreach (FSSOAttendee fsSOAttendeeRow in fsSOAttendeeSet)
            {
                bqlResultSet.Add(new PXResult<FSSOAttendee>(fsSOAttendeeRow));
            }

            return bqlResultSet;
        }
        #endregion
    }
}
