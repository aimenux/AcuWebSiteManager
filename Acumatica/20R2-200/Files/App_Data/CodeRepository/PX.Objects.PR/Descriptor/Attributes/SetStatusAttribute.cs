using System;
using PX.Data;

namespace PX.Objects.PR
{
	public class SetStatusAttribute : PXEventSubscriberAttribute, IPXRowUpdatingSubscriber, IPXRowInsertingSubscriber
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), nameof(PRPayment.hold),
				(cache, e) =>
				{
					PXBoolAttribute.ConvertValue(e);

					var document = e.Row as PRPayment;
					if (document != null)
					{
						StatusSet(cache, document);
					}
				});

			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), nameof(PRPayment.status),
				(cache, e) => { e.NewValue = cache.GetValue<PRPayment.status>(e.Row); });

			sender.Graph.RowSelected.AddHandler(sender.GetItemType(),
				(cache, e) =>
				{
					var document = e.Row as PRPayment;

					if (document != null)
					{
						StatusSet(cache, document);
					}
				});
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var item = e.Row as PRPayment;
			if(item == null)
			{
				return;
			}

			StatusSet(sender, item);
		}

		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var item = e.NewRow as PRPayment;
			if (item == null)
			{
				return;
			}

			StatusSet(sender, item);
		}

		protected virtual void StatusSet(PXCache sender, PRPayment document)
		{
			switch (document.DocType)
			{
				case PayrollType.VoidCheck:
					SetVoidPaymentStatus(document);
					break;
				default:
					SetPaymentStatus(sender, document);
					break;
			}
		}

		private void SetVoidPaymentStatus(PRPayment document)
		{
			if (document.Closed == true)
			{
				document.Status = PaymentStatus.Closed;
				return;
			}
			if (document.Released == true)
			{
				document.Status = PaymentStatus.Released;
				return;
			}
			if (document.Hold == true)
			{
				document.Status = PaymentStatus.Hold;
				return;
			}

			document.Status = PaymentStatus.Open;
		}

		private void SetPaymentStatus(PXCache sender, PRPayment document)
		{
			if (document.Closed == true || (document.Released == true && document.HasUpdatedGL == false))
			{
				document.Status = PaymentStatus.Closed;
				return;
			}
			if (document.Voided == true)
			{
				document.Status = PaymentStatus.Voided;
				return;
			}
			if (document.Hold == true)
			{
				document.Status = PaymentStatus.Hold;
				return;
			}
			if ((document.Calculated == true || document.DocType == PayrollType.Adjustment) && document.Printed == false && document.Released == false)
			{
				PXSetup<PRSetup> setup = new PXSetup<PRSetup>(sender.Graph);
				document.Status = document.NetAmount > 0 && setup.Current.UpdateGL == true ?
					PaymentStatus.PendingPrintOrPayment : PaymentStatus.Open;
				return;
			}
			if (document.Released == false && document.Printed == true)
			{
				document.Status = PaymentStatus.CheckPrintedOrPaid;
				return;
			}
			if (document.Released == true && document.LiabilityPartiallyPaid == true)
			{
				document.Status = PaymentStatus.LiabilityPartiallyPaid;
				return;
			}
			if (document.Released == true)
			{
				document.Status = PaymentStatus.Released;
				return;
			}

			document.Status = PaymentStatus.NeedCalculation;
		}
	}
}
