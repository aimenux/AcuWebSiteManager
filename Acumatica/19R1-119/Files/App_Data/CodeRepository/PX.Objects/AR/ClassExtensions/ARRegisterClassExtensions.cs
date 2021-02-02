using System.Collections.Generic;

namespace PX.Objects.AR
{
	public static class ARRegisterClassExtensions
	{
		/// <summary>
		/// Returns an enumerable string array, which is included all 
		/// possible original document types for voiding document.
		/// </summary>
		public static IEnumerable<string> PossibleOriginalDocumentTypes(this ARRegister voidpayment)
		{
			switch (voidpayment.DocType)
			{
				case ARDocType.CashReturn:
					return new[] { ARDocType.CashSale };

                case ARDocType.VoidRefund:
                case ARDocType.VoidPayment:
					return ARPaymentType.GetVoidedARDocType(voidpayment.DocType);

                default:
					return new[] { voidpayment.DocType };
			}
		}

		/// <summary>
		/// Indicates whether the record is an original Retainage document
		/// with <see cref="ARRegister.RetainageApply"/> flag equal to true
		/// and <see cref="ARRegister.CuryRetainageTotal"/> amount greater than zero.
		/// </summary>
		public static bool IsOriginalRetainageDocument(this ARRegister doc)
		{
			return
				doc.CuryRetainageTotal > 0m &&
				doc.RetainageApply == true;
		}

		/// <summary>
		/// Indicates whether the record is a child Retainage document
		/// with <see cref="ARRegister.IsRetainageDocument"/> flag equal to true
		/// and existing reference on the original Retainage document.
		/// </summary>
		public static bool IsChildRetainageDocument(this ARRegister doc)
		{
			return
				doc.IsRetainageDocument == true &&
				!string.IsNullOrEmpty(doc.OrigDocType) &&
				!string.IsNullOrEmpty(doc.OrigRefNbr);
		}
	}
}
