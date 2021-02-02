using PX.Api;
using PX.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using WebDAVClient.Model;
using FileInfo = PX.SM.FileInfo;

namespace PX.Commerce.BigCommerce.API.WebDAV
{
	public class BCWebDavClient : IBigCommerceWebDAVClient
	{
		private const string SLASH = "/";

		public Uri ServerHttps { get; set; }
		public Uri ServerHttp { get; set; }
		public string BasePath { get; set; }
		public Item ContentFolder { get; set; }

		private NetworkCredential Credential { get; }
		private WebDAVClient.Client Client { get; }

		public BCWebDavClient(IWebDAVOptions options)
		{
			// init credential
			Credential = new NetworkCredential
			{
				UserName = options.ClientUser,
				Password = options.ClientPassword
			};

			// init client and Paths
			Client = new WebDAVClient.Client(Credential);
			ServerHttps = new Uri(options.ServerHttpsUri);
			ServerHttp = new Uri(ServerHttps.AbsoluteUri.Replace("https", "http"));
			BasePath = GetBasePath(ServerHttps.Segments[0], ServerHttps.Segments[1]);

			// change server https uri
			options.ServerHttpsUri = ServerHttps.AbsoluteUri.Replace(ServerHttps.AbsolutePath, "/");

			// set paths to server
			Client.Server = options.ServerHttpsUri;
			Client.BasePath = BasePath;

			// set content folder
			var folder = List().FirstOrDefault(f => f.Href.EndsWith("/content/"));
			ContentFolder = folder ?? new Item { Href = SLASH, DisplayName = string.Empty };
		}

		public IEnumerable<Item> List(string path = SLASH, int? depth = 1)
		{
			try
			{
				return Client.List(path, depth).Result;
			}
			catch (Exception ex)
			{
				throw new PXException(BigCommerceMessages.TestConnectionFolderNotFound, ex);
			}
		}

		public void DeleteFile(string href)
		{
			Client?.DeleteFile(href).Wait();
		}

		public Item GetFolder(string path = SLASH)
		{
			return Client.GetFolder(path).Result;
		}

		public T Upload<T>(byte[] byteData, string name) where T : IWebDAVEntity, new()
		{
			using (var fileStream = new MemoryStream())
			{
				fileStream.Write(byteData, 0, byteData.Length);
				fileStream.Flush();
				fileStream.Seek(0, SeekOrigin.Begin);

				Client?.Upload(ContentFolder.Href, fileStream, name).Wait();

			}

			var result = new T
			{
				ImageUrl = Uri.EscapeUriString(GetImageUrl(name)),
				ImageFile = name,
			};

			return result;
		}

		public bool CreateDir(string remotePath, string name)
		{
			return Client.CreateDir(remotePath, name).Result;
		}

		public FileInfo Download(string remoteFilePath)
		{
			var stream = Client.Download(remoteFilePath).Result;
			var fileName = string.IsNullOrEmpty(remoteFilePath.Trim()) || !remoteFilePath.Contains(".") ? string.Empty : Path.GetFileName(new Uri(remoteFilePath).AbsolutePath);
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return new FileInfo(fileName, null, ms.ToArray());
			}
		}

		private string GetImageUrl(string name)
		{
			return ServerHttp.AbsoluteUri.Replace(ServerHttp.AbsolutePath, "/")
				   + (ContentFolder.DisplayName.IsNullOrEmpty() ?
					   "" :
					   ContentFolder.DisplayName + SLASH)
				   + name;
		}

		private string GetBasePath(string segment0, string segment1)
		{
			return segment0.IsNullOrEmpty() ?
				SLASH :
				segment0
				  + (segment1.IsNullOrEmpty() ?
					  String.Empty :
					  segment1.EndsWith(SLASH) ?
						  segment1 :
						  segment1
						  + SLASH);
		}

		public IEnumerable<Item> GetServerFilesList()
		{
			var folder = ContentFolder;

			var folderReloaded = GetFolder(folder.Href);
			var folderNodes = List(folderReloaded.Href);
			return folderNodes.Where(f => f.IsCollection == false);
		}

		#region Not implemented

		public Item GetFile(string path = "/")
		{
			throw new NotImplementedException();
		}

		public void DeleteFolder(string href)
		{
			throw new NotImplementedException();
		}

		public bool MoveFolder(string srcFolderPath, string dstFolderPath)
		{
			throw new NotImplementedException();
		}

		public bool MoveFile(string srcFilePath, string dstFilePath)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
