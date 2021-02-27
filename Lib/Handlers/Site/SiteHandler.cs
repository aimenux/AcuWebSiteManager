using System;
using System.IO;
using Dapper;
using Lib.ChainOfResponsibilityPattern;
using Lib.Helpers;
using Lib.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Lib.Handlers.Site
{
    public class SiteHandler : AbstractRequestHandler, ISiteHandler
    {
        private readonly IDatabaseHelper _databaseHelper;
        private readonly IFileHelper _fileHelper;
        private readonly ILogger _logger;

        public SiteHandler(IDatabaseHelper databaseHelper, IFileHelper fileHelper, ILogger logger)
        {
            _databaseHelper = databaseHelper;
            _fileHelper = fileHelper;
            _logger = logger;
        }

        public override void Handle(Request request)
        {
            SwitchSite(request);
            base.Handle(request);
        }

        public void SwitchSite(Request request)
        {
            ModifyUserServerRole(request);
            ModifyWebConfigFile(request);
        }

        private void ModifyWebConfigFile(Request request)
        {
            try
            {
                var webConfigFile = Path.GetFullPath(Path.Combine(request.SiteDirectoryPath, @"Web.config"));
                if (!File.Exists(webConfigFile))
                {
                    LogSiteError($"Unfounded [{webConfigFile}] file");
                    return;
                }

                var oldString = $"Initial Catalog={request.DatabaseName}";
                var newString = $"Initial Catalog={request.SwitchDatabaseName}";
                _fileHelper.ModifyFile(webConfigFile, oldString, newString);
            }
            catch (Exception ex)
            {
                LogSiteException(ex);
            }
        }

        private void ModifyUserServerRole(Request request)
        {
            try
            {
                var connectionString = GetConnectionString(request);
                var sqlAlterRole = GetSqlAlterServerRole(request);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Execute(sqlAlterRole);
                }
            }
            catch (Exception ex)
            {
                LogSiteException(ex);
            }
        }

        private string GetConnectionString(Request request)
        {
            const string databaseName = @"master";
            var serverName = request.ServerName;
            return _databaseHelper.GetConnectionString(serverName, databaseName);
        }

        private static string GetSqlAlterServerRole(Request request)
        {
            var appPoolName = request.AppPoolName;
            return $@"ALTER SERVER ROLE [sysadmin] ADD MEMBER [IIS APPPOOL\{appPoolName}]";
        }

        private void LogSiteException(Exception ex)
        {
            _logger.LogError("An error has occurred on [{name}] {ex}", Name, ex);
        }

        private void LogSiteError(string message)
        {
            _logger.LogError("An error has occurred on [{name}] {message}", Name, message);
        }
    }
}