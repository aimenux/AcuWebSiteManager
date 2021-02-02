using System;
using System.Diagnostics;
using PX.Data;
using PX.Data.EP;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL.FinPeriods
{
	[Serializable]
	[PXProjection(typeof(Select<FinYear, 
								Where<FinYear.organizationID, Equal<FinPeriod.organizationID.masterValue>>>),
						Persistent = true)]
	[PXPrimaryGraph(
		new Type[] { typeof(MasterFinPeriodMaint) },
		new Type[] { typeof(Select<MasterFinYear,
			Where<MasterFinYear.year, Equal<Current<MasterFinYear.year>>>>)
		})]
	[DebuggerDisplay("{GetType()}: Year = {Year}, tstamp = {PX.Data.PXDBTimestampAttribute.ToString(tstamp)}")]
	public class MasterFinYear : IBqlTable, IFinYear
	{
		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }

		/// <summary>
		/// Key field.
		/// The financial year.
		/// </summary>
		[PXDBString(4, IsKey = true, IsFixed = true, BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault("")]
		[PXUIField(DisplayName = "Financial Year", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search3<MasterFinYear.year, OrderBy<Desc<MasterFinYear.year>>>))]
		[PXFieldDescription]
		public virtual string Year { get; set; }
		#endregion

		#region OrganizationID

		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[PXExtraKey]
		[PXDBInt(BqlTable = typeof(FinYear))]
		[PXDefault(FinPeriod.organizationID.MasterValue)]
		public virtual int? OrganizationID { get; set; }
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		/// <summary>
		/// The start date of the year.
		/// </summary>
		[PXDBDate(BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Start Date", Enabled = false)]
		public virtual DateTime? StartDate
		{
			get;
			set;
		}
		#endregion

		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		/// <summary>
		/// The end date of the year (inclusive).
		/// </summary>
		[PXDBDate(BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault()]
		[PXUIField(DisplayName = "EndDate", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual DateTime? EndDate
		{
			get;
			set;
		}
		#endregion

		#region FinPeriods
		public abstract class finPeriods : PX.Data.BQL.BqlShort.Field<finPeriods> { }

		/// <summary>
		/// The number of periods in the year.
		/// </summary>
		[PXDBShort(BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Number of Periods", Enabled = false)]
		public virtual short? FinPeriods { get; set; }
		#endregion

		#region CustomPeriods
		public abstract class customPeriods : PX.Data.BQL.BqlBool.Field<customPeriods> { }

		/// <summary>
		/// Indicates whether the <see cref="FinPeriod">periods</see> of the year can be modified by user.
		/// </summary>
		[PXDBBool(BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "User-Defined Periods")]
		public virtual Boolean? CustomPeriods
		{
			get;
			set;
		}
		#endregion

		#region BegFinYearHist
		public abstract class begFinYearHist : PX.Data.BQL.BqlDateTime.Field<begFinYearHist> { }

		/// <summary>
		/// The start date of the financial year.
		/// </summary>
		/// <value>
		/// Defaults to the value of the <see cref="FinYearSetup.BegFinYear"/> field of the financial year setup record.
		/// </value>
		[PXDBDate(BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault(typeof(Search<FinYearSetup.begFinYear>))]
		[PXUIField(DisplayName = "Financial Year Starts On", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual DateTime? BegFinYearHist
		{
			get;
			set;
		}
		#endregion

		#region PeriodsStartDateHist
		public abstract class periodsStartDateHist : PX.Data.BQL.BqlDateTime.Field<periodsStartDateHist> { }

		/// <summary>
		/// The start date of the first period of the year.
		/// </summary>
		/// <value>
		/// Defaults to the value of the <see cref="FinYearSetup.PeriodsStartDate"/> field of the financial year setup record.
		/// </value>
		[PXDBDate(BqlTable = typeof(TableDefinition.FinYear))]
		[PXDefault(typeof(Search<FinYearSetup.periodsStartDate>))]
		[PXUIField(DisplayName = "Periods Start Date", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual DateTime? PeriodsStartDateHist
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXNote(DescriptionField = typeof(MasterFinYear.year), BqlTable = typeof(TableDefinition.FinYear))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual byte[] tstamp { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual String CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime(BqlTable = typeof(TableDefinition.FinYear))]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
	}
}
