using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.TM;

namespace PX.Objects.EP
{
    [PXPrimaryGraph(typeof(EquipmentTimeCardMaint))]
    [PXCacheName(Messages.EquipmentTimeCard)]
	[Serializable]
    [PXEMailSource]
	public partial class EPEquipmentTimeCard : IBqlTable, IAssign
	{
        #region TimeCardCD
		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(EPSetup.equipmentTimeCardNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(
            typeof(EPEquipmentTimeCard.timeCardCD),
            typeof(EPEquipmentTimeCard.timeCardCD),
            typeof(EPEquipmentTimeCard.equipmentID),
            typeof(EPEquipmentTimeCard.weekDescription),
            typeof(EPEquipmentTimeCard.status))]
		[PXFieldDescription]
		public virtual String TimeCardCD { get; set; }
		#endregion

        #region EquipmentID
        public abstract class equipmentID : PX.Data.BQL.BqlInt.Field<equipmentID> { }
        protected Int32? _EquipmentID;
        [PXDefault]
        [PXDBInt()]
        [PXUIField(DisplayName = "Equipment ID")]
        [PXSelector(typeof(Search<EPEquipment.equipmentID, Where<EPEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>>), SubstituteKey = typeof(EPEquipment.equipmentCD))]
        public virtual Int32? EquipmentID
        {
            get
            {
                return this._EquipmentID;
            }
            set
            {
                this._EquipmentID = value;
            }
        }
        #endregion
		#region Status

		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXDBString(1)]
		[PXDefault("H")]
		[EPEquipmentTimeCardStatus]
		[PXUIField(DisplayName = "Status")]
		public virtual String Status { get; set; }

		#endregion
		#region WeekID

		public abstract class weekId : PX.Data.BQL.BqlInt.Field<weekId> { }

	    protected Int32? _WeekID;
	    [PXDBInt]
	    [PXUIField(DisplayName = "Week")]
		[PXWeekSelector2(DescriptionField = typeof(EPWeekRaw.shortDescription))]
        public virtual Int32? WeekID
	    {
            get
            {
                return this._WeekID;
            }
            set
            {
                this._WeekID = value;
            }
	    }

		#endregion
		#region OrigTimeCardCD
		public abstract class origTimeCardCD : PX.Data.BQL.BqlString.Field<origTimeCardCD> { }
        [PXUIField(DisplayName = "Orig. Ref. Nbr.", Enabled = false)]
		[PXDBString(10, IsUnicode = true)]
		public virtual String OrigTimeCardCD { get; set; }
		#endregion
		#region IsApproved

		public abstract class isApproved : PX.Data.BQL.BqlBool.Field<isApproved> { }

