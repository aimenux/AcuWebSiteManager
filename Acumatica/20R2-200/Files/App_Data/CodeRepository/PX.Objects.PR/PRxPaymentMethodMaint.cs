using PX.Data;
using PX.Objects.CA;
using PX.Objects.CS;

namespace PX.Objects.PR
{
	public class PRxPaymentMethodMaint : PXGraphExtension<PaymentMethodMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region CacheAttached

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "DisplayName", "AP/PR Default")]
		protected virtual void _(Events.CacheAttached<PaymentMethodAccount.aPIsDefault> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "DisplayName", "AP/PR - Suggest Next Number")]
		protected virtual void _(Events.CacheAttached<PaymentMethodAccount.aPAutoNextNbr> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "DisplayName", "AP/PR Last Reference Number")]
		protected virtual void _(Events.CacheAttached<PaymentMethodAccount.aPLastRefNbr> e) { }

		#endregion CacheAttached

		#region Events

		protected virtual void _(Events.RowSelected<PaymentMethod> e, PXRowSelected del)
		{
			del?.Invoke(e.Cache, e.Args);

			var rowExt = PXCache<PaymentMethod>.GetExtension<PRxPaymentMethod>(e.Row);
			bool useForAPorPR = e.Row.UseForAP == true || rowExt.UseForPR == true;
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPIsDefault>(Base.CashAccounts.Cache, null, useForAPorPR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPAutoNextNbr>(Base.CashAccounts.Cache, null, useForAPorPR);
			PXUIFieldAttribute.SetVisible<PaymentMethodAccount.aPLastRefNbr>(Base.CashAccounts.Cache, null, useForAPorPR);
		}

		protected virtual void PaymentMethod_UseForPR_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (PaymentMethod)e.Row;
			var rowExt = PXCache<PaymentMethod>.GetExtension<PRxPaymentMethod>(row);

			if (rowExt.PRCheckReportID == null)
			{
				sender.SetDefaultExt<PRxPaymentMethod.prCheckReportID>(row);
			}
			if (rowExt.PRPrintChecks == null)
			{
				sender.SetDefaultExt<PRxPaymentMethod.prPrintChecks>(row);
			}
			if (rowExt.PRCreateBatchPayment == null)
			{
				sender.SetDefaultExt<PRxPaymentMethod.prCreateBatchPayment>(row);
			}
		}

		protected virtual void PaymentMethod_PRProcessing_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (PaymentMethod)e.Row;
			var rowExt = PXCache<PaymentMethod>.GetExtension<PRxPaymentMethod>(row);
			switch (rowExt.PRProcessing)
			{
				case PRxPaymentMethod.prProcessing.CreateBatchPayment:
					rowExt.PRCreateBatchPayment = true;
					rowExt.PRPrintChecks = false;
					break;
				case PRxPaymentMethod.prProcessing.PrintChecks:
				default:
					rowExt.PRCreateBatchPayment = false;
					rowExt.PRPrintChecks = true;
					break;
			}
		}

		protected virtual void _(Events.RowUpdated<PaymentMethod> e, PXRowUpdated del)
		{
			del?.Invoke(e.Cache, e.Args);

			var methodExt = PXCache<PaymentMethod>.GetExtension<PRxPaymentMethod>(e.Row);
			if (methodExt.UseForPR == false)
			{
				foreach (PaymentMethodAccount account in Base.CashAccounts.Select())
				{
					var accountExt = PXCache<PaymentMethodAccount>.GetExtension<PRxPaymentMethodAccount>(account);
					accountExt.UseForPR = false;
					Base.CashAccounts.Update(account);
				}
			}
		}

		protected virtual void _(Events.RowSelected<PaymentMethodAccount> e, PXRowSelected del)
		{
			del?.Invoke(e.Cache, e.Args);

			var rowExt = PXCache<PaymentMethodAccount>.GetExtension<PRxPaymentMethodAccount>(e.Row);
			bool useForAPorPR = e.Row.UseForAP == true || rowExt.UseForPR == true;
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPIsDefault>(e.Cache, e.Row, useForAPorPR);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPAutoNextNbr>(e.Cache, e.Row, useForAPorPR);
			PXUIFieldAttribute.SetEnabled<PaymentMethodAccount.aPLastRefNbr>(e.Cache, e.Row, useForAPorPR);
		}

		protected virtual void _(Events.RowUpdating<PaymentMethodAccount> e, PXRowUpdating del)
		{
			del?.Invoke(e.Cache, e.Args);

			if (e.NewRow != null && !e.Cache.ObjectsEqual<PaymentMethodAccount.useForAP, PRxPaymentMethodAccount.useForPR>(e.Row, e.NewRow))
			{
				var newRowExt = PXCache<PaymentMethodAccount>.GetExtension<PRxPaymentMethodAccount>(e.NewRow);
				if (e.NewRow.UseForAP == false && newRowExt.UseForPR == false)
				{
					e.NewRow.APIsDefault = false;
				}
				else
				{
					e.NewRow.APIsDefault = e.Row.APIsDefault;
				}
			}
		}

		#endregion Events
	}
}
