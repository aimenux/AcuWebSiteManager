using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName("Work Center Schedule Detail")]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMWCSchdDetail : IBqlTable, ISchdDetail<AMWCSchdDetail>, ISchdReference
    {
        internal string DebuggerDisplay => $"RecordID = {RecordID}, WcID = {WcID}, ShiftID = {ShiftID}, SchdDate = {SchdDate?.ToShortDateString()}; {StartTime.GetValueOrDefault().ToShortTimeString()} - {EndTime.GetValueOrDefault().ToShortTimeString()}{(IsBreak == true ? "; IsBreak=true" : string.Empty)}";

        #region Keys
        public class PK : PrimaryKeyOf<AMWCSchdDetail>.By<recordID>
        {
            public static AMWCSchdDetail Find(PXGraph graph, int recordID) => FindBy(graph, recordID);
        }

        public static class FK
        {
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMWCSchdDetail>.By<siteID> { }
            public class WorkCenter : AMWC.PK.ForeignKeyOf<AMWCSchdDetail>.By<wcID> { }
            public class Shift : AMShift.PK.ForeignKeyOf<AMWCSchdDetail>.By<wcID, shiftID> { }
        }
        #endregion

        #region ResourceID (Unbound)
        public abstract class resourceID : PX.Data.BQL.BqlString.Field<resourceID> { }

        [PXString(20, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Resource ID", Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual string ResourceID
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
        #region RecordID (KEY)
        public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }

        protected Int64? _RecordID;
        [PXDBLongIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Record ID", Enabled = false, Visible = false)]
        [PXParent(typeof(Select<AMWCSchd,
            Where<AMWCSchd.wcID, Equal<Current<AMWCSchdDetail.wcID>>,
                And<AMWCSchd.shiftID, Equal<Current<AMWCSchdDetail.shiftID>>,
                And<AMWCSchd.schdDate, Equal<Current<AMWCSchdDetail.schdDate>>>>>>))]
        public virtual Int64? RecordID
        {
            get { return this._RecordID; }
            set { this._RecordID = value; }
        }
        #endregion
        #region SchdKey

        public abstract class schdKey : PX.Data.BQL.BqlGuid.Field<schdKey> { }

        protected Guid? _SchdKey;
        [PXDBGuid]
        [PXUIField(DisplayName = "Schedule Key", Enabled = false, Visible = false)]
        public virtual Guid? SchdKey
        {
            get { return this._SchdKey; }
            set { this._SchdKey = value; }
        }
        #endregion
        #region WcID

        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

        protected String _WcID;
        [WorkCenterIDField(Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDefault]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        public virtual String WcID
        {
            get { return this._WcID; }
            set { this._WcID = value; }
        }

        #endregion
        #region ShiftID
        public abstract class shiftID : PX.Data.BQL.BqlString.Field<shiftID> { }

        protected String _ShiftID;
        [PXDBString(4)]
        [PXDefault]
        [PXUIField(DisplayName = "Shift", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXSelector(typeof(Search<AMShiftMst.shiftID>))]
        public virtual String ShiftID
        {
            get { return this._ShiftID; }
            set { this._ShiftID = value; }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PX.Objects.IN.Site(Enabled = false)]
        [PXDefault]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        public virtual Int32? SiteID
        {
            get { return this._SiteID; }
            set { this._SiteID = value; }
        }
        #endregion
        #region SchdDate

        public abstract class schdDate : PX.Data.BQL.BqlDateTime.Field<schdDate> { }

        protected DateTime? _SchdDate;
        [PXDBDate]
        [PXDefault]
        [PXUIField(DisplayName = "Schedule Date", Enabled = false)]
        public virtual DateTime? SchdDate
        {
            get { return this._SchdDate; }
            set { this._SchdDate = value; }
        }

        #endregion
        #region StartTime

        public abstract class startTime : PX.Data.BQL.BqlDateTime.Field<startTime> { }

        protected DateTime? _StartTime;

        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXUIField(DisplayName = "Start Time", Enabled = false)]
        [PXDefault]
        public virtual DateTime? StartTime
        {
            get { return this._StartTime; }
            set
            {
                this._StartTime = value;
                this._StartTimeString = this._StartTime == null ? string.Empty : this._StartTime.GetValueOrDefault().ToString("hh:mm tt");
            }
        }

        #endregion
        #region EndTime

        public abstract class endTime : PX.Data.BQL.BqlDateTime.Field<endTime> { }

        protected DateTime? _EndTime;

        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXUIField(DisplayName = "End Time", Enabled = false)]
        [PXDefault]
        public virtual DateTime? EndTime
        {
            get { return this._EndTime; }
            set
            {
                this._EndTime = value;
                this._EndTimeString = this._EndTime == null ? string.Empty : this._EndTime.GetValueOrDefault().ToString("hh:mm tt");
            }
        }

        #endregion

        #region TEMP WORK AROUNDS FOR GI
        /// <summary>
        /// Work around for GI not being able to display time
        /// </summary>
        public abstract class startTimeString : PX.Data.BQL.BqlDateTime.Field<startTimeString> { }

        protected String _StartTimeString;
        /// <summary>
        /// Work around for GI not being able to display time
        /// </summary>
        [PXString]
        [PXUIField(DisplayName = "Start Time", Enabled = false)]
        [PXDependsOnFields(typeof(AMWCSchdDetail.startTime))]
        public virtual String StartTimeString
        {
            get { return this._StartTimeString; }
        }

        /// <summary>
        /// Work around for GI not being able to display time
        /// </summary>
        public abstract class endTimeString : PX.Data.BQL.BqlDateTime.Field<endTimeString> { }

        protected String _EndTimeString;
        /// <summary>
        /// Work around for GI not being able to display time
        /// </summary>
        [PXString]
        [PXUIField(DisplayName = "End Time", Enabled = false)]
        [PXDependsOnFields(typeof(AMWCSchdDetail.endTime))]
        public virtual String EndTimeString
        {
            get { return this._EndTimeString; }
        }

        #endregion

        #region OrderByDate

        public abstract class orderByDate : PX.Data.BQL.BqlDateTime.Field<orderByDate> { }

        protected DateTime? _OrderByDate;

        [PXDBDateAndTime(UseTimeZone = false)]
        [PXUIField(DisplayName = "Order By Date Time", Enabled = false, Visible = false)]
        [PXDefault]
        public virtual DateTime? OrderByDate
        {
            get { return this._OrderByDate; }
            set { this._OrderByDate = value; }
        }

        #endregion
        #region Description

        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        public virtual String Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }
        #endregion
        #region SchdTime

        public abstract class schdTime : PX.Data.BQL.BqlInt.Field<schdTime> { }

        protected int? _SchdTime;
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Schedule Time w/o Efficiency", Enabled = false)]
        [PXDefault(0)]
        [PXFormula(null, typeof(SumCalc<AMWCSchd.schdTime>))]
        public virtual int? SchdTime
        {
            get { return this._SchdTime; }
            set { this._SchdTime = value; }
        }

        #endregion
        #region SchdEfficiencyTime

        public abstract class schdEfficiencyTime : PX.Data.BQL.BqlInt.Field<schdEfficiencyTime> { }

        protected int? _SchdEfficiencyTime;
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Schedule Time", Enabled = false)]
        [PXDefault(0)]
        [PXFormula(null, typeof(SumCalc<AMWCSchd.schdEfficiencyTime>))]
        public virtual int? SchdEfficiencyTime
        {
            get { return this._SchdEfficiencyTime; }
            set { this._SchdEfficiencyTime = value; }
        }

        #endregion
        #region PlanBlocks
        public abstract class planBlocks : PX.Data.BQL.BqlInt.Field<planBlocks> { }

        protected int? _PlanBlocks;
        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Plan Blocks", Enabled = false)]
        [PXDefault(0)]
        [PXFormula(null, typeof(SumCalc<AMWCSchd.planBlocks>))]
        public virtual int? PlanBlocks
        {
            get { return this._PlanBlocks; }
            set { this._PlanBlocks = value; }
        }
        #endregion
        #region SchdBlocks
        public abstract class schdBlocks : PX.Data.BQL.BqlInt.Field<schdBlocks> { }

        protected int? _SchdBlocks;
        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Scheduled Blocks", Enabled = false)]
        [PXDefault(0)]
        [PXFormula(null, typeof(SumCalc<AMWCSchd.schdBlocks>))]
        public virtual int? SchdBlocks
        {
            get { return this._SchdBlocks; }
            set { this._SchdBlocks = value; }
        }
        #endregion
        #region IsBreak
        public abstract class isBreak : PX.Data.BQL.BqlBool.Field<isBreak> { }

        protected bool? _IsBreak;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Break", Enabled = false)]
        public virtual bool? IsBreak
        {
            get { return this._IsBreak; }
            set { this._IsBreak = value; }
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

        /// <summary>
        /// Makes a copy of the object excluding the recordID, created by, last mod by, and timestamps fields
        /// </summary>
        /// <param name="schdDetail">Schd Detail record to copy</param>
        /// <returns>new object with copied values</returns>
        public static AMWCSchdDetail Copy(AMWCSchdDetail schdDetail)
        {
            return schdDetail.Copy();
        }

#pragma warning disable PX1031 // DACs cannot contain instance methods
        /// <summary>
        /// Makes a copy of the object excluding the recordID, created by, last mod by, and timestamps fields
        /// </summary>
        /// <returns>new object with copied values</returns>
        public virtual AMWCSchdDetail Copy()
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            return new AMWCSchdDetail
            {
                SchdKey = this.SchdKey,
                WcID = this.WcID,
                ShiftID = this.ShiftID,
                SiteID = this.SiteID,
                SchdDate = this.SchdDate,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                OrderByDate = this.OrderByDate,
                Description = this.Description,
                SchdTime = this.SchdTime,
                SchdEfficiencyTime = this.SchdEfficiencyTime,
                PlanBlocks = this.PlanBlocks,
                SchdBlocks = this.SchdBlocks,
                IsBreak = this.IsBreak
            };
        }
    }

    /// <summary>
    /// Group by Work Center, Shift, Schd Date for AMWCSchdDetail records
    /// </summary>
    [Serializable]
    [PXHidden]
    [PXProjection(typeof(Select4<
        AMWCSchdDetail,
        Aggregate<
            GroupBy<AMWCSchdDetail.wcID,
            GroupBy<AMWCSchdDetail.shiftID,
            GroupBy<AMWCSchdDetail.schdDate,
                Sum<AMWCSchdDetail.schdTime, 
                Sum<AMWCSchdDetail.schdEfficiencyTime,
                Sum<AMWCSchdDetail.planBlocks,
                Sum<AMWCSchdDetail.schdBlocks>>>>>>>>>), Persistent = false)]
    public class AMWCSchdDetailGroupByWcSchiftDate : IBqlTable
    {
        #region WcID

        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

        protected String _WcID;
        [WorkCenterIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Enabled = false, BqlField = typeof(AMWCSchdDetail.wcID))]
        [PXDefault]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        public virtual String WcID
        {
            get { return this._WcID; }
            set { this._WcID = value; }
        }

        #endregion
        #region ShiftID
        public abstract class shiftID : PX.Data.BQL.BqlString.Field<shiftID> { }

        protected String _ShiftID;
        [PXDBString(4, IsKey = true, BqlField = typeof(AMWCSchdDetail.shiftID))]
        [PXDefault]
        [PXUIField(DisplayName = "Shift", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXSelector(typeof(Search<AMShiftMst.shiftID>))]
        public virtual String ShiftID
        {
            get { return this._ShiftID; }
            set { this._ShiftID = value; }
        }
        #endregion
        #region SchdDate

        public abstract class schdDate : PX.Data.BQL.BqlDateTime.Field<schdDate> { }

        protected DateTime? _SchdDate;
        [PXDBDate(IsKey = true, BqlField = typeof(AMWCSchdDetail.schdDate))]
        [PXDefault]
        [PXUIField(DisplayName = "Schedule Date", Enabled = false)]
        public virtual DateTime? SchdDate
        {
            get { return this._SchdDate; }
            set { this._SchdDate = value; }
        }

        #endregion

        #region SchdTime (SUM)

        public abstract class schdTime : PX.Data.BQL.BqlInt.Field<schdTime> { }

        protected int? _SchdTime;
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes, BqlField = typeof(AMWCSchdDetail.schdTime))]
        [PXUIField(DisplayName = "Schedule Time", Enabled = false)]
        public virtual int? SchdTime
        {
            get { return this._SchdTime; }
            set { this._SchdTime = value; }
        }

        #endregion
        #region PlanBlocks (SUM)
        public abstract class planBlocks : PX.Data.BQL.BqlInt.Field<planBlocks> { }

        protected int? _PlanBlocks;
        [PXDBInt(MinValue = 0, BqlField = typeof(AMWCSchdDetail.planBlocks))]
        [PXUIField(DisplayName = "Plan Blocks", Enabled = false)]
        public virtual int? PlanBlocks
        {
            get { return this._PlanBlocks; }
            set { this._PlanBlocks = value; }
        }
        #endregion
        #region SchdBlocks (SUM)
        public abstract class schdBlocks : PX.Data.BQL.BqlInt.Field<schdBlocks> { }

        protected int? _SchdBlocks;
        [PXDBInt(MinValue = 0, BqlField = typeof(AMWCSchdDetail.schdBlocks))]
        [PXUIField(DisplayName = "Scheduled Blocks", Enabled = false)]
        public virtual int? SchdBlocks
        {
            get { return this._SchdBlocks; }
            set { this._SchdBlocks = value; }
        }
        #endregion
    }
}