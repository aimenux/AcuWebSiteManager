using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using System.Collections.Generic;
using PX.Commerce.Core;
using PX.Commerce.Objects;
using PX.Data.ReferentialIntegrity.Attributes;
using static PX.Commerce.Shopify.SPConnector;

namespace PX.Commerce.Shopify
{
	[Serializable]
	[PXCacheName("Shopify Settings")]
	public class BCBindingShopify : IBqlTable
	{
		public class PK : PrimaryKeyOf<BCBindingShopify>.By<BCBindingShopify.bindingID>
		{
			public static BCBindingShopify Find(PXGraph graph, int? binding) => FindBy(graph, binding);
		}

		#region BindingID
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BCBinding.bindingID))]
		[PXUIField(DisplayName = "Store", Visible = false)]
		[PXParent(typeof(Select<BCBinding, Where<BCBinding.bindingID, Equal<Current<BCBindingShopify.bindingID>>>>))]
		public int? BindingID { get; set; }
		public abstract class bindingID : IBqlField { }
		#endregion

		//Connection
		#region StoreBaseUrl
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Admin URL")]
		[PXDefault()]
		public virtual string ShopifyApiBaseUrl { get; set; }
		public abstract class shopifyApiBaseUrl : IBqlField { }
		#endregion
		#region ShopifyApiKey
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "API Key")]
		[PXDefault()]
		public virtual string ShopifyApiKey { get; set; }
		public abstract class shopifyApiKey : IBqlField { }
		#endregion
		#region ShopifyApiPassword
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "API Password")]
		[PXDefault()]
		public virtual string ShopifyApiPassword { get; set; }
		public abstract class shopifyApiPassword : IBqlField { }
		#endregion
		#region StoreSharedSecret
		//[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Shared Secret")]
		[PXDefault()]
		public virtual string StoreSharedSecret { get; set; }
		public abstract class storeSharedSecret : IBqlField { }
		#endregion
		#region ShopifyPlus
		[PXDBString(2)]
		[PXUIField(DisplayName = "Store Plan")]
		[BCShopifyStorePlanAttribute]
		[PXDefault(BCShopifyStorePlanAttribute.NormalPlan, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string ShopifyStorePlan { get; set; }
		public abstract class shopifyStorePlan : IBqlField { }
		#endregion
		#region CombineCategoriesToTags
		[PXDBString(2)]
		[PXUIField(DisplayName = "Sales Category Export")]
		[BCSalesCategoriesExport]
		[PXDefault(BCSalesCategoriesExportAttribute.ExportAsTags, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string CombineCategoriesToTags { get; set; }
		public abstract class combineCategoriesToTags : IBqlField { }
		#endregion
		#region ShopifyDefaultCurrency
		[PXDBString(12, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Currency", IsReadOnly = true)]
		public virtual string ShopifyDefaultCurrency { get; set; }
		public abstract class shopifyDefaultCurrency : IBqlField { }
		#endregion
		#region ShopifySupportCurrencies 
		[PXDBString(200, IsUnicode = true)]
		[PXUIField(DisplayName = "Supported Currencies", IsReadOnly = true)]
		public virtual string ShopifySupportCurrencies { get; set; }
		public abstract class shopifySupportCurrencies : IBqlField { }
		#endregion
		#region ShopifyStoreUrl
		[PXDBString(200, IsUnicode = true)]
		[PXUIField(DisplayName = "Store URL", IsReadOnly = true)]
		public virtual string ShopifyStoreUrl { get; set; }
		public abstract class shopifyStoreUrl : IBqlField { }
		#endregion
		#region ShopifyStoreTimeZone 
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Store Time Zone", IsReadOnly = true)]
		public virtual string ShopifyStoreTimeZone { get; set; }
		public abstract class shopifyStoreTimeZone : IBqlField { }
        #endregion
        #region ApiDelaySeconds
        [PXDBInt(MinValue = 0)]
        [PXDefault(180, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? ApiDelaySeconds { get; set; }
        public abstract class apiDelaySeconds : IBqlField { }
        #endregion

        #region ApiCallLimit
        [PXInt()]
		public virtual int? ApiCallLimit
		{
			get
			{
				return this.ShopifyStorePlan == BCShopifyStorePlanAttribute.PlusPlan ? ShopifyCaptions.ApiCallLimitPlus : ShopifyCaptions.ApiCallLimitDefault;
			}
		}
		public abstract class apiCallLimit : IBqlField { }
		#endregion
	}

	[PXPrimaryGraph(new Type[] { typeof(BCShopifyStoreMaint) },
					new Type[] { typeof(Where<BCBinding.connectorType, Equal<spConnectorType>>),})]
	public class BCBindingShopifyExtension : PXCacheExtension<BCBinding>
	{

	}
}