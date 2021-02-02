using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
    [Serializable]
	[PXCacheName(Messages.INItemCategory, PXDacType.Catalogue)]
	public class INItemCategory : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemCategory>.By<inventoryID, categoryID>
		{
			public static INItemCategory Find(PXGraph graph, int? inventoryID, int? categoryID) => FindBy(graph, inventoryID, categoryID);
		}
		public static class FK
		{
			public class Category : IN.INCategory.PK.ForeignKeyOf<INItemCategory>.By<categoryID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemCategory>.By<inventoryID> { }
		}
		#endregion
		#region CategoryID
        public abstract class categoryID : PX.Data.BQL.BqlInt.Field<categoryID> { }
        protected int? _CategoryID;
        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(INCategory.categoryID))]
        [PXParent(typeof(FK.Category))]
        [PXUIField(DisplayName = "Category")]
        public virtual int? CategoryID
        {
            get { return this._CategoryID; }
            set { this._CategoryID = value; }
        }
        #endregion
        
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
        [PXParent(typeof(FK.InventoryItem))]
        [PXSelector(typeof(Search<InventoryItem.inventoryID, Where2<Match<Current<AccessInfo.userName>>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>>>>>), SubstituteKey = typeof(InventoryItem.inventoryCD))]
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

        #region CategorySelected
        public abstract class categorySelected : PX.Data.BQL.BqlBool.Field<categorySelected> { }
        protected bool? _CategorySelected;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Category Selected", Visibility = PXUIVisibility.Service)]
        public virtual bool? CategorySelected
        {
            get
            {
                return this._CategorySelected;
            }
            set
            {
                this._CategorySelected = value;
            }
        }
        #endregion
	}

    [Serializable]
    public class INItemCategoryBuffer : PX.Data.IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [PXInt(IsKey = true)]
        [PXDefault()]
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
