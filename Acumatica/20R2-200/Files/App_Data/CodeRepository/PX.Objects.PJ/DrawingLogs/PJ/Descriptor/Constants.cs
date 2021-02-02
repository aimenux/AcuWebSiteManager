namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor
{
    public class Constants
    {
        public const string DrawingLogsZipFileName = "Drawing Logs.zip";
        public const string DrawingLogClassId = "DRAWINGLOGS";
        public const string DisciplineNameField = "Name";
        public const string DisciplineSortOrderField = "SortOrder";
        public const string DrawingLogCdSearchPattern = "DL-";

        public static string DisciplineViewName = string.Concat("_Cache#PX.Objects.PJ.DrawingLogs.PJ.DAC",
            ".DrawingLog_DisciplineId_PX.Objects.PJ.DrawingLogs.PJ.DAC.DrawingLogDiscipline+drawingLogDisciplineId_");

        public static string DisciplineFilterViewName = string.Concat("_Cache#PX.Objects.PJ.DrawingLogs.PJ.DAC",
            ".DrawingLogFilter_DisciplineId_PX.Objects.PJ.DrawingLogs.PJ.DAC.DrawingLogDiscipline+drawingLogDisciplineId_");
    }
}
