namespace PX.Objects.PM
{
	using System;
	using PX.Data;
	using PX.Objects.IN.S;
	using PX.Objects.IN;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.PMItemRate)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMItemRate : PX.Data.IBqlTable
	{
		#region RateDefinitionID
		public abstract class rateDefinitionID : PX.Data.BQL.BqlInt.Field<rateDefinitionID> { }
		protected int? _RateDefinitionID;
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PMRateSequence.rateDefinitionID))]
		[PXParent(typeof(Select<PMRateSequence, Where<PMRateSequence.rateDefinitionID, Equal<Current<PMItemRate.rateDefinitionID>>, And<PMRateSequence.rateCodeID, Equal<Current<PMItemRate.rateCodeID>>>>>))]
		public virtual int? RateDefinitionID
		{
			get
			{
				return this._RateDefinitionID;
			}
			set
			{
				this._RateDefinitionID = value;
			}
		}
		#endregion
		#region RateCodeID
		public abstract class rateCodeID : PX.Data.BQL.BqlString.Field<rateCodeID> { }
		protected String _RateCodeID;

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(PMRateSequence.rateCodeID))]
		public virtual String RateCodeID
		{
			get
			{
				return this._RateCodeID;
			}
			set
			{
				this._RateCodeID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDefault()]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
		[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>), IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMItemRate.inventoryID>>>>))]
		[PMInventorySelector(Filterable = true)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
	}
}
