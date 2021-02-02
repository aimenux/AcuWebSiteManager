using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Commerce.Core;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.SM;
using PX.Objects.CR;
using PX.Objects.TX;
using System.Collections;
using PX.Api.ContractBased.Models;
using PX.Objects.IN;
using System.Reflection;
using PX.Common;

namespace PX.Commerce.Objects
{
	public class BCSOOrderEntryExt : PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		public delegate void PersistDelegate();
		[PXOverride]
		public void Persist(PersistDelegate handler)
		{
			SOOrder entry = Base.Document.Current;
			BCAPISyncScope.BCSyncScopeContext context = BCAPISyncScope.GetScoped();
			AdjustAppliedtoOrderAmount(entry, context);
			handler();
		}

		protected virtual void AdjustAppliedtoOrderAmount(SOOrder entry, BCAPISyncScope.BCSyncScopeContext context)
		{
			if (context != null)
			{
				//Adjust applied to order field to handle refunds flow
				var sOAdjusts = Base.Adjustments.Select();
				if (sOAdjusts.Count > 0)
				{
					var appliedTotal = sOAdjusts?.ToList()?.Sum(x => x.GetItem<SOAdjust>().CuryAdjdAmt ?? 0) ?? 0;
					if (entry.CuryOrderTotal < appliedTotal)
					{
						decimal? difference = appliedTotal - entry.CuryOrderTotal;
						foreach (var soadjust in sOAdjusts)
						{
							var adjust = soadjust.GetItem<SOAdjust>();
							if (difference == 0) break;
							if (adjust.CuryAdjdAmt > 0)
							{
								if (difference >= adjust.CuryAdjdAmt)
									adjust.CuryAdjdAmt = difference = difference - adjust.CuryAdjdAmt;
								else if (difference < adjust.CuryAdjdAmt)
								{
									adjust.CuryAdjdAmt = adjust.CuryAdjdAmt - difference;
									difference = 0;
								}

								Base.Adjustments.Update(adjust);
							}
						}
					}
					else if (entry.CuryOrderTotal > appliedTotal)
					{
						decimal? difference =entry.CuryOrderTotal -appliedTotal;
						foreach (var soadjust in sOAdjusts)
						{
							var adjust = soadjust.GetItem<SOAdjust>();
							if (difference == 0) break;
							decimal? balance = adjust.CuryOrigDocAmt - adjust.CuryAdjdAmt;
							if (balance > 0)
							{
								if (difference <= balance)
								{
									adjust.CuryAdjdAmt =adjust.CuryAdjdAmt + difference;
									difference = 0;
								}
								else if (difference > balance)
								{
									difference = difference - balance;
									adjust.CuryAdjdAmt += balance;
								}
								adjust.CuryDocBal = null;

								Base.Adjustments.Update(adjust);
							}
						}
					}
				}
			}
		}

