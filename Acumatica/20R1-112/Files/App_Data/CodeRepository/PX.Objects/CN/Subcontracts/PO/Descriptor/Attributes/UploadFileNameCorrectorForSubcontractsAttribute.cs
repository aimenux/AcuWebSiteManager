using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PX.Data;
using PX.Objects.PO;
using PX.SM;

namespace PX.Objects.CN.Subcontracts.PO.Descriptor.Attributes
{
    public class UploadFileNameCorrectorForSubcontractsAttribute : PXEventSubscriberAttribute
    {
        private PXCache cache;
        private PXGraph graph;

        public override void CacheAttached(PXCache eventCache)
        {
            cache = eventCache;
            graph = eventCache.Graph;
            graph.FieldUpdating.AddHandler(typeof(POOrder), Messages.NoteFilesFieldName, UpdateFileNamesForCommitment);
            graph.FieldUpdating.AddHandler(typeof(POLine), Messages.NoteFilesFieldName,
                UpdateFileNamesForCommitmentLine);
        }

        private void UpdateFileNamesForCommitment(PXCache eventCache, PXFieldUpdatingEventArgs args)
        {
            UpdateFileNames<POOrder, POOrder.orderType>(eventCache, args);
        }

        private void UpdateFileNamesForCommitmentLine(PXCache eventCache, PXFieldUpdatingEventArgs args)
        {
            UpdateFileNames<POLine, POLine.orderType>(eventCache, args);
        }

        private void UpdateFileNames<TEntity, TField>(PXCache eventCache, PXFieldUpdatingEventArgs args)
            where TEntity : class
            where TField : IBqlField
        {
            var noteFileIds = args.NewValue as Guid[];
            var entity = args.Row as TEntity;
            var entityType = (string) eventCache.GetValue<TField>(entity);
            if (entityType == POOrderType.RegularSubcontract && noteFileIds?.Length > 0)
            {
                UpdateFileNames(noteFileIds);
            }
        }

        private void UpdateFileNames(IEnumerable<Guid> fileIds)
        {
            foreach (var uploadFile in GetUploadFiles(fileIds))
            {
                UpdateFileNameIfNeed(uploadFile);
            }
        }

        private void UpdateFileNameIfNeed(UploadFile uploadFile)
        {
            var newName = GetUploadFileName(uploadFile);
            if (newName != uploadFile.Name)
            {
                UpdateFileName(uploadFile.FileID, newName);
            }
        }

        private string GetUploadFileName(UploadFile uploadFile)
        {
            var shortFileName = Path.GetFileName(uploadFile.Name);
            var commitment = cache.Current as POOrder;
            return $"{PXSiteMap.CurrentNode.Title} ({commitment?.OrderNbr})\\{shortFileName}";
        }

        private void UpdateFileName(Guid? fileId, string name)
        {
            PXDatabase.Update<UploadFile>(
                new PXDataFieldAssign(nameof(UploadFile.Name), PXDbType.Text, name),
                new PXDataFieldRestrict(nameof(UploadFile.FileID), PXDbType.UniqueIdentifier, fileId));
        }

        private IEnumerable<UploadFile> GetUploadFiles(IEnumerable fileIds)
        {
            var query = new PXSelect<UploadFile,
                Where<UploadFile.fileID, In<Required<UploadFile.fileID>>>>(graph);
            return query.Select(fileIds).FirstTableItems;
        }
    }
}