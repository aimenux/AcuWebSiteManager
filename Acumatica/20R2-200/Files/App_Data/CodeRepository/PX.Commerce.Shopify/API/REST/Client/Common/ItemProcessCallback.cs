using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify.API.REST
{
	public class ItemProcessCallback<T>
	{
		public readonly Int32 Index;
		public readonly Boolean IsSuccess;
		public readonly T Result;
		public readonly Exception Error;

		public ItemProcessCallback(Int32 index, T result)
		{
			Index = index;
			Result = result;
			IsSuccess = true;
		}
		public ItemProcessCallback(Int32 index, Exception error)
		{
			Index = index;
			Error = error;
			IsSuccess = false;
		}
	}
}
