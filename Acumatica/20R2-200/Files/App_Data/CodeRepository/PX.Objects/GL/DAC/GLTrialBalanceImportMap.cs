using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.GL
{
	[Serializable]
	[PXPrimaryGraph(typeof(JournalEntryImport))]
	[PXCacheName(Messages.GLTrialBalanceImportMap)]
	public partial class GLTrialBalanceImportMap : IBqlTable
	{
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        protected Int32? _BranchID;
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

		#region Number
		public abstract class number : PX.Data.BQL.BqlString.Field<number> { }

		protected String _Number;

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Import Number", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(GLTrialBalanceImportMap.number),
			typeof(GLTrialBalanceImportMap.number), typeof(GLTrialBalanceImportMap.ledgerID), typeof(GLTrialBalanceImportMap.status), typeof(GLTrialBalanceImportMap.branchID), typeof(GLTrialBalanceImportMap.finPeriodID), typeof(GLTrialBalanceImportMap.importDate), typeof(GLTrialBalanceImportMap.totalBalance), typeof(GLTrialBalanceImportMap.batchNbr))]
		[AutoNumber(typeof(GLSetup.tBImportNumberingID), typeof(GLTrialBalanceImportMap.importDate))]
		[PXFieldDescription]
		public virtual String Number
		{
			get { return _Number; }
			set { _Number = value; }
		}

		#endregion

		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleGL>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual String BatchNbr
		{
			get { return _BatchNbr; }
			set { _BatchNbr = value; }
		}
		#endregion

		#region ImportDate
		public abstract class importDate : PX.Data.BQL.BqlDateTime.Field<importDate> { }

		protected DateTime? _ImportDate;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Import Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? ImportDate
		{
			get { return _ImportDate; }
			set { _ImportDate = value; }
		}
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		protected String _FinPeriodID;
		[OpenPeriod(null, typeof(GLTrialBalanceImportMap.importDate), typeof(GLTrialBalanceImportMap.branchID))]
		[PXDefault()]
		[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
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

		#region BegFinPeriod
		public abstract class begFinPeriod : PX.Data.BQL.BqlString.Field<begFinPeriod> { }

		public virtual String BegFinPeriod
		{
			[PXDependsOnFields(typeof(finPeriodID))]
			get
			{
				return this._FinPeriodID == null ? null : 
					string.Concat(FinPeriodUtils.FiscalYear(this._FinPeriodID), "01");
			}
		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		protected String _Description;
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		public virtual String Description
		{
			get { return _Description; }
			set { _Description = value; }
		}
		#endregion

		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		protected Int32? _LedgerID;
		[PXDBInt]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<GLTrialBalanceImportMap.branchID>>>>))]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search5<Ledger.ledgerID,
								LeftJoin<OrganizationLedgerLink,
									On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>>,
									LeftJoin<Branch,
										On<Branch.organizationID, Equal<OrganizationLedgerLink.organizationID>>>>,
								Where<Ledger.balanceType, NotEqual<LedgerBalanceType.budget>,
									And<Branch.branchID, Equal<Current2<GLTrialBalanceImportMap.branchID>>>>,
								Aggregate<GroupBy<Ledger.ledgerID>>>),
			SubstituteKey = typeof(Ledger.ledgerCD),
			DescriptionField = typeof(Ledger.descr))]
		public virtual Int32? LedgerID
		{
			get { return _LedgerID; }
			set { _LedgerID = value; }
		}
		#endregion

		#region IsHold
		public abstract class isHold : PX.Data.BQL.BqlBool.Field<isHold> { }

		protected Boolean? _IsHold;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold")]
		public virtual Boolean? IsHold
		{
			get { return _IsHold; }
			set { _IsHold = value; }
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlInt.Field<status> { }

		protected Int32? _Status;
		[PXDBInt]
		[TrialBalanceImportMapStatus]
		[PXDefault(TrialBalanceImportMapStatusAttribute.HOLD)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public virtual Int32? Status
		{
			get { return _Status; }
			set { _Status = value; }
		}
		#endregion

		#region CreditTotalBalance
		public abstract class creditTotalBalance : PX.Data.BQL.BqlDecimal.Field<creditTotalBalance> { }

		protected Decimal? _CreditTotalBalance;

		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Total", Enabled = false)]
		public virtual Decimal? CreditTotalBalance
		{
			get { return _CreditTotalBalance; }
			set { _CreditTotalBalance = value; }
		}
		#endregion

		#region DebitTotalBalance
		public abstract class debitTotalBalance : PX.Data.BQL.BqlDecimal.Field<debitTotalBalance> { }

		protected Decimal? _DebitTotalBalance;
		
		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Total", Enabled = false)]
		public virtual Decimal? DebitTotalBalance
		{
			get { return _DebitTotalBalance; }
			set { _DebitTotalBalance = value; }
		}
		#endregion

		#region TotalBalance
		public abstract class totalBalance : PX.Data.BQL.BqlDecimal.Field<totalBalance> { }

		protected Decimal? _TotalBalance;

		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Total", Enabled = false)]
		public virtual Decimal? TotalBalance
		{
			get
			{
				return _TotalBalance;
			}
			set
			{
				_TotalBalance = value;
			}
		}
		#endregion

        #region LineCntr
        public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
        protected Int32? _LineCntr;
        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? LineCntr
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

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(DescriptionField = typeof(GLTrialBalanceImportMap.number))]
		public virtual Guid? NoteID { get; set; }
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
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedByID]
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
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
