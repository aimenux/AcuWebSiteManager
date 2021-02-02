using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using System;

namespace PX.Objects.IN.RelatedItems
{
	/// <summary>
	/// Represents Related Items for stock- and non-stock- items.
	/// The records of this type are created and edited through the Realetd Items tab in the Stock Items (IN.20.25.00)
	/// (corresponds to the <see cref="InventoryItemMaint"/> graph) and
	/// the Non-Stock Items (IN.20.20.00) (corresponds to the <see cref="NonStockItemMaint"/> graph) screens.
	/// The cache of this type in the graphs is active only if <see cref="FeaturesSet.commerceIntegration"/> feature is enabled.
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(new Type[] {
					typeof(NonStockItemMaint),
					typeof(InventoryItemMaint)},
				new Type[] {
					typeof(Where<InventoryItem.stkItem, Equal<False>>),
					typeof(Where<InventoryItem.stkItem, Equal<True>>)
					})]
	[PXCacheName(Messages.RelatedItem, 
		PXDacType.Catalogue, 
		CacheGlobal = true)]
	public class INRelatedInventory: IBqlTable
	{
		#region Keys
		public class PK: PrimaryKeyOf<INRelatedInventory>.By<inventoryID, lineID>
		{
			public static INRelatedInventory Find(PXGraph graph, int? inventoryID, int? lineID) 
				=> FindBy(graph, inventoryID, lineID);
		}
		public static class FK
		{
			public class Inventory : InventoryItem.PK.ForeignKeyOf<INRelatedInventory>.By<inventoryID> { }
			public class RelatedInventory: InventoryItem.PK.ForeignKeyOf<INRelatedInventory>.By<relatedInventoryID> { }
		}
		#endregion

		#region InventoryID
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		[PXParent(typeof(FK.Inventory))]
		public int? InventoryID { get; set; }
		public abstract class inventoryID: BqlInt.Field<inventoryID> { }
		#endregion

		#region LineID
		[PXDBIdentity(IsKey = true)]
		public virtual int? LineID { get; set; }
		public abstract class lineID : BqlInt.Field<lineID> { }
		#endregion

		#region Relation
		[PXDBString(5)]
		[PXUIField(DisplayName = "Relation")]
		[PXDefault(InventoryRelation.CrossSell)]
		[InventoryRelation.List]
		public string Relation { get; set; }
		public abstract class relation : BqlString.Field<relation> { }
		#endregion

		#region Rank
		[PXDBInt]
		[PXUIField(DisplayName = "Rank")]
		[PXDefault]
		public int? Rank { get; set; }
		public abstract class rank : BqlInt.Field<rank> { }
		#endregion

		#region Tag
		[PXDBString(4, IsFixed = true)]
		[PXUIField(DisplayName = "Tag")]
		[PXDefault(InventoryRelationTag.Related)]
		[InventoryRelationTag.List]
		public string Tag { get; set; }
		public abstract class tag : BqlString.Field<tag> { }
		#endregion

		#region RelatedInventoryID
		[Inventory]
		[PXRestrictor(typeof(Where<InventoryItem.inventoryID.IsNotEqual<inventoryID.FromCurrent>>), Messages.UsingInventoryAsItsRelated)]
		[PXDefault]
		[PXParent(typeof(FK.RelatedInventory))]
		public int? RelatedInventoryID { get; set; }
		public abstract class relatedInventoryID : BqlInt.Field<relatedInventoryID> { }
		#endregion

		#region Desc
		[PXString]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXFormula(typeof(Selector<relatedInventoryID, InventoryItem.descr>))]
		public string Desc { get; set; }
		public abstract class desc : BqlInt.Field<relatedInventoryID> { }
		#endregion

		#region UOM
		[INUnit(typeof(relatedInventoryID))]
		[PXFormula(typeof(Selector<relatedInventoryID, InventoryItem.baseUnit>))]
		[PXDefault]
		public string UOM { get; set; }
		public abstract class uom : BqlString.Field<uom> { }
		#endregion

		#region EffectiveDate
		[PXDBDate]
		[PXUIField(DisplayName = "Effective Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public DateTime? EffectiveDate { get; set; }
		public abstract class effectiveDate: BqlDateTime.Field<effectiveDate> { }
		#endregion

		#region ExpirationDate
		[PXDBDate]
		[PXUIField(DisplayName = "Expiration Date")]
		public DateTime? ExpirationDate { get; set; }
		public abstract class expirationDate : BqlDateTime.Field<expirationDate> { }
		#endregion

		#region IsActive
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public bool? IsActive { get; set; }
		public abstract class isActive: BqlBool.Field<isActive> { }
		#endregion

		#region NoteID
		[PXNote(PopupTextEnabled = true)]
		public virtual Guid? NoteID { get; set; }
		public abstract class noteID : BqlGuid.Field<noteID> { }
		#endregion

		#region System fields

		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion

		#region tstamp
		[PXDBTimestamp()]
		public virtual byte[] tstamp { get; set; }
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		#endregion

		#endregion
	}
}
