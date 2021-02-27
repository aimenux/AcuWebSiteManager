using Lib.Models;
using Microsoft.SqlServer.Management.Smo;
using SmoDatabase = Microsoft.SqlServer.Management.Smo.Database;

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
                var server = new Server(serverName);
                var database = new SmoDatabase(server, databaseName);
                return database.IsAccessible;
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