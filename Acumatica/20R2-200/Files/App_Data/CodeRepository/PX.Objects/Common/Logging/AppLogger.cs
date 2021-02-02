using PX.Data;

namespace PX.Objects.Common.Logging
{
	public class AppLogger : IAppLogger
	{
		public virtual void WriteWarning(string message)
		{
			PXTrace.WriteWarning(message);
		}
	}
}
