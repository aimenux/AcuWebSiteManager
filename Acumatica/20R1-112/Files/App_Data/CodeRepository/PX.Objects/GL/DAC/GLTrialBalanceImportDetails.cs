using System;
using PX.Data;

namespace PX.Objects.GL
{
    [Serializable]
    [PXCacheName(Messages.GLTrialBalanceImportDetails)]
    public partial class GLTrialBalanceImportDetails : IBqlTable
    {
        #region MapNumber
        public abstract class mapNumber : PX.Data.BQL.BqlString.Field<mapNumber> { }

        protected String _MapNumber;
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXDBDefault(typeof(GLTrialBalanceImportMap.number))]
        [PXUIField(Visible = false)]
        [PXParent(typeof(Select<GLTrialBalanceImportMap, Where<GLTrialBalanceImportMap.number, Equal<Current<GLTrialBalanceImportDetails.mapNumber>>>>))]
        public virtual String MapNumber
        {
            get { return _MapNumber; }
            set { _MapNumber = value; }
        }
        #endregion

        #region Line
        public abstract class line : PX.Data.BQL.BqlInt.Field<line> { }

        protected Int32? _Line;
        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(GLTrialBalanceImportMap.lineCntr))]
        [PXUIField(Visible = false)]
        public virtual Int32? Line
        {
            get { return _Line; }
            set { _Line = value; }
        }
        #endregion

        #region ImportAccountCDError
        public abstract class importAccountCDError : PX.Data.BQL.BqlString.Field<importAccountCDError> { }

        protected String _ImportAccountCDError;
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(Visible = false)]
        public virtual String ImportAccountCDError
        {
            get { return _ImportAccountCDError; }
            set { _ImportAccountCDError = value; }
        }
        #endregion

        #region ImportAccountCD
        public abstract class importAccountCD : PX.Data.BQL.BqlString.Field<importAccountCD> { }

        protected String _ImportAccountCD;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Imported Account", FieldClass = AccountAttribute.DimensionName)]
        [PXDimensionSelector(AccountAttribute.DimensionName, typeof(Search<Account.accountCD,
                Where2<Match<Current<AccessInfo.userName>>,
                And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>), DescriptionField = typeof(Account.description))]
        [PersistError(typeof(GLTrialBalanceImportDetails.importAccountCDError))]
        public virtual String ImportAccountCD
        {
            get { return _ImportAccountCD; }
            set { _ImportAccountCD = value; }
        }
        #endregion

        #region MapAccountID
        public abstract class mapAccountID : PX.Data.BQL.BqlInt.Field<mapAccountID> { }

        protected Int32? _MapAccountID;
        [Account(DisplayName = "Mapped Account", Enabled = false)]
        public virtual Int32? MapAccountID
        {
            get { return _MapAccountID; }
            set { _MapAccountID = value; }
        }
        #endregion

        #region ImportSubAccountCDError
        public abstract class importSubAccountCDError : PX.Data.BQL.BqlString.Field<importSubAccountCDError> { }

        protected String _ImportSubAccountCDError;
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(Visible = false)]
        public virtual String ImportSubAccountCDError
        {
            get { return _ImportSubAccountCDError; }
            set { _ImportSubAccountCDError = value; }
        }
        #endregion

        #region ImportSubAccountCD
        public abstract class importSubAccountCD : PX.Data.BQL.BqlString.Field<importSubAccountCD> { }

        protected String _ImportSubAccountCD;
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Imported Subaccount", FieldClass = SubAccountAttribute.DimensionName)]
        [PXDimensionSelector(SubAccountAttribute.DimensionName, typeof(Search<Sub.subCD, Where<Match<Current<AccessInfo.userName>>>>))]
        [PersistError(typeof(GLTrialBalanceImportDetails.importSubAccountCDError))]
        public virtual String ImportSubAccountCD
        {
            get { return _ImportSubAccountCD; }
            set { _ImportSubAccountCD = value; }
        }
        #endregion

        #region MapSubAccountID
        public abstract class mapSubAccountID : PX.Data.BQL.BqlInt.Field<mapSubAccountID> { }

        protected Int32? _MapSubAccountID;
        [SubAccount(typeof(GLTrialBalanceImportDetails.mapAccountID), DisplayName = "Mapped Subaccount", Enabled = false)]
        public virtual Int32? MapSubAccountID
        {
            get { return _MapSubAccountID; }
            set { _MapSubAccountID = value; }
        }
        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        protected Boolean? _Selected;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual Boolean? Selected
        {
            get { return _Selected; }
            set { _Selected = value; }
        }
        #endregion

        #region Status
        public abstract class status : PX.Data.BQL.BqlInt.Field<status> { }

        protected Int32? _Status;
        [PXDBInt]
        [TrialBalanceImportStatus]
        [PXDefault(TrialBalanceImportStatusAttribute.NEW)]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        public virtual Int32? Status
        {
            get { return _Status; }
            set { _Status = value; }
        }
        #endregion

        #region YtdBalance
        public abstract class ytdBalance : PX.Data.BQL.BqlDecimal.Field<ytdBalance> { }

        protected Decimal? _YtdBalance;

        [CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "YTD Balance")]
        public virtual Decimal? YtdBalance
        {
            get { return _YtdBalance; }
            set { _YtdBalance = value; }
        }
        #endregion

        #region CuryYtdBalance
        public abstract class curyYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyYtdBalance> { }
        protected Decimal? _CuryYtdBalance;
        [CM.PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Currency YTD Balance", Visibility = PXUIVisibility.Visible)]
		//GLTrialBalanceImportDetails.importAccountCD and Ledger.ledgerID fields are used in the formula only for handling their events
		[PXFormula(typeof(IIf<Where<GLTrialBalanceImportDetails.importAccountCD, IsNotNull,
			Or<Current<Ledger.ledgerID>, IsNotNull>>, GLTrialBalanceImportDetails.ytdBalance, GLTrialBalanceImportDetails.ytdBalance>))]
        public virtual Decimal? CuryYtdBalance
        {
            get
            {
                return this._CuryYtdBalance;
            }
            set
            {
                this._CuryYtdBalance = value;
            }
        }
        #endregion

        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXString]
        [PXUIField(DisplayName = "Description", IsReadOnly = true, Enabled = false)]
        [PXFormula(typeof(Selector<GLTrialBalanceImportDetails.mapAccountID, Account.description>))]
        public virtual String Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        #endregion

        #region AccountType
        public abstract class accountType : PX.Data.BQL.BqlString.Field<accountType> { }

        protected String _AccountType;
        [PXString(1)]
        [AccountType.List()]
        [PXUIField(DisplayName = "Type", IsReadOnly = true, Enabled = false)]
        [PXFormula(typeof(Selector<GLTrialBalanceImportDetails.mapAccountID, Account.type>))]
        public virtual String AccountType
        {
            get { return _AccountType; }
            set { _AccountType = value; }
        }
		#endregion

		#region AccountCuryID
		public abstract class accountCuryID : PX.Data.BQL.BqlString.Field<accountCuryID> { }

		[PXString(5, IsUnicode = true)]
		[PXFormula(typeof(Selector<GLTrialBalanceImportDetails.importAccountCD, Account.curyID>))]
		public virtual string AccountCuryID { get; set; }
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
        [PXUIField(Visible = false, Enabled = false)]
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
        [PXUIField(Visible = false, Enabled = false)]
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
        [PXUIField(Visible = false, Enabled = false)]
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
        [PXUIField(Visible = false, Enabled = false)]
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
        [PXUIField(Visible = false, Enabled = false)]
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
        [PXUIField(Visible = false, Enabled = false)]
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
