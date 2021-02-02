using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	[Serializable]
    [PXCacheName(Messages.BankTranHeader)]
	public partial class CABankTranHeader : IBqlTable
	{
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[PXDefault]
		[CashAccount(null, typeof(Search<CashAccount.cashAccountID,
						Where<Match<Current<AccessInfo.userName>>>>), IsKey = true, DisplayName = "Cash Account",
						Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr), Required = true)]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		[PXSelector(typeof(Search<CABankTranHeader.refNbr,
                                    Where<CABankTranHeader.cashAccountID, Equal<Optional<CABankTranHeader.cashAccountID>>,
                                      And<CABankTranHeader.tranType, Equal<Optional<CABankTranHeader.tranType>>>>,
                                    OrderBy<Desc<CABankTranHeader.refNbr>>>),
                    typeof(CABankTranHeader.refNbr),
                    typeof(CABankTranHeader.cashAccountID),
                    typeof(CABankTranHeader.curyID),
                    typeof(CABankTranHeader.docDate),
                    typeof(CABankTranHeader.endBalanceDate),
                    typeof(CABankTranHeader.curyEndBalance))]
		[CS.AutoNumber(typeof(CABankTranHeader.tranType), typeof(CABankTranHeader.docDate), 
			new string[] {CABankTranType.Statement, CABankTranType.PaymentImport},
			new Type[] { typeof(CASetup.cAStatementNumberingID), typeof(CASetup.cAImportPaymentsNumberingID) })]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(typeof(CABankTranType.statement))]
		[CABankTranType.List]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Visible = false, TabOrder = 0)]
		public virtual string TranType
		{
			get;
			set;
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }

		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Statement Date")]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5, InputMask = ">LLLLL", IsUnicode = true)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<CABankTranHeader.cashAccountID>>>>))]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region StartBalanceDate
		public abstract class startBalanceDate : PX.Data.BQL.BqlDateTime.Field<startBalanceDate> { }

		[PXDBDate]
		[PXDefault(typeof(Search<CABankTranHeader.endBalanceDate,
					 Where<CABankTranHeader.cashAccountID, Equal<Current<CABankTranHeader.cashAccountID>>,
						And<CABankTranHeader.tranType, Equal<Current<CABankTranHeader.tranType>>,
						And<CABankTranHeader.endBalanceDate, LessEqual<Current<CABankTranHeader.docDate>>,
						And<Where<Current<CABankTranHeader.refNbr>, IsNull, Or<CABankTranHeader.refNbr, NotEqual<Current<CABankTranHeader.refNbr>>>>>>>>,
						OrderBy<Desc<CABankTranHeader.startBalanceDate>>>),
						PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Start Balance Date")]
		public virtual DateTime? StartBalanceDate
		{
			get;
			set;
		}
		#endregion
		#region EndBalanceDate
		public abstract class endBalanceDate : PX.Data.BQL.BqlDateTime.Field<endBalanceDate> { }

		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "End Balance Date")]
		public virtual DateTime? EndBalanceDate
		{
			get;
			set;
		}
		#endregion
		#region CuryBegBalance
		public abstract class curyBegBalance : PX.Data.BQL.BqlDecimal.Field<curyBegBalance> { }

		[PXDBCury(typeof(CABankTranHeader.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0",
			typeof(Search<CABankTranHeader.curyEndBalance,
				 Where<CABankTranHeader.cashAccountID, Equal<Current<CABankTranHeader.cashAccountID>>,
					And<CABankTranHeader.tranType, Equal<Current<CABankTranHeader.tranType>>,
					And<CABankTranHeader.endBalanceDate, LessEqual<Current<CABankTranHeader.docDate>>,
					And<Where<Current<CABankTranHeader.refNbr>, IsNull, Or<CABankTranHeader.refNbr, NotEqual<Current<CABankTranHeader.refNbr>>>>>>>>,
					OrderBy<Desc<CABankTranHeader.startBalanceDate>>>))]
		[PXUIField(DisplayName = "Beginning Balance")]
		public virtual decimal? CuryBegBalance
		{
			get;
			set;
		}
		#endregion
		#region CuryEndBalance
		public abstract class curyEndBalance : PX.Data.BQL.BqlDecimal.Field<curyEndBalance> { }

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(CABankTranHeader.curyID))]
		[PXUIField(DisplayName = "Ending Balance")]
		public virtual decimal? CuryEndBalance
		{
			get;
			set;
		}
		#endregion
		#region CuryDebitsTotal
		public abstract class curyDebitsTotal : PX.Data.BQL.BqlDecimal.Field<curyDebitsTotal> { }

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(CABankTranHeader.curyID))]
		[PXUIField(DisplayName = "Total Receipts", Enabled = false)]
		public virtual decimal? CuryDebitsTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryCreditsTotal
		public abstract class curyCreditsTotal : PX.Data.BQL.BqlDecimal.Field<curyCreditsTotal> { }

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(CABankTranHeader.curyID))]
		[PXUIField(DisplayName = "Total Disbursements", Enabled = false)]
		public virtual decimal? CuryCreditsTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryDetailsEndBalance
		public abstract class curyDetailsEndBalance : PX.Data.BQL.BqlDecimal.Field<curyDetailsEndBalance> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Calculated Balance", Enabled = false)]
		[PXCury(typeof(CABankTranHeader.curyID))]
		public virtual decimal? CuryDetailsEndBalance
		{
			[PXDependsOnFields(typeof(curyBegBalance), typeof(curyDebitsTotal), typeof(curyCreditsTotal))]
			get
			{
				return this.CuryBegBalance + (this.CuryDebitsTotal - this.CuryCreditsTotal);
			}

			set
			{

			}
		}
		#endregion
		#region BankStatementFormat
		public abstract class bankStatementFormat : PX.Data.BQL.BqlString.Field<bankStatementFormat> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Statements Format")]
		public virtual string BankStatementFormat
		{
			get;
			set;
		}
		#endregion
		#region FormatVerisionNbr
		public abstract class formatVerisionNbr : PX.Data.BQL.BqlString.Field<formatVerisionNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Format Verision Nbr")]
		public virtual string FormatVerisionNbr
		{
			get;
			set;
		}
		#endregion
		#region TranMaxDate
		public abstract class tranMaxDate : PX.Data.BQL.BqlDateTime.Field<tranMaxDate> { }

		[PXDBDate]
		public virtual DateTime? TranMaxDate
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(new Type[0])]
		public virtual Guid? NoteID
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
	}
}
