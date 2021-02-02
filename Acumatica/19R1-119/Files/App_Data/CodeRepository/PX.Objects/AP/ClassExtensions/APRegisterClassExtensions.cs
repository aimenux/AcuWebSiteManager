using System.Collections.Generic;

namespace PX.Objects.AP
{
	public static class APRegisterClassExtensions
	{
		/// <summary>
		/// Returns an enumerable string array, which is included all 
		/// possible original document types for voiding document.
		/// </summary>
		public static IEnumerable<string> PossibleOriginalDocumentTypes(this APRegister voidcheck)
		{
			switch (voidcheck.DocType)
			{
				case APDocType.VoidCheck:
                case APDocType.VoidRefund:
				case APDocType.VoidQuickCheck:
                    return APPaymentType.GetVoidedAPDocType(voidcheck.DocType);

				default:
					return new[] { voidcheck.DocType };
			}
		}

		/// <summary>
		/// Indicates whether the record is an original Retainage document
		/// with <see cref="APRegister.RetainageApply"/> flag equal to true
		/// and <see cref="APRegister.CuryRetainageTotal"/> amount greater than zero.
		/// </summary>
		public static bool IsOriginalRetainageDocument(this APRegister doc)
		{
			return
				doc.CuryRetainageTotal > 0m &&
				doc.RetainageApply == true;
		}

		/// <summary>
		/// Indicates whether the record is a child Retainage document
		/// with <see cref="APRegister.IsRetainageDocument"/> flag equal to true
		/// and existing reference on the original Retainage document.
		/// </summary>
		public static bool IsChildRetainageDocument(this APRegister doc)
		{
			return
				doc.IsRetainageDocument == true &&
				!string.IsNullOrEmpty(doc.OrigDocType) &&
				!string.IsNullOrEmpty(doc.OrigRefNbr);
		}
	}
}
