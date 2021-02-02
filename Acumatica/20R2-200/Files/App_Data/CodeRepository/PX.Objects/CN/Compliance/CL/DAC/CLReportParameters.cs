using PX.Data;
using PX.Objects.CN.Compliance.CL.Descriptor;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    /// <summary>
    /// Auxiliary DAC used in reports.
    /// </summary>
    [PXHidden]
    public class CLReportParameters : IBqlTable
    {
        [PXInt]
        [PXUIField]
        [LienWaiverReportSelector(Constants.LienWaiverDocumentTypeValues.UnconditionalPartial)]
        public virtual int? UnconditionalPartial
        {
            get;
            set;
        }

        [PXInt]
        [PXUIField]
        [LienWaiverReportSelector(Constants.LienWaiverDocumentTypeValues.ConditionalPartial)]
        public virtual int? ConditionalPartial
        {
            get;
            set;
        }

        [PXInt]
        [PXUIField]
        [LienWaiverReportSelector(Constants.LienWaiverDocumentTypeValues.UnconditionalFinal)]
        public virtual int? UnconditionalFinal
        {
            get;
            set;
        }

        [PXInt]
        [PXUIField]
        [LienWaiverReportSelector(Constants.LienWaiverDocumentTypeValues.ConditionalFinal)]
        public virtual int? ConditionalFinal
        {
            get;
            set;
        }
    }
}
