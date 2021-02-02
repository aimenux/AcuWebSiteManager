using System;
using System.Linq;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PM.CacheExtensions;
using PX.Data;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CR;
using PX.Objects.PM;
using PX.Objects.PM.ChangeRequest;
using Messages = PX.Objects.CR.Messages;

namespace PX.Objects.PJ.ProjectManagement.PJ.Graphs
{
    public abstract class ProjectManagementBaseMaint<TGraph, TProjectManagementEntity>
        : PXGraph<TGraph, TProjectManagementEntity>
        where TGraph : ProjectManagementBaseMaint<TGraph, TProjectManagementEntity>
        where TProjectManagementEntity : class, IBqlTable, IProjectManagementDocumentBase, new()
    {
        public CRAttributeList<TProjectManagementEntity> Attributes;
        public PXSelect<TProjectManagementEntity> ProjectManagementEntity;

        [PXViewName(Messages.Activities)]
        [PXFilterable]
        public ProjectManagementActivityList<TProjectManagementEntity> Activities;

        [PXUIField(DisplayName = "Convert to Change Request", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual void convertToChangeRequest()
        {
            Persist();
            CheckChangeOrderWorkflow();
            var graph = CreateInstance<ChangeRequestEntry>();
            graph.Document.Current = CreateChangeRequest(graph);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
        }

        public virtual void _(Events.RowSelected<TProjectManagementEntity> args)
        {
            Activities.CurrentProjectManagementEntityNoteId = Activities.CurrentProjectManagementEntityNoteId ?? (Caches[typeof(TProjectManagementEntity)].Current as BaseCache)?.NoteID;
        }

        protected void SetActivityDefaultSubject(string subjectPattern, params object[] subjectFields)
        {
            Activities.DefaultSubject = PXMessages.LocalizeFormatNoPrefixNLA(subjectPattern, subjectFields);
        }

        protected virtual PMChangeRequest CreateChangeRequest(ChangeRequestEntry graph)
        {
            var changeRequest = graph.Document.Insert();
            changeRequest.ProjectID = ProjectManagementEntity.Current.ProjectId;
            var extension = PXCache<PMChangeRequest>.GetExtension<PmChangeRequestExtension>(changeRequest);
            extension.ConvertedFrom = ProjectManagementEntity.Current.GetType().Name;
            return changeRequest;
        }

        private void CheckChangeOrderWorkflow()
        {
            var project = Select<PMProject>().SingleOrDefault(p => p.ContractID == ProjectManagementEntity.Current.ProjectId);
            if (project?.ChangeOrderWorkflow != true)
            {
                throw new Exception(ProjectManagementMessages.EnableChangeOrderWorkflowForTheProject);
            }
        }
    }
}