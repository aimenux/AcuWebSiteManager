using PX.Data;

namespace PX.Objects.CN.Common.Services
{
    public class CacheService : ICacheService
    {
        public object GetValueOriginal<T>(PXCache cache, object record)
            where T : IBqlField
        {
            return cache.GetValueOriginal<T>(record);
        }
    }
}