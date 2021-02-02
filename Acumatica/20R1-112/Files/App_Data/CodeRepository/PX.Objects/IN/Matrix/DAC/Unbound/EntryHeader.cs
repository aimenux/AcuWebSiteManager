using System;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.Attributes;

namespace PX.Objects.IN.Matrix.DAC.Unbound
{
	[PXCacheName(Messages.EntityHeaderDAC)]
	public class EntryHeader : IBqlTable
	{
		#region TemplateItemID
		public abstract class templateItemID : Data.BQL.BqlInt.Field<templateItemID> { }

		[PXUIField(DisplayName = "Template Item")]
		[TemplateInventory]
		public virtual int? TemplateItemID
		{
			get;
			set;
		}
		#endregion

		#region Description
		public abstract class description : Data.BQL.BqlString.Field<description> { }

		[PXUIField(DisplayName = "Description", Enabled = false)]
		[PXString]
		public virtual string Description
		{
			get;
			set;
		}
		#endregion

		#region ColAttributeID
		public abstract class colAttributeID : PX.Data.BQL.BqlString.Field<colAttributeID> { }
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Column Attribute ID")]
		[PXDefault(typeof(Search<InventoryItem.defaultColumnMatrixAttributeID, Where<InventoryItem.inventoryID, Equal<Current<templateItemID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<templateItemID>))]
		[PXSelector(typeof(Search2<CSAttributeGroup.attributeID,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<Current<templateItemID>>>>,
			Where<CSAttributeGroup.isActive, Equal<True>,
				And<CSAttributeGroup.entityClassID, Equal<InventoryItem.itemClassID>,
				And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
				And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>>>>>>),
			typeof(CSAttributeGroup.attributeID), DescriptionField = typeof(CSAttributeGroup.description))]
		public virtual string ColAttributeID
		{
			get;
			set;
		}
		#endregion

		#region RowAttributeID
		public abstract class rowAttributeID : PX.Data.BQL.BqlString.Field<rowAttributeID> { }
		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Row Attribute ID")]
		[PXDefault(typeof(Search<InventoryItem.defaultRowMatrixAttributeID, Where<InventoryItem.inventoryID, Equal<Current<templateItemID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<templateItemID>))]
		[PXSelector(typeof(Search2<CSAttributeGroup.attributeID,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<Current<templateItemID>>>>,
			Where<CSAttributeGroup.isActive, Equal<True>,
				And<CSAttributeGroup.entityClassID, Equal<InventoryItem.itemClassID>,
				And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
				And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>>>>>>),
			typeof(CSAttributeGroup.attributeID), DescriptionField = typeof(CSAttributeGroup.description))]
		public virtual string RowAttributeID
		{
			get;
			set;
		}
		#endregion

		#region ShowAvailable
		public abstract class showAvailable : PX.Data.BQL.BqlBool.Field<showAvailable> { }
		[PXBool]
		[PXUIField(DisplayName = "Display Availability Details")]
		public virtual bool? ShowAvailable
		{
			get;
			set;
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[Site(DescriptionField = typeof(INSite.descr))]
		public virtual Int32? SiteID
		{
			get;
			set;
		}
		#endregion

		#region LocationID
		public abstract class locationID : Data.BQL.BqlInt.Field<locationID> { }
		[Location(typeof(siteID), KeepEntry = false, DescriptionField = typeof(INLocation.descr))]
		public virtual int? LocationID
		{
			get;
			set;
		}
		#endregion

		#region DisplayPlanType
		public abstract class displayPlanType : Data.BQL.BqlString.Field<displayPlanType> { }
		[PXDBString(2, IsFixed = true)]
		[PXDefault(PlanType.Available)]
		[PlanType.List]
		[PXUIField(DisplayName = "Plan Type")]
		public virtual string DisplayPlanType
		{
			get;
			set;
		}
		#endregion

		#region SmartPanelType
		public abstract class smartPanelType : Data.BQL.BqlString.Field<smartPanelType>
		{
			public const string Entry = "E";
			public const string Lookup = "L";
		}
		[PXString(1, IsFixed = true)]
		[PXDefault(smartPanelType.Entry, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string SmartPanelType
		{
			get;
			set;
		}
		#endregion
	}
}
