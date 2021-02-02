namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.CS;
	
    /// <summary>
    /// Provides access to the preferences of the General Ledger module.
    /// Can be edited by user through the General Ledger Preferences (GL.10.20.00) screen.
    /// </summary>
	[System.SerializableAttribute()]
    [PXPrimaryGraph(typeof(GLSetupMaint))]
    [PXCacheName(Messages.GLSetupMaint)]
    public partial class GLSetup : PX.Data.IBqlTable
	{
		#region YtdNetIncAccountID
		public abstract class ytdNetIncAccountID : PX.Data.BQL.BqlInt.Field<ytdNetIncAccountID> { }
		protected Int32? _YtdNetIncAccountID;

        /// <summary>
        /// Identifier of the Year-to-Date Net Income <see cref="Account"/>.
        /// The account tracks net income accumulated since the beginning of the financial year. It is updated by every transaction posted in the system.
        /// The account must not be changed after any journal transactions have been posted.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// Only Accounts of <see cref="Account.Type">type</see> Liability (<c>"L"</c>) can be specified as YTD Net Income account.
        /// </value>
		[PXDefault]
		[Account(
			null, 
			typeof(Search<Account.accountID, Where<Match<Current<AccessInfo.userName>>>>), 
			DisplayName = "YTD Net Income Account", 
			DescriptionField = typeof(Account.description), 
			Visibility = PXUIVisibility.SelectorVisible,
			AvoidControlAccounts = true)]
        [PXRestrictor(typeof(Where<Account.type, Equal<AccountType.liability>>), Messages.YTDNetIncomeMayBeLiability)]
        public virtual Int32? YtdNetIncAccountID
		{
			get
			{
				return this._YtdNetIncAccountID;
			}
			set
			{
				this._YtdNetIncAccountID = value;
			}
		}
		#endregion
		#region RetEarnAccountID
		public abstract class retEarnAccountID : PX.Data.BQL.BqlInt.Field<retEarnAccountID> { }
		protected Int32? _RetEarnAccountID;

        /// <summary>
        /// Identifier of the Retained Earnings <see cref="Account"/>.
        /// The account is updated by the amount accumulated on the <see cref="YtdNetIncAccountID">YTD Net Income</see> account during the financial year closing.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[Account(
			null, 
			typeof(Search<Account.accountID, Where<Match<Current<AccessInfo.userName>>>>), 
			DisplayName = "Retained Earnings Account", 
			DescriptionField = typeof(Account.description), 
			Visibility = PXUIVisibility.SelectorVisible,
			AvoidControlAccounts = true)]
		public virtual Int32? RetEarnAccountID
		{
			get
			{
				return this._RetEarnAccountID;
			}
			set
			{
				this._RetEarnAccountID = value;
			}
		}
		#endregion
		#region AutoRevOption
		public abstract class autoRevOption : PX.Data.BQL.BqlString.Field<autoRevOption> { }
		protected String _AutoRevOption;

        /// <summary>
        /// Determines when the system generates reversing batches.
        /// </summary>
        /// <value>
        /// Possible values are: <c>"P"</c> - On Post and <c>"C"</c> - On Period Closing.
        /// Default value is On Post.
        /// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(AutoRevOptions.OnPost)]
		[PXUIField(DisplayName = "Generate Reversing Entries", Visibility = PXUIVisibility.Visible)]
		[AutoRevOptions.List()]
		//[PXStringList("P;On Post,C;On Period Closing")]
		public virtual String AutoRevOption
		{
			get
			{
				return this._AutoRevOption;
			}
			set
			{
				this._AutoRevOption = value;
			}
		}
		#endregion
		#region AutoRevEntry
		public abstract class autoRevEntry : PX.Data.BQL.BqlBool.Field<autoRevEntry> { }
		protected Boolean? _AutoRevEntry;

        /// <summary>
        /// Determines whether the system creates negative entries when reversing a <see cref="Batch"/>.
        /// If set to <c>true</c>, the transactions of the reversing batch will retain the Debit/Credit amounts of the original transactions, but with opposite signs.
        /// If set to <c>false</c>, the transactions of the reversing batch will retain the sign of the original amounts, but will have them on the opposite sides of Debit/Credit.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Create Negative Entries on Reversal")]
		public virtual Boolean? AutoRevEntry
		{
			get
			{
				return this._AutoRevEntry;
			}
			set
			{
				this._AutoRevEntry = value;
			}
		}
		#endregion
		#region AutoPostOption
		public abstract class autoPostOption : PX.Data.BQL.BqlBool.Field<autoPostOption> { }
		protected Boolean? _AutoPostOption;

        /// <summary>
        /// Determines whether the system must automatically post <see cref="Batch">Batches</see> once they are released.
        /// When this option is set to <c>true</c> there won't be any batches with status Unposted in the system.
        /// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Automatically Post on Release", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? AutoPostOption
		{
			get
			{
				return this._AutoPostOption;
			}
			set
			{
				this._AutoPostOption = value;
			}
		}
		#endregion
		#region COAOrder
		public abstract class cOAOrder : PX.Data.BQL.BqlShort.Field<cOAOrder> { }
		protected Int16? _COAOrder;

        /// <summary>
        /// Determines the order of <see cref="Account">Accounts</see> in the reports of the General Ledger module.
        /// </summary>
        /// <value>
        /// Allowed values are:
        /// <c>0</c> - 1.Assets, 2.Liabilities, 3.Income and Expenses
        /// <c>1</c> - 1.Assets, 2.Liabilities, 3.Income, 4.Expenses
        /// <c>2</c> - 1.Income, 2. Expenses, 3. Assets, 4.Liabilities
        /// <c>3</c> - 1.Income and Expenses, 2. Assets, 3.Liabilities
        /// <c>128</c> - Custom order. 
        /// If the last option is selected, the order of accounts is determined by the <see cref="Account.COAOrder">COAOrder</see> field of the individual <see cref="Account">Accounts</see>.
        /// Defaults to <c>0</c>.
        /// </value>
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Chart of Accounts Order")]
		public virtual Int16? COAOrder
		{
			get
			{
				return this._COAOrder;
			}
			set
			{
				this._COAOrder = value;
			}
		}
		#endregion
		#region RequireControlTotal
		public abstract class requireControlTotal : PX.Data.BQL.BqlBool.Field<requireControlTotal> { }
		protected Boolean? _RequireControlTotal;

        /// <summary>
        /// Indicates whether the system must enforce validation of batch control totals on Journal Transactions (GL.30.10.00) screen.
        /// </summary>
        /// <value>
        /// When set to <c>true</c> the <see cref="Batch.CuryControlTotal">control total</see> and <see cref="Batch.CuryDebitTotal">debit</see>/<see cref="Batch.CuryCreditTotal">credit total</see> 
        /// of batches must match and user won't be able to save the batch until this condition is met.
        /// Otherwise, validation is performed only after clearing the <see cref="Batch.Hold">Hold</see> flag on the Batch.
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Validate Batch Control Totals on Entry")]
		public virtual Boolean? RequireControlTotal
		{
			get
			{
				return this._RequireControlTotal;
			}
			set
			{
				this._RequireControlTotal = value;
			}
		}
		#endregion	
		#region RequireRefNbrForTaxEntry
		public abstract class requireRefNbrForTaxEntry : PX.Data.BQL.BqlBool.Field<requireRefNbrForTaxEntry> { }
		protected Boolean? _RequireRefNbrForTaxEntry;

        /// <summary>
        /// Indicates whether the Reference Number fields are required on Journal Transactions (GL.30.10.00) screen in case of entering Gl transactions with taxes.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Ref. Numbers for GL Documents with Taxes")]
		public virtual Boolean? RequireRefNbrForTaxEntry
		{
			get
			{
				return this._RequireRefNbrForTaxEntry;
			}
			set
			{
				this._RequireRefNbrForTaxEntry = value;
			}
		}
		#endregion
		#region PostClosedPeriods
		[Obsolete]
		public abstract class postClosedPeriods : PX.Data.BQL.BqlBool.Field<postClosedPeriods> { }
		protected Boolean? _PostClosedPeriods;

        /// <summary>
        /// Determines whether the system allows to post transactions to closed <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Periods</see>.
        /// If set to <c>false</c>, the system raises an error on attempts to enter or post transactions into a closed period.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
        [Obsolete]
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Posting to Closed Periods")]
		public virtual Boolean? PostClosedPeriods
		{
			get
			{
				return this._PostClosedPeriods;
			}
			set
			{
				this._PostClosedPeriods = value;
			}
		}
		#endregion
		#region RestrictAccessToClosedPeriods
		public abstract class restrictAccessToClosedPeriods : PX.Data.BQL.BqlBool.Field<restrictAccessToClosedPeriods> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Restrict Access to Closed Periods")]
		public virtual Boolean? RestrictAccessToClosedPeriods { get; set; }
		#endregion
		#region BatchNumberingID
		public abstract class batchNumberingID : PX.Data.BQL.BqlString.Field<batchNumberingID> { }
		protected String _BatchNumberingID;

        /// <summary>
        /// The identifier of the <see cref="Numbering">Numbering Sequence</see> used for batches.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Numbering.NumberingID"/> field.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
		[PXUIField(DisplayName = "Batch Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID),
			DescriptionField = typeof(Numbering.descr))]
		public virtual String BatchNumberingID
		{
			get
			{
				return this._BatchNumberingID;
			}
			set
			{
				this._BatchNumberingID = value;
			}
		}
		#endregion
		#region DocBatchNumberingID
		public abstract class docBatchNumberingID : PX.Data.BQL.BqlString.Field<docBatchNumberingID> { }
        protected String _DocBatchNumberingID;

        /// <summary>
        /// The identifier of the <see cref="Numbering">Numbering Sequence</see> used for batches of documents
        /// created using the Journal Vouchers (GL.30.40.00) form. See the <see cref="GLDocBatch"/> DAC.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Numbering.NumberingID"/> field.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
		[PXUIField(DisplayName = "Document Batch Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID),
			DescriptionField = typeof(Numbering.descr))]
		public virtual String DocBatchNumberingID
		{
			get
			{
				return this._DocBatchNumberingID;
			}
			set
			{
				this._DocBatchNumberingID = value;
			}
		}
		#endregion
		#region TBImportNumberingID
		public abstract class tBImportNumberingID : PX.Data.BQL.BqlString.Field<tBImportNumberingID> { }
        protected String _TBImportNumberingID;

        /// <summary>
        /// The identifier of the <see cref="Numbering">Numbering Sequence</see> used for imports of trial balances.
        /// See the <see cref="GLTrialBalanceImportMap"/> DAC.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Numbering.NumberingID"/> field.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("TBIMPORT")]
		[PXUIField(DisplayName = "Import Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID),
			DescriptionField = typeof(Numbering.descr))]
		public virtual String TBImportNumberingID
		{
			get
			{
				return this._TBImportNumberingID;
			}
			set
			{
				this._TBImportNumberingID = value;
			}
		}
		#endregion
		#region AllocationNumberingID
		public abstract class allocationNumberingID : PX.Data.BQL.BqlString.Field<allocationNumberingID> { }
        protected String _AllocationNumberingID;

        /// <summary>
        /// The identifier of the <see cref="Numbering">Numbering Sequence</see> used for allocations.
        /// See the <see cref="GLAllocation"/> DAC.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Numbering.NumberingID"/> field.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ALLOCATION")]
		[PXUIField(DisplayName = "Allocation Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID),
			DescriptionField = typeof(Numbering.descr))]
		public virtual String AllocationNumberingID
		{
			get
			{
				return this._AllocationNumberingID;
			}
			set
			{
				this._AllocationNumberingID = value;
			}
		}
		#endregion
		#region ScheduleNumberingID
		public abstract class scheduleNumberingID : PX.Data.BQL.BqlString.Field<scheduleNumberingID> { }
        protected String _ScheduleNumberingID;

        /// <summary>
        /// The identifier of the <see cref="Numbering">Numbering Sequence</see> used for schedules.
        /// See the <see cref="Schedule"/> DAC.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Numbering.NumberingID"/> field.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("SCHEDULE")]
		[PXUIField(DisplayName = "Schedule Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID),
			DescriptionField = typeof(Numbering.descr))]
		public virtual String ScheduleNumberingID
		{
			get
			{
				return this._ScheduleNumberingID;
			}
			set
			{
				this._ScheduleNumberingID = value;
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
		#region HoldEntry
		public abstract class holdEntry : PX.Data.BQL.BqlBool.Field<holdEntry> { }
		protected Boolean? _HoldEntry;

        /// <summary>
        /// Determines whether the system must set the <see cref="Batch.Hold">Hold</see> flag and <see cref="Batch.Status">Status</see> On Hold
        /// when creating new <see cref="Batch">batches</see>.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Batches on Entry")]
		public virtual Boolean? HoldEntry
		{
			get
			{
				return this._HoldEntry;
			}
			set
			{
				this._HoldEntry = value;
			}
		}
		#endregion
        #region VouchersHoldEntry
        public abstract class vouchersHoldEntry : PX.Data.BQL.BqlBool.Field<vouchersHoldEntry> { }
        protected Boolean? _VouchersHoldEntry;

        /// <summary>
        /// Determines whether the system must set the <see cref="GLDocBatch.Hold">Hold</see> flag and <see cref="GLDocBatch.Status">Status</see> On Hold
        /// when creating new <see cref="GLDocBatch">vouchers</see>.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c>.
        /// </value>
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Hold Vouchers on Entry")]
        public virtual Boolean? VouchersHoldEntry
        {
            get
            {
                return this._VouchersHoldEntry;
            }
            set
            {
                this._VouchersHoldEntry = value;
            }
        }
        #endregion
		#region ConsolSegmentId
		public abstract class consolSegmentId : PX.Data.BQL.BqlShort.Field<consolSegmentId>
		{
			public class PXConsolSegmentSelectorAttribute : PXSelectorAttribute
			{
				public PXConsolSegmentSelectorAttribute()
					: base(typeof(Search<Segment.segmentID, Where<Segment.dimensionID, Equal<SubAccountAttribute.dimensionName>>>),
							typeof(Segment.segmentID), typeof(Segment.descr))
				{
					DescriptionField = typeof(Segment.descr);
					_UnconditionalSelect = new Search<Segment.segmentID, Where<Segment.dimensionID, Equal<SubAccountAttribute.dimensionName>,
												And<Segment.segmentID, Equal<Required<Segment.segmentID>>>>>();
				}
			}
		}
		protected Int16? _ConsolSegmentId;

        /// <summary>
        /// The identifier of the <see cref="Segment">Segment</see> of the <see cref="Sub">Subaccount</see> segmented key,
        /// whose values denote the consolidation unit in the subaccounts of the parent company.
        /// Controls the selector on the <see cref="GLConsolSetup.SegmentValue"/> field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Segment.SegmentID"/> field.
        /// </value>
		[PXDBShort()]
		[PXUIField(DisplayName = "Consolidation Segment Number")]
		[consolSegmentId.PXConsolSegmentSelector]
		public virtual Int16? ConsolSegmentId
		{
			get
			{
				return this._ConsolSegmentId;
			}
			set
			{
				this._ConsolSegmentId = value;
			}
		}
		#endregion
		#region PerRetainTran
		public abstract class perRetainTran : PX.Data.BQL.BqlShort.Field<perRetainTran> { }
		protected Int16? _PerRetainTran;

        /// <summary>
        /// Determines the number of periods the system will keep the transactions in the database.
        /// </summary>
        /// <value>
        /// Defaults to <c>99</c>.
        /// </value>
		[PXDBShort()]
		[PXDefault((short)99)]
		[PXUIField(DisplayName = "Keep Transactions for", Visibility = PXUIVisibility.Visible)]
		public virtual Int16? PerRetainTran
		{
			get
			{
				return this._PerRetainTran;
			}
			set
			{
				this._PerRetainTran = value;
			}
		}
		#endregion
		#region DefaultSubID
		public abstract class defaultSubID : PX.Data.BQL.BqlInt.Field<defaultSubID> { }
		protected Int32? _DefaultSubID;

        /// <summary>
        /// Identifier of the <see cref="Sub">Subaccount</see> to be used as the default one with <see cref="Account">Accounts</see>,
        /// which have the <see cref="Account.NoSubDetail">NoSubDetail</see> field set to <c>true</c>.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[SubAccount(DisplayName="Default Subaccount")]
		public virtual Int32? DefaultSubID
		{
			get
			{
				return this._DefaultSubID;
			}
			set
			{
				this._DefaultSubID = value;
			}
		}
		#endregion
		#region TrialBalanceSign
		public abstract class trialBalanceSign : PX.Data.BQL.BqlString.Field<trialBalanceSign>
		{
            public const string Normal = "N";
            public const string Reversed = "R";

            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                        new string[] { Normal, Reversed },
                        new string[] { Messages.Normal, Messages.Reversed }) { }
            }

            public class normal : PX.Data.BQL.BqlString.Constant<normal> { public normal() : base(Normal) {} }
            public class reversed : PX.Data.BQL.BqlString.Constant<reversed> { public reversed() : base(Reversed) {} }

        }
		protected String _TrialBalanceSign;

        /// <summary>
        /// Determines whether the system should automatically change the sign of the trial balance when importing data.
        /// Also affects the way trial balance is displayed on reports and inquiries.
        /// </summary>
        /// <value>
        /// Allowed values are:
        /// <c>"N"</c> - Normal - the system doesn't change the signs of balances of expense and income accounts on import.
        /// On reports and inquiries all the accounts are grouped by account type.
        /// <c>"R"</c> - Reversed - the system reverses the signs of balances of expense and income accounts on import.
        /// On reports and inquiries all the accounts except for the <see cref="YtdNetIncAccountID">Year-to-Date Net Income account</see> are listed.
        /// Defaults to <c>"N"</c>.
        /// </value>
		[PXDBString]
		[trialBalanceSign.List]
		[PXDefault(trialBalanceSign.Normal)]
		[PXUIField(DisplayName = "Sign of the Trial Balance")]
		public virtual String TrialBalanceSign
		{
			get
			{
				return this._TrialBalanceSign;
			}
			set
			{
				this._TrialBalanceSign = value;
			}
		}
        #endregion
        #region ReuseRefNbrsInVouchers
        public abstract class reuseRefNbrsInVouchers : PX.Data.BQL.BqlBool.Field<reuseRefNbrsInVouchers> { }
        protected bool? _ReuseRefNbrsInVouchers;

        /// <summary>
        /// Determines whether the reference numbers for documents created on the Journal Vouchers (GL.30.40.00) form) are reused
        /// in case the number was allocated for the document that was not persisted, or the document was deleted thus freeing the reference number.
        /// When set to <c>true</c>, the system will pick up the free numbers from such documents when creating new entries on the mentioned form.
        /// (Note: this may lead to non-linear numbering.)
        /// Otherwise the form will always assign new numbers to the documents even if there are reusable ones.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c>.
        /// </value>
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Reuse reference numbers in Journal Vouchers")]
        public virtual bool? ReuseRefNbrsInVouchers
        {
            get
            {
                return this._ReuseRefNbrsInVouchers;
            }
            set
            {
                this._ReuseRefNbrsInVouchers = value;
            }
        }
        #endregion
		#region RoundingLimit
		public abstract class roundingLimit : PX.Data.BQL.BqlDecimal.Field<roundingLimit> { }
	    protected decimal? _RoundingLimit;
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0")]
		[PXUIField(DisplayName = "Rounding Limit")]
	    public virtual decimal? RoundingLimit
	    {
		    get { return this._RoundingLimit; }
			set { this._RoundingLimit = value; }
	    }
		#endregion
		#region ConsolidatedPosting
		public abstract class consolidatedPosting : PX.Data.BQL.BqlBool.Field<consolidatedPosting> { }

	    [PXDBBool()]
	    [PXDefault(false)]
	    [PXUIField(DisplayName = "Generate Consolidated Batches")]
	    public virtual bool? ConsolidatedPosting { get; set; }

	    #endregion
		#region AutoReleaseReclassBatch
		public abstract class autoReleaseReclassBatch : PX.Data.BQL.BqlBool.Field<autoReleaseReclassBatch> { }
		protected Boolean? _AutoReleaseReclassBatch;

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Release Reclassification Batches")]
		public virtual Boolean? AutoReleaseReclassBatch
		{
			get
			{
				return this._AutoReleaseReclassBatch;
			}
			set
			{
				this._AutoReleaseReclassBatch = value;
			}
		}
		#endregion
	}

	public static class AutoRevOptions
	{
		public const string OnPost ="P";
		public const string OnPeriodClosing ="C";

		public class ListAttribute : PXStringListAttribute
		{
				public ListAttribute()
					: base(
						new string[] { AutoRevOptions.OnPost, AutoRevOptions.OnPeriodClosing},
						new string[] { Messages.AutoRevOnPost, Messages.AutoRevOnPeriodClosing}) { }
		}	
	}

}
