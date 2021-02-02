using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.CacheExtensions
{
    public sealed class ApPaymentExt : PXCacheExtension<APPayment>
    {
        [PXBool]
        [PXUIField(DisplayName = "Joint Check", IsReadOnly = true)]
        public bool? IsJointCheck
        {
            get;
            set;
        }

        [PXDecimal]
        [PXUIField(DisplayName = "Vendor Payment Amount", IsReadOnly = true)]
        public decimal? VendorPaymentAmount
        {
            get;
            set;
        }

        [PXDecimal]
        [PXUIField(DisplayName = "Joint Payment Amount", IsReadOnly = true)]
        public decimal? JointPaymentAmount
        {
            get;
            set;
        }

        // this field is used in Joint Amount Application tab and always contains empty value
        // should be removed in future after adding multi-currency support for joint checks
        [PXString]
        [PXUIField(DisplayName = "Currency")]
        public string CurrencyStub
        {
            get;
            set;
        }

        /// <summary>
        /// Used to mark a payment during creation of joint checks. While creating a joint check, the payment entry
        /// is persisted twice: before and after creation of <see cref="JointPayeePayment"/> records. Lien waivers
        /// should be created on the second time in order to set lien waiver amount for joint vendors.
        /// </summary>
        [PXBool]
        public bool? ShouldCreateLienWaivers
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class isJointCheck : BqlBool.Field<isJointCheck>
        {
        }

        public abstract class vendorPaymentAmount : BqlDecimal.Field<vendorPaymentAmount>
        {
        }

        public abstract class jointPaymentAmount : BqlDecimal.Field<jointPaymentAmount>
        {
        }

        public abstract class currencyStub : BqlString.Field<currencyStub>
        {
        }

        public abstract class shouldCreateLienWaivers : BqlBool.Field<shouldCreateLienWaivers>
        {
        }
    }
}