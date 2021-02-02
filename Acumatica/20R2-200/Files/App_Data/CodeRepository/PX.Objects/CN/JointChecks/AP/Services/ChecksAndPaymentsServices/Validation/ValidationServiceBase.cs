using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;

namespace PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices.Validation
{
	public abstract class ValidationServiceBase
	{
		protected readonly APPaymentEntry Graph;
		protected VendorPreparedBalanceCalculationService VendorPreparedBalanceCalculationService;
		protected CashDiscountCalculationService CashDiscountCalculationService;

		protected ValidationServiceBase(APPaymentEntry graph)
		{
			Graph = graph;
		}

		protected IEnumerable<APAdjust> Adjustments => Graph.Adjustments.SelectMain();

		protected IEnumerable<APAdjust> ActualAdjustments => Adjustments.Where(adjustment => adjustment.Voided != true);

		protected IEnumerable<APAdjust> ActualBillAdjustments =>
			ActualAdjustments.Where(adjustment => adjustment.AdjdDocType == APDocType.Invoice);

		protected void InitializeServices(bool isPaymentByLine)
		{
			VendorPreparedBalanceCalculationService = new VendorPreparedBalanceCalculationService(Graph);
			CashDiscountCalculationService = isPaymentByLine
				? new CashDiscountPerLineCalculationService(Graph)
				: new CashDiscountCalculationService(Graph);
		}

		protected void ShowErrorMessage<TField>(object entity, string format, params object[] args)
			where TField : IBqlField
		{
			var cache = Graph.Caches[entity.GetType()];
			var fieldValue = cache.GetValue<TField>(entity);
			ShowErrorMessage<TField>(entity, fieldValue, format, args);
		}

		protected void ShowErrorMessage<TField>(object entity, object fieldValue, string format, params object[] args)
			where TField : IBqlField
		{
			var cache = Graph.Caches[entity.GetType()];
			var exception = new PXSetPropertyException(format, args);
			cache.RaiseExceptionHandling<TField>(entity, fieldValue, exception);
		}

		protected static void ShowErrorOnPersistIfRequired(PXCache cache, bool showErrorOnPersist)
		{
			if (showErrorOnPersist)
			{
				throw new PXException(ErrorMessages.RecordRaisedErrors, null, cache.DisplayName);
			}
		}
	}
}