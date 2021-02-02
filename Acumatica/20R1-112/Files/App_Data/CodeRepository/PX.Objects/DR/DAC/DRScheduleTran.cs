using System;
using System.Diagnostics;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.CM;

namespace PX.Objects.DR
{
	/// <summary>
	/// Represents a deferred revenue or expense recognition transaction.
	/// Encapsulates the amount to be recognized (or projected to be recognized)
	/// on a particular date and in a particular financial period.
	/// The entities of this type are created by <see cref="TransactionsGenerator"/>
	/// upon the release of <see cref="ARTran"/> or <see cref="APTran"/> document lines
	/// containing a <see cref="DRDeferredCode">deferral code</see>.
	/// Deferral transactions can be added or edited by the user on the Deferral Schedule 
	/// (DR201500) form, which corresponds to the <see cref="DraftScheduleMaint"/> graph.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("SheduleID={ScheduleID} LineNbr={LineNbr} Amount={Amount} RecDate={RecDate}")]
	[PXCacheName(Messages.DRScheduleTran)]
	public partial class DRScheduleTran : PX.Data.IBqlTable
	{
		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }
		protected Int32? _ScheduleID;
		/// <summary>
		/// The identifier of the parent <see cref="DRSchedule">deferral schedule</see>. 
		/// This field is a part of the compound key of the record and is a part of
		/// the foreign key reference to <see cref="DRScheduleDetail"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="DRScheduleDetail.ScheduleID"/> field.
		/// </value>
		[PXParent(typeof(Select<DRScheduleDetail, Where<DRScheduleDetail.scheduleID, Equal<Current<DRScheduleTran.scheduleID>>, And<DRScheduleDetail.componentID, Equal<Current<DRScheduleTran.componentID>>>>>))]
		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(DRScheduleDetail.scheduleID))]
		[PXUIField(DisplayName = Messages.ScheduleNbr, Enabled = false, Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(DRSchedule.scheduleID), SubstituteKey = typeof(DRSchedule.scheduleNbr), DirtyRead = true)]
		public virtual Int32? ScheduleID
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
		#region ComponentID
		public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
		protected Int32? _ComponentID;
		/// <summary>
		/// The component identifier of the parent <see cref="DRScheduleDetail"/>deferral 
		/// schedule component</summary>. This field is a part of the compound key of 
		/// the record and is a part of the foreign key reference to <see cref="DRScheduleDetail"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="DRScheduleDetail.ComponentID"/>.
		/// </value>
		[PXDBLiteDefault(typeof(DRScheduleDetail.componentID))]
		[PXDBInt(IsKey = true)]
		public virtual Int32? ComponentID
		{
			get
			{
				return this._ComponentID;
			}
			set
			{
				this._ComponentID = value;
			}
		}
		#endregion
		#region DetailLineNbr
		public abstract class detailLineNbr : PX.Data.BQL.BqlInt.Field<detailLineNbr> { }

		/// <summary>
		/// The component line number of the parent <see cref="DRScheduleDetail"/>deferral 
		/// schedule component</summary>. This field is a part of the compound key of 
		/// the record and is a part of the foreign key reference to <see cref="DRScheduleDetail"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="DRScheduleDetail.DetailLineNbr"/>.
		/// </value>
		[PXDBLiteDefault(typeof(DRScheduleDetail.detailLineNbr))]
		[PXDBInt(IsKey = true)]
		public virtual int? DetailLineNbr { get; set; }		
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		/// <summary>
		/// The line number of the deferral transaction.
		/// This field is defaulted from the current value of
		/// the <see cref="DRScheduleDetail.LineCntr"/> field 
		/// of the parent schedule component.
		/// </summary>
		/// <remarks>
		/// If the value of this field is equal to the value of 
		/// <see cref="DRScheduleDetail.CreditLineNbr"/> of the parent schedule 
		/// component, the deferral transaction is a "credit line transaction", 
		/// which means that the whole <see cref="DRScheduleDetail.DefTotal">
		/// deferral amount</see> of the parent component should be posted to 
		/// the deferred revenue or expense account.
		/// Otherwise, the deferral transaction is a normal deferred revenue
		/// or expense recognition transaction.
		/// </remarks>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXLineNbr(typeof(DRScheduleDetail.lineCntr))]
		[PXUIField(DisplayName = Messages.TransactionNumber, Enabled = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        protected Int32? _BranchID;
		/// <summary>
		/// The identifier of the <see cref="Branch">branch</see> 
		/// associated with the deferral transaction.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
        [Branch()]
        public virtual Int32? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }
        #endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		/// <summary>
		/// The status of the deferral transaction.
		/// </summary>
		/// <value>
		/// This field can have one of the values defined by
		/// <see cref="DRScheduleTranStatus.ListAttribute"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(DRScheduleTranStatus.Open)]
		[DRScheduleTranStatus.List]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
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
		#region RecDate
		public abstract class recDate : PX.Data.BQL.BqlDateTime.Field<recDate> { }
		protected DateTime? _RecDate;
		/// <summary>
		/// The date on which the associated deferred revenue
		/// or deferred expense <see cref="Amount">amount</see>
		/// is expected to be recognized.
		/// </summary>
		[PXDBDate()]
		[PXDefault()]
		[PXUIField(DisplayName = "Rec. Date")]
		public virtual DateTime? RecDate
		{
			get
			{
				return this._RecDate;
			}
			set
			{
				this._RecDate = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		/// <summary>
		/// The date on which the recognition <see cref="GLTran">
		/// journal transaction</see> was released for the
		/// deferral transaction.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Tran. Date", Enabled=false)]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		protected Decimal? _Amount;
		/// <summary>
		/// The deferred revenue or expense amount to
		/// be recognized (in base currency).
		/// </summary>
		[PXDBBaseCuryAttribute()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? Amount
		{
			get
			{
				return this._Amount;
			}
			set
			{
				this._Amount = value;
			}
		}
		#endregion

		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		[PXString(15, IsUnicode = true)]
		[PXFormula(typeof(Search<APTran.receiptNbr, Where<APTran.tranType, Equal<Current<DRSchedule.docType>>, And<APTran.refNbr, Equal<Current<DRSchedule.refNbr>>, And<APTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>))]
		[PXDefault(typeof(Search<APTran.receiptNbr, Where<APTran.tranType, Equal<Current<DRSchedule.docType>>, And<APTran.refNbr, Equal<Current<DRSchedule.refNbr>>, And<APTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string ReceiptNbr { get; set; }
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }
		[PXString(15, IsUnicode = true)]
		[PXFormula(typeof(Search<APTran.pONbr, Where<APTran.tranType, Equal<Current<DRSchedule.docType>>, And<APTran.refNbr, Equal<Current<DRSchedule.refNbr>>, And<APTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>))]
		[PXDefault(typeof(Search<APTran.pONbr, Where<APTran.tranType, Equal<Current<DRSchedule.docType>>, And<APTran.refNbr, Equal<Current<DRSchedule.refNbr>>, And<APTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PONbr { get; set; }
		#endregion
		#region AllowControlAccountForModuleField
		public abstract class allowControlAccountForModule : PX.Data.BQL.BqlString.Field<allowControlAccountForModule> { }
		[PXString]
		[PXFormula(typeof(Switch<Case<Where<receiptNbr, IsNotNull, Or<pONbr, IsNotNull>>, ControlAccountModule.pO>, Empty>))]
		public virtual string AllowControlAccountForModule { get; set; }
		#endregion

		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		/// <summary>
		/// The identifier of the income or expense account
		/// associated with the transaction.
		/// </summary>
		/// <value>
		/// Corresponds to <see cref="DRScheduleDetail.AccountID"/>.
		/// </value>
		[PXDefault(typeof(DRScheduleDetail.accountID))]
		[PXForeignReference(typeof(Field<accountID>.IsRelatedTo<Account.accountID>))]
		[Account(DisplayName = "Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true,
			AllowControlAccountForModuleField = typeof(allowControlAccountForModule))]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		/// <summary>
		/// The identifier of the income or expense subaccount
		/// associated with the transaction.
		/// </summary>
		/// <value>
		/// Corresponds to <see cref="DRScheduleDetail.SubID"/>.
		/// </value>
		[PXDefault(typeof(DRScheduleDetail.subID))]
		[SubAccount(typeof(DRScheduleTran.accountID), DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		/// <summary>
		/// The identifier of the <see cref="OrganizationFinPeriod">
		/// financial period</see> in which the transaction
		/// is expected to be recognized.
		/// </summary>
		[FinPeriodID(
			branchSourceType: typeof(DRScheduleTran.branchID),
			masterFinPeriodIDType: typeof(DRScheduleTran.tranPeriodID),
			headerMasterFinPeriodIDType: typeof(DRScheduleDetail.tranPeriodID))]
		[PXUIField(DisplayName = "Fin. Period", Enabled=false)]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : IBqlField { }

		[PeriodID]
		public virtual string TranPeriodID { get; set; }
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		/// <summary>
		/// The number of <see cref="Batch">journal entry batch</see>,
		/// which contains recognition entries for the deferral transaction.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Nbr.", Enabled=false)]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region AdjgDocType
		public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
		protected String _AdjgDocType;
		/// <summary>
		/// Represents the adjusting document type for deferral transactions 
		/// created on payment application (if the parent <see cref="DRScheduleDetail">
		/// schedule component</see> uses a deferred code with the 
		/// <see cref="DeferredMethodType.CashReceipt">"on payment"</see> 
		/// recognition method).
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARAdjust.AdjgDocType"/> field.
		/// </value>
		[PXDBString(3, IsFixed = true, InputMask = "")]
		public virtual String AdjgDocType
		{
			get
			{
				return this._AdjgDocType;
			}
			set
			{
				this._AdjgDocType = value;
			}
		}
		#endregion
		#region AdjgRefNbr
		public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
		protected String _AdjgRefNbr;
		/// <summary>
		/// Represents the adjusting document reference number for deferral transactions 
		/// created on payment application (if the parent <see cref="DRScheduleDetail">
		/// schedule component</see> uses a deferred code with the 
		/// <see cref="DeferredMethodType.CashReceipt">"on payment"</see> 
		/// recognition method).
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARAdjust.AdjgRefNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		public virtual String AdjgRefNbr
		{
			get
			{
				return this._AdjgRefNbr;
			}
			set
			{
				this._AdjgRefNbr = value;
			}
		}
		#endregion
		#region AdjNbr
		public abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		protected Int32? _AdjNbr;
		/// <summary>
		/// Represents the associated <see cref="AR.ARAdjust">adjustment</see> number 
		/// for deferral transactions created on payment application (if the parent 
		/// <see cref="DRScheduleDetail">schedule component</see> uses a deferred code 
		/// with the <see cref="DeferredMethodType.CashReceipt">"on payment"</see> 
		/// recognition method).
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARAdjust.AdjNbr"/> field.
		/// </value>
		[PXDBInt()]
		public virtual Int32? AdjNbr
		{
			get
			{
				return this._AdjNbr;
			}
			set
			{
				this._AdjNbr = value;
			}
		}
		#endregion
		#region IsSamePeriod
		[Obsolete(
			"This BQL field is obsolete and will be removed in Acumatica 6.0. To check whether a transaction " +
			"is in the same period as the incoming transaction, perform an explicit check.",
			false)]
		public abstract class isSamePeriod : PX.Data.BQL.BqlBool.Field<isSamePeriod> { }
		protected Boolean? _IsSamePeriod;

		/// <summary>
		/// The property is obsolete and is not used anywhere.
		/// It cannot be removed at the moment due to the bug in the Copy-Paste 
		/// functionality (AC-77988), but should be removed once the source bug 
		/// is fixed.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Same Period", Enabled = true, Visible = false)]
		[Obsolete(
			"This property is obsolete and will be removed in Acumatica 6.0. To check whether a transaction " +
			"is in the same period as the incoming transaction, perform an explicit check.", 
			false)]
		public virtual Boolean? IsSamePeriod
		{
			get
			{
				return this._IsSamePeriod;
			}
			set
			{
				this._IsSamePeriod = value;
			}
		}
		#endregion

		#region System Columns
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
	}

	public static class DRScheduleTranStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new [] { Open, Posted, Projected },
				new [] { Messages.Open, Messages.Posted, Messages.Projected })
			{ }
		}

		public const string Open = "O";
		public const string Posted = "P";
		public const string Projected = "J";

		public class OpenStatus : PX.Data.BQL.BqlString.Constant<OpenStatus>
		{
			public OpenStatus() : base(DRScheduleTranStatus.Open) { ;}
		}

		public class PostedStatus : PX.Data.BQL.BqlString.Constant<PostedStatus>
		{
			public PostedStatus() : base(DRScheduleTranStatus.Posted) { ;}
		}

		public class ProjectedStatus : PX.Data.BQL.BqlString.Constant<ProjectedStatus>
		{
			public ProjectedStatus() : base(DRScheduleTranStatus.Projected) { ;}
		}
	}
}
