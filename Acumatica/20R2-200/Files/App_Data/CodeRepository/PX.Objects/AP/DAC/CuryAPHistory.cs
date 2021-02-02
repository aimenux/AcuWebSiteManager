using PX.Objects.GL.Attributes;
using PX.Objects.GL.Formula;

namespace PX.Objects.AP
{
	using System;
	using PX.Data;

	/// <summary>
	/// Represents an Accounts Payable history record, which accumulates a number 
	/// of important year-to-date and period-to-date amounts (such as sales, debit and credit 
	/// adjustments, gains and losses) in foreign currency. The history is accumulated separately 
	/// across the following dimensions: branch, GL account, GL subaccount, financial period, 
	/// vendor, and currency. History records are created and updated during the document 
	/// release process (see <see cref="APDocumentRelease"/> graph). Various helper projections
	/// over this DAC are used in a number of AR inquiry forms and reports, such as Vendor 
	/// Summary (AP401000).
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.CuryAPHistory)]
	public partial class CuryAPHistory : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDBInt(IsKey = true)]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
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
		[PXDBInt(IsKey = true)]
		[PXDefault()]
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
		[GL.FinPeriodID(IsKey=true)]
		[PXDefault()]
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
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, IsKey=true, InputMask = ">LLLLL")]
		[PXDefault()]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region DetDeleted
		public abstract class detDeleted : PX.Data.BQL.BqlBool.Field<detDeleted> { }
		protected Boolean? _DetDeleted;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? DetDeleted
		{
			get
			{
				return this._DetDeleted;
			}
			set
			{
				this._DetDeleted = value;
			}
		}
		#endregion
		#region FinBegBalance
		public abstract class finBegBalance : PX.Data.BQL.BqlDecimal.Field<finBegBalance> { }
		protected Decimal? _FinBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinBegBalance
		{
			get
			{
				return this._FinBegBalance;
			}
			set
			{
				this._FinBegBalance = value;
			}
		}
		#endregion
		#region FinPtdPurchases
		public abstract class finPtdPurchases : PX.Data.BQL.BqlDecimal.Field<finPtdPurchases> { }
		protected Decimal? _FinPtdPurchases;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdPurchases
		{
			get
			{
				return this._FinPtdPurchases;
			}
			set
			{
				this._FinPtdPurchases = value;
			}
		}
		#endregion
		#region FinPtdPayments
		public abstract class finPtdPayments : PX.Data.BQL.BqlDecimal.Field<finPtdPayments> { }
		protected Decimal? _FinPtdPayments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdPayments
		{
			get
			{
				return this._FinPtdPayments;
			}
			set
			{
				this._FinPtdPayments = value;
			}
		}
		#endregion
		#region FinPtdDrAdjustments
		public abstract class finPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdDrAdjustments> { }
		protected Decimal? _FinPtdDrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdDrAdjustments
		{
			get
			{
				return this._FinPtdDrAdjustments;
			}
			set
			{
				this._FinPtdDrAdjustments = value;
			}
		}
		#endregion
		#region FinPtdCrAdjustments
		public abstract class finPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdCrAdjustments> { }
		protected Decimal? _FinPtdCrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdCrAdjustments
		{
			get
			{
				return this._FinPtdCrAdjustments;
			}
			set
			{
				this._FinPtdCrAdjustments = value;
			}
		}
		#endregion
		#region FinPtdDiscTaken
		public abstract class finPtdDiscTaken : PX.Data.BQL.BqlDecimal.Field<finPtdDiscTaken> { }
		protected Decimal? _FinPtdDiscTaken;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdDiscTaken
		{
			get
			{
				return this._FinPtdDiscTaken;
			}
			set
			{
				this._FinPtdDiscTaken = value;
			}
		}
		#endregion
		#region FinPtdWhTax
		public abstract class finPtdWhTax : PX.Data.BQL.BqlDecimal.Field<finPtdWhTax> { }
		protected Decimal? _FinPtdWhTax;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdWhTax
		{
			get
			{
				return this._FinPtdWhTax;
			}
			set
			{
				this._FinPtdWhTax = value;
			}
		}
		#endregion
		#region FinPtdRGOL
		public abstract class finPtdRGOL : PX.Data.BQL.BqlDecimal.Field<finPtdRGOL> { }
		protected Decimal? _FinPtdRGOL;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdRGOL
		{
			get
			{
				return this._FinPtdRGOL;
			}
			set
			{
				this._FinPtdRGOL = value;
			}
		}
		#endregion
		#region FinYtdBalance
		public abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
		protected Decimal? _FinYtdBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinYtdBalance
		{
			get
			{
				return this._FinYtdBalance;
			}
			set
			{
				this._FinYtdBalance = value;
			}
		}
		#endregion
		#region FinPtdDeposits
		public abstract class finPtdDeposits : PX.Data.BQL.BqlDecimal.Field<finPtdDeposits> { }
		protected Decimal? _FinPtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdDeposits
		{
			get
			{
				return this._FinPtdDeposits;
			}
			set
			{
				this._FinPtdDeposits = value;
			}
		}
		#endregion
		#region FinYtdDeposits
		public abstract class finYtdDeposits : PX.Data.BQL.BqlDecimal.Field<finYtdDeposits> { }
		protected Decimal? _FinYtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinYtdDeposits
		{
			get
			{
				return this._FinYtdDeposits;
			}
			set
			{
				this._FinYtdDeposits = value;
			}
		}
		#endregion
		#region FinPtdRevalued
		public abstract class finPtdRevalued : PX.Data.BQL.BqlDecimal.Field<finPtdRevalued> { }
		protected Decimal? _FinPtdRevalued;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdRevalued
		{
			get
			{
				return this._FinPtdRevalued;
			}
			set
			{
				this._FinPtdRevalued = value;
			}
		}
		#endregion
		#region TranBegBalance
		public abstract class tranBegBalance : PX.Data.BQL.BqlDecimal.Field<tranBegBalance> { }
		protected Decimal? _TranBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranBegBalance
		{
			get
			{
				return this._TranBegBalance;
			}
			set
			{
				this._TranBegBalance = value;
			}
		}
		#endregion
		#region TranPtdPurchases
		public abstract class tranPtdPurchases : PX.Data.BQL.BqlDecimal.Field<tranPtdPurchases> { }
		protected Decimal? _TranPtdPurchases;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdPurchases
		{
			get
			{
				return this._TranPtdPurchases;
			}
			set
			{
				this._TranPtdPurchases = value;
			}
		}
		#endregion
		#region TranPtdPayments
		public abstract class tranPtdPayments : PX.Data.BQL.BqlDecimal.Field<tranPtdPayments> { }
		protected Decimal? _TranPtdPayments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdPayments
		{
			get
			{
				return this._TranPtdPayments;
			}
			set
			{
				this._TranPtdPayments = value;
			}
		}
		#endregion
		#region TranPtdDrAdjustments
		public abstract class tranPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdDrAdjustments> { }
		protected Decimal? _TranPtdDrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdDrAdjustments
		{
			get
			{
				return this._TranPtdDrAdjustments;
			}
			set
			{
				this._TranPtdDrAdjustments = value;
			}
		}
		#endregion
		#region TranPtdCrAdjustments
		public abstract class tranPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdCrAdjustments> { }
		protected Decimal? _TranPtdCrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdCrAdjustments
		{
			get
			{
				return this._TranPtdCrAdjustments;
			}
			set
			{
				this._TranPtdCrAdjustments = value;
			}
		}
		#endregion
		#region TranPtdDiscTaken
		public abstract class tranPtdDiscTaken : PX.Data.BQL.BqlDecimal.Field<tranPtdDiscTaken> { }
		protected Decimal? _TranPtdDiscTaken;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdDiscTaken
		{
			get
			{
				return this._TranPtdDiscTaken;
			}
			set
			{
				this._TranPtdDiscTaken = value;
			}
		}
		#endregion
		#region TranPtdWhTax
		public abstract class tranPtdWhTax : PX.Data.BQL.BqlDecimal.Field<tranPtdWhTax> { }
		protected Decimal? _TranPtdWhTax;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdWhTax
		{
			get
			{
				return this._TranPtdWhTax;
			}
			set
			{
				this._TranPtdWhTax = value;
			}
		}
		#endregion
		#region TranPtdRGOL
		public abstract class tranPtdRGOL : PX.Data.BQL.BqlDecimal.Field<tranPtdRGOL> { }
		protected Decimal? _TranPtdRGOL;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdRGOL
		{
			get
			{
				return this._TranPtdRGOL;
			}
			set
			{
				this._TranPtdRGOL = value;
			}
		}
		#endregion
		#region TranYtdBalance
		public abstract class tranYtdBalance : PX.Data.BQL.BqlDecimal.Field<tranYtdBalance> { }
		protected Decimal? _TranYtdBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYtdBalance
		{
			get
			{
				return this._TranYtdBalance;
			}
			set
			{
				this._TranYtdBalance = value;
			}
		}
		#endregion
		#region TranPtdDeposits
		public abstract class tranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranPtdDeposits> { }
		protected Decimal? _TranPtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdDeposits
		{
			get
			{
				return this._TranPtdDeposits;
			}
			set
			{
				this._TranPtdDeposits = value;
			}
		}
		#endregion
		#region TranYtdDeposits
		public abstract class tranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranYtdDeposits> { }
		protected Decimal? _TranYtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYtdDeposits
		{
			get
			{
				return this._TranYtdDeposits;
			}
			set
			{
				this._TranYtdDeposits = value;
			}
		}
		#endregion
		#region CuryFinBegBalance
		public abstract class curyFinBegBalance : PX.Data.BQL.BqlDecimal.Field<curyFinBegBalance> { }
		protected Decimal? _CuryFinBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinBegBalance
		{
			get
			{
				return this._CuryFinBegBalance;
			}
			set
			{
				this._CuryFinBegBalance = value;
			}
		}
		#endregion
		#region CuryFinPtdPurchases
		public abstract class curyFinPtdPurchases : PX.Data.BQL.BqlDecimal.Field<curyFinPtdPurchases> { }
		protected Decimal? _CuryFinPtdPurchases;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdPurchases
		{
			get
			{
				return this._CuryFinPtdPurchases;
			}
			set
			{
				this._CuryFinPtdPurchases = value;
			}
		}
		#endregion
		#region CuryFinPtdPayments
		public abstract class curyFinPtdPayments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdPayments> { }
		protected Decimal? _CuryFinPtdPayments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdPayments
		{
			get
			{
				return this._CuryFinPtdPayments;
			}
			set
			{
				this._CuryFinPtdPayments = value;
			}
		}
		#endregion
		#region CuryFinPtdDrAdjustments
		public abstract class curyFinPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDrAdjustments> { }
		protected Decimal? _CuryFinPtdDrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdDrAdjustments
		{
			get
			{
				return this._CuryFinPtdDrAdjustments;
			}
			set
			{
				this._CuryFinPtdDrAdjustments = value;
			}
		}
		#endregion
		#region CuryFinPtdCrAdjustments
		public abstract class curyFinPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdCrAdjustments> { }
		protected Decimal? _CuryFinPtdCrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdCrAdjustments
		{
			get
			{
				return this._CuryFinPtdCrAdjustments;
			}
			set
			{
				this._CuryFinPtdCrAdjustments = value;
			}
		}
		#endregion
		#region CuryFinPtdDiscTaken
		public abstract class curyFinPtdDiscTaken : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDiscTaken> { }
		protected Decimal? _CuryFinPtdDiscTaken;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdDiscTaken
		{
			get
			{
				return this._CuryFinPtdDiscTaken;
			}
			set
			{
				this._CuryFinPtdDiscTaken = value;
			}
		}
		#endregion
		#region CuryFinPtdWhTax
		public abstract class curyFinPtdWhTax : PX.Data.BQL.BqlDecimal.Field<curyFinPtdWhTax> { }
		protected Decimal? _CuryFinPtdWhTax;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdWhTax
		{
			get
			{
				return this._CuryFinPtdWhTax;
			}
			set
			{
				this._CuryFinPtdWhTax = value;
			}
		}
		#endregion
		#region CuryFinYtdBalance
		public abstract class curyFinYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdBalance> { }
		protected Decimal? _CuryFinYtdBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinYtdBalance
		{
			get
			{
				return this._CuryFinYtdBalance;
			}
			set
			{
				this._CuryFinYtdBalance = value;
			}
		}
		#endregion
		#region CuryFinPtdDeposits
		public abstract class curyFinPtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDeposits> { }
		protected Decimal? _CuryFinPtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdDeposits
		{
			get
			{
				return this._CuryFinPtdDeposits;
			}
			set
			{
				this._CuryFinPtdDeposits = value;
			}
		}
		#endregion
		#region CuryFinYtdDeposits
		public abstract class curyFinYtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyFinYtdDeposits> { }
		protected Decimal? _CuryFinYtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinYtdDeposits
		{
			get
			{
				return this._CuryFinYtdDeposits;
			}
			set
			{
				this._CuryFinYtdDeposits = value;
			}
		}
		#endregion
		#region CuryTranBegBalance
		public abstract class curyTranBegBalance : PX.Data.BQL.BqlDecimal.Field<curyTranBegBalance> { }
		protected Decimal? _CuryTranBegBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranBegBalance
		{
			get
			{
				return this._CuryTranBegBalance;
			}
			set
			{
				this._CuryTranBegBalance = value;
			}
		}
		#endregion
		#region CuryTranPtdPurchases
		public abstract class curyTranPtdPurchases : PX.Data.BQL.BqlDecimal.Field<curyTranPtdPurchases> { }
		protected Decimal? _CuryTranPtdPurchases;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdPurchases
		{
			get
			{
				return this._CuryTranPtdPurchases;
			}
			set
			{
				this._CuryTranPtdPurchases = value;
			}
		}
		#endregion
		#region CuryTranPtdPayments
		public abstract class curyTranPtdPayments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdPayments> { }
		protected Decimal? _CuryTranPtdPayments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdPayments
		{
			get
			{
				return this._CuryTranPtdPayments;
			}
			set
			{
				this._CuryTranPtdPayments = value;
			}
		}
		#endregion
		#region CuryTranPtdDrAdjustments
		public abstract class curyTranPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDrAdjustments> { }
		protected Decimal? _CuryTranPtdDrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdDrAdjustments
		{
			get
			{
				return this._CuryTranPtdDrAdjustments;
			}
			set
			{
				this._CuryTranPtdDrAdjustments = value;
			}
		}
		#endregion
		#region CuryTranPtdCrAdjustments
		public abstract class curyTranPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdCrAdjustments> { }
		protected Decimal? _CuryTranPtdCrAdjustments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdCrAdjustments
		{
			get
			{
				return this._CuryTranPtdCrAdjustments;
			}
			set
			{
				this._CuryTranPtdCrAdjustments = value;
			}
		}
		#endregion
		#region CuryTranPtdDiscTaken
		public abstract class curyTranPtdDiscTaken : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDiscTaken> { }
		protected Decimal? _CuryTranPtdDiscTaken;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdDiscTaken
		{
			get
			{
				return this._CuryTranPtdDiscTaken;
			}
			set
			{
				this._CuryTranPtdDiscTaken = value;
			}
		}
		#endregion
		#region CuryTranPtdWhTax
		public abstract class curyTranPtdWhTax : PX.Data.BQL.BqlDecimal.Field<curyTranPtdWhTax> { }
		protected Decimal? _CuryTranPtdWhTax;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdWhTax
		{
			get
			{
				return this._CuryTranPtdWhTax;
			}
			set
			{
				this._CuryTranPtdWhTax = value;
			}
		}
		#endregion
		#region CuryTranYtdBalance
		public abstract class curyTranYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyTranYtdBalance> { }
		protected Decimal? _CuryTranYtdBalance;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranYtdBalance
		{
			get
			{
				return this._CuryTranYtdBalance;
			}
			set
			{
				this._CuryTranYtdBalance = value;
			}
		}
		#endregion
		#region CuryTranPtdDeposits
		public abstract class curyTranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDeposits> { }
		protected Decimal? _CuryTranPtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdDeposits
		{
			get
			{
				return this._CuryTranPtdDeposits;
			}
			set
			{
				this._CuryTranPtdDeposits = value;
			}
		}
		#endregion
		#region CuryTranYtdDeposits
		public abstract class curyTranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyTranYtdDeposits> { }
		protected Decimal? _CuryTranYtdDeposits;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranYtdDeposits
		{
			get
			{
				return this._CuryTranYtdDeposits;
			}
			set
			{
				this._CuryTranYtdDeposits = value;
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
		#region FinFlag
		public abstract class finFlag : PX.Data.BQL.BqlBool.Field<finFlag> { }
        protected Boolean? _FinFlag = true;
		[PXBool()]
        public virtual Boolean? FinFlag
		{
			get
			{
				return this._FinFlag;
			}
			set
			{
				this._FinFlag = value;
			}
		}
		#endregion
		#region PtdCrAdjustments
		[PXDecimal(4)]
		public virtual Decimal? PtdCrAdjustments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdCrAdjustments),typeof(tranPtdCrAdjustments))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdCrAdjustments : this._TranPtdCrAdjustments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdCrAdjustments = value;
				}
				else
				{
					this._TranPtdCrAdjustments = value;
				}
			}
		}
		#endregion
		#region PtdDrAdjustments
		[PXDecimal(4)]
		public virtual Decimal? PtdDrAdjustments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdDrAdjustments),typeof(tranPtdDrAdjustments))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdDrAdjustments : this._TranPtdDrAdjustments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdDrAdjustments = value;
				}
				else
				{
					this._TranPtdDrAdjustments = value;
				}
			}
		}
		#endregion
		#region PtdPurchases
		[PXDecimal(4)]
		public virtual Decimal? PtdPurchases
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdPurchases),typeof(tranPtdPurchases))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdPurchases : this._TranPtdPurchases;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdPurchases = value;
				}
				else
				{
					this._TranPtdPurchases = value;
				}
			}
		}
		#endregion
		#region PtdPayments
		[PXDecimal(4)]
		public virtual Decimal? PtdPayments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdPayments),typeof(tranPtdPayments))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdPayments : this._TranPtdPayments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdPayments = value;
				}
				else
				{
					this._TranPtdPayments = value;
				}
			}
		}
		#endregion
		#region PtdDiscTaken
		[PXDecimal(4)]
		public virtual Decimal? PtdDiscTaken
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdDiscTaken),typeof(tranPtdDiscTaken))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdDiscTaken : this._TranPtdDiscTaken;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdDiscTaken = value;
				}
				else
				{
					this._TranPtdDiscTaken = value;
				}
			}
		}
		#endregion
		#region PtdWhTax
		[PXDecimal(4)]
		public virtual Decimal? PtdWhTax
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdWhTax),typeof(tranPtdWhTax))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdWhTax : this._TranPtdWhTax;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdWhTax = value;
				}
				else
				{
					this._TranPtdWhTax = value;
				}
			}
		}
		#endregion
		#region PtdRGOL
		[PXDecimal(4)]
		public virtual Decimal? PtdRGOL
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdRGOL),typeof(tranPtdRGOL))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdRGOL : this._TranPtdRGOL;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdRGOL = value;
				}
				else
				{
					this._TranPtdRGOL = value;
				}
			}
		}
		#endregion
		#region YtdBalance
		[PXDecimal(4)]
		public virtual Decimal? YtdBalance
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finYtdBalance),typeof(tranYtdBalance))]
			get
			{
				return ((bool)_FinFlag) ? this._FinYtdBalance : this._TranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinYtdBalance = value;
				}
				else
				{
					this._TranYtdBalance = value;
				}
			}
		}
		#endregion
		#region BegBalance
		[PXDecimal(4)]
		public virtual Decimal? BegBalance
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finBegBalance),typeof(tranBegBalance))]
			get
			{
				return ((bool)_FinFlag) ? this._FinBegBalance : this._TranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinBegBalance = value;
				}
				else
				{
					this._TranBegBalance = value;
				}
			}
		}
		#endregion
		#region PtdDeposits
		[PXDecimal(4)]
		public virtual Decimal? PtdDeposits
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finPtdDeposits),typeof(tranPtdDeposits))]
			get
			{
				return ((bool)_FinFlag) ? this._FinPtdDeposits : this._TranPtdDeposits;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinPtdDeposits = value;
				}
				else
				{
					this._TranPtdDeposits = value;
				}
			}
		}
		#endregion
		#region YtdDeposits
		[PXDecimal(4)]
		public virtual Decimal? YtdDeposits
		{
			[PXDependsOnFields(typeof(finFlag),typeof(finYtdDeposits),typeof(tranYtdDeposits))]
			get
			{
				return ((bool)_FinFlag) ? this._FinYtdDeposits : this._TranYtdDeposits;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._FinYtdDeposits = value;
				}
				else
				{
					this._TranYtdDeposits = value;
				}
			}
		}
		#endregion
		#region CuryPtdCrAdjustments
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdCrAdjustments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdCrAdjustments),typeof(curyTranPtdCrAdjustments))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdCrAdjustments : this._CuryTranPtdCrAdjustments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdCrAdjustments = value;
				}
				else
				{
					this._CuryTranPtdCrAdjustments = value;
				}
			}
		}
		#endregion
		#region CuryPtdDrAdjustments
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdDrAdjustments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdDrAdjustments),typeof(curyTranPtdDrAdjustments))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdDrAdjustments : this._CuryTranPtdDrAdjustments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdDrAdjustments = value;
				}
				else
				{
					this._CuryTranPtdDrAdjustments = value;
				}
			}
		}
		#endregion
		#region CuryPtdPurchases
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdPurchases
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdPurchases),typeof(curyTranPtdPurchases))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdPurchases : this._CuryTranPtdPurchases;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdPurchases = value;
				}
				else
				{
					this._CuryTranPtdPurchases = value;
				}
			}
		}
		#endregion
		#region CuryPtdPayments
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdPayments
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdPayments),typeof(curyTranPtdPayments))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdPayments : this._CuryTranPtdPayments;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdPayments = value;
				}
				else
				{
					this._CuryTranPtdPayments = value;
				}
			}
		}
		#endregion
		#region CuryPtdDiscTaken
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdDiscTaken
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdDiscTaken),typeof(curyTranPtdDiscTaken))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdDiscTaken : this._CuryTranPtdDiscTaken;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdDiscTaken = value;
				}
				else
				{
					this._CuryTranPtdDiscTaken = value;
				}
			}
		}
		#endregion
		#region CuryPtdWhTax
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdWhTax
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdWhTax),typeof(curyTranPtdWhTax))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdWhTax : this._CuryTranPtdWhTax;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdWhTax = value;
				}
				else
				{
					this._CuryTranPtdWhTax = value;
				}
			}
		}
		#endregion
		#region CuryYtdBalance
		[PXDecimal(4)]
		public virtual Decimal? CuryYtdBalance
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinYtdBalance),typeof(curyTranYtdBalance))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinYtdBalance : this._CuryTranYtdBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinYtdBalance = value;
				}
				else
				{
					this._CuryTranYtdBalance = value;
				}
			}
		}
		#endregion
		#region CuryBegBalance
		[PXDecimal(4)]
		public virtual Decimal? CuryBegBalance
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinBegBalance),typeof(curyTranBegBalance))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinBegBalance : this._CuryTranBegBalance;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinBegBalance = value;
				}
				else
				{
					this._CuryTranBegBalance = value;
				}
			}
		}
		#endregion
		#region CuryPtdDeposits
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdDeposits
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinPtdDeposits),typeof(curyTranPtdDeposits))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinPtdDeposits : this._CuryTranPtdDeposits;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinPtdDeposits = value;
				}
				else
				{
					this._CuryTranPtdDeposits = value;
				}
			}
		}
		#endregion
		#region CuryYtdDeposits
		[PXDecimal(4)]
		public virtual Decimal? CuryYtdDeposits
		{
			[PXDependsOnFields(typeof(finFlag),typeof(curyFinYtdDeposits),typeof(curyTranYtdDeposits))]
			get
			{
				return ((bool)_FinFlag) ? this._CuryFinYtdDeposits : this._CuryTranYtdDeposits;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this._CuryFinYtdDeposits = value;
				}
				else
				{
					this._CuryTranYtdDeposits = value;
				}
			}
		}
		#endregion
		#region FinPtdRetainageWithheld
		public abstract class finPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageWithheld> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region FinYtdRetainageWithheld
		public abstract class finYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageWithheld> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinYtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region TranPtdRetainageWithheld
		public abstract class tranPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageWithheld> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranPtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region TranYtdRetainageWithheld
		public abstract class tranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageWithheld> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region PtdRetainageWithheld
		[PXDecimal(4)]
		public virtual Decimal? PtdRetainageWithheld
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdRetainageWithheld), typeof(tranPtdRetainageWithheld))]
			get
			{
				return ((bool)_FinFlag) ? this.FinPtdRetainageWithheld : this.TranPtdRetainageWithheld;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this.FinPtdRetainageWithheld = value;
				}
				else
				{
					this.TranPtdRetainageWithheld = value;
				}
			}
		}
		#endregion
		#region YtdRetainageWithheld
		[PXDecimal(4)]
		public virtual Decimal? YtdRetainageWithheld
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finYtdRetainageWithheld), typeof(tranYtdRetainageWithheld))]
			get
			{
				return ((bool)_FinFlag) ? this.FinYtdRetainageWithheld : this.TranYtdRetainageWithheld;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this.FinYtdRetainageWithheld = value;
				}
				else
				{
					this.TranYtdRetainageWithheld = value;
				}
			}
		}
		#endregion

		#region CuryFinPtdRetainageWithheld
		public abstract class curyFinPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyFinPtdRetainageWithheld> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region CuryFinYtdRetainageWithheld
		public abstract class curyFinYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainageWithheld> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinYtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region CuryTranPtdRetainageWithheld
		public abstract class curyTranPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyTranPtdRetainageWithheld> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region CuryTranYtdRetainageWithheld
		public abstract class curyTranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainageWithheld> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranYtdRetainageWithheld
		{
			get;
			set;
		}
		#endregion
		#region CuryPtdRetainageWithheld
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdRetainageWithheld
		{
			[PXDependsOnFields(typeof(finFlag), typeof(curyFinPtdRetainageWithheld), typeof(curyTranPtdRetainageWithheld))]
			get
			{
				return ((bool)_FinFlag) ? this.CuryFinPtdRetainageWithheld : this.CuryTranPtdRetainageWithheld;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this.CuryFinPtdRetainageWithheld = value;
				}
				else
				{
					this.CuryTranPtdRetainageWithheld = value;
				}
			}
		}
		#endregion
		#region CuryYtdRetainageWithheld
		[PXDecimal(4)]
		public virtual Decimal? CuryYtdRetainageWithheld
		{
			[PXDependsOnFields(typeof(finFlag), typeof(curyFinYtdRetainageWithheld), typeof(curyTranYtdRetainageWithheld))]
			get
			{
				return ((bool)_FinFlag) ? this.CuryFinYtdRetainageWithheld : this.CuryTranYtdRetainageWithheld;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this.CuryFinYtdRetainageWithheld = value;
				}
				else
				{
					this.CuryTranYtdRetainageWithheld = value;
				}
			}
		}
		#endregion
		
		#region FinPtdRetainageReleased
		public abstract class finPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageReleased> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? FinPtdRetainageReleased
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
		public virtual Decimal? TranPtdRetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region TranYtdRetainageReleased
		public abstract class tranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageReleased> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranYtdRetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region PtdRetainageReleased
		[PXDecimal(4)]
		public virtual Decimal? PtdRetainageReleased
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finPtdRetainageReleased), typeof(tranPtdRetainageReleased))]
			get
			{
				return ((bool)_FinFlag) ? this.FinPtdRetainageReleased : this.TranPtdRetainageReleased;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this.FinPtdRetainageReleased = value;
				}
				else
				{
					this.TranPtdRetainageReleased = value;
				}
			}
		}
		#endregion
		#region YtdRetainageReleased
		[PXDecimal(4)]
		public virtual Decimal? YtdRetainageReleased
		{
			[PXDependsOnFields(typeof(finFlag), typeof(finYtdRetainageReleased), typeof(tranYtdRetainageReleased))]
			get
			{
				return ((bool)_FinFlag) ? this.FinYtdRetainageReleased : this.TranYtdRetainageReleased;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this.FinYtdRetainageReleased = value;
				}
				else
				{
					this.TranYtdRetainageReleased = value;
				}
			}
		}
		#endregion

		#region CuryFinPtdRetainageReleased
		public abstract class curyFinPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyFinPtdRetainageReleased> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinPtdRetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region CuryFinYtdRetainageReleased
		public abstract class curyFinYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainageReleased> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryFinYtdRetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region CuryTranPtdRetainageReleased
		public abstract class curyTranPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyTranPtdRetainageReleased> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranPtdRetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region CuryTranYtdRetainageReleased
		public abstract class curyTranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainageReleased> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranYtdRetainageReleased
		{
			get;
			set;
		}
		#endregion
		#region CuryPtdRetainageReleased
		[PXDecimal(4)]
		public virtual Decimal? CuryPtdRetainageReleased
		{
			[PXDependsOnFields(typeof(finFlag), typeof(curyFinPtdRetainageReleased), typeof(curyTranPtdRetainageReleased))]
			get
			{
				return ((bool)_FinFlag) ? this.CuryFinPtdRetainageReleased : this.CuryTranPtdRetainageReleased;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this.CuryFinPtdRetainageReleased = value;
				}
				else
				{
					this.CuryTranPtdRetainageReleased = value;
				}
			}
		}
		#endregion
		#region CuryYtdRetainageReleased
		[PXDecimal(4)]
		public virtual Decimal? CuryYtdRetainageReleased
		{
			[PXDependsOnFields(typeof(finFlag), typeof(curyFinYtdRetainageReleased), typeof(curyTranYtdRetainageReleased))]
			get
			{
				return ((bool)_FinFlag) ? this.CuryFinYtdRetainageReleased : this.CuryTranYtdRetainageReleased;
			}
			set
			{
				if ((bool)_FinFlag)
				{
					this.CuryFinYtdRetainageReleased = value;
				}
				else
				{
					this.CuryTranYtdRetainageReleased = value;
				}
			}
		}
		#endregion
	}
}
