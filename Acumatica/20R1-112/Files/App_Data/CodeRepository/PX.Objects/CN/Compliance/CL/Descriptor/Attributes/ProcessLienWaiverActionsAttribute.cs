using PX.Data;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ProcessLienWaiverActionsAttribute : PXStringListAttribute
    {
        public const string EmailLienWaiver = "Email Lien Waivers";
        public const string PrintLienWaiver = "Print Lien Waivers";

        private static readonly string[] ProcessLienWaiverActions =
        {
            EmailLienWaiver,
            PrintLienWaiver
        };

        public ProcessLienWaiverActionsAttribute()
            : base(ProcessLienWaiverActions, ProcessLienWaiverActions)
        {
        }
    }
}