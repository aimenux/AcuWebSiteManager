using Lib.ChainOfResponsibilityPattern;
using Lib.Models;

namespace Lib.Handlers.Serialization
{
    public interface ISerializationHandler : IRequestHandler
    {
        void XmlSerialize(Request request);
        void JsonSerialize(Request request);
    }
}
