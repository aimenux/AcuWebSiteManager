using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Work Center Shift
    /// </summary>
	[Serializable]
    [PXCacheName(Messages.Shift)]
    public class AMShift : IBqlTable, INotable
    {
        #region Keys
        public class PK : PrimaryKeyOf<AMShift>.By<shiftID, wcID>
        {
            public static AMShift Find(PXGraph graph, string shiftID, string wcID) => FindBy(graph, shiftID, wcID);
        }

        public static class FK
        {
            public class Shift : AMShiftMst.PK.ForeignKeyOf<AMShift>.By<shiftID> { }
            public class WorkCenter : AMWC.PK.ForeignKeyOf<AMShift>.By<wcID> { }
        }        
        #endregion

        #region ShiftID
        public abstract class shiftID : PX.Data.BQL.BqlString.Field<shiftID> { }

        protected String _ShiftID;
        [PXDBString(4, IsKey = true, InputMask = "####")]
        [PXDefault]
        [PXUIField(DisplayName = "Shift")]
        [PXSelector(typeof(Search<AMShiftMst.shiftID>))]
        [PXParent(typeof(Select<AMWC, Where<AMWC.wcID, Equal<Current<AMShift.wcID>>>>))]
        [PXForeignReference(typeof(Field<AMShift.shiftID>.IsRelatedTo<AMShiftMst.shiftID>))]
        public virtual String ShiftID
        {
            get
            {
                return this._ShiftID;
            }
            set
            {
                this._ShiftID = value;
            }
        }
        #endregion
		#region CrewSize
		public abstract class crewSize : PX.Data.BQL.BqlDecimal.Field<crewSize> { }

        protected Decimal? _CrewSize;
		[PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Crew Size")]
        public virtual Decimal? CrewSize
		{
			get
			{
				return this._CrewSize;
			}
			set
			{
				this._CrewSize = value;
			}
		}
		#endregion
        #region MachNbr
        public abstract class machNbr : PX.Data.BQL.BqlDecimal.Field<machNbr> { }

        protected Decimal? _MachNbr;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Machines")]
        public virtual Decimal? MachNbr
        {
            get
            {
                return this._MachNbr;
            }
            set
            {
                this._MachNbr = value;
            }
        }
        #endregion
        #region ShftEff
        public abstract class shftEff : PX.Data.BQL.BqlDecimal.Field<shftEff> { }

        protected Decimal? _ShftEff;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Efficiency")]
        public virtual Decimal? ShftEff
        {
            get
            {
                return this._ShftEff;
            }
            set
            {
                this._ShftEff = value;
            }
        }
        #endregion
        #region CalendarID
        public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }

        protected String _CalendarID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Calendar ID", Required = true)]
        [PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
        [PXForeignReference(typeof(Field<AMShift.calendarID>.IsRelatedTo<CSCalendar.calendarID>))]
        public virtual String CalendarID
        {
            get
            {
                return this._CalendarID;
            }
            set
            {
                this._CalendarID = value;
            }
        }
        #endregion
        #region LaborCodeID

        public abstract class laborCodeID : PX.Data.BQL.BqlString.Field<laborCodeID> { }

        protected String _LaborCodeID;
        [PXDBString(15, InputMask = ">AAAAAAAAAAAAAAA")]
        [PXUIField(DisplayName = "Labor Code", Required = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [DirectLabor]
        public virtual String LaborCodeID
        {
            get
            {
                return this._LaborCodeID;
            }
            set
            {
                this._LaborCodeID = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion
		#region WcID
		public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

		protected String _WcID;
        [WorkCenterIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMWC.wcID))]
		public virtual String WcID
		{
			get
			{
				return this._WcID;
			}
			set
			{
				this._WcID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime

        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID

        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
	}
}
