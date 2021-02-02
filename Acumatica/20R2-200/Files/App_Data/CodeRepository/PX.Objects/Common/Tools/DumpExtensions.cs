using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using PX.Data;
using PX.Objects.Common.Extensions;

namespace PX.Objects.Common.Tools
{
	public static class DumpExtensions
	{
		private static string[] ignoredFields = new[]
		{
			"NoteID",
			"CreatedByID",
			"CreatedByScreenID",
			"CreatedDateTime",
			"LastModifiedByID",
			"LastModifiedByScreenID",
			"LastModifiedDateTime",
			"tstamp",
			"AssignDate"
		};

		private const string Indent = "    ";

		public static string Dump(this object obj, Action<object, StringBuilder> dumpSingleObject)
		{
			var sb = new StringBuilder();

			sb.AppendLine(obj.ToString());

			var enumeration = obj as IEnumerable;
			if (enumeration != null)
			{
				sb.AppendLine("{");

				foreach (var item in enumeration)
				{
					dumpSingleObject(item, sb);
					sb.Append(",");
				}

				sb.AppendLine("}");
			}
			else
			{
				dumpSingleObject(obj, sb);
			}

			return sb.ToString();
		}

		public static string Dump(this object obj)
		{
			return Dump(obj, (ob, sb) => DumpForSingleObject(ob, sb));
		}

		public static string DumpForSingleObject(this object obj, StringBuilder sb, int getFullImageStackLevel = -1)
		{
			getFullImageStackLevel++;

			PropertyInfo[] propertyInfos = obj.GetType().GetProperties().OrderBy(pi => pi.Name).ToArray();

			var indent = GetIndent(getFullImageStackLevel);
			var detailIndent = GetIndent(getFullImageStackLevel + 1);

			sb.AppendLine();
			sb.AppendLine(string.Concat(indent, "{"));

			foreach (var propertyInfo in propertyInfos)
			{
				if (ignoredFields.Any(fieldName=> propertyInfo.Name.Contains(fieldName)))
					continue;

				var value = propertyInfo.GetValue(obj, null);

				if (value != null)
				{
					if (value.IsComplex())
					{
						value = DumpForSingleObject(value, sb, getFullImageStackLevel + 1);
					}

					if (value is string)
					{
						value = string.Concat("\"", value, "\"");
					}

					if (value is decimal
					    || value is decimal?)
					{
						value = ((decimal) value).ToString(CultureInfo.InvariantCulture);
					}
				}
				else
				{
					value = "null";
				}

				sb.AppendLine(string.Format("{0}{1}: {2}", detailIndent, propertyInfo.Name, value));
			}

			sb.AppendLine(string.Concat(indent, "}"));

			return sb.ToString();
		}

		private static string GetIndent(int level)
		{
			return string.Join(string.Empty, Enumerable.Repeat(Indent, level));
		}

		public static string DumpAsTable<TItem>(this IReadOnlyCollection<TItem> items, PXCache cache)
		{
			PropertyInfo[] propertyInfos = typeof (TItem).GetProperties(BindingFlags.Instance | BindingFlags.Public)
															.Where(info=> !ignoredFields.Contains(info.Name))
															.ToArray();

			Dictionary<string, int> maxValueLengths = new Dictionary<string, int>();

			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				int maxValueLength = 0;

				if (typeof(decimal?).IsAssignableFrom(propertyInfo.PropertyType))
				{
					maxValueLength = decimal.MinValue.ToString(CultureInfo.InvariantCulture).Length;
				}
				else if (typeof(int?).IsAssignableFrom(propertyInfo.PropertyType))
				{
					maxValueLength = int.MinValue.ToString(CultureInfo.InvariantCulture).Length;
				}
				else if (typeof(bool?).IsAssignableFrom(propertyInfo.PropertyType))
				{
					maxValueLength = 5;
				}
				else if (propertyInfo.PropertyType == typeof (string))
				{
					PXDBStringAttribute dbAttr = cache.GetAttributes(propertyInfo.Name).OfType<PXDBStringAttribute>().SingleOrDefault();

					if (dbAttr != null)
					{
						maxValueLength = dbAttr.Length;
					}
					else
					{
						PXStringAttribute attr = cache.GetAttributes(propertyInfo.Name).OfType<PXStringAttribute>().SingleOrDefault();

						if (attr != null)
						{
							maxValueLength = attr.Length;
						}
					}
				}
				else
				{
					throw new Exception("Unexpected type");
				}

				maxValueLengths[propertyInfo.Name] = maxValueLength;
			}

			KeyValuePair<string, int>[] fieldsWithUnknownLength = maxValueLengths.Where(kvp => kvp.Value == 0).ToArray();

			string[] fieldsList = propertyInfos.Select(info => info.Name).ToArray();

			if (!fieldsWithUnknownLength.Any())
			{
				return DumpAsTable<TItem>(items, cache, fieldsList, maxValueLengths);
			}

			foreach (TItem item in items)
			{
				foreach (KeyValuePair<string, int> kvp in fieldsWithUnknownLength)
				{
					int? length = cache.GetValue(item, kvp.Key)?.ToString().Length;

					if (length > maxValueLengths[kvp.Key])
					{
						maxValueLengths[kvp.Key] = length.Value;
					}
				}
			}

			return DumpAsTable<TItem>(items, cache, fieldsList, maxValueLengths);
		}

		private static string DumpAsTable<TItem>(this IEnumerable<TItem> items, 
														PXCache cache, 
														string[] origFieldList,
														Dictionary<string, int> maxValueLengths)
		{
			Dictionary<string, int> columnsLengths = new Dictionary<string, int>(maxValueLengths);
			StringBuilder sb = new StringBuilder();

			List<string> fieldsList = cache.BqlKeys.Select(t => t.Name.Capitalize()).ToList();
			fieldsList.AddRange(origFieldList.Except(fieldsList));

			sb.Append("|");

			foreach (string field in fieldsList)
			{
				if (field.Length > columnsLengths[field])
				{
					columnsLengths[field] = field.Length;
				}

				sb.Append(field.PadRight(columnsLengths[field], ' '));
				sb.Append("|");
			}

			sb.AppendLine();

			foreach (TItem item in items)
			{
				sb.Append("|");
				foreach (string field in fieldsList)
				{
					object value = cache.GetValue(item, field);

					sb.Append((value?.ToString() ?? string.Empty).PadRight(columnsLengths[field], ' '));
					sb.Append("|");
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
