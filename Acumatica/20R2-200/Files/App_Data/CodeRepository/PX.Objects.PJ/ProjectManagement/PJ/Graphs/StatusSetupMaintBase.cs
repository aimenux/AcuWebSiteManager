using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Common;
using PX.Data;

namespace PX.Objects.PJ.ProjectManagement.PJ.Graphs
{
    public abstract class StatusSetupMaintBase<TGraph, TDocumentWithConfigurableStatus, TStatus> : PXGraph<TGraph>
        where TGraph : PXGraph<TGraph>, new()
        where TDocumentWithConfigurableStatus : class, IBqlTable, IDocumentWithConfigurableStatus, new()
        where TStatus : class, IStatus, IBqlTable, new()
    {
        protected abstract PXSelectBase<TStatus> Statuses
        {
            get;
        }

        protected abstract string DocumentName
        {
            get;
        }

        public virtual void _(Events.RowDeleting<TStatus> args)
        {
            if (args.Row.IsDefault.GetValueOrDefault())
            {
                throw new Exception(ProjectManagementMessages.SystemRecordCannotBeDeleted);
            }
            if (GetDocuments(args.Row.StatusId).Any() && !IsDeletionConfirmed(args.Row))
            {
                args.Cancel = true;
            }
        }

        public virtual void _(Events.RowPersisting<TStatus> args)
        {
            if (args.Operation == PXDBOperation.Delete)
            {
                UpdateDocumentStatus(args.Row.StatusId);
            }
        }

        protected abstract IEnumerable<TDocumentWithConfigurableStatus> GetDocuments(int? statusId);

        protected abstract TStatus GetDefaultStatus();

        private void UpdateDocumentStatus(int? statusId)
        {
            var documents = GetDocuments(statusId);
            var defaultStatusId = GetDefaultStatus().StatusId;
            documents.ForEach(doc => UpdateDocumentStatus(doc, defaultStatusId));
            this.Caches<TDocumentWithConfigurableStatus>().Persist(PXDBOperation.Update);
        }

        private void UpdateDocumentStatus(TDocumentWithConfigurableStatus document, int? defaultStatusId)
        {
            document.StatusId = defaultStatusId;
            this.Caches<TDocumentWithConfigurableStatus>().Update(document);
        }

        private bool IsDeletionConfirmed(TStatus documentStatus)
        {
            return Statuses.Ask(PX.Objects.CN.Common.Descriptor.SharedMessages.Warning,
                string.Format(ProjectManagementMessages.AssignDefaultStatus, DocumentName, documentStatus.Name,
                    GetDefaultStatus().Name), MessageButtons.YesNo).IsPositive();
        }
    }
}