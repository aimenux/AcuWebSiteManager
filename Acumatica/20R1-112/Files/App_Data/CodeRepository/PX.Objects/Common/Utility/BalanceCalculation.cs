using System;

using PX.Data;

using PX.Objects.Common.Abstractions;
using PX.Objects.Common.Extensions;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CM;

namespace PX.Objects.Common
{
	public static class BalanceCalculation
	{
		/// <summary>
		/// For an application of a payment to an invoice, calculates the 
		/// payment's document balance in the currency of the target invoice 
		/// document.
		/// </summary>
		/// <param name="paymentBalanceInBase">
		/// The balance of the payment document in the base currency.
		/// </param>
		/// <param name="paymentBalanceInCurrency">
		/// The balance of the payment document in the document's currency.
		/// Will be re-used in case when the invoice's currency is the same
		/// as the payment's currency.
		/// </param>
		public static decimal CalculateApplicationDocumentBalance(
			PXCache cache,
			CurrencyInfo paymentCurrencyInfo,
			CurrencyInfo invoiceCurrencyInfo,
			decimal? paymentBalanceInBase,
			decimal? paymentBalanceInCurrency)
		{
			decimal applicationDocumentBalance;

			if (string.Equals(paymentCurrencyInfo.CuryID, invoiceCurrencyInfo.CuryID))
			{
				applicationDocumentBalance = (paymentBalanceInCurrency ?? 0m);
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvCury(
					cache,
					invoiceCurrencyInfo,
					paymentBalanceInBase ?? 0m,
					out applicationDocumentBalance);
			}

			return applicationDocumentBalance;
		}

		/// <summary>
		/// For an application of a payment to an invoice, calculates the 
		/// value of the <see cref="ARAdjust.CuryDocBal"/> field, which is
		/// the remaining balance of the applied payment.
		/// </summary>
		public static void CalculateApplicationDocumentBalance(
			PXCache cache,
			ARPayment payment,
			IAdjustment application,
			CurrencyInfo paymentCurrencyInfo,
			CurrencyInfo invoiceCurrencyInfo)
		{
			decimal CuryDocBal = CalculateApplicationDocumentBalance(
				cache,
				paymentCurrencyInfo,
				invoiceCurrencyInfo,
				payment.Released == true ? payment.DocBal : payment.OrigDocAmt,
				payment.Released == true ? payment.CuryDocBal : payment.CuryOrigDocAmt);

			if (application == null) return;

			if (application.Released == false)
			{
				if (application.CuryAdjdAmt > CuryDocBal)
				{
					// TODO: if reconsidered need to calculate RGOL.
					// -
					application.CuryDocBal = CuryDocBal;
					application.CuryAdjdAmt = 0m;
				}
				else
				{
					application.CuryDocBal = CuryDocBal - application.CuryAdjdAmt;
				}
			}
			else
			{
				application.CuryDocBal = CuryDocBal;
			}
		}

		/// <summary>
		/// Given an application, returns a <see cref="FullBalanceDelta"/>
		/// object indicating by how much the application should reduce the
		/// balances of the adjusting and the adjusted document.
		/// </summary>
		public static FullBalanceDelta GetFullBalanceDelta(this IAdjustmentAmount application)
			=> new FullBalanceDelta
			{
				BaseAdjustingBalanceDelta = (application.AdjAmt ?? 0m),

				CurrencyAdjustingBalanceDelta = (application.CuryAdjgAmt ?? 0m),

				BaseAdjustedExtraAmount =
					(application.AdjDiscAmt ?? 0m)
					+ (application.AdjThirdAmount ?? 0m)
					+ ((application.ReverseGainLoss == true ? -application.RGOLAmt : application.RGOLAmt) ?? 0m),

				CurrencyAdjustedExtraAmount =
					(application.CuryAdjdDiscAmt ?? 0m)
					+ (application.CuryAdjdThirdAmount ?? 0m),

				CurrencyAdjustingExtraAmount =
					(application.CuryAdjgDiscAmt ?? 0m)
					+ (application.CuryAdjgThirdAmount ?? 0m),

				BaseAdjustedBalanceDelta =
					(application.AdjAmt ?? 0m)
					+ (application.AdjDiscAmt ?? 0m)
					+ (application.AdjThirdAmount ?? 0m)
					+ ((application.ReverseGainLoss == true ? -application.RGOLAmt : application.RGOLAmt) ?? 0m),

				CurrencyAdjustedBalanceDelta =
					(application.CuryAdjdAmt ?? 0m)
					+ (application.CuryAdjdDiscAmt ?? 0m)
					+ (application.CuryAdjdThirdAmount ?? 0m),
			};

