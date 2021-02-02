using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public interface IEntityResponse<T>
    {
        T Data { get; set; }
        Meta Meta { get; set; }
    }

    public interface IEntitiesResponse<T>
    {
        List<T> Data { get; set; }
        Meta Meta { get; set; }
    }
}
