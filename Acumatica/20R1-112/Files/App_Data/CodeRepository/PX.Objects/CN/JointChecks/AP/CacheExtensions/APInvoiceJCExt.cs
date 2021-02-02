using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.CacheExtensions
{
    public sealed class APInvoiceJCExt : PXCacheExtension<APInvoice>
    {
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Joint Payees")]
        public bool? IsJointPayees
        {
            get;
            set;
        }

        [PXDBDecimal]
        public decimal? TotalJointAmount
        {
            get;
            set;
        }

        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Amount To Pay")]
        public decimal? AmountToPay
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false)]
        public bool? IsPaymentCycleWorkflow
        {
            get;
            set;
        }

        [PXDecimal]
        [PXUIField(DisplayName = JointCheckLabels.VendorBalance)]
        public decimal? VendorBalance
        {
            get;
            set;
        }

        [PXBool]
        public bool? IsAdjustingJointAmountsInProgress
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class isJointPayees : BqlBool.Field<isJointPayees>
        {
        }

        public abstract class totalJointAmount : BqlDecimal.Field<totalJointAmount>
        {
        }

        public abstract class amountToPay : BqlDecimal.Field<amountToPay>
        {
        }

        public abstract class isPaymentCycleWorkflow : BqlBool.Field<isPaymentCycleWorkflow>
        {
        }

        public abstract class vendorBalance : BqlDecimal.Field<vendorBalance>
        {
        }

        public abstract class isAdjustingJointAmountsInProgress : BqlBool.Field<isAdjustingJointAmountsInProgress>
        {
        }
    }
}