using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace Lib.Helpers
{
    public class XmlHelper : IXmlHelper
    {
        private readonly ILogger _logger;

        public XmlHelper(ILogger logger)
        {
            _logger = logger;
        }

        public bool TryCopyXmlFile(string source, string target)
        {
            try
            {
                File.Copy(source, target);
                return true;
            }
            catch
            {
                LogXmlFileException(source, target);
                return false;
            }
        }

        public bool TryLoadXmlFile(string filename, out XDocument xmlDoc)
        {
            try
            {
                xmlDoc = XDocument.Load(filename);
                return true;
            }
            catch
            {
                LogXmlFileException(filename);
                xmlDoc = null;
                return false;
            }
        }

        public string GetAttributeValue(XDocument xmlDoc, string xpath, string attribute)
        {
            try
            {
                var element = xmlDoc.XPathSelectElement(xpath);
                var value = element?.Attribute(attribute)?.Value;
                return value;
            }
            catch
            {
                LogXmlDocException(attribute);
                return null;
            }
        }

        private void LogXmlFileException(string source, string target)
        {
            _logger.LogError("Failed to copy xml file [{source}] to [{target}]", source, target);
        }

        private void LogXmlFileException(string filename)
        {
            _logger.LogError("Failed to load xml file [{name}]", filename);
        }

        private void LogXmlDocException(string attribute)
        {
            _logger.LogError("Failed to get xml attribute [{attribute}]", attribute);
        }
    }
}