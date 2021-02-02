using Lib.ChainOfResponsibilityPattern;

namespace Lib.Handlers.Database
{
    public interface IDatabaseHandler : IRequestHandler
    {
        void RemoveDatabase(string serverName, string databaseName);
    }
}
