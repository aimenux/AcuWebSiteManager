using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;


namespace PX.Objects.CA
{
    /// <summary>
    /// Contains the main properties of CA transaction details.
    /// CA transaction details are edited on the Transactions (CA304000) form (which corresponds to the <see cref="CATranEntry"/> graph).
    /// </summary>
    [Serializable]
	[PXCacheName(Messages.CASplit)]
	public partial class CASplit : IBqlTable, IDocumentTran, ICATranDetail
	{
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        /// <summary>
        /// The identifier of the branch of the parent document.
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="CAAdj.branchID"/> field.
        /// </value>
        [Branch(typeof(CAAdj.branchID))]
        public virtual int? BranchID
        {
            get;
            set;
        }
        #endregion
        #region AdjRefNbr
		public abstract class adjRefNbr : PX.Data.BQL.BqlString.Field<adjRefNbr> { }

        /// <summary>
        /// The reference number of the parent document.
        /// This field is a part of the compound key of the document.
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="CAAdj.AdjRefNbr"/> field.
        /// </value>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CAAdj.adjRefNbr))]
		[PXParent(typeof(Select<CAAdj, Where<CAAdj.adjTranType, Equal<Current<CASplit.adjTranType>>, And<CAAdj.adjRefNbr, Equal<Current<CASplit.adjRefNbr>>>>>))]
		[PXUIField(Visible = false)]
		public virtual string AdjRefNbr
		{
			get;
			set;
		}
		#endregion
		#region AdjTranType
		public abstract class adjTranType : PX.Data.BQL.BqlString.Field<adjTranType> { }

