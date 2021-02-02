using System;
using PX.Data;
using PX.Commerce.Core;

namespace PX.Commerce.Objects.Availability
{
	[PXHidden]
	public class BCBindingExt : IBqlTable
	{
		#region BindingID
		[PXDBIdentity]
		[PXUIField(DisplayName = "Store", Visible = false)]
		public int? BindingID { get; set; }
		public abstract class bindingID : IBqlField { }
		#endregion
		#region Availability
		[PXDBString(1, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Availability")]
		[BCItemAvailabilities.List]
		[PXDefault(BCItemAvailabilities.AvailableSkip)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductAvailability })]
		public virtual string Availability { get; set; }
		public abstract class availability : IBqlField { }
		#endregion
        #region WarehouseMode
        [PXDBString(1)]
        [PXUIField(DisplayName = "Warehouse Mode")]
        [PXDefault(BCWarehouseModeAttribute.AllWarehouse, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [BCWarehouseMode]
        public virtual string WarehouseMode { get; set; }
        public abstract class warehouseMode : IBqlField { }
        #endregion
	}
}