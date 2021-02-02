using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.RUTROT
{
	[Serializable]
	public class BranchRUTROT : PXCacheExtension<Branch>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
		}
		#region AllowsRUTROT
		public abstract class allowsRUTROT : PX.Data.BQL.BqlBool.Field<allowsRUTROT> { }

		/// <summary>
		/// When set to <c>true</c>, indicates that the Branch allows for ROT and RUT deduction.
		/// This, in turn, allows to mark the lines of AR invoices as <see cref="AR.ARTran.IsRUTROTDeductible">RUT and ROT deductible</see>.
		/// This field is relevant only if the <see cref="FeaturesSet.RutRotDeduction">ROT and RUT Deduction</see> feature has been activated.
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Uses ROT & RUT deduction", FieldClass = RUTROTMessages.FieldClass)]
		public virtual bool? AllowsRUTROT
		{
			get;
			set;
		}
		#endregion
		#region RUTDeductionPct
		public abstract class rUTDeductionPct : PX.Data.BQL.BqlDecimal.Field<rUTDeductionPct> { }

		/// <summary>
		/// The percentage of RUT deduction for this Branch.
		/// This field is relevant only for the branches that <see cref="AllowsRUTROT">allow RUT and ROT deduction</see>.
		/// </summary>
		/// <value>
		/// Determines the percentage of invoice amount that can be deducted and claimed from government.
		/// </value>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = RUTROTMessages.DeductionPercent, FieldClass = RUTROTMessages.FieldClass)]
		public decimal? RUTDeductionPct
		{
			get;
			set;
		}
		#endregion
		#region RUTPersonalAllowanceLimit
		public abstract class rUTPersonalAllowanceLimit : PX.Data.BQL.BqlDecimal.Field<rUTPersonalAllowanceLimit> { }

		/// <summary>
		/// The personal allowance limit for RUT deduction.
		/// When RUT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field.
		/// This field is relevant only for the branches that <see cref="AllowsRUTROT">allow RUT and ROT deduction</see>.
		/// </summary>
		/// <value>
		/// Given in the <see cref="RUTROTCuryID">currency of deduction</see>.
		/// </value>
		[PXDBDecimal(MinValue = 0, MaxValue = 100000000)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = RUTROTMessages.AllowanceLimit, FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? RUTPersonalAllowanceLimit
		{
			get;
			set;
		}
		#endregion
		#region RUTExtraAllowanceLimit
		public abstract class rUTExtraAllowanceLimit : PX.Data.BQL.BqlDecimal.Field<rUTExtraAllowanceLimit> { }

		/// <summary>
		/// The personal allowance limit for RUT deduction.
		/// When RUT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field.
		/// This field is relevant only for the branches that <see cref="AllowsRUTROT">allow RUT and ROT deduction</see>.
		/// </summary>
		/// <value>
		/// Given in the <see cref="RUTROTCuryID">currency of deduction</see>.
		/// </value>
		[PXDBDecimal(MinValue = 0, MaxValue = 100000000)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = RUTROTMessages.AllowanceLimitExtra, FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? RUTExtraAllowanceLimit
		{
			get;
			set;
		}
		#endregion
		#region ROTDeductionPct
		public abstract class rOTDeductionPct : PX.Data.BQL.BqlDecimal.Field<rOTDeductionPct> { }

		/// <summary>
		/// The percentage of ROT deduction for this Branch.
		/// This field is relevant only for the branches that <see cref="AllowsRUTROT">allow RUT and ROT deduction</see>.
		/// </summary>
		/// <value>
		/// Determines the percentage of invoice amount that can be deducted and claimed from government.
		/// </value>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = RUTROTMessages.DeductionPercent, FieldClass = RUTROTMessages.FieldClass)]
		public decimal? ROTDeductionPct
		{
			get;
			set;
		}
		#endregion
		#region ROTPersonalAllowanceLimit
		public abstract class rOTPersonalAllowanceLimit : PX.Data.BQL.BqlDecimal.Field<rOTPersonalAllowanceLimit> { }

		/// <summary>
		/// The personal allowance limit for ROT deduction.
		/// When ROT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field.
		/// This field is relevant only for the branches that <see cref="AllowsRUTROT">allow RUT and ROT deduction</see>.
		/// </summary>
		/// <value>
		/// Given in the <see cref="RUTROTCuryID">currency of deduction</see>.
		/// </value>
		[PXDBDecimal(MinValue = 0, MaxValue = 100000000)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = RUTROTMessages.AllowanceLimit, FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? ROTPersonalAllowanceLimit
		{
			get;
			set;
		}
		#endregion
		#region ROTExtraAllowanceLimit
		public abstract class rOTExtraAllowanceLimit : PX.Data.BQL.BqlDecimal.Field<rOTExtraAllowanceLimit> { }

		/// <summary>
		/// The personal allowance limit for ROT deduction for a certain group of household members.
		/// When ROT deduction is distributed between household members, the amounts assigned
		/// to each of the members can't exceed the value specified in this field.
		/// This field is relevant only for the branches that <see cref="AllowsRUTROT">allow RUT and ROT deduction</see>.
		/// </summary>
		/// <value>
		/// Given in the <see cref="RUTROTCuryID">currency of deduction</see>.
		/// </value>
		[PXDBDecimal(MinValue = 0, MaxValue = 100000000)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = RUTROTMessages.AllowanceLimitExtra, FieldClass = RUTROTMessages.FieldClass)]
		public virtual decimal? ROTExtraAllowanceLimit
		{
			get;
			set;
		}
		#endregion
		#region RUTROTCuryID
		public abstract class rUTROTCuryID : PX.Data.BQL.BqlString.Field<rUTROTCuryID> { }

		/// <summary>
		/// The identifier of the <see cref="Currency"/>, in which RUT and ROT are deducted.
		/// This field is relevant only for the branches that <see cref="AllowsRUTROT">allow RUT and ROT deduction</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Currency.CuryID"/> field.
		/// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", FieldClass = RUTROTMessages.FieldClass)]
		[PXSelector(typeof(Currency.curyID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string RUTROTCuryID
		{
			get;
			set;
		}
		#endregion
		#region RUTROTClaimNextRefNbr
		public abstract class rUTROTClaimNextRefNbr : PX.Data.BQL.BqlInt.Field<rUTROTClaimNextRefNbr> { }

		/// <summary>
		/// The number of the next RUT and ROT claim document.
		/// The value of this field is used to assign consistent numbers to the RUT and ROT claims created through the
		/// Claim RUT and ROT (AR.53.10.00) screen (corresponds to the <see cref="ClaimRUTROT"/> graph).
		/// This field is relevant only for the branches that <see cref="AllowsRUTROT">allow RUT and ROT deduction</see>.
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 100000000)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Next Claim Nbr", FieldClass = RUTROTMessages.FieldClass)]
		public virtual int? RUTROTClaimNextRefNbr
		{
			get;
			set;
		}
		#endregion
		#region RUTROTOrgNbrValidRegEx
		public abstract class rUTROTOrgNbrValidRegEx : PX.Data.BQL.BqlString.Field<rUTROTOrgNbrValidRegEx> { }

		/// <summary>
		/// The regular expression used to validate <see cref="RUTROT.ROTOrganizationNbr">Organization Number</see>
		/// on ROT-deductible <see cref="ARInvoiceRUTROT">Invoices</see>.
		/// This field is relevant only for the branches that <see cref="AllowsRUTROT">allow RUT and ROT deduction</see>.
		/// </summary>
		[PXDBString(255)]
		[PXUIField(DisplayName = "Org. Nbr. Validation Reg. Exp.", FieldClass = RUTROTMessages.FieldClass)]
		public virtual string RUTROTOrgNbrValidRegEx
		{
			get;
			set;
		}
		#endregion
		#region Default Type
		public abstract class defaultRUTROTType : PX.Data.BQL.BqlString.Field<defaultRUTROTType> { }

		/// <summary>
		/// The default type of RUT or ROT deduction used for 
		///  documents (<see cref="ARInvoiceRUTROT"/> or <see cref="SOOrderRUTROT"/>) creation.
		/// This field is relevant only for the branches that <see cref="AllowsRUTROT">allow RUT and ROT deduction</see>.
		/// </summary>
		[PXDBString(1)]
		[RUTROTTypes.List]
		[PXDefault(RUTROTTypes.RUT)]
		[PXUIField(DisplayName = "Default RUTROT Type", FieldClass = RUTROTMessages.FieldClass)]
		public virtual string DefaultRUTROTType
		{
			get;
			set;
		}

        #endregion
        #region TaxAgencyAccountID
        public abstract class taxAgencyAccountID : PX.Data.BQL.BqlInt.Field<taxAgencyAccountID> { }

        [AR.CustomerActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(AR.Customer.acctName), Filterable = true, TabOrder = 2, DisplayName = "Tax Agency Account")]
        public virtual int? TaxAgencyAccountID
        {
            get;
            set;
        }
        #endregion
        #region BalanceOnProcess
        public abstract class balanceOnProcess : PX.Data.BQL.BqlString.Field<balanceOnProcess> { }

        [PXDBString(1)]
        [RUTROTBalanceOn.List]
        [PXDefault(RUTROTBalanceOn.Release)]
        [PXUIField(DisplayName = "Balance Invoices On", FieldClass = RUTROTMessages.FieldClass)]
        public virtual string BalanceOnProcess { get; set; }
        #endregion
    }
}
