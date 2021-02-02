using PX.Common;

namespace PX.Objects.PJ.DailyFieldReports.Descriptor
{
    [PXLocalizable]
    public static class DailyFieldReportMessages
    {
        public const string ThereAreRelatedEntitiesToDfrOnDelete = "This daily field report cannot be deleted because it has at least one related {0}.";
        public const string DepartureTimeMustBeLaterThanArrivalTime = "The departure time must be later than the arrival time.";
        public const string ValueMustBePositive = "Value must be positive.";
        public const string WorkingHoursCannotExceedDefaultValue = "Working Hours cannot exceed {0}.";
        public const string EntityCannotBeSelectedTwice = "You have already selected this {0}.";

        public const string DfrDateMustNotBeEarlierThenProjectStartDate =
            "The date must not be earlier than the project start date.";

        public const string ThereIsOneOrMoreEntitiesRelatedToTheProjectOnTheTab =
            "The project cannot be changed because at least one {0} is linked to this project on the {1} tab.";

        public const string ThisEquipmentIsAssociatedWithSubmittedTimeCard =
            "This equipment utilization record is associated with submitted time card. Use the Equipment Time Card form to make changes to this record.";

        public const string EntityCannotBeDeletedBecauseItIsLinked =
            "{0} cannot be deleted because it is linked to the daily field report.";

        public const string TheFileIsReferredToTheDailyFieldReport =
            "The file is referred to the Daily Field Report and cannot be deleted.";

        public const string ThisIsRequiredSettingForDailyFieldReport =
            "This is a required setting for daily field reports. A user will have to fill it in manually for a new copy of the document.";
    }
}