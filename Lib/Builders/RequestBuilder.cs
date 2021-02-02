using System.IO;
using System.Linq;
using System.Xml.Linq;
using Lib.Helpers;
using Lib.Models;

namespace Lib.Builders
{
    public class RequestBuilder : IRequestBuilder
    {
        private readonly IXmlHelper _xmlHelper;

        public RequestBuilder(IXmlHelper xmlHelper)
        {
            _xmlHelper = xmlHelper;
        }

        public Request Build(string configXmlFile, string configExeFile = null)
        {
            var request = new Request
            {
                ConfigXmlFile = configXmlFile
            };

            if (!_xmlHelper.TryLoadXmlFile(configXmlFile, out var xmlDoc))
            {
                return request;
            }

            const string attribute = "Value";
            const string userPassXpath = @"Root/upass";
            const string appPoolNameXpath = @"Root/spool";
            const string siteDirNameXpath = @"Root/iname";
            const string siteDirPathXpath = @"Root/ipath";
            const string serverNameXpath = @"Root/dbsrvname";
            const string databaseNameXpath = @"Root/dbname";
            const string siteVDirNameXpath = @"Root/svirtdir";

            request.RootDirectory = GetRootDirectory(xmlDoc);
            request.ConfigExeArguments = $"-f:\"{configXmlFile}\"";
            request.ConfigExeFile = GetConfigExeFile(xmlDoc, configXmlFile, configExeFile);
            request.ServerName = _xmlHelper.GetAttributeValue(xmlDoc, serverNameXpath, attribute);
            request.AppPoolName = _xmlHelper.GetAttributeValue(xmlDoc, appPoolNameXpath, attribute);
            request.DatabaseName = _xmlHelper.GetAttributeValue(xmlDoc, databaseNameXpath, attribute);
            request.SiteDirectoryName = _xmlHelper.GetAttributeValue(xmlDoc, siteDirNameXpath, attribute);
            request.SiteDirectoryPath = _xmlHelper.GetAttributeValue(xmlDoc, siteDirPathXpath, attribute);
            request.SiteVirtualDirectoryName = _xmlHelper.GetAttributeValue(xmlDoc, siteVDirNameXpath, attribute);
            request.Password = _xmlHelper.GetAttributeValue(xmlDoc, userPassXpath, attribute) ?? Settings.DefaultPassword;

            request.DefaultDirectories = new[]
            {
                "\\",
                "Snapshots",
                "BackUp\\Sites",
                "Customization",
                "TemporaryAspFiles"
            };

            request.AbsoluteDirectories = request.DefaultDirectories
                .Select(x => $"{request.RootDirectory}\\{x}\\{request.SiteDirectoryName}")
                .ToArray();

            return request;
        }

        private string GetRootDirectory(XDocument xmlDoc)
        {
            const string attribute = "Value";
            const string siteDirXpath = @"Root/ipath";
            var siteDir = _xmlHelper.GetAttributeValue(xmlDoc, siteDirXpath, attribute);
            return Path.GetFullPath(Path.Combine(siteDir, @".."));
        }

        private string GetConfigExeFile(XDocument xmlDoc, string configXmlFile, string configExeFile)
        {
            if (!string.IsNullOrWhiteSpace(configExeFile))
            {
                return configExeFile;
            }

            var exeFromXmlFile = GetConfigExeFileFromXmlFile(configXmlFile);
            if (!string.IsNullOrWhiteSpace(exeFromXmlFile))
            {
                return exeFromXmlFile;
            }

            var exeFromLogFile = GetConfigExeFileFromLogFile(xmlDoc);
            return exeFromLogFile;
        }

        private static string GetConfigExeFileFromXmlFile(string configXmlFile)
        {
            var xmlFileDir = Path.GetDirectoryName(configXmlFile);
            if (string.IsNullOrWhiteSpace(xmlFileDir)) return null;
            var exePaths = new[]
            {
                Path.GetFullPath(Path.Combine(xmlFileDir, @"ac.exe")),
                Path.GetFullPath(Path.Combine(xmlFileDir, @"Data\ac.exe")),
                Path.GetFullPath(Path.Combine(xmlFileDir, @"Acumatica ERP\Data\ac.exe"))
            };
            return exePaths.FirstOrDefault(File.Exists);
        }

        private string GetConfigExeFileFromLogFile(XDocument xmlDoc)
        {
            const string attribute = "Value";
            const string logFileXpath = @"Root/logfile";
            var logFileDir = _xmlHelper.GetAttributeValue(xmlDoc, logFileXpath, attribute);
            if (string.IsNullOrWhiteSpace(logFileDir)) return null;
            var exeDir = Path.GetFullPath(Path.Combine(logFileDir, @".."));
            return Path.GetFullPath(Path.Combine(exeDir, @"Data\ac.exe"));
        }
    }
}
