using System.Collections;
using PX.Objects.PJ.Common.GraphExtensions;
using PX.Objects.PJ.RequestsForInformation.CR.CacheExtensions;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.DrawingLogs.CR.GraphExtensions
{
    public class CrEmailActivityMaintDrawingLogExt : PXGraphExtension<CrEmailActivityMaintExt, CREmailActivityMaint>
    {
        [PXOverride]
        public IEnumerable send(PXAdapter adapter, CrEmailActivityMaintExt.SendDelegate baseMethod)
        {
            if (Base1.IsDrawingLogEmail())
            {
                Base.Persist();
                var graph = PXGraph.CreateInstance<UploadFileMaintenance>();
                Base1.EmailFileAttachService.AttachDrawingLogArchive(graph);
            }
            return baseMethod(adapter);
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }
    }
}