		[PXDBBool]
        [PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsApproved { get; set; }

		#endregion
		#region IsRejected

		public abstract class isRejected : PX.Data.BQL.BqlBool.Field<isRejected> { }

		[PXDBBool]
        [PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsRejected { get; set; }

		#endregion
		#region IsHold

		public abstract class isHold : PX.Data.BQL.BqlBool.Field<isHold> { }

		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsHold { get; set; }

		#endregion
		#region IsReleased

		public abstract class isReleased : PX.Data.BQL.BqlBool.Field<isReleased> { }

		[PXDBBool]
        [PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual Boolean? IsReleased { get; set; }

		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		[PXInt]		
		[PXUIField(DisplayName = "Workgroup ID", Visible = false)]
		[PXSelector(typeof(EPCompanyTreeOwner.workGroupID), SubstituteKey = typeof(EPCompanyTreeOwner.description))]
		public virtual int? WorkgroupID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		[PXGuid]
		[PXUIField(Visible = false)]
		public virtual Guid? OwnerID { get; set; }
		#endregion
		#region SummaryLineCntr
		public abstract class summaryLineCntr : PX.Data.BQL.BqlInt.Field<summaryLineCntr> { }
		protected int? _SummaryLineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? SummaryLineCntr
		{
			get
			{
				return this._SummaryLineCntr;
			}
			set
			{
				this._SummaryLineCntr = value;
			}
		}
		#endregion	
        #region DetailLineCntr
        public abstract class detailLineCntr : PX.Data.BQL.BqlInt.Field<detailLineCntr> { }
        protected int? _DetailLineCntr;
        [PXDBInt()]
        [PXDefault(0)]
        public virtual int? DetailLineCntr
        {
            get
            {
                return this._DetailLineCntr;
            }
            set
            {
                this._DetailLineCntr = value;
            }
        }
        #endregion	

        #region System Columns
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote(typeof(EPTimeCard),
            DescriptionField = typeof(EPTimeCard.timeCardCD),
            Selector = typeof(EPTimeCard.timeCardCD)
            )]
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


        #region Unbound Fields (Calculated in the TimecardMaint graph)

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion

        #region WeekStartDate (Used in Report)

        public abstract class weekStartDate : PX.Data.BQL.BqlDateTime.Field<weekStartDate> { }

		[TimecardWeekStartDate(typeof(weekId))]
		[PXUIField(DisplayName = "Week Start Date")]
		public virtual DateTime? WeekStartDate { get; set; }

        #endregion
        
        public abstract class weekDescription : PX.Data.BQL.BqlString.Field<weekDescription> { }
		[PXString]
		[PXUIField(DisplayName = "Week")]
		[PXFormula(typeof(Selector<EPEquipmentTimeCard.weekId, EPWeekRaw.description>))]
		public virtual String WeekDescription { get; set; }

        public abstract class weekShortDescription : PX.Data.BQL.BqlString.Field<weekShortDescription> { }
		[PXString]
		[PXUIField(DisplayName = "Week")]
		[PXFieldDescription]
		[PXFormula(typeof(Selector<EPEquipmentTimeCard.weekId, EPWeekRaw.shortDescription>))]
		public virtual String WeekShortDescription { get; set; }

        public abstract class timeSetupCalc : PX.Data.BQL.BqlInt.Field<timeSetupCalc> { }
        [PXInt]
        [PXTimeList]
        [PXUIField(DisplayName = "Time Spent", Enabled = false)]
        public virtual Int32? TimeSetupCalc { get; set; }

        public abstract class timeRunCalc : PX.Data.BQL.BqlInt.Field<timeRunCalc> { }
        [PXInt]
        [PXTimeList]
        [PXUIField(DisplayName = "Run", Enabled = false)]
        public virtual Int32? TimeRunCalc { get; set; }

        public abstract class timeSuspendCalc : PX.Data.BQL.BqlInt.Field<timeSuspendCalc> { }
        [PXInt]
        [PXTimeList]
        [PXUIField(DisplayName = "Suspend", Enabled = false)]
        public virtual Int32? TimeSuspendCalc { get; set; }

        public abstract class timeTotalCalc : PX.Data.BQL.BqlInt.Field<timeTotalCalc> { }
        [PXInt]
        [PXTimeList]
        [PXUIField(DisplayName = "Total", Enabled = false)]
        public virtual Int32? TimeTotalCalc 
        {
            get
            {
                return TimeSetupCalc.GetValueOrDefault() + TimeRunCalc.GetValueOrDefault() +
                       TimeSuspendCalc.GetValueOrDefault();
            } 
        }


        public abstract class timeBillableSetupCalc : PX.Data.BQL.BqlInt.Field<timeBillableSetupCalc> { }
        [PXInt]
        [PXTimeList]
        [PXUIField(DisplayName = "Billable", Enabled = false)]
        public virtual Int32? TimeBillableSetupCalc { get; set; }

        public abstract class timeBillableRunCalc : PX.Data.BQL.BqlInt.Field<timeBillableRunCalc> { }
        [PXInt]
        [PXTimeList]
        [PXUIField(DisplayName = "Billable Run", Enabled = false)]
        public virtual Int32? TimeBillableRunCalc { get; set; }

        public abstract class timeBillableSuspendCalc : PX.Data.BQL.BqlInt.Field<timeBillableSuspendCalc> { }
        [PXInt]
        [PXTimeList]
        [PXUIField(DisplayName = "Billable Suspend", Enabled = false)]
        public virtual Int32? TimeBillableSuspendCalc { get; set; }


        public abstract class timeBillableTotalCalc : PX.Data.BQL.BqlInt.Field<timeBillableTotalCalc> { }
        [PXInt]
        [PXTimeList]
        [PXUIField(DisplayName = "Billable Total", Enabled = false)]
        public virtual Int32? TimeBillableTotalCalc
        {
            get
            {
                return TimeBillableSetupCalc.GetValueOrDefault() + TimeBillableRunCalc.GetValueOrDefault() +
                       TimeBillableSuspendCalc.GetValueOrDefault();
            }
        }

        public abstract class timecardType : PX.Data.BQL.BqlString.Field<timecardType> { }
	    [PXString]
	    [PXStringList(new string[] { "N", "C" }, new string[] { "Normal", "Correction" })]
	    [PXUIField(DisplayName = "Type", Enabled = false)]
	    public virtual string TimecardType
	    {
	        get { return string.IsNullOrEmpty(OrigTimeCardCD) ? "N" : "C"; }
	    }
        
        #endregion
    }


	#region Projections

	[PXProjection(typeof(Select4<EPEquipmentDetail,
	Aggregate<
	GroupBy<EPEquipmentDetail.timeCardCD,
	Sum<EPEquipmentDetail.setupTime,
	Sum<EPEquipmentDetail.runTime,
	Sum<EPEquipmentDetail.suspendTime>>>>>>))]
	[Serializable]
	[PXHidden]
	public partial class EPEquipmentTimeCardSpentTotals : IBqlTable
	{
		#region TimeCardCD

		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBString(10, IsKey = true, BqlField = typeof(EPEquipmentDetail.timeCardCD))]
		public virtual String TimeCardCD { get; set; }

		#endregion

		#region RunTime
		public abstract class runTime : PX.Data.BQL.BqlInt.Field<runTime> { }
		protected Int32? _RunTime;
		[PXDBInt(BqlField = typeof(EPEquipmentDetail.runTime))]
		[PXUIField(DisplayName = "Run Time")]
		public virtual Int32? RunTime
		{
			get
			{
				return this._RunTime;
			}
			set
			{
				this._RunTime = value;
			}
		}
		#endregion
		#region SetupTime
		public abstract class setupTime : PX.Data.BQL.BqlInt.Field<setupTime> { }
		protected Int32? _SetupTime;
		[PXDBInt(BqlField = typeof(EPEquipmentDetail.setupTime))]
		[PXUIField(DisplayName = "Setup Time")]
		public virtual Int32? SetupTime
		{
			get
			{
				return this._SetupTime;
			}
			set
			{
				this._SetupTime = value;
			}
		}
		#endregion
		#region SuspendTime
		public abstract class suspendTime : PX.Data.BQL.BqlInt.Field<suspendTime> { }
		protected Int32? _SuspendTime;
		[PXDBInt(BqlField = typeof(EPEquipmentDetail.suspendTime))]
		[PXUIField(DisplayName = "Suspend Time")]
		public virtual Int32? SuspendTime
		{
			get
			{
				return this._SuspendTime;
			}
			set
			{
				this._SuspendTime = value;
			}
		}
		#endregion

		public abstract class timeTotalCalc : PX.Data.BQL.BqlInt.Field<timeTotalCalc> { }
		[PXInt]
		[PXUIField(DisplayName = "Total", Enabled = false)]
		public virtual Int32? TimeTotalCalc
		{
			get
			{
				return RunTime.GetValueOrDefault() + SetupTime.GetValueOrDefault() +
					   SuspendTime.GetValueOrDefault();
			}
		}
	}

