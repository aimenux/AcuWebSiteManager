using System;
using System.Diagnostics;
using Lib.Models;

namespace Lib.ChainOfResponsibilityPattern
{
    public class RequestHandlerChain
    {
        private readonly IRequestHandler _rootHandler;

        public RequestHandlerChain(IRequestHandler rootHandler, params IRequestHandler[] nextHandlers)
        {
            _rootHandler = rootHandler;
            if (nextHandlers == null) return;
            var previousHandler = _rootHandler;
            foreach (var nextHandler in nextHandlers)
            {
                if (nextHandler == null) continue;
                previousHandler.SetNext(nextHandler);
                previousHandler = nextHandler;
            }
        }

        public TimeSpan Handle(Request request)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            _rootHandler?.Handle(request);

            stopWatch.Stop();
            return stopWatch.Elapsed;
        }
    }
}
