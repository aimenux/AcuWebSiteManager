using System;
using PX.Commerce.Core;
using PX.Data;
using PX.Objects.IN;

namespace PX.Commerce.Objects
{
	[Serializable]
	[PXCacheName("BigCommerce Inventory Item")]
	public class BCInventoryItem : PXCacheExtension<InventoryItem>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		#region Visibility
		[PXDBString(1, IsUnicode = true)]
        [PXUIField(DisplayName = "Visibility")]
		[BCItemVisibility]
		[PXDefault(BCItemVisibilityAttribute.Visible)]
        public virtual string Visibility { get; set; }
        public abstract class visibility : IBqlField { }
		#endregion
		#region Availability
		[PXDBString(1, IsUnicode = true)]
		[PXUIField(DisplayName = "Availability")]
		[BCItemAvailabilities.ListDef]
		[PXDefault(BCItemAvailabilities.StoreDefault)]
		public virtual string Availability { get; set; }
		public abstract class availability : IBqlField { }
		#endregion
		#region NotAvailMode
		[PXDBString(1, IsUnicode = true)]
		[PXUIField(DisplayName = "When Qty Unavailable")]
		[BCItemNotAvailModes.ListDef]
		[PXDefault(BCItemNotAvailModes.StoreDefault)]
		[PXUIEnabled(typeof(Where<BCInventoryItem.availability, Equal<BCItemAvailabilities.availableTrack>>))]
		[PXFormula(typeof(Default<BCInventoryItem.availability>))]
		public virtual string NotAvailMode { get; set; }
		public abstract class notAvailMode : IBqlField { }
		#endregion
	
        #region CustomURL
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Custom URL")]
        public virtual string CustomURL { get; set; }
        public abstract class customURL : IBqlField { }
        #endregion
        #region PageTitle
		[PXDBLocalizableString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Page Title")]
        public virtual string PageTitle { get; set; }
        public abstract class pageTitle : IBqlField { }
		#endregion
		#region MetaDescription
		[PXDBLocalizableString(1024, IsUnicode = true)]
		[PXUIField(DisplayName = "Meta Description")]
        public virtual string MetaDescription { get; set; }
        public abstract class metaDescription : IBqlField { }
		#endregion
		#region MetaKeywords
		[PXDBLocalizableString(1024, IsUnicode = true)]
		[PXUIField(DisplayName = "Meta Keywords")]
		public virtual string MetaKeywords { get; set; }
        public abstract class metaKeywords : IBqlField { }
		#endregion
		#region SearchKeywords
		[PXDBLocalizableString(1024, IsUnicode = true)]
		[PXUIField(DisplayName = "Search Keywords")]
		public virtual string SearchKeywords { get; set; }
        public abstract class searchKeywords : IBqlField { }
		#endregion
		#region ShortDescription
		[PXDBLocalizableString(1024, IsUnicode = true)]
		[PXUIField(DisplayName = "Short Description", Visible = false)]
		public virtual string ShortDescription { get; set; }
		public abstract class shortDescription : IBqlField { }
		#endregion
	}
}