	[PXProjection(typeof(Select4<EPEquipmentDetail,
	Where<EPEquipmentDetail.isBillable, Equal<True>>,
	Aggregate<
		GroupBy<EPEquipmentDetail.timeCardCD,
		Sum<EPEquipmentDetail.setupTime,
		Sum<EPEquipmentDetail.runTime,
		Sum<EPEquipmentDetail.suspendTime>>>>>>))]
	[Serializable]
	[PXHidden]
	public partial class EPEquipmentTimeCardBillableTotals : IBqlTable
	{
		#region TimeCardCD

		public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBString(10, IsKey = true, BqlField = typeof(EPEquipmentDetail.timeCardCD))]
		public virtual String TimeCardCD { get; set; }

		#endregion

		#region RunTime
		public abstract class runTime : PX.Data.BQL.BqlInt.Field<runTime> { }
		protected Int32? _RunTime;
		[PXDBInt(BqlField = typeof(EPEquipmentDetail.runTime))]
		[PXUIField(DisplayName = "Billable Run Time")]
		public virtual Int32? RunTime
		{
			get
			{
				return this._RunTime;
			}
			set
			{
				this._RunTime = value;
			}
		}
		#endregion
		#region SetupTime
		public abstract class setupTime : PX.Data.BQL.BqlInt.Field<setupTime> { }
		protected Int32? _SetupTime;
		[PXDBInt(BqlField = typeof(EPEquipmentDetail.setupTime))]
		[PXUIField(DisplayName = "Billable Setup Time")]
		public virtual Int32? SetupTime
		{
			get
			{
				return this._SetupTime;
			}
			set
			{
				this._SetupTime = value;
			}
		}
		#endregion
		#region SuspendTime
		public abstract class suspendTime : PX.Data.BQL.BqlInt.Field<suspendTime> { }
		protected Int32? _SuspendTime;
		[PXDBInt(BqlField = typeof(EPEquipmentDetail.suspendTime))]
		[PXUIField(DisplayName = "Billable Suspend Time")]
		public virtual Int32? SuspendTime
		{
			get
			{
				return this._SuspendTime;
			}
			set
			{
				this._SuspendTime = value;
			}
		}
		#endregion

