using PX.Data;
using PX.Objects.CS;
using System.Collections.Generic;
using System.Linq;

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

		/// <summary>
		/// Indicates whether the record has zero document balance and zero lines balances
		/// depending on <see cref="ARRegister.PaymentsByLinesAllowed"/> flag value.
		/// </summary>
		public static bool HasZeroBalance<TDocBal, TLineBal>(this ARRegister document, PXGraph graph)
			where TDocBal : IBqlField
			where TLineBal : IBqlField
		{
			PXCache cache = graph.Caches[typeof(ARRegister)];
			return (cache.GetValue<TDocBal>(document) as decimal? ?? 0m) == 0m && (document.PaymentsByLinesAllowed != true ||
				// Select + AsEnumerable here to check Any() on merged cache.
				!PXSelect<ARTran,
					Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
						And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
						And<TLineBal, NotEqual<decimal0>>>>>
					.Select(graph, document.DocType, document.RefNbr).AsEnumerable().Any());
		}
	}
}
