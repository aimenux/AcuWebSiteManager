namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	/// <summary>A class that describes an error in processing a credit card payment.</summary>
	public class CCError
	{
		/// <summary>Defines the error sources.</summary>
		public enum CCErrorSource
		{
			/// <summary>No error.</summary>
			None,
			/// <summary>An internal object error.</summary>
			Internal,
			/// <summary>A processing center error.</summary>
			ProcessingCenter,
			/// <summary>A network error.</summary>
			Network,
		}

		/// <summary>The error message.</summary>
		public string ErrorMessage;

		/// <summary>The source of the error.</summary>
		public CCErrorSource source;

		private static string[] _codes = { "NON", "INT", "PRC", "NET" };

		private static string[] _descr = { CCProcessingBase.Messages.NoError, CCProcessingBase.Messages.InternalError,
			CCProcessingBase.Messages.ProcessingCenterError, CCProcessingBase.Messages.NetworkError };

		/// <summary>Retrieves the code of the error.</summary>
		/// <param name="aErrSrc">The error source.</param>
		public static string GetCode(CCErrorSource aErrSrc)
		{
			return _codes[(int)aErrSrc];
		}

		/// <summary>Retrieves the description of the error.</summary>
		/// <param name="aErrSrc">The error source.</param>
		public static string GetDescription(CCErrorSource aErrSrc)
		{
			return _descr[(int)aErrSrc];
		}
	}
}
