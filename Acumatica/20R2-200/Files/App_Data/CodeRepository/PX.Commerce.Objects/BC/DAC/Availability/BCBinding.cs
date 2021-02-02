using System;
using PX.Data;
using PX.Commerce.Core;

namespace PX.Commerce.Objects.Availability
{
	[PXHidden]
	public class BCBinding : IBqlTable
	{
		#region BindingID
		[PXDBIdentity]
		[PXUIField(DisplayName = "Store", Visible = false)]
		public int? BindingID { get; set; }
		public abstract class bindingID : IBqlField { }
		#endregion
		#region IsActive
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; }
		public abstract class isActive : IBqlField { }
		#endregion
		#region ConnectorType
		[PXDBString(3, IsKey = true)]
		[PXUIField(DisplayName = "Connector", Enabled = false)]
		[BCConnectors]
		public virtual string ConnectorType { get; set; }
		public abstract class connectorType : PX.Data.BQL.BqlString.Field<connectorType> { }
		#endregion
	}
}