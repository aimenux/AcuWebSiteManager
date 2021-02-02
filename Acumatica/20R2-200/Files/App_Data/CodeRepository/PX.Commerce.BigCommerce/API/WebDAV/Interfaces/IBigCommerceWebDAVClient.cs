using System;
using System.Collections.Generic;
using PX.SM;
using WebDAVClient.Model;

namespace PX.Commerce.BigCommerce.API.WebDAV
{
    public interface IBigCommerceWebDAVClient
    {
        Uri ServerHttps { get; set; }
        Uri ServerHttp { get; set; }
        string BasePath { get; set; }
        Item ContentFolder { get; set; }

        IEnumerable<Item> List(string path = "/", int? depth = 1);
        FileInfo Download(string remoteFilePath);
        T Upload<T>(byte[] byteData, string name) where T : IWebDAVEntity, new();

        Item GetFolder(string path = "/");
        Item GetFile(string path = "/");
        IEnumerable<Item> GetServerFilesList();

        bool CreateDir(string remotePath, string name);
        void DeleteFolder(string href);
        void DeleteFile(string href);
        bool MoveFolder(string srcFolderPath, string dstFolderPath);
        bool MoveFile(string srcFilePath, string dstFilePath);
    }
}
