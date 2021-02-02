using PX.Common;

namespace PX.Objects.CN.ProjectAccounting.Descriptor
{
    [PXLocalizable]
    public class ProjectAccountingMessages
    {
        public const string TaskTypeIsNotAvailable = "Task Type is not valid";

        public const string TaskTypeCannotBeChanged =
            "Task type cannot be changed. The Task is already used in at least one {0} related document.";

        public const string TaskCannotBeDeleted =
            "Cannot delete Task since it already has at least one Document associated with it.";
        
		public const string CostTaskTypeIsNotValid =
			"Project Task Type is not valid. Only Tasks of 'Cost Task' and 'Cost and Revenue Task' types are allowed.";

        public const string RevenueTaskTypeIsNotValid =
            "Project Task Type is not valid. Only Tasks of 'Revenue Task' and 'Cost and Revenue Task' types are allowed.";
    }
}