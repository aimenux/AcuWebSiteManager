using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.AR.BQL;

namespace PX.Objects.AR
{
	/// <summary>
	/// This Class is used in the AR Statement report. Cash Sales and Cash Returns 
	/// are excluded to prevent them from appearing in the Statement Report.
	/// </summary>
	[PXCacheName(Messages.ARStatementDetailInfo)]
	[PXProjection(typeof(Select2<
		ARStatementDetail,
			InnerJoin<ARRegister, 
				On<ARStatementDetail.docType, Equal<ARRegister.docType>,
				And<ARStatementDetail.refNbr, Equal<ARRegister.refNbr>>>,
			LeftJoin<Standalone.ARInvoice, 
				On<Standalone.ARInvoice.docType, Equal<ARRegister.docType>,
				And<Standalone.ARInvoice.refNbr, Equal<ARRegister.refNbr>>>,
			LeftJoin<Standalone.ARPayment, 
				On<Standalone.ARPayment.docType, Equal<ARRegister.docType>,
				And<Standalone.ARPayment.refNbr, Equal<ARRegister.refNbr>>>>>>,
		Where<IsNotSelfApplying<ARRegister.docType>>>), 
		Persistent = false)]
	[Serializable]
	public partial class ARStatementDetailInfo : ARStatementDetail
	{
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
		#region StatementDate
		public new abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion
		#region DocBalance
		public new abstract class docBalance : PX.Data.BQL.BqlDecimal.Field<docBalance> { }
		#endregion
		#region CuryDocBalance
		public new abstract class curyDocBalance : PX.Data.BQL.BqlDecimal.Field<curyDocBalance> { }
		#endregion
		#region IsOpen
		public new abstract class isOpen : PX.Data.BQL.BqlBool.Field<isOpen> { }
		#endregion
		#region DocStatementDate
		public abstract class docStatementDate : PX.Data.BQL.BqlDateTime.Field<docStatementDate> { }
		[PXDBDate(BqlField = typeof(ARRegister.statementDate))]
		public virtual DateTime? DocStatementDate
		{
			get;
			set;
		}
		#endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		[PXDBString(60, IsUnicode = true, BqlField = typeof(ARRegister.docDesc))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DocDesc
		{
			get;
			set;
		}
		#endregion
		#region IsMigratedRecord
		public abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the record has been created 
		/// in migration mode without affecting GL module.
		/// </summary>
		[PXDBBool(BqlField = typeof(ARRegister.isMigratedRecord))]
		public virtual bool? IsMigratedRecord
		{
			get;
			set;
		}
		#endregion

		#region PrintDocType
		public abstract class printDocType : PX.Data.BQL.BqlString.Field<printDocType> { }
		[PXString(3, IsFixed = true)]
		[ARDocType.PrintList]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual string PrintDocType
		{
			get
			{
				return this.DocType;
			}
		}
		#endregion
		#region Payable
		public abstract class payable : PX.Data.BQL.BqlBool.Field<payable> { }

		public virtual bool? Payable
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARDocType.Payable(this.DocType);
			}		
		}
        #endregion
        #region BalanceSign
        public abstract class balanceSign : PX.Data.BQL.BqlDecimal.Field<balanceSign> { }

        public virtual decimal? BalanceSign
        {
            [PXDependsOnFields(typeof(docType))]
            get
            {
                return ARDocType.SignBalance(this.DocType);
            }
        }
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		[PXDBDate(BqlField = typeof(ARRegister.docDate))]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong(BqlField=typeof(ARRegister.curyInfoID))]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARStatementDetailInfo.curyInfoID), typeof(ARStatementDetailInfo.origDocAmt), BqlField = typeof(ARRegister.curyOrigDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryOrigDocAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		[PXDBBaseCury(BqlField = typeof(ARRegister.origDocAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrigDocAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryInitDocBal
		public abstract class curyInitDocBal : PX.Data.BQL.BqlDecimal.Field<curyInitDocBal> { }
		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARStatementDetailInfo.curyInfoID), typeof(ARStatementDetailInfo.initDocBal), BqlField = typeof(ARRegister.curyInitDocBal))]
		public virtual decimal? CuryInitDocBal
		{
			get;
			set;
		}
		#endregion

		#region InitDocBal
		public abstract class initDocBal : PX.Data.BQL.BqlDecimal.Field<initDocBal> { }
		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(ARRegister.initDocBal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? InitDocBal
		{
			get;
			set;
		}
		#endregion

		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		[PXDBString(40, IsUnicode = true,BqlField= typeof(Standalone.ARInvoice.invoiceNbr))]
		[PXUIField(DisplayName = "Customer Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required=false)]
		public virtual string InvoiceNbr
		{
			get;
			set;
		}
		#endregion
		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
		[PXDBDate(BqlField=typeof(ARRegister.dueDate))]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DueDate
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		[PXDBString(40, IsUnicode = true, BqlField=typeof(Standalone.ARPayment.extRefNbr))]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region DocExtRefNbr
		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ext. Ref.#", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DocExtRefNbr
		{
			[PXDependsOnFields(typeof(payable),typeof(invoiceNbr),typeof(extRefNbr))]
			get
			{
				return this.Payable.HasValue
					? (this.Payable.Value ? this.InvoiceNbr : this.ExtRefNbr)
					: string.Empty;
			}

		}
		#endregion
		#region CuryOrigDocAmtSigned
		
		[PXDecimal]
		[PXUIField(DisplayName = "Origin. Amt", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryOrigDocAmtSigned
		{
            [PXDependsOnFields(typeof(docType), typeof(balanceSign), typeof(curyOrigDocAmt))]
			get
			{
                return BalanceSign * CuryOrigDocAmt;
			}			
		}
		#endregion
		#region OrigDocAmtSigned
		[PXDecimal]
		public virtual decimal? OrigDocAmtSigned
		{
			[PXDependsOnFields(typeof(docType), typeof(balanceSign), typeof(origDocAmt))]
			get
			{
                return BalanceSign * OrigDocAmt;
			}
		}
		#endregion

		#region CuryInitDocBalSigned
		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDecimal]
		public virtual decimal? CuryInitDocBalSigned
		{
			[PXDependsOnFields(typeof(docType), typeof(balanceSign), typeof(curyInitDocBal))]
			get
			{
				return BalanceSign * CuryInitDocBal;
			}
		}
		#endregion
		#region InitDocBalSigned
		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDecimal]
		public virtual decimal? InitDocBalSigned
		{
			[PXDependsOnFields(typeof(docType), typeof(balanceSign), typeof(initDocBal))]
			get
			{
				return BalanceSign * InitDocBal;
			}
		}
		#endregion

		#region CuryDocBalanceSigned

		[PXDecimal]
		[PXUIField(DisplayName = "Amount Due", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryDocBalanceSigned
		{
			[PXDependsOnFields(typeof(payable), typeof(curyDocBalance))]
			get
			{
				return this.Payable.HasValue
					? (this.Payable.Value ? this.CuryDocBalance	: -this.CuryDocBalance)
					: null;
			}
		}
		#endregion
		#region DocBalanceSigned
		[PXDecimal]
		public virtual decimal? DocBalanceSigned
		{
			[PXDependsOnFields(typeof(payable),typeof(docBalance))]
			get
			{
				return this.Payable.HasValue
					? (this.Payable.Value ? this.DocBalance : -this.DocBalance)
					: null;
			}
		}
		#endregion		
	}
}