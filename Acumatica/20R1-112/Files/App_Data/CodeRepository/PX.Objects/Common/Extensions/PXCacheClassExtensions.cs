using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using PX.Common;
using PX.Data;

namespace PX.Objects.Common.Extensions
{
	public static class PXCacheExtensions
	{
		public static IEqualityComparer<TDAC> GetKeyComparer<TDAC>(this PXCache<TDAC> cache) 
			where TDAC : class, IBqlTable, new()
		{
			return new CustomComparer<TDAC>(cache.GetObjectHashCode, cache.ObjectsEqual);
		}

		public static void ClearFieldErrors<TField>(this PXCache cache, object record)
			where TField : IBqlField
			=> cache.RaiseExceptionHandling<TField>(record, null, null);

		public static void ClearFieldSpecificError<TField>(this PXCache cache, object record, string errorMsg, params object[] errorMessageArguments)
			where TField : IBqlField
		{
			String fieldError = PXUIFieldAttribute.GetError<TField>(cache, record);
			String specificError = PXMessages.LocalizeFormatNoPrefix(errorMsg, errorMessageArguments);
			if (String.Equals(fieldError, specificError, StringComparison.CurrentCulture))
				cache.ClearFieldErrors<TField>(record);
		}

		public static void DisplayFieldError<TField>(
			this PXCache cache,
			object record,
			PXErrorLevel errorLevel,
			string message,
			params object[] errorMessageArguments)
			where TField : IBqlField
		{
			PXSetPropertyException<TField> setPropertyException =
				new PXSetPropertyException<TField>(message, errorLevel, errorMessageArguments);

			if (cache.RaiseExceptionHandling<TField>(record, null, setPropertyException))
			{
				throw setPropertyException;
			}
		}

		public static void DisplayFieldError<TField>(
			this PXCache cache,
			object record,
			string message,
			params object[] errorMessageArguments)
			where TField : IBqlField
			=> DisplayFieldError<TField>(cache, record, PXErrorLevel.Error, message, errorMessageArguments);

		public static void DisplayFieldWarning<TField>(
			this PXCache cache,
			object record,
			object newValue,
			string message,
			params object[] errorMessageArguments)
			where TField : IBqlField
		{
			PXSetPropertyException<TField> setPropertyException =
				new PXSetPropertyException<TField>(message, PXErrorLevel.Warning, errorMessageArguments);

			if (cache.RaiseExceptionHandling<TField>(record, newValue, setPropertyException))
			{
				throw setPropertyException;
			}
		}

		public static string GetFullDescription(this PXCache cache, object record)
		{
			if (cache == null) throw new ArgumentNullException(nameof(cache));

			StringBuilder description = new StringBuilder($"{ cache.GetItemType().Name }.");

			if (record == null)
			{
				description.Append(" [NULL]");
				return description.ToString();
			}

			foreach (Type field in cache.BqlFields)
			{
				description.Append(
					$" {field.Name}: {cache.GetValue(record, field.Name) ?? "[NULL]"};");
			}

			return description.ToString();
		}

		/// <summary>
		/// A PXCache extension method that sets all cache permissions to edit data: <see cref="PXCache.AllowInsert"/>, <see cref="PXCache.AllowDelete"/>, <see cref="PXCache.AllowUpdate"/>.
		/// </summary>
		/// <param name="cache">The cache to set permissions.</param>
		/// <param name="allowEdit">True to set all permissions to allow edit of data, false to prohibit edit.</param>
		public static void SetAllEditPermissions(this PXCache cache, bool allowEdit)
		{
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			cache.AllowInsert = allowEdit;
			cache.AllowDelete = allowEdit;
			cache.AllowUpdate = allowEdit;
		}
		/// <summary>
		/// Checks if the value of a field has been updated during the round-trip.
		/// </summary>
		public static bool IsValueUpdated<TValue, TField>(this PXCache cache, object row, 
			IEqualityComparer<TValue> comparer = default(IEqualityComparer<TValue>))
			where TField : IBqlField
		{
			if (cache.GetStatus(row) != PXEntryStatus.Updated)
				throw new ArgumentException("Row is not in Updated status", nameof (row));

			if (comparer == null) 
				comparer = EqualityComparer<TValue>.Default;

			TValue current = (TValue) cache.GetValue<TField>(row);
			TValue original = (TValue) cache.GetValueOriginal<TField>(row);

			return !comparer.Equals(current, original);
		}

