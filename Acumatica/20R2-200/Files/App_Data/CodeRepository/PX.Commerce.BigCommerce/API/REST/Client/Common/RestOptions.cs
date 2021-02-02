using System;

namespace PX.Commerce.BigCommerce.API.REST
{
    public interface IRestOptions
	{
        string BaseUri { get; set; }
        string XAuthClient { get; set; }
        string XAuthTocken { get; set; }
    }

    public class RestOptions : IRestOptions
    {
        public string BaseUri { get; set; }
        public string XAuthClient { get; set; }
        public string XAuthTocken { get; set; }
        public override string ToString()
        {
            return $"Url: {BaseUri},{Environment.NewLine}" +
                   $"XAuthTocken: {XAuthTocken},{Environment.NewLine}" +
                   $"XAuthClient: {XAuthClient}";
        }
    }
}
