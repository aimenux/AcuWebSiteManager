using Lib.ChainOfResponsibilityPattern;

namespace Lib.Handlers.WebServer
{
    public interface IWebServerHandler : IRequestHandler
    {
        void RemoveSite(string appPoolName, string siteVirtualDirectoryName);
    }
}
