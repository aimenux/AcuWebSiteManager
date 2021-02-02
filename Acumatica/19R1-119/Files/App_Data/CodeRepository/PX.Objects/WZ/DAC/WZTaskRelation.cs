using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.Description.GI;

namespace PX.Objects.WZ
{
    [Serializable]
	[PXHidden]
	public partial class WZTaskRelation : IBqlTable
    {
        #region ScenarioID
        public abstract class scenarioID : PX.Data.BQL.BqlGuid.Field<scenarioID> { }

        protected Guid? _ScenarioID;

        [PXDBGuid(IsKey = true)]
        [PXDBDefault(typeof(WZScenario.scenarioID))]
        [PXParent(typeof(Select<WZScenario, Where<WZScenario.scenarioID, Equal<Current<WZTaskRelation.scenarioID>>>>))]
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
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlGuid.Field<taskID> { }

        protected Guid? _TaskID;

        [PXDBGuid(IsKey = true)]
        [PXUIField(DisplayName = "Successor Name", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBDefault(typeof(WZTask.taskID))]
        [PXParent(typeof(Select<WZTask, Where<WZTask.taskID, Equal<Current<WZTaskRelation.taskID>>>>))]
        [PXSelector(typeof(Search<WZTask.taskID,
                                Where<WZTask.scenarioID, Equal<Current<WZTaskRelation.scenarioID>>,
                                And<WZTask.taskID, NotEqual<Current<WZTask.taskID>>>>>), SubstituteKey = typeof(WZTask.name))]
        public virtual Guid? TaskID
        {
            get
            {
                return this._TaskID;
            }
            set
            {
                this._TaskID = value;
            }
        }
        #endregion
        #region PredecessorID
        public abstract class predecessorID : PX.Data.BQL.BqlGuid.Field<predecessorID> { }

        protected Guid? _PredecessorID;

        [PXDBGuid(IsKey = true)]
        [PXUIField(DisplayName = "Predecessor Name", Visible = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
        [PXParent(typeof(Select<WZTask, Where<WZTask.taskID, Equal<Current<WZTaskRelation.predecessorID>>>>))]
        [PXSelector(typeof(Search<WZTask.taskID,
                                Where<WZTask.scenarioID, Equal<Current<WZTaskRelation.scenarioID>>,
                                And<WZTask.taskID, NotEqual<Current<WZTask.taskID>>>>>), SubstituteKey = typeof(WZTask.name))]
        public virtual Guid? PredecessorID
        {
            get
            {
                return this._PredecessorID;
            }
            set
            {
                this._PredecessorID = value;
            }
        }
        #endregion

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp(RecordComesFirst=true)]
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
