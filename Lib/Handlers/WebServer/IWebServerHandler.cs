using Lib.ChainOfResponsibilityPattern;
using Lib.Models;

namespace Lib.Handlers.WebServer
{
    public interface IWebServerHandler : IRequestHandler
    {
        void RemoveSite(Request request);
        void RemoveApplicationPool(Request request);
    }
}