		public abstract class timeTotalCalc : PX.Data.BQL.BqlInt.Field<timeTotalCalc> { }
		[PXInt]
		[PXUIField(DisplayName = "Billable Total", Enabled = false)]
		public virtual Int32? TimeTotalCalc
		{
			get
			{
				return RunTime.GetValueOrDefault() + SetupTime.GetValueOrDefault() +
					   SuspendTime.GetValueOrDefault();
			}
		}
	} 

	#endregion

	public class EPEquipmentTimeCardStatusAttribute : PXStringListAttribute
	{
		public const string OnHold = "H";
		public const string PendingApproval = "O";
		public const string Approved = "A";
		public const string Rejected = "C";
		public const string Released = "R";

		public EPEquipmentTimeCardStatusAttribute()
			: base(
				new[] { OnHold, PendingApproval, Approved, Rejected, Released },
				new[] { "On Hold", "Pending Approval", "Approved", "Rejected", "Released" }) { }

		public sealed class onHold : PX.Data.BQL.BqlString.Constant<onHold>
		{
			public onHold() : base(OnHold) { }
		}
		public sealed class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
		{
			public pendingApproval() : base(PendingApproval) { }
		}
		public sealed class approved : PX.Data.BQL.BqlString.Constant<approved>
		{
			public approved() : base(Approved) { }
		}
		public sealed class rejected : PX.Data.BQL.BqlString.Constant<rejected>
		{
			public rejected() : base(Rejected) { }
		}
		public sealed class released : PX.Data.BQL.BqlString.Constant<released>
		{
			public released() : base(Released) { }
		}
	}
}
