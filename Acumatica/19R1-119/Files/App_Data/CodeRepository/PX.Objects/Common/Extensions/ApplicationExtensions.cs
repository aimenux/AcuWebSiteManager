using PX.Objects.Common.Abstractions;

namespace PX.Objects.Common.Extensions
{
	public static class ApplicationExtensions
	{
		/// <summary>
		/// Returns <c>true</c> if the specified document is on the adjusting side of the
		/// specified application, <c>false</c> otherwise. For example, this method will
		/// return <c>true</c> if you call it on an application of a payment to an invoice, 
		/// and specify the payment as the argument.
		/// </summary>
		public static bool IsOutgoingApplicationFor(this IDocumentAdjustment application, IDocumentKey document)
			=> document.DocType == application.AdjgDocType
			&& document.RefNbr == application.AdjgRefNbr;

		/// <summary>
		/// Returns <c>true</c> if the specified document is on the adjusted side of the
		/// specified application, <c>false</c> otherwise. For example, this method will
		/// return <c>true</c> if you call it on an application of a payment to an invoice, 
		/// and specify the invoice as the argument.
		/// </summary>
		public static bool IsIncomingApplicationFor(this IDocumentAdjustment application, IDocumentKey document)
			=> document.DocType == application.AdjdDocType
			&& document.RefNbr == application.AdjdRefNbr;

		/// <summary>
		/// Returns <c>true</c> if and only if the calling application corresponds to the
		/// specified document (on either the adjusting or the adjusted side).
		/// </summary>
		public static bool IsApplicationFor(this IDocumentAdjustment application, IDocumentKey document)
			=> application.IsOutgoingApplicationFor(document)
			|| application.IsIncomingApplicationFor(document);
	}
}
