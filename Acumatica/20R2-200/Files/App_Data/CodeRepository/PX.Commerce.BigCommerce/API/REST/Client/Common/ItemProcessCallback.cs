using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class ItemProcessCallback<T>
	{
		public readonly Int32 Index;
		public readonly Boolean IsSuccess;
		public readonly T Result;
		public readonly RestException Error;

		public ItemProcessCallback(Int32 index, T result)
		{
			Index = index;
			Result = result;
			IsSuccess = true;
		}
		public ItemProcessCallback(Int32 index, RestException error)
		{
			Index = index;
			Error = error;
			IsSuccess = false;
		}
	}
}
