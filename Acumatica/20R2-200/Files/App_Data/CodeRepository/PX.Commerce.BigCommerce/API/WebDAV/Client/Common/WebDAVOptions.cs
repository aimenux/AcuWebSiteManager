using System;

namespace PX.Commerce.BigCommerce.API.WebDAV
{
    public interface IWebDAVOptions
    {
        string ServerHttpsUri { get; set; }
        string ClientUser { get; set; }
        string ClientPassword { get; set; }
    }

    public class WebDAVOptions : IWebDAVOptions
    {
        public string ServerHttpsUri { get; set; }
        public string ClientUser { get; set; }
        public string ClientPassword { get; set; }

        public override string ToString()
        {
            return $"Server Https Url: {ServerHttpsUri},{Environment.NewLine}" +
                   $"ClientUser: {ClientUser},{Environment.NewLine}" +
                   $"ClientPassword: {ClientPassword}";
        }
    }
}
