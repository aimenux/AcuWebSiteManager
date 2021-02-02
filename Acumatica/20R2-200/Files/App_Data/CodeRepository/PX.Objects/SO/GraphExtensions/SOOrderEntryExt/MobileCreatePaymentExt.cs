using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.CS;
using PX.Objects.Extensions.PaymentTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
    public class MobileCreatePaymentExt : PXGraphExtension<CreatePaymentExt, SOOrderEntry>
    {
        #region Buttons

        public PXAction<SOOrder> mobileCreatePayment;
		[PXUIField(DisplayName = "Create Payment", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, Tooltip = "Create Payment")]
		protected virtual void MobileCreatePayment()
		{
			if (Base.Document.Current != null)
			{
				Base1.CheckTermsInstallmentType();

				Base.Save.Press();

				PXGraph target;
				MobileCreatePaymentProc(Base.Document.Current, out target);

				throw new PXPopupRedirectException(target, "New Payment", true);
			}
		}

		public PXAction<SOOrder> mobileCreatePrepayment;
		[PXUIField(DisplayName = "Create Prepayment", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, Tooltip = "Create Prepayment")]
		protected virtual void MobileCreatePrepayment()
		{
			if (Base.Document.Current != null)
			{
				Base1.CheckTermsInstallmentType();

				Base.Save.Press();

				PXGraph target;
				MobileCreatePaymentProc(Base.Document.Current, out target, ARPaymentType.Prepayment);

				throw new PXPopupRedirectException(target, "New Payment", true);
			}
		}

        #endregion // Buttons

        #region SOOrder events

		protected virtual void _(Events.RowSelected<SOOrder> eventArgs)
        {
			bool PaymentsAndApplicationsEnabled = Base.soordertype.Current.CanHaveApplications;
			mobileCreatePayment.SetEnabled(PaymentsAndApplicationsEnabled && eventArgs.Cache.GetStatus(eventArgs.Row) != PXEntryStatus.Inserted);
			mobileCreatePrepayment.SetEnabled(PaymentsAndApplicationsEnabled && eventArgs.Cache.GetStatus(eventArgs.Row) != PXEntryStatus.Inserted);
		}

        #endregion // SOOrder events

        #region Methods

		public virtual void MobileCreatePaymentProc(SOOrder order, out PXGraph target, string paymentType = ARPaymentType.Payment)
		{
			ARPaymentEntry docgraph = PXGraph.CreateInstance<ARPaymentEntry>();
			target = docgraph;

			docgraph.Clear();
			ARPayment payment = new ARPayment()
			{
				DocType = paymentType,
			};

			AROpenPeriodAttribute.SetThrowErrorExternal<ARPayment.adjFinPeriodID>(docgraph.Document.Cache, true);
			payment = PXCache<ARPayment>.CreateCopy(docgraph.Document.Insert(payment));
			AROpenPeriodAttribute.SetThrowErrorExternal<ARPayment.adjFinPeriodID>(docgraph.Document.Cache, false);

			payment.CustomerID = order.CustomerID;
			payment.CustomerLocationID = order.CustomerLocationID;
			payment.PaymentMethodID = order.PaymentMethodID;
			payment.PMInstanceID = order.PMInstanceID;
			payment.CuryOrigDocAmt = 0m;
			payment.DocDesc = order.OrderDesc;
			payment.CashAccountID = order.CashAccountID;
			payment = docgraph.Document.Update(payment);

			MobileInsertSOAdjustments(order, docgraph, payment);

			if (payment.CuryOrigDocAmt == 0m)
			{
				payment.CuryOrigDocAmt = payment.CurySOApplAmt;
				payment = docgraph.Document.Update(payment);
			}
		}

		protected virtual void MobileInsertSOAdjustments(SOOrder order, ARPaymentEntry docgraph, ARPayment payment)
		{
			SOAdjust adj = new SOAdjust()
			{
				AdjdOrderType = order.OrderType,
				AdjdOrderNbr = order.OrderNbr
			};

			try
			{
				docgraph.SOAdjustments.Insert(adj);
			}
			catch (PXSetPropertyException)
			{
				payment.CuryOrigDocAmt = 0m;
			}
		}

		#endregion Methods
	}
}
