using Lib.Models;

namespace Lib.Helpers
{
    public interface IDatabaseHelper
    {
        bool IsServerExists(Request request);
        bool IsDatabaseExists(Request request);
        bool IsDatabaseExists(string serverName, string databaseName);
        string GetConnectionString(Request request);
        string GetConnectionString(string serverName, string databaseName);
    }
}
