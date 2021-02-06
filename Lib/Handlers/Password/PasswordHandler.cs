using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Lib.ChainOfResponsibilityPattern;
using Lib.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lib.Handlers.Password
{
    public class PasswordHandler : AbstractRequestHandler, IPasswordHandler
    {
        private readonly ILogger _logger;
        private readonly Settings _settings;

        public PasswordHandler(ILogger logger, IOptions<Settings> options)
        {
            _logger = logger;
            _settings = options.Value;
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

        private static string GetConnectionString(Request request)
        {
            var serverName = request.ServerName;
            var databaseName = request.DatabaseName;
            return $"Data Source={serverName};Initial Catalog={databaseName};Integrated Security=SSPI;";
        }

        private static string GetSqlUpdate(Settings settings)
        {
            var tenants = settings.Tenants ?? new List<int>();
            if (!tenants.Any())
            {
                const int defaultTenantId = 2;
                tenants.Add(defaultTenantId);
            }

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