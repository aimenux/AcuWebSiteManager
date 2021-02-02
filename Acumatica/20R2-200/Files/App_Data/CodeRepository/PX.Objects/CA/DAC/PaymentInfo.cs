using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CR;

namespace PX.Objects.CA
{
	[Serializable]
	public partial class PaymentInfo : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module>
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Doc. Module", Visibility = PXUIVisibility.Visible, Visible = true)]
		public virtual string Module
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType>
		{
		}


		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(AR.Standalone.ARPayment.docType))]
		[CAAPARTranType.ListByModule(typeof(module))]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
		}

		[PXDBString(15, IsKey = true, InputMask = "", BqlField = typeof(AR.Standalone.ARPayment.refNbr))]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID>
		{
		}

		[PXDBInt]
		[PXSelector(typeof(BAccountR.bAccountID), SubstituteKey = typeof(BAccountR.acctCD), DescriptionField = typeof(BAccountR.acctName), CacheGlobal = true)]
		[PXUIField(DisplayName = "Customer/Vendor", Enabled = true, Visible = true)]
		public virtual int? BAccountID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID>
		{
		}

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<PaymentInfo.bAccountID>>>), DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		[PXUIField(DisplayName = "Location")]
		public virtual int? LocationID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID>
		{
		}

		[PXDBString(10, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIField(DisplayName = "Payment Method", Visible = true)]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr>
		{
		}

		[PXDBString(40, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.Visible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
		}

		[PXDBString(1, IsFixed = true)]
		[PXDefault(APDocStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[APDocStatus.List]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate>
		{
		}

		[PXDBDate]
		[PXUIField(DisplayName = "Payment Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID>
		{
		}

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visible = true, Enabled = false)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID>
		{
		}

		[PXDBLong]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt>
		{
		}

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(PaymentInfo.curyID))]
		[PXUIField(DisplayName = "Payment Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryOrigDocAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt>
		{
		}

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigDocAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryChargeTotal
		public abstract class curyChargeTotal : PX.Data.BQL.BqlDecimal.Field<curyChargeTotal>
		{
		}

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(PaymentInfo.curyID))]
		[PXUIField(DisplayName = "Charge Amount", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryChargeTotal
		{
			get;
			set;
		}
		#endregion
		#region ChargeTotal
		public abstract class chargeTotal : PX.Data.BQL.BqlDecimal.Field<chargeTotal>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ChargeTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryGrossPaymentAmount
		public abstract class curyGrossPaymentAmount : PX.Data.BQL.BqlDecimal.Field<curyGrossPaymentAmount>
		{
		}

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(PaymentInfo.curyID))]
		[PXUIField(DisplayName = "Gross Payment Amount", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryGrossPaymentAmount
		{
			get;
			set;
		}
		#endregion
		#region GrossPaymentAmount
		public abstract class grossPaymentAmount : PX.Data.BQL.BqlDecimal.Field<grossPaymentAmount>
		{
		}
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? GrossPaymentAmount
		{
			get;
			set;
		}
		#endregion
		#region DepositAfter
		public abstract class depositAfter : PX.Data.BQL.BqlDateTime.Field<depositAfter>
		{
		}

		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deposit After", Enabled = false, Visible = false)]
		public virtual DateTime? DepositAfter
		{
			get;
			set;
		}
		#endregion
		#region DepositType
		public abstract class depositType : PX.Data.BQL.BqlString.Field<depositType>
		{
		}

		[PXDBString(3, IsFixed = true, BqlField = typeof(AR.Standalone.ARPayment.depositType))]
		public virtual string DepositType
		{
			get;
			set;
		}
		#endregion
		#region DepositNbr
		public abstract class depositNbr : PX.Data.BQL.BqlString.Field<depositNbr>
		{
		}

		[PXDBString(15, IsUnicode = true, BqlField = typeof(AR.Standalone.ARPayment.depositNbr))]
		[PXUIField(DisplayName = "Batch Deposit Nbr.", Enabled = false)]
		public virtual string DepositNbr
		{
			get;
			set;
		}
		#endregion
		#region Deposited
		public abstract class deposited : PX.Data.BQL.BqlBool.Field<deposited>
		{
		}

		[PXDBBool(BqlField = typeof(AR.Standalone.ARPayment.deposited))]
		[PXDefault(false)]
		public virtual bool? Deposited
		{
			get;
			set;
		}
		#endregion
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID>
		{
		}

		[PXDBInt]
		[PXUIField(DisplayName = "Card/Account No")]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
		public virtual int? PMInstanceID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID>
		{
		}

		[CashAccount(Visibility = PXUIVisibility.Visible)]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
	}
}
