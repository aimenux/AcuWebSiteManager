using Lib.Models;

namespace Lib.ChainOfResponsibilityPattern
{
    public interface IRequestHandler
    {
        IRequestHandler SetNext(IRequestHandler handler);

        void Handle(Request request);
    }
}
