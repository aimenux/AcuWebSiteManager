using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public interface IChildReadOnlyRestDataProvider<T> where T : class
    {
        int Count(string parentId);
        List<T> Get(string parentId);
        T GetByID(string id, string parentId);
    }

    public interface IChildRestDataProvider<T> : IChildReadOnlyRestDataProvider<T> where T : class
    {
        T Create(T entity, string parentId);
        T Update(T entity, string id, string parentId);
        bool Delete(string id, string parentId);        
    }

    public interface ISubChildRestDataProvider<T>  where T : class
    {
        int Count(string parentId, string subId);
        List<T> Get(string parentId, string subId);
        T GetByID(string parentId, string subId, string id);

        T Create(T entity, string parentId, string subId);
        T Update(T entity, string parentId, string subId, string id);
        bool Delete(string parentId, string subId, string id);
    }
}