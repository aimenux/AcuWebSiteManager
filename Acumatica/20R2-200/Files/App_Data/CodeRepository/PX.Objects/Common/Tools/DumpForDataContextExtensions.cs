using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common.Tools
{
	public static class DumpForDataContextExtensions
	{
		private static string[] _ignoredFields = new[]
		{
			"CreatedByID",
			"CreatedByScreenID",
			"CreatedDateTime",
			"LastModifiedByID",
			"LastModifiedByScreenID",
			"LastModifiedDateTime",
			"tstamp",
			"DeletedDatabaseRecord"
		};

		public static string DumpForDataContext(this object obj)
		{
			return obj.Dump(DumpForDataContextForSingleObject);
		}

		private static void DumpForDataContextForSingleObject(object obj, StringBuilder sb)
		{
			if (obj == null)
				return;

			var pxresult = obj as PXResult;
			if (pxresult != null)
			{
				var listTypeNames = new List<string>();

				var itemsSb = new StringBuilder[pxresult.TableCount];

				for (int i = 0; i < pxresult.TableCount; i++)
				{
					listTypeNames.Add(pxresult.GetItemType(i).Name);

					itemsSb[i] = new StringBuilder();

					DumpForDataContextForSingleObject(pxresult[i], itemsSb[i]);
				}

				sb.AppendFormat("new PXResult<{0}>({1})", string.Join(",", listTypeNames), string.Join(",", (object[])itemsSb));

				return;
			}

			var objType = obj.GetType();
			var propertyInfosToExport = objType.GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance);

			sb.AppendFormat("new {0}()", objType.Name);
			sb.AppendLine();
			sb.AppendLine("{");

			foreach (var propertyInfo in propertyInfosToExport)
			{
				var value = propertyInfo.GetValue(obj, null);

				if (value == null
					|| _ignoredFields.Contains(propertyInfo.Name))
					continue;

				if (propertyInfo.PropertyType == typeof(string))
				{
					sb.AppendFormat("{0} = \"{1}\"", propertyInfo.Name, value);
				}
				else if ((propertyInfo.PropertyType == typeof(DateTime)
						  || propertyInfo.PropertyType == typeof(DateTime?)))
				{
					var dateTime = (DateTime)value;

					sb.AppendFormat("{0} = new DateTime({1}, {2}, {3}, {4}, {5}, {6}, {7})",
						propertyInfo.Name,
						dateTime.Year,
						dateTime.Month,
						dateTime.Day,
						dateTime.Hour,
						dateTime.Minute,
						dateTime.Second,
						dateTime.Millisecond);
				}
				else if ((propertyInfo.PropertyType == typeof(Guid)
						  || propertyInfo.PropertyType == typeof(Guid?)))
				{
					var guid = (Guid)value;

					sb.AppendFormat("{0} = Guid.Parse(\"{1}\")", propertyInfo.Name, guid);
				}
				else if ((propertyInfo.PropertyType == typeof(bool)
						  || propertyInfo.PropertyType == typeof(bool?)))
				{
					var strValue = (bool)value
										? "true"
										: "false";

					sb.AppendFormat("{0} = {1}", propertyInfo.Name, strValue);
				}
				else if ((propertyInfo.PropertyType == typeof(decimal)
						  || propertyInfo.PropertyType == typeof(decimal?)))
				{
					sb.AppendFormat("{0} = {1}m", propertyInfo.Name, value);
				}
				else
				{
					sb.AppendFormat("{0} = {1}", propertyInfo.Name, value);
				}

				sb.AppendLine(",");
			}
			sb.AppendLine("}");
		}
	}
}
