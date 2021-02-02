using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public interface IParentRestDataProvider<T>  
		where T : class 
    {   
        T Create(T entity);

        T Update(T entity, int id);

		bool Delete(T entity,int id);
        bool Delete(int id);

        List<T> Get(IFilter filter = null) ;
        IEnumerable<T> GetAll(IFilter filter = null);
        T GetByID(string id);

        ItemCount Count();
        ItemCount Count(IFilter filter);
    }
}