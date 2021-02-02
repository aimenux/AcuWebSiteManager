using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.TM;
using IRegister = PX.Objects.CM.IRegister;

namespace PX.Objects.CA
{
    /// <summary>
    /// Contains the main properties of CA transactions.
    /// CA transaction are edited on the Transactions (CA304000) form (which corresponds to the <see cref="CATranEntry"/> graph).
    /// </summary>
    [Serializable]
	[PXPrimaryGraph(typeof(CATranEntry))]
	[PXCacheName(Messages.CashTransactions)]
	[PXEMailSource]
	public partial class CAAdj : IBqlTable, ICADocument, IAssign, IRegister
	{
        /// <summary>
        /// The type of the cash transaction.
        /// Returns the value of the <see cref="AdjTranType"/> field.
        /// This field implements the ICADocument interface member.
        /// </summary>
        public string DocType
		{
		    get
		    {
		        return this.AdjTranType;
		    }

		    set
		    {
		        AdjTranType = value;
		    }
		}

        /// <summary>
        /// The user-friendly unique identifier assigned to the cash transaction in accordance with the numbering sequence.
        /// Returns the value of the <see cref="AdjRefNbr"/> field.
        /// This field implements the ICADocument interface member.
        /// </summary>
		public string RefNbr
		{
		    get
		    {
		        return this.AdjRefNbr;
		    }

		    set
		    {
		        AdjRefNbr = value;
		    }
		}

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        /// <summary>
        /// The branch of the cash transaction.
        /// </summary>
		[Branch(typeof(Search<CashAccount.branchID, Where<CashAccount.cashAccountID, Equal<Current<cashAccountID>>>>))]
		[PXFormula(typeof(Default<cashAccountID>))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region AdjRefNbr
		public abstract class adjRefNbr : PX.Data.BQL.BqlString.Field<adjRefNbr> { }

        /// <summary>
        /// The user-friendly unique identifier assigned to the cash transaction in accordance with the numbering sequence.
        /// This field is the key field.
        /// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CAAdj.adjRefNbr, Where<CAAdj.draft, Equal<False>>>),
			typeof(adjRefNbr), typeof(extRefNbr), typeof(cashAccountID), typeof(tranDate), 
			typeof(tranDesc), typeof(employeeID), typeof(entryTypeID), typeof(status), typeof(curyID), typeof(curyTranAmt))]
		[AutoNumber(typeof(CASetup.registerNumberingID), typeof(CAAdj.tranDate))]
		public virtual string AdjRefNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjTranType
		public abstract class adjTranType : PX.Data.BQL.BqlString.Field<adjTranType> { }

        /// <summary>
        /// The type of the cash transaction.
        /// On the Transactions (CA304000) form, transactions of the only Cash Entry type are created.
        /// </summary>
        /// <value>
        /// The field can have one of the following values:
        /// <c>"CTE"</c>: Expense entry,
        /// <c>"CAE"</c>: Cash entry.
        /// </value>
        [PXDBString(3, IsFixed = true)]
		[CATranType.List]
		[PXDefault(CATranType.CAAdjustment)]
		[PXUIField(DisplayName = "Tran. Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string AdjTranType
		{
			get;
			set;
		}
		#endregion
		#region TransferNbr
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }

		/// <summary>
		/// The unique identifier of the cash transfer.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CATransfer.TransferNbr"/> field.
		/// The value of the field can be empty.
		/// If the value of the field is not empty, this cash transaction is an expense entry (<see cref="AdjTranType"/> is <c>"CTE"</c>), which was created on the Funds Transfers (CA301000) form.
		/// Then this cash transaction is an expense entry from the form Funds Transfer (CA301000).
		/// </value>
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		[PXDBString(15, IsUnicode = true)]
		public virtual string TransferNbr
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }

