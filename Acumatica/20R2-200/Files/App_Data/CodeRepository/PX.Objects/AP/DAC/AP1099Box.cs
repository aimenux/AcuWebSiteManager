namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	
    /// <summary>
    /// Represents a type of 1099 payment, which generally corresponds to a box on the 1099-MISC form.
    /// This DAC is used by Acumatica ERP to track 1099-related payments.
    /// 1099 boxes are configured through the AP Preferences screen (AP.10.10.00).
    /// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.AP1099Box)]
	public partial class AP1099Box : PX.Data.IBqlTable
	{
		#region BoxNbr
		public abstract class boxNbr : PX.Data.BQL.BqlShort.Field<boxNbr> { }
		protected Int16? _BoxNbr;

        /// <summary>
        /// <b>[key]</b> The line number, which is automatically added. A box is used for each payment made to a 1099 vendor.
        /// </summary>
		[PXDBShort(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName="Box", Visibility=PXUIVisibility.Visible, Enabled=false)]
		public virtual Int16? BoxNbr
		{
			get
			{
				return this._BoxNbr;
			}
			set
			{
				this._BoxNbr = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

        /// <summary>
        /// The description of the 1099 type, which usually is based on the box’s name on the 1099-MISC form.
        /// </summary>
		[PXDBString(60, IsUnicode=true)]
		[PXUIField(DisplayName="Description", Visibility=PXUIVisibility.Visible, Enabled=false)]
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
		#region MinReportAmt
		public abstract class minReportAmt : PX.Data.BQL.BqlDecimal.Field<minReportAmt> { }
		protected Decimal? _MinReportAmt;

        /// <summary>
        /// The minimum payment amount for the 1099 type to be included for reporting.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName="Minimum Report Amount", Visibility=PXUIVisibility.Visible)]
		public virtual Decimal? MinReportAmt
		{
			get
			{
				return this._MinReportAmt;
			}
			set
			{
				this._MinReportAmt = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;

        /// <summary>
        /// The optional default expense account associated with this type of 1099 payment.
        /// </summary>
        /// <value>
        /// Serves as a link to <see cref="Account"/>.
        /// </value>
		[UnboundAccount(DisplayName = "Account", Visibility = PXUIVisibility.Visible)]
		[AvoidControlAccounts]
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
		#region OldAccountID
		public abstract class oldAccountID : PX.Data.BQL.BqlInt.Field<oldAccountID> { }
		protected Int32? _OldAccountID;
        /// <summary>
        /// System field used to perform two sided update of the 1099Box-<see cref="Account"/> relation.
        /// </summary>
		[PXInt()]
		public virtual Int32? OldAccountID
		{
			get
			{
				return this._OldAccountID;
			}
			set
			{
				this._OldAccountID = value;
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
	}
}
