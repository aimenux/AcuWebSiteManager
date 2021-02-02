using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.DAC.Abstract;
using PX.Objects.TX;
using PX.SM;
using PX.TM;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.EP
{
	/// <summary>
	/// The current status of the expense claim, which is set by the system.
	/// The fields that determine the status of a document are: 
	/// <see cref="EPExpenseClaim.Hold"/>, <see cref="EPExpenseClaim.Released"/>, 
	/// <see cref="EPExpenseClaim.Approved"/>, <see cref="EPExpenseClaim.Rejected"/>.
	/// </summary>
	/// <value>
	/// The field can have one of the following values:
	/// <c>"H"</c>: The claim is a draft and cannot be released.
	/// <c>"O"</c>: The claim is in the approval process. If the claim is approved, the status changes to Approved.
	/// <c>"A"</c>: The claim has been approved.
	/// <c>"C"</c>: The claim has been rejected by the approver.
	/// <c>"R"</c>: The claim has been released and then an Accounts Payable bill in the employee's name has been generated.
	/// </value>
	public class EPExpenseClaimStatus : ILabelProvider
	{
		public const string ApprovedStatus = "A";
		public const string HoldStatus = "H";
		public const string ReleasedStatus = "R";
		public const string OpenStatus = "O";
		public const string RejectedStatus = "C";

		private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ HoldStatus, Messages.HoldStatus },
			{ OpenStatus, Messages.OpenStatus },
			{ ApprovedStatus, Messages.Approved },
			{ RejectedStatus, Messages.RejectedStatus },
			{ ReleasedStatus, Messages.ReleasedStatus  },
		};

		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;

		public class ListAttribute : LabelListAttribute
		{
			public ListAttribute() : base(_valueLabelPairs)
			{ }
		}
	}

	/// <summary>
	/// Contains the main properties of the claim document, which an employee can use for the reimbursement of expenses he or she incurred on behalf of the company.
	/// Expense claim are edited on Expense Claim (EP301000) form (which corresponds to the <see cref="ExpenseClaimEntry"/> graph).
	/// Expense claims are edited on the Expense Claims (EP301030) form (which corresponds to the <see cref="ExpenseClaimMaint"/> graph) contains the list of all claims.
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(ExpenseClaimEntry))]
	[PXCacheName(Messages.ExpenseClaim)]
	[PXEMailSource]
	public partial class EPExpenseClaim : IBqlTable, PX.Data.EP.IAssign, IAccountable
	{
		public const string DocType = "ECL";

		public class docType : PX.Data.BQL.BqlString.Constant<docType>
		{
			public docType() : base(DocType)
			{

			}
		}

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		
		/// <summary>
		/// The branch of the claim.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="GL.Branch.BranchID"/> field.
		/// </value>
		[Branch(typeof(Search2<
						GL.Branch.branchID,
						InnerJoin<EPEmployee,
							On<GL.Branch.bAccountID, Equal<EPEmployee.parentBAccountID>>>,
						Where<EPEmployee.bAccountID, Equal<Current<employeeID>>>>))]
		
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		
		/// <summary>
		/// The unique reference number of the expense claim document, which the system assigns based on the numbering sequence 
		/// specified for claims on the Time and Expenses Preferences (EP101000) form (which corresponds to the <see cref="EPSetupMaint"/> graph).
		/// This field is the key field.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[EPExpenceClaimSelector]
		[AutoNumber(typeof(EPSetup.claimNumberingID), typeof(EPExpenseClaim.docDate))]
		[PX.Data.EP.PXFieldDescription]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		
		/// <summary>
		/// The identifier of the <see cref="EPEmployee">employee</see> who claims the expenses.
		/// When the claim is released, an Accounts Payable bill will be generated for this employee.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="EPEmployee.bAccountID"/> field.
		/// </value>
		[PXDBInt]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
		[PXSubordinateAndWingmenSelector]
		[PXUIField(DisplayName = "Claimed By", Visibility = PXUIVisibility.SelectorVisible)]
		[PXForeignReference(typeof(Field<employeeID>.IsRelatedTo<BAccount.bAccountID>))]
		public virtual int? EmployeeID
		{
			get;
			set;
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		
		/// <summary>
		/// The workgroup that is responsible for the document approval process.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Approval Workgroup")]
		[PXSelector(typeof(EPCompanyTreeOwner.workGroupID), SubstituteKey = typeof(EPCompanyTreeOwner.description))]
		public virtual int? WorkgroupID
		{
			get;
			set;
		}
		#endregion
		#region ApproverID
		public abstract class approverID : PX.Data.BQL.BqlGuid.Field<approverID> { }
		
		/// <summary>
		/// The identifier of the <see cref="EPEmployee">employee</see> responsible for the document approval process.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPEmployee.PKID"/> field.
		/// </value>
		[PXDBGuid]
		[PXOwnerSelector(typeof(EPExpenseClaim.workgroupID))]
		[PXDefault(typeof(Search<EPCompanyTreeMember.userID,
		 Where<EPCompanyTreeMember.workGroupID, Equal<Current<EPExpenseClaim.workgroupID>>,
			And<EPCompanyTreeMember.isOwner, Equal<boolTrue>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Approver", Visible = false)]
		public virtual Guid? ApproverID
		{
			get;
			set;
		}
		#endregion
		#region ApprovedByID
		public abstract class approvedByID : PX.Data.BQL.BqlGuid.Field<approvedByID> { }
		[PXDBGuid]
		[PXOwnerSelector]
		[PXUIField(DisplayName = "Approved By", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Guid? ApprovedByID
		{
			get;
			set;
		}
		#endregion
		#region DepartmentID
		public abstract class departmentID : PX.Data.BQL.BqlString.Field<departmentID> { }
		
		/// <summary>
		/// The department associated with the expense claim.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="EPEmployee.DepartmentID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<EPEmployee.departmentID, Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaim.employeeID>>>>))]
		[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
		[PXUIField(DisplayName = "Department ID", Enabled = false, Visibility = PXUIVisibility.Visible)]
		public virtual string DepartmentID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		
		/// <summary>
		/// The location ID that is set to the AP bill created as a result of claim release.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="Location.LocationID"/> field.
		/// </value>
		[PXDefault(typeof(Search<EPEmployee.defLocationID, Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaim.employeeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<EPExpenseClaim.employeeID>>>), Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? LocationID
		{
			get;
			set;
		}
		#endregion
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		
		/// <summary>
		/// The date when the claim was entered.
		/// </summary>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get;
			set;
		}
		#endregion
		#region ApproveDate
		public abstract class approveDate : PX.Data.BQL.BqlDateTime.Field<approveDate> { }
		
		/// <summary>
		/// The date when the claim was approved.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Approval Date", Enabled = false)]

		public virtual DateTime? ApproveDate
		{
			get;
			set;
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		
		/// <summary>
		/// The <see cref="PX.Objects.GL.Obsolete.FinPeriod">financial period</see> of the document.
		/// </summary>
		/// <value>
		/// Is determined by the <see cref="EPExpenseClaim.DocDate">date of the document</see>. Unlike <see cref="EPExpenseClaim.FinPeriodID"/>
		/// the value of this field can't be overridden by user.
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
		/// The period to which the AP document should be posted. 
		/// The selected period is copied to the Post Period box on the Bills and Adjustments form (AP301000) (which corresponds to the <see cref="APInvoiceEntry"/> graph) 
		/// for the AP document created upon the release of the expense claim.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="PX.Objects.GL.Obsolete.FinPeriod.FinPeriodID"/> field.
		/// </value>
		[APOpenPeriod(null, typeof(branchID), masterFinPeriodIDType: typeof(tranPeriodID), ValidatePeriod = PeriodValidation.Nothing)]
		[PXFormula(typeof(Switch<Case<Where<EPExpenseClaim.hold, Equal<True>>, Null>, finPeriodID>))]
		[PXUIField(DisplayName = "Post to Period")]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
	
		/// <summary>
		/// A description of the claim.
		/// </summary>
		[PXDBString(Constants.TranDescLength, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string DocDesc
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		
		/// <summary>
		/// The code of the <see cref="PX.Objects.CM.Currency">currency</see> of the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Company.BaseCuryID">company's base currency</see>.
		/// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
	
		/// <summary>
		/// The identifier of the <see cref="PX.Objects.CM.CurrencyInfo">CurrencyInfo</see> object associated with the document.
		/// </summary>
		/// <value>
		/// Is generated automatically, and corresponds to the <see cref="PX.Objects.CM.CurrencyInfo.CurrencyInfoID"/> field.
		/// </value>
		[PXDBLong]
		[CurrencyInfo(ModuleCode = "EP")]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
	
		/// <summary>
		/// The total amount of the claim in the <see cref="CuryID">currency of the document</see>.
		/// The amount is calculated as the sum of the amounts in the <see cref="EPExpenseClaimDetails.CuryTranAmt">Claim Amount</see> column 
		/// of the Expense Claim Details table located on the Expense Claim (EP301000) form for all lines specified for the claim with taxes applied.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(EPExpenseClaim.curyInfoID), typeof(EPExpenseClaim.docBal))]
		[PXUIField(DisplayName = "Claim Total", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryDocBal
		{
			get;
			set;
		}
		#endregion
		#region DocBal
		public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
	
		/// <summary>
		/// The total amount of the claim in the <see cref = "Company.BaseCuryID">base currency of the company</see>.
		/// The amount is calculated as the sum of the amounts in the <see cref="EPExpenseClaimDetails.TranAmt">Claim Amount</see> column
		/// for all lines specified for the claim, with taxes applied.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DocBal
		{
			get;
			set;
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
		
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the expense claim has been approved by a responsible person
		/// and is in the Approved <see cref="EPExpenseClaim.Status">status</see> now.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Approve", Visibility = PXUIVisibility.Visible)]
		public virtual bool? Approved
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
	
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the expense claim was released 
		/// and is in the Released <see cref="EPExpenseClaim.Status">status</see> now.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the expense claim is in the On Hold <see cref="EPExpenseClaim.Status">status</see>,
		/// which means that the claim can be edited but cannot be release.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		public virtual bool? Hold
		{
			get;
			set;
		}
		#endregion
		#region Rejected
		public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
	
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the expense claim has been rejected by a responsible person.
		/// When the claim is rejected, its <see cref="EPExpenseClaim.Status">status</see> changes to Rejected.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Rejected
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
	
		/// <summary>
		/// The status of the expense claim.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="EPExpenseClaimStatus.ListAttribute"/>.
		/// </value>
		[PXDBString(1)]
		[PXDefault(EPExpenseClaimStatus.HoldStatus)]
		[EPExpenseClaimStatus.List]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		
		/// <summary>
		/// The tax zone associated with the branch.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="TaxZone.TaxZoneID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXRestrictor(typeof(Where<TaxZone.isManualVATZone, Equal<False>>), TX.Messages.CantUseManualVAT)]
		[PXDefault(typeof(Coalesce<Search<EPEmployee.receiptAndClaimTaxZoneID,
										Where<EPEmployee.bAccountID, Equal<Current<employeeID>>>>,
									Search2<Location.vTaxZoneID,
										RightJoin<EPEmployee, On<EPEmployee.defLocationID, Equal<Location.locationID>>>,
										Where<EPEmployee.bAccountID, Equal<Current<employeeID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string TaxZoneID
		{
			get;
			set;
		}
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
		[PXDefault(TaxCalculationMode.TaxSetting)]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode
		{
			get;
			set;
		}
        #endregion
        #region HasWithHoldTax
        public abstract class hasWithHoldTax : PX.Data.BQL.BqlBool.Field<hasWithHoldTax> { }
        [PXBool]
        [RestrictWithholdingTaxCalcMode(typeof(taxZoneID), typeof(taxCalcMode))]
        public virtual bool? HasWithHoldTax
        {
            get;
            set;
        }
        #endregion
        #region HasUseTax
        public abstract class hasUseTax : PX.Data.BQL.BqlBool.Field<hasUseTax> { }
        [PXBool]
        [RestrictUseTaxCalcMode(typeof(taxZoneID), typeof(taxCalcMode))]
        public virtual bool? HasUseTax
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
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
		[PXSearchable(SM.SearchCategory.TM, Messages.SearchableTitleExpenseClaim, new Type[] { typeof(EPExpenseClaim.refNbr), typeof(EPExpenseClaim.employeeID), typeof(EPEmployee.acctName) },
				new Type[] { typeof(EPExpenseClaim.docDesc) },
				NumberFields = new Type[] { typeof(EPExpenseClaim.refNbr) },
				Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(EPExpenseClaim.docDate), typeof(EPExpenseClaim.status), typeof(EPExpenseClaim.refNbr) },
				Line2Format = "{0}", Line2Fields = new Type[] { typeof(EPExpenseClaim.docDesc) },
				SelectForFastIndexing = typeof(Select2<EPExpenseClaim, InnerJoin<EPEmployee, On<EPExpenseClaim.employeeID, Equal<EPEmployee.bAccountID>>>>),
				SelectDocumentUser = typeof(Select2<Users,
				InnerJoin<EPEmployee, On<Users.pKID, Equal<EPEmployee.userID>>>,
				Where<EPEmployee.bAccountID, Equal<Current<EPExpenseClaim.employeeID>>>>))]
		[PXNote(DescriptionField = typeof(EPExpenseClaim.refNbr),
			Selector = typeof(EPExpenseClaim.refNbr))]
		public virtual Guid? NoteID
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
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField]
		public bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		
		/// <summary>
		/// The total amount of taxes associated with the document in the <see cref="CuryID">currency of the document</see>.
		/// (Presented in the currency of the document, see <see cref="CuryID"/>)
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(taxTotal))]
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
		/// The total amount of taxes associated with the document in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxRoundDiff
		public abstract class curyTaxRoundDiff : PX.Data.BQL.BqlDecimal.Field<curyTaxRoundDiff> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(taxRoundDiff), BaseCalc = true)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Discrepancy", Enabled = false)]
		public decimal? CuryTaxRoundDiff
		{
			get;
			set;
		}
		#endregion
		#region TaxRoundDiff
		public abstract class taxRoundDiff : PX.Data.BQL.BqlDecimal.Field<taxRoundDiff> { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? TaxRoundDiff
		{
			get;
			set;
		}
		#endregion
		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		[PXCurrency(typeof(EPExpenseClaim.curyInfoID), typeof(EPExpenseClaim.lineTotal))]
		[PXUIField(DisplayName = "Detail Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryLineTotal
		{
			get;
			set;
		}
		#endregion
		#region LineTotal
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? LineTotal
		{
			get;
			set;
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[CustomerActive(DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID
		{
			get;
			set;
		}
		#endregion
		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		[PXDefault(typeof(Search<Customer.defLocationID, Where<Customer.bAccountID, Equal<Current<customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current2<customerID>>>), DescriptionField = typeof(Location.descr))]
		[PXUIEnabled(typeof(Where<Current2<customerID>, IsNotNull>))]
		[PXFormula(typeof(Switch<Case<Where<Current2<customerID>, IsNull>, Null>, Selector<customerID, Customer.defLocationID>>))]
		public virtual int? CustomerLocationID
		{
			get;
			set;
		}
		#endregion
		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }
	
		/// <summary>
		/// The document total (in the <see cref="CuryID">currency of the document</see>) that is exempt from VAT.
		/// This total is calculated as the taxable amount for the tax with the Include in VAT Exempt Total check box selected on the Taxes (TX205000) form. 
		/// This box is available only if the VAT Reporting feature is enabled on the Enable/Disable Features (CS100000) form (which corresponds to the <see cref="FeaturesMaint"/> graph).
		/// (Presented in the currency of the document, see <see cref="CuryID"/>)
		/// </summary>
		[PXDBCurrency(typeof(EPExpenseClaim.curyInfoID), typeof(EPExpenseClaim.vatExemptTotal))]
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
		/// The document total (in the <see cref="Company.BaseCuryID">base currency of the company</see>) that is exempt from VAT.
		/// This total is calculated as the taxable amount for the tax with the Include in VAT Exempt Total check box selected on the Taxes form. 
		/// This box is available only if the VAT Reporting feature is enabled on the Enable/Disable Features (CS100000) form (which corresponds to the <see cref="FeaturesMaint"/> graph).
		/// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
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
		/// The document total (in the <see cref="CuryID">currency of the document</see>) that is subject to VAT.
		/// This box is available only if the VAT Reporting feature is enabled on the Enable/Disable Features (CS100000) form (which corresponds to the <see cref="FeaturesMaint"/> graph). 
		/// The VAT taxable amount is displayed in this box only if the Include in VAT Taxable Total check box 
		/// is selected for the applicable tax on the Taxes (TX205000) form (which corresponds to the <see cref="SalesTaxMaint"/> graph). If the check box is cleared, this box will be empty.
		/// (Presented in the currency of the document, see <see cref="CuryID"/>)
		/// </summary>
		[PXDBCurrency(typeof(EPExpenseClaim.curyInfoID), typeof(EPExpenseClaim.vatTaxableTotal))]
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
		/// The document total (in the <see cref="Company.BaseCuryID">base currency of the company</see>) that is subject to VAT.
		/// This box is available only if the VAT Reporting feature is enabled on the Enable/Disable Features (CS100000) form (which corresponds to the <see cref="FeaturesMaint"/> graph). 
		/// The VAT taxable amount is displayed in this box only if the Include in VAT Taxable Total check box 
		/// is selected for the applicable tax on the Taxes (TX205000) form (which corresponds to the <see cref="SalesTaxMaint"/> graph). If the check box is cleared, this box will be empty.
		/// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? VatTaxableTotal
		{
			get;
			set;
		}
		#endregion
		#region IAssign Members

		public int? ID
		{
			get
			{
				return null;
			}
		}


		public string EntityType
		{
			get
			{
				return null;
			}
		}
		
		public abstract class ownerID : IBqlField { }

		/// <summary>
		/// The <see cref="EPEmployee">employee</see> responsible 
		/// for the document approval process.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPEmployee.PKID"/> field.
		/// </value>
		public Guid? OwnerID
		{
			get
			{
				return ApproverID;
			}

			set
			{
				ApproverID = value;
			}
		}
		#endregion
	}
}