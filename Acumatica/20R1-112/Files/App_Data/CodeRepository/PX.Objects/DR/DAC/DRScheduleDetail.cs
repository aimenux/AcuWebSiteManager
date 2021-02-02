using System;
using System.Diagnostics;

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.CT;
using PX.Objects.CS;

namespace PX.Objects.DR
{
	/// <summary>
	/// Represents a revenue or expense recognition component of a <see cref="DRSchedule">deferral 
	/// schedule</see>. This entity encapsulates the <see cref="INComponent">inventory item 
	/// component</see> level information of the deferral schedule.
	/// Usually, deferral schedules contain a single component corresponding to the
	/// <see cref="InventoryItem">inventory item</see> specified in the source document
	/// line. However, multiple components can be present in case the inventory item
	/// associated with the document line is a multiple deliverable arrangement (MDA),
	/// or when custom deferral components are added by the user.
	/// The entities of this type are edited on the Deferral Schedule (DR201500) form,
	/// which corresponds to the <see cref="DraftScheduleMaint"/> graph.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("SheduleID={ScheduleID} ComponentID={ComponentID} TotalAmt={TotalAmt} DefAmt={DefAmt}")]
	[PXCacheName(Messages.DRScheduleDetail)]
	[PXPrimaryGraph(
		new []
		{
			typeof(DraftScheduleMaint)
		},
		new [] 
		{
			typeof (Search<
				DRSchedule.scheduleID, 
				Where<DRSchedule.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>)
		}
	)]
	public class DRScheduleDetail : IBqlTable
	{
		public const int EmptyComponentID = 0;
		public const int EmptyBAccountID = 0;

		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }
		protected int? _ScheduleID;
		/// <summary>
		/// The unique identifier of the parent <see cref="DRSchedule">
		/// deferral schedule</see>. This field is a part of the 
		/// compound key of the record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="DRSchedule.ScheduleID"/> field.
		/// </value>
		[PXDBLiteDefault(typeof(DRSchedule.scheduleID))]
		[PXSelector(typeof(DRSchedule.scheduleID), SubstituteKey = typeof(DRSchedule.scheduleNbr), DirtyRead = true)]
		[PXParent(typeof(Select<DRSchedule, Where<DRSchedule.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>))]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = Messages.ScheduleNbr)]
		public virtual int? ScheduleID
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
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch]
		public virtual int? BranchID { get; set; }
		#endregion
		#region ComponentID
		public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
		protected int? _ComponentID;
		/// <summary>
		/// The unique identifier of the <see cref="InventoryItem">
		/// inventory item</see> (or one of its components, in case
		/// of a multiple deliverable arrangement) associated
		/// with the schedule component. This field is a part of the 
		/// compound key of the record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="InventoryItem.InventoryID"/>
		/// field. If the schedule component originates from a
		/// document line with no inventory item specified, the value
		/// of this field is equal to <see cref="EmptyComponentID"/>.
		/// </value>
		/// <remarks>
		/// Within a <see cref="DRSchedule">deferral schedule</see>, there 
		/// can be only one schedule component with <see cref="ComponentID"/> 
		/// equal to <see cref="EmptyComponentID"/>.
		/// </remarks>
		[PXDBInt(IsKey=true)]
		[PXUIField(DisplayName = Messages.ComponentID, Visibility = PXUIVisibility.Visible)]
		[DRComponentSelector(SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
		public virtual int? ComponentID
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
		#region InventoryID
		public abstract class parentInventoryID : PX.Data.BQL.BqlInt.Field<parentInventoryID> { }

		[PXInt]
		public virtual int? ParentInventoryID { get; set; }
		#endregion
		#region DetailLineNbr
		public abstract class detailLineNbr : PX.Data.BQL.BqlInt.Field<detailLineNbr> { }

		/// <summary>
		/// The line number of the component.
		/// This field is defaulted from the current value of
		/// the <see cref="DRSchedule.DetailLineCntr"/> field 
		/// of the parent deferred schedule.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXLineNbr(typeof(DRSchedule.detailLineCntr), DecrementOnDelete = true)]
		[PXUIField(DisplayName = "Detail Line Nbr.", Enabled = false)]
		public virtual int? DetailLineNbr { get; set; }
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;
		/// <summary>
		/// The module associated with the schedule component.
		/// This field defaults to <see cref="DRSchedule.Module"/>.
		/// </summary>
		/// <value>
		/// This field can have one of the following values:
		/// <c>"AP"</c>: Accounts Payable,
		/// <c>"AR"</c>: Accounts Receivable.
		/// </value>
		/// <remarks>
		/// Any changes made to this field also affect the value
		/// of the <see cref="DefCodeType"/> field.
		/// </remarks>
		[PXDefault(typeof(DRSchedule.module))]
		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[] { BatchModule.AR, BatchModule.AP }, new string[] { Messages.Income, Messages.Expense })]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.Invisible)]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;

				switch (value)
				{
					case BatchModule.AP:
						_BAccountType = CR.BAccountType.VendorType;
						DefCodeType = DeferredAccountType.Expense;
						break;
					default:
						_BAccountType = CR.BAccountType.CustomerType;
						DefCodeType = DeferredAccountType.Income;
						break;
				}

			}
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		/// <summary>
		/// The type of the document from which the schedule component
		/// has been created. The value defaults to <see cref="DRSchedule.DocType"/>.
		/// </summary>
		[PXDefault(ARDocType.Invoice)]
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Doc. Type", Visibility = PXUIVisibility.Invisible)]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		/// <summary>
		/// The reference number of the document from which the 
		/// schedule component has been created. The value defaults to
		/// <see cref="DRSchedule.RefNbr"/>.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[DRDocumentSelector(typeof(DRScheduleDetail.module), typeof(DRScheduleDetail.docType))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected int? _LineNbr;
		/// <summary>
		/// The document line number associated with the
		/// parent <see cref="DRSchedule">deferral schedule</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="DRSchedule.LineNbr"/> field.
		/// </value>
		[PXDBInt()]
		[PXUIField(DisplayName = "Line Nbr.", Enabled = false, Visible = false)]
		public virtual int? LineNbr
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
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		[PXDBLong]
		[CurrencyInfo(typeof(DRSchedule.curyInfoID))]
		public virtual long? CuryInfoID { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		/// <summary>
		/// The status of the schedule component.
		/// </summary>
		/// <value>
		/// This field can have one of the values defined by
		/// <see cref="DRScheduleStatus.ListAttribute"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(DRScheduleStatus.Open)]
		[DRScheduleStatus.List]
		[PXUIField(DisplayName = "Status", Enabled = false, IsReadOnly = true, Visibility=PXUIVisibility.SelectorVisible)]
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
		#region CuryTotalAmt
		public abstract class curyTotalAmt : PX.Data.BQL.BqlDecimal.Field<curyTotalAmt> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(totalAmt))]
		public virtual decimal? CuryTotalAmt { get; set; }
		#endregion
		#region TotalAmt
		public abstract class totalAmt : PX.Data.BQL.BqlDecimal.Field<totalAmt> { }
		protected decimal? _TotalAmt;
		/// <summary>
		/// The total amount of expense or revenue that is associated
		/// with the component and should be posted to the Accounts
		/// Receivable or Accounts Payable account.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(DRSchedule.origLineAmt))]
		[PXUIField(DisplayName = "Total Amount")]
		public virtual decimal? TotalAmt
		{
			get
			{
				return this._TotalAmt;
			}
			set
			{
				this._TotalAmt = value;
			}
		}
		#endregion
		#region CuryDefAmt
		public abstract class curyDefAmt : PX.Data.BQL.BqlDecimal.Field<curyDefAmt> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(defAmt))]
		public virtual decimal? CuryDefAmt { get; set; }
		#endregion
		#region DefAmt
		public abstract class defAmt : PX.Data.BQL.BqlDecimal.Field<defAmt> { }
		protected decimal? _DefAmt;
		/// <summary>
		/// The remaining deferred revenue or expense amount to be recognized 
		/// upon the release of <see cref="DRScheduleTran">deferral 
		/// transactions</see> associated with the component. 
		/// </summary>
		/// <value>
		/// For normal components, the value of this field is initially 
		/// equal to <see cref="TotalAmt"/>, and is decreased during the 
		/// /// revenue or expense recognition process (which is performed on 
		/// the Run Recognition (DR501000) form). For residual components of 
		/// multiple deliverable arrangement items (for which the 
		/// <see cref="IsResidual"/> flag is set to <c>true</c>), this value 
		/// is zero because the amount is directly posted to the AP or AR account
		/// without deferral.
		/// </value>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deferred Amount", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? DefAmt
		{
			get
			{
				return this._DefAmt;
			}
			set
			{
				this._DefAmt = value;
			}
		}
		#endregion
		#region DefCode
		public abstract class defCode : PX.Data.BQL.BqlString.Field<defCode> { }
		protected String _DefCode;
		/// <summary>
		/// The identifier of the <see cref="DRDeferredCode">
		/// deferral code</see> associated with the component.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="DRDeferredCode.DeferredCodeID"/>
		/// field.
		/// </value>
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, Where<DRDeferredCode.accountType, Equal<Current<DRScheduleDetail.defCodeType>>>>))]
        [PXRestrictor(typeof(Where<DRDeferredCode.active, Equal<True>>), DR.Messages.InactiveDeferralCode, typeof(DRDeferredCode.deferredCodeID))]
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Deferral Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIEnabled(typeof(Where<Current<DRScheduleDetail.isCustom>, Equal<True>>))]
		[PXForeignReference(typeof(Field<DRScheduleDetail.defCode>.IsRelatedTo<DRDeferredCode.deferredCodeID>))]
		public virtual String DefCode
		{
			get
			{
				return this._DefCode;
			}
			set
			{
				this._DefCode = value;
			}
		}
		#endregion
		#region DefAcctID
		public abstract class defAcctID : PX.Data.BQL.BqlInt.Field<defAcctID> { }
		protected int? _DefAcctID;
		/// <summary>
		/// The identifier of the deferred revenue or deferred expense 
		/// account associated with the component.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXDefault]
		// If deferral code type is "revenue", then deferral
		// account should be of the Liability type.
		// -
		[PXUIVerify(
			typeof(Where<
				DRScheduleDetail.defAcctID, IsNull, Or<
				DRScheduleDetail.defCodeType, NotEqual<DeferredAccountType.income>, Or<
				Selector<DRScheduleDetail.defAcctID, Account.type>, Equal<AccountType.liability>>>>), 
			PXErrorLevel.Warning,
			Messages.AccountTypeIsNotLiability)]
		// If deferral code type is "expense", then deferral
		// account should be of the Asset type.
		// -
		[PXUIVerify(
			typeof(Where<
				DRScheduleDetail.defAcctID, IsNull, Or<
				DRScheduleDetail.defCodeType, NotEqual<DeferredAccountType.expense>, Or<
				Selector<DRScheduleDetail.defAcctID, Account.type>, Equal<AccountType.asset>>>>),
			PXErrorLevel.Warning,
			Messages.AccountTypeIsNotAsset)]
		[Account(DisplayName = "Deferral Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.DR)]
		public virtual int? DefAcctID
		{
			get
			{
				return this._DefAcctID;
			}
			set
			{
				this._DefAcctID = value;
			}
		}
		#endregion
		#region DefSubID
		public abstract class defSubID : PX.Data.BQL.BqlInt.Field<defSubID> { }
		protected int? _DefSubID;
		/// <summary>
		/// The identifier of the deferred revenue or deferred expense
		/// subaccount associated with the component.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXDefault]
        [SubAccount(typeof(DRScheduleDetail.defAcctID), DisplayName = "Deferral Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual int? DefSubID
		{
			get
			{
				return this._DefSubID;
			}
			set
			{
				this._DefSubID = value;
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
		protected int? _AccountID;
		/// <summary>
		/// The identifier of the income or expense account associated 
		/// with the component.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXDefault]
		[Account(DisplayName = "Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true,
			AllowControlAccountForModuleField = typeof(allowControlAccountForModule))]
		public virtual int? AccountID
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
		protected int? _SubID;
		/// <summary>
		/// The identifier of the income or expense subaccount
		/// associated with the component.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXDefault]
		[SubAccount(typeof(DRScheduleDetail.accountID), DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual int? SubID
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
		#region IsOpen
		public abstract class isOpen : PX.Data.BQL.BqlBool.Field<isOpen> { }
		protected Boolean? _IsOpen;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the component
		/// is open, which means that there are unrecognized
		/// <see cref="DRScheduleTran">deferral transactions</see>
		/// associated with the component.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "IsOpen")]
		public virtual Boolean? IsOpen
		{
			get
			{
				return this._IsOpen;
			}
			set
			{
				this._IsOpen = value;
			}
		}
		#endregion
		
		#region DefTotal
		public abstract class defTotal : PX.Data.BQL.BqlDecimal.Field<defTotal> { }
		protected decimal? _DefTotal;
		/// <summary>
		/// The total deferred revenue or expense amount to be recognized 
		/// upon the release of <see cref="DRScheduleTran">deferral 
		/// transactions</see> associated with the component. 
		/// </summary>
		[PXBaseCury]
		[PXUIField(DisplayName = "Line Total", Enabled = false)]
		public virtual decimal? DefTotal
		{
			get
			{
				return this._DefTotal;
			}
			set
			{
				this._DefTotal = value;
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected int? _LineCntr;
		/// <summary>
		/// The number of <see cref="DRScheduleTran">deferral transactions
		/// </see> associated with the component. When a deferral transaction
		/// is added to the component, this field provides the value for
		/// <see cref="DRScheduleTran.LineNbr"/>, being incremented after
		/// each added transaction.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		protected DateTime? _DocDate;
		/// <summary>
		/// The date of the parent <see cref="DRSchedule">
		/// deferral schedule</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="DRSchedule.DocDate"/> field.
		/// </value>
		[PXDBDate()]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = Messages.Date, Enabled = false)]
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
		#region TranPeriodID
		public abstract class tranPeriodID : IBqlField { }

		[PeriodID]
		public virtual string TranPeriodID { get; set; }
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		
		/// <summary>
		/// The financial period ID of the parent <see cref="DRSchedule">
		/// deferral schedule</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="DRSchedule.FinPeriodID"/> field.
		/// </value>
		[PXDefault]
		[APAROpenPeriod(typeof(DRScheduleDetail.module), typeof(DRSchedule.docDate),
			branchSourceType: typeof(DRScheduleDetail.branchID),
			masterFinPeriodIDType: typeof(DRScheduleDetail.tranPeriodID))]
		[PXUIField(DisplayName = "Post Period", Enabled = false)]
		public virtual string FinPeriodID { get; set; }
		#endregion
		#region LastRecFinPeriodID
		public abstract class lastRecFinPeriodID : PX.Data.BQL.BqlString.Field<lastRecFinPeriodID> { }
		protected String _LastRecFinPeriodID;
		/// <summary>
		/// The financial period of the latest recognition of a 
		/// <see cref="DRScheduleTran">deferral transaction</see> 
		/// associated with the component.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="OrganizationFinPeriod.FinPeriodID"/> field.
		/// </value>
		[FinPeriodID]
		[PXUIField(DisplayName = "Last Recognition Period", Enabled = false)]
		public virtual String LastRecFinPeriodID
		{
			get
			{
				return this._LastRecFinPeriodID;
			}
			set
			{
				this._LastRecFinPeriodID = value;
			}
		}
		#endregion
		#region CloseFinPeriodID
		public abstract class closeFinPeriodID : PX.Data.BQL.BqlString.Field<closeFinPeriodID> { }
		protected String _CloseFinPeriodID;
		/// <summary>
		/// The financial period in which all <see cref="DRScheduleTran">
		/// deferral transactions</see> associated with the component
		/// have been recognized and <see cref="DefAmt"/> has become zero.
		/// </summary>
		[FinPeriodID]
		[PXUIField(DisplayName = "Close Period", Enabled = false)]
		public virtual String CloseFinPeriodID
		{
			get
			{
				return this._CloseFinPeriodID;
			}
			set
			{
				this._CloseFinPeriodID = value;
			}
		}
		#endregion
		#region CreditLineNbr
		public abstract class creditLineNbr : PX.Data.BQL.BqlInt.Field<creditLineNbr> { }
		protected int? _CreditLineNbr;
		/// <summary>
		/// The integer line number of the <see cref="DRScheduleTran">
		/// deferral transaction</see>, which posts the <see cref="TotalAmt">total 
		/// component amount</see> to the deferral account specified by the 
		/// <see cref="DefAcctID"/> field. The value defaults to zero.
		/// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Credit Line Nbr.", Visibility=PXUIVisibility.Invisible, Enabled = false)]
		public virtual int? CreditLineNbr
		{
			get
			{
				return this._CreditLineNbr;
			}
			set
			{
				this._CreditLineNbr = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected int? _BAccountID;
		/// <summary>
		/// The unique identifier of the <see cref="BAccount">
		/// business account</see> associated with the component.
		/// </summary>
		/// <value>
		/// The value of this field is always equal to the
		/// value of the <see cref="DRSchedule.BAccountID"/>
		/// field in the parent schedule.
		/// </value>
		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = Messages.BusinessAccount, Enabled = false)]
		[PXSelector(typeof(Search<
			BAccountR.bAccountID, 
			Where<
				BAccountR.type, Equal<Current<DRScheduleDetail.bAccountType>>, 
				Or<BAccountR.type, Equal<BAccountType.combinedType>>>>), 
			new Type[] 
			{
				typeof(BAccountR.acctCD),
				typeof(BAccountR.acctName),
				typeof(BAccountR.type)
			}, 
			SubstituteKey = typeof(BAccountR.acctCD),
			DescriptionField = typeof(BAccountR.acctName))]
		public virtual int? BAccountID
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
		#region BAccountID_BAccount_acctName
		public abstract class bAccountID_BAccount_acctName : PX.Data.BQL.BqlString.Field<bAccountID_BAccount_acctName> { }
		#endregion
		#region IsCustom
		public abstract class isCustom : PX.Data.BQL.BqlBool.Field<isCustom> { }
		protected Boolean? _IsCustom;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the
		/// schedule component has been manually added to 
		/// the parent <see cref="DRSchedule">deferral schedule
		/// </see> by the user.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Custom", Enabled=false)]
		public virtual Boolean? IsCustom
		{
			get
			{
				return this._IsCustom;
			}
			set
			{
				this._IsCustom = value;
			}
		}
		#endregion

		#region DocumentType
		public abstract class documentType : PX.Data.BQL.BqlString.Field<documentType> { }
		protected String _DocumentType;
		/// <summary>
		/// The type of the document to which the parent 
		/// <see cref="DRSchedule">deferral schedule</see>
		/// corresponds.
		/// </summary>
		/// <value>
		/// The value of this field is always equal to the
		/// value of the <see cref="DRSchedule.DocumentType"/>
		/// field in the parent deferral schedule.
		/// </value>
		[PXString(3, IsFixed = true)]
		[DRScheduleDocumentType.List]
		[PXUIField(DisplayName = "Doc. Type", Visibility=PXUIVisibility.SelectorVisible, Enabled = false, Required=true)]
		public virtual String DocumentType
		{
			get
			{
				return this._DocumentType;
			}
			set
			{
				this._DocumentType = value;
			}
		}
		#endregion

		#region BAccountType
		public abstract class bAccountType : PX.Data.BQL.BqlString.Field<bAccountType> { }
		protected String _BAccountType;
		/// <summary>
		/// The type of the <see cref="BAccount">business account</see> associated
		/// with the current schedule component.
		/// </summary>
		[PXDefault(CR.BAccountType.CustomerType, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXString(2, IsFixed = true)]
		[PXStringList(	new string[] { CR.BAccountType.VendorType, CR.BAccountType.CustomerType },
				new string[] { CR.Messages.VendorType, CR.Messages.CustomerType })]
		public virtual String BAccountType
		{
			get
			{
				return this._BAccountType;
			}
			set
			{
				this._BAccountType = value;
			}
		}
		#endregion

		#region DefCodeType
		public abstract class defCodeType : PX.Data.BQL.BqlString.Field<defCodeType> { }
		protected string _DefCodeType;
		/// <summary>
		/// The type of the deferral account for the schedule component.
		/// </summary>
		/// <value>
		/// This field can have one of the values defined in the
		/// <see cref="DeferredAccountType"/> class.
		/// </value>
		/// <remarks>
		/// The value of this field changes automatically upon changing
		/// the <see cref="Module"/> field.
		/// </remarks>
		[PXString(1)]
		[PXDefault(DeferredAccountType.Income)]
		[LabelList(typeof(DeferredAccountType))]
		public virtual string DefCodeType
		{
			get
			{
				return this._DefCodeType;
			}
			set
			{
				this._DefCodeType = value;
			}
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the record
		/// has been selected for processing in the UI.
		/// </summary>
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Visible)]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		
		/// <summary>
		/// The unique identifier of the <see cref="PM.PMProject">
		/// project</see> specified in the document line (if any)
		/// associated with the parent <see cref="DRSchedule">deferral schedule</see>.
		/// </summary>
		[PXDBInt]
		[PXDefault(typeof(Search<ARTran.projectID, Where<ARTran.tranType, Equal<Current<DRSchedule.docType>>, 
			And<ARTran.refNbr, Equal<Current<DRSchedule.refNbr>>, And<ARTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDimensionSelector("CONTRACT", typeof(Search<Contract.contractID>), typeof(Contract.contractCD))]
		[PXUIField(DisplayName = "Contract ID", Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual int? ProjectID { get; set; }
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

		[PXDBInt]
		[PXDefault(typeof(Search<ARTran.taskID, Where<ARTran.tranType, Equal<Current<DRSchedule.docType>>,
			And<ARTran.refNbr, Equal<Current<DRSchedule.refNbr>>, And<ARTran.lineNbr, Equal<Current<DRSchedule.lineNbr>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<ContractTask.taskID, Where<ContractTask.contractID, Equal<Optional<ARTran.projectID>>>>), SubstituteKey = typeof(ContractTask.taskCD))]
		[PXUIField(DisplayName = "Project Task", Enabled = false, FieldClass = nameof(FeaturesSet.ASC606))]
		public virtual int? TaskID { get; set; }
		#endregion
		#region IsResidual
		public abstract class isResidual : PX.Data.BQL.BqlBool.Field<isResidual> { }

		/// <summary>
		/// If set to <c>true</c>, indicates that the deferral schedule line 
		/// represents a residual component whose amount should be directly
		/// posted to the AP or AR account upon component release (without
		/// generating or recognizing <see cref="DRScheduleTran">deferral
		/// transactions</see>). For details, see <see cref="INComponent.AmtOption"/>.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public bool? IsResidual { get; set; }
		#endregion

		#region ASC606
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<componentID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(componentID))]
		public virtual string UOM { get; set; }
		#endregion
		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Base Qty.", Visible = false, Enabled = false)]
		public virtual Decimal? BaseQty { get; set; }
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

		[PXDBQuantity(typeof(uOM), typeof(baseQty))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Quantity")]
		public virtual decimal? Qty { get; set; }
		#endregion
		#region FairValuePrice
		public abstract class fairValuePrice : PX.Data.BQL.BqlDecimal.Field<fairValuePrice> { }

		[PXDBPriceCost]
		[PXUIField(DisplayName = "Fair Value Price")]
		public virtual decimal? FairValuePrice { get; set; }
		#endregion
		#region Percentage
		public abstract class percentage : PX.Data.BQL.BqlDecimal.Field<percentage>
		{
			public const int SharePrecision = 5;
		}

		[PXDBDecimal(percentage.SharePrecision)]
		[PXUIField(DisplayName = "Share")]
		public virtual decimal? Percentage { get; set; }
		#endregion
		#region CoTermRate
		public abstract class coTermRate : PX.Data.BQL.BqlDecimal.Field<coTermRate> { }

		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "CoTerm Rate", Visible = false, Enabled = false)]
		public virtual Decimal? CoTermRate { get; set; }
		#endregion
		#region EffectiveFairValuePrice
		public abstract class effectiveFairValuePrice : PX.Data.BQL.BqlDecimal.Field<effectiveFairValuePrice> { }

		[PXDBPriceCost]
		[PXUIField(DisplayName = "Effective Fair Value Price")]
		public virtual decimal? EffectiveFairValuePrice { get; set; }
		#endregion
		#region FairValueCuryID
		public abstract class fairValueCuryID : IBqlField { }

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Fair Value Currency", Enabled = false)]
		public virtual string FairValueCuryID { get; set; }
		#endregion
		#endregion
		#region TermStartDate
		public abstract class termStartDate : PX.Data.BQL.BqlDateTime.Field<termStartDate> { }

		[PXDBDate]
		[PXDefault(typeof(Search<ARTran.dRTermStartDate, Where<ARTran.tranType, Equal<Current<docType>>,
											And<ARTran.refNbr, Equal<Current<refNbr>>,
												And<ARTran.lineNbr, Equal<Current<lineNbr>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Term Start Date", Enabled = false, FieldClass = nameof(FeaturesSet.ASC606))]
		public virtual DateTime? TermStartDate { get; set; }
		#endregion
		#region TermEndDate
		public abstract class termEndDate : PX.Data.BQL.BqlDateTime.Field<termEndDate> { }

		[PXDBDate]
		[PXDefault(typeof(Search<ARTran.dRTermEndDate, Where<ARTran.tranType, Equal<Current<docType>>,
											And<ARTran.refNbr, Equal<Current<refNbr>>,
												And<ARTran.lineNbr, Equal<Current<lineNbr>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Term End Date", Enabled = false, FieldClass = nameof(FeaturesSet.ASC606))]
		public virtual DateTime? TermEndDate { get; set; }
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

	public static class DRScheduleStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Open, Closed, Draft },
				new string[] { Messages.Open, Messages.Closed, Messages.Draft }) { ; }
		}
		public const string Open = "O";
		public const string Closed = "C";
		public const string Draft = "D";

		public class OpenStatus : PX.Data.BQL.BqlString.Constant<OpenStatus>
		{
			public OpenStatus() : base(DRScheduleStatus.Open) { ;}
		}
		public class ClosedStatus : PX.Data.BQL.BqlString.Constant<ClosedStatus>
		{
			public ClosedStatus() : base(DRScheduleStatus.Closed) { ;}
		}
		public class DraftStatus : PX.Data.BQL.BqlString.Constant<DraftStatus>
		{
			public DraftStatus() : base(DRScheduleStatus.Draft) { ;}
		}
	}

	public static class DRScheduleDocumentType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Invoice, ARDocType.CreditMemo, ARDocType.DebitMemo, ARDocType.CashSale, ARDocType.CashReturn, Bill, APDocType.CreditAdj, APDocType.DebitAdj, APDocType.QuickCheck, APDocType.VoidQuickCheck },
				new string[] { AR.Messages.Invoice, AR.Messages.CreditMemo, AR.Messages.DebitMemo, AR.Messages.CashSale, AR.Messages.CashReturn, AP.Messages.Invoice, AP.Messages.CreditAdj, AP.Messages.DebitAdj, AP.Messages.QuickCheck, AP.Messages.VoidQuickCheck }) { ; }
		}
		public const string Invoice = "ARI";
		public const string Bill = "API";

		public static string ExtractModule(string documentType)
		{
			switch (documentType)
			{
				case Invoice:
				case ARDocType.CreditMemo:
				case ARDocType.DebitMemo:
					return BatchModule.AR;
				case Bill:
				case APDocType.CreditAdj:
				case APDocType.DebitAdj:
					return BatchModule.AP;
				case APDocType.QuickCheck:
				case APDocType.VoidQuickCheck:
				case ARDocType.CashSale:
				case ARDocType.CashReturn:
					// TODO: we will allow these document types when AC-68080 is fixed.
				default:
					throw new PXException(Messages.DocumentTypeNotSupported);
			}
		}

		public static string ExtractDocType(string documentType)
		{
			switch (documentType)
			{
				case Invoice:
					return ARDocType.Invoice;
				case Bill:
					return APDocType.Invoice;
				default:
					return documentType;
			}
		}

		public static string BuildDocumentType(string module, string docType)
		{
			switch (docType)
			{
				case APDocType.Invoice:	//case ARDocType.Invoice:
					if (module == BatchModule.AR)
						return Invoice;
					else
						return Bill;
				default:
					return docType;
			}
		}
	}
}
