using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.SO
{
    [Serializable]
	[PXCacheName(Messages.SOSetupApproval)]
    public partial class SOSetupApproval : PX.Data.IBqlTable, IAssignedMap
    {
		#region Keys
		public class PK: PrimaryKeyOf<SOSetupApproval>.By<approvalID>
		{
			public static SOSetupApproval Find(PXGraph graph, int? approvalID) => FindBy(graph, approvalID);
		}
		public static class FK
		{
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOSetupApproval>.By<orderType> { }
		}
		#endregion
		#region ApprovalID
		public abstract class approvalID : PX.Data.BQL.BqlInt.Field<approvalID> { }
        protected int? _ApprovalID;
        [PXDBIdentity(IsKey = true)]
        public virtual int? ApprovalID
        {
            get
            {
                return this._ApprovalID;
            }
            set
            {
                this._ApprovalID = value;
            }
        }
        #endregion
        #region OrderType
        public class ApprovableOrderTypeSelectorAttribute : PXCustomSelectorAttribute
        {

            public ApprovableOrderTypeSelectorAttribute()
                : base(typeof(Search2<SOOrderType.orderType, InnerJoin<SOOrderTypeOperation, On2<SOOrderTypeOperation.FK.OrderType, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>>>))
            {
            }

            protected virtual System.Collections.IEnumerable GetRecords()
            {
                foreach (PXResult<SOOrderType, SOOrderTypeOperation> it in PXSelectJoin<SOOrderType, InnerJoin<SOOrderTypeOperation, On2<SOOrderTypeOperation.FK.OrderType, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>>>.Select(this._Graph))
                {
                    var ot = (SOOrderType)it;
                    var oto = (SOOrderTypeOperation)it;
                    SOAutomationHelper.SetSupportsApproval(this._Graph, ot);
                    if (ot.SupportsApproval != true)
                        continue;
                    if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && oto.INDocType == INTranType.Transfer)
                        continue;
                    yield return it;
                }
            }
        }

        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
        protected String _OrderType;
        [PXDBString(2, IsFixed = true, InputMask = ">aa")]
        [PXUIField(DisplayName = "SO Type", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
        [ApprovableOrderTypeSelector]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region AssignmentMapID
        public abstract class assignmentMapID : PX.Data.BQL.BqlInt.Field<assignmentMapID> { }
        protected int? _AssignmentMapID;
        [PXDefault]
        [PXDBInt()]
        [PXCheckUnique(typeof(SOSetupApproval.orderType))]
        [PXSelector(
			typeof(Search<
				EPAssignmentMap.assignmentMapID, 
				Where<
					EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeSalesOrder>,
					And<EPAssignmentMap.mapType, NotEqual<EPMapType.assignment>>>>), 
			DescriptionField = typeof(EPAssignmentMap.name))]
        [PXUIField(DisplayName = "Approval Map")]
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
        #region AssignmentNotificationID
        public abstract class assignmentNotificationID : PX.Data.BQL.BqlInt.Field<assignmentNotificationID> { }
        protected int? _AssignmentNotificationID;
        [PXDBInt]
        [PXSelector(typeof(PX.SM.Notification.notificationID), SubstituteKey = typeof(PX.SM.Notification.name))]
        [PXUIField(DisplayName = "Pending Approval Notification")]
        public virtual int? AssignmentNotificationID
        {
            get
            {
                return this._AssignmentNotificationID;
            }
            set
            {
                this._AssignmentNotificationID = value;
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
        #region NonExistence
        public abstract class nonExistence : PX.Data.BQL.BqlBool.Field<nonExistence> { }
        protected bool? _NonExistence = false;
        [PXNonExistence()]
        public virtual bool? NonExistence
        {
            get
            {
                return this._NonExistence;
            }
            set
            {

                this._NonExistence = value;
            }
        }
        #endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected Boolean? _IsActive;
        [PXDBBool()]
        [PXDefault(typeof(Search<SOSetup.orderRequestApproval>), PersistingCheck = PXPersistingCheck.Nothing)]
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
    }
}