		public void SOOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOOrder order = (SOOrder)e.Row;
			BCAPISyncScope.BCSyncScopeContext context = BCAPISyncScope.GetScoped();
			if (context != null && order != null)
			{

				if (String.IsNullOrEmpty(order.ShipVia)
					&& ((order.OverrideFreightAmount == true && order.CuryFreightAmt > 0)
						|| order.CuryPremiumFreightAmt > 0))
					throw new PXException(BCObjectsMessages.OrderMissingShipVia);
				
			}
		}
		protected void SOOrder_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e, PXFieldDefaulting baseHandler)
		{
			baseHandler?.Invoke(sender, e);

			if (e.NewValue == null)
			{
				BCAPISyncScope.BCSyncScopeContext context = BCAPISyncScope.GetScoped();
				if (context == null) return;

				BCBindingExt store = BCBindingExt.PK.Find(Base, context.Binding);
				if (store != null && !store.SyncTaxes.Equals(BCTaxSyncAttribute.NoSync))
					e.NewValue = store.DefaultTaxZoneID;
			}
		}
	
		public bool? isTaxValid = null;
		protected virtual void _(Events.FieldUpdated<SOOrder, SOOrder.isTaxValid> e)
		{
			if (e.Row == null || e.NewValue == null) return;

			if (BCAPISyncScope.IsScoped() && (e.NewValue as Boolean?) == true)
			{
				isTaxValid = true;
			}
		}

		//Sync Time 
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PX.Commerce.Core.BCSyncExactTime()]
		public void SOOrder_LastModifiedDateTime_CacheAttached(PXCache sender) { }


		protected virtual void _(Events.RowPersisting<SOOrder> e)
		{
			if (e.Row == null || (e.Operation & PXDBOperation.Command) != PXDBOperation.Update) return;
			Object oldRow = e.Cache.GetOriginal(e.Row);

			List<Type> monitoringTypes = new List<Type>();
			monitoringTypes.Add(typeof(SOOrder.customerID));
			monitoringTypes.Add(typeof(SOOrder.customerLocationID));
			monitoringTypes.Add(typeof(SOOrder.curyID));
			monitoringTypes.Add(typeof(SOOrder.orderQty));
			monitoringTypes.Add(typeof(SOOrder.curyDiscTot));
			monitoringTypes.Add(typeof(SOOrder.curyTaxTotal));
			monitoringTypes.Add(typeof(SOOrder.curyOrderTotal));
			monitoringTypes.Add(typeof(SOOrder.curyFreightTot));
			monitoringTypes.Add(typeof(SOOrder.shipVia));

			if (e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted && !BCAPISyncScope.IsScoped() && ((bool?)e.Cache.GetValue<BCSOOrderExt.externalOrderOriginal>(e.Row) == true)
				&& monitoringTypes.Any(t => !object.Equals(e.Cache.GetValue(e.Row, t.Name), e.Cache.GetValue(oldRow, t.Name))))
			{
				e.Cache.SetValueExt<BCSOOrderExt.externalOrderOriginal>(e.Row, false);
			}
		}
		protected virtual void _(Events.RowPersisting<SOLine> e)
		{
			if (e.Row == null || (e.Operation & PXDBOperation.Command) != PXDBOperation.Update) return;
			Object oldRow = e.Cache.GetOriginal(e.Row);

			List<Type> monitoringTypes = new List<Type>();
			monitoringTypes.Add(typeof(SOLine.inventoryID));
			monitoringTypes.Add(typeof(SOLine.curyDiscAmt));
			monitoringTypes.Add(typeof(SOLine.curyLineAmt));

			if (e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted && !BCAPISyncScope.IsScoped()
				&& ((bool?)Base.Document.Cache.GetValue<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current) == true)
				&& monitoringTypes.Any(t => !object.Equals(e.Cache.GetValue(e.Row, t.Name), e.Cache.GetValue(oldRow, t.Name))))
			{
				Base.Document.Cache.SetValueExt<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current, false);
			}
		}
		protected virtual void _(Events.RowUpdated<SOBillingAddress> e)
		{
			if (e.Row == null || e.OldRow == null || Base.Document.Current == null) return;

			if (e.ExternalCall && e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted && !BCAPISyncScope.IsScoped()
				&& ((bool?)Base.Document.Cache.GetValue<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current) == true))
			{
				Base.Document.Cache.SetValueExt<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current, false);
			}
		}
		protected virtual void _(Events.RowUpdated<SOBillingContact> e)
		{
			if (e.Row == null || e.OldRow == null || Base.Document.Current == null) return;

			if (e.ExternalCall && e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted && !BCAPISyncScope.IsScoped()
				&& ((bool?)Base.Document.Cache.GetValue<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current) == true))
			{
				Base.Document.Cache.SetValueExt<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current, false);
			}
		}
		protected virtual void _(Events.RowUpdated<SOShippingAddress> e)
		{
			if (e.Row == null || e.OldRow == null || Base.Document.Current == null) return;

			if (e.ExternalCall && e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted && !BCAPISyncScope.IsScoped()
				&& ((bool?)Base.Document.Cache.GetValue<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current) == true))
			{
				Base.Document.Cache.SetValueExt<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current, false);
			}
		}
		protected virtual void _(Events.RowUpdated<SOShippingContact> e)
		{
			if (e.Row == null || e.OldRow == null || Base.Document.Current == null) return;

			if (e.ExternalCall && e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted && !BCAPISyncScope.IsScoped()
				&& ((bool?)Base.Document.Cache.GetValue<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current) == true))
			{
				Base.Document.Cache.SetValueExt<BCSOOrderExt.externalOrderOriginal>(Base.Document.Current, false);
			}
		}
	}
}