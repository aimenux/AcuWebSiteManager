using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.TX;

namespace PX.Objects.CA
{
	[PXProjection(typeof(Select<TaxTran, Where<TaxTran.module, Equal<BatchModule.moduleCA>>>), Persistent = true)]
    [Serializable]
	[PXCacheName(Messages.CATaxTran)]
	public partial class CATaxTran : TaxTran
	{
		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
        [PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault((string)BatchModule.CA)]
		[PXUIField(DisplayName = "Module", Enabled = false, Visible = false)]
		public override string Module
		{
			get
			{
				return this._Module;
			}

			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region TranType
		public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
        [PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(CAAdj.adjTranType))]
		[PXParent(typeof(Select<CAAdj, Where<CAAdj.adjTranType, Equal<Current<TaxTran.tranType>>, And<CAAdj.adjRefNbr, Equal<Current<TaxTran.refNbr>>>>>))]
		[PXUIField(DisplayName = "Tran. Type", Enabled = false, Visible = false)]
		public override string TranType
		{
			get
			{
				return this._TranType;
			}

			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        [PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDBDefault(typeof(CAAdj.adjRefNbr))]
		[PXUIField(DisplayName = "Ref. Nbr.", Enabled = false, Visible = false)]
		public override string RefNbr
		{
			get
			{
				return this._RefNbr;
			}

			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(typeof(Search<CashAccount.branchID, Where<CashAccount.cashAccountID, Equal<Current<CAAdj.cashAccountID>>>>), Enabled = false)]
		public override int? BranchID
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
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		#endregion
		#region TaxPeriodID
		public new abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }
		[FinPeriodID]
		public override string TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}

			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
	    [FinPeriodID(branchSourceType: typeof(CATaxTran.branchID),
	        headerMasterFinPeriodIDType: typeof(CAAdj.finPeriodID))]
        [PXDefault]
		public override string FinPeriodID
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
		#region TaxID
		public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
        [PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr), DirtyRead = true)]
		public override string TaxID
		{
			get
			{
				return this._TaxID;
			}

			set
			{
				this._TaxID = value;
			}
		}
		#endregion
		#region RecordID
		public new abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID>
		{
		}

		/// <summary>
		/// This is an auto-numbered field, which is a part of the primary key.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		public override Int32? RecordID { get; set; }
		#endregion
		#region VendorID
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[PXDBInt]
		[PXDefault(typeof(Search<Tax.taxVendorID, Where<Tax.taxID, Equal<Current<CATaxTran.taxID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public override int? VendorID
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
		#region TaxZoneID
		public new abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDBDefault(typeof(CAAdj.taxZoneID))]
		public override string TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}

			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account]
		[PXDefault]
		public override int? AccountID
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
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[SubAccount]
		[PXDefault]
		public override int? SubID
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
		#region TranDate
		public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate]
		[PXDBDefault(typeof(CAAdj.tranDate))]
		public override DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}

			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region TaxType
		public new abstract class taxType : PX.Data.BQL.BqlString.Field<taxType> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		public override string TaxType
		{
			get
			{
				return this._TaxType;
			}

			set
			{
				this._TaxType = value;
			}
		}
		#endregion
		#region TaxBucketID
		public new abstract class taxBucketID : PX.Data.BQL.BqlInt.Field<taxBucketID> { }
		[PXDBInt]
		[PXDefault(typeof(Search<TaxRev.taxBucketID, Where<TaxRev.taxID, Equal<Current<CATaxTran.taxID>>, And<Current<CATaxTran.tranDate>, Between<TaxRev.startDate, TaxRev.endDate>, And2<Where<TaxRev.taxType, Equal<Current<CATaxTran.taxType>>, Or<TaxRev.taxType, Equal<TaxType.sales>, And<Current<CATaxTran.taxType>, Equal<TaxType.pendingSales>, Or<TaxRev.taxType, Equal<TaxType.purchase>, And<Current<CATaxTran.taxType>, Equal<TaxType.pendingPurchase>>>>>>, And<TaxRev.outdated, Equal<boolFalse>>>>>>))]
		public override int? TaxBucketID
		{
			get
			{
				return this._TaxBucketID;
			}

			set
			{
				this._TaxBucketID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		[CurrencyInfo(typeof(CAAdj.curyInfoID))]
		public override long? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}

			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryTaxableAmt
		public new abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		[PXDBCurrency(typeof(CATaxTran.curyInfoID), typeof(CATaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
        [PXUnboundFormula(typeof(Switch<Case<WhereExempt<CATaxTran.taxID>, CATaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<CAAdj.curyVatExemptTotal>))]
        [PXUnboundFormula(typeof(Switch<Case<WhereTaxable<CATaxTran.taxID>, CATaxTran.curyTaxableAmt>, decimal0>), typeof(SumCalc<CAAdj.curyVatTaxableTotal>))]
		public override decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}

			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
		#region TaxableAmt
		public new abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		#endregion
		#region CuryExemptedAmt
		public new abstract class curyExemptedAmt : IBqlField { }

		/// <summary>
		/// The exempted amount in the record currency.
		/// </summary>
		[PXDBCurrency(typeof(CATaxTran.curyInfoID), typeof(CATaxTran.exemptedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public override decimal? CuryExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region ExemptedAmt
		public new abstract class exemptedAmt : IBqlField { }

		/// <summary>
		/// The exempted amount in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public override decimal? ExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public new abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		[PXDBCurrency(typeof(CATaxTran.curyInfoID), typeof(CATaxTran.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public override decimal? CuryTaxAmt
		{
			get
			{
				return this._CuryTaxAmt;
			}

			set
			{
				this._CuryTaxAmt = value;
			}
		}
		#endregion
		#region TaxAmt
		public new abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		#endregion
		#region CuryTaxAmtSumm
		public new abstract class curyTaxAmtSumm : PX.Data.BQL.BqlDecimal.Field<curyTaxAmtSumm> { }
		[PXDBCurrency(typeof(CATaxTran.curyInfoID), typeof(CATaxTran.taxAmtSumm))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override decimal? CuryTaxAmtSumm { get; set; }
		#endregion
		#region TaxAmtSumm
		public new abstract class taxAmtSumm : PX.Data.BQL.BqlDecimal.Field<taxAmtSumm> { }
		#endregion

        #region NonDeductibleTaxRate
        public new abstract class nonDeductibleTaxRate : PX.Data.BQL.BqlDecimal.Field<nonDeductibleTaxRate> { }
        #endregion
        #region ExpenseAmt
        public new abstract class expenseAmt : PX.Data.BQL.BqlDecimal.Field<expenseAmt> { }
        #endregion
        #region CuryExpenseAmt
        public new abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }
		[PXDBCurrency(typeof(CATaxTran.curyInfoID), typeof(CATaxTran.expenseAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override decimal? CuryExpenseAmt
		{
			get; set;
		}
		#endregion
	}
}
