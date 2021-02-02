using System.Runtime.Serialization;

using PX.Data;

namespace PX.Objects.Common.DataIntegrity
{
	public class DataIntegrityException : PXException
	{
		public string InconsistencyCode { get; private set; } = "UNKNOWN";

		public DataIntegrityException()
			: base(Messages.DataIntegrityErrorDuringProcessingDefault)
		{ }

		public DataIntegrityException(string inconsistencyCode, string message)
			: base(message)
		{
			InconsistencyCode = inconsistencyCode;
		}

		public DataIntegrityException(string inconsistencyCode, string format, params object[] args)
			: base(format, args)
		{
			InconsistencyCode = inconsistencyCode;
		}

		public DataIntegrityException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
	}
}