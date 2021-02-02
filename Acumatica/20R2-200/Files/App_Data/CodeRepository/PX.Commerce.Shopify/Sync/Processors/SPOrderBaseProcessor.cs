using PX.Api.ContractBased.Models;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify
{
	public abstract class SPOrderBaseProcessor<TGraph, TEntityBucket, TPrimaryMapped> : BCProcessorSingleBase<TGraph, TEntityBucket, TPrimaryMapped>
		where TGraph : PXGraph
		where TEntityBucket : class, IEntityBucket, new()
		where TPrimaryMapped : class, IMappedEntity, new()
	{
		protected BCBinding currentBinding;
		protected BCBindingExt currentBindingExt;
		protected InventoryItem refundItem;

		public override void Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			base.Initialise(iconnector, operation);
			currentBinding = GetBinding();
			currentBindingExt = GetBindingExt<BCBindingExt>();
			refundItem = currentBindingExt.RefundAmountItemID != null ? PX.Objects.IN.InventoryItem.PK.Find((PXGraph)this, currentBindingExt.RefundAmountItemID) : throw new PXException(ShopifyMessages.NoRefundItem);
		}

		protected virtual String GetInventoryCDByExternID(String productID, String variantID, String sku, String description, out string uom)
		{
			PX.Objects.IN.InventoryItem item = null;
			if (!string.IsNullOrWhiteSpace(sku))
			{
				item = SelectFrom<PX.Objects.IN.InventoryItem>.
					LeftJoin<BCSyncDetail>.On<PX.Objects.IN.InventoryItem.noteID.IsEqual<BCSyncDetail.localID>>.
					InnerJoin<BCSyncStatus>.On<BCSyncDetail.syncID.IsEqual<BCSyncStatus.syncID>>.
					Where<BCSyncStatus.connectorType.IsEqual<BCEntity.connectorType.FromCurrent>.
					And<BCSyncStatus.bindingID.IsEqual<BCEntity.bindingID.FromCurrent>.
					And<Brackets<BCSyncStatus.entityType.IsEqual<@P.AsString>.Or<BCSyncStatus.entityType.IsEqual<@P.AsString>.Or<BCSyncStatus.entityType.IsEqual<@P.AsString>>>>.
					And<BCSyncDetail.externID.IsEqual<@P.AsString>.
					And<BCSyncStatus.externID.IsEqual<@P.AsString>.
					And<InventoryItem.inventoryCD.IsEqual<@P.AsString>>>>>>>.View.
					Select(this, BCEntitiesAttribute.StockItem, BCEntitiesAttribute.NonStockItem, BCEntitiesAttribute.ProductWithVariant, variantID, productID, sku);
			}
			if (item == null)
				item = PXSelect<PX.Objects.IN.InventoryItem,
								Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>
								.Select(this, string.IsNullOrEmpty(sku) ? description : sku);
			if (item == null)
				throw new PXException(BCMessages.InvenotryNotFound, string.IsNullOrEmpty(sku) ? description : sku, new Object[] { productID, variantID }.KeyCombine());
			if (item.ItemStatus == PX.Objects.IN.INItemStatus.Inactive)
				throw new PXException(BCMessages.InvenotryInactive, item.InventoryCD);

			uom = item?.BaseUnit?.Trim();
			return item?.InventoryCD?.Trim();
		}


		public virtual string TrimAutomaticTaxNameForAvalara(string mappedTaxName)
		{
			return mappedTaxName.Split(new string[] { " - " }, StringSplitOptions.None).FirstOrDefault() ?? mappedTaxName;
		}
		public virtual SalesOrderDetail InsertRefundAmountItem(decimal amount, StringValue branch)
		{
			decimal quantity = 1;

			SalesOrderDetail detail = new SalesOrderDetail();
			detail.InventoryID = refundItem.InventoryCD?.TrimEnd().ValueField();
			detail.OrderQty = quantity.ValueField();
			detail.UOM = refundItem.BaseUnit.ValueField();
			detail.Branch = branch;
			detail.UnitPrice = (amount).ValueField();
			detail.ManualPrice = true.ValueField();
			detail.ReasonCode = currentBindingExt.ReasonCode?.ValueField();
			return detail;

		}
		public virtual void ValidateTaxes(int? syncID, SalesOrder impl , SalesOrder local)
		{
			if (impl != null && (currentBindingExt.SyncTaxes != BCTaxSyncAttribute.NoSync) && (local.IsTaxValid?.Value == true))
			{
				String receivedTaxes = String.Join("; ", impl.TaxDetails?.Select(x => String.Join("=", x.TaxID?.Value, x.TaxAmount?.Value)).ToArray() ?? new String[] { BCConstants.None });
				this.LogInfo(Operation.LogScope(syncID), BCMessages.LogTaxesOnOrderReceived,
					impl.OrderNbr?.Value,
					impl.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None,
					String.IsNullOrEmpty(receivedTaxes) ? BCConstants.None : receivedTaxes);

				List<TaxDetail> sentTaxesToValidate = local?.TaxDetails.ToList() ?? new List<TaxDetail>();
				List<TaxDetail> receivedTaxesToValidate = impl.TaxDetails.ToList() ?? new List<TaxDetail>();
				//Validate Tax Zone
				if (sentTaxesToValidate.Count > 0 && impl.FinancialSettings.CustomerTaxZone.Value == null)
				{
					throw new PXException(BCObjectsMessages.CannotFindTaxZone,
						String.Join(", ", sentTaxesToValidate.Select(x => x.TaxID?.Value).Where(x => x != null).ToArray() ?? new String[] { BCConstants.None }));
				}
				//Validate tax codes and amounts
				List<TaxDetail> invalidSentTaxes = new List<TaxDetail>();
				foreach (TaxDetail sent in sentTaxesToValidate)
				{
					TaxDetail received = receivedTaxesToValidate.FirstOrDefault(x => String.Equals(x.TaxID?.Value, sent.TaxID?.Value, StringComparison.InvariantCultureIgnoreCase));
					// This is the line to filter out the incoming taxes that has 0 value, thus if settings in AC are correct they wont be created as lines on SO
					if ((received == null && sent.TaxAmount.Value != 0)
						|| (received != null && !Equal(sent.TaxAmount?.Value , received.TaxAmount?.Value)))
					{
						invalidSentTaxes.Add(sent);
					}

					if (received != null) receivedTaxesToValidate.Remove(received);
				}
				if (invalidSentTaxes.Count > 0)
				{
					throw new PXException(BCObjectsMessages.CannotFindMatchingTaxExt,
						String.Join(",", invalidSentTaxes.Select(x => x.TaxID?.Value)),
						impl.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None);
				}
				List<TaxDetail> invalidReceivedTaxes = receivedTaxesToValidate.Where(x => (x.TaxAmount?.Value ?? 0m) == 0m && (x.TaxableAmount?.Value ?? 0m) == 0m).ToList();
				if (invalidReceivedTaxes.Count > 0)
				{
					throw new PXException(BCObjectsMessages.CannotFindMatchingTaxAcu,
						String.Join(",", invalidReceivedTaxes.Select(x => x.TaxID?.Value)),
						impl.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None);
				}
			}
		}

		public virtual bool Equal(decimal? sent, decimal? received)
		{
			if (sent.HasValue && received.HasValue)
			{
				int countSent = BitConverter.GetBytes(decimal.GetBits(sent.Value)[3])[2];
				int countReceived = BitConverter.GetBytes(decimal.GetBits(received.Value)[3])[2];
				int precision = countSent < countReceived ? countSent : countReceived;

				return PX.Objects.CM.PXCurrencyAttribute.PXCurrencyHelper.Round(sent.Value, precision) == PX.Objects.CM.PXCurrencyAttribute.PXCurrencyHelper.Round(received.Value, precision);
			}
			return false;
		}


		public virtual void LogTaxDetails(int? syncID,SalesOrder order)
		{
			//Logging for taxes
			if ((currentBindingExt.SyncTaxes != BCTaxSyncAttribute.NoSync) && (order.IsTaxValid?.Value == true))
			{
				String sentTaxes = String.Join("; ", order.TaxDetails?.Select(x => String.Join("=", x.TaxID?.Value, x.TaxAmount?.Value)).ToArray() ?? new String[] { BCConstants.None });
				this.LogInfo(Operation.LogScope(syncID), BCMessages.LogTaxesOnOrderSent,
					order.OrderNbr?.Value ?? BCConstants.None,
					order.FinancialSettings?.CustomerTaxZone?.Value ?? BCConstants.None,
					String.IsNullOrEmpty(sentTaxes) ? BCConstants.None : sentTaxes);
			}
		}

	}
}
