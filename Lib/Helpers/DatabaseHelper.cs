using Lib.Models;
using Microsoft.Data.SqlClient;

namespace Lib.Helpers
{
    public class DatabaseHelper : IDatabaseHelper
    {
        public bool IsServerExists(Request request)
        {
            try
            {
                const string databaseName = @"master";
                var serverName = request.ServerName;
                var databaseUserName = request.DatabaseUserName;
                var databasePassword = request.DatabasePassword;
                var connectionString = GetConnectionString(serverName, databaseName, databaseUserName, databasePassword);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool IsDatabaseExists(Request request)
        {
            try
            {
                var connectionString = GetConnectionString(request);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool IsDatabaseExists(string serverName, string databaseName)
        {
            try
            {
                var connectionString = GetConnectionString(serverName, databaseName);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public string GetConnectionString(Request request)
        {
            return GetConnectionString(request.ServerName, request.DatabaseName, request.DatabaseUserName, request.DatabasePassword);
        }

        public string GetConnectionString(string serverName, string databaseName)
        {
            return GetConnectionString(serverName, databaseName, null, null);
        }

        private static string GetConnectionString(
            string serverName,
            string databaseName,
            string databaseUserName,
            string databasePassword)
        {
            if (databaseUserName == null || databasePassword == null)
            {
                var windowsLoginBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = serverName,
                    InitialCatalog = databaseName,
                    IntegratedSecurity = true,
                    ApplicationName = Settings.ApplicationName,
                };

                return windowsLoginBuilder.ConnectionString;
            }

            var sqlLoginBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                UserID = databaseUserName,
                Password = databasePassword,
                InitialCatalog = databaseName,
                ApplicationName = Settings.ApplicationName,
            };

            return sqlLoginBuilder.ConnectionString;
        }
    }
}