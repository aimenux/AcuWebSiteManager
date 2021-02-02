using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.CM;

namespace PX.Objects.AM
{
	[Serializable]
    [PXPrimaryGraph(typeof(ToolMaint))]
    [PXCacheName(Messages.ToolMst)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMToolMst : PX.Data.IBqlTable
	{
        internal string DebuggerDisplay => $"ToolID = {ToolID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMToolMst>.By<toolID>
        {
            public static AMToolMst Find(PXGraph graph, string toolID) => FindBy(graph, toolID);
        }

        #endregion

        #region ToolID
        public abstract class toolID : PX.Data.BQL.BqlString.Field<toolID> { }

        protected String _ToolID;
        [ToolIDField(IsKey = true, Required = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXSelector(typeof(Search<AMToolMst.toolID>))]
        public virtual String ToolID
        {
            get
            {
                return this._ToolID;
            }
            set
            {
                this._ToolID = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected String _Descr;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility=PXUIVisibility.SelectorVisible)]
        public virtual String Descr
        {
            get
            {
                return this._Descr;
            }
            set
            {
                this._Descr = value;
            }
        }
        #endregion
        #region Active
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

        protected Boolean? _Active;
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual Boolean? Active
        {
            get
            {
                return this._Active;
            }
            set
            {
                this._Active = value;
            }
        }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        protected Decimal? _UnitCost;
        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Unit Cost")]
        public virtual Decimal? UnitCost
        {
            get
            {
                return this._UnitCost;
            }
            set
            {
                this._UnitCost = value;
            }
        }
        #endregion
        #region TotalCost
        public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }

        protected Decimal? _TotalCost;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Cost")]
        public virtual Decimal? TotalCost
        {
            get
            {
                return this._TotalCost;
            }
            set
            {
                this._TotalCost = value;
            }
        }
        #endregion
        #region ActualCost
        public abstract class actualCost : PX.Data.BQL.BqlDecimal.Field<actualCost> { }

        protected Decimal? _ActualCost;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Consumed Cost", Enabled=false)]
        public virtual Decimal? ActualCost
        {
            get
            {
                return this._ActualCost;
            }
            set
            {
                this._ActualCost = value;
            }
        }
        #endregion
        #region ActualUses
        public abstract class actualUses : PX.Data.BQL.BqlDecimal.Field<actualUses> { }

        protected Decimal? _ActualUses;
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Uses", Enabled=false)]
        public virtual Decimal? ActualUses
        {
            get
            {
                return this._ActualUses;
            }
            set
            {
                this._ActualUses = value;
            }
        }
        #endregion
		#region AcctID
		public abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }

		protected Int32? _AcctID;
        [PXDBDefault]        
        [Account]
		public virtual Int32? AcctID
		{
			get
			{
                return this._AcctID;
			}
			set
			{
                this._AcctID = value;
			}
		}
		#endregion
        #region SubID
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        protected Int32? _SubID;
        [PXDBDefault]
        [SubAccount(typeof(AMToolMst.acctID))]
        public virtual Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
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
        #region ScheduleEnabled
        /// <summary>
        /// Indicates the tool is scheduled in APS.
        /// (Only used in APS.)
        /// </summary>
        public abstract class scheduleEnabled : PX.Data.BQL.BqlBool.Field<scheduleEnabled> { }

        protected Boolean? _ScheduleEnabled;
        /// <summary>
        /// Indicates the tool is scheduled in APS.
        /// (Only used in APS.)
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Schedule", FieldClass = Features.ADVANCEDPLANNINGFIELDCLASS)]
        public virtual Boolean? ScheduleEnabled
        {
            get
            {
                return this._ScheduleEnabled;
            }
            set
            {
                this._ScheduleEnabled = value;
            }
        }
        #endregion
        #region ScheduleQty
        /// <summary>
        /// APS Schedule qty/units for scheduling tools. 
        /// The number of tools available for scheduling.
        /// </summary>
        public abstract class scheduleQty : PX.Data.BQL.BqlInt.Field<scheduleQty> { }

        protected int? _ScheduleQty;
        /// <summary>
        /// APS Schedule qty/units for scheduling tools. 
        /// The number of tools available for scheduling.
        /// </summary>
        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Total Schedule Qty", FieldClass = Features.ADVANCEDPLANNINGFIELDCLASS)]
        public virtual int? ScheduleQty
        {
            get
            {
                return this._ScheduleQty;
            }
            set
            {
                this._ScheduleQty = value;
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
