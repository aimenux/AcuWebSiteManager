using System;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class ImageExtractorEmailProcessor : BasicEmailProcessor
	{
		protected override bool Process(Package package)
		{
			EMailAccount account = package.Account;
			CRSMEmail message = package.Message;
			PXGraph graph = package.Graph;

			if (message.Body == null)
				return true;

			if (message.NoteID == null)
				return false;

			Guid noteID = message.NoteID.Value;
			var extractor = new PX.Data.ImageExtractor();
			var body = message.Body;

			if (extractor.ExtractEmbedded(body, img => AddExtractedImage(graph, noteID, img),
				out var newBody, out var _))
			{
				message.Body = newBody;
			}

			return true;
		}

		private (string src, string title) AddExtractedImage(PXGraph graph, Guid noteId, PX.Data.ImageExtractor.ImageInfo img)
		{
			CreateFile(graph, noteId, img.ID, img.Name, img.Bytes);

			string url = string.Format("~/Frames/GetFile.ashx?fileID={0}", img.CID);
			return (url, img.Name);
		}

		private void CreateFile(PXGraph graph, Guid noteId, Guid newFileId, string name, byte[] content)
		{
			var noteDocCache = graph.Caches[typeof(NoteDoc)];
			var noteDoc = (NoteDoc)noteDocCache.CreateInstance();
			noteDoc.NoteID = noteId;
			noteDoc.FileID = newFileId;
			noteDocCache.Insert(noteDoc);
			graph.EnsureCachePersistence(typeof(NoteDoc));

			var uploadFileCache = graph.Caches[typeof(CommonMailReceiveProvider.UploadFile)];
			var uploadFile = (CommonMailReceiveProvider.UploadFile)uploadFileCache.CreateInstance();
			uploadFile.FileID = newFileId;
			uploadFile.LastRevisionID = 1;
			uploadFile.Versioned = true;
			uploadFile.IsPublic = false;
			if (name.Length > 200) name = name.Substring(0, 200);
			uploadFile.Name = newFileId + @"\" + name;
			uploadFile.PrimaryScreenID = "CR306015"; //TODO: need review
			uploadFileCache.Insert(uploadFile);
			graph.EnsureCachePersistence(typeof(CommonMailReceiveProvider.UploadFile));

			var fileRevisionCache = graph.Caches[typeof(UploadFileRevision)];
			var fileRevision = (UploadFileRevision)fileRevisionCache.CreateInstance();
			fileRevision.FileID = newFileId;
			fileRevision.FileRevisionID = 1;
			fileRevision.Data = content;
			fileRevision.Size = UploadFileHelper.BytesToKilobytes(content.Length);
			fileRevisionCache.Insert(fileRevision);
			graph.EnsureCachePersistence(typeof(UploadFileRevision));
		}
	}
}
