using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Data;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CS;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [PXPrimaryGraph(typeof(DrawingLogsSetupMaint))]
    [PXCacheName(CacheNames.DrawingLogPreference)]
    public class DrawingLogSetup : BaseCache, IBqlTable
    {
        [PXDBString(10, IsUnicode = true)]
        [PXDefault("DRAWINGLOG")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Drawing Log Numbering Sequence")]
        public string DrawingLogNumberingSequenceId
        {
            get;
            set;
        }

        public abstract class drawingLogNumberingSequenceId : IBqlField
        {
        }
    }
}