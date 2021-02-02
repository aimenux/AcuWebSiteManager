using System;
using System.Runtime.Serialization;
using PX.Data;

namespace PX.Objects.Common.Exceptions
{
	public class RowNotFoundException : PXException
	{
		const string KeysSeparator = " ";

		public Type RowType { get; protected set; }
		public object[] Keys { get; protected set; }

		public RowNotFoundException(PXCache cache, params object[] keys)
			: base(Messages.CannotFindEntityByKeys, cache.DisplayName, string.Join(KeysSeparator, keys))
		{
			RowType = cache.GetItemType();
			Keys = keys;
		}

		public RowNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
