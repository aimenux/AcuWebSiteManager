using System;
using Dapper;
using Lib.ChainOfResponsibilityPattern;
using Lib.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Lib.Handlers.Password
{
    public class PasswordHandler : AbstractRequestHandler, IPasswordHandler
    {
        private readonly ILogger _logger;

        public PasswordHandler(ILogger logger)
        {
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
                var connectionString = $"Data Source={request.ServerName};Initial Catalog={request.DatabaseName};Integrated Security=SSPI;";
                using (var connection = new SqlConnection(connectionString))
                {
                    const string updateSql = @"UPDATE [Users] SET 
                                                [Password] = @UserPassword,
                                                [PasswordChangeOnNextLogin] = 0
                                              WHERE [Username] = 'admin' AND [CompanyID] = 2";
                    connection.Execute(updateSql, new { UserPassword = request.Password });
                }
            }
            catch (Exception ex)
            {
                LogPasswordException(ex);
            }
        }

        private void LogPasswordException(Exception ex)
        {
            _logger.LogError("An error has occurred on [{name}] {ex}", Name, ex);
        }
    }
}