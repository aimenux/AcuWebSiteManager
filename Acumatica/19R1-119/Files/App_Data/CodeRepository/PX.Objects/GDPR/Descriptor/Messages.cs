using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.GDPR
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		#region Validation and Processing Messages

		public const string Prefix = "GDPR Error";

		public const string NoConsent = "Consent to the processing of personal data has not been given or has expired.";
		public const string IsConsented = "Consented to the Processing of Personal Data";
		public const string DateOfConsent = "Date of Consent";
		public const string ConsentExpires = "Consent Expires";
		public const string ConsentExpired = "The consent has expired.";
		public const string ConsentDateNull = "No consent date has been specified.";

		public const string NotPseudonymized = "Not Pseudonymized";
		public const string Pseudonymized = "Pseudonymized";
		public const string Erased = "Erased";

		public const string NavigateDeleted = "A deleted contact cannot be opened.";
		public const string OpenContact = "Open Contact";

		public const string Pseudonymize = "Pseudonymize";
		public const string PseudonymizeAll = "Pseudonymize All";

		public const string Erase = "Erase";
		public const string EraseAll = "Erase All";

		public const string Restore = "Restore";

		public const string ContactOPTypeForIndex = "Opportunity Contact {0}";
		
		#endregion
		
		public static string GetLocal(string message)
		{
			return PXLocalizer.Localize(message, typeof(Messages).FullName);
		}
	}
}
