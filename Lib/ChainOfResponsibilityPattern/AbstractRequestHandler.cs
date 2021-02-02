using Lib.Models;

namespace Lib.ChainOfResponsibilityPattern
{
    public abstract class AbstractRequestHandler : IRequestHandler
    {
        private IRequestHandler _nextHandler;

        public string Name => GetType().Name;

        public IRequestHandler SetNext(IRequestHandler handler)
        {
            _nextHandler = handler;
            return _nextHandler;
        }

        public virtual void Handle(Request request)
        {
            _nextHandler?.Handle(request);
        }
    }
}