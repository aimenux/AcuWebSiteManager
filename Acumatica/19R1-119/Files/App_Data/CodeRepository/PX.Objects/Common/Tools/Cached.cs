using System;

namespace PX.Objects.Common.Tools
{
	[Obsolete(PX.Objects.Common.InternalMessages.ClassIsObsoleteAndWillBeRemoved2019R2)]
	public static class Cached
	{
		public static TValue GetValue<TValue>(ref TValue value, Func<TValue> initializer)
		{
			if (value == null)
			{
				value = initializer();
			}

			return value;
		}
	}
}
