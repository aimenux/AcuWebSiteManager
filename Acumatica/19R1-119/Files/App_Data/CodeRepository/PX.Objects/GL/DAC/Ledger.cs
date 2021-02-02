using System;
using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.GL
{
	public class LedgerBalanceType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Actual, Report, Statistical, Budget },
				new string[] { Messages.Actual, Messages.Report, Messages.Statistical, Messages.Budget }) { ; }
		}

		public const string Actual = "A";
		public const string Report = "R";
		public const string Statistical = "S";
		public const string Budget = "B";

		public class actual : PX.Data.BQL.BqlString.Constant<actual>
		{
			public actual() : base(Actual) { ; }
		}

		public class report : PX.Data.BQL.BqlString.Constant<report>
		{
			public report() : base(Report) { ; }
		}

		public class statistical : PX.Data.BQL.BqlString.Constant<statistical>
		{
			public statistical() : base(Statistical) { ; }
		}

		public class budget : PX.Data.BQL.BqlString.Constant<budget>
		{
			public budget() : base(Budget) { ; }
		}
	}

    /// <summary>
    /// Represents a financial ledger.
    /// The records of this type are added and edited through the Ledger (GL.20.15.00) form
    /// (corresponds to the <see cref="GeneralLedgerMaint"/> graph).
    /// </summary>
	[PXPrimaryGraph(typeof(GeneralLedgerMaint))]
	[System.SerializableAttribute()]
    [PXCacheName(Messages.Ledger)]
	public partial class Ledger : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;

        /// <summary>
        /// Indicates whether the Ledger is selected for processing.
        /// </summary>
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
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
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		protected Int32? _LedgerID;

        /// <summary>
        /// Database identity.
        /// Unique identifier of the Ledger.
        /// </summary>
		[PXDBIdentity()]
		[PXUIField(DisplayName = "Ledger ID", Visibility = PXUIVisibility.Visible, Visible = false )]
		public virtual Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion
		#region LedgerCD
		public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }
		protected string _LedgerCD;

        /// <summary>
        /// Key field.
        /// Unique user-friendly identifier of the Ledger.
        /// </summary>
        [PXSelector(typeof(Search<Ledger.ledgerCD>))]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXDefault]
		[PXUIField(DisplayName = "Ledger ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string LedgerCD
		{
			get
			{
				return this._LedgerCD;
			}
			set
			{
				this._LedgerCD = value;
			}
		}
        #endregion
        #region OrganizationID
        public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

        /// <summary>
        /// Reference to <see cref="Organization"/> record. The field is filled only for ledgers of actual type and is used for data denormalization.
        /// </summary>
        [PXDBInt]
        public virtual int? OrganizationID { get; set; } 
        #endregion
        #region BaseCuryID
        public abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
		protected String _BaseCuryID;

        /// <summary>
        /// Base <see cref="Currency"/> of the Ledger.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Currency.CuryID"/> field.
        /// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
        /// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXDefault(typeof(Company.baseCuryID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String BaseCuryID
		{
			get
			{
				return this._BaseCuryID;
			}
			set
			{
				this._BaseCuryID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

        /// <summary>
        /// The description of the Ledger.
        /// </summary>
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region BalanceType
		public abstract class balanceType : PX.Data.BQL.BqlString.Field<balanceType> { }
		protected String _BalanceType;

        /// <summary>
        /// The type of the balance of the ledger.
        /// </summary>
        /// <value>
        /// Possible values are:
        /// <c>"A"</c> - Actual,
        /// <c>"R"</c> - Reporting,
        /// <c>"S"</c> - Statistical,
        /// <c>"B"</c> - Budget.
        /// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(LedgerBalanceType.Actual)]
		[LedgerBalanceType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible)]
		public virtual String BalanceType
		{
			get
			{
				return this._BalanceType;
			}
			set
			{
				this._BalanceType = value;
			}
		}
		#endregion
		#region DefBranchID
		public abstract class defBranchID : PX.Data.BQL.BqlInt.Field<defBranchID> { }
		protected Int32? _DefBranchID;

		/// <summary>
		/// The identifier of the consolidating <see cref="Branch"/> of the Ledger.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		[PXDBInt]
		public virtual Int32? DefBranchID
		{
			get
			{
				return this._DefBranchID;
			}
			set
			{
				this._DefBranchID = value;
			}
		}
		#endregion
		#region PostInterCompany
		public abstract class postInterCompany : PX.Data.BQL.BqlBool.Field<postInterCompany> { }
		protected Boolean? _PostInterCompany;

		/// <summary>
		/// When set to <c>true</c>, indicates that the system must automatically generate inter-branch transactions
		/// for this Ledger to balance transactions for all branches involved.
		/// This field is relevant only if the <see cref="FeaturesSet.InterBranch">Inter-Branch Transactions</see> feature has been activated
		/// and the <see cref="DefBranchID">Consolidating Branch</see> is specified.
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool()]
		[PXUIField(DisplayName = "Branch Accounting")]
		[PXDefault(true)]
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public virtual Boolean? PostInterCompany
		{
			get
			{
				return this._PostInterCompany;
			}
			set
			{
				this._PostInterCompany = value;
			}
		}
		#endregion
		#region ConsolAllowed
		public abstract class consolAllowed : PX.Data.BQL.BqlBool.Field<consolAllowed> { }
		protected bool? _ConsolAllowed;

        /// <summary>
        /// When set to <c>true</c>, indicates that the system must use the Ledger as a source Ledger for consolidation.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Consolidation Source")]
		public virtual bool? ConsolAllowed
		{
			get
			{
				return this._ConsolAllowed;
			}
			set
			{
				this._ConsolAllowed = value;
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
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
