using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PX.Common;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.SM;
using FileInfo = PX.SM.FileInfo;

namespace PX.Objects.PJ.Common.Services
{
    public abstract class BaseZipService<TDocument>
    {
        protected readonly PXCache Cache;

        protected BaseZipService(PXCache cache)
        {
            Cache = cache;
        }

        protected abstract string ZipFileName
        {
            get;
        }

        protected abstract string NoDocumentsSelectedMessage
        {
            get;
        }

        protected abstract string NoAttachedFilesInDocumentMessage
        {
            get;
        }

        protected abstract string NoAttachedFilesInDocumentsMessage
        {
            get;
        }

        public List<Guid> GetDocumentFileIds(List<TDocument> documents)
        {
            ValidateAnyDocumentsExists(documents, NoDocumentsSelectedMessage);
            var fileIds = GetFileIdsOfDocuments(documents).ToList();
            ValidateAnyAttachedFileExists(fileIds, NoAttachedFilesInDocumentsMessage);
            return fileIds;
        }

        public virtual void DownloadZipFile(TDocument document)
        {
            var fileIds = GetFileIdsIfAttachedExist(document);
            CreateZipArchive(fileIds, AddDocumentFilesToZipArchive);
        }

        public FileInfo GetZipFile(TDocument document)
        {
            var fileIds = GetFileIdsIfAttachedExist(document);
            return GetZipArchive(fileIds, AddDocumentsFilesToZipArchive, ZipFileName);
        }

        public void DownloadZipFile(List<Guid> fileIds)
        {
            CreateZipArchive(fileIds, AddDocumentsFilesToZipArchive);
        }

        public static FileInfo GetZipFile(List<Guid> fileIds, string fileName)
        {
            return GetZipArchive(fileIds, AddDocumentsFilesToZipArchive, fileName);
        }

        protected void CreateZipArchive(List<Guid> fileIds, Action<ZipArchive, List<Guid>> addFilesToZipArchive)
        {
            var zipFile = GetZipArchive(fileIds, addFilesToZipArchive, ZipFileName);
            throw new PXRedirectToFileException(zipFile, true);
        }

        protected static FileInfo GetZipArchive(List<Guid> fileIds, Action<ZipArchive, List<Guid>> addFilesToZipArchive, string fileName)
        {
	        using (var memoryStream = new MemoryStream())
	        {
		        using (var zipArchive = ZipArchive.CreateFrom(memoryStream, false))
		        {
			        addFilesToZipArchive(zipArchive, fileIds);
		        }
		        return new FileInfo(fileName, null, memoryStream.ToArray());
			}
        }

        protected abstract IEnumerable<Guid> GetFileIdsOfDocument(TDocument document);

        protected IEnumerable<Guid> GetFileIdsOfDocuments(IEnumerable<TDocument> documents)
        {
            return documents.SelectMany(GetFileIdsOfDocument);
        }

        protected static void AddDocumentsFilesToZipArchive(ZipArchive zipArchive, List<Guid> fileIds)
        {
            var fileNames = GetFileNamesUnique(fileIds);
            var graph = PXGraph.CreateInstance<UploadFileMaintenance>();
            foreach (var (id, fileName) in fileNames)
            {
                var fileData = graph.GetFile(id);
                zipArchive.AddFile(fileName, fileData.BinData);
            }
        }

        protected static void AddDocumentFilesToZipArchive(ZipArchive zipArchive, List<Guid> fileIds)
        {
            var graph = PXGraph.CreateInstance<UploadFileMaintenance>();
            foreach (var file in fileIds.Select(fileId => graph.GetFile(fileId)))
            {
                zipArchive.AddFile(file.Name, file.BinData);
            }
        }

        protected static void ValidateAnyAttachedFileExists(IEnumerable<Guid> fileIds, string validationMessage)
        {
            if (fileIds.IsEmpty())
            {
                throw new Exception(validationMessage);
            }
        }

        protected static void ValidateAnyDocumentsExists(IEnumerable<TDocument> documents, string message)
        {
            if (documents.IsEmpty())
            {
                throw new Exception(message);
            }
        }

        protected List<Guid> GetFileIdsIfAttachedExist(TDocument document)
        {
            var fileIds = GetFileIdsOfDocument(document).ToList();
            ValidateAnyAttachedFileExists(fileIds, NoAttachedFilesInDocumentMessage);
            return fileIds;
        }

        private static IEnumerable<(Guid id, string name)> GetFileNamesUnique(IEnumerable<Guid> fileIds)
        {
            var fileMaintenanceGraph = PXGraph.CreateInstance<UploadFileMaintenance>();
            var fileNames = (from fileId in fileIds
                let fileName = fileMaintenanceGraph.GetFileWithNoData(fileId).Name
                select (fileId, fileName)).ToList();
            return UpdateFileNamesIfNeeded(fileNames);
        }

        private static IEnumerable<(Guid id, string name)> UpdateFileNamesIfNeeded(List<(Guid id, string name)> fileNames)
        {
            var fileNameDuplicateGroups = fileNames.GroupBy(f => f.name).Where(g => g.Count() > 1).ToList();
            if (fileNameDuplicateGroups.IsEmpty())
            {
                return fileNames;
            }
            foreach (var fileNameDuplicateGroup in fileNameDuplicateGroups)
            {
                RenameDuplicates(fileNames, fileNameDuplicateGroup);
            }
            return UpdateFileNamesIfNeeded(fileNames);
        }

        private static void RenameDuplicates(List<(Guid id, string name)> fileNames,
            IGrouping<string, (Guid id, string name)> fileNameDuplicateGroup)
        {
            var firstFile = fileNameDuplicateGroup.First().name;
            var name = Path.GetFileNameWithoutExtension(firstFile);
            var extension = Path.GetExtension(firstFile);
            var index = 1;
            var match = Regex.Match(name, Descriptor.Constants.DocumentCopyNamePattern);
            if (match.Success)
            {
                name = name.Remove(match.Index - 1);
                index = int.Parse(match.Groups[Descriptor.Constants.DocumentCopyIndexName].Value) + 1;
            }
            foreach (var fileIndex in fileNameDuplicateGroup.Skip(1).ToList()
                .Select(fileId => fileNames.FindIndex(f => f.name == fileId.name && f.id == fileId.id)))
            {
                var fileId = fileNames[fileIndex].id;
                fileNames[fileIndex] = (fileId, $"{name} ({index}){extension}");
                index++;
            }
        }
    }
}