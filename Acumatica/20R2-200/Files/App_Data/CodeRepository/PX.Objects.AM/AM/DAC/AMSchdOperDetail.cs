using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName("Scheduled Operation Details")]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMSchdOperDetail : IBqlTable, IProdOper, ISchdReference
    {
        internal string DebuggerDisplay => $"[{OrderType}:{ProdOrdID}:{OperationID}] Date = {SchdDate?.ToShortDateString()}, Minutes = {SchdTime}";

        #region Keys

        public class PK : PrimaryKeyOf<AMSchdOper>.By<recordID>
        {
            public static AMSchdOper Find(PXGraph graph, long? recordID) => FindBy(graph, recordID);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMSchdOper>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMSchdOper>.By<orderType, prodOrdID> { }
            public class Operation : AMProdOper.PK.ForeignKeyOf<AMSchdOper>.By<orderType, prodOrdID, operationID> { }
        }

        #endregion

        #region RecordID (KEY)
        public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }

        protected Int64? _RecordID;
        [PXDBLongIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Record ID", Enabled = false, Visible = false)]
        public virtual Int64? RecordID
        {
            get { return this._RecordID; }
            set { this._RecordID = value; }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(Enabled = false)]
        [PXDefault]
        [AMOrderTypeSelector]
        public virtual String OrderType
        {
            get { return this._OrderType; }
            set { this._OrderType = value; }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected string _ProdOrdID;
        [ProductionNbr(Enabled = false)]
        [PXDefault]
        [ProductionOrderSelector(typeof(AMSchdOperDetail.orderType), includeAll:true, ValidateValue = false)]
        public virtual string ProdOrdID
        {
            get { return this._ProdOrdID; }
            set { this._ProdOrdID = value; }
        }
        #endregion
        #region IsPlan
        /// <summary>
        /// Indicates if the record is plan order (most likely out of MRP planning)
        /// Plan orders should not have an Actual order in AMProdItem as of 6.0 release
        /// </summary>
        public abstract class isPlan : PX.Data.BQL.BqlBool.Field<isPlan> { }

        protected bool? _IsPlan;
        /// <summary>
        /// Indicates if the record is plan order (most likely out of MRP planning)
        /// Plan orders should not have an Actual order in AMProdItem as of 6.0 release
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Plan", Enabled = false)]
        public virtual bool? IsPlan
        {
            get
            {
                return this._IsPlan;
            }
            set
            {
                this._IsPlan = value;
            }
        }
        #endregion
        #region IsMRP
        /// <summary>
        /// When MRP Reruns it will reset the IsPlan records for this field back to false.
        /// Then rerun and if the same order is picked back up to reschedule it will contain a true value
        /// </summary>
        public abstract class isMRP : PX.Data.BQL.BqlBool.Field<isMRP> { }

        protected bool? _IsMRP;
        /// <summary>
        /// When MRP Reruns it will reset the IsPlan records for this field back to false.
        /// Then rerun and if the same order is picked back up to reschedule it will contain a true value
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is MRP", Enabled = false)]
        public virtual bool? IsMRP
        {
            get
            {
                return this._IsMRP;
            }
            set
            {
                this._IsMRP = value;
            }
        }
        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected int? _OperationID;
        [OperationIDField(Enabled = false)]
        [PXDefault]

        public virtual int? OperationID
        {
            get
            {
                return this._OperationID;
            }
            set
            {
                this._OperationID = value;
            }
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
        #region SchdTime

        public abstract class schdTime : PX.Data.BQL.BqlInt.Field<schdTime> { }

        protected int? _SchdTime;
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Schedule Time w/o Efficiency", Enabled = false)]
        [PXDefault(0)]
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
        public virtual int? SchdEfficiencyTime
        {
            get { return this._SchdEfficiencyTime; }
            set { this._SchdEfficiencyTime = value; }
        }

        #endregion
        #region FirmSchedule
        public abstract class firmSchedule : PX.Data.BQL.BqlBool.Field<firmSchedule> { }

        protected bool? _FirmSchedule;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Firm Schedule", Visible = false, Enabled = false)]
        public virtual bool? FirmSchedule
        {
            get { return this._FirmSchedule; }
            set { this._FirmSchedule = value; }
        }
        #endregion
        #region SchdQty
        /// <summary>
        /// Planned Schedule quantity related to the current detail row
        /// </summary>
        public abstract class schdQty : PX.Data.BQL.BqlDecimal.Field<schdQty> { }

        protected decimal? _SchdQty;
        /// <summary>
        /// Planned Schedule quantity related to the current detail row
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Schedule Qty", Visible = false)]
        public virtual decimal? SchdQty
        {
            get { return this._SchdQty; }
            set { this._SchdQty = value; }
        }
        #endregion
        #region Chain
        /// <summary>
        /// Groups consecutive schedule details together into a chain. A break in the chain is indicated with a new chain number
        /// </summary>
        public abstract class chain : PX.Data.BQL.BqlInt.Field<chain> { }

        protected int? _Chain;
        /// <summary>
        /// Groups consecutive schedule details together into a chain. A break in the chain is indicated with a new chain number
        /// </summary>
        [PXDBInt]
        [PXUIField(DisplayName = "Chain", Enabled = false, Visible = false)]
        [PXDefault(0)]
        public virtual int? Chain
        {
            get { return this._Chain; }
            set { this._Chain = value; }
        }

        #endregion
        #region SchdKey
        /// <summary>
        /// Links the order detail to a schedule detail record for scheduled blocks.
        /// </summary>
        public abstract class schdKey : PX.Data.BQL.BqlGuid.Field<schdKey> { }

        protected Guid? _SchdKey;
        /// <summary>
        /// Links the order detail to a schedule detail record for scheduled blocks.
        /// </summary>
        [PXDBGuid]
        [PXUIField(DisplayName = "Schedule Key", Enabled = false, Visible = false)]
        public virtual Guid? SchdKey
        {
            get { return this._SchdKey; }
            set { this._SchdKey = value; }
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
    }
}