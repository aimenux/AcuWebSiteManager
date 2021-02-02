using System;

namespace PX.Commerce.BigCommerce.API.WebDAV
{
    public class WebDAVParentDataProvider
    {
        protected IBigCommerceWebDAVClient _webDavClient;

        public WebDAVParentDataProvider(IBigCommerceWebDAVClient webDavClient)
        {
            _webDavClient = webDavClient;
        }
    }
}
