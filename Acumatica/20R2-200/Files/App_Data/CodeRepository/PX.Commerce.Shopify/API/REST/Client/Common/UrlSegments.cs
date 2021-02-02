using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.REST
{
    public class UrlSegments
    {
        private readonly Dictionary<string, string> _urlSegments = new Dictionary<string, string>();

        public void Add(string key, string value)
        {
            _urlSegments.Add(key, value);
        }

        public void Delete(string key)
        {
            if (_urlSegments.ContainsKey(key))
            {
                _urlSegments.Remove(key);
            }
        }

        public  Dictionary<string, string> GetUrlSegments()
        {
            return _urlSegments;
        }             
    }
}
