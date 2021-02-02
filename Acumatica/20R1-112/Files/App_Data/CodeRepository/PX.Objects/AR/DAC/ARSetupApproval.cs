using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.EP;
using System;

namespace PX.Objects.AR
{
	/// <summary>
	/// The settings for approval of AR documents.
	/// </summary>
	[Serializable]
	public class ARSetupApproval : IBqlTable, IAssignedMap
	{
		#region ApprovalID
		public abstract class approvalID : PX.Data.BQL.BqlInt.Field<approvalID> { }

		[PXDBIdentity(IsKey = true)]
		public virtual int? ApprovalID
		{
			get; set;
		}
		#endregion

		#region AssignmentMapID
		public abstract class assignmentMapID : PX.Data.BQL.BqlInt.Field<assignmentMapID> { }

		[PXDefault]
		[PXDBInt]
		[PXSelector(typeof(
			SearchFor<EPAssignmentMap.assignmentMapID>
			.In<SelectFrom<EPAssignmentMap>
				.Where<
					Brackets<ARSetupApproval.docType.FromCurrent.IsEqual<ARDocType.refund>
						.And<EPAssignmentMap.entityType.IsEqual<AssignmentMapType.AssignmentMapTypeARPayment>>>
					.Or<ARSetupApproval.docType.FromCurrent.IsEqual<ARDocType.cashReturn>
						.And<EPAssignmentMap.entityType.IsEqual<AssignmentMapType.AssignmentMapTypeARCashSale>>>
					.Or<ARSetupApproval.docType.FromCurrent.IsEqual<ARDocType.invoice>
						.And<EPAssignmentMap.entityType.IsEqual<AssignmentMapType.AssignmentMapTypeARInvoice>>>
					.Or<ARSetupApproval.docType.FromCurrent.IsEqual<ARDocType.creditMemo>
						.And<EPAssignmentMap.entityType.IsEqual<AssignmentMapType.AssignmentMapTypeARInvoice>>>
					.Or<ARSetupApproval.docType.FromCurrent.IsEqual<ARDocType.debitMemo>
						.And<EPAssignmentMap.entityType.IsEqual<AssignmentMapType.AssignmentMapTypeARInvoice>>>>>),
			DescriptionField = typeof(EPAssignmentMap.name),
			SubstituteKey = typeof(EPAssignmentMap.name))]
		[PXUIField(DisplayName = "Approval Map")]
		[PXCheckUnique(typeof(ARSetupApproval.docType))]
		public virtual int? AssignmentMapID
		{
			get; set;
		}
		#endregion

		#region AssignmentNotificationID
		public abstract class assignmentNotificationID : PX.Data.BQL.BqlInt.Field<assignmentNotificationID> { }

		[PXDBInt]
		[PXSelector(typeof(PX.SM.Notification.notificationID), SubstituteKey = typeof(PX.SM.Notification.name))]
		[PXUIField(DisplayName = "Pending Approval Notification")]
		public virtual int? AssignmentNotificationID
		{
			get; set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual byte[] tstamp
		{
			get; set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get; set;
		}
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID
		{
			get; set;
		}
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get; set;
		}
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get; set;
		}
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID
		{
			get; set;
		}
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get; set;
		}
		#endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the approval map is applied to documents of the <see cref="DocType"/> type.
		/// </summary>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive
		{
			get; set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		/// <summary>
		/// Specifies the document type to which the approval map is applied.
		/// </summary>
		[PXDBString(3, IsFixed = true)]
		[PXDefault(ARDocType.Refund)]
		[ARDocType.ARApprovalDocTypeList]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		[PXFieldDescription]
		public virtual String DocType
		{
			get; set;
		} 
		#endregion
	}
}
