using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	public class CADepositDetailType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { CheckDeposit, VoidCheckDeposit, CashDeposit, VoidCashDeposit},
				new string[] { "Check Deposit", "Void Check Deposit", "Cash Deposit", "Void Cash Deposit"}) { }
		}

		public const string CheckDeposit = "CHD";
		public const string VoidCheckDeposit = "VCD";
		public const string CashDeposit = "CSD";
		public const string VoidCashDeposit = "VSD";		
			
		public class checkDeposit : PX.Data.BQL.BqlString.Constant<checkDeposit>
		{
			public checkDeposit() : base(CheckDeposit) { }
		}

		public class voidCheckDeposit : PX.Data.BQL.BqlString.Constant<voidCheckDeposit>
		{
			public voidCheckDeposit() : base(VoidCheckDeposit) { }
		}

		public class cashDeposit : PX.Data.BQL.BqlString.Constant<cashDeposit>
		{
			public cashDeposit() : base(CashDeposit) { }
		}

		public class voidCashDeposit : PX.Data.BQL.BqlString.Constant<voidCashDeposit>
		{
			public voidCashDeposit() : base(VoidCashDeposit) { }
		}		
	}

	
	[Serializable]
	[PXCacheName(Messages.CADepositDetail)]
	public partial class CADepositDetail : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        /// <summary>
        /// The type of the deposit.
        /// This field is a part of the compound key of the deposit.
        /// </summary>
        /// <value>
        /// The field can have one of the following values: 
        /// <c>"CDT"</c>: CA Deposit;
        /// <c>"CVD"</c>: CA Void Deposit.
        /// </value>
        [PXDBString(3, IsFixed = true, IsKey = true)]
		[CATranType.DepositList]
		[PXDefault(typeof(CADeposit.tranType))]
		[PXUIField(DisplayName = "Tran. Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string TranType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
      
		/// <summary>
        /// The reference number of the deposit.
        /// This field is a part of the compound key of<see cref="DepositDetail"/>.
        /// </summary>
		[PXDBString(15, IsKey = true, InputMask = "", IsUnicode = true)]
		[PXDBDefault(typeof(CADeposit.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXParent(typeof(Select<CADeposit, Where<CADeposit.tranType, Equal<Current<CADepositDetail.tranType>>,
									And<CADeposit.refNbr, Equal<Current<CADepositDetail.refNbr>>>>>))]
		
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        /// <summary>
        /// The line number of the detail line.
        /// The <see cref="CADeposit.lineCntr"/> field depends on this field.
        /// This field is a part of the compound key of <see cref="DepositDetail"/>.
        /// </summary>
        [PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		// DecrementOnDelete is set to false to force new line always be inserted (insted of being updated in case some lines were deleted).
		[PXLineNbr(typeof(CADeposit.lineCntr), DecrementOnDelete = false)] 
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region DetailType
		public abstract class detailType : PX.Data.BQL.BqlString.Field<detailType> { }

        /// <summary>
        /// The type of the delail, which is one of the following options:
        /// <c>"CHD"</c>: Check Deposit;
        /// <c>"VCD"</c>: Void Check Deposit;
        /// <c>"CSD"</c>: Cash Deposit;
        /// <c>"VSD"</c>: Void Cash Deposit.
        /// </summary>
        [PXDBString(3, IsFixed = true)]
		[CADepositDetailType.List]
		[PXDefault(CADepositDetailType.CheckDeposit)]		
		[PXUIField(DisplayName = "Detail. Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DetailType
		{
			get;
			set;
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }

        /// <summary>
        /// The module of the origin for the payment document.
        /// </summary>
        /// /// <value>
        /// The field can have one of the following values:
        /// <c>"AP"</c>: Accounts Payable;
        /// <c>"AR"</c>: Accounts Receivable.
        /// </value>
        [PXDBString(2, IsFixed = true)]
		[PXDefault(GL.BatchModule.AR)]
		[PXUIField(DisplayName = "Doc. Module", Visibility = PXUIVisibility.Visible)]
		public virtual string OrigModule
		{
			get;
			set;
		}
		#endregion
		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

        /// <summary>
        /// The type of the payment, which is one of the following options: Payment, Credit Memo, Prepayment, or Customer Refund.
        /// </summary>
		[PXDBString(3, IsFixed = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Doc.Type", Visible = true)]	
        [CAAPARTranType.ListByModule(typeof(CADepositDetail.origModule))]
		public virtual string OrigDocType
		{
			get;
			set;
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

        /// <summary>
        /// The reference number of the payment as assigned by the system.
        /// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Reference Nbr.")]		
		public virtual string OrigRefNbr
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

        /// <summary>
        /// The payment method used by the customer for the payment.
        /// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.Null)]
		[PXUIField(DisplayName = "Payment Method", Visible = false )]
		[PXSelector(typeof(PaymentMethod.paymentMethodID))]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

        /// <summary>
        /// The cash account to hold the payment.
        /// </summary>
		[PXDefault]
		[CashAccount(DisplayName = "Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(CashAccount.descr))]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }

        /// <summary>
        /// The balance type of the <see cref="DepositDetail"/> line.
        /// </summary>
        /// <value>
        /// The field can have one of the following values:
        /// <c>"D"</c>: Receipt;
        /// <c>"C"</c>: Disbursement.
        /// </value>
        [PXDefault(CADrCr.CACredit)]
		[PXDBString(1, IsFixed = true)]
		[CADrCr.List]
		[PXUIField(DisplayName = "Disb. / Receipt")]
		public virtual string DrCr
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        /// <summary>
        /// The description of the deposit detail.
        /// This field is copied to the <see cref="CATran.TranDesc"/> field.
        /// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		public virtual string TranDesc
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        /// <summary>
        /// A flag that indicates (if selected) that the deposit is released.
        /// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion	
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        /// <summary>
        /// The identifier of the exchange rate record for the deposit.
        /// Corresponds to the <see cref="CurrencyInfo.CuryInfoID"/> field.
        /// </summary>
		[PXDBLong]
		[CurrencyInfo(typeof(CADeposit.curyInfoID))]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion		
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

        /// <summary>
        /// The amount of the payment to be deposited in the selected currency.
        /// </summary>
		[PXDBCurrency(typeof(CADepositDetail.curyInfoID), typeof(CADepositDetail.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<CADeposit.curyDetailTotal>))]
		[PXUIField(DisplayName = "Deposit Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryTranAmt
		{
			get;
			set;
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

        /// <summary>
        /// The amount of the payment to be deposited in the base currency.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tran Amount")]
		public virtual decimal? TranAmt
		{
			get;
			set;
		}
		#endregion	
		#region OrigCuryID
		public abstract class origCuryID : PX.Data.BQL.BqlString.Field<origCuryID> { }

        /// <summary>
        /// The currency of the deposit detail, which corresponds to the currency of the cash account (<see cref="CashAccount.CashAccountID"/>).
        /// </summary>
		[PXDBString(5, InputMask = ">LLLLL", IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<CADepositDetail.accountID>>>>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string OrigCuryID
		{
			get;
			set;
		}
		#endregion
		#region OrigCuryInfoID
		public abstract class origCuryInfoID : PX.Data.BQL.BqlLong.Field<origCuryInfoID> { }

        /// <summary>
        /// The currency of the original payment document, which corresponds to the currency of the cash account (<see cref="CashAccount.CuryID"/>).
        /// </summary>
		[PXDBLong]
		[CurrencyInfo(CuryIDField = "OrigCuryID")]
		public virtual long? OrigCuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigAmt
		public abstract class curyOrigAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigAmt> { }

        /// <summary>
        /// The amount of the original payment document in the selected currency.
        /// </summary>
		[PXDBCurrency(typeof(CADepositDetail.origCuryInfoID), typeof(CADepositDetail.origAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Amount", Visible = false)]
		public virtual decimal? CuryOrigAmt
		{
			get;
			set;
		}
		#endregion		
		#region OrigAmt
		public abstract class origAmt : PX.Data.BQL.BqlDecimal.Field<origAmt> { }

        /// <summary>
        /// The amount of the original payment document in the base currency.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrigAmt
		{
			get;
			set;
		}
		#endregion		
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }

        /// <summary>
        /// The identifier of the corresponding cash transaction, which corresponds to the <see cref="CATran.TranID"/> field.
        /// </summary>
		[PXDBLong]
		[PXUIField(DisplayName = "CA Tran ID")]
		[DepositDetailTranID]
		public virtual long? TranID
		{
			get;
			set;
		}
		#endregion		
		#region ChargeEntryTypeID
		public abstract class entryTypeID : PX.Data.BQL.BqlString.Field<entryTypeID> { }

        /// <summary>
        /// The entry type of the charges that apply to the payment included in the deposit.
        /// An entry type is a type of entered cash transaction that your site uses to classify the transactions for appropriate processing.
        /// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Charge Type")]
		[PXSelector(typeof(CAEntryType.entryTypeId))]
		public virtual string ChargeEntryTypeID
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion

		public abstract class origDocSign : PX.Data.BQL.BqlDecimal.Field<origDocSign> { }

        /// <summary>
        /// The sign of the amount.
        /// </summary>
        public decimal OrigDocSign 
		{
			[PXDependsOnFields(typeof(drCr), typeof(origDocType), typeof(origModule))]
			get
			{
				bool isAP = (this.OrigModule == GL.BatchModule.AP);
				return ((!isAP) ?
					((ARPaymentType.DrCr(this.OrigDocType) == CADrCr.CACredit) ? decimal.MinusOne : decimal.One) :
					((AP.APPaymentType.DrCr(this.OrigDocType) == CADrCr.CACredit) ? decimal.MinusOne : decimal.One));
			}
		}

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the original payment document created in the AP module.
        /// </summary>
		public bool IsAP
		{
			[PXDependsOnFields(typeof(origModule))]
			get
			{
				return (this.OrigModule == GL.BatchModule.AP);				
			}
		}
        #region CuryOrigAmtSigned
        /// <summary>
        /// The signed amount of the original payment document in the selected currency.
        /// </summary>
        [PXDecimal(4)]		
		public virtual decimal? CuryOrigAmtSigned
		{
			[PXDependsOnFields(typeof(curyOrigAmt), typeof(origDocSign))]
			get
			{
				return this.CuryOrigAmt * this.OrigDocSign;
			}			
		}
		#endregion
		#region OrigAmt
		[PXDecimal(4)]		
		public virtual decimal? OrigAmtSigned
		{
			[PXDependsOnFields(typeof(origAmt), typeof(origDocSign))]
			get
			{
				return this.OrigAmt * this.OrigDocSign;
			}			
		}
		#endregion		
	}
}