using System;
using PX.Common.Serialization;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CN.JointChecks.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.DAC
{
    [PXSerializable]
    [PXCacheName("Joint Payee Payment")]
    public class JointPayeePayment : BaseCache, IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? JointPayeePaymentId
        {
            get;
            set;
        }

        [PXDBInt]
        public virtual int? JointPayeeId
        {
            get;
            set;
        }

        [PXInt]
        [PXUnboundDefault(typeof(SelectFrom<JointPayee>
                .Where<JointPayee.jointPayeeId.IsEqual<jointPayeeId.FromCurrent>>),
            SourceField = typeof(JointPayee.billLineNumber))]
        [PXUIField(DisplayName = JointCheckLabels.BillLineNumber,
            Visibility = PXUIVisibility.Invisible, Enabled = false)]
        public virtual int? BillLineNumber
        {
            get;
            set;
        }

        [PXDBString]
        public virtual string PaymentRefNbr
        {
            get;
            set;
        }

        [PXDBString]
        public virtual string PaymentDocType
        {
            get;
            set;
        }

        [PXDBString]
        [PXSelector(typeof(Search<APInvoice.refNbr, Where<APInvoice.docType, Equal<APDocType.invoice>>>),
            SubstituteKey = typeof(APInvoice.refNbr))]
        [PXUIField(DisplayName = JointCheckLabels.ApBillNbr, Enabled = false)]
        public virtual string InvoiceRefNbr
        {
            get;
            set;
        }

        [PXDBString]
        public virtual string InvoiceDocType
        {
            get;
            set;
        }

        [PXDBInt]
        public virtual int? AdjustmentNumber
        {
            get;
            set;
        }

        [PXDBDecimal(MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = JointCheckLabels.JointAmountToPay)]
        public virtual decimal? JointAmountToPay
        {
            get;
            set;
        }

        public abstract class jointPayeePaymentId : BqlInt.Field<jointPayeePaymentId>
        {
        }

        public abstract class jointPayeeId : BqlInt.Field<jointPayeeId>
        {
        }

        public abstract class billLineNumber : BqlInt.Field<billLineNumber>
        {
        }

        public abstract class paymentRefNbr : BqlString.Field<paymentRefNbr>
        {
        }

        public abstract class paymentDocType : BqlString.Field<paymentDocType>
        {
        }

        public abstract class invoiceRefNbr : BqlString.Field<invoiceRefNbr>
        {
        }

        public abstract class invoiceDocType : BqlString.Field<invoiceDocType>
        {
        }

        public abstract class jointAmountToPay : BqlDecimal.Field<jointAmountToPay>
        {
        }

        public abstract class adjustmentNumber : BqlInt.Field<adjustmentNumber>
        {
        }
    }
}