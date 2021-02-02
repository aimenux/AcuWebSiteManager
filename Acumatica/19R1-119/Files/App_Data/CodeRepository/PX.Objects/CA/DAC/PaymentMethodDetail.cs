using System;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.CA
{
	[Serializable]
	[PXCacheName(Messages.PaymentMethodDetail)]
	public partial class PaymentMethodDetail : IBqlTable,ICCPaymentMethodDetail
	{
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(PaymentMethod.paymentMethodID))]
		[PXUIField(DisplayName = "Payment Method", Visible = false)]
		[PXParent(typeof(Select<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<PaymentMethodDetail.paymentMethodID>>>>))]
		public virtual string PaymentMethodID
			{
			get;
			set;
		}
		#endregion
		#region UseFor
		public abstract class useFor : PX.Data.BQL.BqlString.Field<useFor> { }
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(PaymentMethodDetailUsage.UseForAll)]
		[PXUIField(DisplayName = "Used In")]
		public virtual string UseFor
		{
			get;
			set;
		}
		#endregion		

		#region DetailID
		public abstract class detailID : PX.Data.BQL.BqlString.Field<detailID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "ID", Visible = true)]
		public virtual string DetailID
			{
			get;
			set;
		}
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Descr
			{
			get;
			set;
		}
		#endregion
		#region EntryMask
		public abstract class entryMask : PX.Data.BQL.BqlString.Field<entryMask> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Entry Mask")]
		public virtual string EntryMask
		{
			get;
			set;
		}
		#endregion
		#region ValidRegexp
		public abstract class validRegexp : PX.Data.BQL.BqlString.Field<validRegexp> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Validation Reg. Exp.")]
		public virtual string ValidRegexp
		{
			get;
			set;
		}
		#endregion
		#region DisplayMask
		public abstract class displayMask : PX.Data.BQL.BqlString.Field<displayMask> { }
		[PXDBString(255)]
		[PXUIField(DisplayName = "Display Mask", Enabled = false)]
		public virtual string DisplayMask
		{
			get;
			set;
		}
		#endregion
		#region IsEncrypted
		public abstract class isEncrypted : PX.Data.BQL.BqlBool.Field<isEncrypted> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Encrypted")]
		public virtual bool? IsEncrypted
			{
			get;
			set;
		}
		#endregion
		#region IsRequired
		public abstract class isRequired : PX.Data.BQL.BqlBool.Field<isRequired> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Required")]
		public virtual bool? IsRequired
			{
			get;
			set;
		}
		#endregion
		#region IsIdentifier
		public abstract class isIdentifier : PX.Data.BQL.BqlBool.Field<isIdentifier> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Card/Account No")]
		[Common.UniqueBool(typeof(PaymentMethodDetail.paymentMethodID))]
		public virtual bool? IsIdentifier
			{
			get;
			set;
		}
			#endregion
		#region IsExpirationDate
		public abstract class isExpirationDate : PX.Data.BQL.BqlBool.Field<isExpirationDate> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Exp. Date")]
		[Common.UniqueBool(typeof(PaymentMethodDetail.paymentMethodID))]
		public virtual bool? IsExpirationDate
			{
			get;
			set;
		}
		#endregion
		#region IsOwnerName
		public abstract class isOwnerName : PX.Data.BQL.BqlBool.Field<isOwnerName> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Name on Card")]
		[Common.UniqueBool(typeof(PaymentMethodDetail.paymentMethodID))]
		public virtual bool? IsOwnerName
			{
			get;
			set;
		}
		#endregion
		#region IsCCProcessingID
		public abstract class isCCProcessingID : PX.Data.BQL.BqlBool.Field<isCCProcessingID> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Payment Profile ID")]
		[Common.UniqueBool(typeof(PaymentMethodDetail.paymentMethodID))]
		public virtual bool? IsCCProcessingID
		{
			get;
			set;
		}
		#endregion
		#region IsCVV
		public abstract class isCVV : PX.Data.BQL.BqlBool.Field<isCVV> { }
		protected Boolean? _IsCVV;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "CVV Code")]
		[Common.UniqueBool(typeof(PaymentMethodDetail.paymentMethodID))]
		public virtual Boolean? IsCVV
		{
			get
			{
				return this._IsCVV;
			}
			set
			{
				this._IsCVV = value;
			}
		}
		#endregion
		#region OrderIndex
		public abstract class orderIndex : PX.Data.BQL.BqlShort.Field<orderIndex> { }
		[PXDBShort]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual short? OrderIndex
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
			{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
	}

	public class PaymentMethodDetailUsage
	{

		public const string UseForVendor = "V";
		public const string UseForCashAccount = "C";
		public const string UseForAll = "A";
		public const string UseForARCards = "R";
		public const string UseForAPCards = "P";


		public class useForVendor : PX.Data.BQL.BqlString.Constant<useForVendor>
		{
			public useForVendor() : base(UseForVendor) { }
		}

		public class useForCashAccount : PX.Data.BQL.BqlString.Constant<useForCashAccount>
		{
			public useForCashAccount() : base(UseForCashAccount) { }
		}

		public class useForAll : PX.Data.BQL.BqlString.Constant<useForAll>
		{
			public useForAll() : base(UseForAll) { }
		}

		public class useForARCards : PX.Data.BQL.BqlString.Constant<useForARCards>
		{
			public useForARCards() : base(UseForARCards) { }
		}

		public class useForAPCards : PX.Data.BQL.BqlString.Constant<useForAPCards>
		{
			public useForAPCards() : base(UseForAPCards) { }
		}

	}
}