        /// <summary>
        /// The reference number of the external document.
        /// </summary>
		[PXDBString(40, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Document Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

        /// <summary>
        /// The cash account that is the source account for the transaction.
        /// </summary>
		[PXDefault]
		[CashAccount(null, typeof(Search<CashAccount.cashAccountID, Where2<Match<Current<AccessInfo.userName>>, And<CashAccount.clearingAccount, Equal<CS.boolFalse>>>>), Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        /// <summary>
        /// The date of the transaction.
        /// </summary>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Tran. Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? TranDate
		{
		    get;
			set;
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }

        /// <summary>
        /// The basic type of the transaction: Receipt or Disbursement.
        /// </summary>
		[PXDefault(CADrCr.CADebit)]
		[PXDBString(1, IsFixed = true)]
		[CADrCr.List]
		[PXUIField(DisplayName = "Disbursement/Receipt", Enabled = false)]
		public virtual string DrCr
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        /// <summary>
        /// The description of the transaction.
        /// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual string TranDesc
		{
			get;
			set;
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }

		/// <summary>
		/// <see cref="PX.Objects.GL.FinPeriods.OrganizationFinPeriod">financial period</see> of the document.
		/// </summary>
		/// <value>
		/// Is determined by the <see cref="TranDate">date of the cash transaction</see>. 
		/// Unlike <see cref="FinPeriodID"/> the value of this field can't be overridden by a user.
		/// </value>
		[PeriodID]
		public virtual string TranPeriodID
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		/// <summary>
		/// <see cref="PX.Objects.GL.FinPeriods.OrganizationFinPeriod">financial period</see> of the document.
		/// </summary>
		/// <value>
		/// Defaults to the period to which the <see cref="APRegister.DocDate"/> belongs, but can be overridden by a user.
		/// </value>
		[CAOpenPeriod(
			typeof(tranDate),
			typeof(cashAccountID),
			typeof(Selector<cashAccountID, CashAccount.branchID>),
			masterFinPeriodIDType: typeof(tranPeriodID),
			IsHeader = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Fin. Period")]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

        /// <summary>
        /// The tax zone that applies to the transaction.
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="TaxZone.TaxZoneID"/> field.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXDefault(typeof(Search<CashAccountETDetail.taxZoneID, Where<CashAccountETDetail.accountID, Equal<Current<CAAdj.cashAccountID>>, And<CashAccountETDetail.entryTypeID, Equal<Current<CAAdj.entryTypeID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string TaxZoneID
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        /// <summary>
        /// The identifier of the exchange rate record for the deposit.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyInfo.CuryInfoID"/> field.
        /// </value>
		[PXDBLong]
		[CurrencyInfo]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region Draft
		public abstract class draft : PX.Data.BQL.BqlBool.Field<draft> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the adjustment was created on the Journal Vouchers (GL304000) form
        /// and the parent <see cref="GLDocBatch"/> record of the adjustment has the <c>"On Hold"</c> status.
        /// </summary>
        [PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Draft")]		
		public virtual bool? Draft
		{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the cash transaction is on hold, 
        /// which means that it can be edited but cannot be released.
        /// </summary>
        [PXDBBool]
		[PXDefault(typeof(Search<CASetup.holdEntry>))]
		[PXUIField(DisplayName = "Hold")]
		[PXNoUpdate]
		public virtual bool? Hold
		{
			get;
			set;
		}
		#endregion
		#region RequestApproval
		public abstract class requestApproval : PX.Data.BQL.BqlBool.Field<requestApproval> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the cash transaction must be approved before it can be released.
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="CASetup.RequestApproval"/> field.
        /// </value>
		[PXBool]		
		[PXDBScalar(typeof(Search<CASetup.requestApproval>))]		
		[PXDefault(typeof(Search<CASetup.requestApproval>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? RequestApproval
		{
			get;
			set;
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the transaction has been approved by a responsible person.
        /// This field is displayed if the <see cref="CASetup.RequestApproval"/> field is set to <c>true</c>.
        /// </summary>
        [PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Approved", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual bool? Approved
		{
			get;
			set;
		}
		#endregion
		#region Rejected
		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the transaction has been rejected by a responsible person.
        /// When the transaction has been rejected, its status changes to On Hold.
        /// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Reject", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public bool? Rejected
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the transaction was released.
        /// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion
		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.BQL.BqlBool.Field<isTaxValid> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the transaction's tax is validated by the external tax provider.
        /// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is up to date", Enabled = false)]
		public virtual bool? IsTaxValid
		{
			get;
			set;
		}
		#endregion
		#region IsTaxPosted
		public abstract class isTaxPosted : PX.Data.BQL.BqlBool.Field<isTaxPosted> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the transaction's tax is posted or committed to the external tax provider.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is posted/commited to the external Tax Engine(Avalara)", Enabled = false)]
		public virtual bool? IsTaxPosted
		{
			get;
			set;
		}
		#endregion
		#region IsTaxSaved
		public abstract class isTaxSaved : PX.Data.BQL.BqlBool.Field<isTaxSaved> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the transaction's tax is saved in the external tax provider.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax has been saved in the external tax provider", Enabled = false)]
		public virtual bool? IsTaxSaved
		{
			get;
			set;
		}
		#endregion
		#region NonTaxable
		public abstract class nonTaxable : PX.Data.BQL.BqlBool.Field<nonTaxable> { }
		/// <summary>
		/// Get or set NonTaxable that mark current document does not impose sales taxes.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Non-Taxable", Enabled = false)]
		public virtual Boolean? NonTaxable
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

        /// <summary>
        /// The total amount of tax paid on the document in the selected currency.
        /// </summary>
		[PXDBCurrency(typeof(CAAdj.curyInfoID), typeof(CAAdj.taxTotal))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }

        /// <summary>
        /// The total amount of tax paid on the document in the base currency.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxTotal
		{
			get;
			set;
		}
		#endregion
				
		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }

        /// <summary>
        /// The document total that is exempt from VAT in the selected currency. 
        /// This total is calculated as the taxable amount for the tax 
        /// with the <see cref="Tax.ExemptTax"/> field set to <c>true</c> (that is, the Include in VAT Exempt Total check box selected on the Taxes (TX205000) form).
        /// </summary>
		[PXDBCurrency(typeof(CAAdj.curyInfoID), typeof(CAAdj.vatExemptTotal))]
		[PXUIField(DisplayName = "VAT Exempt Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryVatExemptTotal
		{
			get;
			set;
		}
		#endregion

		#region VatExemptTaxTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }

        /// <summary>
        /// The document total that is exempt from VAT in the base currency. 
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? VatExemptTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }

        /// <summary>
        /// The document total that is subjected to VAT in the selected currency.
        /// The field is displayed only if 
        /// the <see cref="Tax.IncludeInTaxable"/> field is set to <c>true</c> (that is, the Include in VAT Exempt Total check box is selected on the Taxes (TX205000) form).
        /// </summary>
        [PXDBCurrency(typeof(curyInfoID), typeof(vatTaxableTotal))]
		[PXUIField(DisplayName = "VAT Taxable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryVatTaxableTotal
		{
			get;
			set;
		}
		#endregion

		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }

        /// <summary>
        /// The document total that is subjected to VAT in the base currency.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? VatTaxableTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }

        /// <summary>
        /// The tax amount to be paid for the document in the selected currency.
        /// This field is enable and visible only if the <see cref="CASetup.RequireControlTaxTotal"/> field 
        /// and the <see cref="FeaturesSet.NetGrossEntryMode"/> field are set to <c>true</c>.
        /// </summary>
        [PXDBCurrency(typeof(CAAdj.curyInfoID), typeof(CAAdj.taxAmt))]
	    [PXUIField(DisplayName = "Tax Amount")]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual decimal? CuryTaxAmt
	    {
	        get;
            set;
	    }
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }

        /// <summary>
        /// The tax amount to be paid for the document in the base currency.
        /// </summary>
	    [PXDBDecimal(4, BqlField = typeof(CAAdj.taxAmt))]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual decimal? TaxAmt
	    {
	        get;
            set;
	    }
		#endregion

		#region CurySplitTotal
		public abstract class curySplitTotal : PX.Data.BQL.BqlDecimal.Field<curySplitTotal> { }

        /// <summary>
        /// The sum of amounts of all detail lines in the selected currency.
        /// </summary>
		[PXDBCurrency(typeof(CAAdj.curyInfoID), typeof(CAAdj.splitTotal))]
		[PXUIField(DisplayName = "Detail Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CurySplitTotal
		{
			get;
			set;
		}
		#endregion
		#region SplitTotal
		public abstract class splitTotal : PX.Data.BQL.BqlDecimal.Field<splitTotal> { }

        /// <summary>
        /// The sum of amounts of all detail lines in the base currency.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? SplitTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

        /// <summary>
        /// The amount of the transaction in the selected currency.
        /// </summary>
		[PXDBCurrency(typeof(CAAdj.curyInfoID), typeof(CAAdj.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Enabled = false)]
		public virtual decimal? CuryTranAmt
		{
			get;
			set;
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

        /// <summary>
        /// The amount of the transaction in the base currency.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tran. Amount", Enabled = false)]
		public virtual decimal? TranAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

        /// <summary>
        /// The currency of the cash transaction.
        /// </summary>
        /// <value>
        /// Corresponds to the currency of the <see cref="CashAccount.CuryID"/> cash account.
        /// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXDefault(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<Current<CAAdj.cashAccountID>>>>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryControlAmt
		public abstract class curyControlAmt : PX.Data.BQL.BqlDecimal.Field<curyControlAmt> { }

        /// <summary>
        /// The control total of the transaction in the selected currency.
        /// A user enters this amount manually.
        /// This amount should be equal to the <see cref="CurySplitTotal">sum of amounts of all detail lines</see> of the transaction.
        /// </summary>
		[PXDBCurrency(typeof(CAAdj.curyInfoID), typeof(CAAdj.controlAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Total")]
		public virtual decimal? CuryControlAmt
		{
			get;
			set;
		}
		#endregion
		#region ControlAmt
		public abstract class controlAmt : PX.Data.BQL.BqlDecimal.Field<controlAmt> { }

        /// <summary>
        /// The control total of the transaction in the base currency.
        /// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ControlAmt
		{
			get;
			set;
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        /// <summary>
        /// The user who created the cash transaction.
        /// </summary>
		[PXDBInt]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSubordinateSelector]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? EmployeeID
		{
			get;
			set;
		}
		#endregion		
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

        /// <summary>
        /// The ID of the workgroup which was assigned to approve the transaction.
        /// </summary>
		[PXInt]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID>), SubstituteKey = typeof(EPCompanyTree.description))]
		[PXUIField(DisplayName = "Approval Workgroup ID", Enabled = false)]
		public virtual int? WorkgroupID
		{
			get;
			set;
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

        /// <summary>
        /// The ID of the employee who was assigned to approve the transaction.
        /// </summary>
		[PXGuid]
		[PXOwnerSelector]
		[PXUIField(DisplayName = "Approver", Enabled = false)]
		public virtual Guid? OwnerID
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXSearchable(SM.SearchCategory.CA, "{0} {1}", new Type[] { typeof(CAAdj.adjTranType), typeof(CAAdj.adjRefNbr) },
			new Type[] { typeof(CAAdj.entryTypeID), typeof(CAAdj.tranDesc), typeof(CAAdj.extRefNbr) },
			NumberFields = new Type[] { typeof(CAAdj.adjRefNbr) },
			Line1Format = "{0:d}{1}{2}{4}", Line1Fields = new Type[] { typeof(CAAdj.tranDate), typeof(CAAdj.entryTypeID), typeof(CAAdj.extRefNbr), typeof(CAAdj.ownerID), typeof(BAccount.acctName) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(CAAdj.tranDesc) }
		)]
		[PXNote(new Type[0], DescriptionField = typeof(CAAdj.adjRefNbr))]
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
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

        /// <summary>
        /// The counter of child <see cref="CASplit"/> records.
        /// <c>PXParentAttribute</c> of the <see cref="CASplit.LineNbr"/> field refers to this field.
        /// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LineCntr
		{
			get;
			set;
		}
		#endregion
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }

        /// <summary>
        /// The identifier of the transaction that is recorded to the cash account.
        /// Corresponds to the <see cref="CATran.TranID"/> field.
        /// </summary>
		[PXDBLong]
		[AdjCashTranID]
		[PXSelector(typeof(Search<CATran.tranID>), DescriptionField = typeof(CATran.batchNbr))]
		public virtual long? TranID
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

        /// <summary>
        /// The status of the cash transaction.
        /// </summary>
        /// <value>
        /// The field can have one of the values described in <see cref="CATransferStatus.ListAttribute"/>.
        /// </value>
        [PXDBString(1, IsFixed = true)]
		[PXDefault(CATransferStatus.Balanced, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[CATransferStatus.List]
		[SetStatus]
		[PXDependsOnFields(
			typeof(CAAdj.hold),
			typeof(CAAdj.approved),
			typeof(CAAdj.requestApproval),
			typeof(CAAdj.rejected),
			typeof(CAAdj.released))]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion
		#region EntryTypeID
		public abstract class entryTypeID : PX.Data.BQL.BqlString.Field<entryTypeID> { }

        /// <summary>
        /// The user-defined transaction type. 
        /// Selects the appropriate type from the <see cref="CAEntryType">list of entry types</see> defined for the selected cash account.
        /// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId, InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>, 
									Where<CashAccountETDetail.accountID, Equal<Current<CAAdj.cashAccountID>>, And<CAEntryType.module, Equal<BatchModule.moduleCA>>>>), DescriptionField = typeof(CAEntryType.descr))]
		[PXUIField(DisplayName = "Entry Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual string EntryTypeID
		{
			get;
			set;
		}
		#endregion
		#region PaymentsReclassification
		public abstract class paymentsReclassification : PX.Data.BQL.BqlBool.Field<paymentsReclassification> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that this transaction is used for payments reclassification.
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="CAEntryType.UseToReclassifyPayments"/> field of the selected <see cref="CAEntryType">entry type</see>.
        /// </value>
		[PXBool]
		[PXDBScalar(typeof(Search<CAEntryType.useToReclassifyPayments, Where<CAEntryType.entryTypeId, Equal<CAAdj.entryTypeID>>>))]
		public virtual bool? PaymentsReclassification
		{
			get;
			set;
		}
		#endregion
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the transaction was cleared.
        /// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cleared")]
		public virtual bool? Cleared
		{
			get;
			set;
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }

        /// <summary>
        /// The date when the transaction was cleared.
        /// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Clear Date")]
		public virtual DateTime? ClearDate
		{
			get;
			set;
		}
        #endregion
        #region TranID_CATran_batchNbr
        /// <summary>
        /// The number of the batch generated to implement the cash transaction. 
        /// The number appears automatically after the transaction is released. 
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="CATran.batchNbr"/> field.
        /// The value is filled in by the <see cref="CATranEntry.CAAdj_TranID_CATran_BatchNbr_FieldSelecting"/> method.
        /// </value>
        public abstract class tranID_CATran_batchNbr : PX.Data.BQL.BqlString.Field<tranID_CATran_batchNbr> { }
		#endregion

		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }

        /// <summary>
        /// The tax calculation mode, which defines which amounts (tax-inclusive or tax-exclusive) 
        /// should be entered in the detail lines of a document. 
        /// This field is displayed only if the <see cref="FeaturesSet.NetGrossEntryMode"/> field is set to <c>true</c>.
        /// </summary>
        /// <value>
        /// The field can have one of the following values:
        /// <c>"T"</c> (Tax Settings): The tax amount for the document is calculated according to the settings of the applicable tax or taxes.
        /// <c>"G"</c> (Gross): The amount in the document detail line includes a tax or taxes.
        /// <c>"N"</c> (Net): The amount in the document detail line does not include taxes.
        /// </value>
        [PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<CashAccountETDetail.taxCalcMode, 
			Where<CashAccountETDetail.accountID, Equal<Current<CAAdj.cashAccountID>>,
				And<CashAccountETDetail.entryTypeID, Equal<Current<CAAdj.entryTypeID>>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxRoundDiff
		public abstract class curyTaxRoundDiff : PX.Data.BQL.BqlDecimal.Field<curyTaxRoundDiff> { }

        /// <summary>
        /// The difference between the original document amount and the rounded amount in the selected currency.
        /// </summary>
		[PXDBCurrency(typeof(CAAdj.curyInfoID), typeof(CAAdj.taxRoundDiff), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Rounding Diff.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public decimal? CuryTaxRoundDiff
		{
			get;
			set;
		}
		#endregion
		#region TaxRoundDiff
		public abstract class taxRoundDiff : PX.Data.BQL.BqlDecimal.Field<taxRoundDiff> { }

        /// <summary>
        /// The difference between the original document amount and the rounded amount in the base currency.
        /// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? TaxRoundDiff
		{
			get;
			set;
		}
		#endregion
		#region HasWithHoldTax
		public abstract class hasWithHoldTax : PX.Data.BQL.BqlBool.Field<hasWithHoldTax> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that withholding taxes are applied to the document.
        /// This is a technical field, which is calculated on the fly and is used to restrict the values of the <see cref="TaxCalcMode"/> field.
        /// </summary>
        [PXBool]
		[RestrictWithholdingTaxCalcMode(typeof(CAAdj.taxZoneID), typeof(CAAdj.taxCalcMode))]
		public virtual bool? HasWithHoldTax
		{
			get;
			set;
		}
		#endregion
		#region HasUseTax
		public abstract class hasUseTax : PX.Data.BQL.BqlBool.Field<hasUseTax> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that use taxes are applied to the document.
        /// This is a technical field, which is calculated on the fly and is used to restrict the values of the <see cref="TaxCalcMode"/> field.
        /// </summary>
        [PXBool]
		[RestrictUseTaxCalcMode(typeof(CAAdj.taxZoneID), typeof(CAAdj.taxCalcMode))]
		public virtual bool? HasUseTax
		{
			get;
			set;
		}
		#endregion
		#region UsesManualVAT
		public abstract class usesManualVAT : PX.Data.BQL.BqlBool.Field<usesManualVAT> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the document's tax zone is set up as "Manual VAT Zone".
        /// This is a technical field, which is used to restrict the values of the <see cref="TaxCalcMode"/> field.
        /// </summary>
		[PXDBBool]
		[RestrictManualVAT(typeof(CAAdj.taxZoneID), typeof(CAAdj.taxCalcMode))]
		[PXUIField(DisplayName = "Manual VAT Entry", Enabled = false)]
		public virtual bool? UsesManualVAT
		{
			get;
			set;
		}
		#endregion

		#region IRegister

		public DateTime? DocDate
		{
			get;
			set;
		}

		public string DocDesc
		{
		    get
		    {
		        return TranDesc;
		    }

		    set
		    {
		        TranDesc = value;
		    }
		}

		public string OrigModule
		{
		    get
		    {
		        return BatchModule.CA;
		    }

		    set
		    {
		    }
		}

	    public decimal? CuryOrigDocAmt
	    {
	        get
	        {
	            return CuryTranAmt;
	        }

	        set
	        {
	            CuryTranAmt = value;
	        }
	    }

	    public decimal? OrigDocAmt
	    {
            get
            {
                return TranAmt;
            }

            set
            {
                TranAmt = value;
            }
        }

		#endregion
	}

	public class SetStatusAttribute : PXEventSubscriberAttribute,
		IPXRowSelectingSubscriber, IPXRowInsertingSubscriber, IPXRowUpdatingSubscriber, IPXRowSelectedSubscriber
	{
        public static void UpdateStatus(PXCache cache, CAAdj row)
        {
            foreach (var attr in cache.GetAttributes<CAAdj.status>(row))
            {
                if (attr is SetStatusAttribute)
                {
                    (attr as SetStatusAttribute).UpdateStatus(cache, row, row.Hold);
                }
            }
        }

        public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.AddHandler<CAAdj.hold>((cache, e) =>
			{
				PXBoolAttribute.ConvertValue(e);

				CAAdj item = e.Row as CAAdj;
				if (item != null)
				{
					UpdateStatus(cache, item, (bool?)e.NewValue);
				}
			});

			sender.Graph.FieldVerifying.AddHandler<CAAdj.status>((cache, e) => { e.NewValue = cache.GetValue<CAAdj.status>(e.Row); });
		}

		public void UpdateStatus(PXCache cache, CAAdj row, bool? isHold)
		{
			if (isHold == true)
			{
				cache.SetValue(row, _FieldOrdinal, CATransferStatus.Hold);
			}
			else if (row.Approved == false && row.RequestApproval == true)
			{
				cache.SetValue(row, _FieldOrdinal, CATransferStatus.Pending);
			}
			else if (row.Rejected == true)
			{
				cache.SetValue(row, _FieldOrdinal, CATransferStatus.Rejected);
			}
			else if (row.Released == true)
			{
				cache.SetValue(row, _FieldOrdinal, CATransferStatus.Released);
			}
			else
			{
				cache.SetValue(row, _FieldOrdinal, CATransferStatus.Balanced);
			}
		}

		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			var item = (CAAdj)e.Row;

		    if (item != null)
		    {
		        UpdateStatus(sender, item, item.Hold);
		    }
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var item = (CAAdj)e.Row;
			UpdateStatus(sender, item, item.Hold);
		}

		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var item = (CAAdj)e.NewRow;
			UpdateStatus(sender, item, item.Hold);
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var item = (CAAdj)e.Row;

		    if (item != null)
		    {
		        UpdateStatus(sender, item, item.Hold);
		    }
		}
	}
}
