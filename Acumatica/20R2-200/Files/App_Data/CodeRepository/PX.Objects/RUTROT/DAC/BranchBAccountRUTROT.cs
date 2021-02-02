using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.RUTROT.DAC;

namespace PX.Objects.RUTROT
{
	[Serializable]
	public class BranchBAccountRUTROT : PXCacheExtension<BranchMaint.BranchBAccount>, IRUTROTConfigurationHolder
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
		}

		#region AllowsRUTROT
		public abstract class allowsRUTROT : PX.Data.BQL.BqlBool.Field<allowsRUTROT> { }

		[PXDBBool(BqlField = typeof(BranchRUTROT.allowsRUTROT))]
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

		[PXDBDecimal(BqlField = typeof(BranchRUTROT.rUTDeductionPct), MinValue = 0, MaxValue = 100)]
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

		[PXDBDecimal(BqlField = typeof(BranchRUTROT.rUTPersonalAllowanceLimit), MinValue = 0, MaxValue = 100000000)]
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

		[PXDBDecimal(BqlField = typeof(BranchRUTROT.rUTExtraAllowanceLimit), MinValue = 0, MaxValue = 100000000)]
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

		[PXDBDecimal(BqlField = typeof(BranchRUTROT.rOTDeductionPct), MinValue = 0, MaxValue = 100)]
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

		[PXDBDecimal(BqlField = typeof(BranchRUTROT.rOTPersonalAllowanceLimit), MinValue = 0, MaxValue = 100000000)]
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

		[PXDBDecimal(BqlField = typeof(BranchRUTROT.rOTExtraAllowanceLimit), MinValue = 0, MaxValue = 100000000)]
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

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(BranchRUTROT.rUTROTCuryID))]
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

		[PXDBInt(MinValue = 0, MaxValue = 100000000, BqlField = typeof(BranchRUTROT.rUTROTClaimNextRefNbr))]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Next Export File Ref Nbr", FieldClass = RUTROTMessages.FieldClass)]
		public virtual int? RUTROTClaimNextRefNbr
		{
			get;
			set;
		}
		#endregion
		#region RUTROTOrgNbrValidRegEx
		public abstract class rUTROTOrgNbrValidRegEx : PX.Data.BQL.BqlString.Field<rUTROTOrgNbrValidRegEx> { }

		[PXDBString(255, BqlField = typeof(BranchRUTROT.rUTROTOrgNbrValidRegEx))]
		[PXUIField(DisplayName = "Org. Nbr. Validation Reg. Exp.", FieldClass = RUTROTMessages.FieldClass)]
		public virtual string RUTROTOrgNbrValidRegEx
		{
			get;
			set;
		}
        #endregion
        #region Default Type
        public abstract class defaultRUTROTType : PX.Data.BQL.BqlString.Field<defaultRUTROTType> { }

        [PXDBString(1, BqlField = typeof(BranchRUTROT.defaultRUTROTType))]
        [RUTROTTypes.List]
        [PXDefault(RUTROTTypes.RUT)]
        [PXUIField(DisplayName = "Default Type", FieldClass = RUTROTMessages.FieldClass)]
		public virtual string DefaultRUTROTType
		{
			get;
			set;
		}

        #endregion
        #region TaxAgencyAccountID
        public abstract class taxAgencyAccountID : PX.Data.BQL.BqlInt.Field<taxAgencyAccountID> { }

        [AR.CustomerActive(BqlField = typeof(BranchRUTROT.taxAgencyAccountID), Visibility = PXUIVisibility.SelectorVisible, 
            DescriptionField = typeof(AR.Customer.acctName), Filterable = true, TabOrder = 2, DisplayName = "Tax Agency Account")]
        public virtual int? TaxAgencyAccountID
        {
            get;
            set;
        }
        #endregion
        #region BalanceOnProcess
        public abstract class balanceOnProcess : PX.Data.BQL.BqlString.Field<balanceOnProcess> { }

        [PXDBString(1, BqlField = typeof(BranchRUTROT.balanceOnProcess))]
        [RUTROTBalanceOn.List]
        [PXDefault(RUTROTBalanceOn.Release)]
        [PXUIField(DisplayName = "BalanceInvoicesOn", FieldClass = RUTROTMessages.FieldClass)]
        public virtual string BalanceOnProcess { get; set; }
        #endregion
    }
}
