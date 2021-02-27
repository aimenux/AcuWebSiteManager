using Lib.Models;

namespace Lib.Helpers
{
    public interface IDatabaseHelper
    {
        bool IsDatabaseExists(Request request);
        bool IsDatabaseExists(string serverName, string databaseName);
        string GetConnectionString(string serverName, string databaseName);
    }
}
