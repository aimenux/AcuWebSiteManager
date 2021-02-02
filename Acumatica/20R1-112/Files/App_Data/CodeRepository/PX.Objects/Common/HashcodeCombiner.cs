using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common
{
	public static class HashcodeCombiner
	{
		/// <summary>
		/// Combines a sequence of hash codes into a single hash code using
		/// an algorithm that ensures a minimum number of collisions.
		/// </summary>
		public static int Combine(IEnumerable<int> hashCodes)
		{
			const int seedHashCode = 17;
			const int hashMultiplier = 31;

			int result = seedHashCode;

			unchecked
			{
				foreach (int hashCode in hashCodes)
				{
					result = result * hashMultiplier + hashCode;
				}
			}

			return result;
		}

		/// <summary>
		/// Combines the hash codes of a sequence of objects 
		/// into a single hash code.
		/// </summary>
		/// <remarks>
		/// If any object passed is <c>null</c>, its hash code
		/// is considered to be zero.
		/// </remarks>
		public static int Combine(IEnumerable<object> objects)
		{
			return Combine(
				objects.Select(
					entity => entity?.GetHashCode() ?? 0));
		}
	}
}