using System.Collections.Generic;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	/// <summary>A class that holds the responses returned by the processing center.</summary>
	public class APIResponse
	{
		/// <summary>
		/// Must be <tt>true</tt> if the request was completed without any errors and <tt>false</tt> otherwise.
		/// </summary>
		public bool isSucess = false;
		/// <summary>
		/// Contains the error messages received from the processing center.
		/// </summary>
		public Dictionary<string, string> Messages;
		/// <summary>Specifies the error source.</summary>
		public CCError.CCErrorSource ErrorSource = CCError.CCErrorSource.None;

		/// <exclude/>
		public APIResponse()
		{
			Messages = new Dictionary<string, string>();
		}
	}
}
