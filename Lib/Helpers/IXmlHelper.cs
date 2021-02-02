using System.Xml.Linq;

namespace Lib.Helpers
{
    public interface IXmlHelper
    {
        bool TryCopyXmlFile(string source, string target);
        bool TryLoadXmlFile(string filename, out XDocument xmlDoc);
        string GetAttributeValue(XDocument xmlDoc, string xpath, string attribute);
    }
}
