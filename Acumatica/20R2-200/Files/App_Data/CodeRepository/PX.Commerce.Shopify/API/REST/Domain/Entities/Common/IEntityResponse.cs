using Newtonsoft.Json;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.REST
{
    public interface IEntityResponse<T>
    {
        T Data { get; set; }
    }

    public interface IEntitiesResponse<T>
    {
        IEnumerable<T> Data { get; set; }
    }
}
