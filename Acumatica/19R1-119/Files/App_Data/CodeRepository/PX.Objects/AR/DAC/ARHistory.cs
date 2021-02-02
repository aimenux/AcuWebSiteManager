using PX.Objects.GL.Attributes;
using PX.Objects.GL.Formula;

namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.GL;

	/// <summary>
	/// Represents an Accounts Receivable history record, which accumulates a number 
	/// of important year-to-date and period-to-date amounts (such as sales, debit/credit 
	/// adjustments, and gains/losses) in base currency. The history is accumulated separately 
	/// across the following dimensions: branch, GL account, GL subaccount, financial period, 
	/// and customer. History records are created and updated during the document release
	/// process (see <see cref="ARDocumentRelease"/> graph). Various helper projections
	/// over this DAC are used in a number of AR inquiry forms and reports, such as Customer 
	/// Summary (AR401000).
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.ARHistory)]
	public partial class ARHistory : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <summary>
		/// A reference to the <see cref="Branch"/> to which the history belongs.
		/// This field is a key field.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		/// <summary>
		/// A reference to the <see cref="Account"/> to which the history belongs.
		/// This field is a key field.
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		/// <summary>
		/// A reference to the <see cref="Sub"/> to which the history belongs.
		/// This field is a key field.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? SubID
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		/// <summary>
		/// A reference to the <see cref="PX.Objects.GL.Obsolete.FinPeriod"/> to which the history belongs.
		/// This field is a key field.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.GL.Obsolete.FinPeriod.FinPeriodID"/> field.
		/// </value>
		[FinPeriodID(IsKey = true)]
		[PXDefault]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		/// <summary>
		/// A reference to the <see cref="Customer"/> to which the history belongs.
		/// This field is a key field.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Customer.CustomerID"/> field.
		/// </value>
		[Customer(IsKey = true, DisplayName = "Customer ID")]
		[PXDefault]
		public virtual int? CustomerID
		{
			get;
			set;
		}
		#endregion
		#region DetDeleted
		public abstract class detDeleted : PX.Data.BQL.BqlBool.Field<detDeleted> { }
		/// <summary>
		/// A Boolean field that indicates (if set to <c>true</c>) that the documents 
		/// that are related to this record have been deleted (archived).
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? DetDeleted
		{
			get;
			set;
		}
		#endregion

		#region FinPtdDrAdjustments
		public abstract class finPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdDrAdjustments> { }
		/// <summary>
		/// The period-to-date amount of debit adjustments for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignDrMemos"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdDrAdjustments
		{
			get;
			set;
		}
		#endregion
		#region FinPtdCrAdjustments
		public abstract class finPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdCrAdjustments> { }
		/// <summary>
		/// The period-to-date amount of credit adjustments for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignCrMemos"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdCrAdjustments
		{
			get;
			set;
		}
		#endregion
		#region FinPtdSales
		public abstract class finPtdSales : PX.Data.BQL.BqlDecimal.Field<finPtdSales> { }
		/// <summary>
		/// The period-to-date amount of sales for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignSales"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdSales
		{
			get;
			set;
		}
		#endregion
		#region FinPtdPayments
		public abstract class finPtdPayments : PX.Data.BQL.BqlDecimal.Field<finPtdPayments> { }
		/// <summary>
		/// The period-to-date amount of payments for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignPayments"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdPayments
		{
			get;
			set;
		}
		#endregion
		#region FinPtdDiscounts
		public abstract class finPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<finPtdDiscounts> { }
		/// <summary>
		/// The period-to-date amount of discounts for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignDiscTaken"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdDiscounts
		{
			get;
			set;
		}
		#endregion
		#region FinYtdBalance
		public abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
		/// <summary>
		/// The year-to-date balance of documents for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignPtd"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinYtdBalance
		{
			get;
			set;
		}
		#endregion
		#region FinBegBalance
		public abstract class finBegBalance : PX.Data.BQL.BqlDecimal.Field<finBegBalance> { }
		/// <summary>
		/// The beginning balance of documents for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinBegBalance
		{
			get;
			set;
		}
		#endregion
		#region FinPtdCOGS
		public abstract class finPtdCOGS : PX.Data.BQL.BqlDecimal.Field<finPtdCOGS> { }
		/// <summary>
		/// The period-to-date cost of goods that are sold during the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The value is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdCOGS
		{
			get;
			set;
		}
		#endregion
		#region FinPtdRGOL
		public abstract class finPtdRGOL : PX.Data.BQL.BqlDecimal.Field<finPtdRGOL> { }
		/// <summary>
		/// The period-to-date amount of realized gains or losses for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignRGOL"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdRGOL
		{
			get;
			set;
		}
		#endregion
		#region FinPtdFinCharges
		public abstract class finPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<finPtdFinCharges> { }
		/// <summary>
		/// The period-to-date amount of financial charges for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignFinCharges"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdFinCharges
		{
			get;
			set;
		}
		#endregion
		#region FinPtdDeposits
		public abstract class finPtdDeposits : PX.Data.BQL.BqlDecimal.Field<finPtdDeposits> { }
		/// <summary>
		/// The period-to-date amount of deposits for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignDeposits"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdDeposits
		{
			get;
			set;
		}
		#endregion
		#region FinYtdDeposits
		public abstract class finYtdDeposits : PX.Data.BQL.BqlDecimal.Field<finYtdDeposits> { }
		/// <summary>
		/// The year-to-date amount of deposits for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignDeposits"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinYtdDeposits
		{
			get;
			set;
		}
		#endregion
		#region FinPtdItemDiscounts
		public abstract class finPtdItemDiscounts : PX.Data.BQL.BqlDecimal.Field<finPtdItemDiscounts> { }
		/// <summary>
		/// The period-to-date amount of item discounts for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignPtdItemDiscounts"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdItemDiscounts
		{
			get;
			set;
		}
		#endregion
		#region FinPtdRevalued
		public abstract class finPtdRevalued : PX.Data.BQL.BqlDecimal.Field<finPtdRevalued> { }
		/// <summary>
		/// The period-to-date amount of item discounts for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.FinPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdRevalued
		{
			get;
			set;
		}
		#endregion

		#region TranPtdDrAdjustments
		public abstract class tranPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdDrAdjustments> { }
		/// <summary>
		/// The period-to-date amount of debit adjustments for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignDrMemos"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdDrAdjustments
		{
			get;
			set;
		}
		#endregion
		#region TranPtdCrAdjustments
		public abstract class tranPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdCrAdjustments> { }
		/// <summary>
		/// The period-to-date amount of credit adjustments for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignCrMemos"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdCrAdjustments
		{
			get;
			set;
		}
		#endregion
		#region TranPtdSales
		public abstract class tranPtdSales : PX.Data.BQL.BqlDecimal.Field<tranPtdSales> { }
		/// <summary>
		/// The period-to-date amount of sales for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignSales"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdSales
		{
			get;
			set;
		}
		#endregion
		#region TranPtdPayments
		public abstract class tranPtdPayments : PX.Data.BQL.BqlDecimal.Field<tranPtdPayments> { }
		/// <summary>
		/// The period-to-date amount of payments for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignPayments"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdPayments
		{
			get;
			set;
		}
		#endregion
		#region TranPtdDiscounts
		public abstract class tranPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<tranPtdDiscounts> { }
		/// <summary>
		/// The period-to-date amount of discounts for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignDiscTaken"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdDiscounts
		{
			get;
			set;
		}
		#endregion
		#region TranYtdBalance
		public abstract class tranYtdBalance : PX.Data.BQL.BqlDecimal.Field<tranYtdBalance> { }
		/// <summary>
		/// The year-to-date balance of documents for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignPtd"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranYtdBalance
		{
			get;
			set;
		}
		#endregion
		#region TranBegBalance
		public abstract class tranBegBalance : PX.Data.BQL.BqlDecimal.Field<tranBegBalance> { }
		/// <summary>
		/// The beginning balance of documents for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranBegBalance
		{
			get;
			set;
		}
		#endregion
		#region TranPtdCOGS
		public abstract class tranPtdCOGS : PX.Data.BQL.BqlDecimal.Field<tranPtdCOGS> { }
		/// <summary>
		/// The period-to-date cost of goods that are sold during the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The value is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdCOGS
		{
			get;
			set;
		}
		#endregion
		#region TranPtdRGOL
		public abstract class tranPtdRGOL : PX.Data.BQL.BqlDecimal.Field<tranPtdRGOL> { }
		/// <summary>
		/// The period-to-date amount of realized gains or losses for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignRGOL"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdRGOL
		{
			get;
			set;
		}
		#endregion
		#region TranPtdFinCharges
		public abstract class tranPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<tranPtdFinCharges> { }
		/// <summary>
		/// The period-to-date amount of financial charges for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignFinCharges"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdFinCharges
		{
			get;
			set;
		}
		#endregion
		#region TranPtdDeposits
		public abstract class tranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranPtdDeposits> { }
		/// <summary>
		/// The period-to-date amount of deposits for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignDeposits"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdDeposits
		{
			get;
			set;
		}
		#endregion
		#region TranYtdDeposits
		public abstract class tranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranYtdDeposits> { }
		/// <summary>
		/// The year-to-date amount of deposits for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignDeposits"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranYtdDeposits
		{
			get;
			set;
		}
		#endregion
		#region TranPtdItemDiscounts
		public abstract class tranPtdItemDiscounts : PX.Data.BQL.BqlDecimal.Field<tranPtdItemDiscounts> { }
		/// <summary>
		/// The period-to-date amount of item discounts for the <see cref="FinPeriodID"/> period 
		/// (which is related to the <see cref="ARRegister.TranPeriodID"/> field).
		/// The amount is specified in the <see cref="Company.BaseCuryID"> base currency of the company</see>.
		/// The sign of the amount is taken from <see cref = "ARReleaseProcess.ARHistBucket.SignPtdItemDiscounts"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdItemDiscounts
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion

		#region FinFlag
		public abstract class finFlag : PX.Data.BQL.BqlBool.Field<finFlag> { }
		/// <summary>
		/// When <c>true</c>, indicates that the fields with 'Fin' prefix should be used.
		/// When <c>false</c>, indicates that the fields with 'Tran' prefix should be used.
		/// </summary>
		[PXBool]
		public virtual bool? FinFlag
		{
			get;
			set;
		} = true;
		#endregion
		#region NumberInvoicePaid
		public abstract class numberInvoicePaid : PX.Data.BQL.BqlInt.Field<numberInvoicePaid> { }
		/// <summary>
		/// The field is not used.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? NumberInvoicePaid
		{
			get;
			set;
		}
		#endregion
		#region PaidInvoiceDays
		public abstract class paidInvoiceDays : PX.Data.BQL.BqlShort.Field<paidInvoiceDays> { }
		/// <summary>
		/// The field is not used.
		/// </summary>
		[PXDBShort]
		[PXDefault((short)0)]
		public virtual short? PaidInvoiceDays
		{
			get;
			set;
		}
		#endregion

		#region PtdCrAdjustments
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinPtdCrAdjustments"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranPtdCrAdjustments" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? PtdCrAdjustments
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdCrAdjustments), typeof(tranPtdCrAdjustments))]
			get
			{
				return FinFlag == true ? FinPtdCrAdjustments : TranPtdCrAdjustments;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdCrAdjustments = value;
				}
				else
				{
					TranPtdCrAdjustments = value;
				}
			}
		}
		#endregion
		#region PtdDrAdjustments
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinPtdDrAdjustments"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranPtdDrAdjustments" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? PtdDrAdjustments
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdDrAdjustments), typeof(tranPtdDrAdjustments))]
			get
			{
				return FinFlag == true ? FinPtdDrAdjustments : TranPtdDrAdjustments;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdDrAdjustments = value;
				}
				else
				{
					TranPtdDrAdjustments = value;
				}
			}
		}
		#endregion
		#region PtdSales
		[PXDecimal(4)]
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinPtdSales"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranPtdSales" /> field.
		/// </summary>
		public virtual decimal? PtdSales
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdSales), typeof(tranPtdSales))]
			get
			{
				return FinFlag == true ? FinPtdSales : TranPtdSales;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdSales = value;
				}
				else
				{
					TranPtdSales = value;
				}
			}
		}
		#endregion
		#region PtdPayments
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinPtdPayments"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranPtdPayments" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? PtdPayments
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdPayments), typeof(tranPtdPayments))]
			get
			{
				return (FinFlag == true) ? FinPtdPayments : TranPtdPayments;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdPayments = value;
				}
				else
				{
					TranPtdPayments = value;
				}
			}
		}
		#endregion
		#region PtdDiscounts
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinPtdDiscounts"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranPtdDiscounts" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? PtdDiscounts
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdDiscounts), typeof(tranPtdDiscounts))]
			get
			{
				return FinFlag == true ? FinPtdDiscounts : TranPtdDiscounts;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdDiscounts = value;
				}
				else
				{
					TranPtdDiscounts = value;
				}
			}
		}
		#endregion
		#region YtdBalance
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinYtdBalance"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranYtdBalance" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? YtdBalance
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finYtdBalance), typeof(tranYtdBalance))]
			get
			{
				return (FinFlag == true) ? FinYtdBalance : TranYtdBalance;
			}
			set
			{
				if (FinFlag == true)
				{
					FinYtdBalance = value;
				}
				else
				{
					TranYtdBalance = value;
				}
			}
		}
		#endregion
		#region BegBalance
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinBegBalance"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranBegBalance" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? BegBalance
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finBegBalance), typeof(tranBegBalance))]
			get
			{
				return FinFlag == true ? FinBegBalance : TranBegBalance;
			}
			set
			{
				if (FinFlag == true)
				{
					FinBegBalance = value;
				}
				else
				{
					TranBegBalance = value;
				}
			}
		}
		#endregion
		#region PtdCOGS
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinPtdCOGS"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranPtdCOGS" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? PtdCOGS
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdCOGS), typeof(tranPtdCOGS))]
			get
			{
				return FinFlag == true ? FinPtdCOGS : TranPtdCOGS;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdCOGS = value;
				}
				else
				{
					TranPtdCOGS = value;
				}
			}
		}
		#endregion
		#region PtdRGOL
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinPtdRGOL"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranPtdRGOL" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? PtdRGOL
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdRGOL), typeof(tranPtdRGOL))]
			get
			{
				return FinFlag == true ? FinPtdRGOL : TranPtdRGOL;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdRGOL = value;
				}
				else
				{
					TranPtdRGOL = value;
				}
			}
		}
		#endregion
		#region PtdFinCharges
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinPtdFinCharges"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranPtdFinCharges" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? PtdFinCharges
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdFinCharges), typeof(tranPtdFinCharges))]
			get
			{
				return (FinFlag == true) ? FinPtdFinCharges : TranPtdFinCharges;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdFinCharges = value;
				}
				else
				{
					TranPtdFinCharges = value;
				}
			}
		}
		#endregion
		#region PtdDeposits
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinPtdDeposits"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranPtdDeposits" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? PtdDeposits
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdDeposits), typeof(tranPtdDeposits))]
			get
			{
				return (FinFlag == true) ? FinPtdDeposits : TranPtdDeposits;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdDeposits = value;
				}
				else
				{
					TranPtdDeposits = value;
				}
			}
		}
		#endregion
		#region YtdDeposits
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinYtdDeposits"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranYtdDeposits" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? YtdDeposits
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finYtdDeposits), typeof(tranYtdDeposits))]
			get
			{
				return FinFlag == true ? FinYtdDeposits : TranYtdDeposits;
			}
			set
			{
				if (FinFlag == true)
				{
					FinYtdDeposits = value;
				}
				else
				{
					TranYtdDeposits = value;
				}
			}
		}
		#endregion
		#region PtdItemDiscounts
		/// <summary>
		/// If <see cref="FinFlag"/> is <c>true</c>, represents the <see cref="FinPtdItemDiscounts"/> field.
		/// If <see cref="FinFlag"/> is <c>false</c>, represents the <see cref= "TranPtdItemDiscounts" /> field.
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? PtdItemDiscounts
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdItemDiscounts), typeof(tranPtdItemDiscounts))]
			get
			{
				return FinFlag == true ? FinPtdItemDiscounts : TranPtdItemDiscounts;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdItemDiscounts = value;
				}
				else
				{
					TranPtdItemDiscounts = value;
				}
			}
		}
		#endregion

		#region FinPtdRetainageWithheld
		public abstract class finPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageWithheld> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region FinYtdRetainageWithheld
		public abstract class finYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageWithheld> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinYtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region TranPtdRetainageWithheld
		public abstract class tranPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageWithheld> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region TranYtdRetainageWithheld
		public abstract class tranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageWithheld> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranYtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region PtdRetainageWithheld
		[PXDecimal(4)]
		public virtual decimal? PtdRetainageWithheld
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdRetainageWithheld), typeof(tranPtdRetainageWithheld))]
			get
			{
				return FinFlag == true 
					? FinPtdRetainageWithheld 
					: TranPtdRetainageWithheld;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdRetainageWithheld = value;
				}
				else
				{
					TranPtdRetainageWithheld = value;
				}
			}
		}
		#endregion
		#region YtdRetainageWithheld
		[PXDecimal(4)]
		public virtual decimal? YtdRetainageWithheld
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finYtdRetainageWithheld), typeof(tranYtdRetainageWithheld))]
			get
			{
				return FinFlag == true 
					? FinYtdRetainageWithheld 
					: TranYtdRetainageWithheld;
			}
			set
			{
				if (FinFlag == true)
				{
					FinYtdRetainageWithheld = value;
				}
				else
				{
					TranYtdRetainageWithheld = value;
				}
			}
		}
		#endregion
		#region FinPtdRetainageReleased
		public abstract class finPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageReleased> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? FinPtdRetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region FinYtdRetainageReleased
		public abstract class finYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageReleased> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinYtdRetainageReleased
		{
			get;
			set;
		}
		#endregion

		#region TranPtdRetainageReleased
		public abstract class tranPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageReleased> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranPtdRetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region TranYtdRetainageReleased
		public abstract class tranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageReleased> { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranYtdRetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region PtdRetainageReleased
		[PXDecimal(4)]
		public virtual decimal? PtdRetainageReleased
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdRetainageReleased), typeof(tranPtdRetainageReleased))]
			get
			{
				return FinFlag == true 
					? FinPtdRetainageReleased 
					: TranPtdRetainageReleased;
			}
			set
			{
				if (FinFlag == true)
				{
					FinPtdRetainageReleased = value;
				}
				else
				{
					TranPtdRetainageReleased = value;
				}
			}
		}
		#endregion
		#region YtdRetainageReleased
		[PXDecimal(4)]
		public virtual decimal? YtdRetainageReleased
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finYtdRetainageReleased), typeof(tranYtdRetainageReleased))]
			get
			{
				return FinFlag == true 
					? FinYtdRetainageReleased 
					: TranYtdRetainageReleased;
			}
			set
			{
				if (FinFlag == true)
				{
					FinYtdRetainageReleased = value;
				}
				else
				{
					TranYtdRetainageReleased = value;
				}
			}
		}
		#endregion
	}
}
