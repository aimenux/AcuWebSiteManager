using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.CA
{
    [Serializable]
	[PXCacheName(Messages.CASetupApproval)]
	public partial class CASetupApproval : IBqlTable, IAssignedMap
	{
		#region ApprovalID
		public abstract class approvalID : PX.Data.BQL.BqlInt.Field<approvalID> { }

		[PXDBIdentity(IsKey = true)]
		public virtual int? ApprovalID
		{
			get;
			set;
		}
		#endregion		
		#region AssignmentMapID
		public abstract class assignmentMapID : PX.Data.BQL.BqlInt.Field<assignmentMapID> { }

		[PXDefault]
		[PXDBInt]
		[PXSelector(
			typeof(Search<
				EPAssignmentMap.assignmentMapID, 
				Where<
					EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeCashTransaction>,
					And<EPAssignmentMap.mapType, NotEqual<EPMapType.assignment>>>>), 
			DescriptionField = typeof(EPAssignmentMap.name))]
        [PXUIField(DisplayName = "Approval Map")]
		[PXCheckUnique]
		public virtual int? AssignmentMapID
		{
			get;
			set;
		}
		#endregion
        #region AssignmentNotificationID
        public abstract class assignmentNotificationID : PX.Data.BQL.BqlInt.Field<assignmentNotificationID> { }

        [PXDBInt]
        [PXSelector(typeof(PX.SM.Notification.notificationID), SubstituteKey = typeof(PX.SM.Notification.name))]
        [PXUIField(DisplayName = "Pending Approval Notification")]
        public virtual int? AssignmentNotificationID
        {
			get;
			set;
            }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool]
		[PXDefault(typeof(Search<CASetup.requestApproval>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? IsActive
		{
			get;
			set;
        }
        #endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp
			{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
			{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
