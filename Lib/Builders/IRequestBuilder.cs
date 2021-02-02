using Lib.Models;

namespace Lib.Builders
{
    public interface IRequestBuilder
    {
        Request Build(string configXmlFile, string configExeFile = null);
    }
}