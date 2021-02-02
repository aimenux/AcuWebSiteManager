using Lib.ChainOfResponsibilityPattern;
using Lib.Models;

namespace Lib.Handlers.Password
{
    public interface IPasswordHandler : IRequestHandler
    {
        void UpdatePassword(Request request);
    }
}
