using PX.Data;
using PX.Objects.IN.Matrix.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.TX;

namespace PX.Objects.IN.Matrix.DAC.Unbound
{
	[PXVirtual]
	[PXCacheName(Messages.InventoryItemWithAttributeValuesDAC)]
	[PXBreakInheritance]
	public class MatrixInventoryItem : InventoryItem
	{
		#region InventoryCD
		public new abstract class inventoryCD : Data.BQL.BqlString.Field<inventoryCD> { }
		/// <summary>
		/// The user-friendly unique identifier of the Inventory Item.
		/// The structure of the identifier is determined by the <i>INVENTORY</i> <see cref="CS.Dimension">Segmented Key</see>.
		/// </summary>
		[PXDefault]
		[InventoryRaw(DisplayName = "Inventory ID")]
		[MatrixInventoryItem]
		public override string InventoryCD
		{
			get => base.InventoryCD;
			set => base.InventoryCD = value;
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }
		[PXInt(IsKey = true)]
		public override int? InventoryID
		{
			get => base.InventoryID;
			set => base.InventoryID = value;
		}
		#endregion
		#region NoteID
		public new abstract class noteID : Data.BQL.BqlGuid.Field<noteID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXNote(PopupTextEnabled = true)]
		public override Guid? NoteID
		{
			get; 
			set;
		}
		#endregion

		#region Exists
		public abstract class exists : PX.Data.BQL.BqlBool.Field<exists> { }
		/// <summary>
		/// Indicates that an item with such identifier already exists.
		/// </summary>
		[PXBool]
		public virtual bool? Exists
		{
			get;
			set;
		}
		#endregion
		#region New
		public abstract class @new : PX.Data.BQL.BqlBool.Field<@new> { }
		/// <summary>
		/// Indicates that the item does not currently exist.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "New", Visibility = PXUIVisibility.Visible)]
		public virtual bool? New
		{
			get;
			set;
		}
		#endregion
		#region Duplicate
		public abstract class duplicate : PX.Data.BQL.BqlByteArray.Field<duplicate> { }
		/// <summary>
		/// Indicates that an item with such identifier is already going to be created with another attribute values.
		/// </summary>
		[PXBool]
		public virtual bool? Duplicate
		{
			get;
			set;
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		/// <summary>
		/// The quantity of the item to add to a document.
		/// </summary>
		[PXQuantity]
		[PXUIField(DisplayName = "Quantity", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? Qty
		{
			get;
			set;
		}
		#endregion

		#region AttributeIDs
		public abstract class attributeIDs : PX.Data.BQL.BqlByteArray.Field<attributeIDs> { }
		/// <summary>
		/// Array to store attributes (CSAttributeDetail.AttributeID)
		/// </summary>
		public virtual string[] AttributeIDs
		{
			get;
			set;
		}
		#endregion
		#region AttributeValues
		public abstract class attributeValues : PX.Data.BQL.BqlByteArray.Field<attributeValues> { }
		/// <summary>
		/// Array to store attribute values (CSAttributeDetail.ValueID)
		/// </summary>
		public virtual string[] AttributeValues
		{
			get;
			set;
		}
		#endregion
		#region AttributeValueDescrs
		public abstract class attributeValueDescrs : PX.Data.BQL.BqlByteArray.Field<attributeValueDescrs> { }
		/// <summary>
		/// Array to store attribute values (CSAttributeDetail.Descr)
		/// </summary>
		public virtual string[] AttributeValueDescrs
		{
			get;
			set;
		}
		#endregion
		#region BasePrice
		public abstract new class basePrice : PX.Data.BQL.BqlDecimal.Field<basePrice> { }
		/// <summary>
		/// The price used as the default price, if there are no other prices defined for this item in any price list in the Accounts Receivable module.
		/// </summary>
		[PXDBPriceCost()]
		[PXDefault(typeof(Search<InventoryItem.basePrice, Where<InventoryItem.inventoryID, Equal<Current<EntryHeader.templateItemID>>>>))]
		[PXUIField(DisplayName = "Default Price", Visibility = PXUIVisibility.Visible)]
		public override decimal? BasePrice
		{
			get => base.BasePrice;
			set => base.BasePrice = value;
		}
		#endregion
		#region StkItem
		public abstract new class stkItem : PX.Data.BQL.BqlBool.Field<stkItem> { }
		/// <summary>
		/// When set to <c>true</c>, indicates that this item is a Stock Item.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<InventoryItem.stkItem, Where<InventoryItem.inventoryID, Equal<Current<EntryHeader.templateItemID>>>>))]
		[PXUIField(DisplayName = "Stock Item")]
		public override Boolean? StkItem
		{
			get => base.StkItem;
			set => base.StkItem = value;
		}
		#endregion
		#region ItemClassID
		public abstract new class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		/// <summary>
		/// The identifier of the <see cref="INItemClass">Item Class</see>, to which the Inventory Item belongs.
		/// Item Classes provide default settings for items, which belong to them, and are used to group items.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INItemClass.ItemClassID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.Visible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr),
			CacheGlobal = true)]
		[PXDefault(typeof(Search<InventoryItem.itemClassID, Where<InventoryItem.inventoryID, Equal<Current<EntryHeader.templateItemID>>>>))]
		[PXUIRequired(typeof(INItemClass.stkItem))]
		public override int? ItemClassID
		{
			get => base.ItemClassID;
			set => base.ItemClassID = value;
		}
		#endregion
		#region TaxCategoryID
		public abstract new class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		/// <summary>
		/// Identifier of the <see cref="TaxCategory"/> associated with the item.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INItemClass.TaxCategoryID">Tax Category</see> associated with the <see cref="ItemClassID">Item Class</see>.
		/// Corresponds to the <see cref="TaxCategory.TaxCategoryID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<InventoryItem.taxCategoryID, Where<InventoryItem.inventoryID, Equal<Current<EntryHeader.templateItemID>>>>))]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		public override String TaxCategoryID
		{
			get => base.TaxCategoryID;
			set => base.TaxCategoryID = value;
		}
		#endregion
		#region Descr
		public abstract new class descr : PX.Data.BQL.BqlString.Field<descr> { }
		/// <summary>
		/// The description of the Inventory Item.
		/// </summary>
		[DBMatrixLocalizableDescription(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		[PX.Data.EP.PXFieldDescription]
		public override String Descr
		{
			get => base.Descr;
			set => base.Descr = value;
		}
		#endregion

	}
}
