using System.Collections;
using PX.Objects.PJ.Common.GraphExtensions;
using PX.Objects.PJ.Submittals.PJ.Services;
using PX.Data;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.PJ.Submittals.CR.GraphExtensions
{
    public class CrEmailActivityMaintSubmittalsExt : PXGraphExtension<CrEmailActivityMaintExt,
        CREmailActivityMaint>
    {
        [PXOverride]
        public IEnumerable send(PXAdapter adapter, CrEmailActivityMaintExt.SendDelegate baseMethod)
        {
            if (Base1.IsSubmittalEmail())
            {
                Base.Persist();

                var refNoteId = Base.Message.Current.RefNoteID;
                PJSubmittal document = (PJSubmittal)new EntityHelper(Base).GetEntityRow(refNoteId);

                var graph = PXGraph.CreateInstance<UploadFileMaintenance>();
                ((SubmittalEmailAttachService) Base1.EmailFileAttachService).GenerateAndAttachReport(document, graph);
            }
            return baseMethod(adapter);
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }
    }
}