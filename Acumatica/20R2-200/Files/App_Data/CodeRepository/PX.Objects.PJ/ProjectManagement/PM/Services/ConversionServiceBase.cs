using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;

namespace PX.Objects.PJ.ProjectManagement.PM.Services
{
    public abstract class ConversionServiceBase
    {
        protected ChangeRequestEntry Graph;

        protected ConversionServiceBase(ChangeRequestEntry graph)
        {
            Graph = graph;
        }

        public void ProcessConvertedChangeRequestIfRequired(PMChangeRequest changeRequest)
        {
            if (IsNewChangeRequest(changeRequest) && !Graph.HasErrors())
            {
                ProcessConvertedChangeRequest(changeRequest);
            }
        }

        public abstract void UpdateConvertedEntity(PMChangeRequest changeRequest);

        public abstract void SetFieldReadonly(PMChangeRequest changeRequest);

        protected abstract void ProcessConvertedChangeRequest(PMChangeRequest changeRequest);

        protected void CopyFilesToChangeRequest<TTable>(object row, PMChangeRequest changeRequest)
            where TTable : class, IBqlTable, new()
        {
            PXNoteAttribute.CopyNoteAndFiles(Graph.Caches<TTable>(), row, Graph.Document.Cache, changeRequest, false,
                true);
            Graph.Caches<NoteDoc>().Persist(PXDBOperation.Insert);
        }

        protected void SetFieldReadOnly<TField>(PMChangeRequest changeRequest)
            where TField : IBqlField
        {
            PXUIFieldAttribute.SetEnabled<TField>(Graph.Document.Cache, changeRequest, false);
        }

        private bool IsNewChangeRequest(PMChangeRequest changeRequest)
        {
            return Graph.Document.Cache.GetStatus(changeRequest) == PXEntryStatus.Inserted;
        }
    }
}
