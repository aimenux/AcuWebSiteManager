using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PX.Data;
using PX.SM;

namespace PX.Objects.AM
{
    public static class NoteFileHelper
    {
        /// <summary>
        /// Get the attached files related to the given row
        /// </summary>
        public static List<UploadFile> GetDocumentFiles<DAC>(PXCache cache, DAC row) where DAC : IBqlTable
        {
            var list = new List<UploadFile>();
            if (row == null)
            {
                return list;
            }

            var noteID = PXNoteAttribute.GetNoteIDIfExists(cache, row);

            if (noteID == null)
            {
                return list;
            }

            foreach (UploadFile file in
                PXSelectJoin<UploadFile,
                    InnerJoin<NoteDoc, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
                    Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.
                Select(cache.Graph, noteID))
            {
                list.Add(file);
            }

            return list;
        }

        /// <summary>
        /// Get the file object which contains a matching file name
        /// </summary>
        public static UploadFile GetFileFromFileName<DAC>(PXCache cache, DAC row, string fileName)
            where DAC : IBqlTable
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            foreach (var file in GetDocumentFiles(cache, row))
            {
                if (file.Name != null && GetFileName(file.Name).EqualsWithTrim(fileName))
                {
                    return file;
                }
            }

            return null;
        }

        public static string GetFileNameFromFileName<DAC>(PXCache cache, DAC row, string fileName)
            where DAC : IBqlTable
        {
            return GetFileFromFileName(cache, row, fileName)?.Name;
        }

        public static string GetFileName(string fileName)
        {
            return string.IsNullOrWhiteSpace(fileName) || !fileName.Contains('\\') ? null : fileName.Split('\\').Last();
        }

        public static bool SaveFile(PXCache cache, object cacheRow, string fileName, StringBuilder stringData, bool createRevision)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new PXArgumentException(nameof(fileName));
            }

            if (stringData == null)
            {
                throw new PXArgumentException(nameof(stringData));
            }

            if (stringData.Length == 0)
            {
                return false;
            }

            return SaveFile(cache, cacheRow, fileName, Encoding.ASCII.GetBytes(stringData.ToString()), createRevision);
        }

        public static bool SaveFile(PXCache cache, object cacheRow, string fileName, byte[] data, bool createRevision)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new PXArgumentException(nameof(fileName));
            }

            if (data == null)
            {
                throw new PXArgumentException(nameof(data));
            }

            if (data.Length == 0)
            {
                return false;
            }

            try
            {
                var filegraph = PXGraph.CreateInstance<UploadFileMaintenance>();
                var fileinfo = new PX.SM.FileInfo(fileName, null, data);

                if (filegraph.SaveFile(fileinfo, createRevision ? FileExistsAction.CreateVersion : FileExistsAction.ThrowException) && fileinfo.UID.HasValue)
                {
                    PXNoteAttribute.SetFileNotes(cache, cacheRow, fileinfo.UID.Value);
                    return true;
                }    
            }
            catch (Exception e)
            {
                PXTrace.WriteWarning($"Unable to save file '{fileName}'. {e.Message}");
            }

            return false;
        }

        /// <summary>
        /// Write the content to a file in memory and download to the user
        /// </summary>
        public static PXRedirectToFileException CreateFileRedirect(string fileName, string content)
        {
            return CreateFileRedirect(fileName, content, true);
        }

        public static UTF8Encoding FileHelperEncoding => new UTF8Encoding(false);

        /// <summary>
        /// Write the content to a file in memory and download to the user
        /// </summary>
        public static PXRedirectToFileException CreateFileRedirect(string fileName, string content, bool forceDownload)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                PXTrace.WriteWarning(AM.Messages.GetLocal(AM.Messages.FileContentEmpty, fileName));
                //PXTrace.WriteWarning($"File {fileName} contents are empty");
            }

            using (var stream = new MemoryStream())
            using (var sw = new StreamWriter(stream, FileHelperEncoding))
            {
                sw.Write(content);
                sw.Flush();
                var fileInfo = new PX.SM.FileInfo(fileName, null, stream.ToArray());
                return new PXRedirectToFileException(fileInfo, forceDownload);
            }
        }

        /// <summary>
        /// Make sure the file name doesn't contain any invalid characters
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/questions/146134/how-to-remove-illegal-characters-from-path-and-filenames
        /// </remarks>
        public static string CleanFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();

            return new string(fileName
                .Where(x => !invalidChars.Contains(x))
                .ToArray());
        }

        /// <summary>
        /// Find the rough size of the file in KB
        /// </summary>
        public static double FileSizeKB(this PX.SM.FileInfo fileInfo)
        {
            return fileInfo?.BinData == null ? 0 : Math.Round(fileInfo.BinData.Length / 1024f, MidpointRounding.ToEven);
        }

        /// <summary>
        /// Convert file info content (BinData) to UTF8 string encoded value
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static string BinDataToString(this PX.SM.FileInfo fileInfo)
        {
            return fileInfo?.BinData == null ? null : FileHelperEncoding.GetString(fileInfo.BinData);
        }

        public static bool IsFileExtension(this PX.SM.FileInfo fileInfo, string ext)
        {
            return fileInfo?.FullName?.EndsWith($".{ext.Replace(".", string.Empty)}") ?? false;
        }
    }
}