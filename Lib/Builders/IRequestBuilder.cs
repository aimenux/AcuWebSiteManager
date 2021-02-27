using Lib.Models;

namespace Lib.Builders
{
    public interface IRequestBuilder
    {
        Request Build(Parameters parameters);
    }
}