using Lib.ChainOfResponsibilityPattern;

namespace Lib.Handlers.Process
{
    public interface IProcessHandler : IRequestHandler
    {
        void RunProcess(string name, string arguments);
    }
}
