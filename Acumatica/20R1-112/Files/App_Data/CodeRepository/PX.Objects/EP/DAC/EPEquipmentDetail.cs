using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.EP
{
    [Serializable]
	[PXCacheName(Messages.EquipmentDetail)]
    public class EPEquipmentDetail : PX.Data.IBqlTable
    {
        #region TimeCardCD

        public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

        [PXDBDefault(typeof(EPEquipmentTimeCard.timeCardCD))]
        [PXDBString(10, IsKey = true)]
        [PXUIField(Visible = false)]
        [PXParent(typeof(Select<EPEquipmentTimeCard, Where<EPEquipmentTimeCard.timeCardCD, Equal<Current<EPEquipmentDetail.timeCardCD>>>>))]
        public virtual String TimeCardCD { get; set; }

        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(EPEquipmentTimeCard.detailLineCntr))]
        [PXUIField(Visible = false)]
        public virtual Int32? LineNbr { get; set; }
        #endregion

        #region SetupSummaryLineNbr
        public abstract class setupSummarylineNbr : PX.Data.BQL.BqlInt.Field<setupSummarylineNbr> { }
        [PXDBInt()]
        public virtual Int32? SetupSummaryLineNbr { get; set; }
        #endregion

        #region RunSummaryLineNbr
        public abstract class runSummarylineNbr : PX.Data.BQL.BqlInt.Field<runSummarylineNbr> { }
        [PXDBInt()]
        public virtual Int32? RunSummaryLineNbr { get; set; }
        #endregion

        #region SuspendSummaryLineNbr
        public abstract class suspendSummarylineNbr : PX.Data.BQL.BqlInt.Field<suspendSummarylineNbr> { }
        [PXDBInt()]
        public virtual Int32? SuspendSummaryLineNbr { get; set; }
        #endregion

        #region OrigLineNbr
        public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
        [PXDBInt()]
        public virtual Int32? OrigLineNbr { get; set; }
        #endregion
        #region Date
        public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
        protected DateTime? _Date;
        [PXDBDate()]
        [PXDefault()]
        [PXUIField(DisplayName = "Date")]
        public virtual DateTime? Date
        {
            get
            {
                return this._Date;
            }
            set
            {
                this._Date = value;
            }
        }
        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        protected String _Description;
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual String Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                this._Description = value;
            }
        }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        protected Int32? _ProjectID;
        [PXDefault(typeof(PMProject.contractID))]
        [EPEquipmentActiveProject]
        public virtual Int32? ProjectID
        {
            get
            {
                return this._ProjectID;
            }
            set
            {
                this._ProjectID = value;
            }
        }
        #endregion
        #region ProjectTaskID
        public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[EPTimecardProjectTask(typeof(EPEquipmentDetail.projectID), GL.BatchModule.TA, DisplayName = "Project Task")]
		public virtual Int32? ProjectTaskID { get; set; }
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[CostCode(null, typeof(projectTaskID), GL.AccountType.Expense, DescriptionField = typeof(PMCostCode.description))]
		public virtual Int32? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region RunTime
		public abstract class runTime : PX.Data.BQL.BqlInt.Field<runTime> { }
        protected Int32? _RunTime;
        [PXDBInt]
        [PXTimeList]
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
        [PXDBInt]
        [PXTimeList]
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
        [PXDBInt]
        [PXTimeList]
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
        #region IsBillable
        public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }
        protected Boolean? _IsBillable;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Billable")]
        public virtual Boolean? IsBillable
        {
            get
            {
                return this._IsBillable;
            }
            set
            {
                this._IsBillable = value;
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


        #region Unbound Fields (Calculated in the TimecardMaint graph)

       
       

        

        #endregion
    }
}
