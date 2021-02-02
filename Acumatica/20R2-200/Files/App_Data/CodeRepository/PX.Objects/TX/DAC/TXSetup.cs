using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.TX
{
	/// <summary>
	/// Provides access to the preferences of the Taxes module.
	/// Can be edited by user through the Tax Preferences (TX.10.30.00) screen.
	/// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(TXSetupMaint))]
	[PXCacheName(Messages.TXSetupMaint)]
	public partial class TXSetup : PX.Data.IBqlTable
	{
		#region TaxRoundingGainAcctID
		public abstract class taxRoundingGainAcctID : PX.Data.BQL.BqlInt.Field<taxRoundingGainAcctID> { }
		/// <summary>
		/// An expense account to book positive discrepancy by the credit side.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXDefault]
		[Account(null,
			DisplayName = "Tax Rounding Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		[PXUIRequired(typeof(FeatureInstalled<CS.FeaturesSet.netGrossEntryMode>))]
		public virtual int? TaxRoundingGainAcctID { get; set; }
		#endregion
		#region TaxRoundingGainSubID
		public abstract class taxRoundingGainSubID : PX.Data.BQL.BqlInt.Field<taxRoundingGainSubID> { }
		/// <summary>
		/// A subaccount to book positive discrepancy by the credit side. Visible if Subaccounts feature is activated.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXDefault]
		[SubAccount(typeof(taxRoundingGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Tax Rounding Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXUIRequired(typeof(FeatureInstalled<CS.FeaturesSet.netGrossEntryMode>))]
		public virtual int? TaxRoundingGainSubID { get; set; }
		#endregion
		#region TaxRoundingLossAcctID
		public abstract class taxRoundingLossAcctID : PX.Data.BQL.BqlInt.Field<taxRoundingLossAcctID> { }
		/// <summary>
		/// An expense account to book negative discrepancy by the debit side.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXDefault]
		[Account(null,
			DisplayName = "Tax Rounding Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description),
			AvoidControlAccounts = true)]
		[PXUIRequired(typeof(FeatureInstalled<CS.FeaturesSet.netGrossEntryMode>))]
		public virtual int? TaxRoundingLossAcctID { get; set; }
		#endregion
		#region TaxRoundingLossSubID
		public abstract class taxRoundingLossSubID : PX.Data.BQL.BqlInt.Field<taxRoundingLossSubID> { }
		/// <summary>
		/// A subaccount to book negative discrepancy by the debit side. Visible if Subaccounts feature is activated.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXDefault]
		[SubAccount(typeof(taxRoundingLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Tax Rounding Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXUIRequired(typeof(FeatureInstalled<CS.FeaturesSet.netGrossEntryMode>))]
		public virtual int? TaxRoundingLossSubID { get; set; }
		#endregion

		#region system fields
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
		#endregion
	}
}