		/// <summary>
		/// Collects all the non-key fields of the specified cache which are decorated with <see cref="PXDBDecimalAttribute"/>.
		/// </summary>
		public static IEnumerable<string> GetAllDBDecimalFields(this PXCache cache, params string[] excludeFields)
		{
			IEnumerable<string> ret =
				cache.Fields
				.Where(fld => cache.GetAttributesReadonly(fld).OfType<PXDBDecimalAttribute>().Any(a => a.IsKey != true));

			if (excludeFields?.Length > 0)
			{
				ret = ret.Where(fld => !excludeFields.Any(efld => string.Equals(fld, efld, StringComparison.OrdinalIgnoreCase)));
			}

			return ret;
		}

		public static bool ObjectsEqualExceptFields<Field1>(this PXCache cache, object a, object b)
			where Field1 : IBqlField
		{
			return ObjectsEqualExceptFields(cache, a, b, typeof(Field1));
		}

		public static bool ObjectsEqualExceptFields<Field1, Field2>(this PXCache cache, object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
		{
			return ObjectsEqualExceptFields(cache, a, b, typeof(Field1), typeof(Field2));
		}

		public static bool ObjectsEqualExceptFields<Field1, Field2, Field3>(this PXCache cache, object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
		{
			return ObjectsEqualExceptFields(cache, a, b, typeof(Field1), typeof(Field2), typeof(Field3));
		}

		public static bool ObjectsEqualExceptFields<Field1, Field2, Field3, Field4>(this PXCache cache, object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
		{
			return ObjectsEqualExceptFields(cache, a, b, typeof(Field1), typeof(Field2), typeof(Field3), typeof(Field4));
		}

		public static bool ObjectsEqualExceptFields<Field1, Field2, Field3, Field4, Field5>(this PXCache cache, object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
		{
			return ObjectsEqualExceptFields(cache, a, b, typeof(Field1), typeof(Field2), typeof(Field3), typeof(Field4), typeof(Field5));
		}

		public static bool ObjectsEqualExceptFields<Field1, Field2, Field3, Field4, Field5, Field6>(this PXCache cache, object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
		{
			return ObjectsEqualExceptFields(cache, a, b, typeof(Field1), typeof(Field2), typeof(Field3), typeof(Field4), typeof(Field5), typeof(Field6));
		}

		public static bool ObjectsEqualExceptFields<Field1, Field2, Field3, Field4, Field5, Field6, Field7>(this PXCache cache, object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
		{
			return ObjectsEqualExceptFields(cache, a, b, typeof(Field1), typeof(Field2), typeof(Field3), typeof(Field4), typeof(Field5), typeof(Field6), typeof(Field7));
		}

		public static bool ObjectsEqualExceptFields<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8>(this PXCache cache, object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
			where Field8 : IBqlField
		{
			return ObjectsEqualExceptFields(cache, a, b, typeof(Field1), typeof(Field2), typeof(Field3), typeof(Field4), typeof(Field5), typeof(Field6), typeof(Field7), typeof(Field8));
		}

		public static bool ObjectsEqualExceptFields(this PXCache cache, object a, object b, params Type[] exceptFields)
		{
			var exceptFieldsHashSet = exceptFields
				.Select(efld => efld.Name)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);
			foreach (string fld in cache.Fields.Where(fld => !exceptFieldsHashSet.Contains(fld)))
			{
				if (!object.Equals(cache.GetValue(a, fld), cache.GetValue(b, fld)))
					return false;
			}
			return true;
		}

		public static void VerifyFieldAndRaiseException<T>(this PXCache cache, object row)
			where T: IBqlField
		{
			object newValue = cache.GetValue<T>(row);

			try
			{
				cache.RaiseFieldVerifying<T>(row, ref newValue);
			}
			catch (PXSetPropertyException ex)
			{
				cache.RaiseExceptionHandling<T>(row, newValue, ex);
			}
		}

		/// <summary>
		/// The method returns: display cache name; display field name and user friendly value for each key from DAC.
		/// </summary>
		public static string GetRowDescription(this PXCache cache, object row, string cacheNameSeparator = ": ", string keysSeparator = "; ")
		{
			if (cache == null)
				throw new PXArgumentException(nameof(cache));

			if (row == null)
				return null;

			var values = new List<object>();

			foreach (string key in cache.Keys)
			{
				object newValue = cache.GetValue(row, key);
				if (newValue != null)
					cache.RaiseFieldSelecting(key, row, ref newValue, true); // We can't use GetValueExt because we need to get value from PXStringListAttribute

				var displayName = (newValue as PXFieldState)?.DisplayName;
				if (!string.IsNullOrEmpty(displayName))
				{
					values.Add($"{displayName} = {newValue}");
				}
				else if (newValue != null)
				{
					values.Add(newValue);
				}
			}

			return string.Concat(cache.DisplayName, cacheNameSeparator, string.Join(keysSeparator, values));
		}
	}
}
