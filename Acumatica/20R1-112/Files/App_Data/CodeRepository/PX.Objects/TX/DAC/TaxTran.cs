using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.TX
{

	/// <summary>
	/// Represents a tax transaction. The entity serves for the following two purposes simultaneously:
	/// <li/>To report the tax amount, which is stored in the TaxTran record, to a tax period.
	/// A tax report is built by TaxTran records, and reportable TaxTran records belong to the <see cref="TaxTran.TaxPeriodID">specified tax period</see> after the tax report is prepared.
	/// <li/>To store the tax amount of the document for each applied tax.
	/// </summary>
	[PXCacheName(Messages.TaxTransaction)]
	[Serializable]
	public partial class TaxTran : TaxDetail, PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;

		/// <summary>
		/// Indicates (if set to <c>true</c>) that the tax transaction is selected on a form.
		/// </summary>
		[PXBool]
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
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;

		/// <summary>
		/// The source module of the record.
		/// The field is a part of the primary key.
		/// </summary>
        [PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(BatchModule.GL)]
		[PXUIField(DisplayName = "Module")]
		public virtual String Module
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
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType>
		{
			public const string TranForward = "TFW";
			public const string TranReversed = "TRV";
		}
		protected String _TranType;

		/// <summary>
		/// The type of the record.
		/// </summary>
		/// <value>
		/// If the record is related to a document, the field contains the type of the document.
		/// In other cases, the field contains one of the following values:
		/// <c>TFW</c>: GL tax record
		/// <c>TRV</c>: GL tax record
		/// <c>INT</c>: Output tax adjustment
		/// <c>RET</c>: Input tax adjustment
		/// <c>VTI</c>: Input VAT
		/// <c>VTO</c>: Output VAT
		/// <c>REI</c>: Reverse input VAT
		/// <c>REO</c>: Reverse output VAT
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXDBDefault(typeof(TaxAdjustment.docType))]
		[PXParent(typeof(Select<TaxAdjustment, Where<TaxAdjustment.docType, Equal<Current<TaxTran.tranType>>, And<TaxAdjustment.refNbr, Equal<Current<TaxTran.refNbr>>>>>))]
		[TaxAdjustmentType.List()]
		[PXUIField(DisplayName="Tran. Type")]
		public virtual String TranType
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
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;

		/// <summary>
		/// The reference number of the document to which the record is releated.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXDBDefault(typeof(TaxAdjustment.refNbr))]
		[PXUIField(DisplayName = "Ref. Nbr.")]
		public virtual String RefNbr
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
        #region LineRefNbr
        public abstract class lineRefNbr : PX.Data.BQL.BqlString.Field<lineRefNbr> { }
        protected String _LineRefNbr;

		/// <summary>
		/// The reference number of the transaction to which the record is related.
		/// The field is used for the records that are created from GL.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Line Ref. Number")]
        [PXDefault("")]
        public virtual String LineRefNbr
        {
            get
            {
                return this._LineRefNbr;
            }
            set
            {
                this._LineRefNbr = value;
            }
        }
        #endregion
		#region OrigTranType
		public abstract class origTranType : PX.Data.BQL.BqlString.Field<origTranType> { }
		protected String _OrigTranType;

		/// <summary>
		/// The original document type for which the tax amount has been entered.
		/// The field is used for the records that are created on the Tax Bills and Adjustments (TX303000) form.
	    /// </summary>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Orig. Tran. Type")]
		[PXDefault("")]
		public virtual String OrigTranType
		{
			get
			{
				return this._OrigTranType;
			}
			set
			{
				this._OrigTranType = value;
			}
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		protected String _OrigRefNbr;

		/// <summary>
		/// The original document reference number for which the tax amount has been entered.
		/// The field is used for the records that are created on the Tax Bills and Adjustments (TX303000) form.
	    /// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Orig. Doc. Number")]
		[PXDefault("")]
		public virtual String OrigRefNbr
		{
			get
			{
				return this._OrigRefNbr;
			}
			set
			{
				this._OrigRefNbr = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;

		/// <summary>
		/// Indicates (if set to <c>true</c>) that the record has been released.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		protected Boolean? _Voided;

		/// <summary>
		/// Indicates (if set to <c>true</c>) that the record has been voided.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID :  PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;

		/// <summary>
		/// The reference to the <see cref="Branch"/> record to which the record belongs.
		/// </summary>
		/// <value>The value is copied from the document from which the record is created.</value>
		[Branch(Enabled = false)]
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
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;

		/// <summary>
		/// The reference to the financial period of the document to which the record belongs.
		/// </summary>
		[FinPeriodID(branchSourceType: typeof(TaxTran.branchID),
			headerMasterFinPeriodIDType: typeof(TaxAdjustment.tranPeriodID))]				
		[PXDefault]
		public virtual String FinPeriodID
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
        #region FinDate
		public abstract class finDate : PX.Data.BQL.BqlDateTime.Field<finDate> { }

		/// <summary>
		/// The last day (<see cref="PX.Objects.GL.Obsolete.FinPeriod.FinDate"/>) of the financial period of the document to which the record belongs.
		/// </summary>
		[PXDBDate()]
		[PXDBDefault(typeof(Search2<OrganizationFinPeriod.finDate,
		    InnerJoin<Branch,
		        On<OrganizationFinPeriod.organizationID, Equal<Branch.organizationID>>>,
		    Where<Branch.branchID, Equal<Current2<TaxTran.branchID>>,
		        And<OrganizationFinPeriod.finPeriodID, Equal<Current2<TaxTran.finPeriodID>>>>>))]
		public virtual DateTime? FinDate
		{
			get;
			set;
		}
		#endregion
		#region TaxPeriodID
		public abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }
		protected String _TaxPeriodID;

		/// <summary>
		/// The key of the tax period to which the record has been reported.
		/// The field has the null value for the unreported records.
		/// </summary>
		[GL.FinPeriodID]
		[PXDBDefault(typeof(TaxAdjustment.taxPeriod), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string TaxPeriodID
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
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }

		/// <summary>
		/// The reference to the <see cref="Tax"/> record.
		/// </summary>
		[PXDBString(Tax.taxID.Length, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Tax.taxID,Where<Tax.taxVendorID,Equal<Current<TaxAdjustment.vendorID>>>>), DirtyRead = true)]
		[PXForeignReference(typeof(Field<TaxTran.taxID>.IsRelatedTo<Tax.taxID>))]
		public override String TaxID
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
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID>
        {
        }
		protected Int32? _RecordID;

		/// <summary>
		/// This is an auto-numbered field, which is a part of the primary key.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region JurisType
		public abstract class jurisType : PX.Data.BQL.BqlString.Field<jurisType> { }
        protected String _JurisType;

		/// <summary>
		/// The tax jurisdiction type. The field is used for the taxes from Avalara.
		/// </summary>
		[PXDBString(9, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Jurisdiction Type")]
        public virtual String JurisType
        {
            get
            {
                return this._JurisType;
            }
            set
            {
                this._JurisType = value;
            }
        }
        #endregion
        #region JurisName
        public abstract class jurisName : PX.Data.BQL.BqlString.Field<jurisName> { }
        protected String _JurisName;

		/// <summary>
		/// The tax jurisdiction name. The field is used for the taxes from Avalara.
		/// </summary>
		[PXDBString(200, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Jurisdiction Name")]
        public virtual String JurisName
        {
            get
            {
                return this._JurisName;
            }
            set
            {
                this._JurisName = value;
            }
        }
	#endregion		
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;

		/// <summary>
		/// The foreign key to <see cref="PX.Objects.AP.Vendor"/>, which specifies the tax agency to which the record belongs.
		/// </summary>
		/// <value>
		/// When the record is created, the field is assigned the default value based on the document.
		/// The value of the field is updated during preparation of a tax report.
		/// </value>
        [PXDBInt()]
        [PXDBDefault(typeof(TaxAdjustment.vendorID))]
		public virtual Int32? VendorID
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
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;

		/// <summary>
		/// The revision of the tax report to which the record was included.
		/// </summary>
		[PXDBInt()]
		public virtual Int32? RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;

		/// <summary>
		/// The reference to the vendor record (<see cref="Vendor.BAccountID"/>) or customer record (<see cref="Customer.BAccountID"/>).
		/// The field is used for the records that have been created in the AP or AR module.
		/// </summary>
		[PXDBInt]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;

		/// <summary>
		/// The reference to the tax zone (<see cref="TaxZone.TaxZoneID"/>). The value is assigned based on the document to which the record belongs.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName="Tax Zone")]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>))]
		[PXDefault()]
		public virtual String TaxZoneID
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
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;

		/// <summary>
		/// The reference to the account record (<see cref="Account.AccountID"/>) of the related <see cref="Tax"/> record.
		/// </summary>
		[Account(DisplayName = "Account", Visibility = PXUIVisibility.Visible, ControlAccountForModule = ControlAccountModule.TX)]
		[PXDefault()]
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
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;

		/// <summary>
		/// The reference to the subaccount (<see cref="Account.AccountID"/>) of the related <see cref="Tax"/> record.
		/// </summary>
		[SubAccount(typeof(TaxTran.accountID), DisplayName = "Sub.", Visibility = PXUIVisibility.Visible)]
		[PXDefault()]
		public virtual Int32? SubID
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
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;

		/// <summary>
		/// The date of the tax record.
		/// </summary>
		/// <value>
		/// The value corresponds to the date of the document to which the record belongs.
		/// </value>
		[PXDBDate()]
		[PXDBDefault(typeof(TaxAdjustment.docDate))]
		[PXUIField(DisplayName = "Tran. Date")]
		public virtual DateTime? TranDate
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
		#region TaxInvoiceNbr
		public abstract class taxInvoiceNbr : PX.Data.BQL.BqlString.Field<taxInvoiceNbr> { }
		protected String _TaxInvoiceNbr;

		/// <summary>
		/// The reference number of the tax invoice. The field is used for recognized SVAT records.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Invoice Nbr.")]
		public virtual String TaxInvoiceNbr
		{
			get
			{
				return this._TaxInvoiceNbr;
			}
			set
			{
				this._TaxInvoiceNbr = value;
			}
		}
		#endregion
		#region TaxInvoiceDate
		public abstract class taxInvoiceDate : PX.Data.BQL.BqlDateTime.Field<taxInvoiceDate> { }
		protected DateTime? _TaxInvoiceDate;

		/// <summary>
		/// The date of the tax invoice. The field is used for recognized SVAT records.
		/// </summary>
		[PXDBDate(InputMask = "d", DisplayMask = "d")]
		[PXUIField(DisplayName = "Tax Invoice Date")]
		public virtual DateTime? TaxInvoiceDate
		{
			get
			{
				return this._TaxInvoiceDate;
			}
			set
			{
				this._TaxInvoiceDate = value;
			}
		}
		#endregion
		#region TaxType
		public abstract class taxType : PX.Data.BQL.BqlString.Field<taxType> { }

		/// <summary>
		/// The reporting type of the tax, as it has been recognized by the system.
		/// The value is used to calculate the sign of record amounts during preparation of the tax report.
		/// </summary>
		/// <value>
		/// <c>S</c>: Output (sales)
		/// <c>P</c>: Input (purchase)
		/// <c>A</c>: Suspended VAT output
		/// <c>B</c>: Suspended VAT input
		/// </value>
		protected String _TaxType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public virtual String TaxType
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
		public abstract class taxBucketID : PX.Data.BQL.BqlInt.Field<taxBucketID> { }
		protected Int32? _TaxBucketID;

		/// <summary>
		/// The reference to the reporting group (<see cref="TaxBucket.BucketID"/>) for which the record should be reported.
		/// During record creation, the value of the field is assigned based on the document.
		/// During preparation of the tax report, the value is updated from the relevant<see cref= "TaxRev.TaxBucketID" /> record.
	    /// </summary>
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? TaxBucketID
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
		#region TaxRate
		public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }

		/// <summary>
		/// The tax rate of the relevant <see cref="Tax"/> record.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override Decimal? TaxRate
		{
			get
			{
				return this._TaxRate;
			}
			set
			{
				this._TaxRate = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		/// <summary>
		/// The reference to the <see cref="CurrencyInfo"/> record that is related to the document.
		/// </summary>
		[PXDBLong()]
		[CurrencyInfo(typeof(TaxAdjustment.curyInfoID))]
		public override Int64? CuryInfoID
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
		#region CuryOrigTaxableAmt
		public abstract class curyOrigTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigTaxableAmt> { }

		/// <summary>
		/// The original taxable amount (before truncation by minimal or maximal value) in the record currency.
		/// </summary>
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.origTaxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Orig. Taxable Amount")]
		public virtual Decimal? CuryOrigTaxableAmt { get; set; }
		#endregion
		#region OrigTaxableAmt
		public abstract class origTaxableAmt : PX.Data.BQL.BqlDecimal.Field<origTaxableAmt> { }

		/// <summary>
		/// The original taxable amount (before truncation by minimal or maximal value) in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Orig. Taxable Amount")]
		public virtual Decimal? OrigTaxableAmt { get; set; }
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		protected decimal? _CuryTaxableAmt;

		/// <summary>
		/// The taxable amount in the record currency.
		/// </summary>
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryTaxableAmt
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
		public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		protected Decimal? _TaxableAmt;

		/// <summary>
		/// The taxable amount in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? TaxableAmt
		{
			get
			{
				return this._TaxableAmt;
			}
			set
			{
				this._TaxableAmt = value;
			}
		}
		#endregion
		#region CuryExemptedAmt
		public abstract class curyExemptedAmt : IBqlField { }

		/// <summary>
		/// The exempted amount in the record currency.
		/// </summary>
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.exemptedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public virtual decimal? CuryExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region ExemptedAmt
		public abstract class exemptedAmt : IBqlField { }

		/// <summary>
		/// The exempted amount in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public virtual decimal? ExemptedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		protected decimal? _CuryTaxAmt;

		/// <summary>
		/// The tax amount in the record currency.
		/// </summary>
		[PXDBCurrency(typeof(TaxTran.curyInfoID),typeof(TaxTran.taxAmt))]
		[PXFormula(typeof(Mult<TaxTran.curyTaxableAmt, Div<TaxTran.taxRate, decimal100>>), typeof(SumCalc<TaxAdjustment.curyDocBal>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryTaxAmt
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
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		protected Decimal? _TaxAmt;

		/// <summary>
		/// The tax amount in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? TaxAmt
		{
			get
			{
				return this._TaxAmt;
			}
			set
			{
				this._TaxAmt = value;
			}
		}
		#endregion
		#region CuryTaxAmtSumm
		public abstract class curyTaxAmtSumm : PX.Data.BQL.BqlDecimal.Field<curyTaxAmtSumm> { }

		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.taxAmtSumm))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTaxAmtSumm { get; set; }
		#endregion
		#region TaxAmtSumm
		public abstract class taxAmtSumm : PX.Data.BQL.BqlDecimal.Field<taxAmtSumm> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxAmtSumm { get; set; }
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;

		/// <summary>
		/// The reference to the currency (<see cref="Currency.CuryID"/>) of the document to which the record belongs.
		/// </summary>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency")]
		[PXSelector(typeof(Currency.curyID))]
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
		#region ReportCuryID
		public abstract class reportCuryID : PX.Data.BQL.BqlString.Field<reportCuryID> { }
		protected String _ReportCuryID;

		/// <summary>
		/// The reference to the currency (<see cref="Currency.CuryID"/>) of the tax agency.
		/// </summary>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Report Currency")]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String ReportCuryID
		{
			get
			{
				return this._ReportCuryID;
			}
			set
			{
				this._ReportCuryID = value;
			}
		}
		#endregion
		#region ReportCuryRateTypeID
		public abstract class reportCuryRateTypeID : PX.Data.BQL.BqlString.Field<reportCuryRateTypeID> { }
		protected String _ReportCuryRateTypeID;

		/// <summary>
		/// The reference to the currency rate type (<see cref="CurrencyRateType.CuryRateTypeID"/>), 
		/// which is used during report preparation to obtain amounts in the tax agency currency.
		/// </summary>
		[PXDBString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "Report Currency Rate Type", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID), DescriptionField = typeof(CurrencyRateType.descr))]
		public virtual String ReportCuryRateTypeID
		{
			get
			{
				return this._ReportCuryRateTypeID;
			}
			set
			{
				this._ReportCuryRateTypeID = value;
			}
		}
		#endregion
		#region ReportCuryEffDate
		public abstract class reportCuryEffDate : PX.Data.BQL.BqlDateTime.Field<reportCuryEffDate> { }
		protected DateTime? _ReportCuryEffDate;

		/// <summary>
		/// The effective date of the currency rate (<see cref="CurrencyRate.CuryEffDate"/>), 
		/// which is used during report preparation to obtain amounts in the tax agency currency.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Report Effective Date")]
		public virtual DateTime? ReportCuryEffDate
		{
			get
			{
				return this._ReportCuryEffDate;
			}
			set
			{
				this._ReportCuryEffDate = value;
			}
		}
		#endregion
		#region ReportCuryMultDiv
		public abstract class reportCuryMultDiv : PX.Data.BQL.BqlString.Field<reportCuryMultDiv> { }
		protected String _ReportCuryMultDiv;

		/// <summary>
		/// The conversion type of the currency rate (<see cref="CurrencyRate.CuryMultDiv"/>), 
		/// which is used during report preparation to obtain amounts in the tax agency currency.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault("M", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Report Mult Div")]
		public virtual String ReportCuryMultDiv
		{
			get
			{
				return this._ReportCuryMultDiv;
			}
			set
			{
				this._ReportCuryMultDiv = value;
			}
		}
		#endregion
		#region ReportCuryRate
		public abstract class reportCuryRate : PX.Data.BQL.BqlDecimal.Field<reportCuryRate> { }
		protected Decimal? _ReportCuryRate;

		/// <summary>
		/// The currency rate value (<see cref="CurrencyRate.CuryRate"/>) of the (<see cref="CurrencyRate"/>) record 
		/// which is used on report prepare to obtaion amounts in the tax agency currency.
		/// </summary>
		[PXDBDecimal(8)]
		[PXDefault(TypeCode.Decimal, "1.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? ReportCuryRate
		{
			get
			{
				return this._ReportCuryRate;
			}
			set
			{
				this._ReportCuryRate = value;
			}
		}
		#endregion
		#region ReportTaxableAmt
		public abstract class reportTaxableAmt : PX.Data.BQL.BqlDecimal.Field<reportTaxableAmt> { }
		protected Decimal? _ReportTaxableAmt;

		/// <summary>
		/// The taxable amount in the tax agency currency.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal]
		[PXUIField(DisplayName = "Report Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReportTaxableAmt
		{
			get
			{
				return this._ReportTaxableAmt;
			}
			set
			{
				this._ReportTaxableAmt = value;
			}
		}
		#endregion		
		#region ReportExemptedAmt
		public abstract class reportExemptedAmt : IBqlField
		{
		}

		/// <summary>
		/// The exempted amount in the tax agency currency.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal]
		[PXUIField(DisplayName = "Report Exempted Amount", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.ExemptedTaxReporting))]
		public virtual Decimal? ReportExemptedAmt { get; set; }
		#endregion
		#region ReportTaxAmt
		public abstract class reportTaxAmt : PX.Data.BQL.BqlDecimal.Field<reportTaxAmt> { }
		protected Decimal? _ReportTaxAmt;

		/// <summary>
		/// The tax amount in the tax agency currency.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal]
		[PXUIField(DisplayName = "Report Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReportTaxAmt
		{
			get
			{
				return this._ReportTaxAmt;
			}
			set
			{
				this._ReportTaxAmt = value;
			}
		}
		#endregion
		#region NonDeductibleTaxRate
		public abstract class nonDeductibleTaxRate : PX.Data.BQL.BqlDecimal.Field<nonDeductibleTaxRate> { }

		/// <summary>
		/// The deductible tax rate from the related <see cref="TaxRev"/> record.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deductible Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override Decimal? NonDeductibleTaxRate { get; set; }
		#endregion
		#region ExpenseAmt
		public abstract class expenseAmt : PX.Data.BQL.BqlDecimal.Field<expenseAmt> { }

		/// <summary>
		/// The expense amount in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? ExpenseAmt { get; set; }
		#endregion
		#region CuryExpenseAmt
		public abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }

		/// <summary>
		/// The expense amount in the record currency.
		/// </summary>
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.expenseAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override Decimal? CuryExpenseAmt { get; set; }
		#endregion

		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
        protected DateTime? _CuryEffDate;

		/// <summary>
		/// The effective date of the suitable <see cref="CurrencyRate"/> record.
		/// The field is filled in only during preparation of the tax report.
		/// </summary>
		[PXDate()]
        public virtual DateTime? CuryEffDate
        {
            get
            {
                return this._CuryEffDate;
            }
            set
            {
                this._CuryEffDate = value;
            }
        }
		#endregion


		#region AdjdDocType
		public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		protected String _AdjdDocType;

		/// <summary>
		/// Link to <see cref="APPayment"/> (Check) application. Used for withholding taxes.
		/// </summary>
		[PXDBString(3)]
		public virtual String AdjdDocType
		{
			get
			{
				return this._AdjdDocType;
			}
			set
			{
				this._AdjdDocType = value;
			}
		}
		#endregion
		#region AdjdRefNbr
		public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
		protected String _AdjdRefNbr;

		/// <summary>
		/// Link to <see cref="APPayment"/> (Check) application. Used for withholding taxes.
		/// </summary>
		[PXDBString(15)]
		public virtual String AdjdRefNbr
		{
			get
			{
				return this._AdjdRefNbr;
			}
			set
			{
				this._AdjdRefNbr = value;
			}
		}
		#endregion
		#region AdjNbr
		public abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
		protected Int32? _AdjNbr;

		/// <summary>
		/// Link to <see cref="APPayment"/> (Check) application. Used for withholding taxes.
		/// </summary>
		[PXDBInt]
		public virtual Int32? AdjNbr
		{
			get
			{
				return this._AdjNbr;
			}
			set
			{
				this._AdjNbr = value;
			}
		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;

		/// <summary>
		/// The description of the transaction.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
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

		#region CuryRetainedTaxableAmt
		public abstract class curyRetainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxableAmt> { }
		protected decimal? _CuryRetainedTaxableAmt;

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.retainedTaxableAmt))]
		[PXUIField(DisplayName = "Retained Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryRetainedTaxableAmt
		{
			get
			{
				return _CuryRetainedTaxableAmt;
			}
			set
			{
				_CuryRetainedTaxableAmt = value;
			}
		}
		#endregion
		#region RetainedTaxableAmt
		public abstract class retainedTaxableAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxableAmt> { }
		protected decimal? _RetainedTaxableAmt;

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Retained Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? RetainedTaxableAmt
		{
			get
			{
				return _RetainedTaxableAmt;
			}
			set
			{
				_RetainedTaxableAmt  = value;
			}
		}
		#endregion
		#region CuryRetainedTaxAmt
		public abstract class curyRetainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxAmt> { }
		protected decimal? _CuryRetainedTaxAmt;

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.retainedTaxAmt))]
		[PXUIField(DisplayName = "Retained Tax", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? CuryRetainedTaxAmt
		{
			get
			{
				return _CuryRetainedTaxAmt;
			}
			set
			{
				_CuryRetainedTaxAmt = value;
			}
		}
		#endregion
		#region RetainedTaxAmt
		public abstract class retainedTaxAmt : PX.Data.BQL.BqlDecimal.Field<retainedTaxAmt> { }
		protected decimal? _RetainedTaxAmt;

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Retained Tax", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainedTaxAmt
		{
			get
			{
				return _RetainedTaxAmt;
			}
			set
			{
				_RetainedTaxAmt = value;
			}
		}
		#endregion
		#region CuryRetainedTaxAmtSumm
		public abstract class curyRetainedTaxAmtSumm : PX.Data.BQL.BqlDecimal.Field<curyRetainedTaxAmtSumm> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(TaxTran.curyInfoID), typeof(TaxTran.retainedTaxAmtSumm))]
		public virtual decimal? CuryRetainedTaxAmtSumm { get; set; }
		#endregion
		#region RetainedTaxAmtSumm
		public abstract class retainedTaxAmtSumm : PX.Data.BQL.BqlDecimal.Field<retainedTaxAmtSumm> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(4)]
		public virtual decimal? RetainedTaxAmtSumm { get; set; }
		#endregion

		public static string GetKeyImage(string module, int? recordID, DateTime? tranDate)
		{
			return String.Format("{0}:{1}, {2}:{3}, {4}:{5}", typeof(TaxTran.module).Name, module,
																typeof(TaxTran.recordID).Name, recordID,
																typeof(TaxTran.tranDate).Name, tranDate);
		}

		public string GetKeyImage()
		{
			return GetKeyImage(Module, RecordID, TranDate);
		}

		public static string GetImage( string module, int? recordID, DateTime? tranDate)
		{
			return string.Format("{0}[{1}]", 
								EntityHelper.GetFriendlyEntityName(typeof(TaxTran)),
								GetKeyImage(module, recordID, tranDate));
		}

		public override string ToString()
		{
			return GetImage(Module, RecordID, TranDate);
		}
	}
}