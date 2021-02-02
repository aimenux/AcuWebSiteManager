namespace PX.Objects.AP
{
	using System;
	using PX.Data;
    using PX.Objects.CM;
	using PX.Objects.GL.Attributes;
	using PX.Objects.GL.Formula;

	/// <summary>
	/// Represents an Accounts Payable history record, which accumulates a number 
	/// of important year-to-date and period-to-date amounts (such as purchases, debit and credit 
	/// adjustments, gains and losses) in base currency. The history is accumulated separately 
	/// across the following dimensions: branch, GL account, GL subaccount, financial period, 
	/// and vendor. History records are created and updated during the document release
	/// process (see <see cref="APDocumentRelease"/> graph). Various helper projections
	/// over this DAC are used in a number of AR inquiry forms and reports, such as Vendor 
	/// Summary (AP401000).
	/// </summary>
	[PXCacheName(Messages.APHistory)]
	[System.SerializableAttribute()]
	public partial class APHistory : PX.Data.IBqlTable
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
		[Vendor(IsKey = true, DisplayName = "Vendor ID")]
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
		//[PXDBDecimal(4)]
        [PXDBBaseCury()]
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
        [PXUIField(DisplayName="PTD Revalued Amount")]
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
		//---
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
	}
}
