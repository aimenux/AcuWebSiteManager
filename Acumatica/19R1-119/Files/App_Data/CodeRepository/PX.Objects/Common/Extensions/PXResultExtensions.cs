using System;

using PX.Data;

namespace PX.Objects.Common.Extensions
{
	public static class PXResultExtensions
	{
		public static Tuple<T0, T1> ToTuple<T0, T1>(this PXResult<T0, T1> result)
			where T0 : class, IBqlTable, new()
			where T1 : class, IBqlTable, new()
		{
			return Tuple.Create<T0, T1>(result, result);
		}

		public static Tuple<T0, T1, T2> ToTuple<T0, T1, T2>(this PXResult<T0, T1, T2> result)
			where T0 : class, IBqlTable, new()
			where T1 : class, IBqlTable, new()
			where T2 : class, IBqlTable, new()
		{
			return Tuple.Create<T0, T1, T2>(result, result, result);
		}
	}
}