        /// <summary>
        /// The type of the parent document.
        /// This field is a part of the compound key of the document.
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="CAAdj.AdjTranType"/> field.
        /// </value>
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(CAAdj.adjTranType))]
		[PXUIField(DisplayName = "Type", Visible = false)]
		public virtual string AdjTranType
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        /// <summary>
        /// The number of the line in details of the <see cref="CAAdj"/> document.
        /// The value of this field affects the <see cref="CAAdj.LineCntr">counter</see> of the parent document.
        /// </summary>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(CAAdj.lineCntr))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
        #region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

        /// <summary>
        /// The account to be updated by the transaction.
        /// By default, this is the offset account that is defined by the entry type selected for the cash account.
        /// </summary>
		[Account(typeof(CASplit.branchID), DisplayName = "Offset Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        /// <summary>
        /// The subaccount to be used for the transaction.
        /// </summary>
		[SubAccount(typeof(CASplit.accountID), typeof(CASplit.branchID), DisplayName = "Offset Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault]
		public virtual int? SubID
		{
			get;
			set;
		}
		#endregion
		#region ReclassificationProhibited
		public abstract class reclassificationProhibited : PX.Data.BQL.BqlBool.Field<reclassificationProhibited> { }

		/// <summary>
		/// It is used only to pass ReclassificationProhibited flag to GL tran on Cash-in-Transit Account.
		/// It is not persisted.
		/// </summary>
		[PXBool]
		public virtual bool? ReclassificationProhibited
		{
			get;
			set;
		}
		#endregion
        #region CashAccountID
        public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

        /// <summary>
        /// The cash account of the line.
        /// This is a virtual field and it has no representation in the database.
        /// </summary>
        [PXRestrictor(typeof(Where<CashAccount.branchID, Equal<Current<CASplit.branchID>>>), Messages.CashAccountNotMatchBranch)]
        [PXRestrictor(typeof(Where<CashAccount.curyID, Equal<Current<CAAdj.curyID>>>), Messages.OffsetAccountForThisEntryTypeMustBeInSameCurrency)]
        [CashAccountScalar(DisplayName = "Offset Cash Account", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr))]
        [PXDBScalar(typeof(Search<CashAccount.cashAccountID, Where<CashAccount.accountID, Equal<CASplit.accountID>,
                                   And<CashAccount.subID, Equal<CASplit.subID>, And<CashAccount.branchID, Equal<CASplit.branchID>>>>>))]
        public virtual int? CashAccountID
        {
            get;
            set;
        }
        #endregion
        #region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

        /// <summary>
        /// The tax category that applies to the transaction.
        /// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[CATax(typeof(CAAdj), typeof(CATax), typeof(CATaxTran), typeof(CAAdj.taxCalcMode), parentBranchIDField: typeof(CAAdj.branchID),
		    CuryOrigDocAmt = typeof(CAAdj.curyControlAmt), DocCuryTaxAmt = typeof(CAAdj.curyTaxAmt))]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
									Where<InventoryItem.inventoryID, Equal<Current<CASplit.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string TaxCategoryID
		{
			get;
			set;
		}
        #endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        /// <summary>
        /// The description provided for the item.
        /// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string TranDesc
		{
			get;
			set;
		}

        /// <summary>
        /// This field implements the member of the <see cref="IDocumentTran"/> interface.
        /// </summary>
	    public DateTime? TranDate
	    {
	        get
	        {
	            return null;
	        }

	        set
	        {
	        }
	    }

		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        /// <summary>
        /// The identifier of the exchange rate record for the line.
        /// Corresponds to the <see cref="CAAdj.CuryInfoID"/> field.
        /// </summary>
		[PXDBLong]
		[CurrencyInfo(typeof(CAAdj.curyInfoID))]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

        /// <summary>
        /// The total amount of this line in the selected currency.
        /// </summary>
		[PXDBCurrency(typeof(CASplit.curyInfoID), typeof(CASplit.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		public virtual decimal? CuryTranAmt
		{
			get;
			set;
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

        /// <summary>
        /// The total amount this line in the base currency.
        /// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tran. Amount")]
		[PXFormula(null, typeof(SumCalc<CAAdj.tranAmt>))]
		[PXFormula(null, typeof(SumCalc<CAAdj.splitTotal>))]
		public virtual decimal? TranAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }

        /// <summary>
        /// The line total that is subjected to all taxes with calculation rule "Inclusive Line-Level" (see <see cref="TaxBaseAttribute.TaxSetLineDefault"/>) in the selected currency.
        /// </summary>
		[PXDBCurrency(typeof(CASplit.curyInfoID), typeof(CASplit.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }

        /// <summary>
        /// The line total that is subjected to all taxes with calculation rule "Inclusive Line-Level" (see <see cref="TaxBaseAttribute.TaxSetLineDefault"/>) in the base currency.
        /// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxableAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }

		/// <summary>
		/// The amount of tax (VAT) associated with the line in the selected currency.
		/// </summary>
		[PXDBCurrency(typeof(CAAdj.curyInfoID), typeof(CAAdj.taxAmt))]
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
        /// The amount of tax (VAT) associated with the line in the base currency.
        /// </summary>
        [PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxAmt
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        /// <summary>
        /// The user-friendly identifier of the non-stock item specified as the transaction subject.
        /// </summary>
        /// <value>
        /// Corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
        /// </value>
		[NonStockNonKitItem]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
		[PXUIField(DisplayName = "Item ID")]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : IBqlField { }

		[FinPeriodID(
			branchSourceType: typeof(branchID),
			masterFinPeriodIDType: typeof(tranPeriodID),
			headerMasterFinPeriodIDType: typeof(CAAdj.tranPeriodID))]
		public virtual string FinPeriodID { get; set; }
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : IBqlField { }

		[PeriodID]
		public virtual string TranPeriodID { get; set; }
		#endregion

		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

		/// <summary>
		/// The <see cref="PX.Objects.IN.INUnit">unit of measure</see> for the transaction.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Objects.IN.INUnit.FromUnit">INUnit.FromUnit</see> field.
		/// </value>
		[PXDefault(typeof(Coalesce<
			Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<CASplit.inventoryID>>, And<Current<CAAdj.drCr>, Equal<CADrCr.cACredit>>>>,
			Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<CASplit.inventoryID>>, And<Current<CAAdj.drCr>, Equal<CADrCr.cADebit>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(CASplit.inventoryID))]
		public virtual string UOM
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        /// <summary>
        /// The quantity of the item.
        /// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "1.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual decimal? Qty
		{
			get;
			set;
		}
		#endregion
		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

		/// <summary>
		/// The unit price for the item in the base currency.
		/// </summary>
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? UnitPrice
		{
			get;
			set;
		}
		#endregion

		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }

        /// <summary>
        /// The unit price for the item in the selected currency.
        /// </summary>
		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(CASplit.curyInfoID), typeof(CASplit.unitPrice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Price")]
		public virtual decimal? CuryUnitPrice
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        /// <summary>
        /// The project with which this transaction is associated, or the code indicating that this transaction is not associated with any project;
        /// the non-project code is specified on the Projects Preferences (PM101000) form.
        /// This field appears in the UI only if the Projects module has been enabled in your system and integrated with the Cash Management module.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PMProject"/>.
        /// </value>
		[ProjectDefault(BatchModule.CA)]
		[CAActiveProject]
        public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

		/// <summary>
		/// The particular task of the project with which this transaction is associated.
		/// This field appears in the UI only if the Projects module has been enabled in your system and integrated with the Cash Management module.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="PMTask.TaskID"/> field.
		/// </value>
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveProjectTask(typeof(CASplit.projectID), BatchModule.CA, DisplayName = "Project Task")]
		public virtual int? TaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[CostCode(null, typeof(taskID), null)]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region NonBillable
		public abstract class nonBillable : PX.Data.BQL.BqlBool.Field<nonBillable> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that this transaction is non-billable in the project.
        /// This column appears only if the Projects module has been enabled in your system and integrated with the Cash Management module.
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Non Billable", FieldClass = ProjectAttribute.DimensionName)]
        public virtual bool? NonBillable
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

		#region IDocumentTran
		public string TranType
		{
		    get
		    {
		        return AdjTranType;
		    }

		    set
		    {
		        AdjTranType = value;
		    }
		}

		public string RefNbr
		{
		    get
		    {
		        return AdjRefNbr;
		    }

		    set
		    {
		        AdjRefNbr = value;
		    }
		}

		public decimal? CuryCashDiscBal { get; set; }
		public decimal? CashDiscBal { get; set; }
		public decimal? CuryTranBal { get; set; }
		public decimal? TranBal { get; set; }

		#endregion
	}
}
