using PX.Data;

namespace PX.Objects.CN.Common.Services
{
	public interface ICacheService
	{
		object GetValueOriginal<T>(PXCache cache, object record)
			where T : IBqlField;
	}
}