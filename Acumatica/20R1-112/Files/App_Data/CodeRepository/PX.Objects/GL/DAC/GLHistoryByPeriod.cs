using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	using PX.Objects.GL.FinPeriods.TableDefinition;

	/// <summary>
	/// The DAC used to simplify selection and aggregation of proper <see cref="GLHistory"/> records
	/// on various inquiry and processing screens of the General Ledger module. The main purpose of this DAC is
	/// to close the gaps in GL history records, which appear in case GL history records do not exist for 
	/// every financial period defined in the system. To close these gaps, this projection DAC
	/// calculates the <see cref="LastActivityPeriod">last activity period</see> for every existing
	/// <see cref="PX.Objects.GL.Obsolete.FinPeriod">financial period</see>, so that inquiries and reports that produce information
	/// for a given financial period can look at the latest available <see cref="GLHistory"/> record.
	/// </summary>
	[System.SerializableAttribute()]
	[PXProjection(typeof(Select5<GLHistory,
		LeftJoin<MasterFinPeriod, 
			On<MasterFinPeriod.finPeriodID, GreaterEqual<GLHistory.finPeriodID>>>,
		Aggregate<
			Max<GLHistory.finPeriodID,
			GroupBy<GLHistory.branchID,
			GroupBy<GLHistory.ledgerID,
			GroupBy<GLHistory.accountID,
			GroupBy<GLHistory.subID,
			GroupBy<MasterFinPeriod.finPeriodID
        >>>>>>>>))]
	[GLHistoryPrimaryGraph]
    [PXCacheName(Messages.GLHistoryByPeriod)]
	public partial class GLHistoryByPeriod : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        protected Int32? _BranchID;

        /// <summary>
        /// Identifier of the <see cref="Branch"/>, which the history record belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Branch.BAccountID"/> field.
        /// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.branchID))]
        [PXUIField(DisplayName="Branch")]
		[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD))]
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
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
        protected Int32? _LedgerID;

        /// <summary>
        /// Identifier of the <see cref="Ledger"/>, which the history record belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Ledger.LedgerID"/> field.
        /// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.ledgerID))]
        [PXUIField(DisplayName = "Ledger")]
		[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD))]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;

        /// <summary>
        /// Identifier of the <see cref="Account"/> associated with the history record.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[Account(IsKey = true, BqlField = typeof(GLHistory.accountID))]
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
        /// Identifier of the <see cref="Sub">Subaccount</see> associated with the history record.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[SubAccount(IsKey = true, BqlField = typeof(GLHistory.subID))]
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
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see> of the last activity on the Account and Subaccount associated with the history record,
        /// with regards to Ledger and Branch.
        /// </summary>
		[GL.FinPeriodID(BqlField = typeof(GLHistory.finPeriodID))]
        [PXUIField(DisplayName = "Last Activity Period", Visibility = PXUIVisibility.Invisible)]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see>, for which the history data is given.
        /// </summary>
        [PXUIField(DisplayName = "Financial Period", Visibility = PXUIVisibility.Invisible)]
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
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
		#region FinYear
		public abstract class finYear: PX.Data.IBqlField {}

		[PXString]
		public virtual String FinYear
		{
			get => FinPeriodID?.Substring(0, 4);
			set{}
		}
		#endregion
	}

	[Serializable]
	[PXProjection(typeof(Select5<
		GLHistory,
		InnerJoin<MasterFinPeriod,
			On<MasterFinPeriod.finPeriodID, GreaterEqual<GLHistory.finPeriodID>>,
		InnerJoin<Branch,
			On<GLHistory.branchID, Equal<Branch.branchID>>,
		InnerJoin<OrganizationFinPeriodCurrent,
			On<OrganizationFinPeriodCurrent.finPeriodID, Equal<CurrentValue<GLHistoryFilter.finPeriodID>>,
			And<MasterFinPeriod.finPeriodID, Equal<OrganizationFinPeriodCurrent.prevFinPeriodID>,
			And<Branch.organizationID, Equal<OrganizationFinPeriodCurrent.organizationID>>>>>>>,
		Aggregate <
			Max<GLHistory.finPeriodID,
			GroupBy<GLHistory.branchID,
			GroupBy<GLHistory.ledgerID,
			GroupBy<GLHistory.accountID,
			GroupBy<GLHistory.subID,
			GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>))]
	[GLHistoryPrimaryGraph]
	[PXCacheName(Messages.GLHistoryByPeriod)]
	public partial class GLHistoryByPeriodCurrent : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// Identifier of the <see cref="Branch"/>, which the history record belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.branchID))]
		[PXUIField(DisplayName = "Branch")]
		[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD))]
		public virtual int? BranchID { get; set; }
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }

		/// <summary>
		/// Identifier of the <see cref="Ledger"/>, which the history record belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Ledger.LedgerID"/> field.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.ledgerID))]
		[PXUIField(DisplayName = "Ledger")]
		[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual int? LedgerID { get; set; }
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		/// <summary>
		/// Identifier of the <see cref="Account"/> associated with the history record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(IsKey = true, BqlField = typeof(GLHistory.accountID))]
		public virtual int? AccountID { get; set; }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		/// <summary>
		/// Identifier of the <see cref="Sub">Subaccount</see> associated with the history record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(IsKey = true, BqlField = typeof(GLHistory.subID))]
		public virtual int? SubID { get; set; }
		#endregion
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see> of the last activity on the Account and Subaccount associated with the history record,
		/// with regards to Ledger and Branch.
		/// </summary>
		[GL.FinPeriodID(BqlField = typeof(GLHistory.finPeriodID))]
		[PXUIField(DisplayName = "Last Activity Period", Visibility = PXUIVisibility.Invisible)]
		public virtual string LastActivityPeriod { get; set; }
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see>, for which the history data is given.
		/// </summary>
		[PXUIField(DisplayName = "Financial Period", Visibility = PXUIVisibility.Invisible)]
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
		public virtual string FinPeriodID { get; set; }
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.IBqlField { }

		[PXString]
		public virtual String FinYear
		{
			get => FinPeriodID?.Substring(0, 4);
			set { }
		}
		#endregion
	}

	[Serializable]
	[PXProjection(typeof(Select5<
	GLHistory,
	InnerJoin<MasterFinPeriod,
		On<MasterFinPeriod.finPeriodID, GreaterEqual<GLHistory.finPeriodID>>,
	InnerJoin<OrganizationFinPeriodCurrent,
		On<OrganizationFinPeriodCurrent.finPeriodID, Equal<CurrentValue<GLHistoryFilter.finPeriodID>>,
		And<MasterFinPeriod.finPeriodID, Equal<OrganizationFinPeriodCurrent.prevFinPeriodID>,
		And<OrganizationFinPeriodCurrent.organizationID, Equal<FinPeriod.organizationID.masterValue>>>>>>,
	Aggregate<
		Max<GLHistory.finPeriodID,
		GroupBy<GLHistory.branchID,
		GroupBy<GLHistory.ledgerID,
		GroupBy<GLHistory.accountID,
		GroupBy<GLHistory.subID,
		GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>))]
	[GLHistoryPrimaryGraph]
	[PXCacheName(Messages.GLHistoryByPeriod)]
	public partial class GLHistoryByPeriodMasterCurrent : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <summary>
		/// Identifier of the <see cref="Branch"/>, which the history record belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.branchID))]
		[PXUIField(DisplayName = "Branch")]
		[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD))]
		public virtual int? BranchID { get; set; }
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		/// <summary>
		/// Identifier of the <see cref="Ledger"/>, which the history record belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Ledger.LedgerID"/> field.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.ledgerID))]
		[PXUIField(DisplayName = "Ledger")]
		[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual int? LedgerID { get; set; }
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		/// <summary>
		/// Identifier of the <see cref="Account"/> associated with the history record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(IsKey = true, BqlField = typeof(GLHistory.accountID))]
		public virtual int? AccountID { get; set; }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		/// <summary>
		/// Identifier of the <see cref="Sub">Subaccount</see> associated with the history record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(IsKey = true, BqlField = typeof(GLHistory.subID))]
		public virtual int? SubID { get; set; }
		#endregion
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see> of the last activity on the Account and Subaccount associated with the history record,
		/// with regards to Ledger and Branch.
		/// </summary>
		[GL.FinPeriodID(BqlField = typeof(GLHistory.finPeriodID))]
		[PXUIField(DisplayName = "Last Activity Period", Visibility = PXUIVisibility.Invisible)]
		public virtual string LastActivityPeriod { get; set; }
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see>, for which the history data is given.
		/// </summary>
		[PXUIField(DisplayName = "Financial Period", Visibility = PXUIVisibility.Invisible)]
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
		public virtual string FinPeriodID { get; set; }
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.IBqlField { }

		[PXString]
		public virtual String FinYear
		{
			get => FinPeriodID?.Substring(0, 4);
			set { }
		}
		#endregion
	}

	[Serializable]
	[PXProjection(typeof(Select4<GLHistory,
		Where<GLHistory.finPtdRevalued, NotEqual<decimal0>>,
		Aggregate<
			GroupBy<GLHistory.branchID,
			GroupBy<GLHistory.ledgerID,
			GroupBy<GLHistory.accountID,
			GroupBy<GLHistory.subID,
			Max<GLHistory.finPeriodID>>>>>>>))]
	public partial class GLHistoryLastRevaluation : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;

		/// <summary>
		/// Identifier of the <see cref="Branch"/>, which the history record belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BAccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.branchID))]
		[PXUIField(DisplayName = "Branch")]
		[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD))]
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
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		protected Int32? _LedgerID;

		/// <summary>
		/// Identifier of the <see cref="Ledger"/>, which the history record belongs.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Ledger.LedgerID"/> field.
		/// </value>
		[PXDBInt(IsKey = true, BqlField = typeof(GLHistory.ledgerID))]
		[PXUIField(DisplayName = "Ledger")]
		[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD))]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;

		/// <summary>
		/// Identifier of the <see cref="Account"/> associated with the history record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(IsKey = true, BqlField = typeof(GLHistory.accountID))]
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
		/// Identifier of the <see cref="Sub">Subaccount</see> associated with the history record.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(IsKey = true, BqlField = typeof(GLHistory.subID))]
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
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see> of the last activity on the Account and Subaccount associated with the history record,
		/// with regards to Ledger and Branch.
		/// </summary>
		[GL.FinPeriodID(BqlField = typeof(GLHistory.finPeriodID))]
		[PXUIField(DisplayName = "Last Activity Period", Visibility = PXUIVisibility.Invisible)]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
	}
}
