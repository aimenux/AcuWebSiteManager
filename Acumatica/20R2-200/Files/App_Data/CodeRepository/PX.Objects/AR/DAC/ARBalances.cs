using System;
using PX.Data;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents a balance record of an Accounts Receivable customer.
	/// Customer balances are accumulated into records across the 
	/// following dimensions: branch, customer, and customer location. 
	/// The balance records are created and updated by the <see cref="ARDocumentRelease"/> 
	/// graph during the document release process.
	/// </summary>
	[Serializable]
    [Overrides.ARDocumentRelease.ARBalAccum]
	[PXCacheName(Messages.ARBalances)]
    public partial class ARBalances : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDBInt(IsKey=true)]
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

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARBalances.customerID>>>>))]
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? CustomerID { get; set; }
		#endregion

		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		protected Int32? _CustomerLocationID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? CustomerLocationID
		{
			get
			{
				return this._CustomerLocationID;
			}
			set
			{
				this._CustomerLocationID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
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
		#region CurrentBal
		public abstract class currentBal : PX.Data.BQL.BqlDecimal.Field<currentBal> { }
		protected Decimal? _CurrentBal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CurrentBal
		{
			get
			{
				return this._CurrentBal;
			}
			set
			{
				this._CurrentBal = value;
			}
		}
		#endregion
		#region UnreleasedBal
		public abstract class unreleasedBal : PX.Data.BQL.BqlDecimal.Field<unreleasedBal> { }
		protected Decimal? _UnreleasedBal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnreleasedBal
		{
			get
			{
				return this._UnreleasedBal;
			}
			set
			{
				this._UnreleasedBal = value;
			}
		}
		#endregion
		#region TotalPrepayments
		public abstract class totalPrepayments : PX.Data.BQL.BqlDecimal.Field<totalPrepayments> { }
		protected Decimal? _TotalPrepayments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalPrepayments
		{
			get
			{
				return this._TotalPrepayments;
			}
			set
			{
				this._TotalPrepayments = value;
			}
		}
		#endregion
		#region TotalQuotations
		public abstract class totalQuotations : PX.Data.BQL.BqlDecimal.Field<totalQuotations> { }
		protected Decimal? _TotalQuotations;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalQuotations
		{
			get
			{
				return this._TotalQuotations;
			}
			set
			{
				this._TotalQuotations = value;
			}
		}
		#endregion
		#region TotalOpenOrders
		public abstract class totalOpenOrders : PX.Data.BQL.BqlDecimal.Field<totalOpenOrders> { }
		protected Decimal? _TotalOpenOrders;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalOpenOrders
		{
			get
			{
				return this._TotalOpenOrders;
			}
			set
			{
				this._TotalOpenOrders = value;
			}
		}
		#endregion
		#region TotalShipped
		public abstract class totalShipped : PX.Data.BQL.BqlDecimal.Field<totalShipped> { }
		protected Decimal? _TotalShipped;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalShipped
		{
			get
			{
				return this._TotalShipped;
			}
			set
			{
				this._TotalShipped = value;
			}
		}
		#endregion
		#region LastInvoiceDate
		public abstract class lastInvoiceDate : PX.Data.BQL.BqlDateTime.Field<lastInvoiceDate> { }
		protected DateTime? _LastInvoiceDate;
		[PXDBDate()]
		public virtual DateTime? LastInvoiceDate
		{
			get
			{
				return this._LastInvoiceDate;
			}
			set
			{
				this._LastInvoiceDate = value;
			}
		}
		#endregion
		#region OldInvoiceDate
		public abstract class oldInvoiceDate : PX.Data.BQL.BqlDateTime.Field<oldInvoiceDate> { }
		protected DateTime? _OldInvoiceDate;
		[PXDBDate()]
		public virtual DateTime? OldInvoiceDate
		{
			get
			{
				return this._OldInvoiceDate;
			}
			set
			{
				this._OldInvoiceDate = value;
			}
		}
		#endregion
		#region NumberInvoicePaid
		public abstract class numberInvoicePaid : PX.Data.BQL.BqlInt.Field<numberInvoicePaid> { }
		protected Int32? _NumberInvoicePaid;
		[PXDBInt()]
		public virtual Int32? NumberInvoicePaid
		{
			get
			{
				return this._NumberInvoicePaid;
			}
			set
			{
				this._NumberInvoicePaid = value;
			}
		}
		#endregion
		#region PaidInvoiceDays
		public abstract class paidInvoiceDays : PX.Data.BQL.BqlInt.Field<paidInvoiceDays> { }
		protected Int32? _PaidInvoiceDays;
		[PXDBInt()]
		public virtual Int32? PaidInvoiceDays
		{
			get
			{
				return this._PaidInvoiceDays;
			}
			set
			{
				this._PaidInvoiceDays = value;
			}
		}
		#endregion
		#region AverageDaysToPay
		public abstract class averageDaysToPay : PX.Data.BQL.BqlInt.Field<averageDaysToPay> { }
		protected Int32? _AverageDaysToPay;
		[PXDBInt()]
		public virtual Int32? AverageDaysToPay
		{
			get
			{
				return this._AverageDaysToPay;
			}
			set
			{
				this._AverageDaysToPay = value;
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
        #region DatesUpdated
        public abstract class datesUpdated : PX.Data.BQL.BqlBool.Field<datesUpdated> { }
        protected Boolean? _DatesUpdated;
        [PXBool]
        [PXDefault(false)]
        public virtual Boolean? DatesUpdated
        {
            get
            {
                return this._DatesUpdated;
            }
            set
            {
                this._DatesUpdated = value;
            }
        }
		#endregion
		#region LastDocDate
		public abstract class lastDocDate : PX.Data.BQL.BqlDateTime.Field<lastDocDate> { }
		[PXDBDate()]
		public virtual DateTime? LastDocDate { get; set; }
		#endregion
		#region StatementRequired
		public abstract class statementRequired : PX.Data.BQL.BqlBool.Field<statementRequired> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? StatementRequired { get; set; }
		#endregion
	}
}
