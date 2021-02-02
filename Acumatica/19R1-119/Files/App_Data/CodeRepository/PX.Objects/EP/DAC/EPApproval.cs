using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.TM;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	[PXPrimaryGraph(typeof(EPApprovalMaint))]
	[PXEMailSource]
    [Serializable]
	[PXCacheName(Messages.Approval)]
	public partial class EPApproval : IBqlTable
	{
		#region ApprovalID
		public abstract class approvalID : PX.Data.BQL.BqlInt.Field<approvalID> { }
		protected int? _ApprovalID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "ApprovalID", Visibility = PXUIVisibility.Service)]
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
		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		protected Guid? _RefNoteID;
		[PXRefNote]
		[PXUIField(DisplayName = "References Nbr.")]
		public virtual Guid? RefNoteID
		{
			get
			{
				return this._RefNoteID;
			}
			set
			{
				this._RefNoteID = value;
			}
		}
		#endregion
		#region AssignmentMapID
		public abstract class assignmentMapID : PX.Data.BQL.BqlInt.Field<assignmentMapID> { }
		protected int? _AssignmentMapID;
		[PXDBInt]
		[PXUIField(DisplayName = "Map", Visibility = PXUIVisibility.Service, Enabled = false)]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID, Where<EPAssignmentMap.assignmentMapID, Equal<Current<assignmentMapID>>>>), SubstituteKey = typeof(EPAssignmentMap.name), ValidateValue = false)]
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
		#region StepID
		public abstract class stepID : PX.Data.BQL.BqlGuid.Field<stepID> { }

		[PXDBGuid]
		[PXUIField(DisplayName = "Map Step", Enabled = false)]
		[PXSelector(typeof(Search<EPRule.ruleID, Where<EPRule.ruleID, Equal<Current<stepID>>>>), SubstituteKey = typeof(EPRule.name), ValidateValue = false)]
		public virtual Guid? StepID { get; set; }
		#endregion
		#region RuleID
		public abstract class ruleID : PX.Data.BQL.BqlGuid.Field<ruleID> { }

		[PXDBGuid()]
		[PXUIField(DisplayName = "Map Rule", Enabled = false)]
		[PXSelector(typeof(Search<EPRule.ruleID, Where<EPRule.ruleID, Equal<Current<ruleID>>>>), SubstituteKey = typeof(EPRule.name), ValidateValue = false)]
		public virtual Guid? RuleID { get; set; }
		#endregion
		#region NotificationID
		public abstract class notificationID : PX.Data.BQL.BqlInt.Field<notificationID> { }
        protected int? _NotificationID;
        [PXDBInt()]
        public virtual int? NotificationID
        {
            get
            {
                return this._NotificationID;
            }
            set
            {
                this._NotificationID = value;
            }
        }
        #endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;
		[PXDBInt]
		[PXCompanyTreeSelector(ValidateValue = false)]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
		[PXDBGuid()]
		[PXOwnerSelector]
		[PXUIField(DisplayName = "Assigned Approver ID", Visibility = PXUIVisibility.SelectorVisible)]
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

		protected Guid? _DocumentOwnerID;
		public abstract class documentOwnerID : IBqlField
		{
		}

		[PXDBGuid()]
		[PXOwnerSelector]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Guid? DocumentOwnerID
		{
			get
			{
				return this._DocumentOwnerID;
			}
			set
			{
				this._DocumentOwnerID = value;
			}
		}

		#endregion
		#region ApprovedByID
		public abstract class approvedByID : PX.Data.BQL.BqlGuid.Field<approvedByID> { }
		protected Guid? _ApprovedByID;
		[PXDBGuid()]
		[PX.TM.PXOwnerSelector]
		[PXUIField(DisplayName = "Approver ID", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Guid? ApprovedByID
		{
			get
			{
				return this._ApprovedByID;
			}
			set
			{
				this._ApprovedByID = value;
			}
		}
		#endregion
		#region ApproveDate
		public abstract class approveDate : PX.Data.BQL.BqlDateTime.Field<approveDate> { }
		protected DateTime? _ApproveDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Approval Date", Enabled = false)]
		public virtual DateTime? ApproveDate
		{
			get
			{
				return this._ApproveDate;
			}
			set
			{
				this._ApproveDate = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(new Type[0])]
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
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(EPApprovalStatus.Pending)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[EPApprovalStatus.List()]
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
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		protected DateTime? _DocDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Document Date")]
		public virtual DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Business Account")]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName))]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region WaitTime
		public abstract class waitTime : PX.Data.BQL.BqlInt.Field<waitTime> { }
		protected int? _WaitTime;
		[PXDBInt]
		[PXUIField(DisplayName = "Wait Time", Visibility = PXUIVisibility.Visible)]
		public virtual int? WaitTime
		{
			get
			{
				return this._WaitTime;
			}
			set
			{
				this._WaitTime = value;
			}
		}
		#endregion
		#region PendingWaitTime
		public abstract class pendingWaitTime : PX.Data.BQL.BqlInt.Field<pendingWaitTime> { }
		protected int? _PendingWaitTime;
		[PXInt]
		[PXUIField(DisplayName = "Pending Wait Time", Visibility = PXUIVisibility.Visible)]
		[PXDBCalced(typeof(Switch<Case<Where<EPApproval.status, Equal<EPApprovalStatus.pending>>, DateDiff<EPApproval.createdDateTime, Now, DateDiff.minute>>, Zero>), typeof(int))]
		public virtual int? PendingWaitTime
		{
			get
			{
				return this._PendingWaitTime;
			}
			set
			{
				this._PendingWaitTime = value;
			}
		}
		#endregion

		#region IsPreApproved
		public abstract class isPreApproved : PX.Data.BQL.BqlBool.Field<isPreApproved> { }

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsPreApproved { get; set; }
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(500, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
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
		#region Reason
		public abstract class reason : PX.Data.BQL.BqlString.Field<reason>
		{
		}
		protected String _Reason;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Reason")]
		public virtual String Reason
		{
			get
			{
				return this._Reason;
			}
			set
			{
				this._Reason = value;
			}
		}
		#endregion
        #region Details
        public abstract class details : PX.Data.BQL.BqlString.Field<details> { }
        protected String _Details;
        [PXDBString(500, IsUnicode = true)]
        [PXUIField(DisplayName = "Details")]
        public virtual String Details
        {
            get
            {
                return this._Details;
            }
            set
            {
                this._Details = value;
            }
        }
        #endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryTotalAmount
		public abstract class curyTotalAmount : PX.Data.BQL.BqlDecimal.Field<curyTotalAmount> { }
		protected Decimal? _CuryTotalAmount;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Total Amount")]
		public virtual Decimal? CuryTotalAmount
		{
			get
			{
				return this._CuryTotalAmount;
			}
			set
			{
				this._CuryTotalAmount = value;
			}
		}
		#endregion
		#region TotalAmount
		public abstract class totalAmount : PX.Data.BQL.BqlDecimal.Field<totalAmount> { }
		protected Decimal? _TotalAmount;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? TotalAmount
		{
			get
			{
				return this._TotalAmount;
			}
			set
			{
				this._TotalAmount = value;
			}
		}
		#endregion
		#region SourceItemType
		public abstract class sourceItemType : PX.Data.BQL.BqlString.Field<sourceItemType> { }
		/// <summary>
		/// The detailed (record-level) type of the source entity to 
		/// be approved, e.g. "Bill", "Debit Adjustment", "Cash Return".
		/// </summary>
		[PXDBString]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string SourceItemType
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		private string _DocType;
		/// <summary>
		/// The friendly name of the source entity to be approved's DAC,
		/// as defined by <see cref="PXCacheNameAttribute"/> residing on 
		/// that DAC. For example, "AP Invoice", "SO Order".
		/// </summary>
		[PXEntityName(typeof(EPApproval.refNoteID))]
		[PXUIField(DisplayName = "Document")]
		public virtual string DocType
		{
			get
			{
				return _DocType;
			}
			set
			{
				_DocType = value;
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
		[PXDBCreatedDateTime(UseTimeZone = true)]
		[PXUIField(DisplayName = "Assignment Date")]
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

	public class EPApprovalStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Pending, Approved, Rejected },
				new string[] { Messages.Pending, Messages.Approved, Messages.Rejected }) { ; }
		}


		public const string Pending = "P";
		public const string Approved = "A";
		public const string Rejected = "R";


		public class pending : PX.Data.BQL.BqlString.Constant<pending>
		{
			public pending() : base(Pending) { ;}
		}

		public class approved : PX.Data.BQL.BqlString.Constant<approved>
		{
			public approved() : base(Approved) { ;}
		}

		public class rejected : PX.Data.BQL.BqlString.Constant<rejected>
		{
			public rejected() : base(Rejected) { ;}
		}
	}

}
