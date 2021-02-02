using PX.Web.UI;

namespace PX.Objects.PJ.Common.Descriptor
{
    public static class Constants
    {
        public const string PriorityIconHeaderImage = Sprite.AliasControl + "@" + Sprite.Control.PriorityHead;
        public const string DocumentCopyNamePattern = @"\((?<index>\d+)\)$";
        public const string DocumentCopyIndexName = "index";
        public const string FilesDateFormat = "MM-dd-yyyy";

        public static class ReportIds
        {
            public const string BudgetForecast = "PJ629600";
        }
    }
}