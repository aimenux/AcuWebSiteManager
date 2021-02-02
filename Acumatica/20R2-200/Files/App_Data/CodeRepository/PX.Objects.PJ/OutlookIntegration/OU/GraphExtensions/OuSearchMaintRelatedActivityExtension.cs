using System;
using System.Linq;
using PX.Objects.PJ.OutlookIntegration.CR.GraphExtensions;
using PX.Objects.PJ.OutlookIntegration.OU.CacheExtensions;
using PX.Objects.PJ.OutlookIntegration.OU.Descriptor;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.OutlookIntegration.OU.GraphExtensions
{
    public class OuSearchMaintRelatedActivityExtension : PXGraphExtension<OuSearchMaintExtensionBase, OUSearchMaint>
    {
        private OUActivity CurrentActivity => Base.NewActivity.Current;

        private OuActivityExtension OutlookActivityExtension =>
            PXCache<OUActivity>.GetExtension<OuActivityExtension>(CurrentActivity);

        private string DocumentName
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        // Acuminator disable once PX1000 InvalidPXActionHandlerSignature [Justification]
        [PXOverride]
        public virtual void createActivity(Action baseHandler)
        {
            if (OutlookActivityExtension.IsLinkProject == true)
            {
                DocumentName = OutlookIntegrationMessages.Project;
                Base1.CreateEntity(CreateProjectActivity);
            }
            else if (OutlookActivityExtension.IsLinkRequestForInformation == true)
            {
                DocumentName = OutlookIntegrationMessages.RequestForInformation;
                Base1.CreateEntity(CreateProjectManagementActivity<RequestForInformation,
                    RequestForInformation.requestForInformationId>);
            }
            else if (OutlookActivityExtension.IsLinkProjectIssue == true)
            {
                DocumentName = OutlookIntegrationMessages.ProjectIssue;
                Base1.CreateEntity(CreateProjectManagementActivity<ProjectIssue,
                    ProjectIssue.projectIssueId>);
            }
            else
            {
                baseHandler.Invoke();
            }
        }

        public virtual void _(Events.RowSelected<OUSearchEntity> args, PXRowSelected baseHandler)
        {
            baseHandler?.Invoke(args.Cache, args.Args);
            if (args.Row is OUSearchEntity searchEntity)
            {
                var isCaseCdFieldVisible = searchEntity.Operation == OUOperation.Activity &&
                    CurrentActivity.IsLinkOpportunity != true &&
                    OutlookActivityExtension.IsLinkProject != true &&
                    OutlookActivityExtension.IsLinkRequestForInformation != true &&
                    OutlookActivityExtension.IsLinkProjectIssue != true;
                PXUIFieldAttribute.SetVisible<OUActivity.caseCD>(Base.NewActivity.Cache,
                    CurrentActivity, isCaseCdFieldVisible);
            }
        }

        public virtual void _(Events.RowSelected<OUActivity> args)
        {
            if (args.Row is OUActivity activity)
            {
                DisableDocumentCheckBoxIfNeeded<PMProject, OuActivityExtension.isLinkProject>(args.Cache, activity);
                DisableDocumentCheckBoxIfNeeded<ProjectIssue, OuActivityExtension.isLinkProjectIssue>(
                    args.Cache, activity);
                DisableDocumentCheckBoxIfNeeded<RequestForInformation, OuActivityExtension.isLinkRequestForInformation>(
                    args.Cache, activity);
            }
        }

        private void DisableDocumentCheckBoxIfNeeded<TTable, TCheckboxField>(PXCache cache, OUActivity activity)
            where TTable : class, IBqlTable, new()
            where TCheckboxField : IBqlField
        {
            var doesAnyDocumentExist = SelectFrom<TTable>.View.Select(Base).Any();
            PXUIFieldAttribute.SetEnabled<TCheckboxField>(cache, activity, doesAnyDocumentExist);
        }

        private void CreateProjectManagementActivity<TTable, TDocumentIdField>()
            where TTable : class, IBqlTable, new()
            where TDocumentIdField : BqlInt.Field<TDocumentIdField>
        {
            var documentId = Base.NewActivity.Cache.GetValue<TDocumentIdField>(CurrentActivity) as int?;
            if (GetDocument<TTable, TDocumentIdField>(documentId)
                is BaseCache projectManagementDocument && !Base.HasErrors())
            {
                Base.PersisMessage(projectManagementDocument.NoteID, null, null);
            }
            else
            {
                throw new Exception(string.Format(
                    OutlookIntegrationMessages.TheEntityWasNotFoundInAcumatica, DocumentName));
            }
        }

        private void CreateProjectActivity()
        {
            var project = GetDocument<PMProject, PMProject.contractID>(OutlookActivityExtension.ProjectId);
            if (project != null && !Base.HasErrors())
            {
                Base.PersisMessage(project.NoteID, null, null);
                CreateProjectTimeActivity(project.ContractID);
            }
            else
            {
                throw new Exception(string.Format(
                    OutlookIntegrationMessages.TheEntityWasNotFoundInAcumatica, DocumentName));
            }
        }

        /// <summary>
        /// Creates <see cref="PMTimeActivity"/> in order to email be displayed on Project Activities tab.
        /// Deletes all previously created activities linked to this email.
        /// <see cref="PMTimeActivity"/> is created implicitly on <see cref="CRSMEmail"/> inserting.
        /// </summary>
        private void CreateProjectTimeActivity(int? projectId)
        {
            var email = GetProjectEmail();
            var graph = PXGraph.CreateInstance<CREmailActivityMaint>();
            DeletePreviouslyLinkedActivities(graph, email);
            graph.Message.Insert(email);
            graph.TimeActivity.Current.ProjectID = projectId;
            graph.TimeActivity.Current.IsBillable = false;
            graph.TimeActivity.Cache.Persist(graph.TimeActivity.Current, PXDBOperation.Insert);
        }

        private CRSMEmail GetProjectEmail()
        {
            var messageId = Base.SourceMessage.Current.MessageId;
            var emailReferenceNoteId = GetSystemEmailReferenceNoteId(messageId);
            return GetActivityEmail(emailReferenceNoteId);
        }

        private static void DeletePreviouslyLinkedActivities(PXGraph graph, INotable email)
        {
            var extension = graph.GetExtension<CrEmailActivityMaintExtension>();
            var activitiesToDelete = extension.TimeActivities.SelectMain()
                .Where(ta => ta.RefNoteID == email.NoteID);
            extension.TimeActivities.Cache.DeleteAll(activitiesToDelete);
            extension.TimeActivities.Cache.Persist(PXDBOperation.Delete);
        }

        private TTable GetDocument<TTable, TDocumentIdField>(int? documentId)
            where TTable : class, IBqlTable, new()
            where TDocumentIdField : BqlInt.Field<TDocumentIdField>
        {
            return SelectFrom<TTable>
                .Where<BqlInt.Field<TDocumentIdField>.IsEqual<P.AsInt>>.View.SelectSingleBound(Base, null, documentId);
        }

        private CRSMEmail GetActivityEmail(Guid? refNoteId)
        {
            return SelectFrom<CRSMEmail>
                .Where<CRSMEmail.noteID.IsEqual<P.AsGuid>>.View.Select(Base, refNoteId);
        }

        private Guid? GetSystemEmailReferenceNoteId(string messageId)
        {
            SMEmail email = SelectFrom<SMEmail>
                .Where<SMEmail.messageId.IsEqual<P.AsString>>.View.Select(Base, messageId);
            return email.RefNoteID;
        }
    }
}