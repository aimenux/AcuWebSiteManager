using System;
using PX.Data;

namespace PX.Objects.GL
{
	/// <summary>
	/// Stores the amount budgeted for a particular <see cref="GLBudgetLineDetail.FinPeriodID">financial period</see> under a certain article of a budget (see <see cref="GLBudgetLine"/>).
	/// The record is related to the master budget article via the (<see cref="BranchID"/>, <see cref="LedgerID"/>, <see cref="FinYear"/>, <see cref="GroupID"/>) tuple.
	/// The compound key includes all of the above fields and the <see cref="FinPeriodID"/> field.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.GLBudgetLineDetail)]
	public partial class GLBudgetLineDetail : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// The identifier of the <see cref="Branch">branch</see> to which the budget article belongs.
		/// This field is a part of the compound key.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? BranchID { get; set; }
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }

		/// <summary>
		/// The identifier of the <see cref="Ledger">ledger</see> to which the budget article belongs.
		/// This field is a part of the compound key.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Ledger.LedgerID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? LedgerID { get; set; }
		#endregion
		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }

		/// <summary>
		/// The <see cref="FinYear">financial year</see> to which the budget article belongs.
		/// This field is a part of the compound key.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="FinYear.year"/> field.
		/// </value>
		[PXDBString(4, IsKey = true, IsFixed = true)]
		[PXDefault]
		public virtual string FinYear { get; set; }
		#endregion
		#region GroupID
		public abstract class groupID : PX.Data.BQL.BqlGuid.Field<groupID> { }
		protected Guid? _GroupID;

		/// <summary>
		/// The identifier of the budget article.
		/// This field is a part of the compound key.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="GLBudgetLine.GroupID"/> field of the master record.
		/// </value>
		[PXDBGuid(IsKey = true)]
		[PXDefault]
        [PXParent(typeof(Select<GLBudgetLine, Where<GLBudgetLine.groupID, Equal<Current<GLBudgetLineDetail.groupID>>>>))]
		[PXUIField(DisplayName = "GroupID", Visibility = PXUIVisibility.Invisible)]
		public virtual Guid? GroupID
		{
			get
			{
				return this._GroupID;
			}
			set
			{
				this._GroupID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		/// <summary>
		/// The identifier of the GL <see cref="Account">account</see> of the budget article.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// The value of this field is inherited from the <see cref="GLBudgetLine.AccountID"/> field of the master record.
		/// </value>
		[PXDBInt]
		[PXDefault]
		public virtual int? AccountID { get; set; }
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		/// <summary>
		/// The identifier of the GL <see cref="Sub">subaccount</see> of the budget article.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// The value of this field is inherited from the <see cref="GLBudgetLine.SubID"/> field of the master record.
		/// </value>
		[PXDefault]
		[SubAccount]
		public virtual int? SubID { get; set; }
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		/// <summary>
		/// The identifier of the <see cref="PX.Objects.GL.Obsolete.FinPeriod">financial period</see>, which is represented by the record.
		/// This field is a part of the compound key.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.GL.Obsolete.FinPeriod.FinPeriodID"/> field.
		/// </value>
		[PXDBString(6, IsKey = true)]
		[PXDefault]
		public virtual string FinPeriodID { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }

		/// <summary>
		/// The amount allocated to the specified <see cref="FinPeriodID">financial period</see>.
		/// </summary>
		/// <value>
		/// The value is specified by a user or assigned by one of the procedures that distribute budgeted amounts.
		/// The values of these fields of detail lines are summed up to the <see cref="GLBudgetLine.Amount"/> field of the master record.
		/// </value>
		[PXDBDecimal(typeof(Search2<CM.Currency.decimalPlaces,
			InnerJoin<Ledger, On<Ledger.baseCuryID, Equal<CM.Currency.curyID>>>,
			Where<Ledger.ledgerID, Equal<Current<GLBudgetLineDetail.ledgerID>>>>))]
		[PXUIField(DisplayName = "Budget Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<GLBudgetLine.allocatedAmount>))]
		public virtual decimal? Amount { get; set; }
		#endregion
		#region ReleasedAmount
		public abstract class releasedAmount : PX.Data.BQL.BqlDecimal.Field<releasedAmount> { }
		protected Decimal? _ReleasedAmount;

		/// <summary>
		/// The currently released amount for the specified <see cref="FinPeriodID">financial period</see>.
		/// </summary>
		/// <value>
		/// This field is updated with the value of the <see cref="Amount"/> field upon release of the corresponding budget article.
		/// The difference between the value of this field and the <see cref="Amount"/> field shows the difference
		/// between the current state of the article and the corresponding figures in the budget ledger.
		/// </value>
		[PXDBDecimal(typeof(Search2<CM.Currency.decimalPlaces,
			InnerJoin<Ledger, On<Ledger.baseCuryID, Equal<CM.Currency.curyID>>>,
			Where<Ledger.ledgerID, Equal<Current<GLBudgetLineDetail.ledgerID>>>>))]
		[PXUIField(DisplayName = "Released Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ReleasedAmount
		{
			get
			{
				return this._ReleasedAmount;
			}
			set
			{
				this._ReleasedAmount = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
	}
}
