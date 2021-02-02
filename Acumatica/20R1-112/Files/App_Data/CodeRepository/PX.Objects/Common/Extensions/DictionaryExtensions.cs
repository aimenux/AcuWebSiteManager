using System;
using System.Collections.Generic;

namespace PX.Objects.Common.Extensions
{
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Given a dictionary, gets the value by key. If the key is not 
		/// present, adds it to the dictionary along with the value generated 
		/// by the initializer function, and returns that value.
		/// </summary>
		public static TValue GetOrAdd<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, 
			TKey key, 
			Func<TValue> initializer)
		{
			if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (initializer == null) throw new ArgumentNullException(nameof(initializer));

			TValue result;

			if (!dictionary.TryGetValue(key, out result))
			{
				result = initializer();
				dictionary[key] = result;
			}

			return result;
		}

		public static ICollection<TValue> GetValueOrEmpty<TKey, TValue>(
			this IDictionary<TKey, ICollection<TValue>> dictionary,
			TKey key)
		{
			ICollection<TValue> result;

			dictionary.TryGetValue(key, out result);

			return result ?? new TValue[0];
		}
	}
}
