using Lib.ChainOfResponsibilityPattern;
using Lib.Models;

namespace Lib.Handlers.Site
{
    public interface ISiteHandler : IRequestHandler
    {
        void SwitchSite(Request request);
    }
}
