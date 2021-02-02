using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.SM;
using PX.TM;

namespace PX.Objects.WZ
{
    [Serializable]
    [PXPrimaryGraph(typeof(WZScenarioEntry))]
    [PXCacheName(Messages.WizardScenarioName)]
    [PXEMailSource]
    [PX.Objects.GL.TableAndChartDashboardType]
	[PXHidden]
    public partial class WZScenario : IBqlTable, PX.Data.EP.IAssign
    {
        #region ScenraioID
        public abstract class scenarioID : PX.Data.BQL.BqlGuid.Field<scenarioID> { }

        protected Guid? _ScenarioID;
        [PXDBGuid(IsKey = true)]
        [PXUIField(DisplayName = "Scenario ID", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Guid? ScenarioID 
        {
            get 
            { 
                return this._ScenarioID; 
            }
            set 
            { 
                this._ScenarioID = value; 
            }
        }
        #endregion
        #region Name
        public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

        protected String _Name;
        [PXDBString(50, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Scenario Name", Visibility=PXUIVisibility.SelectorVisible)]
        public virtual String Name 
        {
            get 
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
            }

        }
        #endregion
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

        protected String _Status;
        [PXDBString(2, IsFixed=true)]
        [WizardScenarioStatuses]
        [PXDefault(WizardScenarioStatusesAttribute._PENDING)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual String Status 
        {
            get 
            {
                return this._Status;
            }
            set
            {
                this._Status = value;
            }
        }
        #endregion
        #region ExecutionDate
        public abstract class executionDate : PX.Data.BQL.BqlDateTime.Field<executionDate> { }

        protected DateTime? _ExecutionDate;
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Execution Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual DateTime? ExecutionDate
        {
            get 
            {
                return this._ExecutionDate;
            }
            set
            {
                this._ExecutionDate = value;
            }
        }
        #endregion
        #region ExecutionPeriodID
        public abstract class executionPeriodID : PX.Data.BQL.BqlString.Field<executionPeriodID> { }
        protected String _ExecutionPeriodID;
        [FinPeriodID(typeof(WZScenario.executionDate))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Execution Period", Enabled = false)]
        public virtual String ExecutionPeriodID
        {
            get
            {
                return this._ExecutionPeriodID;
            }
            set
            {
                this._ExecutionPeriodID = value;
            }
        }
        #endregion
        #region Rolename
        public abstract class rolename : PX.Data.BQL.BqlString.Field<rolename> { }

        protected String _Rolename;
        [PXDBString(64, IsUnicode = true)]
        [PXUIField(DisplayName = "Role Name", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<PX.SM.Roles.rolename, Where<PX.SM.Roles.guest, Equal<False>>>), DescriptionField = typeof(PX.SM.Roles.descr))]
        public virtual String Rolename
        {
            get 
            {
                return this._Rolename;
            }
            set
            {
                this._Rolename = value;
            }
        }
        #endregion
        #region NodeID
        public abstract class nodeID : PX.Data.BQL.BqlGuid.Field<nodeID> { }

        protected Guid? _NodeID;
        [PXDBGuid]
        [PXDefault]
        [PXUIField(DisplayName = "Site Map Location", Visibility = PXUIVisibility.SelectorVisible)]        
        public virtual Guid? NodeID
        {
            get
            {
                return this._NodeID;
            }
            set
            {
                this._NodeID = value;
            }
        }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }

        protected String _ScheduleID;

        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXParent(typeof(Select<Schedule, Where<Schedule.scheduleID, Equal<Current<WZScenario.scheduleID>>>>), LeaveChildren = true)]
        public virtual String ScheduleID 
        {
            get
            {
                return this._ScheduleID;
            }
            set
            {
                this._ScheduleID = value;
            }
        }
        #endregion
        #region Scheduled
        public abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }
        protected Boolean? _Scheduled;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Scheduled
        {
            get
            {
                return this._Scheduled;
            }
            set
            {
                this._Scheduled = value;
            }
        }
        #endregion
        #region ScenarioOrder
        public abstract class scenarioOrder : PX.Data.BQL.BqlInt.Field<scenarioOrder> { }

        protected int? _ScenarioOrder;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Order")]
        public virtual int? ScenarioOrder
        {
            get
            {
                return this._ScenarioOrder;
            }
            set
            {
                this._ScenarioOrder = value;
            }
        }
        #endregion


        #region AssignmentMapID
        public abstract class assignmentMapID : PX.Data.BQL.BqlInt.Field<assignmentMapID> { }
        protected int? _AssignmentMapID;
        [PXDBInt]
        [PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID, 
                            Where<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeImplementationScenario>>>), SubstituteKey = typeof(EPAssignmentMap.name))]
        [PXUIField(DisplayName = "Assignment Map")]
        public virtual int? AssignmentMapID
        {
            get
            {
                return this._AssignmentMapID;
            }
            set
            {
                this._AssignmentMapID = value;
            }
        }
        #endregion
        #region WorkgroupID
        public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
        protected int? _WorkgroupID;
        [PXInt]
        [PXSelector(typeof(Search<EPCompanyTree.workGroupID>), SubstituteKey = typeof(EPCompanyTree.description))]
        [PXUIField(DisplayName = "Workgroup ID", Enabled = false)]
        public virtual int? WorkgroupID
        {
            get
            {
                return this._WorkgroupID;
            }
            set
            {
                this._WorkgroupID = value;
            }
        }
        #endregion
        #region OwnerID
        public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
        protected Guid? _OwnerID;
        [PXGuid()]
        [PX.TM.PXOwnerSelector]
        [PXUIField(DisplayName = "Assignee", Enabled = false)]
        public virtual Guid? OwnerID
        {
            get
            {
                return this._OwnerID;
            }
            set
            {
                this._OwnerID = value;
            }
        }
        #endregion

        #region
        public abstract class tasksCompleted : PX.Data.BQL.BqlString.Field<tasksCompleted> { }

        protected String _TasksCompleted;

        [PXString]
        [PXUIField(DisplayName = "Tasks Completed")]
        public virtual String TasksCompleted
        {
            get
            {
                return this._TasksCompleted;
            }
            set
            {
                this._TasksCompleted = value;
            }
        }
        #endregion

        #region IAssign Members

        int? PX.Data.EP.IAssign.WorkgroupID
        {
            get { return WorkgroupID; }
            set { WorkgroupID = value; }
        }

        Guid? PX.Data.EP.IAssign.OwnerID
        {
            get { return OwnerID; }
            set { OwnerID = value; }
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
