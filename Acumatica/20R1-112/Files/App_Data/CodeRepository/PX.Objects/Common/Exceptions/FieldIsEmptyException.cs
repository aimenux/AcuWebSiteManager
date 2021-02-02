using PX.Data;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace PX.Objects.Common.Exceptions
{
	/// <exclude/>
	public class FieldIsEmptyException : PXException
	{
		const string FieldSeparator = " - ";
		const string KeysSeparator = " ";

		public Type RowType { get; protected set; }
		public Type FieldType { get; protected set; }

		public static string GetErrorText(PXCache cache, object row, Type fieldType, params object[] keys)
		{
			return string.Format(PXMessages.LocalizeNoPrefix(ErrorMessages.FieldIsEmpty), GetFieldDescription(cache, row, fieldType, keys));
		}

		public FieldIsEmptyException(PXCache cache, object row, Type fieldType, params object[] keys)
			: base(ErrorMessages.FieldIsEmpty, GetFieldDescription(cache, row, fieldType, keys))
		{
			RowType = cache.GetItemType();
			FieldType = fieldType;
		}

		public FieldIsEmptyException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		static string GetFieldDescription(PXCache cache, object row, Type fieldType, object[] keys)
		{
			string description = cache.DisplayName;
			if (keys.Any())
				description += KeysSeparator + string.Join(KeysSeparator, keys);

			description += FieldSeparator;

			PXFieldState state = (PXFieldState)cache.GetStateExt(row, fieldType.Name);
			description += state.DisplayName;

			return description;
		}
	}
}
