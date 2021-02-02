using PX.Common;
using PX.Objects.CN.ProjectAccounting.Descriptor;

namespace PX.Objects.PJ.DrawingLogs.Descriptor
{
    [PXLocalizable]
    public static class DrawingLogLabels
    {
        public const string Project = "Project";
        public const string DrawingLogId = "Drawing Log ID";
        public const string OriginalDrawingId = "Original Drawing";
        public const string Discipline = "Discipline";
        public const string Status = "Status";

        [PXLocalizable]
        public static class DisciplinesOrderedSelect
        {
            public const string PasteLine = "PasteLine";
            public const string ResetOrder = "ResetOrder";
        }
    }
}