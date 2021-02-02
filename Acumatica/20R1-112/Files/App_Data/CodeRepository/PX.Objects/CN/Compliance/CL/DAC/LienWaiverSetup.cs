using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;
using PX.Objects.CN.Compliance.CL.Graphs;
using PX.Objects.CN.Compliance.Descriptor;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    [PXCacheName("Compliance Preferences")]
    [PXPrimaryGraph(typeof(ComplianceDocumentSetupMaint))]
    public class LienWaiverSetup : BaseCache, IBqlTable
    {
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Warn of Outstanding Lien Waivers During AP Bill Entry")]
        public virtual bool? ShouldWarnOnBillEntry
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Warn of Outstanding Lien Waivers When Selecting AP Bill for Payment")]
        public virtual bool? ShouldWarnOnPayment
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Stop Payment of AP Bill When There Are Outstanding Lien Waivers")]
        public virtual bool? ShouldStopPayments
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.AutomaticallyGenerateLienWaivers)]
        public virtual bool? ShouldGenerateConditional
        {
            get;
            set;
        }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.AutomaticallyGenerateLienWaivers)]
        public virtual bool? ShouldGenerateUnconditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXDefault(LienWaiverGenerationEvent.PayingBill, PersistingCheck = PXPersistingCheck.Nothing)]
        [LienWaiverGenerationEvent.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.GenerateLienWaiversOn, Enabled = false)]
        public virtual string GenerationEventConditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXDefault(LienWaiverGenerationEvent.PayingBill, PersistingCheck = PXPersistingCheck.Nothing)]
        [LienWaiverGenerationEvent.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.GenerateLienWaiversOn, Enabled = false)]
        public virtual string GenerationEventUnconditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXUIEnabled(typeof(Where<shouldGenerateConditional.IsEqual<True>>))]
        [PXDefault(LienWaiverThroughDateSource.PostingPeriodEndDate, PersistingCheck = PXPersistingCheck.Nothing)]
        [LienWaiverThroughDateSource.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.ThroughDate)]
        public virtual string ThroughDateSourceConditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXUIEnabled(typeof(Where<shouldGenerateUnconditional.IsEqual<True>>))]
        [PXDefault(LienWaiverThroughDateSource.PaymentDate, PersistingCheck = PXPersistingCheck.Nothing)]
        [LienWaiverThroughDateSource.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.ThroughDate)]
        public virtual string ThroughDateSourceUnconditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXDefault(LienWaiverAmountSource.BillAmount, PersistingCheck = PXPersistingCheck.Nothing)]
        [LienWaiverAmountSource.ListConditional]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.FinalLienWaiverAmount, Enabled = false)]
        public virtual string FinalAmountSourceConditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXDefault(LienWaiverAmountSource.AmountPaid, PersistingCheck = PXPersistingCheck.Nothing)]
        [LienWaiverAmountSource.ListUnconditional]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.FinalLienWaiverAmount, Enabled = false)]
        public virtual string FinalAmountSourceUnconditional
        {
            get;
            set;
        }

        public abstract class shouldWarnOnBillEntry : BqlBool.Field<shouldWarnOnBillEntry>
        {
        }

        public abstract class shouldWarnOnPayment : BqlBool.Field<shouldWarnOnPayment>
        {
        }

        public abstract class shouldStopPayments : BqlBool.Field<shouldStopPayments>
        {
        }

        public abstract class shouldGenerateConditional : BqlBool.Field<shouldGenerateConditional>
        {
        }

        public abstract class shouldGenerateUnconditional : BqlBool.Field<shouldGenerateUnconditional>
        {
        }

        public abstract class generationEventConditional : BqlString.Field<generationEventConditional>
        {
        }

        public abstract class generationEventUnconditional : BqlString.Field<generationEventUnconditional>
        {
        }

        public abstract class throughDateSourceConditional : BqlString.Field<throughDateSourceConditional>
        {
        }

        public abstract class throughDateSourceUnconditional : BqlString.Field<throughDateSourceUnconditional>
        {
        }

        public abstract class finalAmountSourceConditional : BqlString.Field<finalAmountSourceConditional>
        {
        }

        public abstract class finalAmountSourceUnconditional : BqlString.Field<finalAmountSourceUnconditional>
        {
        }
    }
}
