using Lib.Models;
using Microsoft.Data.SqlClient;

namespace Lib.Helpers
{
    public class DatabaseHelper : IDatabaseHelper
    {
        public bool IsDatabaseExists(Request request)
        {
            var serverName = request.ServerName;
            var databaseName = request.DatabaseName;
            return IsDatabaseExists(serverName, databaseName);
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

        public string GetConnectionString(string serverName, string databaseName)
        {
            return $"Data Source={serverName};Initial Catalog={databaseName};Integrated Security=SSPI;";
        }
    }
}