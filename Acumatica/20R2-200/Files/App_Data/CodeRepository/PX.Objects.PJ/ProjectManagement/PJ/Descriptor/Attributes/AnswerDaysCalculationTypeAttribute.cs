using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes
{
    public class AnswerDaysCalculationTypeAttribute : PXStringListAttribute, IPXFieldUpdatedSubscriber
    {
        public const string SequentialDays = "S";
        public const string BusinessDays = "B";
        public const string SequentialDaysLabel = "Sequential Days (incl. weekends)";
        public const string BusinessDaysLabel = "Business Days";

        public AnswerDaysCalculationTypeAttribute()
            : base(new[]
            {
                SequentialDays,
                BusinessDays
            }, new[]
            {
                SequentialDaysLabel,
                BusinessDaysLabel
            })
        {
        }

        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (args.Row is ProjectManagementSetup projectManagementSetup)
            {
                projectManagementSetup.CalendarId = null;
            }
        }
    }
}
