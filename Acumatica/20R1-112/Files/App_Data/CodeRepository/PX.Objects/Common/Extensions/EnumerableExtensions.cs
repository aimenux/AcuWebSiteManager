using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;

using PX.Data;

namespace PX.Objects.Common.Extensions
{
	public static class EnumerableExtensions
	{
		public static bool IsEmpty<T>(this IEnumerable<T> sequence) => !sequence.Any();

		/// <returns>
		/// <c>true</c>, if the specified sequence contains exactly 
		/// one element, <c>false</c> otherwise.
		/// </returns>
		public static bool IsSingleElement<T>(this IEnumerable<T> sequence)
		{
			if (sequence == null) throw new ArgumentNullException(nameof(sequence));

			using (IEnumerator<T> enumerator = sequence.GetEnumerator())
				return enumerator.MoveNext() && !enumerator.MoveNext();
		}


		public static IEnumerable<TNode> DistinctByKeys<TNode>(this IEnumerable<TNode> sequence, PXGraph graph)
			where TNode : class, IBqlTable, new()
			=> DistinctByKeys(sequence, graph?.Caches[typeof(TNode)]);

		/// <summary>
		/// Given a sequence of records and a <see cref="PXCache"/> object,
		/// returns a sequence of elements that have different keys.
		/// </summary>
		/// <remarks>The collection of keys is defined by <see cref="PXCache.Keys"/>.</remarks>
		public static IEnumerable<TNode> DistinctByKeys<TNode>(this IEnumerable<TNode> sequence, PXCache cache)
			where TNode : class, IBqlTable, new()
		{
			if (sequence == null) throw new ArgumentNullException(nameof(sequence));
			if (cache == null) throw new ArgumentNullException(nameof(cache));

			return sequence.Distinct(new RecordKeyComparer<TNode>(cache));
		}

		/// <summary>
		/// Returns the index of the first element that satisfies the 
		/// specified predicate, or a negative value in case such an
		/// element cannot be found.
		/// </summary>
		/// <remarks>
		/// In case when the element satisfying the predicate cannot
		/// be found, the value returned by this method corresponds
		/// to the negative number of elements in the sequence, further
		/// decremented by one.
		/// </remarks>
		public static int FindIndex<T>(this IEnumerable<T> sequence, Predicate<T> predicate)
		{
			int index = 0;

			foreach (T element in sequence)
			{
				if (predicate(element)) return index;
				++index;
			}

			return -index - 1;
		}

		/// <returns>
		/// <c>true</c>, if the specified sequence contains at least
		/// two elements, <c>false</c> otherwise.
		/// </returns>
		public static bool HasAtLeastTwoItems<T>(this IEnumerable<T> sequence) => sequence.HasAtLeast(2);

		/// <summary>
		/// Flattens a sequence of element groups into a sequence of elements.
		/// </summary>
		public static IEnumerable<TValue> Flatten<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> sequenceOfGroups)
			=> sequenceOfGroups.SelectMany(x => x);

		/// <summary>
		/// Return one row which contains sum for all decimal fields and identical value if all rows contains that value otherwise is null.
		/// </summary>
		public static RecordType CalculateSumTotal<RecordType>(this IEnumerable<RecordType> rows, PX.Data.PXCache cache)
					where RecordType : PX.Data.IBqlTable, new()
		{
			var total = new RecordType();

			var properties = cache.GetAttributesOfType<PX.Data.PXUIFieldAttribute>(null, null)
				.Select(p => new
				{
					uiAttribute = p,
					sumGroupOperation = cache.GetAttributesOfType<PX.Data.PXDecimalAttribute>(null, p.FieldName).Any() ||
						cache.GetAttributesOfType<PX.Data.PXDBDecimalAttribute>(null, p.FieldName).Any()
				}).ToList();

			var propertiesWithSum = properties.Where(p => p.sumGroupOperation).ToList();
			var propertiesWithoutSum = properties.Where(p => !p.sumGroupOperation).ToList();

			var sumAggregateValues = new decimal[propertiesWithSum.Count];
			var aggregateValues = new object[propertiesWithoutSum.Count];

			bool firstRow = true;

			foreach (var row in rows)
			{
				for (int fieldIndex = 0; fieldIndex < propertiesWithSum.Count; fieldIndex++)
				{
					var value = (decimal?)cache.GetValue(row, propertiesWithSum[fieldIndex].uiAttribute.FieldName);
					sumAggregateValues[fieldIndex] += value ?? 0;
				}

				for (int fieldIndex = 0; fieldIndex < propertiesWithoutSum.Count; fieldIndex++)
				{
					var value = cache.GetValue(row, propertiesWithoutSum[fieldIndex].uiAttribute.FieldName);
					aggregateValues[fieldIndex] = (firstRow || aggregateValues[fieldIndex]?.Equals(value) == true) ? value : null;
				}

				firstRow = false;
			}

			for (int fieldIndex = 0; fieldIndex < propertiesWithSum.Count; fieldIndex++)
			{
				cache.SetValue(total, propertiesWithSum[fieldIndex].uiAttribute.FieldName, sumAggregateValues[fieldIndex]);
			}
			for (int fieldIndex = 0; fieldIndex < propertiesWithoutSum.Count; fieldIndex++)
			{
				cache.SetValue(total, propertiesWithoutSum[fieldIndex].uiAttribute.FieldName, aggregateValues[fieldIndex]);
			}

			return total;
		}

		public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, int size,
			Func<IEnumerable<TSource>, TResult> resultSelector)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
			if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

			return _(); IEnumerable<TResult> _()
			{
				TSource[] bucket = null;
				var count = 0;

				foreach (var item in source)
				{
					if (bucket == null)
					{
						bucket = new TSource[size];
					}

					bucket[count++] = item;

					// The bucket is fully buffered before it's yielded
					if (count != size)
					{
						continue;
					}

					yield return resultSelector(bucket);

					bucket = null;
					count = 0;
				}

				// Return the last bucket with all remaining elements
				if (bucket != null && count > 0)
				{
					Array.Resize(ref bucket, count);
					yield return resultSelector(bucket);
				}
			}
		}
	}
}
