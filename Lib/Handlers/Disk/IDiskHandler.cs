using Lib.ChainOfResponsibilityPattern;

namespace Lib.Handlers.Disk
{
    public interface IDiskHandler : IRequestHandler
    {
        void RemoveDirectory(string directoryName);
        void RemoveDirectories(params string[] directoryNames);
    }
}
