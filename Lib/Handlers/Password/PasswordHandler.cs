using System;
using System.Collections.Generic;
using Dapper;
using Lib.ChainOfResponsibilityPattern;
using Lib.Helpers;
using Lib.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lib.Handlers.Password
{
    public class PasswordHandler : AbstractRequestHandler, IPasswordHandler
    {
        private readonly IDatabaseHelper _databaseHelper;
        private readonly Settings _settings;
        private readonly ILogger _logger;

        public PasswordHandler(IDatabaseHelper databaseHelper, IOptions<Settings> options, ILogger logger)
        {
            _databaseHelper = databaseHelper;
            _settings = options.Value;
            _logger = logger;
        }

        public override void Handle(Request request)
        {
            UpdatePassword(request);
            base.Handle(request);
        }

        public void UpdatePassword(Request request)
        {
            try
            {
                var sqlUpdate = GetSqlUpdate(_settings);
                var connectionString = GetConnectionString(request);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Execute(sqlUpdate, new { UserPassword = request.Password });
                }
            }
            catch (Exception ex)
            {
                LogPasswordException(ex);
            }
        }

        private string GetConnectionString(Request request)
        {
            var serverName = request.ServerName;
            var databaseName = request.DatabaseName;
            return _databaseHelper.GetConnectionString(serverName, databaseName);
        }

        private static string GetSqlUpdate(Settings settings)
        {
            var defaultTenants = new List<int> { 2, 3 };
            var tenants = settings.Tenants ?? defaultTenants;
            var ids = string.Join(", ", tenants);
            return $@"UPDATE [Users] SET 
                         [Password] = @UserPassword,
                         [PasswordChangeOnNextLogin] = 0
                      WHERE [Username] = 'admin' AND [CompanyID] IN ({ids})";
        }

        private void LogPasswordException(Exception ex)
        {
            _logger.LogError("An error has occurred on [{name}] {ex}", Name, ex);
        }
    }
}