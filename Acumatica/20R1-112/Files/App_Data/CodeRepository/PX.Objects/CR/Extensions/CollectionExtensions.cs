using System;
using PX.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.CR
{
	internal static class CollectionExtensions
	{
		public static void Add<T>(this ICollection<T> col, IEnumerable<T> addPart)
		{
			addPart.ForEach(col.Add);
		}

	}
}
