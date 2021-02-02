using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using System.Collections.Generic;
using PX.Commerce.BigCommerce;
using PX.Commerce.Core;
using PX.Data.ReferentialIntegrity.Attributes;
using static PX.Commerce.BigCommerce.BCConnector;
using PX.Commerce.Objects;
namespace PX.Commerce.BigCommerce
{
	[Serializable]
	[PXCacheName("BigCommerce Settings")]
	public class BCBindingBigCommerce : IBqlTable
	{
		public class PK : PrimaryKeyOf<BCBindingBigCommerce>.By<BCBindingBigCommerce.bindingID>
		{
			public static BCBindingBigCommerce Find(PXGraph graph, int? binding) => FindBy(graph, binding);
		}

		#region BindingID
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BCBinding.bindingID))]
		[PXUIField(DisplayName = "Store", Visible = false)]
		[PXParent(typeof(Select<BCBinding, Where<BCBinding.bindingID, Equal<Current<BCBindingBigCommerce.bindingID>>>>))]
		public int? BindingID { get; set; }
		public abstract class bindingID : IBqlField { }
		#endregion

		//Connection
		#region StoreBaseUrl
		[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "API Path")]
		[PXDefault()]
		public virtual string StoreBaseUrl { get; set; }
		public abstract class storeBaseUrl : IBqlField { }
		#endregion
		#region StoreXAuthClient
		//[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Client ID")]
		[PXDefault()]
		public virtual string StoreXAuthClient { get; set; }
		public abstract class storeXAuthClient : IBqlField { }
		#endregion
		#region StoreXAuthToken
		//[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Access Token")]
		[PXDefault()]
		public virtual string StoreXAuthToken { get; set; }
		public abstract class storeXAuthToken : IBqlField { }
		#endregion

		#region BigCommerceDefaultCurrency
		[PXDBString(12, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Currency", IsReadOnly = true)]
		public virtual string BigCommerceDefaultCurrency { get; set; }
		public abstract class bigCommerceDefaultCurrency : IBqlField { }
		#endregion
		#region BigCommerceStoreTimeZone 
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Store Time Zone", IsReadOnly = true)]
		public virtual string BigCommerceStoreTimeZone { get; set; }
		public abstract class bigCommerceStoreTimeZone : IBqlField { }
		#endregion

		#region StoreWDAVServerUrl
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "WebDAV Path")]
		[PXDefault()]
		public virtual string StoreWDAVServerUrl { get; set; }
		public abstract class storeWDAVServerUrl : IBqlField { }
		#endregion
		#region StoreWDAVClientUser
		[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "WebDAV Username")]
		[PXDefault()]
		public virtual string StoreWDAVClientUser { get; set; }
		public abstract class storeWDAVClientUser : IBqlField { }
		#endregion
		#region StoreWDAVClientPass
		//[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXRSACryptString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "WebDAV Password")]
		[PXDefault()]
		public virtual string StoreWDAVClientPass { get; set; }
		public abstract class storeWDAVClientPass : IBqlField { }
		#endregion
		#region StoreAdminURL
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Admin Path")]
		[PXDefault()]
		public virtual string StoreAdminUrl { get; set; }
		public abstract class storeAdminUrl : IBqlField { }
		#endregion
	}

	[PXPrimaryGraph(new Type[] { typeof(BCBigCommerceStoreMaint) },
					new Type[] { typeof(Where<BCBinding.connectorType, Equal<bcConnectorType>>)})]

	public class BCBindingBigCommerceExtension : PXCacheExtension<BCBindingExt>
	{
		[PXRemoveBaseAttribute(typeof(BCSettingsCheckerAttribute))]
		public virtual string ReasonCode { get; set; }

		[PXRemoveBaseAttribute(typeof(BCSettingsCheckerAttribute))]
		public virtual Int32? RefundAmountItemID { get; set; }
	}
}