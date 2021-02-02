using PX.Objects.PJ.Common.CacheExtensions;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.PJ.DrawingLogs.CR.CacheExtensions
{
    public sealed class NoteDocDrawingLogExt : PXCacheExtension<NoteDocExt, NoteDoc>
    {
        [PXString]
        [PXUIField(DisplayName = DrawingLogLabels.DrawingLogId, Enabled = false)]
        public string DrawingLogCd
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Drawing Number", Enabled = false)]
        public string Number
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "Revision", Enabled = false)]
        public string Revision
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class drawingLogCd : IBqlField
        {
        }

        public abstract class number : IBqlField
        {
        }

        public abstract class revision : IBqlField
        {
        }
    }
}