using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.Common.Extensions
{
	public static class ObjectExtensions
	{
		public static T[] CreateArray<T>(this T item)
		{
			return new[]
			{
				item
			};
		}

		public static T SingleOrNull<T>(this IEnumerable<T> enumerable)
		{
			var list = enumerable.ToList();
			return list.IsSingleElement()
				? list.Single()
				: default(dynamic);
		}

		public static T GetPropertyValue<T>(this object entity, string propertyName)
		{
			var value = entity.GetType().GetProperty(propertyName)
				?.GetValue(entity, null);
			return value == null
				? default
				: (T)value;
		}

		public static T Cast<T>(this object entity)
		{
			var serializeObject = JsonConvert.SerializeObject(entity);
			return JsonConvert.DeserializeObject<T>(serializeObject);
		}

		public static dynamic Cast(this object entity, Type resultType)
		{
			var serializeObject = JsonConvert.SerializeObject(entity);
			return JsonConvert.DeserializeObject(serializeObject, resultType);
		}
	}
}