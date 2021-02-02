using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.EP
{
    [Serializable]
	[PXCacheName(Messages.EmployeePosition)]
	public class EPEmployeePosition : PX.Data.IBqlTable
    {
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(EPEmployee.bAccountID))]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPEmployeePosition.employeeID>>>>))]
		public virtual Int32? EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this._EmployeeID = value;
			}
		}
		#endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(EPEmployee.positionLineCntr))]
        [PXUIField(Visible = false)]
        public virtual Int32? LineNbr { get; set; }
        #endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected Boolean? _IsActive;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region PositionID
		public abstract class positionID : PX.Data.BQL.BqlString.Field<positionID> { }
		protected String _PositionID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault()]
		[PXSelector(typeof(EPPosition.positionID), DescriptionField = typeof(EPPosition.description))]
		[PXUIField(DisplayName = "Position", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String PositionID
		{
			get
			{
				return this._PositionID;
			}
			set
			{
				this._PositionID = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;
		[PXDefault]
		[PXDBDate()]
		[PXCheckUnique(typeof(employeeID), typeof(startDate))]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region StartReason
		public abstract class startReason : PX.Data.BQL.BqlString.Field<startReason> { }
		protected String _StartReason;
		[PXDBString(3, IsFixed = true)]
		[EPStartReason.List()]
		[PXDefault(EPStartReason.New)]
		[PXUIField(DisplayName = "Start Reason")]
		public virtual String StartReason
		{
			get
			{
				return this._StartReason;
			}
			set
			{
				this._StartReason = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<EPEmployeePosition.isTerminated, Equal<True>, Or<EPEmployeePosition.isActive, Equal<False>>>))]
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region TermReason
		public abstract class termReason : PX.Data.BQL.BqlString.Field<termReason> { }
		protected String _TermReason;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(EPEmployeePosition.isTerminated))]
		[PXUIRequired(typeof(EPEmployeePosition.isTerminated))]
		[PXDBString(3, IsFixed = true)]
		[EPTermReason.List()]
		[PXUIField(DisplayName = "Termination Reason")]
		public virtual String TermReason
		{
			get
			{
				return this._TermReason;
			}
			set
			{
				this._TermReason = value;
			}
		}
		#endregion
		#region IsTerminated
		public abstract class isTerminated : PX.Data.BQL.BqlBool.Field<isTerminated> { }
		protected Boolean? _IsTerminated;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Terminated")]
		public virtual Boolean? IsTerminated
		{
			get
			{
				return this._IsTerminated;
			}
			set
			{
				this._IsTerminated = value;
			}
		}
		#endregion
		
		#region IsRehirable
		public abstract class isRehirable : PX.Data.BQL.BqlBool.Field<isRehirable> { }
		protected Boolean? _IsRehirable;
		[PXUIEnabled(typeof(Where<EPEmployeePosition.isTerminated, Equal<True>, And<EPEmployeePosition.termReason, NotEqual<EPTermReason.deseased>>>))]
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Rehire Eligible")]
		public virtual Boolean? IsRehirable
		{
			get
			{
				return this._IsRehirable;
			}
			set
			{
				this._IsRehirable = value;
			}
		}
		#endregion
		
        #region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
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
        #endregion
	}

	public static class EPStartReason
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { New, Rehire, Promotion, Demotion, NewSkills, Reorganization, Other },
				new string[] { Messages.New, Messages.Rehire, Messages.Promotion, Messages.Demotion, Messages.NewSkills, Messages.Reorganization, Messages.Other }) { ; }
		}
		public const string New = "NEW";
		public const string Rehire = "REH";
		public const string Promotion = "PRO";
		public const string Demotion = "DEM";
		public const string NewSkills = "SKI";
		public const string Reorganization = "REO";
		public const string Other = "OTH";
	}

	public static class EPTermReason
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Retirement, Layoff, TerminatedForCause, Resignation, Deceased, MedicalIssues },
				new string[] { Messages.Retirement, Messages.Layoff, Messages.TerminatedForCause, Messages.Resignation, Messages.Deceased, Messages.MedicalIssues }) { ; }
		}
		public const string Retirement = "RET";
		public const string Layoff = "LAY";
		public const string TerminatedForCause = "FIR";
		public const string Resignation = "RES";
		public const string Deceased = "DEC";
        public const string MedicalIssues = "MIS";

		 public class deseased : PX.Data.BQL.BqlString.Constant<deseased>
		{
            public deseased() : base(Deceased) { ;}
        }
	}
}