		/// <summary>
		/// Gets the sign with which the document of a particular 
		/// type affects the business account balance.
		/// </summary>
		public static decimal GetBalanceSign(string documentType, string module)
		{
			switch (module)
			{
				case BatchModule.AP:
					return APDocType.SignBalance(documentType) ?? 0m;
				case BatchModule.AR:
					return ARDocType.SignBalance(documentType) ?? 0m;
				default:
					throw new PXException();
			}
		}

		/// <summary>
		/// Get the sign with which the adjusting document affects the 
		/// business account balance.
		/// </summary>
		public static decimal GetSignAffectingBusinessAccountBalanceAdjusting(this IDocumentAdjustment application)
			=> GetBalanceSign(application.AdjgDocType, application.Module);

		/// <summary>
		/// Gets the sign with which the adjusted document affects the
		/// business account balance.
		/// </summary>
		public static decimal GetSignAffectingBusinessAccountBalanceAdjusted(this IDocumentAdjustment application)
			=> GetBalanceSign(application.AdjdDocType, application.Module);

		/// <summary>
		/// Gets the sign correction necessary to correctly calculate balances
		/// of the adjusting document inside the <see cref="AdjustBalance{TDocument, TAdjustment}(TDocument, TAdjustment, decimal)"/> method.
		/// </summary>
		public static decimal GetDocumentBalanceSignCorrection(this IDocumentAdjustment application)
		{
			if (application.AdjgDocType == ARDocType.Refund || application.AdjgDocType == ARDocType.VoidRefund)
			{
				return -1m;
			}

			return 1m;
		}

		public static void AdjustBalance<TDocument, TAdjustment>(
			this TDocument document,
			TAdjustment application)
			where TDocument : IBalance, CM.IRegister
			where TAdjustment : IAdjustmentAmount, IDocumentAdjustment
		{
			AdjustBalance<TDocument, TAdjustment>(document, application, 1m);
		}

		public static void AdjustBalance<TDocument, TAdjustment>(
			this TDocument document,
			TAdjustment application,
			decimal sign)
			where TDocument : IBalance, CM.IRegister
			where TAdjustment : IAdjustmentAmount, IDocumentAdjustment
		{
			if (document == null) throw new ArgumentNullException(nameof(document));
			if (application == null) throw new ArgumentNullException(nameof(application));

			if (!application.IsApplicationFor(document))
			{
				throw new PXException(Messages.ApplicationDoesNotCorrespondToDocument);
			}

			FullBalanceDelta balanceAdjustment = application.GetFullBalanceDelta();

			if (application.IsIncomingApplicationFor(document))
			{
				document.DocBal -= balanceAdjustment.BaseAdjustedBalanceDelta * sign;
				document.CuryDocBal -= balanceAdjustment.CurrencyAdjustedBalanceDelta * sign;
			}
			else if (application.IsOutgoingApplicationFor(document))
			{
				decimal balanceAdjustmentSign = 
					application.GetSignAffectingBusinessAccountBalanceAdjusted() * application.GetDocumentBalanceSignCorrection();
				
				document.DocBal -= balanceAdjustmentSign * balanceAdjustment.BaseAdjustingBalanceDelta * sign;
				document.CuryDocBal -= balanceAdjustmentSign * balanceAdjustment.CurrencyAdjustingBalanceDelta * sign;
			}
		}

		public static bool HasBalance(this IBalance document)
			=> document.DocBal.IsNonZero()
			&& document.CuryDocBal.IsNonZero();

		/// <summary>
		/// Forces the document's control total amount to be equal to the 
		/// document's outstanding balance, afterwards updating the
		/// record in the relevant cache.
		/// </summary>
		public static void ForceDocumentControlTotals(
			PXGraph graph,
			IInvoice invoice)
		{
			if (invoice.CuryOrigDocAmt != invoice.CuryDocBal)
			{
				invoice.CuryOrigDocAmt = (invoice.CuryDocBal ?? 0m);
				graph.Caches[invoice.GetType()].Update(invoice);
			}
		}
	}
}