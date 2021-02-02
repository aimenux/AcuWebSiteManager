using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Commerce.Shopify.API.REST;
using PX.Api;
using PX.Data;
using RestSharp;
using PX.Commerce.Core;
using PX.Objects.CA;
using PX.Commerce.Objects;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Commerce.Shopify
{
	public class BCShopifyStoreMaint : BCStoreMaint
	{
		public PXSelect<BCBindingShopify, Where<BCBindingShopify.bindingID, Equal<Current<BCBinding.bindingID>>>> CurrentBindingShopify;

		public BCShopifyStoreMaint()
		{
			base.Bindings.WhereAnd<Where<BCBinding.connectorType, Equal<SPConnector.spConnectorType>>>();
		}

		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Order, BCEntitiesAttribute.OrderRefunds })]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void BCBindingExt_ReasonCode_CacheAttached(PXCache sender)
		{
		}

		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Order, BCEntitiesAttribute.OrderRefunds })]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void BCBindingExt_RefundAmountItemID_CacheAttached(PXCache sender)
		{
		}

		#region Actions
		public PXAction<BCBinding> TestConnection;
		[PXButton]
		[PXUIField(DisplayName = "Test Connection", Enabled = false)]
		protected virtual IEnumerable testConnection(PXAdapter adapter)
		{
			Actions.PressSave();

			BCBinding binding = Bindings.Current;
			BCBindingShopify bindingShopify = CurrentBindingShopify.Current ?? CurrentBindingShopify.Select();

			if (binding.ConnectorType != SPConnector.TYPE) return adapter.Get();
			if (binding == null || bindingShopify == null || bindingShopify.ShopifyApiBaseUrl == null 
				|| string.IsNullOrEmpty(bindingShopify.ShopifyApiKey) || string.IsNullOrEmpty(bindingShopify.ShopifyApiPassword))
			{
				throw new PXException(BCMessages.TestConnectionFailedParameters);
			}

			PXLongOperation.StartOperation(this, delegate
			{
				BCShopifyStoreMaint graph = PXGraph.CreateInstance<BCShopifyStoreMaint>();
				graph.Bindings.Current = binding;
				graph.CurrentBindingShopify.Current = bindingShopify;

				StoreRestDataProvider restClient = new StoreRestDataProvider(SPConnector.GetRestClient(bindingShopify));
				try
				{
					var store = restClient.Get();
					if (store == null || store.Id == null)
						throw new PXException(ShopifyMessages.TestConnectionStoreNotFound);

					graph.CurrentBindingShopify.Cache.SetValueExt(binding, nameof(BCBindingShopify.ShopifyStoreUrl), store.Domain);
					graph.CurrentBindingShopify.Cache.SetValueExt(binding, nameof(BCBindingShopify.ShopifyDefaultCurrency), store.Currency);
					graph.CurrentBindingShopify.Cache.SetValueExt(binding, nameof(BCBindingShopify.ShopifySupportCurrencies), string.Join(",", store.EnabledPresentmentCurrencies));
					graph.CurrentBindingShopify.Cache.SetValueExt(binding, nameof(BCBindingShopify.ShopifyStoreTimeZone), store.Timezone);
					graph.CurrentBindingShopify.Update(bindingShopify);

					graph.Persist();
				}
				catch (Exception ex)
				{
					throw new PXException(ex, BCMessages.TestConnectionFailedGeneral, ex.Message);
				}
			});

			return adapter.Get();
		}
		#endregion

		#region BCBinding Events
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(BCConnectorsAttribute), "DefaultConnector", SPConnector.TYPE)]
		public virtual void _(Events.CacheAttached<BCBinding.connectorType> e) { }

		public override void _(Events.RowSelected<BCBinding> e)
		{
			base._(e);

			BCBinding row = e.Row as BCBinding;
			if (row == null) return;

			//Actions
			TestConnection.SetEnabled(row.BindingID > 0 && row.ConnectorType == SPConnector.TYPE);
		}
		public override void _(Events.RowInserted<BCBinding> e)
		{
			base._(e);

			bool dirty = CurrentBindingShopify.Cache.IsDirty;
			CurrentBindingShopify.Insert();
			CurrentBindingShopify.Cache.IsDirty = dirty;
		}
		protected virtual void _(Events.RowPersisting<BCBindingShopify> e)
		{
			BCBindingShopify row = e.Row as BCBindingShopify;
			if (row == null || string.IsNullOrEmpty(row.ShopifyApiBaseUrl) || string.IsNullOrWhiteSpace(row.ShopifyApiKey) || string.IsNullOrWhiteSpace(row.ShopifyApiPassword))
				return;

			StoreRestDataProvider restClient = new StoreRestDataProvider(SPConnector.GetRestClient(row.ShopifyApiBaseUrl, row.ShopifyApiKey, row.ShopifyApiPassword, row.StoreSharedSecret, row.ApiCallLimit));
			try
			{
				var store = restClient.Get();

				CurrentBindingShopify.Cache.SetValueExt(row, nameof(row.ShopifyStoreUrl), store.Domain);
				CurrentBindingShopify.Cache.SetValueExt(row, nameof(row.ShopifyDefaultCurrency), store.Currency);
				CurrentBindingShopify.Cache.SetValueExt(row, nameof(row.ShopifySupportCurrencies), string.Join(",", store.EnabledPresentmentCurrencies));
				CurrentBindingShopify.Cache.SetValueExt(row, nameof(row.ShopifyStoreTimeZone), store.Timezone);
				CurrentBindingShopify.Cache.IsDirty = true;
				CurrentBindingShopify.Cache.Update(row);
			}
			catch (Exception ex)
			{
				throw new PXException(ex.Message);
			}
		}

		protected virtual void _(Events.FieldVerifying<BCBindingShopify, BCBindingShopify.shopifyApiBaseUrl> e)
		{
			string val = e.NewValue?.ToString();
			if (val != null)
			{
				if (!val.EndsWith("/")) val += "/";
				if (val.ToLower().EndsWith(".myshopify.com/")) val += "admin/";
				if (!val.ToLower().EndsWith("/admin/"))
				{
					throw new PXSetPropertyException(ShopifyMessages.InvalidStoreUrl, PXErrorLevel.Warning);
				}
				e.NewValue = val;
			}
		}

		public override void _(Events.FieldUpdated<BCEntity, BCEntity.isActive> e)
		{
			base._(e);

			BCEntity row = e.Row;
			if (row == null || row.CreatedDateTime == null) return;

			if (row.IsActive == true)
			{
				if (row.EntityType == BCEntitiesAttribute.ProductWithVariant)
					if (PXAccess.FeatureInstalled<FeaturesSet.matrixItem>() == false)
					{
						EntityReturn(row.EntityType).IsActive = false;
						e.Cache.Update(EntityReturn(row.EntityType));
						throw new PXSetPropertyException(BCMessages.MatrixFeatureRequired);
					}
			}
		}
		#endregion
	}
}