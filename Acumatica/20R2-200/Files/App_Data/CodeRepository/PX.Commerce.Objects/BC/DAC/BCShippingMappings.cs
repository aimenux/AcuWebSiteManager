using PX.Commerce.Core;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	[Serializable]
	[PXCacheName("BCShippingMappings")]
	public class BCShippingMappings : IBqlTable
	{
		#region Keys
	
		public static class FK
		{
			public class Binding : BCBinding.BindingIndex.ForeignKeyOf<BCPaymentMethods>.By<bindingID> { }
			public class ShipZone : BCBinding.BindingIndex.ForeignKeyOf<ShippingZone>.By<zoneID> { }
		}
		#endregion
		#region ShippingMappingID
		[PXDBIdentity(IsKey = true)]
		public int? ShippingMappingID { get; set; }
		public abstract class shippingMappingID : IBqlField { }
		#endregion

		#region BindingID
		[PXDBInt()]
		[PXUIField(DisplayName = "Store")]
		[PXParent(typeof(Select<BCBinding,
			Where<BCBinding.bindingID, Equal<Current<BCShippingMappings.bindingID>>>>))]
		[PXDBDefault(typeof(BCBinding.bindingID))]

		public virtual int? BindingID { get; set; }
		public abstract class bindingID : IBqlField { }
		#endregion

		#region BC Shipping zone
		[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Shipping Zone")]
		public virtual string ShippingZone { get; set; }
		public abstract class shippingZone : IBqlField { }
		#endregion
		#region BC Shipping method
		[PXDBString(50, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Shipping Method")]
		[PXDefault()]
		public virtual string ShippingMethod { get; set; }
		public abstract class shippingMethod : IBqlField { }
		#endregion

		#region Erp Ship Via
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Carrier.carrierID),
					SubstituteKey = typeof(Carrier.carrierID),
					DescriptionField = typeof(Carrier.description))]
		public virtual String CarrierID { get; set; }
		public abstract class carrierID : IBqlField { }
		#endregion
		#region Erp Shipping zone
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipping Zone")]
		[PXSelector(typeof(ShippingZone.zoneID),
					SubstituteKey = typeof(ShippingZone.zoneID),
					DescriptionField = typeof(ShippingZone.description))]
		public virtual String ZoneID { get; set; }
		public abstract class zoneID : IBqlField { }
		#endregion
		#region Erp Shipping terms
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Terms")]
		[PXSelector(typeof(ShipTerms.shipTermsID),
					SubstituteKey = typeof(ShipTerms.shipTermsID),
					DescriptionField = typeof(ShipTerms.description))]
		public virtual String ShipTermsID { get; set; }
		public abstract class shipTermsID : IBqlField { }
		#endregion
	}

	[Serializable]
	[PXCacheName("BCShippingZones")]
	[PXHidden]
	public class BCShippingZones : IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class Binding : BCBinding.BindingIndex.ForeignKeyOf<BCPaymentMethods>.By<bindingID> { }
		}
		#endregion

		#region BindingID
		[PXDBInt(IsKey = true)]
		public virtual int? BindingID { get; set; }
		public abstract class bindingID : IBqlField { }
		#endregion

		#region BC Shipping zone
		[PXDBString(50, IsKey = true, IsUnicode = true, InputMask = "")]
		public virtual string ShippingZone { get; set; }
		public abstract class shippingZone : IBqlField { }
		#endregion

		#region BC Shipping method
		[PXDBString(50, IsKey = true, IsUnicode = true, InputMask = "")]
		public virtual string ShippingMethod { get; set; }
		public abstract class shippingMethod : IBqlField { }
		#endregion

		#region Enabled
		[PXBool]
		public virtual bool? Enabled { get; set; }
		public abstract class enabled : IBqlField { }
		#endregion
	}
}
