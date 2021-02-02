using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL.Attributes;

namespace PX.Objects.GL.FinPeriods.TableDefinition
{
	public class FinYear: IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(false, IsKey = true)]
		public virtual int? OrganizationID { get; set; }

		#endregion

		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		protected String _Year;

		/// <summary>
		/// Key field.
		/// The financial year.
		/// </summary>
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXDefault("")]
		[PXFieldDescription]
		public virtual String Year
		{
			get
			{
				return this._Year;
			}
			set
			{
				this._Year = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		protected DateTime? _StartDate;

		/// <summary>
		/// The start date of the year.
		/// </summary>
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
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
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		protected DateTime? _EndDate;

		/// <summary>
		/// The end date of the year (inclusive).
		/// </summary>
		[PXDBDate()]
		[PXDefault()]
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
		#region FinPeriods
		public abstract class finPeriods : PX.Data.BQL.BqlShort.Field<finPeriods> { }
		protected Int16? _FinPeriods;

		/// <summary>
		/// The number of periods in the year.
		/// </summary>
		[PXDBShort()]
		[PXDefault((short)0)]
		public virtual Int16? FinPeriods
		{
			get
			{
				return this._FinPeriods;
			}
			set
			{
				this._FinPeriods = value;
			}
		}
		#endregion
		#region CustomPeriods
		public abstract class customPeriods : PX.Data.BQL.BqlBool.Field<customPeriods> { }
		protected Boolean? _CustomPeriods;

		/// <summary>
		/// Indicates whether the <see cref="PX.Objects.GL.Obsolete.FinPeriod">periods</see> of the year can be modified by user.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? CustomPeriods
		{
			get
			{
				return this._CustomPeriods;
			}
			set
			{
				this._CustomPeriods = value;
			}
		}
		#endregion
		#region BegFinYearHist
		public abstract class begFinYearHist : PX.Data.BQL.BqlDateTime.Field<begFinYearHist> { }
		protected DateTime? _BegFinYearHist;

		/// <summary>
		/// The start date of the financial year.
		/// </summary>
		/// <value>
		/// Defaults to the value of the <see cref="FinYearSetup.BegFinYear"/> field of the financial year setup record.
		/// </value>
		[PXDBDate()]
		public virtual DateTime? BegFinYearHist
		{
			get
			{
				return this._BegFinYearHist;
			}
			set
			{
				this._BegFinYearHist = value;
			}
		}
		#endregion
		#region PeriodsStartDateHist
		public abstract class periodsStartDateHist : PX.Data.BQL.BqlDateTime.Field<periodsStartDateHist> { }
		protected DateTime? _PeriodsStartDateHist;

		/// <summary>
		/// The start date of the first period of the year.
		/// </summary>
		/// <value>
		/// Defaults to the value of the <see cref="FinYearSetup.PeriodsStartDate"/> field of the financial year setup record.
		/// </value>
		[PXDBDate()]
		public virtual DateTime? PeriodsStartDateHist
		{
			get
			{
				return this._PeriodsStartDateHist;
			}
			set
			{
				this._PeriodsStartDateHist = value;
			}
		}
		#endregion
		#region StartMasterFinPeriodID
		public abstract class startMasterFinPeriodID : PX.Data.BQL.BqlString.Field<startMasterFinPeriodID> { }

		/// <summary>
		/// Key field.
		/// Unique identifier of the Financial Period.
		/// </summary>
		/// Consists of the year and the number of the period in the year. For more information see <see cref="FinPeriodIDAttribute"/>.
		[PXDBString(6, IsFixed = true)]
		[FinPeriodIDFormatting]
		public virtual string StartMasterFinPeriodID { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXNote(DescriptionField = typeof(FinYear.year))]
		public virtual Guid? NoteID { get; set; }
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
