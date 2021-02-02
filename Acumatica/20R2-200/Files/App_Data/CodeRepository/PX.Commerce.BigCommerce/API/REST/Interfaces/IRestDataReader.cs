namespace PX.Commerce.BigCommerce.API.REST
{
    public interface IRestDataReader<out T> where T : class
    {
        T Get();
    }
}