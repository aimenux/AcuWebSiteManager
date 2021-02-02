using System.Runtime.Serialization;

using PX.Data;

namespace PX.Objects.Common.Exceptions
{
	/// <summary>
	/// Common class of errors that happen during entity release process.
	/// </summary>
	public class ReleaseException : PXException
	{
		public ReleaseException(string format, params object[] args)
			: base(format, args)
		{ }

		public ReleaseException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
	}
}
