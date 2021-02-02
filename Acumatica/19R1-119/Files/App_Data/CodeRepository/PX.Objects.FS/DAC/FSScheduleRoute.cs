using System;
using PX.Data;

namespace PX.Objects.FS
{
	[System.SerializableAttribute]
	public class FSScheduleRoute : PX.Data.IBqlTable
	{
		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt(IsKey = true)]
        [PXDBLiteDefault(typeof(FSSchedule.scheduleID))]
        [PXParent(typeof(Select<FSSchedule, Where<FSSchedule.scheduleID, Equal<Current<FSScheduleRoute.scheduleID>>>>))]
        public virtual int? ScheduleID { get; set; }
		#endregion
        #region DfltRouteID
        public abstract class dfltRouteID : PX.Data.BQL.BqlInt.Field<dfltRouteID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Route ID")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [FSSelectorRouteID]
        public virtual int? DfltRouteID { get; set; }
        #endregion
		#region GlobalSequence
		public abstract class globalSequence : PX.Data.BQL.BqlString.Field<globalSequence> { }

        [PXDBString(5, IsUnicode = true)]
        [PXUIField(DisplayName = "Order")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string GlobalSequence { get; set; }
		#endregion
		#region RouteIDFriday
		public abstract class routeIDFriday : PX.Data.BQL.BqlInt.Field<routeIDFriday> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Route Friday")]
        [FSSelectorRouteID]
        public virtual int? RouteIDFriday { get; set; }
		#endregion
		#region RouteIDMonday
		public abstract class routeIDMonday : PX.Data.BQL.BqlInt.Field<routeIDMonday> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Route Monday")]
        [FSSelectorRouteID]
        public virtual int? RouteIDMonday { get; set; }
		#endregion
		#region RouteIDSaturday
		public abstract class routeIDSaturday : PX.Data.BQL.BqlInt.Field<routeIDSaturday> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Route Saturday")]
        [FSSelectorRouteID]
        public virtual int? RouteIDSaturday { get; set; }
		#endregion
		#region RouteIDSunday
		public abstract class routeIDSunday : PX.Data.BQL.BqlInt.Field<routeIDSunday> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Route Sunday")]
        [FSSelectorRouteID]
        public virtual int? RouteIDSunday { get; set; }
		#endregion
		#region RouteIDThursday
		public abstract class routeIDThursday : PX.Data.BQL.BqlInt.Field<routeIDThursday> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Route Thursday")]
        [FSSelectorRouteID]
        public virtual int? RouteIDThursday { get; set; }
		#endregion
		#region RouteIDTuesday
		public abstract class routeIDTuesday : PX.Data.BQL.BqlInt.Field<routeIDTuesday> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Route Tuesday")]
        [FSSelectorRouteID]
        public virtual int? RouteIDTuesday { get; set; }
		#endregion
		#region RouteIDWednesday
		public abstract class routeIDWednesday : PX.Data.BQL.BqlInt.Field<routeIDWednesday> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Route Wednesday")]
        [FSSelectorRouteID]
        public virtual int? RouteIDWednesday { get; set; }
		#endregion
		#region SequenceFriday
		public abstract class sequenceFriday : PX.Data.BQL.BqlString.Field<sequenceFriday> { }

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Sequence Friday")]
        public virtual string SequenceFriday { get; set; }
		#endregion
		#region SequenceMonday
		public abstract class sequenceMonday : PX.Data.BQL.BqlString.Field<sequenceMonday> { }

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Sequence Monday")]
        public virtual string SequenceMonday { get; set; }
		#endregion
		#region SequenceSaturday
		public abstract class sequenceSaturday : PX.Data.BQL.BqlString.Field<sequenceSaturday> { }

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Sequence Saturday")]
        public virtual string SequenceSaturday { get; set; }
		#endregion
		#region SequenceSunday
		public abstract class sequenceSunday : PX.Data.BQL.BqlString.Field<sequenceSunday> { }

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Sequence Sunday")]
        public virtual string SequenceSunday { get; set; }
		#endregion
		#region SequenceThursday
		public abstract class sequenceThursday : PX.Data.BQL.BqlString.Field<sequenceThursday> { }

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Sequence Thursday")]
        public virtual string SequenceThursday { get; set; }
		#endregion
		#region SequenceTuesday
		public abstract class sequenceTuesday : PX.Data.BQL.BqlString.Field<sequenceTuesday> { }

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Sequence Tuesday")]
        public virtual string SequenceTuesday { get; set; }
		#endregion
		#region SequenceWednesday
		public abstract class sequenceWednesday : PX.Data.BQL.BqlString.Field<sequenceWednesday> { }

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Sequence Wednesday")]
        public virtual string SequenceWednesday { get; set; }
		#endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region DeliveryNotes
        public abstract class deliveryNotes : PX.Data.BQL.BqlString.Field<deliveryNotes> { }

        [PXDBString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Delivery Notes")]
        public virtual string DeliveryNotes { get; set; }
        #endregion
        #region InternalNotes
        public abstract class internalNotes : PX.Data.BQL.BqlString.Field<internalNotes> { }

        [PXDBString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Internal Notes")]
        public virtual string InternalNotes { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXUIField(DisplayName = "CreatedByID")]
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXUIField(DisplayName = "CreatedByScreenID")]
        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXUIField(DisplayName = "CreatedDateTime")]
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXUIField(DisplayName = "LastModifiedByID")]
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXUIField(DisplayName = "LastModifiedDateTime")]
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region SortingIndex
        public abstract class sortingIndex : PX.Data.BQL.BqlInt.Field<sortingIndex> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Sorting Index")]
        public virtual int? SortingIndex { get; set; }
        #endregion
	}
}
