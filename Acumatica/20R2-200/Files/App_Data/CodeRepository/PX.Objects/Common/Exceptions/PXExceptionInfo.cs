using PX.Data;
using System;

namespace PX.Objects.Common.Exceptions
{
	public class PXExceptionInfo
	{
		public string MessageFormat { get; }

		public object[] MessageArguments { get; set; }

		public PXErrorLevel? ErrorLevel { get; set; }

		public PXExceptionInfo(string messageFormat, params object[] messageArgs)
		{
			MessageFormat = messageFormat;
			MessageArguments = messageArgs ?? Array.Empty<object>();
		}

		public PXExceptionInfo(PXErrorLevel errorLevel, string messageFormat, params object[] messageArgs)
			: this(messageFormat, messageArgs)
		{
			ErrorLevel = errorLevel;
		}
	}
}
