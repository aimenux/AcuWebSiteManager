using RestSharp;
using RestSharp.Authenticators;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class Autentificator : IAuthenticator
    {
        private readonly string _xAuthClient;
        private readonly string _xAuthTocken;

        public Autentificator(string xAuthClient, string xAuthTocken)
        {
            _xAuthClient = xAuthClient;
            _xAuthTocken = xAuthTocken;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddHeader("X-Auth-Client", _xAuthClient);
            request.AddHeader("X-Auth-Token", _xAuthTocken);
        }
    }
}
