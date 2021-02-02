using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services;

namespace PX.Objects.CN.JointChecks.AP.Descriptor.Attributes
{
    public sealed class AdjustmentReferenceNumberSelectorAttribute : PXSelectorAttribute
    {
        private static readonly Type[] Fields =
        {
            typeof(APRegister.refNbr),
            typeof(APRegister.docDate),
            typeof(APRegister.finPeriodID),
            typeof(APRegister.vendorLocationID),
            typeof(APRegister.curyID),
            typeof(APRegister.curyOrigDocAmt),
            typeof(APRegister.curyDocBal),
            typeof(APRegister.status),
            typeof(APAdjust.APInvoice.dueDate),
            typeof(APAdjust.APInvoice.invoiceNbr),
            typeof(APRegister.docDesc)
        };

        public AdjustmentReferenceNumberSelectorAttribute()
            : base(typeof(Search2<APAdjust.APInvoice.refNbr,
                LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APAdjust.APInvoice.docType>,
                    And<APAdjust.adjdRefNbr, Equal<APAdjust.APInvoice.refNbr>,
                    And<APAdjust.released, Equal<False>>>>,
                LeftJoin<APAdjust2, On<APAdjust2.adjgDocType, Equal<APAdjust.APInvoice.docType>,
                    And<APAdjust2.adjgRefNbr, Equal<APAdjust.APInvoice.refNbr>,
                    And<APAdjust2.released, Equal<False>,
                    And<APAdjust2.voided, Equal<False>>>>>,
                LeftJoin<APPayment, On<APPayment.docType, Equal<APAdjust.APInvoice.docType>,
                    And<APPayment.refNbr, Equal<APAdjust.APInvoice.refNbr>,
                    And<Where<APPayment.docType, Equal<APDocType.prepayment>,
                    Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>>,
                Where<APAdjust.APInvoice.vendorID, Equal<Optional<APPayment.vendorID>>,
                    And<APAdjust.APInvoice.docType, Equal<Optional<APAdjust.adjdDocType>>,
                    And2<Where<APAdjust.APInvoice.released, Equal<True>,
                        Or<APRegister.prebooked, Equal<True>>>,
                    And<APAdjust.APInvoice.openDoc, Equal<True>,
                    And2<Where<APAdjust.adjgRefNbr, IsNull,
                        Or<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>>>,
                    And<APAdjust2.adjdRefNbr, IsNull,
                    And2<Where<APPayment.refNbr, IsNull,
                        And<Current<APPayment.docType>, NotEqual<APDocType.refund>,
                        Or<APPayment.refNbr, IsNotNull,
                        And<Current<APPayment.docType>, Equal<APDocType.refund>,
                        Or<APPayment.docType, Equal<APDocType.debitAdj>,
                        And<Current<APPayment.docType>, Equal<APDocType.check>,
                        Or<APPayment.docType, Equal<APDocType.debitAdj>,
                        And<Current<APPayment.docType>, Equal<APDocType.voidCheck>>>>>>>>>,
                    And2<Where<APAdjust.APInvoice.docDate, LessEqual<Current<APPayment.adjDate>>,
                        And<APAdjust.APInvoice.tranPeriodID, LessEqual<Current<APPayment.adjTranPeriodID>>,
                        Or<Current<APPayment.adjFinPeriodID>, IsNull,
                        Or<Current<APPayment.docType>, Equal<APDocType.check>,
                        And<Current<APSetup.earlyChecks>, Equal<True>,
                        Or<Current<APPayment.docType>, Equal<APDocType.voidCheck>,
                        And<Current<APSetup.earlyChecks>, Equal<True>,
                        Or<Current<APPayment.docType>, Equal<APDocType.prepayment>,
                        And<Current<APSetup.earlyChecks>, Equal<True>>>>>>>>>>,
                        And2<Where<Current<APSetup.migrationMode>, NotEqual<True>,
                            Or<APAdjust.APInvoice.isMigratedRecord, Equal<Current<APRegister.isMigratedRecord>>>>,
                        And<Where<APAdjust.APInvoice.pendingPPD, NotEqual<True>,
                        Or<Current<APRegister.pendingPPD>, Equal<True>>>>>>>>>>>>>>), Fields)
        {
            Filterable = true;
            ValidateValue = SiteMapExtension.IsChecksAndPaymentsScreenId();
        }
    }
}