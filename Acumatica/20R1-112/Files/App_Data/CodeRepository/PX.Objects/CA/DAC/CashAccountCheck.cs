using System;
using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.CA
{
	[Serializable]
	[PXCacheName(Messages.CashAccountCheck)]
	public partial class CashAccountCheck : IBqlTable
	{
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Cash Account ID", Visible = false)]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(PaymentMethod.paymentMethodID))]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountCheckID
		public abstract class cashAccountCheckID : PX.Data.BQL.BqlInt.Field<cashAccountCheckID> { }
		[PXDBIdentity]
		public virtual int? CashAccountCheckID
		{
			get;
			set;
		}
		#endregion
		#region CheckNbr
		public abstract class checkNbr : PX.Data.BQL.BqlString.Field<checkNbr> { }

		[PXDBString(40, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Check Number")]
		[PXDefault]
		[PXParent(typeof(Select<APAdjust, Where<APAdjust.stubNbr, Equal<Current<CashAccountCheck.checkNbr>>,
			And<APAdjust.paymentMethodID, Equal<Current<CashAccountCheck.paymentMethodID>>,
			And<APAdjust.cashAccountID, Equal<Current<CashAccountCheck.accountID>>,
			And<APAdjust.voided, NotEqual<True>>>>>>))]
		public virtual string CheckNbr
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[APDocType.List]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[PXDBString(6, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Application Period", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[GL.FinPeriodIDFormatting]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Document Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[PXDefault]
		[Vendor]
		public virtual int? VendorID
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
	}
}
