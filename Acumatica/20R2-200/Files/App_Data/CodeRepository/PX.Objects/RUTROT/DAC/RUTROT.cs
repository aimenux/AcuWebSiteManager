using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.SO;


namespace PX.Objects.RUTROT
{
	[Serializable]
    public class RUTROT : IBqlTable
	{
        #region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		/// <summary>
		/// The type of the document RUTROT item is related to.
		/// </summary>
		/// <value>
		/// Correspond to <see cref="ARInvoice.DocType"/> or <see cref="SOOrder.OrderType"/>.
		/// </value>
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(ARInvoice.docType))]
		[PXParent(typeof(Select<ARInvoice, Where<ARInvoice.docType, Equal<Current<RUTROT.docType>>,
			And<ARInvoice.refNbr, Equal<Current<RUTROT.refNbr>>>>>))]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		/// <summary>
		/// The reference number of the document RUTROT item is related to.
		/// </summary>
		/// <value>
		/// Correspond to <see cref="ARInvoice.RefNbr"/> or <see cref="SOOrder.OrderNbr"/>.
		/// </value>
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDBDefault(typeof(ARInvoice.refNbr))]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region RUTROTType
		public abstract class rUTROTType : PX.Data.BQL.BqlString.Field<rUTROTType> { }

		/// <summary>
		/// The type of deduction for a ROT and RUT deductible document.
		/// </summary>
		/// <value>
		/// Allowed values are:
		/// <c>"U"</c> - RUT,
		/// <c>"O"</c> - ROT.
		/// Defaults to <see cref="BranchRUTROT.defaultRUTROTType"/>.
		/// </value>
		[PXDBString(1)]
		[RUTROTTypes.List]
		[PXDefault(typeof(Search<BranchRUTROT.defaultRUTROTType, Where<Branch.branchID, Equal<Current<ARInvoice.branchID>>, Or<Branch.branchID, Equal<Current<SOOrder.branchID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deduction Type", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual string RUTROTType
		{
			get;
			set;
		}
		#endregion

		#region CuryROTPersonalAllowance
		public abstract class curyROTPersonalAllowance : PX.Data.BQL.BqlDecimal.Field<curyROTPersonalAllowance> { }

		/// <summary>
		/// The personal allowance limit for ROT deductions.
		/// When ROT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field (see <see cref="RUTROTDistribution"/>).
		/// </summary>
		/// <value>
		/// Defaults to <see cref="BranchRUTROT.ROTPersonalAllowanceLimit"/>.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </value>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(RUTROT.rOTPersonalAllowance))]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<BranchRUTROT.rOTPersonalAllowanceLimit, Where<GL.Branch.branchID, Equal<Current<ARInvoice.branchID>>>>))]
		[PXUIField(DisplayName = RUTROTMessages.AllowanceLimit, FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual decimal? CuryROTPersonalAllowance
		{
			get;
			set;
		}
		#endregion
		#region ROTPersonalAllowance
		public abstract class rOTPersonalAllowance : PX.Data.BQL.BqlDecimal.Field<rOTPersonalAllowance> { }

		/// <summary>
		/// The personal allowance limit for ROT deductions.
		/// When ROT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field (see <see cref="RUTROTDistribution"/>).
		/// </summary>
		/// <value>
		/// Defaults to <see cref="BranchRUTROT.ROTPersonalAllowanceLimit"/>.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </value>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ROTPersonalAllowance
		{
			get;
			set;
		}
		#endregion
		#region CuryROTExtraAllowance
		public abstract class curyROTExtraAllowance : PX.Data.BQL.BqlDecimal.Field<curyROTExtraAllowance> { }

		/// <summary>
		/// The personal allowance limit for ROT deductions for a certain group of household members.
		/// When ROT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field (see <see cref="RUTROTDistribution"/>).
		/// </summary>
		/// <value>
		/// Defaults to <see cref="BranchRUTROT.ROTExtraAllowanceLimit"/>.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </value>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(RUTROT.rOTExtraAllowance))]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<BranchRUTROT.rOTExtraAllowanceLimit, Where<GL.Branch.branchID, Equal<Current<ARInvoice.branchID>>>>))]
		[PXUIField(DisplayName = RUTROTMessages.AllowanceLimitExtra, FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual decimal? CuryROTExtraAllowance
		{
			get;
			set;
		}
		#endregion
		#region ROTExtraAllowance
		public abstract class rOTExtraAllowance : PX.Data.BQL.BqlDecimal.Field<rOTExtraAllowance> { }

		/// <summary>
		/// The personal allowance limit for ROT deductions for a certain group of household members.
		/// When ROT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field (see <see cref="RUTROTDistribution"/>).
		/// </summary>
		/// <value>
		/// Defaults to <see cref="BranchRUTROT.ROTExtraAllowanceLimit"/>.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </value>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ROTExtraAllowance
		{
			get;
			set;
		}
		#endregion
		#region ROTDeductionPct
		public abstract class rOTDeductionPct : PX.Data.BQL.BqlDecimal.Field<rOTDeductionPct> { }

		/// <summary>
		/// The percentage of ROT deduction for the document.
		/// </summary>
		/// <value>
		/// Determines the percentage of invoice amount that can be deducted and claimed from government.
		/// Defaults to the <see cref="BranchRUTROT.ROTDeductionPct">deduction percentage</see> specified 
		/// for the <see cref="BranchID">branch</see> of the document.
		/// </value>
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<BranchRUTROT.rOTDeductionPct, Where<GL.Branch.branchID, Equal<Current<ARInvoice.branchID>>>>))]
		[PXUIField(DisplayName = RUTROTMessages.DeductionPercent, Visible = false, FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? ROTDeductionPct
		{
			get;
			set;
		}
		#endregion

		#region CuryRUTPersonalAllowance
		public abstract class curyRUTPersonalAllowance : PX.Data.BQL.BqlDecimal.Field<curyRUTPersonalAllowance> { }

		/// <summary>
		/// The personal allowance limit for RUT deductions.
		/// When RUT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field (see <see cref="RUTROTDistribution"/>).
		/// </summary>
		/// <value>
		/// Defaults to <see cref="BranchRUTROT.RUTPersonalAllowanceLimit"/>.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </value>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(RUTROT.rUTPersonalAllowance))]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<BranchRUTROT.rUTPersonalAllowanceLimit, Where<GL.Branch.branchID, Equal<Current<ARInvoice.branchID>>>>))]
		[PXUIField(DisplayName = RUTROTMessages.AllowanceLimit, FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual decimal? CuryRUTPersonalAllowance
		{
			get;
			set;
		}
		#endregion
		#region RUTPersonalAllowance
		public abstract class rUTPersonalAllowance : PX.Data.BQL.BqlDecimal.Field<rUTPersonalAllowance> { }

		/// <summary>
		/// The personal allowance limit for RUT deductions.
		/// When RUT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field (see <see cref="RUTROTDistribution"/>).
		/// </summary>
		/// <value>
		/// Defaults to <see cref="BranchRUTROT.RUTPersonalAllowanceLimit"/>.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </value>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RUTPersonalAllowance
		{
			get;
			set;
		}
		#endregion
		#region CuryRUTExtraAllowance
		public abstract class curyRUTExtraAllowance : PX.Data.BQL.BqlDecimal.Field<curyRUTExtraAllowance> { }

		/// <summary>
		/// The personal allowance limit for RUT deductions for a certain group of household members.
		/// When RUT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field (see <see cref="RUTROTDistribution"/>).
		/// </summary>
		/// <value>
		/// Defaults to <see cref="BranchRUTROT.RUTExtraAllowanceLimit"/>.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </value>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(RUTROT.rUTExtraAllowance))]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<BranchRUTROT.rUTExtraAllowanceLimit, Where<GL.Branch.branchID, Equal<Current<ARInvoice.branchID>>>>))]
		[PXUIField(DisplayName = RUTROTMessages.AllowanceLimitExtra, FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual decimal? CuryRUTExtraAllowance
		{
			get;
			set;
		}
		#endregion
		#region RUTExtraAllowance
		public abstract class rUTExtraAllowance : PX.Data.BQL.BqlDecimal.Field<rUTExtraAllowance> { }

		/// <summary>
		/// The personal allowance limit for RUT deductions for a certain group of household members.
		/// When RUT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field (see <see cref="RUTROTDistribution"/>).
		/// </summary>
		/// <value>
		/// Defaults to <see cref="BranchRUTROT.RUTExtraAllowanceLimit"/>.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </value>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RUTExtraAllowance
		{
			get;
			set;
		}
		#endregion
		#region RUTDeductionPct
		public abstract class rUTDeductionPct : PX.Data.BQL.BqlDecimal.Field<rUTDeductionPct> { }

		/// <summary>
		/// The percentage of RUT deduction for the document.
		/// </summary>
		/// <value>
		/// Determines the percentage of invoice amount that can be deducted and claimed from government.
		/// Defaults to the <see cref="BranchRUTROT.RUTDeductionPct">deduction percentage</see> specified 
		/// for the <see cref="BranchID">branch</see> of the document.
		/// </value>
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<BranchRUTROT.rUTDeductionPct, Where<GL.Branch.branchID, Equal<Current<ARInvoice.branchID>>>>))]
		[PXUIField(DisplayName = RUTROTMessages.DeductionPercent, Visible = false, FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? RUTDeductionPct
		{
			get;
			set;
		}
		#endregion

		#region DeductionPct
		public abstract class deductionPct : PX.Data.BQL.BqlDecimal.Field<deductionPct> { }
		/// <summary>
		/// The percentage of RUT and ROT deductions for the document.
		/// </summary>
		/// <value>
		/// Determines the percentage of invoice amount that can be deducted and claimed from the government.
		/// Equals <see cref="RUTDeductionPct"/> or <see cref="ROTDeductionPct"/> depending on <see cref="RUTROTType"/> or the document.
		/// </value>
		[PXDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = RUTROTMessages.DeductionPercent, Visible = false, FieldClass = RUTROTMessages.FieldClass)]
		[PXFormula(typeof(Switch<Case<Where<RUTROT.rUTROTType, Equal<RUTROTTypes.rut>>, rUTDeductionPct>, rOTDeductionPct>))]
		public virtual decimal? DeductionPct
		{
			[PXDependsOnFields(typeof(rUTDeductionPct), typeof(rUTDeductionPct), typeof(rUTROTType))]
			get { return RUTROTType == RUTROTTypes.RUT ? RUTDeductionPct : ROTDeductionPct; }
			set { }
		}
		#endregion
		#region AutoDistribution
		public abstract class autoDistribution : PX.Data.BQL.BqlBool.Field<autoDistribution> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the RUT and ROT deduction 
		/// amount must be distributed between the household members automatically.
		/// If set to <c>false</c>, the amount assigned to each member is entered manually.
		/// (See <see cref="RUTROTDistribution"/>).
		/// </summary>
		/// <value>
		/// Defaults to <c>true</c>.
		/// </value>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Distribute Automatically", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual bool? AutoDistribution
		{
			get;
			set;
		}
		#endregion

		#region IsClaimed
		public abstract class isClaimed : PX.Data.BQL.BqlBool.Field<isClaimed> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the RUT and ROT deduction amount associated 
		/// with the document has been claimed from the government.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "ROT and RUT was claimed", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual bool? IsClaimed
		{
			get;
			set;
		}
		#endregion
		#region CuryAllowedAmt
		public abstract class curyAllowedAmt : PX.Data.BQL.BqlDecimal.Field<curyAllowedAmt> { }

		/// <summary>
		/// The maximum amount of RUT and ROT deduction allowed for the document.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </summary>
		/// <value>
		/// The value of this field is calculated automatically based 
		/// on <see cref="CuryRUTPersonalAllowance"/> or <see cref="CuryROTPersonalAllowance"/> and
		/// the number of household members specified for the document. (See <see cref="RUTROTDistribution"/>).
		/// </value>
		[PXCurrency(typeof(ARInvoice.curyInfoID), typeof(allowedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryAllowedAmt
		{
			get;
			set;
		}
		#endregion
		#region AllowedAmt
		public abstract class allowedAmt : PX.Data.BQL.BqlDecimal.Field<allowedAmt> { }

		/// <summary>
		/// The maximum amount of RUT and ROT deduction allowed for the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </summary>
		/// <value>
		/// See <see cref="CuryAllowedAmt"/>.
		/// </value>
		[PXBaseCury]
		public virtual decimal? AllowedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryDistributedAmt
		public abstract class curyDistributedAmt : PX.Data.BQL.BqlDecimal.Field<curyDistributedAmt> { }

		/// <summary>
		/// The amount of RUT and ROT deductions that has been distributed between the household members.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </summary>
		/// <value>
		/// This field equals the total of <see cref="RUTROTDistribution.CuryAmount"/> for the 
		/// RUT and ROT distribution records associated with the document.
		/// </value>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(distributedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Distributed Amount", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual decimal? CuryDistributedAmt
		{
			get;
			set;
		}
		#endregion
		#region DistributedAmt
		public abstract class distributedAmt : PX.Data.BQL.BqlDecimal.Field<distributedAmt> { }

		/// <summary>
		/// The amount of RUT and ROT deductions that has been distributed between the household members.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </summary>
		/// <value>
		/// See <see cref="CuryDistributedAmt"/>.
		/// </value>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DistributedAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTotalAmt
		public abstract class curyTotalAmt : PX.Data.BQL.BqlDecimal.Field<curyTotalAmt> { }

		/// <summary>
		/// The portion of the document amount that is RUT and ROT deductible.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </summary>
		/// <value>
		/// The value of this field is calculated as the total of the RUT and ROT deductible lines
		/// including VAT taxes. (See <see cref="ARTranRUTROT.CuryRUTROTAvailableAmt"/>).
		/// </value>
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(RUTROT.totalAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Deductible Amount", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		[PXFormula(typeof(Validate<RUTROT.curyAllowedAmt>))]
		public virtual decimal? CuryTotalAmt
		{
			get;
			set;
		}
		#endregion
		#region TotalAmt
		public abstract class totalAmt : PX.Data.BQL.BqlDecimal.Field<totalAmt> { }

		/// <summary>
		/// The portion of the document amount that is RUT and ROT deductible.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </summary>
		/// <value>
		/// See <see cref="CuryTotalAmt"/>.
		/// </value>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TotalAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryUndistributedAmt
		public abstract class curyUndistributedAmt : PX.Data.BQL.BqlDecimal.Field<curyUndistributedAmt> { }

		/// <summary>
		/// The portion of the document amount that is RUT and ROT deductible,
		/// but has not been distributed between household members.
		/// Given in the <see cref="CuryID">currency</see> of the document.
		/// </summary>
		[PXCurrency(typeof(ARInvoice.curyInfoID), typeof(undistributedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(IsNull<Sub<RUTROT.curyTotalAmt, RUTROT.curyDistributedAmt>, decimal0>))]
		[PXUIField(DisplayName = "Undistributed Amount", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual decimal? CuryUndistributedAmt
		{
			get;
			set;
		}
		#endregion
		#region UndistributedAmt
		public abstract class undistributedAmt : PX.Data.BQL.BqlDecimal.Field<undistributedAmt> { }

		/// <summary>
		/// The portion of the document amount that is RUT and ROT deductible,
		/// but has not been distributed between household members.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </summary>
		[PXBaseCury]
		public virtual decimal? UndistributedAmt
		{
			get;
			set;
		}
		#endregion
		#region DistributionLineCntr
		public abstract class distributionLineCntr : PX.Data.BQL.BqlInt.Field<distributionLineCntr> { }

		/// <summary>
		/// The counter of the <see cref="RUTROTDistribution"/> records associated with the document,
		/// used to assign consistent numbers to the child records.
		/// Do not rely on this field to determine the exact number of child records,
		/// because it might not reflect this number under various conditions.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? DistributionLineCntr
		{
			get;
			set;
		}
		#endregion
		#region ClaimDate
		public abstract class claimDate : PX.Data.BQL.BqlDateTime.Field<claimDate> { }

		/// <summary>
		/// The date when the RUT and ROT claim file that includes this document was generated.
		/// </summary>
		/// <value>
		/// A value is assigned to this field when the document is exported through the Claim ROT and RUT
		/// (AR531000) form (corresponds to the <see cref="ClaimRUTROT"/> graph).
		/// </value>
		[PXDBDate]
		[PXUIField(DisplayName = "Export Date", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public DateTime? ClaimDate
		{
			get;
			set;
		}
		#endregion
		#region ClaimFileName
		public abstract class claimFileName : PX.Data.BQL.BqlString.Field<claimFileName> { }

		/// <summary>
		/// The name of the RUT and ROT claim file that includes this document.
		/// </summary>
		/// <value>
		/// A value is assigned to this field when the document is exported through the Claim ROT and RUT
		/// (AR531000) form (corresponds to the <see cref="ClaimRUTROT"/> graph).
		/// </value>
		[PXDBString(40)]
		[PXUIField(DisplayName = "Export File", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual string ClaimFileName
		{
			get;
			set;
		}
		#endregion
		#region ExportRefNbr
		public abstract class exportRefNbr : PX.Data.BQL.BqlInt.Field<exportRefNbr> { }

		/// <summary>
		/// The reference number of the RUT or ROT claim that includes the document.
		/// </summary>
		/// <value>
		/// The system uses the <see cref="BranchRUTROT.RUTROTClaimNextRefNbr"/> field to generate the claim number.
		/// A value is assigned to this field when the document is exported through the Claim ROT and RUT
		/// (AR531000) form (corresponds to the <see cref="ClaimRUTROT"/> graph).
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Export Ref Nbr.", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual int? ExportRefNbr
		{
			get;
			set;
		}
		#endregion

		#region ROTAppartment
		public abstract class rOTAppartment : PX.Data.BQL.BqlString.Field<rOTAppartment> { }

		/// <summary>
		/// The <see cref="CustomerID">customer's</see> apartment number for a ROT deductible document.
		/// This field is relevant only if <see cref="RUTROTType"/> is ROT (<c>"O"</c>).
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Apartment", FieldClass = RUTROTMessages.FieldClass)]
		public virtual string ROTAppartment
		{
			get;
			set;
		}
		#endregion
		#region ROTEstate
		public abstract class rOTEstate : PX.Data.BQL.BqlString.Field<rOTEstate> { }

		/// <summary>
		/// The <see cref="CustomerID">customer's</see> real estate number for a ROT deductible document.
		/// This field is relevant only if <see cref="RUTROTType"/> is ROT (<c>"O"</c>).
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Real estate", FieldClass = RUTROTMessages.FieldClass)]
		public virtual string ROTEstate
		{
			get;
			set;
		}
		#endregion
		#region ROTOrganizationNbr
		public abstract class rOTOrganizationNbr : PX.Data.BQL.BqlString.Field<rOTOrganizationNbr> { }

		/// <summary>
		/// The organization number for a ROT deductible document.
		/// This field is relevant only if <see cref="RUTROTType"/> is ROT (<c>"O"</c>).
		/// </summary>
		/// <value>
		/// The system validates the organization number according to 
		/// the <see cref="BranchRUTROT.RUTROTOrgNbrValidRegEx">regular expression</see>
		/// specified for the <see cref="BranchID">branch</see> of the document.
		/// </value>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(20)]
		[PXUIField(DisplayName = "Organization nbr.", FieldClass = RUTROTMessages.FieldClass)]
		[DynamicValueValidation(typeof(Search<BranchRUTROT.rUTROTOrgNbrValidRegEx,
											Where<Current<ARInvoiceRUTROT.isRUTROTDeductible>, Equal<boolTrue>,
											And<Current<RUTROT.rUTROTType>, Equal<RUTROTTypes.rot>,
											And<GL.Branch.branchID, Equal<Current<ARInvoice.branchID>>>>>>))]
		public virtual string ROTOrganizationNbr
		{
			get;
			set;
		}
		#endregion

		#region CuryOtherCost
		public abstract class curyOtherCost : PX.Data.BQL.BqlDecimal.Field<curyOtherCost> { }
		/// <summary>
		/// The portion of the document amount that is related to the lines with the <see cref="RUTROTItemTypes.OtherCost"/> type.
		/// The value is specified in the <see cref="CuryID">currency</see> of the document.
		/// </summary>
		[PXUIField(DisplayName = "Other Cost", FieldClass = RUTROTMessages.FieldClass)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(RUTROT.otherCost))]
		public virtual decimal? CuryOtherCost
		{
			get;
			set;
		}
		#endregion
		#region CuryMaterialCost
		public abstract class curyMaterialCost : PX.Data.BQL.BqlDecimal.Field<curyMaterialCost> { }
		/// <summary>
		/// The portion of the document amount that is related to the lines with the <see cref="RUTROTItemTypes.MaterialCost"/> type.
		/// The value is specified in the <see cref="CuryID">currency</see> of the document.
		/// </summary>
		[PXUIField(DisplayName = "Material Cost", FieldClass = RUTROTMessages.FieldClass)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(RUTROT.materialCost))]
		public virtual decimal? CuryMaterialCost
		{
			get;
			set;
		}
		#endregion
		#region CuryWorkPrice
		public abstract class curyWorkPrice : PX.Data.BQL.BqlDecimal.Field<curyWorkPrice> { }
		/// <summary>
		/// The portion of the document amount (in the <see cref="CuryID">currency</see> of the document) 
		/// that is related to the lines with the <see cref="RUTROTItemTypes.Service"/> type.
		/// </summary>
		[PXUIField(DisplayName = "Work Price", FieldClass = RUTROTMessages.FieldClass)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(RUTROT.workPrice))]
		public virtual decimal? CuryWorkPrice
		{
			get;
			set;
		}
		#endregion
		#region OtherCost
		public abstract class otherCost : PX.Data.BQL.BqlDecimal.Field<otherCost> { }
		/// <summary>
		/// The portion of the document amount (in the base <see cref="Company.BaseCuryID">currency</see>) 
		/// that is related to the lines with the <see cref="RUTROTItemTypes.OtherCost"/> type.
		/// </summary>
		[PXBaseCury]
		public virtual decimal? OtherCost
		{
			get;
			set;
		}
		#endregion
		#region MaterialCost
		public abstract class materialCost : PX.Data.BQL.BqlDecimal.Field<materialCost> { }
		/// <summary>
		/// The portion of the document amount (in the base <see cref="Company.BaseCuryID">currency</see>) 
		/// that is related to the lines with the <see cref="RUTROTItemTypes.MaterialCost"/> type.
		/// </summary>
		[PXBaseCury]
		public virtual decimal? MaterialCost
		{
			get;
			set;
		}
		#endregion
		#region WorkPrice
		public abstract class workPrice : PX.Data.BQL.BqlDecimal.Field<workPrice> { }
		/// <summary>
		/// The portion of the document amount (in the base <see cref="Company.BaseCuryID">currency</see>) 
		/// that is related to the lines with the <see cref="RUTROTItemTypes.Service"/> type.
		/// </summary>
		[PXBaseCury]
		public virtual decimal? WorkPrice
		{
			get;
			set;
		}
        #endregion

        #region BalancingCreditMemoDocType
        public abstract class balancingCreditMemoDocType : PX.Data.BQL.BqlString.Field<balancingCreditMemoDocType> { }

        [PXDBString(3, IsFixed = true)]
        [PXDefault(typeof(ARDocType.creditMemo))]
        public virtual string BalancingCreditMemoDocType { get; set; }
        #endregion
        #region BalancingDebitMemoDocType
        public abstract class balancingDebitMemoDocType : PX.Data.BQL.BqlString.Field<balancingDebitMemoDocType> { }

        [PXDBString(3, IsFixed = true)]
        [PXDefault(typeof(ARDocType.debitMemo))]
        public virtual string BalancingDebitMemoDocType { get; set; }
        #endregion
        #region BalancingCreditMemoRefNbr
        public abstract class balancingCreditMemoRefNbr : PX.Data.BQL.BqlString.Field<balancingCreditMemoRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Balancing Credit Memo Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
        public virtual string BalancingCreditMemoRefNbr { get; set; }
        #endregion
        #region BalancingDebitMemoRefNbr
        public abstract class balancingDebitMemoRefNbr : PX.Data.BQL.BqlString.Field<balancingDebitMemoRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Balancing Debit Memo Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
        public virtual string BalancingDebitMemoRefNbr { get; set; }
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
	}
}
